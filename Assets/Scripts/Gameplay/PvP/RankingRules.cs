using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.PvP
{
    public enum RankingBoard
    {
        /// <summary>Grand resets, then resets, then level, then total XP (MU order; with no resets it is level, then XP).</summary>
        Level = 0,

        /// <summary>Arena rating of a bracket (placed players only), then wins.</summary>
        Pvp = 1,

        /// <summary>Season event points (Bema, world bosses, scheduled events).</summary>
        Events = 2,
    }

    /// <summary>One character's numbers for the boards (filled by the server from its records).</summary>
    public readonly struct RankingEntry
    {
        public readonly string PlayerId;
        public readonly string Name;
        public readonly int Level;
        public readonly long Experience;
        public readonly int PvpRating;
        public readonly int PvpWins;
        public readonly int PvpMatches;
        public readonly bool PvpPlaced;
        public readonly long EventPoints;

        /// <summary>Server time the current score of the board was reached (earlier = ahead on a tie).</summary>
        public readonly double ReachedAt;

        /// <summary>"Voltas da Luz" inside the current grand cycle (<c>Rebirth.RebirthState.Resets</c>, 0–10).</summary>
        public readonly int Resets;

        /// <summary>"Subidas da Coluna" (<c>Rebirth.RebirthState.GrandResets</c>, 0–5).</summary>
        public readonly int GrandResets;

        public RankingEntry(string playerId, string name, int level, long experience, int pvpRating = ArenaRules.StartRating,
            int pvpWins = 0, int pvpMatches = 0, bool pvpPlaced = false, long eventPoints = 0, double reachedAt = 0d,
            int resets = 0, int grandResets = 0)
        {
            Resets = resets;
            GrandResets = grandResets;
            PlayerId = playerId;
            Name = name;
            Level = level;
            Experience = experience;
            PvpRating = pvpRating;
            PvpWins = pvpWins;
            PvpMatches = pvpMatches;
            PvpPlaced = pvpPlaced;
            EventPoints = eventPoints;
            ReachedAt = reachedAt;
        }
    }

    public readonly struct RankedEntry
    {
        /// <summary>1-based place in the ordered board (strict, every place is unique).</summary>
        public readonly int Position;

        /// <summary>Shared rank by score alone ("1, 2, 2, 4"): equal scores share the number, the time only breaks the order.</summary>
        public readonly int Rank;

        /// <summary>Same score as the entry above (settled by <see cref="RankingEntry.ReachedAt"/>, then id).</summary>
        public readonly bool TiedWithPrevious;

        public readonly RankingEntry Entry;

        public RankedEntry(int position, int rank, bool tiedWithPrevious, RankingEntry entry)
        {
            Position = position;
            Rank = rank;
            TiedWithPrevious = tiedWithPrevious;
            Entry = entry;
        }
    }

    /// <summary>
    /// The boards of FASE 12.3 ("Ranking: nível, PvP, eventos"): a total, stable order — the score keys of the board, then
    /// who reached that score first (<see cref="RankingEntry.ReachedAt"/>), then the player id (ordinal) — so two servers and
    /// two refreshes always list the same order. Equal scores share the displayed rank (competition ranking: 1, 2, 2, 4).
    /// The PvP board lists only placed players (<see cref="ArenaRating.IsPlaced"/>); entries with no id are left out.
    /// Season resets: the PvP board follows the arena season (<see cref="ArenaRules.SoftReset"/>); event points reset each
    /// season; the level board never resets.
    /// </summary>
    public static class RankingRules
    {
        public static List<RankedEntry> Rank(IEnumerable<RankingEntry> entries, RankingBoard board, int top = int.MaxValue)
        {
            var list = new List<RankingEntry>();
            if (entries != null)
            {
                foreach (RankingEntry e in entries)
                {
                    if (string.IsNullOrEmpty(e.PlayerId) || (board == RankingBoard.Pvp && !e.PvpPlaced))
                    {
                        continue;
                    }

                    list.Add(e);
                }
            }

            // List.Sort is not stable; the comparer is total (id last), so the order is fully determined anyway.
            list.Sort((a, b) => Compare(a, b, board));
            var ranked = new List<RankedEntry>();
            int rank = 0;
            for (int i = 0; i < list.Count && i < top; i++)
            {
                bool tied = i > 0 && CompareScore(list[i - 1], list[i], board) == 0;
                if (!tied)
                {
                    rank = i + 1;
                }

                ranked.Add(new RankedEntry(i + 1, rank, tied, list[i]));
            }

            return ranked;
        }

        /// <summary>Board order: negative when <paramref name="a"/> is ahead.</summary>
        public static int Compare(RankingEntry a, RankingEntry b, RankingBoard board)
        {
            int byScore = CompareScore(a, b, board);
            if (byScore != 0)
            {
                return byScore;
            }

            int byTime = a.ReachedAt.CompareTo(b.ReachedAt);
            return byTime != 0 ? byTime : string.CompareOrdinal(a.PlayerId, b.PlayerId);
        }

        /// <summary>Score keys only (no time, no id): 0 = tied.</summary>
        public static int CompareScore(RankingEntry a, RankingEntry b, RankingBoard board)
        {
            switch (board)
            {
                case RankingBoard.Level:
                {
                    // MU order: resets weigh more than level (a reset hero at 50 is ahead of a 500 with fewer resets).
                    int c = b.GrandResets.CompareTo(a.GrandResets);
                    if (c != 0)
                    {
                        return c;
                    }

                    c = b.Resets.CompareTo(a.Resets);
                    if (c != 0)
                    {
                        return c;
                    }

                    c = b.Level.CompareTo(a.Level);
                    return c != 0 ? c : b.Experience.CompareTo(a.Experience);
                }

                case RankingBoard.Pvp:
                {
                    int c = b.PvpRating.CompareTo(a.PvpRating);
                    if (c != 0)
                    {
                        return c;
                    }

                    c = b.PvpWins.CompareTo(a.PvpWins);
                    // Same rating and wins: fewer matches played (better ratio) ahead.
                    return c != 0 ? c : a.PvpMatches.CompareTo(b.PvpMatches);
                }

                default:
                    return b.EventPoints.CompareTo(a.EventPoints);
            }
        }

        /// <summary>The place of one player on a board (1-based position), or 0 when not listed.</summary>
        public static int PositionOf(IEnumerable<RankingEntry> entries, RankingBoard board, string playerId)
        {
            foreach (RankedEntry r in Rank(entries, board))
            {
                if (r.Entry.PlayerId == playerId)
                {
                    return r.Position;
                }
            }

            return 0;
        }

        /// <summary>Whether a board is cleared at a season change.</summary>
        public static bool ResetsWithSeason(RankingBoard board) => board != RankingBoard.Level;

        /// <summary>
        /// A copy of <paramref name="e"/> with the season's event points and the time they were reached (the Events board's
        /// tie-break). Every other field is kept. Fed by <c>TimedEvents.TimedEventSeasonPoints</c> (Docs/Network.md §14).
        /// </summary>
        public static RankingEntry WithEventPoints(RankingEntry e, long eventPoints, double reachedAt)
        {
            return new RankingEntry(e.PlayerId, e.Name, e.Level, e.Experience, e.PvpRating, e.PvpWins, e.PvpMatches, e.PvpPlaced,
                Math.Max(0L, eventPoints), reachedAt, e.Resets, e.GrandResets);
        }
    }
}
