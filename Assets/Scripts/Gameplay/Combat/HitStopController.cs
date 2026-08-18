using System.Collections;
using Rapadura.Core.EventBus;
using UnityEngine;

namespace Rapadura.Gameplay.Combat
{
    /// <summary>
    /// Listens for <see cref="HitStopRequestedEvent"/> and briefly drops <see cref="Time.timeScale"/>
    /// to create a "hit-stop"/frame-freeze on impactful hits — classic action-game game-feel.
    /// A single instance should live on a persistent bootstrap object (alongside GameManager);
    /// it does not need to be per-entity since time scale is global.
    /// Uses unscaled time internally so the freeze duration is unaffected by the very time
    /// scale it is manipulating.
    /// </summary>
    public class HitStopController : MonoBehaviour
    {
        [Tooltip("Caps how long a single hit-stop can freeze the game, regardless of what a hitbox requests.")]
        [SerializeField] private float _maxDuration = 0.3f;

        private Coroutine _activeHitStop;

        private void OnEnable()
        {
            EventBus.Subscribe<HitStopRequestedEvent>(OnHitStopRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HitStopRequestedEvent>(OnHitStopRequested);

            if (_activeHitStop != null)
            {
                Time.timeScale = 1f;
            }
        }

        private void OnHitStopRequested(HitStopRequestedEvent evt)
        {
            float duration = Mathf.Clamp(evt.Duration, 0f, _maxDuration);
            if (duration <= 0f)
            {
                return;
            }

            if (_activeHitStop != null)
            {
                StopCoroutine(_activeHitStop);
            }

            _activeHitStop = StartCoroutine(RunHitStop(duration, Mathf.Clamp01(evt.TimeScale)));
        }

        private IEnumerator RunHitStop(float duration, float frozenScale)
        {
            float previousScale = Time.timeScale;
            Time.timeScale = frozenScale;

            yield return new WaitForSecondsRealtime(duration);

            Time.timeScale = previousScale > 0f ? previousScale : 1f;
            _activeHitStop = null;
        }
    }
}
