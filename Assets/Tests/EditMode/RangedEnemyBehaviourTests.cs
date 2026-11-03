using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Finishing pass of the [distância] enemies: the keep-distance rule (<see cref="EnemySteering.RangedShouldBackOff"/>) shared by
    /// the map shooters and the ossuary archer, the strike rule (a shooter only hurts with its shot: the archer used to also land its
    /// 10-damage swing at 14 m) and one procedural shot sound per projectile (<see cref="EnemyShotAudio"/>).
    /// </summary>
    public class RangedEnemyBehaviourTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            foreach (DungeonProjectile shot in Object.FindObjectsByType<DungeonProjectile>(FindObjectsSortMode.None))
            {
                Object.DestroyImmediate(shot.gameObject);
            }
        }

        // ------------------------------------------------------------------
        // Keep-distance rule (pure)
        // ------------------------------------------------------------------

        [Test]
        public void TooClose_IsHalfTheRange_KeptIn3To4m_TheArcherKeepsIts4m()
        {
            Assert.AreEqual(4f, EnemySteering.RangedTooClose(14f), 1e-5f, "Arqueiro Esqueleto (14 m)");
            Assert.AreEqual(DungeonRules.ArcherRetreatTrigger, EnemySteering.RangedTooClose(DungeonRules.ArcherRange), 1e-5f);
            Assert.AreEqual(3.5f, EnemySteering.RangedTooClose(7f), 1e-5f, "Lanterna Quebrada (7 m)");
            Assert.AreEqual(4f, EnemySteering.RangedTooClose(9f), 1e-5f, "Gavião (9 m)");
            Assert.AreEqual(3f, EnemySteering.RangedTooClose(5f), 1e-5f, "floor");
            Assert.Less(EnemySteering.RangedTooClose(2f), 2f, "always inside the range");
        }

        [Test]
        public void Preferred_OpensPastTooClose_ButStaysInRange()
        {
            foreach (float range in new[] { 7f, 7.5f, 8f, 9f, 14f })
            {
                float tooClose = EnemySteering.RangedTooClose(range);
                float preferred = EnemySteering.RangedPreferred(range);
                Assert.Greater(preferred, tooClose, $"range {range}");
                Assert.LessOrEqual(preferred, range * EnemySteering.RangedPreferredFraction + 1e-5f, $"range {range}: still in reach of the next shot");
            }

            Assert.AreEqual(7f, EnemySteering.RangedPreferred(14f), 1e-5f);
        }

        [Test]
        public void ShouldBackOff_HasHysteresis_AndNeverForMelee()
        {
            const float range = 8f; // too close 4, preferred min(7, 6.4) = 6.4
            Assert.IsTrue(EnemySteering.RangedShouldBackOff(3.9f, range, false));
            Assert.IsFalse(EnemySteering.RangedShouldBackOff(5f, range, false), "planted: 5 m is fine");
            Assert.IsTrue(EnemySteering.RangedShouldBackOff(5f, range, true), "backing off: keeps going past 'too close'");
            Assert.IsFalse(EnemySteering.RangedShouldBackOff(6.5f, range, true), "stops at the preferred distance");
            Assert.IsFalse(EnemySteering.RangedShouldBackOff(0.5f, 0f, false), "no range, no back-off");

            Assert.IsTrue(DungeonRules.ArcherShouldRetreat(3f));
            Assert.IsFalse(DungeonRules.ArcherShouldRetreat(5f));
        }

        [Test]
        public void BackOffDirection_LeadsAway_FlankingToTheChosenSide()
        {
            Vector3 self = Vector3.zero;
            Vector3 hero = new Vector3(0f, 0f, 2f); // hero to the north: away is south
            Vector3 right = EnemySteering.RangedBackOffDirection(self, hero, true);
            Vector3 left = EnemySteering.RangedBackOffDirection(self, hero, false);

            foreach (Vector3 dir in new[] { right, left })
            {
                Assert.AreEqual(1f, dir.magnitude, 1e-4f);
                Assert.AreEqual(0f, dir.y, 1e-6f);
                Assert.Less(dir.z, -0.7f, "mostly away from the hero");
                Assert.AreEqual(EnemySteering.RangedFlankAngle, Vector3.Angle(dir, Vector3.back), 0.1f);
            }

            Assert.Less(right.x * left.x, 0f, "the two sides bend opposite ways");
            Assert.AreEqual(1f, EnemySteering.RangedBackOffDirection(hero, hero, true).magnitude, 1e-4f, "standing on the hero: still a direction");
        }

        // ------------------------------------------------------------------
        // Strike rule
        // ------------------------------------------------------------------

        [Test]
        public void StrikeKind_ShootersNeverSwing()
        {
            Assert.AreEqual(EnemyStrikeKind.Swing, EnemyRangedAttack.StrikeKindOf(false, false));
            Assert.AreEqual(EnemyStrikeKind.Projectile, EnemyRangedAttack.StrikeKindOf(true, false));
            Assert.AreEqual(EnemyStrikeKind.Projectile, EnemyRangedAttack.StrikeKindOf(true, true));
            Assert.AreEqual(EnemyStrikeKind.External, EnemyRangedAttack.StrikeKindOf(false, true));
        }

        [TestCase(DungeonRules.SkeletonArcherId, 14f, 10f, true)]
        [TestCase(DungeonRules.BoneSorcererId, 10f, 14f, false)]
        public void OssuaryShooter_StrikeDealsNoSwingDamageAtRange(string enemyId, float range, float damage, bool keepsDistance)
        {
            EnemyController enemy = CreateEnemy(range, damage, EnemyProjectileKind.None);
            DungeonEnemyBehaviour behaviour = DungeonDirector.AddBehaviour(enemy.gameObject, enemyId);
            Assert.IsNotNull(behaviour);
            Assert.AreEqual(EnemyStrikeKind.External, enemy.StrikeKind, "the component fires the arrow/orb");
            Assert.AreEqual(keepsDistance, enemy.KeepsDistance);

            Health hero = CreateTarget(enemy, new Vector3(range - 1f, 0f, 0f));
            float full = hero.CurrentHealth;
            int strikes = enemy.StrikeSequence;
            for (int i = 0; i < 120 && enemy.StrikeSequence == strikes; i++)
            {
                enemy.TickBrain(0.05f);
            }

            Assert.Greater(enemy.StrikeSequence, strikes, "setup: the brain struck");
            Assert.AreEqual(full, hero.CurrentHealth, $"{enemyId}: no {damage}-damage swing at {range - 1f} m, only its projectile hurts");
        }

        [Test]
        public void MeleeEnemy_StillSwings_AndAPooledArcherBodyForgetsTheClaim()
        {
            EnemyController enemy = CreateEnemy(5f, 10f, EnemyProjectileKind.None);
            enemy.ClaimRangedStrike(true);
            Assert.AreEqual(EnemyStrikeKind.External, enemy.StrikeKind);

            enemy.ResetForSpawn(enemy.Definition, Vector3.zero);
            Assert.AreEqual(EnemyStrikeKind.Swing, enemy.StrikeKind);
            Assert.IsFalse(enemy.KeepsDistance);

            Health hero = CreateTarget(enemy, new Vector3(1f, 0f, 0f));
            float full = hero.CurrentHealth;
            for (int i = 0; i < 60 && hero.CurrentHealth >= full; i++)
            {
                enemy.TickBrain(0.05f);
            }

            Assert.Less(hero.CurrentHealth, full, "a melee enemy still lands its swing");
        }

        [Test]
        public void MapShooter_BacksOff_WhenTheHeroClosesIn_WithoutStriking()
        {
            EnemyController enemy = CreateEnemy(8f, 12f, EnemyProjectileKind.Ember);
            Assert.IsTrue(enemy.KeepsDistance);
            Health hero = CreateTarget(enemy, new Vector3(2f, 0f, 0f));
            float full = hero.CurrentHealth;
            int strikes = enemy.StrikeSequence;
            bool backedOff = false;

            for (int i = 0; i < 20; i++)
            {
                enemy.TickBrain(0.05f);
                backedOff |= enemy.IsBackingOff;
            }

            Assert.IsTrue(backedOff, "too close (2 m < 4 m): it backs off");
            Vector3 flat = enemy.transform.position - hero.transform.position;
            flat.y = 0f;
            Assert.Greater(flat.magnitude, 2.3f, "it opened distance");
            Assert.AreEqual(strikes, enemy.StrikeSequence, "it does not shoot mid-step");
            Assert.AreEqual(full, hero.CurrentHealth);
        }

        [Test]
        public void MapShooter_AtComfortableRange_StaysPlanted_AndShoots()
        {
            EnemyController enemy = CreateEnemy(8f, 12f, EnemyProjectileKind.Feather);
            Health hero = CreateTarget(enemy, new Vector3(6f, 0f, 0f));
            float full = hero.CurrentHealth;
            int strikes = enemy.StrikeSequence;
            for (int i = 0; i < 80 && enemy.StrikeSequence == strikes; i++)
            {
                enemy.TickBrain(0.05f);
                Assert.IsFalse(enemy.IsBackingOff, "6 m is outside 'too close'");
            }

            Assert.Greater(enemy.StrikeSequence, strikes, "it shoots");
            Assert.AreEqual(full, hero.CurrentHealth, "the damage is the shot's, never a swing at 6 m");
            Assert.IsNotEmpty(Object.FindObjectsByType<DungeonProjectile>(FindObjectsSortMode.None), "a shot is in flight");
        }

        // ------------------------------------------------------------------
        // Shot sounds
        // ------------------------------------------------------------------

        [Test]
        public void EachProjectile_HasItsOwnShotCue_NotTheOssuaryArrow()
        {
            Assert.IsNull(EnemyShotAudio.CueOf(EnemyProjectileKind.None));
            EnemyProjectileKind[] kinds = { EnemyProjectileKind.Ember, EnemyProjectileKind.Feather, EnemyProjectileKind.LanternBolt, EnemyProjectileKind.LureGlob };
            string[] cues = kinds.Select(EnemyShotAudio.CueOf).ToArray();
            CollectionAssert.AllItemsAreNotNull(cues);
            CollectionAssert.AllItemsAreUnique(cues);
            CollectionAssert.DoesNotContain(cues, DungeonFx.ArrowCue);
            CollectionAssert.AreEquivalent(EnemyShotAudio.AllCues, cues);
            CollectionAssert.AreEquivalent(EnemyShotAudio.AllCues, EnemyShotAudio.BuildCues(false).Select(c => c.id));
            Assert.IsEmpty(EnemyShotAudio.AllCues.Intersect(DungeonFx.AllCues), "no clash with the ossuary cues");
        }

        [Test]
        public void ShotSynths_AreAudible_Bounded_Short_AndDistinct()
        {
            float[][] all = { EnemyShotAudio.GenerateEmber(), EnemyShotAudio.GenerateLanternBolt(), EnemyShotAudio.GenerateFeather(), EnemyShotAudio.GenerateLureGlob() };
            foreach (float[] samples in all)
            {
                Assert.Greater(samples.Length, EnemyShotAudio.SampleRate / 20, "at least 50 ms");
                Assert.Less(samples.Length, EnemyShotAudio.SampleRate / 2, "a shot, not a phrase (< 0.5 s)");
                float peak = samples.Max(Mathf.Abs);
                Assert.Greater(peak, 0.3f, "audible");
                Assert.LessOrEqual(peak, 1f, "no clipping");
                Assert.IsFalse(samples.Any(float.IsNaN));
            }

            for (int a = 0; a < all.Length; a++)
            {
                for (int b = a + 1; b < all.Length; b++)
                {
                    Assert.IsFalse(all[a].Length == all[b].Length && all[a].SequenceEqual(all[b]), $"synth {a} and {b} differ");
                }
            }

            CollectionAssert.AreEqual(EnemyShotAudio.GenerateEmber(), EnemyShotAudio.GenerateEmber(), "deterministic");
        }

        // ------------------------------------------------------------------

        private EnemyController CreateEnemy(float range, float damage, EnemyProjectileKind projectile)
        {
            var go = new GameObject("RangedTestEnemy");
            _created.Add(go);
            go.AddComponent<Health>();
            go.AddComponent<BuffController>();
            EnemyController enemy = go.AddComponent<EnemyController>();

            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(definition);
            SetPrivateField(definition, "_maxHealth", 100f);
            SetPrivateField(definition, "_detectionRadius", 20f);
            SetPrivateField(definition, "_loseTargetRadius", 50f);
            SetPrivateField(definition, "_attackRange", range);
            SetPrivateField(definition, "_attackCooldown", 1f);
            SetPrivateField(definition, "_attackDamage", damage);
            SetPrivateField(definition, "_fleeHealthThreshold", 0f);
            SetPrivateField(definition, "_chaseSpeed", 4f);
            SetPrivateField(definition, "_projectile", projectile);
            enemy.ResetForSpawn(definition, Vector3.zero);
            return enemy;
        }

        private Health CreateTarget(EnemyController enemy, Vector3 position)
        {
            var go = new GameObject("RangedTestHero");
            _created.Add(go);
            go.transform.position = position;
            Health health = go.AddComponent<Health>();
            enemy.Target = go.transform;
            return health;
        }

        private static void SetPrivateField(EnemyDefinition definition, string name, object value)
        {
            FieldInfo field = typeof(EnemyDefinition).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, name);
            field.SetValue(definition, value);
        }
    }
}
