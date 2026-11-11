using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// "As Cartas da Estrada" (Docs/HistoriaFinal_Cartas.md): the main story of regions 9–16 points only at things that exist (zones, fauna, residents, items), has a
    /// different line per cousin in both languages with the grandparents present, opens in order by step and by level, and ends at the Varanda of map 100.
    /// </summary>
    public class LateStoryCatalogTests
    {
        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        private static readonly IEnumerable<LateStoryStep> AllSteps = LateStoryCatalog.Steps;
        private static readonly Dictionary<string, LocalizationEntry> Texts = LateStoryTexts.All.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());

        private static string HeroName(CharacterId hero) => hero == CharacterId.Icaro ? "Ícaro" : hero.ToString();

        private static OverworldZone ZoneOf(LateStoryStep step) => OverworldLayouts.ForMap(step.MapId).Zone(step.ZoneId);

        private static bool FaunaHas(OverworldZone zone, string enemyId) => zone.Fauna.Any(f =>
            f.Day.Concat(f.Night).Concat(f.Dusk ?? new PeriodSpawnEntry[0]).Any(e => e.EnemyId == enemyId));

        [Test]
        public void Chain_CoversRegions9To16_WithTwoOrThreeSteps_AndEndsAtTheVaranda()
        {
            CollectionAssert.AreEqual(Enumerable.Range(9, 8).ToArray(), LateStoryCatalog.Steps.Select(s => s.Region).Distinct().ToArray(), "one chain, region after region");
            foreach (int region in Enumerable.Range(9, 8))
            {
                Assert.That(LateStoryCatalog.StepsIn(region).Count(), Is.InRange(2, 3), $"region {region}");
            }

            Assert.AreEqual("map_100_varanda", LateStoryCatalog.Last.MapId);
            Assert.AreEqual("varanda_do_tempo_de_depois", LateStoryCatalog.Last.ZoneId);
            Assert.IsTrue(LateStoryCatalog.Steps.Any(s => s.ZoneId == "pes_da_coluna" && s.MapId == "map_098_pes_da_coluna"), "the chain goes through the Coluna da Glória");
            Assert.AreEqual(LateStoryCatalog.Steps.Count, LateStoryCatalog.Steps.Select(s => s.QuestId).Distinct().Count(), "unique ids");
        }

        [Test]
        public void Chain_HasNoCycles_EachStepRequiresThePreviousOne_AndLevelsGrow()
        {
            // The letters open after "A Lua que Não Enche" (MidStoryCatalog): the one root of the story is the first step of that chain, and the walk below goes
            // through both chains, so the first letter must require the last step of the mid story.
            Assert.AreEqual(MidStoryCatalog.FinalQuestId, LateStoryCatalog.Steps[0].Quest.PrerequisiteQuestId, "the first letter waits for the full moon");
            Assert.AreEqual(1, LateStoryCatalog.WholeStory.Count(s => string.IsNullOrEmpty(s.Quest.PrerequisiteQuestId)), "the chain has one root");
            Assert.IsNull(LateStoryCatalog.WholeStory.First().Quest.PrerequisiteQuestId, "the root comes first");
            int level = 0;
            string previous = null;
            var seen = new HashSet<string>();
            foreach (LateStoryStep step in LateStoryCatalog.WholeStory)
            {
                Assert.IsTrue(seen.Add(step.QuestId), $"{step.QuestId} repeats");
                Assert.AreEqual(previous, step.Quest.PrerequisiteQuestId, $"{step.QuestId} requires the step right before it");
                Assert.Greater(step.MinLevel, level, $"{step.QuestId}: level gate grows");
                level = step.MinLevel;
                previous = step.QuestId;
            }
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

                MapDefinition map = MapCatalog.Get(step.MapId);
                Assert.IsNotNull(map, step.MapId);
                OverworldZone zone = ZoneOf(step);
                Assert.IsNotNull(zone, $"{step.QuestId}: zone {step.ZoneId} of {step.MapId}");
                Assert.AreEqual(step.MapId, zone.MapIdOrOwner, step.QuestId);
                Assert.GreaterOrEqual(step.MinLevel, zone.WalkLevel, $"{step.QuestId}: a hero of the step's level may walk in");
                Assert.AreNotEqual(step.CityId, zone.Id, $"{step.QuestId}: the place is not the giver's own town, where the ReachLocation report could never happen");

                RegionObjectiveSpec reach = step.Quest.Objectives.Single(o => o.Type == QuestObjectiveType.ReachLocation);
                Assert.AreEqual(step.ZoneId, reach.TargetId, step.QuestId);
                foreach (RegionObjectiveSpec kill in step.Quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType))
                {
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{kill.TargetId}");
                    Assert.IsNotNull(enemy, $"{step.QuestId}: {kill.TargetId}");
                    Assert.IsFalse(enemy.IsBoss, $"{step.QuestId}: a boss is fought once");
                    Assert.Greater(kill.RequiredAmount, 0, step.QuestId);
                    Assert.IsTrue(FaunaHas(zone, kill.TargetId), $"{step.QuestId}: {kill.TargetId} lives in the fauna of {zone.Id}");
                }

                Assert.AreEqual(1, step.Quest.Objectives.Count(o => o.Type == QuestObjectiveType.ReachLocation), step.QuestId);
                Assert.IsTrue(step.Quest.Objectives.All(o => o.Type == QuestObjectiveType.KillEnemyType || o.Type == QuestObjectiveType.ReachLocation), $"{step.QuestId}: only kinds with a live source");
            }
        }

        [Test]
        public void Rewards_AreInTheCityQuestBand_AndResolve()
        {
            foreach (LateStoryStep step in AllSteps)
            {
                Assert.That(step.Quest.RewardExperience, Is.InRange(2000, 4800), step.QuestId);
                Assert.IsTrue(step.Quest.Rewards.All(r => ItemDatabase.GetById(r.ItemId) != null), $"{step.QuestId}: items exist");
                Assert.IsFalse(step.Quest.Rewards.Any(r => r.ItemId == "item_pedra_vida" || r.ItemId == "item_pedra_caos"), step.QuestId);
                Assert.That(step.Quest.Rewards.Where(r => r.ItemId == "item_gold").Sum(r => r.Quantity), Is.InRange(100, 320), step.QuestId);

                QuestDefinition quest = LateStoryCatalog.GetQuest(step.QuestId);
                Assert.AreEqual(QuestType.MainStory, quest.QuestType);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), step.QuestId);
                Assert.AreSame(quest, CityQuestCatalog.GetQuest(step.QuestId), "found through the city catalog (dialogue, quest log, save)");
            }

            Assert.IsNotNull(CityQuestCatalog.GetAllQuests().FirstOrDefault(q => q.QuestId == LateStoryCatalog.SkyPorch), "in the quest log");
        }

        [Test]
        public void EveryStep_HasADifferentLinePerCousin_InBothLanguages()
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
                    Assert.IsTrue(Texts.ContainsKey(step.KeyBase + suffix), step.KeyBase + suffix);
                }

                foreach (RegionObjectiveSpec objective in step.Quest.Objectives)
                {
                    Assert.IsTrue(Texts.ContainsKey(step.Quest.ObjectiveKey(objective.ObjectiveId)), step.Quest.ObjectiveKey(objective.ObjectiveId));
                }
            }

            foreach (CharacterId hero in Heroes)
            {
                foreach (string keyBase in new[] { LateStoryCatalog.TooLowKeyBase, LateStoryCatalog.FinaleKeyBase })
                {
                    LocalizationEntry entry = Texts[keyBase + LateStoryCatalog.HeroKey(hero)];
                    StringAssert.Contains(HeroName(hero), entry.PtBR);
                    StringAssert.Contains(HeroName(hero), entry.En);
                }
            }
        }

        [Test]
        public void TheGrandparents_ArePresent_InEveryStep_InBothLanguages()
        {
            foreach (LateStoryStep step in AllSteps)
            {
                LocalizationEntry description = Texts[step.KeyBase + ".description"];
                LocalizationEntry journal = Texts[step.KeyBase + ".journal"];
                StringAssert.Contains("Vô Jorge", description.PtBR, step.QuestId);
                StringAssert.Contains("Grandpa Jorge", description.En, step.QuestId);
                StringAssert.Contains("Vó Edna", journal.PtBR, step.QuestId);
                StringAssert.Contains("Grandma Edna", journal.En, step.QuestId);
            }

            foreach (CharacterId hero in Heroes)
            {
                LocalizationEntry finale = Texts[LateStoryCatalog.FinaleKeyBase + LateStoryCatalog.HeroKey(hero)];
                StringAssert.Contains("Edna", finale.PtBR, hero.ToString());
                StringAssert.Contains("Edna", finale.En, hero.ToString());
            }
        }

        [Test]
        public void Texts_FollowTheBible_NoFolkloreNoDoctrine_AndKeysAreUnique()
        {
            Assert.AreEqual(LateStoryTexts.All.Count, LateStoryTexts.All.Select(e => e.Key).Distinct().Count(), "duplicate key");
            var banned = new Regex(@"\b(saci|curupira|iara|boitatá|lobisomem|dona|religião|pecado|salvação|igreja|deus|deuses|profeta|rezar|orar|amém|sertão|rapadura|vovó)\b", RegexOptions.IgnoreCase);
            var bannedEn = new Regex(@"\b(god|gods|pray|prayer|amen|church|prophet|faith)\b", RegexOptions.IgnoreCase);
            foreach (LocalizationEntry entry in LateStoryTexts.All)
            {
                Assert.IsFalse(banned.IsMatch(entry.PtBR), $"{entry.Key}: {banned.Match(entry.PtBR).Value}");
                StringAssert.DoesNotContain("Seu ", entry.PtBR, entry.Key);
                Assert.IsFalse(bannedEn.IsMatch(entry.En), $"{entry.Key}: {bannedEn.Match(entry.En).Value}");
            }
        }

        [Test]
        public void Decide_OpensTheChainInOrder_ByStepAndByLevel()
        {
            var active = new HashSet<string>();
            var done = new HashSet<string>();
            LateStoryStep first = LateStoryCatalog.Steps[0];
            LateStoryStep second = LateStoryCatalog.Steps[1];

            // The first letter now waits for "A Lua que Não Enche": Soslan has nothing to say until the full moon, at any level.
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(first.GiverResidentId, active.Contains, done.Contains, 500, out _), "before the mid story is done");
            done.UnionWith(MidStoryCatalog.Steps.Select(s => s.QuestId));

            Assert.AreEqual(LateStoryTalk.TooLow, LateStoryCatalog.Decide(first.GiverResidentId, active.Contains, done.Contains, first.MinLevel - 1, out _), "under the level");
            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(first.GiverResidentId, active.Contains, done.Contains, first.MinLevel, out LateStoryStep offered));
            Assert.AreSame(first, offered);
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(second.GiverResidentId, active.Contains, done.Contains, 500, out _), "the next step waits for the previous one");

            active.Add(first.QuestId);
            Assert.AreEqual(LateStoryTalk.Active, LateStoryCatalog.Decide(first.GiverResidentId, active.Contains, done.Contains, 500, out _));

            active.Clear();
            done.Add(first.QuestId);
            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(second.GiverResidentId, active.Contains, done.Contains, second.MinLevel, out LateStoryStep next));
            Assert.AreSame(second, next);
            Assert.AreEqual(LateStoryTalk.None, LateStoryCatalog.Decide(first.GiverResidentId, active.Contains, done.Contains, 500, out _), "their part is done");

            foreach (LateStoryStep step in AllSteps)
            {
                done.Add(step.QuestId);
            }

            Assert.AreEqual(LateStoryTalk.Finale, LateStoryCatalog.Decide(LateStoryCatalog.Last.GiverResidentId, active.Contains, done.Contains, 500, out _));
        }

        [Test]
        public void Talk_StartsTheStep_AndAReportedZoneCompletesTheFinalStep()
        {
            // The mid story is done (the first letter requires its last step).
            var quests = new QuestManager();
            quests.RestoreState(new QuestSaveState { CompletedQuestIds = MidStoryCatalog.Steps.Select(s => s.QuestId).ToList() });
            LateStoryStep first = LateStoryCatalog.Steps[0];
            Assert.AreEqual(first.OfferKey(CharacterId.Lavine), LateStoryCatalog.Talk(quests, first.GiverResidentId, CharacterId.Lavine, first.MinLevel));
            Assert.IsTrue(quests.IsActive(first.QuestId));
            Assert.AreEqual(first.ActiveKey, LateStoryCatalog.Talk(quests, first.GiverResidentId, CharacterId.Maithe, first.MinLevel));

            // The last step asks only for the place: the report of the zone finishes it.
            LateStoryStep last = LateStoryCatalog.Last;
            var finalQuests = new QuestManager();
            finalQuests.RestoreState(new QuestSaveState { CompletedQuestIds = LateStoryCatalog.Steps.Take(LateStoryCatalog.Steps.Count - 1).Select(s => s.QuestId).ToList() });
            Assert.AreEqual(last.OfferKey(CharacterId.Maria), LateStoryCatalog.Talk(finalQuests, last.GiverResidentId, CharacterId.Maria, last.MinLevel));
            finalQuests.ReportLocationReached("rio_de_luz");
            Assert.IsTrue(finalQuests.IsActive(last.QuestId), "another zone does not count");
            finalQuests.ReportLocationReached(last.ZoneId);
            Assert.IsTrue(finalQuests.IsCompleted(last.QuestId));
            Assert.AreEqual(LateStoryCatalog.FinaleKeyBase + "maria", LateStoryCatalog.Talk(finalQuests, last.GiverResidentId, CharacterId.Maria, 500));
        }

        [Test]
        public void TextsLoad_ThroughTheCityQuestTexts_AndTheFallback()
        {
            var manager = new LocalizationManager();
            manager.Initialize();
            CityQuestTexts.EnsureLoaded(manager);
            LateStoryStep step = LateStoryCatalog.Steps[0];
            Assert.IsTrue(manager.HasKey(step.KeyBase + ".title"));
            Assert.IsTrue(manager.HasKey(step.OfferKey(CharacterId.Icaro)));
            Assert.IsNotNull(CityQuestTexts.GetFallback(step.KeyBase + ".journal"));
        }
    }
}
