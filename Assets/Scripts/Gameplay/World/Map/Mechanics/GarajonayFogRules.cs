namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum GarajonayFogPhase
    {
        Clear = 0,

        /// <summary>The fog is rising from the stream beds: ten seconds to climb to a platform or close in.</summary>
        Rising = 1,

        /// <summary>The fog is up: on the forest floor no beast shows past 8 m.</summary>
        Fog = 2,
    }

    /// <summary>
    /// The fog of the Copas de Garajonay (Docs/Mapas100.md 05: "névoa: a cada 3 min a névoa sobe e esconde bichos a mais de 8 m; o Coletor de
    /// Névoa da cidade dá gotas por névoa atravessada"), pure and server-runnable (a function of the clock, the same for everyone). Every 3
    /// minutes: 2 min 15 s clear, 10 s of rising fog (the warning) and 35 s of fog. In the fog, on the forest floor, the hero sees no beast
    /// farther than 8 m (bosses show); up on a canopy platform (6 m or more above the ground) the hero is above the fog and sees as usual.
    /// A hero who spends 20 s or more of one fog in the map off the city has crossed it: the drop the leaves gave is the Coletor de Névoa's,
    /// who hands it over (an herb of the laurel forest) the next time the hero stands in the city; at most 3 drops are earned per game day and
    /// at most 3 wait for the Coletor. Both counts stay in the save.
    /// <code>
    /// cycle 180 s: 135 clear · 10 rising · 35 fog · fog: beasts past 8 m unseen on the floor, 6 m up = above the fog
    /// crossed = 20 s of a fog in the map off the city → 1 drop (≤ 3 per day, ≤ 3 waiting) · the Coletor pays them in the city
    /// </code>
    /// </summary>
    public static class GarajonayFogRules
    {
        public const double CycleSeconds = 180d;
        public const double RisingSeconds = 10d;
        public const double FogSeconds = 35d;
        public const float SightRange = 8f;
        public const float AboveFogHeight = 6f;
        public const float CrossSeconds = 20f;
        public const int DropsPerDay = 3;
        public const int MaxWaiting = 3;

        /// <summary>The second of the cycle the fog starts to rise.</summary>
        public const double RisingStart = CycleSeconds - FogSeconds - RisingSeconds;

        /// <summary>The save key of the drops earned today (a time mark holding <c>day × 10 + count</c>).</summary>
        public const string EarnedKey = "zarafshan.garajonay.mist_earned";

        /// <summary>The save key of the drops waiting for the Coletor (a time mark holding the count).</summary>
        public const string WaitingKey = "zarafshan.garajonay.mist_waiting";

        public static GarajonayFogPhase PhaseAt(double unixSeconds)
        {
            double t = unixSeconds % CycleSeconds;
            t = t < 0d ? t + CycleSeconds : t;
            return t < RisingStart ? GarajonayFogPhase.Clear : t < RisingStart + RisingSeconds ? GarajonayFogPhase.Rising : GarajonayFogPhase.Fog;
        }

        /// <summary>Which fog the clock is in (it changes at the end of each cycle).</summary>
        public static long FogIndex(double unixSeconds) => (long)System.Math.Floor(unixSeconds / CycleSeconds);

        /// <summary>The hero's sight: <see cref="SightRange"/> in the fog on the floor, no cut above it or out of the fog.</summary>
        public static float SightFor(GarajonayFogPhase phase, float heightAboveGround, bool inCity, bool alive)
        {
            return phase == GarajonayFogPhase.Fog && heightAboveGround < AboveFogHeight && !inCity && alive ? SightRange : BeastSightRules.NoLimit;
        }

        /// <summary>Whether the seconds spent in one fog make it crossed.</summary>
        public static bool Crossed(float secondsInFog) => secondsInFog >= CrossSeconds;

        /// <summary>The drops earned on <paramref name="day"/>, read from the stored <c>day × 10 + count</c> (0 on another day).</summary>
        public static int EarnedOn(long stored, int day) => stored > 0L && stored / 10L == day ? (int)(stored % 10L) : 0;

        public static long StoreEarned(int day, int count) => day * 10L + System.Math.Max(0, System.Math.Min(9, count));

        /// <summary>Whether one more crossed fog earns a drop: under the day's cap and room at the Coletor.</summary>
        public static bool CanEarn(int earnedToday, int waiting) => earnedToday < DropsPerDay && waiting < MaxWaiting;
    }
}
