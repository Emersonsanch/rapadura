using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The Huma's help in the fight with the Rei-Vendaval (Docs/Mapas100.md 58: "a Huma ajuda na luta"; the huma is the bird whose shadow brings
    /// luck), pure and server-runnable: the fight clock is the seconds since the hero came within <see cref="FightRange"/> of the living boss.
    /// <code>
    /// first pass at 12 s, then every 24 s · the shadow is marked for 4 s as a golden ring (3.5 m) 6 m from where the hero stood
    /// the ring turns 137.5° round the hero each pass (never twice in the same spot)
    /// standing in it while it is marked: +8% of max health (never over max) and +15% attack damage for 10 s, once per pass
    /// </code>
    /// Decision (2026-11-16): the Huma helps the hero, never hurts the boss, so the fight's three phases stay as they were; it is a reason to move
    /// on the hero's own terms between the Vendaval's feathers and the marked Grande Golpe. Online each player gets their own shadow.
    /// </summary>
    public static class HumaShadowRules
    {
        public const float FirstPassSeconds = 12f;
        public const float PassSeconds = 24f;
        public const float MarkSeconds = 4f;
        public const float RingRadius = 3.5f;
        public const float RingOffset = 6f;
        public const float HealFraction = 0.08f;
        public const float DamageBonus = 0.15f;
        public const float BuffSeconds = 10f;
        public const float GoldenAngleDegrees = 137.5f;

        /// <summary>Metres from the arena inside which the boss's fight counts (the boss itself leaves at 150 m).</summary>
        public const float FightRange = 45f;

        /// <summary>The pass the fight clock is in (0, 1, 2…), or −1 before the first one.</summary>
        public static int PassIndex(float fightSeconds) =>
            fightSeconds < FirstPassSeconds ? -1 : Mathf.FloorToInt((fightSeconds - FirstPassSeconds) / PassSeconds);

        /// <summary>Whether the shadow of the current pass is on the ground.</summary>
        public static bool Marked(float fightSeconds)
        {
            if (PassIndex(fightSeconds) < 0)
            {
                return false;
            }

            float into = (fightSeconds - FirstPassSeconds) % PassSeconds;
            return into < MarkSeconds;
        }

        /// <summary>Seconds until the next shadow is marked (0 while one is).</summary>
        public static float SecondsToNext(float fightSeconds)
        {
            if (fightSeconds < FirstPassSeconds)
            {
                return FirstPassSeconds - fightSeconds;
            }

            return Marked(fightSeconds) ? 0f : PassSeconds - (fightSeconds - FirstPassSeconds) % PassSeconds;
        }

        /// <summary>Where the shadow of pass <paramref name="pass"/> falls for a hero who stood at <paramref name="hero"/> when it was marked.</summary>
        public static Vector2 RingCenter(Vector2 hero, int pass)
        {
            float a = pass * GoldenAngleDegrees * Mathf.Deg2Rad;
            return hero + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * RingOffset;
        }

        public static bool Inside(Vector2 center, Vector2 p) => (p - center).sqrMagnitude <= RingRadius * RingRadius;

        /// <summary>The health the shadow gives back: <see cref="HealFraction"/> of max, never past max.</summary>
        public static float HealAmount(float current, float max) => Mathf.Max(0f, Mathf.Min(max * HealFraction, max - current));
    }
}
