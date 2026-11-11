using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What the Interact button does at a tower of the great wall (<see cref="MingchengWallRules.ActionAt"/>).</summary>
    public enum MingchengWallAction
    {
        None = 0,
        Climb = 1,
        LightBeacon = 2,
        Descend = 3,
    }

    /// <summary>
    /// The walk on top of the Muralha das Mil Torres (Docs/Mapas100.md, map 45: "a muralha é o caminho"), pure and in the region's own coordinates.
    /// The deck is cut in pieces of <see cref="Step"/> m along <see cref="OverworldLayouts.GreatWallZ"/>, the same grid the wall's runs stand on, and
    /// its height is the ground under it plus <see cref="DeckHeight"/>, so it follows the crest. Numbers:
    /// <code>
    /// deck 3.4 m over the ground, 3.4 m wide · from x −1140 to 708 · 29 pieces around the hero (±84 m)
    /// a tower's stair: within 7 m of its centre · the way down lands 5.5 m south of the wall
    /// </code>
    /// Nothing can hold the hero up here: the sides are open, stepping off is a 3.4 m drop with no damage, and every tower has the stair down.
    /// </summary>
    public static class MingchengWallRules
    {
        public const float Step = 6f;
        public const float DeckHeight = 3.4f;
        public const float DeckWidth = 3.4f;
        public const float MinX = -1140f;
        public const float MaxX = 708f;
        public const int WindowPieces = 14;
        public const float StairReach = 7f;
        public const float FootOffset = 5.5f;

        /// <summary>How far (m) off the line the deck is kept alive for a hero walking along or under the wall.</summary>
        public const float ActiveRange = 40f;

        /// <summary>Slack (m) on the hero's height and on the deck's sides when deciding whether they stand on it.</summary>
        public const float StandSlack = 1f;

        /// <summary>The first and last valid piece (a piece runs from its index to the next).</summary>
        public static int MinIndex => Mathf.CeilToInt(MinX / Step);

        public static int MaxIndex => Mathf.FloorToInt(MaxX / Step) - 1;

        /// <summary>The wall's line at grid point <paramref name="index"/> (region coordinates).</summary>
        public static Vector2 PointOf(int index)
        {
            float x = index * Step;
            return new Vector2(x, OverworldLayouts.GreatWallZ(x));
        }

        /// <summary>The wall's line at <paramref name="x"/> (region coordinates).</summary>
        public static Vector2 LineAt(float x) => new Vector2(x, OverworldLayouts.GreatWallZ(x));

        /// <summary>The piece under <paramref name="x"/>, kept on the deck.</summary>
        public static int PieceAt(float x) => Mathf.Clamp(Mathf.FloorToInt(x / Step), MinIndex, MaxIndex);

        /// <summary>Whether <paramref name="p"/> is within <paramref name="range"/> m of the deck's line, along the deck's length.</summary>
        public static bool NearLine(Vector2 p, float range) => p.x >= MinX - range && p.x <= MaxX + range && Mathf.Abs(p.y - OverworldLayouts.GreatWallZ(Mathf.Clamp(p.x, MinX, MaxX))) <= range;

        /// <summary>Whether a hero at <paramref name="p"/> and height <paramref name="heroY"/> stands on the deck, the ground under them being <paramref name="groundY"/>.</summary>
        public static bool OnDeck(Vector2 p, float heroY, float groundY) =>
            p.x >= MinX && p.x <= MaxX
            && Mathf.Abs(p.y - OverworldLayouts.GreatWallZ(p.x)) <= DeckWidth * 0.5f + StandSlack
            && Mathf.Abs(heroY - (groundY + DeckHeight)) <= StandSlack;

        /// <summary>The nearest tower within the stair's reach of <paramref name="p"/>, or −1.</summary>
        public static int TowerAt(IReadOnlyList<float> towerXs, Vector2 p)
        {
            int best = -1;
            float bestDistance = StairReach * StairReach;
            for (int i = 0; i < towerXs.Count; i++)
            {
                float distance = (LineAt(towerXs[i]) - p).sqrMagnitude;
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            return best;
        }

        /// <summary>Where the way down from the tower at <paramref name="x"/> lands: south of the wall, square to its line.</summary>
        public static Vector2 FootOf(float x)
        {
            Vector2 along = (LineAt(x + 1f) - LineAt(x - 1f)).normalized;
            return LineAt(x) + new Vector2(along.y, -along.x) * FootOffset;
        }

        /// <summary>What Interact does at a tower: from the ground it climbs; on the deck it lights a dark beacon, and from a lit one it goes down.</summary>
        public static MingchengWallAction ActionAt(bool atTower, bool onDeck, bool lit)
        {
            if (!atTower)
            {
                return MingchengWallAction.None;
            }

            if (!onDeck)
            {
                return MingchengWallAction.Climb;
            }

            return lit ? MingchengWallAction.Descend : MingchengWallAction.LightBeacon;
        }
    }
}
