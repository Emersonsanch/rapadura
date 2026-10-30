using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>How good the materials of one craft are, from their average rarity (<see cref="CraftTierRules.QualityOf"/>).</summary>
    public enum MaterialQuality
    {
        /// <summary>Rústico: plain wood, fiber, herbs.</summary>
        Rustic = 0,
        /// <summary>Bom: some creature parts in the mix.</summary>
        Good = 1,
        /// <summary>Fino: mostly parts.</summary>
        Fine = 2,
        /// <summary>Primoroso: rare parts / a Troféu da Fera on top.</summary>
        Superb = 3,
    }

    /// <summary>
    /// Pure rules of the crafting tiers (FASE 16, "Crafting avançado"; Docs/Balance/README.md "Crafting avançado").
    /// <list type="bullet">
    /// <item>Tier = how far the journey went: 1 + the deepest region unlocked (Floresta 1 … Cavernas 4); every Nova
    /// Jornada+ cycle opens all four (<see cref="TierFor"/>). A recipe / enchantment of a higher tier stays locked.</item>
    /// <item>Material quality = the quantity-weighted average rarity of the ingredients (<see cref="QualityOf"/>), one step
    /// up with a Troféu da Fera as catalyst.</item>
    /// <item>Crafted gear rolls its rarity on <see cref="RarityTable"/> by quality, never below the base item and never
    /// above the tier's cap (<see cref="RarityCap"/>: Incomum, then Raro only with the Cavernas open). The cap is what the
    /// power budget leaves (<c>tools/balance/crafting_stack.py</c>: Raro from the Montanha already breaks it): forged
    /// pieces are a reliable floor, the best rolls still come from boss loot.</item>
    /// </list>
    /// Server-friendly: no Unity, the RNG is passed in.
    /// </summary>
    public static class CraftTierRules
    {
        public const int MinTier = 1;
        public const int MaxTier = 4;

        /// <summary>Highest rarity crafting or fusion can reach per tier (index 0 = tier 1). Parsed by tools/balance/crafting_stack.py.</summary>
        public static readonly ItemRarity[] RarityCaps = { ItemRarity.Uncommon, ItemRarity.Uncommon, ItemRarity.Uncommon, ItemRarity.Rare };

        /// <summary>Average rarity (inclusive lower bounds) for Bom / Fino / Primoroso.</summary>
        public const float GoodThreshold = 0.25f;
        public const float FineThreshold = 0.6f;
        public const float SuperbThreshold = 1.0f;

        /// <summary>Weights of Common / Uncommon / Rare / Epic per <see cref="MaterialQuality"/> (rows sum to 100).</summary>
        public static readonly int[][] RarityTable =
        {
            new[] { 60, 30, 10, 0 },  // Rústico
            new[] { 30, 45, 22, 3 },  // Bom
            new[] { 0, 45, 40, 15 },  // Fino (= a boss drop)
            new[] { 0, 20, 50, 30 },  // Primoroso
        };

        /// <summary>Tier opened by <paramref name="deepestRegionIndex"/> (BiomeType: Forest 0 … Caves 3; negative = none yet).</summary>
        public static int TierFor(int deepestRegionIndex, int newGamePlusCycle = 0)
        {
            if (newGamePlusCycle > 0)
            {
                return MaxTier;
            }

            return Math.Max(MinTier, Math.Min(MaxTier, deepestRegionIndex + 1));
        }

        public static bool IsUnlocked(int requiredTier, int currentTier) => requiredTier <= currentTier;

        public static ItemRarity RarityCap(int tier) => RarityCaps[Math.Max(MinTier, Math.Min(MaxTier, tier)) - 1];

        /// <summary>Quality from (rarity, quantity) pairs; <paramref name="withTrophy"/> adds one step (capped at Primoroso).</summary>
        public static MaterialQuality QualityOf(IEnumerable<(ItemRarity Rarity, int Quantity)> materials, bool withTrophy = false)
        {
            float weighted = 0f;
            int units = 0;
            if (materials != null)
            {
                foreach ((ItemRarity rarity, int quantity) in materials)
                {
                    if (quantity <= 0)
                    {
                        continue;
                    }

                    weighted += (int)rarity * quantity;
                    units += quantity;
                }
            }

            float average = units > 0 ? weighted / units : 0f;
            int step = average >= SuperbThreshold ? 3 : average >= FineThreshold ? 2 : average >= GoodThreshold ? 1 : 0;
            if (withTrophy)
            {
                step++;
            }

            return (MaterialQuality)Math.Min((int)MaterialQuality.Superb, step);
        }

        /// <summary>Chance (0..1) of each rarity Common..Epic for a craft, after the floor and the cap are applied.</summary>
        public static float[] RarityChances(MaterialQuality quality, int tier, ItemRarity floor = ItemRarity.Common)
        {
            int[] weights = RarityTable[(int)quality];
            int cap = (int)RarityCap(tier);
            int low = Math.Min((int)floor, cap);
            var chances = new float[weights.Length];
            float total = 0f;
            foreach (int w in weights)
            {
                total += w;
            }

            for (int i = 0; i < weights.Length; i++)
            {
                int landed = Math.Max(low, Math.Min(cap, i));
                chances[landed] += weights[i] / total;
            }

            return chances;
        }

        /// <summary>Rolls the rarity of one crafted piece (one draw of <paramref name="random"/>).</summary>
        public static ItemRarity RollRarity(MaterialQuality quality, int tier, Random random, ItemRarity floor = ItemRarity.Common)
        {
            float[] chances = RarityChances(quality, tier, floor);
            double roll = random != null ? random.NextDouble() : 0.0;
            double acc = 0.0;
            for (int i = 0; i < chances.Length; i++)
            {
                acc += chances[i];
                if (roll < acc - 1e-9)
                {
                    return (ItemRarity)i;
                }
            }

            for (int i = chances.Length - 1; i >= 0; i--)
            {
                if (chances[i] > 0f)
                {
                    return (ItemRarity)i;
                }
            }

            return floor;
        }
    }
}
