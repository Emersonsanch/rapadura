using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Settings screen (gráficos / áudio / controles / idioma), built with UI Toolkit
    /// following the same pattern as the rest of Assets/Scripts/UI. Four tabs share one panel host;
    /// only one tab's content is visible at a time (a plain <see cref="MenuNavigationStack"/> tracks
    /// which one, so the "which tab is active" rule is unit-testable independent of UI Toolkit).
    ///
    /// - Idioma: calls <see cref="LocalizationManager.SetLanguage"/> (resolved via ServiceLocator,
    ///   same as GameManager registers it — LocalizationManager.cs is not edited here).
    /// - Áudio: calls <see cref="AudioManager.SetCategoryVolume"/>/<see cref="AudioManager.GetCategoryVolume"/>
    ///   per <see cref="AudioCategory"/>, resolved the same way (AudioManager.cs is not edited here).
    /// - Gráficos: thin wrapper over Unity's own QualitySettings/Screen APIs — no project-specific
    ///   manager exists for this yet, so there is nothing to delegate to.
    /// - Controles: placeholder only. Real key/gamepad rebinding needs the Input System's
    ///   `InputActionRebindingExtensions` wired to `PlayerControls.inputactions` at runtime, which is
    ///   a bigger feature than this screen's scope — see the TODO note in the UXML/inline below.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SettingsMenuController : MonoBehaviour
    {
        private const string TabGraphics = "graphics";
        private const string TabAudio = "audio";
        private const string TabControls = "controls";
        private const string TabLanguage = "language";

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _closeButton;

        private Button _tabGraphicsButton;
        private Button _tabAudioButton;
        private Button _tabControlsButton;
        private Button _tabLanguageButton;

        private VisualElement _panelGraphics;
        private VisualElement _panelAudio;
        private VisualElement _panelControls;
        private VisualElement _panelLanguage;

        // Graphics
        private DropdownField _qualityDropdown;
        private Toggle _fullscreenToggle;

        // Audio
        private Slider _masterVolumeSlider;
        private Slider _musicVolumeSlider;
        private Slider _sfxVolumeSlider;
        private Slider _uiVolumeSlider;

        // Language
        private DropdownField _languageDropdown;

        private readonly MenuNavigationStack _tabStack = new MenuNavigationStack();

        private AudioManager _audioManager;
        private LocalizationManager _localizationManager;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            ServiceLocator.TryGet(out _audioManager);
            ServiceLocator.TryGet(out _localizationManager);

            BindElements();
            RegisterCallbacks();
            InitializeGraphicsPanel();
            InitializeAudioPanel();
            InitializeLanguagePanel();

            _tabStack.Reset(TabGraphics);
            ApplyActiveTab();

            Hide();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
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

            _panelGraphics = rootVisualElement.Q<VisualElement>("panel-graphics");
            _panelAudio = rootVisualElement.Q<VisualElement>("panel-audio");
            _panelControls = rootVisualElement.Q<VisualElement>("panel-controls");
            _panelLanguage = rootVisualElement.Q<VisualElement>("panel-language");

            _qualityDropdown = rootVisualElement.Q<DropdownField>("quality-dropdown");
            _fullscreenToggle = rootVisualElement.Q<Toggle>("fullscreen-toggle");

            _masterVolumeSlider = rootVisualElement.Q<Slider>("master-volume-slider");
            _musicVolumeSlider = rootVisualElement.Q<Slider>("music-volume-slider");
            _sfxVolumeSlider = rootVisualElement.Q<Slider>("sfx-volume-slider");
            _uiVolumeSlider = rootVisualElement.Q<Slider>("ui-volume-slider");

            _languageDropdown = rootVisualElement.Q<DropdownField>("language-dropdown");
        }

        private void RegisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked += Hide;

            if (_tabGraphicsButton != null) _tabGraphicsButton.clicked += () => SwitchTab(TabGraphics);
            if (_tabAudioButton != null) _tabAudioButton.clicked += () => SwitchTab(TabAudio);
            if (_tabControlsButton != null) _tabControlsButton.clicked += () => SwitchTab(TabControls);
            if (_tabLanguageButton != null) _tabLanguageButton.clicked += () => SwitchTab(TabLanguage);

            if (_qualityDropdown != null) _qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);
            if (_fullscreenToggle != null) _fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);

            if (_masterVolumeSlider != null) _masterVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Master, evt.newValue));
            if (_musicVolumeSlider != null) _musicVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Music, evt.newValue));
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Sfx, evt.newValue));
            if (_uiVolumeSlider != null) _uiVolumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(AudioCategory.Ui, evt.newValue));

            if (_languageDropdown != null) _languageDropdown.RegisterValueChangedCallback(OnLanguageChanged);
        }

        private void UnregisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked -= Hide;

            if (_qualityDropdown != null) _qualityDropdown.UnregisterValueChangedCallback(OnQualityChanged);
            if (_fullscreenToggle != null) _fullscreenToggle.UnregisterValueChangedCallback(OnFullscreenChanged);
            if (_languageDropdown != null) _languageDropdown.UnregisterValueChangedCallback(OnLanguageChanged);
        }

        // ------------------------------------------------------------------
        // Tabs
        // ------------------------------------------------------------------

        private void SwitchTab(string tab)
        {
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

            SetTabSelected(_tabGraphicsButton, active == TabGraphics);
            SetTabSelected(_tabAudioButton, active == TabAudio);
            SetTabSelected(_tabControlsButton, active == TabControls);
            SetTabSelected(_tabLanguageButton, active == TabLanguage);
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
        // Show/Hide
        // ------------------------------------------------------------------

        public void Show()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            _tabGraphicsButton?.Focus();
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }
        }
    }
}
