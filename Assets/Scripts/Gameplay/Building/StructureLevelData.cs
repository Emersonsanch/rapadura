using System;
using Rapadura.Gameplay.Crafting;

namespace Rapadura.Gameplay.Building
{
    /// <summary>
    /// Cost and stats for one level of a <see cref="StructureDefinition"/>. Level 1 is the cost
    /// to place the structure for the first time; level 2+ is the cost to upgrade from the
    /// previous level. Reuses <see cref="RecipeIngredient"/> so structure costs are expressed the
    /// same way recipe ingredients are (item + quantity), instead of inventing a parallel type.
    /// </summary>
    [Serializable]
    public class StructureLevelData
    {
        public int level = 1;
        public RecipeIngredient[] cost = Array.Empty<RecipeIngredient>();
        public float maxHealth = 100f;

        /// <summary>Optional Addressable key override for this level's visual (e.g. an upgraded model). Empty = use the base key.</summary>
        public string prefabAddressableKeyOverride = string.Empty;
    }
}
