using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The sun shadows of the Pirâmides Altas de Meroé (Docs/Mapas100.md 27: "sombras do sol: as sombras das pirâmides apontam para capelas;
    /// ficar na ponta da sombra na hora certa abre a capela (baú)"), pure and server-runnable. Three great pyramids stand round the Pátio dos
    /// Relevos; by day (06:30–17:30 of the game clock) each throws a shadow that swings from west (morning) through north (noon) to east
    /// (evening) and shortens towards noon. Each pyramid has its chapel where the tip of its shadow falls at the chapel's hour (09:00, 12:00
    /// and 15:00). Standing on the tip (2.5 m) for 3 s while it lies on the chapel's door (3 m) opens the chapel: a chest, once per game day.
    /// <code>
    /// sun φ = π (h − 6) / 12 · shadow points (−cos φ, sin φ) · length 18 + 30 (1 − sin φ) m · day 06:30–17:30
    /// chapels at the tips of 09:00, 12:00, 15:00 · door 3 m · stand 2.5 m from the tip for 3 s · once per chapel per game day
    /// </code>
    /// </summary>
    public static class SunShadowRules
    {
        public const float DawnHour = 6.5f;
        public const float DuskHour = 17.5f;
        public const float NoonLength = 18f;
        public const float SlantLength = 30f;
        public const float DoorReach = 3f;
        public const float TipReach = 2.5f;
        public const float HoldSeconds = 3f;

        public static readonly float[] ChapelHours = { 9f, 12f, 15f };

        private const string KeyPrefix = "costa.meroe.chapel.";

        private static string[] s_keys;

        /// <summary>The daily mark of chapel <paramref name="chapel"/> (the game day it was last opened), built once.</summary>
        public static string ChapelKey(int chapel)
        {
            if (s_keys == null)
            {
                var keys = new string[ChapelHours.Length];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = KeyPrefix + i;
                }

                s_keys = keys;
            }

            return chapel >= 0 && chapel < s_keys.Length ? s_keys[chapel] : KeyPrefix + chapel;
        }

        /// <summary>Whether the sun throws shadows at <paramref name="hour"/>.</summary>
        public static bool Shining(float hour)
        {
            float h = Mathf.Repeat(hour, 24f);
            return h >= DawnHour && h <= DuskHour;
        }

        /// <summary>Where the tip of the shadow of a pyramid at <paramref name="apex"/> falls at <paramref name="hour"/> (only meaningful while <see cref="Shining"/>).</summary>
        public static Vector2 TipAt(Vector2 apex, float hour)
        {
            float phi = Mathf.PI * (Mathf.Repeat(hour, 24f) - 6f) / 12f;
            float length = NoonLength + SlantLength * (1f - Mathf.Sin(phi));
            return apex + new Vector2(-Mathf.Cos(phi), Mathf.Sin(phi)) * length;
        }

        /// <summary>The door of the chapel of pyramid <paramref name="chapel"/>: the tip at the chapel's hour.</summary>
        public static Vector2 ChapelDoor(Vector2 apex, int chapel) => TipAt(apex, ChapelHours[chapel]);

        /// <summary>Whether the tip lies on the chapel's door now.</summary>
        public static bool OnDoor(Vector2 apex, int chapel, float hour) => Shining(hour) && (TipAt(apex, hour) - ChapelDoor(apex, chapel)).sqrMagnitude <= DoorReach * DoorReach;

        /// <summary>Whether <paramref name="hero"/> stands on the tip.</summary>
        public static bool OnTip(Vector2 apex, float hour, Vector2 hero) => Shining(hour) && (TipAt(apex, hour) - hero).sqrMagnitude <= TipReach * TipReach;

        /// <summary>The chapel opens after <see cref="HoldSeconds"/> on the tip while it lies on the door, once per game day.</summary>
        public static bool Opens(float heldSeconds, int openedDay, int today) => heldSeconds >= HoldSeconds && today > openedDay;

        /// <summary>The pyramid whose chapel door the hero stands nearest to (within <paramref name="reach"/>), or −1.</summary>
        public static int NearestPyramid(IReadOnlyList<Vector2> apexes, Vector2 hero, float reach)
        {
            int best = -1;
            float bestSq = reach * reach;
            for (int i = 0; apexes != null && i < apexes.Count && i < ChapelHours.Length; i++)
            {
                float sq = (ChapelDoor(apexes[i], i) - hero).sqrMagnitude;
                if (sq <= bestSq)
                {
                    best = i;
                    bestSq = sq;
                }
            }

            return best;
        }
    }
}
