using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 13 — Rio das Doze Horas: the look of the river's terrains (240–259, Docs/Mapas100.md §5 região 13, maps 71–76): night
    /// <c>#1E2050</c>, gold <c>#D4AF37</c>, papyrus <c>#8DAA4A</c>, sandstone <c>#C9A27A</c> and dark water <c>#22324A</c> on the quay and its
    /// banks; the reed fields of golden wheat (<c>#D8B85A</c>) under the stars; the dark hidden sands (<c>#8A7350</c>) with zigzag ridges and
    /// closed stone doors; Mehen's green scales (<c>#5E8F5A</c>) in rings; the Lake of Fire (<c>#FF9A3D</c>) with palm islets; Apep's dry
    /// sandbank and purple scales (<c>#5A3A6E</c>). The relief stays low on the quay, the banks and the fields (≤ 4 m), rolls on the sands
    /// (~8 m) and stands up in Mehen's rings (~12 m) and Apep's body (~14 m).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Rio das Doze Horas terrain (Região 13, 240–259).</summary>
        public static bool IsRio(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 240 && i < 260;
        }

        private static void RioStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation hills = TownVegetation.Mountain;
            int[] pines = { 2 };

            // The quay (71): dark sandstone slabs and packed night-sand round the rafts, papyrus by the water.
            styles[(int)OverworldTerrain.NightQuay] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.NightQuay, Relief = 2.5f, ReliefScale = 240f, ReliefDetail = 0.06f,
                Base = TownSwatch.QuaySandstone, Alt = TownSwatch.Sandstone, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.SandDark, Hollow = TownSwatch.RiverNight,
                Patches = new[] { TownSwatch.SandDark, TownSwatch.RiverNight, TownSwatch.Slate }, AltThreshold = 0.55f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.RiverNight, Slope = TownSwatch.QuaySandstone, Bank = TownSwatch.Mud, Bed = TownSwatch.NightWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.004f, Rocks = 0.006f, RockTint = TownSwatch.Sandstone,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.PapyrusClump, OverworldDecor.NightObelisk },
                Tufts = 0.05f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.KemetGold, Reeds = 0.08f, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The papyrus bank (71): mud and tall papyrus, the way in from the ash.
            styles[(int)OverworldTerrain.PapyrusBank] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PapyrusBank, Shape = ReliefShape.Rolling, Relief = 3.5f, ReliefScale = 200f, ReliefDetail = 0.08f,
                Base = TownSwatch.Mud, Alt = TownSwatch.PapyrusGreen, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.NightWater, Crest = TownSwatch.PapyrusGreen,
                Patches = new[] { TownSwatch.PapyrusGreen, TownSwatch.DirtDark, TownSwatch.Moss }, AltThreshold = 0.5f, AccentThreshold = 0.94f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.Mud, Slope = TownSwatch.Mud, Bank = TownSwatch.Mud, Bed = TownSwatch.NightWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.01f, Rocks = 0.004f, RockTint = TownSwatch.Mud,
                Specials = 0.016f, SpecialMix = new[] { OverworldDecor.PapyrusClump, OverworldDecor.PapyrusClump, OverworldDecor.StrandedHull },
                Tufts = 0.18f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.Reed, Reeds = 0.16f, Puddles = 0.03f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The ash delta (71): the Devouring Fire's ash washed down to the black water, burnt logs.
            styles[(int)OverworldTerrain.AshDelta] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.AshDelta, Shape = ReliefShape.Hummocks, Relief = 3f, ReliefScale = 180f, ReliefDetail = 0.1f,
                Base = TownSwatch.PotIron, Alt = TownSwatch.Mud, Accent = TownSwatch.PapyrusGreen, Mottle = TownSwatch.Stone, Hollow = TownSwatch.NightWater, Crest = TownSwatch.GoodSmoke,
                Patches = new[] { TownSwatch.Mud, TownSwatch.GoodSmoke, TownSwatch.RiverNight }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.StoneDark, Clearing = TownSwatch.PotIron, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Mud, Bank = TownSwatch.Mud, Bed = TownSwatch.NightWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.002f, Rocks = 0.008f, RockTint = TownSwatch.PotIron,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.Stump, OverworldDecor.PapyrusClump },
                Tufts = 0.05f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.PapyrusGreen, Reeds = 0.06f, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The rowers' beach (71): hauled-up reed skiffs on dark sand, oars and nets.
            styles[(int)OverworldTerrain.RowersBeach] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.RowersBeach, Relief = 3f, ReliefScale = 200f, ReliefDetail = 0.08f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Sand, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Mud, Hollow = TownSwatch.NightWater, Crest = TownSwatch.Sand,
                Patches = new[] { TownSwatch.Mud, TownSwatch.Sand, TownSwatch.RiverNight }, AltThreshold = 0.55f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.RiverNight, Slope = TownSwatch.SandDark, Bank = TownSwatch.Mud, Bed = TownSwatch.NightWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.002f, Rocks = 0.006f, RockTint = TownSwatch.SandDark,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.StrandedHull, OverworldDecor.PapyrusClump },
                Tufts = 0.04f, TuftColor = TownSwatch.Reed, TuftTip = TownSwatch.KemetGold, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The piers of the hours (71): dark sandstone banks along the river, obelisks marking the hours.
            styles[(int)OverworldTerrain.HourPiers] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HourPiers, Shape = ReliefShape.Terraced, TerraceSteps = 2, Relief = 3.5f, ReliefScale = 220f, ReliefDetail = 0.06f,
                Base = TownSwatch.Sandstone, Alt = TownSwatch.QuaySandstone, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Slate, Hollow = TownSwatch.RiverNight, Crest = TownSwatch.NileLinen,
                Patches = new[] { TownSwatch.Slate, TownSwatch.RiverNight, TownSwatch.SandDark }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.NileLinen, Clearing = TownSwatch.QuaySandstone, Cliff = TownSwatch.RiverNight, Slope = TownSwatch.Sandstone, Bank = TownSwatch.Mud, Bed = TownSwatch.NightWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.004f, Rocks = 0.008f, RockTint = TownSwatch.Sandstone,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.NightObelisk, OverworldDecor.PapyrusClump, OverworldDecor.StrandedHull },
                Tufts = 0.06f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.Reed, Reeds = 0.1f, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The barque's canal (71): the wide east reach of the river, mooring stones and lotus.
            styles[(int)OverworldTerrain.BarqueCanal] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BarqueCanal, Shape = ReliefShape.Islets, Relief = 3f, ReliefScale = 190f, ReliefDetail = 0.06f,
                Base = TownSwatch.Mud, Alt = TownSwatch.Sandstone, Accent = TownSwatch.Lotus, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.NightWater, Crest = TownSwatch.PapyrusGreen,
                Patches = new[] { TownSwatch.Sandstone, TownSwatch.Lotus, TownSwatch.RiverNight }, AltThreshold = 0.52f, AccentThreshold = 0.94f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.RiverNight, Slope = TownSwatch.Mud, Bank = TownSwatch.Mud, Bed = TownSwatch.NightWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.006f, Rocks = 0.006f, RockTint = TownSwatch.Sandstone,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.PapyrusClump, OverworldDecor.NightObelisk },
                Tufts = 0.08f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.Lotus, Reeds = 0.16f, Puddles = 0.03f, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The reed beds (72): raised beds of reed in a grid, dark furrows between them.
            styles[(int)OverworldTerrain.ReedBeds] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ReedBeds, Shape = ReliefShape.ReedBedGrid, Relief = 2.2f, ReliefScale = 200f, ReliefDetail = 0.05f,
                Base = TownSwatch.PapyrusGreen, Alt = TownSwatch.Reed, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Moss, Hollow = TownSwatch.DirtDark, Crest = TownSwatch.Reed,
                Patches = new[] { TownSwatch.Reed, TownSwatch.Moss, TownSwatch.DirtDark }, AltThreshold = 0.5f, AccentThreshold = 0.94f,
                Trail = TownSwatch.Straw, Clearing = TownSwatch.Reed, Cliff = TownSwatch.Mud, Slope = TownSwatch.Mud, Bank = TownSwatch.Mud, Bed = TownSwatch.MurkWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.004f, Rocks = 0.002f, RockTint = TownSwatch.Mud,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.PapyrusClump, OverworldDecor.PapyrusClump, OverworldDecor.WheatSheaf },
                Tufts = 0.24f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.KemetGold, Reeds = 0.2f, Pebbles = 0f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The fields of reeds (72): gold wheat and reed under the stars, the calm sekhet-aaru of the tales.
            styles[(int)OverworldTerrain.ReedFields] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ReedFields, Relief = 2.5f, ReliefScale = 260f, ReliefDetail = 0.06f,
                Base = TownSwatch.GoldGrass, Alt = TownSwatch.Reed, Accent = TownSwatch.PapyrusGreen, Mottle = TownSwatch.Straw, Hollow = TownSwatch.Reed, Crest = TownSwatch.KemetGold,
                Patches = new[] { TownSwatch.PapyrusGreen, TownSwatch.Straw, TownSwatch.Reed }, AltThreshold = 0.48f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.Straw, Cliff = TownSwatch.Mud, Slope = TownSwatch.Reed, Bank = TownSwatch.Mud, Bed = TownSwatch.MurkWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.002f, Rocks = 0.002f, RockTint = TownSwatch.Sandstone,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.WheatSheaf, OverworldDecor.PapyrusClump },
                Tufts = 0.3f, TuftColor = TownSwatch.GoldGrass, TuftTip = TownSwatch.KemetGold, Reeds = 0.12f, Flowers = 0.03f, Pebbles = 0f,
                FlowerColors = new[] { TownSwatch.KemetGold, TownSwatch.StarGlow },
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The wheat of light (72): low gold wheat that glows faintly at the tips, more sheaves.
            styles[(int)OverworldTerrain.LightWheat] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LightWheat, Shape = ReliefShape.Rolling, Relief = 3f, ReliefScale = 240f, ReliefDetail = 0.06f,
                Base = TownSwatch.KemetGold, Alt = TownSwatch.GoldGrass, Accent = TownSwatch.StarGlow, Mottle = TownSwatch.Straw, Hollow = TownSwatch.Reed, Crest = TownSwatch.StarGlow,
                Patches = new[] { TownSwatch.Straw, TownSwatch.GoldGrass, TownSwatch.PapyrusGreen }, AltThreshold = 0.5f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.GoldGrass, Cliff = TownSwatch.Mud, Slope = TownSwatch.GoldGrass, Bank = TownSwatch.Mud, Bed = TownSwatch.MurkWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.002f, RockTint = TownSwatch.Sandstone,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.WheatSheaf, OverworldDecor.WheatSheaf, OverworldDecor.ScarabIdol },
                Tufts = 0.3f, TuftColor = TownSwatch.KemetGold, TuftTip = TownSwatch.StarGlow, Flowers = 0.02f, Pebbles = 0f,
                FlowerColors = new[] { TownSwatch.StarGlow, TownSwatch.KemetGold },
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The canals of Sekhet (72): low green banks, sluice stones and lotus ponds.
            styles[(int)OverworldTerrain.SekhetCanals] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SekhetCanals, Shape = ReliefShape.Terraced, TerraceSteps = 2, Relief = 3f, ReliefScale = 210f, ReliefDetail = 0.05f,
                Base = TownSwatch.PapyrusGreen, Alt = TownSwatch.Moss, Accent = TownSwatch.Lotus, Mottle = TownSwatch.DirtDark, Hollow = TownSwatch.MurkWater, Crest = TownSwatch.GoldGrass,
                Patches = new[] { TownSwatch.Moss, TownSwatch.Lotus, TownSwatch.GoldGrass }, AltThreshold = 0.52f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.Mud, Slope = TownSwatch.Moss, Bank = TownSwatch.Mud, Bed = TownSwatch.MurkWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.008f, Rocks = 0.006f, RockTint = TownSwatch.Sandstone,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.PapyrusClump, OverworldDecor.WheatSheaf, OverworldDecor.NightObelisk },
                Tufts = 0.2f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.Lotus, Reeds = 0.16f, Puddles = 0.04f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The hidden sands (73): dark sand (#8A7350) in low drifts, gold in the stone.
            styles[(int)OverworldTerrain.HiddenSands] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.HiddenSands, Shape = ReliefShape.Dunes, Relief = 6f, ReliefScale = 200f, ReliefDetail = 0.1f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Sandstone, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Mud, Hollow = TownSwatch.RiverNight, Crest = TownSwatch.QuaySandstone,
                Patches = new[] { TownSwatch.Mud, TownSwatch.QuaySandstone, TownSwatch.RiverNight }, AltThreshold = 0.55f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Slate, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.RiverNight, Slope = TownSwatch.Sandstone, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.008f, RockTint = TownSwatch.Sandstone,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.ScarabIdol, OverworldDecor.NightObelisk },
                Tufts = 0.02f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.KemetGold, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The closed gates (73): stone doors in the dark sand, paved lanes between ridges.
            styles[(int)OverworldTerrain.ClosedGates] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ClosedGates, Shape = ReliefShape.ZigzagDunes, Relief = 7f, ReliefScale = 200f, ReliefDetail = 0.08f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Slate, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Sandstone, Hollow = TownSwatch.RiverNight, Crest = TownSwatch.Sandstone,
                Patches = new[] { TownSwatch.Slate, TownSwatch.Sandstone, TownSwatch.RiverNight }, AltThreshold = 0.5f, AccentThreshold = 0.94f,
                Trail = TownSwatch.NileLinen, Clearing = TownSwatch.Slate, Cliff = TownSwatch.RiverNight, Slope = TownSwatch.SandDark, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.01f, RockTint = TownSwatch.Slate,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.NightObelisk, OverworldDecor.ScarabIdol, OverworldDecor.ScarabIdol },
                Tufts = 0f, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The zigzag of the sands (73): sharp diagonal ridges of dark sand, lanes in zigzag.
            styles[(int)OverworldTerrain.ZigzagSands] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ZigzagSands, Shape = ReliefShape.ZigzagDunes, Relief = 8f, ReliefScale = 180f, ReliefDetail = 0.1f,
                Base = TownSwatch.Sandstone, Alt = TownSwatch.SandDark, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Mud, Hollow = TownSwatch.RiverNight, Crest = TownSwatch.QuaySandstone,
                Patches = new[] { TownSwatch.SandDark, TownSwatch.QuaySandstone, TownSwatch.Mud }, AltThreshold = 0.5f, AccentThreshold = 0.95f,
                Trail = TownSwatch.Slate, Clearing = TownSwatch.Mud, Cliff = TownSwatch.RiverNight, Slope = TownSwatch.Sandstone, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.012f, RockTint = TownSwatch.Sandstone,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.ScarabIdol, OverworldDecor.NightObelisk },
                Tufts = 0f, Pebbles = 0.07f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Mehen's coils (74): rings of green scales and gold light around the Quiet Ring.
            styles[(int)OverworldTerrain.MehenCoils] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MehenCoils, Shape = ReliefShape.CoilRings, Relief = 12f, ReliefScale = 200f, ReliefDetail = 0.08f,
                Base = TownSwatch.RootGreen, Alt = TownSwatch.PapyrusGreen, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Moss, Hollow = TownSwatch.RiverNight, Crest = TownSwatch.KemetGold,
                Patches = new[] { TownSwatch.PapyrusGreen, TownSwatch.KemetGold, TownSwatch.RiverNight }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.KemetGold, Clearing = TownSwatch.RiverNight, Cliff = TownSwatch.RootGreen, Slope = TownSwatch.PapyrusGreen, Bank = TownSwatch.Mud, Bed = TownSwatch.RiverNight,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.006f, RockTint = TownSwatch.RootGreen,
                Specials = 0.014f, SpecialMix = new[] { OverworldDecor.ScarabIdol, OverworldDecor.NightObelisk },
                Tufts = 0.04f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.KemetGold, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The Wrong Knot (74): the darkest coils, tangled ridges, purple shadow.
            styles[(int)OverworldTerrain.CoilKnot] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CoilKnot, Shape = ReliefShape.CoilRings, Relief = 11f, ReliefScale = 170f, ReliefDetail = 0.14f,
                Base = TownSwatch.RiverNight, Alt = TownSwatch.RootGreen, Accent = TownSwatch.Violet, Mottle = TownSwatch.Indigo, Hollow = TownSwatch.Black, Crest = TownSwatch.PapyrusGreen,
                Patches = new[] { TownSwatch.Violet, TownSwatch.Indigo, TownSwatch.RootGreen }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Trail = TownSwatch.PapyrusGreen, Clearing = TownSwatch.Indigo, Cliff = TownSwatch.Black, Slope = TownSwatch.RootGreen, Bank = TownSwatch.Mud, Bed = TownSwatch.RiverNight,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.012f, RockTint = TownSwatch.Indigo,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.ApepScaleSlab, OverworldDecor.ApepScaleSlab, OverworldDecor.NightObelisk },
                Tufts = 0f, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The outer coil (74): the first low ring of scales on the dark sand.
            styles[(int)OverworldTerrain.OuterCoil] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.OuterCoil, Shape = ReliefShape.CoilRings, Relief = 9f, ReliefScale = 220f, ReliefDetail = 0.08f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.RootGreen, Accent = TownSwatch.KemetGold, Mottle = TownSwatch.Moss, Hollow = TownSwatch.RiverNight, Crest = TownSwatch.PapyrusGreen,
                Patches = new[] { TownSwatch.RootGreen, TownSwatch.Moss, TownSwatch.KemetGold }, AltThreshold = 0.5f, AccentThreshold = 0.93f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.RootGreen, Cliff = TownSwatch.Moss, Slope = TownSwatch.SandDark, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.002f, Rocks = 0.008f, RockTint = TownSwatch.SandDark,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.ScarabIdol, OverworldDecor.WheatSheaf },
                Tufts = 0.06f, TuftColor = TownSwatch.PapyrusGreen, TuftTip = TownSwatch.KemetGold, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The boiling shore of the Lake of Fire (75): ember sand, palms, a low red glow.
            styles[(int)OverworldTerrain.FireLakeShore] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FireLakeShore, Shape = ReliefShape.Terraced, TerraceSteps = 2, Relief = 4f, ReliefScale = 210f, ReliefDetail = 0.08f,
                Base = TownSwatch.Laterite, Alt = TownSwatch.SandDark, Accent = TownSwatch.Ember, Mottle = TownSwatch.ColdEmber, Hollow = TownSwatch.Black, Crest = TownSwatch.Sandstone,
                Patches = new[] { TownSwatch.ColdEmber, TownSwatch.Sandstone, TownSwatch.Black }, AltThreshold = 0.5f, AccentThreshold = 0.94f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.ColdEmber, Cliff = TownSwatch.Black, Slope = TownSwatch.Laterite, Bank = TownSwatch.ColdEmber, Bed = TownSwatch.ColdEmber,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.004f, Rocks = 0.008f, RockTint = TownSwatch.ColdEmber,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.DatePalm, OverworldDecor.DatePalm, OverworldDecor.NightObelisk },
                Tufts = 0.04f, TuftColor = TownSwatch.Straw, TuftTip = TownSwatch.Ember, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The Lake of Fire (75): ember rock round the lake, palms on the islets.
            styles[(int)OverworldTerrain.FireLake] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FireLake, Relief = 3f, ReliefScale = 230f, ReliefDetail = 0.08f,
                Base = TownSwatch.ColdEmber, Alt = TownSwatch.Laterite, Accent = TownSwatch.Ember, Mottle = TownSwatch.Black, Hollow = TownSwatch.Black, Crest = TownSwatch.Sandstone,
                Patches = new[] { TownSwatch.Laterite, TownSwatch.Black, TownSwatch.Ember }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Sandstone, Clearing = TownSwatch.Laterite, Cliff = TownSwatch.Black, Slope = TownSwatch.ColdEmber, Bank = TownSwatch.Black, Bed = TownSwatch.ColdEmber,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.006f, RockTint = TownSwatch.Black,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.DatePalm, OverworldDecor.DatePalm },
                Tufts = 0.02f, TuftColor = TownSwatch.Straw, TuftTip = TownSwatch.Ember, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Apep's sandbank (76): the dry bed of the river, the serpent's body in long ridges.
            styles[(int)OverworldTerrain.ApepSandbank] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ApepSandbank, Shape = ReliefShape.ApepBody, Relief = 14f, ReliefScale = 230f, ReliefDetail = 0.08f,
                Base = TownSwatch.Sandstone, Alt = TownSwatch.SandDark, Accent = TownSwatch.Violet, Mottle = TownSwatch.Mud, Hollow = TownSwatch.Indigo, Crest = TownSwatch.Violet,
                Patches = new[] { TownSwatch.Violet, TownSwatch.SandDark, TownSwatch.Mud }, AltThreshold = 0.5f, AccentThreshold = 0.92f,
                Trail = TownSwatch.Slate, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Indigo, Slope = TownSwatch.Sandstone, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.006f, RockTint = TownSwatch.Sandstone,
                Specials = 0.01f, SpecialMix = new[] { OverworldDecor.ApepScaleSlab, OverworldDecor.StrandedHull },
                Tufts = 0f, Pebbles = 0.05f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The purple scales and quicksand (76): the serpent's skin (#5A3A6E) on dark sand.
            styles[(int)OverworldTerrain.ApepScales] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ApepScales, Shape = ReliefShape.ApepBody, Relief = 12f, ReliefScale = 190f, ReliefDetail = 0.1f,
                Base = TownSwatch.Violet, Alt = TownSwatch.SandDark, Accent = TownSwatch.Lotus, Mottle = TownSwatch.Indigo, Hollow = TownSwatch.Black, Crest = TownSwatch.Lotus,
                Patches = new[] { TownSwatch.Indigo, TownSwatch.SandDark, TownSwatch.Mud }, AltThreshold = 0.48f, AccentThreshold = 0.93f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Indigo, Cliff = TownSwatch.Black, Slope = TownSwatch.Violet, Bank = TownSwatch.Mud, Bed = TownSwatch.SandDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.01f, RockTint = TownSwatch.Indigo,
                Specials = 0.012f, SpecialMix = new[] { OverworldDecor.ApepScaleSlab, OverworldDecor.ApepScaleSlab, OverworldDecor.ScarabIdol },
                Tufts = 0f, Pebbles = 0.06f,
                WaterVegetation = hills, WaterVariants = pines,
            };
        }
    }
}
