using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Mingcheng, a Cidade dos Nove Anéis, capital of the Planície de Mingcheng (Docs/Mapas100.md §2.1 and map 41; return city
    /// of maps 41, 43, 44 and 46), in its own style (<see cref="TownStyles.RingWall"/>): courtyard halls of grey brick with
    /// red-lacquered columns and curved tile roofs round a flagstone plaza (the 6th ring) with the Torre do Tambor e do Sino in
    /// the middle (the city's clock: the gates open with the drum, close with the bell). Bögü's hall closes the head of the
    /// plaza (the old chief of the ring guard: the missions), right in front of the wall of the inner rings; Mōzak's lanterns
    /// (the city's shop: the Sun Seal of map 42) and the Cais da Barca on the West street, the road in along the river; Lin
    /// Deng, the master of the canal (second mission), and Fuduodan, who reads the Two Principles to whoever asks, on the East
    /// street; two halls on the South street, the way out to the rice terraces; canals crossing the East and South streets,
    /// each with its humpback bridge; willows and lantern frames of the Festa das Lanternas between the streets; no street to
    /// the north: there the ring wall of the 7th ring stands in three runs, the closed three-arch gate of the citadel (map 46)
    /// with its golden gate tower in the middle run and the golden roofs of the inner rings behind it. Everything stays inside a
    /// circle of radius <see cref="MingchengSolidRadius"/> at y = 0 (the place the Mingcheng overworld will keep for it), with
    /// three roads out (W, E, S) ending at ±<see cref="MingchengArmLength"/>.
    /// Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 41, 43, 44 and 46 (= <c>MapCatalog.Mingcheng</c>, <see cref="CityCatalog.Mingcheng"/>).</summary>
        public const string MingchengCityId = "mingcheng";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float MingchengArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float MingchengHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float MingchengSolidRadius = 22.5f;

        /// <summary>Where the Torre do Tambor e do Sino stands (north of the plaza centre, in front of Bögü's hall).</summary>
        public static readonly Vector2 MingchengBellDrumTower = new Vector2(0f, 3.6f);

        public static CityTownRecipe Mingcheng()
        {
            var r = new CityTownRecipe
            {
                CityId = MingchengCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.RingWall,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = MingchengHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, MingchengArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, MingchengArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, MingchengArmLength, exitToMap: true));

            // ---- Plaza head: Bögü's hall, the old chief of the ring guard (the missions), before the wall of the 7th ring ----
            TownBuildingSpec bogu = HallSpec(TownShopDressing.None, wallHeight: 3.0f);
            bogu.FullVaranda = true;
            bogu.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_de_bogu", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = bogu, ResidentId = "bogu",
            });

            // ---- West street (the river road in): Mōzak's lanterns (the city's shop), the Cais da Barca; a hall across ----
            TownBuildingSpec lanternas = HallSpec(TownShopDressing.LanternMaker, door: 0.18f, wallHeight: 2.9f);
            lanternas.SignColor = TownSwatch.GoldTile;
            r.Lots.Add(Street("lanternas_mozak", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "mozak", CityShopCatalog.LanternasMozak, lanternas));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, HallSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street: Lin Deng, the master of the canal (the second mission), Fuduodan the reader ----
            r.Lots.Add(Street("casa_lin_deng", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "lin_deng", null,
                HallSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.8f)));
            TownBuildingSpec fuduodan = HallSpec(TownShopDressing.None, wallHeight: 2.6f);
            fuduodan.AccentColor = TownSwatch.GoldTile;
            r.Lots.Add(Street("casa_fuduodan", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "fuduodan", null, fuduodan));

            // ---- South street (the way out to the rice terraces): two halls ----
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, null, null,
                HallSpec(TownShopDressing.None, door: 0.15f, wallHeight: 2.8f)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, HallSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The plaza (the 6th ring): the Torre do Tambor e do Sino, lamps ----
            r.PlazaProps.Add(At(CityPropKind.BellDrumTower, MingchengBellDrumTower.x, MingchengBellDrumTower.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Canals crossing the East and South streets, each with its humpback bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.CanalBridge, 15.8f, -2.4f, 15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.CanalBridge, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: willows (two with a lantern), lantern frames of the Festa das Lanternas ----
            r.PlazaProps.Add(At(CityPropKind.Willow, -9.5f, 12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Willow, 9.2f, 12f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Willow, 11.5f, -4.4f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.Willow, 13f, -9f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Willow, -14.5f, -12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Willow, -18f, -5.5f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.Willow, 16f, -13.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.LanternFrame, 6.5f, -17.4f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.LanternFrame, -11f, -12.5f, 20f, 1f, 0));

            // ---- The wall of the 7th ring: no street to the north; three runs (the closed citadel gate on the middle one) ----
            r.PlazaProps.Add(Segment(CityPropKind.RingWall, -8f, 19.5f, 8f, 19.5f, 1f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.RingWall, -15.5f, 14f, -10.5f, 14f));
            r.PlazaProps.Add(Segment(CityPropKind.RingWall, 10.5f, 14f, 15.5f, 14f));

            // ---- Lantern frames in front of the side runs ----
            r.PlazaProps.Add(At(CityPropKind.LanternFrame, -13f, 12.4f, CityGeometry.YawFacing(Vector2.down), 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.LanternFrame, 13f, 12.4f, CityGeometry.YawFacing(Vector2.down), 1f, 1));
            return r;
        }

        /// <summary>A courtyard hall of Mingcheng: grey brick, red-lacquered columns and joinery, a jade band, a curved grey-tile roof.</summary>
        private static TownBuildingSpec HallSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.8f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.GreyBrick, TownSwatch.Lacquer, TownSwatch.Lacquer, TownSwatch.Jade, roof: TownRoofKind.Gable, dressing: dressing,
                sign: TownSwatch.GoldTile, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.RoofTile);
            spec.RingWallHall = true;
            spec.PlinthColor = TownSwatch.StoneDark;
            return spec;
        }
    }
}
