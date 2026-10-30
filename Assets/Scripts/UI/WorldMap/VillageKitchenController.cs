using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Fogão a lenha" of the village hub (<see cref="VillageHubButton"/> → Serviços): Vó Edna's line for the cousin at
    /// the stove, the meal in their belly and one row per dish of <see cref="MealCatalog"/> (ingredients against the
    /// bag, "Cozinhar"). Built in code on the map HUD's document like the hub panel, under <c>wm-hud-root</c> with
    /// the Mochila overlay classes plus <c>Resources/UI/VillageKitchen.uss</c>; the hub registers it as one of the
    /// HUD's screens (one at a time, Esc closes it).
    /// <list type="bullet">
    /// <item>Works on the hub's live <see cref="CarriedInventorySession"/> bag, so cooked ingredients leave the
    /// carried inventory as soon as the window closes.</item>
    /// <item>On open, Vó Edna may teach the cousin their favourite (<see cref="MealService.TryTeachFavourite"/>).</item>
    /// <item>Rules (validation, consumption, one meal at a time) live in <see cref="CookingRules"/>; texts in
    /// <see cref="KitchenPresenter"/>.</item>
    /// </list>
    /// </summary>
    [DisallowMultipleComponent]
    public class VillageKitchenController : MonoBehaviour
    {
        public const string OverlayName = "wm-kitchen-overlay";
        public const string StyleResourcePath = "UI/VillageKitchen";
        public const string RecipeButtonPrefix = "wm-kitchen-cook-";

        private UIDocument _hudDocument;
        private VisualElement _overlay;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _grandmaLabel;
        private Label _activeLabel;
        private ScrollView _list;
        private Label _statusLabel;
        private Button _closeButton;
        private readonly List<Button> _cookButtons = new List<Button>();

        private InventoryManager _bag;
        private CharacterId? _hero;
        private MealRecipeSpec _taughtOnOpen;
        // Region Vó Edna remembers on this opening (KitchenPresenter.CameFromForVisit); visits counted per session.
        private BiomeType? _cameFromOnOpen;
        private static int s_visits;
        private bool _open;

        public bool IsOpen => _open;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        private void Awake()
        {
            _hudDocument = GetComponent<UIDocument>();
        }

        private void OnDisable()
        {
            Close();
            _overlay?.RemoveFromHierarchy();
            _overlay = null;
        }

        /// <summary>Opens the stove on <paramref name="bag"/> (the hub's live session inventory).</summary>
        public void Open(InventoryManager bag)
        {
            if (_open || bag == null || !EnsureOverlay())
            {
                return;
            }

            _bag = bag;
            _hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            MealService meals = MealService.ResolveShared();
            _taughtOnOpen = meals.TryTeachFavourite(_hero);
            StoryProgress story = ServiceLocator.TryGet(out StoryProgress progress) ? progress : null;
            _cameFromOnOpen = KitchenPresenter.CameFromForVisit(story != null ? StoryBeats.LastRegionCleared(story.HasSeen) : null, s_visits++);

            _open = true;
            _overlay.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);

            LocalizationManager localization = Localization;
            _statusLabel.text = _taughtOnOpen != null
                ? KitchenTexts.Format(localization, KitchenTexts.StatusLearned, KitchenPresenter.DishName(localization, _taughtOnOpen))
                : string.Empty;
            Rebuild();
        }

        public void Close()
        {
            if (_overlay != null)
            {
                _overlay.style.display = DisplayStyle.None;
            }

            _open = false;
            _bag = null;
            GameplayPause.Release(this);
        }

        private bool EnsureOverlay()
        {
            if (_hudDocument == null || _hudDocument.rootVisualElement == null)
            {
                return false;
            }

            VisualElement root = _hudDocument.rootVisualElement.Q<VisualElement>("wm-hud-root") ?? _hudDocument.rootVisualElement;
            if (_overlay != null && _overlay.parent == root)
            {
                return true;
            }

            _overlay?.RemoveFromHierarchy();

            _overlay = new VisualElement { name = OverlayName };
            _overlay.AddToClassList("wm-backpack-overlay");
            _overlay.style.display = DisplayStyle.None;
            var sheet = Resources.Load<StyleSheet>(StyleResourcePath);
            if (sheet != null)
            {
                _overlay.styleSheets.Add(sheet);
            }

            var panel = new VisualElement { name = "wm-kitchen-panel" };
            panel.AddToClassList("rap-panel");
            panel.AddToClassList("wm-backpack-panel");
            panel.AddToClassList("kitchen-panel");
            _overlay.Add(panel);

            _titleLabel = AddLabel(panel, "wm-backpack-title");
            _subtitleLabel = AddLabel(panel, "wm-backpack-hint");
            _grandmaLabel = AddLabel(panel, "kitchen-grandma");
            _activeLabel = AddLabel(panel, "kitchen-active");

            _list = new ScrollView(ScrollViewMode.Vertical) { name = "wm-kitchen-list" };
            _list.AddToClassList("kitchen-list");
            panel.Add(_list);

            _statusLabel = AddLabel(panel, "kitchen-status");

            _closeButton = new Button(Close) { name = "wm-kitchen-close-button", focusable = true };
            _closeButton.AddToClassList("rap-button");
            _closeButton.AddToClassList("rap-button--primary");
            _closeButton.AddToClassList("wm-backpack-close");
            panel.Add(_closeButton);

            root.Add(_overlay);
            return true;
        }

        private static Label AddLabel(VisualElement parent, string className)
        {
            var label = new Label();
            label.AddToClassList(className);
            parent.Add(label);
            return label;
        }

        private void Rebuild()
        {
            if (_overlay == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            MealService meals = MealService.ResolveShared();
            System.Func<string, int> count = _bag != null ? _bag.GetTotalCount : (System.Func<string, int>)(_ => 0);

            _titleLabel.text = KitchenTexts.Get(localization, KitchenTexts.Title);
            _subtitleLabel.text = KitchenTexts.Get(localization, KitchenTexts.Subtitle);
            _grandmaLabel.text = KitchenPresenter.GrandmaLine(_hero, _taughtOnOpen, localization, _cameFromOnOpen);
            _activeLabel.text = KitchenPresenter.ActiveMealLine(meals.Active, localization);
            _closeButton.text = KitchenTexts.Get(localization, KitchenTexts.Close);

            // Few dishes and rebuilt only on open / after cooking: simpler than diffing rows.
            _list.Clear();
            _cookButtons.Clear();
            foreach (MealRecipeSpec recipe in MealCatalog.Recipes)
            {
                KitchenRecipeRow row = KitchenPresenter.BuildRow(recipe, meals.IsKnown(recipe.RecipeId), count, meals.Active, _hero, localization);
                _list.Add(BuildRowElement(recipe, row, localization));
            }

            Button first = _cookButtons.Find(button => button.enabledSelf);
            (first ?? _closeButton).Focus();
        }

        private VisualElement BuildRowElement(MealRecipeSpec recipe, KitchenRecipeRow row, LocalizationManager localization)
        {
            var element = new VisualElement { name = "wm-kitchen-recipe-" + recipe.RecipeId };
            element.AddToClassList("kitchen-recipe");
            element.EnableInClassList("kitchen-recipe--locked", !row.Known);
            element.EnableInClassList("kitchen-recipe--active", row.IsActive);
            element.EnableInClassList("kitchen-recipe--favourite", row.Known && row.IsHeroFavourite);

            element.Add(MealHudBadge.BuildIcon(row.Known ? recipe : null, localization));

            var body = new VisualElement();
            body.AddToClassList("kitchen-recipe-body");
            element.Add(body);
            AddLine(body, row.Title, "kitchen-recipe-title");
            AddLine(body, row.Effect, "kitchen-recipe-line");
            AddLine(body, row.Ingredients, "kitchen-recipe-line");
            AddLine(body, row.Provisions, "kitchen-recipe-line");
            AddLine(body, row.Lasts, "kitchen-recipe-line", "kitchen-recipe-line--dim");
            AddLine(body, row.Favourite, "kitchen-recipe-line", "kitchen-recipe-line--favourite");
            AddLine(body, row.HowToLearn, "kitchen-recipe-line", "kitchen-recipe-line--dim");

            if (row.Known)
            {
                string recipeId = recipe.RecipeId;
                var cook = new Button(() => OnCookClicked(recipeId)) { name = RecipeButtonPrefix + recipeId, focusable = true, text = row.ButtonText };
                cook.AddToClassList("rap-button");
                cook.AddToClassList("kitchen-cook-button");
                cook.SetEnabled(row.CanCook);
                element.Add(cook);
                _cookButtons.Add(cook);
            }

            return element;
        }

        private static void AddLine(VisualElement parent, string text, params string[] classes)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var label = new Label(text);
            foreach (string cls in classes)
            {
                label.AddToClassList(cls);
            }

            parent.Add(label);
        }

        private void OnCookClicked(string recipeId)
        {
            if (!_open || _bag == null)
            {
                return;
            }

            MealService meals = MealService.ResolveShared();
            MealRecipeSpec previous = meals.ActiveRecipe;
            CookCheck check = meals.TryCook(recipeId, _bag, out ServeOutcome outcome);
            MealRecipeSpec recipe = MealCatalog.Get(recipeId);

            LocalizationManager localization = Localization;
            _statusLabel.text = KitchenPresenter.CookStatus(check, outcome, recipe, previous, CookingRules.IsFavourite(recipe, _hero), localization);
            _taughtOnOpen = null;
            Rebuild();
        }
    }
}
