using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Wardrobe
{
    /// <summary>
    /// The wardrobe catalog (234 entries of <c>Docs/Housing/CatalogoRoupas.md</c>; data in the generated
    /// <c>WardrobeCatalogData.g.cs</c>, written by "Rapadura ▸ Housing ▸ Import Wardrobe Catalog"). Same approach as
    /// <c>HouseCatalog</c>: one compiled table, Unity-free, new entries come in as new rows with new ids.
    /// </summary>
    public static partial class WardrobeCatalog
    {
        private static WardrobeEntry[] _entries;
        private static Dictionary<string, WardrobeEntry> _byId;

        public static IReadOnlyList<WardrobeEntry> All
        {
            get
            {
                EnsureBuilt();
                return _entries;
            }
        }

        public static WardrobeEntry Get(string id)
        {
            EnsureBuilt();
            return id != null && _byId.TryGetValue(id, out WardrobeEntry entry) ? entry : null;
        }

        public static bool Contains(string id) => Get(id) != null;

        /// <summary>English name: the translation table when it has one, else the pt-BR name (translation TODO).</summary>
        public static string EnglishName(WardrobeEntry entry)
        {
            if (entry == null) return string.Empty;
            return GeneratedEnglishNames.TryGetValue(entry.Id, out string name) && !string.IsNullOrEmpty(name) ? name : entry.NamePtBR;
        }

        public static IEnumerable<WardrobeEntry> Looks => All.Where(e => e.IsLook);
        public static IEnumerable<WardrobeEntry> Dyes => All.Where(e => e.IsDye);

        /// <summary>Looks of <paramref name="slot"/> that <paramref name="cousin"/> can wear.</summary>
        public static IEnumerable<WardrobeEntry> LooksFor(AppearanceSlot slot, CharacterId cousin) => Looks.Where(e => e.Slot == slot && e.FitsCousin(cousin));

        /// <summary>
        /// Default look of a procedural piece (§9.3): the cousin's set piece of the same slot and rarity (festive sets
        /// excluded), e.g. a Rare chest on Maithe → "Túnica de linho verde bordada de vento". Null when there is none.
        /// </summary>
        public static WardrobeEntry DefaultLookFor(CharacterId cousin, AppearanceSlot slot, ItemRarity rarity)
        {
            return All.FirstOrDefault(e => e.Kind == WardrobeEntryKind.Piece && e.Owner == cousin && e.Slot == slot && e.Rarity == rarity && !e.IsFestive);
        }

        private static void EnsureBuilt()
        {
            if (_entries != null) return;
            WardrobeEntry[] entries = BuildEntries();
            var byId = new Dictionary<string, WardrobeEntry>(StringComparer.Ordinal);
            foreach (WardrobeEntry entry in entries)
            {
                if (!byId.ContainsKey(entry.Id)) byId.Add(entry.Id, entry);
            }

            _byId = byId;
            _entries = entries;
        }
    }
}
