using Rapadura.Core.Events;
using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Raised by <see cref="PlayerProceduralAnimator"/> when the hero touches down hard enough to show a
    /// landing squash (<see cref="Intensity"/> ≥ <see cref="MinIntensity"/>). For a subtle landing SFX;
    /// nothing in gameplay depends on it.
    /// </summary>
    public readonly struct PlayerLandedEvent : IGameEvent
    {
        /// <summary>Landings softer than this (fraction of the full landing squash) raise nothing.</summary>
        public const float MinIntensity = 0.2f;

        public readonly GameObject Player;
        public readonly Vector3 Position;
        /// <summary>0..1: share of the hero's full landing squash (by fall speed).</summary>
        public readonly float Intensity;

        public PlayerLandedEvent(GameObject player, Vector3 position, float intensity)
        {
            Player = player;
            Position = position;
            Intensity = intensity;
        }
    }

    /// <summary>
    /// Raised by <see cref="PlayerFootsteps"/> when one of the hero's feet plants while walking/running:
    /// the footstep sound (<c>hero.step</c> via SfxEventRouter) and the dust puff
    /// (<see cref="PlayerFootstepDust"/>) listen to it. Timing comes from the plant phases baked into the
    /// controller (Rebuild Hero Animators), else from the animated feet, else from the rigid step phase.
    /// </summary>
    public readonly struct PlayerFootstepEvent : IGameEvent
    {
        public readonly GameObject Player;
        /// <summary>World position of the planted foot (or of the hero for rig-less bodies).</summary>
        public readonly Vector3 Position;
        public readonly bool LeftFoot;
        /// <summary>0..1: soft walking steps, full at run speed (<see cref="FootstepMath.Intensity"/>).</summary>
        public readonly float Intensity;

        public PlayerFootstepEvent(GameObject player, Vector3 position, bool leftFoot, float intensity)
        {
            Player = player;
            Position = position;
            LeftFoot = leftFoot;
            Intensity = intensity;
        }
    }
}
