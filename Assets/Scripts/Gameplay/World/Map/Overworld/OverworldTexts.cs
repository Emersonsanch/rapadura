using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Localization (en + pt-BR) of the continuous regions: the area names (from the zones of every region) and the lines
    /// of the overworld (barrier, Barca between docks of the same region). Same self-contained pattern as <see cref="MapTexts"/>.
    /// </summary>
    public static class OverworldTexts
    {
        /// <summary>{0} = walk level.</summary>
        public const string BarrierLevel = "ui.overworld.barrier_level";
        /// <summary>{0} = requirement.</summary>
        public const string BarrierRequirement = "ui.overworld.barrier_requirement";
        public const string DockNotVisited = "ui.overworld.dock_not_visited";
        /// <summary>{0} = settlement / area name.</summary>
        public const string BarcaArrived = "ui.overworld.barca_arrived";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;
        private static LocalizationManager _mergedInto;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var entries = new List<LocalizationEntry>
            {
                new LocalizationEntry(BarrierLevel, "The way is too dangerous: on foot only from level {0}", "O caminho é perigoso demais: a pé só a partir do nível {0}"),
                new LocalizationEntry(BarrierRequirement, "You cannot go on without: {0}", "Não dá para seguir sem: {0}"),
                new LocalizationEntry(DockNotVisited, "Walk to that dock once before asking the barca", "Ande até esse cais uma vez antes de pedir a barca"),
                new LocalizationEntry(BarcaArrived, "The barca leaves you at {0}", "A barca deixa vocês em {0}"),
            };

            foreach (string regionId in OverworldLayouts.RegionLayoutIds)
            {
                OverworldLayout region = OverworldLayouts.Get(regionId);
                if (region == null)
                {
                    continue;
                }

                foreach (OverworldZone zone in region.Zones)
                {
                    entries.Add(new LocalizationEntry(zone.NameKey, zone.NameEn ?? zone.Id, zone.NamePt ?? zone.Id));
                }
            }

            return entries;
        }

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(BarrierLevel)))
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

            if (_fallback == null)
            {
                _fallback = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallback[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallback.TryGetValue(key, out string text) ? text : key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string format = Get(manager, key);
            try
            {
                return string.Format(format, args);
            }
            catch (System.FormatException)
            {
                return format;
            }
        }

        /// <summary>The area's name (pt-BR fallback without a manager: the zone's own text).</summary>
        public static string ZoneName(LocalizationManager manager, OverworldZone zone)
        {
            if (zone == null)
            {
                return string.Empty;
            }

            string text = Get(manager, zone.NameKey);
            return text == zone.NameKey ? zone.NamePt ?? zone.Id : text;
        }
    }
}
