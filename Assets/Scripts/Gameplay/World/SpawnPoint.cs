using UnityEngine;

namespace Rapadura.Gameplay.World
{
    /// <summary>
    /// Marks a location in the scene as a valid player spawn/checkpoint. Place one at the
    /// scene's initial spawn and one at every checkpoint; <see cref="CheckpointManager"/>
    /// tracks which one was last activated.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _id = "spawn";
        [SerializeField] private bool _isInitialSpawn;

        public string Id => _id;
        public bool IsInitialSpawn => _isInitialSpawn;
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (Rapadura.Core.DI.ServiceLocator.TryGet(out CheckpointManager checkpointManager))
            {
                checkpointManager.ActivateCheckpoint(this);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = _isInitialSpawn ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        }
#endif
    }
}
