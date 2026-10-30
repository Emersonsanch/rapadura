using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Refining
{
    /// <summary>Which piece is refined: a bag entry (by index) or an equipped slot.</summary>
    public readonly struct RefineTarget
    {
        public readonly bool IsEquipped;
        public readonly int SlotIndex;
        public readonly EquipmentSlot EquipSlot;

        private RefineTarget(bool isEquipped, int slotIndex, EquipmentSlot equipSlot)
        {
            IsEquipped = isEquipped;
            SlotIndex = slotIndex;
            EquipSlot = equipSlot;
        }

        public static RefineTarget Bag(int slotIndex) => new RefineTarget(false, slotIndex, EquipmentSlot.None);
        public static RefineTarget Equipped(EquipmentSlot slot) => new RefineTarget(true, -1, slot);
    }

    /// <summary>What the refine window shows for one piece before the player confirms (pure snapshot).</summary>
    public readonly struct RefinePreview
    {
        public readonly ItemDefinition Item;
        public readonly InventorySlotData Data;
        public readonly int Level;
        public readonly int Option;
        public readonly RefineStone Stone;
        public readonly int StonesOwned;
        public readonly int GoldCost;
        public readonly int GoldOwned;
        public readonly int CharmsOwned;
        public readonly int LifeStonesOwned;
        public readonly float Chance;
        public readonly bool CanFail;
        public readonly int FailureLevel;
        public readonly RefineCheck Check;
        public readonly RefineCheck OptionCheck;

        public RefinePreview(ItemDefinition item, InventorySlotData data, int level, int option, RefineStone stone, int stonesOwned,
            int goldCost, int goldOwned, int charmsOwned, int lifeStonesOwned, float chance, bool canFail, int failureLevel,
            RefineCheck check, RefineCheck optionCheck)
        {
            Item = item;
            Data = data;
            Level = level;
            Option = option;
            Stone = stone;
            StonesOwned = stonesOwned;
            GoldCost = goldCost;
            GoldOwned = goldOwned;
            CharmsOwned = charmsOwned;
            LifeStonesOwned = lifeStonesOwned;
            Chance = chance;
            CanFail = canFail;
            FailureLevel = failureLevel;
            Check = check;
            OptionCheck = optionCheck;
        }

        public bool CanRefine => Check == RefineCheck.Ok;
        public bool CanAddOption => OptionCheck == RefineCheck.Ok;
    }

    /// <summary>Raised after a refine roll was applied to an item (success or failure).</summary>
    public readonly struct ItemRefinedEvent : IGameEvent
    {
        public readonly string ItemId;
        public readonly RefineAttempt Attempt;

        public ItemRefinedEvent(string itemId, RefineAttempt attempt)
        {
            ItemId = itemId;
            Attempt = attempt;
        }
    }

    /// <summary>
    /// Applies <see cref="RefineRules"/> to a live <see cref="InventoryManager"/>: reads the stones/gold/charm in the bag,
    /// spends them (<see cref="InventoryManager.RemoveById"/>), rolls with the given seed and writes the result on the
    /// item instance (<see cref="InventorySlotData.refineLevel"/> / <see cref="InventorySlotData.refineOptionLevel"/>),
    /// which travels with it everywhere (bag, equipment, chest, death drop, save). Nothing is produced, so no fit check
    /// is needed (the grid API is untouched). Online (Fase 12) this whole call runs on the server with its seed.
    /// </summary>
    public static class RefineService
    {
        /// <summary>The live data of <paramref name="target"/>, or null when there is nothing there.</summary>
        public static InventorySlotData Resolve(InventoryManager bag, RefineTarget target)
        {
            if (bag == null)
            {
                return null;
            }

            if (target.IsEquipped)
            {
                return bag.Equipped.TryGetValue(target.EquipSlot, out InventorySlotData equipped) && equipped != null && !equipped.IsEmpty ? equipped : null;
            }

            IReadOnlyList<InventorySlotData> slots = bag.Slots;
            if (target.SlotIndex < 0 || target.SlotIndex >= slots.Count)
            {
                return null;
            }

            InventorySlotData slot = slots[target.SlotIndex];
            return slot != null && !slot.IsEmpty ? slot : null;
        }

        /// <summary>Every refinable piece the hero has: equipped first, then the bag in entry order.</summary>
        public static List<RefineTarget> ListTargets(InventoryManager bag)
        {
            var targets = new List<RefineTarget>();
            if (bag == null)
            {
                return targets;
            }

            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                if (slot == EquipmentSlot.None)
                {
                    continue;
                }

                InventorySlotData data = Resolve(bag, RefineTarget.Equipped(slot));
                if (data != null && RefineRules.IsRefinable(ItemDatabase.GetById(data.itemId)))
                {
                    targets.Add(RefineTarget.Equipped(slot));
                }
            }

            IReadOnlyList<InventorySlotData> slots = bag.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData data = slots[i];
                if (data != null && !data.IsEmpty && RefineRules.IsRefinable(ItemDatabase.GetById(data.itemId)))
                {
                    targets.Add(RefineTarget.Bag(i));
                }
            }

            return targets;
        }

        public static RefinePreview Preview(InventoryManager bag, RefineTarget target, bool useCharm)
        {
            InventorySlotData data = Resolve(bag, target);
            ItemDefinition item = data != null ? ItemDatabase.GetById(data.itemId) : null;
            bool refinable = RefineRules.IsRefinable(item);
            int level = data != null ? RefineRules.ClampLevel(data.refineLevel) : 0;
            int option = data != null ? RefineRules.ClampOption(data.refineOptionLevel) : 0;
            RefineStone stone = RefineRules.StoneFor(level);
            string stoneId = RefineItems.IdOf(stone);
            int stones = bag != null && stoneId != null ? bag.GetTotalCount(stoneId) : 0;
            int gold = bag != null ? bag.GetTotalCount(RefineItems.Gold) : 0;
            int charms = bag != null ? bag.GetTotalCount(RefineItems.LuckCharm) : 0;
            int life = bag != null ? bag.GetTotalCount(RefineItems.LifeStone) : 0;
            bool canFail = RefineRules.CanFail(level);
            bool charm = useCharm && canFail;

            return new RefinePreview(item, data, level, option, stone, stones, RefineRules.GoldCostFor(level), gold, charms, life,
                RefineRules.ChanceFor(level, charm), canFail, RefineRules.FailureLevel(level),
                RefineRules.Check(refinable, level, stones, gold, charm, charms),
                RefineRules.CheckOption(refinable, option, life));
        }

        /// <summary>
        /// Spends the stone (+ gold, + charm when <paramref name="useCharm"/> on a roll that can fail), rolls with
        /// <paramref name="seed"/> and writes the new level. False (nothing spent) when <see cref="RefinePreview.Check"/> fails.
        /// </summary>
        public static bool TryRefine(InventoryManager bag, RefineTarget target, bool useCharm, int seed, out RefineAttempt attempt, out RefineCheck check)
        {
            attempt = default;
            RefinePreview preview = Preview(bag, target, useCharm);
            check = preview.Check;
            if (!preview.CanRefine)
            {
                return false;
            }

            bool charm = useCharm && preview.CanFail;
            InventorySlotData data = preview.Data;
            if (!bag.RemoveById(RefineItems.IdOf(preview.Stone), 1))
            {
                check = RefineCheck.MissingStone;
                return false;
            }

            if (preview.GoldCost > 0)
            {
                bag.RemoveById(RefineItems.Gold, preview.GoldCost);
            }

            if (charm)
            {
                bag.RemoveById(RefineItems.LuckCharm, 1);
            }

            attempt = RefineRules.Attempt(preview.Level, charm, seed);
            data.refineLevel = attempt.ToLevel;
            bag.NotifyItemDataChanged(target.IsEquipped ? target.EquipSlot : EquipmentSlot.None);
            EventBus.Publish(new ItemRefinedEvent(data.itemId, attempt));
            return true;
        }

        /// <summary>One Pedra da Vida on the target's option line. <paramref name="success"/> tells whether it went up.</summary>
        public static bool TryAddOption(InventoryManager bag, RefineTarget target, int seed, out bool success, out RefineCheck check)
        {
            success = false;
            RefinePreview preview = Preview(bag, target, false);
            check = preview.OptionCheck;
            if (!preview.CanAddOption)
            {
                return false;
            }

            InventorySlotData data = preview.Data;
            if (!bag.RemoveById(RefineItems.LifeStone, 1))
            {
                check = RefineCheck.MissingStone;
                return false;
            }

            int option = RefineRules.ResolveOption(preview.Option, RefineRules.Roll(seed));
            success = option > preview.Option;
            data.refineOptionLevel = option;
            bag.NotifyItemDataChanged(target.IsEquipped ? target.EquipSlot : EquipmentSlot.None);
            return true;
        }
    }
}
