using System;
using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.UI.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Inventory
{
    /// <summary>
    /// Drives the backpack/equipment window (InventoryView.uxml/.uss), following the same
    /// single-<see cref="UIDocument"/>, <c>Q&lt;T&gt;()</c>-resolved-once, rows-generated-at-runtime
    /// pattern as <c>ShopUIController</c>. Purely reactive: reads <see cref="InventoryManager.Slots"/>/
    /// <see cref="InventoryManager.Equipped"/> and forwards input back into the manager; refreshes on
    /// <see cref="InventoryChangedEvent"/>. Opened from the HUD's "Mochila" button (<c>HudController</c>) via <see cref="Toggle"/>.
    ///
    /// The backpack is an MU Online-style grid (<see cref="InventoryManager.Columns"/> × <see cref="InventoryManager.Rows"/>):
    /// background cells, and every item drawn over its footprint (<see cref="ItemFootprint"/>) with its rarity border colour.
    /// <list type="bullet">
    /// <item>PC: drag an item (press and move past <see cref="InventoryDragDrop.DragThresholdPixels"/>) — a ghost of its
    /// footprint follows the pointer, green where it fits, red where it does not; drop on the grid moves/merges/swaps
    /// (<see cref="InventoryManager.MoveItem"/>), on its equipment slot equips. Right-click uses/equips at once.</item>
    /// <item>Mobile (and mouse clicks, gamepad A): tap an item to pick it up, tap a cell (or another item) to put it
    /// there, tap the held item again to use/equip it (<see cref="InventoryDragDrop.ResolveTap"/>). While something is
    /// held, the free cells become focusable so a gamepad can place it too.</item>
    /// <item>Hold (touch) / hover (mouse) / focus (gamepad): tooltip (<see cref="TooltipTrigger"/>).</item>
    /// <item>Equipped item dragged (or picked from its row) onto the grid: unequips into that spot.</item>
    /// <item>"Organizar": <see cref="InventoryManager.SortInventory"/>. The header chip shows used/total cells.</item>
    /// </list>
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class InventoryUIController : MonoBehaviour
    {
        private static readonly EquipmentSlot[] DisplayedEquipmentSlots =
        {
            EquipmentSlot.Head,
            EquipmentSlot.Chest,
            EquipmentSlot.Legs,
            EquipmentSlot.Boots,
            EquipmentSlot.MainHand,
            EquipmentSlot.OffHand,
            EquipmentSlot.Tool,
            EquipmentSlot.Accessory,
            EquipmentSlot.Wings,
        };

        /// <summary>Pixel size of one grid cell (8 cells = 416px; 52px keeps touch targets above 44px).</summary>
        public const float CellSize = 52f;

        public const string BagGridName = "inventory-bag-grid";
        public const string BagGhostName = "inventory-bag-ghost";

        private const string DragSourceClass = "inventory-slot--drag-source";
        private const string HeldClass = "inventory-bag-item--held";
        private const string DropValidClass = "inventory-drop--valid";
        private const string DropInvalidClass = "inventory-drop--invalid";
        private const string GhostValidClass = "inventory-bag-ghost--valid";
        private const string GhostInvalidClass = "inventory-bag-ghost--invalid";

        [Header("References")]
        [Tooltip("Player's InventoryManager. If empty, resolved lazily from the scene on Show().")]
        [SerializeField] private InventoryManager _inventory;

        private UIDocument _uiDocument;
        private VisualElement _root;
        private Label _titleLabel;
        private Label _weightLabel;
        private Label _equipmentTitleLabel;
        private Label _itemsTitleLabel;
        private Label _hintLabel;
        private VisualElement _equipmentListContainer;
        private VisualElement _itemListContainer;
        private Button _closeButton;
        private Button _sortButton;
        private Label _dragGhost;

        private VisualElement _bagGrid;
        private VisualElement _cellGhost;
        private readonly List<VisualElement> _cells = new List<VisualElement>();

        private TooltipController _tooltip;
        private FocusNavigator _focusNavigator;
        private readonly List<VisualElement> _spawnedEquipmentRows = new List<VisualElement>();
        private readonly List<VisualElement> _spawnedItemRows = new List<VisualElement>();

        // Drop targets for drags: equipment rows (+ the grid, handled separately).
        private sealed class DropTarget
        {
            public VisualElement Element;
            public InventoryDragEndpoint Endpoint;
            public ScrollView Scroll;
        }

        private readonly List<DropTarget> _dropTargets = new List<DropTarget>();
        private readonly List<Rect> _dropTargetRects = new List<Rect>();
        private readonly List<Rect> _dropClipRects = new List<Rect>();

        // Press / drag state.
        private int _pressPointerId = -1;
        private Vector2 _pressPosition;
        private InventoryDragEndpoint _dragSource;
        private EquipmentSlot _dragSourceEquipSlot;
        private VisualElement _dragSourceElement;
        private string _dragLabel;
        private bool _isDragging;
        private DropTarget _hoveredTarget;
        private Vector2Int _grab;

        // Tap-to-pick state (mobile, clicks, gamepad).
        private int _heldIndex = -1;
        private Vector2Int _heldGrab;
        private Vector2 _lastPointerPosition;
        private bool _lastActivationWasPointer;

        public bool IsVisible => _root != null && _root.style.display == DisplayStyle.Flex;

        /// <summary>Bag entry picked up by a tap, or -1.</summary>
        public int HeldIndex => _heldIndex;

        private void Awake()
        {
            _uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _tooltip = TooltipController.GetOrAdd(gameObject);
            BindElements();
            RegisterCallbacks();
            _focusNavigator = FocusNavigator.Attach(_root, () => IsVisible, Hide, () => _closeButton);
            Hide();
        }

        private void OnDisable()
        {
            _focusNavigator?.Detach();
            _focusNavigator = null;
            UnregisterCallbacks();
            GameplayPause.Release(this);
        }

        /// <summary>Lets a bootstrap script assign the player's inventory explicitly.</summary>
        public void SetInventory(InventoryManager inventory)
        {
            _inventory = inventory;
        }

        private void BindElements()
        {
            VisualElement rootVisualElement = _uiDocument.rootVisualElement;

            _root = rootVisualElement.Q<VisualElement>("inventory-root");
            _titleLabel = rootVisualElement.Q<Label>("inventory-title");
            _weightLabel = rootVisualElement.Q<Label>("inventory-weight");
            _equipmentTitleLabel = rootVisualElement.Q<Label>("inventory-equipment-title");
            _itemsTitleLabel = rootVisualElement.Q<Label>("inventory-items-title");
            _hintLabel = rootVisualElement.Q<Label>("inventory-hint");
            _equipmentListContainer = rootVisualElement.Q<VisualElement>("inventory-equipment-list");
            _itemListContainer = rootVisualElement.Q<VisualElement>("inventory-item-list");
            _closeButton = rootVisualElement.Q<Button>("inventory-close-button");
            _sortButton = rootVisualElement.Q<Button>("inventory-sort-button");

            if (_root != null && _dragGhost == null)
            {
                // Last child of the full-screen root so it draws above the window; never pickable,
                // so it doesn't hide the cell under the finger from hit testing.
                _dragGhost = new Label { name = "inventory-drag-ghost", pickingMode = PickingMode.Ignore };
                _dragGhost.AddToClassList("inventory-drag-ghost");
                _dragGhost.style.display = DisplayStyle.None;
                _root.Add(_dragGhost);
            }
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);

            if (_closeButton != null)
            {
                _closeButton.clicked += Hide;
            }

            if (_sortButton != null)
            {
                _sortButton.clicked += OnSortClicked;
            }

            if (_root != null)
            {
                // TrickleDown on the full-screen root: runs before the ScrollViews/tiles below, so an
                // active drag can keep the moves to itself (no list scrolling, no button clicking).
                _root.RegisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
                _root.RegisterCallback<PointerMoveEvent>(OnRootPointerMove, TrickleDown.TrickleDown);
                _root.RegisterCallback<PointerUpEvent>(OnRootPointerUp, TrickleDown.TrickleDown);
                _root.RegisterCallback<PointerCancelEvent>(OnRootPointerCancel, TrickleDown.TrickleDown);
            }
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);

            if (_closeButton != null)
            {
                _closeButton.clicked -= Hide;
            }

            if (_sortButton != null)
            {
                _sortButton.clicked -= OnSortClicked;
            }

            if (_root != null)
            {
                _root.UnregisterCallback<PointerDownEvent>(OnRootPointerDown, TrickleDown.TrickleDown);
                _root.UnregisterCallback<PointerMoveEvent>(OnRootPointerMove, TrickleDown.TrickleDown);
                _root.UnregisterCallback<PointerUpEvent>(OnRootPointerUp, TrickleDown.TrickleDown);
                _root.UnregisterCallback<PointerCancelEvent>(OnRootPointerCancel, TrickleDown.TrickleDown);
            }

            EndDrag();
            ReleaseHeld();
            ClearRows();
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            // Mid-drag the tiles stay as they are (rebuilding would destroy the dragged tile); the drop
            // itself refreshes.
            if (IsVisible && !_isDragging)
            {
                RefreshAll();
            }
        }

        public void Show()
        {
            if (_root == null)
            {
                return;
            }

            // Lazy resolve, same reason as the ShopUIController/SaveLoadMenuController fixes in
            // TODO.md: scene object init order isn't guaranteed, so never resolve only once.
            if (_inventory == null)
            {
                _inventory = FindFirstObjectByType<InventoryManager>();
            }

            _root.style.display = DisplayStyle.Flex;
            GameplayPause.Acquire(this);
            RefreshAll();
            _focusNavigator?.FocusInitial();
        }

        public void Hide()
        {
            _tooltip?.Hide();
            EndDrag();
            ReleaseHeld();
            _focusNavigator?.ReleaseFocus();

            if (_root != null)
            {
                _root.style.display = DisplayStyle.None;
            }

            GameplayPause.Release(this);

            ClearRows();
        }

        public void Toggle()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void OnSortClicked()
        {
            ReleaseHeld();
            _inventory?.SortInventory();
        }

        private void RefreshAll()
        {
            _tooltip?.Hide();
            Func<string, string> localize = TooltipController.GetLocalizer();

            SetText(_titleLabel, Localize(localize, "ui.inventory.title"));
            SetText(_equipmentTitleLabel, Localize(localize, "ui.inventory.equipment"));
            SetText(_itemsTitleLabel, Localize(localize, "ui.inventory.items"));
            SetText(_hintLabel, Localize(localize, "ui.inventory.hint"));

            if (_sortButton != null)
            {
                _sortButton.text = Localize(localize, "ui.inventory.sort");
            }

            if (_inventory == null)
            {
                SetText(_weightLabel, string.Empty);
                ClearRows();
                return;
            }

            if (_heldIndex >= 0 && (_heldIndex >= _inventory.Slots.Count || _inventory.Slots[_heldIndex].IsEmpty))
            {
                _heldIndex = -1;
            }

            string spacePattern = Localize(localize, "ui.inventory.space");
            SetText(_weightLabel, string.Format(CultureInfo.InvariantCulture, spacePattern, _inventory.UsedCellCount, _inventory.CellCount));

            _dropTargets.Clear();
            RebuildEquipmentRows(localize);
            RebuildBagGrid();
            _focusNavigator?.RestoreFocusAfterRebuild();
        }

        private void RebuildEquipmentRows(Func<string, string> localize)
        {
            ClearRows(_equipmentListContainer, _spawnedEquipmentRows);

            if (_equipmentListContainer == null)
            {
                return;
            }

            ScrollView scroll = _equipmentListContainer.GetFirstAncestorOfType<ScrollView>();

            foreach (EquipmentSlot equipSlot in DisplayedEquipmentSlots)
            {
                string slotName = Localize(localize, "ui.tooltip.slot." + TooltipLocalization.ToSnakeCase(equipSlot.ToString()));
                _inventory.Equipped.TryGetValue(equipSlot, out InventorySlotData equippedData);
                ItemDefinition item = equippedData != null ? ItemDatabase.GetById(equippedData.itemId) : null;

                EquipmentSlot capturedSlot = equipSlot;
                var row = new Button(() => OnEquipmentRowClicked(capturedSlot));
                row.AddToClassList("inventory-row-button");
                row.AddToClassList("inventory-equipment-row");

                if (item != null)
                {
                    string itemName = GeneratedItemName.Compose(item, equippedData, ContentText.CurrentManager);
                    row.text = $"{slotName}: {itemName}";
                    AddRarityClass(row, TooltipContentBuilder.ResolveRarity(item, equippedData));

                    InventorySlotData tooltipData = equippedData;
                    TooltipTrigger.Bind(row, _tooltip, () => TooltipContentBuilder.ForItem(item, tooltipData, TooltipController.GetLocalizer()));
                    RegisterDragSource(row, InventoryDragEndpoint.Equipment(equipSlot), equipSlot, itemName, Vector2Int.zero);
                }
                else
                {
                    // Nothing to unequip, but still a drop/tap target for gear: enabled only while gear for it is held.
                    row.text = $"{slotName}: {Localize(localize, "ui.tooltip.empty_slot")}";
                    row.SetEnabled(HeldItemEquipSlot() == equipSlot);
                }

                _dropTargets.Add(new DropTarget { Element = row, Endpoint = InventoryDragEndpoint.Equipment(equipSlot), Scroll = scroll });
                _equipmentListContainer.Add(row);
                _spawnedEquipmentRows.Add(row);
            }
        }

        private void OnEquipmentRowClicked(EquipmentSlot equipSlot)
        {
            if (_inventory == null)
            {
                return;
            }

            if (_heldIndex >= 0)
            {
                int held = _heldIndex;
                ReleaseHeld();
                if (HeldItemEquipSlot(held) == equipSlot)
                {
                    _inventory.EquipFromSlot(held);
                }

                RefreshIfVisible();
                return;
            }

            _inventory.UnequipSlot(equipSlot);
        }

        // ---------------------------------------------------------------
        // Grid
        // ---------------------------------------------------------------

        private void RebuildBagGrid()
        {
            ClearRows(_itemListContainer, _spawnedItemRows);
            _cells.Clear();
            _bagGrid = null;
            _cellGhost = null;

            if (_itemListContainer == null)
            {
                return;
            }

            int columns = _inventory.Columns;
            int rows = _inventory.Rows;

            var grid = new VisualElement { name = BagGridName };
            grid.AddToClassList(BagGridName);
            grid.style.width = columns * CellSize;
            grid.style.height = rows * CellSize;
            grid.RegisterCallback<PointerMoveEvent>(OnGridPointerMove);
            grid.RegisterCallback<PointerUpEvent>(OnGridPointerUp);
            grid.RegisterCallback<PointerLeaveEvent>(_ => UpdateHeldGhostAtCurrentSpot());

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    var cell = new VisualElement { name = $"inventory-cell-{x}-{y}", pickingMode = PickingMode.Ignore };
                    cell.AddToClassList("inventory-cell");
                    Place(cell, x, y, 1, 1);

                    int cx = x;
                    int cy = y;
                    cell.RegisterCallback<NavigationSubmitEvent>(evt =>
                    {
                        _lastActivationWasPointer = false;
                        OnCellActivated(cx, cy);
                        evt.StopPropagation();
                    });

                    grid.Add(cell);
                    _cells.Add(cell);
                }
            }

            IReadOnlyList<InventorySlotData> slots = _inventory.Slots;
            BagGrid occupancy = _inventory.BuildGrid();

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                if (slot == null || slot.IsEmpty)
                {
                    continue;
                }

                GridRect rect = _inventory.GetSlotRect(i);
                if (occupancy.At(rect.X, rect.Y) != i)
                {
                    continue; // not on the grid (should not happen; never draw two items on one cell)
                }

                grid.Add(CreateItemTile(i, slot, rect));
            }

            _cellGhost = new VisualElement { name = BagGhostName, pickingMode = PickingMode.Ignore };
            _cellGhost.AddToClassList(BagGhostName);
            _cellGhost.style.display = DisplayStyle.None;
            grid.Add(_cellGhost);

            _itemListContainer.Add(grid);
            _spawnedItemRows.Add(grid);
            _bagGrid = grid;

            SetCellsFocusable(_heldIndex >= 0);
            UpdateHeldGhostAtCurrentSpot();
        }

        private VisualElement CreateItemTile(int slotIndex, InventorySlotData slot, GridRect rect)
        {
            ItemDefinition item = ItemDatabase.GetById(slot.itemId);
            string itemName = item != null ? GeneratedItemName.Compose(item, slot, ContentText.CurrentManager) : slot.itemId;

            var tile = new Button(() => OnTileActivated(slotIndex)) { name = "inventory-bag-item-" + slotIndex };
            tile.AddToClassList("inventory-slot");
            tile.AddToClassList("inventory-bag-item");
            if (slotIndex == _heldIndex)
            {
                tile.AddToClassList(HeldClass);
            }

            Place(tile, rect.X, rect.Y, rect.Width, rect.Height);

            var nameLabel = new Label(itemName) { pickingMode = PickingMode.Ignore };
            nameLabel.AddToClassList("inventory-bag-item-name");
            tile.Add(nameLabel);

            if (item != null)
            {
                AddRarityClass(tile, TooltipContentBuilder.ResolveRarity(item, slot));
                InventorySlotData tooltipSlot = slot;
                TooltipTrigger.Bind(tile, _tooltip, () => TooltipContentBuilder.ForItem(item, tooltipSlot, TooltipController.GetLocalizer()));
            }

            if (slot.quantity > 1)
            {
                var quantity = new Label($"x{slot.quantity}") { pickingMode = PickingMode.Ignore };
                quantity.AddToClassList("inventory-slot-quantity");
                tile.Add(quantity);
            }

            // Right-click = use/equip at once (MU habit).
            tile.RegisterCallback<PointerUpEvent>(evt =>
            {
                if (evt.pointerType == UnityEngine.UIElements.PointerType.mouse && evt.button == 1 && !_isDragging)
                {
                    ReleaseHeld();
                    UseOrEquip(slotIndex);
                    evt.StopPropagation();
                }
            });

            string label = slot.quantity > 1 ? $"{itemName} x{slot.quantity}" : itemName;
            RegisterDragSource(tile, InventoryDragEndpoint.Backpack(slotIndex), item != null ? item.EquipSlot : EquipmentSlot.None, label, Vector2Int.zero);
            return tile;
        }

        private static void Place(VisualElement element, int x, int y, int width, int height)
        {
            element.style.position = Position.Absolute;
            element.style.left = x * CellSize;
            element.style.top = y * CellSize;
            element.style.width = width * CellSize;
            element.style.height = height * CellSize;
        }

        /// <summary>Tile clicked (tap, left click, gamepad A): pick it, use/equip it, or put the held item on it.</summary>
        private void OnTileActivated(int slotIndex)
        {
            if (_inventory == null || _isDragging)
            {
                return;
            }

            bool pointer = _lastActivationWasPointer && !FocusNavigator.NavigationInputActive;
            _lastActivationWasPointer = false;

            switch (InventoryDragDrop.ResolveTap(_heldIndex, slotIndex))
            {
                case BagTapAction.Pick:
                    Vector2Int grab = Vector2Int.zero;
                    if (pointer && TryGetPointerCell(_lastPointerPosition, out int px, out int py))
                    {
                        GridRect rect = _inventory.GetSlotRect(slotIndex);
                        grab = new Vector2Int(Mathf.Clamp(px - rect.X, 0, rect.Width - 1), Mathf.Clamp(py - rect.Y, 0, rect.Height - 1));
                    }

                    Hold(slotIndex, grab);
                    break;

                case BagTapAction.UseOrEquip:
                    ReleaseHeld();
                    UseOrEquip(slotIndex);
                    RefreshIfVisible();
                    break;

                case BagTapAction.Place:
                    if (pointer && TryGetPointerCell(_lastPointerPosition, out int cx, out int cy))
                    {
                        TryPlaceHeld(cx, cy);
                    }
                    else
                    {
                        // Gamepad: onto the target item's own top-left cell.
                        GridRect target = _inventory.GetSlotRect(slotIndex);
                        TryPlaceHeld(target.X + _heldGrab.x, target.Y + _heldGrab.y);
                    }

                    break;
            }
        }

        /// <summary>An empty cell tapped (or submitted with a gamepad while holding).</summary>
        private void OnCellActivated(int x, int y)
        {
            if (_heldIndex >= 0)
            {
                TryPlaceHeld(x, y);
            }
        }

        /// <summary>Puts the held item with its grabbed cell on (cellX, cellY). Keeps holding (red ghost) when it doesn't fit.</summary>
        private void TryPlaceHeld(int cellX, int cellY)
        {
            if (_heldIndex < 0 || _inventory == null)
            {
                return;
            }

            Vector2Int anchor = InventoryDragDrop.AnchorFor(cellX, cellY, _heldGrab.x, _heldGrab.y);
            if (_inventory.MoveItem(_heldIndex, anchor.x, anchor.y))
            {
                ReleaseHeld();
                RefreshIfVisible();
                return;
            }

            ShowGhost(_heldIndex, anchor.x, anchor.y);
        }

        private void Hold(int slotIndex, Vector2Int grab)
        {
            _heldIndex = slotIndex;
            _heldGrab = grab;
            _tooltip?.Hide();
            RefreshIfVisible();
        }

        private void ReleaseHeld()
        {
            if (_heldIndex < 0)
            {
                return;
            }

            _heldIndex = -1;
            _heldGrab = Vector2Int.zero;
            HideGhost();
            SetCellsFocusable(false);
        }

        private void UseOrEquip(int slotIndex)
        {
            if (_inventory == null || slotIndex < 0 || slotIndex >= _inventory.Slots.Count)
            {
                return;
            }

            ItemDefinition item = ItemDatabase.GetById(_inventory.Slots[slotIndex].itemId ?? string.Empty);

            if (item == null)
            {
                return;
            }

            if (item.IsEquippable)
            {
                _inventory.EquipFromSlot(slotIndex);
            }
            else if (item.IsConsumable)
            {
                _inventory.UseItemInSlot(slotIndex);
            }
        }

        private EquipmentSlot HeldItemEquipSlot(int index = -2)
        {
            int held = index == -2 ? _heldIndex : index;
            if (_inventory == null || held < 0 || held >= _inventory.Slots.Count || _inventory.Slots[held].IsEmpty)
            {
                return EquipmentSlot.None;
            }

            ItemDefinition item = ItemDatabase.GetById(_inventory.Slots[held].itemId);
            return item != null ? item.EquipSlot : EquipmentSlot.None;
        }

        private void SetCellsFocusable(bool focusable)
        {
            if (_inventory == null)
            {
                return;
            }

            BagGrid occupancy = focusable ? _inventory.BuildGrid() : null;
            for (int i = 0; i < _cells.Count; i++)
            {
                int x = i % _inventory.Columns;
                int y = i / _inventory.Columns;
                bool canTake = focusable && occupancy.At(x, y) == BagGrid.Empty;
                _cells[i].focusable = canTake;
                _cells[i].pickingMode = PickingMode.Ignore;
            }
        }

        private bool TryGetPointerCell(Vector2 pointerPosition, out int x, out int y)
        {
            x = -1;
            y = -1;
            if (_bagGrid == null || _inventory == null)
            {
                return false;
            }

            Vector2 local = _bagGrid.WorldToLocal(pointerPosition);
            return InventoryDragDrop.TryGetCell(local, CellSize, _inventory.Columns, _inventory.Rows, out x, out y);
        }

        private bool IsOverGrid(Vector2 pointerPosition)
        {
            if (_bagGrid == null)
            {
                return false;
            }

            Rect bounds = _bagGrid.worldBound;
            ScrollView scroll = _bagGrid.GetFirstAncestorOfType<ScrollView>();
            return bounds.Contains(pointerPosition) && (scroll == null || scroll.contentViewport.worldBound.Contains(pointerPosition));
        }

        private void OnGridPointerMove(PointerMoveEvent evt)
        {
            if (_isDragging || _heldIndex < 0 || evt.pointerType != UnityEngine.UIElements.PointerType.mouse)
            {
                return;
            }

            // Mouse hover while holding: preview where the held item would land.
            if (TryGetPointerCell(evt.position, out int x, out int y))
            {
                Vector2Int anchor = InventoryDragDrop.AnchorFor(x, y, _heldGrab.x, _heldGrab.y);
                ShowGhost(_heldIndex, anchor.x, anchor.y);
            }
        }

        private void OnGridPointerUp(PointerUpEvent evt)
        {
            // A tap on an empty cell (the tiles handle their own taps).
            if (_isDragging || evt.target != _bagGrid || (evt.pointerType == UnityEngine.UIElements.PointerType.mouse && evt.button != 0))
            {
                return;
            }

            if (InventoryDragDrop.HasExceededDragThreshold(_pressPosition, evt.position))
            {
                return;
            }

            if (TryGetPointerCell(evt.position, out int x, out int y))
            {
                OnCellActivated(x, y);
            }
        }

        private void UpdateHeldGhostAtCurrentSpot()
        {
            if (_heldIndex < 0 || _inventory == null)
            {
                HideGhost();
                return;
            }

            GridRect rect = _inventory.GetSlotRect(_heldIndex);
            ShowGhost(_heldIndex, rect.X, rect.Y);
        }

        /// <summary>Footprint ghost of entry <paramref name="slotIndex"/> at anchor (x, y): green where it fits, red where it doesn't.</summary>
        private void ShowGhost(int slotIndex, int x, int y)
        {
            if (_cellGhost == null || _inventory == null || slotIndex < 0 || slotIndex >= _inventory.Slots.Count)
            {
                return;
            }

            GridRect rect = _inventory.GetSlotRect(slotIndex);
            ShowGhostRect(x, y, rect.Width, rect.Height, _inventory.PlanMove(slotIndex, x, y).IsValid);
        }

        private void ShowGhostRect(int x, int y, int width, int height, bool valid)
        {
            if (_cellGhost == null || _inventory == null)
            {
                return;
            }

            // Clamp the drawing into the grid (an anchor off the edge still shows, red, at the edge).
            int cx = Mathf.Clamp(x, 0, Mathf.Max(0, _inventory.Columns - width));
            int cy = Mathf.Clamp(y, 0, Mathf.Max(0, _inventory.Rows - height));
            Place(_cellGhost, cx, cy, width, height);
            _cellGhost.EnableInClassList(GhostValidClass, valid);
            _cellGhost.EnableInClassList(GhostInvalidClass, !valid);
            _cellGhost.style.display = DisplayStyle.Flex;
            _cellGhost.BringToFront();
        }

        private void HideGhost()
        {
            if (_cellGhost != null)
            {
                _cellGhost.style.display = DisplayStyle.None;
            }
        }

        private void RefreshIfVisible()
        {
            if (IsVisible && !_isDragging)
            {
                RefreshAll();
            }
        }

        // ---------------------------------------------------------------
        // Drag & drop
        // ---------------------------------------------------------------

        private void RegisterDragSource(VisualElement element, InventoryDragEndpoint endpoint, EquipmentSlot itemEquipSlot, string label, Vector2Int grab)
        {
            // TrickleDown like TooltipTrigger: seen before the Button's Clickable handles the press.
            element.RegisterCallback<PointerDownEvent>(evt =>
            {
                if (_isDragging || !evt.isPrimary || (evt.pointerType == UnityEngine.UIElements.PointerType.mouse && evt.button != 0))
                {
                    return;
                }

                _pressPointerId = evt.pointerId;
                _pressPosition = evt.position;
                _dragSource = endpoint;
                _dragSourceEquipSlot = itemEquipSlot;
                _dragSourceElement = element;
                _dragLabel = label;
                _grab = grab;

                if (endpoint.Location == InventoryDragLocation.BackpackSlot && _inventory != null && TryGetPointerCell(evt.position, out int px, out int py))
                {
                    GridRect rect = _inventory.GetSlotRect(endpoint.SlotIndex);
                    _grab = new Vector2Int(Mathf.Clamp(px - rect.X, 0, rect.Width - 1), Mathf.Clamp(py - rect.Y, 0, rect.Height - 1));
                }
            }, TrickleDown.TrickleDown);
        }

        private void OnRootPointerDown(PointerDownEvent evt)
        {
            _lastPointerPosition = evt.position;
            if (evt.isPrimary)
            {
                _pressPosition = evt.position;
            }
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

                // Not stopped: this same move must still reach the tile so TooltipTrigger cancels its
                // pending long-press (same threshold). Every later move is kept by the drag.
                BeginDrag();
                UpdateDrag(evt.position);
                return;
            }

            UpdateDrag(evt.position);
            evt.StopPropagation();
        }

        private void OnRootPointerUp(PointerUpEvent evt)
        {
            _lastPointerPosition = evt.position;
            _lastActivationWasPointer = true;

            if (evt.pointerId != _pressPointerId)
            {
                return;
            }

            if (!_isDragging)
            {
                // A tap or a long-press: leave it to the tile (OnTileActivated) / TooltipTrigger.
                _pressPointerId = -1;
                _dragSourceElement = null;
                return;
            }

            // Swallow the release: no click on the tile under the finger, no list fling.
            evt.StopPropagation();
            _root.panel?.ReleasePointer(evt.pointerId);
            _lastActivationWasPointer = false;

            InventoryDragEndpoint source = _dragSource;
            EquipmentSlot sourceEquipSlot = _dragSourceEquipSlot;
            Vector2Int grab = _grab;
            Vector2 position = evt.position;
            DropTarget target = FindDropTarget(position);
            bool overGrid = IsOverGrid(position);
            EndDrag();

            if (target != null)
            {
                PerformDrop(source, target.Endpoint, sourceEquipSlot);
            }
            else if (overGrid && TryGetPointerCell(position, out int x, out int y))
            {
                Vector2Int anchor = InventoryDragDrop.AnchorFor(x, y, grab.x, grab.y);
                if (source.Location == InventoryDragLocation.BackpackSlot)
                {
                    _inventory?.MoveItem(source.SlotIndex, anchor.x, anchor.y);
                }
                else if (source.Location == InventoryDragLocation.EquipmentSlot)
                {
                    UnequipInto(source.EquipSlot, anchor.x, anchor.y);
                }
            }

            // Always rebuild: resets the tiles' Button/tooltip press state even after a rejected drop.
            if (IsVisible)
            {
                RefreshAll();
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

            if (wasDragging && IsVisible)
            {
                RefreshAll();
            }
        }

        private void BeginDrag()
        {
            _isDragging = true;
            ReleaseHeld();
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
            // A long-press that fired before the finger moved could have shown a tooltip.
            if (_tooltip != null && _tooltip.IsVisible)
            {
                _tooltip.Hide();
            }

            if (_dragGhost != null && _root != null)
            {
                Vector2 local = _root.WorldToLocal(pointerPosition);
                // Offset up-left of the finger so the item stays visible while dragging on touch.
                _dragGhost.style.left = local.x - 24f;
                _dragGhost.style.top = local.y - 56f;
            }

            SetHoveredTarget(FindDropTarget(pointerPosition));
            UpdateGridGhostForDrag(pointerPosition);
        }

        private void UpdateGridGhostForDrag(Vector2 pointerPosition)
        {
            if (_inventory == null || !IsOverGrid(pointerPosition) || !TryGetPointerCell(pointerPosition, out int x, out int y))
            {
                HideGhost();
                return;
            }

            Vector2Int anchor = InventoryDragDrop.AnchorFor(x, y, _grab.x, _grab.y);
            if (_dragSource.Location == InventoryDragLocation.BackpackSlot)
            {
                ShowGhost(_dragSource.SlotIndex, anchor.x, anchor.y);
                return;
            }

            // An equipped item: preview its footprint on a free area.
            if (_inventory.Equipped.TryGetValue(_dragSource.EquipSlot, out InventorySlotData equipped) && equipped != null)
            {
                GridSize size = _inventory.GetFootprint(ItemDatabase.GetById(equipped.itemId));
                var rect = new GridRect(anchor.x, anchor.y, size);
                ShowGhostRect(anchor.x, anchor.y, size.Width, size.Height, _inventory.BuildGrid().IsFree(rect));
            }
        }

        private void SetHoveredTarget(DropTarget target)
        {
            if (target == _hoveredTarget)
            {
                return;
            }

            ClearHoverClasses();
            _hoveredTarget = target;

            if (target == null || target.Element == _dragSourceElement)
            {
                return;
            }

            bool valid = InventoryDragDrop.ResolveDrop(_dragSource, target.Endpoint, _dragSourceEquipSlot) != InventoryDropAction.None;
            target.Element.AddToClassList(valid ? DropValidClass : DropInvalidClass);
        }

        private void ClearHoverClasses()
        {
            if (_hoveredTarget?.Element != null)
            {
                _hoveredTarget.Element.RemoveFromClassList(DropValidClass);
                _hoveredTarget.Element.RemoveFromClassList(DropInvalidClass);
            }

            _hoveredTarget = null;
        }

        private DropTarget FindDropTarget(Vector2 pointerPosition)
        {
            _dropTargetRects.Clear();
            _dropClipRects.Clear();

            foreach (DropTarget target in _dropTargets)
            {
                _dropTargetRects.Add(target.Element.worldBound);
                _dropClipRects.Add(target.Scroll != null ? target.Scroll.contentViewport.worldBound : target.Element.worldBound);
            }

            int index = InventoryDragDrop.FindTargetIndex(pointerPosition, _dropTargetRects, _dropClipRects);
            return index >= 0 ? _dropTargets[index] : null;
        }

        private void EndDrag()
        {
            ClearHoverClasses();
            _dragSourceElement?.RemoveFromClassList(DragSourceClass);

            if (_dragGhost != null)
            {
                _dragGhost.style.display = DisplayStyle.None;
            }

            if (_isDragging)
            {
                HideGhost();
            }

            _isDragging = false;
            _pressPointerId = -1;
            _dragSourceElement = null;
            _dragSource = InventoryDragEndpoint.None;
            _dragSourceEquipSlot = EquipmentSlot.None;
        }

        private void PerformDrop(InventoryDragEndpoint source, InventoryDragEndpoint target, EquipmentSlot sourceEquipSlot)
        {
            if (_inventory == null)
            {
                return;
            }

            // Equipment rows are the only list targets now; grid drops are handled by cell (OnRootPointerUp).
            if (InventoryDragDrop.ResolveDrop(source, target, sourceEquipSlot) == InventoryDropAction.Equip)
            {
                _inventory.EquipFromSlot(source.SlotIndex);
            }
        }

        /// <summary>
        /// UnequipSlot returns the item to the first free area; then move it onto the cell it was dropped on
        /// (<see cref="InventoryManager.MoveItem"/>: if that spot doesn't fit, it stays where it landed).
        /// </summary>
        private void UnequipInto(EquipmentSlot equipSlot, int x, int y)
        {
            if (_inventory == null || !_inventory.Equipped.TryGetValue(equipSlot, out InventorySlotData equipped) || equipped == null)
            {
                return;
            }

            string itemId = equipped.itemId;
            IReadOnlyList<InventorySlotData> slots = _inventory.Slots;
            var wasEmpty = new bool[slots.Count];

            for (int i = 0; i < slots.Count; i++)
            {
                wasEmpty[i] = slots[i].IsEmpty;
            }

            if (!_inventory.UnequipSlot(equipSlot))
            {
                return;
            }

            slots = _inventory.Slots;

            for (int i = 0; i < slots.Count && i < wasEmpty.Length; i++)
            {
                if (wasEmpty[i] && !slots[i].IsEmpty && slots[i].itemId == itemId)
                {
                    _inventory.MoveItem(i, x, y);
                    return;
                }
            }
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static void AddRarityClass(VisualElement element, ItemRarity rarity)
        {
            element.AddToClassList("inventory-row--" + TooltipLocalization.ToSnakeCase(rarity.ToString()));
        }

        private static string Localize(Func<string, string> localize, string key)
        {
            string resolved = localize?.Invoke(key);
            if (!string.IsNullOrEmpty(resolved) && !(resolved.StartsWith("[", StringComparison.Ordinal) && resolved.EndsWith("]", StringComparison.Ordinal)))
            {
                return resolved;
            }

            return TooltipLocalization.GetFallback(key) ?? key;
        }

        private static void SetText(Label label, string text)
        {
            if (label != null)
            {
                label.text = text;
            }
        }

        private void ClearRows()
        {
            _dropTargets.Clear();
            ClearRows(_equipmentListContainer, _spawnedEquipmentRows);
            ClearRows(_itemListContainer, _spawnedItemRows);
            _cells.Clear();
            _bagGrid = null;
            _cellGhost = null;
        }

        private static void ClearRows(VisualElement container, List<VisualElement> spawned)
        {
            if (container != null)
            {
                foreach (VisualElement element in spawned)
                {
                    container.Remove(element);
                }
            }

            spawned.Clear();
        }
    }
}
