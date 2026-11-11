using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One long canal of the Campos de Juncos (map 72), in the region's own coordinates (<c>OverworldLayouts.RioRaftCanals</c>): the centre line
    /// of the water, west to east, and a landing on the bank past each end.
    /// </summary>
    public readonly struct RioRaftCanal
    {
        public readonly string Id;
        public readonly Vector2[] Water;
        public readonly Vector2 WestLanding;
        public readonly Vector2 EastLanding;

        public RioRaftCanal(string id, Vector2[] water, Vector2 westLanding, Vector2 eastLanding)
        {
            Id = id;
            Water = water;
            WestLanding = westLanding;
            EastLanding = eastLanding;
        }
    }

    /// <summary>What [E] at a landing does (<see cref="RioRaftRules.Check"/>).</summary>
    public enum RioRaftCheck
    {
        /// <summary>A raft is ready (built less than <see cref="RioRaftRules.RaftLifeSeconds"/> ago, or the hero rides Taweret's hippo): [E] sails.</summary>
        Ride = 0,

        /// <summary>No raft, but the hero carries the reed bundles: [E] ties a raft and sails at once.</summary>
        Build = 1,
        NeedReeds = 2,
        ZoneBlocked = 3,
        Resting = 4,
    }

    /// <summary>
    /// The rafts of the Reed Fields (Docs/Mapas100.md 72, "jangadas: colher junco e montar jangadas (2 min) abre canais longos; a Taweret
    /// Hipopotinha passa direto"), pure and server-runnable. Eight reed clumps grow on the banks of the two long canals; [E] at one cuts a bundle.
    /// At a landing at either end of a canal, three bundles tie a raft that sails the whole canal to the far landing, and the raft keeps for
    /// 2 minutes after it was tied (time enough for the way back). Riding Taweret's Pygmy Hippo (<c>mount_capybara</c>), the hero needs no raft and
    /// the mud hippos leave them alone. The bundles held are not a counter of their own: they are the clumps cut since the last raft was tied
    /// (time marks in <c>WorldMapProgress</c>), so the save keeps them and tying a raft spends them.
    /// <code>
    /// 2 canals of 840 m (z 500 and 760, x −420 to 420) · a landing 12 m past each end, reach 4 m · 8 clumps, reach 3.5 m, regrow 5 min
    /// 3 bundles = 1 raft · the raft keeps 120 s from tying · sails at 14 m/s, 0.6 m over the water: 864 m in ~62 s · 3 s between rides
    /// walking instead: ~2 km by the roads round the Casa de Junco (~6 min at 5.5 m/s), ~850 m straight through the clearings of the fields
    /// mud hippos: a bite every 5 s on the raft, 2 % of max life (3 % at night), never under 30 %; none on Taweret's hippo
    /// the splash of landing: beasts within 8 m of the landing lose 6 % of max life (a boss 1 %)
    /// </code>
    /// </summary>
    public static class RioRaftRules
    {
        public const float ReedReach = 3.5f;
        public const long ReedRegrowSeconds = 300L;
        public const int BundlesPerRaft = 3;
        public const long RaftLifeSeconds = 120L;
        public const float LandingReach = 4f;
        public const float Speed = 14f;
        public const float Hover = 0.6f;
        public const float CooldownSeconds = 3f;
        public const float BiteSeconds = 5f;
        public const float BiteFraction = 0.02f;
        public const float NightBiteFraction = 0.03f;
        public const float BiteFloorFraction = 0.3f;
        public const float SplashRadius = 8f;
        public const float SplashFraction = 0.06f;
        public const float SplashBossFraction = 0.01f;

        /// <summary>The save key of the raft (a time mark: the unix second it was tied, 0 = never).</summary>
        public const string BuiltKey = "rio.raft.built";

        /// <summary>The save key of reed clump <paramref name="clump"/> (a time mark: the unix second it was last cut, 0 = never).</summary>
        public static string ReedKey(int clump) => "rio.raft.reed." + clump;

        /// <summary>Whether a clump last cut at <paramref name="cutAt"/> has grown back by <paramref name="now"/> (unix seconds).</summary>
        public static bool ReedReady(long cutAt, long now) => cutAt <= 0L || now - cutAt >= ReedRegrowSeconds;

        /// <summary>Seconds until a clump cut at <paramref name="cutAt"/> grows back (0 when ready).</summary>
        public static long ReedSecondsLeft(long cutAt, long now) => ReedReady(cutAt, now) ? 0L : ReedRegrowSeconds - (now - cutAt);

        /// <summary>The bundles held: the clumps cut after the last raft was tied (<paramref name="builtAt"/>), at most <see cref="BundlesPerRaft"/>.</summary>
        public static int Bundles(IReadOnlyList<long> cutMarks, long builtAt)
        {
            int held = 0;
            for (int i = 0; cutMarks != null && i < cutMarks.Count; i++)
            {
                if (cutMarks[i] > 0L && cutMarks[i] > builtAt)
                {
                    held++;
                }
            }

            return Mathf.Min(held, BundlesPerRaft);
        }

        /// <summary>Whether the raft tied at <paramref name="builtAt"/> still keeps at <paramref name="now"/>.</summary>
        public static bool RaftLive(long builtAt, long now) => builtAt > 0L && now >= builtAt && now - builtAt < RaftLifeSeconds;

        /// <summary>What [E] at a landing does. <paramref name="zonesOpen"/> = the soft barrier lets the hero over every zone of the canal.</summary>
        public static RioRaftCheck Check(bool zonesOpen, bool raftLive, bool hippoMount, int bundles, float sinceRide)
        {
            if (!zonesOpen)
            {
                return RioRaftCheck.ZoneBlocked;
            }

            if (sinceRide < CooldownSeconds)
            {
                return RioRaftCheck.Resting;
            }

            if (raftLive || hippoMount)
            {
                return RioRaftCheck.Ride;
            }

            return bundles >= BundlesPerRaft ? RioRaftCheck.Build : RioRaftCheck.NeedReeds;
        }

        /// <summary>The landing within <see cref="LandingReach"/> of <paramref name="p"/>: 2·canal (west) or 2·canal + 1 (east), or −1.</summary>
        public static int LandingAt(IReadOnlyList<RioRaftCanal> canals, Vector2 p)
        {
            for (int i = 0; i < canals.Count; i++)
            {
                if ((canals[i].WestLanding - p).sqrMagnitude <= LandingReach * LandingReach)
                {
                    return 2 * i;
                }

                if ((canals[i].EastLanding - p).sqrMagnitude <= LandingReach * LandingReach)
                {
                    return 2 * i + 1;
                }
            }

            return -1;
        }

        /// <summary>The landing of a canal (<paramref name="landing"/> = 2·canal + side).</summary>
        public static Vector2 LandingPoint(IReadOnlyList<RioRaftCanal> canals, int landing)
        {
            RioRaftCanal canal = canals[landing / 2];
            return (landing & 1) == 0 ? canal.WestLanding : canal.EastLanding;
        }

        /// <summary>The landing at the far end of the same canal.</summary>
        public static int FarLanding(int landing) => landing ^ 1;

        /// <summary>The clump within <see cref="ReedReach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int ReedAt(IReadOnlyList<Vector2> clumps, Vector2 p)
        {
            int best = -1;
            float bestSq = ReedReach * ReedReach;
            for (int i = 0; i < clumps.Count; i++)
            {
                float sq = (clumps[i] - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>The raft's course from <paramref name="landing"/> (region coordinates): the landing, the canal end to end, the far landing.</summary>
        public static Vector2[] Course(IReadOnlyList<RioRaftCanal> canals, int landing)
        {
            RioRaftCanal canal = canals[landing / 2];
            bool fromWest = (landing & 1) == 0;
            int n = canal.Water.Length;
            var course = new Vector2[n + 2];
            course[0] = fromWest ? canal.WestLanding : canal.EastLanding;
            for (int i = 0; i < n; i++)
            {
                course[i + 1] = canal.Water[fromWest ? i : n - 1 - i];
            }

            course[n + 1] = fromWest ? canal.EastLanding : canal.WestLanding;
            return course;
        }

        /// <summary>Seconds of the sail from <paramref name="landing"/>.</summary>
        public static float RideSeconds(IReadOnlyList<RioRaftCanal> canals, int landing) => RidePath.Length(Course(canals, landing)) / Speed;

        /// <summary>How many hippo bites are due after <paramref name="elapsed"/> seconds on the water (the first at <see cref="BiteSeconds"/>).</summary>
        public static int BitesDue(float elapsed) => Mathf.FloorToInt(Mathf.Max(0f, elapsed) / BiteSeconds);

        /// <summary>One bite of a mud hippo: a slice of max life that never takes the hero under <see cref="BiteFloorFraction"/>.</summary>
        public static float BiteAmount(float currentHealth, float maxHealth, bool night) =>
            ProfundoHazardRules.Bite(currentHealth, maxHealth, night ? NightBiteFraction : BiteFraction, BiteFloorFraction);

        /// <summary>The splash of landing on a beast within <see cref="SplashRadius"/>.</summary>
        public static float SplashAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? SplashBossFraction : SplashFraction);
    }
}
