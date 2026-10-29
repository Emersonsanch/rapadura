using System;
using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.UI.Common;
using Rapadura.UI.Inventory;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// "Baú da Vó Zefa" window (<c>Resources/UI/VillageChestView.uxml</c>, styled by <c>UI/Village/VillageChestView.uss</c>
    /// on top of the shared theme and the Mochila's tile look): two panes, Mochila and Baú, one tile per slot.
    /// <list type="bullet">
    /// <item>Tap / click / A on a tile: a single item moves to the other side at once; a stack opens the amount bar
    /// (−, +, Tudo, Levar, Cancelar), defaulting to the whole stack.</item>
    /// <item>Drag a tile onto the other pane: the whole stack moves (same press-then-move threshold as the Mochila,
    /// <see cref="InventoryDragDrop.DragThresholdPixels"/>).</item>
    /// <item>"Guardar materiais" / "Pegar o que as missões pedem": <see cref="VillageChestTransfer.StoreMaterials"/> /
    /// <see cref="VillageChestTransfer.TakeForQuests"/> with the quests' outstanding collect amounts.</item>
    /// <item>Everything into the bag goes through the bag's own weight and slot rules (<see cref="VillageChestTransfer"/>).</item>
    /// <item>Gamepad / keyboard through <see cref="FocusNavigator"/> (B / Esc closes); tooltips on every item.</item>
    /// </list>
    /// Opened by <see cref="VillageHubButton"/> on the live <see cref="CarriedInventorySession"/> inventory; the hub
    /// hands it back to the carry once this window closes.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class VillageChestUIController : MonoBehaviour
    {
        public enum Pane
        {
            Bag,
            Chest,
        }

        public const string RootName = "village-chest-root";
        public const string StoreMaterialsButtonName = "village-chest-store-materials-button";
        public const string TakeForQuestsButtonName = "village-chest-take-quests-button";
        public const string CloseButtonName = "village-chest-close-button";
        public const string BagGridName = "village-chest-bag-grid";
        public const string ChestGridName = "village-chest-chest-grid";

        private const string SlotClass = "inventory-slot";
        private const string EmptySlotClass = "inventory-slot--empty";
        private const string QuantityClass = "inventory-slot-quantity";
        private const string SelectedClass = "village-chest-slot--selected";
        private const string DragSourceClass = "inventory-slot--drag-source";
        private const string DropPaneClass = "village-chest-pane--drop";

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private Label _bagTitleLabel;
        private Label _chestTitleLabel;
        private Label _bagWeightLabel;
        private Label _chestSlotsLabel;
        private VisualElement _bagPane;
        private VisualElement _chestPane;
        private VisualElement _bagGrid;
        private VisualElement _chestGrid;
        private Label _chestEmptyLabel;
        private VisualElement _amountBar;
        private Label _amountLabel;
        private Label _amountValue;
        private Button _amountMinus;
        private Button _amountPlus;
        private Button _amountAll;
        private Button _amountMove;
        private Button _amountCancel;
        private Button _storeMaterialsButton;
        private Button _takeForQuestsButton;
        private Button _closeButton;
        private Label _statusLabel;
        private Label _hintLabel;
        private Label _dragGhost;

        private TooltipController _tooltip;
        private FocusNavigator _focusNavigator;

        private InventoryManager _bag;
        private VillageChestStorage _chest;
        private Func<string, int, int> _outstandingForQuests;
        private bool _open;

        // Amount bar selection.
        private bool _hasSelection;
        private Pane _selectedPane;
        private int _selectedIndex = -1;
        private string _selectedItemId;
        private int _amount;

        // Drag state.
        private int _pressPointerId = -1;
        private Vector2 _pressPosition;
        private Pane _dragPane;
        private int _dragIndex = -1;
        private VisualElement _dragSourceElement;
        private string _dragLabel;
        private bool _isDragging;

        public bool IsOpen => _open;
        public InventoryManager Bag => _bag;
        public VillageChestStorage Chest => _chest;

        private static LocalizationManager Localization => ServiceLocator.TryGet(out LocalizationManager localization) ? localization : null;

        // ------------------------------------------------------------------
        // Pure rules (tested)
        // ------------------------------------------------------------------

        /// <summary>The amount bar keeps its value between 1 and the stack size.</summary>
        public static int ClampAmount(int amount, int stackSize) => Mathf.Clamp(amount, 1, Mathf.Max(1, stackSize));

        /// <summary>A stack asks how many to move; a single item moves at once.</summary>
        public static bool NeedsAmountPicker(InventorySlotData slot) => slot != null && !slot.IsEmpty && slot.quantity > 1;

        /// <summary>Status line after a bulk move. <paramref name="storing"/>: "Guardar materiais", else "Pegar o que as missões pedem".</summary>
        public static string DescribeBulkResult(bool storing, VillageChestBulkResult result, LocalizationManager localization)
        {
            if (result.Moved <= 0)
            {
                if (result.LeftBehind > 0)
                {
                    return VillageChestTexts.Get(localization, storing ? VillageChestTexts.StatusChestFull : VillageChestTexts.StatusBagFull);
                }

                return VillageChestTexts.Get(localization, storing ? VillageChestTexts.StatusNothingToStore : VillageChestTexts.StatusNothingForQuests);
            }

            if (result.LeftBehind > 0)
            {
                return VillageChestTexts.Format(localization, storing ? VillageChestTexts.StatusStoredPartial : VillageChestTexts.StatusTakenPartial, result.Moved, result.LeftBehind);
            }

            return VillageChestTexts.Format(localization, storing ? VillageChestTexts.StatusStored : VillageChestTexts.StatusTaken, result.Moved);
        }

        // ------------------------------------------------------------------
        // Lifecycle
        // ------------------------------------------------------------------

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            if (_uiDocument == null)
            {
                _uiDocument = GetComponent<UIDocument>();
            }

            _tooltip = TooltipController.GetOrAdd(gameObject);
            BindElements();
            RegisterCallbacks();
            _focusNavigator = FocusNavigator.Attach(_root, () => IsOpen, Close, () => _closeButton);
            EventBus.Subscribe<InventoryChangedEvent>(OnContentsChanged);
            EventBus.Subscribe<VillageChestChangedEvent>(OnChestChanged);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            SetVisible(false);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnContentsChanged);
            EventBus.Unsubscribe<VillageChestChangedEvent>(OnChestChanged);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            _focusNavigator?.Detach();
            _focusNavigator = null;
            UnregisterCallbacks();
            EndDrag();
            _open = false;
            GameplayPause.Release(this);
        }

        public void Open(InventoryManager bag, VillageChestStorage chest, Func<string, int, int> outstandingForQuests)
        {
            if (_root == null || bag == null || chest == null)
            {
                return;
            }

            _bag = bag;
            _chest = chest;
            _outstandingForQuests = outstandingForQuests;
            ClearSelection();
            SetStatus(string.Empty);
            UiMotionPreferences.ApplyTo(_root, UiMotionPreferences.IsReducedMotion());

            _open = true;
            SetVisible(true);
            GameplayPause.Acquire(this);
            Rebuild();
            _focusNavigator?.FocusInitial();
        }

        public void Close()
        {
            if (!_open)
            {
                return;
            }

            _tooltip?.Hide();
            EndDrag();
            _focusNavigator?.ReleaseFocus();
            _open = false;
            SetVisible(false);
            GameplayPause.Release(this);
            ClearSelection();
            ClearGrid(_bagGrid);
            ClearGrid(_chestGrid);
            _bag = null;
            _outstandingForQuests = null;
        }

        private void SetVisible(bool visible)
        {
            if (_root != null)
            {
                _root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void BindElements()
        {
            VisualElement tree = _uiDocument != null ? _uiDocument.rootVisualElement : null;
            if (tree == null)
            {
                return;
            }

            _root = tree.Q<VisualElement>(RootName);
            _titleLabel = tree.Q<Label>("village-chest-title");
            _subtitleLabel = tree.Q<Label>("village-chest-subtitle");
            _bagTitleLabel = tree.Q<Label>("village-chest-bag-title");
            _chestTitleLabel = tree.Q<Label>("village-chest-chest-title");
            _bagWeightLabel = tree.Q<Label>("village-chest-bag-weight");
            _chestSlotsLabel = tree.Q<Label>("village-chest-chest-slots");
            _bagPane = tree.Q<VisualElement>("village-chest-bag-pane");
            _chestPane = tree.Q<VisualElement>("village-chest-chest-pane");
            _bagGrid = tree.Q<VisualElement>(BagGridName);
            _chestGrid = tree.Q<VisualElement>(ChestGridName);
            _chestEmptyLabel = tree.Q<Label>("village-chest-empty");
            _amountBar = tree.Q<VisualElement>("village-chest-amount-bar");
            _amountLabel = tree.Q<Label>("village-chest-amount-label");
            _amountValue = tree.Q<Label>("village-chest-amount-value");
            _amountMinus = tree.Q<Button>("village-chest-amount-minus");
            _amountPlus = tree.Q<Button>("village-chest-amount-plus");
            _amountAll = tree.Q<Button>("village-chest-amount-all");
            _amountMove = tree.Q<Button>("village-chest-amount-move");
            _amountCancel = tree.Q<Button>("village-chest-amount-cancel");
            _storeMaterialsButton = tree.Q<Button>(StoreMaterialsButtonName);
            _takeForQuestsButton = tree.Q<Button>(TakeForQuestsButtonName);
            _closeButton = tree.Q<Button>(CloseButtonName);
            _statusLabel = tree.Q<Label>("village-chest-status");
            _hintLabel = tree.Q<Label>("village-chest-hint");

            if (_root != null && _dragGhost == null)
            {
                // Same ghost look as the Mochila's drag (InventoryView.uss); never pickable.
                _dragGhost = new Label { name = "village-chest-drag-ghost", pickingMode = PickingMode.Ignore };
                _dragGhost.AddToClassList("inventory-drag-ghost");
                _dragGhost.style.display = DisplayStyle.None;
                _root.Add(_dragGhost);
            }
        }

        private void RegisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked += Close;
            if (_storeMaterialsButton != null) _storeMaterialsButton.clicked += OnStoreMaterialsClicked;
            if (_takeForQuestsButton != null) _takeForQuestsButton.clicked += OnTakeForQuestsClicked;
            if (_amountMinus != null) _amountMinus.clicked += OnAmountMinus;
            if (_amountPlus != null) _amountPlus.clicked += OnAmountPlus;
            if (_amountAll != null) _amountAll.clicked += OnAmountAll;
            if (_amountMove != null) _amountMove.clicked += OnAmountMove;
            if (_amountCancel != null) _amountCancel.clicked += OnAmountCancel;

            if (_root != null)
            {
                _root.RegisterCallback<PointerMoveEvent>(OnRootPointerMove, TrickleDown.TrickleDown);
                _root.RegisterCallback<PointerUpEvent>(OnRootPointerUp, TrickleDown.TrickleDown);
                _root.RegisterCallback<PointerCancelEvent>(OnRootPointerCancel, TrickleDown.TrickleDown);
            }
        }

        private void UnregisterCallbacks()
        {
            if (_closeButton != null) _closeButton.clicked -= Close;
            if (_storeMaterialsButton != null) _storeMaterialsButton.clicked -= OnStoreMaterialsClicked;
            if (_takeForQuestsButton != null) _takeForQuestsButton.clicked -= OnTakeForQuestsClicked;
            if (_amountMinus != null) _amountMinus.clicked -= OnAmountMinus;
            if (_amountPlus != null) _amountPlus.clicked -= OnAmountPlus;
            if (_amountAll != null) _amountAll.clicked -= OnAmountAll;
            if (_amountMove != null) _amountMove.clicked -= OnAmountMove;
            if (_amountCancel != null) _amountCancel.clicked -= OnAmountCancel;

            if (_root != null)
            {
                _root.UnregisterCallback<PointerMoveEvent>(OnRootPointerMove, TrickleDown.TrickleDown);
                _root.UnregisterCallback<PointerUpEvent>(OnRootPointerUp, TrickleDown.TrickleDown);
                _root.UnregisterCallback<PointerCancelEvent>(OnRootPointerCancel, TrickleDown.TrickleDown);
            }
        }

        private void OnContentsChanged(InventoryChangedEvent evt) => RefreshIfOpen();

        private void OnChestChanged(VillageChestChangedEvent evt) => RefreshIfOpen();

        private void OnGameLoaded(GameLoadedEvent evt) => RefreshIfOpen();

        private void RefreshIfOpen()
        {
            // Mid-drag the tiles stay (rebuilding would destroy the dragged one); the drop refreshes.
            if (_open && !_isDragging)
            {
                Rebuild();
            }
        }

        // ------------------------------------------------------------------
        // Building
        // ------------------------------------------------------------------

        private void Rebuild()
        {
            if (!_open || _bag == null || _chest == null)
            {
                return;
            }

            _tooltip?.Hide();
            LocalizationManager localization = Localization;

            SetText(_titleLabel, VillageChestTexts.Get(localization, VillageChestTexts.Title));
            SetText(_subtitleLabel, VillageChestTexts.Get(localization, VillageChestTexts.Subtitle));
            SetText(_bagTitleLabel, VillageChestTexts.Get(localization, VillageChestTexts.BagPane));
            SetText(_chestTitleLabel, VillageChestTexts.Get(localization, VillageChestTexts.ChestPane));
            SetText(_bagWeightLabel, VillageChestTexts.Format(localization, VillageChestTexts.BagWeight,
                _bag.GetTotalWeight().ToString("0.#", CultureInfo.InvariantCulture),
                _bag.MaxWeight.ToString("0.#", CultureInfo.InvariantCulture)));
            SetText(_chestSlotsLabel, VillageChestTexts.Format(localization, VillageChestTexts.ChestSlots, _chest.UsedSlotCount, _chest.SlotCount));
            SetText(_chestEmptyLabel, VillageChestTexts.Get(localization, VillageChestTexts.EmptyChest));
            SetText(_storeMaterialsButton, VillageChestTexts.Get(localization, VillageChestTexts.StoreMaterials));
            SetText(_takeForQuestsButton, VillageChestTexts.Get(localization, VillageChestTexts.TakeForQuests));
            SetText(_hintLabel, VillageChestTexts.Get(localization, VillageChestTexts.Hint));
            SetText(_amountAll, VillageChestTexts.Get(localization, VillageChestTexts.AmountAll));
            SetText(_amountMove, VillageChestTexts.Get(localization, VillageChestTexts.AmountMove));
            SetText(_amountCancel, VillageChestTexts.Get(localization, VillageChestTexts.AmountCancel));
            if (_closeButton != null)
            {
                _closeButton.tooltip = VillageChestTexts.Get(localization, VillageChestTexts.Close);
            }

            if (_chestEmptyLabel != null)
            {
                _chestEmptyLabel.style.display = _chest.IsEmpty ? DisplayStyle.Flex : DisplayStyle.None;
            }

            BuildGrid(_bagGrid, Pane.Bag, _bag.Slots);
            BuildGrid(_chestGrid, Pane.Chest, _chest.Slots);
            ValidateSelection();
            RefreshAmountBar();
            _focusNavigator?.RestoreFocusAfterRebuild();
        }

        private void BuildGrid(VisualElement grid, Pane pane, IReadOnlyList<InventorySlotData> slots)
        {
            if (grid == null)
            {
                return;
            }

            ClearGrid(grid);
            Func<string, string> localize = TooltipController.GetLocalizer();

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                ItemDefinition item = slot.IsEmpty ? null : ItemDatabase.GetById(slot.itemId);

                if (item == null)
                {
                    // Not focusable: gamepad navigation only stops on items.
                    var empty = new VisualElement();
                    empty.AddToClassList(SlotClass);
                    empty.AddToClassList(EmptySlotClass);
                    grid.Add(empty);
                    continue;
                }

                int index = i;
                string itemName = GeneratedItemName.Compose(item, slot, ContentText.CurrentManager);
                var button = new Button(() => OnTileClicked(pane, index)) { text = itemName, focusable = true };
                button.AddToClassList(SlotClass);
                button.AddToClassList("inventory-row--" + TooltipLocalization.ToSnakeCase(TooltipContentBuilder.ResolveRarity(item, slot).ToString()));
                if (_hasSelection && _selectedPane == pane && _selectedIndex == i)
                {
                    button.AddToClassList(SelectedClass);
                }

                if (slot.quantity > 1)
                {
                    var quantity = new Label("x" + slot.quantity) { pickingMode = PickingMode.Ignore };
                    quantity.AddToClassList(QuantityClass);
                    button.Add(quantity);
                }

                InventorySlotData tooltipSlot = slot;
                TooltipTrigger.Bind(button, _tooltip, () => TooltipContentBuilder.ForItem(item, tooltipSlot, localize));
                RegisterDragSource(button, pane, index, slot.quantity > 1 ? $"{itemName} x{slot.quantity}" : itemName);
                grid.Add(button);
            }
        }

        private static void ClearGrid(VisualElement grid)
        {
            grid?.Clear();
        }

        // ------------------------------------------------------------------
        // Moves
        // ------------------------------------------------------------------

        private InventorySlotData SlotAt(Pane pane, int index)
        {
            IReadOnlyList<InventorySlotData> slots = pane == Pane.Bag ? _bag?.Slots : _chest?.Slots;
            return slots != null && index >= 0 && index < slots.Count ? slots[index] : null;
        }

        private void OnTileClicked(Pane pane, int index)
        {
            InventorySlotData slot = SlotAt(pane, index);
            if (slot == null || slot.IsEmpty)
            {
                return;
            }

            if (NeedsAmountPicker(slot))
            {
                _hasSelection = true;
                _selectedPane = pane;
                _selectedIndex = index;
                _selectedItemId = slot.itemId;
                _amount = slot.quantity;
                Rebuild();
                _amountMove?.Focus();
                return;
            }

            MoveStack(pane, index, slot.quantity);
        }

        /// <summary>Moves <paramref name="amount"/> units of a tile to the other side and reports what happened.</summary>
        private void MoveStack(Pane pane, int index, int amount)
        {
            if (_bag == null || _chest == null)
            {
                return;
            }

            LocalizationManager localization = Localization;
            int moved;
            if (pane == Pane.Bag)
            {
                moved = VillageChestTransfer.StoreFromBag(_bag, index, amount, _chest);
                SetStatus(moved <= 0
                    ? VillageChestTexts.Get(localization, VillageChestTexts.StatusChestFull)
                    : moved < amount
                        ? VillageChestTexts.Format(localization, VillageChestTexts.StatusStoredPartial, moved, amount - moved)
                        : VillageChestTexts.Format(localization, VillageChestTexts.StatusStored, moved));
            }
            else
            {
                moved = VillageChestTransfer.TakeToBag(_chest, index, amount, _bag);
                SetStatus(moved <= 0
                    ? VillageChestTexts.Get(localization, VillageChestTexts.StatusBagFull)
                    : moved < amount
                        ? VillageChestTexts.Format(localization, VillageChestTexts.StatusTakenPartial, moved, amount - moved)
                        : VillageChestTexts.Format(localization, VillageChestTexts.StatusTaken, moved));
            }

            ClearSelection();
            Rebuild();
        }

        private void OnStoreMaterialsClicked()
        {
            if (_bag == null || _chest == null)
            {
                return;
            }

            ClearSelection();
            VillageChestBulkResult result = VillageChestTransfer.StoreMaterials(_bag, _chest, _outstandingForQuests);
            SetStatus(DescribeBulkResult(true, result, Localization));
            Rebuild();
        }

        private void OnTakeForQuestsClicked()
        {
            if (_bag == null || _chest == null)
            {
                return;
            }

            ClearSelection();
            VillageChestBulkResult result = VillageChestTransfer.TakeForQuests(_chest, _bag, _outstandingForQuests);
            SetStatus(DescribeBulkResult(false, result, Localization));
            Rebuild();
        }

        // ------------------------------------------------------------------
        // Amount bar
        // ------------------------------------------------------------------

        private void ValidateSelection()
        {
            if (!_hasSelection)
            {
                return;
            }

            InventorySlotData slot = SlotAt(_selectedPane, _selectedIndex);
            if (slot == null || slot.IsEmpty || slot.itemId != _selectedItemId || slot.quantity <= 1)
            {
                ClearSelection();
                return;
            }

            _amount = ClampAmount(_amount, slot.quantity);
        }

        private void ClearSelection()
        {
            _hasSelection = false;
            _selectedIndex = -1;
            _selectedItemId = null;
            _amount = 0;
            RefreshAmountBar();
        }

        private void RefreshAmountBar()
        {
            if (_amountBar == null)
            {
                return;
            }

            InventorySlotData slot = _hasSelection ? SlotAt(_selectedPane, _selectedIndex) : null;
            bool show = slot != null && !slot.IsEmpty;
            _amountBar.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show)
            {
                return;
            }

            ItemDefinition item = ItemDatabase.GetById(slot.itemId);
            string itemName = item != null ? GeneratedItemName.Compose(item, slot, ContentText.CurrentManager) : slot.itemId;
            LocalizationManager localization = Localization;
            SetText(_amountLabel, VillageChestTexts.Format(localization,
                _selectedPane == Pane.Bag ? VillageChestTexts.AmountToChest : VillageChestTexts.AmountToBag, itemName));
            SetText(_amountValue, string.Format(CultureInfo.InvariantCulture, "{0}/{1}", _amount, slot.quantity));
            _amountMinus?.SetEnabled(_amount > 1);
            _amountPlus?.SetEnabled(_amount < slot.quantity);
        }

        private void StepAmount(int delta)
        {
            InventorySlotData slot = _hasSelection ? SlotAt(_selectedPane, _selectedIndex) : null;
            if (slot == null)
            {
                return;
            }

            _amount = ClampAmount(_amount + delta, slot.quantity);
            RefreshAmountBar();
        }

        private void OnAmountMinus() => StepAmount(-1);

        private void OnAmountPlus() => StepAmount(1);

        private void OnAmountAll()
        {
            InventorySlotData slot = _hasSelection ? SlotAt(_selectedPane, _selectedIndex) : null;
            if (slot != null)
            {
                _amount = slot.quantity;
                RefreshAmountBar();
            }
        }

        private void OnAmountMove()
        {
            if (_hasSelection)
            {
                MoveStack(_selectedPane, _selectedIndex, _amount);
            }
        }

        private void OnAmountCancel()
        {
            ClearSelection();
            Rebuild();
            _closeButton?.Focus();
        }

        // ------------------------------------------------------------------
        // Drag & drop (whole stack onto the other pane)
        // ------------------------------------------------------------------

        private void RegisterDragSource(VisualElement element, Pane pane, int index, string label)
        {
            element.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (_isDragging || !evt.isPrimary || (evt.pointerType == UnityEngine.UIElements.PointerType.mouse && evt.button != 0))
                {
                    return;
                }

                _pressPointerId = evt.pointerId;
                _pressPosition = evt.position;
                _dragPane = pane;
                _dragIndex = index;
                _dragSourceElement = element;
                _dragLabel = label;
            }, TrickleDown.TrickleDown);
        }

        private void OnRootPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pressPointerId)
            {
                return;
            }

            if (!_isDragging)
            {
                if (!InventoryDragDrop.HasExceededDragThreshold(_pressPosition, evt.position))
                {
                    return;
                }

                BeginDrag();
                UpdateDrag(evt.position);
                return;
            }

            UpdateDrag(evt.position);
            evt.StopPropagation();
        }

        private void OnRootPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pressPointerId)
            {
                return;
            }

            if (!_isDragging)
            {
                _pressPointerId = -1;
                _dragSourceElement = null;
                return;
            }

            evt.StopPropagation();
            _root.panel?.ReleasePointer(evt.pointerId);

            Pane source = _dragPane;
            int index = _dragIndex;
            bool overTarget = IsOverPane(OtherPane(source), evt.position);
            EndDrag();

            InventorySlotData slot = SlotAt(source, index);
            if (overTarget && slot != null && !slot.IsEmpty)
            {
                MoveStack(source, index, slot.quantity);
            }
            else
            {
                Rebuild();
            }
        }

        private void OnRootPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != _pressPointerId)
            {
                return;
            }

            bool wasDragging = _isDragging;
            EndDrag();
            if (wasDragging)
            {
                RefreshIfOpen();
            }
        }

        private static Pane OtherPane(Pane pane) => pane == Pane.Bag ? Pane.Chest : Pane.Bag;

        private VisualElement PaneElement(Pane pane) => pane == Pane.Bag ? _bagPane : _chestPane;

        private bool IsOverPane(Pane pane, Vector2 pointerPosition)
        {
            VisualElement element = PaneElement(pane);
            return element != null && element.worldBound.Contains(pointerPosition);
        }

        private void BeginDrag()
        {
            _isDragging = true;
            _tooltip?.Hide();
            _dragSourceElement?.AddToClassList(DragSourceClass);

            if (_dragGhost != null)
            {
                _dragGhost.text = _dragLabel;
                _dragGhost.style.display = DisplayStyle.Flex;
                _dragGhost.BringToFront();
            }
        }

        private void UpdateDrag(Vector2 pointerPosition)
        {
            if (_tooltip != null && _tooltip.IsVisible)
            {
                _tooltip.Hide();
            }

            if (_dragGhost != null && _root != null)
            {
                Vector2 local = _root.WorldToLocal(pointerPosition);
                _dragGhost.style.left = local.x - 24f;
                _dragGhost.style.top = local.y - 56f;
            }

            PaneElement(OtherPane(_dragPane))?.EnableInClassList(DropPaneClass, IsOverPane(OtherPane(_dragPane), pointerPosition));
        }

        private void EndDrag()
        {
            _dragSourceElement?.RemoveFromClassList(DragSourceClass);
            _bagPane?.RemoveFromClassList(DropPaneClass);
            _chestPane?.RemoveFromClassList(DropPaneClass);

            if (_dragGhost != null)
            {
                _dragGhost.style.display = DisplayStyle.None;
            }

            _isDragging = false;
            _pressPointerId = -1;
            _dragIndex = -1;
            _dragSourceElement = null;
        }

        // ------------------------------------------------------------------

        private void SetStatus(string text)
        {
            if (_statusLabel != null)
            {
                _statusLabel.text = text ?? string.Empty;
                _statusLabel.style.display = string.IsNullOrEmpty(text) ? DisplayStyle.None : DisplayStyle.Flex;
            }
        }

        private static void SetText(TextElement element, string text)
        {
            if (element != null && element.text != text)
            {
                element.text = text;
            }
        }
    }
}
