using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// Mercado dos Túneis, the race city of the kobolds (Docs/Mapas100.md §3.3 and map 22; return city of maps 22–23), in its
    /// own style (<see cref="TownStyles.TunnelBazaar"/>): the market at the crossing of the four galleries of the Grutas do
    /// Limiar. Four streets, one per gallery, each with its cart rails; Miwnay's Feira (the city's shop: a stall in an arched
    /// niche with racks of bottled Ar Sereno) and the Cais da Barca on the West gallery; Takhsich the inventor on the North
    /// gallery; Shayn the mender on the East gallery; Farnkhund the cart driver's station on the South gallery. Copper air
    /// pipes with a fan inside a tube, small niche stalls, carts and a pulley around the crossing; the round basalt tunnel
    /// wall (copper pipe and bottle lamps along it) closes the north and the east. Everything stays inside ±20 m at y = 0
    /// (the place the Limiar overworld keeps for the market: safe zone x −21…22, z −20…21, flat ground ±24), with the four
    /// roads out where the overworld's galleries arrive. Same conventions as the other recipes (x = east, z = north; fronts
    /// towards the isometric camera).
    /// </summary>
    public static partial class CityTownRecipes
    {
        /// <summary>The return city id of the maps 22–23 (= <c>MapCatalog.MercadoDosTuneis</c>, <see cref="CityCatalog.MercadoDosTuneis"/>).</summary>
        public const string TunnelMarketCityId = "mercado_dos_tuneis";

        /// <summary>Half side of the walkable square (inside the overworld's safe zone x −21…22, z −20…21).</summary>
        public const float TunnelMarketHalfExtent = 20f;

        public static CityTownRecipe TunnelMarket()
        {
            var r = new CityTownRecipe
            {
                CityId = TunnelMarketCityId,
                Region = BiomeType.Caves,
                Style = TownStyles.TunnelBazaar,
                PlazaRadius = 6f,
                StreetWidth = 3.5f,
                SidewalkWidth = 0.8f,
                EntryArm = CityArm.North,
                WalkHalfExtent = TunnelMarketHalfExtent,
                AutoLamps = false,
                AutoYards = false,
                Backdrop = false,
                NightLights = 6,
                AmbientNight = 0.65f,
            };

            foreach (CityArm arm in new[] { CityArm.North, CityArm.South, CityArm.East, CityArm.West })
            {
                r.Arms.Add(new CityArmRecipe(arm, 18f, exitToMap: true));
            }

            // ---- West gallery: the Feira da Miwnay (the city's shop), the Cais da Barca ----
            TownBuildingSpec feira = StallSpec(TownShopDressing.AirBottles, TownSwatch.ClothRed, wallHeight: 3.3f);
            feira.SignColor = TownSwatch.GlassAmber;
            feira.WideDoor = true;
            r.Lots.Add(Street("feira_da_miwnay", CityLotKind.Shop, CityArm.West, CityArm.North, 9.5f, 6.5f, 4f, 1.6f, "miwnay", CityShopCatalog.FeiraMiwnay, feira));
            r.Lots.Add(Street("cais_do_mercado", CityLotKind.ShipDock, CityArm.West, CityArm.North, 16f, 5f, 3f, 1f));

            // ---- North gallery (the way in from the Lamparinas): Takhsich's workshop; niche stalls on the other side ----
            r.Lots.Add(Street("oficina_takhsich", CityLotKind.House, CityArm.North, CityArm.East, 10f, 6f, 4f, 1.4f, "takhsich", null,
                StallSpec(TownShopDressing.Tinkerer, TownSwatch.ClothIndigo, wallHeight: 3.1f)));
            float facingEast = CityGeometry.YawFacing(Vector2.right);
            r.PlazaProps.Add(At(CityPropKind.NicheStall, -4.7f, 11.5f, facingEast, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.NicheStall, -4.7f, 16f, facingEast, 1f, 1));

            // ---- East gallery: Shayn's mending bench; a cart of bottles on its spur ----
            r.Lots.Add(Street("banca_da_shayn", CityLotKind.House, CityArm.East, CityArm.North, 13f, 6f, 4f, 1.4f, "shayn", null,
                StallSpec(TownShopDressing.Tinkerer, TownSwatch.ClothGreen, wallHeight: 2.9f)));
            r.PlazaProps.Add(At(CityPropKind.MineCart, 12f, -5.5f, 0f, 1f, 1));

            // ---- South gallery: Farnkhund's cart station; a loaded cart and the pulley on the other side ----
            r.Lots.Add(Street("estacao_farnkhund", CityLotKind.House, CityArm.South, CityArm.East, 10f, 6f, 4f, 1.4f, "farnkhund", null,
                StallSpec(TownShopDressing.GeneralStore, TownSwatch.ClothRed, wallHeight: 3.0f)));
            r.PlazaProps.Add(At(CityPropKind.MineCart, -5.2f, -11.5f, 90f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Pulley, -9f, -7f, 0f));
            r.PlazaProps.Add(At(CityPropKind.Crates, -8.5f, -11f, 20f));
            r.PlazaProps.Add(At(CityPropKind.Jugs, -11.5f, -5f, 10f));

            // ---- The crossing: cart rails down every gallery, air pipes, a niche stall facing the plaza, lamps ----
            r.PlazaProps.Add(Segment(CityPropKind.MineRails, 0f, 6.2f, 0f, 18f));
            r.PlazaProps.Add(Segment(CityPropKind.MineRails, 0f, -6.2f, 0f, -18f));
            r.PlazaProps.Add(Segment(CityPropKind.MineRails, 6.2f, 0f, 18f, 0f));
            r.PlazaProps.Add(Segment(CityPropKind.MineRails, -6.2f, 0f, -18f, 0f));
            r.PlazaProps.Add(At(CityPropKind.AirPipe, -4.8f, 4.8f));
            r.PlazaProps.Add(At(CityPropKind.AirPipe, -4.8f, -4.8f));
            r.PlazaProps.Add(At(CityPropKind.AirPipe, 6f, -6f));
            Vector2 stall = Polar(35f, 8.2f);
            r.PlazaProps.Add(At(CityPropKind.NicheStall, stall.x, stall.y, CityGeometry.YawFacing(-stall.normalized), 1f, 2));
            foreach (float angle in new[] { 60f, 120f, 240f, 315f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 6.6f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // Gallery lamps on the curb opposite the doors (by hand: none in front of a door).
            foreach (Vector2 at in new[] { new Vector2(-10f, -2.05f), new Vector2(-15.5f, -2.05f), new Vector2(10f, -2.05f), new Vector2(15.5f, -2.05f), new Vector2(-2.05f, -10f), new Vector2(-2.05f, -15.5f), new Vector2(-2.05f, 13.8f) })
            {
                Vector2 toRoad = Mathf.Abs(at.y) < 3f ? Vector2.up : Vector2.right;
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = at, Yaw = CityGeometry.YawFacing(toRoad) });
            }

            // ---- The round basalt wall of the tunnels: north and east (away from the camera), gaps for the galleries ----
            const float wall = 19.4f;
            r.PlazaProps.Add(Segment(CityPropKind.RockWall, -19.4f, wall, -3.3f, wall, 1f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.RockWall, 3.3f, wall, wall, wall, 1f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.RockWall, wall, 3.3f, wall, wall, 1f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.RockWall, wall, -19.3f, wall, -3.3f, 1f, 1));
            return r;
        }

        /// <summary>A stall in an arched basalt niche: copper voussoirs, a canvas awning striped with <paramref name="stripe"/> (TownKit.TunnelBazaar).</summary>
        private static TownBuildingSpec StallSpec(TownShopDressing dressing, TownSwatch stripe, float door = 0f, float wallHeight = 3.0f)
        {
            TownBuildingSpec spec = Spec(TownSwatch.Basalt, TownSwatch.Copper, TownSwatch.Copper, stripe, roof: TownRoofKind.Rock, dressing: dressing,
                sign: TownSwatch.GlassAmber, door: door, wallHeight: wallHeight, windows: 1, roofColor: TownSwatch.Basalt);
            spec.TunnelStall = true;
            spec.PlinthColor = TownSwatch.Basalt;
            return spec;
        }
    }
}
