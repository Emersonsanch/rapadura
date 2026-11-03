using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The hand-made recipes of the three regional towns (Docs/Cities.md §5.3, Docs/WorldBible_Maniqueismo.md §2.2–2.4).
    /// Same conventions as <see cref="CityTownRecipes.VilaEdna"/>: x = east, z = north; shop fronts face the isometric
    /// camera (south side of the W/E streets' north lots, west side of the South street's east lots, or a free lot facing
    /// south/west); nothing tall stands between the camera and a street (south side of the plaza and streets stays low).
    /// </summary>
    public static partial class CityTownRecipes
    {
        // ------------------------------------------------------------------
        // Oásis de Qocho (Areias Flamejantes)
        // ------------------------------------------------------------------

        /// <summary>
        /// Qocho (bible §2.2): rammed-earth houses the colour of rose taipa, grape-drying lofts of honeycomb brick on the
        /// roofs, karez water. The plaza head is Vandak's <b>caravanserai</b> (the Selaria): a long arcade of brick piers and
        /// pointed arches, market stalls on both sides of it and the pack camel resting by the arcade. The karez comes down
        /// from the north-west as a line of well shafts and an open channel that crosses the West street into
        /// <b>Yardena's cistern dome</b> on the plaza. West street: the potter's house, Yardena's domed house, the Cais da
        /// Barca; East street (dead end): Uzzai's house and his goat pen, a palm grove closing the street; South street:
        /// the road in from the map. The <b>Ruínas dos Artífices</b> (steep Meroë pyramids, lotus columns, ushabtis) stand
        /// at the south-east edge and in the sands beyond.
        /// </summary>
        public static CityTownRecipe Qocho()
        {
            var r = new CityTownRecipe
            {
                CityId = CityCatalog.AreiaBranca,
                Region = BiomeType.Desert,
                PlazaRadius = 8f,
                EntryArm = CityArm.South,
                NightLights = 6,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 34f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 30f, exitToMap: false));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 36f, exitToMap: true));

            // ---- Plaza head: the caravanserai of Vandak (Selaria) ----
            TownBuildingSpec caravanserai = Spec(TownSwatch.TaipaRose, TownSwatch.Sandstone, TownSwatch.Walnut, TownSwatch.Gold, sign: TownSwatch.ClothIndigo,
                dressing: TownShopDressing.Selaria, wallHeight: 3.6f);
            caravanserai.Arcade = true;
            caravanserai.FullVaranda = true;
            caravanserai.WideDoor = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "caravancara", Kind = CityLotKind.Shop, Free = true, Center = new Vector2(0f, 12.2f), Front = Vector2.down,
                Width = 11f, Depth = 5f, Porch = 2f, ResidentId = "vava", ShopId = CityShopCatalog.SelariaVava, Spec = caravanserai,
            });

            // ---- West street (the karez road; the Cais da Barca near its gate) ----
            TownBuildingSpec olaria = Spec(TownSwatch.TaipaRose, TownSwatch.Plaster, TownSwatch.Violet, TownSwatch.Gold, dressing: TownShopDressing.Pottery, door: 0.15f);
            olaria.DryingLoft = true;
            r.Lots.Add(Street("olaria", CityLotKind.House, CityArm.West, CityArm.North, 12.5f, 7f, 5.5f, 1.6f, null, null, olaria));
            TownBuildingSpec yardena = Spec(TownSwatch.Adobe, TownSwatch.Sandstone, TownSwatch.Walnut, TownSwatch.Turquoise, roof: TownRoofKind.Dome,
                dressing: TownShopDressing.Cistern, door: -0.15f);
            r.Lots.Add(Street("casa_yardena", CityLotKind.House, CityArm.West, CityArm.North, 20.5f, 6.5f, 5.5f, 1.5f, "socorro", null, yardena));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 28.5f, 7f, 3.2f, 1.0f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 12.5f, 7f, 5.5f, 1.3f));
            r.Lots.Add(Street("casa_w2", CityLotKind.House, CityArm.West, CityArm.South, 20.5f, 6.5f, 5.2f, 1.2f));
            r.Lots.Add(Street("casa_w3", CityLotKind.House, CityArm.West, CityArm.South, 28.5f, 6f, 5f, 1.2f));

            // ---- East street (dead end: Uzzai's house, his goat pen, the palm grove) ----
            TownBuildingSpec uzzai = Spec(TownSwatch.Sandstone, TownSwatch.Plaster, TownSwatch.WoodDark, TownSwatch.Gold, dressing: TownShopDressing.Herder, door: 0.15f);
            r.Lots.Add(Street("casa_uzzai", CityLotKind.House, CityArm.East, CityArm.North, 12.5f, 7f, 5.5f, 1.6f, "tiao", null, uzzai));
            TownBuildingSpec loft = Spec(TownSwatch.TaipaRose, TownSwatch.Sandstone, TownSwatch.Walnut, TownSwatch.Gold, door: -0.18f, windows: 1);
            loft.DryingLoft = true;
            r.Lots.Add(Street("casa_e1", CityLotKind.House, CityArm.East, CityArm.North, 20.5f, 6.5f, 5.2f, 1.3f, null, null, loft));
            r.Lots.Add(Street("casa_e2", CityLotKind.House, CityArm.East, CityArm.South, 15f, 6.5f, 5.2f, 1.3f));
            r.Lots.Add(Street("casa_e3", CityLotKind.House, CityArm.East, CityArm.South, 23f, 6.5f, 5.5f, 1.4f));

            // ---- South street (the road in from the map) ----
            TownBuildingSpec loftSouth = Spec(TownSwatch.Adobe, TownSwatch.Sandstone, TownSwatch.Violet, TownSwatch.Gold, door: 0.18f);
            loftSouth.DryingLoft = true;
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 12.5f, 7f, 5.5f, 1.3f, null, null, loftSouth));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.East, 20.5f, 6.5f, 5.2f, 1.3f));
            r.Lots.Add(Street("casa_s3", CityLotKind.House, CityArm.South, CityArm.East, 28.5f, 6f, 5f, 1.2f));
            r.Lots.Add(Street("casa_s4", CityLotKind.House, CityArm.South, CityArm.West, 15f, 6.5f, 5.2f, 1.3f));
            r.Lots.Add(Street("casa_s5", CityLotKind.House, CityArm.South, CityArm.West, 23f, 6.5f, 5.5f, 1.4f));
            r.Lots.Add(Street("casa_s6", CityLotKind.House, CityArm.South, CityArm.West, 30.5f, 5.5f, 5f, 1.2f));

            // ---- The caravanserai market: stalls flanking the arcade, the camel resting by it, jars ----
            r.PlazaProps.Add(At(CityPropKind.Stall, -4.4f, 6.4f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Stall, 4.4f, 6.4f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Camel, 7.6f, 10f, 90f));
            r.PlazaProps.Add(At(CityPropKind.Jugs, 6.2f, 5.0f, 30f));
            r.PlazaProps.Add(At(CityPropKind.Tree, -6.4f, 13.8f, 20f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Tree, 6.6f, 14.2f, 140f, 1.0f, 3));

            // ---- Water: the karez shafts in a line from the north-west, the open channel, Yardena's cistern ----
            Vector2 cistern = new Vector2(-4.5f, -4.6f);
            r.PlazaProps.Add(new CityProp { Kind = CityPropKind.CisternDome, Position = cistern, Yaw = CityGeometry.YawFacing(-cistern), Scale = 0.85f });
            foreach (float z in new[] { 22f, 28f, 34f, 42f, 49f, 56f, 64f, 72f })
            {
                r.PlazaProps.Add(At(CityPropKind.KarezShaft, -7.4f, z, z * 7f));
            }

            r.PlazaProps.Add(Segment(CityPropKind.KarezChannel, -7.4f, 19.5f, -7.4f, 4.2f));
            r.PlazaProps.Add(Segment(CityPropKind.KarezChannel, -7.4f, 4.2f, -5.5f, -3.3f));

            // ---- Plaza ----
            AddPlazaBench(r, 45f, 6.3f);
            AddPlazaBench(r, 135f, 6.3f);
            AddPlazaBench(r, 320f, 6.6f);
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // ---- Uzzai's goat pen (adobe walls, the gate on the street side) and the palm grove at the street's end ----
            r.PlazaProps.Add(Fence(26f, 4.6f, 33f, 4.6f));
            r.PlazaProps.Add(Fence(33f, 4.6f, 33f, 10.6f));
            r.PlazaProps.Add(Fence(26f, 10.6f, 33f, 10.6f));
            r.PlazaProps.Add(Fence(26f, 4.6f, 26f, 6.6f));
            r.PlazaProps.Add(Fence(26f, 8.6f, 26f, 10.6f));
            r.PlazaProps.Add(At(CityPropKind.HayBale, 30.5f, 8.6f, 15f));
            r.PlazaProps.Add(At(CityPropKind.Tree, 32.6f, -1.6f, 10f, 1.2f, 0));
            r.PlazaProps.Add(At(CityPropKind.Tree, 33.6f, 1.9f, 80f, 1.0f, 3));
            r.PlazaProps.Add(At(CityPropKind.Tree, 35.4f, 0.1f, 200f, 1.3f, 2));

            // ---- The Ruínas dos Artífices at the south-east edge, and bigger ones in the sands beyond ----
            r.PlazaProps.Add(At(CityPropKind.Ruin, 24f, -22f, 20f, 1.1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Ruin, 31f, -28f, -15f, 1.4f, 0));
            r.PlazaProps.Add(At(CityPropKind.Ruin, 25.5f, -32.5f, 0f, 1f, 1));
            r.PlazaProps.Add(At(CityPropKind.Ruin, 20.5f, -27f, 0f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.Ruin, 31f, -19.5f, 200f, 1f, 2));
            r.PlazaProps.Add(At(CityPropKind.Ruin, 52f, -30f, 10f, 2.5f, 0));
            r.PlazaProps.Add(At(CityPropKind.Ruin, 60f, -46f, -20f, 3f, 0));
            r.PlazaProps.Add(At(CityPropKind.Ruin, 47f, -52f, 30f, 2f, 0));
            r.PlazaProps.Add(At(CityPropKind.Bush, 15f, -19f, 0f, 1.1f));
            r.PlazaProps.Add(At(CityPropKind.Bush, 18f, -35f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.Bush, 35f, -35f, 0f, 1.2f));
            return r;
        }

        // ------------------------------------------------------------------
        // Vila de Litema (Serra de Kaggen)
        // ------------------------------------------------------------------

        /// <summary>
        /// Litema (bible §2.3): Basotho rondavels (round stone-and-clay walls with painted litema bands, conical thatch with
        /// the mokorotlo knot), dry-stone yard walls. The plaza head is a <b>Great Zimbabwe enclosure</b>: a tall curved
        /// dry-stone wall with the chevron frieze and the conical tower inside, its way in facing the plaza; the
        /// <b>Aksum stela</b> stands in front of it and a baobab shades the plaza. West street: Coti the weaver (blankets on
        /// the line), Innaios the player (drums), the Cais da Barca; East street ends at <b>Mestre Mihr's forge</b> (stone
        /// walls, thatch gable, the mine bell); South street: the road in, with a San <b>rock art</b> shelter by the gate
        /// and another at the plaza. Aloes in red flower all round.
        /// </summary>
        public static CityTownRecipe Litema()
        {
            var r = new CityTownRecipe
            {
                CityId = CityCatalog.SerraAlta,
                Region = BiomeType.Mountain,
                PlazaRadius = 8f,
                EntryArm = CityArm.South,
                NightLights = 5,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 34f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 28f, exitToMap: false));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 34f, exitToMap: true));

            // ---- West street: Coti, Innaios, the Cais da Barca; plain rondavels opposite ----
            r.Lots.Add(Street("casa_coti", CityLotKind.House, CityArm.West, CityArm.North, 12.5f, 6.6f, 5.4f, 1.5f, "graca", null,
                Rondavel(TownSwatch.RedClay, TownSwatch.ClothIndigo, TownSwatch.ClothYellow, TownShopDressing.Weaver)));
            r.Lots.Add(Street("casa_innaios", CityLotKind.House, CityArm.West, CityArm.North, 20.5f, 6.4f, 5.2f, 1.5f, "quinzinho", null,
                Rondavel(TownSwatch.LimeWhite, TownSwatch.RedClay, TownSwatch.ClothIndigo, TownShopDressing.Drums, door: -0.1f)));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 28.5f, 7f, 3.2f, 1.0f));
            r.Lots.Add(Street("casa_w1", CityLotKind.House, CityArm.West, CityArm.South, 12.5f, 6.4f, 5.2f, 0f));
            r.Lots.Add(Street("casa_w2", CityLotKind.House, CityArm.West, CityArm.South, 20.5f, 6f, 5f, 0f));
            r.Lots.Add(Street("casa_w3", CityLotKind.House, CityArm.West, CityArm.South, 28f, 6.2f, 5.2f, 0f));

            // ---- East street, closed by Mestre Mihr's forge ----
            r.Lots.Add(Street("casa_e1", CityLotKind.House, CityArm.East, CityArm.North, 12.5f, 6.4f, 5.2f, 0f, null, null,
                Rondavel(TownSwatch.RedClay, TownSwatch.LimeWhite, TownSwatch.ClothIndigo)));
            r.Lots.Add(Street("casa_e2", CityLotKind.House, CityArm.East, CityArm.North, 20.5f, 6f, 5f, 0f));
            r.Lots.Add(Street("casa_e3", CityLotKind.House, CityArm.East, CityArm.South, 15f, 6.2f, 5.2f, 0f));
            r.Lots.Add(Street("casa_e4", CityLotKind.House, CityArm.East, CityArm.South, 22f, 6f, 5f, 0f));
            TownBuildingSpec forge = Spec(TownSwatch.Stone, TownSwatch.LimeWhite, TownSwatch.WoodDark, TownSwatch.ClothIndigo, roof: TownRoofKind.Gable,
                dressing: TownShopDressing.Forja, sign: TownSwatch.ClothYellow, wallHeight: 3.0f, roofColor: TownSwatch.Thatch);
            forge.Chimney = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "forja_mihr", Kind = CityLotKind.Workshop, Free = true, Center = new Vector2(33.6f, 0f), Front = Vector2.left,
                Width = 8f, Depth = 5.5f, Porch = 1.6f, ResidentId = "tonho", ShopId = CityShopCatalog.ForjaTonho, Spec = forge,
            });

            // ---- South street (the road in from the map) ----
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 12.5f, 6.4f, 5.2f, 0f, null, null,
                Rondavel(TownSwatch.LimeWhite, TownSwatch.ClothYellow, TownSwatch.RedClay)));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.East, 20.5f, 6.2f, 5.2f, 0f));
            r.Lots.Add(Street("casa_s3", CityLotKind.House, CityArm.South, CityArm.West, 15f, 6.2f, 5.2f, 0f));
            r.Lots.Add(Street("casa_s4", CityLotKind.House, CityArm.South, CityArm.West, 23f, 6f, 5f, 0f));

            // ---- Plaza head: the Great Zimbabwe enclosure (tall curved dry-stone wall, the way in facing the plaza) ----
            var enclosure = new Vector2(0f, 15.5f);
            const float ring = 6f;
            // Chords every 20 degrees from 282 round the back to 258 (= 618): the 24-degree gap at the bottom is the way in.
            const float gapStart = 282f;
            const float gapEnd = 258f + 360f;
            for (float angle = gapStart; angle < gapEnd - 0.01f; angle += 20f)
            {
                Vector2 a = enclosure + Polar(angle, ring);
                Vector2 c = enclosure + Polar(Mathf.Min(angle + 20f, gapEnd), ring);
                r.PlazaProps.Add(Fence(a.x, a.y, c.x, c.y, 2.4f));
            }

            r.PlazaProps.Add(At(CityPropKind.ConicalTower, 2.2f, 17f));
            r.PlazaProps.Add(At(CityPropKind.Stela, 0f, 5.6f, 0f, 1.1f));
            r.PlazaProps.Add(At(CityPropKind.RockArt, 5.6f, 5.6f, CityGeometry.YawFacing(new Vector2(-1f, -1f))));
            r.PlazaProps.Add(At(CityPropKind.Tree, Polar(225f, 5.6f).x, Polar(225f, 5.6f).y, 30f, 1.1f, 3));
            AddPlazaBench(r, 135f, 6.4f);
            AddPlazaBench(r, 315f, 6.4f);
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7.5f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            // Aloes in red flower on the plaza edge and by the enclosure; a rock shelter by the south gate.
            r.PlazaProps.Add(At(CityPropKind.Bush, -6.4f, 6.0f, 0f, 1.1f));
            r.PlazaProps.Add(At(CityPropKind.Bush, -5.2f, -6.4f, 40f, 1.0f));
            r.PlazaProps.Add(At(CityPropKind.Bush, 7.4f, 9.6f, 80f, 1.2f));
            r.PlazaProps.Add(At(CityPropKind.Bush, -7.6f, 11.5f, 10f, 1.0f));
            r.PlazaProps.Add(At(CityPropKind.Bush, 7.2f, 21.6f, 60f, 1.1f));
            r.PlazaProps.Add(At(CityPropKind.RockArt, 4.55f, -29f, CityGeometry.YawFacing(Vector2.left), 1f));
            r.PlazaProps.Add(At(CityPropKind.Rock, 6.4f, -27.2f, 30f, 1.2f));
            r.PlazaProps.Add(At(CityPropKind.Rock, 33f, 8.5f, 70f, 1.6f));
            return r;
        }

        // ------------------------------------------------------------------
        // Vila das Lamparinas (Grutas do Limiar)
        // ------------------------------------------------------------------

        /// <summary>
        /// The Lamparinas (bible §2.4): a town under the cave, its houses cut into a <b>pink tuff cliff</b> that runs the
        /// whole north side, a second level of cave houses on a ledge with carved stairs, <b>rope bridges</b> high over the
        /// street and between the cliff faces, and <b>strings of lamps</b> zig-zagging from lamp post to lamp post along
        /// every street (and across the plaza): the town keeps a permanent dusk (<see cref="CityTownRecipe.AmbientNight"/>)
        /// with its lamps always lit. The plaza head is Parthenos's lamp shop, cut into the cliff; old Mihrshah, Aso and
        /// Gabryab live along the main street; the East street ends at the <b>spring</b> in the rock; the
        /// <b>vishapakar</b> guards the road in. South side of the streets stays low (camera side).
        /// </summary>
        public static CityTownRecipe Lamparinas()
        {
            var r = new CityTownRecipe
            {
                CityId = CityCatalog.Lamparinas,
                Region = BiomeType.Caves,
                PlazaRadius = 7.5f,
                EntryArm = CityArm.South,
                NightLights = 8,
                AmbientNight = 0.65f,
                LampStrings = true,
                LampSpacing = 5.5f,
            };

            r.Arms.Add(new CityArmRecipe(CityArm.West, 36f, exitToMap: true));
            r.Arms.Add(new CityArmRecipe(CityArm.East, 34f, exitToMap: false));
            r.Arms.Add(new CityArmRecipe(CityArm.South, 30f, exitToMap: true));

            // ---- Plaza head: Lamparinas da Parthenos, cut into the cliff ----
            TownBuildingSpec lamps = Spec(TownSwatch.PinkTuff, TownSwatch.Basalt, TownSwatch.WoodDark, TownSwatch.Copper, roof: TownRoofKind.Rock,
                dressing: TownShopDressing.Lamparinas, sign: TownSwatch.ClothIndigo, wallHeight: 3.2f);
            lamps.FullVaranda = true;
            r.Lots.Add(new CityLotRecipe
            {
                Id = "lamparinas_parthenos", Kind = CityLotKind.Shop, Free = true, Center = new Vector2(0f, 12.4f), Front = Vector2.down,
                Width = 7f, Depth = 5.2f, Porch = 1.8f, ResidentId = "benedita", ShopId = CityShopCatalog.LamparinasBenedita, Spec = lamps,
            });

            // ---- West street (north side cut into the cliff; the Cais da Barca near the gate) ----
            r.Lots.Add(Street("casa_mihrshah", CityLotKind.House, CityArm.West, CityArm.North, 12.5f, 7f, 5f, 1.4f, "candinho", null,
                Tuff(TownShopDressing.OldMiner, door: 0.15f)));
            r.Lots.Add(Street("casa_wn2", CityLotKind.House, CityArm.West, CityArm.North, 20.5f, 6.5f, 5f, 1.3f));
            r.Lots.Add(Street("cais_da_barca", CityLotKind.ShipDock, CityArm.West, CityArm.North, 28.5f, 7f, 3.2f, 1.0f));
            r.Lots.Add(Street("casa_ws1", CityLotKind.House, CityArm.West, CityArm.South, 12.5f, 6.5f, 4.8f, 1.0f, null, null, Tuff(wallHeight: 2.6f)));
            r.Lots.Add(Street("casa_ws2", CityLotKind.House, CityArm.West, CityArm.South, 28f, 6.5f, 4.8f, 1.0f, null, null, Tuff(wallHeight: 2.6f, door: 0.15f)));

            // ---- East street (Aso, Gabryab; the spring in the rock at its end) ----
            r.Lots.Add(Street("casa_aso", CityLotKind.House, CityArm.East, CityArm.North, 12.5f, 7f, 5f, 1.4f, "fia", null,
                Tuff(TownShopDressing.Storyteller, door: -0.15f)));
            r.Lots.Add(Street("casa_gabryab", CityLotKind.House, CityArm.East, CityArm.North, 20.5f, 6.5f, 5f, 1.4f, "zezinho", null,
                Tuff(TownShopDressing.Wicks, door: 0.15f)));
            r.Lots.Add(Street("casa_en3", CityLotKind.House, CityArm.East, CityArm.North, 28.5f, 6.5f, 5f, 1.3f));
            r.Lots.Add(Street("casa_es1", CityLotKind.House, CityArm.East, CityArm.South, 15f, 6.5f, 4.8f, 1.0f, null, null, Tuff(wallHeight: 2.6f)));
            r.Lots.Add(Street("casa_es2", CityLotKind.House, CityArm.East, CityArm.South, 26f, 6.5f, 4.8f, 1.0f, null, null, Tuff(wallHeight: 2.6f, door: -0.15f)));

            // ---- South street (the road in) ----
            r.Lots.Add(Street("casa_s1", CityLotKind.House, CityArm.South, CityArm.East, 12.5f, 6.5f, 5f, 1.2f));
            r.Lots.Add(Street("casa_s2", CityLotKind.House, CityArm.South, CityArm.East, 20.5f, 6.2f, 5f, 1.2f));
            r.Lots.Add(Street("casa_s3", CityLotKind.House, CityArm.South, CityArm.West, 15f, 6.2f, 4.8f, 1.0f, null, null, Tuff(wallHeight: 2.6f)));
            r.Lots.Add(Street("casa_s4", CityLotKind.House, CityArm.South, CityArm.West, 23f, 6f, 4.8f, 1.0f, null, null, Tuff(wallHeight: 2.6f, door: 0.15f)));

            // ---- The cliff: behind the north houses, a lower buttress either side of the plaza head, behind Parthenos,
            //      a wall closing the East street, and a bare spire south of the West street holding the high bridge. ----
            r.PlazaProps.Add(Segment(CityPropKind.Cliff, -38f, 10.1f, -9f, 10.1f));
            r.PlazaProps.Add(Segment(CityPropKind.Cliff, -9f, 10.6f, -3.7f, 10.6f, 0.6f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.Cliff, -3.7f, 16f, 3.7f, 16f, 1.15f));
            r.PlazaProps.Add(Segment(CityPropKind.Cliff, 3.7f, 10.6f, 9f, 10.6f, 0.6f, 1));
            r.PlazaProps.Add(Segment(CityPropKind.Cliff, 9f, 10.1f, 38f, 10.1f));
            r.PlazaProps.Add(Segment(CityPropKind.Cliff, 37.5f, 9f, 37.5f, -9f));
            r.PlazaProps.Add(Segment(CityPropKind.Cliff, -19f, -3.6f, -22f, -3.6f, 0.95f, 1));

            // ---- Rope bridges: high over the West street, and from the cliff ledges to Parthenos's rock ----
            r.PlazaProps.Add(Segment(CityPropKind.RopeBridge, -20.5f, 10.1f, -20.5f, -3.6f));
            r.PlazaProps.Add(Segment(CityPropKind.RopeBridge, -9.6f, 10.3f, -3.3f, 16f));
            r.PlazaProps.Add(Segment(CityPropKind.RopeBridge, 3.3f, 16f, 9.6f, 10.3f));

            // ---- Plaza: lamps strung across it, benches, oil jars, glowing mushrooms, ferns ----
            foreach (float angle in new[] { 30f, 150f, 210f, 330f })
            {
                r.PlazaProps.Add(new CityProp { Kind = CityPropKind.LampPost, Position = Polar(angle, 7f), Yaw = CityGeometry.YawFacing(-Polar(angle, 1f)) });
            }

            r.PlazaProps.Add(Segment(CityPropKind.LampString, Polar(30f, 7f).x, Polar(30f, 7f).y, Polar(150f, 7f).x, Polar(150f, 7f).y));
            r.PlazaProps.Add(Segment(CityPropKind.LampString, Polar(210f, 7f).x, Polar(210f, 7f).y, Polar(330f, 7f).x, Polar(330f, 7f).y));
            AddPlazaBench(r, 45f, 6f);
            AddPlazaBench(r, 135f, 6f);
            r.PlazaProps.Add(At(CityPropKind.Jugs, -2.4f, 6.9f, 20f));
            r.PlazaProps.Add(At(CityPropKind.Tree, -5f, -5.2f, 10f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Tree, 6.2f, 5.6f, 70f, 1.2f, 3));
            r.PlazaProps.Add(At(CityPropKind.Tree, -6.2f, 5.6f, 130f, 0.9f, 1));
            r.PlazaProps.Add(At(CityPropKind.Bush, 3f, 6.9f, 0f, 0.9f));
            r.PlazaProps.Add(At(CityPropKind.Bush, -6.6f, -3.4f, 0f, 0.8f));

            // ---- The spring at the end of the East street; the vishapakar at the road in ----
            r.PlazaProps.Add(At(CityPropKind.Fountain, 35.4f, 0f, CityGeometry.YawFacing(Vector2.left)));
            r.PlazaProps.Add(At(CityPropKind.Tree, 34.6f, 4.6f, 0f, 1.1f, 2));
            r.PlazaProps.Add(At(CityPropKind.Tree, 34.8f, -4.4f, 0f, 1f, 0));
            r.PlazaProps.Add(At(CityPropKind.Vishapakar, -4.4f, -27.6f, CityGeometry.YawFacing(Vector2.right)));
            return r;
        }

        // ------------------------------------------------------------------
        // Spec helpers
        // ------------------------------------------------------------------

        private static TownBuildingSpec Spec(TownSwatch wall, TownSwatch trim, TownSwatch joinery, TownSwatch accent, TownRoofKind roof = TownRoofKind.Flat,
            TownShopDressing dressing = TownShopDressing.None, TownSwatch sign = TownSwatch.ClothRed, float door = 0f, float wallHeight = 3.0f, int windows = 2,
            TownSwatch roofColor = TownSwatch.Mud)
        {
            return new TownBuildingSpec
            {
                Dressing = dressing,
                WallColor = wall,
                TrimColor = trim,
                RoofColor = roofColor,
                JoineryColor = joinery,
                AccentColor = accent,
                SignColor = sign,
                Roof = roof,
                DoorOffset = door,
                WallHeight = wallHeight,
                FrontWindows = windows,
            };
        }

        /// <summary>A Litema rondavel: wall colour, the two litema paint colours, an optional dressing for its resident.</summary>
        private static TownBuildingSpec Rondavel(TownSwatch wall, TownSwatch paint, TownSwatch accent, TownShopDressing dressing = TownShopDressing.None, float door = 0f)
        {
            return Spec(wall, paint, TownSwatch.WoodDark, accent, roof: TownRoofKind.Cone, dressing: dressing, door: door, wallHeight: 2.9f, roofColor: TownSwatch.Thatch);
        }

        /// <summary>A Lamparinas house cut into the tuff (pink tuff, basalt frames, copper lamps).</summary>
        private static TownBuildingSpec Tuff(TownShopDressing dressing = TownShopDressing.None, float door = 0f, float wallHeight = 3.0f)
        {
            return Spec(TownSwatch.PinkTuff, TownSwatch.Basalt, TownSwatch.WoodDark, TownSwatch.Copper, roof: TownRoofKind.Rock, dressing: dressing, door: door,
                wallHeight: wallHeight, windows: 1, roofColor: TownSwatch.PinkTuff);
        }
    }
}
