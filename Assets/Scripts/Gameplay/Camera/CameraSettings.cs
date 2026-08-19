using UnityEngine;

namespace Rapadura.Gameplay.Cameras
{
    /// <summary>
    /// Tunable data for the MU Online-style fixed isometric camera (see <see cref="CameraController"/>).
    /// A <c>ScriptableObject</c> so designers can author/swap presets from the Inspector without
    /// touching code, same pattern as <c>DamageBalanceConfig</c>.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraSettings", menuName = "Rapadura/Camera Settings")]
    public class CameraSettings : ScriptableObject
    {
        [Header("Angle (MU Online reference: yaw ~45°, pitch 55-60°)")]
        public float cameraYaw = 45f;
        public float cameraPitch = 57.5f;

        [Header("Follow")]
        [Tooltip("SmoothDamp time in seconds — lower is snappier, higher is smoother/laggier.")]
        public float followSpeed = 0.15f;

        [Header("Zoom")]
        public float minZoom = 10f;
        public float maxZoom = 25f;
        public float defaultZoom = 15f;
        public float zoomSpeed = 8f;
        [Tooltip("SmoothDamp time in seconds for zoom distance changes.")]
        public float zoomSmoothTime = 0.15f;

        [Header("Collision")]
        public LayerMask collisionMask = ~0;
        public float collisionRadius = 0.3f;
        [Tooltip("SmoothDamp time in seconds when the camera pulls in/back out for collision.")]
        public float collisionSmoothTime = 0.08f;

        [Header("Optional Rotation (Q/E, 45° steps)")]
        public bool allowRotation = true;
        public float rotationStepDegrees = 45f;
        [Tooltip("SmoothDamp time in seconds for the Q/E snap-rotation.")]
        public float rotationSmoothTime = 0.2f;
    }
}
