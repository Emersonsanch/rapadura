using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Guild;
using Rapadura.Gameplay.Party;

namespace Rapadura.Gameplay.Social
{
    /// <summary>The chat channels. Never renumber (the value goes over the wire and into the log).</summary>
    public enum ChatChannel
    {
        /// <summary>Everyone online in the same region of the open world (<see cref="ChatPresence.RegionId"/>).</summary>
        General = 0,

        /// <summary>The members of the sender's party (<see cref="PartyState"/>).</summary>
        Party = 1,

        /// <summary>The members of the sender's guild (<see cref="GuildState"/>).</summary>
        Guild = 2,

        /// <summary>One player, by id ("sussurro").</summary>
        Whisper = 3,

        /// <summary>Only the server writes here (<see cref="ChatRules.SystemMessage"/>): events, world bosses, restarts.</summary>
        System = 4,
    }

    /// <summary>Why a chat message was refused. Texts: <see cref="SocialTexts.ChatRefusalKey"/>. Never renumber.</summary>
    public enum ChatRefusal
    {
        None = 0,

        /// <summary>No sender id.</summary>
        InvalidSender = 1,

        /// <summary>Nothing left after <see cref="ChatFilter.Clean"/>.</summary>
        Empty = 2,

        /// <summary>Longer than the channel's <see cref="ChatChannelLimits.MaxLength"/>.</summary>
        TooLong = 3,

        /// <summary>A player writing in <see cref="ChatChannel.System"/>.</summary>
        NotAllowed = 4,

        NotInParty = 5,
        NotInGuild = 6,

        /// <summary>A whisper with no target.</summary>
        NoWhisperTarget = 7,

        CannotWhisperSelf = 8,

        /// <summary>The whisper target is offline, or blocked the sender (on purpose the same answer, <see cref="PlayerBlockRules"/>).</summary>
        RecipientUnavailable = 9,

        /// <summary>The sender blocked the target: unblock first (they could not answer).</summary>
        YouBlockedRecipient = 10,

        /// <summary>The general chat needs level <see cref="ChatRules.MinLevelForGeneral"/> (fresh bot accounts stay out).</summary>
        LevelTooLow = 11,

        /// <summary>Out of tokens (<see cref="ChatTokenBucket"/>): wait <see cref="ChatSendResult.RetryAfterSeconds"/>.</summary>
        Flood = 12,

        /// <summary>The same message in the same channel within <see cref="ChatRules.RepeatWindowSeconds"/>.</summary>
        Repeated = 13,

        /// <summary>This refusal was strike number <see cref="ChatRules.StrikesToAutoMute"/>: muted until <see cref="ChatSendResult.MutedUntil"/>.</summary>
        AutoMuted = 14,

        /// <summary>Still inside an automatic mute.</summary>
        Muted = 15,

        /// <summary>Muted by a moderator or GM (<see cref="GmCommandKind.Mute"/>).</summary>
        MutedByStaff = 16,

        UnknownEmote = 17,
        UnknownChannel = 18,

        /// <summary>A general message from someone the server has in no region (loading, between servers).</summary>
        NoRegion = 19,
    }

    /// <summary>The numbers of one channel (<see cref="ChatRules.LimitsOf"/>).</summary>
    public readonly struct ChatChannelLimits
    {
        /// <summary>Characters after <see cref="ChatFilter.Clean"/>.</summary>
        public readonly int MaxLength;

        /// <summary>Token bucket size: messages that may go out at once. 0 = no limit (system only).</summary>
        public readonly int BurstMessages;

        /// <summary>Seconds to earn one message back.</summary>
        public readonly double RefillSeconds;

        public ChatChannelLimits(int maxLength, int burstMessages, double refillSeconds)
        {
            MaxLength = maxLength;
            BurstMessages = burstMessages;
            RefillSeconds = refillSeconds;
        }

        /// <summary>Steady pace once the burst is spent, in messages per minute.</summary>
        public double MessagesPerMinute => RefillSeconds > 0d ? 60d / RefillSeconds : double.PositiveInfinity;
    }

    /// <summary>One player online, as the chat router sees them.</summary>
    public readonly struct ChatPresence
    {
        public readonly string PlayerId;

        /// <summary>The region of the open world they stand in (the scope of the general chat).</summary>
        public readonly string RegionId;

        public ChatPresence(string playerId, string regionId)
        {
            PlayerId = playerId;
            RegionId = regionId;
        }
    }

    /// <summary>
    /// Everything the server knows about the sender for one message: who, the party and guild they are in, who is online
    /// and the block lists. Filled by the server from its own records; nothing here comes from the client except the
    /// whisper target.
    /// </summary>
    public sealed class ChatContext
    {
        public string SenderId;
        public int SenderLevel;

        /// <summary>The sender's region; the general chat goes to everyone online in it.</summary>
        public string SenderRegionId;

        /// <summary>The sender's cousin (null for an own character): picks the emote line (<see cref="ChatEmotes.LineKey"/>).</summary>
        public CharacterId? SenderCousin;

        /// <summary>The sender's live party, or null.</summary>
        public PartyState Party;

        /// <summary>The sender's guild, or null.</summary>
        public GuildState Guild;

        /// <summary>Who is online now (null = nobody but the sender).</summary>
        public IReadOnlyList<ChatPresence> Online;

        /// <summary>The block list of a player (null result = blocks nobody). Null = no block lists at all.</summary>
        public Func<string, PlayerBlockList> BlockListOf;

        /// <summary>Only for <see cref="ChatChannel.Whisper"/>.</summary>
        public string WhisperTargetId;

        /// <summary>
        /// Until when a moderator muted the sender, on the same clock as the <c>now</c> of <see cref="ChatRules.TrySend"/> (the
        /// server passes UTC unix seconds to both, so this is <see cref="GmCommandRecord.expiresAtUnix"/>); 0 = not muted.
        /// </summary>
        public double StaffMutedUntil;

        internal PlayerBlockList BlocksOf(string playerId) => BlockListOf?.Invoke(playerId);
    }

    /// <summary>
    /// One message that went out, as plain serializable data: what the server sends to the recipients
    /// (<see cref="text"/>, already masked) and what it appends to the chat log, which a report attaches as evidence
    /// (<see cref="raw"/>, as typed). Built only by <see cref="ChatRules"/>.
    /// </summary>
    [Serializable]
    public sealed class ChatMessage
    {
        public long messageId;
        public ChatChannel channel;
        public string senderId;

        /// <summary>The whisper target; null in the other channels.</summary>
        public string targetId;

        public string regionId;

        /// <summary>What the recipients see (<see cref="ChatFilter.Apply"/>).</summary>
        public string text;

        /// <summary>What the sender typed, after <see cref="ChatFilter.Clean"/>. Server only: never sent to the recipients.</summary>
        public string raw;

        public bool masked;

        /// <summary>Non-null for an emote (<see cref="ChatEmotes"/>); then <see cref="emoteKey"/> is the line the client shows.</summary>
        public string emoteId;

        public string emoteKey;

        /// <summary>Server time.</summary>
        public double at;

        /// <summary>
        /// One line for an append-only log, stable and invariant:
        /// <c>chat=&lt;id&gt; at=&lt;t&gt; ch=whisper from=&lt;a&gt; to=&lt;b&gt; region=&lt;r&gt; masked=0 emote=- text="..."</c>.
        /// </summary>
        public string ToLogLine()
        {
            var sb = new StringBuilder(96 + (raw?.Length ?? 0));
            sb.Append("chat=").Append(messageId.ToString(CultureInfo.InvariantCulture));
            sb.Append(" at=").Append(at.ToString("0.###", CultureInfo.InvariantCulture));
            sb.Append(" ch=").Append(SocialLog.Snake(channel.ToString()));
            sb.Append(" from=").Append(SocialLog.Token(senderId));
            sb.Append(" to=").Append(SocialLog.Token(targetId));
            sb.Append(" region=").Append(SocialLog.Token(regionId));
            sb.Append(" masked=").Append(masked ? '1' : '0');
            sb.Append(" emote=").Append(SocialLog.Token(emoteId));
            sb.Append(" text=").Append(SocialLog.Quote(raw));
            return sb.ToString();
        }
    }

    /// <summary>Outcome of <see cref="ChatRules.TrySend"/>: no exceptions, a refusal (<see cref="ChatRefusal.None"/> = sent).</summary>
    public sealed class ChatSendResult
    {
        public ChatRefusal Refusal { get; }

        /// <summary>The message that went out (null when refused).</summary>
        public ChatMessage Message { get; }

        /// <summary>Who gets it, sender included (the echo), in a stable order; empty when refused.</summary>
        public IReadOnlyList<string> Recipients { get; }

        /// <summary>For <see cref="ChatRefusal.Muted"/>, <see cref="ChatRefusal.AutoMuted"/>, <see cref="ChatRefusal.MutedByStaff"/>.</summary>
        public double MutedUntil { get; }

        /// <summary>For <see cref="ChatRefusal.Flood"/>: seconds until the next message may go.</summary>
        public double RetryAfterSeconds { get; }

        internal ChatSendResult(ChatRefusal refusal, ChatMessage message, IReadOnlyList<string> recipients, double mutedUntil = 0d,
            double retryAfterSeconds = 0d)
        {
            Refusal = refusal;
            Message = message;
            Recipients = recipients ?? Array.Empty<string>();
            MutedUntil = mutedUntil;
            RetryAfterSeconds = retryAfterSeconds;
        }

        public bool Ok => Refusal == ChatRefusal.None;

        internal static ChatSendResult Refused(ChatRefusal refusal, double mutedUntil = 0d, double retryAfter = 0d) =>
            new ChatSendResult(refusal, null, null, mutedUntil, retryAfter);
    }

    /// <summary>
    /// Who receives a message: a pure function of the channel, the party, the guild, the regions of who is online and the
    /// block lists. The sender is always in the list (the echo); anyone else who blocked the sender is left out (the
    /// message is simply never shown to them), except in <see cref="ChatChannel.System"/>.
    /// </summary>
    public static class ChatRouting
    {
        public static List<string> Recipients(ChatChannel channel, ChatContext context)
        {
            var recipients = new List<string>();
            if (context == null)
            {
                return recipients;
            }

            string sender = context.SenderId;
            if (channel == ChatChannel.Whisper)
            {
                if (IsOnline(context, context.WhisperTargetId)
                    && PlayerBlockRules.CanContact(context.BlocksOf(context.WhisperTargetId), sender, SocialContactKind.Whisper))
                {
                    recipients.Add(context.WhisperTargetId);
                }

                AddSender(recipients, sender);
                return recipients;
            }

            if (context.Online != null)
            {
                foreach (ChatPresence presence in context.Online)
                {
                    if (string.IsNullOrEmpty(presence.PlayerId) || presence.PlayerId == sender || recipients.Contains(presence.PlayerId))
                    {
                        continue;
                    }

                    if (!Reaches(channel, context, presence))
                    {
                        continue;
                    }

                    if (channel != ChatChannel.System
                        && !PlayerBlockRules.CanContact(context.BlocksOf(presence.PlayerId), sender, SocialContactKind.ChatMessage))
                    {
                        continue;
                    }

                    recipients.Add(presence.PlayerId);
                }
            }

            if (channel != ChatChannel.System)
            {
                AddSender(recipients, sender);
            }

            return recipients;
        }

        private static bool Reaches(ChatChannel channel, ChatContext context, ChatPresence presence)
        {
            switch (channel)
            {
                case ChatChannel.General:
                    return !string.IsNullOrEmpty(context.SenderRegionId)
                        && string.Equals(presence.RegionId, context.SenderRegionId, StringComparison.Ordinal);
                case ChatChannel.Party:
                    return PartyRules.AreInSameParty(context.Party, context.SenderId, presence.PlayerId);
                case ChatChannel.Guild:
                    return context.Guild != null && !context.Guild.IsDisbanded && context.Guild.IsMember(context.SenderId)
                        && context.Guild.IsMember(presence.PlayerId);
                case ChatChannel.System:
                    // A system message with a region reaches that region; without one, everybody.
                    return string.IsNullOrEmpty(context.SenderRegionId)
                        || string.Equals(presence.RegionId, context.SenderRegionId, StringComparison.Ordinal);
                default:
                    return false;
            }
        }

        internal static bool IsOnline(ChatContext context, string playerId)
        {
            if (string.IsNullOrEmpty(playerId) || context.Online == null)
            {
                return false;
            }

            foreach (ChatPresence presence in context.Online)
            {
                if (presence.PlayerId == playerId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void AddSender(List<string> recipients, string sender)
        {
            if (!string.IsNullOrEmpty(sender) && !recipients.Contains(sender))
            {
                recipients.Add(sender);
            }
        }
    }

    /// <summary>
    /// The chat of FASE 12.3 (TODO.md, "Chat (geral, grupo, clã, sussurro) com filtro de palavras e denúncia; emotes para o
    /// mobile"). Pure C# and exception-free: the server calls <see cref="TrySend"/> with its own clock for each message a
    /// client sends, and the client only shows <see cref="SocialTexts.ChatRefusalKey"/>. Decisions (2026-11-23):
    /// <list type="table">
    /// <item><term>Geral</term><description>everyone online in the sender's <b>region</b> (the world is one seamless map, so
    /// "the map" has no edge a player can see; a region is a few neighbouring zones). Up to 120 characters; a burst of 3,
    /// then 1 every 6 s (10 a minute); level <see cref="MinLevelForGeneral"/> or above.</description></item>
    /// <item><term>Grupo</term><description>the party members online; 200 characters; burst 6, then 1 every 2 s.</description></item>
    /// <item><term>Guilda</term><description>the guild members online, recruits included; 200 characters; burst 5, then 1 every 3 s.</description></item>
    /// <item><term>Sussurro</term><description>one player online who did not block the sender; 200 characters; burst 5, then 1
    /// every 2 s.</description></item>
    /// <item><term>Sistema</term><description>only the server (<see cref="SystemMessage"/>), to everyone or one region; 300
    /// characters; no limit, no filter, no block.</description></item>
    /// </list>
    /// <b>Repetition:</b> the same message (by <see cref="ChatFilter.RepeatKey"/>: case, accents and "!!!" do not make it
    /// different) in the same channel within <see cref="RepeatWindowSeconds"/> is refused. <b>Automatic mute:</b> each flood or
    /// repetition refusal is a strike; <see cref="StrikesToAutoMute"/> strikes within <see cref="StrikeWindowSeconds"/> mute
    /// every channel for 1 min, then 5 min, 15 min and 1 h on the next ones; a clean hour after a mute brings it back to 1
    /// min. <b>Filter:</b> masks, never refuses (<see cref="ChatFilter"/>).
    /// </summary>
    public static class ChatRules
    {
        public const int MinLevelForGeneral = 5;
        public const double RepeatWindowSeconds = 30d;

        /// <summary>Recent messages remembered per player for the repetition rule.</summary>
        public const int RepeatMemory = 8;

        public const double StrikeWindowSeconds = 60d;
        public const int StrikesToAutoMute = 4;

        /// <summary>Length of the 1st, 2nd, 3rd and every later automatic mute.</summary>
        public static readonly double[] AutoMuteSeconds = { 60d, 300d, 900d, 3600d };

        /// <summary>Clean time after a mute ends that brings the next mute back to the first length.</summary>
        public const double AutoMuteDecaySeconds = 3600d;

        public static readonly ChatChannelLimits GeneralLimits = new ChatChannelLimits(120, 3, 6d);
        public static readonly ChatChannelLimits PartyLimits = new ChatChannelLimits(200, 6, 2d);
        public static readonly ChatChannelLimits GuildLimits = new ChatChannelLimits(200, 5, 3d);
        public static readonly ChatChannelLimits WhisperLimits = new ChatChannelLimits(200, 5, 2d);
        public static readonly ChatChannelLimits SystemLimits = new ChatChannelLimits(300, 0, 0d);

        public static readonly ChatChannel[] PlayerChannels = { ChatChannel.General, ChatChannel.Party, ChatChannel.Guild, ChatChannel.Whisper };

        public static ChatChannelLimits LimitsOf(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.General: return GeneralLimits;
                case ChatChannel.Party: return PartyLimits;
                case ChatChannel.Guild: return GuildLimits;
                case ChatChannel.Whisper: return WhisperLimits;
                default: return SystemLimits;
            }
        }

        /// <summary>
        /// A player sends <paramref name="text"/> in <paramref name="channel"/>. <paramref name="spam"/> is that player's
        /// <see cref="ChatSpamState"/> (null = no anti-flood: server scripts and tests). <paramref name="now"/> is the server
        /// clock in seconds; <paramref name="messageId"/> comes from the server (the key of the log).
        /// </summary>
        public static ChatSendResult TrySend(ChatSpamState spam, ChatChannel channel, string text, ChatContext context, double now,
            long messageId = 0)
        {
            return Send(spam, channel, text, null, context, now, messageId);
        }

        /// <summary>Same as <see cref="TrySend"/> for an emote (<see cref="ChatEmotes"/>): no text, no filter, same limits.</summary>
        public static ChatSendResult TrySendEmote(ChatSpamState spam, ChatChannel channel, string emoteId, ChatContext context, double now,
            long messageId = 0)
        {
            if (!ChatEmotes.Exists(emoteId))
            {
                return ChatSendResult.Refused(ChatRefusal.UnknownEmote);
            }

            return Send(spam, channel, null, emoteId, context, now, messageId);
        }

        /// <summary>
        /// The server announces something: to everyone online (<paramref name="regionId"/> null) or to one region. No limit,
        /// no filter, no block; only the length (<see cref="SystemLimits"/>) is checked.
        /// </summary>
        public static ChatSendResult SystemMessage(string text, string regionId, IReadOnlyList<ChatPresence> online, double now, long messageId = 0)
        {
            string cleaned = ChatFilter.Clean(text);
            if (cleaned.Length == 0)
            {
                return ChatSendResult.Refused(ChatRefusal.Empty);
            }

            if (cleaned.Length > SystemLimits.MaxLength)
            {
                return ChatSendResult.Refused(ChatRefusal.TooLong);
            }

            var context = new ChatContext { SenderRegionId = regionId, Online = online };
            var message = new ChatMessage
            {
                messageId = messageId,
                channel = ChatChannel.System,
                senderId = null,
                regionId = regionId,
                text = cleaned,
                raw = cleaned,
                at = now,
            };
            return new ChatSendResult(ChatRefusal.None, message, ChatRouting.Recipients(ChatChannel.System, context));
        }

        /// <summary>The channel check alone (membership, whisper target, level), without text, limits or state: what the UI greys out.</summary>
        public static ChatRefusal CanUseChannel(ChatChannel channel, ChatContext context)
        {
            if (context == null || string.IsNullOrEmpty(context.SenderId))
            {
                return ChatRefusal.InvalidSender;
            }

            switch (channel)
            {
                case ChatChannel.General:
                    if (context.SenderLevel < MinLevelForGeneral)
                    {
                        return ChatRefusal.LevelTooLow;
                    }

                    return string.IsNullOrEmpty(context.SenderRegionId) ? ChatRefusal.NoRegion : ChatRefusal.None;

                case ChatChannel.Party:
                    return context.Party != null && !context.Party.IsDisbanded && context.Party.IsMember(context.SenderId)
                        ? ChatRefusal.None
                        : ChatRefusal.NotInParty;

                case ChatChannel.Guild:
                    return context.Guild != null && !context.Guild.IsDisbanded && context.Guild.IsMember(context.SenderId)
                        ? ChatRefusal.None
                        : ChatRefusal.NotInGuild;

                case ChatChannel.Whisper:
                    string target = context.WhisperTargetId;
                    if (string.IsNullOrEmpty(target))
                    {
                        return ChatRefusal.NoWhisperTarget;
                    }

                    if (target == context.SenderId)
                    {
                        return ChatRefusal.CannotWhisperSelf;
                    }

                    if (PlayerBlockRules.Blocks(context.BlocksOf(context.SenderId), target))
                    {
                        return ChatRefusal.YouBlockedRecipient;
                    }

                    if (!ChatRouting.IsOnline(context, target)
                        || !PlayerBlockRules.CanContact(context.BlocksOf(target), context.SenderId, SocialContactKind.Whisper))
                    {
                        return ChatRefusal.RecipientUnavailable;
                    }

                    return ChatRefusal.None;

                case ChatChannel.System:
                    return ChatRefusal.NotAllowed;

                default:
                    return ChatRefusal.UnknownChannel;
            }
        }

        private static ChatSendResult Send(ChatSpamState spam, ChatChannel channel, string text, string emoteId, ChatContext context,
            double now, long messageId)
        {
            if (context == null || string.IsNullOrEmpty(context.SenderId))
            {
                return ChatSendResult.Refused(ChatRefusal.InvalidSender);
            }

            if (channel == ChatChannel.System)
            {
                return ChatSendResult.Refused(ChatRefusal.NotAllowed);
            }

            if (!Enum.IsDefined(typeof(ChatChannel), channel))
            {
                return ChatSendResult.Refused(ChatRefusal.UnknownChannel);
            }

            if (now < context.StaffMutedUntil)
            {
                return ChatSendResult.Refused(ChatRefusal.MutedByStaff, context.StaffMutedUntil);
            }

            if (spam != null && spam.IsAutoMuted(now))
            {
                return ChatSendResult.Refused(ChatRefusal.Muted, spam.AutoMuteUntil);
            }

            ChatChannelLimits limits = LimitsOf(channel);
            string cleaned = emoteId == null ? ChatFilter.Clean(text) : string.Empty;
            if (emoteId == null)
            {
                if (cleaned.Length == 0)
                {
                    return ChatSendResult.Refused(ChatRefusal.Empty);
                }

                if (cleaned.Length > limits.MaxLength)
                {
                    return ChatSendResult.Refused(ChatRefusal.TooLong);
                }
            }

            ChatRefusal access = CanUseChannel(channel, context);
            if (access != ChatRefusal.None)
            {
                return ChatSendResult.Refused(access);
            }

            string key = emoteId != null ? "emote:" + emoteId : ChatFilter.RepeatKey(cleaned);
            if (spam != null)
            {
                if (spam.IsRepeat(channel, key, now))
                {
                    return Strike(spam, ChatRefusal.Repeated, now, 0d);
                }

                ChatTokenBucket bucket = spam.BucketOf(channel, limits.BurstMessages);
                bool took = bucket.TryTake(limits.BurstMessages, limits.RefillSeconds, now);
                spam.SetBucket(channel, bucket);
                if (!took)
                {
                    return Strike(spam, ChatRefusal.Flood, now, bucket.SecondsUntilToken(limits.BurstMessages, limits.RefillSeconds, now));
                }

                spam.Remember(channel, key, now);
            }

            ChatFilterResult filtered = ChatFilter.Apply(cleaned);
            var message = new ChatMessage
            {
                messageId = messageId,
                channel = channel,
                senderId = context.SenderId,
                targetId = channel == ChatChannel.Whisper ? context.WhisperTargetId : null,
                regionId = context.SenderRegionId,
                text = filtered.Text,
                raw = cleaned,
                masked = filtered.Masked,
                emoteId = emoteId,
                emoteKey = emoteId != null ? ChatEmotes.LineKey(emoteId, context.SenderCousin) : null,
                at = now,
            };
            return new ChatSendResult(ChatRefusal.None, message, ChatRouting.Recipients(channel, context));
        }

        private static ChatSendResult Strike(ChatSpamState spam, ChatRefusal refusal, double now, double retryAfter)
        {
            if (spam.AddStrike(now))
            {
                spam.StartAutoMute(now);
                return ChatSendResult.Refused(ChatRefusal.AutoMuted, spam.AutoMuteUntil);
            }

            return ChatSendResult.Refused(refusal, 0d, retryAfter);
        }
    }
}
