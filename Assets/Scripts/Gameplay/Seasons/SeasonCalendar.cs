using System;
using Rapadura.Gameplay.Quests;

namespace Rapadura.Gameplay.Seasons
{
    /// <summary>The seasonal events of the "Sistema de temporadas" (Docs/Seasons.md). Only the Festa Junina for now.</summary>
    public enum SeasonalEventId
    {
        None,
        /// <summary>June: bandeirinhas and fogueira in the Vila, the quadrilha and the Barraca de Prendas.</summary>
        FestaJunina,
    }

    /// <summary>
    /// Which seasonal event is on, from the date alone (pure, deterministic: no server, no random): June is the Festa
    /// Junina, every day of it, any year. <see cref="Clock"/> is the device clock by default and a fixed clock in tests;
    /// <see cref="DebugForceFestaJunina"/> turns the festa on outside June (Editor menu "Rapadura/Debug/Festa Junina").
    /// Unity-free so the online server can run the same calendar.
    /// </summary>
    public static class SeasonCalendar
    {
        public const int FestaJuninaMonth = 6;

        /// <summary>The house items' event id (<c>HouseItemSource.RefId</c> in CatalogoCasa.md).</summary>
        public const string FestaJuninaEventId = "event_festa_junina";

        /// <summary>Null = <see cref="SystemDailyClock"/>.</summary>
        public static IDailyClock Clock { get; set; }

        /// <summary>Debug toggle: the festa is on whatever the date.</summary>
        public static bool DebugForceFestaJunina { get; set; }

        public static DateTime Today => (Clock ?? SystemDailyClock.Instance).Now.Date;

        public static bool IsFestaJunina(DateTime date) => date.Month == FestaJuninaMonth;

        /// <summary>The event of <paramref name="date"/> (no debug toggle).</summary>
        public static SeasonalEventId ActiveOn(DateTime date) => IsFestaJunina(date) ? SeasonalEventId.FestaJunina : SeasonalEventId.None;

        /// <summary>Today's event, the debug toggle included.</summary>
        public static SeasonalEventId Active => DebugForceFestaJunina ? SeasonalEventId.FestaJunina : ActiveOn(Today);

        public static bool IsFestaJuninaActive => Active == SeasonalEventId.FestaJunina;

        /// <summary>Which year's festa <paramref name="date"/> belongs to (the festa of June 2027 is 2027).</summary>
        public static int SeasonYear(DateTime date) => date.Year;

        /// <summary>yyyymmdd, the day key used for "first dance of the day".</summary>
        public static int DayKey(DateTime date) => date.Year * 10000 + date.Month * 100 + date.Day;
    }
}
