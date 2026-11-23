using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Social
{
    /// <summary>
    /// A token bucket: holds up to <c>capacity</c> tokens, gains one every <c>refillSeconds</c>, and each message takes one.
    /// So a player may send a short burst at once and then keeps a steady pace. Starts full.
    /// </summary>
    public struct ChatTokenBucket
    {
        public double Tokens;

        /// <summary>Server time of the last update; NaN = never used (full).</summary>
        public double UpdatedAt;

        public static ChatTokenBucket Full(int capacity) => new ChatTokenBucket { Tokens = capacity, UpdatedAt = double.NaN };

        /// <summary>Refills for the time passed and takes one token if there is a whole one.</summary>
        public bool TryTake(int capacity, double refillSeconds, double now)
        {
            Refill(capacity, refillSeconds, now);
            if (Tokens < 1d)
            {
                return false;
            }

            Tokens -= 1d;
            return true;
        }

        /// <summary>Seconds until the next whole token (0 = one is there now).</summary>
        public double SecondsUntilToken(int capacity, double refillSeconds, double now)
        {
            ChatTokenBucket copy = this;
            copy.Refill(capacity, refillSeconds, now);
            return copy.Tokens >= 1d ? 0d : (1d - copy.Tokens) * refillSeconds;
        }

        private void Refill(int capacity, double refillSeconds, double now)
        {
            if (double.IsNaN(UpdatedAt))
            {
                Tokens = capacity;
                UpdatedAt = now;
                return;
            }

            // A clock that goes back never gives tokens.
            if (now > UpdatedAt && refillSeconds > 0d)
            {
                Tokens = Math.Min(capacity, Tokens + (now - UpdatedAt) / refillSeconds);
            }

            UpdatedAt = Math.Max(UpdatedAt, now);
        }
    }

    /// <summary>
    /// The anti-flood memory of one player, held by the server (lost on logout is fine: a reconnect does not reset an
    /// automatic mute, because the server keeps this per account while the mute runs). Changed only by
    /// <see cref="ChatRules.TrySend"/>; the numbers are in <see cref="ChatRules"/>.
    /// </summary>
    public sealed class ChatSpamState
    {
        private readonly Dictionary<ChatChannel, ChatTokenBucket> _buckets = new Dictionary<ChatChannel, ChatTokenBucket>();
        private readonly List<RecentMessage> _recent = new List<RecentMessage>();
        private readonly List<double> _strikes = new List<double>();

        public ChatSpamState(string playerId)
        {
            PlayerId = playerId;
        }

        public string PlayerId { get; }

        /// <summary>Server time until which the automatic mute lasts (0 = never muted).</summary>
        public double AutoMuteUntil { get; internal set; }

        /// <summary>How many automatic mutes in a row (each one longer, <see cref="ChatRules.AutoMuteSeconds"/>); decays after a clean hour.</summary>
        public int AutoMuteLevel { get; internal set; }

        /// <summary>Flood / repetition refusals within <see cref="ChatRules.StrikeWindowSeconds"/>.</summary>
        public int StrikeCount => _strikes.Count;

        public bool IsAutoMuted(double now) => now < AutoMuteUntil;

        internal ChatTokenBucket BucketOf(ChatChannel channel, int capacity) =>
            _buckets.TryGetValue(channel, out ChatTokenBucket bucket) ? bucket : ChatTokenBucket.Full(capacity);

        internal void SetBucket(ChatChannel channel, ChatTokenBucket bucket) => _buckets[channel] = bucket;

        /// <summary>True when the same <paramref name="key"/> went out in <paramref name="channel"/> within the window.</summary>
        internal bool IsRepeat(ChatChannel channel, string key, double now)
        {
            DropOld(now);
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            foreach (RecentMessage message in _recent)
            {
                if (message.Channel == channel && string.Equals(message.Key, key, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        internal void Remember(ChatChannel channel, string key, double now)
        {
            _recent.Add(new RecentMessage(channel, key, now));
            while (_recent.Count > ChatRules.RepeatMemory)
            {
                _recent.RemoveAt(0);
            }
        }

        /// <summary>Adds a strike; true when this one reaches <see cref="ChatRules.StrikesToAutoMute"/> (the caller then mutes).</summary>
        internal bool AddStrike(double now)
        {
            _strikes.RemoveAll(time => now - time >= ChatRules.StrikeWindowSeconds);
            _strikes.Add(now);
            return _strikes.Count >= ChatRules.StrikesToAutoMute;
        }

        internal void StartAutoMute(double now)
        {
            if (AutoMuteLevel > 0 && now - AutoMuteUntil >= ChatRules.AutoMuteDecaySeconds)
            {
                AutoMuteLevel = 0;
            }

            AutoMuteLevel = Math.Min(AutoMuteLevel + 1, ChatRules.AutoMuteSeconds.Length);
            AutoMuteUntil = now + ChatRules.AutoMuteSeconds[AutoMuteLevel - 1];
            _strikes.Clear();
        }

        private void DropOld(double now) => _recent.RemoveAll(message => now - message.At >= ChatRules.RepeatWindowSeconds);

        private readonly struct RecentMessage
        {
            public readonly ChatChannel Channel;
            public readonly string Key;
            public readonly double At;

            public RecentMessage(ChatChannel channel, string key, double at)
            {
                Channel = channel;
                Key = key;
                At = at;
            }
        }
    }
}
