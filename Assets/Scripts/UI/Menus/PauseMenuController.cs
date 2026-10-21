using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Save;
using Rapadura.UI.Achievements;
using Rapadura.UI.Crafting;
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
    ///
    /// "Conquistas" and "Criação" live here instead of on the HUD: they're rarely-used, full-screen
    /// screens and the top-right HUD cluster (Missões/Mochila/Skills/Menu) is already at the limit
    /// of what fits next to the vitals panel on a 20:9 phone with a notch. Both close the pause menu
    /// and then open their screen — they behave the same as when opened anywhere else (whether they
    /// freeze gameplay is their own GameplayPause concern), and stacking them over the pause dim
    /// would hide them depending on UIDocument sort order. Crafting also keeps its in-world workbench trigger.
    ///
    /// <see cref="HandleBack"/> is the Esc / gamepad Start behaviour (driven by HudController):
    /// close an open sub-screen first (Settings — unless a rebind is listening —, Save/Load), else resume.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private SaveLoadMenuController _saveLoadMenu;
        [Tooltip("Opened by the Conquistas button. If empty, resolved from the scene on first tap.")]
        [SerializeField] private AchievementListController _achievementList;
        [Tooltip("Opened by the Criação button (hand crafting, no station). If empty, resolved from the scene on first tap.")]
        [SerializeField] private CraftingUIController _craftingUI;

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
        private Button _achievementsButton;
        private Button _craftingButton;
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
            _achievementsButton = rootVisualElement.Q<Button>("achievements-button");
            _craftingButton = rootVisualElement.Q<Button>("crafting-button");
            _mainMenuButton = rootVisualElement.Q<Button>("main-menu-button");
        }

        private void RegisterCallbacks()
        {
            if (_resumeButton != null) _resumeButton.clicked += Hide_NoArgs;
            if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
            if (_saveButton != null) _saveButton.clicked += OnSaveClicked;
            if (_achievementsButton != null) _achievementsButton.clicked += OnAchievementsClicked;
            if (_craftingButton != null) _craftingButton.clicked += OnCraftingClicked;
            if (_mainMenuButton != null) _mainMenuButton.clicked += OnMainMenuClicked;
        }

        private void UnregisterCallbacks()
        {
            if (_resumeButton != null) _resumeButton.clicked -= Hide_NoArgs;
            if (_settingsButton != null) _settingsButton.clicked -= OnSettingsClicked;
            if (_saveButton != null) _saveButton.clicked -= OnSaveClicked;
            if (_achievementsButton != null) _achievementsButton.clicked -= OnAchievementsClicked;
            if (_craftingButton != null) _craftingButton.clicked -= OnCraftingClicked;
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

        /// <summary>
        /// Esc / gamepad Start while the pause menu is open: closes the topmost sub-screen opened
        /// from here, otherwise resumes. Ignored while Settings is listening for a rebind (Esc
        /// cancels the rebind there — see SettingsMenuController.Update).
        /// </summary>
        public void HandleBack()
        {
            if (_settingsMenu != null && _settingsMenu.IsOpen)
            {
                if (!_settingsMenu.IsRebinding)
                {
                    _settingsMenu.Hide();
                    _settingsButton?.Focus();
                }

                return;
            }

            if (_saveLoadMenu != null && _saveLoadMenu.IsOpen)
            {
                _saveLoadMenu.Hide();
                _saveButton?.Focus();
                return;
            }

            Hide();
        }

        private void OnAchievementsClicked()
        {
            if (_achievementList == null)
            {
                _achievementList = FindFirstObjectByType<AchievementListController>();
            }

            if (_achievementList != null)
            {
                Hide();
                _achievementList.Open();
            }
        }

        private void OnCraftingClicked()
        {
            if (_craftingUI == null)
            {
                _craftingUI = FindFirstObjectByType<CraftingUIController>();
            }

            if (_craftingUI != null)
            {
                Hide();
                _craftingUI.Open();
            }
        }

        private void OnMainMenuClicked()
        {
            OnMainMenuRequested?.Invoke();
        }
    }
}
