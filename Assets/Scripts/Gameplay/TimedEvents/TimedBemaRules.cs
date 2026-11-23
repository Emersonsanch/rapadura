using System;
using Rapadura.Gameplay.Seasons;

namespace Rapadura.Gameplay.TimedEvents
{
    /// <summary>What the Bema changes in the scheduled events on one server-local day (all 1 / false outside it).</summary>
    public readonly struct TimedBemaModifiers
    {
        public readonly bool Active;

        /// <summary>The last day of June: the "Grande Bema".</summary>
        public readonly bool GreatBema;

        /// <summary>Multiplies the XP of every event reward (the same for everyone: no purchase involved).</summary>
        public readonly float XpMultiplier;

        /// <summary>Multiplies the chance of a rare cosmetic.</summary>
        public readonly float CosmeticChanceMultiplier;

        public TimedBemaModifiers(bool active, bool greatBema, float xpMultiplier, float cosmeticChanceMultiplier)
        {
            Active = active;
            GreatBema = greatBema;
            XpMultiplier = xpMultiplier;
            CosmeticChanceMultiplier = cosmeticChanceMultiplier;
        }

        public static readonly TimedBemaModifiers Off = new TimedBemaModifiers(false, false, 1f, 1f);
    }

    /// <summary>
    /// The Bema as the seasonal event of the online (Docs/Seasons.md, Docs/Network.md §14). Same calendar as the offline
    /// <see cref="SeasonCalendar"/> (all of June), but read on the server-local date (Brasília) of the instant passed in,
    /// never the device. Pure rules only; the decoration and the Hinos stay with the offline Bema.
    /// <list type="bullet">
    /// <item>Truce of the Bema: the Cerco da Treva does not come in June ("no Bema a Treva não chega à cidade").</item>
    /// <item>"Vigília do Bema": one extra Vigília da Coluna at 23:00 and 2 Vigília entries per day instead of 1.</item>
    /// <item>Event rewards: XP ×1,1 and oferendas (the Bema currency, <c>FestaJuninaProgress.Prendas</c>) by band.</item>
    /// <item>The "Lamparina do Bema" cosmetic joins every event's cosmetic table; on the Grande Bema (30 June) the cosmetic chance doubles.</item>
    /// </list>
    /// </summary>
    public static class TimedBemaRules
    {
        public const float XpMultiplier = 1.1f;
        public const float GreatBemaCosmeticMultiplier = 2f;
        public const int GreatBemaDayOfMonth = 30;

        /// <summary>Cosmetic only (a little lamp that floats by the hero): never stats.</summary>
        public const string BemaCosmeticId = "cosm_lamparina_do_bema";

        public static bool IsBemaDay(long localDay) => SeasonCalendar.IsFestaJunina(TimedEventAgenda.LocalDate(localDay));

        public static bool IsBema(DateTime utc) => IsBemaDay(TimedEventAgenda.LocalDay(utc));

        public static bool IsGreatBemaDay(long localDay)
        {
            DateTime date = TimedEventAgenda.LocalDate(localDay);
            return SeasonCalendar.IsFestaJunina(date) && date.Day == GreatBemaDayOfMonth;
        }

        public static TimedBemaModifiers ModifiersOn(long localDay)
        {
            if (!IsBemaDay(localDay))
            {
                return TimedBemaModifiers.Off;
            }

            bool great = IsGreatBemaDay(localDay);
            return new TimedBemaModifiers(true, great, XpMultiplier, great ? GreatBemaCosmeticMultiplier : 1f);
        }

        /// <summary>Oferendas of one event reward band during the Bema (0 outside it).</summary>
        public static int Offerings(TimedRewardBand band, bool bemaActive)
        {
            if (!bemaActive)
            {
                return 0;
            }

            switch (band)
            {
                case TimedRewardBand.Gold: return 6;
                case TimedRewardBand.Silver: return 4;
                case TimedRewardBand.Bronze: return 2;
                case TimedRewardBand.Participation: return 1;
                default: return 0;
            }
        }

        /// <summary>UTC of the first instant of the Bema that is on at <paramref name="utc"/>, or of the next one.</summary>
        public static DateTime NextBemaStartUtc(DateTime utc)
        {
            long day = TimedEventAgenda.LocalDay(utc);
            DateTime date = TimedEventAgenda.LocalDate(day);
            int year = date.Month > SeasonCalendar.FestaJuninaMonth ? date.Year + 1 : date.Year;
            var first = new DateTime(year, SeasonCalendar.FestaJuninaMonth, 1, 0, 0, 0, DateTimeKind.Utc);
            long firstDay = (long)Math.Round((first - TimedEventAgendaEpoch).TotalDays);
            return TimedEventAgenda.UtcAt(firstDay, 0);
        }

        /// <summary>UTC of the end of the Bema that is on at / next after <paramref name="utc"/> (1 July 00:00 server-local).</summary>
        public static DateTime NextBemaEndUtc(DateTime utc)
        {
            DateTime start = NextBemaStartUtc(utc);
            long startDay = TimedEventAgenda.LocalDay(start);
            return TimedEventAgenda.UtcAt(startDay + 30, 0);
        }

        private static DateTime TimedEventAgendaEpoch => Rapadura.Gameplay.WorldEvents.WorldEventCalendar.Epoch;
    }
}
