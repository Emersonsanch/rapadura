using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The forgotten bridges of the Pontes de Raiz Viva (Docs/Mapas100.md 32: "fazer crescer: regar as pontes desfeitas com Água Viva (skills do
    /// Joaquim ou Gota de Água Viva) as faz crescer em 10 s; abre caminhos e baús"), pure and server-runnable. Three living-root bridges over
    /// the gorges came undone. [E] at one (from 5 m) waters it: Joaquim with the Living Water of his own hands, any other cousin with a full
    /// Odre de Água (it is not used up). Staying within 8 m for 10 s grows it; walking off stops the growth (it starts again). A grown bridge
    /// stays for 10 minutes of the visit, and the root chest on its far side opens (from 3 m), once per bridge per game day.
    /// <code>
    /// 3 bridges · [E] 5 m · Joaquim or the Odre de Água · grow 10 s within 8 m · grown 600 s · chest 3 m, once per bridge per game day
    /// </code>
    /// </summary>
    public static class RootBridgeRules
    {
        public const float WaterReach = 5f;
        public const float GrowReach = 8f;
        public const float GrowSeconds = 10f;
        public const float GrownSeconds = 600f;
        public const float ChestReach = 3f;
        public const string WaterskinItemId = "item_odre_agua";

        private const string KeyPrefix = "delta.rootbridge.";

        private static string[] s_keys;

        /// <summary>The daily mark of bridge <paramref name="bridge"/>'s chest, built once.</summary>
        public static string ChestKey(int bridge)
        {
            if (s_keys == null)
            {
                var keys = new string[8];
                for (int i = 0; i < keys.Length; i++)
                {
                    keys[i] = KeyPrefix + i;
                }

                s_keys = keys;
            }

            return bridge >= 0 && bridge < s_keys.Length ? s_keys[bridge] : KeyPrefix + bridge;
        }

        /// <summary>Whether the hero can water a bridge: Joaquim always, the others with a waterskin.</summary>
        public static bool CanWater(CharacterId? hero, bool hasWaterskin) => hero == CharacterId.Joaquim || hasWaterskin;

        /// <summary>The growth after <paramref name="seconds"/> (0–1): it grows only while the hero stays near, and falls back to 0 otherwise.</summary>
        public static float Grow(float growth, bool near, float seconds) => near ? Mathf.Min(1f, growth + seconds / GrowSeconds) : 0f;

        public static bool Grown(float grownAt, float now) => !float.IsNaN(grownAt) && now >= grownAt && now - grownAt < GrownSeconds;

        /// <summary>Whether the chest opens: the bridge grown and not opened today.</summary>
        public static bool ChestOpens(bool grown, int openedDay, int today) => grown && today > openedDay;

        /// <summary>The point within <paramref name="reach"/> of <paramref name="p"/> (the nearest), or −1.</summary>
        public static int Nearest(IReadOnlyList<Vector2> points, Vector2 p, float reach)
        {
            int best = -1;
            float bestSq = reach * reach;
            for (int i = 0; points != null && i < points.Count; i++)
            {
                float sq = (points[i] - p).sqrMagnitude;
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
