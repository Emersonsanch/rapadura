using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Combat.Weapons
{
    /// <summary>
    /// Shared base for equippable weapons (<see cref="MeleeWeapon"/>, <see cref="RangedWeapon"/>).
    /// Mirrors the "WeaponBase + WeaponAmmo-style component" split commonly used in Unity weapon
    /// frameworks (e.g. MoreMountains' TopDown Engine): a base class owns identity/durability
    /// concerns that are common to every weapon, while concrete subclasses own how the weapon
    /// actually deals damage. Deliberately does not duplicate <see cref="InventoryManager"/> or
    /// <see cref="ItemDefinition"/> logic — it only reads/writes through their existing public API
    /// (the <see cref="InventorySlotData"/> instance returned by <c>InventoryManager.Equipped</c>
    /// is the same live object the inventory owns, so mutating its <c>currentDurability</c> here
    /// is a legitimate, already-supported pattern, not a workaround).
    /// </summary>
    public abstract class WeaponController : MonoBehaviour
    {
        [Header("Equip Binding")]
        [Tooltip("Inventory this weapon draws its equipped item/durability/ammo from. Auto-resolved from this or a parent object if unset.")]
        [SerializeField] private InventoryManager _inventory;

        [Tooltip("Equipment slot this weapon occupies. Used to look up the live InventorySlotData and to unequip on break.")]
        [SerializeField] private EquipmentSlot _equipSlot = EquipmentSlot.MainHand;

        [Tooltip("Durability consumed from the equipped item per use (per swing / per shot).")]
        [SerializeField] private float _durabilityCostPerUse = 1f;

        public EquipmentSlot EquipSlot => _equipSlot;
        public bool IsBroken { get; private set; }

        /// <summary>The item currently equipped in this weapon's slot, or null if nothing is equipped there.</summary>
        public ItemDefinition EquippedItem
        {
            get
            {
                InventorySlotData slot = EquippedSlotData;
                return slot != null ? ItemDatabase.GetById(slot.itemId) : null;
            }
        }

        /// <summary>Live reference to the equipped slot's runtime data (durability, item id), or null if empty.</summary>
        protected InventorySlotData EquippedSlotData
        {
            get
            {
                if (_inventory == null)
                {
                    return null;
                }

                return _inventory.Equipped.TryGetValue(_equipSlot, out InventorySlotData data) ? data : null;
            }
        }

        protected virtual void Awake()
        {
            if (_inventory == null)
            {
                _inventory = GetComponent<InventoryManager>();
            }

            if (_inventory == null)
            {
                _inventory = GetComponentInParent<InventoryManager>();
            }
        }

        /// <summary>
        /// True when this weapon is usable: something is equipped in its slot, that item has
        /// weapon stats/durability tracking, and it has not broken yet.
        /// </summary>
        public bool HasUsableWeaponEquipped()
        {
            if (IsBroken)
            {
                return false;
            }

            ItemDefinition item = EquippedItem;
            return item != null && item.Type == ItemType.Weapon;
        }

        /// <summary>Damage to use for a hit, sourced from the equipped item's <see cref="ItemDefinition.WeaponDamage"/> when available.</summary>
        protected float ResolveWeaponDamage(float fallback)
        {
            ItemDefinition item = EquippedItem;
            return item != null && item.WeaponDamage > 0f ? item.WeaponDamage : fallback;
        }

        /// <summary>
        /// Consumes <see cref="_durabilityCostPerUse"/> from the equipped item (if it tracks
        /// durability), publishing change/break events. Returns false if the weapon broke as a
        /// result of this call (or was already broken/unequipped), true if it is still usable.
        /// </summary>
        protected bool ConsumeDurabilityForUse()
        {
            if (IsBroken)
            {
                return false;
            }

            InventorySlotData slotData = EquippedSlotData;
            ItemDefinition item = EquippedItem;

            if (slotData == null || item == null || !item.HasDurability)
            {
                // No durability tracking configured for this item — treat every use as valid.
                return true;
            }

            slotData.currentDurability = Mathf.Max(0f, slotData.currentDurability - _durabilityCostPerUse);

            EventBus.Publish(new WeaponDurabilityChangedEvent(gameObject, item.ItemId, slotData.currentDurability, item.MaxDurability));

            if (slotData.currentDurability <= 0f)
            {
                Break(item);
                return false;
            }

            return true;
        }

        private void Break(ItemDefinition item)
        {
            IsBroken = true;
            GameLogger.Info("Weapon", $"{item.ItemId} broke (durability reached 0) and was unequipped from {_equipSlot}.");
            EventBus.Publish(new WeaponBrokenEvent(gameObject, item.ItemId, _equipSlot));
            _inventory?.UnequipSlot(_equipSlot);
        }

        /// <summary>Resets the broken flag — used when a fresh (non-broken) weapon is equipped into this slot.</summary>
        public void ResetBrokenState()
        {
            IsBroken = false;
        }
    }
}
