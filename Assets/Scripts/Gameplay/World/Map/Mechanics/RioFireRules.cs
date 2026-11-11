using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The fire that chooses (Docs/Mapas100.md, map 75: the lake heals whoever carries light and burns the dimmed beasts), pure
    /// and server-runnable. Pools are the (A, B, Width) capsules of <c>OverworldLayouts.LagoDeFogoPools</c>.
    /// <code>
    /// shore band 8 m from the edge of the fire (the fire itself counts as shore)
    /// hero: +2.5% of max health per second, only after 5 s without being hurt
    /// beasts: 4% of max health per second as raw fire damage, one tick a second, never below 20% and never a boss
    /// </code>
    /// The fire never hurts the hero. It cannot kill a beast either, so the last blow, its experience and its loot stay with the
    /// hero: luring beasts to the shore softens them, it does not farm them.
    /// </summary>
    public static class RioFireRules
    {
        public const float ShoreBand = 8f;
        public const float HealFractionPerSecond = 0.025f;
        public const float OutOfCombatSeconds = 5f;
        public const float BurnFractionPerSecond = 0.04f;
        public const float BurnFloorFraction = 0.2f;

        /// <summary>Metres from <paramref name="p"/> to the edge of the nearest pool: negative inside the fire.</summary>
        public static float DistanceToFire(IReadOnlyList<(Vector2 A, Vector2 B, float Width)> pools, Vector2 p)
        {
            float best = float.PositiveInfinity;
            for (int i = 0; i < pools.Count; i++)
            {
                (Vector2 a, Vector2 b, float width) = pools[i];
                Vector2 ab = b - a;
                float sqr = ab.sqrMagnitude;
                float t = sqr > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / sqr) : 0f;
                float d = Vector2.Distance(p, a + ab * t) - width * 0.5f;
                if (d < best)
                {
                    best = d;
                }
            }

            return best;
        }

        public static bool OnShore(IReadOnlyList<(Vector2 A, Vector2 B, float Width)> pools, Vector2 p) => DistanceToFire(pools, p) <= ShoreBand;

        /// <summary>Health the fire gives a hero over <paramref name="seconds"/>: nothing while fighting or when already full.</summary>
        public static float HealAmount(float current, float max, float secondsSinceHurt, float seconds)
        {
            if (secondsSinceHurt < OutOfCombatSeconds || current >= max || current <= 0f)
            {
                return 0f;
            }

            return Mathf.Min(max - current, max * HealFractionPerSecond * seconds);
        }

        /// <summary>Raw damage a one-second tick does to a beast on the shore: zero for a boss, a beast at the floor, or a dead one.</summary>
        public static float BurnAmount(float current, float max, bool isBoss)
        {
            float floor = max * BurnFloorFraction;
            if (isBoss || current <= floor)
            {
                return 0f;
            }

            return Mathf.Min(current - floor, max * BurnFractionPerSecond);
        }
    }
}
