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
    /// The Fortim dos Gigantes-da-Colina as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 49): its
    /// <see cref="CitySpec"/> among the race cities, the three giants of the ficha (Baraqel, Virogdad and Shahmizad) at their
    /// doors with their own look (a head and shoulders over everyone), the HillFort style (round giant houses of dry stone
    /// under turf, the great pot, stone tables, giant spoons, earth ramparts in rings with a watch-tower, the trilithon gate),
    /// the whole town inside a circle of radius ~22.5 m at y = 0 with its two roads out (W, S), the Panela Grande with the
    /// crampons of the Corrente de Amirani (52; never the life or chaos stones), the two city quests and the respawn of the
    /// maps whose return city it is (49, 52).
    /// </summary>
    public class CityFortimTests
    {
        private static readonly string[] Cast = { "baraqel", "virogdad", "shahmizad" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "baraqel", "Baraqel" }, { "virogdad", "Virogdad" }, { "shahmizad", "Shahmizad" },
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
        public void Fortim_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec fortim = CityCatalog.Get(CityCatalog.Fortim);
            Assert.IsNotNull(fortim, "the Fortim has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, fortim.Region, "the Estepe dos Nartes borrows the Forest (always unlocked)");
            Assert.AreEqual(MapCatalog.Fortim, fortim.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.FortimCityId, fortim.CityId, "the recipe's city id");

            Assert.IsTrue(CityCatalog.IsRaceCity(fortim));
            Assert.IsFalse(CityCatalog.IsLateRegionCity(fortim));
            Assert.IsFalse(CityCatalog.IsHubCity(fortim));
            Assert.IsFalse(fortim.HasGrandmaServices);
            Assert.IsFalse(fortim.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(fortim.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(fortim.CityId));
        }

        [Test]
        public void MapsOfTheFort_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.Fortim).ToList();
            CollectionAssert.IsSubsetOf(maps.Select(m => m.MapId).ToArray(), new[] { 49, 52 }, "the Fortim is the return city of 49 and 52 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.Fortim, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_052_amirani", _ => CityCatalog.Fortim, CityCatalog.VilaZefa)));

            var cities = new CityProgress();
            CitySpec fortim = CityCatalog.Get(CityCatalog.Fortim);
            Assert.AreSame(fortim, cities.ApplyDefeatIn(fortim), "a defeat on 49 or 52 wakes the cousins by the great pot");
            Assert.AreSame(fortim, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Fortim_IsAHillFort_WithRoundGiantHousesThePotTablesSpoonsAndRamparts()
        {
            CityTownRecipe recipe = CityTownRecipes.Fortim();
            Assert.AreSame(TownStyles.HillFort, recipe.Style);
            Assert.AreEqual(TownArchitecture.HillFort, TownStyles.HillFort.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Fortim);
            Assert.AreSame(TownStyles.HillFort, layout.Style);
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.GiantPot), "the pot the size of a house");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.EarthRing), 4, "the ramparts: three runs to the north and the east slope");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.EarthRing && p.Variant == 1), "the watch-tower");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.GiantTable), 2, "stone tables");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.SpoonBench), 2, "giant spoons as benches");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.EarthRing));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.GiantPot));
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.EarthRing), layout.WalkBounds).HasValue, "the rampart stops the walker");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.GiantPot), layout.WalkBounds).HasValue, "the pot stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.GiantRoundhouse && !l.Building.FeltYurt && !l.Building.HollowTrunk),
                "every house is a round giant house");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.WallHeight >= 3.5f), "giant walls");
            Assert.AreEqual(TownShopDressing.GreatPot, recipe.Lots.Single(l => l.ResidentId == "shahmizad").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North || s.Arm == CityArm.East), "the rings to the north, the slope to the east");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.LivingFire), 0f, "the Living Fire glows at night");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.FortStone), "the stone does not");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.EarthRing && p.Kind <= CityPropKind.SpoonBench))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.HillFort);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.HillFort);
            Assert.Greater(gate.TriangleCount, 40, "the trilithon gate");
        }

        [Test]
        public void Fortim_FitsACircleOfRadius22_AtGroundLevel_WithTwoRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Fortim);
            float radius = CityTownRecipes.FortimSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld keeps a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.South }, layout.Streets.Select(s => s.Arm).ToArray());
            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(2, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.FortimHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Fortim().EntryArm, "the road up the hill");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Fortim_Residents_AreTheFichas_GiantsAHeadOverEveryone()
        {
            CitySpec fortim = CityCatalog.Get(CityCatalog.Fortim);
            CollectionAssert.AreEquivalent(Cast, fortim.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.HillEarth, NpcSwatch.HillStone, NpcSwatch.GoodSmoke, NpcSwatch.HillGreen, NpcSwatch.LivingEmber };
            float tallestOther = NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Max(r => r.Height);
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.GreaterOrEqual(look.Height, 1.4f, $"{id}: a giant");
                Assert.Greater(look.Height, tallestOther, $"{id}: taller than anyone outside the Fortim");
                Assert.IsTrue(look.Skin == NpcSwatch.SkinGiantRuddy || look.Skin == NpcSwatch.SkinGiantOchre, $"{id}: a giant's skin");
                Assert.IsFalse(look.HorseBody || look.Twigs || look.ElfEars || look.Horns, $"{id}: a giant");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the hill");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.PanelaGrande, fortim.Residents.Single(r => r.ResidentId == "shahmizad").ShopId, "Shahmizad keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.PanelaGrande }, fortim.ShopIds.ToArray());
        }

        [Test]
        public void Fortim_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Baraqel, o Chefe do Fortim", CityTexts.ResidentName(null, Resident("baraqel")));
        }

        [Test]
        public void Fortim_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Fortim, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Fortim);
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

            Assert.AreEqual("panela_grande", layout.LotOfShop(CityShopCatalog.PanelaGrande).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // The Panela Grande
        // ------------------------------------------------------------------

        [Test]
        public void PanelaGrande_SellsGroupFoodAndTheCramponsOf52_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.PanelaGrande);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            foreach (string fixedItem in new[] { "item_cooked_meat", MapRequirementItems.ClimbingCrampons })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Shahmizad sells {fixedItem}");
                Assert.IsFalse(entry.Rotating, $"{fixedItem} is always on the counter");
            }

            // Every item gate of the maps whose return city is the Fortim (today: the crampons of 52) is met by this counter.
            foreach (MapDefinition map in MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.Fortim && m.HasSpecialRequirement))
            {
                foreach (string itemId in MapRequirementRules.KitOf(map.SpecialRequirementTag))
                {
                    Assert.IsNotNull(CityShopCatalog.EntryOf(shop.ShopId, itemId), $"{map}: {itemId} on the Fortim's counter");
                }
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
                Assert.Contains(MapRequirementItems.ClimbingCrampons, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec fortim = CityCatalog.Get(CityCatalog.Fortim);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(fortim, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.PanelaGrande, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Fortim_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec fortim = CityCatalog.Get(CityCatalog.Fortim);
            var keys = new List<string> { fortim.NameKey, fortim.ButtonKey, fortim.FlavourKey, CityShopCatalog.Get(CityShopCatalog.PanelaGrande).NameKey };
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
            StringAssert.Contains("colher", all, "the giant spoons");
            StringAssert.Contains("gigante", all, "the giants");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByBaraqelAndVirogdad_OnExistingEnemies_AndPayTheCrampons()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.HomeQuestsIn(CityCatalog.Fortim).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "baraqel", "virogdad" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.ClimbingCrampons), "Virogdad pays with the crampons of 52");

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("baraqel", active.Contains, done.Contains, out CityQuestSpec boars));
            Assert.AreEqual(CityQuestCatalog.FortimBoars, boars.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("virogdad", active.Contains, done.Contains, out _), "the chain starts with the chief");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("shahmizad", active.Contains, done.Contains, out _), "Shahmizad gives no quest");
            done.Add(CityQuestCatalog.FortimBoars);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("virogdad", active.Contains, done.Contains, out CityQuestSpec crows));
            Assert.AreEqual(CityQuestCatalog.FortimCrows, crows.QuestId);
        }
    }
}
