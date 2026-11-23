using System;
using System.Collections.Generic;
using Rapadura.Gameplay.WorldEvents;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>One occurrence of an event: its day (server-local), slot of that day and every instant of it (UTC).</summary>
    public readonly struct TimedEventOccurrence
    {
        public readonly TimedEventKind Kind;

        /// <summary>Server-local day index (days since 1970-01-01 local).</summary>
        public readonly long Day;

        /// <summary>0-based slot inside the day (Bema extra starts come after the normal ones, in time order).</summary>
        public readonly int Slot;

        public readonly DateTime WarningUtc;
        public readonly DateTime EntryOpensUtc;
        public readonly DateTime StartUtc;
        public readonly DateTime EndUtc;

        public TimedEventOccurrence(TimedEventKind kind, long day, int slot, DateTime warningUtc, DateTime entryOpensUtc, DateTime startUtc, DateTime endUtc)
        {
            Kind = kind;
            Day = day;
            Slot = slot;
            WarningUtc = warningUtc;
            EntryOpensUtc = entryOpensUtc;
            StartUtc = startUtc;
            EndUtc = endUtc;
        }

        /// <summary>Unique per event: day × 32 + slot. Feeds the seeds and the room registry.</summary>
        public long Key => Day * 32 + Slot;

        public bool IsValid => Kind != TimedEventKind.None;
    }

    /// <summary>Where an event stands at an instant: its phase, the occurrence it refers to and the seconds to the next change.</summary>
    public readonly struct TimedEventStatus
    {
        public readonly TimedEventPhase Phase;

        /// <summary>The occurrence running / open / announced now, or the next one when <see cref="TimedEventPhase.Closed"/>.</summary>
        public readonly TimedEventOccurrence Occurrence;

        public readonly double SecondsToChange;

        public TimedEventStatus(TimedEventPhase phase, TimedEventOccurrence occurrence, double secondsToChange)
        {
            Phase = phase;
            Occurrence = occurrence;
            SecondsToChange = secondsToChange;
        }
    }

    /// <summary>
    /// The agenda of the scheduled events: everything is a pure function of the UTC instant passed in (no system clock),
    /// on the fixed server zone of <see cref="WorldEventCalendar"/> (Brasília, UTC−3). Bema days (June, server-local) add
    /// the extra starts and drop the events suspended for the truce.
    /// </summary>
    public static class TimedEventAgenda
    {
        public const int MinutesPerDay = 24 * 60;

        /// <summary>How far <see cref="Next"/> looks ahead (covers a whole Bema month without the Cerco).</summary>
        public const int LookAheadDays = 45;

        public static long LocalDay(DateTime utc) => (long)Math.Floor(WorldEventCalendar.LocalMinutes(utc) / MinutesPerDay);

        /// <summary>Server-local calendar date of a day index.</summary>
        public static DateTime LocalDate(long day) => WorldEventCalendar.Epoch.AddDays(day);

        /// <summary>UTC of a server-local minute of a day.</summary>
        public static DateTime UtcAt(long day, int minuteOfDay) => WorldEventCalendar.UtcFromLocalMinutes(day * (double)MinutesPerDay + minuteOfDay);

        /// <summary>The occurrences of <paramref name="definition"/> on server-local day <paramref name="day"/>, in time order.</summary>
        public static List<TimedEventOccurrence> OccurrencesOn(TimedEventDefinition definition, long day)
        {
            var list = new List<TimedEventOccurrence>();
            if (definition == null)
            {
                return list;
            }

            bool bema = TimedBemaRules.IsBemaDay(day);
            if (bema && definition.SuspendedDuringBema)
            {
                return list;
            }

            var starts = new List<int>(definition.StartMinutes);
            if (bema)
            {
                starts.AddRange(definition.BemaExtraStartMinutes);
            }

            starts.Sort();
            for (int slot = 0; slot < starts.Count; slot++)
            {
                DateTime start = UtcAt(day, starts[slot]);
                list.Add(new TimedEventOccurrence(definition.Kind, day, slot, start.AddMinutes(-definition.WarningMinutes),
                    start.AddMinutes(-definition.EntryMinutes), start, start.AddMinutes(definition.DurationMinutes)));
            }

            return list;
        }

        /// <summary>The current occurrence (not over yet at <paramref name="utc"/>) or the next one; invalid when none in <see cref="LookAheadDays"/>.</summary>
        public static TimedEventOccurrence Next(TimedEventDefinition definition, DateTime utc)
        {
            if (definition == null)
            {
                return default;
            }

            long today = LocalDay(utc);
            for (long day = today - 1; day <= today + LookAheadDays; day++)
            {
                foreach (TimedEventOccurrence occurrence in OccurrencesOn(definition, day))
                {
                    if (occurrence.EndUtc > utc)
                    {
                        return occurrence;
                    }
                }
            }

            return default;
        }

        /// <summary>The next occurrence whose gate has not opened yet at <paramref name="utc"/> (for "próxima Vigília às …").</summary>
        public static TimedEventOccurrence NextNotYetOpen(TimedEventDefinition definition, DateTime utc)
        {
            if (definition == null)
            {
                return default;
            }

            long today = LocalDay(utc);
            for (long day = today - 1; day <= today + LookAheadDays; day++)
            {
                foreach (TimedEventOccurrence occurrence in OccurrencesOn(definition, day))
                {
                    if (occurrence.EntryOpensUtc > utc)
                    {
                        return occurrence;
                    }
                }
            }

            return default;
        }

        public static TimedEventStatus StatusAt(TimedEventDefinition definition, DateTime utc)
        {
            TimedEventOccurrence occurrence = Next(definition, utc);
            if (!occurrence.IsValid)
            {
                return new TimedEventStatus(TimedEventPhase.Closed, occurrence, double.PositiveInfinity);
            }

            if (utc < occurrence.WarningUtc)
            {
                return new TimedEventStatus(TimedEventPhase.Closed, occurrence, (occurrence.WarningUtc - utc).TotalSeconds);
            }

            if (utc < occurrence.EntryOpensUtc)
            {
                return new TimedEventStatus(TimedEventPhase.Announced, occurrence, (occurrence.EntryOpensUtc - utc).TotalSeconds);
            }

            if (utc < occurrence.StartUtc)
            {
                return new TimedEventStatus(TimedEventPhase.EntryOpen, occurrence, (occurrence.StartUtc - utc).TotalSeconds);
            }

            return new TimedEventStatus(TimedEventPhase.Running, occurrence, (occurrence.EndUtc - utc).TotalSeconds);
        }

        /// <summary>Seconds of fight elapsed in the occurrence (0 before the start, capped at the duration).</summary>
        public static double ElapsedSeconds(TimedEventOccurrence occurrence, DateTime utc)
        {
            if (!occurrence.IsValid || utc <= occurrence.StartUtc)
            {
                return 0d;
            }

            return Math.Min((utc - occurrence.StartUtc).TotalSeconds, (occurrence.EndUtc - occurrence.StartUtc).TotalSeconds);
        }

        /// <summary>
        /// Seed of everything random in one occurrence (waves, gates, loot): FNV-1a of the event and the occurrence key,
        /// reusing <see cref="WorldEventCalendar.RewardSeed"/>. Pass the player id for per-player reward rolls, empty for the room.
        /// </summary>
        public static int Seed(TimedEventKind kind, long occurrenceKey, int roomIndex, int instance, string playerId = "")
        {
            return WorldEventCalendar.RewardSeed("timed_" + (int)kind, roomIndex + "." + instance, occurrenceKey, playerId ?? string.Empty);
        }

        /// <summary>Deterministic uniform [0,1) from a seed and three salts (SplitMix64), the same on client and server.</summary>
        public static double Roll(int seed, int a, int b = 0, int c = 0)
        {
            unchecked
            {
                ulong x = (ulong)(uint)seed;
                x = Mix(x + 0x9E3779B97F4A7C15UL * (ulong)(uint)(a + 1));
                x = Mix(x + 0x9E3779B97F4A7C15UL * (ulong)(uint)(b + 7));
                x = Mix(x + 0x9E3779B97F4A7C15UL * (ulong)(uint)(c + 13));
                return (x >> 11) * (1.0 / 9007199254740992.0);
            }
        }

        private static ulong Mix(ulong z)
        {
            unchecked
            {
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}
