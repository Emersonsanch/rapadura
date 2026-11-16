using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The sledge of the Areal Escondido (Docs/Mapas100.md 73: "puxar o trenó: a barca vira trenó e o grupo puxa pela corda (botão segurado
    /// juntos); sozinho, devagar"; "as portas brilham quando a barca passa"), pure and server-runnable. At the Trenó de Areia the barque of the
    /// hours turns into a sledge; [E] takes the rope and the sledge runs down the road through the three stone doors of the Portas Fechadas to the
    /// road's end (and back up from there). Alone it crawls; every tap of [E] on the way is a heave on the rope, a burst of speed that fades; each
    /// ally hauling beside the hero adds a steady pull. Once A Grande Serpente do Areal is freed she remembers she pulls the barque and the sledge
    /// runs at her pace. The doors glow as the sledge goes through, and the first run of each day (UTC) to the doors' end is paid by the
    /// sledge-masters.
    /// <code>
    /// alone 3 m/s · +1.6 m/s for every heave in the last 1.5 s · +2 m/s for each ally within 12 m (at most 3) · capped at 9 m/s
    /// the Grande Serpente freed: 12 m/s, no heaving needed · first run to the doors' end each UTC day: 150 gold
    /// </code>
    /// </summary>
    public static class HiddenSandsSledgeRules
    {
        public const float StationReach = 4f;
        public const float SoloSpeed = 3f;
        public const float HeaveSpeed = 1.6f;
        public const float HeaveSeconds = 1.5f;
        public const float AllySpeed = 2f;
        public const int MaxAllies = 3;
        public const float AllyReach = 12f;
        public const float MaxHauledSpeed = 9f;
        public const float SerpentSpeed = 12f;
        public const int DailyGold = 150;

        /// <summary>The enemy id of A Grande Serpente do Areal (73's map boss): once she is defeated she pulls the sledge.</summary>
        public const string SerpentEnemyId = "enemy_mountain_sledge_serpent";

        /// <summary>The save key of the last paid run (a daily mark holding the UTC day).</summary>
        public const string PaidKey = "rio.hidden_sands.sledge_paid";

        /// <summary>Metres round a door within which it glows while the sledge goes through.</summary>
        public const float DoorGlow = 10f;

        /// <summary>The sledge's speed now: the serpent's pace once she is freed, else the hero's heaves and the allies' pull.</summary>
        public static float SpeedFor(bool serpentFreed, int allies, int heavesInWindow)
        {
            if (serpentFreed)
            {
                return SerpentSpeed;
            }

            float speed = SoloSpeed + Mathf.Clamp(allies, 0, MaxAllies) * AllySpeed + Mathf.Max(0, heavesInWindow) * HeaveSpeed;
            return Mathf.Min(MaxHauledSpeed, speed);
        }

        /// <summary>Whether a door <paramref name="doorAlong"/> metres down the course glows with the sledge <paramref name="sledgeAlong"/> metres down it.</summary>
        public static bool DoorGlows(float doorAlong, float sledgeAlong) => Mathf.Abs(doorAlong - sledgeAlong) <= DoorGlow;

        /// <summary>Whether the run to the doors' end on UTC day <paramref name="today"/> is paid (the last paid run was on <paramref name="lastPaidDay"/>).</summary>
        public static bool Pays(long lastPaidDay, long today) => today > lastPaidDay;

        /// <summary>0 at the Trenó (the course's start), 1 at the doors' end, −1 at neither.</summary>
        public static int StationAt(Vector2[] course, Vector2 p)
        {
            if (course == null || course.Length < 2)
            {
                return -1;
            }

            if ((course[0] - p).sqrMagnitude <= StationReach * StationReach)
            {
                return 0;
            }

            return (course[course.Length - 1] - p).sqrMagnitude <= StationReach * StationReach ? 1 : -1;
        }
    }
}
