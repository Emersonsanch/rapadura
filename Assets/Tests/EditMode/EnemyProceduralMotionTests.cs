using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.States;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Rig-free enemy animation and movement feel: archetype resolution, gait frequency vs speed,
    /// telegraphed attack timing (windup → strike → recover), recoil direction, movement smoothing,
    /// and an end-to-end EnemyProceduralAnimator tick on a real hierarchy.
    /// </summary>
    public class EnemyProceduralMotionTests
    {
        private readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (Object o in _created)
            {
                if (o != null)
                {
                    Object.DestroyImmediate(o);
                }
            }

            _created.Clear();
        }

        private static readonly (string Id, EnemyMotionArchetype Expected)[] ShippedEnemies =
        {
            ("enemy_forest_wolf", EnemyMotionArchetype.Quadruped),
            ("enemy_forest_alpha_wolf", EnemyMotionArchetype.Quadruped),
            ("enemy_forest_spider", EnemyMotionArchetype.Arthropod),
            ("enemy_desert_scorpion", EnemyMotionArchetype.Arthropod),
            ("enemy_cave_bat_swarm", EnemyMotionArchetype.Flyer),
            ("enemy_cave_dweller", EnemyMotionArchetype.Heavy),
            ("enemy_desert_ruin_guardian", EnemyMotionArchetype.Heavy),
            ("enemy_mountain_stone_golem", EnemyMotionArchetype.Heavy),
            ("enemy_mountain_warlord", EnemyMotionArchetype.Heavy),
        };

        // ------------------------------------------------------------------ archetypes

        [Test]
        public void ShippedEnemies_HaveTheExpectedArchetype_AuthoredAndByNameDefault()
        {
            foreach ((string id, EnemyMotionArchetype expected) in ShippedEnemies)
            {
                EnemyDefinition definition = Resources.Load<EnemyDefinition>("Enemies/" + id);
                Assert.IsNotNull(definition, id);
                Assert.AreEqual(expected, definition.AuthoredMotionArchetype, id + " (asset value)");
                Assert.AreEqual(expected, definition.MotionArchetype, id);
                Assert.AreEqual(expected, EnemyMotionArchetypes.DefaultFor(definition.name, definition.EnemyName, definition.IsBoss),
                    id + ": Auto would resolve the same");
            }
        }

        [Test]
        public void ArchetypeDefaults_FallBackByBossFlag_AndNullIsQuadruped()
        {
            Assert.AreEqual(EnemyMotionArchetype.Heavy, EnemyMotionArchetypes.DefaultFor("enemy_unknown_thing", "Coisa", true));
            Assert.AreEqual(EnemyMotionArchetype.Quadruped, EnemyMotionArchetypes.DefaultFor("enemy_unknown_thing", "Coisa", false));
            Assert.AreEqual(EnemyMotionArchetype.Flyer, EnemyMotionArchetypes.DefaultFor(null, "Morcego Gigante", false));
            Assert.AreEqual(EnemyMotionArchetype.Arthropod, EnemyMotionArchetypes.DefaultFor(null, "Escorpião Rei", true));
            Assert.AreEqual(EnemyMotionArchetype.Quadruped, EnemyMotionArchetypes.Resolve(null));
        }

        // ------------------------------------------------------------------ gait

        [Test]
        public void StrideFrequency_GrowsWithSpeed_IsClamped_AndZeroAtRestForWalkers()
        {
            foreach (EnemyMotionProfile p in new[] { EnemyMotionProfile.Quadruped, EnemyMotionProfile.Arthropod, EnemyMotionProfile.Heavy })
            {
                Assert.AreEqual(0f, EnemyMotionMath.StrideFrequency(p, 0f), 1e-5f, p.Archetype + " at rest");
                float previous = 0f;
                for (float speed = 0.5f; speed <= 8f; speed += 0.5f)
                {
                    float hz = EnemyMotionMath.StrideFrequency(p, speed);
                    Assert.GreaterOrEqual(hz, previous - 1e-4f, $"{p.Archetype} monotonic at {speed}");
                    Assert.GreaterOrEqual(hz, p.MinStrideHz - 1e-4f);
                    Assert.LessOrEqual(hz, p.MaxStrideHz + 1e-4f);
                    previous = hz;
                }
            }

            Assert.Greater(EnemyMotionMath.StrideFrequency(EnemyMotionProfile.Flyer, 0f), 0f, "flyers keep flapping while hovering");
            Assert.Greater(EnemyMotionMath.StrideFrequency(EnemyMotionProfile.Arthropod, 3f), EnemyMotionMath.StrideFrequency(EnemyMotionProfile.Heavy, 3f),
                "skitter is much faster than a heavy stomp at the same speed");
            Assert.Less(EnemyMotionMath.StrideFrequency(EnemyMotionProfile.Quadruped, 2f, 1.4f), EnemyMotionMath.StrideFrequency(EnemyMotionProfile.Quadruped, 2f, 1f),
                "a bigger body (alpha wolf) takes longer, slower strides");
        }

        [Test]
        public void GallopBlend_TrotBelowThreshold_GallopAtChaseSpeed_OnlyForQuadrupeds()
        {
            EnemyMotionProfile wolf = EnemyMotionProfile.Quadruped;
            Assert.AreEqual(0f, EnemyMotionMath.GallopBlend(wolf, 2f), 1e-5f, "patrol (2 m/s) trots");
            Assert.AreEqual(1f, EnemyMotionMath.GallopBlend(wolf, 4.5f), 1e-5f, "chase (4.5 m/s) gallops");
            float mid = EnemyMotionMath.GallopBlend(wolf, (wolf.GallopStartSpeed + wolf.GallopFullSpeed) * 0.5f);
            Assert.That(mid, Is.InRange(0.1f, 0.9f));
            Assert.AreEqual(0f, EnemyMotionMath.GallopBlend(EnemyMotionProfile.Arthropod, 6f));
        }

        [Test]
        public void HeavySteps_TwoFootfallsPerGaitCycle()
        {
            EnemyMotionProfile heavy = EnemyMotionProfile.Heavy;
            const float speed = 2.5f;
            const float dt = 1f / 60f;
            float hz = EnemyMotionMath.StrideFrequency(heavy, speed);
            float phase = 0f;
            int lastStep = EnemyMotionMath.StepIndex(phase);
            int steps = 0;
            for (int frame = 0; frame < 600; frame++) // 10 s
            {
                phase += hz * dt;
                int step = EnemyMotionMath.StepIndex(phase);
                if (step != lastStep)
                {
                    steps++;
                    lastStep = step;
                }
            }

            Assert.AreEqual(hz * 2f * 10f, steps, 1.01f);
        }

        [Test]
        public void ArthropodGait_StaysLow_AndBobsLessThanAGallopingWolf()
        {
            float arthropodSum = 0f, arthropodPeak = 0f, wolfPeak = 0f;
            const int samples = 200;
            for (int i = 0; i < samples; i++)
            {
                float phase = i / (float)samples;
                EnemyPose skitter = EnemyMotionMath.SampleGait(EnemyMotionProfile.Arthropod, phase, 1f, 0f);
                EnemyPose gallop = EnemyMotionMath.SampleGait(EnemyMotionProfile.Quadruped, phase, 1f, 1f);
                arthropodSum += skitter.Offset.y;
                arthropodPeak = Mathf.Max(arthropodPeak, Mathf.Abs(skitter.Offset.y));
                wolfPeak = Mathf.Max(wolfPeak, Mathf.Abs(gallop.Offset.y));
            }

            Assert.Less(arthropodSum / samples, 0f, "body crouches while skittering");
            Assert.Less(arthropodPeak, wolfPeak * 0.6f);
        }

        [Test]
        public void Gait_IsStillAtRest_ExceptTheFlyerHover()
        {
            EnemyPose wolf = EnemyMotionMath.SampleGait(EnemyMotionProfile.Quadruped, 0.3f, 0f, 0f);
            Assert.AreEqual(Vector3.zero, wolf.Offset);
            Assert.AreEqual(Vector3.zero, wolf.Euler);

            EnemyPose bat = EnemyMotionMath.SampleGait(EnemyMotionProfile.Flyer, 0.3f, 0f, 0f);
            Assert.Greater(bat.Offset.y, 0f, "bats hover above the collider's feet");
        }

        // ------------------------------------------------------------------ attack timing

        [Test]
        public void ResolveWindup_IsReadable_AndFitsInsideEveryShippedCooldown()
        {
            foreach ((string id, _) in ShippedEnemies)
            {
                EnemyDefinition definition = Resources.Load<EnemyDefinition>("Enemies/" + id);
                EnemyMotionProfile profile = EnemyMotionProfile.For(definition.MotionArchetype);
                float windup = EnemyMotionMath.ResolveWindup(profile.BaseWindup, definition.AttackCooldown);

                Assert.GreaterOrEqual(windup, EnemyMotionMath.MinTelegraph, id);
                Assert.LessOrEqual(windup, definition.AttackCooldown - EnemyMotionMath.MinRecoverWindow + 1e-4f, id + ": strike cadence unchanged");
            }

            Assert.AreEqual(EnemyMotionMath.MinTelegraph, EnemyMotionMath.ResolveWindup(0.1f, 2f), 1e-5f);
            Assert.AreEqual(EnemyMotionMath.MinTelegraph, EnemyMotionMath.ResolveWindup(0.9f, 0.4f), 1e-5f, "never below the telegraph floor");
        }

        [Test]
        public void AttackPose_LeansBack_ThenLungePeaksAtTheDamageMoment_ThenRecovers()
        {
            foreach (EnemyMotionProfile p in new[] { EnemyMotionProfile.Quadruped, EnemyMotionProfile.Arthropod, EnemyMotionProfile.Flyer, EnemyMotionProfile.Heavy })
            {
                const float windup = 0.5f;
                EnemyPose coil = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Windup, windup * 0.5f, windup);
                Assert.Less(coil.Euler.x, 0f, p.Archetype + ": leans back / rears up in the windup");
                Assert.Less(coil.Offset.z, 0f, p.Archetype + ": pulls back");

                EnemyPose peak = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Windup, windup, windup);
                Assert.AreEqual(p.LungeDistance, peak.Offset.z, 1e-4f, p.Archetype + ": lunge fully extended at the strike");
                Assert.That(p.LungeDistance, Is.InRange(0.3f, 0.6f));
                Assert.Greater(peak.Euler.x, 0f, p.Archetype + ": lunges forward");

                EnemyPose recoverStart = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Recover, 0f, windup);
                Assert.AreEqual(peak.Offset.z, recoverStart.Offset.z, 1e-4f, "recover starts from the peak (no pop)");

                EnemyPose done = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Recover, p.RecoverDuration, windup);
                Assert.AreEqual(0f, done.Offset.magnitude, 1e-4f);
                Assert.AreEqual(0f, done.Euler.magnitude, 1e-3f);
            }

            Assert.Less(EnemyMotionMath.StrikePeak(EnemyMotionProfile.Flyer).Offset.y, 0f, "bats dip toward the target");
        }

        [Test]
        public void AttackState_TelegraphsThenStrikes_KeepingCooldownAndDamage()
        {
            EnemyController enemy = NewEnemy(Definition(cooldown: 1f, damage: 12f, range: 3f), out Health target);
            enemy.StateMachine.ChangeState<EnemyAttackState>();
            float windup = enemy.ResolvedAttackWindup;
            Assert.GreaterOrEqual(windup, EnemyMotionMath.MinTelegraph);

            const float dt = 0.01f;
            float time = 0f;
            float maxBefore = target.CurrentHealth;
            var strikeTimes = new List<float>();
            int strikes = enemy.StrikeSequence;
            bool sawWindup = false;
            for (int i = 0; i < 320; i++)
            {
                target.ClearInvulnerability();
                enemy.StateMachine.Tick(dt);
                time += dt;
                sawWindup |= enemy.IsWindingUp;
                if (enemy.StrikeSequence != strikes)
                {
                    strikes = enemy.StrikeSequence;
                    strikeTimes.Add(time);
                }
            }

            Assert.IsTrue(sawWindup);
            Assert.GreaterOrEqual(strikeTimes.Count, 3);
            Assert.AreEqual(windup, strikeTimes[0], 0.02f, "first strike lands right after the windup");
            Assert.AreEqual(1f, strikeTimes[1] - strikeTimes[0], 0.02f, "strike-to-strike = AttackCooldown");
            Assert.AreEqual(1f, strikeTimes[2] - strikeTimes[1], 0.02f);
            Assert.AreEqual(maxBefore - 12f * strikeTimes.Count, target.CurrentHealth, 0.01f, "damage per strike unchanged");
        }

        [Test]
        public void AttackState_LeavingReachDuringTheWindup_CancelsTheStrike()
        {
            EnemyController enemy = NewEnemy(Definition(cooldown: 1f, damage: 12f, range: 1.5f), out Health target);
            enemy.StateMachine.ChangeState<EnemyAttackState>();
            float before = target.CurrentHealth;
            int cancels = enemy.AttackCancelSequence;

            enemy.StateMachine.Tick(0.1f);
            Assert.IsTrue(enemy.IsWindingUp);

            target.transform.position = new Vector3(0f, 0f, 1.5f + EnemyAttackState.RangeHysteresis + 0.5f); // dodged back
            enemy.StateMachine.Tick(0.1f);

            Assert.IsTrue(enemy.StateMachine.IsInState<EnemyChaseState>());
            Assert.IsFalse(enemy.IsWindingUp);
            Assert.AreEqual(cancels + 1, enemy.AttackCancelSequence);
            Assert.AreEqual(before, target.CurrentHealth, "a dodged windup deals nothing");
        }

        // ------------------------------------------------------------------ reactions

        [Test]
        public void RecoilDirection_PointsAwayFromTheAttacker_InLocalSpace()
        {
            Quaternion facingNorth = Quaternion.identity;
            AssertDir(Vector3.back, EnemyMotionMath.RecoilDirectionLocal(Vector3.zero, facingNorth, true, new Vector3(0f, 0f, 2f)), "hit from the front → pushed back");
            AssertDir(Vector3.left, EnemyMotionMath.RecoilDirectionLocal(Vector3.zero, facingNorth, true, new Vector3(3f, 1f, 0f)), "hit from the right → pushed left");
            AssertDir(Vector3.forward, EnemyMotionMath.RecoilDirectionLocal(Vector3.zero, facingNorth, true, new Vector3(0f, 0f, -2f)), "hit from behind → pushed forward");

            Quaternion facingEast = Quaternion.Euler(0f, 90f, 0f);
            AssertDir(Vector3.back, EnemyMotionMath.RecoilDirectionLocal(Vector3.zero, facingEast, true, new Vector3(5f, 0f, 0f)), "rotation-aware");

            AssertDir(Vector3.back, EnemyMotionMath.RecoilDirectionLocal(Vector3.zero, facingNorth, false, Vector3.zero), "no source (DoT)");
            AssertDir(Vector3.back, EnemyMotionMath.RecoilDirectionLocal(Vector3.zero, facingNorth, true, Vector3.zero), "source on top of the enemy");
        }

        [Test]
        public void HitKick_ShovesAndTiltsAway_OvershootsBack_ThenSettles()
        {
            EnemyMotionProfile p = EnemyMotionProfile.Quadruped;
            EnemyHitKick hitFromFront = EnemyMotionMath.HitKick(p, Vector3.back, 1f);
            Assert.Less(hitFromFront.OffsetVelocity.y, 0f, "shoved back (local -Z)");
            Assert.Less(hitFromFront.TiltVelocity.x, 0f, "top tilts back (nose up)");
            Assert.Greater(hitFromFront.SquashVelocity, 0f, "short squash");

            EnemyHitKick pushedRight = EnemyMotionMath.HitKick(p, Vector3.right, 1f);
            Assert.Greater(pushedRight.OffsetVelocity.x, 0f);
            Assert.Less(pushedRight.TiltVelocity.y, 0f, "top tilts toward +X");

            // Integrate the shove spring the way the animator does: peaks at RecoilDistance, swings back past rest, settles.
            float x = 0f, v = hitFromFront.OffsetVelocity.y, min = 0f, max = 0f;
            for (int i = 0; i < 120; i++)
            {
                MotionFeelMath.StepSpring(ref x, ref v, 0f, p.HitSpringHz, p.HitSpringDamping, 1f / 60f);
                min = Mathf.Min(min, x);
                max = Mathf.Max(max, x);
            }

            Assert.AreEqual(-p.RecoilDistance, min, 0.01f, "first excursion = RecoilDistance away from the attacker");
            Assert.Greater(max, 0.005f, "overshoots back toward the attacker (spring follow-through)");
            Assert.AreEqual(0f, x, 0.005f, "settled after 2 s");
            Assert.AreEqual(Vector2.zero, EnemyMotionMath.HitKick(p, Vector3.back, 0f).OffsetVelocity, "zero strength, no kick");
        }

        [Test]
        public void ToppleAndSpawnCurves_StartAndEndAtRest()
        {
            Assert.AreEqual(0f, EnemyMotionMath.Topple01(0f), 1e-5f);
            Assert.Less(EnemyMotionMath.Topple01(EnemyMotionMath.ToppleDuration * 0.5f), 0.5f, "accelerating fall");
            Assert.AreEqual(1f, EnemyMotionMath.Topple01(EnemyMotionMath.ToppleDuration + 0.5f), 1e-5f);
            Assert.AreEqual(0f, EnemyMotionMath.SpawnRise01(0f), 1e-4f);
            Assert.AreEqual(1f, EnemyMotionMath.SpawnRise01(EnemyMotionMath.SpawnRiseDuration), 1e-5f);
        }

        // ------------------------------------------------------------------ movement smoothing

        [Test]
        public void MoveInDirection_Accelerates_AndReversesWithoutSnapping()
        {
            EnemyController enemy = NewEnemy(Definition(), out _);
            enemy.MoveInDirection(Vector3.forward, 4f, 0.05f);
            float first = enemy.Velocity.magnitude;
            Assert.Less(first, 4f, "eases in, no instant full speed");
            Assert.Greater(first, 0f);

            for (int i = 0; i < 40; i++)
            {
                enemy.MoveInDirection(Vector3.forward, 4f, 0.05f);
            }

            Assert.AreEqual(4f, enemy.Velocity.magnitude, 1e-3f, "reaches the commanded speed, never beyond");

            enemy.MoveInDirection(Vector3.back, 4f, 0.05f);
            Assert.Greater(enemy.Velocity.z, 0f, "one frame of reversal only brakes");

            Vector3 forwardBefore = enemy.transform.forward;
            enemy.MoveInDirection(Vector3.back, 4f, 0.05f);
            Assert.Less(Vector3.Angle(forwardBefore, enemy.transform.forward), 90f, "turns smoothly, no 180° snap in one frame");
        }

        [Test]
        public void TickBrain_CoastsToAStop_WhenNoStateMovesTheEnemy()
        {
            EnemyController enemy = NewEnemy(Definition(), out _);
            for (int i = 0; i < 40; i++)
            {
                enemy.MoveInDirection(Vector3.forward, 4f, 0.05f);
            }

            enemy.HoldAggro(5f); // holding: no state ticks, only coast + face
            Vector3 before = enemy.transform.position;
            for (int i = 0; i < 40; i++)
            {
                enemy.TickBrain(0.05f);
            }

            Assert.AreEqual(0f, enemy.Velocity.magnitude, 1e-4f);
            float drift = Vector3.Distance(before, enemy.transform.position);
            Assert.Greater(drift, 0f, "decelerates instead of stopping dead");
            Assert.Less(drift, 1f, "short coast");
        }

        [Test]
        public void ArriveSpeed_BrakesOnlyNearTheGoal()
        {
            Assert.AreEqual(4.5f, EnemyMotionMath.ArriveSpeed(4.5f, 5f, 20f), 1e-5f);
            Assert.Less(EnemyMotionMath.ArriveSpeed(4.5f, 0.1f, 20f), 4.5f);
            Assert.AreEqual(0.5f, EnemyMotionMath.ArriveSpeed(4.5f, 0f, 20f), 1e-5f, "never fully stalls short of range");
        }

        // ------------------------------------------------------------------ animator end to end

        [Test]
        public void Animator_PosesOnlyTheVisual_HoldsWhilePaused_AndTopplesOnDeath()
        {
            EnemyController enemy = NewEnemy(Definition(), out _);
            Transform root = enemy.transform;
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _created.Add(visual);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            visual.transform.SetParent(root, false);
            visual.transform.localPosition = new Vector3(0f, -0.5f, 0f);

            EnemyProceduralAnimator animator = root.gameObject.AddComponent<EnemyProceduralAnimator>();
            animator.SetVisualRoot(visual.transform);
            Vector3 restPosition = visual.transform.localPosition;
            Quaternion restRotation = visual.transform.localRotation;

            // Run forward for 1 s: gait engages, the collider root keeps its upright rotation.
            for (int i = 0; i < 60; i++)
            {
                root.position += Vector3.forward * (4.5f / 60f);
                animator.Tick(1f / 60f);
            }

            Assert.Greater(animator.MeasuredSpeed, 3f);
            Assert.AreEqual(Quaternion.identity, root.rotation, "root never posed");
            Assert.AreNotEqual(restPosition, visual.transform.localPosition, "visual animated");

            Vector3 held = visual.transform.localPosition;
            animator.Tick(0f);
            Assert.AreEqual(held, visual.transform.localPosition, "paused (deltaTime 0): pose held");

            // Telegraphed attack: windup pulls the visual back, the strike throws it forward.
            enemy.NotifyAttackWindup(0.45f);
            for (int i = 0; i < 12; i++)
            {
                animator.Tick(1f / 60f);
            }

            Assert.AreEqual(EnemyAttackVisualPhase.Windup, animator.AttackPhase);

            // Death: topples well past 60° from rest within the first ~0.4 s.
            enemy.Health.ApplyDamage(1_000_000f, ElementType.Physical, null);
            Assert.IsTrue(enemy.Health.IsDead);
            for (int i = 0; i < 30; i++)
            {
                animator.Tick(1f / 60f);
            }

            Assert.Greater(Quaternion.Angle(restRotation, visual.transform.localRotation), 60f);
            Assert.AreEqual(EnemyAttackVisualPhase.None, animator.AttackPhase, "death cancels the attack pose");
        }

        // ------------------------------------------------------------------ helpers

        private static void AssertDir(Vector3 expected, Vector3 actual, string message)
        {
            Assert.Less(Vector3.Distance(expected, actual), 1e-3f, $"{message}: expected {expected}, got {actual}");
        }

        private EnemyDefinition Definition(float cooldown = 1f, float damage = 8f, float range = 1.5f)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(definition);
            SetPrivateField(definition, "_maxHealth", 100f);
            SetPrivateField(definition, "_attackCooldown", cooldown);
            SetPrivateField(definition, "_attackDamage", damage);
            SetPrivateField(definition, "_attackRange", range);
            SetPrivateField(definition, "_fleeHealthThreshold", 0f);
            SetPrivateField(definition, "_loseTargetRadius", 50f);
            return definition;
        }

        private EnemyController NewEnemy(EnemyDefinition definition, out Health target)
        {
            var enemyObject = new GameObject("Enemy");
            _created.Add(enemyObject);
            enemyObject.AddComponent<Health>();
            EnemyController controller = enemyObject.AddComponent<EnemyController>();

            var targetObject = new GameObject("Target");
            _created.Add(targetObject);
            target = targetObject.AddComponent<Health>();
            target.ConfigureMaxHealth(1000f);
            targetObject.transform.position = new Vector3(0f, 0f, 1f);
            controller.Target = targetObject.transform;
            controller.ResetForSpawn(definition, Vector3.zero);
            return controller;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, $"Expected private field '{fieldName}' on {target.GetType().Name}.");
            field.SetValue(target, value);
        }
    }
}
