using System;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Serializable snapshot of PlayerStats used by the save system.
    /// </summary>
    [Serializable]
    public class PlayerStatsSaveState
    {
        public float currentHealth;
        public float currentStamina;
        public float currentMana;
        public int currentExperience;
        public int level;
    }

    /// <summary>
    /// Holds the player's core vitals: health, stamina/energy, mana and experience/level.
    /// Publishes change events through the EventBus so UI (health bar, stamina bar, XP bar)
    /// stays decoupled from gameplay logic, and persists itself through ISaveable.
    /// Also implements ICombatTarget so skills and future combat/AI systems can damage or
    /// heal the player without depending on this concrete class.
    /// </summary>
    public class PlayerStats : MonoBehaviour, ISaveable, IRestoreOrder, ICombatTarget, ISkillResourceProvider, IMeleeDamageTarget
    {
        [Header("Health")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private float _healthRegenPerSecond = 1f;

        [Header("Stamina")]
        [SerializeField] private float _maxStamina = 100f;
        [SerializeField] private float _staminaDrainPerSecond = 15f;
        [SerializeField] private float _staminaRegenPerSecond = 10f;
        [SerializeField] private float _minStaminaToRun = 5f;

        [Header("Mana")]
        [SerializeField] private float _maxMana = 100f;
        [SerializeField] private float _manaRegenPerSecond = 5f;

        [Header("Defense")]
        [Tooltip("Flat armor points before modifiers (affixes add more). Goes through DamageCalculator's armor curve; percent Defense buffs/debuffs from skills stack on top.")]
        [SerializeField] private float _baseDefense = 0f;

        [Header("Experience")]
        [Tooltip("Level curve (XP per level, vitals growth). Left empty, the shipped Resources/Balance/ProgressionCurve asset is used; the two fields below are only a fallback if that asset is missing.")]
        [SerializeField] private ProgressionCurve _progressionCurve;
        [SerializeField] private int _baseExperienceToLevel = 100;
        [SerializeField] private float _experienceCurveMultiplier = 1.25f;

        [Header("Invulnerability")]
        [Tooltip("Seconds of damage immunity granted right after taking a hit.")]
        [SerializeField] private float _invulnerabilityDuration = 0.5f;

        private float _invulnerabilityTimer;
        private bool _initialized;

        /// <summary>
        /// XP granted while the previous scene's carried PlayerStats state was still waiting to be
        /// applied (see <see cref="IsAwaitingCarriedState"/>). GameManager restores carried state one
        /// frame after a scene load, so a kill/quest reward in Main.unity's first frames used to be
        /// wiped by <see cref="RestoreState"/> overwriting level/XP. The XP is applied immediately (the
        /// bar still moves) and re-applied on top of the restored state by <see cref="RestoreState"/>.
        /// </summary>
        private int _experienceAwaitingCarriedRestore;

        /// <summary>Optional — absence means no resistance/immunity to any element (see <see cref="ElementResistance"/>).</summary>
        private ElementResistance _elementResistance;

        private float _currentHealth;
        private float _currentStamina;
        private float _currentMana;

        /// <summary>
        /// Every Current* vital reads through <see cref="EnsureInitialized"/> here, not just
        /// <see cref="Awake"/> — in Edit Mode (outside Play Mode), <c>AddComponent&lt;PlayerStats&gt;()</c>
        /// does NOT call <c>Awake</c> synchronously the way Play Mode always does (see the same
        /// fix in <c>Health.cs</c>), which left every vital at its default 0 for any EditMode test
        /// that added the component and read/damaged/healed it in the same call. Routing every
        /// getter through the same guard (rather than only fixing the specific method a test
        /// happened to call) means any current or future caller is covered automatically.
        /// </summary>
        public float CurrentHealth
        {
            get { EnsureInitialized(); return _currentHealth; }
            private set => _currentHealth = value;
        }

        public float CurrentStamina
        {
            get { EnsureInitialized(); return _currentStamina; }
            private set => _currentStamina = value;
        }

        public float CurrentMana
        {
            get { EnsureInitialized(); return _currentMana; }
            private set => _currentMana = value;
        }

        // Base values from the inspector plus whatever the progression curve grants above level 1,
        // then (health/mana) every MaxHealth/MaxMana modifier on the BuffController: Vitalidade/Espírito
        // (AttributeSet), equipment and affixes such as affix_vital (InventoryManager), passives and
        // buffs. Those modifiers used to be applied but never read here, so the attributes did nothing.
        public float MaxMana => ApplyStatModifiers(StatType.MaxMana, _maxMana + (Curve != null ? Curve.GetMaxManaBonus(Level) : 0f));
        public float MaxHealth => ApplyStatModifiers(StatType.MaxHealth, _maxHealth + (Curve != null ? Curve.GetMaxHealthBonus(Level) : 0f));
        public float MaxStamina => ApplyStatModifiers(StatType.MaxStamina, _maxStamina + (Curve != null ? Curve.GetMaxStaminaBonus(Level) : 0f));

        // Regeneration per second through the matching *Regen modifiers (item affixes' Flat +HP/s,
        // future buffs). Tick-over-time effects (DoT/HoT) also use HealthRegen with value 0, so they add
        // nothing here — their damage/heal comes from BuffController's ticks. Never below 0. These
        // stats (and MaxStamina above) were applied by item affixes but read by nothing before.
        public float HealthRegenPerSecond => ApplyRegenModifiers(StatType.HealthRegen, _healthRegenPerSecond);
        public float ManaRegenPerSecond => ApplyRegenModifiers(StatType.ManaRegen, _manaRegenPerSecond);
        public float StaminaRegenPerSecond => ApplyRegenModifiers(StatType.StaminaRegen, _staminaRegenPerSecond);

        /// <summary>Flat armor points (base + Flat Defense modifiers such as item affixes).</summary>
        public float FlatDefense
        {
            get
            {
                DamageCalculator.GetDefense(Buffs, _baseDefense, out float flat, out _);
                return flat;
            }
        }

        /// <summary>
        /// Damage-taken multiplier from defense (see <see cref="DamageCalculator.GetDefenseMultiplier"/>):
        /// 1 = unmitigated, 0.75 under Campo Magnético (+25%), 1.3 under Berserk's -30% Defense.
        /// </summary>
        public float DefenseMultiplier
        {
            get
            {
                DamageCalculator.GetDefense(Buffs, _baseDefense, out float flat, out float percent);
                return DamageCalculator.GetDefenseMultiplier(flat, percent, null);
            }
        }

        /// <summary>
        /// Sibling BuffController (optional), resolved lazily because EditMode tests and runtime setup
        /// may add it after this component. Subscribes once to <see cref="BuffController.OnModifiersChanged"/>
        /// so current health/mana are re-clamped when a max bonus is gained or lost.
        /// </summary>
        private BuffController Buffs
        {
            get
            {
                if (_buffController == null)
                {
                    _buffController = GetComponent<BuffController>();

                    if (_buffController != null)
                    {
                        _buffController.OnModifiersChanged += HandleModifiersChanged;
                    }
                }

                return _buffController;
            }
        }

        private BuffController _buffController;
        private float _lastMaxHealth;
        private float _lastMaxMana;

        private float ApplyStatModifiers(StatType stat, float baseValue)
        {
            BuffController buffs = Buffs;
            // Never below 1 so a big percent debuff can't produce a zero/negative max (division in HUD bars).
            return buffs != null ? Mathf.Max(1f, buffs.GetModifiedValue(stat, baseValue)) : baseValue;
        }

        private float ApplyRegenModifiers(StatType stat, float baseValue)
        {
            BuffController buffs = Buffs;
            return buffs != null ? Mathf.Max(0f, buffs.GetModifiedValue(stat, baseValue)) : baseValue;
        }

        /// <summary>The level curve in use (inspector reference, else the shipped Resources asset; may be null).</summary>
        public ProgressionCurve Curve
        {
            get
            {
                if (_progressionCurve == null && !_curveLookupDone)
                {
                    _curveLookupDone = true;
                    _progressionCurve = ProgressionCurve.LoadDefault();
                }

                return _progressionCurve;
            }
        }

        private bool _curveLookupDone;
        public int CurrentExperience { get; private set; }
        public int Level { get; private set; } = 1;
        public bool IsDead { get; private set; }

        public bool IsAlive => !IsDead;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;
        public Transform Transform => transform;

        public string SaveKey => "PlayerStats";

        /// <summary>
        /// After AttributeSet/InventoryManager/SkillManager (see <see cref="RestoreOrders.AfterStatBonuses"/>):
        /// RestoreState caches MaxHealth/MaxMana, which must already include every bonus.
        /// </summary>
        public int RestoreOrder => RestoreOrders.AfterStatBonuses;

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            CurrentHealth = MaxHealth;
            CurrentStamina = MaxStamina;
            CurrentMana = MaxMana;
            _lastMaxHealth = _currentHealth;
            _lastMaxMana = _currentMana;
            _elementResistance = GetComponent<ElementResistance>();
        }

        private void OnDestroy()
        {
            if (_buffController != null)
            {
                _buffController.OnModifiersChanged -= HandleModifiersChanged;
            }
        }

        private void HandleModifiersChanged()
        {
            if (_initialized)
            {
                RefreshVitalMaximums();
            }
        }

        /// <summary>
        /// Reconciles current health/mana with their (possibly changed) maximums: a vital that was full
        /// stays full at the new max (spawning with Vitalidade, equipping a +HP item at full health);
        /// otherwise it keeps its value, clamped down if the max shrank (buff expired, item unequipped).
        /// Not refilling a partially-damaged bar avoids an equip/unequip heal exploit.
        /// </summary>
        private void RefreshVitalMaximums()
        {
            float maxHealth = MaxHealth;
            if (!Mathf.Approximately(maxHealth, _lastMaxHealth))
            {
                bool wasFull = _currentHealth >= _lastMaxHealth - 0.001f;
                _lastMaxHealth = maxHealth;
                SetHealth(wasFull && !IsDead ? maxHealth : _currentHealth);
            }

            float maxMana = MaxMana;
            if (!Mathf.Approximately(maxMana, _lastMaxMana))
            {
                bool wasFull = _currentMana >= _lastMaxMana - 0.001f;
                _lastMaxMana = maxMana;
                SetMana(wasFull ? maxMana : _currentMana);
            }

            // Stamina regenerates in seconds, so a lost MaxStamina bonus only needs the clamp (and the
            // HUD refresh); a gained one refills on its own.
            if (_currentStamina > MaxStamina)
            {
                SetStamina(_currentStamina);
            }
        }

        private void Update()
        {
            RegenerateStamina(Time.deltaTime);
            RegenerateHealth(Time.deltaTime);
            RegenerateMana(Time.deltaTime);
            TickInvulnerability(Time.deltaTime);
        }

        private void TickInvulnerability(float deltaTime)
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= deltaTime;
            }
        }

        /// <summary>Grants temporary damage immunity, refreshing/extending any window already in progress.</summary>
        public void GrantInvulnerability(float duration)
        {
            _invulnerabilityTimer = Mathf.Max(_invulnerabilityTimer, duration);
        }

        /// <summary>Immediately ends any active invulnerability window.</summary>
        public void ClearInvulnerability()
        {
            _invulnerabilityTimer = 0f;
        }

        public bool HasMana(float amount)
        {
            return CurrentMana >= amount;
        }

        public void ConsumeMana(float amount)
        {
            SetMana(CurrentMana - amount);
        }

        public bool HasEnergy(float amount)
        {
            return CurrentStamina >= amount;
        }

        public void ConsumeEnergy(float amount)
        {
            SetStamina(CurrentStamina - amount);
        }

        private void RegenerateMana(float deltaTime)
        {
            if (CurrentMana < MaxMana)
            {
                SetMana(CurrentMana + ManaRegenPerSecond * deltaTime);
            }
        }

        public void RestoreMana(float amount)
        {
            SetMana(CurrentMana + amount);
        }

        private void SetMana(float value)
        {
            CurrentMana = Mathf.Clamp(value, 0f, MaxMana);
            EventBus.Publish(new PlayerManaChangedEvent(CurrentMana, MaxMana));
        }

        public bool HasStamina()
        {
            return CurrentStamina > _minStaminaToRun;
        }

        public void DrainStamina(float deltaTime)
        {
            SetStamina(CurrentStamina - _staminaDrainPerSecond * deltaTime);
        }

        private void RegenerateStamina(float deltaTime)
        {
            if (CurrentStamina < MaxStamina)
            {
                SetStamina(CurrentStamina + StaminaRegenPerSecond * deltaTime);
            }
        }

        public void RestoreStamina(float amount)
        {
            SetStamina(CurrentStamina + amount);
        }

        private void RegenerateHealth(float deltaTime)
        {
            if (!IsDead && CurrentHealth < MaxHealth)
            {
                SetHealth(CurrentHealth + HealthRegenPerSecond * deltaTime);
            }
        }

        public void ApplyDamage(float amount)
        {
            ApplyDamageInternal(amount, ElementType.None, null, isCritical: false);
        }

        /// <summary>
        /// Shared damage-application path for both the plain <see cref="ApplyDamage(float)"/> entry
        /// point and <see cref="ICombatTarget"/>'s. Also publishes <see cref="DamageAppliedEvent"/>
        /// so the player benefits from the same audio/analytics hooks as <see cref="Health"/>-based
        /// targets (which previously only fired for enemies/NPCs, never the player).
        /// Defense (base armor + affixes + skill buffs/debuffs) is applied here, so every way of hurting
        /// the player is mitigated: <c>amount × DamageCalculator.GetDefenseMultiplier</c>. Unlike
        /// <see cref="Health"/> there is no rounding/minimum-damage floor, so an unbuffed player (multiplier
        /// exactly 1) takes exactly the same damage as before Defense was wired in.
        /// </summary>
        /// <returns>The damage actually taken (0 when dead, invulnerable or a non-positive amount).</returns>
        private float ApplyDamageInternal(float amount, ElementType element, GameObject source, bool isCritical)
        {
            if (IsDead || IsInvulnerable || amount <= 0f)
            {
                return 0f;
            }

            // Context-aware dodge (e.g. the "Figa de Guiné" hunting charm); never without an attacker.
            if (CombatModifierHooks.ShouldDodge(gameObject, source))
            {
                return 0f;
            }

            // Context-aware protection (hunting charms / prey against a creature type); 1 when no hook is installed.
            amount *= CombatModifierHooks.ResolveDamageTakenMultiplier(gameObject, source);
            amount *= DefenseMultiplier;

            SetHealth(CurrentHealth - amount);
            GrantInvulnerability(_invulnerabilityDuration);

            EventBus.Publish(new DamageAppliedEvent(gameObject, source, amount, element, isCritical));

            if (CurrentHealth <= 0f)
            {
                Die(source);
            }

            return amount;
        }

        public void Heal(float amount)
        {
            if (IsDead)
            {
                return;
            }

            SetHealth(CurrentHealth + amount);
        }

        void ICombatTarget.ApplyDamage(float amount, ElementType element, GameObject source)
        {
            ApplyResistedDamage(amount, element, source);
        }

        /// <summary>
        /// A melee hit (every enemy attack, see <see cref="IMeleeDamageTarget"/>): same mitigation as
        /// <see cref="ICombatTarget.ApplyDamage"/>, then <see cref="ThornsRules.TryReflect"/> sends
        /// Σ Thorns × the damage actually taken back to <paramref name="attacker"/> — even when the hit
        /// was lethal (the spines are still there). Returns the damage taken.
        /// </summary>
        public float ApplyMeleeDamage(float amount, ElementType element, GameObject attacker)
        {
            float taken = ApplyResistedDamage(amount, element, attacker);
            ThornsRules.TryReflect(gameObject, attacker, taken);
            return taken;
        }

        private float ApplyResistedDamage(float amount, ElementType element, GameObject source)
        {
            // Applies the optional ElementResistance table (see Gameplay/Combat/ElementResistance.cs)
            // as a simple percentage multiplier; Defense (DamageCalculator's armor curve + percent
            // modifiers) is applied afterwards in ApplyDamageInternal, for every damage entry point.
            if (_elementResistance != null)
            {
                float resistanceFraction = _elementResistance.GetResistance(element);

                if (resistanceFraction >= 1f)
                {
                    return 0f;
                }

                amount *= (1f - resistanceFraction);
            }

            return ApplyDamageInternal(amount, element, source, isCritical: false);
        }

        void ICombatTarget.ApplyHealing(float amount, GameObject source)
        {
            Heal(amount);
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (IsAwaitingCarriedState())
            {
                _experienceAwaitingCarriedRestore += amount;
            }

            ApplyExperience(amount);
        }

        /// <summary>
        /// Death penalty (<c>DeathXpLossRules</c>): removes up to <paramref name="amount"/> XP from the current level only —
        /// never below 0, so the level never goes down. Returns the XP actually removed.
        /// </summary>
        public int RemoveExperience(int amount)
        {
            int removed = Mathf.Clamp(amount, 0, CurrentExperience);
            if (removed <= 0)
            {
                return 0;
            }

            CurrentExperience -= removed;
            EventBus.Publish(new PlayerExperienceChangedEvent(CurrentExperience, GetExperienceToNextLevel(), Level));
            return removed;
        }

        /// <summary>
        /// True while the SaveManager still holds carried state for this save key, i.e. a scene hop
        /// happened and <c>SaveManager.RestoreCarriedState</c> hasn't applied it to this fresh instance
        /// yet (inside that restore the entry is only dropped after <see cref="RestoreState"/> returns).
        /// Never true during a slot <c>Load</c>: it clears the carry and applies registered saveables directly.
        /// </summary>
        private bool IsAwaitingCarriedState()
        {
            return ServiceLocator.TryGet(out SaveManager saveManager) && saveManager != null && saveManager.HasCarriedState(SaveKey);
        }

        private void ApplyExperience(int amount)
        {
            CurrentExperience += amount;
            int levelBefore = Level;
            int experienceToNextLevel = GetExperienceToNextLevel();

            while (CurrentExperience >= experienceToNextLevel)
            {
                CurrentExperience -= experienceToNextLevel;
                Level++;
                experienceToNextLevel = GetExperienceToNextLevel();
            }

            if (Level != levelBefore)
            {
                PublishVitalsIfCurveGrew(levelBefore);
                RefillVitalsOnLevelUp();
            }

            EventBus.Publish(new PlayerExperienceChangedEvent(CurrentExperience, experienceToNextLevel, Level));
        }

        public int GetExperienceToNextLevel()
        {
            if (Curve != null)
            {
                return Curve.GetExperienceToNextLevel(Level);
            }

            return Mathf.RoundToInt(_baseExperienceToLevel * Mathf.Pow(_experienceCurveMultiplier, Level - 1));
        }

        /// <summary>Refreshes the vitals bars only when the curve actually raised a max on this level-up (no-op with the default flat curve).</summary>
        private void PublishVitalsIfCurveGrew(int levelBefore)
        {
            if (Curve == null)
            {
                return;
            }

            // Health/mana go through the same reconciliation as buff changes (no-op if the max didn't move).
            RefreshVitalMaximums();

            if (!Mathf.Approximately(Curve.GetMaxStamina(levelBefore), Curve.GetMaxStamina(Level)))
            {
                SetStamina(CurrentStamina);
            }
        }

        /// <summary>
        /// Usual RPG reward: a level-up fully restores health and mana (at the new, possibly larger,
        /// maximums already reconciled by <see cref="RefreshVitalMaximums"/>). Skipped while dead so
        /// XP granted on the killing blow can't revive the player behind the death screen.
        /// </summary>
        private void RefillVitalsOnLevelUp()
        {
            if (IsDead)
            {
                return;
            }

            _lastMaxHealth = MaxHealth;
            _lastMaxMana = MaxMana;
            SetHealth(MaxHealth);
            SetMana(MaxMana);
        }

        private void SetHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0f, MaxHealth);
            EventBus.Publish(new PlayerHealthChangedEvent(CurrentHealth, MaxHealth));
        }

        private void SetStamina(float value)
        {
            CurrentStamina = Mathf.Clamp(value, 0f, MaxStamina);
            EventBus.Publish(new PlayerStaminaChangedEvent(CurrentStamina, MaxStamina));
        }

        private void Die(GameObject killer)
        {
            IsDead = true;
            EventBus.Publish(new PlayerDiedEvent());

            // Also raised as a generic combat event (in addition to the player-specific one above)
            // so systems that already key off CombatTargetDiedEvent for any other actor — death
            // SFX in AudioManager, kill tracking in QuestManager/AnalyticsManager — behave the same
            // way when the target happens to be the player, instead of silently doing nothing.
            EventBus.Publish(new CombatTargetDiedEvent(gameObject, killer));
        }

        /// <summary>
        /// Fully revives the player at max health/stamina, clearing <see cref="IsDead"/> and any
        /// lingering invulnerability window. Nothing called this before — <see cref="Die"/> set
        /// <see cref="IsDead"/> permanently true with no way to clear it anywhere in the codebase, so
        /// once a player died in this scene every future hit was silently ignored
        /// (<see cref="ApplyDamageInternal"/>'s <c>if (IsDead ...) return;</c> guard) and the run was
        /// permanently soft-locked. Called by <see cref="Rapadura.Gameplay.World.CheckpointManager"/>
        /// after respawn-positioning the player.
        /// </summary>
        public void Revive()
        {
            IsDead = false;
            ClearInvulnerability();
            SetHealth(MaxHealth);
            SetStamina(MaxStamina);
        }

        public object CaptureState()
        {
            return new PlayerStatsSaveState
            {
                currentHealth = CurrentHealth,
                currentStamina = CurrentStamina,
                currentMana = CurrentMana,
                currentExperience = CurrentExperience,
                level = Level
            };
        }

        public void RestoreState(object state)
        {
            if (state is not PlayerStatsSaveState saveState)
            {
                return;
            }

            // Carried restore (scene hop): keep XP earned since this instance spawned. Slot load: the file wins.
            int experienceEarnedBeforeRestore = IsAwaitingCarriedState() ? _experienceAwaitingCarriedRestore : 0;
            _experienceAwaitingCarriedRestore = 0;

            CurrentHealth = saveState.currentHealth;
            CurrentStamina = saveState.currentStamina;
            CurrentMana = saveState.currentMana;
            CurrentExperience = saveState.currentExperience;
            Level = saveState.level;
            IsDead = CurrentHealth <= 0f;
            _lastMaxHealth = MaxHealth;
            _lastMaxMana = MaxMana;

            EventBus.Publish(new PlayerHealthChangedEvent(CurrentHealth, MaxHealth));
            EventBus.Publish(new PlayerStaminaChangedEvent(CurrentStamina, MaxStamina));
            EventBus.Publish(new PlayerManaChangedEvent(CurrentMana, MaxMana));
            EventBus.Publish(new PlayerExperienceChangedEvent(CurrentExperience, GetExperienceToNextLevel(), Level));

            if (experienceEarnedBeforeRestore > 0)
            {
                ApplyExperience(experienceEarnedBeforeRestore);
            }
        }
    }
}
