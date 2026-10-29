using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Achievements;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using Rapadura.UI.Menus;
using Rapadura.UI.Quests;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// In-game UI of WorldMap.unity once a hero is in play (WorldMapHudView.uxml/.uss):
    /// <list type="bullet">
    /// <item>Top bar: hero name + level, current region, "Vitórias: 2/3 para enfrentar o chefe" (or "Região Floresta concluída! Próximo: ..."),
    /// weather badge/text and the in-game clock (TimeOfDay/Weather events, never polled).</item>
    /// <item>Buttons Missões / Mochila / Conquistas / Menu. Missões and Conquistas reuse
    /// <see cref="QuestLogController"/>/<see cref="AchievementListController"/> (their managers are
    /// GameManager-wide, so they work on the map). Menu reuses <see cref="PauseMenuController"/>
    /// (Configurações, Salvar via the Save/Load screen, "Menu Principal" → ReturnToTitleDialog →
    /// GameFlowService.ReturnToTitle).</item>
    /// <item>Mochila is a <b>read-only summary</b>: InventoryManager lives on the Player in Main.unity
    /// and exists on the map only as SaveManager carried JSON, so equipping/using/dropping here would
    /// have no live inventory to act on (and would have to rewrite the carried JSON behind the
    /// Player's back). The summary shows item/slot counts, gold, equipped count and the biggest stacks.</item>
    /// <item>Region labels: one per <see cref="WorldMapRegionNode"/>, projected from world space onto
    /// the panel (throttled to <see cref="_labelUpdateInterval"/>; no per-frame allocation), with
    /// name, padlock + unlock requirement, "Você está aqui" / "Toque para viajar". Tapping a label is
    /// the same as tapping its node (<see cref="WorldMapController.TryTravelTo"/>).</item>
    /// </list>
    /// Hidden while the front-end (title / hero picker, <see cref="GameFrontEndController"/>) is up:
    /// shown only in <see cref="GameFlowState.World"/> with a hero (always, without a GameManager).
    /// Map clicks that land on this HUD are swallowed via <see cref="WorldMapController.PointerBlocker"/>
    /// (WorldMap.unity has no EventSystem). Esc / gamepad Start: close the open screen, else pause
    /// menu back/close, else open the pause menu — same contract as <c>HudController</c> in Main.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WorldMapHudController : MonoBehaviour
    {
        // Save keys of the Player-scoped saveables read from SaveManager's carried state.
        private const string PlayerStatsSaveKey = "PlayerStats";
        private const string InventorySaveKey = "Inventory";

        private const string QuestLogScreenId = "quests";
        private const string AchievementsScreenId = "achievements";
        private const string BackpackScreenId = "backpack";

        private const string LabelClass = "wm-region-label";
        private const string LabelCurrentClass = "wm-region-label--current";
        private const string LabelTravelableClass = "wm-region-label--travelable";
        private const string LabelLockedClass = "wm-region-label--locked";
        private const string LabelClearedClass = "wm-region-label--cleared";
        private const string LabelCompactClass = "wm-region-label--compact";

        /// <summary>Minimum panel-space gap (px) kept between region labels by the overlap avoidance.</summary>
        private const float LabelGap = 4f;
        private const string BossButtonVisibleClass = "wm-boss-button--visible";

        [Header("Scene references (auto-resolved when empty)")]
        [SerializeField] private WorldMapController _worldMap;
        [SerializeField] private PauseMenuController _pauseMenu;
        [SerializeField] private QuestLogController _questLog;
        [SerializeField] private AchievementListController _achievementList;
        [Tooltip("Camera the region labels are projected with. Defaults to Camera.main.")]
        [SerializeField] private Camera _camera;

        [Header("Region labels")]
        [Tooltip("Seconds between label position updates (the map camera barely moves).")]
        [SerializeField] [Min(0.02f)] private float _labelUpdateInterval = 0.1f;
        [Tooltip("World-space height above each node's position the label is anchored to (bottom-center).")]
        [SerializeField] private float _labelHeightOffset = 1.6f;

        [Header("Mochila summary")]
        [SerializeField] [Min(0)] private int _backpackMaxItems = 6;

        private sealed class RegionLabel
        {
            public WorldMapRegionNode Node;
            public VisualElement Root;
            public Label Name;
            public Label Status;
            public RegionLabelStatus State = (RegionLabelStatus)(-1);
            public bool Cleared;
            public bool FullCard = true;
            public int Priority = 3;
            // Hidden until the first projection places it (no flash at the panel's top-left corner).
            public bool OnScreen;
            public Vector2 LastPosition = new Vector2(float.NaN, float.NaN);
        }

        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private readonly ScreenExclusivityCoordinator _screens = new ScreenExclusivityCoordinator();
        private readonly List<RegionLabel> _labels = new List<RegionLabel>();

        // Overlap-avoidance scratch buffers, sized with _labels (no per-update allocation).
        private Rect[] _labelRects = new Rect[0];
        private Vector2[] _labelAnchors = new Vector2[0];
        private int[] _labelPriorities = new int[0];
        private float[] _labelOffsets = new float[0];
        private int[] _labelOrder = new int[0];

        private UIDocument _uiDocument;
        private VisualElement _root;
        private VisualElement _labelsLayer;
        private Label _heroNameLabel;
        private Label _heroLevelLabel;
        private Label _regionNameLabel;
        private Label _victoriesLabel;
        private VisualElement _weatherIcon;
        private Label _weatherLabel;
        private Label _clockLabel;
        private Button _questsButton;
        private Button _inventoryButton;
        private Button _achievementsButton;
        private Button _menuButton;
        private Button _bossButton;
        private Button _exploreButton;
        private VisualElement _backpackOverlay;
        private Label _backpackHeroLabel;
        private VisualElement _backpackLines;
        private Button _backpackCloseButton;

        private bool _visible;
        private bool _backpackOpen;
        private bool _labelsBuilt;
        private bool _progressSeen;
        private bool _wasTraveling;
        private bool _labelsHiddenByModal;
        private float _nextLabelUpdate;
        private string _weatherIconClass;

        public bool IsVisible => _visible;
        public bool IsBackpackOpen => _backpackOpen;

        private System.Func<Vector2, bool> _pointerBlocker;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
            _pointerBlocker = IsScreenPointOverHud;
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            RegisterScreens();
            _localizer.Attach(_uiDocument.rootVisualElement, OnLanguageChanged, WorldMapTexts.Get);
            SetBackpackVisible(false);

            EventBus.Subscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
            EventBus.Subscribe<WorldMapRegionChangedEvent>(OnRegionChanged);
            EventBus.Subscribe<WorldMapRegionUnlockedEvent>(OnRegionUnlocked);
            EventBus.Subscribe<TimeOfDayHourChangedEvent>(OnHourChanged);
            EventBus.Subscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Subscribe<WeatherChangedEvent>(OnWeatherChanged);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Subscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
            EventBus.Subscribe<RegionBossChallengeStateChangedEvent>(OnBossChallengeStateChanged);
            EventBus.Subscribe<WorldMapRegionClearedEvent>(OnRegionCleared);

            if (_worldMap != null)
            {
                _worldMap.PointerBlocker = _pointerBlocker;
            }

            // Hidden until Start/the flow says otherwise: never flash over the title screen.
            ApplyVisibility(isStart: false);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameFlowStateChangedEvent>(OnFlowStateChanged);
            EventBus.Unsubscribe<WorldMapRegionChangedEvent>(OnRegionChanged);
            EventBus.Unsubscribe<WorldMapRegionUnlockedEvent>(OnRegionUnlocked);
            EventBus.Unsubscribe<TimeOfDayHourChangedEvent>(OnHourChanged);
            EventBus.Unsubscribe<TimeOfDayPhaseChangedEvent>(OnPhaseChanged);
            EventBus.Unsubscribe<WeatherChangedEvent>(OnWeatherChanged);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
            EventBus.Unsubscribe<CharacterSelectionRestoredEvent>(OnCharacterRestored);
            EventBus.Unsubscribe<RegionBossChallengeStateChangedEvent>(OnBossChallengeStateChanged);
            EventBus.Unsubscribe<WorldMapRegionClearedEvent>(OnRegionCleared);

            if (_worldMap != null && _worldMap.PointerBlocker == _pointerBlocker)
            {
                _worldMap.PointerBlocker = null;
            }

            SetBackpackVisible(false);
            UnregisterCallbacks();
            _localizer.Detach();
            GameplayPause.Release(this);
        }

        private void Start()
        {
            if (_worldMap == null)
            {
                _worldMap = FindFirstObjectByType<WorldMapController>(FindObjectsInactive.Include);
                if (_worldMap != null)
                {
                    _worldMap.PointerBlocker = _pointerBlocker;
                }
            }

            BuildRegionLabels();
            ApplyVisibility(isStart: true);
        }

        private void Update()
        {
            if (!_visible)
            {
                return;
            }

            if (BackPressedThisFrame())
            {
                HandleBack();
            }

            // Region labels sit in this low-sort-order HUD; any modal (quest log, achievements, pause,
            // the Mochila summary...) freezes gameplay, so hide them instead of letting them show through.
            SetLabelsHiddenByModal(ShouldHideRegionLabels(GameplayPause.IsPaused));

            if (Time.unscaledTime < _nextLabelUpdate)
            {
                return;
            }

            _nextLabelUpdate = Time.unscaledTime + _labelUpdateInterval;

            // WorldMapController builds its progress in Start; pick it up whenever it shows up.
            if (!_progressSeen && Progress != null)
            {
                _progressSeen = true;
                RefreshAll();
            }

            bool traveling = _worldMap != null && _worldMap.IsTraveling;
            if (traveling != _wasTraveling)
            {
                _wasTraveling = traveling;
                RefreshLabelStates();
                RefreshBossButton();
            }

            UpdateLabelPositions();
        }

        // ------------------------------------------------------------------
        // Binding
        // ------------------------------------------------------------------

        private void BindElements()
        {
            VisualElement root = _uiDocument.rootVisualElement;
            // The document root fills the screen: keep it transparent to picks so the map stays tappable.
            root.pickingMode = PickingMode.Ignore;

            _root = root.Q<VisualElement>("wm-hud-root");
            _labelsLayer = root.Q<VisualElement>("wm-labels-layer");
            _heroNameLabel = root.Q<Label>("wm-hero-name-label");
            _heroLevelLabel = root.Q<Label>("wm-hero-level-label");
            _regionNameLabel = root.Q<Label>("wm-region-name-label");
            _victoriesLabel = root.Q<Label>("wm-victories-label");
            _weatherIcon = root.Q<VisualElement>("wm-weather-icon");
            _weatherLabel = root.Q<Label>("wm-weather-label");
            _clockLabel = root.Q<Label>("wm-clock-label");
            _questsButton = root.Q<Button>("wm-quests-button");
            _inventoryButton = root.Q<Button>("wm-inventory-button");
            _achievementsButton = root.Q<Button>("wm-achievements-button");
            _menuButton = root.Q<Button>("wm-menu-button");
            _bossButton = root.Q<Button>("wm-boss-button");
            _exploreButton = root.Q<Button>("wm-explore-button");
            _backpackOverlay = root.Q<VisualElement>("wm-backpack-overlay");
            _backpackHeroLabel = root.Q<Label>("wm-backpack-hero-label");
            _backpackLines = root.Q<VisualElement>("wm-backpack-lines");
            _backpackCloseButton = root.Q<Button>("wm-backpack-close-button");

            // Labels are rebuilt for the (possibly new) tree after a re-enable.
            _labels.Clear();
            _labelsBuilt = false;
            _labelsHiddenByModal = false;
        }

        private void RegisterCallbacks()
        {
            if (_questsButton != null) _questsButton.clicked += OnQuestsClicked;
            if (_inventoryButton != null) _inventoryButton.clicked += OnInventoryClicked;
            if (_achievementsButton != null) _achievementsButton.clicked += OnAchievementsClicked;
            if (_menuButton != null) _menuButton.clicked += OnMenuClicked;
            if (_bossButton != null) _bossButton.clicked += OnBossClicked;
            if (_exploreButton != null) _exploreButton.clicked += OnExploreClicked;
            if (_backpackCloseButton != null) _backpackCloseButton.clicked += CloseBackpack;
        }

        private void UnregisterCallbacks()
        {
            if (_questsButton != null) _questsButton.clicked -= OnQuestsClicked;
            if (_inventoryButton != null) _inventoryButton.clicked -= OnInventoryClicked;
            if (_achievementsButton != null) _achievementsButton.clicked -= OnAchievementsClicked;
            if (_menuButton != null) _menuButton.clicked -= OnMenuClicked;
            if (_bossButton != null) _bossButton.clicked -= OnBossClicked;
            if (_exploreButton != null) _exploreButton.clicked -= OnExploreClicked;
            if (_backpackCloseButton != null) _backpackCloseButton.clicked -= CloseBackpack;
        }

        /// <summary>
        /// Lets a map add-on (e.g. <see cref="VillageHubButton"/>) join this HUD's screens: one open at a
        /// time, and Esc / gamepad Start closes it before opening the pause menu.
        /// </summary>
        public void RegisterScreen(string id, System.Func<bool> isOpen, System.Action open, System.Action close)
        {
            _screens.Register(id, isOpen, open, close);
        }

        private void RegisterScreens()
        {
            _screens.Register(QuestLogScreenId,
                () => ResolveQuestLog() && _questLog.IsOpen,
                () => { if (ResolveQuestLog()) _questLog.Open(); },
                () => { if (ResolveQuestLog()) _questLog.Hide(); });

            _screens.Register(AchievementsScreenId,
                () => ResolveAchievements() && _achievementList.IsOpen,
                () => { if (ResolveAchievements()) _achievementList.Open(); },
                () => { if (ResolveAchievements()) _achievementList.Hide(); });

            _screens.Register(BackpackScreenId, () => _backpackOpen, OpenBackpack, CloseBackpack);
        }

        private bool ResolveQuestLog()
        {
            if (_questLog == null) _questLog = FindFirstObjectByType<QuestLogController>();
            return _questLog != null;
        }

        private bool ResolveAchievements()
        {
            if (_achievementList == null) _achievementList = FindFirstObjectByType<AchievementListController>();
            return _achievementList != null;
        }

        private bool ResolvePauseMenu()
        {
            if (_pauseMenu == null) _pauseMenu = FindFirstObjectByType<PauseMenuController>();
            return _pauseMenu != null;
        }

        /// <summary>Region labels are hidden while any modal screen (or the pause menu) freezes gameplay.</summary>
        public static bool ShouldHideRegionLabels(bool gameplayPaused) => gameplayPaused;

        private void SetLabelsHiddenByModal(bool hidden)
        {
            if (hidden == _labelsHiddenByModal || _labelsLayer == null)
            {
                return;
            }

            _labelsHiddenByModal = hidden;
            _labelsLayer.style.display = hidden ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private WorldMapProgress Progress => _worldMap != null ? _worldMap.Progress : null;

        private RegionBossProgress Bosses => _worldMap != null ? _worldMap.RegionBosses : null;

        private WorldMapGraph Graph => _worldMap != null ? _worldMap.Graph : null;

        private LocalizationManager Localization
        {
            get
            {
                ServiceLocator.TryGet(out LocalizationManager manager);
                return manager;
            }
        }

        // ------------------------------------------------------------------
        // Visibility (front-end)
        // ------------------------------------------------------------------

        /// <summary>World state with a hero; always without a GameManager (scene played on its own).</summary>
        public static bool ShouldShow(bool hasFlow, GameFlowState state, bool hasHero)
        {
            return !hasFlow || (GameFlowMachine.IsWorldInteractive(state) && hasHero);
        }

        private void ApplyVisibility(bool isStart)
        {
            bool show;
            if (ServiceLocator.TryGet(out GameFlowService flow))
            {
                show = ShouldShow(true, flow.State, flow.HasHero);
            }
            else
            {
                // OnEnable runs before GameManager may have registered anything: wait for Start.
                show = isStart && ShouldShow(false, default, false);
            }

            SetVisible(show);
        }

        private void SetVisible(bool visible)
        {
            bool wasVisible = _visible;
            _visible = visible;

            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (!visible)
            {
                _screens.CloseAll();
                return;
            }

            if (!wasVisible)
            {
                _localizer.Refresh();
                BuildRegionLabels();
                RefreshAll();
                _nextLabelUpdate = 0f;
            }
        }

        // ------------------------------------------------------------------
        // Text
        // ------------------------------------------------------------------

        private void RefreshAll()
        {
            RefreshHero();
            RefreshRegion();
            RefreshClock();
            RefreshWeather();
            RefreshLabelStates();
        }

        private void RefreshHero()
        {
            LocalizationManager localization = Localization;
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

            string json = null;
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.TryGetCarriedJson(PlayerStatsSaveKey, out json);
            }

            SetText(_heroNameLabel, WorldMapHudPresenter.HeroName(localization, hero));
            SetText(_heroLevelLabel, WorldMapHudPresenter.LevelText(localization, WorldMapHudPresenter.ParseLevel(json)));
        }

        private void RefreshRegion()
        {
            WorldMapProgress progress = Progress;
            if (progress == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            SetText(_regionNameLabel, WorldMapHudPresenter.RegionName(localization, progress.CurrentRegion));
            SetText(_victoriesLabel, WorldMapHudPresenter.GoalText(localization, Graph, progress, Bosses));
            RefreshBossButton();
        }

        /// <summary>"Enfrentar o chefe" is visible only while the current region's boss challenge can start.</summary>
        private void RefreshBossButton()
        {
            if (_bossButton == null)
            {
                return;
            }

            bool offered = _worldMap != null && _worldMap.isActiveAndEnabled && _worldMap.CanChallengeCurrentBoss;
            _bossButton.EnableInClassList(BossButtonVisibleClass, offered);
            _bossButton.SetEnabled(offered);
        }

        private void RefreshClock()
        {
            if (ServiceLocator.TryGet(out TimeOfDayManager clock))
            {
                SetClock(clock.Day, Mathf.FloorToInt(clock.CurrentHour), clock.Phase);
            }
            else if (_clockLabel != null)
            {
                _clockLabel.style.display = DisplayStyle.None;
            }
        }

        private void SetClock(int day, int hour, TimeOfDayPhase phase)
        {
            if (_clockLabel == null)
            {
                return;
            }

            _clockLabel.style.display = DisplayStyle.Flex;
            _clockLabel.text = WorldMapHudPresenter.ClockText(Localization, day, hour, phase);
        }

        private void RefreshWeather()
        {
            if (ServiceLocator.TryGet(out WeatherSystem weather))
            {
                SetWeather(weather.CurrentWeather);
            }
            else
            {
                SetWeather(WeatherType.Clear);
            }
        }

        private void SetWeather(WeatherType weather)
        {
            SetText(_weatherLabel, WorldMapHudPresenter.WeatherText(Localization, weather));

            if (_weatherIcon == null)
            {
                return;
            }

            string iconClass = WorldMapHudPresenter.WeatherIconClass(weather);
            if (iconClass == _weatherIconClass)
            {
                return;
            }

            foreach (string cls in WorldMapHudPresenter.AllWeatherIconClasses)
            {
                _weatherIcon.RemoveFromClassList(cls);
            }

            _weatherIcon.AddToClassList(iconClass);
            _weatherIconClass = iconClass;
        }

        private static void SetText(TextElement element, string text)
        {
            if (element != null)
            {
                element.text = text;
            }
        }

        private void OnLanguageChanged()
        {
            RefreshAll();
            ForceLabelTextRefresh();

            if (_backpackOpen)
            {
                PopulateBackpack();
            }
        }

        // ------------------------------------------------------------------
        // Region labels
        // ------------------------------------------------------------------

        private void BuildRegionLabels()
        {
            if (_labelsBuilt || _labelsLayer == null || _worldMap == null || _worldMap.Nodes == null)
            {
                return;
            }

            _labelsBuilt = true;
            _labelsLayer.Clear();
            _labels.Clear();

            foreach (WorldMapRegionNode node in _worldMap.Nodes)
            {
                if (node == null)
                {
                    continue;
                }

                var label = new RegionLabel { Node = node, Root = new VisualElement() };
                label.Root.AddToClassList(LabelClass);
                label.Root.name = "wm-region-label-" + node.Region.ToString().ToLowerInvariant();
                label.Root.style.display = DisplayStyle.None;

                var header = new VisualElement { pickingMode = PickingMode.Ignore };
                header.AddToClassList("wm-region-label-header");

                var lockIcon = new VisualElement { pickingMode = PickingMode.Ignore };
                lockIcon.AddToClassList("wm-lock-icon");
                var shackle = new VisualElement { pickingMode = PickingMode.Ignore };
                shackle.AddToClassList("wm-lock-shackle");
                var body = new VisualElement { pickingMode = PickingMode.Ignore };
                body.AddToClassList("wm-lock-body");
                lockIcon.Add(shackle);
                lockIcon.Add(body);

                label.Name = new Label { pickingMode = PickingMode.Ignore };
                label.Name.AddToClassList("wm-region-label-name");
                label.Status = new Label { pickingMode = PickingMode.Ignore };
                label.Status.AddToClassList("wm-region-label-status");

                // Cleared marker: a check mark drawn with two borders (no glyph: the default font has no ✓).
                var clearedIcon = new VisualElement { pickingMode = PickingMode.Ignore };
                clearedIcon.AddToClassList("wm-cleared-icon");
                var check = new VisualElement { pickingMode = PickingMode.Ignore };
                check.AddToClassList("wm-cleared-check");
                clearedIcon.Add(check);

                header.Add(lockIcon);
                header.Add(clearedIcon);
                header.Add(label.Name);
                label.Root.Add(header);
                label.Root.Add(label.Status);

                RegionLabel captured = label;
                label.Root.AddManipulator(new Clickable(() => OnRegionLabelClicked(captured)));

                _labelsLayer.Add(label.Root);
                _labels.Add(label);
            }

            int count = _labels.Count;
            _labelRects = new Rect[count];
            _labelAnchors = new Vector2[count];
            _labelPriorities = new int[count];
            _labelOffsets = new float[count];
            _labelOrder = new int[count];

            ForceLabelTextRefresh();
        }

        private void ForceLabelTextRefresh()
        {
            foreach (RegionLabel label in _labels)
            {
                label.State = (RegionLabelStatus)(-1);
            }

            RefreshLabelStates();
        }

        /// <summary>Re-evaluates each label's status; text is rebuilt only when its status changed (or after a language switch).</summary>
        private void RefreshLabelStates()
        {
            if (_labels.Count == 0)
            {
                return;
            }

            WorldMapProgress progress = Progress;
            WorldMapGraph graph = Graph;
            RegionBossProgress bosses = Bosses;
            bool traveling = _worldMap != null && _worldMap.IsTraveling;
            LocalizationManager localization = null;
            bool localizationResolved = false;

            foreach (RegionLabel label in _labels)
            {
                RegionLabelStatus status = WorldMapHudPresenter.GetLabelStatus(graph, progress, label.Node.Region, traveling);
                bool cleared = WorldMapHudPresenter.IsCleared(progress, bosses, label.Node.Region);

                // Locked labels show live victory counts, so they always refresh.
                if (status == label.State && cleared == label.Cleared && status != RegionLabelStatus.Locked)
                {
                    continue;
                }

                if (!localizationResolved)
                {
                    localization = Localization;
                    localizationResolved = true;
                }

                bool fullCard = WorldMapHudPresenter.ShowFullLabel(graph, progress, label.Node.Region, status);

                label.State = status;
                label.Cleared = cleared;
                label.FullCard = fullCard;
                label.Priority = WorldMapHudPresenter.LabelPriority(status, fullCard);
                label.Name.text = WorldMapHudPresenter.RegionName(localization, label.Node.Region);
                string statusText = WorldMapHudPresenter.LabelStatusText(localization, graph, progress, label.Node.Region, status, bosses);
                label.Status.text = statusText;
                // Far regions collapse to a name chip so cards don't stack over each other.
                label.Status.style.display = !fullCard || string.IsNullOrEmpty(statusText) ? DisplayStyle.None : DisplayStyle.Flex;
                label.Root.EnableInClassList(LabelCompactClass, !fullCard);
                label.Root.EnableInClassList(LabelCurrentClass, status == RegionLabelStatus.Current);
                label.Root.EnableInClassList(LabelTravelableClass, status == RegionLabelStatus.Travelable);
                label.Root.EnableInClassList(LabelLockedClass, status == RegionLabelStatus.Locked);
                label.Root.EnableInClassList(LabelClearedClass, cleared);
            }
        }

        /// <summary>
        /// Projects every node onto the panel, then nudges overlapping labels apart vertically
        /// (<see cref="WorldMapHudPresenter.ResolveLabelOverlaps"/>: current/travelable labels keep
        /// their spot, lower-priority ones move). Struct-only math and style writes: no allocation.
        /// </summary>
        private void UpdateLabelPositions()
        {
            if (_labels.Count == 0 || _root == null || _labelRects.Length != _labels.Count)
            {
                return;
            }

            IPanel panel = _root.panel;
            if (panel == null)
            {
                return;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                {
                    return;
                }
            }

            int count = _labels.Count;

            for (int i = 0; i < count; i++)
            {
                RegionLabel label = _labels[i];
                _labelRects[i] = default;
                _labelPriorities[i] = label.Priority;

                if (label.Node == null)
                {
                    continue;
                }

                Vector3 world = label.Node.Position + Vector3.up * _labelHeightOffset;
                bool onScreen = _camera.WorldToViewportPoint(world).z > 0f;

                if (onScreen != label.OnScreen)
                {
                    label.OnScreen = onScreen;
                    label.Root.style.display = onScreen ? DisplayStyle.Flex : DisplayStyle.None;
                }

                if (!onScreen)
                {
                    continue;
                }

                Vector2 anchor = RuntimePanelUtils.CameraTransformWorldToPanel(panel, world, _camera);
                _labelAnchors[i] = anchor;

                // USS anchors the label's bottom-centre on the point (translate: -50% -100%). Before
                // the first layout pass the size is NaN/0 and the label simply isn't avoided yet.
                Rect layout = label.Root.layout;
                float width = float.IsNaN(layout.width) ? 0f : layout.width;
                float height = float.IsNaN(layout.height) ? 0f : layout.height;
                _labelRects[i] = new Rect(anchor.x - width * 0.5f, anchor.y - height, width, height);
            }

            WorldMapHudPresenter.ResolveLabelOverlaps(_labelRects, _labelPriorities, count, LabelGap, _labelOffsets, _labelOrder);

            for (int i = 0; i < count; i++)
            {
                RegionLabel label = _labels[i];
                if (label.Node == null || !label.OnScreen)
                {
                    continue;
                }

                Vector2 position = _labelAnchors[i] + new Vector2(0f, _labelOffsets[i]);
                if ((position - label.LastPosition).sqrMagnitude < 0.25f)
                {
                    continue;
                }

                label.LastPosition = position;
                label.Root.style.left = position.x;
                label.Root.style.top = position.y;
            }
        }

        private void OnRegionLabelClicked(RegionLabel label)
        {
            if (!_visible || GameplayPause.IsPaused || _worldMap == null || !_worldMap.isActiveAndEnabled || label?.Node == null)
            {
                return;
            }

            _worldMap.TryTravelTo(label.Node);
        }

        /// <summary><see cref="WorldMapController.PointerBlocker"/>: true when <paramref name="screenPosition"/> (bottom-left origin) hits this HUD.</summary>
        public bool IsScreenPointOverHud(Vector2 screenPosition)
        {
            if (!_visible || _root == null || _root.panel == null)
            {
                return false;
            }

            IPanel panel = _root.panel;
            Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            VisualElement picked = panel.Pick(panelPosition);
            return picked != null && (picked == _root || _root.Contains(picked));
        }

        // ------------------------------------------------------------------
        // Buttons / back
        // ------------------------------------------------------------------

        private bool IsPauseMenuOpen => ResolvePauseMenu() && _pauseMenu.IsOpen;

        private void OnQuestsClicked() => ToggleScreen(QuestLogScreenId);

        private void OnBossClicked()
        {
            if (!_visible || GameplayPause.IsPaused || _worldMap == null || !_worldMap.isActiveAndEnabled)
            {
                return;
            }

            _worldMap.TryChallengeBoss();
            RefreshBossButton();
        }

        private void OnExploreClicked()
        {
            if (!_visible || GameplayPause.IsPaused || _worldMap == null || !_worldMap.isActiveAndEnabled)
            {
                return;
            }

            _worldMap.TryExplore();
        }

        private void OnInventoryClicked() => ToggleScreen(BackpackScreenId);

        private void OnAchievementsClicked() => ToggleScreen(AchievementsScreenId);

        /// <summary>Exclusive toggle; ignored while the pause menu (or something else) owns the screen.</summary>
        private void ToggleScreen(string screenId)
        {
            if (IsPauseMenuOpen || (GameplayPause.IsPaused && !_screens.IsAnyOpen))
            {
                return;
            }

            _screens.Toggle(screenId);
        }

        private void OnMenuClicked()
        {
            if (!ResolvePauseMenu())
            {
                return;
            }

            if (_pauseMenu.IsOpen)
            {
                _pauseMenu.Hide();
                return;
            }

            if (GameplayPause.IsPaused && !_screens.IsAnyOpen)
            {
                return;
            }

            // Show first so a screen holding a GameplayPause releases it only once the menu paused.
            _pauseMenu.Show();
            _screens.CloseAll();
        }

        /// <summary>Esc / gamepad Start: pause menu back, else close the open screen, else open the pause menu.</summary>
        public void HandleBack()
        {
            if (IsPauseMenuOpen)
            {
                _pauseMenu.HandleBack();
                return;
            }

            if (_screens.CloseAll() > 0 || GameplayPause.IsPaused)
            {
                return;
            }

            if (ResolvePauseMenu())
            {
                _pauseMenu.Show();
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
            return gamepad != null && gamepad.startButton.wasPressedThisFrame;
        }

        // ------------------------------------------------------------------
        // Mochila (read-only summary)
        // ------------------------------------------------------------------

        public void OpenBackpack()
        {
            if (_backpackOverlay == null)
            {
                return;
            }

            PopulateBackpack();
            SetBackpackVisible(true);
            _backpackCloseButton?.Focus();
        }

        public void CloseBackpack() => SetBackpackVisible(false);

        private void SetBackpackVisible(bool visible)
        {
            _backpackOpen = visible && _backpackOverlay != null;

            if (_backpackOverlay != null)
            {
                _backpackOverlay.style.display = _backpackOpen ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_backpackOpen)
            {
                GameplayPause.Acquire(this);
            }
            else
            {
                GameplayPause.Release(this);
            }
        }

        private void PopulateBackpack()
        {
            LocalizationManager localization = Localization;
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

            string statsJson = null;
            string inventoryJson = null;
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.TryGetCarriedJson(PlayerStatsSaveKey, out statsJson);
                saveManager.TryGetCarriedJson(InventorySaveKey, out inventoryJson);
            }

            SetText(_backpackHeroLabel, WorldMapHudPresenter.HeroName(localization, hero) + " · " +
                                        WorldMapHudPresenter.LevelText(localization, WorldMapHudPresenter.ParseLevel(statsJson)));

            if (_backpackLines == null)
            {
                return;
            }

            _backpackLines.Clear();
            BackpackSummary summary = WorldMapHudPresenter.Summarize(WorldMapHudPresenter.ParseInventory(inventoryJson));
            foreach (string line in WorldMapHudPresenter.BackpackLines(localization, summary, _backpackMaxItems))
            {
                var row = new Label(line);
                row.AddToClassList("wm-backpack-line");
                _backpackLines.Add(row);
            }
        }

        // ------------------------------------------------------------------
        // Events
        // ------------------------------------------------------------------

        private void OnFlowStateChanged(GameFlowStateChangedEvent evt) => ApplyVisibility(isStart: true);

        private void OnRegionChanged(WorldMapRegionChangedEvent evt)
        {
            RefreshRegion();
            RefreshLabelStates();
        }

        private void OnRegionUnlocked(WorldMapRegionUnlockedEvent evt)
        {
            RefreshRegion();
            RefreshLabelStates();
        }

        private void OnHourChanged(TimeOfDayHourChangedEvent evt) => SetClock(evt.Day, evt.Hour, evt.Phase);

        private void OnPhaseChanged(TimeOfDayPhaseChangedEvent evt) => RefreshClock();

        private void OnWeatherChanged(WeatherChangedEvent evt) => SetWeather(evt.CurrentWeather);

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            if (_visible)
            {
                RefreshAll();
            }
        }

        private void OnCharacterSelected(CharacterSelectedEvent evt) => RefreshHero();

        private void OnBossChallengeStateChanged(RegionBossChallengeStateChangedEvent evt) => RefreshBossButton();

        private void OnRegionCleared(WorldMapRegionClearedEvent evt)
        {
            RefreshRegion();
            RefreshLabelStates();
        }

        private void OnCharacterRestored(CharacterSelectionRestoredEvent evt) => RefreshHero();
    }
}
