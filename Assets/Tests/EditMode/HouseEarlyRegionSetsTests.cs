using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Housing.Visuals;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities;
using Rapadura.UI.Housing;

namespace Rapadura.Tests
{
    /// <summary>
    /// The four furniture sets of Regions 1–4, the hub regions (Docs/Housing/CatalogoCasa.md), in the mould of
    /// <see cref="HouseMiddleRegionSetsTests"/>: each one is sold in its region's hub village at the catalog price (that village's
    /// counter keeps its older furniture too), has 8 pieces in the fixed rarity spread with the Epic as the week's special, its
    /// texts say it is furniture (not the equipment sets of <c>ArmorSetRules.Early</c>), and it costs less than the middle sets.
    /// </summary>
    public class HouseEarlyRegionSetsTests
    {
        private static readonly (string SetId, string ShopId, string CityId, string NamePt, string NameEn)[] Sets =
        {
            ("set_moveis_tear_seda", CityShopCatalog.CostureiraFatima, CityCatalog.VilaZefa, "Móveis do Tear de Seda", "Silk Loom Furniture"),
            ("set_moveis_cisterna_qocho", CityShopCatalog.SelariaVava, CityCatalog.AreiaBranca, "Móveis da Cisterna de Qocho", "Qocho Cistern Furniture"),
            ("set_moveis_rondavel", CityShopCatalog.ForjaTonho, CityCatalog.SerraAlta, "Móveis do Rondavel", "Rondavel Furniture"),
            ("set_moveis_casa_rocha", CityShopCatalog.LamparinasBenedita, CityCatalog.Lamparinas, "Móveis da Casa na Rocha", "House in the Rock Furniture"),
        };

        private static readonly string[] MiddleSets =
        {
            "set_moveis_porta_suaili", "set_moveis_casa_raizes", "set_moveis_terraco_pamir", "set_moveis_patio_nove_aneis", "set_moveis_tenda_feltro",
        };

        private static List<HouseItemDefinition> PiecesOf(string setId) => HouseCatalog.All.Where(i => i.SetId == setId).ToList();

        [Test]
        public void EachSet_IsKnown_AndHasEightPieces_OneCommon_ThreeUncommon_ThreeRare_OneEpic()
        {
            foreach ((string setId, _, _, _, _) in Sets)
            {
                Assert.IsNotNull(HouseSetCatalog.Get(setId), setId);
                List<HouseItemDefinition> pieces = PiecesOf(setId);
                Assert.AreEqual(8, pieces.Count, setId);
                Assert.AreEqual(1, pieces.Count(i => i.Rarity == ItemRarity.Common), setId);
                Assert.AreEqual(3, pieces.Count(i => i.Rarity == ItemRarity.Uncommon), setId);
                Assert.AreEqual(3, pieces.Count(i => i.Rarity == ItemRarity.Rare), setId);
                Assert.AreEqual(1, pieces.Count(i => i.Rarity == ItemRarity.Epic), setId);
                Assert.AreEqual(3, HouseSetCatalog.StageOf(pieces.Count, HouseSetCatalog.PieceCount(setId, HouseCatalog.All)), setId);
            }
        }

        [Test]
        public void EachSet_IsSoldOnlyInItsRegionsHub_AtTheCatalogPrice_WithTheEpicAsTheWeeksSpecial()
        {
            foreach ((string setId, string shopId, string cityId, _, _) in Sets)
            {
                CitySpec city = CityCatalog.Get(cityId);
                Assert.Contains(shopId, city.ShopIds.ToList(), cityId);
                Assert.IsTrue(CityCatalog.Cities.Contains(city), $"{cityId} is the region's hub, not a race city");
                CityShopSpec shop = CityShopCatalog.Get(shopId);
                Assert.Greater(shop.HouseSlots, 0, shopId);
                Assert.Greater(shop.WeeklySlots, 0, $"{shopId}: Epics are only ever on the weekly pool");

                foreach (HouseItemDefinition item in PiecesOf(setId))
                {
                    Assert.AreEqual(HouseItemSourceType.Shop, item.PrimarySource.Type, item.ItemId);
                    Assert.AreEqual(shopId, item.PrimarySource.RefId, item.ItemId);
                    CityShopEntry entry = CityShopCatalog.EntryOf(shopId, item.ItemId);
                    Assert.IsNotNull(entry, item.ItemId);
                    Assert.AreEqual(CityShopEntryKind.HouseItem, entry.Kind, item.ItemId);
                    Assert.AreEqual(item.BasePrice, entry.Price, item.ItemId);
                    Assert.AreEqual(item.Rarity == ItemRarity.Epic ? ShopPool.Weekly : ShopPool.House, entry.Pool, item.ItemId);

                    // No other counter sells it.
                    foreach (CityShopSpec other in CityShopCatalog.Shops.Where(s => s.ShopId != shopId))
                    {
                        Assert.IsNull(CityShopCatalog.EntryOf(other.ShopId, item.ItemId), $"{item.ItemId} also at {other.ShopId}");
                    }
                }
            }
        }

        [Test]
        public void TheQochoCounter_SellsOnlyItsOwnSet()
        {
            // The Selaria had no furniture before: its whole house pool is the cistern set.
            IEnumerable<string> sets = CityShopCatalog.Get(CityShopCatalog.SelariaVava).Entries.Where(e => e.Kind == CityShopEntryKind.HouseItem)
                .Select(e => HouseCatalog.Get(e.ItemId).SetId).Distinct();
            CollectionAssert.AreEquivalent(new[] { "set_moveis_cisterna_qocho" }, sets.ToList());
        }

        [Test]
        public void EachPiece_CostsLessThanTheSamePieceOfTheMiddleSets()
        {
            List<HouseItemDefinition> middle = MiddleSets.SelectMany(PiecesOf).ToList();
            Assert.AreEqual(40, middle.Count);
            foreach ((string setId, _, _, _, _) in Sets)
            {
                foreach (HouseItemDefinition item in PiecesOf(setId))
                {
                    int middleMin = middle.Where(i => i.Rarity == item.Rarity).Min(i => i.BasePrice);
                    Assert.Less(item.BasePrice, middleMin, item.ItemId);
                    Assert.LessOrEqual(item.Comfort, middle.Where(i => i.Rarity == item.Rarity).Max(i => i.Comfort), item.ItemId);
                    Assert.IsTrue(item.Sellable && item.Tradeable, item.ItemId);
                }
            }
        }

        [Test]
        public void EachSet_NamesItselfAsFurniture_InBothLanguages()
        {
            Dictionary<string, (string En, string Pt)> texts = HouseLifeTexts.Entries.ToDictionary(e => e.Key, e => (e.En, e.PtBR));
            foreach ((string setId, _, _, string namePt, string nameEn) in Sets)
            {
                HouseSetDefinition set = HouseSetCatalog.Get(setId);
                Assert.AreEqual(namePt, texts[set.NameKey].Pt, setId);
                Assert.AreEqual(nameEn, texts[set.NameKey].En, setId);
                StringAssert.StartsWith("Móveis ", texts[set.NameKey].Pt, setId);
                StringAssert.Contains("Furniture", texts[set.NameKey].En, setId);
                Assert.IsFalse(string.IsNullOrEmpty(texts[set.TitleKey].Pt), setId);
                Assert.IsFalse(string.IsNullOrEmpty(texts[set.TitleKey].En), setId);
            }
        }

        [Test]
        public void EveryPiece_HasAnEnglishName_AndBuildsAMesh()
        {
            foreach ((string setId, _, _, _, _) in Sets)
            {
                foreach (HouseItemDefinition item in PiecesOf(setId))
                {
                    Assert.IsTrue(HouseCatalog.HasEnglishName(item.ItemId), item.ItemId);
                    Assert.AreNotEqual(item.NamePtBR, HouseCatalog.EnglishName(item), item.ItemId);
                    var builder = new HouseMeshBuilder();
                    HouseFurnitureMeshes.Build(builder, item, HousePlacementRules.DefaultLayer(item));
                    Assert.Greater(builder.ToMesh("test").vertexCount, 0, item.ItemId);
                }
            }
        }
    }
}
