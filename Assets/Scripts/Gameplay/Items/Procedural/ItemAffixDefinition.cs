using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Items.Procedural
{
    /// <summary>Where in the generated name an affix's text is inserted.</summary>
    public enum AffixSlotKind
    {
        /// <summary>Rendered before the base item name (e.g. "Flamejante Espada").</summary>
        Prefix,

        /// <summary>Rendered after the base item name (e.g. "Espada da Fúria").</summary>
        Suffix
    }

    /// <summary>
    /// Data-driven definition of a single procedural affix, following the same
    /// ScriptableObject-per-asset pattern as <see cref="ItemDefinition"/> and
    /// <see cref="Rapadura.Gameplay.Skills.StatModifierDefinition"/>: designers author affixes as
    /// assets under <c>Resources/Affixes</c>, <see cref="AffixDatabase"/> loads them, and
    /// <see cref="ProceduralItemGenerator"/> rolls from that pool at generation time.
    ///
    /// Follows the well-known ARPG (Diablo/Path of Exile-style) affix model: each affix affects a
    /// single <see cref="StatType"/> within a designer-tuned min/max range, carries a rarity
    /// weight (rarer/stronger affixes should use a lower weight so they're drawn less often), and
    /// can optionally restrict itself to a specific <see cref="EquipmentSlot"/> (e.g. a "% attack
    /// speed" affix should only ever roll on weapons).
    /// </summary>
    [CreateAssetMenu(fileName = "NewAffix", menuName = "Rapadura/Items/Item Affix Definition", order = 1)]
    public class ItemAffixDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _affixId = "affix_new";

        [Tooltip("Localization key resolved via LocalizationManager.Get(key). Should format to a short " +
                 "adjective/phrase, e.g. \"Flamejante\" (prefix) or \"da Fúria\" (suffix).")]
        [SerializeField] private string _nameLocalizationKey = string.Empty;

        [Tooltip("Plain-text fallback used when no localizer is supplied (tests, tools, or missing key).")]
        [SerializeField] private string _fallbackDisplayText = "Misterioso";

        [SerializeField] private AffixSlotKind _slotKind = AffixSlotKind.Prefix;

        [Header("Effect")]
        [SerializeField] private StatType _affectedStat = StatType.AttackDamage;
        [SerializeField] private ModifierApplication _application = ModifierApplication.Flat;
        [SerializeField] private float _minValue = 1f;
        [SerializeField] private float _maxValue = 5f;

        [Header("Roll Weighting")]
        [Tooltip("Relative weight used when randomly drawing from the affix pool. Higher = more common. " +
                 "Stronger/rarer affixes (e.g. large value ranges) should use a lower weight.")]
        [SerializeField] private int _rarityWeight = 100;

        [Header("Compatibility")]
        [Tooltip("If not None, this affix can only roll on items equipping this slot. Leave as None to allow any item.")]
        [SerializeField] private EquipmentSlot _requiredEquipSlot = EquipmentSlot.None;

        public string AffixId => _affixId;
        public string NameLocalizationKey => _nameLocalizationKey;
        public string FallbackDisplayText => _fallbackDisplayText;
        public AffixSlotKind SlotKind => _slotKind;

        public StatType AffectedStat => _affectedStat;
        public ModifierApplication Application => _application;
        public float MinValue => Mathf.Min(_minValue, _maxValue);
        public float MaxValue => Mathf.Max(_minValue, _maxValue);

        public int RarityWeight => Mathf.Max(1, _rarityWeight);

        public EquipmentSlot RequiredEquipSlot => _requiredEquipSlot;

        /// <summary>True if this affix has no slot restriction, or matches the given item's equip slot.</summary>
        public bool IsCompatibleWith(EquipmentSlot itemEquipSlot)
        {
            return _requiredEquipSlot == EquipmentSlot.None || _requiredEquipSlot == itemEquipSlot;
        }
    }
}
