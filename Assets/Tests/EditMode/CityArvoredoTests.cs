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
    /// The Arvoredo de Haripushpa as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 50): its
    /// <see cref="CitySpec"/> among the race cities, the two trunk folk of the ficha (Haripushpa and Suvarnapushpa, bark skin,
    /// moss hair and twigs on the crown) and the Sogdian herbalist passing through (Aryaman Puhr, human) at their doors, the
    /// LivingGrove style (houses in hollow trunks, the circle of mossy stones, larches, meltwater streams with log bridges,
    /// the root-arch gate), the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S),
    /// Suvarnapushpa's Viveiro (never the life or chaos stones), the two city quests and the respawn of map 50.
    /// </summary>
    public class CityArvoredoTests
    {
        private static readonly string[] Cast = { "haripushpa", "suvarnapushpa", "aryaman" };
        private static readonly string[] TrunkFolk = { "haripushpa", "suvarnapushpa" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "haripushpa", "Haripushpa" }, { "suvarnapushpa", "Suvarnapushpa" }, { "aryaman", "Aryaman" },
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
        public void Arvoredo_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec arvoredo = CityCatalog.Get(CityCatalog.Arvoredo);
            Assert.IsNotNull(arvoredo, "the Arvoredo has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, arvoredo.Region, "the Estepe dos Nartes borrows the Forest (always unlocked)");
            Assert.AreEqual(MapCatalog.Arvoredo, arvoredo.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.ArvoredoCityId, arvoredo.CityId, "the recipe's city id");

            Assert.IsTrue(CityCatalog.IsRaceCity(arvoredo));
            Assert.IsFalse(CityCatalog.IsLateRegionCity(arvoredo));
            Assert.IsFalse(CityCatalog.IsHubCity(arvoredo));
            Assert.IsFalse(arvoredo.HasGrandmaServices);
            Assert.IsFalse(arvoredo.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(arvoredo.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(arvoredo.CityId));
        }

        [Test]
        public void MapsOfTheGrove_RespawnInItsPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.Arvoredo).ToList();
            CollectionAssert.IsSubsetOf(maps.Select(m => m.MapId).ToArray(), new[] { 50 }, "the Arvoredo is the return city of 50 (Docs/Mapas100.md)");
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.Arvoredo, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.IsNotNull(CityCatalog.Get(SafeZoneRules.RespawnCity("map_050_haripushpa", _ => CityCatalog.Arvoredo, CityCatalog.VilaZefa)));

            var cities = new CityProgress();
            CitySpec arvoredo = CityCatalog.Get(CityCatalog.Arvoredo);
            Assert.AreSame(arvoredo, cities.ApplyDefeatIn(arvoredo), "a defeat on 50 wakes the cousins in the circle of stones");
            Assert.AreSame(arvoredo, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Arvoredo_IsALivingGrove_WithHollowTrunksTheStoneCircleLarchesAndMeltStreams()
        {
            CityTownRecipe recipe = CityTownRecipes.Arvoredo();
            Assert.AreSame(TownStyles.LivingGrove, recipe.Style);
            Assert.AreEqual(TownArchitecture.LivingGrove, TownStyles.LivingGrove.Architecture);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Arvoredo);
            Assert.AreSame(TownStyles.LivingGrove, layout.Style);
            List<CityProp> circle = recipe.PlazaProps.Where(p => p.Kind == CityPropKind.MossStone && p.Position.magnitude < layout.PlazaRadius).ToList();
            Assert.GreaterOrEqual(circle.Count, 6, "the circle of standing stones on the plaza");
            Assert.IsTrue(circle.All(p => Mathf.Abs(p.Position.magnitude - CityTownRecipes.ArvoredoStoneCircle) < 0.01f), "a true circle");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Larch), 8, "larches");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Larch && p.Variant == 1), 1, "an old one asleep");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.MeltStream), 2, "meltwater streams with log bridges");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.MeltStream));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.MossStone));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.MeltStream), layout.WalkBounds), "the log bridges are walkable");
            Assert.IsTrue(CitySolids.PropSolid(circle[0], layout.WalkBounds).HasValue, "a standing stone stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.HollowTrunk && !l.Building.FeltYurt && !l.Building.GiantRoundhouse),
                "every house is a hollow trunk");
            Assert.AreEqual(TownShopDressing.Nursery, recipe.Lots.Single(l => l.ResidentId == "suvarnapushpa").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the taiga");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.MeltWater), 0f, "the Living Water of the thaw glows a little");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.GroveBark), "the bark does not");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.MossStone && p.Kind <= CityPropKind.MeltStream))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.LivingGrove);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.LivingGrove);
            Assert.Greater(gate.TriangleCount, 40, "the root-arch gate");
        }

        [Test]
        public void Arvoredo_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Arvoredo);
            float radius = CityTownRecipes.ArvoredoSolidRadius;
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

            Assert.AreEqual(CityTownRecipes.ArvoredoHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Arvoredo().EntryArm, "the trail in from the steppe");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Arvoredo_Residents_AreTheFichas_TrunkFolkAndAHumanHerbalist()
        {
            CitySpec arvoredo = CityCatalog.Get(CityCatalog.Arvoredo);
            CollectionAssert.AreEquivalent(Cast, arvoredo.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.TrunkMoss, NpcSwatch.SpringLeaf, NpcSwatch.Catkin, NpcSwatch.LarchGreen };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                bool trunk = TrunkFolk.Contains(id);
                Assert.AreEqual(trunk, look.Twigs, $"{id}: twigs on the crown only for the trunk folk");
                if (trunk)
                {
                    Assert.IsTrue(look.Skin == NpcSwatch.SkinBarkOld || look.Skin == NpcSwatch.SkinBarkYoung, $"{id}: bark skin");
                    Assert.IsTrue(look.HairColor == NpcSwatch.TrunkMoss || look.HairColor == NpcSwatch.SpringLeaf, $"{id}: moss or leaf hair");
                }

                Assert.IsFalse(look.HorseBody || look.ElfEars || look.Horns || look.Gills || look.Tusks, $"{id}");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the grove");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            StringAssert.Contains("/trunk", NpcRecipes.Find("haripushpa").SilhouetteKey);
            Assert.AreEqual(CityShopCatalog.ViveiroSuvarnapushpa, arvoredo.Residents.Single(r => r.ResidentId == "suvarnapushpa").ShopId, "Suvarnapushpa keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.ViveiroSuvarnapushpa }, arvoredo.ShopIds.ToArray());
        }

        [Test]
        public void Arvoredo_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Haripushpa, o Mais Velho", CityTexts.ResidentName(null, Resident("haripushpa")));
        }

        [Test]
        public void Arvoredo_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Arvoredo, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Arvoredo);
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

            Assert.AreEqual("viveiro", layout.LotOfShop(CityShopCatalog.ViveiroSuvarnapushpa).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // The Viveiro
        // ------------------------------------------------------------------

        [Test]
        public void Viveiro_SellsHerbsFibreAndTheHealingRune_AtBalancedPrices_TurnsDaily_NeverTheLifeOrChaosStones()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.ViveiroSuvarnapushpa);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            foreach (string fixedItem in new[] { "item_herb", "item_fiber", "item_runa_cura" })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, fixedItem);
                Assert.IsNotNull(entry, $"Suvarnapushpa sells {fixedItem}");
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
                Assert.Contains("item_runa_cura", today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");

            CitySpec arvoredo = CityCatalog.Get(CityCatalog.Arvoredo);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(arvoredo, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.ViveiroSuvarnapushpa, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Arvoredo_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec arvoredo = CityCatalog.Get(CityCatalog.Arvoredo);
            var keys = new List<string> { arvoredo.NameKey, arvoredo.ButtonKey, arvoredo.FlavourKey, CityShopCatalog.Get(CityShopCatalog.ViveiroSuvarnapushpa).NameKey };
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
            StringAssert.Contains("árvore", all, "the trees");
            StringAssert.Contains("degelo", all, "the meltwater");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByHaripushpaAndAryaman_OnExistingEnemies()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Arvoredo).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "haripushpa", "aryaman" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("haripushpa", active.Contains, done.Contains, out CityQuestSpec bark));
            Assert.AreEqual(CityQuestCatalog.ArvoredoBarkEaters, bark.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("aryaman", active.Contains, done.Contains, out _), "the chain starts with the eldest");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("suvarnapushpa", active.Contains, done.Contains, out _), "Suvarnapushpa gives no quest");
            done.Add(CityQuestCatalog.ArvoredoBarkEaters);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("aryaman", active.Contains, done.Contains, out CityQuestSpec birds));
            Assert.AreEqual(CityQuestCatalog.ArvoredoSmokeBirds, birds.QuestId);
        }
    }
}
