using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 7 — Teto do Mundo: the look of the high-mountain terrains (120–139, Docs/Mapas100.md §5 região 7, maps 36–40):
    /// green terraced valleys with apricots in flower and poplars along the channels round Sangkent, golden high steppe,
    /// grey gravel of the meltwater and the moraines, turquoise lake shore, polished stone of the chutes, snow and blue ice
    /// higher up, the fort's grey crag and the dragon's black rock. The relief is kept ≤ 16 m where a town, a post, a gate
    /// or the river runs (their flat ground and valley sides must stay walkable) and rises to 26–36 m on the seracs, the
    /// ridges and the peaks. Trees: the Zarafshan's poplars and tamarisks in the valleys, the Serra's cedars (junipers) on the
    /// slopes, almost nothing on the ice; the Teto's own pieces (OverworldDecorKit.Pamir.cs) are the apricots in flower,
    /// old junipers, stone huts, chutes and their stations, seracs, wind wheels, watch towers, ribbon lines and Aydahar's
    /// shed scales. Only the shared palette rows every build already has are used (no dependency on the city kit's newer rows).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Teto do Mundo terrain (Região 7, 120–139).</summary>
        public static bool IsPamir(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 120 && i < 140;
        }

        private static void PamirStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation valley = TownVegetation.Forest;
            TownVegetation mountain = TownVegetation.Mountain;
            int[] poplars = { 0, 1 };
            int[] cedars = { 0, 2 };

            // Sangkent's valley (36): green terraces held by stone walls, apricots and poplars, cobbled lanes.
            styles[(int)OverworldTerrain.StairTerraces] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.StairTerraces, Shape = ReliefShape.Terraced, TerraceSteps = 4, Relief = 12f, ReliefScale = 220f, ReliefDetail = 0.15f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Vine, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Vine, TownSwatch.LeafGold, TownSwatch.Stone }, AltThreshold = 0.52f, AccentThreshold = 0.82f,
                Trail = TownSwatch.Cobble, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Stone, Slope = TownSwatch.GrassDark, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = valley, TreeVariants = poplars, Trees = 0.04f, Bushes = 0.03f, Rocks = 0.008f, RockTint = TownSwatch.Stone,
                Specials = 0.035f, SpecialMix = new[] { OverworldDecor.ApricotBlossom, OverworldDecor.ApricotBlossom, OverworldDecor.StoneHut },
                Tufts = 0.18f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Vine, Flowers = 0.06f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Tamarisk, TownSwatch.ClothYellow },
            };

            // A high valley of apricot orchards in blossom on low terraces.
            styles[(int)OverworldTerrain.ApricotValley] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ApricotValley, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 8f, ReliefScale = 240f, ReliefDetail = 0.1f,
                Base = TownSwatch.Vine, Alt = TownSwatch.Grass, Accent = TownSwatch.Tamarisk, Mottle = TownSwatch.LeafGold, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Tamarisk, TownSwatch.ClothWhite }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Stone, Slope = TownSwatch.Grass, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = valley, TreeVariants = poplars, Trees = 0.02f, Bushes = 0.02f, Rocks = 0.004f, RockTint = TownSwatch.Stone,
                Specials = 0.09f, Special = OverworldDecor.ApricotBlossom,
                Tufts = 0.2f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Grass, Flowers = 0.1f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Tamarisk },
            };

            // Poplar rows along the irrigation channels, green meadow, stone field walls.
            styles[(int)OverworldTerrain.PoplarCanals] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PoplarCanals, Relief = 4f, ReliefScale = 200f, ReliefDetail = 0.12f,
                Base = TownSwatch.Grass, Alt = TownSwatch.GrassDark, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.Vine, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.GrassDark, TownSwatch.LeafGold }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.GrassDark,
                Vegetation = valley, TreeVariants = poplars, Trees = 0.14f, Bushes = 0.05f, Rocks = 0.004f, TreeScale = new Vector2(1.1f, 1.6f),
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.StoneHut, OverworldDecor.ApricotBlossom },
                Tufts = 0.3f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Vine, Flowers = 0.08f, Reeds = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite, TownSwatch.Violet },
            };

            // The Pamir's high steppe: thin golden grass, dwarf shrubs, stone cushions, wide and cold.
            styles[(int)OverworldTerrain.HighSteppe] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HighSteppe, Shape = ReliefShape.Hills, Relief = 9f, ReliefScale = 260f, ReliefDetail = 0.3f,
                Base = TownSwatch.Thatch, Alt = TownSwatch.SandDark, Accent = TownSwatch.Stone, Mottle = TownSwatch.LeafGold, Crest = TownSwatch.SandDark, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.SandDark, TownSwatch.Stone, TownSwatch.GoldGrass }, AltThreshold = 0.55f, AccentThreshold = 0.84f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Stone, Slope = TownSwatch.SandDark,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.003f, Bushes = 0.06f, Rocks = 0.02f, RockTint = TownSwatch.Stone,
                Specials = 0.006f, Special = OverworldDecor.Cairn,
                Tufts = 0.45f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.LeafGold, Flowers = 0.03f, Pebbles = 0.04f,
                FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothYellow },
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // A braided meltwater river on grey gravel bars, willows (tamarisks) and poplars on the banks.
            styles[(int)OverworldTerrain.MeltRiver] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MeltRiver, Relief = 3f, ReliefScale = 160f, ReliefDetail = 0.1f,
                Base = TownSwatch.Stone, Alt = TownSwatch.Grass, Accent = TownSwatch.Sand, Mottle = TownSwatch.Cobble, Hollow = TownSwatch.Stone,
                Patches = new[] { TownSwatch.Grass, TownSwatch.Sand, TownSwatch.Cobble }, AltThreshold = 0.5f, AccentThreshold = 0.84f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Cobble, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Bank = TownSwatch.Cobble, Bed = TownSwatch.StoneDark,
                Vegetation = valley, TreeVariants = new[] { 2, 0 }, Trees = 0.03f, Bushes = 0.04f, Rocks = 0.03f, RockTint = TownSwatch.Stone,
                Specials = 0.004f, Special = OverworldDecor.Cairn,
                Tufts = 0.14f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Grass, Pebbles = 0.12f, Reeds = 0.02f,
            };

            // Slopes of old twisted junipers over rock and scree.
            styles[(int)OverworldTerrain.JuniperSlope] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.JuniperSlope, Shape = ReliefShape.Hills, Relief = 14f, ReliefScale = 200f, ReliefDetail = 0.6f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Stone, Accent = TownSwatch.Thatch, Mottle = TownSwatch.Pine, Crest = TownSwatch.Stone, Hollow = TownSwatch.GrassDark,
                Patches = new[] { TownSwatch.Stone, TownSwatch.Thatch }, AltThreshold = 0.56f, AccentThreshold = 0.84f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.05f, Bushes = 0.05f, Rocks = 0.04f, RockScale = new Vector2(1f, 2.6f), RockTint = TownSwatch.Stone,
                Specials = 0.05f, Special = OverworldDecor.Juniper,
                Tufts = 0.2f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Thatch, Pebbles = 0.05f, Stumps = 0.004f, Logs = 0.003f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // Summer pastures of the yaks: short turf, stone pens, the herders' stone huts.
            styles[(int)OverworldTerrain.YakPasture] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.YakPasture, Shape = ReliefShape.Hills, Relief = 12f, ReliefScale = 230f, ReliefDetail = 0.4f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Moss, Accent = TownSwatch.Stone, Mottle = TownSwatch.GrassDark, Crest = TownSwatch.Thatch, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Thatch, TownSwatch.Stone }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.Stone, Slope = TownSwatch.GrassDark,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.006f, Bushes = 0.03f, Rocks = 0.02f, RockTint = TownSwatch.Stone,
                Specials = 0.006f, Special = OverworldDecor.Cairn,
                Tufts = 0.36f, TuftColor = TownSwatch.Grass, TuftTip = TownSwatch.Thatch, Flowers = 0.07f, Pebbles = 0.03f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Violet, TownSwatch.ClothWhite },
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The shore of a turquoise glacial lake: pale gravel, cushion plants, cairns.
            styles[(int)OverworldTerrain.TurquoiseLake] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TurquoiseLake, Relief = 5f, ReliefScale = 180f, ReliefDetail = 0.15f,
                Base = TownSwatch.Sand, Alt = TownSwatch.Stone, Accent = TownSwatch.Moss, Mottle = TownSwatch.SandDark, Hollow = TownSwatch.Turquoise,
                Patches = new[] { TownSwatch.Stone, TownSwatch.Moss, TownSwatch.Turquoise }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Stone, Slope = TownSwatch.Sand, Bank = TownSwatch.Pearl, Bed = TownSwatch.Turquoise,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.002f, Bushes = 0.02f, Rocks = 0.02f, RockTint = TownSwatch.Stone,
                Specials = 0.012f, Special = OverworldDecor.Cairn,
                Tufts = 0.12f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Thatch, Pebbles = 0.1f, Flowers = 0.02f,
                FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothWhite },
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The Calhas de Pedra (37): steep stepped slopes of grey stone, snow in the hollows, chutes and stations.
            styles[(int)OverworldTerrain.StoneChutes] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.StoneChutes, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 15f, ReliefScale = 200f, ReliefDetail = 0.5f,
                Base = TownSwatch.Stone, Alt = TownSwatch.Snow, Accent = TownSwatch.GrassDark, Mottle = TownSwatch.Slate, Crest = TownSwatch.Snow, Hollow = TownSwatch.LimeWhite,
                Patches = new[] { TownSwatch.Snow, TownSwatch.GrassDark, TownSwatch.Slate }, AltThreshold = 0.6f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Cobble, Clearing = TownSwatch.Cobble, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.01f, Bushes = 0.01f, Rocks = 0.05f, RockScale = new Vector2(1f, 2.6f), RockTint = TownSwatch.Stone,
                Specials = 0.008f, Special = OverworldDecor.Cairn,
                Tufts = 0.08f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Thatch, Pebbles = 0.06f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The snowy upper terraces above Sangkent (36): old terraces under snow, stone huts, cairns.
            styles[(int)OverworldTerrain.SnowTerraces] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SnowTerraces, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 14f, ReliefScale = 210f, ReliefDetail = 0.3f,
                Base = TownSwatch.Snow, Alt = TownSwatch.Thatch, Accent = TownSwatch.Stone, Mottle = TownSwatch.LimeWhite, Crest = TownSwatch.Snow, Hollow = TownSwatch.LimeWhite,
                Patches = new[] { TownSwatch.Thatch, TownSwatch.Stone }, AltThreshold = 0.6f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Stone, Slope = TownSwatch.Thatch, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.008f, Bushes = 0.01f, Rocks = 0.03f, RockTint = TownSwatch.Stone,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.StoneHut, OverworldDecor.Cairn, OverworldDecor.Cairn },
                Tufts = 0.08f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.03f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // Moraines: long ridges of grey gravel and boulders left by the glacier.
            styles[(int)OverworldTerrain.Moraine] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Moraine, Shape = ReliefShape.Moraines, Relief = 12f, ReliefScale = 90f, ReliefDetail = 0.4f,
                Base = TownSwatch.Stone, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Snow, Mottle = TownSwatch.Cobble, Crest = TownSwatch.Cobble, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.StoneDark, TownSwatch.Snow, TownSwatch.Moss }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Cobble, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.Turquoise,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0f, Bushes = 0.01f, Rocks = 0.08f, RockScale = new Vector2(1f, 2.4f), RockTint = TownSwatch.Stone,
                Specials = 0.012f, Special = OverworldDecor.GraniteBlock,
                Tufts = 0.05f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Thatch, Pebbles = 0.14f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The Geleira do Teto do Mundo (38): a tongue of white ice, blue in the grooves.
            styles[(int)OverworldTerrain.Glacier] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Glacier, Shape = ReliefShape.IceTongue, Relief = 16f, ReliefScale = 140f, ReliefDetail = 0.25f,
                Base = TownSwatch.Snow, Alt = TownSwatch.LimeWhite, Accent = TownSwatch.Glass, Mottle = TownSwatch.Pearl, Crest = TownSwatch.Snow, Hollow = TownSwatch.Glass,
                Patches = new[] { TownSwatch.LimeWhite, TownSwatch.Glass }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.LimeWhite, Cliff = TownSwatch.Glass, Slope = TownSwatch.LimeWhite, Bank = TownSwatch.Glass, Bed = TownSwatch.Sea,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0f, Bushes = 0f, Rocks = 0.01f, RockTint = TownSwatch.StoneDark,
                Specials = 0.008f, Special = OverworldDecor.Serac,
                Tufts = 0f, Pebbles = 0.01f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // A field of seracs: towers and blocks of blue ice.
            styles[(int)OverworldTerrain.Seracs] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Seracs, Shape = ReliefShape.SeracField, Relief = 30f, ReliefScale = 120f, ReliefDetail = 0.4f,
                Base = TownSwatch.LimeWhite, Alt = TownSwatch.Snow, Accent = TownSwatch.Glass, Mottle = TownSwatch.Pearl, Crest = TownSwatch.Snow, Hollow = TownSwatch.Glass,
                Patches = new[] { TownSwatch.Snow, TownSwatch.Glass, TownSwatch.Sea }, AltThreshold = 0.5f, AccentThreshold = 0.84f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Snow, Cliff = TownSwatch.Glass, Slope = TownSwatch.Pearl, Bank = TownSwatch.Glass, Bed = TownSwatch.Sea,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0f, Bushes = 0f, Rocks = 0.005f, RockTint = TownSwatch.StoneDark,
                Specials = 0.05f, Special = OverworldDecor.Serac,
                Tufts = 0f, Pebbles = 0.005f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // Windswept ridges: bare rock and snow cornices, the wooden wind wheels.
            styles[(int)OverworldTerrain.WindRidge] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WindRidge, Shape = ReliefShape.Peaks, Relief = 28f, ReliefScale = 240f, ReliefDetail = 0.7f,
                Base = TownSwatch.Stone, Alt = TownSwatch.Snow, Accent = TownSwatch.Thatch, Mottle = TownSwatch.Slate, Crest = TownSwatch.Snow, Hollow = TownSwatch.Thatch,
                Patches = new[] { TownSwatch.Snow, TownSwatch.Thatch, TownSwatch.Slate }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Stone, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.004f, Bushes = 0.01f, Rocks = 0.05f, RockScale = new Vector2(1.2f, 3f), RockTint = TownSwatch.Stone,
                Specials = 0.01f, Special = OverworldDecor.RockSpire,
                Tufts = 0.08f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.05f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The Forte dos Ventos (39): a crag of grey rock cut by the gorge, timber and stone towers.
            styles[(int)OverworldTerrain.FortRock] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FortRock, Shape = ReliefShape.Gorges, Relief = 20f, ReliefScale = 220f, ReliefDetail = 0.6f,
                Base = TownSwatch.StoneDark, Alt = TownSwatch.Stone, Accent = TownSwatch.LimeWhite, Mottle = TownSwatch.Granite, Crest = TownSwatch.Thatch, Hollow = TownSwatch.Basalt,
                Patches = new[] { TownSwatch.Stone, TownSwatch.Thatch, TownSwatch.Granite }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Cobble, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Basalt, Slope = TownSwatch.StoneDark, Bank = TownSwatch.Stone, Bed = TownSwatch.Basalt,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0.01f, Bushes = 0.02f, Rocks = 0.05f, RockScale = new Vector2(1.2f, 3.2f), RockTint = TownSwatch.StoneDark,
                Specials = 0.01f, Special = OverworldDecor.RockSpire,
                Tufts = 0.1f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.05f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The upper Wakhan valley: gravel flats along the river, thin grass, the caravans' ribbons and cairns.
            styles[(int)OverworldTerrain.WakhanValley] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WakhanValley, Shape = ReliefShape.Hills, Relief = 8f, ReliefScale = 230f, ReliefDetail = 0.3f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Thatch, Accent = TownSwatch.GrassDark, Mottle = TownSwatch.Stone, Crest = TownSwatch.Snow, Hollow = TownSwatch.Grass,
                Patches = new[] { TownSwatch.Thatch, TownSwatch.GrassDark, TownSwatch.Stone }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Stone, Slope = TownSwatch.SandDark, Bank = TownSwatch.Cobble, Bed = TownSwatch.StoneDark,
                Vegetation = valley, TreeVariants = new[] { 2 }, Trees = 0.006f, Bushes = 0.03f, Rocks = 0.03f, RockTint = TownSwatch.Stone,
                Specials = 0.01f, Special = OverworldDecor.Cairn,
                Tufts = 0.26f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.LeafGold, Pebbles = 0.08f, Flowers = 0.02f,
                FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothYellow },
                WaterVegetation = valley, WaterVariants = new[] { 2, 1 },
            };

            // The Passo de Wakhan (40): the high pass under snow, stepped, cairns and ribbons along the road.
            styles[(int)OverworldTerrain.WakhanPass] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WakhanPass, Shape = ReliefShape.Terraced, TerraceSteps = 2, Relief = 14f, ReliefScale = 220f, ReliefDetail = 0.5f,
                Base = TownSwatch.Snow, Alt = TownSwatch.Stone, Accent = TownSwatch.SandDark, Mottle = TownSwatch.LimeWhite, Crest = TownSwatch.Snow, Hollow = TownSwatch.Stone,
                Patches = new[] { TownSwatch.Stone, TownSwatch.SandDark }, AltThreshold = 0.58f, AccentThreshold = 0.86f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Stone, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0f, Bushes = 0.004f, Rocks = 0.04f, RockScale = new Vector2(1.2f, 3f), RockTint = TownSwatch.Stone,
                Specials = 0.014f, Special = OverworldDecor.Cairn,
                Tufts = 0.04f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.05f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The dragon's crags (40): black rock, old snow, Aydahar's shed scales.
            styles[(int)OverworldTerrain.DragonCrags] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DragonCrags, Shape = ReliefShape.Peaks, Relief = 28f, ReliefScale = 200f, ReliefDetail = 0.8f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Snow, Mottle = TownSwatch.Slate, Crest = TownSwatch.Snow, Hollow = TownSwatch.Basalt,
                Patches = new[] { TownSwatch.StoneDark, TownSwatch.Snow, TownSwatch.Verdigris }, AltThreshold = 0.52f, AccentThreshold = 0.86f,
                Trail = TownSwatch.StoneDark, Clearing = TownSwatch.Basalt, Cliff = TownSwatch.Basalt, Slope = TownSwatch.StoneDark, Bank = TownSwatch.Basalt, Bed = TownSwatch.Black,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0f, Bushes = 0f, Rocks = 0.06f, RockScale = new Vector2(1.2f, 3.4f), RockTint = TownSwatch.Basalt,
                Specials = 0.012f, Special = OverworldDecor.RockSpire,
                Tufts = 0f, Pebbles = 0.06f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };

            // The ice peaks at the roof of the world.
            styles[(int)OverworldTerrain.IcePeaks] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.IcePeaks, Shape = ReliefShape.Peaks, Relief = 46f, ReliefScale = 260f, ReliefDetail = 1f,
                Base = TownSwatch.Snow, Alt = TownSwatch.LimeWhite, Accent = TownSwatch.StoneDark, Mottle = TownSwatch.Pearl, Crest = TownSwatch.Snow, Hollow = TownSwatch.Glass,
                Patches = new[] { TownSwatch.StoneDark, TownSwatch.Glass }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.LimeWhite, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.LimeWhite, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = mountain, TreeVariants = cedars, Trees = 0f, Bushes = 0f, Rocks = 0.04f, RockScale = new Vector2(1.4f, 3.6f), RockTint = TownSwatch.StoneDark,
                Specials = 0.01f, Special = OverworldDecor.Serac,
                Tufts = 0f, Pebbles = 0.02f,
                WaterVegetation = mountain, WaterVariants = cedars,
            };
        }
    }
}
