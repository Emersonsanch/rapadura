using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Refining;

namespace Rapadura.Gameplay.Items.Durability
{
    /// <summary>Why a repair can or cannot be done right now.</summary>
    public enum RepairCheck
    {
        Ok = 0,
        NotRepairable = 1,
        NothingToRepair = 2,
        NotEnoughGold = 3,
    }

    /// <summary>What the ferreiro shows for one piece before repairing (pure snapshot).</summary>
    public readonly struct RepairQuote
    {
        public readonly ItemDefinition Item;
        public readonly InventorySlotData Data;
        public readonly float Current;
        public readonly float Max;
        public readonly int Cost;
        public readonly int GoldOwned;
        public readonly RepairCheck Check;

        public RepairQuote(ItemDefinition item, InventorySlotData data, float current, float max, int cost, int goldOwned, RepairCheck check)
        {
            Item = item;
            Data = data;
            Current = current;
            Max = max;
            Cost = cost;
            GoldOwned = goldOwned;
            Check = check;
        }

        public bool CanRepair => Check == RepairCheck.Ok;
        public bool IsBroken => Max > 0f && Current <= 0f;
    }

    /// <summary>Raised when an equipped piece's durability reaches 0 (it stays equipped and gives no stats).</summary>
    public readonly struct EquipmentBrokenEvent : IGameEvent
    {
        public readonly string ItemId;
        public readonly EquipmentSlot Slot;

        public EquipmentBrokenEvent(string itemId, EquipmentSlot slot)
        {
            ItemId = itemId;
            Slot = slot;
        }
    }

    /// <summary>Raised after the ferreiro repaired one or more pieces.</summary>
    public readonly struct EquipmentRepairedEvent : IGameEvent
    {
        public readonly int Pieces;
        public readonly int Gold;

        public EquipmentRepairedEvent(int pieces, int gold)
        {
            Pieces = pieces;
            Gold = gold;
        }
    }

    /// <summary>
    /// Applies <see cref="DurabilityRules"/> to a live <see cref="InventoryManager"/>: wear (weapon per attack, armour per
    /// hit, everything per death) and the ferreiro's repair paid in gold (<see cref="RefineItems.Gold"/>). Targets reuse
    /// <see cref="RefineTarget"/> (a bag entry or an equipped slot). Any shop/NPC (e.g. a city ferreiro) can call
    /// <see cref="Quote"/> / <see cref="TryRepair"/> / <see cref="TryRepairAll"/>; online (Fase 12) the server runs them.
    /// A piece that breaks stays where it is (never destroyed): its equip effects are re-applied, which for a broken piece
    /// means none, and a repair re-applies them again.
    /// </summary>
    public static class RepairService
    {
        /// <summary>Every piece with durability below its maximum: equipped first, then the bag in entry order.</summary>
        public static List<RefineTarget> ListWorn(InventoryManager bag)
        {
            var targets = new List<RefineTarget>();
            if (bag == null)
            {
                return targets;
            }

            foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in SortedEquipped(bag))
            {
                if (NeedsRepair(ItemDatabase.GetById(pair.Value.itemId), pair.Value))
                {
                    targets.Add(RefineTarget.Equipped(pair.Key));
                }
            }

            IReadOnlyList<InventorySlotData> slots = bag.Slots;
            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData data = slots[i];
                if (data != null && !data.IsEmpty && NeedsRepair(ItemDatabase.GetById(data.itemId), data))
                {
                    targets.Add(RefineTarget.Bag(i));
                }
            }

            return targets;
        }

        public static bool NeedsRepair(ItemDefinition item, InventorySlotData data) =>
            DurabilityRules.IsTracked(item) && data != null && data.currentDurability < item.MaxDurability;

        public static RepairQuote Quote(InventoryManager bag, RefineTarget target)
        {
            InventorySlotData data = RefineService.Resolve(bag, target);
            ItemDefinition item = data != null ? ItemDatabase.GetById(data.itemId) : null;
            int gold = bag != null ? bag.GetTotalCount(RefineItems.Gold) : 0;
            if (!DurabilityRules.IsTracked(item))
            {
                return new RepairQuote(item, data, 0f, 0f, 0, gold, RepairCheck.NotRepairable);
            }

            int cost = DurabilityRules.RepairCost(item, data);
            RepairCheck check = cost <= 0 ? RepairCheck.NothingToRepair : gold < cost ? RepairCheck.NotEnoughGold : RepairCheck.Ok;
            return new RepairQuote(item, data, data.currentDurability, item.MaxDurability, cost, gold, check);
        }

        /// <summary>Pays and repairs one piece to full. False (nothing spent) when the quote's check fails.</summary>
        public static bool TryRepair(InventoryManager bag, RefineTarget target, out int cost, out RepairCheck check)
        {
            RepairQuote quote = Quote(bag, target);
            cost = quote.Cost;
            check = quote.Check;
            if (!quote.CanRepair || !bag.RemoveById(RefineItems.Gold, quote.Cost))
            {
                if (quote.CanRepair)
                {
                    check = RepairCheck.NotEnoughGold;
                }

                return false;
            }

            quote.Data.currentDurability = quote.Max;
            bag.NotifyItemDataChanged(target.IsEquipped ? target.EquipSlot : EquipmentSlot.None);
            EventBus.Publish(new EquipmentRepairedEvent(1, quote.Cost));
            return true;
        }

        /// <summary>Gold to repair everything in <see cref="ListWorn"/>.</summary>
        public static int RepairAllCost(InventoryManager bag, out int pieces)
        {
            int total = 0;
            List<RefineTarget> worn = ListWorn(bag);
            pieces = worn.Count;
            foreach (RefineTarget target in worn)
            {
                total += Quote(bag, target).Cost;
            }

            return total;
        }

        /// <summary>Repairs every worn piece, all or nothing (not enough gold for all = nothing repaired).</summary>
        public static bool TryRepairAll(InventoryManager bag, out int cost, out int pieces, out RepairCheck check)
        {
            cost = RepairAllCost(bag, out pieces);
            if (pieces == 0)
            {
                check = RepairCheck.NothingToRepair;
                return false;
            }

            if (bag.GetTotalCount(RefineItems.Gold) < cost || !bag.RemoveById(RefineItems.Gold, cost))
            {
                check = RepairCheck.NotEnoughGold;
                return false;
            }

            foreach (RefineTarget target in ListWorn(bag))
            {
                InventorySlotData data = RefineService.Resolve(bag, target);
                ItemDefinition item = data != null ? ItemDatabase.GetById(data.itemId) : null;
                if (item == null)
                {
                    continue;
                }

                data.currentDurability = item.MaxDurability;
                if (target.IsEquipped)
                {
                    bag.NotifyItemDataChanged(target.EquipSlot);
                }
            }

            bag.NotifyItemDataChanged();
            check = RepairCheck.Ok;
            EventBus.Publish(new EquipmentRepairedEvent(pieces, cost));
            return true;
        }

        // ------------------------------------------------------------------ wear

        /// <summary>The equipped weapon (MainHand / OffHand with weapon damage) loses <paramref name="amount"/>.</summary>
        public static int WearWeapon(InventoryManager bag, float amount = DurabilityRules.WeaponWearPerAttack) =>
            WearEquipped(bag, item => item.WeaponDamage > 0f || item.Type == ItemType.Weapon, _ => amount);

        /// <summary>Every equipped armour piece loses <paramref name="amount"/> (one hit taken).</summary>
        public static int WearArmour(InventoryManager bag, float amount = DurabilityRules.ArmourWearPerHit) =>
            WearEquipped(bag, item => item.ArmorDefense > 0f, _ => amount);

        /// <summary>Every equipped tracked piece loses <see cref="DurabilityRules.DeathWearFraction"/> of its maximum.</summary>
        public static int WearOnDeath(InventoryManager bag) =>
            WearEquipped(bag, _ => true, item => DurabilityRules.DeathWear(item.MaxDurability));

        /// <summary>
        /// Wears every equipped tracked piece accepted by <paramref name="filter"/>; a piece reaching 0 gets its effects
        /// re-applied (none while broken) and raises <see cref="EquipmentBrokenEvent"/>. Returns how many just broke.
        /// </summary>
        public static int WearEquipped(InventoryManager bag, System.Func<ItemDefinition, bool> filter, System.Func<ItemDefinition, float> amount)
        {
            if (bag == null)
            {
                return 0;
            }

            var broke = new List<KeyValuePair<EquipmentSlot, InventorySlotData>>();
            foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in SortedEquipped(bag))
            {
                ItemDefinition item = ItemDatabase.GetById(pair.Value.itemId);
                if (!DurabilityRules.IsTracked(item) || !filter(item) || pair.Value.currentDurability <= 0f)
                {
                    continue;
                }

                pair.Value.currentDurability = DurabilityRules.Wear(pair.Value.currentDurability, amount(item));
                if (pair.Value.currentDurability <= 0f)
                {
                    broke.Add(pair);
                }
            }

            foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in broke)
            {
                bag.NotifyItemDataChanged(pair.Key);
                EventBus.Publish(new EquipmentBrokenEvent(pair.Value.itemId, pair.Key));
            }

            return broke.Count;
        }

        private static List<KeyValuePair<EquipmentSlot, InventorySlotData>> SortedEquipped(InventoryManager bag)
        {
            var list = new List<KeyValuePair<EquipmentSlot, InventorySlotData>>();
            foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in bag.Equipped)
            {
                if (pair.Value != null && !pair.Value.IsEmpty)
                {
                    list.Add(pair);
                }
            }

            list.Sort((a, b) => ((int)a.Key).CompareTo((int)b.Key));
            return list;
        }
    }
}
