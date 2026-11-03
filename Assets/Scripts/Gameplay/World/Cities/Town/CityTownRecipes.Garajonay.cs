using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Copas de Garajonay, the race city of the elfos-da-mata (Docs/Mapas100.md §3.3 and map 05): round plank houses with
    /// layered laurel-leaf roofs on the forest floor, and the <b>canopy</b> above them: giant laurels each carrying a ring
    /// platform with a round house (<see cref="CityPropKind.CanopyTree"/>), linked by rope bridges at the same height, a
    /// spiral stair round every trunk. Same conventions as the other recipes (x = east, z = north; fronts towards the
    /// isometric camera; the tall canopy stands north of the streets and at the far ends, the south side stays low).
    /// A <see cref="CityCatalog.RaceCities"/> city since 2026-11-03: the same recipe is its walkable town (the city walk, the
    /// respawn of maps 05–06) and the settlement of its map (MapLayouts.Zarafshan, the overworld). Its five residents stand
    /// at their own doors (Gara at her hall, Dácil at her herb house with the city's shop, Jonay, Bencomo, Tinguaro);
    /// giving them a keeper changes no footprint.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 05–06 (= <c>MapCatalog.CopasGarajonay</c>).</summary>
        public const string GarajonayCityId = "copas_garajonay";

        /// <summary>Scale of the canopy trees and their rope bridges (platforms at ~8.8 m).</summary>
        public const float GarajonayCanopyScale = 1.4f;

        /// <summary>The canopy trees of the city, west to east (each pair of neighbours is linked by a rope bridge).</summary>
        public static readonly Vector2[] GarajonayCanopy =
        {
            new Vector2(-50f, 14f), new Vector2(-30f, 24f), new Vector2(-12f, 28f), new Vector2(12f, 28f), new Vector2(30f, 24f), new Vector2(48f, 14f),
        };

        public static CityTownRecipe Garajonay()
        {
            var r = new CityTownRecipe
            {
                CityId = GarajonayCityId,
                Region = BiomeType.Forest,
                PlazaRadius = 8f,
                EntryArm = CityArm.South,
                AutoYards = false,
                NightLights = 8,
                LampStrings = true,
                LampSpacing = 7f,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 34f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 30f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 30f, exitToMap: true));

            // ---- Plaza head: Gara's hall and the Coletor de Névoa ----
            TownBuildingSpec hall = Round(TownSwatch.Walnut, TownSwatch.ClothGreen, TownSwatch.Turquoise, wallHeight: 3.2f);
            hall.FullVaranda = true;
            hall.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "salao_de_gara", Kind = CityLotKind.Workshop, Free = true, Center = new Vector2(0f, 12.5f), Front = Vector2.down,
                Width = 8f, Depth = 5f, Porch = 1.8f, Spec = hall, ResidentId = "gara",
            });

            // ---- West street (Dácil the herbalist, Jonay the archer, the Cais da Barca) ----
            r.Lots.Add(Street("casa_dacil", CityLotKind.House, CityArm.West, CityArm.North, 12.5f, 6.5f, 5f, 1.4f, "dacil", CityShopCatalog.ErvariaDacil,
                Round(TownSwatch.WoodDark, TownSwatch.ClothGreen, TownSwatch.Tamarisk, door: 0.15f)));
            r.Lots.Add(Street("casa_jonay", CityLotKind.House, CityArm.West, CityArm.North, 20.5f, 6.5f, 5f, 1.3f, "jonay", null,
                Round(TownSwatch.Walnut, TownSwatch.LeafDark, TownSwatch.ClothIndigo)));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 28.5f, 7f, 3.2f, 1.0f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 12.5f, 6f, 4.8f, 1.0f, null, null,
                Round(TownSwatch.WoodWeathered, TownSwatch.ClothGreen, TownSwatch.Turquoise, wallHeight: 2.6f)));
            r.Lots.Add(Street("casa_w2", CityLotKind.House, CityArm.West, CityArm.South, 22f, 6f, 4.8f, 1.0f, null, null,
                Round(TownSwatch.Walnut, TownSwatch.LeafDark, TownSwatch.Tamarisk, wallHeight: 2.6f)));

            // ---- East street (Bencomo the old chief, Tinguaro the tracker) ----
            r.Lots.Add(Street("casa_bencomo", CityLotKind.House, CityArm.East, CityArm.North, 12.5f, 7f, 5.2f, 1.4f, "bencomo", null,
                Round(TownSwatch.Walnut, TownSwatch.Gold, TownSwatch.Turquoise, door: -0.15f)));
            r.Lots.Add(Street("casa_tinguaro", CityLotKind.House, CityArm.East, CityArm.North, 20.5f, 6.5f, 5f, 1.3f, "tinguaro", null,
                Round(TownSwatch.WoodDark, TownSwatch.LeafDark, TownSwatch.ClothRed)));
            r.Lots.Add(Street("casa_e1", CityLotKind.House, CityArm.East, CityArm.South, 15f, 6f, 4.8f, 1.0f, null, null,
                Round(TownSwatch.WoodWeathered, TownSwatch.ClothGreen, TownSwatch.Tamarisk, wallHeight: 2.6f)));

            // ---- South street (the road in from the forest floor) ----
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 12.5f, 6f, 4.8f, 1.0f, null, null,
                Round(TownSwatch.Walnut, TownSwatch.ClothGreen, TownSwatch.Turquoise, wallHeight: 2.6f)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 15f, 6f, 4.8f, 1.0f, null, null,
                Round(TownSwatch.WoodDark, TownSwatch.LeafDark, TownSwatch.Tamarisk, wallHeight: 2.6f)));
            r.Lots.Add(Street("casa_s3", CityLotKind.House, CityArm.South, CityArm.East, 22f, 6f, 4.8f, 1.0f, null, null,
                Round(TownSwatch.WoodWeathered, TownSwatch.Gold, TownSwatch.ClothIndigo, wallHeight: 2.6f)));

            // ---- The canopy: giant laurels with ring platforms, rope bridges between neighbours at platform height ----
            float s = GarajonayCanopyScale;
            for (int i = 0; i < GarajonayCanopy.Length; i++)
            {
                Vector2 p = GarajonayCanopy[i];
                r.PlazaProps.Add(At(CityPropKind.CanopyTree, p.x, p.y, 0f, s));
                if (i + 1 < GarajonayCanopy.Length)
                {
                    r.PlazaProps.Add(CanopyBridge(p, GarajonayCanopy[i + 1], s));
                }
            }

            // ---- Plaza: the mist collector's basin, benches, lamps, ferns and heather ----
            // The basin at the head of the plaza, before Gara's hall: the south-east stays clear for Ammo's stall (the validator
            // runs every city with the caravaneer too, and at (4.2, -3.8) the basin stood on his customers' spot).
            r.PlazaProps.Add(At(CityPropKind.Well, 0f, 5.2f, 0f, 0.9f));
            AddPlazaBench(r, 135f, 6.4f);
            AddPlazaBench(r, 45f, 6.4f);
            AddPlazaBench(r, 235f, 6.4f); // clear of the South road (at 250° it stood on it)
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            r.PlazaProps.Add(At(CityPropKind.Bush, -6.6f, 5.4f, 0f, 1.1f));
            r.PlazaProps.Add(At(CityPropKind.Bush, 6.8f, 5.0f, 40f, 1.0f));
            r.PlazaProps.Add(At(CityPropKind.Bush, -5.6f, -6.2f, 80f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.FlowerBed, -3.2f, -6.8f, 0f, 1f, 1));
            return r;
        }

        /// <summary>A rope bridge from platform rim to platform rim between two canopy trees of scale <paramref name="s"/>.</summary>
        private static CityProp CanopyBridge(Vector2 a, Vector2 b, float s)
        {
            Vector2 d = (b - a).normalized;
            float rim = TownKit.CanopyPlatformRadius * s - 0.4f;
            Vector2 from = a + d * rim;
            Vector2 to = b - d * rim;
            return Segment(CityPropKind.RopeBridge, from.x, from.y, to.x, to.y, s);
        }

        /// <summary>A round plank house of the elves on the forest floor: wooden walls, a cone of laurel leaves.</summary>
        private static TownBuildingSpec Round(TownSwatch wall, TownSwatch trim, TownSwatch accent, float door = 0f, float wallHeight = 2.9f)
        {
            TownBuildingSpec spec = Spec(wall, trim, TownSwatch.WoodDark, accent, roof: TownRoofKind.Cone, door: door, wallHeight: wallHeight, roofColor: TownSwatch.LeafDark);
            spec.WoodenWalls = true;
            return spec;
        }
    }
}
