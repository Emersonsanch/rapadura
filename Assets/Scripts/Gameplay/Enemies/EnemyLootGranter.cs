using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items.Procedural;
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
    /// The base loot never rolls procedural affixes. Bosses add on top of it (see
    /// <see cref="BossBonusRules"/>): their base loot is guaranteed, and an optional
    /// <see cref="EnemyDefinition.BossBonusDropItem"/> is rolled through
    /// <see cref="ProceduralItemGenerator"/> and stored with its affixes via
    /// <see cref="InventoryManager.AddGeneratedItem"/>.
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
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            InventoryManager inventory = evt.Killer.GetComponentInParent<InventoryManager>();
            if (inventory == null)
            {
                return;
            }

            EnemyDefinition definition = enemy.Definition;

            if (BossBonusRules.ShouldDropLoot(definition, Random.value))
            {
                int quantity = Random.Range(definition.LootQuantityMin, definition.LootQuantityMax + 1);
                if (quantity > 0)
                {
                    inventory.AddItem(definition.LootItem, quantity);
                }
            }

            GeneratedItemInstance bonusDrop = BossBonusRules.RollBonusDrop(definition);
            if (bonusDrop != null && !inventory.AddGeneratedItem(bonusDrop))
            {
                GameLogger.Warning("Enemy", $"Boss bonus drop '{bonusDrop.GeneratedName}' lost: killer inventory has no empty slot.");
            }
        }
    }
}
