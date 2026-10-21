using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Rapadura.Core.Systems
{
    /// <summary>
    /// Per-frame view of the fingers on the <see cref="Touchscreen"/> that are touching the
    /// <em>world</em> (not the HUD), shared by camera pinch-zoom (<c>CameraZoom</c>) and tap/hold-to-move
    /// (<c>PlayerInputHandler</c>). A finger that went down on UI (virtual joystick, Atacar, a hotbar
    /// slot...) never counts toward a pinch, so "joystick + Atacar" with two thumbs doesn't zoom the
    /// camera, and a real two-finger pinch never also walks the player.
    ///
    /// Whether a touch started over UI is decided once, on the first frame it is seen pressed
    /// (<see cref="UiPointerQuery.IsPointerOverUi"/> at its start position — picking panels isn't free,
    /// so never per frame), and remembered by touch id until the finger lifts.
    ///
    /// <see cref="IsPinchLatched"/> stays true from the moment a pinch starts until every finger is
    /// up: lifting one finger of a pinch leaves the other on the glass, and the Input System's
    /// primaryTouch may then report a fresh press that must not turn into a tap-to-move.
    /// </summary>
    public static class TouchGestureTracker
    {
        private static int _lastRefreshFrame = -1;
        private static readonly Dictionary<int, bool> StartedOverUiById = new Dictionary<int, bool>();
        private static readonly List<int> ActiveIds = new List<int>();
        private static readonly List<int> StaleIds = new List<int>();

        /// <summary>Fingers currently down, anywhere.</summary>
        public static int ActiveTouchCount { get; private set; }

        /// <summary>Fingers currently down that started on the world (not on UI).</summary>
        public static int WorldTouchCount { get; private set; }

        /// <summary>Screen position of the first / second world touch (valid when <see cref="WorldTouchCount"/> is at least 1 / 2).</summary>
        public static Vector2 WorldTouch0 { get; private set; }
        public static Vector2 WorldTouch1 { get; private set; }

        /// <summary>Two or more world fingers are down right now (camera pinch).</summary>
        public static bool IsPinching => IsPinch(WorldTouchCount);

        /// <summary>A pinch happened during the current gesture and not every finger has lifted yet.</summary>
        public static bool IsPinchLatched { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _lastRefreshFrame = -1;
            StartedOverUiById.Clear();
            ActiveIds.Clear();
            StaleIds.Clear();
            ActiveTouchCount = 0;
            WorldTouchCount = 0;
            IsPinchLatched = false;
        }

        /// <summary>Pure pinch rule (EditMode-testable): two or more world fingers.</summary>
        public static bool IsPinch(int worldTouchCount) => worldTouchCount >= 2;

        /// <summary>
        /// Pure latch rule (EditMode-testable): set by a pinch, kept while any finger is still down,
        /// cleared once the screen is empty.
        /// </summary>
        public static bool NextPinchLatch(bool latched, int worldTouchCount, int activeTouchCount)
        {
            if (IsPinch(worldTouchCount))
            {
                return true;
            }

            return activeTouchCount > 0 && latched;
        }

        /// <summary>Re-reads the touchscreen at most once per frame; every reader calls this first.</summary>
        public static void Refresh()
        {
            int frame = Time.frameCount;
            if (frame == _lastRefreshFrame)
            {
                return;
            }

            _lastRefreshFrame = frame;
            ActiveIds.Clear();
            int active = 0;
            int world = 0;

            Touchscreen touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                // touches is a fixed-size slot array (device capacity, not fingers down): only
                // pressed slots are real fingers (see CameraZoom's history for the stale-slot bug).
                foreach (TouchControl touch in touchscreen.touches)
                {
                    if (!touch.press.isPressed)
                    {
                        continue;
                    }

                    active++;
                    int id = touch.touchId.ReadValue();
                    ActiveIds.Add(id);

                    if (!StartedOverUiById.TryGetValue(id, out bool startedOverUi))
                    {
                        startedOverUi = UiPointerQuery.IsPointerOverUi(touch.startPosition.ReadValue());
                        StartedOverUiById[id] = startedOverUi;
                    }

                    if (startedOverUi)
                    {
                        continue;
                    }

                    if (world == 0)
                    {
                        WorldTouch0 = touch.position.ReadValue();
                    }
                    else if (world == 1)
                    {
                        WorldTouch1 = touch.position.ReadValue();
                    }

                    world++;
                }
            }

            ForgetLiftedTouches();

            ActiveTouchCount = active;
            WorldTouchCount = world;
            IsPinchLatched = NextPinchLatch(IsPinchLatched, world, active);
        }

        private static void ForgetLiftedTouches()
        {
            if (StartedOverUiById.Count == ActiveIds.Count)
            {
                return;
            }

            StaleIds.Clear();
            foreach (int id in StartedOverUiById.Keys)
            {
                if (!ActiveIds.Contains(id))
                {
                    StaleIds.Add(id);
                }
            }

            foreach (int id in StaleIds)
            {
                StartedOverUiById.Remove(id);
            }
        }
    }
}
