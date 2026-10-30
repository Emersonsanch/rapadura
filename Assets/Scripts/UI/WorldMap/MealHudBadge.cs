using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Cooking;
using Rapadura.UI.HUD;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// The active meal (<see cref="MealService"/>) as a small dish icon + "Cuscuz · 2" pill on the HUD: under the
    /// hero's name on the world map (<c>wm-hero-block</c>) and under the vitals bars in the arena
    /// (<c>hud-vitals-panel</c>). Hidden with no meal. Self-bootstrapping, no scene or UXML edit: added to the
    /// <see cref="WorldMapHudController"/> / <see cref="HudController"/> GameObjects after every scene load; the
    /// element is (re)injected whenever the HUD document rebuilds its tree. Styled by
    /// <c>Resources/UI/VillageKitchen.uss</c>.
    /// </summary>
    [DisallowMultipleComponent]
    public class MealHudBadge : MonoBehaviour
    {
        public const string BadgeName = "meal-hud-badge";
        public const string MapAnchorName = "wm-hero-block";
        public const string ArenaAnchorName = "hud-vitals-panel";
        private const float RefreshInterval = 0.5f;

        private UIDocument _document;
        private VisualElement _badge;
        private VisualElement _icon;
        private Label _iconLabel;
        private Label _label;
        private float _nextRefresh;
        private bool _dirty = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachToHuds();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachToHuds();

        private static void AttachToHuds()
        {
            Attach(FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include));
            Attach(FindFirstObjectByType<HudController>(FindObjectsInactive.Include));
        }

        private static void Attach(Component hud)
        {
            if (hud != null && hud.GetComponent<UIDocument>() != null && hud.GetComponent<MealHudBadge>() == null)
            {
                hud.gameObject.AddComponent<MealHudBadge>();
            }
        }

        /// <summary>A round dish icon (<c>meal-icon</c> + <c>meal-icon--&lt;dish&gt;</c>) with its letter; null recipe = locked "?".</summary>
        public static VisualElement BuildIcon(MealRecipeSpec recipe, LocalizationManager localization)
        {
            var icon = new VisualElement { pickingMode = PickingMode.Ignore };
            icon.AddToClassList("meal-icon");
            icon.AddToClassList(recipe != null ? KitchenPresenter.IconClass(recipe.RecipeId) : "meal-icon--locked");
            var letter = new Label(recipe != null ? KitchenPresenter.IconGlyph(recipe, localization) : "?") { pickingMode = PickingMode.Ignore };
            letter.AddToClassList("meal-icon-label");
            icon.Add(letter);
            return icon;
        }

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<MealChangedEvent>(OnMealChanged);
            _dirty = true;
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<MealChangedEvent>(OnMealChanged);
            _badge?.RemoveFromHierarchy();
            _badge = null;
        }

        private void OnMealChanged(MealChangedEvent evt) => _dirty = true;

        private void Update()
        {
            if (!_dirty && Time.unscaledTime < _nextRefresh)
            {
                return;
            }

            _nextRefresh = Time.unscaledTime + RefreshInterval;
            _dirty = false;
            Refresh();
        }

        private void Refresh()
        {
            if (!EnsureBadge())
            {
                return;
            }

            LocalizationManager localization = ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;
            ActiveMeal meal = MealService.ResolveShared().Active;
            string text = KitchenPresenter.HudBadgeText(meal, localization);
            if (text == null)
            {
                _badge.style.display = DisplayStyle.None;
                return;
            }

            MealRecipeSpec recipe = MealCatalog.Get(meal.recipeId);
            _badge.style.display = DisplayStyle.Flex;
            if (_label.text != text)
            {
                _label.text = text;
            }

            string iconClass = KitchenPresenter.IconClass(recipe.RecipeId);
            if (!_icon.ClassListContains(iconClass))
            {
                _icon.ClearClassList();
                _icon.AddToClassList("meal-icon");
                _icon.AddToClassList(iconClass);
            }

            _iconLabel.text = KitchenPresenter.IconGlyph(recipe, localization);
        }

        private bool EnsureBadge()
        {
            if (_document == null || _document.rootVisualElement == null)
            {
                return false;
            }

            VisualElement anchor = _document.rootVisualElement.Q<VisualElement>(MapAnchorName)
                                   ?? _document.rootVisualElement.Q<VisualElement>(ArenaAnchorName);
            if (anchor == null)
            {
                return false;
            }

            if (_badge != null && _badge.parent == anchor)
            {
                return true;
            }

            _badge?.RemoveFromHierarchy();
            _badge = new VisualElement { name = BadgeName, pickingMode = PickingMode.Ignore };
            _badge.AddToClassList("meal-badge");
            var sheet = Resources.Load<StyleSheet>(VillageKitchenController.StyleResourcePath);
            if (sheet != null)
            {
                _badge.styleSheets.Add(sheet);
            }

            _icon = BuildIcon(null, null);
            _iconLabel = _icon.Q<Label>();
            _badge.Add(_icon);
            _label = new Label { pickingMode = PickingMode.Ignore };
            _label.AddToClassList("meal-badge-label");
            _badge.Add(_label);
            _badge.style.display = DisplayStyle.None;
            anchor.Add(_badge);
            return true;
        }
    }
}
