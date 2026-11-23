using System;
using System.Globalization;
using System.Text;

namespace Rapadura.Gameplay.Social
{
    /// <summary>The staff role of an account; a higher value outranks a lower one. Never renumber.</summary>
    public enum StaffRole
    {
        Player = 0,
        Moderator = 1,
        GameMaster = 2,
    }

    /// <summary>The staff commands. Never renumber (the value goes into the moderation log).</summary>
    public enum GmCommandKind
    {
        Mute = 0,
        Unmute = 1,

        /// <summary>Disconnects the player now; they may log in again.</summary>
        Kick = 2,

        Ban = 3,
        Unban = 4,
        Teleport = 5,

        /// <summary>Read a log (chat, reports, trades, moderation).</summary>
        QueryLog = 6,
    }

    public enum GmTeleportMode
    {
        /// <summary>The staff member goes to where the target is.</summary>
        GoToPlayer = 0,

        /// <summary>The target comes to where the staff member is.</summary>
        BringPlayer = 1,

        /// <summary>The target (or the staff member, with no target) goes to a map and a point.</summary>
        ToPosition = 2,
    }

    public enum GmLogKind
    {
        Chat = 0,
        Reports = 1,
        Trade = 2,
        Moderation = 3,
    }

    /// <summary>Why a staff command was refused. Texts: <see cref="SocialTexts.GmRefusalKey"/>. Never renumber.</summary>
    public enum GmRefusal
    {
        None = 0,

        /// <summary>The account has no staff role.</summary>
        NotStaff = 1,

        /// <summary>The role may not use this command (or this log).</summary>
        NoPermission = 2,

        /// <summary>The command needs a target and has none.</summary>
        InvalidTarget = 3,

        CannotTargetSelf = 4,

        /// <summary>The target's staff role is equal to or above the actor's.</summary>
        TargetOutranks = 5,

        /// <summary>Mute, kick and ban need a reason of at least <see cref="GmRules.MinReasonLength"/> characters.</summary>
        ReasonRequired = 6,

        ReasonTooLong = 7,

        /// <summary>Zero, negative or below the minimum.</summary>
        InvalidDuration = 8,

        /// <summary>Above the role's maximum (or a permanent ban by someone who may not).</summary>
        DurationTooLong = 9,

        /// <summary>No map, or a coordinate that is not a finite number.</summary>
        InvalidPosition = 10,

        /// <summary>A log query with the end before the start, or a bad limit.</summary>
        InvalidLogQuery = 11,

        /// <summary>A log query over more than <see cref="GmRules.MaxLogSpanSeconds"/>.</summary>
        LogRangeTooLong = 12,

        UnknownCommand = 13,
    }

    /// <summary>
    /// A staff command as the client asks for it; the server fills <see cref="ActorRole"/> and <see cref="TargetRole"/> from
    /// its own records (never from the client) before <see cref="GmRules.TryIssue"/>.
    /// </summary>
    public sealed class GmCommand
    {
        public GmCommandKind Kind;
        public string ActorId;
        public StaffRole ActorRole;
        public string TargetId;
        public StaffRole TargetRole;

        /// <summary>Mute and ban length in seconds.</summary>
        public long DurationSeconds;

        /// <summary>A ban with no end (game master only).</summary>
        public bool Permanent;

        public string Reason;

        public GmTeleportMode TeleportMode;
        public string MapId;
        public float X;
        public float Y;
        public float Z;

        public GmLogKind LogKind;
        public long LogFromUnix;
        public long LogToUnix;
        public int LogLimit = GmRules.DefaultLogLimit;
    }

    /// <summary>
    /// One staff command, issued or refused, as plain serializable data for the moderation log (refused attempts are logged
    /// too: they are how a stolen staff account shows). The server executes only a record with <see cref="refusal"/> = None.
    /// </summary>
    [Serializable]
    public sealed class GmCommandRecord
    {
        public string commandId;

        /// <summary>UTC unix seconds.</summary>
        public long atUnix;

        public GmCommandKind kind;
        public string actorId;
        public StaffRole actorRole;
        public string targetId;
        public long durationSeconds;
        public bool permanent;

        /// <summary>When a mute or ban ends (UTC unix seconds); 0 = no end (permanent) or not a timed command.</summary>
        public long expiresAtUnix;

        public string reason;
        public GmTeleportMode teleportMode;
        public string mapId;
        public float x;
        public float y;
        public float z;
        public GmLogKind logKind;
        public long logFromUnix;
        public long logToUnix;
        public int logLimit;
        public GmRefusal refusal;

        public bool Ok => refusal == GmRefusal.None;

        /// <summary>
        /// One line for an append-only log:
        /// <c>gm=&lt;id&gt; at=&lt;unix&gt; by=&lt;actor&gt;(game_master) cmd=ban on=&lt;target&gt; dur=86400 until=&lt;unix&gt; ok=1 reason="..."</c>,
        /// plus <c>tp=</c> for a teleport and <c>log=</c> for a query.
        /// </summary>
        public string ToLogLine()
        {
            var sb = new StringBuilder(128);
            sb.Append("gm=").Append(SocialLog.Token(commandId));
            sb.Append(" at=").Append(atUnix.ToString(CultureInfo.InvariantCulture));
            sb.Append(" by=").Append(SocialLog.Token(actorId)).Append('(').Append(SocialLog.Snake(actorRole.ToString())).Append(')');
            sb.Append(" cmd=").Append(SocialLog.Snake(kind.ToString()));
            sb.Append(" on=").Append(SocialLog.Token(targetId));
            if (kind == GmCommandKind.Mute || kind == GmCommandKind.Ban)
            {
                sb.Append(" dur=").Append(permanent ? "permanent" : durationSeconds.ToString(CultureInfo.InvariantCulture));
                sb.Append(" until=").Append(expiresAtUnix.ToString(CultureInfo.InvariantCulture));
            }

            if (kind == GmCommandKind.Teleport)
            {
                sb.Append(" tp=").Append(SocialLog.Snake(teleportMode.ToString()));
                if (teleportMode == GmTeleportMode.ToPosition)
                {
                    sb.Append(':').Append(SocialLog.Token(mapId)).Append('@')
                        .Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                        .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(',')
                        .Append(z.ToString("0.##", CultureInfo.InvariantCulture));
                }
            }

            if (kind == GmCommandKind.QueryLog)
            {
                sb.Append(" log=").Append(SocialLog.Snake(logKind.ToString())).Append(':')
                    .Append(logFromUnix.ToString(CultureInfo.InvariantCulture)).Append('-')
                    .Append(logToUnix.ToString(CultureInfo.InvariantCulture)).Append('x')
                    .Append(logLimit.ToString(CultureInfo.InvariantCulture));
            }

            sb.Append(" ok=").Append(Ok ? '1' : '0');
            if (!Ok)
            {
                sb.Append(" refusal=").Append(SocialLog.Snake(refusal.ToString()));
            }

            sb.Append(" reason=").Append(SocialLog.Quote(reason));
            return sb.ToString();
        }
    }

    /// <summary>
    /// The staff tools of FASE 12.3 (TODO.md, "Moderação: ... ferramentas de GM (teleporte, banimento, logs)"): only the
    /// permission check and the record; the server executes. Pure C# and exception-free. Decisions (2026-11-23):
    /// <list type="table">
    /// <item><term>Moderador</term><description>mute (1 min to 24 h), unmute, kick, and read the chat and report logs.</description></item>
    /// <item><term>Game master</term><description>all of that, mute up to 7 days, ban (1 h to 30 days, or permanent), unban,
    /// teleport (go to a player, bring a player, send to a map point) and read every log.</description></item>
    /// </list>
    /// Nobody mutes, kicks, bans, brings or sends someone of the same or a higher role, nor themself. Mute, kick and ban need a
    /// reason (it shows to the player and in the log). A log query covers at most 7 days and 500 lines.
    /// </summary>
    public static class GmRules
    {
        public const long MinMuteSeconds = 60;
        public const long ModeratorMaxMuteSeconds = 24 * 3600;
        public const long GameMasterMaxMuteSeconds = 7 * 24 * 3600;
        public const long MinBanSeconds = 3600;
        public const long MaxBanSeconds = 30L * 24 * 3600;
        public const int MinReasonLength = 3;
        public const int MaxReasonLength = 200;
        public const long MaxLogSpanSeconds = 7 * 24 * 3600;
        public const int DefaultLogLimit = 100;
        public const int MaxLogLimit = 500;

        /// <summary>Whether <paramref name="role"/> may use <paramref name="kind"/> at all.</summary>
        public static bool Can(StaffRole role, GmCommandKind kind)
        {
            switch (role)
            {
                case StaffRole.GameMaster:
                    return Enum.IsDefined(typeof(GmCommandKind), kind);
                case StaffRole.Moderator:
                    return kind == GmCommandKind.Mute || kind == GmCommandKind.Unmute || kind == GmCommandKind.Kick
                        || kind == GmCommandKind.QueryLog;
                default:
                    return false;
            }
        }

        /// <summary>Whether <paramref name="role"/> may read <paramref name="log"/>.</summary>
        public static bool CanRead(StaffRole role, GmLogKind log)
        {
            switch (role)
            {
                case StaffRole.GameMaster:
                    return true;
                case StaffRole.Moderator:
                    return log == GmLogKind.Chat || log == GmLogKind.Reports;
                default:
                    return false;
            }
        }

        public static long MaxMuteSeconds(StaffRole role) =>
            role == StaffRole.GameMaster ? GameMasterMaxMuteSeconds : role == StaffRole.Moderator ? ModeratorMaxMuteSeconds : 0;

        /// <summary>The check alone (see the class summary).</summary>
        public static GmRefusal Validate(GmCommand command)
        {
            if (command == null)
            {
                return GmRefusal.UnknownCommand;
            }

            if (string.IsNullOrEmpty(command.ActorId) || command.ActorRole <= StaffRole.Player || !Enum.IsDefined(typeof(StaffRole), command.ActorRole))
            {
                return GmRefusal.NotStaff;
            }

            if (!Enum.IsDefined(typeof(GmCommandKind), command.Kind))
            {
                return GmRefusal.UnknownCommand;
            }

            if (!Can(command.ActorRole, command.Kind))
            {
                return GmRefusal.NoPermission;
            }

            switch (command.Kind)
            {
                case GmCommandKind.Mute:
                case GmCommandKind.Kick:
                case GmCommandKind.Ban:
                {
                    GmRefusal target = CheckTarget(command, true);
                    if (target != GmRefusal.None)
                    {
                        return target;
                    }

                    GmRefusal reason = CheckReason(command.Reason, true);
                    if (reason != GmRefusal.None)
                    {
                        return reason;
                    }

                    if (command.Kind == GmCommandKind.Mute)
                    {
                        return CheckDuration(command.DurationSeconds, MinMuteSeconds, MaxMuteSeconds(command.ActorRole));
                    }

                    if (command.Kind == GmCommandKind.Ban && !command.Permanent)
                    {
                        return CheckDuration(command.DurationSeconds, MinBanSeconds, MaxBanSeconds);
                    }

                    return GmRefusal.None;
                }

                case GmCommandKind.Unmute:
                case GmCommandKind.Unban:
                {
                    if (string.IsNullOrEmpty(command.TargetId))
                    {
                        return GmRefusal.InvalidTarget;
                    }

                    return CheckReason(command.Reason, false);
                }

                case GmCommandKind.Teleport:
                    return CheckTeleport(command);

                case GmCommandKind.QueryLog:
                    return CheckLogQuery(command);

                default:
                    return GmRefusal.UnknownCommand;
            }
        }

        /// <summary>
        /// Validates <paramref name="command"/> and always returns the record for the moderation log (with the refusal when
        /// refused). <paramref name="nowUnix"/> is the server's UTC clock; mute and ban get their <see cref="GmCommandRecord.expiresAtUnix"/>.
        /// </summary>
        public static GmRefusal TryIssue(GmCommand command, string commandId, long nowUnix, out GmCommandRecord record)
        {
            GmRefusal refusal = Validate(command);
            record = new GmCommandRecord
            {
                commandId = commandId,
                atUnix = nowUnix,
                refusal = refusal,
            };

            if (command == null)
            {
                return refusal;
            }

            record.kind = command.Kind;
            record.actorId = command.ActorId;
            record.actorRole = command.ActorRole;
            record.targetId = command.TargetId;
            record.durationSeconds = command.DurationSeconds;
            record.permanent = command.Kind == GmCommandKind.Ban && command.Permanent;
            record.reason = ChatFilter.Clean(command.Reason);
            record.teleportMode = command.TeleportMode;
            record.mapId = command.MapId;
            record.x = command.X;
            record.y = command.Y;
            record.z = command.Z;
            record.logKind = command.LogKind;
            record.logFromUnix = command.LogFromUnix;
            record.logToUnix = command.LogToUnix;
            record.logLimit = command.LogLimit;
            if (refusal == GmRefusal.None && (command.Kind == GmCommandKind.Mute || (command.Kind == GmCommandKind.Ban && !command.Permanent)))
            {
                record.expiresAtUnix = nowUnix + command.DurationSeconds;
            }

            return refusal;
        }

        /// <summary>Whether a mute or ban record is still in force at <paramref name="nowUnix"/> (ignores later unmute / unban: the server's).</summary>
        public static bool IsInForce(GmCommandRecord record, long nowUnix)
        {
            if (record == null || !record.Ok || (record.kind != GmCommandKind.Mute && record.kind != GmCommandKind.Ban))
            {
                return false;
            }

            return record.permanent || nowUnix < record.expiresAtUnix;
        }

        private static GmRefusal CheckTarget(GmCommand command, bool mustOutrank)
        {
            if (string.IsNullOrEmpty(command.TargetId))
            {
                return GmRefusal.InvalidTarget;
            }

            if (string.Equals(command.TargetId, command.ActorId, StringComparison.Ordinal))
            {
                return GmRefusal.CannotTargetSelf;
            }

            return mustOutrank && command.TargetRole >= command.ActorRole ? GmRefusal.TargetOutranks : GmRefusal.None;
        }

        private static GmRefusal CheckReason(string reason, bool required)
        {
            string cleaned = ChatFilter.Clean(reason);
            if (cleaned.Length > MaxReasonLength)
            {
                return GmRefusal.ReasonTooLong;
            }

            return required && cleaned.Length < MinReasonLength ? GmRefusal.ReasonRequired : GmRefusal.None;
        }

        private static GmRefusal CheckDuration(long seconds, long min, long max)
        {
            if (seconds < min)
            {
                return GmRefusal.InvalidDuration;
            }

            return seconds > max ? GmRefusal.DurationTooLong : GmRefusal.None;
        }

        private static GmRefusal CheckTeleport(GmCommand command)
        {
            if (!Enum.IsDefined(typeof(GmTeleportMode), command.TeleportMode))
            {
                return GmRefusal.UnknownCommand;
            }

            bool self = string.IsNullOrEmpty(command.TargetId)
                || string.Equals(command.TargetId, command.ActorId, StringComparison.Ordinal);
            switch (command.TeleportMode)
            {
                case GmTeleportMode.GoToPlayer:
                    // Going to someone moves only the staff member: any role may be visited.
                    return string.IsNullOrEmpty(command.TargetId) ? GmRefusal.InvalidTarget
                        : self ? GmRefusal.CannotTargetSelf : GmRefusal.None;

                case GmTeleportMode.BringPlayer:
                {
                    GmRefusal target = CheckTarget(command, true);
                    return target;
                }

                default:
                {
                    if (string.IsNullOrEmpty(command.MapId) || !Finite(command.X) || !Finite(command.Y) || !Finite(command.Z))
                    {
                        return GmRefusal.InvalidPosition;
                    }

                    // Sending oneself anywhere is fine; sending someone else needs to outrank them.
                    return self ? GmRefusal.None : CheckTarget(command, true);
                }
            }
        }

        private static GmRefusal CheckLogQuery(GmCommand command)
        {
            if (!Enum.IsDefined(typeof(GmLogKind), command.LogKind))
            {
                return GmRefusal.InvalidLogQuery;
            }

            if (!CanRead(command.ActorRole, command.LogKind))
            {
                return GmRefusal.NoPermission;
            }

            if (command.LogToUnix <= command.LogFromUnix || command.LogLimit <= 0 || command.LogLimit > MaxLogLimit)
            {
                return GmRefusal.InvalidLogQuery;
            }

            return command.LogToUnix - command.LogFromUnix > MaxLogSpanSeconds ? GmRefusal.LogRangeTooLong : GmRefusal.None;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
