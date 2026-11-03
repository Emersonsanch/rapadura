using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 6 — Delta de Jhilpur: the look of the delta terrains (100–119, Docs/Mapas100.md §5 região 6, maps 31–35): murky
    /// green-brown water's edge, root bark and moss, lotus pink, the delta's grey mud, golden reed and raffia green, and in the
    /// Coração do Lodo black mud with violet bubbles and a few springs of Living Water (teal). Trees are the town kit's palms
    /// (the delta's nipa and raffia palms) and casuarinas, plus the delta's own pieces (OverworldDecorKit.Delta.cs): banyans
    /// with curtains of aerial roots, root arches in the gorges, fishers' stilt huts, lotus lanterns, paddy dikes, rotten roots.
    /// Only the shared palette rows every build already has are used (no dependency on the city kit's newer rows).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A delta terrain (Região 6, 100–119).</summary>
        public static bool IsDelta(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 100 && i < 120;
        }

        private static void DeltaStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation coast = TownVegetation.Coast;
            int[] palms = { 0, 2 };

            // Jhilpur's lake (31): moss and root-brown ground round the murky water, banyans, lotus at the edge.
            styles[(int)OverworldTerrain.RootLake] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.RootLake, Relief = 1.2f, ReliefScale = 150f, ReliefDetail = 0.08f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Moss, Accent = TownSwatch.Tamarisk, Mottle = TownSwatch.Kelp, Hollow = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Mud, TownSwatch.Tamarisk }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Brick, Clearing = TownSwatch.Mud, Cliff = TownSwatch.MangroveRoot, Slope = TownSwatch.GrassDark,
                Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = palms, Trees = 0.03f, Bushes = 0.06f, Rocks = 0.003f, RockTint = TownSwatch.Brick,
                WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.02f, SpecialMix = new[] { OverworldDecor.Banyan, OverworldDecor.LanternPole },
                Tufts = 0.2f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.Vine, Reeds = 0.04f, Puddles = 0.03f,
                Flowers = 0.06f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothWhite },
            };

            // Banyan grove: deep shade, moss and ferns, the banyans' root curtains.
            styles[(int)OverworldTerrain.BanyanGrove] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BanyanGrove, Shape = ReliefShape.Hills, Relief = 4f, ReliefScale = 170f, ReliefDetail = 0.3f,
                Base = TownSwatch.LeafDark, Alt = TownSwatch.Moss, Accent = TownSwatch.MangroveRoot, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Moss, TownSwatch.MangroveRoot }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.MangroveRoot, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.MangroveRoot, Slope = TownSwatch.LeafDark,
                Vegetation = coast, TreeVariants = palms, Trees = 0.03f, Bushes = 0.12f, Rocks = 0.006f, RockTint = TownSwatch.StoneDark,
                WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.06f, SpecialMix = new[] { OverworldDecor.Banyan, OverworldDecor.Banyan, OverworldDecor.RootArch },
                Logs = 0.006f, MossRocks = 0.01f, Ferns = 0.26f, Tufts = 0.08f, TuftColor = TownSwatch.Pine, TuftTip = TownSwatch.Moss,
            };

            // Lotus lagoons: flat murky shallows, pads and pink flowers, reeds.
            styles[(int)OverworldTerrain.LotusMarsh] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LotusMarsh, Relief = 0.6f, ReliefScale = 110f, ReliefDetail = 0.05f,
                Base = TownSwatch.Kelp, Alt = TownSwatch.Water, Accent = TownSwatch.Tamarisk, Mottle = TownSwatch.Moss, Hollow = TownSwatch.Water,
                Patches = new[] { TownSwatch.Water, TownSwatch.Tamarisk, TownSwatch.Moss }, AltThreshold = 0.62f, AccentThreshold = 0.9f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.Mud, Cliff = TownSwatch.Mud, Slope = TownSwatch.Kelp, Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = palms, Trees = 0.008f, Bushes = 0.03f, Rocks = 0.002f, RockTint = TownSwatch.StoneDark,
                WaterVegetation = coast, WaterVariants = palms,
                Tufts = 0.12f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.Vine, Reeds = 0.12f, Puddles = 0.06f,
                Flowers = 0.06f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothWhite },
            };

            // The Pontes de Raiz Viva (32): rain-forest gorges, wet stone, moss; root arches and banyans on the rims.
            styles[(int)OverworldTerrain.RootGorge] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.RootGorge, Shape = ReliefShape.Gorges, Relief = 16f, ReliefScale = 210f, ReliefDetail = 0.5f,
                Base = TownSwatch.Moss, Alt = TownSwatch.LeafDark, Accent = TownSwatch.Stone, Mottle = TownSwatch.Pine, Crest = TownSwatch.Vine, Hollow = TownSwatch.StoneDark,
                Patches = new[] { TownSwatch.Stone, TownSwatch.Pine }, AltThreshold = 0.52f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Moss, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Moss,
                Bank = TownSwatch.StoneDark, Bed = TownSwatch.Basalt,
                Vegetation = coast, TreeVariants = palms, Trees = 0.04f, Bushes = 0.1f, Rocks = 0.03f, RockTint = TownSwatch.StoneDark,
                RockScale = new Vector2(1f, 2.4f), WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.04f, SpecialMix = new[] { OverworldDecor.RootArch, OverworldDecor.Banyan },
                MossRocks = 0.03f, Logs = 0.004f, Ferns = 0.3f, Tufts = 0.08f, TuftColor = TownSwatch.Pine, TuftTip = TownSwatch.Vine,
            };

            // Rain forest on rolling ground: palms, broadleaf shade, ferns, mossy boulders.
            styles[(int)OverworldTerrain.RainForest] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.RainForest, Shape = ReliefShape.Hills, Relief = 7f, ReliefScale = 190f, ReliefDetail = 0.5f,
                Base = TownSwatch.Pine, Alt = TownSwatch.LeafDark, Accent = TownSwatch.Moss, Mottle = TownSwatch.GrassDark, Hollow = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Moss, TownSwatch.GrassDark }, AltThreshold = 0.5f, AccentThreshold = 0.84f,
                Trail = TownSwatch.DirtDark, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.LeafDark,
                Vegetation = coast, TreeVariants = palms, Trees = 0.16f, Bushes = 0.16f, Rocks = 0.008f, RockTint = TownSwatch.StoneDark,
                TreeScale = new Vector2(1.2f, 1.8f), WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.015f, Special = OverworldDecor.Banyan,
                MossRocks = 0.02f, Logs = 0.01f, Stumps = 0.004f, Ferns = 0.3f, Tufts = 0.06f, TuftColor = TownSwatch.Pine, TuftTip = TownSwatch.Vine,
                Flowers = 0.02f, FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.Tamarisk },
            };

            // Flooded paddies between earthen dikes (the bunds are their own runs, the rice is ground detail).
            styles[(int)OverworldTerrain.PaddyLevees] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PaddyLevees, Shape = ReliefShape.Levees, Relief = 1.6f, ReliefScale = 90f, ReliefDetail = 0.03f,
                Base = TownSwatch.Vine, Alt = TownSwatch.Water, Accent = TownSwatch.Mud, Mottle = TownSwatch.Grass, Crest = TownSwatch.Mud, Hollow = TownSwatch.Water,
                Patches = new[] { TownSwatch.Water, TownSwatch.Mud, TownSwatch.Grass }, AltThreshold = 0.6f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Mud, Clearing = TownSwatch.Mud, Cliff = TownSwatch.Mud, Slope = TownSwatch.Vine, Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = palms, Trees = 0.01f, Bushes = 0.01f, Rocks = 0.001f,
                WaterVegetation = coast, WaterVariants = palms,
                Tufts = 0.06f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Grass, Puddles = 0.03f,
            };

            // The delta's grey mud banks: low, bare, puddles, a few reeds and palms.
            styles[(int)OverworldTerrain.MudFlats] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MudFlats, Relief = 0.8f, ReliefScale = 120f, ReliefDetail = 0.05f,
                Base = TownSwatch.Mud, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Kelp, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.Water,
                Patches = new[] { TownSwatch.Kelp, TownSwatch.Water }, AltThreshold = 0.6f, AccentThreshold = 0.84f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Mud, Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = palms, Trees = 0.006f, Bushes = 0.02f, Rocks = 0.002f, RockTint = TownSwatch.StoneDark,
                WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.004f, Special = OverworldDecor.DeadTree,
                Tufts = 0.06f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.Vine, Reeds = 0.04f, Puddles = 0.08f, Pebbles = 0.02f,
            };

            // Open swamp of reed islands (33): golden reeds on the hummocks, fishers' huts, lilac mist.
            styles[(int)OverworldTerrain.ReedIslands] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ReedIslands, Shape = ReliefShape.Hummocks, Relief = 1.6f, ReliefScale = 70f, ReliefDetail = 0.05f,
                Base = TownSwatch.Reed, Alt = TownSwatch.Kelp, Accent = TownSwatch.Violet, Mottle = TownSwatch.ReedMat, Crest = TownSwatch.Reed, Hollow = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Kelp, TownSwatch.ReedMat, TownSwatch.Violet }, AltThreshold = 0.58f, AccentThreshold = 0.92f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.ReedMat, Cliff = TownSwatch.Mud, Slope = TownSwatch.Kelp, Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = new[] { 3 }, Trees = 0.006f, Bushes = 0.02f, Rocks = 0.001f,
                WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.012f, Special = OverworldDecor.StiltHut,
                Tufts = 0.14f, TuftColor = TownSwatch.Reed, TuftTip = TownSwatch.ReedMat, Reeds = 0.17f, Puddles = 0.03f,
            };

            // The Charco do Ninki Nanka (34): tall reeds and raffia palms, hollow logs, dark water.
            styles[(int)OverworldTerrain.RaffiaBog] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.RaffiaBog, Shape = ReliefShape.Hummocks, Relief = 2.2f, ReliefScale = 85f, ReliefDetail = 0.08f,
                Base = TownSwatch.Kelp, Alt = TownSwatch.Reed, Accent = TownSwatch.StoneDark, Mottle = TownSwatch.LeafDark, Crest = TownSwatch.Reed, Hollow = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Reed, TownSwatch.LeafDark }, AltThreshold = 0.54f, AccentThreshold = 0.88f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.Mud, Cliff = TownSwatch.Mud, Slope = TownSwatch.Kelp, Bank = TownSwatch.Mud, Bed = TownSwatch.Basalt,
                Vegetation = coast, TreeVariants = palms, Trees = 0.045f, Bushes = 0.05f, Rocks = 0.002f, RockTint = TownSwatch.StoneDark,
                TreeScale = new Vector2(1.1f, 1.6f), WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.LanternPole, OverworldDecor.DeadTree },
                Logs = 0.012f, Tufts = 0.12f, TuftColor = TownSwatch.Reed, TuftTip = TownSwatch.Kelp, Reeds = 0.17f, Puddles = 0.03f,
            };

            // The Coração do Lodo (35): black mud, rotten roots, violet bubbles, springs of Living Water.
            styles[(int)OverworldTerrain.BlackMire] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BlackMire, Shape = ReliefShape.Hummocks, Relief = 1.4f, ReliefScale = 95f, ReliefDetail = 0.06f,
                Base = TownSwatch.Black, Alt = TownSwatch.Basalt, Accent = TownSwatch.Violet, Mottle = TownSwatch.MangroveRoot, Crest = TownSwatch.MangroveRoot,
                Hollow = TownSwatch.Black, Patches = new[] { TownSwatch.Violet, TownSwatch.MangroveRoot, TownSwatch.ReefTeal }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.Basalt, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Black, Bank = TownSwatch.Basalt, Bed = TownSwatch.Black,
                Vegetation = coast, TreeVariants = new[] { 3 }, Trees = 0.002f, Bushes = 0.004f, Rocks = 0.004f, RockTint = TownSwatch.Basalt,
                WaterVegetation = coast, WaterVariants = new[] { 3 },
                Specials = 0.035f, SpecialMix = new[] { OverworldDecor.RottenRoot, OverworldDecor.RottenRoot, OverworldDecor.DeadTree },
                Tufts = 0.03f, TuftColor = TownSwatch.MangroveRoot, TuftTip = TownSwatch.Violet, Puddles = 0.05f, Pebbles = 0.02f,
            };

            // A drowned dead wood: grey snags and rotten roots over dark moss.
            styles[(int)OverworldTerrain.DeadWood] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DeadWood, Shape = ReliefShape.Mounds, Relief = 3f, ReliefScale = 140f, ReliefDetail = 0.2f,
                Base = TownSwatch.DirtDark, Alt = TownSwatch.Moss, Accent = TownSwatch.Stone, Mottle = TownSwatch.MangroveRoot, Hollow = TownSwatch.Black,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Stone }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.MangroveRoot, Slope = TownSwatch.DirtDark,
                Bank = TownSwatch.Mud, Bed = TownSwatch.Black,
                Vegetation = coast, TreeVariants = new[] { 3 }, Trees = 0.004f, Bushes = 0.03f, Rocks = 0.006f, RockTint = TownSwatch.StoneDark,
                Specials = 0.06f, SpecialMix = new[] { OverworldDecor.DeadTree, OverworldDecor.DeadTree, OverworldDecor.RottenRoot },
                Logs = 0.02f, Stumps = 0.02f, Tufts = 0.06f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Stone, Ferns = 0.04f, Puddles = 0.03f,
            };

            // The tidal forest of the delta's mouths: palms, stilt-rooted trees along every creek.
            styles[(int)OverworldTerrain.TidalForest] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TidalForest, Relief = 1.2f, ReliefScale = 120f, ReliefDetail = 0.08f,
                Base = TownSwatch.Mangrove, Alt = TownSwatch.Mud, Accent = TownSwatch.MangroveRoot, Mottle = TownSwatch.LeafDark, Hollow = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Mud, TownSwatch.MangroveRoot }, AltThreshold = 0.55f, AccentThreshold = 0.84f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.Mud, Cliff = TownSwatch.MangroveRoot, Slope = TownSwatch.Mangrove, Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = palms, Trees = 0.08f, Bushes = 0.06f, Rocks = 0.001f, WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.05f, SpecialMix = new[] { OverworldDecor.MangroveClump, OverworldDecor.MangroveClump, OverworldDecor.Banyan },
                Stumps = 0.004f, Tufts = 0.1f, TuftColor = TownSwatch.Mangrove, TuftTip = TownSwatch.Vine, Reeds = 0.04f, Puddles = 0.05f,
            };
        }
    }
}
