using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// What a resident's post does when the cousins step up (<see cref="CityResidentTalk"/>): a keeper with a quest speaks before the shop, every city quest can be offered, and
    /// the last step of the main story ends on sitting by the chair.
    /// </summary>
    public class CityResidentTalkTests
    {
        private const string Ammo = "ammo_caravaneiro";
        private const string Adda = "adda_roda";
        private const string Greeting = "city.resident.x.line.maria";

        private static QuestManager NewQuests(params string[] completed)
        {
            var quests = new QuestManager();
            quests.RestoreState(new QuestSaveState { CompletedQuestIds = completed.ToList() });
            return quests;
        }

        /// <summary>Every step of the main story after the campaign: "A Lua que Não Enche" and the letters.</summary>
        private static string[] WholeStory() => LateStoryCatalog.WholeStory.Select(s => s.QuestId).ToArray();

        private static string[] MidStory() => MidStoryCatalog.Steps.Select(s => s.QuestId).ToArray();

        private static CityQuestSpec Spec(string questId) => CityQuestCatalog.FindSpec(questId);

        private static CityResidentPlan KeeperPlan(string questLine, bool greeted) => CityResidentTalk.Plan(true, questLine, Greeting, greeted);

        [Test]
        public void Keeper_WithAQuest_OffersIt_AndKeepsTheShopForAfter()
        {
            QuestManager quests = NewQuests();
            string line = CityResidentTalk.QuestLine(quests, Ammo, CharacterId.Maria, 420);
            Assert.AreEqual(Spec(CityQuestCatalog.CaisColunaStowaways).OfferKey, line);
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.CaisColunaStowaways));

            CityResidentPlan plan = KeeperPlan(line, true);
            Assert.AreEqual(CityResidentAction.Converse, plan.Action, "speaks first, even after the first visit");
            Assert.AreEqual(line, plan.LineKey);
            Assert.IsFalse(plan.IsGreeting);
        }

        [Test]
        public void Keeper_WithARunningQuest_ShowsHowItGoes()
        {
            QuestManager quests = NewQuests();
            CityResidentTalk.QuestLine(quests, Ammo, CharacterId.Maria, 420);
            string line = CityResidentTalk.QuestLine(quests, Ammo, CharacterId.Maria, 420);
            Assert.AreEqual(Spec(CityQuestCatalog.CaisColunaStowaways).ActiveKey, line);
            Assert.AreEqual(CityResidentAction.Converse, KeeperPlan(line, true).Action);
        }

        [Test]
        public void Keeper_WithNothingPending_GreetsOnTheFirstVisit_ThenOpensTheShop()
        {
            QuestManager quests = NewQuests(CityQuestCatalog.CaisColunaStowaways);
            Assert.IsNull(CityResidentTalk.QuestLine(quests, Ammo, CharacterId.Maria, 420), "his quest is done");

            CityResidentPlan first = KeeperPlan(null, false);
            Assert.AreEqual(CityResidentAction.Converse, first.Action);
            Assert.AreEqual(Greeting, first.LineKey);
            Assert.IsTrue(first.IsGreeting);

            Assert.AreEqual(CityResidentAction.OpenShop, KeeperPlan(null, true).Action);
            Assert.AreEqual(CityResidentAction.OpenShop, CityResidentTalk.Plan(true, null, null, false).Action, "a counter with no keeper");
        }

        [Test]
        public void Resident_WithoutACounter_SaysTheirLine()
        {
            CityResidentPlan usual = CityResidentTalk.Plan(false, null, Greeting, true);
            Assert.AreEqual(CityResidentAction.Say, usual.Action);
            Assert.AreEqual(Greeting, usual.LineKey);
            Assert.AreEqual("quest.line", CityResidentTalk.Plan(false, "quest.line", Greeting, false).LineKey);
            Assert.AreEqual(CityResidentAction.Nothing, CityResidentTalk.Plan(false, null, null, false).Action);
        }

        [Test]
        public void GreetedMark_IsPerResidentAndPerCousin()
        {
            Assert.AreEqual("city.greeted.mozak.lavine", CityResidentTalk.GreetedMark("mozak", CharacterId.Lavine));
            Assert.AreEqual("city.greeted.mozak.any", CityResidentTalk.GreetedMark("mozak", null));
        }

        [Test]
        public void AmmoThenAdda_TheChainNoLongerLocks()
        {
            var quests = new QuestManager();
            CityQuestCatalog.RegisterAll(quests);

            Assert.IsNull(CityResidentTalk.QuestLine(quests, Adda, CharacterId.Maithe, 420), "Adda waits for Ammo");
            Assert.AreEqual(Spec(CityQuestCatalog.CaisColunaStowaways).OfferKey, CityResidentTalk.QuestLine(quests, Ammo, CharacterId.Maithe, 420));

            CityQuestSpec stowaways = Spec(CityQuestCatalog.CaisColunaStowaways);
            foreach (RegionObjectiveSpec objective in stowaways.Quest.Objectives)
            {
                quests.UpdateObjectiveProgress(objective.Type, objective.TargetId, objective.RequiredAmount);
            }

            Assert.IsTrue(quests.IsCompleted(CityQuestCatalog.CaisColunaStowaways));
            Assert.AreEqual(Spec(CityQuestCatalog.CaisColunaBirds).OfferKey, CityResidentTalk.QuestLine(quests, Adda, CharacterId.Maithe, 420));
            Assert.IsTrue(quests.IsActive(CityQuestCatalog.CaisColunaBirds));
        }

        [Test]
        public void TheStoryLinesThatHandOutNothing_NeverHideACityQuest()
        {
            // Under the level of the story's first step, Soslan still hands out his own quest.
            QuestManager young = NewQuests();
            Assert.AreEqual(Spec(CityQuestCatalog.SoslanWolves).OfferKey, CityResidentTalk.QuestLine(young, "soslan", CharacterId.Joaquim, 1));

            // The whole chain closed: Adda's closing line used to hide his city quest for good.
            QuestManager closed = NewQuests(WholeStory().Append(CityQuestCatalog.CaisColunaStowaways).ToArray());
            Assert.AreEqual(Spec(CityQuestCatalog.CaisColunaBirds).OfferKey, CityResidentTalk.QuestLine(closed, Adda, CharacterId.Maria, 500));
            Assert.AreEqual(Spec(CityQuestCatalog.CaisColunaBirds).ActiveKey, CityResidentTalk.QuestLine(closed, Adda, CharacterId.Maria, 500));

            // Adda's quests on the road come after the birds; with all of them done, nothing is left.
            QuestManager allDone = NewQuests(WholeStory().Append(CityQuestCatalog.CaisColunaStowaways)
                .Concat(CityQuestCatalog.QuestsOf(Adda).Select(q => q.QuestId)).ToArray());
            Assert.AreEqual(LateStoryCatalog.FinaleKeyBase + "maria", CityResidentTalk.QuestLine(allDone, Adda, CharacterId.Maria, 500), "the closing line is back when nothing is left");
        }

        [Test]
        public void TheStoryStillSpeaksBeforeTheCityQuests()
        {
            LateStoryStep first = LateStoryCatalog.Steps[0];
            Assert.AreEqual("soslan", first.GiverResidentId);
            QuestManager quests = NewQuests(MidStory()); // the first letter requires the full moon of the mid story
            Assert.AreEqual(first.OfferKey(CharacterId.Lavine), CityResidentTalk.QuestLine(quests, "soslan", CharacterId.Lavine, first.MinLevel));
            Assert.IsFalse(quests.IsActive(CityQuestCatalog.SoslanWolves));
        }

        [Test]
        public void EveryCityQuest_CanBeOffered_BySomeoneStandingInTheirTown()
        {
            foreach (CityQuestSpec spec in CityQuestCatalog.Quests)
            {
                CitySpec city = CityCatalog.Get(spec.CityId);
                Assert.IsNotNull(city, spec.CityId);
                Assert.IsTrue(city.Residents.Any(r => r.ResidentId == spec.GiverResidentId), $"{spec.QuestId}: {spec.GiverResidentId} lives in {spec.CityId}");

                CityLot lot = CityTownRecipes.LayoutFor(spec.CityId).LotOfResident(spec.GiverResidentId);
                Assert.IsNotNull(lot, $"{spec.QuestId}: {spec.GiverResidentId} stands nowhere in {spec.CityId}");

                string prerequisite = spec.Quest.PrerequisiteQuestId;
                Assert.IsTrue(string.IsNullOrEmpty(prerequisite) || CityQuestCatalog.IsCityQuest(prerequisite), $"{spec.QuestId}: unknown prerequisite {prerequisite}");

                var earlier = new List<string>();
                if (!string.IsNullOrEmpty(prerequisite))
                {
                    earlier.Add(prerequisite);
                }

                // The giver's earlier quests are done, so this is the one they are up to.
                earlier.AddRange(CityQuestCatalog.QuestsOf(spec.GiverResidentId).TakeWhile(q => q != spec).Select(q => q.QuestId));
                // A quest on the road is offered from its level on (from level 1 for the city's own).
                AssertOffered(spec, lot, NewQuests(earlier.ToArray()), Mathf.Max(1, spec.MinLevel));
                AssertOffered(spec, lot, NewQuests(earlier.Concat(WholeStory()).Distinct().ToArray()), 500);
            }
        }

        private static void AssertOffered(CityQuestSpec spec, CityLot lot, QuestManager quests, int level)
        {
            string line = CityResidentTalk.QuestLine(quests, spec.GiverResidentId, CharacterId.Lavine, level);
            Assert.AreEqual(spec.OfferKey, line, $"{spec.QuestId}, level {level}");
            Assert.IsTrue(quests.IsActive(spec.QuestId), spec.QuestId);

            CityResidentPlan plan = CityResidentTalk.Plan(!string.IsNullOrEmpty(lot.ShopId), line, Greeting, true);
            Assert.AreEqual(string.IsNullOrEmpty(lot.ShopId) ? CityResidentAction.Say : CityResidentAction.Converse, plan.Action, spec.QuestId);
            Assert.AreEqual(line, plan.LineKey, spec.QuestId);
        }

        // ------------------------------------------------------------------
        // The conversation
        // ------------------------------------------------------------------

        private static DialogueDefinition Conversation(string lineKey)
        {
            var definition = ScriptableObject.CreateInstance<DialogueDefinition>();
            definition.SetDataForTests(CityResidentTalk.DialogueId(Ammo), CityResidentTalk.LineNodeId, CityResidentTalk.BuildNodes(Ammo, "city.resident.ammo.name", lineKey));
            return definition;
        }

        [Test]
        public void Conversation_ShopChoice_RaisesTheFlag_AndEnds()
        {
            DialogueDefinition definition = Conversation("some.line");
            var manager = new DialogueManager();
            try
            {
                manager.StartDialogue(definition);
                Assert.AreEqual("some.line", manager.CurrentNode.TextKey);
                Assert.AreEqual(2, manager.AvailableChoices.Count);
                Assert.AreEqual(CityTexts.TalkOpenShop, manager.AvailableChoices[0].TextKey);
                Assert.IsFalse(manager.HasFlag(CityResidentTalk.OpenShopFlag));

                manager.SelectChoice(0);
                Assert.IsFalse(manager.IsActive);
                Assert.IsTrue(manager.HasFlag(CityResidentTalk.OpenShopFlag));
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Conversation_Goodbye_LeavesTheShopClosed()
        {
            DialogueDefinition definition = Conversation("some.line");
            var manager = new DialogueManager();
            try
            {
                manager.StartDialogue(definition);
                manager.SelectChoice(1);
                Assert.IsFalse(manager.IsActive);
                Assert.IsFalse(manager.HasFlag(CityResidentTalk.OpenShopFlag));
            }
            finally
            {
                Object.DestroyImmediate(definition);
            }
        }

        [Test]
        public void Conversation_LabelsExist_InBothLanguages()
        {
            foreach (string key in new[] { CityTexts.TalkOpenShop, CityTexts.TalkGoodbye })
            {
                var entry = CityTexts.Entries.Single(e => e.Key == key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
            }
        }

        // ------------------------------------------------------------------
        // The chair
        // ------------------------------------------------------------------

        private static void Walk(OverworldZone from, OverworldZone to) =>
            EventBus.Publish(new AreaChangedEvent("test", from, to, null, null, null, null, false));

        [TearDown]
        public void StopReporting() => LateStoryLocationReporter.Install(null);

        [Test]
        public void FinalStep_EndsOnSitting_NotOnArriving()
        {
            LateStoryStep last = LateStoryCatalog.Last;
            OverworldLayout layout = OverworldLayouts.ForMap(last.MapId);
            OverworldZone porch = layout.Zone(last.ZoneId);
            Assert.IsNotNull(porch);

            QuestManager quests = NewQuests(LateStoryCatalog.Steps.Take(LateStoryCatalog.Steps.Count - 1).Select(s => s.QuestId).ToArray());
            Assert.IsTrue(quests.StartQuest(LateStoryCatalog.GetQuest(last.QuestId)));
            LateStoryLocationReporter.Install(quests);

            Walk(null, porch);
            Assert.IsTrue(quests.IsActive(last.QuestId), "arriving is not enough");

            EventBus.Publish(new RockingChairSatEvent());
            Assert.IsTrue(quests.IsCompleted(last.QuestId));
        }

        [Test]
        public void OtherSteps_StillEndOnArriving()
        {
            LateStoryStep skyRiver = LateStoryCatalog.FindStep(LateStoryCatalog.SkyRiver);
            OverworldZone zone = OverworldLayouts.ForMap(skyRiver.MapId).Zone(skyRiver.ZoneId);
            var quests = new QuestManager();
            quests.RegisterDefinition(LateStoryCatalog.GetQuest(skyRiver.QuestId));
            quests.RestoreState(new QuestSaveState { CompletedQuestIds = new List<string> { skyRiver.Quest.PrerequisiteQuestId } });
            Assert.IsTrue(quests.StartQuest(LateStoryCatalog.GetQuest(skyRiver.QuestId)));
            LateStoryLocationReporter.Install(quests);

            Walk(null, zone);
            RegionObjectiveSpec reach = skyRiver.Quest.Objectives.Single(o => o.Type == QuestObjectiveType.ReachLocation);
            QuestProgress progress = quests.GetProgress(skyRiver.QuestId);
            Assert.IsTrue(progress.Objectives.Single(o => o.ObjectiveId == reach.ObjectiveId).IsComplete);
        }
    }
}
