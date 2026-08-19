using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the Fase 5 "Crafting" backend: <see cref="CraftingManager"/> and
    /// <see cref="RecipeDefinition"/>. Items and recipes are ScriptableObjects with private
    /// [SerializeField] fields, populated via reflection the same way <c>WeaponSystemTests</c>
    /// populates <c>ItemDefinition</c>.
    /// </summary>
    public class CraftingManagerTests
    {
        private GameObject _owner;
        private InventoryManager _inventory;
        private CraftingManager _crafting;

        private ItemDefinition _wood;
        private ItemDefinition _stone;
        private ItemDefinition _axe;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _owner = new GameObject("CraftOwner");
            _inventory = _owner.AddComponent<InventoryManager>();
            _crafting = new CraftingManager();
            _crafting.Initialize();

            _wood = CreateItem("item_wood", 99);
            _stone = CreateItem("item_stone", 99);
            _axe = CreateItem("item_axe", 1);
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            _crafting.Shutdown();
            Object.DestroyImmediate(_owner);
            Object.DestroyImmediate(_wood);
            Object.DestroyImmediate(_stone);
            Object.DestroyImmediate(_axe);
        }

        [Test]
        public void TryCraft_WithAllIngredients_ConsumesIngredientsAndProducesResult()
        {
            RecipeDefinition recipe = CreateRecipe("recipe_axe", new[] { (_wood, 2), (_stone, 1) }, _axe, 1, knownByDefault: true);
            _inventory.AddItem(_wood, 2);
            _inventory.AddItem(_stone, 1);

            bool crafted = _crafting.TryCraft(recipe, _inventory);

            Assert.IsTrue(crafted);
            Assert.AreEqual(0, _inventory.GetTotalCount("item_wood"));
            Assert.AreEqual(0, _inventory.GetTotalCount("item_stone"));
            Assert.AreEqual(1, _inventory.GetTotalCount("item_axe"));
        }

        [Test]
        public void TryCraft_MissingIngredient_FailsAndConsumesNothing()
        {
            RecipeDefinition recipe = CreateRecipe("recipe_axe", new[] { (_wood, 2), (_stone, 1) }, _axe, 1, knownByDefault: true);
            _inventory.AddItem(_wood, 2);
            // Missing stone.

            bool crafted = _crafting.TryCraft(recipe, _inventory);

            Assert.IsFalse(crafted);
            Assert.AreEqual(2, _inventory.GetTotalCount("item_wood"));
            Assert.AreEqual(0, _inventory.GetTotalCount("item_axe"));
        }

        [Test]
        public void TryCraft_UnknownRecipe_Fails()
        {
            RecipeDefinition recipe = CreateRecipe("recipe_axe", new[] { (_wood, 2) }, _axe, 1, knownByDefault: false);
            _inventory.AddItem(_wood, 5);

            bool crafted = _crafting.TryCraft(recipe, _inventory);

            Assert.IsFalse(crafted);
            Assert.AreEqual(5, _inventory.GetTotalCount("item_wood"));
        }

        [Test]
        public void UnlockRecipe_MakesItKnownAndCraftable()
        {
            RecipeDefinition recipe = CreateRecipe("recipe_axe", new[] { (_wood, 2) }, _axe, 1, knownByDefault: false);
            _inventory.AddItem(_wood, 5);

            Assert.IsFalse(_crafting.IsRecipeKnown(recipe));
            bool unlocked = _crafting.UnlockRecipe(recipe, playerLevel: 5);
            Assert.IsTrue(unlocked);
            Assert.IsTrue(_crafting.IsRecipeKnown(recipe));

            bool crafted = _crafting.TryCraft(recipe, _inventory);
            Assert.IsTrue(crafted);
        }

        [Test]
        public void UnlockRecipe_BelowLevelRequirement_Fails()
        {
            RecipeDefinition recipe = CreateRecipe("recipe_axe", new[] { (_wood, 2) }, _axe, 1, knownByDefault: false, minLevel: 10);

            bool unlocked = _crafting.UnlockRecipe(recipe, playerLevel: 3);

            Assert.IsFalse(unlocked);
            Assert.IsFalse(_crafting.IsRecipeKnown(recipe));
        }

        [Test]
        public void CanCraft_RequiresMatchingCraftingStation()
        {
            RecipeDefinition recipe = CreateRecipe("recipe_axe", new[] { (_wood, 2) }, _axe, 1, knownByDefault: true, station: CraftingStationType.Forge);
            _inventory.AddItem(_wood, 5);

            bool canCraftWithoutStation = _crafting.CanCraft(recipe, _inventory, CraftingStationType.None);
            bool canCraftAtWrongStation = _crafting.CanCraft(recipe, _inventory, CraftingStationType.Campfire);
            bool canCraftAtCorrectStation = _crafting.CanCraft(recipe, _inventory, CraftingStationType.Forge);

            Assert.IsFalse(canCraftWithoutStation);
            Assert.IsFalse(canCraftAtWrongStation);
            Assert.IsTrue(canCraftAtCorrectStation);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static ItemDefinition CreateItem(string itemId, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private static RecipeDefinition CreateRecipe(
            string recipeId,
            (ItemDefinition item, int quantity)[] ingredients,
            ItemDefinition result,
            int resultQuantity,
            bool knownByDefault,
            int minLevel = 1,
            CraftingStationType station = CraftingStationType.None)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();

            var recipeIngredients = new RecipeIngredient[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                recipeIngredients[i] = new RecipeIngredient { item = ingredients[i].item, quantity = ingredients[i].quantity };
            }

            SetPrivateField(recipe, "_recipeId", recipeId);
            SetPrivateField(recipe, "_displayName", recipeId);
            SetPrivateField(recipe, "_ingredients", recipeIngredients);
            SetPrivateField(recipe, "_resultItem", result);
            SetPrivateField(recipe, "_resultQuantity", resultQuantity);
            SetPrivateField(recipe, "_knownByDefault", knownByDefault);
            SetPrivateField(recipe, "_minimumPlayerLevel", minLevel);
            SetPrivateField(recipe, "_requiredStation", station);

            return recipe;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"expected private field '{fieldName}' on {target.GetType().Name}");
            field.SetValue(target, value);
        }
    }
}
