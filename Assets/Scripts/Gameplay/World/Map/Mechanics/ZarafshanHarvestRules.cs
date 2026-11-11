using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>One tree of the Pomar that can be picked (<see cref="Id"/> is what the save keeps, so the list can grow).</summary>
    public readonly struct ZarafshanHarvestSpot
    {
        public readonly string Id;
        public readonly Vector2 Position;

        public ZarafshanHarvestSpot(string id, Vector2 position)
        {
            Id = id;
            Position = position;
        }
    }

    /// <summary>
    /// The harvest of the Pomar da Vó Edna (Docs/Mapas100.md 01: "as árvores do pomar dão fruta a cada dia do jogo"), pure: the first regional
    /// mechanic a new hero meets, so it only gives and never asks. Nothing is lost by ignoring it and no quest or tutorial step depends on it.
    /// <code>
    /// 4 trees · reach 4 m · each tree once per game day · 1 item, Cooking ingredients in turn (herb, fibre)
    /// </code>
    /// The day picked is stored with <c>WorldMapProgress.SetDailyMark</c> (older saves read 0: every tree ripe).
    /// </summary>
    public static class ZarafshanHarvestRules
    {
        public const float ReachRadius = 4f;
        public const string MarkPrefix = "zarafshan_harvest_";

        /// <summary>What the trees give (existing Cooking ingredients, not the attribute fruits of the Tacho), one a day, in turn.</summary>
        public static readonly string[] Items = { "item_herb", "item_fiber" };

        /// <summary>Index of the tree within reach of <paramref name="local"/> (the closest), or -1.</summary>
        public static int SpotAt(Vector2 local, IReadOnlyList<ZarafshanHarvestSpot> spots)
        {
            int best = -1;
            float bestSqr = ReachRadius * ReachRadius;
            for (int i = 0; spots != null && i < spots.Count; i++)
            {
                float sqr = (spots[i].Position - local).sqrMagnitude;
                if (sqr <= bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>A tree is ripe again on any game day after the last one it was picked (<paramref name="lastDay"/> 0 = never).</summary>
        public static bool IsRipe(int lastDay, int day) => day > lastDay;

        public static string MarkKey(string spotId) => MarkPrefix + spotId;

        /// <summary>The item of tree <paramref name="spot"/> on <paramref name="day"/>.</summary>
        public static string ItemFor(int spot, int day) => Items[Mathf.Abs(spot + day) % Items.Length];

        /// <summary>Whether the Vó Edna has the line after this pick (every other one); otherwise the hero does.</summary>
        public static bool GrandmaSpeaks(int spot, int day) => Mathf.Abs(spot + day) % 2 == 0;
    }
}
