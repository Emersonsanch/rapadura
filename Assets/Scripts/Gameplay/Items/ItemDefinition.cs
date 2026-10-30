using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Items
{
    /// <summary>
    /// Data-driven definition of a single item. Every property a designer needs to tune —
    /// identity, economy, stacking, equipment stats, consumable effects — lives here as a
    /// ScriptableObject asset, so new items can be created and balanced entirely from the
    /// Unity Editor (or the custom Item Editor window) without touching code. InventoryManager
    /// reads these assets; it never hardcodes per-item behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Rapadura/Items/Item Definition", order = 0)]
    public class ItemDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _itemId = "item_new";
        [SerializeField] private string _displayName = "New Item";
        [TextArea(2, 5)]
        [SerializeField] private string _description = string.Empty;
        [SerializeField] private Sprite _icon;
        [SerializeField] private GameObject _worldModelPrefab;

        [Header("Classification")]
        [SerializeField] private ItemType _type = ItemType.Material;
        [SerializeField] private ItemCategory _category = ItemCategory.Miscellaneous;
        [SerializeField] private ItemRarity _rarity = ItemRarity.Common;

        [Header("Economy & Stacking")]
        [Tooltip("Weight of ONE unit (stacks weigh per unit). Drives the carry load (EncumbranceRules): a heavy bag slows the hero and the mount. 0 = suggested from category x grid cells (ItemWeightRules); currency/gold always weigh 0.")]
        [SerializeField] private float _weight = 1f;
        [SerializeField] private int _value = 1;
        [SerializeField] private int _maxStack = 1;

        [Header("Mochila (grade estilo MU)")]
        [Tooltip("Cells wide on the backpack grid. 0 = the category default (ItemFootprint.ForCategory).")]
        [SerializeField, Min(0)] private int _gridWidth = 0;
        [Tooltip("Cells tall on the backpack grid. 0 = the category default (ItemFootprint.ForCategory).")]
        [SerializeField, Min(0)] private int _gridHeight = 0;
        [Tooltip("Bound to the hero: never falls out of the backpack on death (DeathDropRules).")]
        [SerializeField] private bool _isBound = false;

        [Header("Durability")]
        [SerializeField] private bool _hasDurability = false;
        [SerializeField] private float _maxDurability = 100f;

        [Header("Equipment")]
        [SerializeField] private EquipmentSlot _equipSlot = EquipmentSlot.None;
        [SerializeField] private float _armorDefense = 0f;
        [SerializeField] private float _weaponDamage = 0f;
        [SerializeField] private float _weaponAttackSpeed = 1f;
        [SerializeField] private StatModifierDefinition[] _equipStatModifiers = System.Array.Empty<StatModifierDefinition>();

        [Header("Consumable Effects")]
        [SerializeField] private float _healthRestore = 0f;
        [SerializeField] private float _staminaRestore = 0f;
        [SerializeField] private float _manaRestore = 0f;
        [SerializeField] private StatModifierDefinition[] _consumeBuffs = System.Array.Empty<StatModifierDefinition>();

        public string ItemId => _itemId;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public GameObject WorldModelPrefab => _worldModelPrefab;

        public ItemType Type => _type;
        public ItemCategory Category => _category;
        public ItemRarity Rarity => _rarity;

        /// <summary>Authored unit weight (0 = use the suggestion). Prefer <see cref="CarryWeight"/>.</summary>
        public float Weight => _weight;

        /// <summary>Effective unit weight used by the carry load (<see cref="ItemWeightRules.UnitWeight(ItemDefinition)"/>).</summary>
        public float CarryWeight => ItemWeightRules.UnitWeight(this);
        public int Value => _value;
        public int MaxStack => Mathf.Max(1, _maxStack);
        public bool IsStackable => MaxStack > 1;

        /// <summary>Per-item grid width override (0 = category default). See <c>ItemFootprint</c>.</summary>
        public int GridWidthOverride => Mathf.Max(0, _gridWidth);
        /// <summary>Per-item grid height override (0 = category default). See <c>ItemFootprint</c>.</summary>
        public int GridHeightOverride => Mathf.Max(0, _gridHeight);
        /// <summary>Bound items never drop on death (<c>DeathDropRules</c>).</summary>
        public bool IsBound => _isBound;

        public bool HasDurability => _hasDurability;
        public float MaxDurability => _maxDurability;

        public bool IsEquippable => _equipSlot != EquipmentSlot.None;
        public EquipmentSlot EquipSlot => _equipSlot;
        public float ArmorDefense => _armorDefense;
        public float WeaponDamage => _weaponDamage;
        public float WeaponAttackSpeed => _weaponAttackSpeed;
        public StatModifierDefinition[] EquipStatModifiers => _equipStatModifiers;

        public bool IsConsumable => _type == ItemType.Consumable;
        public float HealthRestore => _healthRestore;
        public float StaminaRestore => _staminaRestore;
        public float ManaRestore => _manaRestore;
        public StatModifierDefinition[] ConsumeBuffs => _consumeBuffs;
    }
}
