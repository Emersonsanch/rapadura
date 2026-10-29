using System.Collections.Generic;

namespace Rapadura.Gameplay.Tutorial
{
    /// <summary>Where a tutorial hint may be shown (see <see cref="TutorialManager.CurrentHint"/>).</summary>
    public enum TutorialContext
    {
        /// <summary>Anywhere in gameplay (world map or encounter) — never on the title / hero screens.</summary>
        Any = 0,
        /// <summary>Only while walking the world map.</summary>
        World = 1,
        /// <summary>
        /// Only inside an encounter (Main.unity). The hint stays hidden while the intro banner
        /// ("Prepare-se!" countdown or boss banner) is on screen and shows once it is gone
        /// (see <c>TutorialHintController.ShouldDefer</c>).
        /// </summary>
        Encounter = 2,
    }

    /// <summary>
    /// One first-time hint ("dica"). Plain data, built in code by <see cref="TutorialSteps"/> (no
    /// ScriptableObject: the list is short, fixed and only ever edited together with the code that
    /// triggers it). Texts are localization keys resolved by <c>Rapadura.UI.Tutorial.TutorialTexts</c>.
    /// </summary>
    public sealed class TutorialStepDefinition
    {
        /// <summary>Stable id, persisted in the tutorial profile file — never rename one that shipped.</summary>
        public string Id { get; }

        public string TitleKey { get; }
        public string BodyKey { get; }

        /// <summary>Optional HUD element name (HudView.uxml) to outline while the hint is up, e.g. "skill-slot-0".</summary>
        public string HighlightElement { get; }

        public TutorialContext Context { get; }

        /// <summary>
        /// Freezes gameplay (GameplayPause) while the hint is on screen, until "Entendi". Supported by
        /// the hint UI but off for every built-in step: hints are meant to be non-blocking, and a
        /// pause on the first enemy spawn would also stall the PlayMode smoke tests (fresh profile
        /// per run) that wait for gameplay to run.
        /// </summary>
        public bool PausesGameplay { get; }

        /// <summary>Position in <see cref="TutorialSteps.All"/>: when several hints are pending, the lowest is shown first.</summary>
        public int Order { get; }

        /// <summary>
        /// Jumps the queue: shown ahead of any older pending hint as soon as it triggers (the death hint
        /// must be read on the death screen, not after the still-pending encounter tips).
        /// </summary>
        public bool Interrupts { get; }

        public TutorialStepDefinition(string id, int order, TutorialContext context, string highlightElement = null, bool pausesGameplay = false, bool interrupts = false)
        {
            Id = id;
            Order = order;
            Interrupts = interrupts;
            Context = context;
            HighlightElement = highlightElement;
            PausesGameplay = pausesGameplay;
            TitleKey = TutorialSteps.TitleKey(id);
            BodyKey = TutorialSteps.BodyKey(id);
        }
    }

    /// <summary>The built-in onboarding steps, in presentation order, with the events that trigger them (see <see cref="TutorialManager"/>).</summary>
    public static class TutorialSteps
    {
        public const string KeyPrefix = "ui.tutorial.";

        /// <summary>First time on the world map (GameFlowState.World): press "Explorar" (outlined), then travel.</summary>
        public const string WorldMap = "world_map";
        /// <summary>First enemy spawned in an encounter: move/attack.</summary>
        public const string Encounter = "encounter";
        /// <summary>First skill learned/equipped: the hotbar.</summary>
        public const string Skills = "skills";
        /// <summary>First item added to the inventory after defeating an enemy: the Mochila.</summary>
        public const string Loot = "loot";
        /// <summary>First quest started: Missões.</summary>
        public const string Quest = "quest";
        /// <summary>
        /// First time at level 2+: attribute/skill points (outlines the HUD "Ficha" button, where attribute
        /// points are spent). Encounter-only: the world map's Ficha is a read-only summary (the
        /// AttributeSet lives on the Player in Main.unity), so the tip waits for the next fight, where
        /// "Ficha" can actually spend the points.
        /// </summary>
        public const string LevelUp = "level_up";
        /// <summary>First player death: the death screen.</summary>
        public const string Death = "death";
        /// <summary>First night (TimeOfDayPhase.Night) during gameplay.</summary>
        public const string Night = "night";
        /// <summary>First time a known recipe has all its ingredients in the inventory: crafting / bancada.</summary>
        public const string Crafting = "crafting";
        /// <summary>
        /// First time the world-map token stands idle in a region whose quest-giver can be talked to
        /// (the HUD's "Conversar com…" button is on screen; outlines it). Reported by the hint UI
        /// through <see cref="TutorialManager.NotifyWorldMapIdle"/>.
        /// </summary>
        public const string RegionNpc = "region_npc";
        /// <summary>First time idle in the Floresta with the "Vila" button on screen, after the prologue (outlines it).</summary>
        public const string Village = "village";
        /// <summary>First biome material dropped by an enemy (EnemyLootDroppedEvent): what materials are for.</summary>
        public const string Materials = "materials";
        /// <summary>First stun the hero lands on an enemy (StunAppliedEvent on an EnemyController).</summary>
        public const string Stun = "stun";

        /// <summary>World-map HUD element names outlined by <see cref="RegionNpc"/>/<see cref="Village"/> (RegionNpcTalkButton.ButtonName / VillageHubButton.ButtonName).</summary>
        public const string TalkButtonElement = "wm-talk-button";
        public const string VillageButtonElement = "wm-village-button";

        public static string TitleKey(string stepId) => KeyPrefix + stepId + ".title";
        public static string BodyKey(string stepId) => KeyPrefix + stepId + ".body";

        public static readonly IReadOnlyList<TutorialStepDefinition> All = new List<TutorialStepDefinition>
        {
            new TutorialStepDefinition(WorldMap, 0, TutorialContext.World, "wm-explore-button"),
            new TutorialStepDefinition(Encounter, 1, TutorialContext.Encounter),
            new TutorialStepDefinition(Skills, 2, TutorialContext.Encounter, "skill-slot-0"),
            new TutorialStepDefinition(Loot, 3, TutorialContext.Any, "inventory-button"),
            new TutorialStepDefinition(Quest, 4, TutorialContext.Any, "quests-button"),
            new TutorialStepDefinition(LevelUp, 5, TutorialContext.Encounter, "character-sheet-button"),
            new TutorialStepDefinition(Death, 6, TutorialContext.Any, interrupts: true),
            new TutorialStepDefinition(Night, 7, TutorialContext.Any),
            new TutorialStepDefinition(Crafting, 8, TutorialContext.Any),
            // Appended (2026-10-29): ids and orders of the steps above are persisted / relied on.
            new TutorialStepDefinition(RegionNpc, 9, TutorialContext.World, TalkButtonElement),
            new TutorialStepDefinition(Village, 10, TutorialContext.World, VillageButtonElement),
            new TutorialStepDefinition(Materials, 11, TutorialContext.Any, "inventory-button"),
            new TutorialStepDefinition(Stun, 12, TutorialContext.Encounter),
        };
    }
}
