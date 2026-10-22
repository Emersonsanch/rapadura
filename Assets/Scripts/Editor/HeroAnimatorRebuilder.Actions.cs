using System.Linq;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Player.States;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Editor
{
    /// <summary>
    /// Second half of <see cref="HeroAnimatorRebuilder"/>: the one-shot mocap actions. For humanoid
    /// heroes, adds
    /// <list type="bullet">
    /// <item>an "Actions" layer (upper-body avatar mask, so legs keep walking) with AttackA/AttackB
    ///   (sword/punch, alternated by <c>AttackVariant</c>), Cast and HitReact, entered from Any State
    ///   on the <c>Attack</c>/<c>Cast</c>/<c>Hit</c> triggers and leaving to an empty state after the
    ///   move's peak + a short recovery (so a 2 s mocap follow-through never blocks a 0.5 s attack);</item>
    /// <item>on the base layer, a Jump state (played by <c>PlayerJumpState</c>) that returns to
    ///   Locomotion, and Death/DeathBack (forward/backward, picked by <c>DeathVariant</c>) entered on
    ///   <c>Die</c> and left on <c>Revive</c>;</item>
    /// <item>each clip sped up so its measured peak (fastest hand) lands at
    ///   <see cref="HeroActionTiming.StrikeDelay"/>/<see cref="HeroActionTiming.CastReleaseDelay"/>,
    ///   and the real strike time baked into the <c>AttackStrikeTime</c> float that
    ///   <c>PlayerBasicAttack</c> waits for before landing damage.</item>
    /// </list>
    /// Clips come from <c>Assets/Animations/Mocap/Mocap_*.anim</c> (see Docs/Mocap.md); a missing
    /// clip simply skips its state, and the runtime (<c>PlayerActionAnimator</c>) checks parameters
    /// before firing anything.
    /// </summary>
    public static partial class HeroAnimatorRebuilder
    {
        private const string MocapFolder = "Assets/Animations/Mocap";
        private const string MaskPath = "Assets/Animations/Masks/HeroUpperBody.mask";
        private const string NoneStateName = "None";
        private const int ActionSamples = 60;

        /// <summary>Hero-independent: which mocap clip each slot uses.</summary>
        private static readonly (string state, string clip)[] AttackClips =
        {
            ("AttackA", "Mocap_AttackSword"),
            ("AttackB", "Mocap_AttackPunch"),
        };

        private struct ActionTiming
        {
            public float PeakTime;
            public float Speed;
            public float ExitTime;
        }

        private static string AddActions(AnimatorController controller, string hero, string modelPath, bool humanoid)
        {
            // Always clear a previous Actions layer so the rebuild is idempotent.
            while (controller.layers.Length > 1)
            {
                controller.RemoveLayer(controller.layers.Length - 1);
            }

            if (!humanoid)
            {
                return IncludeMocapActions ? "no mocap actions (Generic rig)" : "no mocap actions (off; use the \"with mocap actions\" menu)";
            }

            AnimationClip attackA = LoadMocap(AttackClips[0].clip);
            AnimationClip attackB = LoadMocap(AttackClips[1].clip);
            AnimationClip cast = LoadMocap("Mocap_Cast");
            AnimationClip hit = LoadMocap("Mocap_HitReact");
            AnimationClip jump = LoadMocap("Mocap_Jump");
            AnimationClip deathForward = LoadMocap("Mocap_DeathForward");
            AnimationClip deathBackward = LoadMocap("Mocap_DeathBackward");

            if (attackA == null && cast == null && hit == null && jump == null && deathForward == null)
            {
                return "no mocap actions (no Mocap_*.anim clips; run Rapadura/Animation/Import CMU Mocap Set)";
            }

            AddTriggerIfMissing(controller, PlayerAnimatorHashes.AttackName);
            AddIntIfMissing(controller, PlayerAnimatorHashes.AttackVariantName);
            AddTriggerIfMissing(controller, PlayerAnimatorHashes.CastName);
            AddTriggerIfMissing(controller, PlayerAnimatorHashes.HitName);
            AddTriggerIfMissing(controller, PlayerAnimatorHashes.DieName);
            AddTriggerIfMissing(controller, PlayerAnimatorHashes.ReviveName);
            AddIntIfMissing(controller, PlayerAnimatorHashes.DeathVariantName);
            AddFloatIfMissing(controller, PlayerAnimatorHashes.AttackStrikeTimeName, HeroActionTiming.StrikeDelay);
            AddFloatIfMissing(controller, PlayerAnimatorHashes.AttackStrikeTimeBName, HeroActionTiming.StrikeDelay);

            var report = new System.Text.StringBuilder();

            // ---------------- Actions layer (upper body)
            controller.AddLayer(PlayerAnimatorHashes.ActionsLayerName);
            AnimatorControllerLayer[] layers = controller.layers;
            AnimatorControllerLayer actions = layers[layers.Length - 1];
            actions.avatarMask = LoadOrCreateUpperBodyMask();
            actions.defaultWeight = 1f;
            actions.blendingMode = AnimatorLayerBlendingMode.Override;
            controller.layers = layers;

            AnimatorStateMachine machine = controller.layers[layers.Length - 1].stateMachine;
            // Empty state + Write Defaults OFF everywhere on this layer: with Write Defaults on, an
            // empty state writes the bind pose into the masked bones (T-posed arms while idle).
            AnimatorState none = machine.AddState(NoneStateName, new Vector3(300f, 0f, 0f));
            none.writeDefaultValues = false;
            machine.defaultState = none;

            float bakedStrike = HeroActionTiming.StrikeDelay;
            float bakedStrikeB = HeroActionTiming.StrikeDelay;
            int row = 1;
            for (int i = 0; i < AttackClips.Length; i++)
            {
                AnimationClip clip = i == 0 ? attackA : attackB;
                if (clip == null)
                {
                    continue;
                }

                ActionTiming timing = MeasureAction(clip, modelPath, HeroActionTiming.StrikeDelay, HumanBodyBones.RightHand, HumanBodyBones.LeftHand);
                AnimatorState state = AddOneShot(machine, none, AttackClips[i].state, clip, timing, new Vector3(600f, row++ * 80f, 0f));
                AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
                ConfigureEnter(enter, 0.04f, canSelf: true);
                enter.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.AttackName);
                enter.AddCondition(AnimatorConditionMode.Equals, i, PlayerAnimatorHashes.AttackVariantName);
                float real = HeroActionTiming.RealPeakTime(timing.PeakTime, timing.Speed);
                if (real > 0f)
                {
                    if (i == 0) bakedStrike = real;
                    else bakedStrikeB = real;
                }

                report.Append($"{AttackClips[i].state} peak {timing.PeakTime:0.00}s x{timing.Speed:0.0} → hit @ {real:0.00}s; ");
            }

            SetFloatDefault(controller, PlayerAnimatorHashes.AttackStrikeTimeName, bakedStrike);
            SetFloatDefault(controller, PlayerAnimatorHashes.AttackStrikeTimeBName, bakedStrikeB);

            if (cast != null)
            {
                ActionTiming timing = MeasureAction(cast, modelPath, HeroActionTiming.CastReleaseDelay, HumanBodyBones.RightHand, HumanBodyBones.LeftHand);
                AnimatorState state = AddOneShot(machine, none, "Cast", cast, timing, new Vector3(600f, row++ * 80f, 0f));
                AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
                ConfigureEnter(enter, 0.05f, canSelf: true);
                enter.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.CastName);
                report.Append($"Cast peak {timing.PeakTime:0.00}s x{timing.Speed:0.0}; ");
            }

            if (hit != null)
            {
                var timing = new ActionTiming { PeakTime = 0f, Speed = 1.6f, ExitTime = Mathf.Clamp(0.9f / Mathf.Max(0.1f, hit.length), 0.2f, 1f) };
                AnimatorState state = AddOneShot(machine, none, "HitReact", hit, timing, new Vector3(600f, row++ * 80f, 0f));
                AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
                ConfigureEnter(enter, 0.03f, canSelf: false);
                enter.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.HitName);
            }

            // ---------------- Base layer: Jump, Death
            AnimatorStateMachine baseMachine = controller.layers[0].stateMachine;
            AnimatorState locomotion = FindState(baseMachine, LocomotionStateName);
            AnimatorState idle = FindState(baseMachine, IdleStateName) ?? locomotion;

            if (jump != null && locomotion != null)
            {
                AnimatorState jumpState = baseMachine.AddState(PlayerAnimatorHashes.JumpStateName, new Vector3(600f, 100f, 0f));
                jumpState.motion = jump;
                jumpState.speed = Mathf.Clamp(jump.length / 0.9f, 1f, 3f);
                jumpState.writeDefaultValues = true;
                AnimatorStateTransition land = jumpState.AddTransition(locomotion);
                land.hasExitTime = true;
                land.exitTime = 0.95f;
                land.hasFixedDuration = true;
                land.duration = 0.15f;
            }

            if (deathForward != null && idle != null)
            {
                AddDeath(baseMachine, idle, PlayerAnimatorHashes.DeathStateName, deathForward, 0, new Vector3(600f, 200f, 0f));
                AddDeath(baseMachine, idle, PlayerAnimatorHashes.DeathStateName + "Back", deathBackward != null ? deathBackward : deathForward, 1, new Vector3(600f, 280f, 0f));
            }

            EditorUtility.SetDirty(controller);
            return "actions: " + (report.Length > 0 ? report.ToString().TrimEnd(' ', ';') : "none measured")
                   + (jump != null ? "; Jump" : string.Empty) + (deathForward != null ? "; Death" : string.Empty);
        }

        // ------------------------------------------------------------------

        private static AnimationClip LoadMocap(string name)
        {
            return AssetDatabase.LoadAssetAtPath<AnimationClip>($"{MocapFolder}/{name}.anim");
        }

        private static AnimatorState FindState(AnimatorStateMachine machine, string name)
        {
            return machine.states.Select(c => c.state).FirstOrDefault(s => s != null && s.name == name);
        }

        private static AnimatorState AddOneShot(AnimatorStateMachine machine, AnimatorState none, string name, AnimationClip clip, ActionTiming timing, Vector3 position)
        {
            AnimatorState state = machine.AddState(name, position);
            state.motion = clip;
            state.speed = timing.Speed;
            state.writeDefaultValues = false;

            AnimatorStateTransition exit = state.AddTransition(none);
            exit.hasExitTime = true;
            exit.exitTime = timing.ExitTime;
            exit.hasFixedDuration = true;
            exit.duration = 0.2f;
            exit.interruptionSource = TransitionInterruptionSource.None;
            return state;
        }

        private static void ConfigureEnter(AnimatorStateTransition transition, float duration, bool canSelf)
        {
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = duration;
            transition.canTransitionToSelf = canSelf;
            transition.interruptionSource = TransitionInterruptionSource.None;
        }

        private static void AddDeath(AnimatorStateMachine machine, AnimatorState idle, string name, AnimationClip clip, int variant, Vector3 position)
        {
            AnimatorState death = machine.AddState(name, position);
            death.motion = clip;
            death.speed = 1.15f;
            death.writeDefaultValues = true;

            AnimatorStateTransition die = machine.AddAnyStateTransition(death);
            ConfigureEnter(die, 0.1f, canSelf: false);
            die.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.DieName);
            die.AddCondition(AnimatorConditionMode.Equals, variant, PlayerAnimatorHashes.DeathVariantName);

            AnimatorStateTransition revive = death.AddTransition(idle);
            revive.hasExitTime = false;
            revive.hasFixedDuration = true;
            revive.duration = 0.3f;
            revive.AddCondition(AnimatorConditionMode.If, 0f, PlayerAnimatorHashes.ReviveName);
        }

        private static void AddTriggerIfMissing(AnimatorController controller, string name)
        {
            if (controller.parameters.All(p => p.name != name))
            {
                controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
            }
        }

        private static void AddIntIfMissing(AnimatorController controller, string name)
        {
            if (controller.parameters.All(p => p.name != name))
            {
                controller.AddParameter(name, AnimatorControllerParameterType.Int);
            }
        }

        private static void AddFloatIfMissing(AnimatorController controller, string name, float defaultValue)
        {
            if (controller.parameters.All(p => p.name != name))
            {
                controller.AddParameter(name, AnimatorControllerParameterType.Float);
            }

            SetFloatDefault(controller, name, defaultValue);
        }

        private static void SetFloatDefault(AnimatorController controller, string name, float value)
        {
            AnimatorControllerParameter[] parameters = controller.parameters;
            foreach (AnimatorControllerParameter parameter in parameters)
            {
                if (parameter.name == name)
                {
                    parameter.defaultFloat = value;
                }
            }

            controller.parameters = parameters;
        }

        private static AvatarMask LoadOrCreateUpperBodyMask()
        {
            var mask = AssetDatabase.LoadAssetAtPath<AvatarMask>(MaskPath);
            if (mask != null)
            {
                return mask;
            }

            string folder = System.IO.Path.GetDirectoryName(MaskPath)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/'), System.IO.Path.GetFileName(folder));
            }

            mask = new AvatarMask();
            for (int i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
            {
                var part = (AvatarMaskBodyPart)i;
                bool upper = part == AvatarMaskBodyPart.Body || part == AvatarMaskBodyPart.Head
                             || part == AvatarMaskBodyPart.LeftArm || part == AvatarMaskBodyPart.RightArm
                             || part == AvatarMaskBodyPart.LeftFingers || part == AvatarMaskBodyPart.RightFingers;
                mask.SetHumanoidBodyPartActive(part, upper);
            }

            AssetDatabase.CreateAsset(mask, MaskPath);
            return mask;
        }

        /// <summary>
        /// Samples the clip on the hero model and finds when the tracked hands move fastest (relative
        /// to the hips), then derives playback speed and exit time from <see cref="HeroActionTiming"/>.
        /// Falls back to "peak at 40% of the clip" when the model can't be sampled.
        /// </summary>
        private static ActionTiming MeasureAction(AnimationClip clip, string modelPath, float targetDelay, HumanBodyBones primary, HumanBodyBones secondary)
        {
            float peak = clip.length * 0.4f;
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
            if (model != null && clip.length > 0f)
            {
                GameObject instance = Object.Instantiate(model);
                instance.hideFlags = HideFlags.DontSave;
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                try
                {
                    Animator animator = instance.GetComponentInChildren<Animator>();
                    Transform root = animator != null ? animator.transform : instance.transform;
                    Transform hips = Bone(animator, root, HumanBodyBones.Hips, "Hips");
                    Transform handA = Bone(animator, root, primary, primary == HumanBodyBones.RightHand ? "RightHand" : "LeftHand");
                    Transform handB = Bone(animator, root, secondary, secondary == HumanBodyBones.RightHand ? "RightHand" : "LeftHand");
                    if (hips != null && (handA != null || handB != null))
                    {
                        var samples = new Vector3[ActionSamples + 1];
                        float interval = clip.length / ActionSamples;
                        AnimationMode.StartAnimationMode();
                        try
                        {
                            for (int i = 0; i <= ActionSamples; i++)
                            {
                                AnimationMode.BeginSampling();
                                AnimationMode.SampleAnimationClip(instance, clip, i * interval);
                                AnimationMode.EndSampling();
                                Vector3 hipsLocal = root.InverseTransformPoint(hips.position);
                                Vector3 a = handA != null ? root.InverseTransformPoint(handA.position) - hipsLocal : Vector3.zero;
                                Vector3 b = handB != null ? root.InverseTransformPoint(handB.position) - hipsLocal : Vector3.zero;
                                // Track whichever hand travels more over the clip by summing both; the
                                // faster one dominates the magnitude.
                                samples[i] = a + b;
                            }
                        }
                        finally
                        {
                            AnimationMode.StopAnimationMode();
                        }

                        int index = HeroActionTiming.PeakSpeedSample(samples, interval);
                        if (index > 0)
                        {
                            peak = index * interval;
                        }
                    }
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }

            float speed = HeroActionTiming.PlaybackSpeedForPeak(peak, targetDelay);
            return new ActionTiming
            {
                PeakTime = peak,
                Speed = speed,
                ExitTime = HeroActionTiming.ExitTime(clip.length, peak, speed),
            };
        }
    }
}
