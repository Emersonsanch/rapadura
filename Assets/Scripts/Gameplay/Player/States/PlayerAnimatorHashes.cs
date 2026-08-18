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
    }
}
