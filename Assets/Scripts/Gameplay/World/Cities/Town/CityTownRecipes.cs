using System.Collections.Generic;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The town recipes (content as code, like <c>CityCatalog</c>): every city is laid out by hand (Docs/Cities.md §5.3):
    /// <see cref="VilaEdna"/>, <see cref="Qocho"/>, <see cref="Litema"/>, <see cref="Lamparinas"/> and the race city
    /// <see cref="Garajonay"/>, <see cref="TamborRubro"/>, <see cref="BronzeHalls"/>, <see cref="TunnelMarket"/>, <see cref="Aratta"/>, <see cref="Kirta"/> and <see cref="Tarchon"/>
    /// (<see cref="CityCatalog.RaceCities"/>; the orcs' war camp, the dwarves' rune halls, the kobolds' tunnel bazaar, the
    /// povo-touro's reed halls, the povo-do-mar's coral towers and the trols' long houses on piles in their own styles) and Taa Pwani (<see cref="TaaPwani"/>, the first of the
    /// <see cref="CityCatalog.LateRegionCities"/>, in its own coral-stone style; then <see cref="Jhilpur"/>, the city in the roots). A city added to the
    /// catalog without a recipe gets <see cref="Default"/> (the Vila's plan filled with its residents, shops and regional
    /// style) until it has one. Layouts are cached per city and "mascate today" flag (the generator is deterministic,
    /// so the cache never goes stale).
    /// </summary>
    public static partial class CityTownRecipes
    {
        public const string MascateLotId = "mascate";

        private static readonly Dictionary<string, CityLayout> Cache = new Dictionary<string, CityLayout>();

        /// <summary>The layout of <paramref name="cityId"/>'s walkable town (null for an unknown city).</summary>
        public static CityLayout LayoutFor(string cityId, bool mascateToday = false)
        {
            string key = cityId + (mascateToday ? "+mascate" : string.Empty);
            if (Cache.TryGetValue(key, out CityLayout cached))
            {
                return cached;
            }

            CityTownRecipe recipe = RecipeFor(cityId, mascateToday);
            if (recipe == null)
            {
                return null;
            }

            CityLayout layout = CityLayoutGenerator.Generate(recipe);
            Cache[key] = layout;
            return layout;
        }

        public static void ClearCache() => Cache.Clear();

        public static CityTownRecipe RecipeFor(string cityId, bool mascateToday = false)
        {
            CitySpec city = CityCatalog.Get(cityId);
            if (city == null)
            {
                return null;
            }

            CityTownRecipe recipe = HandRecipe(cityId) ?? Default(city);
            if (mascateToday)
            {
                AddMascate(recipe);
            }

            return recipe;
        }

        /// <summary>The hand-made recipe of <paramref name="cityId"/> (null when the city has none yet).</summary>
        public static CityTownRecipe HandRecipe(string cityId)
        {
            switch (cityId)
            {
                case CityCatalog.VilaZefa: return VilaEdna();
                case CityCatalog.AreiaBranca: return Qocho();
                case CityCatalog.SerraAlta: return Litema();
                case CityCatalog.Lamparinas: return Lamparinas();
                case CityCatalog.CopasGarajonay: return Garajonay();
                case CityCatalog.TaaPwani: return TaaPwani();
                case CityCatalog.TamborRubro: return TamborRubro();
                case CityCatalog.SaloesDeBronze: return BronzeHalls();
                case CityCatalog.MercadoDosTuneis: return TunnelMarket();
                case CityCatalog.Aratta: return Aratta();
                case CityCatalog.Kirta: return Kirta();
                case CityCatalog.Jhilpur: return Jhilpur();
                case CityCatalog.PalafitasTarchon: return Tarchon();
                default: return null;
            }
        }

        // ------------------------------------------------------------------
        // Vila da Vó Edna (Floresta)
        // ------------------------------------------------------------------

        /// <summary>
        /// The Vila (Bosque do Zarafshan, world bible §2.1): Sogdian adobe houses the colour of apricots, flat roofs with
        /// parapets, deep aivans on carved walnut columns, an aryk running along the streets. A round plaza with the lamp
        /// house at its head (pishtaq portal, turquoise dome, the lamp tower: the light that keeps the dark away) and the
        /// Relogio de Zurvan (sundial) in front of it, the well with copper jugs, benches and Nana's bazaar; three streets:
        /// West (along the river woods, the Cais da Barca near its gate), East (the Loja Geral, Vo Edna's house with its
        /// long aivan, tandir and vine trellis, the stable closing the street) and South (the road in from the map: the
        /// smithy, the carpenter, the seamstress). Shop fronts face the isometric camera (north side of the W/E streets,
        /// east side of the South street); poplars, tamarisks and apricot trees in the yards behind adobe yard walls.
        /// </summary>
        public static CityTownRecipe VilaEdna()
        {
            var r = new CityTownRecipe
            {
                CityId = CityCatalog.VilaZefa,
                Region = BiomeType.Forest,
                PlazaRadius = 8f,
                EntryArm = CityArm.South,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 36f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 30f, exitToMap: false));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 36f, exitToMap: true));

            // ---- West street (the road along the river woods; the Cais da Barca near its gate) ----
            r.Lots.Add(Street("mercearia", CityLotKind.Shop, CityArm.West, CityArm.North, 12.5f, 7f, 5.5f, 1.8f, "lazaro", CityShopCatalog.MerceariaLazaro,
                Shop(TownShopDressing.Mercearia, TownSwatch.Adobe, TownSwatch.Plaster, TownSwatch.Walnut, TownSwatch.ClothGreen, door: 0.18f, upper: true)));
            r.Lots.Add(Street("armarinho", CityLotKind.Shop, CityArm.West, CityArm.North, 20.5f, 6.5f, 5.5f, 1.6f, "cida", CityShopCatalog.ArmarinhoCida,
                Shop(TownShopDressing.Armarinho, TownSwatch.TaipaRose, TownSwatch.LimeWhite, TownSwatch.Turquoise, TownSwatch.ClothIndigo, door: -0.15f)));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 29f, 7f, 3.2f, 1.0f, null, null, null));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 12.5f, 7f, 5.5f, 1.4f));
            r.Lots.Add(Street("casa_w2", CityLotKind.House, CityArm.West, CityArm.South, 20.5f, 6.5f, 5.2f, 1.3f));
            r.Lots.Add(Street("casa_w3", CityLotKind.House, CityArm.West, CityArm.South, 28.5f, 6f, 5f, 1.3f));

            // ---- East street (the Loja Geral, Vo Edna's house, the stable closing the street) ----
            r.Lots.Add(Street("loja_geral", CityLotKind.Shop, CityArm.East, CityArm.North, 12.5f, 7.5f, 6f, 1.8f, null, CityShopCatalog.GeneralStore,
                Shop(TownShopDressing.GeneralStore, TownSwatch.AdobeOchre, TownSwatch.Plaster, TownSwatch.Walnut, TownSwatch.ClothRed, door: 0f, floors: 2)));
            r.Lots.Add(Street("casa_vo_edna", CityLotKind.Grandma, CityArm.East, CityArm.North, 22f, 8.5f, 6f, 2.2f, "vo_edna", null, new TownBuildingSpec
            {
                WallColor = TownSwatch.Adobe,
                TrimColor = TownSwatch.LimeWhite,
                RoofColor = TownSwatch.Mud,
                JoineryColor = TownSwatch.Walnut,
                AccentColor = TownSwatch.Turquoise,
                Roof = TownRoofKind.Flat,
                FullVaranda = true,
                Chimney = true,
                UpperRoom = true,
                StorkNest = true,
                FrontWindows = 2,
                DoorOffset = -0.12f,
                WallHeight = 3.1f,
            }));
            r.Lots.Add(Street("casa_e1", CityLotKind.House, CityArm.East, CityArm.South, 15f, 6.5f, 5.2f, 1.3f));
            r.Lots.Add(Street("casa_e2", CityLotKind.House, CityArm.East, CityArm.South, 23f, 6.5f, 5.5f, 1.4f));

            // ---- South street (the road in from the map: the smithy, the carpenter, the seamstress) ----
            r.Lots.Add(Street("ferraria", CityLotKind.Workshop, CityArm.South, CityArm.East, 12.5f, 7f, 4.8f, 2.0f, "severino", CityShopCatalog.FerrariaSeverino,
                Shop(TownShopDressing.Ferraria, TownSwatch.AdobeOchre, TownSwatch.Plaster, TownSwatch.WoodDark, TownSwatch.Iron, door: 0f, chimney: true)));
            r.Lots.Add(Street("marcenaria", CityLotKind.Shop, CityArm.South, CityArm.East, 20.5f, 7f, 5.5f, 1.7f, "damiao", CityShopCatalog.MarcenariaDamiao,
                Shop(TownShopDressing.Marcenaria, TownSwatch.Adobe, TownSwatch.AdobeOchre, TownSwatch.Walnut, TownSwatch.Walnut, door: -0.15f, wooden: true)));
            r.Lots.Add(Street("costureira", CityLotKind.Shop, CityArm.South, CityArm.East, 28.5f, 6.2f, 5.2f, 1.6f, "fatima", CityShopCatalog.CostureiraFatima,
                Shop(TownShopDressing.Costureira, TownSwatch.Plaster, TownSwatch.LimeWhite, TownSwatch.Turquoise, TownSwatch.Violet, door: 0.15f)));
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.West, 15f, 6.5f, 5.2f, 1.3f));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.West, 23f, 6.5f, 5.5f, 1.4f));
            r.Lots.Add(Street("casa_s3", CityLotKind.House, CityArm.South, CityArm.West, 30.5f, 5.5f, 5f, 1.2f));

            // ---- Plaza head: the lamp house (pishtaq, dome, lamp tower); the stable closing the East street ----
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_da_lampada", Kind = CityLotKind.LampHouse, Free = true, Center = new Vector2(0f, 13.5f), Front = Vector2.down,
                Width = 6f, Depth = 9f, Porch = 0f,
                Spec = new TownBuildingSpec { WallColor = TownSwatch.Plaster, TrimColor = TownSwatch.Turquoise, RoofColor = TownSwatch.Turquoise, JoineryColor = TownSwatch.Walnut, AccentColor = TownSwatch.Gold, WallHeight = 4.4f },
            });
            r.Lots.Add(new CityLotRecipe
            {
                Id = "haras", Kind = CityLotKind.Stable, Free = true, Center = new Vector2(35.5f, 0f), Front = Vector2.left,
                Width = 9f, Depth = 6f, Porch = 1f, ResidentId = "tonico_haras", ShopId = CityShopCatalog.HarasVila,
                Spec = Shop(TownShopDressing.Haras, TownSwatch.Adobe, TownSwatch.AdobeOchre, TownSwatch.Walnut, TownSwatch.ClothRed, door: 0f, wooden: true, roof: TownRoofKind.Gable),
            });

            // ---- The bazaar (Nana's feira): two stalls with suzani awnings facing the plaza ----
            r.Lots.Add(new CityLotRecipe
            {
                Id = "feira", Kind = CityLotKind.Market, Free = true, Center = new Vector2(-3.1f, 5.6f), Front = Vector2.down,
                Width = 4.6f, Depth = 1.2f, Porch = 0f, ResidentId = "nenem", ShopId = CityShopCatalog.FeiraLivre,
                StandOverride = new Vector2(-3.1f, 6.8f), ApproachOverride = new Vector2(-3.1f, 4.15f),
            });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Stall, Position = new Vector2(-1.9f, 5.6f), Yaw = 0f, Variant = 0 });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Stall, Position = new Vector2(-4.3f, 5.6f), Yaw = 0f, Variant = 1 });

            // ---- The lore cast without a counter (TODO.md "História e Arcos dos NPCs"), standing on the plaza: Ramrat by the
            // lamp house (the lamp and the Bênção da Romaria), the two musicians of the Bema on the east side (the Call and the
            // Answer), Patig the tracker at the mouth of the West street (the road to the woods). Standing spots: no building.
            AddStandingSpot(r, "lampada_ramrat", "socorro_vila", new Vector2(-1.6f, 8.1f), new Vector2(-1.6f, 7.0f));
            AddStandingSpot(r, "musicos_xroshtag", "xroshtag", new Vector2(4.6f, -2.2f), new Vector2(3.5f, -1.7f));
            AddStandingSpot(r, "musicos_padwaxtag", "padwaxtag", new Vector2(5.9f, -0.5f), new Vector2(4.8f, -0.1f));
            AddStandingSpot(r, "rastros_patig", "patig", new Vector2(-5.9f, -2.4f), new Vector2(-4.9f, -1.8f));

            // ---- Plaza ----
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Well, Position = new Vector2(3.6f, 3.6f) });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Sundial, Position = new Vector2(0.4f, 5.4f) });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Jugs, Position = new Vector2(5.0f, 2.4f), Yaw = 40f });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Tree, Position = Polar(225f, 5.2f), Scale = 1.5f, Variant = 0, Yaw = 30f });
            AddPlazaBench(r, 235f, 6.4f);
            AddPlazaBench(r, 305f, 6.4f);
            AddPlazaBench(r, 72f, 6.7f);
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Vo Edna's tandir and vine trellis ----
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Tandir, Position = new Vector2(26.9f, 4.25f) });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Trellis, Position = new Vector2(22f, 14.6f), Yaw = 0f, Scale = 1f });

            // ---- Stable paddock (the gate on the street side) and hay ----
            r.PlazaProps.Add(Fence(31.5f, 5.6f, 39.5f, 5.6f));
            r.PlazaProps.Add(Fence(39.5f, 5.6f, 39.5f, 12f));
            r.PlazaProps.Add(Fence(31.5f, 12f, 39.5f, 12f));
            r.PlazaProps.Add(Fence(31.5f, 5.6f, 31.5f, 7.8f));
            r.PlazaProps.Add(Fence(31.5f, 9.8f, 31.5f, 12f));
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.HayBale, Position = new Vector2(31.2f, -5.6f), Yaw = 90f });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.HayBale, Position = new Vector2(33.2f, 8.8f), Yaw = 20f });

            // A few shrubs on the plaza edge and by the lamp house.
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Bush, Position = new Vector2(4.4f, 9.6f), Scale = 1.2f });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Bush, Position = new Vector2(-6.2f, 9.8f), Scale = 1.1f });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.FlowerBed, Position = new Vector2(2.2f, 8.6f), Variant = 2 });
            return r;
        }

        // ------------------------------------------------------------------
        // The other cities until they get their own recipe
        // ------------------------------------------------------------------

        /// <summary>
        /// A generic town for <paramref name="city"/>: the Vila's plan (plaza with the lamp house, W/E/S streets, exits W and S,
        /// the Cais da Barca on the West street) filled with the city's shops on the camera-facing street sides and its
        /// other residents in houses, in the region's style. Good enough to walk; meant to be replaced by a hand recipe.
        /// </summary>
        public static CityTownRecipe Default(CitySpec city)
        {
            var r = new CityTownRecipe
            {
                CityId = city.CityId,
                Region = city.Region,
                PlazaRadius = 7.5f,
                EntryArm = CityArm.South,
                NightLights = city.Region == BiomeType.Caves ? 6 : 4,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 34f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 30f, exitToMap: false));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 34f, exitToMap: true));

            // Camera-facing slots first (shops), then the others (homes of the residents without a counter).
            var front = new List<(CityArm Arm, CityArm Side, float Along)>
            {
                (CityArm.West, CityArm.North, 12.5f), (CityArm.East, CityArm.North, 12.5f), (CityArm.South, CityArm.East, 12.5f),
                (CityArm.West, CityArm.North, 20.5f), (CityArm.East, CityArm.North, 20.5f), (CityArm.South, CityArm.East, 20.5f),
                (CityArm.East, CityArm.North, 28f), (CityArm.South, CityArm.East, 28.5f),
            };
            var back = new List<(CityArm Arm, CityArm Side, float Along)>
            {
                (CityArm.West, CityArm.South, 12.5f), (CityArm.East, CityArm.South, 15f), (CityArm.South, CityArm.West, 15f),
                (CityArm.West, CityArm.South, 20.5f), (CityArm.East, CityArm.South, 23f), (CityArm.South, CityArm.West, 23f),
                (CityArm.West, CityArm.South, 28f),
            };

            int f = 0;
            int b = 0;
            foreach (CityResidentSpec resident in city.Residents)
            {
                bool shop = !string.IsNullOrEmpty(resident.ShopId);
                (CityArm Arm, CityArm Side, float Along) slot = shop && f < front.Count ? front[f++] : b < back.Count ? back[b++] : front[f++];
                r.Lots.Add(Street("lot_" + resident.ResidentId, shop ? CityLotKind.Shop : CityLotKind.House, slot.Arm, slot.Side, slot.Along, 6.5f, 5.3f, 1.4f,
                    resident.ResidentId, resident.ShopId));
            }

            // Fill the remaining slots with plain houses so the streets read as streets.
            while (b < back.Count)
            {
                (CityArm Arm, CityArm Side, float Along) slot = back[b++];
                r.Lots.Add(Street("casa_" + b, CityLotKind.House, slot.Arm, slot.Side, slot.Along, 6.2f, 5f, 1.2f));
            }

            while (f < front.Count - 1)
            {
                (CityArm Arm, CityArm Side, float Along) slot = front[f++];
                r.Lots.Add(Street("casa_f" + f, CityLotKind.House, slot.Arm, slot.Side, slot.Along, 6.2f, 5f, 1.2f));
            }

            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 28.5f, 7f, 3.2f, 1.0f, null, null, null));
            r.Lots.Add(new CityLotRecipe
            {
                Id = "casa_da_lampada", Kind = CityLotKind.LampHouse, Free = true, Center = new Vector2(0f, 12.6f), Front = Vector2.down, Width = 5.6f, Depth = 8f, Porch = 0f,
            });

            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Well, Position = new Vector2(3.4f, 3.4f) });
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Tree, Position = Polar(225f, 4.8f), Scale = 1.4f });
            AddPlazaBench(r, 235f, 6f);
            AddPlazaBench(r, 72f, 6.2f);
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            return r;
        }

        /// <summary>Ammo's stall in the plaza (south-east), on the days his mule is in this city.</summary>
        private static void AddMascate(CityTownRecipe r)
        {
            // South-east of the plaza, between the South and East street mouths, facing north (axis-aligned: tight AABB).
            var at = new Vector2(r.PlazaRadius * 0.575f, -r.PlazaRadius * 0.675f);
            r.PlazaProps.RemoveAll(p => p.Kind == CityPropKind.Bench && Vector2.Distance(p.Position, at) < 2.5f);
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Stall, Position = at, Yaw = CityGeometry.YawFacing(Vector2.up), Variant = 2 });
            r.Lots.Add(new CityLotRecipe
            {
                Id = MascateLotId, Kind = CityLotKind.Market, Free = true, Center = at, Front = Vector2.up, Width = 2.2f, Depth = 1.2f, Porch = 0f,
                ResidentId = CityCatalog.Mascate.ResidentId, ShopId = CityCatalog.Mascate.ShopId,
                StandOverride = at + Vector2.down * 1.25f, ApproachOverride = at + Vector2.up * 1.4f,
            });
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        public static Vector2 Polar(float degrees, float radius)
        {
            float r = degrees * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(r), Mathf.Sin(r)) * radius;
        }

        private static void AddPlazaBench(CityTownRecipe r, float angle, float radius)
        {
            Vector2 at = Polar(angle, radius);
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.Bench, Position = at, Yaw = CityGeometry.YawFacing(-at) });
        }

/// <summary>
        /// A resident standing on the plaza without a building of their own (a <see cref="CityLotKind.Market"/> lot of
        /// 1 × 1 m: no walls, no ground patch): the keeper stands at <paramref name="stand"/>, the cousins step to
        /// <paramref name="approach"/> to hear them.
        /// </summary>
        private static void AddStandingSpot(CityTownRecipe r, string id, string residentId, Vector2 stand, Vector2 approach)
        {
            r.Lots.Add(new CityLotRecipe
            {
                Id = id, Kind = CityLotKind.Market, Free = true, Center = stand, Front = (approach - stand).normalized, Width = 1f, Depth = 1f, Porch = 0f,
                ResidentId = residentId, StandOverride = stand, ApproachOverride = approach,
            });
        }

                private static CityProp Fence(float x0, float z0, float x1, float z1, float height = 1f) =>
            new CityProp { Kind = CityPropKind.FenceRun, Position = new Vector2(x0, z0), End = new Vector2(x1, z1), Scale = height };

        private static CityProp Segment(CityPropKind kind, float x0, float z0, float x1, float z1, float scale = 1f, int variant = 0) =>
            new CityProp { Kind = kind, Position = new Vector2(x0, z0), End = new Vector2(x1, z1), Scale = scale, Variant = variant };

        private static CityProp At(CityPropKind kind, float x, float z, float yaw = 0f, float scale = 1f, int variant = 0) =>
            new CityProp { Kind = kind, Position = new Vector2(x, z), Yaw = yaw, Scale = scale, Variant = variant };

        private static CityLotRecipe Street(string id, CityLotKind kind, CityArm arm, CityArm side, float along, float width, float depth, float porch,
            string residentId = null, string shopId = null, TownBuildingSpec spec = null)
        {
            return new CityLotRecipe
            {
                Id = id, Kind = kind, Arm = arm, Side = side, Along = along, Width = width, Depth = depth, Porch = porch,
                ResidentId = residentId, ShopId = shopId, Spec = spec,
            };
        }

        private static TownBuildingSpec Shop(TownShopDressing dressing, TownSwatch wall, TownSwatch trim, TownSwatch joinery, TownSwatch sign,
            float door = 0f, int floors = 1, bool chimney = false, bool wooden = false, TownRoofKind roof = TownRoofKind.Flat, bool upper = false)
        {
            return new TownBuildingSpec
            {
                Dressing = dressing,
                WallColor = wall,
                TrimColor = trim,
                RoofColor = TownSwatch.Mud,
                JoineryColor = joinery,
                AccentColor = TownSwatch.Turquoise,
                SignColor = sign,
                DoorOffset = door,
                Floors = floors,
                Chimney = chimney,
                WoodenWalls = wooden,
                Roof = roof,
                UpperRoom = upper,
                WideDoor = dressing == TownShopDressing.Marcenaria || dressing == TownShopDressing.GeneralStore,
                FrontWindows = 2,
                WallHeight = floors > 1 ? 2.8f : 3.1f,
            };
        }
    }
}
