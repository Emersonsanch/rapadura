using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Where the giant's breath is in its minute (<see cref="MisturaShoulderRules.PhaseAt"/>).</summary>
    public enum MisturaBreathPhase
    {
        Still = 0,
        /// <summary>The warning: the stone creaks and the anchor stones light up, nothing moves yet.</summary>
        Warning = 1,
        Tilting = 2,
    }

    /// <summary>
    /// The breathing shoulder of Upelluri (Docs/Mapas100.md, map 81: "a cada 60 s o terreno inclina, tudo escorrega devagar para um lado; usar as
    /// pedras como apoio"; "de noite o gigante dorme e não inclina"), pure. The slope only ever pushes the hero along the giant's shoulder, north
    /// on one breath and south on the next, so the drift cancels out and never carries them sideways into another map's column; it is a slow
    /// push the hero can walk against, it does no damage and it stops by itself.
    /// <code>
    /// every 60 s: 45 s still · 5 s warning · 10 s tilting · night: always still
    /// slide 1.8 m/s standing, × 0.4 while walking (at most 18 m a breath) · within 5 m of an anchor stone: no slide
    /// </code>
    /// </summary>
    public static class MisturaShoulderRules
    {
        public const float CycleSeconds = 60f;
        public const float StillSeconds = 45f;
        public const float WarningSeconds = 5f;
        public const float TiltSeconds = 10f;
        public const float SlideSpeed = 1.8f;
        public const float WalkingFactor = 0.4f;
        public const float StoneReach = 5f;

        /// <summary>Which breath <paramref name="time"/> falls in.</summary>
        public static int BreathIndex(float time) => Mathf.FloorToInt(Mathf.Max(0f, time) / CycleSeconds);

        /// <summary>The breath's phase at <paramref name="time"/>; the giant sleeps at night.</summary>
        public static MisturaBreathPhase PhaseAt(float time, bool night)
        {
            if (night)
            {
                return MisturaBreathPhase.Still;
            }

            float t = Mathf.Repeat(Mathf.Max(0f, time), CycleSeconds);
            if (t < StillSeconds)
            {
                return MisturaBreathPhase.Still;
            }

            return t < StillSeconds + WarningSeconds ? MisturaBreathPhase.Warning : MisturaBreathPhase.Tilting;
        }

        /// <summary>The way breath <paramref name="index"/> tilts: north on the even ones, south on the odd.</summary>
        public static Vector2 Direction(int index) => (index & 1) == 0 ? Vector2.up : Vector2.down;

        /// <summary>Whether <paramref name="p"/> is close enough to an anchor stone to hold on (region coordinates).</summary>
        public static bool Sheltered(IReadOnlyList<Vector2> stones, Vector2 p)
        {
            for (int i = 0; i < stones.Count; i++)
            {
                if ((stones[i] - p).sqrMagnitude <= StoneReach * StoneReach)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>The slide (m/s) of this frame: zero unless the shoulder is tilting and the hero is not holding on to a stone.</summary>
        public static Vector2 SlideVelocity(MisturaBreathPhase phase, Vector2 direction, bool walking, bool sheltered)
        {
            if (phase != MisturaBreathPhase.Tilting || sheltered)
            {
                return Vector2.zero;
            }

            return direction * (SlideSpeed * (walking ? WalkingFactor : 1f));
        }
    }
}
