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
    /// The Mercado dos Túneis as a walkable race city (Docs/Cities.md, Docs/Mapas100.md §3.3 and map 22): its
    /// <see cref="CitySpec"/> among the race cities of the Grutas do Limiar, the four kobolds of the ficha at their doors with
    /// their own look (small, a muzzle, smooth scales), the TunnelBazaar style (niche stalls under arches, cart rails, carts,
    /// pulleys, air pipes with fans, the basalt tunnel wall), the whole market inside the place the Limiar overworld keeps
    /// for it with its four galleries, Miwnay's Feira with the eternal-fire oil of map 23, the two city quests and the
    /// respawn of 22–23.
    /// </summary>
    public class CityTunnelMarketTests
    {
        private static readonly string[] Cast = { "miwnay", "shayn", "farnkhund", "takhsich" };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        /// <summary>The overworld's place for the market (OverworldLayouts.Limiar: safe zone x −21…22, z −20…21, flat ground ±24).</summary>
        private static readonly Rect OverworldSafe = Rect.MinMaxRect(-21f, -20f, 22f, 21f);

        [TearDown]
        public void TearDown() => CityTownRecipes.ClearCache();

        private static CityResidentSpec Resident(string id) => CityCatalog.AllResidents().Single(r => r.ResidentId == id);

        // ------------------------------------------------------------------
        // The CitySpec
        // ------------------------------------------------------------------

        [Test]
        public void TunnelMarket_HasACitySpec_AsARaceCityOfTheCaves_SameIdAsItsMapAndRecipe()
        {
            CitySpec market = CityCatalog.Get(CityCatalog.MercadoDosTuneis);
            Assert.IsNotNull(market, "the Mercado dos Túneis has a CitySpec");
            Assert.AreEqual(BiomeType.Caves, market.Region);
            Assert.AreEqual(MapCatalog.MercadoDosTuneis, market.CityId, "the map's return city id");
            Assert.AreEqual(CityTownRecipes.TunnelMarketCityId, market.CityId, "the recipe's city id");
            Assert.IsTrue(CityCatalog.IsRaceCity(market));
            Assert.IsFalse(CityCatalog.IsHubCity(market));
            Assert.IsFalse(CityCatalog.Cities.Contains(market), "the hub list keeps one city per region");
            Assert.AreEqual(CityCatalog.Lamparinas, CityCatalog.ForRegion(BiomeType.Caves).CityId, "the Caves hub is still the Lamparinas");
            Assert.IsFalse(market.HasGrandmaServices);
            Assert.IsFalse(market.HasStoryVillagers);
            Assert.AreEqual(CityCatalog.AllCities.Count(), CityCatalog.AllCities.Select(c => c.CityId).Distinct().Count());
            Assert.IsNotNull(CityTownRecipes.HandRecipe(market.CityId), "walks on its own hand recipe");
            Assert.IsNotNull(CityTownRecipes.LayoutFor(market.CityId));
        }

        [Test]
        public void MapsOfTheMarket_RespawnInItsPlaza()
        {
            Assert.AreEqual(CityCatalog.MercadoDosTuneis, MapCatalog.RespawnCityFor("map_022_mercado_tuneis").CityId);
            Assert.AreEqual(CityCatalog.MercadoDosTuneis, MapCatalog.RespawnCityFor("map_023_canion_roda").CityId);
            Assert.AreEqual(CityCatalog.Lamparinas, MapCatalog.RespawnCityFor("map_021_pedras_dragao").CityId, "21 and 24 still wake in the Lamparinas");
            Assert.AreEqual(CityCatalog.Lamparinas, MapCatalog.RespawnCityFor("map_024_mahaway").CityId);

            var cities = new CityProgress();
            CitySpec market = CityCatalog.Get(CityCatalog.MercadoDosTuneis);
            Assert.AreSame(market, cities.ApplyDefeatIn(market), "a defeat on 22/23 wakes the cousins at the crossing");
            Assert.AreSame(market, cities.ConsumeRespawnNotice());
        }

        // ------------------------------------------------------------------
        // The town and its style
        // ------------------------------------------------------------------

        [Test]
        public void TunnelMarket_IsATunnelBazaar_WithRailsCartsPipesAndNicheStalls()
        {
            CityTownRecipe recipe = CityTownRecipes.TunnelMarket();
            Assert.AreSame(TownStyles.TunnelBazaar, recipe.Style);
            Assert.AreEqual(TownArchitecture.TunnelBazaar, TownStyles.TunnelBazaar.Architecture);
            Assert.AreEqual(BiomeType.Caves, TownStyles.TunnelBazaar.Region);
            Assert.AreEqual(TownRoofKind.Rock, TownStyles.TunnelBazaar.HouseRoof);

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.MercadoDosTuneis);
            Assert.AreSame(TownStyles.TunnelBazaar, layout.Style, "the hand-picked style reaches the layout");
            Assert.AreEqual(4, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.MineRails), "rails down every gallery");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.MineCart), 2);
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.AirPipe), 3, "the pipes that pull the Ar Sereno down");
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.NicheStall), 3);
            Assert.AreEqual(1, recipe.PlazaProps.Count(p => p.Kind == CityPropKind.Pulley));
            Assert.GreaterOrEqual(recipe.PlazaProps.Count(p => p.Kind == CityPropKind.RockWall && p.Variant == 1), 4, "the basalt tunnel wall");
            Assert.IsTrue(TownKit.IsSegment(CityPropKind.MineRails));
            Assert.IsNull(CitySolids.PropSolid(recipe.PlazaProps.First(p => p.Kind == CityPropKind.MineRails), layout.WalkBounds), "the rails are walkable");
            Assert.IsTrue(layout.Lots.Where(l => l.Kind == CityLotKind.House || l.Kind == CityLotKind.Shop).All(l => l.Building.TunnelStall && l.Building.Roof == TownRoofKind.Rock),
                "every house is a stall in an arched niche");
            Assert.AreEqual(TownShopDressing.AirBottles, recipe.Lots.Single(l => l.ResidentId == "miwnay").Spec.Dressing);
            Assert.AreEqual(TownShopDressing.Tinkerer, recipe.Lots.Single(l => l.ResidentId == "takhsich").Spec.Dressing);

            // The Lamparinas (the region's hub) keep their pink tuff.
            Assert.IsFalse(CityTownRecipes.LayoutFor(CityCatalog.Lamparinas).Lots.Any(l => l.Building != null && l.Building.TunnelStall));
        }

        [Test]
        public void TunnelMarket_FitsThePlaceTheLimiarOverworldKeeps_WithItsFourGalleries()
        {
            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.MercadoDosTuneis);
            foreach (CitySolid solid in layout.Solids)
            {
                Assert.IsTrue(CityGeometry.Contains(OverworldSafe, solid.Footprint), $"{solid.Owner} at {solid.Footprint} leaves the place the overworld keeps");
            }

            foreach (CityLot lot in layout.Lots)
            {
                Assert.IsTrue(CityGeometry.Contains(OverworldSafe, lot.Footprint), lot.Id);
            }

            foreach (CityStreet street in layout.Streets)
            {
                Assert.IsTrue(OverworldSafe.Contains(street.End), street.Arm.ToString());
            }

            foreach (CityExit exit in layout.Exits.Where(e => e.Kind == CityExitKind.ToMap))
            {
                Assert.IsTrue(OverworldSafe.Contains(exit.Target), exit.Id);
            }

            Assert.IsTrue(CityGeometry.Contains(OverworldSafe, layout.WalkBounds), "the walk box inside the overworld's safe zone");
            CollectionAssert.AreEquivalent(new[] { CityArm.North, CityArm.South, CityArm.East, CityArm.West }, layout.Streets.Select(s => s.Arm).ToArray(),
                "one street per gallery, where the overworld's galleries arrive");
            Assert.AreEqual(4, layout.Exits.Count(e => e.Kind == CityExitKind.ToMap));
            Assert.AreEqual(CityArm.North, CityTownRecipes.TunnelMarket().EntryArm, "the way in from the Lamparinas");
        }

        // ------------------------------------------------------------------
        // The cast
        // ------------------------------------------------------------------

        [Test]
        public void TunnelMarket_Residents_AreTheFichasKobolds_WithALookOfTheirOwn()
        {
            CitySpec market = CityCatalog.Get(CityCatalog.MercadoDosTuneis);
            CollectionAssert.AreEquivalent(Cast, market.Residents.Select(r => r.ResidentId).ToArray());
            var silhouettes = new HashSet<string>(NpcRecipes.Named.Where(r => !Cast.Contains(r.Id)).Select(r => r.SilhouetteKey));
            foreach (string id in Cast)
            {
                NpcRecipe look = NpcRecipes.Find(id);
                Assert.IsNotNull(look, $"{id} has no recipe");
                Assert.AreEqual(id, look.Id, $"{id}: its own recipe, not an alias");
                Assert.IsTrue(look.Snout, $"{id}: a kobold's muzzle");
                Assert.IsFalse(look.Tusks || look.ElfEars);
                Assert.IsTrue(look.Skin == NpcSwatch.SkinKoboldRust || look.Skin == NpcSwatch.SkinKoboldSlate, $"{id}: smooth rust or slate scales");
                Assert.Less(look.Height, 0.82f, $"{id}: kobolds are small");
                Assert.IsTrue(silhouettes.Add(look.SilhouetteKey), $"{id} shares a silhouette");
                StringAssert.EndsWith("/kobold", look.SilhouetteKey);
                Assert.IsFalse(string.IsNullOrEmpty(look.Story));
            }

            Assert.AreEqual(CityShopCatalog.FeiraMiwnay, market.Residents.Single(r => r.ResidentId == "miwnay").ShopId, "Miwnay keeps the city's counter");
            CollectionAssert.AreEqual(new[] { CityShopCatalog.FeiraMiwnay }, market.ShopIds.ToArray());
        }

        [Test]
        public void TunnelMarket_Names_AreTheFichas_NoFolkloreTitles()
        {
            foreach (string id in Cast)
            {
                string pt = CityTexts.ResidentName(null, Resident(id));
                StringAssert.DoesNotStartWith("Seu ", pt);
                StringAssert.DoesNotStartWith("Dona ", pt);
                StringAssert.DoesNotContain("Dona", pt);
                StringAssert.Contains(char.ToUpperInvariant(id[0]) + id.Substring(1), pt, $"{id}: named after the ficha ({pt})");
            }

            Assert.AreEqual("Miwnay, a Mestra da Feira", CityTexts.ResidentName(null, Resident("miwnay")));
            Assert.AreEqual("Takhsich, o Inventor", CityTexts.ResidentName(null, Resident("takhsich")));
        }

        [Test]
        public void TunnelMarket_Layout_PassesTheWholeValidator_AndTheCastStandsAtTheirDoors()
        {
            foreach (bool mascate in new[] { false, true })
            {
                List<string> problems = CityLayoutRules.Validate(CityLayoutGenerator.Generate(CityTownRecipes.RecipeFor(CityCatalog.MercadoDosTuneis, mascate)));
                Assert.IsEmpty(problems, (mascate ? "+caravaneer\n" : string.Empty) + string.Join("\n", problems));
            }

            CityLayout layout = CityTownRecipes.LayoutFor(CityCatalog.MercadoDosTuneis);
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

            Assert.AreEqual("feira_da_miwnay", layout.LotOfShop(CityShopCatalog.FeiraMiwnay).Id);
            Assert.AreEqual(1, layout.Exits.Count(e => e.Kind == CityExitKind.ShipDock), "the Cais da Barca");
            Assert.AreEqual(layout.PlazaCenter, layout.Respawn, "respawn at the crossing");
            Assert.GreaterOrEqual(CityLayoutRules.Clearance(layout, layout.Respawn), CityLayoutRules.WalkerRadius, "nothing stands on the respawn");
        }

        // ------------------------------------------------------------------
        // Miwnay's fair
        // ------------------------------------------------------------------

        [Test]
        public void FeiraDaMiwnay_SellsTheEternalFireOilOf23_AtBalancedPrices_AndItsCounterTurnsDaily()
        {
            CityShopSpec shop = CityShopCatalog.Get(CityShopCatalog.FeiraMiwnay);
            Assert.IsNotNull(shop);
            Assert.AreEqual(BiomeType.Caves, shop.HomeRegion);
            Assert.AreEqual(ShopTheme.Feira, shop.Theme);
            Assert.AreEqual(ShopRotationPeriod.Daily, shop.Period);

            // Map 22 has no gate; map 23 (Cânion da Roda, return city the Mercado) asks for the eternal-fire oil; map 24's
            // wings or double jump is met by Takhsich's canvas wings, sold here too (with the bottled Pure Air).
            Assert.IsTrue(string.IsNullOrEmpty(MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 22).SpecialRequirementTag)));
            string wings = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 24).SpecialRequirementTag);
            Assert.AreEqual(MapRequirementItems.CanvasWings, wings);
            Assert.IsNotNull(CityShopCatalog.EntryOf(shop.ShopId, wings), "Miwnay sells Takhsich's canvas wings");
            Assert.IsFalse(CityShopCatalog.EntryOf(shop.ShopId, wings).Rotating);
            Assert.IsNotNull(CityShopCatalog.EntryOf(shop.ShopId, "item_garrafa_ar_sereno"), "the kobolds sell bottled Pure Air");
            string oil = MapRequirementRules.PrimaryItemOf(MapCatalog.Maps.Single(m => m.MapId == 23).SpecialRequirementTag);
            Assert.AreEqual(MapRequirementItems.EternalFireOil, oil);
            CityShopEntry gate = CityShopCatalog.EntryOf(shop.ShopId, oil);
            Assert.IsNotNull(gate, "Miwnay sells the eternal-fire oil");
            Assert.IsFalse(gate.Rotating);
            Assert.IsFalse(gate.Unlock.IsGated);

            foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
            {
                ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                Assert.IsNotNull(item, entry.ItemId);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), entry.ItemId);
            }

            Assert.Greater(shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main), shop.RotatingSlots, "more lines than slots: the fair turns");
            long day = ShopRotation.DayIndex(new DateTime(2026, 11, 3));
            var counters = new HashSet<string>();
            for (int d = 0; d < 14; d++)
            {
                List<string> today = ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList();
                CollectionAssert.AreEqual(today, ShopRotation.PickStock(shop, day + d, null).Select(e => e.ItemId).ToList(), "same day, same counter");
                Assert.Contains(oil, today);
                counters.Add(string.Join(",", today));
            }

            Assert.Greater(counters.Count, 1, "the fair changes from day to day");
        }

        [Test]
        public void TunnelMarket_OpensMiwnaysFair_AndNeverTheCaravaneer()
        {
            CitySpec market = CityCatalog.Get(CityCatalog.MercadoDosTuneis);
            for (int d = 0; d < 28; d++)
            {
                List<string> open = CityShopService.ShopsOpenIn(market, new DateTime(2026, 11, 3).AddDays(d), null);
                Assert.Contains(CityShopCatalog.FeiraMiwnay, open);
                Assert.IsFalse(open.Contains(CityShopCatalog.MascateBento), "Ammo's mule stops at the region's hub only");
            }
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void TunnelMarket_HasTextsInBothLanguages_ALinePerCousin_AndTheFamilyIsRemembered()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);
            var byKey = CityTexts.Entries.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

            CitySpec market = CityCatalog.Get(CityCatalog.MercadoDosTuneis);
            var keys = new List<string> { market.NameKey, market.ButtonKey, market.FlavourKey, CityShopCatalog.Get(CityShopCatalog.FeiraMiwnay).NameKey };
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
            StringAssert.Contains("Ar Sereno", all, "the market's quiet air");
            foreach (string folk in new[] { "Saci", "Curupira", "Seu ", "Dona " })
            {
                StringAssert.DoesNotContain(folk, all);
            }
        }

        // ------------------------------------------------------------------
        // City quests
        // ------------------------------------------------------------------

        [Test]
        public void CityQuests_AreGivenByTheMarketsResidents_WithTheEternalFireOilAsTheLastReward()
        {
            List<CityQuestSpec> quests = CityQuestCatalog.QuestsIn(CityCatalog.MercadoDosTuneis).ToList();
            Assert.AreEqual(2, quests.Count);
            CollectionAssert.AreEqual(new[] { "takhsich", "farnkhund" }, quests.Select(q => q.GiverResidentId).ToArray());
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
            }

            Assert.IsTrue(quests[1].Quest.Rewards.Any(r => r.ItemId == MapRequirementItems.EternalFireOil), "Farnkhund pays with the eternal-fire oil (map 23)");
            Assert.AreEqual(CityQuestCatalog.Quests.Count, CityQuestCatalog.Quests.Select(q => q.QuestId).Distinct().Count());
        }

        [Test]
        public void CityQuests_TakhsichOffersFirst_FarnkhundOnlyAfterHisQuest_OnTheQuestManager()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            Func<string, bool> isActive = active.Contains;
            Func<string, bool> isDone = done.Contains;

            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("takhsich", isActive, isDone, out CityQuestSpec inventor));
            Assert.AreEqual(CityQuestCatalog.MercadoWebs, inventor.QuestId);
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("farnkhund", isActive, isDone, out _), "the chain starts with the inventor");
            Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide("miwnay", isActive, isDone, out _), "Miwnay gives no quest");

            done.Add(CityQuestCatalog.MercadoWebs);
            Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide("farnkhund", isActive, isDone, out CityQuestSpec driver));
            Assert.AreEqual(CityQuestCatalog.MercadoRails, driver.QuestId);

            var quests = new QuestManager();
            quests.Initialize();
            CityQuestCatalog.RegisterAll(quests);
            CityQuestSpec webs = CityQuestCatalog.Quests.Single(q => q.QuestId == CityQuestCatalog.MercadoWebs);
            Assert.AreEqual(webs.OfferKey, CityQuestCatalog.Talk(quests, "takhsich"));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.MercadoWebs));
            Assert.AreEqual(webs.ActiveKey, CityQuestCatalog.Talk(quests, "takhsich"));
            Assert.IsNull(CityQuestCatalog.Talk(quests, "farnkhund"), "his quest waits for the inventor's");
            quests.Shutdown();
        }
    }
}
