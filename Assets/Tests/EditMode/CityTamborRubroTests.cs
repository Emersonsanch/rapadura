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
    /// The Tambor Rubro as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 11): its <see cref="CitySpec"/>
    /// among the race cities of the Areias Flamejantes, the five orcs of the ficha at their doors with their own look (tusks),
    /// the WarCamp style (log halls, palisade, drum tower, clan tents), the whole town inside the place the Areias overworld
    /// keeps for it, Tushratta's Forja de Tambor with the theriac of map 12, the two city quests and the respawn of 11–12.
    /// </summary>
    public class CityTamborRubroTests
    {
        private static readonly string[] Cast = { "gurparanzah", "kessi", "appu", "ashmunikal", "tushratta" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        /// <summary>The overworld's place for the city (OverworldLayouts.Areias: safe zone ±34, flat ground ±38).</summary>
        private const float OverworldSafeHalf = 34f;

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void TamborRubro_HasACitySpec_AsARaceCityOfTheDesert_SameIdAsItsMapAndRecipe()
        {
            CitySpec tambor = CityCatalog.Get(CityCatalog.TamborRubro);
            Assert.IsNotNull(tambor, "the Tambor Rubro has a CitySpec");
            Assert.AreEqual(BiomeType.Desert, tambor.Region);
            Assert.AreEqual(MapCatalog.TamborRubro, tambor.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.TamborRubroCityId, tambor.CityId, "the recipe's city id");
            Assert.IsTrue(CityCatalog.IsRaceCity(tambor));
            Assert.IsFalse(CityCatalog.IsHubCity(tambor));
            Assert.IsFalse(CityCatalog.Cities.Contains(tambor), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.AreiaBranca, CityCatalog.ForRegion(BiomeType.Desert).CityId, "the Desert hub is still Qocho");
            Assert.IsFalse(tambor.HasGrandmaServices);
            Assert.IsFalse(tambor.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(tambor.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(tambor.CityId));
        }

        [Test]
        public void MapsOfTheTamborRubro_RespawnInItsPlaza()
        {
            Assert.AreEqual(CityCatalog.TamborRubro, MapCatalog.RespawnCityFor("map_011_tambor_rubro").CityId);
            Assert.AreEqual(CityCatalog.TamborRubro, MapCatalog.RespawnCityFor("map_012_namrael").CityId);
            Assert.AreEqual(CityCatalog.AreiaBranca, MapCatalog.RespawnCityFor("map_007_caravanas").CityId, "07–10 still wake in Qocho");

            var cities = new CityProgress();
            CitySpec tambor = CityCatalog.Get(CityCatalog.TamborRubro);
            Assert.AreSame(tambor, cities.ApplyDefeatIn(tambor), "a defeat on 11/12 wakes the cousins in the war camp's plaza");
            Assert.AreSame(tambor, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void TamborRubro_IsAWarCamp_WithPalisadeDrumTowerTentsAndFires()
        {
            CityTownRecipe recipe = CityTownRecipes.TamborRubro();
            Assert.AreSame(TownStyles.WarCamp, recipe.Style);
            Assert.AreEqual(TownArchitecture.WarCamp, TownStyles.WarCamp.Architecture);
            Assert.AreEqual(BiomeType.Desert, TownStyles.WarCamp.Region);
            Assert.AreEqual(TownRoofKind.Gable, TownStyles.WarCamp.HouseRoof);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TamborRubro);
            Assert.AreSame(TownStyles.WarCamp, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.DrumTower), "Ashmunikal's drum tower");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Palisade), 7, "the palisade all round, gaps for the streets");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ClanTent), 6);
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Bonfire), 3);
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.TrainingRing), "the sand arena");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.Palisade));
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.LogHall && l.Building.Roof == TownRoofKind.Gable),
                "every hall is a log hall with a steep gable");
            Assert.AreEqual(TownShopDressing.Forja, recipe.Lots.Single(l => l.ResidentId == "tushratta").Spec.Dressing);
            Assert.AreEqual(TownShopDressing.HuntingRack, recipe.Lots.Single(l => l.ResidentId == "kessi").Spec.Dressing);

            // Other towns keep their own style (log halls only by the war camp's style or a hand spec).
            Assert.IsFalse(CityTownRecipes.LayoutFor(CityCatalog.AreiaBranca).Lots.Any(l => l.Building != null && l.Building.LogHall));
        }

        [Test]
        public void TamborRubro_FitsThePlaceTheAreiasOverworldKeeps_AtGroundLevel()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TamborRubro);
            var keep = new Rect(-OverworldSafeHalf, -OverworldSafeHalf, OverworldSafeHalf * 2f, OverworldSafeHalf * 2f);
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.IsTrue(CityGeometry.Contains(keep, solid.Footprint), $"{solid.Owner} at {solid.Footprint} leaves the ±34 m the overworld keeps");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.IsTrue(CityGeometry.Contains(keep, lot.Footprint), lot.Id);
            }

            foreach (CityStreet street in layout.Streets)
            {
                Assert.IsTrue(keep.Contains(street.End), street.Arm.ToString());
            }

            foreach (CityExit exit in layout.Exits.Where(e => e.Kind == CityExitKind.ToMap))
            {
                Assert.IsTrue(keep.Contains(exit.Target), exit.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.South, CityArm.East, CityArm.West }, layout.Streets.Select(s => s.Arm).ToArray(), "same three roads out as the overworld's trails");
            Assert.AreEqual(3, layout.Exits.Count(e => e.Kind == CityExitKind.ToMap));
        }

        [Test]
        public void TownPalette_AndNpcPalette_CoverEverySwatch()
        {
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, Enum.GetValues(typeof(TownSwatch)).Length);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);
            Assert.AreEqual(NpcPalette.Count, Enum.GetValues(typeof(NpcSwatch)).Length);
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void TamborRubro_Residents_AreTheFichasOrcs_WithALookOfTheirOwn()
        {
            CitySpec tambor = CityCatalog.Get(CityCatalog.TamborRubro);
            CollectionAssert.AreEquivalent(Cast, tambor.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.Tusks, $"{id}: an orc of the Red Drum");
                Assert.IsFalse(look.ElfEars);
                Assert.IsTrue(look.Skin == NpcSwatch.SkinOrcOlive || look.Skin == NpcSwatch.SkinOrcGrey, $"{id}: olive or grey skin");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
            }

            Assert.AreEqual(CityShopCatalog.ForjaTambor, tambor.Residents.Single(r => r.ResidentId == "tushratta").ShopId, "Tushratta keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.ForjaTambor }, tambor.ShopIds.ToArray());
        }

        [Test]
        public void TamborRubro_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotStartWith("Dona ", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Gurparanzah, a Chefe-Arqueira", CityTexts.ResidentName(null, Resident("gurparanzah")));
            Assert.AreEqual("Ashmunikal, a Matriarca dos Tambores", CityTexts.ResidentName(null, Resident("ashmunikal")));
        }

        [Test]
        public void TamborRubro_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.TamborRubro, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TamborRubro);
            var grid = CityLayoutRules.WalkGrid.Build(layout, CityLayoutRules.MinCorridor * 0.5f);
            Assert.IsTrue(grid.Flood(layout.PlazaCenter));
            foreach (string id in Cast)
            {
                CityLot lot = layout.LotOfResident(id);
                Assert.IsNotNull(lot, $"{id} has no door to stand at");
                Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, lot.StandPoint), CityLayoutRules.WalkerRadius, $"{id} stands inside a collider");
                Assert.LessOrEqual(Vector2.Distance(lot.StandPoint, lot.DoorApproach), lot.InteractRadius, $"{id} out of reach");
                Assert.IsTrue(grid.IsReached(lot.DoorApproach), $"nobody can walk up to {id}");
                Assert.IsTrue(lot.Front.x < -0.5f || lot.Front.y < -0.5f, $"{id}'s door turns its back to the camera");
                Assert.IsTrue(layout.SafeZone.Contains(lot.StandPoint));
            }

            Assert.AreEqual("salao_grande", layout.LotOfResident("gurparanzah").Id, "Gurparanzah in the Salão Grande at the plaza head");
            Assert.AreEqual("forja_de_tambor", layout.LotOfShop(CityShopCatalog.ForjaTambor).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn at the plaza");
        }

        // ------------------------------------------------------------------
        // Tushratta's forge
        // ------------------------------------------------------------------

        [Test]
        public void ForjaDeTambor_SellsTheTheriacOfMap12_AtBalancedPrices_AndItsCounterTurnsWeekly()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.ForjaTambor);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Desert, shop.HomeRegion);
            Assert.AreEqual(ShopTheme.Ferreiro, shop.Theme);
            Assert.AreEqual(ShopRotationPeriod.Weekly, shop.Period);

            // Map 11 has no gate; map 12 (Trono de Namrael) asks for protection from poison: the theriac, always on the counter.
            Assert.IsTrue(string.IsNullOrEmpty(MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 11).SpecialRequirementTag)));
            string theriac = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 12).SpecialRequirementTag);
            Assert.AreEqual(MapRequirementItems.Theriac, theriac);
            CityShopEntry gate = CityShopCatalog.EntryOf(shop.ShopId, theriac);
            Assert.IsNotNull(gate, "Tushratta sells the theriac");
            Assert.IsFalse(gate.Rotating);
            Assert.IsFalse(gate.Unlock.IsGated);

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            Assert.Greater(shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main), shop.RotatingSlots, "more lines than slots: the counter turns");
            long week = ShopRotation.PeriodIndex(new DateTime(2026, 11, 3), ShopRotationPeriod.Weekly);
            var counters = new HashSet<string>();
            for (int w = 0; w < 8; w++)
            {
                List<string> stock = ShopRotation.PickStock(shop, week + w, null).Select(e => e.ItemId).ToList();
                CollectionAssert.AreEqual(stock, ShopRotation.PickStock(shop, week + w, null).Select(e => e.ItemId).ToList(), "same week, same counter");
                Assert.Contains(theriac, stock);
                counters.Add(string.Join(",", stock));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from week to week");
        }

        [Test]
        public void TamborRubro_OpensTheForge_AndNeverTheCaravaneer_WhoStillVisitsQocho()
        {
            CitySpec tambor = CityCatalog.Get(CityCatalog.TamborRubro);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(tambor, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.ForjaTambor, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void TamborRubro_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec tambor = CityCatalog.Get(CityCatalog.TamborRubro);
            var keys = new List<string> { tambor.NameKey, tambor.ButtonKey, tambor.FlavourKey, CityShopCatalog.Get(CityShopCatalog.ForjaTambor).NameKey };
            foreach (string id in Cast)
            {
                CityResidentSpec resident = Resident(id);
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

            foreach (string id in Cast)
            {
                foreach (CharacterId hero in Heroes)
                {
                    string line = CityTexts.ResidentLine(null, Resident(id), hero);
                    string name = hero == CharacterId.Icaro ? "Ícaro" : hero.ToString();
                    StringAssert.Contains(name, line, $"{id} → {hero}");
                }
            }

            string all = string.Join(" ", Cast.SelectMany(id => Heroes.Select(h => CityTexts.ResidentLine(null, Resident(id), h))));
            StringAssert.Contains("Jorge", all);
            StringAssert.Contains("Edna", all);
            foreach (string folk in new[] { "Saci", "Curupira", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByTheTamborRubrosResidents_WithTheTheriacAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.TamborRubro).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "gurparanzah", "kessi" }, quests.Select(q => q.GiverResidentId).ToArray());
            foreach (CityQuestSpec spec in quests)
            {
                foreach (RegionObjectiveSpec objective in spec.Quest.Objectives)
                {
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{objective.TargetId}");
                    Assert.IsNotNull(enemy, objective.TargetId);
                    Assert.IsFalse(enemy.IsBoss, objective.TargetId);
                }

                QuestDefinition quest = CityQuestCatalog.GetQuest(spec.QuestId);
                Assert.IsNotNull(quest);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), $"{spec.QuestId}: rewards resolve");
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.Theriac), "Kessi pays with the theriac (map 12)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_GurparanzahOffersFirst_KessiOnlyAfterHerQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("gurparanzah", isActive, isDone, out CityQuestSpec chief));
            Assert.AreEqual(CityQuestCatalog.TamborRubroScorpions, chief.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("kessi", isActive, isDone, out _), "the chain starts with the chief");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("appu", isActive, isDone, out _), "Appu gives no quest");

            done.Add(CityQuestCatalog.TamborRubroScorpions);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("kessi", isActive, isDone, out CityQuestSpec kessi));
            Assert.AreEqual(CityQuestCatalog.TamborRubroHawks, kessi.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec scorpions = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.TamborRubroScorpions);
            Assert.AreEqual(scorpions.OfferKey, CityQuestCatalog.Talk(quests, "gurparanzah"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.TamborRubroScorpions));
            Assert.AreEqual(scorpions.ActiveKey, CityQuestCatalog.Talk(quests, "gurparanzah"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "kessi"), "his quest waits for the chief's");
            quests.Shutdown();
        }
    }
}
