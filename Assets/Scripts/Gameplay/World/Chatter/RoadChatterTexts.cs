using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Chatter
{
    /// <summary>
    /// Localization keys (en + pt-BR) of "Conversas entre primos na estrada": the bubble's chrome
    /// (<c>ui.road_chatter.*</c>) and both lines of every <see cref="RoadChatterCatalog"/> exchange
    /// (<c>road_chatter.&lt;id&gt;.line_1/2</c>, built from the catalog's inline texts). Same self-contained pattern as
    /// <c>WorldEventTexts</c>: merged into the live <see cref="LocalizationManager"/> on first use (keys it already has
    /// win), pt-BR fallback without one.
    /// </summary>
    public static class RoadChatterTexts
    {
        public const string HeaderRadio = "ui.road_chatter.header_radio";
        public const string HeaderMemory = "ui.road_chatter.header_memory";
        /// <summary>{0} = speaker label, {1} = the line.</summary>
        public const string Line = "ui.road_chatter.line";
        /// <summary>{0} = cousin answering on the radio.</summary>
        public const string OnTheRadio = "ui.road_chatter.on_the_radio";
        /// <summary>{0} = cousin being remembered.</summary>
        public const string Remembered = "ui.road_chatter.remembered";

        public static readonly List<LocalizationEntry> Entries = Build();

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        private static List<LocalizationEntry> Build()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(HeaderRadio, "Battery radio", "Radinho de pilha"),
                new LocalizationEntry(HeaderMemory, "A memory on the road", "Lembrança da estrada"),
                new LocalizationEntry(Line, "{0}: {1}", "{0}: {1}"),
                new LocalizationEntry(OnTheRadio, "{0} (on the radio)", "{0} (no rádio)"),
                new LocalizationEntry(Remembered, "{0} (remembered)", "{0} (lembrança)"),
            };

            foreach (RoadChatterSpec spec in RoadChatterCatalog.All)
            {
                entries.Add(new LocalizationEntry(spec.LineKey(0), spec.FirstLine.En, spec.FirstLine.Pt));
                entries.Add(new LocalizationEntry(spec.LineKey(1), spec.SecondLine.En, spec.SecondLine.Pt));
            }

            return entries;
        }

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(HeaderRadio)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

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

        /// <summary><see cref="Get"/> + <c>string.Format</c>, tolerant of malformed translations.</summary>
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
