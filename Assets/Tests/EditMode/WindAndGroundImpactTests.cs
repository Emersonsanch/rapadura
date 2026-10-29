using NUnit.Framework;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Environment.Impact;
using Rapadura.Gameplay.Environment.Wind;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the wind field (strength per weather, gust bounds, determinism, lean
    /// direction), the sway component (rest pose respected, leans toward the wind about its base),
    /// the ground-impact math (falloff, settle back to rest), the prop naming rules and the
    /// impact system kicking only rocks in range.
    /// </summary>
    public class WindAndGroundImpactTests
    {
        // ------------------------------------------------------------ wind math

        [Test]
        public void StrengthFor_SandstormIsStrongest_ClearIsABreeze_FogIsCalmest()
        {
            Assert.AreEqual(1f, WindMath.StrengthFor(WeatherType.Sandstorm));
            Assert.Greater(WindMath.StrengthFor(WeatherType.Rain), WindMath.StrengthFor(WeatherType.Clear));
            Assert.Greater(WindMath.StrengthFor(WeatherType.Clear), 0f, "clear skies keep a light breeze so foliage never freezes");
            Assert.Less(WindMath.StrengthFor(WeatherType.Fog), WindMath.StrengthFor(WeatherType.Clear));
        }

        [Test]
        public void BlendedStrength_FollowsTransitionProgress()
        {
            float start = WindMath.BlendedStrength(WeatherType.Clear, WeatherType.Sandstorm, 0f);
            float mid = WindMath.BlendedStrength(WeatherType.Clear, WeatherType.Sandstorm, 0.5f);
            float end = WindMath.BlendedStrength(WeatherType.Clear, WeatherType.Sandstorm, 1f);
            Assert.AreEqual(WindMath.StrengthFor(WeatherType.Clear), start, 1e-5f);
            Assert.AreEqual(WindMath.StrengthFor(WeatherType.Sandstorm), end, 1e-5f);
            Assert.Greater(mid, start);
            Assert.Less(mid, end);
        }

        [Test]
        public void Sample_IsDeterministic_BoundedAndZeroWhenCalm()
        {
            Vector2 dir = new Vector2(0.6f, 0.8f);
            Assert.AreEqual(Vector2.zero, WindMath.Sample(new Vector2(3f, 4f), 1f, dir, 0f));
            Assert.AreEqual(Vector2.zero, WindMath.Sample(new Vector2(3f, 4f), 1f, Vector2.zero, 1f));

            Vector2 a = WindMath.Sample(new Vector2(3f, 4f), 2.5f, dir, 1f);
            Vector2 b = WindMath.Sample(new Vector2(3f, 4f), 2.5f, dir, 1f);
            Assert.AreEqual(a, b);

            for (int i = 0; i < 200; i++)
            {
                Vector2 w = WindMath.Sample(new Vector2(i * 0.7f - 30f, i * 1.3f), i * 0.11f, dir, 1f);
                Assert.LessOrEqual(w.magnitude, WindMath.MaxStrength + 1e-4f);
                Assert.Greater(Vector2.Dot(w, dir), 0f, "wind never blows against the prevailing direction");
            }
        }

        [Test]
        public void Sample_GustsVaryAcrossTheField()
        {
            Vector2 dir = Vector2.up;
            float min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < 64; i++)
            {
                float m = WindMath.Sample(new Vector2(0f, i * 1.5f), 0f, dir, 1f).magnitude;
                min = Mathf.Min(min, m);
                max = Mathf.Max(max, m);
            }

            Assert.Greater(max - min, 0.1f, "a gust band should make some trees feel more wind than others");
        }

        [Test]
        public void TiltAxis_LeansUpTowardTheWind()
        {
            Vector2 wind = new Vector2(1f, 0f); // +X
            Vector3 axis = WindMath.TiltAxis(wind);
            Vector3 leaned = Quaternion.AngleAxis(10f, axis) * Vector3.up;
            Assert.Greater(leaned.x, 0.1f, "the top should move downwind");
            Assert.AreEqual(Vector3.zero, WindMath.TiltAxis(Vector2.zero));
        }

        [Test]
        public void TiltAngle_IsCappedAndZeroWithoutWind()
        {
            WindSwayProfile grass = WindSwayProfile.Grass;
            Assert.AreEqual(0f, WindMath.TiltAngle(Vector2.zero, in grass, 3f, 0.2f), 1e-5f);
            float storm = WindMath.TiltAngle(new Vector2(0f, 10f), in grass, 0f, 0f);
            Assert.LessOrEqual(storm, grass.MaxTilt * 1.5f + grass.FlutterAmplitude + 1e-4f);
        }

        [Test]
        public void Smooth_ConvergesWithoutOvershoot()
        {
            Vector2 felt = Vector2.zero;
            Vector2 target = new Vector2(0.5f, 0.5f);
            for (int i = 0; i < 300; i++)
            {
                felt = WindMath.Smooth(felt, target, 0.25f, 1f / 60f);
                Assert.LessOrEqual(felt.magnitude, target.magnitude + 1e-5f);
            }

            Assert.AreEqual(target.x, felt.x, 1e-3f);
            Assert.AreEqual(target, WindMath.Smooth(felt, target, 0f, 0.016f), "no response time = instant");
        }

        [Test]
        public void PhaseFor_IsStableAndSpread()
        {
            Assert.AreEqual(WindMath.PhaseFor(new Vector3(1.5f, 0f, -2f)), WindMath.PhaseFor(new Vector3(1.5f, 0f, -2f)));
            float a = WindMath.PhaseFor(new Vector3(1f, 0f, 1f));
            float b = WindMath.PhaseFor(new Vector3(2f, 0f, 1f));
            Assert.AreNotEqual(a, b);
            Assert.That(a, Is.InRange(0f, 1f));
        }

        // ------------------------------------------------------------ sway component

        [Test]
        public void WindSway_NoWindKeepsRestPose_WindLeansAboutTheBase()
        {
            var go = new GameObject("Tree_Test");
            try
            {
                go.transform.rotation = Quaternion.Euler(0f, 37f, 0f);
                Quaternion rest = go.transform.localRotation;
                var sway = go.AddComponent<WindSway>();
                sway.Configure(WindSwayKind.Tree);
                sway.CaptureRest();

                sway.Apply(new WindFieldState(Vector2.up, 0f), 1f, 0.016f);
                Assert.AreEqual(rest, go.transform.localRotation);

                var field = new WindFieldState(new Vector2(1f, 0f), 1f);
                for (int i = 0; i < 240; i++)
                {
                    sway.Apply(in field, i * 0.016f, 0.016f);
                }

                Vector3 top = go.transform.up;
                Assert.Greater(top.x, 0.005f, "canopy should lean downwind (+X)");
                Assert.Greater(Quaternion.Angle(rest, go.transform.localRotation), 0.5f);
                Assert.Less(Quaternion.Angle(rest, go.transform.localRotation), WindSwayProfile.Tree.MaxTilt * 1.5f + WindSwayProfile.Tree.FlutterAmplitude + 0.01f);
                Assert.AreEqual(Vector3.zero, go.transform.localPosition, "sway rotates about the pivot, never moves the base");
            }
            finally
            {
                Object.DestroyImmediate(go);
            }
        }

        // ------------------------------------------------------------ ground impact math

        [Test]
        public void Falloff_FullAtCentre_ZeroAtEdge()
        {
            Assert.AreEqual(1f, GroundImpactMath.Falloff(0f, 5f), 1e-5f);
            Assert.AreEqual(0f, GroundImpactMath.Falloff(5f, 5f), 1e-5f);
            Assert.AreEqual(0f, GroundImpactMath.Falloff(9f, 5f), 1e-5f);
            Assert.AreEqual(0.25f, GroundImpactMath.Falloff(2.5f, 5f), 1e-5f);
            Assert.AreEqual(0f, GroundImpactMath.Falloff(1f, 0f));
        }

        [Test]
        public void Evaluate_SettlesBackToRest_AndPebblesHop()
        {
            Vector2 away = new Vector2(0f, 1f);
            GroundImpactPose mid = GroundImpactMath.Evaluate(GroundImpactKind.Pebble, 1f, GroundImpactMath.PebbleDuration * 0.5f, away, 0.3f);
            Assert.IsFalse(mid.Finished);
            Assert.Greater(mid.PositionOffset.y, 0.05f, "pebble leaves the ground mid-hop");
            Assert.Greater(mid.PositionOffset.z, 0f, "pebble skids away from the impact");

            GroundImpactPose done = GroundImpactMath.Evaluate(GroundImpactKind.Pebble, 1f, 10f, away, 0.3f);
            Assert.IsTrue(done.Finished);
            Assert.AreEqual(Vector3.zero, done.PositionOffset);

            GroundImpactPose boulder = GroundImpactMath.Evaluate(GroundImpactKind.Boulder, 1f, 0.05f, away, 0f);
            Assert.IsFalse(boulder.Finished);
            Assert.Less(boulder.PositionOffset.magnitude, 0.02f, "boulders stay planted, they only rock");
            Assert.IsTrue(GroundImpactMath.Evaluate(GroundImpactKind.Boulder, 0f, 0.1f, away, 0f).Finished);
        }

        // ------------------------------------------------------------ rules

        [Test]
        public void Rules_ClassifyAuthoredPropNames()
        {
            Assert.AreEqual(WindSwayKind.Tree, EnvironmentPropRules.ClassifyWind("Tree_Pine_RO_3", null, true));
            Assert.AreEqual(WindSwayKind.None, EnvironmentPropRules.ClassifyWind("Tree_Pine_RO_3", null, false), "only the LOD root sways, not the LOD children");
            Assert.AreEqual(WindSwayKind.Grass, EnvironmentPropRules.ClassifyWind("Tuft_12", EnvironmentPropRules.GrassTuftMeshName, false));
            Assert.AreEqual(WindSwayKind.Shrub, EnvironmentPropRules.ClassifyWind("ArenaProp_Cactus_4", null, false));
            Assert.AreEqual(WindSwayKind.None, EnvironmentPropRules.ClassifyWind("Enemy_Wolf", "Wolf", false));
            Assert.AreEqual(WindSwayKind.None, EnvironmentPropRules.ClassifyWind(null, null, false));

            Assert.AreEqual(GroundImpactKind.Boulder, EnvironmentPropRules.ClassifyImpact("ProceduralRock_7"));
            Assert.AreEqual(GroundImpactKind.Boulder, EnvironmentPropRules.ClassifyImpact("Rock_Scattered_2"));
            Assert.AreEqual(GroundImpactKind.Boulder, EnvironmentPropRules.ClassifyImpact("Rock"));
            Assert.AreEqual(GroundImpactKind.Boulder, EnvironmentPropRules.ClassifyImpact("ArenaProp_SnowyRock_1"));
            Assert.AreEqual(GroundImpactKind.Pebble, EnvironmentPropRules.ClassifyImpact("Pebble_3"));
            Assert.AreEqual(GroundImpactKind.Pebble, EnvironmentPropRules.ClassifyImpact("Twig_9"));
            Assert.AreEqual(GroundImpactKind.None, EnvironmentPropRules.ClassifyImpact("Rocks"), "the group parent must not shake");
            Assert.AreEqual(GroundImpactKind.None, EnvironmentPropRules.ClassifyImpact("Player"));
        }

        // ------------------------------------------------------------ impact system

        [Test]
        public void Strike_KicksOnlyRocksInRange_AndTheySettle()
        {
            var systemGo = new GameObject("[Environment]");
            var near = new GameObject("Pebble_1");
            var far = new GameObject("ProceduralRock_2");
            try
            {
                near.transform.position = new Vector3(1f, 0f, 0f);
                far.transform.position = new Vector3(40f, 0f, 0f);
                var nearShaker = near.AddComponent<GroundImpactShaker>();
                nearShaker.Configure(GroundImpactKind.Pebble);
                var farShaker = far.AddComponent<GroundImpactShaker>();
                farShaker.Configure(GroundImpactKind.Boulder);
                // AddComponent outside Play Mode doesn't call OnEnable synchronously: register by hand.
                if (!GroundImpactShaker.Registered.Contains(nearShaker)) GroundImpactShaker.Registered.Add(nearShaker);
                if (!GroundImpactShaker.Registered.Contains(farShaker)) GroundImpactShaker.Registered.Add(farShaker);
                nearShaker.CaptureRest();
                farShaker.CaptureRest();

                var system = systemGo.AddComponent<GroundImpactSystem>();
                system.Strike(Vector3.zero, 5f, 1f);

                Assert.IsTrue(nearShaker.IsReacting);
                Assert.IsFalse(farShaker.IsReacting);
                Assert.AreEqual(1, system.ReactingCountForTests);

                system.StepForTests(0.1f);
                Assert.Greater(near.transform.position.y, 0f, "pebble is in the air");

                for (int i = 0; i < 60; i++)
                {
                    system.StepForTests(0.05f);
                }

                Assert.IsFalse(nearShaker.IsReacting);
                Assert.AreEqual(0, system.ReactingCountForTests);
                Assert.AreEqual(0f, near.transform.position.y, 1e-5f, "pebble lands exactly at rest");
                Assert.AreEqual(1f, near.transform.position.x, 1e-5f);
            }
            finally
            {
                GroundImpactShaker.Registered.Clear();
                Object.DestroyImmediate(near);
                Object.DestroyImmediate(far);
                Object.DestroyImmediate(systemGo);
            }
        }

        [Test]
        public void SkillStrike_OnlyEarthAreaSkills()
        {
            var earthArea = ScriptableObject.CreateInstance<Rapadura.Gameplay.Skills.SkillDefinition>();
            try
            {
                Assert.IsFalse(GroundImpactSystem.TryGetSkillStrike(null, out _));
                Assert.IsFalse(GroundImpactSystem.TryGetSkillStrike(earthArea, out _), "a default definition has no element/area");
            }
            finally
            {
                Object.DestroyImmediate(earthArea);
            }
        }
    }
}
