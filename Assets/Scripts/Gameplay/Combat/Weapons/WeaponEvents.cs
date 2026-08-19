using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Combat.Weapons
{
    /// <summary>Raised when a melee weapon lands an active swing (hitbox window opened), regardless of whether it hits anything.</summary>
    public readonly struct WeaponMeleeAttackEvent : IGameEvent
    {
        public readonly GameObject Wielder;
        public readonly string ItemId;

        public WeaponMeleeAttackEvent(GameObject wielder, string itemId)
        {
            Wielder = wielder;
            ItemId = itemId;
        }
    }

    /// <summary>Raised whenever a ranged weapon successfully fires a projectile.</summary>
    public readonly struct WeaponFiredEvent : IGameEvent
    {
        public readonly GameObject Wielder;
        public readonly string ItemId;
        public readonly int AmmoRemaining;

        public WeaponFiredEvent(GameObject wielder, string itemId, int ammoRemaining)
        {
            Wielder = wielder;
            ItemId = itemId;
            AmmoRemaining = ammoRemaining;
        }
    }

    /// <summary>Raised when a ranged weapon tries to fire with an empty magazine.</summary>
    public readonly struct WeaponOutOfAmmoEvent : IGameEvent
    {
        public readonly GameObject Wielder;
        public readonly string ItemId;

        public WeaponOutOfAmmoEvent(GameObject wielder, string itemId)
        {
            Wielder = wielder;
            ItemId = itemId;
        }
    }

    /// <summary>Raised when a reload begins.</summary>
    public readonly struct WeaponReloadStartedEvent : IGameEvent
    {
        public readonly GameObject Wielder;
        public readonly string ItemId;
        public readonly float Duration;

        public WeaponReloadStartedEvent(GameObject wielder, string itemId, float duration)
        {
            Wielder = wielder;
            ItemId = itemId;
            Duration = duration;
        }
    }

    /// <summary>Raised when a reload finishes (successfully or by running out of reserve ammo).</summary>
    public readonly struct WeaponReloadCompletedEvent : IGameEvent
    {
        public readonly GameObject Wielder;
        public readonly string ItemId;
        public readonly int CurrentAmmo;
        public readonly int MagazineSize;

        public WeaponReloadCompletedEvent(GameObject wielder, string itemId, int currentAmmo, int magazineSize)
        {
            Wielder = wielder;
            ItemId = itemId;
            CurrentAmmo = currentAmmo;
            MagazineSize = magazineSize;
        }
    }

    /// <summary>Raised whenever an equipped weapon's durability changes (usually a decrease from use).</summary>
    public readonly struct WeaponDurabilityChangedEvent : IGameEvent
    {
        public readonly GameObject Wielder;
        public readonly string ItemId;
        public readonly float CurrentDurability;
        public readonly float MaxDurability;

        public WeaponDurabilityChangedEvent(GameObject wielder, string itemId, float currentDurability, float maxDurability)
        {
            Wielder = wielder;
            ItemId = itemId;
            CurrentDurability = currentDurability;
            MaxDurability = maxDurability;
        }
    }

    /// <summary>Raised when a weapon's durability reaches zero and it is unequipped/broken.</summary>
    public readonly struct WeaponBrokenEvent : IGameEvent
    {
        public readonly GameObject Wielder;
        public readonly string ItemId;
        public readonly EquipmentSlot Slot;

        public WeaponBrokenEvent(GameObject wielder, string itemId, EquipmentSlot slot)
        {
            Wielder = wielder;
            ItemId = itemId;
            Slot = slot;
        }
    }
}
