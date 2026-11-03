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
    }
}
