using System;
using Rapadura.Gameplay.Items;

namespace Rapadura.Gameplay.Crafting
{
    /// <summary>One (item, quantity) requirement inside a <see cref="RecipeDefinition"/>.</summary>
    [Serializable]
    public struct RecipeIngredient
    {
        public ItemDefinition item;
        public int quantity;
    }
}
