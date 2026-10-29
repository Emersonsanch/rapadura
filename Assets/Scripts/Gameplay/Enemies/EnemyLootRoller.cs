using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Enemies
{
    /// <summary>
    /// One thing an enemy dropped: either a stack of a plain item (<see cref="Item"/> x <see cref="Quantity"/>)
    /// or a single procedurally rolled piece of equipment (<see cref="Generated"/>, with rarity and affixes).
    /// </summary>
    public readonly struct LootDrop
    {
        public readonly ItemDefinition Item;
        public readonly int Quantity;
        public readonly GeneratedItemInstance Generated;

        public LootDrop(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
            Generated = null;
        }

        public LootDrop(GeneratedItemInstance generated)
        {
            Item = generated?.BaseItem;
            Quantity = 1;
            Generated = generated;
        }

        public bool IsGenerated => Generated != null;
    }

    /// <summary>
    /// Pure loot rules for one enemy death, kept free of MonoBehaviour/EventBus/UnityEngine.Random so
    /// EditMode tests can drive them with an injected <see cref="System.Random"/>. Used by
    /// <see cref="EnemyLootGranter"/>. Three independent sources, in this order (fixed, so a seed always
    /// reproduces the same drops):
    /// <list type="number">
    /// <item><b>Base loot</b> — <see cref="EnemyDefinition.LootItem"/> (gold today) at
    /// <see cref="EnemyDefinition.LootDropChance"/>, guaranteed for bosses (<see cref="BossBonusRules"/>).</item>
    /// <item><b>Biome table</b> — the enemy's biome (<see cref="BiomeRegistry.FindBiomeOf"/>) rolls its
    /// <see cref="BiomeDefinition.Loot"/> table <see cref="BiomeDefinition.LootRollsPerKill"/> times at
    /// <see cref="BiomeDefinition.LootDropChance"/> each; a boss rolls it
    /// <see cref="BiomeDefinition.BossLootRolls"/> times, every roll guaranteed. This is where the crafting
    /// materials and quest items (wood, fiber, herb, iron ore...) come from. Equipment entries become one
    /// <see cref="ProceduralItemGenerator"/> piece with a rarity from <see cref="CommonRarityWeights"/> /
    /// <see cref="BossRarityWeights"/> (never below the base item's own rarity).</item>
    /// <item><b>Boss bonus</b> — <see cref="EnemyDefinition.BossBonusDropItem"/> at its rarity
    /// (<see cref="BossBonusRules.RollBonusDrop"/>).</item>
    /// </list>
    /// Numbers and the expected fights per collect quest live in Docs/Balance/README.md §8.
    /// </summary>
    public static class EnemyLootRoller
    {
        /// <summary>Rarity weights for equipment rolled off a common enemy's biome table.</summary>
        public static readonly IReadOnlyList<(ItemRarity rarity, int weight)> CommonRarityWeights = new[]
        {
            (ItemRarity.Common, 65),
            (ItemRarity.Uncommon, 28),
            (ItemRarity.Rare, 7),
        };

        /// <summary>Rarity weights for equipment rolled off a boss's biome table (bosses drop better gear).</summary>
        public static readonly IReadOnlyList<(ItemRarity rarity, int weight)> BossRarityWeights = new[]
        {
            (ItemRarity.Uncommon, 45),
            (ItemRarity.Rare, 40),
            (ItemRarity.Epic, 15),
        };

        /// <summary>
        /// Rolls every drop of one death of <paramref name="enemy"/>. <paramref name="biome"/> may be null
        /// (only base loot and the boss bonus then). <paramref name="affixPool"/>/<paramref name="localizeName"/>
        /// are forwarded to <see cref="ProceduralItemGenerator.Generate"/> (tests pass an explicit pool).
        /// </summary>
        public static List<LootDrop> Roll(
            EnemyDefinition enemy,
            BiomeDefinition biome,
            Random random,
            IEnumerable<ItemAffixDefinition> affixPool = null,
            Func<string, string> localizeName = null)
        {
            var drops = new List<LootDrop>();
            if (enemy == null || random == null)
            {
                return drops;
            }

            // 1. Base loot (the definition's own item).
            if (BossBonusRules.ShouldDropLoot(enemy, (float)random.NextDouble()))
            {
                int quantity = random.Next(enemy.LootQuantityMin, Math.Max(enemy.LootQuantityMin, enemy.LootQuantityMax) + 1);
                if (quantity > 0)
                {
                    drops.Add(new LootDrop(enemy.LootItem, quantity));
                }
            }

            // 2. Biome loot table.
            if (biome != null)
            {
                int rolls = GetBiomeRolls(enemy, biome);
                float chance = GetBiomeRollChance(enemy, biome);
                for (int i = 0; i < rolls; i++)
                {
                    if (random.NextDouble() >= chance)
                    {
                        continue;
                    }

                    BiomeLootEntry entry = biome.RollLootEntry(random);
                    if (entry?.Item == null)
                    {
                        continue;
                    }

                    if (IsRolledAsEquipment(entry.Item))
                    {
                        ItemRarity rarity = RollEquipmentRarity(entry.Item, enemy.IsBoss, random);
                        drops.Add(new LootDrop(ProceduralItemGenerator.Generate(entry.Item, rarity, random.Next(), localizeName, affixPool)));
                    }
                    else
                    {
                        drops.Add(new LootDrop(entry.Item, random.Next(entry.QuantityMin, entry.QuantityMax + 1)));
                    }
                }
            }

            // 3. Boss bonus drop.
            if (BossBonusRules.HasBonusDrop(enemy))
            {
                drops.Add(new LootDrop(BossBonusRules.RollBonusDrop(enemy, random.Next(), localizeName, affixPool)));
            }

            return drops;
        }

        /// <summary>How many times <paramref name="enemy"/>'s death rolls <paramref name="biome"/>'s table.</summary>
        public static int GetBiomeRolls(EnemyDefinition enemy, BiomeDefinition biome)
        {
            if (enemy == null || biome == null)
            {
                return 0;
            }

            return enemy.IsBoss ? biome.BossLootRolls : biome.LootRollsPerKill;
        }

        /// <summary>Success chance of each of those rolls (1 for bosses).</summary>
        public static float GetBiomeRollChance(EnemyDefinition enemy, BiomeDefinition biome)
        {
            if (enemy == null || biome == null)
            {
                return 0f;
            }

            return enemy.IsBoss ? 1f : biome.LootDropChance;
        }

        /// <summary>
        /// Expected amount of <paramref name="itemId"/> one death of <paramref name="enemy"/> yields from
        /// its biome table (equipment counts 1 per piece). Used by the balance doc and tests.
        /// </summary>
        public static float ExpectedBiomeQuantity(EnemyDefinition enemy, BiomeDefinition biome, string itemId)
        {
            if (enemy == null || biome == null || string.IsNullOrEmpty(itemId))
            {
                return 0f;
            }

            int totalWeight = 0;
            float weightedQuantity = 0f;
            foreach (BiomeLootEntry entry in biome.Loot)
            {
                if (entry?.Item == null || entry.Weight <= 0)
                {
                    continue;
                }

                totalWeight += entry.Weight;
                if (entry.Item.ItemId == itemId)
                {
                    float average = IsRolledAsEquipment(entry.Item) ? 1f : (entry.QuantityMin + entry.QuantityMax) * 0.5f;
                    weightedQuantity += entry.Weight * average;
                }
            }

            if (totalWeight <= 0)
            {
                return 0f;
            }

            return GetBiomeRolls(enemy, biome) * GetBiomeRollChance(enemy, biome) * weightedQuantity / totalWeight;
        }

        /// <summary>True for items that drop as a procedural piece (equippable and non-stackable).</summary>
        public static bool IsRolledAsEquipment(ItemDefinition item)
        {
            return item != null && item.IsEquippable && !item.IsStackable;
        }

        /// <summary>Rolls a rarity from the common/boss weights, raised to the base item's own rarity if that is higher.</summary>
        public static ItemRarity RollEquipmentRarity(ItemDefinition item, bool isBoss, Random random)
        {
            IReadOnlyList<(ItemRarity rarity, int weight)> table = isBoss ? BossRarityWeights : CommonRarityWeights;
            int total = 0;
            foreach ((ItemRarity _, int weight) in table)
            {
                total += Math.Max(0, weight);
            }

            ItemRarity rolled = table[0].rarity;
            int roll = random.Next(total);
            int cumulative = 0;
            foreach ((ItemRarity rarity, int weight) in table)
            {
                cumulative += Math.Max(0, weight);
                if (roll < cumulative)
                {
                    rolled = rarity;
                    break;
                }
            }

            ItemRarity floor = item != null ? item.Rarity : ItemRarity.Common;
            return rolled < floor ? floor : rolled;
        }
    }
}
