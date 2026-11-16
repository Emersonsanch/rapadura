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
    /// 4 trees · reach 4 m · each tree once per game day · 1 item
    /// the 2 apricot trees (damasqueiro_*) give a Damasco do Pomar (food: +15 health, +20 stamina) every day
    /// the 2 pomegranate trees give Cooking ingredients in turn (herb, fibre)
    /// </code>
    /// The day picked is stored with <c>WorldMapProgress.SetDailyMark</c> (older saves read 0: every tree ripe).
    /// </summary>
    public static class ZarafshanHarvestRules
    {
        public const float ReachRadius = 4f;
        public const string MarkPrefix = "zarafshan_harvest_";

        /// <summary>What the pomegranate trees give (existing Cooking ingredients, not the attribute fruits of the Tacho), one a day, in turn.</summary>
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

        /// <summary>The Pomar's own fruit, picked from the apricot trees (since 2026-11-16).</summary>
        public const string Apricot = "item_damasco";

        public const string ApricotTreePrefix = "damasqueiro";

        /// <summary>Whether the tree <paramref name="spotId"/> is an apricot tree (it gives <see cref="Apricot"/>).</summary>
        public static bool IsApricotTree(string spotId) =>
            !string.IsNullOrEmpty(spotId) && spotId.StartsWith(ApricotTreePrefix, System.StringComparison.Ordinal);

        /// <summary>
        /// The item tree <paramref name="spotId"/> (index <paramref name="spot"/>) gives on <paramref name="day"/>: an apricot tree always its
        /// apricot, any other tree the Cooking ingredients in turn (<see cref="ItemFor(int, int)"/>).
        /// </summary>
        public static string ItemFor(string spotId, int spot, int day) => IsApricotTree(spotId) ? Apricot : ItemFor(spot, day);

        /// <summary>The Cooking ingredient of the non-apricot tree <paramref name="spot"/> on <paramref name="day"/>.</summary>
        public static string ItemFor(int spot, int day) => Items[Mathf.Abs(spot + day) % Items.Length];

        /// <summary>Whether the Vó Edna has the line after this pick (every other one); otherwise the hero does.</summary>
        public static bool GrandmaSpeaks(int spot, int day) => Mathf.Abs(spot + day) % 2 == 0;
    }
}
