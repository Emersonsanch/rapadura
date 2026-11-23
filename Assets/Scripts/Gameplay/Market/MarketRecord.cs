using System;
using System.Globalization;
using System.Text;

namespace Rapadura.Gameplay.Market
{
    /// <summary>What a <see cref="MarketRecord"/> is about.</summary>
    public enum MarketRecordKind
    {
        StallOpened = 0,
        StallSale = 1,
        StallClosed = 2,
        AuctionListed = 3,
        AuctionBid = 4,
        AuctionOutbidRefund = 5,
        AuctionSold = 6,
        AuctionUnsold = 7,
        AuctionCancelled = 8,
        MailTaken = 9,
        MailExpired = 10,
    }

    /// <summary>
    /// One economy-log entry of the market, as plain serializable data (TODO.md Fase 12.2 "logs de economia"), the market's
    /// sibling of <see cref="Trade.TradeRecord"/>. <see cref="goldBurned"/> is what left the economy (tax and deposit), so
    /// summing it over a day of logs gives the real gold drain to compare with the estimate in Docs/Network.md §17.
    /// </summary>
    [Serializable]
    public sealed class MarketRecord
    {
        public MarketRecordKind kind;

        /// <summary>UTC unix seconds.</summary>
        public long atUnix;

        /// <summary>Stall id, listing id or mail id.</summary>
        public string refId;

        /// <summary>Who acted (buyer, bidder, seller, recipient).</summary>
        public string actor;

        /// <summary>The other side (seller of a stall sale, previous bidder of a refund…); may be null.</summary>
        public string counterpart;

        /// <summary>Gold that moved between players (price paid, bid, refund, net received).</summary>
        public long gold;

        /// <summary>Gold destroyed by this action (tax, deposit).</summary>
        public long goldBurned;

        public string itemId;
        public int quantity;
        public int refineLevel;
        public string fingerprint;

        public static MarketRecord Of(MarketRecordKind kind, long atUnix, string refId, string actor, string counterpart,
            long gold, long goldBurned, MarketItem item)
        {
            return new MarketRecord
            {
                kind = kind,
                atUnix = atUnix,
                refId = refId,
                actor = actor,
                counterpart = counterpart,
                gold = gold,
                goldBurned = goldBurned,
                itemId = item?.ItemId,
                quantity = item?.Quantity ?? 0,
                refineLevel = item?.RefineLevel ?? 0,
                fingerprint = item?.fingerprint,
            };
        }

        /// <summary>
        /// One stable, invariant line for an append-only log:
        /// <c>market=stall_sale at=&lt;unix&gt; ref=&lt;id&gt; actor=&lt;p&gt; other=&lt;q&gt; gold=120 burned=2 item=item_x*3+0</c>.
        /// </summary>
        public string ToLogLine()
        {
            var sb = new StringBuilder(128);
            sb.Append("market=").Append(Snake(kind.ToString()));
            sb.Append(" at=").Append(atUnix.ToString(CultureInfo.InvariantCulture));
            sb.Append(" ref=").Append(refId ?? "-");
            sb.Append(" actor=").Append(actor ?? "-");
            sb.Append(" other=").Append(counterpart ?? "-");
            sb.Append(" gold=").Append(gold.ToString(CultureInfo.InvariantCulture));
            sb.Append(" burned=").Append(goldBurned.ToString(CultureInfo.InvariantCulture));
            sb.Append(" item=");
            if (string.IsNullOrEmpty(itemId))
            {
                sb.Append('-');
            }
            else
            {
                sb.Append(itemId).Append('*').Append(quantity.ToString(CultureInfo.InvariantCulture))
                    .Append('+').Append(refineLevel.ToString(CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }

        internal static string Snake(string pascal)
        {
            var sb = new StringBuilder(pascal.Length + 8);
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        sb.Append('_');
                    }

                    sb.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
