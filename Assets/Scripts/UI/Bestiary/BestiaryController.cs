using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Common;
using Rapadura.UI.Menus;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Rapadura.UI.Bestiary
{
    /// <summary>
    /// "Caderno da Vó Edna", the bestiary window (Resources/UI/BestiaryView.uxml/.uss): a page per enemy
    /// type and boss of <see cref="BestiaryCatalog"/>, grouped by region, unlocked on the first defeat
    /// (<see cref="BestiaryProgress"/>). Locked pages are a dark silhouette with "???"; unlocked ones show the
    /// name, region, kill count, health/hit, Vó Edna's lore, tips and drops derived from the enemy data, and
    /// a one-liner from the cousin being played (texts in <see cref="BestiaryTexts"/>, built by
    /// <see cref="BestiaryPresenter"/>).
    /// <list type="bullet">
    /// <item>Opened from the pause menu's "Caderno" (encounter and world map, <see cref="PauseMenuController"/>)
    /// and from a "Caderno" button this controller injects into the world map HUD's <c>wm-button-cluster</c>
    /// (before "Menu"); on the map it also joins the HUD's screens (<see cref="WorldMapHudController.RegisterScreen"/>),
    /// so it closes with the HUD and Esc / gamepad Start backs out of it.</item>
    /// <item>Modal: holds a <see cref="GameplayPause"/> while up, released one frame after closing so the
    /// closing Esc cannot also open the pause menu. Esc / gamepad East / "Fechar" close it.</item>
    /// <item>Self-bootstrapping like <c>StoryPanelController</c>: one DontDestroyOnLoad host created after the
    /// first scene loads (no scene edit), borrowing the PanelSettings of another UIDocument.</item>
    /// </list>
    /// </summary>
    public class BestiaryController : MonoBehaviour
    {
        /// <summary>Above the map HUD (5) and windows (10-30); below the tutorial bubble (40), story (45), death screen (50) and toasts (60).</summary>
        public const float SortingOrder = 32f;

        public const string ViewResourcePath = "UI/BestiaryView";
        /// <summary>Name of the button injected into the world map HUD.</summary>
        public const string MapButtonName = "wm-bestiary-button";
        public const string MapScreenId = "bestiary";
        public const string ListItemClass = "bestiary-list-item";
        public const string ListItemLockedClass = "bestiary-list-item--locked";
        public const string ListItemBossClass = "bestiary-list-item--boss";
        public const string ListItemSelectedClass = "bestiary-list-item--selected";
        public const string RegionHeaderClass = "bestiary-region-header";
        public const string LineClass = "bestiary-line";
        public const string PortraitLockedClass = "bestiary-portrait--locked";
        public const string PortraitBossClass = "bestiary-portrait--boss";

        private const float HudScanInterval = 0.25f;
        /// <summary>Real seconds between two lookups of a missing map HUD (Main.unity has none).</summary>
        private const float HudLookupInterval = 2f;

        private static BestiaryController _instance;

        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private readonly List<(string Id, Button Button)> _listButtons = new List<(string, Button)>();
        private UIDocument _uiDocument;
        private VisualTreeAsset _viewAsset;
        private VisualElement _root;
        private ScrollView _list;
        private ScrollView _page;
        private Label _progressLabel;
        private Button _closeButton;
        private VisualElement _portrait;
        private Label _portraitGlyph;
        private Label _nameLabel;
        private Label _metaLabel;
        private Label _killsLabel;
        private Label _statsLabel;
        private Label _loreLabel;
        private VisualElement _tipsSection;
        private VisualElement _tipsLines;
        private VisualElement _dropsLines;
        private VisualElement _comment;
        private Label _commentHeader;
        private Label _commentText;

        private bool _open;
        private string _selectedId;
        private int _releasePauseAtFrame = -1;
        private LocalizationManager _localization;
        /// <summary>"Páginas | Amuletos" tabs, the charms view and the "Bicho destaque" banner (hunting features).</summary>
        private Rapadura.UI.Hunting.BestiaryCharmsTab _charmsTab;

        private WorldMapHudController _hud;
        private float _nextHudLookup;
        private Button _mapButton;
        private float _nextHudScan;

        /// <summary>True while the notebook is on screen.</summary>
        public static bool IsOpenShared => _instance != null && _instance._open;

        /// <summary>Whether <see cref="OpenShared"/> can show the notebook (the host exists and is enabled).</summary>
        public static bool CanOpen => _instance != null && _instance.isActiveAndEnabled;

        /// <summary>The page shown right now (null when closed).</summary>
        public static string SelectedIdShared => _instance != null && _instance._open ? _instance._selectedId : null;

        /// <summary>Opens the notebook (e.g. from the pause menu). False if it can't render yet.</summary>
        public static bool OpenShared() => _instance != null && _instance.Open();

        public static void CloseShared()
        {
            if (_instance != null)
            {
                _instance.Close();
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null || FindFirstObjectByType<BestiaryController>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(BestiaryController));
            host.AddComponent<UIDocument>();
            host.AddComponent<BestiaryController>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                _uiDocument = gameObject.AddComponent<UIDocument>();
            }

            _uiDocument.sortingOrder = SortingOrder;
            // Self-bootstrapped document: outside UiClickSoundBinder's per-scene scans.
            Rapadura.Audio.UiClickSoundBinder.Register(_uiDocument);
        }

        private void OnEnable()
        {
            _root = null;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus.Subscribe<BestiaryEntryUnlockedEvent>(OnEntryUnlocked);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus.Unsubscribe<BestiaryEntryUnlockedEvent>(OnEntryUnlocked);
            _open = false;
            SetRootVisible(false);
            GameplayPause.Release(this);
            RemoveMapButton();
            _localizer.Detach();
            _root = null;
        }

        private void OnDestroy()
        {
            GameplayPause.Release(this);
            Rapadura.Audio.UiClickSoundBinder.Unregister(_uiDocument);
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // The notebook never spans a scene swap; the old map HUD (and its button) is gone too.
            if (_open)
            {
                _open = false;
                SetRootVisible(false);
            }

            GameplayPause.Release(this);
            _releasePauseAtFrame = -1;
            RemoveMapButton();
            _hud = null;
            _nextHudLookup = 0f;
            _nextHudScan = 0f;
        }

        private void Update()
        {
            if (_releasePauseAtFrame >= 0 && Time.frameCount >= _releasePauseAtFrame)
            {
                _releasePauseAtFrame = -1;
                GameplayPause.Release(this);
            }

            if (_open && BackPressedThisFrame())
            {
                Close();
            }

            if (Time.unscaledTime >= _nextHudScan)
            {
                _nextHudScan = Time.unscaledTime + HudScanInterval;
                RefreshMapButton();
            }
        }

        private static bool BackPressedThisFrame()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonEast.wasPressedThisFrame;
        }

        // ------------------------------------------------------------------
        // Open / close
        // ------------------------------------------------------------------

        public bool IsOpen => _open;

        public bool Open()
        {
            if (_open)
            {
                return true;
            }

            if (!EnsureBound())
            {
                return false;
            }

            _open = true;
            _releasePauseAtFrame = -1;
            GameplayPause.Acquire(this);
            if (_selectedId == null || !BestiaryCatalog.Contains(_selectedId))
            {
                _selectedId = DefaultSelection();
            }

            Render();
            SetRootVisible(true);
            UiAccessibilityApplier.RequestRefresh();
            FocusSelected();
            return true;
        }

        public void Close()
        {
            if (!_open)
            {
                return;
            }

            _open = false;
            SetRootVisible(false);
            // A frame later: the Esc that closed this must not also reach the HUD's "open the pause menu".
            _releasePauseAtFrame = Time.frameCount + 1;
        }

        private static BestiaryProgress Progress => ServiceLocator.TryGet(out BestiaryProgress progress) ? progress : null;

        private static CharacterId? Hero => ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

        /// <summary>The first unlocked page, else the first page.</summary>
        public static string DefaultSelection(IReadOnlyList<BestiaryEntry> entries, System.Func<string, bool> isUnlocked)
        {
            if (entries == null || entries.Count == 0)
            {
                return null;
            }

            foreach (BestiaryEntry entry in entries)
            {
                if (isUnlocked != null && isUnlocked(entry.Id))
                {
                    return entry.Id;
                }
            }

            return entries[0].Id;
        }

        private static string DefaultSelection()
        {
            BestiaryProgress progress = Progress;
            return DefaultSelection(BestiaryCatalog.Entries, id => progress != null && progress.IsUnlocked(id));
        }

        private void OnEntryUnlocked(BestiaryEntryUnlockedEvent evt)
        {
            if (_open)
            {
                Render();
            }
        }

        // ------------------------------------------------------------------
        // Binding
        // ------------------------------------------------------------------

        /// <summary>Gives the document a panel (borrowed PanelSettings) and clones the view into it once. False until it can render.</summary>
        private bool EnsureBound()
        {
            if (_uiDocument.panelSettings == null)
            {
                PanelSettings donor = FindPanelSettings();
                if (donor == null)
                {
                    return false;
                }

                _uiDocument.panelSettings = donor;
                _uiDocument.sortingOrder = SortingOrder;
            }

            VisualElement documentRoot = _uiDocument.rootVisualElement;
            if (documentRoot == null)
            {
                return false;
            }

            if (_root != null && _root.parent != null)
            {
                return true;
            }

            if (_viewAsset == null)
            {
                _viewAsset = Resources.Load<VisualTreeAsset>(ViewResourcePath);
                if (_viewAsset == null)
                {
                    Debug.LogWarning($"[BestiaryController] Missing Resources/{ViewResourcePath}.uxml; bestiary disabled.");
                    enabled = false;
                    return false;
                }
            }

            documentRoot.pickingMode = PickingMode.Ignore;
            _root = documentRoot.Q<VisualElement>("bestiary-root");
            if (_root == null)
            {
                _viewAsset.CloneTree(documentRoot);
                _root = documentRoot.Q<VisualElement>("bestiary-root");
            }

            if (_root == null)
            {
                return false;
            }

            // CloneTree wraps the view in a TemplateContainer: it must not catch input while hidden.
            for (VisualElement element = _root.parent; element != null && element != documentRoot; element = element.parent)
            {
                element.pickingMode = PickingMode.Ignore;
            }

            _list = _root.Q<ScrollView>("bestiary-list");
            _page = _root.Q<ScrollView>("bestiary-page");
            _progressLabel = _root.Q<Label>("bestiary-progress");
            _closeButton = _root.Q<Button>("bestiary-close");
            _portrait = _root.Q<VisualElement>("bestiary-portrait");
            _portraitGlyph = _root.Q<Label>("bestiary-portrait-glyph");
            _nameLabel = _root.Q<Label>("bestiary-name");
            _metaLabel = _root.Q<Label>("bestiary-meta");
            _killsLabel = _root.Q<Label>("bestiary-kills");
            _statsLabel = _root.Q<Label>("bestiary-stats");
            _loreLabel = _root.Q<Label>("bestiary-lore");
            _tipsSection = _root.Q<VisualElement>("bestiary-tips-section");
            _tipsLines = _root.Q<VisualElement>("bestiary-tips");
            _dropsLines = _root.Q<VisualElement>("bestiary-drops");
            _comment = _root.Q<VisualElement>("bestiary-comment");
            _commentHeader = _root.Q<Label>("bestiary-comment-header");
            _commentText = _root.Q<Label>("bestiary-comment-text");
            _listButtons.Clear();

            if (_closeButton != null)
            {
                _closeButton.clicked -= Close;
                _closeButton.clicked += Close;
            }

            _root.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel);
            _root.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel);

            _charmsTab = Rapadura.UI.Hunting.BestiaryCharmsTab.Attach(_root, _charmsTab);
            _localizer.Attach(_root, () => { if (_open) Render(); }, BestiaryTexts.Get);
            SetRootVisible(false);
            return true;
        }

        private PanelSettings FindPanelSettings()
        {
            foreach (UIDocument document in FindObjectsByType<UIDocument>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (document != _uiDocument && document.panelSettings != null)
                {
                    return document.panelSettings;
                }
            }

            return null;
        }

        private void OnNavigationCancel(NavigationCancelEvent evt)
        {
            if (!_open)
            {
                return;
            }

            evt.StopPropagation();
            Close();
        }

        // ------------------------------------------------------------------
        // Rendering
        // ------------------------------------------------------------------

        private void Render()
        {
            if (_localization == null && ServiceLocator.TryGet(out _localization))
            {
                _localizer.SetManager(_localization);
            }
            else
            {
                _localizer.Refresh();
            }

            BestiaryProgress progress = Progress;
            IReadOnlyList<BestiaryEntry> entries = BestiaryCatalog.Entries;
            int unlocked = progress != null ? progress.UnlockedCount : 0;

            if (_progressLabel != null)
            {
                _progressLabel.text = BestiaryPresenter.ProgressText(_localization, unlocked, entries.Count);
            }

            RenderList(entries, progress);
            RenderPage(BestiaryCatalog.Get(_selectedId), progress);
            _charmsTab?.Render(_localization);
        }

        private void RenderList(IReadOnlyList<BestiaryEntry> entries, BestiaryProgress progress)
        {
            if (_list == null)
            {
                return;
            }

            _list.Clear();
            _listButtons.Clear();
            BiomeType? currentRegion = null;
            bool first = true;

            foreach (BestiaryEntry entry in entries)
            {
                if (first || entry.Region != currentRegion)
                {
                    first = false;
                    currentRegion = entry.Region;
                    var header = new Label(BestiaryPresenter.RegionName(_localization, entry));
                    header.AddToClassList(RegionHeaderClass);
                    _list.Add(header);
                }

                bool isUnlocked = progress != null && progress.IsUnlocked(entry.Id);
                string id = entry.Id;
                var button = new Button(() => Select(id))
                {
                    name = "bestiary-item-" + id,
                    text = BestiaryPresenter.ListLabel(_localization, entry, isUnlocked),
                    focusable = true,
                };
                button.AddToClassList("rap-button");
                button.AddToClassList(ListItemClass);
                button.EnableInClassList(ListItemLockedClass, !isUnlocked);
                button.EnableInClassList(ListItemBossClass, entry.IsBoss);
                button.EnableInClassList(ListItemSelectedClass, id == _selectedId);
                _list.Add(button);
                _listButtons.Add((id, button));
            }
        }

        private void Select(string id)
        {
            if (id == _selectedId)
            {
                return;
            }

            _selectedId = id;
            foreach ((string itemId, Button button) in _listButtons)
            {
                button.EnableInClassList(ListItemSelectedClass, itemId == _selectedId);
            }

            RenderPage(BestiaryCatalog.Get(_selectedId), Progress);
            if (_page != null)
            {
                _page.scrollOffset = Vector2.zero;
            }
        }

        private void FocusSelected()
        {
            foreach ((string id, Button button) in _listButtons)
            {
                if (id == _selectedId)
                {
                    button.Focus();
                    _list?.ScrollTo(button);
                    return;
                }
            }

            _closeButton?.Focus();
        }

        private void RenderPage(BestiaryEntry entry, BestiaryProgress progress)
        {
            bool unlocked = entry != null && progress != null && progress.IsUnlocked(entry.Id);
            int kills = entry != null && progress != null ? progress.GetKills(entry.Id) : 0;
            CharacterId? hero = Hero;
            BestiaryPageView view = BestiaryPresenter.BuildPage(_localization, entry, unlocked, kills, hero);

            if (_portrait != null)
            {
                _portrait.EnableInClassList(PortraitLockedClass, view.Locked);
                _portrait.EnableInClassList(PortraitBossClass, view.IsBoss);
            }

            if (_portraitGlyph != null)
            {
                _portraitGlyph.text = view.Locked || string.IsNullOrEmpty(view.Name) ? "?" : view.Name.Substring(0, 1).ToUpperInvariant();
            }

            SetText(_nameLabel, view.Name);
            SetText(_metaLabel, view.Meta);
            SetText(_killsLabel, view.Kills);
            SetText(_statsLabel, view.Stats);
            SetText(_loreLabel, view.Lore);

            FillLines(_tipsLines, view.Tips);
            FillLines(_dropsLines, view.Drops);
            SetDisplayed(_tipsSection, !view.Locked);
            SetDisplayed(_dropsLines?.parent, !view.Locked);
            SetDisplayed(_comment, !view.Locked);

            SetText(_commentHeader, view.CommentHeader);
            SetText(_commentText, view.Comment);
            if (_comment != null)
            {
                StyleColor accent = view.Hero.HasValue ? new StyleColor(CharacterRegistry.Get(view.Hero.Value).AccentColor) : new StyleColor(StyleKeyword.Null);
                _comment.style.borderTopColor = _comment.style.borderBottomColor = _comment.style.borderLeftColor = _comment.style.borderRightColor = accent;
            }
        }

        private static void FillLines(VisualElement container, List<string> lines)
        {
            if (container == null)
            {
                return;
            }

            container.Clear();
            foreach (string line in lines)
            {
                var label = new Label("• " + line);
                label.AddToClassList(LineClass);
                container.Add(label);
            }
        }

        private static void SetText(Label label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
                label.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private static void SetDisplayed(VisualElement element, bool displayed)
        {
            if (element != null)
            {
                element.style.display = displayed ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void SetRootVisible(bool visible)
        {
            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
                _root.pickingMode = visible ? PickingMode.Position : PickingMode.Ignore;
            }
        }

        // ------------------------------------------------------------------
        // World map HUD button
        // ------------------------------------------------------------------

        /// <summary>Clicks on the HUD button: ignored while something else holds the pause (pause menu, another screen).</summary>
        public static bool CanToggleFromMap(bool notebookOpen, bool pausedByOthers) => notebookOpen || !pausedByOthers;

        private void RefreshMapButton()
        {
            if (_hud == null && Time.unscaledTime >= _nextHudLookup)
            {
                // Throttled: Main.unity has no map HUD, and a failed lookup every scan is wasteful.
                _nextHudLookup = Time.unscaledTime + HudLookupInterval;
                _hud = FindFirstObjectByType<WorldMapHudController>(FindObjectsInactive.Include);
                if (_hud != null)
                {
                    _hud.RegisterScreen(MapScreenId, () => _open, () => Open(), Close);
                }
            }

            if (_hud == null)
            {
                return;
            }

            UIDocument hudDocument = _hud.GetComponent<UIDocument>();
            VisualElement cluster = hudDocument != null && hudDocument.rootVisualElement != null
                ? hudDocument.rootVisualElement.Q<VisualElement>("wm-button-cluster")
                : null;
            if (cluster == null)
            {
                return;
            }

            if (_mapButton == null || _mapButton.parent != cluster)
            {
                // First time, or the HUD document rebuilt its tree (re-enable): (re)inject before "Menu".
                RemoveMapButton();
                _mapButton = new Button(OnMapButtonClicked) { name = MapButtonName, focusable = true };
                _mapButton.AddToClassList("wm-menu-button");
                VisualElement menu = cluster.Q<Button>("wm-menu-button");
                int index = menu != null ? cluster.IndexOf(menu) : -1;
                if (index >= 0)
                {
                    cluster.Insert(index, _mapButton);
                }
                else
                {
                    cluster.Add(_mapButton);
                }
            }

            string label = UiTexts.Get(ContentLocalization(), UiTexts.PauseBestiary);
            if (_mapButton.text != label)
            {
                _mapButton.text = label;
            }

            _mapButton.style.display = _hud.IsVisible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private static LocalizationManager ContentLocalization() => ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;

        private void OnMapButtonClicked()
        {
            if (!CanToggleFromMap(_open, GameplayPause.IsPausedByOthers(this)))
            {
                return;
            }

            if (_open)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void RemoveMapButton()
        {
            if (_mapButton != null)
            {
                _mapButton.RemoveFromHierarchy();
                _mapButton = null;
            }
        }
    }
}
