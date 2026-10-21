using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.UI.Crafting;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="CraftingRecipePresenter"/> — the pure have/need and
    /// "why is Craft disabled" rules behind the Crafting screen. Items/recipes are built the same
    /// way as <c>CraftingManagerTests</c> (SerializedObject/reflection on private fields).
    /// </summary>
    public class CraftingRecipePresenterTests
    {
        private GameObject _owner;
        private InventoryManager _inventory;
        private CraftingManager _crafting;

        private ItemDefinition _wood;
        private ItemDefinition _stone;
        private ItemDefinition _axe;
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();

            _owner = new GameObject("CraftPresenterOwner");
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

            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
        }

        [Test]
        public void GetIngredientStatuses_ReportsHaveAndNeedPerIngredient()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 3), (_stone, 1) }, knownByDefault: true);
            _inventory.AddItem(_wood, 1);
            _inventory.AddItem(_stone, 5);

            List<IngredientStatus> statuses = CraftingRecipePresenter.GetIngredientStatuses(recipe, _inventory);

            Assert.AreEqual(2, statuses.Count);
            Assert.AreEqual(_wood, statuses[0].Item);
            Assert.AreEqual(1, statuses[0].Have);
            Assert.AreEqual(3, statuses[0].Need);
            Assert.IsFalse(statuses[0].IsSatisfied);
            Assert.AreEqual(5, statuses[1].Have);
            Assert.IsTrue(statuses[1].IsSatisfied);
            Assert.AreEqual(1, CraftingRecipePresenter.CountMissingIngredients(statuses));
        }

        [Test]
        public void GetIngredientStatuses_SkipsMalformedEntries_AndNullInventoryReportsZero()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 2), ((ItemDefinition)null, 4), (_stone, 0) }, knownByDefault: true);

            List<IngredientStatus> statuses = CraftingRecipePresenter.GetIngredientStatuses(recipe, null);

            Assert.AreEqual(1, statuses.Count);
            Assert.AreEqual(0, statuses[0].Have);
            Assert.AreEqual(2, statuses[0].Need);
            Assert.IsEmpty(CraftingRecipePresenter.GetIngredientStatuses(null, _inventory));
        }

        [Test]
        public void FormatHaveNeed_UsesSlashFormat()
        {
            Assert.AreEqual("1/2", CraftingRecipePresenter.FormatHaveNeed(1, 2));
            Assert.AreEqual("7/3", CraftingRecipePresenter.FormatHaveNeed(7, 3));
        }

        [Test]
        public void GetCraftBlockReason_MissingMaterials_DisablesCraft()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 2) }, knownByDefault: true);
            _inventory.AddItem(_wood, 1);

            Assert.AreEqual(CraftingRecipePresenter.MissingMaterialsReason,
                CraftingRecipePresenter.GetCraftBlockReason(_crafting, recipe, _inventory, CraftingStationType.None));
            Assert.IsFalse(CraftingRecipePresenter.IsCraftable(_crafting, recipe, _inventory, CraftingStationType.None));
        }

        [Test]
        public void GetCraftBlockReason_AllMaterials_EnablesCraft_AndAgreesWithCanCraft()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 2) }, knownByDefault: true);
            _inventory.AddItem(_wood, 2);

            Assert.IsNull(CraftingRecipePresenter.GetCraftBlockReason(_crafting, recipe, _inventory, CraftingStationType.None));
            Assert.IsTrue(_crafting.CanCraft(recipe, _inventory));
        }

        [Test]
        public void GetCraftBlockReason_UnknownRecipe_IsBlocked()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 1) }, knownByDefault: false);
            _inventory.AddItem(_wood, 5);

            Assert.AreEqual(CraftingRecipePresenter.UnknownRecipeReason,
                CraftingRecipePresenter.GetCraftBlockReason(_crafting, recipe, _inventory, CraftingStationType.None));
        }

        [Test]
        public void GetCraftBlockReason_WrongStation_NamesRequiredStation()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 1) }, knownByDefault: true, station: CraftingStationType.Forge);
            _inventory.AddItem(_wood, 5);

            Assert.AreEqual(CraftingRecipePresenter.FormatStationRequirement(CraftingStationType.Forge),
                CraftingRecipePresenter.GetCraftBlockReason(_crafting, recipe, _inventory, CraftingStationType.None));
            Assert.IsNull(CraftingRecipePresenter.GetCraftBlockReason(_crafting, recipe, _inventory, CraftingStationType.Forge));
        }

        [Test]
        public void GetCraftBlockReason_NullInventory_IsBlocked()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 1) }, knownByDefault: true);

            Assert.AreEqual(CraftingRecipePresenter.NoInventoryReason,
                CraftingRecipePresenter.GetCraftBlockReason(_crafting, recipe, null, CraftingStationType.None));
        }

        [Test]
        public void FormatSuccessMessage_IncludesResultNameAndQuantity()
        {
            RecipeDefinition recipe = CreateRecipe(new[] { (_wood, 1) }, knownByDefault: true, resultQuantity: 2);

            Assert.AreEqual("Criado: item_axe x2!", CraftingRecipePresenter.FormatSuccessMessage(recipe));
            Assert.AreEqual("Resultado: item_axe x2", CraftingRecipePresenter.FormatResult(recipe));
        }

        private ItemDefinition CreateItem(string itemId, int maxStack)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            var serialized = new SerializedObject(item);
            serialized.FindProperty("_itemId").stringValue = itemId;
            serialized.FindProperty("_displayName").stringValue = itemId;
            serialized.FindProperty("_type").enumValueIndex = (int)ItemType.Material;
            serialized.FindProperty("_maxStack").intValue = maxStack;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(item);
            return item;
        }

        private RecipeDefinition CreateRecipe(
            (ItemDefinition item, int quantity)[] ingredients,
            bool knownByDefault,
            CraftingStationType station = CraftingStationType.None,
            int resultQuantity = 1)
        {
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();

            var recipeIngredients = new RecipeIngredient[ingredients.Length];
            for (int i = 0; i < ingredients.Length; i++)
            {
                recipeIngredients[i] = new RecipeIngredient { item = ingredients[i].item, quantity = ingredients[i].quantity };
            }

            SetPrivateField(recipe, "_recipeId", "recipe_test_axe");
            SetPrivateField(recipe, "_displayName", "Test Axe");
            SetPrivateField(recipe, "_ingredients", recipeIngredients);
            SetPrivateField(recipe, "_resultItem", _axe);
            SetPrivateField(recipe, "_resultQuantity", resultQuantity);
            SetPrivateField(recipe, "_knownByDefault", knownByDefault);
            SetPrivateField(recipe, "_requiredStation", station);

            _created.Add(recipe);
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
