using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Hinokura, a Ilha-Forja, the capital of the Arquipélago de Hinokura (Docs/Mapas100.md map 65; the Fogo Vivo, Ícaro's
    /// element), in its own style (<see cref="TownStyles.LacquerPalace"/>): a forge town on a volcanic isle round the plaza of
    /// pumice where the Forja de Hinokura stands. Mestra Akane's hall of red lacquer, the master of the lacquer (the missions),
    /// closes the head of the plaza below the palace courts; Kurogane's black-iron forge (the city's shop: the gear of the gates
    /// of maps 65–70) and the stone harbour (Cais de Pedra) on the West street, the quarter of the forges; Hikari, who keeps the
    /// sluices of the lava channels, and a hall on the East street, the quarter of the sluices; Sango, the boatwoman of the
    /// Barco-Farol (the second mission), on the South street, the road in, with a hall across; channels of tamed lava with
    /// sluice gates and black stone bridges crossing the West and South streets; stone lanterns, pines pruned on lava rock and
    /// boulders of volcanic rock between the streets; no street to the north: there the isle climbs to the palace courts and the
    /// volcano, with rocks, pines and lanterns. Everything stays inside a circle of radius <see cref="HinokuraSolidRadius"/> at
    /// y = 0 (the place the archipelago overworld will keep for it), with three roads out (W, E, S) ending at
    /// ±<see cref="HinokuraArmLength"/>. Same conventions as the other recipes.
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of maps 65–70 (= <see cref="CityCatalog.Hinokura"/>; MapCatalog will use the same id).</summary>
        public const string HinokuraCityId = "hinokura";

        /// <summary>Length of the three streets (W, E, S): their gates and exits stand at this distance from the plaza centre.</summary>
        public const float HinokuraArmLength = 19f;

        /// <summary>Half side of the walkable square (the invisible walls and the safe zone).</summary>
        public const float HinokuraHalfExtent = 22f;

        /// <summary>Every collider, lot and gate of the town stays inside this radius from the plaza centre.</summary>
        public const float HinokuraSolidRadius = 22.5f;

        /// <summary>Where the Forja de Hinokura stands on the plaza (north of the centre, clear of the respawn).</summary>
        public static readonly Vector2 HinokuraForge = new Vector2(0f, 4.2f);

        public static CityTownRecipe Hinokura()
        {
            var r = new CityTownRecipe
            {
                CityId = HinokuraCityId,
                Region = BiomeType.Forest,
                Style = TownStyles.LacquerPalace,
                PlazaRadius = 7f,
                StreetWidth = 3.4f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.South,
                WalkHalfExtent = HinokuraHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 8,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, HinokuraArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, HinokuraArmLength, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.South, HinokuraArmLength, exitToMap: true));

            // ---- Plaza head: Mestra Akane's hall of red lacquer, below the palace courts (the missions) ----
            TownBuildingSpec akane = LacquerSpec(TownShopDressing.None, wallHeight: 3.1f);
            akane.FullVaranda = true;
            akane.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "salao_de_akane", Kind = CityLotKind.House, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down,
                Width = 8.5f, Depth = 6f, Porch = 1.8f, Spec = akane, ResidentId = "akane",
            });

            // ---- West street (the quarter of the forges, out to the Jardins de Lava Laqueada): Kurogane's black iron (the city's shop), the stone harbour; a hall across ----
            TownBuildingSpec ferro = LacquerSpec(TownShopDressing.BlackIronForge, door: 0.18f, wallHeight: 2.9f);
            ferro.SignColor = TownSwatch.Gold;
            ferro.WallColor = TownSwatch.VolcanicStone;
            r.Lots.Add(Street("ferro_preto_do_kurogane", CityLotKind.Shop, CityArm.West, CityArm.North, 11f, 7f, 5f, 1.6f, "kurogane", CityShopCatalog.FerroPretoKurogane, ferro));
            r.Lots.Add(Street("cais_de_pedra", CityLotKind.ShipDock, CityArm.West, CityArm.North, 17.5f, 4.5f, 3f, 1f));
            r.Lots.Add(Street("salao_w1", CityLotKind.House, CityArm.West, CityArm.South, 11f, 6.5f, 4.8f, 1.2f, null, null, LacquerSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- East street (the quarter of the sluices, out to Monte Echeyde): Hikari's hall by the sluices; a hall of coral ----
            r.Lots.Add(Street("salao_de_hikari", CityLotKind.House, CityArm.East, CityArm.North, 11f, 6.5f, 5f, 1.4f, "hikari", null,
                LacquerSpec(TownShopDressing.None, door: -0.15f, wallHeight: 2.9f)));
            TownBuildingSpec coral = LacquerSpec(TownShopDressing.None, wallHeight: 2.6f);
            coral.WallColor = TownSwatch.CoralStone;
            coral.TrimColor = TownSwatch.PalaceLacquer;
            r.Lots.Add(Street("salao_e2", CityLotKind.House, CityArm.East, CityArm.North, 17f, 5f, 4.2f, 1.2f, null, null, coral));

            // ---- South street (the road in from the harbour): Sango, the boatwoman of the Barco-Farol (the second mission); a hall across ----
            r.Lots.Add(Street("salao_de_sango", CityLotKind.House, CityArm.South, CityArm.East, 11f, 6.5f, 5f, 1.4f, "sango", null,
                LacquerSpec(TownShopDressing.None, door: 0.15f, wallHeight: 3.0f, wide: true)));
            r.Lots.Add(Street("salao_s2", CityLotKind.House, CityArm.South, CityArm.West, 12f, 6f, 4.8f, 1.2f, null, null, LacquerSpec(TownShopDressing.None, wallHeight: 2.7f)));

            // ---- The plaza of pumice: the Forja de Hinokura (north of the centre), stone lanterns at its corners ----
            r.PlazaProps.Add(At(CityPropKind.IsleForge, HinokuraForge.x, HinokuraForge.y, 0f, 1f));
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Channels of tamed lava crossing the West and South streets, each with its sluices and black stone bridge ----
            r.PlazaProps.Add(Segment(CityPropKind.LavaSluice, -15.8f, -2.4f, -15.8f, 2.4f));
            r.PlazaProps.Add(Segment(CityPropKind.LavaSluice, -2.4f, -15.8f, 2.4f, -15.8f));

            // ---- Between the streets: lava rocks, stone lanterns and pruned pines ----
            r.PlazaProps.Add(At(CityPropKind.LavaRock, -9.5f, 12.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.LavaRock, 9.5f, 12.5f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.StoneLantern, 13f, -6f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.PrunedPine, 13.5f, -11f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.LavaRock, 16.5f, -6.5f, 0f, 0.8f, 0));
            r.PlazaProps.Add(At(CityPropKind.StoneLantern, -14.5f, -12.5f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.StoneLantern, -18f, -5.5f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.PrunedPine, -11f, -12.5f, 0f, 0.8f, 0));
            r.PlazaProps.Add(At(CityPropKind.LavaRock, 7.5f, -17.5f, 0f, 0.8f, 0));

            // ---- No street to the north: the isle climbs to the palace courts and the volcano ----
            r.PlazaProps.Add(At(CityPropKind.LavaRock, -7.5f, 19.2f, 0f, 1.1f, 1));
            r.PlazaProps.Add(At(CityPropKind.LavaRock, 7.5f, 19.2f, 0f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.LavaRock, 0f, 19.8f, 0f, 1.2f, 1));
            r.PlazaProps.Add(At(CityPropKind.StoneLantern, -4f, 19.4f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.StoneLantern, 4f, 19.4f, 0f, 0.9f, 0));
            r.PlazaProps.Add(At(CityPropKind.PrunedPine, -13f, 14f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.PrunedPine, 13f, 14f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.LavaRock, -16f, 11.5f, 0f, 0.7f, 0));
            r.PlazaProps.Add(At(CityPropKind.LavaRock, 16f, 11.5f, 0f, 0.7f, 0));
            return r;
        }

        /// <summary>A hall of Hinokura: red lacquer on a curved coral base, white ridges on red tiles, gold bands.</summary>
        private static TownBuildingSpec LacquerSpec(TownShopDressing dressing, float door = 0f, float wallHeight = 2.9f, bool wide = false)
        {
            TownBuildingSpec spec = Spec(TownSwatch.PalaceLacquer, TownSwatch.CoralStone, TownSwatch.Obsidian, TownSwatch.Gold, dressing: dressing,
                sign: TownSwatch.PalaceLacquer, door: door, wallHeight: wallHeight, windows: 2, roofColor: TownSwatch.FireTile);
            spec.LacquerHall = true;
            spec.WideDoor = wide;
            spec.PlinthColor = TownSwatch.CoralStone;
            return spec;
        }
    }
}
