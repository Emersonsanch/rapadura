using System.Collections.Generic;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Party
{
    /// <summary>
    /// The "Grupo" (party) of FASE 12.3 (TODO.md), MU Online style: invite / accept / decline / expire, leave, kick,
    /// pass the leadership, loot mode, at most <see cref="MaxMembers"/>. Pure C# (no Unity), deterministic and
    /// exception-free: every action returns a <see cref="PartyResult"/> with a <see cref="PartyRefusal"/>, so the future
    /// authoritative server runs this very file and the client only shows <see cref="PartyTexts.RefusalKey"/>.
    /// <list type="bullet">
    /// <item>Only the leader invites, kicks, passes the leadership and picks the loot mode.</item>
    /// <item>An invite lives <see cref="InviteSeconds"/>; a player belongs to one party at a time (the server answers
    /// <c>inviteeInAnyParty</c>).</item>
    /// <item>The leader leaving hands the leadership to the oldest remaining member (lowest <see cref="PartyMember.JoinOrder"/>).</item>
    /// <item>A party left with fewer than 2 members and no pending invite is disbanded (as in MU).</item>
    /// </list>
    /// Combat, XP and loot of the group: <see cref="PartyCombatRules"/>, <see cref="PartyXpRules"/>, <see cref="PartyLootRules"/>.
    /// </summary>
    public static class PartyRules
    {
        public const int MaxMembers = 5;

        /// <summary>Seconds an invite waits for an answer.</summary>
        public const double InviteSeconds = 60d;

        public static PartyResult Invite(PartyState party, string inviterId, string inviteeId, CharacterId? inviteeCousin,
            bool inviteeInAnyParty, double now)
        {
            if (party == null || party.IsDisbanded)
            {
                return PartyResult.Refused(PartyRefusal.Disbanded, party?.LeaderId);
            }

            if (string.IsNullOrEmpty(inviterId) || string.IsNullOrEmpty(inviteeId))
            {
                return PartyResult.Refused(PartyRefusal.InvalidPlayer, party.LeaderId);
            }

            if (!party.IsMember(inviterId))
            {
                return PartyResult.Refused(PartyRefusal.NotMember, party.LeaderId);
            }

            if (!party.IsLeader(inviterId))
            {
                return PartyResult.Refused(PartyRefusal.NotLeader, party.LeaderId);
            }

            if (inviterId == inviteeId)
            {
                return PartyResult.Refused(PartyRefusal.CannotTargetSelf, party.LeaderId);
            }

            if (party.IsMember(inviteeId) || inviteeInAnyParty)
            {
                return PartyResult.Refused(PartyRefusal.AlreadyInParty, party.LeaderId);
            }

            // A stale invite does not block a new one.
            ExpireInvites(party, now);
            if (party.TryGetInvite(inviteeId, out _))
            {
                return PartyResult.Refused(PartyRefusal.AlreadyInvited, party.LeaderId);
            }

            if (party.Count >= MaxMembers)
            {
                return PartyResult.Refused(PartyRefusal.PartyFull, party.LeaderId);
            }

            party.AddInvite(new PartyInvite(inviteeId, inviterId, inviteeCousin, now + InviteSeconds));
            return Done(party);
        }

        /// <summary>The invitee joins. Refused when there is no invite, it expired (and is dropped) or the party filled up meanwhile.</summary>
        public static PartyResult Accept(PartyState party, string inviteeId, double now)
        {
            if (party == null || party.IsDisbanded)
            {
                return PartyResult.Refused(PartyRefusal.Disbanded, party?.LeaderId);
            }

            if (string.IsNullOrEmpty(inviteeId))
            {
                return PartyResult.Refused(PartyRefusal.InvalidPlayer, party.LeaderId);
            }

            if (!party.TryGetInvite(inviteeId, out PartyInvite invite))
            {
                return PartyResult.Refused(party.IsMember(inviteeId) ? PartyRefusal.AlreadyInParty : PartyRefusal.NoInvite, party.LeaderId);
            }

            party.RemoveInvite(inviteeId);
            if (IsExpired(invite, now))
            {
                return Refuse(party, PartyRefusal.InviteExpired);
            }

            if (party.Count >= MaxMembers)
            {
                return Refuse(party, PartyRefusal.PartyFull);
            }

            party.AddMember(new PartyMember(inviteeId, invite.InviteeCousin, party.NextJoinOrder++));
            return Done(party);
        }

        /// <summary>The invitee says no. A forming party (only the leader) with no other invite is then disbanded.</summary>
        public static PartyResult Decline(PartyState party, string inviteeId)
        {
            if (party == null || party.IsDisbanded)
            {
                return PartyResult.Refused(PartyRefusal.Disbanded, party?.LeaderId);
            }

            if (string.IsNullOrEmpty(inviteeId))
            {
                return PartyResult.Refused(PartyRefusal.InvalidPlayer, party.LeaderId);
            }

            if (!party.RemoveInvite(inviteeId))
            {
                return PartyResult.Refused(PartyRefusal.NoInvite, party.LeaderId);
            }

            return Done(party);
        }

        /// <summary>Drops every invite past its time (server tick) and returns the invitee ids, in invite order.</summary>
        public static List<string> ExpireInvites(PartyState party, double now)
        {
            if (party == null)
            {
                return new List<string>();
            }

            List<string> expired = party.RemoveInvitesWhere(invite => IsExpired(invite, now));
            if (expired.Count > 0)
            {
                DisbandIfEmpty(party);
            }

            return expired;
        }

        public static bool IsExpired(PartyInvite invite, double now) => now >= invite.ExpiresAt;

        /// <summary>A member leaves. If it was the leader, the oldest remaining member takes over.</summary>
        public static PartyResult Leave(PartyState party, string playerId)
        {
            if (party == null || party.IsDisbanded)
            {
                return PartyResult.Refused(PartyRefusal.Disbanded, party?.LeaderId);
            }

            if (string.IsNullOrEmpty(playerId))
            {
                return PartyResult.Refused(PartyRefusal.InvalidPlayer, party.LeaderId);
            }

            if (!party.RemoveMember(playerId))
            {
                return PartyResult.Refused(PartyRefusal.NotMember, party.LeaderId);
            }

            if (party.LeaderId == playerId)
            {
                party.LeaderId = OldestMember(party)?.PlayerId;
            }

            return Done(party);
        }

        /// <summary>The leader removes <paramref name="targetId"/> (never themself: that is <see cref="Leave"/>).</summary>
        public static PartyResult Kick(PartyState party, string leaderId, string targetId)
        {
            PartyRefusal check = CheckLeaderOnTarget(party, leaderId, targetId);
            if (check != PartyRefusal.None)
            {
                return PartyResult.Refused(check, party?.LeaderId);
            }

            party.RemoveMember(targetId);
            return Done(party);
        }

        /// <summary>The leader hands the leadership to another member.</summary>
        public static PartyResult PassLeadership(PartyState party, string leaderId, string targetId)
        {
            PartyRefusal check = CheckLeaderOnTarget(party, leaderId, targetId);
            if (check != PartyRefusal.None)
            {
                return PartyResult.Refused(check, party?.LeaderId);
            }

            party.LeaderId = targetId;
            return Done(party);
        }

        /// <summary>The leader picks how drops are shared (<see cref="PartyLootRules"/>).</summary>
        public static PartyResult SetLootMode(PartyState party, string leaderId, PartyLootMode mode)
        {
            if (party == null || party.IsDisbanded)
            {
                return PartyResult.Refused(PartyRefusal.Disbanded, party?.LeaderId);
            }

            if (!party.IsMember(leaderId))
            {
                return PartyResult.Refused(string.IsNullOrEmpty(leaderId) ? PartyRefusal.InvalidPlayer : PartyRefusal.NotMember, party.LeaderId);
            }

            if (!party.IsLeader(leaderId))
            {
                return PartyResult.Refused(PartyRefusal.NotLeader, party.LeaderId);
            }

            party.LootMode = mode;
            return Done(party);
        }

        /// <summary>The member with the lowest join order (the leader inherits from the oldest), or null when empty.</summary>
        public static PartyMember OldestMember(PartyState party)
        {
            PartyMember oldest = null;
            if (party == null)
            {
                return null;
            }

            foreach (PartyMember member in party.Members)
            {
                if (oldest == null || member.JoinOrder < oldest.JoinOrder)
                {
                    oldest = member;
                }
            }

            return oldest;
        }

        /// <summary>Two players are allies (no friendly fire, shared XP/loot) when both are members of the same live party.</summary>
        public static bool AreInSameParty(PartyState party, string a, string b) =>
            party != null && !party.IsDisbanded && party.IsMember(a) && party.IsMember(b);

        private static PartyRefusal CheckLeaderOnTarget(PartyState party, string leaderId, string targetId)
        {
            if (party == null || party.IsDisbanded)
            {
                return PartyRefusal.Disbanded;
            }

            if (string.IsNullOrEmpty(leaderId) || string.IsNullOrEmpty(targetId))
            {
                return PartyRefusal.InvalidPlayer;
            }

            if (!party.IsMember(leaderId))
            {
                return PartyRefusal.NotMember;
            }

            if (!party.IsLeader(leaderId))
            {
                return PartyRefusal.NotLeader;
            }

            if (leaderId == targetId)
            {
                return PartyRefusal.CannotTargetSelf;
            }

            return party.IsMember(targetId) ? PartyRefusal.None : PartyRefusal.TargetNotMember;
        }

        private static PartyResult Refuse(PartyState party, PartyRefusal reason)
        {
            bool disbanded = DisbandIfEmpty(party);
            return new PartyResult(reason, disbanded, party.LeaderId);
        }

        private static PartyResult Done(PartyState party)
        {
            bool disbanded = DisbandIfEmpty(party);
            return new PartyResult(PartyRefusal.None, disbanded, party.LeaderId);
        }

        /// <summary>Fewer than 2 members and nobody left to answer an invite: the party ends.</summary>
        private static bool DisbandIfEmpty(PartyState party)
        {
            if (party.IsDisbanded || party.Count >= 2 || (party.Count == 1 && party.Invites.Count > 0))
            {
                return party.IsDisbanded;
            }

            party.Clear();
            party.LeaderId = null;
            party.IsDisbanded = true;
            return true;
        }
    }
}
