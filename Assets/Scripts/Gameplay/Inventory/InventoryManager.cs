using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>Serializable snapshot of the whole inventory + equipment for the save system.</summary>
    [Serializable]
    public class InventorySaveState
    {
        public InventorySlotData[] slots = Array.Empty<InventorySlotData>();
        public EquippedEntry[] equipped = Array.Empty<EquippedEntry>();

        [Serializable]
        public class EquippedEntry
        {
            public EquipmentSlot slot;
            public string itemId;
            public float durability;

            // Procedural data of the equipped item (mirrors InventorySlotData's generated* fields).
            // Before these existed, equipping a generated item and saving/loading turned it back
            // into a plain base item (affixes, generated name and rolled rarity were all dropped).
            // Old saves simply lack these keys: JsonUtility leaves them null/empty/Common, which
            // InventorySlotData already treats as "plain item" - so old saves load unchanged.
            public string[] generatedAffixIds;
            public float[] generatedAffixValues;
            public string generatedName;
            public ItemRarity generatedRarity;
        }
    }

    /// <summary>
    /// Owns the player's carried items and equipped gear. Handles slot-based storage with
    /// weight limits and stacking, category/type filters for UI, moving/splitting stacks,
    /// equipping/unequipping (applying equip stat bonuses through BuffController) and
    /// consuming consumable items (healing/mana/stamina/buffs through PlayerStats). Persists
    /// itself to JSON through ISaveable, storing only item ids so it can be restored via
    /// <see cref="ItemDatabase"/> regardless of asset GUID churn.
    /// </summary>
    public class InventoryManager : MonoBehaviour, ISaveable
    {
        [Header("Capacity")]
        [SerializeField] private int _slotCount = 30;
        [SerializeField] private float _maxWeight = 100f;

        [Header("Optional References")]
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private BuffController _buffController;

        private InventorySlotData[] _slots;
        private readonly Dictionary<EquipmentSlot, InventorySlotData> _equipped = new Dictionary<EquipmentSlot, InventorySlotData>();

        public IReadOnlyList<InventorySlotData> Slots
        {
            get
            {
                EnsureSlotsInitialized();
                return _slots;
            }
        }
        public IReadOnlyDictionary<EquipmentSlot, InventorySlotData> Equipped => _equipped;
        public float MaxWeight => _maxWeight;

        public string SaveKey => "Inventory";

        private void Awake()
        {
            EnsureSlotsInitialized();

            if (_playerStats == null)
            {
                _playerStats = GetComponent<PlayerStats>();
            }

            if (_buffController == null)
            {
                _buffController = GetComponent<BuffController>();
            }
        }

        /// <summary>
        /// Guards every public entry point below against being called before Unity has run this
        /// component's <see cref="Awake"/> — which, unlike Play Mode, is NOT synchronous right
        /// after <c>AddComponent</c> in Edit Mode (confirmed: <c>_slots</c> was still null
        /// immediately after <c>AddComponent&lt;InventoryManager&gt;()</c> outside Play Mode),
        /// so EditMode tests calling <see cref="AddItem(ItemDefinition,int)"/> right after adding
        /// the component hit a NullReferenceException here. Idempotent and cheap to call from
        /// every public method.
        /// </summary>
        private void EnsureSlotsInitialized()
        {
            if (_slots != null)
            {
                return;
            }

            _slots = new InventorySlotData[_slotCount];

            for (int i = 0; i < _slotCount; i++)
            {
                _slots[i] = InventorySlotData.Empty();
            }
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.Unregister(this);
            }
        }

        // ---------------------------------------------------------------
        // Weight
        // ---------------------------------------------------------------

        public float GetTotalWeight()
        {
            EnsureSlotsInitialized();
            float total = 0f;

            foreach (InventorySlotData slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    continue;
                }

                ItemDefinition item = ItemDatabase.GetById(slot.itemId);

                if (item != null)
                {
                    total += item.Weight * slot.quantity;
                }
            }

            return total;
        }

        public bool HasWeightCapacityFor(ItemDefinition item, int quantity)
        {
            return GetTotalWeight() + item.Weight * quantity <= _maxWeight;
        }

        // ---------------------------------------------------------------
        // Add / Remove
        // ---------------------------------------------------------------

        /// <summary>Attempts to add a quantity of an item, filling existing stacks first, then empty slots. Returns leftover that could not fit.</summary>
        public int AddItem(ItemDefinition item, int quantity)
        {
            return AddItem(item, quantity, null);
        }

        /// <summary>
        /// Same as <see cref="AddItem(ItemDefinition,int)"/>, but when the item lands in a fresh
        /// empty slot, <paramref name="preserveDurability"/> (if given) is used instead of resetting
        /// to <c>MaxDurability</c>. Used when returning an item that already existed at some
        /// durability (e.g. unequipping) so it doesn't come back fully repaired. A brand-new item
        /// (preserveDurability == null) still starts at MaxDurability as before.
        /// </summary>
        public int AddItem(ItemDefinition item, int quantity, float? preserveDurability)
        {
            EnsureSlotsInitialized();

            if (item == null || quantity <= 0)
            {
                return quantity;
            }

            int remaining = quantity;

            if (item.IsStackable)
            {
                foreach (InventorySlotData slot in _slots)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    // Never stack into a slot holding a generated item: the plain items would
                    // inherit (or, when later split/removed, be confused with) that roll's affixes.
                    if (slot.itemId != item.ItemId || slot.quantity >= item.MaxStack || slot.HasGeneratedData)
                    {
                        continue;
                    }

                    int spaceInStack = item.MaxStack - slot.quantity;
                    int amountToAdd = Mathf.Min(spaceInStack, remaining, GetWeightAffordableAmount(item, remaining));
                    slot.quantity += amountToAdd;
                    remaining -= amountToAdd;
                }
            }

            foreach (InventorySlotData slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (!slot.IsEmpty)
                {
                    continue;
                }

                int amountToAdd = Mathf.Min(item.MaxStack, remaining, GetWeightAffordableAmount(item, remaining));

                if (amountToAdd <= 0)
                {
                    break;
                }

                slot.itemId = item.ItemId;
                slot.quantity = amountToAdd;
                slot.currentDurability = item.HasDurability
                    ? (preserveDurability ?? item.MaxDurability)
                    : 0f;
                remaining -= amountToAdd;
            }

            if (remaining != quantity)
            {
                EventBus.Publish(new InventoryChangedEvent());
            }

            return remaining;
        }

        /// <summary>
        /// Adds a single procedurally-generated item (with rolled affixes) into the first empty
        /// slot. There was previously no way at all to get a <see cref="GeneratedItemInstance"/>
        /// into an inventory — <see cref="AddItem(ItemDefinition,int)"/> only ever accepted a plain
        /// base <see cref="ItemDefinition"/> — so loot/quest-reward code had nowhere to hand off a
        /// Rare/Epic/Legendary roll's affixes even though <see cref="InventorySlotData"/> already had
        /// fields to store them (<see cref="InventorySlotData.generatedAffixIds"/>/
        /// <see cref="InventorySlotData.generatedAffixValues"/>). Generated items are never stacked
        /// with anything (each roll is unique), unlike <see cref="AddItem(ItemDefinition,int)"/>.
        /// Returns false if no empty slot was available.
        /// </summary>
        public bool AddGeneratedItem(GeneratedItemInstance instance)
        {
            EnsureSlotsInitialized();

            if (instance?.BaseItem == null)
            {
                return false;
            }

            foreach (InventorySlotData slot in _slots)
            {
                if (!slot.IsEmpty)
                {
                    continue;
                }

                slot.itemId = instance.BaseItem.ItemId;
                slot.quantity = 1;
                slot.currentDurability = instance.BaseItem.HasDurability ? instance.BaseItem.MaxDurability : 0f;
                slot.generatedAffixIds = instance.AffixIds;
                slot.generatedAffixValues = instance.AffixValues;
                slot.generatedName = instance.GeneratedName;
                slot.generatedRarity = instance.Rarity;

                EventBus.Publish(new InventoryChangedEvent());
                return true;
            }

            return false;
        }

        private int GetWeightAffordableAmount(ItemDefinition item, int desiredAmount)
        {
            if (item.Weight <= 0f)
            {
                return desiredAmount;
            }

            float remainingCapacity = _maxWeight - GetTotalWeight();
            int affordable = Mathf.FloorToInt(remainingCapacity / item.Weight);
            return Mathf.Clamp(affordable, 0, desiredAmount);
        }

        public bool RemoveFromSlot(int slotIndex, int quantity)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].IsEmpty || _slots[slotIndex].quantity < quantity)
            {
                return false;
            }

            _slots[slotIndex].quantity -= quantity;

            if (_slots[slotIndex].quantity <= 0)
            {
                _slots[slotIndex].Clear();
            }

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        public bool RemoveById(string itemId, int quantity)
        {
            EnsureSlotsInitialized();
            int remaining = quantity;

            // Two passes: plain stacks first, generated (affixed/named) items only if still short.
            // Callers that remove "by id" (crafting/building ingredients, the shop's buy rollback)
            // don't care which instance goes, so a single first-match pass used to happily eat a
            // player's Rare/Epic roll while an identical plain copy sat in a later slot - and in
            // ShopManager.BuyItem's rollback it would remove the generated item instead of the
            // plain one that was just added.
            remaining = RemoveByIdPass(itemId, remaining, generatedPass: false);
            remaining = RemoveByIdPass(itemId, remaining, generatedPass: true);

            bool success = remaining <= 0;

            if (success)
            {
                EventBus.Publish(new InventoryChangedEvent());
            }

            return success;
        }

        private int RemoveByIdPass(string itemId, int remaining, bool generatedPass)
        {
            foreach (InventorySlotData slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.itemId != itemId || slot.HasGeneratedData != generatedPass)
                {
                    continue;
                }

                int amountToRemove = Mathf.Min(slot.quantity, remaining);
                slot.quantity -= amountToRemove;
                remaining -= amountToRemove;

                if (slot.quantity <= 0)
                {
                    slot.Clear();
                }
            }

            return remaining;
        }

        public int GetTotalCount(string itemId)
        {
            EnsureSlotsInitialized();
            return _slots.Where(s => s.itemId == itemId).Sum(s => s.quantity);
        }

        // ---------------------------------------------------------------
        // Move / Split
        // ---------------------------------------------------------------

        public void MoveOrMergeSlot(int fromIndex, int toIndex)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(fromIndex) || !IsValidSlot(toIndex) || fromIndex == toIndex)
            {
                return;
            }

            InventorySlotData from = _slots[fromIndex];
            InventorySlotData to = _slots[toIndex];

            if (to.IsEmpty)
            {
                _slots[toIndex] = from;
                _slots[fromIndex] = InventorySlotData.Empty();
            }
            else if (to.itemId == from.itemId && !to.HasGeneratedData && !from.HasGeneratedData)
            {
                // Merging is only safe between plain stacks: merging a generated item into (or
                // with) another stack of the same base id would keep just one side's
                // affixes/name/rarity and silently discard the other's. Generated items swap below.
                ItemDefinition item = ItemDatabase.GetById(from.itemId);
                int maxStack = item != null ? item.MaxStack : int.MaxValue;
                int spaceInStack = maxStack - to.quantity;
                int amountToMove = Mathf.Min(spaceInStack, from.quantity);

                to.quantity += amountToMove;
                from.quantity -= amountToMove;

                if (from.quantity <= 0)
                {
                    from.Clear();
                }
            }
            else
            {
                _slots[fromIndex] = to;
                _slots[toIndex] = from;
            }

            EventBus.Publish(new InventoryChangedEvent());
        }

        public bool SplitStack(int slotIndex, int amount)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].quantity <= amount || amount <= 0)
            {
                return false;
            }

            int emptySlotIndex = Array.FindIndex(_slots, s => s.IsEmpty);

            if (emptySlotIndex < 0)
            {
                return false;
            }

            InventorySlotData source = _slots[slotIndex];
            _slots[emptySlotIndex].itemId = source.itemId;
            _slots[emptySlotIndex].quantity = amount;
            _slots[emptySlotIndex].currentDurability = source.currentDurability;
            _slots[emptySlotIndex].CopyGeneratedDataFrom(source);

            source.quantity -= amount;

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _slots.Length;
        }

        // ---------------------------------------------------------------
        // Filtering
        // ---------------------------------------------------------------

        public IEnumerable<InventorySlotData> GetSlotsByType(ItemType type)
        {
            EnsureSlotsInitialized();
            return _slots.Where(slot => !slot.IsEmpty && ItemDatabase.GetById(slot.itemId)?.Type == type);
        }

        public IEnumerable<InventorySlotData> GetSlotsByCategory(ItemCategory category)
        {
            EnsureSlotsInitialized();
            return _slots.Where(slot => !slot.IsEmpty && ItemDatabase.GetById(slot.itemId)?.Category == category);
        }

        // ---------------------------------------------------------------
        // Equipment
        // ---------------------------------------------------------------

        public bool EquipFromSlot(int slotIndex)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].IsEmpty)
            {
                return false;
            }

            InventorySlotData slot = _slots[slotIndex];
            ItemDefinition item = ItemDatabase.GetById(slot.itemId);

            if (item == null || !item.IsEquippable)
            {
                return false;
            }

            // Deep copy (affixes, generated name, rolled rarity, durability) captured BEFORE
            // RemoveFromSlot below — InventorySlotData is a class, so `slot` and
            // `_slots[slotIndex]` are the same object, and RemoveFromSlot's Clear() (once quantity
            // hits 0) wipes the generated fields on that shared instance.
            InventorySlotData equippedData = slot.CloneWithQuantity(1);

            // Take the new item out of the inventory FIRST, then return whatever was equipped in
            // that slot. This makes a swap work even with a full inventory (the old item lands in
            // the slot the new one just freed). It used to be the other way round, ignoring
            // UnequipSlot's result: with no free slot the old item was never returned to the
            // inventory and was then silently overwritten in _equipped below — destroyed.
            RemoveFromSlot(slotIndex, 1);

            if (_equipped.ContainsKey(item.EquipSlot) && !UnequipSlot(item.EquipSlot))
            {
                // Couldn't make room for the previously equipped item — put the new one back
                // exactly as it was (including its generated data) and abort.
                InventorySlotData source = _slots[slotIndex];

                if (source.IsEmpty)
                {
                    _slots[slotIndex] = equippedData;
                }
                else
                {
                    source.quantity += 1;
                }

                EventBus.Publish(new InventoryChangedEvent());
                return false;
            }

            _equipped[item.EquipSlot] = equippedData;
            ApplyEquipEffects(item, equippedData, item.EquipSlot);

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>
        /// Applies an equipped item's base <see cref="ItemDefinition.EquipStatModifiers"/> plus its
        /// procedurally-rolled affixes as permanent buffs under the slot's source key. Shared by
        /// <see cref="EquipFromSlot"/> and <see cref="RestoreState"/> — the latter used to re-apply
        /// only the base modifiers, so a generated item equipped across a save/load lost its
        /// affix bonuses even when its data survived.
        /// </summary>
        private void ApplyEquipEffects(ItemDefinition item, InventorySlotData equippedData, EquipmentSlot equipSlot)
        {
            if (_buffController == null || item == null)
            {
                return;
            }

            if (item.EquipStatModifiers.Length > 0)
            {
                _buffController.ApplyEffects(item.EquipStatModifiers.Select(WithPermanentDuration), GetEquipSourceKey(equipSlot));
            }

            // Procedurally-rolled affixes (ProceduralItemGenerator/GeneratedItemInstance) were
            // stored on the slot (generatedAffixIds/generatedAffixValues) but nothing anywhere in
            // the codebase ever read them back out to apply a real stat bonus — a Rare/Epic/
            // Legendary item's rolled affixes only ever showed up in its generated display name
            // ("Espada Flamejante da Fúria"), with zero actual gameplay effect once equipped, same
            // stats as a plain Common item. Mirrors the EquipStatModifiers path above exactly.
            string[] generatedAffixIds = equippedData.generatedAffixIds;
            float[] generatedAffixValues = equippedData.generatedAffixValues ?? Array.Empty<float>();

            if (generatedAffixIds != null && generatedAffixIds.Length > 0)
            {
                // Grouped by (stat, application) and summed before calling ApplyEffects: each call
                // to BuffController.ApplyEffect removes any existing effect for the same
                // affectedStat before adding its own (see BuffController.ApplyEffect), so two
                // rolled affixes hitting the same stat (e.g. this session's test roll had both
                // "affix_brutal" +10.6 and "affix_sharp" +2.5 AttackDamage) would otherwise have the
                // second silently wipe out the first instead of both contributing.
                var byStat = new Dictionary<(StatType, ModifierApplication), float>();
                for (int i = 0; i < generatedAffixIds.Length; i++)
                {
                    ItemAffixDefinition affix = AffixDatabase.GetById(generatedAffixIds[i]);
                    if (affix == null)
                    {
                        continue;
                    }

                    float value = i < generatedAffixValues.Length ? generatedAffixValues[i] : 0f;
                    var key = (affix.AffectedStat, affix.Application);
                    byStat.TryGetValue(key, out float existing);
                    byStat[key] = existing + value;
                }

                var generatedModifiers = byStat.Select(kvp => new StatModifierDefinition
                {
                    displayName = "generated_" + kvp.Key.Item1,
                    affectedStat = kvp.Key.Item1,
                    application = kvp.Key.Item2,
                    value = kvp.Value,
                });

                _buffController.ApplyEffects(generatedModifiers.Select(WithPermanentDuration), GetEquipSourceKey(equipSlot));
            }
        }

        public bool UnequipSlot(EquipmentSlot equipSlot)
        {
            EnsureSlotsInitialized();

            if (!_equipped.TryGetValue(equipSlot, out InventorySlotData equippedData))
            {
                return false;
            }

            ItemDefinition item = ItemDatabase.GetById(equippedData.itemId);

            if (item != null && !ReturnToInventory(item, equippedData))
            {
                // No inventory space to receive the unequipped item back — abort unequip.
                return false;
            }

            _buffController?.RemoveEffectsBySource(GetEquipSourceKey(equipSlot));
            _equipped.Remove(equipSlot);

            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        /// <summary>
        /// Puts a single previously-existing item (e.g. one being unequipped) back into the
        /// inventory without losing anything about it. Plain items go through
        /// <see cref="AddItem(ItemDefinition,int,float?)"/> as before (may stack, keeps
        /// durability). Generated items used to go through that same AddItem too, which only
        /// knows the base ItemDefinition — so unequipping a Rare/Epic/Legendary roll turned it
        /// into a plain base item (affixes, generated name and rarity gone). They now go, as a
        /// deep copy, into an empty slot of their own (never stacked), subject to the same weight
        /// limit AddItem enforces.
        /// </summary>
        private bool ReturnToInventory(ItemDefinition item, InventorySlotData data)
        {
            if (!data.HasGeneratedData)
            {
                return AddItem(item, 1, data.currentDurability) == 0;
            }

            if (!HasWeightCapacityFor(item, 1))
            {
                return false;
            }

            int emptySlotIndex = Array.FindIndex(_slots, s => s.IsEmpty);

            if (emptySlotIndex < 0)
            {
                return false;
            }

            _slots[emptySlotIndex] = data.CloneWithQuantity(1);
            EventBus.Publish(new InventoryChangedEvent());
            return true;
        }

        private static string GetEquipSourceKey(EquipmentSlot slot)
        {
            return $"equip_{slot}";
        }

        private static StatModifierDefinition WithPermanentDuration(StatModifierDefinition source)
        {
            return new StatModifierDefinition
            {
                displayName = source.displayName,
                affectedStat = source.affectedStat,
                application = source.application,
                value = source.value,
                durationSeconds = float.MaxValue / 2f,
                isDebuff = source.isDebuff,
                ticksOverTime = false,
                tickIntervalSeconds = source.tickIntervalSeconds,
                tickDamageOrHeal = 0f
            };
        }

        // ---------------------------------------------------------------
        // Consumables
        // ---------------------------------------------------------------

        public bool UseItemInSlot(int slotIndex)
        {
            EnsureSlotsInitialized();

            if (!IsValidSlot(slotIndex) || _slots[slotIndex].IsEmpty)
            {
                return false;
            }

            ItemDefinition item = ItemDatabase.GetById(_slots[slotIndex].itemId);

            if (item == null || !item.IsConsumable)
            {
                return false;
            }

            if (_playerStats != null)
            {
                if (item.HealthRestore > 0f)
                {
                    _playerStats.Heal(item.HealthRestore);
                }

                if (item.ManaRestore > 0f)
                {
                    _playerStats.RestoreMana(item.ManaRestore);
                }

                if (item.StaminaRestore > 0f)
                {
                    _playerStats.RestoreStamina(item.StaminaRestore);
                }
            }

            if (_buffController != null && item.ConsumeBuffs.Length > 0)
            {
                _buffController.ApplyEffects(item.ConsumeBuffs);
            }

            return RemoveFromSlot(slotIndex, 1);
        }

        // ---------------------------------------------------------------
        // Persistence
        // ---------------------------------------------------------------

        public object CaptureState()
        {
            EnsureSlotsInitialized();
            var equippedEntries = _equipped.Select(pair => new InventorySaveState.EquippedEntry
            {
                slot = pair.Key,
                itemId = pair.Value.itemId,
                durability = pair.Value.currentDurability,
                generatedAffixIds = pair.Value.generatedAffixIds,
                generatedAffixValues = pair.Value.generatedAffixValues,
                generatedName = pair.Value.generatedName,
                generatedRarity = pair.Value.generatedRarity
            }).ToArray();

            return new InventorySaveState
            {
                slots = _slots,
                equipped = equippedEntries
            };
        }

        public void RestoreState(object state)
        {
            if (state is not InventorySaveState saveState)
            {
                return;
            }

            _slots = new InventorySlotData[_slotCount];

            for (int i = 0; i < _slotCount; i++)
            {
                _slots[i] = i < saveState.slots.Length && saveState.slots[i] != null ? saveState.slots[i] : InventorySlotData.Empty();
            }

            _equipped.Clear();
            _buffController?.ClearAllEffects();

            foreach (InventorySaveState.EquippedEntry entry in saveState.equipped)
            {
                // Old saves (before EquippedEntry carried generated data) deserialize the generated
                // fields as null/empty/Common, which InventorySlotData treats as a plain item.
                var equippedData = new InventorySlotData
                {
                    itemId = entry.itemId,
                    quantity = 1,
                    currentDurability = entry.durability,
                    generatedAffixIds = entry.generatedAffixIds,
                    generatedAffixValues = entry.generatedAffixValues,
                    generatedName = entry.generatedName,
                    generatedRarity = entry.generatedRarity
                };
                _equipped[entry.slot] = equippedData;

                ApplyEquipEffects(ItemDatabase.GetById(entry.itemId), equippedData, entry.slot);
            }

            EventBus.Publish(new InventoryChangedEvent());
        }
    }
}
