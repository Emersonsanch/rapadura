using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Porto da Lua Branca, the capital of the Mar Gelado (Docs/Mapas100.md map 59; the Ar Sereno, Lavine's element), in its own
    /// style (<see cref="TownStyles.IceCanal"/>): a harbour on islands of ice joined by canals of black water and stone bridges of
    /// whale-rib arches, round the plaza on the largest island where the Cais da Lua stands (the quay where the Moon Ship moors
    /// on the full moon). Yirkap's long plank house, the ski hunter (the missions), closes the head of the plaza; Pamour's furs
    /// and boats (the city's shop: the gear of the gates of maps 59–64) and the Cais da Barca on the West street, the fishing
    /// quarter, with fish racks and boats drawn up on the ice; Makarios, who keeps the moon lanterns, and a snow dome on the
    /// East street, the quarter of the lanterns; Pera, the good giant who breaks the ice for the boats (the second mission), on
    /// the South street, the road in, the quarter of the furs; canals with whale-rib bridges crossing the West and South
    /// streets; ridges of sea ice, snow domes and crescent boats between the streets; no street to the north: there is the bay,
    /// frozen, with boats and ice. Everything stays inside a circle of radius <see cref="PortoDaLuaBrancaSolidRadius"/> at
    /// y = 0 (the place the Mar Gelado overworld keeps for it), with three roads out (W, E, S) ending at
    /// ±<see cref="PortoDaLuaBrancaArmLength"/>. Same conventions as the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of maps 59–64 (= <c>MapCatalog.PortoDaLuaBranca</c>, <see cref="CityCatalog.PortoDaLuaBranca"/>).</summary>
        public const string PortoDaLuaBrancaCityId = "porto_da_lua_branca";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float PortoDaLuaBrancaArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float PortoDaLuaBrancaHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float PortoDaLuaBrancaSolidRadius = 22.5f;

        /// <summary>Where the Cais da Lua stands on the plaza (north of the centre, clear of the respawn).</summary>
        public static readonly Vector2 PortoDaLuaBrancaMoonQuay = new Vector2(0f, 4.2f);

        public static CityTownRecipe PortoDaLuaBranca()
        {
            var r = new CityTownRecipe
            {
                CityId = PortoDaLuaBrancaCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.IceCanal,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = PortoDaLuaBrancaHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 8,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, PortoDaLuaBrancaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, PortoDaLuaBrancaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, PortoDaLuaBrancaArmLength, exitToMap: true));

            // ---- Plaza head: Yirkap's long plank house, the ski hunter (the missions) ----
            TownBuildingSpec yirkap = MoonHouseSpec(TownShopDressing.None, wallHeight: 3.0f);
            yirkap.FullVaranda = true;
            yirkap.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_de_yirkap", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = yirkap, ResidentId = "yirkap",
            });

            // ---- West street (the fishing quarter, out to the Canais de Gelo): Pamour's furs and boats (the city's shop), the Cais da Barca; a house across ----
            TownBuildingSpec peles = MoonHouseSpec(TownShopDressing.FurAndBoat, door: 0.18f, wallHeight: 2.8f);
            peles.SignColor = TownSwatch.MoonLilac;
            r.Lots.Add(Street("peles_do_pamour", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "pamour", CityShopCatalog.PelesPamour, peles));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, MoonHouseSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street (the quarter of the lanterns): Makarios' house of moon lanterns; a snow dome ----
            r.Lots.Add(Street("casa_de_makarios", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "makarios", null,
                MoonHouseSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.9f)));
            r.Lots.Add(Street("domo_e2", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, null, null, DomeSpec()));

            // ---- South street (the road in, the quarter of the furs): Pera, the good giant (the second mission); a snow dome across ----
            r.Lots.Add(Street("casa_de_pera", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "pera", null,
                MoonHouseSpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.2f, wide: true)));
            r.Lots.Add(Street("domo_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, DomeSpec()));

            // ---- The plaza on the largest island: the Cais da Lua (north of the centre), lamps ----
            r.PlazaProps.Add(At(CityPropKind.MoonQuay, PortoDaLuaBrancaMoonQuay.x, PortoDaLuaBrancaMoonQuay.y, 0f, 1f));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Canals of black water crossing the West and South streets between the islands, each with its whale-rib bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.IceCanal, -15.8f, -2.4f, -15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.IceCanal, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: ridges of sea ice, the fishing racks and boats of the West, snow domes ----
            r.PlazaProps.Add(At(CityPropKind.IceBlock, -9.5f, 12.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.IceBlock, 9.5f, 12.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.CrescentBoat, 13f, -6f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.SnowDomeHut, 13.5f, -11f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.IceBlock, 16.5f, -6.5f, 0f, 0.8f, 0));
            r.PlazaProps.Add(At(CityPropKind.FishRack, -14.5f, -12.5f, 0f, 1f));
            r.PlazaProps.Add(At(CityPropKind.CrescentBoat, -18f, -5.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.FishRack, -11f, -12.5f, 0f, 1f));
            r.PlazaProps.Add(At(CityPropKind.IceBlock, 7.5f, -17.5f, 0f, 0.8f, 0));

            // ---- No street to the north: the frozen bay, boats drawn up and ridges of ice ----
            r.PlazaProps.Add(At(CityPropKind.IceBlock, -7.5f, 19.2f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.IceBlock, 7.5f, 19.2f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.IceBlock, 0f, 19.8f, 0f, 1.2f, 1));
            r.PlazaProps.Add(At(CityPropKind.CrescentBoat, -4f, 19.4f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.CrescentBoat, 4f, 19.4f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.SnowDomeHut, -13f, 14f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.SnowDomeHut, 13f, 14f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.IceBlock, -16f, 11.5f, 0f, 0.7f, 0));
            r.PlazaProps.Add(At(CityPropKind.IceBlock, 16f, 11.5f, 0f, 0.7f, 0));
            return r;
        }

        /// <summary>A long plank house of the Porto: dark plank walls, the lilac and silver moon on the gable, a snowy roof.</summary>
        private static TownBuildingSpec MoonHouseSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.9f, bool wide = false)
        {
            TownBuildingSpec spec = Spec(TownSwatch.MoonPlank, TownSwatch.MoonLilac, TownSwatch.MoonPlank, TownSwatch.MoonSilver, dressing: dressing,
                sign: TownSwatch.MoonSilver, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.Snowfield);
            spec.MoonLonghouse = true;
            spec.WideDoor = wide;
            spec.PlinthColor = TownSwatch.Snowfield;
            return spec;
        }

        /// <summary>A snow dome of the Porto: snow courses, an entrance tunnel to the street, a moon lantern by it.</summary>
        private static TownBuildingSpec DomeSpec()
        {
            TownBuildingSpec spec = Spec(TownSwatch.Snowfield, TownSwatch.MoonLilac, TownSwatch.MoonPlank, TownSwatch.MoonSilver,
                sign: TownSwatch.MoonSilver, wallHeight: 2.6f, windows: 0, roofColor: TownSwatch.Snowfield);
            spec.SnowDome = true;
            spec.PlinthColor = TownSwatch.SeaIce;
            return spec;
        }
    }
}
