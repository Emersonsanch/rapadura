using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Quests;

namespace Rapadura.Tests
{
    /// <summary>
    /// Old saves and "A Lua que Não Enche" (<see cref="MidStorySaveMigration"/>, Docs/HistoriaMeio_20a100.md): a hero who already took a letter gets the moon done with no
    /// rewards; a hero past level 100 without letters is pointed at the moon; a new save is left alone.
    /// </summary>
    public class MidStorySaveMigrationTests
    {
        private static IEnumerable<string> MoonIds => MidStoryCatalog.Steps.Select(s => s.QuestId);

        [Test]
        public void SaveWithFirstLetterDone_GetsWholeMoonDone_AndNoRewards()
        {
            var save = new QuestSaveState
            {
                CompletedQuestIds = new List<string> { LateStoryCatalog.SteppeFoals },
                ActiveQuests = new List<QuestProgress> { new QuestProgress { QuestId = LateStoryCatalog.SteppeKurgans } },
            };

            var quests = new QuestManager();
            quests.RestoreState(save);

            foreach (string id in MoonIds)
            {
                Assert.IsTrue(quests.IsCompleted(id), id);
                Assert.IsFalse(quests.IsActive(id), id);
            }

            Assert.IsTrue(quests.IsCompleted(LateStoryCatalog.SteppeFoals));
            Assert.IsTrue(quests.IsActive(LateStoryCatalog.SteppeKurgans), "the running letter stays where it was");
            CollectionAssert.IsEmpty(quests.PendingRewardQuestIds, "no XP, gold or items owed for the moon");
            Assert.AreEqual(LateStoryCatalog.SteppeKurgans, MidStorySaveMigration.NextMainStep(quests.IsCompleted).QuestId, "the chain does not go back to the moon");

            var saved = (QuestSaveState)quests.CaptureState();
            Assert.AreEqual(MidStorySaveMigration.Schema, saved.StorySchema);
            Assert.AreEqual(saved.CompletedQuestIds.Count, saved.CompletedQuestIds.Distinct().Count(), "no duplicates");
        }

        [Test]
        public void SaveWithOnlyARunningLetter_AlsoGetsTheMoon_AndKeepsOwedRewardsAsTheyWere()
        {
            var save = new QuestSaveState
            {
                ActiveQuests = new List<QuestProgress> { new QuestProgress { QuestId = LateStoryCatalog.SteppeFoals }, new QuestProgress { QuestId = MidStoryCatalog.MingchengLock } },
                CompletedQuestIds = new List<string> { MidStoryCatalog.LimiarMoon },
                PendingRewardQuestIds = new List<string> { "quest_owed_before" },
            };

            Assert.AreEqual(MidStoryCatalog.Steps.Count - 1, MidStorySaveMigration.Upgrade(save));
            CollectionAssert.IsSubsetOf(MoonIds, save.CompletedQuestIds);
            Assert.IsFalse(save.ActiveQuests.Any(p => p.QuestId == MidStoryCatalog.MingchengLock), "a running moon step is closed, not left active");
            CollectionAssert.AreEqual(new[] { "quest_owed_before" }, save.PendingRewardQuestIds);
            Assert.AreEqual(0, MidStorySaveMigration.Upgrade(save), "runs once");
        }

        [Test]
        public void SaveAtLevel120WithoutLetters_OffersTheMoon_FromVoEdna()
        {
            var save = new QuestSaveState { CompletedQuestIds = new List<string> { "quest_some_city_quest" } };
            var quests = new QuestManager();
            quests.RestoreState(save);

            foreach (string id in MoonIds)
            {
                Assert.IsFalse(quests.IsCompleted(id), "the moon is new content: " + id);
            }

            LateStoryStep next = MidStorySaveMigration.NextMainStep(quests.IsCompleted);
            Assert.AreSame(MidStoryCatalog.First, next);
            Assert.AreEqual(MidStoryCatalog.FirstGiver, next.GiverResidentId);
            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(next.GiverResidentId, quests.IsActive, quests.IsCompleted, 120, out LateStoryStep offered));
            Assert.AreSame(next, offered);
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(LateStoryCatalog.Steps[0].GiverResidentId, quests.IsActive, quests.IsCompleted, 120, out _));
        }

        [Test]
        public void SaveAtLevel120MidwayThroughTheMoon_OffersTheNextMoonStep()
        {
            var done = new HashSet<string> { MidStoryCatalog.LimiarMoon, MidStoryCatalog.ZarafshanGarden, MidStoryCatalog.AreiasThrone };
            var active = new HashSet<string>();
            Assert.IsEmpty(MidStorySaveMigration.MoonStepsToMarkDone(active.Contains, done.Contains));

            LateStoryStep next = MidStorySaveMigration.NextMainStep(done.Contains);
            Assert.AreEqual(MidStoryCatalog.KaggenSummit, next.QuestId);
            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(next.GiverResidentId, active.Contains, done.Contains, 120, out LateStoryStep offered));
            Assert.AreSame(next, offered);
        }

        [Test]
        public void NewSave_NothingChanges()
        {
            var save = new QuestSaveState();
            Assert.AreEqual(0, MidStorySaveMigration.Upgrade(save));
            CollectionAssert.IsEmpty(save.CompletedQuestIds);
            CollectionAssert.IsEmpty(save.ActiveQuests);
            CollectionAssert.IsEmpty(save.PendingRewardQuestIds);

            var quests = new QuestManager();
            quests.RestoreState(new QuestSaveState());
            CollectionAssert.IsEmpty(quests.CompletedQuestIds);
            Assert.AreSame(MidStoryCatalog.First, MidStorySaveMigration.NextMainStep(quests.IsCompleted));

            var current = new QuestSaveState { StorySchema = MidStorySaveMigration.Schema, CompletedQuestIds = new List<string> { LateStoryCatalog.SteppeFoals } };
            Assert.AreEqual(0, MidStorySaveMigration.Upgrade(current), "a save already at the schema is left alone");
            CollectionAssert.AreEqual(new[] { LateStoryCatalog.SteppeFoals }, current.CompletedQuestIds);
        }
    }
}
