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

        /// <summary>Every page of "Caderno da Vó Edna" unlocked (<c>BestiaryCompletedEvent</c> from <c>BestiaryProgress</c>).</summary>
        BestiaryCompleted = 10,

        /// <summary>Highest "Nova Jornada+" cycle finished (<c>NewGamePlusCycleCompletedEvent.Cycle</c> from <c>NewGamePlusProgress</c>; max, not a sum).</summary>
        NewGamePlusCompleted = 11,

        /// <summary>
        /// "Lembranças do Vô Jorge" collected (fed directly by <c>SecretsProgress.AddRelic</c> through
        /// <see cref="AchievementManager.RecordProgress"/>, distinct ids). Target ids = relic ids. Kept apart
        /// from the values above so features added in parallel can take the next numbers without clashing.
        /// </summary>
        RelicsCollected = 20,

        /// <summary>
        /// Fish species landed in "Pesca no açude" (fed directly by <c>FishingProgress.RecordCatch</c> through
        /// <see cref="AchievementManager.RecordProgress"/>, distinct ids). Target ids = fish item ids (<c>FishCatalog</c>).
        /// </summary>
        FishSpeciesCaught = 30,

        /// <summary>
        /// Timed event rooms cleared (fed by <c>EventRoomProgress.RecordRun</c>, distinct ids). Target ids = room ids
        /// (<c>terreiro</c>, <c>torre</c>).
        /// </summary>
        EventRoomCleared = 50,

        /// <summary>Best score in any timed event room (<c>EventRoomProgress</c>, through <see cref="AchievementManager.RecordMax"/>).</summary>
        EventRoomBestScore = 51,

        /// <summary>
        /// Wings made in the Tacho Encantado (fed by <c>TachoService</c> through <see cref="AchievementManager.RecordProgress"/>
        /// on every successful "Asas nível 1"). Target value = how many.
        /// </summary>
        WingsCrafted = 60,

        /// <summary>Highest wing level reached in the Tacho Encantado (<c>TachoService</c>, through <see cref="AchievementManager.RecordMax"/>).</summary>
        WingLevelReached = 61,
    }
}
