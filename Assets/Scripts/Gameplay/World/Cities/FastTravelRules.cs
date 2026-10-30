using System;
using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.World.Cities
{
    public enum FastTravelCheck
    {
        Ok,
        /// <summary>Already in that city.</summary>
        SameCity,
        /// <summary>The Barcas de Luz only stops at cities the cousins already walked to.</summary>
        NotVisited,
        /// <summary>The region is locked on the map (e.g. after starting a Nova Jornada+).</summary>
        Locked,
        NoRoute,
        NotEnoughGold,
        /// <summary>The cart leaves from a city: the token must be idle in one.</summary>
        NotInCity,
        /// <summary>Viagem rápida: the cart only stops in a region whose boss the cousins already beat (the Vila, home, always).</summary>
        BossNotDefeated,
        /// <summary>A road event card, an encounter or a trip is under way: the cart waits.</summary>
        EventActive,
    }

    /// <summary>
    /// "Cais da Barca": instant trip between cities already visited, for a little gold per road (pure, tested). The
    /// cart follows the roads, so the price is <see cref="GoldPerRoad"/> per edge of the map graph between the two
    /// cities (Vila → Lamparinas = 3 roads = 6 ouro ≈ 3 fights of gold, Docs/Balance/README.md, "Cidades"). No encounter, no
    /// road event on the way.
    /// <para>"Viagem rápida" (TODO 12.x): a stop opens only after that region's boss is beaten
    /// (<see cref="IsStopOpen"/>; the Vila, <see cref="HomeRegion"/>, is home and always open), and the cart
    /// never leaves while an event is going on (<see cref="FastTravelCheck.EventActive"/>). The trip itself is a short
    /// cosmetic transition (<c>FastTravelTransition</c>) and then the token is placed in the destination city: it is
    /// not a walk on the map, so no encounter roll, no road event card and no road chatter.</para>
    /// </summary>
    public static class FastTravelRules
    {
        public const int GoldPerRoad = 2;

        /// <summary>The Vila (Floresta): home, the cart always goes back there.</summary>
        public const BiomeType HomeRegion = BiomeType.Forest;

        /// <summary>The cart stops in <paramref name="region"/>: home, or its boss was beaten (null lookup = no boss gate).</summary>
        public static bool IsStopOpen(BiomeType region, Func<BiomeType, bool> isBossCleared)
        {
            return region == HomeRegion || isBossCleared == null || isBossCleared(region);
        }

        /// <summary>Something is happening on the map, so the cart must not leave (road event card, pending encounter, a walk under way).</summary>
        public static bool IsEventActive(bool eventCardOpen, bool encounterPending, bool travelling)
        {
            return eventCardOpen || encounterPending || travelling;
        }

        /// <summary>Roads between <paramref name="from"/> and <paramref name="to"/> (BFS over <paramref name="neighbours"/>); 0 = same, -1 = no route.</summary>
        public static int Roads(Func<BiomeType, IEnumerable<BiomeType>> neighbours, BiomeType from, BiomeType to)
        {
            if (from == to)
            {
                return 0;
            }

            if (neighbours == null)
            {
                return -1;
            }

            var distance = new Dictionary<BiomeType, int> { [from] = 0 };
            var queue = new Queue<BiomeType>();
            queue.Enqueue(from);
            while (queue.Count > 0)
            {
                BiomeType current = queue.Dequeue();
                IEnumerable<BiomeType> next = neighbours(current);
                if (next == null)
                {
                    continue;
                }

                foreach (BiomeType neighbour in next)
                {
                    if (distance.ContainsKey(neighbour))
                    {
                        continue;
                    }

                    distance[neighbour] = distance[current] + 1;
                    if (neighbour == to)
                    {
                        return distance[neighbour];
                    }

                    queue.Enqueue(neighbour);
                }
            }

            return -1;
        }

        public static int Cost(int roads) => roads <= 0 ? 0 : roads * GoldPerRoad;

        /// <summary>Everything that must hold before the cart leaves, in the order the player should hear about it.</summary>
        public static FastTravelCheck Check(BiomeType from, BiomeType to, bool tokenIdleInCity, Func<BiomeType, bool> isVisited,
            Func<BiomeType, bool> isUnlocked, int roads, int gold)
        {
            return Check(from, to, tokenIdleInCity, isVisited, isUnlocked, roads, gold, null, false);
        }

        /// <summary>As above, plus the viagem rápida gates: the destination's boss (<see cref="IsStopOpen"/>) and no event going on.</summary>
        public static FastTravelCheck Check(BiomeType from, BiomeType to, bool tokenIdleInCity, Func<BiomeType, bool> isVisited,
            Func<BiomeType, bool> isUnlocked, int roads, int gold, Func<BiomeType, bool> isBossCleared, bool eventActive)
        {
            if (from == to)
            {
                return FastTravelCheck.SameCity;
            }

            if (eventActive)
            {
                return FastTravelCheck.EventActive;
            }

            if (!tokenIdleInCity)
            {
                return FastTravelCheck.NotInCity;
            }

            if (isVisited == null || !isVisited(to))
            {
                return FastTravelCheck.NotVisited;
            }

            if (isUnlocked != null && !isUnlocked(to))
            {
                return FastTravelCheck.Locked;
            }

            if (!IsStopOpen(to, isBossCleared))
            {
                return FastTravelCheck.BossNotDefeated;
            }

            if (roads <= 0)
            {
                return FastTravelCheck.NoRoute;
            }

            return gold < Cost(roads) ? FastTravelCheck.NotEnoughGold : FastTravelCheck.Ok;
        }
    }
}
