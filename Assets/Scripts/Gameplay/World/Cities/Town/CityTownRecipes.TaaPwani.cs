using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Taa Pwani, the Costa da Lâmpada (Docs/Mapas100.md região 5 and map 25; Kilwa Kisiwani, Lamu and Gedi as the real
    /// reference): the city of return of the Costa das Monções, in its own style (<see cref="TownStyles.Coast"/>):
    /// whitewashed coral-stone houses with roof terraces and stepped merlons, carved teak doors studded with brass, a
    /// <i>baraza</i> along each front, sand streets edged in coral and lit by strings of lanterns. The Farol de Coral (the
    /// lamp tower that gives the town its name) stands behind Nuru's house at the head of the plaza; the pearl market
    /// spreads on the plaza; the West street is the bazaar (Mwana's counter, old Mbaraka, the Cais da Barca); the East
    /// street runs down to the water (Captain Liyongo's two-storey house, Zawadi braiding root boots) and ends at the dhows
    /// moored in their tidal pools; mangroves fringe the south-east, coconut palms and baobabs in the yards. Same
    /// conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 25 and 30 (= <c>MapCatalog.TaaPwani</c>, <see cref="CityCatalog.TaaPwani"/>).</summary>
        public const string TaaPwaniCityId = "taa_pwani";

        /// <summary>The two dhows moored past the end of the East street (bow to the north, the second with its sail furled).</summary>
        public static readonly Vector2[] TaaPwaniDhows = { new Vector2(35f, 9f), new Vector2(35.5f, -9.5f) };

        /// <summary>Where the Farol de Coral stands (behind Nuru's house, at the head of the plaza).</summary>
        public static readonly Vector2 TaaPwaniLampTower = new Vector2(0f, 19.5f);

        public static CityTownRecipe TaaPwani()
        {
            var r = new CityTownRecipe
            {
                CityId = TaaPwaniCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.Coast,
                PlazaRadius = 8f,
                EntryArm = CityArm.South,
                NightLights = 8,
                LampStrings = true,
                LampSpacing = 7f,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 34f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 30f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 30f, exitToMap: true));

            // ---- Plaza head: Nuru's house at the foot of the Farol de Coral ----
            TownBuildingSpec farol = Coral(TownSwatch.Sea, wallHeight: 3.2f);
            farol.FullVaranda = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_do_farol", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.5f), Front = Vector2.down,
                Width = 7f, Depth = 5f, Porch = 1.6f, Spec = farol, ResidentId = "nuru",
            });
            r.PlazaProps.Add(At(CityPropKind.CoralTower, TaaPwaniLampTower.x, TaaPwaniLampTower.y));

            // ---- West street: the bazaar (Mwana's pearls, old Mbaraka, the Cais da Barca) ----
            r.Lots.Add(Street("bazar_mwana", CityLotKind.Shop, CityArm.West, CityArm.North, 12.5f, 7.5f, 6f, 1.8f, "mwana", CityShopCatalog.BazarMwana,
                Coral(TownSwatch.Laterite, dressing: TownShopDressing.Pearls, door: 0.18f, wallHeight: 3.1f)));
            TownBuildingSpec mbaraka = Coral(TownSwatch.Sea, door: -0.15f);
            mbaraka.UpperRoom = true;
            r.Lots.Add(Street("casa_mbaraka", CityLotKind.House, CityArm.West, CityArm.North, 21f, 6.5f, 5.2f, 1.4f, "mbaraka", null, mbaraka));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 28.5f, 7f, 3.2f, 1.0f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 12.5f, 6f, 4.8f, 1.0f));
            r.Lots.Add(Street("casa_w2", CityLotKind.House, CityArm.West, CityArm.South, 21f, 6f, 4.8f, 1.0f));
            r.Lots.Add(Street("casa_w3", CityLotKind.House, CityArm.West, CityArm.South, 28.5f, 6f, 4.8f, 1.0f));

            // ---- East street: down to the water (the nakhoda's house, Zawadi's root boots, the dhows) ----
            TownBuildingSpec nakhoda = Coral(TownSwatch.Laterite, door: -0.18f, floors: 2);
            nakhoda.FullVaranda = true;
            nakhoda.WideDoor = true;
            r.Lots.Add(Street("casa_liyongo", CityLotKind.House, CityArm.East, CityArm.North, 12.5f, 7.5f, 5.4f, 1.8f, "liyongo", null, nakhoda));
            r.Lots.Add(Street("casa_zawadi", CityLotKind.House, CityArm.East, CityArm.North, 21.5f, 6.5f, 5f, 1.4f, "zawadi", null,
                Coral(TownSwatch.Sail, dressing: TownShopDressing.RootWeaver, door: 0.15f)));
            r.Lots.Add(Street("casa_e1", CityLotKind.House, CityArm.East, CityArm.South, 14f, 6f, 4.8f, 1.0f));
            r.Lots.Add(Street("casa_e2", CityLotKind.House, CityArm.East, CityArm.South, 22f, 6f, 4.8f, 1.0f));

            r.PlazaProps.Add(At(CityPropKind.Dhow, TaaPwaniDhows[0].x, TaaPwaniDhows[0].y, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Dhow, TaaPwaniDhows[1].x, TaaPwaniDhows[1].y, 15f, 0.9f, 1));

            // ---- South street (the road in from the dunes) ----
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 12.5f, 6f, 4.8f, 1.0f));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 15f, 6f, 4.8f, 1.0f));
            r.Lots.Add(Street("casa_s3", CityLotKind.House, CityArm.South, CityArm.East, 22f, 6f, 4.8f, 1.0f));

            // ---- The mangrove fringe of the south-east (between the East and South streets) ----
            r.PlazaProps.Add(At(CityPropKind.Mangrove, 17.5f, -17f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Mangrove, 24.5f, -14f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Mangrove, 27f, -21.5f, 0f, 0.9f, 2));
            r.PlazaProps.Add(At(CityPropKind.Mangrove, 14.5f, -26f, 0f, 1f, 3));

            // ---- Plaza: the pearl market, the cistern, barazas, coconut palms, lamps ----
            r.PlazaProps.Add(At(CityPropKind.Stall, -2.2f, 5.4f, 0f, 1f, 3));
            r.PlazaProps.Add(At(CityPropKind.Stall, -4.7f, 4.0f, 0f, 1f, 3));
            r.PlazaProps.Add(At(CityPropKind.Well, -4.4f, -4.0f, 0f, 0.9f));
            AddPlazaBench(r, 70f, 6.4f);
            AddPlazaBench(r, 315f, 6.4f);
            r.PlazaProps.Add(At(CityPropKind.Tree, 4.4f, 4.4f, 0f, 1.1f, 0));
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Tree, Position = Polar(240f, 6.2f), Scale = 1.0f, Variant = 2 });
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            r.PlazaProps.Add(At(CityPropKind.Bush, 6.6f, 6.0f, 0f, 1.0f));
            r.PlazaProps.Add(At(CityPropKind.Bush, -6.8f, -6.6f, 30f, 0.9f));
            return r;
        }

        /// <summary>A coral-stone house of Taa Pwani: coral-lime walls and roof terrace, teak joinery, brass accents.</summary>
        private static TownBuildingSpec Coral(TownSwatch trim, TownShopDressing dressing = TownShopDressing.None, float door = 0f, float wallHeight = 3.0f, int floors = 1)
        {
            TownBuildingSpec spec = Spec(TownSwatch.CoralLime, trim, TownSwatch.Teak, TownSwatch.Gold, dressing: dressing, sign: TownSwatch.Sea, door: door,
                wallHeight: floors > 1 ? 2.8f : wallHeight, roofColor: TownSwatch.CoralLime);
            spec.Floors = floors;
            spec.CoralStone = true;
            return spec;
        }
    }
}
