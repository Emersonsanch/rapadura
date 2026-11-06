using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 10 — Picos de Farnkesh: the look of the peaks' terrains (180–199, Docs/Mapas100.md §5 região 10, maps 53–58): the
    /// satyrs' vale of vines and woods at the foot of the peaks (verde-claro <c>#9CCB6A</c>, vime <c>#C9A76A</c>) with the Pactolo's
    /// golden pebbles (ouro do rio <c>#E3B341</c>) and its ring of glades, the chestnut woods of the first slopes, the high meadows of
    /// wind bells, the forest of sandstone pinnacles (arenito <c>#B58A60</c>), Farnkesh's stone nests (cal <c>#F1ECE2</c>, madeira
    /// vermelha <c>#9A3B2E</c>), the sea of clouds (nuvem <c>#EEF3F7</c>), the rope wind bridges, the cliff of the hanging
    /// hermitages, the grey Gorge of the Ays and its scree (poeira <c>#C9B79A</c>), and the cloud isles round Huma's golden nest.
    /// The relief stays low in the vale (≤ 8 m), rolls in the woods and meadows (~10–12 m), and stands up in towers and walls:
    /// the pinnacles (~26–34 m), the hermitages' cliff (~30 m), the gorge's walls (~32 m) and the cloud isles (~22–28 m), each over
    /// a low floor where the trails and clearings run. Trees: the Zarafshan's poplars by the water and in the vale, the Serra's
    /// cedars and pines on the heights; the region's own pieces (OverworldDecorKit.Farnkesh.cs) are the sandstone pinnacles,
    /// hermitages, wind bells, rope bridges, paper streamers, rope baskets, vines, gourd huts, stone stages, shelter stones,
    /// twisted trees and the golden nest. Only the palette rows every build already has (no dependency on the newer city rows).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Picos de Farnkesh terrain (Região 10, 180–199).</summary>
        public static bool IsFarnkesh(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 180 && i < 200;
        }

        private static void FarnkeshStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation plain = TownVegetation.Forest;
            TownVegetation hills = TownVegetation.Mountain;
            int[] poplars = { 0, 1 };
            int[] woods = { 0, 1, 2 };
            int[] cedars = { 0, 2 };

            // The satyrs' vineyards (53): light green slopes, vines on stakes, wicker, flowers.
            styles[(int)OverworldTerrain.VineVale] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.VineVale, Shape = ReliefShape.Hills, Relief = 8f, ReliefScale = 230f, ReliefDetail = 0.1f,
                Base = TownSwatch.Vine, Alt = TownSwatch.Grass, Accent = TownSwatch.Loam, Mottle = TownSwatch.GoldGrass, Hollow = TownSwatch.GrassDark,
                Crest = TownSwatch.GoldGrass, Patches = new[] { TownSwatch.Loam, TownSwatch.GoldGrass, TownSwatch.Straw }, AltThreshold = 0.52f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Grass, Bank = TownSwatch.Sand, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.004f, Bushes = 0.02f, Rocks = 0.003f,
                Specials = 0.05f, Special = OverworldDecor.GrapeVine,
                Tufts = 0.2f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Flowers = 0.06f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothYellow, TownSwatch.ClothWhite },
            };

            // The Pactolo (53): shingle bars of golden pebbles, green banks, poplars and reeds.
            styles[(int)OverworldTerrain.GoldenRiver] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.GoldenRiver, Relief = 3f, ReliefScale = 220f, ReliefDetail = 0.06f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.Sand, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Sand, TownSwatch.Cobble, TownSwatch.Reed }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Sand, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Sand, Bed = TownSwatch.Cobble,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.014f, Bushes = 0.04f, Rocks = 0.005f,
                Specials = 0.004f, Special = OverworldDecor.StoneStage,
                Tufts = 0.22f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Flowers = 0.04f, Reeds = 0.08f, Pebbles = 0.05f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow },
            };

            // The satyrs' glades (53): soft lawns in a ring of woods, stone stages, gourd huts, flowers (flor #E27A9B).
            styles[(int)OverworldTerrain.SatyrGlades] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SatyrGlades, Relief = 3.5f, ReliefScale = 250f, ReliefDetail = 0.06f,
                Base = TownSwatch.Vine, Alt = TownSwatch.Grass, Accent = TownSwatch.Stone, Mottle = TownSwatch.Moss, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Stone, TownSwatch.Moss }, AltThreshold = 0.54f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Grass, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Sand, Bed = TownSwatch.Cobble,
                Vegetation = plain, TreeVariants = woods, Trees = 0.02f, Bushes = 0.04f, Rocks = 0.004f, MossRocks = 0.004f,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.GourdHut, OverworldDecor.StoneStage, OverworldDecor.GourdHut },
                Tufts = 0.16f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Flowers = 0.12f, Ferns = 0.02f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.TaipaRose, TownSwatch.TaipaRose, TownSwatch.ClothWhite, TownSwatch.ClothYellow },
            };

            // The woods at the foot of the peaks: chestnuts and oaks (the forest's broadleaves), ferns, mossy boulders, logs.
            styles[(int)OverworldTerrain.FootWoods] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FootWoods, Shape = ReliefShape.Hills, Relief = 12f, ReliefScale = 210f, ReliefDetail = 0.2f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Moss, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.LeafDark, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.Moss, TownSwatch.Mud }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Trail = TownSwatch.DirtDark, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Moss, Bank = TownSwatch.Mud, Bed = TownSwatch.StoneDark,
                Vegetation = plain, TreeVariants = woods, Trees = 0.05f, Bushes = 0.04f, Rocks = 0.008f, MossRocks = 0.012f, Logs = 0.008f, Stumps = 0.006f,
                Specials = 0.006f, Special = OverworldDecor.TwistedTree,
                Tufts = 0.12f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Moss, Ferns = 0.14f, Flowers = 0.02f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Violet },
                WaterVegetation = plain, WaterVariants = poplars,
            };

            // The high meadows under the pinnacles: rolling flowered grass, wind bells on poles, paper streamers.
            styles[(int)OverworldTerrain.AlpineMeadow] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.AlpineMeadow, Relief = 9f, ReliefScale = 260f, ReliefDetail = 0.08f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.GoldGrass, Mottle = TownSwatch.Moss, Hollow = TownSwatch.GrassDark,
                Crest = TownSwatch.GoldGrass, Patches = new[] { TownSwatch.GoldGrass, TownSwatch.Stone, TownSwatch.Vine }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.Grass, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.004f, Bushes = 0.012f, Rocks = 0.008f, RockTint = TownSwatch.Sandstone,
                Specials = 0.004f, SpecialMix = new[] { OverworldDecor.WindBell, OverworldDecor.Cairn },
                Tufts = 0.2f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.GoldGrass, Flowers = 0.12f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothYellow, TownSwatch.ClothWhite, TownSwatch.SilkBlue },
            };

            // The forest of sandstone pinnacles (Meteora's form): banded towers over pines and scree, hermitages on top.
            styles[(int)OverworldTerrain.PinnacleWood] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PinnacleWood, Shape = ReliefShape.Pinnacles, Relief = 30f, ReliefScale = 200f, ReliefDetail = 0.2f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Pine, Accent = TownSwatch.Sandstone, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Moss, Crest = TownSwatch.Sandstone,
                Patches = new[] { TownSwatch.Sandstone, TownSwatch.Moss, TownSwatch.Stone }, AltThreshold = 0.52f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.AdobeOchre, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.03f, Bushes = 0.02f, Rocks = 0.012f, RockTint = TownSwatch.Sandstone,
                Specials = 0.004f, Special = OverworldDecor.Hermitage,
                Tufts = 0.1f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Moss, Ferns = 0.04f, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // Farnkesh, the Stone Nests (54): sandstone towers round the city's platform, white hermitages, rope baskets, wind bells.
            styles[(int)OverworldTerrain.StoneNests] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.StoneNests, Shape = ReliefShape.Pinnacles, Relief = 34f, ReliefScale = 210f, ReliefDetail = 0.2f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Sandstone, Accent = TownSwatch.LimeWhite, Mottle = TownSwatch.AdobeOchre, Hollow = TownSwatch.GrassDark,
                Crest = TownSwatch.LimeWhite, Patches = new[] { TownSwatch.Sandstone, TownSwatch.LimeWhite, TownSwatch.Moss }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.AdobeOchre, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.008f, Bushes = 0.012f, Rocks = 0.008f, RockTint = TownSwatch.Sandstone,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.Hermitage, OverworldDecor.WindBell, OverworldDecor.RopeBasket },
                Tufts = 0.12f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.GoldGrass, Flowers = 0.04f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow },
            };

            // The sea of clouds: a low pale floor (cloud-white grass, wet stone) under drifting banks, rare pines.
            styles[(int)OverworldTerrain.CloudSea] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CloudSea, Relief = 3f, ReliefScale = 260f, ReliefDetail = 0.06f,
                Base = TownSwatch.LimeWhite, Alt = TownSwatch.Firn, Accent = TownSwatch.Stone, Mottle = TownSwatch.Plaster, Hollow = TownSwatch.Firn,
                Patches = new[] { TownSwatch.Firn, TownSwatch.Stone, TownSwatch.Moss }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Firn, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.003f, Bushes = 0.006f, Rocks = 0.006f,
                Specials = 0.003f, Special = OverworldDecor.SandstonePinnacle,
                Tufts = 0.08f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Firn, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The wind bridges (55): small pinnacles of sandstone over the clouds, rope and plank, streamers.
            styles[(int)OverworldTerrain.WindBridges] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WindBridges, Shape = ReliefShape.Pinnacles, Relief = 26f, ReliefScale = 180f, ReliefDetail = 0.15f,
                Base = TownSwatch.Firn, Alt = TownSwatch.Sandstone, Accent = TownSwatch.LimeWhite, Mottle = TownSwatch.Stone, Hollow = TownSwatch.Firn, Crest = TownSwatch.GoldGrass,
                Patches = new[] { TownSwatch.LimeWhite, TownSwatch.Sandstone, TownSwatch.Stone }, AltThreshold = 0.52f, AccentThreshold = 0.86f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.AdobeOchre, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.004f, Bushes = 0.006f, Rocks = 0.01f, RockTint = TownSwatch.Sandstone,
                Specials = 0.004f, Special = OverworldDecor.WindBell,
                Tufts = 0.06f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Firn, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The cliff of the hanging hermitages (56): a stepped rock wall, white houses on the ledges, ladders, cracked bells.
            styles[(int)OverworldTerrain.BellCliffs] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BellCliffs, Shape = ReliefShape.Terraced, TerraceSteps = 4, Relief = 30f, ReliefScale = 200f, ReliefDetail = 0.25f,
                Base = TownSwatch.Stone, Alt = TownSwatch.Sandstone, Accent = TownSwatch.LimeWhite, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Moss, Crest = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Sandstone, TownSwatch.Moss, TownSwatch.StoneDark }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.008f, Bushes = 0.012f, Rocks = 0.02f, RockScale = new Vector2(1f, 2.4f), RockTint = TownSwatch.Stone,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.Hermitage, OverworldDecor.WindBell },
                Tufts = 0.08f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Moss, Ferns = 0.02f, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The Gorge of the Ays (57): grey walls, dust floor (poeira #C9B79A), twisted trees, shelter stones.
            styles[(int)OverworldTerrain.AysGorge] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.AysGorge, Shape = ReliefShape.NarrowGorge, Relief = 32f, ReliefScale = 230f, ReliefDetail = 0.3f,
                Base = TownSwatch.Sand, Alt = TownSwatch.Stone, Accent = TownSwatch.StoneDark, Mottle = TownSwatch.SandDark, Hollow = TownSwatch.Sand, Crest = TownSwatch.Granite,
                Patches = new[] { TownSwatch.SandDark, TownSwatch.Stone, TownSwatch.Granite }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Sand, Clearing = TownSwatch.Sand, Cliff = TownSwatch.Granite, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.002f, Bushes = 0.01f, Rocks = 0.03f, RockScale = new Vector2(1f, 2.6f), RockTint = TownSwatch.Granite,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.TwistedTree, OverworldDecor.ShelterStone },
                Tufts = 0.05f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Straw, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The wind-scoured scree above the gorge: grey shingle, twisted junipers, cairns.
            styles[(int)OverworldTerrain.WindScree] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WindScree, Shape = ReliefShape.Peaks, Relief = 24f, ReliefScale = 220f, ReliefDetail = 0.4f,
                Base = TownSwatch.Stone, Alt = TownSwatch.Granite, Accent = TownSwatch.Sand, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Sand, Crest = TownSwatch.Granite,
                Patches = new[] { TownSwatch.Sand, TownSwatch.StoneDark, TownSwatch.Thatch }, AltThreshold = 0.52f, AccentThreshold = 0.85f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Granite, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.003f, Bushes = 0.01f, Rocks = 0.05f, RockScale = new Vector2(1f, 2.4f), RockTint = TownSwatch.Granite,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.TwistedTree, OverworldDecor.Cairn, OverworldDecor.ShelterStone },
                Tufts = 0.06f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Straw, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The cloud isles (58): sheer-sided stone isles with gold-white grass on top over the cloud floor.
            styles[(int)OverworldTerrain.CloudIsles] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CloudIsles, Shape = ReliefShape.CloudIsles, Relief = 22f, ReliefScale = 200f, ReliefDetail = 0.1f,
                Base = TownSwatch.Firn, Alt = TownSwatch.LimeWhite, Accent = TownSwatch.GoldGrass, Mottle = TownSwatch.Plaster, Hollow = TownSwatch.Firn, Crest = TownSwatch.GoldGrass,
                Patches = new[] { TownSwatch.GoldGrass, TownSwatch.LimeWhite, TownSwatch.Stone }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Stone, Slope = TownSwatch.StoneDark, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.002f, Bushes = 0.004f, Rocks = 0.008f, RockTint = TownSwatch.LimeWhite,
                Specials = 0.004f, Special = OverworldDecor.RockSpire,
                Tufts = 0.1f, TuftColor = TownSwatch.GoldGrass, TuftTip = TownSwatch.Gold, Flowers = 0.02f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow },
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // Huma's nest (58): the highest isles, white rock and gold, the giant nest of golden branches.
            styles[(int)OverworldTerrain.HumaNest] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HumaNest, Shape = ReliefShape.CloudIsles, Relief = 28f, ReliefScale = 220f, ReliefDetail = 0.1f,
                Base = TownSwatch.GoldGrass, Alt = TownSwatch.LimeWhite, Accent = TownSwatch.Gold, Mottle = TownSwatch.Straw, Hollow = TownSwatch.Firn, Crest = TownSwatch.Gold,
                Patches = new[] { TownSwatch.Gold, TownSwatch.LimeWhite, TownSwatch.Firn }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Trail = TownSwatch.LimeWhite, Clearing = TownSwatch.Stone, Cliff = TownSwatch.LimeWhite, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0f, Bushes = 0.004f, Rocks = 0.006f, RockTint = TownSwatch.LimeWhite,
                Specials = 0.003f, Special = OverworldDecor.GoldenNest,
                Tufts = 0.12f, TuftColor = TownSwatch.GoldGrass, TuftTip = TownSwatch.Gold, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = cedars,
            };
        }
    }
}
