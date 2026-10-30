using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// A way out of the walkable city: the gate at the end of a street (back to the map) or the Cais da Barca (back to
    /// the map with the Barcas de Luz window open). Ignores the first moments after arrival so walking in never bounces out.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CityExitTrigger : MonoBehaviour
    {
        private const string PlayerTag = "Player";
        public const float ArmDelaySeconds = 1.5f;

        /// <summary>Off in tests that walk past the exits on purpose.</summary>
        public static bool Enabled { get; set; } = true;

        [SerializeField] private CityExitKind _kind;
        [SerializeField] private string _exitId;

        private float _armedAt;

        public CityExitKind Kind => _kind;
        public string ExitId => _exitId;
        public bool PlayerInside { get; private set; }

        public void Configure(string exitId, CityExitKind kind)
        {
            _exitId = exitId;
            _kind = kind;
        }

        private void OnEnable()
        {
            _armedAt = Time.time + ArmDelaySeconds;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            PlayerInside = true;
            if (Enabled && Time.time >= _armedAt)
            {
                CityModeController.Leave(_kind);
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
