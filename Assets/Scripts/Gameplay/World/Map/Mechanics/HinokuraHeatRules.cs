using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>How hot the hero is, as a band of the meter (the driver speaks when it goes up).</summary>
    public enum HinokuraHeatBand
    {
        Cool,
        Warm,
        Danger,
        Scorching,
    }

    /// <summary>
    /// The heat meter of the Caldera (Docs/Mapas100.md 70), pure and server-runnable.
    /// <code>
    /// meter 0–100. Within 90 m of the edge of the lava lake it rises, 3 per second at the edge (33 s at worst), fading to 0 at 90 m
    /// further away it falls 12 per second; on a cold stone (7 m) it is zero at once
    /// with Karkadé Tea active it rises at half speed
    /// warnings at 50 and 80; only at 100 it hurts: 1.5% of max health per second, never below 20%
    /// </code>
    /// The sheet says the tea zeroes the meter; meals are cooked in the village and last for encounters, so a zeroing sip could never
    /// happen at the caldera. Halving keeps the dish useful where it is eaten. The heat never kills and never holds the hero.
    /// </summary>
    public static class HinokuraHeatRules
    {
        public const float MaxHeat = 100f;
        public const float HeatRange = 90f;
        public const float MaxGainPerSecond = 3f;
        public const float CoolPerSecond = 12f;
        public const float ColdStoneRadius = 7f;
        public const float TeaFactor = 0.5f;
        public const float WarmAt = 50f;
        public const float DangerAt = 80f;
        public const float BurnFractionPerSecond = 0.015f;
        public const float BurnFloorFraction = 0.2f;

        /// <summary>The meter after <paramref name="seconds"/> at <paramref name="distanceToLava"/> metres from the edge of the lake (negative inside).</summary>
        public static float Step(float heat, float distanceToLava, bool onColdStone, bool tea, float seconds)
        {
            if (onColdStone)
            {
                return 0f;
            }

            if (distanceToLava >= HeatRange)
            {
                return Mathf.Max(0f, heat - CoolPerSecond * seconds);
            }

            float gain = MaxGainPerSecond * (1f - Mathf.Clamp01(distanceToLava / HeatRange)) * (tea ? TeaFactor : 1f);
            return Mathf.Min(MaxHeat, heat + gain * seconds);
        }

        public static HinokuraHeatBand BandOf(float heat)
        {
            if (heat >= MaxHeat)
            {
                return HinokuraHeatBand.Scorching;
            }

            if (heat >= DangerAt)
            {
                return HinokuraHeatBand.Danger;
            }

            return heat >= WarmAt ? HinokuraHeatBand.Warm : HinokuraHeatBand.Cool;
        }

        /// <summary>Health the heat takes over <paramref name="seconds"/>: nothing below a full meter or at the floor.</summary>
        public static float BurnAmount(float heat, float current, float max, float seconds)
        {
            float floor = max * BurnFloorFraction;
            if (heat < MaxHeat || current <= floor)
            {
                return 0f;
            }

            return Mathf.Min(current - floor, max * BurnFractionPerSecond * seconds);
        }
    }
}
