using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// H3 house-item drops (Housing.md §7.2). One extra roll, separate from <c>EnemyLootRoller</c> (it never touches the
    /// material/gold/equipment tables) and from the bag: the result goes to the Depósito. Pure and RNG-injected, so the
    /// online server can make the very same decision.
    /// <list type="bullet">
    /// <item><b>Common enemy:</b> <see cref="CommonChance"/> per death (<see cref="NightChance"/> at night); rarity
    /// <see cref="CommonRarityWeights"/>; group = the catalog's "Drop: &lt;enemy&gt;" items (their weight, 3) + the shop
    /// items of the enemy's biome city up to Rare (weight 1). No item at the rolled rarity → one tier down until found
    /// (then up, so a roll is never wasted).</item>
    /// <item><b>Boss victory:</b> the first win gives its trophy plaque (<see cref="BossTrophies"/>); every win 1 roll +
    /// <see cref="BossSecondRollChance"/> of a second, rarity <see cref="BossRarityWeights"/>, from the boss's own
    /// "Drop:" group (the plaque excluded). Bad-luck protection: after <see cref="PityWins"/> wins in a row without a
    /// Legendary from that boss, the next roll is Legendary.</item>
    /// </list>
    /// </summary>
    public static class HouseDropRules
    {
        public const double CommonChance = 0.04;
        public const double NightChance = 0.05;
        public const double BossSecondRollChance = 0.25;
        public const int PityWins = 25;
        public const int ShopItemWeight = 1;
        public const ItemRarity ShopDropMaxRarity = ItemRarity.Rare;

        public static readonly IReadOnlyList<(ItemRarity Rarity, int Weight)> CommonRarityWeights = new[]
        {
            (ItemRarity.Common, 62), (ItemRarity.Uncommon, 28), (ItemRarity.Rare, 9), (ItemRarity.Epic, 1),
        };

        public static readonly IReadOnlyList<(ItemRarity Rarity, int Weight)> BossRarityWeights = new[]
        {
            (ItemRarity.Uncommon, 40), (ItemRarity.Rare, 37), (ItemRarity.Epic, 19), (ItemRarity.Legendary, 4),
        };

        /// <summary>The "lembrança" plaque of each region boss's first defeat (also the H4 casarão keepsakes).</summary>
        public static readonly IReadOnlyDictionary<string, string> BossTrophies = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "enemy_forest_alpha_wolf", "casa_trofeu_001" },
            { "enemy_desert_ruin_guardian", "casa_trofeu_007" },
            { "enemy_mountain_warlord", "casa_trofeu_013" },
            { "enemy_cave_dweller", "casa_trofeu_019" },
        };

        /// <summary>Region of each shop of the house catalog (§8.1). "*" = sold in every city (Armazém and its branches, Feira).</summary>
        public static readonly IReadOnlyDictionary<string, string> ShopRegions = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "shop_general_store", "*" }, { "shop_feira_livre", "*" },
            { "shop_marcenaria_damiao", "forest" }, { "shop_armarinho_cida", "forest" }, { "shop_mercearia_lazaro", "forest" },
            { "shop_costureira_fatima", "forest" }, { "shop_viveiro_nenem", "forest" }, { "shop_casa_musica_bene", "forest" },
            { "shop_olaria_rosa", "desert" }, { "shop_forja_tonho", "mountain" }, { "shop_lamparinas_benedita", "caves" },
            // The Mascate's curiosities never drop (§7.7: they are his stock).
        };

        public static bool IsBossWithTrophy(string enemyId) => enemyId != null && BossTrophies.ContainsKey(enemyId);

        /// <summary>"enemy_forest_wolf" → "forest"; "enemy_cave_dweller" → "caves". Null for an unknown id.</summary>
        public static string RegionOfEnemy(string enemyId)
        {
            if (string.IsNullOrEmpty(enemyId)) return null;
            if (enemyId.StartsWith("enemy_forest_", StringComparison.Ordinal)) return "forest";
            if (enemyId.StartsWith("enemy_desert_", StringComparison.Ordinal)) return "desert";
            if (enemyId.StartsWith("enemy_mountain_", StringComparison.Ordinal)) return "mountain";
            if (enemyId.StartsWith("enemy_cave_", StringComparison.Ordinal) || enemyId.StartsWith("enemy_caves_", StringComparison.Ordinal)) return "caves";
            return null;
        }

        public static bool ShopIn(string shopId, string region)
        {
            return shopId != null && region != null && ShopRegions.TryGetValue(shopId, out string where) && (where == "*" || where == region);
        }

        // ------------------------------------------------------------------
        // Groups
        // ------------------------------------------------------------------

        /// <summary>Common enemy group: its own "Drop:" items (their weight) + the biome city's shop items up to Rare.</summary>
        public static List<(HouseItemDefinition Item, int Weight)> CommonPool(string enemyId, IEnumerable<HouseItemDefinition> catalog = null)
        {
            var pool = new List<(HouseItemDefinition, int)>();
            string region = RegionOfEnemy(enemyId);
            foreach (HouseItemDefinition item in catalog ?? HouseCatalog.All)
            {
                HouseItemSource source = item?.PrimarySource;
                if (source == null || item.Seasonal != HouseSeason.None) continue;
                if (source.Type == HouseItemSourceType.Drop && source.RefId == enemyId) pool.Add((item, Math.Max(1, source.Weight)));
                else if (source.Type == HouseItemSourceType.Shop && item.Rarity <= ShopDropMaxRarity && ShopIn(source.RefId, region)) pool.Add((item, ShopItemWeight));
            }

            return pool;
        }

        /// <summary>Boss group: the boss's own "Drop:" items, its trophy plaque excluded (that one is the first-win gift).</summary>
        public static List<(HouseItemDefinition Item, int Weight)> BossPool(string bossId, IEnumerable<HouseItemDefinition> catalog = null)
        {
            BossTrophies.TryGetValue(bossId ?? string.Empty, out string trophy);
            return (catalog ?? HouseCatalog.All)
                .Where(i => i?.PrimarySource != null && i.PrimarySource.Type == HouseItemSourceType.Drop && i.PrimarySource.RefId == bossId && i.ItemId != trophy)
                .Select(i => (i, Math.Max(1, i.PrimarySource.Weight)))
                .ToList();
        }

        // ------------------------------------------------------------------
        // Rolls
        // ------------------------------------------------------------------

        public static ItemRarity PickRarity(IReadOnlyList<(ItemRarity Rarity, int Weight)> table, Random random)
        {
            int total = table.Sum(t => Math.Max(0, t.Weight));
            int roll = random.Next(Math.Max(1, total));
            int cumulative = 0;
            foreach ((ItemRarity rarity, int weight) in table)
            {
                cumulative += Math.Max(0, weight);
                if (roll < cumulative) return rarity;
            }

            return table[0].Rarity;
        }

        /// <summary>A weighted item of <paramref name="rarity"/>; none there → lower tiers first, then higher. Null for an empty pool.</summary>
        public static HouseItemDefinition PickFromPool(IReadOnlyList<(HouseItemDefinition Item, int Weight)> pool, ItemRarity rarity, Random random)
        {
            if (pool == null || pool.Count == 0) return null;
            foreach (ItemRarity tier in FallbackOrder(rarity))
            {
                List<(HouseItemDefinition Item, int Weight)> candidates = pool.Where(p => p.Item.Rarity == tier).ToList();
                if (candidates.Count == 0) continue;
                int total = candidates.Sum(c => c.Weight);
                int roll = random.Next(total);
                foreach ((HouseItemDefinition item, int weight) in candidates)
                {
                    if (roll < weight) return item;
                    roll -= weight;
                }
            }

            return null;
        }

        /// <summary>Rolled tier, then each lower tier, then each higher one.</summary>
        public static IEnumerable<ItemRarity> FallbackOrder(ItemRarity rarity)
        {
            for (int r = (int)rarity; r >= (int)ItemRarity.Common; r--) yield return (ItemRarity)r;
            for (int r = (int)rarity + 1; r <= (int)ItemRarity.Legendary; r++) yield return (ItemRarity)r;
        }

        /// <summary>One common-enemy death: the item id dropped, or null (no luck, or nothing to drop).</summary>
        public static string RollCommon(string enemyId, bool night, Random random, IEnumerable<HouseItemDefinition> catalog = null)
        {
            if (random == null || string.IsNullOrEmpty(enemyId)) return null;
            if (random.NextDouble() >= (night ? NightChance : CommonChance)) return null;
            ItemRarity rarity = PickRarity(CommonRarityWeights, random);
            return PickFromPool(CommonPool(enemyId, catalog), rarity, random)?.ItemId;
        }

        /// <summary>
        /// One boss victory: the trophy on the first win, then 1 roll + 25% of a second (pity forces a Legendary).
        /// Updates <paramref name="record"/> (wins, streak without Legendary, trophy given).
        /// </summary>
        public static List<string> RollBoss(string bossId, HouseBossRecord record, Random random, IEnumerable<HouseItemDefinition> catalog = null)
        {
            var drops = new List<string>();
            if (random == null || string.IsNullOrEmpty(bossId) || record == null) return drops;
            IReadOnlyCollection<HouseItemDefinition> items = (catalog ?? HouseCatalog.All).ToList();

            if (!record.trophyGiven && BossTrophies.TryGetValue(bossId, out string trophy) && items.Any(i => i.ItemId == trophy))
            {
                drops.Add(trophy);
                record.trophyGiven = true;
            }

            List<(HouseItemDefinition Item, int Weight)> pool = BossPool(bossId, items);
            int rolls = 1 + (random.NextDouble() < BossSecondRollChance ? 1 : 0);
            bool legendary = false;
            for (int i = 0; i < rolls; i++)
            {
                bool pity = !legendary && record.sinceLegendary >= PityWins;
                ItemRarity rarity = pity ? ItemRarity.Legendary : PickRarity(BossRarityWeights, random);
                HouseItemDefinition item = PickFromPool(pool, rarity, random);
                if (item == null) continue;
                drops.Add(item.ItemId);
                if (item.Rarity == ItemRarity.Legendary) legendary = true;
            }

            record.wins++;
            record.sinceLegendary = legendary ? 0 : record.sinceLegendary + 1;
            return drops;
        }
    }
}
