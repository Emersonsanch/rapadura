using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Quests;
using Rapadura.UI.SkillTree;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the session flow fixes: the <see cref="GameFlowMachine"/> state machine and
    /// its <see cref="GameFlowService"/> event wiring, the modal pause bookkeeping
    /// (<see cref="PauseRequestTracker"/>), the hero-aware skill tree / quest tracker defaults, and
    /// SaveManager's carried (cross-scene) player state.
    /// </summary>
    public class GameFlowTests
    {
        private const int TestSlot = 90031;

        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            DeleteSlotFiles(TestSlot);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                Object.DestroyImmediate(obj);
            }

            _created.Clear();
            EventBus.Clear();
            DeleteSlotFiles(TestSlot);
            TestSaveRoot.Exit();
        }

        // ---- GameFlowMachine -------------------------------------------------------

        [Test]
        public void Machine_Boot_EntersTitleScreen()
        {
            var machine = new GameFlowMachine();

            Assert.IsTrue(machine.EnterFrontEnd(hasHero: false));
            Assert.AreEqual(GameFlowState.MainMenu, machine.State);

            // Re-entering the map's front-end while on the title screen changes nothing.
            Assert.IsFalse(machine.EnterFrontEnd(hasHero: false));
            Assert.AreEqual(GameFlowState.MainMenu, machine.State);
        }

        [Test]
        public void Machine_NewGame_GoesThroughCharacterSelectToWorld()
        {
            var machine = new GameFlowMachine();
            machine.EnterFrontEnd(false);

            Assert.IsTrue(machine.StartNewGame());
            Assert.AreEqual(GameFlowState.CharacterSelect, machine.State);

            Assert.IsTrue(machine.HeroChosen());
            Assert.AreEqual(GameFlowState.World, machine.State);
        }

        [Test]
        public void Machine_NewGameSelection_HidesSavedContinue()
        {
            var machine = new GameFlowMachine();
            machine.EnterFrontEnd(false);
            Assert.IsTrue(machine.OffersSavedContinueInSelect, "Title screen: no restriction.");

            machine.StartNewGame();
            Assert.IsTrue(machine.IsNewGameSelection);
            Assert.IsFalse(machine.OffersSavedContinueInSelect, "Novo jogo must not offer 'Continuar com <herói>'.");

            // Backing out to the title and in through a hero-less Continue offers it again.
            machine.CancelCharacterSelect();
            Assert.IsFalse(machine.IsNewGameSelection);
            machine.CompleteContinue(hasHero: false);
            Assert.AreEqual(GameFlowState.CharacterSelect, machine.State);
            Assert.IsTrue(machine.OffersSavedContinueInSelect);
        }

        [Test]
        public void Machine_NewGameSelection_ClearsOnceAHeroIsChosen()
        {
            var machine = new GameFlowMachine();
            machine.EnterFrontEnd(false);
            machine.StartNewGame();

            machine.HeroChosen();

            Assert.IsFalse(machine.IsNewGameSelection);
            Assert.IsTrue(machine.OffersSavedContinueInSelect);
        }

        [Test]
        public void Machine_BootSelect_StillOffersSavedContinue()
        {
            // Main.unity opened directly in the Editor shows the picker from Boot.
            var machine = new GameFlowMachine();
            Assert.IsTrue(machine.OffersSavedContinueInSelect);
        }

        [Test]
        public void Machine_Continue_WithHero_GoesStraightToWorld()
        {
            var machine = new GameFlowMachine();
            machine.EnterFrontEnd(false);

            Assert.IsTrue(machine.CompleteContinue(hasHero: true));
            Assert.AreEqual(GameFlowState.World, machine.State);
        }

        [Test]
        public void Machine_Continue_WithoutHero_AsksForOne()
        {
            var machine = new GameFlowMachine();
            machine.EnterFrontEnd(false);

            Assert.IsTrue(machine.CompleteContinue(hasHero: false));
            Assert.AreEqual(GameFlowState.CharacterSelect, machine.State);
        }

        [Test]
        public void Machine_EncounterRoundTrip_ReturnsToWorld_AndFrontEndStaysHidden()
        {
            var machine = new GameFlowMachine();
            machine.EnterFrontEnd(false);
            machine.CompleteContinue(true);

            Assert.IsTrue(machine.EncounterStarted());
            Assert.AreEqual(GameFlowState.Encounter, machine.State);
            Assert.IsTrue(machine.EncounterEnded());
            Assert.AreEqual(GameFlowState.World, machine.State);

            // Back on WorldMap.unity: no title screen again.
            Assert.IsFalse(machine.EnterFrontEnd(hasHero: true));
            Assert.AreEqual(GameFlowState.World, machine.State);
        }

        [Test]
        public void Machine_WorldWithoutHero_ForcesCharacterSelect()
        {
            var machine = new GameFlowMachine();
            machine.EnterFrontEnd(false);
            machine.CompleteContinue(true);

            Assert.IsTrue(machine.EnterFrontEnd(hasHero: false));
            Assert.AreEqual(GameFlowState.CharacterSelect, machine.State);
        }

        [Test]
        public void Machine_IgnoresInvalidRequests()
        {
            var machine = new GameFlowMachine();

            Assert.IsFalse(machine.StartNewGame(), "No new game before the title screen.");
            Assert.IsFalse(machine.CompleteContinue(true));
            Assert.IsFalse(machine.EncounterEnded());
            Assert.AreEqual(GameFlowState.Boot, machine.State);

            machine.EnterFrontEnd(false);
            Assert.IsFalse(machine.HeroChosen(), "A hero restored while on the title screen is resolved by Continue, not HeroChosen.");
            Assert.AreEqual(GameFlowState.MainMenu, machine.State);
        }

        [Test]
        public void Machine_HeroChosenFromBoot_IsTheEditorShortcut()
        {
            var machine = new GameFlowMachine();

            Assert.IsTrue(machine.HeroChosen());
            Assert.AreEqual(GameFlowState.World, machine.State);
        }

        [Test]
        public void Machine_WorldIsInteractiveOnlyInWorldState()
        {
            Assert.IsTrue(GameFlowMachine.IsWorldInteractive(GameFlowState.World));
            Assert.IsFalse(GameFlowMachine.IsWorldInteractive(GameFlowState.MainMenu));
            Assert.IsFalse(GameFlowMachine.IsWorldInteractive(GameFlowState.CharacterSelect));
            Assert.IsFalse(GameFlowMachine.IsWorldInteractive(GameFlowState.Boot));
        }

        [Test]
        public void CombatSceneSelect_NeverShowsDuringEncounter()
        {
            Assert.AreEqual(CharacterSelectDecision.ReapplyAndSkip, GameFlowMachine.DecideCombatSceneSelect(hasHero: true, encounterPending: true));
            Assert.AreEqual(CharacterSelectDecision.ReapplyAndSkip, GameFlowMachine.DecideCombatSceneSelect(hasHero: true, encounterPending: false));
            Assert.AreEqual(CharacterSelectDecision.AbortEncounter, GameFlowMachine.DecideCombatSceneSelect(hasHero: false, encounterPending: true));
            Assert.AreEqual(CharacterSelectDecision.Show, GameFlowMachine.DecideCombatSceneSelect(hasHero: false, encounterPending: false));
        }

        [Test]
        public void PickNewestSlot_ReturnsMostRecent_OrNoSlot()
        {
            DateTime now = DateTime.UtcNow;

            Assert.AreEqual(0, GameFlowMachine.PickNewestSlot(new[] { (SaveManager.AutoSaveSlot, now.AddMinutes(-5)), (0, now) }));
            Assert.AreEqual(SaveManager.AutoSaveSlot, GameFlowMachine.PickNewestSlot(new[] { (SaveManager.AutoSaveSlot, now), (0, now.AddDays(-1)) }));
            Assert.AreEqual(GameFlowMachine.NoSlot, GameFlowMachine.PickNewestSlot(new (int, DateTime)[0]));
            Assert.AreEqual(GameFlowMachine.NoSlot, GameFlowMachine.PickNewestSlot(null));
        }

        // ---- GameFlowService (event wiring) ----------------------------------------

        [Test]
        public void Service_NewGame_ClearsHero_AndConfirmingOneEntersWorld()
        {
            var selection = new CharacterSelectionState();
            selection.Select(CharacterId.Joaquim);
            var service = new GameFlowService(selection, null);
            service.Initialize();

            var published = new List<GameFlowState>();
            Action<GameFlowStateChangedEvent> handler = e => published.Add(e.Current);
            EventBus.Subscribe(handler);

            try
            {
                Assert.AreEqual(GameFlowState.MainMenu, service.EnterFrontEnd());
                Assert.IsTrue(service.StartNewGame());
                Assert.IsFalse(selection.HasSelection, "Novo jogo must forget the previous hero so the picker shows.");
                Assert.AreEqual(GameFlowState.CharacterSelect, service.State);
                Assert.IsFalse(service.OffersSavedContinueInSelect, "The Novo jogo picker must hide 'Continuar com <herói>'.");

                selection.Select(CharacterId.Maithe); // CharacterSelectController.ConfirmSelection
                Assert.AreEqual(GameFlowState.World, service.State);

                EventBus.Publish(new EncounterStartedEvent(null));
                Assert.AreEqual(GameFlowState.Encounter, service.State);
                EventBus.Publish(new EncounterEndedEvent(null, EncounterOutcome.Defeat));
                Assert.AreEqual(GameFlowState.World, service.State);

                CollectionAssert.AreEqual(
                    new[] { GameFlowState.MainMenu, GameFlowState.CharacterSelect, GameFlowState.World, GameFlowState.Encounter, GameFlowState.World },
                    published);
            }
            finally
            {
                EventBus.Unsubscribe(handler);
                service.Shutdown();
            }
        }

        [Test]
        public void Service_Continue_LoadsSaveWithHero_AndGoesToWorld()
        {
            var saveManager = new SaveManager();
            saveManager.Initialize();
            var selection = new CharacterSelectionState();
            saveManager.Register(selection);

            selection.Select(CharacterId.Lavine);
            saveManager.Save(TestSlot);
            selection.Clear();

            var service = new GameFlowService(selection, saveManager);
            service.Initialize();

            try
            {
                service.EnterFrontEnd();
                Assert.IsTrue(service.CanContinue(new[] { TestSlot }));
                Assert.IsTrue(service.Continue(new[] { TestSlot }));
                Assert.AreEqual(CharacterId.Lavine, selection.Selected);
                Assert.AreEqual(GameFlowState.World, service.State);
            }
            finally
            {
                service.Shutdown();
            }
        }

        // ---- PauseRequestTracker ---------------------------------------------------

        [Test]
        public void PauseTracker_IsReferenceCounted()
        {
            var tracker = new PauseRequestTracker();
            object select = new object();
            object inventory = new object();

            Assert.IsTrue(tracker.Acquire(select), "First owner freezes the game.");
            Assert.IsFalse(tracker.Acquire(inventory), "Already paused.");
            Assert.IsFalse(tracker.Acquire(select), "Same owner counts once.");
            Assert.AreEqual(2, tracker.OwnerCount);

            Assert.IsFalse(tracker.Release(select), "Inventory still open — stay paused.");
            Assert.IsTrue(tracker.IsPaused);
            Assert.IsTrue(tracker.Release(inventory), "Last owner resumes.");
            Assert.IsFalse(tracker.IsPaused);
            Assert.IsFalse(tracker.Release(inventory), "Releasing twice is a no-op.");
        }

        [Test]
        public void PauseTracker_ExternalPause_KeepsGamePausedAfterLastRelease()
        {
            var tracker = new PauseRequestTracker();
            object settings = new object();

            tracker.SetExternalPaused(true); // pause menu open
            Assert.IsFalse(tracker.Acquire(settings), "Pause menu already froze the game.");
            Assert.IsFalse(tracker.Release(settings), "Closing settings must not resume under the pause menu.");
            Assert.IsTrue(tracker.IsPaused);

            tracker.SetExternalPaused(false);
            Assert.IsFalse(tracker.IsPaused);
        }

        [Test]
        public void PauseTracker_IgnoresNullOwner()
        {
            var tracker = new PauseRequestTracker();

            Assert.IsFalse(tracker.Acquire(null));
            Assert.IsFalse(tracker.Release(null));
            Assert.IsFalse(tracker.IsPaused);
        }

        // ---- Skill tree default ----------------------------------------------------

        [Test]
        public void SkillTreeSelection_PrefersHeroTree_FallsBackToFirstAvailable()
        {
            SkillTreeDefinition general = Track(ScriptableObject.CreateInstance<SkillTreeDefinition>());
            SkillTreeDefinition maithe = Track(ScriptableObject.CreateInstance<SkillTreeDefinition>());
            var available = new[] { general };

            Func<CharacterId, SkillTreeDefinition> load = id => id == CharacterId.Maithe ? maithe : null;

            Assert.AreSame(maithe, SkillTreeSelection.Resolve(CharacterId.Maithe, load, available));
            Assert.AreSame(general, SkillTreeSelection.Resolve(CharacterId.Joaquim, load, available), "Hero without a tree asset falls back.");
            Assert.AreSame(general, SkillTreeSelection.Resolve(null, load, available), "No hero yet → first configured tree.");
            Assert.IsNull(SkillTreeSelection.Resolve(null, load, null));
        }

        // ---- Quest tracker default -------------------------------------------------

        [Test]
        public void TrackedQuest_DefaultsToCurrentHerosPersonalQuest()
        {
            List<QuestEntryViewModel> active = Entries("quest_joaquim_echoes_of_home", "quest_convergence_the_gathering", "quest_maithe_windtrail");

            Assert.AreEqual("quest_maithe_windtrail", QuestLogPresenter.ResolveTrackedQuestId(null, active, CharacterId.Maithe));
            Assert.AreEqual("quest_joaquim_echoes_of_home", QuestLogPresenter.ResolveTrackedQuestId(null, active, CharacterId.Joaquim));
        }

        [Test]
        public void TrackedQuest_ExplicitChoiceStillWins_AndFallsBackToFirstActive()
        {
            List<QuestEntryViewModel> active = Entries("quest_joaquim_echoes_of_home", "quest_maithe_windtrail");

            Assert.AreEqual("quest_joaquim_echoes_of_home", QuestLogPresenter.ResolveTrackedQuestId("quest_joaquim_echoes_of_home", active, CharacterId.Maithe));
            Assert.AreEqual("quest_joaquim_echoes_of_home", QuestLogPresenter.ResolveTrackedQuestId(null, active, CharacterId.Icaro), "Hero's quest not active → first active.");
            Assert.AreEqual("quest_joaquim_echoes_of_home", QuestLogPresenter.ResolveTrackedQuestId(null, active, null));
            Assert.IsNull(QuestLogPresenter.ResolveTrackedQuestId(null, new List<QuestEntryViewModel>(), CharacterId.Maithe));
        }

        [Test]
        public void PersonalQuestPrefix_MatchesSeededIds()
        {
            Assert.IsTrue(QuestLogPresenter.IsPersonalQuestOf("quest_icaro_ancient_gears", CharacterId.Icaro));
            Assert.IsTrue(QuestLogPresenter.IsPersonalQuestOf("quest_maria_fading_light", CharacterId.Maria));
            Assert.IsFalse(QuestLogPresenter.IsPersonalQuestOf("quest_maria_fading_light", CharacterId.Maithe));
            Assert.IsFalse(QuestLogPresenter.IsPersonalQuestOf(null, CharacterId.Maithe));
        }

        // ---- SaveManager carried state ---------------------------------------------

        [Serializable]
        private class ProbeState
        {
            public int value;
        }

        private class Probe : ISaveable
        {
            public Probe(string key) { SaveKey = key; }
            public string SaveKey { get; }
            public int Value;
            public int RestoreCount;
            public object CaptureState() => new ProbeState { value = Value };
            public void RestoreState(object state) { Value = ((ProbeState)state).value; RestoreCount++; }
        }

        private class SceneLocalProbe : Probe, ISceneLocalSaveable
        {
            public SceneLocalProbe(string key) : base(key) { }
        }

        [Test]
        public void Carried_UnregisteredPlayerState_SurvivesSceneSwap_AndIsStillSaved()
        {
            var manager = new SaveManager();
            manager.Initialize();

            var oldPlayer = new Probe("PlayerStats") { Value = 7 };
            manager.Register(oldPlayer);
            manager.Unregister(oldPlayer); // Main.unity unloads

            Assert.IsTrue(manager.HasCarriedState("PlayerStats"));

            // A save on the world map (no player alive) keeps the player's entry.
            manager.Save(TestSlot);
            SaveData saved = manager.PeekSlot(TestSlot);
            Assert.IsTrue(saved.entries.Exists(e => e.key == "PlayerStats"));

            // Next Main.unity: fresh player registers, then GameManager restores the carry.
            var newPlayer = new Probe("PlayerStats");
            manager.Register(newPlayer);
            Assert.AreEqual(1, manager.RestoreCarriedState());
            Assert.AreEqual(7, newPlayer.Value);
            Assert.IsFalse(manager.HasCarriedState("PlayerStats"));
        }

        [Test]
        public void Carried_LoadWithoutOwner_IsAppliedWhenOwnerRegisters()
        {
            var writer = new SaveManager();
            writer.Initialize();
            var player = new Probe("Inventory") { Value = 42 };
            writer.Register(player);
            writer.Save(TestSlot);

            // "Continuar" on the world map: nobody registered for the player's key yet.
            var manager = new SaveManager();
            manager.Initialize();
            Assert.IsTrue(manager.Load(TestSlot));
            Assert.IsTrue(manager.HasCarriedState("Inventory"));

            var fresh = new Probe("Inventory");
            manager.Register(fresh);
            Assert.AreEqual(0, fresh.RestoreCount, "Registering alone doesn't restore (component Start order is undefined).");
            manager.RestoreCarriedState();
            Assert.AreEqual(42, fresh.Value);
        }

        [Test]
        public void Carried_SceneLocalState_IsNotAppliedToNewScene()
        {
            var manager = new SaveManager();
            manager.Initialize();

            var oldTransform = new SceneLocalProbe("PlayerTransform") { Value = 99 };
            manager.Register(oldTransform);
            manager.Unregister(oldTransform);

            var newTransform = new SceneLocalProbe("PlayerTransform") { Value = 1 };
            manager.Register(newTransform);
            Assert.AreEqual(0, manager.RestoreCarriedState());
            Assert.AreEqual(1, newTransform.Value, "The arena spawn must win over the previous scene's position.");
        }

        [Test]
        public void Carried_ClearedByNewGame()
        {
            var manager = new SaveManager();
            manager.Initialize();
            var player = new Probe("PlayerStats") { Value = 3 };
            manager.Register(player);
            manager.Unregister(player);

            manager.ClearCarriedState();

            Assert.IsFalse(manager.HasCarriedState("PlayerStats"));
        }

        // ---- helpers ---------------------------------------------------------------

        private T Track<T>(T obj) where T : Object
        {
            _created.Add(obj);
            return obj;
        }

        private static List<QuestEntryViewModel> Entries(params string[] ids)
        {
            var list = new List<QuestEntryViewModel>();

            foreach (string id in ids)
            {
                list.Add(new QuestEntryViewModel { QuestId = id, Title = id });
            }

            return list;
        }

        private static void DeleteSlotFiles(int slot)
        {
            string main = TestSaveRoot.SlotPath(slot);

            foreach (string path in new[] { main, main + ".bak", main + ".tmp" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }
    }
}
