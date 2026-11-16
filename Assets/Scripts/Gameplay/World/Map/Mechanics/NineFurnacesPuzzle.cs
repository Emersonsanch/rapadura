using System.Collections.Generic;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What a pump of a furnace's bellows did (<see cref="NineFurnacesPuzzle.Pump"/>).</summary>
    public enum FurnacePumpResult
    {
        Pumped = 0,
        /// <summary>The furnace is already at its hottest: the bellows will not take it further.</summary>
        TooHot = 1,
        /// <summary>A neighbour would go colder than its coldest: the bellows will not pull more heat from it.</summary>
        NeighbourTooCold = 2,
        /// <summary>All nine are balanced already.</summary>
        Balanced = 3,
    }

    /// <summary>
    /// The temperature of the nine furnaces of the Castelo das Nove Fornalhas (Docs/Mapas100.md 69: "cada fornalha tem um medidor; equilibrar as
    /// 9 (quebra-cabeça em grupo) abre o pátio"; "Noite: as fornalhas ficam mais quentes (mais difícil)"), pure and server-runnable: the state is
    /// nine numbers any server can hold for every player in the court at once (that is what makes it a group puzzle).
    /// <code>
    /// heat 1–9 per furnace, balanced = all nine at 5 · the furnaces stand in a ring
    /// a bellows (pump) on furnace i: i +2, its two neighbours −1 each (the heat moves, the total stays 45)
    /// a pump is refused past 9 on i or under 1 on a neighbour
    /// the day's fire: from all-5, 4 distinct furnaces (5 at night) had their heat pulled into their neighbours (i −2, neighbours +1);
    /// pumping exactly those furnaces once each, in any order, balances the ring
    /// 3 minutes without a pump: the fire settles back to a new start (and after each balance too)
    /// </code>
    /// The reward (once a UTC day, the first time with a Pedra da Mistura) is kept by the driver.
    /// Decision (2026-11-16): the court has no wall to open, so balancing the ring "opens" the court's forge: the day's reward of 60 gold and a
    /// Pedra-Faísca.
    /// </summary>
    public static class NineFurnacesPuzzle
    {
        public const int Count = 9;
        public const int Balance = 5;
        public const int MinHeat = 1;
        public const int MaxHeat = 9;
        public const int PumpHeat = 2;
        public const int PumpPull = 1;
        public const int DayScrambles = 4;
        public const int NightScrambles = 5;
        public const float IdleResetSeconds = 180f;

        /// <summary>Metres from a furnace's bellows where [E] pumps it.</summary>
        public const float Reach = 5f;

        public const int DailyGold = 60;
        public const int DailyStones = 1;
        public const int FirstMixtureStones = 1;

        public const string DayKey = "hinokura.furnaces.day";
        public const string FirstKey = "hinokura.furnaces.first";

        public static int Left(int i) => (i + Count - 1) % Count;

        public static int Right(int i) => (i + 1) % Count;

        /// <summary>The distinct furnaces whose heat the fire of <paramref name="seed"/> pulled away (<paramref name="scrambles"/> of them, 1–8).</summary>
        public static int[] ScrambledFurnaces(long seed, int scrambles)
        {
            int count = scrambles < 1 ? 1 : scrambles > Count - 1 ? Count - 1 : scrambles;
            var order = new List<int>(Count);
            for (int i = 0; i < Count; i++)
            {
                order.Add(i);
            }

            ulong state = unchecked((ulong)seed * 0x9E3779B97F4A7C15UL + 0x69UL);
            var picked = new int[count];
            for (int k = 0; k < count; k++)
            {
                state = unchecked(state * 6364136223846793005UL + 1442695040888963407UL);
                int at = (int)((state >> 33) % (ulong)order.Count);
                picked[k] = order[at];
                order.RemoveAt(at);
            }

            return picked;
        }

        /// <summary>The ring's start for <paramref name="seed"/>: all at <see cref="Balance"/>, then the scrambled furnaces' heat pulled into their neighbours.</summary>
        public static int[] Start(long seed, int scrambles)
        {
            var heat = new int[Count];
            for (int i = 0; i < Count; i++)
            {
                heat[i] = Balance;
            }

            foreach (int i in ScrambledFurnaces(seed, scrambles))
            {
                heat[i] -= PumpHeat;
                heat[Left(i)] += PumpPull;
                heat[Right(i)] += PumpPull;
            }

            return heat;
        }

        /// <summary>The seed of round <paramref name="round"/> of UTC day <paramref name="day"/> (night is a different fire).</summary>
        public static long SeedFor(long day, int round, bool night) => day * 64L + round * 2L + (night ? 1L : 0L);

        public static int ScramblesFor(bool night) => night ? NightScrambles : DayScrambles;

        /// <summary>Pumps the bellows of furnace <paramref name="i"/> in <paramref name="heat"/> (changed only when the result is <see cref="FurnacePumpResult.Pumped"/>).</summary>
        public static FurnacePumpResult Pump(int[] heat, int i)
        {
            if (IsBalanced(heat))
            {
                return FurnacePumpResult.Balanced;
            }

            if (heat[i] + PumpHeat > MaxHeat)
            {
                return FurnacePumpResult.TooHot;
            }

            if (heat[Left(i)] - PumpPull < MinHeat || heat[Right(i)] - PumpPull < MinHeat)
            {
                return FurnacePumpResult.NeighbourTooCold;
            }

            heat[i] += PumpHeat;
            heat[Left(i)] -= PumpPull;
            heat[Right(i)] -= PumpPull;
            return FurnacePumpResult.Pumped;
        }

        public static int BalancedCount(IReadOnlyList<int> heat)
        {
            int count = 0;
            for (int i = 0; i < heat.Count; i++)
            {
                if (heat[i] == Balance)
                {
                    count++;
                }
            }

            return count;
        }

        public static bool IsBalanced(IReadOnlyList<int> heat) => BalancedCount(heat) == heat.Count;

        /// <summary>The bellows within <see cref="Reach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int FurnaceAt(IReadOnlyList<UnityEngine.Vector2> bellows, UnityEngine.Vector2 p)
        {
            int best = -1;
            float bestSqr = Reach * Reach;
            for (int i = 0; bellows != null && i < bellows.Count; i++)
            {
                float sqr = (bellows[i] - p).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>The reward of a balance: (gold, Pedras-Faísca, Pedras da Mistura); nothing when the day's was already paid.</summary>
        public static (int Gold, int Stones, int Mixture) Reward(bool paidToday, bool firstEver)
        {
            if (paidToday)
            {
                return (0, 0, 0);
            }

            return (DailyGold, DailyStones, firstEver ? FirstMixtureStones : 0);
        }
    }
}
