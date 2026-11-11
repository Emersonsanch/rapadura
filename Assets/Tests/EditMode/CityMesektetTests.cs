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
    /// Cais de Mesektet, the quay of the night barque, as a walkable capital (Docs/Cities.md, Docs/Mapas100.md map 71): its
    /// <see cref="CitySpec"/> among the late-region cities, the four people of the tales of Kemet named after the ficha (Sinuhe,
    /// Khunanup, Djedi, Setne) at their doors, the PapyrusQuay style (halls of sandstone on rafts with papyrus columns, the Cais
    /// da Barca with the Mesektet moored, the piers of the hours with plank bridges, papyrus columns and thickets, reed skiffs),
    /// the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), the Trocas do Khunanup with
    /// the gear the hours of the river ask for (never the life or chaos stones) and the two city quests; maps 71–76 respawn on its
    /// plaza and their gates are on Khunanup's counter (MapRequirementTests).
    /// </summary>
    public class CityMesektetTests
    {
        private static readonly string[] Cast = { "sinuhe", "khunanup", "djedi", "setne" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void Mesektet_HasACitySpec_AsALateRegionCity_SameIdAsItsRecipe()
        {
            CitySpec cais = CityCatalog.Get(CityCatalog.CaisDeMesektet);
            Assert.IsNotNull(cais, "the Cais has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, cais.Region, "the river borrows the Forest (always unlocked)");
            Assert.AreEqual("cais_de_mesektet", cais.CityId, "the return city id of maps 71–76");
            Assert.AreEqual(CityTownRecipes.MesektetCityId, cais.CityId, "the recipe's city id");

            Assert.IsTrue(CityCatalog.IsLateRegionCity(cais), "a capital of the late regions, as Hinokura and the Porto");
            Assert.IsFalse(CityCatalog.IsRaceCity(cais));
            Assert.IsFalse(CityCatalog.IsHubCity(cais));
            Assert.IsFalse(cais.HasGrandmaServices);
            Assert.IsFalse(cais.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(cais.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(cais.CityId));

            var cities = new CityProgress();
            Assert.AreSame(cais, cities.ApplyDefeatIn(cais), "a defeat on the river wakes the cousins by the barque");
            Assert.AreSame(cais, cities.ConsumeRespawnNotice());
        }

        [Test]
        public void MapsOfTheRiver_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.CaisDeMesektet).ToList();
            CollectionAssert.AreEquivalent(new[] { 71, 72, 73, 74, 75, 76 }, maps.Select(m => m.MapId).ToArray(), "the Cais is the return city of 71–76 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.CaisDeMesektet, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get("map_071_cais_mesektet").Settlement);
            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_071_cais_mesektet", _ => CityCatalog.CaisDeMesektet, CityCatalog.VilaZefa)));
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Mesektet_IsAPapyrusQuay_WithTheBarquePiersColumnsThicketsAndSkiffs()
        {
            CityTownRecipe recipe = CityTownRecipes.CaisDeMesektet();
            Assert.AreSame(TownStyles.PapyrusQuay, recipe.Style);
            Assert.AreEqual(TownArchitecture.PapyrusQuay, TownStyles.PapyrusQuay.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.CaisDeMesektet);
            Assert.AreSame(TownStyles.PapyrusQuay, layout.Style);
            CityProp barque = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.NightBarque);
            Assert.Less(barque.Position.magnitude, layout.PlazaRadius, "the Cais da Barca stands on the plaza");
            Assert.Greater(barque.Position.magnitude, TownKit.NightBarqueRadius + CityLayoutRules.WalkerRadius, "...but not on the respawn");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.PapyrusColumn), 4, "papyrus columns");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.PapyrusColumn && p.Variant == 1), 1, "a taller column with painted stars");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.PapyrusThicket), 4, "thickets of papyrus");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ReedSkiff), 3, "reed skiffs on the bank");

            List<CityProp> piers = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.HourCanal).ToList();
            Assert.GreaterOrEqual(piers.Count, 2, "piers of the hours with their plank bridges");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.HourCanal));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.NightBarque));
            Assert.IsNull(CitySolids.PropSolid(piers[0], layout.WalkBounds), "the plank bridges are walkable");
            foreach (CityProp pier in piers)
            {
                Assert.IsTrue(layout.Streets.Any(s => s.Road.Contains((pier.Position + pier.End) * 0.5f)), $"the bridge at {pier.Position} carries a street");
            }

            Assert.IsTrue(CitySolids.PropSolid(barque, layout.WalkBounds).HasValue, "the barque stops the walker");
            List<CityLot> houses = layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).ToList();
            Assert.IsTrue(houses.All(l => l.Building.PapyrusHall && !l.Building.LacquerHall && !l.Building.MoonLonghouse && !l.Building.ReedHall),
                "every house is a hall of the quay (not Hinokura's lacquer, nor Aratta's reed)");
            Assert.AreEqual(TownShopDressing.ReedMarket, recipe.Lots.Single(l => l.ResidentId == "khunanup").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the bank drops to the dark river");
            Assert.AreEqual(TownSwatch.QuaySandstone, TownStyles.PapyrusQuay.Ground, "the sandbank of the quay");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.StarGlow), 0.5f, "the starlight of the lamps glows");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.QuaySandstone), "the sandstone does not glow");
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);

            foreach (CityLot lot in houses)
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.NightBarque && p.Kind <= CityPropKind.ReedSkiff))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.PapyrusQuay);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.PapyrusQuay);
            Assert.Greater(gate.TriangleCount, 40, "the gate of two papyrus columns under a cornice");
        }

        [Test]
        public void ReedHorn_KeepsItsOwnReedMat_NotTheQuaysMat()
        {
            CollectionAssert.Contains(TownStyles.ReedHorn.Roofs, TownSwatch.ReedMat, "Aratta's roofs are its own reed mat");
            CollectionAssert.DoesNotContain(TownStyles.ReedHorn.Roofs, TownSwatch.MesektetMat);
            CollectionAssert.DoesNotContain(TownStyles.ReedHorn.Walls, TownSwatch.MesektetMat);
        }

        [Test]
        public void Mesektet_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.CaisDeMesektet);
            float radius = CityTownRecipes.MesektetSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld will keep a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.East, CityArm.South }, layout.Streets.Select(s => s.Arm).ToArray());
            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.MesektetHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.CaisDeMesektet().EntryArm, "the road in from the sandbanks");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Mesektet_Residents_AreTheFichas_PeopleOfTheQuayWithALookOfTheirOwn()
        {
            CitySpec cais = CityCatalog.Get(CityCatalog.CaisDeMesektet);
            CollectionAssert.AreEquivalent(Cast, cais.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.RiverNight, NpcSwatch.KemetGold, NpcSwatch.PapyrusGreen, NpcSwatch.QuaySandstone, NpcSwatch.NightWater, NpcSwatch.NileLinen, NpcSwatch.LapisBlue, NpcSwatch.KohlBlack };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Satyr || look.Snout, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the quay");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsFalse(look.Beard == NpcBeard.LongBraided && (look.Layers & NpcLayer.Cape) != 0, $"{id}: a long braided beard and a cape blow the budget");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.TrocasKhunanup, cais.Residents.Single(r => r.ResidentId == "khunanup").ShopId, "Khunanup keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.TrocasKhunanup }, cais.ShopIds.ToArray());
        }

        [Test]
        public void Mesektet_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
            }

            Assert.AreEqual("Sinuhe, o Barqueiro-Chefe", CityTexts.ResidentName(null, Resident("sinuhe")));
            Assert.AreEqual("Khunanup, o Comerciante do Cais", CityTexts.ResidentName(null, Resident("khunanup")));
            Assert.AreEqual("Djedi, o Mágico", CityTexts.ResidentName(null, Resident("djedi")));
            Assert.AreEqual("Setne, o Bibliotecário do Caderno", CityTexts.ResidentName(null, Resident("setne")));
        }

        [Test]
        public void Mesektet_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.CaisDeMesektet, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            // Ammo's stall (PlazaRadius · (0.575, −0.675)): nothing solid in front of his counter.
            CityTownRecipe recipe = CityTownRecipes.CaisDeMesektet();
            var stall = new Vector2(recipe.PlazaRadius * 0.575f, -recipe.PlazaRadius * 0.675f);
            CityLayout withAmmo = CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.CaisDeMesektet, true));
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(withAmmo, stall + Vector2.up * 1.4f), CityLayoutRules.WalkerRadius, "the cousins can step up to Ammo's counter");

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.CaisDeMesektet);
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

            Assert.AreEqual("trocas_do_khunanup", layout.LotOfShop(CityShopCatalog.TrocasKhunanup).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the landing of the boats");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // The Trocas do Khunanup
        // ------------------------------------------------------------------

        [Test]
        public void TrocasDoKhunanup_SellsTheGearOfTheHours_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.TrocasKhunanup);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            // The gear the hours 71–76 ask for, fixed from the first day (MapRequirementTests checks their tags against this counter).
            var gear = new[]
            {
                MapRequirementItems.DhowPassage, MapRequirementItems.ReedBoat, MapRequirementItems.RootBoots, MapRequirementItems.OilLamp, MapRequirementItems.RunicLamp,
                MapRequirementItems.EternalFireOil, MapRequirementItems.Waterskin, MapRequirementItems.Theriac, MapRequirementItems.AntiVenomAmulet, MapRequirementItems.ClimbingRope,
            };
            foreach (string fixedItem in gear)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Khunanup sells {fixedItem}");
                Assert.IsFalse(entry.Rotating, $"{fixedItem} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, $"{fixedItem} from the first day");
            }

            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, "item_pedra_vida"), "the life stone is never sold");
            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, "item_pedra_caos"), "the chaos stone is never sold");

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            Assert.Greater(shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main), shop.RotatingSlots, "more lines than slots: the counter turns");
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 6));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                Assert.Contains(MapRequirementItems.ReedBoat, today);
                Assert.Contains(MapRequirementItems.RunicLamp, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec cais = CityCatalog.Get(CityCatalog.CaisDeMesektet);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(cais, new DateTime(2026, 11, 6).AddDays(d), null);
                Assert.Contains(CityShopCatalog.TrocasKhunanup, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Mesektet_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec cais = CityCatalog.Get(CityCatalog.CaisDeMesektet);
            var keys = new List<string> { cais.NameKey, cais.ButtonKey, cais.FlavourKey, CityShopCatalog.Get(CityShopCatalog.TrocasKhunanup).NameKey };
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
            StringAssert.Contains("papiro", all, "the papyri of the quay");
            StringAssert.Contains("Mesektet", all, "the barque of the night");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenBySinuheAndDjedi_OnExistingEnemies_WithARunicLampAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.HomeQuestsIn(CityCatalog.CaisDeMesektet).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "sinuhe", "djedi" }, quests.Select(q => q.GiverResidentId).ToArray());
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
                Assert.IsFalse(spec.Quest.Rewards.Any(r => r.ItemId == "item_pedra_vida" || r.ItemId == "item_pedra_caos"), spec.QuestId);
                foreach (string suffix in new[] { ".title", ".description", ".journal", ".offer", ".active" })
                {
                    Assert.IsNotNull(CityQuestTexts.GetFallback(spec.KeyBase + suffix), spec.KeyBase + suffix);
                }
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.RunicLamp), "Djedi pays with a runic lamp (the endless night, 73–74)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("sinuhe", active.Contains, done.Contains, out CityQuestSpec crocodiles));
            Assert.AreEqual(CityQuestCatalog.MesektetCrocodiles, crocodiles.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("djedi", active.Contains, done.Contains, out _), "the chain starts with the chief boatman");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("khunanup", active.Contains, done.Contains, out _), "Khunanup gives no quest");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("setne", active.Contains, done.Contains, out _), "Setne gives no quest");
            done.Add(CityQuestCatalog.MesektetCrocodiles);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("djedi", active.Contains, done.Contains, out CityQuestSpec scorpions));
            Assert.AreEqual(CityQuestCatalog.MesektetScorpions, scorpions.QuestId);
        }
    }
}
