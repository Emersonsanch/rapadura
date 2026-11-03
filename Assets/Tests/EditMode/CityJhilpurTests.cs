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
    /// Jhilpur, a Cidade nas Raízes, as a walkable late-region city (Docs/Cities.md, Docs/Mapas100.md §2.1 and map 31): its
    /// <see cref="CitySpec"/> among the late-region cities, the four residents of the ficha (Salmaios, Nuhzadag, Abiesous and
    /// Tehat) at their doors with their own look, the RootStilt style (houses on root-columns, the Raiz-Mãe, living-root
    /// bridges, lotus ponds, banyans, the ghats in the south), the whole town inside a circle of radius ~22.5 m at y = 0 with
    /// its three roads out (W, E, N), Nuhzadag's counter with the gear of every gate of the delta (32–35), the two city
    /// quests and the respawn of the maps whose return city it is (31, 32, 35).
    /// </summary>
    public class CityJhilpurTests
    {
        private static readonly string[] Cast = { "salmaios", "nuhzadag", "abiesous", "tehat" };
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
        public void Jhilpur_HasACitySpec_AsALateRegionCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec jhilpur = CityCatalog.Get(CityCatalog.Jhilpur);
            Assert.IsNotNull(jhilpur, "Jhilpur has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, jhilpur.Region, "the Delta de Jhilpur borrows the Forest (always unlocked), as Taa Pwani");
            Assert.AreEqual(MapCatalog.Jhilpur, jhilpur.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.JhilpurCityId, jhilpur.CityId, "the recipe's city id");
            Assert.AreEqual(CityCatalog.Jhilpur, MapCatalog.Get(31).ReturnCityId, "the capital of map 31");
            Assert.IsTrue(CityCatalog.IsLateRegionCity(jhilpur));
            Assert.IsFalse(CityCatalog.IsRaceCity(jhilpur));
            Assert.IsFalse(CityCatalog.IsHubCity(jhilpur));
            Assert.IsFalse(CityCatalog.Cities.Contains(jhilpur), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.VilaZefa, CityCatalog.ForRegion(BiomeType.Forest).CityId, "the Forest hub is still the Vila");
            Assert.IsFalse(jhilpur.HasGrandmaServices);
            Assert.IsFalse(jhilpur.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(jhilpur.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(jhilpur.CityId));
        }

        [Test]
        public void MapsOfJhilpur_RespawnInItsPlaza()
        {
            Assert.AreEqual(CityCatalog.Jhilpur, MapCatalog.RespawnCityFor("map_031_jhilpur").CityId);
            Assert.AreEqual(CityCatalog.Jhilpur, MapCatalog.RespawnCityFor("map_032_raiz_viva").CityId);
            Assert.AreEqual(CityCatalog.Jhilpur, MapCatalog.RespawnCityFor("map_035_coracao_lodo").CityId);
            Assert.AreEqual(CityCatalog.TaaPwani, MapCatalog.RespawnCityFor("map_025_taa_pwani").CityId, "the coast still wakes in Taa Pwani");

            var cities = new CityProgress();
            CitySpec jhilpur = CityCatalog.Get(CityCatalog.Jhilpur);
            Assert.AreSame(jhilpur, cities.ApplyDefeatIn(jhilpur), "a defeat on 31/32/35 wakes the cousins by the Raiz-Mãe");
            Assert.AreSame(jhilpur, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Jhilpur_IsARootStiltTown_WithStiltHousesTheMotherRootBridgesPondsAndGhats()
        {
            CityTownRecipe recipe = CityTownRecipes.Jhilpur();
            Assert.AreSame(TownStyles.RootStilt, recipe.Style);
            Assert.AreEqual(TownArchitecture.RootStilt, TownStyles.RootStilt.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.RootStilt.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Jhilpur);
            Assert.AreSame(TownStyles.RootStilt, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.MotherRoot), "the Raiz-Mãe");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.RootBridge), 2, "living-root bridges over the canals");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.LotusPond), 2, "lotus ponds");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Banyan), 2, "banyans");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Ghat), 1, "the ghats down to the water");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.RootBridge));
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.Ghat));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.RootBridge), layout.WalkBounds), "the bridges are walkable");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.Ghat), layout.WalkBounds).HasValue, "the ghat parapet stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.RootStilt && !l.Building.ReefTower && !l.Building.ReedHall),
                "every house stands on root-columns");
            Assert.AreEqual(TownShopDressing.BoatTrader, recipe.Lots.Single(l => l.ResidentId == "nuhzadag").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.South), "no street to the south: the ghats");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.LotusLantern), 0f, "the lotus lanterns glow at night");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.LivingWater), 0f, "the Living Water glows at night");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.MurkWater), "the murky water does not");

            foreach (CityLot lot in layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop))
            {
                var b = new TownMeshBuilder();
                TownBuildingBuilder.Build(b, lot.Building);
                Assert.Greater(b.TriangleCount, 40, $"{lot.Id} built (almost) nothing");
                Assert.Less(b.TriangleCount, 6000, $"{lot.Id} is over the per-building budget");
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
        public void Jhilpur_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Jhilpur);
            float radius = CityTownRecipes.JhilpurSolidRadius;
            Assert.LessOrEqual(radius, 24f, "the overworld keeps a circle of ~24 m");
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.LessOrEqual(Reach(solid), radius, $"{solid.Owner} at {solid.Footprint} leaves the circle");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.LessOrEqual(Reach(lot.Footprint), radius, lot.Id);
            }

            CollectionAssert.AreEquivalent(new[] { CityArm.West, CityArm.East, CityArm.North }, layout.Streets.Select(s => s.Arm).ToArray());
            foreach (CityStreet street in layout.Streets)
            {
                Assert.AreEqual(CityTownRecipes.JhilpurArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.JhilpurHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Jhilpur().EntryArm, "the road in along the delta");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Jhilpur_Residents_AreTheFichas_WithALookOfTheirOwn()
        {
            CitySpec jhilpur = CityCatalog.Get(CityCatalog.Jhilpur);
            CollectionAssert.AreEquivalent(Cast, jhilpur.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            var jhilpurSwatches = new[] { NpcSwatch.Turmeric, NpcSwatch.Sandalwood, NpcSwatch.Lotus, NpcSwatch.RootGreen, NpcSwatch.LivingWater, NpcSwatch.MurkWater, NpcSwatch.RootBark, NpcSwatch.Madder };
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsFalse(look.ElfEars || look.Tusks || look.Snout || look.Horns || look.Gills, $"{id}: human");
                Assert.IsTrue(jhilpurSwatches.Contains(look.Main) || jhilpurSwatches.Contains(look.LayerColor), $"{id}: dressed in the delta's colours");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsNotNull(NpcModelBuilder.Build(look), $"{id} builds a model");
                Assert.LessOrEqual(NpcModelBuilder.CountTriangles(look), 1500, $"{id} is over the mobile budget");
            }

            Assert.AreEqual(CityShopCatalog.TrocasNuhzadag, jhilpur.Residents.Single(r => r.ResidentId == "nuhzadag").ShopId, "Nuhzadag keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.TrocasNuhzadag }, jhilpur.ShopIds.ToArray());
        }

        [Test]
        public void Jhilpur_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Salmaios, o Guardião da Raiz-Mãe", CityTexts.ResidentName(null, Resident("salmaios")));
            Assert.AreEqual("Nuhzadag, o Barqueiro", CityTexts.ResidentName(null, Resident("nuhzadag")));
        }

        [Test]
        public void Jhilpur_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Jhilpur, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Jhilpur);
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

            Assert.AreEqual("trocas_nuhzadag", layout.LotOfShop(CityShopCatalog.TrocasNuhzadag).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Nuhzadag's counter
        // ------------------------------------------------------------------

        [Test]
        public void NuhzadagsCounter_SellsTheGearOfEveryGateOfTheDelta_AtBalancedPrices_AndTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.TrocasNuhzadag);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            List<MapDefinition> gated = MapCatalog.Maps.Where(m => m.Region == MapRegion.DeltaDeJhilpur && m.HasSpecialRequirement).ToList();
            CollectionAssert.AreEquivalent(new[] { 32, 33, 34, 35 }, gated.Select(m => m.MapId).ToArray());
            List<string> gates = gated.Select(m => MapRequirementRules.PrimaryItemOf(m.SpecialRequirementTag)).ToList();
            CollectionAssert.AreEquivalent(
                new[] { MapRequirementItems.BalancePole, MapRequirementItems.ReedBoat, MapRequirementItems.AntiVenomAmulet, MapRequirementItems.PurifyingFruit }, gates);
            foreach (string gate in gates)
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, gate);
                Assert.IsNotNull(entry, $"Nuhzadag sells {gate}");
                Assert.IsFalse(entry.Rotating, $"{gate} is always on the counter");
                Assert.IsFalse(entry.Unlock.IsGated, gate);
            }

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
        public void Jhilpur_OpensNuhzadagsCounter_AndNeverTheCaravaneer()
        {
            CitySpec jhilpur = CityCatalog.Get(CityCatalog.Jhilpur);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(jhilpur, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.TrocasNuhzadag, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Jhilpur_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec jhilpur = CityCatalog.Get(CityCatalog.Jhilpur);
            var keys = new List<string> { jhilpur.NameKey, jhilpur.ButtonKey, jhilpur.FlavourKey, CityShopCatalog.Get(CityShopCatalog.TrocasNuhzadag).NameKey };
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
            StringAssert.Contains("raiz", all, "the roots the city lives in");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByJhilpursResidents_WithThePurifyingFruitAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Jhilpur).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "salmaios", "abiesous" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.PurifyingFruit), "Abiesous pays with the purifying fruit (map 35)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_SalmaiosOffersFirst_AbiesousOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("salmaios", isActive, isDone, out CityQuestSpec ghats));
            Assert.AreEqual(CityQuestCatalog.JhilpurGhats, ghats.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("abiesous", isActive, isDone, out _), "the chain starts with Salmaios");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("nuhzadag", isActive, isDone, out _), "Nuhzadag gives no quest");

            done.Add(CityQuestCatalog.JhilpurGhats);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("abiesous", isActive, isDone, out CityQuestSpec oars));
            Assert.AreEqual(CityQuestCatalog.JhilpurOars, oars.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec first = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.JhilpurGhats);
            Assert.AreEqual(first.OfferKey, CityQuestCatalog.Talk(quests, "salmaios"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.JhilpurGhats));
            Assert.AreEqual(first.ActiveKey, CityQuestCatalog.Talk(quests, "salmaios"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "abiesous"), "his quest waits for Salmaios's");
            quests.Shutdown();
        }
    }
}
