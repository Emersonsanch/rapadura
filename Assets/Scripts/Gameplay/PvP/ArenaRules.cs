using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Party;

namespace Rapadura.Gameplay.PvP
{
    /// <summary>Arena brackets; the value is the team size. Each bracket has its own rating.</summary>
    public enum ArenaBracket
    {
        Solo = 1,
        Duo = 2,
        Trio = 3,
    }

    public enum ArenaOutcome
    {
        TeamAWins = 0,
        TeamBWins = 1,
        Draw = 2,
    }

    /// <summary>Season tiers, by the season's highest rating. Rewards are titles and cosmetics only.</summary>
    public enum ArenaTier
    {
        None = 0,
        Centelha = 1,
        Chama = 2,
        Lampada = 3,
        Estrela = 4,
        ColunaDaGloria = 5,
    }

    /// <summary>One player's rating in one bracket for the current season (server-side).</summary>
    public sealed class ArenaRating
    {
        public int Rating { get; internal set; }
        public int Matches { get; internal set; }
        public int Wins { get; internal set; }
        public int Losses { get; internal set; }
        public int Draws { get; internal set; }
        public int SeasonHigh { get; internal set; }

        public ArenaRating(int rating = ArenaRules.StartRating, int matches = 0, int wins = 0, int losses = 0, int draws = 0, int? seasonHigh = null)
        {
            Rating = rating;
            Matches = Math.Max(0, matches);
            Wins = Math.Max(0, wins);
            Losses = Math.Max(0, losses);
            Draws = Math.Max(0, draws);
            SeasonHigh = seasonHigh ?? (Matches >= ArenaRules.PlacementMatches ? rating : 0);
        }

        /// <summary>Placement done: the rating shows and counts for the board and rewards.</summary>
        public bool IsPlaced => Matches >= ArenaRules.PlacementMatches;

        public int PlacementLeft => Math.Max(0, ArenaRules.PlacementMatches - Matches);
    }

    /// <summary>What the server knows about one player entering the queue.</summary>
    public readonly struct ArenaQueueMember
    {
        public readonly string Id;
        public readonly int Rating;
        public readonly int Level;
        public readonly PkStanding Standing;

        /// <summary>In a duel, a challenge, another queue entry or a match.</summary>
        public readonly bool Busy;

        public ArenaQueueMember(string id, int rating, int level, PkStanding standing = PkStanding.Clean, bool busy = false)
        {
            Id = id;
            Rating = rating;
            Level = level;
            Standing = standing;
            Busy = busy;
        }
    }

    public sealed class ArenaQueueEntry
    {
        public readonly string EntryId;
        public readonly ArenaBracket Bracket;
        public readonly IReadOnlyList<string> MemberIds;

        /// <summary>Average of the members' bracket ratings (rounded).</summary>
        public readonly int Rating;

        public readonly double EnqueuedAt;

        public ArenaQueueEntry(string entryId, ArenaBracket bracket, IReadOnlyList<string> memberIds, int rating, double enqueuedAt)
        {
            EntryId = entryId;
            Bracket = bracket;
            MemberIds = memberIds;
            Rating = rating;
            EnqueuedAt = enqueuedAt;
        }
    }

    /// <summary>Two queue entries matched. <see cref="TeamA"/> starts on the first side; <see cref="FieldIndex"/> picks the arena field.</summary>
    public readonly struct ArenaPairing
    {
        public readonly ArenaBracket Bracket;
        public readonly ArenaQueueEntry TeamA;
        public readonly ArenaQueueEntry TeamB;
        public readonly int FieldIndex;

        public ArenaPairing(ArenaBracket bracket, ArenaQueueEntry teamA, ArenaQueueEntry teamB, int fieldIndex)
        {
            Bracket = bracket;
            TeamA = teamA;
            TeamB = teamB;
            FieldIndex = fieldIndex;
        }
    }

    /// <summary>The season reward: a title and a cosmetic, never stats, gold or items that fight.</summary>
    public readonly struct ArenaSeasonReward
    {
        public readonly ArenaTier Tier;
        public readonly string TitleKey;
        public readonly string CosmeticId;

        public ArenaSeasonReward(ArenaTier tier, string titleKey, string cosmeticId)
        {
            Tier = tier;
            TitleKey = titleKey;
            CosmeticId = cosmeticId;
        }

        public bool HasReward => Tier != ArenaTier.None;

        /// <summary>Arena rewards never change stats (no pay-to-win, no grind-to-win).</summary>
        public bool GrantsStats => false;
    }

    /// <summary>
    /// The ranked arena of FASE 12.3: a simple Elo per bracket (1v1, 2v2, 3v3 by party), placement matches, a queue whose
    /// rating band widens with the wait, a season with a soft reset and cosmetic / title rewards only. Pure and
    /// deterministic: integer ratings, rounding away from zero, the server seed for sides and fields.
    /// <list type="bullet">
    /// <item>Start <see cref="StartRating"/>; the first <see cref="PlacementMatches"/> matches move with K =
    /// <see cref="PlacementK"/>, then K = 32 below 1800, 24 below 2100, 16 above. Never below <see cref="MinRating"/>.</item>
    /// <item>Teams: the expected score uses the team averages; each member moves by their own K (a placing member moves more).</item>
    /// <item>Queue band: ±<see cref="InitialBand"/> widening by <see cref="BandGrowth"/> each <see cref="BandStepSeconds"/>
    /// of wait, up to ±<see cref="MaxBand"/>; two entries match when the gap fits the larger of their bands; the longest
    /// waiting entry is served first and takes the closest rating.</item>
    /// <item>Season end: rating → 1500 + (rating − 1500) × <see cref="SoftResetKeep"/>, placement again, high reset; reward by
    /// the season high with at least <see cref="MinMatchesForReward"/> matches.</item>
    /// </list>
    /// </summary>
    public static class ArenaRules
    {
        public const int StartRating = 1500;
        public const int MinRating = 100;
        public const int PlacementMatches = 5;
        public const int PlacementK = 64;

        /// <summary>Level to enter the ranked arena (any region; it is consented PvP in its own instance).</summary>
        public const int MinLevel = 30;

        public const int InitialBand = 100;
        public const int BandGrowth = 25;
        public const double BandStepSeconds = 10d;
        public const int MaxBand = 500;

        public const double SoftResetKeep = 0.5d;
        public const int MinMatchesForReward = 10;

        /// <summary>Arena fields the server picks from with its seed (Pátio do Bema, Salão das Colunas, Jardim das Lâmpadas).</summary>
        public const int FieldCount = 3;

        public static int KFactor(ArenaRating rating)
        {
            if (rating == null || !rating.IsPlaced)
            {
                return PlacementK;
            }

            if (rating.Rating < 1800)
            {
                return 32;
            }

            return rating.Rating < 2100 ? 24 : 16;
        }

        /// <summary>Elo expected score of <paramref name="rating"/> against <paramref name="opponent"/>.</summary>
        public static double ExpectedScore(double rating, double opponent) => 1d / (1d + Math.Pow(10d, (opponent - rating) / 400d));

        public static int Delta(int k, double rating, double opponent, double score) =>
            (int)Math.Round(k * (score - ExpectedScore(rating, opponent)), MidpointRounding.AwayFromZero);

        public static int TeamRating(IReadOnlyList<ArenaRating> team)
        {
            if (team == null || team.Count == 0)
            {
                return StartRating;
            }

            long sum = 0;
            foreach (ArenaRating r in team)
            {
                sum += r?.Rating ?? StartRating;
            }

            return (int)Math.Round((double)sum / team.Count, MidpointRounding.AwayFromZero);
        }

        /// <summary>Applies a finished match to every member. Returns the rating change of each member, team A then team B.</summary>
        public static int[] ApplyMatch(IReadOnlyList<ArenaRating> teamA, IReadOnlyList<ArenaRating> teamB, ArenaOutcome outcome)
        {
            if (teamA == null || teamB == null)
            {
                return Array.Empty<int>();
            }

            int ratingA = TeamRating(teamA);
            int ratingB = TeamRating(teamB);
            double scoreA = outcome == ArenaOutcome.TeamAWins ? 1d : outcome == ArenaOutcome.TeamBWins ? 0d : 0.5d;
            var deltas = new int[teamA.Count + teamB.Count];
            for (int i = 0; i < teamA.Count; i++)
            {
                deltas[i] = ApplyOne(teamA[i], ratingA, ratingB, scoreA);
            }

            for (int i = 0; i < teamB.Count; i++)
            {
                deltas[teamA.Count + i] = ApplyOne(teamB[i], ratingB, ratingA, 1d - scoreA);
            }

            return deltas;
        }

        private static int ApplyOne(ArenaRating r, int own, int opponent, double score)
        {
            if (r == null)
            {
                return 0;
            }

            int delta = Delta(KFactor(r), own, opponent, score);
            int before = r.Rating;
            r.Rating = Math.Max(MinRating, r.Rating + delta);
            r.Matches++;
            if (score >= 1d)
            {
                r.Wins++;
            }
            else if (score <= 0d)
            {
                r.Losses++;
            }
            else
            {
                r.Draws++;
            }

            if (r.IsPlaced)
            {
                r.SeasonHigh = Math.Max(r.SeasonHigh, r.Rating);
            }

            return r.Rating - before;
        }

        /// <summary>Half-width of the rating band after waiting <paramref name="waitSeconds"/>.</summary>
        public static int SearchBand(double waitSeconds)
        {
            if (waitSeconds <= 0d)
            {
                return InitialBand;
            }

            long steps = (long)Math.Floor(waitSeconds / BandStepSeconds);
            long band = InitialBand + steps * BandGrowth;
            return (int)Math.Min(MaxBand, band);
        }

        /// <summary>Season end for one rating: soft reset toward the start, placement again, season high cleared.</summary>
        public static void SoftReset(ArenaRating r)
        {
            if (r == null)
            {
                return;
            }

            r.Rating = Math.Max(MinRating, (int)Math.Round(StartRating + (r.Rating - StartRating) * SoftResetKeep, MidpointRounding.AwayFromZero));
            r.Matches = 0;
            r.Wins = 0;
            r.Losses = 0;
            r.Draws = 0;
            r.SeasonHigh = 0;
        }

        public static ArenaTier TierOf(int rating)
        {
            if (rating >= 2000)
            {
                return ArenaTier.ColunaDaGloria;
            }

            if (rating >= 1800)
            {
                return ArenaTier.Estrela;
            }

            if (rating >= 1600)
            {
                return ArenaTier.Lampada;
            }

            return rating >= 1400 ? ArenaTier.Chama : ArenaTier.Centelha;
        }

        /// <summary>The reward at season end (before the soft reset): placed, at least <see cref="MinMatchesForReward"/> matches, tier by the season high.</summary>
        public static ArenaSeasonReward SeasonReward(ArenaRating r, ArenaBracket bracket, int season)
        {
            if (r == null || !r.IsPlaced || r.Matches < MinMatchesForReward)
            {
                return new ArenaSeasonReward(ArenaTier.None, null, null);
            }

            ArenaTier tier = TierOf(Math.Max(r.SeasonHigh, r.Rating));
            string tierName = PvpTexts.Snake(tier.ToString());
            string bracketName = PvpTexts.Snake(bracket.ToString());
            return new ArenaSeasonReward(tier, PvpTexts.ArenaTitleKey(tier), "cosmetic_arena_s" + season + "_" + bracketName + "_" + tierName);
        }

        /// <summary>Checks a queue entry: team size, one live party for 2v2 / 3v3, level, PK standing, busy.</summary>
        public static PvpRefusal CheckEntry(ArenaBracket bracket, IReadOnlyList<ArenaQueueMember> members, PartyState party)
        {
            if (members == null || members.Count != (int)bracket)
            {
                return PvpRefusal.WrongTeamSize;
            }

            var seen = new HashSet<string>();
            foreach (ArenaQueueMember m in members)
            {
                if (string.IsNullOrEmpty(m.Id) || !seen.Add(m.Id))
                {
                    return PvpRefusal.InvalidPlayer;
                }
            }

            if (members.Count > 1)
            {
                for (int i = 1; i < members.Count; i++)
                {
                    if (!PartyRules.AreInSameParty(party, members[0].Id, members[i].Id))
                    {
                        return PvpRefusal.TeamNotOneParty;
                    }
                }
            }

            foreach (ArenaQueueMember m in members)
            {
                if (m.Busy)
                {
                    return PvpRefusal.Busy;
                }

                if (m.Level < MinLevel)
                {
                    return PvpRefusal.LevelTooLow;
                }

                if (!PkRules.CanQueueArena(m.Standing))
                {
                    return PvpRefusal.OutlawCannotQueue;
                }
            }

            return PvpRefusal.None;
        }

        /// <summary>Deterministic pick in [0, count) from the server seed and a salt (splitmix64).</summary>
        public static int PickIndex(int seed, int salt, int count)
        {
            if (count <= 1)
            {
                return 0;
            }

            unchecked
            {
                ulong z = ((ulong)(uint)seed << 32) ^ (uint)salt;
                z += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                z ^= z >> 31;
                return (int)(z % (ulong)count);
            }
        }
    }

    /// <summary>The ranked queue the server keeps: enqueue, leave, and the matchmaking pass of each tick.</summary>
    public sealed class ArenaQueue
    {
        private readonly List<ArenaQueueEntry> _entries = new List<ArenaQueueEntry>();
        private int _matchCounter;

        public IReadOnlyList<ArenaQueueEntry> Entries => _entries;

        public bool Contains(string playerId)
        {
            foreach (ArenaQueueEntry e in _entries)
            {
                foreach (string id in e.MemberIds)
                {
                    if (id == playerId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>Puts an entry in the queue. <paramref name="party"/> is the members' party (null for 1v1).</summary>
        public PvpRefusal Enqueue(string entryId, ArenaBracket bracket, IReadOnlyList<ArenaQueueMember> members, PartyState party, double now)
        {
            if (string.IsNullOrEmpty(entryId))
            {
                return PvpRefusal.InvalidPlayer;
            }

            PvpRefusal check = ArenaRules.CheckEntry(bracket, members, party);
            if (check != PvpRefusal.None)
            {
                return check;
            }

            foreach (ArenaQueueEntry e in _entries)
            {
                if (e.EntryId == entryId)
                {
                    return PvpRefusal.AlreadyQueued;
                }
            }

            var ids = new List<string>();
            long sum = 0;
            foreach (ArenaQueueMember m in members)
            {
                if (Contains(m.Id))
                {
                    return PvpRefusal.AlreadyQueued;
                }

                ids.Add(m.Id);
                sum += m.Rating;
            }

            int rating = (int)Math.Round((double)sum / members.Count, MidpointRounding.AwayFromZero);
            _entries.Add(new ArenaQueueEntry(entryId, bracket, ids, rating, now));
            return PvpRefusal.None;
        }

        public PvpRefusal Leave(string entryId)
        {
            int index = _entries.FindIndex(e => e.EntryId == entryId);
            if (index < 0)
            {
                return PvpRefusal.NotQueued;
            }

            _entries.RemoveAt(index);
            return PvpRefusal.None;
        }

        /// <summary>
        /// One matchmaking pass: oldest entry first (then entry id, ordinal), each takes the closest-rated entry of its bracket
        /// whose gap fits the larger of the two bands (ties: the one waiting longer, then id). Matched entries leave the queue.
        /// Sides and field come from <paramref name="seed"/> and a running match counter, so a replay gives the same result.
        /// </summary>
        public List<ArenaPairing> FindMatches(double now, int seed)
        {
            var ordered = new List<ArenaQueueEntry>(_entries);
            ordered.Sort(CompareByAge);
            var taken = new HashSet<ArenaQueueEntry>();
            var pairings = new List<ArenaPairing>();
            foreach (ArenaQueueEntry entry in ordered)
            {
                if (taken.Contains(entry))
                {
                    continue;
                }

                ArenaQueueEntry best = null;
                int bestGap = int.MaxValue;
                foreach (ArenaQueueEntry other in ordered)
                {
                    if (other == entry || taken.Contains(other) || other.Bracket != entry.Bracket)
                    {
                        continue;
                    }

                    int gap = Math.Abs(entry.Rating - other.Rating);
                    int band = Math.Max(ArenaRules.SearchBand(now - entry.EnqueuedAt), ArenaRules.SearchBand(now - other.EnqueuedAt));
                    if (gap > band || gap >= bestGap)
                    {
                        continue; // ordered by age, so an equal gap keeps the older candidate
                    }

                    best = other;
                    bestGap = gap;
                }

                if (best == null)
                {
                    continue;
                }

                taken.Add(entry);
                taken.Add(best);
                int salt = _matchCounter++;
                bool swap = ArenaRules.PickIndex(seed, salt * 2, 2) == 1;
                int field = ArenaRules.PickIndex(seed, salt * 2 + 1, ArenaRules.FieldCount);
                pairings.Add(new ArenaPairing(entry.Bracket, swap ? best : entry, swap ? entry : best, field));
            }

            _entries.RemoveAll(taken.Contains);
            return pairings;
        }

        private static int CompareByAge(ArenaQueueEntry a, ArenaQueueEntry b)
        {
            int byTime = a.EnqueuedAt.CompareTo(b.EnqueuedAt);
            return byTime != 0 ? byTime : string.CompareOrdinal(a.EntryId, b.EntryId);
        }
    }
}
