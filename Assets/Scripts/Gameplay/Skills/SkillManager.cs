using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Combat;
using UnityEngine;

namespace Rapadura.Gameplay.Skills
{
    /// <summary>Serializable snapshot of one learned skill for the save system.</summary>
    [Serializable]
    public class LearnedSkillSaveEntry
    {
        public string skillId;
        public int level;
        public int experience;
        public int equippedSlot = -1;
    }

    /// <summary>Serializable snapshot of the full skill book for the save system.</summary>
    [Serializable]
    public class SkillManagerSaveState
    {
        public LearnedSkillSaveEntry[] skills = Array.Empty<LearnedSkillSaveEntry>();
        public int availableSkillPoints;
        public int lastKnownPlayerLevel = 1;
    }

    /// <summary>
    /// Central runtime authority over a character's skills: learning, evolving, equipping into
    /// hotbar slots, cooldown tracking, resource costs, damage/heal calculation and triggering
    /// the associated animation/sound/particles/VFX. Reads every per-skill number from the
    /// data-driven <see cref="SkillDefinition"/> asset — this class contains no per-skill
    /// special-casing, so new skills never require a code change.
    /// </summary>
    public class SkillManager : MonoBehaviour, ISaveable
    {
        [Header("Configuration")]
        [SerializeField] private int _hotbarSlotCount = 6;
        [SerializeField] private LayerMask _targetLayerMask = ~0;
        [SerializeField] private float _minimumAreaRadius = 0.5f;

        [Header("Skill Tree Progression")]
        [SerializeField] private int _skillPointsPerLevel = 1;

        [Header("Skill Database")]
        [Tooltip("Every SkillDefinition this character could ever learn. Used to resolve skill IDs back to assets when loading a save file.")]
        [SerializeField] private SkillDefinition[] _availableSkills = Array.Empty<SkillDefinition>();

        [Header("Optional References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private BuffController _buffController;

        private readonly Dictionary<string, SkillInstance> _learnedSkills = new Dictionary<string, SkillInstance>();
        private Dictionary<string, SkillDefinition> _skillDatabase;
        private SkillInstance[] _equippedSlots;
        private ISkillResourceProvider _resourceProvider;
        private ICombatTarget _selfCombatTarget;
        private int _lastKnownPlayerLevel = 1;

        public string SaveKey => "SkillManager";

        public IReadOnlyCollection<SkillInstance> LearnedSkills => _learnedSkills.Values;
        public IReadOnlyList<SkillInstance> EquippedSlots => _equippedSlots;
        public int AvailableSkillPoints { get; private set; }

        public event Action OnSkillPointsChanged;

        private void Awake()
        {
            _equippedSlots = new SkillInstance[_hotbarSlotCount];
            _resourceProvider = GetComponent<ISkillResourceProvider>();
            _selfCombatTarget = GetComponent<ICombatTarget>();

            _skillDatabase = new Dictionary<string, SkillDefinition>();
            foreach (SkillDefinition definition in _availableSkills)
            {
                if (definition != null)
                {
                    _skillDatabase[definition.SkillId] = definition;
                }
            }

            if (_buffController == null)
            {
                _buffController = GetComponent<BuffController>();
            }

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            if (Rapadura.Core.DI.ServiceLocator.TryGet(out Rapadura.Save.SaveManager saveManager))
            {
                saveManager.Register(this);
            }

            EventBus.Subscribe<PlayerExperienceChangedEvent>(HandlePlayerExperienceChanged);
        }

        private void OnDestroy()
        {
            if (Rapadura.Core.DI.ServiceLocator.TryGet(out Rapadura.Save.SaveManager saveManager))
            {
                saveManager.Unregister(this);
            }

            EventBus.Unsubscribe<PlayerExperienceChangedEvent>(HandlePlayerExperienceChanged);
        }

        private void HandlePlayerExperienceChanged(PlayerExperienceChangedEvent evt)
        {
            if (evt.Level <= _lastKnownPlayerLevel)
            {
                return;
            }

            int levelsGained = evt.Level - _lastKnownPlayerLevel;
            _lastKnownPlayerLevel = evt.Level;
            GrantSkillPoints(levelsGained * _skillPointsPerLevel);
        }

        public void GrantSkillPoints(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            AvailableSkillPoints += amount;
            OnSkillPointsChanged?.Invoke();
        }

        /// <summary>Learns a brand-new skill from the skill tree, spending one skill point. Requirements are still enforced.</summary>
        public bool LearnSkillWithPoint(SkillDefinition definition, int casterLevel)
        {
            if (AvailableSkillPoints <= 0 || !CanLearnSkill(definition, casterLevel))
            {
                return false;
            }

            SkillInstance instance = LearnSkill(definition, casterLevel, force: true);

            if (instance == null)
            {
                return false;
            }

            AvailableSkillPoints--;
            OnSkillPointsChanged?.Invoke();
            return true;
        }

        /// <summary>Spends one skill point to raise an already-learned skill's level by one.</summary>
        public bool LevelUpSkillWithPoint(string skillId)
        {
            if (AvailableSkillPoints <= 0 || !_learnedSkills.TryGetValue(skillId, out SkillInstance instance))
            {
                return false;
            }

            if (!instance.LevelUp())
            {
                return false;
            }

            AvailableSkillPoints--;
            OnSkillPointsChanged?.Invoke();
            EventBus.Publish(new SkillLeveledUpEvent(instance.Definition, instance.Level));
            return true;
        }

        public SkillInstance GetLearnedSkill(string skillId)
        {
            return _learnedSkills.TryGetValue(skillId, out SkillInstance instance) ? instance : null;
        }

        public bool IsSkillLearned(string skillId)
        {
            return _learnedSkills.ContainsKey(skillId);
        }

        private void Update()
        {
            foreach (SkillInstance instance in _learnedSkills.Values)
            {
                instance.TickCooldown(Time.deltaTime);
            }
        }

        // ---------------------------------------------------------------
        // Learning & Progression
        // ---------------------------------------------------------------

        public bool CanLearnSkill(SkillDefinition definition, int casterLevel)
        {
            if (definition == null || _learnedSkills.ContainsKey(definition.SkillId))
            {
                return false;
            }

            SkillRequirement requirement = definition.Requirement;

            if (casterLevel < requirement.minimumPlayerLevel)
            {
                return false;
            }

            foreach (SkillDefinition required in requirement.requiredSkills)
            {
                if (required == null)
                {
                    continue;
                }

                if (!_learnedSkills.TryGetValue(required.SkillId, out SkillInstance requiredInstance))
                {
                    return false;
                }

                if (requiredInstance.Level < requirement.requiredSkillLevel)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Learns a new skill, bypassing requirement checks if <paramref name="force"/> is true (used by save/load and admin tools).</summary>
        public SkillInstance LearnSkill(SkillDefinition definition, int casterLevel, bool force = false)
        {
            if (!force && !CanLearnSkill(definition, casterLevel))
            {
                return null;
            }

            if (_learnedSkills.TryGetValue(definition.SkillId, out SkillInstance existing))
            {
                return existing;
            }

            var instance = new SkillInstance(definition);
            _learnedSkills[definition.SkillId] = instance;
            ApplyPassiveEffectIfNeeded(definition);

            EventBus.Publish(new SkillLearnedEvent(definition));
            return instance;
        }

        /// <summary>
        /// Passives grant their bonus automatically the instant they're learned, rather than requiring
        /// a cast through <see cref="TryCast"/>. The buff is tagged with a source key derived from the
        /// skill id so <see cref="RespecSkill"/>/<see cref="ResetSkillPoints"/> can cleanly remove it again.
        /// </summary>
        private void ApplyPassiveEffectIfNeeded(SkillDefinition definition)
        {
            if (definition.SkillType != SkillType.Passive || _buffController == null || definition.Buffs.Length == 0)
            {
                return;
            }

            _buffController.ApplyEffects(definition.Buffs, PassiveSourceKey(definition.SkillId));
        }

        private static string PassiveSourceKey(string skillId)
        {
            return "passive_" + skillId;
        }

        public void GrantSkillExperience(string skillId, int amount)
        {
            if (!_learnedSkills.TryGetValue(skillId, out SkillInstance instance))
            {
                return;
            }

            int levelBefore = instance.Level;
            instance.AddExperience(amount);

            if (instance.Level != levelBefore)
            {
                EventBus.Publish(new SkillLeveledUpEvent(instance.Definition, instance.Level));
            }
        }

        // ---------------------------------------------------------------
        // Respec
        // ---------------------------------------------------------------

        /// <summary>
        /// Unlearns a single skill, refunding every skill point spent on it (its current level —
        /// one point was spent learning it, one more per subsequent level-up) and undoing any
        /// passive buff it applied. Refuses to respec a skill that another currently-learned skill
        /// still depends on (via <see cref="SkillRequirement.requiredSkills"/>) — unlearn the
        /// dependent skill first, or use <see cref="ResetSkillPoints"/> to clear everything at once.
        /// </summary>
        public bool RespecSkill(string skillId)
        {
            if (!_learnedSkills.TryGetValue(skillId, out SkillInstance instance))
            {
                return false;
            }

            if (HasLearnedDependents(skillId))
            {
                return false;
            }

            RefundAndRemove(instance);
            return true;
        }

        /// <summary>
        /// Full respec: unlearns every learned skill, refunding all spent skill points and undoing
        /// every passive buff. Dependencies are resolved automatically (skills with dependents still
        /// learned are removed after their dependents), so this always succeeds unless nothing is learned.
        /// Returns the total number of skill points refunded.
        /// </summary>
        public int ResetSkillPoints()
        {
            int totalRefunded = 0;

            while (_learnedSkills.Count > 0)
            {
                string removableId = null;

                foreach (string id in _learnedSkills.Keys)
                {
                    if (!HasLearnedDependents(id))
                    {
                        removableId = id;
                        break;
                    }
                }

                if (removableId == null)
                {
                    // Should never happen (would imply a circular requirement graph) — bail out safely.
                    break;
                }

                SkillInstance instance = _learnedSkills[removableId];
                int pointsBefore = AvailableSkillPoints;
                RefundAndRemove(instance);
                totalRefunded += AvailableSkillPoints - pointsBefore;
            }

            return totalRefunded;
        }

        private bool HasLearnedDependents(string skillId)
        {
            foreach (SkillInstance learned in _learnedSkills.Values)
            {
                foreach (SkillDefinition required in learned.Definition.Requirement.requiredSkills)
                {
                    if (required != null && required.SkillId == skillId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void RefundAndRemove(SkillInstance instance)
        {
            SkillDefinition definition = instance.Definition;

            for (int i = 0; i < _equippedSlots.Length; i++)
            {
                if (_equippedSlots[i] == instance)
                {
                    UnequipSkill(i);
                }
            }

            if (definition.SkillType == SkillType.Passive && _buffController != null)
            {
                _buffController.RemoveEffectsBySource(PassiveSourceKey(definition.SkillId));
            }

            int refund = instance.Level;
            _learnedSkills.Remove(definition.SkillId);

            AvailableSkillPoints += refund;
            OnSkillPointsChanged?.Invoke();
            EventBus.Publish(new SkillRespecEvent(definition, refund));
        }

        // ---------------------------------------------------------------
        // Equip / Unequip
        // ---------------------------------------------------------------

        public bool EquipSkill(int slotIndex, string skillId)
        {
            if (!IsValidSlot(slotIndex) || !_learnedSkills.TryGetValue(skillId, out SkillInstance instance))
            {
                return false;
            }

            _equippedSlots[slotIndex] = instance;
            EventBus.Publish(new SkillEquippedChangedEvent(slotIndex, instance.Definition));
            return true;
        }

        public void UnequipSkill(int slotIndex)
        {
            if (!IsValidSlot(slotIndex))
            {
                return;
            }

            _equippedSlots[slotIndex] = null;
            EventBus.Publish(new SkillEquippedChangedEvent(slotIndex, null));
        }

        private bool IsValidSlot(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _equippedSlots.Length;
        }

        // ---------------------------------------------------------------
        // Casting
        // ---------------------------------------------------------------

        /// <summary>Attempts to cast whatever skill is equipped in the given hotbar slot.</summary>
        public bool TryCastSlot(int slotIndex, Vector3 aimPoint, GameObject explicitTarget = null)
        {
            if (!IsValidSlot(slotIndex) || _equippedSlots[slotIndex] == null)
            {
                return false;
            }

            return TryCast(_equippedSlots[slotIndex], aimPoint, explicitTarget);
        }

        /// <summary>Attempts to cast a specific learned skill instance, applying costs, cooldown, damage/healing and effects.</summary>
        public bool TryCast(SkillInstance instance, Vector3 aimPoint, GameObject explicitTarget = null)
        {
            if (instance == null || instance.IsOnCooldown)
            {
                return false;
            }

            SkillDefinition skill = instance.Definition;

            if (!CanAffordCost(skill))
            {
                return false;
            }

            PayCost(skill);
            instance.StartCooldown(GetCooldownReductionPercent());

            EventBus.Publish(new SkillCastEvent(gameObject, skill, aimPoint));
            PlayPresentation(skill);

            ApplyBuffsToSelf(skill, instance.Level);
            ApplyDamageAndDebuffs(skill, instance.Level, aimPoint, explicitTarget);
            ApplyHealingToSelf(skill, instance.Level);

            return true;
        }

        private bool CanAffordCost(SkillDefinition skill)
        {
            if (_resourceProvider == null)
            {
                return true;
            }

            float multiplier = 1f - GetResourceCostReductionPercent();
            return _resourceProvider.HasMana(skill.ManaCost * multiplier) && _resourceProvider.HasEnergy(skill.EnergyCost * multiplier);
        }

        private void PayCost(SkillDefinition skill)
        {
            if (_resourceProvider == null)
            {
                return;
            }

            float multiplier = 1f - GetResourceCostReductionPercent();
            float manaCost = skill.ManaCost * multiplier;
            float energyCost = skill.EnergyCost * multiplier;

            if (manaCost > 0f)
            {
                _resourceProvider.ConsumeMana(manaCost);
            }

            if (energyCost > 0f)
            {
                _resourceProvider.ConsumeEnergy(energyCost);
            }
        }

        private float GetCooldownReductionPercent()
        {
            return _buffController != null ? Mathf.Clamp01(_buffController.GetModifiedValue(StatType.CooldownReduction, 0f)) : 0f;
        }

        /// <summary>Percent (0..1) taken off mana/energy costs, e.g. from a completed <see cref="ComboDefinition"/> CostReduction bonus.</summary>
        private float GetResourceCostReductionPercent()
        {
            return _buffController != null ? Mathf.Clamp01(_buffController.GetModifiedValue(StatType.ResourceCostReduction, 0f)) : 0f;
        }

        private void PlayPresentation(SkillDefinition skill)
        {
            if (_animator != null && !string.IsNullOrEmpty(skill.AnimationTrigger))
            {
                _animator.SetTrigger(skill.AnimationTrigger);
            }

            if (skill.CastSound != null)
            {
                AudioSource.PlayClipAtPoint(skill.CastSound, transform.position);
            }

            if (skill.CastParticlesPrefab != null)
            {
                GameObject particles = Instantiate(skill.CastParticlesPrefab, transform.position, transform.rotation);
                Destroy(particles, 5f);
            }
        }

        private void ApplyBuffsToSelf(SkillDefinition skill, int level)
        {
            if (_buffController == null || skill.Buffs.Length == 0)
            {
                return;
            }

            _buffController.ApplyEffects(skill.Buffs);
        }

        private void ApplyHealingToSelf(SkillDefinition skill, int level)
        {
            float healing = skill.GetHealingForLevel(level);

            if (healing <= 0f || _selfCombatTarget == null)
            {
                return;
            }

            _selfCombatTarget.ApplyHealing(healing, gameObject);
        }

        private void ApplyDamageAndDebuffs(SkillDefinition skill, int level, Vector3 aimPoint, GameObject explicitTarget)
        {
            float damage = skill.GetDamageForLevel(level);

            // Applies AttackDamage buffs — including the DamageMultiplier bonus from a completed combo.
            if (damage > 0f && _buffController != null)
            {
                damage = _buffController.GetModifiedValue(StatType.AttackDamage, damage);
            }

            if (damage <= 0f && skill.Debuffs.Length == 0)
            {
                return;
            }

            if (explicitTarget != null)
            {
                ApplyToTarget(explicitTarget, skill, damage);
                return;
            }

            float radius = Mathf.Max(skill.AreaRadius, _minimumAreaRadius);
            Collider[] hits = Physics.OverlapSphere(aimPoint, radius, _targetLayerMask, QueryTriggerInteraction.Collide);

            foreach (Collider hit in hits)
            {
                if (hit.gameObject == gameObject)
                {
                    continue;
                }

                ApplyToTarget(hit.gameObject, skill, damage);

                if (skill.ImpactVfxPrefab != null)
                {
                    GameObject vfx = Instantiate(skill.ImpactVfxPrefab, hit.ClosestPoint(aimPoint), Quaternion.identity);
                    Destroy(vfx, 5f);
                }
            }
        }

        private void ApplyToTarget(GameObject targetObject, SkillDefinition skill, float damage)
        {
            if (!targetObject.TryGetComponent(out ICombatTarget target) || !target.IsAlive)
            {
                return;
            }

            if (damage > 0f)
            {
                target.ApplyDamage(damage, skill.Element, gameObject);
            }

            if (skill.Debuffs.Length > 0 && targetObject.TryGetComponent(out BuffController targetBuffs))
            {
                targetBuffs.ApplyEffects(skill.Debuffs);
            }
        }

        // ---------------------------------------------------------------
        // Persistence
        // ---------------------------------------------------------------

        public object CaptureState()
        {
            var entries = new List<LearnedSkillSaveEntry>(_learnedSkills.Count);

            foreach (SkillInstance instance in _learnedSkills.Values)
            {
                int equippedSlot = Array.IndexOf(_equippedSlots, instance);

                entries.Add(new LearnedSkillSaveEntry
                {
                    skillId = instance.Definition.SkillId,
                    level = instance.Level,
                    experience = instance.Experience,
                    equippedSlot = equippedSlot
                });
            }

            return new SkillManagerSaveState
            {
                skills = entries.ToArray(),
                availableSkillPoints = AvailableSkillPoints,
                lastKnownPlayerLevel = _lastKnownPlayerLevel
            };
        }

        public void RestoreState(object state)
        {
            if (state is not SkillManagerSaveState saveState)
            {
                return;
            }

            _learnedSkills.Clear();
            Array.Clear(_equippedSlots, 0, _equippedSlots.Length);

            foreach (LearnedSkillSaveEntry entry in saveState.skills)
            {
                if (!_skillDatabase.TryGetValue(entry.skillId, out SkillDefinition definition))
                {
                    Debug.LogWarning($"[SkillManager] Unknown skill id '{entry.skillId}' in save data. Skipping.");
                    continue;
                }

                var instance = new SkillInstance(definition);
                instance.SetLevel(entry.level, entry.experience);
                _learnedSkills[entry.skillId] = instance;
                ApplyPassiveEffectIfNeeded(definition);

                if (entry.equippedSlot >= 0 && IsValidSlot(entry.equippedSlot))
                {
                    _equippedSlots[entry.equippedSlot] = instance;
                }
            }

            AvailableSkillPoints = saveState.availableSkillPoints;
            _lastKnownPlayerLevel = Mathf.Max(1, saveState.lastKnownPlayerLevel);
            OnSkillPointsChanged?.Invoke();
        }
    }
}
