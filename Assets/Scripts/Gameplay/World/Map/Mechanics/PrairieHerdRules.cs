using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The herding of the Pradaria dos Mil Cavalos (Docs/Mapas100.md 51: "pastorear: levar cavalos soltos até o acampamento de Soslan dá
    /// pontos"), pure and server-runnable. Five loose horses graze the prairie, each walking a slow circle of 6 m round its spot (a function
    /// of the clock). [E] by one (from 4 m) and it follows the hero, three at most; brought within 22 m of the Poço da Taça, where Soslan's
    /// herders keep the corral of the prairie, each one is delivered: 1 herding point (kept in the save) and 15 gold nuggets. Each horse comes
    /// once per UTC day. Leaving the prairie or falling lets the following horses go back to their spots.
    /// <code>
    /// 5 horses · circle 6 m, one lap per 120 s · [E] 4 m · follow ≤ 3 · corral 22 m round the Poço da Taça → +1 point, 15 gold · once per day each
    /// </code>
    /// </summary>
    public static class PrairieHerdRules
    {
        public const int HorseCount = 5;
        public const float LeadReach = 4f;
        public const int MaxFollowing = 3;
        public const float CorralRadius = 22f;
        public const int PurseEach = 15;
        public const float GrazeRadius = 6f;
        public const float GrazeLapSeconds = 120f;
        public const float FollowGap = 3f;

        /// <summary>The save key of the herding points (a time mark holding the count).</summary>
        public const string PointsKey = "nartes.herd.points";

        private const string DayKeyPrefix = "horse";

        private static string[] s_dayKeys;

        /// <summary>The Nartes driver's daily claim key of horse <paramref name="horse"/> (its <c>nartes.day.</c> prefix is added by the driver), built once.</summary>
        public static string DayKey(int horse)
        {
            if (s_dayKeys == null)
            {
                var keys = new string[HorseCount];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = DayKeyPrefix + i;
                }

                s_dayKeys = keys;
            }

            return horse >= 0 && horse < s_dayKeys.Length ? s_dayKeys[horse] : DayKeyPrefix + horse;
        }

        /// <summary>Where a grazing horse stands at <paramref name="seconds"/>: on its circle, each horse a fifth of a lap from the next.</summary>
        public static Vector2 GrazingAt(Vector2 spot, int horse, double seconds)
        {
            double lap = (seconds / GrazeLapSeconds + horse / (double)HorseCount) % 1d;
            float a = (float)(lap * System.Math.PI * 2d);
            return spot + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * GrazeRadius;
        }

        /// <summary>Whether a horse delivered last on <paramref name="deliveredDay"/> grazes free on <paramref name="today"/>.</summary>
        public static bool Free(long deliveredDay, long today) => deliveredDay != today;

        public static bool CanLead(int following) => following < MaxFollowing;

        public static bool InCorral(Vector2 corral, Vector2 p) => (corral - p).sqrMagnitude <= CorralRadius * CorralRadius;

        /// <summary>Where the <paramref name="index"/>th following horse walks: a line behind the hero, <see cref="FollowGap"/> apart.</summary>
        public static Vector2 FollowAt(Vector2 hero, Vector2 heroForward, int index)
        {
            Vector2 back = heroForward.sqrMagnitude > 1e-6f ? -heroForward.normalized : Vector2.down;
            return hero + back * (FollowGap * (index + 1));
        }
    }
}
