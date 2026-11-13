using System;
using System.Collections.Generic;
using Rapadura.Gameplay.WorldEvents;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One rope lift (up only) or chute (down only) of Sangkent's terraces, in the region's own coordinates: from <see cref="From"/> to <see cref="To"/>.</summary>
    public readonly struct SangkentTerraceLine
    {
        public readonly string Id;
        public readonly bool IsLift;
        public readonly Vector2 From;
        public readonly Vector2 To;

        public SangkentTerraceLine(string id, bool isLift, Vector2 from, Vector2 to)
        {
            Id = id;
            IsLift = isLift;
            From = from;
            To = to;
        }
    }

    /// <summary>What [E] at a terrace station does (<see cref="SangkentLiftRules.Check"/>).</summary>
    public enum TerraceCheck
    {
        /// <summary>A chute: down at once.</summary>
        Slide = 0,

        /// <summary>A lift: into the queue for the next basket.</summary>
        Queue = 1,

        /// <summary>Kustai's bell rings for the avalanche: lifts and chutes stand still.</summary>
        Avalanche = 2,

        /// <summary>The soft barrier would throw the hero back where the line arrives.</summary>
        ZoneBlocked = 3,
    }

    /// <summary>
    /// The seven terraces of Sangkent (Docs/Mapas100.md 36: "calhas: descer de carrinho é rápido, subir pede escada ou elevador de corda (fila)"),
    /// pure and server-runnable on the real clock. Two rope lifts climb the terraces north of the town straight up (the 1st terrace on the east
    /// road → the 4th at the north road's bend → the 7th in the Terraços Altos), where the north road winds round; a basket leaves every
    /// <see cref="BasketSeconds"/> (the queue: the hero waits at the foot). Two stone chutes run back down to the foot in a straight line, fast
    /// (faster still when they freeze at night). While the Avalanche of the town is warned or on (<see cref="SangkentAvalancheRules"/>), Kustai's
    /// bell stops them all: the snow lanes run beside them.
    /// <code>
    /// lifts: a basket every 16 s (same instant for everyone) · 6 m/s · up only (foot → 4th ≈ 245 m, 4th → 7th ≈ 253 m)
    /// chutes: 14 m/s, 18 m/s at night, down only (7th → foot ≈ 497 m in ≈ 35 s; the road down is ≈ 590 m, ≈ 107 s on foot)
    /// stations: reach 4 m; leaving the station empties the queue · stopped while the avalanche is warned or open
    /// </code>
    /// </summary>
    public static class SangkentLiftRules
    {
        public const float StationReach = 4f;
        public const float QueueLeash = 5f;
        public const float BasketSeconds = 16f;
        public const float LiftSpeed = 6f;
        public const float ChuteSpeed = 14f;
        public const float NightChuteSpeed = 18f;
        public const float ChuteDropSpeed = 12f;

        /// <summary>Seconds until the next basket leaves (the same for every lift and every hero: one rope, one clock).</summary>
        public static float SecondsToBasket(double unixSeconds)
        {
            double t = unixSeconds % BasketSeconds;
            if (t < 0d)
            {
                t += BasketSeconds;
            }

            return (float)(BasketSeconds - t);
        }

        /// <summary>Whether the basket a hero queued for at <paramref name="queuedAt"/> has left by <paramref name="now"/>.</summary>
        public static bool BasketLeft(double queuedAt, double now) => now >= queuedAt + SecondsToBasket(queuedAt) - 1e-6;

        /// <summary>The speed of a chute.</summary>
        public static float ChuteSpeedFor(bool night) => night ? NightChuteSpeed : ChuteSpeed;

        /// <summary>Whether Kustai's bell has the lines stopped at <paramref name="utc"/> (the avalanche warned or open).</summary>
        public static bool Stopped(DateTime utc)
        {
            RealTimeWindow window = SangkentAvalancheRules.WindowAt(utc);
            return window.IsOpen || SangkentAvalancheRules.IsWarning(window);
        }

        /// <summary>What [E] at a station of <paramref name="line"/> does.</summary>
        public static TerraceCheck Check(SangkentTerraceLine line, bool stopped, bool zonesOpen)
        {
            if (stopped)
            {
                return TerraceCheck.Avalanche;
            }

            if (!zonesOpen)
            {
                return TerraceCheck.ZoneBlocked;
            }

            return line.IsLift ? TerraceCheck.Queue : TerraceCheck.Slide;
        }

        /// <summary>The line whose station (its <see cref="SangkentTerraceLine.From"/>) is within <see cref="StationReach"/> of <paramref name="p"/>, or −1.</summary>
        public static int StationAt(IReadOnlyList<SangkentTerraceLine> lines, Vector2 p)
        {
            for (int i = 0; lines != null && i < lines.Count; i++)
            {
                if ((lines[i].From - p).sqrMagnitude <= StationReach * StationReach)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
