using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Player-facing touch control options, persisted in PlayerPrefs (device-wide, like the audio
    /// volumes and the rebinds — a phone's control preference isn't per save slot). Written by the
    /// Settings "Controles" tab (<c>TouchControlsSettingsSection</c>) and read by the HUD
    /// (<c>HudController</c>), which listens to <see cref="Changed"/> to show/hide the joystick live.
    ///
    /// The virtual joystick is OFF by default: tap/hold-to-move is the primary touch control (that's
    /// what the first-run tutorial teaches, see <c>TutorialTexts</c>), the joystick is an option for
    /// players who prefer it.
    /// </summary>
    public static class TouchControlsPreferences
    {
        public const string VirtualJoystickKey = "Controls.Touch.VirtualJoystick";
        public const bool VirtualJoystickDefault = false;

        /// <summary>Raised after any option changes.</summary>
        public static event Action Changed;

        public static bool VirtualJoystickEnabled
        {
            get => PlayerPrefs.GetInt(VirtualJoystickKey, VirtualJoystickDefault ? 1 : 0) != 0;
            set
            {
                if (value == VirtualJoystickEnabled)
                {
                    return;
                }

                PlayerPrefs.SetInt(VirtualJoystickKey, value ? 1 : 0);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        /// <summary>True when a touchscreen exists — the touch-only HUD controls (Atacar, camera, joystick) are shown only then.</summary>
        public static bool HasTouchscreen => Touchscreen.current != null;

        /// <summary>Pure rule (EditMode-testable): touch-only HUD controls need a touchscreen.</summary>
        public static bool ShouldShowTouchControls(bool hasTouchscreen) => hasTouchscreen;

        /// <summary>Pure rule (EditMode-testable): the joystick needs a touchscreen AND the player's opt-in.</summary>
        public static bool ShouldShowVirtualJoystick(bool hasTouchscreen, bool joystickEnabled) => hasTouchscreen && joystickEnabled;
    }
}
