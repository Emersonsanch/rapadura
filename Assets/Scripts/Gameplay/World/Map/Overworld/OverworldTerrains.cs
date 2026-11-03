using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>How one <see cref="OverworldTerrain"/> looks: ground colours mixed by noise, vegetation and scatter densities.</summary>
    public sealed class OverworldTerrainStyle
    {
        public OverworldTerrain Terrain;

        /// <summary>Ground: base everywhere, alt in the noise blobs, accent in the rare spots (patches, flowers, mud).</summary>
        public TownSwatch Base = TownSwatch.Grass;
        public TownSwatch Alt = TownSwatch.GrassDark;
        public TownSwatch Accent = TownSwatch.Dirt;

        /// <summary>Small discs scattered on the ground (flowers, puddles, stones).</summary>
        public TownSwatch[] Patches = { TownSwatch.LeafGold };

        public float AltThreshold = 0.52f;
        public float AccentThreshold = 0.8f;

        /// <summary>Noise frequency of the ground blobs (1/m).</summary>
        public float NoiseScale = 1f / 38f;

        public TownSwatch Trail = TownSwatch.Dirt;
        public TownSwatch Clearing = TownSwatch.GrassDark;

        /// <summary>Which kit vegetation the trees and shrubs use, and the tree variants (TownKit.Tree).</summary>
        public TownVegetation Vegetation = TownVegetation.Forest;
        public int[] TreeVariants = { 0, 1, 2 };
        public TownVegetation? SecondVegetation;
        public int[] SecondVariants;
        public float SecondShare;

        /// <summary>Probabilities per scatter sample (one sample every <see cref="OverworldChunkBuilder.SampleSpacing"/> m).</summary>
        public float Trees = 0.1f;
        public float Bushes = 0.05f;
        public float Rocks = 0.01f;

        /// <summary>Earth mounds (Rock variant 9), the hills.</summary>
        public float Mounds;

        /// <summary>Broken dry-stone wall runs (ruins, garden walls).</summary>
        public float Walls;

        /// <summary>Hay bales (fields).</summary>
        public float Bales;

        public Vector2 TreeScale = new Vector2(0.9f, 1.4f);
        public Vector2 RockScale = new Vector2(0.7f, 1.6f);

        /// <summary>Trees planted on a lattice (orchards, mulberry rows) instead of the noise scatter.</summary>
        public bool Rows;
        public Vector2 RowSpacing = new Vector2(6.5f, 5.5f);
    }

    /// <summary>The look of every overworld terrain, and the deterministic ground colour at a point (EditMode-testable).</summary>
    public static class OverworldTerrains
    {
        private static readonly OverworldTerrainStyle[] Styles = Build();

        public static OverworldTerrainStyle For(OverworldTerrain terrain) => Styles[(int)terrain];

        /// <summary>The terrain drawn at <paramref name="p"/>: the zone found through a noise warp, so borders between areas meander.</summary>
        public static OverworldTerrain TerrainAt(OverworldLayout layout, Vector2 p)
        {
            Vector2 warped = p + Warp(p) * 34f;
            OverworldZone zone = OverworldRules.NearestZone(layout, warped);
            return zone != null ? zone.Terrain : OverworldTerrain.Meadow;
        }

        /// <summary>A smooth −1..1 offset field (two octaves of Perlin), the same everywhere for the same point.</summary>
        public static Vector2 Warp(Vector2 p)
        {
            float x = Mathf.PerlinNoise(p.x * 0.009f + 11.3f, p.y * 0.009f + 3.7f) * 2f - 1f;
            float y = Mathf.PerlinNoise(p.x * 0.009f + 71.9f, p.y * 0.009f + 29.1f) * 2f - 1f;
            x += (Mathf.PerlinNoise(p.x * 0.031f + 5.1f, p.y * 0.031f + 17.2f) * 2f - 1f) * 0.35f;
            y += (Mathf.PerlinNoise(p.x * 0.031f + 43.5f, p.y * 0.031f + 8.8f) * 2f - 1f) * 0.35f;
            return new Vector2(x, y);
        }

        /// <summary>The ground colour of <paramref name="terrain"/> at <paramref name="p"/> (noise blobs; patchwork plots for the fields).</summary>
        public static TownSwatch GroundAt(OverworldTerrain terrain, Vector2 p)
        {
            OverworldTerrainStyle style = For(terrain);
            if (terrain == OverworldTerrain.Fields)
            {
                return FieldPlot(p);
            }

            float f = style.NoiseScale;
            float n = Mathf.PerlinNoise(p.x * f + 31.7f * (int)terrain, p.y * f + 7.3f) * 0.7f
                + Mathf.PerlinNoise(p.x * f * 2.7f + 3.1f, p.y * f * 2.7f + 12.9f * (int)terrain) * 0.3f;
            if (n >= style.AccentThreshold)
            {
                return style.Accent;
            }

            return n >= style.AltThreshold ? style.Alt : style.Base;
        }

        /// <summary>The fields: plots of ~28 × 14 m (rows of plots slightly offset), each a crop or fallow, lanes between them.</summary>
        public static TownSwatch FieldPlot(Vector2 p)
        {
            const float w = 28f;
            const float h = 14f;
            int row = Mathf.FloorToInt(p.y / h);
            float shifted = p.x + (row & 1) * 11f + Mathf.PerlinNoise(row * 0.37f, 2.1f) * 9f;
            int col = Mathf.FloorToInt(shifted / w);
            float u = shifted - col * w;
            float v = p.y - row * h;
            if (u < 1.2f || v < 1f)
            {
                return TownSwatch.DirtDark;
            }

            uint hash = TownStyle.Hash("plot", col * 7919 + row * 104729);
            switch (hash % 6)
            {
                case 0: return TownSwatch.Thatch;
                case 1: return TownSwatch.LeafGold;
                case 2: return TownSwatch.Vine;
                case 3: return TownSwatch.Dirt;
                case 4: return TownSwatch.Grass;
                default: return TownSwatch.Poplar;
            }
        }

        private static OverworldTerrainStyle[] Build()
        {
            var styles = new OverworldTerrainStyle[15];
            styles[(int)OverworldTerrain.Orchard] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Orchard, Base = TownSwatch.Grass, Alt = TownSwatch.Poplar, Accent = TownSwatch.Dirt,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.Fruit, TownSwatch.GrassDark }, AltThreshold = 0.56f, AccentThreshold = 0.82f,
                TreeVariants = new[] { 3 }, Trees = 0.82f, Bushes = 0.04f, Rocks = 0.004f, Rows = true, RowSpacing = new Vector2(6.5f, 5.5f),
                TreeScale = new Vector2(0.85f, 1.2f),
            };
            styles[(int)OverworldTerrain.Fields] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Fields, Base = TownSwatch.Thatch, Alt = TownSwatch.Vine, Accent = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.Fruit }, TreeVariants = new[] { 0, 3 }, Trees = 0.012f, Bushes = 0.02f,
                Rocks = 0.002f, Bales = 0.018f, Clearing = TownSwatch.Dirt,
            };
            styles[(int)OverworldTerrain.Steppe] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Steppe, Base = TownSwatch.Thatch, Alt = TownSwatch.SandDark, Accent = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Dirt, TownSwatch.Sand }, AltThreshold = 0.5f, AccentThreshold = 0.74f,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1, 2 }, Trees = 0.012f, Bushes = 0.05f, Rocks = 0.02f,
                Clearing = TownSwatch.SandDark, Trail = TownSwatch.SandDark,
            };
            styles[(int)OverworldTerrain.Meadow] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Meadow, Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.LeafGold,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.ClothYellow, TownSwatch.Fruit, TownSwatch.Tamarisk }, AltThreshold = 0.5f,
                AccentThreshold = 0.76f, TreeVariants = new[] { 0, 1 }, Trees = 0.018f, Bushes = 0.03f, Rocks = 0.006f,
                TreeScale = new Vector2(1.1f, 1.6f),
            };
            styles[(int)OverworldTerrain.Riverbank] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Riverbank, Base = TownSwatch.GrassDark, Alt = TownSwatch.Grass, Accent = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Tamarisk, TownSwatch.LeafGold }, TreeVariants = new[] { 0, 1, 1, 2, 2 }, Trees = 0.16f, Bushes = 0.06f,
                Rocks = 0.01f, TreeScale = new Vector2(1.05f, 1.6f),
            };
            styles[(int)OverworldTerrain.Floodplain] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Floodplain, Base = TownSwatch.Vine, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Water, TownSwatch.Mud, TownSwatch.Tamarisk }, AltThreshold = 0.48f, AccentThreshold = 0.74f,
                TreeVariants = new[] { 2, 2, 1 }, Trees = 0.06f, Bushes = 0.14f, Rocks = 0.004f,
            };
            styles[(int)OverworldTerrain.Hills] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Hills, Base = TownSwatch.GrassDark, Alt = TownSwatch.Grass, Accent = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Stone, TownSwatch.LeafGold }, Vegetation = TownVegetation.Mountain, TreeVariants = new[] { 0, 2 },
                SecondVegetation = TownVegetation.Forest, SecondVariants = new[] { 0 }, SecondShare = 0.4f,
                Trees = 0.05f, Bushes = 0.04f, Rocks = 0.04f, Mounds = 0.035f, RockScale = new Vector2(1f, 2.4f),
            };
            styles[(int)OverworldTerrain.HighHills] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HighHills, Base = TownSwatch.DirtDark, Alt = TownSwatch.Stone, Accent = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.StoneDark, TownSwatch.Thatch }, AltThreshold = 0.55f, Vegetation = TownVegetation.Mountain,
                TreeVariants = new[] { 0, 2 }, Trees = 0.035f, Bushes = 0.05f, Rocks = 0.09f, Mounds = 0.03f, RockScale = new Vector2(1.2f, 3.4f),
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Dirt,
            };
            styles[(int)OverworldTerrain.Ruins] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Ruins, Base = TownSwatch.SandDark, Alt = TownSwatch.Cobble, Accent = TownSwatch.Sandstone,
                Patches = new[] { TownSwatch.Paving, TownSwatch.PavingDark, TownSwatch.Grass }, AltThreshold = 0.55f,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1, 2 }, Trees = 0.025f, Bushes = 0.04f, Rocks = 0.05f, Walls = 0.025f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Dirt,
            };
            styles[(int)OverworldTerrain.DenseWood] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DenseWood, Base = TownSwatch.LeafDark, Alt = TownSwatch.GrassDark, Accent = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Trunk }, AltThreshold = 0.45f, TreeVariants = new[] { 0, 0, 2 },
                SecondVegetation = TownVegetation.Mountain, SecondVariants = new[] { 0 }, SecondShare = 0.3f,
                Trees = 0.32f, Bushes = 0.1f, Rocks = 0.012f, TreeScale = new Vector2(1.2f, 1.9f),
            };
            styles[(int)OverworldTerrain.Ravine] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Ravine, Base = TownSwatch.StoneDark, Alt = TownSwatch.Basalt, Accent = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Stone }, AltThreshold = 0.5f, Vegetation = TownVegetation.Mountain,
                TreeVariants = new[] { 0 }, Trees = 0.025f, Bushes = 0.03f, Rocks = 0.12f, Mounds = 0.02f, RockScale = new Vector2(1.4f, 4f),
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Dirt,
            };
            styles[(int)OverworldTerrain.Marsh] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Marsh, Base = TownSwatch.Mud, Alt = TownSwatch.Vine, Accent = TownSwatch.Water,
                Patches = new[] { TownSwatch.Water, TownSwatch.DirtDark, TownSwatch.Thatch }, AltThreshold = 0.5f, AccentThreshold = 0.78f,
                TreeVariants = new[] { 2 }, Trees = 0.04f, Bushes = 0.16f, Rocks = 0.008f, TreeScale = new Vector2(0.7f, 1.1f),
                Clearing = TownSwatch.DirtDark, Trail = TownSwatch.WoodWeathered,
            };
            styles[(int)OverworldTerrain.Mulberry] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Mulberry, Base = TownSwatch.GrassDark, Alt = TownSwatch.Poplar, Accent = TownSwatch.Violet,
                Patches = new[] { TownSwatch.Violet, TownSwatch.ClothWhite }, AccentThreshold = 0.84f, TreeVariants = new[] { 3, 3, 0 },
                Trees = 0.7f, Bushes = 0.05f, Rocks = 0.006f, Rows = true, RowSpacing = new Vector2(7.5f, 7f), TreeScale = new Vector2(1f, 1.35f),
            };
            styles[(int)OverworldTerrain.Canopy] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Canopy, Base = TownSwatch.LeafDark, Alt = TownSwatch.Pine, Accent = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Vine }, AltThreshold = 0.5f, TreeVariants = new[] { 0, 0, 2 },
                SecondVegetation = TownVegetation.Mountain, SecondVariants = new[] { 0 }, SecondShare = 0.25f,
                Trees = 0.26f, Bushes = 0.14f, Rocks = 0.01f, TreeScale = new Vector2(1.7f, 2.6f),
            };
            styles[(int)OverworldTerrain.Garden] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Garden, Base = TownSwatch.Grass, Alt = TownSwatch.PavingDark, Accent = TownSwatch.Paving,
                Patches = new[] { TownSwatch.Tamarisk, TownSwatch.ClothRed, TownSwatch.Turquoise }, AltThreshold = 0.6f, AccentThreshold = 0.78f,
                TreeVariants = new[] { 0 }, Trees = 0.08f, Bushes = 0.1f, Rocks = 0.01f, Walls = 0.02f, TreeScale = new Vector2(1f, 1.3f),
                Trail = TownSwatch.Paving,
            };
            return styles;
        }
    }
}
