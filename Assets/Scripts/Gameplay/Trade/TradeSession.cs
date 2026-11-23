using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Trade
{
    /// <summary>
    /// One line of an offer: <see cref="Quantity"/> units of the bag entry <see cref="SlotIndex"/>, as it was when it was put
    /// in (<see cref="Snapshot"/>, <see cref="Fingerprint"/>). Settlement checks the entry still is that.
    /// </summary>
    public sealed class TradeOfferLine
    {
        public readonly int SlotIndex;
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly string Fingerprint;
        public readonly bool IsRare;
        public readonly ItemRarity Rarity;

        /// <summary>A deep copy of the entry with <see cref="Quantity"/> (for the summary and the record; never the live entry).</summary>
        public readonly InventorySlotData Snapshot;

        public TradeOfferLine(int slotIndex, InventorySlotData slot, int quantity, ItemDefinition item)
        {
            SlotIndex = slotIndex;
            ItemId = slot.itemId;
            Quantity = quantity;
            Fingerprint = TradeItemRules.Fingerprint(slot);
            IsRare = TradeItemRules.IsRare(slot, item);
            Rarity = DeathDropRules.RarityOf(slot, item);
            Snapshot = slot.CloneWithQuantity(quantity);
        }
    }

    /// <summary>What one side puts on the table: item lines and gold.</summary>
    public sealed class TradeOffer
    {
        private readonly List<TradeOfferLine> _lines = new List<TradeOfferLine>();

        public IReadOnlyList<TradeOfferLine> Lines => _lines;
        public int Gold { get; internal set; }
        public bool IsEmpty => _lines.Count == 0 && Gold <= 0;

        public int IndexOfSlot(int slotIndex) => _lines.FindIndex(line => line.SlotIndex == slotIndex);

        internal void Add(TradeOfferLine line) => _lines.Add(line);
        internal void RemoveAt(int index) => _lines.RemoveAt(index);
    }

    /// <summary>A notice of the summary, shown to <see cref="Side"/>.</summary>
    public readonly struct TradeNotice
    {
        public readonly TradeWarning Warning;
        public readonly TradeSide Side;

        public TradeNotice(TradeWarning warning, TradeSide side)
        {
            Warning = warning;
            Side = side;
        }
    }

    /// <summary>
    /// The final summary both sides see after locking: what each gives, at which <see cref="Revision"/>, and the warnings.
    /// A confirmation names the revision it saw (<see cref="TradeSession.Confirm"/>), so nobody confirms an old summary.
    /// </summary>
    public sealed class TradeSummary
    {
        public readonly int Revision;
        public readonly TradeOffer GivenByA;
        public readonly TradeOffer GivenByB;
        public readonly IReadOnlyList<TradeNotice> Notices;

        public TradeSummary(int revision, TradeOffer givenByA, TradeOffer givenByB, IReadOnlyList<TradeNotice> notices)
        {
            Revision = revision;
            GivenByA = givenByA;
            GivenByB = givenByB;
            Notices = notices ?? Array.Empty<TradeNotice>();
        }

        public TradeOffer GivenBy(TradeSide side) => side == TradeSide.A ? GivenByA : GivenByB;
        public TradeOffer ReceivedBy(TradeSide side) => GivenBy(TradeRules.Other(side));

        public bool Has(TradeWarning warning, TradeSide side)
        {
            foreach (TradeNotice notice in Notices)
            {
                if (notice.Warning == warning && notice.Side == side)
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// <summary>
    /// The trade window between two players as a pure state machine (TODO.md Fase 12.3; Docs/Network.md §11). The server
    /// owns one per trade and feeds it the players' intents with its clock (<c>now</c>, seconds); nothing here touches a
    /// bag: <see cref="TradeSettlement.Settle"/> does, all or nothing.
    /// <list type="number">
    /// <item><see cref="TryOpen"/>: only when <see cref="TradeRules.CanOpen"/> says so (same city, or face to face).</item>
    /// <item>Each side puts and takes items (<see cref="AddItem"/>, <see cref="RemoveItem"/>) and gold
    /// (<see cref="SetGold"/>). <b>Any change, by either side, undoes both locks and both confirmations</b> and bumps
    /// <see cref="Revision"/>.</item>
    /// <item>Each side locks (<see cref="Lock"/>, "travar"). With both locked the state is <see cref="TradeState.Locked"/>
    /// and <see cref="BuildSummary"/> gives the final summary; each side confirms it (<see cref="Confirm"/>) naming the
    /// revision it saw. With both confirmed, <see cref="ReadyToSettle"/>.</item>
    /// <item><see cref="Cancel"/> by either side, or expiry (<see cref="Tick"/>: <see cref="TradeRules.IdleTimeoutSeconds"/>
    /// idle or <see cref="TradeRules.MaxSessionSeconds"/> in all) closes it for good.</item>
    /// </list>
    /// </summary>
    public sealed class TradeSession
    {
        private readonly TradeOffer[] _offers = { new TradeOffer(), new TradeOffer() };
        private readonly bool[] _locked = new bool[2];
        private readonly bool[] _confirmed = new bool[2];

        private TradeSession(string tradeId, string playerA, string playerB, double now)
        {
            TradeId = tradeId;
            PlayerA = playerA;
            PlayerB = playerB;
            OpenedAt = now;
            LastActivityAt = now;
            State = TradeState.Negotiating;
        }

        public string TradeId { get; }
        public string PlayerA { get; }
        public string PlayerB { get; }
        public double OpenedAt { get; }
        public double LastActivityAt { get; private set; }
        public double ClosedAt { get; private set; } = -1d;
        public TradeState State { get; private set; }

        /// <summary>Bumped on every change of either offer (and on a failed settlement).</summary>
        public int Revision { get; private set; }

        /// <summary>Who cancelled (null when not cancelled).</summary>
        public TradeSide? CancelledBy { get; private set; }

        /// <summary>Why the last settlement failed (<see cref="TradeRefusal.None"/> otherwise).</summary>
        public TradeRefusal LastSettleRefusal { get; private set; }

        public bool IsOpen => State == TradeState.Negotiating || State == TradeState.Locked;
        public bool ReadyToSettle => State == TradeState.Locked && _confirmed[0] && _confirmed[1];

        /// <summary>
        /// Opens a trade between <paramref name="a"/> (asked) and <paramref name="b"/> (accepted) standing
        /// <paramref name="distance"/> m apart. <paramref name="tradeId"/> comes from the server (unique; it is the key of the log).
        /// </summary>
        public static TradeRefusal TryOpen(string tradeId, TradePresence a, TradePresence b, float distance, double now, out TradeSession session)
        {
            session = null;
            if (string.IsNullOrEmpty(tradeId))
            {
                return TradeRefusal.SessionClosed;
            }

            TradeRefusal refusal = TradeRules.CanOpen(a, b, distance);
            if (refusal != TradeRefusal.None)
            {
                return refusal;
            }

            session = new TradeSession(tradeId, a.PlayerId, b.PlayerId, now);
            return TradeRefusal.None;
        }

        public string PlayerOf(TradeSide side) => side == TradeSide.A ? PlayerA : PlayerB;

        public bool TryGetSide(string playerId, out TradeSide side)
        {
            side = TradeSide.A;
            if (string.IsNullOrEmpty(playerId))
            {
                return false;
            }

            if (string.Equals(playerId, PlayerA, StringComparison.Ordinal))
            {
                return true;
            }

            side = TradeSide.B;
            return string.Equals(playerId, PlayerB, StringComparison.Ordinal);
        }

        public TradeOffer OfferOf(TradeSide side) => _offers[(int)side];
        public bool IsLocked(TradeSide side) => _locked[(int)side];
        public bool IsConfirmed(TradeSide side) => _confirmed[(int)side];

        /// <summary>
        /// Puts <paramref name="quantity"/> units of bag entry <paramref name="slotIndex"/> of <paramref name="side"/>'s bag
        /// (<paramref name="bag"/>, the server's copy) on the table. One line per bag entry: the same entry twice is
        /// <see cref="TradeRefusal.AlreadyOffered"/> (take it out and put it back with the new quantity).
        /// </summary>
        public TradeRefusal AddItem(TradeSide side, IReadOnlyList<InventorySlotData> bag, int slotIndex, int quantity,
            Func<string, ItemDefinition> resolve, double now)
        {
            TradeRefusal state = CheckOpen(now);
            if (state != TradeRefusal.None)
            {
                return state;
            }

            if (bag == null || slotIndex < 0 || slotIndex >= bag.Count)
            {
                return TradeRefusal.InvalidSlot;
            }

            InventorySlotData slot = bag[slotIndex];
            if (slot == null || slot.IsEmpty)
            {
                return TradeRefusal.EmptySlot;
            }

            if (quantity <= 0)
            {
                return TradeRefusal.InvalidQuantity;
            }

            if (quantity > slot.quantity)
            {
                return TradeRefusal.NotEnoughItems;
            }

            TradeOffer offer = OfferOf(side);
            if (offer.IndexOfSlot(slotIndex) >= 0)
            {
                return TradeRefusal.AlreadyOffered;
            }

            if (offer.Lines.Count >= TradeRules.MaxItemLinesPerSide)
            {
                return TradeRefusal.TooManyLines;
            }

            ItemDefinition item = resolve?.Invoke(slot.itemId);
            TradeRefusal tradeable = TradeItemRules.CanTrade(slot, item);
            if (tradeable != TradeRefusal.None)
            {
                return tradeable;
            }

            offer.Add(new TradeOfferLine(slotIndex, slot, quantity, item));
            Changed(now);
            return TradeRefusal.None;
        }

        /// <summary>Takes the line of bag entry <paramref name="slotIndex"/> off <paramref name="side"/>'s offer.</summary>
        public TradeRefusal RemoveItem(TradeSide side, int slotIndex, double now)
        {
            TradeRefusal state = CheckOpen(now);
            if (state != TradeRefusal.None)
            {
                return state;
            }

            TradeOffer offer = OfferOf(side);
            int index = offer.IndexOfSlot(slotIndex);
            if (index < 0)
            {
                return TradeRefusal.InvalidSlot;
            }

            offer.RemoveAt(index);
            Changed(now);
            return TradeRefusal.None;
        }

        /// <summary>Sets <paramref name="side"/>'s gold to <paramref name="gold"/> (0 takes it off); <paramref name="goldHeld"/> = the gold in that bag now.</summary>
        public TradeRefusal SetGold(TradeSide side, int gold, long goldHeld, double now)
        {
            TradeRefusal state = CheckOpen(now);
            if (state != TradeRefusal.None)
            {
                return state;
            }

            if (gold < 0)
            {
                return TradeRefusal.InvalidQuantity;
            }

            if (gold > TradeRules.MaxGoldPerTrade)
            {
                return TradeRefusal.GoldOverTradeLimit;
            }

            if (gold > goldHeld)
            {
                return TradeRefusal.NotEnoughGold;
            }

            TradeOffer offer = OfferOf(side);
            if (offer.Gold == gold)
            {
                LastActivityAt = now;
                return TradeRefusal.None;
            }

            offer.Gold = gold;
            Changed(now);
            return TradeRefusal.None;
        }

        /// <summary>"Travar": <paramref name="side"/> is happy with both offers as they are. Both locked → <see cref="TradeState.Locked"/>.</summary>
        public TradeRefusal Lock(TradeSide side, double now)
        {
            TradeRefusal state = CheckOpen(now);
            if (state != TradeRefusal.None)
            {
                return state;
            }

            _locked[(int)side] = true;
            LastActivityAt = now;
            if (_locked[0] && _locked[1])
            {
                State = TradeState.Locked;
            }

            return TradeRefusal.None;
        }

        /// <summary>Takes <paramref name="side"/>'s lock back: like any change, it undoes both locks and both confirmations.</summary>
        public TradeRefusal Unlock(TradeSide side, double now)
        {
            TradeRefusal state = CheckOpen(now);
            if (state != TradeRefusal.None)
            {
                return state;
            }

            Changed(now);
            return TradeRefusal.None;
        }

        /// <summary>The final summary (only meaningful once <see cref="TradeState.Locked"/>; built at any time for previews).</summary>
        public TradeSummary BuildSummary()
        {
            var notices = new List<TradeNotice>();
            foreach (TradeSide side in new[] { TradeSide.A, TradeSide.B })
            {
                TradeOffer given = OfferOf(side);
                TradeOffer received = OfferOf(TradeRules.Other(side));

                foreach (TradeOfferLine line in given.Lines)
                {
                    if (line.IsRare)
                    {
                        notices.Add(new TradeNotice(TradeWarning.RareItemGiven, side));
                        break;
                    }
                }

                if (!given.IsEmpty && received.IsEmpty)
                {
                    notices.Add(new TradeNotice(TradeWarning.GivingForNothing, side));
                }

                if (given.Gold >= TradeRules.LargeGoldWarning)
                {
                    notices.Add(new TradeNotice(TradeWarning.LargeGold, side));
                }
            }

            return new TradeSummary(Revision, OfferOf(TradeSide.A), OfferOf(TradeSide.B), notices);
        }

        /// <summary>
        /// The final confirmation of <paramref name="side"/>, for the summary of <paramref name="revisionSeen"/>. Needs both
        /// locked (<see cref="TradeRefusal.NotLocked"/>) and the current revision (<see cref="TradeRefusal.StaleSummary"/>:
        /// something changed since that summary was shown). An empty trade on both sides is <see cref="TradeRefusal.EmptyTrade"/>.
        /// </summary>
        public TradeRefusal Confirm(TradeSide side, int revisionSeen, double now)
        {
            TradeRefusal state = CheckOpen(now);
            if (state != TradeRefusal.None)
            {
                return state;
            }

            if (State != TradeState.Locked)
            {
                return TradeRefusal.NotLocked;
            }

            if (revisionSeen != Revision)
            {
                return TradeRefusal.StaleSummary;
            }

            if (_offers[0].IsEmpty && _offers[1].IsEmpty)
            {
                return TradeRefusal.EmptyTrade;
            }

            _confirmed[(int)side] = true;
            LastActivityAt = now;
            return TradeRefusal.None;
        }

        /// <summary>Closes the window for good (a side cancelled, walked away or went offline). False when already closed.</summary>
        public bool Cancel(TradeSide side, double now)
        {
            if (!IsOpen)
            {
                return false;
            }

            CancelledBy = side;
            Close(TradeState.Cancelled, now);
            return true;
        }

        /// <summary>Expires the window when it is due; true only on the call that expired it.</summary>
        public bool Tick(double now)
        {
            if (!IsOpen || !IsDue(now))
            {
                return false;
            }

            Close(TradeState.Expired, now);
            return true;
        }

        public bool IsDue(double now)
        {
            return now - LastActivityAt >= TradeRules.IdleTimeoutSeconds || now - OpenedAt >= TradeRules.MaxSessionSeconds;
        }

        // ------------------------------------------------------------------
        // Settlement hooks (TradeSettlement)
        // ------------------------------------------------------------------

        /// <summary>Open, not expired (expires it when due), or why not.</summary>
        internal TradeRefusal CheckOpen(double now)
        {
            if (State == TradeState.Completed)
            {
                return TradeRefusal.AlreadySettled;
            }

            if (State == TradeState.Expired || Tick(now))
            {
                return TradeRefusal.Expired;
            }

            return IsOpen ? TradeRefusal.None : TradeRefusal.SessionClosed;
        }

        internal void MarkCompleted(double now)
        {
            LastSettleRefusal = TradeRefusal.None;
            Close(TradeState.Completed, now);
        }

        /// <summary>A failed settlement: nothing moved; both sides are unlocked and must look at the offers again.</summary>
        internal void MarkSettleFailed(TradeRefusal refusal, double now)
        {
            LastSettleRefusal = refusal;
            Changed(now);
        }

        private void Changed(double now)
        {
            Revision++;
            _locked[0] = _locked[1] = false;
            _confirmed[0] = _confirmed[1] = false;
            State = TradeState.Negotiating;
            LastActivityAt = now;
        }

        private void Close(TradeState final, double now)
        {
            State = final;
            ClosedAt = now;
            _confirmed[0] = _confirmed[1] = false;
        }
    }
}
