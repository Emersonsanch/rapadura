using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The water of the Oásis de Qocho coming back (map 09, Docs/Mapas100.md: "A Raiz que Bebeu o Poço ... libertada, devolve a água ao
    /// oásis"), pure and server-runnable. While the Root drinks the karez the oasis lake is low and gives nothing; once the Root is freed (the
    /// boss defeated, kept by the save's defeated registry) the lake is Living Water again: on its shore, out of combat, it mends the hero.
    /// <code>
    /// the lake: the oasis river east of Qocho, 26 m wide · the shore: up to 5 m past the water
    /// Root freed · on the shore · 4 s out of combat: 3% of max life and 3% of max stamina a second, up to full
    /// Root not freed: nothing (Yardena says the water is low)
    /// </code>
    /// </summary>
    public static class QochoOasisRules
    {
        public const string RootEnemyId = "enemy_mountain_well_drinking_root";
        public const float LakeWidth = 26f;
        public const float ShoreReach = 5f;
        public const float HealPerSecond = 0.03f;
        public const float StaminaPerSecond = 0.03f;
        public const float OutOfCombatSeconds = 4f;

        /// <summary>The ends of the oasis lake (the river the Areias layout draws east of the town).</summary>
        public static readonly Vector2 LakeA = new Vector2(112f, -112f);
        public static readonly Vector2 LakeB = new Vector2(172f, -140f);

        /// <summary>Whether <paramref name="local"/> (Areias coordinates) is in the lake or on its shore.</summary>
        public static bool OnShore(Vector2 local) => MapLayoutRules.DistanceToSegment(local, LakeA, LakeB) <= LakeWidth * 0.5f + ShoreReach;

        /// <summary>Whether the lake mends now: the Root freed, the hero alive, on the shore and out of combat.</summary>
        public static bool Mends(bool rootFreed, bool alive, bool onShore, float secondsSinceCombat) =>
            rootFreed && alive && onShore && secondsSinceCombat >= OutOfCombatSeconds;

        /// <summary>What <paramref name="seconds"/> at <paramref name="fractionPerSecond"/> of <paramref name="max"/> gives back, never past full.</summary>
        public static float MendOver(float current, float max, float fractionPerSecond, float seconds)
        {
            if (max <= 0f || seconds <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp(max * fractionPerSecond * seconds, 0f, Mathf.Max(0f, max - current));
        }
    }
}
