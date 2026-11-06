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
    /// Farnkesh, os Ninhos de Pedra, as a walkable capital (Docs/Cities.md, Docs/Mapas100.md map 54): its <see cref="CitySpec"/>
    /// among the late-region cities, the four people of the wind of the ficha and of the Manichaean mission to the east
    /// (Shad-Ohrmizd, Sisin, Tadi, Ardawan) at their doors, the PinnacleNest style (white nest-houses on sandstone pinnacles,
    /// the giant wind bell, rope basket lifts, gaps of cloud with plank-and-rope bridges, the gate of bell posts), the whole
    /// town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), the Cordas do Sisin with the gear of
    /// every gate of the Picos de Farnkesh that returns here (never the life or chaos stones), the two city quests and the
    /// respawn of maps 54–58.
    /// </summary>
    public class CityFarnkeshTests
    {
        private static readonly string[] Cast = { "shad_ohrmizd", "sisin", "tadi", "ardawan" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "shad_ohrmizd", "Shad-Ohrmizd" }, { "sisin", "Sisin" }, { "tadi", "Tadi" }, { "ardawan", "Ardawan" },
        };

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        private static float Reach(CitySolid solid) => solid.IsCapsule ? solid.Center.magnitude + solid.Radius : Reach(solid.Footprint);

        private static float Reach(Rect r) => new[] { new Vector2(r.xMin, r.yMin), new Vector2(r.xMin, r.yMax), new Vector2(r.xMax, r.yMin), new Vector2(r.xMax, r.yMax) }.Max(p => p.magnitude);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void Farnkesh_HasACitySpec_AsALateRegionCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec farnkesh = CityCatalog.Get(CityCatalog.Farnkesh);
            Assert.IsNotNull(farnkesh, "Farnkesh has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, farnkesh.Region, "the Picos de Farnkesh borrow the Forest (always unlocked)");
            Assert.AreEqual("farnkesh", farnkesh.CityId, "the maps' return city id");
            Assert.AreEqual(CityTownRecipes.FarnkeshCityId, farnkesh.CityId, "the recipe's city id");

            Assert.IsTrue(CityCatalog.IsLateRegionCity(farnkesh), "a capital of the late regions, as Mingcheng");
            Assert.IsFalse(CityCatalog.IsRaceCity(farnkesh));
            Assert.IsFalse(CityCatalog.IsHubCity(farnkesh));
            Assert.IsFalse(farnkesh.HasGrandmaServices);
            Assert.IsFalse(farnkesh.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(farnkesh.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(farnkesh.CityId));
        }

        [Test]
        public void MapsOfThePeaks_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.Farnkesh).ToList();
            CollectionAssert.IsSubsetOf(maps.Select(m => m.MapId).ToArray(), new[] { 54, 55, 56, 57, 58 }, "Farnkesh is the return city of 54–58 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.Farnkesh, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_054_farnkesh", _ => CityCatalog.Farnkesh, CityCatalog.VilaZefa)));

            var cities = new CityProgress();
            CitySpec farnkesh = CityCatalog.Get(CityCatalog.Farnkesh);
            Assert.AreSame(farnkesh, cities.ApplyDefeatIn(farnkesh), "a defeat on the peaks wakes the cousins under the wind bell");
            Assert.AreSame(farnkesh, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Farnkesh_IsAPinnacleNest_WithTheWindBellPinnaclesRopeBasketsAndCloudBridges()
        {
            CityTownRecipe recipe = CityTownRecipes.Farnkesh();
            Assert.AreSame(TownStyles.PinnacleNest, recipe.Style);
            Assert.AreEqual(TownArchitecture.PinnacleNest, TownStyles.PinnacleNest.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Farnkesh);
            Assert.AreSame(TownStyles.PinnacleNest, layout.Style);
            CityProp bell = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.WindBell);
            Assert.Less(bell.Position.magnitude, layout.PlazaRadius, "the giant wind bell hangs on the plaza");
            Assert.Greater(bell.Position.magnitude, TownKit.WindBellRadius + CityLayoutRules.WalkerRadius, "...but not on the respawn");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StonePinnacle), 9, "the nine pinnacles (and more) above the clouds");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StonePinnacle && p.Variant == 1), 1, "a pinnacle with a small nest-house on top");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.RopeBasket), 2, "the rope baskets Sisin pulls");
            List<CityProp> gaps = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.CloudGap).ToList();
            Assert.GreaterOrEqual(gaps.Count, 2, "gaps of cloud with rope bridges");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.CloudGap));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.StonePinnacle));
            Assert.IsNull(CitySolids.PropSolid(gaps[0], layout.WalkBounds), "the rope bridges are walkable");
            foreach (CityProp gap in gaps)
            {
                Assert.IsTrue(layout.Streets.Any(s => s.Road.Contains((gap.Position + gap.End) * 0.5f)), $"the bridge at {gap.Position} carries a street");
            }

            Assert.IsTrue(CitySolids.PropSolid(bell, layout.WalkBounds).HasValue, "the bell's plinth stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.PinnacleNest && !l.Building.GourdHut && !l.Building.SpireHouse),
                "every house is a nest on a pinnacle");
            Assert.AreEqual(TownShopDressing.RopeWalker, recipe.Lots.Single(l => l.ResidentId == "sisin").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the pinnacles in the clouds");
            Assert.AreEqual(TownSwatch.CloudWhite, TownStyles.PinnacleNest.Ground, "the ground between the pinnacles is the sea of clouds");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.NestLime), "the lime does not glow");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.WindBell && p.Kind <= CityPropKind.CloudGap))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.PinnacleNest);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.PinnacleNest);
            Assert.Greater(gate.TriangleCount, 40, "the gate of bell posts");
        }

        [Test]
        public void Farnkesh_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Farnkesh);
            float radius = CityTownRecipes.FarnkeshSolidRadius;
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
            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.FarnkeshHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.South, CityTownRecipes.Farnkesh().EntryArm, "the road up from the Vale dos Sátiros");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Farnkesh_Residents_AreTheFichas_PeopleOfTheWindWithALookOfTheirOwn()
        {
            CitySpec farnkesh = CityCatalog.Get(CityCatalog.Farnkesh);
            CollectionAssert.AreEquivalent(Cast, farnkesh.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.NestLime, NpcSwatch.NestRed, NpcSwatch.HighSky, NpcSwatch.YakRope };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Satyr || look.Snout, $"{id}: human");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the nests");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreNotSame(NpcRecipes.Find("sisinnios"), NpcRecipes.Find("sisin"), "Sisin of Farnkesh is not Sangkent's outfitter");
            Assert.AreEqual(CityShopCatalog.CordasSisin, farnkesh.Residents.Single(r => r.ResidentId == "sisin").ShopId, "Sisin keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.CordasSisin }, farnkesh.ShopIds.ToArray());
        }

        [Test]
        public void Farnkesh_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Shad-Ohrmizd, o Guardião dos Sinos", CityTexts.ResidentName(null, Resident("shad_ohrmizd")));
            Assert.AreEqual("Sisin, o Mestre das Pontes", CityTexts.ResidentName(null, Resident("sisin")));
        }

        [Test]
        public void Farnkesh_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Farnkesh, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Farnkesh);
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

            Assert.AreEqual("cordas_do_sisin", layout.LotOfShop(CityShopCatalog.CordasSisin).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // The Cordas do Sisin
        // ------------------------------------------------------------------

        [Test]
        public void CordasDoSisin_SellTheGearOfEveryGateOfThePeaks_AtBalancedPrices_TurnDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.CordasSisin);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            // The gates of the maps that return to Farnkesh (57 storm cloak, 58 canvas wings today): fixed from the first day.
            List<MapDefinition> gated = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.Farnkesh && m.HasSpecialRequirement).ToList();
            foreach (MapDefinition map in gated)
            {
                foreach (string itemId in MapRequirementRules.KitOf(map.SpecialRequirementTag))
                {
                    CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, itemId);
                    Assert.IsNotNull(entry, $"{map}: Sisin sells {itemId}");
                    Assert.IsFalse(entry.Rotating, $"{itemId} is always on the counter");
                    Assert.IsFalse(entry.Unlock.IsGated, $"{itemId} from the first day");
                }
            }

            foreach (string fixedItem in new[] { MapRequirementItems.StormCloak, MapRequirementItems.CanvasWings, MapRequirementItems.ClimbingRope, MapRequirementItems.BalancePole })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Sisin sells {fixedItem}");
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
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 6));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                Assert.Contains(MapRequirementItems.StormCloak, today);
                Assert.Contains(MapRequirementItems.CanvasWings, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec farnkesh = CityCatalog.Get(CityCatalog.Farnkesh);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(farnkesh, new DateTime(2026, 11, 6).AddDays(d), null);
                Assert.Contains(CityShopCatalog.CordasSisin, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Farnkesh_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec farnkesh = CityCatalog.Get(CityCatalog.Farnkesh);
            var keys = new List<string> { farnkesh.NameKey, farnkesh.ButtonKey, farnkesh.FlavourKey, CityShopCatalog.Get(CityShopCatalog.CordasSisin).NameKey };
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
            StringAssert.Contains("Vento Vivo", all, "the element of the capital");
            StringAssert.Contains("sino", all, "the wind bells");
            StringAssert.Contains("ponte", all, "the rope bridges");
            StringAssert.Contains("Vento Vivo", CityTexts.ResidentLine(null, Resident("shad_ohrmizd"), CharacterId.Maithe), "Maithe hears the Living Wind");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByShadOhrmizdAndTadi_OnExistingEnemies_WithTheStormCloakAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Farnkesh).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "shad_ohrmizd", "tadi" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.StormCloak), "Tadi pays with the storm cloak (map 57)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("shad_ohrmizd", active.Contains, done.Contains, out CityQuestSpec vultures));
            Assert.AreEqual(CityQuestCatalog.FarnkeshVultures, vultures.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("tadi", active.Contains, done.Contains, out _), "the chain starts with the keeper of the bells");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("sisin", active.Contains, done.Contains, out _), "Sisin gives no quest");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("ardawan", active.Contains, done.Contains, out _), "Ardawan gives no quest");
            done.Add(CityQuestCatalog.FarnkeshVultures);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("tadi", active.Contains, done.Contains, out CityQuestSpec mist));
            Assert.AreEqual(CityQuestCatalog.FarnkeshIllWind, mist.QuestId);
        }
    }
}
