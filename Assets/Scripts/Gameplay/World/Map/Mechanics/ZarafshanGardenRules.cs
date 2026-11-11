using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One trapped spark of the Jardim de Mihrshah (map 06) and the arm of the dry canal it fills, in the region's own coordinates.</summary>
    public readonly struct ZarafshanGardenSpark
    {
        public readonly string Id;
        public readonly Vector2 Cage;
        public readonly Vector2 CanalA;
        public readonly Vector2 CanalB;

        public ZarafshanGardenSpark(string id, Vector2 cage, Vector2 canalA, Vector2 canalB)
        {
            Id = id;
            Cage = cage;
            CanalA = canalA;
            CanalB = canalB;
        }
    }

    /// <summary>What [E] at a spark's cage does (<see cref="ZarafshanGardenRules.Check"/>).</summary>
    public enum ZarafshanSparkCheck
    {
        /// <summary>No beast guards the cage any more: [E] frees the spark.</summary>
        Free = 0,

        /// <summary>The quadrant's beasts still stand round the cage.</summary>
        Guarded = 1,

        /// <summary>This spark is already free (its canal arm runs with Água Viva).</summary>
        AlreadyFree = 2,
    }

    /// <summary>
    /// The canals of the Jardim Esquecido de Mihrshah (Docs/Mapas100.md 06: "libertar as 4 faíscas dos quadrantes enche os canais de Água Viva e
    /// abre o pavilhão; a água acesa cura fora de combate"), pure and server-runnable. One spark is caught in each quadrant of the chahar bagh,
    /// on the clearing where the quadrant's beasts stand; it can only be freed once none of them is alive round it, so freeing the four is the
    /// clearing of the four quadrants. Each freed spark fills one arm of the cross of canals for ever (a time mark in <c>WorldMapProgress</c>, the
    /// second it was freed; 0 = still caught). Walking beside a lit arm out of combat mends life; with all four free the pavilion at the centre
    /// (Saklas's arena outside his event) mends life and mana faster.
    /// <code>
    /// 4 sparks, one per quadrant, reach 3 m · guarded while a live beast (not a boss) stands within 14 m of the cage
    /// a lit canal arm: within 3.5 m of it, 4 % of max life a second, up to full, after 4 s out of combat
    /// the open pavilion (all 4 free): within 9 m of the centre, 6 % of max life and 6 % of max mana a second, after 4 s out of combat
    /// out of combat = no damage dealt or taken by the hero for 4 s · freed for ever (the save keeps 4 numbers)
    /// </code>
    /// </summary>
    public static class ZarafshanGardenRules
    {
        public const float CageReach = 3f;
        public const float GuardRadius = 14f;
        public const float CanalReach = 3.5f;
        public const float CanalHealPerSecond = 0.04f;
        public const float PavilionRadius = 9f;
        public const float PavilionHealPerSecond = 0.06f;
        public const float PavilionManaPerSecond = 0.06f;
        public const float OutOfCombatSeconds = 4f;

        /// <summary>The save key of a spark (a time mark: the unix second it was freed, 0 = still caught).</summary>
        public static string FreedKey(string sparkId) => "zarafshan.garden.spark." + sparkId;

        public static bool IsFree(long freedAt) => freedAt > 0L;

        /// <summary>How many of the sparks are free.</summary>
        public static int FreeCount(IReadOnlyList<long> freedMarks)
        {
            int free = 0;
            for (int i = 0; freedMarks != null && i < freedMarks.Count; i++)
            {
                if (IsFree(freedMarks[i]))
                {
                    free++;
                }
            }

            return free;
        }

        /// <summary>Whether the pavilion is open: every one of <paramref name="sparks"/> sparks is free.</summary>
        public static bool PavilionOpen(IReadOnlyList<long> freedMarks, int sparks) => sparks > 0 && FreeCount(freedMarks) >= sparks;

        /// <summary>What [E] at a cage does: <paramref name="guards"/> = live beasts (bosses left out) within <see cref="GuardRadius"/> of it.</summary>
        public static ZarafshanSparkCheck Check(long freedAt, int guards)
        {
            if (IsFree(freedAt))
            {
                return ZarafshanSparkCheck.AlreadyFree;
            }

            return guards > 0 ? ZarafshanSparkCheck.Guarded : ZarafshanSparkCheck.Free;
        }

        /// <summary>The cage within <see cref="CageReach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int CageAt(IReadOnlyList<ZarafshanGardenSpark> sparks, Vector2 p)
        {
            int best = -1;
            float bestSq = CageReach * CageReach;
            for (int i = 0; sparks != null && i < sparks.Count; i++)
            {
                float sq = (sparks[i].Cage - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Whether a beast at <paramref name="beast"/> guards the cage at <paramref name="cage"/>.</summary>
        public static bool Guards(Vector2 cage, Vector2 beast) => (beast - cage).sqrMagnitude <= GuardRadius * GuardRadius;

        /// <summary>The lit canal arm within <see cref="CanalReach"/> of <paramref name="p"/>, or −1 (an arm is lit when its spark is free).</summary>
        public static int LitCanalAt(IReadOnlyList<ZarafshanGardenSpark> sparks, IReadOnlyList<long> freedMarks, Vector2 p)
        {
            for (int i = 0; sparks != null && freedMarks != null && i < sparks.Count && i < freedMarks.Count; i++)
            {
                if (IsFree(freedMarks[i]) && MapLayoutRules.DistanceToSegment(p, sparks[i].CanalA, sparks[i].CanalB) <= CanalReach)
                {
                    return i;
                }
            }

            return -1;
        }

        public static bool InPavilion(Vector2 centre, Vector2 p) => (p - centre).sqrMagnitude <= PavilionRadius * PavilionRadius;

        public static bool OutOfCombat(float secondsSinceCombat) => secondsSinceCombat >= OutOfCombatSeconds;

        /// <summary>Life mended over <paramref name="seconds"/> at <paramref name="fractionPerSecond"/> of the max, never past full.</summary>
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
