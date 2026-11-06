using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Farnkesh, os Ninhos de Pedra, the capital of the Picos de Farnkesh (Docs/Mapas100.md map 54, style
    /// <c>PinnacleNest</c>): the houses on the pinnacles (<see cref="PinnacleNestHouse"/>: a sandstone rock foot coming up out
    /// of the clouds, white lime walls leaning in, a band of painted red wood under the eaves, painted wooden windows, a
    /// shallow plank roof weighed down with stones, a gold finial and a balcony out over the void), the giant wind bell of the
    /// plaza, sandstone pinnacles (one with a small nest-house on top), the rope basket lifts Sisin pulls, the cloud gaps
    /// crossed by plank-and-rope bridges and the gate of two bell posts with a rope of wind chimes; Sisin's rope rack. The
    /// dzongs of Bhutan and the rocks of Meteora are form only: houses of people who like wind and silence, no cult. Same rules
    /// as the rest of the kit: flat colours from <see cref="TownPalette"/> (row 19 holds lime, painted red, gold, pinnacle
    /// sandstone, cloud, the high sky, yak rope and bell bronze), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of the <see cref="CityPropKind.WindBell"/>'s plinth (× its scale): its collider.</summary>
        public const float WindBellRadius = 1.3f;

        /// <summary>Radius of a <see cref="CityPropKind.StonePinnacle"/>'s foot (× its scale): its collider.</summary>
        public const float StonePinnacleRadius = 0.9f;

        /// <summary>Radius of a <see cref="CityPropKind.RopeBasket"/> lift (× its scale), gantry feet included: its collider.</summary>
        public const float RopeBasketRadius = 0.9f;

        /// <summary>Half the width of the void of a <see cref="CityPropKind.CloudGap"/>.</summary>
        public const float CloudGapHalfWidth = 0.8f;

        /// <summary>Height of the sandstone foot a <see cref="TownBuildingSpec.PinnacleNest"/> house stands on.</summary>
        private const float NestRockTop = 0.7f;

        // ------------------------------------------------------------------
        // The nest-house on its pinnacle
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.PinnacleNest"/> (local frame, front −z): the sandstone rock foot (<see cref="TownBuildingSpec.PlinthColor"/>)
        /// with cloud tufts at its corners, white lime walls (<see cref="TownBuildingSpec.WallColor"/>) leaning in up to the
        /// eaves, the painted red band under them (<see cref="TownBuildingSpec.TrimColor"/>), painted wooden windows, a door
        /// with a red frame and a gold lintel, the shallow plank roof (<see cref="TownBuildingSpec.RoofColor"/>) weighed down
        /// with stones and a gold finial; in front a plank balcony on two red posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>,
        /// the colliders use the same list) with a yak-rope rail, a small wind chime and the sign hanging from the rail.
        /// </summary>
        public static void PinnacleNestHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float cz = (zFacade + zBack) * 0.5f;
            float hd = (zBack - zFacade) * 0.5f;
            float eave = Mathf.Max(top, NestRockTop + 2.4f);
            TownSwatch rock = spec.PlinthColor;

            // The rock foot coming up out of the clouds (a frustum a little wider at the bottom) and cloud tufts at its corners.
            Frustum(b, 0f, cz, hw, hd, hw - 0.1f, hd - 0.1f, 0f, NestRockTop, rock, TownSwatch.Sandstone);
            foreach (float sx in new[] { -1f, 1f })
            {
                b.Blob(new Vector3(sx * (hw - 0.35f), 0.15f, zBack - 0.35f), new Vector3(0.7f, 0.35f, 0.6f), TownSwatch.CloudWhite);
                b.Blob(new Vector3(sx * (hw - 0.2f), 0.45f, cz), new Vector3(0.35f, 0.5f, 0.6f), rock);
            }

            // The leaning lime walls and the painted red band under the eaves.
            float wb = hw - 0.18f, db = hd - 0.18f;
            float wt = hw - 0.5f, dt = hd - 0.45f;
            float band = eave - 0.5f;
            Frustum(b, 0f, cz, wb, db, wt, dt, NestRockTop, band, spec.WallColor, null);
            float wbBand = Mathf.Lerp(wb, wt, (band - NestRockTop) / (eave - NestRockTop));
            float dbBand = Mathf.Lerp(db, dt, (band - NestRockTop) / (eave - NestRockTop));
            Frustum(b, 0f, cz, wbBand + 0.03f, dbBand + 0.03f, wt + 0.03f, dt + 0.03f, band, eave, spec.TrimColor, spec.TrimColor);

            // The windows: painted wooden frames on the leaning front wall (the wall leans in, so they sit a little back as they rise).
            float frontAt(float y) => cz - Mathf.Lerp(db, dt, (y - NestRockTop) / (eave - NestRockTop)) - 0.03f;
            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -wb + 0.7f, wb - 0.7f);
            for (int i = 0; i < windows; i++)
            {
                float side = doorX > 0f ? -1f : 1f;
                float x = i == 0 ? doorX + side * 1.2f : doorX - side * 1.1f;
                x = Mathf.Clamp(x, -wt + 0.35f, wt - 0.35f);
                float y = band - 0.65f;
                b.Box(new Vector3(x, y, frontAt(y)), new Vector3(0.62f, 0.62f, 0.06f), spec.TrimColor, 0f);
                b.Box(new Vector3(x, y, frontAt(y) - 0.02f), new Vector3(0.42f, 0.42f, 0.04f), TownSwatch.WindowLit, 0f);
                b.Box(new Vector3(x, y - 0.36f, frontAt(y) - 0.05f), new Vector3(0.7f, 0.08f, 0.12f), spec.AccentColor, 0f);
            }

            // The door with its red frame and gold lintel.
            float doorW = spec.WideDoor ? 1.25f : 0.95f;
            float doorTop = NestRockTop + 1.85f;
            float dz = frontAt(NestRockTop + 0.9f);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.12f, NestRockTop, dz - 0.06f), new Vector3(doorX + doorW * 0.5f + 0.12f, doorTop + 0.12f, dz + 0.04f), spec.TrimColor);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, NestRockTop, dz - 0.09f), new Vector3(doorX + doorW * 0.5f, doorTop, dz - 0.06f), TownSwatch.DoorPlank);
            b.Box(new Vector3(doorX, doorTop + 0.2f, dz - 0.08f), new Vector3(doorW + 0.4f, 0.1f, 0.1f), spec.AccentColor, 0f);

            // Stone steps from the door down the rock foot to the balcony / street.
            for (int i = 0; i < 3; i++)
            {
                float y = NestRockTop * (3 - i) / 3f;
                float z = cz - hd - 0.12f - i * 0.22f;
                if (z < zFront + 0.05f)
                {
                    break;
                }

                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, 0f, z - 0.22f), new Vector3(doorX + doorW * 0.5f, y, z), rock, TownSwatch.Sandstone);
            }

            // The shallow plank roof weighed down with stones, and the gold finial.
            float roofY = eave + 0.06f;
            b.Box(new Vector3(0f, roofY, cz), new Vector3(Mathf.Min(hw * 2f, wt * 2f + 0.7f), 0.14f, Mathf.Min(hd * 2f + 0.2f, dt * 2f + 0.7f)), spec.RoofColor, 0f, TownSwatch.WoodWeathered);
            b.Box(new Vector3(0f, roofY + 0.14f, cz), new Vector3(wt * 2f - 0.2f, 0.14f, dt * 2f - 0.2f), spec.RoofColor, 0f, TownSwatch.WoodWeathered);
            string key = spec.Seed + "nest";
            for (int i = 0; i < 5; i++)
            {
                float x = Mathf.Lerp(-wt + 0.3f, wt - 0.3f, TownStyle.Hash01(key, i));
                float z = cz + Mathf.Lerp(-dt + 0.3f, dt - 0.3f, TownStyle.Hash01(key, i + 7));
                b.Blob(new Vector3(x, roofY + 0.3f, z), new Vector3(0.32f, 0.2f, 0.28f), TownSwatch.Stone);
            }

            b.Cylinder(new Vector3(0f, roofY + 0.2f, cz), 0.12f, 0.05f, 0.5f, 6, spec.AccentColor);
            b.Blob(new Vector3(0f, roofY + 0.78f, cz), new Vector3(0.16f, 0.2f, 0.16f), TownSwatch.NestGold);

            if (spec.PorchDepth > 0.01f)
            {
                // The balcony over the void: a plank deck on two red posts, a yak-rope rail, a small wind chime and the sign.
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.WoodWeathered, TownSwatch.WoodLight);
                float z = zFront + TownBuildingBuilder.PostInset;
                float railY = 1.0f;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, (deck + eave - 0.4f) * 0.5f, z), new Vector3(0.16f, eave - 0.4f - deck, 0.16f), spec.JoineryColor, 0f);
                    b.Box(new Vector3(x, eave - 0.36f, z), new Vector3(0.26f, 0.08f, 0.26f), spec.AccentColor, 0f);
                }

                b.Box(new Vector3(0f, eave - 0.25f, z), new Vector3(hw * 2f - 0.1f, 0.14f, 0.18f), spec.JoineryColor, 0f);
                b.Box(new Vector3(0f, eave - 0.2f, (z + zFacade) * 0.5f), new Vector3(hw * 2f - 0.1f, 0.08f, zFacade - z + 0.2f), spec.RoofColor, 0f);
                b.Log(new Vector3(-hw + 0.1f, railY, z - 0.02f), new Vector3(hw - 0.1f, railY, z - 0.02f), 0.035f, TownSwatch.YakRope, 4);
                b.Log(new Vector3(-hw + 0.1f, railY * 0.55f, z - 0.02f), new Vector3(hw - 0.1f, railY * 0.55f, z - 0.02f), 0.03f, TownSwatch.YakRope, 4);

                float chimeX = Mathf.Clamp(-doorX, -hw + 0.4f, hw - 0.4f);
                b.Log(new Vector3(chimeX, eave - 0.3f, z), new Vector3(chimeX, eave - 0.75f, z), 0.01f, TownSwatch.YakRope, 3);
                b.Cylinder(new Vector3(chimeX, eave - 1.0f, z), 0.1f, 0.06f, 0.25f, 6, TownSwatch.BellBronze);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, railY - 0.05f, z - 0.08f);
                    b.Box(at, new Vector3(1.1f, 0.32f, 0.05f), spec.JoineryColor, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.94f, 0.22f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>
        /// A box with a smaller top than bottom (leaning walls): bottom half extents (<paramref name="hx0"/>, <paramref name="hz0"/>)
        /// at <paramref name="y0"/>, top ones at <paramref name="y1"/>, centred on (<paramref name="cx"/>, <paramref name="cz"/>); the
        /// top face in <paramref name="topSwatch"/> (none when null).
        /// </summary>
        private static void Frustum(TownMeshBuilder b, float cx, float cz, float hx0, float hz0, float hx1, float hz1, float y0, float y1,
            TownSwatch swatch, TownSwatch? topSwatch)
        {
            var bl = new[]
            {
                new Vector3(cx - hx0, y0, cz - hz0), new Vector3(cx + hx0, y0, cz - hz0), new Vector3(cx + hx0, y0, cz + hz0), new Vector3(cx - hx0, y0, cz + hz0),
            };
            var tl = new[]
            {
                new Vector3(cx - hx1, y1, cz - hz1), new Vector3(cx + hx1, y1, cz - hz1), new Vector3(cx + hx1, y1, cz + hz1), new Vector3(cx - hx1, y1, cz + hz1),
            };
            var centre = new Vector3(cx, (y0 + y1) * 0.5f, cz);
            for (int i = 0; i < 4; i++)
            {
                int j = (i + 1) % 4;
                Vector3 mid = (bl[i] + bl[j] + tl[i] + tl[j]) * 0.25f;
                b.QuadFacing(bl[i], bl[j], tl[j], tl[i], mid - centre, swatch);
            }

            if (topSwatch.HasValue)
            {
                b.QuadFacing(tl[0], tl[1], tl[2], tl[3], Vector3.up, topSwatch.Value);
            }
        }

        /// <summary>
        /// The gate of Farnkesh at a street's end: two red-painted posts with gold caps on sandstone feet, a yak rope sagging
        /// between them with five small bronze wind chimes and a puff of cloud at each foot.
        /// </summary>
        public static void BellPostGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                foreach (float x in new[] { 0f, w })
                {
                    b.Cylinder(new Vector3(x, 0f, 0f), 0.4f, 0.32f, 0.5f, 6, TownSwatch.PinnacleStone, TownSwatch.Sandstone);
                    b.Box(new Vector3(x, 2.2f, 0f), new Vector3(0.26f, 3.4f, 0.26f), TownSwatch.NestRed, 0f);
                    b.Box(new Vector3(x, 3.95f, 0f), new Vector3(0.4f, 0.12f, 0.4f), TownSwatch.NestGold, 0f);
                    b.Blob(new Vector3(x, 4.15f, 0f), new Vector3(0.18f, 0.24f, 0.18f), TownSwatch.NestGold);
                    b.Blob(new Vector3(x, 0.2f, -0.4f), new Vector3(0.9f, 0.35f, 0.5f), TownSwatch.CloudWhite);
                }

                const int pieces = 6;
                for (int i = 0; i < pieces; i++)
                {
                    float x0 = w * i / pieces, x1 = w * (i + 1) / pieces;
                    b.Log(new Vector3(x0, RopeSag(x0, w), 0f), new Vector3(x1, RopeSag(x1, w), 0f), 0.035f, TownSwatch.YakRope, 4);
                }

                for (int i = 1; i < 6; i++)
                {
                    float x = w * i / 6f;
                    float y = RopeSag(x, w);
                    b.Log(new Vector3(x, y, 0f), new Vector3(x, y - 0.35f, 0f), 0.01f, TownSwatch.YakRope, 3);
                    b.Cylinder(new Vector3(x, y - 0.6f, 0f), 0.09f, 0.05f, 0.25f, 6, TownSwatch.BellBronze);
                }
            });
        }

        /// <summary>The height of the gate's rope at <paramref name="x"/> along a span of <paramref name="w"/> (a gentle sag).</summary>
        private static float RopeSag(float x, float w)
        {
            float t = w > 0f ? x / w : 0f;
            return 3.7f - 0.6f * 4f * t * (1f - t);
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The giant wind bell of the plaza: a round sandstone plinth, a frame of two red-painted posts and a beam with gold caps,
        /// the big bronze bell hanging from it with its clapper and a ring of four small wind chimes on the plinth's rim.
        /// </summary>
        private static void WindBell(TownMeshBuilder b, float s)
        {
            float r = WindBellRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.92f, 0.4f * s, 10, TownSwatch.PinnacleStone, TownSwatch.Sandstone);
            float beamY = 3.8f * s;
            foreach (float x in new[] { -r * 0.75f, r * 0.75f })
            {
                b.Box(new Vector3(x, (0.4f * s + beamY) * 0.5f, 0f), new Vector3(0.24f, beamY - 0.4f * s, 0.24f) * 1f, TownSwatch.NestRed, 0f);
            }

            b.Box(new Vector3(0f, beamY, 0f), new Vector3(r * 1.9f, 0.26f, 0.3f), TownSwatch.NestRed, 0f, TownSwatch.NestGold);
            foreach (float x in new[] { -r * 0.95f, r * 0.95f })
            {
                b.Blob(new Vector3(x, beamY + 0.12f, 0f), new Vector3(0.24f, 0.24f, 0.34f), TownSwatch.NestGold);
            }

            // The bell: a bronze body widening to the lip, a domed crown and the clapper.
            float bellTop = beamY - 0.35f * s;
            b.Log(new Vector3(0f, beamY, 0f), new Vector3(0f, bellTop, 0f), 0.06f * s, TownSwatch.YakRope, 4);
            b.Cylinder(new Vector3(0f, bellTop - 1.3f * s, 0f), 0.75f * s, 0.5f * s, 1.15f * s, 10, TownSwatch.BellBronze);
            b.Cylinder(new Vector3(0f, bellTop - 0.15f * s, 0f), 0.5f * s, 0.18f * s, 0.15f * s, 10, TownSwatch.BellBronze, TownSwatch.BellBronze);
            b.Cylinder(new Vector3(0f, bellTop - 1.32f * s, 0f), 0.78f * s, 0.78f * s, 0.1f * s, 10, TownSwatch.NestGold);
            b.Log(new Vector3(0f, bellTop - 0.4f * s, 0f), new Vector3(0f, bellTop - 1.45f * s, 0f), 0.03f * s, TownSwatch.Iron, 4);
            b.Blob(new Vector3(0f, bellTop - 1.5f * s, 0f), new Vector3(0.16f, 0.16f, 0.16f) * s, TownSwatch.Iron);

            for (int i = 0; i < 4; i++)
            {
                float a = (45f + i * 90f) * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * 0.8f);
                b.Log(at + Vector3.up * 0.4f * s, at + Vector3.up * 1.6f * s, 0.03f, TownSwatch.NestRed, 4);
                b.Log(at + Vector3.up * 1.6f * s, at + Vector3.up * 1.6f * s + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.25f, 0.025f, TownSwatch.NestRed, 4);
                b.Cylinder(at + Vector3.up * 1.15f * s + new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * 0.25f, 0.08f, 0.04f, 0.22f, 6, TownSwatch.BellBronze);
            }
        }

        /// <summary>
        /// A sandstone pinnacle out of the clouds: three tapering drums with darker strata, clouds round its foot (Variant 1: a
        /// small white nest-house with a red band and a plank roof clinging to its top, a balcony over the void).
        /// </summary>
        private static void StonePinnacle(TownMeshBuilder b, float s, int variant)
        {
            float r = StonePinnacleRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.8f, 2.2f * s, 7, TownSwatch.PinnacleStone);
            b.Cylinder(new Vector3(0f, 2.2f * s, 0f), r * 0.82f, r * 0.62f, 2.0f * s, 7, TownSwatch.Sandstone, null, 0.3f);
            b.Cylinder(new Vector3(0f, 4.2f * s, 0f), r * 0.64f, r * 0.42f, 1.6f * s, 7, TownSwatch.PinnacleStone, TownSwatch.Sandstone, 0.6f);
            b.Cylinder(new Vector3(0f, 2.15f * s, 0f), r * 0.84f, r * 0.84f, 0.12f * s, 7, TownSwatch.WoodWeathered);
            foreach (float a in new[] { 20f, 150f, 260f })
            {
                float rad = a * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * (r * 0.85f) + Vector3.up * 0.25f, new Vector3(1.0f, 0.45f, 0.8f) * s, TownSwatch.CloudWhite);
            }

            if ((variant % 2) == 1)
            {
                float y = 5.8f * s;
                float h = r * 0.55f;
                b.Box(new Vector3(0f, y + 0.45f * s, 0f), new Vector3(h * 2f, 0.9f * s, h * 2f), TownSwatch.NestLime, 0f);
                b.Box(new Vector3(0f, y + 0.95f * s, 0f), new Vector3(h * 2f + 0.05f, 0.2f * s, h * 2f + 0.05f), TownSwatch.NestRed, 0f);
                b.Box(new Vector3(0f, y + 1.12f * s, 0f), new Vector3(h * 2.4f, 0.1f * s, h * 2.4f), TownSwatch.WoodWeathered, 0f);
                b.Box(new Vector3(0f, y + 0.4f * s, -h - 0.02f), new Vector3(0.3f, 0.3f, 0.04f) * s, TownSwatch.WindowLit, 0f);
                b.Box(new Vector3(0f, y + 0.05f * s, -h - 0.25f * s), new Vector3(h * 2f, 0.06f * s, 0.5f * s), TownSwatch.WoodWeathered, 0f);
            }
        }

        /// <summary>
        /// A rope basket lift: an A-frame gantry of two timber legs with a red crossbeam and a pulley wheel, the yak rope running
        /// down to a wicker basket hanging at waist height and a coil of rope at the foot.
        /// </summary>
        private static void RopeBasket(TownMeshBuilder b, float s)
        {
            float r = RopeBasketRadius * s;
            float topY = 3.4f * s;
            foreach (float x in new[] { -r * 0.8f, r * 0.8f })
            {
                b.Log(new Vector3(x, 0f, 0.15f), new Vector3(x * 0.35f, topY, 0f), 0.08f * s, TownSwatch.WoodWeathered, 5);
            }

            b.Box(new Vector3(0f, topY, 0f), new Vector3(r * 1.1f, 0.16f, 0.18f), TownSwatch.NestRed, 0f);
            b.Cylinder(new Vector3(0f, topY - 0.25f, -0.06f), 0.18f, 0.18f, 0.12f, 8, TownSwatch.WoodDark);
            b.Log(new Vector3(0.16f, topY - 0.25f, 0f), new Vector3(0.16f, 1.55f * s, 0f), 0.02f, TownSwatch.YakRope, 3);
            b.Log(new Vector3(-0.16f, topY - 0.25f, 0f), new Vector3(-0.4f, 0.3f, -0.3f), 0.02f, TownSwatch.YakRope, 3);
            b.Cylinder(new Vector3(0.16f, 0.95f * s, 0f), 0.3f * s, 0.36f * s, 0.55f * s, 8, TownSwatch.Wicker, TownSwatch.WoodDark);
            b.Log(new Vector3(-0.1f, 1.5f * s, 0f), new Vector3(0.42f, 1.5f * s, 0f), 0.02f, TownSwatch.YakRope, 3);
            b.Cylinder(new Vector3(-0.45f, 0f, -0.3f), 0.25f, 0.22f, 0.18f, 8, TownSwatch.YakRope, TownSwatch.WoodDark);
        }

        /// <summary>
        /// A gap of cloud from the origin to <paramref name="d"/> (the void <see cref="CloudGapHalfWidth"/> each side, the high sky
        /// far below and cloud puffs along both lips) crossed at its middle by a plank bridge with yak-rope rails on four posts.
        /// Walkable: the planks stay under the walker's step.
        /// </summary>
        private static void CloudGap(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = CloudGapHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.02f, TownSwatch.HighSky);
                int puffs = Mathf.Max(2, Mathf.RoundToInt(length / 1.1f));
                for (int i = 0; i < puffs; i++)
                {
                    float x = length * (i + 0.5f) / puffs;
                    if (Mathf.Abs(x - length * 0.5f) < 1.3f)
                    {
                        continue;
                    }

                    b.Blob(new Vector3(x, 0.1f, -w * 0.4f), new Vector3(0.9f, 0.22f, 0.7f), TownSwatch.CloudWhite);
                    b.Blob(new Vector3(x + 0.3f, 0.08f, w * 0.45f), new Vector3(0.8f, 0.2f, 0.6f), TownSwatch.CloudWhite);
                }

                float xm = length * 0.5f;
                for (int i = -3; i <= 3; i++)
                {
                    b.Box(new Vector3(xm + i * 0.3f, 0.1f, 0f), new Vector3(0.24f, 0.06f, w * 2f + 0.5f), TownSwatch.WoodWeathered, 0f);
                }

                foreach (float x in new[] { xm - 1.0f, xm + 1.0f })
                {
                    foreach (float z in new[] { -w - 0.2f, w + 0.2f })
                    {
                        b.Box(new Vector3(x, 0.5f, z), new Vector3(0.1f, 0.9f, 0.1f), TownSwatch.NestRed, 0f);
                    }
                }

                foreach (float z in new[] { -w - 0.2f, w + 0.2f })
                {
                    b.Log(new Vector3(xm - 1.0f, 0.9f, z), new Vector3(xm + 1.0f, 0.82f, z), 0.025f, TownSwatch.YakRope, 4);
                }
            });
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Sisin's front: coils of yak rope on a peg rack, a stack of bridge planks, a balance pole leaning and a lift basket.</summary>
        private static void PinnacleNestDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Box(new Vector3(x - 0.9f, y + 0.8f, z - 0.35f), new Vector3(0.9f, 0.08f, 0.08f), TownSwatch.NestRed, 0f);
            for (int i = 0; i < 3; i++)
            {
                b.Cylinder(new Vector3(x - 1.2f + i * 0.3f, y + 0.45f, z - 0.4f), 0.13f, 0.13f, 0.32f, 8, TownSwatch.YakRope, TownSwatch.WoodDark);
            }

            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(x + 0.7f, y + 0.05f + i * 0.08f, z - 0.45f), new Vector3(1.0f, 0.06f, 0.24f), TownSwatch.WoodWeathered, i * 6f);
            }

            b.Log(new Vector3(x + 1.25f, y, z - 0.25f), new Vector3(x + 1.1f, y + 2.0f, z - 0.15f), 0.03f, TownSwatch.WoodLight, 4);
            b.Cylinder(new Vector3(x + 0.1f, y, z - 0.55f), 0.24f, 0.3f, 0.4f, 8, TownSwatch.Wicker, TownSwatch.WoodDark);
        }
    }
}
