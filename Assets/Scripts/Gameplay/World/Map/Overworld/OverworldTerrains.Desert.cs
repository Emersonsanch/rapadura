using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 2 — Areias Flamejantes: the look of the desert terrains (Docs/Mapas100.md §5, 07–12): sand <c>#E7C27D</c>,
    /// red sandstone <c>#B5482F</c>, camel thorn, old gold and violet shadow, salt <c>#F0E6DA</c>; the date palms, pink
    /// tamarisks and desert poplars of the town kit's desert flora; each area with its own piece
    /// (<see cref="OverworldDecorKit"/>, OverworldDecorKit.Desert.cs).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A desert terrain (Região 2, 20–39).</summary>
        public static bool IsDesert(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 20 && i < 40;
        }

        private static void DesertStyles(OverworldTerrainStyle[] styles)
        {
            // 07 · the caravan road: dry steppe turning to sand.
            styles[(int)OverworldTerrain.CaravanSteppe] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CaravanSteppe, Relief = 5f, ReliefScale = 240f, ReliefDetail = 0.35f,
                Base = TownSwatch.Thatch, Alt = TownSwatch.SandDark, Accent = TownSwatch.Sand, Mottle = TownSwatch.LeafGold, Crest = TownSwatch.Sand,
                Hollow = TownSwatch.Dirt, Patches = new[] { TownSwatch.Sand, TownSwatch.Dirt, TownSwatch.LeafGold }, AltThreshold = 0.5f, AccentThreshold = 0.74f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.SandDark, Slope = TownSwatch.Sand, Bank = TownSwatch.Sand,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1, 2 }, Trees = 0.008f, Bushes = 0.08f, Rocks = 0.02f,
                RockTint = TownSwatch.Sandstone, Specials = 0.006f, SpecialMix = new[] { OverworldDecor.Marker, OverworldDecor.Bones, OverworldDecor.KarezShaft },
                Tufts = 0.28f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.04f,
                Flowers = 0.015f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothYellow },
            };

            // The Oásis de Qocho: palms and green patches round the water.
            styles[(int)OverworldTerrain.Oasis] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Oasis, Relief = 1.6f, ReliefScale = 180f, ReliefDetail = 0.12f,
                Base = TownSwatch.Sand, Alt = TownSwatch.Vine, Accent = TownSwatch.Grass, Mottle = TownSwatch.Thatch, Hollow = TownSwatch.Vine,
                Patches = new[] { TownSwatch.Vine, TownSwatch.Grass, TownSwatch.Thatch }, AltThreshold = 0.55f, AccentThreshold = 0.8f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.SandDark, Slope = TownSwatch.Sand, Bank = TownSwatch.Sand,
                Bed = TownSwatch.SandDark, Vegetation = TownVegetation.Desert, TreeVariants = new[] { 0, 0, 3, 1 }, Trees = 0.07f, Bushes = 0.05f,
                Rocks = 0.004f, RockTint = TownSwatch.Adobe, TreeScale = new Vector2(1f, 1.45f), WaterVegetation = TownVegetation.Desert,
                WaterVariants = new[] { 0, 0, 3, 1 }, Tufts = 0.3f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Grass, Reeds = 0.01f,
                Flowers = 0.06f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothYellow, TownSwatch.Fruit }, Pebbles = 0.01f,
            };

            // The palm grove south of Qocho: date palms in rows, watered by the karez.
            styles[(int)OverworldTerrain.PalmGrove] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PalmGrove, Relief = 1.2f, ReliefScale = 200f, ReliefDetail = 0.08f,
                Base = TownSwatch.Dirt, Alt = TownSwatch.Vine, Accent = TownSwatch.SandDark, Mottle = TownSwatch.Grass,
                Patches = new[] { TownSwatch.Vine, TownSwatch.Grass, TownSwatch.SandDark }, AltThreshold = 0.52f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Dirt, Bank = TownSwatch.Mud,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 0, 3 }, Trees = 0.72f, Bushes = 0.03f, Rocks = 0.003f, Rows = true,
                TreeScale = new Vector2(0.9f, 1.3f), WaterVegetation = TownVegetation.Desert, WaterVariants = new[] { 0, 1 },
                Specials = 0.004f, SpecialMix = new[] { OverworldDecor.KarezShaft }, Tufts = 0.25f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Grass,
                Flowers = 0.03f, FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Tamarisk }, Reeds = 0.01f, Puddles = 0.012f, Pebbles = 0f,
            };

            // 08 · the Artífices' stony desert (hamada): gravel, broken lotus columns, ushabtis, markers.
            styles[(int)OverworldTerrain.StoneDesert] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.StoneDesert, Relief = 4f, ReliefScale = 150f, ReliefDetail = 0.3f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Cobble, Accent = TownSwatch.PavingDark, Mottle = TownSwatch.Dirt, Crest = TownSwatch.Gold,
                Patches = new[] { TownSwatch.Cobble, TownSwatch.PavingDark, TownSwatch.Gold }, AltThreshold = 0.52f, AccentThreshold = 0.78f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Cobble, Cliff = TownSwatch.PavingDark, Slope = TownSwatch.Dirt,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0.004f, Bushes = 0.03f, Rocks = 0.06f,
                RockScale = new Vector2(0.6f, 1.8f), RockTint = TownSwatch.Sandstone, Specials = 0.02f,
                SpecialMix = new[] { OverworldDecor.LotusColumn, OverworldDecor.Ushabtis, OverworldDecor.Marker },
                Tufts = 0.1f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.12f,
            };

            // Low rolling dunes.
            styles[(int)OverworldTerrain.LowDunes] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LowDunes, Shape = ReliefShape.Dunes, Relief = 5f, ReliefScale = 70f, ReliefDetail = 0.06f,
                Base = TownSwatch.Sand, Alt = TownSwatch.SandDark, Accent = TownSwatch.Adobe, Mottle = TownSwatch.Thatch, Crest = TownSwatch.Adobe,
                Hollow = TownSwatch.SandDark, Patches = new[] { TownSwatch.SandDark, TownSwatch.Adobe }, AltThreshold = 0.58f, AccentThreshold = 0.82f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.AdobeOchre, Slope = TownSwatch.SandDark,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0.003f, Bushes = 0.025f, Rocks = 0.002f,
                RockTint = TownSwatch.Sandstone, Specials = 0.004f, SpecialMix = new[] { OverworldDecor.Bones },
                Tufts = 0.06f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.008f,
                Flowers = 0.008f, FlowerColors = new[] { TownSwatch.ClothYellow, TownSwatch.Tamarisk },
            };

            // The erg: high crested dunes, almost nothing grows.
            styles[(int)OverworldTerrain.Erg] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Erg, Shape = ReliefShape.Dunes, Relief = 13f, ReliefScale = 110f, ReliefDetail = 0.05f,
                Base = TownSwatch.Sand, Alt = TownSwatch.Adobe, Accent = TownSwatch.SandDark, Crest = TownSwatch.Plaster, Hollow = TownSwatch.AdobeOchre,
                Patches = new[] { TownSwatch.Adobe, TownSwatch.Plaster }, AltThreshold = 0.6f, AccentThreshold = 0.85f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.AdobeOchre, Slope = TownSwatch.SandDark,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 0 }, Trees = 0f, Bushes = 0.01f, Rocks = 0.001f,
                RockTint = TownSwatch.Sandstone, Specials = 0.004f, SpecialMix = new[] { OverworldDecor.Bones, OverworldDecor.Bones, OverworldDecor.Marker },
                Tufts = 0.025f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Sand, Pebbles = 0.004f,
            };

            // Red sandstone canyons: chimneys and natural arches.
            styles[(int)OverworldTerrain.RedCanyon] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.RedCanyon, Shape = ReliefShape.Canyons, Relief = 15f, ReliefScale = 160f, ReliefDetail = 0.4f, TerraceSteps = 4,
                Base = TownSwatch.Laterite, Alt = TownSwatch.Sandstone, Accent = TownSwatch.AdobeOchre, Mottle = TownSwatch.RedClay, Crest = TownSwatch.AdobeOchre,
                Hollow = TownSwatch.SandDark, Patches = new[] { TownSwatch.Sandstone, TownSwatch.RedClay, TownSwatch.SandDark }, AltThreshold = 0.55f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.RedClay, Bank = TownSwatch.SandDark,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0.004f, Bushes = 0.03f, Rocks = 0.04f,
                RockScale = new Vector2(1f, 3f), RockTint = TownSwatch.Sandstone, Specials = 0.02f,
                SpecialMix = new[] { OverworldDecor.Hoodoo, OverworldDecor.Hoodoo, OverworldDecor.RockArch },
                Tufts = 0.06f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.05f,
            };

            // Salt flats: white crust (OverworldChunkBuilder.Desert.cs), brine pools, a few camel thorns.
            styles[(int)OverworldTerrain.SaltFlat] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SaltFlat, Relief = 0.6f, ReliefScale = 200f, ReliefDetail = 0.03f,
                Base = TownSwatch.LimeWhite, Alt = TownSwatch.Plaster, Accent = TownSwatch.Pearl, Mottle = TownSwatch.CoralLime,
                Patches = new[] { TownSwatch.Plaster, TownSwatch.Pearl, TownSwatch.CoralLime }, AltThreshold = 0.5f, AccentThreshold = 0.8f,
                Trail = TownSwatch.PavingDark, Clearing = TownSwatch.Paving, Cliff = TownSwatch.Paving, Slope = TownSwatch.Plaster, Bank = TownSwatch.Plaster,
                Bed = TownSwatch.Mud, Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0f, Bushes = 0.006f, Rocks = 0.004f,
                RockTint = TownSwatch.Paving, Specials = 0.003f, SpecialMix = new[] { OverworldDecor.Bones },
                Tufts = 0.01f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Plaster, Pebbles = 0.02f,
            };

            // The Meroë pyramid fields: steep pyramids, lotus columns, ushabtis, tells.
            styles[(int)OverworldTerrain.MeroeRuins] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MeroeRuins, Shape = ReliefShape.Mounds, Relief = 5f, ReliefScale = 140f, ReliefDetail = 0.25f,
                Base = TownSwatch.Sand, Alt = TownSwatch.SandDark, Accent = TownSwatch.Gold, Mottle = TownSwatch.Adobe, Crest = TownSwatch.Gold,
                Patches = new[] { TownSwatch.Gold, TownSwatch.Paving, TownSwatch.Violet }, AltThreshold = 0.55f, AccentThreshold = 0.8f,
                Trail = TownSwatch.Paving, Clearing = TownSwatch.Paving, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.SandDark,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 0 }, Trees = 0.004f, Bushes = 0.02f, Rocks = 0.02f,
                RockTint = TownSwatch.Sandstone, Walls = 0.008f, Specials = 0.03f,
                SpecialMix = new[] { OverworldDecor.Pyramid, OverworldDecor.LotusColumn, OverworldDecor.Ushabtis, OverworldDecor.LotusColumn },
                Tufts = 0.06f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.05f,
            };

            // Flat-topped sandstone mesas over a sand floor.
            styles[(int)OverworldTerrain.Mesas] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Mesas, Shape = ReliefShape.Mesas, Relief = 20f, ReliefScale = 210f, ReliefDetail = 0.15f,
                Base = TownSwatch.Sand, Alt = TownSwatch.AdobeOchre, Accent = TownSwatch.Laterite, Mottle = TownSwatch.SandDark, Crest = TownSwatch.Laterite,
                Hollow = TownSwatch.Sand, Patches = new[] { TownSwatch.AdobeOchre, TownSwatch.Laterite }, AltThreshold = 0.58f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.AdobeOchre,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0.004f, Bushes = 0.035f, Rocks = 0.035f,
                RockScale = new Vector2(1f, 3.2f), RockTint = TownSwatch.Sandstone, Specials = 0.012f, SpecialMix = new[] { OverworldDecor.Hoodoo },
                Tufts = 0.07f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.SandDark, Pebbles = 0.04f,
            };

            // Lines of karez shafts across the clay plain.
            styles[(int)OverworldTerrain.KarezField] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.KarezField, Relief = 2.2f, ReliefScale = 200f, ReliefDetail = 0.15f,
                Base = TownSwatch.Mud, Alt = TownSwatch.Dirt, Accent = TownSwatch.SandDark, Mottle = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.Dirt, TownSwatch.SandDark }, AltThreshold = 0.52f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.Dirt, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Dirt, Bank = TownSwatch.Mud,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1 }, Trees = 0.006f, Bushes = 0.04f, Rocks = 0.01f,
                RockTint = TownSwatch.Adobe, Specials = 0.02f, SpecialMix = new[] { OverworldDecor.KarezShaft },
                Tufts = 0.12f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Vine, Pebbles = 0.03f,
            };

            // A tamarisk thicket round an oasis lake.
            styles[(int)OverworldTerrain.TamariskOasis] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TamariskOasis, Relief = 2.5f, ReliefScale = 170f, ReliefDetail = 0.2f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Vine, Accent = TownSwatch.Grass, Mottle = TownSwatch.Thatch,
                Patches = new[] { TownSwatch.Vine, TownSwatch.Tamarisk, TownSwatch.Grass }, AltThreshold = 0.5f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.SandDark, Bank = TownSwatch.Sand,
                Bed = TownSwatch.Mud, Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1, 1, 1, 2, 0 }, Trees = 0.13f, Bushes = 0.08f,
                Rocks = 0.006f, RockTint = TownSwatch.Adobe, WaterVegetation = TownVegetation.Desert, WaterVariants = new[] { 1, 1, 0 },
                Logs = 0.004f, Reeds = 0.03f, Tufts = 0.3f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Thatch,
                Flowers = 0.05f, FlowerColors = new[] { TownSwatch.Tamarisk, TownSwatch.ClothWhite }, Pebbles = 0.01f,
            };

            // Dark sand strewn with bones and broken caravans.
            styles[(int)OverworldTerrain.BoneValley] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BoneValley, Relief = 6f, ReliefScale = 200f, ReliefDetail = 0.3f,
                Base = TownSwatch.SandDark, Alt = TownSwatch.Dirt, Accent = TownSwatch.DirtDark, Mottle = TownSwatch.Mud, Crest = TownSwatch.Sand,
                Hollow = TownSwatch.DirtDark, Patches = new[] { TownSwatch.Dirt, TownSwatch.DirtDark, TownSwatch.Violet }, AltThreshold = 0.52f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Dirt,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0.003f, Bushes = 0.03f, Rocks = 0.025f,
                RockTint = TownSwatch.StoneDark, Specials = 0.035f, SpecialMix = new[] { OverworldDecor.Bones, OverworldDecor.Bones, OverworldDecor.CaravanWreck },
                Tufts = 0.08f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.DirtDark, Pebbles = 0.06f,
            };

            // 09 · the Galerias do Karez seen from above: sunken clay, spoil mounds, shafts and seeping wells.
            styles[(int)OverworldTerrain.KarezGallery] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.KarezGallery, Shape = ReliefShape.Mounds, Relief = 3f, ReliefScale = 120f, ReliefDetail = 0.2f,
                Base = TownSwatch.Mud, Alt = TownSwatch.DirtDark, Accent = TownSwatch.AdobeOchre, Mottle = TownSwatch.Dirt, Hollow = TownSwatch.DirtDark,
                Patches = new[] { TownSwatch.DirtDark, TownSwatch.AdobeOchre, TownSwatch.Water }, AltThreshold = 0.5f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.DirtDark, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.Mud, Bank = TownSwatch.Mud,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 1, 0 }, Trees = 0.01f, Bushes = 0.04f, Rocks = 0.02f,
                RockTint = TownSwatch.Adobe, Specials = 0.035f, SpecialMix = new[] { OverworldDecor.KarezShaft, OverworldDecor.KarezShaft, OverworldDecor.Marker },
                Puddles = 0.01f, Tufts = 0.12f, TuftColor = TownSwatch.Vine, TuftTip = TownSwatch.Thatch, Pebbles = 0.03f,
            };

            // 10 · the Dunas que Cantam: the tallest dunes, palms round the crescent lake.
            styles[(int)OverworldTerrain.SingingDunes] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SingingDunes, Shape = ReliefShape.Dunes, Relief = 17f, ReliefScale = 150f, ReliefDetail = 0.05f,
                Base = TownSwatch.Adobe, Alt = TownSwatch.Sand, Accent = TownSwatch.AdobeOchre, Mottle = TownSwatch.Thatch, Crest = TownSwatch.Plaster,
                Hollow = TownSwatch.AdobeOchre, Patches = new[] { TownSwatch.Sand, TownSwatch.AdobeOchre, TownSwatch.Plaster }, AltThreshold = 0.56f,
                Trail = TownSwatch.SandDark, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.AdobeOchre, Slope = TownSwatch.SandDark, Bank = TownSwatch.Sand,
                Bed = TownSwatch.SandDark, Vegetation = TownVegetation.Desert, TreeVariants = new[] { 0 }, Trees = 0.002f, Bushes = 0.01f, Rocks = 0.001f,
                RockTint = TownSwatch.Sandstone, WaterVegetation = TownVegetation.Desert, WaterVariants = new[] { 0, 0, 3, 1 }, Specials = 0.004f,
                SpecialMix = new[] { OverworldDecor.Bones, OverworldDecor.Marker }, Tufts = 0.03f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Sand,
                Pebbles = 0f, Reeds = 0.005f,
            };

            // 11 · the Ermos do Tambor Rubro: layered red badlands, chimneys, arches, the crowned clans' camps.
            styles[(int)OverworldTerrain.Badlands] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Badlands, Shape = ReliefShape.Canyons, Relief = 18f, ReliefScale = 150f, ReliefDetail = 0.4f, TerraceSteps = 5,
                Base = TownSwatch.RedClay, Alt = TownSwatch.Laterite, Accent = TownSwatch.Sandstone, Mottle = TownSwatch.AdobeOchre, Crest = TownSwatch.AdobeOchre,
                Hollow = TownSwatch.SandDark, Patches = new[] { TownSwatch.Laterite, TownSwatch.Sandstone, TownSwatch.AdobeOchre }, AltThreshold = 0.5f,
                Trail = TownSwatch.AdobeOchre, Clearing = TownSwatch.SandDark, Cliff = TownSwatch.Sandstone, Slope = TownSwatch.Laterite, Bank = TownSwatch.SandDark,
                Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0.002f, Bushes = 0.03f, Rocks = 0.04f,
                RockScale = new Vector2(1f, 3f), RockTint = TownSwatch.Sandstone, Specials = 0.025f,
                SpecialMix = new[] { OverworldDecor.Hoodoo, OverworldDecor.RockArch, OverworldDecor.Hoodoo, OverworldDecor.ClanTent },
                Tufts = 0.05f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.AdobeOchre, Pebbles = 0.05f,
            };

            // 12 · the Trono de Namrael: three terraces of salt, red crystal, rammed-earth towers.
            styles[(int)OverworldTerrain.SaltThrone] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SaltThrone, Shape = ReliefShape.Terraced, Relief = 14f, ReliefScale = 200f, ReliefDetail = 0.1f, TerraceSteps = 3,
                Base = TownSwatch.LimeWhite, Alt = TownSwatch.Plaster, Accent = TownSwatch.Violet, Mottle = TownSwatch.Pearl, Crest = TownSwatch.LimeWhite,
                Patches = new[] { TownSwatch.Plaster, TownSwatch.Violet, TownSwatch.Sandstone }, AltThreshold = 0.55f, AccentThreshold = 0.86f,
                Trail = TownSwatch.PavingDark, Clearing = TownSwatch.Paving, Cliff = TownSwatch.Laterite, Slope = TownSwatch.Plaster, Bank = TownSwatch.Plaster,
                Bed = TownSwatch.Laterite, Vegetation = TownVegetation.Desert, TreeVariants = new[] { 2 }, Trees = 0f, Bushes = 0.004f, Rocks = 0.02f,
                RockTint = TownSwatch.Paving, Specials = 0.03f,
                SpecialMix = new[] { OverworldDecor.SaltCrystal, OverworldDecor.SaltCrystal, OverworldDecor.TaipaTower, OverworldDecor.Bones },
                Tufts = 0.005f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Plaster, Pebbles = 0.02f,
            };
        }
    }
}
