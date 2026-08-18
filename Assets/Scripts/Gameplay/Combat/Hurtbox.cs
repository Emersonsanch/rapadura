using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Marks a collider as a region that can receive damage on behalf of an owning
    /// <see cref="ICombatTarget"/> (e.g. a separate "head" or "weak point" collider on an enemy
    /// rig, distinct from the movement/physics collider). Deliberately dumb — it does not know
    /// what a <see cref="Hitbox"/> is; <see cref="Hitbox"/> looks for this component on whatever
    /// it hits, keeping the two decoupled and testable in isolation.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Hurtbox : MonoBehaviour
    {
        [Tooltip("Multiplies incoming damage before mitigation — e.g. > 1 for a critical weak point.")]
        [SerializeField] private float _damageMultiplier = 1f;

        [Tooltip("Optional explicit owner. If unset, the ICombatTarget is looked up on this object or its parents.")]
        [SerializeField] private Component _ownerOverride;

        private ICombatTarget _owner;

        public float DamageMultiplier => _damageMultiplier;

        public ICombatTarget Owner
        {
            get
            {
                if (_owner != null)
                {
                    return _owner;
                }

                if (_ownerOverride is ICombatTarget overrideTarget)
                {
                    _owner = overrideTarget;
                }
                else
                {
                    _owner = GetComponentInParent<ICombatTarget>();
                }

                return _owner;
            }
        }

        /// <summary>Convenience accessor used by combat/knockback code that wants the physical body to push.</summary>
        public GameObject OwnerGameObject => Owner?.Transform != null ? Owner.Transform.gameObject : gameObject;
    }
}
