using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// All-or-nothing reward into the hero's bag for the map mechanics (the same rule as the city events' payout): every item goes in or none
    /// does, so a reward that does not fit stays due and is paid when the hero comes back with room.
    /// </summary>
    public static class MechanicsBagGrant
    {
        public const string GoldItemId = "item_gold";

        /// <summary>Puts every (id, count) of <paramref name="items"/> into <paramref name="bag"/>; false (and nothing given) when one is unknown or does not fit.</summary>
        public static bool Grant(InventoryManager bag, params (string Id, int Count)[] items)
        {
            if (bag == null)
            {
                return false;
            }

            var definitions = new ItemDefinition[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Count <= 0)
                {
                    continue;
                }

                definitions[i] = ItemDatabase.GetById(items[i].Id);
                if (definitions[i] == null)
                {
                    return false;
                }
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (definitions[i] == null)
                {
                    continue;
                }

                int left = bag.AddItem(definitions[i], items[i].Count);
                if (left <= 0)
                {
                    continue;
                }

                // Take back this item's part and everything before it.
                if (items[i].Count - left > 0)
                {
                    bag.RemoveById(items[i].Id, items[i].Count - left);
                }

                for (int j = 0; j < i; j++)
                {
                    if (definitions[j] != null)
                    {
                        bag.RemoveById(items[j].Id, items[j].Count);
                    }
                }

                return false;
            }

            return true;
        }
    }
}
