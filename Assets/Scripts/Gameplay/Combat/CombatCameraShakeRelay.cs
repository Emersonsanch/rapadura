using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Cameras;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Forwards <see cref="CameraShakeRequestedEvent"/> to the scene's <see cref="CameraController"/>.
    /// Combat code (hitboxes, future explosions/skills) never references the camera directly —
    /// it just publishes the event — so combat stays decoupled from the camera implementation and
    /// this relay is the only place that needs to know both sides exist. Attach next to (or on)
    /// the CameraController GameObject.
    /// </summary>
    public class CombatCameraShakeRelay : MonoBehaviour
    {
        [SerializeField] private CameraController _playerCamera;

        private void Awake()
        {
            if (_playerCamera == null)
            {
                _playerCamera = GetComponent<CameraController>();
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<CameraShakeRequestedEvent>(OnCameraShakeRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<CameraShakeRequestedEvent>(OnCameraShakeRequested);
        }

        private void OnCameraShakeRequested(CameraShakeRequestedEvent evt)
        {
            float magnitude = evt.Magnitude;

            if (ServiceLocator.TryGet(out AccessibilitySettings accessibilitySettings))
            {
                magnitude = accessibilitySettings.ScaleShakeMagnitude(magnitude);
            }

            _playerCamera?.Shake(evt.Duration, magnitude);
        }
    }
}
