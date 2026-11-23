using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Trade;
using Rapadura.Gameplay.World.Cities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure base of the trade between players (Fase 12.3): opening (<see cref="TradeRules"/>), the state machine
    /// (<see cref="TradeSession"/>: put/take, any change unlocks both, lock → summary → confirm, cancel, expiry), what can
    /// be traded (<see cref="TradeItemRules"/>), the atomic settlement (<see cref="TradeSettlement"/>) including the
    /// duplication attempts, the new-account limits (<see cref="TradeAccountProtection"/>), the record and the texts.
    /// </summary>
    public class TradeTests
    {
        private const int Cells = 64;
        private const long Unix = 1_794_916_800L; // 2026-11-17 12:00 UTC

        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        private ItemDefinition _sword;
        private ItemDefinition _herb;
        private ItemDefinition _gold;
        private ItemDefinition _rareRing;
        private ItemDefinition _anvil;

        [SetUp]
        public void SetUp()
        {
            _sword = Create("trade_sword", ItemType.Weapon, ItemCategory.MeleeWeapon, 1, ItemRarity.Common, weight: 3f);
            _herb = Create("trade_herb", ItemType.Material, ItemCategory.Herb, 50, ItemRarity.Common, weight: 0.2f);
            _gold = Create(TradeRules.GoldItemId, ItemType.Currency, ItemCategory.Gold, 9999, ItemRarity.Common, weight: 0f);
            _rareRing = Create("trade_ring", ItemType.Armor, ItemCategory.Accessory, 1, ItemRarity.Epic, weight: 0.1f);
            _anvil = Create("trade_anvil", ItemType.Material, ItemCategory.Stone, 1, ItemRarity.Common, weight: 40f);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created.Where(o => o != null))
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
            _items.Clear();
        }

        // ------------------------------------------------------------------
        // Opening
        // ------------------------------------------------------------------

        [Test]
        public void Open_SameCity_AllowsAcrossThePlaza_WildsOnlyFaceToFace()
        {
            var cityA = new TradePresence("p1", ZoneKind.City, "vila");
            var cityB = new TradePresence("p2", ZoneKind.City, "vila");
            Assert.AreEqual(TradeRefusal.None, TradeRules.CanOpen(cityA, cityB, 35f));
            Assert.AreEqual(TradeRefusal.TooFar, TradeRules.CanOpen(cityA, cityB, TradeRules.CityTradeDistance + 1f));

            var wildA = new TradePresence("p1", ZoneKind.Wilds);
            var wildB = new TradePresence("p2", ZoneKind.Wilds);
            Assert.AreEqual(TradeRefusal.None, TradeRules.CanOpen(wildA, wildB, TradeRules.TradeDistance));
            Assert.AreEqual(TradeRefusal.TooFar, TradeRules.CanOpen(wildA, wildB, TradeRules.TradeDistance + 0.5f));

            // One in the city, one outside: face to face only.
            Assert.AreEqual(TradeRefusal.TooFar, TradeRules.CanOpen(cityA, wildB, 10f));
            Assert.AreEqual(TradeRefusal.None, TradeRules.CanOpen(cityA, wildB, 3f));

            // Two different cities are not "the same safe zone".
            var otherCity = new TradePresence("p2", ZoneKind.City, "farnkesh");
            Assert.AreEqual(TradeRefusal.TooFar, TradeRules.CanOpen(cityA, otherCity, 20f));
            Assert.AreEqual(TradeRefusal.TooFar, TradeRules.CanOpen(cityA, cityB, -1f));
        }

        [Test]
        public void Open_RefusesSelf_Unavailable_Busy_AndCombatOutsideTheCity()
        {
            var a = new TradePresence("p1", ZoneKind.Wilds);
            Assert.AreEqual(TradeRefusal.SamePlayer, TradeRules.CanOpen(a, new TradePresence("p1", ZoneKind.Wilds), 1f));
            Assert.AreEqual(TradeRefusal.PartnerUnavailable, TradeRules.CanOpen(a, new TradePresence("p2", ZoneKind.Wilds, available: false), 1f));
            Assert.AreEqual(TradeRefusal.PartnerUnavailable, TradeRules.CanOpen(a, new TradePresence(null, ZoneKind.Wilds), 1f));
            Assert.AreEqual(TradeRefusal.AlreadyTrading, TradeRules.CanOpen(a, new TradePresence("p2", ZoneKind.Wilds, inAnotherTrade: true), 1f));

            var fighting = new TradePresence("p2", ZoneKind.Wilds, secondsSinceCombat: 3d);
            Assert.AreEqual(TradeRefusal.InCombat, TradeRules.CanOpen(a, fighting, 1f));
            var rested = new TradePresence("p2", ZoneKind.Wilds, secondsSinceCombat: SafeZoneRules.PlayerCombatTagSeconds);
            Assert.AreEqual(TradeRefusal.None, TradeRules.CanOpen(a, rested, 1f));

            // In a city nobody fights, so a stale combat clock does not matter.
            Assert.AreEqual(TradeRefusal.None, TradeRules.CanOpen(
                new TradePresence("p1", ZoneKind.City, "vila", secondsSinceCombat: 1d),
                new TradePresence("p2", ZoneKind.City, "vila", secondsSinceCombat: 1d), 5f));

            Assert.AreEqual(TradeRefusal.SessionClosed, TradeSession.TryOpen(null, a, new TradePresence("p2", ZoneKind.Wilds), 1f, 0d, out TradeSession none));
            Assert.IsNull(none);
            Assert.AreEqual(TradeRefusal.TooFar, TradeSession.TryOpen("t", a, new TradePresence("p2", ZoneKind.Wilds), 50f, 0d, out none));
            Assert.IsNull(none);
        }

        // ------------------------------------------------------------------
        // State machine
        // ------------------------------------------------------------------

        [Test]
        public void AnyChange_UndoesBothLocksAndConfirmations_AndBumpsRevision()
        {
            InventorySlotData[] bagA = Bag((0, "trade_sword", 1, 0, 0), (1, "trade_herb", 20, 1, 0));
            InventorySlotData[] bagB = Bag((0, "trade_herb", 10, 0, 0));
            TradeSession session = Open();

            Assert.AreEqual(TradeRefusal.None, session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.None, session.Lock(TradeSide.A, 2d));
            Assert.IsTrue(session.IsLocked(TradeSide.A));
            Assert.AreEqual(TradeState.Negotiating, session.State);

            int before = session.Revision;
            Assert.AreEqual(TradeRefusal.None, session.AddItem(TradeSide.B, bagB, 0, 5, Resolve, 3d));
            Assert.IsFalse(session.IsLocked(TradeSide.A), "the other side's change unlocks me");
            Assert.Greater(session.Revision, before);

            LockBoth(session, 4d);
            Assert.AreEqual(TradeState.Locked, session.State);
            Assert.AreEqual(TradeRefusal.None, session.Confirm(TradeSide.A, session.Revision, 5d));
            Assert.IsTrue(session.IsConfirmed(TradeSide.A));

            Assert.AreEqual(TradeRefusal.None, session.SetGold(TradeSide.B, 0, 0, 6d), "same gold is not a change");
            Assert.IsTrue(session.IsConfirmed(TradeSide.A));

            Assert.AreEqual(TradeRefusal.None, session.RemoveItem(TradeSide.B, 0, 7d));
            Assert.IsFalse(session.IsConfirmed(TradeSide.A));
            Assert.IsFalse(session.IsLocked(TradeSide.A));
            Assert.IsFalse(session.IsLocked(TradeSide.B));
            Assert.AreEqual(TradeState.Negotiating, session.State);

            LockBoth(session, 8d);
            Assert.AreEqual(TradeRefusal.None, session.Unlock(TradeSide.B, 9d));
            Assert.IsFalse(session.IsLocked(TradeSide.A), "taking a lock back also undoes the other");
        }

        [Test]
        public void Confirm_NeedsBothLocked_TheCurrentRevision_AndSomethingOnTheTable()
        {
            InventorySlotData[] bagA = Bag((0, "trade_sword", 1, 0, 0));
            TradeSession session = Open();

            Assert.AreEqual(TradeRefusal.NotLocked, session.Confirm(TradeSide.A, session.Revision, 1d));
            LockBoth(session, 1d);
            Assert.AreEqual(TradeRefusal.EmptyTrade, session.Confirm(TradeSide.A, session.Revision, 2d));

            Assert.AreEqual(TradeRefusal.None, session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 3d));
            int seen = session.Revision;
            Assert.AreEqual(TradeRefusal.None, session.SetGold(TradeSide.B, 0, 0, 3.5d));
            LockBoth(session, 4d);
            Assert.AreEqual(TradeRefusal.None, session.Confirm(TradeSide.A, seen, 5d));

            Assert.AreEqual(TradeRefusal.None, session.RemoveItem(TradeSide.A, 0, 6d));
            Assert.AreEqual(TradeRefusal.None, session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 6d));
            LockBoth(session, 7d);
            Assert.AreEqual(TradeRefusal.StaleSummary, session.Confirm(TradeSide.B, seen, 8d), "a summary from before the change");
            Assert.AreEqual(TradeRefusal.None, session.Confirm(TradeSide.B, session.Revision, 8d));
            Assert.IsFalse(session.ReadyToSettle);
            Assert.AreEqual(TradeRefusal.None, session.Confirm(TradeSide.A, session.Revision, 9d));
            Assert.IsTrue(session.ReadyToSettle);
        }

        [Test]
        public void AddItem_Refuses_BadSlots_Quantities_TheSameStackTwice_Gold_AndAFullWindow()
        {
            InventorySlotData[] bag = Bag((0, "trade_herb", 20, 0, 0), (1, TradeRules.GoldItemId, 500, 1, 0));
            TradeSession session = Open();

            Assert.AreEqual(TradeRefusal.InvalidSlot, session.AddItem(TradeSide.A, bag, -1, 1, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.InvalidSlot, session.AddItem(TradeSide.A, bag, Cells, 1, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.EmptySlot, session.AddItem(TradeSide.A, bag, 5, 1, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.InvalidQuantity, session.AddItem(TradeSide.A, bag, 0, 0, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.InvalidQuantity, session.AddItem(TradeSide.A, bag, 0, -3, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.NotEnoughItems, session.AddItem(TradeSide.A, bag, 0, 21, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.GoldItemUseGoldField, session.AddItem(TradeSide.A, bag, 1, 100, Resolve, 1d));

            Assert.AreEqual(TradeRefusal.None, session.AddItem(TradeSide.A, bag, 0, 10, Resolve, 1d));
            Assert.AreEqual(TradeRefusal.AlreadyOffered, session.AddItem(TradeSide.A, bag, 0, 10, Resolve, 1d), "the same stack twice");
            Assert.AreEqual(1, session.OfferOf(TradeSide.A).Lines.Count);

            Assert.AreEqual(TradeRefusal.InvalidQuantity, session.SetGold(TradeSide.A, -1, 500, 1d));
            Assert.AreEqual(TradeRefusal.NotEnoughGold, session.SetGold(TradeSide.A, 501, 500, 1d));
            Assert.AreEqual(TradeRefusal.GoldOverTradeLimit, session.SetGold(TradeSide.A, TradeRules.MaxGoldPerTrade + 1, long.MaxValue, 1d));
            Assert.AreEqual(TradeRefusal.None, session.SetGold(TradeSide.A, 500, 500, 1d));

            var many = new List<(int, string, int, int, int)>();
            for (int i = 0; i < TradeRules.MaxItemLinesPerSide + 1; i++)
            {
                many.Add((i, "trade_herb", 1, i % 8, i / 8));
            }

            InventorySlotData[] bagB = Bag(many.ToArray());
            for (int i = 0; i < TradeRules.MaxItemLinesPerSide; i++)
            {
                Assert.AreEqual(TradeRefusal.None, session.AddItem(TradeSide.B, bagB, i, 1, Resolve, 2d));
            }

            Assert.AreEqual(TradeRefusal.TooManyLines, session.AddItem(TradeSide.B, bagB, TradeRules.MaxItemLinesPerSide, 1, Resolve, 2d));
        }

        [Test]
        public void Cancel_AndExpiry_CloseForGood()
        {
            TradeSession session = Open();
            Assert.IsTrue(session.Cancel(TradeSide.B, 5d));
            Assert.AreEqual(TradeState.Cancelled, session.State);
            Assert.AreEqual(TradeSide.B, session.CancelledBy);
            Assert.IsFalse(session.Cancel(TradeSide.A, 6d));
            Assert.AreEqual(TradeRefusal.SessionClosed, session.Lock(TradeSide.A, 6d));

            TradeSession idle = Open();
            Assert.IsFalse(idle.Tick(TradeRules.IdleTimeoutSeconds - 1d));
            Assert.IsTrue(idle.Tick(TradeRules.IdleTimeoutSeconds));
            Assert.IsFalse(idle.Tick(TradeRules.IdleTimeoutSeconds + 1d), "expires once");
            Assert.AreEqual(TradeState.Expired, idle.State);

            // Activity keeps it alive, but never past the hard limit.
            TradeSession busy = Open();
            InventorySlotData[] bag = Bag((0, "trade_herb", 50, 0, 0));
            for (double t = 60d; t < TradeRules.MaxSessionSeconds; t += 60d)
            {
                Assert.AreEqual(TradeRefusal.None, busy.Lock(TradeSide.A, t), "t=" + t);
            }

            Assert.AreEqual(TradeRefusal.Expired, busy.AddItem(TradeSide.A, bag, 0, 1, Resolve, TradeRules.MaxSessionSeconds));
            Assert.AreEqual(TradeState.Expired, busy.State);
        }

        // ------------------------------------------------------------------
        // What can be traded
        // ------------------------------------------------------------------

        [Test]
        public void Untradeable_StoryRelicsAchievementsMountsHouseCurrencyAndBound()
        {
            Assert.AreEqual(TradeRefusal.None, TradeItemRules.CanTrade(_sword));
            Assert.AreEqual(TradeRefusal.None, TradeItemRules.CanTrade(_herb));
            Assert.AreEqual(TradeRefusal.UnknownItem, TradeItemRules.CanTrade(null));
            Assert.AreEqual(TradeRefusal.GoldItemUseGoldField, TradeItemRules.CanTrade(_gold));
            Assert.AreEqual(TradeRefusal.Currency, TradeItemRules.CanTrade(Create("item_gold_coin", ItemType.Currency, ItemCategory.Currency, 999, ItemRarity.Common)));
            Assert.AreEqual(TradeRefusal.StoryItem, TradeItemRules.CanTrade(Create("item_chave_engenho", ItemType.QuestItem, ItemCategory.QuestItem, 1, ItemRarity.Uncommon)));
            Assert.AreEqual(TradeRefusal.StoryItem, TradeItemRules.CanTrade(Create("story_carta_vo", ItemType.Material, ItemCategory.Miscellaneous, 1, ItemRarity.Common)));
            Assert.AreEqual(TradeRefusal.FamilyRelic, TradeItemRules.CanTrade(Create(PatuaRules.ItemId, ItemType.Material, ItemCategory.Miscellaneous, 5, ItemRarity.Uncommon)));
            Assert.AreEqual(TradeRefusal.FamilyRelic, TradeItemRules.CanTrade(Create(DeathDropRules.RelicIdPrefix + "carranca", ItemType.Material, ItemCategory.Miscellaneous, 1, ItemRarity.Legendary)));
            Assert.AreEqual(TradeRefusal.AchievementItem, TradeItemRules.CanTrade(Create("conquista_estandarte", ItemType.Material, ItemCategory.Miscellaneous, 1, ItemRarity.Rare)));
            Assert.AreEqual(TradeRefusal.MountItem, TradeItemRules.CanTrade(Create("item_whistle_mount_pony", ItemType.Material, ItemCategory.Miscellaneous, 1, ItemRarity.Common)));
            Assert.AreEqual(TradeRefusal.HouseItem, TradeItemRules.CanTrade(Create("casa_quarto_006", ItemType.Material, ItemCategory.Miscellaneous, 1, ItemRarity.Rare)));
            Assert.AreEqual(TradeRefusal.Bound, TradeItemRules.CanTrade(Create("item_asas_carcara_1", ItemType.Armor, ItemCategory.Accessory, 1, ItemRarity.Rare, bound: true)));

            // Mount feed is food, not a mount.
            Assert.AreEqual(TradeRefusal.None, TradeItemRules.CanTrade(Create("item_mount_feed", ItemType.Material, ItemCategory.Food, 20, ItemRarity.Common)));

            // The session refuses them too.
            ItemDefinition bound = _items["item_asas_carcara_1"];
            InventorySlotData[] bag = Bag((0, bound.ItemId, 1, 0, 0));
            Assert.AreEqual(TradeRefusal.Bound, Open().AddItem(TradeSide.A, bag, 0, 1, Resolve, 1d));
        }

        [Test]
        public void Summary_WarnsOfRareItems_OneSidedTrades_AndLargeGold()
        {
            InventorySlotData[] bagA = Bag((0, "trade_ring", 1, 0, 0), (1, "trade_sword", 1, 1, 0), (2, TradeRules.GoldItemId, 9999, 3, 0));
            bagA[1].refineLevel = TradeItemRules.RareRefineLevel;
            Assert.IsTrue(TradeItemRules.IsRare(bagA[0], _rareRing), "epic rarity");
            Assert.IsTrue(TradeItemRules.IsRare(bagA[1], _sword), "+7");
            Assert.IsFalse(TradeItemRules.IsRare(Slot("trade_sword", 1), _sword));
            InventorySlotData excellent = Slot("trade_sword", 1);
            excellent.excellentMask = 1;
            Assert.IsTrue(TradeItemRules.IsRare(excellent, _sword));

            TradeSession session = Open();
            session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 1d);
            TradeSummary summary = session.BuildSummary();
            Assert.IsTrue(summary.Has(TradeWarning.RareItemGiven, TradeSide.A));
            Assert.IsTrue(summary.Has(TradeWarning.GivingForNothing, TradeSide.A));
            Assert.IsFalse(summary.Has(TradeWarning.GivingForNothing, TradeSide.B), "B receives, B gives nothing: no warning for B");
            Assert.IsFalse(summary.Has(TradeWarning.LargeGold, TradeSide.A));

            InventorySlotData[] rich = Bag((0, TradeRules.GoldItemId, 9999, 0, 0));
            long held = TradeRules.LargeGoldWarning;
            Assert.AreEqual(TradeRefusal.None, session.SetGold(TradeSide.B, TradeRules.LargeGoldWarning, held, 2d));
            summary = session.BuildSummary();
            Assert.IsTrue(summary.Has(TradeWarning.LargeGold, TradeSide.B));
            Assert.IsFalse(summary.Has(TradeWarning.GivingForNothing, TradeSide.A));
            Assert.AreEqual(session.Revision, summary.Revision);
            Assert.AreSame(session.OfferOf(TradeSide.B), summary.ReceivedBy(TradeSide.A));
            Assert.AreEqual(9999, TradeSettlement.GoldIn(rich));
        }

        [Test]
        public void Fingerprint_ChangesWithRolledDataAndRefine_NotWithQuantityOrPosition()
        {
            InventorySlotData plain = Slot("trade_sword", 1);
            string fp = TradeItemRules.Fingerprint(plain);
            InventorySlotData moved = plain.CloneWithQuantity(3);
            moved.gridX = 5;
            Assert.AreEqual(fp, TradeItemRules.Fingerprint(moved));

            InventorySlotData refined = plain.CloneWithQuantity(1);
            refined.refineLevel = 9;
            Assert.AreNotEqual(fp, TradeItemRules.Fingerprint(refined));

            InventorySlotData rolled = plain.CloneWithQuantity(1);
            rolled.generatedAffixIds = new[] { "affix_fire" };
            rolled.generatedAffixValues = new[] { 4f };
            Assert.AreNotEqual(fp, TradeItemRules.Fingerprint(rolled));
            Assert.AreEqual(string.Empty, TradeItemRules.Fingerprint(InventorySlotData.Empty()));
        }

        // ------------------------------------------------------------------
        // Settlement
        // ------------------------------------------------------------------

        [Test]
        public void Settle_SwapsItemsAndGold_AtomicallyOnCopies_AndWritesTheRecord()
        {
            InventorySlotData[] bagA = Bag((0, "trade_sword", 1, 0, 0), (1, "trade_herb", 30, 1, 0), (2, TradeRules.GoldItemId, 300, 2, 0));
            bagA[0].refineLevel = 4;
            InventorySlotData[] bagB = Bag((0, "trade_herb", 40, 0, 0), (1, TradeRules.GoldItemId, 1000, 1, 0));
            string snapshotA = Dump(bagA);
            string snapshotB = Dump(bagB);

            TradeSession session = Open();
            session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 1d);
            session.AddItem(TradeSide.A, bagA, 1, 10, Resolve, 1d);
            session.SetGold(TradeSide.A, 100, TradeSettlement.GoldIn(bagA), 1d);
            session.SetGold(TradeSide.B, 750, TradeSettlement.GoldIn(bagB), 1d);
            ConfirmBoth(session, 2d);

            TradeSettlementResult result = TradeSettlement.Settle(session, Side(bagA), Side(bagB), Resolve, 3d, Unix);

            Assert.IsTrue(result.Success, result.Refusal.ToString());
            Assert.AreEqual(TradeState.Completed, session.State);
            Assert.AreEqual(snapshotA, Dump(bagA), "the inputs are never touched");
            Assert.AreEqual(snapshotB, Dump(bagB));

            Assert.AreEqual(0, Count(result.SlotsA, "trade_sword"));
            Assert.AreEqual(1, Count(result.SlotsB, "trade_sword"));
            Assert.AreEqual(4, result.SlotsB.First(s => s.itemId == "trade_sword").refineLevel, "the refine travels with the piece");
            Assert.AreEqual(20, Count(result.SlotsA, "trade_herb"));
            Assert.AreEqual(50, Count(result.SlotsB, "trade_herb"));
            Assert.AreEqual(300 - 100 + 750, TradeSettlement.GoldIn(result.SlotsA));
            Assert.AreEqual(1000 - 750 + 100, TradeSettlement.GoldIn(result.SlotsB));

            // Nothing created or destroyed.
            foreach (string id in new[] { "trade_sword", "trade_herb", TradeRules.GoldItemId })
            {
                Assert.AreEqual(Count(bagA, id) + Count(bagB, id), Count(result.SlotsA, id) + Count(result.SlotsB, id), id);
            }

            Assert.AreEqual(Cells, result.SlotsA.Length);
            AssertNoOverlap(result.SlotsA);
            AssertNoOverlap(result.SlotsB);

            TradeRecord record = result.Record;
            Assert.AreEqual("trade-1", record.tradeId);
            Assert.AreEqual("p1", record.playerA);
            Assert.AreEqual("p2", record.playerB);
            Assert.AreEqual(Unix, record.completedAtUnix);
            Assert.AreEqual(100, record.goldAToB);
            Assert.AreEqual(750, record.goldBToA);
            Assert.AreEqual(2, record.itemsAToB.Length);
            Assert.AreEqual(0, record.itemsBToA.Length);
            Assert.AreEqual(4, record.itemsAToB[0].refineLevel);
            Assert.AreEqual("trade=trade-1 at=1794916800 a=p1 b=p2 a>b=[gold:100;trade_sword*1+4;trade_herb*10+0] b>a=[gold:750]", record.ToLogLine());
            Assert.AreEqual(record.ToLogLine(), JsonUtility.FromJson<TradeRecord>(JsonUtility.ToJson(record)).ToLogLine(), "serializable");
        }

        [Test]
        public void Settle_Twice_NeverPaysTwice()
        {
            InventorySlotData[] bagA = Bag((0, "trade_sword", 1, 0, 0));
            InventorySlotData[] bagB = Bag();
            TradeSession session = Open();
            session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 1d);
            ConfirmBoth(session, 2d);

            Assert.IsTrue(TradeSettlement.Settle(session, Side(bagA), Side(bagB), Resolve, 3d, Unix).Success);
            TradeSettlementResult again = TradeSettlement.Settle(session, Side(bagA), Side(bagB), Resolve, 3d, Unix);
            Assert.IsFalse(again.Success);
            Assert.AreEqual(TradeRefusal.AlreadySettled, again.Refusal);
            Assert.IsNull(again.SlotsB);
            Assert.AreEqual(TradeRefusal.AlreadySettled, session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 4d));
        }

        [Test]
        public void Settle_BeforeBothConfirm_IsRefused_WithoutChangingTheSession()
        {
            InventorySlotData[] bagA = Bag((0, "trade_sword", 1, 0, 0));
            TradeSession session = Open();
            session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 1d);
            LockBoth(session, 2d);
            session.Confirm(TradeSide.A, session.Revision, 2d);

            TradeSettlementResult result = TradeSettlement.Settle(session, Side(bagA), Side(Bag()), Resolve, 3d, Unix);
            Assert.AreEqual(TradeRefusal.NotConfirmed, result.Refusal);
            Assert.IsTrue(session.IsConfirmed(TradeSide.A), "a premature settle does not reset anyone");
            Assert.AreEqual(TradeRefusal.SessionClosed, TradeSettlement.Settle(null, Side(bagA), Side(Bag()), Resolve, 3d, Unix).Refusal);
        }

        [Test]
        public void Settle_ItemSpentBetweenLockAndConclude_IsRefused_AndUnlocksBoth()
        {
            InventorySlotData[] bagA = Bag((0, "trade_herb", 10, 0, 0));
            TradeSession session = Open();
            session.AddItem(TradeSide.A, bagA, 0, 10, Resolve, 1d);
            ConfirmBoth(session, 2d);

            // A uses 3 herbs after confirming (the server's copy of the bag now has 7).
            bagA[0].quantity = 7;
            TradeSettlementResult result = TradeSettlement.Settle(session, Side(bagA), Side(Bag()), Resolve, 3d, Unix);

            Assert.IsFalse(result.Success);
            Assert.AreEqual(TradeRefusal.ItemNoLongerHeld, result.Refusal);
            Assert.AreEqual(TradeSide.A, result.FailedSide);
            Assert.IsNull(result.SlotsA);
            Assert.IsNull(result.Record);
            Assert.AreEqual(TradeState.Negotiating, session.State);
            Assert.IsFalse(session.IsLocked(TradeSide.A) || session.IsLocked(TradeSide.B));
            Assert.AreEqual(TradeRefusal.ItemNoLongerHeld, session.LastSettleRefusal);
            Assert.AreEqual(7, bagA[0].quantity);

            // Spent entirely, or the entry now holds something else.
            bagA[0].Clear();
            ConfirmBoth(session, 4d);
            Assert.AreEqual(TradeRefusal.ItemNoLongerHeld, TradeSettlement.Settle(session, Side(bagA), Side(Bag()), Resolve, 5d, Unix).Refusal);
        }

        [Test]
        public void Settle_EntrySwappedForAWorseCopy_IsRefused()
        {
            InventorySlotData[] bagA = Bag((0, "trade_sword", 1, 0, 0));
            bagA[0].refineLevel = 15;
            TradeSession session = Open();
            session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 1d);
            ConfirmBoth(session, 2d);

            bagA[0].refineLevel = 0; // the +15 left the entry and a +0 of the same id took its place
            TradeSettlementResult result = TradeSettlement.Settle(session, Side(bagA), Side(Bag()), Resolve, 3d, Unix);
            Assert.AreEqual(TradeRefusal.ItemChanged, result.Refusal);
            Assert.AreEqual(TradeSide.A, result.FailedSide);
        }

        [Test]
        public void Settle_GoldSpentAfterConfirming_IsRefused()
        {
            InventorySlotData[] bagB = Bag((0, TradeRules.GoldItemId, 500, 0, 0));
            InventorySlotData[] bagA = Bag((0, "trade_sword", 1, 0, 0));
            TradeSession session = Open();
            session.AddItem(TradeSide.A, bagA, 0, 1, Resolve, 1d);
            session.SetGold(TradeSide.B, 500, 500, 1d);
            ConfirmBoth(session, 2d);

            bagB[0].quantity = 100;
            TradeSettlementResult result = TradeSettlement.Settle(session, Side(bagA), Side(bagB), Resolve, 3d, Unix);
            Assert.AreEqual(TradeRefusal.GoldNoLongerHeld, result.Refusal);
            Assert.AreEqual(TradeSide.B, result.FailedSide);
            Assert.AreEqual(1, Count(bagA, "trade_sword"));
        }

        [Test]
        public void Settle_TheSameStackTwice_IsRefusedEvenIfTheOfferWasForged()
        {
            // The session never lets the same entry in twice; check the settlement's own guard too, by offering the two
            // halves of one stack of 10 (5 + 5, through two sessions sharing the bag) and spending one half first.
            InventorySlotData[] bagA = Bag((0, "trade_herb", 10, 0, 0));
            TradeSession first = Open();
            TradeSession second = Open("trade-2");
            first.AddItem(TradeSide.A, bagA, 0, 10, Resolve, 1d);
            second.AddItem(TradeSide.A, bagA, 0, 10, Resolve, 1d);
            ConfirmBoth(first, 2d);
            ConfirmBoth(second, 2d);

            TradeSettlementResult one = TradeSettlement.Settle(first, Side(bagA), Side(Bag()), Resolve, 3d, Unix);
            Assert.IsTrue(one.Success);

            // The server applies the first result; the second trade now sees the bag without the herbs.
            TradeSettlementResult two = TradeSettlement.Settle(second, Side(one.SlotsA), Side(Bag()), Resolve, 3d, Unix);
            Assert.IsFalse(two.Success);
            Assert.AreEqual(TradeRefusal.ItemNoLongerHeld, two.Refusal);
            Assert.AreEqual(10, Count(one.SlotsB, "trade_herb"));
        }

        [Test]
        public void Settle_BagFull_IsAllOrNothing_ButGivingFreesRoom()
        {
            InventorySlotData[] full = FullOfSwords();
            InventorySlotData[] bagB = Bag((0, "trade_sword", 1, 0, 0), (1, "trade_herb", 5, 1, 0));

            TradeSession session = Open();
            session.AddItem(TradeSide.B, bagB, 0, 1, Resolve, 1d);
            session.AddItem(TradeSide.B, bagB, 1, 5, Resolve, 1d);
            ConfirmBoth(session, 2d);
            TradeSettlementResult result = TradeSettlement.Settle(session, Side(full), Side(bagB), Resolve, 3d, Unix);
            Assert.AreEqual(TradeRefusal.BagFull, result.Refusal);
            Assert.AreEqual(TradeSide.A, result.FailedSide);
            Assert.AreEqual(1, Count(bagB, "trade_sword"), "nothing moved");

            // A sword for a sword: A's bag is still full, but giving one frees the room for the one coming in.
            TradeSession swap = Open("trade-2");
            swap.AddItem(TradeSide.A, full, 0, 1, Resolve, 1d);
            swap.AddItem(TradeSide.B, bagB, 0, 1, Resolve, 1d);
            ConfirmBoth(swap, 2d);
            TradeSettlementResult swapped = TradeSettlement.Settle(swap, Side(full), Side(bagB), Resolve, 3d, Unix);
            Assert.IsTrue(swapped.Success, swapped.Refusal.ToString());
            Assert.AreEqual(Count(full, "trade_sword"), Count(swapped.SlotsA, "trade_sword"));
            AssertNoOverlap(swapped.SlotsA);
        }

        [Test]
        public void Settle_TooHeavy_UnlessTheLoadGoesDown()
        {
            InventorySlotData[] bagA = Bag((0, "trade_herb", 10, 0, 0));
            InventorySlotData[] bagB = Bag((0, "trade_anvil", 1, 0, 0));
            TradeSession session = Open();
            session.AddItem(TradeSide.B, bagB, 0, 1, Resolve, 1d);
            ConfirmBoth(session, 2d);

            TradeSideInput weak = Side(bagA);
            weak.Capacity = 30f;
            TradeSettlementResult result = TradeSettlement.Settle(session, weak, Side(bagB), Resolve, 3d, Unix);
            Assert.AreEqual(TradeRefusal.TooHeavy, result.Refusal);
            Assert.AreEqual(TradeSide.A, result.FailedSide);

            // Already overloaded but getting lighter: allowed (B carries 40 with capacity 10 and gives the anvil away).
            ConfirmBoth(session, 4d);
            weak.Capacity = 60f;
            TradeSideInput overloaded = Side(bagB);
            overloaded.Capacity = 10f;
            Assert.IsTrue(TradeSettlement.Settle(session, weak, overloaded, Resolve, 5d, Unix).Success);
        }

        [Test]
        public void Settle_GoldAboveTheHeldLimit_IsRefused()
        {
            InventorySlotData[] rich = Bag((0, TradeRules.GoldItemId, 9999, 0, 0));
            rich[0].quantity = int.MaxValue; // a corrupt/huge stack: the cap still holds and nothing overflows
            InventorySlotData[] bagB = Bag((0, TradeRules.GoldItemId, 10, 0, 0));
            TradeSession session = Open();
            session.SetGold(TradeSide.B, 10, 10, 1d);
            ConfirmBoth(session, 2d);
            TradeSettlementResult result = TradeSettlement.Settle(session, Side(rich), Side(bagB), Resolve, 3d, Unix);
            Assert.AreEqual(TradeRefusal.GoldOverHeldLimit, result.Refusal);
            Assert.AreEqual(TradeSide.A, result.FailedSide);
        }

        // ------------------------------------------------------------------
        // New accounts
        // ------------------------------------------------------------------

        [Test]
        public void NewAccount_IsYoungOrLowLevel_AndHasDailyLimits()
        {
            Assert.IsTrue(TradeAccountProtection.IsNewAccount(new TradeAccountStanding(10d, 80)));
            Assert.IsTrue(TradeAccountProtection.IsNewAccount(new TradeAccountStanding(500d, TradeAccountProtection.NewAccountLevel - 1)));
            Assert.IsFalse(TradeAccountProtection.IsNewAccount(new TradeAccountStanding(TradeAccountProtection.NewAccountHours, TradeAccountProtection.NewAccountLevel)));
            Assert.IsFalse(TradeAccountProtection.IsNewAccount(TradeAccountStanding.Veteran));

            var fresh = new TradeAccountStanding(2d, 5);
            var tally = new TradeDailyTally { dayKey = "2026-11-17", goldReceived = 19_000 };
            Assert.AreEqual(TradeRefusal.None, TradeAccountProtection.Check(fresh, tally, 1_000, 0, 0, 0));
            Assert.AreEqual(TradeRefusal.NewAccountGoldLimit, TradeAccountProtection.Check(fresh, tally, 1_001, 0, 0, 0));
            Assert.AreEqual(TradeRefusal.NewAccountGoldLimit, TradeAccountProtection.Check(fresh, default, 0, TradeAccountProtection.NewAccountGoldPerDay + 1, 0, 0));
            Assert.AreEqual(TradeRefusal.NewAccountItemLimit, TradeAccountProtection.Check(fresh, default, 0, 0, TradeAccountProtection.NewAccountItemsPerDay + 1, 0));
            Assert.AreEqual(TradeRefusal.None, TradeAccountProtection.Check(TradeAccountStanding.Veteran, tally, 10_000_000, 10_000_000, 999, 999));

            Assert.AreEqual(1_000, TradeAccountProtection.GoldLeftToday(fresh, tally, receiving: true));
            Assert.AreEqual(TradeAccountProtection.NewAccountItemsPerDay, TradeAccountProtection.ItemsLeftToday(fresh, tally, receiving: false));
            Assert.AreEqual(long.MaxValue, TradeAccountProtection.GoldLeftToday(TradeAccountStanding.Veteran, tally, true));

            // A new day starts from zero.
            TradeDailyTally tomorrow = tally.ForDay("2026-11-18");
            Assert.AreEqual(0, tomorrow.goldReceived);
            Assert.AreEqual(19_000, tally.ForDay("2026-11-17").goldReceived);
            Assert.AreEqual("2026-11-17", TradeRules.DayKey(Unix));
        }

        [Test]
        public void Settle_RespectsTheNewAccountLimit_AndUpdatesTheTallies()
        {
            InventorySlotData[] mule = Bag();
            InventorySlotData[] seller = Bag((0, TradeRules.GoldItemId, 9999, 0, 0), (1, TradeRules.GoldItemId, 9999, 1, 0),
                (2, TradeRules.GoldItemId, 9999, 2, 0), (3, "trade_herb", 3, 3, 0));
            TradeSession session = Open();
            session.SetGold(TradeSide.B, 25_000, TradeSettlement.GoldIn(seller), 1d);
            ConfirmBoth(session, 2d);

            TradeSideInput newbie = Side(mule);
            newbie.Standing = new TradeAccountStanding(5d, 3);
            newbie.Tally = new TradeDailyTally { dayKey = "2026-11-16", goldReceived = 20_000 }; // yesterday: does not count
            TradeSettlementResult result = TradeSettlement.Settle(session, newbie, Side(seller), Resolve, 3d, Unix);
            Assert.AreEqual(TradeRefusal.NewAccountGoldLimit, result.Refusal);
            Assert.AreEqual(TradeSide.A, result.FailedSide);

            Assert.AreEqual(TradeRefusal.None, session.SetGold(TradeSide.B, 15_000, TradeSettlement.GoldIn(seller), 4d));
            Assert.AreEqual(TradeRefusal.None, session.AddItem(TradeSide.B, seller, 3, 3, Resolve, 4d));
            ConfirmBoth(session, 5d);
            result = TradeSettlement.Settle(session, newbie, Side(seller), Resolve, 6d, Unix);
            Assert.IsTrue(result.Success, result.Refusal.ToString());
            Assert.AreEqual("2026-11-17", result.TallyA.dayKey);
            Assert.AreEqual(15_000, result.TallyA.goldReceived);
            Assert.AreEqual(1, result.TallyA.itemsReceived);
            Assert.AreEqual(15_000, result.TallyB.goldGiven);
            Assert.AreEqual(1, result.TallyB.itemsGiven);
            Assert.AreEqual(15_000, TradeSettlement.GoldIn(result.SlotsA));
            Assert.AreEqual(3, Count(result.SlotsA, "trade_herb"));
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_EveryRefusalAndWarning_HasEnglishAndPortuguese()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = TradeTexts.Entries.ToDictionary(e => e.Key);
            foreach (TradeRefusal refusal in System.Enum.GetValues(typeof(TradeRefusal)))
            {
                if (refusal == TradeRefusal.None)
                {
                    continue;
                }

                string key = TradeTexts.RefusalKey(refusal);
                Assert.IsTrue(table.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(table[key].En), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(table[key].PtBR), key);
            }

            foreach (TradeWarning warning in System.Enum.GetValues(typeof(TradeWarning)))
            {
                string key = TradeTexts.WarningKey(warning);
                Assert.IsTrue(table.ContainsKey(key), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(table[key].En), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(table[key].PtBR), key);
            }

            Assert.AreEqual("ui.trade.refusal.gold_item_use_gold_field", TradeTexts.RefusalKey(TradeRefusal.GoldItemUseGoldField));
            Assert.AreEqual("Longe demais: cheguem mais perto ou se encontrem na mesma cidade.", TradeTexts.Get(null, TradeRefusal.TooFar));
            Assert.AreEqual(string.Empty, TradeTexts.Get(null, TradeRefusal.None));
            Assert.AreEqual("1500 de ouro", TradeTexts.Format(null, TradeTexts.SummaryGold, 1500));
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private ItemDefinition Resolve(string id) => id != null && _items.TryGetValue(id, out ItemDefinition item) ? item : null;

        private static TradeSession Open(string id = "trade-1")
        {
            Assert.AreEqual(TradeRefusal.None, TradeSession.TryOpen(id,
                new TradePresence("p1", ZoneKind.City, "vila"), new TradePresence("p2", ZoneKind.City, "vila"), 3f, 0d, out TradeSession session));
            return session;
        }

        private static void LockBoth(TradeSession session, double now)
        {
            Assert.AreEqual(TradeRefusal.None, session.Lock(TradeSide.A, now));
            Assert.AreEqual(TradeRefusal.None, session.Lock(TradeSide.B, now));
        }

        private static void ConfirmBoth(TradeSession session, double now)
        {
            LockBoth(session, now);
            int revision = session.BuildSummary().Revision;
            Assert.AreEqual(TradeRefusal.None, session.Confirm(TradeSide.A, revision, now));
            Assert.AreEqual(TradeRefusal.None, session.Confirm(TradeSide.B, revision, now));
            Assert.IsTrue(session.ReadyToSettle);
        }

        private static TradeSideInput Side(InventorySlotData[] slots) => new TradeSideInput { Slots = slots };

        private static InventorySlotData Slot(string id, int quantity) => new InventorySlotData { itemId = id, quantity = quantity };

        private static InventorySlotData[] Bag(params (int index, string id, int quantity, int x, int y)[] entries)
        {
            var slots = new InventorySlotData[Cells];
            for (int i = 0; i < Cells; i++)
            {
                slots[i] = InventorySlotData.Empty();
            }

            foreach ((int index, string id, int quantity, int x, int y) in entries)
            {
                slots[index] = new InventorySlotData { itemId = id, quantity = quantity, gridX = x, gridY = y };
            }

            return slots;
        }

        /// <summary>8 columns of 1×3 swords on rows 0–2 and 3–5, plus herbs filling rows 6–7: no 1×3 area left.</summary>
        private static InventorySlotData[] FullOfSwords()
        {
            var entries = new List<(int, string, int, int, int)>();
            int index = 0;
            for (int band = 0; band < 2; band++)
            {
                for (int x = 0; x < 8; x++)
                {
                    entries.Add((index++, "trade_sword", 1, x, band * 3));
                }
            }

            for (int y = 6; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    entries.Add((index++, "trade_herb", 50, x, y));
                }
            }

            return Bag(entries.ToArray());
        }

        private static int Count(IEnumerable<InventorySlotData> slots, string id) =>
            slots.Where(s => s != null && !s.IsEmpty && s.itemId == id).Sum(s => s.quantity);

        private static string Dump(IEnumerable<InventorySlotData> slots) =>
            string.Join(";", slots.Select(s => s.IsEmpty ? "-" : $"{s.itemId}x{s.quantity}@{s.gridX},{s.gridY}+{s.refineLevel}"));

        private void AssertNoOverlap(InventorySlotData[] slots)
        {
            var unplaced = new List<int>();
            BagGridRules.Build(InventoryManager.DefaultColumns, InventoryManager.DefaultRows, slots, Resolve, unplaced);
            Assert.IsEmpty(unplaced, "every entry has its own area on the grid");
        }

        private ItemDefinition Create(string id, ItemType type, ItemCategory category, int maxStack, ItemRarity rarity, float weight = 1f, bool bound = false)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_category").enumValueIndex = (int)category;
            serialized.FindProperty("_rarity").enumValueIndex = (int)rarity;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_weight").floatValue = weight;
            serialized.FindProperty("_isBound").boolValue = bound;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            _items[id] = item;
            return item;
        }
    }
}
