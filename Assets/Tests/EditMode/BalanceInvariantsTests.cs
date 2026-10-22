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
    /// fails only when a change breaks the intent (e.g. a boss that is trivial, or one that no hero can
    /// beat at the expected level even with defensive play; see README §5).
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

        // ---------------------------------------------------------------
        // Bosses and commons after the r6 rebalance (Docs/Balance/README.md §5)
        //
        // Model, at the biome's expected level, with no dodging, contact damage, crits or attributes:
        //  - player basic DPS = basic damage × damage_mult(level) / 0.5 s
        //  - hero kit DPS     = Σ (hit + DoT) / max(cooldown, cast + recovery) over the hero's damaging
        //                       actives × damage_mult, capped by the mana a 20 s fight allows
        //                       (100 pool + 5/s regen)
        //  - boss DPS         = attack damage / attack cooldown
        // Design target: a boss is a 10–35 s fight with the full kit; with the basic attack alone the boss
        // wins, and defensive play (Muralha-level damage reduction) lets every hero, even the low-damage
        // Joaquim/Maria, win it.
        // ---------------------------------------------------------------

        private const float PlayerManaPool = 100f;
        private const float PlayerManaRegenPerSecond = 5f;
        private const float ReferenceFightSeconds = 20f;

        private static readonly string[] BossIds =
        {
            "enemy_forest_alpha_wolf", "enemy_desert_ruin_guardian", "enemy_mountain_warlord", "enemy_cave_dweller",
        };

        private static readonly CharacterId[] Heroes =
        {
            CharacterId.Maithe, CharacterId.Lavine, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria,
        };

        private static EnemyDefinition LoadEnemy(string id)
        {
            EnemyDefinition enemy = Resources.Load<EnemyDefinition>("Enemies/" + id);
            Assert.IsNotNull(enemy, id);
            return enemy;
        }

        private static float BasicDps(int level)
        {
            PlayerCombatBalance balance = PlayerCombatBalance.LoadDefault();
            Assert.IsNotNull(balance, "Expected Resources/Balance/PlayerCombatBalance.asset.");
            return balance.BasicAttackDamage * Curve.GetDamageMultiplier(level) / BasicAttackCooldown;
        }

        private static float TickTotal(StatModifierDefinition[] effects)
        {
            float total = 0f;
            if (effects == null)
            {
                return total;
            }

            foreach (StatModifierDefinition effect in effects)
            {
                if (effect != null && effect.ticksOverTime && effect.tickIntervalSeconds > 0f)
                {
                    total += Mathf.Floor(effect.durationSeconds / effect.tickIntervalSeconds) * effect.tickDamageOrHeal;
                }
            }

            return total;
        }

        /// <summary>Sustained DPS of the hero's damaging active skills (skill level 1), mana-limited, at player <paramref name="level"/>.</summary>
        private static float KitDps(CharacterId hero, int level)
        {
            float dps = 0f;
            float manaPerSecond = 0f;

            foreach (SkillDefinition skill in CharacterSkillSet.GetSkills(hero).Where(s => s.SkillType == SkillType.Active))
            {
                float hit = skill.GetDamageForLevel(1) + TickTotal(skill.Debuffs);
                float cycle = Mathf.Max(skill.CooldownSeconds, skill.CastTimeSeconds + skill.RecoverySeconds);
                if (hit <= 0f || cycle <= 0f)
                {
                    continue;
                }

                dps += hit / cycle;
                manaPerSecond += skill.ManaCost / cycle;
            }

            float affordableManaPerSecond = PlayerManaPool / ReferenceFightSeconds + PlayerManaRegenPerSecond;
            float uptime = manaPerSecond > 0f ? Mathf.Min(1f, affordableManaPerSecond / manaPerSecond) : 1f;
            return dps * uptime * Curve.GetDamageMultiplier(level);
        }

        private static float BossDps(EnemyDefinition boss) => boss.AttackDamage / boss.AttackCooldown;

        private static float BossTimeToKillPlayer(EnemyDefinition boss, int level) => Curve.GetMaxHealth(level) / BossDps(boss);

        /// <summary>Damage reduction of Joaquim's Muralha de Pedra (Defense, PercentAdditive): the reference "defensive play".</summary>
        private static float DefensiveReduction()
        {
            SkillDefinition muralha = Resources.Load<SkillDefinition>("Skills/Characters/Joaquim/skill_joaquim_muralha_de_pedra");
            Assert.IsNotNull(muralha, "Expected Joaquim's Muralha de Pedra.");
            StatModifierDefinition defense = muralha.Buffs.Single(b => b.affectedStat == StatType.Defense);
            return Mathf.Clamp(defense.value, 0f, DamageCalculator.MaxPercentDefense);
        }

        [TestCase("enemy_forest_alpha_wolf", 350f, 550f)]
        [TestCase("enemy_desert_ruin_guardian", 400f, 650f)]
        [TestCase("enemy_mountain_warlord", 450f, 750f)]
        [TestCase("enemy_cave_dweller", 550f, 900f)]
        public void BossHealth_IsWithinItsBiomeBand(string bossId, float min, float max)
        {
            // r5 bosses (120–200 HP) died in 3–11 s in the r6 playtest with nobody dying; r6 makes them a real fight.
            Assert.That(LoadEnemy(bossId).MaxHealth, Is.InRange(min, max), bossId);
        }

        [TestCaseSource(nameof(BossIds))]
        public void Boss_KillsAnUndefendedPlayerInAFewRotations_NotInstantly(string bossId)
        {
            EnemyDefinition boss = LoadEnemy(bossId);
            int level = ExpectedLevelByBiomePrefix[BiomeOf(boss)];

            // Below ~12 s one mistake is unrecoverable; above ~40 s defensive skills stop mattering.
            Assert.That(BossTimeToKillPlayer(boss, level), Is.InRange(12f, 40f),
                $"{bossId} at level {level}: {BossDps(boss):0.0} DPS vs {Curve.GetMaxHealth(level)} HP.");
        }

        [TestCaseSource(nameof(BossIds))]
        public void Boss_WithTheFullKit_IsATenToThirtyFiveSecondFight_ForEveryHero(string bossId)
        {
            EnemyDefinition boss = LoadEnemy(bossId);
            int level = ExpectedLevelByBiomePrefix[BiomeOf(boss)];

            foreach (CharacterId hero in Heroes)
            {
                float seconds = boss.MaxHealth / (BasicDps(level) + KitDps(hero, level));
                Assert.That(seconds, Is.InRange(10f, 35f), $"{bossId} vs {hero} at level {level}");
            }
        }

        [TestCaseSource(nameof(BossIds))]
        public void Boss_OutlastsABasicAttackOnlyPlayer_SoSkillsAndDefenseMatter(string bossId)
        {
            EnemyDefinition boss = LoadEnemy(bossId);
            int level = ExpectedLevelByBiomePrefix[BiomeOf(boss)];
            float playerNeeds = boss.MaxHealth / BasicDps(level);
            float bossNeeds = BossTimeToKillPlayer(boss, level);

            Assert.Greater(playerNeeds, bossNeeds * 1.05f,
                $"{bossId} at level {level}: the basic attack alone kills it in {playerNeeds:0.0}s and it kills the player in {bossNeeds:0.0}s, so there is no death risk.");
        }

        [TestCaseSource(nameof(BossIds))]
        public void Boss_IsWinnableByEveryHero_WithDefensivePlay(string bossId)
        {
            EnemyDefinition boss = LoadEnemy(bossId);
            int level = ExpectedLevelByBiomePrefix[BiomeOf(boss)];
            float defendedBossNeeds = BossTimeToKillPlayer(boss, level) / (1f - DefensiveReduction());

            foreach (CharacterId hero in Heroes)
            {
                float playerNeeds = boss.MaxHealth / (BasicDps(level) + KitDps(hero, level));
                Assert.Less(playerNeeds * 1.05f, defendedBossNeeds,
                    $"{bossId} vs {hero} at level {level}: needs {playerNeeds:0.0}s, a defended player dies in {defendedBossNeeds:0.0}s.");
            }
        }

        [Test]
        public void Boss_CanBeOutDamagedWithoutDefense_ByTheBestDamageHero()
        {
            // Pure damage heroes (Maithe) can still race a boss; the others need their defensive kit.
            foreach (string bossId in BossIds)
            {
                EnemyDefinition boss = LoadEnemy(bossId);
                int level = ExpectedLevelByBiomePrefix[BiomeOf(boss)];
                float bestKit = Heroes.Max(hero => KitDps(hero, level));

                Assert.Less(boss.MaxHealth / (BasicDps(level) + bestKit), BossTimeToKillPlayer(boss, level), bossId);
            }
        }

        [Test]
        public void CommonEnemies_TakeSeveralSwings_AndAreNeverOneShotByTheKit()
        {
            foreach (EnemyDefinition common in Enemies().Where(e => !e.IsBoss))
            {
                int level = ExpectedLevelByBiomePrefix[BiomeOf(common)];

                // r5: 1.0–3.5 s basic-only (1.0–2.3 s in the r6 playtest with skills). r6: ~3–4.5 s basic-only.
                Assert.That(common.MaxHealth / BasicDps(level), Is.InRange(2.5f, 8f), $"{common.name} basic-only TTK at level {level}");

                float bestKit = Heroes.Max(hero => KitDps(hero, level));
                Assert.GreaterOrEqual(common.MaxHealth / (BasicDps(level) + bestKit), 1.5f, $"{common.name} with the best kit at level {level}");
            }
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
