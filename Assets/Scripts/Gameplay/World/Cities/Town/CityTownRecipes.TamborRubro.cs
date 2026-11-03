using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Tambor Rubro, the race city of the orcs (Docs/Mapas100.md §3.3 and map 11; return city of maps 11–12), in its own
    /// style (<see cref="TownStyles.WarCamp"/>): a war camp inside a palisade of sharpened logs. The Salão Grande (a log
    /// hall with a very steep hide roof and crossed ridge poles) closes the head of the plaza, where Gurparanzah gives the
    /// missions; Ashmunikal's house of drums on the East street with the drum tower behind it (the city's guardian lamp on
    /// its peak); Tushratta's Forja de Tambor and the Cais da Barca on the West street; Kessi the hunter at the end of the
    /// East street; Appu, who adopts every orphan of the camp, on the South street. Clan tents of ochre-dyed hide, clan
    /// posts, fire pits and the sand training ring fill the quarters between the streets. Everything stays inside ±33 m
    /// at y = 0 (the place the Areias overworld keeps for the city: safe zone ±34, flat ground ±38). Same conventions as
    /// the other recipes (x = east, z = north; fronts towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 11–12 (= <c>MapCatalog.TamborRubro</c>, <see cref="CityCatalog.TamborRubro"/>).</summary>
        public const string TamborRubroCityId = "tambor_rubro";

        /// <summary>Half side of the palisade square (its logs stand on this line, the gaps let the three streets out).</summary>
        public const float TamborRubroPalisade = 32.5f;

        /// <summary>Where the drum tower stands (behind Ashmunikal's house): the city's guardian lamp is on its peak.</summary>
        public static readonly Vector2 TamborRubroDrumTower = new Vector2(12.5f, 13.8f);

        public static CityTownRecipe TamborRubro()
        {
            var r = new CityTownRecipe
            {
                CityId = TamborRubroCityId,
                Region = BiomeType.Desert,
                Style = TownStyles.WarCamp,
                PlazaRadius = 8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = 36f,
                AutoYards = false,
                NightLights = 8,
                LampSpacing = 9f,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.South, 30f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 26f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.West, 26f, exitToMap: true));

            // ---- Plaza head: the Salão Grande (Gurparanzah, the missions), war drums at its corners ----
            TownBuildingSpec salao = Hall(TownSwatch.RedClay, TownShopDressing.Drums, wallHeight: 3.6f);
            salao.FullVaranda = true;
            salao.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "salao_grande", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 15f), Front = Vector2.down,
                Width = 12f, Depth = 6.5f, Porch = 2f, Spec = salao, ResidentId = "gurparanzah",
            });
            r.PlazaProps.Add(At(CityPropKind.WarDrum, -7.4f, 9f, 0f, 1f));
            r.PlazaProps.Add(At(CityPropKind.WarDrum, 7.4f, 9f, 0f, 1f));

            // ---- West street: Tushratta's Forja de Tambor (the city's shop), the Cais da Barca ----
            TownBuildingSpec forja = Hall(TownSwatch.Laterite, TownShopDressing.Forja, door: 0.18f, wallHeight: 3.2f);
            forja.Chimney = true;
            forja.SignColor = TownSwatch.ClothYellow;
            r.Lots.Add(Street("forja_de_tambor", CityLotKind.Shop, CityArm.West, CityArm.North, 12.5f, 7.5f, 5.5f, 1.8f, "tushratta", CityShopCatalog.ForjaTambor, forja));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 21.5f, 6.5f, 3.2f, 1.0f));
            r.PlazaProps.Add(At(CityPropKind.Crates, -17.3f, 9.4f, 15f));

            // ---- East street: Ashmunikal's house of drums with the drum tower behind it, Kessi the hunter ----
            r.Lots.Add(Street("casa_dos_tambores", CityLotKind.House, CityArm.East, CityArm.North, 12.5f, 7f, 5f, 1.6f, "ashmunikal", null,
                Hall(TownSwatch.AdobeOchre, TownShopDressing.Drums, door: -0.15f)));
            r.PlazaProps.Add(At(CityPropKind.DrumTower, TamborRubroDrumTower.x, TamborRubroDrumTower.y));
            r.Lots.Add(Street("casa_kessi", CityLotKind.House, CityArm.East, CityArm.North, 21.5f, 6.5f, 5f, 1.4f, "kessi", null,
                Hall(TownSwatch.Laterite, TownShopDressing.HuntingRack, door: 0.15f, wallHeight: 2.8f)));

            // ---- South street (the road in from the badlands): Appu and his orphans, a plain hall ----
            r.Lots.Add(Street("casa_appu", CityLotKind.House, CityArm.South, CityArm.East, 12.5f, 7f, 5f, 1.6f, "appu", null,
                Hall(TownSwatch.RedClay, TownShopDressing.GeneralStore, door: 0.15f)));
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 21.5f, 6f, 4.8f, 1.0f, null, null,
                Hall(TownSwatch.AdobeOchre, wallHeight: 2.8f)));

            // ---- Clan tents of dyed hide between the streets (fronts to the nearest street) ----
            float south = CityGeometry.YawFacing(Vector2.down);
            float north = CityGeometry.YawFacing(Vector2.up);
            float east = CityGeometry.YawFacing(Vector2.right);
            float west = CityGeometry.YawFacing(Vector2.left);
            r.PlazaProps.Add(At(CityPropKind.ClanTent, -12f, -8f, north, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, -20.5f, -8.5f, north, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, 12f, -8f, north, 1f, 2));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, 20.5f, -8.5f, north, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, -8.5f, -14.5f, east, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, 16f, -20f, west, 1f, 2));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, 23f, -26f, west, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, -22f, 16f, south, 1f, 2));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, -13f, 20f, south, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ClanTent, 22f, 16f, south, 1f, 1));

            // ---- The training arena of sand (south-west), fire pits, clan posts ----
            r.PlazaProps.Add(At(CityPropKind.TrainingRing, -17f, -20f));
            r.PlazaProps.Add(At(CityPropKind.ClanPost, -10.6f, -20f, east, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ClanPost, -23.4f, -20f, west, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Bonfire, -3.8f, -4.2f));
            r.PlazaProps.Add(At(CityPropKind.Bonfire, -24f, -27.5f));
            r.PlazaProps.Add(At(CityPropKind.Bonfire, 0f, 24f, 0f, 1.2f));
            r.PlazaProps.Add(At(CityPropKind.Bonfire, 15f, -27f));
            r.PlazaProps.Add(At(CityPropKind.ClanPost, -3.8f, 6.6f, 0f, 1f, 2));
            r.PlazaProps.Add(At(CityPropKind.ClanPost, 3.8f, 6.6f, 0f, 1f, 0));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            r.PlazaProps.Add(At(CityPropKind.Jugs, 6.2f, 2.4f, 30f));
            r.PlazaProps.Add(At(CityPropKind.Tree, -27f, 26f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Tree, 27.5f, 24f, 40f, 1f, 2));
            r.PlazaProps.Add(At(CityPropKind.Tree, 27f, -15f, 80f, 0.9f, 1));

            // ---- The palisade: sharpened logs all round, gaps for the three streets ----
            float p = TamborRubroPalisade;
            const float gap = 4.6f;
            r.PlazaProps.Add(Segment(CityPropKind.Palisade, -p, p, p, p));
            r.PlazaProps.Add(Segment(CityPropKind.Palisade, -p, -p, -gap, -p));
            r.PlazaProps.Add(Segment(CityPropKind.Palisade, gap, -p, p, -p));
            r.PlazaProps.Add(Segment(CityPropKind.Palisade, -p, -p, -p, -gap));
            r.PlazaProps.Add(Segment(CityPropKind.Palisade, -p, gap, -p, p));
            r.PlazaProps.Add(Segment(CityPropKind.Palisade, p, -p, p, -gap));
            r.PlazaProps.Add(Segment(CityPropKind.Palisade, p, gap, p, p));
            return r;
        }

        /// <summary>A log hall of the war camp: hide walls of <paramref name="wall"/>, teak hide roof, dark logs, yellow cloth.</summary>
        private static TownBuildingSpec Hall(TownSwatch wall, TownShopDressing dressing = TownShopDressing.None, float door = 0f, float wallHeight = 3.0f)
        {
            TownBuildingSpec spec = Spec(wall, TownSwatch.Teak, TownSwatch.WoodDark, TownSwatch.ClothYellow, roof: TownRoofKind.Gable, dressing: dressing,
                sign: TownSwatch.ClothYellow, door: door, wallHeight: wallHeight, windows: 1, roofColor: TownSwatch.Teak);
            spec.LogHall = true;
            spec.PlinthColor = TownSwatch.StoneDark;
            return spec;
        }
    }
}
