using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// Process-wide context of <see cref="NightVisibilityRules"/>, read by the scene presenters that already rewrite
    /// RenderSettings every refresh (DayNightLighting multiplies its ambient by <see cref="AmbientMultiplier"/>,
    /// WeatherVisuals adds <see cref="ExtraFogDensity"/>) — writing RenderSettings from here would be overwritten, the
    /// same reason <c>EnvironmentOverrides</c> exists. <see cref="InArena"/>/<see cref="DarkPlace"/> are set by
    /// <see cref="NightArenaDirector"/> for the encounter arena (fog, far plane and lamparina only there; the world map
    /// only gets the milder ambient darkening). <see cref="LightRadiusBonus"/> is the hook for items/mounts that make
    /// the lamparina bigger. Defaults change nothing but the ambient at night.
    /// </summary>
    public static class NightVisibility
    {
        /// <summary>True while the combat arena is up (set/cleared by <see cref="NightArenaDirector"/>).</summary>
        public static bool InArena { get; set; }

        /// <summary>True when the current place is dark by itself (the Cavernas arena), so night darkening is softened.</summary>
        public static bool DarkPlace { get; set; }

        /// <summary>Extra lamparina radius (m) from items/mount abilities. Not saved here: whoever grants it sets it.</summary>
        public static float LightRadiusBonus { get; set; }

        /// <summary>Test override for <see cref="ClearerNight"/> (null = read AccessibilitySettings).</summary>
        public static bool? ClearerNightOverride { get; set; }

        /// <summary>"Noite mais clara" (Acessibilidade): limits every night darkening.</summary>
        public static bool ClearerNight
        {
            get
            {
                if (ClearerNightOverride.HasValue)
                {
                    return ClearerNightOverride.Value;
                }

                return ServiceLocator.TryGet(out AccessibilitySettings settings) && settings != null && settings.ClearerNight;
            }
        }

        public static float AmbientMultiplier(float daylightFactor) =>
            NightVisibilityRules.AmbientMultiplier(daylightFactor, ClearerNight, DarkPlace);

        /// <summary>Extra exponential fog density; 0 outside the arena.</summary>
        public static float ExtraFogDensity(float daylightFactor) =>
            InArena ? NightVisibilityRules.ExtraFogDensity(daylightFactor, ClearerNight, DarkPlace) : 0f;

        public static void Reset()
        {
            InArena = false;
            DarkPlace = false;
            LightRadiusBonus = 0f;
            ClearerNightOverride = null;
        }

        // Statics survive Enter Play Mode without a domain reload; start every session neutral.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad() => Reset();
    }
}
