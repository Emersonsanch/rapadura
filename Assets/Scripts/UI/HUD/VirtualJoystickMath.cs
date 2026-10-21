using UnityEngine;

namespace Rapadura.UI.HUD
{
    /// <summary>
    /// Pure math for the HUD's UI Toolkit virtual joystick (EditMode-testable, no UI Toolkit calls).
    /// Input is the pointer's offset from the joystick centre in <em>panel</em> space (y grows
    /// downward); output is a gameplay move vector (y up), clamped to the unit circle, with a
    /// radial dead zone that is rescaled so the vector still ramps smoothly from 0 at the dead-zone
    /// edge to 1 at the rim (no jump in speed when leaving the dead zone).
    /// </summary>
    public static class VirtualJoystickMath
    {
        /// <summary>Fraction of the radius ignored around the centre (resting thumb jitter).</summary>
        public const float DefaultDeadZone = 0.15f;

        /// <summary>Panel-space offset → move vector (x right, y up), magnitude in [0, 1].</summary>
        public static Vector2 ComputeMove(Vector2 panelOffsetFromCenter, float radius, float deadZone = DefaultDeadZone)
        {
            if (radius <= 0f)
            {
                return Vector2.zero;
            }

            // Panel y points down; gameplay "up" on the stick is forward.
            Vector2 normalized = new Vector2(panelOffsetFromCenter.x, -panelOffsetFromCenter.y) / radius;
            float magnitude = normalized.magnitude;
            deadZone = Mathf.Clamp(deadZone, 0f, 0.95f);

            if (magnitude <= deadZone || magnitude <= Mathf.Epsilon)
            {
                return Vector2.zero;
            }

            float clamped = Mathf.Min(magnitude, 1f);
            float rescaled = (clamped - deadZone) / (1f - deadZone);
            return normalized / magnitude * rescaled;
        }

        /// <summary>Where the knob is drawn: the raw offset, clamped so the knob never leaves the base.</summary>
        public static Vector2 ClampKnobOffset(Vector2 panelOffsetFromCenter, float radius)
        {
            return radius <= 0f ? Vector2.zero : Vector2.ClampMagnitude(panelOffsetFromCenter, radius);
        }
    }
}
