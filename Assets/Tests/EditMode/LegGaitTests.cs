using NUnit.Framework;
using Rapadura.Gameplay.Locomotion;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the procedural leg locomotion math (<see cref="LegGaitMath"/>) and the per-leg
    /// state machine (<see cref="LegGaitSolver"/>), simulated without transforms: gait phase offsets and
    /// duty factors, the speed-driven gait blends, cadence/stride, the step trigger, landing prediction,
    /// the swing arc, the two-bone IK solve, and the no-foot-sliding invariant while walking, galloping,
    /// skittering, stomping, stopping and turning in place.
    /// </summary>
    public class LegGaitTests
    {
        private const float Dt = 1f / 60f;

        // ------------------------------------------------------------------ phase offsets / duty

        [Test]
        public void Trot_DiagonalPairsMoveTogether_HalfACycleFromTheOtherPair()
        {
            float fl = LegGaitMath.QuadrupedOffset(0, 1f, 0f);
            float fr = LegGaitMath.QuadrupedOffset(1, 1f, 0f);
            float hl = LegGaitMath.QuadrupedOffset(2, 1f, 0f);
            float hr = LegGaitMath.QuadrupedOffset(3, 1f, 0f);

            Assert.AreEqual(0f, LegGaitMath.PhaseDelta(fl, hr), 1e-5f, "FL + HR");
            Assert.AreEqual(0f, LegGaitMath.PhaseDelta(fr, hl), 1e-5f, "FR + HL");
            Assert.AreEqual(0.5f, Mathf.Abs(LegGaitMath.PhaseDelta(fl, fr)), 1e-5f);
        }

        [Test]
        public void Walk_IsALateralSequence_AQuarterCycleApart()
        {
            float fl = LegGaitMath.QuadrupedOffset(0, 0f, 0f);
            float fr = LegGaitMath.QuadrupedOffset(1, 0f, 0f);
            float hl = LegGaitMath.QuadrupedOffset(2, 0f, 0f);
            float hr = LegGaitMath.QuadrupedOffset(3, 0f, 0f);

            // HL → FL → HR → FR.
            Assert.AreEqual(0.25f, LegGaitMath.Frac(fl - hl), 1e-5f);
            Assert.AreEqual(0.25f, LegGaitMath.Frac(hr - fl), 1e-5f);
            Assert.AreEqual(0.25f, LegGaitMath.Frac(fr - hr), 1e-5f);
        }

        [Test]
        public void Gallop_PairsFrontAndHindFeet_HalfACycleApart()
        {
            float fl = LegGaitMath.QuadrupedOffset(0, 1f, 1f);
            float fr = LegGaitMath.QuadrupedOffset(1, 1f, 1f);
            float hl = LegGaitMath.QuadrupedOffset(2, 1f, 1f);
            float hr = LegGaitMath.QuadrupedOffset(3, 1f, 1f);

            Assert.Less(Mathf.Abs(LegGaitMath.PhaseDelta(fl, fr)), 0.15f, "fore pair close together");
            Assert.Less(Mathf.Abs(LegGaitMath.PhaseDelta(hl, hr)), 0.15f, "hind pair close together");
            Assert.Greater(Mathf.Abs(LegGaitMath.PhaseDelta(fl, hl)), 0.4f, "fore and hind half a cycle apart");
        }

        [Test]
        public void GaitBlend_MovesOffsetsContinuously()
        {
            for (int leg = 0; leg < 4; leg++)
            {
                float previous = LegGaitMath.QuadrupedOffset(leg, 0f, 0f);
                for (int i = 1; i <= 20; i++)
                {
                    float t = i / 20f;
                    float next = LegGaitMath.QuadrupedOffset(leg, Mathf.Clamp01(t * 2f), Mathf.Clamp01(t * 2f - 1f));
                    Assert.Less(Mathf.Abs(LegGaitMath.PhaseDelta(previous, next)), 0.1f, $"leg {leg} jumps at {t}");
                    previous = next;
                }
            }
        }

        [Test]
        public void Arthropod_EveryNeighbourStepsHalfACycleAway_AlternatingTetrapod()
        {
            const int legs = 8;
            for (int pair = 0; pair < 4; pair++)
            {
                float left = LegGaitMath.PhaseOffset(LegGaitStyle.Arthropod, pair * 2, legs);
                float right = LegGaitMath.PhaseOffset(LegGaitStyle.Arthropod, pair * 2 + 1, legs);
                Assert.AreEqual(0.5f, Mathf.Abs(LegGaitMath.PhaseDelta(left, right)), 1e-4f, $"across, pair {pair}");

                if (pair < 3)
                {
                    float behind = LegGaitMath.PhaseOffset(LegGaitStyle.Arthropod, pair * 2 + 2, legs);
                    Assert.AreEqual(0.5f, Mathf.Abs(LegGaitMath.PhaseDelta(left, behind)), LegGaitMath.ArthropodRipple + 1e-4f, $"along, pair {pair}");
                }
            }

            // Group A (L0, R1, L2, R3) vs group B: each group is a tetrapod of four feet.
            float a = LegGaitMath.PhaseOffset(LegGaitStyle.Arthropod, 0, legs);
            foreach (int leg in new[] { 3, 4, 7 })
            {
                Assert.Less(Mathf.Abs(LegGaitMath.PhaseDelta(a, LegGaitMath.PhaseOffset(LegGaitStyle.Arthropod, leg, legs))), 0.2f, $"leg {leg} with L0");
            }
        }

        [Test]
        public void Biped_LegsAlternate_AndDutyShortensFromWalkToRun()
        {
            Assert.AreEqual(0f, LegGaitMath.PhaseOffset(LegGaitStyle.Biped, 0, 2));
            Assert.AreEqual(0.5f, LegGaitMath.PhaseOffset(LegGaitStyle.Biped, 1, 2));
            Assert.Greater(LegGaitMath.DutyFactor(LegGaitStyle.Biped, 2, 0f), 0.5f, "walking: double support");
            Assert.Less(LegGaitMath.DutyFactor(LegGaitStyle.Biped, 2, 1f), 0.5f, "running: flight phase");
        }

        [Test]
        public void Duty_ShrinksFromWalkToTrotToGallop()
        {
            float walk = LegGaitMath.DutyFactor(LegGaitStyle.Quadruped, 4, 0f, 0f);
            float trot = LegGaitMath.DutyFactor(LegGaitStyle.Quadruped, 4, 1f, 0f);
            float gallop = LegGaitMath.DutyFactor(LegGaitStyle.Quadruped, 4, 1f, 1f);
            Assert.Greater(walk, trot);
            Assert.Greater(trot, gallop);
            Assert.Greater(walk, 0.5f, "a walk always has three feet down on average");
        }

        [Test]
        public void QuadrupedBlend_WalkAtRest_TrotAtTrotSpeed_GallopAtFullSpeed()
        {
            LegGaitMath.QuadrupedBlend(0f, 1.4f, 2.8f, 3.9f, out float trot, out float gallop);
            Assert.AreEqual(0f, trot);
            Assert.AreEqual(0f, gallop);

            LegGaitMath.QuadrupedBlend(1.4f, 1.4f, 2.8f, 3.9f, out trot, out gallop);
            Assert.AreEqual(1f, trot, 1e-5f);
            Assert.AreEqual(0f, gallop);

            LegGaitMath.QuadrupedBlend(4.5f, 1.4f, 2.8f, 3.9f, out trot, out gallop);
            Assert.AreEqual(1f, gallop, 1e-5f);
        }

        // ------------------------------------------------------------------ cadence / stride / trigger

        [Test]
        public void CadenceAndStride_GrowWithSpeed_AreClamped_AndStopAtRest()
        {
            Assert.AreEqual(0f, LegGaitMath.Cadence(0f, 1f, 1f, 3f));
            float slow = LegGaitMath.Cadence(1.5f, LegGaitMath.StrideLength(1f, 1.5f, 4.5f, 0.4f), 1f, 3f);
            float fast = LegGaitMath.Cadence(4f, LegGaitMath.StrideLength(1f, 4f, 4.5f, 0.4f), 1f, 3f);
            Assert.Greater(fast, slow);
            Assert.AreEqual(3f, LegGaitMath.Cadence(50f, 1f, 1f, 3f), 1e-5f);
            Assert.AreEqual(1f, LegGaitMath.Cadence(0.1f, 1f, 1f, 3f), 1e-5f);
            Assert.Greater(LegGaitMath.StrideLength(1f, 4.5f, 4.5f, 0.4f), LegGaitMath.StrideLength(1f, 1f, 4.5f, 0.4f));
        }

        [Test]
        public void ShouldStep_NeedsTheWindowAndADrift_OrBeingBeyondReach()
        {
            Assert.IsFalse(LegGaitMath.ShouldStep(0.01f, 0.05f, true, 0.3f), "settled foot stays");
            Assert.IsFalse(LegGaitMath.ShouldStep(0.1f, 0.05f, false, 0.3f), "waits for its phase");
            Assert.IsTrue(LegGaitMath.ShouldStep(0.1f, 0.05f, true, 0.3f), "drifted + window");
            Assert.IsTrue(LegGaitMath.ShouldStep(0.35f, 0.05f, false, 0.3f), "beyond reach: steps now");
        }

        [Test]
        public void PredictLanding_LeadsBySpeed_IsClamped_AndTurnsWithTheBody()
        {
            Vector3 body = Vector3.zero;
            Vector3 rest = new Vector3(0.2f, 0f, 0.3f);
            AssertNear(rest, LegGaitMath.PredictLanding(body, rest, Vector3.zero, 0f, 0.2f, 0.3f, 1f), "standing: the rest target");

            Vector3 moving = LegGaitMath.PredictLanding(body, rest, new Vector3(0f, 0f, 2f), 0f, 0.2f, 0.3f, 1f);
            Assert.AreEqual(rest.z + 2f * (0.2f + 0.15f), moving.z, 1e-4f, "carried to landing + half a stance");

            Vector3 clamped = LegGaitMath.PredictLanding(body, rest, new Vector3(0f, 0f, 10f), 0f, 0f, 0.3f, 0.1f);
            Assert.AreEqual(rest.z + 0.1f, clamped.z, 1e-4f, "lead clamped");

            Vector3 turned = LegGaitMath.PredictLanding(body, new Vector3(0f, 0f, 1f), Vector3.zero, 90f, 0.5f, 0f, 1f);
            Assert.Greater(turned.x, 0.6f, "turning right swings the target to the right");
            Assert.AreEqual(1f, turned.magnitude, 1e-4f);
        }

        [Test]
        public void Swing_StartsAndEndsOnTheFeet_PeaksInTheMiddle()
        {
            Vector3 from = new Vector3(0f, 0f, 0f), to = new Vector3(0f, 0f, 0.5f);
            AssertNear(from, LegGaitMath.SwingPosition(from, to, 0f, 0.1f), "lift-off");
            AssertNear(to, LegGaitMath.SwingPosition(from, to, 1f, 0.1f), "touch-down");
            Assert.Greater(LegGaitMath.SwingPosition(from, to, 0.45f, 0.1f).y, 0.09f, "apex");
            Assert.AreEqual(0f, LegGaitMath.CircularLerp(0.9f, 0.1f, 0.5f), 1e-5f, "phases blend the short way round");
        }

        // ------------------------------------------------------------------ IK

        [Test]
        public void TwoBoneIK_ReachesTheTarget_KeepsBoneLengths_AndBendsTowardThePole()
        {
            Vector3 hip = new Vector3(0f, 1f, 0f);
            Vector3 target = new Vector3(0.2f, 0.25f, 0.1f);
            bool reached = LegGaitMath.SolveTwoBone(hip, target, 0.5f, 0.45f, Vector3.forward, out Vector3 knee, out Vector3 foot);

            Assert.IsTrue(reached);
            AssertNear(target, foot, "foot on target");
            Assert.AreEqual(0.5f, Vector3.Distance(hip, knee), 1e-4f);
            Assert.AreEqual(0.45f, Vector3.Distance(knee, foot), 1e-4f);
            Vector3 mid = (hip + foot) * 0.5f;
            Assert.Greater(Vector3.Dot(knee - mid, Vector3.forward), 0.05f, "knee bends toward the pole");

            reached = LegGaitMath.SolveTwoBone(hip, target, 0.5f, 0.45f, Vector3.back, out knee, out _);
            Assert.Less(Vector3.Dot(knee - mid, Vector3.forward), -0.05f, "flipping the pole flips the knee");
        }

        [Test]
        public void TwoBoneIK_OutOfReach_StretchesStraightTowardTheTarget_TooCloseFoldsToMinReach()
        {
            Vector3 hip = Vector3.zero;
            bool reached = LegGaitMath.SolveTwoBone(hip, new Vector3(0f, -3f, 0f), 0.5f, 0.4f, Vector3.forward, out Vector3 knee, out Vector3 foot);
            Assert.IsFalse(reached);
            Assert.AreEqual(0.9f, foot.magnitude, 1e-3f, "full reach");
            Assert.Less(Vector3.Angle(Vector3.down, foot), 0.1f, "aimed at the target");
            Assert.AreEqual(0.5f, knee.magnitude, 1e-4f);

            reached = LegGaitMath.SolveTwoBone(hip, new Vector3(0f, -0.01f, 0f), 0.5f, 0.4f, Vector3.forward, out knee, out foot);
            Assert.IsFalse(reached);
            Assert.AreEqual(0.1f, foot.magnitude, 1e-3f, "folded to |upper - lower|");
            Assert.AreEqual(0.4f, Vector3.Distance(knee, foot), 1e-3f);
        }

        // ------------------------------------------------------------------ solver simulation

        private static LegGaitSolver Quadruped(float legLength = 0.45f)
        {
            var solver = new LegGaitSolver { Settings = LegGaitSettings.Default(LegGaitStyle.Quadruped) };
            // FL, FR, HL, HR (gait order); the ground is the body's y = 0, feet points 5 cm above it.
            solver.AddLeg(new Vector3(-0.12f, 0.05f, 0.3f), 0f, legLength, -1, 1);
            solver.AddLeg(new Vector3(0.12f, 0.05f, 0.3f), 0f, legLength, 1, 1);
            solver.AddLeg(new Vector3(-0.12f, 0.05f, -0.4f), 0f, legLength, -1, -1);
            solver.AddLeg(new Vector3(0.12f, 0.05f, -0.4f), 0f, legLength, 1, -1);
            return solver;
        }

        private static LegGaitSolver Arthropod()
        {
            var solver = new LegGaitSolver { Settings = LegGaitSettings.Default(LegGaitStyle.Arthropod) };
            float[] z = { 0.4f, 0.15f, -0.1f, -0.35f };
            for (int pair = 0; pair < 4; pair++)
            {
                solver.AddLeg(new Vector3(-0.5f, 0.01f, z[pair]), 0f, 0.55f, -1, pair < 2 ? 1 : -1);
                solver.AddLeg(new Vector3(0.5f, 0.01f, z[pair]), 0f, 0.55f, 1, pair < 2 ? 1 : -1);
            }

            return solver;
        }

        private static LegGaitSolver Biped()
        {
            var solver = new LegGaitSolver { Settings = LegGaitSettings.Default(LegGaitStyle.Biped) };
            solver.AddLeg(new Vector3(-0.3f, 0.1f, 0f), 0f, 0.9f, -1, 0);
            solver.AddLeg(new Vector3(0.3f, 0.1f, 0f), 0f, 0.9f, 1, 0);
            return solver;
        }

        /// <summary>
        /// Walks <paramref name="solver"/> straight ahead at <paramref name="speed"/> for <paramref name="seconds"/>,
        /// asserting every frame that a foot planted on the previous frame and still planted has not moved
        /// (the no-sliding invariant) and that the feet keep up with the body. Returns the landings.
        /// </summary>
        private static int Walk(LegGaitSolver solver, float speed, float seconds, ref Vector3 body, Quaternion rotation, float yawRate = 0f)
        {
            int count = solver.LegCount;
            var wasPlanted = new bool[count];
            var planted = new Vector3[count];
            int landings = 0;
            Vector3 velocity = rotation * Vector3.forward * speed;
            for (int frame = 0; frame < Mathf.RoundToInt(seconds / Dt); frame++)
            {
                body += velocity * Dt;
                rotation = Quaternion.AngleAxis(yawRate * Dt, Vector3.up) * rotation;
                velocity = rotation * Vector3.forward * speed;
                solver.Step(Dt, body, rotation, velocity, yawRate);
                landings += solver.Landings;

                for (int i = 0; i < count; i++)
                {
                    bool isPlanted = !solver.IsSwinging(i);
                    Vector3 position = solver.FootPosition(i);
                    if (isPlanted && wasPlanted[i])
                    {
                        Assert.AreEqual(planted[i], position, $"leg {i} slid at frame {frame} ({speed} m/s)");
                    }

                    wasPlanted[i] = isPlanted;
                    planted[i] = position;

                    float drift = LegGaitMath.PlanarDistance(position, solver.RestTarget(i, body, rotation));
                    Assert.Less(drift, solver.LegLength(i) * 1.5f, $"leg {i} left behind at frame {frame} ({speed} m/s)");
                }
            }

            return landings;
        }

        [Test]
        public void Quadruped_NoFootEverSlides_AndTheFeetKeepUp_FromWalkToGallop()
        {
            foreach (float speed in new[] { 0.6f, 2f, 3.2f, 4.5f, 5.5f })
            {
                LegGaitSolver solver = Quadruped();
                Vector3 body = Vector3.zero;
                solver.Reset(body, Quaternion.identity);
                int landings = Walk(solver, speed, 4f, ref body, Quaternion.identity);
                Assert.Greater(landings, 8, $"{speed} m/s: feet must keep stepping");
            }
        }

        [Test]
        public void Quadruped_Trot_LiftsTheDiagonalsTogether_Gallop_TheFrontPairTogether()
        {
            Assert.Greater(Agreement(Quadruped(), 2f, 0, 3), Agreement(Quadruped(), 2f, 0, 1), "trot: FL moves with HR, not with FR");
            Assert.Greater(Agreement(Quadruped(), 5f, 0, 1), Agreement(Quadruped(), 5f, 0, 2), "gallop: FL moves with FR, not with HL");
        }

        /// <summary>Fraction of frames (after a warm-up) in which legs a and b are in the same swing/stance state.</summary>
        private static float Agreement(LegGaitSolver solver, float speed, int a, int b)
        {
            Vector3 body = Vector3.zero;
            solver.Reset(body, Quaternion.identity);
            Walk(solver, speed, 1f, ref body, Quaternion.identity);
            int same = 0, frames = 180;
            for (int frame = 0; frame < frames; frame++)
            {
                body += Vector3.forward * speed * Dt;
                solver.Step(Dt, body, Quaternion.identity, Vector3.forward * speed, 0f);
                same += solver.IsSwinging(a) == solver.IsSwinging(b) ? 1 : 0;
            }

            return same / (float)frames;
        }

        [Test]
        public void Arthropod_NeverSlides_NeighboursRarelyLiftTogether_AndNeverMostLegsAtOnce()
        {
            LegGaitSolver solver = Arthropod();
            Vector3 body = Vector3.zero;
            solver.Reset(body, Quaternion.identity);
            int landings = Walk(solver, 3f, 3f, ref body, Quaternion.identity);
            Assert.Greater(landings, 30);

            int together = 0, crowded = 0, frames = 240;
            for (int frame = 0; frame < frames; frame++)
            {
                body += Vector3.forward * 3f * Dt;
                solver.Step(Dt, body, Quaternion.identity, Vector3.forward * 3f, 0f);
                int up = 0;
                for (int i = 0; i < 8; i++)
                {
                    up += solver.IsSwinging(i) ? 1 : 0;
                }

                crowded += up > 5 ? 1 : 0;
                together += solver.IsSwinging(2) && solver.IsSwinging(3) ? 1 : 0;
            }

            Assert.Less(together / (float)frames, 0.15f, "L1 and R1 alternate");
            Assert.AreEqual(0, crowded, "a tetrapod gait keeps at least three feet down");
        }

        [Test]
        public void Biped_Stomps_TwoFootfallsPerCycle_AndTheBodyBobs()
        {
            LegGaitSolver solver = Biped();
            Vector3 body = Vector3.zero;
            solver.Reset(body, Quaternion.identity);
            Walk(solver, 1.5f, 1f, ref body, Quaternion.identity);

            float minBob = float.MaxValue, maxBob = float.MinValue, cycles = 0f;
            int landings = 0;
            for (int frame = 0; frame < 240; frame++)
            {
                body += Vector3.forward * 1.5f * Dt;
                solver.Step(Dt, body, Quaternion.identity, Vector3.forward * 1.5f, 0f);
                landings += solver.Landings;
                cycles += solver.Cadence * Dt;
                minBob = Mathf.Min(minBob, solver.Bob);
                maxBob = Mathf.Max(maxBob, solver.Bob);
            }

            Assert.AreEqual(cycles * 2f, landings, 1.5f, "one footfall per leg per cycle");
            Assert.Greater(maxBob - minBob, solver.Settings.BobAmplitude * 0.5f, "the body rises and falls with the steps");
        }

        [Test]
        public void StandingStill_NeverSteps_AndStopping_SettlesEveryFootUnderTheBody()
        {
            LegGaitSolver solver = Quadruped();
            Vector3 body = Vector3.zero;
            solver.Reset(body, Quaternion.identity);
            Assert.AreEqual(0, Walk(solver, 0f, 2f, ref body, Quaternion.identity), "idle: no steps");
            Assert.AreEqual(0f, solver.Cadence);

            Walk(solver, 2.5f, 2f, ref body, Quaternion.identity);
            Walk(solver, 0f, 2f, ref body, Quaternion.identity);
            Assert.AreEqual(0f, solver.Cadence, "clock stops once settled");
            for (int i = 0; i < 4; i++)
            {
                Assert.IsFalse(solver.IsSwinging(i));
                Assert.Less(LegGaitMath.PlanarDistance(solver.PlantedPosition(i), solver.RestTarget(i, body, Quaternion.identity)),
                    solver.LegLength(i) * solver.Settings.MinStepFraction + 1e-3f, $"leg {i} settled under the body");
            }
        }

        [Test]
        public void TurningInPlace_StepsRound_WithoutSliding()
        {
            foreach (LegGaitSolver solver in new[] { Quadruped(), Arthropod(), Biped() })
            {
                Vector3 body = Vector3.zero;
                solver.Reset(body, Quaternion.identity);
                int landings = Walk(solver, 0f, 2.5f, ref body, Quaternion.identity, 72f); // 180° on the spot
                Assert.GreaterOrEqual(landings, solver.LegCount, $"{solver.Settings.Style}: feet step round while turning");

                Quaternion facing = Quaternion.Euler(0f, 180f, 0f);
                Walk(solver, 0f, 2f, ref body, facing);
                for (int i = 0; i < solver.LegCount; i++)
                {
                    Assert.Less(LegGaitMath.PlanarDistance(solver.PlantedPosition(i), solver.RestTarget(i, body, facing)), solver.Reach(i),
                        $"{solver.Settings.Style} leg {i} ends near its turned rest target");
                }
            }
        }

        [Test]
        public void Teleport_AndReset_ReplantEveryFootOnItsRestTarget()
        {
            LegGaitSolver solver = Quadruped();
            Vector3 body = Vector3.zero;
            solver.Reset(body, Quaternion.identity);
            Walk(solver, 3f, 1f, ref body, Quaternion.identity);

            // Pool reuse: the enemy reappears 30 m away.
            Vector3 far = new Vector3(30f, 0f, -12f);
            solver.Step(Dt, far, Quaternion.identity, Vector3.zero, 0f);
            for (int i = 0; i < 4; i++)
            {
                AssertNear(solver.RestTarget(i, far, Quaternion.identity), solver.FootPosition(i), $"leg {i} re-planted");
            }

            Quaternion turned = Quaternion.Euler(0f, 90f, 0f);
            solver.Reset(Vector3.one, turned);
            for (int i = 0; i < 4; i++)
            {
                Assert.IsFalse(solver.IsSwinging(i));
                AssertNear(solver.RestTarget(i, Vector3.one, turned), solver.PlantedPosition(i), $"leg {i} reset");
            }
        }

        [Test]
        public void GroundProbe_PlantsTheFeetOnTheHit_AtTheirSoleHeight()
        {
            LegGaitSolver solver = Quadruped();
            solver.Probe = (Vector3 point, float up, float down, out Vector3 ground, out Vector3 normal) =>
            {
                ground = new Vector3(point.x, 0.2f, point.z); // a 20 cm step under everything
                normal = Vector3.up;
                return true;
            };

            solver.Reset(Vector3.zero, Quaternion.identity);
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual(0.25f, solver.PlantedPosition(i).y, 1e-4f, "ground 0.2 + sole 0.05");
            }
        }

        private static void AssertNear(Vector3 expected, Vector3 actual, string message)
        {
            Assert.Less(Vector3.Distance(expected, actual), 1e-3f, $"{message}: expected {expected}, got {actual}");
        }
    }
}
