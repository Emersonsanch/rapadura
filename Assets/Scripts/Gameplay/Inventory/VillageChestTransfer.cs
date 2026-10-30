using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>Outcome of a bulk chest move ("Guardar materiais" / "Pegar o que as missões pedem").</summary>
    public readonly struct VillageChestBulkResult
    {
        /// <summary>Units that changed side.</summary>
        public readonly int Moved;

        /// <summary>Units that should have moved but did not fit (chest out of slots, no room on the bag's grid).</summary>
        public readonly int LeftBehind;

        public VillageChestBulkResult(int moved, int leftBehind)
        {
            Moved = moved;
            LeftBehind = leftBehind;
        }
    }

    /// <summary>
    /// Moves between the bag (<see cref="InventoryManager"/>, on the map the live <see cref="CarriedInventorySession"/>
    /// one) and the <see cref="VillageChestStorage"/>. Static and pure so EditMode tests drive it on a bare bag:
    /// <list type="bullet">
    /// <item>Into the chest: only its slots limit (no weight cap).</item>
    /// <item>Into the bag: the bag's own grid rules (the shared fit API) —
    /// <see cref="InventoryManager.AddItem(ItemDefinition,int)"/> for plain stacks (stacks first, then free areas),
    /// <see cref="InventoryManager.TryAddCopy"/> for a generated roll or a piece of gear, whose affixes, name, rarity
    /// and durability come back intact.</item>
    /// <item>Bulk moves take the quests' needs from <c>QuestManager.GetOutstandingCollectAmount</c>
    /// ((itemId, held) → units still needed), passed in as a delegate.</item>
    /// </list>
    /// </summary>
    public static class VillageChestTransfer
    {
        /// <summary>Stores up to <paramref name="amount"/> units of bag slot <paramref name="bagSlot"/>. Returns how many moved.</summary>
        public static int StoreFromBag(InventoryManager bag, int bagSlot, int amount, VillageChestStorage chest)
        {
            if (bag == null || chest == null || bagSlot < 0 || bagSlot >= bag.Slots.Count)
            {
                return 0;
            }

            InventorySlotData slot = bag.Slots[bagSlot];
            if (slot.IsEmpty || amount <= 0)
            {
                return 0;
            }

            amount = Mathf.Min(amount, slot.quantity);
            int stored = chest.Store(slot.CloneWithQuantity(amount), amount);
            if (stored > 0)
            {
                bag.RemoveFromSlot(bagSlot, stored);
            }

            return stored;
        }

        /// <summary>Takes up to <paramref name="amount"/> units of chest slot <paramref name="chestSlot"/> into the bag, within its grid. Returns how many moved.</summary>
        public static int TakeToBag(VillageChestStorage chest, int chestSlot, int amount, InventoryManager bag)
        {
            if (bag == null || chest == null || chestSlot < 0 || chestSlot >= chest.Slots.Count)
            {
                return 0;
            }

            InventorySlotData slot = chest.Slots[chestSlot];
            ItemDefinition item = slot.IsEmpty ? null : ItemDatabase.GetById(slot.itemId);
            if (item == null || amount <= 0)
            {
                return 0;
            }

            amount = Mathf.Min(amount, slot.quantity);
            int moved;

            if (slot.HasGeneratedData || !item.IsStackable)
            {
                // A generated roll or a piece of gear: one entry, copied as it is (durability, affixes).
                int pieceAmount = slot.HasGeneratedData ? amount : 1;
                moved = bag.TryAddCopy(slot.CloneWithQuantity(pieceAmount)) ? pieceAmount : 0;
            }
            else
            {
                moved = amount - bag.AddItem(item, amount);
            }

            if (moved > 0)
            {
                chest.RemoveFromSlot(chestSlot, moved);
            }

            return moved;
        }

        /// <summary>
        /// How many units of a Material the bag keeps on "Guardar materiais": what the active quests ask for
        /// (<paramref name="outstandingForQuests"/> with nothing held = the largest open CollectItem amount), capped by what is held.
        /// </summary>
        public static int UnitsToKeep(string itemId, int held, Func<string, int, int> outstandingForQuests)
        {
            int needed = outstandingForQuests != null ? outstandingForQuests(itemId, 0) : 0;
            return Mathf.Clamp(needed, 0, Mathf.Max(0, held));
        }

        /// <summary>
        /// "Guardar materiais": every <see cref="ItemType.Material"/> in the bag goes to the chest, except the units an
        /// active quest still needs (<see cref="UnitsToKeep"/>). Units are taken from the last slots first, so the
        /// kept ones stay where they were.
        /// </summary>
        public static VillageChestBulkResult StoreMaterials(InventoryManager bag, VillageChestStorage chest, Func<string, int, int> outstandingForQuests)
        {
            if (bag == null || chest == null)
            {
                return default;
            }

            int moved = 0;
            int leftBehind = 0;

            foreach (string itemId in DistinctIds(bag.Slots, ItemType.Material))
            {
                int held = bag.GetTotalCount(itemId);
                int toStore = held - UnitsToKeep(itemId, held, outstandingForQuests);

                for (int i = bag.Slots.Count - 1; i >= 0 && toStore > 0; i--)
                {
                    InventorySlotData slot = bag.Slots[i];
                    if (slot.IsEmpty || slot.itemId != itemId)
                    {
                        continue;
                    }

                    int asked = Mathf.Min(slot.quantity, toStore);
                    int stored = StoreFromBag(bag, i, asked, chest);
                    moved += stored;
                    toStore -= stored;
                    if (stored < asked)
                    {
                        break;
                    }
                }

                leftBehind += Mathf.Max(0, toStore);
            }

            return new VillageChestBulkResult(moved, leftBehind);
        }

        /// <summary>
        /// "Pegar o que as missões pedem": for every item in the chest, takes what the active collect objectives still
        /// need beyond what the bag holds (<paramref name="outstandingForQuests"/>), plain stacks first, within the bag's
        /// grid. What does not fit stays in the chest (<see cref="VillageChestBulkResult.LeftBehind"/>).
        /// </summary>
        public static VillageChestBulkResult TakeForQuests(VillageChestStorage chest, InventoryManager bag, Func<string, int, int> outstandingForQuests)
        {
            if (bag == null || chest == null || outstandingForQuests == null)
            {
                return default;
            }

            int moved = 0;
            int leftBehind = 0;

            foreach (string itemId in DistinctIds(chest.Slots, null))
            {
                int need = Mathf.Min(outstandingForQuests(itemId, bag.GetTotalCount(itemId)), chest.GetTotalCount(itemId));
                if (need <= 0)
                {
                    continue;
                }

                foreach (bool generatedPass in new[] { false, true })
                {
                    for (int i = 0; i < chest.Slots.Count && need > 0; i++)
                    {
                        InventorySlotData slot = chest.Slots[i];
                        if (slot.IsEmpty || slot.itemId != itemId || slot.HasGeneratedData != generatedPass)
                        {
                            continue;
                        }

                        int asked = Mathf.Min(slot.quantity, need);
                        int taken = TakeToBag(chest, i, asked, bag);
                        moved += taken;
                        need -= taken;
                    }
                }

                leftBehind += Mathf.Max(0, need);
            }

            return new VillageChestBulkResult(moved, leftBehind);
        }

        // ------------------------------------------------------------------

        private static List<string> DistinctIds(IReadOnlyList<InventorySlotData> slots, ItemType? type)
        {
            var ids = new List<string>();
            foreach (InventorySlotData slot in slots)
            {
                if (slot.IsEmpty || ids.Contains(slot.itemId))
                {
                    continue;
                }

                if (type.HasValue)
                {
                    ItemDefinition item = ItemDatabase.GetById(slot.itemId);
                    if (item == null || item.Type != type.Value)
                    {
                        continue;
                    }
                }

                ids.Add(slot.itemId);
            }

            return ids;
        }
    }
}
