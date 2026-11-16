using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Housing.Visuals;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities;
using Rapadura.UI.Housing;

namespace Rapadura.Tests
{
    /// <summary>
    /// The ten furniture sets that are the other "conjunto novo" of a ficha of Regions 5–12 (Docs/Mapas100.md 28, 33, 44, 42, 48, 53,
    /// 58, 66, 68 and 69; Docs/Housing/CatalogoCasa.md), in the mould of <see cref="HouseMiddleRegionSetsTests"/> and
    /// <see cref="HouseHighRegionSetsTests"/>: each one is sold in the ficha's return city at the catalog price (Kirta, Tarchon, Torres
    /// Brancas and the Vale dos Sátiros got their first furniture counter; Mingcheng, Soslan and Farnkesh sell it beside the region's first
    /// set; Hinokura, the only city of Região 12, sells three beside the Palácio de Laca), has 8 pieces in the fixed rarity spread with the
    /// Epic as the week's special, says it is furniture in both languages, and costs what the sets of its band cost.
    /// </summary>
    public class HouseFichaSetsTests
    {
        private static readonly (string SetId, int Ficha, int Region, string ShopId, string CityId, string NamePt, string NameEn)[] Sets =
        {
            ("set_moveis_jardim_coral", 28, 5, CityShopCatalog.MergulhoDanel, CityCatalog.Kirta, "Móveis do Jardim de Coral", "Coral Garden Furniture"),
            ("set_moveis_palafita", 33, 6, CityShopCatalog.CestariaLarthia, CityCatalog.PalafitasTarchon, "Móveis da Palafita", "Stilt House Furniture"),
            ("set_moveis_arrozal", 44, 8, CityShopCatalog.LanternasMozak, CityCatalog.Mingcheng, "Móveis do Arrozal", "Rice Terrace Furniture"),
            ("set_moveis_torre_branca", 42, 8, CityShopCatalog.EspelhosNatakamani, CityCatalog.TorresBrancas, "Móveis da Torre Branca", "White Spire Furniture"),
            ("set_moveis_ouro_estepe", 48, 9, CityShopCatalog.SelariaNomade, CityCatalog.AcampamentoSoslan, "Móveis do Ouro da Estepe", "Steppe Gold Furniture"),
            ("set_moveis_flauta_dupla", 53, 10, CityShopCatalog.FlautaDupla, CityCatalog.ValeDosSatiros, "Móveis da Flauta Dupla", "Double Flute Furniture"),
            ("set_moveis_ninho_ouro", 58, 10, CityShopCatalog.CordasSisin, CityCatalog.Farnkesh, "Móveis do Ninho de Ouro", "Golden Nest Furniture"),
            ("set_moveis_jardim_pedra", 66, 12, CityShopCatalog.FerroPretoKurogane, CityCatalog.Hinokura, "Móveis do Jardim de Pedra", "Stone Garden Furniture"),
            ("set_moveis_vidro_brasa", 68, 12, CityShopCatalog.FerroPretoKurogane, CityCatalog.Hinokura, "Móveis do Vidro de Brasa", "Ember Glass Furniture"),
            ("set_moveis_fornalha", 69, 12, CityShopCatalog.FerroPretoKurogane, CityCatalog.Hinokura, "Móveis da Fornalha", "Furnace Furniture"),
        };

        /// <summary>The region's first set already on the same counter (the capital is the ficha's return city).</summary>
        private static readonly Dictionary<string, string> FirstSetAtCounter = new Dictionary<string, string>
        {
            { CityShopCatalog.LanternasMozak, "set_moveis_patio_nove_aneis" },
            { CityShopCatalog.SelariaNomade, "set_moveis_tenda_feltro" },
            { CityShopCatalog.CordasSisin, "set_moveis_ninho_pedra" },
            { CityShopCatalog.FerroPretoKurogane, "set_moveis_palacio_laca" },
        };

        private static readonly string[] FarSets =
        {
            "set_moveis_casa_lua_branca", "set_moveis_cais_papiro", "set_moveis_tijolo_azul", "set_moveis_mil_lampadas", "set_moveis_cais_luz",
        };

        private static readonly string[] MiddleSets =
        {
            "set_moveis_porta_suaili", "set_moveis_casa_raizes", "set_moveis_terraco_pamir", "set_moveis_patio_nove_aneis", "set_moveis_tenda_feltro",
        };

        private static List<HouseItemDefinition> PiecesOf(string setId) => HouseCatalog.All.Where(i => i.SetId == setId).ToList();

        [Test]
        public void EachSet_IsKnown_AndHasEightPieces_OneCommon_ThreeUncommon_ThreeRare_OneEpic()
        {
            foreach ((string setId, _, _, _, _, _, _) in Sets)
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
        public void EachSet_IsTheConjuntoNovoOfItsFicha()
        {
            string fichas = File.ReadAllText(Path.Combine(Directory.GetParent(UnityEngine.Application.dataPath).FullName, "Docs/Mapas100.md"), Encoding.UTF8);
            string flat = Regex.Replace(fichas, @"\s+", " ");
            foreach ((string setId, int ficha, _, _, _, string namePt, _) in Sets)
            {
                string theme = namePt.Substring(namePt.IndexOf(' ', "Móveis ".Length) + 1);
                int at = flat.IndexOf($"### {ficha} · ", System.StringComparison.Ordinal);
                Assert.GreaterOrEqual(at, 0, $"ficha {ficha}");
                int next = flat.IndexOf("### ", at + 4, System.StringComparison.Ordinal);
                string body = flat.Substring(at, (next < 0 ? flat.Length : next) - at);
                StringAssert.Contains($"conjunto novo **{theme}**", body, setId);
            }
        }

        [Test]
        public void EachSet_IsSoldOnlyInItsFichasReturnCity_AtTheCatalogPrice_WithTheEpicAsTheWeeksSpecial()
        {
            foreach ((string setId, _, _, string shopId, string cityId, _, _) in Sets)
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
                    Assert.IsTrue(item.Sellable && item.Tradeable, item.ItemId);

                    foreach (CityShopSpec other in CityShopCatalog.Shops.Where(s => s.ShopId != shopId))
                    {
                        Assert.IsNull(CityShopCatalog.EntryOf(other.ShopId, item.ItemId), $"{item.ItemId} also at {other.ShopId}");
                    }
                }
            }
        }

        [Test]
        public void EachCounter_SellsItsSet_AndAtMostTheRegionsFirstSetBesideIt()
        {
            foreach ((string setId, _, _, string shopId, _, _, _) in Sets)
            {
                IEnumerable<string> sets = CityShopCatalog.Get(shopId).Entries.Where(e => e.Kind == CityShopEntryKind.HouseItem)
                    .Select(e => HouseCatalog.Get(e.ItemId).SetId).Distinct();
                // Its own set, the other ficha sets of the same counter (Hinokura sells three) and the region's first set, if any.
                List<string> expected = Sets.Where(s => s.ShopId == shopId).Select(s => s.SetId).ToList();
                if (FirstSetAtCounter.TryGetValue(shopId, out string first)) expected.Add(first);
                CollectionAssert.Contains(expected, setId, shopId);
                CollectionAssert.AreEquivalent(expected, sets.ToList(), shopId);
            }
        }

        [Test]
        public void TheHinokuraCounter_TurnsItsFourSets_WithMoreSlots_AndTwoEpicsAWeek()
        {
            CityShopSpec kurogane = CityShopCatalog.Get(CityShopCatalog.FerroPretoKurogane);
            List<CityShopEntry> house = kurogane.Entries.Where(e => e.Kind == CityShopEntryKind.HouseItem).ToList();
            Assert.AreEqual(32, house.Count, "4 sets of 8");
            Assert.AreEqual(4, house.Count(e => e.Pool == ShopPool.Weekly), "one Epic per set");
            Assert.Greater(kurogane.HouseSlots, CityShopCatalog.Get(CityShopCatalog.CordasSisin).HouseSlots, "more furniture than a two-set counter");
            Assert.AreEqual(2, kurogane.WeeklySlots);
            Assert.Greater(house.Count(e => e.Pool == ShopPool.House), kurogane.HouseSlots, "more furniture than slots: it rotates");
        }

        [Test]
        public void EachPiece_CostsWhatItsBandCosts()
        {
            List<HouseItemDefinition> middle = MiddleSets.SelectMany(PiecesOf).ToList();
            List<HouseItemDefinition> far = FarSets.SelectMany(PiecesOf).ToList();
            Assert.AreEqual(40, middle.Count);
            Assert.AreEqual(40, far.Count);
            foreach ((string setId, _, int region, _, _, _, _) in Sets)
            {
                foreach (HouseItemDefinition item in PiecesOf(setId))
                {
                    List<HouseItemDefinition> farSame = far.Where(i => i.Rarity == item.Rarity).ToList();
                    if (region <= 9)
                    {
                        // Regions 5–9 (HouseMiddleRegionSetsTests): no more than the cheapest far piece of the same rarity.
                        Assert.LessOrEqual(item.BasePrice, farSame.Min(i => i.BasePrice), item.ItemId);
                    }
                    else
                    {
                        // Regions 10–12 (HouseHighRegionSetsTests): above the middle sets, no more than the far ones.
                        Assert.Greater(item.BasePrice, middle.Where(i => i.Rarity == item.Rarity).Max(i => i.BasePrice), item.ItemId);
                        Assert.LessOrEqual(item.BasePrice, farSame.Max(i => i.BasePrice), item.ItemId);
                    }

                    Assert.LessOrEqual(item.Comfort, farSame.Max(i => i.Comfort), item.ItemId);
                }
            }
        }

        [Test]
        public void EachSet_NamesItselfAsFurniture_InBothLanguages()
        {
            Dictionary<string, (string En, string Pt)> texts = HouseLifeTexts.Entries.ToDictionary(e => e.Key, e => (e.En, e.PtBR));
            foreach ((string setId, _, _, _, _, string namePt, string nameEn) in Sets)
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
            foreach ((string setId, _, _, _, _, _, _) in Sets)
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
