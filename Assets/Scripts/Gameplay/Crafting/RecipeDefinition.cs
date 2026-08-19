using System;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Gameplay.Crafting
{
    /// <summary>
    /// Data-driven definition of a single crafting recipe. Follows the same pattern as
    /// <see cref="ItemDefinition"/>/SkillDefinition: every number a designer needs to tune —
    /// ingredients, result, craft time, station requirement and the unlock condition — lives
    /// here as a ScriptableObject asset, so <see cref="CraftingManager"/> contains no
    /// per-recipe special-casing.
    /// </summary>
    [CreateAssetMenu(fileName = "NewRecipe", menuName = "Rapadura/Crafting/Recipe Definition", order = 0)]
    public class RecipeDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _recipeId = "recipe_new";
        [SerializeField] private string _displayName = "New Recipe";
        [TextArea(2, 4)]
        [SerializeField] private string _description = string.Empty;

        [Header("Ingredients")]
        [SerializeField] private RecipeIngredient[] _ingredients = Array.Empty<RecipeIngredient>();

        [Header("Result")]
        [SerializeField] private ItemDefinition _resultItem;
        [SerializeField] private int _resultQuantity = 1;

        [Header("Crafting")]
        [SerializeField] private float _craftTimeSeconds = 1f;
        [SerializeField] private CraftingStationType _requiredStation = CraftingStationType.None;

        [Header("Unlock")]
        [Tooltip("If true, this recipe is craftable from the start without needing to be unlocked via CraftingManager.UnlockRecipe.")]
        [SerializeField] private bool _knownByDefault = true;
        [Tooltip("Minimum player level required before the recipe can be unlocked/crafted.")]
        [SerializeField] private int _minimumPlayerLevel = 1;

        public string RecipeId => _recipeId;
        public string DisplayName => _displayName;
        public string Description => _description;

        public RecipeIngredient[] Ingredients => _ingredients;

        public ItemDefinition ResultItem => _resultItem;
        public int ResultQuantity => Mathf.Max(1, _resultQuantity);

        public float CraftTimeSeconds => Mathf.Max(0f, _craftTimeSeconds);
        public CraftingStationType RequiredStation => _requiredStation;
        public bool RequiresCraftingStation => _requiredStation != CraftingStationType.None;

        public bool KnownByDefault => _knownByDefault;
        public int MinimumPlayerLevel => _minimumPlayerLevel;
    }
}
