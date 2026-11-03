using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 8 — Planície de Mingcheng: the look of the plain's terrains (140–159, Docs/Mapas100.md §5 região 8, maps 41–46):
    /// green rings of villages and willows round Mingcheng, golden millet on yellow earth, the upper Rio Dourado and the
    /// Canal das Lanternas between willows and red lanterns, rice terraces flooded like mirrors, combed tea hills, bamboo, the
    /// white hills and garden terraces of the Torres Brancas, karst towers, misty pine ridges, the great wall on its crest,
    /// the terracotta pits, the spirit way of stone lions and the loess gullies. The relief stays low (≤ 5 m) on the plain
    /// round the city, the river and the canal, and rises on the terraces (12–14 m), the wall (~22 m), the pines (~26 m) and
    /// the karst (~34 m). Trees: the Zarafshan's poplars on the plain, the Serra's cedars in the hills; the region's own pieces
    /// (OverworldDecorKit.Mingcheng.cs) are the willows, lanterns, pavilions, lions, turtles, terracotta pits, the wall and its
    /// beacon towers, bamboo, karst towers, mountain pines, mirrors, white spires, the water wheel and the scarecrows. Only the
    /// palette rows every build already has (no dependency on the newer city rows).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Planície de Mingcheng terrain (Região 8, 140–159).</summary>
        public static bool IsMingcheng(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 140 && i < 160;
        }

        private static void MingchengStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation plain = TownVegetation.Forest;
            TownVegetation hills = TownVegetation.Mountain;
            int[] poplars = { 0, 1 };
            int[] cedars = { 0, 2 };

            // The outer rings of Mingcheng (41): green plots between grey-brick villages, willows, red lanterns, flagstone lanes.
            styles[(int)OverworldTerrain.NineRings] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.NineRings, Relief = 3f, ReliefScale = 220f, ReliefDetail = 0.08f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.Paving, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Vine, TownSwatch.Paving, TownSwatch.Loam }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Loam, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Stone, Bed = TownSwatch.Sea,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.025f, Bushes = 0.03f, Rocks = 0.003f,
                Specials = 0.035f, SpecialMix = new[] { OverworldDecor.Willow, OverworldDecor.RedLantern, OverworldDecor.Willow },
                Tufts = 0.2f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Vine, Flowers = 0.07f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow, TownSwatch.ClothWhite },
            };

            // Golden millet on the yellow earth of the outer rings: field banks, haystacks, a roadside pavilion.
            styles[(int)OverworldTerrain.MilletFields] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MilletFields, Relief = 4f, ReliefScale = 260f, ReliefDetail = 0.1f,
                Base = TownSwatch.GoldGrass, Alt = TownSwatch.Straw, Accent = TownSwatch.Loam, Mottle = TownSwatch.LeafGold, Hollow = TownSwatch.Grass,
                Patches = new[] { TownSwatch.Straw, TownSwatch.Loam, TownSwatch.Grass }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.AdobeOchre, Slope = TownSwatch.Straw, Bank = TownSwatch.Loam, Bed = TownSwatch.Sea,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.006f, Bushes = 0.01f, Rocks = 0.002f, Bales = 0.012f,
                Specials = 0.004f, Special = OverworldDecor.Pavilion,
                Tufts = 0.3f, TuftColor = TownSwatch.Straw, TuftTip = TownSwatch.LeafGold, Flowers = 0.02f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.Violet },
            };

            // The upper Rio Dourado on the plain: weeping willows on grassy banks, sandbars, reeds.
            styles[(int)OverworldTerrain.WillowRiver] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WillowRiver, Relief = 3f, ReliefScale = 180f, ReliefDetail = 0.08f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Moss, Accent = TownSwatch.Sand, Mottle = TownSwatch.Vine, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Sand, TownSwatch.Reed }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Sand, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.02f, Bushes = 0.04f, Rocks = 0.004f,
                Specials = 0.05f, Special = OverworldDecor.Willow,
                Tufts = 0.24f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Flowers = 0.04f, Reeds = 0.06f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow },
            };

            // The Canal das Lanternas (43): stone banks, willows, red lantern posts, the stone turtles with their steles.
            styles[(int)OverworldTerrain.LanternCanal] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LanternCanal, Relief = 2f, ReliefScale = 200f, ReliefDetail = 0.06f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Paving, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.GrassDark, TownSwatch.Paving, TownSwatch.Moss }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.PavingDark, Cliff = TownSwatch.Stone, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Stone, Bed = TownSwatch.Sea,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.012f, Bushes = 0.03f, Rocks = 0.003f,
                Specials = 0.045f, SpecialMix = new[] { OverworldDecor.Willow, OverworldDecor.RedLantern, OverworldDecor.Willow, OverworldDecor.StoneTurtle },
                Tufts = 0.18f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Grass, Flowers = 0.03f, Reeds = 0.05f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothWhite },
            };

            // The Arrozais Espelhados (44): flooded terraces up the slope, the water a mirror of the sky, mud risers, rice green.
            styles[(int)OverworldTerrain.MirrorPaddies] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MirrorPaddies, Shape = ReliefShape.Terraced, TerraceSteps = 6, Relief = 12f, ReliefScale = 230f, ReliefDetail = 0.04f,
                Base = TownSwatch.Glass, Alt = TownSwatch.Grass, Accent = TownSwatch.SereneAir, Mottle = TownSwatch.Pearl, Hollow = TownSwatch.Glass, Crest = TownSwatch.Vine,
                Patches = new[] { TownSwatch.Grass, TownSwatch.SereneAir, TownSwatch.Mud }, AltThreshold = 0.6f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Mud, Clearing = TownSwatch.Mud, Cliff = TownSwatch.Mud, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Mud, Bed = TownSwatch.Glass,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.003f, Bushes = 0.01f, Rocks = 0.002f,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.Scarecrow, OverworldDecor.Scarecrow, OverworldDecor.Pavilion },
                Tufts = 0.06f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Reeds = 0.02f, Pebbles = 0.002f,
            };

            // Tea hills: rounded terraces combed by tea rows, dark green, a pavilion on the tops.
            styles[(int)OverworldTerrain.TeaTerraces] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TeaTerraces, Shape = ReliefShape.Terraced, TerraceSteps = 4, Relief = 13f, ReliefScale = 210f, ReliefDetail = 0.12f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Grass, Accent = TownSwatch.Loam, Mottle = TownSwatch.RootGreen, Hollow = TownSwatch.Moss, Crest = TownSwatch.Grass,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Loam, TownSwatch.RootGreen }, AltThreshold = 0.56f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.Loam, Slope = TownSwatch.Moss, Bank = TownSwatch.Stone, Bed = TownSwatch.Sea,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.006f, Bushes = 0.02f, Rocks = 0.003f,
                Specials = 0.004f, Special = OverworldDecor.Pavilion,
                Tufts = 0.16f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Grass, Flowers = 0.03f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow },
            };

            // A bamboo grove: tall green culms in clumps, deep shade, fallen leaves, ferns.
            styles[(int)OverworldTerrain.BambooGrove] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BambooGrove, Shape = ReliefShape.Hills, Relief = 8f, ReliefScale = 200f, ReliefDetail = 0.2f,
                Base = TownSwatch.Moss, Alt = TownSwatch.GrassDark, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.Pine, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.GrassDark, TownSwatch.LeafGold, TownSwatch.Straw }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Mud, Bed = TownSwatch.Sea,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.01f, Bushes = 0.03f, Rocks = 0.006f, MossRocks = 0.004f,
                Specials = 0.13f, Special = OverworldDecor.Bamboo,
                Tufts = 0.12f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Grass, Ferns = 0.12f, Pebbles = 0.01f,
            };

            // The white hills east of Mingcheng (42): lime-white slopes in garden terraces, cypresses, bronze mirrors.
            styles[(int)OverworldTerrain.WhiteHills] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WhiteHills, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 14f, ReliefScale = 220f, ReliefDetail = 0.2f,
                Base = TownSwatch.LimeWhite, Alt = TownSwatch.Grass, Accent = TownSwatch.Plaster, Mottle = TownSwatch.Pearl, Hollow = TownSwatch.Grass, Crest = TownSwatch.LimeWhite,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Plaster, TownSwatch.Vine }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.Plaster, Cliff = TownSwatch.LimeWhite, Slope = TownSwatch.Plaster, Bank = TownSwatch.LimeWhite, Bed = TownSwatch.Sea,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.01f, Bushes = 0.02f, Rocks = 0.006f, RockTint = TownSwatch.LimeWhite,
                Specials = 0.03f, SpecialMix = new[] { OverworldDecor.Cypress, OverworldDecor.Cypress, OverworldDecor.MirrorPost },
                Tufts = 0.12f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Flowers = 0.05f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow, TownSwatch.SilkBlue },
            };

            // The garden terraces of the Torres Brancas (42): lawns in rows, white spires, small Meroë pyramids, mirror posts.
            styles[(int)OverworldTerrain.SpireGardens] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SpireGardens, Relief = 5f, ReliefScale = 220f, ReliefDetail = 0.06f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.LimeWhite, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Vine, TownSwatch.LimeWhite, TownSwatch.Sandstone }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.Sandstone, Cliff = TownSwatch.LimeWhite, Slope = TownSwatch.GrassDark, Bank = TownSwatch.LimeWhite, Bed = TownSwatch.Sea,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.02f, Bushes = 0.03f, Rocks = 0.002f, Rows = true, RowSpacing = new Vector2(9f, 8f),
                Specials = 0.02f, SpecialMix = new[] { OverworldDecor.WhiteSpire, OverworldDecor.MirrorPost, OverworldDecor.Cypress },
                Tufts = 0.14f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Flowers = 0.09f, Pebbles = 0.005f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow, TownSwatch.SilkBlue, TownSwatch.Violet },
            };

            // Karst: limestone towers sheer from a flat green floor; pines on their ledges.
            styles[(int)OverworldTerrain.KarstPeaks] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.KarstPeaks, Shape = ReliefShape.KarstTowers, Relief = 30f, ReliefScale = 200f, ReliefDetail = 0.15f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Moss, Accent = TownSwatch.Stone, Mottle = TownSwatch.GrassDark, Crest = TownSwatch.Pine, Hollow = TownSwatch.Grass,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Stone, TownSwatch.Pine }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Stone, Slope = TownSwatch.LimeWhite, Bank = TownSwatch.Stone, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.012f, Bushes = 0.03f, Rocks = 0.02f, RockTint = TownSwatch.Stone,
                Specials = 0.02f, Special = OverworldDecor.MountainPine,
                Tufts = 0.18f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Grass, Ferns = 0.04f, Pebbles = 0.02f,
                WaterVegetation = plain, WaterVariants = poplars,
            };

            // Misty pine ridges over grey granite.
            styles[(int)OverworldTerrain.PineRidge] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PineRidge, Shape = ReliefShape.Peaks, Relief = 26f, ReliefScale = 230f, ReliefDetail = 0.5f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Granite, Accent = TownSwatch.Stone, Mottle = TownSwatch.Pine, Crest = TownSwatch.Granite, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Granite, TownSwatch.Stone, TownSwatch.Pine }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Granite, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.02f, Bushes = 0.02f, Rocks = 0.04f, RockScale = new Vector2(1f, 2.6f), RockTint = TownSwatch.Granite,
                Specials = 0.06f, Special = OverworldDecor.MountainPine,
                Tufts = 0.12f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Thatch, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The Muralha das Mil Torres (45): grassy crest under the wall, scree, pines below.
            styles[(int)OverworldTerrain.GreatWall] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.GreatWall, Shape = ReliefShape.WallRidge, Relief = 22f, ReliefScale = 220f, ReliefDetail = 0.3f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Thatch, Accent = TownSwatch.Stone, Mottle = TownSwatch.Moss, Crest = TownSwatch.Thatch, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Thatch, TownSwatch.Stone, TownSwatch.Moss }, AltThreshold = 0.56f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Stone, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.01f, Bushes = 0.02f, Rocks = 0.03f, RockTint = TownSwatch.Stone,
                Specials = 0.02f, Special = OverworldDecor.MountainPine,
                Tufts = 0.22f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.LeafGold, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The terracotta pits: flat loess, long pits of clay soldiers, burial mounds (the Mounds shape).
            styles[(int)OverworldTerrain.TerracottaPits] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TerracottaPits, Shape = ReliefShape.Mounds, Relief = 6f, ReliefScale = 160f, ReliefDetail = 0.08f,
                Base = TownSwatch.Loam, Alt = TownSwatch.SandDark, Accent = TownSwatch.RedClay, Mottle = TownSwatch.AdobeOchre, Hollow = TownSwatch.DirtDark, Crest = TownSwatch.GoldGrass,
                Patches = new[] { TownSwatch.SandDark, TownSwatch.RedClay, TownSwatch.GoldGrass }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Loam, Cliff = TownSwatch.AdobeOchre, Slope = TownSwatch.Loam, Bank = TownSwatch.Loam, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.004f, Bushes = 0.015f, Rocks = 0.004f, RockTint = TownSwatch.AdobeOchre,
                Specials = 0.004f, Special = OverworldDecor.StoneLion,
                Tufts = 0.14f, TuftColor = TownSwatch.GoldGrass, TuftTip = TownSwatch.Straw, Pebbles = 0.03f,
            };

            // The spirit way into Mingcheng (41): lawns and pines, stone lions and stele turtles along the avenue.
            styles[(int)OverworldTerrain.SpiritWay] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SpiritWay, Relief = 2.5f, ReliefScale = 220f, ReliefDetail = 0.06f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Paving, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.GrassDark, TownSwatch.Paving }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.PavingDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Stone, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.02f, Bushes = 0.02f, Rocks = 0.002f,
                Specials = 0.03f, SpecialMix = new[] { OverworldDecor.StoneLion, OverworldDecor.MountainPine, OverworldDecor.StoneTurtle },
                Tufts = 0.18f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Grass, Flowers = 0.03f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite },
            };

            // Loess gullies: yellow earth cut by deep ravines, earth pillars, thin grass.
            styles[(int)OverworldTerrain.LoessGullies] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LoessGullies, Shape = ReliefShape.Gorges, Relief = 16f, ReliefScale = 210f, ReliefDetail = 0.3f,
                Base = TownSwatch.AdobeOchre, Alt = TownSwatch.Loam, Accent = TownSwatch.GoldGrass, Mottle = TownSwatch.Sandstone, Crest = TownSwatch.GoldGrass, Hollow = TownSwatch.Loam,
                Patches = new[] { TownSwatch.Loam, TownSwatch.GoldGrass, TownSwatch.Sandstone }, AltThreshold = 0.52f, AccentThreshold = 0.84f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.AdobeOchre, Bank = TownSwatch.Loam, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.004f, Bushes = 0.03f, Rocks = 0.01f, RockTint = TownSwatch.AdobeOchre,
                Specials = 0.01f, Special = OverworldDecor.Hoodoo,
                Tufts = 0.2f, TuftColor = TownSwatch.GoldGrass, TuftTip = TownSwatch.Straw, Pebbles = 0.03f,
                WaterVegetation = plain, WaterVariants = poplars,
            };
        }
    }
}
