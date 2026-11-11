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
    /// The five furniture sets of Regions 5–9 (Docs/Mapas100.md, the road of the middle story; Docs/Housing/CatalogoCasa.md), in
    /// the mould of <see cref="HouseFarRegionSetsTests"/>: each one is sold in its region's capital at the catalog price, has 8
    /// pieces in the fixed rarity spread with the Epic as the week's special, its texts say it is furniture (not the equipment
    /// sets of <c>ArmorSetRules.Middle</c>), and it costs less than the far sets.
    /// </summary>
    public class HouseMiddleRegionSetsTests
    {
        private static readonly (string SetId, string ShopId, string CityId, string NamePt, string NameEn)[] Sets =
        {
            ("set_moveis_porta_suaili", CityShopCatalog.BazarMwana, CityCatalog.TaaPwani, "Móveis da Porta Suaíli", "Swahili Door Furniture"),
            ("set_moveis_casa_raizes", CityShopCatalog.TrocasNuhzadag, CityCatalog.Jhilpur, "Móveis da Casa nas Raízes", "House in the Roots Furniture"),
            ("set_moveis_terraco_pamir", CityShopCatalog.AgasalhosSisinnios, CityCatalog.Sangkent, "Móveis do Terraço do Pamir", "Pamir Terrace Furniture"),
            ("set_moveis_patio_nove_aneis", CityShopCatalog.LanternasMozak, CityCatalog.Mingcheng, "Móveis do Pátio dos Nove Anéis", "Nine Rings Courtyard Furniture"),
            ("set_moveis_tenda_feltro", CityShopCatalog.SelariaNomade, CityCatalog.AcampamentoSoslan, "Móveis da Tenda de Feltro", "Felt Tent Furniture"),
        };

        private static readonly string[] FarSets =
        {
            "set_moveis_casa_lua_branca", "set_moveis_cais_papiro", "set_moveis_tijolo_azul", "set_moveis_mil_lampadas", "set_moveis_cais_luz",
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
        public void EachSet_IsSoldInItsRegionsCapital_AtTheCatalogPrice_WithTheEpicAsTheWeeksSpecial()
        {
            foreach ((string setId, string shopId, string cityId, _, _) in Sets)
            {
                Assert.Contains(shopId, CityCatalog.Get(cityId).ShopIds.ToList(), cityId);
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
                }

                // Only this region's set on this counter: the capital sells no furniture of another set.
                IEnumerable<string> sets = shop.Entries.Where(e => e.Kind == CityShopEntryKind.HouseItem)
                    .Select(e => HouseCatalog.Get(e.ItemId).SetId).Distinct();
                CollectionAssert.AreEquivalent(new[] { setId }, sets.ToList(), shopId);
            }
        }

        [Test]
        public void EachPiece_CostsNoMoreThanTheSamePieceOfTheFarSets()
        {
            List<HouseItemDefinition> far = FarSets.SelectMany(PiecesOf).ToList();
            foreach ((string setId, _, _, _, _) in Sets)
            {
                foreach (HouseItemDefinition item in PiecesOf(setId))
                {
                    int farMin = far.Where(i => i.Rarity == item.Rarity).Min(i => i.BasePrice);
                    Assert.LessOrEqual(item.BasePrice, farMin, item.ItemId);
                    Assert.LessOrEqual(item.Comfort, far.Where(i => i.Rarity == item.Rarity).Max(i => i.Comfort), item.ItemId);
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
