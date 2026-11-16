using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The shadow clock of the Torres Brancas' square (map 42, Docs/Mapas100.md: "O Pequeno Eclipse ... libertado, vira o 'relógio de sombra' da
    /// praça"), pure and server-runnable. Once the Little Eclipse is freed (the boss defeated, kept by the save's defeated registry) it stands on
    /// the south street of the white city as a dial: by day its shadow turns with the sun, and [E] at it reads the hour, how long until the light
    /// of the mirror clock (<see cref="MirrorClockRules"/>) changes its way, and where the first mirror (the north one) must face now. At night
    /// it has no shadow to read.
    /// <code>
    /// [E] within 3 m · day = 6 h to 18 h: the sun goes east (6 h), south (noon), west (18 h); the shadow points away from it
    /// reads: the hour · hours to the next band of the mirror clock (every 6 h) · the north mirror's right way in this band
    /// </code>
    /// Decision (2026-11-16): the clock gives the first mirror only, so the puzzle still asks to follow the light round the other three.
    /// </summary>
    public static class ShadowClockRules
    {
        public const string EclipseEnemyId = "enemy_mountain_little_eclipse";
        public const float Reach = 3f;
        public const float SunriseHour = 6f;
        public const float SunsetHour = 18f;
        public const float GnomonHeight = 2.2f;

        /// <summary>Where the dial stands (Mingcheng region coordinates): on the south street of the Torres Brancas, 12 m from the spire.</summary>
        public static readonly Vector2 Dial = new Vector2(960f, 588f);

        /// <summary>Whether the dial casts a shadow at <paramref name="hour"/> (by day).</summary>
        public static bool HasShadow(float hour)
        {
            float h = Mathf.Repeat(hour, 24f);
            return h >= SunriseHour && h < SunsetHour;
        }

        /// <summary>
        /// The flat direction (x/z) of the shadow at <paramref name="hour"/>: the sun goes east (6 h) → south (noon, the Mingcheng plain lies north of
        /// the sun's path) → west (18 h), and the shadow points away from it. Zero at night.
        /// </summary>
        public static Vector2 ShadowDirection(float hour)
        {
            if (!HasShadow(hour))
            {
                return Vector2.zero;
            }

            float t = (Mathf.Repeat(hour, 24f) - SunriseHour) / (SunsetHour - SunriseHour);
            float a = Mathf.Lerp(90f, 270f, t) * Mathf.Deg2Rad;
            Vector2 sun = new Vector2(Mathf.Sin(a), Mathf.Cos(a));
            return -sun;
        }

        /// <summary>Hours (rounded up, at least 1) until the mirror clock's band changes.</summary>
        public static int HoursToNextBand(float hour)
        {
            float into = Mathf.Repeat(hour, MirrorClockRules.BandHours);
            return Mathf.Max(1, Mathf.CeilToInt(MirrorClockRules.BandHours - into));
        }

        /// <summary>The way the north mirror (the first) must face at <paramref name="hour"/>: 0 north, 1 east, 2 south, 3 west.</summary>
        public static int FirstMirrorState(float hour) => MirrorClockRules.RightState(0, MirrorClockRules.BandOf(hour));

        public static bool AtDial(Vector2 dial, Vector2 p) => (dial - p).sqrMagnitude <= Reach * Reach;
    }
}
