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
    /// Kirta, the Cidade-Recife, as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 28): its
    /// <see cref="CitySpec"/> among the race cities, the four sea folk of the ficha at their doors with their own look (fins,
    /// gills, nacre or reef skin), the CoralReef style (coral towers, the mother coral, shell bridges, kelp gardens, the reef
    /// edge over the abyss), the whole town inside a circle of radius ~22.5 m at y = 0 with its three roads out (W, E, N),
    /// Danel's diving tools with the shell of map 28 and the air bubble of map 29, the two city quests and the respawn of 28–29.
    /// </summary>
    public class CityKirtaTests
    {
        private static readonly string[] Cast = { "kirta", "pughat", "danel", "aqhat" };
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
        public void Kirta_HasACitySpec_AsARaceCity_SameIdAsItsMapAndRecipe()
        {
            CitySpec kirta = CityCatalog.Get(CityCatalog.Kirta);
            Assert.IsNotNull(kirta, "Kirta has a CitySpec");
            Assert.AreEqual(BiomeType.Forest, kirta.Region, "the Costa das Monções borrows the Forest (always unlocked), as Taa Pwani");
            Assert.AreEqual(MapCatalog.Kirta, kirta.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.KirtaCityId, kirta.CityId, "the recipe's city id");
            Assert.IsTrue(CityCatalog.IsRaceCity(kirta));
            Assert.IsFalse(CityCatalog.IsHubCity(kirta));
            Assert.IsFalse(CityCatalog.Cities.Contains(kirta), "the hub list keeps one city per region");
            Assert.IsFalse(kirta.HasGrandmaServices);
            Assert.IsFalse(kirta.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(kirta.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(kirta.CityId));
        }

        [Test]
        public void MapsOfKirta_RespawnInItsPlaza()
        {
            Assert.AreEqual(CityCatalog.Kirta, MapCatalog.RespawnCityFor("map_028_kirta").CityId);
            Assert.AreEqual(CityCatalog.Kirta, MapCatalog.RespawnCityFor("map_029_kilwa").CityId);
            Assert.AreEqual(CityCatalog.TaaPwani, MapCatalog.RespawnCityFor("map_030_liyongo").CityId, "30 still wakes in Taa Pwani");

            var cities = new CityProgress();
            CitySpec kirta = CityCatalog.Get(CityCatalog.Kirta);
            Assert.AreSame(kirta, cities.ApplyDefeatIn(kirta), "a defeat on 28/29 wakes the cousins by the mother coral");
            Assert.AreSame(kirta, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void Kirta_IsACoralReefTown_WithCoralTowersTheMotherCoralBridgesAndTheReefEdge()
        {
            CityTownRecipe recipe = CityTownRecipes.Kirta();
            Assert.AreSame(TownStyles.CoralReef, recipe.Style);
            Assert.AreEqual(TownArchitecture.CoralReef, TownStyles.CoralReef.Architecture);
            Assert.AreEqual(BiomeType.Forest, TownStyles.CoralReef.Region);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Kirta);
            Assert.AreSame(TownStyles.CoralReef, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CoralMother), "the mother coral");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ShellBridge), 2, "shell bridges over the reef water");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.KelpGarden), 2, "kelp gardens");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.CoralSpire), 1);
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.ReefEdge), 1, "the reef edge over the abyss");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.ShellBridge));
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.ReefEdge));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.ShellBridge), layout.WalkBounds), "the bridges are walkable");
            Assert.IsTrue(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.ReefEdge), layout.WalkBounds).HasValue, "the reef edge stops the walker");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.ReefTower && !l.Building.ReedHall),
                "every house is a coral tower");
            Assert.AreEqual(TownShopDressing.DivingGear, recipe.Lots.Single(l => l.ResidentId == "danel").Spec.Dressing);
            Assert.IsFalse(layout.Streets.Any(s => s.Arm == CityArm.South), "no street to the south: the abyss");
            Assert.Greater(TownPalette.GlowOf(TownSwatch.CoralLive), 0f, "the relit coral glows at night");
            Assert.AreEqual(0f, TownPalette.GlowOf(TownSwatch.CoralDim), "the dimmed coral does not");
        }

        [Test]
        public void Kirta_FitsACircleOfRadius22_AtGroundLevel_WithThreeRoadsOut()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Kirta);
            float radius = CityTownRecipes.KirtaSolidRadius;
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
                Assert.AreEqual(CityTownRecipes.KirtaArmLength, street.Length, 0.001f, street.Arm.ToString());
            }

            List<CityExit> exits = layout.Exits.Where(e => e.Kind == CityExitKind.ToMap).ToList();
            Assert.AreEqual(3, exits.Count);
            foreach (CityExit exit in exits)
            {
                Assert.LessOrEqual(exit.Target.magnitude, radius, exit.Id);
                Assert.IsTrue(layout.WalkBounds.Contains(exit.Target), exit.Id);
            }

            Assert.AreEqual(CityTownRecipes.KirtaHalfExtent, layout.WalkBounds.xMax, 0.001f);
            Assert.AreEqual(CityArm.West, CityTownRecipes.Kirta().EntryArm, "the road in along the atoll");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void Kirta_Residents_AreTheFichasSeaFolk_WithALookOfTheirOwn()
        {
            CitySpec kirta = CityCatalog.Get(CityCatalog.Kirta);
            CollectionAssert.AreEquivalent(Cast, kirta.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.Gills, $"{id}: fins and gills");
                Assert.IsFalse(look.Tusks || look.ElfEars || look.Snout || look.Horns);
                Assert.IsTrue(look.Skin == NpcSwatch.SkinReef || look.Skin == NpcSwatch.SkinNacre, $"{id}: reef-turquoise or mother-of-pearl skin");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                StringAssert.EndsWith("/reef", look.SilhouetteKey);
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
                Assert.IsNotNull(NpcModelBuilder.Build(look), $"{id} builds a model");
            }

            Assert.AreEqual(CityShopCatalog.MergulhoDanel, kirta.Residents.Single(r => r.ResidentId == "danel").ShopId, "Danel keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.MergulhoDanel }, kirta.ShopIds.ToArray());
        }

        [Test]
        public void Kirta_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Kirta, o Rei do Recife", CityTexts.ResidentName(null, Resident("kirta")));
            Assert.AreEqual("Danel, o Juiz Justo", CityTexts.ResidentName(null, Resident("danel")));
        }

        [Test]
        public void Kirta_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.Kirta, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.Kirta);
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

            Assert.AreEqual("mergulho_danel", layout.LotOfShop(CityShopCatalog.MergulhoDanel).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn on the plaza");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Danel's diving tools
        // ------------------------------------------------------------------

        [Test]
        public void DivingTools_SellTheShellOf28_AndTheAirBubbleOf29_AtBalancedPrices_AndTheCounterTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.MergulhoDanel);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Forest, shop.HomeRegion);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            string shell = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 28).SpecialRequirementTag);
            string bubble = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 29).SpecialRequirementTag);
            Assert.AreEqual(MapRequirementItems.BreathingShell, shell);
            Assert.AreEqual(MapRequirementItems.AirBubble, bubble);
            foreach (string gate in new[] { shell, bubble })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop.ShopId, gate);
                Assert.IsNotNull(entry, $"Danel sells {gate}");
                Assert.IsFalse(entry.Rotating);
                Assert.IsFalse(entry.Unlock.IsGated);
            }

            // Kirta stands on map 28: the shell is also on Mwana's counter in Taa Pwani, for the way in.
            Assert.IsNotNull(CityShopCatalog.EntryOf(CityShopCatalog.BazarMwana, shell), "the shell can be bought before reaching Kirta");

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
                Assert.Contains(shell, today);
                Assert.Contains(bubble, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the counter changes from day to day");
        }

        [Test]
        public void Kirta_OpensDanelsCounter_AndNeverTheCaravaneer()
        {
            CitySpec kirta = CityCatalog.Get(CityCatalog.Kirta);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(kirta, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.MergulhoDanel, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Kirta_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec kirta = CityCatalog.Get(CityCatalog.Kirta);
            var keys = new List<string> { kirta.NameKey, kirta.ButtonKey, kirta.FlavourKey, CityShopCatalog.Get(CityShopCatalog.MergulhoDanel).NameKey };
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
            StringAssert.Contains("coral", all, "the reef the sea folk tend");
            foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByKirtasResidents_WithTheAirBubbleAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.Kirta).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "kirta", "pughat" }, quests.Select(q => q.GiverResidentId).ToArray());
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

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.AirBubble), "Pughat pays with the air bubble (map 29)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_KirtaOffersFirst_PughatOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("kirta", isActive, isDone, out CityQuestSpec king));
            Assert.AreEqual(CityQuestCatalog.KirtaOoze, king.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("pughat", isActive, isDone, out _), "the chain starts with the king");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("danel", isActive, isDone, out _), "Danel gives no quest");

            done.Add(CityQuestCatalog.KirtaOoze);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("pughat", isActive, isDone, out CityQuestSpec rescue));
            Assert.AreEqual(CityQuestCatalog.KirtaRescue, rescue.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec ooze = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.KirtaOoze);
            Assert.AreEqual(ooze.OfferKey, CityQuestCatalog.Talk(quests, "kirta"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.KirtaOoze));
            Assert.AreEqual(ooze.ActiveKey, CityQuestCatalog.Talk(quests, "kirta"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "pughat"), "her quest waits for the king's");
            quests.Shutdown();
        }
    }
}
