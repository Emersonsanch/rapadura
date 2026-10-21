using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Guards the shape of the rebalance documented in <c>Docs/Balance/README.md</c> against the shipped
    /// assets (ProgressionCurve, PlayerCombatBalance, Resources/Enemies, hero skills). These are ranges and
    /// orderings rather than exact numbers, so designers can keep tuning without editing tests — a test
    /// fails only when a change breaks the intent (e.g. a boss a level-appropriate player can't outlast).
    /// </summary>
    public class BalanceInvariantsTests
    {
        /// <summary>PlayerBasicAttack._cooldownSeconds as serialized in Main.unity (one swing every 0.5 s).</summary>
        private const float BasicAttackCooldown = 0.5f;

        /// <summary>Expected player level while fighting in each biome (middle of the README §2 ranges).</summary>
        private static readonly Dictionary<string, int> ExpectedLevelByBiomePrefix = new Dictionary<string, int>
        {
            { "enemy_forest_", 2 },
            { "enemy_desert_", 4 },
            { "enemy_mountain_", 6 },
            { "enemy_cave_", 8 },
        };

        private static readonly string[] BiomeOrder = { "enemy_forest_", "enemy_desert_", "enemy_mountain_", "enemy_cave_" };

        private static ProgressionCurve Curve
        {
            get
            {
                ProgressionCurve curve = ProgressionCurve.LoadDefault();
                Assert.IsNotNull(curve, "Expected Resources/Balance/ProgressionCurve.asset.");
                return curve;
            }
        }

        private static List<EnemyDefinition> Enemies()
        {
            List<EnemyDefinition> enemies = Resources.LoadAll<EnemyDefinition>("Enemies").ToList();
            Assert.IsNotEmpty(enemies, "Expected EnemyDefinition assets under Resources/Enemies.");
            return enemies;
        }

        private static string BiomeOf(EnemyDefinition enemy)
        {
            string prefix = BiomeOrder.FirstOrDefault(p => enemy.name.StartsWith(p));
            Assert.IsNotNull(prefix, $"Enemy '{enemy.name}' doesn't follow the enemy_<biome>_ naming convention.");
            return prefix;
        }

        // ---------------------------------------------------------------
        // Progression
        // ---------------------------------------------------------------

        [Test]
        public void ExperienceToReachLevel10_IsWithinTargetRange()
        {
            // Old 1.25 curve: 2 579 XP (≈258 kills at 10 XP). Target: ultimates (level 10) reachable in a
            // first playthrough of the four biomes + hero quests.
            long total = Curve.GetTotalExperienceToReach(10);

            Assert.That(total, Is.InRange(1000L, 2000L));
        }

        [Test]
        public void ExperienceToReachLevel50_StaysReachable()
        {
            // Old curve: 22.4 million XP — progression effectively stopped around level 15.
            Assert.Less(Curve.GetTotalExperienceToReach(50), 250_000L);
        }

        [Test]
        public void PlayerMaxHealth_GrowsEveryLevel()
        {
            for (int level = 2; level <= ProgressionCurve.DefaultMaxLevel; level++)
            {
                Assert.Greater(Curve.GetMaxHealth(level), Curve.GetMaxHealth(level - 1), $"Max health at level {level}");
            }
        }

        // ---------------------------------------------------------------
        // Enemies
        // ---------------------------------------------------------------

        [Test]
        public void CommonEnemyExperience_GrowsWithBiomeTier()
        {
            List<EnemyDefinition> enemies = Enemies();
            int previous = 0;

            foreach (string biome in BiomeOrder)
            {
                int[] rewards = enemies.Where(e => !e.IsBoss && BiomeOf(e) == biome).Select(e => e.ExperienceReward).ToArray();
                Assert.IsNotEmpty(rewards, biome);
                Assert.Greater(rewards.Min(), previous, $"Commons in {biome} should give more XP than the previous biome.");
                previous = rewards.Max();
            }
        }

        [Test]
        public void Bosses_GiveSeveralTimesTheXpOfTheirBiomesCommons()
        {
            List<EnemyDefinition> enemies = Enemies();

            foreach (EnemyDefinition boss in enemies.Where(e => e.IsBoss))
            {
                int commonXp = enemies.Where(e => !e.IsBoss && BiomeOf(e) == BiomeOf(boss)).Max(e => e.ExperienceReward);
                Assert.GreaterOrEqual(BossBonusRules.GetExperienceReward(boss), 5 * commonXp, boss.name);
            }
        }

        [Test]
        public void BossHealth_DoesNotDropInLaterBiomes_AndDesertMountainCommonsOutlastForest()
        {
            List<EnemyDefinition> enemies = Enemies();
            float previousBoss = 0f;

            foreach (string biome in BiomeOrder)
            {
                float bossHp = enemies.Where(e => e.IsBoss && BiomeOf(e) == biome).Max(e => e.MaxHealth);
                Assert.GreaterOrEqual(bossHp, previousBoss, $"Boss of {biome}");
                previousBoss = bossHp;
            }

            float forestCommon = enemies.Where(e => !e.IsBoss && BiomeOf(e) == "enemy_forest_").Max(e => e.MaxHealth);
            Assert.Greater(enemies.Where(e => !e.IsBoss && BiomeOf(e) == "enemy_desert_").Min(e => e.MaxHealth), forestCommon);
            Assert.Greater(enemies.Where(e => !e.IsBoss && BiomeOf(e) == "enemy_mountain_").Min(e => e.MaxHealth), forestCommon);
        }

        /// <summary>
        /// DPS race at the biome's expected level, basic attack only (no skills, attributes, dodging or
        /// contact damage — the README's TTK model): the player must kill the boss before the boss's
        /// attacks kill the player, with at least 5% margin. Skills and Destreza crits are extra margin.
        /// </summary>
        [TestCase("enemy_forest_alpha_wolf")]
        [TestCase("enemy_desert_ruin_guardian")]
        [TestCase("enemy_mountain_warlord")]
        [TestCase("enemy_cave_dweller")]
        public void Boss_IsWinnableByALevelAppropriatePlayer(string bossId)
        {
            EnemyDefinition boss = Resources.Load<EnemyDefinition>("Enemies/" + bossId);
            Assert.IsNotNull(boss, bossId);
            PlayerCombatBalance balance = PlayerCombatBalance.LoadDefault();
            Assert.IsNotNull(balance, "Expected Resources/Balance/PlayerCombatBalance.asset.");

            int level = ExpectedLevelByBiomePrefix[BiomeOf(boss)];
            float playerHp = Curve.GetMaxHealth(level);
            float hitDamage = balance.BasicAttackDamage * Curve.GetDamageMultiplier(level);

            int hitsToKill = Mathf.CeilToInt(boss.MaxHealth / hitDamage);
            float playerTimeToKill = (hitsToKill - 1) * BasicAttackCooldown; // first swing lands at t = 0
            float bossDps = boss.AttackDamage / boss.AttackCooldown;
            float bossTimeToKill = playerHp / bossDps;

            Assert.Less(playerTimeToKill * 1.05f, bossTimeToKill,
                $"{bossId} at level {level}: player needs {playerTimeToKill:0.0}s, boss kills the player in {bossTimeToKill:0.0}s.");
        }

        [Test]
        public void MountainWarlord_IsNoLongerTheBossThatOutDamagesThePlayer()
        {
            // Was 26 dmg / 1.1 s = 23.6 DPS: killed a 100 HP player in 4.2 s, faster than anyone could kill it.
            EnemyDefinition warlord = Resources.Load<EnemyDefinition>("Enemies/enemy_mountain_warlord");
            Assert.IsNotNull(warlord);

            Assert.Less(warlord.AttackDamage / warlord.AttackCooldown, 20f);
        }

        // ---------------------------------------------------------------
        // Skills
        // ---------------------------------------------------------------

        [TestCase(CharacterId.Maithe)]
        [TestCase(CharacterId.Lavine)]
        [TestCase(CharacterId.Icaro)]
        public void DamageUltimate_HitsAtLeastTwiceAsHardAsTheHerosBestActive(CharacterId id)
        {
            IReadOnlyList<SkillDefinition> skills = CharacterSkillSet.GetSkills(id);
            SkillDefinition ultimate = skills.Single(s => s.SkillType == SkillType.Ultimate);
            float bestActive = skills.Where(s => s.SkillType == SkillType.Active).Max(s => s.BaseDamage);

            Assert.GreaterOrEqual(ultimate.BaseDamage, 2f * bestActive, $"{ultimate.SkillId} vs best active ({bestActive}).");
        }

        [Test]
        public void BasicAttack_IsWeakerThanBefore_SoSkillsMatter()
        {
            PlayerCombatBalance balance = PlayerCombatBalance.LoadDefault();
            Assert.IsNotNull(balance);

            // 15 per 0.5 s (30 DPS, free) out-damaged every skill in the game.
            Assert.Less(balance.BasicAttackDamage / BasicAttackCooldown, 30f);
        }
    }
}
