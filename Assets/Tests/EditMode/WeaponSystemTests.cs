using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Combat.Weapons;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 2 "Armas" systems: <see cref="MeleeWeapon"/>,
    /// <see cref="RangedWeapon"/>/<see cref="Projectile"/> (ammo + reload) and durability
    /// break-on-zero. <see cref="ItemDefinition"/> is a ScriptableObject with private
    /// [SerializeField] fields, so tests populate it via SerializedObject (same pattern as
    /// <c>SkillComboAndRespecTests</c>), and it is injected into <see cref="ItemDatabase"/>'s
    /// private static lookup via reflection since the database only otherwise loads assets from
    /// Resources — <see cref="ItemDatabase.Invalidate"/> resets it back to a clean state on teardown.
    /// </summary>
    public class WeaponSystemTests
    {
        private GameObject _wielder;
        private InventoryManager _inventory;

        private ItemDefinition _meleeWeaponItem;
        private ItemDefinition _rangedWeaponItem;
        private ItemDefinition _ammoItem;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _wielder = new GameObject("Wielder");
            _inventory = _wielder.AddComponent<InventoryManager>();

            _meleeWeaponItem = CreateItem("weapon_sword", ItemType.Weapon, EquipmentSlot.MainHand, hasDurability: true, maxDurability: 2f, weaponDamage: 15f, maxStack: 1);
            _rangedWeaponItem = CreateItem("weapon_bow", ItemType.Weapon, EquipmentSlot.MainHand, hasDurability: true, maxDurability: 100f, weaponDamage: 8f, maxStack: 1);
            _ammoItem = CreateItem("ammo_arrow", ItemType.Material, EquipmentSlot.None, hasDurability: false, maxDurability: 0f, weaponDamage: 0f, maxStack: 99);

            RegisterItems(_meleeWeaponItem, _rangedWeaponItem, _ammoItem);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
            Object.DestroyImmediate(_wielder);
            Object.DestroyImmediate(_meleeWeaponItem);
            Object.DestroyImmediate(_rangedWeaponItem);
            Object.DestroyImmediate(_ammoItem);
        }

        // ---------------------------------------------------------------
        // Melee
        // ---------------------------------------------------------------

        [Test]
        public void MeleeWeapon_TryAttack_ActivatesHitboxAndDealsDamage()
        {
            EquipItem(_meleeWeaponItem);
            MeleeWeapon melee = BuildMeleeWeapon();
            (Hurtbox hurtbox, Health health, Collider targetCollider) = BuildTarget();

            bool attacked = melee.TryAttack();
            float healthAfterHitboxEnter = SimulateHit(melee, targetCollider, health);

            Assert.IsTrue(attacked);
            Assert.Less(healthAfterHitboxEnter, health.MaxHealth);
            Object.DestroyImmediate(hurtbox.gameObject);
        }

        [Test]
        public void MeleeWeapon_CannotAttackAgainDuringCooldown()
        {
            EquipItem(_meleeWeaponItem);
            MeleeWeapon melee = BuildMeleeWeapon();

            bool first = melee.TryAttack();
            bool second = melee.TryAttack();

            Assert.IsTrue(first);
            Assert.IsFalse(second);
        }

        [Test]
        public void MeleeWeapon_WithoutEquippedWeapon_CannotAttack()
        {
            MeleeWeapon melee = BuildMeleeWeapon();

            bool attacked = melee.TryAttack();

            Assert.IsFalse(attacked);
        }

        [Test]
        public void MeleeWeapon_DurabilityReachesZero_BreaksAndUnequips()
        {
            EquipItem(_meleeWeaponItem); // maxDurability = 2, cost per use = 1
            MeleeWeapon melee = BuildMeleeWeapon();

            melee.TryAttack();
            melee.Tick(10f); // clear cooldown
            bool secondAttack = melee.TryAttack();

            Assert.IsTrue(secondAttack);
            Assert.IsTrue(melee.IsBroken);
            Assert.IsFalse(_inventory.Equipped.ContainsKey(EquipmentSlot.MainHand));
        }

        [Test]
        public void MeleeWeapon_BreakingWeapon_PublishesWeaponBrokenEvent()
        {
            EquipItem(_meleeWeaponItem);
            MeleeWeapon melee = BuildMeleeWeapon();
            bool brokenEventReceived = false;
            EventBus.Subscribe<WeaponBrokenEvent>(_ => brokenEventReceived = true);

            melee.TryAttack();
            melee.Tick(10f);
            melee.TryAttack();

            Assert.IsTrue(brokenEventReceived);
        }

        // ---------------------------------------------------------------
        // Ranged / Ammo / Reload
        // ---------------------------------------------------------------

        [Test]
        public void RangedWeapon_TryFire_ConsumesAmmoAndPublishesFiredEvent()
        {
            EquipItem(_rangedWeaponItem);
            RangedWeapon ranged = BuildRangedWeapon(magazineSize: 3, startingAmmo: 3);
            bool firedEventReceived = false;
            EventBus.Subscribe<WeaponFiredEvent>(_ => firedEventReceived = true);

            bool fired = ranged.TryFire(Vector3.forward);

            Assert.IsTrue(fired);
            Assert.AreEqual(2, ranged.CurrentAmmo);
            Assert.IsTrue(firedEventReceived);
        }

        [Test]
        public void RangedWeapon_OutOfAmmo_CannotFireAndPublishesOutOfAmmoEvent()
        {
            EquipItem(_rangedWeaponItem);
            RangedWeapon ranged = BuildRangedWeapon(magazineSize: 1, startingAmmo: 0);
            bool outOfAmmoEventReceived = false;
            EventBus.Subscribe<WeaponOutOfAmmoEvent>(_ => outOfAmmoEventReceived = true);

            bool fired = ranged.TryFire(Vector3.forward);

            Assert.IsFalse(fired);
            Assert.IsTrue(outOfAmmoEventReceived);
        }

        [Test]
        public void RangedWeapon_Reload_PullsAmmoFromInventoryReserve()
        {
            EquipItem(_rangedWeaponItem);
            _inventory.AddItem(_ammoItem, 10);
            RangedWeapon ranged = BuildRangedWeapon(magazineSize: 5, startingAmmo: 0);

            bool started = ranged.TryReload();
            ranged.Tick(999f); // exceed reload duration to force completion

            Assert.IsTrue(started);
            Assert.IsFalse(ranged.IsReloading);
            Assert.AreEqual(5, ranged.CurrentAmmo);
            Assert.AreEqual(5, _inventory.GetTotalCount("ammo_arrow"));
        }

        [Test]
        public void RangedWeapon_CannotFireWhileReloading()
        {
            EquipItem(_rangedWeaponItem);
            _inventory.AddItem(_ammoItem, 10);
            RangedWeapon ranged = BuildRangedWeapon(magazineSize: 5, startingAmmo: 2);

            bool started = ranged.TryReload();
            bool firedWhileReloading = ranged.TryFire(Vector3.forward);

            Assert.IsTrue(started);
            Assert.IsFalse(firedWhileReloading);
        }

        [Test]
        public void RangedWeapon_ReloadWithNoReserveAmmo_FailsToStart()
        {
            EquipItem(_rangedWeaponItem);
            RangedWeapon ranged = BuildRangedWeapon(magazineSize: 5, startingAmmo: 0);

            bool started = ranged.TryReload();

            Assert.IsFalse(started);
        }

        // ---------------------------------------------------------------
        // Projectile
        // ---------------------------------------------------------------

        [Test]
        public void Projectile_AdvanceStep_MovesForwardAlongDirection()
        {
            var projectileGo = new GameObject("Projectile");
            projectileGo.AddComponent<BoxCollider>().isTrigger = true;
            Projectile projectile = projectileGo.AddComponent<Projectile>();
            projectile.Launch(Vector3.forward, speed: 10f, damage: 5f, ElementType.Physical, _wielder);

            projectile.AdvanceStep(1f);

            Assert.AreEqual(new Vector3(0f, 0f, 10f), projectileGo.transform.position);
            Object.DestroyImmediate(projectileGo);
        }

        [Test]
        public void Projectile_ProcessCollision_WithHurtbox_DealsDamageViaHitbox()
        {
            var projectileGo = new GameObject("Projectile");
            projectileGo.AddComponent<BoxCollider>().isTrigger = true;
            Projectile projectile = projectileGo.AddComponent<Projectile>();
            projectile.Launch(Vector3.forward, speed: 10f, damage: 25f, ElementType.Physical, _wielder);

            (Hurtbox hurtbox, Health health, Collider targetCollider) = BuildTarget();
            float before = health.CurrentHealth;

            bool hit = projectile.ProcessCollision(targetCollider);

            Assert.IsTrue(hit);
            Assert.Less(health.CurrentHealth, before);
            Object.DestroyImmediate(projectileGo);
            Object.DestroyImmediate(hurtbox.gameObject);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private MeleeWeapon BuildMeleeWeapon()
        {
            MeleeWeapon melee = _wielder.AddComponent<MeleeWeapon>();
            var hitboxGo = new GameObject("Hitbox");
            hitboxGo.transform.SetParent(_wielder.transform);
            hitboxGo.AddComponent<BoxCollider>().isTrigger = true;
            Hitbox hitbox = hitboxGo.AddComponent<Hitbox>();

            SetPrivateField(melee, "_hitbox", hitbox);
            SetPrivateField(melee, "_attackCooldown", 1f);
            SetPrivateField(melee, "_activeWindowDuration", 0.2f);
            SetPrivateField(melee, "_durabilityCostPerUse", 1f);
            SetPrivateField(melee, "_equipSlot", EquipmentSlot.MainHand);

            InvokeAwake(melee);
            return melee;
        }

        private RangedWeapon BuildRangedWeapon(int magazineSize, int startingAmmo)
        {
            RangedWeapon ranged = _wielder.AddComponent<RangedWeapon>();

            SetPrivateField(ranged, "_magazineSize", magazineSize);
            SetPrivateField(ranged, "_currentAmmo", startingAmmo);
            SetPrivateField(ranged, "_ammoItemId", "ammo_arrow");
            SetPrivateField(ranged, "_reloadDuration", 1f);
            SetPrivateField(ranged, "_durabilityCostPerUse", 1f);
            SetPrivateField(ranged, "_equipSlot", EquipmentSlot.MainHand);

            InvokeAwake(ranged);
            return ranged;
        }

        private void EquipItem(ItemDefinition item)
        {
            int leftover = _inventory.AddItem(item, 1);
            Assert.AreEqual(0, leftover, "test setup: item must fit in inventory");

            int slotIndex = System.Array.FindIndex((InventorySlotData[])GetPrivateField(_inventory, "_slots"), s => s.itemId == item.ItemId);
            bool equipped = _inventory.EquipFromSlot(slotIndex);
            Assert.IsTrue(equipped, "test setup: item must equip successfully");
        }

        private static (Hurtbox hurtbox, Health health, Collider collider) BuildTarget()
        {
            var targetGo = new GameObject("Target");
            Collider collider = targetGo.AddComponent<BoxCollider>();
            Health health = targetGo.AddComponent<Health>();
            Hurtbox hurtbox = targetGo.AddComponent<Hurtbox>();
            return (hurtbox, health, collider);
        }

        /// <summary>Drives a hitbox's trigger-enter logic directly (physics doesn't run in EditMode).</summary>
        private static float SimulateHit(MeleeWeapon melee, Collider targetCollider, Health health)
        {
            Hitbox hitbox = (Hitbox)GetPrivateField(melee, "_hitbox");
            hitbox.ProcessTriggerEnter(targetCollider);
            return health.CurrentHealth;
        }

        private static ItemDefinition CreateItem(string itemId, ItemType type, EquipmentSlot equipSlot, bool hasDurability, float maxDurability, float weaponDamage, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);

            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_hasDurability").boolValue = hasDurability;
            serialized.FindProperty("_maxDurability").floatValue = maxDurability;
            serialized.FindProperty("_equipSlot").enumValueIndex = (int)equipSlot;
            serialized.FindProperty("_weaponDamage").floatValue = weaponDamage;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return item;
        }

        private static void RegisterItems(params ItemDefinition[] items)
        {
            FieldInfo field = typeof(ItemDatabase).GetField("_itemsById", BindingFlags.NonPublic | BindingFlags.Static);
            var map = new System.Collections.Generic.Dictionary<string, ItemDefinition>();

            foreach (ItemDefinition item in items)
            {
                map[item.ItemId] = item;
            }

            field.SetValue(null, map);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = FindFieldInHierarchy(target.GetType(), fieldName);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }

        private static object GetPrivateField(object target, string fieldName)
        {
            FieldInfo field = FindFieldInHierarchy(target.GetType(), fieldName);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            return field.GetValue(target);
        }

        /// <summary>Private [SerializeField]s declared on a base class (e.g. WeaponController) are not
        /// returned by GetField on the derived type even with FlattenHierarchy (that flag excludes
        /// private members), so walk the chain manually.</summary>
        private static FieldInfo FindFieldInHierarchy(System.Type type, string fieldName)
        {
            for (System.Type current = type; current != null; current = current.BaseType)
            {
                FieldInfo field = current.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (field != null)
                {
                    return field;
                }
            }

            return null;
        }

        private static void InvokeAwake(object component)
        {
            MethodInfo method = component.GetType().GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            method?.Invoke(component, null);
        }
    }
}
