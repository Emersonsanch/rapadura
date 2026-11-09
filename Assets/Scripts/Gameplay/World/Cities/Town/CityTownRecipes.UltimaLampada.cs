using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The Fortaleza da Última Lâmpada, the fortress of the guardians of the lamps on the edge of the Land of Darkness (Docs/Mapas100.md
    /// map 83, região 15), in its own style (<see cref="TownStyles.BasaltFort"/>): keeps of dark basalt round the plaza, the Lâmpada-Mãe
    /// Grande on the plaza north of the respawn, and to the north the arc of the rampart over the abyss with its thousand niches and the
    /// five gates of the pits. Tehat's wick workshop (the city's shop: the gear of maps 83–90) on the West street, the house of Pamour, the
    /// commander of the lamps (the missions), on the East street, the house of letters of Makarios on the South street, the road up from
    /// the Grutas do Limiar. Everything stays inside a circle of radius <see cref="UltimaLampadaSolidRadius"/> at y = 0 (the place the
    /// overworld will keep for it), with three roads out (W, E, S) ending at ±<see cref="UltimaLampadaArmLength"/>. The five gates of the
    /// ficha are the sealed gates of the rampart for now; the postos of maps 84–90 are not towns (see Docs/Cities.md). Same conventions
    /// as the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of maps 83–90 (= <see cref="CityCatalog.FortalezaUltimaLampada"/>; MapCatalog will use the same id).</summary>
        public const string FortalezaUltimaLampadaCityId = "fortaleza_ultima_lampada";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float UltimaLampadaArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float UltimaLampadaHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float UltimaLampadaSolidRadius = 22.5f;

        /// <summary>Where the Lâmpada-Mãe Grande stands on the plaza (north of the centre, clear of the respawn).</summary>
        public static readonly Vector2 UltimaLampadaMotherLamp = new Vector2(0f, 3.9f);

        public static CityTownRecipe FortalezaUltimaLampada()
        {
            var r = new CityTownRecipe
            {
                CityId = FortalezaUltimaLampadaCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.BasaltFort,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = UltimaLampadaHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 12,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, UltimaLampadaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, UltimaLampadaArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, UltimaLampadaArmLength, exitToMap: true));

            // ---- West street: Tehat's wick workshop (the city's shop); a keep across ----
            TownBuildingSpec workshop = BasaltSpec(TownShopDressing.WickLoom, door: 0.18f, wallHeight: 2.9f);
            workshop.SignColor = TownSwatch.ThousandLamps;
            r.Lots.Add(Street("oficina_de_pavios", CityLotKind.Shop, CityArm.West, CityArm.North, 10.5f, 6f, 4.6f, 1.4f, "tehat_pavios", CityShopCatalog.PaviosTehat, workshop));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, BasaltSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street: the house of Pamour (the missions), right under the rampart; a keep across ----
            r.Lots.Add(Street("casa_de_pamour", CityLotKind.House, CityArm.East, CityArm.North, 10.5f, 6f, 4.6f, 1.4f, "pamour_lampadas", null,
                BasaltSpec(TownShopDressing.None, door: -0.15f, wallHeight: 3.0f)));
            r.Lots.Add(Street("casa_e1", CityLotKind.House, CityArm.East, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, BasaltSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- South street (the road up from the Grutas do Limiar): the house of letters of Makarios; a keep across ----
            r.Lots.Add(Street("casa_das_cartas", CityLotKind.House, CityArm.South, CityArm.East, 12.5f, 6.5f, 5f, 1.4f, "makarios_cartas", null,
                BasaltSpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.0f, wide: true)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, BasaltSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The Lâmpada-Mãe Grande on the plaza, the lamp posts round it ----
            r.PlazaProps.Add(At(CityPropKind.MotherLamp, UltimaLampadaMotherLamp.x, UltimaLampadaMotherLamp.y));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- The arc of the rampart over the abyss, and the five gates of the pits set into it, one for each dark element ----
            r.PlazaProps.Add(At(CityPropKind.LampRampart, 0f, 0f));
            float[] gates = { 150f, 120f, 90f, 60f, 30f };
            for (int i = 0; i < gates.Length; i++)
            {
                Vector2 at = Polar(gates[i], TownKit.LampRampartRadius - TownKit.LampRampartHalfThickness - 0.05f);
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.CoveGate, Position = at, Yaw = CityGeometry.YawFacing(-at), Variant = i });
            }

            // ---- Inside the arc: the niche steles and the oil, in front of the gates ----
            r.PlazaProps.Add(At(CityPropKind.NicheStele, -5.5f, 11.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.NicheStele, 5.5f, 11.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.NicheStele, 0f, 14f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.OilCistern, -3f, 9.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.OilCistern, 3f, 9.5f, 0f, 1f, 0));

            // ---- Between the streets: Tehat's racks of wicks, cisterns and steles ----
            r.PlazaProps.Add(At(CityPropKind.WickRack, -11.5f, -12.5f, 15f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.WickRack, -15.5f, -11f, -30f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.WickRack, 15.5f, -12f, 20f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.OilCistern, 12.5f, -12f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.OilCistern, -15.5f, -14.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.NicheStele, 16f, -7.5f, -90f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.NicheStele, -16.5f, -7f, 90f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.NicheStele, 7.5f, -17.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.NicheStele, -7.5f, -17.5f, 0f, 1f, 1));
            return r;
        }

        /// <summary>A keep of the Fortaleza: dark basalt with rows of lit niches, a copper-framed door, merlons and a porch under white cloth.</summary>
        private static TownBuildingSpec BasaltSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.9f, bool wide = false)
        {
            TownBuildingSpec spec = Spec(TownSwatch.LampBasalt, TownSwatch.LampCopper, TownSwatch.LampCopper, TownSwatch.ThousandLamps, dressing: dressing,
                sign: TownSwatch.WickLinen, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.AbyssIndigo);
            spec.BasaltKeep = true;
            spec.WideDoor = wide;
            spec.PlinthColor = TownSwatch.AbyssIndigo;
            return spec;
        }
    }
}
