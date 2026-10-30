using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>
    /// Localization (en + pt-BR) of the wardrobe catalog: <c>wardrobe.&lt;id&gt;.name</c> / <c>.desc</c> for all 234 entries.
    /// Names: pt-BR from <c>CatalogoRoupas.md</c>; English from the optional <c>CatalogoRoupas.en.tsv</c>, else the pt-BR
    /// name (translation TODO, same as the house catalog). Descriptions are generated: "Peito · Raro · Conjunto Pena de
    /// Gavião · Drop: Kök-Böri (chefe)". Self-contained like <see cref="HouseItemTexts"/>.
    /// </summary>
    public static class WardrobeTexts
    {
        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(WardrobeCatalog.All.Count * 2);
            foreach (WardrobeEntry entry in WardrobeCatalog.All)
            {
                entries.Add(new LocalizationEntry(entry.NameKey, WardrobeCatalog.EnglishName(entry), entry.NamePtBR));
                entries.Add(new LocalizationEntry(entry.DescKey, Describe(entry, false), Describe(entry, true)));
            }

            return entries;
        }

        public static string Describe(WardrobeEntry entry, bool ptBR)
        {
            var parts = new List<string> { entry.IsDye ? (ptBR ? "Tinta · " + entry.ColorName : "Dye · " + entry.ColorName) : SlotName(entry.Slot, ptBR) };
            parts.Add(HouseItemTexts.RarityName(entry.Rarity, ptBR));
            if (!string.IsNullOrEmpty(entry.SetName)) parts.Add((ptBR ? "Conjunto " : "Set ") + entry.SetName);
            parts.Add(OriginText(entry.SourceLabel, ptBR));
            return string.Join(" · ", parts);
        }

        public static string SlotName(AppearanceSlot slot, bool ptBR)
        {
            switch (slot)
            {
                case AppearanceSlot.Head: return ptBR ? "Cabeça" : "Head";
                case AppearanceSlot.Chest: return ptBR ? "Peito" : "Chest";
                case AppearanceSlot.Legs: return ptBR ? "Pernas" : "Legs";
                case AppearanceSlot.Boots: return ptBR ? "Pés" : "Feet";
                case AppearanceSlot.Accessory: return ptBR ? "Acessório" : "Accessory";
                case AppearanceSlot.FaceNeck: return ptBR ? "Rosto/Pescoço" : "Face/Neck";
                case AppearanceSlot.Back: return ptBR ? "Costas" : "Back";
                case AppearanceSlot.Caramelo: return ptBR ? "Ari" : "Ari";
                default: return ptBR ? "Arma" : "Weapon";
            }
        }

        private static readonly (string Pt, string En)[] OriginPrefixes =
        {
            ("Loja: ", "Shop: "), ("Missão da Casa: ", "House quest: "), ("Missão pessoal da ", "Personal quest of "),
            ("Missão pessoal do ", "Personal quest of "), ("Missão regional da Floresta (2ª)", "Second Zarafshan Woods region quest"),
            ("Missão: ", "Quest: "), ("Diária: ", "Daily: "), ("Conquista: ", "Achievement: "), ("Evento: ", "Event: "),
            ("Crafting: ", "Crafting: "), ("Drop: inimigos comuns da Floresta e do Deserto", "Drop: common enemies of the Zarafshan Woods and the Flaming Sands"),
            ("Drop: ", "Drop: "), ("Segredo do mapa", "Map secret"), ("Desbloqueado desde o começo", "Unlocked from the start"),
        };

        /// <summary>Source labels kept as parse keys in the catalog (WardrobeCatalogImport) but shown with the re-themed region names.</summary>
        private static readonly (string Key, string Pt)[] PtOriginDisplay =
        {
            ("Missão regional da Floresta (2ª)", "Missão regional do Bosque do Zarafshan (2ª)"),
            ("Drop: inimigos comuns da Floresta e do Deserto", "Drop: inimigos comuns do Bosque do Zarafshan e das Areias Flamejantes"),
        };

        public static string OriginText(string origin, bool ptBR)
        {
            string text = (origin ?? string.Empty).Replace("`", string.Empty).Trim();
            if (ptBR)
            {
                foreach ((string key, string pt) in PtOriginDisplay)
                {
                    if (text.StartsWith(key, StringComparison.Ordinal)) return pt + text.Substring(key.Length);
                }

                return text;
            }
            foreach ((string pt, string en) in OriginPrefixes)
            {
                if (text.StartsWith(pt, StringComparison.Ordinal)) return en + text.Substring(pt.Length);
            }

            return text;
        }

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
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key)) return manager.Get(key);
            }

            return GetFallback(key) ?? key;
        }

        public static string Name(LocalizationManager manager, string entryId)
        {
            WardrobeEntry entry = WardrobeCatalog.Get(entryId);
            return entry != null ? Get(manager, entry.NameKey) : entryId ?? string.Empty;
        }

        public static string Description(LocalizationManager manager, string entryId)
        {
            WardrobeEntry entry = WardrobeCatalog.Get(entryId);
            return entry != null ? Get(manager, entry.DescKey) : string.Empty;
        }

        public static string GetFallback(string key)
        {
            if (key == null) return null;
            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries) table[entry.Key] = entry.PtBR;
                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
