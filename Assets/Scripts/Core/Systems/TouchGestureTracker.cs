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
    /// <see cref="IsPinchLatched"/> stays true from the moment a pinch starts until every <em>world</em>
    /// finger is up: lifting one finger of a pinch leaves the other on the glass, and the Input
    /// System's primaryTouch may then report a fresh press that must not turn into a tap-to-move.
    /// Fingers on the HUD (a thumb holding Correr or the joystick) don't keep the latch, or the next
    /// tap on the ground would be swallowed until every finger lifted.
    ///
    /// The pinch pair (<see cref="PinchId0"/>/<see cref="PinchId1"/>, positions in
    /// <see cref="WorldTouch0"/>/<see cref="WorldTouch1"/>) is tracked by touch id, not by slot
    /// order: a third world finger lifting must not swap the pair under the camera zoom.
    /// </summary>
    public static class TouchGestureTracker
    {
        private static int _lastRefreshFrame = -1;
        private static readonly Dictionary<int, bool> StartedOverUiById = new Dictionary<int, bool>();
        private static readonly List<int> ActiveIds = new List<int>();
        private static readonly List<int> StaleIds = new List<int>();
        private static readonly List<int> WorldIds = new List<int>();
        private static readonly List<Vector2> WorldPositions = new List<Vector2>();

        /// <summary>Fingers currently down, anywhere.</summary>
        public static int ActiveTouchCount { get; private set; }

        /// <summary>Fingers currently down that started on the world (not on UI).</summary>
        public static int WorldTouchCount { get; private set; }

        /// <summary>
        /// Screen position of the first / second world touch (valid when <see cref="WorldTouchCount"/>
        /// is at least 1 / 2). While pinching these are the pinch pair (<see cref="PinchId0"/>/<see cref="PinchId1"/>).
        /// </summary>
        public static Vector2 WorldTouch0 { get; private set; }
        public static Vector2 WorldTouch1 { get; private set; }

        /// <summary>Touch ids of the current pinch pair (-1 when not pinching). Stable while both fingers stay down.</summary>
        public static int PinchId0 { get; private set; } = NoTouch;
        public static int PinchId1 { get; private set; } = NoTouch;

        public const int NoTouch = -1;

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
            WorldIds.Clear();
            WorldPositions.Clear();
            PinchId0 = NoTouch;
            PinchId1 = NoTouch;
            ActiveTouchCount = 0;
            WorldTouchCount = 0;
            IsPinchLatched = false;
        }

        /// <summary>Pure pinch rule (EditMode-testable): two or more world fingers.</summary>
        public static bool IsPinch(int worldTouchCount) => worldTouchCount >= 2;

        /// <summary>
        /// Pure latch rule (EditMode-testable): set by a pinch, kept while a world finger (one left
        /// over from the pinch) is still down, cleared once no world finger remains — fingers on UI
        /// (Correr, joystick) never hold it.
        /// </summary>
        public static bool NextPinchLatch(bool latched, int worldTouchCount)
        {
            if (IsPinch(worldTouchCount))
            {
                return true;
            }

            return worldTouchCount > 0 && latched;
        }

        /// <summary>
        /// Pure pinch-pair rule (EditMode-testable): keeps <paramref name="id0"/>/<paramref name="id1"/>
        /// while both are still among <paramref name="worldIds"/>; otherwise picks the first two world
        /// touches (or clears the pair below two). Returns true when the pair changed.
        /// </summary>
        public static bool SelectPinchPair(List<int> worldIds, ref int id0, ref int id1)
        {
            if (worldIds == null || !IsPinch(worldIds.Count))
            {
                bool hadPair = id0 != NoTouch || id1 != NoTouch;
                id0 = NoTouch;
                id1 = NoTouch;
                return hadPair;
            }

            if (id0 != NoTouch && id1 != NoTouch && id0 != id1 && worldIds.Contains(id0) && worldIds.Contains(id1))
            {
                return false;
            }

            id0 = worldIds[0];
            id1 = worldIds[1];
            return true;
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
            WorldIds.Clear();
            WorldPositions.Clear();
            int active = 0;

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

                    WorldIds.Add(id);
                    WorldPositions.Add(touch.position.ReadValue());
                }
            }

            ForgetLiftedTouches();

            int world = WorldIds.Count;
            ActiveTouchCount = active;
            WorldTouchCount = world;
            IsPinchLatched = NextPinchLatch(IsPinchLatched, world);

            int pinchId0 = PinchId0;
            int pinchId1 = PinchId1;
            SelectPinchPair(WorldIds, ref pinchId0, ref pinchId1);
            PinchId0 = pinchId0;
            PinchId1 = pinchId1;

            if (pinchId0 != NoTouch)
            {
                WorldTouch0 = WorldPositions[WorldIds.IndexOf(pinchId0)];
                WorldTouch1 = WorldPositions[WorldIds.IndexOf(pinchId1)];
            }
            else if (world == 1)
            {
                WorldTouch0 = WorldPositions[0];
            }
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
