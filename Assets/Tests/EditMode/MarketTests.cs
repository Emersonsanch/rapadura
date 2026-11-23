using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Market;
using Rapadura.Gameplay.Trade;
using Rapadura.Gameplay.World.Cities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The pure base of the player market (Fase 12.3): fees (<see cref="MarketRules"/>), stall spots
    /// (<see cref="StallSpotRegistry"/>), the stall (<see cref="MarketStall"/>: escrowed stock, atomic purchase, the race for
    /// the last unit, closing to the mailbox), the auction house (<see cref="AuctionHouse"/>: deposit, increments, refund
    /// of the outbid, anti-sniping, buyout, duplicate bids, limits), the mailbox (<see cref="MarketPost"/>: take all or
    /// nothing, expiry back to the owner), the new-account limits, the logs, the texts and the gold-drain estimate.
    /// </summary>
    public class MarketTests
    {
        private const int Cells = 64;
        private const long T0 = 1_794_916_800L; // 2026-11-17 12:00 UTC
        private const string City = MarketRules.HubCityId;

        private readonly List<Object> _created = new List<Object>();
        private readonly Dictionary<string, ItemDefinition> _items = new Dictionary<string, ItemDefinition>();

        [SetUp]
        public void SetUp()
        {
            Create("mk_sword", ItemType.Weapon, ItemCategory.MeleeWeapon, 1, weight: 3f);
            Create("mk_herb", ItemType.Material, ItemCategory.Herb, 50, weight: 0.2f);
            Create(TradeRules.GoldItemId, ItemType.Currency, ItemCategory.Gold, 9999, weight: 0f);
            Create("mk_wings", ItemType.Armor, ItemCategory.Accessory, 1, weight: 1f, bound: true);
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
        // Fees
        // ------------------------------------------------------------------

        [Test]
        public void Fees_RoundDown_AndDepositScalesWithDurationAndTheLargerPrice()
        {
            Assert.AreEqual(2, MarketRules.StallTax(100));
            Assert.AreEqual(0, MarketRules.StallTax(49), "tiny sales go untaxed");
            Assert.AreEqual(50, MarketRules.AuctionTax(1000));
            Assert.AreEqual(50_000, MarketRules.AuctionTax(1_000_000));
            Assert.AreEqual(0, MarketRules.Fee(-5, 500));

            Assert.AreEqual(10, MarketRules.Deposit(1000, 0, AuctionDuration.Short12h));
            Assert.AreEqual(20, MarketRules.Deposit(1000, 0, AuctionDuration.Medium24h));
            Assert.AreEqual(40, MarketRules.Deposit(1000, 0, AuctionDuration.Long48h));
            Assert.AreEqual(MarketRules.MinDeposit, MarketRules.Deposit(10, 0, AuctionDuration.Short12h));
            Assert.AreEqual(100, MarketRules.Deposit(1, 10_000, AuctionDuration.Short12h), "a 1-gold start does not dodge the deposit");
            Assert.AreEqual(40_000, MarketRules.Deposit(1_000_000, 0, AuctionDuration.Long48h));
            Assert.LessOrEqual(MarketRules.Deposit(1_000_000, 1_000_000, AuctionDuration.Long48h), MarketRules.MaxDeposit);

            Assert.AreEqual(100, MarketRules.MinNextBid(100, 0, false));
            Assert.AreEqual(105, MarketRules.MinNextBid(100, 100, true));
            Assert.AreEqual(11, MarketRules.MinNextBid(1, 10, true), "never less than +1");
            Assert.AreEqual(1_050_000, MarketRules.MinNextBid(1, 1_000_000, true));
        }

        // ------------------------------------------------------------------
        // Stall spots
        // ------------------------------------------------------------------

        [Test]
        public void Spots_PerCity_ReservedOnlyInsideTheCity_FirstComeFirstServed()
        {
            Assert.AreEqual(MarketRules.HubStallSpots, MarketRules.StallSpotsFor(MarketRules.HubCityId));
            Assert.AreEqual(MarketRules.CityStallSpots, MarketRules.StallSpotsFor(CityCatalog.AreiaBranca));
            Assert.AreEqual(0, MarketRules.StallSpotsFor("nowhere"));

            var spots = new StallSpotRegistry();
            Assert.AreEqual(MarketRefusal.NotInCity, spots.Reserve(new TradePresence("p1", ZoneKind.Wilds), City, 0, T0));
            Assert.AreEqual(MarketRefusal.WrongCity, spots.Reserve(InCity("p1", CityCatalog.AreiaBranca), City, 0, T0));
            Assert.AreEqual(MarketRefusal.InvalidSpot, spots.Reserve(InCity("p1"), City, MarketRules.HubStallSpots, T0));

            Assert.AreEqual(MarketRefusal.None, spots.Reserve(InCity("p1"), City, 0, T0));
            Assert.AreEqual(MarketRefusal.SpotTaken, spots.Reserve(InCity("p2"), City, 0, T0 + 10));
            Assert.AreEqual(MarketRules.HubStallSpots - 1, spots.FreeSpots(City, T0));

            // One spot per character: reserving another drops the old hold.
            Assert.AreEqual(MarketRefusal.None, spots.Reserve(InCity("p1"), City, 1, T0 + 20));
            Assert.IsNull(spots.HolderOf(City, 0, T0 + 20));

            // A hold that runs out frees the spot.
            Assert.AreEqual(MarketRefusal.None, spots.Reserve(InCity("p2"), City, 1, T0 + 20 + MarketRules.SpotHoldSeconds));

            // An occupied spot stays taken until released.
            Assert.AreEqual(MarketRefusal.None, spots.Reserve(InCity("p3"), City, 2, T0));
            Assert.AreEqual(MarketRefusal.None, spots.Occupy(City, 2, "p3", "stall-3", T0));
            Assert.AreEqual("p3", spots.HolderOf(City, 2, T0 + 100_000));
            Assert.AreEqual(MarketRefusal.AlreadyHasStall, spots.Reserve(InCity("p3"), City, 3, T0 + 5));
            spots.Release(City, 2, "p3");
            Assert.IsNull(spots.HolderOf(City, 2, T0 + 5));
        }

        // ------------------------------------------------------------------
        // Stall
        // ------------------------------------------------------------------

        [Test]
        public void Stall_Open_NeedsAReservedSpot_RefusesBoundItems_AndEscrowsTheStock()
        {
            var spots = new StallSpotRegistry();
            InventorySlotData[] bag = Bag((0, "mk_herb", 30, 0, 0), (1, "mk_wings", 1, 1, 0), (2, "mk_sword", 1, 2, 0));

            StallOpenResult noSpot = MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks((0, 10, 5)), Resolve, T0);
            Assert.AreEqual(MarketRefusal.SpotNotReserved, noSpot.Refusal);

            Assert.AreEqual(MarketRefusal.None, spots.Reserve(InCity("seller"), City, 0, T0));
            Assert.AreEqual(MarketRefusal.NotTradeable, MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks((1, 1, 500)), Resolve, T0).Refusal);
            Assert.AreEqual(TradeRefusal.Bound, MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks((1, 1, 500)), Resolve, T0).ItemReason);
            Assert.AreEqual(MarketRefusal.InvalidPrice, MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks((0, 10, 0)), Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.AlreadyListed, MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks((0, 5, 5), (0, 5, 5)), Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.NotEnoughItems, MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks((0, 31, 5)), Resolve, T0).Refusal);
            var tooMany = Enumerable.Range(0, MarketRules.StallMaxLines + 1).Select(i => (i, 1, 1)).ToArray();
            Assert.AreEqual(MarketRefusal.TooManyLines, MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks(tooMany), Resolve, T0).Refusal);

            StallOpenResult open = MarketStall.TryOpen("s1", InCity("seller"), 0, spots, Side(bag), Picks((0, 10, 5), (2, 1, 300)), Resolve, T0);
            Assert.IsTrue(open.Success, open.Refusal.ToString());
            Assert.AreEqual(20, Count(open.OwnerBag, "mk_herb"), "the stock left the bag");
            Assert.AreEqual(0, Count(open.OwnerBag, "mk_sword"));
            Assert.AreEqual(30, Count(bag, "mk_herb"), "the input bag is never touched");
            Assert.AreEqual(11, open.Stall.UnitsLeft);
            Assert.AreEqual(T0 + MarketRules.StallMaxOpenSeconds, open.Stall.ClosesAtUnix);
            Assert.AreEqual("seller", spots.HolderOf(City, 0, T0 + 10_000));
        }

        [Test]
        public void Stall_Buy_IsAtomic_PaysTheTax_AndChecksPriceCityAndDuplicates()
        {
            MarketStall stall = OpenStall(out _, (0, "mk_herb", 30, 0, 0), (0, 10, 100));
            InventorySlotData[] buyerBag = Bag((0, TradeRules.GoldItemId, 5000, 0, 0));

            Assert.AreEqual(MarketRefusal.OwnStall, stall.Buy(InCity("seller"), Side(buyerBag), 0, 1, 100, "r0", Veteran, default, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.WrongCity, stall.Buy(InCity("buyer", CityCatalog.AreiaBranca), Side(buyerBag), 0, 1, 100, "r0", Veteran, default, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.PriceChanged, stall.Buy(InCity("buyer"), Side(buyerBag), 0, 1, 90, "r0", Veteran, default, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.NotEnoughStock, stall.Buy(InCity("buyer"), Side(buyerBag), 0, 11, 100, "r0", Veteran, default, Resolve, T0).Refusal);

            StallBuyResult buy = stall.Buy(InCity("buyer"), Side(buyerBag), 0, 4, 100, "r1", Veteran, default, Resolve, T0 + 60);
            Assert.IsTrue(buy.Success, buy.Refusal.ToString());
            Assert.AreEqual(400, buy.Paid);
            Assert.AreEqual(8, buy.Tax, "2% of 400");
            Assert.AreEqual(392, stall.Till);
            Assert.AreEqual(6, stall.Lines[0].Units);
            Assert.AreEqual(4600, Count(buy.BuyerBag, TradeRules.GoldItemId));
            Assert.AreEqual(4, Count(buy.BuyerBag, "mk_herb"));
            Assert.AreEqual("market=stall_sale at=" + (T0 + 60) + " ref=stall-1 actor=buyer other=seller gold=400 burned=8 item=mk_herb*4+0",
                buy.Record.ToLogLine());

            // The same request again (a resent click) does nothing.
            StallBuyResult again = stall.Buy(InCity("buyer"), Side(buy.BuyerBag), 0, 4, 100, "r1", Veteran, default, Resolve, T0 + 61);
            Assert.AreEqual(MarketRefusal.DuplicateRequest, again.Refusal);
            Assert.IsNull(again.BuyerBag);
            Assert.AreEqual(6, stall.Lines[0].Units);

            // The owner changes the price: whoever saw the old one is refused.
            Assert.AreEqual(MarketRefusal.NotOwner, stall.SetPrice("buyer", 0, 1));
            Assert.AreEqual(MarketRefusal.None, stall.SetPrice("seller", 0, 120));
            Assert.AreEqual(MarketRefusal.PriceChanged, stall.Buy(InCity("buyer"), Side(buy.BuyerBag), 0, 1, 100, "r2", Veteran, default, Resolve, T0 + 62).Refusal);

            // Not enough gold → nothing moves.
            StallBuyResult poor = stall.Buy(InCity("poor"), Side(Bag((0, TradeRules.GoldItemId, 50, 0, 0))), 0, 1, 120, "r3", Veteran, default, Resolve, T0 + 63);
            Assert.AreEqual(MarketRefusal.NotEnoughGold, poor.Refusal);
            Assert.AreEqual(6, stall.Lines[0].Units);
            Assert.AreEqual(392, stall.Till);
        }

        [Test]
        public void Stall_TwoBuyersOfTheLastUnit_OnlyTheFirstGetsIt_NothingIsDuplicated()
        {
            MarketStall stall = OpenStall(out _, (0, "mk_sword", 1, 0, 0), (0, 1, 700));
            InventorySlotData[] bagA = Bag((0, TradeRules.GoldItemId, 1000, 0, 0));
            InventorySlotData[] bagB = Bag((0, TradeRules.GoldItemId, 1000, 0, 0));

            // Both saw "1 left at 700" and clicked at the same moment: the server runs them one after the other.
            StallBuyResult first = stall.Buy(InCity("a"), Side(bagA), 0, 1, 700, "a-1", Veteran, default, Resolve, T0 + 5);
            StallBuyResult second = stall.Buy(InCity("b"), Side(bagB), 0, 1, 700, "b-1", Veteran, default, Resolve, T0 + 5);

            Assert.IsTrue(first.Success);
            Assert.AreEqual(MarketRefusal.SoldOut, second.Refusal);
            Assert.IsNull(second.BuyerBag, "the second buyer keeps the bag it had: nothing paid");
            Assert.AreEqual(1, Count(first.BuyerBag, "mk_sword"));
            Assert.AreEqual(0, stall.UnitsLeft);
            Assert.AreEqual(700 - MarketRules.StallTax(700), stall.Till, "the till grew once");

            // Units in the world: one sword, in A's bag.
            Assert.AreEqual(1, Count(first.BuyerBag, "mk_sword") + Count(bagB, "mk_sword") + stall.UnitsLeft);
        }

        [Test]
        public void Stall_BagFull_NothingMoves()
        {
            MarketStall stall = OpenStall(out _, (0, "mk_sword", 1, 0, 0), (0, 1, 10));
            InventorySlotData[] full = FullOfSwords();
            full[31] = new InventorySlotData { itemId = TradeRules.GoldItemId, quantity = 100, gridX = 7, gridY = 7 }; // a herb cell turned gold
            StallBuyResult buy = stall.Buy(InCity("buyer"), Side(full), 0, 1, 10, "r", Veteran, default, Resolve, T0);
            Assert.AreEqual(MarketRefusal.BagFull, buy.Refusal);
            Assert.AreEqual(1, stall.UnitsLeft);
            Assert.AreEqual(0, stall.Till);
        }

        [Test]
        public void Stall_StaysOpenOffline_ClosesWhenTheOwnerWalksOffOrTimeIsUp_EverythingGoesToTheMailbox()
        {
            MarketStall stall = OpenStall(out StallSpotRegistry spots, (0, "mk_herb", 30, 0, 0), (0, 10, 100));
            var post = new MarketPost();
            InventorySlotData[] buyerBag = Bag((0, TradeRules.GoldItemId, 5000, 0, 0));
            Assert.IsTrue(stall.Buy(InCity("buyer"), Side(buyerBag), 0, 3, 100, "r", Veteran, default, Resolve, T0 + 1).Success);

            Assert.AreEqual(StallCloseReason.None, stall.Tick(ownerOnline: false, ownerDistanceFromSpot: 500f, T0 + 3600, post, spots));
            Assert.AreEqual(StallCloseReason.None, stall.Tick(ownerOnline: true, ownerDistanceFromSpot: 1f, T0 + 3601, post, spots));
            Assert.AreEqual(StallCloseReason.OwnerLeft, stall.Tick(ownerOnline: true, ownerDistanceFromSpot: 10f, T0 + 3602, post, spots));
            Assert.IsFalse(stall.IsOpen);
            Assert.IsNull(spots.HolderOf(City, 0, T0 + 3602), "the spot is free again");

            IReadOnlyList<MarketMail> mail = post.MailOf("seller");
            Assert.AreEqual(2, mail.Count);
            Assert.AreEqual(7, post.UnitsInFlight("mk_herb"), "the 7 unsold herbs went back");
            Assert.AreEqual(300 - MarketRules.StallTax(300), post.GoldInFlight());
            Assert.AreEqual(0, stall.UnitsLeft);
            Assert.AreEqual(MarketRefusal.StallClosed, stall.Buy(InCity("buyer"), Side(buyerBag), 0, 1, 100, "r2", Veteran, default, Resolve, T0 + 3603).Refusal);

            // Time up, even offline.
            MarketStall other = OpenStall(out StallSpotRegistry spots2, (0, "mk_herb", 30, 0, 0), (0, 10, 100));
            Assert.AreEqual(StallCloseReason.TimeUp, other.Tick(false, 0f, T0 + MarketRules.StallMaxOpenSeconds, post, spots2));
        }

        [Test]
        public void Stall_NewAccounts_BuyerAndSellerDailyLimits()
        {
            MarketStall stall = OpenStall(out _, (0, "mk_herb", 50, 0, 0), (0, 50, 1000));
            var newbie = new TradeAccountStanding(10d, 5);
            var rich = Bag((0, TradeRules.GoldItemId, 9999, 0, 0), (1, TradeRules.GoldItemId, 9999, 1, 0), (2, TradeRules.GoldItemId, 9999, 2, 0));

            var newBuyer = new TradeSideInput { Slots = rich, Standing = newbie };
            StallBuyResult over = stall.Buy(InCity("buyer"), newBuyer, 0, 21, 1000, "r1", Veteran, default, Resolve, T0);
            Assert.AreEqual(MarketRefusal.NewAccountGoldLimit, over.Refusal);
            Assert.IsTrue(stall.Buy(InCity("buyer"), newBuyer, 0, 20, 1000, "r2", Veteran, default, Resolve, T0).Success);

            var veteranBuyer = new TradeSideInput { Slots = rich };
            var sellerTally = new TradeDailyTally { dayKey = TradeRules.DayKey(T0), goldReceived = TradeAccountProtection.NewAccountGoldPerDay - 500 };
            Assert.AreEqual(MarketRefusal.SellerAccountLimit,
                stall.Buy(InCity("vet"), veteranBuyer, 0, 1, 1000, "r3", newbie, sellerTally, Resolve, T0).Refusal);
        }

        // ------------------------------------------------------------------
        // Auction house
        // ------------------------------------------------------------------

        [Test]
        public void Auction_List_TakesThePieceAndTheDeposit_RefusesBoundBadPricesAndLimits()
        {
            var house = new AuctionHouse(new MarketPost());
            InventorySlotData[] bag = Bag((0, "mk_sword", 1, 0, 0), (1, "mk_wings", 1, 1, 0), (2, TradeRules.GoldItemId, 1000, 2, 0));

            AuctionListResult bound = house.List(InCity("seller"), Side(bag), new MarketPick(1, 1), 100, 0, AuctionDuration.Short12h, Resolve, T0);
            Assert.AreEqual(MarketRefusal.NotTradeable, bound.Refusal);
            Assert.AreEqual(TradeRefusal.Bound, bound.ItemReason);
            Assert.AreEqual(MarketRefusal.NotInCity, house.List(new TradePresence("seller", ZoneKind.Wilds), Side(bag), new MarketPick(0, 1), 100, 0, AuctionDuration.Short12h, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.BuyoutBelowStart, house.List(InCity("seller"), Side(bag), new MarketPick(0, 1), 100, 50, AuctionDuration.Short12h, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.InvalidDuration, house.List(InCity("seller"), Side(bag), new MarketPick(0, 1), 100, 0, (AuctionDuration)9, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.InvalidPrice, house.List(InCity("seller"), Side(bag), new MarketPick(0, 1), 0, 0, AuctionDuration.Short12h, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.NotEnoughGold, house.List(InCity("seller"), Side(Bag((0, "mk_sword", 1, 0, 0))), new MarketPick(0, 1), 100, 0, AuctionDuration.Short12h, Resolve, T0).Refusal);

            AuctionListResult ok = house.List(InCity("seller"), Side(bag), new MarketPick(0, 1), 2000, 5000, AuctionDuration.Medium24h, Resolve, T0);
            Assert.IsTrue(ok.Success, ok.Refusal.ToString());
            Assert.AreEqual(100, ok.Deposit, "1% of the 5000 buyout × 2 (24 h)");
            Assert.AreEqual(900, Count(ok.SellerBag, TradeRules.GoldItemId));
            Assert.AreEqual(0, Count(ok.SellerBag, "mk_sword"));
            Assert.AreEqual(T0 + 24 * 3600, ok.Listing.endsAtUnix);
            Assert.AreEqual(100, house.GoldBurned);

            // New accounts: at most 5 listings and nothing priced over the daily gold limit.
            var newbie = new TradeAccountStanding(1d, 1);
            InventorySlotData[] herbs = Bag((0, "mk_herb", 50, 0, 0), (1, TradeRules.GoldItemId, 5000, 1, 0));
            var newSide = new TradeSideInput { Slots = herbs, Standing = newbie };
            Assert.AreEqual(MarketRefusal.NewAccountPriceLimit,
                house.List(InCity("newbie"), newSide, new MarketPick(0, 1), 100, TradeAccountProtection.NewAccountGoldPerDay + 1, AuctionDuration.Short12h, Resolve, T0).Refusal);
            for (int i = 0; i < MarketRules.NewAccountMaxListings; i++)
            {
                AuctionListResult r = house.List(InCity("newbie"), newSide, new MarketPick(0, 1), 10, 0, AuctionDuration.Short12h, Resolve, T0);
                Assert.IsTrue(r.Success, r.Refusal.ToString());
                newSide = new TradeSideInput { Slots = r.SellerBag, Standing = newbie, Tally = r.SellerTally };
            }

            Assert.AreEqual(MarketRefusal.TooManyListings, house.List(InCity("newbie"), newSide, new MarketPick(0, 1), 10, 0, AuctionDuration.Short12h, Resolve, T0).Refusal);
            Assert.AreEqual(MarketRules.MaxListingsPerCharacter, AuctionHouse.ListingLimit(TradeAccountStanding.Veteran));
        }

        [Test]
        public void Auction_Bids_IncreaseByFivePercent_OutbidGetsGoldBack_DuplicateBidsRefused()
        {
            var house = new AuctionHouse(new MarketPost());
            AuctionListing listing = ListSword(house, 1000, 0, AuctionDuration.Short12h);
            InventorySlotData[] bagA = Bag((0, TradeRules.GoldItemId, 5000, 0, 0));
            InventorySlotData[] bagB = Bag((0, TradeRules.GoldItemId, 5000, 0, 0));

            Assert.AreEqual(MarketRefusal.OwnListing, house.Bid(InCity("seller"), Side(bagA), listing.listingId, 1000, "x", Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.BidTooLow, house.Bid(InCity("a"), Side(bagA), listing.listingId, 999, "a0", Resolve, T0).Refusal);
            Assert.AreEqual(MarketRefusal.ListingNotFound, house.Bid(InCity("a"), Side(bagA), "nope", 1000, "a0", Resolve, T0).Refusal);

            AuctionBidResult a1 = house.Bid(InCity("a"), Side(bagA), listing.listingId, 1000, "a1", Resolve, T0 + 10);
            Assert.IsTrue(a1.Success);
            Assert.AreEqual(4000, Count(a1.BidderBag, TradeRules.GoldItemId), "the bid is escrowed");
            bagA = a1.BidderBag;

            // Doubled click and resent request.
            Assert.AreEqual(MarketRefusal.DuplicateRequest, house.Bid(InCity("a"), Side(bagA), listing.listingId, 2000, "a1", Resolve, T0 + 11).Refusal);
            Assert.AreEqual(MarketRefusal.AlreadyHighestBidder, house.Bid(InCity("a"), Side(bagA), listing.listingId, 2000, "a2", Resolve, T0 + 11).Refusal);

            Assert.AreEqual(1050, listing.MinNextBid);
            Assert.AreEqual(MarketRefusal.BidTooLow, house.Bid(InCity("b"), Side(bagB), listing.listingId, 1049, "b1", Resolve, T0 + 12).Refusal);
            AuctionBidResult b1 = house.Bid(InCity("b"), Side(bagB), listing.listingId, 1050, "b1", Resolve, T0 + 12);
            Assert.IsTrue(b1.Success);
            Assert.AreEqual("b", listing.highBidder);

            MarketMail refund = house.Post.MailOf("a").Single();
            Assert.AreEqual(MailKind.AuctionOutbidRefund, refund.kind);
            Assert.AreEqual(1000, refund.gold);
            Assert.AreEqual(1050, house.GoldInEscrow(), "only the high bid is held");
            Assert.IsTrue(b1.Records.Any(r => r.kind == MarketRecordKind.AuctionOutbidRefund && r.actor == "a" && r.gold == 1000));

            // Gold of A + B: everything is somewhere (bags, escrow, mail).
            long total = Count(bagA, TradeRules.GoldItemId) + Count(b1.BidderBag, TradeRules.GoldItemId) + house.GoldInEscrow() + house.Post.GoldInFlight();
            Assert.AreEqual(10_000, total);
        }

        [Test]
        public void Auction_AntiSniping_ExtendsTheEnd_UpToOneHour()
        {
            var house = new AuctionHouse(new MarketPost());
            AuctionListing listing = ListSword(house, 100, 0, AuctionDuration.Short12h);
            long end = listing.endsAtUnix;

            AuctionBidResult early = house.Bid(InCity("a"), Side(Rich()), listing.listingId, 100, "a1", Resolve, end - 3600);
            Assert.IsFalse(early.Extended);
            Assert.AreEqual(end, listing.endsAtUnix);

            AuctionBidResult snipe = house.Bid(InCity("b"), Side(Rich()), listing.listingId, 200, "b1", Resolve, end - 60);
            Assert.IsTrue(snipe.Extended);
            Assert.AreEqual(end - 60 + MarketRules.AntiSnipeWindowSeconds, listing.endsAtUnix);

            // Keep sniping: the end never moves more than an hour past the original.
            string[] who = { "a", "b" };
            long bid = 300;
            for (int i = 0; i < 40; i++)
            {
                long now = listing.endsAtUnix - 30;
                AuctionBidResult r = house.Bid(InCity(who[i % 2]), Side(Rich()), listing.listingId, bid, "s" + i, Resolve, now);
                Assert.IsTrue(r.Success, r.Refusal.ToString());
                bid = listing.MinNextBid;
            }

            Assert.AreEqual(end + MarketRules.MaxExtensionSeconds, listing.endsAtUnix);
            Assert.AreEqual(MarketRules.MaxExtensionSeconds, listing.extendedSeconds);
        }

        [Test]
        public void Auction_Buyout_EndsAtOnce_WinnerGetsThePiece_SellerGetsNinetyFivePercent()
        {
            var house = new AuctionHouse(new MarketPost());
            AuctionListing listing = ListSword(house, 1000, 4000, AuctionDuration.Long48h);
            AuctionBidResult buy = house.Bid(InCity("a"), Side(Rich()), listing.listingId, 999_999, "a1", Resolve, T0 + 100);
            Assert.IsTrue(buy.Success);
            Assert.IsTrue(buy.BoughtOut);
            Assert.AreEqual(4000, buy.Paid, "pays the buyout, not the bid");
            Assert.AreEqual(AuctionState.Sold, listing.state);

            MarketMail won = house.Post.MailOf("a").Single();
            Assert.AreEqual(MailKind.AuctionWon, won.kind);
            Assert.AreEqual("mk_sword", won.item.ItemId);
            MarketMail proceeds = house.Post.MailOf("seller").Single();
            Assert.AreEqual(3800, proceeds.gold, "4000 − 5%");
            Assert.AreEqual(MarketRules.Deposit(1000, 4000, AuctionDuration.Long48h) + 200, house.GoldBurned);
            Assert.AreEqual(MarketRefusal.ListingEnded, house.Bid(InCity("b"), Side(Rich()), listing.listingId, 5000, "b1", Resolve, T0 + 101).Refusal);
            Assert.IsTrue(buy.Records.Any(r => r.kind == MarketRecordKind.AuctionSold && r.goldBurned == 200));
        }

        [Test]
        public void Auction_End_SoldToTheHighBidder_OrBackToTheSeller_CancelOnlyWithoutBids()
        {
            var house = new AuctionHouse(new MarketPost());
            AuctionListing sold = ListSword(house, 500, 0, AuctionDuration.Short12h);
            AuctionListing unsold = ListSword(house, 500, 0, AuctionDuration.Short12h);
            AuctionListing cancel = ListSword(house, 500, 0, AuctionDuration.Short12h);

            Assert.IsTrue(house.Bid(InCity("a"), Side(Rich()), sold.listingId, 800, "a1", Resolve, T0 + 60).Success);
            Assert.AreEqual(MarketRefusal.HasBids, house.Cancel("seller", sold.listingId, T0 + 61));
            Assert.AreEqual(MarketRefusal.NotOwner, house.Cancel("a", cancel.listingId, T0 + 61));
            Assert.AreEqual(MarketRefusal.None, house.Cancel("seller", cancel.listingId, T0 + 61));
            Assert.AreEqual(AuctionState.Cancelled, cancel.state);

            Assert.IsEmpty(house.Tick(sold.endsAtUnix - 1));
            List<MarketRecord> log = house.Tick(sold.endsAtUnix);
            Assert.AreEqual(2, log.Count);
            Assert.AreEqual(AuctionState.Sold, sold.state);
            Assert.AreEqual(AuctionState.Unsold, unsold.state);

            Assert.AreEqual(MailKind.AuctionWon, house.Post.MailOf("a").Single().kind);
            List<MailKind> sellerMail = house.Post.MailOf("seller").Select(m => m.kind).ToList();
            CollectionAssert.AreEquivalent(new[] { MailKind.AuctionCancelled, MailKind.AuctionUnsold, MailKind.AuctionProceeds }, sellerMail);
            Assert.AreEqual(800 - 40, house.Post.MailOf("seller").Single(m => m.kind == MailKind.AuctionProceeds).gold);

            // Deposits are never refunded: three listings, three deposits, plus the 5% of the one sale.
            Assert.AreEqual(3 * MarketRules.Deposit(500, 0, AuctionDuration.Short12h) + 40, house.GoldBurned);
            Assert.AreEqual(3, house.Post.UnitsInFlight("mk_sword"), "every sword is in exactly one mailbox");
            Assert.AreEqual(3, house.Prune(sold.endsAtUnix + 10, 0));
        }

        // ------------------------------------------------------------------
        // Mailbox
        // ------------------------------------------------------------------

        [Test]
        public void Mail_TakeIsAllOrNothing_Once_AndOnlyByTheRecipient()
        {
            var post = new MarketPost();
            MarketMail mail = post.Send("p1", MailKind.AuctionWon, "auction-1", 250, MarketItem.From(new InventorySlotData { itemId = "mk_sword", quantity = 1 }, 1), T0);

            Assert.AreEqual(MarketRefusal.NotRecipient, post.Take("p2", mail.mailId, Side(Bag()), Resolve, T0).Refusal);
            MailTakeResult full = post.Take("p1", mail.mailId, Side(FullOfSwords()), Resolve, T0);
            Assert.AreEqual(MarketRefusal.BagFull, full.Refusal);
            Assert.AreEqual(1, post.CountOf("p1"), "the mail stays");

            MailTakeResult take = post.Take("p1", mail.mailId, Side(Bag()), Resolve, T0 + 5);
            Assert.IsTrue(take.Success);
            Assert.AreEqual(1, Count(take.Bag, "mk_sword"));
            Assert.AreEqual(250, Count(take.Bag, TradeRules.GoldItemId));
            Assert.AreEqual(MarketRefusal.MailNotFound, post.Take("p1", mail.mailId, Side(Bag()), Resolve, T0 + 6).Refusal, "never twice");
            Assert.AreEqual("market=mail_taken at=" + (T0 + 5) + " ref=" + mail.mailId + " actor=p1 other=- gold=250 burned=0 item=mk_sword*1+0",
                take.Record.ToLogLine());
        }

        [Test]
        public void Mail_ExpiresAfterThirtyDays_AndReturnsToTheOwnersChest()
        {
            var post = new MarketPost();
            MarketMail mail = post.Send("p1", MailKind.StallReturn, "stall-1", 1200, MarketItem.From(new InventorySlotData { itemId = "mk_herb", quantity = 7 }, 7), T0);
            Assert.AreEqual(T0 + 30L * 86400L, mail.expiresAtUnix);

            Assert.IsEmpty(post.Sweep(mail.expiresAtUnix - 1));
            Assert.AreEqual(MarketRefusal.MailExpired, post.Take("p1", mail.mailId, Side(Bag()), Resolve, mail.expiresAtUnix).Refusal);

            List<MailExpiry> expired = post.Sweep(mail.expiresAtUnix);
            Assert.AreEqual(1, expired.Count);
            Assert.AreEqual(0, post.CountOf("p1"));
            Assert.AreEqual(7, Count(expired[0].ChestEntries, "mk_herb"));
            Assert.AreEqual(1200, Count(expired[0].ChestEntries, TradeRules.GoldItemId), "nothing is destroyed");
            Assert.AreEqual(MarketRecordKind.MailExpired, expired[0].Record.kind);
        }

        // ------------------------------------------------------------------
        // Whole loop, texts and the drain
        // ------------------------------------------------------------------

        [Test]
        public void FullAuction_GoldIsConserved_ExceptWhatTheMarketBurns()
        {
            var house = new AuctionHouse(new MarketPost());
            InventorySlotData[] seller = Bag((0, "mk_sword", 1, 0, 0), (1, TradeRules.GoldItemId, 1000, 1, 0));
            InventorySlotData[] a = Bag((0, TradeRules.GoldItemId, 3000, 0, 0));
            InventorySlotData[] b = Bag((0, TradeRules.GoldItemId, 3000, 0, 0));
            long before = 7000;

            AuctionListResult list = house.List(InCity("seller"), Side(seller), new MarketPick(0, 1), 500, 0, AuctionDuration.Medium24h, Resolve, T0);
            seller = list.SellerBag;
            AuctionBidResult bidA = house.Bid(InCity("a"), Side(a), list.Listing.listingId, 600, "a1", Resolve, T0 + 10);
            a = bidA.BidderBag;
            AuctionBidResult bidB = house.Bid(InCity("b"), Side(b), list.Listing.listingId, 2000, "b1", Resolve, T0 + 20);
            b = bidB.BidderBag;
            house.Tick(list.Listing.endsAtUnix);

            foreach (string who in new[] { "seller", "a", "b" })
            {
                foreach (MarketMail mail in house.Post.MailOf(who).ToList())
                {
                    InventorySlotData[] bag = who == "seller" ? seller : who == "a" ? a : b;
                    MailTakeResult take = house.Post.Take(who, mail.mailId, Side(bag), Resolve, T0 + 2 * 86400);
                    Assert.IsTrue(take.Success, take.Refusal.ToString());
                    if (who == "seller") seller = take.Bag; else if (who == "a") a = take.Bag; else b = take.Bag;
                }
            }

            long after = Count(seller, TradeRules.GoldItemId) + Count(a, TradeRules.GoldItemId) + Count(b, TradeRules.GoldItemId);
            Assert.AreEqual(before - house.GoldBurned, after);
            Assert.AreEqual(3000, Count(a, TradeRules.GoldItemId), "the outbid bidder lost nothing");
            Assert.AreEqual(1, Count(b, "mk_sword"));
            Assert.AreEqual(0, Count(seller, "mk_sword"));
            Assert.AreEqual(1000 - list.Deposit + 2000 - 100, Count(seller, TradeRules.GoldItemId));
        }

        [Test]
        public void Texts_EveryRefusalAndMailKind_HasPtAndEn()
        {
            Dictionary<string, Rapadura.Core.Localization.LocalizationEntry> table = MarketTexts.Entries.ToDictionary(e => e.Key);
            foreach (MarketRefusal refusal in System.Enum.GetValues(typeof(MarketRefusal)))
            {
                if (refusal == MarketRefusal.None)
                {
                    continue;
                }

                Assert.IsTrue(table.ContainsKey(MarketTexts.RefusalKey(refusal)), refusal.ToString());
            }

            foreach (MailKind kind in System.Enum.GetValues(typeof(MailKind)))
            {
                Assert.IsTrue(table.ContainsKey(MarketTexts.MailKey(kind)), kind.ToString());
            }

            Assert.AreEqual("ui.market.refusal.sold_out", MarketTexts.RefusalKey(MarketRefusal.SoldOut));
            Assert.AreEqual("Esgotou! Alguém foi mais rápido.", MarketTexts.Get(null, MarketRefusal.SoldOut));
            Assert.AreEqual(string.Empty, MarketTexts.Get(null, MarketRefusal.None));
        }

        [Test]
        public void GoldDrain_Estimate_IsSmallButSteady()
        {
            MarketDrainEstimate e = MarketGoldDrain.Estimate();
            Assert.AreEqual(3000d, e.IncomePerDay, 1e-6);
            Assert.AreEqual(36d, e.AuctionTaxPerDay, 1e-6);
            Assert.AreEqual(28.8d, e.DepositsPerDay, 1e-6);
            Assert.AreEqual(9.6d, e.StallTaxPerDay, 1e-6);
            Assert.AreEqual(74.4d, e.TotalPerDay, 1e-6);
            Assert.That(e.ShareOfIncome, Is.InRange(0.015d, 0.05d), "the market burns 1.5–5% of the day's income");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private static readonly TradeAccountStanding Veteran = TradeAccountStanding.Veteran;

        private ItemDefinition Resolve(string id) => id != null && _items.TryGetValue(id, out ItemDefinition item) ? item : null;

        private static TradePresence InCity(string player, string city = City) => new TradePresence(player, ZoneKind.City, city);

        private static TradeSideInput Side(InventorySlotData[] slots) => new TradeSideInput { Slots = slots };

        private static InventorySlotData[] Rich() => Bag((0, TradeRules.GoldItemId, 9999, 0, 0), (1, TradeRules.GoldItemId, 9999, 1, 0));

        private static List<MarketPick> Picks(params (int slot, int quantity, int price)[] picks) =>
            picks.Select(p => new MarketPick(p.slot, p.quantity, p.price)).ToList();

        private MarketStall OpenStall(out StallSpotRegistry spots, (int index, string id, int quantity, int x, int y) entry, (int slot, int quantity, int price) pick)
        {
            spots = new StallSpotRegistry();
            Assert.AreEqual(MarketRefusal.None, spots.Reserve(InCity("seller"), City, 0, T0));
            StallOpenResult open = MarketStall.TryOpen("stall-1", InCity("seller"), 0, spots, Side(Bag(entry)), Picks(pick), Resolve, T0);
            Assert.IsTrue(open.Success, open.Refusal.ToString());
            return open.Stall;
        }

        private AuctionListing ListSword(AuctionHouse house, long start, long buyout, AuctionDuration duration)
        {
            InventorySlotData[] bag = Bag((0, "mk_sword", 1, 0, 0), (1, TradeRules.GoldItemId, 9999, 1, 0));
            AuctionListResult r = house.List(InCity("seller"), Side(bag), new MarketPick(0, 1), start, buyout, duration, Resolve, T0);
            Assert.IsTrue(r.Success, r.Refusal.ToString());
            return r.Listing;
        }

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
                    entries.Add((index++, "mk_sword", 1, x, band * 3));
                }
            }

            for (int y = 6; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    entries.Add((index++, "mk_herb", 50, x, y));
                }
            }

            return Bag(entries.ToArray());
        }

        private static int Count(IEnumerable<InventorySlotData> slots, string id) =>
            slots.Where(s => s != null && !s.IsEmpty && s.itemId == id).Sum(s => s.quantity);

        private void Create(string id, ItemType type, ItemCategory category, int maxStack, float weight = 1f, bool bound = false)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = id;
            serialized.FindProperty("_displayName").stringValue = id;
            serialized.FindProperty("_type").enumValueIndex = (int)type;
            serialized.FindProperty("_category").enumValueIndex = (int)category;
            serialized.FindProperty("_rarity").enumValueIndex = (int)ItemRarity.Common;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.FindProperty("_weight").floatValue = weight;
            serialized.FindProperty("_isBound").boolValue = bound;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            _items[id] = item;
        }
    }
}
