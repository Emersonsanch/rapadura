using System;
using System.Collections.Generic;
using System.Linq;

namespace Rapadura.Gameplay.Items.Procedural
{
    /// <summary>
    /// A single rolled affix result: which <see cref="ItemAffixDefinition"/> was drawn and the
    /// concrete value rolled within its [min, max] range.
    /// </summary>
    public readonly struct RolledAffix
    {
        public readonly ItemAffixDefinition Affix;
        public readonly float RolledValue;

        public RolledAffix(ItemAffixDefinition affix, float rolledValue)
        {
            Affix = affix;
            RolledValue = rolledValue;
        }
    }

    /// <summary>
    /// The result of <see cref="ProceduralItemGenerator"/> rolling affixes onto a base
    /// <see cref="ItemDefinition"/>: the base item, the rarity it was generated at, the affixes
    /// rolled (with concrete values), and the final display name combining prefix/suffix affixes
    /// around the base item's name (e.g. "Espada Flamejante da Fúria").
    ///
    /// This is a plain, non-ScriptableObject runtime data holder — it is not itself persisted;
    /// see <see cref="Rapadura.Gameplay.Inventory.InventorySlotData"/> for the minimal
    /// serializable representation (affix ids + rolled values) used for save/load.
    /// </summary>
    public class GeneratedItemInstance
    {
        public ItemDefinition BaseItem { get; }
        public ItemRarity Rarity { get; }
        public IReadOnlyList<RolledAffix> Affixes { get; }
        public string GeneratedName { get; }

        public GeneratedItemInstance(ItemDefinition baseItem, ItemRarity rarity, IReadOnlyList<RolledAffix> affixes, string generatedName)
        {
            BaseItem = baseItem;
            Rarity = rarity;
            Affixes = affixes ?? Array.Empty<RolledAffix>();
            GeneratedName = generatedName;
        }

        /// <summary>Convenience accessor mirroring the shape saved into <see cref="Inventory.InventorySlotData"/>.</summary>
        public string[] AffixIds => Affixes.Select(a => a.Affix.AffixId).ToArray();

        /// <summary>Convenience accessor mirroring the shape saved into <see cref="Inventory.InventorySlotData"/>.</summary>
        public float[] AffixValues => Affixes.Select(a => a.RolledValue).ToArray();
    }
}
