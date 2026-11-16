using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The seven resin lanterns of the Cedral de Hobabish (map 80, Docs/Mapas100.md: "os sete mantos: cada lanterna acesa tira um manto do
    /// chefe (fica menos protegido)"), pure and server-runnable. The lanterns stand on the ring of cedars round the Clareira do Lenhador
    /// (<see cref="OverworldLayouts.ResinLanternPosition"/>), and Hobabish's ring is inside it:
    /// <code>
    /// [E] within 3.5 m, or any fire skill that lands within 4 m (or is cast from there), lights a lantern for the rest of the game day
    /// mantles Hobabish keeps on the ground = 7 − lanterns lit, never fewer than 1 (the newest one is always his)
    /// </code>
    /// Decision (2026-11-16): the lanterns are a walk round the cedars before the fight (~940 m of ring), not a puzzle inside it; each one is
    /// worth it on its own (a mantle fewer to dodge), and all seven leave him only the mantle he just threw. The lit state is a daily mark,
    /// written when a lantern is lit; online the server keeps one set for everyone on the map.
    /// </summary>
    public static class ResinLanternRules
    {
        public const int LanternCount = OverworldLayouts.ResinLanternCount;
        public const float Reach = 3.5f;
        public const float FireReach = 4f;
        public const int MinMantles = 1;

        /// <summary>The daily mark of lantern <paramref name="index"/> (the game day it was lit).</summary>
        public static string MarkKey(int index) => "mistura.resin_lantern." + index;

        public static bool LitToday(int markedDay, int today) => markedDay > 0 && markedDay == today;

        /// <summary>How many of the lanterns (game day each was lit) burn on <paramref name="today"/>.</summary>
        public static int LitCount(IReadOnlyList<int> markedDays, int today)
        {
            int lit = 0;
            for (int i = 0; markedDays != null && i < markedDays.Count; i++)
            {
                if (LitToday(markedDays[i], today))
                {
                    lit++;
                }
            }

            return Mathf.Min(lit, LanternCount);
        }

        /// <summary>How many mantles Hobabish may keep on the ground with <paramref name="lit"/> lanterns burning.</summary>
        public static int MantleCap(int lit) => Mathf.Clamp(RegionBossRules.MantleCount - Mathf.Max(0, lit), MinMantles, RegionBossRules.MantleCount);

        /// <summary>Takes the oldest mantles back off the ground until no more than <paramref name="cap"/> are left.</summary>
        public static void TrimMantles(List<Vector2> mantles, int cap)
        {
            int keep = Mathf.Max(MinMantles, cap);
            while (mantles != null && mantles.Count > keep)
            {
                mantles.RemoveAt(0);
            }
        }

        public static bool AllLit(int lit) => lit >= LanternCount;

        /// <summary>The lantern (the nearest) within <paramref name="reach"/> of <paramref name="p"/>, or −1.</summary>
        public static int LanternNear(IReadOnlyList<Vector2> lanterns, Vector2 p, float reach)
        {
            int best = -1;
            float bestSq = reach * reach;
            for (int i = 0; lanterns != null && i < lanterns.Count; i++)
            {
                float sq = (lanterns[i] - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }
    }

    /// <summary>
    /// How many resin lanterns burn today, as Hobabish's mantles read it (<c>RegionBossAbilities</c>, SevenMantles): written by
    /// <see cref="MisturaMechanicsDriver"/> from the daily marks. Online the server keeps it.
    /// </summary>
    public static class ResinLanternBoard
    {
        public static int Lit { get; private set; }

        public static int MantleCap => ResinLanternRules.MantleCap(Lit);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset() => Lit = 0;

        public static void Set(int lit) => Lit = Mathf.Clamp(lit, 0, ResinLanternRules.LanternCount);
    }
}
