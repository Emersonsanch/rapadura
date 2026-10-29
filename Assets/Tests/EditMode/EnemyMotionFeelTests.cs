using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Enemy "weight and personality" (Docs/Animation.md §7): profile scaling by body size/mass, the
    /// attack timeline (anticipation → stretched strike on the gameplay damage moment → follow-through),
    /// volume-preserving squash, death bounce / spawn overshoot, idle personality, and the
    /// "Reduzir Movimento" accessibility option (multiplier, shake, persistence).
    /// </summary>
    public class EnemyMotionFeelTests
    {
        private const string ReduceMotionKey = "Accessibility.ReduceMotion";
        private const string ShakeKey = "Accessibility.ScreenShakeIntensity";

        private static readonly EnemyMotionProfile[] Archetypes =
        {
            EnemyMotionProfile.Quadruped, EnemyMotionProfile.Arthropod, EnemyMotionProfile.Flyer, EnemyMotionProfile.Heavy,
        };

        private readonly List<Object> _created = new List<Object>();
        private AccessibilitySettings _settings;

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(ReduceMotionKey);
            PlayerPrefs.DeleteKey(ShakeKey);
            ServiceLocator.Unregister<AccessibilitySettings>();
        }

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
            _settings?.Shutdown();
            _settings = null;
            ServiceLocator.Unregister<AccessibilitySettings>();
            PlayerPrefs.DeleteKey(ReduceMotionKey);
            PlayerPrefs.DeleteKey(ShakeKey);
        }

        // ------------------------------------------------------------------ profile scaling

        [Test]
        public void ScaledForBody_BiggerIsHeavierAndSlower_ReferenceSizeIsUnchanged()
        {
            foreach (EnemyMotionProfile p in Archetypes)
            {
                EnemyMotionProfile same = p.ScaledForBody(1f);
                Assert.AreEqual(p.GaitEaseTime, same.GaitEaseTime, 1e-5f, p.Archetype + ": reference size keeps the authored feel");
                Assert.AreEqual(p.HitSpringHz, same.HitSpringHz, 1e-5f);
                Assert.AreEqual(p.Mass, same.Mass, 1e-5f);

                EnemyMotionProfile small = p.ScaledForBody(0.7f);
                EnemyMotionProfile big = p.ScaledForBody(1.6f);
                string id = p.Archetype.ToString();
                Assert.Greater(big.Mass, same.Mass, id + ": more mass");
                Assert.Greater(big.GaitEaseTime, same.GaitEaseTime, id + ": slower ease in/out");
                Assert.Less(big.LeanSpringHz, same.LeanSpringHz, id + ": more inertia (slower lean spring)");
                Assert.Less(big.SquashSpringHz, same.SquashSpringHz, id);
                Assert.Less(big.HitSpringHz, same.HitSpringHz, id);
                Assert.Less(big.RecoilDistance, same.RecoilDistance, id + ": shoved less");
                Assert.Less(big.SpawnOvershoot, same.SpawnOvershoot, id + ": less spawn overshoot");
                Assert.GreaterOrEqual(big.RecoverDuration, same.RecoverDuration, id);
                Assert.Less(small.GaitEaseTime, same.GaitEaseTime, id + ": a smaller body is snappier");

                EnemyMotionProfile boss = p.ScaledForBody(1f, isBoss: true);
                Assert.Greater(boss.Mass, same.Mass, id + ": bosses are heavier");
                Assert.Greater(boss.GaitEaseTime, same.GaitEaseTime, id);
            }
        }

        [Test]
        public void ScaledForBody_NeverTouchesGameplayFields()
        {
            foreach (EnemyMotionProfile p in Archetypes)
            {
                foreach (float size in new[] { 0.5f, 0.8f, 1f, 1.4f, 2f })
                {
                    foreach (bool boss in new[] { false, true })
                    {
                        EnemyMotionProfile s = p.ScaledForBody(size, boss);
                        string id = $"{p.Archetype} size {size} boss {boss}";
                        Assert.AreEqual(p.BaseWindup, s.BaseWindup, id + ": windup (telegraph) is gameplay");
                        Assert.AreEqual(p.Acceleration, s.Acceleration, id);
                        Assert.AreEqual(p.Deceleration, s.Deceleration, id);
                        Assert.AreEqual(p.TurnSmoothTime, s.TurnSmoothTime, id);
                        Assert.AreEqual(p.MaxTurnSpeed, s.MaxTurnSpeed, id);
                    }
                }
            }
        }

        [Test]
        public void ArchetypeWindups_AreTheShippedGameplayValues()
        {
            // The visual rework must not move any telegraph: these feed EnemyAttackState via ResolveWindup.
            Assert.AreEqual(0.42f, EnemyMotionProfile.Quadruped.BaseWindup, 1e-6f);
            Assert.AreEqual(0.4f, EnemyMotionProfile.Arthropod.BaseWindup, 1e-6f);
            Assert.AreEqual(0.36f, EnemyMotionProfile.Flyer.BaseWindup, 1e-6f);
            Assert.AreEqual(0.65f, EnemyMotionProfile.Heavy.BaseWindup, 1e-6f);
        }

        // ------------------------------------------------------------------ attack timeline vs gameplay windup

        [Test]
        public void WindupVisual_PeaksExactlyAtTheGameplayWindup_NeverEarlierOrLater()
        {
            foreach (EnemyMotionProfile archetype in Archetypes)
            {
                foreach (float size in new[] { 0.6f, 1f, 1.8f })
                {
                    EnemyMotionProfile p = archetype.ScaledForBody(size, size > 1.5f);
                    foreach (float windup in new[] { EnemyMotionMath.MinTelegraph, 0.42f, 0.65f, 0.9f })
                    {
                        string id = $"{p.Archetype} size {size} windup {windup}";
                        float strikeStart = EnemyMotionMath.StrikeStartTime(p, windup);
                        Assert.Greater(strikeStart, 0f, id);
                        Assert.Less(strikeStart, windup, id + ": the lunge happens inside the windup");
                        Assert.GreaterOrEqual(strikeStart, windup * 0.5f - 1e-5f, id + ": at least half the windup is readable anticipation");

                        // Never early: still short of full extension one frame before the damage moment.
                        EnemyPose before = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Windup, windup - 1f / 60f, windup);
                        Assert.Less(before.Offset.z, p.LungeDistance - 1e-3f, id + ": peak not reached before the damage moment");

                        // Exactly on time, and held (never later) if the strike report arrives a frame late.
                        EnemyPose peak = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Windup, windup, windup);
                        EnemyPose late = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Windup, windup + 0.1f, windup);
                        Assert.AreEqual(p.LungeDistance, peak.Offset.z, 1e-4f, id);
                        Assert.AreEqual(peak.Offset.z, late.Offset.z, 1e-5f, id + ": holds the peak until the strike is reported");
                        Assert.Less(peak.Squash, 0f, id + ": stretched at the strike");

                        // Anticipation: crouched/pulled back with squash during the coil.
                        EnemyPose coil = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Windup, strikeStart * 0.8f, windup);
                        Assert.Less(coil.Offset.z, 0f, id + ": pulls back");
                        Assert.Greater(coil.Squash, 0f, id + ": squash in the anticipation");
                        Assert.LessOrEqual(coil.Squash, MotionFeelMath.MaxSquash + 1e-5f, id);
                    }
                }
            }
        }

        [Test]
        public void Recover_StartsAtThePeak_FollowsThroughPastIt_EndsAtRest()
        {
            foreach (EnemyMotionProfile p in Archetypes)
            {
                Assert.AreEqual(1f, EnemyMotionMath.RecoverWeight(0f, p.FollowThrough), 1e-5f, "no pop at the strike");
                Assert.AreEqual(0f, EnemyMotionMath.RecoverWeight(1f, p.FollowThrough), 1e-5f);
                float max = 0f;
                for (float r = 0f; r <= 1f; r += 0.01f)
                {
                    max = Mathf.Max(max, EnemyMotionMath.RecoverWeight(r, p.FollowThrough));
                }

                Assert.AreEqual(1f + p.FollowThrough, max, 0.01f, p.Archetype + ": carries on past the peak");
                Assert.Greater(p.FollowThrough, 0f, p.Archetype.ToString());

                EnemyPose through = EnemyMotionMath.EvaluateAttack(p, EnemyAttackVisualPhase.Recover, p.RecoverDuration * EnemyMotionMath.FollowThroughPortion, 0.5f);
                Assert.Greater(through.Offset.z, p.LungeDistance, p.Archetype + ": follow-through extends past the lunge");
            }
        }

        [Test]
        public void Animator_HoldsTheWindupUntilTheControllerStrikes_AndControllerMotionStaysUnscaled()
        {
            EnemyController enemy = NewEnemy("enemy_test_wolf", out _);
            EnemyProceduralAnimator animator = AddAnimator(enemy, out _);
            EnemyMotionProfile before = enemy.Motion;
            float windupBefore = enemy.ResolvedAttackWindup;

            // EditMode never calls OnEnable, so the animator's first Tick is its BeginLife (it adopts the
            // controller's counters as "already seen"). Tick once first, as a live enemy has by the time it attacks.
            animator.Tick(1f / 60f);

            enemy.NotifyAttackWindup(0.45f);
            for (int i = 0; i < 45; i++) // 0.75 s: well past the windup, but no strike reported yet
            {
                animator.Tick(1f / 60f);
            }

            Assert.AreEqual(EnemyAttackVisualPhase.Windup, animator.AttackPhase, "the visual never ends the windup on its own");
            enemy.NotifyAttackStrike();
            animator.Tick(1f / 60f);
            Assert.AreEqual(EnemyAttackVisualPhase.Recover, animator.AttackPhase, "recovery starts on the gameplay strike");

            Assert.AreEqual(before.BaseWindup, enemy.Motion.BaseWindup);
            Assert.AreEqual(before.Acceleration, enemy.Motion.Acceleration);
            Assert.AreEqual(windupBefore, enemy.ResolvedAttackWindup, "gameplay windup unchanged by the animator");
        }

        // ------------------------------------------------------------------ squash, springs, reactions

        [Test]
        public void SpringVelocityForPeak_KickPeaksAtTheRequestedValue()
        {
            foreach ((float hz, float zeta) in new[] { (2.2f, 0.5f), (3.2f, 0.38f), (6f, 0.4f), (1.3f, 1f) })
            {
                float v = MotionFeelMath.SpringVelocityForPeak(0.2f, hz, zeta);
                float x = 0f, peak = 0f;
                for (int i = 0; i < 360; i++) // sampled at the integrator's own sub-step, so the peak is not missed between frames
                {
                    MotionFeelMath.StepSpring(ref x, ref v, 0f, hz, zeta, MotionFeelMath.MaxSpringStep);
                    peak = Mathf.Max(peak, x);
                }

                Assert.AreEqual(0.2f, peak, 0.004f, $"{hz} Hz ζ {zeta}");
            }

            Assert.AreEqual(0f, MotionFeelMath.SpringVelocityForPeak(0.2f, 0f, 0.5f));
            Assert.Greater(MotionFeelMath.EaseFrequency(0.2f), MotionFeelMath.EaseFrequency(0.4f), "longer ease = softer spring");
        }

        [Test]
        public void Animator_SquashIsVolumePreservingAndCapped_ThroughAttackAndDeath()
        {
            EnemyController enemy = NewEnemy("enemy_test_wolf", out _);
            EnemyProceduralAnimator animator = AddAnimator(enemy, out Transform visual);
            Vector3 rest = visual.localScale;
            float restVolume = rest.x * rest.y * rest.z;

            void Check(string when)
            {
                Vector3 s = visual.localScale;
                Assert.AreEqual(restVolume, s.x * s.y * s.z, restVolume * 1e-3f, when + ": volume preserved");
                Assert.LessOrEqual(Mathf.Abs(s.y / rest.y - 1f), MotionFeelMath.MaxSquash + 1e-4f, when + ": ≤ 6 %");
            }

            for (int i = 0; i < 40; i++)
            {
                enemy.transform.position += Vector3.forward * (4.5f / 60f);
                animator.Tick(1f / 60f);
                Check("spawn/run frame " + i);
            }

            enemy.NotifyAttackWindup(0.42f);
            for (int i = 0; i < 26; i++)
            {
                animator.Tick(1f / 60f);
                Check("windup frame " + i);
            }

            enemy.NotifyAttackStrike();
            enemy.Health.ApplyDamage(1_000_000f, ElementType.Physical, null);
            for (int i = 0; i < 60; i++)
            {
                animator.Tick(1f / 60f);
                Check("death frame " + i);
            }
        }

        [Test]
        public void DeathBounce_AndSpawnOvershoot_Curves()
        {
            Assert.AreEqual(0f, EnemyMotionMath.ImpactBounce01(0f), 1e-5f);
            Assert.AreEqual(1f, EnemyMotionMath.ImpactBounce01(EnemyMotionMath.FirstBounceDuration * 0.5f), 1e-4f, "first hop");
            float second = EnemyMotionMath.ImpactBounce01(EnemyMotionMath.FirstBounceDuration + EnemyMotionMath.SecondBounceDuration * 0.5f);
            Assert.AreEqual(EnemyMotionMath.SecondBounceHeight, second, 1e-4f, "smaller second hop");
            Assert.AreEqual(0f, EnemyMotionMath.ImpactBounce01(1f), 1e-5f, "then the sink takes over");

            float maxWith = 0f, maxWithout = 0f;
            for (float t = 0f; t <= EnemyMotionMath.SpawnRiseDuration; t += 0.005f)
            {
                maxWith = Mathf.Max(maxWith, EnemyMotionMath.SpawnRise01(t, 1.7f));
                maxWithout = Mathf.Max(maxWithout, EnemyMotionMath.SpawnRise01(t, 0f));
            }

            Assert.Greater(maxWith, 1.02f, "emerges past rest (overshoot)");
            Assert.LessOrEqual(maxWithout, 1f + 1e-4f, "no overshoot with 0");
        }

        [Test]
        public void TurnBrake_LeansBackOnlyWhenTurningHardWhileMoving()
        {
            EnemyMotionProfile wolf = EnemyMotionProfile.Quadruped;
            Assert.AreEqual(0f, EnemyMotionMath.TurnBrake01(wolf, 60f, 1f), 1e-5f, "gentle arc");
            Assert.AreEqual(0f, EnemyMotionMath.TurnBrake01(wolf, 400f, 0f), 1e-5f, "turning in place");
            Assert.AreEqual(1f, EnemyMotionMath.TurnBrake01(wolf, 400f, 1f), 1e-5f);
            Assert.AreEqual(0f, EnemyMotionMath.TurnBrake01(EnemyMotionProfile.Flyer, 400f, 1f), "flyers bank instead");

            Vector2 hardTurn = EnemyMotionMath.LeanTarget(wolf, 0f, 400f, 1f);
            Assert.Less(hardTurn.x, 0f, "rears back to brake");
            Assert.Less(hardTurn.y, 0f, "banks into a right turn");
            Assert.Greater(EnemyMotionMath.LeanTarget(wolf, 10f, 0f, 1f).x, 0f, "leans into a start");
            Assert.Less(EnemyMotionMath.SurgeTarget(wolf, 10f), 0f, "body trails behind on a start");
            Assert.Greater(EnemyMotionMath.SurgeTarget(wolf, -10f), 0f, "carries forward on a stop");
        }

        [Test]
        public void Animator_EasesIntoAndOutOfTheGait()
        {
            EnemyController enemy = NewEnemy("enemy_test_golem", out _);
            EnemyProceduralAnimator animator = AddAnimator(enemy, out _);
            for (int i = 0; i < 6; i++)
            {
                enemy.transform.position += Vector3.forward * (2.5f / 60f);
                animator.Tick(1f / 60f);
            }

            Assert.Less(animator.EasedSpeed, animator.MeasuredSpeed, "eases in: the heavy gait lags the real speed");
            for (int i = 0; i < 120; i++)
            {
                enemy.transform.position += Vector3.forward * (2.5f / 60f);
                animator.Tick(1f / 60f);
            }

            Assert.AreEqual(animator.MeasuredSpeed, animator.EasedSpeed, 0.1f, "catches up");
            for (int i = 0; i < 4; i++)
            {
                animator.Tick(1f / 60f);
            }

            Assert.Greater(animator.EasedSpeed, animator.MeasuredSpeed, "eases out: still settling after the stop");
        }

        // ------------------------------------------------------------------ idle personality

        [Test]
        public void Idle_WolfLooksAround_AndEveryArchetypeBreathes()
        {
            float maxYaw = 0f;
            for (float t = 0f; t < 30f; t += 0.05f)
            {
                maxYaw = Mathf.Max(maxYaw, Mathf.Abs(EnemyMotionMath.SampleIdle(EnemyMotionProfile.Quadruped, t, 1.3f, 0f).Euler.y));
            }

            Assert.Greater(maxYaw, 5f, "glances around while idle");
            Assert.AreEqual(default(EnemyPose).Euler, EnemyMotionMath.SampleIdle(EnemyMotionProfile.Quadruped, 12f, 1.3f, 1f).Euler, "no idle while running");

            foreach (EnemyMotionProfile p in Archetypes)
            {
                float min = 0f, max = 0f;
                for (float t = 0f; t < 10f; t += 0.05f)
                {
                    float s = EnemyMotionMath.SampleIdle(p, t, 0.7f, 0f).Squash;
                    min = Mathf.Min(min, s);
                    max = Mathf.Max(max, s);
                }

                Assert.Greater(max - min, 0.005f, p.Archetype + " breathes");
            }
        }

        [Test]
        public void Animator_TwitchesSeparateArthropodLegs()
        {
            Assert.IsTrue(EnemyProceduralAnimator.IsLegName("Leg_L1"));
            Assert.IsTrue(EnemyProceduralAnimator.IsLegName("perna_dir"));
            Assert.IsFalse(EnemyProceduralAnimator.IsLegName("Body"));

            EnemyController enemy = NewEnemy("enemy_test_spider", out _);
            Assert.AreEqual(EnemyMotionArchetype.Arthropod, enemy.Definition.MotionArchetype);
            GameObject visual = Primitive(enemy.transform);
            GameObject leg = Primitive(visual.transform);
            leg.name = "Leg_L1";
            EnemyProceduralAnimator animator = enemy.gameObject.AddComponent<EnemyProceduralAnimator>();
            animator.SetVisualRoot(visual.transform);
            Assert.AreEqual(1, animator.LegCount);

            Quaternion rest = leg.transform.localRotation;
            float maxAngle = 0f;
            for (int i = 0; i < 300; i++) // 5 s idle
            {
                animator.Tick(1f / 60f);
                maxAngle = Mathf.Max(maxAngle, Quaternion.Angle(rest, leg.transform.localRotation));
            }

            Assert.Greater(maxAngle, 3f, "the leg flicks");
            animator.SetVisualRoot(null);
            Assert.AreEqual(0f, Quaternion.Angle(rest, leg.transform.localRotation), 1e-3f, "restored when the visual is swapped out");
        }

        // ------------------------------------------------------------------ Reduzir Movimento

        [Test]
        public void ReduceMotion_DefaultsOff_PersistsAcrossInstances_AndRaisesChanged()
        {
            _settings = new AccessibilitySettings();
            _settings.Initialize();
            Assert.IsFalse(_settings.ReduceMotion, "default off");
            Assert.AreEqual(1f, _settings.MotionScale);

            int changes = 0;
            _settings.SettingsChanged += () => changes++;
            _settings.SetReduceMotion(true);
            Assert.AreEqual(1, changes);
            _settings.Shutdown();

            var second = new AccessibilitySettings();
            second.Initialize();
            Assert.IsTrue(second.ReduceMotion, "round-trips through PlayerPrefs");
            Assert.IsFalse(second.ReduceFlashEffects, "independent of Reduzir Flashes");
            second.SetReduceMotion(false);
            second.Shutdown();

            var third = new AccessibilitySettings();
            third.Initialize();
            Assert.IsFalse(third.ReduceMotion);
            third.Shutdown();
        }

        [Test]
        public void ReduceMotion_MultiplierAppliesToAnimators_ShakeAndUiEntrances()
        {
            Assert.AreEqual(1f, AccessibilitySettings.MotionScaleFor(null), "no settings: full motion");

            _settings = new AccessibilitySettings();
            _settings.Initialize();
            _settings.SetScreenShakeIntensity(0.8f);
            Assert.AreEqual(1f, AccessibilitySettings.MotionScaleFor(_settings));
            Assert.AreEqual(0.8f, _settings.ScaleShakeMagnitude(1f), 1e-5f);
            Assert.IsFalse(_settings.SuppressUiMotion);

            _settings.SetReduceMotion(true);
            Assert.AreEqual(AccessibilitySettings.ReducedMotionScale, AccessibilitySettings.MotionScaleFor(_settings), 1e-6f);
            Assert.AreEqual(0.3f, AccessibilitySettings.MotionScaleFor(_settings, 0.3f), 1e-6f, "per-animator override (Inspector)");
            Assert.AreEqual(0.8f * AccessibilitySettings.ReducedMotionShakeScale, _settings.ScaleShakeMagnitude(1f), 1e-5f, "weaker camera shake");
            Assert.IsTrue(_settings.SuppressUiMotion, "story/boss/tutorial entrances skip their slide");

            ServiceLocator.Register(_settings);
            EnemyController enemy = NewEnemy("enemy_test_wolf", out _);
            EnemyProceduralAnimator animator = AddAnimator(enemy, out _);
            animator.Tick(1f / 60f);
            Assert.AreEqual(AccessibilitySettings.ReducedMotionScale, animator.MotionScale, 1e-6f, "enemy animator reads the option");
        }

        [Test]
        public void ReduceMotion_HalvesTheEnemyGaitBob()
        {
            float full = MaxGaitBob(reduce: false);
            float reduced = MaxGaitBob(reduce: true);
            Assert.Greater(full, 0.01f);
            Assert.Less(reduced, full * 0.75f, $"bob reduced (full {full}, reduced {reduced})");
        }

        private float MaxGaitBob(bool reduce)
        {
            ServiceLocator.Unregister<AccessibilitySettings>();
            var settings = new AccessibilitySettings();
            settings.Initialize();
            settings.SetReduceMotion(reduce);
            ServiceLocator.Register(settings);

            EnemyController enemy = NewEnemy("enemy_test_wolf", out _);
            EnemyProceduralAnimator animator = AddAnimator(enemy, out Transform visual);
            Vector3 rest = visual.localPosition;
            float max = 0f;
            for (int i = 0; i < 150; i++)
            {
                enemy.transform.position += Vector3.forward * (2f / 60f); // trot
                animator.Tick(1f / 60f);
                if (i > 60) // after the spawn emerge and the ease-in
                {
                    max = Mathf.Max(max, Mathf.Abs(visual.localPosition.y - rest.y));
                }
            }

            settings.Shutdown();
            ServiceLocator.Unregister<AccessibilitySettings>();
            return max;
        }

        // ------------------------------------------------------------------ helpers

        private GameObject Primitive(Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _created.Add(go);
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            return go;
        }

        private EnemyProceduralAnimator AddAnimator(EnemyController enemy, out Transform visual)
        {
            GameObject body = Primitive(enemy.transform);
            body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
            body.transform.localScale = new Vector3(0.6f, 0.9f, 1.2f);
            visual = body.transform;
            EnemyProceduralAnimator animator = enemy.gameObject.AddComponent<EnemyProceduralAnimator>();
            animator.SetVisualRoot(visual);
            return animator;
        }

        private EnemyController NewEnemy(string assetName, out Health target)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definition.name = assetName;
            _created.Add(definition);
            SetPrivateField(definition, "_maxHealth", 100f);
            SetPrivateField(definition, "_attackCooldown", 1f);
            SetPrivateField(definition, "_attackDamage", 5f);
            SetPrivateField(definition, "_attackRange", 1.5f);
            SetPrivateField(definition, "_fleeHealthThreshold", 0f);
            SetPrivateField(definition, "_loseTargetRadius", 50f);

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
