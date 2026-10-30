using UnityEngine;

namespace Rapadura.Gameplay.Environment
{
    /// <summary>
    /// Marks a hierarchy whose props must never get solid colliders from <see cref="SolidPropBinder"/>
    /// — e.g. the world-map dressing, where the hero token travels on straight lines between nodes
    /// (no CharacterController) and map trees must not swallow node clicks. Put it on the root;
    /// everything below is skipped. See Docs/Collision.md.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class NonSolidPropZone : MonoBehaviour
    {
        /// <summary>Adds the marker to <paramref name="root"/> if it doesn't carry one yet.</summary>
        public static void Ensure(GameObject root)
        {
            if (root != null && !root.TryGetComponent(out NonSolidPropZone _))
            {
                root.AddComponent<NonSolidPropZone>();
            }
        }
    }
}
