using NUnit.Framework;
using Rapadura.Editor.Mocap;
using UnityEngine;

namespace Rapadura.Tests.Mocap
{
    public class MocapMathTests
    {
        private const string TinyBvh = @"HIERARCHY
ROOT Hips
{
	OFFSET 0 0 0
	CHANNELS 6 Xposition Yposition Zposition Zrotation Yrotation Xrotation
	JOINT LeftArm
	{
		OFFSET 1 0 0
		CHANNELS 3 Zrotation Yrotation Xrotation
		End Site
		{
			OFFSET 1 0 0
		}
	}
}
MOTION
Frames: 2
Frame Time: 0.0333333
0 1 0 0 0 0 0 0 0
2 1 3 0 90 0 0 0 -90
";

        private static void AssertDir(Vector3 expected, Vector3 actual, float tol = 1e-4f)
            => Assert.Less((expected.normalized - actual.normalized).magnitude, tol, $"expected {expected} got {actual}");

        [Test]
        public void Parser_ReadsHierarchyAndFrames()
        {
            var clip = BvhParser.Parse(TinyBvh);
            Assert.AreEqual(2, clip.Joints.Count);
            Assert.AreEqual(2, clip.FrameCount);
            Assert.AreEqual(0, clip.Joints[1].Parent);
            Assert.AreEqual(6, clip.Joints[1].ChannelStart);
            Assert.AreEqual(new Vector3(1, 0, 0), clip.Joints[1].EndSite);
            Assert.AreEqual(1f / 30f, clip.FrameTime, 1e-4f);
        }

        [Test]
        public void Evaluate_MirrorsXIntoUnitySpace()
        {
            var clip = BvhParser.Parse(TinyBvh);
            var pos = new Vector3[2];
            var rot = new Quaternion[2];
            clip.Evaluate(0, 1f, pos, rot);
            // BVH +X (character's left) becomes Unity -X (character's left in Unity).
            Assert.AreEqual(new Vector3(-1, 1, 0), pos[1]);

            // Frame 1: root yaw +90 about Y in right-handed space turns +X into -Z; mirrored X does not touch Z.
            clip.Evaluate(1, 2f, pos, rot);
            Assert.AreEqual(-2f * 2f, pos[0].x, 1e-4f);
            Assert.AreEqual(3f * 2f, pos[0].z, 1e-4f);
            AssertDir(new Vector3(0, 0, -1), pos[1] - pos[0]);
        }

        [Test]
        public void ChannelOrder_ComposesInListedOrder()
        {
            var ch = new[] { "Zrotation", "Yrotation", "Xrotation" };
            MocapMath.ReadChannels(ch, new[] { 30f, 40f, 50f }, 0, out _, out var q, out bool hasPos);
            var expected = Quaternion.AngleAxis(30, Vector3.forward) * Quaternion.AngleAxis(40, Vector3.up) * Quaternion.AngleAxis(50, Vector3.right);
            Assert.IsFalse(hasPos);
            Assert.Less(Quaternion.Angle(expected, q), 1e-3f);
        }

        [Test]
        public void RightToLeft_RotationMatchesMirroredVectors()
        {
            var q = Quaternion.Euler(20, 35, -50);
            var v = new Vector3(0.3f, -0.7f, 1.1f);
            Vector3 viaRotation = MocapMath.RightToLeft(q) * MocapMath.RightToLeft(v);
            Vector3 viaMirror = MocapMath.RightToLeft(q * v);
            Assert.Less((viaRotation - viaMirror).magnitude, 1e-4f);
        }

        [Test]
        public void TPoseNormalization_AlignsLimbDirections()
        {
            // An A-pose left upper arm (pointing down-left) is normalized to point straight along -X.
            var aPoseDir = new Vector3(-0.7f, -0.7f, 0.05f);
            var ideal = MocapMath.TPoseDirection(HumanBodyBones.LeftUpperArm).Value;
            var fix = MocapMath.NormalizeToTPose(aPoseDir, ideal);
            AssertDir(Vector3.left, fix * aPoseDir);
            AssertDir(Vector3.right, MocapMath.TPoseDirection(HumanBodyBones.RightLowerArm).Value);
            AssertDir(Vector3.down, MocapMath.TPoseDirection(HumanBodyBones.LeftLowerLeg).Value);
            AssertDir(Vector3.up, MocapMath.TPoseDirection(HumanBodyBones.Neck).Value);
            Assert.IsNull(MocapMath.TPoseDirection(HumanBodyBones.Hips));
        }

        [Test]
        public void OnlyArmsAreNormalizedToTheIdealTPose()
        {
            // Clavicles, spine, legs and feet differ in joint placement between CMU and the Meshy rigs: rest-to-rest.
            Assert.IsTrue(MocapMath.NormalizesRestDirection(HumanBodyBones.LeftUpperArm));
            Assert.IsTrue(MocapMath.NormalizesRestDirection(HumanBodyBones.RightLowerArm));
            foreach (var bone in new[] { HumanBodyBones.LeftShoulder, HumanBodyBones.Spine, HumanBodyBones.Neck,
                                         HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.LeftFoot })
                Assert.IsFalse(MocapMath.NormalizesRestDirection(bone), bone.ToString());
        }

        [Test]
        public void Retarget_TransfersRelativeRotationBetweenDifferentRests()
        {
            // Source bone rest along +X, target bone rest with an arbitrary local frame; a 90° source swing
            // must swing the target's bone direction the same way in world space.
            var sourceRest = Quaternion.identity;
            var targetRest = Quaternion.Euler(10, 70, -30);
            var swing = Quaternion.AngleAxis(90, Vector3.forward);
            var targetNow = MocapMath.Retarget(swing * sourceRest, sourceRest, targetRest);
            var boneAxisLocal = Quaternion.Inverse(targetRest) * Vector3.left; // whatever local axis points along -X at rest
            AssertDir(swing * Vector3.left, targetNow * boneAxisLocal);
        }

        [Test]
        public void CmuLinks_CoverMajorBones()
        {
            var links = MocapMath.CmuLinks(true);
            Assert.IsTrue(links.Exists(l => l.Bone == HumanBodyBones.Hips && l.BvhJoint == "Hips"));
            Assert.IsTrue(links.Exists(l => l.Bone == HumanBodyBones.LeftUpperArm && l.BvhJoint == "LeftArm" && l.BvhChild == "LeftForeArm"));
            Assert.IsTrue(links.Exists(l => l.Bone == HumanBodyBones.RightLowerLeg && l.BvhJoint == "RightLeg"));
            Assert.IsTrue(links.Exists(l => l.Bone == HumanBodyBones.UpperChest));
            Assert.IsFalse(MocapMath.CmuLinks(false).Exists(l => l.Bone == HumanBodyBones.UpperChest));
            Assert.AreEqual("Spine1", MocapMath.CmuLinks(false).Find(l => l.Bone == HumanBodyBones.Chest).BvhJoint);
        }

        [Test]
        public void MuscleNames_MapToClipProperties()
        {
            Assert.AreEqual("LeftHand.Index.1 Stretched", MocapMath.MuscleToClipProperty("Left Index 1 Stretched"));
            Assert.AreEqual("RightHand.Thumb.Spread", MocapMath.MuscleToClipProperty("Right Thumb Spread"));
            Assert.AreEqual("Spine Front-Back", MocapMath.MuscleToClipProperty("Spine Front-Back"));
            // Every Unity muscle name must map to something non-empty.
            foreach (var m in HumanTrait.MuscleName) Assert.IsFalse(string.IsNullOrEmpty(MocapMath.MuscleToClipProperty(m)));
        }

        [Test]
        public void CloseLoop_MakesEndMatchStart()
        {
            var v = new[] { 0f, 1f, 2f, 3.5f };
            MocapMath.CloseLoop(v);
            Assert.AreEqual(0f, v[0], 1e-6f);
            Assert.AreEqual(v[0], v[3], 1e-5f);

            var q = new[] { Quaternion.identity, Quaternion.Euler(0, 5, 0), Quaternion.Euler(0, 12, 0) };
            MocapMath.CloseLoop(q);
            Assert.Less(Quaternion.Angle(q[0], q[2]), 1e-3f);
        }

        [Test]
        public void EnsureContinuity_FlipsHemisphere()
        {
            var q = new[] { Quaternion.identity, new Quaternion(0, 0, 0, -1) };
            MocapMath.EnsureContinuity(q);
            Assert.Greater(q[1].w, 0f);
        }
    }
}
