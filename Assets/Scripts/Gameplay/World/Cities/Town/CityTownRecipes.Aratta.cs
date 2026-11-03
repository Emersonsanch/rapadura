using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Aratta, the Cidade dos Chifres, race city of the povo-touro (Docs/Mapas100.md §3.3 and map 26; return city of maps
    /// 26–27), in its own style (<see cref="TownStyles.ReedHorn"/>): great halls of bound reed (mudhif) on red-clay platforms
    /// round a sand plaza with the bull-headed lyre (the Lira do Touro) and the low fire of the Fogo Vivo. Enmerkar's great
    /// hall closes the head of the plaza (the missions); Lugalbanda's Alforje (the city's shop) and the Cais da Barca on the
    /// West street; Shulgi the runner on the East street; Ensuhkeshdanna, the friendly rival, on the South street, the road
    /// in, where the horn posts with blue ribbons mark the start of the Corrida de Shulgi. Round dry-stone pens, acacias and
    /// a baobab between the streets. Everything stays inside a circle of radius <see cref="ArattaSolidRadius"/> at y = 0 (the
    /// place the Costa das Monções overworld keeps for it), with three roads out (W, E, S) ending at ±<see cref="ArattaArmLength"/>.
    /// Same conventions as the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 26–27 (= <c>MapCatalog.Aratta</c>, <see cref="CityCatalog.Aratta"/>).</summary>
        public const string ArattaCityId = "aratta";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float ArattaArmLength = 20f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float ArattaHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float ArattaSolidRadius = 22f;

        public static CityTownRecipe Aratta()
        {
            var r = new CityTownRecipe
            {
                CityId = ArattaCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.ReedHorn,
                PlazaRadius = 7f,
                StreetWidth = 3.6f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = ArattaHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, ArattaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, ArattaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, ArattaArmLength, exitToMap: true));

            // ---- Plaza head: Enmerkar's great reed hall (the missions) ----
            TownBuildingSpec hall = ReedSpec(TownShopDressing.None, wallHeight: 3.2f);
            hall.FullVaranda = true;
            hall.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "salao_de_enmerkar", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 9f, Depth = 6f, Porch = 1.8f, Spec = hall, ResidentId = "enmerkar",
            });

            // ---- West street: Lugalbanda's Alforje (the city's shop), the Cais da Barca ----
            TownBuildingSpec alforje = ReedSpec(TownShopDressing.Wanderer, door: 0.18f, wallHeight: 2.9f);
            alforje.SignColor = TownSwatch.Ribbon;
            r.Lots.Add(Street("alforje_lugalbanda", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "lugalbanda", CityShopCatalog.AlforjeLugalbanda, alforje));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 18f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, ReedSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street: Shulgi the runner ----
            r.Lots.Add(Street("casa_shulgi", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "shulgi", null,
                ReedSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.8f)));

            // ---- South street (the road in): Ensuhkeshdanna, the friendly rival; a plain hall across ----
            r.Lots.Add(Street("casa_ensuhkeshdanna", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "ensuhkeshdanna", null,
                ReedSpec(TownShopDressing.Drums, door: 0.15f, wallHeight: 3.0f)));
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, ReedSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The plaza: the Lira do Touro, the low fire of the Fogo Vivo, horn posts and lamps ----
            r.PlazaProps.Add(At(CityPropKind.BullLyre, -3.2f, 4.4f));
            r.PlazaProps.Add(At(CityPropKind.Bonfire, 3.6f, 3.8f, 0f, 0.8f));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            r.PlazaProps.Add(At(CityPropKind.HornPost, -5.6f, 9.4f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.HornPost, 5.6f, 9.4f, 0f, 1f, 1));

            // ---- The start of the Corrida de Shulgi: horn posts down both sides of the South street ----
            foreach (float z in new[] { -8f, -15f })
            {
                r.PlazaProps.Add(At(CityPropKind.HornPost, -2.25f, z, 0f, 1f, 0));
                r.PlazaProps.Add(At(CityPropKind.HornPost, 2.25f, z, 0f, 1f, 2));
            }

            // ---- Between the streets: round stone pens, acacias, the baobab behind the great hall ----
            r.PlazaProps.Add(At(CityPropKind.StoneKraal, 14f, -9f));
            r.PlazaProps.Add(At(CityPropKind.StoneKraal, -14f, -12.5f));
            r.PlazaProps.Add(At(CityPropKind.Acacia, 17.5f, 12f, 0f, 1f));
            r.PlazaProps.Add(At(CityPropKind.Acacia, -17.5f, 11.5f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.Acacia, 8f, -17.5f, 0f, 1.1f));
            r.PlazaProps.Add(At(CityPropKind.Tree, -9f, 16f, 0f, 1.5f, 1));
            r.PlazaProps.Add(At(CityPropKind.HayBale, 17f, -3.6f, 90f));
            r.PlazaProps.Add(At(CityPropKind.Jugs, 9.2f, -4f, 20f));
            r.PlazaProps.Add(At(CityPropKind.Bush, 8.5f, 15.5f, 0f, 1f));
            return r;
        }

        /// <summary>A reed hall of Aratta: bound-reed arches, reed-mat vault, red-clay platform and trims, ribbon-blue accents.</summary>
        private static TownBuildingSpec ReedSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 3.0f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.Reed, TownSwatch.RedClay, TownSwatch.Reed, TownSwatch.Ribbon, roof: TownRoofKind.Gable, dressing: dressing,
                sign: TownSwatch.Ribbon, door: door, wallHeight: wallHeight, windows: 0, roofColor: TownSwatch.ReedMat);
            spec.ReedHall = true;
            spec.PlinthColor = TownSwatch.RedClay;
            return spec;
        }
    }
}
