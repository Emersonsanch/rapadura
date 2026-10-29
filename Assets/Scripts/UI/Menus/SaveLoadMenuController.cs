using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Save;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Save/Load screen: a simple list of numbered slots, each backed by
    /// <see cref="SaveManager.Save"/>/<see cref="SaveManager.Load"/>/<see cref="SaveManager.SlotExists"/>.
    /// Reused from the Pause menu ("Salvar", opened in <see cref="ScreenMode.Save"/>), the death
    /// screen and the title screen's "Carregar" (<see cref="ScreenMode.Load"/>) — same panel, the mode
    /// only changes which action each slot's primary button performs (see <see cref="SaveSlotPresenter"/>
    /// for the pure label/enabled-state rules, unit tested separately from this UIDocument-driven class).
    /// A caller may pass its own load handler to <see cref="Show(ScreenMode, Func{int, bool}, Action)"/>
    /// (the title screen routes the load through GameFlowService so the flow leaves the title); the
    /// panel closes itself once that handler reports success. Gamepad/keyboard: <see cref="FocusNavigator"/>
    /// (D-pad between rows, A acts, B closes).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SaveLoadMenuController : MonoBehaviour
    {
        public enum ScreenMode
        {
            Save,
            Load
        }

        [Header("Slots")]
        [Tooltip("Number of manual save slots offered (slot indices 0..Count-1; SaveManager.AutoSaveSlot is separate and not listed here).")]
        [SerializeField] private int _slotCount = SaveManager.ManualSlotCount;

        private UIDocument _uiDocument;
        private VisualElement _root;

        /// <summary>Read by PauseMenuController.HandleBack (Esc/Start closes this before the pause menu).</summary>
        public bool IsOpen => _root != null && _root.style.display == DisplayStyle.Flex;
        private Label _titleLabel;
        private Button _closeButton;
        private VisualElement _slotListContainer;

        private SaveManager _saveManager;
        private ScreenMode _mode = ScreenMode.Save;
        private Func<int, bool> _loadHandler;
        private Action _onClosed;
        private FocusNavigator _focusNavigator;

        private readonly List<SlotRow> _slotRows = new List<SlotRow>();
        private readonly UiDocumentLocalizer _localizer = new UiDocumentLocalizer();

        private struct SlotRow
        {
            public int SlotIndex;
            public Label TitleLabel;
            public Label StatusLabel;
            public Button PrimaryButton;
            public Button DeleteButton;
        }

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            ServiceLocator.TryGet(out _saveManager);

            BindElements();
            RegisterCallbacks();
            _localizer.Attach(_uiDocument.rootVisualElement, OnLanguageChanged);
            BuildSlotRows();
            _focusNavigator = FocusNavigator.Attach(_root, () => IsOpen, Hide, FirstEnabledPrimaryButton);
            Hide();
        }

        private void OnDisable()
        {
            _focusNavigator?.Detach();
            _focusNavigator = null;
            _localizer.Detach();
            UnregisterCallbacks();
            GameplayPause.Release(this);
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("save-load-menu-root");
            _titleLabel = rootVisualElement.Q<Label>("save-load-title-label");
            _closeButton = rootVisualElement.Q<Button>("close-button");
            _slotListContainer = rootVisualElement.Q<VisualElement>("slot-list-container");
        }

        private void RegisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked += Hide;
        }

        private void UnregisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked -= Hide;
        }

        private void BuildSlotRows()
        {
            if (_slotListContainer == null)
            {
                return;
            }

            _slotListContainer.Clear();
            _slotRows.Clear();

            for (int i = 0; i < _slotCount; i++)
            {
                int slotIndex = i;

                var row = new VisualElement { name = $"slot-row-{slotIndex}" };
                row.AddToClassList("save-slot-row");

                var titleLabel = new Label { name = "slot-title-label" };
                titleLabel.AddToClassList("save-slot-title-label");
                row.Add(titleLabel);

                var statusLabel = new Label { name = "slot-status-label" };
                statusLabel.AddToClassList("save-slot-status-label");
                row.Add(statusLabel);

                var primaryButton = new Button { name = "slot-primary-button", focusable = true };
                primaryButton.AddToClassList("save-slot-primary-button");
                primaryButton.clicked += () => OnPrimaryActionClicked(slotIndex);
                row.Add(primaryButton);

                var deleteButton = new Button { name = "slot-delete-button", text = UiTexts.Get(_localizer.Manager, UiTexts.SaveLoadDelete), focusable = true };
                deleteButton.AddToClassList("save-slot-delete-button");
                deleteButton.clicked += () => OnDeleteClicked(slotIndex);
                row.Add(deleteButton);

                _slotListContainer.Add(row);

                _slotRows.Add(new SlotRow
                {
                    SlotIndex = slotIndex,
                    TitleLabel = titleLabel,
                    StatusLabel = statusLabel,
                    PrimaryButton = primaryButton,
                    DeleteButton = deleteButton
                });
            }
        }

        private void RefreshSlots()
        {
            foreach (SlotRow row in _slotRows)
            {
                bool exists = _saveManager != null && _saveManager.SlotExists(row.SlotIndex);
                bool isSaveMode = _mode == ScreenMode.Save;

                LocalizationManager localization = _localizer.Manager;
                row.TitleLabel.text = SaveSlotPresenter.GetSlotTitle(row.SlotIndex, localization);
                row.StatusLabel.text = SaveSlotPresenter.GetStatusLabel(exists, localization);
                row.PrimaryButton.text = SaveSlotPresenter.GetPrimaryActionLabel(isSaveMode, exists, localization);
                row.DeleteButton.text = UiTexts.Get(localization, UiTexts.SaveLoadDelete);
                row.PrimaryButton.SetEnabled(SaveSlotPresenter.IsPrimaryActionEnabled(isSaveMode, exists));
                row.DeleteButton.SetEnabled(SaveSlotPresenter.CanDelete(exists));
            }
        }

        private void OnPrimaryActionClicked(int slotIndex)
        {
            if (_saveManager == null)
            {
                return;
            }

            if (_mode == ScreenMode.Save)
            {
                _saveManager.Save(slotIndex);
            }
            else if (_saveManager.SlotExists(slotIndex))
            {
                if (_loadHandler != null)
                {
                    if (_loadHandler(slotIndex))
                    {
                        Hide();
                        return;
                    }
                }
                else
                {
                    _saveManager.Load(slotIndex);
                }
            }

            RefreshSlots();
        }

        private void OnDeleteClicked(int slotIndex)
        {
            if (_saveManager == null || !_saveManager.SlotExists(slotIndex))
            {
                return;
            }

            _saveManager.DeleteSlot(slotIndex);
            RefreshSlots();
        }

        /// <param name="loadHandler">Load mode only: replaces <see cref="SaveManager.Load"/> for the chosen slot; returning true closes the panel. Null = load directly (in-game behaviour).</param>
        /// <param name="onClosed">Run once when this opening of the panel closes (e.g. to give focus back to the button that opened it).</param>
        public void Show(ScreenMode mode, Func<int, bool> loadHandler = null, Action onClosed = null)
        {
            // Same class of bug as ShopUIController/DialogueUIController (fixed earlier this
            // session): OnEnable's ServiceLocator.TryGet(out _saveManager) runs once at scene
            // start, before GameManager.Awake() is guaranteed to have registered SaveManager
            // (Unity doesn't order Awake/OnEnable across different GameObjects) — left
            // _saveManager permanently null, so every Save/Load/Delete button on this screen
            // silently did nothing (confirmed via Play Mode: _saveManager was null after a normal
            // scene start). Re-resolving here is cheap (only looks up while still null).
            if (_saveManager == null)
            {
                ServiceLocator.TryGet(out _saveManager);
            }

            _mode = mode;
            _loadHandler = mode == ScreenMode.Load ? loadHandler : null;
            _onClosed = onClosed;
            _localizer.Refresh();
            UpdateTitle();

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            GameplayPause.Acquire(this);

            RefreshSlots();

            if (_focusNavigator != null)
            {
                _focusNavigator.FocusInitial();
            }
            else
            {
                FirstEnabledPrimaryButton()?.Focus();
            }
        }

        /// <summary>First slot whose primary action is available (in Load mode, the first saved slot), else Fechar.</summary>
        private VisualElement FirstEnabledPrimaryButton()
        {
            foreach (SlotRow row in _slotRows)
            {
                if (row.PrimaryButton.enabledSelf)
                {
                    return row.PrimaryButton;
                }
            }

            return _closeButton;
        }

        private void UpdateTitle()
        {
            if (_titleLabel != null)
            {
                _titleLabel.text = UiTexts.Get(_localizer.Manager, _mode == ScreenMode.Save ? UiTexts.SaveLoadTitleSave : UiTexts.SaveLoadTitleLoad);
            }
        }

        private void OnLanguageChanged()
        {
            UpdateTitle();
            RefreshSlots();
        }

        public void Hide()
        {
            bool wasOpen = IsOpen;

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            GameplayPause.Release(this);

            if (!wasOpen)
            {
                return;
            }

            _focusNavigator?.ReleaseFocus();
            _loadHandler = null;
            Action onClosed = _onClosed;
            _onClosed = null;
            onClosed?.Invoke();
        }
    }
}
