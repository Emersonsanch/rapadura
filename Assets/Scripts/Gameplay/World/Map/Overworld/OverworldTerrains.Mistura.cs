using Rapadura.Gameplay.World.Cities.Town;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 14 — Margens da Mistura: the look of its terrains (260–279, Docs/Mapas100.md §5 região 14, maps 77–82): baked brick <c>#B07A4A</c>,
    /// glazed blue <c>#2F5FA8</c>, bitumen <c>#2A2420</c>, marsh reed, the gold of Elam and the dust of the sky <c>#E8C690</c> round Dur-Jarras; the
    /// pale flat Desert of the Clock with violet shadows; the black pitch wells among reeds; the green cedars (<c>#3E5A3A</c>) and gold resin of
    /// Hobabish; the giant's mossy stone shoulder and dark diorite; the grey tablets and still water of the dream field. The relief stays low
    /// round the city and in the desert (≤ 4 m, dial ridges 6 m), low in the pitch, higher in the cedars (~8 m) and on the shoulder (~14 m), with the
    /// diorite cone standing up (~22 m).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Margens da Mistura terrain (Região 14, 260–279).</summary>
        public static bool IsMistura(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 260 && i < 280;
        }

        private static void MisturaStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation desert = TownVegetation.Desert;
            TownVegetation hills = TownVegetation.Mountain;
            int[] palms = { 0 };
            int[] cedars = { 0 };
            int[] noTrees = { 2 };

            // The terrace round Dur-Jarras (77): packed earth and brick paving, palms and glazed lions.
            styles[(int)OverworldTerrain.ZigguratTerrace] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ZigguratTerrace, Relief = 2.5f, ReliefScale = 240f, ReliefDetail = 0.05f,
                Base = TownSwatch.AdobeOchre, Alt = TownSwatch.BakedBrick, Accent = TownSwatch.GlazedBlue, Mottle = TownSwatch.Dirt, Hollow = TownSwatch.Bitumen, Crest = TownSwatch.DustSky,
                Patches = new[] { TownSwatch.BakedBrick, TownSwatch.DustSky, TownSwatch.Dirt }, AltThreshold = 0.56f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Brick, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Bitumen, Slope = TownSwatch.AdobeOchre, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = desert, TreeVariants = palms, Trees = 0f, Bushes = 0.004f, Rocks = 0.004f, RockTint = TownSwatch.BakedBrick,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.GlazedLionStatue, OverworldDecor.DatePalm, OverworldDecor.BrickStack },
                Tufts = 0.08f, TuftColor = TownSwatch.PalmFrond, TuftTip = TownSwatch.MarshReed, Pebbles = 0.03f,
                WaterVegetation = desert, WaterVariants = palms,
            };

            // The brick gate (77): dust and old brick at the way in from the river.
            styles[(int)OverworldTerrain.BrickGate] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BrickGate, Shape = ReliefShape.Rolling, Relief = 3f, ReliefScale = 220f, ReliefDetail = 0.08f,
                Base = TownSwatch.Dirt, Alt = TownSwatch.DustSky, Accent = TownSwatch.BakedBrick, Mottle = TownSwatch.AdobeOchre, Hollow = TownSwatch.DirtDark, Crest = TownSwatch.DustSky,
                Patches = new[] { TownSwatch.DustSky, TownSwatch.BakedBrick, TownSwatch.DirtDark }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Brick, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Dirt, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = desert, TreeVariants = palms, Trees = 0f, Bushes = 0.006f, Rocks = 0.008f, RockTint = TownSwatch.BakedBrick,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.BrickStack, OverworldDecor.GlazedLionStatue },
                Tufts = 0.1f, TuftColor = TownSwatch.MarshReed, TuftTip = TownSwatch.DustSky, Pebbles = 0.04f,
                WaterVegetation = desert, WaterVariants = palms,
            };

            // The brick kilns (77): clay pits, kilns and drying stacks.
            styles[(int)OverworldTerrain.BrickKilns] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BrickKilns, Shape = ReliefShape.Hummocks, Relief = 3f, ReliefScale = 180f, ReliefDetail = 0.1f,
                Base = TownSwatch.RedClay, Alt = TownSwatch.Dirt, Accent = TownSwatch.Ember, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.Bitumen, Crest = TownSwatch.BakedBrick,
                Patches = new[] { TownSwatch.Dirt, TownSwatch.BakedBrick, TownSwatch.Bitumen }, AltThreshold = 0.5f, AccentThreshold = 0.94f,
                Trail = TownSwatch.AdobeOchre, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.Bitumen, Slope = TownSwatch.RedClay, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = desert, TreeVariants = palms, Trees = 0f, Bushes = 0.002f, Rocks = 0.012f, RockTint = TownSwatch.RedClay,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.BrickKiln, OverworldDecor.BrickStack },
                Tufts = 0.04f, TuftColor = TownSwatch.MarshReed, TuftTip = TownSwatch.RedClay, Pebbles = 0.08f,
                WaterVegetation = desert, WaterVariants = palms,
            };

            // The tells (77): round mounds of old cities, potsherds and brick.
            styles[(int)OverworldTerrain.TellMounds] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TellMounds, Shape = ReliefShape.Mounds, Relief = 4.5f, ReliefScale = 170f, ReliefDetail = 0.1f,
                Base = TownSwatch.DustSky, Alt = TownSwatch.AdobeOchre, Accent = TownSwatch.ElamGold, Mottle = TownSwatch.Dirt, Hollow = TownSwatch.Dirt, Crest = TownSwatch.BakedBrick,
                Patches = new[] { TownSwatch.AdobeOchre, TownSwatch.BakedBrick, TownSwatch.Dirt }, AltThreshold = 0.5f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Brick, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.AdobeOchre, Slope = TownSwatch.DustSky, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = desert, TreeVariants = palms, Trees = 0f, Bushes = 0.004f, Rocks = 0.01f, RockTint = TownSwatch.AdobeOchre,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.BrickStack, OverworldDecor.BitumenJar, OverworldDecor.Stump },
                Tufts = 0.08f, TuftColor = TownSwatch.MarshReed, TuftTip = TownSwatch.ElamGold, Pebbles = 0.06f,
                WaterVegetation = desert, WaterVariants = palms,
            };

            // The palm canal (77): palms and reed along the small canals.
            styles[(int)OverworldTerrain.PalmCanal] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PalmCanal, Shape = ReliefShape.Islets, Relief = 2.5f, ReliefScale = 200f, ReliefDetail = 0.06f,
                Base = TownSwatch.PalmFrond, Alt = TownSwatch.MarshReed, Accent = TownSwatch.ElamGold, Mottle = TownSwatch.Moss, Hollow = TownSwatch.MurkWater, Crest = TownSwatch.PalmFrond,
                Patches = new[] { TownSwatch.MarshReed, TownSwatch.Moss, TownSwatch.Dirt }, AltThreshold = 0.52f, AccentThreshold = 0.94f,
                Trail = TownSwatch.Brick, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.Mud, Slope = TownSwatch.PalmFrond, Bank = TownSwatch.Mud, Bed = TownSwatch.MurkWater,
                Vegetation = desert, TreeVariants = palms, Trees = 0f, Bushes = 0.008f, Rocks = 0.004f, RockTint = TownSwatch.BakedBrick,
                Specials = 0.018f, SpecialMix = new[] { OverworldDecor.DatePalm, OverworldDecor.DatePalm, OverworldDecor.BitumenJar },
                Tufts = 0.2f, TuftColor = TownSwatch.PalmFrond, TuftTip = TownSwatch.MarshReed, Reeds = 0.18f, Pebbles = 0.01f, Puddles = 0.03f,
                WaterVegetation = desert, WaterVariants = palms,
            };

            // The barley terraces (77): stepped fields of barley.
            styles[(int)OverworldTerrain.BarleyTerraces] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BarleyTerraces, Shape = ReliefShape.Terraced, TerraceSteps = 3, Relief = 3.5f, ReliefScale = 210f, ReliefDetail = 0.05f,
                Base = TownSwatch.GoldGrass, Alt = TownSwatch.DustSky, Accent = TownSwatch.ElamGold, Mottle = TownSwatch.Straw, Hollow = TownSwatch.Dirt, Crest = TownSwatch.DustSky,
                Patches = new[] { TownSwatch.Straw, TownSwatch.DustSky, TownSwatch.PalmFrond }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Brick, Clearing = TownSwatch.Straw, Cliff = TownSwatch.Dirt, Slope = TownSwatch.GoldGrass, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = desert, TreeVariants = palms, Trees = 0f, Bushes = 0.002f, Rocks = 0.002f, RockTint = TownSwatch.AdobeOchre,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.WheatSheaf, OverworldDecor.WheatSheaf, OverworldDecor.GlazedLionStatue },
                Tufts = 0.28f, TuftColor = TownSwatch.GoldGrass, TuftTip = TownSwatch.ElamGold, Pebbles = 0f,
                WaterVegetation = desert, WaterVariants = palms,
            };

            // The Desert of the Clock (78): pale flat sand, fallen gnomons and violet shadows.
            styles[(int)OverworldTerrain.ClockDesert] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ClockDesert, Shape = ReliefShape.GnomonDial, Relief = 6f, ReliefScale = 220f, ReliefDetail = 0.05f,
                Base = TownSwatch.Sand, Alt = TownSwatch.DustSky, Accent = TownSwatch.Violet, Mottle = TownSwatch.SandDark, Hollow = TownSwatch.Violet, Crest = TownSwatch.DustSky,
                Patches = new[] { TownSwatch.DustSky, TownSwatch.Violet, TownSwatch.SandDark }, AltThreshold = 0.55f, AccentThreshold = 0.96f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Violet, Slope = TownSwatch.Sand, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0f, Rocks = 0.004f, RockTint = TownSwatch.Sandstone,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.FallenGnomon, OverworldDecor.HourglassPillar },
                Tufts = 0.02f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.DustSky, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The late shadows (78): dark drifts where the shadows arrive after the beast.
            styles[(int)OverworldTerrain.ShadowSands] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ShadowSands, Shape = ReliefShape.Dunes, Relief = 5f, ReliefScale = 190f, ReliefDetail = 0.08f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Violet, Accent = TownSwatch.Indigo, Mottle = TownSwatch.Sand, Hollow = TownSwatch.Indigo, Crest = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Violet, TownSwatch.Indigo, TownSwatch.Sand }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.Violet, Cliff = TownSwatch.Indigo, Slope = TownSwatch.SandDark, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0f, Rocks = 0.006f, RockTint = TownSwatch.Indigo,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.HourglassPillar, OverworldDecor.StoneDoorway },
                Tufts = 0.02f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Violet, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The sands of the hours (78): pale ripples and hourglass pillars.
            styles[(int)OverworldTerrain.HourSands] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HourSands, Shape = ReliefShape.Dunes, Relief = 4f, ReliefScale = 240f, ReliefDetail = 0.05f,
                Base = TownSwatch.DustSky, Alt = TownSwatch.Sand, Accent = TownSwatch.ElamGold, Mottle = TownSwatch.SandDark, Hollow = TownSwatch.SandDark, Crest = TownSwatch.DustSky,
                Patches = new[] { TownSwatch.Sand, TownSwatch.ElamGold, TownSwatch.Violet }, AltThreshold = 0.5f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.DustSky, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0f, Rocks = 0.006f, RockTint = TownSwatch.Sandstone,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.HourglassPillar, OverworldDecor.FallenGnomon, OverworldDecor.ScarabIdol },
                Tufts = 0.02f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.DustSky, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The tar pools (79): a net of black pitch on dark ground.
            styles[(int)OverworldTerrain.TarPools] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TarPools, Shape = ReliefShape.Hummocks, Relief = 2f, ReliefScale = 170f, ReliefDetail = 0.06f,
                Base = TownSwatch.Bitumen, Alt = TownSwatch.Mud, Accent = TownSwatch.MarshReed, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.Black, Crest = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Mud, TownSwatch.DirtDark, TownSwatch.Black }, AltThreshold = 0.5f, AccentThreshold = 0.93f,
                Trail = TownSwatch.MarshReed, Clearing = TownSwatch.Mud, Cliff = TownSwatch.Black, Slope = TownSwatch.Bitumen, Bank = TownSwatch.Bitumen, Bed = TownSwatch.Black,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0.002f, Rocks = 0.004f, RockTint = TownSwatch.Bitumen,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.BitumenJar, OverworldDecor.PapyrusClump },
                Tufts = 0.06f, TuftColor = TownSwatch.MarshReed, TuftTip = TownSwatch.Reed, Reeds = 0.12f, Pebbles = 0.02f, Puddles = 0.05f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The Bitumen Wells (79): wells, jars and rafts, the Balsa de Junco.
            styles[(int)OverworldTerrain.BitumenWells] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BitumenWells, Shape = ReliefShape.Levees, Relief = 2.5f, ReliefScale = 190f, ReliefDetail = 0.05f,
                Base = TownSwatch.Mud, Alt = TownSwatch.Bitumen, Accent = TownSwatch.ElamGold, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.Black, Crest = TownSwatch.MarshReed,
                Patches = new[] { TownSwatch.Bitumen, TownSwatch.MarshReed, TownSwatch.DirtDark }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.ReedMat, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.Black, Slope = TownSwatch.Mud, Bank = TownSwatch.Bitumen, Bed = TownSwatch.Black,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0.002f, Rocks = 0.004f, RockTint = TownSwatch.Mud,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.BitumenJar, OverworldDecor.StrandedHull },
                Tufts = 0.08f, TuftColor = TownSwatch.MarshReed, TuftTip = TownSwatch.ElamGold, Reeds = 0.1f, Pebbles = 0.02f, Puddles = 0.04f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The tar reeds (79): tall reeds growing out of the pitch.
            styles[(int)OverworldTerrain.TarReeds] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TarReeds, Shape = ReliefShape.Rolling, Relief = 2.5f, ReliefScale = 210f, ReliefDetail = 0.06f,
                Base = TownSwatch.MarshReed, Alt = TownSwatch.Bitumen, Accent = TownSwatch.Reed, Mottle = TownSwatch.Mud, Hollow = TownSwatch.Black, Crest = TownSwatch.Reed,
                Patches = new[] { TownSwatch.Reed, TownSwatch.Bitumen, TownSwatch.Mud }, AltThreshold = 0.54f, AccentThreshold = 0.9f,
                Trail = TownSwatch.ReedMat, Clearing = TownSwatch.Bitumen, Cliff = TownSwatch.Black, Slope = TownSwatch.MarshReed, Bank = TownSwatch.Bitumen, Bed = TownSwatch.Black,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0.004f, Rocks = 0.002f, RockTint = TownSwatch.Bitumen,
                Specials = 0.018f, SpecialMix = new[] { OverworldDecor.PapyrusClump, OverworldDecor.PapyrusClump, OverworldDecor.BitumenJar },
                Tufts = 0.3f, TuftColor = TownSwatch.MarshReed, TuftTip = TownSwatch.Reed, Reeds = 0.24f, Pebbles = 0.01f, Puddles = 0.04f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The edge of the cedars (80): the first giant cedars and stumps.
            styles[(int)OverworldTerrain.CedarEdge] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CedarEdge, Shape = ReliefShape.Hills, Relief = 6f, ReliefScale = 180f, ReliefDetail = 0.1f,
                Base = TownSwatch.Pine, Alt = TownSwatch.Moss, Accent = TownSwatch.ElamGold, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.DirtDark, Crest = TownSwatch.Moss,
                Patches = new[] { TownSwatch.Moss, TownSwatch.DirtDark, TownSwatch.ElamGold }, AltThreshold = 0.5f, AccentThreshold = 0.95f,
                Trail = TownSwatch.DirtDark, Clearing = TownSwatch.Moss, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Pine, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.02f, Bushes = 0.04f, Rocks = 0.012f, RockTint = TownSwatch.StoneDark,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.GiantCedar, OverworldDecor.Stump, OverworldDecor.BrickStack },
                Tufts = 0.14f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Pine, Ferns = 0.1f, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The rings of Hobabish (80): rings of giant cedars, roots in the ground.
            styles[(int)OverworldTerrain.CedarRings] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CedarRings, Shape = ReliefShape.CedarRings, Relief = 8f, ReliefScale = 200f, ReliefDetail = 0.08f,
                Base = TownSwatch.Pine, Alt = TownSwatch.Moss, Accent = TownSwatch.ElamGold, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.DirtDark, Crest = TownSwatch.ElamGold,
                Patches = new[] { TownSwatch.ElamGold, TownSwatch.Moss, TownSwatch.DirtDark }, AltThreshold = 0.52f, AccentThreshold = 0.94f,
                Trail = TownSwatch.ElamGold, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Pine, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.03f, Bushes = 0.03f, Rocks = 0.006f, RockTint = TownSwatch.StoneDark,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.GiantCedar, OverworldDecor.ResinLantern },
                Tufts = 0.1f, TuftColor = TownSwatch.Pine, TuftTip = TownSwatch.ElamGold, Ferns = 0.14f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The elder cedars (80): the oldest trees, resin glowing.
            styles[(int)OverworldTerrain.ElderCedars] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ElderCedars, Shape = ReliefShape.Hills, Relief = 7f, ReliefScale = 210f, ReliefDetail = 0.08f,
                Base = TownSwatch.Moss, Alt = TownSwatch.Pine, Accent = TownSwatch.ZigguratFlame, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.DirtDark, Crest = TownSwatch.ElamGold,
                Patches = new[] { TownSwatch.Pine, TownSwatch.DirtDark, TownSwatch.ElamGold }, AltThreshold = 0.48f, AccentThreshold = 0.96f,
                Trail = TownSwatch.ElamGold, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Moss, Bank = TownSwatch.Mud, Bed = TownSwatch.Bitumen,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.035f, Bushes = 0.03f, Rocks = 0.008f, RockTint = TownSwatch.StoneDark,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.GiantCedar, OverworldDecor.GiantCedar, OverworldDecor.ResinLantern, OverworldDecor.CutTrunk },
                Tufts = 0.12f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.ElamGold, Ferns = 0.16f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The shoulder of Upelluri (81): the giant's shoulder, moss on stone.
            styles[(int)OverworldTerrain.ShoulderStone] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ShoulderStone, Shape = ReliefShape.GiantShoulder, Relief = 14f, ReliefScale = 230f, ReliefDetail = 0.1f,
                Base = TownSwatch.Stone, Alt = TownSwatch.Moss, Accent = TownSwatch.Lichen, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.StoneDark, Crest = TownSwatch.Lichen,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Lichen, TownSwatch.StoneDark }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Cobble, Clearing = TownSwatch.Moss, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Stone, Bank = TownSwatch.Mud, Bed = TownSwatch.Stone,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0.02f, Rocks = 0.02f, RockTint = TownSwatch.Stone,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.DioriteShard, OverworldDecor.Basalt },
                Tufts = 0.12f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Lichen, Ferns = 0.04f, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The folds of stone skin (81): sharp parallel creases, dry grit.
            styles[(int)OverworldTerrain.SkinFolds] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SkinFolds, Shape = ReliefShape.SkinFolds, Relief = 10f, ReliefScale = 210f, ReliefDetail = 0.08f,
                Base = TownSwatch.Granite, Alt = TownSwatch.Stone, Accent = TownSwatch.Lichen, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.Basalt, Crest = TownSwatch.Lichen,
                Patches = new[] { TownSwatch.Stone, TownSwatch.StoneDark, TownSwatch.Lichen }, AltThreshold = 0.5f, AccentThreshold = 0.94f,
                Trail = TownSwatch.Cobble, Clearing = TownSwatch.Stone, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Granite, Bank = TownSwatch.Mud, Bed = TownSwatch.Stone,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0.006f, Rocks = 0.024f, RockTint = TownSwatch.Granite,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.DioriteShard, OverworldDecor.Stump },
                Tufts = 0.04f, TuftColor = TownSwatch.Lichen, TuftTip = TownSwatch.Stone, Pebbles = 0.08f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The growing diorite (81): black rock pushing up through the shoulder.
            styles[(int)OverworldTerrain.DioriteRise] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DioriteRise, Shape = ReliefShape.DioriteRise, Relief = 22f, ReliefScale = 230f, ReliefDetail = 0.14f,
                Base = TownSwatch.Basalt, Alt = TownSwatch.StoneDark, Accent = TownSwatch.Granite, Mottle = TownSwatch.Black, Hollow = TownSwatch.Black, Crest = TownSwatch.Granite,
                Patches = new[] { TownSwatch.StoneDark, TownSwatch.Black, TownSwatch.Granite }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.Black, Slope = TownSwatch.Basalt, Bank = TownSwatch.Black, Bed = TownSwatch.Black,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0f, Rocks = 0.03f, RockTint = TownSwatch.StoneDark,
                Specials = 0.016f, SpecialMix = new[] { OverworldDecor.DioriteShard },
                Tufts = 0f, Pebbles = 0.1f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The Field of the Dream Tablets (82): standing tablets on grey ground, cold water.
            styles[(int)OverworldTerrain.DreamTablets] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.DreamTablets, Shape = ReliefShape.Terraced, TerraceSteps = 2, Relief = 3f, ReliefScale = 230f, ReliefDetail = 0.06f,
                Base = TownSwatch.CoralDim, Alt = TownSwatch.Stone, Accent = TownSwatch.MoonLilac, Mottle = TownSwatch.StoneDark, Hollow = TownSwatch.ColdWater, Crest = TownSwatch.LimeWhite,
                Patches = new[] { TownSwatch.Stone, TownSwatch.MoonLilac, TownSwatch.ColdWater }, AltThreshold = 0.5f, AccentThreshold = 0.94f,
                Trail = TownSwatch.Curb, Clearing = TownSwatch.StoneDark, Cliff = TownSwatch.ColdWater, Slope = TownSwatch.CoralDim, Bank = TownSwatch.Mud, Bed = TownSwatch.ColdWater,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0.002f, Rocks = 0.006f, RockTint = TownSwatch.CoralDim,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.TabletStone, OverworldDecor.TabletStone, OverworldDecor.CutTrunk },
                Tufts = 0.06f, TuftColor = TownSwatch.Lichen, TuftTip = TownSwatch.MoonLilac, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };

            // The cut garden (82): felled trunks and still water, the grass gone grey.
            styles[(int)OverworldTerrain.CutGarden] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CutGarden, Shape = ReliefShape.Rolling, Relief = 3f, ReliefScale = 240f, ReliefDetail = 0.06f,
                Base = TownSwatch.Lichen, Alt = TownSwatch.CoralDim, Accent = TownSwatch.MoonLilac, Mottle = TownSwatch.Moss, Hollow = TownSwatch.ColdWater, Crest = TownSwatch.CoralDim,
                Patches = new[] { TownSwatch.CoralDim, TownSwatch.Moss, TownSwatch.ColdWater }, AltThreshold = 0.5f, AccentThreshold = 0.93f,
                Trail = TownSwatch.Curb, Clearing = TownSwatch.Moss, Cliff = TownSwatch.ColdWater, Slope = TownSwatch.Lichen, Bank = TownSwatch.Mud, Bed = TownSwatch.ColdWater,
                Vegetation = hills, TreeVariants = noTrees, Trees = 0f, Bushes = 0.004f, Rocks = 0.004f, RockTint = TownSwatch.CoralDim,
                Specials = 0.016f, SpecialMix = new[] { OverworldDecor.CutTrunk, OverworldDecor.CutTrunk, OverworldDecor.TabletStone },
                Tufts = 0.14f, TuftColor = TownSwatch.Lichen, TuftTip = TownSwatch.CoralDim, Reeds = 0.06f, Pebbles = 0.03f, Puddles = 0.04f,
                WaterVegetation = hills, WaterVariants = noTrees,
            };
        }
    }
}
