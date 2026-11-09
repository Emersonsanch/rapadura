using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Dur-Jarras, the ziggurat city of the Margens da Mistura (Docs/Mapas100.md map 77, região 14; Akkadian <i>dūr</i>, fortress,
    /// an echo of Dur-Untash at Chogha Zanbil), in its own style (<see cref="TownStyles.Ziggurat"/>): a brick town round the plaza
    /// at the foot of the ziggurat of five steps, which closes its north side (no street there). The Relógio de Zurvan stands on the
    /// plaza, kept by Utnapishtim, whose house is on the East street with Siduri's tavern beyond it; Atrahasis' bitumen yard (the
    /// city's shop: the gear of maps 77–82) and the raft landing (Cais das Balsas) on the West street, the road to the Poços de
    /// Betume; Sin-leqi-unninni's house of tablets on the South street, the road in from the Deserto do Relógio; two bitumen canals
    /// (with brick bridges) cross the West and South streets; date palms, glazed lions and stacks of jars between the streets.
    /// Everything stays inside a circle of radius <see cref="DurJarrasSolidRadius"/> at y = 0 (the place the overworld will keep
    /// for it), with three roads out (W, E, S) ending at ±<see cref="DurJarrasArmLength"/>. Same conventions as the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of maps 77–82 (= <see cref="CityCatalog.DurJarras"/>; MapCatalog will use the same id).</summary>
        public const string DurJarrasCityId = "dur_jarras";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float DurJarrasArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float DurJarrasHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float DurJarrasSolidRadius = 22.5f;

        /// <summary>Where the ziggurat stands, at the head of the plaza (just outside its circle).</summary>
        public static readonly Vector2 DurJarrasZiggurat = new Vector2(0f, 13.2f);

        /// <summary>Where the Relógio de Zurvan stands on the plaza (north of the centre, clear of the respawn).</summary>
        public static readonly Vector2 DurJarrasClock = new Vector2(0f, 4.2f);

        public static CityTownRecipe DurJarras()
        {
            var r = new CityTownRecipe
            {
                CityId = DurJarrasCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.Ziggurat,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = DurJarrasHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 10,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, DurJarrasArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, DurJarrasArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, DurJarrasArmLength, exitToMap: true));

            // ---- West street (to the Poços de Betume): Atrahasis' bitumen yard (the city's shop), the raft landing; a brick house across ----
            TownBuildingSpec yard = BrickSpec(TownShopDressing.BitumenYard, door: 0.18f, wallHeight: 2.9f);
            yard.SignColor = TownSwatch.ElamGold;
            r.Lots.Add(Street("patio_do_atrahasis", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "atrahasis", CityShopCatalog.BetumeAtrahasis, yard));
            r.Lots.Add(Street("cais_das_balsas", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, BrickSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street (to the Cedral de Hobabish): Utnapishtim's house (the missions), Siduri's tavern beyond it ----
            r.Lots.Add(Street("casa_de_utnapishtim", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "utnapishtim", null,
                BrickSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.9f)));
            TownBuildingSpec tavern = BrickSpec(TownShopDressing.None, wallHeight: 2.6f);
            tavern.WallColor = TownSwatch.DustSky;
            tavern.TrimColor = TownSwatch.Bitumen;
            r.Lots.Add(Street("taberna_de_siduri", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "siduri", null, tavern));

            // ---- South street (the road in from the Deserto do Relógio): the house of tablets of Sin-leqi-unninni; a brick house across ----
            r.Lots.Add(Street("casa_das_tabuas", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "sin_leqi_unninni", null,
                BrickSpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.0f, wide: true)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, BrickSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The ziggurat of five steps at the head of the plaza, the Relógio de Zurvan in front of it, lamp posts round the plaza ----
            r.PlazaProps.Add(At(CityPropKind.Ziggurat, DurJarrasZiggurat.x, DurJarrasZiggurat.y));
            r.PlazaProps.Add(At(CityPropKind.ZurvanClock, DurJarrasClock.x, DurJarrasClock.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Two canals of black water crossing the West and South streets, each with its brick bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.BitumenCanal, -15.8f, -2.4f, -15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.BitumenCanal, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: date palms, the glazed lions of the walls, stacks of jars ----
            r.PlazaProps.Add(At(CityPropKind.DatePalm, -9.5f, 12.5f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.DatePalm, 9.5f, 12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.GlazedLion, 13f, -6f, 180f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.JarStack, 13.5f, -11f, 30f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.DatePalm, 16.5f, -6.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.GlazedLion, -14.5f, -12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.DatePalm, -18f, -5.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.JarStack, -11f, -12.5f, -20f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.DatePalm, 7.5f, -17.5f, 0f, 0.9f, 0));

            // ---- No street to the north: palms, lions and jars round the back of the ziggurat ----
            r.PlazaProps.Add(At(CityPropKind.DatePalm, -7.5f, 19.2f, 0f, 1.2f, 1));
            r.PlazaProps.Add(At(CityPropKind.DatePalm, 7.5f, 19.2f, 0f, 1.2f, 0));
            r.PlazaProps.Add(At(CityPropKind.JarStack, 0f, 19.8f, 0f, 1.2f, 1));
            r.PlazaProps.Add(At(CityPropKind.GlazedLion, -7f, 8.9f, 90f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.GlazedLion, 7f, 8.9f, -90f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.JarStack, -13f, 14f, 60f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.JarStack, 13f, 14f, -60f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.DatePalm, -16f, 11.5f, 0f, 0.8f, 0));
            r.PlazaProps.Add(At(CityPropKind.DatePalm, 16f, 11.5f, 0f, 0.8f, 1));
            return r;
        }

        /// <summary>A house of Dur-Jarras: baked brick with buttress niches, a glazed blue band, a stepped parapet and a flat roof of tarred reed.</summary>
        private static TownBuildingSpec BrickSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.9f, bool wide = false)
        {
            TownBuildingSpec spec = Spec(TownSwatch.BakedBrick, TownSwatch.GlazedBlue, TownSwatch.Bitumen, TownSwatch.ElamGold, dressing: dressing,
                sign: TownSwatch.GlazedBlue, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.Bitumen);
            spec.ZigguratHall = true;
            spec.WideDoor = wide;
            spec.PlinthColor = TownSwatch.Bitumen;
            return spec;
        }
    }
}
