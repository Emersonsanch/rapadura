using UnityEngine;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Pure "click-to-move is pushing into a wall" rule (MU Online behaviour: a hero blocked head-on
    /// by a trunk/rock/bench stops instead of running in place forever). Fed every frame with the
    /// current destination and the XZ distance to it; reports a stall once the distance hasn't
    /// improved by <see cref="MinProgress"/> for <see cref="StallSeconds"/> of frames in which the motor was
    /// actually trying to move (rooted, casting or paused frames don't count). Sliding along an
    /// obstacle keeps reducing the distance, so it never trips. A destination moved by more than
    /// <see cref="RetargetDistance"/> (new click, hold-to-walk re-aim) restarts the watch.
    /// EditMode-tested (SolidPropRulesTests).
    /// </summary>
    public sealed class BlockedMoveDetector
    {
        public const float DefaultStallSeconds = 0.6f;
        public const float DefaultMinProgress = 0.08f;
        public const float RetargetDistance = 0.5f;

        private readonly float _stallSeconds;
        private readonly float _minProgress;
        private bool _watching;
        private Vector3 _destination;
        private float _bestDistance;
        private float _stalledFor;

        public BlockedMoveDetector(float stallSeconds = DefaultStallSeconds, float minProgress = DefaultMinProgress)
        {
            _stallSeconds = Mathf.Max(0.05f, stallSeconds);
            _minProgress = Mathf.Max(0.001f, minProgress);
        }

        public float StallSeconds => _stallSeconds;
        public float MinProgress => _minProgress;

        public void Reset()
        {
            _watching = false;
            _stalledFor = 0f;
        }

        /// <summary>One frame. Returns true (once, then resets) when the move has been blocked long enough to give up.</summary>
        public bool Tick(Vector3 destination, float distanceToDestination, bool motorPushing, float deltaTime)
        {
            Vector3 moved = destination - _destination;
            moved.y = 0f;
            if (!_watching || moved.sqrMagnitude > RetargetDistance * RetargetDistance)
            {
                _watching = true;
                _destination = destination;
                _bestDistance = distanceToDestination;
                _stalledFor = 0f;
                return false;
            }

            if (distanceToDestination < _bestDistance - _minProgress)
            {
                _bestDistance = distanceToDestination;
                _stalledFor = 0f;
                return false;
            }

            if (!motorPushing || deltaTime <= 0f)
            {
                return false;
            }

            _stalledFor += deltaTime;
            if (_stalledFor >= _stallSeconds)
            {
                Reset();
                return true;
            }

            return false;
        }
    }
}
