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
    /// The Vale dos Sátiros as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 53): its
    /// <see cref="CitySpec"/> among the race cities, the three satyrs of the ficha (Midas, Marsyas and Gordias: goat legs, goat
    /// horns, satyr skin) at their doors, the Glade style (gourd-shaped wicker huts, the stone stages of the dance, rows of
    /// vines, broad oaks, the river of golden pebbles with its fords, the leaf-tunnel gate), the whole town inside a circle of
    /// radius ~22.5 m at y = 0 with its three roads out (W, N, E), Marsyas' Flauta Dupla (never the life or chaos stones), the
    /// two city quests and the respawn of map 53.
    /// </summary>
    public class CityValeDosSatirosTests
    {
        private static readonly string[] Cast = { "midas", "marsyas", "gordias" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "midas", "Midas" }, { "marsyas", "Marsyas" }, { "gordias", "Gordias" },
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
        public void Vale_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec vale = CityCatalog.Get(CityCatalog.ValeDosSatiros);
            Assert.IsNotNull(vale, "the Vale dos Sátiros has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, vale.Region, "the Picos de Farnkesh borrow the Forest (always unlocked)");
            Assert.AreEqual("vale_dos_satiros", vale.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.ValeCityId, vale.CityId, "the recipe's city id");

            Assert.IsTrue(CityCatalog.IsRaceCity(vale));
            Assert.IsFalse(CityCatalog.IsLateRegionCity(vale));
            Assert.IsFalse(CityCatalog.IsHubCity(vale));
            Assert.IsFalse(vale.HasGrandmaServices);
            Assert.IsFalse(vale.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(vale.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(vale.CityId));
        }

        [Test]
        public void MapsOfTheVale_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.ValeDosSatiros).ToList();
            CollectionAssert.IsSubsetOf(maps.Select(m => m.MapId).ToArray(), new[] { 53 }, "the Vale is the return city of 53 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.ValeDosSatiros, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_053_vale_satiros", _ => CityCatalog.ValeDosSatiros, CityCatalog.VilaZefa)));

            var cities = new CityProgress();
            CitySpec vale = CityCatalog.Get(CityCatalog.ValeDosSatiros);
            Assert.AreSame(vale, cities.ApplyDefeatIn(vale), "a defeat on 53 wakes the cousins in the clearing of the dance");
            Assert.AreSame(vale, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Vale_IsAGlade_WithGourdHutsStonestagesVinesOaksAndTheGoldenRiver()
        {
            CityTownRecipe recipe = CityTownRecipes.ValeDosSatiros();
            Assert.AreSame(TownStyles.Glade, recipe.Style);
            Assert.AreEqual(TownArchitecture.Glade, TownStyles.Glade.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.ValeDosSatiros);
            Assert.AreSame(TownStyles.Glade, layout.Style);
            CityProp stage = recipe.PlazaProps.Single(p => p.Kind == CityPropKind.StoneStage && p.Position.magnitude < layout.PlazaRadius);
            Assert.AreEqual(0, stage.Variant, "the big stage of the dance on the plaza");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.StoneStage), 2, "clearings in a ring: more than one stage");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.GroveOak), 8, "the grove");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.GroveOak && p.Variant == 1), 1, "an oak with a double flute");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.VineRow), 2, "rows of vines");
            List<CityProp> rivers = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.GoldenRiver).ToList();
            Assert.GreaterOrEqual(rivers.Count, 2, "the river of golden pebbles and its branch");
            Assert.IsTrue(rivers.Any(p => (p.End - p.Position).magnitude > 30f), "the river runs across the whole valley");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.GoldenRiver));
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.VineRow));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.StoneStage));
            Assert.IsNull(CitySolids.PropSolid(rivers[0], layout.WalkBounds), "the fords of the river are walkable");
            Assert.IsTrue(CitySolids.PropSolid(stage, layout.WalkBounds).HasValue, "the stage stops the walker");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.VineRow), layout.WalkBounds).HasValue, "the vines stop the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.GourdHut && !l.Building.PinnacleNest && !l.Building.HollowTrunk),
                "every house is a gourd hut");
            Assert.AreEqual(TownShopDressing.DoubleFlute, recipe.Lots.Single(l => l.ResidentId == "marsyas").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.South), "no street to the south: the river and the grove");
            Assert.IsTrue(layout.Streets.Any(s => s.Arm == CityArm.North), "the climb to Farnkesh, to the north");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.PactolusGold), 0f, "the gold of the river glows a little");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.Firefly), 0f, "the fireflies of the dance glow");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.Wicker), "the wicker does not");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.GoldenRiver && p.Kind <= CityPropKind.GroveOak))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.Glade);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, 19f), new Vector3(2f, 0f, 19f), TownStyles.Glade);
            Assert.Greater(gate.TriangleCount, 40, "the leaf-tunnel gate");
        }

        [Test]
        public void TownPalette_AndNpcPalette_CoverEverySwatch()
        {
            Assert.AreEqual(TownPalette.Columns * TownPalette.Rows, TownPalette.Count);
            Assert.AreEqual(TownPalette.Count, Enum.GetValues(typeof(TownSwatch)).Length);
            Assert.AreEqual(NpcPalette.Columns * NpcPalette.Rows, NpcPalette.Count);
            Assert.AreEqual(NpcPalette.Count, Enum.GetValues(typeof(NpcSwatch)).Length);
        }

        [Test]
        public void Vale_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.ValeDosSatiros);
            float radius = CityTownRecipes.ValeSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld keeps a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.North, CityArm.East }, layout.Streets.Select(s => s.Arm).ToArray());
            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.ValeHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.ValeDosSatiros().EntryArm, "the way in");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Vale_Residents_AreTheFichasSatyrs_WithGoatLegsAndHornsOfTheirOwn()
        {
            CitySpec vale = CityCatalog.Get(CityCatalog.ValeDosSatiros);
            CollectionAssert.AreEquivalent(Cast, vale.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.Grape, NpcSwatch.PactolusGold, NpcSwatch.GladeGreen, NpcSwatch.VineGreen };
            var furs = new[] { NpcSwatch.FurBrown, NpcSwatch.FurTawny, NpcSwatch.FurBlack };
            Assert.IsFalse(NpcRecipes.Named.Any(r => r.Satyr && !Cast.Contains(r.Id)), "the satyrs live in the Vale");
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.Satyr, $"{id}: a satyr");
                Assert.IsTrue(look.Skin == NpcSwatch.SkinSatyr || look.Skin == NpcSwatch.SkinSatyrRuddy, $"{id}: satyr skin");
                Assert.Contains(look.Fur, furs, $"{id}: goat fur");
                Assert.AreEqual(NpcGarment.Tunic, look.Garment, $"{id}: a short tunic, so the goat legs show");
                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks || look.Twigs || look.Snout, $"{id}");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the vale");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            StringAssert.Contains("/satyr", NpcRecipes.Find("midas").SilhouetteKey);
            Assert.AreEqual(CityShopCatalog.FlautaDupla, vale.Residents.Single(r => r.ResidentId == "marsyas").ShopId, "Marsyas keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.FlautaDupla }, vale.ShopIds.ToArray());
        }

        [Test]
        public void Vale_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Midas, o Rei de Mãos Douradas", CityTexts.ResidentName(null, Resident("midas")));
        }

        [Test]
        public void Vale_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.ValeDosSatiros, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.ValeDosSatiros);
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

            Assert.AreEqual("flauta_dupla", layout.LotOfShop(CityShopCatalog.FlautaDupla).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // The Flauta Dupla
        // ------------------------------------------------------------------

        [Test]
        public void FlautaDupla_SellsWindRunesAndSereneAir_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.FlautaDupla);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            foreach (string fixedItem in new[] { "item_runa_vento", "item_garrafa_ar_sereno" })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Marsyas sells {fixedItem}");
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
                Assert.Contains("item_runa_vento", today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec vale = CityCatalog.Get(CityCatalog.ValeDosSatiros);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(vale, new DateTime(2026, 11, 6).AddDays(d), null);
                Assert.Contains(CityShopCatalog.FlautaDupla, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Vale_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec vale = CityCatalog.Get(CityCatalog.ValeDosSatiros);
            var keys = new List<string> { vale.NameKey, vale.ButtonKey, vale.FlavourKey, CityShopCatalog.Get(CityShopCatalog.FlautaDupla).NameKey };
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
            StringAssert.Contains("ouro", all, "Midas' gold");
            StringAssert.Contains("flauta", all, "the double flute");
            StringAssert.Contains("nó", all, "Gordias' knots");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByMidasAndGordias_OnExistingEnemies()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.ValeDosSatiros).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "midas", "gordias" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("midas", active.Contains, done.Contains, out CityQuestSpec bees));
            Assert.AreEqual(CityQuestCatalog.ValeGoldenBees, bees.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("gordias", active.Contains, done.Contains, out _), "the chain starts with the king");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("marsyas", active.Contains, done.Contains, out _), "Marsyas gives no quest");
            done.Add(CityQuestCatalog.ValeGoldenBees);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("gordias", active.Contains, done.Contains, out CityQuestSpec foxes));
            Assert.AreEqual(CityQuestCatalog.ValeVineFoxes, foxes.QuestId);
        }
    }
}
