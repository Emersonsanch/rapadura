using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 3 — Serra de Kaggen: the look of the mountain terrains (60–79). The relief is strong where nothing has to be
    /// reached on a ramp (passes, scarps, snowfields, the summit: 28–40 m) and kept at ≤ 18 m in the areas with a town, a
    /// gate or a torrent (their flat ground and valley sides must stay walkable, Docs/Mapas100.md §0.10 "Relevo").
    /// Trees and shrubs are the Serra's (twisted cedars, tree aloes, baobabs; aloe shrubs), also along the water; the heavy
    /// baobabs and tree aloes are the second flora, so the dense backdrop beyond the walls is cedars only (chunk budget).
    /// </summary>
    public static partial class OverworldTerrains
    {
        private static void MountainStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation mountain = TownVegetation.Mountain;
            int[] riverTrees = { 0, 2, 1 };

            styles[(int)OverworldTerrain.GoldenSlopes] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.GoldenSlopes, Relief = 9f, ReliefScale = 200f, ReliefDetail = 0.5f, Shape = ReliefShape.Hills,
                Base = TownSwatch.LeafGold, Alt = TownSwatch.Thatch, Accent = TownSwatch.Grass, Mottle = TownSwatch.SandDark,
                Patches = new[] { TownSwatch.Thatch, TownSwatch.Grass, TownSwatch.RedClay }, AltThreshold = 0.52f, AccentThreshold = 0.8f,
                Crest = TownSwatch.Thatch, Hollow = TownSwatch.Grass, Cliff = TownSwatch.RedClay, Slope = TownSwatch.Thatch, Clearing = TownSwatch.Dirt,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 3, 1 }, SecondShare = 0.7f, Trees = 0.018f, Bushes = 0.07f, Rocks = 0.02f, RockScale = new Vector2(0.9f, 2.2f),
                TreeScale = new Vector2(1f, 1.5f), Specials = 0.004f, Special = OverworldDecor.Cairn,
                Tufts = 0.42f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.LeafGold, Flowers = 0.07f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothRed, TownSwatch.ClothWhite },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.FarmTerraces] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FarmTerraces, Relief = 14f, ReliefScale = 240f, ReliefDetail = 0.12f, Shape = ReliefShape.Terraced, TerraceSteps = 4,
                Base = TownSwatch.Dirt, Alt = TownSwatch.Vine, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Vine, TownSwatch.LeafGold, TownSwatch.Stone }, AltThreshold = 0.5f, AccentThreshold = 0.78f,
                Cliff = TownSwatch.Stone, Slope = TownSwatch.DirtDark, Clearing = TownSwatch.DirtDark, Hollow = TownSwatch.Grass,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 3 }, SecondShare = 0.6f, Trees = 0.008f, Bushes = 0.02f, Rocks = 0.008f,
                Tufts = 0.14f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.LeafGold, Flowers = 0.02f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.AloeSlopes] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.AloeSlopes, Relief = 16f, ReliefScale = 190f, ReliefDetail = 0.9f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Thatch, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Stone, Mottle = TownSwatch.LeafGold,
                Patches = new[] { TownSwatch.Stone, TownSwatch.Aloe, TownSwatch.LeafGold }, AltThreshold = 0.54f, AccentThreshold = 0.8f,
                Crest = TownSwatch.Stone, Hollow = TownSwatch.GrassDark, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Thatch, Trail = TownSwatch.Dirt,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 1 }, SecondShare = 0.7f, Trees = 0.05f, Bushes = 0.09f, Rocks = 0.05f, RockScale = new Vector2(1f, 2.8f),
                Specials = 0.012f, Special = OverworldDecor.Cairn, Tufts = 0.3f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark,
                Flowers = 0.1f, Pebbles = 0.04f, FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.Tamarisk, TownSwatch.ClothYellow },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.BaobabGrass] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BaobabGrass, Relief = 10f, ReliefScale = 230f, ReliefDetail = 0.4f,
                Base = TownSwatch.LeafGold, Alt = TownSwatch.Grass, Accent = TownSwatch.Thatch, Mottle = TownSwatch.SandDark,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Thatch }, AltThreshold = 0.55f, AccentThreshold = 0.8f, Crest = TownSwatch.Thatch,
                Hollow = TownSwatch.Grass, Cliff = TownSwatch.RedClay, Slope = TownSwatch.Thatch,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 3, 3, 1 }, SecondShare = 0.8f, Trees = 0.03f, Bushes = 0.04f, Rocks = 0.01f, TreeScale = new Vector2(1.2f, 1.9f),
                Stumps = 0.002f, Tufts = 0.46f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.LeafGold, Flowers = 0.05f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.HighPass] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HighPass, Relief = 16f, ReliefScale = 210f, ReliefDetail = 0.8f, Shape = ReliefShape.Terraced, TerraceSteps = 3,
                Base = TownSwatch.Stone, Alt = TownSwatch.Snow, Accent = TownSwatch.GrassDark, Mottle = TownSwatch.StoneDark,
                Patches = new[] { TownSwatch.Snow, TownSwatch.GrassDark }, AltThreshold = 0.6f, AccentThreshold = 0.82f, Crest = TownSwatch.Snow,
                Hollow = TownSwatch.Snow, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Trail = TownSwatch.Stone, Clearing = TownSwatch.Dirt,
                Vegetation = mountain, TreeVariants = new[] { 0, 2 }, Trees = 0.04f, Bushes = 0.03f, Rocks = 0.07f, RockScale = new Vector2(1.2f, 3.2f),
                Specials = 0.01f, Special = OverworldDecor.Cairn, Tufts = 0.18f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Thatch,
                Pebbles = 0.05f, Flowers = 0.02f, FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Violet },
                WaterVegetation = mountain, WaterVariants = new[] { 0, 2 },
            };
            styles[(int)OverworldTerrain.Torrents] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Torrents, Relief = 18f, ReliefScale = 170f, ReliefDetail = 0.6f, Shape = ReliefShape.Gorges,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Moss, Accent = TownSwatch.Stone, Mottle = TownSwatch.Pine,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Stone }, AltThreshold = 0.52f, Hollow = TownSwatch.GrassDark, Crest = TownSwatch.Grass,
                Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Trail = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = new[] { 0, 2 }, SecondVegetation = mountain, SecondVariants = new[] { 1 }, SecondShare = 0.25f, Trees = 0.11f, Bushes = 0.08f, Rocks = 0.04f, RockScale = new Vector2(1f, 2.6f),
                MossRocks = 0.02f, Logs = 0.004f, Ferns = 0.14f, Tufts = 0.24f, Reeds = 0.01f, Pebbles = 0.04f, TuftColor = TownSwatch.GrassDark,
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.Kraals] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Kraals, Relief = 14f, ReliefScale = 210f, ReliefDetail = 0.5f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Grass, Alt = TownSwatch.LeafGold, Accent = TownSwatch.Dirt, Mottle = TownSwatch.Thatch,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.Dirt }, AltThreshold = 0.55f, AccentThreshold = 0.82f, Crest = TownSwatch.LeafGold,
                Hollow = TownSwatch.GrassDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 3 }, SecondShare = 0.5f, Trees = 0.015f, Bushes = 0.05f, Rocks = 0.02f, Bales = 0.006f,
                Tufts = 0.4f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.LeafGold, Flowers = 0.06f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Tamarisk, TownSwatch.ClothWhite },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.NarrowPass] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.NarrowPass, Relief = 28f, ReliefScale = 200f, ReliefDetail = 1f, Shape = ReliefShape.Gorges,
                Base = TownSwatch.StoneDark, Alt = TownSwatch.Stone, Accent = TownSwatch.GrassDark, Mottle = TownSwatch.Basalt,
                Patches = new[] { TownSwatch.Stone, TownSwatch.GrassDark }, AltThreshold = 0.5f, Hollow = TownSwatch.GrassDark, Crest = TownSwatch.Snow,
                Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Trail = TownSwatch.Stone, Clearing = TownSwatch.Dirt,
                Vegetation = mountain, TreeVariants = new[] { 0 }, Trees = 0.02f, Bushes = 0.03f, Rocks = 0.09f, RockScale = new Vector2(1.4f, 4f),
                Specials = 0.006f, Special = OverworldDecor.Cairn, Tufts = 0.14f, TuftColor = TownSwatch.GrassDark, Pebbles = 0.06f,
                WaterVegetation = mountain, WaterVariants = new[] { 0, 2 },
            };
            styles[(int)OverworldTerrain.SandstoneScarp] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SandstoneScarp, Relief = 30f, ReliefScale = 260f, ReliefDetail = 0.6f, Shape = ReliefShape.Terraced, TerraceSteps = 3,
                Base = TownSwatch.Sandstone, Alt = TownSwatch.RedClay, Accent = TownSwatch.Thatch, Mottle = TownSwatch.SandDark,
                Patches = new[] { TownSwatch.RedClay, TownSwatch.Thatch, TownSwatch.Grass }, AltThreshold = 0.55f, AccentThreshold = 0.78f,
                Crest = TownSwatch.Thatch, Hollow = TownSwatch.Grass, Cliff = TownSwatch.RedClay, Slope = TownSwatch.Sandstone, Trail = TownSwatch.SandDark,
                Clearing = TownSwatch.SandDark, RockTint = TownSwatch.Sandstone,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 1 }, SecondShare = 0.4f, Trees = 0.02f, Bushes = 0.05f, Rocks = 0.06f, RockScale = new Vector2(1.2f, 3.4f),
                Tufts = 0.25f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Flowers = 0.03f, Pebbles = 0.05f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.PaintedShelters] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PaintedShelters, Relief = 15f, ReliefScale = 220f, ReliefDetail = 0.5f, Shape = ReliefShape.Terraced, TerraceSteps = 2,
                Base = TownSwatch.Thatch, Alt = TownSwatch.Sandstone, Accent = TownSwatch.Grass, Mottle = TownSwatch.LeafGold,
                Patches = new[] { TownSwatch.Sandstone, TownSwatch.Grass, TownSwatch.RedClay }, AltThreshold = 0.56f, AccentThreshold = 0.8f,
                Crest = TownSwatch.Sandstone, Hollow = TownSwatch.Grass, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.RedClay, Trail = TownSwatch.SandDark,
                RockTint = TownSwatch.Sandstone,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 1, 3 }, SecondShare = 0.6f, Trees = 0.03f, Bushes = 0.06f, Rocks = 0.04f, RockScale = new Vector2(1f, 2.8f),
                Tufts = 0.38f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.LeafGold, Flowers = 0.05f, Pebbles = 0.03f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothWhite, TownSwatch.Tamarisk },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.DryStone] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DryStone, Relief = 15f, ReliefScale = 200f, ReliefDetail = 0.7f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Grass, Alt = TownSwatch.Moss, Accent = TownSwatch.Stone, Mottle = TownSwatch.Vine,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Stone, TownSwatch.LeafGold }, AltThreshold = 0.52f, AccentThreshold = 0.8f,
                Crest = TownSwatch.Stone, Hollow = TownSwatch.GrassDark, Cliff = TownSwatch.Stone, Slope = TownSwatch.GrassDark, Trail = TownSwatch.Cobble,
                Vegetation = mountain, TreeVariants = new[] { 0 }, SecondVegetation = mountain, SecondVariants = new[] { 1 }, SecondShare = 0.4f, Trees = 0.02f, Bushes = 0.04f, Rocks = 0.06f, RockScale = new Vector2(1.2f, 3.6f),
                MossRocks = 0.02f, Tufts = 0.33f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Moss, Flowers = 0.03f, Pebbles = 0.04f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Violet },
                WaterVegetation = mountain, WaterVariants = riverTrees,
            };
            styles[(int)OverworldTerrain.BronzeHalls] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BronzeHalls, Relief = 15f, ReliefScale = 190f, ReliefDetail = 0.6f, Shape = ReliefShape.Terraced, TerraceSteps = 3,
                Base = TownSwatch.StoneDark, Alt = TownSwatch.Basalt, Accent = TownSwatch.Slate, Mottle = TownSwatch.Stone,
                Patches = new[] { TownSwatch.Basalt, TownSwatch.Copper }, AltThreshold = 0.52f, AccentThreshold = 0.8f, Hollow = TownSwatch.Basalt,
                Crest = TownSwatch.Stone, Cliff = TownSwatch.Basalt, Slope = TownSwatch.StoneDark, Trail = TownSwatch.Cobble, Clearing = TownSwatch.Stone,
                RockTint = TownSwatch.StoneDark, Bank = TownSwatch.StoneDark, Bed = TownSwatch.Basalt,
                Vegetation = mountain, TreeVariants = new[] { 0 }, Trees = 0.01f, Bushes = 0.01f, Rocks = 0.06f, RockScale = new Vector2(1.2f, 3f),
                Specials = 0.02f, Special = OverworldDecor.Basalt, Tufts = 0.05f, TuftColor = TownSwatch.GrassDark, Pebbles = 0.08f,
                WaterVegetation = mountain, WaterVariants = new[] { 0 },
            };
            styles[(int)OverworldTerrain.Snowfield] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Snowfield, Relief = 32f, ReliefScale = 230f, ReliefDetail = 1f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Snow, Alt = TownSwatch.Stone, Accent = TownSwatch.StoneDark, Mottle = TownSwatch.LimeWhite,
                Patches = new[] { TownSwatch.Stone, TownSwatch.LimeWhite }, AltThreshold = 0.6f, AccentThreshold = 0.84f, Crest = TownSwatch.Snow,
                Hollow = TownSwatch.LimeWhite, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone,
                Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = new[] { 0, 2 }, Trees = 0.015f, Bushes = 0.004f, Rocks = 0.06f, RockScale = new Vector2(1.2f, 3.4f),
                Specials = 0.004f, Special = OverworldDecor.Cairn, Tufts = 0.02f, TuftColor = TownSwatch.Thatch, Pebbles = 0.04f,
                WaterVegetation = mountain, WaterVariants = new[] { 0, 2 },
            };
            styles[(int)OverworldTerrain.Summit] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Summit, Relief = 40f, ReliefScale = 260f, ReliefDetail = 1.2f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Snow, Alt = TownSwatch.LimeWhite, Accent = TownSwatch.Stone, Mottle = TownSwatch.Pearl,
                Patches = new[] { TownSwatch.Stone, TownSwatch.Pearl }, AltThreshold = 0.56f, AccentThreshold = 0.84f, Crest = TownSwatch.Snow,
                Hollow = TownSwatch.LimeWhite, Cliff = TownSwatch.Stone, Slope = TownSwatch.Snow, Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone,
                Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = new[] { 0 }, Trees = 0f, Bushes = 0f, Rocks = 0.05f, RockScale = new Vector2(1.4f, 3.8f),
                Specials = 0.004f, Special = OverworldDecor.Cairn, Tufts = 0f, Pebbles = 0.03f,
                WaterVegetation = mountain, WaterVariants = new[] { 0 },
            };
            styles[(int)OverworldTerrain.StelaPlateau] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.StelaPlateau, Relief = 16f, ReliefScale = 200f, ReliefDetail = 0.5f, Shape = ReliefShape.Terraced, TerraceSteps = 3,
                Base = TownSwatch.Snow, Alt = TownSwatch.Thatch, Accent = TownSwatch.Stone, Mottle = TownSwatch.Grass,
                Patches = new[] { TownSwatch.Thatch, TownSwatch.Stone }, AltThreshold = 0.58f, AccentThreshold = 0.84f, Crest = TownSwatch.Snow,
                Hollow = TownSwatch.Thatch, Cliff = TownSwatch.Stone, Slope = TownSwatch.Thatch, Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone,
                Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = new[] { 0 }, Trees = 0.01f, Bushes = 0.02f, Rocks = 0.04f, RockScale = new Vector2(1.2f, 3f),
                Specials = 0.01f, Special = OverworldDecor.Cairn, Tufts = 0.12f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.03f,
                WaterVegetation = mountain, WaterVariants = new[] { 0 },
            };
        }
    }
}
