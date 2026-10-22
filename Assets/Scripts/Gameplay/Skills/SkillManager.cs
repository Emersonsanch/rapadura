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
        [Tooltip("Skill points per level and the level damage multiplier come from this curve (or the shipped Resources/Balance/ProgressionCurve when empty); the per-level field below is only a fallback if no curve exists.")]
        [SerializeField] private Rapadura.Gameplay.Player.ProgressionCurve _progressionCurve;
        [SerializeField] private int _skillPointsPerLevel = 1;

        [Header("Skill Database")]
        [Tooltip("Every SkillDefinition this character could ever learn. Used to resolve skill IDs back to assets when loading a save file.")]
        [SerializeField] private SkillDefinition[] _availableSkills = Array.Empty<SkillDefinition>();

        [Header("Optional References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private BuffController _buffController;
        [Tooltip("Crit tuning shared with the basic attack. Left empty, the shipped Resources/Balance/PlayerCombatBalance asset is used.")]
        [SerializeField] private PlayerCombatBalance _combatBalance;

        private readonly Dictionary<string, SkillInstance> _learnedSkills = new Dictionary<string, SkillInstance>();
        private Dictionary<string, SkillDefinition> _skillDatabase;
        private SkillInstance[] _equippedSlots;
        private ISkillResourceProvider _resourceProvider;
        private ICombatTarget _selfCombatTarget;
        private int _lastKnownPlayerLevel = 1;

        public string SaveKey => "SkillManager";

        public IReadOnlyCollection<SkillInstance> LearnedSkills => _learnedSkills.Values;
        public IReadOnlyList<SkillInstance> EquippedSlots
        {
            get { EnsureInitialized(); return _equippedSlots; }
        }
        public int AvailableSkillPoints { get; private set; }

        public event Action OnSkillPointsChanged;

        /// <summary>Uniform [0, 1] sampler for skill crit rolls. Null = Unity's global RNG; tests inject a fixed value.</summary>
        public Func<float> CriticalRoll { get; set; }

        private bool _combatBalanceLookupDone;

        private bool _initialized;

        private void Awake()
        {
            EnsureInitialized();
        }

        /// <summary>
        /// Guards every public entry point that touches <see cref="_equippedSlots"/>/
        /// <see cref="_skillDatabase"/>/<see cref="_buffController"/> against being called before
        /// <see cref="Awake"/> has run — in Edit Mode (outside Play Mode),
        /// <c>AddComponent&lt;SkillManager&gt;()</c> does NOT call <c>Awake</c> synchronously the
        /// way Play Mode always does. Concretely: <c>RefundAndRemove</c> indexed the still-null
        /// <c>_equippedSlots</c> array (NullReferenceException), and passive skills silently
        /// granted no buff at all since <c>ApplyPassiveEffectIfNeeded</c> treats a null
        /// <c>_buffController</c> as "no buff controller attached" rather than "not initialized yet".
        /// </summary>
        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
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

        /// <summary>
        /// Adds skills to the lookup database at runtime (on top of the serialized
        /// <see cref="_availableSkills"/>), so skills granted after scene setup — e.g. the selected
        /// character's signature set from <c>CharacterSkillSet</c> — can still be resolved by id when
        /// a save file is restored. Already-known ids are simply overwritten with the same asset.
        /// </summary>
        public void RegisterSkills(IEnumerable<SkillDefinition> definitions)
        {
            EnsureInitialized();

            if (definitions == null)
            {
                return;
            }

            foreach (SkillDefinition definition in definitions)
            {
                if (definition != null)
                {
                    _skillDatabase[definition.SkillId] = definition;
                }
            }
        }

        /// <summary>
        /// Reverse of <see cref="RegisterSkills"/>: drops runtime-registered ids from the lookup database
        /// (e.g. the previous hero's signature set on a hero switch). Skills from the serialized
        /// <see cref="_availableSkills"/> stay known.
        /// </summary>
        public void UnregisterSkills(IEnumerable<SkillDefinition> definitions)
        {
            EnsureInitialized();

            if (definitions == null)
            {
                return;
            }

            foreach (SkillDefinition definition in definitions)
            {
                if (definition != null && Array.IndexOf(_availableSkills, definition) < 0)
                {
                    _skillDatabase.Remove(definition.SkillId);
                }
            }
        }

        /// <summary>True if a skill with this id is known to this manager (serialized or registered at runtime).</summary>
        public bool IsSkillRegistered(string skillId)
        {
            EnsureInitialized();
            return _skillDatabase.ContainsKey(skillId);
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
            int previousLevel = _lastKnownPlayerLevel;
            _lastKnownPlayerLevel = evt.Level;
            Rapadura.Gameplay.Player.ProgressionCurve curve = ResolveProgressionCurve();
            GrantSkillPoints(curve != null
                ? curve.GetSkillPointsGained(previousLevel, evt.Level)
                : levelsGained * _skillPointsPerLevel);
        }

        private Rapadura.Gameplay.Player.ProgressionCurve ResolveProgressionCurve()
        {
            if (_progressionCurve == null)
            {
                _progressionCurve = Rapadura.Gameplay.Player.ProgressionCurve.LoadDefault();
            }

            return _progressionCurve;
        }

        public void GrantSkillPoints(int amount)
        {
            EnsureInitialized();

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
            EnsureInitialized();

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

        /// <summary>BuffController source key of a cast skill's self-buffs.</summary>
        public static string SelfBuffSourceKey(string skillId)
        {
            return "skill_" + skillId;
        }

        /// <summary>BuffController source key of a cast skill's debuffs on its targets.</summary>
        public static string DebuffSourceKey(string skillId)
        {
            return "skill_" + skillId + "_debuff";
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
            EnsureInitialized();

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
            EnsureInitialized();
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

        /// <summary>
        /// Unlearns <paramref name="skillId"/> without <see cref="RespecSkill"/>'s dependency check and
        /// refunds exactly <paramref name="pointsToRefund"/> (skills granted for free must not turn into
        /// free points). Unequips it, removes its passive buff and publishes <see cref="SkillRespecEvent"/>
        /// (so <see cref="SkillHotbar"/> prunes it). Used when the selected hero changes.
        /// </summary>
        public bool ForgetSkill(string skillId, int pointsToRefund)
        {
            EnsureInitialized();

            if (string.IsNullOrEmpty(skillId) || !_learnedSkills.TryGetValue(skillId, out SkillInstance instance))
            {
                return false;
            }

            RefundAndRemove(instance, Mathf.Max(0, pointsToRefund));
            return true;
        }

        private void RefundAndRemove(SkillInstance instance)
        {
            RefundAndRemove(instance, instance.Level);
        }

        private void RefundAndRemove(SkillInstance instance, int refund)
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
            EnsureInitialized();
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
            EnsureInitialized();

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

        /// <summary>
        /// Read-only version of the cost check <see cref="TryCast"/> runs (including any
        /// ResourceCostReduction buff), so UI such as the HUD skill hotbar can grey out a slot the
        /// caster can't currently pay for without duplicating the cost math.
        /// </summary>
        public bool CanAffordSkill(SkillDefinition skill)
        {
            EnsureInitialized();
            return skill != null && CanAffordCost(skill);
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

            // Per-skill source: with a null source two different skills' same-stat buffs (or a
            // skill's and a potion's) replaced each other; recasting the same skill still refreshes.
            _buffController.ApplyEffects(skill.Buffs, SelfBuffSourceKey(skill.SkillId));
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
            // Shared attacker pipeline (same as PlayerBasicAttack): player-level damage growth from the
            // progression curve (1x with the default curve), AttackDamage buffs — including the
            // DamageMultiplier bonus from a completed combo — and one crit roll per cast (Destreza).
            Rapadura.Gameplay.Player.ProgressionCurve curve = ResolveProgressionCurve();
            float levelMultiplier = curve != null ? curve.GetDamageMultiplier(_lastKnownPlayerLevel) : 1f;
            float damage = OutgoingDamage.Resolve(skill.GetDamageForLevel(level), levelMultiplier, _buffController,
                ResolveCombatBalance(), CriticalRoll, out bool isCritical);

            if (damage <= 0f && skill.Debuffs.Length == 0)
            {
                return;
            }

            if (explicitTarget != null)
            {
                ApplyToTarget(explicitTarget, skill, damage, isCritical);
                return;
            }

            float radius = Mathf.Max(skill.AreaRadius, _minimumAreaRadius);
            Collider[] hits = Physics.OverlapSphere(aimPoint, radius, _targetLayerMask, QueryTriggerInteraction.Collide);

            // Resolve each collider to its owning ICombatTarget (enemy colliders often live on child
            // meshes, and CharacterController + hurtbox gives several per enemy) so a correctly aimed
            // area hits every target exactly once — the old hit.gameObject lookup silently skipped
            // child colliders, and the caster's own child colliders are excluded the same way.
            var alreadyHit = new HashSet<ICombatTarget>();

            foreach (Collider hit in hits)
            {
                ICombatTarget target = hit.GetComponentInParent<ICombatTarget>();

                if (target == null || target.Transform == transform || !alreadyHit.Add(target))
                {
                    continue;
                }

                ApplyToTarget(target.Transform.gameObject, skill, damage, isCritical);

                if (skill.ImpactVfxPrefab != null)
                {
                    GameObject vfx = Instantiate(skill.ImpactVfxPrefab, hit.ClosestPoint(aimPoint), Quaternion.identity);
                    Destroy(vfx, 5f);
                }
            }
        }

        private PlayerCombatBalance ResolveCombatBalance()
        {
            if (_combatBalance == null && !_combatBalanceLookupDone)
            {
                _combatBalanceLookupDone = true;
                _combatBalance = PlayerCombatBalance.LoadDefault();
            }

            return _combatBalance;
        }

        private void ApplyToTarget(GameObject targetObject, SkillDefinition skill, float damage, bool isCritical)
        {
            if (!targetObject.TryGetComponent(out ICombatTarget target) || !target.IsAlive)
            {
                return;
            }

            if (damage > 0f)
            {
                // Per-skill channel: Health only blocks a repeat of THIS skill within its short window,
                // so a skill cast right after a basic attack/other skill still lands.
                OutgoingDamage.DeliverDamage(target, damage, skill.Element, gameObject, isCritical, DamageChannel.ForSkill(skill.SkillId));
            }

            if (skill.Debuffs.Length > 0 && targetObject.TryGetComponent(out BuffController targetBuffs))
            {
                // Caster as instigator: DoT ticks are credited to the player (kill XP/loot, damage numbers).
                targetBuffs.ApplyEffects(skill.Debuffs, DebuffSourceKey(skill.SkillId), gameObject);
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

            EnsureInitialized();

            _learnedSkills.Clear();
            Array.Clear(_equippedSlots, 0, _equippedSlots.Length);

            foreach (LearnedSkillSaveEntry entry in saveState.skills)
            {
                if (!_skillDatabase.TryGetValue(entry.skillId, out SkillDefinition definition))
                {
                    // Character signature skills are only registered once a hero is applied, which
                    // may not have happened yet (save restored before/without the character select
                    // screen) — resolve them straight from Resources/Skills/Characters instead.
                    definition = Rapadura.Gameplay.Characters.CharacterSkillSet.FindSkill(entry.skillId);

                    if (definition == null)
                    {
                        Debug.LogWarning($"[SkillManager] Unknown skill id '{entry.skillId}' in save data. Skipping.");
                        continue;
                    }

                    _skillDatabase[definition.SkillId] = definition;
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
