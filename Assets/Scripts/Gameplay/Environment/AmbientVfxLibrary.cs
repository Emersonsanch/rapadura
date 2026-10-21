using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Data-driven table of ambient particle layers per biome, loaded by <see cref="AmbientVfx"/>
    /// from <c>Resources/Vfx/AmbientVfxLibrary</c> (falls back to <see cref="CreateBuiltInDefaults"/>
    /// when the asset is missing — same pattern as <c>SkillVfxLibrary</c>). A layer is active when its
    /// biome matches and both the current <see cref="TimeOfDayPhase"/> and <see cref="WeatherType"/>
    /// are in its masks. Particle caps are always clamped to <see cref="MaxParticlesCap"/> and scaled
    /// down in the WorldMap scene and on mobile (<see cref="GetCapMultiplier"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "AmbientVfxLibrary", menuName = "Rapadura/Environment/Ambient VFX Library", order = 30)]
    public class AmbientVfxLibrary : ScriptableObject
    {
        public const string ResourcePath = "Vfx/AmbientVfxLibrary";
        public const int MaxParticlesCap = 80;

        [SerializeField] private List<AmbientVfxPreset> _presets = new List<AmbientVfxPreset>();
        [Tooltip("Cap/rate multiplier in the WorldMap scene (mostly static overworld, 30 fps on mobile).")]
        [SerializeField, Range(0.05f, 1f)] private float _worldMapCapMultiplier = 0.5f;
        [Tooltip("Extra cap/rate multiplier on mobile platforms.")]
        [SerializeField, Range(0.05f, 1f)] private float _mobileCapMultiplier = 0.75f;
        [Tooltip("Fade in/out duration when a layer turns on/off because of a phase or biome change.")]
        [SerializeField, Min(0f)] private float _fadeSeconds = 3f;

        public IReadOnlyList<AmbientVfxPreset> Presets => _presets;
        public float WorldMapCapMultiplier => _worldMapCapMultiplier;
        public float MobileCapMultiplier => _mobileCapMultiplier;
        public float FadeSeconds => _fadeSeconds;

        /// <summary>Replaces the library contents in code (runtime fallback when no asset exists, and EditMode tests).</summary>
        public void Initialize(IEnumerable<AmbientVfxPreset> presets, float worldMapCapMultiplier = 0.5f, float mobileCapMultiplier = 0.75f, float fadeSeconds = 3f)
        {
            _presets = presets != null ? new List<AmbientVfxPreset>(presets) : new List<AmbientVfxPreset>();
            _worldMapCapMultiplier = Mathf.Clamp(worldMapCapMultiplier, 0.05f, 1f);
            _mobileCapMultiplier = Mathf.Clamp(mobileCapMultiplier, 0.05f, 1f);
            _fadeSeconds = Mathf.Max(0f, fadeSeconds);
        }

        public bool TryGetPreset(string presetId, out AmbientVfxPreset preset)
        {
            preset = null;
            if (string.IsNullOrEmpty(presetId) || _presets == null)
            {
                return false;
            }

            for (int i = 0; i < _presets.Count; i++)
            {
                if (_presets[i] != null && _presets[i].presetId == presetId)
                {
                    preset = _presets[i];
                    return true;
                }
            }

            return false;
        }

        /// <summary>Whether <paramref name="preset"/> should be visible for this biome/phase/weather.</summary>
        public static bool IsActive(AmbientVfxPreset preset, BiomeType biome, TimeOfDayPhase phase, WeatherType weather)
        {
            return preset != null
                && preset.biome == biome
                && preset.AllowsPhase(phase)
                && preset.AllowsWeather(weather);
        }

        /// <summary>Fills <paramref name="results"/> (cleared first) with the active layers. Allocation-free with a reused list.</summary>
        public int ResolveActive(BiomeType biome, TimeOfDayPhase phase, WeatherType weather, List<AmbientVfxPreset> results)
        {
            results.Clear();
            if (_presets == null)
            {
                return 0;
            }

            for (int i = 0; i < _presets.Count; i++)
            {
                if (IsActive(_presets[i], biome, phase, weather))
                {
                    results.Add(_presets[i]);
                }
            }

            return results.Count;
        }

        /// <summary>Combined cap/rate multiplier for the current context (1 = Main scene on desktop).</summary>
        public float GetCapMultiplier(bool isWorldMap, bool isMobile)
        {
            float multiplier = 1f;
            if (isWorldMap)
            {
                multiplier *= _worldMapCapMultiplier;
            }

            if (isMobile)
            {
                multiplier *= _mobileCapMultiplier;
            }

            return multiplier;
        }

        /// <summary>Scales a preset cap, always clamped to [1, <see cref="MaxParticlesCap"/>].</summary>
        public static int ScaleCap(int maxParticles, float multiplier)
        {
            int cap = Mathf.Min(maxParticles, MaxParticlesCap);
            return Mathf.Clamp(Mathf.RoundToInt(cap * Mathf.Max(0f, multiplier)), 1, MaxParticlesCap);
        }

        /// <summary>
        /// In-memory library with the same layers as the seeded
        /// <c>Resources/Vfx/AmbientVfxLibrary.asset</c>, used when that asset is missing.
        /// </summary>
        public static AmbientVfxLibrary CreateBuiltInDefaults()
        {
            var library = CreateInstance<AmbientVfxLibrary>();
            library.name = "AmbientVfxLibrary_BuiltIn";
            library.hideFlags = HideFlags.DontSave;
            library.Initialize(CreateDefaultPresets());
            return library;
        }

        public static AmbientVfxPreset[] CreateDefaultPresets()
        {
            const AmbientPhaseMask byDay = AmbientPhaseMask.Dawn | AmbientPhaseMask.Day;
            const AmbientPhaseMask byNight = AmbientPhaseMask.Dusk | AmbientPhaseMask.Night;
            const AmbientWeatherMask calm = AmbientWeatherMask.Clear | AmbientWeatherMask.Fog;

            return new[]
            {
                // Forest
                new AmbientVfxPreset("forest_fireflies", BiomeType.Forest, AmbientVfxKind.Fireflies, byNight, calm,
                    new Color(0.85f, 1f, 0.45f, 0.9f), new Color(1f, 0.9f, 0.4f, 0.8f), 4f, 30, 0.09f, 6f, 0.15f, 0f,
                    new Vector3(16f, 2.5f, 16f), 1f, isBlinking: true, isFlashLike: false, isAdditive: true),
                new AmbientVfxPreset("forest_leaves", BiomeType.Forest, AmbientVfxKind.FallingLeaves, byDay, calm,
                    new Color(0.55f, 0.7f, 0.25f, 0.9f), new Color(0.8f, 0.6f, 0.25f, 0.9f), 3f, 30, 0.14f, 7f, 0.3f, 0f,
                    new Vector3(18f, 1f, 18f), 7f),

                // Desert
                new AmbientVfxPreset("desert_dust", BiomeType.Desert, AmbientVfxKind.DustMotes, byDay, AmbientWeatherMask.Clear,
                    new Color(0.95f, 0.85f, 0.65f, 0.45f), new Color(0.85f, 0.72f, 0.5f, 0.35f), 6f, 50, 0.05f, 7f, 0.4f, 0f,
                    new Vector3(18f, 3f, 18f), 1.2f),
                new AmbientVfxPreset("desert_heat", BiomeType.Desert, AmbientVfxKind.HeatShimmer, AmbientPhaseMask.Day, AmbientWeatherMask.Clear,
                    new Color(1f, 0.95f, 0.85f, 0.08f), new Color(1f, 0.9f, 0.75f, 0.05f), 3f, 20, 0.9f, 3.5f, 0.25f, 0f,
                    new Vector3(16f, 0.3f, 16f), 0.3f),

                // Mountain
                new AmbientVfxPreset("mountain_flurry", BiomeType.Mountain, AmbientVfxKind.SnowFlurry, AmbientPhaseMask.All, calm,
                    new Color(1f, 1f, 1f, 0.8f), new Color(0.9f, 0.95f, 1f, 0.7f), 6f, 60, 0.07f, 8f, 0.4f, 0f,
                    new Vector3(20f, 1f, 20f), 8f),
                new AmbientVfxPreset("mountain_wind", BiomeType.Mountain, AmbientVfxKind.WindWisps, AmbientPhaseMask.All, calm | AmbientWeatherMask.Snow,
                    new Color(0.92f, 0.96f, 1f, 0.25f), new Color(0.85f, 0.9f, 1f, 0.15f), 2f, 16, 0.06f, 2.5f, 4f, 0f,
                    new Vector3(6f, 3f, 18f), 1.5f),

                // Caves (underground: ignores weather)
                new AmbientVfxPreset("caves_dust", BiomeType.Caves, AmbientVfxKind.DustMotes, AmbientPhaseMask.All, AmbientWeatherMask.All,
                    new Color(0.8f, 0.75f, 0.65f, 0.35f), new Color(0.65f, 0.6f, 0.5f, 0.25f), 4f, 40, 0.04f, 8f, 0.1f, 0f,
                    new Vector3(14f, 3f, 14f), 1.5f),
                new AmbientVfxPreset("caves_drips", BiomeType.Caves, AmbientVfxKind.Drips, AmbientPhaseMask.All, AmbientWeatherMask.All,
                    new Color(0.7f, 0.85f, 1f, 0.9f), new Color(0.9f, 0.95f, 1f, 0.8f), 0.8f, 10, 0.05f, 1.5f, 0f, 1f,
                    new Vector3(12f, 0.2f, 12f), 5f, isBlinking: true, isFlashLike: true, isAdditive: true),
            };
        }
    }
}
