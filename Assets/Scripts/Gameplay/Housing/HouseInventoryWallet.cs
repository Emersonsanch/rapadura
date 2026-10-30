using System;
using Rapadura.Gameplay.Inventory;

namespace Rapadura.Gameplay.Housing
{
    /// <summary>
    /// <see cref="IHouseWallet"/> over the live bag (a <c>CarriedInventorySession</c> inventory on the map) and the Baú
    /// da casa: counts both, spends from the bag first and the house chest after. Either may be null.
    /// </summary>
    public sealed class HouseInventoryWallet : IHouseWallet
    {
        private readonly InventoryManager _bag;
        private readonly VillageChestStorage _chest;

        public HouseInventoryWallet(InventoryManager bag, VillageChestStorage chest)
        {
            _bag = bag;
            _chest = chest;
        }

        public int BagCount(string itemId) => _bag != null && !string.IsNullOrEmpty(itemId) ? _bag.GetTotalCount(itemId) : 0;

        public int ChestCount(string itemId) => _chest != null && !string.IsNullOrEmpty(itemId) ? _chest.GetTotalCount(itemId) : 0;

        public int Count(string itemId) => BagCount(itemId) + ChestCount(itemId);

        public bool Remove(string itemId, int quantity)
        {
            if (quantity <= 0) return true;
            if (Count(itemId) < quantity) return false;

            int fromBag = Math.Min(BagCount(itemId), quantity);
            if (fromBag > 0 && !_bag.RemoveById(itemId, fromBag)) return false;

            int left = quantity - fromBag;
            if (left <= 0 || _chest == null) return left <= 0;
            for (int i = 0; i < _chest.Slots.Count && left > 0; i++)
            {
                InventorySlotData slot = _chest.Slots[i];
                if (slot == null || slot.IsEmpty || slot.itemId != itemId) continue;
                int take = Math.Min(slot.quantity, left);
                if (_chest.RemoveFromSlot(i, take)) left -= take;
            }

            return left <= 0;
        }
    }
}
