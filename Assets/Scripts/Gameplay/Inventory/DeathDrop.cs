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
    /// <item>Each unprotected player death rolls <see cref="DropChance"/> once (the Patuá da Vó Edna and the Bênção da
    /// Romaria turn it into 0%, <see cref="DeathPenaltyRules.AllowsItemDrop"/>).</item>
    /// <item>On success <b>exactly one unit</b> of one random eligible backpack entry falls: a stack of 6 potions loses 1
    /// and keeps 5 (<see cref="DropQuantity"/>). Equipped gear is never touched (it is not in the backpack grid).</item>
    /// <item>Not eligible (<see cref="IsEligible"/>): gold/currency, quest and story items, relics, items flagged
    /// <see cref="ItemDefinition.IsBound"/>, unknown ids.</item>
    /// <item><b>A lost item can not be recovered.</b> Offline it is gone for good (there is no buy-back). Online (FASE 12)
    /// the server drops that same unit on the ground where the hero fell, for anyone to pick up (MU Online style); the
    /// rule that picks it is this one, so the server can run it as is.</item>
    /// </list>
    /// </summary>
    public static class DeathDropRules
    {
        /// <summary>Chance per death that one item falls out of the backpack.</summary>
        public const float DropChance = 0.09f;

        /// <summary>Units that fall from the chosen entry: always one, never the whole stack.</summary>
        public const int DropQuantity = 1;

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

        /// <summary>How many units fall from <paramref name="slot"/>: <see cref="DropQuantity"/>, or 0 for an empty slot.</summary>
        public static int QuantityToDrop(InventorySlotData slot) =>
            slot == null || slot.IsEmpty ? 0 : Math.Min(DropQuantity, slot.quantity);

        /// <summary>A generated roll counts at its rolled rarity when higher than the base item's (as the tooltip shows it).</summary>
        public static ItemRarity RarityOf(InventorySlotData slot, ItemDefinition item)
        {
            ItemRarity baseRarity = item != null ? item.Rarity : ItemRarity.Common;
            return slot != null && slot.HasGeneratedData && slot.generatedRarity > baseRarity ? slot.generatedRarity : baseRarity;
        }
    }

    /// <summary>
    /// Raised when the death drop took an item out of the backpack (<see cref="DeathDropService.Apply"/>). Offline nothing
    /// keeps it; online the server turns this into a ground drop at the death spot.
    /// </summary>
    public readonly struct ItemDroppedOnDeathEvent : IGameEvent
    {
        /// <summary>The unit that fell (quantity 1, affixes/durability as they were).</summary>
        public readonly InventorySlotData Item;

        public ItemDroppedOnDeathEvent(InventorySlotData item)
        {
            Item = item;
        }
    }

    /// <summary>What <see cref="DeathDropService.Apply"/> did.</summary>
    public readonly struct DeathDropResult
    {
        public readonly bool Dropped;

        /// <summary>The unit that fell (quantity <see cref="DeathDropRules.DropQuantity"/>), or null.</summary>
        public readonly InventorySlotData Item;

        public DeathDropResult(InventorySlotData item)
        {
            Dropped = item != null;
            Item = item;
        }

        public static DeathDropResult None => default;
    }

    /// <summary>Applies <see cref="DeathDropRules"/> to a live bag: one unit leaves it for good.</summary>
    public static class DeathDropService
    {
        public static DeathDropResult Apply(InventoryManager bag, Random random, Func<string, ItemDefinition> resolve = null)
        {
            if (bag == null)
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
            int quantity = DeathDropRules.QuantityToDrop(slot);
            InventorySlotData dropped = slot.CloneWithQuantity(quantity);

            if (quantity <= 0 || !bag.RemoveFromSlot(index, quantity))
            {
                return DeathDropResult.None;
            }

            EventBus.Publish(new ItemDroppedOnDeathEvent(dropped));
            return new DeathDropResult(dropped);
        }
    }
}
