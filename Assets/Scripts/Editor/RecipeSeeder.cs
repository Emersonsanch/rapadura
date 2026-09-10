using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Items;

namespace Rapadura.Editor
{
    /// <summary>
    /// Seeds a handful of example <see cref="RecipeDefinition"/> assets under Resources/Recipes.
    /// CraftingManager (registered in GameManager, full TryCraft/CanCraft/UnlockRecipe API already
    /// implemented and tested in isolation) had zero recipes anywhere in the project to actually
    /// craft — same class of gap as the Skill Tree before SkillTreeSeeder. All ingredients/results
    /// reference base items already seeded under Resources/Items.
    /// </summary>
    public static class RecipeSeeder
    {
        private const string OutputFolder = "Assets/Resources/Recipes";

        private class RecipeSeed
        {
            public string Id;
            public string DisplayName;
            public string Description;
            public (string itemId, int quantity)[] Ingredients;
            public string ResultItemId;
            public int ResultQuantity;
        }

        [MenuItem("Rapadura/Seed Example Recipes")]
        public static void Seed()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Recipes");
            }

            var seeds = new List<RecipeSeed>
            {
                new RecipeSeed
                {
                    Id = "recipe_iron_ingot",
                    DisplayName = "Lingote de Ferro",
                    Description = "Funde minério de ferro em um lingote utilizável.",
                    Ingredients = new[] { ("item_iron_ore", 2), ("item_wood", 1) },
                    ResultItemId = "item_iron",
                    ResultQuantity = 1,
                },
                new RecipeSeed
                {
                    Id = "recipe_iron_sword",
                    DisplayName = "Espada de Ferro",
                    Description = "Forja uma espada de ferro a partir de lingotes e madeira.",
                    Ingredients = new[] { ("item_iron", 2), ("item_wood", 1) },
                    ResultItemId = "item_iron_sword",
                    ResultQuantity = 1,
                },
                new RecipeSeed
                {
                    Id = "recipe_health_potion",
                    DisplayName = "Poção de Vida",
                    Description = "Prepara uma poção de cura a partir de ervas colhidas.",
                    Ingredients = new[] { ("item_herb", 3) },
                    ResultItemId = "item_health_potion",
                    ResultQuantity = 1,
                },
            };

            int created = 0;
            foreach (RecipeSeed seed in seeds)
            {
                string path = $"{OutputFolder}/{seed.Id}.asset";
                RecipeDefinition recipe = AssetDatabase.LoadAssetAtPath<RecipeDefinition>(path);
                bool isNew = recipe == null;
                if (isNew)
                {
                    recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
                }

                var ingredients = new List<RecipeIngredient>();
                bool allItemsFound = true;
                foreach ((string itemId, int quantity) in seed.Ingredients)
                {
                    ItemDefinition item = ItemDatabase.GetById(itemId);
                    if (item == null)
                    {
                        Debug.LogWarning($"[RecipeSeeder] Ingredient '{itemId}' not found, skipping recipe '{seed.Id}'.");
                        allItemsFound = false;
                        break;
                    }
                    ingredients.Add(new RecipeIngredient { item = item, quantity = quantity });
                }

                ItemDefinition resultItem = ItemDatabase.GetById(seed.ResultItemId);
                if (!allItemsFound || resultItem == null)
                {
                    if (resultItem == null)
                    {
                        Debug.LogWarning($"[RecipeSeeder] Result item '{seed.ResultItemId}' not found, skipping recipe '{seed.Id}'.");
                    }
                    continue;
                }

                var so = new SerializedObject(recipe);
                so.FindProperty("_recipeId").stringValue = seed.Id;
                so.FindProperty("_displayName").stringValue = seed.DisplayName;
                so.FindProperty("_description").stringValue = seed.Description;
                so.FindProperty("_resultItem").objectReferenceValue = resultItem;
                so.FindProperty("_resultQuantity").intValue = seed.ResultQuantity;
                so.FindProperty("_craftTimeSeconds").floatValue = 1.5f;
                so.FindProperty("_knownByDefault").boolValue = true;
                so.FindProperty("_minimumPlayerLevel").intValue = 1;

                SerializedProperty ingredientsProp = so.FindProperty("_ingredients");
                ingredientsProp.arraySize = ingredients.Count;
                for (int i = 0; i < ingredients.Count; i++)
                {
                    SerializedProperty element = ingredientsProp.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("item").objectReferenceValue = ingredients[i].item;
                    element.FindPropertyRelative("quantity").intValue = ingredients[i].quantity;
                }
                so.ApplyModifiedProperties();

                if (isNew)
                {
                    AssetDatabase.CreateAsset(recipe, path);
                }
                else
                {
                    EditorUtility.SetDirty(recipe);
                }

                created++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[RecipeSeeder] Seeded {created} recipe(s) under {OutputFolder}.");
        }
    }
}
