using Rapadura.Core.DI;
using Rapadura.Core.Performance;
using Rapadura.Save;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Main Menu screen (Novo Jogo / Continuar / Configurações / Créditos / Sair), built with
    /// UI Toolkit following the same pattern as <c>SkillTreeController</c>/<c>HudController</c>:
    /// a single <see cref="UIDocument"/>, elements resolved once via <c>Q&lt;T&gt;()</c>, no heavy
    /// business logic here.
    ///
    /// This screen deliberately does not load scenes or start gameplay itself — the project has no
    /// scene-loading/bootstrap system yet (TODO.md Fase 1: "Main.unity" doesn't exist), so
    /// "Novo Jogo"/"Continuar" only decide *whether* the action is currently valid and raise a
    /// <see cref="UnityEvent"/> a future bootstrap script can hook up (e.g. to call
    /// SceneManager.LoadScene). "Continuar" is only enabled when a save actually exists, checked via
    /// <see cref="SaveManager.HasAutoSave"/>/<see cref="SaveManager.SlotExists"/> resolved from the
    /// <see cref="ServiceLocator"/> (mirrors how GameManager registers/resolves every other manager).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private CreditsController _creditsMenu;

        [Header("Continue Slot")]
        [Tooltip("Manual slot checked (in addition to the auto-save slot) to decide if 'Continuar' should be enabled.")]
        [SerializeField] private int _continueSlot = 0;

        [Header("Events")]
        public UnityEvent OnNewGameRequested;
        public UnityEvent OnContinueRequested;
        public UnityEvent OnQuitRequested;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _newGameButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _creditsButton;
        private Button _quitButton;

        private SaveManager _saveManager;
        private FrameRateManager _frameRateManager;
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            ServiceLocator.TryGet(out _saveManager);

            BindElements();
            RegisterCallbacks();
            _localizer.Attach(_uiDocument.rootVisualElement);
            RefreshContinueAvailability();
            FocusFirstElement();
            NotifyMenuOpen(IsVisible());
        }

        /// <summary>Start runs after every Awake, so GameManager's FrameRateManager is registered by now even if OnEnable missed it.</summary>
        private void Start()
        {
            _localizer.Refresh();
            NotifyMenuOpen(IsVisible());
        }

        private bool IsVisible()
        {
            return _root == null || _root.style.display.value != DisplayStyle.None;
        }

        private void OnDisable()
        {
            _localizer.Detach();
            UnregisterCallbacks();
            NotifyMenuOpen(false);
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("main-menu-root");
            _newGameButton = rootVisualElement.Q<Button>("new-game-button");
            _continueButton = rootVisualElement.Q<Button>("continue-button");
            _settingsButton = rootVisualElement.Q<Button>("settings-button");
            _creditsButton = rootVisualElement.Q<Button>("credits-button");
            _quitButton = rootVisualElement.Q<Button>("quit-button");
        }

        private void RegisterCallbacks()
        {
            if (_newGameButton != null) _newGameButton.clicked += OnNewGameClicked;
            if (_continueButton != null) _continueButton.clicked += OnContinueClicked;
            if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
            if (_creditsButton != null) _creditsButton.clicked += OnCreditsClicked;
            if (_creditsMenu != null) _creditsMenu.Closed += OnCreditsClosed;
            if (_quitButton != null) _quitButton.clicked += OnQuitClicked;
        }

        private void UnregisterCallbacks()
        {
            if (_newGameButton != null) _newGameButton.clicked -= OnNewGameClicked;
            if (_continueButton != null) _continueButton.clicked -= OnContinueClicked;
            if (_settingsButton != null) _settingsButton.clicked -= OnSettingsClicked;
            if (_creditsButton != null) _creditsButton.clicked -= OnCreditsClicked;
            if (_creditsMenu != null) _creditsMenu.Closed -= OnCreditsClosed;
            if (_quitButton != null) _quitButton.clicked -= OnQuitClicked;
        }

        /// <summary>Sets a predictable first-focused element for keyboard/gamepad navigation (no mouse required to start navigating the menu).</summary>
        private void FocusFirstElement()
        {
            _newGameButton?.Focus();
        }

        private void RefreshContinueAvailability()
        {
            bool canContinue = HasAnySave();
            _continueButton?.SetEnabled(canContinue);
        }

        private bool HasAnySave()
        {
            if (_saveManager == null)
            {
                return false;
            }

            return _saveManager.HasAutoSave() || _saveManager.SlotExists(_continueSlot);
        }

        private void OnNewGameClicked()
        {
            OnNewGameRequested?.Invoke();
        }

        private void OnContinueClicked()
        {
            if (!HasAnySave())
            {
                return;
            }

            OnContinueRequested?.Invoke();
        }

        private void OnSettingsClicked()
        {
            _settingsMenu?.Show();
        }

        private void OnCreditsClicked()
        {
            _creditsMenu?.Show();
        }

        /// <summary>Credits is a separate UIDocument overlay — give focus back to the button that opened it.</summary>
        private void OnCreditsClosed()
        {
            _creditsButton?.Focus();
        }

        /// <summary>
        /// Tells <see cref="FrameRateManager"/> the main menu is up (mobile drops to 30 fps). Resolved
        /// lazily: OnEnable can run before GameManager.Awake has registered managers.
        /// </summary>
        private void NotifyMenuOpen(bool open)
        {
            if (_frameRateManager == null)
            {
                ServiceLocator.TryGet(out _frameRateManager);
            }

            _frameRateManager?.SetMenuOpen(open);
        }

        private void OnQuitClicked()
        {
            OnQuitRequested?.Invoke();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void Show()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            RefreshContinueAvailability();
            FocusFirstElement();
            NotifyMenuOpen(true);
        }

        public void Hide()
        {
            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            NotifyMenuOpen(false);
        }
    }
}
