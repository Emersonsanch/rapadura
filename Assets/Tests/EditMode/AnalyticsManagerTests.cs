using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Analytics;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Crafting;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="AnalyticsManager"/>: manual TrackEvent buffering, FlushToLog
    /// draining the buffer, and automatic tracking of the existing EventBus events it subscribes
    /// to (CombatTargetDiedEvent, PlayerExperienceChangedEvent, RecipeCraftedEvent).
    /// </summary>
    public class AnalyticsManagerTests
    {
        private AnalyticsManager _manager;

        [TearDown]
        public void TearDown()
        {
            _manager?.Shutdown();
            _manager = null;
            EventBus.Clear();
        }

        [Test]
        public void TrackEvent_AddsEntryToBuffer()
        {
            _manager = new AnalyticsManager();

            _manager.TrackEvent("test_event", new Dictionary<string, object> { ["foo"] = 42 });

            Assert.AreEqual(1, _manager.BufferedEvents.Count);
            Assert.AreEqual("test_event", _manager.BufferedEvents[0].Name);
            Assert.AreEqual(42, _manager.BufferedEvents[0].Properties["foo"]);
        }

        [Test]
        public void TrackEvent_WithoutProperties_StoresEmptyPropertyBag()
        {
            _manager = new AnalyticsManager();

            _manager.TrackEvent("no_props_event");

            Assert.AreEqual(1, _manager.BufferedEvents.Count);
            Assert.AreEqual(0, _manager.BufferedEvents[0].Properties.Count);
        }

        [Test]
        public void TrackEvent_WithEmptyName_IsIgnored()
        {
            _manager = new AnalyticsManager();

            _manager.TrackEvent(string.Empty);
            _manager.TrackEvent(null);

            Assert.AreEqual(0, _manager.BufferedEvents.Count);
        }

        [Test]
        public void TrackEvent_UsesProvidedTimestampProvider()
        {
            var fixedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _manager = new AnalyticsManager(() => fixedTime);

            _manager.TrackEvent("timed_event");

            Assert.AreEqual(fixedTime, _manager.BufferedEvents[0].TimestampUtc);
        }

        [Test]
        public void FlushToLog_ClearsBuffer()
        {
            _manager = new AnalyticsManager();
            _manager.TrackEvent("event_a");
            _manager.TrackEvent("event_b");

            _manager.FlushToLog();

            Assert.AreEqual(0, _manager.BufferedEvents.Count);
        }

        [Test]
        public void ClearBuffer_DiscardsEventsWithoutLogging()
        {
            _manager = new AnalyticsManager();
            _manager.TrackEvent("event_a");

            _manager.ClearBuffer();

            Assert.AreEqual(0, _manager.BufferedEvents.Count);
        }

        [Test]
        public void Initialize_TracksCombatTargetDiedEvent()
        {
            _manager = new AnalyticsManager();
            _manager.Initialize();
            var target = new GameObject("Target");
            var killer = new GameObject("Killer");

            try
            {
                EventBus.Publish(new CombatTargetDiedEvent(target, killer));

                Assert.AreEqual(1, _manager.BufferedEvents.Count);
                Assert.AreEqual("combat_target_died", _manager.BufferedEvents[0].Name);
                Assert.AreEqual("Target", _manager.BufferedEvents[0].Properties["target"]);
                Assert.AreEqual("Killer", _manager.BufferedEvents[0].Properties["killer"]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(killer);
            }
        }

        [Test]
        public void Initialize_TracksPlayerExperienceChangedEvent()
        {
            _manager = new AnalyticsManager();
            _manager.Initialize();

            EventBus.Publish(new PlayerExperienceChangedEvent(currentExperience: 50, experienceToNextLevel: 100, level: 3));

            Assert.AreEqual(1, _manager.BufferedEvents.Count);
            Assert.AreEqual("player_experience_changed", _manager.BufferedEvents[0].Name);
            Assert.AreEqual(3, _manager.BufferedEvents[0].Properties["level"]);
        }

        [Test]
        public void Initialize_TracksRecipeCraftedEvent()
        {
            _manager = new AnalyticsManager();
            _manager.Initialize();
            var recipe = ScriptableObject.CreateInstance<RecipeDefinition>();
            recipe.name = "TestRecipe";

            try
            {
                EventBus.Publish(new RecipeCraftedEvent(recipe));

                Assert.AreEqual(1, _manager.BufferedEvents.Count);
                Assert.AreEqual("recipe_crafted", _manager.BufferedEvents[0].Name);
                Assert.AreEqual("TestRecipe", _manager.BufferedEvents[0].Properties["recipe"]);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(recipe);
            }
        }

        [Test]
        public void Shutdown_StopsTrackingFurtherEvents()
        {
            _manager = new AnalyticsManager();
            _manager.Initialize();
            _manager.Shutdown();

            EventBus.Publish(new PlayerExperienceChangedEvent(1, 10, 1));

            Assert.AreEqual(0, _manager.BufferedEvents.Count);
        }
    }
}
