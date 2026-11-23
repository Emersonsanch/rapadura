using System;
using System.Globalization;
using Rapadura.Gameplay.Refining;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Trade
{
    /// <summary>The two sides of a <see cref="TradeSession"/>: A opened it (sent the request), B accepted.</summary>
    public enum TradeSide
    {
        A = 0,
        B = 1,
    }

    /// <summary>Where a <see cref="TradeSession"/> is.</summary>
    public enum TradeState
    {
        /// <summary>Both sides put and take items and gold; zero, one or both may have locked.</summary>
        Negotiating = 0,

        /// <summary>Both locked ("travar"): the summary is shown and each side gives the final confirmation.</summary>
        Locked = 1,

        /// <summary>Settled by <see cref="TradeSettlement.Settle"/>: the items changed hands. Final.</summary>
        Completed = 2,

        /// <summary>One side cancelled (or walked away / logged off). Final.</summary>
        Cancelled = 3,

        /// <summary>Nobody acted for <see cref="TradeRules.IdleTimeoutSeconds"/>, or the window was open for too long. Final.</summary>
        Expired = 4,
    }

    /// <summary>
    /// Why a trade action was refused. Every value but <see cref="None"/> has a PT/EN text in <see cref="TradeTexts"/>.
    /// </summary>
    public enum TradeRefusal
    {
        None = 0,

        // ---- Opening ----
        SamePlayer,
        PartnerUnavailable,
        AlreadyTrading,
        TooFar,
        InCombat,

        // ---- Session ----
        SessionClosed,
        Expired,
        AlreadySettled,
        NotLocked,
        StaleSummary,
        NotConfirmed,
        EmptyTrade,

        // ---- Putting items and gold ----
        InvalidSlot,
        EmptySlot,
        AlreadyOffered,
        InvalidQuantity,
        NotEnoughItems,
        TooManyLines,
        GoldItemUseGoldField,
        NotEnoughGold,
        GoldOverTradeLimit,

        // ---- "Pode ser trocado?" (TradeItemRules) ----
        UnknownItem,
        Bound,
        StoryItem,
        FamilyRelic,
        AchievementItem,
        MountItem,
        HouseItem,
        Currency,

        // ---- Settlement ----
        ItemNoLongerHeld,
        ItemChanged,
        GoldNoLongerHeld,
        GoldOverHeldLimit,
        BagFull,
        TooHeavy,
        NewAccountGoldLimit,
        NewAccountItemLimit,
    }

    /// <summary>A notice of the final summary (<see cref="TradeSummary"/>): not a refusal, the trade still goes on.</summary>
    public enum TradeWarning
    {
        /// <summary>The side is giving a rare piece (rare+ rarity, +7 or more, excellent or ancient).</summary>
        RareItemGiven = 0,

        /// <summary>The side gives something and receives nothing (a gift, or a scam).</summary>
        GivingForNothing = 1,

        /// <summary>The side is giving at least <see cref="TradeRules.LargeGoldWarning"/> gold.</summary>
        LargeGold = 2,
    }

    /// <summary>
    /// Where a player stands when a trade is asked for (filled by the server from its own records; offline nobody trades).
    /// </summary>
    public readonly struct TradePresence
    {
        public readonly string PlayerId;
        public readonly ZoneKind Zone;

        /// <summary>The city the player is in (<see cref="ZoneKind.City"/>); null = unknown / any.</summary>
        public readonly string CityId;

        /// <summary>Alive, online and not in a blocking state (dead, loading, in a cutscene).</summary>
        public readonly bool Available;

        /// <summary>Already in another trade window (one trade at a time).</summary>
        public readonly bool InAnotherTrade;

        /// <summary>Seconds since the player last fought (hit or was hit); negative = never.</summary>
        public readonly double SecondsSinceCombat;

        public TradePresence(string playerId, ZoneKind zone, string cityId = null, bool available = true,
            bool inAnotherTrade = false, double secondsSinceCombat = -1d)
        {
            PlayerId = playerId;
            Zone = zone;
            CityId = cityId;
            Available = available;
            InAnotherTrade = inAnotherTrade;
            SecondsSinceCombat = secondsSinceCombat;
        }
    }

    /// <summary>
    /// The numbers and the "can these two open a trade?" rule of the trade between players (TODO.md Fase 12.3, "Troca entre
    /// jogadores"; Docs/Network.md §10). Plain C# with no Unity dependency, like <see cref="SafeZoneRules"/>, so the
    /// authoritative server runs this very file. Decisions (2026-11-23):
    /// <list type="bullet">
    /// <item><b>Where:</b> both in the safe zone of the <i>same</i> city → up to <see cref="CityTradeDistance"/> m apart
    /// (across a plaza); anywhere else → face to face, up to <see cref="TradeDistance"/> m, and neither fought in the last
    /// <see cref="SafeZoneRules.PlayerCombatTagSeconds"/> s (no trading in the middle of a fight).</item>
    /// <item><b>How much:</b> up to <see cref="MaxItemLinesPerSide"/> item lines and <see cref="MaxGoldPerTrade"/> gold per
    /// side; nobody ends with more than <see cref="MaxGoldHeld"/> gold.</item>
    /// <item><b>How long:</b> the window closes after <see cref="IdleTimeoutSeconds"/> s with no action, or
    /// <see cref="MaxSessionSeconds"/> s in all.</item>
    /// </list>
    /// </summary>
    public static class TradeRules
    {
        /// <summary>Face to face outside a city (or across two different zones), in metres.</summary>
        public const float TradeDistance = 5f;

        /// <summary>Both in the safe zone of the same city, in metres (a plaza; no need to stand nose to nose).</summary>
        public const float CityTradeDistance = 40f;

        /// <summary>Item lines (one stack or one piece each) per side; a MU-style 4×3 trade grid.</summary>
        public const int MaxItemLinesPerSide = 12;

        /// <summary>Gold one side may put in one trade.</summary>
        public const int MaxGoldPerTrade = 1_000_000;

        /// <summary>Gold one hero may hold after a trade (well below <see cref="int.MaxValue"/>: no overflow, ever).</summary>
        public const long MaxGoldHeld = 2_000_000_000L;

        /// <summary>Giving this much gold or more shows <see cref="TradeWarning.LargeGold"/> in the summary.</summary>
        public const int LargeGoldWarning = 100_000;

        /// <summary>The window closes after this many seconds with nobody acting.</summary>
        public const double IdleTimeoutSeconds = 120d;

        /// <summary>The window never stays open longer than this.</summary>
        public const double MaxSessionSeconds = 600d;

        /// <summary>The gold item of the bag (shops, refines): traded through the gold field, never as an item line.</summary>
        public const string GoldItemId = RefineItems.Gold;

        /// <summary>True when the pair is close enough: same city and within <see cref="CityTradeDistance"/>, or within <see cref="TradeDistance"/>.</summary>
        public static bool InRange(TradePresence a, TradePresence b, float distance)
        {
            if (float.IsNaN(distance) || distance < 0f)
            {
                return false;
            }

            return distance <= MaxDistanceFor(a, b);
        }

        /// <summary>The distance limit for this pair (<see cref="CityTradeDistance"/> in the same city, else <see cref="TradeDistance"/>).</summary>
        public static float MaxDistanceFor(TradePresence a, TradePresence b)
        {
            return SameSafeZone(a, b) ? CityTradeDistance : TradeDistance;
        }

        /// <summary>Both in a city and, when both cities are known, the same one.</summary>
        public static bool SameSafeZone(TradePresence a, TradePresence b)
        {
            if (!SafeZoneRules.IsSafe(a.Zone) || !SafeZoneRules.IsSafe(b.Zone))
            {
                return false;
            }

            return string.IsNullOrEmpty(a.CityId) || string.IsNullOrEmpty(b.CityId)
                || string.Equals(a.CityId, b.CityId, StringComparison.Ordinal);
        }

        /// <summary>Whether <paramref name="a"/> may open a trade with <paramref name="b"/> standing <paramref name="distance"/> m away.</summary>
        public static TradeRefusal CanOpen(TradePresence a, TradePresence b, float distance)
        {
            if (string.IsNullOrEmpty(a.PlayerId) || string.IsNullOrEmpty(b.PlayerId) || !a.Available || !b.Available)
            {
                return TradeRefusal.PartnerUnavailable;
            }

            if (string.Equals(a.PlayerId, b.PlayerId, StringComparison.Ordinal))
            {
                return TradeRefusal.SamePlayer;
            }

            if (a.InAnotherTrade || b.InAnotherTrade)
            {
                return TradeRefusal.AlreadyTrading;
            }

            if (!InRange(a, b, distance))
            {
                return TradeRefusal.TooFar;
            }

            if (!SameSafeZone(a, b) && (InCombat(a) || InCombat(b)))
            {
                return TradeRefusal.InCombat;
            }

            return TradeRefusal.None;
        }

        /// <summary>Fought within <see cref="SafeZoneRules.PlayerCombatTagSeconds"/> (in a city nobody fights, so it never counts there).</summary>
        public static bool InCombat(TradePresence presence)
        {
            return !SafeZoneRules.IsSafe(presence.Zone)
                && presence.SecondsSinceCombat >= 0d
                && presence.SecondsSinceCombat < SafeZoneRules.PlayerCombatTagSeconds;
        }

        /// <summary>The UTC day ("yyyy-MM-dd") of a unix time: the key of <see cref="TradeDailyTally"/>.</summary>
        public static string DayKey(long unixSeconds)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        public static TradeSide Other(TradeSide side) => side == TradeSide.A ? TradeSide.B : TradeSide.A;
    }
}
