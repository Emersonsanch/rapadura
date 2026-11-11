using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One spark hung over the course of the cart (map 37): <see cref="Distance"/> metres down the course, <see cref="Offset"/> metres to its right.</summary>
    public readonly struct TetoCartSpark
    {
        public readonly float Distance;
        public readonly float Offset;

        public TetoCartSpark(float distance, float offset)
        {
            Distance = distance;
            Offset = offset;
        }
    }

    /// <summary>What [E] at the top station does (<see cref="TetoCartRules.Check"/>).</summary>
    public enum TetoCartCheck
    {
        Ride = 0,

        /// <summary>The bell rings over a chute of the course: the avalanche would meet the cart.</summary>
        Avalanche = 1,
        ZoneBlocked = 2,
        Resting = 3,
    }

    /// <summary>
    /// One run of the cart, as a server would rank it (Docs/Mapas100.md 37: "ranking online"): today the save keeps the best
    /// <see cref="ScoreMs"/>; the online board will take the same struct.
    /// </summary>
    public readonly struct TetoCartRun
    {
        public readonly int Sparks;
        public readonly float Seconds;

        public TetoCartRun(int sparks, float seconds)
        {
            Sparks = sparks;
            Seconds = seconds;
        }

        /// <summary>The run's time with the sparks taken off, in milliseconds (lower is better, never under 1).</summary>
        public long ScoreMs => TetoCartRules.ScoreMs(Seconds, Sparks);
    }

    /// <summary>
    /// The timed descent of the Calhas de Pedra (Docs/Mapas100.md 37: "descer de carrinho pegando faíscas no caminho (corrida; ranking online)"),
    /// pure. At the top station, on the chute-bridge road where it crosses the east chute of the Calhas Geladas, [E] boards a cart of the
    /// Artífices that runs down that chute, over the shelf to the east chute of the Calhas de Pedra and down to its foot, beside the Encosta dos
    /// Zimbros. The cart holds the controls; left and right steer it across the chute to catch the sparks hung over the course. Every spark takes
    /// 1.5 s off the run's time; the best score stays in the save, and the first run of each UTC day pays nuggets for the sparks caught. The
    /// avalanches of the chutes (<see cref="TetoAvalancheRules"/>) run on the same clock: the station holds the cart when this cycle's slide is on a
    /// chute of the course and the run would still be on it when the bell rings. At night the chutes freeze: faster, and the sparks are harder to take.
    /// <code>
    /// course ≈ 813 m: east chute of the Calhas Geladas (286 m), the shelf (92 m), east chute of the Calhas de Pedra (434 m)
    /// 17 m/s by day (≈ 48 s), 21 m/s at night (≈ 39 s); on foot ≈ 150 s · steer ±6 m off the centre line at 8 m/s
    /// 12 sparks every 60 m from 60 m; caught within 1.8 m sideways (1.4 m at night) · −1.5 s a spark
    /// first run of the UTC day: 3 nuggets a spark (36 at most) · 4 s between runs
    /// the cart knocks a beast within 2.5 m of it once a run: 4 % of its max life (a boss 1 %)
    /// held at the station: this cycle's avalanche lane on the course and (second in cycle + run time) &gt; 60 s, before the slide ends (87 s)
    /// </code>
    /// </summary>
    public static class TetoCartRules
    {
        public const float StationReach = 4f;
        public const float DaySpeed = 17f;
        public const float NightSpeed = 21f;
        public const float Hover = 0.5f;
        public const float DropSpeed = 9f;
        public const float SteerLimit = 6f;
        public const float SteerSpeed = 8f;
        public const float CatchHalfWidth = 1.8f;
        public const float NightCatchHalfWidth = 1.4f;
        public const float SparkBonusSeconds = 1.5f;
        public const int NuggetsPerSpark = 3;
        public const float CooldownSeconds = 4f;
        public const float KnockRadius = 2.5f;
        public const float KnockFraction = 0.04f;
        public const float KnockBossFraction = 0.01f;

        /// <summary>The save key of the best score (stored in a time mark: milliseconds, 0 = never run).</summary>
        public const string BestKey = "teto.cart.best_ms";

        /// <summary>The save key of the day the run paid (a daily mark: the UTC day).</summary>
        public const string PaidKey = "teto.cart.paid";

        /// <summary>The indices (into <c>OverworldLayouts.ChuteLanes</c>) of the avalanche lanes the course runs down.</summary>
        public static readonly int[] CourseLanes = { 3, 0 };

        /// <summary>The sparks over the course: one every 60 m, at an offset that swings from side to side.</summary>
        public static readonly TetoCartSpark[] Sparks =
        {
            new TetoCartSpark(60f, -3f), new TetoCartSpark(120f, 2f), new TetoCartSpark(180f, 5f), new TetoCartSpark(240f, 0f),
            new TetoCartSpark(300f, -4.5f), new TetoCartSpark(360f, -1f), new TetoCartSpark(420f, 3.5f), new TetoCartSpark(480f, 5.5f),
            new TetoCartSpark(540f, 1f), new TetoCartSpark(600f, -3.5f), new TetoCartSpark(660f, -5.5f), new TetoCartSpark(720f, 0.5f),
        };

        public static float Speed(bool night) => night ? NightSpeed : DaySpeed;

        public static float CatchWidth(bool night) => night ? NightCatchHalfWidth : CatchHalfWidth;

        /// <summary>Seconds of the run along a course of <paramref name="length"/> metres.</summary>
        public static float RunSeconds(float length, bool night) => Mathf.Max(0f, length) / Speed(night);

        /// <summary>The new sideways offset after <paramref name="dt"/> seconds of <paramref name="steer"/> (−1 left … 1 right), inside the chute.</summary>
        public static float Steer(float offset, float steer, float dt) =>
            Mathf.Clamp(offset + Mathf.Clamp(steer, -1f, 1f) * SteerSpeed * Mathf.Max(0f, dt), -SteerLimit, SteerLimit);

        /// <summary>
        /// Whether the cart takes <paramref name="spark"/> moving from <paramref name="fromDistance"/> to <paramref name="toDistance"/> along the
        /// course at <paramref name="offset"/> metres to the right.
        /// </summary>
        public static bool Catches(TetoCartSpark spark, float fromDistance, float toDistance, float offset, bool night) =>
            spark.Distance > fromDistance && spark.Distance <= toDistance && Mathf.Abs(offset - spark.Offset) <= CatchWidth(night);

        /// <summary>A run's score in milliseconds: the time less <see cref="SparkBonusSeconds"/> a spark, never under 1.</summary>
        public static long ScoreMs(float seconds, int sparks) =>
            System.Math.Max(1L, (long)Mathf.Round((Mathf.Max(0f, seconds) - SparkBonusSeconds * Mathf.Max(0, sparks)) * 1000f));

        /// <summary>Whether <paramref name="scoreMs"/> beats the best kept (0 = none yet).</summary>
        public static bool IsRecord(long bestMs, long scoreMs) => scoreMs > 0L && (bestMs <= 0L || scoreMs < bestMs);

        /// <summary>The nuggets a run pays: only the first run of a UTC day (<paramref name="paidDay"/> = the day it last paid).</summary>
        public static int Nuggets(int sparks, int paidDay, int today) => paidDay == today ? 0 : Mathf.Max(0, sparks) * NuggetsPerSpark;

        /// <summary>
        /// Whether the avalanches let the cart go at unix time <paramref name="t"/>: false when this cycle's lane is on the course and the run
        /// (<paramref name="runSeconds"/>) would reach the bell before the slide is over.
        /// </summary>
        public static bool AvalancheAllows(double t, float runSeconds)
        {
            int lane = TetoAvalancheRules.LaneAt(t);
            if (System.Array.IndexOf(CourseLanes, lane) < 0)
            {
                return true;
            }

            double second = t - TetoAvalancheRules.CycleOf(t) * TetoAvalancheRules.CycleSeconds;
            return second >= TetoAvalancheRules.SlidingEnd || second + runSeconds <= TetoAvalancheRules.WarningStart;
        }

        /// <summary>What [E] at the station does.</summary>
        public static TetoCartCheck Check(bool zonesOpen, bool avalancheAllows, float sinceRun)
        {
            if (!zonesOpen)
            {
                return TetoCartCheck.ZoneBlocked;
            }

            if (sinceRun < CooldownSeconds)
            {
                return TetoCartCheck.Resting;
            }

            return avalancheAllows ? TetoCartCheck.Ride : TetoCartCheck.Avalanche;
        }

        /// <summary>Whether <paramref name="p"/> stands at the station <paramref name="station"/>.</summary>
        public static bool AtStation(Vector2 station, Vector2 p) => (station - p).sqrMagnitude <= StationReach * StationReach;

        public static float KnockAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? KnockBossFraction : KnockFraction);

        /// <summary>How many of <paramref name="caught"/> are true.</summary>
        public static int Count(IReadOnlyList<bool> caught)
        {
            int n = 0;
            for (int i = 0; caught != null && i < caught.Count; i++)
            {
                if (caught[i])
                {
                    n++;
                }
            }

            return n;
        }
    }
}
