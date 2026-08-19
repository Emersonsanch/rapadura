using UnityEngine;

namespace Rapadura.Core.EventBus
{
    /// <summary>Raised whenever the player's health value changes.</summary>
    public readonly struct PlayerHealthChangedEvent : IGameEvent
    {
        public readonly float CurrentHealth;
        public readonly float MaxHealth;

        public PlayerHealthChangedEvent(float currentHealth, float maxHealth)
        {
            CurrentHealth = currentHealth;
            MaxHealth = maxHealth;
        }
    }

    /// <summary>Raised whenever the player's stamina/energy value changes.</summary>
    public readonly struct PlayerStaminaChangedEvent : IGameEvent
    {
        public readonly float CurrentStamina;
        public readonly float MaxStamina;

        public PlayerStaminaChangedEvent(float currentStamina, float maxStamina)
        {
            CurrentStamina = currentStamina;
            MaxStamina = maxStamina;
        }
    }

    /// <summary>Raised whenever the player's mana value changes.</summary>
    public readonly struct PlayerManaChangedEvent : IGameEvent
    {
        public readonly float CurrentMana;
        public readonly float MaxMana;

        public PlayerManaChangedEvent(float currentMana, float maxMana)
        {
            CurrentMana = currentMana;
            MaxMana = maxMana;
        }
    }

    /// <summary>Raised whenever the player gains experience or levels up.</summary>
    public readonly struct PlayerExperienceChangedEvent : IGameEvent
    {
        public readonly int CurrentExperience;
        public readonly int ExperienceToNextLevel;
        public readonly int Level;

        public PlayerExperienceChangedEvent(int currentExperience, int experienceToNextLevel, int level)
        {
            CurrentExperience = currentExperience;
            ExperienceToNextLevel = experienceToNextLevel;
            Level = level;
        }
    }

    /// <summary>Raised when the player dies.</summary>
    public readonly struct PlayerDiedEvent : IGameEvent
    {
    }

    /// <summary>Raised right before a save operation writes data to disk, so systems can flush state.</summary>
    public readonly struct GameSavingEvent : IGameEvent
    {
    }

    /// <summary>Raised right after a load operation reads data from disk, so systems can apply state.</summary>
    public readonly struct GameLoadedEvent : IGameEvent
    {
    }

    /// <summary>Raised whenever an item is added to or removed from the inventory.</summary>
    public readonly struct InventoryChangedEvent : IGameEvent
    {
    }
}
