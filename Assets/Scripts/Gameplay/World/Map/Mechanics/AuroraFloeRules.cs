using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What [E] at an end of the floe chain does (<see cref="AuroraFloeRules.Check"/>).</summary>
    public enum FloeCheck
    {
        /// <summary>The floes lie in a line: hop across.</summary>
        Hop = 0,

        /// <summary>The floes drift apart: wait for the aurora's dance (or the night).</summary>
        Drifting = 1,

        /// <summary>The soft barrier would throw the hero back on the other side.</summary>
        ZoneBlocked = 2,
    }

    /// <summary>
    /// The floes of the Mar das Auroras (Docs/Mapas100.md 63: "pular placas: as placas se movem; quando a aurora dança (a cada 5 min), ela ilumina
    /// a rota segura"; "Noite: a aurora fica no céu a noite toda; as placas param"), pure and server-runnable on the real clock. A chain of floes
    /// runs straight over the open sea from the north road (west of the Aurora Presa) to the lighthouse road; each drifts to and fro across the
    /// line, each at its own pace, so in the day the chain is broken. Every 5 min the aurora dances for 50 s: the floes swing into line under it
    /// and the safe way is lit; at night the aurora stays up and the floes stand still in line. The hop over the chain is far shorter than the
    /// road round by the sea fork.
    /// <code>
    /// dance: the first 50 s of every 5 min (UTC) · night: always in line · drift: up to 7 m across, periods 17–38 s, eased in/out over 6 s
    /// hop: 9 m/s, 1.2 m arcs over each floe · reach 4 m at either end
    /// </code>
    /// </summary>
    public static class AuroraFloeRules
    {
        public const double DanceEvery = 300d;
        public const double DanceSeconds = 50d;
        public const float DriftAmplitude = 7f;
        public const float EaseSeconds = 6f;
        public const float StationReach = 4f;
        public const float HopSpeed = 9f;
        public const float HopArc = 1.2f;

        /// <summary>Whether the aurora dances at <paramref name="unixSeconds"/> (the first <see cref="DanceSeconds"/> of every <see cref="DanceEvery"/>).</summary>
        public static bool Dancing(double unixSeconds) => Phase(unixSeconds) < DanceSeconds;

        /// <summary>Seconds until the next dance begins (0 while it dances).</summary>
        public static float SecondsToDance(double unixSeconds)
        {
            double phase = Phase(unixSeconds);
            return phase < DanceSeconds ? 0f : (float)(DanceEvery - phase);
        }

        /// <summary>Seconds of the dance still to come (0 when it does not dance).</summary>
        public static float DanceLeft(double unixSeconds)
        {
            double phase = Phase(unixSeconds);
            return phase < DanceSeconds ? (float)(DanceSeconds - phase) : 0f;
        }

        /// <summary>Whether the chain lies in line now: the aurora dances, or it is night.</summary>
        public static bool InLine(double unixSeconds, bool night) => night || Dancing(unixSeconds);

        /// <summary>
        /// Metres floe <paramref name="floe"/> stands off the line (to the line's right) at <paramref name="unixSeconds"/>: 0 while in line, swinging
        /// in and out over <see cref="EaseSeconds"/> at the dance's edges.
        /// </summary>
        public static float Drift(int floe, double unixSeconds, bool night)
        {
            if (night)
            {
                return 0f;
            }

            double phase = Phase(unixSeconds);
            if (phase < DanceSeconds)
            {
                return 0f;
            }

            float ease = Mathf.Clamp01((float)Math.Min(phase - DanceSeconds, DanceEvery - phase) / EaseSeconds);
            double period = 17d + (floe * 7 % 22);
            double angle = 2d * Math.PI * (unixSeconds % (period * 1000d)) / period + floe * 1.7d;
            return DriftAmplitude * ease * (float)Math.Sin(angle);
        }

        /// <summary>What [E] at an end of the chain does.</summary>
        public static FloeCheck Check(double unixSeconds, bool night, bool zonesOpen)
        {
            if (!InLine(unixSeconds, night))
            {
                return FloeCheck.Drifting;
            }

            return zonesOpen ? FloeCheck.Hop : FloeCheck.ZoneBlocked;
        }

        /// <summary>0 at the west end of <paramref name="chain"/>, 1 at its east end, −1 when <paramref name="p"/> is at neither.</summary>
        public static int EndAt(Vector2[] chain, Vector2 p)
        {
            if (chain == null || chain.Length < 2)
            {
                return -1;
            }

            if ((chain[0] - p).sqrMagnitude <= StationReach * StationReach)
            {
                return 0;
            }

            return (chain[chain.Length - 1] - p).sqrMagnitude <= StationReach * StationReach ? 1 : -1;
        }

        private static double Phase(double unixSeconds)
        {
            double phase = unixSeconds % DanceEvery;
            return phase < 0d ? phase + DanceEvery : phase;
        }
    }
}
