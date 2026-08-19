using Rapadura.Core.EventBus;

namespace Rapadura.Gameplay.Crafting
{
    /// <summary>Raised when a recipe is successfully crafted (ingredients consumed, result produced).</summary>
    public readonly struct RecipeCraftedEvent : IGameEvent
    {
        public readonly RecipeDefinition Recipe;

        public RecipeCraftedEvent(RecipeDefinition recipe)
        {
            Recipe = recipe;
        }
    }

    /// <summary>Raised when a new recipe becomes known (unlocked) for crafting.</summary>
    public readonly struct RecipeUnlockedEvent : IGameEvent
    {
        public readonly RecipeDefinition Recipe;

        public RecipeUnlockedEvent(RecipeDefinition recipe)
        {
            Recipe = recipe;
        }
    }
}
