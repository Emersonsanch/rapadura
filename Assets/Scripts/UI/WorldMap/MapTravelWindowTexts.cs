using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the region filters of the Barcas de Luz window (<see cref="FastTravelWindowController"/>,
    /// window M): "Todas", "Do meu nível", the region summary under each header and the empty-filter line. The map, region and
    /// requirement names themselves stay in <c>MapTexts</c>. Same self-contained pattern as <see cref="NewGamePlusTexts"/>:
    /// merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without one.
    /// </summary>
    public static class MapTravelWindowTexts
    {
        public const string FilterAll = "ui.map_travel.filter.all";
        public const string FilterLevel = "ui.map_travel.filter.level";
        /// <summary>{0} = map count, {1} = lowest monster level, {2} = highest monster level.</summary>
        public const string RegionSummary = "ui.map_travel.region_summary";
        /// <summary>{0} = hero level.</summary>
        public const string EmptyLevel = "ui.map_travel.empty_level";
        public const string FilterHint = "ui.map_travel.filter_hint";
        /// <summary>{0} = requirement hint (where to get it).</summary>
        public const string WhereToGet = "ui.map_travel.where_to_get";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(FilterAll, "All", "Todas"),
            new LocalizationEntry(FilterLevel, "My level", "Do meu nível"),
            new LocalizationEntry(RegionSummary, "{0} maps · monsters Lv {1}–{2}", "{0} mapas · monstros Nv {1}–{2}"),
            new LocalizationEntry(EmptyLevel,
                "No map here suits level {0}. Turn off \"My level\" or pick another region.",
                "Nenhum mapa daqui serve para o nível {0}. Desligue \"Do meu nível\" ou escolha outra região."),
            new LocalizationEntry(FilterHint, "Choose a region (LB/RB or PgUp/PgDn).", "Escolha uma região (LB/RB ou PgUp/PgDn)."),
            new LocalizationEntry(WhereToGet, "Where to get it: {0}", "Onde conseguir: {0}"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
