using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>
    /// "Partes dos bichos" (FASE 16, crafting avançado; Docs/Balance/README.md "Crafting avançado"): stackable materials
    /// (<c>Resources/Items</c>, 1×1) that feed the weapon enchantments (<see cref="ImbueCatalog"/>), the runes and the
    /// recipes with quality (<see cref="AdvancedRecipeCatalog"/>). One part per region, the Aranha's silk as the Floresta's
    /// second, and the Troféu da Fera that only bosses drop (the quality catalyst of recipes and of the fusion forge).
    /// Names and descriptions live in <see cref="CraftingTexts"/> (<c>item.&lt;id&gt;.*</c>).
    /// </summary>
    public static class CreatureParts
    {
        public const string WolfFang = "item_parte_presa_lobo";
        public const string SpiderSilk = "item_parte_seda_aranha";
        public const string ScorpionStinger = "item_parte_ferrao_escorpiao";
        public const string GolemShard = "item_parte_lasca_golem";
        public const string BatWing = "item_parte_asa_morcego";
        public const string BeastTrophy = "item_parte_trofeu_fera";

        /// <summary>Every part, in drop-roll order.</summary>
        public static readonly string[] All = { WolfFang, SpiderSilk, ScorpionStinger, GolemShard, BatWing, BeastTrophy };

        /// <summary>The part every common of <paramref name="region"/> drops unless it has its own (<see cref="OwnPartByEnemy"/>).</summary>
        public static string RegionPart(BiomeType region)
        {
            switch (region)
            {
                case BiomeType.Forest: return WolfFang;
                case BiomeType.Desert: return ScorpionStinger;
                case BiomeType.Mountain: return GolemShard;
                case BiomeType.Caves: return BatWing;
                default: return null;
            }
        }

        /// <summary>Enemies (asset name) whose part is not their region's default.</summary>
        public static readonly IReadOnlyDictionary<string, string> OwnPartByEnemy = new Dictionary<string, string>
        {
            { "enemy_forest_spider", SpiderSilk },
        };

        public static bool IsPart(string itemId) => Array.IndexOf(All, itemId) >= 0;

        /// <summary>The part an enemy drops: its own one, else its region's (null without a region).</summary>
        public static string PartOf(string enemyId, BiomeType? region)
        {
            if (!string.IsNullOrEmpty(enemyId) && OwnPartByEnemy.TryGetValue(enemyId, out string own))
            {
                return own;
            }

            return region.HasValue ? RegionPart(region.Value) : null;
        }
    }

    /// <summary>
    /// The fifth loot source (after <see cref="EnemyLootRoller"/>, the hunting extra rolls, the stamina scaling and
    /// <c>RefineStoneDrops</c>): rolled last by <see cref="EnemyLootGranter"/> so every older seed reproduces the same drops.
    /// A common drops its part with <see cref="CommonChance"/>; a boss always drops <see cref="BossParts"/> of its region's
    /// part and rolls the Troféu da Fera (<see cref="TrophyChance"/>). Each Nova Jornada+ cycle adds
    /// <see cref="NewGamePlusBonusPerCycle"/> to the common chance. Always draws two numbers, so seeds stay stable.
    /// Online: the server rolls it with the rest of the loot.
    /// </summary>
    public static class CreaturePartDrops
    {
        public const float CommonChance = 0.30f;
        public const int BossParts = 3;
        public const float TrophyChance = 0.5f;
        /// <summary>A trophy also drops off an elite / event common with this chance (none today: reserved, kept at 0).</summary>
        public const float CommonTrophyChance = 0f;
        public const float NewGamePlusBonusPerCycle = 0.05f;

        public static float ChanceOfPart(bool isBoss, int newGamePlusCycle = 0)
        {
            return isBoss ? 1f : Math.Min(1f, CommonChance + NewGamePlusBonusPerCycle * NewGamePlusRules.ClampCycle(newGamePlusCycle));
        }

        /// <summary>Expected parts (any kind but the trophy) per kill — docs/tests.</summary>
        public static float ExpectedParts(bool isBoss, int newGamePlusCycle = 0) => ChanceOfPart(isBoss, newGamePlusCycle) * (isBoss ? BossParts : 1);

        /// <summary>Rolls the parts of one death (<paramref name="enemyId"/> = the EnemyDefinition asset name). No region: nothing.</summary>
        public static List<LootDrop> Roll(string enemyId, bool isBoss, BiomeType? region, Random random, int newGamePlusCycle = 0,
            Func<string, ItemDefinition> resolveItem = null)
        {
            var drops = new List<LootDrop>();
            if (random == null || !region.HasValue)
            {
                return drops;
            }

            resolveItem = resolveItem ?? ItemDatabase.GetById;
            double partRoll = random.NextDouble();
            double trophyRoll = random.NextDouble();

            string partId = CreatureParts.PartOf(enemyId, region);
            if (partId != null && partRoll < ChanceOfPart(isBoss, newGamePlusCycle))
            {
                ItemDefinition part = resolveItem(partId);
                if (part != null)
                {
                    drops.Add(new LootDrop(part, isBoss ? BossParts : 1));
                }
            }

            if (trophyRoll < (isBoss ? TrophyChance : CommonTrophyChance))
            {
                ItemDefinition trophy = resolveItem(CreatureParts.BeastTrophy);
                if (trophy != null)
                {
                    drops.Add(new LootDrop(trophy, 1));
                }
            }

            return drops;
        }
    }
}
