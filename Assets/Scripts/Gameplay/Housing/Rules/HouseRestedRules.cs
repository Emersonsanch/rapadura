using System;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>The "Descansado" buff in the save (Housing.md §12.2 <c>restCharges</c>, <c>restFightsLeft</c>, <c>lastRestDay</c>).</summary>
    [Serializable]
    public class HouseRestedState
    {
        /// <summary>Nights with a bonus left today (<see cref="HouseRestedRules.ChargesPerDay"/> per calendar day; unused ones do not carry over).</summary>
        public int charges;
        /// <summary>The calendar day (yyyymmdd, <see cref="HouseRestedRules.DateKey"/>) <see cref="charges"/> belong to; 0 = never.</summary>
        public int chargeDate;
        /// <summary>Fights the bonus still lasts.</summary>
        public int fightsLeft;
        /// <summary>Kill-XP bonus in percent while <see cref="fightsLeft"/> &gt; 0.</summary>
        public int percent;
        /// <summary>Comfort tier of the night it was earned (for the HUD/house text).</summary>
        public int tier;

        public HouseRestedState Clone() => (HouseRestedState)MemberwiseClone();
    }

    /// <summary>
    /// "Descansado" (Housing.md §6.1): sleeping in a bed of one's own house gives a small kill-XP bonus for a number of
    /// fights, by the comfort tier. It recharges <see cref="ChargesPerDay"/> times per calendar day (the dailies'
    /// <c>IDailyClock</c>) and unused nights do not pile up; a new night replaces the current bonus (never stacks).
    /// XP only: no damage, no loot. Applied at the one kill-XP choke point (<c>EnemyRewardGranter</c>) as one more
    /// multiplier in the chain (stamina, NG+…), so they compose. Unity-free.
    /// </summary>
    public static class HouseRestedRules
    {
        public const int ChargesPerDay = 2;
        public const int TierCount = 5;

        /// <summary>Lower comfort bound of each tier: 0–19, 20–59, 60–149, 150–299, 300+.</summary>
        public static readonly int[] TierThresholds = { 0, 20, 60, 150, 300 };
        /// <summary>
        /// Kill-XP bonus per tier. The doc proposed 2/3/5/7/10 %; tiers 2–4 are capped at +3 % so a hero who sleeps at
        /// home before every region stays inside the r9 power-stack budget (Docs/Balance/README.md, "Casa: Descansado"):
        /// at +4–5 % over both campaigns the NG+1 Desert commons fall a level ahead. Higher tiers last longer instead.
        /// </summary>
        public static readonly int[] TierPercent = { 2, 2, 3, 3, 3 };
        public static readonly int[] TierFights = { 10, 15, 20, 25, 30 };

        public static int TierOf(int comfort)
        {
            int tier = 0;
            for (int i = 0; i < TierThresholds.Length; i++)
            {
                if (comfort >= TierThresholds[i]) tier = i;
            }

            return tier;
        }

        public static int ClampTier(int tier) => Math.Max(0, Math.Min(TierCount - 1, tier));

        public static int PercentOf(int tier) => TierPercent[ClampTier(tier)];

        public static int FightsOf(int tier) => TierFights[ClampTier(tier)];

        /// <summary>Comfort still missing for the next tier (0 at the top tier).</summary>
        public static int ComfortToNextTier(int comfort)
        {
            int tier = TierOf(comfort);
            return tier >= TierCount - 1 ? 0 : TierThresholds[tier + 1] - comfort;
        }

        public static int DateKey(DateTime date) => date.Year * 10000 + date.Month * 100 + date.Day;

        /// <summary>A new calendar day refills the nights to <see cref="ChargesPerDay"/> (the old ones are not kept).</summary>
        public static void RefreshCharges(HouseRestedState state, int dateKey)
        {
            if (state == null) return;
            if (state.chargeDate != dateKey)
            {
                state.chargeDate = dateKey;
                state.charges = ChargesPerDay;
            }

            state.charges = Math.Max(0, Math.Min(ChargesPerDay, state.charges));
        }

        public static int ChargesLeft(HouseRestedState state, int dateKey)
        {
            if (state == null) return 0;
            return state.chargeDate != dateKey ? ChargesPerDay : Math.Max(0, Math.Min(ChargesPerDay, state.charges));
        }

        /// <summary>
        /// A night in the house bed. With a charge left today: spends it and sets the bonus of <paramref name="tier"/>
        /// (+<paramref name="extraFights"/>, Luz da Maria), replacing whatever was left. True when the bonus was given.
        /// </summary>
        public static bool Sleep(HouseRestedState state, int dateKey, int tier, int extraFights = 0)
        {
            if (state == null) return false;
            RefreshCharges(state, dateKey);
            if (state.charges <= 0) return false;
            state.charges--;
            state.tier = ClampTier(tier);
            state.percent = PercentOf(tier);
            state.fightsLeft = FightsOf(tier) + Math.Max(0, extraFights);
            return true;
        }

        public static bool IsRested(HouseRestedState state) => state != null && state.fightsLeft > 0 && state.percent > 0;

        /// <summary>×(1 + percent/100) while rested, else ×1.</summary>
        public static float Multiplier(HouseRestedState state) => IsRested(state) ? 1f + state.percent / 100f : 1f;

        /// <summary>One fight ended: one less to go (the bonus ends at 0).</summary>
        public static void ConsumeFight(HouseRestedState state)
        {
            if (state == null || state.fightsLeft <= 0) return;
            state.fightsLeft--;
            if (state.fightsLeft <= 0)
            {
                state.fightsLeft = 0;
                state.percent = 0;
            }
        }

        /// <summary>Kill XP with the bonus (rounded; a positive reward never drops to 0; ×1 returns it unchanged).</summary>
        public static int ScaleExperience(int experience, float multiplier)
        {
            if (experience <= 0 || multiplier == 1f || float.IsNaN(multiplier) || multiplier <= 0f) return experience;
            return Math.Max(1, (int)Math.Round(experience * (double)multiplier, MidpointRounding.AwayFromZero));
        }

        /// <summary>The most extra XP one night can give, in "fights of XP" (percent × fights / 100): 0.2 … 0.9.</summary>
        public static float ExtraFightsWorthOfXp(int tier, int extraFights = 0) => PercentOf(tier) * (FightsOf(tier) + extraFights) / 100f;
    }
}
