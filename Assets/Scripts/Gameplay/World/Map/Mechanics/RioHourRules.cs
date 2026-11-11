using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What a hero at a pier of the hours can do now (<see cref="RioHourRules.Check"/>).</summary>
    public enum RioBarqueCheck
    {
        /// <summary>The barque lies at this pier and its destination lets the hero in.</summary>
        Board = 0,
        /// <summary>The barque is at another pier.</summary>
        PierClosed = 1,
        /// <summary>The soft barrier would throw the hero back at the destination.</summary>
        ZoneBlocked = 2,
    }

    /// <summary>
    /// The hours of the barque on the river of the night (Docs/Mapas100.md, map 71), pure in the server's clock (Unix seconds)
    /// so every player online sees the same barque. Twelve piers line the south bank; the Mesektet lies at one per "hour" and
    /// sails for one of the posts of maps 72–76, which come up in sequence. Every post has a return berth that is always
    /// served, and every post is also on the roads, so a missed barque is a wait or a walk, never a dead end.
    /// <code>
    /// hour 180 s (the sheet's 12 min would make a post wait almost an hour) · 12 piers, 36 min the whole river
    /// destination = pier mod 5 (Casa de Junco, Trenó de Areia, Anel Quieto, Ilha das Palmeiras, Proa Encalhada)
    /// a given post comes up at least once every 7 hours (21 min) · boarding 2.5 s · pier reach 6 m
    /// </code>
    /// </summary>
    public static class RioHourRules
    {
        public const int Piers = 12;
        public const int HourSeconds = 180;
        public const int Destinations = 5;
        public const float BoardingSeconds = 2.5f;
        public const float PierReach = 6f;

        /// <summary>Height (m) of the bob while boarding.</summary>
        public const float BoardingArc = 0.5f;

        /// <summary>The hour (0–11) at <paramref name="unixSeconds"/>; which is also the pier the barque lies at.</summary>
        public static int HourIndex(long unixSeconds)
        {
            long hour = FloorDiv(unixSeconds, HourSeconds) % Piers;
            return (int)(hour < 0 ? hour + Piers : hour);
        }

        /// <summary>Seconds (1–180) until the barque moves to the next pier.</summary>
        public static int SecondsToNextHour(long unixSeconds)
        {
            long into = unixSeconds - FloorDiv(unixSeconds, HourSeconds) * HourSeconds;
            return HourSeconds - (int)into;
        }

        public static bool IsOpen(int pier, long unixSeconds) => pier == HourIndex(unixSeconds);

        /// <summary>Seconds until the barque reaches <paramref name="pier"/>: 0 while it lies there.</summary>
        public static int SecondsUntilOpen(int pier, long unixSeconds)
        {
            if (IsOpen(pier, unixSeconds))
            {
                return 0;
            }

            int hoursAhead = (pier - HourIndex(unixSeconds) + Piers) % Piers;
            return SecondsToNextHour(unixSeconds) + (hoursAhead - 1) * HourSeconds;
        }

        /// <summary>Which post (0–4, north along the river) the barque of <paramref name="pier"/> sails for.</summary>
        public static int DestinationOf(int pier) => ((pier % Destinations) + Destinations) % Destinations;

        public static RioBarqueCheck Check(int pier, long unixSeconds, bool destinationOpen)
        {
            if (!IsOpen(pier, unixSeconds))
            {
                return RioBarqueCheck.PierClosed;
            }

            return destinationOpen ? RioBarqueCheck.Board : RioBarqueCheck.ZoneBlocked;
        }

        /// <summary>The nearest point of <paramref name="points"/> within <see cref="PierReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int NearestWithinReach(IReadOnlyList<Vector2> points, Vector2 p)
        {
            int best = -1;
            float bestSqr = PierReach * PierReach;
            for (int i = 0; i < points.Count; i++)
            {
                float sqr = (points[i] - p).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>"m:ss", for the texts.</summary>
        public static string Clock(int seconds)
        {
            int s = Mathf.Max(0, seconds);
            return (s / 60) + ":" + (s % 60).ToString("00");
        }

        private static long FloorDiv(long a, long b)
        {
            long q = a / b;
            return a % b != 0 && (a < 0) != (b < 0) ? q - 1 : q;
        }
    }
}
