using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rapadura.Editor.Mocap;
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
    /// <item>Mocap locomotion (switch <see cref="UseMocapLocomotion"/>, menu <c>Rapadura/Animation/Use Mocap
    /// Locomotion</c>, on by default): Humanoid heroes get the CMU clips baked on their own avatar
    /// (<c>Assets/Animations/Mocap/&lt;Hero&gt;/</c>, else the shared Ícaro bake) — Walk = <see cref="MocapWalkClip"/>,
    /// Jog (threshold 0.75) = <see cref="MocapJogClip"/>, Run = <see cref="MocapRunClip"/> and the Idle state =
    /// <see cref="MocapIdleClip"/> — each measured the same way and checked by <see cref="MocapClipSanity"/>. A missing
    /// or out-of-range clip falls back to the Meshy Walk/Run, as do the Generic (Maria) and rig-less (Maithe) heroes.</item>
    /// </list>
    /// Safe to run repeatedly.
    /// </summary>
    public static partial class HeroAnimatorRebuilder
    {
        private const string CharactersRoot = "Assets/Models/Characters";
        private const string MocapIdlePath = "Assets/Animations/Mocap/Mocap_Idle.anim";
        private const string LocomotionStateName = "Locomotion";
        private const string IdleStateName = "Idle";
        private const int SamplesPerCycle = 90;

        /// <summary>Locomotion → Idle below this MoveBlend; back above <see cref="IdleExitBlend"/> (hysteresis).</summary>
        private const float IdleEnterBlend = 0.03f;
        private const float IdleExitBlend = 0.06f;

        /// <summary>
        /// When true, <see cref="RebuildAll"/> also wires the Mocap_*.anim action clips (see
        /// HeroAnimatorRebuilder.Actions.cs). Off by default: the CMU retarget still produces
        /// out-of-range muscles (Mocap_Idle lifts a leg, the death clips twist ±2.8), which left the
        /// heroes deformed/invisible in play. Use the "(with mocap actions)" menu once the clips
        /// pass MocapClipSanity.
        /// </summary>
        public static bool IncludeMocapActions;

        /// <summary>Mocap locomotion clips (names of the baked <c>.anim</c>, see tools/mocap/clips.json).</summary>
        public const string MocapWalkClip = "Mocap_WalkKid";
        public const string MocapJogClip = "Mocap_Jog";
        public const string MocapRunClip = "Mocap_Run";
        public const string MocapIdleClip = "Mocap_Idle";

        private const string UseMocapLocomotionPrefKey = "Rapadura.HeroAnimators.MocapLocomotion";
        private const string UseMocapLocomotionMenu = "Rapadura/Animation/Use Mocap Locomotion";

        /// <summary>
        /// The single switch between CMU mocap locomotion (true, default) and the Meshy Walk/Run clips for the
        /// Humanoid heroes. Stored per machine (EditorPrefs); takes effect on the next Rebuild Hero Animators.
        /// </summary>
        public static bool UseMocapLocomotion
        {
            get => EditorPrefs.GetBool(UseMocapLocomotionPrefKey, true);
            set => EditorPrefs.SetBool(UseMocapLocomotionPrefKey, value);
        }

        [MenuItem(UseMocapLocomotionMenu, false, 40)]
        private static void ToggleMocapLocomotion()
        {
            UseMocapLocomotion = !UseMocapLocomotion;
            Debug.Log($"[HeroAnimatorRebuilder] Mocap locomotion {(UseMocapLocomotion ? "ON" : "OFF (Meshy Walk/Run)")} — run Rapadura/Animation/Rebuild Hero Animators to apply.");
        }

        [MenuItem(UseMocapLocomotionMenu, true)]
        private static bool ToggleMocapLocomotionValidate()
        {
            Menu.SetChecked(UseMocapLocomotionMenu, UseMocapLocomotion);
            return true;
        }

        [MenuItem("Rapadura/Animation/Rebuild Hero Animators")]
        public static void RebuildAllMenu()
        {
            IncludeMocapActions = false;
            string report = RebuildAll();
            Debug.Log(report);
            EditorUtility.DisplayDialog("Rebuild Hero Animators", report, "OK");
        }

        [MenuItem("Rapadura/Animation/Rebuild Hero Animators (with mocap actions)")]
        public static void RebuildAllWithActionsMenu()
        {
            IncludeMocapActions = true;
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
            AnimationClip mocapIdle = humanoid && IncludeMocapActions ? AssetDatabase.LoadAssetAtPath<AnimationClip>(MocapIdlePath) : null;

            // 2b) Mocap locomotion (Humanoid only): replaces Walk/Run, adds Jog and the Idle state.
            string locomotionNote = humanoid ? "Meshy locomotion (mocap switch off)" : "Meshy locomotion (Generic rig)";
            AnimationClip jog = null;
            float jogSpeed = 0f;
            if (humanoid && UseMocapLocomotion)
            {
                MocapLocomotion mocap = LoadMocapLocomotion(hero, walkPath, out locomotionNote);
                if (mocap != null)
                {
                    walk = mocap.Walk;
                    run = mocap.Run;
                    jog = mocap.Jog;
                    walkMeasure.GroundSpeed = mocap.WalkSpeed;
                    runMeasure.GroundSpeed = mocap.RunSpeed;
                    jogSpeed = mocap.JogSpeed;
                    walkName = walk.name;
                    runName = run.name;
                    if (mocap.Idle != null)
                    {
                        mocapIdle = mocap.Idle;
                    }
                }
            }

            // 3) Controller.
            RebuildController(controller, walk, run, jog, idlePose, mocapIdle, walkMeasure.GroundSpeed, runMeasure.GroundSpeed, jogSpeed, humanoid);

            // 4) One-shot mocap actions (attack/cast/hit/jump/death), see HeroAnimatorRebuilder.Actions.cs.
            string actionsReport = AddActions(controller, hero, walkPath, humanoid && IncludeMocapActions);
            AssetDatabase.SaveAssets();

            return $"{hero}: {locomotionNote}; Walk '{walkName}' {walkMeasure.GroundSpeed:0.###} u/s, "
                   + (jog != null ? $"Jog '{jog.name}' {jogSpeed:0.###} u/s, " : string.Empty)
                   + $"Run '{runName}' {runMeasure.GroundSpeed:0.###} u/s (model units, ×model scale at runtime), "
                   + $"idle pose @ {walkMeasure.StandingTime:0.###}s{(idlePose == null ? " (MISSING)" : string.Empty)}, "
                   + $"{(humanoid ? "Humanoid" : "Generic")}{(mocapIdle != null ? $", Idle state = {AssetDatabase.GetAssetPath(mocapIdle)}" : string.Empty)}; {actionsReport}.";
        }

        // ------------------------------------------------------------------
        // Mocap locomotion
        // ------------------------------------------------------------------

        private sealed class MocapLocomotion
        {
            public AnimationClip Walk, Jog, Run, Idle;
            public float WalkSpeed, JogSpeed, RunSpeed;
        }

        /// <summary>
        /// The hero's mocap Walk/Jog/Run/Idle (own-avatar bake preferred), measured on <paramref name="modelPath"/>
        /// like the Meshy clips. Null (with the reason in <paramref name="note"/>) when Walk or Run is missing,
        /// unmeasurable or fails <see cref="MocapClipSanity"/>; a bad Jog/Idle is just left out.
        /// </summary>
        private static MocapLocomotion LoadMocapLocomotion(string hero, string modelPath, out string note)
        {
            AnimationClip walk = MocapClipBuilder.LoadForHero(hero, MocapWalkClip);
            AnimationClip run = MocapClipBuilder.LoadForHero(hero, MocapRunClip);
            if (walk == null || run == null)
            {
                note = $"Meshy locomotion ({MocapWalkClip}/{MocapRunClip} not baked — run Rapadura/Animation/Import CMU Mocap Set)";
                return null;
            }

            foreach (AnimationClip clip in new[] { walk, run })
            {
                if (!MocapClipSanity.IsHealthy(clip))
                {
                    note = $"Meshy locomotion ({AssetDatabase.GetAssetPath(clip)}{MocapClipSanity.Describe(clip)})";
                    return null;
                }
            }

            var result = new MocapLocomotion
            {
                Walk = walk,
                Run = run,
                WalkSpeed = Measure(walk, modelPath).GroundSpeed,
                RunSpeed = Measure(run, modelPath).GroundSpeed,
            };
            if (result.WalkSpeed <= 0f || result.RunSpeed <= 0f)
            {
                note = "Meshy locomotion (mocap stride could not be measured)";
                return null;
            }

            AnimationClip jog = MocapClipBuilder.LoadForHero(hero, MocapJogClip);
            if (MocapClipSanity.IsHealthy(jog))
            {
                result.JogSpeed = Measure(jog, modelPath).GroundSpeed;
                result.Jog = result.JogSpeed > 0f ? jog : null;
            }

            AnimationClip idle = MocapClipBuilder.LoadForHero(hero, MocapIdleClip);
            result.Idle = MocapClipSanity.IsHealthy(idle) ? idle : null;

            bool own = AssetDatabase.GetAssetPath(walk).StartsWith(MocapClipBuilder.HeroFolder(hero) + "/", StringComparison.Ordinal);
            note = $"Mocap locomotion ({(own ? "baked on " + hero : "shared Ícaro bake")})";
            return result;
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
                            // Only the hero's own FBX clips: the mocap .anim children are re-chosen by LoadMocapLocomotion.
                            if (motion.motion is AnimationClip clip && IsModelClip(clip))
                            {
                                if (Mathf.Approximately(motion.threshold, 0.5f)) walk = clip;
                                else if (Mathf.Approximately(motion.threshold, 1f)) run = clip;
                            }
                        }
                    }
                    else if (state.motion is AnimationClip clip && IsModelClip(clip))
                    {
                        if (state.name == "Walk") walk = clip;
                        else if (state.name == "Run") run = clip;
                    }
                }
            }

            walk = walk != null ? walk : FirstClip($"{folder}/{hero}_Walk_1.fbx") ?? FirstClip($"{folder}/{hero}_Walk.fbx");
            run = run != null ? run : FirstClip($"{folder}/{hero}_Run_1.fbx") ?? FirstClip($"{folder}/{hero}_Run.fbx");
        }

        private static bool IsModelClip(AnimationClip clip)
        {
            return AssetDatabase.GetAssetPath(clip).EndsWith(".fbx", StringComparison.OrdinalIgnoreCase);
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

        /// <summary>Locomotion → Jump: airborne going up faster than this (m/s).</summary>
        private const float JumpEnterSpeed = 1f;
        /// <summary>Locomotion → Fall: airborne and already falling faster than this (m/s) — about 0.1 s off a ledge, so slopes/steps that unground for a frame or two don't flicker into Fall.</summary>
        private const float FallEnterSpeed = -4f;
        /// <summary>Fall/Jump → Land only for touchdowns faster than this (m/s); softer ones go straight back to Locomotion.</summary>
        private const float LandSpeed = -5f;
        /// <summary>Seconds the Land state holds before handing back to Locomotion (exit by time).</summary>
        private const float LandHoldSeconds = 0.18f;

        /// <summary>
        /// Base-layer airborne chain, driven only by parameters (no Play() from code): Jump (takeoff) →
        /// Fall (loop) → Land (short, exit by time) → Locomotion; Land only when the touchdown was hard
        /// (VerticalSpeed holds the touchdown speed for a moment after IsGrounded flips). No dedicated
        /// clips exist yet, so the three states hold <paramref name="placeholder"/> (the idle pose) and
        /// <see cref="PlayerProceduralAnimator"/>/<see cref="PlayerFootIK"/> do the crouch, tuck, arc
        /// and landing squash; a real clip can be dropped into a state's Motion later without touching
        /// any code. Timings:
        /// <list type="bullet">
        /// <item>into Jump 0.08 s, into Fall 0.15 s, into Land 0.08 s, Land → Locomotion 0.15 s, soft
        /// touchdown → Locomotion 0.12 s; all fixed-duration, no exit time except Land.</item>
        /// <item>Interruption source: ground → Jump/Fall "Destination" (next state) (a jump that lands at once can leave
        /// through Jump's own exits mid-blend), Jump → Fall "Source" (current state) (re-grounding mid-blend wins) —
        /// input never waits for a blend to finish.</item>
        /// </list>
        /// </summary>
        private static void AddAirborneStates(AnimatorStateMachine machine, AnimatorState locomotion, AnimatorState idle, AnimationClip placeholder)
        {
            AnimatorState jump = machine.AddState(PlayerAnimatorHashes.JumpStateName, new Vector3(600f, 0f, 0f));
            AnimatorState fall = machine.AddState(PlayerAnimatorHashes.FallStateName, new Vector3(600f, 100f, 0f));
            AnimatorState land = machine.AddState(PlayerAnimatorHashes.LandStateName, new Vector3(600f, 200f, 0f));
            foreach (AnimatorState state in new[] { jump, fall, land })
            {
                state.motion = placeholder;
                state.writeDefaultValues = true;
            }

            // A 1-frame placeholder would end at once: stretch it to the Land hold.
            land.speed = placeholder != null && placeholder.length > 0f ? Mathf.Clamp(placeholder.length / LandHoldSeconds, 0.01f, 1f) : 1f;

            foreach (AnimatorState ground in new[] { locomotion, idle })
            {
                if (ground == null)
                {
                    continue;
                }

                AnimatorStateTransition toJump = Transition(ground, jump, 0.08f, TransitionInterruptionSource.Destination);
                toJump.AddCondition(AnimatorConditionMode.IfNot, 0f, PlayerAnimatorHashes.IsGroundedName);
                toJump.AddCondition(AnimatorConditionMode.Greater, JumpEnterSpeed, PlayerAnimatorHashes.VerticalSpeedName);

                AnimatorStateTransition toFall = Transition(ground, fall, 0.15f, TransitionInterruptionSource.Destination);
                toFall.AddCondition(AnimatorConditionMode.IfNot, 0f, PlayerAnimatorHashes.IsGroundedName);
                toFall.AddCondition(AnimatorConditionMode.Less, FallEnterSpeed, PlayerAnimatorHashes.VerticalSpeedName);
            }

            AnimatorStateTransition apex = Transition(jump, fall, 0.15f, TransitionInterruptionSource.Source);
            apex.AddCondition(AnimatorConditionMode.Less, 0f, PlayerAnimatorHashes.VerticalSpeedName);

            foreach (AnimatorState air in new[] { jump, fall })
            {
                // Order matters: the hard-landing check first, the soft one second (mutually exclusive anyway).
                AnimatorStateTransition hard = Transition(air, land, 0.08f, TransitionInterruptionSource.None);
                hard.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.IsGroundedName);
                hard.AddCondition(AnimatorConditionMode.Less, LandSpeed, PlayerAnimatorHashes.VerticalSpeedName);

                AnimatorStateTransition soft = Transition(air, locomotion, 0.12f, TransitionInterruptionSource.None);
                soft.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.IsGroundedName);
                soft.AddCondition(AnimatorConditionMode.Greater, LandSpeed, PlayerAnimatorHashes.VerticalSpeedName);
            }

            AnimatorStateTransition recover = land.AddTransition(locomotion);
            recover.hasExitTime = true;
            recover.exitTime = 0.9f;
            recover.hasFixedDuration = true;
            recover.duration = 0.15f;

            // Leaving the ground again during Land (a quick re-jump) must not wait for the exit time.
            AnimatorStateTransition rejump = Transition(land, jump, 0.08f, TransitionInterruptionSource.None);
            rejump.AddCondition(AnimatorConditionMode.IfNot, 0f, PlayerAnimatorHashes.IsGroundedName);
            rejump.AddCondition(AnimatorConditionMode.Greater, JumpEnterSpeed, PlayerAnimatorHashes.VerticalSpeedName);
        }

        private static AnimatorStateTransition Transition(AnimatorState from, AnimatorState to, float duration, TransitionInterruptionSource interruption)
        {
            AnimatorStateTransition transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.interruptionSource = interruption;
            transition.orderedInterruption = true;
            return transition;
        }

        private static void RebuildController(AnimatorController controller, AnimationClip walk, AnimationClip run, AnimationClip jog, AnimationClip idlePose, AnimationClip mocapIdle,
                                              float walkClipSpeed, float runClipSpeed, float jogClipSpeed, bool humanoid)
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
            if (jog != null)
            {
                controller.AddParameter(PlayerAnimatorHashes.JogClipSpeedName, AnimatorControllerParameterType.Float);
            }
            controller.AddParameter(PlayerAnimatorHashes.TurnAngleName, AnimatorControllerParameterType.Float);
            controller.AddParameter(PlayerAnimatorHashes.IsGroundedName, AnimatorControllerParameterType.Bool);
            controller.AddParameter(PlayerAnimatorHashes.VerticalSpeedName, AnimatorControllerParameterType.Float);

            AnimatorControllerParameter[] parameters = controller.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.name == PlayerAnimatorHashes.MoveSpeedMultiplierName) parameter.defaultFloat = 1f;
                else if (parameter.name == PlayerAnimatorHashes.WalkClipSpeedName) parameter.defaultFloat = walkClipSpeed;
                else if (parameter.name == PlayerAnimatorHashes.RunClipSpeedName) parameter.defaultFloat = runClipSpeed;
                else if (parameter.name == PlayerAnimatorHashes.JogClipSpeedName) parameter.defaultFloat = jogClipSpeed;
                else if (parameter.name == PlayerAnimatorHashes.IsGroundedName) parameter.defaultBool = true;
            }

            controller.parameters = parameters;

            if (controller.layers.Length == 0)
            {
                controller.AddLayer("Base Layer");
            }

            // IK Pass on the base layer for Humanoid rigs: PlayerFootIK plants the feet in OnAnimatorIK
            // (never called without it). Generic rigs (Maria) have no humanoid IK goals.
            AnimatorControllerLayer[] layers = controller.layers;
            layers[0].iKPass = humanoid;
            controller.layers = layers;

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
            if (jog != null)
            {
                tree.AddChild(jog, LocomotionMath.JogBlend);
            }

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

                // Between locomotion states: 0.2-0.25 s (settling into idle is slower than leaving it).
                AnimatorStateTransition toIdle = locomotion.AddTransition(idle);
                toIdle.hasExitTime = false;
                toIdle.hasFixedDuration = true;
                toIdle.duration = 0.25f;
                toIdle.AddCondition(AnimatorConditionMode.Less, IdleEnterBlend, PlayerAnimatorHashes.MoveBlendName);
                toIdle.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.IsGroundedName);

                AnimatorStateTransition toLocomotion = idle.AddTransition(locomotion);
                toLocomotion.hasExitTime = false;
                toLocomotion.hasFixedDuration = true;
                toLocomotion.duration = 0.2f;
                toLocomotion.AddCondition(AnimatorConditionMode.Greater, IdleExitBlend, PlayerAnimatorHashes.MoveBlendName);
            }

            AddAirborneStates(machine, locomotion, machine.defaultState != locomotion ? machine.defaultState : null, idlePose != null ? idlePose : walk);

            EditorUtility.SetDirty(tree);
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
        }
    }
}
