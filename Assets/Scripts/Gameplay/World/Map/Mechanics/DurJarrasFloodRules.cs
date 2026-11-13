using System;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum FloodTabletPhase
    {
        /// <summary>Not washed out yet.</summary>
        Waiting,
        /// <summary>Lying on the road: [E] picks it up.</summary>
        Lying,
        /// <summary>The water took it (not picked up in time).</summary>
        Lost,
    }

    /// <summary>
    /// A Cheia of Dur-Jarras (Docs/Mapas100.md 77: "o rio sobe: salvar as tábuas da biblioteca") and the ziggurat's steps ("degraus: subir o
    /// zigurate libera serviços; do alto se vê a Roda Celeste"), pure and server-runnable on the real clock (<see cref="WorldEventCalendar"/>,
    /// Brasília). Seconds below are seconds into the window.
    /// <code>
    /// every hour at :13, open 5 min; Utnapishtim warns 60 s before (the hero within 260 m of the town)
    /// the water climbs one step of the ziggurat a minute (step 1 to 5: the market, the crafts, the library, the clock, the Tower); the flooded
    ///   ground starts 150, 120, 95, 70 and 50 m from the plaza: there the current bites 2% of max life every 2 s, never below 40% (Joaquim,
    ///   the Living Water, swims through it)
    /// five clay tablets of the library are washed out onto the town's roads (W, E, S, 42 m out) at 30, 85, 140, 195 and 250 s, each on one of
    ///   the three roads, never the same twice in a row, the order fixed by the window (<see cref="CityEventTurns"/>); a tablet left 30 s is lost
    /// [E] within 3 m of a lying tablet picks it up (any number carried); within 4.5 m of the foot of the ziggurat they go up to the library: saved
    /// paid once per window after the window: 35 gold per tablet saved, all five: +1 Pedra-Faísca; each step of the ziggurat already open adds 10%
    /// the steps: the tablets saved over all the Cheias open the ziggurat a step at a time (0, 3, 8, 15 and 25 tablets: the market is always
    ///   open); a step opening pays once 100 gold per step, and from the Tower (step 5) the Roda Celeste is seen turning in the sky
    /// </code>
    /// The roads' tablets stand 42 m from the plaza: 8 s on foot each way, and two tablets never lie at the same time.
    /// </summary>
    public static class DurJarrasFloodRules
    {
        /// <summary>Every hour at :13 (Brasília), 5 minutes: clear of the Tremor (:05–:10) and the Maré de Lodo (:20).</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(60, 13, 5);

        public const double WarningSeconds = 60d;
        public const float AlertRange = 260f;

        public const int StepCount = 5;
        public const double StepSeconds = 60d;
        private static readonly float[] FloodRadii = { 150f, 120f, 95f, 70f, 50f };

        public const float BiteEverySeconds = 2f;
        public const float BiteFraction = 0.02f;
        public const float BiteFloorFraction = 0.4f;

        public const int TabletCount = 5;
        public const int RoadCount = 3;
        public static readonly double[] TabletAt = { 30d, 85d, 140d, 195d, 250d };
        public const double TabletLifeSeconds = 30d;

        public const float PickReach = 3f;
        public const float DeliverReach = 4.5f;

        public const int GoldPerTablet = 35;
        public const int AllSavedStones = 1;
        public const float StepBonus = 0.1f;

        /// <summary>Tablets saved over all the Cheias needed to open each step (1–5); the first is always open.</summary>
        private static readonly int[] StepThresholds = { 0, 3, 8, 15, 25 };

        public const int GoldPerStepOpened = 100;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last window paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.cheia_dur_jarras.paid";

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the tablets saved over all the Cheias.</summary>
        public const string SavedTotalKey = "city_event.cheia_dur_jarras.tablets";

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the highest step whose opening was paid (0: none past the market).</summary>
        public const string StepPaidKey = "city_event.cheia_dur_jarras.step";

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        public static bool IsWarning(RealTimeWindow window) => !window.IsOpen && window.SecondsToChange <= WarningSeconds;

        public static double SecondsInto(RealTimeWindow window, DateTime utc) => (utc - window.StartUtc).TotalSeconds;

        public static long PaidMark(long windowIndex) => windowIndex + 1L;

        public static bool IsPaid(long mark, long windowIndex) => mark == PaidMark(windowIndex);

        /// <summary>The step the water has reached (1–5) <paramref name="secondsIn"/> into the window.</summary>
        public static int StepAt(double secondsIn) => Mathf.Clamp(1 + (int)Math.Floor(Math.Max(0d, secondsIn) / StepSeconds), 1, StepCount);

        /// <summary>Where the flooded ground starts (m from the plaza) at step <paramref name="step"/>.</summary>
        public static float FloodRadius(int step) => FloodRadii[Mathf.Clamp(step, 1, StepCount) - 1];

        /// <summary>The current bites: out in the flood, unless the hero is Joaquim (the Living Water).</summary>
        public static bool Bites(float distanceFromPlaza, int step, bool livingWater) => !livingWater && distanceFromPlaza > FloodRadius(step);

        public static float Bite(float current, float max) => ProfundoHazardRules.Bite(current, max, BiteFraction, BiteFloorFraction);

        /// <summary>The road (0 W, 1 E, 2 S) tablet <paramref name="tablet"/> of window <paramref name="windowIndex"/> lies on (salted apart from the other events).</summary>
        public static int RoadOf(long windowIndex, int tablet) => CityEventTurns.SideOf(windowIndex + 3L, tablet, RoadCount);

        public static FloodTabletPhase PhaseOf(int tablet, double secondsIn)
        {
            if (tablet < 0 || tablet >= TabletCount || secondsIn < TabletAt[tablet])
            {
                return FloodTabletPhase.Waiting;
            }

            return secondsIn < TabletAt[tablet] + TabletLifeSeconds ? FloodTabletPhase.Lying : FloodTabletPhase.Lost;
        }

        /// <summary>The tablet lying now, or −1 (two never lie at the same time).</summary>
        public static int LyingTablet(double secondsIn)
        {
            for (int i = 0; i < TabletCount; i++)
            {
                if (PhaseOf(i, secondsIn) == FloodTabletPhase.Lying)
                {
                    return i;
                }
            }

            return -1;
        }

        public static double SecondsLeft(int tablet, double secondsIn) =>
            tablet < 0 || tablet >= TabletCount ? 0d : Math.Max(0d, TabletAt[tablet] + TabletLifeSeconds - secondsIn);

        public static bool CanPick(Vector2 tablet, Vector2 hero) => Vector2.Distance(tablet, hero) <= PickReach;

        public static bool CanDeliver(Vector2 ziggurat, Vector2 hero) => Vector2.Distance(ziggurat, hero) <= DeliverReach;

        /// <summary>The window is over: what was not brought up is lost, the window can be paid.</summary>
        public static bool AllDone(double secondsIn) => secondsIn >= Schedule.WindowMinutes * 60d;

        /// <summary>How many steps of the ziggurat stand open (1–5) for <paramref name="savedTotal"/> tablets saved over all the Cheias.</summary>
        public static int StepsOpen(long savedTotal)
        {
            int open = 0;
            for (int i = 0; i < StepCount; i++)
            {
                if (savedTotal >= StepThresholds[i])
                {
                    open = i + 1;
                }
            }

            return open;
        }

        /// <summary>Tablets needed for step <paramref name="step"/> (1–5).</summary>
        public static int TabletsForStep(int step) => StepThresholds[Mathf.Clamp(step, 1, StepCount) - 1];

        /// <summary>Tablets still to save for the next step, or 0 when the Tower is open.</summary>
        public static int TabletsToNextStep(long savedTotal)
        {
            int open = StepsOpen(savedTotal);
            return open >= StepCount ? 0 : (int)Math.Max(0L, StepThresholds[open] - savedTotal);
        }

        public static bool TowerOpen(long savedTotal) => StepsOpen(savedTotal) >= StepCount;

        /// <summary>The gold of a window: 35 per tablet, +10% for each step open past the market.</summary>
        public static int GoldFor(int saved, int stepsOpen)
        {
            int baseGold = Mathf.Clamp(saved, 0, TabletCount) * GoldPerTablet;
            float bonus = 1f + StepBonus * Mathf.Clamp(stepsOpen - 1, 0, StepCount - 1);
            return Mathf.RoundToInt(baseGold * bonus);
        }

        public static int StonesFor(int saved) => saved >= TabletCount ? AllSavedStones : 0;

        /// <summary>The one-time gold of a step opening.</summary>
        public static int GoldForStep(int step) => Mathf.Clamp(step, 0, StepCount) * GoldPerStepOpened;
    }
}
