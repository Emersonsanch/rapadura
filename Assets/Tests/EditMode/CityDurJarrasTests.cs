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
    /// Dur-Jarras, the ziggurat city of the Margens da Mistura, as a walkable capital (Docs/Cities.md, Docs/Mapas100.md map 77): its
    /// <see cref="CitySpec"/> among the late-region cities, the three people of the ficha (Utnapishtim, Atrahasis, Sin-leqi-unninni) and
    /// Siduri at their doors, the Ziggurat style (houses of baked brick, the ziggurat of five steps, the Relógio de Zurvan, the bitumen
    /// canals with brick bridges, palms, glazed lions and stacks of jars), the whole town inside a circle of radius ~22.5 m at y = 0
    /// with its three roads out (W, E, S), the Betume do Atrahasis with the gear maps 77–82 will ask for (never the life or chaos
    /// stones) and the two city quests. Maps 77–82 are not in <see cref="MapCatalog"/> yet: no respawn through the catalog and no gates
    /// here (when they enter, add them as in CityHinokuraTests.MapsOfTheArchipelago_RespawnInItsPlaza and MapRequirementTests).
    /// </summary>
    public class CityDurJarrasTests
    {
        private static readonly string[] Cast = { "utnapishtim", "atrahasis", "sin_leqi_unninni", "siduri" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        [Test]
        public void DurJarras_HasACitySpec_AsALateRegionCity_SameIdAsItsRecipe()
        {
            CitySpec city = CityCatalog.Get(CityCatalog.DurJarras);
            Assert.IsNotNull(city, "Dur-Jarras has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, city.Region, "the Margens borrow the Forest (always unlocked)");
            Assert.AreEqual("dur_jarras", city.CityId, "the return city id maps 77–82 will use");
            Assert.AreEqual(CityTownRecipes.DurJarrasCityId, city.CityId);

            Assert.IsTrue(CityCatalog.IsLateRegionCity(city));
            Assert.IsFalse(CityCatalog.IsRaceCity(city));
            Assert.IsFalse(CityCatalog.IsHubCity(city));
            Assert.IsFalse(city.HasGrandmaServices);
            Assert.IsFalse(city.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(city.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(city.CityId));

            var cities = new CityProgress();
            Assert.AreSame(city, cities.ApplyDefeatIn(city), "a defeat in the Margens wakes the cousins at the foot of the ziggurat");
            Assert.AreSame(city, cities.ConsumeRespawnNotice());
        }

        [Test]
        public void DurJarras_IsAZigguratTown_WithTheStepsClockCanalsPalmsLionsAndJars()
        {
            CityTownRecipe recipe = CityTownRecipes.DurJarras();
            Assert.AreSame(TownStyles.Ziggurat, recipe.Style);
            Assert.AreEqual(TownArchitecture.Ziggurat, TownStyles.Ziggurat.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.DurJarras);
            Assert.AreSame(TownStyles.Ziggurat, layout.Style);

            CityProp ziggurat = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.Ziggurat);
            Assert.IsFalse(CityGeometry.RectOverlapsCircle(CitySolids.PropSolid(ziggurat, layout.WalkBounds).Value.Footprint, layout.PlazaCenter, layout.PlazaRadius),
                "the ziggurat closes the plaza from outside its circle");

            CityProp clock = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.ZurvanClock);
            Assert.Less(clock.Position.magnitude, layout.PlazaRadius, "the Relógio de Zurvan stands on the plaza");
            Assert.Greater(clock.Position.magnitude, TownKit.ZurvanClockRadius + CityLayoutRules.WalkerRadius, "...but not on the respawn");
            Assert.IsTrue(CitySolids.PropSolid(clock, layout.WalkBounds).HasValue, "the clock stops the walker");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.DatePalm), 4, "date palms");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.GlazedLion), 3, "glazed lions");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.JarStack), 3, "the jars that name the city");

            List<CityProp> canals = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.BitumenCanal).ToList();
            Assert.GreaterOrEqual(canals.Count, 2, "canals with their brick bridges");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.BitumenCanal));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.Ziggurat));
            Assert.IsNull(CitySolids.PropSolid(canals[0], layout.WalkBounds), "the brick bridges are walkable");
            foreach (CityProp canal in canals)
            {
                Assert.IsTrue(layout.Streets.Any(s => s.Road.Contains((canal.Position + canal.End) * 0.5f)), $"the bridge at {canal.Position} carries a street");
            }

            List<CityLot> houses = layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).ToList();
            Assert.IsTrue(houses.All(l => l.Building.ZigguratHall && !l.Building.PapyrusHall && !l.Building.LacquerHall), "every house is of baked brick");
            Assert.AreEqual(TownShopDressing.BitumenYard, recipe.Lots.Single(l => l.ResidentId == "atrahasis").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the ziggurat closes it");
            Assert.AreEqual(TownSwatch.DustSky, TownStyles.Ziggurat.Ground);
            Assert.Greater(TownPalette.GlowOf(TownSwatch.ZigguratFlame), 0.5f, "the flame of the ziggurat glows");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.BakedBrick), "brick does not glow");
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);

            foreach (CityLot lot in houses)
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.Ziggurat && p.Kind <= CityPropKind.JarStack))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.Ziggurat);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.Ziggurat);
            Assert.Greater(gate.TriangleCount, 40, "the gate of two stepped brick towers");
        }

        [Test]
        public void DurJarras_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.DurJarras);
            float radius = CityTownRecipes.DurJarrasSolidRadius;
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

            Assert.AreEqual(CityTownRecipes.DurJarrasHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.DurJarras().EntryArm, "the road in from the Deserto do Relógio");
        }

        [Test]
        public void DurJarras_Residents_AreTheFichas_PeopleOfTheBrickTownWithALookOfTheirOwn()
        {
            CitySpec city = CityCatalog.Get(CityCatalog.DurJarras);
            CollectionAssert.AreEquivalent(Cast, city.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.BakedBrick, NpcSwatch.GlazedBlue, NpcSwatch.Bitumen, NpcSwatch.ElamGold, NpcSwatch.DustSky, NpcSwatch.PalmFrond, NpcSwatch.ClayTablet, NpcSwatch.FringeWool };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Satyr || look.Snout, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the brick town");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsFalse(look.Beard == NpcBeard.LongBraided && (look.Layers & NpcLayer.Cape) != 0, $"{id}: a long braided beard and a cape blow the budget");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.BetumeAtrahasis, city.Residents.Single(r => r.ResidentId == "atrahasis").ShopId, "Atrahasis keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.BetumeAtrahasis }, city.ShopIds.ToArray());
        }

        [Test]
        public void DurJarras_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
            }

            Assert.AreEqual("Utnapishtim, o Guardião do Relógio", CityTexts.ResidentName(null, Resident("utnapishtim")));
            Assert.AreEqual("Atrahasis, o Mestre das Balsas", CityTexts.ResidentName(null, Resident("atrahasis")));
            Assert.AreEqual("Sin-leqi-unninni, o Bibliotecário", CityTexts.ResidentName(null, Resident("sin_leqi_unninni")));
            Assert.AreEqual("Siduri, a Taberneira", CityTexts.ResidentName(null, Resident("siduri")));
        }

        [Test]
        public void DurJarras_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.DurJarras, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            // Ammo's stall (PlazaRadius · (0.575, −0.675)): nothing solid in front of his counter.
            CityTownRecipe recipe = CityTownRecipes.DurJarras();
            var stall = new Vector2(recipe.PlazaRadius * 0.575f, -recipe.PlazaRadius * 0.675f);
            CityLayout withAmmo = CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.DurJarras, true));
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(withAmmo, stall + Vector2.up * 1.4f), CityLayoutRules.WalkerRadius, "the cousins can step up to Ammo's counter");

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.DurJarras);
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

            Assert.AreEqual("patio_do_atrahasis", layout.LotOfShop(CityShopCatalog.BetumeAtrahasis).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the raft landing");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        [Test]
        public void BetumeDoAtrahasis_SellsTheGearOfTheMargens_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.BetumeAtrahasis);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            var gear = new[]
            {
                MapRequirementItems.Waterskin, MapRequirementItems.ReedBoat, MapRequirementItems.DhowPassage, MapRequirementItems.RootBoots, MapRequirementItems.EternalFireOil,
                MapRequirementItems.Theriac, MapRequirementItems.ClimbingRope, MapRequirementItems.ClimbingCrampons, MapRequirementItems.OilLamp,
            };
            foreach (string fixedItem in gear)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Atrahasis sells {fixedItem}");
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
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 9));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                Assert.Contains(MapRequirementItems.ReedBoat, today);
                Assert.Contains(MapRequirementItems.ClimbingCrampons, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec city = CityCatalog.Get(CityCatalog.DurJarras);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(city, new DateTime(2026, 11, 9).AddDays(d), null);
                Assert.Contains(CityShopCatalog.BetumeAtrahasis, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        [Test]
        public void DurJarras_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec city = CityCatalog.Get(CityCatalog.DurJarras);
            var keys = new List<string> { city.NameKey, city.ButtonKey, city.FlavourKey, CityShopCatalog.Get(CityShopCatalog.BetumeAtrahasis).NameKey };
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
            StringAssert.Contains("tijolo", all, "the brick of the town");
            StringAssert.Contains("Betume", all, "the bitumen of the rafts");
            StringAssert.Contains("relógio", all, "the clock of Zurvan");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        [Test]
        public void CityQuests_AreGivenByUtnapishtimAndSinLeqi_OnExistingEnemies_WithAReedBoatAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.DurJarras).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "utnapishtim", "sin_leqi_unninni" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.ReedBoat), "Sin-leqi-unninni pays with the reed boat he keeps for the flood");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("utnapishtim", active.Contains, done.Contains, out CityQuestSpec lions));
            Assert.AreEqual(CityQuestCatalog.DurJarrasLions, lions.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("sin_leqi_unninni", active.Contains, done.Contains, out _), "the chain starts with Utnapishtim");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("atrahasis", active.Contains, done.Contains, out _), "Atrahasis gives no quest");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("siduri", active.Contains, done.Contains, out _), "Siduri gives no quest");
            done.Add(CityQuestCatalog.DurJarrasLions);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("sin_leqi_unninni", active.Contains, done.Contains, out CityQuestSpec beetles));
            Assert.AreEqual(CityQuestCatalog.DurJarrasBeetles, beetles.QuestId);
        }
    }
}
