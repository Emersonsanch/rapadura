using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Trade;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Market
{
    /// <summary>Why a <see cref="MarketStall"/> closed.</summary>
    public enum StallCloseReason
    {
        None = 0,
        OwnerClosed = 1,

        /// <summary>The owner, online, walked off the spot (or fought, or left the city).</summary>
        OwnerLeft = 2,

        /// <summary><see cref="MarketRules.StallMaxOpenSeconds"/> passed.</summary>
        TimeUp = 3,

        /// <summary>Every line sold out.</summary>
        SoldOut = 4,
    }

    /// <summary>One line of a stall: its stock (out of the owner's bag) and its unit price.</summary>
    [Serializable]
    public sealed class StallLine
    {
        public MarketItem stock;
        public int unitPrice;

        /// <summary>Bumped on every sale and price change: what a buyer's window shows can be told stale.</summary>
        public int revision;

        public int Units => stock != null ? stock.Quantity : 0;
    }

    /// <summary>What <see cref="MarketStall.TryOpen"/> did.</summary>
    public sealed class StallOpenResult
    {
        public bool Success;
        public MarketRefusal Refusal;
        public TradeRefusal ItemReason;
        public MarketStall Stall;

        /// <summary>The owner's bag with the stock taken out; null on failure.</summary>
        public InventorySlotData[] OwnerBag;

        public MarketRecord Record;
    }

    /// <summary>What <see cref="MarketStall.Buy"/> did. On failure nothing moved.</summary>
    public sealed class StallBuyResult
    {
        public bool Success;
        public MarketRefusal Refusal;
        public InventorySlotData[] BuyerBag;
        public TradeDailyTally BuyerTally;
        public TradeDailyTally SellerTally;
        public long Paid;
        public long Tax;
        public MarketItem Bought;
        public MarketRecord Record;
    }

    /// <summary>
    /// A player's stall ("barraca de venda", TODO.md Fase 12.3; Docs/Network.md §17), MU personal-store style: the owner
    /// puts up to <see cref="MarketRules.StallMaxLines"/> bag entries for sale, each with a unit price, and the stock
    /// <b>leaves the bag</b> (escrow) so it cannot be sold, traded or dropped twice. It stays open while the owner stands
    /// on the spot or is offline, at most <see cref="MarketRules.StallMaxOpenSeconds"/>.
    /// <para><b>Buying is atomic</b> (<see cref="Buy"/>): the buyer's bag pays and receives on a copy, and only when that
    /// fits does the stock go down and the till go up. The server calls the stall from one thread, so two buyers of the
    /// last unit are two calls in a row: the first buys it, the second gets <see cref="MarketRefusal.SoldOut"/> and pays
    /// nothing. The buyer quotes the price it saw (a price changed after the window opened is
    /// <see cref="MarketRefusal.PriceChanged"/>) and a request id (a resent click is
    /// <see cref="MarketRefusal.DuplicateRequest"/>).</para>
    /// <para>2% of each sale (<see cref="MarketRules.StallTax"/>) is burned; the rest goes to the till, which reaches the owner
    /// by mail when the stall closes (with any unsold stock), so an offline owner never misses a sale.</para>
    /// </summary>
    public sealed class MarketStall
    {
        private readonly List<StallLine> _lines = new List<StallLine>();
        private readonly HashSet<string> _requests = new HashSet<string>(StringComparer.Ordinal);

        public string StallId { get; private set; }
        public string OwnerId { get; private set; }
        public string CityId { get; private set; }
        public int Spot { get; private set; }
        public long OpenedAtUnix { get; private set; }
        public long ClosesAtUnix { get; private set; }

        /// <summary>Net gold of the sales so far (after tax), paid to the owner by mail on close.</summary>
        public long Till { get; private set; }

        /// <summary>Gold burned by the stall's tax so far.</summary>
        public long TaxPaid { get; private set; }

        public StallCloseReason CloseReason { get; private set; }
        public bool IsOpen => CloseReason == StallCloseReason.None;
        public IReadOnlyList<StallLine> Lines => _lines;

        public int UnitsLeft
        {
            get
            {
                int units = 0;
                foreach (StallLine line in _lines)
                {
                    units += line.Units;
                }

                return units;
            }
        }

        private MarketStall()
        {
        }

        /// <summary>
        /// Opens a stall on <paramref name="spot"/> of the owner's city (reserved first with
        /// <see cref="StallSpotRegistry.Reserve"/>): 1 to <see cref="MarketRules.StallMaxLines"/> picks, each with a unit
        /// price of 1 to <see cref="MarketRules.MaxUnitPrice"/>, every entry tradeable (<see cref="TradeItemRules"/>).
        /// </summary>
        public static StallOpenResult TryOpen(string stallId, TradePresence owner, int spot, StallSpotRegistry spots,
            TradeSideInput ownerBag, IReadOnlyList<MarketPick> picks, Func<string, ItemDefinition> resolve, long nowUnix)
        {
            var result = new StallOpenResult();
            if (string.IsNullOrEmpty(stallId) || spots == null || string.IsNullOrEmpty(owner.PlayerId) || !owner.Available)
            {
                return Fail(result, MarketRefusal.NotAvailable);
            }

            if (!SafeZoneRules.IsSafe(owner.Zone) || string.IsNullOrEmpty(owner.CityId))
            {
                return Fail(result, MarketRefusal.NotInCity);
            }

            if (!StallSpotRegistry.IsValidSpot(owner.CityId, spot))
            {
                return Fail(result, MarketRefusal.InvalidSpot);
            }

            if (!string.Equals(spots.HolderOf(owner.CityId, spot, nowUnix), owner.PlayerId, StringComparison.Ordinal))
            {
                return Fail(result, MarketRefusal.SpotNotReserved);
            }

            if (picks == null || picks.Count == 0)
            {
                return Fail(result, MarketRefusal.NoLines);
            }

            if (picks.Count > MarketRules.StallMaxLines)
            {
                return Fail(result, MarketRefusal.TooManyLines);
            }

            foreach (MarketPick pick in picks)
            {
                if (pick.UnitPrice < 1 || pick.UnitPrice > MarketRules.MaxUnitPrice)
                {
                    return Fail(result, MarketRefusal.InvalidPrice);
                }
            }

            MarketRefusal take = MarketBag.TryTake(ownerBag, picks, 0L, resolve, out InventorySlotData[] after, out List<MarketItem> taken, out TradeRefusal reason);
            if (take != MarketRefusal.None)
            {
                result.ItemReason = reason;
                return Fail(result, take);
            }

            MarketRefusal occupy = spots.Occupy(owner.CityId, spot, owner.PlayerId, stallId, nowUnix);
            if (occupy != MarketRefusal.None)
            {
                return Fail(result, occupy);
            }

            var stall = new MarketStall
            {
                StallId = stallId,
                OwnerId = owner.PlayerId,
                CityId = owner.CityId,
                Spot = spot,
                OpenedAtUnix = nowUnix,
                ClosesAtUnix = nowUnix + MarketRules.StallMaxOpenSeconds,
            };

            for (int i = 0; i < taken.Count; i++)
            {
                stall._lines.Add(new StallLine { stock = taken[i], unitPrice = picks[i].UnitPrice });
            }

            result.Success = true;
            result.Stall = stall;
            result.OwnerBag = after;
            result.Record = MarketRecord.Of(MarketRecordKind.StallOpened, nowUnix, stallId, owner.PlayerId, null, 0L, 0L, null);
            return result;
        }

        /// <summary>
        /// Buys <paramref name="quantity"/> units of line <paramref name="lineIndex"/> at <paramref name="expectedUnitPrice"/>.
        /// <paramref name="seller"/> is the owner's standing and daily tally as the server keeps them (a new-account owner may
        /// not receive more than <see cref="TradeAccountProtection.NewAccountGoldPerDay"/> a day through the stall either).
        /// </summary>
        public StallBuyResult Buy(TradePresence buyer, TradeSideInput buyerBag, int lineIndex, int quantity, int expectedUnitPrice,
            string requestId, TradeAccountStanding sellerStanding, TradeDailyTally sellerTally, Func<string, ItemDefinition> resolve, long nowUnix)
        {
            string day = TradeRules.DayKey(nowUnix);
            var result = new StallBuyResult
            {
                BuyerTally = buyerBag != null ? buyerBag.Tally : default,
                SellerTally = sellerTally,
            };

            if (!IsOpen || nowUnix >= ClosesAtUnix)
            {
                return Fail(result, MarketRefusal.StallClosed);
            }

            if (string.IsNullOrEmpty(buyer.PlayerId) || !buyer.Available || buyerBag == null)
            {
                return Fail(result, MarketRefusal.NotAvailable);
            }

            if (string.Equals(buyer.PlayerId, OwnerId, StringComparison.Ordinal))
            {
                return Fail(result, MarketRefusal.OwnStall);
            }

            if (!SafeZoneRules.IsSafe(buyer.Zone))
            {
                return Fail(result, MarketRefusal.NotInCity);
            }

            if (!string.Equals(buyer.CityId, CityId, StringComparison.Ordinal))
            {
                return Fail(result, MarketRefusal.WrongCity);
            }

            if (!string.IsNullOrEmpty(requestId) && _requests.Contains(requestId))
            {
                return Fail(result, MarketRefusal.DuplicateRequest);
            }

            if (lineIndex < 0 || lineIndex >= _lines.Count)
            {
                return Fail(result, MarketRefusal.InvalidSlot);
            }

            StallLine line = _lines[lineIndex];
            if (quantity <= 0)
            {
                return Fail(result, MarketRefusal.InvalidQuantity);
            }

            if (line.Units <= 0)
            {
                return Fail(result, MarketRefusal.SoldOut);
            }

            if (quantity > line.Units)
            {
                return Fail(result, MarketRefusal.NotEnoughStock);
            }

            if (expectedUnitPrice != line.unitPrice)
            {
                return Fail(result, MarketRefusal.PriceChanged);
            }

            long total = (long)line.unitPrice * quantity;
            if (total > TradeRules.MaxGoldPerTrade)
            {
                return Fail(result, MarketRefusal.GoldOverTradeLimit);
            }

            long tax = MarketRules.StallTax(total);
            long net = total - tax;

            TradeDailyTally buyerToday = buyerBag.Tally.ForDay(day);
            TradeRefusal buyerLimit = TradeAccountProtection.Check(buyerBag.Standing, buyerToday, 0L, total, 1, 0);
            if (buyerLimit != TradeRefusal.None)
            {
                return Fail(result, MarketRules.FromTrade(buyerLimit));
            }

            TradeDailyTally sellerToday = sellerTally.ForDay(day);
            if (TradeAccountProtection.Check(sellerStanding, sellerToday, net, 0L, 0, 1) != TradeRefusal.None)
            {
                return Fail(result, MarketRefusal.SellerAccountLimit);
            }

            if (Till + net > TradeRules.MaxGoldHeld)
            {
                return Fail(result, MarketRefusal.GoldOverHeldLimit);
            }

            MarketItem bought = line.stock.Split(quantity);
            MarketRefusal exchange = MarketBag.TryExchange(buyerBag, total, 0L, new[] { bought }, resolve, out InventorySlotData[] after);
            if (exchange != MarketRefusal.None)
            {
                return Fail(result, exchange);
            }

            // Commit: nothing below can fail.
            line.stock.snapshot.quantity -= quantity;
            line.revision++;
            Till += net;
            TaxPaid += tax;
            if (!string.IsNullOrEmpty(requestId))
            {
                _requests.Add(requestId);
            }

            result.Success = true;
            result.Refusal = MarketRefusal.None;
            result.BuyerBag = after;
            result.BuyerTally = buyerToday.With(day, 0L, total, 1, 0);
            result.SellerTally = sellerToday.With(day, net, 0L, 0, 1);
            result.Paid = total;
            result.Tax = tax;
            result.Bought = bought;
            result.Record = MarketRecord.Of(MarketRecordKind.StallSale, nowUnix, StallId, buyer.PlayerId, OwnerId, total, tax, bought);
            return result;
        }

        /// <summary>The owner changes a line's price (bumps its revision: buyers who saw the old price are refused).</summary>
        public MarketRefusal SetPrice(string playerId, int lineIndex, int unitPrice)
        {
            if (!IsOpen)
            {
                return MarketRefusal.StallClosed;
            }

            if (!string.Equals(playerId, OwnerId, StringComparison.Ordinal))
            {
                return MarketRefusal.NotOwner;
            }

            if (lineIndex < 0 || lineIndex >= _lines.Count)
            {
                return MarketRefusal.InvalidSlot;
            }

            if (unitPrice < 1 || unitPrice > MarketRules.MaxUnitPrice)
            {
                return MarketRefusal.InvalidPrice;
            }

            if (_lines[lineIndex].unitPrice != unitPrice)
            {
                _lines[lineIndex].unitPrice = unitPrice;
                _lines[lineIndex].revision++;
            }

            return MarketRefusal.None;
        }

        /// <summary>
        /// The server's periodic check: closes the stall when the time is up, everything sold, or the owner is online and
        /// more than <see cref="MarketRules.StallOwnerLeaveDistance"/> from the spot (an offline owner keeps it open).
        /// Returns the reason it closed now, or <see cref="StallCloseReason.None"/>.
        /// </summary>
        public StallCloseReason Tick(bool ownerOnline, float ownerDistanceFromSpot, long nowUnix, MarketPost post,
            StallSpotRegistry spots, List<MarketRecord> log = null)
        {
            if (!IsOpen)
            {
                return StallCloseReason.None;
            }

            StallCloseReason reason = StallCloseReason.None;
            if (nowUnix >= ClosesAtUnix)
            {
                reason = StallCloseReason.TimeUp;
            }
            else if (UnitsLeft <= 0)
            {
                reason = StallCloseReason.SoldOut;
            }
            else if (ownerOnline && (float.IsNaN(ownerDistanceFromSpot) || ownerDistanceFromSpot > MarketRules.StallOwnerLeaveDistance))
            {
                reason = StallCloseReason.OwnerLeft;
            }

            if (reason != StallCloseReason.None)
            {
                Close(reason, nowUnix, post, spots, log);
            }

            return reason;
        }

        /// <summary>The owner closes the stall.</summary>
        public MarketRefusal CloseByOwner(string playerId, long nowUnix, MarketPost post, StallSpotRegistry spots, List<MarketRecord> log = null)
        {
            if (!IsOpen)
            {
                return MarketRefusal.StallClosed;
            }

            if (!string.Equals(playerId, OwnerId, StringComparison.Ordinal))
            {
                return MarketRefusal.NotOwner;
            }

            Close(StallCloseReason.OwnerClosed, nowUnix, post, spots, log);
            return MarketRefusal.None;
        }

        /// <summary>Unsold stock (one mail per line) and the till go to the owner's mailbox; the spot is freed.</summary>
        private void Close(StallCloseReason reason, long nowUnix, MarketPost post, StallSpotRegistry spots, List<MarketRecord> log)
        {
            CloseReason = reason;
            foreach (StallLine line in _lines)
            {
                if (line.Units > 0)
                {
                    post?.Send(OwnerId, MailKind.StallReturn, StallId, 0L, line.stock.Split(line.Units), nowUnix);
                    line.stock.snapshot.quantity = 0;
                }
            }

            long till = Till;
            if (till > 0)
            {
                post?.Send(OwnerId, MailKind.StallProceeds, StallId, till, null, nowUnix);
                Till = 0;
            }

            spots?.Release(CityId, Spot, OwnerId);
            log?.Add(MarketRecord.Of(MarketRecordKind.StallClosed, nowUnix, StallId, OwnerId, null, till, 0L, null));
        }

        private static StallOpenResult Fail(StallOpenResult result, MarketRefusal refusal)
        {
            result.Success = false;
            result.Refusal = refusal;
            return result;
        }

        private static StallBuyResult Fail(StallBuyResult result, MarketRefusal refusal)
        {
            result.Success = false;
            result.Refusal = refusal;
            result.BuyerBag = null;
            return result;
        }
    }
}
