using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map.Mechanics;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 2026-11-16 items of the fichas 64 and 68 (Docs/Mapas100.md): the baits of Old Kul's lake and the silver lure that calls the Velho Kul back
    /// (<see cref="IceBaitRules"/>, <see cref="KulLureCall"/>, <see cref="RegionBossReturns.CallNow"/>), and the Bomba de Água that hardens the lava
    /// (<see cref="WaterBombRules"/>).
    /// </summary>
    public class BaitAndWaterBombTests
    {
        private static readonly CharacterId[] Cousins = { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine };

        // ---------------- 64 · baits ----------------

        [Test]
        public void Bait_SilverOnlyAtTheGreatHoleWhileTheCallIsOpen_ElseTheGrub()
        {
            Assert.AreEqual(IceBait.Silver, IceBaitRules.Choose(true, true, 5, 1));
            Assert.AreEqual(IceBait.Grub, IceBaitRules.Choose(true, false, 5, 1), "call closed: the silver stays in the bag");
            Assert.AreEqual(IceBait.Grub, IceBaitRules.Choose(false, true, 5, 1), "never silver at a small hole");
            Assert.AreEqual(IceBait.None, IceBaitRules.Choose(false, true, 0, 3));
            Assert.AreEqual(IceBait.None, IceBaitRules.Choose(true, true, 0, 0));
            Assert.AreEqual(IceBaitRules.GrubItemId, IceBaitRules.ItemOf(IceBait.Grub));
            Assert.AreEqual(IceBaitRules.SilverItemId, IceBaitRules.ItemOf(IceBait.Silver));
            Assert.IsNull(IceBaitRules.ItemOf(IceBait.None));
        }

        [Test]
        public void Bait_ChangesTheOdds_AndTheGrubBitesSooner()
        {
            // 0.92 is an uncommon fish with nothing on the hook and a rare one on the silver lure (the rare band doubles).
            Assert.AreEqual(FishRarity.Uncommon, IceFishingRules.Roll(0.92, false, false).Rarity);
            Assert.AreEqual(FishCatalog.Surubim, IceFishingRules.Roll(0.92, false, false, IceBait.Silver).Id);
            Assert.AreEqual(IceFishingRules.Roll(0.5, false, false).Id, IceFishingRules.Roll(0.5, false, false, IceBait.None).Id);

            for (int seed = 0; seed < 20; seed++)
            {
                var session = new IceFishingSession(seed, false, false, IceBait.Grub);
                Assert.AreEqual(IceBait.Grub, session.Bait);
                session.Tick(IceBaitRules.GrubMaxBiteDelay + 0.01f);
                Assert.AreEqual(IceFishingState.Bite, session.State, "seed " + seed);
            }

            Assert.Less(IceBaitRules.GrubMaxBiteDelay, IceFishingSession.MaxBiteDelay);
            Assert.IsTrue(new IceFishingSession(1, false, true, IceBait.Silver).GreatHole);
        }

        [Test]
        public void KulCall_ThreeTugsWithinTheWindow_CallsHim_ThenRecharges()
        {
            var call = new KulLureCall();
            Assert.IsTrue(call.Open(1000, true));
            Assert.IsFalse(call.Open(1000, false), "the Kul is in the world: nothing to call");
            Assert.AreEqual(KulLureResult.KulHere, call.Tug(1000, false));
            Assert.AreEqual(0, call.TugsAt(1000));

            Assert.AreEqual(KulLureResult.Counted, call.Tug(1000, true));
            Assert.AreEqual(KulLureResult.Counted, call.Tug(1100, true));
            Assert.AreEqual(2, call.TugsAt(1100));
            Assert.AreEqual(KulLureResult.Called, call.Tug(1200, true));
            Assert.AreEqual(1200, call.CalledAt);
            Assert.AreEqual(0, call.TugsAt(1200));

            Assert.IsTrue(call.Recharging(1200 + KulLureCall.RechargeSeconds - 1));
            Assert.AreEqual(KulLureResult.Recharging, call.Tug(1300, true));
            Assert.AreEqual(KulLureCall.RechargeSeconds - 100, call.RechargeLeft(1300));
            Assert.IsFalse(call.Open(1300, true));
            Assert.IsTrue(call.Open(1200 + KulLureCall.RechargeSeconds, true));
        }

        [Test]
        public void KulCall_TheWindowCloses_AndOnlyTheCallSurvivesTheSave()
        {
            var call = new KulLureCall();
            call.Tug(0, true);
            call.Tug(10, true);
            Assert.AreEqual(0, call.TugsAt(10 + KulLureCall.WindowSeconds + 1), "the first tug is too old");
            Assert.AreEqual(KulLureResult.Counted, call.Tug(10 + KulLureCall.WindowSeconds + 1, true), "starts over at one");

            var loaded = new KulLureCall();
            loaded.Restore(KulLureCall.NeverCalled);
            Assert.IsFalse(loaded.Recharging(0));
            loaded.Restore(5000);
            Assert.IsTrue(loaded.Recharging(5100));
            Assert.AreEqual(0, loaded.Tugs);
        }

        [Test]
        public void BossReturns_CallNow_OnlyShortensTheWaitOfABeatenBoss()
        {
            var returns = new RegionBossReturns();
            Assert.IsFalse(returns.Waiting(0f), "at the arena from the start");
            Assert.IsFalse(returns.CallNow(0f), "nothing to call: he is there");

            returns.Withdraw(100f, false, 0);
            Assert.IsTrue(returns.Waiting(150f));
            Assert.IsFalse(returns.CanReturn(150f, true, 1, 12f, false, 18f));
            Assert.IsTrue(returns.CallNow(150f));
            Assert.IsTrue(returns.CanReturn(150f, true, 1, 12f, false, 18f));
            Assert.IsFalse(returns.CallNow(151f), "already back");

            var apep = new RegionBossReturns();
            apep.Withdraw(100f, true, 3);
            Assert.IsFalse(apep.CallNow(150f), "one that waits for the night keeps its own rule");
            Assert.IsTrue(RegionBossRules.SpawnOf(RegionBossRules.OldKulId) != null && !RegionBossRules.SpawnOf(RegionBossRules.OldKulId).ReturnsNextNight);
        }

        // ---------------- 68 · water bomb ----------------

        [Test]
        public void WaterBomb_HardensTheLavaInsideForItsTime()
        {
            var splash = new WaterSplash(new Vector2(10f, 10f), 100.0);
            Assert.IsTrue(WaterBombRules.Covers(splash, new Vector2(10f + WaterBombRules.Radius - 0.1f, 10f), 101.0));
            Assert.IsFalse(WaterBombRules.Covers(splash, new Vector2(10f + WaterBombRules.Radius + 0.1f, 10f), 101.0));
            Assert.IsFalse(WaterBombRules.Covers(splash, new Vector2(10f, 10f), 100.0 + WaterBombRules.Seconds + 0.01), "dried");
            Assert.IsFalse(WaterBombRules.Covers(splash, new Vector2(10f, 10f), 99.0), "not thrown yet");

            var splashes = new List<WaterSplash> { splash, new WaterSplash(Vector2.zero, 0.0) };
            WaterBombRules.Prune(splashes, 50.0);
            Assert.AreEqual(1, splashes.Count, "the old splash dried");
        }

        [Test]
        public void WaterBomb_CoolsTheTurtlesWake_AndTheWakeNoLongerBursts()
        {
            var wake = new List<Vector2> { new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(20f, 0f) };
            var splashes = new List<WaterSplash> { new WaterSplash(new Vector2(1f, 0f), 10.0) };
            Assert.AreEqual(2, WaterBombRules.CoolWake(wake, splashes, 12.0));
            Assert.AreEqual(1, wake.Count);
            Assert.IsFalse(RegionBossRules.WakeHits(wake, new Vector2(0f, 0f)), "the cooled spots do not burst");
            Assert.IsTrue(RegionBossRules.WakeHits(wake, new Vector2(20f, 0f)), "outside the splash it still does");
            Assert.AreEqual(0, WaterBombRules.CoolWake(wake, splashes, 40.0), "dried: nothing more cools");

            WaterBombBoard.Clear();
            var spots = new List<Vector2> { new Vector2(3f, 3f) };
            WaterBombBoard.Splash(new Vector2(3f, 3f), 5.0);
            Assert.IsTrue(WaterBombBoard.Covers(new Vector2(4f, 3f), 6.0));
            Assert.AreEqual(1, WaterBombBoard.CoolWake(spots, 6.0));
            WaterBombBoard.Clear();
        }

        [Test]
        public void WaterBomb_IsAConsumableThatThrowsWater()
        {
            Assert.AreEqual(WaterBombRules.ItemId, ConsumableEffectRules.WaterBomb);
            Assert.IsTrue(ConsumableEffectRules.ThrowsWater(WaterBombRules.ItemId));
            Assert.IsFalse(ConsumableEffectRules.ThrowsWater(ConsumableEffectRules.PureAirBottle));
        }

        // ---------------- items, shops, texts ----------------

        [Test]
        public void NewItems_HaveAssetsIconsAndAShop()
        {
            ItemDefinition bomb = ItemDatabase.GetById(WaterBombRules.ItemId);
            Assert.IsNotNull(bomb);
            Assert.IsTrue(bomb.IsConsumable);
            foreach (string id in new[] { IceBaitRules.GrubItemId, IceBaitRules.SilverItemId, WaterBombRules.ItemId })
            {
                ItemDefinition item = ItemDatabase.GetById(id);
                Assert.IsNotNull(item, id);
                Assert.IsNotNull(item.Icon, id + " has an icon");
            }

            Assert.IsFalse(ItemDatabase.GetById(IceBaitRules.SilverItemId).IsConsumable, "a bait goes on the hook, not in the mouth");
            foreach ((string shop, string id) in new[] { (CityShopCatalog.PelesPamour, IceBaitRules.GrubItemId), (CityShopCatalog.PelesPamour, IceBaitRules.SilverItemId),
                         (CityShopCatalog.FerroPretoKurogane, WaterBombRules.ItemId) })
            {
                CityShopEntry entry = CityShopCatalog.EntryOf(shop, id);
                Assert.IsNotNull(entry, id);
                Assert.IsFalse(entry.Rotating, id + " is always on the counter");
                ItemDefinition item = ItemDatabase.GetById(id);
                Assert.That(entry.Price, Is.InRange(Mathf.FloorToInt(item.Value * 0.8f), Mathf.CeilToInt(item.Value * 1.25f)), id);
            }
        }

        [Test]
        public void Texts_HaveBothLanguages_AndALinePerCousin()
        {
            foreach (var entries in new[] { IceBaitTexts.Entries, WaterBombTexts.Entries })
            {
                foreach (var entry in entries)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key);
                    Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key);
                    StringAssert.DoesNotContain("Dona ", entry.PtBR);
                    StringAssert.DoesNotContain("Seu ", entry.PtBR);
                }
            }

            foreach (CharacterId hero in Cousins)
            {
                Assert.IsNotNull(IceBaitTexts.CalledLineFor(hero), hero.ToString());
                Assert.IsNotNull(WaterBombTexts.LineFor(hero), hero.ToString());
            }

            Assert.IsNull(IceBaitTexts.CalledLineFor(null));
            StringAssert.Contains("{0}", IceBaitTexts.Get(null, IceBaitTexts.Tug));
            StringAssert.Contains("{0}", WaterBombTexts.Get(null, WaterBombTexts.Wake));
        }
    }
}
