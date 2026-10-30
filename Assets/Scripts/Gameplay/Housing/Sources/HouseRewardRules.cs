using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// H3 quest, daily and achievement house rewards (Housing.md §7.3, §7.4), read from the catalog's "Origem" column plus
    /// the extras the doc lists that are not an item's primary origin (<see cref="ExtraQuestRewards"/>). Every one of them is
    /// paid once per save (<see cref="HouseSourceBook"/>), on top of the quest's existing reward — nothing is taken away.
    /// Dailies: each completed daily gives a "Caixinha da Vó" (a random shop item of the house's city, Comum 80 /
    /// Incomum 18 / Raro 2) and four dailies give a fixed item the first time.
    /// </summary>
    public static class HouseRewardRules
    {
        /// <summary>§7.3 "Missões regionais": the two Floresta items whose primary origin is a shop/drop in the catalog.</summary>
        public static readonly IReadOnlyDictionary<string, string[]> ExtraQuestRewards = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            { "quest_region_forest_howling_trail", new[] { "casa_regional_011" } },
            { "quest_region_forest_spider_webs", new[] { "casa_regional_005" } },
        };

        public static readonly IReadOnlyList<(ItemRarity Rarity, int Weight)> DailyBoxRarityWeights = new[]
        {
            (ItemRarity.Common, 80), (ItemRarity.Uncommon, 18), (ItemRarity.Rare, 2),
        };

        /// <summary>The house's city region offline (the Vila da Vó Edna, Floresta): the shops the daily box draws from.</summary>
        public const string DailyBoxRegion = "forest";

        /// <summary>House items a quest (regional, personal, convergence) adds to its reward.</summary>
        public static List<string> QuestRewards(string questId, IEnumerable<HouseItemDefinition> catalog = null)
        {
            var rewards = (catalog ?? HouseCatalog.All)
                .Where(i => i?.PrimarySource != null && i.PrimarySource.Type == HouseItemSourceType.Quest && i.PrimarySource.RefId == questId)
                .Select(i => i.ItemId).ToList();
            if (questId != null && ExtraQuestRewards.TryGetValue(questId, out string[] extra))
            {
                foreach (string id in extra)
                {
                    if (!rewards.Contains(id)) rewards.Add(id);
                }
            }

            return rewards;
        }

        /// <summary>The fixed item a daily gives the first time it is completed (four dailies have one).</summary>
        public static List<string> DailyFirstRewards(string questId, IEnumerable<HouseItemDefinition> catalog = null)
        {
            return (catalog ?? HouseCatalog.All)
                .Where(i => i?.PrimarySource != null && i.PrimarySource.Type == HouseItemSourceType.DailyQuest && i.PrimarySource.RefId == questId)
                .Select(i => i.ItemId).ToList();
        }

        public static List<string> AchievementRewards(string achievementId, IEnumerable<HouseItemDefinition> catalog = null)
        {
            return (catalog ?? HouseCatalog.All)
                .Where(i => i?.PrimarySource != null && i.PrimarySource.Type == HouseItemSourceType.Achievement && i.PrimarySource.RefId == achievementId)
                .Select(i => i.ItemId).ToList();
        }

        /// <summary>"Caixinha da Vó" group: shop items of the city's shops, up to Rare, never seasonal.</summary>
        public static List<(HouseItemDefinition Item, int Weight)> DailyBoxPool(IEnumerable<HouseItemDefinition> catalog = null)
        {
            return (catalog ?? HouseCatalog.All)
                .Where(i => i?.PrimarySource != null && i.PrimarySource.Type == HouseItemSourceType.Shop && i.Seasonal == HouseSeason.None
                            && i.Rarity <= ItemRarity.Rare && HouseDropRules.ShopIn(i.PrimarySource.RefId, DailyBoxRegion))
                .Select(i => (i, 1)).ToList();
        }

        public static string RollDailyBox(Random random, IEnumerable<HouseItemDefinition> catalog = null)
        {
            if (random == null) return null;
            ItemRarity rarity = HouseDropRules.PickRarity(DailyBoxRarityWeights, random);
            return HouseDropRules.PickFromPool(DailyBoxPool(catalog), rarity, random)?.ItemId;
        }
    }
}
