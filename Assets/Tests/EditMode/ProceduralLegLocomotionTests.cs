using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Locomotion;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests of <see cref="ProceduralLegLocomotion"/> on the real jointed procedural models:
    /// leg chains found in gait order per kind, the IK putting each foot transform on its planted point
    /// and keeping it there while the body moves and crouches (no sliding through the IK), weight 0 =
    /// rest pose, and <see cref="EnemyProceduralAnimator"/> end to end (legs wake after the emerge, feet
    /// step and stay planted between steps, legs rest on death and re-plant after a pool respawn).
    /// </summary>
    public class ProceduralLegLocomotionTests
    {
        private const float Dt = 1f / 60f;
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

        private GameObject Root()
        {
            var root = new GameObject("Enemy");
            _created.Add(root);
            root.AddComponent<CapsuleCollider>().height = 2f;
            return root;
        }

        /// <summary>The cached model of <paramref name="kind"/> seated on the root's feet, like EnemyVisualSwapper does.</summary>
        private static Transform Model(ProceduralEnemyKind kind, GameObject root)
        {
            GameObject visual = ProceduralEnemyModels.Instantiate(kind, root.transform);
            visual.transform.SetLocalPositionAndRotation(new Vector3(0f, EnemyVisualSwapper.GetFootHeight(root.transform), 0f), Quaternion.identity);
            EnemyVisualSwapper.GroundModel(root.transform, visual.transform);
            return visual.transform;
        }

        private ProceduralLegLocomotion Bound(ProceduralEnemyKind kind, out GameObject root, out Transform visual)
        {
            root = Root();
            visual = Model(kind, root);
            var loco = visual.gameObject.AddComponent<ProceduralLegLocomotion>();
            loco.Bind(root.transform, visual, EnemyVisualSwapper.GetFootHeight(root.transform));
            return loco;
        }

        private static Transform Find(Transform visual, string partName)
        {
            foreach (Transform t in visual.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == partName)
                {
                    return t;
                }
            }

            return null;
        }

        [Test]
        public void Bind_FindsTheJointedLegs_WithTheGaitForTheirCount()
        {
            var expected = new Dictionary<ProceduralEnemyKind, (int Legs, LegGaitStyle Style)>
            {
                { ProceduralEnemyKind.Wolf, (4, LegGaitStyle.Quadruped) },
                { ProceduralEnemyKind.AlphaWolf, (4, LegGaitStyle.Quadruped) },
                { ProceduralEnemyKind.Spider, (8, LegGaitStyle.Arthropod) },
                { ProceduralEnemyKind.Scorpion, (8, LegGaitStyle.Arthropod) },
                { ProceduralEnemyKind.StoneGolem, (2, LegGaitStyle.Biped) },
                { ProceduralEnemyKind.Warlord, (2, LegGaitStyle.Biped) },
                { ProceduralEnemyKind.RuinGuardian, (2, LegGaitStyle.Biped) },
                { ProceduralEnemyKind.AncientDweller, (2, LegGaitStyle.Biped) },
            };

            foreach (KeyValuePair<ProceduralEnemyKind, (int Legs, LegGaitStyle Style)> entry in expected)
            {
                ProceduralLegLocomotion loco = Bound(entry.Key, out _, out _);
                Assert.IsTrue(loco.HasLegs, entry.Key.ToString());
                Assert.AreEqual(entry.Value.Legs, loco.LegCount, entry.Key.ToString());
                Assert.AreEqual(entry.Value.Style, loco.DetectedStyle, entry.Key.ToString());
                Assert.Greater(loco.MeanLegLength, 0.2f, entry.Key.ToString());

                // Gait order: front to back, left before right.
                LegGaitSolver solver = loco.Solver;
                Assert.AreEqual(-1, solver.Side(0), $"{entry.Key}: leg 0 is a left leg");
                Assert.AreEqual(1, solver.Side(1), $"{entry.Key}: leg 1 is a right leg");
            }

            ProceduralLegLocomotion bats = Bound(ProceduralEnemyKind.BatSwarm, out _, out _);
            Assert.IsFalse(bats.HasLegs, "bats fly");
        }

        [Test]
        public void Solve_PutsTheFeetOnTheirPlantedPoints_AndKeepsThemThereWhileTheBodyMovesAndCrouches()
        {
            foreach (ProceduralEnemyKind kind in new[] { ProceduralEnemyKind.Wolf, ProceduralEnemyKind.Spider, ProceduralEnemyKind.StoneGolem })
            {
                ProceduralLegLocomotion loco = Bound(kind, out GameObject root, out Transform visual);
                loco.Advance(Dt, Vector3.zero, 0f);
                loco.Solve(Dt, 1f);

                var feet = new List<Transform>();
                foreach (Transform t in visual.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.StartsWith(ProceduralLegLocomotion.FootPrefix))
                    {
                        feet.Add(t);
                    }
                }

                Assert.AreEqual(loco.LegCount, feet.Count, kind.ToString());
                var planted = new Vector3[feet.Count];
                for (int i = 0; i < feet.Count; i++)
                {
                    planted[i] = feet[i].position;
                }

                Vector3 hipToFoot = Find(visual, ProceduralLegLocomotion.HipPrefix + feet[0].name.Substring(ProceduralLegLocomotion.FootPrefix.Length)).position - planted[0];

                // The body creeps forward 3 cm (no step needed) and crouches 4 cm: the feet must not move.
                root.transform.position += Vector3.forward * 0.03f;
                visual.localPosition += Vector3.down * 0.04f;
                loco.Advance(Dt, Vector3.forward * 0.03f / Dt * 0.1f, 0f);
                loco.Solve(Dt, 1f);
                for (int i = 0; i < feet.Count; i++)
                {
                    Assert.Less(Vector3.Distance(planted[i], feet[i].position), 0.01f, $"{kind} {feet[i].name} slid");
                }

                Vector3 hipToFootNow = Find(visual, ProceduralLegLocomotion.HipPrefix + feet[0].name.Substring(ProceduralLegLocomotion.FootPrefix.Length)).position - feet[0].position;
                Assert.Less(hipToFootNow.magnitude, hipToFoot.magnitude, $"{kind}: the knee bent to absorb the crouch");

                // Weight 0 = rest pose: the legs ride the body again.
                Quaternion[] rest = new Quaternion[feet.Count];
                loco.Solve(Dt, 0f);
                loco.RestoreRest();
                for (int i = 0; i < feet.Count; i++)
                {
                    rest[i] = feet[i].parent.localRotation;
                }

                loco.Solve(Dt, 0f);
                for (int i = 0; i < feet.Count; i++)
                {
                    Assert.AreEqual(rest[i], feet[i].parent.localRotation, $"{kind}: weight 0 leaves the rest pose");
                }
            }
        }

        // ------------------------------------------------------------------ animator end to end

        [Test]
        public void Animator_StepsTheWolf_AfterTheEmerge_FeetStayPlanted_RestOnDeath_ReplantAfterRespawn()
        {
            EnemyDefinition definition = Definition();
            EnemyController enemy = NewEnemy(definition);
            GameObject root = enemy.gameObject;
            Transform visual = Model(ProceduralEnemyKind.Wolf, root);

            EnemyProceduralAnimator animator = root.AddComponent<EnemyProceduralAnimator>();
            animator.AlwaysAnimateLegs = true;
            animator.SetVisualRoot(visual);
            Assert.IsNotNull(animator.LegRig, "the wolf gets a stepping-legs driver");
            Assert.AreEqual(0, animator.LegCount, "stepping legs replace the leg twitch");

            // Emerging from the ground: the legs wait, then blend in.
            animator.Tick(Dt);
            Assert.AreEqual(0f, animator.LegWeight, "no stepping while emerging");
            for (int i = 0; i < 60; i++)
            {
                animator.Tick(Dt);
            }

            Assert.AreEqual(1f, animator.LegWeight, 1e-3f, "legs live after the emerge");

            // Trot forward for 2 s: feet lift and land, and a foot that stays down does not slide.
            Transform paw = Find(visual, ProceduralLegLocomotion.FootPrefix + "FL");
            LegGaitSolver solver = animator.LegRig.Solver;
            int swings = 0;
            bool wasDown = false;
            Vector3 down = Vector3.zero;
            for (int i = 0; i < 120; i++)
            {
                root.transform.position += Vector3.forward * (2f / 60f);
                animator.Tick(Dt);
                bool isDown = !solver.IsSwinging(0);
                swings += isDown ? 0 : 1;
                if (isDown && wasDown)
                {
                    Assert.Less(Vector3.Distance(down, paw.position), 0.02f, $"planted paw slid at frame {i}");
                }

                wasDown = isDown;
                down = paw.position;
            }

            Assert.Greater(swings, 10, "the front-left paw stepped");
            Assert.Less(Mathf.Abs(paw.position.y - (root.transform.position.y - 1f)), 0.25f, "paw near the ground");

            // Death: the legs blend back to rest and stop being driven.
            enemy.Health.ApplyDamage(1_000_000f, ElementType.Physical, null);
            for (int i = 0; i < 40; i++)
            {
                animator.Tick(Dt);
            }

            Assert.AreEqual(0f, animator.LegWeight, "corpses don't step");

            // Pool respawn far away: after the emerge the feet are re-planted under the new position.
            Vector3 spawn = new Vector3(20f, 1f, -8f);
            enemy.ResetForSpawn(definition, spawn);
            root.transform.position = spawn;
            for (int i = 0; i < 60; i++)
            {
                animator.Tick(Dt);
            }

            Assert.Greater(animator.LegWeight, 0.5f);
            for (int leg = 0; leg < solver.LegCount; leg++)
            {
                Assert.Less(LegGaitMath.PlanarDistance(solver.FootPosition(leg), spawn), 1f, $"leg {leg} re-planted at the respawn");
            }
        }

        private EnemyDefinition Definition()
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            _created.Add(definition);
            SetPrivateField(definition, "_maxHealth", 100f);
            SetPrivateField(definition, "_fleeHealthThreshold", 0f);
            return definition;
        }

        private EnemyController NewEnemy(EnemyDefinition definition)
        {
            var enemyObject = new GameObject("Enemy");
            _created.Add(enemyObject);
            enemyObject.AddComponent<CapsuleCollider>().height = 2f;
            enemyObject.AddComponent<Health>();
            EnemyController controller = enemyObject.AddComponent<EnemyController>();
            controller.ResetForSpawn(definition, new Vector3(0f, 1f, 0f));
            enemyObject.transform.position = new Vector3(0f, 1f, 0f);
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
