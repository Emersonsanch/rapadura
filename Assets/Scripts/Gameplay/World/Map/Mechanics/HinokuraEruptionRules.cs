using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    public enum HinokuraEruptionPhase
    {
        Calm,
        Warning,
        Erupting,
    }

    /// <summary>
    /// Mount Echeyde's eruption (Docs/Mapas100.md 67), pure and server-runnable; routes are the (A, B, Width) capsules of
    /// <c>OverworldLayouts.EruptionRoutes</c>.
    /// <code>
    /// every 10 min on the clock: 30 s of the shepherds' horn (the route is shown), then 20 s of eruption down ONE route of 3, in rotation
    /// inside the route during the eruption: 4% of max health per second, never below 15%
    /// </code>
    /// The routes are 14 m wide, so a hero who ignored the whole warning is a second's walk from safety. The lava never blocks a road or
    /// holds the hero, and it never kills.
    /// </summary>
    public static class HinokuraEruptionRules
    {
        public const int Routes = 3;
        public const long CycleSeconds = 600L;
        public const long WarnSeconds = 30L;
        public const long EruptSeconds = 20L;
        public const float DamageFractionPerSecond = 0.04f;
        public const float DamageFloorFraction = 0.15f;

        private static long Cycle(long unix) => unix >= 0L ? unix / CycleSeconds : (unix - CycleSeconds + 1L) / CycleSeconds;

        private static long Second(long unix) => unix - Cycle(unix) * CycleSeconds;

        public static HinokuraEruptionPhase PhaseAt(long unix)
        {
            long second = Second(unix);
            if (second < EruptSeconds)
            {
                return HinokuraEruptionPhase.Erupting;
            }

            return second >= CycleSeconds - WarnSeconds ? HinokuraEruptionPhase.Warning : HinokuraEruptionPhase.Calm;
        }

        /// <summary>The route of the eruption running now, or of the next one (during the calm and the warning).</summary>
        public static int RouteAt(long unix)
        {
            long cycle = Cycle(unix) + (PhaseAt(unix) == HinokuraEruptionPhase.Erupting ? 0L : 1L);
            long routes = Routes;
            return (int)(((cycle % routes) + routes) % routes);
        }

        /// <summary>Seconds until the phase changes.</summary>
        public static long SecondsToChange(long unix)
        {
            long second = Second(unix);
            switch (PhaseAt(unix))
            {
                case HinokuraEruptionPhase.Erupting:
                    return EruptSeconds - second;
                case HinokuraEruptionPhase.Warning:
                    return CycleSeconds - second;
                default:
                    return CycleSeconds - WarnSeconds - second;
            }
        }

        public static bool InRoute(IReadOnlyList<(Vector2 A, Vector2 B, float Width)> routes, int route, Vector2 p)
        {
            (Vector2 a, Vector2 b, float width) = routes[route];
            Vector2 ab = b - a;
            float sqr = ab.sqrMagnitude;
            float t = sqr > 0f ? Mathf.Clamp01(Vector2.Dot(p - a, ab) / sqr) : 0f;
            return Vector2.Distance(p, a + ab * t) <= width * 0.5f;
        }

        /// <summary>Health the lava takes over <paramref name="seconds"/> from a hero inside the erupting route.</summary>
        public static float DamageAmount(HinokuraEruptionPhase phase, bool inRoute, float current, float max, float seconds)
        {
            float floor = max * DamageFloorFraction;
            if (phase != HinokuraEruptionPhase.Erupting || !inRoute || current <= floor)
            {
                return 0f;
            }

            return Mathf.Min(current - floor, max * DamageFractionPerSecond * seconds);
        }
    }
}
