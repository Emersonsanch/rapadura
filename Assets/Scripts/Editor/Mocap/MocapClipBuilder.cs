using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Rapadura.Editor.Mocap
{
    /// <summary>Options for one BVH → Humanoid AnimationClip bake.</summary>
    [Serializable]
    public sealed class MocapImportOptions
    {
        public string ClipName = "Mocap_Clip";
        public string OutputFolder = MocapClipBuilder.DefaultOutputFolder;
        /// <summary>BVH units → meters. Files from tools/mocap/out are already in meters (1); raw CMU files use <see cref="MocapMath.CmuUnitToMeters"/>.</summary>
        public float UnitScale = 1f;
        /// <summary>Frame 0 is a T-pose (cgspeed CMU conversion and tools/mocap output) — used as the rest reference and dropped from the clip.</summary>
        public bool TPoseFrame0 = true;
        public bool Loop;
        /// <summary>Removes the start→end XZ drift of the root (keeps sway) — locomotion cycles played in place.</summary>
        public bool InPlace;
        /// <summary>Keep XZ/rotation root motion (not baked into the pose) — e.g. a turn clip used with Apply Root Motion.</summary>
        public bool RootMotion;
    }

    /// <summary>
    /// Bakes a BVH onto a humanoid avatar: a hidden instance of the model is posed every frame by rotation
    /// retargeting (both rest poses normalized to an ideal T-pose, see <see cref="MocapMath"/>), then
    /// <see cref="HumanPoseHandler.GetHumanPose"/> samples muscle space, which becomes a Humanoid clip
    /// (muscle curves + RootT/RootQ) usable by every humanoid hero.
    /// </summary>
    public static class MocapClipBuilder
    {
        public const string DefaultOutputFolder = "Assets/Animations/Mocap";
        public const string DefaultModelPath = "Assets/Models/Characters/Icaro_RO/Rigged/Icaro_Rigged_1.fbx";

        public static AnimationClip ImportFile(string bvhPath, GameObject model, MocapImportOptions opt)
        {
            var bvh = BvhParser.Parse(File.ReadAllText(bvhPath));
            var clip = Bake(bvh, model, opt);
            return SaveClip(clip, opt);
        }

        public static AnimationClip Bake(BvhClip bvh, GameObject model, MocapImportOptions opt)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (bvh.FrameCount < (opt.TPoseFrame0 ? 3 : 2)) throw new InvalidOperationException("BVH has too few frames.");

            var instance = UnityEngine.Object.Instantiate(model);
            instance.hideFlags = HideFlags.HideAndDontSave;
            HumanPoseHandler handler = null;
            try
            {
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                instance.transform.localScale = Vector3.one;
                var animator = instance.GetComponentInChildren<Animator>();
                if (animator == null || animator.avatar == null || !animator.avatar.isHuman || !animator.avatar.isValid)
                    throw new InvalidOperationException($"'{model.name}' has no valid humanoid Avatar (Rig → Animation Type: Humanoid).");
                var avatar = animator.avatar;
                var root = animator.transform;

                ApplyAvatarTPose(instance.transform, avatar);

                bool hasUpperChest = animator.GetBoneTransform(HumanBodyBones.UpperChest) != null;
                var links = MocapMath.CmuLinks(hasUpperChest)
                    .Where(l => animator.GetBoneTransform(l.Bone) != null && bvh.IndexOf(l.BvhJoint) >= 0)
                    .ToList();
                if (!links.Any(l => l.Bone == HumanBodyBones.Hips))
                    throw new InvalidOperationException("BVH has no 'Hips' joint — only the CMU/MotionBuilder naming is mapped.");

                // Parents first so setting a world rotation never disturbs an already-posed child.
                links.Sort((a, b) => Depth(animator.GetBoneTransform(a.Bone)).CompareTo(Depth(animator.GetBoneTransform(b.Bone))));

                // ---- target (avatar) rest, normalized to the ideal T-pose
                var targetRest = new Dictionary<HumanBodyBones, Quaternion>();
                foreach (var l in links)
                {
                    var t = animator.GetBoneTransform(l.Bone);
                    var rest = t.rotation;
                    var ideal = MocapMath.TPoseDirection(l.Bone);
                    var child = l.UnityChild == HumanBodyBones.LastBone ? null : animator.GetBoneTransform(l.UnityChild);
                    if (ideal.HasValue && child != null)
                        rest = MocapMath.NormalizeToTPose(child.position - t.position, ideal.Value) * rest;
                    targetRest[l.Bone] = rest;
                }
                var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                float targetFootY = LowestY(animator, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.LeftToes, HumanBodyBones.RightToes);
                float targetHipsHeight = hips.position.y - targetFootY;

                // ---- source (BVH) rest
                int J = bvh.Joints.Count;
                var pos = new Vector3[J];
                var rot = new Quaternion[J];
                var sourceRest = new Dictionary<HumanBodyBones, Quaternion>();
                if (opt.TPoseFrame0) bvh.Evaluate(0, opt.UnitScale, pos, rot);
                else EvaluateZeroPose(bvh, opt.UnitScale, pos, rot);
                foreach (var l in links)
                {
                    int j = bvh.IndexOf(l.BvhJoint);
                    var rest = rot[j];
                    var ideal = MocapMath.TPoseDirection(l.Bone);
                    int c = l.BvhChild == null ? -1 : bvh.IndexOf(l.BvhChild);
                    if (ideal.HasValue && c >= 0)
                        rest = MocapMath.NormalizeToTPose(pos[c] - pos[j], ideal.Value) * rest;
                    sourceRest[l.Bone] = rest;
                }
                int hipsJ = bvh.IndexOf("Hips");
                float sourceFootY = LowestY(bvh, pos, "LeftFoot", "RightFoot", "LeftToeBase", "RightToeBase");
                float sourceHipsHeight = pos[hipsJ].y - sourceFootY;
                float ratio = sourceHipsHeight > 1e-4f ? targetHipsHeight / sourceHipsHeight : 1f;

                // ---- bake
                handler = new HumanPoseHandler(avatar, root);
                var pose = new HumanPose();
                int first = opt.TPoseFrame0 ? 1 : 0;
                int n = bvh.FrameCount - first;
                int muscleCount = HumanTrait.MuscleCount;
                var muscles = new float[muscleCount][];
                for (int m = 0; m < muscleCount; m++) muscles[m] = new float[n];
                var rootT = new Vector3[n];
                var rootQ = new Quaternion[n];

                for (int f = 0; f < n; f++)
                {
                    bvh.Evaluate(first + f, opt.UnitScale, pos, rot);
                    var hp = pos[hipsJ];
                    hips.position = new Vector3(hp.x * ratio, targetFootY + (hp.y - sourceFootY) * ratio, hp.z * ratio);
                    foreach (var l in links)
                    {
                        var t = animator.GetBoneTransform(l.Bone);
                        t.rotation = MocapMath.Retarget(rot[bvh.IndexOf(l.BvhJoint)], sourceRest[l.Bone], targetRest[l.Bone]);
                    }
                    handler.GetHumanPose(ref pose);
                    rootT[f] = pose.bodyPosition;
                    rootQ[f] = pose.bodyRotation;
                    for (int m = 0; m < muscleCount; m++) muscles[m][f] = pose.muscles[m];
                }

                return BuildClip(opt, bvh.FrameTime, muscles, rootT, rootQ);
            }
            finally
            {
                handler?.Dispose();
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        /// <summary>Builds the humanoid clip from sampled muscle space (loop closure / in-place applied here).</summary>
        public static AnimationClip BuildClip(MocapImportOptions opt, float frameTime, float[][] muscles, Vector3[] rootT, Quaternion[] rootQ)
        {
            int n = rootT.Length;
            MocapMath.EnsureContinuity(rootQ);
            if (opt.InPlace && n > 1)
            {
                Vector3 drift = rootT[n - 1] - rootT[0];
                for (int f = 0; f < n; f++)
                {
                    float k = (float)f / (n - 1);
                    rootT[f] -= new Vector3(drift.x * k, 0f, drift.z * k);
                }
            }

            var clip = new AnimationClip { name = opt.ClipName, frameRate = Mathf.Round(1f / frameTime) };
            var tx = new float[n]; var ty = new float[n]; var tz = new float[n];
            for (int f = 0; f < n; f++) { tx[f] = rootT[f].x; ty[f] = rootT[f].y; tz[f] = rootT[f].z; }
            if (opt.Loop)
            {
                foreach (var m in muscles) MocapMath.CloseLoop(m);
                MocapMath.CloseLoop(ty);
                if (opt.InPlace) { MocapMath.CloseLoop(tx); MocapMath.CloseLoop(tz); }
                if (!opt.RootMotion) MocapMath.CloseLoop(rootQ);
            }

            for (int m = 0; m < muscles.Length; m++)
                SetCurve(clip, MocapMath.MuscleToClipProperty(HumanTrait.MuscleName[m]), frameTime, muscles[m]);
            SetCurve(clip, "RootT.x", frameTime, tx);
            SetCurve(clip, "RootT.y", frameTime, ty);
            SetCurve(clip, "RootT.z", frameTime, tz);
            SetCurve(clip, "RootQ.x", frameTime, rootQ.Select(q => q.x).ToArray());
            SetCurve(clip, "RootQ.y", frameTime, rootQ.Select(q => q.y).ToArray());
            SetCurve(clip, "RootQ.z", frameTime, rootQ.Select(q => q.z).ToArray());
            SetCurve(clip, "RootQ.w", frameTime, rootQ.Select(q => q.w).ToArray());

            var s = AnimationUtility.GetAnimationClipSettings(clip);
            s.loopTime = opt.Loop;
            s.loopBlend = opt.Loop;
            s.loopBlendOrientation = !opt.RootMotion;   // "Bake Into Pose"
            s.loopBlendPositionY = true;
            s.loopBlendPositionXZ = !opt.RootMotion;
            s.keepOriginalOrientation = true;            // "Based Upon: Original"
            s.keepOriginalPositionY = true;
            s.keepOriginalPositionXZ = true;
            s.heightFromFeet = false;
            AnimationUtility.SetAnimationClipSettings(clip, s);
            return clip;
        }

        /// <summary>Writes (or overwrites in place, keeping the GUID so controllers stay wired) the clip asset.</summary>
        public static AnimationClip SaveClip(AnimationClip clip, MocapImportOptions opt)
        {
            EnsureFolder(opt.OutputFolder);
            string path = $"{opt.OutputFolder}/{opt.ClipName}.anim";
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (existing != null)
            {
                EditorUtility.CopySerialized(clip, existing);
                existing.name = opt.ClipName;
                EditorUtility.SetDirty(existing);
                UnityEngine.Object.DestroyImmediate(clip);
                clip = existing;
            }
            else
            {
                AssetDatabase.CreateAsset(clip, path);
            }
            AssetDatabase.SaveAssets();
            return clip;
        }

        // ------------------------------------------------------------------ helpers

        private static void SetCurve(AnimationClip clip, string property, float frameTime, float[] values)
        {
            var keys = new Keyframe[values.Length];
            for (int i = 0; i < values.Length; i++) keys[i] = new Keyframe(i * frameTime, values[i]);
            var curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++) curve.SmoothTangents(i, 0f);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), property), curve);
        }

        /// <summary>Puts the instance in the T-pose stored in the Avatar's HumanDescription (what the avatar was configured with).</summary>
        private static void ApplyAvatarTPose(Transform instanceRoot, Avatar avatar)
        {
            var byName = new Dictionary<string, Transform>();
            foreach (var t in instanceRoot.GetComponentsInChildren<Transform>(true))
                if (!byName.ContainsKey(t.name)) byName[t.name] = t;
            var skeleton = avatar.humanDescription.skeleton;
            if (skeleton == null) return;
            foreach (var sb in skeleton)
            {
                if (!byName.TryGetValue(sb.name, out var t) || t == instanceRoot) continue;
                t.localPosition = sb.position;
                t.localRotation = sb.rotation;
                t.localScale = sb.scale;
            }
        }

        private static void EvaluateZeroPose(BvhClip bvh, float scale, Vector3[] pos, Quaternion[] rot)
        {
            var saved = bvh.Frames[0];
            var zero = new float[saved.Length];
            var rootJ = bvh.Joints[0];
            for (int c = 0; c < rootJ.Channels.Count; c++)
                if (rootJ.Channels[c].EndsWith("position", StringComparison.OrdinalIgnoreCase))
                    zero[rootJ.ChannelStart + c] = saved[rootJ.ChannelStart + c];
            bvh.Frames[0] = zero;
            try { bvh.Evaluate(0, scale, pos, rot); }
            finally { bvh.Frames[0] = saved; }
        }

        private static float LowestY(Animator a, params HumanBodyBones[] bones)
        {
            float y = float.PositiveInfinity;
            foreach (var b in bones) { var t = a.GetBoneTransform(b); if (t != null) y = Mathf.Min(y, t.position.y); }
            return float.IsPositiveInfinity(y) ? 0f : y;
        }

        private static float LowestY(BvhClip bvh, Vector3[] pos, params string[] joints)
        {
            float y = float.PositiveInfinity;
            foreach (var n in joints) { int j = bvh.IndexOf(n); if (j >= 0) y = Mathf.Min(y, pos[j].y); }
            return float.IsPositiveInfinity(y) ? 0f : y;
        }

        private static int Depth(Transform t)
        {
            int d = 0;
            while (t != null) { d++; t = t.parent; }
            return d;
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
