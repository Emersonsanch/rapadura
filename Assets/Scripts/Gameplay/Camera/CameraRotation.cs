using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Gameplay.Cameras
{
    /// <summary>
    /// Optional Q/E snap-rotation around the fixed isometric angle, in
    /// <see cref="CameraSettings.rotationStepDegrees"/> steps (default 45°), smoothed. Disabled
    /// entirely when <see cref="CameraSettings.allowRotation"/> is off — the camera then stays at
    /// a constant yaw for the whole session, matching classic MU Online. Pure data/logic (no scene
    /// dependencies besides input), read by <see cref="CameraController"/> each frame.
    /// </summary>
    public class CameraRotation
    {
        private readonly CameraSettings _settings;
        private float _targetYawOffset;
        private float _currentYawOffset;
        private float _velocity;

        public CameraRotation(CameraSettings settings)
        {
            _settings = settings;
        }

        /// <summary>Current smoothed yaw offset (degrees) to add on top of <see cref="CameraSettings.cameraYaw"/>.</summary>
        public float CurrentYawOffset => _currentYawOffset;

        public void Tick(float deltaTime)
        {
            if (!_settings.allowRotation)
            {
                return;
            }

            ReadInput();
            _currentYawOffset = Mathf.SmoothDamp(_currentYawOffset, _targetYawOffset, ref _velocity, _settings.rotationSmoothTime);
        }

        /// <summary>Rotates one step counter-clockwise (Q).</summary>
        public void RotateLeft()
        {
            if (_settings.allowRotation)
            {
                _targetYawOffset -= _settings.rotationStepDegrees;
            }
        }

        /// <summary>Rotates one step clockwise (E).</summary>
        public void RotateRight()
        {
            if (_settings.allowRotation)
            {
                _targetYawOffset += _settings.rotationStepDegrees;
            }
        }

        private void ReadInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.qKey.wasPressedThisFrame)
            {
                RotateLeft();
            }
            else if (keyboard.eKey.wasPressedThisFrame)
            {
                RotateRight();
            }
        }
    }
}
