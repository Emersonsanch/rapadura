using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Audio
{
    /// <summary>
    /// Music and weather of the area the hero walks in the continuous world (added by <see cref="MapModeController"/> next to
    /// the <see cref="OverworldView"/>; consumes <see cref="AreaChangedEvent"/>):
    /// <list type="bullet">
    /// <item>music: <see cref="OverworldAreaAmbience.MoodOf"/> (town / field / wild / danger) and day or night →
    /// <see cref="AreaMusicRules"/> → <see cref="MusicDirector.RequestAmbient"/> (crossfade of the existing tracks);</item>
    /// <item>weather: an area with its own climate (<see cref="OverworldAreaAmbience.ClimateOf(OverworldZone, OverworldSettlement)"/>:
    /// fog, dust, snow, the caves' damp mist, clear) blends the region's <see cref="WeatherSystem"/> into it (its usual
    /// transition, so <see cref="AmbientVfx"/> and the weather visuals follow); walking back into an area without one lets
    /// the biome roll again.</item>
    /// </list>
    /// Re-checked every <see cref="RefreshSeconds"/> (hero level ups, day / night, a biome roll over a held weather). On
    /// destroy (leaving map mode) the world theme comes back, unless something else (an encounter) took the music meanwhile.
    /// </summary>
    public sealed class AreaAmbienceDirector : MonoBehaviour
    {
        public const float RefreshSeconds = 1f;

        private OverworldZone _zone;
        private OverworldSettlement _settlement;
        private OverworldPart _part;
        private WeatherType? _heldWeather;
        private bool _hasArea;
        private float _nextRefresh;
        private MusicContext _requestedContext = MusicContext.None;
        private string _requestedVariant = string.Empty;

        /// <summary>The weather the current area holds (null: the biome rolls).</summary>
        public WeatherType? HeldWeather => _heldWeather;

        public MusicContext RequestedContext => _requestedContext;
        public string RequestedVariant => _requestedVariant;

        private void OnEnable()
        {
            EventBus.Subscribe<AreaChangedEvent>(OnAreaChanged);
            EventBus.Subscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<AreaChangedEvent>(OnAreaChanged);
            EventBus.Unsubscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out MusicDirector music) && music != null && _requestedContext != MusicContext.None
                && music.BaseContext == _requestedContext && music.BaseVariant == _requestedVariant)
            {
                music.RequestAmbient(MusicContext.WorldMap);
            }

            if (_heldWeather.HasValue && ServiceLocator.TryGet(out WeatherSystem weather) && weather != null)
            {
                weather.RollNext();
            }
        }

        private void OnAreaChanged(AreaChangedEvent evt)
        {
            _zone = evt.Current;
            _settlement = evt.CurrentSettlement;
            _part = evt.CurrentPart;
            _hasArea = _zone != null;
            Apply();
        }

        private void OnPhaseChanged(TimeOfDayPhaseChangedEvent evt) => Apply();

        private void Update()
        {
            if (!_hasArea || Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            Apply();
        }

        private void Apply()
        {
            _nextRefresh = Time.unscaledTime + RefreshSeconds;
            if (!_hasArea)
            {
                return;
            }

            ApplyWeather(OverworldAreaAmbience.ClimateOf(_zone, _settlement).Weather);
            ApplyMusic();
        }

        private void ApplyWeather(WeatherType? wanted)
        {
            if (!ServiceLocator.TryGet(out WeatherSystem weather) || weather == null)
            {
                return;
            }

            if (wanted.HasValue)
            {
                // Held: blended in (the weather's own transition), and taken back if the biome rolled over it.
                if (weather.CurrentWeather != wanted.Value)
                {
                    weather.ForceWeather(wanted.Value);
                }
            }
            else if (_heldWeather.HasValue)
            {
                weather.RollNext();
            }

            _heldWeather = wanted;
        }

        private void ApplyMusic()
        {
            if (!ServiceLocator.TryGet(out MusicDirector music) || music == null)
            {
                return;
            }

            MapModeController mode = MapModeController.Current;
            int level = mode != null ? mode.HeroLevel : 1;
            bool night = ServiceLocator.TryGet(out TimeOfDayManager time) && time != null && time.Phase == TimeOfDayPhase.Night;
            BiomeType biome = _part != null ? _part.Biome : mode != null && mode.Overworld != null ? mode.Overworld.World.Biome : BiomeType.Forest;
            AreaMood mood = OverworldAreaAmbience.MoodOf(_zone, _settlement, level);
            AreaMusicRules.Pick(mood, night, biome, out MusicContext context, out string variant);

            // An encounter / boss took the music (the flow): leave it alone until it hands back the world theme.
            MusicContext current = music.BaseContext;
            bool ours = current == MusicContext.WorldMap || current == MusicContext.None || (current == _requestedContext && music.BaseVariant == _requestedVariant);
            if (!ours)
            {
                return;
            }

            _requestedContext = context;
            _requestedVariant = SfxEventMap.Normalize(variant);
            music.RequestAmbient(context, variant);
        }
    }
}
