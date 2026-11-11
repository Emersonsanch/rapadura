using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Localization;
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
    /// The city quests on the road (<see cref="CityQuestSpec.IsOnTheRoad"/>, Docs/Balanco100a500.md §5.3 sugestão 5): the givers of the cities of
    /// regions 9–16 send the cousins to a zone of their region at a level. Every band of 10 levels of 101–500 has a quest; each road quest points at a
    /// map that returns to its city, a zone of that map whose level holds the quest's, and a beast of that zone's fauna; the giver waits for the level,
    /// the quest pays 25% of that level's XP, and its texts exist in both languages and bring the family along.
    /// </summary>
    public class CityRoadQuestTests
    {
        private static readonly string[] Cousins = { "Maithe", "Ícaro", "Joaquim", "Maria", "Lavine", "Edna", "Jorge" };

        private static IEnumerable<CityQuestSpec> Road => CityQuestCatalog.Quests.Where(q => q.IsOnTheRoad);

        private static bool FaunaHas(OverworldZone zone, string enemyId) => zone.Fauna.Any(f =>
            f.Day.Concat(f.Night).Concat(f.Dusk ?? new PeriodSpawnEntry[0]).Any(e => e.EnemyId == enemyId));

        private static int BandOf(int level) => (level - 1) / 10;

        [Test]
        public void EveryBandOf101To500_HasAQuest()
        {
            var levels = new List<int>();
            levels.AddRange(CityQuestCatalog.Quests.Select(q => q.PaidLevel));
            levels.AddRange(LateStoryCatalog.WholeStory.Select(s => s.MinLevel));
            var bands = new HashSet<int>(levels.Select(BandOf));
            for (int low = 101; low < 500; low += 10)
            {
                Assert.IsTrue(bands.Contains(BandOf(low)), $"no quest in {low}–{low + 10}");
            }

            // The bands the road fills get two city quests each (one is 2.5% of a band: under the 3% floor of tools/balance).
            foreach (IGrouping<int, CityQuestSpec> band in Road.GroupBy(q => BandOf(q.MinLevel)))
            {
                Assert.AreEqual(2, band.Count(), $"band {band.Key * 10 + 1}");
            }

            Assert.AreEqual(40, Road.Count(), "two for each of the 20 bands that had no quest of their own level");
        }

        [Test]
        public void RoadQuests_PointAtAZoneOfTheirRegion_WithTheBeast_AtTheLevel()
        {
            foreach (CityQuestSpec spec in Road)
            {
                Assert.IsNotNull(CityCatalog.Get(spec.CityId), spec.QuestId);
                Assert.IsTrue(CityCatalog.Get(spec.CityId).Residents.Any(r => r.ResidentId == spec.GiverResidentId), $"{spec.QuestId}: {spec.GiverResidentId}");

                MapDefinition map = MapCatalog.Get(spec.MapId);
                Assert.IsNotNull(map, $"{spec.QuestId}: {spec.MapId}");
                Assert.AreEqual(spec.CityId, map.ReturnCityId, $"{spec.QuestId}: {spec.MapId} belongs to the giver's city");
                Assert.GreaterOrEqual(spec.MinLevel, map.WalkLevelRequirement, $"{spec.QuestId}: a hero of the quest's level may walk the map");

                OverworldZone zone = OverworldLayouts.ForMap(spec.MapId)?.Zone(spec.ZoneId);
                Assert.IsNotNull(zone, $"{spec.QuestId}: zone {spec.ZoneId} of {spec.MapId}");
                Assert.AreEqual(spec.MapId, zone.MapIdOrOwner, spec.QuestId);
                Assert.That(spec.MinLevel, Is.InRange(zone.WalkLevel, zone.MaxLevel), $"{spec.QuestId}: the level of {zone.Id} ({zone.WalkLevel}–{zone.MaxLevel})");

                Assert.IsNotEmpty(spec.Quest.Objectives, spec.QuestId);
                foreach (RegionObjectiveSpec objective in spec.Quest.Objectives)
                {
                    Assert.AreEqual(QuestObjectiveType.KillEnemyType, objective.Type, $"{spec.QuestId}: progresses in combat");
                    Assert.That(objective.RequiredAmount, Is.InRange(1, 10), spec.QuestId);
                    EnemyDefinition enemy = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{objective.TargetId}");
                    Assert.IsNotNull(enemy, $"{spec.QuestId}: {objective.TargetId}");
                    Assert.IsFalse(enemy.IsBoss, $"{spec.QuestId}: {objective.TargetId}");
                    Assert.IsTrue(FaunaHas(zone, objective.TargetId), $"{spec.QuestId}: {objective.TargetId} lives in the fauna of {zone.Id}");
                }
            }
        }

        [Test]
        public void RoadQuests_PayTheirLevel_WithRealRewards()
        {
            foreach (CityQuestSpec spec in Road)
            {
                Assert.AreEqual(spec.MinLevel, spec.PaidLevel, spec.QuestId);
                Assert.AreEqual(QuestXpRules.For(QuestXpKind.City, spec.MinLevel), spec.Quest.RewardExperience, $"{spec.QuestId}: 25% of the XP of its level");
                Assert.Greater(spec.Quest.RewardExperience, QuestXpRules.For(QuestXpKind.City, CityQuestCatalog.LevelOf(spec.CityId)), spec.QuestId);

                Assert.IsTrue(spec.Quest.Rewards.All(r => ItemDatabase.GetById(r.ItemId) != null), $"{spec.QuestId}: items exist");
                Assert.IsFalse(spec.Quest.Rewards.Any(r => r.ItemId == "item_pedra_vida" || r.ItemId == "item_pedra_caos"), spec.QuestId);
                Assert.That(spec.Quest.Rewards.Where(r => r.ItemId == "item_gold").Sum(r => r.Quantity), Is.InRange(150, 400), spec.QuestId);
                QuestDefinition quest = CityQuestCatalog.GetQuest(spec.QuestId);
                Assert.IsNotNull(quest, spec.QuestId);
                Assert.AreEqual(spec.Quest.RewardExperience, quest.RewardExperience, spec.QuestId);
                Assert.IsTrue(quest.RewardItems.All(r => r.Item != null), spec.QuestId);
            }

            // The quests of the cities' own maps keep their city's level.
            CityQuestSpec home = CityQuestCatalog.FindSpec(CityQuestCatalog.FarnkeshVultures);
            Assert.IsFalse(home.IsOnTheRoad);
            Assert.AreEqual(CityQuestCatalog.LevelOf(CityCatalog.Farnkesh), home.PaidLevel);
        }

        [Test]
        public void RoadQuests_FollowTheGiversChain_AndWaitForTheirLevel()
        {
            foreach (CityQuestSpec spec in Road)
            {
                List<CityQuestSpec> chain = CityQuestCatalog.QuestsOf(spec.GiverResidentId).ToList();
                int index = chain.IndexOf(spec);
                Assert.Greater(index, 0, $"{spec.QuestId}: after the giver's own quest");
                Assert.AreEqual(chain[index - 1].QuestId, spec.Quest.PrerequisiteQuestId, $"{spec.QuestId}: requires the giver's previous quest");
                Assert.Greater(spec.MinLevel, chain[index - 1].PaidLevel, $"{spec.QuestId}: levels grow along the chain");

                var done = new HashSet<string>(chain.Take(index).Select(q => q.QuestId));
                var active = new HashSet<string>();
                Assert.AreEqual(CityQuestTalk.None, CityQuestCatalog.Decide(spec.GiverResidentId, active.Contains, done.Contains, spec.MinLevel - 1, out _),
                    $"{spec.QuestId}: not before level {spec.MinLevel}");
                Assert.AreEqual(CityQuestTalk.Offer, CityQuestCatalog.Decide(spec.GiverResidentId, active.Contains, done.Contains, spec.MinLevel, out CityQuestSpec offered), spec.QuestId);
                Assert.AreSame(spec, offered);
                active.Add(spec.QuestId);
                Assert.AreEqual(CityQuestTalk.Active, CityQuestCatalog.Decide(spec.GiverResidentId, active.Contains, done.Contains, 1, out _), $"{spec.QuestId}: once taken, it runs at any level");
            }

            // Talk starts it only from the level on.
            CityQuestSpec gorge = CityQuestCatalog.FindSpec(CityQuestCatalog.FarnkeshGorge);
            var quests = new QuestManager();
            quests.RestoreState(new QuestSaveState { CompletedQuestIds = new List<string> { CityQuestCatalog.FarnkeshVultures } });
            Assert.IsNull(CityQuestCatalog.Talk(quests, gorge.GiverResidentId, gorge.MinLevel - 1));
            Assert.IsFalse(quests.IsActive(gorge.QuestId));
            Assert.AreEqual(gorge.OfferKey, CityQuestCatalog.Talk(quests, gorge.GiverResidentId, gorge.MinLevel));
            Assert.IsTrue(quests.IsActive(gorge.QuestId));
        }

        [Test]
        public void RoadQuests_HaveTheirTexts_InBothLanguages_WithTheFamily()
        {
            Dictionary<string, LocalizationEntry> texts = CityQuestTexts.All.GroupBy(e => e.Key).ToDictionary(g => g.Key, g => g.First());
            foreach (CityQuestSpec spec in Road)
            {
                var keys = new List<string> { spec.Quest.TitleKey, spec.Quest.DescriptionKey, spec.Quest.JournalKey, spec.OfferKey, spec.ActiveKey };
                keys.AddRange(spec.Quest.Objectives.Select(o => spec.Quest.ObjectiveKey(o.ObjectiveId)));
                foreach (string key in keys)
                {
                    Assert.IsTrue(texts.TryGetValue(key, out LocalizationEntry entry), $"missing {key}");
                    Assert.AreEqual(1, CityQuestTexts.All.Count(e => e.Key == key), $"{key} is written once");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En) || string.IsNullOrWhiteSpace(entry.PtBR), key);
                    Assert.AreNotEqual(entry.En, entry.PtBR, $"{key} is not translated");
                }

                string story = texts[spec.Quest.JournalKey].PtBR + " " + texts[spec.OfferKey].PtBR + " " + texts[spec.ActiveKey].PtBR;
                Assert.IsTrue(Cousins.Any(name => story.Contains(name)), $"{spec.QuestId}: the journal or the giver's lines bring a cousin or a grandparent along");
                string all = string.Join(" ", keys.Select(k => texts[k].PtBR + " " + texts[k].En));
                foreach (string folk in new[] { "Saci", "Curupira", "Iara", "Boitatá", "Seu ", "Dona " })
                {
                    StringAssert.DoesNotContain(folk, all, spec.QuestId);
                }
            }
        }
    }
}
