using UnityEngine.InputSystem;

namespace Rapadura.Core.Debug
{
    /// <summary>
    /// Pure toggle rules for the debug overlay, EditMode-testable without a touchscreen:
    /// a keyboard key (<see cref="IsToggleKey"/>: F1 or the backquote/tilde key) or a quick
    /// three-finger tap. <see cref="Step"/> is fed the number of fingers down once per frame; a tap
    /// fires on the frame the last finger lifts, if exactly <see cref="RequiredFingers"/> fingers were
    /// down together at some point and the whole gesture (first finger down → last finger up) took
    /// at most <see cref="MaxTapSeconds"/>. Holding, pinching with two fingers or a four-finger touch
    /// never toggles.
    /// </summary>
    public class DebugOverlayToggleGesture
    {
        public const int RequiredFingers = 3;
        public const float MaxTapSeconds = 0.5f;

        private bool _tracking;
        private float _startTime;
        private int _maxFingers;

        /// <summary>A gesture is in progress (at least one finger went down and not all have lifted yet).</summary>
        public bool IsTracking => _tracking;

        /// <summary>Keyboard keys that toggle the overlay (polled by <see cref="DebugOverlayController"/>).</summary>
        public static readonly Key[] ToggleKeys = { Key.F1, Key.Backquote };

        public static bool IsToggleKey(Key key) => System.Array.IndexOf(ToggleKeys, key) >= 0;

        /// <summary>Advances the detector. Returns true on the frame a valid three-finger tap completes.</summary>
        /// <param name="fingersDown">Touches currently pressed.</param>
        /// <param name="time">Unscaled time in seconds (keeps working while gameplay is paused).</param>
        public bool Step(int fingersDown, float time)
        {
            if (fingersDown > 0)
            {
                if (!_tracking)
                {
                    _tracking = true;
                    _startTime = time;
                    _maxFingers = 0;
                }

                if (fingersDown > _maxFingers)
                {
                    _maxFingers = fingersDown;
                }

                return false;
            }

            if (!_tracking)
            {
                return false;
            }

            _tracking = false;
            return _maxFingers == RequiredFingers && time - _startTime <= MaxTapSeconds;
        }

        public void Reset()
        {
            _tracking = false;
            _maxFingers = 0;
        }
    }
}
