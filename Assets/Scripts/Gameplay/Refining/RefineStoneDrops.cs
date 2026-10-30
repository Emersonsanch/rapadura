using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Refining
{
    /// <summary>One refine item's drop numbers per region (index = <see cref="BiomeType"/>).</summary>
    public sealed class RefineDropSpec
    {
        public readonly string ItemId;
        /// <summary>Chance per common kill.</summary>
        public readonly float[] CommonChance;
        /// <summary>Chance per boss kill.</summary>
        public readonly float[] BossChance;
        /// <summary>Units a boss drops when its roll succeeds.</summary>
        public readonly int[] BossQuantity;

        public RefineDropSpec(string itemId, float[] commonChance, float[] bossChance, int[] bossQuantity)
        {
            ItemId = itemId;
            CommonChance = commonChance;
            BossChance = bossChance;
            BossQuantity = bossQuantity;
        }
    }

    /// <summary>
    /// Rare drops of the "Pedras do Sertão" (Docs/Balance/README.md "Refino de item", fontes das pedras): a fourth loot source on top of
    /// <see cref="EnemyLootRoller"/>, rolled by <see cref="EnemyLootGranter"/> after it so the existing seeds keep
    /// reproducing the same base/biome/boss drops. One roll per item, in <see cref="RefineItems.All"/> order. The
    /// Bênção is the common one (bosses always drop it); Alma from the Deserto on; Vida and Caos only from the Serra.
    /// A Nova Jornada+ cycle raises the common-kill chances by <see cref="NewGamePlusBonusPerCycle"/> each.
    /// Online: the server rolls this table, like the rest of the loot.
    /// </summary>
    public static class RefineStoneDrops
    {
        public const float NewGamePlusBonusPerCycle = 0.25f;

        //                                                               Floresta Deserto Montanha Cavernas
        public static readonly IReadOnlyList<RefineDropSpec> Table = new[]
        {
            new RefineDropSpec(RefineItems.BlessingStone, new[] { 0.06f, 0.08f, 0.09f, 0.10f }, new[] { 1f, 1f, 1f, 1f }, new[] { 2, 2, 3, 3 }),
            new RefineDropSpec(RefineItems.SoulStone, new[] { 0f, 0.015f, 0.025f, 0.04f }, new[] { 0f, 1f, 1f, 1f }, new[] { 0, 1, 1, 2 }),
            new RefineDropSpec(RefineItems.LifeStone, new[] { 0f, 0f, 0.01f, 0.015f }, new[] { 0f, 0.25f, 0.35f, 0.5f }, new[] { 0, 1, 1, 1 }),
            new RefineDropSpec(RefineItems.ChaosStone, new[] { 0f, 0f, 0f, 0.01f }, new[] { 0f, 0f, 0.15f, 0.25f }, new[] { 0, 0, 1, 1 }),
            new RefineDropSpec(RefineItems.LuckCharm, new[] { 0f, 0.01f, 0.01f, 0.015f }, new[] { 0.1f, 0.15f, 0.2f, 0.25f }, new[] { 1, 1, 1, 1 }),
        };

        public static RefineDropSpec Get(string itemId)
        {
            foreach (RefineDropSpec spec in Table)
            {
                if (spec.ItemId == itemId)
                {
                    return spec;
                }
            }

            return null;
        }

        /// <summary>Chance of <paramref name="spec"/> on one kill (NG+ raises commons only; clamped to 1).</summary>
        public static float ChanceOf(RefineDropSpec spec, BiomeType region, bool isBoss, int newGamePlusCycle = 0)
        {
            if (spec == null)
            {
                return 0f;
            }

            int index = (int)region;
            if (index < 0 || index >= spec.CommonChance.Length)
            {
                return 0f;
            }

            if (isBoss)
            {
                return spec.BossQuantity[index] > 0 ? spec.BossChance[index] : 0f;
            }

            float bonus = 1f + NewGamePlusBonusPerCycle * NewGamePlusRules.ClampCycle(newGamePlusCycle);
            return Math.Min(1f, spec.CommonChance[index] * bonus);
        }

        /// <summary>Units of <paramref name="itemId"/> expected from one kill (docs/tests).</summary>
        public static float Expected(string itemId, BiomeType region, bool isBoss, int newGamePlusCycle = 0)
        {
            RefineDropSpec spec = Get(itemId);
            if (spec == null)
            {
                return 0f;
            }

            float quantity = isBoss ? spec.BossQuantity[(int)region] : 1f;
            return ChanceOf(spec, region, isBoss, newGamePlusCycle) * quantity;
        }

        /// <summary>
        /// Rolls the refine items of one death in <paramref name="region"/> (null = no region: nothing drops).
        /// Always draws one number per table row, so a seed reproduces the same drops. Items missing from
        /// <c>Resources/Items</c> are skipped.
        /// </summary>
        public static List<LootDrop> Roll(bool isBoss, BiomeType? region, Random random, int newGamePlusCycle = 0,
            Func<string, ItemDefinition> resolveItem = null)
        {
            var drops = new List<LootDrop>();
            if (random == null || !region.HasValue)
            {
                return drops;
            }

            resolveItem = resolveItem ?? ItemDatabase.GetById;
            foreach (RefineDropSpec spec in Table)
            {
                double roll = random.NextDouble();
                float chance = ChanceOf(spec, region.Value, isBoss, newGamePlusCycle);
                if (roll >= chance)
                {
                    continue;
                }

                ItemDefinition item = resolveItem(spec.ItemId);
                int quantity = isBoss ? spec.BossQuantity[(int)region.Value] : 1;
                if (item != null && quantity > 0)
                {
                    drops.Add(new LootDrop(item, quantity));
                }
            }

            return drops;
        }
    }
}
