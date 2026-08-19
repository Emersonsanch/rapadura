using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// The five primary attributes shared by every playable character (see the "Atributo Principal"
    /// of each character's lore in <c>TODO.md</c> and <c>Assets/Scripts/Gameplay/Characters</c>).
    /// </summary>
    public enum AttributeType
    {
        /// <summary>Joaquim's primary attribute. Raises max health.</summary>
        Vitality,
        /// <summary>Maria's primary attribute. Raises max mana.</summary>
        Spirit,
        /// <summary>Maithe's primary attribute. Raises critical hit chance.</summary>
        Dexterity,
        /// <summary>Ícaro's primary attribute. Raises cooldown reduction.</summary>
        Intelligence,
        /// <summary>Lavine's primary attribute. Raises attack damage.</summary>
        ArcanePower
    }

    /// <summary>Serializable snapshot of <see cref="AttributeSet"/> used by the save system.</summary>
    [Serializable]
    public class AttributeSetSaveState
    {
        public int vitality;
        public int spirit;
        public int dexterity;
        public int intelligence;
        public int arcanePower;
        public int availableAttributePoints;
        public int lastKnownPlayerLevel = 1;
    }

    /// <summary>
    /// Holds the character's five primary attributes (Vitalidade/Espírito/Destreza/Inteligência/
    /// Poder Arcano — see the GDD's "Progressão significativa" pillar) and converts them into
    /// derived stat bonuses applied through the existing <see cref="BuffController"/>, the same
    /// pipeline equipment bonuses use (<c>InventoryManager.EquipSlot</c>). This reuses the classic
    /// "primary attribute + flat/percent derived stat" pattern common to action-RPGs (e.g.
    /// Diablo-style Strength/Dexterity/Vitality/Energy or Path of Exile's Str/Dex/Int) rather than
    /// inventing a new stat model from scratch.
    ///
    /// Points are earned automatically on player level-up (reusing <see cref="PlayerStats"/>'s
    /// existing XP/Level system via <see cref="Core.EventBus.PlayerExperienceChangedEvent"/>,
    /// mirroring how <see cref="SkillManager"/> grants skill points on level-up) and spent by the
    /// player via <see cref="TrySpendPoint"/>. Each character's <see cref="Characters.PlayableCharacter.ApplyPassive"/>
    /// additionally grants a small flat bonus to its own primary attribute via
    /// <see cref="ApplyCharacterPassiveBonus"/>, representing the character's innate talent.
    ///
    /// Derived formulas (documented here as the single source of truth):
    /// <list type="bullet">
    /// <item>Vitality → +<see cref="VitalityHealthPerPoint"/> flat Max Health per point.</item>
    /// <item>Spirit → +<see cref="SpiritManaPerPoint"/> flat Max Mana per point.</item>
    /// <item>Dexterity → +<see cref="DexterityCriticalChancePerPoint"/> Critical Chance per point (percent-additive).</item>
    /// <item>Intelligence → +<see cref="IntelligenceCooldownReductionPerPoint"/> Cooldown Reduction per point (percent-additive).</item>
    /// <item>Poder Arcano (ArcanePower) → +<see cref="ArcanePowerAttackDamagePerPoint"/> Attack Damage per point (percent-additive).</item>
    /// </list>
    /// Each attribute owns exactly one <see cref="StatType"/> so recomputing its bonus never clobbers
    /// another attribute's bonus inside the shared <see cref="BuffController"/> (see
    /// <see cref="BuffController.ApplyEffect"/>, which replaces any existing effect for the same stat).
    /// </summary>
    public class AttributeSet : MonoBehaviour, ISaveable
    {
        /// <summary>Flat Max Health granted per point invested in Vitality.</summary>
        public const float VitalityHealthPerPoint = 10f;

        /// <summary>Flat Max Mana granted per point invested in Spirit.</summary>
        public const float SpiritManaPerPoint = 8f;

        /// <summary>Percent-additive Critical Chance granted per point invested in Dexterity (0.005 = 0.5%).</summary>
        public const float DexterityCriticalChancePerPoint = 0.005f;

        /// <summary>Percent-additive Cooldown Reduction granted per point invested in Intelligence (0.004 = 0.4%).</summary>
        public const float IntelligenceCooldownReductionPerPoint = 0.004f;

        /// <summary>Percent-additive Attack Damage granted per point invested in Poder Arcano (0.01 = 1%).</summary>
        public const float ArcanePowerAttackDamagePerPoint = 0.01f;

        /// <summary>Source key tagging every stat modifier this component applies to the shared BuffController.</summary>
        public const string BuffSourceKey = "attributes";

        /// <summary>Flat bonus a character's innate talent grants to its own primary attribute (see <see cref="ApplyCharacterPassiveBonus"/>).</summary>
        public const int CharacterPassiveBonusPoints = 5;

        [Header("Progression")]
        [SerializeField] private int _attributePointsPerLevel = 2;

        [Header("Optional References")]
        [SerializeField] private BuffController _buffController;

        private readonly Dictionary<AttributeType, int> _allocatedPoints = new Dictionary<AttributeType, int>();
        private readonly Dictionary<AttributeType, int> _passiveBonus = new Dictionary<AttributeType, int>();
        private int _lastKnownPlayerLevel = 1;

        public string SaveKey => "AttributeSet";

        public int AvailableAttributePoints { get; private set; }

        /// <summary>Raised whenever allocated points, passive bonuses or available points change.</summary>
        public event Action OnAttributesChanged;

        private bool _isInitialized;

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Idempotently seeds <see cref="_allocatedPoints"/>/<see cref="_passiveBonus"/> with all five
        /// attributes at zero and caches <see cref="_buffController"/>. Called from <see cref="Awake"/>
        /// for the normal Play Mode/Scene path, but also defensively at the top of every public method —
        /// EditMode tests construct this component via <c>gameObject.AddComponent&lt;AttributeSet&gt;()</c>
        /// and call into it the same frame, and Unity does not guarantee <see cref="Awake"/> has already
        /// run by then in that context, so relying on Awake alone left the dictionaries empty and every
        /// indexer access threw <see cref="KeyNotFoundException"/>.
        /// </summary>
        private void EnsureInitialized()
        {
            if (_isInitialized)
            {
                return;
            }

            foreach (AttributeType type in AllTypes)
            {
                _allocatedPoints[type] = 0;
                _passiveBonus[type] = 0;
            }

            if (_buffController == null)
            {
                _buffController = GetComponent<BuffController>();
            }

            _isInitialized = true;
        }

        private void Start()
        {
            if (ServiceLocator.TryGet(out Rapadura.Save.SaveManager saveManager))
            {
                saveManager.Register(this);
            }

            EventBus.Subscribe<PlayerExperienceChangedEvent>(HandlePlayerExperienceChanged);
        }

        private void OnDestroy()
        {
            if (ServiceLocator.TryGet(out Rapadura.Save.SaveManager saveManager))
            {
                saveManager.Unregister(this);
            }

            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(HandlePlayerExperienceChanged);
        }

        private static IEnumerable<AttributeType> AllTypes
        {
            get
            {
                yield return AttributeType.Vitality;
                yield return AttributeType.Spirit;
                yield return AttributeType.Dexterity;
                yield return AttributeType.Intelligence;
                yield return AttributeType.ArcanePower;
            }
        }

        private void HandlePlayerExperienceChanged(PlayerExperienceChangedEvent evt)
        {
            if (evt.Level <= _lastKnownPlayerLevel)
            {
                return;
            }

            int levelsGained = evt.Level - _lastKnownPlayerLevel;
            _lastKnownPlayerLevel = evt.Level;
            GrantAttributePoints(levelsGained * _attributePointsPerLevel);
        }

        /// <summary>Grants unspent attribute points, e.g. on level-up.</summary>
        public void GrantAttributePoints(int amount)
        {
            EnsureInitialized();

            if (amount <= 0)
            {
                return;
            }

            AvailableAttributePoints += amount;
            OnAttributesChanged?.Invoke();
        }

        /// <summary>Number of points the player has allocated into the given attribute (excludes character passive bonuses).</summary>
        public int GetAllocatedPoints(AttributeType type)
        {
            EnsureInitialized();
            return _allocatedPoints[type];
        }

        /// <summary>Flat bonus points granted by the character's innate passive talent (see <see cref="ApplyCharacterPassiveBonus"/>).</summary>
        public int GetPassiveBonus(AttributeType type)
        {
            EnsureInitialized();
            return _passiveBonus[type];
        }

        /// <summary>Total value of the attribute: allocated points plus the character's passive bonus.</summary>
        public int GetTotal(AttributeType type)
        {
            EnsureInitialized();
            return _allocatedPoints[type] + _passiveBonus[type];
        }

        /// <summary>
        /// Spends unspent attribute points into the given attribute and immediately recomputes its
        /// derived stat bonus. Returns false if there aren't enough available points.
        /// </summary>
        public bool TrySpendPoint(AttributeType type, int amount = 1)
        {
            EnsureInitialized();

            if (amount <= 0 || AvailableAttributePoints < amount)
            {
                return false;
            }

            AvailableAttributePoints -= amount;
            _allocatedPoints[type] += amount;
            ApplyDerivedEffect(type);
            OnAttributesChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Applies (replacing any previous value) the flat bonus a character's innate talent grants to
        /// its own primary attribute. Intended to be called once from
        /// <see cref="Characters.PlayableCharacter.ApplyPassive"/> right after the character spawns.
        /// </summary>
        public void ApplyCharacterPassiveBonus(AttributeType type, int bonus = CharacterPassiveBonusPoints)
        {
            EnsureInitialized();
            _passiveBonus[type] = Mathf.Max(0, bonus);
            ApplyDerivedEffect(type);
            OnAttributesChanged?.Invoke();
        }

        /// <summary>Recomputes and (re)applies every attribute's derived stat bonus to the BuffController.</summary>
        private void ApplyAllDerivedEffects()
        {
            foreach (AttributeType type in AllTypes)
            {
                ApplyDerivedEffect(type);
            }
        }

        private void ApplyDerivedEffect(AttributeType type)
        {
            if (_buffController == null)
            {
                return;
            }

            StatModifierDefinition definition = BuildDerivedEffect(type, GetTotal(type));
            _buffController.ApplyEffect(definition, BuffSourceKey);
        }

        /// <summary>Builds the permanent stat modifier a given total of an attribute produces (see class docs for formulas).</summary>
        private static StatModifierDefinition BuildDerivedEffect(AttributeType type, int totalPoints)
        {
            (StatType stat, ModifierApplication application, float perPoint, string label) = type switch
            {
                AttributeType.Vitality => (StatType.MaxHealth, ModifierApplication.Flat, VitalityHealthPerPoint, "Vitalidade"),
                AttributeType.Spirit => (StatType.MaxMana, ModifierApplication.Flat, SpiritManaPerPoint, "Espírito"),
                AttributeType.Dexterity => (StatType.CriticalChance, ModifierApplication.PercentAdditive, DexterityCriticalChancePerPoint, "Destreza"),
                AttributeType.Intelligence => (StatType.CooldownReduction, ModifierApplication.PercentAdditive, IntelligenceCooldownReductionPerPoint, "Inteligência"),
                AttributeType.ArcanePower => (StatType.AttackDamage, ModifierApplication.PercentAdditive, ArcanePowerAttackDamagePerPoint, "Poder Arcano"),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

            return new StatModifierDefinition
            {
                displayName = $"Atributo: {label}",
                affectedStat = stat,
                application = application,
                value = perPoint * totalPoints,
                // Permanent bonus: only removed by explicitly clearing/reapplying via this component,
                // mirroring InventoryManager's equip-bonus convention (see WithPermanentDuration).
                durationSeconds = float.MaxValue / 2f,
                isDebuff = false,
                ticksOverTime = false
            };
        }

        /// <summary>Maps a character's lore-facing <c>PrimaryAttribute</c> label (pt-BR) to its <see cref="AttributeType"/>.</summary>
        public static bool TryParsePrimaryAttribute(string label, out AttributeType type)
        {
            switch (label)
            {
                case "Vitalidade":
                    type = AttributeType.Vitality;
                    return true;
                case "Espírito":
                    type = AttributeType.Spirit;
                    return true;
                case "Destreza":
                    type = AttributeType.Dexterity;
                    return true;
                case "Inteligência":
                    type = AttributeType.Intelligence;
                    return true;
                case "Poder Arcano":
                    type = AttributeType.ArcanePower;
                    return true;
                default:
                    type = default;
                    return false;
            }
        }

        public object CaptureState()
        {
            EnsureInitialized();
            return new AttributeSetSaveState
            {
                vitality = _allocatedPoints[AttributeType.Vitality],
                spirit = _allocatedPoints[AttributeType.Spirit],
                dexterity = _allocatedPoints[AttributeType.Dexterity],
                intelligence = _allocatedPoints[AttributeType.Intelligence],
                arcanePower = _allocatedPoints[AttributeType.ArcanePower],
                availableAttributePoints = AvailableAttributePoints,
                lastKnownPlayerLevel = _lastKnownPlayerLevel
            };
        }

        public void RestoreState(object state)
        {
            EnsureInitialized();

            if (state is not AttributeSetSaveState saveState)
            {
                return;
            }

            _allocatedPoints[AttributeType.Vitality] = saveState.vitality;
            _allocatedPoints[AttributeType.Spirit] = saveState.spirit;
            _allocatedPoints[AttributeType.Dexterity] = saveState.dexterity;
            _allocatedPoints[AttributeType.Intelligence] = saveState.intelligence;
            _allocatedPoints[AttributeType.ArcanePower] = saveState.arcanePower;
            AvailableAttributePoints = saveState.availableAttributePoints;
            _lastKnownPlayerLevel = Mathf.Max(1, saveState.lastKnownPlayerLevel);

            ApplyAllDerivedEffects();
            OnAttributesChanged?.Invoke();
        }
    }
}
