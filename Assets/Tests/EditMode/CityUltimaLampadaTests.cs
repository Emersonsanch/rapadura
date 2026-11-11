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
    /// The Fortaleza da Última Lâmpada, the fortress of the Limiar Profundo, as a walkable capital (Docs/Cities.md, Docs/Mapas100.md map 83): its
    /// <see cref="CitySpec"/> among the late-region cities, the three people of the ficha (Pamour, Tehat, Makarios, in their own recipes) at their doors, the
    /// BasaltFort style (keeps of basalt with lit niches, the arc of the rampart with its five sealed gates of the pits, the Lâmpada-Mãe Grande, steles,
    /// racks of wicks and oil cisterns), the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), the Pavios da Tehat with
    /// the gear maps 83–90 ask for (never the life or chaos stones) and the two city quests; maps 83–90 respawn on its plaza and their gates are on Tehat's counter (MapRequirementTests).
    /// </summary>
    public class CityUltimaLampadaTests
    {
        private static readonly string[] Cast = { "pamour_lampadas", "tehat_pavios", "makarios_cartas" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        [Test]
        public void Fortaleza_HasACitySpec_AsALateRegionCity_SameIdAsItsRecipe()
        {
            CitySpec city = CityCatalog.Get(CityCatalog.FortalezaUltimaLampada);
            Assert.IsNotNull(city, "the Fortaleza has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, city.Region, "the Limiar Profundo borrows the Forest (always unlocked)");
            Assert.AreEqual("fortaleza_ultima_lampada", city.CityId, "the return city id of maps 83–90");
            Assert.AreEqual(CityTownRecipes.FortalezaUltimaLampadaCityId, city.CityId);

            Assert.IsTrue(CityCatalog.IsLateRegionCity(city));
            Assert.IsFalse(CityCatalog.IsRaceCity(city));
            Assert.IsFalse(CityCatalog.IsHubCity(city));
            Assert.IsFalse(city.HasGrandmaServices);
            Assert.IsFalse(city.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(city.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(city.CityId));

            var cities = new CityProgress();
            Assert.AreSame(city, cities.ApplyDefeatIn(city), "a defeat in the pits wakes the cousins at the Lâmpada-Mãe");
            Assert.AreSame(city, cities.ConsumeRespawnNotice());
        }

        [Test]
        public void MapsOfTheDeepThreshold_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.FortalezaUltimaLampada).ToList();
            CollectionAssert.AreEquivalent(new[] { 83, 84, 85, 86, 87, 88, 89, 90 }, maps.Select(m => m.MapId).ToArray(), "the Fortaleza is the return city of 83–90 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.FortalezaUltimaLampada, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.AreEqual(MapSettlementKind.Capital, MapCatalog.Get("map_083_fortaleza_ultima_lampada").Settlement);
            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_083_fortaleza_ultima_lampada", _ => CityCatalog.FortalezaUltimaLampada, CityCatalog.VilaZefa)));
        }

        [Test]
        public void Fortaleza_IsABasaltTown_WithTheRampartGatesMotherLampStelesRacksAndCisterns()
        {
            CityTownRecipe recipe = CityTownRecipes.FortalezaUltimaLampada();
            Assert.AreSame(TownStyles.BasaltFort, recipe.Style);
            Assert.AreEqual(TownArchitecture.BasaltFort, TownStyles.BasaltFort.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.FortalezaUltimaLampada);
            Assert.AreSame(TownStyles.BasaltFort, layout.Style);

            CityProp mother = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.MotherLamp);
            Assert.Less(mother.Position.magnitude, layout.PlazaRadius, "the Lâmpada-Mãe Grande stands on the plaza");
            Assert.Greater(mother.Position.magnitude, TownKit.MotherLampRadius + CityLayoutRules.WalkerRadius, "...but not on the respawn");
            Assert.IsTrue(CitySolids.PropSolid(mother, layout.WalkBounds).HasValue, "the lamp stops the walker");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.NicheStele), 4, "niche steles");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.WickRack), 2, "racks of wicks");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.OilCistern), 3, "oil cisterns");

            // The rampart is an arc to the north, with the five gates set into it, one for each dark element.
            List<CitySolid> arc = layout.Solids.Where(s => s.Owner == nameof(CityPropKind.LampRampart)).ToList();
            Assert.Greater(arc.Count, 10, "the rampart is a chain of colliders");
            Assert.IsTrue(arc.All(s => s.Center.y > 2f), "the rampart closes the north, never the roads");

            // The chain of colliders leaves no gap a hero could slip through.
            for (int i = 1; i < arc.Count; i++)
            {
                Assert.Less(Vector2.Distance(arc[i - 1].Center, arc[i].Center), arc[i].Radius * 2f, "the rampart has no holes");
            }

            List<CityProp> gates = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.CoveGate).ToList();
            Assert.AreEqual(5, gates.Count, "one gate for each pit");
            CollectionAssert.AreEquivalent(new[] { 0, 1, 2, 3, 4 }, gates.Select(g => g.Variant).ToArray());
            Assert.AreEqual(5, Enumerable.Range(0, 5).Select(TownKit.ElementSwatch).Distinct().Count(), "each pit has its own colour");
            foreach (CityProp gate in gates)
            {
                Assert.IsNull(CitySolids.PropSolid(gate, layout.WalkBounds), "the gate is the face of the rampart");
                Assert.AreEqual(TownKit.LampRampartRadius - TownKit.LampRampartHalfThickness, gate.Position.magnitude, 0.2f, "set into the rampart");
            }

            List<CityLot> houses = layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).ToList();
            Assert.IsTrue(houses.All(l => l.Building.BasaltKeep && !l.Building.ZigguratHall && !l.Building.PapyrusHall), "every house is of basalt");
            Assert.AreEqual(TownShopDressing.WickLoom, recipe.Lots.Single(l => l.ResidentId == "tehat_pavios").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the rampart closes it");
            Assert.AreEqual(TownSwatch.BasaltDust, TownStyles.BasaltFort.Ground);
            Assert.Greater(TownPalette.GlowOf(TownSwatch.ThousandLamps), 0.5f, "the thousand lamps glow");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.LampBasalt), "basalt does not glow");
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);

            foreach (CityLot lot in houses)
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.MotherLamp && p.Kind <= CityPropKind.OilCistern))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.BasaltFort);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gateMesh = new TownMeshBuilder();
            TownKit.Gate(gateMesh, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.BasaltFort);
            Assert.Greater(gateMesh.TriangleCount, 40, "the gate of two basalt piers");
        }

        [Test]
        public void Fortaleza_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.FortalezaUltimaLampada);
            float radius = CityTownRecipes.UltimaLampadaSolidRadius;
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

            Assert.AreEqual(CityTownRecipes.UltimaLampadaHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.FortalezaUltimaLampada().EntryArm, "the road up from the Grutas do Limiar");
        }

        [Test]
        public void Fortaleza_Residents_AreTheFichas_InRecipesOfTheirOwn()
        {
            CitySpec city = CityCatalog.Get(CityCatalog.FortalezaUltimaLampada);
            CollectionAssert.AreEquivalent(Cast, city.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.FortBasalt, NpcSwatch.MotherAmber, NpcSwatch.FortCopper, NpcSwatch.FortLinen, NpcSwatch.FortIndigo, NpcSwatch.WickThread, NpcSwatch.AbyssGrey, NpcSwatch.LampOilBrown };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Satyr || look.Snout, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the watch");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsFalse(look.Beard == NpcBeard.LongBraided && (look.Layers & NpcLayer.Cape) != 0, $"{id}: a long braided beard and a cape blow the budget");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.PaviosTehat, city.Residents.Single(r => r.ResidentId == "tehat_pavios").ShopId, "Tehat keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.PaviosTehat }, city.ShopIds.ToArray());
        }

        [Test]
        public void Fortaleza_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
            }

            Assert.AreEqual("Pamour, o Comandante das Lâmpadas", CityTexts.ResidentName(null, Resident("pamour_lampadas")));
            Assert.AreEqual("Tehat, a Tecelã de Pavios", CityTexts.ResidentName(null, Resident("tehat_pavios")));
            Assert.AreEqual("Makarios, o Escritor de Cartas", CityTexts.ResidentName(null, Resident("makarios_cartas")));
        }

        [Test]
        public void Fortaleza_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.FortalezaUltimaLampada, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            // Ammo's stall (PlazaRadius · (0.575, −0.675)): nothing solid in front of his counter.
            CityTownRecipe recipe = CityTownRecipes.FortalezaUltimaLampada();
            var stall = new Vector2(recipe.PlazaRadius * 0.575f, -recipe.PlazaRadius * 0.675f);
            CityLayout withAmmo = CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.FortalezaUltimaLampada, true));
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(withAmmo, stall + Vector2.up * 1.4f), CityLayoutRules.WalkerRadius, "the cousins can step up to Ammo's counter");

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.FortalezaUltimaLampada);
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

            Assert.AreEqual("oficina_de_pavios", layout.LotOfShop(CityShopCatalog.PaviosTehat).Id);
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        [Test]
        public void PaviosDaTehat_SellsTheGearOfThePits_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.PaviosTehat);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            var gear = new[]
            {
                "item_garrafa_ar_sereno", MapRequirementItems.Theriac, MapRequirementItems.CanvasWings, MapRequirementItems.StormCloak, MapRequirementItems.BreathingShell,
                MapRequirementItems.AirBubble, MapRequirementItems.OilLamp, MapRequirementItems.RunicLamp, MapRequirementItems.EternalFireOil, MapRequirementItems.BalancePole, MapRequirementItems.ClimbingRope,
            };
            foreach (string fixedItem in gear)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Tehat sells {fixedItem}");
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
                Assert.Contains(MapRequirementItems.RunicLamp, today);
                Assert.Contains(MapRequirementItems.AirBubble, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec city = CityCatalog.Get(CityCatalog.FortalezaUltimaLampada);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(city, new DateTime(2026, 11, 9).AddDays(d), null);
                Assert.Contains(CityShopCatalog.PaviosTehat, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        [Test]
        public void Fortaleza_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec city = CityCatalog.Get(CityCatalog.FortalezaUltimaLampada);
            var keys = new List<string> { city.NameKey, city.ButtonKey, city.FlavourKey, CityShopCatalog.Get(CityShopCatalog.PaviosTehat).NameKey };
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
            StringAssert.Contains("muralha", all, "the rampart of the fortress");
            StringAssert.Contains("pavio", all, "the wicks of Tehat");
            StringAssert.Contains("carta", all, "the letters of Makarios");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        [Test]
        public void CityQuests_AreGivenByPamourAndMakarios_OnExistingEnemies_WithARunicLampAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.HomeQuestsIn(CityCatalog.FortalezaUltimaLampada).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "pamour_lampadas", "makarios_cartas" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.RunicLamp), "Makarios pays with the lamp he reads by");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("pamour_lampadas", active.Contains, done.Contains, out CityQuestSpec bats));
            Assert.AreEqual(CityQuestCatalog.UltimaLampadaBats, bats.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("makarios_cartas", active.Contains, done.Contains, out _), "the chain starts with Pamour");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("tehat_pavios", active.Contains, done.Contains, out _), "Tehat gives no quest");
            done.Add(CityQuestCatalog.UltimaLampadaBats);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("makarios_cartas", active.Contains, done.Contains, out CityQuestSpec ushabti));
            Assert.AreEqual(CityQuestCatalog.UltimaLampadaUshabti, ushabti.QuestId);
        }
    }
}
