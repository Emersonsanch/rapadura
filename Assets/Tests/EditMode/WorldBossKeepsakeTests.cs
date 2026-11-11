using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Wardrobe;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Each world boss of Regions 5–16 (<see cref="RegionBossRules.AllSpawns"/>) drops its own keepsake (the asset's
    /// <c>LootItem</c>, <see cref="HouseCraftRules.WorldBossKeepsakes"/>), named in both languages, and every keepsake is
    /// spent by at least one house recipe (the boss pieces of the Loom and the Forge). Pure data: no MonoBehaviour.
    /// </summary>
    public class WorldBossKeepsakeTests
    {
        private static IEnumerable<string> BossIds() => RegionBossRules.AllSpawns.Select(s => s.EnemyId);

        private static IEnumerable<string> KeepsakeIds() => HouseCraftRules.WorldBossKeepsakes.Values;

        private static EnemyDefinition LoadBoss(string id)
        {
            EnemyDefinition boss = Resources.Load<EnemyDefinition>("Enemies/" + id);
            Assert.IsNotNull(boss, id);
            return boss;
        }

        private static LocalizationEntry Entry(string key) => ContentTexts.Entries.FirstOrDefault(e => e.Key == key);

        [Test]
        public void EveryWorldBoss_HasAKeepsake_AndEachIsItsOwn()
        {
            CollectionAssert.AreEquivalent(BossIds().ToList(), HouseCraftRules.WorldBossKeepsakes.Keys.ToList());
            Assert.AreEqual(HouseCraftRules.WorldBossKeepsakes.Count, KeepsakeIds().Distinct().Count(), "one keepsake per boss");
        }

        [TestCaseSource(nameof(BossIds))]
        public void Boss_DropsItsKeepsake_WithARealChance(string id)
        {
            EnemyDefinition boss = LoadBoss(id);
            ItemDefinition loot = boss.LootItem;
            Assert.IsNotNull(loot, id);
            Assert.AreEqual(HouseCraftRules.WorldBossKeepsakes[id], loot.ItemId, id);
            Assert.IsNotNull(ItemDatabase.GetById(loot.ItemId), $"{id}: the item is in Resources/Items");

            float chance = BossBonusRules.GetEffectiveLootDropChance(boss);
            Assert.Greater(chance, 0f, id);
            Assert.Less(chance, 1f, $"{id}: a keepsake is not guaranteed");
            Assert.GreaterOrEqual(boss.LootQuantityMin, 1, id);
            Assert.GreaterOrEqual(boss.LootQuantityMax, boss.LootQuantityMin, id);
        }

        [TestCaseSource(nameof(KeepsakeIds))]
        public void Keepsake_Exists_WithTextInBothLanguages_AndAnIcon(string itemId)
        {
            ItemDefinition item = ItemDatabase.GetById(itemId);
            Assert.IsNotNull(item, itemId);
            Assert.GreaterOrEqual(item.Rarity, ItemRarity.Rare, itemId);
            Assert.IsTrue(item.IsStackable, itemId);
            Assert.IsFalse(item.IsEquippable, itemId);
            Assert.Greater(item.Value, 0, $"{itemId}: sells at any shop");
            Assert.IsNotNull(item.Icon, itemId);

            LocalizationEntry name = Entry(ContentText.ItemNameKey(itemId));
            LocalizationEntry description = Entry(ContentText.ItemDescriptionKey(itemId));
            Assert.IsNotNull(name, itemId);
            Assert.IsNotNull(description, itemId);
            Assert.IsNotEmpty(name.En, itemId);
            Assert.IsNotEmpty(description.En, itemId);
            Assert.AreEqual(item.DisplayName, name.PtBR, $"{itemId}: pt-BR name matches the asset");
            Assert.AreEqual(item.Description, description.PtBR, $"{itemId}: pt-BR description matches the asset");
            Assert.AreEqual(name.PtBR, HouseSourceTexts.GetFallback(HouseSourceTexts.MaterialKey(itemId)), $"{itemId}: the workbench names it too");
        }

        [TestCaseSource(nameof(KeepsakeIds))]
        public void Keepsake_IsSpent_ByAHouseRecipe_ThatMakesSomethingReal(string itemId)
        {
            List<HouseCraftRecipe> uses = HouseCraftRules.UsesOf(itemId).ToList();
            Assert.IsNotEmpty(uses, $"{itemId}: no recipe uses it");
            foreach (HouseCraftRecipe recipe in uses)
            {
                Assert.IsTrue(recipe.Kind == HouseCraftResultKind.Wardrobe
                    ? WardrobeCatalog.Contains(recipe.ResultId)
                    : HouseCatalog.Get(recipe.ResultId) != null, recipe.Id);
                Assert.That(recipe.Station, Is.EqualTo(HouseItemFunction.Loom).Or.EqualTo(HouseItemFunction.Forge), recipe.Id);
            }
        }

        [Test]
        public void LoomRecipes_PayAllOrNothing_WithTheKeepsakes()
        {
            HouseCraftRecipe skirt = HouseCraftRules.ForResult("roupa_lavine_019");
            Assert.IsNotNull(skirt);
            var owned = new Dictionary<string, int> { { "item_fiber", 12 } };
            System.Func<HouseItemFunction, string, bool> loom = (f, id) => f == HouseItemFunction.Loom;
            Assert.AreEqual(HouseCraftBlock.MissingMaterials, HouseCraftRules.Check(skirt, true, loom, _ => true, id => owned.TryGetValue(id, out int n) ? n : 0));
            foreach (HouseMaterialCost cost in skirt.Costs)
            {
                owned[cost.ItemId] = cost.Quantity;
            }

            Assert.AreEqual(HouseCraftBlock.None, HouseCraftRules.Check(skirt, true, loom, _ => true, id => owned.TryGetValue(id, out int n) ? n : 0));
            Assert.AreEqual(HouseCraftBlock.NoStation, HouseCraftRules.Check(skirt, true, (f, id) => f == HouseItemFunction.Bench, _ => true, id => owned.TryGetValue(id, out int n) ? n : 0));
            Assert.AreEqual(HouseSourceTexts.StationLoom, HouseSourceTexts.StationKey(skirt));
            Assert.AreEqual(4, skirt.Costs.Count(c => HouseCraftRules.WorldBossKeepsakes.Values.Contains(c.ItemId)), "four boss keepsakes");
        }
    }
}
