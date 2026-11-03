using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Torres Brancas de Amanirenas, race city of the high elves (Docs/Mapas100.md §3.3 and map 42; return city of maps 42
    /// and 45), in its own style (<see cref="TownStyles.WhiteSpire"/>): white spire houses on stepped sandstone bases with glass
    /// domes and bronze mirrors round a lime-white plaza with the Relógio de Espelhos in the middle (the mirrors pass the
    /// sunlight down to the ground, day and night). Queen Amanirenas's house closes the head of the plaza (the queen-guardian:
    /// the missions); Natakamani's mirrors (the city's shop: refining with reflected light) and the Cais da Barca on the West
    /// street, the road in from Mingcheng; Amanitore the builder and Arkamani who reads the stars on the East street;
    /// Shanakdakhete, who teaches the light (the second mission), on the South street; garden rills with leaf bridges crossing
    /// the East and South streets; mirror posts, garden trees and small steep pyramids (the memory of Meroë) between the
    /// streets; no street to the north: there the garden terraces step up in three runs with the white spires on them, the
    /// tallest one with its glass dome and great mirror in the middle run, joined to two lower spires by leaf bridges.
    /// Everything stays inside a circle of radius <see cref="TorresBrancasSolidRadius"/> at y = 0 (the place the Mingcheng
    /// overworld will keep for it), with three roads out (W, E, S) ending at ±<see cref="TorresBrancasArmLength"/>.
    /// Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 42 and 45 (= <c>MapCatalog.TorresBrancas</c>, <see cref="CityCatalog.TorresBrancas"/>).</summary>
        public const string TorresBrancasCityId = "torres_brancas";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float TorresBrancasArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float TorresBrancasHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float TorresBrancasSolidRadius = 22.5f;

        /// <summary>Where the Relógio de Espelhos stands (north of the plaza centre, in front of Amanirenas's house).</summary>
        public static readonly Vector2 TorresBrancasMirrorClock = new Vector2(0f, 3.6f);

        public static CityTownRecipe TorresBrancas()
        {
            var r = new CityTownRecipe
            {
                CityId = TorresBrancasCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.WhiteSpire,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = TorresBrancasHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, TorresBrancasArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, TorresBrancasArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, TorresBrancasArmLength, exitToMap: true));

            // ---- Plaza head: the house of Queen Amanirenas, the queen-guardian (the missions) ----
            TownBuildingSpec queen = SpireSpec(TownShopDressing.None, wallHeight: 3.0f);
            queen.FullVaranda = true;
            queen.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_de_amanirenas", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = queen, ResidentId = "amanirenas",
            });

            // ---- West street (the road in from Mingcheng): Natakamani's mirrors (the city's shop), the Cais da Barca; a house across ----
            TownBuildingSpec espelhos = SpireSpec(TownShopDressing.Mirrors, door: 0.18f, wallHeight: 2.9f);
            espelhos.SignColor = TownSwatch.SkyBlue;
            r.Lots.Add(Street("espelhos_natakamani", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "natakamani", CityShopCatalog.EspelhosNatakamani, espelhos));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, SpireSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street: Amanitore the builder, Arkamani who reads the stars ----
            r.Lots.Add(Street("casa_amanitore", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "amanitore", null,
                SpireSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.8f)));
            r.Lots.Add(Street("observatorio_arkamani", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, "arkamani", null,
                SpireSpec(TownShopDressing.None, wallHeight: 2.6f)));

            // ---- South street: Shanakdakhete, who teaches the light (the second mission); a house across ----
            r.Lots.Add(Street("casa_shanakdakhete", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "shanakdakhete", null,
                SpireSpec(TownShopDressing.None, door: 0.15f, wallHeight: 2.8f)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, SpireSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The plaza: the Relógio de Espelhos, lamps ----
            r.PlazaProps.Add(At(CityPropKind.MirrorClock, TorresBrancasMirrorClock.x, TorresBrancasMirrorClock.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Garden rills crossing the East and South streets, each with its leaf bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.LeafBridge, 15.8f, -2.4f, 15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.LeafBridge, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: garden trees, mirror posts passing the light, small pyramids ----
            r.PlazaProps.Add(At(CityPropKind.Tree, -9.5f, 12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Tree, 9.2f, 12f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.MirrorPost, 11.5f, -4.4f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.MirrorPost, 13f, -9f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.MirrorPost, -14.5f, -12.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Tree, -18f, -5.5f, 0f, 0.9f, 2));
            r.PlazaProps.Add(At(CityPropKind.Tree, 16f, -13.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.MirrorPost, 6.5f, -17.4f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.SpirePyramid, -11f, -12.5f, 20f));

            // ---- The garden terraces: no street to the north; three runs (the tallest spire on the middle one) ----
            r.PlazaProps.Add(Segment(CityPropKind.SpireTerrace, -8f, 19.5f, 8f, 19.5f, 1f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.SpireTerrace, -15.5f, 14f, -10.5f, 14f));
            r.PlazaProps.Add(Segment(CityPropKind.SpireTerrace, 10.5f, 14f, 15.5f, 14f));

            // ---- Small pyramids in front of the side runs ----
            r.PlazaProps.Add(At(CityPropKind.SpirePyramid, -13f, 12.4f, CityGeometry.YawFacing(Vector2.down)));
            r.PlazaProps.Add(At(CityPropKind.SpirePyramid, 13f, 12.4f, CityGeometry.YawFacing(Vector2.down)));
            return r;
        }

        /// <summary>A white spire house of the Torres Brancas: lime walls, pale-gold trim, bronze door and mirror, a sandstone base, a glass dome.</summary>
        private static TownBuildingSpec SpireSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.8f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.SpireLime, TownSwatch.PaleGold, TownSwatch.MirrorBronze, TownSwatch.SkyBlue, roof: TownRoofKind.Dome, dressing: dressing,
                sign: TownSwatch.SkyBlue, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.Glass);
            spec.SpireHouse = true;
            spec.PlinthColor = TownSwatch.MeroeSandstone;
            return spec;
        }
    }
}
