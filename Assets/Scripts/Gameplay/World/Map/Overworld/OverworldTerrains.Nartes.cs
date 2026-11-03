using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 9 — Estepe dos Nartes: the look of the steppe's terrains (160–179, Docs/Mapas100.md §5 região 9, maps 47–52):
    /// tall feather grass (capim <c>#BFAE5A</c>), shallow rivers on shingle with willows and the wide ford, the centaurs' camp of
    /// felt tents (feltro <c>#D8C9A8</c>), the golden kurgans with their stone babas and Scythian stags (ouro cita
    /// <c>#D4A93A</c>), the silver wormwood and its salt pans, birch groves of the forest-steppe, the hill giants' green hills
    /// ringed by earthen ramparts and their vale of dream tablets, the taiga of giant larches, poppy prairies (papoula
    /// <c>#D9453A</c>), the short-grass plains of the wild herds with their stone wells, and the Caucasus (montanha
    /// <c>#6E7480</c>): svan towers in the high valleys, the crags of Amirani's chain and the snow crest. The relief stays low
    /// on the steppe (≤ 6 m), rolls on the prairies (~9 m), rises on the giants' hills (~14 m), the kurgans (~11 m mounds)
    /// and the taiga, and climbs into the Caucasus (svan valleys ~20 m, the chain's crags ~34 m, the crest ~40 m). Trees:
    /// the Zarafshan's poplars by the water, the Serra's cedars and pines on the high ground; the region's own pieces
    /// (OverworldDecorKit.Nartes.cs) are the felt tents, horse standards, stone babas, golden stags, birches, larches, dream
    /// tablets, giant pots, svan towers, the chain, the stone wells and the earthen ramparts. Only the palette rows every
    /// build already has (no dependency on the newer city rows).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>An Estepe dos Nartes terrain (Região 9, 160–179).</summary>
        public static bool IsNartes(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 160 && i < 180;
        }

        private static void NartesStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation plain = TownVegetation.Forest;
            TownVegetation hills = TownVegetation.Mountain;
            int[] poplars = { 0, 1 };
            int[] cedars = { 0, 2 };

            // The open steppe of feather grass: straw-gold waves, few trees, horse standards and a stone baba here and there.
            styles[(int)OverworldTerrain.FeatherSteppe] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FeatherSteppe, Relief = 5f, ReliefScale = 300f, ReliefDetail = 0.08f,
                Base = TownSwatch.GoldGrass, Alt = TownSwatch.Straw, Accent = TownSwatch.Grass, Mottle = TownSwatch.Thatch, Hollow = TownSwatch.Grass,
                Crest = TownSwatch.Straw, Patches = new[] { TownSwatch.Straw, TownSwatch.Grass, TownSwatch.Reed }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.Loam, Slope = TownSwatch.Straw, Bank = TownSwatch.Sand, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.002f, Bushes = 0.01f, Rocks = 0.003f,
                Specials = 0.004f, SpecialMix = new[] { OverworldDecor.HorseStandard, OverworldDecor.StoneBaba },
                Tufts = 0.34f, TuftColor = TownSwatch.Straw, TuftTip = TownSwatch.GoldGrass, Flowers = 0.02f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothYellow },
                WaterVegetation = plain, WaterVariants = poplars,
            };

            // The shallow rivers: shingle bars, green banks, willows (the Zarafshan's poplars behind them), reeds by the fords.
            styles[(int)OverworldTerrain.SteppeRiver] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SteppeRiver, Relief = 3f, ReliefScale = 220f, ReliefDetail = 0.06f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GoldGrass, Accent = TownSwatch.Cobble, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.GoldGrass, TownSwatch.Cobble, TownSwatch.Reed }, AltThreshold = 0.56f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Cobble, Bed = TownSwatch.Stone,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.012f, Bushes = 0.04f, Rocks = 0.006f,
                Specials = 0.03f, Special = OverworldDecor.Willow,
                Tufts = 0.22f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.GoldGrass, Flowers = 0.03f, Reeds = 0.07f, Pebbles = 0.05f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow },
            };

            // The centaurs' camp (47): trampled grass and bare loam round the felt tents, horse standards, a well.
            styles[(int)OverworldTerrain.HorseCamp] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HorseCamp, Relief = 2.5f, ReliefScale = 260f, ReliefDetail = 0.05f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GoldGrass, Accent = TownSwatch.Loam, Mottle = TownSwatch.Straw, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Loam, TownSwatch.Straw, TownSwatch.Dirt }, AltThreshold = 0.52f, AccentThreshold = 0.82f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Cobble, Bed = TownSwatch.Stone,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.002f, Bushes = 0.008f, Rocks = 0.002f, Bales = 0.006f,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.HorseStandard, OverworldDecor.HorseStandard, OverworldDecor.FeltYurt },
                Tufts = 0.22f, TuftColor = TownSwatch.GoldGrass, TuftTip = TownSwatch.Straw, Flowers = 0.03f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite, TownSwatch.Violet },
            };

            // The golden kurgans (48): mounds of earth capped with stones, thin grass, babas and stags of gold.
            styles[(int)OverworldTerrain.KurganField] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.KurganField, Shape = ReliefShape.Kurgans, Relief = 11f, ReliefScale = 170f, ReliefDetail = 0.05f,
                Base = TownSwatch.GoldGrass, Alt = TownSwatch.Loam, Accent = TownSwatch.Stone, Mottle = TownSwatch.Thatch, Hollow = TownSwatch.Grass,
                Crest = TownSwatch.TerraceStone, Patches = new[] { TownSwatch.Loam, TownSwatch.Stone, TownSwatch.Straw }, AltThreshold = 0.56f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.TerraceStone, Slope = TownSwatch.Loam, Bank = TownSwatch.Sand, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.001f, Bushes = 0.008f, Rocks = 0.012f, RockTint = TownSwatch.TerraceStone,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.StoneBaba, OverworldDecor.StoneBaba, OverworldDecor.GoldStag },
                Tufts = 0.26f, TuftColor = TownSwatch.Straw, TuftTip = TownSwatch.GoldGrass, Flowers = 0.015f, Pebbles = 0.03f,
                FlowerColors = new[] { TownSwatch.ClothYellow },
            };

            // The silver wormwood steppe: grey-green tussocks, white salt pans, bones of the herds, the odd baba.
            styles[(int)OverworldTerrain.WormwoodSteppe] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WormwoodSteppe, Relief = 4f, ReliefScale = 280f, ReliefDetail = 0.06f,
                Base = TownSwatch.Aloe, Alt = TownSwatch.Straw, Accent = TownSwatch.LimeWhite, Mottle = TownSwatch.Sand, Hollow = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Sand, TownSwatch.LimeWhite, TownSwatch.Straw }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Sand, Clearing = TownSwatch.Sand, Cliff = TownSwatch.SandDark, Slope = TownSwatch.Sand, Bank = TownSwatch.LimeWhite, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.001f, Bushes = 0.03f, Rocks = 0.004f, BushVegetation = TownVegetation.Desert,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.Bones, OverworldDecor.StoneBaba },
                Tufts = 0.3f, TuftColor = TownSwatch.Aloe, TuftTip = TownSwatch.LimeWhite, Pebbles = 0.02f,
                WaterVegetation = plain, WaterVariants = poplars,
            };

            // The forest-steppe: birch groves on low hills, golden grass between them.
            styles[(int)OverworldTerrain.BirchSteppe] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BirchSteppe, Shape = ReliefShape.Hills, Relief = 7f, ReliefScale = 230f, ReliefDetail = 0.12f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GoldGrass, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.LeafGold, TownSwatch.GoldGrass, TownSwatch.Moss }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.006f, Bushes = 0.03f, Rocks = 0.004f, Logs = 0.004f, Stumps = 0.004f,
                Specials = 0.08f, Special = OverworldDecor.Birch,
                Tufts = 0.22f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.GoldGrass, Flowers = 0.05f, Ferns = 0.03f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Violet, TownSwatch.ClothYellow },
            };

            // The hill giants' green hills (49): rounded tops ringed by terraces, earthen ramparts, giant pots, boulders.
            styles[(int)OverworldTerrain.GiantHills] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.GiantHills, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 14f, ReliefScale = 210f, ReliefDetail = 0.15f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Loam, Mottle = TownSwatch.Vine, Hollow = TownSwatch.Moss, Crest = TownSwatch.Vine,
                Patches = new[] { TownSwatch.Vine, TownSwatch.Loam, TownSwatch.GoldGrass }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.Loam, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.006f, Bushes = 0.02f, Rocks = 0.012f, RockScale = new Vector2(1.2f, 2.6f), RockTint = TownSwatch.Stone,
                Specials = 0.003f, Special = OverworldDecor.GiantCauldron,
                Tufts = 0.2f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Vine, Flowers = 0.04f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite },
                WaterVegetation = plain, WaterVariants = poplars,
            };

            // The vale of the dream tablets (49): a mossy green valley, standing slabs, ferns.
            styles[(int)OverworldTerrain.DreamVale] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DreamVale, Shape = ReliefShape.Hills, Relief = 10f, ReliefScale = 200f, ReliefDetail = 0.15f,
                Base = TownSwatch.Moss, Alt = TownSwatch.Grass, Accent = TownSwatch.StoneDark, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Grass, TownSwatch.StoneDark, TownSwatch.Vine }, AltThreshold = 0.52f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Moss, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.008f, Bushes = 0.03f, Rocks = 0.01f, MossRocks = 0.01f, RockTint = TownSwatch.StoneDark,
                Specials = 0.02f, Special = OverworldDecor.DreamTablet,
                Tufts = 0.16f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Moss, Ferns = 0.08f, Flowers = 0.03f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothWhite },
            };

            // The taiga (50): giant larches and dark cedars over moss, logs and ferns.
            styles[(int)OverworldTerrain.Taiga] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Taiga, Shape = ReliefShape.Hills, Relief = 9f, ReliefScale = 220f, ReliefDetail = 0.2f,
                Base = TownSwatch.Moss, Alt = TownSwatch.Pine, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Pine, TownSwatch.LeafGold, TownSwatch.Mud }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.DirtDark, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Moss, Bank = TownSwatch.Mud, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.03f, Bushes = 0.03f, Rocks = 0.006f, MossRocks = 0.01f, Logs = 0.008f, Stumps = 0.006f,
                Specials = 0.1f, Special = OverworldDecor.Larch,
                Tufts = 0.12f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Moss, Ferns = 0.14f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The poppy prairie (51): long green-gold swells red with poppies.
            styles[(int)OverworldTerrain.PoppyPrairie] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PoppyPrairie, Relief = 9f, ReliefScale = 240f, ReliefDetail = 0.06f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GoldGrass, Accent = TownSwatch.ClothRed, Mottle = TownSwatch.Vine, Hollow = TownSwatch.GrassDark,
                Crest = TownSwatch.GoldGrass, Patches = new[] { TownSwatch.ClothRed, TownSwatch.GoldGrass, TownSwatch.Vine }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.Loam, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.002f, Bushes = 0.01f, Rocks = 0.002f,
                Specials = 0.002f, Special = OverworldDecor.HorseStandard,
                Tufts = 0.2f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.GoldGrass, Flowers = 0.16f, Pebbles = 0.005f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothRed, TownSwatch.SilkRed, TownSwatch.ClothWhite },
            };

            // The plains of the wild herds (51): short turf, cairns, the stone wells.
            styles[(int)OverworldTerrain.HorsePlains] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HorsePlains, Relief = 6f, ReliefScale = 320f, ReliefDetail = 0.05f,
                Base = TownSwatch.Vine, Alt = TownSwatch.Grass, Accent = TownSwatch.GoldGrass, Mottle = TownSwatch.Straw, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.GoldGrass, TownSwatch.Straw, TownSwatch.Loam }, AltThreshold = 0.54f, AccentThreshold = 0.84f,
                Trail = TownSwatch.Loam, Clearing = TownSwatch.Loam, Cliff = TownSwatch.Loam, Slope = TownSwatch.Grass, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = plain, TreeVariants = poplars, Trees = 0.001f, Bushes = 0.006f, Rocks = 0.004f,
                Specials = 0.004f, SpecialMix = new[] { OverworldDecor.Cairn, OverworldDecor.StoneBaba, OverworldDecor.Cairn },
                Tufts = 0.18f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Straw, Flowers = 0.06f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow, TownSwatch.Violet },
            };

            // The svan valleys of the Caucasus (52): steep green slopes, stone walls, birch woods, svan towers.
            styles[(int)OverworldTerrain.SvanValley] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SvanValley, Shape = ReliefShape.Hills, Relief = 20f, ReliefScale = 210f, ReliefDetail = 0.3f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Granite, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Moss, Crest = TownSwatch.Granite,
                Patches = new[] { TownSwatch.Granite, TownSwatch.Moss, TownSwatch.GoldGrass }, AltThreshold = 0.54f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Granite, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.012f, Bushes = 0.02f, Rocks = 0.02f, RockTint = TownSwatch.Granite,
                Specials = 0.03f, Special = OverworldDecor.Birch,
                Tufts = 0.16f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.GoldGrass, Flowers = 0.04f, Pebbles = 0.03f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Violet, TownSwatch.ClothWhite },
                WaterVegetation = plain, WaterVariants = poplars,
            };

            // The crags of Amirani's chain (52): grey rock, scree, old snow in the hollows, twisted pines.
            styles[(int)OverworldTerrain.ChainPeak] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ChainPeak, Shape = ReliefShape.Peaks, Relief = 34f, ReliefScale = 230f, ReliefDetail = 0.5f,
                Base = TownSwatch.Granite, Alt = TownSwatch.Stone, Accent = TownSwatch.Snow, Mottle = TownSwatch.StoneDark, Crest = TownSwatch.Snow, Hollow = TownSwatch.Firn,
                Patches = new[] { TownSwatch.Snow, TownSwatch.StoneDark, TownSwatch.Moss }, AltThreshold = 0.52f, AccentThreshold = 0.82f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Granite, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.004f, Bushes = 0.008f, Rocks = 0.05f, RockScale = new Vector2(1f, 2.8f), RockTint = TownSwatch.Granite,
                Specials = 0.02f, Special = OverworldDecor.MountainPine,
                Tufts = 0.08f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Straw, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The snow crest under Ohya's summit (52): snow and ice over black rock, nothing grows.
            styles[(int)OverworldTerrain.SnowCrest] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SnowCrest, Shape = ReliefShape.Massif, Relief = 40f, ReliefScale = 240f, ReliefDetail = 0.4f,
                Base = TownSwatch.Snow, Alt = TownSwatch.Firn, Accent = TownSwatch.Basalt, Mottle = TownSwatch.Granite, Crest = TownSwatch.Snow, Hollow = TownSwatch.Firn,
                Patches = new[] { TownSwatch.Firn, TownSwatch.Basalt, TownSwatch.Granite }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Firn, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Granite, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0f, Bushes = 0.002f, Rocks = 0.04f, RockScale = new Vector2(1f, 2.4f), RockTint = TownSwatch.Basalt,
                Specials = 0.012f, Special = OverworldDecor.RockSpire,
                Tufts = 0.02f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Firn, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = cedars,
            };
        }
    }
}
