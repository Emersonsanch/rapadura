using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Items.Durability
{
    /// <summary>
    /// en + pt-BR text of durability and the ferreiro's repair (FASE 16): tooltip lines, the repair part of the
    /// "Ferreiro" window and its results. Same self-contained pattern as the other *Texts helpers: merged into the live
    /// <see cref="LocalizationManager"/> on first use (keys it already has win), pt-BR fallback without one.
    /// </summary>
    public static class DurabilityTexts
    {
        public const string TooltipWorn = "ui.durability.tooltip.worn";
        public const string TooltipBroken = "ui.durability.tooltip.broken";
        /// <summary>{0} current, {1} max, {2} gold.</summary>
        public const string RowLine = "ui.durability.repair.row";
        /// <summary>{0} gold.</summary>
        public const string RepairButton = "ui.durability.repair.button";
        /// <summary>{0} pieces, {1} gold.</summary>
        public const string RepairAllButton = "ui.durability.repair.all_button";
        public const string NothingToRepair = "ui.durability.repair.nothing";
        /// <summary>{0} gold needed.</summary>
        public const string MissingGold = "ui.durability.repair.missing_gold";
        /// <summary>{0} item.</summary>
        public const string Repaired = "ui.durability.repair.done_one";
        /// <summary>{0} pieces, {1} gold.</summary>
        public const string RepairedAll = "ui.durability.repair.done_all";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(TooltipWorn, "Worn out: take it to the blacksmith soon.", "Gasto: leve ao ferreiro logo."),
            new LocalizationEntry(TooltipBroken, "Broken: gives no bonus until the blacksmith fixes it (it is never lost).",
                "Quebrado: não dá bônus até o ferreiro consertar (nunca some)."),
            new LocalizationEntry(RowLine, "Durability {0}/{1} · repair {2} gold", "Durabilidade {0}/{1} · conserto {2} de ouro"),
            new LocalizationEntry(RepairButton, "Repair ({0} gold)", "Consertar ({0} de ouro)"),
            new LocalizationEntry(RepairAllButton, "Repair all: {0} pieces ({1} gold)", "Consertar tudo: {0} peças ({1} de ouro)"),
            new LocalizationEntry(NothingToRepair, "Everything is in good shape.", "Tudo inteirinho, nada para consertar."),
            new LocalizationEntry(MissingGold, "Not enough gold: the repair costs {0}.", "Falta ouro: o conserto custa {0}."),
            new LocalizationEntry(Repaired, "{0} is as good as new!", "{0} ficou novinho em folha!"),
            new LocalizationEntry(RepairedAll, "{0} pieces repaired for {1} gold.", "{0} peças consertadas por {1} de ouro."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
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
