using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What a hero at a canal's landing can do today (<see cref="MoonCanalRules.Check"/>).</summary>
    public enum CanalCrossCheck
    {
        /// <summary>The canal is frozen: across on foot.</summary>
        WalkOnIce = 0,
        /// <summary>The canal is open and the hero carries a boat (or owns a water mount).</summary>
        ByBoat = 1,
        /// <summary>The canal is open and the hero has no boat.</summary>
        NeedsBoat = 2,
        /// <summary>Frozen, but the ice of a canal will not bear a heavy mount.</summary>
        HeavyMount = 3,
        /// <summary>The soft barrier would throw the hero back on the far bank.</summary>
        ZoneBlocked = 4,
    }

    /// <summary>
    /// The moon over the canals of the Mar Gelado (Docs/Mapas100.md, map 59: "na lua cheia os canais congelam, anda-se a pé; na
    /// lua nova abrem, só barco"), pure in the in-game day so the online server can run the same calendar for everyone. The
    /// crossings are the <see cref="SkyRoute"/>s of <c>OverworldLayouts.CanalCrossings</c>. Every bank is also joined by the
    /// roads and the whalebone bridges, so a crossing is a shortcut and a closed canal never strands anyone.
    /// <code>
    /// cycle 8 in-game days (20 real minutes each): day 1 = new moon · day 5 = full moon
    /// frozen 3 days out of 8 (moon index 3, 4, 5) · open the other 5
    /// on foot 6 m/s · by boat 9 m/s · about 20 m of crossing, so 2–3 s against a detour of 200 m or more
    /// </code>
    /// </summary>
    public static class MoonCanalRules
    {
        public const int CycleDays = 8;
        public const int FullMoonIndex = 4;

        /// <summary>Moon indices either side of the full moon when the canals hold (3 days in all).</summary>
        public const int FrozenHalfSpan = 1;

        public const float WalkSpeed = 6f;
        public const float BoatSpeed = 9f;

        /// <summary>Height (m) of the small arc of a crossing: enough to clear the banks.</summary>
        public const float CrossingArc = 0.6f;

        /// <summary>0 (new moon) to 7, for an in-game day counted from 1 (any integer works).</summary>
        public static int MoonIndex(int day)
        {
            int index = (day - 1) % CycleDays;
            return index < 0 ? index + CycleDays : index;
        }

        public static bool IsFrozen(int day) => Mathf.Abs(MoonIndex(day) - FullMoonIndex) <= FrozenHalfSpan;

        /// <summary>In-game days until the canals freeze: 0 while they are frozen.</summary>
        public static int DaysUntilFrozen(int day)
        {
            for (int wait = 0; wait < CycleDays; wait++)
            {
                if (IsFrozen(day + wait))
                {
                    return wait;
                }
            }

            return 0;
        }

        /// <summary>Who crosses and how: the barrier first, then the state of the water.</summary>
        public static CanalCrossCheck Check(int day, bool hasBoat, bool ridingHeavyMount, bool zonesOpen)
        {
            if (!zonesOpen)
            {
                return CanalCrossCheck.ZoneBlocked;
            }

            if (IsFrozen(day))
            {
                return ridingHeavyMount ? CanalCrossCheck.HeavyMount : CanalCrossCheck.WalkOnIce;
            }

            return hasBoat ? CanalCrossCheck.ByBoat : CanalCrossCheck.NeedsBoat;
        }

        /// <summary>Seconds a crossing of <paramref name="metres"/> takes; only the two passable results move the hero.</summary>
        public static float Duration(float metres, CanalCrossCheck how) => metres / (how == CanalCrossCheck.ByBoat ? BoatSpeed : WalkSpeed);
    }
}
