using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>What earns (or costs) points in a room. Never renumber.</summary>
    public enum TimedScoreAction
    {
        KillShade = 0,
        KillRunner = 1,
        KillCaster = 2,
        KillBrute = 3,
        KillElite = 4,
        KillArchon = 5,

        /// <summary>Everyone alive in the room when a wave / assault is cleared.</summary>
        WaveCleared = 6,

        /// <summary>Cerco: a rekindle that restored health.</summary>
        LampRekindled = 7,

        /// <summary>Got an ally back on their feet (support counts as much as killing).</summary>
        AllyRevived = 8,

        /// <summary>Hit an enemy someone else killed within 10 s.</summary>
        Assist = 9,

        /// <summary>Cerco: the lamp ended ≥ 75% (everyone in the room).</summary>
        LampIntact = 10,

        /// <summary>Victory of the room (everyone active in it).</summary>
        Victory = 11,

        Death = 12,
    }

    /// <summary>The reward band of one player in one room.</summary>
    public enum TimedRewardBand
    {
        None = 0,

        /// <summary>Active but below Bronze (the floor).</summary>
        Participation = 1,
        Bronze = 2,
        Silver = 3,
        Gold = 4,
    }

    /// <summary>One line of a room's board.</summary>
    public readonly struct TimedScoreEntry
    {
        public readonly string PlayerId;
        public readonly int Points;
        public readonly int Deaths;

        /// <summary>Order of arrival in the room (tie-break: who came first).</summary>
        public readonly int JoinOrder;

        /// <summary>False = idle (the server's activity count, like <see cref="TimedCacadaRules.IsActive"/>): no band.</summary>
        public readonly bool Active;

        public TimedScoreEntry(string playerId, int points, int deaths, int joinOrder, bool active = true)
        {
            PlayerId = playerId;
            Points = points;
            Deaths = deaths;
            JoinOrder = joinOrder;
            Active = active;
        }
    }

    public readonly struct TimedRankedEntry
    {
        public readonly TimedScoreEntry Entry;

        /// <summary>1-based; equal points and deaths share the position ("1, 2, 2, 4").</summary>
        public readonly int Position;

        public readonly TimedRewardBand Band;

        public TimedRankedEntry(TimedScoreEntry entry, int position, TimedRewardBand band)
        {
            Entry = entry;
            Position = position;
            Band = band;
        }
    }

    /// <summary>A player's running tally in a room (the server keeps one per player per room).</summary>
    public sealed class TimedScoreSheet
    {
        private readonly Dictionary<TimedScoreAction, int> _counts = new Dictionary<TimedScoreAction, int>();

        public int Count(TimedScoreAction action) => _counts.TryGetValue(action, out int c) ? c : 0;

        public void Add(TimedScoreAction action, int times = 1)
        {
            if (times > 0)
            {
                _counts[action] = Count(action) + times;
            }
        }

        /// <summary>Extra points from outside the table (the Caçada's contribution points).</summary>
        public int BonusPoints { get; set; }

        public int Deaths => Count(TimedScoreAction.Death);

        /// <summary>Sum of the table, never below 0.</summary>
        public int Points
        {
            get
            {
                int total = BonusPoints;
                foreach (KeyValuePair<TimedScoreAction, int> pair in _counts)
                {
                    total += TimedEventScoring.PointsFor(pair.Key) * pair.Value;
                }

                return Math.Max(0, total);
            }
        }
    }

    /// <summary>Points of each action, the room ranking and the band of each place (Docs/Network.md §14).</summary>
    public static class TimedEventScoring
    {
        /// <summary>Top 10% (and always the 1st) → Gold; top 35% → Silver; the rest with ≥ 60 points → Bronze; active → Participation.</summary>
        public const float GoldFraction = 0.10f;
        public const float SilverFraction = 0.35f;
        public const int BronzeMinPoints = 60;

        public static int PointsFor(TimedScoreAction action)
        {
            switch (action)
            {
                case TimedScoreAction.KillShade: return 5;
                case TimedScoreAction.KillRunner: return 5;
                case TimedScoreAction.KillCaster: return 10;
                case TimedScoreAction.KillBrute: return 15;
                case TimedScoreAction.KillElite: return 40;
                case TimedScoreAction.KillArchon: return 150;
                case TimedScoreAction.WaveCleared: return 20;
                case TimedScoreAction.LampRekindled: return 15;
                case TimedScoreAction.AllyRevived: return 25;
                case TimedScoreAction.Assist: return 2;
                case TimedScoreAction.LampIntact: return 60;
                case TimedScoreAction.Victory: return 50;
                case TimedScoreAction.Death: return -20;
                default: return 0;
            }
        }

        public static TimedScoreAction KillAction(TimedEnemyRole role)
        {
            switch (role)
            {
                case TimedEnemyRole.Runner: return TimedScoreAction.KillRunner;
                case TimedEnemyRole.Caster: return TimedScoreAction.KillCaster;
                case TimedEnemyRole.Brute: return TimedScoreAction.KillBrute;
                case TimedEnemyRole.Elite: return TimedScoreAction.KillElite;
                case TimedEnemyRole.Archon: return TimedScoreAction.KillArchon;
                default: return TimedScoreAction.KillShade;
            }
        }

        /// <summary>
        /// Room ranking: points (more first), then deaths (fewer first), then join order, then id (ordinal). Idle players
        /// go last with no band. Bands by place among the active ones.
        /// </summary>
        public static List<TimedRankedEntry> Rank(IEnumerable<TimedScoreEntry> entries)
        {
            var list = new List<TimedScoreEntry>();
            if (entries != null)
            {
                list.AddRange(entries);
            }

            list.Sort((a, b) =>
            {
                if (a.Active != b.Active) return a.Active ? -1 : 1;
                if (a.Points != b.Points) return b.Points.CompareTo(a.Points);
                if (a.Deaths != b.Deaths) return a.Deaths.CompareTo(b.Deaths);
                if (a.JoinOrder != b.JoinOrder) return a.JoinOrder.CompareTo(b.JoinOrder);
                return string.CompareOrdinal(a.PlayerId, b.PlayerId);
            });

            int activeCount = list.FindAll(e => e.Active).Count;
            var ranked = new List<TimedRankedEntry>(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                TimedScoreEntry entry = list[i];
                int position = i + 1;
                if (i > 0 && list[i - 1].Active == entry.Active && list[i - 1].Points == entry.Points && list[i - 1].Deaths == entry.Deaths)
                {
                    position = ranked[i - 1].Position;
                }

                ranked.Add(new TimedRankedEntry(entry, position, BandFor(position, activeCount, entry)));
            }

            return ranked;
        }

        public static TimedRewardBand BandFor(int position, int activeCount, TimedScoreEntry entry)
        {
            if (!entry.Active || activeCount <= 0)
            {
                return TimedRewardBand.None;
            }

            int goldSlots = Math.Max(1, (int)Math.Ceiling(activeCount * GoldFraction));
            int silverSlots = Math.Max(goldSlots, (int)Math.Ceiling(activeCount * SilverFraction));
            if (entry.Points <= 0)
            {
                return TimedRewardBand.Participation;
            }

            if (position <= goldSlots)
            {
                return TimedRewardBand.Gold;
            }

            if (position <= silverSlots)
            {
                return TimedRewardBand.Silver;
            }

            return entry.Points >= BronzeMinPoints ? TimedRewardBand.Bronze : TimedRewardBand.Participation;
        }
    }
}
