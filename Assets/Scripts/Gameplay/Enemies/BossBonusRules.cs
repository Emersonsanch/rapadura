using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items.Procedural;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// Pure rules for the extra rewards a boss (<see cref="EnemyDefinition.IsBoss"/>) gives on death,
    /// used by <see cref="EnemyLootGranter"/> and <see cref="EnemyRewardGranter"/> and kept free of
    /// MonoBehaviour/EventBus state so EditMode tests can cover them directly.
    ///
    /// Scope deliberately limited to what the design docs support today — Docs/GDD.md only says the
    /// loop climaxes in "enfrentar bosses" and TODO.md lists "bônus especiais de boss"/"Loot raro"
    /// without numbers — so every bonus reuses an existing system and is tuned per boss asset:
    /// <list type="bullet">
    /// <item><b>Guaranteed loot</b>: the boss's normal <see cref="EnemyDefinition.LootItem"/> ignores
    /// <see cref="EnemyDefinition.LootDropChance"/> (default on).</item>
    /// <item><b>Rare affixed drop</b>: an optional <see cref="EnemyDefinition.BossBonusDropItem"/> rolled
    /// via <see cref="ProceduralItemGenerator"/> at <see cref="EnemyDefinition.BossBonusDropRarity"/>
    /// (default Rare) and stored through <c>InventoryManager.AddGeneratedItem</c>.</item>
    /// <item><b>Extra XP</b>: <see cref="EnemyDefinition.BossExperienceMultiplier"/> (default 1 = none).</item>
    /// </list>
    /// Non-boss enemies always get exactly the old behavior.
    /// </summary>
    public static class BossBonusRules
    {
        /// <summary>Drop chance actually used for the definition's base loot item.</summary>
        public static float GetEffectiveLootDropChance(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return 0f;
            }

            if (definition.IsBoss && definition.BossGuaranteedLoot)
            {
                return 1f;
            }

            return Mathf.Clamp01(definition.LootDropChance);
        }

        /// <summary>
        /// True if the base loot drops for a uniform roll in [0, 1] (e.g. <c>Random.value</c>). Same
        /// comparison <see cref="EnemyLootGranter"/> always used (<c>roll &gt; chance</c> means no drop),
        /// so a guaranteed boss drop (chance 1) succeeds for every possible roll, including 1.
        /// </summary>
        public static bool ShouldDropLoot(EnemyDefinition definition, float roll01)
        {
            if (definition == null || definition.LootItem == null)
            {
                return false;
            }

            return roll01 <= GetEffectiveLootDropChance(definition);
        }

        /// <summary>Experience granted on kill: base reward, times the boss multiplier (never below 1x) for bosses.</summary>
        public static int GetExperienceReward(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            int baseReward = definition.ExperienceReward;
            if (!definition.IsBoss)
            {
                return baseReward;
            }

            float multiplier = Mathf.Max(1f, definition.BossExperienceMultiplier);
            return Mathf.RoundToInt(baseReward * multiplier);
        }

        /// <summary>True if this definition grants the extra procedurally rolled boss drop.</summary>
        public static bool HasBonusDrop(EnemyDefinition definition)
        {
            return definition != null && definition.IsBoss && definition.BossBonusDropItem != null;
        }

        /// <summary>
        /// Rolls the boss bonus drop, or returns null if this definition has none. Parameters mirror
        /// <see cref="ProceduralItemGenerator.Generate"/> (seed/pool are for tests and loot debugging).
        /// </summary>
        public static GeneratedItemInstance RollBonusDrop(
            EnemyDefinition definition,
            int? seed = null,
            Func<string, string> localizeName = null,
            IEnumerable<ItemAffixDefinition> affixPool = null)
        {
            if (!HasBonusDrop(definition))
            {
                return null;
            }

            return ProceduralItemGenerator.Generate(definition.BossBonusDropItem, definition.BossBonusDropRarity, seed, localizeName, affixPool);
        }
    }
}
