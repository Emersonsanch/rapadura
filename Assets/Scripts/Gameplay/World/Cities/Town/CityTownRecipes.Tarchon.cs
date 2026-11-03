using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Palafitas de Tarchon, race city of the trols (Docs/Mapas100.md §3.3 and map 33; return city of maps 33–34), in its
    /// own style (<see cref="TownStyles.StiltVillage"/>): long houses on tall piles with their gable to the street and leaf
    /// roofs down to the floor, round a plaza of wet planks with the Mastro dos Fogos-Fátuos (the wisps set free light the
    /// city). Old Tarchon's great long house closes the head of the plaza (the missions); Larthia's Cestaria (the city's shop:
    /// the reed boat of 33 and the antivenom amulet of 34) and the Cais da Barca on the West street, the road in along the
    /// delta; Ramtha the healer and Thanchvil the storyteller on the East street; Vel the fisher on the South street, with his
    /// net racks; plank boardwalks over the bog channels crossing the East and South streets; giant baskets, floating reed
    /// islands and mangroves between the streets; no street to the north: behind Tarchon's house the rail of piles opens on
    /// the open swamp. Everything stays inside a circle of radius <see cref="TarchonSolidRadius"/> at y = 0 (the place the
    /// delta overworld keeps for it), with three roads out (W, E, S) ending at ±<see cref="TarchonArmLength"/>.
    /// Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 33–34 (= <c>MapCatalog.PalafitasTarchon</c>, <see cref="CityCatalog.PalafitasTarchon"/>).</summary>
        public const string TarchonCityId = "palafitas_tarchon";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float TarchonArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float TarchonHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float TarchonSolidRadius = 22.5f;

        /// <summary>Where the Mastro dos Fogos-Fátuos stands (north of the plaza centre, in front of Tarchon's long house).</summary>
        public static readonly Vector2 TarchonWispMast = new Vector2(0f, 3.6f);

        public static CityTownRecipe Tarchon()
        {
            var r = new CityTownRecipe
            {
                CityId = TarchonCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.StiltVillage,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = TarchonHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, TarchonArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, TarchonArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, TarchonArmLength, exitToMap: true));

            // ---- Plaza head: old Tarchon's great long house (the missions) ----
            TownBuildingSpec great = LonghouseSpec(TownShopDressing.None, wallHeight: 3.2f);
            great.FullVaranda = true;
            great.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_de_tarchon", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = great, ResidentId = "tarchon",
            });

            // ---- West street (the road in): Larthia's Cestaria (the city's shop), the Cais da Barca; a plain house across ----
            TownBuildingSpec cestaria = LonghouseSpec(TownShopDressing.BasketWeaver, door: 0.18f, wallHeight: 2.9f);
            cestaria.SignColor = TownSwatch.WillWisp;
            r.Lots.Add(Street("cestaria_larthia", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "larthia", CityShopCatalog.CestariaLarthia, cestaria));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, LonghouseSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street: Ramtha the healer, Thanchvil the storyteller ----
            r.Lots.Add(Street("casa_ramtha", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "ramtha", null,
                LonghouseSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.8f)));
            r.Lots.Add(Street("casa_thanchvil", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "thanchvil", null,
                LonghouseSpec(TownShopDressing.None, wallHeight: 2.6f)));

            // ---- South street: Vel the fisher; a plain house across ----
            r.Lots.Add(Street("casa_vel", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "vel", null,
                LonghouseSpec(TownShopDressing.None, door: 0.15f, wallHeight: 2.8f)));
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, LonghouseSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The plaza: the Mastro dos Fogos-Fátuos, lamps ----
            r.PlazaProps.Add(At(CityPropKind.WispMast, TarchonWispMast.x, TarchonWispMast.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Bog channels crossing the East and South streets, each with its plank boardwalk ----
            r.PlazaProps.Add(Segment(CityPropKind.Boardwalk, 15.8f, -2.4f, 15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.Boardwalk, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: giant baskets, Vel's net racks, floating reed islands, mangroves ----
            r.PlazaProps.Add(At(CityPropKind.GiantBasket, -9.5f, 12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.GiantBasket, 9.2f, 12f, 0f, 1.1f, 2));
            r.PlazaProps.Add(At(CityPropKind.GiantBasket, 11.5f, -4.2f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.NetRack, 6.5f, -17.4f));
            r.PlazaProps.Add(At(CityPropKind.NetRack, -14f, 12.2f));
            r.PlazaProps.Add(At(CityPropKind.ReedIsland, 13f, -9f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.ReedIsland, -14.5f, -12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Mangrove, -18f, -5.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Mangrove, 16f, -13.5f, 0f, 0.9f, 1));

            // ---- The open swamp: no street to the north, the rail of piles behind Tarchon's house in three runs ----
            r.PlazaProps.Add(Segment(CityPropKind.SwampEdge, -8f, 19.5f, 8f, 19.5f));
            r.PlazaProps.Add(Segment(CityPropKind.SwampEdge, -15.5f, 14f, -10.5f, 14f));
            r.PlazaProps.Add(Segment(CityPropKind.SwampEdge, 10.5f, 14f, 15.5f, 14f));
            return r;
        }

        /// <summary>A trol long house of Tarchon: rush walls, moss-blue bands, wet-wood joinery and piles, a steep leaf roof.</summary>
        private static TownBuildingSpec LonghouseSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.8f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.Rush, TownSwatch.TrollMoss, TownSwatch.WetWood, TownSwatch.WillWisp, roof: TownRoofKind.Gable, dressing: dressing,
                sign: TownSwatch.WillWisp, door: door, wallHeight: wallHeight, windows: 1, roofColor: TownSwatch.SwampLeaf);
            spec.StiltLonghouse = true;
            spec.PlinthColor = TownSwatch.BogWater;
            return spec;
        }
    }
}
