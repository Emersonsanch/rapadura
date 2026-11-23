using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Trade;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Market
{
    /// <summary>
    /// The stall spots ("lugares de barraca") of every city: <see cref="MarketRules.StallSpotsFor"/> numbered spots per city
    /// (24 in the hub, 12 elsewhere; where they stand in the scene is the city's layout, not decided here). Decided
    /// 2026-11-23: first come, first served, no rent. A player standing in that city's safe zone <see cref="Reserve"/>s a
    /// free spot from the "abrir barraca" window; the spot is held for <see cref="MarketRules.SpotHoldSeconds"/> while the
    /// stall is filled, then <see cref="Occupy"/> turns the hold into the stall's spot until it closes
    /// (<see cref="Release"/>). One spot per character (reserving another spot drops the old hold; an open stall must close
    /// first). A hold that runs out frees the spot by itself. Server-held, single-threaded like the rest of the market.
    /// </summary>
    public sealed class StallSpotRegistry
    {
        private sealed class Hold
        {
            public string PlayerId;
            public long UntilUnix;
            public string StallId;
        }

        private readonly Dictionary<string, Hold> _holds = new Dictionary<string, Hold>(StringComparer.Ordinal);

        public static string Key(string cityId, int spot) => cityId + "#" + spot;

        public static bool IsValidSpot(string cityId, int spot) => spot >= 0 && spot < MarketRules.StallSpotsFor(cityId);

        public MarketRefusal Reserve(TradePresence who, string cityId, int spot, long nowUnix)
        {
            if (string.IsNullOrEmpty(who.PlayerId) || !who.Available)
            {
                return MarketRefusal.NotAvailable;
            }

            if (!SafeZoneRules.IsSafe(who.Zone))
            {
                return MarketRefusal.NotInCity;
            }

            if (!string.Equals(who.CityId, cityId, StringComparison.Ordinal))
            {
                return MarketRefusal.WrongCity;
            }

            if (!IsValidSpot(cityId, spot))
            {
                return MarketRefusal.InvalidSpot;
            }

            string key = Key(cityId, spot);
            Hold current = Live(key, nowUnix);
            if (current != null && !string.Equals(current.PlayerId, who.PlayerId, StringComparison.Ordinal))
            {
                return MarketRefusal.SpotTaken;
            }

            string mine = KeyHeldBy(who.PlayerId, nowUnix);
            if (mine != null && !string.Equals(mine, key, StringComparison.Ordinal))
            {
                if (_holds[mine].StallId != null)
                {
                    return MarketRefusal.AlreadyHasStall;
                }

                _holds.Remove(mine);
            }

            if (current != null && current.StallId != null)
            {
                return MarketRefusal.AlreadyHasStall;
            }

            _holds[key] = new Hold { PlayerId = who.PlayerId, UntilUnix = nowUnix + MarketRules.SpotHoldSeconds };
            return MarketRefusal.None;
        }

        /// <summary>The reserved spot becomes the open stall's until <see cref="Release"/>.</summary>
        public MarketRefusal Occupy(string cityId, int spot, string playerId, string stallId, long nowUnix)
        {
            Hold hold = Live(Key(cityId, spot), nowUnix);
            if (hold == null || !string.Equals(hold.PlayerId, playerId, StringComparison.Ordinal))
            {
                return MarketRefusal.SpotNotReserved;
            }

            if (hold.StallId != null)
            {
                return MarketRefusal.AlreadyHasStall;
            }

            hold.StallId = stallId;
            hold.UntilUnix = long.MaxValue;
            return MarketRefusal.None;
        }

        public void Release(string cityId, int spot, string playerId)
        {
            string key = Key(cityId, spot);
            if (_holds.TryGetValue(key, out Hold hold) && string.Equals(hold.PlayerId, playerId, StringComparison.Ordinal))
            {
                _holds.Remove(key);
            }
        }

        /// <summary>Who holds the spot now (reserved or with a stall); null when free.</summary>
        public string HolderOf(string cityId, int spot, long nowUnix) => Live(Key(cityId, spot), nowUnix)?.PlayerId;

        public int FreeSpots(string cityId, long nowUnix)
        {
            int free = 0;
            int total = MarketRules.StallSpotsFor(cityId);
            for (int i = 0; i < total; i++)
            {
                if (Live(Key(cityId, i), nowUnix) == null)
                {
                    free++;
                }
            }

            return free;
        }

        private Hold Live(string key, long nowUnix)
        {
            if (!_holds.TryGetValue(key, out Hold hold))
            {
                return null;
            }

            if (hold.StallId == null && nowUnix >= hold.UntilUnix)
            {
                _holds.Remove(key);
                return null;
            }

            return hold;
        }

        private string KeyHeldBy(string playerId, long nowUnix)
        {
            foreach (KeyValuePair<string, Hold> pair in _holds)
            {
                if (string.Equals(pair.Value.PlayerId, playerId, StringComparison.Ordinal)
                    && (pair.Value.StallId != null || nowUnix < pair.Value.UntilUnix))
                {
                    return pair.Key;
                }
            }

            return null;
        }
    }
}
