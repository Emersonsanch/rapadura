using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>
    /// Where the wardrobe entries come from (Housing.md §9.7, the catalog's "Origem"), pure and RNG-injected like
    /// <c>HouseDropRules</c>. The service (<see cref="WardrobeService"/>) feeds it the game events:
    /// <list type="bullet">
    /// <item><b>Starter:</b> in the Coleção from the start (<see cref="WardrobeBook"/>).</item>
    /// <item><b>Quest / daily / achievement / map secret:</b> the entries pointing at that id, paid once per save.</item>
    /// <item><b>Boss:</b> the first win against a boss guarantees one of its "Drop:" entries still missing; every later win
    /// has <see cref="BossChance"/> of one more.</item>
    /// <item><b>Common enemy:</b> <see cref="CommonChance"/> per death of an entry of that enemy or, in the Floresta and the
    /// Deserto, of the "inimigos comuns da Floresta e do Deserto" group.</item>
    /// <item><b>Loot:</b> every equipment piece looted unlocks its default look (§9.3, <see cref="WardrobeCatalog.DefaultLookFor"/>).</item>
    /// <item><b>Shops</b> (Costureira and others, <c>CityShopService.OutfitResolver</c>), <b>house quests</b> and <b>house
    /// crafting</b> (HouseService's owed list) are handled by the service.</item>
    /// </list>
    /// Events (Junina, Natal, Carnaval) and the Tear recipes have no source in the game yet: those entries stay locked.
    /// </summary>
    public static class WardrobeSourceRules
    {
        public const double BossChance = 0.35;
        public const double CommonChance = 0.03;

        public static IEnumerable<WardrobeEntry> Catalog(IEnumerable<WardrobeEntry> catalog) => catalog ?? WardrobeCatalog.All;

        public static List<string> QuestRewards(string questId, IEnumerable<WardrobeEntry> catalog = null)
        {
            return Catalog(catalog).Where(e => (e.Source == WardrobeSourceType.Quest || e.Source == WardrobeSourceType.DailyQuest) && e.SourceRef == questId)
                .Select(e => e.Id).ToList();
        }

        public static List<string> AchievementRewards(string achievementId, IEnumerable<WardrobeEntry> catalog = null)
        {
            return Catalog(catalog).Where(e => e.Source == WardrobeSourceType.Achievement && e.SourceRef == achievementId).Select(e => e.Id).ToList();
        }

        public static List<string> RelicRewards(string relicId, IEnumerable<WardrobeEntry> catalog = null)
        {
            return Catalog(catalog).Where(e => e.Source == WardrobeSourceType.Secret && e.SourceRef == relicId).Select(e => e.Id).ToList();
        }

        public static bool IsForestOrDesert(string enemyId)
        {
            return enemyId != null && (enemyId.StartsWith("enemy_forest_", StringComparison.Ordinal) || enemyId.StartsWith("enemy_desert_", StringComparison.Ordinal));
        }

        /// <summary>Drop group of an enemy: its own entries plus, for a common Floresta/Deserto enemy, the shared group.</summary>
        public static List<WardrobeEntry> DropPool(string enemyId, bool isBoss, IEnumerable<WardrobeEntry> catalog = null)
        {
            return Catalog(catalog).Where(e => e.Source == WardrobeSourceType.Drop
                                               && (e.SourceRef == enemyId || (!isBoss && e.SourceRef == WardrobeEntry.CommonDropRef && IsForestOrDesert(enemyId))))
                .ToList();
        }

        /// <summary>
        /// One enemy death: the entry id won, or null. Looks already in the Coleção are not drawn (a dye always can be);
        /// <paramref name="firstBossWin"/> skips the chance roll (the guaranteed first-win gift).
        /// </summary>
        public static string RollDrop(string enemyId, bool isBoss, bool firstBossWin, Func<string, bool> unlocked, Random random, IEnumerable<WardrobeEntry> catalog = null)
        {
            if (random == null || string.IsNullOrEmpty(enemyId)) return null;
            List<WardrobeEntry> pool = DropPool(enemyId, isBoss, catalog).Where(e => e.IsDye || unlocked == null || !unlocked(e.Id)).ToList();
            if (pool.Count == 0) return null;
            if (!(isBoss && firstBossWin) && random.NextDouble() >= (isBoss ? BossChance : CommonChance)) return null;
            return pool[random.Next(pool.Count)].Id;
        }

        public static AppearanceSlot? SlotOf(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Head: return AppearanceSlot.Head;
                case EquipmentSlot.Chest: return AppearanceSlot.Chest;
                case EquipmentSlot.Legs: return AppearanceSlot.Legs;
                case EquipmentSlot.Boots: return AppearanceSlot.Boots;
                default: return null;
            }
        }

        /// <summary>The look a looted equipment piece unlocks for <paramref name="cousin"/> (null when it has none).</summary>
        public static string LootLook(CharacterId cousin, EquipmentSlot slot, ItemRarity rarity)
        {
            AppearanceSlot? layer = SlotOf(slot);
            return layer.HasValue ? WardrobeCatalog.DefaultLookFor(cousin, layer.Value, rarity)?.Id : null;
        }

        public static string QuestKey(string questId) => "quest:" + questId;
        public static string AchievementKey(string achievementId) => "ach:" + achievementId;
        public static string RelicKey(string relicId) => "relic:" + relicId;
        public static string BossKey(string bossId) => "boss:" + bossId;
    }
}
