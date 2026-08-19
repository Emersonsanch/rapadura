using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.Crafting
{
    /// <summary>
    /// Static lookup that resolves a recipe id back to its <see cref="RecipeDefinition"/> asset.
    /// Mirrors <see cref="Rapadura.Gameplay.Items.ItemDatabase"/>: recipes are loaded from
    /// Resources/Recipes so save data and CraftingManager only need to store recipe ids.
    /// </summary>
    public static class RecipeDatabase
    {
        private const string ResourcesFolder = "Recipes";

        private static Dictionary<string, RecipeDefinition> _recipesById;

        private static void EnsureLoaded()
        {
            if (_recipesById != null)
            {
                return;
            }

            _recipesById = new Dictionary<string, RecipeDefinition>();

            foreach (RecipeDefinition recipe in Resources.LoadAll<RecipeDefinition>(ResourcesFolder))
            {
                if (recipe == null || string.IsNullOrEmpty(recipe.RecipeId))
                {
                    continue;
                }

                _recipesById[recipe.RecipeId] = recipe;
            }
        }

        public static RecipeDefinition GetById(string recipeId)
        {
            EnsureLoaded();
            return _recipesById.TryGetValue(recipeId, out RecipeDefinition recipe) ? recipe : null;
        }

        public static bool TryGetById(string recipeId, out RecipeDefinition recipe)
        {
            EnsureLoaded();
            return _recipesById.TryGetValue(recipeId, out recipe);
        }

        public static IEnumerable<RecipeDefinition> GetAll()
        {
            EnsureLoaded();
            return _recipesById.Values;
        }

        /// <summary>Forces the database to re-scan Resources/Recipes. Call after generating/removing recipes in the editor.</summary>
        public static void Invalidate()
        {
            _recipesById = null;
        }
    }
}
