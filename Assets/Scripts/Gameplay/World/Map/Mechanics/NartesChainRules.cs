using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Climbing Amirani's chain (Docs/Mapas100.md, map 52: "andar nos elos (estreitos, com vento); cair leva ao elo de baixo (não
    /// ao vale)"), pure and on the shared clock. The links are a narrow lane 6 m wide; every 40 m an iron ring (an anchor) gives
    /// shelter. Gusts cross the chain from alternating sides: stand at a ring while one blows, or be walked off the links. Being
    /// blown off drops the hero to the ring just below (never further), for a small hit that cannot kill. Stepping off without a
    /// gust costs nothing, so the chain never holds anyone.
    /// <code>
    /// cycle 16 s = 10.5 s calm + 3 s warning + 2.5 s blowing · sides alternate each cycle
    /// push 3.5 m/s across the links (run is 5.5 m/s: walking against it holds) = 8.75 m over a gust, past the 3 m half lane
    /// rings every 40 m (~7 s at a run) · shelter 5 m round a ring
    /// a fall: back to the ring below · −4 % of max health, never below 1 HP · counts up to 1 s after the gust
    /// the top ring pays 150 nuggets once per UTC day
    /// </code>
    /// </summary>
    public static class NartesChainRules
    {
        public const float HalfWidth = 3f;
        public const float AnchorSpacing = 40f;
        public const float ShelterRadius = 5f;
        public const float PeriodSeconds = 16f;
        public const float WarningSeconds = 3f;
        public const float BlowSeconds = 2.5f;
        public const float PushSpeed = 3.5f;
        public const float FallHealthFraction = 0.04f;
        public const float BlownGraceSeconds = 1f;
        public const int TopPurse = 150;

        private const float CalmSeconds = PeriodSeconds - WarningSeconds - BlowSeconds;

        private static float Cycle(double time)
        {
            double t = time % PeriodSeconds;
            return (float)(t < 0d ? t + PeriodSeconds : t);
        }

        public static WindGustPhase PhaseAt(double time)
        {
            float t = Cycle(time);
            if (t < CalmSeconds)
            {
                return WindGustPhase.Calm;
            }

            return t < CalmSeconds + WarningSeconds ? WindGustPhase.Warning : WindGustPhase.Blowing;
        }

        /// <summary>The side the gust of this cycle pushes to: +1 along the chain's left normal, −1 opposite; alternates.</summary>
        public static int GustSide(double time)
        {
            long cycle = (long)System.Math.Floor(time / PeriodSeconds);
            return cycle % 2L == 0L ? 1 : -1;
        }

        public static float Length(IReadOnlyList<Vector2> line)
        {
            float total = 0f;
            for (int i = 1; i < line.Count; i++)
            {
                total += Vector2.Distance(line[i - 1], line[i]);
            }

            return total;
        }

        /// <summary>The point <paramref name="along"/> metres up the line (clamped to its ends).</summary>
        public static Vector2 PointAt(IReadOnlyList<Vector2> line, float along)
        {
            float left = Mathf.Max(0f, along);
            for (int i = 1; i < line.Count; i++)
            {
                float length = Vector2.Distance(line[i - 1], line[i]);
                if (left <= length)
                {
                    return Vector2.Lerp(line[i - 1], line[i], length > 0f ? left / length : 0f);
                }

                left -= length;
            }

            return line[line.Count - 1];
        }

        /// <summary>The rings: foot, then every <see cref="AnchorSpacing"/> m, and the top end (a last short span under 10 m is merged into it).</summary>
        public static Vector2[] Anchors(IReadOnlyList<Vector2> line)
        {
            float length = Length(line);
            int spans = Mathf.Max(1, Mathf.FloorToInt((length - 10f) / AnchorSpacing) + 1);
            var anchors = new Vector2[spans + 1];
            for (int i = 0; i < spans; i++)
            {
                anchors[i] = PointAt(line, i * AnchorSpacing);
            }

            anchors[spans] = line[line.Count - 1];
            return anchors;
        }

        /// <summary>Where <paramref name="p"/> falls on the line: metres up it, distance from it, and the unit left normal there.</summary>
        public static void Project(IReadOnlyList<Vector2> line, Vector2 p, out float along, out float distance, out Vector2 across)
        {
            along = 0f;
            distance = float.MaxValue;
            across = Vector2.up;
            float walked = 0f;
            for (int i = 1; i < line.Count; i++)
            {
                Vector2 segment = line[i] - line[i - 1];
                float length = segment.magnitude;
                if (length <= 0f)
                {
                    continue;
                }

                Vector2 direction = segment / length;
                float at = Mathf.Clamp(Vector2.Dot(p - line[i - 1], direction), 0f, length);
                float d = Vector2.Distance(p, line[i - 1] + direction * at);
                if (d < distance)
                {
                    distance = d;
                    along = walked + at;
                    across = new Vector2(-direction.y, direction.x);
                }

                walked += length;
            }
        }

        public static bool IsSheltered(IReadOnlyList<Vector2> anchors, Vector2 p)
        {
            for (int i = 0; i < anchors.Count; i++)
            {
                if ((anchors[i] - p).sqrMagnitude <= ShelterRadius * ShelterRadius)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The hit of a fall: never lethal (the hero keeps at least 1 HP, and a hero at 1 HP takes nothing).</summary>
        public static float FallDamage(float currentHealth, float maxHealth) => Mathf.Max(0f, Mathf.Min(maxHealth * FallHealthFraction, currentHealth - 1f));
    }

    /// <summary>One climb up the chain: which ring the hero would drop to, and the highest one reached.</summary>
    public sealed class NartesChainClimb
    {
        private readonly int _rings;
        private readonly float _length;

        public NartesChainClimb(int rings, float length, float startAlong)
        {
            _rings = rings;
            _length = length;
            Reach(startAlong);
        }

        /// <summary>The ring just below the hero (a fall lands here).</summary>
        public int SafeAnchor { get; private set; }

        public int BestAnchor { get; private set; }

        public bool ReachedTop => BestAnchor >= _rings - 1;

        public void Reach(float along)
        {
            int ring = along >= _length - NartesChainRules.ShelterRadius
                ? _rings - 1
                : Mathf.Clamp(Mathf.FloorToInt((along + NartesChainRules.ShelterRadius) / NartesChainRules.AnchorSpacing), 0, _rings - 2);
            SafeAnchor = ring;
            BestAnchor = Mathf.Max(BestAnchor, ring);
        }
    }
}
