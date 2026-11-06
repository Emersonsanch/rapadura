using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// How one <see cref="OverworldTerrain"/> looks: its relief, its ground colours (mixed per facet by noise), its vegetation
    /// and the densities of everything scattered on it (trees, shrubs, rocks, logs, grass tufts, flowers, reeds…).
    /// </summary>
    public sealed class OverworldTerrainStyle
    {
        public OverworldTerrain Terrain;

        // ---------------- Relief (OverworldRelief) ----------------

        /// <summary>Height (m) of the relief at its highest.</summary>
        public float Relief = 1f;

        /// <summary>Wavelength (m) of the relief's swells.</summary>
        public float ReliefScale = 160f;

        /// <summary>Amplitude (m) of the small bumps on top (off the trails).</summary>
        public float ReliefDetail = 0.2f;

        public ReliefShape Shape = ReliefShape.Rolling;
        public int TerraceSteps = 3;

        // ---------------- Ground ----------------

        /// <summary>Ground: base everywhere, alt in the noise blobs, accent in the rare spots (patches, flowers, mud).</summary>
        public TownSwatch Base = TownSwatch.Grass;
        public TownSwatch Alt = TownSwatch.GrassDark;
        public TownSwatch Accent = TownSwatch.Dirt;

        /// <summary>Kept for the colour variety checks: the accents the ground mixes in.</summary>
        public TownSwatch[] Patches = { TownSwatch.LeafGold };

        public float AltThreshold = 0.52f;
        public float AccentThreshold = 0.8f;

        /// <summary>Noise frequency of the ground blobs (1/m).</summary>
        public float NoiseScale = 1f / 38f;

        /// <summary>A close shade mottled into the base in small patches (a few facets), when set.</summary>
        public TownSwatch? Mottle;

        public TownSwatch Trail = TownSwatch.Dirt;
        public TownSwatch Clearing = TownSwatch.GrassDark;

        /// <summary>Steep facets (bare earth or rock) and half-steep ones.</summary>
        public TownSwatch Cliff = TownSwatch.DirtDark;
        public TownSwatch Slope = TownSwatch.GrassDark;

        /// <summary>River banks and beds.</summary>
        public TownSwatch Bank = TownSwatch.Sand;
        public TownSwatch Bed = TownSwatch.SandDark;

        /// <summary>The tops of the relief (above ~60 % of <see cref="Relief"/>), when set.</summary>
        public TownSwatch? Crest;

        /// <summary>The hollows of the relief (below ~18 % of <see cref="Relief"/>), when set.</summary>
        public TownSwatch? Hollow;

        // ---------------- Vegetation and scatter ----------------

        /// <summary>Which kit vegetation the trees and shrubs use, and the tree variants (TownKit.Tree).</summary>
        public TownVegetation Vegetation = TownVegetation.Forest;

        /// <summary>The shrubs' flora when it differs from the trees' (null = <see cref="Vegetation"/>).</summary>
        public TownVegetation? BushVegetation;

        public int[] TreeVariants = { 0, 1, 2 };
        public TownVegetation? SecondVegetation;
        public int[] SecondVariants;
        public float SecondShare;

        /// <summary>Probabilities per scatter sample (one sample every <see cref="OverworldChunkBuilder.SampleSpacing"/> m).</summary>
        public float Trees = 0.1f;
        public float Bushes = 0.05f;
        public float Rocks = 0.01f;

        /// <summary>Earth mounds (Rock variant 9).</summary>
        public float Mounds;

        /// <summary>Broken dry-stone wall runs (ruins, garden walls).</summary>
        public float Walls;

        /// <summary>Hay bales (fields).</summary>
        public float Bales;

        /// <summary>Solid dressing: fallen logs, stumps, mossy rocks and the area's own piece (<see cref="Special"/>).</summary>
        public float Logs;
        public float Stumps;
        public float MossRocks;
        public float Specials;
        public OverworldDecor Special = OverworldDecor.None;

        /// <summary>Ground detail per detail sample (one every <see cref="OverworldChunkBuilder.DetailSpacing"/> m): no colliders.</summary>
        public float Tufts = 0.2f;
        public float Flowers;
        public float Reeds;
        public float Pebbles = 0.01f;
        public float Ferns;
        public float Puddles;

        public TownSwatch TuftColor = TownSwatch.GrassDark;
        public TownSwatch TuftTip = TownSwatch.Grass;
        public TownSwatch[] FlowerColors = { TownSwatch.ClothYellow, TownSwatch.ClothWhite };

        public Vector2 TreeScale = new Vector2(0.9f, 1.4f);
        public Vector2 RockScale = new Vector2(0.7f, 1.6f);

        /// <summary>Trees planted on a lattice (orchards, mulberry rows) instead of the noise scatter.</summary>
        public bool Rows;
        public Vector2 RowSpacing = new Vector2(6.5f, 5.5f);

        /// <summary>Several own pieces drawn in turn instead of <see cref="Special"/> (null = only <see cref="Special"/>).</summary>
        public OverworldDecor[] SpecialMix;

        /// <summary>The trees along the water (null = the tugai's tamarisks and poplars), and their variants.</summary>
        public TownVegetation? WaterVegetation;
        public int[] WaterVariants;

        /// <summary>The colour of the scattered rocks and pebbles (null = the default per terrain).</summary>
        public TownSwatch? RockTint;
    }

    /// <summary>The crop of one plot of the Campos do Aryk.</summary>
    public enum FieldCrop
    {
        Wheat = 0,
        Cotton = 1,
        Vines = 2,
        Melons = 3,
        Fallow = 4,
        Hay = 5,
        Plowed = 6,
    }

    /// <summary>Where a point falls in the field lattice: its plot, the plot's crop and the local position in it.</summary>
    public struct FieldPlotInfo
    {
        public int Col;
        public int Row;
        public FieldCrop Crop;

        /// <summary>World rectangle of the plot (lanes included on its low x and low z side).</summary>
        public Rect Rect;

        /// <summary>Position inside the plot (m from its low corner).</summary>
        public float U;
        public float V;

        /// <summary>On the lane between plots.</summary>
        public bool Lane;
    }

    /// <summary>The look of every overworld terrain, and the deterministic ground colour at a point (EditMode-testable).</summary>
    public static partial class OverworldTerrains
    {
        /// <summary>Field plots: width (x) and depth (z) in m, lane widths.</summary>
        public const float PlotWidth = 28f;
        public const float PlotDepth = 14f;
        public const float LaneX = 1.2f;
        public const float LaneZ = 1f;

        private static readonly OverworldTerrainStyle[] Styles = Build();

        public static OverworldTerrainStyle For(OverworldTerrain terrain) => Styles[(int)terrain];

        /// <summary>The terrain drawn at <paramref name="p"/>: the zone found through a noise warp, so borders between areas meander.</summary>
        public static OverworldTerrain TerrainAt(OverworldLayout layout, Vector2 p)
        {
            Vector2 warped = p + Warp(p) * 34f;
            OverworldZone zone = OverworldRules.NearestZone(layout, warped);
            return zone != null ? zone.Terrain : OverworldTerrain.Meadow;
        }

        /// <summary>A smooth −1..1 offset field (two octaves of gradient noise), the same everywhere for the same point.</summary>
        public static Vector2 Warp(Vector2 p)
        {
            float x = OverworldNoise.Signed(p.x * 0.009f + 11.3f, p.y * 0.009f + 3.7f);
            float y = OverworldNoise.Signed(p.x * 0.009f + 71.9f, p.y * 0.009f + 29.1f);
            x += OverworldNoise.Signed(p.x * 0.031f + 5.1f, p.y * 0.031f + 17.2f) * 0.35f;
            y += OverworldNoise.Signed(p.x * 0.031f + 43.5f, p.y * 0.031f + 8.8f) * 0.35f;
            return new Vector2(x, y);
        }

        /// <summary>The ground-colour noise of <paramref name="terrain"/> at <paramref name="p"/> (0..1).</summary>
        public static float Tone(OverworldTerrain terrain, Vector2 p)
        {
            float f = For(terrain).NoiseScale;
            return OverworldNoise.Perlin(p.x * f + 31.7f * (int)terrain, p.y * f + 7.3f) * 0.7f
                + OverworldNoise.Perlin(p.x * f * 2.7f + 3.1f, p.y * f * 2.7f + 12.9f * (int)terrain) * 0.3f;
        }

        /// <summary>The ground colour of <paramref name="terrain"/> at <paramref name="p"/> (noise blobs; the plots for the fields).</summary>
        public static TownSwatch GroundAt(OverworldTerrain terrain, Vector2 p) => GroundFor(terrain, p, Tone(terrain, p), Fine(p));

        /// <summary>Fine mottling noise (patches of a few metres), 0..1.</summary>
        public static float Fine(Vector2 p) => OverworldNoise.Perlin(p.x / 7.5f + 91.3f, p.y / 7.5f + 17.9f);

        /// <summary>The ground colour for a given tone (the facets mix the tone of their corners with a little dither).</summary>
        public static TownSwatch GroundFor(OverworldTerrain terrain, Vector2 p, float tone, float fine = 0.5f)
        {
            OverworldTerrainStyle style = For(terrain);
            if (terrain == OverworldTerrain.Fields)
            {
                return PlotGround(PlotAt(p), tone);
            }

            if (tone >= style.AccentThreshold)
            {
                return style.Accent;
            }

            if (tone >= style.AltThreshold)
            {
                return style.Alt;
            }

            return style.Mottle.HasValue && fine > 0.58f ? style.Mottle.Value : style.Base;
        }

        /// <summary>The fields: plots of 28 × 14 m (rows of plots slightly offset), each with its crop, lanes between them.</summary>
        public static FieldPlotInfo PlotAt(Vector2 p)
        {
            int row = Mathf.FloorToInt(p.y / PlotDepth);
            float shift = (row & 1) * 11f + OverworldNoise.Perlin(row * 0.37f, 2.1f) * 9f;
            float shifted = p.x + shift;
            int col = Mathf.FloorToInt(shifted / PlotWidth);
            float u = shifted - col * PlotWidth;
            float v = p.y - row * PlotDepth;
            uint hash = TownStyle.Hash("plot", col * 7919 + row * 104729);
            FieldCrop crop;
            switch (hash % 9)
            {
                case 0:
                case 1: crop = FieldCrop.Wheat; break;
                case 2: crop = FieldCrop.Cotton; break;
                case 3:
                case 4: crop = FieldCrop.Vines; break;
                case 5: crop = FieldCrop.Melons; break;
                case 6: crop = FieldCrop.Fallow; break;
                case 7: crop = FieldCrop.Hay; break;
                default: crop = FieldCrop.Plowed; break;
            }

            return new FieldPlotInfo
            {
                Col = col, Row = row, Crop = crop, U = u, V = v, Lane = u < LaneX || v < LaneZ,
                Rect = new Rect(col * PlotWidth - shift, row * PlotDepth, PlotWidth, PlotDepth),
            };
        }

        /// <summary>The bare ground of a plot (the crop rows stand on it as their own meshes).</summary>
        public static TownSwatch PlotGround(FieldPlotInfo plot, float tone)
        {
            switch (plot.Crop)
            {
                case FieldCrop.Wheat: return tone > 0.62f ? TownSwatch.LeafGold : TownSwatch.Thatch;
                case FieldCrop.Cotton: return tone > 0.6f ? TownSwatch.DirtDark : TownSwatch.Dirt;
                case FieldCrop.Vines: return tone > 0.55f ? TownSwatch.Grass : TownSwatch.Dirt;
                case FieldCrop.Melons: return tone > 0.58f ? TownSwatch.Vine : TownSwatch.Dirt;
                case FieldCrop.Fallow: return tone > 0.55f ? TownSwatch.GrassDark : TownSwatch.Grass;
                case FieldCrop.Hay: return tone > 0.6f ? TownSwatch.SandDark : TownSwatch.Thatch;
                default: return tone > 0.6f ? TownSwatch.Mud : TownSwatch.DirtDark;
            }
        }

        private static OverworldTerrainStyle[] Build()
        {
            // Room for the regions' own terrains (Região 4: 40–59, OverworldTerrains.Caves.cs; … Região 7: 120–139, OverworldTerrains.Pamir.cs;
            // Região 8: 140–159, OverworldTerrains.Mingcheng.cs; Região 9: 160–179, OverworldTerrains.Nartes.cs;
            // Região 10: 180–199, OverworldTerrains.Farnkesh.cs; Região 11: 200–219, OverworldTerrains.MarGelado.cs).
            var styles = new OverworldTerrainStyle[220];
            styles[(int)OverworldTerrain.Orchard] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Orchard, Mottle = TownSwatch.Vine, Relief = 2.6f, ReliefScale = 200f, ReliefDetail = 0.2f, Hollow = TownSwatch.GrassDark,
                Base = TownSwatch.Grass, Alt = TownSwatch.Poplar, Accent = TownSwatch.Vine,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.Fruit, TownSwatch.GrassDark }, AltThreshold = 0.56f, AccentThreshold = 0.8f,
                TreeVariants = new[] { 3 }, Trees = 0.82f, Bushes = 0.04f, Rocks = 0.004f, Rows = true, RowSpacing = new Vector2(6.5f, 5.5f),
                TreeScale = new Vector2(0.85f, 1.2f), Stumps = 0.004f,
                Tufts = 0.27f, Flowers = 0.07f, FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Tamarisk, TownSwatch.Fruit },
                TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Poplar,
            };
            styles[(int)OverworldTerrain.Fields] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Fields, Relief = 1.2f, ReliefScale = 260f, ReliefDetail = 0.05f,
                Base = TownSwatch.Thatch, Alt = TownSwatch.Vine, Accent = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.Fruit }, TreeVariants = new[] { 0, 3 }, Trees = 0.012f, Bushes = 0.02f,
                Rocks = 0.002f, Bales = 0.03f, Clearing = TownSwatch.Dirt,
                Tufts = 0.1f, Flowers = 0.03f, FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow },
            };
            styles[(int)OverworldTerrain.Steppe] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Steppe, Mottle = TownSwatch.LeafGold, Relief = 7f, ReliefScale = 260f, ReliefDetail = 0.5f, Hollow = TownSwatch.Grass,
                Base = TownSwatch.Thatch, Alt = TownSwatch.SandDark, Accent = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Dirt, TownSwatch.Sand }, AltThreshold = 0.5f, AccentThreshold = 0.74f,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1, 2 }, Trees = 0.012f, Bushes = 0.07f, Rocks = 0.025f,
                Clearing = TownSwatch.SandDark, Trail = TownSwatch.SandDark, Cliff = TownSwatch.SandDark, Slope = TownSwatch.Sand,
                Crest = TownSwatch.Sand, Tufts = 0.34f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.04f,
                Flowers = 0.02f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothYellow },
            };
            styles[(int)OverworldTerrain.Meadow] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Meadow, Mottle = TownSwatch.Poplar, Relief = 5.5f, ReliefScale = 200f, ReliefDetail = 0.5f, Crest = TownSwatch.LeafGold, Hollow = TownSwatch.GrassDark,
                Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.LeafGold,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.ClothYellow, TownSwatch.Fruit, TownSwatch.Tamarisk }, AltThreshold = 0.5f,
                AccentThreshold = 0.76f, TreeVariants = new[] { 0, 1 }, Trees = 0.02f, Bushes = 0.04f, Rocks = 0.006f,
                TreeScale = new Vector2(1.1f, 1.6f), MossRocks = 0.003f,
                Tufts = 0.37f, Flowers = 0.22f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Grass,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite, TownSwatch.ClothRed, TownSwatch.Violet, TownSwatch.Tamarisk },
            };
            styles[(int)OverworldTerrain.Riverbank] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Riverbank, Mottle = TownSwatch.Vine, Relief = 2.2f, ReliefScale = 170f, ReliefDetail = 0.25f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Grass, Accent = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Tamarisk, TownSwatch.LeafGold }, TreeVariants = new[] { 0, 1, 1, 2, 2 }, Trees = 0.16f, Bushes = 0.07f,
                Rocks = 0.01f, TreeScale = new Vector2(1.05f, 1.6f), Logs = 0.006f, Stumps = 0.004f,
                Tufts = 0.31f, Flowers = 0.06f, Reeds = 0.02f, Pebbles = 0.02f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothWhite },
            };
            styles[(int)OverworldTerrain.Floodplain] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Floodplain, Mottle = TownSwatch.Grass, Relief = 0.5f, ReliefScale = 150f, ReliefDetail = 0.12f,
                Base = TownSwatch.Vine, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Water, TownSwatch.Mud, TownSwatch.Tamarisk }, AltThreshold = 0.48f, AccentThreshold = 0.74f,
                TreeVariants = new[] { 2, 2, 1 }, Trees = 0.06f, Bushes = 0.14f, Rocks = 0.004f, Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Logs = 0.006f, Tufts = 0.27f, Reeds = 0.14f, Puddles = 0.03f, Flowers = 0.03f, TuftColor = TownSwatch.Vine,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Tamarisk },
            };
            styles[(int)OverworldTerrain.Hills] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Hills, Mottle = TownSwatch.Poplar, Relief = 16f, ReliefScale = 190f, ReliefDetail = 1.1f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.GrassDark, Hollow = TownSwatch.GrassDark, BushVegetation = TownVegetation.Forest,
                Patches = new[] { TownSwatch.Stone, TownSwatch.LeafGold }, Vegetation = TownVegetation.Mountain, TreeVariants = new[] { 0, 2 },
                SecondVegetation = TownVegetation.Forest, SecondVariants = new[] { 0, 1 }, SecondShare = 0.55f, Cliff = TownSwatch.DirtDark,
                Slope = TownSwatch.GrassDark, Crest = TownSwatch.LeafGold,
                Trees = 0.05f, Bushes = 0.05f, Rocks = 0.04f, RockScale = new Vector2(1f, 2.4f), MossRocks = 0.01f,
                Tufts = 0.31f, Flowers = 0.08f, Pebbles = 0.03f, FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothYellow },
            };
            styles[(int)OverworldTerrain.HighHills] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HighHills, Mottle = TownSwatch.LeafGold, Relief = 22f, ReliefScale = 230f, ReliefDetail = 0.9f, Shape = ReliefShape.Terraced,
                Base = TownSwatch.Thatch, Alt = TownSwatch.SandDark, Accent = TownSwatch.GrassDark, Hollow = TownSwatch.GrassDark,
                BushVegetation = TownVegetation.Desert,
                Patches = new[] { TownSwatch.StoneDark, TownSwatch.Thatch }, AltThreshold = 0.55f, Vegetation = TownVegetation.Mountain,
                TreeVariants = new[] { 0, 2 }, Trees = 0.03f, Bushes = 0.05f, Rocks = 0.08f, RockScale = new Vector2(1.2f, 3.4f),
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Stone, Slope = TownSwatch.DirtDark, Crest = TownSwatch.LeafGold,
                Specials = 0.01f, Special = OverworldDecor.Cairn, Tufts = 0.27f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark,
                Pebbles = 0.05f, Flowers = 0.03f, FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Violet },
            };
            styles[(int)OverworldTerrain.Ruins] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Ruins, Mottle = TownSwatch.Thatch, Relief = 5f, ReliefScale = 140f, ReliefDetail = 0.3f, Shape = ReliefShape.Mounds, Crest = TownSwatch.Grass,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Thatch, Accent = TownSwatch.Cobble,
                Patches = new[] { TownSwatch.Paving, TownSwatch.PavingDark, TownSwatch.Grass }, AltThreshold = 0.55f, AccentThreshold = 0.78f,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1, 2 }, Trees = 0.025f, Bushes = 0.05f, Rocks = 0.04f, Walls = 0.03f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Cobble, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Dirt,
                Specials = 0.025f, Special = OverworldDecor.Column, Tufts = 0.24f, TuftColor = TownSwatch.Thatch, Pebbles = 0.06f,
                Flowers = 0.04f, FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow },
            };
            styles[(int)OverworldTerrain.DenseWood] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DenseWood, Mottle = TownSwatch.Pine, Relief = 6f, ReliefScale = 170f, ReliefDetail = 0.8f,
                Base = TownSwatch.LeafDark, Alt = TownSwatch.GrassDark, Accent = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Trunk }, AltThreshold = 0.45f, TreeVariants = new[] { 0, 0, 2 },
                SecondVegetation = TownVegetation.Mountain, SecondVariants = new[] { 0 }, SecondShare = 0.3f, Cliff = TownSwatch.DirtDark,
                Trees = 0.3f, Bushes = 0.1f, Rocks = 0.01f, TreeScale = new Vector2(1.2f, 1.9f), Logs = 0.02f, Stumps = 0.015f, MossRocks = 0.012f,
                Tufts = 0.17f, Ferns = 0.16f, TuftColor = TownSwatch.LeafDark, TuftTip = TownSwatch.GrassDark,
            };
            styles[(int)OverworldTerrain.Ravine] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Ravine, Mottle = TownSwatch.Pine, Relief = 15f, ReliefScale = 170f, ReliefDetail = 0.7f, Shape = ReliefShape.Gorges,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Vine, Hollow = TownSwatch.Basalt,
                BushVegetation = TownVegetation.Forest,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Stone }, AltThreshold = 0.55f, Vegetation = TownVegetation.Mountain,
                TreeVariants = new[] { 0 }, Trees = 0.03f, Bushes = 0.04f, Rocks = 0.045f, RockScale = new Vector2(1.4f, 4f),
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Stone, Crest = TownSwatch.GrassDark,
                Specials = 0.03f, Special = OverworldDecor.Basalt, MossRocks = 0.015f, Tufts = 0.17f, Ferns = 0.05f, Pebbles = 0.05f,
                TuftColor = TownSwatch.GrassDark,
            };
            styles[(int)OverworldTerrain.Marsh] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Marsh, Mottle = TownSwatch.DirtDark, Relief = 0.4f, ReliefScale = 90f, ReliefDetail = 0.1f,
                Base = TownSwatch.Mud, Alt = TownSwatch.Vine, Accent = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Water, TownSwatch.DirtDark, TownSwatch.Thatch }, AltThreshold = 0.5f, AccentThreshold = 0.78f,
                TreeVariants = new[] { 2 }, Trees = 0.03f, Bushes = 0.14f, Rocks = 0.008f, TreeScale = new Vector2(0.7f, 1.1f),
                Clearing = TownSwatch.DirtDark, Trail = TownSwatch.WoodWeathered, Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Specials = 0.02f, Special = OverworldDecor.DeadTree, Stumps = 0.01f, Tufts = 0.2f, Reeds = 0.2f, Puddles = 0.04f,
                TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Thatch,
            };
            styles[(int)OverworldTerrain.Mulberry] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Mulberry, Mottle = TownSwatch.LeafDark, Relief = 2.6f, ReliefScale = 180f, ReliefDetail = 0.25f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Poplar, Accent = TownSwatch.Violet,
                Patches = new[] { TownSwatch.Violet, TownSwatch.ClothWhite }, AccentThreshold = 0.84f, TreeVariants = new[] { 3, 3, 0 },
                Trees = 0.7f, Bushes = 0.05f, Rocks = 0.006f, Rows = true, RowSpacing = new Vector2(7.5f, 7f), TreeScale = new Vector2(1f, 1.35f),
                Tufts = 0.27f, Flowers = 0.08f, FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothWhite }, Stumps = 0.004f,
            };
            styles[(int)OverworldTerrain.Canopy] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Canopy, Mottle = TownSwatch.GrassDark, Relief = 8f, ReliefScale = 180f, ReliefDetail = 0.8f,
                Base = TownSwatch.LeafDark, Alt = TownSwatch.Pine, Accent = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Vine }, AltThreshold = 0.5f, TreeVariants = new[] { 0, 0, 2 },
                SecondVegetation = TownVegetation.Mountain, SecondVariants = new[] { 0 }, SecondShare = 0.25f,
                Trees = 0.24f, Bushes = 0.14f, Rocks = 0.01f, TreeScale = new Vector2(1.7f, 2.6f), Logs = 0.02f, MossRocks = 0.02f, Stumps = 0.006f,
                Ferns = 0.22f, Tufts = 0.1f, TuftColor = TownSwatch.Pine,
            };
            styles[(int)OverworldTerrain.Garden] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Garden, Mottle = TownSwatch.Poplar, Relief = 2.4f, ReliefScale = 150f, ReliefDetail = 0.05f, Shape = ReliefShape.Terraced,
                TerraceSteps = 2, Base = TownSwatch.Grass, Alt = TownSwatch.PavingDark, Accent = TownSwatch.Paving,
                Patches = new[] { TownSwatch.Tamarisk, TownSwatch.ClothRed, TownSwatch.Turquoise }, AltThreshold = 0.6f, AccentThreshold = 0.78f,
                TreeVariants = new[] { 0 }, Trees = 0.04f, Bushes = 0.1f, Rocks = 0.006f, Walls = 0.02f, TreeScale = new Vector2(1f, 1.3f),
                Trail = TownSwatch.Paving, Cliff = TownSwatch.PavingDark, Slope = TownSwatch.GrassDark,
                Specials = 0.05f, Special = OverworldDecor.Cypress, Tufts = 0.14f, Flowers = 0.26f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.Tamarisk, TownSwatch.ClothWhite, TownSwatch.Turquoise },
            };
            DesertStyles(styles);
            CaveStyles(styles);
            MountainStyles(styles);
            CoastStyles(styles);
            DeltaStyles(styles);
            PamirStyles(styles);
            MingchengStyles(styles);
            NartesStyles(styles);
            FarnkeshStyles(styles);
            MarGeladoStyles(styles);
            return styles;
        }
    }
}
