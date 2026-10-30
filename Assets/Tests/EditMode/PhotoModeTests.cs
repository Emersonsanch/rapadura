using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.PhotoMode;
using Rapadura.UI.PhotoMode;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests of the photo mode's pure parts: orbit camera constraints (<see cref="PhotoCameraMath"/>),
    /// filter presets (<see cref="PhotoFilterPresets"/>), the per-cousin pose table (<see cref="PhotoPoseLibrary"/>)
    /// and the pose rig on a hand-built skeleton (<see cref="PhotoPoseRig"/>), photo file naming
    /// (<see cref="PhotoCaptureService"/>) and the texts (<see cref="PhotoModeTexts"/>).
    /// </summary>
    public class PhotoModeTests
    {
        private const float Epsilon = 1e-3f;

        private static PhotoCameraLimits Limits => PhotoCameraLimits.ForHeroHeight(PhotoCameraLimits.ReferenceHeroHeight);

        // ---------------- Camera math ----------------

        [Test]
        public void Limits_ScaleWithHeroHeight_AndClampTheScale()
        {
            PhotoCameraLimits reference = Limits;
            PhotoCameraLimits doubled = PhotoCameraLimits.ForHeroHeight(PhotoCameraLimits.ReferenceHeroHeight * 2f);
            PhotoCameraLimits giant = PhotoCameraLimits.ForHeroHeight(1000f);

            Assert.AreEqual(reference.MaxDistance * 2f, doubled.MaxDistance, Epsilon);
            Assert.AreEqual(reference.MaxPanRadius * 2f, doubled.MaxPanRadius, Epsilon);
            Assert.AreEqual(reference.MaxDistance * 4f, giant.MaxDistance, Epsilon, "scale is capped at 4x");
            Assert.AreEqual(reference.MaxPitch, doubled.MaxPitch, "angles don't scale");
            Assert.Less(reference.MinDistance, reference.MaxDistance);
            Assert.LessOrEqual(reference.MaxCameraRadius, reference.MaxDistance + reference.MaxPanRadius);
        }

        [Test]
        public void Clamp_KeepsEveryAxisInsideTheLimits()
        {
            PhotoCameraLimits limits = Limits;
            var wild = new PhotoCameraState
            {
                Yaw = -30f, Pitch = 170f, Roll = -90f, Distance = 500f, Fov = 1f, Pan = new Vector3(40f, -40f, 0f),
            };

            PhotoCameraState clamped = PhotoCameraMath.Clamp(wild, limits);

            Assert.AreEqual(330f, clamped.Yaw, Epsilon, "yaw wraps instead of clamping");
            Assert.AreEqual(limits.MaxPitch, clamped.Pitch, Epsilon);
            Assert.AreEqual(-limits.MaxRoll, clamped.Roll, Epsilon);
            Assert.AreEqual(limits.MaxDistance, clamped.Distance, Epsilon);
            Assert.AreEqual(limits.MinFov, clamped.Fov, Epsilon);
            Assert.AreEqual(limits.MaxPanRadius, new Vector2(clamped.Pan.x, clamped.Pan.z).magnitude, Epsilon);
            Assert.AreEqual(limits.MinPanHeight, clamped.Pan.y, Epsilon);

            PhotoCameraState low = PhotoCameraMath.Clamp(new PhotoCameraState { Pitch = -80f, Distance = 0f, Fov = 200f }, limits);
            Assert.AreEqual(limits.MinPitch, low.Pitch, Epsilon);
            Assert.AreEqual(limits.MinDistance, low.Distance, Epsilon);
            Assert.AreEqual(limits.MaxFov, low.Fov, Epsilon);
        }

        [Test]
        public void ClampPan_LeavesSmallOffsetsAlone()
        {
            var pan = new Vector3(0.3f, 0.2f, -0.4f);
            Assert.That(Vector3.Distance(pan, PhotoCameraMath.ClampPan(pan, Limits)), Is.LessThan(Epsilon));
        }

        [Test]
        public void Position_SitsBehindTheFocus_AlongTheView()
        {
            var state = new PhotoCameraState { Yaw = 0f, Pitch = 0f, Distance = 5f, Fov = 60f, Pan = new Vector3(1f, 0f, 0f) };
            Vector3 position = PhotoCameraMath.Position(new Vector3(0f, 1f, 0f), state, state.Distance);
            Assert.That(Vector3.Distance(new Vector3(1f, 1f, -5f), position), Is.LessThan(Epsilon));

            state.Pitch = 90f; // looking straight down: camera above
            Assert.That(Vector3.Distance(Vector3.up * 5f, PhotoCameraMath.Position(Vector3.zero, new PhotoCameraState { Pitch = 90f, Distance = 5f }, 5f)), Is.LessThan(Epsilon));

            state.Pitch = 0f;
            state.Roll = 25f; // roll never moves the camera
            Assert.That(Vector3.Distance(new Vector3(1f, 1f, -5f), PhotoCameraMath.Position(new Vector3(0f, 1f, 0f), state, 5f)), Is.LessThan(Epsilon));
        }

        [Test]
        public void Collision_PullsTheCameraIn_ButNeverInsideTheMinimum()
        {
            Assert.AreEqual(6f, PhotoCameraMath.ResolveCollisionDistance(6f, false, 0f, 1f, 0.1f), Epsilon);
            Assert.AreEqual(2.9f, PhotoCameraMath.ResolveCollisionDistance(6f, true, 3f, 1f, 0.1f), Epsilon);
            Assert.AreEqual(1f, PhotoCameraMath.ResolveCollisionDistance(6f, true, 0.2f, 1f, 0.1f), Epsilon);
            Assert.AreEqual(2f, PhotoCameraMath.ResolveCollisionDistance(2f, true, 5f, 1f, 0.1f), Epsilon, "a hit beyond the camera changes nothing");
        }

        [Test]
        public void ClampToRadius_PullsFarPositionsBackOntoTheSphere()
        {
            Vector3 center = new Vector3(1f, 0f, 1f);
            Vector3 clamped = PhotoCameraMath.ClampToRadius(center + new Vector3(0f, 0f, 20f), center, 4f);
            Assert.That(Vector3.Distance(center + new Vector3(0f, 0f, 4f), clamped), Is.LessThan(Epsilon));

            Vector3 near = center + Vector3.one;
            Assert.AreEqual(near, PhotoCameraMath.ClampToRadius(near, center, 4f));
        }

        [Test]
        public void PlanarPan_FollowsTheCameraYaw()
        {
            Vector3 forward = PhotoCameraMath.PlanarPanDelta(90f, new Vector2(0f, 1f), 0f);
            Assert.That(Vector3.Distance(Vector3.right, forward), Is.LessThan(Epsilon), "W at yaw 90 moves along +X");

            Vector3 strafe = PhotoCameraMath.PlanarPanDelta(0f, new Vector2(1f, 0f), 0.5f);
            Assert.That(Vector3.Distance(new Vector3(1f, 0.5f, 0f), strafe), Is.LessThan(Epsilon));
        }

        [Test]
        public void FromCamera_RecoversTheOrbitThatPlacedTheCamera()
        {
            PhotoCameraLimits limits = Limits;
            var state = new PhotoCameraState { Yaw = 45f, Pitch = 35f, Distance = 6f, Fov = 50f };
            Vector3 focus = new Vector3(2f, 1f, -3f);
            Vector3 position = PhotoCameraMath.Position(focus, state, state.Distance);

            PhotoCameraState recovered = PhotoCameraMath.FromCamera(position, PhotoCameraMath.Rotation(state), focus, 50f, limits);

            Assert.AreEqual(45f, recovered.Yaw, 0.05f);
            Assert.AreEqual(35f, recovered.Pitch, 0.05f);
            Assert.AreEqual(6f, recovered.Distance, Epsilon);
            Assert.AreEqual(0f, recovered.Roll);
            Assert.AreEqual(Vector3.zero, recovered.Pan);

            // The MU-style gameplay camera sits ~15 m away: photo mode starts at its max distance instead.
            Vector3 far = PhotoCameraMath.Position(focus, new PhotoCameraState { Yaw = 45f, Pitch = 57.5f }, 15f);
            Assert.AreEqual(limits.MaxDistance, PhotoCameraMath.FromCamera(far, Quaternion.identity, focus, 60f, limits).Distance, Epsilon);
        }

        [Test]
        public void Damp_IsFrameRateIndependent_AndTakesTheShortYawArc()
        {
            Assert.AreEqual(1f, PhotoCameraMath.DampFactor(0f, 0.016f), "sharpness 0 snaps");
            Assert.AreEqual(0f, PhotoCameraMath.DampFactor(10f, 0f), Epsilon);

            float oneStep = PhotoCameraMath.DampFactor(10f, 0.1f);
            float twoSteps = 1f - (1f - PhotoCameraMath.DampFactor(10f, 0.05f)) * (1f - PhotoCameraMath.DampFactor(10f, 0.05f));
            Assert.AreEqual(oneStep, twoSteps, Epsilon);

            var current = new PhotoCameraState { Yaw = 350f, Distance = 4f };
            var target = new PhotoCameraState { Yaw = 10f, Distance = 4f };
            PhotoCameraState half = PhotoCameraMath.Damp(current, target, 10f, Mathf.Log(2f) / 10f);
            Assert.AreEqual(0f, Mathf.DeltaAngle(0f, half.Yaw), 0.05f, "350 → 10 goes through 0, not 180");
        }

        [Test]
        public void ZoomDistance_TreatsWheelNotchesTheSameOnEveryPlatform()
        {
            Assert.AreEqual(9f, PhotoCameraMath.ZoomDistance(10f, 120f), Epsilon);
            Assert.AreEqual(9f, PhotoCameraMath.ZoomDistance(10f, 1f), Epsilon);
            Assert.AreEqual(10f / 0.9f, PhotoCameraMath.ZoomDistance(10f, -120f), Epsilon);
            Assert.AreEqual(10f, PhotoCameraMath.ZoomDistance(10f, 0f));
        }

        // ---------------- Filters ----------------

        [Test]
        public void FilterPresets_CoverEveryLook_InSelectorOrder()
        {
            CollectionAssert.AreEqual((PhotoFilter[])Enum.GetValues(typeof(PhotoFilter)), PhotoFilterPresets.All);
            Assert.IsFalse(PhotoFilterPresets.Get(PhotoFilter.None).Enabled);

            foreach (PhotoFilter filter in PhotoFilterPresets.All.Where(f => f != PhotoFilter.None))
            {
                PhotoFilterSettings s = PhotoFilterPresets.Get(filter);
                Assert.IsTrue(s.Enabled, filter.ToString());
                Assert.That(s.Contrast, Is.InRange(-100f, 100f), filter.ToString());
                Assert.That(s.Saturation, Is.InRange(-100f, 100f), filter.ToString());
                Assert.That(s.Temperature, Is.InRange(-100f, 100f), filter.ToString());
                Assert.That(s.VignetteIntensity, Is.InRange(0f, 1f), filter.ToString());
                Assert.That(s.GrainIntensity, Is.InRange(0f, 1f), filter.ToString());
                Assert.That(s.BloomIntensity, Is.GreaterThanOrEqualTo(0f), filter.ToString());
            }
        }

        [Test]
        public void FilterPresets_HaveTheirCharacter()
        {
            PhotoFilterSettings cordel = PhotoFilterPresets.Get(PhotoFilter.Cordel);
            PhotoFilterSettings woodcut = PhotoFilterPresets.Get(PhotoFilter.Woodcut);
            PhotoFilterSettings sunset = PhotoFilterPresets.Get(PhotoFilter.Sunset);
            PhotoFilterSettings night = PhotoFilterPresets.Get(PhotoFilter.SaoJoaoNight);

            Assert.AreEqual(-100f, cordel.Saturation, "sepia starts from mono");
            Assert.Greater(cordel.ColorFilter.r, cordel.ColorFilter.b, "sepia tint is warm");
            Assert.Greater(cordel.GrainIntensity, 0f);
            Assert.Greater(cordel.VignetteIntensity, 0f);

            Assert.AreEqual(-100f, woodcut.Saturation);
            Assert.Greater(woodcut.Contrast, 60f, "xilogravura is high contrast");

            Assert.Greater(sunset.Temperature, 0f, "entardecer is warm");
            Assert.Less(night.Temperature, 0f, "São João night is cool");
            Assert.Greater(night.BloomIntensity, sunset.BloomIntensity, "the bonfire glow blooms the most");
        }

        [Test]
        public void ReduceFlashes_CapsBloom_AndTonesDownGrain()
        {
            PhotoFilterSettings night = PhotoFilterPresets.Get(PhotoFilter.SaoJoaoNight);
            PhotoFilterSettings reduced = PhotoFilterPresets.ForAccessibility(night, reduceFlashes: true);
            Assert.LessOrEqual(reduced.BloomIntensity, PhotoFilterPresets.ReducedFlashMaxBloom);

            PhotoFilterSettings cordel = PhotoFilterPresets.Get(PhotoFilter.Cordel);
            Assert.Less(PhotoFilterPresets.ForAccessibility(cordel, true).GrainIntensity, cordel.GrainIntensity);

            PhotoFilterSettings untouched = PhotoFilterPresets.ForAccessibility(night, reduceFlashes: false);
            Assert.AreEqual(night.BloomIntensity, untouched.BloomIntensity);
            Assert.AreEqual(night.GrainIntensity, untouched.GrainIntensity);
        }

        [Test]
        public void FilterCycle_WrapsBothWays()
        {
            Assert.AreEqual(PhotoFilter.Cordel, PhotoFilterPresets.Cycle(PhotoFilter.None, 1));
            Assert.AreEqual(PhotoFilter.SaoJoaoNight, PhotoFilterPresets.Cycle(PhotoFilter.None, -1));
            Assert.AreEqual(PhotoFilter.None, PhotoFilterPresets.Cycle(PhotoFilter.SaoJoaoNight, 1));
            Assert.AreEqual(PhotoFilter.Woodcut, PhotoFilterPresets.Cycle(PhotoFilter.Cordel, 6));
        }

        // ---------------- Poses ----------------

        [Test]
        public void EveryCousin_HasFivePoses_EndingWithTheirSignature()
        {
            var signatureKeys = new HashSet<string>();
            foreach (CharacterId id in Enum.GetValues(typeof(CharacterId)))
            {
                IReadOnlyList<PhotoPoseDefinition> poses = PhotoPoseLibrary.For(id);
                Assert.AreEqual(PhotoPoseLibrary.PoseCount, poses.Count, id.ToString());
                CollectionAssert.AreEqual(
                    new[] { PhotoPoseKind.Neutral, PhotoPoseKind.Wave, PhotoPoseKind.Victory, PhotoPoseKind.Sit, PhotoPoseKind.Signature },
                    poses.Select(p => p.Kind).ToArray(), id.ToString());
                Assert.IsTrue(signatureKeys.Add(poses[4].LabelKey), $"{id}: signature label must be its own");

                PhotoPoseDefinition signature = poses[4];
                bool movesSomething = new[] { signature.LeftUpperArm, signature.RightUpperArm, signature.LeftForeArm, signature.RightForeArm }
                    .Any(v => v.sqrMagnitude > 0.01f);
                Assert.IsTrue(movesSomething, $"{id}: the signature pose must move the arms");
            }

            Assert.AreEqual(PhotoPoseLibrary.PoseCount, PhotoPoseLibrary.For(null).Count, "no hero known: still five entries");
        }

        [Test]
        public void PoseIndex_Wraps()
        {
            Assert.AreEqual(4, PhotoPoseLibrary.WrapIndex(-1, 5));
            Assert.AreEqual(0, PhotoPoseLibrary.WrapIndex(5, 5));
            Assert.AreEqual(0, PhotoPoseLibrary.WrapIndex(3, 0));
            Assert.AreEqual(PhotoPoseKind.Signature, PhotoPoseLibrary.Get(CharacterId.Lavine, -1).Kind);
        }

        [Test]
        public void PoseRig_AimsTheArms_AndRestoresTheCapturedPose()
        {
            GameObject root = BuildSkeleton(out Transform rightArm, out Transform rightForeArm, out Transform leftArm);
            try
            {
                var rig = new PhotoPoseRig(root.transform, root.transform, 1.8f, 0f);
                Assert.IsTrue(rig.IsRigged);

                Quaternion restRight = rightArm.localRotation;
                Quaternion restLeft = leftArm.localRotation;
                rig.Capture();

                PhotoPoseDefinition victory = PhotoPoseLibrary.Get(CharacterId.Maria, 2);
                rig.SetPose(victory, instant: true);
                rig.Apply(0f, 0.016f, reducedMotion: false);

                Vector3 segment = (rightForeArm.position - rightArm.position).normalized;
                Assert.That(Vector3.Angle(victory.RightUpperArm.normalized, segment), Is.LessThan(1f), "right upper arm points where the pose says (hero space = world here)");

                // Applying again doesn't accumulate (absolute writes from the snapshot).
                Quaternion once = rightArm.rotation;
                rig.Apply(0f, 0.016f, reducedMotion: false);
                Assert.That(Quaternion.Angle(once, rightArm.rotation), Is.LessThan(0.01f));

                rig.Restore();
                Assert.That(Quaternion.Angle(restRight, rightArm.localRotation), Is.LessThan(0.01f));
                Assert.That(Quaternion.Angle(restLeft, leftArm.localRotation), Is.LessThan(0.01f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PoseRig_WithoutBones_FallsBackToWholeBody()
        {
            var root = new GameObject("RigidHero");
            try
            {
                var rig = new PhotoPoseRig(root.transform, root.transform, 1.8f, 0f);
                Assert.IsFalse(rig.IsRigged);
                rig.Capture();
                rig.SetPose(PhotoPoseLibrary.Get(CharacterId.Maithe, 2), instant: true); // Vitória: lift + lean back
                rig.Apply(0f, 0.016f, reducedMotion: false);
                Assert.Greater(root.transform.position.y, 0f);

                rig.Restore();
                Assert.That(root.transform.position.magnitude, Is.LessThan(Epsilon));
                Assert.That(Quaternion.Angle(Quaternion.identity, root.transform.rotation), Is.LessThan(0.01f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        /// <summary>T-pose-ish Meshy-named skeleton facing +Z (right = +X), arms hanging down.</summary>
        private static GameObject BuildSkeleton(out Transform rightArm, out Transform rightForeArm, out Transform leftArm)
        {
            var root = new GameObject("TestHero");
            Transform hips = Child(root.transform, "Hips", new Vector3(0f, 1f, 0f));
            Transform spine = Child(hips, "Spine02", new Vector3(0f, 0.2f, 0f));
            Child(spine, "Head", new Vector3(0f, 0.5f, 0f));
            leftArm = Child(spine, "LeftArm", new Vector3(-0.2f, 0.4f, 0f));
            Transform leftFore = Child(leftArm, "LeftForeArm", new Vector3(0f, -0.3f, 0f));
            Child(leftFore, "LeftHand", new Vector3(0f, -0.25f, 0f));
            rightArm = Child(spine, "RightArm", new Vector3(0.2f, 0.4f, 0f));
            rightForeArm = Child(rightArm, "RightForeArm", new Vector3(0f, -0.3f, 0f));
            Child(rightForeArm, "RightHand", new Vector3(0f, -0.25f, 0f));
            return root;
        }

        private static Transform Child(Transform parent, string name, Vector3 localPosition)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            return go.transform;
        }

        // ---------------- Capture naming ----------------

        [Test]
        public void PhotoFileName_IsTimestamped_WithTheHero()
        {
            var time = new DateTime(2026, 10, 29, 21, 30, 45);
            Assert.AreEqual("rapadura_20261029_213045_icaro.png", PhotoCaptureService.BuildFileName(time, CharacterId.Icaro));
            Assert.AreEqual("rapadura_20261029_213045.png", PhotoCaptureService.BuildFileName(time, null));
        }

        [Test]
        public void PhotoPath_AvoidsOverwritingAPhotoFromTheSameSecond()
        {
            string folder = Path.Combine("x", PhotoCaptureService.FolderName);
            var taken = new HashSet<string> { Path.Combine(folder, "a.png"), Path.Combine(folder, "a_2.png") };

            Assert.AreEqual(Path.Combine(folder, "b.png"), PhotoCaptureService.BuildUniquePath(folder, "b.png", taken.Contains));
            Assert.AreEqual(Path.Combine(folder, "a_3.png"), PhotoCaptureService.BuildUniquePath(folder, "a.png", taken.Contains));
            Assert.AreEqual(Path.Combine(folder, "a.png"), PhotoCaptureService.BuildUniquePath(folder, "a.png", null));
        }

        // ---------------- Texts ----------------

        [Test]
        public void Texts_AreComplete_AndCoverEveryFilterAndPoseLabel()
        {
            var keys = new HashSet<string>();
            foreach (var entry in PhotoModeTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate key " + entry.Key);
                Assert.That(entry.Key, Does.StartWith("ui.photo."));
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key + " en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key + " pt-BR");
            }

            foreach (PhotoFilter filter in PhotoFilterPresets.All)
            {
                Assert.That(keys, Does.Contain(PhotoFilterPresets.LabelKey(filter)), filter.ToString());
            }

            foreach (CharacterId id in Enum.GetValues(typeof(CharacterId)))
            {
                foreach (PhotoPoseDefinition pose in PhotoPoseLibrary.For(id))
                {
                    Assert.That(keys, Does.Contain(pose.LabelKey), $"{id} {pose.Kind}");
                }
            }

            Assert.AreEqual("Modo foto", PhotoModeTexts.Get(null, PhotoModeTexts.Title));
            Assert.AreEqual("Foto salva em C:/x.png", PhotoModeTexts.Format(null, PhotoModeTexts.Saved, "C:/x.png"));
            Assert.AreEqual("Noite de São João", PhotoModeTexts.Get(null, PhotoFilterPresets.LabelKey(PhotoFilter.SaoJoaoNight)));
        }
    }
}
