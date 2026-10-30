using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Performance;
using Rapadura.Save;
using Rapadura.UI.Common;
using Rapadura.UI.WorldMap;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Main Menu screen (Novo Jogo / Continuar / Carregar / Configurações / Créditos / Sair), built with
    /// UI Toolkit following the same pattern as <c>SkillTreeController</c>/<c>HudController</c>:
    /// a single <see cref="UIDocument"/>, elements resolved once via <c>Q&lt;T&gt;()</c>, no heavy
    /// business logic here. The look (illustrated backdrop, entrance animation honouring "Reduzir
    /// Flashes") lives in <see cref="TitleScreenDecor"/> and Common/RapaduraTheme.uss.
    ///
    /// This screen deliberately does not load scenes or start gameplay itself — the project has no
    /// scene-loading/bootstrap system yet (TODO.md Fase 1: "Main.unity" doesn't exist), so
    /// "Novo Jogo"/"Continuar" only decide *whether* the action is currently valid and raise a
    /// <see cref="UnityEvent"/> a future bootstrap script can hook up (e.g. to call
    /// SceneManager.LoadScene). "Continuar" is only enabled when a readable save exists in the
    /// auto-save or any manual slot (GameFrontEndController then loads the newest one), and "Carregar"
    /// opens the shared <see cref="SaveLoadMenuController"/> in Load mode, enabled only when a manual
    /// slot holds a save — rules in <see cref="MainMenuPresenter"/>; <see cref="SaveManager"/> is
    /// resolved from the <see cref="ServiceLocator"/> (mirrors how GameManager registers/resolves
    /// every other manager). Nothing here needs scene wiring: the Save/Load screen is found in the
    /// scene when <see cref="_saveLoadMenu"/> is empty ("Carregar" is hidden when there is none), and
    /// the slot list comes from code (<see cref="SaveManager.ManualSlotCount"/>). After a crash in the
    /// previous session a small <see cref="CrashNoticeBanner"/> sits at the bottom of the screen.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private SettingsMenuController _settingsMenu;
        [SerializeField] private CreditsController _creditsMenu;
        [Tooltip("Optional: the Save/Load screen 'Carregar' opens. Found in the scene (inactive objects included) when empty.")]
        [SerializeField] private SaveLoadMenuController _saveLoadMenu;

        [Header("Events")]
        public UnityEvent OnNewGameRequested;
        public UnityEvent OnContinueRequested;
        public UnityEvent OnQuitRequested;

        /// <summary>
        /// "Carregar" picked a slot: load it and leave the title (GameFrontEndController routes it
        /// through GameFlowService). True when it loaded, which closes the Save/Load screen. Null =
        /// plain <see cref="SaveManager.Load"/>.
        /// </summary>
        public Func<int, bool> LoadSlotHandler { get; set; }

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _newGameButton;
        private Button _continueButton;
        private Button _loadButton;
        private Button _settingsButton;
        private Button _creditsButton;
        private Button _quitButton;
        private Label _continueBadge;

        private SaveManager _saveManager;
        private FrameRateManager _frameRateManager;
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();
        private readonly TitleScreenDecor _decor = new TitleScreenDecor();
        private readonly CrashNoticeBanner _crashNotice = new CrashNoticeBanner();

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();

            if (_saveLoadMenu == null)
            {
                _saveLoadMenu = FindFirstObjectByType<SaveLoadMenuController>(FindObjectsInactive.Include);
            }
        }

        private void OnEnable()
        {
            ServiceLocator.TryGet(out _saveManager);

            BindElements();
            RegisterCallbacks();
            _localizer.Attach(_uiDocument.rootVisualElement);
            _decor.Attach(_uiDocument.rootVisualElement, _root, UiMotionPreferences.IsReducedMotion());
            _crashNotice.Attach(_root);
            RefreshContinueAvailability();
            FocusFirstElement();
            NotifyMenuOpen(IsVisible());
        }

        /// <summary>Start runs after every Awake, so GameManager's FrameRateManager is registered by now even if OnEnable missed it.</summary>
        private void Start()
        {
            _localizer.Refresh();
            _decor.RefreshMotion(UiMotionPreferences.IsReducedMotion());
            NotifyMenuOpen(IsVisible());
        }

        private bool IsVisible()
        {
            return _root == null || _root.style.display.value != DisplayStyle.None;
        }

        private void OnDisable()
        {
            _localizer.Detach();
            _decor.Detach();
            _crashNotice.Detach();
            UnregisterCallbacks();
            NotifyMenuOpen(false);
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("main-menu-root");
            _newGameButton = rootVisualElement.Q<Button>("new-game-button");
            _continueButton = rootVisualElement.Q<Button>("continue-button");
            _loadButton = rootVisualElement.Q<Button>("load-button");
            _settingsButton = rootVisualElement.Q<Button>("settings-button");
            _creditsButton = rootVisualElement.Q<Button>("credits-button");
            _quitButton = rootVisualElement.Q<Button>("quit-button");
        }

        private void RegisterCallbacks()
        {
            if (_newGameButton != null) _newGameButton.clicked += OnNewGameClicked;
            if (_continueButton != null) _continueButton.clicked += OnContinueClicked;
            if (_loadButton != null) _loadButton.clicked += OnLoadClicked;
            if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
            if (_creditsButton != null) _creditsButton.clicked += OnCreditsClicked;
            if (_creditsMenu != null) _creditsMenu.Closed += OnCreditsClosed;
            if (_quitButton != null) _quitButton.clicked += OnQuitClicked;
        }

        private void UnregisterCallbacks()
        {
            if (_newGameButton != null) _newGameButton.clicked -= OnNewGameClicked;
            if (_continueButton != null) _continueButton.clicked -= OnContinueClicked;
            if (_loadButton != null) _loadButton.clicked -= OnLoadClicked;
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
            // OnEnable can run before GameManager.Awake has registered SaveManager.
            if (_saveManager == null)
            {
                ServiceLocator.TryGet(out _saveManager);
            }

            _continueButton?.SetEnabled(MainMenuPresenter.IsContinueEnabled(_saveManager));
            // Small "Jornada+N" badge when the save to continue is on a Nova Jornada+.
            _continueBadge = NewGamePlusHud.ApplyBadge(_continueButton, _continueBadge, MainMenuPresenter.GetContinueCycle(_saveManager),
                ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null, corner: true);

            if (_loadButton != null)
            {
                _loadButton.style.display = _saveLoadMenu != null ? DisplayStyle.Flex : DisplayStyle.None;
                _loadButton.SetEnabled(MainMenuPresenter.IsLoadEnabled(_saveManager, _saveLoadMenu != null));
            }
        }

        private bool HasAnySave()
        {
            return MainMenuPresenter.IsContinueEnabled(_saveManager);
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

        private void OnLoadClicked()
        {
            if (_saveLoadMenu == null || !MainMenuPresenter.IsLoadEnabled(_saveManager, true))
            {
                return;
            }

            _saveLoadMenu.Show(SaveLoadMenuController.ScreenMode.Load, LoadSlot, OnLoadMenuClosed);
        }

        private bool LoadSlot(int slot)
        {
            if (LoadSlotHandler != null)
            {
                return LoadSlotHandler(slot);
            }

            return _saveManager != null && _saveManager.Load(slot);
        }

        /// <summary>Back from the Save/Load screen without loading (or a slot was deleted there): re-check the buttons and give focus back.</summary>
        private void OnLoadMenuClosed()
        {
            if (!isActiveAndEnabled)
            {
                return; // a load left the title screen
            }

            RefreshContinueAvailability();

            if (_loadButton != null && _loadButton.enabledSelf)
            {
                _loadButton.Focus();
            }
            else
            {
                FocusFirstElement();
            }
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

            _decor.RefreshMotion(UiMotionPreferences.IsReducedMotion());
            _decor.PlayEntrance();
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
