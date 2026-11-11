using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Housing.Visuals;
using Rapadura.Gameplay.Items;

namespace Rapadura.Tests
{
    /// <summary>
    /// The house trophy of each world boss of Regions 5–16 (<see cref="RegionBossRules.AllSpawns"/>,
    /// <see cref="HouseCraftRules.WorldBossTrophies"/>, CatalogoCasa.md <c>casa_trofeu_025</c>–<c>036</c>): a catalog wall or
    /// floor piece in the Épico/Lendário bands, forged at the house Forge with that boss's own keepsake once it is beaten,
    /// with a valid procedural stand-in and its name and description in both languages. Pure rules: no MonoBehaviour.
    /// </summary>
    public class WorldBossTrophyTests
    {
        private static IEnumerable<string> BossIds() => RegionBossRules.AllSpawns.Select(s => s.EnemyId);

        private static readonly HashSet<string> BasicMaterials = new HashSet<string>
        {
            HouseUpgradeRules.WoodId, HouseUpgradeRules.FiberId, HouseUpgradeRules.StoneId, HouseUpgradeRules.IronId,
        };

        private static HouseItemDefinition TrophyOf(string bossId)
        {
            Assert.IsTrue(HouseCraftRules.WorldBossTrophies.TryGetValue(bossId, out string trophyId), $"{bossId}: no house trophy");
            HouseItemDefinition item = HouseCatalog.Get(trophyId);
            Assert.IsNotNull(item, trophyId);
            return item;
        }

        [Test]
        public void EveryWorldBoss_HasItsOwnTrophy()
        {
            CollectionAssert.AreEquivalent(BossIds().ToList(), HouseCraftRules.WorldBossTrophies.Keys.ToList());
            Assert.AreEqual(HouseCraftRules.WorldBossTrophies.Count, HouseCraftRules.WorldBossTrophies.Values.Distinct().Count(), "one trophy per boss");
            Assert.IsFalse(HouseCraftRules.WorldBossTrophies.Values.Any(HouseDropRules.BossTrophies.Values.Contains), "not one of the old region bosses' plaques");
            Assert.AreEqual(HouseCraftRules.WorldBossTrophies.Count, HouseCraftRules.BossTrophyRecipes.Count());
        }

        [TestCaseSource(nameof(BossIds))]
        public void Trophy_IsAWallOrFloorPiece_InTheEpicOrLegendaryBand(string bossId)
        {
            HouseItemDefinition item = TrophyOf(bossId);
            Assert.AreEqual(HouseItemCategory.Trophy, item.Category, item.ItemId);
            HouseLayer layer = HousePlacementRules.DefaultLayer(item);
            Assert.That(layer, Is.EqualTo(HouseLayer.Wall).Or.EqualTo(HouseLayer.Floor), item.ItemId);
            Assert.AreEqual(1, item.MinHouseLevel, $"{item.ItemId}: fits the first house");

            switch (item.Rarity)
            {
                case ItemRarity.Epic: Assert.That(item.Comfort, Is.InRange(8, 9), item.ItemId); break;
                case ItemRarity.Legendary: Assert.That(item.Comfort, Is.InRange(12, 15), item.ItemId); break;
                default: Assert.Fail($"{item.ItemId}: a world boss trophy is Épico or Lendário, not {item.Rarity}"); break;
            }

            Assert.AreEqual(0, item.BasePrice, item.ItemId);
            Assert.IsFalse(item.Sellable, $"{item.ItemId}: trophies are keepsakes, never sold");
        }

        [TestCaseSource(nameof(BossIds))]
        public void Trophy_IsForged_WithTheBossKeepsake_AfterBeatingIt(string bossId)
        {
            HouseItemDefinition item = TrophyOf(bossId);
            Assert.AreEqual(HouseItemSourceType.Craft, item.PrimarySource.Type, item.ItemId);
            Assert.AreEqual("forge", item.PrimarySource.RefId, item.ItemId);

            HouseCraftRecipe recipe = HouseCraftRules.ForResult(item.ItemId);
            Assert.IsNotNull(recipe, $"{item.ItemId}: no recipe");
            Assert.AreEqual(HouseCraftResultKind.HouseItem, recipe.Kind, recipe.Id);
            Assert.AreEqual(HouseItemFunction.Forge, recipe.Station, recipe.Id);
            Assert.AreEqual(bossId, recipe.RequiredBossId, recipe.Id);

            string keepsake = HouseCraftRules.WorldBossKeepsakes[bossId];
            Assert.IsTrue(recipe.Costs.Any(c => c.ItemId == keepsake && c.Quantity > 0), $"{recipe.Id}: spends {keepsake}");
            foreach (HouseMaterialCost cost in recipe.Costs)
            {
                Assert.Greater(cost.Quantity, 0, recipe.Id);
                Assert.IsTrue(cost.ItemId == keepsake || BasicMaterials.Contains(cost.ItemId), $"{recipe.Id}: {cost.ItemId} is not a biome material or this boss's keepsake");
            }

            // The station is a real catalog piece the player can get (the Forja de quintal, from a shop).
            HouseItemDefinition forge = HouseCatalog.All.FirstOrDefault(i => i.Function == HouseItemFunction.Forge);
            Assert.IsNotNull(forge, "a Forge station in the catalog");
            Assert.AreEqual(HouseItemSourceType.Shop, forge.PrimarySource.Type, forge.ItemId);

            bool Forge(HouseItemFunction f, string id) => f == HouseItemFunction.Forge;
            int Plenty(string id) => 99;
            Assert.AreEqual(HouseCraftBlock.None, HouseCraftRules.Check(recipe, true, Forge, b => b == bossId, Plenty));
            Assert.AreEqual(HouseCraftBlock.BossNeeded, HouseCraftRules.Check(recipe, true, Forge, b => false, Plenty));
            Assert.AreEqual(HouseCraftBlock.NoStation, HouseCraftRules.Check(recipe, true, (f, id) => f == HouseItemFunction.Bench, b => true, Plenty));
            Assert.AreEqual(HouseCraftBlock.MissingMaterials, HouseCraftRules.Check(recipe, true, Forge, b => true, id => id == keepsake ? 0 : 99));
        }

        [TestCaseSource(nameof(BossIds))]
        public void Trophy_HasAValidMesh(string bossId)
        {
            HouseItemDefinition item = TrophyOf(bossId);
            var builder = new HouseMeshBuilder();
            HouseFurnitureMeshes.Build(builder, item, HousePlacementRules.DefaultLayer(item));
            Assert.Greater(builder.TriangleCount, 0, item.ItemId);
            Assert.LessOrEqual(builder.TriangleCount, 300, item.ItemId);
            Assert.Greater(builder.ToMesh("test_" + item.ItemId).vertexCount, 0, item.ItemId);
        }

        [TestCaseSource(nameof(BossIds))]
        public void Trophy_IsNamedAndDescribed_InBothLanguages(string bossId)
        {
            HouseItemDefinition item = TrophyOf(bossId);
            Assert.IsTrue(HouseCatalog.HasEnglishName(item.ItemId), item.ItemId);
            Assert.AreNotEqual(item.NamePtBR, HouseCatalog.EnglishName(item), item.ItemId);

            var entries = HouseItemTexts.Entries.ToDictionary(e => e.Key);
            Assert.IsTrue(entries.TryGetValue(item.NameKey, out var name), item.NameKey);
            Assert.AreEqual(item.NamePtBR, name.PtBR, item.NameKey);
            Assert.AreEqual(HouseCatalog.EnglishName(item), name.En, item.NameKey);
            Assert.IsTrue(entries.TryGetValue(item.DescKey, out var desc), item.DescKey);
            Assert.IsFalse(string.IsNullOrEmpty(desc.PtBR) || string.IsNullOrEmpty(desc.En), item.DescKey);
            StringAssert.Contains("Crafting: Forja", desc.PtBR, item.DescKey);
        }
    }
}
