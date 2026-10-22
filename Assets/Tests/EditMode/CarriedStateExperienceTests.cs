using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Player;
using Rapadura.Save;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Regression: XP granted in Main.unity's first frames (a kill, a quest reward) was undone by the
    /// carried-state restore. GameManager runs <see cref="SaveManager.RestoreCarriedState"/> one frame
    /// after the scene loads, and <see cref="PlayerStats.RestoreState"/> overwrote level/XP with the
    /// state carried from the previous scene. PlayerStats now re-applies XP earned while carried state
    /// was pending on top of the restored state; an explicit slot load still lets the file win.
    /// </summary>
    public class CarriedStateExperienceTests
    {
        private const int TestSlot = 90083;

        private readonly List<Object> _created = new List<Object>();
        private SaveManager _manager;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            _manager = new SaveManager();
            _manager.Initialize();
            ServiceLocator.Register(_manager);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Unregister<SaveManager>();
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

        private PlayerStats NewStats(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go.AddComponent<PlayerStats>();
        }

        /// <summary>Levels a fresh player with <paramref name="amounts"/> in order — the expected outcome.</summary>
        private PlayerStats Reference(params int[] amounts)
        {
            PlayerStats reference = NewStats("Reference");
            foreach (int amount in amounts)
            {
                reference.AddExperience(amount);
            }

            return reference;
        }

        /// <summary>Previous scene's player: earns XP, then unloads (its state becomes carried).</summary>
        private void CarryPlayerWith(int experience)
        {
            PlayerStats previous = NewStats("PreviousScenePlayer");
            _manager.Register(previous);
            previous.AddExperience(experience);
            _manager.Unregister(previous);
            Assert.IsTrue(_manager.HasCarriedState(previous.SaveKey), "precondition: the previous scene's state is carried");
        }

        [Test]
        public void XpGrantedBeforeCarriedRestore_IsKeptOnTopOfTheRestoredState()
        {
            const int carriedXp = 260; // level 3 with leftover XP on the shipped curve
            const int earlyKillXp = 40;
            CarryPlayerWith(carriedXp);

            // New Main.unity: a kill lands before GameManager's one-frame-late restore (and even before
            // PlayerController.Start registered the stats).
            PlayerStats fresh = NewStats("FreshPlayer");
            fresh.AddExperience(earlyKillXp);
            _manager.Register(fresh);

            Assert.AreEqual(1, _manager.RestoreCarriedState());

            PlayerStats expected = Reference(carriedXp, earlyKillXp);
            Assert.Greater(expected.Level, 1, "precondition: the carried state is not a level-1 player");
            Assert.AreEqual(expected.Level, fresh.Level, "Level after restore");
            Assert.AreEqual(expected.CurrentExperience, fresh.CurrentExperience, "XP granted before the carried restore was lost.");
        }

        [Test]
        public void XpGrantedBeforeCarriedRestore_CanLevelUpFromTheRestoredLevel()
        {
            PlayerStats probe = Reference(260);
            int toNext = probe.GetExperienceToNextLevel() - probe.CurrentExperience;
            CarryPlayerWith(260);

            PlayerStats fresh = NewStats("FreshPlayer");
            fresh.AddExperience(toNext + 5);
            _manager.Register(fresh);
            _manager.RestoreCarriedState();

            Assert.AreEqual(probe.Level + 1, fresh.Level);
            Assert.AreEqual(5, fresh.CurrentExperience);
        }

        [Test]
        public void XpAfterTheCarriedRestore_IsNotCountedTwice()
        {
            CarryPlayerWith(260);
            PlayerStats fresh = NewStats("FreshPlayer");
            fresh.AddExperience(10);
            _manager.Register(fresh);
            _manager.RestoreCarriedState();

            fresh.AddExperience(15);
            // A later explicit restore (e.g. the same state re-applied) must not replay the early XP.
            fresh.RestoreState(fresh.CaptureState());

            PlayerStats expected = Reference(260, 10, 15);
            Assert.AreEqual(expected.Level, fresh.Level);
            Assert.AreEqual(expected.CurrentExperience, fresh.CurrentExperience);
        }

        [Test]
        public void SlotLoad_StillLetsTheFileWin()
        {
            PlayerStats stats = NewStats("Player");
            _manager.Register(stats);
            stats.AddExperience(50);
            _manager.Save(TestSlot);

            stats.AddExperience(30); // not saved: loading the slot rolls it back, as before
            Assert.IsTrue(_manager.Load(TestSlot));

            Assert.AreEqual(1, stats.Level);
            Assert.AreEqual(50, stats.CurrentExperience);
        }
    }
}
