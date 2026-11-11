using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Items.Sets
{
    /// <summary>
    /// The short floating text of each armour-set bonus. Piece names and descriptions live in <c>ContentTexts</c> like every
    /// item's. Same self-contained pattern as the other *Texts helpers.
    /// </summary>
    public static class ArmorSetTexts
    {
        public const string BargeHour = "armor_set.barge_hour";
        public const string GlazedReflect = "armor_set.glazed_reflect";
        public const string ReopenedSeal = "armor_set.reopened_seal";
        public const string ColumnRay = "armor_set.column_ray";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(BargeHour, "Hour of the Barge!", "Hora da Barca!"),
            new LocalizationEntry(GlazedReflect, "Glazed Reflection!", "Reflexo Vidrado!"),
            new LocalizationEntry(ReopenedSeal, "Seal Reopened!", "Selo Reaberto!"),
            new LocalizationEntry(ColumnRay, "Ray of the Column!", "Raio da Coluna!"),
        };

        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(BargeHour)))
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

            LocalizationEntry entry = Entries.FirstOrDefault(e => e.Key == key);
            return entry != null ? entry.PtBR : key;
        }
    }
}
