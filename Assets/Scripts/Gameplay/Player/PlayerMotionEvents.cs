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
}
