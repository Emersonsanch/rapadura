using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Where a gust's cycle stands (<see cref="WindGustRules.PhaseAt"/>).</summary>
    public enum WindGustPhase
    {
        Calm = 0,
        /// <summary>The sky warns: the gust arrives in <see cref="WindGustRules.WarningSeconds"/>.</summary>
        Warning = 1,
        /// <summary>The gust is crossing: whoever is walking is pushed.</summary>
        Blowing = 2,
    }

    /// <summary>
    /// The gusts of the Picos de Farnkesh (Docs/Mapas100.md, map 55: "a cada 20 s uma rajada atravessa, com aviso no céu; quem
    /// está numa ponte para e segura"), pure and deterministic in the shared clock so the online server can run the same
    /// cycle. On a rope bridge the wind blows across the span, in the Gorge of the Ays along a fixed axis; each gust comes from
    /// the side opposite the last one. Standing still is holding on (no push at all), the Capa de Tempestade halves the push.
    /// <code>
    /// cycle 20 s = 15.5 s calm + 3 s warning + 1.5 s blowing
    /// push = 5 m/s × 1.5 s = 7.5 m walking · 3.75 m with the cloak · 0 standing still
    /// </code>
    /// </summary>
    public static class WindGustRules
    {
        public const float PeriodSeconds = 20f;
        public const float WarningSeconds = 3f;
        public const float BlowSeconds = 1.5f;

        /// <summary>Push (m/s) on a hero walking in a gust.</summary>
        public const float PushSpeed = 5f;

        /// <summary>Multiplier on the push for a hero with the Capa de Tempestade.</summary>
        public const float StormCloakFactor = 0.5f;

        /// <summary>Metres to each side of a bridge line where its gusts reach.</summary>
        public const float BridgeHalfWidth = 4.5f;

        private const float CalmSeconds = PeriodSeconds - WarningSeconds - BlowSeconds;

        /// <summary>Seconds into the current cycle (never negative, whatever <paramref name="time"/> is).</summary>
        public static float CycleTime(float time)
        {
            float t = time % PeriodSeconds;
            return t < 0f ? t + PeriodSeconds : t;
        }

        public static WindGustPhase PhaseAt(float time)
        {
            float t = CycleTime(time);
            if (t < CalmSeconds)
            {
                return WindGustPhase.Calm;
            }

            return t < CalmSeconds + WarningSeconds ? WindGustPhase.Warning : WindGustPhase.Blowing;
        }

        /// <summary>Which gust this is (0, 1, 2…): the same during its whole cycle.</summary>
        public static int GustIndex(float time) => Mathf.FloorToInt(time / PeriodSeconds);

        /// <summary><paramref name="axis"/> on even gusts, the opposite on odd ones.</summary>
        public static Vector2 Direction(int gustIndex, Vector2 axis) => (gustIndex & 1) == 0 ? axis : -axis;

        /// <summary>The velocity (m/s) a gust gives the hero: zero when holding on (standing still).</summary>
        public static Vector2 PushVelocity(Vector2 direction, bool holding, bool stormCloak)
        {
            if (holding || direction.sqrMagnitude < 1e-6f)
            {
                return Vector2.zero;
            }

            return direction.normalized * (PushSpeed * (stormCloak ? StormCloakFactor : 1f));
        }

        /// <summary>
        /// The wind's axis at <paramref name="p"/> (region coordinates): across the nearest bridge segment within
        /// <see cref="BridgeHalfWidth"/>, else <paramref name="openAxis"/> inside one of <paramref name="openAreas"/>; false out of the wind.
        /// </summary>
        public static bool TryGetAxis(Vector2 p, IReadOnlyList<Vector2[]> bridges, IReadOnlyList<Rect> openAreas, Vector2 openAxis, out Vector2 axis)
        {
            float best = BridgeHalfWidth * BridgeHalfWidth;
            bool found = false;
            axis = Vector2.zero;
            if (bridges != null)
            {
                foreach (Vector2[] line in bridges)
                {
                    for (int i = 0; i + 1 < line.Length; i++)
                    {
                        Vector2 a = line[i];
                        Vector2 span = line[i + 1] - a;
                        float t = Mathf.Clamp01(Vector2.Dot(p - a, span) / Mathf.Max(1e-4f, span.sqrMagnitude));
                        float d = (a + span * t - p).sqrMagnitude;
                        if (d <= best)
                        {
                            best = d;
                            found = true;
                            axis = new Vector2(-span.y, span.x).normalized;
                        }
                    }
                }
            }

            if (found)
            {
                return true;
            }

            if (openAreas != null)
            {
                foreach (Rect area in openAreas)
                {
                    if (area.Contains(p))
                    {
                        axis = openAxis.normalized;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
