using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>JSON-serializable snapshot of <see cref="VillageChestStorage"/>: every slot in order (empty ones included).</summary>
    [Serializable]
    public class VillageChestSaveState
    {
        public InventorySlotData[] slots = Array.Empty<InventorySlotData>();
    }

    /// <summary>Raised whenever the village chest's contents change (store, take, load, new game).</summary>
    public readonly struct VillageChestChangedEvent : IGameEvent
    {
    }

    /// <summary>
    /// "Baú da Vó Zefa": the village's storage chest, reached from the world map's village hub
    /// (<c>VillageHubButton</c> → <c>VillageChestUIController</c>). <see cref="DefaultSlotCount"/> slots, no weight
    /// limit, stacks like the bag (plain stacks merge up to <see cref="ItemDefinition.MaxStack"/>; generated or
    /// non-stackable pieces take a slot each, affixes and durability kept).
    /// <list type="bullet">
    /// <item>Per save slot: a GameManager-built manager registered with <c>SaveManager</c> under <see cref="Key"/>
    /// and with <c>NewSessionReset</c>, so "Novo jogo" and every load start from an empty chest, and a save
    /// written before the chest existed (no entry) loads as an empty chest.</item>
    /// <item>Moving items between bag and chest (weight/slot rules) lives in <see cref="VillageChestTransfer"/>.</item>
    /// </list>
    /// </summary>
    public class VillageChestStorage : IManager, ISaveable, IResettable
    {
        public const string Key = "VillageChest";
        public const int DefaultSlotCount = 60;

        private readonly int _slotCount;
        private readonly Func<string, ItemDefinition> _resolveItem;
        private InventorySlotData[] _slots;

        /// <param name="resolveItem">Item lookup (stack sizes); defaults to <see cref="ItemDatabase.GetById"/>.</param>
        public VillageChestStorage(int slotCount = DefaultSlotCount, Func<string, ItemDefinition> resolveItem = null)
        {
            _slotCount = Mathf.Max(1, slotCount);
            _resolveItem = resolveItem ?? ItemDatabase.GetById;
            _slots = CreateEmptySlots(_slotCount);
        }

        private static VillageChestStorage _standalone;

        /// <summary>
        /// The GameManager's chest (ServiceLocator); without one (a scene played on its own) a process-wide
        /// stand-alone chest, which is not saved.
        /// </summary>
        public static VillageChestStorage ResolveShared()
        {
            if (ServiceLocator.TryGet(out VillageChestStorage chest))
            {
                return chest;
            }

            return _standalone ??= new VillageChestStorage();
        }

        public string SaveKey => Key;

        public int SlotCount => _slotCount;

        public IReadOnlyList<InventorySlotData> Slots => _slots;

        public int UsedSlotCount
        {
            get
            {
                int used = 0;
                foreach (InventorySlotData slot in _slots)
                {
                    if (!slot.IsEmpty)
                    {
                        used++;
                    }
                }

                return used;
            }
        }

        public int FreeSlotCount => _slotCount - UsedSlotCount;

        public bool IsEmpty => UsedSlotCount == 0;

        public void Initialize()
        {
        }

        public void Shutdown()
        {
        }

        public int GetTotalCount(string itemId)
        {
            int total = 0;
            foreach (InventorySlotData slot in _slots)
            {
                if (!slot.IsEmpty && slot.itemId == itemId)
                {
                    total += slot.quantity;
                }
            }

            return total;
        }

        /// <summary>How many of <paramref name="quantity"/> units like <paramref name="source"/> would fit right now (nothing changes).</summary>
        public int GetStorableAmount(InventorySlotData source, int quantity)
        {
            return Place(source, quantity, apply: false);
        }

        /// <summary>
        /// Stores up to <paramref name="quantity"/> units like <paramref name="source"/> (id, durability and generated
        /// data are copied, <paramref name="source"/> itself is not changed). Returns how many went in.
        /// </summary>
        public int Store(InventorySlotData source, int quantity)
        {
            int stored = Place(source, quantity, apply: true);
            if (stored > 0)
            {
                EventBus.Publish(new VillageChestChangedEvent());
            }

            return stored;
        }

        public bool RemoveFromSlot(int slotIndex, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Length || quantity <= 0 || _slots[slotIndex].IsEmpty || _slots[slotIndex].quantity < quantity)
            {
                return false;
            }

            _slots[slotIndex].quantity -= quantity;
            if (_slots[slotIndex].quantity <= 0)
            {
                _slots[slotIndex].Clear();
            }

            EventBus.Publish(new VillageChestChangedEvent());
            return true;
        }

        /// <summary>Empties the chest (new game).</summary>
        public void Clear()
        {
            _slots = CreateEmptySlots(_slotCount);
            EventBus.Publish(new VillageChestChangedEvent());
        }

        void IResettable.ResetForNewSession() => Clear();

        public object CaptureState()
        {
            var copy = new InventorySlotData[_slots.Length];
            for (int i = 0; i < _slots.Length; i++)
            {
                copy[i] = _slots[i].IsEmpty ? InventorySlotData.Empty() : _slots[i].CloneWithQuantity(_slots[i].quantity);
            }

            return new VillageChestSaveState { slots = copy };
        }

        public void RestoreState(object state)
        {
            if (state is not VillageChestSaveState saved)
            {
                return;
            }

            _slots = CreateEmptySlots(_slotCount);
            if (saved.slots != null)
            {
                // Same positions as saved; anything past the slot count (a smaller chest in a later version)
                // is packed into the free slots left, so nothing silently disappears while there is room.
                var overflow = new List<InventorySlotData>();
                for (int i = 0; i < saved.slots.Length; i++)
                {
                    InventorySlotData slot = saved.slots[i];
                    if (slot == null || slot.IsEmpty)
                    {
                        continue;
                    }

                    if (i < _slotCount)
                    {
                        _slots[i] = slot.CloneWithQuantity(slot.quantity);
                    }
                    else
                    {
                        overflow.Add(slot);
                    }
                }

                foreach (InventorySlotData extra in overflow)
                {
                    int free = Array.FindIndex(_slots, s => s.IsEmpty);
                    if (free < 0)
                    {
                        break;
                    }

                    _slots[free] = extra.CloneWithQuantity(extra.quantity);
                }
            }

            EventBus.Publish(new VillageChestChangedEvent());
        }

        // ------------------------------------------------------------------

        private int Place(InventorySlotData source, int quantity, bool apply)
        {
            if (source == null || source.IsEmpty || quantity <= 0)
            {
                return 0;
            }

            ItemDefinition item = _resolveItem(source.itemId);
            bool stacks = item != null && item.IsStackable && !source.HasGeneratedData;

            if (!stacks)
            {
                // A generated roll / a piece of gear: one slot, never merged (merging would drop affixes).
                int free = Array.FindIndex(_slots, s => s.IsEmpty);
                if (free < 0)
                {
                    return 0;
                }

                if (apply)
                {
                    _slots[free] = source.CloneWithQuantity(quantity);
                }

                return quantity;
            }

            int maxStack = item.MaxStack;
            int remaining = quantity;

            foreach (InventorySlotData slot in _slots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (slot.IsEmpty || slot.itemId != source.itemId || slot.HasGeneratedData || slot.quantity >= maxStack)
                {
                    continue;
                }

                int add = Mathf.Min(maxStack - slot.quantity, remaining);
                if (apply)
                {
                    slot.quantity += add;
                }

                remaining -= add;
            }

            for (int i = 0; i < _slots.Length && remaining > 0; i++)
            {
                if (!_slots[i].IsEmpty)
                {
                    continue;
                }

                int add = Mathf.Min(maxStack, remaining);
                if (apply)
                {
                    _slots[i] = source.CloneWithQuantity(add);
                }

                remaining -= add;
            }

            return quantity - remaining;
        }

        private static InventorySlotData[] CreateEmptySlots(int count)
        {
            var slots = new InventorySlotData[count];
            for (int i = 0; i < count; i++)
            {
                slots[i] = InventorySlotData.Empty();
            }

            return slots;
        }
    }
}
