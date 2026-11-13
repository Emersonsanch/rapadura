using System;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// A lava channel at one of Hinokura's gates (<c>OverworldLayouts.HinokuraLavaDiverters</c>): where the surge comes from (the mouth, out on the
    /// road), the diverter stone that turns it into the forge's channel, and where it spills over the road when the stone was not turned.
    /// </summary>
    public readonly struct HinokuraLavaDiverter
    {
        public readonly Vector2 Mouth;
        public readonly Vector2 Stone;
        public readonly Vector2 Spill;

        public HinokuraLavaDiverter(Vector2 mouth, Vector2 stone, Vector2 spill)
        {
            Mouth = mouth;
            Stone = stone;
            Spill = spill;
        }
    }

    public enum TremorSurgePhase
    {
        Quiet,
        Warning,
        Surging,
        Done,
    }

    /// <summary>
    /// The Grande Tremor of Hinokura (Docs/Mapas100.md 65: "o vulcão treme e os canais mudam: guiar a lava para as forjas"), pure and
    /// server-runnable on the real clock (<see cref="WorldEventCalendar"/>, Brasília). Seconds below are seconds into the window. The small Tremor
    /// of the sluices (<see cref="HinokuraForgeRules"/>, every 12 min on the clock: :00, :12, …) never falls inside it.
    /// <code>
    /// every hour at :05, open 5 min; Kurogane's hammer warns 60 s before (the hero within 260 m of the town)
    /// four surges of lava at 40, 100, 160 and 220 s, each 6 s from its mouth on the road to the diverter stone; the channels change: each surge
    ///   comes down one of the three gates (W, E, S road), never the same twice in a row, the order fixed by the window (<see cref="CityEventTurns"/>);
    ///   the gate is called 25 s before, and the tremor that calls it throws every stone back to the street
    /// [E] within 3.5 m of the called stone turns it to the forge (Ícaro, whose element is the Living Fire, from 6 m), any time in those 25 s
    /// the surge reaches a turned stone: the forge of that gate roars (fed); a stone left to the street: the lava spills over the road beside it,
    ///   a pool of 5 m for 40 s that burns 6% of max life a second, never below 20%
    /// paid once per window after the last surge: 40 gold per forge fed, all four: +1 Pedra-Faísca
    /// </code>
    /// The stones stand 60–84 m apart round the town: 15 s on foot at most against 25 s of warning; a spill always ends before the next surge.
    /// </summary>
    public static class HinokuraGreatTremorRules
    {
        /// <summary>Every hour at :05 (Brasília), 5 minutes: clear of the sluices' Tremors at :00 (jam until :00:20) and :12 (warned at :11:30).</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(60, 5, 5);

        public const double WarningSeconds = 60d;
        public const float AlertRange = 260f;
        public const int DiverterCount = 3;
        public const int SurgeCount = 4;

        public static readonly double[] SurgeAt = { 40d, 100d, 160d, 220d };
        public const double SurgeWarnSeconds = 25d;
        public const double SurgeSeconds = 6d;

        public const float TurnReach = 3.5f;
        public const float LivingFireReach = 6f;

        public const float SpillRadius = 5f;
        public const double SpillSeconds = 40d;
        public const float BurnEverySeconds = 1f;
        public const float BurnFraction = 0.06f;
        public const float BurnFloorFraction = 0.2f;

        public const int GoldPerForge = 40;
        public const int AllFedStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last window paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.grande_tremor_hinokura.paid";

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        public static bool IsWarning(RealTimeWindow window) => !window.IsOpen && window.SecondsToChange <= WarningSeconds;

        public static double SecondsInto(RealTimeWindow window, DateTime utc) => (utc - window.StartUtc).TotalSeconds;

        public static long PaidMark(long windowIndex) => windowIndex + 1L;

        public static bool IsPaid(long mark, long windowIndex) => mark == PaidMark(windowIndex);

        /// <summary>The gate (0 W, 1 E, 2 S) surge <paramref name="surge"/> of window <paramref name="windowIndex"/> comes down (salted apart from the Vendaval's order).</summary>
        public static int ChannelOf(long windowIndex, int surge) => CityEventTurns.SideOf(windowIndex + 1L, surge, DiverterCount);

        public static TremorSurgePhase PhaseOf(int surge, double secondsIn)
        {
            if (surge < 0 || surge >= SurgeCount)
            {
                return TremorSurgePhase.Quiet;
            }

            double at = SurgeAt[surge];
            if (secondsIn >= at + SurgeSeconds)
            {
                return TremorSurgePhase.Done;
            }

            if (secondsIn >= at)
            {
                return TremorSurgePhase.Surging;
            }

            return secondsIn >= at - SurgeWarnSeconds ? TremorSurgePhase.Warning : TremorSurgePhase.Quiet;
        }

        /// <summary>The surge being called or running at <paramref name="secondsIn"/>, or −1 between surges.</summary>
        public static int CurrentSurge(double secondsIn)
        {
            for (int i = 0; i < SurgeCount; i++)
            {
                TremorSurgePhase phase = PhaseOf(i, secondsIn);
                if (phase == TremorSurgePhase.Warning || phase == TremorSurgePhase.Surging)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>How far the surge has run from its mouth (0) to the stone (1).</summary>
        public static float Progress(int surge, double secondsIn)
        {
            if (surge < 0 || surge >= SurgeCount)
            {
                return 0f;
            }

            return Mathf.Clamp01((float)((secondsIn - SurgeAt[surge]) / SurgeSeconds));
        }

        public static double SecondsToSurge(int surge, double secondsIn) => surge < 0 || surge >= SurgeCount ? 0d : Math.Max(0d, SurgeAt[surge] - secondsIn);

        /// <summary>The surge reached the stone: fed or spilled is decided.</summary>
        public static bool Arrived(int surge, double secondsIn) => surge >= 0 && surge < SurgeCount && secondsIn >= SurgeAt[surge] + SurgeSeconds;

        public static bool AllDone(double secondsIn) => Arrived(SurgeCount - 1, secondsIn);

        public static float ReachFor(bool livingFire) => livingFire ? LivingFireReach : TurnReach;

        public static bool CanTurn(Vector2 stone, Vector2 hero, bool livingFire) => Vector2.Distance(stone, hero) <= ReachFor(livingFire);

        /// <summary>A spill made by surge <paramref name="surge"/> still burns at <paramref name="secondsIn"/>.</summary>
        public static bool SpillBurning(int surge, double secondsIn)
        {
            if (!Arrived(surge, secondsIn))
            {
                return false;
            }

            return secondsIn < SurgeAt[surge] + SurgeSeconds + SpillSeconds;
        }

        public static bool InSpill(Vector2 spill, Vector2 hero) => Vector2.Distance(spill, hero) <= SpillRadius;

        public static float Burn(float current, float max) => ProfundoHazardRules.Bite(current, max, BurnFraction, BurnFloorFraction);

        public static int GoldFor(int fed) => Mathf.Clamp(fed, 0, SurgeCount) * GoldPerForge;

        public static int StonesFor(int fed) => fed >= SurgeCount ? AllFedStones : 0;
    }
}
