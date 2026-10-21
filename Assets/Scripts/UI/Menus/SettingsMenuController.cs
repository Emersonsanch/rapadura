using System.Collections.Generic;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Performance;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Settings screen (gráficos / áudio / controles / idioma / acessibilidade), built with
    /// UI Toolkit following the same pattern as the rest of Assets/Scripts/UI. Five tabs share one panel host;
    /// only one tab's content is visible at a time (a plain <see cref="MenuNavigationStack"/> tracks
    /// which one, so the "which tab is active" rule is unit-testable independent of UI Toolkit).
    ///
    /// - Idioma: calls <see cref="LocalizationManager.SetLanguage"/> (resolved via ServiceLocator,
    ///   same as GameManager registers it — LocalizationManager.cs is not edited here).
    /// - Áudio: calls <see cref="AudioManager.SetCategoryVolume"/>/<see cref="AudioManager.GetCategoryVolume"/>
    ///   per <see cref="AudioCategory"/>, resolved the same way (AudioManager.cs is not edited here).
    /// - Gráficos: thin wrapper over Unity's own QualitySettings/Screen APIs, plus the "60 FPS em
    ///   combate" toggle delegating to <see cref="FrameRateManager.SetHighFrameRateInCombat"/>
    ///   (persisted by the manager itself).
    /// - Controles: runtime rebinding through <see cref="InputRebindManager"/> over the active
    ///   <see cref="PlayerInput"/>'s actions asset (PlayerControls.inputactions), falling back to the
    ///   serialized <c>_inputActions</c> when no player exists yet (e.g. main menu). One row per
    ///   <see cref="SettingsMenuTexts.Rows"/> entry with a keyboard and a gamepad button (column
    ///   chosen by <see cref="InputBindingUtility.FindBindingIndex"/>), "press a key" interactive
    ///   rebind restricted to that column's device, Esc / Cancelar to abort, a conflict warning
    ///   (<see cref="InputBindingUtility.FindConflicts"/>), per-row and global reset. Every change is
    ///   persisted immediately via <see cref="InputRebindManager.SaveBindingOverrides"/> (PlayerPrefs);
    ///   <see cref="InputRebindBootstrap"/> re-applies saved overrides on scene load.
    /// - Acessibilidade: font scale / high contrast / colorblind mode / screen shake / reduce
    ///   flashes, written to <see cref="AccessibilitySettings"/>; <see cref="UiAccessibilityApplier"/>
    ///   applies the visual ones to every UIDocument.
    /// Labels created from code use <see cref="SettingsMenuTexts"/> (LocalizationManager with pt-BR fallback).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SettingsMenuController : MonoBehaviour
    {
        private const string TabGraphics = "graphics";
        private const string TabAudio = "audio";
        private const string TabControls = "controls";
        private const string TabLanguage = "language";
        private const string TabAccessibility = "accessibility";

        private const float FontScaleStep = 0.05f;

        private const string RebindRowClass = "settings-rebind-row";
        private const string RebindActionLabelClass = "settings-rebind-action-label";
        private const string RebindButtonClass = "settings-rebind-button";
        private const string RebindButtonOverriddenClass = "settings-rebind-button--overridden";
        private const string RebindButtonListeningClass = "settings-rebind-button--listening";
        private const string RebindResetButtonClass = "settings-rebind-reset-button";
        private const string RebindStatusWarningClass = "settings-rebind-status--warning";

        [Tooltip("Fallback actions asset (PlayerControls.inputactions) used when no PlayerInput is active, e.g. in the main menu.")]
        [SerializeField] private InputActionAsset _inputActions;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _closeButton;

        private Button _tabGraphicsButton;
        private Button _tabAudioButton;
        private Button _tabControlsButton;
        private Button _tabLanguageButton;
        private Button _tabAccessibilityButton;

        private VisualElement _panelGraphics;
        private VisualElement _panelAudio;
        private VisualElement _panelControls;
        private VisualElement _panelLanguage;
        private VisualElement _panelAccessibility;

        // Graphics
        private DropdownField _qualityDropdown;
        private Toggle _fullscreenToggle;
        private Toggle _highFpsCombatToggle;

        // Audio
        private Slider _masterVolumeSlider;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _uiVolumeSlider;

        // Language
        private DropdownField _languageDropdown;

        // Controls (rebind)
        private ScrollView _controlsRowsContainer;
        private Label _controlsHeaderAction;
        private Label _controlsHeaderKeyboard;
        private Label _controlsHeaderGamepad;
        private Label _controlsStatusLabel;
        private Button _controlsCancelButton;
        private Button _controlsResetAllButton;

        private sealed class RebindRowView
        {
            public RebindRowDefinition Definition;
            public InputAction Action;
            public int KeyboardIndex = -1;
            public int GamepadIndex = -1;
            public Label Label;
            public Button KeyboardButton;
            public Button GamepadButton;
            public Button ResetButton;
        }

        private readonly List<RebindRowView> _rebindRows = new List<RebindRowView>();
        private InputActionAsset _boundActions;
        private InputRebindManager _rebindManager;

        /// <summary>Read by PauseMenuController.HandleBack (Esc/Start closes this before the pause menu).</summary>
        public bool IsOpen => _root != null && _root.style.display == DisplayStyle.Flex;

        /// <summary>True while a key/button rebind is listening — Esc then belongs to the rebind (cancel), not to "back".</summary>
        public bool IsRebinding => _rebindManager != null && _rebindManager.IsRebinding;
        private RebindRowView _listeningRow;
        private BindingDeviceKind _listeningKind;

        // Accessibility
        private Slider _fontScaleSlider;
        private Toggle _highContrastToggle;
        private DropdownField _colorblindDropdown;
        private Slider _screenShakeSlider;
        private Toggle _reduceFlashToggle;
        private readonly List<ColorblindMode> _colorblindChoiceModes = new List<ColorblindMode>();

        private readonly MenuNavigationStack _tabStack = new MenuNavigationStack();

        private AudioManager _audioManager;
        private LocalizationManager _localizationManager;
        private LocalizationManager _subscribedLocalization;
        private AccessibilitySettings _accessibility;
        private FrameRateManager _frameRateManager;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            ServiceLocator.TryGet(out _audioManager);
            ServiceLocator.TryGet(out _localizationManager);
            ServiceLocator.TryGet(out _accessibility);
            ServiceLocator.TryGet(out _frameRateManager);
            SubscribeLocalization();

            BindElements();
            RegisterCallbacks();
            InitializeGraphicsPanel();
            InitializeAudioPanel();
            InitializeLanguagePanel();
            InitializeAccessibilityPanel();
            InitializeControlsPanel();

            _tabStack.Reset(TabGraphics);
            ApplyActiveTab();

            Hide();
        }

        private void OnDisable()
        {
            CancelRebind();
            UnregisterCallbacks();
            UnsubscribeLocalization();
        }

        private void Update()
        {
            // Backup for the rebind operation's own WithCancelingThrough(Esc): guarantees Esc always
            // aborts, whatever the operation's include filter (e.g. gamepad-only column).
            if (_rebindManager != null && _rebindManager.IsRebinding)
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                {
                    CancelRebind();
                }
            }
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("settings-menu-root");
            _closeButton = rootVisualElement.Q<Button>("close-button");

            _tabGraphicsButton = rootVisualElement.Q<Button>("tab-graphics-button");
            _tabAudioButton = rootVisualElement.Q<Button>("tab-audio-button");
            _tabControlsButton = rootVisualElement.Q<Button>("tab-controls-button");
            _tabLanguageButton = rootVisualElement.Q<Button>("tab-language-button");
            _tabAccessibilityButton = rootVisualElement.Q<Button>("tab-accessibility-button");

            _panelGraphics = rootVisualElement.Q<VisualElement>("panel-graphics");
            _panelAudio = rootVisualElement.Q<VisualElement>("panel-audio");
            _panelControls = rootVisualElement.Q<VisualElement>("panel-controls");
            _panelLanguage = rootVisualElement.Q<VisualElement>("panel-language");
            _panelAccessibility = rootVisualElement.Q<VisualElement>("panel-accessibility");

            _qualityDropdown = rootVisualElement.Q<DropdownField>("quality-dropdown");
            _fullscreenToggle = rootVisualElement.Q<Toggle>("fullscreen-toggle");
            _highFpsCombatToggle = rootVisualElement.Q<Toggle>("high-fps-combat-toggle");

            _masterVolumeSlider = rootVisualElement.Q<Slider>("master-volume-slider");
            _musicVolumeSlider = rootVisualElement.Q<Slider>("music-volume-slider");
            _sfxVolumeSlider = rootVisualElement.Q<Slider>("sfx-volume-slider");
            _uiVolumeSlider = rootVisualElement.Q<Slider>("ui-volume-slider");

            _languageDropdown = rootVisualElement.Q<DropdownField>("language-dropdown");

            _controlsRowsContainer = rootVisualElement.Q<ScrollView>("controls-rows");
            _controlsHeaderAction = rootVisualElement.Q<Label>("controls-header-action");
            _controlsHeaderKeyboard = rootVisualElement.Q<Label>("controls-header-keyboard");
            _controlsHeaderGamepad = rootVisualElement.Q<Label>("controls-header-gamepad");
            _controlsStatusLabel = rootVisualElement.Q<Label>("controls-status-label");
            _controlsCancelButton = rootVisualElement.Q<Button>("controls-cancel-button");
            _controlsResetAllButton = rootVisualElement.Q<Button>("controls-reset-all-button");

            _fontScaleSlider = rootVisualElement.Q<Slider>("font-scale-slider");
            _highContrastToggle = rootVisualElement.Q<Toggle>("high-contrast-toggle");
            _colorblindDropdown = rootVisualElement.Q<DropdownField>("colorblind-dropdown");
            _screenShakeSlider = rootVisualElement.Q<Slider>("screen-shake-slider");
            _reduceFlashToggle = rootVisualElement.Q<Toggle>("reduce-flash-toggle");
        }

        private void RegisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked += Hide;

            if (_tabGraphicsButton != null) _tabGraphicsButton.clicked += () => SwitchTab(TabGraphics);
            if (_tabAudioButton != null) _tabAudioButton.clicked += () => SwitchTab(TabAudio);
            if (_tabControlsButton != null) _tabControlsButton.clicked += () => SwitchTab(TabControls);
            if (_tabLanguageButton != null) _tabLanguageButton.clicked += () => SwitchTab(TabLanguage);
            if (_tabAccessibilityButton != null) _tabAccessibilityButton.clicked += () => SwitchTab(TabAccessibility);

            if (_qualityDropdown != null) _qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);
            if (_fullscreenToggle != null) _fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);
            if (_highFpsCombatToggle != null) _highFpsCombatToggle.RegisterValueChangedCallback(OnHighFpsCombatChanged);

            if (_masterVolumeSlider != null) _masterVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Master, evt.newValue));
            if (_musicVolumeSlider != null) _musicVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Music, evt.newValue));
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Sfx, evt.newValue));
            if (_uiVolumeSlider != null) _uiVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Ui, evt.newValue));

            if (_languageDropdown != null) _languageDropdown.RegisterValueChangedCallback(OnLanguageChanged);

            if (_controlsCancelButton != null) _controlsCancelButton.clicked += CancelRebind;
            if (_controlsResetAllButton != null) _controlsResetAllButton.clicked += ResetAllBindings;

            if (_fontScaleSlider != null) _fontScaleSlider.RegisterValueChangedCallback(OnFontScaleChanged);
            if (_highContrastToggle != null) _highContrastToggle.RegisterValueChangedCallback(OnHighContrastChanged);
            if (_colorblindDropdown != null) _colorblindDropdown.RegisterValueChangedCallback(OnColorblindModeChanged);
            if (_screenShakeSlider != null) _screenShakeSlider.RegisterValueChangedCallback(OnScreenShakeChanged);
            if (_reduceFlashToggle != null) _reduceFlashToggle.RegisterValueChangedCallback(OnReduceFlashChanged);
        }

        private void UnregisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked -= Hide;

            if (_qualityDropdown != null) _qualityDropdown.UnregisterValueChangedCallback(OnQualityChanged);
            if (_fullscreenToggle != null) _fullscreenToggle.UnregisterValueChangedCallback(OnFullscreenChanged);
            if (_highFpsCombatToggle != null) _highFpsCombatToggle.UnregisterValueChangedCallback(OnHighFpsCombatChanged);
            if (_languageDropdown != null) _languageDropdown.UnregisterValueChangedCallback(OnLanguageChanged);

            if (_controlsCancelButton != null) _controlsCancelButton.clicked -= CancelRebind;
            if (_controlsResetAllButton != null) _controlsResetAllButton.clicked -= ResetAllBindings;

            if (_fontScaleSlider != null) _fontScaleSlider.UnregisterValueChangedCallback(OnFontScaleChanged);
            if (_highContrastToggle != null) _highContrastToggle.UnregisterValueChangedCallback(OnHighContrastChanged);
            if (_colorblindDropdown != null) _colorblindDropdown.UnregisterValueChangedCallback(OnColorblindModeChanged);
            if (_screenShakeSlider != null) _screenShakeSlider.UnregisterValueChangedCallback(OnScreenShakeChanged);
            if (_reduceFlashToggle != null) _reduceFlashToggle.UnregisterValueChangedCallback(OnReduceFlashChanged);
        }

        // ------------------------------------------------------------------
        // Tabs
        // ------------------------------------------------------------------

        private void SwitchTab(string tab)
        {
            if (tab != TabControls)
            {
                CancelRebind();
            }

            _tabStack.Reset(tab);
            ApplyActiveTab();
        }

        private void ApplyActiveTab()
        {
            string active = _tabStack.Current;

            SetPanelVisible(_panelGraphics, active == TabGraphics);
            SetPanelVisible(_panelAudio, active == TabAudio);
            SetPanelVisible(_panelControls, active == TabControls);
            SetPanelVisible(_panelLanguage, active == TabLanguage);
            SetPanelVisible(_panelAccessibility, active == TabAccessibility);

            SetTabSelected(_tabGraphicsButton, active == TabGraphics);
            SetTabSelected(_tabAudioButton, active == TabAudio);
            SetTabSelected(_tabControlsButton, active == TabControls);
            SetTabSelected(_tabLanguageButton, active == TabLanguage);
            SetTabSelected(_tabAccessibilityButton, active == TabAccessibility);
        }

        private static void SetPanelVisible(VisualElement panel, bool visible)
        {
            if (panel != null)
            {
                panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private static void SetTabSelected(Button tabButton, bool selected)
        {
            if (tabButton == null)
            {
                return;
            }

            if (selected)
            {
                tabButton.AddToClassList("settings-tab-button--selected");
            }
            else
            {
                tabButton.RemoveFromClassList("settings-tab-button--selected");
            }
        }

        // ------------------------------------------------------------------
        // Graphics
        // ------------------------------------------------------------------

        private void InitializeGraphicsPanel()
        {
            if (_qualityDropdown != null)
            {
                _qualityDropdown.choices = new System.Collections.Generic.List<string>(QualitySettings.names);
                int currentIndex = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, _qualityDropdown.choices.Count - 1);

                if (_qualityDropdown.choices.Count > 0)
                {
                    _qualityDropdown.SetValueWithoutNotify(_qualityDropdown.choices[currentIndex]);
                }
            }

            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.SetValueWithoutNotify(Screen.fullScreen);
            }

            if (_highFpsCombatToggle != null)
            {
                _highFpsCombatToggle.label = Text(SettingsMenuTexts.HighFpsCombat);
                _highFpsCombatToggle.SetValueWithoutNotify(_frameRateManager != null && _frameRateManager.HighFrameRateInCombat);
                _highFpsCombatToggle.SetEnabled(_frameRateManager != null);
            }
        }

        private void OnQualityChanged(ChangeEvent<string> evt)
        {
            int index = _qualityDropdown.choices.IndexOf(evt.newValue);

            if (index >= 0)
            {
                QualitySettings.SetQualityLevel(index, applyExpensiveChanges: true);
            }
        }

        private void OnFullscreenChanged(ChangeEvent<bool> evt)
        {
            Screen.fullScreen = evt.newValue;
        }

        private void OnHighFpsCombatChanged(ChangeEvent<bool> evt)
        {
            _frameRateManager?.SetHighFrameRateInCombat(evt.newValue);
        }

        // ------------------------------------------------------------------
        // Audio
        // ------------------------------------------------------------------

        private void InitializeAudioPanel()
        {
            SetSliderFromManager(_masterVolumeSlider, AudioCategory.Master);
            SetSliderFromManager(_musicVolumeSlider, AudioCategory.Music);
            SetSliderFromManager(_sfxVolumeSlider, AudioCategory.Sfx);
            SetSliderFromManager(_uiVolumeSlider, AudioCategory.Ui);
        }

        private void SetSliderFromManager(Slider slider, AudioCategory category)
        {
            if (slider == null)
            {
                return;
            }

            float volume = _audioManager != null ? _audioManager.GetCategoryVolume(category) : 1f;
            slider.SetValueWithoutNotify(volume);
        }

        private void OnVolumeChanged(AudioCategory category, float value)
        {
            _audioManager?.SetCategoryVolume(category, value);
        }

        // ------------------------------------------------------------------
        // Language
        // ------------------------------------------------------------------

        private void InitializeLanguagePanel()
        {
            if (_languageDropdown == null)
            {
                return;
            }

            _languageDropdown.choices = new System.Collections.Generic.List<string>
            {
                LanguageCode.en.ToString(),
                LanguageCode.ptBR.ToString(),
            };

            LanguageCode current = _localizationManager != null ? _localizationManager.CurrentLanguage : LanguageCode.en;
            _languageDropdown.SetValueWithoutNotify(current.ToString());
        }

        private void OnLanguageChanged(ChangeEvent<string> evt)
        {
            if (_localizationManager == null)
            {
                return;
            }

            if (System.Enum.TryParse(evt.newValue, out LanguageCode language))
            {
                _localizationManager.SetLanguage(language);
            }
        }

        // ------------------------------------------------------------------
        // Controls (rebind)
        // ------------------------------------------------------------------

        private string Text(string key)
        {
            return SettingsMenuTexts.Get(_localizationManager, key);
        }

        /// <summary>The live actions asset: the active PlayerInput's (so overrides affect the real player), else the serialized fallback.</summary>
        private InputActionAsset ResolveActionsAsset()
        {
            foreach (PlayerInput playerInput in PlayerInput.all)
            {
                if (playerInput != null && playerInput.actions != null)
                {
                    return playerInput.actions;
                }
            }

            return _inputActions;
        }

        private void InitializeControlsPanel()
        {
            InputActionAsset asset = ResolveActionsAsset();
            if (asset != _boundActions)
            {
                CancelRebind();
                _boundActions = asset;
                _rebindManager = asset != null ? new InputRebindManager(asset) : null;
                _rebindManager?.LoadBindingOverrides();
            }

            SetLabel(_controlsHeaderAction, Text(SettingsMenuTexts.HeaderAction));
            SetLabel(_controlsHeaderKeyboard, Text(SettingsMenuTexts.HeaderKeyboard));
            SetLabel(_controlsHeaderGamepad, Text(SettingsMenuTexts.HeaderGamepad));
            if (_controlsCancelButton != null) _controlsCancelButton.text = Text(SettingsMenuTexts.ButtonCancel);
            if (_controlsResetAllButton != null)
            {
                _controlsResetAllButton.text = Text(SettingsMenuTexts.ButtonResetAll);
                _controlsResetAllButton.SetEnabled(_rebindManager != null);
            }

            BuildControlsRows();
            UpdateRebindModeVisuals();
            SetStatus(_rebindManager == null ? Text(SettingsMenuTexts.StatusUnavailable) : string.Empty, warning: false);
        }

        private void BuildControlsRows()
        {
            _rebindRows.Clear();
            if (_controlsRowsContainer == null)
            {
                return;
            }

            _controlsRowsContainer.Clear();

            foreach (RebindRowDefinition definition in SettingsMenuTexts.Rows)
            {
                InputAction action = _boundActions != null ? _boundActions.FindAction(definition.ActionName, throwIfNotFound: false) : null;
                var view = new RebindRowView
                {
                    Definition = definition,
                    Action = action,
                    KeyboardIndex = InputBindingUtility.FindBindingIndex(action, definition.PartName, BindingDeviceKind.Keyboard),
                    GamepadIndex = InputBindingUtility.FindBindingIndex(action, definition.PartName, BindingDeviceKind.Gamepad),
                };

                var row = new VisualElement();
                row.AddToClassList(RebindRowClass);

                view.Label = new Label(Text(definition.LabelKey));
                view.Label.AddToClassList(RebindActionLabelClass);
                row.Add(view.Label);

                view.KeyboardButton = CreateRebindButton(() => StartRebind(view, BindingDeviceKind.Keyboard));
                row.Add(view.KeyboardButton);

                view.GamepadButton = CreateRebindButton(() => StartRebind(view, BindingDeviceKind.Gamepad));
                row.Add(view.GamepadButton);

                view.ResetButton = new Button(() => ResetRow(view)) { text = Text(SettingsMenuTexts.ButtonReset), focusable = true };
                view.ResetButton.AddToClassList(RebindResetButtonClass);
                row.Add(view.ResetButton);

                _controlsRowsContainer.Add(row);
                _rebindRows.Add(view);
            }

            RefreshRebindRows();

            // New rows were created from code: let the font-scale applier pick them up now.
            UiAccessibilityApplier.RequestRefresh();
        }

        private static Button CreateRebindButton(System.Action onClick)
        {
            var button = new Button(onClick) { focusable = true };
            button.AddToClassList(RebindButtonClass);
            return button;
        }

        private void RefreshRebindRows()
        {
            foreach (RebindRowView view in _rebindRows)
            {
                RefreshRebindButton(view, view.KeyboardButton, view.KeyboardIndex, BindingDeviceKind.Keyboard);
                RefreshRebindButton(view, view.GamepadButton, view.GamepadIndex, BindingDeviceKind.Gamepad);

                bool canReset = view.Action != null
                    && (InputBindingUtility.IsOverridden(view.Action, view.KeyboardIndex) || InputBindingUtility.IsOverridden(view.Action, view.GamepadIndex));
                view.ResetButton?.SetEnabled(canReset && _rebindManager != null);
            }
        }

        private void RefreshRebindButton(RebindRowView view, Button button, int bindingIndex, BindingDeviceKind kind)
        {
            if (button == null)
            {
                return;
            }

            bool listening = _listeningRow == view && _listeningKind == kind;
            button.text = listening ? Text(SettingsMenuTexts.Waiting) : InputBindingUtility.GetFormattedDisplay(view.Action, bindingIndex);
            button.SetEnabled(_rebindManager != null && view.Action != null && bindingIndex >= 0);
            button.EnableInClassList(RebindButtonListeningClass, listening);
            button.EnableInClassList(RebindButtonOverriddenClass, InputBindingUtility.IsOverridden(view.Action, bindingIndex));
        }

        private void StartRebind(RebindRowView view, BindingDeviceKind kind)
        {
            if (_rebindManager == null || view?.Action == null)
            {
                return;
            }

            int bindingIndex = kind == BindingDeviceKind.Keyboard ? view.KeyboardIndex : view.GamepadIndex;
            if (bindingIndex < 0)
            {
                return;
            }

            // Cancel any rebind already listening first, so its cancel callback runs before this
            // row's state is set (InputRebindManager would otherwise cancel it from inside the call).
            CancelRebind();

            InputAction action = view.Action;
            string previousOverride = action.bindings[bindingIndex].overridePath;
            string actionLabel = Text(view.Definition.LabelKey);

            _listeningRow = view;
            _listeningKind = kind;
            RefreshRebindRows();
            UpdateRebindModeVisuals();
            SetStatus(SettingsMenuTexts.Format(_localizationManager,
                kind == BindingDeviceKind.Keyboard ? SettingsMenuTexts.StatusPressKey : SettingsMenuTexts.StatusPressButton,
                actionLabel), warning: false);

            _rebindManager.PerformInteractiveRebind(action, bindingIndex,
                onComplete: () => OnRebindComplete(view, action, bindingIndex, previousOverride, actionLabel),
                onCancel: OnRebindCancelled,
                requiredPathPrefixes: InputBindingUtility.GetRebindPathPrefixes(kind));
        }

        private void OnRebindComplete(RebindRowView view, InputAction action, int bindingIndex, string previousOverride, string actionLabel)
        {
            ClearListening();

            // Safety net: if the cancel key itself got bound, treat it as a cancel and restore.
            if (InputBindingUtility.PathsConflict(action.bindings[bindingIndex].effectivePath, InputRebindManager.DefaultCancelPath))
            {
                RestoreOverride(action, bindingIndex, previousOverride);
                OnRebindCancelled();
                return;
            }

            _rebindManager?.SaveBindingOverrides();
            RefreshRebindRows();

            string display = InputBindingUtility.GetFormattedDisplay(action, bindingIndex);
            List<BindingConflict> conflicts = InputBindingUtility.FindConflicts(action, bindingIndex);
            if (conflicts.Count > 0)
            {
                var names = new List<string>(conflicts.Count);
                foreach (BindingConflict conflict in conflicts)
                {
                    names.Add(SettingsMenuTexts.DescribeConflict(_localizationManager, conflict));
                }

                SetStatus(SettingsMenuTexts.Format(_localizationManager, SettingsMenuTexts.StatusConflict, display, string.Join(", ", names)), warning: true);
            }
            else
            {
                SetStatus(SettingsMenuTexts.Format(_localizationManager, SettingsMenuTexts.StatusSaved, actionLabel, display), warning: false);
            }

            Button reboundButton = bindingIndex == view.KeyboardIndex ? view.KeyboardButton : view.GamepadButton;
            reboundButton?.Focus();
        }

        private static void RestoreOverride(InputAction action, int bindingIndex, string previousOverride)
        {
            if (string.IsNullOrEmpty(previousOverride))
            {
                action.RemoveBindingOverride(bindingIndex);
            }
            else
            {
                action.ApplyBindingOverride(bindingIndex, previousOverride);
            }
        }

        private void OnRebindCancelled()
        {
            ClearListening();
            RefreshRebindRows();
            SetStatus(Text(SettingsMenuTexts.StatusCancelled), warning: false);
        }

        private void ClearListening()
        {
            _listeningRow = null;
            UpdateRebindModeVisuals();
        }

        /// <summary>Aborts an in-progress interactive rebind (Esc, Cancelar button, tab switch, Hide).</summary>
        private void CancelRebind()
        {
            if (_rebindManager != null && _rebindManager.IsRebinding)
            {
                _rebindManager.CancelActiveRebind();
            }
        }

        private void ResetRow(RebindRowView view)
        {
            if (_rebindManager == null || view?.Action == null)
            {
                return;
            }

            CancelRebind();
            if (view.KeyboardIndex >= 0) _rebindManager.ResetBinding(view.Action, view.KeyboardIndex);
            if (view.GamepadIndex >= 0) _rebindManager.ResetBinding(view.Action, view.GamepadIndex);
            _rebindManager.SaveBindingOverrides();

            RefreshRebindRows();
            SetStatus(string.Empty, warning: false);
        }

        private void ResetAllBindings()
        {
            if (_rebindManager == null)
            {
                return;
            }

            CancelRebind();
            _rebindManager.ResetAllBindings();
            _rebindManager.SaveBindingOverrides();

            RefreshRebindRows();
            SetStatus(Text(SettingsMenuTexts.StatusResetAll), warning: false);
        }

        private void UpdateRebindModeVisuals()
        {
            if (_controlsCancelButton != null)
            {
                _controlsCancelButton.style.display = _listeningRow != null ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void SetStatus(string text, bool warning)
        {
            if (_controlsStatusLabel == null)
            {
                return;
            }

            _controlsStatusLabel.text = text ?? string.Empty;
            _controlsStatusLabel.EnableInClassList(RebindStatusWarningClass, warning);
        }

        private static void SetLabel(Label label, string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        // ------------------------------------------------------------------
        // Accessibility
        // ------------------------------------------------------------------

        private void InitializeAccessibilityPanel()
        {
            if (_fontScaleSlider != null)
            {
                _fontScaleSlider.lowValue = AccessibilitySettings.MinFontScale;
                _fontScaleSlider.highValue = AccessibilitySettings.MaxFontScale;
                _fontScaleSlider.SetValueWithoutNotify(_accessibility != null ? _accessibility.FontScale : 1f);
            }

            _highContrastToggle?.SetValueWithoutNotify(_accessibility != null && _accessibility.HighContrast);
            _screenShakeSlider?.SetValueWithoutNotify(_accessibility != null ? _accessibility.ScreenShakeIntensity : 1f);
            _reduceFlashToggle?.SetValueWithoutNotify(_accessibility != null && _accessibility.ReduceFlashEffects);

            InitializeColorblindChoices();
        }

        private void InitializeColorblindChoices()
        {
            if (_colorblindDropdown == null)
            {
                return;
            }

            _colorblindChoiceModes.Clear();
            var choices = new List<string>();
            foreach (ColorblindMode mode in (ColorblindMode[])System.Enum.GetValues(typeof(ColorblindMode)))
            {
                _colorblindChoiceModes.Add(mode);
                choices.Add(Text(SettingsMenuTexts.ColorblindKey(mode)));
            }

            _colorblindDropdown.choices = choices;
            ColorblindMode current = _accessibility != null ? _accessibility.ActiveColorblindMode : ColorblindMode.None;
            int index = Mathf.Max(0, _colorblindChoiceModes.IndexOf(current));
            _colorblindDropdown.SetValueWithoutNotify(choices[index]);
        }

        private void OnFontScaleChanged(ChangeEvent<float> evt)
        {
            float snapped = Mathf.Round(evt.newValue / FontScaleStep) * FontScaleStep;
            if (_accessibility != null && !Mathf.Approximately(_accessibility.FontScale, snapped))
            {
                _accessibility.SetFontScale(snapped);
            }
        }

        private void OnHighContrastChanged(ChangeEvent<bool> evt)
        {
            _accessibility?.SetHighContrast(evt.newValue);
        }

        private void OnColorblindModeChanged(ChangeEvent<string> evt)
        {
            int index = _colorblindDropdown.choices.IndexOf(evt.newValue);
            if (_accessibility != null && index >= 0 && index < _colorblindChoiceModes.Count)
            {
                _accessibility.SetColorblindMode(_colorblindChoiceModes[index]);
            }
        }

        private void OnScreenShakeChanged(ChangeEvent<float> evt)
        {
            _accessibility?.SetScreenShakeIntensity(evt.newValue);
        }

        private void OnReduceFlashChanged(ChangeEvent<bool> evt)
        {
            _accessibility?.SetReduceFlashEffects(evt.newValue);
        }

        // ------------------------------------------------------------------
        // Localization refresh
        // ------------------------------------------------------------------

        private void SubscribeLocalization()
        {
            if (_subscribedLocalization == _localizationManager)
            {
                return;
            }

            UnsubscribeLocalization();
            _subscribedLocalization = _localizationManager;
            if (_subscribedLocalization != null)
            {
                _subscribedLocalization.LanguageChanged += OnLanguageSwitched;
            }
        }

        private void UnsubscribeLocalization()
        {
            if (_subscribedLocalization != null)
            {
                _subscribedLocalization.LanguageChanged -= OnLanguageSwitched;
                _subscribedLocalization = null;
            }
        }

        private void OnLanguageSwitched(LanguageCode language)
        {
            InitializeGraphicsPanel();
            InitializeColorblindChoices();
            InitializeControlsPanel();
        }

        // ------------------------------------------------------------------
        // Show/Hide
        // ------------------------------------------------------------------

        public void Show()
        {
            // Same class of bug as ShopUIController/DialogueUIController/SaveLoadMenuController
            // (fixed earlier this session): OnEnable's ServiceLocator.TryGet calls run once at
            // scene start, before GameManager.Awake() is guaranteed to have registered
            // AudioManager/LocalizationManager — left both permanently null so every volume slider
            // and language button on this screen silently did nothing.
            if (_audioManager == null)
            {
                ServiceLocator.TryGet(out _audioManager);
            }
            if (_localizationManager == null)
            {
                ServiceLocator.TryGet(out _localizationManager);
            }
            if (_accessibility == null)
            {
                ServiceLocator.TryGet(out _accessibility);
            }
            if (_frameRateManager == null)
            {
                ServiceLocator.TryGet(out _frameRateManager);
            }
            SubscribeLocalization();

            // Re-read every panel on open: managers may have been resolved only now, and the
            // player (and thus the live actions asset) may have spawned since OnEnable.
            InitializeGraphicsPanel();
            InitializeAudioPanel();
            InitializeLanguagePanel();
            InitializeAccessibilityPanel();
            InitializeControlsPanel();

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            _tabGraphicsButton?.Focus();
        }

        public void Hide()
        {
            CancelRebind();

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }
    }
}
