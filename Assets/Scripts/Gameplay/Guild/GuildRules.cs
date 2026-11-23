using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Guild
{
    /// <summary>Outcome of <see cref="GuildRules.Create"/>: the new guild, or null with the refusal.</summary>
    public readonly struct GuildCreateResult
    {
        public readonly GuildRefusal Refusal;
        public readonly GuildState Guild;

        /// <summary>Gold the server takes from the founder (0 when refused).</summary>
        public readonly long GoldCost;

        public GuildCreateResult(GuildRefusal refusal, GuildState guild, long goldCost)
        {
            Refusal = refusal;
            Guild = guild;
            GoldCost = goldCost;
        }

        public bool Ok => Refusal == GuildRefusal.None;
    }

    /// <summary>
    /// The guild ("clã") of FASE 12.3 (TODO.md): founding, ranks and permissions (<see cref="GuildPermissions"/>), invite /
    /// accept / decline / expire, leave, kick, promote, demote, leadership transfer and the inactive leader. Pure C#,
    /// deterministic and exception-free like <c>PartyRules</c>: every action returns a <see cref="GuildResult"/> and the
    /// client only shows <see cref="GuildTexts.RefusalKey"/>. Time is server seconds (<paramref name="now"/>).
    /// <list type="bullet">
    /// <item>Founding: level <see cref="MinFounderLevel"/>, <see cref="CreationCost"/> gold (a gold sink), a valid unique
    /// name, motto and emblem. The founder is the Leader.</item>
    /// <item>New members come in as Recruit. A player is in one guild at a time, and whoever leaves (or is kicked) waits
    /// <see cref="RejoinCooldownSeconds"/> before joining or founding another (no guild hopping around wars and banks).</item>
    /// <item>Nobody kicks, promotes or demotes a rank equal to or above their own; a promotion stops one rank below the actor.
    /// At most <see cref="MaxVices"/> Vices and <see cref="MaxOfficers"/> Officers.</item>
    /// <item>The leader leaving: the successor is the highest rank, then the oldest member (<see cref="Successor"/>); the last
    /// member leaving disbands the guild.</item>
    /// <item>Leader inactive <see cref="LeaderInactiveDays"/> days (<see cref="CheckLeaderInactivity"/>, server's daily tick):
    /// the leadership passes to the highest-ranked, then oldest, non-Recruit member active in that window; the old leader
    /// stays as a Member. With nobody active, nothing changes.</item>
    /// </list>
    /// </summary>
    public static class GuildRules
    {
        public const int MinFounderLevel = 30;
        public const long CreationCost = 5000;
        public const int MaxMottoLength = 60;
        public const int MaxVices = 2;
        public const int MaxOfficers = 8;

        /// <summary>Seconds an invite waits for an answer (5 min).</summary>
        public const double InviteSeconds = 300d;

        /// <summary>After leaving / being kicked, 24 h before joining or founding a guild.</summary>
        public const double RejoinCooldownSeconds = 24d * 3600d;

        public const int LeaderInactiveDays = 30;
        public const double SecondsPerDay = 86400d;

        /// <summary>The server's UTC day of <paramref name="now"/>: daily limits reset when it changes.</summary>
        public static long DayIndex(double now) => (long)Math.Floor(now / SecondsPerDay);

        /// <param name="secondsSinceLeftGuild">Since the founder last left a guild; negative = never.</param>
        /// <param name="isNameKeyTaken">Whether a live guild already has that <see cref="GuildNameRules.NameKey"/> (server's index).</param>
        public static GuildCreateResult Create(string guildId, string founderId, int founderLevel, long founderGold, bool founderInAnyGuild,
            double secondsSinceLeftGuild, string name, string motto, GuildEmblem emblem, Func<string, bool> isNameKeyTaken, double now)
        {
            if (string.IsNullOrEmpty(guildId) || string.IsNullOrEmpty(founderId))
            {
                return Fail(GuildRefusal.InvalidPlayer);
            }

            if (founderInAnyGuild)
            {
                return Fail(GuildRefusal.AlreadyInGuild);
            }

            if (InCooldown(secondsSinceLeftGuild))
            {
                return Fail(GuildRefusal.RejoinCooldown);
            }

            if (founderLevel < MinFounderLevel)
            {
                return Fail(GuildRefusal.LevelTooLow);
            }

            if (founderGold < CreationCost)
            {
                return Fail(GuildRefusal.NotEnoughGold);
            }

            GuildRefusal nameCheck = GuildNameRules.ValidateName(name);
            if (nameCheck != GuildRefusal.None)
            {
                return Fail(nameCheck);
            }

            string cleanName = GuildNameRules.Clean(name);
            if (isNameKeyTaken != null && isNameKeyTaken(GuildNameRules.NameKey(cleanName)))
            {
                return Fail(GuildRefusal.NameTaken);
            }

            GuildRefusal mottoCheck = GuildNameRules.ValidateText(motto, MaxMottoLength, allowEmpty: true);
            if (mottoCheck != GuildRefusal.None)
            {
                return Fail(mottoCheck);
            }

            if (!emblem.Validate())
            {
                return Fail(GuildRefusal.EmblemInvalid);
            }

            var guild = new GuildState(guildId, cleanName, GuildNameRules.Clean(motto), emblem, founderId, now);
            return new GuildCreateResult(GuildRefusal.None, guild, CreationCost);
        }

        /// <param name="inviteeInAnyGuild">The invitee belongs to a guild (server's index).</param>
        /// <param name="inviteeSecondsSinceLeftGuild">Negative = never left one.</param>
        /// <param name="inviteeBlocks">The invitee's block list (<see cref="Social.PlayerBlockRules"/>); null = blocks nobody. Blocked →
        /// <see cref="GuildRefusal.InvalidPlayer"/>, the same answer as a missing player, so the block stays private.</param>
        public static GuildResult Invite(GuildState guild, string inviterId, string inviteeId, bool inviteeInAnyGuild,
            double inviteeSecondsSinceLeftGuild, double now, Social.PlayerBlockList inviteeBlocks = null)
        {
            GuildRefusal check = CheckActor(guild, inviterId, GuildPermission.Invite, out _);
            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            if (string.IsNullOrEmpty(inviteeId))
            {
                return Refused(guild, GuildRefusal.InvalidPlayer);
            }

            if (inviterId == inviteeId)
            {
                return Refused(guild, GuildRefusal.CannotTargetSelf);
            }

            if (!Social.PlayerBlockRules.CanContact(inviteeBlocks, inviterId, Social.SocialContactKind.GuildInvite))
            {
                return Refused(guild, GuildRefusal.InvalidPlayer);
            }

            if (guild.IsMember(inviteeId) || inviteeInAnyGuild)
            {
                return Refused(guild, GuildRefusal.AlreadyInGuild);
            }

            if (InCooldown(inviteeSecondsSinceLeftGuild))
            {
                return Refused(guild, GuildRefusal.RejoinCooldown);
            }

            ExpireInvites(guild, now);
            if (guild.TryGetInvite(inviteeId, out _))
            {
                return Refused(guild, GuildRefusal.AlreadyInvited);
            }

            if (guild.Count + guild.Invites.Count >= GuildLevelRules.MaxMembers(guild.Level))
            {
                return Refused(guild, GuildRefusal.GuildFull);
            }

            guild.AddInvite(new GuildInvite(inviteeId, inviterId, now + InviteSeconds));
            return Done(guild);
        }

        /// <summary>The invitee joins as Recruit. Refused with no invite, an expired one (then dropped) or a guild filled meanwhile.</summary>
        public static GuildResult Accept(GuildState guild, string inviteeId, double now)
        {
            if (guild == null || guild.IsDisbanded)
            {
                return Refused(guild, GuildRefusal.Disbanded);
            }

            if (string.IsNullOrEmpty(inviteeId))
            {
                return Refused(guild, GuildRefusal.InvalidPlayer);
            }

            if (!guild.TryGetInvite(inviteeId, out GuildInvite invite))
            {
                return Refused(guild, guild.IsMember(inviteeId) ? GuildRefusal.AlreadyInGuild : GuildRefusal.NoInvite);
            }

            guild.RemoveInvite(inviteeId);
            if (now >= invite.ExpiresAt)
            {
                return Refused(guild, GuildRefusal.InviteExpired);
            }

            if (guild.Count >= GuildLevelRules.MaxMembers(guild.Level))
            {
                return Refused(guild, GuildRefusal.GuildFull);
            }

            guild.AddMember(new GuildMember(inviteeId, GuildRank.Recruit, guild.NextJoinOrder++, now));
            return Done(guild);
        }

        public static GuildResult Decline(GuildState guild, string inviteeId)
        {
            if (guild == null || guild.IsDisbanded)
            {
                return Refused(guild, GuildRefusal.Disbanded);
            }

            if (string.IsNullOrEmpty(inviteeId))
            {
                return Refused(guild, GuildRefusal.InvalidPlayer);
            }

            return guild.RemoveInvite(inviteeId) ? Done(guild) : Refused(guild, GuildRefusal.NoInvite);
        }

        /// <summary>Drops every invite past its time (server tick); returns the invitee ids in invite order.</summary>
        public static List<string> ExpireInvites(GuildState guild, double now) =>
            guild == null ? new List<string>() : guild.RemoveInvitesWhere(invite => now >= invite.ExpiresAt);

        /// <summary>A member leaves (the server starts their <see cref="RejoinCooldownSeconds"/>). The leader leaving hands over to <see cref="Successor"/>.</summary>
        public static GuildResult Leave(GuildState guild, string playerId)
        {
            if (guild == null || guild.IsDisbanded)
            {
                return Refused(guild, GuildRefusal.Disbanded);
            }

            if (string.IsNullOrEmpty(playerId))
            {
                return Refused(guild, GuildRefusal.InvalidPlayer);
            }

            if (!guild.RemoveMember(playerId))
            {
                return Refused(guild, GuildRefusal.NotMember);
            }

            if (guild.Count == 0)
            {
                guild.Disband();
                return new GuildResult(GuildRefusal.None, true, null);
            }

            if (guild.LeaderId == playerId)
            {
                GuildMember heir = Successor(guild, null, member => true);
                heir.Rank = GuildRank.Leader;
                guild.LeaderId = heir.PlayerId;
            }

            return Done(guild);
        }

        /// <summary>Removes <paramref name="targetId"/>, whose rank must be below the actor's (the server starts their cooldown).</summary>
        public static GuildResult Kick(GuildState guild, string actorId, string targetId)
        {
            GuildRefusal check = CheckActorOnTarget(guild, actorId, targetId, GuildPermission.Kick, out _, out _);
            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            guild.RemoveMember(targetId);
            return Done(guild);
        }

        /// <summary>Raises <paramref name="targetId"/> to <paramref name="newRank"/>: above their rank, below the actor's, never Leader.</summary>
        public static GuildResult Promote(GuildState guild, string actorId, string targetId, GuildRank newRank)
        {
            GuildRefusal check = CheckActorOnTarget(guild, actorId, targetId, GuildPermission.Promote, out GuildMember actor, out GuildMember target);
            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            if (!Enum.IsDefined(typeof(GuildRank), newRank) || newRank == GuildRank.Leader || newRank <= target.Rank)
            {
                return Refused(guild, GuildRefusal.InvalidRank);
            }

            if (newRank >= actor.Rank)
            {
                return Refused(guild, GuildRefusal.TargetRankTooHigh);
            }

            if (IsRankFull(guild, newRank))
            {
                return Refused(guild, GuildRefusal.RankFull);
            }

            target.Rank = newRank;
            return Done(guild);
        }

        /// <summary>Lowers <paramref name="targetId"/> (rank below the actor's) to <paramref name="newRank"/>, any rank below theirs.</summary>
        public static GuildResult Demote(GuildState guild, string actorId, string targetId, GuildRank newRank)
        {
            GuildRefusal check = CheckActorOnTarget(guild, actorId, targetId, GuildPermission.Demote, out _, out GuildMember target);
            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            if (!Enum.IsDefined(typeof(GuildRank), newRank) || newRank >= target.Rank)
            {
                return Refused(guild, GuildRefusal.InvalidRank);
            }

            target.Rank = newRank;
            return Done(guild);
        }

        /// <summary>
        /// The leader hands the guild to another non-Recruit member. The old leader becomes Vice, or Officer when the Vices are
        /// full (the new leader's old seat counts as free).
        /// </summary>
        public static GuildResult TransferLeadership(GuildState guild, string leaderId, string targetId)
        {
            GuildRefusal check = CheckActor(guild, leaderId, GuildPermission.None, out GuildMember leader);
            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            if (leader.Rank != GuildRank.Leader)
            {
                return Refused(guild, GuildRefusal.NoPermission);
            }

            if (string.IsNullOrEmpty(targetId))
            {
                return Refused(guild, GuildRefusal.InvalidPlayer);
            }

            if (leaderId == targetId)
            {
                return Refused(guild, GuildRefusal.CannotTargetSelf);
            }

            GuildMember target = guild.Find(targetId);
            if (target == null)
            {
                return Refused(guild, GuildRefusal.TargetNotMember);
            }

            if (target.Rank == GuildRank.Recruit)
            {
                return Refused(guild, GuildRefusal.InvalidRank);
            }

            target.Rank = GuildRank.Leader;
            leader.Rank = guild.CountOfRank(GuildRank.Vice) < MaxVices ? GuildRank.Vice : GuildRank.Officer;
            guild.LeaderId = targetId;
            return Done(guild);
        }

        /// <summary>Marks <paramref name="playerId"/> active (login, any guild action). Unknown ids are ignored.</summary>
        public static void Touch(GuildState guild, string playerId, double now)
        {
            GuildMember member = guild?.Find(playerId);
            if (member != null && now > member.LastActiveAt)
            {
                member.LastActiveAt = now;
            }
        }

        public static bool IsInactive(GuildMember member, double now) =>
            member != null && now - member.LastActiveAt >= LeaderInactiveDays * SecondsPerDay;

        /// <summary>
        /// Server's daily tick: a leader inactive <see cref="LeaderInactiveDays"/> days loses the guild to the highest-ranked,
        /// then oldest, non-Recruit member active in that window, and stays as a Member. <see cref="GuildResult.LeaderId"/>
        /// tells whether it changed.
        /// </summary>
        public static GuildResult CheckLeaderInactivity(GuildState guild, double now)
        {
            if (guild == null || guild.IsDisbanded)
            {
                return Refused(guild, GuildRefusal.Disbanded);
            }

            GuildMember leader = guild.Find(guild.LeaderId);
            if (leader == null || !IsInactive(leader, now))
            {
                return Done(guild);
            }

            GuildMember heir = Successor(guild, leader.PlayerId, member => member.Rank != GuildRank.Recruit && !IsInactive(member, now));
            if (heir == null)
            {
                return Done(guild);
            }

            leader.Rank = GuildRank.Member;
            heir.Rank = GuildRank.Leader;
            guild.LeaderId = heir.PlayerId;
            return Done(guild);
        }

        /// <summary>Highest rank first, then the oldest (lowest join order), among the members passing <paramref name="eligible"/>.</summary>
        public static GuildMember Successor(GuildState guild, string excludeId, Func<GuildMember, bool> eligible)
        {
            GuildMember best = null;
            if (guild == null)
            {
                return null;
            }

            foreach (GuildMember member in guild.Members)
            {
                if (member.PlayerId == excludeId || (eligible != null && !eligible(member)))
                {
                    continue;
                }

                if (best == null || member.Rank > best.Rank || (member.Rank == best.Rank && member.JoinOrder < best.JoinOrder))
                {
                    best = member;
                }
            }

            return best;
        }

        public static GuildResult SetMotto(GuildState guild, string actorId, string motto)
        {
            GuildRefusal check = CheckActor(guild, actorId, GuildPermission.EditIdentity, out _);
            if (check == GuildRefusal.None)
            {
                check = GuildNameRules.ValidateText(motto, MaxMottoLength, allowEmpty: true);
            }

            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            guild.Motto = GuildNameRules.Clean(motto);
            return Done(guild);
        }

        public static GuildResult SetEmblem(GuildState guild, string actorId, GuildEmblem emblem)
        {
            GuildRefusal check = CheckActor(guild, actorId, GuildPermission.EditIdentity, out _);
            if (check == GuildRefusal.None && !emblem.Validate())
            {
                check = GuildRefusal.EmblemInvalid;
            }

            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            guild.Emblem = emblem;
            return Done(guild);
        }

        /// <summary>The leader ends the guild. Needs an empty bank (withdraw first: it is logged) and no declared or running war.</summary>
        public static GuildResult Disband(GuildState guild, string leaderId, bool hasOpenWar)
        {
            GuildRefusal check = CheckActor(guild, leaderId, GuildPermission.Disband, out _);
            if (check != GuildRefusal.None)
            {
                return Refused(guild, check);
            }

            if (guild.BankGold > 0 || guild.BankItems.Count > 0)
            {
                return Refused(guild, GuildRefusal.BankNotEmpty);
            }

            if (hasOpenWar)
            {
                return Refused(guild, GuildRefusal.AtWar);
            }

            guild.Disband();
            return new GuildResult(GuildRefusal.None, true, null);
        }

        public static bool IsRankFull(GuildState guild, GuildRank rank)
        {
            switch (rank)
            {
                case GuildRank.Vice: return guild.CountOfRank(GuildRank.Vice) >= MaxVices;
                case GuildRank.Officer: return guild.CountOfRank(GuildRank.Officer) >= MaxOfficers;
                case GuildRank.Leader: return guild.CountOfRank(GuildRank.Leader) >= 1;
                default: return false;
            }
        }

        public static bool AreInSameGuild(GuildState guild, string a, string b) =>
            guild != null && !guild.IsDisbanded && guild.IsMember(a) && guild.IsMember(b);

        /// <summary>Common gate: live guild, valid id, a member, and (unless <see cref="GuildPermission.None"/>) the permission.</summary>
        internal static GuildRefusal CheckActor(GuildState guild, string actorId, GuildPermission permission, out GuildMember actor)
        {
            actor = null;
            if (guild == null || guild.IsDisbanded)
            {
                return GuildRefusal.Disbanded;
            }

            if (string.IsNullOrEmpty(actorId))
            {
                return GuildRefusal.InvalidPlayer;
            }

            actor = guild.Find(actorId);
            if (actor == null)
            {
                return GuildRefusal.NotMember;
            }

            return permission == GuildPermission.None || GuildPermissions.Has(actor.Rank, permission) ? GuildRefusal.None : GuildRefusal.NoPermission;
        }

        private static GuildRefusal CheckActorOnTarget(GuildState guild, string actorId, string targetId, GuildPermission permission,
            out GuildMember actor, out GuildMember target)
        {
            target = null;
            GuildRefusal check = CheckActor(guild, actorId, permission, out actor);
            if (check != GuildRefusal.None)
            {
                return check;
            }

            if (string.IsNullOrEmpty(targetId))
            {
                return GuildRefusal.InvalidPlayer;
            }

            if (actorId == targetId)
            {
                return GuildRefusal.CannotTargetSelf;
            }

            target = guild.Find(targetId);
            if (target == null)
            {
                return GuildRefusal.TargetNotMember;
            }

            return target.Rank >= actor.Rank ? GuildRefusal.TargetRankTooHigh : GuildRefusal.None;
        }

        private static bool InCooldown(double secondsSinceLeftGuild) =>
            secondsSinceLeftGuild >= 0d && secondsSinceLeftGuild < RejoinCooldownSeconds;

        private static GuildCreateResult Fail(GuildRefusal reason) => new GuildCreateResult(reason, null, 0);

        internal static GuildResult Refused(GuildState guild, GuildRefusal reason) => GuildResult.Refused(reason, guild?.LeaderId);

        internal static GuildResult Done(GuildState guild, GuildBankLogEntry entry = null) =>
            new GuildResult(GuildRefusal.None, guild.IsDisbanded, guild.LeaderId, entry);
    }
}
