using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 12 — Arquipélago de Hinokura: the look of the volcanic islands' terrains (220–239, Docs/Mapas100.md §5 região 12, maps
    /// 65–70): the forge isle of black volcanic stone (pedra vulcânica <c>#2E2B2B</c>) and pumice round the lacquer town, the beaches
    /// of black sand, the thaw strait towards the Mar Gelado, the tamed lava channels (brasa <c>#FF8A3D</c>), the hot springs and the
    /// fumarole coast; the Jardins de Lava Laqueada of raked gravel, moss and pruned pines (verde de pinheiro <c>#3E6B4A</c>), the
    /// cloud pines and the red bridges (laca <c>#B23A2E</c>); Mount Echeyde's broom fields (retama <c>#E8D37A</c>, pedra-pomes
    /// <c>#C8BBA8</c>), its cone and its malpaís of black lava (<c>#3A2E2A</c>); the hot channels, the Recife de Obsidiana (obsidiana
    /// <c>#1E1A1E</c>, vapor <c>#E9E4DE</c>, coral de brasa <c>#E8613A</c>) and its glass islets; the castle's curved walls of coral
    /// stone (<c>#E8D9C0</c>), its courts and slag fields; the ash plains (cinza <c>#6A6460</c>) and the caldera of the Devouring Fire.
    /// The relief stays low on the forge isle, the beach, the strait, the gardens and the ash plains (≤ 6 m), rolls on the springs,
    /// the pines and the broom fields (~8 m), and stands up in the malpaís (~14 m), the reef (~18 m), the curved walls (~12 m), the
    /// caldera's rim (~30 m) and Mount Echeyde (~46 m). The region's own pieces (OverworldDecorKit.Hinokura.cs) are lava boulders,
    /// pumice rocks, stone lanterns, cloud pines, lacquer gates, fumaroles, obsidian shards, ember coral, broom bushes, stone roses,
    /// canary pines, furnace towers and slag heaps; Mingcheng's stone lions stand as the shisa of the gardens and its bamboo grows by
    /// the pines. Only the palette rows every build already has (no dependency on the newer city rows).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Hinokura terrain (Região 12, 220–239).</summary>
        public static bool IsHinokura(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 220 && i < 240;
        }

        private static void HinokuraStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation hills = TownVegetation.Mountain;
            int[] pines = { 2 };

            // The forge isle (65): black volcanic stone and pumice round the town, warm rocks, stone lanterns.
            styles[(int)OverworldTerrain.ForgeIsle] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ForgeIsle, Relief = 3f, ReliefScale = 240f, ReliefDetail = 0.06f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.StoneDark, Accent = TownSwatch.GoodSmoke, Mottle = TownSwatch.Slate, Hollow = TownSwatch.StoneDark,
                Patches = new[] { TownSwatch.GoodSmoke, TownSwatch.Slate, TownSwatch.Moss }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.GoodSmoke, Clearing = TownSwatch.Slate, Cliff = TownSwatch.Black, Slope = TownSwatch.StoneDark, Bank = TownSwatch.Basalt, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.006f, Rocks = 0.01f, RockTint = TownSwatch.Basalt,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.StoneLantern, OverworldDecor.LavaBoulder },
                Tufts = 0.04f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The beach of black sand (65): low dunes of dark sand, pumice washed up, driftwood.
            styles[(int)OverworldTerrain.BlackSandBeach] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BlackSandBeach, Relief = 3.5f, ReliefScale = 200f, ReliefDetail = 0.1f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Basalt, Accent = TownSwatch.GoodSmoke, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Basalt, Crest = TownSwatch.SandDark,
                Patches = new[] { TownSwatch.StoneDark, TownSwatch.GoodSmoke, TownSwatch.Black }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.StoneDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Black, Slope = TownSwatch.Basalt, Bank = TownSwatch.Basalt, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.002f, Rocks = 0.008f, RockTint = TownSwatch.Black,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.PumiceRock, OverworldDecor.Log },
                Tufts = 0.03f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Straw, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The thaw strait (65): melting floes on warm water, steam; the Mar Gelado's ice comes this far.
            styles[(int)OverworldTerrain.ThawStrait] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ThawStrait, Shape = ReliefShape.Floes, Relief = 3f, ReliefScale = 210f, ReliefDetail = 0.05f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.Firn, Accent = TownSwatch.Glass, Mottle = TownSwatch.Slate, Hollow = TownSwatch.Sea, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.Firn, TownSwatch.Glass, TownSwatch.StoneDark }, AltThreshold = 0.6f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Slate, Clearing = TownSwatch.Basalt, Cliff = TownSwatch.Glass, Slope = TownSwatch.Firn, Bank = TownSwatch.Basalt, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.006f, RockTint = TownSwatch.Basalt,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.IceBlock, OverworldDecor.Fumarole },
                Tufts = 0f, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The lava channels (65): black stone terraces cut by the tamed lava, warm rocks, lanterns by the bridges.
            styles[(int)OverworldTerrain.LavaChannels] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LavaChannels, Shape = ReliefShape.Terraced, TerraceSteps = 2, Relief = 5f, ReliefScale = 220f, ReliefDetail = 0.1f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.Black, Accent = TownSwatch.Ember, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Black, Crest = TownSwatch.Slate,
                Patches = new[] { TownSwatch.Black, TownSwatch.ColdEmber, TownSwatch.Slate }, AltThreshold = 0.55f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Slate, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.Black, Slope = TownSwatch.Basalt, Bank = TownSwatch.Black, Bed = TownSwatch.ColdEmber,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.012f, RockTint = TownSwatch.Black,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.LavaBoulder, OverworldDecor.StoneLantern },
                Tufts = 0f, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Hot springs (65): sinter mounds round steaming pools, sulphur crust, a few pines.
            styles[(int)OverworldTerrain.HotSprings] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HotSprings, Shape = ReliefShape.Mounds, Relief = 6f, ReliefScale = 190f, ReliefDetail = 0.1f,
                Base = TownSwatch.GoodSmoke, Alt = TownSwatch.Plaster, Accent = TownSwatch.ClothYellow, Mottle = TownSwatch.Stone, Hollow = TownSwatch.Turquoise, Crest = TownSwatch.LimeWhite,
                Patches = new[] { TownSwatch.ClothYellow, TownSwatch.Laterite, TownSwatch.Moss }, AltThreshold = 0.52f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.GoodSmoke, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Plaster, Bank = TownSwatch.LimeWhite, Bed = TownSwatch.ReefTeal,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.008f, Rocks = 0.008f, RockTint = TownSwatch.Stone,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.Fumarole, OverworldDecor.CloudPine, OverworldDecor.PumiceRock },
                Tufts = 0.06f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.ClothYellow, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The fumarole coast (65): low basalt cliffs, sulphur-rimmed vents, steam.
            styles[(int)OverworldTerrain.FumaroleCoast] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FumaroleCoast, Shape = ReliefShape.Cliffs, Relief = 8f, ReliefScale = 200f, ReliefDetail = 0.15f,
                Base = TownSwatch.StoneDark, Alt = TownSwatch.Basalt, Accent = TownSwatch.ClothYellow, Mottle = TownSwatch.Slate, Hollow = TownSwatch.Black, Crest = TownSwatch.GoodSmoke,
                Patches = new[] { TownSwatch.ClothYellow, TownSwatch.GoodSmoke, TownSwatch.Black }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.GoodSmoke, Clearing = TownSwatch.Slate, Cliff = TownSwatch.Black, Slope = TownSwatch.Basalt, Bank = TownSwatch.Basalt, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.004f, Rocks = 0.016f, RockScale = new Vector2(0.8f, 2f), RockTint = TownSwatch.Black,
                Specials = 0.012f, Special = OverworldDecor.Fumarole,
                Tufts = 0.03f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.ClothYellow, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The Jardins de Lava Laqueada (66): raked gravel and moss, pruned pines, stone lanterns, the shisa.
            styles[(int)OverworldTerrain.LavaGardens] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LavaGardens, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 5f, ReliefScale = 230f, ReliefDetail = 0.06f,
                Base = TownSwatch.GoodSmoke, Alt = TownSwatch.Moss, Accent = TownSwatch.Basalt, Mottle = TownSwatch.Plaster, Hollow = TownSwatch.GroveMoss, Crest = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Basalt, TownSwatch.Plaster }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Basalt, Clearing = TownSwatch.Plaster, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Moss, Bank = TownSwatch.Basalt, Bed = TownSwatch.CanalWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.012f, Rocks = 0.006f, RockTint = TownSwatch.Basalt,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.CloudPine, OverworldDecor.StoneLantern, OverworldDecor.StoneLion },
                Tufts = 0.08f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.GroveMoss, Flowers = 0.01f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothWhite },
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The cloud pines (66): moss and black rock under pines pruned in cloud pads, bamboo between them.
            styles[(int)OverworldTerrain.CloudPines] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CloudPines, Shape = ReliefShape.Hills, Relief = 8f, ReliefScale = 210f, ReliefDetail = 0.12f,
                Base = TownSwatch.Moss, Alt = TownSwatch.GroveMoss, Accent = TownSwatch.Basalt, Mottle = TownSwatch.TrollMoss, Hollow = TownSwatch.GroveMoss, Crest = TownSwatch.Pine,
                Patches = new[] { TownSwatch.Basalt, TownSwatch.Pine, TownSwatch.GoodSmoke }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Basalt, Clearing = TownSwatch.GroveMoss, Cliff = TownSwatch.Black, Slope = TownSwatch.Moss, Bank = TownSwatch.Basalt, Bed = TownSwatch.CanalWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.02f, Rocks = 0.008f, RockTint = TownSwatch.Basalt,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.CloudPine, OverworldDecor.CloudPine, OverworldDecor.Bamboo },
                Tufts = 0.14f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Ferns = 0.04f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The red bridges (66): islets in warm ponds, red lacquer gates, lanterns.
            styles[(int)OverworldTerrain.RedBridges] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.RedBridges, Shape = ReliefShape.Islets, Relief = 3f, ReliefScale = 200f, ReliefDetail = 0.06f,
                Base = TownSwatch.Moss, Alt = TownSwatch.GoodSmoke, Accent = TownSwatch.Lacquer, Mottle = TownSwatch.GroveMoss, Hollow = TownSwatch.CanalWater, Crest = TownSwatch.Moss,
                Patches = new[] { TownSwatch.GoodSmoke, TownSwatch.Basalt, TownSwatch.CanalWater }, AltThreshold = 0.55f, AccentThreshold = 0.97f,
                Trail = TownSwatch.Basalt, Clearing = TownSwatch.GoodSmoke, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Moss, Bank = TownSwatch.Basalt, Bed = TownSwatch.CanalWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.012f, Rocks = 0.006f, RockTint = TownSwatch.Basalt,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.StoneLantern, OverworldDecor.CloudPine },
                Tufts = 0.1f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Rush, Reeds = 0.04f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The broom fields of Echeyde (67): pumice slopes, yellow broom (retama #E8D37A), stone roses, canary pines.
            styles[(int)OverworldTerrain.BroomFields] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BroomFields, Shape = ReliefShape.Rolling, Relief = 7f, ReliefScale = 230f, ReliefDetail = 0.12f,
                Base = TownSwatch.GoodSmoke, Alt = TownSwatch.Sandstone, Accent = TownSwatch.GroveFlower, Mottle = TownSwatch.Plaster, Hollow = TownSwatch.Stone, Crest = TownSwatch.Plaster,
                Patches = new[] { TownSwatch.GroveFlower, TownSwatch.Basalt, TownSwatch.Sandstone }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Basalt, Clearing = TownSwatch.Sandstone, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.GoodSmoke, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.006f, Rocks = 0.008f, RockTint = TownSwatch.Stone,
                Specials = 0.016f, SpecialMix = new[] { OverworldDecor.BroomBush, OverworldDecor.BroomBush, OverworldDecor.StoneRose, OverworldDecor.CanaryPine },
                Tufts = 0.08f, TuftColor = TownSwatch.Straw, TuftTip = TownSwatch.GroveFlower, Flowers = 0.04f, Pebbles = 0.04f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.GroveFlower },
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Mount Echeyde (67): the cone of pumice and lava in three rings, the crater.
            styles[(int)OverworldTerrain.EcheydeCone] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.EcheydeCone, Shape = ReliefShape.VolcanoCone, Relief = 46f, ReliefScale = 230f, ReliefDetail = 0.15f,
                Base = TownSwatch.GoodSmoke, Alt = TownSwatch.Basalt, Accent = TownSwatch.Laterite, Mottle = TownSwatch.Stone, Hollow = TownSwatch.ColdEmber, Crest = TownSwatch.Plaster,
                Patches = new[] { TownSwatch.Basalt, TownSwatch.Laterite, TownSwatch.Plaster }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Basalt, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Black, Slope = TownSwatch.Basalt, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.002f, Rocks = 0.016f, RockScale = new Vector2(0.8f, 1.8f), RockTint = TownSwatch.Basalt,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.PumiceRock, OverworldDecor.StoneRose, OverworldDecor.Fumarole },
                Tufts = 0.02f, TuftColor = TownSwatch.Straw, TuftTip = TownSwatch.GroveFlower, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The malpaís (67): rough black lava (#3A2E2A) in flows and lobes, glowing cracks, fumaroles.
            styles[(int)OverworldTerrain.Malpais] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Malpais, Shape = ReliefShape.LavaFlows, Relief = 14f, ReliefScale = 190f, ReliefDetail = 0.2f,
                Base = TownSwatch.HollowDark, Alt = TownSwatch.Black, Accent = TownSwatch.ColdEmber, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Black, Crest = TownSwatch.Basalt,
                Patches = new[] { TownSwatch.Black, TownSwatch.ColdEmber, TownSwatch.StoneDark }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.Black, Slope = TownSwatch.HollowDark, Bank = TownSwatch.Black, Bed = TownSwatch.ColdEmber,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.02f, RockScale = new Vector2(0.8f, 2f), RockTint = TownSwatch.Black,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.LavaBoulder, OverworldDecor.Fumarole },
                Tufts = 0f, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The hot channels (68): steaming water between black glass, ember coral.
            styles[(int)OverworldTerrain.HotChannels] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HotChannels, Shape = ReliefShape.Islets, Relief = 4f, ReliefScale = 190f, ReliefDetail = 0.06f,
                Base = TownSwatch.Black, Alt = TownSwatch.Basalt, Accent = TownSwatch.ReefTeal, Mottle = TownSwatch.Indigo, Hollow = TownSwatch.ReefTeal, Crest = TownSwatch.GoodSmoke,
                Patches = new[] { TownSwatch.ReefTeal, TownSwatch.Indigo, TownSwatch.GoodSmoke }, AltThreshold = 0.52f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Slate, Clearing = TownSwatch.Basalt, Cliff = TownSwatch.Black, Slope = TownSwatch.Basalt, Bank = TownSwatch.Black, Bed = TownSwatch.ReefTeal,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.01f, RockTint = TownSwatch.Black,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.ObsidianShard, OverworldDecor.EmberCoral },
                Tufts = 0f, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The Recife de Obsidiana (68): black shining glass (#1E1A1E) in sharp ridges, ember coral (#E8613A), steam.
            styles[(int)OverworldTerrain.ObsidianReef] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ObsidianReef, Shape = ReliefShape.GlassReef, Relief = 18f, ReliefScale = 200f, ReliefDetail = 0.1f,
                Base = TownSwatch.Black, Alt = TownSwatch.HollowDark, Accent = TownSwatch.CoralLive, Mottle = TownSwatch.Indigo, Hollow = TownSwatch.HollowDark, Crest = TownSwatch.Indigo,
                Patches = new[] { TownSwatch.Indigo, TownSwatch.CoralLive, TownSwatch.GoodSmoke }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Slate, Clearing = TownSwatch.HollowDark, Cliff = TownSwatch.Indigo, Slope = TownSwatch.Black, Bank = TownSwatch.Black, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.008f, RockTint = TownSwatch.Black,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.ObsidianShard, OverworldDecor.EmberCoral, OverworldDecor.ObsidianShard },
                Tufts = 0f, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The glass islets (68): low islets of obsidian in the warm sea, ember coral, the reef's coral heads.
            styles[(int)OverworldTerrain.GlassIslets] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.GlassIslets, Shape = ReliefShape.Islets, Relief = 5f, ReliefScale = 180f, ReliefDetail = 0.08f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.Black, Accent = TownSwatch.CoralLive, Mottle = TownSwatch.ReefTeal, Hollow = TownSwatch.Sea, Crest = TownSwatch.SandDark,
                Patches = new[] { TownSwatch.ReefTeal, TownSwatch.CoralLive, TownSwatch.SandDark }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Basalt, Cliff = TownSwatch.Black, Slope = TownSwatch.Basalt, Bank = TownSwatch.SandDark, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.01f, RockTint = TownSwatch.Black,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.EmberCoral, OverworldDecor.CoralHead, OverworldDecor.ObsidianShard },
                Tufts = 0.02f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.ReefTeal, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The curved walls (69): ramparts of coral stone (#E8D9C0) in rings round the castle, red gates in the gaps.
            styles[(int)OverworldTerrain.CurvedWalls] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CurvedWalls, Shape = ReliefShape.CurvedRamparts, Relief = 12f, ReliefScale = 200f, ReliefDetail = 0.1f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Moss, Mottle = TownSwatch.Slate, Hollow = TownSwatch.StoneDark, Crest = TownSwatch.CoralLime,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Slate, TownSwatch.GoodSmoke }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.GoodSmoke, Clearing = TownSwatch.Slate, Cliff = TownSwatch.CoralLime, Slope = TownSwatch.Pearl, Bank = TownSwatch.Basalt, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.008f, Rocks = 0.006f, RockTint = TownSwatch.CoralLime,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.StoneLantern, OverworldDecor.CloudPine },
                Tufts = 0.06f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The castle's courts (69): black stone courts, furnace towers, slag, lacquer gates.
            styles[(int)OverworldTerrain.NineFurnaces] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.NineFurnaces, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 7f, ReliefScale = 220f, ReliefDetail = 0.06f,
                Base = TownSwatch.Slate, Alt = TownSwatch.Basalt, Accent = TownSwatch.Lacquer, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Black, Crest = TownSwatch.CoralLime,
                Patches = new[] { TownSwatch.Basalt, TownSwatch.ColdEmber, TownSwatch.CoralLime }, AltThreshold = 0.5f, AccentThreshold = 0.96f,
                Trail = TownSwatch.CoralLime, Clearing = TownSwatch.Slate, Cliff = TownSwatch.CoralLime, Slope = TownSwatch.Basalt, Bank = TownSwatch.Basalt, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.006f, RockTint = TownSwatch.Basalt,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.FurnaceTower, OverworldDecor.SlagHeap },
                Tufts = 0f, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Slag fields (69): heaps of slag and cinder, iron-red ground, chimneys.
            styles[(int)OverworldTerrain.SlagFields] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SlagFields, Shape = ReliefShape.Mounds, Relief = 8f, ReliefScale = 180f, ReliefDetail = 0.15f,
                Base = TownSwatch.ColdEmber, Alt = TownSwatch.PotIron, Accent = TownSwatch.Ember, Mottle = TownSwatch.Laterite, Hollow = TownSwatch.Black, Crest = TownSwatch.PotIron,
                Patches = new[] { TownSwatch.PotIron, TownSwatch.Laterite, TownSwatch.Black }, AltThreshold = 0.5f, AccentThreshold = 0.96f,
                Trail = TownSwatch.PotIron, Clearing = TownSwatch.Laterite, Cliff = TownSwatch.Black, Slope = TownSwatch.ColdEmber, Bank = TownSwatch.Black, Bed = TownSwatch.Black,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.012f, RockTint = TownSwatch.PotIron,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.SlagHeap, OverworldDecor.SlagHeap, OverworldDecor.FurnaceTower },
                Tufts = 0f, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The ash plains (70): grey ash (#6A6460) on cold stone, burnt stumps, cinder heaps.
            styles[(int)OverworldTerrain.AshPlain] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.AshPlain, Relief = 4f, ReliefScale = 260f, ReliefDetail = 0.08f,
                Base = TownSwatch.PotIron, Alt = TownSwatch.Stone, Accent = TownSwatch.GoodSmoke, Mottle = TownSwatch.Granite, Hollow = TownSwatch.StoneDark, Crest = TownSwatch.GoodSmoke,
                Patches = new[] { TownSwatch.GoodSmoke, TownSwatch.Black, TownSwatch.Granite }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.StoneDark, Clearing = TownSwatch.Granite, Cliff = TownSwatch.Black, Slope = TownSwatch.Stone, Bank = TownSwatch.StoneDark, Bed = TownSwatch.Black,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.01f, RockTint = TownSwatch.PotIron,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.Stump, OverworldDecor.SlagHeap, OverworldDecor.LavaBoulder },
                Tufts = 0f, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The caldera (70): a ring of cold stone round the lava lake, cinder and black glass, a red-dark sky.
            styles[(int)OverworldTerrain.Caldera] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Caldera, Shape = ReliefShape.CalderaRim, Relief = 30f, ReliefScale = 220f, ReliefDetail = 0.15f,
                Base = TownSwatch.Black, Alt = TownSwatch.PotIron, Accent = TownSwatch.Ember, Mottle = TownSwatch.ColdEmber, Hollow = TownSwatch.ColdEmber, Crest = TownSwatch.Granite,
                Patches = new[] { TownSwatch.ColdEmber, TownSwatch.PotIron, TownSwatch.Granite }, AltThreshold = 0.5f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Granite, Clearing = TownSwatch.PotIron, Cliff = TownSwatch.Granite, Slope = TownSwatch.Black, Bank = TownSwatch.Black, Bed = TownSwatch.ColdEmber,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.016f, RockScale = new Vector2(0.8f, 2f), RockTint = TownSwatch.Granite,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.LavaBoulder, OverworldDecor.ObsidianShard },
                Tufts = 0f, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = pines,
            };
        }
    }
}
