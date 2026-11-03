using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Areas
{
    /// <summary>
    /// A way out of a playable map (<see cref="MapExitSpec"/>): back to the island map, on foot into the neighbouring
    /// map, or the Cais da Barca. Arms only once the hero has stood outside it after <see cref="ArmDelaySeconds"/>, so
    /// arriving next to (or inside) an exit never bounces the hero straight out. <see cref="MapModeController"/> decides
    /// what happens (walk level, closed roads).
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MapExitTrigger : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        public const float ArmDelaySeconds = 1.2f;

        /// <summary>Off in tests that walk past the exits on purpose.</summary>
        public static bool Enabled { get; set; } = true;

        private float _armAt;
        private bool _armed;

        public MapExitSpec Spec { get; private set; }
        public bool PlayerInside { get; private set; }
        public bool IsArmed => _armed;

        public void Configure(MapExitSpec spec)
        {
            Spec = spec;
        }

        private void OnEnable()
        {
            _armAt = Time.time + ArmDelaySeconds;
            _armed = false;
        }

        private void Update()
        {
            if (!_armed && !PlayerInside && Time.time >= _armAt)
            {
                _armed = true;
            }
        }

        /// <summary>Disarms until the hero has left the trigger again (after being pushed back from a closed road).</summary>
        public void Rearm()
        {
            _armed = false;
            _armAt = Time.time + ArmDelaySeconds;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            PlayerInside = true;
            if (Enabled && _armed && Spec != null)
            {
                MapModeController.HandleExit(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PlayerTag))
            {
                PlayerInside = false;
            }
        }
    }
}
