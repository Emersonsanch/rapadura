using Rapadura.Core.DI;
using Rapadura.Core.EventBus;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Pause screen (Retomar / Configurações / Salvar / Menu Principal), built with UI
    /// Toolkit following the same single-<see cref="UIDocument"/>/<c>Q&lt;T&gt;()</c>-once pattern
    /// as <c>SkillTreeController</c>. Publishes <see cref="GamePausedEvent"/>/<see cref="GameResumedEvent"/>
    /// (see MenuEvents.cs — GameEvents.cs has no pause event yet and is out of scope to edit here)
    /// so any other system (audio ducking, gameplay input, AI) can react to pause without a direct
    /// reference to this controller.
    ///
    /// "Salvar" opens the shared Save/Load screen in save mode rather than silently quick-saving,
    /// so the player picks a slot (consistent with the Save/Load screen also used from Main Menu's
    /// "Continuar" flow later). "Menu Principal" only raises a UnityEvent — like MainMenuController,
    /// this project has no scene-loading bootstrap yet, so leaving the scene is a decision for
    /// whatever script wires that event up.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private SaveLoadMenuController _saveLoadMenu;

        [Header("Behaviour")]
        [Tooltip("Whether opening this menu sets Time.timeScale to 0 and closing restores it to 1.")]
        [SerializeField] private bool _controlsTimeScale = true;

        [Header("Events")]
        public UnityEvent OnMainMenuRequested;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _saveButton;
        private Button _mainMenuButton;

        private bool _isOpen;

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            Hide(publishEvent: false);
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("pause-menu-root");
            _resumeButton = rootVisualElement.Q<Button>("resume-button");
            _settingsButton = rootVisualElement.Q<Button>("settings-button");
            _saveButton = rootVisualElement.Q<Button>("save-button");
            _mainMenuButton = rootVisualElement.Q<Button>("main-menu-button");
        }

        private void RegisterCallbacks()
        {
            if (_resumeButton != null) _resumeButton.clicked += Hide_NoArgs;
            if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
            if (_saveButton != null) _saveButton.clicked += OnSaveClicked;
            if (_mainMenuButton != null) _mainMenuButton.clicked += OnMainMenuClicked;
        }

        private void UnregisterCallbacks()
        {
            if (_resumeButton != null) _resumeButton.clicked -= Hide_NoArgs;
            if (_settingsButton != null) _settingsButton.clicked -= OnSettingsClicked;
            if (_saveButton != null) _saveButton.clicked -= OnSaveClicked;
            if (_mainMenuButton != null) _mainMenuButton.clicked -= OnMainMenuClicked;
        }

        private void Hide_NoArgs() => Hide();

        public void Toggle()
        {
            if (_isOpen)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        public void Show()
        {
            _isOpen = true;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            if (_controlsTimeScale)
            {
                Time.timeScale = 0f;
            }

            _resumeButton?.Focus();

            EventBus.Publish(new GamePausedEvent());
        }

        public void Hide() => Hide(publishEvent: true);

        private void Hide(bool publishEvent)
        {
            bool wasOpen = _isOpen;
            _isOpen = false;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            if (_controlsTimeScale)
            {
                Time.timeScale = 1f;
            }

            if (publishEvent && wasOpen)
            {
                EventBus.Publish(new GameResumedEvent());
            }
        }

        private void OnSettingsClicked()
        {
            _settingsMenu?.Show();
        }

        private void OnSaveClicked()
        {
            _saveLoadMenu?.Show(SaveLoadMenuController.ScreenMode.Save);
        }

        private void OnMainMenuClicked()
        {
            OnMainMenuRequested?.Invoke();
        }
    }
}
