using System;
using System.Collections.Generic;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>What an event item is (Docs/Network.md §14). Never renumber.</summary>
    public enum TimedEventItemKind
    {
        /// <summary>Burnt at the gate instead of the gold fee (Vela da Vigília, Marca do Caçador).</summary>
        Ticket = 0,

        /// <summary>Event material: trades and crafts, never stats on its own.</summary>
        Material = 1,

        /// <summary>The Caçada's chest (one per winner).</summary>
        Chest = 2,

        /// <summary>A trophy for the house, bound, no stats (Marca de Ohyah).</summary>
        Trophy = 3,

        /// <summary>Title, aura or look: bound, no stats.</summary>
        Cosmetic = 4,
    }

    /// <summary>How a ticket is earned (all in-game, never real money).</summary>
    public readonly struct TimedTicketSource
    {
        public readonly string TicketId;

        /// <summary>Rare drop: chance per kill of a common beast (no boss) at or above <see cref="DropMinLevel"/>.</summary>
        public readonly double CommonDropChance;

        /// <summary>Rare drop: chance per kill of a region boss (<c>RegionBossRules</c>) at or above <see cref="DropMinLevel"/>.</summary>
        public readonly double BossDropChance;

        public readonly int DropMinLevel;

        /// <summary>The exchange: <see cref="ExchangeCount"/> of <see cref="ExchangeMaterialId"/> for one ticket, at <see cref="ExchangeCityId"/>.</summary>
        public readonly string ExchangeMaterialId;
        public readonly int ExchangeCount;

        /// <summary><c>CityCatalog</c> id of the city whose shop makes the exchange.</summary>
        public readonly string ExchangeCityId;

        public TimedTicketSource(string ticketId, double commonDropChance, double bossDropChance, int dropMinLevel,
            string exchangeMaterialId, int exchangeCount, string exchangeCityId)
        {
            TicketId = ticketId;
            CommonDropChance = commonDropChance;
            BossDropChance = bossDropChance;
            DropMinLevel = dropMinLevel;
            ExchangeMaterialId = exchangeMaterialId;
            ExchangeCount = exchangeCount;
            ExchangeCityId = exchangeCityId;
        }
    }

    /// <summary>
    /// Every item id the scheduled events hand out or burn, with its kind and, for the tickets, where it is earned
    /// (decided 2026-11-23, Docs/Network.md §14). Each id has an <c>ItemDefinition</c> in <c>Resources/Items</c> (EditMode-tested).
    /// <list type="bullet">
    /// <item><b>Vela da Vigília</b>: rare drop (0,4%) of any common beast of level 15+ (the 1st Vigília room), or 5 Cinzas da
    /// Vigília at the Velário of the Vila das Lamparinas (the city of lamps). Never sold for gold: one Vela opens any room,
    /// so a gold price would undercut the room fee (2.000 … 120.000).</item>
    /// <item><b>Marca do Caçador</b>: rare drop (5%) of a region boss of level 100+ (the 1st strength of Ohyah), or 5
    /// Fragmentos de Ohyah at the Fortim dos Gigantes-da-Colina. Same reason: no gold price.</item>
    /// </list>
    /// No pay-to-win: nothing here is sold for real money, tickets and materials trade between players for in-game gold only,
    /// and every cosmetic / trophy is bound and has no stats.
    /// </summary>
    public static class TimedEventItems
    {
        public const string VigilExchangeCityId = CityCatalog.Lamparinas;
        public const string HuntExchangeCityId = CityCatalog.Fortim;

        public const double VigilCandleCommonDropChance = 0.004d;
        public const double HuntMarkBossDropChance = 0.05d;
        public const int ExchangeCount = 5;

        public static readonly TimedTicketSource VigilCandle = new TimedTicketSource(
            TimedEventCatalog.VigiliaTicketId, VigilCandleCommonDropChance, 0d, TimedEventCatalog.SevenRooms[0].MinLevel,
            TimedEventRewards.VigiliaMaterial, ExchangeCount, VigilExchangeCityId);

        public static readonly TimedTicketSource HuntMark = new TimedTicketSource(
            TimedEventCatalog.CacadaTicketId, 0d, HuntMarkBossDropChance, TimedEventCatalog.OhyahRooms[0].MinLevel,
            TimedEventRewards.CacadaMaterial, ExchangeCount, HuntExchangeCityId);

        public static readonly IReadOnlyList<TimedTicketSource> TicketSources = new[] { VigilCandle, HuntMark };

        /// <summary>Every id, in a fixed order: tickets, materials, chest, trophy, then the cosmetics (Bema last).</summary>
        public static IReadOnlyList<string> AllIds => _allIds ?? (_allIds = BuildAll());

        private static List<string> _allIds;

        private static List<string> BuildAll()
        {
            var ids = new List<string>
            {
                TimedEventCatalog.VigiliaTicketId,
                TimedEventCatalog.CacadaTicketId,
                TimedEventRewards.VigiliaMaterial,
                TimedEventRewards.CercoMaterial,
                TimedEventRewards.CacadaMaterial,
                TimedEventRewards.OhyahChest,
                TimedEventRewards.OhyahMark,
            };

            foreach (TimedEventKind kind in new[] { TimedEventKind.VigiliaDaColuna, TimedEventKind.CercoDaTreva, TimedEventKind.CacadaAOhyah })
            {
                if (TimedEventRewards.Cosmetics.TryGetValue(kind, out string[] list))
                {
                    foreach (string id in list)
                    {
                        if (!ids.Contains(id)) ids.Add(id);
                    }
                }
            }

            if (!ids.Contains(TimedBemaRules.BemaCosmeticId)) ids.Add(TimedBemaRules.BemaCosmeticId);
            return ids;
        }

        public static TimedEventItemKind KindOf(string id)
        {
            if (id == TimedEventCatalog.VigiliaTicketId || id == TimedEventCatalog.CacadaTicketId) return TimedEventItemKind.Ticket;
            if (id == TimedEventRewards.OhyahChest) return TimedEventItemKind.Chest;
            if (id == TimedEventRewards.OhyahMark) return TimedEventItemKind.Trophy;
            if (TimedEventRewards.IsCosmetic(id)) return TimedEventItemKind.Cosmetic;
            return TimedEventItemKind.Material;
        }

        /// <summary>Cosmetics and trophies are bound to the hero (never traded, so never a real-money market).</summary>
        public static bool IsBound(string id)
        {
            TimedEventItemKind kind = KindOf(id);
            return kind == TimedEventItemKind.Cosmetic || kind == TimedEventItemKind.Trophy;
        }

        public static bool TryGetTicketSource(string ticketId, out TimedTicketSource source)
        {
            foreach (TimedTicketSource s in TicketSources)
            {
                if (string.Equals(s.TicketId, ticketId, StringComparison.Ordinal))
                {
                    source = s;
                    return true;
                }
            }

            source = default;
            return false;
        }

        /// <summary>Tickets one exchange can make from <paramref name="materialCount"/> of the ticket's material (0 for no exchange).</summary>
        public static int TicketsFromExchange(string ticketId, int materialCount)
        {
            return TryGetTicketSource(ticketId, out TimedTicketSource s) && s.ExchangeCount > 0 && materialCount > 0
                ? materialCount / s.ExchangeCount
                : 0;
        }

        /// <summary>Drop chance of a ticket from one kill (the server rolls it with its own seed).</summary>
        public static double TicketDropChance(string ticketId, int enemyLevel, bool isBoss)
        {
            if (!TryGetTicketSource(ticketId, out TimedTicketSource s) || enemyLevel < s.DropMinLevel)
            {
                return 0d;
            }

            return isBoss ? s.BossDropChance : s.CommonDropChance;
        }
    }
}
