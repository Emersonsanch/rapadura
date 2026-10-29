namespace Rapadura.Gameplay.Achievements
{
    /// <summary>
    /// What an <see cref="AchievementDefinition"/> measures. Each value maps to one gameplay event
    /// that <see cref="AchievementManager"/> listens to on the EventBus (see the per-value notes).
    /// Values are serialized as ints in the .asset files, so never renumber existing entries.
    /// </summary>
    public enum AchievementConditionType
    {
        /// <summary>Total enemies killed (<c>EnemyDiedEvent</c>, bosses included).</summary>
        EnemiesKilled = 0,

        /// <summary>Bosses defeated (<c>EnemyDiedEvent</c> with <c>WasBoss</c>). Target ids = EnemyDefinition asset names.</summary>
        BossesDefeated = 1,

        /// <summary>Quests completed (<c>QuestCompletedEvent</c>). Target ids = specific quest ids that must all be completed.</summary>
        QuestsCompleted = 2,

        /// <summary>Highest player level reached (<c>PlayerExperienceChangedEvent.Level</c>).</summary>
        LevelReached = 3,

        /// <summary>Recipes crafted (<c>RecipeCraftedEvent</c>). Target ids = recipe ids.</summary>
        ItemsCrafted = 4,

        /// <summary>Skills cast by the player (<c>SkillCastEvent</c> from a "Player"-tagged caster). Target ids = skill asset names.</summary>
        SkillsCast = 5,

        /// <summary>World map regions unlocked (<c>WorldMapRegionUnlockedEvent</c>). Target ids = <c>BiomeType</c> names.</summary>
        RegionsUnlocked = 6,

        /// <summary>Gold earned from selling to shops (<c>ItemSoldEvent.TotalPayout</c>).</summary>
        GoldEarned = 7,

        /// <summary>Skill combos completed by the player (<c>ComboCompletedEvent</c>). Target ids = combo ids.</summary>
        CombosCompleted = 8,

        /// <summary>Campaign finished — the final region boss beaten (<c>CampaignCompletedEvent</c> from <c>StoryProgress</c>).</summary>
        CampaignCompleted = 9,
    }
}
