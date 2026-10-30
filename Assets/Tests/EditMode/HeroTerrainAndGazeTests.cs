using NUnit.Framework;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// Terrain speed (Docs/Animation.md §1: climbing slows, descending speeds up a little, across the slope
    /// and on flat ground nothing changes) and the head-look points of interest (what catches the eye, how
    /// far the head pitches, how attention varies).
    /// </summary>
    public class HeroTerrainAndGazeTests
    {
        private static Vector3 SlopeNormal(float degrees, Vector3 downhillXZ)
        {
            // Normal of a plane tilted by `degrees`, whose downhill direction is downhillXZ.
            downhillXZ.y = 0f;
            downhillXZ.Normalize();
            return (Vector3.up * Mathf.Cos(degrees * Mathf.Deg2Rad) + downhillXZ * Mathf.Sin(degrees * Mathf.Deg2Rad)).normalized;
        }

        // ------------------------------------------------------------------ slope speed

        [Test]
        public void SlopeSpeed_FlatGround_IsOne()
        {
            Assert.AreEqual(1f, LocomotionMath.SlopeSpeedFactor(Vector3.up, Vector3.forward), 1e-5f);
            Assert.AreEqual(1f, LocomotionMath.SlopeSpeedFactor(Vector3.up, Vector3.zero), 1e-5f, "no heading");
            Assert.AreEqual(1f, LocomotionMath.SlopeSpeedFactor(Vector3.forward, Vector3.forward), 1e-5f, "a wall is not ground");
        }

        [Test]
        public void SlopeSpeed_UphillSlows_DownhillSpeedsUp_AcrossUnchanged()
        {
            // Downhill is -Z: walking +Z climbs.
            Vector3 normal = SlopeNormal(30f, Vector3.back);

            float up = LocomotionMath.SlopeSpeedFactor(normal, Vector3.forward, 0.3f, 0.08f, 45f);
            float down = LocomotionMath.SlopeSpeedFactor(normal, Vector3.back, 0.3f, 0.08f, 45f);
            float across = LocomotionMath.SlopeSpeedFactor(normal, Vector3.right, 0.3f, 0.08f, 45f);

            Assert.Less(up, 1f);
            Assert.Greater(down, 1f);
            Assert.AreEqual(1f, across, 1e-4f);
            Assert.Greater(1f - up, down - 1f, "climbing costs more than descending gives back");
        }

        [Test]
        public void SlopeSpeed_SaturatesAtTheSlopeLimit()
        {
            Vector3 limit = SlopeNormal(45f, Vector3.back);
            Vector3 steeper = SlopeNormal(60f, Vector3.back);

            float atLimit = LocomotionMath.SlopeSpeedFactor(limit, Vector3.forward, 0.3f, 0.08f, 45f);
            float beyond = LocomotionMath.SlopeSpeedFactor(steeper, Vector3.forward, 0.3f, 0.08f, 45f);

            Assert.AreEqual(0.7f, atLimit, 1e-3f, "full penalty at the limit");
            Assert.AreEqual(0.7f, beyond, 1e-3f, "never below the full penalty");
            Assert.AreEqual(1.08f, LocomotionMath.SlopeSpeedFactor(limit, Vector3.back, 0.3f, 0.08f, 45f), 1e-3f);
        }

        [Test]
        public void SlopeSpeed_GentleSlope_IsProportional()
        {
            Vector3 gentle = SlopeNormal(10f, Vector3.back);
            float up = LocomotionMath.SlopeSpeedFactor(gentle, Vector3.forward, 0.3f, 0.08f, 45f);
            float expected = 1f - 0.3f * (Mathf.Sin(10f * Mathf.Deg2Rad) / Mathf.Sin(45f * Mathf.Deg2Rad));
            Assert.AreEqual(expected, up, 1e-3f);
        }

        // ------------------------------------------------------------------ gaze: interest

        [Test]
        public void Interest_PrefersCloseAndInFront_IgnoresBehindAndFar()
        {
            Vector3 eye = Vector3.up * 1.6f;
            Vector3 forward = Vector3.forward;

            float ahead = MotionFeelMath.InterestScore(eye, forward, new Vector3(0f, 0f, 3f), 9f);
            float aheadFar = MotionFeelMath.InterestScore(eye, forward, new Vector3(0f, 0f, 8f), 9f);
            float side = MotionFeelMath.InterestScore(eye, forward, new Vector3(3f, 0f, 0f), 9f);
            float behind = MotionFeelMath.InterestScore(eye, forward, new Vector3(0f, 0f, -3f), 9f);
            float outOfRange = MotionFeelMath.InterestScore(eye, forward, new Vector3(0f, 0f, 12f), 9f);

            Assert.Greater(ahead, aheadFar);
            Assert.Greater(ahead, side);
            Assert.Greater(side, 0f, "inside the 100° cone");
            Assert.AreEqual(0f, behind);
            Assert.AreEqual(0f, outOfRange);
            Assert.AreEqual(0f, MotionFeelMath.InterestScore(eye, forward, new Vector3(0f, 0f, 3f), 9f, 0f), "zero importance");
            Assert.Greater(MotionFeelMath.InterestScore(eye, forward, new Vector3(0f, 0f, 3f), 9f, 1.5f), ahead, "importance scales");
        }

        [Test]
        public void LookPitch_DownToSmallThings_UpToTallOnes_Clamped()
        {
            Vector3 eye = Vector3.up * 1.6f;

            float down = MotionFeelMath.LookPitch(eye, new Vector3(0f, 0.3f, 2f), 25f);
            float level = MotionFeelMath.LookPitch(eye, new Vector3(0f, 1.6f, 2f), 25f);
            float up = MotionFeelMath.LookPitch(eye, new Vector3(0f, 3f, 2f), 25f);
            float clamped = MotionFeelMath.LookPitch(eye, new Vector3(0f, -5f, 0.5f), 25f);

            Assert.Greater(down, 0f, "+ = looking down");
            Assert.AreEqual(0f, level, 1e-4f);
            Assert.Less(up, 0f);
            Assert.AreEqual(25f, clamped, 1e-4f);
            Assert.AreEqual(0f, MotionFeelMath.LookPitch(eye, eye + Vector3.down, 25f), "right under the eye: no pitch");
        }

        [Test]
        public void InterestTiming_IsJitteredButBounded_AndDeterministic()
        {
            MotionFeelMath.InterestTiming(7, 1, 0.9f, out float hold1, out float cool1);
            MotionFeelMath.InterestTiming(7, 1, 0.9f, out float hold1b, out float cool1b);
            MotionFeelMath.InterestTiming(7, 2, 0.9f, out float hold2, out float cool2);

            Assert.AreEqual(hold1, hold1b);
            Assert.AreEqual(cool1, cool1b);
            Assert.That(hold1, Is.InRange(0.9f * 1.5f, 0.9f * 4f));
            Assert.That(cool1, Is.InRange(0.9f * 2f, 0.9f * 6f));
            Assert.IsTrue(!Mathf.Approximately(hold1, hold2) || !Mathf.Approximately(cool1, cool2), "attention varies between glances");

            MotionFeelMath.InterestTiming(7, 1, 0f, out float holdMin, out _);
            Assert.GreaterOrEqual(holdMin, 0.3f, "a zero base hold still looks for a moment");
        }
    }
}
