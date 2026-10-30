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
        /// <summary>A stun landed on someone (<c>StunAppliedEvent</c>).</summary>
        Stun = 26,
        /// <summary>A kill's loot went into the bag (<c>EnemyLootDroppedEvent</c>); replaces <see cref="ItemPickup"/> for that kill.</summary>
        LootDrop = 27,
        /// <summary>Part of a kill's loot did not fit (<c>LootOverflowEvent</c>, "Mochila cheia").</summary>
        BagFull = 28,
        /// <summary>A story beat starts (first panel).</summary>
        StoryOpen = 29,
        /// <summary>Next story panel.</summary>
        StoryPage = 30,
        /// <summary>"Pular": the beat was skipped.</summary>
        StorySkip = 31,
        /// <summary>Last panel read to the end (not the ending).</summary>
        StoryClose = 32,
        /// <summary>The ending beat ended and the credits roll.</summary>
        StoryToCredits = 33,
        /// <summary>The campaign was completed (<c>CampaignCompletedEvent</c>), played a moment after the victory sting.</summary>
        CampaignCompleted = 34,
        /// <summary>The village hub (Vila) panel opened.</summary>
        VillageOpen = 35,
        /// <summary>The village hub panel closed.</summary>
        VillageClose = 36,
        /// <summary>A conversation started (region NPC, village, Main.unity NPCs: <c>DialogueStartedEvent</c>).</summary>
        NpcTalk = 37,
        /// <summary>Espinhos reflected melee damage back (replaces the plain hit sound of that reflected hit).</summary>
        Thorns = 38,
        /// <summary>An enemy starts its attack windup (anticipation crouch). Subtle.</summary>
        EnemyWindup = 39,
        /// <summary>The hero lands from a fall/jump hard enough to squash. Subtle.</summary>
        HeroLanding = 40,
        /// <summary>Looping countryside bed under the title music (main menu / hero select).</summary>
        TitleAmbience = 41,
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
        /// <summary>Story panels and the credits after the ending. Variant: "ending" for the epilogue.</summary>
        Story = 5,
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
        /// <summary>Wobbly falling tone ("seeing stars").</summary>
        Dizzy = 20,
        /// <summary>Two quick bright plucks, like coins dropped on wood.</summary>
        Clink = 21,
        /// <summary>Hollow double knock on wood (lid shutting, latch).</summary>
        WoodKnock = 22,
        /// <summary>Wooden door/chest creak ending in a soft thunk.</summary>
        WoodCreak = 23,
        /// <summary>Paper rustle / page turn (band-passed noise crinkles).</summary>
        PageRustle = 24,
        /// <summary>Short forró flourish: zabumba thumps + triângulo (muted/open) + a major arpeggio.</summary>
        ForroFlourish = 25,
        /// <summary>Soft "hm-hm" voice-like formant pair.</summary>
        Murmur = 26,
        /// <summary>Thin metallic prick + scrape (thorns).</summary>
        Prick = 27,
        /// <summary>Short rising breath / cloth gather (anticipation).</summary>
        Inhale = 28,
        /// <summary>Muffled sack thump with a cloth rustle (bag full).</summary>
        SackThump = 29,
        /// <summary>Loop: crickets over a soft evening breeze.</summary>
        CountrysideLoop = 30,
        /// <summary>One short dog "woof" (falling voiced tone + breathy onset); higher frequency = a happy yip.</summary>
        Bark = 31,
    }
}
