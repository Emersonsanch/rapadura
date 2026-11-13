using Rapadura.Gameplay.Player;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// A shove from the world (wind, a sliding shoulder, a drifting island) that leaves the hero's footing alone. <c>CharacterController.isGrounded</c> only reports what the last
    /// <c>Move</c> touched and <c>PlayerMotor</c> reads it before its own move, so a flat shove would put a hero who is standing into the fall state. A hero on the ground is pressed
    /// back onto it (enough to follow a walkable downhill); one in the air is not, so a shove never shortens a jump.
    /// </summary>
    public static class GroundedPush
    {
        /// <summary>The least press onto the ground, in metres per shove: a slow island's step would otherwise not reach it.</summary>
        public const float MinPress = 0.02f;

        /// <summary>The whole motion of a shove of <paramref name="step"/> metres (x, z of the world).</summary>
        public static Vector3 Motion(Vector2 step, bool grounded) => new Vector3(step.x, grounded ? -Mathf.Max(MinPress, step.magnitude) : 0f, step.y);

        /// <summary>Shoves the hero by <paramref name="step"/> metres, keeping them on the ground they stand on.</summary>
        public static void Shove(CharacterController character, Vector2 step)
        {
            character.Move(Motion(step, character.isGrounded));
            if (character.TryGetComponent(out PlayerMotor motor))
            {
                motor.NoteExternalShove();
            }
        }
    }
}
