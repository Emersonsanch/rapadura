namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The bait on the hook of an ice-fishing cast (<see cref="IceFishingSession"/>).</summary>
    public enum IceBait
    {
        None = 0,

        /// <summary>Isca de Larva do Gelo: cheap; the fish bite sooner and the better fish come a little more often.</summary>
        Grub = 1,

        /// <summary>Isca de Prata do Kul: the silver the old spirit keeps the fish with; at the Buraco Grande it calls him (<see cref="KulLureCall"/>).</summary>
        Silver = 2,
    }

    /// <summary>What a tug on the silver lure at the Buraco Grande did (<see cref="KulLureCall.Tug"/>).</summary>
    public enum KulLureResult
    {
        /// <summary>The Velho Kul is in the world already: nothing to call.</summary>
        KulHere = 0,

        /// <summary>He was called a short while ago and is still sulking (<see cref="KulLureCall.RechargeSeconds"/>).</summary>
        Recharging = 1,

        /// <summary>One more tug, not yet enough.</summary>
        Counted = 2,

        /// <summary>The last tug: the Velho Kul comes up now.</summary>
        Called = 3,
    }

    /// <summary>
    /// The baits of Old Kul's lake (Docs/Mapas100.md 64: "iscas certas puxam o chefe"), pure and server-runnable.
    /// <code>
    /// Isca de Larva do Gelo: bite after 3.5–8 s (5–11 without); uncommon × 1.5 and rare × 1.5
    /// Isca de Prata do Kul:  rare × 2 and legendary × 2; only cast at the Buraco Grande, and only while the call is open
    /// which bait goes on the hook: at the Great Hole with the call open, silver first; otherwise the grub; never silver elsewhere
    /// </code>
    /// The bait leaves the bag when the line goes down (a scared or lost fish took it).
    /// </summary>
    public static class IceBaitRules
    {
        public const string GrubItemId = "item_isca_larva_gelo";
        public const string SilverItemId = "item_isca_prata_kul";

        public const float GrubMinBiteDelay = 3.5f;
        public const float GrubMaxBiteDelay = 8f;
        public const float GrubUncommonFactor = 1.5f;
        public const float GrubRareFactor = 1.5f;
        public const float SilverRareFactor = 2f;
        public const float SilverLegendaryFactor = 2f;

        /// <summary>The bait for a cast: silver only at the Great Hole while the Kul can be called, else the grub, else none.</summary>
        public static IceBait Choose(bool greatHole, bool callOpen, int grubs, int silvers)
        {
            if (greatHole && callOpen && silvers > 0)
            {
                return IceBait.Silver;
            }

            return grubs > 0 ? IceBait.Grub : IceBait.None;
        }

        public static string ItemOf(IceBait bait)
        {
            switch (bait)
            {
                case IceBait.Grub: return GrubItemId;
                case IceBait.Silver: return SilverItemId;
                default: return null;
            }
        }

        public static float MinBiteDelay(IceBait bait) => bait == IceBait.Grub ? GrubMinBiteDelay : IceFishingSession.MinBiteDelay;

        public static float MaxBiteDelay(IceBait bait) => bait == IceBait.Grub ? GrubMaxBiteDelay : IceFishingSession.MaxBiteDelay;

        public static float UncommonFactor(IceBait bait) => bait == IceBait.Grub ? GrubUncommonFactor : 1f;

        public static float RareFactor(IceBait bait)
        {
            switch (bait)
            {
                case IceBait.Grub: return GrubRareFactor;
                case IceBait.Silver: return SilverRareFactor;
                default: return 1f;
            }
        }

        public static float LegendaryFactor(IceBait bait) => bait == IceBait.Silver ? SilverLegendaryFactor : 1f;
    }

    /// <summary>
    /// Calling the Velho Kul with the silver lure (64: "chama-se pescando"), pure and server-runnable (unix seconds in, so a server keeps it for
    /// the whole lake). The Kul keeps his own rule: he stands at his ring and, beaten, comes back <c>RegionBossRules.RespawnSeconds</c> later.
    /// The lure never makes a second Kul and never moves his ring: it only brings the one that is away back early.
    /// <code>
    /// a tug = a fish landed on the silver lure at the Buraco Grande while the Kul is away
    /// 3 tugs within 10 min of the first → he comes up now; then 20 min of recharge in which the lure does not call
    /// </code>
    /// Only the moment of the last call is saved (an event mark), so the recharge survives a reload; the tugs of a half call do not.
    /// </summary>
    public sealed class KulLureCall
    {
        public const int TugsToCall = 3;
        public const long WindowSeconds = 600;
        public const long RechargeSeconds = 1200;
        public const long NeverCalled = long.MinValue;

        private long _firstTugAt;

        public int Tugs { get; private set; }

        public long CalledAt { get; private set; } = NeverCalled;

        /// <summary>The tugs that still count at <paramref name="now"/> (0 once the window of the first one closed).</summary>
        public int TugsAt(long now) => Tugs > 0 && now - _firstTugAt <= WindowSeconds ? Tugs : 0;

        public bool Recharging(long now) => CalledAt != NeverCalled && now >= CalledAt && now - CalledAt < RechargeSeconds;

        /// <summary>Seconds left of the recharge (0 when none).</summary>
        public long RechargeLeft(long now) => Recharging(now) ? RechargeSeconds - (now - CalledAt) : 0L;

        /// <summary>Whether the silver lure can call now: the Kul is away and the last call is not recharging.</summary>
        public bool Open(long now, bool kulAway) => kulAway && !Recharging(now);

        /// <summary>A fish landed on the silver lure at the Great Hole.</summary>
        public KulLureResult Tug(long now, bool kulAway)
        {
            if (!kulAway)
            {
                return KulLureResult.KulHere;
            }

            if (Recharging(now))
            {
                return KulLureResult.Recharging;
            }

            int counted = TugsAt(now);
            if (counted == 0)
            {
                _firstTugAt = now;
            }

            Tugs = counted + 1;
            if (Tugs < TugsToCall)
            {
                return KulLureResult.Counted;
            }

            Tugs = 0;
            CalledAt = now;
            return KulLureResult.Called;
        }

        /// <summary>Back from the save: only the moment of the last call.</summary>
        public void Restore(long calledAt)
        {
            CalledAt = calledAt;
            Tugs = 0;
        }
    }
}
