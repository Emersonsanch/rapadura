using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// "A Lua que Não Enche" (Docs/HistoriaMeio_20a100.md): the main story between the end of the campaign (Tages and the seal) and the first letter. Points only at things that
    /// exist (residents who stand somewhere, maps, zones, fauna, items), has a line per cousin in both languages with Vó Edna on the radio, opens in order by step and by level,
    /// starts where the campaign ends and hands over to "As Cartas da Estrada".
    /// </summary>
    public class MidStoryCatalogTests
    {
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        private static readonly IReadOnlyList<LateStoryStep> AllSteps = MidStoryCatalog.Steps;
        private static readonly Dictionary<string, LocalizationEntry> Texts = LateStoryTexts.All.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

        private static string HeroName(CharacterId hero) => hero == CharacterId.Icaro ? "Ícaro" : hero.ToString();

        private static OverworldZone ZoneOf(LateStoryStep step) => OverworldLayouts.ForMap(step.MapId).Zone(step.ZoneId);

        private static bool FaunaHas(OverworldZone zone, string enemyId) => zone.Fauna.Any(f =>
            f.Day.Concat(f.Night).Concat(f.Dusk ?? new PeriodSpawnEntry[0]).Any(e => e.EnemyId == enemyId));

        private static bool IsSeal(RegionObjectiveSpec objective) => objective.ObjectiveId == MidStoryCatalog.SealObjectiveId;

        [Test]
        public void Chain_FillsTheGap_FromTheSealToTheFirstLetter_RegionByRegion()
        {
            Assert.That(AllSteps.Count, Is.InRange(12, 16));
            Assert.AreEqual(AllSteps.Count, AllSteps.Select(s => s.QuestId).Distinct().Count(), "unique ids");
            Assert.IsFalse(AllSteps.Any(s => LateStoryCatalog.Steps.Any(l => l.QuestId == s.QuestId)), "no id shared with the letters");

            // Starts where the campaign ends: Vó Edna in the Vila sends the cousins to Tages's hall.
            Assert.AreEqual(4, MidStoryCatalog.First.Region);
            Assert.AreEqual(CityCatalog.VilaZefa, MidStoryCatalog.First.CityId);
            Assert.AreEqual(MidStoryCatalog.FirstGiver, MidStoryCatalog.First.GiverResidentId);
            Assert.AreEqual("map_020_tages", MidStoryCatalog.First.MapId);

            // The second ring of regions 1–4, then every region 5–8 with two or three steps, in order.
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, AllSteps.Skip(1).Take(4).Select(s => s.Region).ToArray(), "the second ring of regions 1–4");
            CollectionAssert.AreEqual(new[] { 5, 6, 7, 8 }, AllSteps.Skip(5).Select(s => s.Region).Distinct().ToArray(), "then regions 5–8, region after region");
            foreach (int region in Enumerable.Range(5, 4))
            {
                Assert.That(MidStoryCatalog.StepsIn(region).Count(), Is.InRange(2, 3), $"region {region}");
            }

            // Ends next to region 9, below the level of the first letter.
            Assert.AreEqual(8, MidStoryCatalog.Last.Region);
            Assert.AreEqual(MidStoryCatalog.FinalQuestId, MidStoryCatalog.Last.QuestId);
            Assert.Less(MidStoryCatalog.Last.MinLevel, LateStoryCatalog.Steps[0].MinLevel);
        }

        [Test]
        public void Chain_HasOneRoot_EachStepRequiresThePreviousOne_AndTheFirstLetterRequiresTheLast()
        {
            Assert.IsNull(MidStoryCatalog.First.Quest.PrerequisiteQuestId, "the root");
            string previous = null;
            int level = 0;
            foreach (LateStoryStep step in AllSteps)
            {
                Assert.AreEqual(previous, step.Quest.PrerequisiteQuestId, $"{step.QuestId} requires the step right before it");
                Assert.Greater(step.MinLevel, level, $"{step.QuestId}: level gate grows");
                previous = step.QuestId;
                level = step.MinLevel;
            }

            LateStoryStep firstLetter = LateStoryCatalog.Steps[0];
            Assert.AreEqual(MidStoryCatalog.FinalQuestId, firstLetter.Quest.PrerequisiteQuestId, "the letters open after the full moon");
            CollectionAssert.AreEqual(new[] { MidStoryCatalog.FinalQuestId }, LateStoryCatalog.GetQuest(firstLetter.QuestId).PrerequisiteQuestIds.ToArray());
            CollectionAssert.AreEqual(AllSteps.Concat(LateStoryCatalog.Steps).Select(s => s.QuestId).ToArray(), LateStoryCatalog.WholeStory.Select(s => s.QuestId).ToArray());
        }

        [Test]
        public void TheRoot_IsTheSealOfTheCampaign_AndTages()
        {
            LateStoryStep root = MidStoryCatalog.First;
            RegionObjectiveSpec seal = root.Quest.Objectives.Single(IsSeal);
            Assert.AreEqual(QuestObjectiveType.KillEnemyType, seal.Type);
            Assert.AreEqual(MidStoryCatalog.SealEnemyId, seal.TargetId);
            Assert.AreEqual(1, seal.RequiredAmount, "a boss is fought once (QuestManager credits it when it already fell)");

            EnemyDefinition tages = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{MidStoryCatalog.SealEnemyId}");
            Assert.IsNotNull(tages);
            Assert.IsTrue(tages.IsBoss, "Tages, the boss of the campaign's end");

            // The same fight that ends the Convocação (Act V) and the epilogue.
            QuestDefinition gathering = Resources.Load<QuestDefinition>("Quests/quest_convergence_the_gathering");
            Assert.IsNotNull(gathering);
            Assert.IsTrue(gathering.Objectives.Any(o => o.Type == QuestObjectiveType.KillEnemyType && o.TargetId == MidStoryCatalog.SealEnemyId));

            Assert.IsTrue(AllSteps.Skip(1).SelectMany(s => s.Quest.Objectives).All(o => !IsSeal(o)), "only the root asks for the seal");
        }

        [Test]
        public void EveryStep_PointsAtAnExistingCityResidentMapZoneAndEnemy()
        {
            foreach (LateStoryStep step in AllSteps)
            {
                CitySpec city = CityCatalog.Get(step.CityId);
                Assert.IsNotNull(city, step.CityId);
                CityResidentSpec giver = city.Residents.FirstOrDefault(r => r.ResidentId == step.GiverResidentId);
                Assert.IsNotNull(giver, $"{step.GiverResidentId} lives in {step.CityId}");
                Assert.IsTrue(string.IsNullOrEmpty(giver.ShopId), $"{step.GiverResidentId} keeps a counter: stepping up opens the shop and they would never speak");
                Assert.IsNotNull(CityTownRecipes.LayoutFor(step.CityId).LotOfResident(step.GiverResidentId), $"{step.GiverResidentId} stands somewhere in {step.CityId}");

                MapDefinition map = MapCatalog.Get(step.MapId);
                Assert.IsNotNull(map, step.MapId);
                OverworldZone zone = ZoneOf(step);
                Assert.IsNotNull(zone, $"{step.QuestId}: zone {step.ZoneId} of {step.MapId}");
                Assert.AreEqual(step.MapId, zone.MapIdOrOwner, step.QuestId);
                Assert.GreaterOrEqual(step.MinLevel, zone.WalkLevel, $"{step.QuestId}: a hero of the step's level may walk in");
                Assert.LessOrEqual(step.MinLevel, zone.MaxLevel + 2, $"{step.QuestId}: the step is not far above the zone's beasts");
                Assert.AreNotEqual(step.CityId, zone.Id, $"{step.QuestId}: the place is not the giver's own town, where the ReachLocation report could never happen");

                RegionObjectiveSpec reach = step.Quest.Objectives.Single(o => o.Type == QuestObjectiveType.ReachLocation);
                Assert.AreEqual(step.ZoneId, reach.TargetId, step.QuestId);
                foreach (RegionObjectiveSpec kill in step.Quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType && !IsSeal(o)))
                {
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{kill.TargetId}");
                    Assert.IsNotNull(enemy, $"{step.QuestId}: {kill.TargetId}");
                    Assert.IsFalse(enemy.IsBoss, $"{step.QuestId}: a boss is fought once");
                    Assert.Greater(kill.RequiredAmount, 0, step.QuestId);
                    Assert.IsTrue(FaunaHas(zone, kill.TargetId), $"{step.QuestId}: {kill.TargetId} lives in the fauna of {zone.Id}");
                }

                Assert.IsTrue(step.Quest.Objectives.All(o => o.Type == QuestObjectiveType.KillEnemyType || o.Type == QuestObjectiveType.ReachLocation), $"{step.QuestId}: only kinds with a live source");
                Assert.AreEqual(1, step.Quest.Objectives.Count(o => o.Type == QuestObjectiveType.KillEnemyType), $"{step.QuestId}: one fight per step");
            }
        }

        [Test]
        public void Rewards_Resolve_AndGrowTowardsTheLetters()
        {
            int experience = 0;
            foreach (LateStoryStep step in AllSteps)
            {
                Assert.Greater(step.Quest.RewardExperience, experience, $"{step.QuestId}: experience grows");
                experience = step.Quest.RewardExperience;
                Assert.IsTrue(step.Quest.Rewards.All(r => ItemDatabase.GetById(r.ItemId) != null), $"{step.QuestId}: items exist");
                Assert.IsFalse(step.Quest.Rewards.Any(r => r.ItemId == "item_pedra_vida" || r.ItemId == "item_pedra_caos"), step.QuestId);
                Assert.That(step.Quest.Rewards.Where(r => r.ItemId == "item_gold").Sum(r => r.Quantity), Is.InRange(30, 200), step.QuestId);

                QuestDefinition quest = LateStoryCatalog.GetQuest(step.QuestId);
                Assert.IsNotNull(quest, step.QuestId);
                Assert.AreEqual(QuestType.MainStory, quest.QuestType);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), step.QuestId);
                Assert.AreSame(quest, CityQuestCatalog.GetQuest(step.QuestId), "found through the city catalog (dialogue, quest log, save)");
            }

            Assert.Less(experience, LateStoryCatalog.Steps[0].Quest.RewardExperience, "the first letter pays more");
            Assert.IsNotNull(CityQuestCatalog.GetAllQuests().FirstOrDefault(q => q.QuestId == MidStoryCatalog.FinalQuestId), "in the quest log");
        }

        [Test]
        public void EveryStep_HasADifferentLinePerCousin_InBothLanguages_WithVoEdnaOnTheRadio()
        {
            foreach (LateStoryStep step in AllSteps)
            {
                var pt = new HashSet<string>();
                var en = new HashSet<string>();
                foreach (CharacterId hero in Heroes)
                {
                    string key = step.OfferKey(hero);
                    Assert.IsTrue(Texts.TryGetValue(key, out LocalizationEntry entry), key);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), $"pt {key}");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), $"en {key}");
                    Assert.AreNotEqual(entry.En, entry.PtBR, $"{key} is not translated");
                    StringAssert.Contains(HeroName(hero), entry.PtBR, key);
                    StringAssert.Contains(HeroName(hero), entry.En, key);
                    Assert.IsTrue(pt.Add(entry.PtBR.Replace(HeroName(hero), "*")), $"{key}: the same line with the name swapped (pt)");
                    Assert.IsTrue(en.Add(entry.En.Replace(HeroName(hero), "*")), $"{key}: the same line with the name swapped (en)");
                }

                foreach (string suffix in new[] { ".title", ".description", ".journal", ".active" })
                {
                    Assert.IsTrue(Texts.TryGetValue(step.KeyBase + suffix, out LocalizationEntry entry), step.KeyBase + suffix);
                    Assert.AreNotEqual(entry.En, entry.PtBR, $"{step.KeyBase + suffix} is not translated");
                }

                foreach (RegionObjectiveSpec objective in step.Quest.Objectives)
                {
                    Assert.IsTrue(Texts.ContainsKey(step.Quest.ObjectiveKey(objective.ObjectiveId)), step.Quest.ObjectiveKey(objective.ObjectiveId));
                }

                LocalizationEntry journal = Texts[step.KeyBase + ".journal"];
                StringAssert.Contains("Vó Edna", journal.PtBR, step.QuestId);
                StringAssert.Contains("Grandma Edna", journal.En, step.QuestId);
            }

            foreach (CharacterId hero in Heroes)
            {
                LocalizationEntry tooLow = Texts[MidStoryCatalog.TooLowKeyBase + LateStoryCatalog.HeroKey(hero)];
                StringAssert.Contains(HeroName(hero), tooLow.PtBR);
                StringAssert.Contains(HeroName(hero), tooLow.En);
                StringAssert.DoesNotContain("carta", tooLow.PtBR, "the letters have not been found yet");
            }

            Assert.IsTrue(AllSteps.Any(s => Texts[s.KeyBase + ".description"].PtBR.Contains("Vô Jorge")), "Vô Jorge is remembered along the way");
            StringAssert.Contains("Caderno", Texts[MidStoryCatalog.Last.KeyBase + ".journal"].PtBR, "the last step finds what the first letter starts from");
        }

        [Test]
        public void Decide_OpensTheChainInOrder_AndTheRootStaysSilentUnderItsLevel()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            LateStoryStep root = MidStoryCatalog.First;
            LateStoryStep second = AllSteps[1];

            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(root.GiverResidentId, active.Contains, done.Contains, 1, out _), "Vó Edna says her usual line to a level 1 hero");
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(root.GiverResidentId, active.Contains, done.Contains, root.MinLevel - 1, out _));
            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(root.GiverResidentId, active.Contains, done.Contains, root.MinLevel, out LateStoryStep offered));
            Assert.AreSame(root, offered);
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(second.GiverResidentId, active.Contains, done.Contains, 500, out _), "the next step waits for the previous one");

            done.Add(root.QuestId);
            Assert.AreEqual(LateStoryTalk.TooLow, LateStoryCatalog.Decide(second.GiverResidentId, active.Contains, done.Contains, second.MinLevel - 1, out _), "under the level");
            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(second.GiverResidentId, active.Contains, done.Contains, second.MinLevel, out LateStoryStep next));
            Assert.AreSame(second, next);
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(root.GiverResidentId, active.Contains, done.Contains, 500, out _), "her part is done");

            LateStoryStep firstLetter = LateStoryCatalog.Steps[0];
            done.UnionWith(AllSteps.Take(AllSteps.Count - 1).Select(s => s.QuestId));
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(firstLetter.GiverResidentId, active.Contains, done.Contains, 500, out _), "the letters wait for the full moon");
            done.Add(MidStoryCatalog.FinalQuestId);
            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(firstLetter.GiverResidentId, active.Contains, done.Contains, firstLetter.MinLevel, out LateStoryStep letter));
            Assert.AreSame(firstLetter, letter);
        }

        [Test]
        public void Talk_CreditsTheSealWhenTagesAlreadyFell_AndTheHallFinishesTheRoot()
        {
            LateStoryStep root = MidStoryCatalog.First;
            var quests = new QuestManager { DefeatedUniqueEnemyLookup = id => id == MidStoryCatalog.SealEnemyId };
            Assert.AreEqual(root.OfferKey(CharacterId.Maria), LateStoryCatalog.Talk(quests, root.GiverResidentId, CharacterId.Maria, root.MinLevel));
            Assert.IsTrue(quests.IsActive(root.QuestId));
            Assert.IsTrue(quests.GetProgress(root.QuestId).Objectives.Single(o => o.ObjectiveId == MidStoryCatalog.SealObjectiveId).IsComplete, "the campaign already faced Tages");
            Assert.AreEqual(root.ActiveKey, LateStoryCatalog.Talk(quests, root.GiverResidentId, CharacterId.Joaquim, root.MinLevel));

            quests.ReportLocationReached(root.ZoneId);
            Assert.IsTrue(quests.IsCompleted(root.QuestId));

            // The next giver, under her level, says the mid story's own "too low" line (no letter in it).
            LateStoryStep second = AllSteps[1];
            Assert.AreEqual(MidStoryCatalog.TooLowKeyBase + "lavine", LateStoryCatalog.Talk(quests, second.GiverResidentId, CharacterId.Lavine, second.MinLevel - 1));
            Assert.AreEqual(second.OfferKey(CharacterId.Lavine), LateStoryCatalog.Talk(quests, second.GiverResidentId, CharacterId.Lavine, second.MinLevel));
        }

        [Test]
        public void Talk_WithoutTages_TheSealWaitsForTheFight()
        {
            LateStoryStep root = MidStoryCatalog.First;
            var quests = new QuestManager { DefeatedUniqueEnemyLookup = _ => false };
            LateStoryCatalog.Talk(quests, root.GiverResidentId, CharacterId.Icaro, root.MinLevel);
            quests.ReportLocationReached(root.ZoneId);
            Assert.IsTrue(quests.IsActive(root.QuestId), "reaching the hall is not enough: the seal is remade with Tages");
            quests.UpdateObjectiveProgress(QuestObjectiveType.KillEnemyType, MidStoryCatalog.SealEnemyId, 1);
            Assert.IsTrue(quests.IsCompleted(root.QuestId));
        }

        [Test]
        public void TheStory_SpeaksBeforeTheCityQuests_OfAGiverWhoHasBoth()
        {
            // Liyongo hands out a city quest too; once the baobabs are done, at the step's level, the story speaks first.
            LateStoryStep mangrove = MidStoryCatalog.FindStep(MidStoryCatalog.CostaMangrove);
            Assert.IsTrue(CityQuestCatalog.QuestsOf(mangrove.GiverResidentId).Any(), "Liyongo has his own quest");
            var quests = new QuestManager();
            quests.RestoreState(new QuestSaveState { CompletedQuestIds = AllSteps.TakeWhile(s => s != mangrove).Select(s => s.QuestId).ToList() });
            Assert.AreEqual(mangrove.OfferKey(CharacterId.Maithe), CityResidentTalk.QuestLine(quests, mangrove.GiverResidentId, CharacterId.Maithe, mangrove.MinLevel));

            // Without the step before it, his city quest is untouched.
            Assert.AreNotEqual(mangrove.OfferKey(CharacterId.Maithe), CityResidentTalk.QuestLine(new QuestManager(), mangrove.GiverResidentId, CharacterId.Maithe, mangrove.MinLevel));
        }
    }
}
