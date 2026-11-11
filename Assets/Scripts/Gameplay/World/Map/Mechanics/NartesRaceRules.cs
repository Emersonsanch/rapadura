using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum NartesRaceState
    {
        Countdown = 0,
        Running = 1,
        Finished = 2,
        TimedOut = 3,
        Abandoned = 4,
    }

    public enum NartesRaceMedal
    {
        None = 0,
        Bronze = 1,
        Silver = 2,
        Gold = 3,
    }

    /// <summary>
    /// A steppe race with the centaurs on the Pistas de Corrida (Docs/Mapas100.md, map 47: "corrida de estepe com os centauros
    /// (prêmios de sela)"), pure: position and time in, gates and medal out, so the online server can referee it. The hero keeps
    /// every control and may just ride away: the race ends by itself at the time limit or when the hero strays too far.
    /// <code>
    /// course: ~1035 m, 6 gates and back to the start (<c>OverworldLayouts.NartesRaceCourse</c>) · gates are taken in order, 9 m radius
    /// 3 s countdown · limit 300 s · straying 500 m from the next gate abandons
    /// run 5.5 m/s ≈ 190–205 s (bronze) · mount ×1.3–1.5 ≈ 130–160 s (silver, gold for a clean ride)
    /// gold ≤ 150 s · silver ≤ 185 s · bronze ≤ 225 s · slower: finishes, no medal
    /// purse: gold 60 · silver 35 · bronze 20 nuggets; the first medal of the UTC day pays × 3
    /// </code>
    /// </summary>
    public sealed class NartesRaceSession
    {
        private readonly Vector2[] _course;
        private float _countdown = NartesRaceRules.CountdownSeconds;

        public NartesRaceSession(Vector2[] course)
        {
            _course = course;
        }

        public NartesRaceState State { get; private set; } = NartesRaceState.Countdown;

        /// <summary>Seconds on the clock (from the end of the countdown).</summary>
        public float Elapsed { get; private set; }

        /// <summary>Index in the course of the gate to take next.</summary>
        public int NextGate { get; private set; }

        public int GateCount => _course.Length;

        public bool IsOver => State == NartesRaceState.Finished || State == NartesRaceState.TimedOut || State == NartesRaceState.Abandoned;

        public Vector2 NextGatePosition => _course[Mathf.Min(NextGate, _course.Length - 1)];

        /// <summary>Whole seconds left of the countdown (shown as 3, 2, 1).</summary>
        public int CountdownLeft => Mathf.CeilToInt(_countdown);

        /// <summary>Advances the race; true when this tick took a gate.</summary>
        public bool Tick(float deltaTime, Vector2 hero)
        {
            if (IsOver)
            {
                return false;
            }

            if (State == NartesRaceState.Countdown)
            {
                _countdown -= deltaTime;
                if (_countdown <= 0f)
                {
                    State = NartesRaceState.Running;
                }

                return false;
            }

            Elapsed += deltaTime;
            if (Elapsed > NartesRaceRules.TimeLimitSeconds)
            {
                State = NartesRaceState.TimedOut;
                return false;
            }

            float distance = Vector2.Distance(hero, NextGatePosition);
            if (distance > NartesRaceRules.LeashMetres)
            {
                State = NartesRaceState.Abandoned;
                return false;
            }

            if (distance > NartesRaceRules.GateRadius)
            {
                return false;
            }

            NextGate++;
            if (NextGate >= _course.Length)
            {
                State = NartesRaceState.Finished;
            }

            return true;
        }

        /// <summary>Called when the hero dies or leaves the region: the race just ends.</summary>
        public void Abandon()
        {
            if (!IsOver)
            {
                State = NartesRaceState.Abandoned;
            }
        }
    }

    /// <summary>The numbers of <see cref="NartesRaceSession"/> (table there).</summary>
    public static class NartesRaceRules
    {
        public const float CountdownSeconds = 3f;
        public const float TimeLimitSeconds = 300f;
        public const float LeashMetres = 500f;
        public const float GateRadius = 9f;

        /// <summary>Distance from the start post at which the race can be called.</summary>
        public const float StartReach = 6f;

        public const float GoldSeconds = 150f;
        public const float SilverSeconds = 185f;
        public const float BronzeSeconds = 225f;
        public const int GoldPurse = 60;
        public const int SilverPurse = 35;
        public const int BronzePurse = 20;
        public const int FirstOfDayFactor = 3;

        public static NartesRaceMedal MedalFor(float seconds)
        {
            if (seconds <= GoldSeconds)
            {
                return NartesRaceMedal.Gold;
            }

            if (seconds <= SilverSeconds)
            {
                return NartesRaceMedal.Silver;
            }

            return seconds <= BronzeSeconds ? NartesRaceMedal.Bronze : NartesRaceMedal.None;
        }

        /// <summary>Nuggets for a medal; <paramref name="firstOfDay"/> is the first medal of this UTC day.</summary>
        public static int Purse(NartesRaceMedal medal, bool firstOfDay)
        {
            int purse;
            switch (medal)
            {
                case NartesRaceMedal.Gold: purse = GoldPurse; break;
                case NartesRaceMedal.Silver: purse = SilverPurse; break;
                case NartesRaceMedal.Bronze: purse = BronzePurse; break;
                default: return 0;
            }

            return firstOfDay ? purse * FirstOfDayFactor : purse;
        }

        /// <summary>The UTC day number of a Unix time (negative times round down).</summary>
        public static long DayOf(long unixSeconds) => (long)System.Math.Floor(unixSeconds / 86400d);
    }
}
