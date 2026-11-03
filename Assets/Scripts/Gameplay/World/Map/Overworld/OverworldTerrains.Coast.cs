using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 5 — Costa das Monções: the look of the coast terrains (80–99, Docs/Mapas100.md §5 região 5): white coral sand
    /// <c>#EFE7DA</c>, the sea <c>#2E9BB0</c> and the reef turquoise <c>#2FB3B3</c> (the walkable shallows of the reef flat,
    /// the shoals and the atoll are painted turquoise; the deep water that blocks is the dark water of the carved channels,
    /// so walkable and not walkable read apart), laterite <c>#B4583A</c>, mangrove leaf and root, golden savanna grass
    /// <c>#D9B44A</c> and Meroë's orange sand. Trees are the town kit's coast flora (leaning coconut palms, baobabs,
    /// casuarinas, sea-grape shrubs); the clove rows borrow the orchard tree; each area has its own pieces
    /// (OverworldDecorKit.Coast.cs, and Meroë the desert's pyramids, lotus columns and ushabtis).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A coast terrain (Região 5, 80–99).</summary>
        public static bool IsCoast(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 80 && i < 100;
        }

        /// <summary>The coast terrains whose ground stands in or by the sea (no reeds nor tugai along their water).</summary>
        public static bool IsSeaShore(OverworldTerrain terrain)
        {
            switch (terrain)
            {
                case OverworldTerrain.WhiteBeach:
                case OverworldTerrain.Reef:
                case OverworldTerrain.Shoals:
                case OverworldTerrain.Atoll:
                case OverworldTerrain.KelpFlats:
                case OverworldTerrain.SunkenCity:
                case OverworldTerrain.SeaCliffs:
                case OverworldTerrain.CoastalDunes:
                    return true;
                default:
                    return false;
            }
        }

        private static void CoastStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation coast = TownVegetation.Coast;
            int[] palms = { 0, 2 };

            // White coral-sand beaches: palms leaning to the sea, the dhows, nets and fish traps (OverworldChunkBuilder.Coast.cs).
            styles[(int)OverworldTerrain.WhiteBeach] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.WhiteBeach, Relief = 1.4f, ReliefScale = 180f, ReliefDetail = 0.08f,
                Base = TownSwatch.CoralLime, Alt = TownSwatch.Sand, Accent = TownSwatch.Pearl, Mottle = TownSwatch.Plaster, Crest = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Sand, TownSwatch.Pearl, TownSwatch.Plaster }, AltThreshold = 0.6f, AccentThreshold = 0.84f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Sand, Cliff = TownSwatch.SandDark, Slope = TownSwatch.Sand,
                Bank = TownSwatch.Pearl, Bed = TownSwatch.ReefTeal,
                Vegetation = coast, TreeVariants = palms, Trees = 0.03f, Bushes = 0.03f, Rocks = 0.003f, RockTint = TownSwatch.CoralDim,
                TreeScale = new Vector2(1f, 1.4f), WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.004f, SpecialMix = new[] { OverworldDecor.NetRack, OverworldDecor.CoralHead },
                Tufts = 0.05f, TuftColor = TownSwatch.Mangrove, TuftTip = TownSwatch.Sand, Pebbles = 0.03f,
                Flowers = 0.01f, FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Tamarisk },
            };

            // The reef flat: turquoise shallows walked at low tide, coral heads and tide pools; nothing grows.
            styles[(int)OverworldTerrain.Reef] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Reef, Shape = ReliefShape.Islets, Relief = 0.9f, ReliefScale = 70f, ReliefDetail = 0.05f,
                Base = TownSwatch.ReefTeal, Alt = TownSwatch.CoralLime, Accent = TownSwatch.CoralLive, Mottle = TownSwatch.Sea, Crest = TownSwatch.CoralLime,
                Hollow = TownSwatch.Sea, Patches = new[] { TownSwatch.CoralLime, TownSwatch.CoralLive, TownSwatch.Sea }, AltThreshold = 0.58f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Pearl, Clearing = TownSwatch.CoralLime, Cliff = TownSwatch.CoralDim, Slope = TownSwatch.ReefTeal,
                Bank = TownSwatch.CoralLime, Bed = TownSwatch.Sea,
                Vegetation = coast, TreeVariants = palms, Trees = 0f, Bushes = 0f, Rocks = 0.012f, RockTint = TownSwatch.CoralDim,
                Specials = 0.035f, SpecialMix = new[] { OverworldDecor.CoralHead, OverworldDecor.CoralHead, OverworldDecor.FishTrap },
                Tufts = 0.03f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.ReefTeal, Pebbles = 0.06f, Puddles = 0.03f,
            };

            // Mangroves: grey mud, mangroves along every channel, crab holes and puddles.
            styles[(int)OverworldTerrain.Mangrove] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Mangrove, Relief = 0.5f, ReliefScale = 90f, ReliefDetail = 0.08f,
                Base = TownSwatch.Mud, Alt = TownSwatch.MangroveRoot, Accent = TownSwatch.Kelp, Mottle = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.MangroveRoot, TownSwatch.Water, TownSwatch.Kelp }, AltThreshold = 0.5f, AccentThreshold = 0.8f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.MangroveRoot, Slope = TownSwatch.Mud,
                Bank = TownSwatch.Mud, Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = palms, Trees = 0.004f, Bushes = 0.05f, Rocks = 0.002f, RockTint = TownSwatch.MangroveRoot,
                Specials = 0.08f, SpecialMix = new[] { OverworldDecor.MangroveClump, OverworldDecor.MangroveClump, OverworldDecor.MangroveClump, OverworldDecor.DeadTree },
                Stumps = 0.006f, Tufts = 0.1f, TuftColor = TownSwatch.Mangrove, TuftTip = TownSwatch.Vine, Reeds = 0.05f, Puddles = 0.05f,
            };

            // Coastal dunes held by casuarinas and sea-grape.
            styles[(int)OverworldTerrain.CoastalDunes] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CoastalDunes, Shape = ReliefShape.Dunes, Relief = 6f, ReliefScale = 80f, ReliefDetail = 0.06f,
                Base = TownSwatch.CoralLime, Alt = TownSwatch.Sand, Accent = TownSwatch.Mangrove, Mottle = TownSwatch.Plaster, Crest = TownSwatch.Plaster,
                Hollow = TownSwatch.Sand, Patches = new[] { TownSwatch.Sand, TownSwatch.Mangrove }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Sand, Cliff = TownSwatch.SandDark, Slope = TownSwatch.Sand, Bank = TownSwatch.Pearl,
                Vegetation = coast, TreeVariants = new[] { 3, 3, 3, 0 }, Trees = 0.07f, Bushes = 0.09f, Rocks = 0.002f, RockTint = TownSwatch.CoralDim,
                TreeScale = new Vector2(1.1f, 1.7f), WaterVegetation = coast, WaterVariants = new[] { 3, 0 },
                Tufts = 0.16f, TuftColor = TownSwatch.Mangrove, TuftTip = TownSwatch.Sand, Pebbles = 0.01f,
                Flowers = 0.03f, FlowerColors = new[] { TownSwatch.Violet, TownSwatch.ClothWhite },
            };

            // Coral-stone ruins (Gedi, the palace of Husuni Kubwa): walls, arches, pillar tombs among baobabs.
            styles[(int)OverworldTerrain.CoralRuins] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CoralRuins, Shape = ReliefShape.Mounds, Relief = 4f, ReliefScale = 130f, ReliefDetail = 0.2f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.CoralLime, Accent = TownSwatch.Paving, Mottle = TownSwatch.Moss, Crest = TownSwatch.CoralLime,
                Patches = new[] { TownSwatch.CoralLime, TownSwatch.Paving, TownSwatch.Moss }, AltThreshold = 0.56f, AccentThreshold = 0.8f,
                Trail = TownSwatch.CoralLime, Clearing = TownSwatch.Paving, Cliff = TownSwatch.CoralDim, Slope = TownSwatch.GrassDark,
                Vegetation = coast, TreeVariants = new[] { 1, 0, 3 }, Trees = 0.03f, Bushes = 0.07f, Rocks = 0.02f, RockTint = TownSwatch.CoralLime,
                TreeScale = new Vector2(1f, 1.5f), WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.035f, SpecialMix = new[] { OverworldDecor.CoralArch, OverworldDecor.PillarTomb, OverworldDecor.Column, OverworldDecor.CoralHead },
                Tufts = 0.2f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Mangrove, Ferns = 0.03f, Pebbles = 0.05f,
                Flowers = 0.03f, FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothWhite },
            };

            // Clove plantations: rows of small evergreen trees (the orchard tree), cloves drying on mats in the lanes.
            styles[(int)OverworldTerrain.CloveRows] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CloveRows, Relief = 3f, ReliefScale = 200f, ReliefDetail = 0.15f,
                Base = TownSwatch.Laterite, Alt = TownSwatch.GrassDark, Accent = TownSwatch.Mangrove, Mottle = TownSwatch.RedClay,
                Patches = new[] { TownSwatch.GrassDark, TownSwatch.RedClay }, AltThreshold = 0.5f, AccentThreshold = 0.82f,
                Trail = TownSwatch.RedClay, Clearing = TownSwatch.RedClay, Cliff = TownSwatch.RedClay, Slope = TownSwatch.Laterite,
                Vegetation = TownVegetation.Forest, TreeVariants = new[] { 3 }, Trees = 0.72f, Bushes = 0.03f, Rocks = 0.002f, Rows = true,
                TreeScale = new Vector2(0.95f, 1.25f), BushVegetation = coast, WaterVegetation = coast, WaterVariants = palms,
                Tufts = 0.14f, TuftColor = TownSwatch.GrassDark, TuftTip = TownSwatch.Mangrove, Flowers = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.Fruit },
            };

            // A coconut plantation: palms in rows over grass.
            styles[(int)OverworldTerrain.CoconutGrove] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CoconutGrove, Relief = 2f, ReliefScale = 200f, ReliefDetail = 0.1f,
                Base = TownSwatch.Grass, Alt = TownSwatch.Sand, Accent = TownSwatch.Mangrove, Mottle = TownSwatch.Vine,
                Patches = new[] { TownSwatch.Sand, TownSwatch.Vine }, AltThreshold = 0.56f, AccentThreshold = 0.84f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Sand, Cliff = TownSwatch.SandDark, Slope = TownSwatch.Grass,
                Vegetation = coast, TreeVariants = palms, Trees = 0.48f, Bushes = 0.03f, Rocks = 0.002f, Rows = true,
                TreeScale = new Vector2(1.05f, 1.45f), WaterVegetation = coast, WaterVariants = palms,
                Tufts = 0.24f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Grass, Flowers = 0.03f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothWhite },
            };

            // Sea cliffs: a laterite headland in steps, sea stacks, crested hawks on the bluffs.
            styles[(int)OverworldTerrain.SeaCliffs] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SeaCliffs, Shape = ReliefShape.Cliffs, Relief = 18f, ReliefScale = 230f, ReliefDetail = 0.4f,
                Base = TownSwatch.GrassDark, Alt = TownSwatch.Laterite, Accent = TownSwatch.LeafGold, Mottle = TownSwatch.Mangrove, Crest = TownSwatch.LeafGold,
                Hollow = TownSwatch.Sand, Patches = new[] { TownSwatch.Laterite, TownSwatch.LeafGold }, AltThreshold = 0.55f, AccentThreshold = 0.82f,
                Trail = TownSwatch.RedClay, Clearing = TownSwatch.Laterite, Cliff = TownSwatch.Laterite, Slope = TownSwatch.RedClay,
                Vegetation = coast, TreeVariants = new[] { 1, 3 }, Trees = 0.02f, Bushes = 0.07f, Rocks = 0.04f, RockTint = TownSwatch.Laterite,
                RockScale = new Vector2(1f, 2.6f), WaterVegetation = coast, WaterVariants = palms,
                Tufts = 0.26f, TuftColor = TownSwatch.LeafGold, TuftTip = TownSwatch.GrassDark, Pebbles = 0.04f,
                Flowers = 0.04f, FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Violet },
            };

            // Islets linked by shallows: sand humps over turquoise shallows, deep channels between them.
            styles[(int)OverworldTerrain.Shoals] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Shoals, Shape = ReliefShape.Islets, Relief = 1.4f, ReliefScale = 90f, ReliefDetail = 0.05f,
                Base = TownSwatch.Pearl, Alt = TownSwatch.ReefTeal, Accent = TownSwatch.Sand, Mottle = TownSwatch.CoralLime, Crest = TownSwatch.CoralLime,
                Hollow = TownSwatch.ReefTeal, Patches = new[] { TownSwatch.ReefTeal, TownSwatch.Sand }, AltThreshold = 0.56f, AccentThreshold = 0.84f,
                Trail = TownSwatch.Sand, Clearing = TownSwatch.CoralLime, Cliff = TownSwatch.Sand, Slope = TownSwatch.Pearl, Bank = TownSwatch.CoralLime,
                Bed = TownSwatch.Sea,
                Vegetation = coast, TreeVariants = palms, Trees = 0.02f, Bushes = 0.04f, Rocks = 0.006f, RockTint = TownSwatch.CoralDim,
                WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.CoralHead, OverworldDecor.FishTrap, OverworldDecor.NetRack },
                Tufts = 0.06f, TuftColor = TownSwatch.Mangrove, TuftTip = TownSwatch.Sand, Pebbles = 0.05f,
            };

            // The Savana dos Auroques (26): golden grass, flat-topped acacias, baobabs, termite mounds.
            styles[(int)OverworldTerrain.Savanna] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Savanna, Relief = 4f, ReliefScale = 260f, ReliefDetail = 0.3f,
                Base = TownSwatch.GoldGrass, Alt = TownSwatch.LeafGold, Accent = TownSwatch.Laterite, Mottle = TownSwatch.Thatch, Crest = TownSwatch.Thatch,
                Hollow = TownSwatch.Grass, Patches = new[] { TownSwatch.Thatch, TownSwatch.Laterite, TownSwatch.Grass }, AltThreshold = 0.52f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Laterite, Clearing = TownSwatch.RedClay, Cliff = TownSwatch.RedClay, Slope = TownSwatch.LeafGold,
                Vegetation = coast, TreeVariants = new[] { 1 }, Trees = 0.006f, Bushes = 0.03f, Rocks = 0.004f, RockTint = TownSwatch.Laterite,
                TreeScale = new Vector2(1.2f, 1.8f), WaterVegetation = coast, WaterVariants = new[] { 1, 0 },
                Specials = 0.03f, SpecialMix = new[] { OverworldDecor.Acacia, OverworldDecor.Acacia, OverworldDecor.Acacia, OverworldDecor.TermiteMound },
                Tufts = 0.46f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.GoldGrass, Flowers = 0.03f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.ClothRed },
            };

            // The Pirâmides Altas de Meroé (27): orange sand, the desert's steep pyramids, lotus columns, ushabti rows.
            styles[(int)OverworldTerrain.KushPyramids] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.KushPyramids, Shape = ReliefShape.Mounds, Relief = 5f, ReliefScale = 150f, ReliefDetail = 0.2f,
                Base = TownSwatch.AdobeOchre, Alt = TownSwatch.Sand, Accent = TownSwatch.Gold, Mottle = TownSwatch.Sandstone, Crest = TownSwatch.Sand,
                Hollow = TownSwatch.Violet, Patches = new[] { TownSwatch.Sand, TownSwatch.Gold, TownSwatch.Violet }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Paving, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.AdobeOchre,
                Vegetation = coast, TreeVariants = new[] { 1 }, Trees = 0.002f, Bushes = 0.02f, Rocks = 0.015f, RockTint = TownSwatch.Sandstone,
                Specials = 0.035f, SpecialMix = new[] { OverworldDecor.Pyramid, OverworldDecor.Pyramid, OverworldDecor.LotusColumn, OverworldDecor.Ushabtis, OverworldDecor.Acacia },
                Tufts = 0.08f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.AdobeOchre, Pebbles = 0.05f,
            };

            // Kirta's atoll (28): a ring of coral sand round the lagoon, turquoise shallows, palms.
            styles[(int)OverworldTerrain.Atoll] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Atoll, Shape = ReliefShape.Islets, Relief = 1.2f, ReliefScale = 80f, ReliefDetail = 0.05f,
                Base = TownSwatch.CoralLime, Alt = TownSwatch.ReefTeal, Accent = TownSwatch.CoralLive, Mottle = TownSwatch.Pearl, Crest = TownSwatch.Pearl,
                Hollow = TownSwatch.ReefTeal, Patches = new[] { TownSwatch.ReefTeal, TownSwatch.CoralLive }, AltThreshold = 0.62f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Pearl, Clearing = TownSwatch.CoralLime, Cliff = TownSwatch.CoralDim, Slope = TownSwatch.CoralLime, Bank = TownSwatch.Pearl,
                Bed = TownSwatch.Sea,
                Vegetation = coast, TreeVariants = palms, Trees = 0.05f, Bushes = 0.04f, Rocks = 0.004f, RockTint = TownSwatch.CoralLive,
                TreeScale = new Vector2(1f, 1.5f), WaterVegetation = coast, WaterVariants = palms,
                Specials = 0.02f, SpecialMix = new[] { OverworldDecor.CoralHead, OverworldDecor.CoralHead, OverworldDecor.NetRack },
                Tufts = 0.05f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.Mangrove, Pebbles = 0.05f,
            };

            // Kilwa Afundada (29): coral streets half drowned, flooded houses, arches and walls.
            styles[(int)OverworldTerrain.SunkenCity] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SunkenCity, Shape = ReliefShape.Mounds, Relief = 2.4f, ReliefScale = 110f, ReliefDetail = 0.12f,
                Base = TownSwatch.Paving, Alt = TownSwatch.ReefTeal, Accent = TownSwatch.Kelp, Mottle = TownSwatch.CoralLime, Crest = TownSwatch.CoralLime,
                Hollow = TownSwatch.ReefTeal, Patches = new[] { TownSwatch.CoralLime, TownSwatch.Kelp, TownSwatch.ReefTeal }, AltThreshold = 0.5f, AccentThreshold = 0.82f,
                Trail = TownSwatch.CoralLime, Clearing = TownSwatch.PavingDark, Cliff = TownSwatch.CoralDim, Slope = TownSwatch.Paving, Bank = TownSwatch.CoralLime,
                Bed = TownSwatch.Sea,
                Vegetation = coast, TreeVariants = palms, Trees = 0.008f, Bushes = 0.03f, Rocks = 0.01f, RockTint = TownSwatch.CoralDim,
                Specials = 0.05f, SpecialMix = new[] { OverworldDecor.CoralArch, OverworldDecor.Column, OverworldDecor.CoralHead, OverworldDecor.PillarTomb },
                Tufts = 0.07f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.ReefTeal, Pebbles = 0.06f, Puddles = 0.04f,
            };

            // The restinga: a thicket on low hills behind the beaches.
            styles[(int)OverworldTerrain.CoastScrub] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CoastScrub, Shape = ReliefShape.Hills, Relief = 6f, ReliefScale = 170f, ReliefDetail = 0.4f,
                Base = TownSwatch.Mangrove, Alt = TownSwatch.Sand, Accent = TownSwatch.Laterite, Mottle = TownSwatch.GrassDark, Crest = TownSwatch.Sand,
                Hollow = TownSwatch.GrassDark, Patches = new[] { TownSwatch.Sand, TownSwatch.Laterite }, AltThreshold = 0.58f, AccentThreshold = 0.84f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Sand, Cliff = TownSwatch.Laterite, Slope = TownSwatch.GrassDark,
                Vegetation = coast, TreeVariants = new[] { 0, 3, 1, 2 }, Trees = 0.06f, Bushes = 0.22f, Rocks = 0.006f, RockTint = TownSwatch.Laterite,
                WaterVegetation = coast, WaterVariants = palms,
                Logs = 0.004f, Tufts = 0.22f, TuftColor = TownSwatch.Mangrove, TuftTip = TownSwatch.Vine, Ferns = 0.04f,
                Flowers = 0.06f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothYellow, TownSwatch.ClothWhite },
            };

            // Tidal lagoons: mud flats and pools, a mangrove fringe, wading herons' ground.
            styles[(int)OverworldTerrain.TideLagoons] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TideLagoons, Relief = 0.6f, ReliefScale = 110f, ReliefDetail = 0.06f,
                Base = TownSwatch.Sand, Alt = TownSwatch.Mud, Accent = TownSwatch.ReefTeal, Mottle = TownSwatch.SandDark,
                Patches = new[] { TownSwatch.Mud, TownSwatch.ReefTeal }, AltThreshold = 0.52f, AccentThreshold = 0.8f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Mud, Slope = TownSwatch.Sand, Bank = TownSwatch.Mud,
                Bed = TownSwatch.Mud,
                Vegetation = coast, TreeVariants = palms, Trees = 0.01f, Bushes = 0.05f, Rocks = 0.003f, RockTint = TownSwatch.CoralDim,
                Specials = 0.025f, SpecialMix = new[] { OverworldDecor.MangroveClump, OverworldDecor.FishTrap, OverworldDecor.MangroveClump },
                Tufts = 0.12f, TuftColor = TownSwatch.Mangrove, TuftTip = TownSwatch.Sand, Reeds = 0.04f, Puddles = 0.06f, Pebbles = 0.02f,
            };

            // Baobab woods on red laterite.
            styles[(int)OverworldTerrain.BaobabWoods] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BaobabWoods, Shape = ReliefShape.Hills, Relief = 7f, ReliefScale = 210f, ReliefDetail = 0.4f,
                Base = TownSwatch.Laterite, Alt = TownSwatch.LeafGold, Accent = TownSwatch.GrassDark, Mottle = TownSwatch.RedClay, Crest = TownSwatch.LeafGold,
                Hollow = TownSwatch.GrassDark, Patches = new[] { TownSwatch.LeafGold, TownSwatch.RedClay }, AltThreshold = 0.54f, AccentThreshold = 0.84f,
                Trail = TownSwatch.RedClay, Clearing = TownSwatch.RedClay, Cliff = TownSwatch.RedClay, Slope = TownSwatch.Laterite,
                Vegetation = coast, TreeVariants = new[] { 1, 1, 3 }, Trees = 0.035f, Bushes = 0.06f, Rocks = 0.01f, RockTint = TownSwatch.Laterite,
                TreeScale = new Vector2(1.2f, 1.9f), WaterVegetation = coast, WaterVariants = new[] { 1, 0 },
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.TermiteMound, OverworldDecor.Acacia },
                Tufts = 0.32f, TuftColor = TownSwatch.LeafGold, TuftTip = TownSwatch.Thatch, Flowers = 0.03f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothRed, TownSwatch.ClothYellow },
            };

            // Kelp flats north of Kirta: green shallows, kelp drifts, coral heads.
            styles[(int)OverworldTerrain.KelpFlats] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.KelpFlats, Shape = ReliefShape.Islets, Relief = 1f, ReliefScale = 75f, ReliefDetail = 0.05f,
                Base = TownSwatch.ReefTeal, Alt = TownSwatch.Kelp, Accent = TownSwatch.CoralLive, Mottle = TownSwatch.Sea, Crest = TownSwatch.Sand,
                Hollow = TownSwatch.Kelp, Patches = new[] { TownSwatch.Kelp, TownSwatch.Sand }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Sand, Clearing = TownSwatch.Sand, Cliff = TownSwatch.CoralDim, Slope = TownSwatch.ReefTeal, Bank = TownSwatch.Sand,
                Bed = TownSwatch.Sea,
                Vegetation = coast, TreeVariants = palms, Trees = 0f, Bushes = 0.01f, Rocks = 0.008f, RockTint = TownSwatch.CoralLive,
                Specials = 0.02f, SpecialMix = new[] { OverworldDecor.CoralHead },
                Tufts = 0.2f, TuftColor = TownSwatch.Kelp, TuftTip = TownSwatch.Vine, Pebbles = 0.04f, Puddles = 0.02f,
            };
        }
    }
}
