using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.NPC.Visuals;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Copas de Garajonay as a walkable race city (Docs/Cities.md, Docs/Mapas100.md 05): its <see cref="CitySpec"/> outside
    /// the one-hub-per-region list, the five elves of the ficha at their doors with their own look, Dácil's rotating counter
    /// with the gear of the 05/06 gates, the two city quests, the respawn of its maps; and the lore cast of the first four
    /// cities (TODO.md "História e Arcos dos NPCs") standing in their towns.
    /// </summary>
    public class CityGarajonayTests
    {
        private static readonly string[] Elves = { "gara", "jonay", "dacil", "bencomo", "tinguaro" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void Garajonay_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec copas = CityCatalog.Get(CityCatalog.CopasGarajonay);
            Assert.IsNotNull(copas, "Copas de Garajonay has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, copas.Region);
            Assert.AreEqual(MapCatalog.CopasGarajonay, copas.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.GarajonayCityId, copas.CityId, "the recipe's city id");
            Assert.IsTrue(CityCatalog.IsRaceCity(copas));
            Assert.IsFalse(CityCatalog.Cities.Contains(copas), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.VilaZefa, CityCatalog.ForRegion(BiomeType.Forest).CityId, "the Forest hub is still the Vila");
            Assert.IsFalse(copas.HasGrandmaServices, "Baú and forno stay in Vó Edna's village");
            Assert.IsFalse(copas.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(copas.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(copas.CityId), "the city walk has a layout");
        }

        [Test]
        public void MapsOfGarajonay_RespawnInItsPlaza_TheVilaMapsStillInTheVila()
        {
            Assert.AreEqual(CityCatalog.CopasGarajonay, MapCatalog.RespawnCityFor("map_005_garajonay").CityId);
            Assert.AreEqual(CityCatalog.CopasGarajonay, MapCatalog.RespawnCityFor("map_006_mihrshah").CityId);
            Assert.AreEqual(CityCatalog.VilaZefa, MapCatalog.RespawnCityFor(MapCatalog.TugaiId).CityId);

            var cities = new CityProgress();
            CitySpec copas = CityCatalog.Get(CityCatalog.CopasGarajonay);
            Assert.AreSame(copas, cities.ApplyDefeatIn(copas), "a defeat on 05/06 wakes the cousins in the elves' plaza");
            Assert.AreSame(copas, cities.ConsumeRespawnNotice());
            Assert.AreEqual(CityCatalog.AreiaBranca, cities.ApplyDefeatIn(CityCatalog.Get(CityCatalog.AreiaBranca)).CityId);
            Assert.IsNotNull(cities.ApplyDefeatIn(null), "no return city: the region's hub");
        }

        // ------------------------------------------------------------------
        // The elves
        // ------------------------------------------------------------------

        [Test]
        public void Garajonay_Residents_AreTheFichasElves_WithAnElfLookOfTheirOwn()
        {
            CitySpec copas = CityCatalog.Get(CityCatalog.CopasGarajonay);
            CollectionAssert.AreEquivalent(Elves, copas.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>();
            foreach (string id in Elves)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.ElfEars, $"{id}: long elf ears");
                Assert.AreEqual(NpcSwatch.SkinBark, look.Skin, $"{id}: bark-coloured skin (Mapas100 §3.3)");
                Assert.AreEqual(BiomeType.Forest, look.Region);
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
            }

            Assert.AreEqual(CityShopCatalog.ErvariaDacil, copas.Residents.Single(r => r.ResidentId == "dacil").ShopId, "Dácil keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.ErvariaDacil }, copas.ShopIds.ToArray());
        }

        [Test]
        public void Garajonay_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Elves)
            {
                CityResidentSpec resident = CityCatalog.AllResidents().Single(r => r.ResidentId == id);
                string pt = CityTexts.ResidentName(null, resident);
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotStartWith("Dona ", pt);
                StringAssert.StartsWith(char.ToUpperInvariant(id[0]).ToString(), pt.Replace("Dá", "Da"), $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Gara, a Guardiã das Copas", CityTexts.ResidentName(null, CityCatalog.AllResidents().Single(r => r.ResidentId == "gara")));
            Assert.AreEqual("Dácil, a Ervateira", CityTexts.ResidentName(null, CityCatalog.AllResidents().Single(r => r.ResidentId == "dacil")));
        }

        [Test]
        public void Garajonay_Keepers_StandAtTheirDoors_OutsideEverySolid_AndReachable()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.CopasGarajonay);
            // The whole validator, buildings and props off the streets included (the 250° bench stood on the South road).
            List<string> problems = CityLayoutRules.Validate(layout);
            Assert.IsEmpty(problems, string.Join("\n", problems));

            foreach (string id in Elves)
            {
                CityLot lot = layout.LotOfResident(id);
                Assert.IsNotNull(lot, $"{id} has no door to stand at");
                Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, lot.StandPoint), CityLayoutRules.WalkerRadius, $"{id} stands inside a collider");
                Assert.IsTrue(lot.Front.x < -0.5f || lot.Front.y < -0.5f, $"{id}'s door turns its back to the camera");
                Assert.IsTrue(layout.SafeZone.Contains(lot.StandPoint));
            }

            Assert.AreEqual("salao_de_gara", layout.LotOfResident("gara").Id, "Gara at her hall");
            Assert.AreEqual("casa_dacil", layout.LotOfShop(CityShopCatalog.ErvariaDacil).Id, "Dácil's counter at her house");
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn at the plaza");
        }

        [Test]
        public void Garajonay_TheRecipeKeepsItsGeometry_ForTheMapThatUsesIt()
        {
            // The map 05 and the overworld use the same recipe as their settlement: giving the elves a keeper changed no lot.
            CityTownRecipe recipe = CityTownRecipes.Garajonay();
            Assert.AreEqual(CityTownRecipes.GarajonayCityId, recipe.CityId);
            Assert.AreEqual(CityTownRecipes.GarajonayCanopy.Length * 2 - 1,
                recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CanopyTree || p.Kind == CityPropKind.RopeBridge));
            Assert.AreEqual(new Vector2(0f, 12.5f), recipe.Lots.Single(l => l.Id == "salao_de_gara").Center);
            Assert.IsFalse(recipe.Lots.Any(l => l.Kind == CityLotKind.Market), "no new standing spots in the elves' plaza");
        }

        // ------------------------------------------------------------------
        // Dácil's counter
        // ------------------------------------------------------------------

        [Test]
        public void DacilsShop_SellsTheGearOfTheGates_AndItsCounterTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.ErvariaDacil);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            CityShopEntry rope = CityShopCatalog.EntryOf(shop.ShopId, MapRequirementItems.ClimbingRope);
            Assert.IsNotNull(rope, "the Corda de Escalada of the canopy (map 05)");
            Assert.IsFalse(rope.Rotating, "the rope is always on the counter");
            Assert.IsFalse(rope.Unlock.IsGated, "from day one: the gate of the city's own map");
            CityShopEntry key = CityShopCatalog.EntryOf(shop.ShopId, MapRequirementItems.GardenKey);
            Assert.IsNotNull(key, "a copy of the Chave do Jardim (map 06)");
            Assert.AreEqual(ShopUnlockKind.RegionCleared, key.Unlock.Kind, "after the Forest boss, like Mariam's copy");
            Assert.AreEqual(BiomeType.Forest, key.Unlock.Region);

            // Still at Mariam's too (Docs/MetodologiaMapasERacas.md §2.1).
            Assert.IsNotNull(CityShopCatalog.EntryOf(CityShopCatalog.ArmarinhoCida, MapRequirementItems.ClimbingRope));
            Assert.IsNotNull(CityShopCatalog.EntryOf(CityShopCatalog.ArmarinhoCida, MapRequirementItems.GardenKey));

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            int rotating = shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main);
            Assert.Greater(rotating, shop.RotatingSlots, "more lines than slots: the counter turns");
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 3));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                CollectionAssert.AreEqual(today, ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList(), "same day, same counter");
                Assert.Contains(MapRequirementItems.ClimbingRope, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void Garajonay_OpensDacilsCounter_AndNeverTheCaravaneer()
        {
            CitySpec copas = CityCatalog.Get(CityCatalog.CopasGarajonay);
            for (int d = 0; d < 21; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(copas, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.ErvariaDacil, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Garajonay_AndTheNewResidents_HaveTextsInBothLanguages_ALinePerCousin()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec copas = CityCatalog.Get(CityCatalog.CopasGarajonay);
            var keys = new List<string> { copas.NameKey, copas.ButtonKey, copas.FlavourKey, CityShopCatalog.Get(CityShopCatalog.ErvariaDacil).NameKey };
            foreach (string id in Elves.Concat(new[] { "vo_edna", "socorro_vila", "xroshtag", "padwaxtag", "patig", "rosa" }))
            {
                CityResidentSpec resident = CityCatalog.AllResidents().Single(r => r.ResidentId == id);
                keys.Add(resident.NameKey);
                keys.Add(resident.AnyLineKey);
                keys.AddRange(Heroes.Select(resident.LineKey));
                Assert.AreEqual(Heroes.Length, Heroes.Select(h => CityTexts.ResidentLine(null, resident, h)).Distinct().Count(), $"{id} repeats a line");
            }

            foreach (string key in keys)
            {
                Assert.IsTrue(manager.HasKey(key), $"missing key {key}");
                Assert.IsTrue(byKey.TryGetValue(key, out LocalizationEntry entry), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"no pt-BR for {key}");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"no English for {key}");
                if (key.Contains(".line."))
                {
                    Assert.AreNotEqual(entry.En, entry.PtBR, $"{key} is not translated");
                }
            }

            // Every elf talks about the family: a cousin by name in each cousin's line, Vó Edna or Vô Jorge somewhere.
            foreach (string id in Elves)
            {
                CityResidentSpec resident = CityCatalog.AllResidents().Single(r => r.ResidentId == id);
                foreach (CharacterId hero in Heroes)
                {
                    string line = CityTexts.ResidentLine(null, resident, hero);
                    string name = hero == CharacterId.Icaro ? "Ícaro" : hero.ToString();
                    StringAssert.Contains(name, line, $"{id} → {hero}");
                }
            }

            string all = string.Join(" ", Elves.SelectMany(id => Heroes.Select(h => CityTexts.ResidentLine(null, CityCatalog.AllResidents().Single(r => r.ResidentId == id), h))));
            StringAssert.Contains("Jorge", all);
            Assert.IsTrue(all.Contains("Edna"), "Vó Edna is remembered in the canopy");
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByGarajonaysResidents_OnTheCitysOwnMonsters_WithRealRewards()
        {
            CitySpec copas = CityCatalog.Get(CityCatalog.CopasGarajonay);
            Assert.That(CityQuestCatalog.QuestsIn(copas.CityId).Count(), Is.InRange(1, 2));
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
            foreach (CityQuestSpec spec in CityQuestCatalog.Quests)
            {
                Assert.IsTrue(CityCatalog.Get(spec.CityId).Residents.Any(r => r.ResidentId == spec.GiverResidentId), $"{spec.QuestId}: the giver lives in the city");
                Assert.IsNull(RegionQuestCatalog.FindSpec(spec.QuestId), "no clash with a region quest");
                foreach (RegionObjectiveSpec objective in spec.Quest.Objectives)
                {
                    Assert.AreEqual(QuestObjectiveType.KillEnemyType, objective.Type, "progresses in combat");
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{objective.TargetId}");
                    Assert.IsNotNull(enemy, objective.TargetId);
                    Assert.IsFalse(enemy.IsBoss, objective.TargetId);
                    Assert.That(objective.RequiredAmount, Is.InRange(1, 10));
                }

                foreach (RegionRewardSpec reward in spec.Quest.Rewards)
                {
                    Assert.IsNotNull(ItemDatabase.GetById(reward.ItemId), reward.ItemId);
                }

                QuestDefinition quest = CityQuestCatalog.GetQuest(spec.QuestId);
                Assert.IsNotNull(quest);
                Assert.AreSame(quest, CityQuestCatalog.GetQuest(spec.QuestId), "cached");
                Assert.AreEqual(QuestType.Side, quest.QuestType);
                Assert.AreEqual(spec.Quest.Objectives.Count, quest.Objectives.Count);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), $"{spec.QuestId}: rewards resolve");

                foreach (string key in new[] { spec.Quest.TitleKey, spec.Quest.DescriptionKey, spec.Quest.JournalKey, spec.OfferKey, spec.ActiveKey }
                             .Concat(spec.Quest.Objectives.Select(o => spec.Quest.ObjectiveKey(o.ObjectiveId))))
                {
                    LocalizationEntry entry = CityQuestTexts.All.FirstOrDefault(e => e.Key == key);
                    Assert.IsNotNull(entry, $"missing {key}");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En) || string.IsNullOrWhiteSpace(entry.PtBR), key);
                }
            }

            Assert.IsNull(CityQuestCatalog.GetQuest("quest_nope"));
        }

        [Test]
        public void CityQuests_GaraOffersFirst_TinguaroOnlyAfterHerQuest()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("gara", isActive, isDone, out CityQuestSpec gara));
            Assert.AreEqual(CityQuestCatalog.GarajonayMist, gara.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("tinguaro", isActive, isDone, out _), "the chain starts with Gara");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("jonay", isActive, isDone, out _), "Jonay gives no quest");

            active.Add(CityQuestCatalog.GarajonayMist);
            Assert.AreEqual(CityQuestTalk.Active, CityQuestCatalog.Decide("gara", isActive, isDone, out _));

            active.Clear();
            done.Add(CityQuestCatalog.GarajonayMist);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("gara", isActive, isDone, out _), "done: back to her usual line");
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("tinguaro", isActive, isDone, out CityQuestSpec tinguaro));
            Assert.AreEqual(CityQuestCatalog.GarajonayMoss, tinguaro.QuestId);
            Assert.AreEqual(CityQuestCatalog.GarajonayMist, tinguaro.Quest.PrerequisiteQuestId);
        }

        [Test]
        public void CityQuests_TalkStartsTheQuest_OnTheQuestManager()
        {
            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            Assert.AreEqual(CityQuestCatalog.Quests[0].OfferKey, CityQuestCatalog.Talk(quests, "gara"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.GarajonayMist));
            Assert.AreEqual(CityQuestCatalog.Quests[0].ActiveKey, CityQuestCatalog.Talk(quests, "gara"), "asked again: how it goes");
            Assert.IsNull(CityQuestCatalog.Talk(quests, "tinguaro"), "his quest waits for Gara's");
            Assert.IsNull(CityQuestCatalog.Talk(null, "gara"));
            quests.Shutdown();
        }

        // ------------------------------------------------------------------
        // The lore cast of the first four cities
        // ------------------------------------------------------------------

        [Test]
        public void LoreCast_OfTheFirstFourCities_StandsInTheirTowns_OutsideEverySolid()
        {
            // TODO.md "História e Arcos dos NPCs da Ilha da Aurora" (old ids where they exist).
            var cast = new Dictionary<string, string[]>
            {
                { CityCatalog.VilaZefa, new[] { "vo_edna", "cida", "severino", "damiao", "fatima", "lazaro", "nenem", "tonico_haras", "patig", "xroshtag", "padwaxtag", "socorro_vila" } },
                { CityCatalog.AreiaBranca, new[] { "rosa", "vava", "socorro", "tiao" } },
                { CityCatalog.SerraAlta, new[] { "tonho", "graca", "quinzinho" } },
                { CityCatalog.Lamparinas, new[] { "benedita", "candinho", "fia", "zezinho" } },
            };

            foreach (KeyValuePair<string, string[]> city in cast)
            {
                CitySpec spec = CityCatalog.Get(city.Key);
                CityLayout layout = CityTownRecipes.LayoutFor(city.Key);
                var grid = CityLayoutRules.WalkGrid.Build(layout, CityLayoutRules.MinCorridor * 0.5f);
                Assert.IsTrue(grid.Flood(layout.PlazaCenter));
                foreach (string id in city.Value)
                {
                    Assert.IsTrue(spec.Residents.Any(r => r.ResidentId == id), $"{city.Key}: {id} is not a resident");
                    Assert.IsNotNull(NpcRecipes.Find(id), $"{id} has no look");
                    CityLot lot = layout.LotOfResident(id);
                    Assert.IsNotNull(lot, $"{city.Key}: {id} stands nowhere");
                    Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, lot.StandPoint), CityLayoutRules.WalkerRadius, $"{city.Key}: {id} stands inside a solid");
                    Assert.LessOrEqual(Vector2.Distance(lot.StandPoint, lot.DoorApproach), lot.InteractRadius, $"{city.Key}: {id} out of reach");
                    Assert.IsTrue(grid.IsReached(lot.DoorApproach), $"{city.Key}: nobody can walk up to {id}");
                }
            }

            Assert.AreEqual("socorro_vila", Rapadura.Gameplay.Inventory.RomariaOratory.ForCity(CityCatalog.VilaZefa).KeeperResidentId, "Ramrat keeps the Vila's lamp");
        }
    }
}
