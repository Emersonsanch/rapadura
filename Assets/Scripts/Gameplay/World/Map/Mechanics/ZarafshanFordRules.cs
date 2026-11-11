using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The fords of the Tugai as Água Viva (Docs/Mapas100.md 02), pure. The sheet's ×0.6 wading speed is deliberately not applied: this is the
    /// second map of the game and a first-hour slowdown on its main crossings would only feel like a bug. Instead the ford is where the region's
    /// element shows itself, gently: it mends a hurt hero up to a ceiling, in or out of a fight, and asks for nothing.
    /// <code>
    /// 6 m around a ford · 3% of max health per second · only up to 70% of max health, never lowers it
    /// </code>
    /// </summary>
    public static class ZarafshanFordRules
    {
        public const float Radius = 6f;
        public const float HealFractionPerSecond = 0.03f;
        public const float CeilingFraction = 0.7f;

        /// <summary>Index of the ford within reach of <paramref name="local"/> (the closest), or -1.</summary>
        public static int FordAt(Vector2 local, IReadOnlyList<Vector2> fords)
        {
            int best = -1;
            float bestSqr = Radius * Radius;
            for (int i = 0; fords != null && i < fords.Count; i++)
            {
                float sqr = (fords[i] - local).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        public static bool BelowCeiling(float health, float maxHealth) => maxHealth > 0f && health < maxHealth * CeilingFraction;

        /// <summary>Health mended over <paramref name="seconds"/>; stops at the ceiling and is never negative.</summary>
        public static float HealOver(float health, float maxHealth, float seconds)
        {
            if (maxHealth <= 0f || seconds <= 0f)
            {
                return 0f;
            }

            return Mathf.Min(maxHealth * HealFractionPerSecond * seconds, Mathf.Max(0f, maxHealth * CeilingFraction - health));
        }
    }
}
