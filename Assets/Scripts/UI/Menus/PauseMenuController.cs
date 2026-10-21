using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Save;
using Rapadura.UI.Achievements;
using Rapadura.UI.Common;
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
    /// "Continuar" flow later). "Menu Principal" opens a "Voltar ao menu?" confirm dialog
    /// (Resources/UI/ReturnToTitleDialog.uxml, texts in <see cref="ReturnToTitleTexts"/>): "Salvar e
    /// sair" auto-saves first, both exits then call GameFlowService.ReturnToTitle (see
    /// <see cref="ReturnToTitleAction"/>) and raise <see cref="OnMainMenuRequested"/>.
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

        [Header("Return to title")]
        [Tooltip("\"Voltar ao menu?\" confirm dialog. If empty, loaded from Resources/" + ReturnToTitleDialogResourcePath + ".")]
        [SerializeField] private VisualTreeAsset _returnToTitleDialogAsset;

        [Header("Events")]
        [Tooltip("Raised after the player confirmed leaving to the title screen (the flow change itself is done here).")]
        public UnityEvent OnMainMenuRequested;

        public const string ReturnToTitleDialogResourcePath = "UI/ReturnToTitleDialog";

        private VisualElement _returnDialog;
        private Label _returnDialogTitle;
        private Label _returnDialogMessage;
        private Button _returnSaveButton;
        private Button _returnQuitButton;
        private Button _returnCancelButton;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Button _resumeButton;
        private Button _settingsButton;
        private Button _saveButton;
        private Button _achievementsButton;
        private Button _craftingButton;
        private Button _mainMenuButton;

        private bool _isOpen;
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();

        public bool IsOpen => _isOpen;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            BindElements();
            RegisterCallbacks();
            _localizer.Attach(_uiDocument.rootVisualElement);
            Hide(publishEvent: false);
        }

        private void OnDisable()
        {
            _localizer.Detach();
            UnregisterCallbacks();
            GameplayPause.Release(this);
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
            BindReturnToTitleDialog();
        }

        private void RegisterCallbacks()
        {
            if (_resumeButton != null) _resumeButton.clicked += Hide_NoArgs;
            if (_settingsButton != null) _settingsButton.clicked += OnSettingsClicked;
            if (_saveButton != null) _saveButton.clicked += OnSaveClicked;
            if (_achievementsButton != null) _achievementsButton.clicked += OnAchievementsClicked;
            if (_craftingButton != null) _craftingButton.clicked += OnCraftingClicked;
            if (_mainMenuButton != null) _mainMenuButton.clicked += OnMainMenuClicked;
            if (_returnSaveButton != null) _returnSaveButton.clicked += OnReturnSaveAndQuitClicked;
            if (_returnQuitButton != null) _returnQuitButton.clicked += OnReturnQuitWithoutSavingClicked;
            if (_returnCancelButton != null) _returnCancelButton.clicked += HideReturnToTitleDialog;
        }

        private void UnregisterCallbacks()
        {
            if (_resumeButton != null) _resumeButton.clicked -= Hide_NoArgs;
            if (_settingsButton != null) _settingsButton.clicked -= OnSettingsClicked;
            if (_saveButton != null) _saveButton.clicked -= OnSaveClicked;
            if (_achievementsButton != null) _achievementsButton.clicked -= OnAchievementsClicked;
            if (_craftingButton != null) _craftingButton.clicked -= OnCraftingClicked;
            if (_mainMenuButton != null) _mainMenuButton.clicked -= OnMainMenuClicked;
            if (_returnSaveButton != null) _returnSaveButton.clicked -= OnReturnSaveAndQuitClicked;
            if (_returnQuitButton != null) _returnQuitButton.clicked -= OnReturnQuitWithoutSavingClicked;
            if (_returnCancelButton != null) _returnCancelButton.clicked -= HideReturnToTitleDialog;
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
            _localizer.Refresh();

            // No Criação screen in this scene (e.g. the pause menu reused on WorldMap.unity): hide the dead button.
            if (_craftingButton != null)
            {
                if (_craftingUI == null) _craftingUI = FindFirstObjectByType<CraftingUIController>();
                _craftingButton.style.display = _craftingUI != null ? DisplayStyle.Flex : DisplayStyle.None;
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            _resumeButton?.Focus();

            // GameplayPause counts holders and publishes GamePausedEvent itself, so this menu no
            // longer resets timeScale to 1 over another screen's pause.
            if (_controlsTimeScale)
            {
                GameplayPause.Acquire(this);
            }
            else
            {
                EventBus.Publish(new GamePausedEvent());
            }
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

            SetReturnToTitleDialogVisible(false);

            if (_controlsTimeScale)
            {
                GameplayPause.Release(this);
            }
            else if (publishEvent && wasOpen)
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
            if (IsReturnToTitleDialogOpen)
            {
                HideReturnToTitleDialog();
                return;
            }

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

        // ------------------------------------------------------------------
        // "Menu Principal": confirm dialog → (auto-save) → GameFlowService.ReturnToTitle
        // ------------------------------------------------------------------

        public bool IsReturnToTitleDialogOpen => _returnDialog != null && _returnDialog.style.display == DisplayStyle.Flex;

        private void OnMainMenuClicked()
        {
            if (_returnDialog == null)
            {
                // No dialog asset found: never strand the player on the button — ask nothing, keep progress.
                Debug.LogWarning("[PauseMenuController] Return-to-title dialog missing; saving before leaving.");
                ConfirmReturnToTitle(ReturnToTitleChoice.SaveAndQuit);
                return;
            }

            ApplyReturnToTitleTexts();
            SetReturnToTitleDialogVisible(true);
            _returnCancelButton?.Focus();
        }

        private void OnReturnSaveAndQuitClicked() => ConfirmReturnToTitle(ReturnToTitleChoice.SaveAndQuit);

        private void OnReturnQuitWithoutSavingClicked() => ConfirmReturnToTitle(ReturnToTitleChoice.QuitWithoutSaving);

        private void HideReturnToTitleDialog()
        {
            SetReturnToTitleDialogVisible(false);
            _mainMenuButton?.Focus();
        }

        private void ConfirmReturnToTitle(ReturnToTitleChoice choice)
        {
            ServiceLocator.TryGet(out SaveManager saveManager);
            ServiceLocator.TryGet(out Rapadura.Gameplay.Flow.GameFlowService flow);

            if (flow != null && ReturnToTitleAction.Execute(choice, saveManager, flow))
            {
                // The world map is loading: keep gameplay frozen (this menu still holds GameplayPause)
                // until the scene swap disables this component, whose OnDisable releases the hold.
                SetReturnToTitleDialogVisible(false);
                OnMainMenuRequested?.Invoke();
                return;
            }

            if (flow == null)
            {
                // Played without a GameManager (scene opened directly): no flow to return to.
                if (choice == ReturnToTitleChoice.SaveAndQuit)
                {
                    saveManager?.AutoSave();
                }

                Debug.LogWarning("[PauseMenuController] No GameFlowService registered (missing GameManager?); cannot return to the title screen.");
            }

            Hide();
            OnMainMenuRequested?.Invoke();
        }

        /// <summary>Instantiates the dialog once into the pause root (serialized asset, else the Resources copy).</summary>
        private void BindReturnToTitleDialog()
        {
            VisualElement host = _root ?? _uiDocument.rootVisualElement;
            _returnDialog = host.Q<VisualElement>("return-title-dialog");

            if (_returnDialog == null)
            {
                VisualTreeAsset asset = _returnToTitleDialogAsset != null
                    ? _returnToTitleDialogAsset
                    : Resources.Load<VisualTreeAsset>(ReturnToTitleDialogResourcePath);

                if (asset == null)
                {
                    return;
                }

                asset.CloneTree(host);
                _returnDialog = host.Q<VisualElement>("return-title-dialog");

                if (_returnDialog == null)
                {
                    return;
                }
            }

            _returnDialogTitle = _returnDialog.Q<Label>("return-title-title");
            _returnDialogMessage = _returnDialog.Q<Label>("return-title-message");
            _returnSaveButton = _returnDialog.Q<Button>("return-title-save-button");
            _returnQuitButton = _returnDialog.Q<Button>("return-title-quit-button");
            _returnCancelButton = _returnDialog.Q<Button>("return-title-cancel-button");
            SetReturnToTitleDialogVisible(false);
        }

        private void SetReturnToTitleDialogVisible(bool visible)
        {
            if (_returnDialog != null)
            {
                _returnDialog.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void ApplyReturnToTitleTexts()
        {
            ServiceLocator.TryGet(out Rapadura.Core.Localization.LocalizationManager localization);

            if (_returnDialogTitle != null) _returnDialogTitle.text = ReturnToTitleTexts.Get(localization, ReturnToTitleTexts.Title);
            if (_returnDialogMessage != null) _returnDialogMessage.text = ReturnToTitleTexts.Get(localization, ReturnToTitleTexts.Message);
            if (_returnSaveButton != null) _returnSaveButton.text = ReturnToTitleTexts.Get(localization, ReturnToTitleTexts.SaveAndQuit);
            if (_returnQuitButton != null) _returnQuitButton.text = ReturnToTitleTexts.Get(localization, ReturnToTitleTexts.QuitWithoutSaving);
            if (_returnCancelButton != null) _returnCancelButton.text = ReturnToTitleTexts.Get(localization, ReturnToTitleTexts.Cancel);
        }
    }
}
