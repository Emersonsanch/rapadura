using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Grants an enemy's loot straight into the killer's <see cref="InventoryManager"/> when it dies.
    /// What drops is decided by <see cref="EnemyLootRoller"/>: the definition's own
    /// <see cref="EnemyDefinition.LootItem"/> (gold), its biome's loot table (crafting materials and quest
    /// items — the biome is the one whose spawn table lists the enemy, <see cref="BiomeRegistry.FindBiomeOf"/>)
    /// and the boss bonus (<see cref="BossBonusRules"/>). Equipment comes as procedural pieces with rarity
    /// and affixes (<see cref="InventoryManager.AddGeneratedItem"/>). Mirrors
    /// <see cref="EnemyRewardGranter"/>'s event wiring — both listen to the same
    /// <see cref="CombatTargetDiedEvent"/> already published by the real combat pipeline.
    ///
    /// Works the same in an encounter arena and in the standalone Main.unity sandbox. Whatever lands in
    /// the inventory goes back to the world map with the rest of the player state
    /// (<c>SaveManager</c>'s carried state), so collect quests progress across the map ⇄ encounter loop.
    /// A short "+2 Madeira" text floats over the corpse for each drop (<see cref="CombatFxManager"/>).
    ///
    /// A full bag never eats loot silently (<see cref="LootGrantRules"/>, Docs/Balance/README.md §9): currency and
    /// the units an active quest still needs go in past the weight limit, and whatever is really lost raises
    /// <see cref="LootOverflowEvent"/> (the arena's "Mochila cheia" toast + a warning text over the corpse).
    /// </summary>
    public class EnemyLootGranter : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        private readonly System.Random _random = new System.Random();

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
            if (evt.Target == null)
            {
                return;
            }

            EnemyController enemy = evt.Target.GetComponent<EnemyController>();
            if (enemy == null || enemy.Definition == null)
            {
                return;
            }

            InventoryManager inventory = ResolveInventory(evt.Killer);
            if (inventory == null)
            {
                return;
            }

            EnemyDefinition definition = enemy.Definition;
            BiomeDefinition biome = BiomeRegistry.FindBiomeOf(definition);
            List<LootDrop> drops = EnemyLootRoller.Roll(definition, biome, _random, newGamePlusCycle: NewGamePlusProgress.ResolveCurrentCycle());

            Vector3 labelPosition = evt.Target.transform.position + Vector3.up * 1.8f;
            Func<string, int, int> outstanding = ResolveQuestNeeds();
            List<LootGrantResult> overflow = null;
            int line = 0;

            for (int i = 0; i < drops.Count; i++)
            {
                LootGrantResult result = LootGrantRules.Grant(inventory, drops[i], outstanding);
                if (result.Granted > 0)
                {
                    CombatFxManager.SpawnLootText(labelPosition + Vector3.up * (0.35f * line++), DescribeDrop(drops[i], result.Granted));
                }

                if (result.Lost > 0)
                {
                    string what = drops[i].IsGenerated ? drops[i].Generated.GeneratedName : drops[i].Item.ItemId;
                    GameLogger.Warning("Enemy", $"Loot '{what}' x{result.Lost} lost: killer inventory is full or over its weight limit.");
                }

                if (result.Lost > 0 || result.KeptOverWeight > 0)
                {
                    (overflow ??= new List<LootGrantResult>()).Add(result);
                }
            }

            if (drops.Count > 0)
            {
                EventBus.Publish(new EnemyLootDroppedEvent(evt.Target, inventory.gameObject, drops));
            }

            if (overflow != null)
            {
                EventBus.Publish(new LootOverflowEvent(labelPosition + Vector3.up * (0.35f * line), overflow));
            }
        }

        /// <summary>The live quest manager's "units still needed" query, or null outside a game session.</summary>
        private static Func<string, int, int> ResolveQuestNeeds()
        {
            return ServiceLocator.TryGet(out QuestManager quests) && quests != null ? quests.GetOutstandingCollectAmount : null;
        }

        /// <summary>
        /// The killer's inventory; for kills whose damage source isn't parented to the hero (or has no
        /// killer at all, e.g. a damage-over-time tick), the tagged Player's — this is a single-hero game.
        /// </summary>
        private static InventoryManager ResolveInventory(GameObject killer)
        {
            InventoryManager inventory = killer != null ? killer.GetComponentInParent<InventoryManager>() : null;
            if (inventory != null)
            {
                return inventory;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            return player != null ? player.GetComponent<InventoryManager>() : null;
        }

        private static string DescribeDrop(LootDrop drop, int quantity)
        {
            if (drop.IsGenerated)
            {
                return "+ " + drop.Generated.GeneratedName;
            }

            string name = drop.Item.LocalizedName();
            return quantity > 1 ? $"+{quantity} {name}" : "+ " + name;
        }
    }
}
