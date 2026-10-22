using UnityEngine;

namespace Rapadura.Gameplay.Player.States
{
    /// <summary>Cached Animator parameter hashes shared by every player state, avoiding string lookups per frame.</summary>
    public static class PlayerAnimatorHashes
    {
        public static readonly int Idle = Animator.StringToHash("Idle");
        public static readonly int Walk = Animator.StringToHash("Walk");
        public static readonly int Run = Animator.StringToHash("Run");
        public static readonly int Jump = Animator.StringToHash("Jump");
        public static readonly int Fall = Animator.StringToHash("Fall");
        public static readonly int Slide = Animator.StringToHash("Slide");
        public static readonly int Crouch = Animator.StringToHash("Crouch");
        public static readonly int Swim = Animator.StringToHash("Swim");
        public static readonly int Climb = Animator.StringToHash("Climb");

        public const string SpeedName = "Speed";
        public const string MoveBlendName = "MoveBlend";
        public const string MoveSpeedMultiplierName = "MoveSpeedMul";
        public const string WalkClipSpeedName = "WalkClipSpeed";
        public const string RunClipSpeedName = "RunClipSpeed";

        // One-shot action clips (mocap), built by Rapadura/Animation/Rebuild Hero Animators.
        public const string AttackName = "Attack";
        public const string AttackVariantName = "AttackVariant";
        public const string CastName = "Cast";
        public const string HitName = "Hit";
        public const string DieName = "Die";
        public const string ReviveName = "Revive";
        public const string DeathVariantName = "DeathVariant";
        public const string AttackStrikeTimeName = "AttackStrikeTime";
        public const string AttackStrikeTimeBName = "AttackStrikeTimeB";
        public const string ActionsLayerName = "Actions";
        public const string DeathStateName = "Death";
        public const string JumpStateName = "Jump";

        /// <summary>Trigger: play a basic-attack swing (<see cref="AttackVariant"/> picks the clip, alternating).</summary>
        public static readonly int Attack = Animator.StringToHash(AttackName);
        public static readonly int AttackVariant = Animator.StringToHash(AttackVariantName);
        /// <summary>Trigger: skill cast pose/release.</summary>
        public static readonly int Cast = Animator.StringToHash(CastName);
        /// <summary>Trigger: hit reaction (upper body).</summary>
        public static readonly int Hit = Animator.StringToHash(HitName);
        /// <summary>Triggers: full-body death clip and the way back to Idle on respawn.</summary>
        public static readonly int Die = Animator.StringToHash(DieName);
        public static readonly int Revive = Animator.StringToHash(ReviveName);
        public static readonly int DeathVariant = Animator.StringToHash(DeathVariantName);
        /// <summary>Read-only "data" parameter: real-time seconds from the Attack trigger to the swing's fastest hand.</summary>
        public static readonly int AttackStrikeTime = Animator.StringToHash(AttackStrikeTimeName);
        /// <summary>Same for the second attack variant (punch); falls back to <see cref="AttackStrikeTime"/> when absent.</summary>
        public static readonly int AttackStrikeTimeB = Animator.StringToHash(AttackStrikeTimeBName);

        /// <summary>Legacy float: raw horizontal speed in m/s (old 3-state controllers transition on it).</summary>
        public static readonly int Speed = Animator.StringToHash(SpeedName);

        /// <summary>Normalized locomotion blend (0 idle, 0.5 walk, 1 run) feeding the "Locomotion" 1D blend tree.</summary>
        public static readonly int MoveBlend = Animator.StringToHash(MoveBlendName);

        /// <summary>Locomotion state speed multiplier (stride matching), bound as the state's speed parameter.</summary>
        public static readonly int MoveSpeedMultiplier = Animator.StringToHash(MoveSpeedMultiplierName);

        /// <summary>Read-only "data" parameters baked by the Rebuild Hero Animators editor tool: each clip's ground speed in model units/s at playback 1.</summary>
        public static readonly int WalkClipSpeed = Animator.StringToHash(WalkClipSpeedName);

        /// <inheritdoc cref="WalkClipSpeed"/>
        public static readonly int RunClipSpeed = Animator.StringToHash(RunClipSpeedName);

        /// <summary>
        /// Plays <paramref name="stateHash"/> on the base layer only if the controller actually has
        /// that state. The hero controllers have no Jump/Fall/Slide/Crouch clips yet, and a blind
        /// Play() logged "State could not be found" every time (and a null Animator threw).
        /// </summary>
        private static readonly System.Collections.Generic.Dictionary<RuntimeAnimatorController, System.Collections.Generic.HashSet<int>> ParameterCache =
            new System.Collections.Generic.Dictionary<RuntimeAnimatorController, System.Collections.Generic.HashSet<int>>();

        /// <summary>
        /// Whether the animator's controller declares a parameter with this hash. Cached per
        /// controller (Animator.parameters allocates), so safe to call from event handlers. False
        /// for a null/controller-less Animator, so callers can simply skip the SetTrigger — a
        /// SetTrigger on a missing parameter logs a warning every call.
        /// </summary>
        public static bool HasParameter(Animator animator, int parameterHash)
        {
            if (animator == null)
            {
                return false;
            }

            RuntimeAnimatorController controller = animator.runtimeAnimatorController;
            if (controller == null)
            {
                return false;
            }

            if (!ParameterCache.TryGetValue(controller, out var hashes))
            {
                hashes = new System.Collections.Generic.HashSet<int>();
                AnimatorControllerParameter[] parameters = animator.parameters;
                for (int i = 0; i < parameters.Length; i++)
                {
                    hashes.Add(parameters[i].nameHash);
                }

                // An Animator that has not initialised yet can report no parameters at all:
                // never cache an empty list, re-read it next time instead.
                if (hashes.Count > 0)
                {
                    ParameterCache[controller] = hashes;
                }
            }

            return hashes.Contains(parameterHash);
        }

        /// <summary>Forgets cached parameter lists (after rebuilding controllers in the editor, or in tests).</summary>
        public static void ClearParameterCache() => ParameterCache.Clear();

        public static bool TryPlay(Animator animator, int stateHash)
        {
            if (animator == null || animator.runtimeAnimatorController == null || !animator.isActiveAndEnabled)
            {
                return false;
            }

            if (!animator.HasState(0, stateHash))
            {
                return false;
            }

            animator.Play(stateHash);
            return true;
        }
    }
}
