using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 4 — Grutas do Limiar: the look of the cave terrains (Docs/Mapas100.md 19–24). Basalt and slate floors, pink tuff
    /// terraces, indigo crystal rock; the light comes from the palette's emissive cells — fluorescent moss (<see cref="TownSwatch.Moss"/>)
    /// and glowing mushrooms/crystals (<see cref="TownSwatch.MushroomBlue"/>) on the ground, in the tufts and in the pieces.
    /// The vegetation is the kit's cave flora (<see cref="TownVegetation.Caves"/>: glowing mushrooms 0/1/3, stalagmite 2).
    /// </summary>
    public static partial class OverworldTerrains
    {
        private static readonly int[] CaveMushrooms = { 0, 1, 3, 3 };
        private static readonly int[] CaveStalagmites = { 2, 2, 3 };

        private static void CaveStyles(OverworldTerrainStyle[] styles)
        {
            styles[(int)OverworldTerrain.CaveMouth] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CaveMouth, Relief = 9f, ReliefScale = 170f, ReliefDetail = 0.6f, Shape = ReliefShape.Hills,
                Base = TownSwatch.StoneDark, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Moss, Mottle = TownSwatch.Slate, Hollow = TownSwatch.Basalt,
                Crest = TownSwatch.Stone, Patches = new[] { TownSwatch.Moss, TownSwatch.Pine }, AltThreshold = 0.56f, AccentThreshold = 0.82f,
                Vegetation = TownVegetation.Mountain, TreeVariants = new[] { 0, 2 }, SecondVegetation = TownVegetation.Caves, SecondVariants = CaveMushrooms,
                SecondShare = 0.5f, BushVegetation = TownVegetation.Caves, Trees = 0.03f, Bushes = 0.05f, Rocks = 0.04f, RockScale = new Vector2(1f, 2.6f),
                Trail = TownSwatch.Stone, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.Basalt, Slope = TownSwatch.StoneDark,
                Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, MossRocks = 0.01f, Specials = 0.012f, Special = OverworldDecor.Stalagmite,
                Tufts = 0.22f, TuftColor = TownSwatch.Pine, TuftTip = TownSwatch.Moss, Ferns = 0.06f, Pebbles = 0.05f, Flowers = 0.02f,
                FlowerColors = new[] { TownSwatch.MushroomBlue, TownSwatch.ClothWhite }, RockTint = TownSwatch.StoneDark,
            };
            styles[(int)OverworldTerrain.LakeShore] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LakeShore, Relief = 3f, ReliefScale = 160f, ReliefDetail = 0.25f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.Black, Accent = TownSwatch.Moss, Mottle = TownSwatch.Slate, Hollow = TownSwatch.Black,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Water }, AltThreshold = 0.58f, AccentThreshold = 0.8f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveMushrooms, BushVegetation = TownVegetation.Caves, Trees = 0.03f, Bushes = 0.06f,
                Rocks = 0.02f, Trail = TownSwatch.Slate, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Slate,
                Bank = TownSwatch.Black, Bed = TownSwatch.Black, MossRocks = 0.008f, Specials = 0.01f, Special = OverworldDecor.GiantMushroom,
                Tufts = 0.2f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.MushroomBlue, Ferns = 0.05f, Pebbles = 0.06f, Puddles = 0.01f,
                RockTint = TownSwatch.Basalt,
            };
            styles[(int)OverworldTerrain.StalactiteHall] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.StalactiteHall, Relief = 6f, ReliefScale = 120f, ReliefDetail = 0.4f, Shape = ReliefShape.Mounds,
                Base = TownSwatch.Slate, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Indigo, Mottle = TownSwatch.Stone, Crest = TownSwatch.Stone,
                Patches = new[] { TownSwatch.Indigo, TownSwatch.Moss }, AltThreshold = 0.54f, AccentThreshold = 0.82f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveStalagmites, BushVegetation = TownVegetation.Caves, Trees = 0.05f, Bushes = 0.03f,
                Rocks = 0.02f, Trail = TownSwatch.Stone, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.Basalt, Slope = TownSwatch.StoneDark,
                Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, Specials = 0.02f, Special = OverworldDecor.CaveColumn,
                Tufts = 0.12f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.09f, RockTint = TownSwatch.Slate,
            };
            styles[(int)OverworldTerrain.MushroomGrove] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MushroomGrove, Relief = 8f, ReliefScale = 150f, ReliefDetail = 0.5f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Indigo, Alt = TownSwatch.Violet, Accent = TownSwatch.MushroomBlue, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Black,
                Crest = TownSwatch.Moss, Patches = new[] { TownSwatch.MushroomBlue, TownSwatch.Moss, TownSwatch.Violet }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveMushrooms, BushVegetation = TownVegetation.Caves, Trees = 0.16f, Bushes = 0.1f,
                Rocks = 0.006f, TreeScale = new Vector2(1.1f, 1.9f), Trail = TownSwatch.Slate, Clearing = TownSwatch.Basalt, Cliff = TownSwatch.Basalt,
                Slope = TownSwatch.Indigo, Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, Specials = 0.02f, Special = OverworldDecor.GiantMushroom,
                Tufts = 0.3f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.MushroomBlue, Ferns = 0.14f, Flowers = 0.06f,
                FlowerColors = new[] { TownSwatch.MushroomBlue, TownSwatch.Violet }, RockTint = TownSwatch.Indigo,
            };
            styles[(int)OverworldTerrain.CrystalField] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CrystalField, Relief = 9f, ReliefScale = 150f, ReliefDetail = 0.5f, Shape = ReliefShape.Terraced, TerraceSteps = 2,
                Base = TownSwatch.Indigo, Alt = TownSwatch.Slate, Accent = TownSwatch.Glass, Mottle = TownSwatch.Violet, Crest = TownSwatch.Glass,
                Hollow = TownSwatch.Black, Patches = new[] { TownSwatch.Glass, TownSwatch.MushroomBlue }, AltThreshold = 0.56f, AccentThreshold = 0.84f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveStalagmites, BushVegetation = TownVegetation.Caves, Trees = 0.02f, Bushes = 0.02f,
                Rocks = 0.03f, Trail = TownSwatch.Slate, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.Violet, Slope = TownSwatch.Indigo,
                Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, Specials = 0.03f, Special = OverworldDecor.Crystal,
                Tufts = 0.06f, TuftColor = TownSwatch.MushroomBlue, TuftTip = TownSwatch.Glass, Pebbles = 0.12f, RockTint = TownSwatch.Violet,
            };
            styles[(int)OverworldTerrain.Chasm] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Chasm, Relief = 15f, ReliefScale = 170f, ReliefDetail = 0.7f, Shape = ReliefShape.Gorges,
                Base = TownSwatch.Basalt, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Moss, Mottle = TownSwatch.Slate, Hollow = TownSwatch.Black,
                Crest = TownSwatch.Slate, Patches = new[] { TownSwatch.Black, TownSwatch.Moss }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveStalagmites, BushVegetation = TownVegetation.Caves, Trees = 0.02f, Bushes = 0.03f,
                Rocks = 0.05f, RockScale = new Vector2(1.3f, 3.6f), Trail = TownSwatch.Stone, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.Black,
                Slope = TownSwatch.Basalt, Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, Specials = 0.03f, Special = OverworldDecor.Basalt,
                Tufts = 0.08f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.08f, RockTint = TownSwatch.Basalt,
            };
            styles[(int)OverworldTerrain.TuffTerraces] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TuffTerraces, Relief = 18f, ReliefScale = 200f, ReliefDetail = 0.4f, Shape = ReliefShape.Terraced, TerraceSteps = 4,
                Base = TownSwatch.PinkTuff, Alt = TownSwatch.TaipaRose, Accent = TownSwatch.Moss, Mottle = TownSwatch.AdobeOchre, Crest = TownSwatch.TaipaRose,
                Hollow = TownSwatch.RedClay, Patches = new[] { TownSwatch.Copper, TownSwatch.Moss }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveMushrooms, BushVegetation = TownVegetation.Caves, Trees = 0.02f, Bushes = 0.04f,
                Rocks = 0.03f, Trail = TownSwatch.TaipaRose, Clearing = TownSwatch.AdobeOchre, Cliff = TownSwatch.RedClay, Slope = TownSwatch.TaipaRose,
                Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, Specials = 0.012f, Special = OverworldDecor.LampNiche,
                Tufts = 0.1f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.06f, RockTint = TownSwatch.PinkTuff,
            };
            styles[(int)OverworldTerrain.ColumnForest] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ColumnForest, Relief = 4f, ReliefScale = 140f, ReliefDetail = 0.3f,
                Base = TownSwatch.Stone, Alt = TownSwatch.Slate, Accent = TownSwatch.Moss, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Basalt,
                Patches = new[] { TownSwatch.Moss, TownSwatch.StoneDark }, AltThreshold = 0.55f, AccentThreshold = 0.84f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveStalagmites, BushVegetation = TownVegetation.Caves, Trees = 0.03f, Bushes = 0.03f,
                Rocks = 0.015f, Trail = TownSwatch.Paving, Clearing = TownSwatch.Slate, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Slate,
                Bank = TownSwatch.Black, Bed = TownSwatch.Black, Specials = 0.015f, Special = OverworldDecor.Stalagmite,
                Tufts = 0.14f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.MushroomBlue, Ferns = 0.04f, Pebbles = 0.06f, RockTint = TownSwatch.Stone,
            };
            styles[(int)OverworldTerrain.MossMeadow] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MossMeadow, Relief = 6f, ReliefScale = 180f, ReliefDetail = 0.4f, Shape = ReliefShape.Hills,
                Base = TownSwatch.Moss, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Vine, Mottle = TownSwatch.Vine, Hollow = TownSwatch.GrassDark,
                Crest = TownSwatch.Basalt, Patches = new[] { TownSwatch.Water, TownSwatch.Basalt, TownSwatch.Vine }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveMushrooms, BushVegetation = TownVegetation.Caves, Trees = 0.02f, Bushes = 0.08f,
                Rocks = 0.012f, Trail = TownSwatch.Basalt, Clearing = TownSwatch.GrassDark, Cliff = TownSwatch.Basalt, Slope = TownSwatch.GrassDark,
                Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, MossRocks = 0.02f, Specials = 0.006f, Special = OverworldDecor.Vishapakar,
                Tufts = 0.34f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Moss, Ferns = 0.12f, Flowers = 0.05f, Reeds = 0.01f,
                FlowerColors = new[] { TownSwatch.MushroomBlue, TownSwatch.ClothWhite }, RockTint = TownSwatch.Basalt,
            };
            styles[(int)OverworldTerrain.TunnelMarket] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TunnelMarket, Relief = 2.2f, ReliefScale = 160f, ReliefDetail = 0.15f,
                Base = TownSwatch.DirtDark, Alt = TownSwatch.Cobble, Accent = TownSwatch.Copper, Mottle = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Copper, TownSwatch.Indigo }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveStalagmites, BushVegetation = TownVegetation.Caves, Trees = 0.015f, Bushes = 0.02f,
                Rocks = 0.02f, Trail = TownSwatch.Cobble, Clearing = TownSwatch.Mud, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.DirtDark,
                Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, Specials = 0.012f, Special = OverworldDecor.MineCart,
                Tufts = 0.05f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.12f, RockTint = TownSwatch.StoneDark,
            };
            styles[(int)OverworldTerrain.WheelCanyon] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WheelCanyon, Relief = 15f, ReliefScale = 200f, ReliefDetail = 0.5f, Shape = ReliefShape.Gorges,
                Base = TownSwatch.PinkTuff, Alt = TownSwatch.Stone, Accent = TownSwatch.Copper, Mottle = TownSwatch.TaipaRose, Hollow = TownSwatch.StoneDark,
                Crest = TownSwatch.TaipaRose, Patches = new[] { TownSwatch.Copper, TownSwatch.Water }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveStalagmites, BushVegetation = TownVegetation.Caves, Trees = 0.015f, Bushes = 0.03f,
                Rocks = 0.04f, RockScale = new Vector2(1.2f, 3f), Trail = TownSwatch.Paving, Clearing = TownSwatch.AdobeOchre, Cliff = TownSwatch.RedClay,
                Slope = TownSwatch.TaipaRose, Bank = TownSwatch.Copper, Bed = TownSwatch.Black, Specials = 0.025f, Special = OverworldDecor.Jar,
                Tufts = 0.06f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.08f, RockTint = TownSwatch.PinkTuff,
            };
            styles[(int)OverworldTerrain.Roost] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Roost, Relief = 24f, ReliefScale = 210f, ReliefDetail = 0.6f, Shape = ReliefShape.Terraced, TerraceSteps = 5,
                Base = TownSwatch.StoneDark, Alt = TownSwatch.Basalt, Accent = TownSwatch.LimeWhite, Mottle = TownSwatch.Indigo, Crest = TownSwatch.Plaster,
                Hollow = TownSwatch.Black, Patches = new[] { TownSwatch.LimeWhite, TownSwatch.Indigo }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Vegetation = TownVegetation.Caves, TreeVariants = CaveStalagmites, BushVegetation = TownVegetation.Caves, Trees = 0.02f, Bushes = 0.02f,
                Rocks = 0.04f, RockScale = new Vector2(1.2f, 3.2f), Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.Slate, Cliff = TownSwatch.Black,
                Slope = TownSwatch.Basalt, Bank = TownSwatch.Basalt, Bed = TownSwatch.Black, Specials = 0.02f, Special = OverworldDecor.Nest,
                Tufts = 0.05f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.1f, RockTint = TownSwatch.StoneDark,
            };
        }
    }
}
