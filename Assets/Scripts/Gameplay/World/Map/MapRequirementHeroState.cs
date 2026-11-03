using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Mounts;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>
    /// Reads the live hero into a <see cref="MapRequirementState"/> for the gates of the maps: the backpack and the worn
    /// gear of an <see cref="InventoryManager"/>, the selected cousin and the owned mounts (<see cref="MountCollection"/>).
    /// The rules themselves stay in the pure <see cref="MapRequirementRules"/>.
    /// </summary>
    public static class MapRequirementHeroState
    {
        /// <summary>The hero now: <paramref name="bag"/> (may be null), the selected cousin and the shared mount collection.</summary>
        public static MapRequirementState Capture(InventoryManager bag)
        {
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            MountCollection mounts = ServiceLocator.TryGet(out MountCollection collection) ? collection : null;
            return Capture(bag != null ? bag.Slots : null, bag != null ? bag.Equipped : null, hero, mounts != null ? mounts.Mounts : null);
        }

        /// <summary>The same from plain data (tests, a carried session).</summary>
        public static MapRequirementState Capture(IEnumerable<InventorySlotData> slots, IEnumerable<KeyValuePair<EquipmentSlot, InventorySlotData>> equipped,
            CharacterId? hero, IEnumerable<OwnedMount> mounts)
        {
            var state = new MapRequirementState(hero);
            if (slots != null)
            {
                foreach (InventorySlotData slot in slots)
                {
                    if (slot != null && slot.quantity > 0 && !string.IsNullOrEmpty(slot.itemId))
                    {
                        state.Carried.Add(slot.itemId);
                    }
                }
            }

            if (equipped != null)
            {
                foreach (KeyValuePair<EquipmentSlot, InventorySlotData> pair in equipped)
                {
                    if (pair.Value != null && !string.IsNullOrEmpty(pair.Value.itemId))
                    {
                        state.Wear(pair.Key, pair.Value.itemId);
                    }
                }
            }

            if (mounts != null)
            {
                foreach (OwnedMount mount in mounts)
                {
                    MountSpec spec = mount != null ? MountCatalog.FindSpec(mount.mountId) : null;
                    if (spec != null)
                    {
                        state.Own(spec.Type);
                    }
                }
            }

            return state;
        }

        /// <summary>The lookup for <see cref="MapAccessRules"/> from <paramref name="bag"/>.</summary>
        public static Func<string, bool> LookupFor(InventoryManager bag) => MapRequirementRules.Lookup(Capture(bag));
    }
}
