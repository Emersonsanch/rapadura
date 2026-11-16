using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One walk along the bottom of Kirta's reef, in the Costa's own coordinates: <see cref="Course"/> runs from the station in the town (its first
    /// point) through the air bubbles (the points between) to the station on the far side (its last point). <see cref="Abyss"/>: it goes down
    /// over the abyss south of the atoll, where the sharks come out at night.
    /// </summary>
    public readonly struct ReefDive
    {
        public readonly string Id;
        public readonly Vector2[] Course;
        public readonly bool Abyss;

        public ReefDive(string id, Vector2[] course, bool abyss)
        {
            Id = id;
            Course = course;
            Abyss = abyss;
        }

        public Vector2 Start => Course[0];

        public Vector2 End => Course[Course.Length - 1];
    }

    /// <summary>What [E] at a dive station does (<see cref="KirtaReefWalkRules.Check"/>).</summary>
    public enum ReefDiveCheck
    {
        /// <summary>Down to the bottom and along it, bubble to bubble.</summary>
        Dive = 0,

        /// <summary>A stretch between two bubbles is longer than the hero's breath.</summary>
        NoAir = 1,

        /// <summary>Night: the sharks of mud are out over the abyss.</summary>
        Sharks = 2,

        /// <summary>The soft barrier would throw the hero back where the walk comes up.</summary>
        ZoneBlocked = 3,
    }

    /// <summary>
    /// The Bolhas de Ar of Kirta (Docs/Mapas100.md 28: "Bolhas de Ar (andar no fundo)"), pure and server-runnable. Two walks along the bottom leave
    /// Kirta: north under the lagoon that walls the town in (rows of water edge to edge) and up the channel of the coral islets to their ford, and
    /// south down over the abyss to the trail of the great lagoon. Each is a chain of air bubbles rising from the coral: the hero breathes at each
    /// one, so what counts is the longest stretch between two of them against the breath the hero carries (bare lungs, Pughat's Concha
    /// Respirável, Danel's Bolha de Ar Engarrafada). At night the sharks of mud come out over the abyss and the south walk is closed.
    /// <code>
    /// breath: 6 s bare · 11 s with the Concha Respirável · 16 s with the Bolha de Ar · walking the bottom at 8 m/s (48 / 88 / 128 m)
    /// north walk: longest stretch ≈ 82 m (the shell) · south walk over the abyss: ≈ 120 m (the bubble), closed at night
    /// </code>
    /// </summary>
    public static class KirtaReefWalkRules
    {
        public const float StationReach = 4f;
        public const float DiveSpeed = 8f;

        /// <summary>Metres under the water line the hero walks where the bottom lies deeper (a head under water, the feet on the coral).</summary>
        public const float DiveDepth = 2.2f;

        public const float BareBreathSeconds = 6f;
        public const float ShellBreathSeconds = 11f;
        public const float BubbleBreathSeconds = 16f;

        /// <summary>Seconds of breath the hero carries.</summary>
        public static float BreathSeconds(bool shell, bool bubble) => bubble ? BubbleBreathSeconds : shell ? ShellBreathSeconds : BareBreathSeconds;

        /// <summary>The longest stretch between two breaths (start, bubbles, end) of a course.</summary>
        public static float LongestStretch(Vector2[] course)
        {
            float longest = 0f;
            for (int i = 0; course != null && i + 1 < course.Length; i++)
            {
                longest = Mathf.Max(longest, Vector2.Distance(course[i], course[i + 1]));
            }

            return longest;
        }

        /// <summary>Seconds of breath a course asks for.</summary>
        public static float BreathNeeded(Vector2[] course) => LongestStretch(course) / DiveSpeed;

        /// <summary>What [E] at a station of <paramref name="dive"/> does.</summary>
        public static ReefDiveCheck Check(ReefDive dive, float breathSeconds, bool night, bool zonesOpen)
        {
            if (dive.Abyss && night)
            {
                return ReefDiveCheck.Sharks;
            }

            if (BreathNeeded(dive.Course) > breathSeconds + 1e-4f)
            {
                return ReefDiveCheck.NoAir;
            }

            return zonesOpen ? ReefDiveCheck.Dive : ReefDiveCheck.ZoneBlocked;
        }

        /// <summary>
        /// The station within <see cref="StationReach"/> of <paramref name="p"/>: dive index × 2, + 1 when it is the far end (the walk back), or −1.
        /// </summary>
        public static int StationAt(IReadOnlyList<ReefDive> dives, Vector2 p)
        {
            for (int i = 0; dives != null && i < dives.Count; i++)
            {
                if ((dives[i].Start - p).sqrMagnitude <= StationReach * StationReach)
                {
                    return i * 2;
                }

                if ((dives[i].End - p).sqrMagnitude <= StationReach * StationReach)
                {
                    return i * 2 + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// How many breaths (bubbles passed, the start counting as the first) the hero has taken <paramref name="travelled"/> metres along a course
        /// walked forward or back: 1 at the start, one more at every bubble.
        /// </summary>
        public static int BreathsTaken(Vector2[] course, float travelled, bool reverse)
        {
            if (course == null || course.Length < 2)
            {
                return 0;
            }

            int breaths = 1;
            float along = 0f;
            int n = course.Length;
            for (int k = 1; k < n - 1; k++)
            {
                int a = reverse ? n - k : k - 1;
                int b = reverse ? n - k - 1 : k;
                along += Vector2.Distance(course[a], course[b]);
                if (travelled + 1e-3f >= along)
                {
                    breaths++;
                }
            }

            return breaths;
        }
    }
}
