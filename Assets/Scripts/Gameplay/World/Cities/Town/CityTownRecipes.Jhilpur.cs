using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Jhilpur, a Cidade nas Raízes, capital of the Delta de Jhilpur (Docs/Mapas100.md §2.1 and map 31; return city of maps
    /// 31, 32 and 35), in its own style (<see cref="TownStyles.RootStilt"/>): plank houses on root-columns with curved
    /// boat-back thatch roofs round a round plank plaza; the Raiz-Mãe on its brick platform on the south side of the plaza
    /// (old Salmaios tends it); Nuhzadag's trade house (the city's shop: the gear of every gate of the delta) and the Cais da
    /// Barca on the West street, the road in along the delta; Salmaios's house on the North street (the missions); Abiesous's
    /// boat school and Tehat's herb house on the East street; canals crossing the North and East streets, each with a
    /// living-root bridge; banyans with aerial roots and lotus ponds between the streets; no street to the south: there the
    /// brick ghats step down into the water. Everything stays inside a circle of radius <see cref="JhilpurSolidRadius"/> at
    /// y = 0 (the place the delta overworld will keep for it), with three roads out (W, E, N) ending at ±<see cref="JhilpurArmLength"/>.
    /// Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 31, 32 and 35 (= <c>MapCatalog.Jhilpur</c>, <see cref="CityCatalog.Jhilpur"/>).</summary>
        public const string JhilpurCityId = "jhilpur";

        /// <summary>Length of the three streets (W, E, N): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float JhilpurArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float JhilpurHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float JhilpurSolidRadius = 22.5f;

        /// <summary>Where the Raiz-Mãe stands (on the south side of the plaza, between the cousins and the ghats).</summary>
        public static readonly Vector2 JhilpurMotherRoot = new Vector2(0f, -4.4f);

        public static CityTownRecipe Jhilpur()
        {
            var r = new CityTownRecipe
            {
                CityId = JhilpurCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.RootStilt,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = JhilpurHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, JhilpurArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, JhilpurArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.North, JhilpurArmLength, exitToMap: true));

            // ---- West street (the road in): Nuhzadag's trade house (the city's shop), the Cais da Barca ----
            TownBuildingSpec trocas = StiltSpec(TownShopDressing.BoatTrader, door: 0.18f, wallHeight: 2.6f);
            trocas.SignColor = TownSwatch.LivingWater;
            r.Lots.Add(Street("trocas_nuhzadag", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "nuhzadag", CityShopCatalog.TrocasNuhzadag, trocas));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));

            // ---- North street: old Salmaios's house (the missions); a plain house across ----
            TownBuildingSpec salmaios = StiltSpec(TownShopDressing.None, wallHeight: 2.8f);
            salmaios.FullVaranda = true;
            salmaios.WideDoor = true;
            r.Lots.Add(Street("casa_salmaios", CityLotKind.House, CityArm.North, CityArm.East, 13f, 7f, 5.5f, 1.8f, "salmaios", null, salmaios));
            r.Lots.Add(Street("casa_n1", CityLotKind.House, CityArm.North, CityArm.West, 13f, 6f, 4.5f, 1f, null, null, StiltSpec(TownShopDressing.None, wallHeight: 2.5f)));

            // ---- East street: Abiesous's boat school (the second mission), Tehat's herb house ----
            r.Lots.Add(Street("escola_de_barco", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "abiesous", null,
                StiltSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.6f)));
            r.Lots.Add(Street("ervas_tehat", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "tehat", null,
                StiltSpec(TownShopDressing.Mercearia, wallHeight: 2.4f)));

            // ---- The plaza: the Raiz-Mãe, lamps ----
            r.PlazaProps.Add(At(CityPropKind.MotherRoot, JhilpurMotherRoot.x, JhilpurMotherRoot.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Canals crossing the North and East streets, each with its living-root bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.RootBridge, -2.4f, 6.8f, 2.4f, 6.8f));
            r.PlazaProps.Add(Segment(CityPropKind.RootBridge, 15.8f, -2.4f, 15.8f, 2.4f));

            // ---- Between the streets: banyans with aerial roots and lotus ponds ----
            r.PlazaProps.Add(At(CityPropKind.Banyan, -14f, 14f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Banyan, 13.5f, -11f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Banyan, -18f, -6f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.Banyan, 18.5f, -6.5f, 0f, 0.85f, 0));
            r.PlazaProps.Add(At(CityPropKind.LotusPond, -8f, -8.5f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.LotusPond, 8.5f, -8f, 0f, 0.8f));
            r.PlazaProps.Add(At(CityPropKind.LotusPond, -13.5f, -11f, 0f, 1f));
            r.PlazaProps.Add(At(CityPropKind.LotusPond, 13f, 15.5f, 0f, 0.8f));

            // ---- The ghats: no street to the south, the brick steps go down into the water in three runs ----
            r.PlazaProps.Add(Segment(CityPropKind.Ghat, 8f, -19f, -8f, -19f));
            r.PlazaProps.Add(Segment(CityPropKind.Ghat, 13.5f, -16f, 9f, -16f));
            r.PlazaProps.Add(Segment(CityPropKind.Ghat, -9f, -16f, -13.5f, -16f));
            return r;
        }

        /// <summary>A house of Jhilpur on root-columns: woven straw, ghat-brick footing and bands, root-bark joinery, a boat-back thatch roof.</summary>
        private static TownBuildingSpec StiltSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.6f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.Straw, TownSwatch.GhatBrick, TownSwatch.RootBark, TownSwatch.LivingWater, roof: TownRoofKind.Gable, dressing: dressing,
                sign: TownSwatch.LivingWater, door: door, wallHeight: wallHeight, windows: 1, roofColor: TownSwatch.Thatch);
            spec.RootStilt = true;
            spec.PlinthColor = TownSwatch.GhatBrick;
            return spec;
        }
    }
}
