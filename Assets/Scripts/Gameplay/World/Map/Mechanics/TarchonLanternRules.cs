using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One dimmed trol standing still in the Palafitas de Tarchon, in the Delta's own coordinates (<see cref="Overworld.OverworldLayouts.TarchonNeighbours"/>).</summary>
    public readonly struct TarchonNeighbour
    {
        public readonly string Id;
        public readonly Vector2 Position;

        public TarchonNeighbour(string id, Vector2 position)
        {
            Id = id;
            Position = position;
        }
    }

    /// <summary>What [E] next to a dimmed neighbour does (<see cref="TarchonLanternRules.Check"/>).</summary>
    public enum NeighbourCheck
    {
        /// <summary>The lit lantern goes into the trol's hands and he wakes.</summary>
        Wake = 0,

        /// <summary>No lit lantern in the hero's hands: light one at the Mastro dos Fogos-Fátuos.</summary>
        NoLantern = 1,

        /// <summary>This neighbour is already awake today.</summary>
        AlreadyAwake = 2,

        TooFar = 3,
    }

    /// <summary>
    /// Despertar o vizinho of the Palafitas de Tarchon (map 33, Docs/Mapas100.md §3.3: "os trols apagados ficam parados na cidade; levar a cada
    /// um uma lanterna acesa o acorda (conta para a missão diária). A cidade fica mais cheia e alegre conforme os primos ajudam") and the swamp
    /// night round the town ("os fogos-fátuos dobram e a névoa sobe; Lanterna de Fogo-Fátuo dá visão normal"), pure and server-runnable.
    /// <code>
    /// [E] within 3.5 m of the Mastro dos Fogos-Fátuos: a lit lantern in the hero's hands for 120 s (one at a time)
    /// [E] within 2.5 m of a dimmed neighbour with a lit lantern: the lantern is his, he wakes for the rest of the game day (a daily mark)
    /// all five awake on one day: Thanchvil's purse (120 gold), once per game day
    /// night inside TarchonNightBounds: the lilac mist cuts the sight to 18 m; a lit lantern in the hands, or the Lanterna de Fogo-Fátuo in the bag, gives the full sight back
    /// </code>
    /// Decision (2026-11-23): the neighbours dim again every night of staring at the wisps, so the wake is daily (the sheet's "missão diária"),
    /// and the town counts every neighbour ever woken (<see cref="TotalKey"/>) for the cousins' lines. The lantern is the mast's own light, held in
    /// the hands, not a bag item: the shop's Lanterna de Fogo-Fátuo is still to come. Online the server keeps the marks per player.
    /// </summary>
    public static class TarchonLanternRules
    {
        public const int NeighbourCount = 5;
        public const float MastReach = 3.5f;
        public const float NeighbourReach = 2.5f;
        public const float LanternSeconds = 120f;
        public const int AllAwakePurse = 120;

        /// <summary>The Lanterna de Fogo-Fátuo of Larthia's Cesto de Lanternas: in the bag, the swamp night's mist hides nothing (it does not wake a neighbour: that is the mast's own light).</summary>
        public const string WispLanternItem = "item_lanterna_fogo_fatuo";

        /// <summary>How far the hero sees in the swamp night without a lantern (metres).</summary>
        public const float NightSight = 18f;

        /// <summary>Daily mark (the game day he woke) of the neighbour <paramref name="id"/>.</summary>
        public static string MarkKey(string id) => "tarchon.neighbour." + id;

        /// <summary>Daily mark of Thanchvil's purse for the five.</summary>
        public const string PurseKey = "tarchon.neighbours.purse";

        /// <summary>Time mark counting every neighbour ever woken by this save.</summary>
        public const string TotalKey = "tarchon.neighbours.total";

        public static bool AwakeToday(int markedDay, int today) => markedDay > 0 && markedDay == today;

        /// <summary>Whether a lantern lit at <paramref name="litAt"/> (seconds, −∞ = none) still burns at <paramref name="now"/>.</summary>
        public static bool LanternLit(float litAt, float now) => now >= litAt && now - litAt < LanternSeconds;

        /// <summary>Seconds of light left (0 when out).</summary>
        public static float LanternLeft(float litAt, float now) => LanternLit(litAt, now) ? LanternSeconds - (now - litAt) : 0f;

        public static bool CanLightAtMast(float distance, bool alive) => alive && distance <= MastReach;

        public static NeighbourCheck Check(bool awake, bool lantern, float distance)
        {
            if (distance > NeighbourReach)
            {
                return NeighbourCheck.TooFar;
            }

            if (awake)
            {
                return NeighbourCheck.AlreadyAwake;
            }

            return lantern ? NeighbourCheck.Wake : NeighbourCheck.NoLantern;
        }

        /// <summary>The nearest neighbour within <paramref name="reach"/> of <paramref name="local"/>, or −1.</summary>
        public static int Nearest(IReadOnlyList<TarchonNeighbour> neighbours, Vector2 local, float reach)
        {
            int best = -1;
            float bestSq = reach * reach;
            for (int i = 0; neighbours != null && i < neighbours.Count; i++)
            {
                float sq = (neighbours[i].Position - local).sqrMagnitude;
                if (sq <= bestSq)
                {
                    bestSq = sq;
                    best = i;
                }
            }

            return best;
        }

        public static int AwakeCount(IReadOnlyList<int> markedDays, int today)
        {
            int count = 0;
            for (int i = 0; markedDays != null && i < markedDays.Count; i++)
            {
                if (AwakeToday(markedDays[i], today))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>Whether Thanchvil pays now: all five awake today and her purse not yet given today.</summary>
        public static bool PaysPurse(int awakeToday, int purseDay, int today) => awakeToday >= NeighbourCount && purseDay != today;

        /// <summary>The sight range in the swamp (<see cref="BeastSightRules.NoLimit"/> = no cut): short at night in the town's ground, unless a lantern is lit.</summary>
        public static float SightRange(bool night, bool inNightBounds, bool lantern, bool wispLanternInBag = false) =>
            night && inNightBounds && !lantern && !wispLanternInBag ? NightSight : BeastSightRules.NoLimit;
    }
}
