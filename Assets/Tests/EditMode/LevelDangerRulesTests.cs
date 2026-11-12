using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Perigoso" — regra B (Docs/MetodologiaMapasERacas.md §4, Docs/Progressao500.md §1.2, <see cref="LevelDangerRules"/>):
    /// +25% creature crit from +10 levels, −40% hero hit and ×1.15 creature damage from +20 rising to ×1.3 at +30; nothing
    /// below +10 (the ramp was ×1.5 → ×3 until 2026-11-12, Docs/Balanco100a500.md §4.6). Then the wiring: <see cref="PlayerStats"/> takes the creature's hit through it, <see cref="Health"/> lets the
    /// hero miss, and campaign creatures (no <see cref="ICreatureLevelSource"/>) are never touched.
    /// </summary>
    public class LevelDangerRulesTests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            LevelDangerCombat.Random = null;
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.Clear();
            LevelDangerCombat.Random = null;
            foreach (Object created in _created)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _created.Clear();
        }

        // ------------------------------------------------------------------
        // Pure rule
        // ------------------------------------------------------------------

        [Test]
        public void Thresholds_AreTheMethodology_10_20_30()
        {
            Assert.AreEqual(10, LevelDangerRules.CritDelta);
            Assert.AreEqual(20, LevelDangerRules.HitRateDelta);
            Assert.AreEqual(30, LevelDangerRules.LethalDelta);
            Assert.AreEqual(0.25f, LevelDangerRules.CritChanceBonus);
            Assert.AreEqual(0.4f, LevelDangerRules.HitRatePenalty);
            Assert.AreEqual(1.15f, LevelDangerRules.HeavyDamageTaken);
            Assert.AreEqual(1.3f, LevelDangerRules.LethalDamageTaken);
        }

        [Test]
        public void SameLevel_OrBelow_OrUpToNineAbove_ChangesNothing()
        {
            foreach (int delta in new[] { -50, -1, 0, 1, 5, 9 })
            {
                Assert.IsTrue(LevelDangerRules.IsNone(delta), $"delta {delta}");
                Assert.IsFalse(LevelDangerRules.IsDangerous(delta), $"delta {delta}");
                Assert.AreEqual(0f, LevelDangerRules.CreatureCritChanceBonus(delta), $"delta {delta}");
                Assert.AreEqual(1f, LevelDangerRules.HeroHitRateMultiplier(delta), $"delta {delta}");
                Assert.AreEqual(1f, LevelDangerRules.CreatureDamageMultiplier(delta), $"delta {delta}");
            }

            Assert.AreEqual(40f, LevelDangerRules.ResolveCreatureHit(40f, 30, 30, 0f, out bool crit));
            Assert.IsFalse(crit, "a roll of 0 never crits at the hero's level");
            Assert.IsTrue(LevelDangerRules.HeroHitLands(30, 30, 0.9999f));
            Assert.IsTrue(LevelDangerRules.HeroHitLands(30, 39, 0.9999f));
        }

        [Test]
        public void FromTen_OnlyTheCritBonus()
        {
            foreach (int delta in new[] { 10, 15, 19 })
            {
                Assert.IsTrue(LevelDangerRules.IsDangerous(delta));
                Assert.AreEqual(0.25f, LevelDangerRules.CreatureCritChanceBonus(delta), 1e-6f);
                Assert.AreEqual(1f, LevelDangerRules.HeroHitRateMultiplier(delta));
                Assert.AreEqual(1f, LevelDangerRules.CreatureDamageMultiplier(delta));
            }

            Assert.AreEqual(60f, LevelDangerRules.ResolveCreatureHit(40f, 10, 20, 0.24f, out bool crit), 1e-4f);
            Assert.IsTrue(crit, "roll under 25% crits (×1.5)");
            Assert.AreEqual(40f, LevelDangerRules.ResolveCreatureHit(40f, 10, 20, 0.25f, out crit), 1e-4f);
            Assert.IsFalse(crit);
        }

        [Test]
        public void FromTwenty_HeroMisses40Percent_AndDamageScales_1_15_To_1_3()
        {
            Assert.AreEqual(0.6f, LevelDangerRules.HeroHitRateMultiplier(20), 1e-6f);
            Assert.AreEqual(0.6f, LevelDangerRules.HeroHitRateMultiplier(45), 1e-6f);
            Assert.IsTrue(LevelDangerRules.HeroHitLands(10, 30, 0.59f));
            Assert.IsFalse(LevelDangerRules.HeroHitLands(10, 30, 0.6f));

            Assert.AreEqual(1f, LevelDangerRules.CreatureDamageMultiplier(19), 1e-6f);
            Assert.AreEqual(1.15f, LevelDangerRules.CreatureDamageMultiplier(20), 1e-6f);
            Assert.AreEqual(1.225f, LevelDangerRules.CreatureDamageMultiplier(25), 1e-5f);
            Assert.AreEqual(1.285f, LevelDangerRules.CreatureDamageMultiplier(29), 1e-5f);
            Assert.AreEqual(1.3f, LevelDangerRules.CreatureDamageMultiplier(30), 1e-6f);
            Assert.AreEqual(1.3f, LevelDangerRules.CreatureDamageMultiplier(80), 1e-6f, "capped at ×1.3");
            Assert.IsTrue(LevelDangerRules.IsLethal(30));
            Assert.IsFalse(LevelDangerRules.IsLethal(29));

            // +30 with a crit: ×1.3 × ×1.5 = ×1.95.
            Assert.AreEqual(78f, LevelDangerRules.ResolveCreatureHit(40f, 1, 31, 0f, out bool crit), 1e-3f);
            Assert.IsTrue(crit);
            Assert.AreEqual(0f, LevelDangerRules.ResolveCreatureHit(0f, 1, 31, 0f, out crit));
            Assert.IsFalse(crit);
        }

        [Test]
        public void PlusThirty_WorstHit_IsUnderTwiceTheBlow_SoAWorldBossNeedsFiveSwings()
        {
            // Docs/Balanco100a500.md §4.6: an on-curve hero has 7.9–10.6 world-boss swings of health 30 levels below the boss
            // (Āz 478 the lowest); the worst +30 blow (damage cap × crit) must stay under 7.85 / 4 so even Āz needs 5.
            float worst = LevelDangerRules.CreatureDamageMultiplier(LevelDangerRules.LethalDelta) * LevelDangerRules.CreatureCriticalMultiplier;
            Assert.Less(worst, 7.85f / 4f);
            Assert.Greater(worst, 1.5f, "still clearly worse than a same-level crit");
            for (int delta = LevelDangerRules.HitRateDelta; delta < LevelDangerRules.LethalDelta; delta++)
            {
                Assert.LessOrEqual(LevelDangerRules.CreatureDamageMultiplier(delta), LevelDangerRules.CreatureDamageMultiplier(delta + 1), $"delta {delta}");
            }
        }

        [Test]
        public void MapAccessRules_AndTheWarning_UseTheSameRule()
        {
            for (int delta = -5; delta <= 40; delta++)
            {
                MonsterLevelPenalty penalty = MapAccessRules.GetMonsterLevelDeltaPenalty(10, 10 + delta);
                Assert.AreEqual(LevelDangerRules.CreatureCritChanceBonus(delta), penalty.MonsterCritChanceBonus, $"delta {delta}");
                Assert.AreEqual(LevelDangerRules.HeroHitRateMultiplier(delta), penalty.PlayerHitRateMultiplier, $"delta {delta}");
                Assert.AreEqual(LevelDangerRules.CreatureDamageMultiplier(delta), penalty.DamageTakenMultiplier, $"delta {delta}");
                Assert.AreEqual(LevelDangerRules.IsNone(delta), penalty.IsNone, $"delta {delta}");
                Assert.AreEqual(LevelDangerRules.IsDangerous(delta), EnemyLevelScaling.IsDangerous(10, 10 + delta), $"delta {delta}");
            }
        }

        // ------------------------------------------------------------------
        // Wiring
        // ------------------------------------------------------------------

        [Test]
        public void CreatureHit_OnTheHero_IsScaled_OnlyForALeveledCreatureFarAbove()
        {
            LevelDangerCombat.Random = () => 0.99f; // never a crit

            Assert.AreEqual(40f, HitHero(Creature(1)), 1e-3f, "same level as the hero (1): unchanged");
            Assert.AreEqual(40f, HitHero(Creature(10)), 1e-3f, "+9: unchanged");
            Assert.AreEqual(40f, HitHero(Campaign()), 1e-3f, "campaign creature without a level: unchanged");
            Assert.AreEqual(46f, HitHero(Creature(21)), 1e-3f, "+20: ×1.15");
            Assert.AreEqual(52f, HitHero(Creature(31)), 1e-3f, "+30: ×1.3");

            LevelDangerCombat.Random = () => 0f; // always under the bonus
            bool critical = false;
            System.Action<DamageAppliedEvent> onDamage = evt => critical = evt.IsCritical;
            EventBus.Subscribe(onDamage);
            Assert.AreEqual(60f, HitHero(Creature(11)), 1e-3f, "+10 and a crit: ×1.5");
            Assert.IsTrue(critical, "the hero sees a critical hit");
            Assert.AreEqual(40f, HitHero(Creature(1)), 1e-3f, "same level: a roll of 0 still never crits");
            Assert.IsFalse(critical);
            EventBus.Unsubscribe(onDamage);
        }

        [Test]
        public void HeroHit_OnACreatureTwentyAbove_CanMiss_NeverBelow()
        {
            PlayerStats hero = NewGameObject("DangerHero").AddComponent<PlayerStats>(); // level 1

            LevelDangerCombat.Random = () => 0.99f; // a miss whenever the rule allows one
            Assert.Greater(DamageCreature(Creature(1), hero.gameObject), 0f, "same level: the hit lands");
            Assert.Greater(DamageCreature(Creature(20), hero.gameObject), 0f, "+19: the hit lands");
            Assert.Greater(DamageCreature(Campaign(), hero.gameObject), 0f, "campaign creature: the hit lands");
            Assert.AreEqual(0f, DamageCreature(Creature(21), hero.gameObject), "+20 and a roll over 60%: Errou!");
            Assert.Greater(DamageCreature(Creature(21), null), 0f, "no hero behind the hit (a hazard): never misses");

            LevelDangerCombat.Random = () => 0.5f;
            Assert.Greater(DamageCreature(Creature(41), hero.gameObject), 0f, "a roll under 60% lands even at +40");
        }

        // ------------------------------------------------------------------

        private GameObject NewGameObject(string name)
        {
            var go = new GameObject(name);
            _created.Add(go);
            return go;
        }

        private GameObject Creature(int level)
        {
            GameObject go = NewGameObject("DangerCreature" + level);
            go.AddComponent<MapMonsterInfo>().Configure(level, "map_test", "zone_test", "enemy_test");
            return go;
        }

        private GameObject Campaign() => NewGameObject("DangerCampaignCreature");

        /// <summary>A fresh level-1 hero takes a 40-damage melee hit from <paramref name="attacker"/>; returns the damage taken.</summary>
        private float HitHero(GameObject attacker)
        {
            PlayerStats hero = NewGameObject("DangerHero").AddComponent<PlayerStats>();
            return hero.ApplyMeleeDamage(40f, ElementType.Physical, attacker);
        }

        /// <summary>The health <paramref name="creature"/> loses to a 10-damage hit from <paramref name="source"/>.</summary>
        private static float DamageCreature(GameObject creature, GameObject source)
        {
            Health health = creature.GetComponent<Health>() ?? creature.AddComponent<Health>();
            float before = health.CurrentHealth;
            health.ApplyDamage(10f, ElementType.Physical, source);
            return before - health.CurrentHealth;
        }
    }
}
