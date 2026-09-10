using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Drops <see cref="EnemyDefinition.LootItem"/> straight into the killer's
    /// <see cref="InventoryManager"/> when an enemy dies, rolling <see cref="EnemyDefinition.LootDropChance"/>
    /// and a quantity in [<see cref="EnemyDefinition.LootQuantityMin"/>, LootQuantityMax]. Mirrors
    /// <see cref="EnemyRewardGranter"/>'s event wiring — both listen to the same
    /// <see cref="CombatTargetDiedEvent"/> already published by the real combat pipeline.
    ///
    /// Deliberately does not roll procedural affixes (<see cref="Rapadura.Gameplay.Items.Procedural.ProceduralItemGenerator"/>):
    /// <see cref="InventoryManager.AddItem"/> only stores a base <c>ItemDefinition</c> + quantity/durability,
    /// with no slot for a rolled affix list, so an affixed drop would have nowhere to persist its
    /// affixes today. Base-item loot (potions, materials, gold) already gives real drops without
    /// needing that larger inventory-format change.
    /// </summary>
    public class EnemyLootGranter : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (evt.Target == null || evt.Killer == null)
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null || enemy.Definition.LootItem == null)
            {
                return;
            }

            if (Random.value > enemy.Definition.LootDropChance)
            {
                return;
            }

            InventoryManager inventory = evt.Killer.GetComponentInParent<InventoryManager>();
            if (inventory == null)
            {
                return;
            }

            int quantity = Random.Range(enemy.Definition.LootQuantityMin, enemy.Definition.LootQuantityMax + 1);
            if (quantity <= 0)
            {
                return;
            }

            inventory.AddItem(enemy.Definition.LootItem, quantity);
        }
    }
}
