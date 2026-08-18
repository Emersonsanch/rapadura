using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Free-look third-person camera. Orbits around a pivot point (the player's shoulder height)
    /// based on look input, with configurable sensitivity and pitch clamping. Handles its own
    /// collision so the camera never clips through world geometry. Supports zoom via mouse
    /// scroll wheel on desktop and two-finger pinch on touch devices, plus an impact-shake hook
    /// for combat/game-feel.
    /// </summary>
    public class PlayerCamera : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _pivot;

        [Header("Sensitivity")]
        [SerializeField] private float _mouseSensitivity = 2.5f;
        [SerializeField] private float _touchSensitivity = 0.15f;
        [SerializeField] private float _minPitch = -40f;
        [SerializeField] private float _maxPitch = 75f;

        [Header("Distance")]
        [SerializeField] private float _distance = 4.5f;
        [SerializeField] private float _minDistance = 1.2f;
        [SerializeField] private float _maxDistance = 8f;
        [SerializeField] private LayerMask _collisionMask = ~0;
        [SerializeField] private float _collisionRadius = 0.25f;

        [Header("Zoom")]
        [SerializeField] private float _scrollZoomSpeed = 1.5f;
        [SerializeField] private float _pinchZoomSpeed = 0.02f;

        [Header("Shake")]
        [SerializeField] private float _shakeDecay = 2.5f;

        private float _yaw;
        private float _pitch = 15f;
        private float _previousPinchDistance = -1f;
        private float _shakeTimeRemaining;
        private float _shakeMagnitude;
        private Vector3 _shakeOffset;

        private void Update()
        {
            HandleZoom();
            UpdateShake();
        }

        private void LateUpdate()
        {
            if (_pivot == null)
            {
                return;
            }

            transform.position = _pivot.position;
            PositionCameraWithCollision();
        }

        /// <summary>Reads scroll wheel (desktop) or two-finger pinch delta (touch) and adjusts <see cref="_distance"/>.</summary>
        private void HandleZoom()
        {
            if (Touchscreen.current != null && Touchscreen.current.touches.Count >= 2)
            {
                Vector2 touch0 = Touchscreen.current.touches[0].position.ReadValue();
                Vector2 touch1 = Touchscreen.current.touches[1].position.ReadValue();
                float currentPinchDistance = Vector2.Distance(touch0, touch1);

                if (_previousPinchDistance > 0f)
                {
                    float pinchDelta = currentPinchDistance - _previousPinchDistance;
                    _distance = Mathf.Clamp(_distance - pinchDelta * _pinchZoomSpeed, _minDistance, _maxDistance);
                }

                _previousPinchDistance = currentPinchDistance;
                return;
            }

            _previousPinchDistance = -1f;

            if (Mouse.current != null)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    _distance = Mathf.Clamp(_distance - scroll * _scrollZoomSpeed * Time.deltaTime, _minDistance, _maxDistance);
                }
            }
        }

        /// <summary>Triggers a short camera shake — call on hit impacts, explosions, etc.</summary>
        public void Shake(float duration, float magnitude)
        {
            _shakeTimeRemaining = duration;
            _shakeMagnitude = magnitude;
        }

        private void UpdateShake()
        {
            if (_shakeTimeRemaining <= 0f)
            {
                _shakeOffset = Vector3.zero;
                return;
            }

            _shakeTimeRemaining -= Time.deltaTime;
            float falloff = Mathf.Clamp01(_shakeTimeRemaining) * _shakeMagnitude;
            _shakeOffset = Random.insideUnitSphere * falloff;
            _shakeMagnitude = Mathf.Max(0f, _shakeMagnitude - _shakeDecay * Time.deltaTime);
        }

        /// <summary>Rotates the camera around its pivot based on raw look delta input (mouse/touch/gamepad).</summary>
        public void ApplyLook(Vector2 lookDelta, bool isTouch)
        {
            float sensitivity = isTouch ? _touchSensitivity : _mouseSensitivity;

            _yaw += lookDelta.x * sensitivity;
            _pitch -= lookDelta.y * sensitivity;
            _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

            Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.rotation = rotation;

            PositionCameraWithCollision();
        }

        private void PositionCameraWithCollision()
        {
            if (_pivot == null)
            {
                return;
            }

            Vector3 desiredPosition = _pivot.position - transform.forward * _distance;
            float finalDistance = _distance;

            if (Physics.SphereCast(_pivot.position, _collisionRadius, -transform.forward, out RaycastHit hit, _distance, _collisionMask, QueryTriggerInteraction.Ignore))
            {
                finalDistance = Mathf.Clamp(hit.distance, _minDistance, _maxDistance);
            }

            transform.position = _pivot.position - transform.forward * finalDistance + _shakeOffset;
        }

        public void SetPivot(Transform pivot)
        {
            _pivot = pivot;
        }
    }
}
