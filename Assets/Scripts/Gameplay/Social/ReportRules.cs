using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rapadura.Gameplay.Social
{
    /// <summary>Why a player is reported. Texts: <see cref="SocialTexts.ReportReasonKey"/>. Never renumber.</summary>
    public enum ReportReason
    {
        Spam = 0,
        OffensiveLanguage = 1,
        Harassment = 2,

        /// <summary>Hacks, speed, item duplication.</summary>
        Cheating = 3,

        /// <summary>Playing by script / farming bot.</summary>
        Botting = 4,

        /// <summary>A trade or promise that was a trick.</summary>
        Scam = 5,

        /// <summary>The character's or guild's name.</summary>
        InappropriateName = 6,

        Other = 7,
    }

    /// <summary>Why a report was refused. Texts: <see cref="SocialTexts.ReportRefusalKey"/>. Never renumber.</summary>
    public enum ReportRefusal
    {
        None = 0,
        InvalidPlayer = 1,
        CannotReportSelf = 2,

        /// <summary><see cref="ReportRules.MaxReportsPerDay"/> reports already today (UTC).</summary>
        DailyLimitReached = 3,

        /// <summary>The same player was already reported by this reporter today.</summary>
        AlreadyReportedToday = 4,

        /// <summary>More than <see cref="ReportRules.MaxCommentLength"/> characters.</summary>
        CommentTooLong = 5,

        /// <summary>Not a <see cref="ReportReason"/> value.</summary>
        InvalidReason = 6,

        /// <summary><see cref="ReportReason.Other"/> needs a comment saying what happened.</summary>
        CommentRequired = 7,
    }

    /// <summary>The reports one player filed on one UTC day; the server keeps one per account.</summary>
    [Serializable]
    public sealed class ReportDailyTally
    {
        /// <summary><see cref="ReportRules.DayKey"/> of the counts below.</summary>
        public string day;

        public List<string> reportedIds = new List<string>();

        public int Count => reportedIds?.Count ?? 0;

        /// <summary>Starts a new day when <paramref name="dayKey"/> is not the stored one.</summary>
        public void Roll(string dayKey)
        {
            if (!string.Equals(day, dayKey, StringComparison.Ordinal))
            {
                day = dayKey;
                reportedIds = new List<string>();
            }

            reportedIds = reportedIds ?? new List<string>();
        }
    }

    /// <summary>
    /// One report as plain serializable data for the moderation queue and log: who, whom, why, the reporter's words and the
    /// last chat lines as evidence (<see cref="ChatMessage.raw"/>, as typed, so a masked insult is still readable to the staff).
    /// </summary>
    [Serializable]
    public sealed class PlayerReport
    {
        public string reportId;
        public string reporterId;
        public string targetId;
        public ReportReason reason;
        public string comment;
        public string regionId;

        /// <summary>UTC unix seconds.</summary>
        public long createdAtUnix;

        public ChatMessage[] evidence = Array.Empty<ChatMessage>();

        /// <summary>
        /// One line for an append-only log (the evidence goes as message ids; each message has its own line in the chat log):
        /// <c>report=&lt;id&gt; at=&lt;unix&gt; by=&lt;a&gt; on=&lt;b&gt; reason=spam region=&lt;r&gt; evidence=[12;13] comment="..."</c>.
        /// </summary>
        public string ToLogLine()
        {
            var sb = new StringBuilder(128);
            sb.Append("report=").Append(SocialLog.Token(reportId));
            sb.Append(" at=").Append(createdAtUnix.ToString(CultureInfo.InvariantCulture));
            sb.Append(" by=").Append(SocialLog.Token(reporterId));
            sb.Append(" on=").Append(SocialLog.Token(targetId));
            sb.Append(" reason=").Append(SocialLog.Snake(reason.ToString()));
            sb.Append(" region=").Append(SocialLog.Token(regionId));
            sb.Append(" evidence=[");
            if (evidence != null)
            {
                for (int i = 0; i < evidence.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(';');
                    }

                    sb.Append(evidence[i] != null ? evidence[i].messageId.ToString(CultureInfo.InvariantCulture) : "-");
                }
            }

            sb.Append("] comment=").Append(SocialLog.Quote(comment));
            return sb.ToString();
        }
    }

    /// <summary>
    /// The report button (TODO.md 12.3, "Chat ... e denúncia" and "Moderação: denúncias"). Pure C#: the server calls
    /// <see cref="TryFile"/> with the reporter's tally and the chat lines the reporter saw, stores the
    /// <see cref="PlayerReport"/> and appends <see cref="PlayerReport.ToLogLine"/>. Decisions (2026-11-23):
    /// <list type="bullet">
    /// <item>Up to <see cref="MaxReportsPerDay"/> reports a UTC day, and one per reported player a day (a second click on the same
    /// player is refused, not counted twice).</item>
    /// <item>Evidence: the last <see cref="EvidenceMessages"/> messages the reported player sent that the reporter received, plus the
    /// whispers between the two, oldest first. Picked by the server from its own log, never sent by the client.</item>
    /// <item>The comment is optional (up to <see cref="MaxCommentLength"/>), except for <see cref="ReportReason.Other"/>.</item>
    /// <item>A report punishes nobody by itself: it goes to the staff (<see cref="GmRules"/>).</item>
    /// </list>
    /// </summary>
    public static class ReportRules
    {
        public const int MaxReportsPerDay = 5;
        public const int EvidenceMessages = 10;
        public const int MaxCommentLength = 200;

        /// <summary>The UTC day ("yyyy-MM-dd") of a unix time: the key of <see cref="ReportDailyTally"/>.</summary>
        public static string DayKey(long unixSeconds) =>
            DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        /// <summary>
        /// <paramref name="reporterId"/> reports <paramref name="targetId"/>. <paramref name="tally"/> is the reporter's
        /// (rolled to today and counted on success; null = no daily limit, staff tools). <paramref name="seenLog"/> is what the
        /// server has of the chat the reporter received, in time order.
        /// </summary>
        public static ReportRefusal TryFile(string reportId, string reporterId, string targetId, ReportReason reason, string comment,
            string regionId, IReadOnlyList<ChatMessage> seenLog, ReportDailyTally tally, long nowUnix, out PlayerReport report)
        {
            report = null;
            if (string.IsNullOrEmpty(reporterId) || string.IsNullOrEmpty(targetId))
            {
                return ReportRefusal.InvalidPlayer;
            }

            if (string.Equals(reporterId, targetId, StringComparison.Ordinal))
            {
                return ReportRefusal.CannotReportSelf;
            }

            if (!Enum.IsDefined(typeof(ReportReason), reason))
            {
                return ReportRefusal.InvalidReason;
            }

            string cleaned = ChatFilter.Clean(comment);
            if (cleaned.Length > MaxCommentLength)
            {
                return ReportRefusal.CommentTooLong;
            }

            if (reason == ReportReason.Other && cleaned.Length == 0)
            {
                return ReportRefusal.CommentRequired;
            }

            if (tally != null)
            {
                tally.Roll(DayKey(nowUnix));
                if (tally.reportedIds.Contains(targetId))
                {
                    return ReportRefusal.AlreadyReportedToday;
                }

                if (tally.Count >= MaxReportsPerDay)
                {
                    return ReportRefusal.DailyLimitReached;
                }

                tally.reportedIds.Add(targetId);
            }

            report = new PlayerReport
            {
                reportId = reportId,
                reporterId = reporterId,
                targetId = targetId,
                reason = reason,
                comment = cleaned,
                regionId = regionId,
                createdAtUnix = nowUnix,
                evidence = Evidence(reporterId, targetId, seenLog, EvidenceMessages),
            };
            return ReportRefusal.None;
        }

        /// <summary>The last <paramref name="count"/> messages of <paramref name="log"/> from the target, or whispered between the two, oldest first.</summary>
        public static ChatMessage[] Evidence(string reporterId, string targetId, IReadOnlyList<ChatMessage> log, int count)
        {
            if (log == null || count <= 0)
            {
                return Array.Empty<ChatMessage>();
            }

            var picked = new List<ChatMessage>(count);
            for (int i = log.Count - 1; i >= 0 && picked.Count < count; i--)
            {
                ChatMessage message = log[i];
                if (message != null && IsEvidence(message, reporterId, targetId))
                {
                    picked.Add(message);
                }
            }

            picked.Reverse();
            return picked.ToArray();
        }

        private static bool IsEvidence(ChatMessage message, string reporterId, string targetId)
        {
            if (message.senderId == targetId)
            {
                return true;
            }

            // The reporter's side of a whisper with the target: the context of an argument.
            return message.channel == ChatChannel.Whisper && message.senderId == reporterId && message.targetId == targetId;
        }
    }
}
