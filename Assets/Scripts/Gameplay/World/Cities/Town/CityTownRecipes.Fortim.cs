using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Fortim dos Gigantes-da-Colina, race city of the hill giants (Docs/Mapas100.md §3.3 and map 49; return city of maps
    /// 49 and 52), in its own style (<see cref="TownStyles.HillFort"/>): round giant houses of dry stone under turf roofs on the
    /// top of the hill, round a plaza of packed earth with the pot the size of a house over the Living Fire. Baraqel's house
    /// closes the head of the plaza (the chief: the missions); old Shahmizad's Panela Grande (the city's shop: food for the
    /// group and the crampons of map 52) and the Cais da Barca on the West street, the road up the hill; Virogdad the dreamer
    /// (the second mission) on the South street, the way down to the valley of the dream tablets; only two roads: the hill
    /// falls away to the east, behind an earth rampart, and to the north the rings of the fort climb in three runs (a round
    /// watch-tower on the middle one). Stone tables, giant spoons as benches and mountain trees between them. Everything stays
    /// inside a circle of radius <see cref="FortimSolidRadius"/> at y = 0 (the place the Estepe dos Nartes overworld keeps for
    /// it), with two roads out (W, S) ending at ±<see cref="FortimArmLength"/>. Same conventions as the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 49 and 52 (= <c>MapCatalog.Fortim</c>, <see cref="CityCatalog.Fortim"/>).</summary>
        public const string FortimCityId = "fortim";

        /// <summary>Length of the two streets (W, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float FortimArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float FortimHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float FortimSolidRadius = 22.5f;

        /// <summary>Where the great pot stands (north of the plaza centre, in front of Baraqel's house).</summary>
        public static readonly Vector2 FortimGreatPot = new Vector2(0f, 3.6f);

        public static CityTownRecipe Fortim()
        {
            var r = new CityTownRecipe
            {
                CityId = FortimCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.HillFort,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = FortimHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, FortimArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, FortimArmLength, exitToMap: true));

            // ---- Plaza head: Baraqel's house, the chief (the missions) ----
            TownBuildingSpec baraqel = RoundhouseSpec(TownShopDressing.None, wallHeight: 4.2f);
            baraqel.FullVaranda = true;
            baraqel.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_de_baraqel", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = baraqel, ResidentId = "baraqel",
            });

            // ---- West street (the road up the hill): Shahmizad's great pot (the city's shop), the Cais da Barca; a house across ----
            TownBuildingSpec panela = RoundhouseSpec(TownShopDressing.GreatPot, door: 0.18f, wallHeight: 3.8f);
            panela.SignColor = TownSwatch.LivingFire;
            r.Lots.Add(Street("panela_grande", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "shahmizad", CityShopCatalog.PanelaGrande, panela));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, RoundhouseSpec(TownShopDressing.None, wallHeight: 3.6f)));

            // ---- South street (the way down to the valley of the dream tablets): Virogdad the dreamer (the second mission); a house across ----
            r.Lots.Add(Street("casa_virogdad", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "virogdad", null,
                RoundhouseSpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.8f)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, RoundhouseSpec(TownShopDressing.None, wallHeight: 3.6f)));

            // ---- The plaza: the great pot over the Living Fire, lamps ----
            r.PlazaProps.Add(At(CityPropKind.GiantPot, FortimGreatPot.x, FortimGreatPot.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- The east side of the hill (no street): stone tables and a spoon bench, the rampart over the slope ----
            r.PlazaProps.Add(At(CityPropKind.GiantTable, 11f, 6.5f, 0f));
            r.PlazaProps.Add(At(CityPropKind.GiantTable, 11.5f, -6.5f, 15f));
            r.PlazaProps.Add(At(CityPropKind.SpoonBench, 9f, 1.5f, 0f));
            r.PlazaProps.Add(Segment(CityPropKind.EarthRing, 19.5f, 9f, 19.5f, -9f));

            // ---- Between the streets: a spoon bench by the chief's house, mountain trees ----
            r.PlazaProps.Add(At(CityPropKind.SpoonBench, -11f, 12.6f, 0f));
            r.PlazaProps.Add(At(CityPropKind.Tree, -18f, -5.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Tree, -12.5f, -13f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Tree, 15f, -13f, 0f, 1f, 2));
            r.PlazaProps.Add(At(CityPropKind.Tree, 6.5f, -17.4f, 0f, 0.9f, 0));

            // ---- The rings of the fort: no street to the north; the earth ramparts in three runs (the watch-tower on the middle one) ----
            r.PlazaProps.Add(Segment(CityPropKind.EarthRing, -8f, 19.5f, 8f, 19.5f, 1f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.EarthRing, -15.5f, 14f, -10.5f, 14f));
            r.PlazaProps.Add(Segment(CityPropKind.EarthRing, 10.5f, 14f, 15.5f, 14f));
            return r;
        }

        /// <summary>A round giant house of the Fortim: dry-stone walls, earth courses, a giant timber door, a turf roof.</summary>
        private static TownBuildingSpec RoundhouseSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 3.8f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.FortStone, TownSwatch.FortEarth, TownSwatch.GiantWood, TownSwatch.LivingFire, dressing: dressing,
                sign: TownSwatch.HillGrass, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.TurfRoof);
            spec.GiantRoundhouse = true;
            spec.PlinthColor = TownSwatch.FortStone;
            return spec;
        }
    }
}
