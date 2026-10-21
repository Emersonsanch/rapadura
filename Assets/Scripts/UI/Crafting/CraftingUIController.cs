using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Crafting
{
    /// <summary>
    /// Drives the Crafting window (CraftingView.uxml/.uss), following the same
    /// single-<see cref="UIDocument"/>, <c>Q&lt;T&gt;()</c>-resolved-once pattern as
    /// <c>ShopUIController</c>. Lists every known recipe from <see cref="RecipeDatabase"/> on the
    /// left; the selected recipe's ingredients (have/need from the player's
    /// <see cref="InventoryManager"/>), result and Craft button on the right. All rules come from
    /// <see cref="CraftingRecipePresenter"/>/<see cref="CraftingManager"/> — this class never decides
    /// craftability itself, it only forwards the click to <see cref="CraftingManager.TryCraft"/>.
    ///
    /// Opened explicitly via <see cref="Open"/> (e.g. a crafting-station trigger or a menu button
    /// UnityEvent); there is no CraftingManager "opened" event to listen for.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class CraftingUIController : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        private const long FeedbackDurationMs = 2000;

        private static readonly Color SuccessFlashColor = new Color(0.35f, 0.8f, 0.4f, 0.9f);
        private static readonly Color FailureFlashColor = new Color(0.9f, 0.3f, 0.3f, 0.9f);

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _closeButton;
        private VisualElement _recipeListContainer;
        private Label _recipeNameLabel;
        private Label _recipeDescriptionLabel;
        private VisualElement _ingredientListContainer;
        private Label _resultLabel;
        private Label _blockReasonLabel;
        private Button _craftButton;
        private Label _feedbackLabel;
        private Label _emptyLabel;

        private CraftingManager _craftingManager;
        private InventoryManager _inventory;
        private CraftingStationType _nearbyStation = CraftingStationType.None;
        private RecipeDefinition _selectedRecipe;
        private bool _isOpen;
        private IVisualElementScheduledItem _feedbackClearItem;

        private readonly List<RecipeDefinition> _recipes = new List<RecipeDefinition>();

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            Close();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("crafting-root");
            _closeButton = rootVisualElement.Q<Button>("crafting-close-button");
            _recipeListContainer = rootVisualElement.Q<VisualElement>("crafting-recipe-list");
            _recipeNameLabel = rootVisualElement.Q<Label>("crafting-recipe-name");
            _recipeDescriptionLabel = rootVisualElement.Q<Label>("crafting-recipe-description");
            _ingredientListContainer = rootVisualElement.Q<VisualElement>("crafting-ingredient-list");
            _resultLabel = rootVisualElement.Q<Label>("crafting-result");
            _blockReasonLabel = rootVisualElement.Q<Label>("crafting-block-reason");
            _craftButton = rootVisualElement.Q<Button>("crafting-craft-button");
            _feedbackLabel = rootVisualElement.Q<Label>("crafting-feedback");
            _emptyLabel = rootVisualElement.Q<Label>("crafting-empty");
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Subscribe<RecipeUnlockedEvent>(OnRecipeUnlocked);

            if (_closeButton != null) _closeButton.clicked += Close;
            if (_craftButton != null) _craftButton.clicked += OnCraftClicked;
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
            EventBus.Unsubscribe<RecipeUnlockedEvent>(OnRecipeUnlocked);

            if (_closeButton != null) _closeButton.clicked -= Close;
            if (_craftButton != null) _craftButton.clicked -= OnCraftClicked;
        }

        /// <summary>Lets a bootstrap/test assign the manager directly instead of resolving it from the ServiceLocator.</summary>
        public void SetCraftingManager(CraftingManager manager)
        {
            _craftingManager = manager;
        }

        /// <summary>Opens the window for the Player-tagged object's inventory, crafting by hand (no station).</summary>
        public void Open()
        {
            Open(null, CraftingStationType.None);
        }

        /// <summary>
        /// Opens the window. <paramref name="inventory"/> defaults to the Player-tagged object's
        /// <see cref="InventoryManager"/>; <paramref name="nearbyStation"/> is the bancada the player
        /// is using (recipes needing another station show as blocked).
        /// </summary>
        public void Open(InventoryManager inventory, CraftingStationType nearbyStation)
        {
            // Lazy re-resolve for the same Awake-ordering reason as ShopUIController.OnShopOpened.
            if (_craftingManager == null)
            {
                ServiceLocator.TryGet(out _craftingManager);
            }

            _inventory = inventory != null ? inventory : ResolvePlayerInventory();
            _nearbyStation = nearbyStation;
            _isOpen = true;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            SetFeedback(string.Empty, false);
            RebuildRecipeList();
        }

        public void Close()
        {
            _isOpen = false;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            _recipeListContainer?.Clear();
            _ingredientListContainer?.Clear();
        }

        public void Toggle()
        {
            if (_isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (_isOpen)
            {
                RefreshRecipeButtons();
                RefreshDetails();
            }
        }

        private void OnRecipeUnlocked(RecipeUnlockedEvent evt)
        {
            if (_isOpen)
            {
                RebuildRecipeList();
            }
        }

        private void RebuildRecipeList()
        {
            _recipes.Clear();

            foreach (RecipeDefinition recipe in RecipeDatabase.GetAll())
            {
                if (recipe != null && _craftingManager != null && _craftingManager.IsRecipeKnown(recipe))
                {
                    _recipes.Add(recipe);
                }
            }

            _recipes.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));

            if (_selectedRecipe == null || !_recipes.Contains(_selectedRecipe))
            {
                _selectedRecipe = _recipes.Count > 0 ? _recipes[0] : null;
            }

            if (_emptyLabel != null)
            {
                _emptyLabel.style.display = _recipes.Count == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_recipeListContainer != null)
            {
                _recipeListContainer.Clear();

                foreach (RecipeDefinition recipe in _recipes)
                {
                    RecipeDefinition captured = recipe;
                    var row = new Button(() => SelectRecipe(captured)) { text = recipe.DisplayName };
                    row.userData = recipe;
                    row.AddToClassList("crafting-recipe-button");
                    _recipeListContainer.Add(row);
                }
            }

            RefreshRecipeButtons();
            RefreshDetails();
        }

        private void SelectRecipe(RecipeDefinition recipe)
        {
            _selectedRecipe = recipe;
            SetFeedback(string.Empty, false);
            RefreshRecipeButtons();
            RefreshDetails();
        }

        private void RefreshRecipeButtons()
        {
            if (_recipeListContainer == null)
            {
                return;
            }

            foreach (VisualElement child in _recipeListContainer.Children())
            {
                if (child.userData is not RecipeDefinition recipe)
                {
                    continue;
                }

                child.EnableInClassList("crafting-recipe-button--selected", recipe == _selectedRecipe);
                child.EnableInClassList("crafting-recipe-button--craftable",
                    CraftingRecipePresenter.IsCraftable(_craftingManager, recipe, _inventory, _nearbyStation));
            }
        }

        private void RefreshDetails()
        {
            RecipeDefinition recipe = _selectedRecipe;

            if (_recipeNameLabel != null) _recipeNameLabel.text = recipe != null ? recipe.DisplayName : string.Empty;
            if (_recipeDescriptionLabel != null) _recipeDescriptionLabel.text = recipe != null ? recipe.Description : string.Empty;
            if (_resultLabel != null) _resultLabel.text = CraftingRecipePresenter.FormatResult(recipe);

            if (_ingredientListContainer != null)
            {
                _ingredientListContainer.Clear();

                foreach (IngredientStatus status in CraftingRecipePresenter.GetIngredientStatuses(recipe, _inventory))
                {
                    var row = new VisualElement();
                    row.AddToClassList("crafting-ingredient-row");
                    row.AddToClassList(status.IsSatisfied ? "crafting-ingredient-row--ok" : "crafting-ingredient-row--missing");

                    var nameLabel = new Label(status.DisplayName);
                    nameLabel.AddToClassList("crafting-ingredient-name");
                    row.Add(nameLabel);

                    var countLabel = new Label(CraftingRecipePresenter.FormatHaveNeed(status.Have, status.Need));
                    countLabel.AddToClassList("crafting-ingredient-count");
                    row.Add(countLabel);

                    _ingredientListContainer.Add(row);
                }
            }

            string blockReason = recipe != null
                ? CraftingRecipePresenter.GetCraftBlockReason(_craftingManager, recipe, _inventory, _nearbyStation)
                : string.Empty;

            if (_blockReasonLabel != null)
            {
                _blockReasonLabel.text = blockReason ?? string.Empty;
            }

            _craftButton?.SetEnabled(recipe != null && blockReason == null);
        }

        private void OnCraftClicked()
        {
            if (_craftingManager == null || _selectedRecipe == null)
            {
                return;
            }

            // TryCraft raises InventoryChangedEvent (via InventoryManager) which already refreshes the
            // counts; RefreshDetails below covers inventories that don't publish it.
            bool crafted = _craftingManager.TryCraft(_selectedRecipe, _inventory, _nearbyStation);

            if (crafted)
            {
                SetFeedback(CraftingRecipePresenter.FormatSuccessMessage(_selectedRecipe), true);
                UiFeedbackUtility.Pulse(_craftButton, SuccessFlashColor);
            }
            else
            {
                SetFeedback("Não foi possível criar (sem espaço no inventário?)", false);
                UiFeedbackUtility.Pulse(_craftButton, FailureFlashColor);
            }

            RefreshRecipeButtons();
            RefreshDetails();
        }

        private void SetFeedback(string text, bool success)
        {
            if (_feedbackLabel == null)
            {
                return;
            }

            _feedbackLabel.text = text;
            _feedbackLabel.EnableInClassList("crafting-feedback--success", success && !string.IsNullOrEmpty(text));
            _feedbackLabel.EnableInClassList("crafting-feedback--error", !success && !string.IsNullOrEmpty(text));

            _feedbackClearItem?.Pause();

            if (!string.IsNullOrEmpty(text))
            {
                _feedbackClearItem = _feedbackLabel.schedule.Execute(() => SetFeedback(string.Empty, false));
                _feedbackClearItem.ExecuteLater(FeedbackDurationMs);
            }
        }

        private static InventoryManager ResolvePlayerInventory()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            return player != null ? player.GetComponent<InventoryManager>() : null;
        }
    }
}
