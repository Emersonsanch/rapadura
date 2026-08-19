using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.Cameras
{
    /// <summary>
    /// Reads mouse scroll (desktop) / two-finger pinch (touch) and smoothly drives a zoom distance
    /// between <see cref="CameraSettings.minZoom"/> and <see cref="CameraSettings.maxZoom"/>, starting
    /// at <see cref="CameraSettings.defaultZoom"/>. Pure data — <see cref="CameraController"/> reads
    /// <see cref="CurrentDistance"/> each frame and applies it; this class has no scene dependencies
    /// beyond input, so it's trivial to unit test.
    /// </summary>
    public class CameraZoom
    {
        private readonly CameraSettings _settings;
        private float _targetDistance;
        private float _currentDistance;
        private float _velocity;
        private float _previousPinchDistance = -1f;

        public CameraZoom(CameraSettings settings)
        {
            _settings = settings;
            _targetDistance = settings.defaultZoom;
            _currentDistance = settings.defaultZoom;
        }

        public float CurrentDistance => _currentDistance;

        /// <summary>Call once per frame with real input; advances the smoothed distance.</summary>
        public void Tick(float deltaTime)
        {
            ReadInput();
            _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _velocity, _settings.zoomSmoothTime);
        }

        /// <summary>Directly requests a target distance (e.g. from a settings menu), clamped to min/max.</summary>
        public void SetTargetDistance(float distance)
        {
            _targetDistance = Mathf.Clamp(distance, _settings.minZoom, _settings.maxZoom);
        }

        private void ReadInput()
        {
            if (Touchscreen.current != null && Touchscreen.current.touches.Count >= 2)
            {
                Vector2 touch0 = Touchscreen.current.touches[0].position.ReadValue();
                Vector2 touch1 = Touchscreen.current.touches[1].position.ReadValue();
                float currentPinchDistance = Vector2.Distance(touch0, touch1);

                if (_previousPinchDistance > 0f)
                {
                    float pinchDelta = currentPinchDistance - _previousPinchDistance;
                    SetTargetDistance(_targetDistance - pinchDelta * 0.02f);
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
                    SetTargetDistance(_targetDistance - scroll * _settings.zoomSpeed * Time.deltaTime);
                }
            }
        }
    }
}
