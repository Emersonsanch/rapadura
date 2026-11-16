using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>A pattern raked in a gravel garden of the Jardins de Lava Laqueada (66).</summary>
    public enum GravelPattern
    {
        Waves = 0,
        Rings = 1,
        Lines = 2,
        Spiral = 3,
    }

    /// <summary>What stands beside a gravel garden: it says which pattern calms it.</summary>
    public enum GravelNeighbour
    {
        Water = 0,
        Stone = 1,
        Pine = 2,
    }

    /// <summary>
    /// The raked gravel of the Jardins de Lava Laqueada (Docs/Mapas100.md 66: "rastelar os desenhos certos (quebra-cabeça) acalma os jardins e abre
    /// a casa de chá secreta"), pure and server-runnable. Three gardens, each beside water, a stone or a pine; [E] rakes the next of four patterns.
    /// Mestra Akane's rule, told at the gardens: "beside water, waves; round a stone, rings; under a pine, straight lines like its needles; the
    /// spiral is the wind's, and no garden here belongs to the wind".
    /// <code>
    /// each UTC day the tanukis mess the gravel: every garden starts on a pattern from the day's seed, never all three right
    /// all three right: the gardens are calm for the rest of the UTC day and the hidden tea house opens
    /// the hidden tea house: the first time 80 gold + 1 Pedra-Faísca, then 40 gold once a UTC day while the gardens are calm
    /// </code>
    /// The rakes themselves are not saved (a reload brings the morning's mess back); the save keeps the day the gardens were calmed, the day
    /// the tea was served and the moment the tea house was first found.
    /// </summary>
    public static class HinokuraGravelRules
    {
        public const int PatternCount = 4;

        /// <summary>Metres from a garden's centre where [E] rakes it.</summary>
        public const float RakeReach = 4.5f;

        /// <summary>Metres from the hidden tea house where [E] takes the tea.</summary>
        public const float TeaReach = 4f;

        public const int FirstGold = 80;
        public const int FirstStones = 1;
        public const int DailyGold = 40;

        public const string CalmDayKey = "hinokura.gravel.calm";
        public const string TeaDayKey = "hinokura.gravel.tea";
        public const string FoundKey = "hinokura.gravel.found";

        /// <summary>What stands beside each garden of <c>OverworldLayouts.GravelGardens</c>.</summary>
        public static readonly GravelNeighbour[] Neighbours = { GravelNeighbour.Pine, GravelNeighbour.Water, GravelNeighbour.Stone };

        public static GravelPattern Target(GravelNeighbour neighbour)
        {
            switch (neighbour)
            {
                case GravelNeighbour.Water: return GravelPattern.Waves;
                case GravelNeighbour.Stone: return GravelPattern.Rings;
                default: return GravelPattern.Lines;
            }
        }

        public static GravelPattern Next(GravelPattern pattern) => (GravelPattern)(((int)pattern + 1) % PatternCount);

        /// <summary>The morning's mess of UTC day <paramref name="day"/>: one pattern per garden, never all of them right.</summary>
        public static GravelPattern[] StartFor(long day)
        {
            var patterns = new GravelPattern[Neighbours.Length];
            ulong state = unchecked((ulong)day * 0x9E3779B97F4A7C15UL + 0x66UL);
            for (int i = 0; i < patterns.Length; i++)
            {
                state = unchecked(state * 6364136223846793005UL + 1442695040888963407UL);
                patterns[i] = (GravelPattern)(int)((state >> 33) % PatternCount);
            }

            if (AllRight(patterns))
            {
                patterns[(int)(((ulong)day) % (ulong)patterns.Length)] = Next(patterns[(int)(((ulong)day) % (ulong)patterns.Length)]);
            }

            return patterns;
        }

        /// <summary>Whether garden <paramref name="garden"/> shows its right pattern.</summary>
        public static bool IsRight(IReadOnlyList<GravelPattern> patterns, int garden) => patterns[garden] == Target(Neighbours[garden]);

        public static int RightCount(IReadOnlyList<GravelPattern> patterns)
        {
            int right = 0;
            for (int i = 0; i < patterns.Count && i < Neighbours.Length; i++)
            {
                if (IsRight(patterns, i))
                {
                    right++;
                }
            }

            return right;
        }

        public static bool AllRight(IReadOnlyList<GravelPattern> patterns) => RightCount(patterns) == Neighbours.Length;

        /// <summary>The right pattern in every garden (a calm day).</summary>
        public static GravelPattern[] Calm()
        {
            var patterns = new GravelPattern[Neighbours.Length];
            for (int i = 0; i < patterns.Length; i++)
            {
                patterns[i] = Target(Neighbours[i]);
            }

            return patterns;
        }

        /// <summary>The garden within <see cref="RakeReach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int GardenAt(IReadOnlyList<Vector2> gardens, Vector2 p)
        {
            int best = -1;
            float bestSqr = RakeReach * RakeReach;
            for (int i = 0; gardens != null && i < gardens.Count; i++)
            {
                float sqr = (gardens[i] - p).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>The reward of the tea: (gold, Pedras-Faísca); nothing when the gardens are not calm or the tea was already served today.</summary>
        public static (int Gold, int Stones) TeaReward(bool calm, bool servedToday, bool foundBefore)
        {
            if (!calm || servedToday)
            {
                return (0, 0);
            }

            return foundBefore ? (DailyGold, 0) : (FirstGold, FirstStones);
        }
    }
}
