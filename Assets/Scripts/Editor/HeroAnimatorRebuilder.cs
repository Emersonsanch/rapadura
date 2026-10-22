using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Player.States;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Editor
{
    /// <summary>
    /// Menu <c>Rapadura/Animation/Rebuild Hero Animators</c>: rebuilds every rigged hero's
    /// AnimatorController (<c>Assets/Models/Characters/*_RO/Rigged/*_AnimatorController.controller</c>,
    /// GUIDs kept so scene references survive) into fluid locomotion:
    /// <list type="number">
    /// <item>Walk/Run clips set to loop (they imported as one-shot defaults).</item>
    /// <item>An idle pose clip: the most "standing" frame of the Walk clip, added as a 1-frame
    /// sub-clip (<c>&lt;Hero&gt;_IdlePose</c>) on the Walk FBX importer — the importer does the
    /// humanoid/generic conversion, so it works for Maria's Generic rig too.</item>
    /// <item>Each clip's ground speed measured by sampling it (planted-foot speed relative to the
    /// hips) and baked into the <c>WalkClipSpeed</c>/<c>RunClipSpeed</c> parameter defaults, which
    /// <see cref="PlayerAnimatorSpeedDriver"/> uses for stride-matched playback.</item>
    /// <item>A "Locomotion" state holding a 1D blend tree on <c>MoveBlend</c> (IdlePose 0, Walk 0.5,
    /// Run 1) with its speed bound to <c>MoveSpeedMul</c>; plus, for humanoid heroes, a separate
    /// "Idle" state playing <c>Assets/Animations/Mocap/Mocap_Idle.anim</c> when that baked mocap clip
    /// exists (a long idle inside a synced blend tree would distort the walk cycle timing).</item>
    /// </list>
    /// Safe to run repeatedly.
    /// </summary>
    public static class HeroAnimatorRebuilder
    {
        private const string CharactersRoot = "Assets/Models/Characters";
        private const string MocapIdlePath = "Assets/Animations/Mocap/Mocap_Idle.anim";
        private const string LocomotionStateName = "Locomotion";
        private const string IdleStateName = "Idle";
        private const int SamplesPerCycle = 90;

        /// <summary>Locomotion → Idle below this MoveBlend; back above <see cref="IdleExitBlend"/> (hysteresis).</summary>
        private const float IdleEnterBlend = 0.03f;
        private const float IdleExitBlend = 0.06f;

        [MenuItem("Rapadura/Animation/Rebuild Hero Animators")]
        public static void RebuildAllMenu()
        {
            string report = RebuildAll();
            Debug.Log(report);
            EditorUtility.DisplayDialog("Rebuild Hero Animators", report, "OK");
        }

        /// <summary>Rebuilds every hero controller found; returns a human-readable report.</summary>
        public static string RebuildAll()
        {
            var report = new StringBuilder("[HeroAnimatorRebuilder]\n");
            string[] guids = AssetDatabase.FindAssets("t:AnimatorController", new[] { CharactersRoot });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (!path.Contains("/Rigged/") || !fileName.EndsWith("_AnimatorController", StringComparison.Ordinal))
                {
                    continue;
                }

                string hero = fileName.Substring(0, fileName.Length - "_AnimatorController".Length);
                try
                {
                    report.AppendLine(RebuildHero(hero, path));
                }
                catch (Exception e)
                {
                    report.AppendLine($"{hero}: FAILED — {e.Message}");
                    Debug.LogException(e);
                }
            }

            AssetDatabase.SaveAssets();
            return report.ToString();
        }

        private static string RebuildHero(string hero, string controllerPath)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            string folder = Path.GetDirectoryName(controllerPath)?.Replace('\\', '/');

            FindLocomotionClips(controller, folder, hero, out AnimationClip walk, out AnimationClip run);
            if (walk == null || run == null)
            {
                return $"{hero}: skipped (Walk/Run clips not found).";
            }

            string walkPath = AssetDatabase.GetAssetPath(walk);
            string runPath = AssetDatabase.GetAssetPath(run);
            string walkName = walk.name;
            string runName = run.name;

            // 1) Measure (before any reimport, on the clip's own model so bones/avatar match).
            ClipMeasurement walkMeasure = Measure(walk, walkPath);
            ClipMeasurement runMeasure = Measure(run, runPath);

            // 2) Importer: loop Walk/Run, add the idle pose sub-clip on the Walk FBX.
            string idleName = hero + "_IdlePose";
            ConfigureImporter(walkPath, walkName, idleName, walkMeasure.StandingTime);
            if (runPath != walkPath)
            {
                ConfigureImporter(runPath, runName, null, 0f);
            }

            walk = LoadClip(walkPath, walkName);
            run = LoadClip(runPath, runName);
            AnimationClip idlePose = LoadClip(walkPath, idleName);
            if (walk == null || run == null)
            {
                return $"{hero}: FAILED (clips missing after reimport).";
            }

            bool humanoid = walk.humanMotion;
            AnimationClip mocapIdle = humanoid ? AssetDatabase.LoadAssetAtPath<AnimationClip>(MocapIdlePath) : null;

            // 3) Controller.
            RebuildController(controller, walk, run, idlePose, mocapIdle, walkMeasure.GroundSpeed, runMeasure.GroundSpeed);

            return $"{hero}: Walk '{walkName}' {walkMeasure.GroundSpeed:0.###} u/s, Run '{runName}' {runMeasure.GroundSpeed:0.###} u/s (model units, ×model scale at runtime), "
                   + $"idle pose @ {walkMeasure.StandingTime:0.###}s{(idlePose == null ? " (MISSING)" : string.Empty)}, "
                   + $"{(humanoid ? "Humanoid" : "Generic")}{(mocapIdle != null ? ", Idle state = Mocap_Idle" : string.Empty)}.";
        }

        // ------------------------------------------------------------------
        // Clip discovery
        // ------------------------------------------------------------------

        private static void FindLocomotionClips(AnimatorController controller, string folder, string hero, out AnimationClip walk, out AnimationClip run)
        {
            walk = null;
            run = null;

            if (controller != null && controller.layers.Length > 0)
            {
                foreach (ChildAnimatorState child in controller.layers[0].stateMachine.states)
                {
                    AnimatorState state = child.state;
                    if (state.motion is BlendTree tree)
                    {
                        foreach (ChildMotion motion in tree.children)
                        {
                            if (motion.motion is AnimationClip clip)
                            {
                                if (Mathf.Approximately(motion.threshold, 0.5f)) walk = clip;
                                else if (Mathf.Approximately(motion.threshold, 1f)) run = clip;
                            }
                        }
                    }
                    else if (state.motion is AnimationClip clip)
                    {
                        if (state.name == "Walk") walk = clip;
                        else if (state.name == "Run") run = clip;
                    }
                }
            }

            walk = walk != null ? walk : FirstClip($"{folder}/{hero}_Walk_1.fbx") ?? FirstClip($"{folder}/{hero}_Walk.fbx");
            run = run != null ? run : FirstClip($"{folder}/{hero}_Run_1.fbx") ?? FirstClip($"{folder}/{hero}_Run.fbx");
        }

        private static AnimationClip FirstClip(string fbxPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => !c.name.StartsWith("__preview__", StringComparison.Ordinal) && !c.name.EndsWith("_IdlePose", StringComparison.Ordinal));
        }

        private static AnimationClip LoadClip(string fbxPath, string clipName)
        {
            return AssetDatabase.LoadAllAssetsAtPath(fbxPath)
                .OfType<AnimationClip>()
                .FirstOrDefault(c => c.name == clipName);
        }

        // ------------------------------------------------------------------
        // Measurement
        // ------------------------------------------------------------------

        private struct ClipMeasurement
        {
            public float GroundSpeed;
            public float StandingTime;
        }

        private static ClipMeasurement Measure(AnimationClip clip, string modelPath)
        {
            var result = new ClipMeasurement();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model == null || clip.length <= 0f)
            {
                return result;
            }

            GameObject instance = Object.Instantiate(model);
            instance.hideFlags = HideFlags.DontSave;
            instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            try
            {
                Animator animator = instance.GetComponentInChildren<Animator>();
                Transform root = animator != null ? animator.transform : instance.transform;
                Transform hips = Bone(animator, root, HumanBodyBones.Hips, "Hips");
                Transform leftFoot = Bone(animator, root, HumanBodyBones.LeftFoot, "LeftFoot");
                Transform rightFoot = Bone(animator, root, HumanBodyBones.RightFoot, "RightFoot");
                if (hips == null || leftFoot == null || rightFoot == null)
                {
                    Debug.LogWarning($"[HeroAnimatorRebuilder] {clip.name}: Hips/feet not found on {modelPath}; stride uses the driver fallbacks.");
                    return result;
                }

                var left = new Vector3[SamplesPerCycle + 1];
                var right = new Vector3[SamplesPerCycle + 1];
                float interval = clip.length / SamplesPerCycle;

                AnimationMode.StartAnimationMode();
                try
                {
                    for (int i = 0; i <= SamplesPerCycle; i++)
                    {
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(instance, clip, i * interval);
                        AnimationMode.EndSampling();

                        Vector3 hipsLocal = root.InverseTransformPoint(hips.position);
                        left[i] = root.InverseTransformPoint(leftFoot.position) - new Vector3(hipsLocal.x, 0f, hipsLocal.z);
                        right[i] = root.InverseTransformPoint(rightFoot.position) - new Vector3(hipsLocal.x, 0f, hipsLocal.z);
                    }
                }
                finally
                {
                    AnimationMode.StopAnimationMode();
                }

                float leftSpeed = LocomotionMath.EstimateStanceSpeed(left, interval);
                float rightSpeed = LocomotionMath.EstimateStanceSpeed(right, interval);
                float speed = leftSpeed > 0f && rightSpeed > 0f ? (leftSpeed + rightSpeed) * 0.5f : Mathf.Max(leftSpeed, rightSpeed);
                result.GroundSpeed = float.IsNaN(speed) || float.IsInfinity(speed) ? 0f : speed;
                result.StandingTime = LocomotionMath.ChooseStandingSample(left, right) * interval;
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }

            return result;
        }

        private static Transform Bone(Animator animator, Transform root, HumanBodyBones humanBone, string name)
        {
            if (animator != null && animator.isHuman && animator.avatar != null)
            {
                Transform mapped = animator.GetBoneTransform(humanBone);
                if (mapped != null)
                {
                    return mapped;
                }
            }

            return root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => string.Equals(t.name, name, StringComparison.OrdinalIgnoreCase));
        }

        // ------------------------------------------------------------------
        // Importer
        // ------------------------------------------------------------------

        private static void ConfigureImporter(string fbxPath, string mainClipName, string idleClipName, float idleTime)
        {
            if (!(AssetImporter.GetAtPath(fbxPath) is ModelImporter importer))
            {
                return;
            }

            List<ModelImporterClipAnimation> clips = (importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations).ToList();
            ModelImporterClipAnimation main = clips.FirstOrDefault(c => c.name == mainClipName) ?? clips.FirstOrDefault(c => idleClipName == null || c.name != idleClipName);
            if (main == null)
            {
                return;
            }

            bool changed = importer.clipAnimations.Length == 0;
            if (!main.loopTime)
            {
                main.loopTime = true;
                changed = true;
            }

            if (idleClipName != null)
            {
                clips.RemoveAll(c => c.name == idleClipName);
                float frameRate = importer.defaultClipAnimations.Length > 0 ? Mathf.Max(1f, GuessFrameRate(fbxPath, mainClipName)) : 30f;
                float frame = Mathf.Round(main.firstFrame + idleTime * frameRate);
                frame = Mathf.Clamp(frame, main.firstFrame, Mathf.Max(main.firstFrame, main.lastFrame - 1f));

                var idle = new ModelImporterClipAnimation
                {
                    name = idleClipName,
                    takeName = main.takeName,
                    firstFrame = frame,
                    lastFrame = frame + 1f,
                    loopTime = true,
                    lockRootRotation = main.lockRootRotation,
                    lockRootHeightY = main.lockRootHeightY,
                    lockRootPositionXZ = main.lockRootPositionXZ,
                    keepOriginalOrientation = main.keepOriginalOrientation,
                    keepOriginalPositionY = main.keepOriginalPositionY,
                    keepOriginalPositionXZ = main.keepOriginalPositionXZ,
                    heightFromFeet = main.heightFromFeet,
                    maskType = main.maskType,
                    maskSource = main.maskSource,
                };
                clips.Add(idle);
                changed = true;
            }

            if (changed)
            {
                importer.clipAnimations = clips.ToArray();
                importer.SaveAndReimport();
            }
        }

        private static float GuessFrameRate(string fbxPath, string clipName)
        {
            AnimationClip clip = LoadClip(fbxPath, clipName);
            return clip != null ? clip.frameRate : 30f;
        }

        // ------------------------------------------------------------------
        // Controller
        // ------------------------------------------------------------------

        private static void RebuildController(AnimatorController controller, AnimationClip walk, AnimationClip run, AnimationClip idlePose, AnimationClip mocapIdle, float walkClipSpeed, float runClipSpeed)
        {
            string path = AssetDatabase.GetAssetPath(controller);

            // Parameters: legacy Speed (raw m/s), MoveBlend, MoveSpeedMul and the baked clip speeds.
            while (controller.parameters.Length > 0)
            {
                controller.RemoveParameter(0);
            }

            controller.AddParameter(PlayerAnimatorHashes.SpeedName, AnimatorControllerParameterType.Float);
            controller.AddParameter(PlayerAnimatorHashes.MoveBlendName, AnimatorControllerParameterType.Float);
            controller.AddParameter(PlayerAnimatorHashes.MoveSpeedMultiplierName, AnimatorControllerParameterType.Float);
            controller.AddParameter(PlayerAnimatorHashes.WalkClipSpeedName, AnimatorControllerParameterType.Float);
            controller.AddParameter(PlayerAnimatorHashes.RunClipSpeedName, AnimatorControllerParameterType.Float);

            AnimatorControllerParameter[] parameters = controller.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.name == PlayerAnimatorHashes.MoveSpeedMultiplierName) parameter.defaultFloat = 1f;
                else if (parameter.name == PlayerAnimatorHashes.WalkClipSpeedName) parameter.defaultFloat = walkClipSpeed;
                else if (parameter.name == PlayerAnimatorHashes.RunClipSpeedName) parameter.defaultFloat = runClipSpeed;
            }

            controller.parameters = parameters;

            if (controller.layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
            }

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            foreach (ChildAnimatorState child in machine.states.ToArray())
            {
                machine.RemoveState(child.state);
            }

            foreach (AnimatorStateTransition transition in machine.anyStateTransitions.ToArray())
            {
                machine.RemoveAnyStateTransition(transition);
            }

            foreach (AnimatorTransition transition in machine.entryTransitions.ToArray())
            {
                machine.RemoveEntryTransition(transition);
            }

            // Leftover sub-assets: old blend trees and the empty embedded "Idle" clips (T-pose).
            foreach (Object sub in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (sub != null && AssetDatabase.IsSubAsset(sub) && (sub is BlendTree || sub is AnimationClip))
                {
                    Object.DestroyImmediate(sub, true);
                }
            }

            var tree = new BlendTree
            {
                name = "Locomotion",
                blendType = BlendTreeType.Simple1D,
                blendParameter = PlayerAnimatorHashes.MoveBlendName,
                useAutomaticThresholds = false,
                hideFlags = HideFlags.HideInHierarchy,
            };
            AssetDatabase.AddObjectToAsset(tree, controller);

            if (idlePose != null)
            {
                tree.AddChild(idlePose, 0f);
            }

            tree.AddChild(walk, 0.5f);
            tree.AddChild(run, 1f);

            // The blend tree syncs normalized time across children by their duration: give the
            // 1-frame idle pose the walk's effective duration so it never speeds the walk up.
            ChildMotion[] children = tree.children;
            for (int i = 0; i < children.Length; i++)
            {
                children[i].timeScale = 1f;
                if (idlePose != null && children[i].motion == idlePose && walk.length > 0f)
                {
                    children[i].timeScale = Mathf.Max(0.001f, idlePose.length / walk.length);
                }
            }

            tree.children = children;

            AnimatorState locomotion = machine.AddState(LocomotionStateName, new Vector3(300f, 100f, 0f));
            locomotion.motion = tree;
            locomotion.speedParameterActive = true;
            locomotion.speedParameter = PlayerAnimatorHashes.MoveSpeedMultiplierName;
            locomotion.writeDefaultValues = true;
            machine.defaultState = locomotion;

            if (mocapIdle != null)
            {
                AnimatorState idle = machine.AddState(IdleStateName, new Vector3(300f, 0f, 0f));
                idle.motion = mocapIdle;
                idle.writeDefaultValues = true;
                machine.defaultState = idle;

                AnimatorStateTransition toIdle = locomotion.AddTransition(idle);
                toIdle.hasExitTime = false;
                toIdle.hasFixedDuration = true;
                toIdle.duration = 0.25f;
                toIdle.AddCondition(AnimatorConditionMode.Less, IdleEnterBlend, PlayerAnimatorHashes.MoveBlendName);

                AnimatorStateTransition toLocomotion = idle.AddTransition(locomotion);
                toLocomotion.hasExitTime = false;
                toLocomotion.hasFixedDuration = true;
                toLocomotion.duration = 0.15f;
                toLocomotion.AddCondition(AnimatorConditionMode.Greater, IdleExitBlend, PlayerAnimatorHashes.MoveBlendName);
            }

            EditorUtility.SetDirty(tree);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
    }
}
