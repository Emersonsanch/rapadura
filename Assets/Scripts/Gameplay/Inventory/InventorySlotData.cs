using System;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// A single inventory slot's runtime state. Stores only the item id (not a direct asset
    /// reference) plus quantity/durability, so this class serializes cleanly to JSON — the
    /// actual <see cref="ItemDefinition"/> is resolved through <see cref="ItemDatabase"/>.
    /// </summary>
    [Serializable]
    public class InventorySlotData
    {
        public string itemId;
        public int quantity;
        public float currentDurability;

        public bool IsEmpty => string.IsNullOrEmpty(itemId) || quantity <= 0;

        public static InventorySlotData Empty()
        {
            return new InventorySlotData { itemId = null, quantity = 0, currentDurability = 0f };
        }

        public void Clear()
        {
            itemId = null;
            quantity = 0;
            currentDurability = 0f;
        }
    }
}
