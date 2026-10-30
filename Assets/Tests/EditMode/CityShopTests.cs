using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests.EditMode
{
    /// <summary>The Vila shops of 2026-10-30: new keepers, house items, outfits, pools, freguesia and the Bornal de couro.</summary>
    public class CityShopTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
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
            foreach (GameObject go in _objects)
            {
                if (go != null)
                {
                    Object.DestroyImmediate(go);
                }
            }

            _objects.Clear();
            CityShopService.ClockOverride = null;
            CityShopService.HasHouseOverride = null;
            CityShopService.OutfitResolver = null;
            CityShopService.ClearCache();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
        }

        private static readonly string[] NewVilaShops =
        {
            CityShopCatalog.FerrariaSeverino, CityShopCatalog.MarcenariaDamiao, CityShopCatalog.CostureiraFatima,
        };

        [Test]
        public void Vila_HasEveryThemedShop_EachWithItsOwnKeeper()
        {
            CitySpec vila = CityCatalog.Get(CityCatalog.VilaZefa);
            foreach (string shopId in new[] { CityShopCatalog.ArmarinhoCida, CityShopCatalog.FerrariaSeverino, CityShopCatalog.FeiraLivre,
                         CityShopCatalog.MerceariaLazaro, CityShopCatalog.MarcenariaDamiao, CityShopCatalog.CostureiraFatima })
            {
                Assert.Contains(shopId, vila.ShopIds.ToList());
                CityResidentSpec keeper = vila.Residents.Single(r => r.ShopId == shopId);
                Assert.AreEqual(CityShopCatalog.Get(shopId).HomeRegion, BiomeType.Forest, shopId);
                Assert.IsNotNull(keeper);
            }

            Assert.AreEqual(ShopTheme.Ferreiro, CityShopCatalog.Get(CityShopCatalog.FerrariaSeverino).Theme);
            Assert.AreEqual(ShopTheme.Marcenaria, CityShopCatalog.Get(CityShopCatalog.MarcenariaDamiao).Theme);
            Assert.AreEqual(ShopTheme.Costureira, CityShopCatalog.Get(CityShopCatalog.CostureiraFatima).Theme);
            Assert.AreEqual(CityShopCatalog.MascateBento, CityCatalog.Mascate.ShopId, "Seu Bento keeps the roaming counter");
        }

        [Test]
        public void NewShopsAndKeepers_HaveTextsInBothLanguages()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityTexts.EnsureRegistered(manager);

            foreach (string shopId in NewVilaShops)
            {
                Assert.IsTrue(manager.HasKey(CityShopCatalog.Get(shopId).NameKey), shopId);
            }

            foreach (string residentId in new[] { "severino", "damiao", "fatima" })
            {
                Assert.IsTrue(manager.HasKey("city.resident." + residentId + ".name"), residentId);
                Assert.IsTrue(manager.HasKey("city.resident." + residentId + ".line.any"), residentId);
            }

            for (int tier = 0; tier <= ShopReputation.MaxTier; tier++)
            {
                Assert.IsFalse(string.IsNullOrEmpty(CityTexts.GetFallback(ShopReputation.TierNameKey(tier))), "tier " + tier);
            }

            Assert.AreEqual("Bornal de couro", CityTexts.GetFallback("item." + CityShopCatalog.BornalItemId + ".name"));

            // House items on a counter get an item.* name, so the shop UI shows the furniture's name.
            CityShopEntry chair = CityShopCatalog.Get(CityShopCatalog.MarcenariaDamiao).Entries.First(e => e.Kind == CityShopEntryKind.HouseItem);
            Assert.AreEqual(HouseCatalog.Get(chair.ItemId).NamePtBR, CityTexts.GetFallback("item." + chair.ItemId + ".name"));
            Assert.IsTrue(manager.HasKey("item." + chair.ItemId + ".name"));
        }

        [Test]
        public void HouseEntries_ComeFromTheHouseCatalog_AtItsPrice_GatedByRarity()
        {
            CityShopSpec marcenaria = CityShopCatalog.Get(CityShopCatalog.MarcenariaDamiao);
            List<CityShopEntry> house = marcenaria.Entries.Where(e => e.Kind == CityShopEntryKind.HouseItem).ToList();
            int expected = HouseCatalog.All.Count(i => i.BasePrice > 0 && i.Rarity != ItemRarity.Legendary
                && i.Sources.Any(s => s.Type == HouseItemSourceType.Shop && s.RefId == CityShopCatalog.MarcenariaDamiao));
            Assert.AreEqual(expected, house.Count);
            Assert.Greater(house.Count, marcenaria.HouseSlots, "more furniture than slots: it rotates");

            foreach (CityShopSpec shop in CityShopCatalog.Shops)
            {
                foreach (CityShopEntry entry in shop.Entries.Where(e => e.Kind == CityShopEntryKind.HouseItem))
                {
                    HouseItemDefinition item = HouseCatalog.Get(entry.ItemId);
                    Assert.IsNotNull(item, entry.ItemId);
                    Assert.AreEqual(item.BasePrice, entry.Price, entry.ItemId);
                    Assert.IsTrue(entry.Rotating);
                    switch (item.Rarity)
                    {
                        case ItemRarity.Common:
                            Assert.IsFalse(entry.Unlock.IsGated);
                            Assert.AreEqual(ShopPool.House, entry.Pool);
                            break;
                        case ItemRarity.Uncommon:
                            Assert.AreEqual(ShopUnlock.Cleared(BiomeType.Forest).Region, entry.Unlock.Region);
                            Assert.AreEqual(ShopUnlockKind.RegionCleared, entry.Unlock.Kind);
                            break;
                        case ItemRarity.Epic:
                            Assert.AreEqual(ShopPool.Weekly, entry.Pool, "Epic furniture is the week's special");
                            Assert.AreEqual(BiomeType.Mountain, entry.Unlock.Region);
                            break;
                    }
                }

                if (shop.Entries.Any(e => e.Kind == CityShopEntryKind.HouseItem && e.Pool == ShopPool.House))
                {
                    Assert.Greater(shop.HouseSlots, 0, shop.ShopId);
                }
            }
        }

        [Test]
        public void HouseItems_AndOutfits_StayOffTheCounter_UntilTheyCanBeSold()
        {
            CityShopSpec marcenaria = CityShopCatalog.Get(CityShopCatalog.MarcenariaDamiao);
            CityShopSpec costureira = CityShopCatalog.Get(CityShopCatalog.CostureiraFatima);
            Func<CityShopEntry, bool> noHouse = e => CityShopService.IsAvailable(e, false, true, null);
            Func<CityShopEntry, bool> withHouse = e => CityShopService.IsAvailable(e, true, true, null);

            for (long day = 0; day < 20; day++)
            {
                Assert.IsFalse(ShopRotation.PickStock(marcenaria, day, null, noHouse).Any(e => e.Kind == CityShopEntryKind.HouseItem), "no house, no furniture");
                List<CityShopEntry> counter = ShopRotation.PickStock(marcenaria, day, null, withHouse);
                Assert.AreEqual(marcenaria.HouseSlots, counter.Count(e => e.Pool == ShopPool.House && e.Rotating));
                Assert.AreEqual(marcenaria.WeeklySlots, counter.Count(e => e.Pool == ShopPool.Weekly), "one Epic a week");

                // Outfits have no wardrobe data yet: they never take a slot, the leather pieces fill in.
                List<CityShopEntry> fatima = ShopRotation.PickStock(costureira, day, null, withHouse);
                Assert.IsFalse(fatima.Any(e => e.Kind == CityShopEntryKind.Outfit));
                Assert.AreEqual(2, fatima.Count(e => e.Rotating && e.Pool == ShopPool.Main));
            }

            Func<CityShopEntry, bool> withOutfits = e => CityShopService.IsAvailable(e, true, true, id => true);
            Assert.AreEqual(costureira.RotatingSlots, ShopRotation.PickStock(costureira, 3, null, withOutfits).Count(e => e.Rotating && e.Pool == ShopPool.Main));
            Assert.Greater(costureira.Entries.Count(e => e.Kind == CityShopEntryKind.Outfit), 20, "CatalogoRoupas.md lines of Dona Fátima");
        }

        [Test]
        public void WeeklySpecial_StaysTheSameAllWeek_InADailyShop()
        {
            CityShopSpec marcenaria = CityShopCatalog.Get(CityShopCatalog.MarcenariaDamiao);
            long monday = ShopRotation.DayIndex(new DateTime(2026, 10, 26));
            Func<CityShopEntry, bool> withHouse = e => CityShopService.IsAvailable(e, true, true, null);
            string Special(long day) => string.Join(",", ShopRotation.PickStock(marcenaria, day, null, withHouse).Where(e => e.Pool == ShopPool.Weekly).Select(e => e.ItemId));

            string week = Special(monday);
            Assert.IsNotEmpty(week);
            for (int d = 1; d < 7; d++)
            {
                Assert.AreEqual(week, Special(monday + d), "same special Monday to Sunday");
            }

            var seen = new HashSet<string>();
            for (int w = 0; w < 12; w++)
            {
                seen.Add(Special(monday + 7 * w));
            }

            Assert.Greater(seen.Count, 1, "the special changes week to week");
            Assert.AreEqual(ShopRotation.WeekOf(monday, ShopRotationPeriod.Daily), ShopRotation.WeekOf(monday + 6, ShopRotationPeriod.Daily));
        }

        [Test]
        public void Reputation_TiersAndGates()
        {
            Assert.AreEqual(ShopReputation.NewCustomer, ShopReputation.TierFor(0));
            Assert.AreEqual(ShopReputation.NewCustomer, ShopReputation.TierFor(49));
            Assert.AreEqual(ShopReputation.Regular, ShopReputation.TierFor(50));
            Assert.AreEqual(ShopReputation.HouseRegular, ShopReputation.TierFor(150));
            Assert.AreEqual(ShopReputation.Compadre, ShopReputation.TierFor(10000));
            Assert.AreEqual(30, ShopReputation.PointsToNextTier(20));
            Assert.AreEqual(0, ShopReputation.PointsToNextTier(400));
            Assert.AreEqual(int.MaxValue, ShopReputation.Add(int.MaxValue - 1, 5));

            var cities = new CityProgress();
            Assert.IsFalse(cities.AddReputation(CityShopCatalog.ArmarinhoCida, 30));
            Assert.IsTrue(cities.AddReputation(CityShopCatalog.ArmarinhoCida, 20), "50 points: Freguês");
            Assert.AreEqual(ShopReputation.Regular, cities.ReputationTier(CityShopCatalog.ArmarinhoCida));
            Assert.AreEqual(0, cities.ReputationTier(CityShopCatalog.FeiraLivre), "per shop");

            CityShopSpec armarinho = CityShopCatalog.Get(CityShopCatalog.ArmarinhoCida);
            var fresh = new CityProgress();
            Assert.IsFalse(ShopRotation.PickStock(armarinho, 1, CityShopService.GateCheck(null, fresh, armarinho.ShopId)).Any(e => e.ItemId == CityShopCatalog.BornalItemId),
                "the bornal waits for a regular");
            Assert.IsTrue(ShopRotation.PickStock(armarinho, 1, CityShopService.GateCheck(null, cities, armarinho.ShopId)).Any(e => e.ItemId == CityShopCatalog.BornalItemId));

            // Saved and restored.
            var copy = new CityProgress();
            copy.RestoreState(cities.CaptureState());
            Assert.AreEqual(50, copy.ReputationPoints(CityShopCatalog.ArmarinhoCida));
        }

        [Test]
        public void Bornal_IsARealItem_SoldOnce_AtABalancedPrice()
        {
            ItemDefinition bornal = ItemDatabase.GetById(CityShopCatalog.BornalItemId);
            Assert.IsNotNull(bornal, "Resources/Items/item_bornal_couro");
            foreach (CityShopSpec shop in CityShopCatalog.Shops)
            {
                CityShopEntry entry = shop.Entries.FirstOrDefault(e => e.ItemId == CityShopCatalog.BornalItemId);
                if (entry != null)
                {
                    Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(bornal.Value * 0.8f), Mathf.CeilToInt(bornal.Value * 1.25f)), shop.ShopId);
                    Assert.AreEqual(1, entry.Stock);
                }
            }

            CityShopEntry cida = CityShopCatalog.EntryOf(CityShopCatalog.ArmarinhoCida, CityShopCatalog.BornalItemId);
            Assert.IsTrue(CityShopService.IsAvailable(cida, false, true, null));
            Assert.IsFalse(CityShopService.IsAvailable(cida, false, false, null), "one bornal per save");
        }

        [Test]
        public void BuyingTheBornal_GrowsTheBag_ByTwoRows_AndIsSaved()
        {
            InventoryManager bag = NewBag();
            var cities = new CityProgress();
            int rows = bag.Rows;
            ItemDefinition bornal = ItemDatabase.GetById(CityShopCatalog.BornalItemId);
            Assert.AreEqual(0, bag.AddItem(bornal, 1));

            CityShopPurchases.Apply(new ItemPurchasedEvent(CityShopCatalog.ArmarinhoCida, CityShopCatalog.BornalItemId, 1, 150), cities, bag, null);

            Assert.AreEqual(rows + CityShopCatalog.BornalRows, bag.Rows);
            Assert.AreEqual(0, bag.GetTotalCount(CityShopCatalog.BornalItemId), "the satchel becomes the bag, it leaves the slots");
            Assert.IsFalse(cities.CanBuyBornal);
            Assert.AreEqual(150, cities.ReputationPoints(CityShopCatalog.ArmarinhoCida), "gold spent counts for the freguesia");

            var restoredCities = new CityProgress();
            restoredCities.RestoreState(cities.CaptureState());
            Assert.IsFalse(restoredCities.CanBuyBornal);

            InventoryManager restored = NewBag();
            restored.RestoreState(bag.CaptureState());
            Assert.AreEqual(rows + CityShopCatalog.BornalRows, restored.Rows, "extraRows saved");
        }

        [Test]
        public void BuyingAHouseItem_SendsItToTheDepot()
        {
            InventoryManager bag = NewBag();
            var cities = new CityProgress();
            var house = new HouseService();
            CityShopEntry entry = CityShopCatalog.Get(CityShopCatalog.MarcenariaDamiao).Entries.First(e => e.Kind == CityShopEntryKind.HouseItem);
            ItemDefinition voucher = CityShopService.ResolveItem(entry);
            Assert.IsNotNull(voucher);
            Assert.AreEqual(entry.ItemId, voucher.ItemId);
            Assert.AreSame(voucher, CityShopService.HouseVoucher(entry.ItemId), "cached");
            Assert.AreEqual(0, bag.AddItem(voucher, 1));

            CityShopPurchases.Apply(new ItemPurchasedEvent(CityShopCatalog.MarcenariaDamiao, entry.ItemId, 1, entry.Price), cities, bag, house);

            Assert.AreEqual(1, house.Layout.DepotCount(entry.ItemId));
            Assert.AreEqual(0, bag.GetTotalCount(entry.ItemId));
            Assert.AreEqual(CityShopPurchases.Kind.HouseItem, CityShopPurchases.Classify(CityShopCatalog.MarcenariaDamiao, entry.ItemId));
            Assert.AreEqual(CityShopPurchases.Kind.Plain, CityShopPurchases.Classify(CityShopCatalog.MarcenariaDamiao, "item_wood"));
        }

        [Test]
        public void Resolve_PutsFurnitureOnTheCounter_OnlyWithTheHouse()
        {
            var shops = new ShopManager();
            var cities = new CityProgress();
            CityShopService.ClockOverride = () => new DateTime(2026, 10, 30);

            CityShopService.HasHouseOverride = () => false;
            ShopDefinition without = CityShopService.Resolve(CityShopCatalog.MarcenariaDamiao, cities, shops, new WorldMapProgress());
            Assert.IsFalse(without.Catalog.Any(e => HouseCatalog.Contains(e.Item.ItemId)));

            CityShopService.HasHouseOverride = () => true;
            ShopDefinition with = CityShopService.Resolve(CityShopCatalog.MarcenariaDamiao, cities, shops, new WorldMapProgress());
            Assert.AreEqual(CityShopCatalog.Get(CityShopCatalog.MarcenariaDamiao).HouseSlots,
                with.Catalog.Count(e => HouseCatalog.Contains(e.Item.ItemId)), "10 a day (the Epic special waits for the Mountain boss)");
        }

        [Test]
        public void PurchaseToasts_SayWhatHappened_InPtBRWithoutAManager()
        {
            var bornal = CityShopToastRelay.For(new CityShopPurchaseAppliedEvent(CityShopCatalog.ArmarinhoCida,
                CityShopCatalog.BornalItemId, CityShopPurchaseEffect.BagExpanded, 2), null);
            Assert.IsNotNull(bornal);
            StringAssert.Contains("2 linhas", bornal.Body);
            Assert.AreEqual("Armarinho da Dona Cida", bornal.Title);

            var rep = CityShopToastRelay.For(new CityShopPurchaseAppliedEvent(CityShopCatalog.FerrariaSeverino, "item_iron",
                CityShopPurchaseEffect.ReputationUp, ShopReputation.Compadre), null);
            StringAssert.Contains("Compadre", rep.Body);
            StringAssert.Contains("Ferraria do Seu Severino", rep.Body);

            Assert.IsNull(CityShopToastRelay.For(new CityShopPurchaseAppliedEvent(CityShopCatalog.FeiraLivre, "item_herb",
                CityShopPurchaseEffect.None, 0), null));
        }

        private InventoryManager NewBag()
        {
            var go = new GameObject("bag");
            _objects.Add(go);
            return go.AddComponent<InventoryManager>();
        }
    }
}
