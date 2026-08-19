using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Save;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Drives the Save/Load screen: a simple list of numbered slots, each backed by
    /// <see cref="SaveManager.Save"/>/<see cref="SaveManager.Load"/>/<see cref="SaveManager.SlotExists"/>.
    /// Reused from both the Pause menu ("Salvar", opened in <see cref="ScreenMode.Save"/>) and a
    /// future in-game Load flow (<see cref="ScreenMode.Load"/>) — same panel, the mode only changes
    /// which action each slot's primary button performs (see <see cref="SaveSlotPresenter"/> for the
    /// pure label/enabled-state rules, unit tested separately from this UIDocument-driven class).
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
        [SerializeField] private int _slotCount = 3;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _titleLabel;
        private Button _closeButton;
        private VisualElement _slotListContainer;

        private SaveManager _saveManager;
        private ScreenMode _mode = ScreenMode.Save;

        private readonly List<SlotRow> _slotRows = new List<SlotRow>();

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
            BuildSlotRows();
            Hide();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
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

                var deleteButton = new Button { name = "slot-delete-button", text = "Excluir", focusable = true };
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

                row.TitleLabel.text = SaveSlotPresenter.GetSlotTitle(row.SlotIndex);
                row.StatusLabel.text = SaveSlotPresenter.GetStatusLabel(exists);
                row.PrimaryButton.text = SaveSlotPresenter.GetPrimaryActionLabel(isSaveMode, exists);
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
                _saveManager.Load(slotIndex);
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

        public void Show(ScreenMode mode)
        {
            _mode = mode;

            if (_titleLabel != null)
            {
                _titleLabel.text = mode == ScreenMode.Save ? "Salvar Jogo" : "Carregar Jogo";
            }

            if (_root != null)
            {
                _root.style.display = DisplayStyle.Flex;
            }

            RefreshSlots();

            if (_slotRows.Count > 0)
            {
                _slotRows[0].PrimaryButton.Focus();
            }
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
