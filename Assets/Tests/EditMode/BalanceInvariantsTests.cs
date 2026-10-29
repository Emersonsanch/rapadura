using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Items.Procedural;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
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

        /// <summary>
        /// Player level at each region's boss for a player who does that region's two side quests
        /// (README §9.2, the r8 reference route; until r7 it was 2/4/6/8, a guess with no quests,
        /// attributes or skill points). <see cref="RegionQuestRoute_ReachesTheExpectedBossLevels"/> checks
        /// that the XP economy really lands there.
        /// </summary>
        private static readonly Dictionary<string, int> ExpectedLevelByBiomePrefix = new Dictionary<string, int>
        {
            { "enemy_forest_", 3 },
            { "enemy_desert_", 7 },
            { "enemy_mountain_", 10 },
            { "enemy_cave_", 15 },
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
        // Bosses and commons (r6 rebalance, re-based on the r8 progression: Docs/Balance/README.md §5, §9)
        //
        // Model, at the biome's expected level (the region-quest route), with no dodging, crits or gear:
        //  - attributes: 1 Vitalidade (+10 HP) and 1 Poder Arcano (+1% AttackDamage) per level gained
        //  - skill points: one per level gained, each into the damaging active with the best
        //                  damage-per-second gain (skill max level 5); the ultimate costs one at level 10
        //  - player basic DPS = basic damage × damage_mult(level) × (1 + Poder Arcano) / 0.5 s
        //  - hero kit DPS     = Σ (hit + DoT) / max(cooldown, cast + recovery) over the hero's damaging
        //                       actives × damage_mult × (1 + Poder Arcano), capped by the mana a 20 s fight
        //                       allows (100 pool + 5/s regen)
        //  - boss DPS         = attack damage / attack cooldown
        // Design target: a boss is a 10–35 s fight with the full kit; with the basic attack alone the boss
        // wins, and defensive play (Muralha-level damage reduction) lets every hero, even the low-damage
        // Joaquim/Maria, win it. tools/balance/run_r8.py runs the time-stepped version of this model
        // (stun, gear, both XP routes) that the README tables come from.
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

        /// <summary>AttackDamage percent from Poder Arcano when every level's second attribute point goes there.</summary>
        private static float ArcanePowerBonus(int level) => (level - 1) * AttributeSet.ArcanePowerAttackDamagePerPoint;

        /// <summary>Max health at <paramref name="level"/> with one Vitalidade point per level gained.</summary>
        private static float PlayerHealth(int level) => Curve.GetMaxHealth(level) + (level - 1) * AttributeSet.VitalityHealthPerPoint;

        private static float BasicDps(int level)
        {
            PlayerCombatBalance balance = PlayerCombatBalance.LoadDefault();
            Assert.IsNotNull(balance, "Expected Resources/Balance/PlayerCombatBalance.asset.");
            return balance.BasicAttackDamage * Curve.GetDamageMultiplier(level) * (1f + ArcanePowerBonus(level)) / BasicAttackCooldown;
        }

        private static float Cycle(SkillDefinition skill) => Mathf.Max(skill.CooldownSeconds, skill.CastTimeSeconds + skill.RecoverySeconds);

        /// <summary>Skill level of each damaging active at player <paramref name="level"/>: greedy skill points (see the model above).</summary>
        private static Dictionary<SkillDefinition, int> SkillLevels(CharacterId hero, int level, List<SkillDefinition> actives)
        {
            var levels = actives.ToDictionary(s => s, _ => 1);
            int points = level - 1;
            if (CharacterSkillSet.GetSkills(hero).Any(s => s.SkillType == SkillType.Ultimate && s.Requirement.minimumPlayerLevel <= level))
            {
                points--;
            }

            for (; points > 0; points--)
            {
                SkillDefinition best = actives
                    .Where(s => levels[s] < s.MaxLevel)
                    .OrderByDescending(s => (s.GetDamageForLevel(levels[s] + 1) - s.GetDamageForLevel(levels[s])) / Cycle(s))
                    .FirstOrDefault();
                if (best == null || best.GetDamageForLevel(levels[best] + 1) <= best.GetDamageForLevel(levels[best]))
                {
                    break;
                }

                levels[best]++;
            }

            return levels;
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

        /// <summary>Sustained DPS of the hero's damaging active skills (greedy skill points), mana-limited, at player <paramref name="level"/>.</summary>
        private static float KitDps(CharacterId hero, int level)
        {
            float dps = 0f;
            float manaPerSecond = 0f;
            List<SkillDefinition> actives = CharacterSkillSet.GetSkills(hero)
                .Where(s => s.SkillType == SkillType.Active && s.GetDamageForLevel(1) + TickTotal(s.Debuffs) > 0f)
                .ToList();
            Dictionary<SkillDefinition, int> levels = SkillLevels(hero, level, actives);

            foreach (SkillDefinition skill in actives)
            {
                float hit = skill.GetDamageForLevel(levels[skill]) + TickTotal(skill.Debuffs);
                float cycle = Cycle(skill);
                if (hit <= 0f || cycle <= 0f)
                {
                    continue;
                }

                dps += hit / cycle;
                manaPerSecond += skill.ManaCost / cycle;
            }

            float affordableManaPerSecond = PlayerManaPool / ReferenceFightSeconds + PlayerManaRegenPerSecond;
            float uptime = manaPerSecond > 0f ? Mathf.Min(1f, affordableManaPerSecond / manaPerSecond) : 1f;
            return dps * uptime * Curve.GetDamageMultiplier(level) * (1f + ArcanePowerBonus(level));
        }

        private static float BossDps(EnemyDefinition boss) => boss.AttackDamage / boss.AttackCooldown;

        private static float BossTimeToKillPlayer(EnemyDefinition boss, int level) => PlayerHealth(level) / BossDps(boss);

        /// <summary>Damage reduction of Joaquim's Muralha de Pedra (Defense, PercentAdditive): the reference "defensive play".</summary>
        private static float DefensiveReduction()
        {
            SkillDefinition muralha = Resources.Load<SkillDefinition>("Skills/Characters/Joaquim/skill_joaquim_muralha_de_pedra");
            Assert.IsNotNull(muralha, "Expected Joaquim's Muralha de Pedra.");
            StatModifierDefinition defense = muralha.Buffs.Single(b => b.affectedStat == StatType.Defense);
            return Mathf.Clamp(defense.value, 0f, DamageCalculator.MaxPercentDefense);
        }

        [TestCase("enemy_forest_alpha_wolf", 450f, 650f)]
        [TestCase("enemy_desert_ruin_guardian", 650f, 950f)]
        [TestCase("enemy_mountain_warlord", 750f, 1100f)]
        [TestCase("enemy_cave_dweller", 1000f, 1400f)]
        public void BossHealth_IsWithinItsBiomeBand(string bossId, float min, float max)
        {
            // r5 bosses (120–200 HP) died in 3–11 s in the r6 playtest with nobody dying; r6 made them a real
            // fight (430–700 HP). r8 re-bases them on the level the region quests really bring (README §9).
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
                // r8 (at the region-quest levels, with attributes): 3.3–5.8 s basic-only.
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
        // XP economy and loot (r8, README §9)
        // ---------------------------------------------------------------

        private static readonly BiomeType[] RegionOrder = { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };

        /// <summary>WorldMap.unity: random encounters bring 1–2 commons; the boss challenge brings 0–2 minions.</summary>
        private const float CommonsPerEncounter = 1.5f;
        private const float BossMinions = 1f;
        private const int VictoriesToChallengeBoss = 3;

        private static float LevelForExperience(float experience)
        {
            for (int level = 1; level < ProgressionCurve.DefaultMaxLevel; level++)
            {
                long next = Curve.GetTotalExperienceToReach(level + 1);
                if (experience < next)
                {
                    long current = Curve.GetTotalExperienceToReach(level);
                    return level + (experience - current) / (next - current);
                }
            }

            return ProgressionCurve.DefaultMaxLevel;
        }

        /// <summary>
        /// Fractional player level when each region's boss fight starts, for a player who does both region
        /// quests first (<paramref name="doQuests"/>) or only the 3 victories the boss challenge needs. Kill
        /// objectives are met at the spawn-weight rate of their enemy; the second quest starts after the first.
        /// </summary>
        private static Dictionary<BiomeType, float> LevelAtEachBoss(bool doQuests)
        {
            var result = new Dictionary<BiomeType, float>();
            float experience = 0f;

            foreach (BiomeType region in RegionOrder)
            {
                BiomeDefinition biome = Resources.Load<BiomeDefinition>("Biomes/" + region);
                Assert.IsNotNull(biome, "Expected Resources/Biomes/" + region);
                var commons = biome.Enemies.Where(e => e.Enemy != null && !e.Enemy.IsBoss && e.Weight > 0).ToList();
                float totalWeight = commons.Sum(e => e.Weight);
                float averageXp = commons.Sum(e => e.Enemy.ExperienceReward * (float)e.Weight) / totalWeight;
                EnemyDefinition boss = biome.Enemies.First(e => e.Enemy != null && e.Enemy.IsBoss && e.Weight > 0).Enemy;

                float fights = VictoriesToChallengeBoss;
                float questXp = 0f;
                if (doQuests)
                {
                    float questFights = 0f;
                    foreach (RegionQuestSpec quest in RegionQuestCatalog.QuestsFor(region))
                    {
                        float needed = 0f;
                        foreach (RegionObjectiveSpec objective in quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType))
                        {
                            float share = commons.Where(e => e.Enemy.name == objective.TargetId).Sum(e => e.Weight) / totalWeight;
                            Assert.Greater(share, 0f, $"{quest.QuestId}: {objective.TargetId} is not a common of {region}");
                            needed = Mathf.Max(needed, objective.RequiredAmount / (CommonsPerEncounter * share));
                        }

                        questFights += needed;
                        questXp += quest.RewardExperience;
                    }

                    fights = Mathf.Max(fights, questFights);
                }

                experience += fights * CommonsPerEncounter * averageXp + questXp;
                result[region] = LevelForExperience(experience);
                experience += BossBonusRules.GetExperienceReward(boss) + BossMinions * averageXp;
            }

            return result;
        }

        [TestCase(BiomeType.Forest, 3f, 4.5f)]
        [TestCase(BiomeType.Desert, 7f, 8.5f)]
        [TestCase(BiomeType.Mountain, 10f, 11.5f)]
        [TestCase(BiomeType.Caves, 15f, 16.5f)]
        public void RegionQuestRoute_ReachesTheExpectedBossLevels(BiomeType region, float min, float max)
        {
            // r8: the region quests gave 2 910 XP (on top of ~2 000 from the kills they need and the bosses)
            // and brought the player to the bosses at levels 5.5 / 10 / 14.5 / 19, where every fight was
            // trivial. The boss/common invariants above assume the (floored) levels pinned here.
            float level = LevelAtEachBoss(doQuests: true)[region];
            Assert.That(level, Is.InRange(min - 0.1f, max), $"{region}: level {level:0.00} at the boss");
            Assert.AreEqual(Mathf.RoundToInt(min), ExpectedLevelByBiomePrefix[BiomeOrder[(int)region]], "keep ExpectedLevelByBiomePrefix in sync");
        }

        [Test]
        public void BossRushRoute_IsBelowTheQuestRoute()
        {
            // Skipping the side quests (3 wins, then the boss) is allowed but harder: 1.5 / 3 / 5.3 / 7.9 in r8.
            Dictionary<BiomeType, float> rush = LevelAtEachBoss(doQuests: false);
            Dictionary<BiomeType, float> quests = LevelAtEachBoss(doQuests: true);

            foreach (BiomeType region in RegionOrder)
            {
                Assert.Less(rush[region], quests[region], region.ToString());
                Assert.GreaterOrEqual(rush[region], 1f, region.ToString());
            }
        }

        [Test]
        public void RegionQuestXp_StaysInProportionToTheKillsItAsksFor()
        {
            // r7 shipped 2 910 XP of region quests, more than all the fights they ask for plus the four
            // bosses (~2 000). r8: 1 140 in total, each quest worth 0.5–2.5× the XP of its own kills.
            Assert.That(RegionQuestCatalog.Quests.Sum(q => q.RewardExperience), Is.InRange(800, 1500));

            foreach (RegionQuestSpec quest in RegionQuestCatalog.Quests)
            {
                float killXp = 0f;
                foreach (RegionObjectiveSpec objective in quest.Objectives.Where(o => o.Type == QuestObjectiveType.KillEnemyType))
                {
                    EnemyDefinition target = LoadEnemy(objective.TargetId);
                    killXp += objective.RequiredAmount * target.ExperienceReward;
                }

                Assert.That(quest.RewardExperience, Is.InRange(killXp * 0.5f, killXp * 2.5f), $"{quest.QuestId}: its kills give {killXp} XP");
            }
        }

        [Test]
        public void FlatAttackDamageAffixes_NeverDoubleTheBasicAttack()
        {
            // Flat AttackDamage is added to every hit (OutgoingDamage.Resolve). With a 10-damage basic attack,
            // Brutal 6–12 + Afiada 2–5 on one Rare weapon gave up to +170% basic damage, and the luckiest 10%
            // of players beat the bosses in 13–15 s instead of 17–20 s (README §9.4).
            PlayerCombatBalance balance = PlayerCombatBalance.LoadDefault();
            Assert.IsNotNull(balance);
            List<ItemAffixDefinition> flat = AffixDatabase.GetAll()
                .Where(a => a.AffectedStat == StatType.AttackDamage && a.Application == ModifierApplication.Flat)
                .ToList();
            Assert.IsNotEmpty(flat);

            float bestTwo = flat.Select(a => a.MaxValue).OrderByDescending(v => v).Take(2).Sum();
            Assert.Less(bestTwo, balance.BasicAttackDamage, "two flat affixes on one weapon");
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
