using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Social
{
    /// <summary>Why a block / unblock was refused. Texts: <see cref="SocialTexts.BlockRefusalKey"/>. Never renumber.</summary>
    public enum BlockRefusal
    {
        None = 0,

        /// <summary>Empty / null player id.</summary>
        InvalidPlayer = 1,

        /// <summary>Nobody blocks themself.</summary>
        CannotBlockSelf = 2,

        AlreadyBlocked = 3,

        /// <summary>The list already has <see cref="PlayerBlockList.MaxBlocked"/> players.</summary>
        ListFull = 4,

        /// <summary>Unblocking someone who is not on the list.</summary>
        NotBlocked = 5,
    }

    /// <summary>Every way one player reaches another; a block shuts all of them (<see cref="PlayerBlockRules.CanContact"/>).</summary>
    public enum SocialContactKind
    {
        /// <summary>A message in the general, party or guild chat: hidden from the one who blocked.</summary>
        ChatMessage = 0,
        Whisper = 1,
        PartyInvite = 2,
        GuildInvite = 3,
        TradeRequest = 4,
        DuelChallenge = 5,
    }

    /// <summary>
    /// The players one player blocked (TODO.md Fase 12.3, "Moderação: ... bloqueio de jogador"). Plain C#: the server keeps
    /// one per player (saved with the account, <see cref="Ids"/> / the constructor that takes them) and the rules ask it.
    /// One-way: the owner stops hearing the blocked player, and the owner may still choose to talk to them.
    /// </summary>
    public sealed class PlayerBlockList
    {
        /// <summary>Players one list may hold (the size of a MU friend list, twice over).</summary>
        public const int MaxBlocked = 100;

        private readonly List<string> _ids = new List<string>();
        private readonly HashSet<string> _set = new HashSet<string>(StringComparer.Ordinal);

        public PlayerBlockList(string ownerId, IEnumerable<string> blockedIds = null)
        {
            OwnerId = ownerId;
            if (blockedIds == null)
            {
                return;
            }

            foreach (string id in blockedIds)
            {
                Block(id);
            }
        }

        public string OwnerId { get; }

        /// <summary>Blocked players in the order they were blocked (what the server saves).</summary>
        public IReadOnlyList<string> Ids => _ids;

        public int Count => _ids.Count;

        public bool Blocks(string playerId) => !string.IsNullOrEmpty(playerId) && _set.Contains(playerId);

        public BlockRefusal Block(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return BlockRefusal.InvalidPlayer;
            }

            if (string.Equals(playerId, OwnerId, StringComparison.Ordinal))
            {
                return BlockRefusal.CannotBlockSelf;
            }

            if (_set.Contains(playerId))
            {
                return BlockRefusal.AlreadyBlocked;
            }

            if (_ids.Count >= MaxBlocked)
            {
                return BlockRefusal.ListFull;
            }

            _ids.Add(playerId);
            _set.Add(playerId);
            return BlockRefusal.None;
        }

        public BlockRefusal Unblock(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
            {
                return BlockRefusal.InvalidPlayer;
            }

            if (!_set.Remove(playerId))
            {
                return BlockRefusal.NotBlocked;
            }

            _ids.Remove(playerId);
            return BlockRefusal.None;
        }
    }

    /// <summary>
    /// The one question the other rules ask about a block. Decision (2026-11-23): the blocked player is <b>never told</b>
    /// they were blocked. Each rule answers with the refusal it already gives for "that player is not there"
    /// (<c>PartyRefusal.InvalidPlayer</c>, <c>GuildRefusal.InvalidPlayer</c>, <c>TradeRefusal.PartnerUnavailable</c>,
    /// <c>PvpRefusal.TargetBlocksDuels</c>, <see cref="ChatRefusal.RecipientUnavailable"/>), so a block cannot be told apart
    /// from being offline or busy, and nobody is baited into a fight over it.
    /// <list type="bullet">
    /// <item><c>PartyRules.Invite</c>, <c>GuildRules.Invite</c>, <c>TradeRules.CanOpen</c> / <c>TradeSession.TryOpen</c> take the
    /// invitee's list as an optional last parameter (null = no block, the behaviour before).</item>
    /// <item><c>DuelRules.Challenge</c> already takes <c>targetBlocksDuels</c>: the server passes <see cref="TargetRefusesDuel"/>.</item>
    /// <item>The chat asks <see cref="CanContact"/> for the whisper and hides the other channels' messages from the one who blocked
    /// (<see cref="ChatRouting.Recipients"/>). System messages and the staff (<see cref="GmRules"/>) are never blocked.</item>
    /// </list>
    /// </summary>
    public static class PlayerBlockRules
    {
        /// <summary>True when the owner of <paramref name="recipientBlocks"/> blocked <paramref name="senderId"/>. Null list = nobody blocked.</summary>
        public static bool Blocks(PlayerBlockList recipientBlocks, string senderId) =>
            recipientBlocks != null && recipientBlocks.Blocks(senderId);

        /// <summary>
        /// Whether <paramref name="senderId"/> may reach the owner of <paramref name="recipientBlocks"/> in that way. Every kind is
        /// shut by a block today; the kind is there for the log and for a future "block only invites" option.
        /// </summary>
        public static bool CanContact(PlayerBlockList recipientBlocks, string senderId, SocialContactKind kind)
        {
            switch (kind)
            {
                case SocialContactKind.ChatMessage:
                case SocialContactKind.Whisper:
                case SocialContactKind.PartyInvite:
                case SocialContactKind.GuildInvite:
                case SocialContactKind.TradeRequest:
                case SocialContactKind.DuelChallenge:
                default:
                    return !Blocks(recipientBlocks, senderId);
            }
        }

        /// <summary>The <c>targetBlocksDuels</c> of <c>DuelRules.Challenge</c>: the target turned duels off, or blocked the challenger.</summary>
        public static bool TargetRefusesDuel(bool targetRefusesAllDuels, PlayerBlockList targetBlocks, string challengerId) =>
            targetRefusesAllDuels || !CanContact(targetBlocks, challengerId, SocialContactKind.DuelChallenge);
    }
}
