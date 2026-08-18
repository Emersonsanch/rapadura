using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Items
{
    /// <summary>
    /// Static lookup that resolves an item id back to its <see cref="ItemDefinition"/> asset.
    /// All example items are generated under Resources/Items so they load through this class
    /// without any manual per-manager reference wiring — the inventory save file only needs to
    /// store item ids and quantities, and this database rebuilds the rest at load time.
    /// </summary>
    public static class ItemDatabase
    {
        private const string ResourcesFolder = "Items";

        private static Dictionary<string, ItemDefinition> _itemsById;

        private static void EnsureLoaded()
        {
            if (_itemsById != null)
            {
                return;
            }

            _itemsById = new Dictionary<string, ItemDefinition>();

            foreach (ItemDefinition item in Resources.LoadAll<ItemDefinition>(ResourcesFolder))
            {
                if (item == null || string.IsNullOrEmpty(item.ItemId))
                {
                    continue;
                }

                _itemsById[item.ItemId] = item;
            }
        }

        public static ItemDefinition GetById(string itemId)
        {
            EnsureLoaded();
            return _itemsById.TryGetValue(itemId, out ItemDefinition item) ? item : null;
        }

        public static bool TryGetById(string itemId, out ItemDefinition item)
        {
            EnsureLoaded();
            return _itemsById.TryGetValue(itemId, out item);
        }

        public static IEnumerable<ItemDefinition> GetAll()
        {
            EnsureLoaded();
            return _itemsById.Values;
        }

        /// <summary>Forces the database to re-scan Resources/Items. Call after generating/removing items in the editor.</summary>
        public static void Invalidate()
        {
            _itemsById = null;
        }
    }
}
