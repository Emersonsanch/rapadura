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
    /// The five furniture sets of the far regions (Docs/Mapas100.md, regions 12–16; Docs/Housing/CatalogoCasa.md): each one is
    /// sold in its region's city, has 8 pieces in a fixed rarity spread, and its texts say it is furniture, so it is not
    /// mistaken for the equipment sets of the same theme.
    /// </summary>
    public class HouseFarRegionSetsTests
    {
        private static readonly (string SetId, string ShopId, string CityId, string NamePt, string NameEn)[] Sets =
        {
            ("set_moveis_casa_lua_branca", CityShopCatalog.PelesPamour, CityCatalog.PortoDaLuaBranca, "Móveis da Casa da Lua Branca", "White Moon House Furniture"),
            ("set_moveis_cais_papiro", CityShopCatalog.TrocasKhunanup, CityCatalog.CaisDeMesektet, "Móveis do Cais do Papiro", "Papyrus Quay Furniture"),
            ("set_moveis_tijolo_azul", CityShopCatalog.BetumeAtrahasis, CityCatalog.DurJarras, "Móveis de Tijolo Azul", "Blue Brick Furniture"),
            ("set_moveis_mil_lampadas", CityShopCatalog.PaviosTehat, CityCatalog.FortalezaUltimaLampada, "Móveis das Mil Lâmpadas", "Thousand Lamps Furniture"),
            ("set_moveis_cais_luz", CityShopCatalog.UltimaCaravanaAmmo, CityCatalog.CaisDaColuna, "Móveis do Cais de Luz", "Quay of Light Furniture"),
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
        public void EachSet_IsSoldInItsRegionsCity_AtTheCatalogPrice_WithTheEpicAsTheWeeksSpecial()
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
            }
        }

        [Test]
        public void EachSet_NamesItselfAsFurniture_InBothLanguages()
        {
            Dictionary<string, LocalizationPair> texts = HouseLifeTexts.Entries.ToDictionary(e => e.Key, e => new LocalizationPair(e.En, e.PtBR));
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

        private readonly struct LocalizationPair
        {
            public readonly string En;
            public readonly string Pt;

            public LocalizationPair(string en, string pt)
            {
                En = en;
                Pt = pt;
            }
        }
    }
}
