using System;
using System.Collections.Generic;
using Rapadura.Gameplay.PvP;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>One player's points from one room of one occurrence (the server writes it when the room ends).</summary>
    public readonly struct TimedEventPointsRecord
    {
        public readonly string PlayerId;
        public readonly TimedEventKind Kind;

        /// <summary><see cref="TimedEventOccurrence.Key"/> (day × 32 + slot): the season is read from its day.</summary>
        public readonly long OccurrenceKey;

        public readonly int RoomIndex;
        public readonly int Points;

        /// <summary>Server time the room ended (unix seconds), the "reached at" of the board.</summary>
        public readonly double At;

        public TimedEventPointsRecord(string playerId, TimedEventKind kind, long occurrenceKey, int roomIndex, int points, double at)
        {
            PlayerId = playerId;
            Kind = kind;
            OccurrenceKey = occurrenceKey;
            RoomIndex = roomIndex;
            Points = points;
            At = at;
        }

        /// <summary>Server-local day of the occurrence.</summary>
        public long Day => (long)Math.Floor(OccurrenceKey / 32d);
    }

    /// <summary>One player's season total on the "Eventos" board.</summary>
    public readonly struct TimedSeasonTotal
    {
        public readonly string PlayerId;
        public readonly long Points;

        /// <summary>Rooms that counted (one per occurrence and event).</summary>
        public readonly int Rooms;

        /// <summary>When the total was reached (the last counted room's time): earlier is ahead on a tie.</summary>
        public readonly double ReachedAt;

        public TimedSeasonTotal(string playerId, long points, int rooms, double reachedAt)
        {
            PlayerId = playerId;
            Points = points;
            Rooms = rooms;
            ReachedAt = reachedAt;
        }
    }

    /// <summary>
    /// The scheduled events' points on the <see cref="RankingBoard.Events"/> board of <see cref="RankingRules"/> (Docs/Network.md
    /// §13–§14). Pure: the server keeps the records, this sums them by season.
    /// <list type="bullet">
    /// <item><b>Season</b>: a quarter of the Brasília calendar (Jan–Mar, Apr–Jun, Jul–Sep, Oct–Dec), read from the occurrence's
    /// server-local day, never the device. June's Bema falls inside the Apr–Jun season. <see cref="RankingRules.ResetsWithSeason"/>
    /// already clears the board at the change; a room counts in the season of the day it <i>started</i>.</item>
    /// <item><b>What counts</b>: the room points of <see cref="TimedEventScoring.Rank"/> for active players with points &gt; 0
    /// (idle ones score nothing, like their band). A result written twice (retry, replay) counts once: per player, event and
    /// occurrence only the best record stays, whatever room or instance it names.</item>
    /// <item><b>Ties</b>: equal totals go by who reached them first (<see cref="TimedSeasonTotal.ReachedAt"/>), then the id.</item>
    /// </list>
    /// </summary>
    public static class TimedEventSeasonPoints
    {
        public const int MonthsPerSeason = 3;

        /// <summary>Season number of a server-local day: year × 4 + quarter (0–3).</summary>
        public static int SeasonOfLocalDay(long localDay)
        {
            DateTime date = TimedEventAgenda.LocalDate(localDay);
            return date.Year * 4 + (date.Month - 1) / MonthsPerSeason;
        }

        public static int SeasonOf(DateTime utc) => SeasonOfLocalDay(TimedEventAgenda.LocalDay(utc));

        public static int SeasonOf(TimedEventOccurrence occurrence) => SeasonOfLocalDay(occurrence.Day);

        /// <summary>The records of one finished room, from its ranked board (active players with points only).</summary>
        public static List<TimedEventPointsRecord> RecordsFrom(TimedEventOccurrence occurrence, int roomIndex,
            IEnumerable<TimedRankedEntry> ranked, double endedAt)
        {
            var records = new List<TimedEventPointsRecord>();
            if (ranked == null || !occurrence.IsValid)
            {
                return records;
            }

            foreach (TimedRankedEntry r in ranked)
            {
                TimedScoreEntry e = r.Entry;
                if (string.IsNullOrEmpty(e.PlayerId) || !e.Active || e.Points <= 0)
                {
                    continue;
                }

                records.Add(new TimedEventPointsRecord(e.PlayerId, occurrence.Kind, occurrence.Key, roomIndex, e.Points, endedAt));
            }

            return records;
        }

        /// <summary>Season totals by player id (only the records of <paramref name="season"/>; replays counted once).</summary>
        public static Dictionary<string, TimedSeasonTotal> Accumulate(IEnumerable<TimedEventPointsRecord> records, int season)
        {
            // Best record per (player, event, occurrence): more points, then earlier, then lower room (fully determined).
            var best = new Dictionary<(string, TimedEventKind, long), TimedEventPointsRecord>();
            if (records != null)
            {
                foreach (TimedEventPointsRecord r in records)
                {
                    if (string.IsNullOrEmpty(r.PlayerId) || r.Kind == TimedEventKind.None || r.Points <= 0 || SeasonOfLocalDay(r.Day) != season)
                    {
                        continue;
                    }

                    var key = (r.PlayerId, r.Kind, r.OccurrenceKey);
                    if (!best.TryGetValue(key, out TimedEventPointsRecord old) || Better(r, old))
                    {
                        best[key] = r;
                    }
                }
            }

            var totals = new Dictionary<string, TimedSeasonTotal>(StringComparer.Ordinal);
            foreach (TimedEventPointsRecord r in best.Values)
            {
                totals.TryGetValue(r.PlayerId, out TimedSeasonTotal t);
                totals[r.PlayerId] = new TimedSeasonTotal(r.PlayerId, t.Points + r.Points, t.Rooms + 1,
                    t.Rooms == 0 ? r.At : Math.Max(t.ReachedAt, r.At));
            }

            return totals;
        }

        private static bool Better(TimedEventPointsRecord a, TimedEventPointsRecord b)
        {
            if (a.Points != b.Points) return a.Points > b.Points;
            if (a.At != b.At) return a.At < b.At;
            return a.RoomIndex < b.RoomIndex;
        }

        /// <summary>
        /// The entries with the season's event points and time written in (<see cref="RankingRules.WithEventPoints"/>); a player
        /// with no record this season gets 0. Players with points but no entry are added with their id as the name.
        /// </summary>
        public static List<RankingEntry> ApplyTo(IEnumerable<RankingEntry> entries, IReadOnlyDictionary<string, TimedSeasonTotal> totals)
        {
            var list = new List<RankingEntry>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            if (entries != null)
            {
                foreach (RankingEntry e in entries)
                {
                    if (e.PlayerId != null) seen.Add(e.PlayerId);
                    TimedSeasonTotal t = default;
                    bool has = e.PlayerId != null && totals != null && totals.TryGetValue(e.PlayerId, out t);
                    list.Add(RankingRules.WithEventPoints(e, has ? t.Points : 0, has ? t.ReachedAt : 0d));
                }
            }

            if (totals != null)
            {
                var missing = new List<string>();
                foreach (string id in totals.Keys)
                {
                    if (!seen.Contains(id)) missing.Add(id);
                }

                missing.Sort(StringComparer.Ordinal);
                foreach (string id in missing)
                {
                    TimedSeasonTotal t = totals[id];
                    list.Add(new RankingEntry(id, id, 0, 0, eventPoints: t.Points, reachedAt: t.ReachedAt));
                }
            }

            return list;
        }

        /// <summary>The season's "Eventos" board in one call: accumulate, write into the entries, rank.</summary>
        public static List<RankedEntry> Board(IEnumerable<RankingEntry> entries, IEnumerable<TimedEventPointsRecord> records, int season,
            int top = int.MaxValue)
        {
            return RankingRules.Rank(ApplyTo(entries, Accumulate(records, season)), RankingBoard.Events, top);
        }
    }
}
