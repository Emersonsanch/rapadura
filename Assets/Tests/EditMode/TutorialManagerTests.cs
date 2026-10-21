using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Tutorial;
using Rapadura.UI.Tutorial;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// <see cref="TutorialManager"/>: every step fires once from its EventBus trigger, hints are held
    /// back on the title/hero screens and by context, progress + "Mostrar dicas" persist per profile
    /// (Saves/tutorial.json under <see cref="TestSaveRoot"/>) and survive "Novo jogo", and
    /// "Reiniciar tutorial" brings everything back. Also the settings section and the texts.
    /// </summary>
    public class TutorialManagerTests
    {
        private readonly List<TutorialManager> _managers = new List<TutorialManager>();
        private readonly List<Object> _created = new List<Object>();
        private bool _craftable;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            _craftable = false;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (TutorialManager manager in _managers)
            {
                manager.Shutdown();
            }

            _managers.Clear();
            EventBus.Clear();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            TestSaveRoot.Exit();
        }

        /// <summary>Runtime manager (built-in steps, profile file inside TestSaveRoot), crafting probe driven by <see cref="_craftable"/>.</summary>
        private TutorialManager CreateManager(GameFlowState state = GameFlowState.Boot)
        {
            var manager = new TutorialManager { CraftableRecipeProbe = () => _craftable };
            _managers.Add(manager);
            manager.Initialize();
            SetFlow(state);
            return manager;
        }

        private static void SetFlow(GameFlowState state) =>
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.Boot, state));

        private SkillDefinition CreateSkill()
        {
            var skill = ScriptableObject.CreateInstance<SkillDefinition>();
            _created.Add(skill);
            return skill;
        }

        private void Shutdown(TutorialManager manager)
        {
            manager.Shutdown();
            _managers.Remove(manager);
        }

        // ------------------------------------------------------------------
        // Triggers
        // ------------------------------------------------------------------

        private static IEnumerable<TestCaseData> StepTriggers()
        {
            yield return new TestCaseData(TutorialSteps.Encounter, (System.Action<TutorialManagerTests>)(t => EventBus.Publish(new EnemySpawnedEvent(null)))).SetName("Encounter_EnemySpawned");
            yield return new TestCaseData(TutorialSteps.Skills, (System.Action<TutorialManagerTests>)(t => EventBus.Publish(new SkillLearnedEvent(t.CreateSkill())))).SetName("Skills_SkillLearned");
            yield return new TestCaseData(TutorialSteps.Loot, (System.Action<TutorialManagerTests>)(t =>
            {
                EventBus.Publish(new EnemyDiedEvent(null, false));
                EventBus.Publish(new InventoryChangedEvent());
            })).SetName("Loot_InventoryChangedAfterKill");
            yield return new TestCaseData(TutorialSteps.Quest, (System.Action<TutorialManagerTests>)(t => EventBus.Publish(new QuestStartedEvent("quest_test")))).SetName("Quest_QuestStarted");
            yield return new TestCaseData(TutorialSteps.LevelUp, (System.Action<TutorialManagerTests>)(t => EventBus.Publish(new PlayerExperienceChangedEvent(0, 100, 2)))).SetName("LevelUp_Level2");
            yield return new TestCaseData(TutorialSteps.Death, (System.Action<TutorialManagerTests>)(t => EventBus.Publish(new PlayerDiedEvent()))).SetName("Death_PlayerDied");
            yield return new TestCaseData(TutorialSteps.Night, (System.Action<TutorialManagerTests>)(t => EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Dusk, TimeOfDayPhase.Night, 1)))).SetName("Night_PhaseNight");
            yield return new TestCaseData(TutorialSteps.Crafting, (System.Action<TutorialManagerTests>)(t =>
            {
                t._craftable = true;
                EventBus.Publish(new InventoryChangedEvent());
            })).SetName("Crafting_RecipeBecomesCraftable");
            yield return new TestCaseData(TutorialSteps.WorldMap, (System.Action<TutorialManagerTests>)(t => SetFlow(GameFlowState.World))).SetName("WorldMap_FlowWorld");
        }

        [TestCaseSource(nameof(StepTriggers))]
        public void EachStep_FiresOnceFromItsEvent(string stepId, System.Action<TutorialManagerTests> fire)
        {
            TutorialManager manager = CreateManager();
            int changes = 0;
            manager.CurrentHintChanged += () => changes++;

            fire(this);
            Assert.IsTrue(manager.IsPending(stepId), $"{stepId} should be pending after its trigger.");
            Assert.AreEqual(stepId, manager.CurrentHint?.Id);
            Assert.AreEqual(1, changes);

            fire(this);
            Assert.AreEqual(1, manager.Steps.Count(s => manager.IsPending(s.Id) && s.Id == stepId));
            Assert.AreEqual(1, changes, "a second trigger while pending must not re-notify");

            Assert.IsTrue(manager.Acknowledge(stepId));
            Assert.IsTrue(manager.IsCompleted(stepId));
            Assert.IsNull(manager.CurrentHint);

            fire(this);
            Assert.IsFalse(manager.IsPending(stepId), $"{stepId} fired again after being acknowledged.");
            Assert.IsFalse(manager.Trigger(stepId));
            Assert.IsFalse(manager.Acknowledge(stepId), "acknowledging twice reports false");
        }

        [Test]
        public void Loot_IgnoresInventoryChangesBeforeAnyKill()
        {
            TutorialManager manager = CreateManager();
            EventBus.Publish(new InventoryChangedEvent());
            Assert.IsFalse(manager.IsPending(TutorialSteps.Loot), "starting gear is not loot");

            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new NewSessionResetEvent());
            EventBus.Publish(new InventoryChangedEvent());
            Assert.IsFalse(manager.IsPending(TutorialSteps.Loot), "a new session forgets the previous kill");
        }

        [Test]
        public void LevelUp_NotTriggeredAtLevel1()
        {
            TutorialManager manager = CreateManager();
            EventBus.Publish(new PlayerExperienceChangedEvent(10, 100, 1));
            Assert.IsFalse(manager.IsPending(TutorialSteps.LevelUp));
        }

        [Test]
        public void Night_IgnoredOnTitleScreen_AndOtherPhasesIgnored()
        {
            TutorialManager manager = CreateManager(GameFlowState.MainMenu);
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Dusk, TimeOfDayPhase.Night, 1));
            Assert.IsFalse(manager.IsPending(TutorialSteps.Night));

            SetFlow(GameFlowState.World);
            EventBus.Publish(new TimeOfDayPhaseChangedEvent(TimeOfDayPhase.Day, TimeOfDayPhase.Dusk, 1));
            Assert.IsFalse(manager.IsPending(TutorialSteps.Night));
        }

        [Test]
        public void SkillUnequip_DoesNotTrigger()
        {
            TutorialManager manager = CreateManager();
            EventBus.Publish(new SkillEquippedChangedEvent(0, null));
            Assert.IsFalse(manager.IsPending(TutorialSteps.Skills));

            EventBus.Publish(new SkillEquippedChangedEvent(0, CreateSkill()));
            Assert.IsTrue(manager.IsPending(TutorialSteps.Skills));
        }

        // ------------------------------------------------------------------
        // Presentation gating
        // ------------------------------------------------------------------

        [Test]
        public void HintsTriggeredOnFrontEnd_WaitForGameplay()
        {
            TutorialManager manager = CreateManager(GameFlowState.CharacterSelect);
            EventBus.Publish(new QuestStartedEvent("daily"));

            Assert.IsTrue(manager.IsPending(TutorialSteps.Quest));
            Assert.IsNull(manager.CurrentHint, "no hint on the hero screen");

            SetFlow(GameFlowState.World);
            Assert.AreEqual(TutorialSteps.WorldMap, manager.CurrentHint?.Id, "lowest order first");

            manager.Acknowledge(TutorialSteps.WorldMap);
            Assert.AreEqual(TutorialSteps.Quest, manager.CurrentHint?.Id);
        }

        [Test]
        public void EncounterOnlySteps_WaitForAnEncounter()
        {
            TutorialManager manager = CreateManager(GameFlowState.World);
            manager.Acknowledge(TutorialSteps.WorldMap);
            EventBus.Publish(new SkillLearnedEvent(CreateSkill()));

            Assert.IsTrue(manager.IsPending(TutorialSteps.Skills));
            Assert.IsNull(manager.CurrentHint, "hotbar hint is encounter-only");

            SetFlow(GameFlowState.Encounter);
            Assert.AreEqual(TutorialSteps.Skills, manager.CurrentHint?.Id);

            EventBus.Publish(new EnemySpawnedEvent(null));
            Assert.AreEqual(TutorialSteps.Encounter, manager.CurrentHint?.Id, "the move/attack hint goes before the hotbar one");
        }

        [Test]
        public void BuiltInHints_NeverPauseGameplay()
        {
            CollectionAssert.IsEmpty(TutorialSteps.All.Where(s => s.PausesGameplay).Select(s => s.Id).ToArray(),
                "hints are non-blocking (a pause here would also stall the PlayMode smoke tests)");
        }

        // ------------------------------------------------------------------
        // Persistence / settings
        // ------------------------------------------------------------------

        [Test]
        public void Progress_PersistsAcrossSessions_AndSurvivesNewGame()
        {
            TutorialManager first = CreateManager();
            EventBus.Publish(new PlayerDiedEvent());
            first.Acknowledge(TutorialSteps.Death);
            Assert.IsTrue(File.Exists(TutorialManager.ProfileFilePath));
            StringAssert.StartsWith(TestSaveRoot.Current, TutorialManager.ProfileFilePath, "The profile must follow SaveRootOverride.");
            Shutdown(first);

            TutorialManager second = CreateManager();
            Assert.IsTrue(second.IsCompleted(TutorialSteps.Death), "completion reloaded from the profile");

            // "Novo jogo" / loading a slot never resets tutorial progress.
            var reset = new NewSessionReset();
            reset.ResetAll();
            EventBus.Publish(new GameLoadedEvent());
            EventBus.Publish(new PlayerDiedEvent());

            Assert.IsTrue(second.IsCompleted(TutorialSteps.Death));
            Assert.IsFalse(second.IsPending(TutorialSteps.Death));
        }

        [Test]
        public void UnacknowledgedHint_ComesBackNextSession()
        {
            TutorialManager first = CreateManager();
            EventBus.Publish(new PlayerDiedEvent());
            Shutdown(first);

            TutorialManager second = CreateManager();
            Assert.IsFalse(second.IsCompleted(TutorialSteps.Death));
            EventBus.Publish(new PlayerDiedEvent());
            Assert.IsTrue(second.IsPending(TutorialSteps.Death));
        }

        [Test]
        public void DisabledHints_SuppressTriggers_AndPersist()
        {
            TutorialManager manager = CreateManager();
            EventBus.Publish(new QuestStartedEvent("q1"));
            Assert.IsNotNull(manager.CurrentHint);

            manager.SetHintsEnabled(false);
            Assert.IsNull(manager.CurrentHint, "turning hints off hides the current one");
            Assert.IsFalse(manager.IsPending(TutorialSteps.Quest));
            Assert.IsFalse(manager.IsCompleted(TutorialSteps.Quest), "a suppressed hint is not marked done");

            EventBus.Publish(new PlayerDiedEvent());
            _craftable = true;
            EventBus.Publish(new InventoryChangedEvent());
            Assert.IsFalse(manager.Trigger(TutorialSteps.Night));
            Assert.IsNull(manager.CurrentHint);
            Assert.IsFalse(manager.Steps.Any(s => manager.IsPending(s.Id)));

            Shutdown(manager);
            TutorialManager reloaded = CreateManager();
            Assert.IsFalse(reloaded.HintsEnabled, "\"Mostrar dicas\" is per profile");

            reloaded.SetHintsEnabled(true);
            EventBus.Publish(new PlayerDiedEvent());
            Assert.AreEqual(TutorialSteps.Death, reloaded.CurrentHint?.Id);
        }

        [Test]
        public void ResetTutorial_LetsEveryStepFireAgain()
        {
            TutorialManager manager = CreateManager();
            foreach (TutorialStepDefinition step in manager.Steps)
            {
                manager.Trigger(step.Id);
                manager.Acknowledge(step.Id);
            }

            Assert.AreEqual(manager.Steps.Count, manager.CompletedCount);
            manager.SetHintsEnabled(false);

            manager.ResetTutorial();
            Assert.AreEqual(0, manager.CompletedCount);
            Assert.IsTrue(manager.HintsEnabled, "reset turns hints back on");

            Shutdown(manager);
            TutorialManager reloaded = CreateManager();
            Assert.AreEqual(0, reloaded.CompletedCount, "the reset was saved");
            EventBus.Publish(new PlayerDiedEvent());
            Assert.IsTrue(reloaded.IsPending(TutorialSteps.Death));
        }

        [Test]
        public void CorruptProfile_FallsBackToFreshProfile()
        {
            Directory.CreateDirectory(TestSaveRoot.Current);
            File.WriteAllText(TutorialManager.ProfileFilePath, "{ not json");

            TutorialManager manager = CreateManager();
            Assert.IsTrue(manager.HintsEnabled);
            Assert.AreEqual(0, manager.CompletedCount);
        }

        [Test]
        public void CustomStepsConstructor_NeverTouchesTheProfile()
        {
            var manager = new TutorialManager(TutorialSteps.All);
            _managers.Add(manager);
            manager.Initialize();
            SetFlow(GameFlowState.Boot);
            manager.Trigger(TutorialSteps.Death);
            manager.Acknowledge(TutorialSteps.Death);

            Assert.IsFalse(manager.PersistsProfile);
            Assert.IsFalse(File.Exists(TutorialManager.ProfileFilePath));
        }

        [Test]
        public void SettingsSection_ReflectsManagerState()
        {
            TutorialManager manager = CreateManager();
            var panel = new VisualElement();
            var section = new TutorialSettingsSection { Manager = manager };

            section.Attach(panel);
            Assert.IsNotNull(panel.Q<Toggle>(TutorialSettingsSection.ToggleName));
            Assert.IsNotNull(panel.Q<Button>(TutorialSettingsSection.ResetButtonName));
            Assert.IsTrue(section.Toggle.value);
            CollectionAssert.Contains(new[] { "Mostrar dicas", "Show tips" }, section.Toggle.label);

            manager.SetHintsEnabled(false);
            section.Refresh();
            Assert.IsFalse(section.Toggle.value);

            section.Attach(panel);
            Assert.AreEqual(1, panel.Query<Toggle>(TutorialSettingsSection.ToggleName).ToList().Count, "attaching twice adds the controls once");

            section.Detach();
            Assert.IsNull(panel.Q<Toggle>(TutorialSettingsSection.ToggleName));
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void EveryStep_HasTitleAndBodyInBothLanguages()
        {
            Dictionary<string, LocalizationEntry> table = TutorialTexts.Entries.ToDictionary(e => e.Key);
            foreach (TutorialStepDefinition step in TutorialSteps.All)
            {
                foreach (string key in new[] { step.TitleKey, step.BodyKey })
                {
                    Assert.IsTrue(table.TryGetValue(key, out LocalizationEntry entry), $"{key} missing from TutorialTexts");
                    Assert.IsFalse(string.IsNullOrEmpty(entry.En), $"{key} has no en text");
                    Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), $"{key} has no pt-BR text");
                }
            }

            Assert.AreEqual(TutorialSteps.All.Count, TutorialSteps.All.Select(s => s.Id).Distinct().Count(), "step ids must be unique");
        }

        [Test]
        public void Texts_ResolveByLanguageWithPtBRFallback()
        {
            Assert.AreEqual("Entendi!", TutorialTexts.Get(null, TutorialTexts.GotIt));
            Assert.AreEqual("Reiniciar tutorial", TutorialTexts.Get(null, TutorialTexts.SettingsReset));

            var localization = new LocalizationManager();
            localization.Initialize();
            localization.SetLanguage(LanguageCode.en);
            Assert.AreEqual("Got it!", TutorialTexts.Get(localization, TutorialTexts.GotIt));
            Assert.AreEqual("Show tips", TutorialTexts.Get(localization, TutorialTexts.SettingsShowHints));

            localization.SetLanguage(LanguageCode.ptBR);
            Assert.AreEqual("Mostrar dicas", TutorialTexts.Get(localization, TutorialTexts.SettingsShowHints));
        }
    }
}
