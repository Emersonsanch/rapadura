using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Arvoredo de Haripushpa, race city of the trunk folk (Docs/Mapas100.md §3.3 and map 50; its own return city), in its
    /// own style (<see cref="TownStyles.LivingGrove"/>): houses inside hollow giant trunks round a plaza of moss with the
    /// circle of standing stones. Old Haripushpa's trunk closes the head of the plaza (the eldest: the missions);
    /// Suvarnapushpa's Viveiro (the city's shop) and the Cais da Barca on the West street, the trail in from the steppe; the
    /// guest trunk of Aryaman Puhr, the Sogdian herbalist passing through (the second mission), on the East street; two trunks
    /// on the South street; meltwater streams with split-log bridges crossing the East and South streets; larches between the
    /// streets (one old one asleep); no street to the north: there the taiga closes in, larches in a row and mossy stones.
    /// Everything stays inside a circle of radius <see cref="ArvoredoSolidRadius"/> at y = 0 (the place the Estepe dos Nartes
    /// overworld keeps for it), with three roads out (W, E, S) ending at ±<see cref="ArvoredoArmLength"/>. Same conventions as
    /// the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of map 50 (= <c>MapCatalog.Arvoredo</c>, <see cref="CityCatalog.Arvoredo"/>).</summary>
        public const string ArvoredoCityId = "arvoredo";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float ArvoredoArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float ArvoredoHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float ArvoredoSolidRadius = 22.5f;

        /// <summary>Radius of the circle of standing stones round the plaza centre.</summary>
        public const float ArvoredoStoneCircle = 4.6f;

        public static CityTownRecipe Arvoredo()
        {
            var r = new CityTownRecipe
            {
                CityId = ArvoredoCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.LivingGrove,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = ArvoredoHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, ArvoredoArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, ArvoredoArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, ArvoredoArmLength, exitToMap: true));

            // ---- Plaza head: old Haripushpa's trunk, the eldest (the missions) ----
            TownBuildingSpec haripushpa = TrunkSpec(TownShopDressing.None, wallHeight: 3.4f);
            haripushpa.FullVaranda = true;
            haripushpa.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "tronco_de_haripushpa", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = haripushpa, ResidentId = "haripushpa",
            });

            // ---- West street (the trail in from the steppe): Suvarnapushpa's nursery (the city's shop), the Cais da Barca; a trunk across ----
            TownBuildingSpec viveiro = TrunkSpec(TownShopDressing.Nursery, door: 0.18f, wallHeight: 3.0f);
            viveiro.SignColor = TownSwatch.GroveFlower;
            r.Lots.Add(Street("viveiro", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "suvarnapushpa", CityShopCatalog.ViveiroSuvarnapushpa, viveiro));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("tronco_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, TrunkSpec(TownShopDressing.None, wallHeight: 2.8f)));

            // ---- East street: the guest trunk of Aryaman Puhr, the herbalist passing through (the second mission); a sleeping trunk ----
            r.Lots.Add(Street("tronco_de_hospedes", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "aryaman", null,
                TrunkSpec(TownShopDressing.None, door: -0.15f, wallHeight: 3.0f)));
            r.Lots.Add(Street("tronco_e2", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, null, null, TrunkSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- South street: two trunks ----
            r.Lots.Add(Street("tronco_s1", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, null, null,
                TrunkSpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.0f)));
            r.Lots.Add(Street("tronco_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, TrunkSpec(TownShopDressing.None, wallHeight: 2.8f)));

            // ---- The plaza: the circle of mossy standing stones (open on the four ways), lamps ----
            for (int i = 0; i < 6; i++)
            {
                float angle = new[] { 45f, 70f, 110f, 135f, 225f, 300f }[i];
                Vector2 at = Polar(angle, ArvoredoStoneCircle);
                r.PlazaProps.Add(At(CityPropKind.MossStone, at.x, at.y, 0f, 1f, i % 3 == 1 ? 1 : 0));
            }

            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Meltwater streams crossing the East and South streets, each with its split-log bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.MeltStream, 15.8f, -2.4f, 15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.MeltStream, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: larches (an old one asleep by the eldest's trunk) ----
            r.PlazaProps.Add(At(CityPropKind.Larch, -9.5f, 12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Larch, 9.2f, 12f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Larch, 11.5f, -4.4f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.Larch, 13f, -9f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Larch, -14.5f, -12.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Larch, -18f, -5.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.Larch, 16f, -13.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.MossStone, 6.5f, -17.4f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.MossStone, -11f, -12.5f, 0f, 1.1f, 0));

            // ---- The taiga closes in: no street to the north; larches in a row and mossy stones ----
            foreach (float x in new[] { -7.5f, -4f, 4f, 7.5f })
            {
                r.PlazaProps.Add(At(CityPropKind.Larch, x, 19.2f, 0f, 1.15f, 0));
            }

            r.PlazaProps.Add(At(CityPropKind.Larch, 0f, 19.6f, 0f, 1.3f, 1));
            r.PlazaProps.Add(At(CityPropKind.Larch, -13f, 14f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Larch, 13f, 14f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.MossStone, -15.5f, 12.4f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.MossStone, 15.5f, 12.4f, 0f, 1f, 0));
            return r;
        }

        /// <summary>A hollow trunk house of the grove: bark walls, moss, a crown of larch, a flower over the door.</summary>
        private static TownBuildingSpec TrunkSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 3.0f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.GroveBark, TownSwatch.GroveMoss, TownSwatch.GroveBark, TownSwatch.GroveFlower, dressing: dressing,
                sign: TownSwatch.NewLeaf, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.Larch);
            spec.HollowTrunk = true;
            spec.PlinthColor = TownSwatch.GroveBark;
            return spec;
        }
    }
}
