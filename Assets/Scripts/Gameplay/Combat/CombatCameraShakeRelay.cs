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
            EnsureCameraResolved();

            float magnitude = evt.Magnitude;

            if (ServiceLocator.TryGet(out AccessibilitySettings accessibilitySettings))
            {
                magnitude = accessibilitySettings.ScaleShakeMagnitude(magnitude);
            }

            _playerCamera?.Shake(evt.Duration, magnitude);
        }

        /// <summary>
        /// Guards against being called before <see cref="Awake"/> has run — in Edit Mode (outside
        /// Play Mode), <c>AddComponent&lt;CombatCameraShakeRelay&gt;()</c> does NOT call
        /// <c>Awake</c> synchronously the way Play Mode always does, which left
        /// <see cref="_playerCamera"/> null whenever nothing had assigned it via the Inspector.
        /// </summary>
        private void EnsureCameraResolved()
        {
            if (_playerCamera == null)
            {
                _playerCamera = GetComponent<CameraController>();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor/test-only entry point running the same logic as the private
        /// <see cref="OnCameraShakeRequested"/> EventBus handler, so EditMode tests don't depend
        /// on <see cref="OnEnable"/> (which, like Awake, is not called synchronously by
        /// <c>AddComponent</c> outside Play Mode) having already subscribed it — same reasoning as
        /// <c>NpcInteractable</c>'s <c>OnTriggerEnterForTests</c>/<c>OnTriggerExitForTests</c>.
        /// </summary>
        public void OnCameraShakeRequestedForTests(CameraShakeRequestedEvent evt) => OnCameraShakeRequested(evt);
#endif
    }
}
