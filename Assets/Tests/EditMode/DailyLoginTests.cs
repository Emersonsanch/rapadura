using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Daily;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.UI.Daily;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Recompensa diária em sequência" (<see cref="DailyLoginRules"/>, <see cref="DailyLoginService"/>): the 7-day track,
    /// the one-step-back penalty for a missed day, the clock guard, the reward table, the save round-trip (old saves →
    /// day 1) and grants through the bag's fit API with the overflow going to the village chest.
    /// </summary>
    public class DailyLoginTests
    {
        private sealed class FakeClock : IDailyClock
        {
            public DateTime Now { get; set; } = new DateTime(2026, 10, 29, 9, 0, 0);
        }

        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created)
            {
                Object.DestroyImmediate(o);
            }

            _created.Clear();
        }

        private static DailyLoginState Claimed(int day, string key) => new DailyLoginState { LastClaimedDay = day, LastClaimDayKey = key };

        // ---------------- progression ----------------

        [Test]
        public void NeverClaimed_OffersDay1()
        {
            DailyLoginOffer offer = DailyLoginRules.Evaluate(new DailyLoginState(), "2026-10-29");
            Assert.IsTrue(offer.CanClaim);
            Assert.AreEqual(1, offer.Day);
            Assert.IsFalse(offer.SteppedBack);
        }

        [Test]
        public void ConsecutiveDays_WalkTheTrack_AndWrapAfter7()
        {
            var state = new DailyLoginState();
            var day = new DateTime(2026, 10, 1);
            var seen = new List<int>();
            for (int i = 0; i < 9; i++)
            {
                string key = DailyLoginRules.ToDayKey(day.AddDays(i));
                DailyLoginOffer offer = DailyLoginRules.Evaluate(state, key);
                Assert.IsTrue(DailyLoginRules.Claim(state, offer, key));
                seen.Add(offer.Day);
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 1, 2 }, seen);
            Assert.AreEqual(1, state.CyclesCompleted);
            Assert.AreEqual(9, state.TotalClaims);
        }

        [Test]
        public void SameDay_ClaimsOnlyOnce()
        {
            var state = new DailyLoginState();
            DailyLoginOffer offer = DailyLoginRules.Evaluate(state, "2026-10-29");
            Assert.IsTrue(DailyLoginRules.Claim(state, offer, "2026-10-29"));
            DailyLoginOffer again = DailyLoginRules.Evaluate(state, "2026-10-29");
            Assert.IsFalse(again.CanClaim);
            Assert.IsFalse(DailyLoginRules.Claim(state, again, "2026-10-29"));
            Assert.AreEqual(1, state.TotalClaims);
        }

        // ---------------- miss penalty ----------------

        [Test]
        public void MissedDay_StepsBackOneDay_NeverToZero()
        {
            DailyLoginOffer offer = DailyLoginRules.Evaluate(Claimed(4, "2026-10-20"), "2026-10-22");
            Assert.IsTrue(offer.CanClaim);
            Assert.AreEqual(4, offer.Day, "Would have been day 5; one step back = day 4.");
            Assert.IsTrue(offer.SteppedBack);

            Assert.AreEqual(4, DailyLoginRules.Evaluate(Claimed(4, "2026-10-20"), "2026-12-25").Day, "However long the absence, one step.");
            Assert.AreEqual(1, DailyLoginRules.Evaluate(Claimed(1, "2026-10-20"), "2026-10-25").Day, "Never below day 1.");
            Assert.AreEqual(1, DailyLoginRules.Evaluate(Claimed(7, "2026-10-20"), "2026-10-25").Day, "After day 7 the track restarts at 1.");
            Assert.AreEqual(6, DailyLoginRules.Evaluate(Claimed(6, "2026-10-20"), "2026-10-23").Day);
        }

        [Test]
        public void ClockTurnedBack_ClaimsNothing()
        {
            DailyLoginOffer offer = DailyLoginRules.Evaluate(Claimed(3, "2026-10-29"), "2026-10-20");
            Assert.IsFalse(offer.CanClaim);
        }

        [Test]
        public void GarbageDayKeys_AreSafe()
        {
            Assert.IsFalse(DailyLoginRules.Evaluate(new DailyLoginState(), "ontem").CanClaim);
            Assert.AreEqual(1, DailyLoginRules.Evaluate(Claimed(5, "???"), "2026-10-29").Day);
        }

        // ---------------- reward table ----------------

        [Test]
        public void RewardTable_Has7Days_GrowingTowardsARareOnDay7()
        {
            for (int day = 1; day <= 7; day++)
            {
                Assert.IsNotEmpty(DailyLoginRules.RewardsFor(day), $"day {day}");
            }

            Assert.AreEqual(DailyLoginRules.GoldItemId, DailyLoginRules.RewardsFor(1)[0].ItemId);
            Assert.AreEqual(DailyLoginRules.HealthPotionItemId, DailyLoginRules.RewardsFor(2)[0].ItemId);
            Assert.AreEqual(DailyLoginRules.IronItemId, DailyLoginRules.RewardsFor(3)[0].ItemId);
            Assert.AreEqual(DailyRewardKind.EventInvite, DailyLoginRules.RewardsFor(5)[0].Kind);
            Assert.AreEqual(DailyRewardKind.RareGear, DailyLoginRules.RewardsFor(7)[0].Kind);

            int GoldOf(int d) => DailyLoginRules.RewardsFor(d).Where(e => e.ItemId == DailyLoginRules.GoldItemId).Sum(e => e.Quantity);
            Assert.Less(GoldOf(1), GoldOf(4));
            Assert.Less(GoldOf(4), GoldOf(7));
        }

        [Test]
        public void RewardItems_ExistInTheDatabase()
        {
            for (int day = 1; day <= 7; day++)
            {
                foreach (DailyRewardEntry entry in DailyLoginRules.RewardsFor(day))
                {
                    (ItemDefinition item, int quantity) = DailyLoginService.Resolve(entry, 0);
                    Assert.IsNotNull(item, $"day {day}: {entry.ItemId} (or its fallback) missing");
                    Assert.Greater(quantity, 0);
                }
            }

            foreach (string id in DailyLoginRules.RareGearBases)
            {
                Assert.IsNotNull(ItemDatabase.GetById(id), id);
            }
        }

        [Test]
        public void EventInvite_FallsBackToGold_WhileTheItemDoesNotExist()
        {
            DailyRewardEntry invite = DailyLoginRules.RewardsFor(5)[0];
            (ItemDefinition item, int quantity) = DailyLoginService.Resolve(invite, 0);
            if (ItemDatabase.GetById(DailyLoginRules.EventInviteItemId) == null)
            {
                Assert.AreEqual(DailyLoginRules.GoldItemId, item.ItemId);
                Assert.AreEqual(DailyLoginRules.EventInviteFallbackGold, quantity);
            }
            else
            {
                Assert.AreEqual(DailyLoginRules.EventInviteItemId, item.ItemId);
            }
        }

        [Test]
        public void RareGearBase_RotatesByCycle()
        {
            Assert.AreEqual(DailyLoginRules.RareGearBases[0], DailyLoginRules.RareGearBaseFor(0));
            Assert.AreEqual(DailyLoginRules.RareGearBases[1], DailyLoginRules.RareGearBaseFor(1));
            Assert.AreEqual(DailyLoginRules.RareGearBases[0], DailyLoginRules.RareGearBaseFor(DailyLoginRules.RareGearBases.Length));
        }

        // ---------------- service ----------------

        private DailyLoginService NewService(FakeClock clock) => new DailyLoginService(clock, _ => { });

        private InventoryManager NewBag()
        {
            var go = new GameObject("DailyLoginTestsBag");
            _created.Add(go);
            return go.AddComponent<InventoryManager>();
        }

        [Test]
        public void Claim_GivesDay1Gold_IntoTheBag_OnceADay()
        {
            var clock = new FakeClock();
            DailyLoginService service = NewService(clock);
            InventoryManager bag = NewBag();
            var chest = new VillageChestStorage();

            List<DailyRewardGrant> grants = service.Claim(bag, chest);
            Assert.IsNotNull(grants);
            Assert.AreEqual(20, bag.GetTotalCount(DailyLoginRules.GoldItemId));
            Assert.IsNull(service.Claim(bag, chest), "Second claim on the same day gives nothing.");

            clock.Now = clock.Now.AddDays(1);
            service.Claim(bag, chest);
            Assert.AreEqual(2, bag.GetTotalCount(DailyLoginRules.HealthPotionItemId));
        }

        [Test]
        public void Claim_WithoutRoomInTheBag_GoesToTheChest()
        {
            var clock = new FakeClock();
            DailyLoginService service = NewService(clock);
            var chest = new VillageChestStorage();

            List<DailyRewardGrant> grants = service.Claim(null, chest);
            Assert.AreEqual(1, grants.Count);
            Assert.AreEqual(20, grants[0].ToChest);
            Assert.AreEqual(20, chest.GetTotalCount(DailyLoginRules.GoldItemId));
        }

        [Test]
        public void Day7_GivesARarePiece()
        {
            var clock = new FakeClock();
            DailyLoginService service = NewService(clock);
            service.RestoreState(Claimed(6, DailyLoginRules.ToDayKey(clock.Now.AddDays(-1))));
            InventoryManager bag = NewBag();

            List<DailyRewardGrant> grants = service.Claim(bag, new VillageChestStorage());
            Assert.IsTrue(grants.Any(g => !string.IsNullOrEmpty(g.GeneratedName) && g.ToBag == 1));
            Assert.IsTrue(bag.Slots.Any(s => !s.IsEmpty && s.generatedRarity == ItemRarity.Rare));
            Assert.AreEqual(1, service.Snapshot.CyclesCompleted);
        }

        [Test]
        public void SaveRoundTrip_AndOldSavesStartAtDay1()
        {
            var clock = new FakeClock();
            DailyLoginService service = NewService(clock);
            service.Claim(null, new VillageChestStorage());
            var json = JsonUtility.ToJson(service.CaptureState());

            DailyLoginService loaded = NewService(clock);
            loaded.RestoreState(JsonUtility.FromJson<DailyLoginState>(json));
            Assert.AreEqual(1, loaded.Snapshot.LastClaimedDay);
            Assert.IsFalse(loaded.Offer.CanClaim);

            DailyLoginService legacy = NewService(clock);
            legacy.ResetForNewSession();
            Assert.AreEqual(1, legacy.Offer.Day);
            Assert.IsTrue(legacy.Offer.CanClaim);
        }

        [Test]
        public void Preview_RoundTripsThroughThePrefsFormat()
        {
            DailyLoginState state = Claimed(4, "2026-10-29");
            DailyLoginState back = DailyLoginService.ParsePreview(DailyLoginService.FormatPreview(state));
            Assert.AreEqual(4, back.LastClaimedDay);
            Assert.AreEqual("2026-10-29", back.LastClaimDayKey);
            Assert.AreEqual(0, DailyLoginService.ParsePreview("lixo").LastClaimedDay);
        }

        // ---------------- popup ----------------

        [Test]
        public void TrackCells_MarkClaimedTodayAndUpcoming()
        {
            var offer = new DailyLoginOffer(true, 3, false);
            Assert.AreEqual(DailyTrackCellState.Claimed, DailyLoginPopupView.CellState(2, offer, 2));
            Assert.AreEqual(DailyTrackCellState.Today, DailyLoginPopupView.CellState(3, offer, 2));
            Assert.AreEqual(DailyTrackCellState.Upcoming, DailyLoginPopupView.CellState(4, offer, 2));

            var done = new DailyLoginOffer(false, 3, false);
            Assert.AreEqual(DailyTrackCellState.Claimed, DailyLoginPopupView.CellState(3, done, 3));
            Assert.AreEqual(DailyTrackCellState.Upcoming, DailyLoginPopupView.CellState(4, done, 3));

            var view = new DailyLoginPopupView();
            view.Populate(null, offer, Claimed(2, "2026-10-28"), canClaimHere: true);
            Assert.AreEqual(DailyLoginRules.TrackLength, view.Track.childCount);
            Assert.AreEqual(UnityEngine.UIElements.DisplayStyle.Flex, view.Claim.style.display.value);
        }
    }
}
