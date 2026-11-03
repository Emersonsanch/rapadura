using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Salões de Bronze de Napirasu, the race city of the dwarves (Docs/Mapas100.md §3.3 and map 17; return city of maps
    /// 17–18), in its own style (<see cref="TownStyles.RuneHall"/>): a hall cut into the Serra de Kaggen. Napirasu's hall of
    /// the forge closes the head of the plaza (a granite front with a massive bronze door under a rune lintel and square
    /// pillars capped in bronze); Puzur's Portões de Runa (the city's shop) and the Cais da Barca on the West street; Untash
    /// the master builder and Kidin's kitchen on the East street; Shilhak the miner on the South street. The Grande Forja,
    /// gone cold, stands in the south-east hall with a pit of cold lava beside it; chain lifts, pits and bronze pillars fill
    /// the rest, and the rock wall of the mountain closes the north and the east (the side away from the camera). Everything
    /// stays inside ±19 m at y = 0 (the place the Kaggen overworld keeps for the city: safe zone x −20…21, z −19…20, flat
    /// ground ±24), with the same three roads out (West and East meet the overworld's trails, South as before). Same
    /// conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 17–18 (= <c>MapCatalog.SaloesDeBronze</c>, <see cref="CityCatalog.SaloesDeBronze"/>).</summary>
        public const string BronzeHallsCityId = "saloes_de_bronze";

        /// <summary>Half side of the walkable square (inside the overworld's safe zone x −20…21, z −19…20).</summary>
        public const float BronzeHallsHalfExtent = 19f;

        /// <summary>Where the cold Grande Forja stands (south-east hall), its front to the camera.</summary>
        public static readonly Vector2 BronzeHallsGreatForge = new Vector2(13.5f, -10f);

        public static CityTownRecipe BronzeHalls()
        {
            var r = new CityTownRecipe
            {
                CityId = BronzeHallsCityId,
                Region = BiomeType.Mountain,
                Style = TownStyles.RuneHall,
                PlazaRadius = 6f,
                StreetWidth = 4f,
                SidewalkWidth = 1f,
                EntryArm = CityArm.East,
                WalkHalfExtent = BronzeHallsHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
                AmbientNight = 0.6f,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 18f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 18f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 17f, exitToMap: true));

            // ---- Plaza head: Napirasu's hall of the forge (the master founder), the widest bronze door of the city ----
            TownBuildingSpec forja = RuneHallSpec(TownShopDressing.Forja, wallHeight: 3.6f);
            forja.Chimney = true;
            forja.WideDoor = true;
            forja.FullVaranda = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "salao_da_forja", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12f), Front = Vector2.down,
                Width = 9f, Depth = 5f, Porch = 1.6f, Spec = forja, ResidentId = "napirasu",
            });

            // ---- West street: Puzur's Portões de Runa (the city's shop), the Cais da Barca ----
            TownBuildingSpec runas = RuneHallSpec(TownShopDressing.Runes, wallHeight: 3.3f);
            runas.SignColor = TownSwatch.Bronze;
            r.Lots.Add(Street("portoes_de_runa", CityLotKind.Shop, CityArm.West, CityArm.North, 10f, 7f, 4.5f, 1.6f, "puzur", CityShopCatalog.PortoesRuna, runas));
            r.Lots.Add(Street("cais_dos_saloes", CityLotKind.ShipDock, CityArm.West, CityArm.North, 16.2f, 5f, 3f, 1f));

            // ---- East street: Untash the master builder, Kidin's kitchen at the gate ----
            r.Lots.Add(Street("casa_untash", CityLotKind.House, CityArm.East, CityArm.North, 9.5f, 6.5f, 4.5f, 1.6f, "untash", null,
                RuneHallSpec(TownShopDressing.Marcenaria, wallHeight: 3.2f, wall: TownSwatch.StoneDark)));
            TownBuildingSpec cozinha = RuneHallSpec(TownShopDressing.Mercearia, wallHeight: 2.9f);
            cozinha.Chimney = true;
            r.Lots.Add(Street("cozinha_do_salao", CityLotKind.House, CityArm.East, CityArm.North, 15.85f, 4.6f, 4f, 1.2f, "kidin", null, cozinha));

            // ---- South street: Shilhak the miner (pick, rope and lamp at his door) ----
            r.Lots.Add(Street("casa_shilhak", CityLotKind.House, CityArm.South, CityArm.East, 10f, 6f, 4.5f, 1.4f, "shilhak", null,
                RuneHallSpec(TownShopDressing.OldMiner, wallHeight: 3.0f, wall: TownSwatch.StoneDark)));

            // ---- The Grande Forja, gone cold, in the south-east hall; cold lava beside it; crates of ore waiting ----
            r.PlazaProps.Add(At(CityPropKind.GreatForge, BronzeHallsGreatForge.x, BronzeHallsGreatForge.y, CityGeometry.YawFacing(Vector2.down)));
            r.PlazaProps.Add(At(CityPropKind.LavaPit, 13.5f, -15.6f));
            r.PlazaProps.Add(At(CityPropKind.Crates, 10f, -6.5f, 15f));
            r.PlazaProps.Add(At(CityPropKind.Jugs, 16.8f, -6.4f, 40f));

            // ---- The south-west hall: a pit of cold lava, a chain lift down to the mines, pillars ----
            r.PlazaProps.Add(At(CityPropKind.LavaPit, -9f, -9.5f));
            r.PlazaProps.Add(At(CityPropKind.ChainLift, -14.5f, -9.5f));
            r.PlazaProps.Add(At(CityPropKind.BronzePillar, -9f, -15.5f));
            r.PlazaProps.Add(At(CityPropKind.BronzePillar, -15f, -15.5f));

            // ---- Behind the north halls: the chain lift up to the mines, pillars along the rock ----
            r.PlazaProps.Add(At(CityPropKind.ChainLift, 12.5f, 14.5f));
            foreach (float x in new[] { -15f, -9f, 8f })
            {
                r.PlazaProps.Add(At(CityPropKind.BronzePillar, x, 14.5f));
            }

            r.PlazaProps.Add(At(CityPropKind.BronzePillar, -17.5f, 11f));

            // ---- The plaza: four bronze pillars with lit runes, lamps between them ----
            foreach (float angle in new[] { 55f, 125f, 235f, 305f })
            {
                Vector2 at = Polar(angle, 7.8f);
                r.PlazaProps.Add(At(CityPropKind.BronzePillar, at.x, at.y));
            }

            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.6f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // Street lamps on the curb opposite the doors (by hand: none in front of a door).
            foreach (Vector2 at in new[] { new Vector2(-10f, -2.3f), new Vector2(-15.5f, -2.3f), new Vector2(10f, -2.3f), new Vector2(15.5f, -2.3f), new Vector2(-2.3f, -10f), new Vector2(-2.3f, -15f) })
            {
                Vector2 toRoad = Mathf.Abs(at.y) < 3f ? Vector2.up : Vector2.right;
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = at, Yaw = CityGeometry.YawFacing(toRoad) });
            }

            // ---- The mountain's rock wall: north and east (away from the camera), a gap for the East road ----
            const float north = 18.4f;
            const float east = 18.95f;
            r.PlazaProps.Add(Segment(CityPropKind.RockWall, -18.9f, north, east, north));
            r.PlazaProps.Add(Segment(CityPropKind.RockWall, east, 3.6f, east, north));
            r.PlazaProps.Add(Segment(CityPropKind.RockWall, east, -18.3f, east, -3.6f));
            return r;
        }

        /// <summary>A hall front cut into the granite: bronze cornice and door, rune lintel, square pillars (TownKit.RuneHall).</summary>
        private static TownBuildingSpec RuneHallSpec(TownShopDressing dressing = TownShopDressing.None, float door = 0f, float wallHeight = 3.2f, TownSwatch wall = TownSwatch.Granite)
        {
            TownBuildingSpec spec = Spec(wall, TownSwatch.Bronze, TownSwatch.Bronze, TownSwatch.RuneLight, roof: TownRoofKind.Rock, dressing: dressing,
                sign: TownSwatch.Bronze, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.Granite);
            spec.RuneHall = true;
            spec.PlinthColor = TownSwatch.StoneDark;
            return spec;
        }
    }
}
