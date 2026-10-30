using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;

namespace Rapadura.Gameplay.Daily
{
    /// <summary>
    /// The fôlego on a kill's loot (the one loot choke point, called by <c>EnemyLootGranter</c> right after
    /// <see cref="EnemyLootRoller.Roll"/>). Only plain stacks scale — gold, crafting materials, quest items — with
    /// <see cref="DailyStaminaRules.ScaleLootQuantity"/>; a stack that rounds to 0 is dropped. Generated equipment
    /// (biome gear and the boss bonus) is never multiplied nor removed: the fôlego changes how fast a child earns
    /// coins and materials, not how strong the gear gets (the power budget, Docs/Balance/README.md §14, stays put).
    /// ×1 returns the list untouched and draws no random numbers, so seeded tests keep their drops.
    /// </summary>
    public static class StaminaLootRules
    {
        public static List<LootDrop> Apply(List<LootDrop> drops, float multiplier, Random random)
        {
            if (drops == null || drops.Count == 0 || Math.Abs(multiplier - 1f) < 1e-6f)
            {
                return drops;
            }

            var scaled = new List<LootDrop>(drops.Count);
            foreach (LootDrop drop in drops)
            {
                if (drop.IsGenerated || drop.Item == null)
                {
                    scaled.Add(drop);
                    continue;
                }

                int quantity = DailyStaminaRules.ScaleLootQuantity(drop.Quantity, multiplier, random != null ? random.NextDouble() : 0.5);
                if (quantity > 0)
                {
                    scaled.Add(new LootDrop(drop.Item, quantity));
                }
            }

            return scaled;
        }
    }
}
