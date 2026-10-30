using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Localization;
using Rapadura.Core.Managers;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Dressing;
using Rapadura.Save;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Cities of every region (TODO 12.3a / FASE 15 "Lojas"): the safe-zone rules (<see cref="SafeZoneRules"/>), the
    /// city and shop catalogs (completeness, items, prices, texts), the date-seeded shop rotation
    /// (<see cref="ShopRotation"/>, <see cref="CityShopService"/>), the carro de boi (<see cref="FastTravelRules"/>,
    /// <see cref="CityFastTravelPanel.BuildRow"/>), visits/respawn and the saved state (<see cref="CityProgress"/>), the
    /// generalised hub's rules and the map dressing geometry.
    /// </summary>
    public class CityTests
    {
        private const int TestSlot = 90161;

        private static readonly BiomeType[] Regions = { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));

        private GameObject _gameManagerObject;
        private GameManager _gm;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
            CityShopService.ClockOverride = null;
            CityShopService.HasHouseOverride = null;
            CityShopService.OutfitResolver = null;
            CityShopService.ClearCache();
        }

        [TearDown]
        public void TearDown()
        {
            if (_gm != null)
            {
                var managers = (List<IManager>)typeof(GameManager)
                    .GetField("_managers", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(_gm);
                foreach (IManager manager in managers)
                {
                    manager.Shutdown();
                }

                _gm = null;
            }

            if (_gameManagerObject != null)
            {
                Object.DestroyImmediate(_gameManagerObject);
            }

            CityShopService.ClockOverride = null;
            CityShopService.HasHouseOverride = null;
            CityShopService.OutfitResolver = null;
            CityShopService.ClearCache();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
            TestSaveRoot.Exit();
        }

        // ------------------------------------------------------------------
        // SafeZoneRules
        // ------------------------------------------------------------------

        [Test]
        public void InACity_NobodyAttacksAndNobodyIsHurt()
        {
            foreach (ZoneActor attacker in Enum.GetValues(typeof(ZoneActor)))
            {
                foreach (ZoneActor target in Enum.GetValues(typeof(ZoneActor)))
                {
                    Assert.IsFalse(SafeZoneRules.CanAttack(attacker, ZoneKind.City, target, ZoneKind.Wilds, pvpAllowed: true), $"{attacker} from a city");
                    Assert.IsFalse(SafeZoneRules.CanAttack(attacker, ZoneKind.Wilds, target, ZoneKind.City, pvpAllowed: true), $"{attacker} into a city");
                }

                Assert.IsFalse(SafeZoneRules.CanTakeDamage(attacker, ZoneKind.City), attacker.ToString());
            }

            Assert.IsFalse(SafeZoneRules.CanAttack(ZoneActor.Player, ZoneKind.City, ZoneActor.Player, ZoneKind.City, targetIsSelf: true), "not even yourself");
            Assert.IsFalse(SafeZoneRules.CanUseDamagingSkill(ZoneKind.City));
        }

        [Test]
        public void InTheWilds_CombatIsUnchanged()
        {
            Assert.IsTrue(SafeZoneRules.CanAttack(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Monster, ZoneKind.Wilds));
            Assert.IsTrue(SafeZoneRules.CanAttack(ZoneActor.Monster, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds));
            Assert.IsTrue(SafeZoneRules.CanAttack(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds, targetIsSelf: true));
            Assert.IsFalse(SafeZoneRules.CanAttack(ZoneActor.Monster, ZoneKind.Wilds, ZoneActor.Monster, ZoneKind.Wilds), "no infighting");
            Assert.IsFalse(SafeZoneRules.CanAttack(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Npc, ZoneKind.Wilds), "residents are never targets");
            Assert.IsFalse(SafeZoneRules.CanAttack(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds), "PvP off by default");
            Assert.IsTrue(SafeZoneRules.CanAttack(ZoneActor.Player, ZoneKind.Wilds, ZoneActor.Player, ZoneKind.Wilds, pvpAllowed: true), "PvP area / duel");
            Assert.IsTrue(SafeZoneRules.CanTakeDamage(ZoneActor.Player, ZoneKind.Wilds));
            Assert.IsTrue(SafeZoneRules.CanTakeDamage(ZoneActor.Monster, ZoneKind.Wilds));
            Assert.IsFalse(SafeZoneRules.CanTakeDamage(ZoneActor.Npc, ZoneKind.Wilds));
            Assert.IsTrue(SafeZoneRules.CanUseDamagingSkill(ZoneKind.Wilds));
        }

        [Test]
        public void Monsters_StopAtTheBorder_AndEncountersAndRoadEventsStayOutside()
        {
            Assert.IsFalse(SafeZoneRules.CanEnemyPursueInto(ZoneKind.City));
            Assert.IsTrue(SafeZoneRules.CanEnemyPursueInto(ZoneKind.Wilds));
            Assert.IsTrue(SafeZoneRules.ShouldDropTarget(ZoneKind.City));
            Assert.IsFalse(SafeZoneRules.ShouldDropTarget(ZoneKind.Wilds));
            Assert.IsFalse(SafeZoneRules.CanStartRandomEncounter(ZoneKind.City));
            Assert.IsTrue(SafeZoneRules.CanStartRandomEncounter(ZoneKind.Wilds));
            Assert.IsFalse(SafeZoneRules.CanTriggerRoadEvent(ZoneKind.City));
            Assert.IsTrue(SafeZoneRules.CanTriggerRoadEvent(ZoneKind.Wilds));
            Assert.IsTrue(SafeZoneRules.ShouldClearDebuffsOnEntry(ZoneKind.Wilds, ZoneKind.City));
            Assert.IsFalse(SafeZoneRules.ShouldClearDebuffsOnEntry(ZoneKind.City, ZoneKind.City));
            Assert.IsFalse(SafeZoneRules.ShouldClearDebuffsOnEntry(ZoneKind.City, ZoneKind.Wilds));
        }

        [Test]
        public void ZoneAtDistance_PzLock_AndRespawnRegion()
        {
            Assert.AreEqual(ZoneKind.City, SafeZoneRules.ZoneAtDistance(0f, SafeZoneRules.MapCityRadius));
            Assert.AreEqual(ZoneKind.City, SafeZoneRules.ZoneAtDistance(SafeZoneRules.MapCityRadius, SafeZoneRules.MapCityRadius), "border counts as city");
            Assert.AreEqual(ZoneKind.Wilds, SafeZoneRules.ZoneAtDistance(SafeZoneRules.MapCityRadius + 0.01f, SafeZoneRules.MapCityRadius));

            Assert.IsTrue(SafeZoneRules.CanEnterCity(-1d), "never attacked a player (offline)");
            Assert.IsFalse(SafeZoneRules.CanEnterCity(3d), "PZ lock");
            Assert.IsTrue(SafeZoneRules.CanEnterCity(SafeZoneRules.PlayerCombatTagSeconds));

            foreach (BiomeType region in Regions)
            {
                Assert.AreEqual(region, SafeZoneRules.RespawnRegion(region, CityCatalog.HasCity, BiomeType.Forest), "every region has a city");
            }

            Assert.AreEqual(BiomeType.Forest, SafeZoneRules.RespawnRegion(BiomeType.Caves, _ => false, BiomeType.Forest), "no city: back to the start");
        }

        [Test]
        public void SafeZoneRules_HasNoUnityDependency()
        {
            // The future server compiles this same file: it must stay plain C#.
            string source = System.IO.File.ReadAllText("Assets/Scripts/Gameplay/World/Cities/SafeZoneRules.cs");
            StringAssert.DoesNotContain("UnityEngine", source);
            StringAssert.DoesNotContain("MonoBehaviour", source);
        }

        // ------------------------------------------------------------------
        // City catalog
        // ------------------------------------------------------------------

        [Test]
        public void EveryRegion_HasExactlyOneCity_WithResidentsAndALocalShop()
        {
            foreach (BiomeType region in Regions)
            {
                Assert.AreEqual(1, CityCatalog.Cities.Count(c => c.Region == region), region.ToString());
                CitySpec city = CityCatalog.ForRegion(region);
                Assert.IsNotEmpty(city.ShopIds, $"{city.CityId} has no shop");
                if (region != BiomeType.Forest)
                {
                    Assert.That(city.Residents.Count, Is.InRange(3, 4), $"{city.CityId} residents");
                    Assert.IsFalse(city.HasGrandmaServices, "Baú and Fogão stay in Vó Zefa's village");
                    Assert.IsFalse(city.HasStoryVillagers);
                }
            }

            CitySpec vila = CityCatalog.ForRegion(BiomeType.Forest);
            Assert.AreEqual(CityCatalog.VilaZefa, vila.CityId);
            Assert.IsTrue(vila.HasGrandmaServices);
            Assert.IsTrue(vila.HasStoryVillagers);
            Assert.Contains(CityShopCatalog.GeneralStore, vila.ShopIds.ToList(), "the old Loja stays in the Vila");
            Assert.AreEqual(CityCatalog.Cities.Count, CityCatalog.Cities.Select(c => c.CityId).Distinct().Count());
        }

        [Test]
        public void EveryCityAndResidentText_ExistsInBothLanguages_WithALinePerCousin()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);

            var keys = new List<string>();
            foreach (CitySpec city in CityCatalog.Cities)
            {
                keys.Add(city.NameKey);
                keys.Add(city.ButtonKey);
                keys.Add(city.FlavourKey);
            }

            foreach (CityResidentSpec resident in CityCatalog.AllResidents())
            {
                keys.Add(resident.NameKey);
                keys.Add(resident.AnyLineKey);
                keys.AddRange(Heroes.Select(resident.LineKey));
            }

            keys.AddRange(CityShopCatalog.Shops.Select(s => s.NameKey));

            foreach (string key in keys)
            {
                Assert.IsTrue(manager.HasKey(key), $"missing key {key}");
                Assert.IsFalse(string.IsNullOrEmpty(CityTexts.GetFallback(key)), $"no pt-BR for {key}");
            }

            Assert.AreEqual(keys.Count, keys.Distinct().Count(), "a key is used twice");
            Assert.AreEqual(CityCatalog.AllResidents().Count(), CityCatalog.AllResidents().Select(r => r.ResidentId).Distinct().Count());
        }

        [Test]
        public void Residents_ChangeTheirLinePerCousin()
        {
            foreach (CityResidentSpec resident in CityCatalog.AllResidents())
            {
                List<string> lines = Heroes.Select(h => CityTexts.ResidentLine(null, resident, h)).ToList();
                Assert.AreEqual(Heroes.Length, lines.Distinct().Count(), $"{resident.ResidentId} repeats a line");
                Assert.IsFalse(string.IsNullOrEmpty(CityTexts.ResidentLine(null, resident, null)));
            }

            CityResidentSpec tiao = CityCatalog.Get(CityCatalog.AreiaBranca).Residents.First(r => r.ResidentId == "tiao");
            StringAssert.StartsWith("Uzzai, o Pastor: “", CityTexts.ResidentSpeech(null, tiao, CharacterId.Lavine));
        }

        // ------------------------------------------------------------------
        // Shop catalog
        // ------------------------------------------------------------------

        [Test]
        public void Shops_AreThemed_KeptByAResident_AndSitInACity()
        {
            var themes = new HashSet<ShopTheme>(CityShopCatalog.Shops.Select(s => s.Theme));
            foreach (ShopTheme theme in new[] { ShopTheme.Armarinho, ShopTheme.Ferreiro, ShopTheme.Feira, ShopTheme.Mercearia, ShopTheme.Mascate })
            {
                Assert.IsTrue(themes.Contains(theme), theme.ToString());
            }

            foreach (CityShopSpec shop in CityShopCatalog.Shops)
            {
                Assert.AreEqual(1, CityCatalog.AllResidents().Count(r => r.ShopId == shop.ShopId), $"{shop.ShopId} keeper");
                if (shop.IsRoaming)
                {
                    Assert.IsFalse(CityCatalog.Cities.Any(c => c.ShopIds.Contains(shop.ShopId)), "the peddler has no fixed counter");
                }
                else
                {
                    Assert.IsTrue(CityCatalog.ForRegion(shop.HomeRegion).ShopIds.Contains(shop.ShopId), $"{shop.ShopId} not in its home city");
                }
            }

            // Every city but the Vila has its own regional speciality (the Vila's is the feira).
            foreach (CitySpec city in CityCatalog.Cities.Where(c => c.Region != BiomeType.Forest))
            {
                Assert.IsTrue(city.ShopIds.Any(id => CityShopCatalog.Get(id) != null && CityShopCatalog.Get(id).HomeRegion == city.Region), city.CityId);
            }

            foreach (CitySpec city in CityCatalog.Cities)
            {
                foreach (string shopId in city.ShopIds)
                {
                    Assert.IsTrue(CityShopCatalog.Get(shopId) != null || Resources.Load<ShopDefinition>(CityShopCatalog.AssetFolder + shopId) != null, shopId);
                }
            }
        }

        [Test]
        public void ShopEntries_UseExistingItems_AtBalancedPrices()
        {
            foreach (CityShopSpec shop in CityShopCatalog.Shops)
            {
                Assert.AreEqual(shop.Entries.Count, shop.Entries.Select(e => e.ItemId).Distinct().Count(), $"{shop.ShopId} lists an item twice");
                int rotatingEntries = shop.Entries.Count(e => e.Rotating && e.Pool == ShopPool.Main);
                if (rotatingEntries > 0)
                {
                    Assert.That(shop.RotatingSlots, Is.InRange(1, rotatingEntries), shop.ShopId);
                }
                else
                {
                    // A fixed catalog (the Haras da Vila: whistles + feed, Docs/Mounts.md §3) has nothing to rotate.
                    Assert.AreEqual(0, shop.RotatingSlots, $"{shop.ShopId} has slots but nothing rotating");
                }

                // House items and outfits are checked in CityShopTests (they are not Resources/Items).
                foreach (CityShopEntry entry in shop.Entries.Where(e => e.IsBagItem))
                {
                    ItemDefinition item = ItemDatabase.GetById(entry.ItemId);
                    Assert.IsNotNull(item, $"{shop.ShopId}: {entry.ItemId} is not in Resources/Items");
                    Assert.AreNotEqual("item_gold", entry.ItemId, "never sells the currency");
                    Assert.AreNotEqual("item_gold_coin", entry.ItemId);
                    Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)),
                        $"{shop.ShopId}: {entry.ItemId} at {entry.Price} vs value {item.Value} (Docs/Balance, Cidades)");
                    // A daily/weekly stock, or -1 = never runs out (ShopInventoryItem): the Haras's whistles and feed.
                    Assert.IsTrue(entry.Stock > 0 || entry.Stock == -1, $"{shop.ShopId}: {entry.ItemId} stock {entry.Stock}");
                }

                if (!shop.IsRoaming)
                {
                    Assert.IsTrue(shop.Entries.Any(e => !e.Rotating && !e.Unlock.IsGated), $"{shop.ShopId} has nothing fixed from day one");
                }
            }

            Assert.IsTrue(CityShopCatalog.Shops.SelectMany(s => s.Entries).Any(e => e.Unlock.Kind == ShopUnlockKind.RegionCleared), "campaign unlocks exist");
            Assert.IsTrue(CityShopCatalog.Shops.SelectMany(s => s.Entries).Any(e => e.Unlock.Kind == ShopUnlockKind.RegionUnlocked));
        }

        // ------------------------------------------------------------------
        // Rotation
        // ------------------------------------------------------------------

        [Test]
        public void Periods_CountDaysAndMondayToSundayWeeks()
        {
            Assert.AreEqual(0, ShopRotation.DayIndex(new DateTime(2026, 1, 5, 23, 59, 0)));
            Assert.AreEqual(1, ShopRotation.DayIndex(new DateTime(2026, 1, 6)));
            Assert.AreEqual(-1, ShopRotation.DayIndex(new DateTime(2026, 1, 4)));

            var monday = new DateTime(2026, 10, 26);
            Assert.AreEqual(DayOfWeek.Monday, monday.DayOfWeek);
            long week = ShopRotation.PeriodIndex(monday, ShopRotationPeriod.Weekly);
            Assert.AreEqual(week, ShopRotation.PeriodIndex(monday.AddDays(6), ShopRotationPeriod.Weekly), "same week until Sunday");
            Assert.AreEqual(week + 1, ShopRotation.PeriodIndex(monday.AddDays(7), ShopRotationPeriod.Weekly));
            Assert.AreEqual(week - 1, ShopRotation.PeriodIndex(monday.AddDays(-1), ShopRotationPeriod.Weekly));
            Assert.AreEqual(-1, ShopRotation.PeriodIndex(new DateTime(2026, 1, 4), ShopRotationPeriod.Weekly), "floor, not truncation");
        }

        [Test]
        public void PickStock_IsSeededByPeriod_FillsTheSlots_AndRotates()
        {
            CityShopSpec feira = CityShopCatalog.Get(CityShopCatalog.FeiraLivre);
            long day = ShopRotation.DayIndex(new DateTime(2026, 10, 29));

            List<string> today = ShopRotation.PickStock(feira, day, null).Select(e => e.ItemId).ToList();
            CollectionAssert.AreEqual(today, ShopRotation.PickStock(feira, day, null).Select(e => e.ItemId).ToList(), "same day, same counter");
            Assert.AreEqual(feira.Entries.Count(e => !e.Rotating) + feira.RotatingSlots + feira.HouseSlots + feira.WeeklySlots, today.Count);
            Assert.IsTrue(today.Contains("item_herb"), "fixed entry always there");

            var seen = new HashSet<string>();
            for (int d = 0; d < 30; d++)
            {
                seen.Add(string.Join(",", ShopRotation.PickStock(feira, day + d, null).Select(e => e.ItemId)));
            }

            Assert.Greater(seen.Count, 3, "the counter rotates from day to day");
            Assert.AreEqual(ShopRotation.Seed("x", 12), ShopRotation.Seed("x", 12));
            Assert.AreNotEqual(ShopRotation.Seed("x", 12), ShopRotation.Seed("x", 13));
        }

        [Test]
        public void PickStock_LeavesLockedEntriesOut_UntilTheCampaignOpensThem()
        {
            CityShopSpec forja = CityShopCatalog.Get(CityShopCatalog.ForjaTonho);
            var map = new WorldMapProgress();
            Func<ShopUnlock, bool> gate = CityShopService.UnlockCheck(map);

            for (long week = 0; week < 60; week++)
            {
                Assert.IsFalse(ShopRotation.PickStock(forja, week, gate).Any(e => e.ItemId == "item_iron_sword"), "sword before the Mountain boss");
            }

            map.Unlock(BiomeType.Mountain);
            map.MarkRegionCleared(BiomeType.Mountain);
            bool appeared = false;
            for (long week = 0; week < 60 && !appeared; week++)
            {
                appeared = ShopRotation.PickStock(forja, week, gate).Any(e => e.ItemId == "item_iron_sword");
            }

            Assert.IsTrue(appeared, "the sword rotates in once the Mountain is cleared");

            CityShopSpec mascate = CityShopCatalog.Get(CityShopCatalog.MascateBento);
            Assert.AreEqual(mascate.RotatingSlots, ShopRotation.PickStock(mascate, 5, _ => false).Count(e => e.Pool == ShopPool.Main), "slots filled from what is open");
        }

        [Test]
        public void EffectivePeriod_IgnoresAClockSetBack()
        {
            Assert.AreEqual(10, ShopRotation.EffectivePeriod(10, null));
            Assert.AreEqual(12, ShopRotation.EffectivePeriod(10, 12), "clock rolled back: keep the later counter");
            Assert.AreEqual(14, ShopRotation.EffectivePeriod(14, 12));

            var cities = new CityProgress();
            Assert.AreEqual(100, cities.ResolveShopPeriod("s", 100, out bool advanced));
            Assert.IsFalse(advanced, "first look is not a restock");
            Assert.AreEqual(100, cities.ResolveShopPeriod("s", 98, out advanced));
            Assert.IsFalse(advanced);
            Assert.AreEqual(101, cities.ResolveShopPeriod("s", 101, out advanced));
            Assert.IsTrue(advanced);
        }

        [Test]
        public void Mascate_VisitsOpenCitiesByDate()
        {
            var open = new List<BiomeType> { BiomeType.Forest, BiomeType.Desert };
            var where = new HashSet<BiomeType>();
            for (long d = 0; d < 40; d++)
            {
                BiomeType? region = ShopRotation.MascateRegion(d, open);
                Assert.IsTrue(region.HasValue && open.Contains(region.Value));
                Assert.AreEqual(region, ShopRotation.MascateRegion(d, new List<BiomeType> { BiomeType.Desert, BiomeType.Forest }), "order of candidates does not matter");
                where.Add(region.Value);
            }

            Assert.AreEqual(2, where.Count, "he moves around");
            Assert.IsNull(ShopRotation.MascateRegion(3, new List<BiomeType>()));

            // In the city of the day, his counter is offered and he is among the residents.
            var date = new DateTime(2026, 10, 29);
            var map = new WorldMapProgress();
            BiomeType today = CityShopService.MascateRegion(date, map).Value;
            Assert.AreEqual(BiomeType.Forest, today, "only the Floresta is open at the start");
            List<string> shops = CityShopService.ShopsOpenIn(CityCatalog.ForRegion(today), date, map);
            Assert.Contains(CityShopCatalog.MascateBento, shops);
            Assert.Contains(CityCatalog.Mascate, VillageHubButton.ResidentsOf(CityCatalog.ForRegion(today), shops));
        }

        [Test]
        public void Resolve_BuildsTodaysCounter_AndRefillsStockWhenThePeriodMovesOn()
        {
            var shops = new ShopManager();
            var cities = new CityProgress();
            var map = new WorldMapProgress();
            var date = new DateTime(2026, 10, 29);
            CityShopService.ClockOverride = () => date;
            CityShopService.HasHouseOverride = () => false;

            ShopDefinition lazaro = CityShopService.Resolve(CityShopCatalog.MerceariaLazaro, cities, shops, map);
            Assert.IsNotNull(lazaro);
            Assert.AreEqual(CityShopCatalog.MerceariaLazaro, lazaro.ShopId);
            Assert.AreEqual("city.shop." + CityShopCatalog.MerceariaLazaro + ".name", lazaro.DisplayNameKey);
            CityShopSpec spec = CityShopCatalog.Get(CityShopCatalog.MerceariaLazaro);
            CollectionAssert.AreEqual(
                ShopRotation.PickStock(spec, ShopRotation.DayIndex(date), CityShopService.UnlockCheck(map), e => e.Kind != CityShopEntryKind.HouseItem).Select(e => e.ItemId),
                lazaro.Catalog.Select(e => e.Item.ItemId));
            Assert.AreEqual(6, lazaro.Catalog.First(e => e.Item.ItemId == "item_cooked_meat").Price);
            Assert.AreEqual(10, lazaro.Catalog.First(e => e.Item.ItemId == "item_cooked_meat").InitialStock);
            Assert.AreSame(lazaro, CityShopService.Resolve(CityShopCatalog.MerceariaLazaro, cities, shops, map), "cached for the same counter");

            // Some cooked meat was bought today...
            shops.RestoreState(new ShopSaveState
            {
                stockKeys = new[] { CityShopCatalog.MerceariaLazaro + "/item_cooked_meat", CityShopCatalog.FeiraLivre + "/item_herb" },
                stockValues = new[] { 2, 3 },
            });
            Assert.AreEqual(2, shops.GetStock(lazaro, lazaro.Catalog.First(e => e.Item.ItemId == "item_cooked_meat")));

            // ...tomorrow the grocer has a new counter and full stock; other shops keep theirs.
            date = date.AddDays(1);
            ShopDefinition tomorrow = CityShopService.Resolve(CityShopCatalog.MerceariaLazaro, cities, shops, map);
            Assert.AreEqual(10, shops.GetStock(tomorrow, tomorrow.Catalog.First(e => e.Item.ItemId == "item_cooked_meat")));
            var feiraState = (ShopSaveState)shops.CaptureState();
            Assert.Contains(CityShopCatalog.FeiraLivre + "/item_herb", feiraState.stockKeys);

            Assert.IsNotNull(CityShopService.Resolve(CityShopCatalog.GeneralStore, cities, shops, map), "the old asset shop still resolves");
        }

        // ------------------------------------------------------------------
        // Carro de boi
        // ------------------------------------------------------------------

        [Test]
        public void FastTravel_CostsTwoGoldPerRoad_AndChecksVisitsUnlocksAndGold()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            Assert.AreEqual(0, FastTravelRules.Roads(graph.GetNeighbours, BiomeType.Forest, BiomeType.Forest));
            Assert.AreEqual(1, FastTravelRules.Roads(graph.GetNeighbours, BiomeType.Forest, BiomeType.Desert));
            Assert.AreEqual(3, FastTravelRules.Roads(graph.GetNeighbours, BiomeType.Forest, BiomeType.Caves));
            Assert.AreEqual(2, FastTravelRules.Roads(graph.GetNeighbours, BiomeType.Caves, BiomeType.Desert));
            Assert.AreEqual(-1, FastTravelRules.Roads(_ => Enumerable.Empty<BiomeType>(), BiomeType.Forest, BiomeType.Caves));
            Assert.AreEqual(6, FastTravelRules.Cost(3));
            Assert.AreEqual(0, FastTravelRules.Cost(0));

            Func<BiomeType, bool> all = _ => true;
            Assert.AreEqual(FastTravelCheck.Ok, FastTravelRules.Check(BiomeType.Forest, BiomeType.Caves, true, all, all, 3, 6));
            Assert.AreEqual(FastTravelCheck.NotEnoughGold, FastTravelRules.Check(BiomeType.Forest, BiomeType.Caves, true, all, all, 3, 5));
            Assert.AreEqual(FastTravelCheck.SameCity, FastTravelRules.Check(BiomeType.Desert, BiomeType.Desert, true, all, all, 0, 99));
            Assert.AreEqual(FastTravelCheck.NotInCity, FastTravelRules.Check(BiomeType.Forest, BiomeType.Desert, false, all, all, 1, 99));
            Assert.AreEqual(FastTravelCheck.NotVisited, FastTravelRules.Check(BiomeType.Forest, BiomeType.Desert, true, r => r == BiomeType.Forest, all, 1, 99));
            Assert.AreEqual(FastTravelCheck.Locked, FastTravelRules.Check(BiomeType.Forest, BiomeType.Desert, true, all, _ => false, 1, 99));
            Assert.AreEqual(FastTravelCheck.NoRoute, FastTravelRules.Check(BiomeType.Forest, BiomeType.Desert, true, all, all, -1, 99));
        }

        [Test]
        public void FastTravelRows_ExplainEveryDestination()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            Func<BiomeType, bool> visited = r => r == BiomeType.Forest || r == BiomeType.Mountain;
            Func<BiomeType, bool> unlocked = r => r != BiomeType.Caves;

            FastTravelRow here = CityFastTravelPanel.BuildRow(CityCatalog.ForRegion(BiomeType.Forest), BiomeType.Forest, true, visited, unlocked, graph.GetNeighbours, 3, null);
            Assert.AreEqual(FastTravelCheck.SameCity, here.Check);
            Assert.AreEqual("Vocês estão aqui", here.Status);

            FastTravelRow serra = CityFastTravelPanel.BuildRow(CityCatalog.ForRegion(BiomeType.Mountain), BiomeType.Forest, true, visited, unlocked, graph.GetNeighbours, 3, null);
            Assert.AreEqual(FastTravelCheck.NotEnoughGold, serra.Check);
            Assert.AreEqual(4, serra.Cost);
            Assert.AreEqual("Falta ouro para a passagem (4)", serra.Status);
            Assert.AreEqual("Embarcar (passagem: 4 ouro)", serra.ButtonText);
            Assert.AreEqual("Vila de Litema", serra.Name);

            FastTravelRow juazeiro = CityFastTravelPanel.BuildRow(CityCatalog.ForRegion(BiomeType.Desert), BiomeType.Forest, true, visited, unlocked, graph.GetNeighbours, 3, null);
            Assert.AreEqual(FastTravelCheck.NotVisited, juazeiro.Check);

            FastTravelRow ok = CityFastTravelPanel.BuildRow(CityCatalog.ForRegion(BiomeType.Mountain), BiomeType.Forest, true, visited, unlocked, graph.GetNeighbours, 10, null);
            Assert.IsTrue(ok.CanTravel);
            Assert.AreEqual("a 2 estrada(s)", ok.Status);
        }

        // ------------------------------------------------------------------
        // Visits, respawn, save
        // ------------------------------------------------------------------

        [Test]
        public void Visits_StartWithTheVila_AndOldSavesKeepRegionsAlreadyFoughtIn()
        {
            var map = new WorldMapProgress();
            var cities = new CityProgress(map);
            Assert.IsTrue(cities.IsVisited(BiomeType.Forest));
            Assert.IsFalse(cities.IsVisited(BiomeType.Desert));
            Assert.IsFalse(cities.MarkVisited(BiomeType.Forest), "the starting city is not news");

            map.Unlock(BiomeType.Desert);
            map.RecordVictory(BiomeType.Desert);
            Assert.IsTrue(cities.IsVisited(BiomeType.Desert), "won a fight there: been there");

            Assert.IsTrue(cities.MarkVisited(BiomeType.Mountain));
            Assert.IsFalse(cities.MarkVisited(BiomeType.Mountain));
            CollectionAssert.AreEquivalent(new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain }, cities.VisitedRegions().ToList());
        }

        [Test]
        public void Defeat_WakesTheHeroInTheRegionsCity()
        {
            var map = new WorldMapProgress();
            map.Unlock(BiomeType.Desert);
            var cities = new CityProgress(map);
            cities.Initialize();

            var payload = new EncounterPayload { biome = BiomeType.Desert };
            payload.enemyIds.Add("enemy_desert_scorpion");
            EventBus.Publish(new EncounterEndedEvent(payload, EncounterOutcome.Fled));
            Assert.IsNull(cities.PendingRespawnCity, "only a defeat");

            EventBus.Publish(new EncounterEndedEvent(payload, EncounterOutcome.Defeat));
            Assert.AreEqual(BiomeType.Desert, map.CurrentRegion);
            Assert.IsTrue(cities.IsVisited(BiomeType.Desert));
            Assert.AreEqual(CityCatalog.AreiaBranca, cities.ConsumeRespawnNotice().CityId);
            Assert.IsNull(cities.ConsumeRespawnNotice(), "told once");
            cities.Shutdown();
        }

        [Test]
        public void SaveLoad_RoundTripsVisitsAndShopPeriods_AndOldSavesLoad()
        {
            var save = new SaveManager();
            save.Initialize();
            var cities = new CityProgress();
            cities.MarkVisited(BiomeType.Caves);
            cities.ResolveShopPeriod(CityShopCatalog.FeiraLivre, 300, out _);
            cities.ResolveShopPeriod(CityShopCatalog.ForjaTonho, 42, out _);
            save.Register(cities);
            save.Save(TestSlot);

            var loadedSave = new SaveManager();
            loadedSave.Initialize();
            var loaded = new CityProgress();
            loadedSave.Register(loaded);
            Assert.IsTrue(loadedSave.Load(TestSlot));
            Assert.IsTrue(loaded.IsVisited(BiomeType.Caves));
            Assert.IsFalse(loaded.IsVisited(BiomeType.Desert));
            Assert.AreEqual(300, loaded.LastShopPeriod(CityShopCatalog.FeiraLivre));
            Assert.AreEqual(42, loaded.LastShopPeriod(CityShopCatalog.ForjaTonho));

            // A save written before cities existed.
            var oldSave = new SaveManager();
            oldSave.Initialize();
            oldSave.Save(TestSlot);
            var fresh = new CityProgress();
            var freshSave = new SaveManager();
            freshSave.Initialize();
            freshSave.Register(fresh);
            Assert.IsTrue(freshSave.Load(TestSlot));
            Assert.IsTrue(fresh.IsVisited(BiomeType.Forest));
            Assert.IsFalse(fresh.IsVisited(BiomeType.Caves));
            Assert.IsNull(fresh.LastShopPeriod(CityShopCatalog.FeiraLivre));
        }

        [Test]
        public void GameManager_RegistersCities_AndNewGameResetsThem()
        {
            if (GameManager.Instance != null)
            {
                Object.DestroyImmediate(GameManager.Instance.gameObject);
            }

            _gameManagerObject = new GameObject("CityTests_GameManager");
            _gm = _gameManagerObject.AddComponent<GameManager>();
            _gm.ForceInitializeForTests();
            Assume.That(_gm.Cities, Is.Not.Null);

            Assert.IsTrue(_gm.NewSessionReset.Resettables.Contains(_gm.Cities));
            Assert.AreSame(_gm.Cities, CityProgress.ResolveShared());

            _gm.Cities.MarkVisited(BiomeType.Mountain);
            _gm.SaveManager.Save(TestSlot);
            Assert.IsTrue(_gm.SaveManager.PeekSlot(TestSlot).entries.Any(e => e.key == CityProgress.Key), "registered with SaveManager");

            _gm.GameFlowService.EnterFrontEnd();
            Assert.IsTrue(_gm.GameFlowService.StartNewGame());
            Assert.IsFalse(_gm.Cities.IsVisited(BiomeType.Mountain), "Novo jogo starts with the Vila only");

            Assert.IsTrue(_gm.SaveManager.Load(TestSlot));
            Assert.IsTrue(_gm.Cities.IsVisited(BiomeType.Mountain));
        }

        // ------------------------------------------------------------------
        // Hub / HUD
        // ------------------------------------------------------------------

        [Test]
        public void HubButton_IsLabelledWithTheCity()
        {
            Assert.AreEqual("Vila", VillageHubButton.BuildButtonLabel(CityCatalog.ForRegion(BiomeType.Forest), false, null));
            Assert.AreEqual("Vila (!)", VillageHubButton.BuildButtonLabel(CityCatalog.ForRegion(BiomeType.Forest), true, null));
            Assert.AreEqual("Qocho", VillageHubButton.BuildButtonLabel(CityCatalog.ForRegion(BiomeType.Desert), false, null));
            Assert.IsNotEmpty(VillageHubButton.StoryVillagersOf(CityCatalog.ForRegion(BiomeType.Forest)));
            Assert.IsEmpty(VillageHubButton.StoryVillagersOf(CityCatalog.ForRegion(BiomeType.Caves)), "story villagers live in the Vila");
            Assert.AreEqual(3, VillageHubButton.ResidentsOf(CityCatalog.ForRegion(BiomeType.Desert), new List<string> { CityShopCatalog.SelariaVava }).Count);
        }

        [Test]
        public void SafeZoneIndicator_NamesTheCity()
        {
            Assert.IsNull(CitySafeZoneHud.IndicatorText(null, null));
            Assert.AreEqual("Você está em zona segura · Vila das Lamparinas", CitySafeZoneHud.IndicatorText(CityCatalog.ForRegion(BiomeType.Caves), null));

            var manager = new LocalizationManager();
            manager.Initialize();
            manager.SetLanguage(LanguageCode.en);
            Assert.AreEqual("You are in a safe zone · Qocho Oasis", CitySafeZoneHud.IndicatorText(CityCatalog.ForRegion(BiomeType.Desert), manager));
        }

        // ------------------------------------------------------------------
        // Map geometry
        // ------------------------------------------------------------------

        [Test]
        public void MapCity_IsTheCircleAroundTheNode()
        {
            var node = new Vector3(10f, 0f, 0f);
            Assert.AreEqual(ZoneKind.City, WorldMapCityPresence.ZoneAround(new Vector3(10f, 0.5f, 0f), node), "token stands on the node");
            Assert.AreEqual(ZoneKind.City, WorldMapCityPresence.ZoneAround(new Vector3(11.2f, 3f, 0.5f), node), "height ignored");
            Assert.AreEqual(ZoneKind.Wilds, WorldMapCityPresence.ZoneAround(new Vector3(12f, 0f, 0f), node));
            Assert.AreEqual(ZoneKind.Wilds, WorldMapCityPresence.ZoneAround(new Vector3(15f, 0f, 2f), node), "half way down the road");
        }

        [Test]
        public void TownLandmark_SitsClearOfRoadsAndTheRegionLandmark_AndTheRingOpensOnRoads()
        {
            var sites = new List<WorldMapSite>
            {
                new WorldMapSite(BiomeType.Forest, new Vector2(0f, 0f)),
                new WorldMapSite(BiomeType.Desert, new Vector2(10f, 0f)),
                new WorldMapSite(BiomeType.Mountain, new Vector2(20f, 4f)),
                new WorldMapSite(BiomeType.Caves, new Vector2(30f, 0f)),
            };
            var paths = new List<WorldMapPath>
            {
                new WorldMapPath(0, 1, sites[0].Position, sites[1].Position),
                new WorldMapPath(1, 2, sites[1].Position, sites[2].Position),
                new WorldMapPath(2, 3, sites[2].Position, sites[3].Position),
            };
            var terrain = new WorldMapTerrain(sites, paths, WorldMapDressing.DefaultSeed, true);

            for (int i = 0; i < sites.Count; i++)
            {
                Vector2 landmark = WorldMapDressingLayout.LandmarkPosition(terrain, i);
                Vector2 town = WorldMapCityDressing.TownSpot(terrain, i, landmark);
                Assert.AreEqual(WorldMapCityDressing.TownDistance, Vector2.Distance(town, sites[i].Position), 0.001f);
                Assert.Greater(terrain.DistanceToPath(town), WorldMapTerrain.PathHalfWidth, $"{sites[i].Region}: town on a road");
                Assert.Greater(Vector2.Distance(town, landmark), WorldMapDressingLayout.LandmarkRadius, $"{sites[i].Region}: town on the landmark");

                List<float> roads = WorldMapCityDressing.RoadAngles(terrain, i);
                Assert.AreEqual(i == 0 || i == 3 ? 1 : 2, roads.Count);
                float gap = WorldMapCityDressing.GateHalfAngle(SafeZoneRules.MapCityRadius);
                foreach (float road in roads)
                {
                    Assert.IsTrue(WorldMapCityDressing.IsInGate(road, roads, gap), "ring open where the road leaves");
                    if (roads.Count == 1)
                    {
                        Assert.IsFalse(WorldMapCityDressing.IsInGate(road + 180f, roads, gap), "closed on the far side");
                    }
                }

                var builder = new WorldMapMeshBuilder();
                WorldMapCityDressing.AppendSafeRing(builder, SafeZoneRules.MapCityRadius, roads);
                WorldMapCityDressing.AppendTown(builder, sites[i].Region);
                Assert.IsFalse(builder.IsEmpty);
            }

            Assert.AreEqual(0f, WorldMapCityDressing.RoadAngles(terrain, 1)[0] - 180f, 0.01f, "Desert's first road goes back west to the Forest");
        }
    }
}
