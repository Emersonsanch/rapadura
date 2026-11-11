using System;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>Why a cage of sparks may not be opened (<see cref="NoveloBridgeRules.CheckCage"/>).</summary>
    public enum NoveloCageCheck
    {
        Ok = 0,
        AlreadyFree = 1,
        FlightDark = 2,
        Last = 3,
    }

    /// <summary>
    /// The Bridge of the Bōlos lights flight by flight (Docs/Mapas100.md 89, "cada lance só aparece quando as faíscas do lance anterior são
    /// libertadas"), pure and server-runnable. The causeway runs south to north in <see cref="FlightCount"/> flights from the bridge head; the
    /// first is always lit. A cage of sparks stands at the pylon of every flight but the last: freeing it (standing beside it for
    /// <see cref="FreeSeconds"/>) lights the next flight, for good (a time mark per flight in <c>WorldMapProgress</c>). The dark flights and the
    /// black ground beside them (the whole east column) push the hero back to the lit ground they came from, faster than a run; the post on the
    /// middle pillar always stays lit. The way round is through the drowned pits to the west.
    /// <code>
    /// 10 flights of 145 m (z −1090 to 360, region coordinates) · flight 0 lit from the start · 9 cages, one at the pylon of flights 0–8
    /// free a cage: within 3.5 m, [E], then stay within 6 m for 5 s · the Novelo's breath meanwhile: 3 % of max life every 1.5 s (4 % at night), never under 30 %
    /// freed sparks burst: beasts within 10 m lose 8 % of max life (a boss 1 %)
    /// the dark: x 1320–1800, z from the first dark flight to 360 · pushed back to where the hero came from at 7 m/s (the hero runs 5.5) · the Pilar do Meio's 40 m stay lit
    /// bridge head to the Selo dos Artífices: 1425 m on the lit causeway; round by the Cova do Lodo column ~1900 m through levels 386–416
    /// </code>
    /// </summary>
    public static class NoveloBridgeRules
    {
        public const int FlightCount = 10;
        public const float FlightStart = -1090f;
        public const float FlightStep = 145f;
        public const float DarkWest = 1320f;
        public const float DarkEast = 1800f;
        public const float PillarSafeRadius = 40f;

        public const float CageReach = 3.5f;
        public const float CageLeash = 6f;
        public const float FreeSeconds = 5f;
        public const float BreathSeconds = 1.5f;
        public const float BreathFraction = 0.03f;
        public const float NightBreathFraction = 0.04f;
        public const float BreathFloorFraction = 0.3f;
        public const float BurstRadius = 10f;
        public const float BurstFraction = 0.08f;
        public const float BurstBossFraction = 0.01f;
        public const float PushSpeed = 7f;

        /// <summary>North end of the last flight (region z).</summary>
        public const float FlightEnd = FlightStart + FlightStep * FlightCount;

        /// <summary>The save key of flight <paramref name="flight"/> (1–9; a time mark: when it lit, 0 = still dark).</summary>
        public static string MarkKey(int flight) => "profundo.novelo.flight." + flight;

        /// <summary>South end (region z) of flight <paramref name="flight"/>.</summary>
        public static float FlightSouth(int flight) => FlightStart + FlightStep * flight;

        /// <summary>The flight over region z <paramref name="z"/> (0–9), or −1 off the bridge.</summary>
        public static int FlightAt(float z)
        {
            if (z < FlightStart || z > FlightEnd)
            {
                return -1;
            }

            return Mathf.Min(FlightCount - 1, Mathf.FloorToInt((z - FlightStart) / FlightStep));
        }

        /// <summary>How many flights are lit from the head (flight 0 always; then each while <paramref name="marked"/> says so, stopping at the first dark one).</summary>
        public static int LitCount(Func<int, bool> marked)
        {
            int lit = 1;
            while (lit < FlightCount && marked != null && marked(lit))
            {
                lit++;
            }

            return lit;
        }

        /// <summary>Can the cage at the pylon of <paramref name="cage"/> (0–8) be opened with <paramref name="litCount"/> flights lit?</summary>
        public static NoveloCageCheck CheckCage(int cage, int litCount)
        {
            if (cage < 0 || cage >= FlightCount - 1)
            {
                return NoveloCageCheck.Last;
            }

            if (cage + 1 < litCount)
            {
                return NoveloCageCheck.AlreadyFree;
            }

            return cage < litCount ? NoveloCageCheck.Ok : NoveloCageCheck.FlightDark;
        }

        /// <summary>
        /// True when region point <paramref name="p"/> is in the dark: the east column over a flight not yet lit, outside the post on the
        /// middle pillar (<paramref name="pillar"/>).
        /// </summary>
        public static bool IsDark(Vector2 p, int litCount, Vector2 pillar)
        {
            if (p.x < DarkWest || p.x > DarkEast)
            {
                return false;
            }

            int flight = FlightAt(p.y);
            if (flight < 0 || flight < litCount)
            {
                return false;
            }

            return (p - pillar).sqrMagnitude > PillarSafeRadius * PillarSafeRadius;
        }

        /// <summary>
        /// The lit ground nearest to region point <paramref name="p"/>, for a hero who turns up in the dark without having walked in (a respawn,
        /// a portal): the end of the lit flights, the north end of the bridge, the west rim of the column, or the rim of the middle pillar's post
        /// when it is in the dark.
        /// </summary>
        public static Vector2 NearestLitPoint(Vector2 p, int litCount, Vector2 pillar)
        {
            Vector2 best = new Vector2(p.x, FlightSouth(Mathf.Clamp(litCount, 1, FlightCount)) - 0.5f);
            Consider(new Vector2(p.x, FlightEnd + 0.5f), p, ref best);
            Consider(new Vector2(DarkWest - 0.5f, p.y), p, ref best);
            if (FlightAt(pillar.y) >= litCount)
            {
                Vector2 away = p - pillar;
                Consider(pillar + (away.sqrMagnitude > 1e-4f ? away.normalized : Vector2.up) * (PillarSafeRadius - 0.5f), p, ref best);
            }

            return best;
        }

        /// <summary>The push of the dark this frame (region x/z metres): straight back towards <paramref name="lit"/>, the lit ground the hero came from.</summary>
        public static Vector2 PushStep(Vector2 p, Vector2 lit, float deltaTime)
        {
            Vector2 d = lit - p;
            return d.sqrMagnitude > 1e-6f ? d.normalized * (PushSpeed * Mathf.Max(0f, deltaTime)) : Vector2.zero;
        }

        /// <summary>The Novelo's breath on the hero freeing a cage: a slice of max life that never takes them under <see cref="BreathFloorFraction"/>.</summary>
        public static float BreathAmount(float currentHealth, float maxHealth, bool night) =>
            ProfundoHazardRules.Bite(currentHealth, maxHealth, night ? NightBreathFraction : BreathFraction, BreathFloorFraction);

        /// <summary>How many breaths are due after <paramref name="elapsed"/> seconds at the cage (the first at <see cref="BreathSeconds"/>).</summary>
        public static int BreathsDue(float elapsed) => Mathf.FloorToInt(Mathf.Max(0f, elapsed) / BreathSeconds);

        /// <summary>The freed sparks on a beast within <see cref="BurstRadius"/>.</summary>
        public static float BurstAmount(float maxHealth, bool isBoss) => Mathf.Max(0f, maxHealth) * (isBoss ? BurstBossFraction : BurstFraction);

        private static void Consider(Vector2 candidate, Vector2 p, ref Vector2 best)
        {
            if ((candidate - p).sqrMagnitude < (best - p).sqrMagnitude)
            {
                best = candidate;
            }
        }
    }
}
