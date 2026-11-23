using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Guild
{
    /// <summary>Why a guild action was refused (<see cref="GuildResult.Refusal"/>). Texts: <see cref="GuildTexts.RefusalKey"/>. Never renumber.</summary>
    public enum GuildRefusal
    {
        None = 0,

        /// <summary>Empty / null player or guild id.</summary>
        InvalidPlayer = 1,

        /// <summary>The one acting is not in the guild.</summary>
        NotMember = 2,

        /// <summary>The rank of the one acting lacks the permission (<see cref="GuildPermissions"/>).</summary>
        NoPermission = 3,

        /// <summary>The target of a kick / promotion / demotion / transfer is not in the guild.</summary>
        TargetNotMember = 4,

        /// <summary>Nobody invites, kicks, promotes or demotes themself.</summary>
        CannotTargetSelf = 5,

        /// <summary>The target's rank is equal to or above the actor's (or the new rank would be).</summary>
        TargetRankTooHigh = 6,

        /// <summary>The player already belongs to a guild (this one or another).</summary>
        AlreadyInGuild = 7,

        /// <summary>The invitee already has a live invite from this guild.</summary>
        AlreadyInvited = 8,

        /// <summary>Accept / decline without a pending invite.</summary>
        NoInvite = 9,

        /// <summary>Accept after <see cref="GuildRules.InviteSeconds"/>: the invite is gone.</summary>
        InviteExpired = 10,

        /// <summary>The guild has <see cref="GuildLevelRules.MaxMembers"/> members for its level.</summary>
        GuildFull = 11,

        /// <summary>The player left a guild less than <see cref="GuildRules.RejoinCooldownSeconds"/> ago.</summary>
        RejoinCooldown = 12,

        /// <summary>The guild was disbanded.</summary>
        Disbanded = 13,

        /// <summary>Founding needs level <see cref="GuildRules.MinFounderLevel"/>.</summary>
        LevelTooLow = 14,

        /// <summary>Founding costs <see cref="GuildRules.CreationCost"/> gold.</summary>
        NotEnoughGold = 15,

        NameTooShort = 16,
        NameTooLong = 17,

        /// <summary>Only letters (with accents), digits (at most 3) and single spaces; starts with a letter.</summary>
        NameInvalidCharacters = 18,

        /// <summary><see cref="GuildNameRules.IsOffensive"/>.</summary>
        NameOffensive = 19,

        /// <summary>Another guild has the same <see cref="GuildNameRules.NameKey"/>.</summary>
        NameTaken = 20,

        /// <summary>Motto, notice or message of the day over its limit.</summary>
        TextTooLong = 21,

        /// <summary>Motto, notice or message of the day caught by the word filter.</summary>
        TextOffensive = 22,

        /// <summary><see cref="GuildEmblem.Validate"/> failed.</summary>
        EmblemInvalid = 23,

        /// <summary>Not a rank that may be given that way (Leader only by transfer).</summary>
        InvalidRank = 24,

        /// <summary>The guild already has the maximum of that rank (<see cref="GuildRules.MaxVices"/>, <see cref="GuildRules.MaxOfficers"/>).</summary>
        RankFull = 25,

        /// <summary>Zero / negative gold or item count.</summary>
        InvalidAmount = 26,

        /// <summary>The bank does not hold that much.</summary>
        NotEnoughInBank = 27,

        /// <summary>The rank's daily withdrawal limit (<see cref="GuildBankRules"/>) would be passed.</summary>
        DailyLimitReached = 28,

        /// <summary>Bound items never go into the bank.</summary>
        ItemNotTradable = 29,

        /// <summary>No free slot / gold cap reached.</summary>
        BankFull = 30,

        /// <summary>Disbanding needs an empty bank.</summary>
        BankNotEmpty = 31,

        NoticeNotFound = 32,

        /// <summary>Disbanding while a war is declared or running.</summary>
        AtWar = 33,

        /// <summary>A guild never declares war on itself.</summary>
        WarSameGuild = 34,

        /// <summary>The two guilds already have a war that has not ended.</summary>
        WarAlreadyExists = 35,

        /// <summary>The guild already has <see cref="GuildWarRules.MaxOpenWars"/> open wars.</summary>
        WarTooManyWars = 36,

        /// <summary>Each guild needs <see cref="GuildWarRules.MinMembers"/> members.</summary>
        WarTooFewMembers = 37,

        /// <summary>The same pair fought less than <see cref="GuildWarRules.RedeclareCooldownSeconds"/> ago.</summary>
        WarCooldown = 38,

        /// <summary>The action does not fit the war's current state.</summary>
        WarWrongState = 39,

        /// <summary>The guild is not one of the two sides.</summary>
        WarNotParticipant = 40,

        /// <summary>Duration / score target out of range.</summary>
        WarInvalidTerms = 41,

        /// <summary>A notice needs some text.</summary>
        TextEmpty = 42,

        /// <summary>
        /// The piece carries its own data (generated roll, refine, options, worn durability): the bank keeps only id and count,
        /// so it would lose them (<see cref="GuildBankRules.CarriesOwnData"/>).
        /// </summary>
        ItemHasOwnData = 43,
    }

    /// <summary>Ranks, highest last-to-first: a higher value outranks a lower one. Never renumber.</summary>
    public enum GuildRank
    {
        Recruit = 0,
        Member = 1,
        Officer = 2,
        Vice = 3,
        Leader = 4,
    }

    /// <summary>What a rank may do (<see cref="GuildPermissions.Has"/>). Flags so the table is one value per rank.</summary>
    [Flags]
    public enum GuildPermission
    {
        None = 0,
        Invite = 1 << 0,
        Kick = 1 << 1,
        Promote = 1 << 2,
        Demote = 1 << 3,
        BankDeposit = 1 << 4,
        BankWithdraw = 1 << 5,
        EditBoard = 1 << 6,
        EditMotd = 1 << 7,
        DeclareWar = 1 << 8,
        EditIdentity = 1 << 9,
        Disband = 1 << 10,
    }

    /// <summary>Outcome of a guild action: no exceptions, just a refusal reason (<see cref="GuildRefusal.None"/> = done).</summary>
    public readonly struct GuildResult
    {
        public readonly GuildRefusal Refusal;

        /// <summary>The guild no longer exists after the action (last member left, or the leader disbanded it).</summary>
        public readonly bool GuildDisbanded;

        /// <summary>Leader after the action.</summary>
        public readonly string LeaderId;

        /// <summary>The bank movement this action made (for the server log), or null.</summary>
        public readonly GuildBankLogEntry LogEntry;

        public GuildResult(GuildRefusal refusal, bool guildDisbanded, string leaderId, GuildBankLogEntry logEntry = null)
        {
            Refusal = refusal;
            GuildDisbanded = guildDisbanded;
            LeaderId = leaderId;
            LogEntry = logEntry;
        }

        public bool Ok => Refusal == GuildRefusal.None;

        public static GuildResult Refused(GuildRefusal reason, string leaderId) => new GuildResult(reason, false, leaderId);
    }

    /// <summary>One member. The bank / contribution counters reset on the server's UTC day (<see cref="GuildRules.DayIndex"/>).</summary>
    public sealed class GuildMember
    {
        public readonly string PlayerId;

        /// <summary>Order of arrival (0 = founder): the tie-break of every succession.</summary>
        public readonly long JoinOrder;

        public GuildRank Rank;

        /// <summary>Last login / action, in server seconds (<see cref="GuildRules.Touch"/>).</summary>
        public double LastActiveAt;

        /// <summary>Lifetime contribution points (the guild's ranking of members).</summary>
        public long ContributionPoints;

        /// <summary>Day of the counters below (-1 = never).</summary>
        public long CounterDay = -1;
        public long GoldWithdrawnToday;
        public int ItemsWithdrawnToday;
        public long GoldPointsToday;

        public GuildMember(string playerId, GuildRank rank, long joinOrder, double now)
        {
            PlayerId = playerId;
            Rank = rank;
            JoinOrder = joinOrder;
            LastActiveAt = now;
        }

        /// <summary>Zeroes the daily counters when <paramref name="day"/> is a new day.</summary>
        public void RollDay(long day)
        {
            if (CounterDay == day)
            {
                return;
            }

            CounterDay = day;
            GoldWithdrawnToday = 0;
            ItemsWithdrawnToday = 0;
            GoldPointsToday = 0;
        }
    }

    /// <summary>A pending invite (lives <see cref="GuildRules.InviteSeconds"/>).</summary>
    public readonly struct GuildInvite
    {
        public readonly string InviteeId;
        public readonly string InviterId;
        public readonly double ExpiresAt;

        public GuildInvite(string inviteeId, string inviterId, double expiresAt)
        {
            InviteeId = inviteeId;
            InviterId = inviterId;
            ExpiresAt = expiresAt;
        }
    }

    /// <summary>One notice of the board ("mural").</summary>
    public sealed class GuildNotice
    {
        public readonly long Id;
        public readonly string AuthorId;
        public readonly string Text;
        public readonly double PostedAt;

        public GuildNotice(long id, string authorId, string text, double postedAt)
        {
            Id = id;
            AuthorId = authorId;
            Text = text;
            PostedAt = postedAt;
        }
    }

    public enum GuildBankAction
    {
        DepositGold = 0,
        WithdrawGold = 1,
        DepositItem = 2,
        WithdrawItem = 3,

        /// <summary>Gold given for contribution points (<see cref="GuildBankRules.Contribute"/>): goes into the bank too.</summary>
        ContributeGold = 4,
    }

    /// <summary>One bank movement: pure data the server appends to its persistent log ("quem tirou o quê").</summary>
    public sealed class GuildBankLogEntry
    {
        public readonly double Time;
        public readonly string PlayerId;
        public readonly GuildRank RankAtTime;
        public readonly GuildBankAction Action;

        /// <summary>Null for gold.</summary>
        public readonly string ItemId;

        /// <summary>Gold or item count moved (always positive).</summary>
        public readonly long Amount;

        /// <summary>Gold in the bank, or count of that item, after the movement.</summary>
        public readonly long BalanceAfter;

        public GuildBankLogEntry(double time, string playerId, GuildRank rankAtTime, GuildBankAction action, string itemId, long amount,
            long balanceAfter)
        {
            Time = time;
            PlayerId = playerId;
            RankAtTime = rankAtTime;
            Action = action;
            ItemId = itemId;
            Amount = amount;
            BalanceAfter = balanceAfter;
        }
    }

    /// <summary>
    /// The state of one guild, held by the server and changed only by <see cref="GuildRules"/>, <see cref="GuildBankRules"/>
    /// and <see cref="GuildBoardRules"/>. Plain C#: the online server runs the very same file.
    /// </summary>
    public sealed class GuildState
    {
        /// <summary>Recent bank movements kept in memory (the server persists all of them).</summary>
        public const int BankLogInMemory = 200;

        public readonly string Id;
        public string Name;

        /// <summary><see cref="GuildNameRules.NameKey"/> of <see cref="Name"/>: what "unique" compares.</summary>
        public string NameKey;

        public string Motto;
        public GuildEmblem Emblem;
        public readonly double CreatedAt;
        public string LeaderId;
        public bool IsDisbanded;
        public long NextJoinOrder;
        public long NextNoticeId = 1;

        /// <summary>Sum of every member's contribution (kept even for members who left): drives <see cref="GuildLevelRules.LevelFor"/>.</summary>
        public long TotalPoints;

        public string MessageOfTheDay = string.Empty;

        public long BankGold;

        private readonly List<GuildMember> _members = new List<GuildMember>();
        private readonly List<GuildInvite> _invites = new List<GuildInvite>();
        private readonly List<GuildNotice> _notices = new List<GuildNotice>();
        private readonly Dictionary<string, long> _bankItems = new Dictionary<string, long>();
        private readonly List<GuildBankLogEntry> _bankLog = new List<GuildBankLogEntry>();

        public GuildState(string id, string name, string motto, GuildEmblem emblem, string founderId, double now)
        {
            Id = id;
            Name = name;
            NameKey = GuildNameRules.NameKey(name);
            Motto = motto ?? string.Empty;
            Emblem = emblem;
            CreatedAt = now;
            LeaderId = founderId;
            _members.Add(new GuildMember(founderId, GuildRank.Leader, NextJoinOrder++, now));
        }

        public IReadOnlyList<GuildMember> Members => _members;
        public IReadOnlyList<GuildInvite> Invites => _invites;
        public IReadOnlyList<GuildNotice> Notices => _notices;
        public IReadOnlyDictionary<string, long> BankItems => _bankItems;
        public IReadOnlyList<GuildBankLogEntry> BankLog => _bankLog;

        public int Count => _members.Count;
        public int Level => GuildLevelRules.LevelFor(TotalPoints);

        public GuildMember Find(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return null;
            }

            foreach (GuildMember member in _members)
            {
                if (member.PlayerId == playerId)
                {
                    return member;
                }
            }

            return null;
        }

        public bool IsMember(string playerId) => Find(playerId) != null;

        public int CountOfRank(GuildRank rank)
        {
            int count = 0;
            foreach (GuildMember member in _members)
            {
                if (member.Rank == rank)
                {
                    count++;
                }
            }

            return count;
        }

        public long ItemCount(string itemId) => itemId != null && _bankItems.TryGetValue(itemId, out long count) ? count : 0;

        public bool TryGetInvite(string inviteeId, out GuildInvite invite)
        {
            foreach (GuildInvite candidate in _invites)
            {
                if (candidate.InviteeId == inviteeId)
                {
                    invite = candidate;
                    return true;
                }
            }

            invite = default;
            return false;
        }

        internal void AddMember(GuildMember member) => _members.Add(member);
        internal bool RemoveMember(string playerId) => _members.RemoveAll(m => m.PlayerId == playerId) > 0;
        internal void AddInvite(GuildInvite invite) => _invites.Add(invite);
        internal bool RemoveInvite(string inviteeId) => _invites.RemoveAll(i => i.InviteeId == inviteeId) > 0;

        internal List<string> RemoveInvitesWhere(Predicate<GuildInvite> match)
        {
            var removed = new List<string>();
            foreach (GuildInvite invite in _invites)
            {
                if (match(invite))
                {
                    removed.Add(invite.InviteeId);
                }
            }

            _invites.RemoveAll(match);
            return removed;
        }

        internal void AddNotice(GuildNotice notice) => _notices.Add(notice);
        internal void RemoveNoticeAt(int index) => _notices.RemoveAt(index);

        internal void SetItemCount(string itemId, long count)
        {
            if (count <= 0)
            {
                _bankItems.Remove(itemId);
            }
            else
            {
                _bankItems[itemId] = count;
            }
        }

        internal void AppendLog(GuildBankLogEntry entry)
        {
            _bankLog.Add(entry);
            if (_bankLog.Count > BankLogInMemory)
            {
                _bankLog.RemoveAt(0);
            }
        }

        internal void ReplaceLastLog(GuildBankLogEntry entry)
        {
            if (_bankLog.Count > 0)
            {
                _bankLog[_bankLog.Count - 1] = entry;
            }
            else
            {
                _bankLog.Add(entry);
            }
        }

        internal void Disband()
        {
            _members.Clear();
            _invites.Clear();
            LeaderId = null;
            IsDisbanded = true;
        }
    }
}
