using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Kirta, the Cidade-Recife, race city of the povo-do-mar (Docs/Mapas100.md §3.3 and map 28; return city of maps 28–29),
    /// in its own style (<see cref="TownStyles.CoralReef"/>): the dry heart of the atoll. Round coral towers over drowned
    /// bases, mother-of-pearl domes and scallop canopies round a pearl plaza; the mother coral in its pool on the south side
    /// of the plaza (half grey from the Lodo, half relit: relighting it is the map's story); King Kirta's tower on the North
    /// street (the missions; the kelp gardens of the ficha lie to the north), Danel's diving tools (the city's shop) and the
    /// Cais da Barca on the West street, Pughat and Aqhat the archer on the East street; shell bridges over the reef water
    /// that crosses the North and East streets; kelp gardens and coral spires between them; no street to the south: there
    /// the reef edge drops into the abyss. Everything stays inside a circle of radius <see cref="KirtaSolidRadius"/> at y = 0
    /// (the place the Costa das Monções overworld keeps for it), with three roads out (W, E, N) ending at ±<see cref="KirtaArmLength"/>.
    /// Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 28–29 (= <c>MapCatalog.Kirta</c>, <see cref="CityCatalog.Kirta"/>).</summary>
        public const string KirtaCityId = "kirta";

        /// <summary>Length of the three streets (W, E, N): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float KirtaArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float KirtaHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float KirtaSolidRadius = 22.5f;

        /// <summary>Where the mother coral stands (on the south side of the plaza, between the cousins and the abyss).</summary>
        public static readonly Vector2 KirtaMotherCoral = new Vector2(0f, -4.4f);

        public static CityTownRecipe Kirta()
        {
            var r = new CityTownRecipe
            {
                CityId = KirtaCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.CoralReef,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = KirtaHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, KirtaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, KirtaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.North, KirtaArmLength, exitToMap: true));

            // ---- West street: Danel's diving tools (the city's shop), the Cais da Barca ----
            TownBuildingSpec mergulho = ReefSpec(TownShopDressing.DivingGear, door: 0.18f, wallHeight: 3.0f);
            mergulho.SignColor = TownSwatch.CoralLive;
            r.Lots.Add(Street("mergulho_danel", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "danel", CityShopCatalog.MergulhoDanel, mergulho));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));

            // ---- North street (to the kelp gardens): King Kirta's tower; a plain tower across ----
            TownBuildingSpec palace = ReefSpec(TownShopDressing.None, wallHeight: 3.4f);
            palace.FullVaranda = true;
            palace.WideDoor = true;
            r.Lots.Add(Street("torre_de_kirta", CityLotKind.House, CityArm.North, CityArm.East, 13f, 7f, 5.5f, 1.8f, "kirta", null, palace));
            r.Lots.Add(Street("casa_n1", CityLotKind.House, CityArm.North, CityArm.West, 13f, 6f, 4.5f, 1f, null, null, ReefSpec(TownShopDressing.None, wallHeight: 2.9f)));

            // ---- East street: Pughat, Aqhat the archer ----
            r.Lots.Add(Street("casa_pughat", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "pughat", null,
                ReefSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.9f)));
            r.Lots.Add(Street("casa_aqhat", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "aqhat", null,
                ReefSpec(TownShopDressing.HuntingRack, wallHeight: 2.8f)));

            // ---- The plaza: the mother coral, lamps ----
            r.PlazaProps.Add(At(CityPropKind.CoralMother, KirtaMotherCoral.x, KirtaMotherCoral.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Reef water crossing the North and East streets, each with its shell bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.ShellBridge, -2.4f, 6.8f, 2.4f, 6.8f));
            r.PlazaProps.Add(Segment(CityPropKind.ShellBridge, 15.8f, -2.4f, 15.8f, 2.4f));

            // ---- Between the streets: coral spires and kelp gardens ----
            r.PlazaProps.Add(At(CityPropKind.CoralSpire, -14f, 14f));
            r.PlazaProps.Add(At(CityPropKind.CoralSpire, 13.5f, -11f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.KelpGarden, -8f, -8.5f));
            r.PlazaProps.Add(At(CityPropKind.KelpGarden, 8.5f, -8f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.KelpGarden, -13.5f, -11f, 0f, 1.1f));
            r.PlazaProps.Add(At(CityPropKind.KelpGarden, 13f, 15.5f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.Tree, -18f, -6f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Tree, 18.5f, -6.5f, 0f, 0.9f, 2));

            // ---- The reef edge over the abyss: no street to the south, the rim steps down in three runs ----
            r.PlazaProps.Add(Segment(CityPropKind.ReefEdge, 8f, -19f, -8f, -19f));
            r.PlazaProps.Add(Segment(CityPropKind.ReefEdge, 13.5f, -16f, 9f, -16f));
            r.PlazaProps.Add(Segment(CityPropKind.ReefEdge, -9f, -16f, -13.5f, -16f));
            return r;
        }

        /// <summary>A coral tower of Kirta: live coral over a drowned base, reef-turquoise trims, kelp joinery, a pearl dome.</summary>
        private static TownBuildingSpec ReefSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 3.0f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.CoralLive, TownSwatch.ReefTeal, TownSwatch.Kelp, TownSwatch.ReefTeal, roof: TownRoofKind.Dome, dressing: dressing,
                sign: TownSwatch.ReefTeal, door: door, wallHeight: wallHeight, windows: 1, roofColor: TownSwatch.Pearl);
            spec.ReefTower = true;
            spec.PlinthColor = TownSwatch.CoralDim;
            return spec;
        }
    }
}
