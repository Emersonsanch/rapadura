using System;
using Rapadura.Core.EventBus;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Skills;
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
    public class PlayerStats : MonoBehaviour, ISaveable, ICombatTarget, ISkillResourceProvider
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

        [Header("Experience")]
        [SerializeField] private int _baseExperienceToLevel = 100;
        [SerializeField] private float _experienceCurveMultiplier = 1.25f;

        [Header("Invulnerability")]
        [Tooltip("Seconds of damage immunity granted right after taking a hit.")]
        [SerializeField] private float _invulnerabilityDuration = 0.5f;

        private float _invulnerabilityTimer;

        public float CurrentHealth { get; private set; }
        public float CurrentStamina { get; private set; }
        public float CurrentMana { get; private set; }
        public float MaxMana => _maxMana;
        public int CurrentExperience { get; private set; }
        public int Level { get; private set; } = 1;
        public bool IsDead { get; private set; }

        public bool IsAlive => !IsDead;
        public bool IsInvulnerable => _invulnerabilityTimer > 0f;
        public Transform Transform => transform;

        public string SaveKey => "PlayerStats";

        private void Awake()
        {
            CurrentHealth = _maxHealth;
            CurrentStamina = _maxStamina;
            CurrentMana = _maxMana;
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
            if (CurrentMana < _maxMana)
            {
                SetMana(CurrentMana + _manaRegenPerSecond * deltaTime);
            }
        }

        public void RestoreMana(float amount)
        {
            SetMana(CurrentMana + amount);
        }

        private void SetMana(float value)
        {
            CurrentMana = Mathf.Clamp(value, 0f, _maxMana);
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
            if (CurrentStamina < _maxStamina)
            {
                SetStamina(CurrentStamina + _staminaRegenPerSecond * deltaTime);
            }
        }

        public void RestoreStamina(float amount)
        {
            SetStamina(CurrentStamina + amount);
        }

        private void RegenerateHealth(float deltaTime)
        {
            if (!IsDead && CurrentHealth < _maxHealth)
            {
                SetHealth(CurrentHealth + _healthRegenPerSecond * deltaTime);
            }
        }

        public void ApplyDamage(float amount)
        {
            if (IsDead || IsInvulnerable || amount <= 0f)
            {
                return;
            }

            SetHealth(CurrentHealth - amount);
            GrantInvulnerability(_invulnerabilityDuration);

            if (CurrentHealth <= 0f)
            {
                Die();
            }
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
            // Element-specific resistance/weakness calculation is intentionally left to a future
            // damage-formula system; for now every element deals its raw amount.
            ApplyDamage(amount);
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

            CurrentExperience += amount;
            int experienceToNextLevel = GetExperienceToNextLevel();

            while (CurrentExperience >= experienceToNextLevel)
            {
                CurrentExperience -= experienceToNextLevel;
                Level++;
                experienceToNextLevel = GetExperienceToNextLevel();
            }

            EventBus.Publish(new PlayerExperienceChangedEvent(CurrentExperience, experienceToNextLevel, Level));
        }

        public int GetExperienceToNextLevel()
        {
            return Mathf.RoundToInt(_baseExperienceToLevel * Mathf.Pow(_experienceCurveMultiplier, Level - 1));
        }

        private void SetHealth(float value)
        {
            CurrentHealth = Mathf.Clamp(value, 0f, _maxHealth);
            EventBus.Publish(new PlayerHealthChangedEvent(CurrentHealth, _maxHealth));
        }

        private void SetStamina(float value)
        {
            CurrentStamina = Mathf.Clamp(value, 0f, _maxStamina);
            EventBus.Publish(new PlayerStaminaChangedEvent(CurrentStamina, _maxStamina));
        }

        private void Die()
        {
            IsDead = true;
            EventBus.Publish(new PlayerDiedEvent());
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

            CurrentHealth = saveState.currentHealth;
            CurrentStamina = saveState.currentStamina;
            CurrentMana = saveState.currentMana;
            CurrentExperience = saveState.currentExperience;
            Level = saveState.level;
            IsDead = CurrentHealth <= 0f;

            EventBus.Publish(new PlayerHealthChangedEvent(CurrentHealth, _maxHealth));
            EventBus.Publish(new PlayerStaminaChangedEvent(CurrentStamina, _maxStamina));
            EventBus.Publish(new PlayerExperienceChangedEvent(CurrentExperience, GetExperienceToNextLevel(), Level));
        }
    }
}
