using System;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum CaravanGiftPhase
    {
        /// <summary>Still in Ammo's caravan.</summary>
        Waiting,
        /// <summary>Unloaded: [E] at the caravan takes it, and the Barque of Light of its pier waits for it.</summary>
        Ready,
        /// <summary>The Barque of Light of its pier went down without it.</summary>
        Gone,
    }

    /// <summary>
    /// A Chegada da Caravana of the Cais da Coluna (Docs/Mapas100.md 91: "a última caravana do Ammo: trazer presentes para os NPCs de todas as
    /// cidades"), pure and server-runnable on the real clock (<see cref="WorldEventCalendar"/>, Brasília). Seconds below are seconds into the
    /// window.
    /// <code>
    /// every hour at :57, open 5 min; Adda sees the caravan's lights 60 s before (the hero within 260 m of the quay)
    /// Ammo's last caravan moors in the plaza and unloads six gifts for the people of the cities down on the island, at 30, 75, 120, 165, 210
    ///   and 255 s; each is called to one of the three piers of the Barques of Light on the quay's roads (W, E, S, 42 m out), never the same
    ///   pier twice in a row, the order fixed by the window (<see cref="CityEventTurns"/>)
    /// [E] within 3.5 m of the caravan takes the gift; within 3.5 m of its pier (Lavine, the Moon Boatwoman, from 6 m) it goes aboard
    /// the barque of that pier waits 25 s after the gift is unloaded, then goes down to the cities with it or without it
    /// paid once per window after the last barque: 30 gold per gift delivered, all six: +1 Pedra-Faísca
    /// </code>
    /// Caravan to pier is 43–47 m: 9 s on foot against 25 s; the low gravity of the quay (<see cref="CeuLowGravityRules"/>) does not change the walk.
    /// </summary>
    public static class CaisCaravanRules
    {
        /// <summary>Every hour at :57 (Brasília), 5 minutes (to :02): clear of the Tremor (:05) and the Vendaval (:50–:55).</summary>
        public static readonly RealTimeSchedule Schedule = new RealTimeSchedule(60, 57, 5);

        public const double WarningSeconds = 60d;
        public const float AlertRange = 260f;
        public const int PierCount = 3;
        public const int GiftCount = 6;

        public static readonly double[] GiftAt = { 30d, 75d, 120d, 165d, 210d, 255d };
        public const double BarqueWaitSeconds = 25d;

        public const float TakeReach = 3.5f;
        public const float BoardReach = 3.5f;
        public const float MoonBoatwomanReach = 6f;

        public const int GoldPerGift = 30;
        public const int AllDeliveredStones = 1;

        /// <summary>Save key (<see cref="WorldMapProgress"/> time mark): the last window paid, as <see cref="PaidMark"/>.</summary>
        public const string PaidKey = "city_event.chegada_caravana.paid";

        public static RealTimeWindow WindowAt(DateTime utc) => WorldEventCalendar.WindowAt(Schedule, utc);

        public static bool IsWarning(RealTimeWindow window) => !window.IsOpen && window.SecondsToChange <= WarningSeconds;

        public static double SecondsInto(RealTimeWindow window, DateTime utc) => (utc - window.StartUtc).TotalSeconds;

        public static long PaidMark(long windowIndex) => windowIndex + 1L;

        public static bool IsPaid(long mark, long windowIndex) => mark == PaidMark(windowIndex);

        /// <summary>The pier (0 W, 1 E, 2 S) gift <paramref name="gift"/> of window <paramref name="windowIndex"/> is called to (salted apart from the other events).</summary>
        public static int PierOf(long windowIndex, int gift) => CityEventTurns.SideOf(windowIndex + 4L, gift, PierCount);

        public static CaravanGiftPhase PhaseOf(int gift, double secondsIn)
        {
            if (gift < 0 || gift >= GiftCount || secondsIn < GiftAt[gift])
            {
                return CaravanGiftPhase.Waiting;
            }

            return secondsIn < GiftAt[gift] + BarqueWaitSeconds ? CaravanGiftPhase.Ready : CaravanGiftPhase.Gone;
        }

        /// <summary>The gift whose barque waits now, or −1 between gifts (two barques never wait at the same time).</summary>
        public static int ReadyGift(double secondsIn)
        {
            for (int i = 0; i < GiftCount; i++)
            {
                if (PhaseOf(i, secondsIn) == CaravanGiftPhase.Ready)
                {
                    return i;
                }
            }

            return -1;
        }

        public static double SecondsLeft(int gift, double secondsIn) =>
            gift < 0 || gift >= GiftCount ? 0d : Math.Max(0d, GiftAt[gift] + BarqueWaitSeconds - secondsIn);

        /// <summary>The last barque went down: the window's result can be paid.</summary>
        public static bool AllDone(double secondsIn) => secondsIn >= GiftAt[GiftCount - 1] + BarqueWaitSeconds;

        public static bool CanTake(Vector2 caravan, Vector2 hero) => Vector2.Distance(caravan, hero) <= TakeReach;

        public static float ReachFor(bool moonBoatwoman) => moonBoatwoman ? MoonBoatwomanReach : BoardReach;

        public static bool CanBoard(Vector2 pier, Vector2 hero, bool moonBoatwoman) => Vector2.Distance(pier, hero) <= ReachFor(moonBoatwoman);

        public static int GoldFor(int delivered) => Mathf.Clamp(delivered, 0, GiftCount) * GoldPerGift;

        public static int StonesFor(int delivered) => delivered >= GiftCount ? AllDeliveredStones : 0;
    }
}
