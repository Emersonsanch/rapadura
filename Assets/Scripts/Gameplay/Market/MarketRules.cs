using System;
using Rapadura.Gameplay.Trade;

namespace Rapadura.Gameplay.Market
{
    /// <summary>
    /// Why a market action (stall, auction house, mailbox) was refused. Every value but <see cref="None"/> has a PT/EN text
    /// in <see cref="MarketTexts"/>. When the item itself cannot change hands, the result is <see cref="NotTradeable"/> and
    /// the exact reason (bound, story item, relic…) comes from <see cref="TradeItemRules"/> in the result's <c>ItemReason</c>.
    /// </summary>
    public enum MarketRefusal
    {
        None = 0,

        // ---- Where / who ----
        NotInCity,
        WrongCity,
        NotAvailable,
        InvalidSpot,
        SpotTaken,
        SpotNotReserved,
        AlreadyHasStall,
        OwnStall,
        StallClosed,
        NotOwner,

        // ---- Lines and prices ----
        NoLines,
        TooManyLines,
        InvalidSlot,
        EmptySlot,
        AlreadyListed,
        InvalidQuantity,
        NotEnoughItems,
        InvalidPrice,
        NotTradeable,
        ItemChanged,

        // ---- Buying ----
        SoldOut,
        NotEnoughStock,
        PriceChanged,
        NotEnoughGold,
        GoldOverTradeLimit,
        GoldOverHeldLimit,
        BagFull,
        TooHeavy,
        DuplicateRequest,

        // ---- Auction ----
        InvalidDuration,
        BuyoutBelowStart,
        TooManyListings,
        ListingNotFound,
        ListingEnded,
        OwnListing,
        BidTooLow,
        AlreadyHighestBidder,
        HasBids,

        // ---- New accounts (TradeAccountProtection) ----
        NewAccountGoldLimit,
        NewAccountItemLimit,
        SellerAccountLimit,
        NewAccountPriceLimit,

        // ---- Mailbox ----
        MailNotFound,
        NotRecipient,
        MailExpired,
    }

    /// <summary>Auction durations offered at the auctioneer (WoW-like 12 / 24 / 48 h).</summary>
    public enum AuctionDuration
    {
        Short12h = 0,
        Medium24h = 1,
        Long48h = 2,
    }

    /// <summary>
    /// The numbers of the player market (TODO.md Fase 12.3, "Barraca de venda" and "casa de leilões com taxa"; Docs/Network.md
    /// §17). Plain C#, server-runnable. All times are UTC unix seconds (a stall stays open while its owner is offline, an
    /// auction lasts days, so the market clock is the wall clock, not the session clock of <see cref="TradeSession"/>).
    /// Gold amounts are integers; every fee is rounded <b>down</b> (tiny sales go untaxed, nobody pays a fraction).
    /// Decisions of 2026-11-23:
    /// <list type="bullet">
    /// <item><b>Stall (barraca):</b> only on a stall spot inside a city's safe zone; <see cref="HubStallSpots"/> spots in the
    /// hub (<see cref="HubCityId"/>), <see cref="CityStallSpots"/> in every other city. Up to <see cref="StallMaxLines"/>
    /// lines (a MU-style 4×4 window), each with its own unit price. Open while the owner stands on the spot or is offline,
    /// at most <see cref="StallMaxOpenSeconds"/> (12 h); <see cref="StallTaxBasisPoints"/> (2%) of each sale is burned.</item>
    /// <item><b>Auction house:</b> start bid, optional buyout, 12/24/48 h. A non-refundable deposit of
    /// <see cref="DepositBasisPointsPer12h"/> (1%) of the larger of start and buyout per 12 h (min
    /// <see cref="MinDeposit"/>, max <see cref="MaxDeposit"/>). Next bid ≥ current + 5% (min +1). A bid in the last
    /// <see cref="AntiSnipeWindowSeconds"/> (5 min) pushes the end to 5 min from then, up to <see cref="MaxExtensionSeconds"/>
    /// (1 h) in all. <see cref="AuctionTaxBasisPoints"/> (5%) of the final price is burned. Up to
    /// <see cref="MaxListingsPerCharacter"/> listings per character (<see cref="NewAccountMaxListings"/> for new accounts).</item>
    /// <item><b>Mailbox:</b> mails expire after <see cref="MailExpiryDays"/> days and go back to their owner through the
    /// chest's overflow inbox (<c>VillageChestSaveState.overflowInbox</c>): nothing is ever destroyed.</item>
    /// </list>
    /// </summary>
    public static class MarketRules
    {
        // ---- Stall ----

        /// <summary>The shared hub city (more players, more spots).</summary>
        public const string HubCityId = "vila_zefa";

        public const int HubStallSpots = 24;
        public const int CityStallSpots = 12;

        /// <summary>A spot reserved from the "abrir barraca" window is held this long while the owner fills it.</summary>
        public const long SpotHoldSeconds = 90L;

        public const int StallMaxLines = 16;

        /// <summary>A stall closes by itself after this long (12 h), online or offline.</summary>
        public const long StallMaxOpenSeconds = 12L * 3600L;

        /// <summary>The owner, when online, must stay within this distance of the spot (metres) or the stall closes.</summary>
        public const float StallOwnerLeaveDistance = 3f;

        /// <summary>2% of each stall sale.</summary>
        public const int StallTaxBasisPoints = 200;

        /// <summary>Highest unit price of a stall line or auction bid; one purchase never moves more than <see cref="TradeRules.MaxGoldPerTrade"/>.</summary>
        public const int MaxUnitPrice = TradeRules.MaxGoldPerTrade;

        // ---- Auction ----

        public const int DepositBasisPointsPer12h = 100;
        public const long MinDeposit = 5L;
        public const long MaxDeposit = 50_000L;

        /// <summary>5% of the final price (WoW and the MU-like servers use 5%).</summary>
        public const int AuctionTaxBasisPoints = 500;

        /// <summary>Each new bid is at least this much over the current one (5%), never less than +1.</summary>
        public const int MinBidIncrementBasisPoints = 500;

        public const long AntiSnipeWindowSeconds = 5L * 60L;
        public const long MaxExtensionSeconds = 60L * 60L;

        public const int MaxListingsPerCharacter = 20;
        public const int NewAccountMaxListings = 5;

        // ---- Mailbox ----

        public const int MailExpiryDays = 30;
        public const long MailExpirySeconds = MailExpiryDays * 86400L;

        /// <summary>Stall spots of <paramref name="cityId"/>; 0 when it is not a city id.</summary>
        public static int StallSpotsFor(string cityId)
        {
            if (string.IsNullOrEmpty(cityId))
            {
                return 0;
            }

            if (string.Equals(cityId, HubCityId, StringComparison.Ordinal))
            {
                return HubStallSpots;
            }

            return World.Cities.CityCatalog.Get(cityId) != null ? CityStallSpots : 0;
        }

        /// <summary><paramref name="amount"/> × <paramref name="basisPoints"/> / 10 000, rounded down (never negative).</summary>
        public static long Fee(long amount, int basisPoints)
        {
            if (amount <= 0 || basisPoints <= 0)
            {
                return 0L;
            }

            return amount / 10_000L * basisPoints + amount % 10_000L * basisPoints / 10_000L;
        }

        public static long StallTax(long saleTotal) => Fee(saleTotal, StallTaxBasisPoints);

        public static long AuctionTax(long finalPrice) => Fee(finalPrice, AuctionTaxBasisPoints);

        public static long DurationSeconds(AuctionDuration duration)
        {
            switch (duration)
            {
                case AuctionDuration.Short12h: return 12L * 3600L;
                case AuctionDuration.Medium24h: return 24L * 3600L;
                case AuctionDuration.Long48h: return 48L * 3600L;
                default: return 0L;
            }
        }

        public static bool IsValidDuration(AuctionDuration duration) => DurationSeconds(duration) > 0L;

        /// <summary>
        /// Non-refundable listing deposit: 1% of max(start, buyout) per 12 h of duration (12 h ×1, 24 h ×2, 48 h ×4 —
        /// longer listings cost more), clamped to [<see cref="MinDeposit"/>, <see cref="MaxDeposit"/>]. The larger price is
        /// used so a 1-gold start with a huge buyout does not dodge the deposit.
        /// </summary>
        public static long Deposit(long startBid, long buyout, AuctionDuration duration)
        {
            long reference = Math.Max(Math.Max(0L, startBid), Math.Max(0L, buyout));
            int units = duration == AuctionDuration.Long48h ? 4 : duration == AuctionDuration.Medium24h ? 2 : 1;
            long raw = Fee(reference, DepositBasisPointsPer12h) * units;
            return Math.Min(MaxDeposit, Math.Max(MinDeposit, raw));
        }

        /// <summary>The least the next bid may be: the start bid when nobody bid yet, else current + max(1, ⌈5%⌉).</summary>
        public static long MinNextBid(long startBid, long currentBid, bool hasBid)
        {
            if (!hasBid)
            {
                return Math.Max(1L, startBid);
            }

            long step = (currentBid * MinBidIncrementBasisPoints + 9_999L) / 10_000L;
            return currentBid + Math.Max(1L, step);
        }

        /// <summary>Maps the item-level refusals of the trade code to the market ones.</summary>
        public static MarketRefusal FromTrade(TradeRefusal refusal)
        {
            switch (refusal)
            {
                case TradeRefusal.None: return MarketRefusal.None;
                case TradeRefusal.InvalidSlot: return MarketRefusal.InvalidSlot;
                case TradeRefusal.EmptySlot: return MarketRefusal.EmptySlot;
                case TradeRefusal.ItemChanged: return MarketRefusal.ItemChanged;
                case TradeRefusal.ItemNoLongerHeld:
                case TradeRefusal.NotEnoughItems: return MarketRefusal.NotEnoughItems;
                case TradeRefusal.GoldNoLongerHeld:
                case TradeRefusal.NotEnoughGold: return MarketRefusal.NotEnoughGold;
                case TradeRefusal.GoldOverHeldLimit: return MarketRefusal.GoldOverHeldLimit;
                case TradeRefusal.BagFull: return MarketRefusal.BagFull;
                case TradeRefusal.TooHeavy: return MarketRefusal.TooHeavy;
                case TradeRefusal.NewAccountGoldLimit: return MarketRefusal.NewAccountGoldLimit;
                case TradeRefusal.NewAccountItemLimit: return MarketRefusal.NewAccountItemLimit;
                default: return MarketRefusal.NotTradeable;
            }
        }
    }
}
