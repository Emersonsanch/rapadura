using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Trade
{
    /// <summary>One side's state at settlement time, as the server holds it (never the live bag: it is only read).</summary>
    public sealed class TradeSideInput
    {
        /// <summary>The bag entries (<see cref="InventoryManager.Slots"/> or the server's copy); indices are the offer's slot indices.</summary>
        public IReadOnlyList<InventorySlotData> Slots;

        public int Columns = InventoryManager.DefaultColumns;
        public int Rows = InventoryManager.DefaultRows;

        /// <summary>Weight of the equipped pieces (part of the carry load, <see cref="EncumbranceRules"/>).</summary>
        public float EquippedWeight;

        /// <summary>Carry capacity (<see cref="EncumbranceRules.HeroCapacity"/>); 0 or less = no weight check.</summary>
        public float Capacity;

        public TradeAccountStanding Standing = TradeAccountStanding.Veteran;
        public TradeDailyTally Tally;
    }

    /// <summary>What <see cref="TradeSettlement.Settle"/> did. On failure nothing moved and the inputs are as they were.</summary>
    public sealed class TradeSettlementResult
    {
        public bool Success;
        public TradeRefusal Refusal;

        /// <summary>The side the refusal is about (whose item is gone, whose bag is full…); null for session-level refusals.</summary>
        public TradeSide? FailedSide;

        /// <summary>The bags after the trade (new arrays, same entry count as the inputs); null on failure.</summary>
        public InventorySlotData[] SlotsA;
        public InventorySlotData[] SlotsB;

        /// <summary>The daily tallies after the trade (rolled to today); on failure, the inputs' unchanged.</summary>
        public TradeDailyTally TallyA;
        public TradeDailyTally TallyB;

        /// <summary>The economy-log entry; null on failure.</summary>
        public TradeRecord Record;

        public InventorySlotData[] SlotsOf(TradeSide side) => side == TradeSide.A ? SlotsA : SlotsB;
        public TradeDailyTally TallyOf(TradeSide side) => side == TradeSide.A ? TallyA : TallyB;
    }

    /// <summary>
    /// Concludes a confirmed <see cref="TradeSession"/>: re-validates everything against the bags as they are <i>now</i>
    /// and swaps the offers <b>all or nothing</b> (TODO.md Fase 12.3, "proteção contra duplicação de item"). Pure: it
    /// works on deep copies and returns the new bags; the server writes them (and the <see cref="TradeRecord"/>) in one
    /// transaction. Checks, in order (the first failure is returned, the session goes back to negotiating, unlocked):
    /// <list type="number">
    /// <item>The session is open, both confirmed (<see cref="TradeRefusal.NotConfirmed"/>), not settled before
    /// (<see cref="TradeRefusal.AlreadySettled"/>: a second settle never pays twice), not expired.</item>
    /// <item>Each line still is in its bag entry: same id and <see cref="TradeItemRules.Fingerprint"/>
    /// (<see cref="TradeRefusal.ItemChanged"/>), enough units (<see cref="TradeRefusal.ItemNoLongerHeld"/>: spent, sold,
    /// moved or dropped between locking and concluding), one line per entry (the same stack twice is
    /// <see cref="TradeRefusal.AlreadyOffered"/>), and still tradeable.</item>
    /// <item>The gold is still there (<see cref="TradeRefusal.GoldNoLongerHeld"/>), within <see cref="TradeRules.MaxGoldPerTrade"/>,
    /// and nobody ends above <see cref="TradeRules.MaxGoldHeld"/>.</item>
    /// <item>New-account limits (<see cref="TradeAccountProtection"/>).</item>
    /// <item>Each bag, after giving its side, has room on its grid for what it receives (<see cref="TradeRefusal.BagFull"/>)
    /// and the load does not go over capacity unless it gets lighter (<see cref="TradeRefusal.TooHeavy"/>).</item>
    /// </list>
    /// </summary>
    public static class TradeSettlement
    {
        public static TradeSettlementResult Settle(TradeSession session, TradeSideInput a, TradeSideInput b,
            Func<string, ItemDefinition> resolve, double now, long unixNow)
        {
            var result = new TradeSettlementResult { TallyA = a != null ? a.Tally : default, TallyB = b != null ? b.Tally : default };

            if (session == null)
            {
                return Fail(result, TradeRefusal.SessionClosed, null);
            }

            TradeRefusal open = session.CheckOpen(now);
            if (open != TradeRefusal.None)
            {
                return Fail(result, open, null);
            }

            if (!session.ReadyToSettle)
            {
                return Fail(result, TradeRefusal.NotConfirmed, null);
            }

            if (a == null || b == null || a.Slots == null || b.Slots == null || resolve == null)
            {
                return FailAndReset(session, result, TradeRefusal.PartnerUnavailable, null, now);
            }

            TradeOffer offerA = session.OfferOf(TradeSide.A);
            TradeOffer offerB = session.OfferOf(TradeSide.B);

            // 1) Every line and the gold are still where they were.
            foreach (TradeSide side in new[] { TradeSide.A, TradeSide.B })
            {
                TradeSideInput input = side == TradeSide.A ? a : b;
                TradeRefusal refusal = ValidateOffer(session.OfferOf(side), input.Slots, resolve);
                if (refusal != TradeRefusal.None)
                {
                    return FailAndReset(session, result, refusal, side, now);
                }
            }

            long goldA = GoldIn(a.Slots);
            long goldB = GoldIn(b.Slots);
            if (offerA.Gold > goldA)
            {
                return FailAndReset(session, result, TradeRefusal.GoldNoLongerHeld, TradeSide.A, now);
            }

            if (offerB.Gold > goldB)
            {
                return FailAndReset(session, result, TradeRefusal.GoldNoLongerHeld, TradeSide.B, now);
            }

            if (goldA - offerA.Gold + offerB.Gold > TradeRules.MaxGoldHeld)
            {
                return FailAndReset(session, result, TradeRefusal.GoldOverHeldLimit, TradeSide.A, now);
            }

            if (goldB - offerB.Gold + offerA.Gold > TradeRules.MaxGoldHeld)
            {
                return FailAndReset(session, result, TradeRefusal.GoldOverHeldLimit, TradeSide.B, now);
            }

            // 2) New accounts.
            string day = TradeRules.DayKey(unixNow);
            TradeDailyTally tallyA = a.Tally.ForDay(day);
            TradeDailyTally tallyB = b.Tally.ForDay(day);
            TradeRefusal limitA = TradeAccountProtection.Check(a.Standing, tallyA, offerB.Gold, offerA.Gold, offerB.Lines.Count, offerA.Lines.Count);
            if (limitA != TradeRefusal.None)
            {
                return FailAndReset(session, result, limitA, TradeSide.A, now);
            }

            TradeRefusal limitB = TradeAccountProtection.Check(b.Standing, tallyB, offerA.Gold, offerB.Gold, offerA.Lines.Count, offerB.Lines.Count);
            if (limitB != TradeRefusal.None)
            {
                return FailAndReset(session, result, limitB, TradeSide.B, now);
            }

            // 3) Build both bags on copies: give first (frees room), then receive.
            var bagA = new TradeBag(a.Slots, a.Columns, a.Rows, resolve);
            var bagB = new TradeBag(b.Slots, b.Columns, b.Rows, resolve);

            TradeRefusal give = Give(bagA, offerA) ?? Give(bagB, offerB) ?? TradeRefusal.None;
            if (give != TradeRefusal.None)
            {
                return FailAndReset(session, result, give, null, now);
            }

            TradeRefusal receiveA = Receive(bagA, offerB, b.Slots, resolve);
            if (receiveA != TradeRefusal.None)
            {
                return FailAndReset(session, result, receiveA, TradeSide.A, now);
            }

            TradeRefusal receiveB = Receive(bagB, offerA, a.Slots, resolve);
            if (receiveB != TradeRefusal.None)
            {
                return FailAndReset(session, result, receiveB, TradeSide.B, now);
            }

            // 4) Weight.
            if (GetsTooHeavy(a, bagA, resolve))
            {
                return FailAndReset(session, result, TradeRefusal.TooHeavy, TradeSide.A, now);
            }

            if (GetsTooHeavy(b, bagB, resolve))
            {
                return FailAndReset(session, result, TradeRefusal.TooHeavy, TradeSide.B, now);
            }

            // Done: one atomic swap.
            result.Success = true;
            result.Refusal = TradeRefusal.None;
            result.SlotsA = bagA.ToArray();
            result.SlotsB = bagB.ToArray();
            result.TallyA = tallyA.With(day, offerB.Gold, offerA.Gold, offerB.Lines.Count, offerA.Lines.Count);
            result.TallyB = tallyB.With(day, offerA.Gold, offerB.Gold, offerA.Lines.Count, offerB.Lines.Count);
            result.Record = TradeRecord.From(session, unixNow, now);
            session.MarkCompleted(now);
            return result;
        }

        /// <summary>Gold held in <paramref name="slots"/> (every <see cref="TradeRules.GoldItemId"/> entry).</summary>
        public static long GoldIn(IReadOnlyList<InventorySlotData> slots)
        {
            long total = 0;
            if (slots == null)
            {
                return total;
            }

            foreach (InventorySlotData slot in slots)
            {
                if (slot != null && !slot.IsEmpty && string.Equals(slot.itemId, TradeRules.GoldItemId, StringComparison.Ordinal))
                {
                    total += slot.quantity;
                }
            }

            return total;
        }

        // ------------------------------------------------------------------

        private static TradeRefusal ValidateOffer(TradeOffer offer, IReadOnlyList<InventorySlotData> slots, Func<string, ItemDefinition> resolve)
        {
            if (offer.Gold < 0)
            {
                return TradeRefusal.InvalidQuantity;
            }

            if (offer.Gold > TradeRules.MaxGoldPerTrade)
            {
                return TradeRefusal.GoldOverTradeLimit;
            }

            if (offer.Lines.Count > TradeRules.MaxItemLinesPerSide)
            {
                return TradeRefusal.TooManyLines;
            }

            var seen = new HashSet<int>();
            foreach (TradeOfferLine line in offer.Lines)
            {
                if (!seen.Add(line.SlotIndex))
                {
                    return TradeRefusal.AlreadyOffered;
                }

                if (line.Quantity <= 0)
                {
                    return TradeRefusal.InvalidQuantity;
                }

                InventorySlotData slot = line.SlotIndex >= 0 && line.SlotIndex < slots.Count ? slots[line.SlotIndex] : null;
                if (slot == null || slot.IsEmpty || !string.Equals(slot.itemId, line.ItemId, StringComparison.Ordinal))
                {
                    return TradeRefusal.ItemNoLongerHeld;
                }

                if (!string.Equals(TradeItemRules.Fingerprint(slot), line.Fingerprint, StringComparison.Ordinal))
                {
                    return TradeRefusal.ItemChanged;
                }

                if (slot.quantity < line.Quantity)
                {
                    return TradeRefusal.ItemNoLongerHeld;
                }

                TradeRefusal tradeable = TradeItemRules.CanTrade(slot, resolve(slot.itemId));
                if (tradeable != TradeRefusal.None)
                {
                    return tradeable;
                }
            }

            return TradeRefusal.None;
        }

        private static TradeRefusal? Give(TradeBag bag, TradeOffer offer)
        {
            foreach (TradeOfferLine line in offer.Lines)
            {
                if (!bag.Remove(line.SlotIndex, line.Quantity))
                {
                    return TradeRefusal.ItemNoLongerHeld;
                }
            }

            if (offer.Gold > 0 && !bag.RemovePlain(TradeRules.GoldItemId, offer.Gold))
            {
                return TradeRefusal.GoldNoLongerHeld;
            }

            return null;
        }

        private static TradeRefusal Receive(TradeBag bag, TradeOffer offer, IReadOnlyList<InventorySlotData> giverSlots, Func<string, ItemDefinition> resolve)
        {
            foreach (TradeOfferLine line in offer.Lines)
            {
                InventorySlotData source = giverSlots[line.SlotIndex];
                ItemDefinition item = resolve(source.itemId);
                if (item == null)
                {
                    return TradeRefusal.UnknownItem;
                }

                InventorySlotData piece = source.CloneWithQuantity(line.Quantity);
                bool placed = !piece.HasGeneratedData && item.IsStackable
                    ? bag.AddStack(item, line.Quantity, item.HasDurability ? piece.currentDurability : (float?)null)
                    : bag.AddPiece(piece, item);
                if (!placed)
                {
                    return TradeRefusal.BagFull;
                }
            }

            if (offer.Gold > 0)
            {
                ItemDefinition gold = resolve(TradeRules.GoldItemId);
                if (gold == null)
                {
                    return TradeRefusal.UnknownItem;
                }

                if (!bag.AddStack(gold, offer.Gold, null))
                {
                    return TradeRefusal.BagFull;
                }
            }

            return TradeRefusal.None;
        }

        private static bool GetsTooHeavy(TradeSideInput input, TradeBag after, Func<string, ItemDefinition> resolve)
        {
            if (input.Capacity <= 0f)
            {
                return false;
            }

            float before = EncumbranceRules.WeightOf(input.Slots, resolve) + input.EquippedWeight;
            float load = after.Weight() + input.EquippedWeight;
            return load > input.Capacity + 0.0001f && load > before + 0.0001f;
        }

        private static TradeSettlementResult Fail(TradeSettlementResult result, TradeRefusal refusal, TradeSide? side)
        {
            result.Success = false;
            result.Refusal = refusal;
            result.FailedSide = side;
            result.SlotsA = null;
            result.SlotsB = null;
            result.Record = null;
            return result;
        }

        private static TradeSettlementResult FailAndReset(TradeSession session, TradeSettlementResult result, TradeRefusal refusal, TradeSide? side, double now)
        {
            session.MarkSettleFailed(refusal, now);
            return Fail(result, refusal, side);
        }

        /// <summary>
        /// A throwaway copy of a bag that places like <see cref="InventoryManager.AddItem(ItemDefinition,int)"/> and
        /// <see cref="InventoryManager.TryAddCopy"/> do (plain stacks first in grid order, then the first free area of the
        /// footprint, MU scan order), on plain data so the server runs it too.
        /// </summary>
        private sealed class TradeBag
        {
            private readonly InventorySlotData[] _slots;
            private readonly int _columns;
            private readonly int _rows;
            private readonly Func<string, ItemDefinition> _resolve;

            public TradeBag(IReadOnlyList<InventorySlotData> source, int columns, int rows, Func<string, ItemDefinition> resolve)
            {
                _columns = Math.Max(1, columns);
                _rows = Math.Max(1, rows);
                _resolve = resolve;
                int count = source?.Count ?? 0;
                _slots = new InventorySlotData[count];
                for (int i = 0; i < count; i++)
                {
                    InventorySlotData slot = source[i];
                    _slots[i] = slot == null || slot.IsEmpty ? InventorySlotData.Empty() : slot.CloneWithQuantity(slot.quantity);
                }
            }

            public InventorySlotData[] ToArray() => _slots;

            public float Weight() => EncumbranceRules.WeightOf(_slots, _resolve);

            public bool Remove(int index, int quantity)
            {
                if (index < 0 || index >= _slots.Length || _slots[index].IsEmpty || _slots[index].quantity < quantity)
                {
                    return false;
                }

                _slots[index].quantity -= quantity;
                if (_slots[index].quantity <= 0)
                {
                    _slots[index].Clear();
                }

                return true;
            }

            /// <summary>Takes <paramref name="quantity"/> plain units of <paramref name="itemId"/>, last entries first.</summary>
            public bool RemovePlain(string itemId, long quantity)
            {
                List<int> order = GridOrder();
                long held = 0;
                foreach (int i in order)
                {
                    if (IsPlain(_slots[i], itemId))
                    {
                        held += _slots[i].quantity;
                    }
                }

                if (held < quantity)
                {
                    return false;
                }

                long left = quantity;
                for (int k = order.Count - 1; k >= 0 && left > 0; k--)
                {
                    InventorySlotData slot = _slots[order[k]];
                    if (!IsPlain(slot, itemId))
                    {
                        continue;
                    }

                    int take = (int)Math.Min(slot.quantity, left);
                    slot.quantity -= take;
                    left -= take;
                    if (slot.quantity <= 0)
                    {
                        slot.Clear();
                    }
                }

                return true;
            }

            public bool AddStack(ItemDefinition item, long quantity, float? durability)
            {
                long remaining = quantity;
                if (item.IsStackable)
                {
                    foreach (int i in GridOrder())
                    {
                        if (remaining <= 0)
                        {
                            break;
                        }

                        InventorySlotData slot = _slots[i];
                        if (!IsPlain(slot, item.ItemId) || slot.quantity >= item.MaxStack)
                        {
                            continue;
                        }

                        int add = (int)Math.Min(item.MaxStack - slot.quantity, remaining);
                        slot.quantity += add;
                        remaining -= add;
                    }
                }

                while (remaining > 0)
                {
                    int entry = Reserve(item);
                    if (entry < 0)
                    {
                        return false;
                    }

                    int add = (int)Math.Min(item.MaxStack, remaining);
                    InventorySlotData slot = _slots[entry];
                    slot.itemId = item.ItemId;
                    slot.quantity = add;
                    slot.currentDurability = item.HasDurability ? (durability ?? item.MaxDurability) : 0f;
                    remaining -= add;
                }

                return true;
            }

            public bool AddPiece(InventorySlotData piece, ItemDefinition item)
            {
                int entry = Reserve(item);
                if (entry < 0)
                {
                    return false;
                }

                int x = _slots[entry].gridX;
                int y = _slots[entry].gridY;
                _slots[entry] = piece.CloneWithQuantity(piece.quantity);
                _slots[entry].gridX = x;
                _slots[entry].gridY = y;
                return true;
            }

            private int Reserve(ItemDefinition item)
            {
                int entry = Array.FindIndex(_slots, s => s.IsEmpty);
                if (entry < 0)
                {
                    return -1;
                }

                BagGrid grid = BagGridRules.Build(_columns, _rows, _slots, _resolve);
                if (!grid.TryFindFirstFit(ItemFootprint.For(item, _columns, _rows), out int x, out int y))
                {
                    return -1;
                }

                _slots[entry].Clear();
                _slots[entry].itemId = item.ItemId;
                _slots[entry].quantity = 1; // occupied from now on (the caller sets the real quantity)
                _slots[entry].gridX = x;
                _slots[entry].gridY = y;
                return entry;
            }

            private List<int> GridOrder()
            {
                var order = new List<int>();
                for (int i = 0; i < _slots.Length; i++)
                {
                    if (!_slots[i].IsEmpty)
                    {
                        order.Add(i);
                    }
                }

                order.Sort((p, q) =>
                {
                    int byRow = _slots[p].gridY.CompareTo(_slots[q].gridY);
                    if (byRow != 0)
                    {
                        return byRow;
                    }

                    int byColumn = _slots[p].gridX.CompareTo(_slots[q].gridX);
                    return byColumn != 0 ? byColumn : p.CompareTo(q);
                });
                return order;
            }

            private static bool IsPlain(InventorySlotData slot, string itemId)
            {
                return slot != null && !slot.IsEmpty && !slot.HasGeneratedData && string.Equals(slot.itemId, itemId, StringComparison.Ordinal);
            }
        }
    }
}
