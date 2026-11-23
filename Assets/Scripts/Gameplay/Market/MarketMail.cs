using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Trade;

namespace Rapadura.Gameplay.Market
{
    /// <summary>Why a <see cref="MarketMail"/> was sent.</summary>
    public enum MailKind
    {
        /// <summary>What was left in a closed stall goes back to its owner.</summary>
        StallReturn = 0,

        /// <summary>The stall's till (sales minus the 2% tax).</summary>
        StallProceeds = 1,

        /// <summary>The piece the winner bought (by bid or buyout).</summary>
        AuctionWon = 2,

        /// <summary>The seller's gold (final price minus the 5% tax).</summary>
        AuctionProceeds = 3,

        /// <summary>Nobody bid: the piece goes back to the seller (the deposit is not refunded).</summary>
        AuctionUnsold = 4,

        /// <summary>The escrowed bid of who was outbid.</summary>
        AuctionOutbidRefund = 5,

        /// <summary>The seller cancelled a listing with no bids.</summary>
        AuctionCancelled = 6,
    }

    /// <summary>One mail: gold and/or one item for <see cref="recipient"/>. Only the market sends mail (no player-to-player mail yet).</summary>
    [Serializable]
    public sealed class MarketMail
    {
        public string mailId;
        public string recipient;
        public MailKind kind;

        /// <summary>The stall or listing it came from.</summary>
        public string refId;

        public long gold;

        /// <summary>The attached item; null when the mail only carries gold.</summary>
        public MarketItem item;

        public long sentAtUnix;
        public long expiresAtUnix;

        public bool IsExpired(long nowUnix) => nowUnix >= expiresAtUnix;
    }

    /// <summary>What happened to a mail that expired: everything in it, as chest entries for its owner.</summary>
    public sealed class MailExpiry
    {
        public MarketMail Mail;

        /// <summary>
        /// Entries for the owner's chest overflow inbox (<see cref="VillageChestSaveState.overflowInbox"/>): the item, and
        /// the gold as an <see cref="TradeRules.GoldItemId"/> entry. The server appends them in the same transaction that
        /// drops the mail.
        /// </summary>
        public List<InventorySlotData> ChestEntries = new List<InventorySlotData>();

        public MarketRecord Record;
    }

    /// <summary>What <see cref="MarketPost.Take"/> did. On failure nothing moved: the mail stays and the bag is unchanged.</summary>
    public sealed class MailTakeResult
    {
        public bool Success;
        public MarketRefusal Refusal;
        public InventorySlotData[] Bag;
        public MarketMail Mail;
        public MarketRecord Record;
    }

    /// <summary>
    /// The mailboxes of every player (TODO.md Fase 12.3; Docs/Network.md §17): where auction deliveries, refunds and stall
    /// returns land, so nothing depends on the recipient being online or having room. Pure, server-held. A mail is taken
    /// whole into the bag (all or nothing, <see cref="MarketBag.TryExchange"/>) and removed in the same step, so taking it
    /// twice is <see cref="MarketRefusal.MailNotFound"/>. After <see cref="MarketRules.MailExpiryDays"/> days an untaken
    /// mail <b>returns to its owner</b>: <see cref="Sweep"/> turns it into chest overflow entries (nothing is destroyed;
    /// every market mail already belongs to its recipient).
    /// </summary>
    public sealed class MarketPost
    {
        private readonly Dictionary<string, List<MarketMail>> _boxes = new Dictionary<string, List<MarketMail>>(StringComparer.Ordinal);
        private long _nextId = 1;

        public MarketMail Send(string recipient, MailKind kind, string refId, long gold, MarketItem item, long nowUnix)
        {
            var mail = new MarketMail
            {
                mailId = "mail-" + _nextId++,
                recipient = recipient,
                kind = kind,
                refId = refId,
                gold = Math.Max(0L, gold),
                item = item,
                sentAtUnix = nowUnix,
                expiresAtUnix = nowUnix + MarketRules.MailExpirySeconds,
            };

            if (!_boxes.TryGetValue(recipient ?? string.Empty, out List<MarketMail> box))
            {
                box = new List<MarketMail>();
                _boxes[recipient ?? string.Empty] = box;
            }

            box.Add(mail);
            return mail;
        }

        public IReadOnlyList<MarketMail> MailOf(string recipient)
        {
            return recipient != null && _boxes.TryGetValue(recipient, out List<MarketMail> box) ? box : (IReadOnlyList<MarketMail>)Array.Empty<MarketMail>();
        }

        public int CountOf(string recipient) => MailOf(recipient).Count;

        /// <summary>Gold in every mailbox (the auditor's "gold in flight").</summary>
        public long GoldInFlight()
        {
            long total = 0;
            foreach (List<MarketMail> box in _boxes.Values)
            {
                foreach (MarketMail mail in box)
                {
                    total += mail.gold;
                }
            }

            return total;
        }

        /// <summary>Units of <paramref name="itemId"/> in every mailbox.</summary>
        public int UnitsInFlight(string itemId)
        {
            int total = 0;
            foreach (List<MarketMail> box in _boxes.Values)
            {
                foreach (MarketMail mail in box)
                {
                    if (mail.item != null && string.Equals(mail.item.ItemId, itemId, StringComparison.Ordinal))
                    {
                        total += mail.item.Quantity;
                    }
                }
            }

            return total;
        }

        /// <summary>Takes mail <paramref name="mailId"/> of <paramref name="recipient"/> into a copy of the bag.</summary>
        public MailTakeResult Take(string recipient, string mailId, TradeSideInput bag, Func<string, ItemDefinition> resolve, long nowUnix)
        {
            var result = new MailTakeResult();
            MarketMail mail = Find(mailId, out List<MarketMail> box);
            if (mail == null)
            {
                result.Refusal = MarketRefusal.MailNotFound;
                return result;
            }

            if (!string.Equals(mail.recipient, recipient, StringComparison.Ordinal))
            {
                result.Refusal = MarketRefusal.NotRecipient;
                return result;
            }

            if (mail.IsExpired(nowUnix))
            {
                result.Refusal = MarketRefusal.MailExpired;
                return result;
            }

            var items = mail.item != null ? new[] { mail.item } : Array.Empty<MarketItem>();
            MarketRefusal refusal = MarketBag.TryExchange(bag, 0L, mail.gold, items, resolve, out InventorySlotData[] after);
            if (refusal != MarketRefusal.None)
            {
                result.Refusal = refusal;
                return result;
            }

            box.Remove(mail);
            result.Success = true;
            result.Bag = after;
            result.Mail = mail;
            result.Record = MarketRecord.Of(MarketRecordKind.MailTaken, nowUnix, mail.mailId, recipient, null, mail.gold, 0L, mail.item);
            return result;
        }

        /// <summary>Every mail expired at <paramref name="nowUnix"/>, removed and handed back to its owner as chest entries.</summary>
        public List<MailExpiry> Sweep(long nowUnix)
        {
            var expired = new List<MailExpiry>();
            foreach (List<MarketMail> box in _boxes.Values)
            {
                for (int i = box.Count - 1; i >= 0; i--)
                {
                    MarketMail mail = box[i];
                    if (!mail.IsExpired(nowUnix))
                    {
                        continue;
                    }

                    box.RemoveAt(i);
                    var expiry = new MailExpiry { Mail = mail };
                    if (mail.item != null && !mail.item.IsEmpty)
                    {
                        expiry.ChestEntries.Add(mail.item.snapshot.CloneWithQuantity(mail.item.Quantity));
                    }

                    long gold = mail.gold;
                    while (gold > 0)
                    {
                        int part = (int)Math.Min(int.MaxValue, gold);
                        expiry.ChestEntries.Add(new InventorySlotData { itemId = TradeRules.GoldItemId, quantity = part });
                        gold -= part;
                    }

                    expiry.Record = MarketRecord.Of(MarketRecordKind.MailExpired, nowUnix, mail.mailId, mail.recipient, null, mail.gold, 0L, mail.item);
                    expired.Add(expiry);
                }
            }

            return expired;
        }

        private MarketMail Find(string mailId, out List<MarketMail> owner)
        {
            owner = null;
            if (string.IsNullOrEmpty(mailId))
            {
                return null;
            }

            foreach (List<MarketMail> box in _boxes.Values)
            {
                foreach (MarketMail mail in box)
                {
                    if (string.Equals(mail.mailId, mailId, StringComparison.Ordinal))
                    {
                        owner = box;
                        return mail;
                    }
                }
            }

            return null;
        }
    }
}
