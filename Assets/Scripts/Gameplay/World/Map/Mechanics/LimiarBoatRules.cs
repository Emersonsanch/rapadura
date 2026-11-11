using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The boats of the Lago do Limiar (Docs/Mapas100.md 19: "lago com barcos"), pure and server-runnable. The Vila das Lamparinas' boatmen keep
    /// a boat at a landing on the north shore, under the Vila, and another on the south beach, by the trail down to the Colunata; [E] at either
    /// sails the dark lake to the other. Free, always there, 3 s between rides; nothing is saved (the boat is the lake's, not the hero's).
    /// <code>
    /// 2 landings 5 m off the shore, reach 4 m · course ~43 m over the water at 6 m/s, 0.5 m over it: ~7 s
    /// walking instead: ~175 m round the west shore by the trails (~32 s at 5.5 m/s) · 3 s between rides
    /// </code>
    /// </summary>
    public static class LimiarBoatRules
    {
        public const float LandingReach = 4f;
        public const float Speed = 6f;
        public const float Hover = 0.5f;
        public const float CooldownSeconds = 3f;

        /// <summary>The landing within <see cref="LandingReach"/> of <paramref name="p"/>: 0 = the first point of the course, 1 = the last, or −1.</summary>
        public static int LandingAt(IReadOnlyList<Vector2> course, Vector2 p)
        {
            if (course == null || course.Count < 2)
            {
                return -1;
            }

            if ((course[0] - p).sqrMagnitude <= LandingReach * LandingReach)
            {
                return 0;
            }

            return (course[course.Count - 1] - p).sqrMagnitude <= LandingReach * LandingReach ? 1 : -1;
        }

        /// <summary>Whether a boat sails now: at a landing and past the rest since the last ride.</summary>
        public static bool CanSail(int landing, float sinceRide) => landing >= 0 && sinceRide >= CooldownSeconds;

        /// <summary>The course from <paramref name="landing"/> (0 sails it forward, 1 backward).</summary>
        public static Vector2[] Course(Vector2[] course, int landing)
        {
            var points = new Vector2[course.Length];
            for (int i = 0; i < course.Length; i++)
            {
                points[i] = course[landing == 0 ? i : course.Length - 1 - i];
            }

            return points;
        }

        /// <summary>Seconds of the sail.</summary>
        public static float RideSeconds(Vector2[] course) => RidePath.Length(course) / Speed;
    }
}
