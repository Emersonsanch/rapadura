using Rapadura.Core.Events;
using Rapadura.Gameplay.Skills;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// The safe-zone trigger of a walkable city (Docs/Cities.md §3.1): a box with the <see cref="CityId"/>, registered in
    /// <see cref="CitySafeZones"/> so <c>Health</c> (no damage, §3.3) and any combat code can read the zone of a position.
    /// When the player steps in from the wilds, <see cref="SafeZoneRules.ShouldClearDebuffsOnEntry"/> clears poison, slow
    /// and stun (§3.6) and <see cref="CityZoneChangedEvent"/> is published.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CitySafeZoneVolume : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [SerializeField] private string _cityId;

        private BoxCollider _box;

        public string CityId => _cityId;

        public void Configure(string cityId)
        {
            _cityId = cityId;
        }

        private void Awake()
        {
            _box = GetComponent<BoxCollider>();
            _box.isTrigger = true;
        }

        private void OnEnable() => CitySafeZones.Register(this);

        private void OnDisable() => CitySafeZones.Unregister(this);

        /// <summary>Point inside the box (world space, rotation respected).</summary>
        public bool Contains(Vector3 position)
        {
            if (_box == null)
            {
                _box = GetComponent<BoxCollider>();
            }

            Vector3 local = transform.InverseTransformPoint(position) - _box.center;
            Vector3 half = _box.size * 0.5f;
            return Mathf.Abs(local.x) <= half.x && Mathf.Abs(local.y) <= half.y && Mathf.Abs(local.z) <= half.z;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            if (SafeZoneRules.ShouldClearDebuffsOnEntry(ZoneKind.Wilds, ZoneKind.City))
            {
                BuffController buffs = other.GetComponentInParent<BuffController>();
                if (buffs != null)
                {
                    buffs.RemoveDebuffs();
                    buffs.ClearStun();
                }
            }

            EventBus.Publish(new CityZoneChangedEvent(other.gameObject, _cityId, ZoneKind.Wilds, ZoneKind.City));
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PlayerTag))
            {
                EventBus.Publish(new CityZoneChangedEvent(other.gameObject, _cityId, ZoneKind.City, ZoneKind.Wilds));
            }
        }
    }
}
