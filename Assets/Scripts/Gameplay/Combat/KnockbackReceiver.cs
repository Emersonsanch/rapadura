using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Generic knockback executor. Works with either a <see cref="CharacterController"/>
    /// (the player, and any NPC reusing <c>PlayerMotor</c>-style movement) or a
    /// <see cref="Rigidbody"/> (ragdoll-ish enemies/props) — whichever is found on the object is
    /// used, so this single component can be dropped on any combat entity without extra wiring.
    /// Deliberately does not know about <see cref="Hitbox"/>/<see cref="DamageInfo"/> directly;
    /// it only exposes <see cref="ApplyKnockback"/> so it can be triggered by combat, explosions,
    /// wind, etc.
    /// </summary>
    public class KnockbackReceiver : MonoBehaviour
    {
        [SerializeField] private float _drag = 8f;

        private CharacterController _characterController;
        private Rigidbody _rigidbody;
        private Vector3 _velocity;

        public Vector3 CurrentVelocity => _velocity;
        public bool IsBeingKnockedBack => _velocity.sqrMagnitude > 0.0001f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (_rigidbody != null && !_rigidbody.isKinematic)
            {
                // Rigidbody path drives itself through physics forces; nothing to integrate here.
                return;
            }

            if (_velocity.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            if (_characterController != null && _characterController.enabled)
            {
                _characterController.Move(_velocity * Time.deltaTime);
            }
            else
            {
                transform.position += _velocity * Time.deltaTime;
            }

            _velocity = Vector3.MoveTowards(_velocity, Vector3.zero, _drag * Time.deltaTime);
        }

        /// <summary>Applies an instantaneous push in <paramref name="direction"/> scaled by <paramref name="force"/>.</summary>
        public void ApplyKnockback(Vector3 direction, float force)
        {
            if (force <= 0f || direction.sqrMagnitude <= 0f)
            {
                return;
            }

            if (_rigidbody != null && !_rigidbody.isKinematic)
            {
                _rigidbody.AddForce(direction.normalized * force, ForceMode.Impulse);
                return;
            }

            _velocity = direction.normalized * force;
        }
    }
}
