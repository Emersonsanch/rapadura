using System;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Story
{
    /// <summary>
    /// Pure numbers of "Nova Jornada+" (New Game+), kept free of MonoBehaviour/EventBus/UnityEngine so
    /// EditMode tests cover them directly. Cycle 0 is the first campaign; each Nova Jornada+ adds one,
    /// up to <see cref="MaxCycle"/> (later journeys stay at the cap). Every multiplier is exactly 1 at
    /// cycle 0, so a save that never started a new journey plays exactly as before.
    ///
    /// Used by <c>EnemyController</c> (HP and damage of every enemy, bosses included),
    /// <c>EnemyRewardGranter</c> (kill XP, after the boss multiplier) and <see cref="Enemies.EnemyLootRoller"/>
    /// (base loot quantity — gold — and a chance to raise each rolled equipment piece one rarity tier).
    /// Quest XP/rewards are not scaled. Numbers and reasoning: Docs/Balance/README.md §10.
    /// </summary>
    public static class NewGamePlusRules
    {
        /// <summary>Highest cycle; NG+5 and later journeys all use the NG+5 numbers.</summary>
        public const int MaxCycle = 5;

        /// <summary>Extra enemy HP per cycle (NG+1 ×1,8 ... NG+5 ×5).</summary>
        public const float HealthPerCycle = 0.8f;

        /// <summary>Extra enemy damage per hit per cycle (NG+1 ×1,5 ... NG+5 ×3,5).</summary>
        public const float DamagePerCycle = 0.5f;

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

        public static float HealthMultiplier(int cycle) => 1f + HealthPerCycle * ClampCycle(cycle);

        public static float DamageMultiplier(int cycle) => 1f + DamagePerCycle * ClampCycle(cycle);

        public static float ExperienceMultiplier(int cycle) => 1f + ExperiencePerCycle * ClampCycle(cycle);

        public static float LootQuantityMultiplier(int cycle) => 1f + LootQuantityPerCycle * ClampCycle(cycle);

        public static float RarityUpgradeChance(int cycle) => RarityUpgradeChancePerCycle * ClampCycle(cycle);

        /// <summary>Max HP of an enemy whose definition says <paramref name="baseHealth"/> (rounded; cycle 0 returns it unchanged).</summary>
        public static float ScaleHealth(float baseHealth, int cycle)
        {
            return ClampCycle(cycle) == 0 ? baseHealth : (float)Math.Round(baseHealth * HealthMultiplier(cycle));
        }

        /// <summary>Damage per hit before the enemy's buffs/debuffs (one decimal kept; cycle 0 returns it unchanged).</summary>
        public static float ScaleDamage(float baseDamage, int cycle)
        {
            return ClampCycle(cycle) == 0 ? baseDamage : (float)Math.Round(baseDamage * DamageMultiplier(cycle), 1);
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
