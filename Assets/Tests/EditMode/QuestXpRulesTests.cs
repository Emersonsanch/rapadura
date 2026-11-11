using System;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Tests
{
    /// <summary>
    /// Quest XP that follows the level (<see cref="QuestXpRules"/>, Docs/Balanco100a500.md §5.3 sugestão 2): above level 50 a
    /// middle-story step pays 50% of the XP of its level, a letter 150%, a city quest 25%, never less than the authored number;
    /// the campaign curve (everything up to 50) keeps what is written. Mirrored by tools/balance/economy.py.
    /// </summary>
    public class QuestXpRulesTests
    {
        private static int Share(double share, int level) => (int)Math.Floor(share * LevelCurveMath.ShippedExperienceToNext(level) + 0.5);

        [Test]
        public void Shares_AreTheDecidedOnes()
        {
            Assert.AreEqual(0.5, QuestXpRules.MidStoryShare);
            Assert.AreEqual(1.5, QuestXpRules.RoadLetterShare);
            Assert.AreEqual(0.25, QuestXpRules.CityShare);
            Assert.AreEqual(50, QuestXpRules.ScaledAboveLevel);
        }

        [Test]
        public void UpToLevel50_AndTheAuthoredKind_NeverScale()
        {
            foreach (QuestXpKind kind in (QuestXpKind[])Enum.GetValues(typeof(QuestXpKind)))
            {
                Assert.AreEqual(0, QuestXpRules.For(kind, 1), kind.ToString());
                Assert.AreEqual(0, QuestXpRules.For(kind, 30), kind.ToString());
                Assert.AreEqual(0, QuestXpRules.For(kind, 50), kind.ToString());
                Assert.AreEqual(320, QuestXpRules.For(kind, 50, 320), kind.ToString());
            }

            Assert.AreEqual(0, QuestXpRules.For(QuestXpKind.Authored, 300));
            Assert.AreEqual(1200, QuestXpRules.For(QuestXpKind.Authored, 300, 1200));
        }

        [Test]
        public void AboveLevel50_PaysTheShareOfTheLevelsXp()
        {
            Assert.AreEqual(12005, LevelCurveMath.ShippedExperienceToNext(52));
            Assert.AreEqual(6003, QuestXpRules.For(QuestXpKind.MidStory, 52));
            Assert.AreEqual(3001, QuestXpRules.For(QuestXpKind.City, 52));
            Assert.AreEqual(85376, LevelCurveMath.ShippedExperienceToNext(100));
            Assert.AreEqual(128064, QuestXpRules.For(QuestXpKind.RoadLetter, 100));
            Assert.AreEqual(Share(0.25, 51), QuestXpRules.For(QuestXpKind.City, 51));
            Assert.AreEqual(Share(1.5, 500), QuestXpRules.For(QuestXpKind.RoadLetter, 500), "fits an int at the cap");

            for (int level = 51; level <= LevelCurveMath.MaxLevel; level++)
            {
                Assert.AreEqual(Share(0.5, level), QuestXpRules.For(QuestXpKind.MidStory, level), $"level {level}");
                Assert.AreEqual(Share(0.25, level), QuestXpRules.For(QuestXpKind.City, level), $"level {level}");
                Assert.Greater(QuestXpRules.For(QuestXpKind.RoadLetter, level), QuestXpRules.For(QuestXpKind.MidStory, level), $"level {level}");
            }
        }

        [Test]
        public void NeverPaysLessThanTheAuthoredNumber()
        {
            Assert.AreEqual(9000, QuestXpRules.For(QuestXpKind.MidStory, 52, 9000));
            Assert.AreEqual(6003, QuestXpRules.For(QuestXpKind.MidStory, 52, 1060));
            Assert.AreEqual(0, QuestXpRules.For(QuestXpKind.Authored, 10, -5), "never negative");
        }

        [Test]
        public void Apply_KeepsTheSpec_OnlyTheXpChanges()
        {
            var objectives = new[] { new RegionObjectiveSpec("o", QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 3) };
            var rewards = new[] { new RegionRewardSpec("item_gold", 10) };
            var spec = new RegionQuestSpec("quest_test_xp", BiomeType.Forest, "quest_before", 560, objectives, rewards);

            Assert.AreSame(spec, QuestXpRules.Apply(spec, QuestXpKind.MidStory, 50), "nothing changes up to 50");
            Assert.AreSame(spec, QuestXpRules.Apply(spec, QuestXpKind.Authored, 300));

            RegionQuestSpec scaled = QuestXpRules.Apply(spec, QuestXpKind.MidStory, 52);
            Assert.AreEqual(6003, scaled.RewardExperience);
            Assert.AreEqual(spec.QuestId, scaled.QuestId);
            Assert.AreEqual(spec.Region, scaled.Region);
            Assert.AreEqual(spec.PrerequisiteQuestId, scaled.PrerequisiteQuestId);
            CollectionAssert.AreEqual(spec.Objectives, scaled.Objectives);
            CollectionAssert.AreEqual(spec.Rewards, scaled.Rewards);
            Assert.IsNull(QuestXpRules.Apply(null, QuestXpKind.City, 100));
        }

        [Test]
        public void TheCatalogs_PayByTheRule()
        {
            // The middle story: every step up to 50 keeps its number (Tages 300, the Areias throne 560, the gulls of 46 960);
            // the baobabs (52) 1 060 → 6 003.
            Assert.AreEqual(300, MidStoryCatalog.FindStep(MidStoryCatalog.LimiarMoon).Quest.RewardExperience);
            Assert.AreEqual(560, MidStoryCatalog.FindStep(MidStoryCatalog.AreiasThrone).Quest.RewardExperience);
            Assert.AreEqual(960, MidStoryCatalog.FindStep(MidStoryCatalog.CostaGulls).Quest.RewardExperience);
            Assert.AreEqual(6003, MidStoryCatalog.FindStep(MidStoryCatalog.CostaBaobabs).Quest.RewardExperience);
            foreach (LateStoryStep step in MidStoryCatalog.Steps)
            {
                Assert.GreaterOrEqual(step.Quest.RewardExperience, QuestXpRules.For(QuestXpKind.MidStory, step.MinLevel), step.QuestId);
            }

            // The letters: 150% of their level (always above the written 2 300–4 600).
            Assert.AreEqual(128064, LateStoryCatalog.FindStep(LateStoryCatalog.SteppeFoals).Quest.RewardExperience);

            // City quests at their city's walk level: Garajonay (18) keeps 320, Taa Pwani (map 25, walk 42) keeps 700; Kirta (map 28,
            // walk 52) 1 040 → 3 001.
            Assert.AreEqual(18, CityQuestCatalog.LevelOf(CityCatalog.CopasGarajonay));
            Assert.AreEqual(320, CityQuestCatalog.FindSpec(CityQuestCatalog.GarajonayMist).Quest.RewardExperience);
            Assert.AreEqual(42, CityQuestCatalog.LevelOf(CityCatalog.TaaPwani));
            Assert.AreEqual(700, CityQuestCatalog.FindSpec(CityQuestCatalog.TaaPwaniGulls).Quest.RewardExperience);
            Assert.AreEqual(52, CityQuestCatalog.LevelOf(CityCatalog.Kirta));
            Assert.AreEqual(3001, CityQuestCatalog.FindSpec(CityQuestCatalog.KirtaOoze).Quest.RewardExperience);
            Assert.AreEqual(1, CityQuestCatalog.LevelOf("no_such_city"));
            foreach (CityQuestSpec quest in CityQuestCatalog.Quests)
            {
                Assert.GreaterOrEqual(quest.Quest.RewardExperience, QuestXpRules.For(QuestXpKind.City, CityQuestCatalog.LevelOf(quest.CityId)), quest.QuestId);
            }

            // The built quest pays what the spec says (the reward is read from the definition).
            Assert.AreEqual(3001, CityQuestCatalog.GetQuest(CityQuestCatalog.KirtaOoze).RewardExperience);
            Assert.AreEqual(128064, LateStoryCatalog.GetQuest(LateStoryCatalog.SteppeFoals).RewardExperience);
            Assert.That(RegionQuestCatalog.Quests.Sum(q => q.RewardExperience), Is.InRange(800, 1500), "the campaign region quests keep their numbers");
        }
    }
}
