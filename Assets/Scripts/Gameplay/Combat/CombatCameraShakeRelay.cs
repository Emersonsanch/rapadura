using Rapadura.Core.Accessibility;
using Rapadura.Core.DI;
using Rapadura.Core.EventBus;
using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Forwards <see cref="CameraShakeRequestedEvent"/> to the scene's <see cref="PlayerCamera"/>.
    /// Combat code (hitboxes, future explosions/skills) never references PlayerCamera directly —
    /// it just publishes the event — so combat stays decoupled from the camera implementation and
    /// this relay is the only place that needs to know both sides exist. Attach next to (or on)
    /// the PlayerCamera GameObject.
    /// </summary>
    public class CombatCameraShakeRelay : MonoBehaviour
    {
        [SerializeField] private PlayerCamera _playerCamera;

        private void Awake()
        {
            if (_playerCamera == null)
            {
                _playerCamera = GetComponent<PlayerCamera>();
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
