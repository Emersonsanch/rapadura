using UnityEngine;

namespace Rapadura.Gameplay.Night
{
    /// <summary>
    /// "Menos visão à noite", pure and EditMode-tested (<c>NightVisibilityRulesTests</c>). Every function takes the
    /// clock's <c>DaylightFactor</c> (1 = day, 0 = full night, smooth through dawn/dusk), whether the player picked
    /// "Noite mais clara" (Acessibilidade) and whether the place is already dark on its own (the Cavernas arena), so the
    /// two darknesses never stack into pitch black.
    /// <list type="bullet">
    /// <item>Ambient: multiplied down to <see cref="NightAmbientFloor"/> at full night (<see cref="ClearerNightAmbientFloor"/>
    /// with the option, <see cref="DarkPlaceAmbientFloor"/> in the caves). Never below <see cref="AbsoluteAmbientFloor"/>.</item>
    /// <item>Fog (arena only): extra exponential density up to <see cref="NightFogDensity"/>, so the far arena fades out.</item>
    /// <item>Camera far plane (arena only): shortened towards <see cref="NightFarClip"/>, never closer than the camera's
    /// distance to the hero + <see cref="MinVisibleBeyondHero"/>, never farther than the scene's own.</item>
    /// <item>Lamparina: a warm light around the hero, <see cref="BaseLightRadius"/> m (+ item/mount bonus, capped at
    /// <see cref="MaxLightRadius"/>), lit with the night (always lit in the caves).</item>
    /// </list>
    /// The enemies' glowing eyes (<see cref="NightVariantTag"/>) are unlit, so they stay readable through all of this.
    /// </summary>
    public static class NightVisibilityRules
    {
        public const float NightAmbientFloor = 0.7f;
        public const float ClearerNightAmbientFloor = 0.9f;

        /// <summary>A place already dark by itself (caves) only loses a little more.</summary>
        public const float DarkPlaceAmbientFloor = 0.88f;

        /// <summary>Hard floor on the ambient multiplier, whatever the inputs.</summary>
        public const float AbsoluteAmbientFloor = 0.6f;

        public const float NightFogDensity = 0.022f;
        public const float ClearerNightFogDensity = 0.01f;
        public const float DarkPlaceFogDensity = 0.008f;

        public const float NightFarClip = 70f;
        public const float ClearerNightFarClip = 120f;
        public const float MinVisibleBeyondHero = 40f;

        public const float BaseLightRadius = 8f;
        public const float MaxLightRadius = 16f;
        public const float LightIntensity = 1.6f;
        public static readonly Color LightColor = new Color(1f, 0.82f, 0.55f, 1f);

        /// <summary>The lamparina never goes fully out in a dark place (caves), whatever the clock says.</summary>
        public const float DarkPlaceMinNight = 0.6f;

        /// <summary>0 by day → 1 at full night, from the clock's daylight factor (clamped).</summary>
        public static float NightAmount(float daylightFactor) => 1f - Mathf.Clamp01(daylightFactor);

        public static float AmbientMultiplier(float daylightFactor, bool clearerNight, bool darkPlace)
        {
            float floor = darkPlace ? DarkPlaceAmbientFloor : NightAmbientFloor;
            if (clearerNight)
            {
                floor = Mathf.Max(floor, ClearerNightAmbientFloor);
            }

            floor = Mathf.Max(AbsoluteAmbientFloor, floor);
            return Mathf.Lerp(1f, floor, NightAmount(daylightFactor));
        }

        public static float ExtraFogDensity(float daylightFactor, bool clearerNight, bool darkPlace)
        {
            float density = darkPlace ? DarkPlaceFogDensity : NightFogDensity;
            if (clearerNight)
            {
                density = Mathf.Min(density, ClearerNightFogDensity);
            }

            return density * NightAmount(daylightFactor);
        }

        /// <summary>Far clip plane at this hour. <paramref name="sceneFarClip"/> is the camera's own (restored by day).</summary>
        public static float FarClip(float sceneFarClip, float daylightFactor, bool clearerNight, float cameraToHeroDistance)
        {
            float nightFar = clearerNight ? ClearerNightFarClip : NightFarClip;
            nightFar = Mathf.Max(nightFar, Mathf.Max(0f, cameraToHeroDistance) + MinVisibleBeyondHero);
            if (sceneFarClip <= nightFar)
            {
                return sceneFarClip;
            }

            return Mathf.Lerp(sceneFarClip, nightFar, NightAmount(daylightFactor));
        }

        /// <summary>Lamparina radius: base + bonus (items such as a better lamparina, a cave-light mount ability), capped.</summary>
        public static float LightRadius(float bonusMeters) => Mathf.Clamp(BaseLightRadius + Mathf.Max(0f, bonusMeters), BaseLightRadius, MaxLightRadius);

        /// <summary>Lamparina intensity: follows the night; at least <see cref="DarkPlaceMinNight"/> of it in a dark place.</summary>
        public static float LightIntensityFor(float daylightFactor, bool darkPlace)
        {
            float night = NightAmount(daylightFactor);
            if (darkPlace)
            {
                night = Mathf.Max(night, DarkPlaceMinNight);
            }

            return LightIntensity * night;
        }
    }
}
