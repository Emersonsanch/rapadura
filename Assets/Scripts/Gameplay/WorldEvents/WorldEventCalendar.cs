using System;

namespace Rapadura.Gameplay.WorldEvents
{
    /// <summary>
    /// A repeating window on the real clock: opens every <see cref="PeriodMinutes"/> starting <see cref="OffsetMinutes"/>
    /// after server-local midnight of 1970-01-01, and stays open <see cref="WindowMinutes"/>. Serializable so online the
    /// server can push its own (it owns the schedule there; clients only display it).
    /// </summary>
    [Serializable]
    public struct RealTimeSchedule
    {
        public int periodMinutes;
        public int offsetMinutes;
        public int windowMinutes;

        public RealTimeSchedule(int periodMinutes, int offsetMinutes, int windowMinutes)
        {
            this.periodMinutes = periodMinutes;
            this.offsetMinutes = offsetMinutes;
            this.windowMinutes = windowMinutes;
        }

        public int PeriodMinutes => periodMinutes;
        public int OffsetMinutes => offsetMinutes;
        public int WindowMinutes => windowMinutes;

        /// <summary>Positive period, a window shorter than it, offset inside one period.</summary>
        public bool IsValid => periodMinutes > 0 && windowMinutes > 0 && windowMinutes < periodMinutes && offsetMinutes >= 0 && offsetMinutes < periodMinutes;
    }

    /// <summary>Where one schedule stands at a real instant (all times UTC).</summary>
    public readonly struct RealTimeWindow
    {
        /// <summary>Inside the window now.</summary>
        public readonly bool IsOpen;

        /// <summary>Index of the window open now, or of the next one to open when closed (never negative).</summary>
        public readonly long Index;

        /// <summary>Start of window <see cref="Index"/> (UTC).</summary>
        public readonly DateTime StartUtc;

        /// <summary>End of window <see cref="Index"/> (UTC).</summary>
        public readonly DateTime EndUtc;

        /// <summary>Seconds until it closes (open) or opens (closed).</summary>
        public readonly double SecondsToChange;

        public RealTimeWindow(bool isOpen, long index, DateTime startUtc, DateTime endUtc, double secondsToChange)
        {
            IsOpen = isOpen;
            Index = index;
            StartUtc = startUtc;
            EndUtc = endUtc;
            SecondsToChange = secondsToChange;
        }
    }

    /// <summary>
    /// The real-time calendar of the world bosses and the Invasão de Āz (Docs/WorldEvents.md §2). Pure and
    /// deterministic: every window is a function of the UTC instant and <see cref="ServerUtcOffsetHours"/> (the server's
    /// fixed time zone, Brasília), never of the device's locale, so every client and the future server agree on what is
    /// open, where and in which form. The injectable <see cref="UtcNow"/> is the only clock (tests set it; online it is
    /// the server time).
    /// </summary>
    public static class WorldEventCalendar
    {
        /// <summary>Server time zone: Brasília (UTC−3, no daylight saving since 2019).</summary>
        public const int ServerUtcOffsetHours = -3;

        public static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>The clock (UTC). Tests inject; online the server time.</summary>
        public static Func<DateTime> UtcNow { get; set; } = () => DateTime.UtcNow;

        /// <summary>Server-local minutes since 1970-01-01 00:00 local.</summary>
        public static double LocalMinutes(DateTime utc)
        {
            DateTime u = utc.Kind == DateTimeKind.Local ? utc.ToUniversalTime() : utc;
            return (u - Epoch).TotalMinutes + ServerUtcOffsetHours * 60d;
        }

        /// <summary>UTC instant of a server-local minute count.</summary>
        public static DateTime UtcFromLocalMinutes(double localMinutes)
        {
            return Epoch.AddMinutes(localMinutes - ServerUtcOffsetHours * 60d);
        }

        /// <summary>Server-local hour of day (0–24) at <paramref name="utc"/>.</summary>
        public static double LocalHourOfDay(DateTime utc)
        {
            double minutes = LocalMinutes(utc) % (24d * 60d);
            if (minutes < 0d)
            {
                minutes += 24d * 60d;
            }

            return minutes / 60d;
        }

        public static RealTimeWindow WindowAt(RealTimeSchedule schedule, DateTime utc)
        {
            if (!schedule.IsValid)
            {
                return new RealTimeWindow(false, 0, DateTime.MaxValue, DateTime.MaxValue, double.PositiveInfinity);
            }

            double sinceOffset = LocalMinutes(utc) - schedule.offsetMinutes;
            long index = (long)Math.Floor(sinceOffset / schedule.periodMinutes);
            double into = sinceOffset - index * (double)schedule.periodMinutes;
            if (into < schedule.windowMinutes)
            {
                return Build(schedule, index, true, (schedule.windowMinutes - into) * 60d);
            }

            return Build(schedule, index + 1, false, (schedule.periodMinutes - into) * 60d);
        }

        /// <summary>Start (UTC) of window <paramref name="index"/>.</summary>
        public static DateTime StartOf(RealTimeSchedule schedule, long index)
        {
            return UtcFromLocalMinutes(schedule.offsetMinutes + index * (double)schedule.periodMinutes);
        }

        private static RealTimeWindow Build(RealTimeSchedule schedule, long index, bool open, double seconds)
        {
            DateTime start = StartOf(schedule, index);
            return new RealTimeWindow(open, index, start, start.AddMinutes(schedule.windowMinutes), Math.Max(0d, seconds));
        }

        // ------------------------------------------------------------------
        // Night forms
        // ------------------------------------------------------------------

        /// <summary>Server-local night: from 18:00 to 06:00 (Brasília).</summary>
        public const double NightStartsHour = 18d;
        public const double NightEndsHour = 6d;

        /// <summary>True when <paramref name="utc"/> is night on the server clock (the world boss comes in its night form).</summary>
        public static bool IsNight(DateTime utc)
        {
            double hour = LocalHourOfDay(utc);
            return hour >= NightStartsHour || hour < NightEndsHour;
        }

        // ------------------------------------------------------------------
        // Deterministic reward seeds (no client RNG)
        // ------------------------------------------------------------------

        /// <summary>
        /// Seed of every reward roll of one event window for one player: FNV-1a over "kind|key|window|player". The same
        /// window and player always roll the same reward, so the server can recompute (and veto) anything a client shows.
        /// </summary>
        public static int RewardSeed(string kind, string key, long windowIndex, string playerId)
        {
            unchecked
            {
                uint hash = 2166136261u;
                string text = (kind ?? string.Empty) + "|" + (key ?? string.Empty) + "|" + windowIndex + "|" + (playerId ?? string.Empty);
                foreach (char c in text)
                {
                    hash ^= c;
                    hash *= 16777619u;
                }

                return (int)(hash & 0x7FFFFFFF);
            }
        }

        // ------------------------------------------------------------------
        // Countdown text
        // ------------------------------------------------------------------

        /// <summary>"12:05" under an hour, "1h05" from an hour up (rounded up, so 0:00 only when it is over).</summary>
        public static string FormatCountdown(double seconds)
        {
            long total = (long)Math.Ceiling(Math.Max(0d, seconds));
            if (total >= 3600)
            {
                long minutes = (total + 59) / 60;
                return $"{minutes / 60}h{minutes % 60:00}";
            }

            return $"{total / 60}:{total % 60:00}";
        }
    }
}
