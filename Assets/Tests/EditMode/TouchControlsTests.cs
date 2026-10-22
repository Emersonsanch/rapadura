using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Player;
using Rapadura.UI.Common;
using Rapadura.UI.HUD;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the touch audit fixes: MoveClick bound to primaryTouch/press (not the
    /// release-only tap) and no dead OnScreenStick binding, hold-to-walk timing and state, the HUD
    /// virtual joystick math (dead zone, clamp, panel y inversion), the pinch guard shared by camera
    /// zoom and tap-to-move, press gating for the HUD Atacar button, touch-only HUD/Settings
    /// visibility rules and the new texts' fallbacks / HUD markup.
    /// </summary>
    public class TouchControlsTests
    {
        private static InputActionAsset LoadPlayerControls()
        {
            string json = File.ReadAllText(Path.Combine("Assets", "InputActions", "PlayerControls.inputactions"));
            return InputActionAsset.FromJson(json);
        }

        // ------------------------------------------------------------------
        // Binding paths.
        // ------------------------------------------------------------------

        [Test]
        public void PlayerControls_MoveClickTouch_UsesPress_NotTap()
        {
            InputActionAsset asset = LoadPlayerControls();

            try
            {
                InputAction moveClick = asset.FindAction("MoveClick", throwIfNotFound: true);
                bool hasTouchPress = false;

                foreach (InputBinding binding in moveClick.bindings)
                {
                    StringAssert.DoesNotEndWith("/tap", binding.path, "Tap only fires on release (<0.2s, <5px) — unreliable for tap-to-move.");
                    hasTouchPress |= binding.path == "<Touchscreen>/primaryTouch/press";
                }

                Assert.IsTrue(hasTouchPress, "MoveClick needs <Touchscreen>/primaryTouch/press for tap/hold-to-move.");
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        [Test]
        public void PlayerControls_HasNoOnScreenControlBindings()
        {
            InputActionAsset asset = LoadPlayerControls();

            try
            {
                foreach (InputActionMap map in asset.actionMaps)
                {
                    foreach (InputBinding binding in map.bindings)
                    {
                        StringAssert.DoesNotContain("OnScreen", binding.path ?? string.Empty,
                            $"{binding.action}: the uGUI OnScreen* controls feed a virtual Gamepad and unpair the Touchscreen; the HUD joystick writes PlayerInputHandler.TouchMoveInput instead.");
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(asset);
            }
        }

        // ------------------------------------------------------------------
        // Hold-to-walk.
        // ------------------------------------------------------------------

        [Test]
        public void HoldToWalkTimer_ReaimsEveryInterval_OnlyWhileHeld()
        {
            var timer = new HoldToWalkTimer(0.15f);
            timer.Restart();

            Assert.IsFalse(timer.Tick(true, 0.1f), "No re-aim before one full interval (the press itself aimed).");
            Assert.IsTrue(timer.Tick(true, 0.1f), "0.2s held → re-aim.");
            Assert.IsFalse(timer.Tick(true, 0.05f), "Remainder 0.05 + 0.05 = 0.1 < 0.15.");
            Assert.IsTrue(timer.Tick(true, 0.1f), "Remainder carried over: 0.2 → re-aim.");

            Assert.IsFalse(timer.Tick(false, 0.5f), "Released: never re-aims.");
            Assert.IsFalse(timer.Tick(true, 0.1f), "Releasing reset the interval.");
        }

        [Test]
        public void HoldToWalkTimer_LongFrame_ReaimsOnce_AndRestartResets()
        {
            var timer = new HoldToWalkTimer(0.15f);

            Assert.IsTrue(timer.Tick(true, 1f), "A 1s hitch yields one re-aim, not six.");
            Assert.IsTrue(timer.Tick(true, 0.1f), "1.0 % 0.15 = 0.1 left over, + 0.1 → re-aim.");

            timer.Tick(true, 0.1f);
            timer.Restart();
            Assert.IsFalse(timer.Tick(true, 0.1f), "Restart (a new accepted press) starts a fresh interval.");
        }

        [Test]
        public void HoldToWalkTimer_InvalidInterval_FallsBackToDefault()
        {
            Assert.AreEqual(HoldToWalkTimer.DefaultInterval, new HoldToWalkTimer(0f).Interval);
            Assert.AreEqual(HoldToWalkTimer.DefaultInterval, new HoldToWalkTimer(-1f).Interval);
        }

        [Test]
        public void NextMoveHoldState_StartsOnAcceptedPress_EndsOnReleasePausePinch()
        {
            Assert.IsTrue(PlayerInputHandler.NextMoveHoldState(false, acceptedPressThisFrame: true, stillPressed: true, paused: false, pinchLatched: false));
            Assert.IsTrue(PlayerInputHandler.NextMoveHoldState(true, false, true, false, false), "Keeps holding.");
            Assert.IsFalse(PlayerInputHandler.NextMoveHoldState(false, false, true, false, false), "A press that started on UI (not accepted) never becomes a hold.");
            Assert.IsFalse(PlayerInputHandler.NextMoveHoldState(true, false, false, false, false), "Released.");
            Assert.IsFalse(PlayerInputHandler.NextMoveHoldState(true, false, true, true, false), "Paused.");
            Assert.IsFalse(PlayerInputHandler.NextMoveHoldState(true, false, true, false, true), "Second finger → pinch.");
        }

        // ------------------------------------------------------------------
        // Virtual joystick math.
        // ------------------------------------------------------------------

        [Test]
        public void Joystick_InsideDeadZone_IsZero()
        {
            Assert.AreEqual(Vector2.zero, VirtualJoystickMath.ComputeMove(Vector2.zero, 75f));
            Assert.AreEqual(Vector2.zero, VirtualJoystickMath.ComputeMove(new Vector2(75f * 0.1f, 0f), 75f, 0.15f));
            Assert.AreEqual(Vector2.zero, VirtualJoystickMath.ComputeMove(new Vector2(10f, 0f), 0f), "Unlaid-out joystick (radius 0) never moves.");
        }

        [Test]
        public void Joystick_RimIsFullSpeed_BeyondRimIsClamped()
        {
            Vector2 rim = VirtualJoystickMath.ComputeMove(new Vector2(75f, 0f), 75f, 0.15f);
            Assert.AreEqual(1f, rim.x, 1e-4f);
            Assert.AreEqual(0f, rim.y, 1e-4f);

            Vector2 beyond = VirtualJoystickMath.ComputeMove(new Vector2(300f, 300f), 75f, 0.15f);
            Assert.AreEqual(1f, beyond.magnitude, 1e-4f);
            Assert.AreEqual(beyond.x, -beyond.y, 1e-4f, "Direction kept (panel down-right = gameplay right-back).");
        }

        [Test]
        public void Joystick_PanelUp_IsForward_AndDeadZoneIsRescaled()
        {
            Vector2 up = VirtualJoystickMath.ComputeMove(new Vector2(0f, -75f), 75f, 0.15f);
            Assert.AreEqual(0f, up.x, 1e-4f);
            Assert.AreEqual(1f, up.y, 1e-4f, "Panel y grows down; pushing the thumb up must walk forward.");

            // (0.575 - 0.15) / 0.85 = 0.5: smooth ramp from the dead-zone edge, no speed jump.
            Vector2 half = VirtualJoystickMath.ComputeMove(new Vector2(75f * 0.575f, 0f), 75f, 0.15f);
            Assert.AreEqual(0.5f, half.x, 1e-3f);
        }

        [Test]
        public void Joystick_KnobOffset_IsClampedToTravelRadius()
        {
            Assert.AreEqual(new Vector2(10f, -5f), VirtualJoystickMath.ClampKnobOffset(new Vector2(10f, -5f), 43f));
            Assert.AreEqual(43f, VirtualJoystickMath.ClampKnobOffset(new Vector2(200f, 0f), 43f).magnitude, 1e-4f);
            Assert.AreEqual(Vector2.zero, VirtualJoystickMath.ClampKnobOffset(new Vector2(5f, 5f), 0f));
        }

        [Test]
        public void CombineMove_SumsJoystickWithAction_ClampedToUnit()
        {
            Assert.AreEqual(new Vector2(0.5f, 0f), PlayerInputHandler.CombineMove(Vector2.zero, new Vector2(0.5f, 0f)));
            Assert.AreEqual(1f, PlayerInputHandler.CombineMove(Vector2.right, Vector2.right).magnitude, 1e-4f);
            Assert.AreEqual(Vector2.up, PlayerInputHandler.CombineMove(Vector2.up, Vector2.zero), "Keyboard/gamepad unchanged without the joystick.");
        }

        // ------------------------------------------------------------------
        // Pinch guard.
        // ------------------------------------------------------------------

        [Test]
        public void Pinch_NeedsTwoWorldFingers()
        {
            Assert.IsFalse(TouchGestureTracker.IsPinch(0));
            Assert.IsFalse(TouchGestureTracker.IsPinch(1));
            Assert.IsTrue(TouchGestureTracker.IsPinch(2));
            Assert.IsTrue(TouchGestureTracker.IsPinch(3));
        }

        [Test]
        public void PinchLatch_HoldsUntilEveryWorldFingerLifts()
        {
            bool latched = TouchGestureTracker.NextPinchLatch(false, worldTouchCount: 2);
            Assert.IsTrue(latched);

            latched = TouchGestureTracker.NextPinchLatch(latched, worldTouchCount: 1);
            Assert.IsTrue(latched, "One finger of the pinch still down: a primaryTouch re-press must not walk.");

            latched = TouchGestureTracker.NextPinchLatch(latched, 0);
            Assert.IsFalse(latched, "No world finger left: next tap walks again.");

            Assert.IsFalse(TouchGestureTracker.NextPinchLatch(false, worldTouchCount: 1),
                "Joystick thumb (UI) + one world finger is not a pinch.");
        }

        [Test]
        public void PinchLatch_UiFingerStillDown_DoesNotKeepTheLatch()
        {
            // Thumb on Correr/joystick (UI, not counted as world) + a two-finger pinch on the ground.
            bool latched = TouchGestureTracker.NextPinchLatch(false, worldTouchCount: 2);
            Assert.IsTrue(latched);

            // Both pinch fingers lift; the thumb stays on Correr: worldTouchCount drops to 0.
            latched = TouchGestureTracker.NextPinchLatch(latched, worldTouchCount: 0);
            Assert.IsFalse(latched, "Holding Correr after a pinch must not block the next tap-to-move.");
        }

        [Test]
        public void PinchPair_StaysOnTheSameIds_WhenAThirdFingerComesAndGoes()
        {
            int id0 = TouchGestureTracker.NoTouch, id1 = TouchGestureTracker.NoTouch;

            Assert.IsTrue(TouchGestureTracker.SelectPinchPair(new List<int> { 3, 5 }, ref id0, ref id1), "Pinch starts: new pair.");
            Assert.AreEqual(3, id0);
            Assert.AreEqual(5, id1);

            // A third finger joins before the pair in slot order, then lifts: pair unchanged.
            Assert.IsFalse(TouchGestureTracker.SelectPinchPair(new List<int> { 7, 3, 5 }, ref id0, ref id1));
            Assert.AreEqual(3, id0);
            Assert.AreEqual(5, id1);
            Assert.IsFalse(TouchGestureTracker.SelectPinchPair(new List<int> { 5, 3 }, ref id0, ref id1), "Slot order swapped: same pair.");

            // One pair finger lifts while two others remain: the pair re-forms (and reports it).
            Assert.IsTrue(TouchGestureTracker.SelectPinchPair(new List<int> { 7, 5 }, ref id0, ref id1));
            Assert.AreEqual(7, id0);
            Assert.AreEqual(5, id1);

            Assert.IsTrue(TouchGestureTracker.SelectPinchPair(new List<int> { 5 }, ref id0, ref id1), "Below two fingers: pair cleared.");
            Assert.AreEqual(TouchGestureTracker.NoTouch, id0);
            Assert.AreEqual(TouchGestureTracker.NoTouch, id1);
            Assert.IsFalse(TouchGestureTracker.SelectPinchPair(new List<int>(), ref id0, ref id1));
        }

        // ------------------------------------------------------------------
        // Press gating.
        // ------------------------------------------------------------------

        [Test]
        public void GatePresses_TouchAttack_SurvivesPointerOverUi_ButNotPause()
        {
            bool jump = false, interact = false, attack = false, moveClick = true;
            PlayerInputHandler.GatePresses(ref jump, ref interact, ref attack, ref moveClick, touchAttack: true, paused: false, pointerOverUi: true, pinchLatched: false);
            Assert.IsTrue(attack, "The HUD Atacar press is a UI press by design.");
            Assert.IsFalse(moveClick, "The same finger on the button must not also walk.");

            jump = interact = attack = false;
            moveClick = true;
            PlayerInputHandler.GatePresses(ref jump, ref interact, ref attack, ref moveClick, touchAttack: true, paused: true, pointerOverUi: true, pinchLatched: false);
            Assert.IsFalse(attack);
            Assert.IsFalse(moveClick);
        }

        [Test]
        public void GatePresses_PinchLatched_DropsOnlyMoveClick()
        {
            bool jump = true, interact = true, attack = true, moveClick = true;
            PlayerInputHandler.GatePresses(ref jump, ref interact, ref attack, ref moveClick, touchAttack: false, paused: false, pointerOverUi: false, pinchLatched: true);

            Assert.IsFalse(moveClick);
            Assert.IsTrue(jump);
            Assert.IsTrue(interact);
            Assert.IsTrue(attack);
        }

        // ------------------------------------------------------------------
        // Visibility rules (HUD / Settings).
        // ------------------------------------------------------------------

        [Test]
        public void TouchControls_ShownOnlyWithTouchscreen_JoystickAlsoNeedsOptIn()
        {
            Assert.IsTrue(TouchControlsPreferences.ShouldShowTouchControls(true));
            Assert.IsFalse(TouchControlsPreferences.ShouldShowTouchControls(false));

            Assert.IsTrue(TouchControlsPreferences.ShouldShowVirtualJoystick(true, true));
            Assert.IsFalse(TouchControlsPreferences.ShouldShowVirtualJoystick(true, false), "Default off: tap-to-move is primary.");
            Assert.IsFalse(TouchControlsPreferences.ShouldShowVirtualJoystick(false, true));
            Assert.IsFalse(TouchControlsPreferences.VirtualJoystickDefault);
        }

        [Test]
        public void ResolveRebindColumns_TouchFirstHidesKeyboard_GamepadOnlyWhenConnected()
        {
            SettingsMenuController.ResolveRebindColumns(hasTouchscreen: false, hasKeyboard: true, hasGamepad: false, isMobilePlatform: false, out bool kb, out bool pad);
            Assert.IsTrue(kb);
            Assert.IsTrue(pad, "Desktop keeps the gamepad column even with no pad connected.");

            SettingsMenuController.ResolveRebindColumns(true, true, false, false, out kb, out pad);
            Assert.IsTrue(kb, "Touch laptop with a keyboard: still desktop.");
            Assert.IsTrue(pad);

            SettingsMenuController.ResolveRebindColumns(true, false, false, false, out kb, out pad);
            Assert.IsFalse(kb);
            Assert.IsFalse(pad);

            SettingsMenuController.ResolveRebindColumns(true, true, false, true, out kb, out pad);
            Assert.IsFalse(kb, "Phone: Android's Back-key Keyboard device doesn't count.");
            Assert.IsFalse(pad);

            SettingsMenuController.ResolveRebindColumns(true, true, true, true, out kb, out pad);
            Assert.IsFalse(kb);
            Assert.IsTrue(pad, "Phone with a gamepad connected can remap it.");
        }

        [Test]
        public void TouchControlsTexts_FallBackToPortuguese()
        {
            Assert.AreEqual("Joystick virtual", TouchControlsTexts.Get(null, TouchControlsTexts.SettingsVirtualJoystick));
            Assert.IsNotNull(TouchControlsTexts.GetFallback(TouchControlsTexts.SettingsVirtualJoystickTouchOnly));
            Assert.IsNotNull(TouchControlsTexts.GetFallback(TouchControlsTexts.StatusTouchOnly));
            Assert.AreEqual("Atacar", UiTexts.Get(null, UiTexts.HudAttack));
        }

        [Test]
        public void HudView_KeepsStyle_AndDeclaresTouchControls_HiddenByDefault()
        {
            string uxml = File.ReadAllText(Path.Combine("Assets", "UI", "HUD", "HudView.uxml"));

            StringAssert.Contains("<Style src=", uxml);
            foreach (string name in new[] { HudController.AttackButtonName, HudController.CameraClusterName, HudController.CameraLeftButtonName,
                         HudController.CameraRightButtonName, HudController.JoystickName, HudController.JoystickKnobName })
            {
                StringAssert.Contains("name=\"" + name + "\"", uxml, name);
            }

            // Desktop must never flash the touch-only controls before HudController hides them.
            foreach (string name in new[] { HudController.AttackButtonName, HudController.CameraClusterName, HudController.JoystickName })
            {
                int at = uxml.IndexOf("name=\"" + name + "\"", System.StringComparison.Ordinal);
                int end = uxml.IndexOf('>', at);
                StringAssert.Contains(HudController.HiddenClass, uxml.Substring(at, end - at), name);
            }
        }
    }
}
