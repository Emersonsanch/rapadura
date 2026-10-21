using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Temporary, process-wide adjustments that the scene-local environment presenters honour on
    /// every refresh: <see cref="WeatherVisuals"/> uses <see cref="HasFogBaseline"/> instead of the
    /// scene's authored clear-weather fog, and <see cref="DayNightLighting"/> multiplies its ambient
    /// colour by <see cref="AmbientTint"/> and the sun by <see cref="SunIntensityMultiplier"/>.
    /// Set by BiomeArenaDressing while an encounter dresses the shared combat arena as another biome
    /// (those presenters rewrite RenderSettings every tick, so writing RenderSettings directly would
    /// be overwritten). Defaults change nothing; <see cref="Reset"/> restores them.
    /// </summary>
    public static class EnvironmentOverrides
    {
        public static bool HasFogBaseline { get; private set; }
        public static Color FogBaselineColor { get; private set; } = Color.gray;
        public static float FogBaselineDensity { get; private set; }

        public static Color AmbientTint { get; private set; } = Color.white;
        public static float SunIntensityMultiplier { get; private set; } = 1f;

        public static bool IsAnyActive =>
            HasFogBaseline || AmbientTint != Color.white || !Mathf.Approximately(SunIntensityMultiplier, 1f);

        /// <summary>Clear-weather fog becomes exponential fog of this colour/density (weather fog still blends on top).</summary>
        public static void SetFogBaseline(Color color, float density)
        {
            HasFogBaseline = true;
            FogBaselineColor = color;
            FogBaselineDensity = Mathf.Max(0f, density);
        }

        public static void ClearFogBaseline()
        {
            HasFogBaseline = false;
            FogBaselineColor = Color.gray;
            FogBaselineDensity = 0f;
        }

        public static void SetLighting(Color ambientTint, float sunIntensityMultiplier)
        {
            AmbientTint = ambientTint;
            SunIntensityMultiplier = Mathf.Max(0f, sunIntensityMultiplier);
        }

        public static void Reset()
        {
            ClearFogBaseline();
            AmbientTint = Color.white;
            SunIntensityMultiplier = 1f;
        }

        // Statics survive Enter Play Mode without a domain reload; start every session neutral.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad() => Reset();
    }
}
