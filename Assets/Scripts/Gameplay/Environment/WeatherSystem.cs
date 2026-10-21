using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>JSON-serializable snapshot of <see cref="WeatherSystem"/>.</summary>
    [Serializable]
    public class WeatherSaveState
    {
        public int biome;
        public int weather;
        public float secondsUntilNextRoll;
    }

    /// <summary>
    /// Simple per-biome weather state machine. Plain C# <see cref="IManager"/> + <see cref="ISaveable"/>
    /// built by GameManager next to <see cref="TimeOfDayManager"/>, ticked by
    /// <see cref="EnvironmentTickRunner"/> in Play Mode (tests call <see cref="Tick"/> directly).
    ///
    /// Every <see cref="MinDurationSeconds"/>..<see cref="MaxDurationSeconds"/> real seconds (and
    /// whenever the world-map region changes, via <see cref="WorldMapRegionChangedEvent"/>) it rolls
    /// the next state from <see cref="WeatherTable"/> for the current biome and blends towards it over
    /// <see cref="TransitionSeconds"/>; <see cref="PreviousWeather"/>/<see cref="CurrentWeather"/>/
    /// <see cref="TransitionProgress"/> are what visuals (WeatherVisuals, DayNightLighting) lerp
    /// between. All randomness comes from one injected seed, so a given seed + tick sequence
    /// reproduces the same weather. Pauses with GamePausedEvent like the clock.
    /// </summary>
    public class WeatherSystem : IManager, ISaveable
    {
        public const float DefaultMinDurationSeconds = 180f;
        public const float DefaultMaxDurationSeconds = 420f;
        public const float DefaultTransitionSeconds = 12f;

        private readonly System.Random _random;
        private readonly Action<float> _tickAction;
        private bool _isInitialized;

        public string SaveKey => "Weather";

        public float MinDurationSeconds { get; set; } = DefaultMinDurationSeconds;
        public float MaxDurationSeconds { get; set; } = DefaultMaxDurationSeconds;
        public float TransitionSeconds { get; set; } = DefaultTransitionSeconds;

        public BiomeType CurrentBiome { get; private set; }
        public WeatherType CurrentWeather { get; private set; } = WeatherType.Clear;
        public WeatherType PreviousWeather { get; private set; } = WeatherType.Clear;

        /// <summary>0 = fully <see cref="PreviousWeather"/>, 1 = fully <see cref="CurrentWeather"/>.</summary>
        public float TransitionProgress { get; private set; } = 1f;

        public float SecondsUntilNextRoll { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsTransitioning => TransitionProgress < 1f;

        /// <summary>Blended sun dimming factor (1 = clear sky) for DayNightLighting.</summary>
        public float SunIntensityMultiplier => Mathf.Lerp(
            WeatherTable.GetSunMultiplier(PreviousWeather),
            WeatherTable.GetSunMultiplier(CurrentWeather),
            TransitionProgress);

        /// <param name="seed">Fixed seed for reproducible weather (tests); null = time-based seed.</param>
        public WeatherSystem(int? seed = null, BiomeType startingBiome = BiomeType.Forest)
        {
            _random = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
            _tickAction = Tick;
            CurrentBiome = startingBiome;
            SecondsUntilNextRoll = NextDuration();
        }

        public void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            if (ServiceLocator.TryGet(out WorldMapProgress progress) && progress != null)
            {
                CurrentBiome = progress.CurrentRegion;
            }

            EventBus.Subscribe<WorldMapRegionChangedEvent>(OnRegionChanged);
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnvironmentTickRunner.Register(_tickAction);
        }

        public void Shutdown()
        {
            if (!_isInitialized)
            {
                return;
            }

            _isInitialized = false;
            EventBus.Unsubscribe<WorldMapRegionChangedEvent>(OnRegionChanged);
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EnvironmentTickRunner.Unregister(_tickAction);
        }

        public void SetPaused(bool paused) => IsPaused = paused;

        public void Tick(float deltaTime)
        {
            if (IsPaused || deltaTime <= 0f)
            {
                return;
            }

            if (TransitionProgress < 1f)
            {
                TransitionProgress = TransitionSeconds <= 0f
                    ? 1f
                    : Mathf.Min(1f, TransitionProgress + deltaTime / TransitionSeconds);
            }

            SecondsUntilNextRoll -= deltaTime;
            if (SecondsUntilNextRoll <= 0f)
            {
                RollNext();
            }
        }

        /// <summary>Changes biome and immediately re-rolls weather for it (blended). No-op if unchanged.</summary>
        public void SetBiome(BiomeType biome)
        {
            if (biome == CurrentBiome)
            {
                return;
            }

            CurrentBiome = biome;
            RollNext();
        }

        /// <summary>Rolls the next weather for <see cref="CurrentBiome"/> and restarts the duration timer.</summary>
        public WeatherType RollNext()
        {
            BiomeRegistry.TryGet(CurrentBiome, out BiomeDefinition definition);
            WeatherType next = WeatherTable.Roll(definition, CurrentBiome, _random);
            SecondsUntilNextRoll = NextDuration();
            BeginTransition(next, instant: false);
            return next;
        }

        /// <summary>Forces a weather state (debug, scripted scenes). Instant skips the blend.</summary>
        public void ForceWeather(WeatherType weather, bool instant = false)
        {
            SecondsUntilNextRoll = NextDuration();
            BeginTransition(weather, instant);
        }

        /// <summary>How strongly <paramref name="weather"/> is currently present (0..1), accounting for the blend.</summary>
        public float GetWeight(WeatherType weather)
        {
            float weight = 0f;
            if (weather == CurrentWeather)
            {
                weight += TransitionProgress;
            }

            if (weather == PreviousWeather)
            {
                weight += 1f - TransitionProgress;
            }

            return weight;
        }

        public object CaptureState()
        {
            return new WeatherSaveState
            {
                biome = (int)CurrentBiome,
                weather = (int)CurrentWeather,
                secondsUntilNextRoll = SecondsUntilNextRoll,
            };
        }

        public void RestoreState(object state)
        {
            if (!(state is WeatherSaveState saved))
            {
                return;
            }

            if (Enum.IsDefined(typeof(BiomeType), saved.biome))
            {
                CurrentBiome = (BiomeType)saved.biome;
            }

            WeatherType weather = Enum.IsDefined(typeof(WeatherType), saved.weather)
                ? (WeatherType)saved.weather
                : WeatherType.Clear;

            SecondsUntilNextRoll = saved.secondsUntilNextRoll > 0f ? saved.secondsUntilNextRoll : NextDuration();
            BeginTransition(weather, instant: true);
        }

        private void BeginTransition(WeatherType next, bool instant)
        {
            WeatherType previous = CurrentWeather;

            if (instant)
            {
                PreviousWeather = next;
                CurrentWeather = next;
                TransitionProgress = 1f;
                EventBus.Publish(new WeatherChangedEvent(previous, next, CurrentBiome, 0f));
                return;
            }

            if (next == CurrentWeather)
            {
                return;
            }

            // Mid-blend retarget: start from whichever state currently dominates so visuals don't pop.
            PreviousWeather = TransitionProgress >= 0.5f ? CurrentWeather : PreviousWeather;
            CurrentWeather = next;
            TransitionProgress = TransitionSeconds <= 0f ? 1f : 0f;
            EventBus.Publish(new WeatherChangedEvent(previous, next, CurrentBiome, Mathf.Max(0f, TransitionSeconds)));
        }

        private float NextDuration()
        {
            float min = Mathf.Max(1f, MinDurationSeconds);
            float max = Mathf.Max(min, MaxDurationSeconds);
            return min + (float)_random.NextDouble() * (max - min);
        }

        private void OnRegionChanged(WorldMapRegionChangedEvent evt) => SetBiome(evt.CurrentRegion);

        private void OnGamePaused(GamePausedEvent _) => IsPaused = true;

        private void OnGameResumed(GameResumedEvent _) => IsPaused = false;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
            {
                IsPaused = false;
            }
        }
    }
}
