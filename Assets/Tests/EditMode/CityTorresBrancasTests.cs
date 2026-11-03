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
    /// The Torres Brancas de Amanirenas as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 42): its
    /// <see cref="CitySpec"/> among the race cities, the five high elves of the ficha (Amanirenas, Amanitore, Natakamani,
    /// Shanakdakhete and Arkamani) at their doors with their own look (tall, dark skin, white hair, elf ears), the WhiteSpire
    /// style (white spire houses with glass domes and mirrors, the Relógio de Espelhos, the garden terraces with the spires,
    /// leaf bridges over the rills, mirror posts, small pyramids), the whole town inside a circle of radius ~22.5 m at y = 0
    /// with its three roads out (W, E, S), Natakamani's counter (the refining stones; the Sun Seal of the way in is sold in
    /// Mingcheng), the two city quests and the respawn of the maps whose return city it is.
    /// </summary>
    public class CityTorresBrancasTests
    {
        private static readonly string[] Cast = { "amanirenas", "amanitore", "natakamani", "shanakdakhete", "arkamani" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private static readonly Dictionary<string, string> FichaNames = new Dictionary<string, string>
        {
            { "amanirenas", "Rainha Amanirenas" }, { "amanitore", "Amanitore" }, { "natakamani", "Natakamani" },
            { "shanakdakhete", "Shanakdakhete" }, { "arkamani", "Arkamani" },
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
        public void TorresBrancas_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec torres = CityCatalog.Get(CityCatalog.TorresBrancas);
            Assert.IsNotNull(torres, "the Torres Brancas have a CitySpec");
            Assert.AreEqual(BiomeType.Forest, torres.Region, "the Planície de Mingcheng borrows the Forest (always unlocked)");
            Assert.AreEqual(MapCatalog.TorresBrancas, torres.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.TorresBrancasCityId, torres.CityId, "the recipe's city id");
            Assert.AreEqual(CityCatalog.TorresBrancas, MapCatalog.Get(42).ReturnCityId, "the return city of map 42");

            Assert.IsTrue(CityCatalog.IsRaceCity(torres));
            Assert.IsFalse(CityCatalog.IsLateRegionCity(torres));
            Assert.IsFalse(CityCatalog.IsHubCity(torres));
            Assert.IsFalse(CityCatalog.Cities.Contains(torres), "the hub list keeps one city per region");
            Assert.IsFalse(torres.HasGrandmaServices);
            Assert.IsFalse(torres.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(torres.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(torres.CityId));
        }

        [Test]
        public void MapsOfTheWhiteSpires_RespawnInTheirPlaza()
        {
            List<MapDefinition> maps = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.TorresBrancas).ToList();
            Assert.IsTrue(maps.Any(m => m.MapId == 42));
            foreach (MapDefinition map in maps)
            {
                Assert.AreEqual(CityCatalog.TorresBrancas, MapCatalog.RespawnCityFor(map.Id).CityId, map.Id);
            }

            Assert.AreEqual(CityCatalog.Mingcheng, MapCatalog.RespawnCityFor("map_041_mingcheng").CityId, "the capital still wakes in Mingcheng");

            var cities = new CityProgress();
            CitySpec torres = CityCatalog.Get(CityCatalog.TorresBrancas);
            Assert.AreSame(torres, cities.ApplyDefeatIn(torres), "a defeat on 42 wakes the cousins by the Relógio de Espelhos");
            Assert.AreSame(torres, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void TorresBrancas_IsAWhiteSpireTown_WithSpireHousesTheMirrorClockTheTerracesLeafBridgesMirrorsAndPyramids()
        {
            CityTownRecipe recipe = CityTownRecipes.TorresBrancas();
            Assert.AreSame(TownStyles.WhiteSpire, recipe.Style);
            Assert.AreEqual(TownArchitecture.WhiteSpire, TownStyles.WhiteSpire.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.WhiteSpire.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TorresBrancas);
            Assert.AreSame(TownStyles.WhiteSpire, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.MirrorClock), "the Relógio de Espelhos");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.SpireTerrace), 3, "the garden terraces in three runs");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.SpireTerrace && p.Variant == 1), "the tallest spire");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.LeafBridge), 2, "leaf bridges over the rills");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.MirrorPost), 3, "mirror posts");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.SpirePyramid), 2, "small pyramids (Meroë)");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.SpireTerrace));
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.LeafBridge));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.MirrorClock));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.LeafBridge), layout.WalkBounds), "the leaf bridges are walkable");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.SpireTerrace), layout.WalkBounds).HasValue, "the terrace wall stops the walker");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.MirrorClock), layout.WalkBounds).HasValue, "the clock stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.SpireHouse && !l.Building.RingWallHall && !l.Building.StoneTerrace),
                "every house is a white spire house");
            Assert.AreEqual(TownShopDressing.Mirrors, recipe.Lots.Single(l => l.ResidentId == "natakamani").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the garden terraces");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.SpireLight), 0f, "the light the mirrors pass down glows at night");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.SpireLime), "the lime does not");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps.Where(p => p.Kind >= CityPropKind.SpireTerrace && p.Kind <= CityPropKind.SpirePyramid))
            {
                var b = new TownMeshBuilder();
                TownKit.Prop(b, prop, TownStyles.WhiteSpire);
                Assert.Greater(b.TriangleCount, 10, $"{prop.Kind} built (almost) nothing");
                Assert.Less(b.TriangleCount, 4000, $"{prop.Kind} is over budget");
            }

            var gate = new TownMeshBuilder();
            TownKit.Gate(gate, new Vector3(-2f, 0f, -19f), new Vector3(2f, 0f, -19f), TownStyles.WhiteSpire);
            Assert.Greater(gate.TriangleCount, 40, "the pylon gate");
        }

        [Test]
        public void TorresBrancas_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TorresBrancas);
            float radius = CityTownRecipes.TorresBrancasSolidRadius;
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
                Assert.AreEqual(CityTownRecipes.TorresBrancasArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.TorresBrancasHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.TorresBrancas().EntryArm, "the road in from Mingcheng");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void TorresBrancas_Residents_AreTheFichas_HighElvesWithALookOfTheirOwn()
        {
            CitySpec torres = CityCatalog.Get(CityCatalog.TorresBrancas);
            CollectionAssert.AreEquivalent(Cast, torres.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var swatches = new[] { NpcSwatch.SpireLime, NpcSwatch.PaleGold, NpcSwatch.SkyBlue, NpcSwatch.SilverLeaf, NpcSwatch.MeroeSandstone, NpcSwatch.SpireLight, NpcSwatch.Carnelian, NpcSwatch.Ebony };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.ElfEars, $"{id}: a high elf");
                Assert.IsFalse(look.Tusks || look.Snout || look.Horns || look.Gills, $"{id}: an elf");
                Assert.AreEqual(NpcSwatch.HairWhite, look.HairColor, $"{id}: white hair (§3.3)");
                Assert.IsTrue(look.Skin == NpcSwatch.SkinDeep || look.Skin == NpcSwatch.SkinBrown, $"{id}: dark skin (§3.3)");
                Assert.IsTrue(swatches.Contains(look.Main) || swatches.Contains(look.LayerColor), $"{id}: dressed in the colours of the towers");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsNotNull(NpcModelBuilder.Build(look), $"{id} builds a model");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.EspelhosNatakamani, torres.Residents.Single(r => r.ResidentId == "natakamani").ShopId, "Natakamani keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.EspelhosNatakamani }, torres.ShopIds.ToArray());
        }

        [Test]
        public void TorresBrancas_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.StartsWith(FichaNames[id], pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Rainha Amanirenas, a Guardiã", CityTexts.ResidentName(null, Resident("amanirenas")));
            Assert.AreEqual("Natakamani, o Mestre dos Espelhos", CityTexts.ResidentName(null, Resident("natakamani")));
        }

        [Test]
        public void TorresBrancas_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.TorresBrancas, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.TorresBrancas);
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

            Assert.AreEqual("espelhos_natakamani", layout.LotOfShop(CityShopCatalog.EspelhosNatakamani).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Natakamani's counter
        // ------------------------------------------------------------------

        [Test]
        public void NatakamanisCounter_SellsTheRefiningStones_AtBalancedPrices_TurnsDaily_AndTheSunSealIsSoldInMingcheng()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.EspelhosNatakamani);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            foreach (string stone in new[] { "item_pedra_bencao", "item_pedra_alma", "item_runa_clareza" })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, stone);
                Assert.IsNotNull(entry, $"Natakamani sells {stone}");
                Assert.IsFalse(entry.Rotating, $"{stone} is always on the counter");
            }

            Assert.IsNull(CityShopCatalog.EntryOf(shop.ShopId, MapRequirementItems.SunSeal), "the Sun Seal of the way in is sold in Mingcheng");
            Assert.IsNotNull(CityShopCatalog.EntryOf(CityShopCatalog.LanternasMozak, MapRequirementItems.SunSeal));

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
                Assert.Contains("item_pedra_alma", today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void TorresBrancas_OpenNatakamanisCounter_AndNeverTheCaravaneer()
        {
            CitySpec torres = CityCatalog.Get(CityCatalog.TorresBrancas);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(torres, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.EspelhosNatakamani, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void TorresBrancas_HaveTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec torres = CityCatalog.Get(CityCatalog.TorresBrancas);
            var keys = new List<string> { torres.NameKey, torres.ButtonKey, torres.FlavourKey, CityShopCatalog.Get(CityShopCatalog.EspelhosNatakamani).NameKey };
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
            StringAssert.Contains("espelho", all, "the mirrors of the towers");
            StringAssert.Contains("torre", all, "the white spires");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByTheQueenAndShanakdakhete_OnExistingEnemies()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.TorresBrancas).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "amanirenas", "shanakdakhete" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == "item_lamparina_runica"), "Shanakdakhete pays with a rune lamp");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_AmanirenasOffersFirst_ShanakdakheteOnlyAfterHerQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("amanirenas", isActive, isDone, out CityQuestSpec mirrors));
            Assert.AreEqual(CityQuestCatalog.TorresMirrors, mirrors.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("shanakdakhete", isActive, isDone, out _), "the chain starts with the queen");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("natakamani", isActive, isDone, out _), "Natakamani gives no quest");

            done.Add(CityQuestCatalog.TorresMirrors);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("shanakdakhete", isActive, isDone, out CityQuestSpec ibises));
            Assert.AreEqual(CityQuestCatalog.TorresIbises, ibises.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec first = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.TorresMirrors);
            Assert.AreEqual(first.OfferKey, CityQuestCatalog.Talk(quests, "amanirenas"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.TorresMirrors));
            Assert.AreEqual(first.ActiveKey, CityQuestCatalog.Talk(quests, "amanirenas"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "shanakdakhete"), "her quest waits for the queen's");
            quests.Shutdown();
        }
    }
}
