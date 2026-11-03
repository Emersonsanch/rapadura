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
    /// The Acampamento de Soslan as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 47): its
    /// <see cref="CitySpec"/> among the race cities, the five centaurs of the ficha (Soslan, Batradz, Satana, Syrdon and
    /// Uryzmæg) at their doors with their own look (a horse from the waist down, short felt tunics), the SteppeCamp style (felt
    /// tents on wagons, the stone oven, horsehair standards, felt carts, rope corrals, the gate of two standards), the whole
    /// town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), Syrdon's saddlery (never the life
    /// or chaos stones), the two city quests and the respawn of the maps whose return city it is (47, 48, 51).
    /// </summary>
    public class CitySoslanTests
    {
        private static readonly string[] Cast = { "soslan", "batradz", "satana", "syrdon", "uryzmaeg" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "soslan", "Soslan" }, { "batradz", "Batradz" }, { "satana", "Satana" }, { "syrdon", "Syrdon" }, { "uryzmaeg", "Uryzmæg" },
        };

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        /// <summary>Distance from the plaza centre to the farthest point of <paramref name="solid"/>.</summary>
        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void Soslan_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec soslan = CityCatalog.Get(CityCatalog.AcampamentoSoslan);
            Assert.IsNotNull(soslan, "the Acampamento de Soslan has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, soslan.Region, "the Estepe dos Nartes borrows the Forest (always unlocked)");
            Assert.AreEqual(MapCatalog.AcampamentoSoslan, soslan.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.SoslanCityId, soslan.CityId, "the recipe's city id");

            Assert.IsTrue(CityCatalog.IsRaceCity(soslan));
            Assert.IsFalse(CityCatalog.IsLateRegionCity(soslan));
            Assert.IsFalse(CityCatalog.IsHubCity(soslan));
            Assert.IsFalse(CityCatalog.Cities.Contains(soslan), "the hub list keeps one city per region");
            Assert.IsFalse(soslan.HasGrandmaServices);
            Assert.IsFalse(soslan.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(soslan.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(soslan.CityId));
        }

        [Test]
        public void MapsOfTheCamp_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.AcampamentoSoslan).ToList();
            CollectionAssert.IsSubsetOf(maps.Select(m => m.MapId).ToArray(), new[] { 47, 48, 51 }, "the camp is the return city of 47, 48 and 51 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.AcampamentoSoslan, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.AreEqual(CityCatalog.AcampamentoSoslan, SafeZoneRules.RespawnCity("map_047_soslan", _ => CityCatalog.AcampamentoSoslan, CityCatalog.VilaZefa),
                "a map naming the camp wakes the hero there");
            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_047_soslan", _ => CityCatalog.AcampamentoSoslan, CityCatalog.VilaZefa)));

            var cities = new CityProgress();
            CitySpec soslan = CityCatalog.Get(CityCatalog.AcampamentoSoslan);
            Assert.AreSame(soslan, cities.ApplyDefeatIn(soslan), "a defeat on 47 wakes the cousins by the stone oven");
            Assert.AreSame(soslan, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Soslan_IsASteppeCamp_WithFeltTentsOnWagonsTheOvenStandardsCartsAndCorrals()
        {
            CityTownRecipe recipe = CityTownRecipes.Soslan();
            Assert.AreSame(TownStyles.SteppeCamp, recipe.Style);
            Assert.AreEqual(TownArchitecture.SteppeCamp, TownStyles.SteppeCamp.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.SteppeCamp.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.AcampamentoSoslan);
            Assert.AreSame(TownStyles.SteppeCamp, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StoneOven), "the collective stone oven");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.RopeCorral), 3, "the corrals in three runs");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.HorseStandard), 3, "horsehair standards");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.FeltWagon), 3, "felt carts");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.RopeCorral));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.StoneOven));
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.RopeCorral), layout.WalkBounds).HasValue, "the corral stops the walker");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.StoneOven), layout.WalkBounds).HasValue, "the oven stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.FeltYurt && !l.Building.GiantRoundhouse && !l.Building.HollowTrunk && !l.Building.SpireHouse),
                "every house is a felt tent on a wagon");
            Assert.AreEqual(TownShopDressing.Saddlery, recipe.Lots.Single(l => l.ResidentId == "syrdon").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the corrals");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.CampFire), 0f, "the camp fire glows at night");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.Felt), "the felt does not");
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, Enum.GetValues(typeof(TownSwatch)).Length);

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.FeltWagon && p.Kind <= CityPropKind.StoneOven))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.SteppeCamp);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.SteppeCamp);
            Assert.Greater(gate.TriangleCount, 40, "the gate of two standards");
        }

        [Test]
        public void Soslan_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.AcampamentoSoslan);
            float radius = CityTownRecipes.SoslanSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld keeps a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.East, CityArm.South }, layout.Streets.Select(s => s.Arm).ToArray());
            foreach (CityStreet street in layout.Streets)
            {
                Assert.AreEqual(CityTownRecipes.SoslanArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.SoslanHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Soslan().EntryArm, "the road in across the steppe");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Soslan_Residents_AreTheFichas_CentaursWithALookOfTheirOwn()
        {
            CitySpec soslan = CityCatalog.Get(CityCatalog.AcampamentoSoslan);
            CollectionAssert.AreEquivalent(Cast, soslan.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.NartRed, NpcSwatch.NartBlue, NpcSwatch.SteppeFelt, NpcSwatch.ScythGold, NpcSwatch.Rawhide };
            var coats = new HashSet<NpcSwatch>();
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.HorseBody, $"{id}: a centaur");
                Assert.AreEqual(NpcGarment.Tunic, look.Garment, $"{id}: a short tunic over the horse");
                Assert.IsFalse(look.ElfEars || look.Tusks || look.Snout || look.Horns || look.Gills || look.Twigs, $"{id}: a centaur");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the camp");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                coats.Add(look.Coat);
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                NpcModel model = NpcModelBuilder.Build(look);
                Assert.IsNotNull(model, $"{id} builds a model");
                Assert.AreEqual(0f, model.Mesh.bounds.min.y, 0.02f, $"{id}: hooves on the ground");
                Assert.Less(model.Mesh.bounds.min.z, -0.6f, $"{id}: the horse half reaches back");
                UnityEngine.Object.DestroyImmediate(model.Mesh);
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(Cast.Length, coats.Count, "every centaur has a coat of its own");
            StringAssert.Contains("/centaur", NpcRecipes.Find("soslan").SilhouetteKey);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);
            Assert.AreEqual(NpcPalette.Count, Enum.GetValues(typeof(NpcSwatch)).Length);
            Assert.AreEqual(CityShopCatalog.SelariaNomade, soslan.Residents.Single(r => r.ResidentId == "syrdon").ShopId, "Syrdon keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.SelariaNomade }, soslan.ShopIds.ToArray());
        }

        [Test]
        public void Soslan_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Soslan, o de Pele de Aço", CityTexts.ResidentName(null, Resident("soslan")));
            Assert.AreEqual("Syrdon, o Trapaceiro", CityTexts.ResidentName(null, Resident("syrdon")));
        }

        [Test]
        public void Soslan_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.AcampamentoSoslan, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.AcampamentoSoslan);
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

            Assert.AreEqual("selaria_nomade", layout.LotOfShop(CityShopCatalog.SelariaNomade).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca rolls along with the camp");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Syrdon's counter
        // ------------------------------------------------------------------

        [Test]
        public void SyrdonsCounter_SellsFeltFeedAndBoots_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.SelariaNomade);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            foreach (string fixedItem in new[] { "item_manta_la", "item_mount_feed", "item_leather_boots" })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Syrdon sells {fixedItem}");
                Assert.IsFalse(entry.Rotating, $"{fixedItem} is always on the counter");
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
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 3));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                CollectionAssert.AreEqual(today, ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList(), "same day, same counter");
                Assert.Contains("item_manta_la", today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void Soslan_OpensSyrdonsCounter_AndNeverTheCaravaneer()
        {
            CitySpec soslan = CityCatalog.Get(CityCatalog.AcampamentoSoslan);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(soslan, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.SelariaNomade, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Soslan_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec soslan = CityCatalog.Get(CityCatalog.AcampamentoSoslan);
            var keys = new List<string> { soslan.NameKey, soslan.ButtonKey, soslan.FlavourKey, CityShopCatalog.Get(CityShopCatalog.SelariaNomade).NameKey };
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
            StringAssert.Contains("feltro", all, "the felt of the tents");
            StringAssert.Contains("potro", all, "the foals of the herd");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenBySoslanAndSatana_OnExistingEnemies()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.AcampamentoSoslan).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "soslan", "satana" }, quests.Select(q => q.GiverResidentId).ToArray());
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
                foreach (string suffix in new[] { ".title", ".description", ".journal", ".offer", ".active" })
                {
                    Assert.IsNotNull(CityQuestTexts.GetFallback(spec.KeyBase + suffix), spec.KeyBase + suffix);
                }
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == "item_lamparina_runica"), "Satana pays with a lamp of their own");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_SoslanOffersFirst_SatanaOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("soslan", isActive, isDone, out CityQuestSpec wolves));
            Assert.AreEqual(CityQuestCatalog.SoslanWolves, wolves.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("satana", isActive, isDone, out _), "the chain starts with Soslan");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("syrdon", isActive, isDone, out _), "Syrdon gives no quest");

            done.Add(CityQuestCatalog.SoslanWolves);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("satana", isActive, isDone, out CityQuestSpec eagles));
            Assert.AreEqual(CityQuestCatalog.SoslanEagles, eagles.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec first = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.SoslanWolves);
            Assert.AreEqual(first.OfferKey, CityQuestCatalog.Talk(quests, "soslan"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.SoslanWolves));
            Assert.AreEqual(first.ActiveKey, CityQuestCatalog.Talk(quests, "soslan"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "satana"), "her quest waits for Soslan's");
            quests.Shutdown();
        }
    }
}
