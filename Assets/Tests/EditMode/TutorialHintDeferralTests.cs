using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Tutorial;
using Rapadura.UI.Death;
using Rapadura.UI.Tutorial;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Tutorial hints vs. modal screens: the bubble sorts below the death screen and steps aside while
    /// any other screen holds a <see cref="GameplayPause"/> (<see cref="PauseRequestTracker.IsPausedByOthers"/>),
    /// and the first-death tip jumps the hint queue (<see cref="TutorialStepDefinition.Interrupts"/>) so
    /// the death screen can show it at once instead of after the still-pending encounter tips.
    /// </summary>
    public class TutorialHintDeferralTests
    {
        private TutorialManager _manager;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            GameplayPause.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            _manager?.Shutdown();
            _manager = null;
            GameplayPause.Reset();
            Time.timeScale = 1f;
            EventBus.Clear();
            TestSaveRoot.Exit();
        }

        [Test]
        public void Tracker_PausedByOthers_IgnoresTheAskingOwner()
        {
            var tracker = new PauseRequestTracker();
            object hint = new object();
            object deathScreen = new object();

            Assert.IsFalse(tracker.IsPausedByOthers(hint));

            tracker.Acquire(hint);
            Assert.IsFalse(tracker.IsPausedByOthers(hint), "a hint's own pause never defers it");
            Assert.IsTrue(tracker.IsPausedByOthers(deathScreen));

            tracker.Acquire(deathScreen);
            Assert.IsTrue(tracker.IsPausedByOthers(hint));

            tracker.Release(deathScreen);
            tracker.Release(hint);
            tracker.SetExternalPaused(true);
            Assert.IsTrue(tracker.IsPausedByOthers(hint), "the pause menu (external pause) defers hints too");
        }

        [Test]
        public void GameplayPause_ModalOpen_DefersTheHint_AndClosingItBringsItBack()
        {
            object hint = new object();
            object modal = new object();

            Assert.IsFalse(TutorialHintController.ShouldDefer(GameplayPause.IsPausedByOthers(hint)));

            GameplayPause.Acquire(modal);
            Assert.IsTrue(TutorialHintController.ShouldDefer(GameplayPause.IsPausedByOthers(hint)), "death screen / quest log / dialogue up: hide the hint");

            GameplayPause.Release(modal);
            Assert.IsFalse(TutorialHintController.ShouldDefer(GameplayPause.IsPausedByOthers(hint)), "modal closed: show it again");
        }

        [Test]
        public void HintSortsBelowTheDeathScreen()
        {
            // Death screen UIDocument: sort order 50 (Main.unity); toasts 60; windows 10-30; HUD 0-5.
            Assert.Less(TutorialHintController.SortingOrder, 50f);
            Assert.Greater(TutorialHintController.SortingOrder, 30f);
        }

        [Test]
        public void DeathTip_JumpsAheadOfPendingEncounterTips()
        {
            _manager = new TutorialManager();
            _manager.Initialize();
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.World, GameFlowState.Encounter));

            EventBus.Publish(new EnemySpawnedEvent(null));
            Assert.AreEqual(TutorialSteps.Encounter, _manager.CurrentHint?.Id);

            EventBus.Publish(new PlayerDiedEvent());
            Assert.AreEqual(TutorialSteps.Death, _manager.CurrentHint?.Id, "the 'Não desista!' tip must show on the death screen, not after returning to the map");

            _manager.Acknowledge(TutorialSteps.Death);
            Assert.AreEqual(TutorialSteps.Encounter, _manager.CurrentHint?.Id, "the older tip comes back afterwards");
        }

        [Test]
        public void OnlyTheDeathStepInterrupts()
        {
            foreach (TutorialStepDefinition step in TutorialSteps.All)
            {
                Assert.AreEqual(step.Id == TutorialSteps.Death, step.Interrupts, step.Id);
            }
        }

        [Test]
        public void DeathScreenView_HasTheInlineTip()
        {
            string uxml = System.IO.File.ReadAllText("Assets/UI/Death/DeathScreenView.uxml");
            StringAssert.Contains("name=\"death-tip\"", uxml);
            StringAssert.Contains("name=\"death-tip-title\"", uxml);
            StringAssert.Contains("name=\"death-tip-body\"", uxml);
            Assert.IsNotNull(typeof(DeathScreenController));
        }
    }
}
