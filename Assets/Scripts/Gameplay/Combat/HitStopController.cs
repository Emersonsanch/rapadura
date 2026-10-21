using System.Collections;
using Rapadura.Core.Events;
using Rapadura.Core.Systems;
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
        private readonly HitStopScaleState _scaleState = new HitStopScaleState();

        private void OnEnable()
        {
            EventBus.Subscribe<HitStopRequestedEvent>(OnHitStopRequested);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<HitStopRequestedEvent>(OnHitStopRequested);

            if (_activeHitStop != null)
            {
                _activeHitStop = null;
                Time.timeScale = _scaleState.End(GameplayPause.IsPaused);
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

            // Overlapping hit-stops: the original scale is captured only by the first one, so a
            // second hit landing mid-freeze no longer "restores" to the frozen scale forever.
            Time.timeScale = _scaleState.Begin(Time.timeScale, Mathf.Clamp01(evt.TimeScale));
            _activeHitStop = StartCoroutine(RunHitStop(duration));
        }

        private IEnumerator RunHitStop(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);

            _activeHitStop = null;
            Time.timeScale = _scaleState.End(GameplayPause.IsPaused);
        }
    }

    /// <summary>
    /// Pure time-scale bookkeeping for <see cref="HitStopController"/> (EditMode-testable): remembers
    /// the scale from before the first of a run of overlapping hit-stops and hands it back on End.
    /// </summary>
    public sealed class HitStopScaleState
    {
        private float _originalScale = 1f;

        public bool IsActive { get; private set; }

        /// <summary>Starts (or extends) a freeze; returns the scale to apply.</summary>
        public float Begin(float currentScale, float frozenScale)
        {
            if (!IsActive)
            {
                _originalScale = currentScale;
                IsActive = true;
            }

            return frozenScale;
        }

        /// <summary>
        /// Ends the freeze; returns the scale to restore — 0 while gameplay is paused (GameplayPause
        /// also re-enforces that), otherwise the captured scale, or 1 if it was itself a pause/freeze.
        /// </summary>
        public float End(bool gameplayPaused)
        {
            float original = _originalScale;
            IsActive = false;
            _originalScale = 1f;

            if (gameplayPaused)
            {
                return 0f;
            }

            return original > 0f ? original : 1f;
        }
    }
}
