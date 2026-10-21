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
        /// <summary>Only inside an encounter (Main.unity).</summary>
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

        public TutorialStepDefinition(string id, int order, TutorialContext context, string highlightElement = null, bool pausesGameplay = false)
        {
            Id = id;
            Order = order;
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

        /// <summary>First time on the world map (GameFlowState.World): how to travel.</summary>
        public const string WorldMap = "world_map";
        /// <summary>First enemy spawned in an encounter: move/attack.</summary>
        public const string Encounter = "encounter";
        /// <summary>First skill learned/equipped: the hotbar.</summary>
        public const string Skills = "skills";
        /// <summary>First item added to the inventory after defeating an enemy: the Mochila.</summary>
        public const string Loot = "loot";
        /// <summary>First quest started: Missões.</summary>
        public const string Quest = "quest";
        /// <summary>First time at level 2+: attribute/skill points.</summary>
        public const string LevelUp = "level_up";
        /// <summary>First player death: the death screen.</summary>
        public const string Death = "death";
        /// <summary>First night (TimeOfDayPhase.Night) during gameplay.</summary>
        public const string Night = "night";
        /// <summary>First time a known recipe has all its ingredients in the inventory: crafting / bancada.</summary>
        public const string Crafting = "crafting";

        public static string TitleKey(string stepId) => KeyPrefix + stepId + ".title";
        public static string BodyKey(string stepId) => KeyPrefix + stepId + ".body";

        public static readonly IReadOnlyList<TutorialStepDefinition> All = new List<TutorialStepDefinition>
        {
            new TutorialStepDefinition(WorldMap, 0, TutorialContext.World),
            new TutorialStepDefinition(Encounter, 1, TutorialContext.Encounter),
            new TutorialStepDefinition(Skills, 2, TutorialContext.Encounter, "skill-slot-0"),
            new TutorialStepDefinition(Loot, 3, TutorialContext.Any, "inventory-button"),
            new TutorialStepDefinition(Quest, 4, TutorialContext.Any, "quests-button"),
            new TutorialStepDefinition(LevelUp, 5, TutorialContext.Any, "skills-button"),
            new TutorialStepDefinition(Death, 6, TutorialContext.Any),
            new TutorialStepDefinition(Night, 7, TutorialContext.Any),
            new TutorialStepDefinition(Crafting, 8, TutorialContext.Any),
        };
    }
}
