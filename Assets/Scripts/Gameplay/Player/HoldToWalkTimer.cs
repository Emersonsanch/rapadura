namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// MU Online-style hold-to-walk cadence for <see cref="PlayerClickToMove"/>: while the pointer
    /// stays held on the ground after an accepted press, the click-to-move destination is re-aimed
    /// at the pointer every <see cref="Interval"/> seconds, so dragging a finger/mouse steers the
    /// walk instead of requiring repeated taps. Pure (no Unity calls) so the timing is
    /// EditMode-testable.
    ///
    /// The initial press sets its own destination, so <see cref="Restart"/> starts a fresh interval
    /// (the first re-aim comes one full interval later, never on the press frame). Letting go
    /// resets it.
    /// </summary>
    public sealed class HoldToWalkTimer
    {
        public const float DefaultInterval = 0.15f;

        private float _elapsed;

        public HoldToWalkTimer(float interval = DefaultInterval)
        {
            Interval = interval > 0f ? interval : DefaultInterval;
        }

        public float Interval { get; }

        /// <summary>Call on the accepted press frame (the press already aimed the destination).</summary>
        public void Restart() => _elapsed = 0f;

        /// <summary>
        /// Advances the timer; returns true when a re-aim is due this frame. A long frame still yields
        /// a single re-aim (the destination is recomputed from the current pointer anyway), and the
        /// remainder carries over so the cadence doesn't drift.
        /// </summary>
        public bool Tick(bool held, float deltaTime)
        {
            if (!held)
            {
                _elapsed = 0f;
                return false;
            }

            if (deltaTime > 0f)
            {
                _elapsed += deltaTime;
            }

            if (_elapsed < Interval)
            {
                return false;
            }

            _elapsed %= Interval;
            return true;
        }
    }
}
