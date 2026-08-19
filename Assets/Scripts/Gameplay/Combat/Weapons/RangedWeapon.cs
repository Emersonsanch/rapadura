using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Combat.Weapons
{
    /// <summary>
    /// Ranged weapon: fires a <see cref="Projectile"/> per shot, consuming ammo from a magazine
    /// that is refilled from the wielder's <see cref="InventoryManager"/>
    /// reserve on reload — the same "ammo item id + magazine size + reserve pulled from inventory"
    /// shape used by common Unity weapon frameworks (e.g. TopDown Engine's WeaponAmmo component),
    /// adapted to this project's existing inventory API instead of a bespoke ammo pool.
    /// </summary>
    public class RangedWeapon : WeaponController
    {
        [Header("Projectile")]
        [SerializeField] private Projectile _projectilePrefab;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private float _projectileSpeed = 20f;
        [SerializeField] private float _fallbackDamage = 8f;
        [SerializeField] private ElementType _damageElement = ElementType.Physical;

        [Header("Ammo")]
        [Tooltip("Item id (as known by ItemDatabase) consumed from inventory reserve to refill the magazine on reload.")]
        [SerializeField] private string _ammoItemId = "ammo_arrow";
        [SerializeField] private int _magazineSize = 6;
        [SerializeField] private int _currentAmmo;

        [Header("Reload")]
        [SerializeField] private float _reloadDuration = 1.5f;

        private float _reloadTimer;
        private bool _isReloading;

        public int CurrentAmmo => _currentAmmo;
        public int MagazineSize => _magazineSize;
        public bool IsReloading => _isReloading;
        public bool CanFire => !IsBroken && !_isReloading && _currentAmmo > 0 && HasUsableWeaponEquipped();

        [SerializeField] private InventoryManager _reserveInventory;

        protected override void Awake()
        {
            base.Awake();

            if (_reserveInventory == null)
            {
                _reserveInventory = GetComponent<InventoryManager>();
            }

            if (_reserveInventory == null)
            {
                _reserveInventory = GetComponentInParent<InventoryManager>();
            }

            _currentAmmo = Mathf.Clamp(_currentAmmo, 0, _magazineSize);
        }

        private void Update()
        {
            Tick(Time.deltaTime);
        }

        /// <summary>Advances the reload timer. Safe to call directly from tests.</summary>
        public void Tick(float deltaTime)
        {
            if (!_isReloading)
            {
                return;
            }

            _reloadTimer -= deltaTime;

            if (_reloadTimer <= 0f)
            {
                CompleteReload();
            }
        }

        /// <summary>Attempts to fire a projectile. Returns false if reloading, broken, or out of ammo (which raises WeaponOutOfAmmoEvent).</summary>
        public bool TryFire(Vector3 direction)
        {
            if (IsBroken || _isReloading || !HasUsableWeaponEquipped())
            {
                return false;
            }

            if (_currentAmmo <= 0)
            {
                EventBus.Publish(new WeaponOutOfAmmoEvent(gameObject, EquippedItem?.ItemId));
                return false;
            }

            _currentAmmo--;
            SpawnProjectile(direction);

            EventBus.Publish(new WeaponFiredEvent(gameObject, EquippedItem?.ItemId, _currentAmmo));

            if (_currentAmmo <= 0)
            {
                EventBus.Publish(new WeaponOutOfAmmoEvent(gameObject, EquippedItem?.ItemId));
            }

            // The shot that exhausts durability still fires — only the *next* attempt is blocked
            // once IsBroken is set (mirrors MeleeWeapon.TryAttack).
            ConsumeDurabilityForUse();

            return true;
        }

        private void SpawnProjectile(Vector3 direction)
        {
            if (_projectilePrefab == null || !Application.isPlaying)
            {
                return;
            }

            Transform origin = _muzzle != null ? _muzzle : transform;
            Projectile projectile = Object.Instantiate(_projectilePrefab, origin.position, origin.rotation);
            projectile.Launch(direction, _projectileSpeed, ResolveWeaponDamage(_fallbackDamage), _damageElement, gameObject);
        }

        /// <summary>Attempts to start a reload. Returns false if already full, already reloading, broken, or no reserve ammo is available.</summary>
        public bool TryReload()
        {
            if (IsBroken || _isReloading || _currentAmmo >= _magazineSize)
            {
                return false;
            }

            if (_reserveInventory == null || _reserveInventory.GetTotalCount(_ammoItemId) <= 0)
            {
                return false;
            }

            _isReloading = true;
            _reloadTimer = _reloadDuration;

            EventBus.Publish(new WeaponReloadStartedEvent(gameObject, EquippedItem?.ItemId, _reloadDuration));
            return true;
        }

        private void CompleteReload()
        {
            _isReloading = false;

            int needed = _magazineSize - _currentAmmo;

            if (needed > 0 && _reserveInventory != null)
            {
                int available = _reserveInventory.GetTotalCount(_ammoItemId);
                int amountToLoad = Mathf.Min(needed, available);

                if (amountToLoad > 0 && _reserveInventory.RemoveById(_ammoItemId, amountToLoad))
                {
                    _currentAmmo += amountToLoad;
                }
            }

            EventBus.Publish(new WeaponReloadCompletedEvent(gameObject, EquippedItem?.ItemId, _currentAmmo, _magazineSize));
        }
    }
}
