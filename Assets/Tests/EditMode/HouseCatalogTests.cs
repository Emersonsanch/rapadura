using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;

namespace Rapadura.Tests
{
    /// <summary>
    /// H0 data (Housing.md §12.1, §13): the 580-item house catalog generated from <c>Docs/Housing/CatalogoCasa.md</c>
    /// — unique ids and names, valid rarities and prices, every origin pointing at something that exists, the
    /// localization keys, and the importer being deterministic / idempotent (the committed table is up to date).
    /// </summary>
    public class HouseCatalogTests
    {
        private static string ProjectRoot => Directory.GetParent(UnityEngine.Application.dataPath).FullName;

        private static string ReadProjectFile(string relative) => File.ReadAllText(Path.Combine(ProjectRoot, relative), Encoding.UTF8);

        private static string Normalize(string name)
        {
            var sb = new StringBuilder();
            foreach (char c in name.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(c)) sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString();
        }

        [Test]
        public void Catalog_Has580Items_WithTheDocumentedRarities()
        {
            Assert.AreEqual(580, HouseCatalog.All.Count);
            Assert.AreEqual(580, HouseCatalog.GeneratedItemCount);
            var byRarity = HouseCatalog.All.GroupBy(i => i.Rarity).ToDictionary(g => g.Key, g => g.Count());
            Assert.AreEqual(225, byRarity[ItemRarity.Common]);
            Assert.AreEqual(149, byRarity[ItemRarity.Uncommon]);
            Assert.AreEqual(126, byRarity[ItemRarity.Rare]);
            Assert.AreEqual(64, byRarity[ItemRarity.Epic]);
            Assert.AreEqual(16, byRarity[ItemRarity.Legendary]);
        }

        [Test]
        public void Ids_AreUnique_AndWellFormed()
        {
            var ids = new HashSet<string>();
            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                Assert.IsTrue(ids.Add(item.ItemId), $"duplicate id {item.ItemId}");
                StringAssert.IsMatch(@"^casa_[a-z]+_\d{3}$", item.ItemId);
                StringAssert.StartsWith("casa_" + HouseCatalogImport.CategoryToken(item.Category) + "_", item.ItemId, "id prefix matches the category");
                Assert.AreSame(item, HouseCatalog.Get(item.ItemId));
            }
        }

        [Test]
        public void Names_AreUnique_IgnoringAccentsCaseAndPunctuation()
        {
            var names = new Dictionary<string, string>();
            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(item.NamePtBR), item.ItemId);
                string key = Normalize(item.NamePtBR);
                Assert.IsFalse(names.ContainsKey(key), $"{item.ItemId} '{item.NamePtBR}' duplicates {(names.TryGetValue(key, out string other) ? other : "")}");
                names[key] = item.ItemId;
            }
        }

        [Test]
        public void Prices_FallInTheirRarityBand_AndLegendariesAreNeverSold()
        {
            var bands = new Dictionary<ItemRarity, (int Min, int Max)>
            {
                { ItemRarity.Common, (8, 30) }, { ItemRarity.Uncommon, (35, 90) }, { ItemRarity.Rare, (100, 250) }, { ItemRarity.Epic, (300, 700) },
            };

            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                if (item.Rarity == ItemRarity.Legendary)
                {
                    Assert.AreEqual(0, item.BasePrice, item.ItemId);
                    Assert.IsFalse(item.Sellable, item.ItemId);
                    continue;
                }

                if (item.PrimarySource.Type == HouseItemSourceType.Shop)
                {
                    (int min, int max) = bands[item.Rarity];
                    Assert.That(item.BasePrice, Is.InRange(min, max), item.ItemId);
                }
                else
                {
                    Assert.AreEqual(0, item.BasePrice, $"{item.ItemId}: only shop items have a gold price");
                }

                if (item.PrimarySource.Type == HouseItemSourceType.Event)
                {
                    Assert.Greater(item.EventPrice, 0, item.ItemId);
                    Assert.AreNotEqual(HouseSeason.None, item.Seasonal, item.ItemId);
                }
            }
        }

        [Test]
        public void EveryOrigin_PointsAtSomethingThatExists()
        {
            var shops = new HashSet<string>
            {
                // Existing / city shops (CityShopCatalog) and the H2 shops still to come (Housing.md §8.1).
                "shop_general_store", "shop_mercearia_lazaro", "shop_armarinho_cida", "shop_feira_livre", "shop_forja_tonho",
                "shop_lamparinas_benedita", "shop_mascate_bento", "shop_marcenaria_damiao", "shop_costureira_fatima",
                "shop_viveiro_nenem", "shop_casa_musica_bene", "shop_olaria_rosa",
            };
            var houseQuests = new HashSet<string>
            {
                HouseRules.FirstQuestId, "house_quest_agua_fresca", "house_quest_fogo_cozinha", "house_quest_parede_barro",
                "house_quest_tijolo_por_tijolo", "house_quest_casa_cheia",
            };
            var events = new HashSet<string> { "event_festa_junina", "event_natal_sertanejo", "event_carnaval" };
            var stations = new HashSet<string> { "workbench", "forge" };

            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                Assert.IsNotEmpty(item.Sources, item.ItemId);
                foreach (HouseItemSource source in item.Sources)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(source.RefId), item.ItemId);
                    Assert.IsFalse(string.IsNullOrEmpty(source.Label), item.ItemId);
                    string where = $"{item.ItemId} → {source.Type} {source.RefId}";
                    switch (source.Type)
                    {
                        case HouseItemSourceType.Shop: Assert.IsTrue(shops.Contains(source.RefId), where); break;
                        case HouseItemSourceType.Drop:
                            Assert.IsTrue(File.Exists(Path.Combine(ProjectRoot, $"Assets/Resources/Enemies/{source.RefId}.asset")), where);
                            Assert.AreEqual(3, source.Weight, where);
                            break;
                        case HouseItemSourceType.Quest:
                            Assert.IsTrue(RegionQuestCatalog.IsRegionQuest(source.RefId) || File.Exists(Path.Combine(ProjectRoot, $"Assets/Resources/Quests/{source.RefId}.asset")), where);
                            break;
                        case HouseItemSourceType.DailyQuest:
                            Assert.IsTrue(File.Exists(Path.Combine(ProjectRoot, $"Assets/Resources/Quests/Daily/{source.RefId}.asset")), where);
                            break;
                        case HouseItemSourceType.Achievement:
                            Assert.IsTrue(File.Exists(Path.Combine(ProjectRoot, $"Assets/Resources/Achievements/{source.RefId}.asset")), where);
                            Assert.IsTrue(item.AccountBound && !item.Sellable && !item.Tradeable, $"{item.ItemId}: achievement items are bound to the account");
                            break;
                        case HouseItemSourceType.HouseQuest: Assert.IsTrue(houseQuests.Contains(source.RefId), where); break;
                        case HouseItemSourceType.Event: Assert.IsTrue(events.Contains(source.RefId), where); break;
                        case HouseItemSourceType.Craft: Assert.IsTrue(stations.Contains(source.RefId), where); break;
                        case HouseItemSourceType.Secret: StringAssert.StartsWith("secret_", source.RefId, where); break;
                    }
                }
            }
        }

        [Test]
        public void TheDocumentedOriginCounts_Hold()
        {
            int Count(params HouseItemSourceType[] types) => HouseCatalog.All.Count(i => types.Contains(i.PrimarySource.Type));
            Assert.AreEqual(376, Count(HouseItemSourceType.Shop));
            Assert.AreEqual(80, Count(HouseItemSourceType.Drop));
            Assert.AreEqual(48, Count(HouseItemSourceType.Event));
            Assert.AreEqual(34, Count(HouseItemSourceType.Quest, HouseItemSourceType.HouseQuest, HouseItemSourceType.DailyQuest));
            Assert.AreEqual(24, Count(HouseItemSourceType.Achievement));
            Assert.AreEqual(12, Count(HouseItemSourceType.Craft));
            Assert.AreEqual(6, Count(HouseItemSourceType.Secret));
        }

        [Test]
        public void EveryItem_HasALayer_AndFitsTheGridOfTheLevelItNeeds()
        {
            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                Assert.AreNotEqual(HousePlacement.None, item.Placement, item.ItemId);
                HouseLevelSpec spec = HouseLevels.Get(item.MinHouseLevel);
                HouseLayer layer = HousePlacementRules.DefaultLayer(item);
                (int w, int h) = HousePlacementRules.BaseFootprint(item, layer);
                switch (layer)
                {
                    case HouseLayer.Wall:
                        Assert.LessOrEqual(w, spec.InteriorDepth, item.ItemId);
                        Assert.LessOrEqual(h, HouseLevelSpec.WallRows, item.ItemId);
                        Assert.AreEqual(0, item.RotationStep, $"{item.ItemId}: wall pieces mirror, they do not turn");
                        break;
                    case HouseLayer.Yard:
                        Assert.LessOrEqual(System.Math.Max(w, h), spec.YardWidth, item.ItemId);
                        break;
                    case HouseLayer.Surface:
                        Assert.AreEqual(45, item.RotationStep, item.ItemId);
                        break;
                    default:
                        Assert.LessOrEqual(System.Math.Max(w, h), spec.InteriorDepth, item.ItemId);
                        Assert.AreEqual(90, item.RotationStep, item.ItemId);
                        break;
                }
            }
        }

        [Test]
        public void QuestRewardsAndStarterItems_ExistAndFitTheirLayers()
        {
            foreach (string id in HouseRules.FirstQuestRewards.Concat(HouseRules.StarterItems))
            {
                HouseItemDefinition item = HouseCatalog.Get(id);
                Assert.IsNotNull(item, id);
                Assert.AreEqual(1, item.MinHouseLevel, id);
            }

            Assert.AreEqual(HouseItemFunction.Chest, HouseCatalog.Get("casa_util_001").Function);
            Assert.AreEqual(0, HouseCatalog.Get("casa_util_001").FunctionParam, "the house chest itself adds no extra slots");
            Assert.AreEqual(HouseItemFunction.Bed, HouseCatalog.Get("casa_quarto_016").Function);
            Assert.AreEqual(HouseItemFunction.Wardrobe, HouseCatalog.Get("casa_util_006").Function);
            Assert.AreEqual(HouseItemFunction.PetBed, HouseCatalog.Get("casa_curral_001").Function);
            Assert.AreEqual(HouseItemFunction.GuestBook, HouseCatalog.Get("casa_util_023").Function);
            Assert.AreEqual(10, HouseCatalog.Get("casa_util_002").FunctionParam);
            Assert.Greater(HouseCatalog.Get("casa_sala_004").SurfaceSlots, 0, "the starter table offers surface spots");
            Assert.AreEqual(HouseItemSourceType.HouseQuest, HouseCatalog.Get("casa_util_001").PrimarySource.Type);
            Assert.AreEqual(HouseRules.FirstQuestId, HouseCatalog.Get("casa_util_001").PrimarySource.RefId);
        }

        // ------------------------------------------------------------------
        // Importer
        // ------------------------------------------------------------------

        [Test]
        public void Importer_IsDeterministic_AndTheCommittedTableIsUpToDate()
        {
            string markdown = ReadProjectFile(HouseCatalogImport.CatalogMarkdownPath);
            string englishPath = Path.Combine(ProjectRoot, HouseCatalogImport.EnglishNamesPath);
            Dictionary<string, string> english = File.Exists(englishPath) ? HouseCatalogImport.ParseEnglishNames(File.ReadAllText(englishPath, Encoding.UTF8)) : null;

            string first = HouseCatalogImport.GenerateCode(HouseCatalogImport.Parse(markdown), english);
            string second = HouseCatalogImport.GenerateCode(HouseCatalogImport.Parse(markdown), english);
            Assert.AreEqual(first, second, "same markdown → same code");
            Assert.IsFalse(first.Contains("\r"), "generated with \\n line endings");

            string committed = ReadProjectFile(HouseCatalogImport.GeneratedCodePath).Replace("\r\n", "\n");
            Assert.AreEqual(first, committed, "HouseCatalogData.g.cs is stale: run Rapadura ▸ Housing ▸ Import House Catalog");
        }

        [Test]
        public void Importer_ParsesEveryRowOfTheMarkdown()
        {
            string markdown = ReadProjectFile(HouseCatalogImport.CatalogMarkdownPath);
            int rows = markdown.Replace("\r\n", "\n").Split('\n').Count(line => line.TrimStart().StartsWith("| `casa_"));
            List<HouseItemDefinition> parsed = HouseCatalogImport.Parse(markdown);
            Assert.AreEqual(rows, parsed.Count);
            for (int i = 0; i < parsed.Count; i++)
            {
                Assert.AreEqual(HouseCatalog.All[i].ItemId, parsed[i].ItemId);
                Assert.AreEqual(HouseCatalog.All[i].NamePtBR, parsed[i].NamePtBR);
            }
        }

        [Test]
        public void Importer_RejectsMalformedRows()
        {
            const string header = "| ID | Nome | Raridade | Origem | Preço (ouro) | Conforto | Conjunto | Encaixe |\n|---|---|---|---|---|---|---|---|\n";
            Assert.Throws<System.FormatException>(() => HouseCatalogImport.Parse(header + "| `casa_sala_999` | X | Mítico | Loja: Bazar da Nana | 10 | 1 | — | Piso |\n"));
            Assert.Throws<System.FormatException>(() => HouseCatalogImport.Parse(header + "| `casa_sala_999` | X | Comum | Achado na rua | 10 | 1 | — | Piso |\n"));
            Assert.Throws<System.FormatException>(() => HouseCatalogImport.Parse(header + "| `casa_sala_999` | X | Comum | Loja: Bazar da Nana | 10 | 1 | — | Telhado |\n"));
            Assert.Throws<System.FormatException>(() => HouseCatalogImport.Parse(header + "| `casa_xyz_001` | X | Comum | Loja: Bazar da Nana | 10 | 1 | — | Piso |\n"));

            HouseItemDefinition ok = HouseCatalogImport.Parse(header + "| `casa_sazonal_999` | Y | Raro | Evento: Festa das Lanternas | — (40 oferendas) | 5 | Festa das Lanternas | Quintal |\n").Single();
            Assert.AreEqual(40, ok.EventPrice);
            Assert.AreEqual(HouseSeason.Carnaval, ok.Seasonal);
            Assert.AreEqual("set_frevo_maracatu", ok.SetId);
            Assert.AreEqual(HousePlacement.Yard, ok.Placement);
        }

        [Test]
        public void Importer_ShopIds_FollowTheCityShopConvention()
        {
            Assert.AreEqual("shop_general_store", HouseCatalogImport.ShopIdOf("Armazém Geral"));
            Assert.AreEqual("shop_armarinho_cida", HouseCatalogImport.ShopIdOf("Bazar de Linhas da Mariam"));
            Assert.AreEqual("shop_forja_tonho", HouseCatalogImport.ShopIdOf("Forja do Mestre Mihr"));
            Assert.AreEqual("shop_mascate_bento", HouseCatalogImport.ShopIdOf("Caravana do Ammo"));
            Assert.AreEqual("shop_viveiro_nenem", HouseCatalogImport.ShopIdOf("Viveiro da Nana"));
            Assert.AreEqual("shop_marcenaria_damiao", HouseCatalogImport.ShopIdOf("Marcenaria do Mestre Omóforo"));
            Assert.AreEqual("set_varanda_vo_zefa", HouseCatalogImport.SetIdOf("Iwan da Vó Edna"));
            Assert.AreEqual("set_arraia", HouseCatalogImport.SetIdOf("Bema"));
            Assert.AreEqual("set_cristal_sol", HouseCatalogImport.SetIdOf("Cristal do Sol"), "kept names still slug to their id");
            // The pre-re-theme labels keep resolving (the wardrobe catalog reuses these tables).
            Assert.AreEqual("shop_mascate_bento", HouseCatalogImport.ShopIdOf("Mascate Seu Bento"));
            Assert.AreEqual("set_varanda_vo_zefa", HouseCatalogImport.SetIdOf("Varanda da Vó Zefa"));
            Assert.AreEqual("enemy_forest_alpha_wolf", HouseCatalogImport.EnemyIds["Kök-Böri, o Lobo Celeste (chefe)"]);
            Assert.AreEqual("enemy_forest_alpha_wolf", HouseCatalogImport.EnemyIds["Lobo Alfa (chefe)"]);
            Assert.AreEqual(HouseSeason.Natal, HouseCatalogImport.Events["Noite de Yalda"].Season);
        }

        // ------------------------------------------------------------------
        // Localization
        // ------------------------------------------------------------------

        [Test]
        public void EveryItem_HasNameAndDescriptionKeys_InBothLanguages()
        {
            var entries = HouseItemTexts.Entries.ToDictionary(e => e.Key);
            Assert.AreEqual(HouseCatalog.All.Count * 2, entries.Count);
            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                Assert.IsTrue(entries.TryGetValue(item.NameKey, out var name), item.NameKey);
                Assert.AreEqual(item.NamePtBR, name.PtBR);
                Assert.IsFalse(string.IsNullOrEmpty(name.En), item.NameKey);
                Assert.IsTrue(entries.TryGetValue(item.DescKey, out var desc), item.DescKey);
                Assert.IsFalse(string.IsNullOrEmpty(desc.En) || string.IsNullOrEmpty(desc.PtBR), item.DescKey);
                Assert.IsFalse(desc.PtBR.Contains("`"), $"{item.DescKey}: designer ids stay out of the text");
            }

            Assert.AreEqual("Sofá de vime do iwan", HouseItemTexts.Get(null, "house.casa_sala_001.name"));
            Assert.AreEqual("Móveis de sala · Comum · Loja: Marcenaria do Mestre Omóforo", HouseItemTexts.Get(null, "house.casa_sala_001.desc"));
            StringAssert.StartsWith("Living room · Common · Shop: ", HouseItemTexts.Entries.First(e => e.Key == "house.casa_sala_001.desc").En);
        }
    }
}
