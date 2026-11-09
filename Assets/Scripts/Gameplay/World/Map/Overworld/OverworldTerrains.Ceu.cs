using Rapadura.Gameplay.World.Cities.Town;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 16 — Reinos do Céu: the look of its terrains (310–349, Docs/Mapas100.md §5 região 16, maps 91–100): white <c>#FAF7EF</c>, gold <c>#E8C35A</c>, lilac <c>#C7B6EE</c>,
    /// sky blue <c>#BFD9F2</c> and the five colours of the elements on firm cloud round the Cais; the gold and bank-cloud of the river of light; silver and lilac on the deck of the Barca da Lua;
    /// bronze and water of light on the Wheel; crystal and gold in the orbits; white, gold, pink and pale green in the Garden; gold and warm white on the deck of the Barca do Sol; the white and
    /// five colours of the Column's steps; gold with black cracks at the Lookout; apricot adobe, gold and lilac at the Porch. Every area has its own ground, relief and pieces.
    /// </summary>
    public static partial class OverworldTerrains
    {
        /// <summary>A Reinos do Céu terrain (Região 16, 310–349).</summary>
        public static bool IsCeu(OverworldTerrain terrain)
        {
            int i = (int)terrain;
            return i >= 310 && i < 350;
        }

        /// <summary>A terrain of the deck of the Barca do Sol (its keel runs on a different row than the Barca da Lua's).</summary>
        public static bool IsSunDeck(OverworldTerrain terrain) => terrain == OverworldTerrain.SunDeck || terrain == OverworldTerrain.SunSailDeck;

        private static OverworldTerrainStyle CeuStyle(OverworldTerrain terrain, ReliefShape shape, int steps, float relief, float scale, float detail,
            TownSwatch ground, TownSwatch alt, TownSwatch accent, TownSwatch mottle, TownSwatch trail, TownSwatch clearing,
            float rocks, TownSwatch rockTint, float specials, OverworldDecor[] mix, float tufts, TownSwatch tuft, float pebbles, float puddles = 0f)
        {
            return new OverworldTerrainStyle
            {
                Terrain = terrain, Shape = shape, TerraceSteps = steps, Relief = relief, ReliefScale = scale, ReliefDetail = detail,
                Base = ground, Alt = alt, Accent = accent, Mottle = mottle, Hollow = TownSwatch.QuayCloudShade, Crest = accent,
                Patches = new[] { alt, mottle, accent }, AltThreshold = 0.5f, AccentThreshold = 0.93f,
                Trail = trail, Clearing = clearing, Cliff = TownSwatch.QuayCloudShade, Slope = ground, Bank = TownSwatch.QuayCloudShade, Bed = TownSwatch.SkyBelow,
                Vegetation = TownVegetation.Mountain, TreeVariants = new[] { 2 }, Trees = 0f, Bushes = 0f, Rocks = rocks, RockTint = rockTint,
                Specials = specials, SpecialMix = mix, Tufts = tufts, TuftColor = tuft, TuftTip = accent, Pebbles = pebbles, Puddles = puddles,
                WaterVegetation = TownVegetation.Mountain, WaterVariants = new[] { 2 },
            };
        }

        private static void CeuStyles(OverworldTerrainStyle[] styles)
        {
            void Add(OverworldTerrainStyle s) => styles[(int)s.Terrain] = s;
            const ReliefShape rolling = ReliefShape.Rolling;
            const ReliefShape terraced = ReliefShape.Terraced;
            const ReliefShape hummocks = ReliefShape.Hummocks;
            const ReliefShape deck = ReliefShape.DeckTiers;
            const ReliefShape wheel = ReliefShape.WheelRim;
            const ReliefShape orbits = ReliefShape.OrbitRidges;
            const ReliefShape rings = ReliefShape.ColumnRings;
            const ReliefShape cracks = ReliefShape.SkyFissures;
            TownSwatch cloud = TownSwatch.QuayCloud;
            TownSwatch shade = TownSwatch.QuayCloudShade;
            TownSwatch gold = TownSwatch.QuayGold;
            TownSwatch lilac = TownSwatch.QuayLilac;
            TownSwatch sky = TownSwatch.QuaySkyBlue;
            TownSwatch glass = TownSwatch.QuayGlassPane;
            TownSwatch light = TownSwatch.SolidLight;
            TownSwatch silver = TownSwatch.MoonSilver;
            TownSwatch bronze = TownSwatch.Bronze;
            TownSwatch fire = TownSwatch.ElementFire;
            TownSwatch water = TownSwatch.ElementWater;
            TownSwatch air = TownSwatch.ElementAir;

            // 91: the Cais and its piers.
            Add(CeuStyle(OverworldTerrain.QuayRimWest, hummocks, 0, 4f, 200f, 0.08f, cloud, shade, sky, glass, light, shade, 0.012f, shade, 0.014f, new[] { OverworldDecor.CloudBank, OverworldDecor.LightBollard }, 0.04f, shade, 0.05f));
            Add(CeuStyle(OverworldTerrain.MoonPierApproach, rolling, 0, 2.5f, 230f, 0.05f, shade, cloud, lilac, silver, light, cloud, 0.008f, shade, 0.016f, new[] { OverworldDecor.LightBollard, OverworldDecor.CloudBank }, 0.03f, lilac, 0.04f));
            Add(CeuStyle(OverworldTerrain.ColumnQuayGround, rolling, 0, 2f, 260f, 0.04f, cloud, glass, gold, shade, light, glass, 0.006f, cloud, 0.012f, new[] { OverworldDecor.LightBollard, OverworldDecor.CloudBank, OverworldDecor.CloudBank }, 0.02f, gold, 0.03f));
            Add(CeuStyle(OverworldTerrain.SunPierApproach, rolling, 0, 2.5f, 230f, 0.05f, cloud, gold, fire, shade, light, gold, 0.008f, gold, 0.016f, new[] { OverworldDecor.LightBollard, OverworldDecor.CloudBank }, 0.03f, gold, 0.04f));
            Add(CeuStyle(OverworldTerrain.CityLightsGap, hummocks, 0, 3f, 190f, 0.06f, cloud, sky, TownSwatch.IslandSpark, TownSwatch.SkyBelow, light, sky, 0.01f, shade, 0.012f, new[] { OverworldDecor.CloudIslet, OverworldDecor.CloudBank }, 0.03f, sky, 0.05f));

            // 92: the Rio de Luz.
            Add(CeuStyle(OverworldTerrain.RiverMouthWest, rolling, 0, 2f, 240f, 0.05f, sky, glass, gold, cloud, gold, sky, 0.006f, sky, 0.014f, new[] { OverworldDecor.LightBuoy, OverworldDecor.CloudIslet }, 0.05f, sky, 0.03f, 0.05f));
            Add(CeuStyle(OverworldTerrain.CloudIslets, hummocks, 0, 3.5f, 170f, 0.1f, cloud, sky, gold, shade, gold, cloud, 0.01f, shade, 0.016f, new[] { OverworldDecor.CloudIslet, OverworldDecor.CloudIslet, OverworldDecor.LightBuoy }, 0.04f, gold, 0.05f, 0.04f));
            Add(CeuStyle(OverworldTerrain.LightRiver, rolling, 0, 1.6f, 260f, 0.04f, gold, TownSwatch.ElementLight, light, sky, gold, gold, 0.004f, gold, 0.016f, new[] { OverworldDecor.LightBuoy, OverworldDecor.LightBuoy, OverworldDecor.CloudBank }, 0.02f, gold, 0.02f, 0.06f));
            Add(CeuStyle(OverworldTerrain.CurrentBanks, terraced, 2, 2.5f, 210f, 0.06f, sky, cloud, gold, glass, gold, sky, 0.008f, shade, 0.016f, new[] { OverworldDecor.LightBuoy, OverworldDecor.LightBollard }, 0.04f, sky, 0.04f, 0.05f));
            Add(CeuStyle(OverworldTerrain.RiverMouthEast, rolling, 0, 2f, 240f, 0.05f, glass, sky, gold, cloud, gold, glass, 0.006f, glass, 0.014f, new[] { OverworldDecor.CloudBank, OverworldDecor.CloudIslet }, 0.05f, glass, 0.03f, 0.05f));

            // 93: the deck of the Barca da Lua, three levels along its keel.
            Add(CeuStyle(OverworldTerrain.MoonProw, deck, 0, 5f, 230f, 0.04f, silver, lilac, gold, shade, lilac, silver, 0.006f, silver, 0.016f, new[] { OverworldDecor.MehenRail, OverworldDecor.DeckCrate }, 0.02f, lilac, 0.02f));
            Add(CeuStyle(OverworldTerrain.MoonMastDeck, deck, 0, 5.5f, 230f, 0.04f, lilac, silver, gold, shade, silver, lilac, 0.004f, lilac, 0.02f, new[] { OverworldDecor.MehenRail, OverworldDecor.MehenRail, OverworldDecor.DeckCrate }, 0.02f, silver, 0.02f));
            Add(CeuStyle(OverworldTerrain.MoonDeck, deck, 0, 6f, 230f, 0.04f, silver, shade, lilac, glass, lilac, silver, 0.004f, silver, 0.014f, new[] { OverworldDecor.DeckCrate, OverworldDecor.MehenRail }, 0.01f, lilac, 0.02f));
            Add(CeuStyle(OverworldTerrain.MoonHold, deck, 0, 5f, 200f, 0.05f, TownSwatch.AbyssIndigo, silver, lilac, shade, lilac, TownSwatch.AbyssIndigo, 0.01f, shade, 0.018f, new[] { OverworldDecor.DeckCrate, OverworldDecor.DeckCrate, OverworldDecor.MehenRail }, 0.01f, lilac, 0.05f));
            Add(CeuStyle(OverworldTerrain.MoonStern, deck, 0, 5f, 230f, 0.04f, silver, lilac, sky, shade, silver, silver, 0.006f, silver, 0.016f, new[] { OverworldDecor.MehenRail, OverworldDecor.LightBollard }, 0.02f, sky, 0.02f));

            // 94: the Celestial Wheel of the Twelve Jars.
            Add(CeuStyle(OverworldTerrain.GearDrift, hummocks, 0, 3f, 190f, 0.08f, bronze, cloud, gold, shade, gold, bronze, 0.012f, bronze, 0.018f, new[] { OverworldDecor.BronzeGear, OverworldDecor.BronzeGear, OverworldDecor.CloudBank }, 0.02f, bronze, 0.05f));
            Add(CeuStyle(OverworldTerrain.JarRimWest, wheel, 0, 9f, 240f, 0.03f, bronze, gold, water, cloud, gold, bronze, 0.008f, bronze, 0.014f, new[] { OverworldDecor.BronzeGear, OverworldDecor.CloudBank }, 0.02f, water, 0.03f));
            Add(CeuStyle(OverworldTerrain.WheelHub, wheel, 0, 9f, 240f, 0.03f, gold, bronze, water, cloud, gold, gold, 0.004f, gold, 0.012f, new[] { OverworldDecor.BronzeGear, OverworldDecor.LightBollard }, 0.01f, water, 0.02f));
            Add(CeuStyle(OverworldTerrain.JarRimEast, wheel, 0, 9f, 240f, 0.03f, bronze, water, gold, cloud, gold, bronze, 0.008f, bronze, 0.014f, new[] { OverworldDecor.BronzeGear, OverworldDecor.BronzeGear, OverworldDecor.LightBollard }, 0.02f, gold, 0.03f));
            Add(CeuStyle(OverworldTerrain.GearDriftEast, hummocks, 0, 3f, 190f, 0.08f, cloud, bronze, water, shade, gold, cloud, 0.012f, bronze, 0.018f, new[] { OverworldDecor.BronzeGear, OverworldDecor.CloudIslet }, 0.02f, cloud, 0.05f));

            // 95: the Spheres of the Archons.
            Add(CeuStyle(OverworldTerrain.OrbitVoidWest, hummocks, 0, 4f, 200f, 0.1f, glass, sky, TownSwatch.ElementAir, shade, gold, glass, 0.016f, glass, 0.016f, new[] { OverworldDecor.OrbitShard, OverworldDecor.CloudIslet }, 0.01f, air, 0.05f));
            Add(CeuStyle(OverworldTerrain.OrbitRidgeWest, orbits, 0, 7f, 230f, 0.05f, glass, gold, air, cloud, gold, glass, 0.008f, glass, 0.018f, new[] { OverworldDecor.OrbitShard, OverworldDecor.OrbitShard }, 0.02f, air, 0.03f));
            Add(CeuStyle(OverworldTerrain.StillIslet, orbits, 0, 7f, 230f, 0.05f, cloud, glass, gold, shade, gold, cloud, 0.006f, cloud, 0.014f, new[] { OverworldDecor.OrbitShard, OverworldDecor.CloudBank }, 0.02f, gold, 0.02f));
            Add(CeuStyle(OverworldTerrain.OrbitRidgeEast, orbits, 0, 7f, 230f, 0.05f, sky, gold, air, cloud, gold, sky, 0.008f, sky, 0.018f, new[] { OverworldDecor.OrbitShard, OverworldDecor.OrbitShard, OverworldDecor.CloudIslet }, 0.02f, sky, 0.03f));
            Add(CeuStyle(OverworldTerrain.OrbitVoidEast, hummocks, 0, 4f, 200f, 0.1f, sky, glass, gold, shade, gold, sky, 0.016f, sky, 0.016f, new[] { OverworldDecor.OrbitShard, OverworldDecor.CloudBank }, 0.01f, gold, 0.05f));

            // 96: the Garden of Light.
            Add(CeuStyle(OverworldTerrain.FlowerGarden, rolling, 0, 2f, 200f, 0.08f, TownSwatch.Grass, cloud, TownSwatch.ElementAir, gold, cloud, TownSwatch.Grass, 0.002f, cloud, 0.02f, new[] { OverworldDecor.SongFlower, OverworldDecor.SongFlower, OverworldDecor.LightTree }, 0.2f, TownSwatch.Grass, 0.01f));
            Add(CeuStyle(OverworldTerrain.LightOrchard, rolling, 0, 2f, 200f, 0.08f, cloud, TownSwatch.Grass, gold, air, cloud, cloud, 0.002f, gold, 0.018f, new[] { OverworldDecor.LightTree, OverworldDecor.LightTree, OverworldDecor.GardenFount }, 0.12f, gold, 0.01f));
            Add(CeuStyle(OverworldTerrain.GardenPavilion, terraced, 2, 2.5f, 210f, 0.05f, TownSwatch.Grass, gold, cloud, air, gold, cloud, 0.002f, cloud, 0.016f, new[] { OverworldDecor.GardenFount, OverworldDecor.LightTree }, 0.1f, TownSwatch.Grass, 0.02f));

            // 97: the deck of the Barca do Sol.
            Add(CeuStyle(OverworldTerrain.SunDeck, deck, 0, 6f, 230f, 0.04f, gold, fire, cloud, TownSwatch.ElementLight, fire, gold, 0.004f, gold, 0.016f, new[] { OverworldDecor.SunCoil, OverworldDecor.DeckCrate }, 0.01f, fire, 0.02f));
            Add(CeuStyle(OverworldTerrain.SunSailDeck, deck, 0, 6f, 230f, 0.04f, cloud, gold, fire, shade, fire, cloud, 0.004f, gold, 0.018f, new[] { OverworldDecor.SunCoil, OverworldDecor.SunCoil, OverworldDecor.LightBollard }, 0.01f, gold, 0.02f));

            // 98: the Foot of the Column.
            Add(CeuStyle(OverworldTerrain.ColumnStepsWest, rings, 0, 22f, 230f, 0.03f, cloud, TownSwatch.ElementLight, air, shade, light, cloud, 0.006f, cloud, 0.016f, new[] { OverworldDecor.CloudSteps, OverworldDecor.HoldPillar }, 0.01f, air, 0.02f));
            Add(CeuStyle(OverworldTerrain.ColumnFoot, rings, 0, 22f, 230f, 0.03f, TownSwatch.ColumnLight, cloud, fire, shade, light, TownSwatch.ColumnLight, 0.004f, cloud, 0.018f, new[] { OverworldDecor.CloudSteps, OverworldDecor.CloudSteps }, 0.01f, fire, 0.02f));

            // 99: the Lookout of the Sky-Holder.
            Add(CeuStyle(OverworldTerrain.SkyCracksWest, cracks, 0, 8f, 200f, 0.05f, gold, TownSwatch.AbyssIndigo, cloud, shade, gold, gold, 0.012f, TownSwatch.AbyssIndigo, 0.016f, new[] { OverworldDecor.HoldPillar, OverworldDecor.CloudBank }, 0.01f, gold, 0.04f));
            Add(CeuStyle(OverworldTerrain.SkyHolderLookout, cracks, 0, 8f, 200f, 0.05f, cloud, gold, TownSwatch.AbyssIndigo, shade, gold, cloud, 0.012f, shade, 0.016f, new[] { OverworldDecor.HoldPillar, OverworldDecor.CloudIslet }, 0.01f, cloud, 0.04f));

            // 100: the Porch of the Time After.
            Add(CeuStyle(OverworldTerrain.TimeAfterPorch, rolling, 0, 1.5f, 260f, 0.04f, TownSwatch.AdobeOchre, gold, lilac, cloud, gold, TownSwatch.AdobeOchre, 0.002f, TownSwatch.AdobeOchre, 0.014f, new[] { OverworldDecor.PorchBench, OverworldDecor.LightTree }, 0.06f, lilac, 0.01f));
        }
    }
}
