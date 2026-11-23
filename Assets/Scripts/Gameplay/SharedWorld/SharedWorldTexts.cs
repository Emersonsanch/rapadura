using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.SharedWorld
{
    /// <summary>
    /// Localization (en + pt-BR) of the shared world: the map-level warnings (<see cref="MapLevelWarning"/>) and the loot
    /// protection lines. Same self-contained pattern as <c>PvpTexts</c>: merged into the live <see cref="LocalizationManager"/>
    /// on first use (keys it already has win), pt-BR fallback otherwise.
    /// </summary>
    public static class SharedWorldTexts
    {
        /// <summary>"Saque de {0} por mais {1} s."</summary>
        public const string LootProtected = "shared.loot.protected";

        /// <summary>"O saque agora é de quem pegar."</summary>
        public const string LootFree = "shared.loot.free";

        /// <summary>"Você vai ficar com o saque: {0}."</summary>
        public const string KillOwned = "shared.loot.kill_owned";

        public static string WarningKey(MapLevelWarning warning) => "shared.map_level." + Snake(warning.ToString());

        private static readonly (MapLevelWarning Warning, string En, string Pt)[] WarningTexts =
        {
            (MapLevelWarning.None, "", ""),
            (MapLevelWarning.BelowWalkLevel, "This land asks for level {0}. You may go on, but walk carefully.",
                "Esta terra pede nível {0}. Dá para seguir, mas ande com cuidado."),
            (MapLevelWarning.StrongMonsters, "Dangerous: the creatures here are much stronger than you (up to level {0}).",
                "Perigoso: os bichos daqui são bem mais fortes que você (até o nível {0})."),
            (MapLevelWarning.Lethal, "Deadly: one blow from the creatures here (level {0}) may be enough to bring you down.",
                "Mortal: um golpe dos bichos daqui (nível {0}) pode bastar para te derrubar."),
        };

        private static readonly LocalizationEntry[] UiEntries =
        {
            new LocalizationEntry(LootProtected, "{0}'s loot for {1} more s.", "Saque de {0} por mais {1} s."),
            new LocalizationEntry(LootFree, "The loot is now anyone's.", "O saque agora é de quem pegar."),
            new LocalizationEntry(KillOwned, "The loot is yours: {0}.", "Você vai ficar com o saque: {0}."),
        };

        public static readonly List<LocalizationEntry> Entries = BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(UiEntries);
            foreach (var t in WarningTexts)
            {
                if (t.Warning != MapLevelWarning.None)
                {
                    entries.Add(new LocalizationEntry(WarningKey(t.Warning), t.En, t.Pt));
                }
            }

            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(LootProtected)))
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
            catch (FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        /// <summary>The warning line for <paramref name="notice"/> (empty for none): the walk level, or the zone's top monster level.</summary>
        public static string Warning(LocalizationManager manager, MapLevelNotice notice, int zoneMaxMonsterLevel)
        {
            if (notice.Warning == MapLevelWarning.None)
            {
                return string.Empty;
            }

            int shown = notice.Warning == MapLevelWarning.BelowWalkLevel ? notice.RequiredLevel : zoneMaxMonsterLevel;
            return Format(manager, WarningKey(notice.Warning), shown);
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                var map = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    map[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = map;
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        private static string Snake(string pascal)
        {
            var chars = new List<char>(pascal.Length + 4);
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c) && i > 0)
                {
                    chars.Add('_');
                }

                chars.Add(char.ToLowerInvariant(c));
            }

            return new string(chars.ToArray());
        }
    }
}
