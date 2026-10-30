using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Pets;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Docs/Narrative.md §9 connections: the <see cref="DialogueConditionType.StoryBeatSeen"/> condition and the village
    /// "news from the road" (<see cref="VillageStoryNews"/>), Vó Edna's kitchen remembering the region the cousins came
    /// back from, and Caramelo's cuddle lines by campaign stage.
    /// </summary>
    public class NarrativeConnectionsTests
    {
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        private static readonly BiomeType[] Regions = (BiomeType[])Enum.GetValues(typeof(BiomeType));

        private HashSet<StoryBeat> _seen;
        private DialogueManager _dialogue;

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
            CarameloCuddle.ResetForTests();
            _seen = new HashSet<StoryBeat>();
            _dialogue = new DialogueManager
            {
                Context = new QuestDialogueContext(() => null, _ => null, () => null) { StoryBeatProvider = _seen.Contains },
            };
            _dialogue.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _dialogue.Shutdown();
            CarameloCuddle.ResetForTests();
            EventBus.Clear();
            ServiceLocator.Clear();
        }

        // ------------------------------------------------------------------
        // StoryBeatSeen
        // ------------------------------------------------------------------

        [Test]
        public void StoryBeatSeen_ReadsTheBeatsSeen_ByIdOrName()
        {
            var condition = new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = "interlude_desert" };
            Assert.IsFalse(_dialogue.IsConditionMet(condition));

            _seen.Add(StoryBeat.InterludeDesert);
            Assert.IsTrue(_dialogue.IsConditionMet(condition));
            Assert.IsTrue(_dialogue.IsConditionMet(new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = "InterludeDesert" }));
            Assert.IsTrue(_dialogue.IsConditionMet(new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = "INTERLUDE_DESERT" }));
            Assert.IsFalse(_dialogue.IsConditionMet(new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = "interlude_desert", Negate = true }));
            Assert.IsFalse(_dialogue.IsConditionMet(new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = "ending" }));
            Assert.IsFalse(_dialogue.IsConditionMet(new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = "not_a_beat" }), "unknown key");
            Assert.IsFalse(_dialogue.IsConditionMet(new DialogueCondition { Type = DialogueConditionType.StoryBeatSeen, Key = "" }), "empty key");
        }

        [Test]
        public void StoryBeatSeen_DefaultsToTheRegisteredStoryProgress_AndIsFalseWithoutOne()
        {
            var context = new QuestDialogueContext(() => null, _ => null, () => null);
            Assert.IsFalse(context.HasSeenStoryBeat(StoryBeat.Prologue), "no StoryProgress registered");

            var story = new StoryProgress();
            story.Initialize();
            ServiceLocator.Register(story);
            story.MarkSeen(StoryBeat.Prologue);
            Assert.IsTrue(context.HasSeenStoryBeat(StoryBeat.Prologue));
            Assert.IsFalse(context.HasSeenStoryBeat(StoryBeat.None));
        }

        [Test]
        public void StoryBeats_TryParseAndLatestSeen()
        {
            Assert.IsTrue(StoryBeats.TryParse("ending", out StoryBeat ending));
            Assert.AreEqual(StoryBeat.Ending, ending);
            Assert.IsFalse(StoryBeats.TryParse(null, out StoryBeat none));
            Assert.AreEqual(StoryBeat.None, none);

            Assert.AreEqual(StoryBeat.None, StoryBeats.LatestSeen(null));
            Assert.IsNull(StoryBeats.LastRegionCleared(_seen.Contains));
            _seen.Add(StoryBeat.Prologue);
            Assert.IsNull(StoryBeats.LastRegionCleared(_seen.Contains), "the prologue clears no region");
            _seen.Add(StoryBeat.InterludeForest);
            Assert.AreEqual(BiomeType.Forest, StoryBeats.LastRegionCleared(_seen.Contains));
            _seen.Add(StoryBeat.InterludeMountain);
            Assert.AreEqual(BiomeType.Mountain, StoryBeats.LastRegionCleared(_seen.Contains), "the newest beat wins");
            _seen.Add(StoryBeat.Ending);
            Assert.AreEqual(BiomeType.Caves, StoryBeats.LastRegionCleared(_seen.Contains));
        }

        // ------------------------------------------------------------------
        // Village news
        // ------------------------------------------------------------------

        [Test]
        public void VillageNews_EveryVillagerHasALinePerBeat_InBothLanguages()
        {
            var keys = VillageStoryNewsTexts.All.ToDictionary(e => e.Key);
            Assert.IsTrue(keys.ContainsKey(VillageStoryNews.ChoiceKey));
            foreach (string npc in VillageStoryNews.NpcKeys)
            {
                Assert.IsNotNull(VillageNpcCatalog.Find(npc), npc + " is a villager");
                foreach (StoryBeat beat in VillageStoryNews.NewestFirst)
                {
                    string key = VillageStoryNews.LineKey(npc, beat);
                    Assert.IsTrue(keys.ContainsKey(key), key);
                    Assert.IsFalse(string.IsNullOrEmpty(keys[key].En), key + " en");
                    Assert.IsFalse(string.IsNullOrEmpty(keys[key].PtBR), key + " pt-BR");
                }
            }
        }

        [Test]
        public void VillageNews_AppearsOnlyAfterTheForestInterlude_AndPicksTheNewestBeat()
        {
            foreach (string npcKey in VillageStoryNews.NpcKeys)
            {
                VillageNpcSpec npc = VillageNpcCatalog.Find(npcKey);
                DialogueDefinition asset = Resources.Load<DialogueDefinition>(npc.DialogueResourcePath);
                DialogueDefinition dialogue = VillageNpcCatalog.LoadDialogue(npc);
                Assert.IsNotNull(dialogue.GetNode(VillageStoryNews.NewsNodeId), npcKey);
                Assert.IsNull(asset.GetNode(VillageStoryNews.NewsNodeId), "the seeded asset itself is never changed");
                Assert.AreEqual(asset.DialogueId, dialogue.DialogueId);
                Assert.AreSame(dialogue, VillageNpcCatalog.LoadDialogue(npc), "cached copy");

                // Every node that can say goodbye offers the news first, gated on the Forest interlude.
                int offered = 0;
                foreach (DialogueNode node in dialogue.Nodes)
                {
                    int goodbye = node.Choices.FindLastIndex(c => string.IsNullOrEmpty(c.NextNodeId));
                    if (goodbye <= 0 || node.Id.StartsWith(VillageStoryNews.NewsNodeId))
                    {
                        continue;
                    }

                    DialogueChoice news = node.Choices[goodbye - 1];
                    Assert.AreEqual(VillageStoryNews.ChoiceKey, news.TextKey, npcKey + "/" + node.Id);
                    Assert.AreEqual(VillageStoryNews.NewsNodeId, news.NextNodeId);
                    Assert.AreEqual(DialogueConditionType.StoryBeatSeen, news.Conditions.Single().Type);
                    offered++;
                }

                Assert.Greater(offered, 0, npcKey + " offers the news somewhere");
            }

            // Played through the manager: the survivor's greeting comes straight from the hub without a quest.
            DialogueDefinition survivor = VillageNpcCatalog.LoadDialogue(VillageNpcCatalog.Find("village_survivor"));
            _dialogue.StartDialogue(survivor);
            Assert.IsFalse(_dialogue.AvailableChoices.Any(c => c.TextKey == VillageStoryNews.ChoiceKey), "no news before the Forest interlude");
            _dialogue.EndDialogue();

            _seen.Add(StoryBeat.Prologue);
            _seen.Add(StoryBeat.InterludeForest);
            _seen.Add(StoryBeat.InterludeDesert);
            _dialogue.StartDialogue(survivor);
            int index = _dialogue.AvailableChoices.ToList().FindIndex(c => c.TextKey == VillageStoryNews.ChoiceKey);
            Assert.GreaterOrEqual(index, 0, "news offered on the greeting");
            Assert.AreEqual(VillageStoryNews.GoodbyeKey, _dialogue.AvailableChoices[_dialogue.AvailableChoices.Count - 1].TextKey, "goodbye stays last");
            _dialogue.SelectChoice(index);
            Assert.AreEqual(VillageStoryNews.BeatNodeId(StoryBeat.InterludeDesert), _dialogue.CurrentNode.Id, "the newest beat seen wins");
            Assert.AreEqual(VillageStoryNews.LineKey("village_survivor", StoryBeat.InterludeDesert), _dialogue.CurrentNode.TextKey);
            Assert.AreEqual(1, _dialogue.AvailableChoices.Count, "the news line can be closed");
            _dialogue.EndDialogue();

            _seen.Add(StoryBeat.InterludeMountain);
            _seen.Add(StoryBeat.Ending);
            _dialogue.StartDialogue(survivor);
            _dialogue.SelectChoice(_dialogue.AvailableChoices.ToList().FindIndex(c => c.TextKey == VillageStoryNews.ChoiceKey));
            Assert.AreEqual(VillageStoryNews.BeatNodeId(StoryBeat.Ending), _dialogue.CurrentNode.Id);
            _dialogue.EndDialogue();
        }

        [Test]
        public void VillageNews_GatheringPointIsLeftAlone()
        {
            VillageNpcSpec gathering = VillageNpcCatalog.Find("maria_gathering_point");
            Assert.IsFalse(VillageStoryNews.HasNews(gathering.NpcKey));
            Assert.IsNull(VillageNpcCatalog.LoadDialogue(gathering).GetNode(VillageStoryNews.NewsNodeId));
            Assert.IsNull(VillageStoryNews.WithNews(null, "village_survivor"));
        }

        // ------------------------------------------------------------------
        // Kitchen
        // ------------------------------------------------------------------

        [Test]
        public void Kitchen_GrandmaRemembersTheRegion_PerCousin()
        {
            foreach (BiomeType region in Regions)
            {
                Assert.IsNotNull(KitchenTexts.GetFallback(KitchenTexts.AfterRegionKey(region, null)), region + " family line");
                foreach (CharacterId hero in Heroes)
                {
                    string key = KitchenTexts.AfterRegionKey(region, hero);
                    Assert.IsNotNull(KitchenTexts.GetFallback(key), key);
                    Assert.AreEqual(KitchenTexts.GetFallback(key), KitchenPresenter.GrandmaLine(hero, null, null, region));
                }
            }

            Assert.AreEqual("ui.kitchen.grandma.after_desert_icaro", KitchenTexts.AfterRegionKey(BiomeType.Desert, CharacterId.Icaro));
            Assert.AreEqual(KitchenTexts.GetFallback("ui.kitchen.grandma.after_caves"), KitchenPresenter.GrandmaLine(null, null, null, BiomeType.Caves));
        }

        [Test]
        public void Kitchen_LessonWins_AndNoRegionKeepsTheGreeting()
        {
            MealRecipeSpec favourite = MealCatalog.Recipes.First();
            Assert.AreEqual(KitchenPresenter.GrandmaLine(CharacterId.Maria, favourite, null),
                KitchenPresenter.GrandmaLine(CharacterId.Maria, favourite, null, BiomeType.Forest), "a lesson still wins");
            Assert.AreEqual(KitchenPresenter.GrandmaLine(CharacterId.Lavine, null, null),
                KitchenPresenter.GrandmaLine(CharacterId.Lavine, null, null, null), "before the Forest interlude: the usual greeting");
        }

        [Test]
        public void Kitchen_FirstVisitRemembers_ThenAlternatesWithTheGreeting()
        {
            Assert.AreEqual(BiomeType.Desert, KitchenPresenter.CameFromForVisit(BiomeType.Desert, 0));
            Assert.IsNull(KitchenPresenter.CameFromForVisit(BiomeType.Desert, 1));
            Assert.AreEqual(BiomeType.Desert, KitchenPresenter.CameFromForVisit(BiomeType.Desert, 2));
            Assert.IsNull(KitchenPresenter.CameFromForVisit(null, 0));
        }

        // ------------------------------------------------------------------
        // Caramelo
        // ------------------------------------------------------------------

        [Test]
        public void Caramelo_EveryCousinHasAStageLinePerRegion_SpokenByThem()
        {
            var keys = new HashSet<string>(CarameloTexts.Entries.Select(e => e.Key));
            foreach (CharacterId hero in Heroes)
            {
                foreach (BiomeType region in Regions)
                {
                    string key = CarameloTexts.StageLineKey(hero, region);
                    Assert.IsTrue(keys.Contains(key), key);
                    StringAssert.StartsWith(CharacterRegistry.Get(hero).DisplayName, CarameloTexts.GetFallback(key), key);
                }
            }
        }

        [Test]
        public void Caramelo_StageLineJoinsTheRotation_First()
        {
            Assert.AreEqual(CarameloTexts.CuddleLineKey(CharacterId.Joaquim, 1), CarameloTexts.CuddleLineKey(CharacterId.Joaquim, 1, null), "no region: the usual rotation");

            Assert.AreEqual(CarameloTexts.StageLineKey(CharacterId.Lavine, BiomeType.Caves), CarameloTexts.CuddleLineKey(CharacterId.Lavine, 0, BiomeType.Caves));
            Assert.AreEqual(CarameloTexts.CuddleLineKey(CharacterId.Lavine, 0), CarameloTexts.CuddleLineKey(CharacterId.Lavine, 1, BiomeType.Caves));
            Assert.AreEqual(CarameloTexts.CuddleLineKey(CharacterId.Lavine, 1), CarameloTexts.CuddleLineKey(CharacterId.Lavine, 2, BiomeType.Caves));
            Assert.AreEqual(CarameloTexts.StageLineKey(CharacterId.Lavine, BiomeType.Caves), CarameloTexts.CuddleLineKey(CharacterId.Lavine, 3, BiomeType.Caves));
            Assert.AreEqual(CarameloTexts.CuddleLineKey(CharacterId.Lavine, 2, BiomeType.Caves), CarameloTexts.CuddleLineKey(CharacterId.Lavine, -1, BiomeType.Caves), "negative index wraps");

            Assert.AreEqual(CarameloTexts.Intro, CarameloCuddle.Cuddle(CharacterId.Maria, false, BiomeType.Desert), "Vó Edna introduces him first");
            Assert.AreEqual(CarameloTexts.StageLineKey(CharacterId.Maria, BiomeType.Desert), CarameloCuddle.Cuddle(CharacterId.Maria, true, BiomeType.Desert));
            Assert.AreEqual(CarameloTexts.CuddleLineKey(CharacterId.Maria, 0), CarameloCuddle.Cuddle(CharacterId.Maria, true, BiomeType.Desert));
            Assert.IsTrue(CarameloCuddle.BuffPending);
        }
    }
}
