using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.ClassEvolution;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Talents;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.ClassEvolution;
using Rapadura.UI.Common;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 500-level cap (Docs/Progressao500.md): the piecewise XP curve against the doc's milestone table, points per
    /// level, the cap itself, old-save migration (int XP, no lifetime total, levels past 50), the master gate moving from
    /// 50 to 500 with old master progress kept, the G4/G5 hooks, creature level scaling and the big-number labels.
    /// </summary>
    public class Level500Tests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp() => EventBus.Clear();

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
        }

        private PlayerStats NewPlayer()
        {
            var go = new GameObject("Level500Tests_Player");
            _created.Add(go);
            return go.AddComponent<PlayerStats>();
        }

        private static PlayerStatsSaveState OldSave(int level, int xp) =>
            JsonUtility.FromJson<PlayerStatsSaveState>(
                "{\"currentHealth\":100,\"currentStamina\":100,\"currentMana\":100,\"currentExperience\":" + xp + ",\"level\":" + level + "}");

        // ------------------------------------------------------------------ curve

        // Docs/Progressao500.md §1.3: level, XP to the next, total XP to reach the level.
        private static readonly long[,] Milestones =
        {
            { 1, 100, 0 },
            { 10, 236, 1_357 },
            { 25, 985, 8_848 },
            { 49, 9_702, 96_013 },
            { 50, 10_672, 105_715 },
            { 51, 11_325, 116_387 },
            { 100, 85_376, 2_069_521 },
            { 150, 288_144, 10_639_402 },
            { 200, 683_008, 33_787_346 },
            { 300, 2_305_152, 171_713_342 },
            { 400, 6_830_080, 580_789_956 },
            { 500, 21_344_000, 1_853_225_538 },
        };

        [Test]
        public void Curve_MatchesTheDocMilestones_PureAndShippedAsset()
        {
            ProgressionCurve shipped = ProgressionCurve.LoadDefault();
            Assert.IsNotNull(shipped);
            for (int i = 0; i < Milestones.GetLength(0); i++)
            {
                int level = (int)Milestones[i, 0];
                Assert.AreEqual(Milestones[i, 1], LevelCurveMath.ShippedExperienceToNext(level), $"pure XP to next at {level}");
                Assert.AreEqual(Milestones[i, 2], LevelCurveMath.ShippedTotalExperienceToReach(level), $"pure total at {level}");
                Assert.AreEqual(Milestones[i, 1], shipped.GetExperienceToNextLevel(level), $"asset XP to next at {level}");
                Assert.AreEqual(Milestones[i, 2], shipped.GetTotalExperienceToReach(level), $"asset total at {level}");
            }

            Assert.Greater(shipped.GetTotalExperienceToReach(500), 1_800_000_000L, "close to int.MaxValue: totals are long");
        }

        [Test]
        public void Curve_OneToFiftyUnchanged_AndGrowsEveryLevelToTheCap()
        {
            ProgressionCurve shipped = ProgressionCurve.LoadDefault();
            for (int level = 1; level <= LevelCurveMath.CampaignLevels; level++)
            {
                Assert.AreEqual(Mathf.RoundToInt(100 * Mathf.Pow(1.1f, level - 1)), shipped.GetExperienceToNextLevel(level), $"level {level}");
                Assert.AreEqual(LevelCurveMath.ShippedExperienceToNext(level), shipped.GetExperienceToNextLevel(level), $"level {level}");
            }

            for (int level = 2; level <= ProgressionCurve.MaxLevel; level++)
            {
                Assert.Greater(shipped.GetExperienceToNextLevel(level), shipped.GetExperienceToNextLevel(level - 1), $"XP at {level}");
                Assert.Greater(shipped.GetMaxHealth(level), shipped.GetMaxHealth(level - 1), $"HP at {level}");
                Assert.Greater(shipped.GetDamageMultiplier(level), shipped.GetDamageMultiplier(level - 1), $"damage at {level}");
            }

            Assert.AreEqual(892f, shipped.GetMaxHealth(100), 0.001f);
            Assert.AreEqual(4092f, shipped.GetMaxHealth(500), 0.001f);
            Assert.AreEqual(2.98f, shipped.GetDamageMultiplier(100), 1e-4f);
            Assert.AreEqual(1.25, LevelCurveMath.Wall(400), 1e-9);
            Assert.AreEqual(2.0, LevelCurveMath.Wall(500), 1e-9);
        }

        [Test]
        public void Points_AttributesEveryLevel_SkillsEveryFiveAfterFifty_WheelEveryTen()
        {
            ProgressionCurve shipped = ProgressionCurve.LoadDefault();
            Assert.AreEqual(98, shipped.GetAttributePointsGained(1, 50));
            Assert.AreEqual(198, shipped.GetAttributePointsGained(1, 100));
            Assert.AreEqual(998, shipped.GetAttributePointsGained(1, 500));
            Assert.AreEqual(998, shipped.GetAttributePointsGained(1, 900), "nothing past the cap");

            Assert.AreEqual(49, shipped.GetSkillPointsGained(1, 50));
            Assert.AreEqual(0, shipped.GetSkillPointsGained(50, 54));
            Assert.AreEqual(1, shipped.GetSkillPointsGained(54, 55));
            Assert.AreEqual(59, shipped.GetSkillPointsGained(1, 100));
            Assert.AreEqual(139, shipped.GetSkillPointsGained(1, 500));

            Assert.AreEqual(39, TalentWheelRules.EarnedPoints(50));
            Assert.AreEqual(39, TalentWheelRules.EarnedPoints(59));
            Assert.AreEqual(40, TalentWheelRules.EarnedPoints(60));
            Assert.AreEqual(44, TalentWheelRules.EarnedPoints(100));
            Assert.AreEqual(84, TalentWheelRules.EarnedPoints(500));
        }

        [Test]
        public void Player_LevelsPastFifty_AndStopsAtTheCapWithAFullBar()
        {
            PlayerStats stats = NewPlayer();
            stats.RestoreState(OldSave(50, 0));
            stats.AddExperience(10_672 + 11_325);
            Assert.AreEqual(52, stats.Level);
            Assert.AreEqual(0, stats.CurrentExperience);
            Assert.AreEqual(LevelCurveMath.ShippedTotalExperienceToReach(52), stats.TotalExperience);

            stats.RestoreState(OldSave(499, 0));
            stats.AddExperience(int.MaxValue);
            Assert.AreEqual(500, stats.Level);
            Assert.IsTrue(stats.IsAtMaxLevel);
            Assert.AreEqual(stats.GetExperienceToNextLevel(), stats.CurrentExperience, "bar full at the cap");
            stats.AddExperience(int.MaxValue);
            Assert.AreEqual(500, stats.Level);
            Assert.AreEqual(stats.GetExperienceToNextLevel(), stats.CurrentExperience);
            Assert.AreEqual(LevelCurveMath.ShippedTotalExperienceToReach(500) + stats.CurrentExperience, stats.TotalExperience);
        }

        // ------------------------------------------------------------------ save migration

        [Test]
        public void OldSave_IntXpWithoutTotal_LoadsAndGainsALongTotal()
        {
            PlayerStats stats = NewPlayer();
            PlayerStatsSaveState old = OldSave(50, 5_000);
            Assert.AreEqual(0L, old.totalExperience, "the field did not exist");

            stats.RestoreState(old);
            Assert.AreEqual(50, stats.Level);
            Assert.AreEqual(5_000, stats.CurrentExperience);
            Assert.AreEqual(105_715L + 5_000L, stats.TotalExperience);

            stats.RestoreState(OldSave(500, 1_000));
            var saved = (PlayerStatsSaveState)stats.CaptureState();
            Assert.AreEqual(1_853_226_538L, saved.totalExperience);
            PlayerStatsSaveState reloaded = JsonUtility.FromJson<PlayerStatsSaveState>(JsonUtility.ToJson(saved));
            Assert.AreEqual(1_853_226_538L, reloaded.totalExperience, "a long above int.MaxValue survives JsonUtility");
        }

        [Test]
        public void OldSave_PastFiftyOnTheOldCurve_LevelsUpOnLoad_NothingLost()
        {
            // Before the cap the formula kept going exponentially past 50 (xp(51) = 11 739); the cubic asks 11 325.
            PlayerStats stats = NewPlayer();
            stats.RestoreState(OldSave(51, 11_500));
            Assert.AreEqual(52, stats.Level);
            Assert.AreEqual(11_500 - 11_325, stats.CurrentExperience);

            stats.RestoreState(OldSave(900, 7));
            Assert.AreEqual(LevelCurveMath.MaxLevel, stats.Level, "clamped to the cap");
            stats.RestoreState(OldSave(0, -5));
            Assert.AreEqual(1, stats.Level);
            Assert.AreEqual(0, stats.CurrentExperience);
        }

        // ------------------------------------------------------------------ master gate

        private static GraduationRecord ThirdGraduation(int masterLevel = 0) =>
            new GraduationRecord { characterId = "Maithe", tier = ClassEvolutionCatalog.MaxTier, masterLevel = masterLevel };

        [Test]
        public void Master_GateIsTheCap_NotFiftyAnymore()
        {
            Assert.AreEqual(500, MasterLevelRules.RequiredLevel);
            GraduationRecord record = ThirdGraduation();
            Assert.IsFalse(MasterLevelRules.IsMaster(record, 50));
            Assert.IsFalse(MasterLevelRules.IsMaster(record, 499));
            Assert.IsTrue(MasterLevelRules.IsMaster(record, 500));
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, 50, 1_000_000));
            Assert.AreEqual(0, record.masterExperience, "below the cap kill XP is normal XP");
            Assert.AreEqual(MasterSpendResult.NotMaster, MasterLevelRules.CanSpend(record, 50, "mao_firme"));

            // The master bar weighs like a level at the cap: xp(500) x 0.5 (x 1.05 per master level).
            Assert.AreEqual(10_672_000L, MasterLevelRules.ExperienceToNext(0));
            Assert.AreEqual(11_205_600L, MasterLevelRules.ExperienceToNext(1));
            Assert.AreEqual(1, MasterLevelRules.AddExperience(record, 500, 10_672_000L));
        }

        [Test]
        public void Master_OldSaveProgressIsKept_SpendableButFrozenUntilTheCap()
        {
            // A level-50 master from the old gate: master level 4, 3 points in "mão firme", XP on the bar.
            const string oldJson = "{\"characterId\":\"Maithe\",\"tier\":3,\"activeMissionTier\":0,\"missionKills\":0,\"masterLevel\":4," +
                                   "\"masterExperience\":12345,\"masterNodes\":[{\"nodeId\":\"mao_firme\",\"rank\":3}]}";
            GraduationRecord record = JsonUtility.FromJson<GraduationRecord>(oldJson);
            ClassEvolutionRules.Sanitize(CharacterId.Maithe, record);

            Assert.AreEqual(4, record.masterLevel);
            Assert.AreEqual(12_345, record.masterExperience);
            Assert.AreEqual(3, MasterLevelRules.RankOf(record, "mao_firme"));
            Assert.AreEqual(1, MasterLevelRules.Modifiers(record).Count, "the bonuses stay on");

            Assert.IsFalse(MasterLevelRules.IsMaster(record, 50));
            Assert.IsTrue(MasterLevelRules.HasKeptMasterProgress(record, 50));
            Assert.IsTrue(MasterLevelRules.CanUseTree(record, 50));
            Assert.AreEqual(MasterSpendResult.Spent, MasterLevelRules.Spend(record, 50, "couro_curtido"), "the earned 4th point");
            Assert.AreEqual(MasterSpendResult.NoPoints, MasterLevelRules.Spend(record, 120, "couro_curtido"));
            Assert.AreEqual(MasterSpendResult.NotMaster, MasterLevelRules.Spend(record, 49, "couro_curtido"));
            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, 120, 50_000_000));
            Assert.AreEqual(12_345, record.masterExperience, "the bar is frozen until 500");

            StringAssert.Contains("500", ClassEvolutionPresenter.MasterHeader(null, record, 120));
            StringAssert.Contains("4", ClassEvolutionPresenter.MasterHeader(null, record, 120));

            Assert.AreEqual(0, MasterLevelRules.AddExperience(record, 500, 1_000), "at 500 the bar waits for the 5th graduation");
            Assert.AreEqual(12_345, record.masterExperience);
            record.tier = ClassEvolutionCatalog.MaxTier;
            Assert.AreEqual(1, MasterLevelRules.AddExperience(record, 500, MasterLevelRules.ExperienceToNext(4)));
            Assert.AreEqual(5, record.masterLevel);
            Assert.AreEqual(12_345, record.masterExperience, "the old bar XP carried over");
        }

        // ------------------------------------------------------------------ G4 / G5

        [Test]
        public void Graduations_G4AndG5ShipAt150And350_MasterNeedsTheFifth()
        {
            Assert.AreEqual(5, ClassEvolutionCatalog.MaxTier);
            Assert.AreEqual(ClassEvolutionCatalog.MaxTier, MasterLevelRules.RequiredTier);
            Assert.AreEqual(3, MasterLevelRules.LegacyRequiredTier);
            int[] expected = { 8, 14, 25, 150, 350 };
            for (int tier = 1; tier <= 5; tier++)
            {
                Assert.AreEqual(expected[tier - 1], ClassEvolutionCatalog.MilestoneLevel(tier), $"tier {tier}");
            }

            foreach (CharacterId hero in new[] { CharacterId.Maithe, CharacterId.Maria, CharacterId.Joaquim, CharacterId.Icaro, CharacterId.Lavine })
            {
                var done = new GraduationRecord { tier = 3 };
                Assert.AreEqual(GraduationMissionStatus.LevelTooLow, ClassEvolutionRules.Status(hero, done, 149));
                Assert.AreEqual(GraduationMissionStatus.Available, ClassEvolutionRules.Status(hero, done, 150), "G4 is offered at 150");
                done.tier = 4;
                Assert.AreEqual(GraduationMissionStatus.LevelTooLow, ClassEvolutionRules.Status(hero, done, 349));
                Assert.AreEqual(GraduationMissionStatus.Available, ClassEvolutionRules.Status(hero, done, 350));
                Assert.IsFalse(MasterLevelRules.IsMaster(done, 500));
                done.tier = 5;
                Assert.IsTrue(MasterLevelRules.IsMaster(done, 500));
            }
        }

        // ------------------------------------------------------------------ creature level

        [Test]
        public void CreatureLevel_CampaignBandsAreIdentity()
        {
            foreach (BiomeType region in new[] { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves })
            {
                int level = EnemyLevelScaling.ResolveLevel(0, EnemyLevelScaling.RegionBandLevel(region));
                Assert.LessOrEqual(level, EnemyLevelScaling.ArchetypeLevel, region.ToString());
                Assert.IsTrue(EnemyLevelScaling.IsIdentity(level));
                Assert.AreEqual(1f, EnemyLevelScaling.HealthMultiplier(level));
                Assert.AreEqual(1f, EnemyLevelScaling.DamageMultiplier(level));
            }

            Assert.AreEqual(40, EnemyLevelScaling.ResolveLevel(40, 3), "the definition's own level wins");
            Assert.AreEqual(EnemyLevelScaling.MaxLevel, EnemyLevelScaling.ResolveLevel(9999, 3));
            var enemy = new GameObject("Level500Tests_Enemy");
            _created.Add(enemy);
            Assert.IsNull(EnemyLevelScaler.Attach(enemy, 15), "no scaler at identity levels");
            Assert.IsNotNull(EnemyLevelScaler.Attach(enemy, 40));
        }

        [Test]
        public void CreatureLevel_ScalesWithTheReferenceHero_DocTable()
        {
            // Docs/Progressao500.md §4.1: x health / x damage at 25, 50, 100, 500.
            Assert.AreEqual(1.26f, EnemyLevelScaling.HealthMultiplier(25), 0.01f);
            Assert.AreEqual(1.44f, EnemyLevelScaling.DamageMultiplier(25), 0.01f);
            Assert.AreEqual(2.02f, EnemyLevelScaling.HealthMultiplier(50), 0.01f);
            Assert.AreEqual(2.70f, EnemyLevelScaling.DamageMultiplier(50), 0.01f);
            Assert.AreEqual(4.06f, EnemyLevelScaling.HealthMultiplier(100), 0.01f);
            Assert.AreEqual(5.88f, EnemyLevelScaling.DamageMultiplier(100), 0.01f);
            Assert.AreEqual(45.1f, EnemyLevelScaling.HealthMultiplier(500), 0.1f);
            Assert.AreEqual(63.1f, EnemyLevelScaling.DamageMultiplier(500), 0.1f);
            for (int level = EnemyLevelScaling.ArchetypeLevel + 1; level <= EnemyLevelScaling.MaxLevel; level++)
            {
                Assert.Greater(EnemyLevelScaling.HealthMultiplier(level), EnemyLevelScaling.HealthMultiplier(level - 1), $"health at {level}");
            }

            Assert.AreEqual(4796f, EnemyLevelScaling.HealthFor(1180f, 100), 2f, "Tages archetype at 100");

            Assert.AreEqual(10, EnemyLevelScaling.KillExperience(2));
            Assert.AreEqual(19, EnemyLevelScaling.KillExperience(5));
            Assert.AreEqual(30, EnemyLevelScaling.KillExperience(9));
            Assert.AreEqual(40, EnemyLevelScaling.KillExperience(13));
            Assert.AreEqual(128, EnemyLevelScaling.KillExperience(50));
            Assert.AreEqual(676, EnemyLevelScaling.KillExperience(100));
            Assert.AreEqual(9 * 676, EnemyLevelScaling.KillExperience(100, EnemyRank.MapBoss), 9);
            Assert.AreEqual(1f, EnemyLevelScaling.LevelDifferenceXpMultiplier(30, 20));
            Assert.AreEqual(0.5f, EnemyLevelScaling.LevelDifferenceXpMultiplier(40, 20), 1e-6f);
            Assert.AreEqual(0.1f, EnemyLevelScaling.LevelDifferenceXpMultiplier(41, 20), 1e-6f);
        }

        // ------------------------------------------------------------------ labels

        [Test]
        public void LargeNumbers_CompactLabels()
        {
            Assert.AreEqual("99999", LargeNumberFormat.Compact(99_999, LanguageCode.en));
            Assert.AreEqual("288k", LargeNumberFormat.Compact(288_144, LanguageCode.en));
            Assert.AreEqual("21.3M", LargeNumberFormat.Compact(21_344_000, LanguageCode.en));
            Assert.AreEqual("21,3M", LargeNumberFormat.Compact(21_344_000, LanguageCode.ptBR));
            Assert.AreEqual("21.3M", LargeNumberFormat.Compact(21_343_999, LanguageCode.en), "truncated, never rounded up");
            Assert.AreEqual("2M", LargeNumberFormat.Compact(2_000_000, LanguageCode.en));
            Assert.AreEqual("1.8B", LargeNumberFormat.Compact(1_853_225_538, LanguageCode.en));
            Assert.AreEqual("0", LargeNumberFormat.Compact(-4, LanguageCode.en));
            Assert.AreEqual("12345/85376", LargeNumberFormat.Fraction(12_345, 85_376, LanguageCode.en));
        }
    }
}
