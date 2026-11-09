using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Cais de Mesektet, the quay of the night barque on the Rio das Doze Horas (Docs/Mapas100.md map 71, região 13; Kemet's
    /// river of the night), in its own style (<see cref="TownStyles.PapyrusQuay"/>): a floating quay town round the sandstone
    /// plaza where the Cais da Barca holds the Mesektet moored. Sinuhe's hall, the chief boatman (the missions), closes the head
    /// of the plaza with the river behind it; Khunanup's reed market (the city's shop: the gear of the hours 71–76) and the
    /// landing of the boats (Cais dos Barcos) on the West street, the road downriver; Djedi, the magician (the second mission),
    /// and a hall on the East street, out to the Campos de Juncos; Setne, the librarian of the Caderno, on the South street, the
    /// road in from the sandbanks, with a hall across; two piers of the hours (channels of dark water with plank bridges) cross
    /// the West and South streets; papyrus columns with oil lamps, thickets of papyrus and reed skiffs between the streets; no
    /// street to the north: there the bank drops to the dark river and its twelve piers, with thickets, skiffs and columns.
    /// Everything stays inside a circle of radius <see cref="MesektetSolidRadius"/> at y = 0 (the place the river overworld will
    /// keep for it), with three roads out (W, E, S) ending at ±<see cref="MesektetArmLength"/>. Same conventions as the other
    /// recipes (the plan of Hinokura, validated, with the river's own pieces).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of maps 71–76 (= <see cref="CityCatalog.CaisDeMesektet"/>; MapCatalog will use the same id).</summary>
        public const string MesektetCityId = "cais_de_mesektet";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float MesektetArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float MesektetHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float MesektetSolidRadius = 22.5f;

        /// <summary>Where the Cais da Barca (the Mesektet moored) stands on the plaza (north of the centre, clear of the respawn).</summary>
        public static readonly Vector2 MesektetBarque = new Vector2(0f, 4.2f);

        public static CityTownRecipe CaisDeMesektet()
        {
            var r = new CityTownRecipe
            {
                CityId = MesektetCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.PapyrusQuay,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = MesektetHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 10,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, MesektetArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, MesektetArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, MesektetArmLength, exitToMap: true));

            // ---- Plaza head: Sinuhe's hall, the chief boatman, with the river behind it (the missions) ----
            TownBuildingSpec sinuhe = QuaySpec(TownShopDressing.None, wallHeight: 3.1f);
            sinuhe.FullVaranda = true;
            sinuhe.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "salao_de_sinuhe", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = sinuhe, ResidentId = "sinuhe",
            });

            // ---- West street (downriver, to the Campos de Juncos' canals): Khunanup's reed market (the city's shop), the landing of the boats; a hall across ----
            TownBuildingSpec trocas = QuaySpec(TownShopDressing.ReedMarket, door: 0.18f, wallHeight: 2.9f);
            trocas.SignColor = TownSwatch.KemetGold;
            trocas.WallColor = TownSwatch.NileLinen;
            r.Lots.Add(Street("trocas_do_khunanup", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "khunanup", CityShopCatalog.TrocasKhunanup, trocas));
            r.Lots.Add(Street("cais_dos_barcos", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("salao_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, QuaySpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street (upriver): Djedi's hall, the magician (the second mission); a hall of linen ----
            r.Lots.Add(Street("salao_de_djedi", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "djedi", null,
                QuaySpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.9f)));
            TownBuildingSpec linen = QuaySpec(TownShopDressing.None, wallHeight: 2.6f);
            linen.WallColor = TownSwatch.NileLinen;
            linen.TrimColor = TownSwatch.RiverNight;
            r.Lots.Add(Street("salao_e2", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, null, null, linen));

            // ---- South street (the road in from the sandbanks): Setne, the librarian of the Caderno; a hall across ----
            r.Lots.Add(Street("salao_de_setne", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "setne", null,
                QuaySpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.0f, wide: true)));
            r.Lots.Add(Street("salao_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, QuaySpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The sandstone plaza: the Cais da Barca with the Mesektet (north of the centre), lamp posts at its corners ----
            r.PlazaProps.Add(At(CityPropKind.NightBarque, MesektetBarque.x, MesektetBarque.y, 0f, 1f));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Two piers of the hours crossing the West and South streets, each with its plank bridge and hour-lamps ----
            r.PlazaProps.Add(Segment(CityPropKind.HourCanal, -15.8f, -2.4f, -15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.HourCanal, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: papyrus thickets, papyrus columns and reed skiffs ----
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, -9.5f, 12.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, 9.5f, 12.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.PapyrusColumn, 13f, -6f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.ReedSkiff, 13.5f, -11f, 30f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, 16.5f, -6.5f, 0f, 0.8f, 0));
            r.PlazaProps.Add(At(CityPropKind.PapyrusColumn, -14.5f, -12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.PapyrusColumn, -18f, -5.5f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.ReedSkiff, -11f, -12.5f, -20f, 0.8f, 0));
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, 7.5f, -17.5f, 0f, 0.8f, 0));

            // ---- No street to the north: the bank drops to the dark river and its twelve piers ----
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, -7.5f, 19.2f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, 7.5f, 19.2f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.ReedSkiff, 0f, 19.8f, 0f, 1.2f, 0));
            r.PlazaProps.Add(At(CityPropKind.PapyrusColumn, -4f, 19.4f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.PapyrusColumn, 4f, 19.4f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.ReedSkiff, -13f, 14f, 60f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.ReedSkiff, 13f, 14f, -60f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, -16f, 11.5f, 0f, 0.7f, 0));
            r.PlazaProps.Add(At(CityPropKind.PapyrusThicket, 16f, 11.5f, 0f, 0.7f, 1));
            return r;
        }

        /// <summary>A hall of the Cais de Mesektet: sandstone on a raft, a gold cornice, papyrus columns, a roof of reed mats.</summary>
        private static TownBuildingSpec QuaySpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.9f, bool wide = false)
        {
            TownBuildingSpec spec = Spec(TownSwatch.QuaySandstone, TownSwatch.KemetGold, TownSwatch.WoodDark, TownSwatch.KemetGold, dressing: dressing,
                sign: TownSwatch.RiverNight, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.MesektetMat);
            spec.PapyrusHall = true;
            spec.WideDoor = wide;
            spec.PlinthColor = TownSwatch.QuaySandstone;
            return spec;
        }
    }
}
