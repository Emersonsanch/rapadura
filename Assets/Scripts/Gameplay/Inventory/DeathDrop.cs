using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// "Na pressa, você deixou cair…": the 9% death drop (Docs/Balance/README.md, "Queda de item ao morrer"). Pure and
    /// seeded (a <see cref="Random"/> is passed in), EditMode-tested.
    /// <list type="bullet">
    /// <item>Each player death rolls <see cref="DropChance"/> once.</item>
    /// <item>On success one random eligible backpack entry falls — the whole entry (a stack falls as a stack).
    /// Equipped gear is never touched (it is not in the backpack grid).</item>
    /// <item>Not eligible (<see cref="IsEligible"/>): gold/currency, quest and story items, relics, items flagged
    /// <see cref="ItemDefinition.IsBound"/>, unknown ids.</item>
    /// <item>Offline the item goes to "Achados e Perdidos" at Vó Zefa's (<see cref="LostAndFoundList"/>), to buy back for
    /// <see cref="BuybackFee"/> gold, so a kid never loses an item for good.</item>
    /// </list>
    /// </summary>
    public static class DeathDropRules
    {
        /// <summary>Chance per death that one item falls out of the backpack.</summary>
        public const float DropChance = 0.09f;

        /// <summary>Gold the buy-back costs at "Achados e Perdidos", per rarity (flat; the whole entry comes back).</summary>
        public static readonly int[] BuybackFeeByRarity = { 10, 25, 60, 150, 400 };

        /// <summary>Id prefix reserved for relic items, should relics ever become bag items (today they live in SecretsProgress).</summary>
        public const string RelicIdPrefix = "relic_";

        public static bool RollsDrop(Random random) => random != null && random.NextDouble() < DropChance;

        public static bool IsEligible(InventorySlotData slot, ItemDefinition item)
        {
            if (slot == null || slot.IsEmpty || item == null || item.IsBound)
            {
                return false;
            }

            if (item.Type == ItemType.Currency || item.Type == ItemType.QuestItem
                || item.Category == ItemCategory.Currency || item.Category == ItemCategory.QuestItem)
            {
                return false;
            }

            return !(item.ItemId ?? string.Empty).StartsWith(RelicIdPrefix, StringComparison.Ordinal);
        }

        /// <summary>
        /// Rolls the 9% and, on success, picks one eligible entry uniformly. Returns its index in <paramref name="slots"/>,
        /// or -1 (no drop: the roll failed or nothing is eligible). The roll is always made first, so the same seed gives
        /// the same "did it happen" answer whatever the bag holds.
        /// </summary>
        public static int PickDrop(IReadOnlyList<InventorySlotData> slots, Func<string, ItemDefinition> resolve, Random random)
        {
            if (!RollsDrop(random) || slots == null)
            {
                return -1;
            }

            var eligible = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot != null && !slot.IsEmpty && IsEligible(slot, resolve?.Invoke(slot.itemId)))
                {
                    eligible.Add(i);
                }
            }

            return eligible.Count == 0 ? -1 : eligible[random.Next(eligible.Count)];
        }

        /// <summary>A generated roll counts at its rolled rarity when higher than the base item's (as the tooltip shows it).</summary>
        public static ItemRarity RarityOf(InventorySlotData slot, ItemDefinition item)
        {
            ItemRarity baseRarity = item != null ? item.Rarity : ItemRarity.Common;
            return slot != null && slot.HasGeneratedData && slot.generatedRarity > baseRarity ? slot.generatedRarity : baseRarity;
        }

        public static int BuybackFee(ItemRarity rarity)
        {
            int index = (int)rarity;
            if (index < 0)
            {
                index = 0;
            }

            return BuybackFeeByRarity[Math.Min(index, BuybackFeeByRarity.Length - 1)];
        }
    }

    /// <summary>One item waiting at "Achados e Perdidos" (the whole dropped entry, affixes included, plus its fee).</summary>
    [Serializable]
    public class LostAndFoundEntry
    {
        public InventorySlotData item;
        public int fee;
    }

    /// <summary>Raised when the death drop took an item out of the backpack (<see cref="DeathDropService.Apply"/>).</summary>
    public readonly struct ItemDroppedOnDeathEvent : IGameEvent
    {
        public readonly InventorySlotData Item;
        public readonly int Fee;

        public ItemDroppedOnDeathEvent(InventorySlotData item, int fee)
        {
            Item = item;
            Fee = fee;
        }
    }

    /// <summary>Outcome of <see cref="LostAndFoundList.TryBuyBack"/>.</summary>
    public enum LostAndFoundBuyback
    {
        Ok,
        Invalid,
        NotEnoughGold,
        BagFull,
    }

    /// <summary>
    /// "Achados e Perdidos" at Vó Zefa's: items the death drop took, waiting to be bought back. Owned and saved by the
    /// village chest (<see cref="VillageChestStorage.LostAndFound"/>, same save entry, same new-game reset).
    /// </summary>
    public sealed class LostAndFoundList
    {
        /// <summary>The shop's currency (<c>ShopDefinition</c> default) pays the fee.</summary>
        public const string FeeCurrencyItemId = "item_gold";

        private readonly List<LostAndFoundEntry> _entries = new List<LostAndFoundEntry>();

        public IReadOnlyList<LostAndFoundEntry> Entries => _entries;

        public int Count => _entries.Count;

        public event Action Changed;

        public void Add(InventorySlotData item, int fee)
        {
            if (item == null || item.IsEmpty)
            {
                return;
            }

            _entries.Add(new LostAndFoundEntry { item = item.CloneWithQuantity(item.quantity), fee = Math.Max(0, fee) });
            Changed?.Invoke();
        }

        /// <summary>
        /// Pays the fee from <paramref name="bag"/> and puts the item back in it (stacks/affixes/durability intact). Nothing
        /// changes unless both the gold and the room are there.
        /// </summary>
        public LostAndFoundBuyback TryBuyBack(int index, InventoryManager bag, string currencyItemId = FeeCurrencyItemId)
        {
            if (bag == null || index < 0 || index >= _entries.Count)
            {
                return LostAndFoundBuyback.Invalid;
            }

            LostAndFoundEntry entry = _entries[index];
            if (entry.fee > 0 && bag.GetTotalCount(currencyItemId) < entry.fee)
            {
                return LostAndFoundBuyback.NotEnoughGold;
            }

            if (!bag.CanFitCopy(entry.item))
            {
                return LostAndFoundBuyback.BagFull;
            }

            if (entry.fee > 0 && !bag.RemoveById(currencyItemId, entry.fee))
            {
                return LostAndFoundBuyback.NotEnoughGold;
            }

            if (!bag.TryAddCopy(entry.item))
            {
                ItemDefinition currency = ItemDatabase.GetById(currencyItemId);
                if (currency != null && entry.fee > 0)
                {
                    bag.AddItem(currency, entry.fee);
                }

                return LostAndFoundBuyback.BagFull;
            }

            _entries.RemoveAt(index);
            Changed?.Invoke();
            return LostAndFoundBuyback.Ok;
        }

        public void Clear()
        {
            if (_entries.Count == 0)
            {
                return;
            }

            _entries.Clear();
            Changed?.Invoke();
        }

        public LostAndFoundEntry[] Capture()
        {
            var copy = new LostAndFoundEntry[_entries.Count];
            for (int i = 0; i < copy.Length; i++)
            {
                copy[i] = new LostAndFoundEntry { item = _entries[i].item.CloneWithQuantity(_entries[i].item.quantity), fee = _entries[i].fee };
            }

            return copy;
        }

        public void Restore(LostAndFoundEntry[] saved)
        {
            _entries.Clear();
            if (saved != null)
            {
                foreach (LostAndFoundEntry entry in saved)
                {
                    if (entry?.item != null && !entry.item.IsEmpty)
                    {
                        _entries.Add(new LostAndFoundEntry { item = entry.item.CloneWithQuantity(entry.item.quantity), fee = Math.Max(0, entry.fee) });
                    }
                }
            }

            Changed?.Invoke();
        }
    }

    /// <summary>What <see cref="DeathDropService.Apply"/> did.</summary>
    public readonly struct DeathDropResult
    {
        public readonly bool Dropped;
        public readonly InventorySlotData Item;
        public readonly int Fee;

        public DeathDropResult(InventorySlotData item, int fee)
        {
            Dropped = item != null;
            Item = item;
            Fee = fee;
        }

        public static DeathDropResult None => default;
    }

    /// <summary>Applies <see cref="DeathDropRules"/> to a live bag (offline: the item moves to "Achados e Perdidos").</summary>
    public static class DeathDropService
    {
        public static DeathDropResult Apply(InventoryManager bag, LostAndFoundList lostAndFound, Random random, Func<string, ItemDefinition> resolve = null)
        {
            if (bag == null || lostAndFound == null)
            {
                return DeathDropResult.None;
            }

            resolve = resolve ?? ItemDatabase.GetById;
            IReadOnlyList<InventorySlotData> slots = bag.Slots;
            int index = DeathDropRules.PickDrop(slots, resolve, random);
            if (index < 0)
            {
                return DeathDropResult.None;
            }

            InventorySlotData slot = slots[index];
            InventorySlotData dropped = slot.CloneWithQuantity(slot.quantity);
            int fee = DeathDropRules.BuybackFee(DeathDropRules.RarityOf(slot, resolve(slot.itemId)));

            if (!bag.RemoveFromSlot(index, slot.quantity))
            {
                return DeathDropResult.None;
            }

            lostAndFound.Add(dropped, fee);
            EventBus.Publish(new ItemDroppedOnDeathEvent(dropped, fee));
            return new DeathDropResult(dropped, fee);
        }
    }
}
