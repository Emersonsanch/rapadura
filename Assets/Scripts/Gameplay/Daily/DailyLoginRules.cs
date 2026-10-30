using System;
using System.Collections.Generic;
using System.Globalization;

namespace Rapadura.Gameplay.Daily
{
    /// <summary>Per-save state of the "Recompensa diária em sequência" (JSON slice "DailyLogin", see <see cref="DailyLoginService"/>).</summary>
    [Serializable]
    public class DailyLoginState
    {
        /// <summary>Day of the 7-day track claimed last (1..7), 0 = never (new game, or a save from before the streak existed).</summary>
        public int LastClaimedDay;

        /// <summary>Local calendar day ("yyyy-MM-dd") of the last claim. A high-water mark: an earlier clock claims nothing.</summary>
        public string LastClaimDayKey;

        /// <summary>Every claim ever (informative, achievements later).</summary>
        public int TotalClaims;

        /// <summary>How many times day 7 was claimed: rotates the day-7 rare piece.</summary>
        public int CyclesCompleted;

        public DailyLoginState Clone() => (DailyLoginState)MemberwiseClone();
    }

    /// <summary>What today's login offers.</summary>
    public readonly struct DailyLoginOffer
    {
        /// <summary>False when today was already claimed (or the clock is behind the last claim).</summary>
        public readonly bool CanClaim;

        /// <summary>1..7: the track day today's reward comes from.</summary>
        public readonly int Day;

        /// <summary>True when at least one day was skipped since the last claim (the track stepped back one day).</summary>
        public readonly bool SteppedBack;

        public DailyLoginOffer(bool canClaim, int day, bool steppedBack)
        {
            CanClaim = canClaim;
            Day = day;
            SteppedBack = steppedBack;
        }
    }

    public enum DailyRewardKind
    {
        /// <summary>A plain stack of an item from the database.</summary>
        Item,

        /// <summary>
        /// A "Convite" for the scheduled event rooms (<see cref="DailyLoginRules.EventInviteItemId"/>); if that item does
        /// not exist in the ItemDatabase the entry's fallback (gold) is given instead and the popup shows the fallback.
        /// </summary>
        EventInvite,

        /// <summary>A small rare piece: a procedural <c>Rare</c> roll of a leather armour base (rotates every cycle).</summary>
        RareGear,
    }

    /// <summary>One line of a day's reward.</summary>
    public readonly struct DailyRewardEntry
    {
        public readonly DailyRewardKind Kind;
        public readonly string ItemId;
        public readonly int Quantity;
        public readonly string FallbackItemId;
        public readonly int FallbackQuantity;

        public DailyRewardEntry(DailyRewardKind kind, string itemId, int quantity, string fallbackItemId = null, int fallbackQuantity = 0)
        {
            Kind = kind;
            ItemId = itemId;
            Quantity = quantity;
            FallbackItemId = fallbackItemId;
            FallbackQuantity = fallbackQuantity;
        }

        public static DailyRewardEntry Item(string itemId, int quantity) => new DailyRewardEntry(DailyRewardKind.Item, itemId, quantity);
    }

    /// <summary>
    /// Pure rules of the "Recompensa diária em sequência" (TODO Fase 16, Tibia), tested in <c>DailyLoginTests</c>:
    /// <list type="bullet">
    /// <item>The first login of each new local calendar day claims one reward of a 7-day track (<see cref="RewardsFor"/>);
    /// after day 7 the track starts again at day 1.</item>
    /// <item>Missing a day never resets to zero: the track <b>steps back one day</b> from where it would continue (claimed
    /// day 4, skipped a day → day 4 again; claimed day 7 → day 1 either way). However long the absence, it is one step.</item>
    /// <item>Clock guard: the last claim's day is a high-water mark — a clock set back claims nothing until the real date
    /// passes it; a clock set forward only ever reads as "missed days" (one step back), never as extra progress.
    /// Online (Fase 12) the server owns the streak.</item>
    /// <item>Saves from before the streak (no slice) and new games start at day 1.</item>
    /// </list>
    /// </summary>
    public static class DailyLoginRules
    {
        public const int TrackLength = 7;
        public const string DayKeyFormat = "yyyy-MM-dd";

        public const string GoldItemId = "item_gold";
        public const string HealthPotionItemId = "item_health_potion";
        public const string ManaPotionItemId = "item_mana_potion";
        public const string IronItemId = "item_iron";
        public const string HerbItemId = "item_herb";

        /// <summary>
        /// The event-room invite of day 5: the "Convite do Terreiro" of the scheduled event rooms
        /// (<c>EventRoomRules.TerreiroTicketItemId</c>, Resources/Items/item_convite_terreiro.asset). Kept as a literal so
        /// the streak does not depend on the event-room code; if the item is ever missing from the ItemDatabase, day 5 pays
        /// the fallback (<see cref="EventInviteFallbackGold"/> gold) instead.
        /// </summary>
        public const string EventInviteItemId = "item_convite_terreiro";
        public const int EventInviteFallbackGold = 30;

        /// <summary>Bases of the day-7 rare piece, in rotation order (by <see cref="DailyLoginState.CyclesCompleted"/>).</summary>
        public static readonly string[] RareGearBases = { "item_leather_helmet", "item_leather_boots", "item_leather_chest" };

        private static readonly DailyRewardEntry[][] Track =
        {
            new[] { DailyRewardEntry.Item(GoldItemId, 20) },
            new[] { DailyRewardEntry.Item(HealthPotionItemId, 2) },
            new[] { DailyRewardEntry.Item(IronItemId, 3) },
            new[] { DailyRewardEntry.Item(GoldItemId, 40), DailyRewardEntry.Item(ManaPotionItemId, 2) },
            new[] { new DailyRewardEntry(DailyRewardKind.EventInvite, EventInviteItemId, 1, GoldItemId, EventInviteFallbackGold) },
            new[] { DailyRewardEntry.Item(HealthPotionItemId, 3), DailyRewardEntry.Item(HerbItemId, 5) },
            new[] { new DailyRewardEntry(DailyRewardKind.RareGear, RareGearBases[0], 1), DailyRewardEntry.Item(GoldItemId, 50) },
        };

        /// <summary>The reward lines of track day <paramref name="day"/> (1..7; out of range clamps).</summary>
        public static IReadOnlyList<DailyRewardEntry> RewardsFor(int day)
        {
            return Track[ClampDay(day) - 1];
        }

        /// <summary>The day-7 rare piece's base for the cycle about to be completed.</summary>
        public static string RareGearBaseFor(int cyclesCompleted)
        {
            int index = ((cyclesCompleted % RareGearBases.Length) + RareGearBases.Length) % RareGearBases.Length;
            return RareGearBases[index];
        }

        public static int ClampDay(int day) => Math.Max(1, Math.Min(TrackLength, day));

        /// <summary>The day after <paramref name="lastClaimedDay"/> (7 → 1; never claimed → 1).</summary>
        public static int ContinuingDay(int lastClaimedDay)
        {
            return lastClaimedDay < 1 || lastClaimedDay >= TrackLength ? 1 : lastClaimedDay + 1;
        }

        /// <summary>One step back from <see cref="ContinuingDay"/>, never below day 1.</summary>
        public static int SteppedBackDay(int lastClaimedDay)
        {
            return Math.Max(1, ContinuingDay(lastClaimedDay) - 1);
        }

        public static DailyLoginOffer Evaluate(DailyLoginState state, string todayKey)
        {
            if (!TryParseDay(todayKey, out DateTime today))
            {
                return new DailyLoginOffer(false, 1, false);
            }

            if (state == null || state.LastClaimedDay < 1 || !TryParseDay(state.LastClaimDayKey, out DateTime last))
            {
                return new DailyLoginOffer(true, 1, false);
            }

            int days = (int)Math.Round((today - last).TotalDays);
            if (days <= 0)
            {
                // Already claimed today, or the clock is behind the last claim (high-water mark).
                return new DailyLoginOffer(false, ClampDay(state.LastClaimedDay), false);
            }

            return days == 1
                ? new DailyLoginOffer(true, ContinuingDay(state.LastClaimedDay), false)
                : new DailyLoginOffer(true, SteppedBackDay(state.LastClaimedDay), true);
        }

        /// <summary>Records a claim of <paramref name="offer"/> on <paramref name="todayKey"/>. No-op (false) when it cannot be claimed.</summary>
        public static bool Claim(DailyLoginState state, DailyLoginOffer offer, string todayKey)
        {
            if (state == null || !offer.CanClaim || !TryParseDay(todayKey, out _))
            {
                return false;
            }

            state.LastClaimedDay = ClampDay(offer.Day);
            state.LastClaimDayKey = todayKey;
            state.TotalClaims++;
            if (state.LastClaimedDay == TrackLength)
            {
                state.CyclesCompleted++;
            }

            return true;
        }

        public static string ToDayKey(DateTime localTime) => localTime.Date.ToString(DayKeyFormat, CultureInfo.InvariantCulture);

        public static bool TryParseDay(string dayKey, out DateTime day)
        {
            return DateTime.TryParseExact(dayKey, DayKeyFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out day);
        }
    }
}
