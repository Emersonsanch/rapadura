using Rapadura.Gameplay.World.Cities.Town;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 15 — Limiar Profundo: the look of its terrains (280–309, Docs/Mapas100.md §5 região 15, maps 83–90): basalt <c>#2C2C33</c>, amber <c>#FFB347</c>,
    /// copper <c>#B06B3A</c>, white of cloth <c>#F2EEE6</c> and indigo <c>#232046</c> round the fortress; the moving grey of the Pit of Smoke; embers and black
    /// in the Pit of Devouring Fire; blue-grey in the shaft of the wind; black-green in the Pit of Mire; total black and gold in the Pit of Darkness; the gold
    /// and lilac of the Bridge of the Bōlos; the cracked stone of the Gate. Every pit has its own ground, relief and pieces.
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Limiar Profundo terrain (Região 15, 280–309).</summary>
        public static bool IsProfundo(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 280 && i < 310;
        }

        private static readonly int[] ProfundoNoTrees = { 2 };

        private static OverworldTerrainStyle ProfundoStyle(OverworldTerrain terrain, ReliefShape shape, int steps, float relief, float scale, float detail,
            TownSwatch ground, TownSwatch alt, TownSwatch accent, TownSwatch mottle, TownSwatch hollow, TownSwatch crest, TownSwatch trail, TownSwatch clearing, TownSwatch cliff,
            float rocks, TownSwatch rockTint, float specials, OverworldDecor[] mix, float tufts, TownSwatch tuft, TownSwatch tuftTip, float pebbles, float puddles = 0f)
        {
            return new OverworldTerrainStyle
            {
                Terrain = terrain, Shape = shape, TerraceSteps = steps, Relief = relief, ReliefScale = scale, ReliefDetail = detail,
                Base = ground, Alt = alt, Accent = accent, Mottle = mottle, Hollow = hollow, Crest = crest,
                Patches = new[] { alt, mottle, accent }, AltThreshold = 0.5f, AccentThreshold = 0.93f,
                Trail = trail, Clearing = clearing, Cliff = cliff, Slope = ground, Bank = hollow, Bed = hollow,
                Vegetation = TownVegetation.Mountain, TreeVariants = ProfundoNoTrees, Trees = 0f, Bushes = 0f, Rocks = rocks, RockTint = rockTint,
                Specials = specials, SpecialMix = mix, Tufts = tufts, TuftColor = tuft, TuftTip = tuftTip, Pebbles = pebbles, Puddles = puddles,
                WaterVegetation = TownVegetation.Mountain, WaterVariants = ProfundoNoTrees,
            };
        }

        private static void ProfundoStyles(OverworldTerrainStyle[] styles)
        {
            void Add(OverworldTerrainStyle s) => styles[(int)s.Terrain] = s;
            const ReliefShape rolling = ReliefShape.Rolling;
            const ReliefShape terraced = ReliefShape.Terraced;
            const ReliefShape hummocks = ReliefShape.Hummocks;
            const ReliefShape corridors = ReliefShape.SmokeCorridors;
            const ReliefShape cells = ReliefShape.EmberCells;
            const ReliefShape shaft = ReliefShape.ShaftTerraces;
            const ReliefShape labyrinth = ReliefShape.DarkLabyrinth;
            const ReliefShape causeway = ReliefShape.NoveloCauseway;
            TownSwatch basalt = TownSwatch.LampBasalt;
            TownSwatch amber = TownSwatch.ThousandLamps;
            TownSwatch copper = TownSwatch.LampCopper;
            TownSwatch linen = TownSwatch.WickLinen;
            TownSwatch abyss = TownSwatch.AbyssBlack;
            TownSwatch indigo = TownSwatch.AbyssIndigo;
            TownSwatch dust = TownSwatch.BasaltDust;
            TownSwatch oil = TownSwatch.OilDark;

            // 83: the fortress and its six areas.
            Add(ProfundoStyle(OverworldTerrain.LampFortress, rolling, 0, 2f, 240f, 0.05f, basalt, dust, amber, TownSwatch.StoneDark, abyss, dust, TownSwatch.Cobble, dust, abyss,
                0.006f, basalt, 0.012f, new[] { OverworldDecor.LampNicheWall, OverworldDecor.SealSlab }, 0.02f, dust, amber, 0.05f));
            Add(ProfundoStyle(OverworldTerrain.RampartArc, terraced, 2, 3f, 220f, 0.06f, dust, basalt, copper, TownSwatch.StoneDark, abyss, basalt, TownSwatch.Cobble, TownSwatch.StoneDark, abyss,
                0.01f, basalt, 0.012f, new[] { OverworldDecor.LampNicheWall, OverworldDecor.Basalt }, 0.03f, dust, copper, 0.06f));
            Add(ProfundoStyle(OverworldTerrain.BasaltNiches, hummocks, 0, 4f, 180f, 0.1f, basalt, TownSwatch.StoneDark, amber, dust, abyss, dust, TownSwatch.Cobble, dust, abyss,
                0.016f, basalt, 0.014f, new[] { OverworldDecor.LampNicheWall, OverworldDecor.LampNicheWall, OverworldDecor.Basalt }, 0.02f, dust, amber, 0.08f));
            Add(ProfundoStyle(OverworldTerrain.OilCisterns, rolling, 0, 2.5f, 200f, 0.06f, oil, basalt, copper, dust, abyss, dust, TownSwatch.Cobble, oil, abyss,
                0.006f, oil, 0.016f, new[] { OverworldDecor.OilCistern, OverworldDecor.OilCistern, OverworldDecor.WickRack }, 0.03f, dust, copper, 0.04f, 0.04f));
            Add(ProfundoStyle(OverworldTerrain.WickYards, terraced, 3, 3f, 210f, 0.05f, dust, linen, amber, TownSwatch.StoneDark, basalt, linen, TownSwatch.Curb, dust, basalt,
                0.004f, dust, 0.016f, new[] { OverworldDecor.WickRack, OverworldDecor.WickRack, OverworldDecor.OilCistern }, 0.1f, linen, amber, 0.02f));
            Add(ProfundoStyle(OverworldTerrain.AbyssBrink, hummocks, 0, 5f, 170f, 0.1f, abyss, indigo, amber, basalt, abyss, dust, TownSwatch.Cobble, indigo, abyss,
                0.02f, abyss, 0.01f, new[] { OverworldDecor.SealSlab, OverworldDecor.LampNicheWall }, 0.01f, indigo, amber, 0.06f));

            // 84: the Pit of Smoke.
            Add(ProfundoStyle(OverworldTerrain.SmokePit, corridors, 0, 7f, 210f, 0.08f, TownSwatch.Granite, TownSwatch.Stone, amber, TownSwatch.StoneDark, abyss, TownSwatch.Granite, TownSwatch.Cobble, TownSwatch.Stone, abyss,
                0.012f, TownSwatch.StoneDark, 0.014f, new[] { OverworldDecor.SmokeColumn, OverworldDecor.ArtificerVent }, 0.02f, TownSwatch.StoneDark, amber, 0.05f));
            Add(ProfundoStyle(OverworldTerrain.ArtificerVents, corridors, 0, 5.5f, 180f, 0.06f, TownSwatch.Stone, copper, amber, TownSwatch.Granite, abyss, TownSwatch.Stone, TownSwatch.Copper, TownSwatch.Granite, abyss,
                0.008f, TownSwatch.Granite, 0.016f, new[] { OverworldDecor.ArtificerVent, OverworldDecor.ArtificerVent, OverworldDecor.SmokeColumn }, 0.02f, TownSwatch.Stone, copper, 0.06f));
            Add(ProfundoStyle(OverworldTerrain.CinderCorridors, corridors, 0, 8f, 150f, 0.1f, TownSwatch.StoneDark, TownSwatch.Granite, TownSwatch.Ember, abyss, abyss, TownSwatch.Granite, TownSwatch.Cobble, TownSwatch.Granite, abyss,
                0.02f, TownSwatch.StoneDark, 0.012f, new[] { OverworldDecor.CinderPile, OverworldDecor.SmokeColumn }, 0.01f, TownSwatch.Granite, TownSwatch.Ember, 0.08f));

            // 85: the Pit of Devouring Fire.
            Add(ProfundoStyle(OverworldTerrain.DevouringFirePit, cells, 0, 3.5f, 200f, 0.05f, TownSwatch.Basalt, TownSwatch.Ember, amber, TownSwatch.StoneDark, abyss, TownSwatch.Ember, TownSwatch.Ember, TownSwatch.Basalt, abyss,
                0.012f, TownSwatch.Basalt, 0.016f, new[] { OverworldDecor.EmberStone, OverworldDecor.EmberStone, OverworldDecor.CinderPile }, 0.01f, TownSwatch.Ember, amber, 0.06f));
            Add(ProfundoStyle(OverworldTerrain.GlowPatternFloor, cells, 0, 3f, 240f, 0.04f, TownSwatch.Basalt, TownSwatch.ColdEmber, TownSwatch.Ember, abyss, abyss, TownSwatch.Ember, TownSwatch.Ember, TownSwatch.ColdEmber, abyss,
                0.006f, TownSwatch.Basalt, 0.02f, new[] { OverworldDecor.EmberStone, OverworldDecor.EmberStone, OverworldDecor.EmberStone, OverworldDecor.SealSlab }, 0f, TownSwatch.Ember, amber, 0.03f));
            Add(ProfundoStyle(OverworldTerrain.HotAshHalls, hummocks, 0, 3.5f, 190f, 0.08f, TownSwatch.Ember, TownSwatch.Basalt, amber, TownSwatch.StoneDark, abyss, amber, TownSwatch.Ember, TownSwatch.Basalt, abyss,
                0.016f, TownSwatch.Basalt, 0.014f, new[] { OverworldDecor.CinderPile, OverworldDecor.CinderPile, OverworldDecor.Basalt }, 0.01f, TownSwatch.Ember, amber, 0.06f));

            // 86: the Pit of Ruinous Wind.
            Add(ProfundoStyle(OverworldTerrain.WindShaft, shaft, 0, 14f, 230f, 0.06f, TownSwatch.MoonSilver, TownSwatch.Stone, TownSwatch.ColdWater, TownSwatch.Granite, indigo, TownSwatch.ColdWater, TownSwatch.Cobble, TownSwatch.Granite, abyss,
                0.012f, TownSwatch.MoonSilver, 0.012f, new[] { OverworldDecor.FloatStone, OverworldDecor.WindSpire }, 0.02f, TownSwatch.ColdWater, TownSwatch.MoonSilver, 0.06f));
            Add(ProfundoStyle(OverworldTerrain.LoosePlatforms, terraced, 4, 9f, 190f, 0.08f, TownSwatch.MoonSilver, indigo, TownSwatch.ColdWater, TownSwatch.Granite, abyss, TownSwatch.Stone, TownSwatch.Curb, TownSwatch.MoonSilver, abyss,
                0.01f, TownSwatch.Granite, 0.016f, new[] { OverworldDecor.FloatStone, OverworldDecor.FloatStone, OverworldDecor.WindSpire }, 0.01f, TownSwatch.ColdWater, TownSwatch.Stone, 0.05f));
            Add(ProfundoStyle(OverworldTerrain.Updrafts, rolling, 0, 7f, 220f, 0.1f, indigo, TownSwatch.MoonSilver, TownSwatch.ColdWater, TownSwatch.Granite, abyss, TownSwatch.ColdWater, TownSwatch.Curb, TownSwatch.MoonSilver, abyss,
                0.012f, indigo, 0.018f, new[] { OverworldDecor.WindSpire, OverworldDecor.WindSpire, OverworldDecor.FloatStone }, 0.02f, TownSwatch.MoonSilver, TownSwatch.ColdWater, 0.05f));

            // 87: the Pit of Mire.
            Add(ProfundoStyle(OverworldTerrain.MirePit, terraced, 3, 3f, 200f, 0.05f, TownSwatch.NightWater, oil, amber, TownSwatch.Mud, abyss, TownSwatch.MurkWater, TownSwatch.ReedMat, TownSwatch.Mud, abyss,
                0.006f, oil, 0.016f, new[] { OverworldDecor.DrownedColumn, OverworldDecor.SluiceGate }, 0.1f, TownSwatch.MurkWater, TownSwatch.NightWater, 0.02f, 0.05f));
            Add(ProfundoStyle(OverworldTerrain.FloodedHalls, terraced, 4, 2.5f, 180f, 0.04f, oil, TownSwatch.MurkWater, TownSwatch.Moss, TownSwatch.Mud, abyss, TownSwatch.MurkWater, TownSwatch.ReedMat, TownSwatch.Mud, abyss,
                0.004f, oil, 0.018f, new[] { OverworldDecor.DrownedColumn, OverworldDecor.DrownedColumn, OverworldDecor.Stump }, 0.12f, TownSwatch.Moss, TownSwatch.MurkWater, 0.01f, 0.07f));
            Add(ProfundoStyle(OverworldTerrain.Sluices, hummocks, 0, 3.5f, 170f, 0.06f, TownSwatch.Mud, oil, copper, TownSwatch.MurkWater, abyss, TownSwatch.Moss, TownSwatch.ReedMat, oil, abyss,
                0.008f, TownSwatch.Mud, 0.016f, new[] { OverworldDecor.SluiceGate, OverworldDecor.SluiceGate, OverworldDecor.DrownedColumn }, 0.08f, TownSwatch.MurkWater, copper, 0.03f, 0.05f));

            // 88: the Pit of Darkness.
            Add(ProfundoStyle(OverworldTerrain.DarknessPit, labyrinth, 0, 6.5f, 200f, 0.05f, abyss, TownSwatch.Black, amber, indigo, abyss, TownSwatch.ElamGold, TownSwatch.ElamGold, TownSwatch.Black, abyss,
                0.01f, TownSwatch.Black, 0.016f, new[] { OverworldDecor.BlindStalagmite, OverworldDecor.BlindStalagmite, OverworldDecor.Basalt }, 0f, abyss, amber, 0.02f));
            Add(ProfundoStyle(OverworldTerrain.BlindTunnels, labyrinth, 0, 7f, 170f, 0.05f, TownSwatch.Black, abyss, TownSwatch.ElamGold, indigo, abyss, indigo, TownSwatch.ElamGold, TownSwatch.Black, abyss,
                0.012f, indigo, 0.018f, new[] { OverworldDecor.BlindStalagmite, OverworldDecor.BlindStalagmite, OverworldDecor.Stump }, 0f, indigo, TownSwatch.ElamGold, 0.02f));

            // 89: the Bridge of the Bōlos, along one causeway.
            Add(ProfundoStyle(OverworldTerrain.BridgeHead, causeway, 0, 9f, 220f, 0.02f, indigo, abyss, TownSwatch.KemetGold, TownSwatch.Black, abyss, TownSwatch.KemetGold, TownSwatch.KemetGold, indigo, abyss,
                0.008f, indigo, 0.012f, new[] { OverworldDecor.SealSlab, OverworldDecor.Basalt }, 0f, indigo, TownSwatch.KemetGold, 0.02f));
            Add(ProfundoStyle(OverworldTerrain.NoveloBridge, causeway, 0, 9f, 220f, 0.02f, abyss, TownSwatch.Black, TownSwatch.MoonLilac, indigo, abyss, TownSwatch.KemetGold, TownSwatch.KemetGold, TownSwatch.Black, abyss,
                0.006f, abyss, 0.01f, new[] { OverworldDecor.LampPedestal, OverworldDecor.SealSlab }, 0f, indigo, TownSwatch.MoonLilac, 0.02f));
            Add(ProfundoStyle(OverworldTerrain.NoveloGlobeFlights, causeway, 0, 9f, 220f, 0.02f, TownSwatch.Black, indigo, TownSwatch.MoonLilac, abyss, abyss, TownSwatch.MoonLilac, TownSwatch.KemetGold, abyss, abyss,
                0.006f, TownSwatch.Black, 0.012f, new[] { OverworldDecor.BlindStalagmite, OverworldDecor.SealSlab }, 0f, indigo, TownSwatch.MoonLilac, 0.02f));

            // 90: the Gate of the Land of Darkness.
            Add(ProfundoStyle(OverworldTerrain.PedestalPlaza, terraced, 2, 3f, 240f, 0.04f, basalt, abyss, TownSwatch.KemetGold, indigo, abyss, TownSwatch.KemetGold, TownSwatch.KemetGold, indigo, abyss,
                0.006f, basalt, 0.016f, new[] { OverworldDecor.LampPedestal, OverworldDecor.LampPedestal, OverworldDecor.SealSlab }, 0f, dust, amber, 0.02f));
            Add(ProfundoStyle(OverworldTerrain.DarkGateGround, hummocks, 0, 5f, 200f, 0.08f, abyss, TownSwatch.Black, TownSwatch.Ember, basalt, abyss, basalt, TownSwatch.KemetGold, TownSwatch.Black, abyss,
                0.016f, TownSwatch.Black, 0.016f, new[] { OverworldDecor.SealSlab, OverworldDecor.SealSlab, OverworldDecor.BlindStalagmite }, 0f, indigo, TownSwatch.Ember, 0.04f));
        }
    }
}
