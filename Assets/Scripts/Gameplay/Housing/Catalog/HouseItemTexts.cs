using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// Localization (en + pt-BR) of the house catalog: <c>house.&lt;id&gt;.name</c> and <c>house.&lt;id&gt;.desc</c> for all
    /// 580 items, built from <see cref="HouseCatalog"/>. Names: pt-BR from the catalog; English from the optional
    /// translation table (<c>Docs/Housing/CatalogoCasa.en.tsv</c>), else the pt-BR name (TODO: translate).
    /// Descriptions are generated: "Móveis de sala · Comum · Loja: Marcenaria do Mestre Damião".
    /// Same self-contained pattern as the other *Texts helpers: merged into the live manager on first use.
    /// </summary>
    public static class HouseItemTexts
    {
        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= BuildEntries();

        private static List<LocalizationEntry> BuildEntries()
        {
            var entries = new List<LocalizationEntry>(HouseCatalog.All.Count * 2);
            foreach (HouseItemDefinition item in HouseCatalog.All)
            {
                entries.Add(new LocalizationEntry(item.NameKey, HouseCatalog.EnglishName(item), item.NamePtBR));
                entries.Add(new LocalizationEntry(item.DescKey, Describe(item, false), Describe(item, true)));
            }

            return entries;
        }

        public static string Describe(HouseItemDefinition item, bool ptBR)
        {
            string origin = item.PrimarySource?.Label ?? string.Empty;
            return CategoryName(item.Category, ptBR) + " · " + RarityName(item.Rarity, ptBR) + " · " + (ptBR ? CleanOrigin(origin) : OriginEn(origin));
        }

        public static string CategoryName(HouseItemCategory category, bool ptBR)
        {
            switch (category)
            {
                case HouseItemCategory.LivingRoom: return ptBR ? "Móveis de sala" : "Living room";
                case HouseItemCategory.Bedroom: return ptBR ? "Quarto" : "Bedroom";
                case HouseItemCategory.Kitchen: return ptBR ? "Cozinha" : "Kitchen";
                case HouseItemCategory.Bathroom: return ptBR ? "Banheiro" : "Bathroom";
                case HouseItemCategory.Yard: return ptBR ? "Quintal e horta" : "Yard and garden";
                case HouseItemCategory.Corral: return ptBR ? "Curral e animais" : "Corral and animals";
                case HouseItemCategory.Lighting: return ptBR ? "Iluminação" : "Lighting";
                case HouseItemCategory.WallDecor: return ptBR ? "Paredes e quadros" : "Walls and pictures";
                case HouseItemCategory.Rug: return ptBR ? "Tapetes" : "Rugs";
                case HouseItemCategory.Curtain: return ptBR ? "Cortinas" : "Curtains";
                case HouseItemCategory.Plant: return ptBR ? "Plantas" : "Plants";
                case HouseItemCategory.Music: return ptBR ? "Instrumentos musicais" : "Musical instruments";
                case HouseItemCategory.Toy: return ptBR ? "Brinquedos" : "Toys";
                case HouseItemCategory.Regional: return ptBR ? "Decoração regional" : "Regional decor";
                case HouseItemCategory.Trophy: return ptBR ? "Troféus de chefes" : "Boss trophies";
                case HouseItemCategory.Seasonal: return ptBR ? "Itens sazonais" : "Seasonal items";
                case HouseItemCategory.Family: return ptBR ? "História de família" : "Family keepsakes";
                default: return ptBR ? "Utilitários" : "Utilities";
            }
        }

        public static string RarityName(ItemRarity rarity, bool ptBR)
        {
            switch (rarity)
            {
                case ItemRarity.Uncommon: return ptBR ? "Incomum" : "Uncommon";
                case ItemRarity.Rare: return ptBR ? "Raro" : "Rare";
                case ItemRarity.Epic: return ptBR ? "Épico" : "Epic";
                case ItemRarity.Legendary: return ptBR ? "Lendário" : "Legendary";
                default: return ptBR ? "Comum" : "Common";
            }
        }

        /// <summary>Drops the <c>`ids`</c> the catalog writes for designers.</summary>
        private static string CleanOrigin(string origin)
        {
            int tick = origin.IndexOf(" (`", StringComparison.Ordinal);
            string text = tick >= 0 ? origin.Substring(0, tick) : origin;
            return text.Replace("`", string.Empty).Trim();
        }

        private static readonly (string Pt, string En)[] OriginPrefixes =
        {
            ("Loja: ", "Shop: "),
            ("Missão da Casa: ", "House quest: "),
            ("Missão pessoal da ", "Personal quest of "),
            ("Missão pessoal do ", "Personal quest of "),
            ("Missão: ", "Quest: "),
            ("Diária: ", "Daily: "),
            ("Conquista: ", "Achievement: "),
            ("Evento: ", "Event: "),
            ("Crafting: ", "Crafting: "),
            ("Drop: ", "Drop: "),
            ("Segredo do mapa", "Map secret"),
        };

        private static string OriginEn(string origin)
        {
            string text = CleanOrigin(origin);
            foreach ((string pt, string en) in OriginPrefixes)
            {
                if (text.StartsWith(pt, StringComparison.Ordinal))
                {
                    return en + text.Substring(pt.Length);
                }
            }

            return text;
        }

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
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key)) return manager.Get(key);
            }

            return GetFallback(key) ?? key;
        }

        public static string Name(LocalizationManager manager, HouseItemDefinition item) => item == null ? string.Empty : Get(manager, item.NameKey);

        public static string Name(LocalizationManager manager, string itemId)
        {
            HouseItemDefinition item = HouseCatalog.Get(itemId);
            return item != null ? Name(manager, item) : itemId ?? string.Empty;
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
