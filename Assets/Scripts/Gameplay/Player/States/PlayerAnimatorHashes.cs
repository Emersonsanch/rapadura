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
