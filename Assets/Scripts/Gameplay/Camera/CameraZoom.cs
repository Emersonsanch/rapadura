using Rapadura.Core.Systems;
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
        private int _pinchId0 = TouchGestureTracker.NoTouch;
        private int _pinchId1 = TouchGestureTracker.NoTouch;

        public CameraZoom(CameraSettings settings)
        {
            _settings = settings;
            _targetDistance = settings.defaultZoom;
            _currentDistance = settings.defaultZoom;
        }

        public float CurrentDistance => _currentDistance;

        /// <summary>Distance the smoothing is heading to (after clamping).</summary>
        public float TargetDistance => _targetDistance;

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
            // Only fingers that started on the world count (TouchGestureTracker): thumbs resting on
            // the virtual joystick / Atacar / hotbar never pinch-zoom. The tracker also only counts
            // touches actually pressed right now — Touchscreen.touches is a fixed-size slot array
            // (device capacity, not fingers down), and indexing [0]/[1] unconditionally used to feed
            // pinch-zoom garbage deltas whenever a touchscreen merely existed.
            TouchGestureTracker.Refresh();

            float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
            ApplyInput(GameplayPause.IsPaused, TouchGestureTracker.IsPinching,
                TouchGestureTracker.PinchId0, TouchGestureTracker.PinchId1,
                TouchGestureTracker.WorldTouch0, TouchGestureTracker.WorldTouch1,
                scroll, Time.deltaTime);
        }

        /// <summary>
        /// One frame of zoom input (EditMode-testable). Nothing zooms while gameplay is paused — the
        /// pause menu and full-screen modals don't block a world pinch through UI picking. The pinch
        /// is measured between the pair <paramref name="pinchId0"/>/<paramref name="pinchId1"/>: when
        /// that pair changes (a third finger lifted, the pair re-formed) the distance is re-baselined
        /// instead of producing a zoom jump.
        /// </summary>
        public void ApplyInput(bool paused, bool pinching, int pinchId0, int pinchId1, Vector2 touch0, Vector2 touch1, float scroll, float deltaTime)
        {
            if (paused)
            {
                EndPinch();
                return;
            }

            if (pinching)
            {
                float currentPinchDistance = Vector2.Distance(touch0, touch1);
                bool samePair = (pinchId0 == _pinchId0 && pinchId1 == _pinchId1) || (pinchId0 == _pinchId1 && pinchId1 == _pinchId0);

                if (_previousPinchDistance > 0f && samePair)
                {
                    float pinchDelta = currentPinchDistance - _previousPinchDistance;
                    SetTargetDistance(_targetDistance - pinchDelta * 0.02f);
                }

                _previousPinchDistance = currentPinchDistance;
                _pinchId0 = pinchId0;
                _pinchId1 = pinchId1;
                return;
            }

            EndPinch();

            if (Mathf.Abs(scroll) > 0.01f)
            {
                SetTargetDistance(_targetDistance - scroll * _settings.zoomSpeed * deltaTime);
            }
        }

        private void EndPinch()
        {
            _previousPinchDistance = -1f;
            _pinchId0 = TouchGestureTracker.NoTouch;
            _pinchId1 = TouchGestureTracker.NoTouch;
        }
    }
}
