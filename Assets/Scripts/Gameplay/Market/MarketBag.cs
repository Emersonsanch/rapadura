using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Trade;

namespace Rapadura.Gameplay.Market
{
    /// <summary>
    /// An item held by the market (a stall line's stock, an auctioned piece, a mail's attachment): a deep copy of the bag
    /// entry it came from, taken out of the bag when it was put up, so it exists in exactly one place at a time.
    /// </summary>
    [Serializable]
    public sealed class MarketItem
    {
        /// <summary>The entry as it was (rolled data, refine, options); <c>quantity</c> = units held.</summary>
        public InventorySlotData snapshot;

        /// <summary><see cref="TradeItemRules.Fingerprint"/> of <see cref="snapshot"/>.</summary>
        public string fingerprint;

        public string ItemId => snapshot != null ? snapshot.itemId : null;
        public int Quantity => snapshot != null ? snapshot.quantity : 0;
        public int RefineLevel => snapshot != null ? snapshot.refineLevel : 0;
        public bool IsEmpty => snapshot == null || snapshot.IsEmpty;

        public static MarketItem From(InventorySlotData slot, int quantity)
        {
            InventorySlotData copy = slot.CloneWithQuantity(quantity);
            copy.gridX = 0;
            copy.gridY = 0;
            return new MarketItem { snapshot = copy, fingerprint = TradeItemRules.Fingerprint(copy) };
        }

        /// <summary>A copy of <paramref name="quantity"/> units of this item (the stock itself is not touched).</summary>
        public MarketItem Split(int quantity) => From(snapshot, quantity);
    }

    /// <summary>One "put this bag entry up" request: <see cref="Quantity"/> units of the entry at <see cref="SlotIndex"/>.</summary>
    public readonly struct MarketPick
    {
        public readonly int SlotIndex;
        public readonly int Quantity;

        /// <summary>Unit price of a stall line (ignored by the auction house, which prices the listing).</summary>
        public readonly int UnitPrice;

        public MarketPick(int slotIndex, int quantity, int unitPrice = 0)
        {
            SlotIndex = slotIndex;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
    }

    /// <summary>
    /// The bag side of every market operation, on deep copies through the trade's own bag model
    /// (<see cref="TradeSettlement.TradeBag"/>: same stacking, MU grid placement and gold handling as a trade), so a market
    /// operation either returns a whole new bag or changes nothing. The server writes the new bag and the market state in
    /// one transaction.
    /// </summary>
    internal static class MarketBag
    {
        /// <summary>
        /// Takes <paramref name="picks"/> (and <paramref name="gold"/> gold) out of a copy of the bag: each entry once, still
        /// held, enough units, tradeable. On success <paramref name="taken"/> holds the copies in pick order.
        /// </summary>
        public static MarketRefusal TryTake(TradeSideInput input, IReadOnlyList<MarketPick> picks, long gold,
            Func<string, ItemDefinition> resolve, out InventorySlotData[] bagAfter, out List<MarketItem> taken, out TradeRefusal itemReason)
        {
            bagAfter = null;
            taken = new List<MarketItem>();
            itemReason = TradeRefusal.None;
            if (input == null || input.Slots == null || resolve == null)
            {
                return MarketRefusal.NotAvailable;
            }

            var seen = new HashSet<int>();
            foreach (MarketPick pick in picks)
            {
                if (pick.SlotIndex < 0 || pick.SlotIndex >= input.Slots.Count)
                {
                    return MarketRefusal.InvalidSlot;
                }

                if (!seen.Add(pick.SlotIndex))
                {
                    return MarketRefusal.AlreadyListed;
                }

                InventorySlotData slot = input.Slots[pick.SlotIndex];
                if (slot == null || slot.IsEmpty)
                {
                    return MarketRefusal.EmptySlot;
                }

                if (pick.Quantity <= 0)
                {
                    return MarketRefusal.InvalidQuantity;
                }

                if (slot.quantity < pick.Quantity)
                {
                    return MarketRefusal.NotEnoughItems;
                }

                TradeRefusal tradeable = TradeItemRules.CanTrade(slot, resolve(slot.itemId));
                if (tradeable != TradeRefusal.None)
                {
                    itemReason = tradeable;
                    return MarketRefusal.NotTradeable;
                }
            }

            if (gold < 0)
            {
                return MarketRefusal.InvalidPrice;
            }

            if (gold > TradeSettlement.GoldIn(input.Slots))
            {
                return MarketRefusal.NotEnoughGold;
            }

            var bag = new TradeSettlement.TradeBag(input.Slots, input.Columns, input.Rows, resolve);
            foreach (MarketPick pick in picks)
            {
                taken.Add(MarketItem.From(input.Slots[pick.SlotIndex], pick.Quantity));
                if (!bag.Remove(pick.SlotIndex, pick.Quantity))
                {
                    taken.Clear();
                    return MarketRefusal.NotEnoughItems;
                }
            }

            if (gold > 0 && !bag.RemovePlain(TradeRules.GoldItemId, gold))
            {
                taken.Clear();
                return MarketRefusal.NotEnoughGold;
            }

            bagAfter = bag.ToArray();
            return MarketRefusal.None;
        }

        /// <summary>
        /// Pays <paramref name="goldOut"/> and receives <paramref name="goldIn"/> plus <paramref name="items"/> on a copy of
        /// the bag: enough gold, nobody above <see cref="TradeRules.MaxGoldHeld"/>, room on the grid (paying first) and the
        /// load not over capacity unless it gets lighter.
        /// </summary>
        public static MarketRefusal TryExchange(TradeSideInput input, long goldOut, long goldIn, IReadOnlyList<MarketItem> items,
            Func<string, ItemDefinition> resolve, out InventorySlotData[] bagAfter)
        {
            bagAfter = null;
            if (input == null || input.Slots == null || resolve == null)
            {
                return MarketRefusal.NotAvailable;
            }

            if (goldOut < 0 || goldIn < 0)
            {
                return MarketRefusal.InvalidPrice;
            }

            long held = TradeSettlement.GoldIn(input.Slots);
            if (goldOut > held)
            {
                return MarketRefusal.NotEnoughGold;
            }

            if (held - goldOut + goldIn > TradeRules.MaxGoldHeld)
            {
                return MarketRefusal.GoldOverHeldLimit;
            }

            var bag = new TradeSettlement.TradeBag(input.Slots, input.Columns, input.Rows, resolve);
            if (goldOut > 0 && !bag.RemovePlain(TradeRules.GoldItemId, goldOut))
            {
                return MarketRefusal.NotEnoughGold;
            }

            if (items != null)
            {
                foreach (MarketItem item in items)
                {
                    if (item == null || item.IsEmpty)
                    {
                        continue;
                    }

                    ItemDefinition definition = resolve(item.ItemId);
                    if (definition == null)
                    {
                        return MarketRefusal.NotTradeable;
                    }

                    InventorySlotData piece = item.snapshot.CloneWithQuantity(item.Quantity);
                    bool placed = !piece.HasGeneratedData && definition.IsStackable
                        ? bag.AddStack(definition, item.Quantity, definition.HasDurability ? piece.currentDurability : (float?)null)
                        : bag.AddPiece(piece, definition);
                    if (!placed)
                    {
                        return MarketRefusal.BagFull;
                    }
                }
            }

            if (goldIn > 0)
            {
                ItemDefinition goldItem = resolve(TradeRules.GoldItemId);
                if (goldItem == null || !bag.AddStack(goldItem, goldIn, null))
                {
                    return MarketRefusal.BagFull;
                }
            }

            if (input.Capacity > 0f)
            {
                float before = EncumbranceRules.WeightOf(input.Slots, resolve) + input.EquippedWeight;
                float load = bag.Weight() + input.EquippedWeight;
                if (load > input.Capacity + 0.0001f && load > before + 0.0001f)
                {
                    return MarketRefusal.TooHeavy;
                }
            }

            bagAfter = bag.ToArray();
            return MarketRefusal.None;
        }
    }
}
