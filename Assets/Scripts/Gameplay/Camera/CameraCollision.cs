using UnityEngine;

namespace Rapadura.Gameplay.Cameras
{
    /// <summary>
    /// Spherecasts from the target toward the desired (uncollided) camera position and pulls the
    /// camera in when something blocks the view, smoothly returning to full distance once the
    /// obstruction clears. Kept as a small standalone helper (rather than inline in
    /// <see cref="CameraController"/>) so it's independently testable/tunable.
    /// </summary>
    public class CameraCollision
    {
        private readonly CameraSettings _settings;
        private float _currentDistance;
        private float _velocity;

        public CameraCollision(CameraSettings settings, float initialDistance)
        {
            _settings = settings;
            _currentDistance = initialDistance;
        }

        /// <summary>
        /// Given the target position and the desired camera direction/distance (post-zoom), returns
        /// the collision-safe distance, smoothly interpolated so pulling in/back out never pops.
        /// </summary>
        public float Resolve(Vector3 targetPosition, Vector3 cameraDirection, float desiredDistance, float deltaTime)
        {
            float allowedDistance = desiredDistance;

            if (Physics.SphereCast(targetPosition, _settings.collisionRadius, cameraDirection, out RaycastHit hit, desiredDistance, _settings.collisionMask, QueryTriggerInteraction.Ignore))
            {
                allowedDistance = Mathf.Max(hit.distance, _settings.minZoom * 0.25f);
            }

            _currentDistance = Mathf.SmoothDamp(_currentDistance, allowedDistance, ref _velocity, _settings.collisionSmoothTime);
            return _currentDistance;
        }
    }
}
