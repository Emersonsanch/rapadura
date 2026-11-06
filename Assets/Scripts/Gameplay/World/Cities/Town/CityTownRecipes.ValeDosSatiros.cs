using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Vale dos Sátiros, race city of the satyrs (Docs/Mapas100.md §3.3 and map 53; its own return city), in its own style
    /// (<see cref="TownStyles.Glade"/>): gourd-shaped wicker houses round the clearing of the dance, with the round stone stage
    /// on the plaza. King Midas' house (the missions) on the North street, the climb to Farnkesh; Marsyas' Flauta Dupla (the
    /// city's shop) and the Cais da Barca on the West street, the way in; Gordias, the farmer of the knots (the second
    /// mission), on the East street; the river of golden pebbles (the Pactolus, where Midas washes his hands) runs across the
    /// south of the valley and a branch of it crosses the North street, each with a ford of flat stones; rows of vines south of
    /// the houses, a small stage in a clearing to the south and broad oaks round the edge (one with a double flute hung on a
    /// branch); no street to the south: there the river and the grove. Everything stays inside a circle of radius
    /// <see cref="ValeSolidRadius"/> at y = 0 (the place the Picos de Farnkesh overworld keeps for it), with three roads out
    /// (W, N, E) ending at ±<see cref="ValeArmLength"/>. Same conventions as the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of map 53 (= <c>MapCatalog.ValeDosSatiros</c>, <see cref="CityCatalog.ValeDosSatiros"/>).</summary>
        public const string ValeCityId = "vale_dos_satiros";

        /// <summary>Length of the three streets (W, N, E): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float ValeArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float ValeHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float ValeSolidRadius = 22.5f;

        /// <summary>The line (z) the river of golden pebbles runs along, south of the houses.</summary>
        public const float ValeRiverZ = -13.5f;

        public static CityTownRecipe ValeDosSatiros()
        {
            var r = new CityTownRecipe
            {
                CityId = ValeCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.Glade,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.West,
                WalkHalfExtent = ValeHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, ValeArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.North, ValeArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, ValeArmLength, exitToMap: true));

            // ---- North street (the climb to Farnkesh): King Midas' house (the missions); a hut across ----
            TownBuildingSpec midas = GourdSpec(TownShopDressing.None, door: 0f, wallHeight: 3.3f);
            midas.WideDoor = true;
            midas.AccentColor = TownSwatch.PactolusGold;
            r.Lots.Add(Street("cabaca_de_midas", CityLotKind.House, CityArm.North, CityArm.East, 11f, 6.5f, 5f, 1.6f, "midas", null, midas));
            r.Lots.Add(Street("cabaca_n2", CityLotKind.House, CityArm.North, CityArm.West, 13f, 6f, 4.8f, 1.2f, null, null, GourdSpec(TownShopDressing.None, wallHeight: 2.9f)));

            // ---- West street (the way in): Marsyas' Flauta Dupla (the city's shop), the Cais da Barca; a hut across ----
            TownBuildingSpec flauta = GourdSpec(TownShopDressing.DoubleFlute, door: 0.18f, wallHeight: 3.1f);
            flauta.SignColor = TownSwatch.VineFlower;
            r.Lots.Add(Street("flauta_dupla", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "marsyas", CityShopCatalog.FlautaDupla, flauta));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("cabaca_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, GourdSpec(TownShopDressing.None, wallHeight: 2.9f)));

            // ---- East street: Gordias, the farmer of the knots (the second mission); a hut across ----
            r.Lots.Add(Street("cabaca_de_gordias", CityLotKind.House, CityArm.East, CityArm.North, 13f, 6.5f, 5f, 1.4f, "gordias", null,
                GourdSpec(TownShopDressing.None, door: -0.15f, wallHeight: 3.0f)));
            r.Lots.Add(Street("cabaca_e1", CityLotKind.House, CityArm.East, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, GourdSpec(TownShopDressing.None, wallHeight: 2.8f)));

            // ---- The clearing of the dance: the round stone stage (south-west of the centre, clear of the respawn), lamps ----
            r.PlazaProps.Add(At(CityPropKind.StoneStage, -3.6f, -3.9f, 0f, 1f, 0));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- The river of golden pebbles across the south of the valley and its branch across the North street (fords) ----
            r.PlazaProps.Add(Segment(CityPropKind.GoldenRiver, -17f, ValeRiverZ, 17f, ValeRiverZ));
            r.PlazaProps.Add(Segment(CityPropKind.GoldenRiver, -2.4f, 15.8f, 2.4f, 15.8f));

            // ---- Rows of vines behind the southern huts, a small stage in the southern clearing ----
            r.PlazaProps.Add(Segment(CityPropKind.VineRow, -16f, -10.8f, -8.5f, -10.8f));
            r.PlazaProps.Add(Segment(CityPropKind.VineRow, 8.5f, -10.8f, 16f, -10.8f));
            r.PlazaProps.Add(At(CityPropKind.StoneStage, 0f, -9.2f, 0f, 0.75f, 1));

            // ---- The grove round the edge: broad oaks (one with a double flute hung on a branch) ----
            r.PlazaProps.Add(At(CityPropKind.GroveOak, -12f, -17f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, -4.5f, -17.8f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, 4.5f, -17.8f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, 12f, -17f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, -14f, 13.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, 14.5f, 13f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, 5.5f, 18.5f, 0f, 0.85f, 0));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, -5.5f, 18.8f, 0f, 0.85f, 0));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, -18.5f, -6f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.GroveOak, 18.5f, -6f, 0f, 0.9f, 0));
            return r;
        }

        /// <summary>A gourd hut of the vale: wicker walls, a flower garland, vine-leaf cap, a gold accent.</summary>
        private static TownBuildingSpec GourdSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 3.0f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.Wicker, TownSwatch.VineFlower, TownSwatch.WoodWeathered, TownSwatch.PactolusGold, dressing: dressing,
                sign: TownSwatch.GladeGreen, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.VineLeaf);
            spec.GourdHut = true;
            spec.PlinthColor = TownSwatch.GladeStone;
            return spec;
        }
    }
}
