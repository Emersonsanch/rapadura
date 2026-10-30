using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Dialogue;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Localization keys (en + pt-BR) for the world map village hub (<see cref="VillageHubButton"/>): the "Vila"
    /// HUD button and its panel (Conversar with each villager, Loja, Bancada). Villager names reuse the
    /// <c>dialogue.&lt;npc&gt;.name</c> keys of their dialogues. Same self-contained pattern as
    /// <see cref="WorldMapTexts"/>: merged into the live <see cref="LocalizationManager"/> on first use (keys
    /// it already has win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class VillageHubTexts
    {
        public const string Button = "ui.village.button";
        public const string ButtonNews = "ui.village.button_news";
        public const string Title = "ui.village.title";
        public const string Subtitle = "ui.village.subtitle";
        public const string TalkHeader = "ui.village.talk_header";
        public const string ServicesHeader = "ui.village.services_header";
        public const string Shop = "ui.village.shop";
        public const string Crafting = "ui.village.crafting";
        /// <summary>{0} = villager name, {1} = the cousin whose story it is.</summary>
        public const string NpcLabel = "ui.village.npc_label";
        /// <summary>Like <see cref="NpcLabel"/>, with the "(!)" marker.</summary>
        public const string NpcLabelNews = "ui.village.npc_label_news";
        public const string GatheringPoint = VillageNpcCatalog.GatheringPointNameKey;
        public const string Close = "ui.village.close";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Button, "Village", "Vila"),
            new LocalizationEntry(ButtonNews, "Village (!)", "Vila (!)"),
            new LocalizationEntry(Title, "Grandma Edna's village", "Vila da Vó Edna"),
            new LocalizationEntry(Subtitle, "Talk to the villagers, shop or use the workbench.", "Converse com os moradores, faça compras ou use a bancada."),
            new LocalizationEntry(TalkHeader, "Talk", "Conversar"),
            new LocalizationEntry(ServicesHeader, "Services", "Serviços"),
            new LocalizationEntry(Shop, "Shop", "Loja"),
            new LocalizationEntry(Crafting, "Workbench", "Bancada"),
            new LocalizationEntry(NpcLabel, "{0} · {1}", "{0} · {1}"),
            new LocalizationEntry(NpcLabelNews, "{0} · {1} (!)", "{0} · {1} (!)"),
            new LocalizationEntry(GatheringPoint, "Gathering point", "Ponto de encontro"),
            new LocalizationEntry(Close, "Close", "Fechar"),
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

        /// <summary>Localized text via <paramref name="manager"/> when it knows the key (ours or the core table's), else the pt-BR fallback, else the key.</summary>
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

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
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

        /// <summary>pt-BR text from <see cref="Entries"/> or the core table (e.g. the villagers' <c>dialogue.*.name</c>), or null.</summary>
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

                foreach (LocalizationEntry entry in LocalizationManager.DefaultEntries.All)
                {
                    if (entry != null && !string.IsNullOrEmpty(entry.Key) && !table.ContainsKey(entry.Key))
                    {
                        table[entry.Key] = entry.PtBR;
                    }
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
