using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Acampamento de Soslan, race city of the centaurs (Docs/Mapas100.md §3.3 and map 47; return city of maps 47, 48 and
    /// 51), in its own style (<see cref="TownStyles.SteppeCamp"/>): round felt tents on wagons round a plaza of packed earth with
    /// the collective stone oven in the middle. Soslan's tent closes the head of the plaza (the steel-skinned hero: the
    /// missions); Syrdon's saddlery (the city's shop: the Selaria Nômade) and the Cais da Barca on the West street, the road in
    /// across the steppe; Satana with the guardian lamp (the second mission) and old Uryzmæg on the East street; Batradz's
    /// forge on the South street; horsehair standards with gold stags and felt carts between the streets; no street to the
    /// north: there the rope corrals of the herd in three runs, hay piled behind them. Everything stays inside a circle of
    /// radius <see cref="SoslanSolidRadius"/> at y = 0 (the place the Estepe dos Nartes overworld keeps for it), with three
    /// roads out (W, E, S) ending at ±<see cref="SoslanArmLength"/>. The camp moves across the steppe (the ficha); the town is
    /// one of its stops. Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 47, 48 and 51 (= <c>MapCatalog.AcampamentoSoslan</c>, <see cref="CityCatalog.AcampamentoSoslan"/>).</summary>
        public const string SoslanCityId = "acampamento_soslan";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float SoslanArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float SoslanHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float SoslanSolidRadius = 22.5f;

        /// <summary>Where the stone oven stands (north of the plaza centre, in front of Soslan's tent).</summary>
        public static readonly Vector2 SoslanStoneOven = new Vector2(0f, 3.6f);

        public static CityTownRecipe Soslan()
        {
            var r = new CityTownRecipe
            {
                CityId = SoslanCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.SteppeCamp,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = SoslanHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, SoslanArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, SoslanArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, SoslanArmLength, exitToMap: true));

            // ---- Plaza head: Soslan's tent, the steel-skinned hero (the missions) ----
            TownBuildingSpec soslan = YurtSpec(TownShopDressing.None, TownSwatch.FeltRed, wallHeight: 3.0f);
            soslan.FullVaranda = true;
            soslan.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "tenda_de_soslan", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = soslan, ResidentId = "soslan",
            });

            // ---- West street (the road in across the steppe): Syrdon's saddlery (the city's shop), the Cais da Barca; a tent across ----
            TownBuildingSpec selaria = YurtSpec(TownShopDressing.Saddlery, TownSwatch.FeltBlue, door: 0.18f, wallHeight: 2.7f);
            selaria.SignColor = TownSwatch.FeltRed;
            r.Lots.Add(Street("selaria_nomade", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "syrdon", CityShopCatalog.SelariaNomade, selaria));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("tenda_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, YurtSpec(TownShopDressing.None, TownSwatch.FeltBlue, wallHeight: 2.6f)));

            // ---- East street: Satana and the guardian lamp (the second mission), old Uryzmæg ----
            r.Lots.Add(Street("tenda_satana", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "satana", null,
                YurtSpec(TownShopDressing.None, TownSwatch.FeltRed, door: -0.15f, wallHeight: 2.7f)));
            r.Lots.Add(Street("tenda_uryzmaeg", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "uryzmaeg", null,
                YurtSpec(TownShopDressing.None, TownSwatch.ScythianGold, wallHeight: 2.6f)));

            // ---- South street: Batradz's forge tent; a tent across ----
            r.Lots.Add(Street("forja_batradz", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "batradz", null,
                YurtSpec(TownShopDressing.None, TownSwatch.FeltBlue, door: 0.15f, wallHeight: 2.7f)));
            r.Lots.Add(Street("tenda_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, YurtSpec(TownShopDressing.None, TownSwatch.FeltRed, wallHeight: 2.6f)));

            // ---- The plaza: the stone oven, lamps ----
            r.PlazaProps.Add(At(CityPropKind.StoneOven, SoslanStoneOven.x, SoslanStoneOven.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Between the streets: horsehair standards with the gold stags, felt carts waiting to move ----
            r.PlazaProps.Add(At(CityPropKind.HorseStandard, -9.5f, 12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.HorseStandard, 9.2f, 12f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.HorseStandard, 11.5f, -4.4f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.HorseStandard, 6.5f, -17.4f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.FeltWagon, 13f, -9f, 30f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.FeltWagon, -18f, -5.5f, -20f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.FeltWagon, 15f, -13f, 60f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.FeltWagon, -12.5f, -13f, 15f, 1f, 1));

            // ---- The corrals of the herd: no street to the north; rope corrals in three runs, hay behind ----
            r.PlazaProps.Add(Segment(CityPropKind.RopeCorral, -8f, 19.5f, 8f, 19.5f));
            r.PlazaProps.Add(Segment(CityPropKind.RopeCorral, -15.5f, 14f, -10.5f, 14f));
            r.PlazaProps.Add(Segment(CityPropKind.RopeCorral, 10.5f, 14f, 15.5f, 14f));

            // ---- Felt carts in front of the side runs ----
            r.PlazaProps.Add(At(CityPropKind.FeltWagon, -13f, 11.4f, 90f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.FeltWagon, 13.4f, 11.2f, -90f, 1f, 1));
            return r;
        }

        /// <summary>A felt tent on a wagon of the camp: felt walls, a dyed band (<paramref name="band"/>), rawhide door, gold accents.</summary>
        private static TownBuildingSpec YurtSpec(TownShopDressing dressing, TownSwatch band, float door = 0f, float wallHeight = 2.6f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.Felt, band, TownSwatch.SteppeLeather, TownSwatch.ScythianGold, dressing: dressing,
                sign: TownSwatch.FeltBlue, door: door, wallHeight: wallHeight, windows: 0, roofColor: TownSwatch.Felt);
            spec.FeltYurt = true;
            spec.PlinthColor = TownSwatch.WoodDark;
            return spec;
        }
    }
}
