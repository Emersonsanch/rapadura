using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Sangkent, a Cidade-Escadaria, capital of the Teto do Mundo (Docs/Mapas100.md §2.1 and map 36; return city of maps
    /// 36–40), in its own style (<see cref="TownStyles.StoneTerrace"/>): two-storey stone-and-loam houses laced with juniper
    /// round a flagstone plaza (the 4th of the 7 terraces) with the Marco das Caravanas in the middle (the five lights met at
    /// this crossroads of the caravans). Abā's house closes the head of the plaza (the chief of the chutes: the missions);
    /// Sisinnios's mountain gear (the city's shop: the thermal clothes of 36, the crampons of 38, the storm cloak of 40) and
    /// the Cais da Barca on the West street, the caravan road in; Kustai, who keeps the caravan notice board, and Baraies,
    /// the teacher who counts the maps, on the East street; two houses on the South street, the way down the valley;
    /// irrigation channels crossing the East and South streets, each with its stone slab; apricot trees in blossom and chute
    /// carts between the streets; no street to the north: there the dry-stone terraces step up (three steps seen from the
    /// plaza, the fort of the 7th terrace on the middle run) and two polished stone chutes come down them to their landings.
    /// Everything stays inside a circle of radius <see cref="SangkentSolidRadius"/> at y = 0 (the place the Teto do Mundo
    /// overworld will keep for it), with three roads out (W, E, S) ending at ±<see cref="SangkentArmLength"/>.
    /// Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 36–40 (= <c>MapCatalog.Sangkent</c>, <see cref="CityCatalog.Sangkent"/>).</summary>
        public const string SangkentCityId = "sangkent";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float SangkentArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float SangkentHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float SangkentSolidRadius = 22.5f;

        /// <summary>Where the Marco das Caravanas stands (north of the plaza centre, in front of Abā's house).</summary>
        public static readonly Vector2 SangkentCaravanStone = new Vector2(0f, 3.5f);

        public static CityTownRecipe Sangkent()
        {
            var r = new CityTownRecipe
            {
                CityId = SangkentCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.StoneTerrace,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = SangkentHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, SangkentArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, SangkentArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, SangkentArmLength, exitToMap: true));

            // ---- Plaza head: Abā's house, the chief of the chutes (the missions) ----
            TownBuildingSpec aba = TerraceSpec(TownShopDressing.None, TownSwatch.TerraceStone, wallHeight: 2.6f);
            aba.FullVaranda = true;
            aba.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_de_aba", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = aba, ResidentId = "aba",
            });

            // ---- West street (the caravan road in): Sisinnios's mountain gear (the city's shop), the Cais da Barca; a house across ----
            TownBuildingSpec agasalhos = TerraceSpec(TownShopDressing.Outfitter, TownSwatch.Loam, door: 0.18f, wallHeight: 2.5f);
            agasalhos.SignColor = TownSwatch.SilkBlue;
            r.Lots.Add(Street("agasalhos_sisinnios", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "sisinnios", CityShopCatalog.AgasalhosSisinnios, agasalhos));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, TerraceSpec(TownShopDressing.None, TownSwatch.Loam, wallHeight: 2.4f)));

            // ---- East street: Kustai and the caravan notice board (the second mission), Baraies the teacher ----
            r.Lots.Add(Street("casa_kustai", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "kustai", null,
                TerraceSpec(TownShopDressing.None, TownSwatch.Loam, door: -0.15f, wallHeight: 2.5f)));
            r.Lots.Add(Street("escola_baraies", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "baraies", null,
                TerraceSpec(TownShopDressing.None, TownSwatch.TerraceStone, wallHeight: 2.4f)));

            // ---- South street (the way down the valley): two houses ----
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, null, null,
                TerraceSpec(TownShopDressing.None, TownSwatch.TerraceStone, door: 0.15f, wallHeight: 2.5f)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, TerraceSpec(TownShopDressing.None, TownSwatch.Loam, wallHeight: 2.4f)));

            // ---- The plaza (the 4th terrace): the Marco das Caravanas, lamps ----
            r.PlazaProps.Add(At(CityPropKind.CaravanStone, SangkentCaravanStone.x, SangkentCaravanStone.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Irrigation channels crossing the East and South streets, each with its stone slab ----
            r.PlazaProps.Add(Segment(CityPropKind.IrrigationChannel, 15.8f, -2.4f, 15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.IrrigationChannel, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: apricot trees in blossom (and one in fruit), chute carts waiting ----
            r.PlazaProps.Add(At(CityPropKind.ApricotTree, -9.5f, 12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ApricotTree, 9.2f, 12f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ApricotTree, 11.5f, -4.4f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.ApricotTree, 13f, -9f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ApricotTree, -14.5f, -12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ApricotTree, -18f, -5.5f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.ApricotTree, 16f, -13.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.ChuteCart, 6.5f, -17.4f, 90f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ChuteCart, -11f, -12.5f, 20f, 1f, 1));

            // ---- The terraces: no street to the north; the retaining walls in three runs (the fort above the middle one) ----
            r.PlazaProps.Add(Segment(CityPropKind.TerraceWall, -8f, 19.5f, 8f, 19.5f, 1f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.TerraceWall, -15.5f, 14f, -10.5f, 14f));
            r.PlazaProps.Add(Segment(CityPropKind.TerraceWall, 10.5f, 14f, 15.5f, 14f));

            // ---- The stone chutes coming down the terraces to their landings, in front of the side runs ----
            r.PlazaProps.Add(At(CityPropKind.StoneChute, -13f, 12.4f, CityGeometry.YawFacing(Vector2.down)));
            r.PlazaProps.Add(At(CityPropKind.StoneChute, 13f, 12.4f, CityGeometry.YawFacing(Vector2.down)));
            return r;
        }

        /// <summary>A Pamir house of Sangkent: two storeys (stone below, <paramref name="upper"/> above), juniper lacing and joinery, a flat loam roof.</summary>
        private static TownBuildingSpec TerraceSpec(TownShopDressing dressing, TownSwatch upper, float door = 0f, float wallHeight = 2.5f)
        {
            TownBuildingSpec spec = Spec(upper, TownSwatch.Juniper, TownSwatch.Juniper, TownSwatch.SilkBlue, roof: TownRoofKind.Flat, dressing: dressing,
                sign: TownSwatch.SilkRed, door: door, wallHeight: wallHeight, windows: 1, roofColor: TownSwatch.Loam);
            spec.StoneTerrace = true;
            spec.Floors = 2;
            spec.PlinthColor = TownSwatch.StoneDark;
            return spec;
        }
    }
}
