using System.Collections.Generic;
using Rapadura.Gameplay.World.Map.Areas;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The gilding of the Vale dos Sátiros (map 53, Docs/Mapas100.md: "dourado: golpes das abelhas douram o herói (lento); lavar no rio
    /// (Água Viva) tira"), pure and server-runnable. Each blow of a Golden Bee leaves a layer of Midas's gold on the hero; the glow of O Ouro de
    /// Midas, when it turns a hero to gold, leaves two. Gold weighs: the hero walks slower for every layer. The Pactolo is Living Water: stepping
    /// into it washes all the gold off at once; out of it the gold flakes off slowly, one layer at a time.
    /// <code>
    /// a Golden Bee's blow: +1 layer · O Ouro de Midas's touch: +2 · at most 5 layers
    /// −6 % walking speed a layer (−30 % at 5) · one layer flakes off 15 s after the last change
    /// in the Pactolo (within 6.5 m of its middle: its 12 m of water and the wet pebbles): all off at once
    /// only in the Picos de Farnkesh (leaving the region washes it off too)
    /// </code>
    /// Decision (2026-11-16): the gold never hurts and never stops the hero (it slows, like the cold of the glacier), and it flakes off by itself
    /// so a hero far from the river is never stuck; the river is the quick way out, as the sheet asks.
    /// </summary>
    public static class PactoloGildRules
    {
        public const string GoldenBeeEnemyId = "enemy_forest_golden_bee";
        public const string MidasGoldEnemyId = "enemy_mountain_midas_gold";
        public const int MaxLayers = 5;
        public const int BeeLayers = 1;
        public const int MidasLayers = 2;
        public const float SlowPerLayer = 0.06f;
        public const float FlakeSeconds = 15f;
        public const float WashReach = 6.5f;

        /// <summary>
        /// The layers any blow from <paramref name="enemyId"/> leaves: a Golden Bee's. O Ouro de Midas gilds only with its touch
        /// (<see cref="MidasLayers"/>, signalled by its ability), not with every shot, so its id gives 0 here.
        /// </summary>
        public static int LayersFrom(string enemyId) => enemyId == GoldenBeeEnemyId ? BeeLayers : 0;

        public static int Add(int layers, int added) => Mathf.Clamp(layers + Mathf.Max(0, added), 0, MaxLayers);

        /// <summary>The factor on the walking speed of a hero under <paramref name="layers"/> of gold.</summary>
        public static float SpeedFactor(int layers) => 1f - SlowPerLayer * Mathf.Clamp(layers, 0, MaxLayers);

        /// <summary>Whether <paramref name="p"/> stands in the river (<paramref name="river"/>, the Pactolo's middle line).</summary>
        public static bool InRiver(IReadOnlyList<Vector2> river, Vector2 p) =>
            river != null && river.Count >= 2 && MapLayoutRules.DistanceToPolyline(p, river) <= WashReach;

        /// <summary>
        /// The layers after <paramref name="dt"/> seconds: all off in the river, else one off every <see cref="FlakeSeconds"/> since the last
        /// change (<paramref name="sinceChange"/>, kept by the caller and reset on any change).
        /// </summary>
        public static int Tick(int layers, bool inRiver, ref float sinceChange, float dt)
        {
            if (layers <= 0)
            {
                sinceChange = 0f;
                return 0;
            }

            if (inRiver)
            {
                sinceChange = 0f;
                return 0;
            }

            sinceChange += Mathf.Max(0f, dt);
            if (sinceChange < FlakeSeconds)
            {
                return layers;
            }

            sinceChange = 0f;
            return layers - 1;
        }
    }
}
