using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Performance;
using Rapadura.UI.Menus;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the battery/thermal frame pacing policy (<see cref="FrameRatePolicy"/>) and
    /// <see cref="FrameRateManager"/>'s context tracking, using a fake <see cref="IFrameRateApplier"/>
    /// so nothing touches the real Application/QualitySettings globals.
    /// </summary>
    public class FrameRateManagerTests
    {
        private const string Combat = "Main";
        private const string WorldMap = "WorldMap";

        private sealed class FakeApplier : IFrameRateApplier
        {
            public readonly List<FrameRateSettings> Applied = new List<FrameRateSettings>();
            public FrameRateSettings Last => Applied[Applied.Count - 1];

            public void Apply(FrameRateSettings settings)
            {
                Applied.Add(settings);
            }
        }

        private FrameRateManager _manager;
        private FakeApplier _applier;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(FrameRateManager.HighFrameRateInCombatKey);
            _applier = new FakeApplier();
        }

        [TearDown]
        public void TearDown()
        {
            _manager?.Shutdown();
            _manager = null;
            PlayerPrefs.DeleteKey(FrameRateManager.HighFrameRateInCombatKey);
        }

        private FrameRateManager CreateMobile()
        {
            _manager = new FrameRateManager(isMobile: true, applier: _applier);
            _manager.Initialize();
            return _manager;
        }

        // ---------------- FrameRatePolicy.Resolve ----------------

        [Test]
        public void Desktop_AlwaysVSyncUncapped()
        {
            foreach (FrameRateContext context in System.Enum.GetValues(typeof(FrameRateContext)))
            {
                FrameRateSettings settings = FrameRatePolicy.Resolve(context, isMobile: false, highFrameRateInCombat: true);
                Assert.AreEqual(1, settings.VSyncCount, context.ToString());
                Assert.AreEqual(-1, settings.TargetFrameRate, context.ToString());
                Assert.AreEqual(1, settings.RenderFrameInterval, context.ToString());
            }
        }

        [TestCase(FrameRateContext.Menu)]
        [TestCase(FrameRateContext.WorldMap)]
        [TestCase(FrameRateContext.Paused)]
        public void Mobile_NonCombat_Is30(FrameRateContext context)
        {
            FrameRateSettings settings = FrameRatePolicy.Resolve(context, isMobile: true, highFrameRateInCombat: true);
            Assert.AreEqual(30, settings.TargetFrameRate);
            Assert.AreEqual(0, settings.VSyncCount);
        }

        [Test]
        public void Mobile_Combat_Is60_WhenOptionOn_And30_WhenOff()
        {
            Assert.AreEqual(60, FrameRatePolicy.Resolve(FrameRateContext.Combat, true, true).TargetFrameRate);
            Assert.AreEqual(30, FrameRatePolicy.Resolve(FrameRateContext.Combat, true, false).TargetFrameRate);
        }

        [Test]
        public void Mobile_Paused_RendersEveryOtherFrame_OthersEveryFrame()
        {
            Assert.AreEqual(2, FrameRatePolicy.Resolve(FrameRateContext.Paused, true, true).RenderFrameInterval);
            Assert.AreEqual(1, FrameRatePolicy.Resolve(FrameRateContext.Combat, true, true).RenderFrameInterval);
            Assert.AreEqual(1, FrameRatePolicy.Resolve(FrameRateContext.Menu, true, true).RenderFrameInterval);
        }

        // ---------------- FrameRatePolicy.ResolveContext ----------------

        [Test]
        public void ResolveContext_MapsScenesAndFlags()
        {
            Assert.AreEqual(FrameRateContext.Combat, FrameRatePolicy.ResolveContext(Combat, Combat, WorldMap, false, false));
            Assert.AreEqual(FrameRateContext.WorldMap, FrameRatePolicy.ResolveContext(WorldMap, Combat, WorldMap, false, false));
            Assert.AreEqual(FrameRateContext.Menu, FrameRatePolicy.ResolveContext("CharacterSelect", Combat, WorldMap, false, false));
            Assert.AreEqual(FrameRateContext.Menu, FrameRatePolicy.ResolveContext(null, Combat, WorldMap, false, false));
        }

        [Test]
        public void ResolveContext_PauseWinsOverMenuAndScene()
        {
            Assert.AreEqual(FrameRateContext.Paused, FrameRatePolicy.ResolveContext(Combat, Combat, WorldMap, true, true));
            Assert.AreEqual(FrameRateContext.Menu, FrameRatePolicy.ResolveContext(Combat, Combat, WorldMap, false, true));
        }

        // ---------------- FrameRateManager ----------------

        [Test]
        public void Initialize_AppliesOnce_AndDefaultsHighFrameRateOn()
        {
            CreateMobile();

            Assert.IsTrue(_manager.HighFrameRateInCombat);
            Assert.AreEqual(1, _applier.Applied.Count);
        }

        [Test]
        public void SceneChanges_SwitchBetween60And30()
        {
            CreateMobile();

            _manager.NotifySceneChanged(Combat);
            Assert.AreEqual(FrameRateContext.Combat, _manager.CurrentContext);
            Assert.AreEqual(60, _applier.Last.TargetFrameRate);

            _manager.NotifySceneChanged(WorldMap);
            Assert.AreEqual(FrameRateContext.WorldMap, _manager.CurrentContext);
            Assert.AreEqual(30, _applier.Last.TargetFrameRate);
        }

        [Test]
        public void PauseEvents_DropTo30_AndResumeRestores60()
        {
            CreateMobile();
            _manager.NotifySceneChanged(Combat);

            EventBus.Publish(new GamePausedEvent());
            Assert.AreEqual(FrameRateContext.Paused, _manager.CurrentContext);
            Assert.AreEqual(30, _applier.Last.TargetFrameRate);

            EventBus.Publish(new GameResumedEvent());
            Assert.AreEqual(FrameRateContext.Combat, _manager.CurrentContext);
            Assert.AreEqual(60, _applier.Last.TargetFrameRate);
        }

        [Test]
        public void MenuOpen_OverridesCombatScene()
        {
            CreateMobile();
            _manager.NotifySceneChanged(Combat);

            _manager.SetMenuOpen(true);
            Assert.AreEqual(FrameRateContext.Menu, _manager.CurrentContext);

            _manager.SetMenuOpen(false);
            Assert.AreEqual(FrameRateContext.Combat, _manager.CurrentContext);
        }

        [Test]
        public void SceneLoad_ClearsStalePause()
        {
            CreateMobile();
            _manager.NotifySceneChanged(Combat);
            _manager.SetPaused(true);

            _manager.NotifySceneChanged(Combat);

            Assert.AreEqual(FrameRateContext.Combat, _manager.CurrentContext);
        }

        [Test]
        public void UnchangedContext_DoesNotReapply()
        {
            CreateMobile();
            _manager.NotifySceneChanged(Combat);
            int count = _applier.Applied.Count;

            _manager.NotifySceneChanged(Combat);
            _manager.SetMenuOpen(false);

            Assert.AreEqual(count, _applier.Applied.Count);
        }

        [Test]
        public void HighFrameRateOption_PersistsAndRaisesEvent()
        {
            CreateMobile();
            _manager.NotifySceneChanged(Combat);
            int raised = 0;
            _manager.SettingsChanged += () => raised++;

            _manager.SetHighFrameRateInCombat(false);

            Assert.AreEqual(1, raised);
            Assert.AreEqual(30, _applier.Last.TargetFrameRate);
            Assert.AreEqual(0, PlayerPrefs.GetInt(FrameRateManager.HighFrameRateInCombatKey, 1));

            _manager.Shutdown();
            _manager = new FrameRateManager(isMobile: true, applier: _applier);
            _manager.Initialize();
            Assert.IsFalse(_manager.HighFrameRateInCombat);
        }

        [Test]
        public void Shutdown_UnsubscribesFromPauseEvents()
        {
            CreateMobile();
            _manager.NotifySceneChanged(Combat);
            _manager.Shutdown();
            int count = _applier.Applied.Count;

            EventBus.Publish(new GamePausedEvent());

            Assert.AreEqual(count, _applier.Applied.Count);
        }

        [Test]
        public void Desktop_Manager_AppliesVSync()
        {
            _manager = new FrameRateManager(isMobile: false, applier: _applier);
            _manager.Initialize();
            _manager.NotifySceneChanged(Combat);

            Assert.AreEqual(1, _applier.Last.VSyncCount);
            Assert.AreEqual(-1, _applier.Last.TargetFrameRate);
        }
    }
}
