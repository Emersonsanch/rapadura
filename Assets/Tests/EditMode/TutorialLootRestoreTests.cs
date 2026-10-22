using System;
using System.IO;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Tutorial;
using Rapadura.Save;

namespace Rapadura.Tests
{
    /// <summary>
    /// Playtest regression: the loot hint fired right after "Continuar" — the previous session's kill
    /// was still remembered and the save restore's InventoryChangedEvent (published before
    /// GameLoadedEvent resets the flag) counted as loot. Covers <see cref="SaveManager.IsRestoring"/>
    /// and <see cref="TutorialManager"/>'s loot condition.
    /// </summary>
    public class TutorialLootRestoreTests
    {
        private const int TestSlot = 90021;

        [Serializable]
        private class InventoryProbeState
        {
            public int value;
        }

        /// <summary>Stands in for InventoryManager: its RestoreState publishes InventoryChangedEvent.</summary>
        private class InventoryProbe : ISaveable
        {
            public SaveManager Owner;
            public bool SawRestoring;

            public string SaveKey => "Inventory";
            public object CaptureState() => new InventoryProbeState { value = 1 };

            public void RestoreState(object state)
            {
                SawRestoring = Owner != null && Owner.IsRestoring;
                EventBus.Publish(new InventoryChangedEvent());
            }
        }

        private TutorialManager _tutorial;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            DeleteSlotFiles();
        }

        [TearDown]
        public void TearDown()
        {
            _tutorial?.Shutdown();
            _tutorial = null;
            EventBus.Clear();
            DeleteSlotFiles();
            TestSaveRoot.Exit();
        }

        [Test]
        public void SaveManager_IsRestoring_OnlyWhileLoadAndCarriedRestoreApplyState()
        {
            var saveManager = new SaveManager();
            saveManager.Initialize();
            var probe = new InventoryProbe { Owner = saveManager };
            saveManager.Register(probe);
            saveManager.Save(TestSlot);

            Assert.IsFalse(saveManager.IsRestoring);
            Assert.IsTrue(saveManager.Load(TestSlot));
            Assert.IsTrue(probe.SawRestoring, "Load must report IsRestoring while RestoreState runs.");
            Assert.IsFalse(saveManager.IsRestoring);

            probe.SawRestoring = false;
            saveManager.Unregister(probe); // carried, like the Player leaving Main.unity
            saveManager.Register(probe);
            Assert.AreEqual(1, saveManager.RestoreCarriedState());
            Assert.IsTrue(probe.SawRestoring, "RestoreCarriedState must report IsRestoring too.");
            Assert.IsFalse(saveManager.IsRestoring);
        }

        [Test]
        public void Loot_NotTriggeredByInventoryRestore_AfterPreviousSessionKill()
        {
            var saveManager = new SaveManager();
            saveManager.Initialize();
            var probe = new InventoryProbe { Owner = saveManager };
            saveManager.Register(probe);
            saveManager.Save(TestSlot);

            _tutorial = CreateTutorial(() => saveManager.IsRestoring);

            // Previous session: a kill (no loot change yet), then "Continuar" loads the save.
            EventBus.Publish(new EnemyDiedEvent(null, false));
            Assert.IsTrue(saveManager.Load(TestSlot));

            Assert.IsFalse(_tutorial.IsPending(TutorialSteps.Loot), "A save restore's inventory change is not loot.");
        }

        [Test]
        public void Loot_InventoryChangeWhileRestoring_Ignored_ButRealLootAfterAKillCounts()
        {
            bool restoring = true;
            _tutorial = CreateTutorial(() => restoring);

            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new InventoryChangedEvent());
            Assert.IsFalse(_tutorial.IsPending(TutorialSteps.Loot));

            restoring = false;
            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new InventoryChangedEvent());
            Assert.IsTrue(_tutorial.IsPending(TutorialSteps.Loot), "Genuine loot after a kill still teaches the step.");
        }

        [Test]
        public void Loot_ReturnToTitle_ForgetsTheKill()
        {
            _tutorial = CreateTutorial(() => false);
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.Boot, GameFlowState.Encounter));

            EventBus.Publish(new EnemyDiedEvent(null, false));
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.Encounter, GameFlowState.MainMenu));
            EventBus.Publish(new GameFlowStateChangedEvent(GameFlowState.MainMenu, GameFlowState.Encounter));
            EventBus.Publish(new InventoryChangedEvent());

            Assert.IsFalse(_tutorial.IsPending(TutorialSteps.Loot), "Returning to the title starts a fresh 'enemy defeated' state.");
        }

        private static TutorialManager CreateTutorial(Func<bool> isRestoring)
        {
            var manager = new TutorialManager(TutorialSteps.All) { IsRestoringSave = isRestoring };
            manager.Initialize();
            return manager;
        }

        private static void DeleteSlotFiles()
        {
            string main = TestSaveRoot.SlotPath(TestSlot);

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
