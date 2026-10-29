using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Campaign framing (<see cref="StoryProgress"/>, <see cref="StoryScript"/>, <see cref="StoryPanelSequence"/>):
    /// when each beat becomes due, the campaign-complete flag and its achievement, the save round-trip
    /// and saves written before the story existed.
    /// </summary>
    public class StoryProgressTests
    {
        private const int Slot = 90041;
        private const int LegacySlot = 90042;

        private readonly List<IManager> _managers = new List<IManager>();
        private readonly List<Object> _created = new List<Object>();
        private int _campaignCompletedEvents;

        private interface IManager
        {
            void Shutdown();
        }

        private sealed class ShutdownHandle : IManager
        {
            private readonly System.Action _shutdown;
            public ShutdownHandle(System.Action shutdown) => _shutdown = shutdown;
            public void Shutdown() => _shutdown();
        }

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            _campaignCompletedEvents = 0;
            EventBus.Subscribe<CampaignCompletedEvent>(OnCampaignCompleted);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (IManager manager in _managers)
            {
                manager.Shutdown();
            }

            _managers.Clear();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
            TestSaveRoot.Exit();
        }

        private void OnCampaignCompleted(CampaignCompletedEvent evt) => _campaignCompletedEvents++;

        private StoryProgress NewStory(WorldMapProgress map)
        {
            var story = new StoryProgress(map);
            story.Initialize();
            _managers.Add(new ShutdownHandle(story.Shutdown));
            return story;
        }

        // ---------------------------------------------------------------
        // When beats trigger
        // ---------------------------------------------------------------

        [Test]
        public void NewGame_PendsThePrologue_AndNothingElse()
        {
            var map = new WorldMapProgress();
            StoryProgress story = NewStory(map);

            Assert.AreEqual(StoryBeat.Prologue, story.GetPendingBeat());

            Assert.IsTrue(story.MarkSeen(StoryBeat.Prologue));
            Assert.IsFalse(story.MarkSeen(StoryBeat.Prologue), "A beat is seen only once.");
            Assert.AreEqual(StoryBeat.None, story.GetPendingBeat(), "No interlude before any boss falls.");
            Assert.IsFalse(story.CampaignComplete);
        }

        [Test]
        public void Interlude_PendsOnlyAfterItsRegionIsCleared()
        {
            var map = new WorldMapProgress();
            StoryProgress story = NewStory(map);
            story.MarkSeen(StoryBeat.Prologue);

            map.RecordVictory(BiomeType.Forest);
            Assert.AreEqual(StoryBeat.None, story.GetPendingBeat(), "A normal victory is not a story beat.");

            map.MarkRegionCleared(BiomeType.Forest);
            Assert.AreEqual(StoryBeat.InterludeForest, story.GetPendingBeat());

            story.MarkSeen(StoryBeat.InterludeForest);
            Assert.AreEqual(StoryBeat.None, story.GetPendingBeat());

            map.MarkRegionCleared(BiomeType.Desert);
            Assert.AreEqual(StoryBeat.InterludeDesert, story.GetPendingBeat());
        }

        [Test]
        public void SeveralClearedRegions_PlayInMapOrder_EndingLast()
        {
            var map = new WorldMapProgress();
            StoryProgress story = NewStory(map);
            map.MarkRegionCleared(BiomeType.Caves);
            map.MarkRegionCleared(BiomeType.Mountain);
            map.MarkRegionCleared(BiomeType.Forest);
            map.MarkRegionCleared(BiomeType.Desert);

            var played = new List<StoryBeat>();
            for (StoryBeat beat = story.GetPendingBeat(); beat != StoryBeat.None; beat = story.GetPendingBeat())
            {
                played.Add(beat);
                story.MarkSeen(beat);
            }

            CollectionAssert.AreEqual(StoryBeats.InOrder, played);
        }

        [Test]
        public void ClearingTheFinalRegion_CompletesTheCampaign_Once_AndPendsTheEnding()
        {
            var map = new WorldMapProgress();
            StoryProgress story = NewStory(map);
            foreach (StoryBeat beat in new[] { StoryBeat.Prologue, StoryBeat.InterludeForest, StoryBeat.InterludeDesert, StoryBeat.InterludeMountain })
            {
                story.MarkSeen(beat);
            }

            map.MarkRegionCleared(BiomeType.Mountain);
            Assert.IsFalse(story.CampaignComplete, "Only the final region completes the campaign.");

            Assert.IsTrue(map.MarkRegionCleared(StoryBeats.FinalRegion));
            Assert.IsTrue(story.CampaignComplete);
            Assert.AreEqual(1, _campaignCompletedEvents);
            Assert.AreEqual(StoryBeat.Ending, story.GetPendingBeat());

            story.MarkSeen(StoryBeat.Ending);
            Assert.AreEqual(StoryBeat.None, story.GetPendingBeat(), "Post-game: nothing left to show.");
            Assert.AreEqual(1, _campaignCompletedEvents, "Completion is announced once per save.");
        }

        [Test]
        public void RegionToBeatMapping_IsConsistent()
        {
            foreach (BiomeType region in (BiomeType[])System.Enum.GetValues(typeof(BiomeType)))
            {
                StoryBeat beat = StoryBeats.AfterRegionCleared(region);
                Assert.AreNotEqual(StoryBeat.None, beat, $"{region} has no story beat.");
                Assert.AreEqual(region, StoryBeats.RegionFor(beat));
                Assert.AreEqual(beat, StoryBeats.FromId(StoryBeats.Id(beat)));
            }

            Assert.AreEqual(StoryBeat.Ending, StoryBeats.AfterRegionCleared(StoryBeats.FinalRegion));
            Assert.AreEqual(StoryBeat.None, StoryBeats.FromId("beat_from_the_future"));
        }

        [Test]
        public void PresentationRules_OnlyOnAnIdleWorldMapWithAHero()
        {
            Assert.IsTrue(StoryPresentationRules.CanPresent(StoryBeat.Prologue, true, true, true, false, false));
            Assert.IsFalse(StoryPresentationRules.CanPresent(StoryBeat.None, true, true, true, false, false), "Nothing pending.");
            Assert.IsFalse(StoryPresentationRules.CanPresent(StoryBeat.Prologue, false, true, true, false, false), "Title/hero screens or an encounter.");
            Assert.IsFalse(StoryPresentationRules.CanPresent(StoryBeat.Prologue, true, false, true, false, false), "No hero yet.");
            Assert.IsFalse(StoryPresentationRules.CanPresent(StoryBeat.Prologue, true, true, false, false, false), "Not on the world map.");
            Assert.IsFalse(StoryPresentationRules.CanPresent(StoryBeat.Prologue, true, true, true, true, false), "Token still walking.");
            Assert.IsFalse(StoryPresentationRules.CanPresent(StoryBeat.Prologue, true, true, true, false, true), "Another modal holds GameplayPause.");

            Assert.IsTrue(StoryPresentationRules.RollsCredits(StoryBeat.Ending));
            Assert.IsFalse(StoryPresentationRules.RollsCredits(StoryBeat.InterludeMountain));
        }

        // ---------------------------------------------------------------
        // Persistence
        // ---------------------------------------------------------------

        [Test]
        public void CaptureRestore_RoundTripsThroughJson()
        {
            var map = new WorldMapProgress();
            StoryProgress story = NewStory(map);
            story.MarkSeen(StoryBeat.Prologue);
            story.MarkSeen(StoryBeat.InterludeForest);
            story.MarkCampaignComplete();

            string json = JsonUtility.ToJson(story.CaptureState());
            var restored = NewStory(new WorldMapProgress());
            restored.RestoreState(JsonUtility.FromJson<StorySaveState>(json));

            Assert.IsTrue(restored.CampaignComplete);
            Assert.IsTrue(restored.HasSeen(StoryBeat.Prologue));
            Assert.IsTrue(restored.HasSeen(StoryBeat.InterludeForest));
            Assert.IsFalse(restored.HasSeen(StoryBeat.InterludeDesert));
            StringAssert.Contains("interlude_forest", json, "Beats are saved by stable id, not enum value.");
        }

        [Test]
        public void RestoreState_IgnoresUnknownIdsAndNullList()
        {
            StoryProgress story = NewStory(new WorldMapProgress());

            story.RestoreState(new StorySaveState { seenBeats = new List<string> { "prologue", "removed_beat", null } });
            CollectionAssert.AreEquivalent(new[] { StoryBeat.Prologue }, story.SeenBeats.ToArray());

            story.RestoreState(new StorySaveState { seenBeats = null, campaignComplete = true });
            Assert.IsEmpty(story.SeenBeats);
            Assert.IsTrue(story.CampaignComplete);

            Assert.DoesNotThrow(() => story.RestoreState("not a story state"));
        }

        [Test]
        public void SaveAndLoad_ThroughSaveManager_KeepsSeenBeats_AndDoesNotReplayThem()
        {
            var writerMap = new WorldMapProgress();
            StoryProgress writerStory = NewStory(writerMap);
            var writer = new SaveManager();
            writer.Initialize();
            writer.Register(writerMap);
            writer.Register(writerStory);
            writerStory.MarkSeen(StoryBeat.Prologue);
            writerMap.MarkRegionCleared(BiomeType.Forest);
            writer.Save(Slot);
            Assume.That(File.Exists(TestSaveRoot.SlotPath(Slot)));

            StoryProgress story = LoadInto(Slot, out WorldMapProgress map);

            Assert.IsTrue(map.IsRegionCleared(BiomeType.Forest));
            Assert.IsTrue(story.HasSeen(StoryBeat.Prologue));
            Assert.AreEqual(StoryBeat.InterludeForest, story.GetPendingBeat(), "Saved right after the boss: the interlude still plays after loading.");
        }

        [Test]
        public void NewSessionReset_ForgetsTheStory()
        {
            StoryProgress story = NewStory(new WorldMapProgress());
            story.MarkSeen(StoryBeat.Prologue);
            story.MarkCampaignComplete();

            var reset = new NewSessionReset();
            reset.Register(story);
            reset.ResetAll();

            Assert.IsEmpty(story.SeenBeats);
            Assert.IsFalse(story.CampaignComplete);
            Assert.AreEqual(StoryBeat.Prologue, story.GetPendingBeat());
        }

        // ---------------------------------------------------------------
        // Saves written before the story existed
        // ---------------------------------------------------------------

        [Test]
        public void LegacySave_MidCampaign_SkipsTheBeatsAlreadyBehindThePlayer()
        {
            var legacyMap = new WorldMapProgress();
            legacyMap.MarkRegionCleared(BiomeType.Forest);
            legacyMap.Unlock(BiomeType.Desert);
            WriteLegacySave(legacyMap);

            StoryProgress story = LoadInto(LegacySlot, out _);

            Assert.IsTrue(story.HasSeen(StoryBeat.Prologue), "No prologue in the middle of an old campaign.");
            Assert.IsTrue(story.HasSeen(StoryBeat.InterludeForest), "Regions cleared back then don't replay their interlude.");
            Assert.AreEqual(StoryBeat.None, story.GetPendingBeat());
            Assert.IsFalse(story.CampaignComplete);
            Assert.AreEqual(0, _campaignCompletedEvents);
        }

        [Test]
        public void LegacySave_WithCavesCleared_CompletesTheCampaign_AndStillShowsTheEndingOnce()
        {
            var legacyMap = new WorldMapProgress();
            foreach (BiomeType region in (BiomeType[])System.Enum.GetValues(typeof(BiomeType)))
            {
                legacyMap.Unlock(region);
                legacyMap.MarkRegionCleared(region);
            }

            WriteLegacySave(legacyMap);
            EventBus.Clear();
            EventBus.Subscribe<CampaignCompletedEvent>(OnCampaignCompleted);
            _campaignCompletedEvents = 0;

            StoryProgress story = LoadInto(LegacySlot, out _);

            Assert.IsTrue(story.CampaignComplete);
            Assert.AreEqual(1, _campaignCompletedEvents, "The achievement hears about old completed campaigns too.");
            Assert.AreEqual(StoryBeat.Ending, story.GetPendingBeat(), "The ending never existed before: show it once.");
        }

        [Test]
        public void LegacySaveCheck_DoesNotTouchALiveSession()
        {
            var map = new WorldMapProgress();
            StoryProgress story = NewStory(map);
            story.ResetForNewSession();

            // New game: the prologue is seen in play, then a boss falls; a later carried-state restore
            // (GameLoadedEvent on entering an encounter) must not mark the new interlude as seen.
            story.MarkSeen(StoryBeat.Prologue);
            map.MarkRegionCleared(BiomeType.Forest);
            EventBus.Publish(new GameLoadedEvent());

            Assert.AreEqual(StoryBeat.InterludeForest, story.GetPendingBeat());
        }

        private void WriteLegacySave(WorldMapProgress legacyMap)
        {
            // Pre-story schema: no "Story" entry in the file.
            var writer = new SaveManager();
            writer.Initialize();
            writer.Register(legacyMap);
            writer.Save(LegacySlot);
            Assume.That(File.Exists(TestSaveRoot.SlotPath(LegacySlot)));
        }

        /// <summary>A GameManager-like reader: reset before load, map + story registered, story listening for GameLoadedEvent.</summary>
        private StoryProgress LoadInto(int slot, out WorldMapProgress map)
        {
            map = new WorldMapProgress();
            StoryProgress story = NewStory(map);

            var reset = new NewSessionReset();
            reset.Register(map);
            reset.Register(story);

            var reader = new SaveManager { SessionReset = reset };
            reader.Initialize();
            reader.Register(map);
            reader.Register(story);
            Assert.IsTrue(reader.Load(slot));
            return story;
        }

        // ---------------------------------------------------------------
        // Script, panels and texts
        // ---------------------------------------------------------------

        [Test]
        public void Script_EveryKey_HasBothLanguages()
        {
            var keys = StoryScript.AllKeys().ToList();
            Assert.IsNotEmpty(keys);

            foreach (string key in keys)
            {
                LocalizationEntry entry = StoryTexts.Entries.FirstOrDefault(e => e.Key == key);
                Assert.IsNotNull(entry, $"StoryTexts has no '{key}'.");
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), $"Missing en for '{key}'.");
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), $"Missing pt-BR for '{key}'.");
            }

            var used = new HashSet<string>(keys);
            foreach (LocalizationEntry entry in StoryTexts.Entries.Where(e => !e.Key.StartsWith("story.ui.")))
            {
                Assert.IsTrue(used.Contains(entry.Key), $"'{entry.Key}' is never shown by StoryScript.");
            }
        }

        [Test]
        public void Script_FocusesTheChosenHero_AndKeepsInterludesShort()
        {
            List<string> prologue = StoryScript.GetPanelKeys(StoryBeat.Prologue, CharacterId.Icaro);
            CollectionAssert.Contains(prologue, "story.prologue.hero.icaro");
            CollectionAssert.DoesNotContain(prologue, "story.prologue.hero.maithe");

            List<string> noHero = StoryScript.GetPanelKeys(StoryBeat.Prologue, null);
            Assert.AreEqual(prologue.Count - 1, noHero.Count, "Without a hero the focus panel is left out.");

            foreach (StoryBeat beat in new[] { StoryBeat.InterludeForest, StoryBeat.InterludeDesert, StoryBeat.InterludeMountain })
            {
                int count = StoryScript.GetPanelKeys(beat, CharacterId.Maria).Count;
                Assert.That(count, Is.InRange(2, 4), $"{beat} should have 2-4 panels.");
            }

            Assert.IsEmpty(StoryScript.GetPanelKeys(StoryBeat.None, CharacterId.Maria));
        }

        [Test]
        public void Sequence_AdvancesPanelByPanel_AndSkipEndsIt()
        {
            var sequence = new StoryPanelSequence(StoryBeat.InterludeForest, CharacterId.Lavine);
            Assert.AreEqual(3, sequence.Count);
            Assert.AreEqual("story.interlude_forest.1", sequence.CurrentKey);
            Assert.AreEqual("story.interlude_forest.title", sequence.TitleKey);

            Assert.IsTrue(sequence.Advance());
            Assert.IsTrue(sequence.Advance());
            Assert.IsTrue(sequence.IsLastPanel);
            Assert.IsFalse(sequence.Advance(), "Advancing past the last panel finishes the beat.");
            Assert.IsTrue(sequence.IsFinished);
            Assert.IsFalse(sequence.WasSkipped);
            Assert.IsNull(sequence.CurrentKey);

            var skipped = new StoryPanelSequence(StoryBeat.Ending, CharacterId.Joaquim);
            skipped.Skip();
            Assert.IsTrue(skipped.IsFinished);
            Assert.IsTrue(skipped.WasSkipped);
            Assert.IsFalse(skipped.Advance());
        }

        [Test]
        public void Texts_FallBackToPtBR_WithoutAManager_AndMergeWithoutOverriding()
        {
            Assert.AreEqual("Pular", StoryTexts.Get(null, StoryTexts.Skip));
            Assert.AreEqual("2/5", StoryTexts.Format(null, StoryTexts.Progress, 2, 5));

            var manager = new LocalizationManager();
            manager.LoadEntries(new[] { new LocalizationEntry(StoryTexts.Skip, "Custom skip", "Pular custom") });
            manager.SetLanguage(LanguageCode.en);
            StoryTexts.EnsureRegistered(manager);

            Assert.AreEqual("Custom skip", manager.Get(StoryTexts.Skip), "An existing key always wins.");
            Assert.AreEqual("Continue", StoryTexts.Get(manager, StoryTexts.Next));
        }

        // ---------------------------------------------------------------
        // Achievement
        // ---------------------------------------------------------------

        [Test]
        public void CampaignCompleted_UnlocksTheCampaignAchievement()
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            _created.Add(definition);
            definition.ConfigureForTests("ach_test_campaign", AchievementConditionType.CampaignCompleted, 1);

            var achievements = new AchievementManager(new[] { definition });
            achievements.Initialize();
            _managers.Add(new ShutdownHandle(achievements.Shutdown));

            StoryProgress story = NewStory(new WorldMapProgress());
            Assert.IsFalse(achievements.IsUnlocked("ach_test_campaign"));

            story.MarkCampaignComplete();
            Assert.IsTrue(achievements.IsUnlocked("ach_test_campaign"));
        }

        [Test]
        public void SeededCatalog_HasTheCampaignAchievement()
        {
            AchievementDefinition[] seeded = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder);
            AchievementDefinition campaign = seeded.FirstOrDefault(d => d.AchievementId == "ach_campaign_complete");

            Assert.IsNotNull(campaign, "Resources/Achievements/ach_campaign_complete.asset is missing.");
            Assert.AreEqual(AchievementConditionType.CampaignCompleted, campaign.ConditionType);
            Assert.AreEqual(1, campaign.TargetValue);
        }
    }
}
