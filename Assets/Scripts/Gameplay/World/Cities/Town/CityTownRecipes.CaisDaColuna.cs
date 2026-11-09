using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Cais da Coluna, the quay above the clouds where the Barcas de Luz moor (Docs/Mapas100.md map 91, região 16), in its own style
    /// (<see cref="TownStyles.SkyQuay"/>): pavilions of glass and gold, a round plaza with the five fountains of the elements, the pier of the Sun
    /// to the east and the pier of the Moon to the west with a barque alongside each, the foot of the Coluna da Glória to the north and gaps in the cloud where the
    /// 27 cities of the island show as little lights. Ammo's last caravan (the city's shop) comes in on the South street, Adda's pavilion stands on the pier of the Sun. The
    /// Remadores are props of light and never speak; the Pai da Grandeza does not appear (bible §0.2). Everything stays inside a circle of radius
    /// <see cref="CaisDaColunaSolidRadius"/> at y = 0, with three roads out (W, E, S) ending at ±<see cref="CaisDaColunaArmLength"/>. The postos of
    /// maps 92–100 are not towns (see Docs/Cities.md).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of maps 91–100 (= <see cref="CityCatalog.CaisDaColuna"/>; MapCatalog will use the same id).</summary>
        public const string CaisDaColunaCityId = "cais_da_coluna";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float CaisDaColunaArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float CaisDaColunaHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float CaisDaColunaSolidRadius = 22.5f;

        /// <summary>Where the five fountains of the elements stand on the plaza (Ar Sereno, Vento Vivo, Luz, Água Viva, Fogo Vivo): off the three roads and clear of Ammo's stall.</summary>
        public static readonly Vector2[] CaisDaColunaFountains =
        {
            new Vector2(0f, 5f), new Vector2(4.5f, 3.2f), new Vector2(3f, -3f), new Vector2(-3f, -3f), new Vector2(-4.5f, 3.2f),
        };

        /// <summary>The foot of the Coluna da Glória, north of the plaza (no street goes there).</summary>
        public static readonly Vector2 CaisDaColunaColumn = new Vector2(0f, 15.5f);

        public static CityTownRecipe CaisDaColuna()
        {
            var r = new CityTownRecipe
            {
                CityId = CaisDaColunaCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.SkyQuay,
                PlazaRadius = 8f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = CaisDaColunaHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 8,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, CaisDaColunaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, CaisDaColunaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, CaisDaColunaArmLength, exitToMap: true));

            // ---- South street (the road the last caravan comes in by): Ammo's counter; a pavilion across ----
            r.Lots.Add(Street("ultima_caravana", CityLotKind.Shop, CityArm.South, CityArm.East, 12.5f, 6.5f, 5f, 1.4f, "ammo_caravaneiro", CityShopCatalog.UltimaCaravanaAmmo,
                PavilionSpec(TownShopDressing.LastCaravan, door: 0.15f, wallHeight: 3.0f, wide: true, sign: TownSwatch.ElementFire)));
            r.Lots.Add(Street("pavilhao_s2", CityLotKind.House, CityArm.South, CityArm.West, 12.5f, 6f, 4.8f, 1.2f, null, null, PavilionSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The pier of the Sun (east): Adda's pavilion on the north side; the Barca do Sol alongside the south side ----
            r.Lots.Add(Street("pavilhao_do_adda", CityLotKind.House, CityArm.East, CityArm.North, 11.5f, 6f, 4.6f, 1.4f, "adda_roda", null, PavilionSpec(TownShopDressing.None, door: -0.15f, wallHeight: 3.0f)));

            // ---- The pier of the Moon (west): a pavilion on the north side; the Barca da Lua alongside the south side ----
            r.Lots.Add(Street("pavilhao_da_lua", CityLotKind.House, CityArm.West, CityArm.North, 11.5f, 6f, 4.6f, 1.4f, null, null, PavilionSpec(TownShopDressing.None, door: 0.15f, wallHeight: 2.9f)));

            // ---- Two small pavilions on the cloud on either side of the Coluna ----
            r.Lots.Add(new CityLotRecipe { Id = "pavilhao_n1", Kind = CityLotKind.House, Free = true, Center = new Vector2(-5.5f, 13f), Front = Vector2.down, Width = 5f, Depth = 4.2f, Porch = 0.8f, Spec = PavilionSpec(TownShopDressing.None, wallHeight: 2.7f) });
            r.Lots.Add(new CityLotRecipe { Id = "pavilhao_n2", Kind = CityLotKind.House, Free = true, Center = new Vector2(5.5f, 13f), Front = Vector2.down, Width = 5f, Depth = 4.2f, Porch = 0.8f, Spec = PavilionSpec(TownShopDressing.None, wallHeight: 2.7f) });

            // ---- The plaza: the five fountains of the elements, the lamp posts round them ----
            for (int i = 0; i < CaisDaColunaFountains.Length; i++)
            {
                r.PlazaProps.Add(At(CityPropKind.ElementFountain, CaisDaColunaFountains[i].x, CaisDaColunaFountains[i].y, 0f, 1f, i));
            }

            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- North: the foot of the Coluna da Glória ----
            r.PlazaProps.Add(At(CityPropKind.ColumnOfGlory, CaisDaColunaColumn.x, CaisDaColunaColumn.y));

            // ---- The two piers of solid light, the barges moored south of them, and the Remadores (silhouettes of light, no speech) on deck ----
            r.PlazaProps.Add(At(CityPropKind.LightPier, 13.5f, 0f));
            r.PlazaProps.Add(At(CityPropKind.LightPier, -13.5f, 0f));
            r.PlazaProps.Add(At(CityPropKind.SkyBarge, 14.5f, -6f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.SkyBarge, -14.5f, -6f, 0f, 1f, 1));
            foreach (float x in new[] { 11.8f, 14.2f, 17f, -11.8f, -14.2f, -17f })
            {
                r.PlazaProps.Add(At(CityPropKind.LightRower, x, -6f, x > 0f ? 90f : -90f, 1f, 1));
            }

            // ---- Over the edge: the 27 cities of the island far below, through two gaps in the cloud ----
            r.PlazaProps.Add(At(CityPropKind.IslandLightsGap, 14.5f, -14.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.IslandLightsGap, -14.5f, -14.5f, 0f, 1f, 1));

            // ---- Banks of cloud round the edge (walked through) ----
            float[,] puffs =
            {
                { 18f, 9f }, { -18f, 9f }, { 12f, 17f }, { -12f, 17f }, { 20f, -9f }, { -20f, -9f }, { 8f, -19f }, { -8f, -19f }, { 19f, 15.5f }, { -19f, 15.5f }, { 0f, 20.5f },
            };
            for (int i = 0; i < puffs.GetLength(0); i++)
            {
                r.PlazaProps.Add(At(CityPropKind.CloudPuff, puffs[i, 0], puffs[i, 1], i * 37f, 1f, i % 2));
            }

            return r;
        }

        /// <summary>A pavilion of the Cais: glass panes between gold mullions on a plinth of cloud, a low lilac dome and a porch under a lilac awning.</summary>
        private static TownBuildingSpec PavilionSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.9f, bool wide = false, TownSwatch sign = TownSwatch.QuaySkyBlue)
        {
            TownBuildingSpec spec = Spec(TownSwatch.QuayGlassPane, TownSwatch.QuayGold, TownSwatch.QuayGold, TownSwatch.SolidLight, dressing: dressing,
                sign: sign, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.QuayLilac);
            spec.SkyPavilion = true;
            spec.WideDoor = wide;
            spec.PlinthColor = TownSwatch.QuayCloud;
            return spec;
        }
    }
}
