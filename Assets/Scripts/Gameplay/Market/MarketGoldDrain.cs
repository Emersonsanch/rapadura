namespace Rapadura.Gameplay.Market
{
    /// <summary>The estimated gold the market burns per active player per day (Docs/Network.md §17).</summary>
    public readonly struct MarketDrainEstimate
    {
        public readonly double IncomePerDay;
        public readonly double MarketVolumePerDay;
        public readonly double AuctionTaxPerDay;
        public readonly double DepositsPerDay;
        public readonly double StallTaxPerDay;

        public MarketDrainEstimate(double income, double volume, double auctionTax, double deposits, double stallTax)
        {
            IncomePerDay = income;
            MarketVolumePerDay = volume;
            AuctionTaxPerDay = auctionTax;
            DepositsPerDay = deposits;
            StallTaxPerDay = stallTax;
        }

        public double TotalPerDay => AuctionTaxPerDay + DepositsPerDay + StallTaxPerDay;

        /// <summary>Share of the day's income the market burns.</summary>
        public double ShareOfIncome => IncomePerDay > 0 ? TotalPerDay / IncomePerDay : 0d;
    }

    /// <summary>
    /// The back-of-the-envelope gold drain of the market, written as code so the test (MarketTests) recomputes it from the
    /// real fees whenever they change. Assumptions (2026-11-23):
    /// <list type="bullet">
    /// <item>Income: <see cref="GoldPerHour"/> ≈ 1 000 gold per hour of play — what <c>tools/balance/economy.py</c> prints
    /// for every band of 1–500 (910–1 180 gold/h: kill loot sold at 50% plus quest gold) — and <see cref="HoursPerDay"/> = 3 h
    /// of play a day, so ~3 000 gold per active player per day.</item>
    /// <item><see cref="MarketShareOfIncome"/> = 40% of that is spent buying from other players (MU style: gear and
    /// jewels change hands through players more than through NPC shops); <see cref="AuctionShareOfVolume"/> = 60% of it at
    /// the auction house, the rest at stalls.</item>
    /// <item>Auction: half the listings sell (<see cref="SellThrough"/>), the typical listing is 24 h and
    /// <see cref="AverageListingPrice"/> = 300 gold.</item>
    /// </list>
    /// Result: auction tax 36 + deposits ≈ 29 + stall tax ≈ 10 ≈ <b>75 gold per player per day, ~2.5% of income</b>
    /// (≈ 75 000 a day on a 1 000-player server). Small on purpose ("taxa pequena"): the market is a steady sink on top of
    /// repairs, Barca fares, house upgrades (4 050 in all, Docs/Housing.md §10.4), guild founding (5 000) and resets
    /// (20 000+), not the main one: the NPC sinks of 101–500 already take 30–60% of the gold
    /// (<c>economy.py</c> <c>BUDGET_SINK_SHARE</c>, refine fee + repair + talent reset), so with the market on top the total
    /// stays under ~65%. Note it is the <i>volume</i> that drains, not the income: gold that only changes hands is taxed once
    /// per sale. The test keeps it within 1.5–5% of income.
    /// </summary>
    public static class MarketGoldDrain
    {
        public const double GoldPerHour = 1000d;
        public const double HoursPerDay = 3d;
        public const double MarketShareOfIncome = 0.40d;
        public const double AuctionShareOfVolume = 0.60d;
        public const double SellThrough = 0.50d;
        public const long AverageListingPrice = 300L;

        public static MarketDrainEstimate Estimate()
        {
            double income = GoldPerHour * HoursPerDay;
            double volume = income * MarketShareOfIncome;
            double auctionVolume = volume * AuctionShareOfVolume;
            double stallVolume = volume - auctionVolume;

            double auctionTax = auctionVolume * MarketRules.AuctionTaxBasisPoints / 10_000d;
            double stallTax = stallVolume * MarketRules.StallTaxBasisPoints / 10_000d;

            double listingsPerDay = auctionVolume / SellThrough / AverageListingPrice;
            double deposits = listingsPerDay * MarketRules.Deposit(AverageListingPrice, 0L, AuctionDuration.Medium24h);

            return new MarketDrainEstimate(income, volume, auctionTax, deposits, stallTax);
        }
    }
}
