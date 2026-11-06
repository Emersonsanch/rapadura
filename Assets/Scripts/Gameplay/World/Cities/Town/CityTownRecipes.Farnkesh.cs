using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Farnkesh, os Ninhos de Pedra, the capital of the Picos de Farnkesh (Docs/Mapas100.md map 54; the Vento Vivo, Maithe's
    /// element), in its own style (<see cref="TownStyles.PinnacleNest"/>): white nest-houses on sandstone pinnacles above the
    /// clouds round the plaza on the top of the largest pinnacle, where the giant wind bell hangs. Shad-Ohrmizd's house, the
    /// guardian of the wind bells (the missions), closes the head of the plaza; Sisin's rope rack (the city's shop: the gear of
    /// the gates 57 and 58) and the Cais da Barca on the West street; Tadi, who folds the wind paper (the second mission), on the
    /// East street; Ardawan, who braids the yak rope, on the South street, the road up from the Vale dos Sátiros; gaps of cloud
    /// with plank-and-rope bridges crossing the East and South streets; sandstone pinnacles and rope basket lifts between the
    /// streets; no street to the north: there the pinnacles stand in the clouds. Everything stays inside a circle of radius
    /// <see cref="FarnkeshSolidRadius"/> at y = 0 (the place the Picos de Farnkesh overworld keeps for it), with three roads
    /// out (W, E, S) ending at ±<see cref="FarnkeshArmLength"/>. Same conventions as the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of maps 54–58 (= <c>MapCatalog.Farnkesh</c>, <see cref="CityCatalog.Farnkesh"/>).</summary>
        public const string FarnkeshCityId = "farnkesh";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float FarnkeshArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float FarnkeshHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float FarnkeshSolidRadius = 22.5f;

        /// <summary>Where the giant wind bell stands on the plaza (north of the centre, clear of the respawn).</summary>
        public static readonly Vector2 FarnkeshWindBell = new Vector2(0f, 4.2f);

        public static CityTownRecipe Farnkesh()
        {
            var r = new CityTownRecipe
            {
                CityId = FarnkeshCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.PinnacleNest,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = FarnkeshHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 8,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, FarnkeshArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, FarnkeshArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, FarnkeshArmLength, exitToMap: true));

            // ---- Plaza head: Shad-Ohrmizd's house, the guardian of the wind bells (the missions) ----
            TownBuildingSpec shad = NestSpec(TownShopDressing.None, wallHeight: 3.6f);
            shad.FullVaranda = true;
            shad.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "ninho_de_shad_ohrmizd", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = shad, ResidentId = "shad_ohrmizd",
            });

            // ---- West street (the bridges out to the Pontes de Vento): Sisin's rope rack (the city's shop), the Cais da Barca; a house across ----
            TownBuildingSpec cordas = NestSpec(TownShopDressing.RopeWalker, door: 0.18f, wallHeight: 3.1f);
            cordas.SignColor = TownSwatch.NestGold;
            r.Lots.Add(Street("cordas_do_sisin", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "sisin", CityShopCatalog.CordasSisin, cordas));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("ninho_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, NestSpec(TownShopDressing.None, wallHeight: 2.9f)));

            // ---- East street: Tadi's house of wind paper (the second mission); a small nest ----
            r.Lots.Add(Street("ninho_de_tadi", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "tadi", null,
                NestSpec(TownShopDressing.None, door: -0.15f, wallHeight: 3.1f)));
            r.Lots.Add(Street("ninho_e2", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, null, null, NestSpec(TownShopDressing.None, wallHeight: 2.8f)));

            // ---- South street (the road up from the Vale): Ardawan, who braids the yak rope; a house across ----
            r.Lots.Add(Street("ninho_de_ardawan", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "ardawan", null,
                NestSpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.1f)));
            r.Lots.Add(Street("ninho_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, NestSpec(TownShopDressing.None, wallHeight: 2.9f)));

            // ---- The plaza on the largest pinnacle: the giant wind bell (north of the centre), lamps ----
            r.PlazaProps.Add(At(CityPropKind.WindBell, FarnkeshWindBell.x, FarnkeshWindBell.y, 0f, 1f));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Gaps of cloud crossing the East and South streets, each with its plank-and-rope bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.CloudGap, 15.8f, -2.4f, 15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.CloudGap, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: pinnacles (one with a small nest-house on top) and the rope basket lifts Sisin pulls ----
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, -9.5f, 12.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 9.5f, 12.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.RopeBasket, 13f, -6f, 0f, 1f));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 13.5f, -11f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 16.5f, -6.5f, 0f, 0.8f, 0));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, -14.5f, -12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, -18f, -5.5f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.RopeBasket, -11f, -12.5f, 0f, 1f));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 7.5f, -17.5f, 0f, 0.8f, 0));

            // ---- No street to the north: pinnacles standing in the clouds, two lifts ----
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, -7.5f, 19.2f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 7.5f, 19.2f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 0f, 19.8f, 0f, 1.2f, 1));
            r.PlazaProps.Add(At(CityPropKind.RopeBasket, -4f, 19.4f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.RopeBasket, 4f, 19.4f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, -13f, 14f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 13f, 14f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, -16f, 11.5f, 0f, 0.7f, 0));
            r.PlazaProps.Add(At(CityPropKind.StonePinnacle, 16f, 11.5f, 0f, 0.7f, 0));
            return r;
        }

        /// <summary>A nest-house of Farnkesh: lime walls, the painted red band and frames, a gold accent, a plank roof.</summary>
        private static TownBuildingSpec NestSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 3.1f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.NestLime, TownSwatch.NestRed, TownSwatch.NestRed, TownSwatch.NestGold, dressing: dressing,
                sign: TownSwatch.HighSky, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.WoodWeathered);
            spec.PinnacleNest = true;
            spec.PlinthColor = TownSwatch.PinnacleStone;
            return spec;
        }
    }
}
