using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Trade;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Market
{
    public enum AuctionState
    {
        Active = 0,
        Sold = 1,
        Unsold = 2,
        Cancelled = 3,
    }

    /// <summary>One auction listing. The piece and the high bid are both held by the house (escrow) until it ends.</summary>
    [Serializable]
    public sealed class AuctionListing
    {
        public string listingId;
        public string sellerId;
        public MarketItem item;
        public long startBid;

        /// <summary>0 = no buyout.</summary>
        public long buyout;

        public AuctionDuration duration;
        public long createdAtUnix;
        public long endsAtUnix;

        /// <summary>Seconds the anti-sniping rule added so far (at most <see cref="MarketRules.MaxExtensionSeconds"/>).</summary>
        public long extendedSeconds;

        public long deposit;
        public string highBidder;
        public long highBid;
        public int bidCount;
        public AuctionState state;

        public bool HasBid => !string.IsNullOrEmpty(highBidder);
        public bool HasBuyout => buyout > 0;
        public long MinNextBid => MarketRules.MinNextBid(startBid, highBid, HasBid);
    }

    public sealed class AuctionListResult
    {
        public bool Success;
        public MarketRefusal Refusal;
        public TradeRefusal ItemReason;
        public AuctionListing Listing;

        /// <summary>The seller's bag without the piece and the deposit; null on failure.</summary>
        public InventorySlotData[] SellerBag;

        public TradeDailyTally SellerTally;
        public long Deposit;
        public MarketRecord Record;
    }

    public sealed class AuctionBidResult
    {
        public bool Success;
        public MarketRefusal Refusal;

        /// <summary>The bidder's bag with the bid taken out (held by the house); null on failure.</summary>
        public InventorySlotData[] BidderBag;

        public TradeDailyTally BidderTally;

        /// <summary>Gold escrowed: the bid, or the buyout when the bid reached it.</summary>
        public long Paid;

        /// <summary>The bid reached the buyout: the listing ended sold at once.</summary>
        public bool BoughtOut;

        /// <summary>The bid fell in the last minutes and pushed the end.</summary>
        public bool Extended;

        public List<MarketRecord> Records = new List<MarketRecord>();
    }

    /// <summary>
    /// The auction house (TODO.md Fase 12.3 "casa de leilões com taxa (dreno de ouro)"; Docs/Network.md §17): any city's
    /// auctioneer, one house for the whole world. Pure and server-held; every call runs on the server's single market
    /// thread, so "two bids at the same time" are two calls in a row.
    /// <list type="bullet">
    /// <item><b>List</b> (<see cref="List"/>): one bag entry (tradeable, <see cref="TradeItemRules"/>: bound items refused),
    /// start bid 1..<see cref="MarketRules.MaxUnitPrice"/>, optional buyout ≥ start, 12/24/48 h. The piece and the
    /// <see cref="MarketRules.Deposit"/> leave the bag; the deposit is burned, sold or not.</item>
    /// <item><b>Bid</b> (<see cref="Bid"/>): at least <see cref="AuctionListing.MinNextBid"/> (start, then +5%); the gold is
    /// escrowed and whoever was outbid gets theirs back by mail at once. The high bidder cannot bid again (a doubled
    /// click is <see cref="MarketRefusal.AlreadyHighestBidder"/>, a resent request <see cref="MarketRefusal.DuplicateRequest"/>);
    /// the seller cannot bid. A bid at or above the buyout pays the buyout and ends it. A bid in the last 5 min pushes
    /// the end to 5 min after it (at most +1 h in all).</item>
    /// <item><b>End</b> (<see cref="Tick"/>): sold → the piece to the winner's mailbox, the price minus 5% to the seller's;
    /// unsold → the piece back to the seller's mailbox. <see cref="Cancel"/> only before the first bid.</item>
    /// </list>
    /// </summary>
    public sealed class AuctionHouse
    {
        private readonly Dictionary<string, AuctionListing> _listings = new Dictionary<string, AuctionListing>(StringComparer.Ordinal);
        private readonly HashSet<string> _requests = new HashSet<string>(StringComparer.Ordinal);
        private long _nextId = 1;

        public AuctionHouse(MarketPost post)
        {
            Post = post ?? new MarketPost();
        }

        public MarketPost Post { get; }

        /// <summary>Deposits and taxes burned so far.</summary>
        public long GoldBurned { get; private set; }

        public IEnumerable<AuctionListing> Listings => _listings.Values;

        public AuctionListing Get(string listingId) =>
            listingId != null && _listings.TryGetValue(listingId, out AuctionListing listing) ? listing : null;

        public int ActiveListingsOf(string sellerId)
        {
            int count = 0;
            foreach (AuctionListing listing in _listings.Values)
            {
                if (listing.state == AuctionState.Active && string.Equals(listing.sellerId, sellerId, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        public static int ListingLimit(TradeAccountStanding standing) =>
            TradeAccountProtection.IsNewAccount(standing) ? MarketRules.NewAccountMaxListings : MarketRules.MaxListingsPerCharacter;

        /// <summary>Gold escrowed in active high bids (the auditor's "gold held by the house").</summary>
        public long GoldInEscrow()
        {
            long total = 0;
            foreach (AuctionListing listing in _listings.Values)
            {
                if (listing.state == AuctionState.Active)
                {
                    total += listing.highBid;
                }
            }

            return total;
        }

        public AuctionListResult List(TradePresence seller, TradeSideInput bag, MarketPick pick, long startBid, long buyout,
            AuctionDuration duration, Func<string, ItemDefinition> resolve, long nowUnix)
        {
            var result = new AuctionListResult { SellerTally = bag != null ? bag.Tally : default };
            if (string.IsNullOrEmpty(seller.PlayerId) || !seller.Available || bag == null)
            {
                return Fail(result, MarketRefusal.NotAvailable);
            }

            if (!SafeZoneRules.IsSafe(seller.Zone))
            {
                return Fail(result, MarketRefusal.NotInCity);
            }

            if (!MarketRules.IsValidDuration(duration))
            {
                return Fail(result, MarketRefusal.InvalidDuration);
            }

            if (startBid < 1 || startBid > MarketRules.MaxUnitPrice || buyout < 0 || buyout > MarketRules.MaxUnitPrice)
            {
                return Fail(result, MarketRefusal.InvalidPrice);
            }

            if (buyout > 0 && buyout < startBid)
            {
                return Fail(result, MarketRefusal.BuyoutBelowStart);
            }

            if (ActiveListingsOf(seller.PlayerId) >= ListingLimit(bag.Standing))
            {
                return Fail(result, MarketRefusal.TooManyListings);
            }

            string day = TradeRules.DayKey(nowUnix);
            TradeDailyTally today = bag.Tally.ForDay(day);
            if (TradeAccountProtection.IsNewAccount(bag.Standing))
            {
                if (Math.Max(startBid, buyout) > TradeAccountProtection.NewAccountGoldPerDay)
                {
                    return Fail(result, MarketRefusal.NewAccountPriceLimit);
                }

                TradeRefusal limit = TradeAccountProtection.Check(bag.Standing, today, 0L, 0L, 0, 1);
                if (limit != TradeRefusal.None)
                {
                    return Fail(result, MarketRules.FromTrade(limit));
                }
            }

            long deposit = MarketRules.Deposit(startBid, buyout, duration);
            MarketRefusal take = MarketBag.TryTake(bag, new[] { pick }, deposit, resolve, out InventorySlotData[] after,
                out List<MarketItem> taken, out TradeRefusal reason);
            if (take != MarketRefusal.None)
            {
                result.ItemReason = reason;
                return Fail(result, take);
            }

            var listing = new AuctionListing
            {
                listingId = "auction-" + _nextId++,
                sellerId = seller.PlayerId,
                item = taken[0],
                startBid = startBid,
                buyout = buyout,
                duration = duration,
                createdAtUnix = nowUnix,
                endsAtUnix = nowUnix + MarketRules.DurationSeconds(duration),
                deposit = deposit,
                state = AuctionState.Active,
            };
            _listings[listing.listingId] = listing;
            GoldBurned += deposit;

            result.Success = true;
            result.Listing = listing;
            result.SellerBag = after;
            result.Deposit = deposit;
            result.SellerTally = today.With(day, 0L, 0L, 0, 1);
            result.Record = MarketRecord.Of(MarketRecordKind.AuctionListed, nowUnix, listing.listingId, seller.PlayerId, null, 0L, deposit, listing.item);
            return result;
        }

        public AuctionBidResult Bid(TradePresence bidder, TradeSideInput bag, string listingId, long amount, string requestId,
            Func<string, ItemDefinition> resolve, long nowUnix)
        {
            var result = new AuctionBidResult { BidderTally = bag != null ? bag.Tally : default };
            if (string.IsNullOrEmpty(bidder.PlayerId) || !bidder.Available || bag == null)
            {
                return Fail(result, MarketRefusal.NotAvailable);
            }

            if (!SafeZoneRules.IsSafe(bidder.Zone))
            {
                return Fail(result, MarketRefusal.NotInCity);
            }

            if (!string.IsNullOrEmpty(requestId) && _requests.Contains(requestId))
            {
                return Fail(result, MarketRefusal.DuplicateRequest);
            }

            AuctionListing listing = Get(listingId);
            if (listing == null)
            {
                return Fail(result, MarketRefusal.ListingNotFound);
            }

            if (listing.state != AuctionState.Active || nowUnix >= listing.endsAtUnix)
            {
                return Fail(result, MarketRefusal.ListingEnded);
            }

            if (string.Equals(listing.sellerId, bidder.PlayerId, StringComparison.Ordinal))
            {
                return Fail(result, MarketRefusal.OwnListing);
            }

            if (string.Equals(listing.highBidder, bidder.PlayerId, StringComparison.Ordinal))
            {
                return Fail(result, MarketRefusal.AlreadyHighestBidder);
            }

            if (amount > MarketRules.MaxUnitPrice || amount < 1)
            {
                return Fail(result, MarketRefusal.InvalidPrice);
            }

            bool buyout = listing.HasBuyout && amount >= listing.buyout;
            long pay = buyout ? listing.buyout : amount;
            if (!buyout && pay < listing.MinNextBid)
            {
                return Fail(result, MarketRefusal.BidTooLow);
            }

            string day = TradeRules.DayKey(nowUnix);
            TradeDailyTally today = bag.Tally.ForDay(day);
            TradeRefusal limit = TradeAccountProtection.Check(bag.Standing, today, 0L, pay, 1, 0);
            if (limit != TradeRefusal.None)
            {
                return Fail(result, MarketRules.FromTrade(limit));
            }

            MarketRefusal escrow = MarketBag.TryExchange(bag, pay, 0L, null, resolve, out InventorySlotData[] after);
            if (escrow != MarketRefusal.None)
            {
                return Fail(result, escrow);
            }

            // Commit.
            if (!string.IsNullOrEmpty(requestId))
            {
                _requests.Add(requestId);
            }

            if (listing.HasBid)
            {
                Post.Send(listing.highBidder, MailKind.AuctionOutbidRefund, listing.listingId, listing.highBid, null, nowUnix);
                result.Records.Add(MarketRecord.Of(MarketRecordKind.AuctionOutbidRefund, nowUnix, listing.listingId,
                    listing.highBidder, bidder.PlayerId, listing.highBid, 0L, null));
            }

            listing.highBidder = bidder.PlayerId;
            listing.highBid = pay;
            listing.bidCount++;
            result.Records.Add(MarketRecord.Of(MarketRecordKind.AuctionBid, nowUnix, listing.listingId, bidder.PlayerId,
                listing.sellerId, pay, 0L, listing.item));

            result.Success = true;
            result.BidderBag = after;
            result.BidderTally = today.With(day, 0L, pay, 1, 0);
            result.Paid = pay;

            if (buyout)
            {
                result.BoughtOut = true;
                End(listing, nowUnix, result.Records);
                return result;
            }

            long left = listing.endsAtUnix - nowUnix;
            if (left < MarketRules.AntiSnipeWindowSeconds && listing.extendedSeconds < MarketRules.MaxExtensionSeconds)
            {
                long push = Math.Min(MarketRules.AntiSnipeWindowSeconds - left, MarketRules.MaxExtensionSeconds - listing.extendedSeconds);
                listing.endsAtUnix += push;
                listing.extendedSeconds += push;
                result.Extended = push > 0;
            }

            return result;
        }

        /// <summary>The seller takes back a listing nobody bid on (the piece by mail; the deposit stays burned).</summary>
        public MarketRefusal Cancel(string sellerId, string listingId, long nowUnix, List<MarketRecord> log = null)
        {
            AuctionListing listing = Get(listingId);
            if (listing == null)
            {
                return MarketRefusal.ListingNotFound;
            }

            if (!string.Equals(listing.sellerId, sellerId, StringComparison.Ordinal))
            {
                return MarketRefusal.NotOwner;
            }

            if (listing.state != AuctionState.Active)
            {
                return MarketRefusal.ListingEnded;
            }

            if (listing.HasBid)
            {
                return MarketRefusal.HasBids;
            }

            listing.state = AuctionState.Cancelled;
            Post.Send(sellerId, MailKind.AuctionCancelled, listingId, 0L, listing.item, nowUnix);
            log?.Add(MarketRecord.Of(MarketRecordKind.AuctionCancelled, nowUnix, listingId, sellerId, null, 0L, 0L, listing.item));
            return MarketRefusal.None;
        }

        /// <summary>Ends every listing whose time is up (the server calls it every few seconds). Returns the records.</summary>
        public List<MarketRecord> Tick(long nowUnix)
        {
            var log = new List<MarketRecord>();
            foreach (AuctionListing listing in _listings.Values)
            {
                if (listing.state == AuctionState.Active && nowUnix >= listing.endsAtUnix)
                {
                    End(listing, nowUnix, log);
                }
            }

            return log;
        }

        /// <summary>Drops ended listings older than <paramref name="keepSeconds"/> (their mail already went out).</summary>
        public int Prune(long nowUnix, long keepSeconds)
        {
            var old = new List<string>();
            foreach (AuctionListing listing in _listings.Values)
            {
                if (listing.state != AuctionState.Active && nowUnix - listing.endsAtUnix > keepSeconds)
                {
                    old.Add(listing.listingId);
                }
            }

            foreach (string id in old)
            {
                _listings.Remove(id);
            }

            return old.Count;
        }

        private void End(AuctionListing listing, long nowUnix, List<MarketRecord> log)
        {
            if (listing.HasBid)
            {
                long tax = MarketRules.AuctionTax(listing.highBid);
                long net = listing.highBid - tax;
                listing.state = AuctionState.Sold;
                listing.endsAtUnix = Math.Min(listing.endsAtUnix, nowUnix);
                GoldBurned += tax;
                Post.Send(listing.highBidder, MailKind.AuctionWon, listing.listingId, 0L, listing.item, nowUnix);
                Post.Send(listing.sellerId, MailKind.AuctionProceeds, listing.listingId, net, null, nowUnix);
                log.Add(MarketRecord.Of(MarketRecordKind.AuctionSold, nowUnix, listing.listingId, listing.highBidder,
                    listing.sellerId, listing.highBid, tax, listing.item));
            }
            else
            {
                listing.state = AuctionState.Unsold;
                Post.Send(listing.sellerId, MailKind.AuctionUnsold, listing.listingId, 0L, listing.item, nowUnix);
                log.Add(MarketRecord.Of(MarketRecordKind.AuctionUnsold, nowUnix, listing.listingId, listing.sellerId, null, 0L, 0L, listing.item));
            }
        }

        private static AuctionListResult Fail(AuctionListResult result, MarketRefusal refusal)
        {
            result.Success = false;
            result.Refusal = refusal;
            return result;
        }

        private static AuctionBidResult Fail(AuctionBidResult result, MarketRefusal refusal)
        {
            result.Success = false;
            result.Refusal = refusal;
            result.BidderBag = null;
            return result;
        }
    }
}
