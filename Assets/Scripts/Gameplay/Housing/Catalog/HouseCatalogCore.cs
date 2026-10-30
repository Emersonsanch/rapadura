using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// The house items (Housing.md §12.1; data in the generated <c>HouseCatalogData.g.cs</c>, written by
    /// "Rapadura ▸ Housing ▸ Import House Catalog" from <c>Docs/Housing/CatalogoCasa.md</c>). Runtime representation:
    /// one compiled table instead of 580 ScriptableObject assets — no asset import step, it diffs line by line,
    /// localization keys exist as soon as the code compiles, and being Unity-free the online server (H8) can
    /// compile the very same file. New items come in as new rows with new ids; an id is never reused.
    /// </summary>
    public static partial class HouseCatalog
    {
        private static HouseItemDefinition[] _items;
        private static Dictionary<string, HouseItemDefinition> _byId;

        public static IReadOnlyList<HouseItemDefinition> All
        {
            get
            {
                EnsureBuilt();
                return _items;
            }
        }

        public static HouseItemDefinition Get(string itemId)
        {
            EnsureBuilt();
            return itemId != null && _byId.TryGetValue(itemId, out HouseItemDefinition item) ? item : null;
        }

        public static bool Contains(string itemId) => Get(itemId) != null;

        /// <summary>English name: the translation table when it has one, else the pt-BR name (translation TODO).</summary>
        public static string EnglishName(HouseItemDefinition item)
        {
            if (item == null) return string.Empty;
            return GeneratedEnglishNames.TryGetValue(item.ItemId, out string name) && !string.IsNullOrEmpty(name) ? name : item.NamePtBR;
        }

        public static bool HasEnglishName(string itemId) => itemId != null && GeneratedEnglishNames.ContainsKey(itemId);

        private static void EnsureBuilt()
        {
            if (_items != null) return;
            HouseItemDefinition[] items = BuildItems();
            var byId = new Dictionary<string, HouseItemDefinition>(StringComparer.Ordinal);
            foreach (HouseItemDefinition item in items)
            {
                // Duplicates are a catalog error (tests catch them); first one wins at runtime.
                if (!byId.ContainsKey(item.ItemId)) byId.Add(item.ItemId, item);
            }

            _byId = byId;
            _items = items;
        }

        private static HouseItemSource S(HouseItemSourceType type, string refId, int weight, string label) => new HouseItemSource(type, refId, weight, label);
    }
}
