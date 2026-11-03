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
    /// The Palafitas de Tarchon as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 33): its
    /// <see cref="CitySpec"/> among the race cities, the five trols of the ficha (Tarchon, Ramtha, Larthia, Vel and Thanchvil)
    /// at their doors with their own look (tall, moss-blue or bark-green), the StiltVillage style (long houses on piles, the
    /// Mastro dos Fogos-Fátuos, boardwalks over the bog, giant baskets, net racks, reed islands, the open swamp in the north),
    /// the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, S), Larthia's counter with
    /// the gear of the gates 33–34, the two city quests and the respawn of the maps whose return city it is (33, 34).
    /// </summary>
    public class CityTarchonTests
    {
        private static readonly string[] Cast = { "tarchon", "ramtha", "larthia", "vel", "thanchvil" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

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
        public void Tarchon_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec tarchon = CityCatalog.Get(CityCatalog.PalafitasTarchon);
            Assert.IsNotNull(tarchon, "the Palafitas de Tarchon have a CitySpec");
            Assert.AreEqual(BiomeType.Forest, tarchon.Region, "the Delta de Jhilpur borrows the Forest (always unlocked), as Jhilpur");
            Assert.AreEqual(MapCatalog.PalafitasTarchon, tarchon.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.TarchonCityId, tarchon.CityId, "the recipe's city id");
            Assert.AreEqual(CityCatalog.PalafitasTarchon, MapCatalog.Get(33).ReturnCityId, "the race city of map 33");
            Assert.AreEqual(CityCatalog.PalafitasTarchon, MapCatalog.Get(34).ReturnCityId, "the return city of map 34");
            Assert.IsTrue(CityCatalog.IsRaceCity(tarchon));
            Assert.IsFalse(CityCatalog.IsLateRegionCity(tarchon));
            Assert.IsFalse(CityCatalog.IsHubCity(tarchon));
            Assert.IsFalse(CityCatalog.Cities.Contains(tarchon), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.VilaZefa, CityCatalog.ForRegion(BiomeType.Forest).CityId, "the Forest hub is still the Vila");
            Assert.IsFalse(tarchon.HasGrandmaServices);
            Assert.IsFalse(tarchon.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(tarchon.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(tarchon.CityId));
        }

        [Test]
        public void MapsOfTarchon_RespawnInItsPlaza()
        {
            Assert.AreEqual(CityCatalog.PalafitasTarchon, MapCatalog.RespawnCityFor("map_033_tarchon").CityId);
            Assert.AreEqual(CityCatalog.PalafitasTarchon, MapCatalog.RespawnCityFor("map_034_ninki_nanka").CityId);
            Assert.AreEqual(CityCatalog.Jhilpur, MapCatalog.RespawnCityFor("map_031_jhilpur").CityId, "the capital's maps still wake in Jhilpur");
            Assert.AreEqual(CityCatalog.Jhilpur, MapCatalog.RespawnCityFor("map_035_coracao_lodo").CityId);

            var cities = new CityProgress();
            CitySpec tarchon = CityCatalog.Get(CityCatalog.PalafitasTarchon);
            Assert.AreSame(tarchon, cities.ApplyDefeatIn(tarchon), "a defeat on 33/34 wakes the cousins by the wisp mast");
            Assert.AreSame(tarchon, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Tarchon_IsAStiltVillage_WithLonghousesTheWispMastBoardwalksBasketsAndTheSwamp()
        {
            CityTownRecipe recipe = CityTownRecipes.Tarchon();
            Assert.AreSame(TownStyles.StiltVillage, recipe.Style);
            Assert.AreEqual(TownArchitecture.StiltVillage, TownStyles.StiltVillage.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.StiltVillage.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.PalafitasTarchon);
            Assert.AreSame(TownStyles.StiltVillage, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.WispMast), "the Mastro dos Fogos-Fátuos");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Boardwalk), 2, "boardwalks over the bog channels");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.GiantBasket), 2, "giant baskets");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.NetRack), 1, "nets drying");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ReedIsland), 2, "floating reed islands");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.SwampEdge), 1, "the open swamp in the north");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.Boardwalk));
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.SwampEdge));
            Assert.IsFalse(TownKit.IsSegment(CityPropKind.WispMast));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.Boardwalk), layout.WalkBounds), "the boardwalks are walkable");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.SwampEdge), layout.WalkBounds).HasValue, "the rail of piles stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop)
                    .All(l => l.Building.StiltLonghouse && !l.Building.RootStilt && !l.Building.ReefTower && !l.Building.ReedHall),
                "every house is a long house on piles");
            Assert.AreEqual(TownShopDressing.BasketWeaver, recipe.Lots.Single(l => l.ResidentId == "larthia").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.North), "no street to the north: the open swamp");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.WillWisp), 0f, "the will-o'-wisps glow at night");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.BogWater), "the bog water does not");
            Assert.Greater(NpcPalette.GlowOf(NpcSwatch.WillWisp), 0f);

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
            }

            foreach (CityProp prop in recipe.PlazaProps)
            {
                var p = new TownMeshBuilder();
                TownKit.Prop(p, prop, layout.Style);
                Assert.Greater(p.TriangleCount, 0, $"{prop.Kind} at {prop.Position} draws nothing");
                Assert.Less(p.TriangleCount, 2500, $"{prop.Kind} is too heavy");
            }
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
        public void Tarchon_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.PalafitasTarchon);
            float radius = CityTownRecipes.TarchonSolidRadius;
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
                Assert.AreEqual(CityTownRecipes.TarchonArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.TarchonHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Tarchon().EntryArm, "the road in along the delta");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Tarchon_Residents_AreTheFichasTrols_TallWithALookOfTheirOwn()
        {
            CitySpec tarchon = CityCatalog.Get(CityCatalog.PalafitasTarchon);
            CollectionAssert.AreEquivalent(Cast, tarchon.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var tarchonSwatches = new[] { NpcSwatch.WetWood, NpcSwatch.SwampLeaf, NpcSwatch.LilacMist, NpcSwatch.WillWisp, NpcSwatch.Rush, NpcSwatch.NetTwine };
            float tallestHuman = NpcRecipes.Named.Where(r => r.Skin != NpcSwatch.SkinTrollMoss && r.Skin != NpcSwatch.SkinTrollBark && !r.Horns).Max(r => r.Height);
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.Skin == NpcSwatch.SkinTrollMoss || look.Skin == NpcSwatch.SkinTrollBark, $"{id}: moss-blue or bark-green trol skin");
                Assert.GreaterOrEqual(look.Height, 1.2f, $"{id}: trols are the tallest folk");
                Assert.GreaterOrEqual(look.Height, tallestHuman - 0.05f, $"{id}: as tall as the tallest human");
                Assert.IsFalse(look.ElfEars || look.Tusks || look.Snout || look.Horns || look.Gills, $"{id}: a trol, not another folk");
                Assert.IsTrue(tarchonSwatches.Contains(look.Main) || tarchonSwatches.Contains(look.LayerColor), $"{id}: dressed in the swamp's colours");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsNotNull(NpcModelBuilder.Build(look), $"{id} builds a model");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.CestariaLarthia, tarchon.Residents.Single(r => r.ResidentId == "larthia").ShopId, "Larthia keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.CestariaLarthia }, tarchon.ShopIds.ToArray());
        }

        [Test]
        public void Tarchon_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Tarchon, o Fundador", CityTexts.ResidentName(null, Resident("tarchon")));
            Assert.AreEqual("Larthia, a Cesteira", CityTexts.ResidentName(null, Resident("larthia")));
        }

        [Test]
        public void Tarchon_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.PalafitasTarchon, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.PalafitasTarchon);
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

            Assert.AreEqual("cestaria_larthia", layout.LotOfShop(CityShopCatalog.CestariaLarthia).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Larthia's counter
        // ------------------------------------------------------------------

        [Test]
        public void LarthiasCounter_SellsTheGearOfTheGates33And34_AtBalancedPrices_AndTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.CestariaLarthia);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            List<MapDefinition> gated = MapCatalog.Maps.Where(m => m.ReturnCityId == CityCatalog.PalafitasTarchon && m.HasSpecialRequirement).ToList();
            CollectionAssert.AreEquivalent(new[] { 33, 34 }, gated.Select(m => m.MapId).ToArray());
            List<string> gates = gated.Select(m => MapRequirementRules.PrimaryItemOf(m.SpecialRequirementTag)).ToList();
            CollectionAssert.AreEquivalent(new[] { MapRequirementItems.ReedBoat, MapRequirementItems.AntiVenomAmulet }, gates);
            foreach (string gate in gates)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, gate);
                Assert.IsNotNull(entry, $"Larthia sells {gate}");
                Assert.IsFalse(entry.Rotating, $"{gate} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, gate);
                Assert.IsNotNull(CityShopCatalog.EntryOf(CityShopCatalog.TrocasNuhzadag, gate), $"Nuhzadag still sells {gate} for the way in");
            }

            Assert.IsNotNull(CityShopCatalog.EntryOf(shop.ShopId, CityShopCatalog.BornalItemId), "Larthia's bags: the Alforje, one per save");
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
                foreach (string gate in gates)
                {
                    Assert.Contains(gate, today);
                }

                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void Tarchon_OpensLarthiasCounter_AndNeverTheCaravaneer()
        {
            CitySpec tarchon = CityCatalog.Get(CityCatalog.PalafitasTarchon);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(tarchon, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.CestariaLarthia, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Tarchon_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec tarchon = CityCatalog.Get(CityCatalog.PalafitasTarchon);
            var keys = new List<string> { tarchon.NameKey, tarchon.ButtonKey, tarchon.FlavourKey, CityShopCatalog.Get(CityShopCatalog.CestariaLarthia).NameKey };
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
            StringAssert.Contains("estaca", all, "the piles the city stands on");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByTarchonsResidents_WithTheAntivenomAmuletAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.PalafitasTarchon).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "tarchon", "vel" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.AntiVenomAmulet), "Vel pays with the antivenom amulet (map 34)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_TarchonOffersFirst_VelOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("tarchon", isActive, isDone, out CityQuestSpec wisps));
            Assert.AreEqual(CityQuestCatalog.TarchonWisps, wisps.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("vel", isActive, isDone, out _), "the chain starts with Tarchon");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("larthia", isActive, isDone, out _), "Larthia gives no quest");

            done.Add(CityQuestCatalog.TarchonWisps);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("vel", isActive, isDone, out CityQuestSpec nets));
            Assert.AreEqual(CityQuestCatalog.TarchonNets, nets.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec first = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.TarchonWisps);
            Assert.AreEqual(first.OfferKey, CityQuestCatalog.Talk(quests, "tarchon"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.TarchonWisps));
            Assert.AreEqual(first.ActiveKey, CityQuestCatalog.Talk(quests, "tarchon"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "vel"), "his quest waits for Tarchon's");
            quests.Shutdown();
        }
    }
}
