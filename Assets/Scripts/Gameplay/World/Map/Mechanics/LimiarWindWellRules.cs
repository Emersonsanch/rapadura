using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// One wind well of the Ninho de Mahaway (map 24), in the region's own coordinates: the updraft rises at <see cref="Well"/> and lets the
    /// glider down at <see cref="Landing"/>, on the other side of a chasm (<c>OverworldLayouts.LimiarWindWells</c>). One way only: the way
    /// back is the well on the other side, or the long walk round to the rope bridge.
    /// </summary>
    public readonly struct LimiarWindWell
    {
        public readonly string Id;
        public readonly Vector2 Well;
        public readonly Vector2 Landing;

        /// <summary>Seconds the well's breathing is shifted, so two wells side by side do not rest together.</summary>
        public readonly float Phase;

        public LimiarWindWell(string id, Vector2 well, Vector2 landing, float phase)
        {
            Id = id;
            Well = well;
            Landing = landing;
            Phase = phase;
        }
    }

    /// <summary>Why a glide may not start (<see cref="LimiarWindWellRules.Check"/>).</summary>
    public enum LimiarWellCheck
    {
        Ok = 0,
        NeedsWings = 1,
        Calm = 2,
        ZoneBlocked = 3,
        Resting = 4,
    }

    /// <summary>
    /// The wind wells of the Ninho de Mahaway (Docs/Mapas100.md 24, "correntes de ar: subir pelos poços de vento (planando) é atalho; cair é a
    /// rota longa"), pure and server-runnable. Each well breathes: it blows, then rests; while it blows, a hero with wings (the
    /// <c>WingsOrDoubleJump</c> requirement: canvas wings, enchanted wings or the griffin, never the ibex) glides over the chasm to the landing
    /// on the far rim. Mahaway's flock pecks at whoever hangs in the air, never past a floor of life; the gust of the landing knocks the
    /// beasts waiting there.
    /// <code>
    /// breath: blows 8 s, rests 4 s (12 s, each well shifted) · reach 4 m · 3 s between glides
    /// glide: 14 m/s (the Farnkesh currents' speed) · arc 12 m · 65–95 m: 5–7 s (walking round to the bridge: 250–450 m)
    /// flock: a peck every 2 s in the air, 2 % of max life (3 % at night), never under 30 %
    /// landing gust: beasts within 8 m lose 6 % of max life (a boss 1 %)
    /// </code>
    /// </summary>
    public static class LimiarWindWellRules
    {
        public const float BlowSeconds = 8f;
        public const float CalmSeconds = 4f;
        public const float CycleSeconds = BlowSeconds + CalmSeconds;
        public const float Reach = 4f;
        public const float CooldownSeconds = 3f;
        public const float GlideSpeed = 14f;
        public const float GlideArc = 12f;
        public const float MinGlideSeconds = 1f;

        public const float PeckSeconds = 2f;
        public const float PeckFraction = 0.02f;
        public const float NightPeckFraction = 0.03f;
        public const float PeckFloorFraction = 0.3f;

        public const float GustRadius = 8f;
        public const float GustFraction = 0.06f;
        public const float GustBossFraction = 0.01f;

        /// <summary>Seconds into the well's own breath (0 to <see cref="CycleSeconds"/>).</summary>
        public static float CycleTime(LimiarWindWell well, float time) => Mathf.Repeat(time + well.Phase, CycleSeconds);

        /// <summary>True while the well blows (the first <see cref="BlowSeconds"/> of its breath).</summary>
        public static bool IsBlowing(LimiarWindWell well, float time) => CycleTime(well, time) < BlowSeconds;

        /// <summary>Seconds until the well blows again (0 while it blows).</summary>
        public static float SecondsUntilBlowing(LimiarWindWell well, float time)
        {
            float t = CycleTime(well, time);
            return t < BlowSeconds ? 0f : CycleSeconds - t;
        }

        /// <summary>The well within <see cref="Reach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int WellAt(IReadOnlyList<LimiarWindWell> wells, Vector2 p)
        {
            int best = -1;
            float bestSq = Reach * Reach;
            for (int i = 0; i < wells.Count; i++)
            {
                float sq = (wells[i].Well - p).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }

        /// <summary>Can the glide start? <paramref name="zonesOpen"/> = the soft barrier lets the hero over every zone under the glide.</summary>
        public static LimiarWellCheck Check(LimiarWindWell well, float time, bool canGlide, bool zonesOpen, float sinceLastGlide)
        {
            if (!canGlide)
            {
                return LimiarWellCheck.NeedsWings;
            }

            if (!zonesOpen)
            {
                return LimiarWellCheck.ZoneBlocked;
            }

            if (sinceLastGlide < CooldownSeconds)
            {
                return LimiarWellCheck.Resting;
            }

            return IsBlowing(well, time) ? LimiarWellCheck.Ok : LimiarWellCheck.Calm;
        }

        /// <summary>Seconds of the glide from the well to its landing.</summary>
        public static float GlideSeconds(LimiarWindWell well) => Mathf.Max(MinGlideSeconds, Vector2.Distance(well.Well, well.Landing) / GlideSpeed);

        /// <summary>How many pecks of the flock are due after <paramref name="elapsed"/> seconds in the air (the first at <see cref="PeckSeconds"/>).</summary>
        public static int PecksDue(float elapsed) => Mathf.FloorToInt(Mathf.Max(0f, elapsed) / PeckSeconds);

        /// <summary>One peck of the flock: a slice of max life that never takes the hero under <see cref="PeckFloorFraction"/>.</summary>
        public static float PeckAmount(float currentHealth, float maxHealth, bool night) =>
            ProfundoHazardRules.Bite(currentHealth, maxHealth, night ? NightPeckFraction : PeckFraction, PeckFloorFraction);

        /// <summary>The landing gust on a beast within <see cref="GustRadius"/> (0 for a dead one).</summary>
        public static float GustAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? GustBossFraction : GustFraction);

        /// <summary>The metres the hero walks instead, round by <paramref name="bridge"/> (the rope bridge or the end of the shaft), for the docs and the tests.</summary>
        public static float WalkRound(LimiarWindWell well, Vector2 bridge) => Vector2.Distance(well.Well, bridge) + Vector2.Distance(bridge, well.Landing);
    }
}
