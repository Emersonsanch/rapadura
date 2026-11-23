using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Party
{
    /// <summary>Why a party action was refused (<see cref="PartyResult.Refusal"/>). Texts: <see cref="PartyTexts.RefusalKey"/>. Never renumber.</summary>
    public enum PartyRefusal
    {
        None = 0,

        /// <summary>Empty / null player id.</summary>
        InvalidPlayer = 1,

        /// <summary>Only the leader may invite, kick or pass the leadership.</summary>
        NotLeader = 2,

        /// <summary>The party already has <see cref="PartyRules.MaxMembers"/> members.</summary>
        PartyFull = 3,

        /// <summary>The invitee already belongs to a party (this one or another).</summary>
        AlreadyInParty = 4,

        /// <summary>The invitee already has a live invite from this party.</summary>
        AlreadyInvited = 5,

        /// <summary>Nobody invites, kicks or passes the leadership to themself.</summary>
        CannotTargetSelf = 6,

        /// <summary>Accept/decline without a pending invite.</summary>
        NoInvite = 7,

        /// <summary>Accept after <see cref="PartyRules.InviteSeconds"/>: the invite is gone.</summary>
        InviteExpired = 8,

        /// <summary>The one acting is not in the party.</summary>
        NotMember = 9,

        /// <summary>The target of a kick / leadership pass is not in the party.</summary>
        TargetNotMember = 10,

        /// <summary>The party was disbanded (fewer than 2 members and no invite left).</summary>
        Disbanded = 11,
    }

    /// <summary>Outcome of a party action: no exceptions, just a refusal reason (<see cref="PartyRefusal.None"/> = done).</summary>
    public readonly struct PartyResult
    {
        public readonly PartyRefusal Refusal;

        /// <summary>The action left the party with fewer than 2 members and no pending invite: it no longer exists.</summary>
        public readonly bool PartyDisbanded;

        /// <summary>Leader after the action (changes on a leader leaving or a pass).</summary>
        public readonly string LeaderId;

        public PartyResult(PartyRefusal refusal, bool partyDisbanded, string leaderId)
        {
            Refusal = refusal;
            PartyDisbanded = partyDisbanded;
            LeaderId = leaderId;
        }

        public bool Ok => Refusal == PartyRefusal.None;

        public static PartyResult Refused(PartyRefusal reason, string leaderId) => new PartyResult(reason, false, leaderId);
    }

    /// <summary>
    /// One member of a party. <see cref="Cousin"/> is the "primo da classe": decision 12.1 (cousin as a class, or an own
    /// character with the cousins as story heroes) is still open, so it is optional — null for an own character, and
    /// then the family bonuses (<see cref="PartyFamilyRules"/>) simply do not count that member.
    /// </summary>
    public sealed class PartyMember
    {
        public readonly string PlayerId;
        public readonly CharacterId? Cousin;

        /// <summary>Order of arrival in the party (0 = founder). The lowest remaining one is "the oldest member".</summary>
        public readonly long JoinOrder;

        public PartyMember(string playerId, CharacterId? cousin, long joinOrder)
        {
            PlayerId = playerId;
            Cousin = cousin;
            JoinOrder = joinOrder;
        }
    }

    /// <summary>A pending invite: who sent it, the cousin of the invitee (if any) and when it stops being valid.</summary>
    public readonly struct PartyInvite
    {
        public readonly string InviteeId;
        public readonly string InviterId;
        public readonly CharacterId? InviteeCousin;
        public readonly double ExpiresAt;

        public PartyInvite(string inviteeId, string inviterId, CharacterId? inviteeCousin, double expiresAt)
        {
            InviteeId = inviteeId;
            InviterId = inviterId;
            InviteeCousin = inviteeCousin;
            ExpiresAt = expiresAt;
        }
    }

    /// <summary>
    /// The data of one party (TODO.md 12.3, "Grupo"), plain C# with no Unity dependency, so the authoritative server keeps
    /// one per party and runs <see cref="PartyRules"/> on it. Mutated only through <see cref="PartyRules"/>; members are
    /// kept in join order. Time is whatever clock the caller uses (server seconds), passed in, never read here.
    /// </summary>
    public sealed class PartyState
    {
        private readonly List<PartyMember> _members = new List<PartyMember>();
        private readonly List<PartyInvite> _invites = new List<PartyInvite>();

        internal long NextJoinOrder;

        public PartyState(string founderId, CharacterId? founderCousin, PartyLootMode lootMode = PartyLootMode.FreeForAll)
        {
            if (!string.IsNullOrEmpty(founderId))
            {
                _members.Add(new PartyMember(founderId, founderCousin, NextJoinOrder++));
                LeaderId = founderId;
            }

            LootMode = lootMode;
        }

        public string LeaderId { get; internal set; }

        /// <summary>Chosen by the leader (<see cref="PartyRules.SetLootMode"/>).</summary>
        public PartyLootMode LootMode { get; internal set; }

        /// <summary>Round-robin cursor (<see cref="PartyLootRules"/>): JoinOrder of the last member who got a drop, -1 = none yet.</summary>
        public long RoundRobinCursor { get; set; } = -1;

        public bool IsDisbanded { get; internal set; }

        /// <summary>Members in join order (founder first).</summary>
        public IReadOnlyList<PartyMember> Members => _members;

        public IReadOnlyList<PartyInvite> Invites => _invites;

        public int Count => _members.Count;

        public bool IsMember(string playerId) => Find(playerId) != null;

        public bool IsLeader(string playerId) => !string.IsNullOrEmpty(playerId) && playerId == LeaderId;

        public PartyMember Find(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return null;
            }

            foreach (PartyMember member in _members)
            {
                if (member.PlayerId == playerId)
                {
                    return member;
                }
            }

            return null;
        }

        public bool TryGetInvite(string inviteeId, out PartyInvite invite)
        {
            foreach (PartyInvite candidate in _invites)
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

        internal void AddMember(PartyMember member) => _members.Add(member);

        internal bool RemoveMember(string playerId) => _members.RemoveAll(m => m.PlayerId == playerId) > 0;

        internal void AddInvite(PartyInvite invite) => _invites.Add(invite);

        internal bool RemoveInvite(string inviteeId) => _invites.RemoveAll(i => i.InviteeId == inviteeId) > 0;

        internal List<string> RemoveInvitesWhere(Predicate<PartyInvite> match)
        {
            var removed = new List<string>();
            for (int i = _invites.Count - 1; i >= 0; i--)
            {
                if (match(_invites[i]))
                {
                    removed.Insert(0, _invites[i].InviteeId);
                    _invites.RemoveAt(i);
                }
            }

            return removed;
        }

        internal void Clear()
        {
            _members.Clear();
            _invites.Clear();
        }
    }
}
