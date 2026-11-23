using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Trade
{
    /// <summary>One item line of a <see cref="TradeRecord"/>.</summary>
    [Serializable]
    public sealed class TradeRecordLine
    {
        public string itemId;
        public int quantity;
        public ItemRarity rarity;
        public int refineLevel;

        /// <summary><see cref="TradeItemRules.Fingerprint"/> of the entry that changed hands (rolled data, options).</summary>
        public string fingerprint;

        public static TradeRecordLine From(TradeOfferLine line)
        {
            return new TradeRecordLine
            {
                itemId = line.ItemId,
                quantity = line.Quantity,
                rarity = line.Rarity,
                refineLevel = line.Snapshot != null ? line.Snapshot.refineLevel : 0,
                fingerprint = line.Fingerprint,
            };
        }
    }

    /// <summary>
    /// What a completed trade did, as plain serializable data for the server's economy log (TODO.md Fase 12.2 "logs de
    /// economia", 12.3 "log de trocas"): who (<see cref="playerA"/>, <see cref="playerB"/>), what (items and gold each
    /// way), when (<see cref="completedAtUnix"/>) and the trade id. Built only by <see cref="TradeSettlement.Settle"/>.
    /// </summary>
    [Serializable]
    public sealed class TradeRecord
    {
        public string tradeId;
        public string playerA;
        public string playerB;

        /// <summary>UTC unix seconds of the settlement.</summary>
        public long completedAtUnix;

        /// <summary>Server clock (seconds) when the window opened and when it settled.</summary>
        public double openedAt;
        public double completedAt;

        /// <summary>The revision both sides confirmed.</summary>
        public int revision;

        public int goldAToB;
        public int goldBToA;
        public TradeRecordLine[] itemsAToB = Array.Empty<TradeRecordLine>();
        public TradeRecordLine[] itemsBToA = Array.Empty<TradeRecordLine>();

        public static TradeRecord From(TradeSession session, long completedAtUnix, double now)
        {
            return new TradeRecord
            {
                tradeId = session.TradeId,
                playerA = session.PlayerA,
                playerB = session.PlayerB,
                completedAtUnix = completedAtUnix,
                openedAt = session.OpenedAt,
                completedAt = now,
                revision = session.Revision,
                goldAToB = session.OfferOf(TradeSide.A).Gold,
                goldBToA = session.OfferOf(TradeSide.B).Gold,
                itemsAToB = Lines(session.OfferOf(TradeSide.A)),
                itemsBToA = Lines(session.OfferOf(TradeSide.B)),
            };
        }

        /// <summary>
        /// One line of text for an append-only log, stable and invariant:
        /// <c>trade=&lt;id&gt; at=&lt;unix&gt; a=&lt;A&gt; b=&lt;B&gt; a&gt;b=[gold:10;item_x*2+0] b&gt;a=[...]</c>.
        /// </summary>
        public string ToLogLine()
        {
            var sb = new StringBuilder(128);
            sb.Append("trade=").Append(tradeId);
            sb.Append(" at=").Append(completedAtUnix.ToString(CultureInfo.InvariantCulture));
            sb.Append(" a=").Append(playerA).Append(" b=").Append(playerB);
            sb.Append(" a>b=");
            AppendSide(sb, goldAToB, itemsAToB);
            sb.Append(" b>a=");
            AppendSide(sb, goldBToA, itemsBToA);
            return sb.ToString();
        }

        private static void AppendSide(StringBuilder sb, int gold, TradeRecordLine[] lines)
        {
            sb.Append("[gold:").Append(gold.ToString(CultureInfo.InvariantCulture));
            if (lines != null)
            {
                foreach (TradeRecordLine line in lines)
                {
                    sb.Append(';').Append(line.itemId).Append('*').Append(line.quantity.ToString(CultureInfo.InvariantCulture))
                        .Append('+').Append(line.refineLevel.ToString(CultureInfo.InvariantCulture));
                }
            }

            sb.Append(']');
        }

        private static TradeRecordLine[] Lines(TradeOffer offer)
        {
            var lines = new List<TradeRecordLine>(offer.Lines.Count);
            foreach (TradeOfferLine line in offer.Lines)
            {
                lines.Add(TradeRecordLine.From(line));
            }

            return lines.ToArray();
        }
    }
}
