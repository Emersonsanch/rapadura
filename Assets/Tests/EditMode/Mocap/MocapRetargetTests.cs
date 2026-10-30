using System.Collections.Generic;
using System.IO;
using System.Text;
using NUnit.Framework;
using Rapadura.Editor.Mocap;
using UnityEngine;

namespace Rapadura.Tests.Mocap
{
    /// <summary>
    /// End-to-end retarget checks on the real hero avatars (Ícaro, Joaquim, Lavine) and the prepared CMU walk
    /// (tools/mocap/out/Mocap_Walk.bvh). Numbers are logged per bone so a regression shows where it hurts.
    /// Root cause these guard against: the old importer forced every bone of both skeletons to an "ideal T-pose"
    /// direction, which copies CMU's skeleton geometry onto the Meshy rigs (T-pose frame 12–55° off the bind pose
    /// on clavicles, shins, feet and spine; measured offline from the .fbx.meta skeletons).
    /// </summary>
    public class MocapRetargetTests
    {
        private const string WalkBvh = "tools/mocap/out/Mocap_Walk.bvh";

        /// <summary>Arms are deliberately normalized (A/T-pose fix): the CMU arm hangs ~8° below horizontal.</summary>
        private const float ArmRestTolerance = 12f;
        private const float RestTolerance = 1f;

        /// <summary>
        /// Arm bones whose own bind pose is off the T-pose beyond what <see cref="ArmRestTolerance"/> budgets for.
        /// A normalized arm lands exactly on the CMU T-pose direction (8.0° below horizontal), so its residual is that
        /// droop plus the avatar's own bind deviation, in whichever direction. Ícaro's Meshy auto-rig is asymmetric:
        /// the right wrist joint sits high (forearm 6.0° above horizontal; left forearm 0.5°, elbow→wrist 12.3 vs 7.5
        /// units), so 8.0 + 6.0 → 14.1°. Measured offline from the .fbx.meta skeletons and the walk's frame 0; every
        /// other hero arm is 6.3–9.8°. A retarget bug would show on every hero, not just on this one bone.
        /// </summary>
        private static readonly Dictionary<(string hero, HumanBodyBones bone), float> RigAsymmetryTolerance =
            new Dictionary<(string hero, HumanBodyBones bone), float>
            {
                { ("Icaro", HumanBodyBones.RightLowerArm), 15f },
            };

        private static float Tolerance(string hero, HumanBodyBones bone)
        {
            if (RigAsymmetryTolerance.TryGetValue((hero, bone), out float own)) return own;
            return MocapMath.NormalizesRestDirection(bone) ? ArmRestTolerance : RestTolerance;
        }

        private static BvhClip LoadWalk()
        {
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, WalkBvh);
            if (!File.Exists(path)) Assert.Ignore($"{WalkBvh} missing — run tools/mocap/bvh_prep.py --set clips.json --out-dir out");
            return BvhParser.Parse(File.ReadAllText(path));
        }

        private static List<(string hero, GameObject model)> Heroes()
        {
            var heroes = MocapClipBuilder.HumanoidHeroModels();
            if (heroes.Count == 0) Assert.Ignore("No humanoid hero models under Assets/Models/Characters.");
            return heroes;
        }

        private static Vector3 BoneDirection(Animator animator, RetargetLink link)
        {
            if (link.UnityChild == HumanBodyBones.LastBone) return Vector3.zero;
            var bone = animator.GetBoneTransform(link.Bone);
            var child = animator.GetBoneTransform(link.UnityChild);
            if (bone == null || child == null) return Vector3.zero;
            Vector3 d = child.position - bone.position;
            return d.sqrMagnitude > 1e-12f ? d.normalized : Vector3.zero;
        }

        /// <summary>Per-bone angle (deg) between the bind pose and the pose of BVH frame 0 (the T-pose).</summary>
        private static Dictionary<HumanBodyBones, float> TPoseErrors(BvhClip bvh, GameObject model, RetargetPolicy policy)
        {
            var errors = new Dictionary<HumanBodyBones, float>();
            using (var rig = new MocapRetargetRig(bvh, model, 1f, true, policy))
            {
                var bind = new Dictionary<HumanBodyBones, Vector3>();
                foreach (var l in rig.Links) bind[l.Bone] = BoneDirection(rig.Animator, l);
                rig.PoseFrame(0);
                foreach (var l in rig.Links)
                {
                    Vector3 now = BoneDirection(rig.Animator, l);
                    if (now == Vector3.zero || bind[l.Bone] == Vector3.zero) continue;
                    errors[l.Bone] = Vector3.Angle(bind[l.Bone], now);
                }
            }
            return errors;
        }

        private static string Table(string title, Dictionary<HumanBodyBones, float> errors)
        {
            var sb = new StringBuilder(title).Append(':');
            foreach (var kv in errors) sb.Append(' ').Append(kv.Key).Append('=').Append(kv.Value.ToString("0.0"));
            return sb.ToString();
        }

        [Test]
        public void TPoseFrame_LandsOnEachHerosBindPose()
        {
            var bvh = LoadWalk();
            var failures = new List<string>();
            foreach (var (hero, model) in Heroes())
            {
                var errors = TPoseErrors(bvh, model, RetargetPolicy.RestToRest);
                TestContext.WriteLine(Table(hero + " rest-to-rest", errors));
                foreach (var kv in errors)
                {
                    float tolerance = Tolerance(hero, kv.Key);
                    if (kv.Value > tolerance) failures.Add($"{hero}.{kv.Key} {kv.Value:0.0}° (max {tolerance}°)");
                }
            }
            Assert.IsEmpty(failures, string.Join("; ", failures));
        }

        [Test]
        public void LegacyNormalizeAll_ReproducesTheDeformation()
        {
            // Documents the bug: forcing clavicles/legs/feet/spine to ideal directions moves them far off the bind pose.
            var bvh = LoadWalk();
            float worst = 0f;
            foreach (var (hero, model) in Heroes())
            {
                var errors = TPoseErrors(bvh, model, RetargetPolicy.NormalizeAll);
                TestContext.WriteLine(Table(hero + " legacy", errors));
                foreach (var kv in errors)
                    if (!MocapMath.NormalizesRestDirection(kv.Key)) worst = Mathf.Max(worst, kv.Value);
            }
            Assert.Greater(worst, 15f, "The legacy policy was expected to deform the non-arm bones by > 15°.");
        }

        [Test]
        public void WalkFrames_ArmsFollowTheMocapAndSurviveMuscleSpace()
        {
            // Arms copy the mocap's world direction; after GetHumanPose → SetHumanPose on a second instance of the
            // same hero every limb must come back close to the posed one (muscle limits / missing DoF would show here).
            var bvh = LoadWalk();
            var failures = new List<string>();
            foreach (var (hero, model) in Heroes())
            {
                using (var rig = new MocapRetargetRig(bvh, model, 1f, true))
                using (var copy = new MocapRetargetRig(bvh, model, 1f, true))
                {
                    var read = new HumanPoseHandler(rig.Animator.avatar, rig.Root);
                    var write = new HumanPoseHandler(copy.Animator.avatar, copy.Root);
                    try
                    {
                        var pose = new HumanPose();
                        float worstArm = 0f, worstTrip = 0f;
                        string worstTripBone = "";
                        for (int f = 1; f < bvh.FrameCount; f += 3)
                        {
                            rig.PoseFrame(f);
                            foreach (var (bone, from, to) in new[]
                                     {
                                         (HumanBodyBones.LeftUpperArm, "LeftArm", "LeftForeArm"), (HumanBodyBones.RightUpperArm, "RightArm", "RightForeArm"),
                                         (HumanBodyBones.LeftLowerArm, "LeftForeArm", "LeftHand"), (HumanBodyBones.RightLowerArm, "RightForeArm", "RightHand"),
                                     })
                            {
                                var link = rig.Links.Find(l => l.Bone == bone);
                                worstArm = Mathf.Max(worstArm, Vector3.Angle(rig.SourceDirection(from, to), BoneDirection(rig.Animator, link)));
                            }

                            read.GetHumanPose(ref pose);
                            copy.ResetToBindPose();
                            write.SetHumanPose(ref pose);
                            foreach (var l in rig.Links)
                            {
                                Vector3 a = BoneDirection(rig.Animator, l), b = BoneDirection(copy.Animator, l);
                                if (a == Vector3.zero || b == Vector3.zero) continue;
                                // Compare in the body frame: SetHumanPose places the body by its own root.
                                Vector3 aLocal = Quaternion.Inverse(rig.Animator.GetBoneTransform(HumanBodyBones.Hips).rotation) * a;
                                Vector3 bLocal = Quaternion.Inverse(copy.Animator.GetBoneTransform(HumanBodyBones.Hips).rotation) * b;
                                float angle = Vector3.Angle(aLocal, bLocal);
                                if (angle > worstTrip) { worstTrip = angle; worstTripBone = $"{l.Bone}@{f}"; }
                            }
                        }
                        TestContext.WriteLine($"{hero}: arm direction error ≤ {worstArm:0.0}°, muscle round-trip ≤ {worstTrip:0.0}° ({worstTripBone})");
                        if (worstArm > 1f) failures.Add($"{hero} arm {worstArm:0.0}°");
                        if (worstTrip > 10f) failures.Add($"{hero} round-trip {worstTrip:0.0}° at {worstTripBone}");
                    }
                    finally
                    {
                        read.Dispose();
                        write.Dispose();
                    }
                }
            }
            Assert.IsEmpty(failures, string.Join("; ", failures));
        }

        [Test]
        public void BakedWalk_MusclesStayInRange_OnEveryHero()
        {
            var bvh = LoadWalk();
            var failures = new List<string>();
            foreach (var (hero, model) in Heroes())
            {
                var clip = MocapClipBuilder.Bake(bvh, model, new MocapImportOptions { ClipName = "Test_Walk", Loop = true, InPlace = true });
                try
                {
                    string report = MocapClipSanity.Describe(clip);
                    TestContext.WriteLine($"{hero}: {(report.Length == 0 ? "all muscles within ±" + MocapClipSanity.MaxMuscle : report)}");
                    if (report.Length > 0) failures.Add(hero + report);
                }
                finally
                {
                    Object.DestroyImmediate(clip);
                }
            }
            Assert.IsEmpty(failures, string.Join("; ", failures));
        }
    }
}
