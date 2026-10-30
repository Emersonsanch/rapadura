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
        /// <summary>How rests are matched (see <see cref="RetargetPolicy"/>). Only tests should need the legacy value.</summary>
        public RetargetPolicy Policy = RetargetPolicy.RestToRest;
    }

    /// <summary>
    /// Bakes a BVH onto a humanoid avatar: a hidden instance of the model is posed every frame by rotation
    /// retargeting (<see cref="MocapRetargetRig"/>: BVH T-pose ↔ avatar bind pose, arms normalized to an ideal
    /// T-pose, see <see cref="MocapMath"/>), then
    /// <see cref="HumanPoseHandler.GetHumanPose"/> samples muscle space, which becomes a Humanoid clip
    /// (muscle curves + RootT/RootQ) usable by every humanoid hero.
    /// </summary>
    public static class MocapClipBuilder
    {
        public const string DefaultOutputFolder = "Assets/Animations/Mocap";
        public const string DefaultModelPath = "Assets/Models/Characters/Icaro_RO/Rigged/Icaro_Rigged_1.fbx";
        private const string CharactersRoot = "Assets/Models/Characters";

        /// <summary>Folder of the clips baked on one hero's own avatar (<c>Assets/Animations/Mocap/&lt;Hero&gt;</c>).</summary>
        public static string HeroFolder(string hero) => $"{DefaultOutputFolder}/{hero}";

        /// <summary>The clip baked for <paramref name="hero"/> if it exists, else the shared one (baked on Ícaro); null when neither exists.</summary>
        public static AnimationClip LoadForHero(string hero, string clipName)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>($"{HeroFolder(hero)}/{clipName}.anim")
                   ?? AssetDatabase.LoadAssetAtPath<AnimationClip>($"{DefaultOutputFolder}/{clipName}.anim");
        }

        /// <summary>Every hero model <c>Characters/&lt;Hero&gt;_RO/Rigged/&lt;Hero&gt;_Rigged_1.fbx</c> with a valid humanoid Avatar (Ícaro, Joaquim, Lavine today).</summary>
        public static List<(string hero, GameObject model)> HumanoidHeroModels()
        {
            var list = new List<(string, GameObject)>();
            if (!AssetDatabase.IsValidFolder(CharactersRoot)) return list;
            foreach (string guid in AssetDatabase.FindAssets("t:Model", new[] { CharactersRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string file = Path.GetFileNameWithoutExtension(path);
                if (!path.Contains("/Rigged/") || !file.EndsWith("_Rigged_1", StringComparison.Ordinal)) continue;
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var animator = model != null ? model.GetComponentInChildren<Animator>() : null;
                if (animator == null || animator.avatar == null || !animator.avatar.isHuman || !animator.avatar.isValid) continue;
                list.Add((file.Substring(0, file.Length - "_Rigged_1".Length), model));
            }
            list.Sort((a, b) => string.CompareOrdinal(a.Item1, b.Item1));
            return list;
        }

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

            HumanPoseHandler handler = null;
            using (var rig = new MocapRetargetRig(bvh, model, opt.UnitScale, opt.TPoseFrame0, opt.Policy))
            {
                try
                {
                    handler = new HumanPoseHandler(rig.Animator.avatar, rig.Root);
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
                        rig.PoseFrame(first + f);
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
                }
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

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
