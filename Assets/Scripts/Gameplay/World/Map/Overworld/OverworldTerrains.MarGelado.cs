using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 11 — Mar Gelado: the look of the frozen sea's terrains (200–219, Docs/Mapas100.md §5 região 11, maps 59–64): the
    /// snowy shore and the Porto da Lua Branca's ice islands (gelo <c>#D8E9F2</c>, neve <c>#F7FAFC</c>, tábua <c>#5B4A3E</c>), the
    /// frozen bay, the Canais de Gelo (gelo azul, água negra <c>#1F3A4A</c>) and the cracked ice of their pressure ridges, Omöl's
    /// tundra of moss and lichen (musgo <c>#8E9A6A</c>, líquen <c>#C8C29A</c>, lagos <c>#6E8FA6</c>, pedras redondas) with its lakes and
    /// Yen's reindeer pastures, the dark fjords and the Galerias dos Mamutes (terra <c>#5A4A3A</c>, cristal, presas) over frozen earth,
    /// the pack ice and the icebergs of the Mar das Auroras (aurora verde-lilás <c>#7FE0B0</c>/<c>#B98CE0</c>) round the lighthouse
    /// isle, and Old Kul's huge frozen lake of black-silver ice (água negra, prata de peixe). The relief stays low on the shore, the
    /// harbour, the bay, the tundra and the lake (≤ 6 m), rolls in the pastures and the frozen earth (~8 m), and stands up in the
    /// pressure ridges (~10 m), the ice vaults (~22 m), the icebergs (~26 m) and the fjords' walls (~34 m), each over a low floor
    /// where the trails and clearings run. Hardly a tree: a few of the Serra's pines in the fjords and on the frozen earth; the
    /// region's own pieces (OverworldDecorKit.MarGelado.cs) are the snow domes, plank houses, crescent boats, whalebone arches,
    /// ice blocks and icebergs, herders' tents, lichen boulders, mammoth tusks, ice lanterns, fish racks, ice beacons and frost
    /// crystals. Only the palette rows every build already has (no dependency on the newer city rows).
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Mar Gelado terrain (Região 11, 200–219).</summary>
        public static bool IsMarGelado(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 200 && i < 220;
        }

        private static void MarGeladoStyles(OverworldTerrainStyle[] styles)
        {
            TownVegetation hills = TownVegetation.Mountain;
            int[] cedars = { 0, 2 };
            int[] pines = { 2 };

            // The snowy shore: snow over grey shingle, drifts, whale bones and boats drawn up.
            styles[(int)OverworldTerrain.SnowShore] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.SnowShore, Relief = 4f, ReliefScale = 240f, ReliefDetail = 0.08f,
                Base = TownSwatch.Snow, Alt = TownSwatch.Firn, Accent = TownSwatch.Stone, Mottle = TownSwatch.LimeWhite, Hollow = TownSwatch.Firn,
                Patches = new[] { TownSwatch.Stone, TownSwatch.Firn, TownSwatch.Cobble }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Cobble, Clearing = TownSwatch.Firn, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Firn, Bank = TownSwatch.Stone, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.004f, Rocks = 0.012f, RockTint = TownSwatch.StoneDark,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.CrescentBoat, OverworldDecor.LichenBoulder },
                Tufts = 0.05f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Straw, Pebbles = 0.04f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The harbour's ice islands (59): packed snow round the town, snow domes, plank houses, ice lanterns.
            styles[(int)OverworldTerrain.MoonHarbor] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MoonHarbor, Relief = 2.5f, ReliefScale = 260f, ReliefDetail = 0.05f,
                Base = TownSwatch.Snow, Alt = TownSwatch.LimeWhite, Accent = TownSwatch.Pearl, Mottle = TownSwatch.Firn, Hollow = TownSwatch.Glass,
                Patches = new[] { TownSwatch.Pearl, TownSwatch.Firn, TownSwatch.WoodWeathered }, AltThreshold = 0.55f, AccentThreshold = 0.9f,
                Trail = TownSwatch.WoodWeathered, Clearing = TownSwatch.Firn, Cliff = TownSwatch.Glass, Slope = TownSwatch.Firn, Bank = TownSwatch.Pearl, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.004f, RockTint = TownSwatch.Pearl,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.SnowDome, OverworldDecor.IceLantern, OverworldDecor.PlankLonghouse },
                Tufts = 0f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The frozen bay: flat plates of sea ice, the odd boat frozen in, fishing holes.
            styles[(int)OverworldTerrain.FrozenBay] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FrozenBay, Shape = ReliefShape.Floes, Relief = 3f, ReliefScale = 220f, ReliefDetail = 0.04f,
                Base = TownSwatch.Firn, Alt = TownSwatch.Pearl, Accent = TownSwatch.Glass, Mottle = TownSwatch.Snow, Hollow = TownSwatch.Glass, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.Glass, TownSwatch.Snow, TownSwatch.SkyBlue }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Snow, Clearing = TownSwatch.Snow, Cliff = TownSwatch.Glass, Slope = TownSwatch.Pearl, Bank = TownSwatch.Glass, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.004f, RockTint = TownSwatch.Pearl,
                Specials = 0.005f, SpecialMix = new[] { OverworldDecor.CrescentBoat, OverworldDecor.IceBlock },
                Tufts = 0f, Pebbles = 0.004f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The Canais de Gelo (60): blocks of blue ice between black-water canals, thin ice by the water.
            styles[(int)OverworldTerrain.IceCanals] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.IceCanals, Shape = ReliefShape.Floes, Relief = 6f, ReliefScale = 160f, ReliefDetail = 0.06f,
                Base = TownSwatch.Glass, Alt = TownSwatch.Firn, Accent = TownSwatch.SkyBlue, Mottle = TownSwatch.Pearl, Hollow = TownSwatch.Sea, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.SkyBlue, TownSwatch.Snow, TownSwatch.Sea }, AltThreshold = 0.5f, AccentThreshold = 0.84f,
                Trail = TownSwatch.Snow, Clearing = TownSwatch.Firn, Cliff = TownSwatch.SkyBlue, Slope = TownSwatch.Glass, Bank = TownSwatch.Glass, Bed = TownSwatch.BogWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.004f, RockTint = TownSwatch.Glass,
                Specials = 0.012f, Special = OverworldDecor.IceBlock,
                Tufts = 0f, Pebbles = 0.004f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Cracked ice (60): ridges of crushed ice in lines, dark cracks, blocks thrown up.
            styles[(int)OverworldTerrain.CrackedIce] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.CrackedIce, Shape = ReliefShape.PressureRidges, Relief = 10f, ReliefScale = 200f, ReliefDetail = 0.2f,
                Base = TownSwatch.Firn, Alt = TownSwatch.Glass, Accent = TownSwatch.Indigo, Mottle = TownSwatch.Pearl, Hollow = TownSwatch.Glass, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.Glass, TownSwatch.Snow, TownSwatch.Indigo }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Snow, Clearing = TownSwatch.Firn, Cliff = TownSwatch.Glass, Slope = TownSwatch.Pearl, Bank = TownSwatch.Glass, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.01f, RockScale = new Vector2(0.8f, 2f), RockTint = TownSwatch.Glass,
                Specials = 0.02f, SpecialMix = new[] { OverworldDecor.IceBlock, OverworldDecor.IceBlock, OverworldDecor.Iceberg },
                Tufts = 0f, Pebbles = 0.02f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Omöl's tundra (61): flat moss and lichen (musgo #8E9A6A, líquen #C8C29A), round stones, snow patches.
            styles[(int)OverworldTerrain.Tundra] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Tundra, Relief = 4f, ReliefScale = 260f, ReliefDetail = 0.08f,
                Base = TownSwatch.Moss, Alt = TownSwatch.Lichen, Accent = TownSwatch.Snow, Mottle = TownSwatch.SteppeGrass, Hollow = TownSwatch.GroveMoss,
                Crest = TownSwatch.Lichen, Patches = new[] { TownSwatch.Snow, TownSwatch.Lichen, TownSwatch.Stone }, AltThreshold = 0.52f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Lichen, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.GroveMoss, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.01f, Rocks = 0.01f, RockTint = TownSwatch.Stone,
                Specials = 0.008f, Special = OverworldDecor.LichenBoulder,
                Tufts = 0.16f, TuftColor = TownSwatch.Moss, TuftTip = TownSwatch.Lichen, Flowers = 0.02f, Pebbles = 0.02f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.Violet },
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Omöl's lakes (61): low hummocks between the round lakes (lagos #6E8FA6), sedge, lichen.
            styles[(int)OverworldTerrain.TundraLakes] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.TundraLakes, Shape = ReliefShape.Mounds, Relief = 3f, ReliefScale = 200f, ReliefDetail = 0.06f,
                Base = TownSwatch.GroveMoss, Alt = TownSwatch.Moss, Accent = TownSwatch.Lichen, Mottle = TownSwatch.Lichen, Hollow = TownSwatch.Mud,
                Patches = new[] { TownSwatch.Lichen, TownSwatch.Snow, TownSwatch.Rush }, AltThreshold = 0.5f, AccentThreshold = 0.84f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Moss, Cliff = TownSwatch.Mud, Slope = TownSwatch.Moss, Bank = TownSwatch.Rush, Bed = TownSwatch.MurkWater,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.02f, Rocks = 0.008f, RockTint = TownSwatch.Stone,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.LichenBoulder, OverworldDecor.FishRack },
                Tufts = 0.2f, TuftColor = TownSwatch.Rush, TuftTip = TownSwatch.Lichen, Reeds = 0.06f, Pebbles = 0.01f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Yen's reindeer pastures: rolling lichen, herders' tents, racks; a few dwarf pines.
            styles[(int)OverworldTerrain.ReindeerPasture] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.ReindeerPasture, Shape = ReliefShape.Hills, Relief = 6f, ReliefScale = 240f, ReliefDetail = 0.1f,
                Base = TownSwatch.Lichen, Alt = TownSwatch.Moss, Accent = TownSwatch.Snow, Mottle = TownSwatch.SteppeGrass, Hollow = TownSwatch.GroveMoss, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.Snow, TownSwatch.Moss, TownSwatch.Felt }, AltThreshold = 0.54f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.Lichen, Cliff = TownSwatch.StoneDark, Slope = TownSwatch.Moss, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0.002f, Bushes = 0.012f, Rocks = 0.008f, RockTint = TownSwatch.Stone,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.HerderTent, OverworldDecor.FishRack, OverworldDecor.LichenBoulder },
                Tufts = 0.18f, TuftColor = TownSwatch.Lichen, TuftTip = TownSwatch.SteppeGrass, Flowers = 0.02f, Pebbles = 0.01f,
                FlowerColors = new[] { TownSwatch.ClothWhite, TownSwatch.ClothYellow },
                WaterVegetation = hills, WaterVariants = pines,
            };

            // A fjord: high stepped walls of dark rock, snow ledges, a frozen channel; pines on the ledges.
            styles[(int)OverworldTerrain.Fjord] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.Fjord, Shape = ReliefShape.FjordWalls, Relief = 34f, ReliefScale = 240f, ReliefDetail = 0.3f,
                Base = TownSwatch.Firn, Alt = TownSwatch.Snow, Accent = TownSwatch.Basalt, Mottle = TownSwatch.Pearl, Hollow = TownSwatch.Glass, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.Basalt, TownSwatch.Snow, TownSwatch.Glass }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Snow, Clearing = TownSwatch.Firn, Cliff = TownSwatch.Basalt, Slope = TownSwatch.Slate, Bank = TownSwatch.Stone, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.006f, Bushes = 0.004f, Rocks = 0.02f, RockScale = new Vector2(1f, 2.4f), RockTint = TownSwatch.Basalt,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.Iceberg, OverworldDecor.IceBlock },
                Tufts = 0.02f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Snow, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The Galerias dos Mamutes (62): domes of ice and frozen earth (terra #5A4A3A), tusks in the walls, frost crystals.
            styles[(int)OverworldTerrain.MammothGalleries] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.MammothGalleries, Shape = ReliefShape.IceVaults, Relief = 22f, ReliefScale = 200f, ReliefDetail = 0.2f,
                Base = TownSwatch.FortEarth, Alt = TownSwatch.Glass, Accent = TownSwatch.Firn, Mottle = TownSwatch.Mud, Hollow = TownSwatch.DirtDark, Crest = TownSwatch.Glass,
                Patches = new[] { TownSwatch.Glass, TownSwatch.Mud, TownSwatch.Snow }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.DirtDark, Clearing = TownSwatch.Mud, Cliff = TownSwatch.Glass, Slope = TownSwatch.FortEarth, Bank = TownSwatch.Mud, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.012f, RockTint = TownSwatch.FortEarth,
                Specials = 0.02f, SpecialMix = new[] { OverworldDecor.MammothTusk, OverworldDecor.FrostCrystal, OverworldDecor.FrostCrystal },
                Tufts = 0f, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Frozen earth at the foot of the peaks: brown permafrost mounds, tusks out of the ground, pines.
            styles[(int)OverworldTerrain.FrozenEarth] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FrozenEarth, Shape = ReliefShape.Mounds, Relief = 8f, ReliefScale = 210f, ReliefDetail = 0.15f,
                Base = TownSwatch.Mud, Alt = TownSwatch.Snow, Accent = TownSwatch.Lichen, Mottle = TownSwatch.FortEarth, Hollow = TownSwatch.Snow, Crest = TownSwatch.Firn,
                Patches = new[] { TownSwatch.Snow, TownSwatch.Lichen, TownSwatch.DirtDark }, AltThreshold = 0.52f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Dirt, Clearing = TownSwatch.Mud, Cliff = TownSwatch.DirtDark, Slope = TownSwatch.FortEarth, Bank = TownSwatch.Stone, Bed = TownSwatch.StoneDark,
                Vegetation = hills, TreeVariants = cedars, Trees = 0.008f, Bushes = 0.01f, Rocks = 0.012f, RockTint = TownSwatch.StoneDark,
                Specials = 0.006f, Special = OverworldDecor.MammothTusk,
                Tufts = 0.08f, TuftColor = TownSwatch.Thatch, TuftTip = TownSwatch.Lichen, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = cedars,
            };

            // The pack ice of the Mar das Auroras (63): floating plates, dark leads, snow on the plates.
            styles[(int)OverworldTerrain.PackIce] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.PackIce, Shape = ReliefShape.Floes, Relief = 5f, ReliefScale = 190f, ReliefDetail = 0.06f,
                Base = TownSwatch.Snow, Alt = TownSwatch.Glass, Accent = TownSwatch.Sea, Mottle = TownSwatch.Firn, Hollow = TownSwatch.Sea, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.Glass, TownSwatch.Firn, TownSwatch.Jade }, AltThreshold = 0.5f, AccentThreshold = 0.9f,
                Trail = TownSwatch.Firn, Clearing = TownSwatch.Snow, Cliff = TownSwatch.Glass, Slope = TownSwatch.Glass, Bank = TownSwatch.Glass, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.004f, RockTint = TownSwatch.Glass,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.IceBlock, OverworldDecor.Iceberg },
                Tufts = 0f, Pebbles = 0.004f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The Mar das Auroras (63): icebergs out of a frozen sea, the ice tinted green-lilac (#7FE0B0 / #B98CE0) by the light.
            styles[(int)OverworldTerrain.AuroraSea] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.AuroraSea, Shape = ReliefShape.Icebergs, Relief = 26f, ReliefScale = 200f, ReliefDetail = 0.1f,
                Base = TownSwatch.Pearl, Alt = TownSwatch.Jade, Accent = TownSwatch.LilacMist, Mottle = TownSwatch.Glass, Hollow = TownSwatch.Sea, Crest = TownSwatch.Snow,
                Patches = new[] { TownSwatch.LilacMist, TownSwatch.Jade, TownSwatch.Snow }, AltThreshold = 0.56f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Firn, Clearing = TownSwatch.Pearl, Cliff = TownSwatch.Glass, Slope = TownSwatch.Snow, Bank = TownSwatch.Glass, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.004f, RockTint = TownSwatch.Glass,
                Specials = 0.006f, Special = OverworldDecor.Iceberg,
                Tufts = 0f, Pebbles = 0.004f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // The lighthouse isle (63): a rocky snow isle in the pack ice, the ice beacons, lanterns.
            styles[(int)OverworldTerrain.LighthouseIsle] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.LighthouseIsle, Shape = ReliefShape.Hills, Relief = 9f, ReliefScale = 200f, ReliefDetail = 0.2f,
                Base = TownSwatch.Snow, Alt = TownSwatch.Granite, Accent = TownSwatch.Lichen, Mottle = TownSwatch.Stone, Hollow = TownSwatch.Firn, Crest = TownSwatch.Granite,
                Patches = new[] { TownSwatch.Granite, TownSwatch.Lichen, TownSwatch.Glass }, AltThreshold = 0.55f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Stone, Clearing = TownSwatch.Snow, Cliff = TownSwatch.Granite, Slope = TownSwatch.Stone, Bank = TownSwatch.Glass, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0.004f, Rocks = 0.02f, RockTint = TownSwatch.Granite,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.IceBeacon, OverworldDecor.IceLantern },
                Tufts = 0.04f, TuftColor = TownSwatch.Lichen, TuftTip = TownSwatch.Snow, Pebbles = 0.03f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Old Kul's frozen lake (64): a plain of silver-dark ice, the fishing holes, the fishers' racks and boats.
            styles[(int)OverworldTerrain.FrozenLake] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.FrozenLake, Relief = 1.5f, ReliefScale = 300f, ReliefDetail = 0.03f,
                Base = TownSwatch.Pearl, Alt = TownSwatch.Glass, Accent = TownSwatch.Snow, Mottle = TownSwatch.SkyBlue, Hollow = TownSwatch.Glass,
                Patches = new[] { TownSwatch.Snow, TownSwatch.Glass, TownSwatch.CanalWater }, AltThreshold = 0.5f, AccentThreshold = 0.86f,
                Trail = TownSwatch.Snow, Clearing = TownSwatch.Snow, Cliff = TownSwatch.Glass, Slope = TownSwatch.Pearl, Bank = TownSwatch.Snow, Bed = TownSwatch.Sea,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.002f, RockTint = TownSwatch.Pearl,
                Specials = 0.006f, SpecialMix = new[] { OverworldDecor.FishRack, OverworldDecor.CrescentBoat, OverworldDecor.IceLantern },
                Tufts = 0f, Pebbles = 0.004f,
                WaterVegetation = hills, WaterVariants = pines,
            };

            // Black ice (64): clear dark ice over the deep (água negra), cracks, frozen bubbles, low ridges.
            styles[(int)OverworldTerrain.BlackIce] = new OverworldTerrainStyle
            {
                Terrain = OverworldTerrain.BlackIce, Shape = ReliefShape.PressureRidges, Relief = 5f, ReliefScale = 240f, ReliefDetail = 0.08f,
                Base = TownSwatch.Sea, Alt = TownSwatch.BogWater, Accent = TownSwatch.Pearl, Mottle = TownSwatch.Indigo, Hollow = TownSwatch.BogWater, Crest = TownSwatch.Pearl,
                Patches = new[] { TownSwatch.Pearl, TownSwatch.Indigo, TownSwatch.Snow }, AltThreshold = 0.5f, AccentThreshold = 0.88f,
                Trail = TownSwatch.Snow, Clearing = TownSwatch.Pearl, Cliff = TownSwatch.Glass, Slope = TownSwatch.Indigo, Bank = TownSwatch.Pearl, Bed = TownSwatch.Black,
                Vegetation = hills, TreeVariants = pines, Trees = 0f, Bushes = 0f, Rocks = 0.004f, RockTint = TownSwatch.Indigo,
                Specials = 0.008f, SpecialMix = new[] { OverworldDecor.IceBlock, OverworldDecor.FrostCrystal },
                Tufts = 0f, Pebbles = 0.004f,
                WaterVegetation = hills, WaterVariants = pines,
            };
        }
    }
}
