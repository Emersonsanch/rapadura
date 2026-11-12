using NUnit.Framework;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Kill XP tied to the creature level (Docs/Progressao500.md §1.2): open-map monsters (<see cref="MapMonsterInfo"/>, an
    /// <see cref="ICreatureLevelSource"/>) keep their asset XP up to level 15 and take the table above it, then the MU-style
    /// cut when the hero is far above; equipment stops dropping beyond 20 levels below. Campaign stats stay identity 1–15.
    /// </summary>
    public class MapMonsterLevelTests
    {
        [Test]
        public void UpToFifteen_TheAssetXpIsKept_SameLevelHero()
        {
            for (int level = 1; level <= EnemyLevelScaling.ArchetypeLevel; level++)
            {
                Assert.AreEqual(10, EnemyLevelScaling.LeveledKillExperience(10, level, EnemyRank.Common, level), $"level {level}");
                Assert.AreEqual(90, EnemyLevelScaling.LeveledKillExperience(90, level, EnemyRank.MapBoss, level), $"boss level {level}");
                Assert.AreEqual(1f, EnemyLevelScaling.HealthMultiplier(level), "stats: identity 1–15");
                Assert.AreEqual(1f, EnemyLevelScaling.DamageMultiplier(level));
            }
        }

        [Test]
        public void AboveFifteen_TheTableXpByLevelAndRank()
        {
            Assert.AreEqual(EnemyLevelScaling.KillExperience(16), EnemyLevelScaling.LeveledKillExperience(10, 16, EnemyRank.Common, 16));
            Assert.AreEqual(EnemyLevelScaling.KillExperience(50), EnemyLevelScaling.LeveledKillExperience(10, 50, EnemyRank.Common, 50));
            Assert.AreEqual(128, EnemyLevelScaling.LeveledKillExperience(1, 50, EnemyRank.Common, 45));
            Assert.AreEqual(EnemyLevelScaling.KillExperience(50, EnemyRank.MapBoss), EnemyLevelScaling.LeveledKillExperience(10, 50, EnemyRank.MapBoss, 50));
            Assert.AreEqual(EnemyRank.MapBoss, EnemyLevelScaling.RankOf(true));
            Assert.AreEqual(EnemyRank.Common, EnemyLevelScaling.RankOf(false));
        }

        [Test]
        public void HeroFarAbove_XpIsCut_FivePercentPerLevelPastTen_TenPercentPastTwenty()
        {
            Assert.AreEqual(100, EnemyLevelScaling.LeveledKillExperience(100, 5, EnemyRank.Common, 15), "10 below: full");
            Assert.AreEqual(95, EnemyLevelScaling.LeveledKillExperience(100, 5, EnemyRank.Common, 16));
            Assert.AreEqual(50, EnemyLevelScaling.LeveledKillExperience(100, 5, EnemyRank.Common, 25), "20 below: half");
            Assert.AreEqual(10, EnemyLevelScaling.LeveledKillExperience(100, 5, EnemyRank.Common, 26), "past 20: the floor");
            Assert.AreEqual(1, EnemyLevelScaling.LeveledKillExperience(2, 1, EnemyRank.Common, 200), "a positive reward stays positive");
            Assert.AreEqual(0, EnemyLevelScaling.LeveledKillExperience(0, 1, EnemyRank.Common, 1), "0 stays 0");
            Assert.AreEqual(100, EnemyLevelScaling.LeveledKillExperience(100, 12, EnemyRank.Common, 1), "monster above: no cut");
            Assert.AreEqual(EnemyLevelScaling.KillExperience(30), EnemyLevelScaling.LeveledKillExperience(10, 30, EnemyRank.Common, 1));
        }

        [Test]
        public void Equipment_StopsDroppingPastTwentyLevelsBelow()
        {
            Assert.IsTrue(EnemyLevelScaling.DropsEquipment(25, 5));
            Assert.IsFalse(EnemyLevelScaling.DropsEquipment(26, 5));
            Assert.IsTrue(EnemyLevelScaling.DropsEquipment(1, 40), "a stronger monster always may");
        }

        [Test]
        public void MapMonsterInfo_IsTheCreatureLevelSource()
        {
            var go = new GameObject("map_monster");
            try
            {
                MapMonsterInfo info = go.AddComponent<MapMonsterInfo>();
                info.Configure(4, "map_002_tugai", "campina_lobos", null);
                Assert.IsTrue(go.TryGetComponent(out ICreatureLevelSource source));
                Assert.AreEqual(4, source.CreatureLevel);
                info.Configure(0, null, null, null);
                Assert.AreEqual(1, source.CreatureLevel, "never below 1");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        [Test]
        public void RegionBosses_TakeTheirOwnXpRank_WorldX23_MapX18()
        {
            Assert.AreEqual(23, RegionBossRules.WorldBossKillXpMultiplier);
            Assert.AreEqual(18, RegionBossRules.MapBossKillXpMultiplier);
            Assert.AreEqual(12, RegionBossRules.AllSpawns.Count);
            foreach (RegionBossSpawn spawn in RegionBossRules.AllSpawns)
            {
                Assert.AreEqual(RegionBossRules.WorldBossKillXpMultiplier, RegionBossRules.KillXpRankMultiplier(spawn.EnemyId, true), spawn.EnemyId);
            }

            Assert.AreEqual(36, RegionBossRules.MapSpawns.Count);
            foreach (RegionBossSpawn spawn in RegionBossRules.MapSpawns)
            {
                Assert.AreEqual(RegionBossRules.MapBossKillXpMultiplier, RegionBossRules.KillXpRankMultiplier(spawn.EnemyId, true), spawn.EnemyId);
            }

            Assert.AreEqual(9, RegionBossRules.KillXpRankMultiplier("enemy_some_campaign_boss", true), "any other boss: the generic x9");
            Assert.AreEqual(1, RegionBossRules.KillXpRankMultiplier("enemy_some_common", false));
            Assert.AreEqual(1, RegionBossRules.KillXpRankMultiplier(null, false));
        }

        [Test]
        public void KillExperienceTimes_MatchesTheRanks_AndTheLeveledPathTakesIt()
        {
            foreach (EnemyRank rank in new[] { EnemyRank.Common, EnemyRank.Elite, EnemyRank.MapBoss, EnemyRank.WorldBoss })
            {
                Assert.AreEqual(EnemyLevelScaling.KillExperience(200, rank), EnemyLevelScaling.KillExperienceTimes(200, EnemyLevelScaling.RankMultiplier(rank)), rank.ToString());
            }

            Assert.AreEqual(0, EnemyLevelScaling.KillExperienceTimes(200, -3), "a negative multiplier pays nothing");
            int common = EnemyLevelScaling.KillExperience(200);
            Assert.AreEqual(23 * common, EnemyLevelScaling.KillExperienceTimes(200, 23), 23);
            Assert.AreEqual(EnemyLevelScaling.KillExperienceTimes(200, 23), EnemyLevelScaling.LeveledKillExperience(10, 200, 23, 200));
            Assert.AreEqual(90, EnemyLevelScaling.LeveledKillExperience(90, 10, 23, 10), "up to 15 the asset XP is kept");
        }

        [Test]
        public void CampingAWorldBoss_PaysAbout1Point3TheCommons_AMapBossAbout1()
        {
            // A kill every RespawnSeconds + ~18 s of fight against 200 commons an hour (Progressao500 §1.3).
            double killsPerHour = 3600.0 / (RegionBossRules.RespawnSeconds + 18.0);
            foreach (int level in new[] { 72, 150, 264, 478 })
            {
                double commons = EnemyLevelScaling.KillExperience(level) * 200.0;
                double world = EnemyLevelScaling.KillExperienceTimes(level, RegionBossRules.WorldBossKillXpMultiplier) * killsPerHour / commons;
                double map = EnemyLevelScaling.KillExperienceTimes(level, RegionBossRules.MapBossKillXpMultiplier) * killsPerHour / commons;
                Assert.That(world, Is.InRange(1.1, 1.5), $"world boss {level}");
                Assert.That(map, Is.InRange(0.8, 1.2), $"map boss {level}");
            }
        }
    }
}
