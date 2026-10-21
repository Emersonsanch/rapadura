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
    /// <see cref="InventoryManager.Equipped"/> and forwards taps back into
    /// <see cref="InventoryManager.EquipFromSlot"/>/<see cref="InventoryManager.UseItemInSlot"/>/
    /// <see cref="InventoryManager.UnequipSlot"/>; refreshes on <see cref="InventoryChangedEvent"/>.
    ///
    /// Every item and equipment row gets a tooltip (name, rarity, stats/affixes, description) via
    /// <see cref="TooltipTrigger"/> — hover with a mouse, press-and-hold on touch, focus on gamepad.
    /// Opened from the HUD's "Mochila" button (<c>HudController</c>) via <see cref="Toggle"/>.
    ///
    /// The backpack is a grid with one tile per slot (empty ones included, so they can receive a
    /// drop). Drag-and-drop (pointer events, works for touch and mouse): pressing an item and moving
    /// past <see cref="InventoryDragDrop.DragThresholdPixels"/> picks it up — holding still instead
    /// keeps showing the long-press tooltip, a quick tap still uses/equips. Dropping on another slot
    /// moves/merges/swaps, on its equipment slot equips, and an equipped item dropped on the backpack
    /// unequips into that slot (rules in <see cref="InventoryDragDrop.ResolveDrop"/>). "Organizar"
    /// runs <see cref="InventoryManager.SortInventory"/>. Gamepad/keyboard: <see cref="FocusNavigator"/>.
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
        };

        private const string DragSourceClass = "inventory-slot--drag-source";
        private const string DropValidClass = "inventory-drop--valid";
        private const string DropInvalidClass = "inventory-drop--invalid";

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

        private TooltipController _tooltip;
        private FocusNavigator _focusNavigator;
        private readonly List<VisualElement> _spawnedEquipmentRows = new List<VisualElement>();
        private readonly List<VisualElement> _spawnedItemRows = new List<VisualElement>();

        // Drag & drop state.
        private sealed class DropTarget
        {
            public VisualElement Element;
            public InventoryDragEndpoint Endpoint;
            public ScrollView Scroll;
        }

        private readonly List<DropTarget> _dropTargets = new List<DropTarget>();
        private readonly List<Rect> _dropTargetRects = new List<Rect>();
        private readonly List<Rect> _dropClipRects = new List<Rect>();
        private int _pressPointerId = -1;
        private Vector2 _pressPosition;
        private InventoryDragEndpoint _dragSource;
        private EquipmentSlot _dragSourceEquipSlot;
        private VisualElement _dragSourceElement;
        private string _dragLabel;
        private bool _isDragging;
        private DropTarget _hoveredTarget;

        public bool IsVisible => _root != null && _root.style.display == DisplayStyle.Flex;

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
                // so it doesn't hide the slot under the finger from hit testing.
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
                // TrickleDown on the full-screen root: runs before the ScrollViews/slots below, so an
                // active drag can keep the moves to itself (no list scrolling, no button clicking).
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
                _root.UnregisterCallback<PointerMoveEvent>(OnRootPointerMove, TrickleDown.TrickleDown);
                _root.UnregisterCallback<PointerUpEvent>(OnRootPointerUp, TrickleDown.TrickleDown);
                _root.UnregisterCallback<PointerCancelEvent>(OnRootPointerCancel, TrickleDown.TrickleDown);
            }

            EndDrag();
            ClearRows();
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            // Mid-drag the rows stay as they are (rebuilding would destroy the dragged tile); the drop
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

            string weightPattern = Localize(localize, "ui.inventory.weight");
            SetText(_weightLabel, string.Format(CultureInfo.InvariantCulture, weightPattern,
                _inventory.GetTotalWeight().ToString("0.#", CultureInfo.InvariantCulture),
                _inventory.MaxWeight.ToString("0.#", CultureInfo.InvariantCulture)));

            _dropTargets.Clear();
            RebuildEquipmentRows(localize);
            RebuildItemRows(localize);
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
                var row = new Button(() => _inventory?.UnequipSlot(capturedSlot));
                row.AddToClassList("inventory-row-button");
                row.AddToClassList("inventory-equipment-row");

                if (item != null)
                {
                    string itemName = GeneratedItemName.Compose(item, equippedData, ContentText.CurrentManager);
                    row.text = $"{slotName}: {itemName}";
                    AddRarityClass(row, TooltipContentBuilder.ResolveRarity(item, equippedData));

                    InventorySlotData tooltipData = equippedData;
                    TooltipTrigger.Bind(row, _tooltip, () => TooltipContentBuilder.ForItem(item, tooltipData, TooltipController.GetLocalizer()));
                    RegisterDragSource(row, InventoryDragEndpoint.Equipment(equipSlot), equipSlot, itemName);
                }
                else
                {
                    // Disabled (nothing to unequip) but still a drop target for dragged gear.
                    row.text = $"{slotName}: {Localize(localize, "ui.tooltip.empty_slot")}";
                    row.SetEnabled(false);
                }

                _dropTargets.Add(new DropTarget { Element = row, Endpoint = InventoryDragEndpoint.Equipment(equipSlot), Scroll = scroll });
                _equipmentListContainer.Add(row);
                _spawnedEquipmentRows.Add(row);
            }
        }

        private void RebuildItemRows(Func<string, string> localize)
        {
            ClearRows(_itemListContainer, _spawnedItemRows);

            if (_itemListContainer == null)
            {
                return;
            }

            ScrollView scroll = _itemListContainer.GetFirstAncestorOfType<ScrollView>();
            IReadOnlyList<InventorySlotData> slots = _inventory.Slots;

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];
                ItemDefinition item = slot.IsEmpty ? null : ItemDatabase.GetById(slot.itemId);
                VisualElement tile;

                if (item == null)
                {
                    // Empty (or unknown-id) slot: not focusable — gamepad navigation only stops on items.
                    tile = new VisualElement();
                    tile.AddToClassList("inventory-slot");
                    tile.AddToClassList("inventory-slot--empty");
                }
                else
                {
                    int slotIndex = i;
                    string itemName = GeneratedItemName.Compose(item, slot, ContentText.CurrentManager);

                    var button = new Button(() => OnItemClicked(slotIndex)) { text = itemName };
                    button.AddToClassList("inventory-slot");
                    AddRarityClass(button, TooltipContentBuilder.ResolveRarity(item, slot));

                    if (slot.quantity > 1)
                    {
                        var quantity = new Label($"x{slot.quantity}") { pickingMode = PickingMode.Ignore };
                        quantity.AddToClassList("inventory-slot-quantity");
                        button.Add(quantity);
                    }

                    InventorySlotData tooltipSlot = slot;
                    TooltipTrigger.Bind(button, _tooltip, () => TooltipContentBuilder.ForItem(item, tooltipSlot, TooltipController.GetLocalizer()));
                    RegisterDragSource(button, InventoryDragEndpoint.Backpack(i), item.EquipSlot, slot.quantity > 1 ? $"{itemName} x{slot.quantity}" : itemName);
                    tile = button;
                }

                _dropTargets.Add(new DropTarget { Element = tile, Endpoint = InventoryDragEndpoint.Backpack(i), Scroll = scroll });
                _itemListContainer.Add(tile);
                _spawnedItemRows.Add(tile);
            }
        }

        private void OnItemClicked(int slotIndex)
        {
            if (_inventory == null || slotIndex < 0 || slotIndex >= _inventory.Slots.Count)
            {
                return;
            }

            ItemDefinition item = ItemDatabase.GetById(_inventory.Slots[slotIndex].itemId);

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

        // ---------------------------------------------------------------
        // Drag & drop
        // ---------------------------------------------------------------

        private void RegisterDragSource(VisualElement element, InventoryDragEndpoint endpoint, EquipmentSlot itemEquipSlot, string label)
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

                // Not stopped: this same move must still reach the slot so TooltipTrigger cancels its
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
            if (evt.pointerId != _pressPointerId)
            {
                return;
            }

            if (!_isDragging)
            {
                // A tap or a long-press: leave it to the Button / TooltipTrigger.
                _pressPointerId = -1;
                _dragSourceElement = null;
                return;
            }

            // Swallow the release: no click on the slot under the finger, no list fling.
            evt.StopPropagation();
            _root.panel?.ReleasePointer(evt.pointerId);

            DropTarget target = FindDropTarget(evt.position);
            InventoryDragEndpoint source = _dragSource;
            EquipmentSlot sourceEquipSlot = _dragSourceEquipSlot;
            EndDrag();

            if (target != null)
            {
                PerformDrop(source, target.Endpoint, sourceEquipSlot);
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

            switch (InventoryDragDrop.ResolveDrop(source, target, sourceEquipSlot))
            {
                case InventoryDropAction.MoveOrMerge:
                    _inventory.MoveOrMergeSlot(source.SlotIndex, target.SlotIndex);
                    break;
                case InventoryDropAction.Equip:
                    _inventory.EquipFromSlot(source.SlotIndex);
                    break;
                case InventoryDropAction.Unequip:
                    UnequipInto(source.EquipSlot, target.SlotIndex);
                    break;
            }
        }

        /// <summary>
        /// UnequipSlot returns the item to the first free slot; then move it onto the slot it was
        /// dropped on (MoveOrMergeSlot swaps with whatever is there and keeps generated data intact).
        /// </summary>
        private void UnequipInto(EquipmentSlot equipSlot, int targetIndex)
        {
            if (!_inventory.Equipped.TryGetValue(equipSlot, out InventorySlotData equipped) || equipped == null)
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
                    if (i != targetIndex)
                    {
                        _inventory.MoveOrMergeSlot(i, targetIndex);
                    }

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
