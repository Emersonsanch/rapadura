namespace Rapadura.Audio
{
    /// <summary>
    /// Gameplay happenings that can trigger a sound. <see cref="SfxEventRouter"/> translates EventBus
    /// events into these (plus an optional lower-case variant such as an element or a weather name)
    /// and <see cref="SfxEventMap"/> turns the pair into a cue id. Values are explicit because the
    /// map asset serializes them as ints — append new members, never renumber.
    /// </summary>
    public enum SfxTrigger
    {
        Hit = 0,
        CritHit = 1,
        PlayerHurt = 2,
        EnemyDeath = 3,
        BossDeath = 4,
        PlayerDeath = 5,
        LevelUp = 6,
        /// <summary>Variant: element ("fire"), then SkillVfxCategory ("heal"); "ultimate" wins over the element.</summary>
        SkillCast = 7,
        ItemPickup = 8,
        ItemPurchased = 9,
        ItemSold = 10,
        RecipeCrafted = 11,
        QuestStarted = 12,
        QuestCompleted = 13,
        AchievementUnlocked = 14,
        UiOpen = 15,
        UiClose = 16,
        UiClick = 17,
        UiHover = 18,
        UiError = 19,
        /// <summary>Variant: biome ("forest").</summary>
        EncounterStart = 20,
        EncounterVictory = 21,
        EncounterDefeat = 22,
        EncounterFled = 23,
        /// <summary>Variant: phase ("dawn", "day", "dusk", "night") — a one-shot ambience sting.</summary>
        TimeOfDayPhase = 24,
        /// <summary>Variant: weather ("rain", "sandstorm", "snow", "fog", "clear") — a looping ambience bed.</summary>
        WeatherLoop = 25,
    }

    /// <summary>Music state chosen by <see cref="MusicDirector"/>. Serialized as int — append only.</summary>
    public enum MusicContext
    {
        None = 0,
        Title = 1,
        WorldMap = 2,
        /// <summary>Variant: biome ("forest", "desert", "mountain", "caves").</summary>
        Encounter = 3,
        Boss = 4,
    }

    /// <summary>Shape of the placeholder sound <see cref="ProceduralSfxSynth"/> bakes when a cue has no clip. Serialized as int — append only.</summary>
    public enum ProceduralSfxPreset
    {
        None = 0,
        Click = 1,
        Blip = 2,
        BlipDown = 3,
        Thud = 4,
        Crunch = 5,
        Zap = 6,
        Buzz = 7,
        Whoosh = 8,
        Chime = 9,
        ChimeDown = 10,
        Fanfare = 11,
        Sparkle = 12,
        Boom = 13,
        Swell = 14,
        Gong = 15,
        Chirp = 16,
        Cricket = 17,
        RainLoop = 18,
        WindLoop = 19,
    }
}
