using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using UnityEngine;

namespace Rapadura.Gameplay.Items.Durability
{
    /// <summary>
    /// Wears the hero's equipment through <see cref="RepairService"/>: the weapon on every basic attack
    /// (<see cref="PlayerAttackPerformedEvent"/> from this object), the armour on every hit taken
    /// (<see cref="DamageAppliedEvent"/> targeting this object) and everything on a death (<see cref="PlayerDiedEvent"/>).
    /// Added at runtime by <see cref="InventoryManager"/> on the object tagged "Player", so no scene/prefab change.
    /// Presentation-free; online (Fase 12) the server applies the same wear from its own combat events.
    /// </summary>
    [DisallowMultipleComponent]
    public class EquipmentWearTracker : MonoBehaviour
    {
        private InventoryManager _bag;

        private void Awake()
        {
            _bag = GetComponent<InventoryManager>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<PlayerAttackPerformedEvent>(OnAttack);
            EventBus.Subscribe<DamageAppliedEvent>(OnDamage);
            EventBus.Subscribe<PlayerDiedEvent>(OnDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<PlayerAttackPerformedEvent>(OnAttack);
            EventBus.Unsubscribe<DamageAppliedEvent>(OnDamage);
            EventBus.Unsubscribe<PlayerDiedEvent>(OnDied);
        }

        private void OnAttack(PlayerAttackPerformedEvent evt)
        {
            if (evt.Attacker == gameObject)
            {
                RepairService.WearWeapon(_bag);
            }
        }

        private void OnDamage(DamageAppliedEvent evt)
        {
            if (evt.Target == gameObject && evt.FinalDamage > 0f)
            {
                RepairService.WearArmour(_bag);
            }
        }

        private void OnDied(PlayerDiedEvent evt)
        {
            RepairService.WearOnDeath(_bag);
        }
    }
}
