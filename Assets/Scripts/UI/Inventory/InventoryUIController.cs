using System;
using System.Collections.Generic;
using System.Globalization;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
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

        private TooltipController _tooltip;
        private readonly List<VisualElement> _spawnedEquipmentRows = new List<VisualElement>();
        private readonly List<VisualElement> _spawnedItemRows = new List<VisualElement>();

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
            Hide();
        }

        private void OnDisable()
        {
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
        }

        private void RegisterCallbacks()
        {
            EventBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);

            if (_closeButton != null)
            {
                _closeButton.clicked += Hide;
            }
        }

        private void UnregisterCallbacks()
        {
            EventBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);

            if (_closeButton != null)
            {
                _closeButton.clicked -= Hide;
            }

            ClearRows();
        }

        private void OnInventoryChanged(InventoryChangedEvent evt)
        {
            if (IsVisible)
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
        }

        public void Hide()
        {
            _tooltip?.Hide();

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

        private void RefreshAll()
        {
            _tooltip?.Hide();
            Func<string, string> localize = TooltipController.GetLocalizer();

            SetText(_titleLabel, Localize(localize, "ui.inventory.title"));
            SetText(_equipmentTitleLabel, Localize(localize, "ui.inventory.equipment"));
            SetText(_itemsTitleLabel, Localize(localize, "ui.inventory.items"));
            SetText(_hintLabel, Localize(localize, "ui.inventory.hint"));

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

            RebuildEquipmentRows(localize);
            RebuildItemRows(localize);
        }

        private void RebuildEquipmentRows(Func<string, string> localize)
        {
            ClearRows(_equipmentListContainer, _spawnedEquipmentRows);

            if (_equipmentListContainer == null)
            {
                return;
            }

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
                    string itemName = equippedData.HasGeneratedAffixes && !string.IsNullOrEmpty(equippedData.generatedName)
                        ? equippedData.generatedName
                        : item.DisplayName;
                    row.text = $"{slotName}: {itemName}";
                    AddRarityClass(row, TooltipContentBuilder.ResolveRarity(item, equippedData));

                    InventorySlotData tooltipData = equippedData;
                    TooltipTrigger.Bind(row, _tooltip, () => TooltipContentBuilder.ForItem(item, tooltipData, TooltipController.GetLocalizer()));
                }
                else
                {
                    row.text = $"{slotName}: {Localize(localize, "ui.tooltip.empty_slot")}";
                    row.SetEnabled(false);
                }

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

            IReadOnlyList<InventorySlotData> slots = _inventory.Slots;

            for (int i = 0; i < slots.Count; i++)
            {
                InventorySlotData slot = slots[i];

                if (slot.IsEmpty)
                {
                    continue;
                }

                ItemDefinition item = ItemDatabase.GetById(slot.itemId);

                if (item == null)
                {
                    continue;
                }

                int slotIndex = i;
                string itemName = slot.HasGeneratedAffixes && !string.IsNullOrEmpty(slot.generatedName) ? slot.generatedName : item.DisplayName;
                string label = slot.quantity > 1 ? $"{itemName} x{slot.quantity}" : itemName;

                var row = new Button(() => OnItemClicked(slotIndex)) { text = label };
                row.AddToClassList("inventory-row-button");
                AddRarityClass(row, TooltipContentBuilder.ResolveRarity(item, slot));

                InventorySlotData tooltipSlot = slot;
                TooltipTrigger.Bind(row, _tooltip, () => TooltipContentBuilder.ForItem(item, tooltipSlot, TooltipController.GetLocalizer()));

                _itemListContainer.Add(row);
                _spawnedItemRows.Add(row);
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

            return TooltipLocalization.GetEnglishFallback(key) ?? key;
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
