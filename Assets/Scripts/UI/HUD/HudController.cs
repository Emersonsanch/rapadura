using System;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.Skills;
using Rapadura.UI.Achievements;
using Rapadura.UI.Common;
using Rapadura.UI.Crafting;
using Rapadura.UI.Inventory;
using Rapadura.UI.Menus;
using Rapadura.UI.Quests;
using Rapadura.UI.Shop;
using Rapadura.UI.SkillTree;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Drives the always-on gameplay HUD (health/stamina/mana/XP bars) built with UI Toolkit
    /// (HudView.uxml/.uss), following the same pattern as <c>SkillTreeController</c>: a single
    /// <see cref="UIDocument"/>, elements resolved once via <c>Q&lt;T&gt;()</c>, and bars updated
    /// reactively from EventBus events published by <see cref="PlayerStats"/> instead of polling
    /// every frame — important on mobile where battery/thermal budget matters (see TODO.md Fase 0,
    /// "Bateria/thermal: evitar Update() caro rodando sempre a 60fps sem necessidade").
    ///
    /// Mana is driven the same way as health/stamina: <see cref="PlayerStats"/> publishes
    /// <see cref="PlayerManaChangedEvent"/> from SetMana, and this controller subscribes to it
    /// directly instead of polling every frame.
    ///
    /// SCREENS: the top-right cluster (Missões / Mochila / Skills / Menu) and the keyboard/gamepad
    /// shortcuts (QuestLog J, Inventory I / Select, SkillTree K, Pause Esc / Start — actions in
    /// PlayerControls.inputactions, read from the player's PlayerInput like PlayerInputHandler does,
    /// but via <c>performed</c> callbacks so the HUD still needs no Update()) all go through a
    /// <see cref="ScreenExclusivityCoordinator"/>: opening a screen closes the other ones (including
    /// Conquistas/Criação, which are opened from the Pause menu). Pausing gameplay stays each
    /// screen's own business (GameplayPause.Acquire/Release); the HUD only ignores screen
    /// buttons/shortcuts while the pause menu or death screen own the screen — Esc/Start then
    /// means "back" in the pause menu. Opening Pause closes any open screen first.
    ///
    /// Screens opened from the world go through the same rule: the Loja (opened by an NPC via
    /// ShopManager.OpenShop) is picked up from <see cref="ShopOpenedEvent"/>, and a crafting bancada
    /// calls <see cref="OpenScreenExclusive"/> with its station-specific open.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class HudController : MonoBehaviour
    {
        /// <summary>Where the HUD anchors itself within the screen/panel.</summary>
        public enum HudAnchorPreset
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Custom
        }

        [Header("References")]
        [SerializeField] private PlayerStats _playerStats;
        [SerializeField] private PlayerInputHandler _playerInput;
        [Tooltip("Opened by the on-screen Skills button — see SkillTreeController.Toggle().")]
        [SerializeField] private SkillTreeController _skillTreeController;
        [Tooltip("Opened by the on-screen Mochila button — see InventoryUIController.Toggle(). If empty, resolved from the scene on first tap.")]
        [SerializeField] private InventoryUIController _inventoryController;
        [Tooltip("Opened by the on-screen Menu button — see PauseMenuController.Toggle().")]
        [SerializeField] private PauseMenuController _pauseMenuController;
        [Tooltip("Opened by the on-screen Missões button / QuestLog shortcut. If empty, resolved from the scene on first use.")]
        [SerializeField] private QuestLogController _questLogController;
        [Tooltip("Pause-menu Conquistas screen — registered here so opening another screen closes it. If empty, resolved from the scene.")]
        [SerializeField] private AchievementListController _achievementListController;
        [Tooltip("Pause-menu / workbench Criação screen — registered here so opening another screen closes it. If empty, resolved from the scene.")]
        [SerializeField] private CraftingUIController _craftingController;
        [Tooltip("NPC-opened Loja screen — registered here so opening another screen ends the shop session, and a shop opening closes the others. If empty, resolved from the scene.")]
        [SerializeField] private ShopUIController _shopController;
        [Tooltip("Player's SkillHotbar (slot -> skill loadout + casting). If empty it is resolved from " +
                 "_playerInput's GameObject, and added there at runtime if that GameObject has a SkillManager but no SkillHotbar yet.")]
        [SerializeField] private SkillHotbar _skillHotbar;

        [Header("Skill Hotbar")]
        [Tooltip("How often (ms) the hotbar's cooldown overlays and mana-insufficient state refresh. " +
                 "Polled on a UI Toolkit schedule instead of every frame (battery budget, see class summary).")]
        [SerializeField, Range(16, 500)] private int _skillHotbarRefreshMs = 100;

        [Header("Adaptive HUD")]
        [Tooltip("Uniform scale applied to the whole HUD root. Exposed here so a future " +
                 "Settings screen (TODO.md Fase 6 > Menus > Configurações) can bind an " +
                 "accessibility slider to SetHudScale() without touching this component.")]
        [SerializeField, Range(0.5f, 2f)] private float _hudScale = 1f;

        [Tooltip("Corner the HUD anchors to. 'Custom' uses _customOffset as a raw left/top " +
                 "offset instead of a corner, for free placement from a future layout editor.")]
        [SerializeField] private HudAnchorPreset _anchorPreset = HudAnchorPreset.TopLeft;

        [Tooltip("Offset from the chosen anchor (or raw left/top when preset is Custom), in pixels.")]
        [SerializeField] private Vector2 _anchorOffset = Vector2.zero;

        [Header("Low Value Warning")]
        [SerializeField, Range(0f, 1f)] private float _lowValueThreshold = 0.25f;

        private UIDocument _uiDocument;
        private VisualElement _root;

        private VisualElement _healthFill;
        private Label _healthValueLabel;
        private VisualElement _staminaFill;
        private Label _staminaValueLabel;
        private VisualElement _manaFill;
        private Label _manaValueLabel;
        private VisualElement _xpFill;
        private Label _xpValueLabel;
        private Label _levelLabel;
        private Button _runButton;
        private Button _jumpButton;
        private Button _skillsButton;
        private Button _inventoryButton;
        private Button _pauseButton;
        private Button _questsButton;

        // Screen ids for the exclusivity coordinator.
        public const string SkillTreeScreenId = "skill-tree";
        public const string InventoryScreenId = "inventory";
        public const string QuestLogScreenId = "quest-log";
        public const string AchievementsScreenId = "achievements";
        public const string CraftingScreenId = "crafting";
        public const string ShopScreenId = "shop";

        // Action names in PlayerControls.inputactions (Player map).
        public const string QuestLogActionName = "QuestLog";
        public const string InventoryActionName = "Inventory";
        public const string SkillTreeActionName = "SkillTree";
        public const string PauseActionName = "Pause";

        private readonly ScreenExclusivityCoordinator _screens = new ScreenExclusivityCoordinator();
        private bool _gamePaused;

        private InputAction _questLogAction;
        private InputAction _inventoryAction;
        private InputAction _skillTreeAction;
        private InputAction _pauseAction;
        private bool _shortcutsBound;
        private float _nextShortcutBindAttempt;
        private const float ShortcutBindRetrySeconds = 1f;

        private int _skillSlotNameCharsPerLine = SkillSlotDisplayMath.SlotNameBaseCharsPerLine;

        private const int SkillSlotCount = HotbarLoadout.DefaultSlotCount;
        private readonly Button[] _skillSlotButtons = new Button[SkillSlotCount];
        private readonly VisualElement[] _skillSlotIcons = new VisualElement[SkillSlotCount];
        private readonly Label[] _skillSlotNames = new Label[SkillSlotCount];
        private readonly VisualElement[] _skillSlotCooldowns = new VisualElement[SkillSlotCount];
        private readonly Label[] _skillSlotCooldownLabels = new Label[SkillSlotCount];
        private readonly SkillDefinition[] _skillSlotShownSkill = new SkillDefinition[SkillSlotCount];
        private readonly int[] _skillSlotCooldownLabelKey = new int[SkillSlotCount];
        private readonly EventCallback<PointerDownEvent>[] _skillSlotPressedCallbacks = new EventCallback<PointerDownEvent>[SkillSlotCount];
        private IVisualElementScheduledItem _skillHotbarRefresh;
        private HotbarLoadout _subscribedLoadout;
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private int _displayedLevel = 1;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            for (int i = 0; i < SkillSlotCount; i++)
            {
                int slotIndex = i;
                _skillSlotPressedCallbacks[i] = evt => OnSkillSlotPressed(slotIndex);
            }

            RegisterScreens();
        }

        private void OnEnable()
        {
            BindElements();
            ApplyAdaptiveLayout();
            RegisterCallbacks();
            _localizer.Attach(_uiDocument.rootVisualElement, UpdateLevelLabel);
            InitializeFromCurrentStats();
        }

        private void Start()
        {
            // OnEnable can run before PlayerStats.Awake() has set its Current*=Max* defaults
            // (Unity doesn't guarantee cross-object Awake/OnEnable ordering for objects spawned
            // at runtime), which left the bars reading 0/0 (invisible fill) until the first
            // stat-change event. Re-reading here, once every object's Awake has definitely run,
            // fixes that first-frame empty-bar bug without needing explicit script execution order.
            // LocalizationManager may only be registered by now (GameManager.Awake ordering).
            _localizer.Refresh();
            InitializeFromCurrentStats();
            RefreshSkillHotbar();
            TryBindShortcuts();
        }

        private void OnDisable()
        {
            _localizer.Detach();
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("hud-root");

            _healthFill = rootVisualElement.Q<VisualElement>("health-bar-fill");
            _healthValueLabel = rootVisualElement.Q<Label>("health-bar-value-label");

            _staminaFill = rootVisualElement.Q<VisualElement>("stamina-bar-fill");
            _staminaValueLabel = rootVisualElement.Q<Label>("stamina-bar-value-label");

            _manaFill = rootVisualElement.Q<VisualElement>("mana-bar-fill");
            _manaValueLabel = rootVisualElement.Q<Label>("mana-bar-value-label");

            _xpFill = rootVisualElement.Q<VisualElement>("xp-bar-fill");
            _xpValueLabel = rootVisualElement.Q<Label>("xp-bar-value-label");
            _levelLabel = rootVisualElement.Q<Label>("level-label");

            _runButton = rootVisualElement.Q<Button>("run-button");
            _jumpButton = rootVisualElement.Q<Button>("jump-button");
            _skillsButton = rootVisualElement.Q<Button>("skills-button");
            _inventoryButton = rootVisualElement.Q<Button>("inventory-button");
            _pauseButton = rootVisualElement.Q<Button>("pause-button");
            _questsButton = rootVisualElement.Q<Button>("quests-button");

            for (int i = 0; i < SkillSlotCount; i++)
            {
                string prefix = "skill-slot-" + i;
                _skillSlotButtons[i] = rootVisualElement.Q<Button>(prefix);
                _skillSlotIcons[i] = rootVisualElement.Q<VisualElement>(prefix + "-icon");
                _skillSlotNames[i] = rootVisualElement.Q<Label>(prefix + "-name");
                _skillSlotCooldowns[i] = rootVisualElement.Q<VisualElement>(prefix + "-cooldown");
                _skillSlotCooldownLabels[i] = rootVisualElement.Q<Label>(prefix + "-cooldown-label");
                _skillSlotShownSkill[i] = null;
                _skillSlotCooldownLabelKey[i] = -1;
            }
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Subscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Subscribe<PlayerManaChangedEvent>(OnManaChanged);
            EventBus.Subscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
            EventBus.Subscribe<ShopOpenedEvent>(OnShopOpened);

            if (_runButton != null)
            {
                // Not the Button's Click event on purpose — Click only fires on release, but Run
                // needs to stay held for as long as the finger/pointer stays down (a "sprint"
                // toggle, not a one-shot action), so PointerDown/Up + PointerLeave (finger sliding
                // off the button without a clean release) drive it directly instead.
                _runButton.RegisterCallback<PointerDownEvent>(OnRunButtonPressed);
                _runButton.RegisterCallback<PointerUpEvent>(OnRunButtonPointerUp);
                _runButton.RegisterCallback<PointerLeaveEvent>(OnRunButtonPointerLeave);
            }

            if (_jumpButton != null)
            {
                // One-shot like the physical Jump key, so PointerDown (not Click, which waits for
                // release) — same responsiveness reasoning as the run button.
                _jumpButton.RegisterCallback<PointerDownEvent>(OnJumpButtonPressed);
            }

            if (_skillsButton != null)
            {
                _skillsButton.clicked += OnSkillsButtonClicked;
            }

            if (_inventoryButton != null)
            {
                _inventoryButton.clicked += OnInventoryButtonClicked;
            }

            if (_pauseButton != null)
            {
                _pauseButton.clicked += OnPauseButtonClicked;
            }

            if (_questsButton != null)
            {
                _questsButton.clicked += OnQuestsButtonClicked;
            }

            for (int i = 0; i < SkillSlotCount; i++)
            {
                // PointerDown (not Click) for the same one-shot responsiveness as the jump button.
                _skillSlotButtons[i]?.RegisterCallback(_skillSlotPressedCallbacks[i]);
            }

            if (_root != null)
            {
                _skillHotbarRefresh = _root.schedule.Execute(RefreshSkillHotbar).Every(_skillHotbarRefreshMs);
            }

            SubscribeToLoadout();
            TryBindShortcuts();
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
            EventBus.Unsubscribe<PlayerStaminaChangedEvent>(OnStaminaChanged);
            EventBus.Unsubscribe<PlayerManaChangedEvent>(OnManaChanged);
            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(OnExperienceChanged);
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
            EventBus.Unsubscribe<ShopOpenedEvent>(OnShopOpened);

            if (_runButton != null)
            {
                _runButton.UnregisterCallback<PointerDownEvent>(OnRunButtonPressed);
                _runButton.UnregisterCallback<PointerUpEvent>(OnRunButtonPointerUp);
                _runButton.UnregisterCallback<PointerLeaveEvent>(OnRunButtonPointerLeave);
            }

            if (_jumpButton != null)
            {
                _jumpButton.UnregisterCallback<PointerDownEvent>(OnJumpButtonPressed);
            }

            if (_skillsButton != null)
            {
                _skillsButton.clicked -= OnSkillsButtonClicked;
            }

            if (_inventoryButton != null)
            {
                _inventoryButton.clicked -= OnInventoryButtonClicked;
            }

            if (_pauseButton != null)
            {
                _pauseButton.clicked -= OnPauseButtonClicked;
            }

            if (_questsButton != null)
            {
                _questsButton.clicked -= OnQuestsButtonClicked;
            }

            for (int i = 0; i < SkillSlotCount; i++)
            {
                _skillSlotButtons[i]?.UnregisterCallback(_skillSlotPressedCallbacks[i]);
            }

            _skillHotbarRefresh?.Pause();
            _skillHotbarRefresh = null;
            UnsubscribeFromLoadout();
            UnbindShortcuts();

            if (_playerInput != null)
            {
                _playerInput.TouchRunHeld = false;
            }
        }

        private void OnJumpButtonPressed(PointerDownEvent evt)
        {
            if (_playerInput != null)
            {
                _playerInput.TouchJumpRequested = true;
            }
        }

        // ---------------------------------------------------------------
        // Skill hotbar
        // ---------------------------------------------------------------

        private void OnSkillSlotPressed(int slotIndex)
        {
            if (ResolveSkillHotbar() && _skillHotbar.TryCastSlot(slotIndex))
            {
                RefreshSkillHotbar();
            }
        }

        /// <summary>
        /// Lazily resolves the player's <see cref="SkillHotbar"/> so the HUD works without extra
        /// scene wiring: falls back to _playerInput's GameObject, adding the component there if the
        /// player has a SkillManager but nobody added a SkillHotbar in the scene yet.
        /// </summary>
        private bool ResolveSkillHotbar()
        {
            if (_skillHotbar != null)
            {
                return true;
            }

            if (_playerInput == null)
            {
                return false;
            }

            _skillHotbar = _playerInput.GetComponent<SkillHotbar>();

            if (_skillHotbar == null && _playerInput.GetComponent<SkillManager>() != null)
            {
                _skillHotbar = _playerInput.gameObject.AddComponent<SkillHotbar>();
            }

            if (_skillHotbar != null)
            {
                SubscribeToLoadout();
            }

            return _skillHotbar != null;
        }

        private void SubscribeToLoadout()
        {
            if (_skillHotbar == null || _subscribedLoadout != null)
            {
                return;
            }

            _subscribedLoadout = _skillHotbar.Loadout;
            _subscribedLoadout.OnChanged += RefreshSkillHotbar;
        }

        private void UnsubscribeFromLoadout()
        {
            if (_subscribedLoadout != null)
            {
                _subscribedLoadout.OnChanged -= RefreshSkillHotbar;
                _subscribedLoadout = null;
            }
        }

        private void RefreshSkillHotbar()
        {
            bool hasHotbar = ResolveSkillHotbar();

            // Cheap retry: the player (and its PlayerInput) may spawn after the HUD.
            TryBindShortcuts();

            int charsPerLine = SkillSlotDisplayMath.SlotNameCharsPerLine(CurrentFontScale());

            if (charsPerLine != _skillSlotNameCharsPerLine)
            {
                // Font scale changed in Settings: re-format every slot name for the new width budget.
                _skillSlotNameCharsPerLine = charsPerLine;
                Array.Clear(_skillSlotShownSkill, 0, _skillSlotShownSkill.Length);
            }

            for (int i = 0; i < SkillSlotCount; i++)
            {
                Button button = _skillSlotButtons[i];

                if (button == null)
                {
                    continue;
                }

                SkillInstance instance = hasHotbar ? _skillHotbar.GetSlotSkill(i) : null;
                SkillDefinition skill = instance?.Definition;

                if (skill != _skillSlotShownSkill[i])
                {
                    _skillSlotShownSkill[i] = skill;
                    ApplySlotSkill(i, skill);
                }

                float remaining = instance != null ? instance.CooldownRemaining : 0f;
                float duration = instance != null && instance.CooldownDuration > 0f
                    ? instance.CooldownDuration
                    : (skill != null ? skill.CooldownSeconds : 0f);
                bool onCooldown = remaining > 0f;
                bool cannotAfford = skill != null && !_skillHotbar.SkillManager.CanAffordSkill(skill);

                if (_skillSlotCooldowns[i] != null)
                {
                    _skillSlotCooldowns[i].style.height = new Length(SkillSlotDisplayMath.ComputeCooldownPercent(remaining, duration), LengthUnit.Percent);
                }

                int labelKey = SkillSlotDisplayMath.CooldownLabelKey(remaining);

                if (_skillSlotCooldownLabels[i] != null && labelKey != _skillSlotCooldownLabelKey[i])
                {
                    _skillSlotCooldownLabelKey[i] = labelKey;
                    _skillSlotCooldownLabels[i].text = SkillSlotDisplayMath.FormatCooldownLabel(remaining);
                }

                button.EnableInClassList("hud-skill-button--empty", skill == null);
                button.EnableInClassList("hud-skill-button--cooldown", onCooldown);
                button.EnableInClassList("hud-skill-button--no-mana", cannotAfford);
            }
        }

        private void ApplySlotSkill(int slotIndex, SkillDefinition skill)
        {
            Sprite icon = skill != null ? skill.Icon : null;

            if (_skillSlotIcons[slotIndex] != null)
            {
                _skillSlotIcons[slotIndex].style.backgroundImage = icon != null
                    ? new StyleBackground(icon)
                    : new StyleBackground(StyleKeyword.None);
            }

            Label nameLabel = _skillSlotNames[slotIndex];

            if (nameLabel != null)
            {
                // Name is the fallback when a skill has no icon yet (many seeded skills don't).
                // Pre-split into whole-word lines so it never wraps mid-word inside the 64px slot.
                nameLabel.text = skill != null
                    ? SkillSlotDisplayMath.FormatSlotName(skill.DisplayName, _skillSlotNameCharsPerLine)
                    : string.Empty;
                nameLabel.EnableInClassList("hud-skill-name--hidden", icon != null);
            }
        }

        private static float CurrentFontScale()
        {
            return ServiceLocator.TryGet(out AccessibilitySettings settings) && settings != null ? settings.FontScale : 1f;
        }

        // ---------------------------------------------------------------
        // Screens (exclusive) + pause
        // ---------------------------------------------------------------

        private void RegisterScreens()
        {
            _screens.Register(SkillTreeScreenId,
                () => ResolveSkillTree() && _skillTreeController.IsVisible,
                () => { if (ResolveSkillTree()) _skillTreeController.Show(); },
                () => { if (ResolveSkillTree()) _skillTreeController.Hide(); });

            _screens.Register(InventoryScreenId,
                () => ResolveInventory() && _inventoryController.IsVisible,
                () => { if (ResolveInventory()) _inventoryController.Show(); },
                () => { if (ResolveInventory()) _inventoryController.Hide(); });

            _screens.Register(QuestLogScreenId,
                () => ResolveQuestLog() && _questLogController.IsOpen,
                () => { if (ResolveQuestLog()) _questLogController.Open(); },
                () => { if (ResolveQuestLog()) _questLogController.Hide(); });

            _screens.Register(AchievementsScreenId,
                () => ResolveAchievements() && _achievementListController.IsOpen,
                () => { if (ResolveAchievements()) _achievementListController.Open(); },
                () => { if (ResolveAchievements()) _achievementListController.Hide(); });

            _screens.Register(CraftingScreenId,
                () => ResolveCrafting() && _craftingController.IsOpen,
                () => { if (ResolveCrafting()) _craftingController.Open(); },
                () => { if (ResolveCrafting()) _craftingController.Close(); });

            // Open only shows an already-started NPC session (a shop can't be opened from the HUD);
            // Close ends the session, so opening e.g. Mochila over the shop doesn't leave it running.
            _screens.Register(ShopScreenId,
                () => ResolveShop() && _shopController.IsOpen,
                () => { if (ResolveShop()) _shopController.Open(); },
                () => { if (ResolveShop()) _shopController.Close(); });
        }

        // Serialized ref first, FindFirstObjectByType fallback (same pattern as the Mochila button had).
        private bool ResolveSkillTree()
        {
            if (_skillTreeController == null) _skillTreeController = FindFirstObjectByType<SkillTreeController>();
            return _skillTreeController != null;
        }

        private bool ResolveInventory()
        {
            if (_inventoryController == null) _inventoryController = FindFirstObjectByType<InventoryUIController>();
            return _inventoryController != null;
        }

        private bool ResolveQuestLog()
        {
            if (_questLogController == null) _questLogController = FindFirstObjectByType<QuestLogController>();
            return _questLogController != null;
        }

        private bool ResolveAchievements()
        {
            if (_achievementListController == null) _achievementListController = FindFirstObjectByType<AchievementListController>();
            return _achievementListController != null;
        }

        private bool ResolveCrafting()
        {
            if (_craftingController == null) _craftingController = FindFirstObjectByType<CraftingUIController>();
            return _craftingController != null;
        }

        private bool ResolveShop()
        {
            if (_shopController == null) _shopController = FindFirstObjectByType<ShopUIController>();
            return _shopController != null;
        }

        private bool ResolvePauseMenu()
        {
            if (_pauseMenuController == null) _pauseMenuController = FindFirstObjectByType<PauseMenuController>();
            return _pauseMenuController != null;
        }

        /// <summary>
        /// True when something other than one of our screens froze the game — the pause menu or the
        /// death screen. Modal screens may themselves hold a pause (GameplayPause.Acquire), so
        /// "paused while one of our screens is open" is NOT treated as blocked.
        /// </summary>
        private bool IsBlockedByForeignPause()
        {
            if (ResolvePauseMenu() && _pauseMenuController.IsOpen)
            {
                return true;
            }

            return _gamePaused && !_screens.IsAnyOpen;
        }

        /// <summary>Toggles a screen exclusively. Ignored while the pause menu / death screen own the screen.</summary>
        private void ToggleScreen(string screenId)
        {
            if (IsBlockedByForeignPause())
            {
                return;
            }

            _screens.Toggle(screenId);
        }

        private void OnSkillsButtonClicked() => ToggleScreen(SkillTreeScreenId);
        private void OnInventoryButtonClicked() => ToggleScreen(InventoryScreenId);
        private void OnQuestsButtonClicked() => ToggleScreen(QuestLogScreenId);

        private void OnPauseButtonClicked()
        {
            if (!ResolvePauseMenu())
            {
                return;
            }

            if (_pauseMenuController.IsOpen)
            {
                _pauseMenuController.Hide();
                return;
            }

            if (IsBlockedByForeignPause())
            {
                // Paused by something else (death screen): don't stack the pause menu on top of it.
                return;
            }

            // Show first so a screen holding a GameplayPause releases it only once the menu paused.
            _pauseMenuController.Show();
            _screens.CloseAll();
        }

        /// <summary>Esc / gamepad Start: back out of the pause menu, else close the open screen, else pause.</summary>
        private void OnPauseShortcut()
        {
            if (ResolvePauseMenu() && _pauseMenuController.IsOpen)
            {
                _pauseMenuController.HandleBack();
                return;
            }

            if (_screens.CloseAll() > 0 || _gamePaused)
            {
                return;
            }

            _pauseMenuController?.Show();
        }

        /// <summary>
        /// Opens a registered screen exclusively (closing the others) from outside the HUD, e.g. a
        /// crafting bancada (<c>CraftingStationTrigger</c>). <paramref name="openOverride"/> replaces
        /// the registered open when the caller needs parameters (station, inventory). Not blocked by
        /// the pause menu/death screen: world triggers can't fire while time is frozen anyway.
        /// Returns false for an unknown id (the override is not run — fall back to opening directly).
        /// </summary>
        public bool OpenScreenExclusive(string screenId, Action openOverride = null)
        {
            return _screens.Open(screenId, openOverride);
        }

        /// <summary>ShopUIController shows itself on the same event; this only closes the other screens.</summary>
        private void OnShopOpened(ShopOpenedEvent evt) => _screens.Open(ShopScreenId);

        private void OnGamePaused(GamePausedEvent evt) => _gamePaused = true;
        private void OnGameResumed(GameResumedEvent evt) => _gamePaused = false;

        // ---------------------------------------------------------------
        // Keyboard / gamepad shortcuts
        // ---------------------------------------------------------------

        /// <summary>
        /// Hooks the QuestLog/Inventory/SkillTree/Pause actions of the player's PlayerInput.
        /// FindAction (not the indexer) so an actions asset without them just leaves the shortcuts
        /// unbound, same as PlayerInputHandler does for Skill1..5. Retried (throttled) from the
        /// hotbar refresh until the player exists — it may spawn after the HUD.
        /// </summary>
        private void TryBindShortcuts()
        {
            if (_shortcutsBound || !isActiveAndEnabled || Time.unscaledTime < _nextShortcutBindAttempt)
            {
                return;
            }

            _nextShortcutBindAttempt = Time.unscaledTime + ShortcutBindRetrySeconds;

            if (_playerInput == null)
            {
                _playerInput = FindFirstObjectByType<PlayerInputHandler>();
            }

            PlayerInput playerInput = _playerInput != null ? _playerInput.GetComponent<PlayerInput>() : null;
            InputActionAsset actions = playerInput != null ? playerInput.actions : null;

            if (actions == null)
            {
                return;
            }

            _questLogAction = actions.FindAction(QuestLogActionName, throwIfNotFound: false);
            _inventoryAction = actions.FindAction(InventoryActionName, throwIfNotFound: false);
            _skillTreeAction = actions.FindAction(SkillTreeActionName, throwIfNotFound: false);
            _pauseAction = actions.FindAction(PauseActionName, throwIfNotFound: false);

            if (_questLogAction != null) _questLogAction.performed += OnQuestLogActionPerformed;
            if (_inventoryAction != null) _inventoryAction.performed += OnInventoryActionPerformed;
            if (_skillTreeAction != null) _skillTreeAction.performed += OnSkillTreeActionPerformed;
            if (_pauseAction != null) _pauseAction.performed += OnPauseActionPerformed;

            _shortcutsBound = true;
        }

        private void UnbindShortcuts()
        {
            if (_questLogAction != null) _questLogAction.performed -= OnQuestLogActionPerformed;
            if (_inventoryAction != null) _inventoryAction.performed -= OnInventoryActionPerformed;
            if (_skillTreeAction != null) _skillTreeAction.performed -= OnSkillTreeActionPerformed;
            if (_pauseAction != null) _pauseAction.performed -= OnPauseActionPerformed;

            _questLogAction = null;
            _inventoryAction = null;
            _skillTreeAction = null;
            _pauseAction = null;
            _shortcutsBound = false;
            _nextShortcutBindAttempt = 0f;
        }

        private void OnQuestLogActionPerformed(InputAction.CallbackContext context) => ToggleScreen(QuestLogScreenId);
        private void OnInventoryActionPerformed(InputAction.CallbackContext context) => ToggleScreen(InventoryScreenId);
        private void OnSkillTreeActionPerformed(InputAction.CallbackContext context) => ToggleScreen(SkillTreeScreenId);
        private void OnPauseActionPerformed(InputAction.CallbackContext context) => OnPauseShortcut();

        private void OnRunButtonPressed(PointerDownEvent evt)
        {
            if (_playerInput != null)
            {
                _playerInput.TouchRunHeld = true;
            }

            _runButton?.AddToClassList("hud-touch-button--active");
        }

        private void OnRunButtonPointerUp(PointerUpEvent evt) => ReleaseRunButton();
        private void OnRunButtonPointerLeave(PointerLeaveEvent evt) => ReleaseRunButton();

        private void ReleaseRunButton()
        {
            if (_playerInput != null)
            {
                _playerInput.TouchRunHeld = false;
            }

            _runButton?.RemoveFromClassList("hud-touch-button--active");
        }

        private void InitializeFromCurrentStats()
        {
            if (_playerStats == null)
            {
                return;
            }

            UpdateHealthBar(_playerStats.CurrentHealth, _playerStats.MaxHealth);
            UpdateStaminaBar(_playerStats.CurrentStamina, _playerStats.MaxStamina);
            UpdateManaBar(_playerStats.CurrentMana, _playerStats.MaxMana);
            UpdateXpBar(_playerStats.CurrentExperience, _playerStats.GetExperienceToNextLevel(), _playerStats.Level);
        }

        private void OnHealthChanged(PlayerHealthChangedEvent evt) => UpdateHealthBar(evt.CurrentHealth, evt.MaxHealth);
        private void OnStaminaChanged(PlayerStaminaChangedEvent evt) => UpdateStaminaBar(evt.CurrentStamina, evt.MaxStamina);
        private void OnManaChanged(PlayerManaChangedEvent evt) => UpdateManaBar(evt.CurrentMana, evt.MaxMana);
        private void OnExperienceChanged(PlayerExperienceChangedEvent evt) => UpdateXpBar(evt.CurrentExperience, evt.ExperienceToNextLevel, evt.Level);

        private void UpdateHealthBar(float current, float max)
        {
            SetBarFill(_healthFill, _healthValueLabel, current, max);
        }

        private void UpdateStaminaBar(float current, float max)
        {
            SetBarFill(_staminaFill, _staminaValueLabel, current, max);
        }

        private void UpdateManaBar(float current, float max)
        {
            SetBarFill(_manaFill, _manaValueLabel, current, max);
        }

        private void UpdateXpBar(int current, int toNextLevel, int level)
        {
            SetBarFill(_xpFill, _xpValueLabel, current, toNextLevel);

            _displayedLevel = level;
            UpdateLevelLabel();
        }

        private void UpdateLevelLabel()
        {
            if (_levelLabel != null)
            {
                _levelLabel.text = UiTexts.Format(_localizer.Manager, UiTexts.HudLevel, _displayedLevel);
            }
        }

        private void SetBarFill(VisualElement fill, Label valueLabel, float current, float max)
        {
            if (fill == null)
            {
                return;
            }

            float percent = HudBarMath.ComputeFillPercent(current, max);
            fill.style.width = new Length(percent, LengthUnit.Percent);

            if (HudBarMath.IsLow(current, max, _lowValueThreshold))
            {
                fill.AddToClassList("hud-bar-fill--low");
            }
            else
            {
                fill.RemoveFromClassList("hud-bar-fill--low");
            }

            if (valueLabel != null)
            {
                valueLabel.text = HudBarMath.FormatValueLabel(current, max);
            }
        }

        /// <summary>
        /// Applies <see cref="_hudScale"/>/<see cref="_anchorPreset"/>/<see cref="_anchorOffset"/>
        /// to the HUD root. Public entry points below let a future Settings screen drive this at
        /// runtime (e.g. an accessibility "UI scale" slider) without needing a new component.
        /// </summary>
        private void ApplyAdaptiveLayout()
        {
            if (_root == null)
            {
                return;
            }

            _root.transform.scale = new Vector3(_hudScale, _hudScale, 1f);

            Vector4 safeInset = ComputeSafeAreaInset();

            float left = safeInset.x;
            float top = safeInset.y;
            float right = safeInset.z;
            float bottom = safeInset.w;

            switch (_anchorPreset)
            {
                case HudAnchorPreset.TopLeft:
                case HudAnchorPreset.Custom:
                    left += _anchorOffset.x;
                    top += _anchorOffset.y;
                    break;
                case HudAnchorPreset.TopRight:
                    right += _anchorOffset.x;
                    top += _anchorOffset.y;
                    break;
                case HudAnchorPreset.BottomLeft:
                    left += _anchorOffset.x;
                    bottom += _anchorOffset.y;
                    break;
                case HudAnchorPreset.BottomRight:
                    right += _anchorOffset.x;
                    bottom += _anchorOffset.y;
                    break;
            }

            // All four sides are always set (not just the two the preset "anchors" to) — every
            // corner of the HUD (vitals top-left, XP bar bottom-left, Run/Jump bottom-right) is
            // positioned absolute relative to hud-root's own box, so every edge needs the safe-area
            // inset, not just whichever two sides the corner preset happens to touch. This also
            // means hud-root's box is now fully defined by its 4 offsets, so the USS width:100%/
            // height:100% no longer applies (Yoga resolves size from left+right / top+bottom instead).
            _root.style.left = left;
            _root.style.top = top;
            _root.style.right = right;
            _root.style.bottom = bottom;
        }

        /// <summary>
        /// Converts <see cref="Screen.safeArea"/> (device screen pixels) into panel-space insets
        /// (left, top, right, bottom) so HUD content stays clear of notches/camera cutouts/rounded
        /// corners/home-indicator bars on real phones. In the Editor (and any device reporting a
        /// full-screen safe area, i.e. no notch) this always returns zero insets — genuinely
        /// exercising the non-zero path needs a real notched device, which this environment
        /// doesn't have; the math mirrors Unity's documented safe-area pattern.
        /// </summary>
        private Vector4 ComputeSafeAreaInset()
        {
            if (_root == null || _root.panel == null)
            {
                return Vector4.zero;
            }

            float pixelsPerPoint = _root.panel.scaledPixelsPerPoint;

            if (pixelsPerPoint <= 0f)
            {
                return Vector4.zero;
            }

            Rect safeArea = Screen.safeArea;

            float left = safeArea.xMin / pixelsPerPoint;
            float bottom = safeArea.yMin / pixelsPerPoint;
            float right = (Screen.width - safeArea.xMax) / pixelsPerPoint;
            float top = (Screen.height - safeArea.yMax) / pixelsPerPoint;

            return new Vector4(left, top, right, bottom);
        }

        /// <summary>Runtime entry point for a future Settings/accessibility screen.</summary>
        public void SetHudScale(float scale)
        {
            _hudScale = Mathf.Clamp(scale, 0.5f, 2f);
            ApplyAdaptiveLayout();
        }

        /// <summary>Runtime entry point for a future Settings/accessibility screen (HUD position).</summary>
        public void SetAnchor(HudAnchorPreset preset, Vector2 offset)
        {
            _anchorPreset = preset;
            _anchorOffset = offset;
            ApplyAdaptiveLayout();
        }
    }
}
