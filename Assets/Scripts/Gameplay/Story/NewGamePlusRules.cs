using System;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Story
{
    /// <summary>
    /// Pure numbers of "Nova Jornada+" (New Game+), kept free of MonoBehaviour/EventBus/UnityEngine so
    /// EditMode tests cover them directly. Cycle 0 is the first campaign; each Nova Jornada+ adds one,
    /// up to <see cref="MaxCycle"/> (later journeys stay at the cap). Every multiplier is exactly 1 at
    /// cycle 0, so a save that never started a new journey plays exactly as before.
    ///
    /// Enemy HP and damage scale <b>by region</b> (r9, Balance §11/§14.4): the hero keeps the level of the
    /// end of the campaign (~16) and reaches ~22 during the journey, so a flat multiplier made the Floresta
    /// trivial and the Cavernas a slog. Now each region gets a one-off bonus from NG+1 on (the biggest in the
    /// Floresta, none in the Cavernas) plus a small step per cycle that is the same everywhere.
    ///
    /// Used by <c>EnemyController</c> (HP and damage of every enemy, bosses included, by the enemy's region),
    /// <c>EnemyRewardGranter</c> (kill XP, after the boss multiplier) and <see cref="Enemies.EnemyLootRoller"/>
    /// (base loot quantity — gold — and a chance to raise each rolled equipment piece one rarity tier).
    /// Quest XP/rewards are not scaled. Numbers and reasoning: Docs/Balance/README.md §11 (checked by the model in §14).
    /// </summary>
    public static class NewGamePlusRules
    {
        /// <summary>Highest cycle; NG+5 and later journeys all use the NG+5 numbers.</summary>
        public const int MaxCycle = 5;

        /// <summary>
        /// One-off extra enemy HP from NG+1 on, per region: the catch-up for the level the hero carries over (~16 against
        /// the 3 / 7 / 10 / 15 the first campaign expects at each boss). NG+1: Floresta ×2,2, Deserto ×1,5,
        /// Montanha ×1,35, Cavernas ×1,15.
        /// </summary>
        public const float ForestHealthBonus = 1.05f;
        public const float DesertHealthBonus = 0.35f;
        public const float MountainHealthBonus = 0.2f;
        public const float CavesHealthBonus = 0f;

        /// <summary>Extra enemy HP per cycle, every region (on top of the region bonus).</summary>
        public const float HealthPerCycle = 0.15f;

        /// <summary>One-off extra enemy damage from NG+1 on, per region (NG+1: ×2,7 / ×1,8 / ×1,6 / ×1,45).</summary>
        public const float ForestDamageBonus = 1.45f;
        public const float DesertDamageBonus = 0.55f;
        public const float MountainDamageBonus = 0.35f;
        public const float CavesDamageBonus = 0.2f;

        /// <summary>Extra enemy damage per hit per cycle, every region (on top of the region bonus).</summary>
        public const float DamagePerCycle = 0.25f;

        /// <summary>An enemy no biome spawn table lists (no region) scales like the Deserto, the middle of the curve.</summary>
        public const BiomeType RegionlessScaling = BiomeType.Desert;

        /// <summary>Extra kill XP per cycle (NG+1 ×1,5 ... NG+5 ×3,5).</summary>
        public const float ExperiencePerCycle = 0.5f;

        /// <summary>Extra base loot quantity (the definition's own item: gold) per cycle (NG+1 ×1,25 ... NG+5 ×2,25).</summary>
        public const float LootQuantityPerCycle = 0.25f;

        /// <summary>Chance per cycle that a rolled equipment piece goes up one rarity tier (NG+1 10% ... NG+5 50%).</summary>
        public const float RarityUpgradeChancePerCycle = 0.1f;

        /// <summary><paramref name="cycle"/> inside [0, <see cref="MaxCycle"/>].</summary>
        public static int ClampCycle(int cycle) => Math.Max(0, Math.Min(MaxCycle, cycle));

        /// <summary>The cycle a new journey started from <paramref name="cycle"/> plays (stays at the cap).</summary>
        public static int NextCycle(int cycle) => ClampCycle(ClampCycle(cycle) + 1);

        /// <summary>
        /// "Nova Jornada+" is offered once the campaign is complete and its ending was shown (so the
        /// epilogue and credits always play before the offer).
        /// </summary>
        public static bool CanOffer(bool campaignComplete, bool endingSeen) => campaignComplete && endingSeen;

        public static float HealthBonus(BiomeType? region)
        {
            switch (region ?? RegionlessScaling)
            {
                case BiomeType.Forest: return ForestHealthBonus;
                case BiomeType.Mountain: return MountainHealthBonus;
                case BiomeType.Caves: return CavesHealthBonus;
                default: return DesertHealthBonus;
            }
        }

        public static float DamageBonus(BiomeType? region)
        {
            switch (region ?? RegionlessScaling)
            {
                case BiomeType.Forest: return ForestDamageBonus;
                case BiomeType.Mountain: return MountainDamageBonus;
                case BiomeType.Caves: return CavesDamageBonus;
                default: return DesertDamageBonus;
            }
        }

        /// <summary>Enemy HP multiplier in <paramref name="region"/> (null = <see cref="RegionlessScaling"/>): 1 on cycle 0, else 1 + region bonus + 0,15 × cycle.</summary>
        public static float HealthMultiplier(int cycle, BiomeType? region = null)
        {
            int c = ClampCycle(cycle);
            return c == 0 ? 1f : 1f + HealthBonus(region) + HealthPerCycle * c;
        }

        /// <summary>Enemy damage multiplier in <paramref name="region"/> (null = <see cref="RegionlessScaling"/>): 1 on cycle 0, else 1 + region bonus + 0,25 × cycle.</summary>
        public static float DamageMultiplier(int cycle, BiomeType? region = null)
        {
            int c = ClampCycle(cycle);
            return c == 0 ? 1f : 1f + DamageBonus(region) + DamagePerCycle * c;
        }

        /// <summary>Lowest and highest enemy HP (or damage) multiplier over the four regions; the HUD shows the range.</summary>
        public static void MultiplierRange(int cycle, bool damage, out float min, out float max)
        {
            min = float.MaxValue;
            max = float.MinValue;
            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                float value = damage ? DamageMultiplier(cycle, region) : HealthMultiplier(cycle, region);
                min = Math.Min(min, value);
                max = Math.Max(max, value);
            }
        }

        public static float ExperienceMultiplier(int cycle) => 1f + ExperiencePerCycle * ClampCycle(cycle);

        public static float LootQuantityMultiplier(int cycle) => 1f + LootQuantityPerCycle * ClampCycle(cycle);

        public static float RarityUpgradeChance(int cycle) => RarityUpgradeChancePerCycle * ClampCycle(cycle);

        /// <summary>Max HP of an enemy whose definition says <paramref name="baseHealth"/> (rounded; cycle 0 returns it unchanged).</summary>
        public static float ScaleHealth(float baseHealth, int cycle, BiomeType? region = null)
        {
            return ClampCycle(cycle) == 0 ? baseHealth : (float)Math.Round(baseHealth * HealthMultiplier(cycle, region));
        }

        /// <summary>Damage per hit before the enemy's buffs/debuffs (one decimal kept; cycle 0 returns it unchanged).</summary>
        public static float ScaleDamage(float baseDamage, int cycle, BiomeType? region = null)
        {
            return ClampCycle(cycle) == 0 ? baseDamage : (float)Math.Round(baseDamage * DamageMultiplier(cycle, region), 1);
        }

        /// <summary>Kill XP (rounded, never below the base reward).</summary>
        public static int ScaleExperience(int baseExperience, int cycle)
        {
            if (ClampCycle(cycle) == 0 || baseExperience <= 0)
            {
                return baseExperience;
            }

            return Math.Max(baseExperience, (int)Math.Round(baseExperience * ExperienceMultiplier(cycle), MidpointRounding.AwayFromZero));
        }

        /// <summary>Base loot quantity (rounded, never below the rolled amount).</summary>
        public static int ScaleLootQuantity(int quantity, int cycle)
        {
            if (ClampCycle(cycle) == 0 || quantity <= 0)
            {
                return quantity;
            }

            return Math.Max(quantity, (int)Math.Round(quantity * LootQuantityMultiplier(cycle), MidpointRounding.AwayFromZero));
        }

        /// <summary>
        /// <paramref name="rarity"/> one tier up when <paramref name="roll01"/> (uniform in [0, 1)) falls under
        /// <see cref="RarityUpgradeChance"/>; never above <see cref="ItemRarity.Legendary"/>.
        /// </summary>
        public static ItemRarity UpgradeRarity(ItemRarity rarity, int cycle, double roll01)
        {
            if (rarity >= ItemRarity.Legendary || roll01 >= RarityUpgradeChance(cycle))
            {
                return rarity;
            }

            return rarity + 1;
        }
    }
}
