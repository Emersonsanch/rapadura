using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Salões de Bronze de Napirasu, the dwarves' city cut into the Serra de Kaggen (Docs/Mapas100.md §3.3
    /// and map 17): the hall fronts carved into the granite (<see cref="RuneHallHouse"/>: square pillars with bronze base and
    /// capital, a massive square bronze door with studs under a rune lintel whose lines glow one by one, slit windows), the
    /// free-standing bronze pillars, the Grande Forja gone cold (its crystal of Light dark: relighting it is the story of the
    /// map), pits of cold lava, the chain lift, the rock wall of the halls (also drawn in basalt for the Mercado dos Túneis)
    /// and the rune gate at the streets' ends; Puzur's rune dressing. Same rules as the rest of the kit: flat colours from
    /// <see cref="TownPalette"/> (row 9 holds the ficha's granite, bronze, green copper, cold ember and rune light),
    /// low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Side of a <see cref="CityPropKind.BronzePillar"/> (× its scale): its collider box.</summary>
        public const float BronzePillarSize = 0.9f;

        /// <summary>Footprint of the <see cref="CityPropKind.GreatForge"/> (local x × z, × its scale): its collider box.</summary>
        public static readonly Vector2 GreatForgeSize = new Vector2(5f, 3.2f);

        /// <summary>Radius of a <see cref="CityPropKind.LavaPit"/>'s granite rim (× its scale): its collider.</summary>
        public const float LavaPitRadius = 1.8f;

        /// <summary>Side of a <see cref="CityPropKind.ChainLift"/>'s square frame (× its scale): its collider box.</summary>
        public const float ChainLiftSize = 2.2f;

        /// <summary>Half the thickness of a <see cref="CityPropKind.RockWall"/>: its collider is the run inflated by this.</summary>
        public const float RockWallHalfThickness = 0.6f;

        // ------------------------------------------------------------------
        // The hall front cut into the rock
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.RuneHall"/> house (local frame, front −z): the granite mass of the mountain behind,
        /// a dressed facade with a stepped bronze cornice, the massive bronze door with studs and the rune lintel, a slit
        /// window each side, and the porch: a granite deck under square pillars (at <see cref="TownBuildingBuilder.PorchPostsX"/>,
        /// the colliders use the same list) carrying a granite beam.
        /// </summary>
        public static void RuneHallHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float y0 = TownBuildingBuilder.FloorY;
            TownSwatch rock = spec.WallColor;
            TownSwatch bronze = spec.TrimColor;

            // The mountain the hall is cut into, and the dressed facade standing proud of it.
            b.Crown(new Vector3(0f, top * 0.85f, (zFacade + zBack) * 0.5f + 0.5f), new Vector3(spec.Width + 1.4f, top * 1.8f, spec.Depth + 1.4f), rock);
            b.Crown(new Vector3(-hw * 0.35f, top * 1.25f, zBack - 1.5f), new Vector3(spec.Width * 0.85f, top * 1.3f, spec.Depth * 0.7f), TownSwatch.StoneDark, 25f);
            b.BoxMinMax(new Vector3(-hw, y0, zFacade), new Vector3(hw, top + 0.55f, zFacade + 0.7f), rock);
            b.BoxMinMax(new Vector3(-hw - 0.12f, top + 0.15f, zFacade - 0.12f), new Vector3(hw + 0.12f, top + 0.3f, zFacade + 0.7f), bronze);
            b.BoxMinMax(new Vector3(-hw + 0.25f, top + 0.3f, zFacade - 0.06f), new Vector3(hw - 0.25f, top + 0.55f, zFacade + 0.7f), rock);
            Plinth(b, -hw, hw, zFacade, zFacade + 0.7f, spec.PlinthColor);

            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.8f : 1.3f;
            b.With(new Vector3(0f, 0f, zFacade), 0f, () =>
            {
                BronzeDoor(b, doorX, y0, doorW, 2.35f, spec.JoineryColor, rock, spec.AccentColor);

                // A slit window each side of the door (where there is room), lit at night.
                foreach (float side in new[] { -1f, 1f })
                {
                    float wx = doorX + side * (doorW * 0.5f + 0.95f);
                    if (Mathf.Abs(wx) > hw - 0.45f)
                    {
                        continue;
                    }

                    b.BoxMinMax(new Vector3(wx - 0.14f, y0 + 0.9f, -0.02f), new Vector3(wx + 0.14f, y0 + 2.1f, 0f), TownSwatch.WindowLit);
                    b.BoxMinMax(new Vector3(wx - 0.24f, y0 + 0.82f, -0.08f), new Vector3(wx + 0.24f, y0 + 0.9f, 0f), bronze);
                    b.BoxMinMax(new Vector3(wx - 0.24f, y0 + 2.1f, -0.08f), new Vector3(wx + 0.24f, y0 + 2.2f, 0f), bronze);
                }
            });

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.StoneDark, TownSwatch.Granite);
                b.BoxMinMax(new Vector3(doorX - 0.8f, 0f, zFront - 0.32f), new Vector3(doorX + 0.8f, deck * 0.5f, zFront), TownSwatch.Granite);
                float z = zFront + TownBuildingBuilder.PostInset;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    SquarePillar(b, new Vector3(x, deck, z), top - deck + 0.2f, 0.42f, rock, bronze, false);
                }

                b.BoxMinMax(new Vector3(-hw - 0.08f, top + 0.05f, z - 0.25f), new Vector3(hw + 0.08f, top + 0.35f, zFacade), rock);
                b.BoxMinMax(new Vector3(-hw - 0.1f, top + 0.05f, z - 0.27f), new Vector3(hw + 0.1f, top + 0.12f, z - 0.24f), bronze);
            }

            if (spec.Chimney)
            {
                // The flue comes out of the rock above the hall: a granite stack with a bronze cap.
                var flue = new Vector3(hw - 0.8f, top + 0.4f, zFacade + 1.4f);
                b.Box(flue + Vector3.up * 0.9f, new Vector3(0.6f, 1.8f, 0.6f), TownSwatch.StoneDark, 0f, TownSwatch.Black);
                b.Box(flue + Vector3.up * 1.85f, new Vector3(0.75f, 0.12f, 0.75f), bronze, 0f);
            }
        }

        /// <summary>
        /// A massive square bronze door (face frame: x across, y up, −z outwards): a deep granite frame, two bronze leaves with
        /// rows of studs and a green-copper band, and over it the rune lintel: a granite beam with a row of rune strokes, every
        /// other one lit (the dwarves' rune gates light one line at a time).
        /// </summary>
        public static void BronzeDoor(TownMeshBuilder b, float x, float y, float width, float height, TownSwatch leaf, TownSwatch frame, TownSwatch rune)
        {
            float hw = width * 0.5f;
            b.BoxMinMax(new Vector3(x - hw, y, -0.02f), new Vector3(x + hw, y + height, 0f), TownSwatch.Black);
            foreach (float side in new[] { -1f, 1f })
            {
                float lx0 = side < 0f ? x - hw + 0.02f : x + 0.01f;
                float lx1 = side < 0f ? x - 0.01f : x + hw - 0.02f;
                b.BoxMinMax(new Vector3(lx0, y, -0.07f), new Vector3(lx1, y + height - 0.02f, -0.02f), leaf);
                b.BoxMinMax(new Vector3(lx0, y + height * 0.46f, -0.085f), new Vector3(lx1, y + height * 0.54f, -0.07f), TownSwatch.Verdigris);
                float cx = (lx0 + lx1) * 0.5f;
                for (int row = 0; row < 4; row++)
                {
                    float sy = y + 0.3f + row * (height - 0.6f) / 3f;
                    foreach (float sx in new[] { cx - (lx1 - lx0) * 0.28f, cx + (lx1 - lx0) * 0.28f })
                    {
                        b.Box(new Vector3(sx, sy, -0.085f), new Vector3(0.07f, 0.07f, 0.04f), TownSwatch.Gold, 45f);
                    }
                }

                // The ring handle.
                b.Box(new Vector3(x + side * 0.12f, y + height * 0.42f, -0.1f), new Vector3(0.04f, 0.18f, 0.04f), TownSwatch.Gold);
            }

            const float f = 0.22f;
            b.BoxMinMax(new Vector3(x - hw - f, y, -0.14f), new Vector3(x - hw, y + height, 0f), frame);
            b.BoxMinMax(new Vector3(x + hw, y, -0.14f), new Vector3(x + hw + f, y + height, 0f), frame);
            b.BoxMinMax(new Vector3(x - hw - f - 0.1f, y + height, -0.18f), new Vector3(x + hw + f + 0.1f, y + height + 0.42f, 0f), frame);
            int strokes = Mathf.Max(4, Mathf.RoundToInt(width / 0.24f));
            for (int i = 0; i < strokes; i++)
            {
                float rx = x - hw + (i + 0.5f) * width / strokes;
                bool lit = (i % 2) == 0;
                b.BoxMinMax(new Vector3(rx - 0.035f, y + height + 0.1f, -0.19f), new Vector3(rx + 0.035f, y + height + 0.32f, -0.18f), lit ? rune : TownSwatch.Bronze);
                if ((i % 3) == 1)
                {
                    b.BoxMinMax(new Vector3(rx - 0.08f, y + height + 0.2f, -0.195f), new Vector3(rx + 0.08f, y + height + 0.24f, -0.19f), lit ? TownSwatch.Bronze : rune);
                }
            }

            b.BoxMinMax(new Vector3(x - hw - 0.25f, Mathf.Max(0f, y - 0.06f), -0.3f), new Vector3(x + hw + 0.25f, y + 0.02f, 0f), TownSwatch.Granite);
        }

        /// <summary>A square granite pillar: bronze base, plain shaft (a lit rune band when <paramref name="runes"/>), stepped bronze capital.</summary>
        public static void SquarePillar(TownMeshBuilder b, Vector3 foot, float height, float side, TownSwatch stone, TownSwatch bronze, bool runes)
        {
            float h = side * 0.5f;
            b.Box(foot + Vector3.up * 0.15f, new Vector3(side + 0.16f, 0.3f, side + 0.16f), bronze, 0f);
            b.Box(foot + Vector3.up * (height * 0.5f), new Vector3(side, height, side), stone, 0f);
            if (runes)
            {
                float y = foot.y + height * 0.45f;
                b.BoxMinMax(new Vector3(foot.x - h - 0.02f, y, foot.z - h - 0.02f), new Vector3(foot.x + h + 0.02f, y + 0.22f, foot.z + h + 0.02f), TownSwatch.RuneLight);
                b.BoxMinMax(new Vector3(foot.x - h - 0.03f, y + 0.08f, foot.z - h - 0.03f), new Vector3(foot.x + h + 0.03f, y + 0.14f, foot.z + h + 0.03f), TownSwatch.Bronze);
            }

            b.Box(foot + Vector3.up * (height - 0.18f), new Vector3(side + 0.14f, 0.16f, side + 0.14f), bronze, 0f);
            b.Box(foot + Vector3.up * (height - 0.05f), new Vector3(side + 0.3f, 0.12f, side + 0.3f), bronze, 0f, TownSwatch.Verdigris);
        }

        /// <summary>The gate of a city cut into rock: two square pillars with rune bands, a granite lintel with a lit rune line, bronze lamps.</summary>
        public static void RuneGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 along = (c - a).normalized;
            foreach (Vector3 p in new[] { a, c })
            {
                SquarePillar(b, p, 3.6f, 0.5f, TownSwatch.Granite, TownSwatch.Bronze, true);
                Lantern(b, p + Vector3.up * 2.2f + (p == a ? along : -along) * 0.45f, 0.75f);
            }

            Vector3 up = Vector3.up * 3.6f;
            Vector3 n = Vector3.Cross(along, Vector3.up).normalized * 0.32f;
            b.Log(a + up + Vector3.up * 0.22f - along * 0.4f, c + up + Vector3.up * 0.22f + along * 0.4f, 0.3f, TownSwatch.Granite, 4, TownSwatch.StoneDark);
            float length = Vector3.Distance(a, c);
            int strokes = Mathf.Max(4, Mathf.RoundToInt(length / 0.35f));
            for (int i = 1; i < strokes; i++)
            {
                Vector3 p = a + along * (length * i / strokes) + up + Vector3.up * 0.22f;
                TownSwatch glyph = (i % 2) == 0 ? TownSwatch.RuneLight : TownSwatch.Bronze;
                b.Box(p - n * 1.02f, new Vector3(0.06f, 0.24f, 0.06f), glyph, 0f);
                b.Box(p + n * 1.02f, new Vector3(0.06f, 0.24f, 0.06f), glyph, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>A free-standing bronze pillar of the halls: granite shaft, bronze base and capital, a band of runes that glows.</summary>
        private static void BronzePillar(TownMeshBuilder b, float s)
        {
            SquarePillar(b, Vector3.zero, 4.4f * s, BronzePillarSize * s, TownSwatch.Granite, TownSwatch.Bronze, true);
            b.Crown(new Vector3(0f, 4.5f * s, 0f), new Vector3(1.1f, 0.5f, 1.1f) * s, TownSwatch.StoneDark);
        }

        /// <summary>
        /// The Grande Forja, gone cold (front −z): a stepped granite furnace with its mouth dark (cold embers only), the bronze
        /// hood rising to a flue, the crystal of Light in its bronze claw on top (dark now), the great bronze bellows at the
        /// side and two anvils at the front. Relighting it is the story of map 17.
        /// </summary>
        private static void GreatForge(TownMeshBuilder b, float s)
        {
            float hw = GreatForgeSize.x * 0.5f * s;
            float hd = GreatForgeSize.y * 0.5f * s;
            b.BoxMinMax(new Vector3(-hw, 0f, -hd + 0.5f * s), new Vector3(hw, 1.1f * s, hd), TownSwatch.StoneDark, TownSwatch.Granite);
            b.BoxMinMax(new Vector3(-hw * 0.72f, 1.1f * s, -hd + 0.9f * s), new Vector3(hw * 0.72f, 2.6f * s, hd - 0.2f * s), TownSwatch.Granite);
            b.BoxMinMax(new Vector3(-hw * 0.76f, 2.6f * s, -hd + 0.85f * s), new Vector3(hw * 0.76f, 2.75f * s, hd - 0.15f * s), TownSwatch.Bronze);

            // The furnace mouth: an arch of bronze framing the dark hearth with cold embers.
            b.With(new Vector3(0f, 0f, -hd + 0.9f * s), 0f, () =>
            {
                b.BoxMinMax(new Vector3(-0.8f * s, 1.15f * s, -0.02f), new Vector3(0.8f * s, 2.0f * s, 0f), TownSwatch.Black);
                ArchFill(b, 0f, 2.0f * s, 1.6f * s, 0.4f * s, -0.02f, TownSwatch.Black);
                b.BoxMinMax(new Vector3(-0.95f * s, 1.1f * s, -0.08f), new Vector3(-0.8f * s, 2.0f * s, 0f), TownSwatch.Bronze);
                b.BoxMinMax(new Vector3(0.8f * s, 1.1f * s, -0.08f), new Vector3(0.95f * s, 2.0f * s, 0f), TownSwatch.Bronze);
                b.Crown(new Vector3(0f, 1.2f * s, -0.15f), new Vector3(1.2f, 0.25f, 0.5f) * s, TownSwatch.ColdEmber);
            });

            // The hood and its flue up into the rock.
            b.Cylinder(new Vector3(0f, 2.75f * s, 0.2f * s), hw * 0.95f, 0.45f * s, 1.6f * s, 4, TownSwatch.Bronze, TownSwatch.Verdigris, 0.5f);
            b.Box(new Vector3(0f, 5.1f * s, 0.2f * s), new Vector3(0.7f, 3.1f, 0.7f) * s, TownSwatch.Verdigris, 0f, TownSwatch.Black);

            // The crystal of Light in its claw, dark since the shadow put it out.
            var claw = new Vector3(-hw * 0.55f, 2.75f * s, -hd + 1.3f * s);
            b.Cylinder(claw, 0.3f * s, 0.2f * s, 0.3f * s, 6, TownSwatch.Bronze, TownSwatch.Bronze);
            b.Cylinder(claw + Vector3.up * 0.3f * s, 0.22f * s, 0f, 0.7f * s, 5, TownSwatch.Indigo);

            // The bellows (two bronze boards with a leather fold) on the right step, and two anvils at the front.
            var bellows = new Vector3(hw * 0.78f, 1.1f * s, 0.2f * s);
            b.Box(bellows + Vector3.up * 0.25f * s, new Vector3(0.9f, 0.08f, 1.6f) * s, TownSwatch.Bronze, 0f);
            b.Box(bellows + Vector3.up * 0.05f * s, new Vector3(0.9f, 0.3f, 1.4f) * s, TownSwatch.Walnut, 0f);
            b.Log(bellows + new Vector3(0f, 0.25f * s, -0.8f * s), bellows + new Vector3(-0.45f * s, 0.25f * s, -1.1f * s), 0.06f * s, TownSwatch.Verdigris, 5);
            foreach (float x in new[] { -hw * 0.45f, hw * 0.4f })
            {
                var anvil = new Vector3(x, 0f, -hd + 0.3f * s);
                b.Box(anvil + Vector3.up * 0.3f * s, new Vector3(0.35f, 0.6f, 0.35f) * s, TownSwatch.StoneDark, 0f);
                b.Box(anvil + Vector3.up * 0.7f * s, new Vector3(0.75f, 0.2f, 0.32f) * s, TownSwatch.Iron, 0f);
                b.Cylinder(anvil + new Vector3(0.38f * s, 0.7f * s, 0f), 0.1f * s, 0f, 0.3f * s, 4, TownSwatch.Iron, null, 0.5f);
            }
        }

        /// <summary>A pit of cold lava: a granite rim, the dark crust with cracks of cold ember and a few last glints, bronze chain posts.</summary>
        private static void LavaPit(TownMeshBuilder b, float s)
        {
            float r = LavaPitRadius * s;
            b.Ring(new Vector3(0f, 0.3f, 0f), r - 0.35f, r, 12, TownSwatch.Granite, TownSwatch.StoneDark);
            b.Cylinder(Vector3.zero, r, r, 0.3f, 12, TownSwatch.StoneDark, null);
            b.Disc(new Vector3(0f, 0.12f, 0f), r - 0.3f, 12, TownSwatch.Black);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 61f;
                float len = (r - 0.5f) * (0.55f + (i % 3) * 0.15f);
                b.Box(new Vector3(0f, 0.135f, 0f) + Quaternion.Euler(0f, a, 0f) * new Vector3(0f, 0f, len * 0.5f), new Vector3(0.06f, 0.02f, len), TownSwatch.ColdEmber, a);
            }

            b.Blob(new Vector3(0.3f * s, 0.15f, -0.2f * s), new Vector3(0.18f, 0.05f, 0.14f) * s, TownSwatch.Ember);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 30f) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r - 0.12f);
                b.Cylinder(p + Vector3.up * 0.3f, 0.05f, 0.05f, 0.75f, 5, TownSwatch.Bronze, TownSwatch.Verdigris);
                float a2 = (i * 60f + 90f) * Mathf.Deg2Rad;
                var q = new Vector3(Mathf.Cos(a2), 0f, Mathf.Sin(a2)) * (r - 0.12f);
                b.Log(p + Vector3.up * 0.95f, q + Vector3.up * 0.95f, 0.018f, TownSwatch.Iron, 4);
            }
        }

        /// <summary>The chain lift: four bronze posts with cross beams, the cage at the bottom (open front), the wheel on top and the chains up into the rock.</summary>
        private static void ChainLift(TownMeshBuilder b, float s)
        {
            float h = ChainLiftSize * 0.5f * s - 0.1f;
            float top = 6.4f * s;
            var corners = new[] { new Vector2(-h, -h), new Vector2(h, -h), new Vector2(h, h), new Vector2(-h, h) };
            foreach (Vector2 c in corners)
            {
                b.Box(new Vector3(c.x, top * 0.5f, c.y), new Vector3(0.18f, top, 0.18f), TownSwatch.Bronze, 0f);
            }

            foreach (float y in new[] { 2.5f * s, top - 0.2f })
            {
                b.BoxMinMax(new Vector3(-h, y - 0.1f, -h - 0.1f), new Vector3(h, y + 0.1f, -h + 0.1f), TownSwatch.Verdigris);
                b.BoxMinMax(new Vector3(-h, y - 0.1f, h - 0.1f), new Vector3(h, y + 0.1f, h + 0.1f), TownSwatch.Verdigris);
                b.BoxMinMax(new Vector3(-h - 0.1f, y - 0.1f, -h), new Vector3(-h + 0.1f, y + 0.1f, h), TownSwatch.Verdigris);
                b.BoxMinMax(new Vector3(h - 0.1f, y - 0.1f, -h), new Vector3(h + 0.1f, y + 0.1f, h), TownSwatch.Verdigris);
            }

            // The cage: a floor, a roof and bars on three sides (the front, −z, open).
            float c0 = h - 0.25f;
            b.BoxMinMax(new Vector3(-c0, 0f, -c0), new Vector3(c0, 0.15f, c0), TownSwatch.WoodDark, TownSwatch.Granite);
            b.BoxMinMax(new Vector3(-c0, 2.1f * s, -c0), new Vector3(c0, 2.2f * s, c0), TownSwatch.Bronze);
            for (int i = 0; i <= 4; i++)
            {
                float t = -c0 + 2f * c0 * i / 4f;
                b.Log(new Vector3(t, 0.15f, c0), new Vector3(t, 2.1f * s, c0), 0.025f, TownSwatch.Iron, 4);
                b.Log(new Vector3(-c0, 0.15f, t), new Vector3(-c0, 2.1f * s, t), 0.025f, TownSwatch.Iron, 4);
                b.Log(new Vector3(c0, 0.15f, t), new Vector3(c0, 2.1f * s, t), 0.025f, TownSwatch.Iron, 4);
            }

            // The wheel and the chains.
            b.Log(new Vector3(-0.15f * s, top + 0.25f, 0f), new Vector3(0.15f * s, top + 0.25f, 0f), 0.6f * s, TownSwatch.Bronze, 10, TownSwatch.Verdigris);
            foreach (float x in new[] { -0.45f * s, 0.45f * s })
            {
                b.Log(new Vector3(x, 2.2f * s, 0f), new Vector3(x * 0.4f, top + 0.2f, 0f), 0.03f, TownSwatch.Iron, 4);
            }

            b.Crown(new Vector3(0f, top + 0.9f, 0.3f * s), new Vector3(2.6f, 1.2f, 2.4f) * s, TownSwatch.StoneDark);
            Lantern(b, new Vector3(-h - 0.05f, 2.1f * s, -h - 0.35f), 0.7f);
        }

        /// <summary>
        /// The rock wall of a city inside the mountain from the origin to <paramref name="d"/> (both faces dressed; the mass
        /// is <see cref="RockWallHalfThickness"/> each side of the line). Variant 0: granite, carved niches with bronze
        /// lintels between bronze ribs, a line of runes and a rough crest; 1: basalt tunnel wall with a copper air pipe on
        /// brackets and bottle lamps of green and amber glass (the Mercado dos Túneis).
        /// </summary>
        private static void RockWall(TownMeshBuilder b, Vector2 d, float s, int variant)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            bool tunnel = variant == 1;
            float yaw = YawAlong(d / length);
            float t = RockWallHalfThickness;
            float height = (tunnel ? 5.2f : 6.4f) * s;
            TownSwatch rock = tunnel ? TownSwatch.Basalt : TownSwatch.Granite;
            b.With(Vector3.zero, yaw, () =>
            {
                string key = "rockwall" + variant + "_" + Mathf.RoundToInt(length * 10f);
                b.BoxMinMax(new Vector3(-0.05f, 0f, -t), new Vector3(length + 0.05f, height, t), rock, TownSwatch.StoneDark);
                int bays = Mathf.Max(1, Mathf.RoundToInt(length / 3f));
                float w = length / bays;
                for (int i = 0; i < bays; i++)
                {
                    float x0 = i * w;
                    float xm = x0 + w * 0.5f;
                    float crest = height + (0.4f + TownStyle.Hash01(key, i) * 1.1f) * s;
                    b.Crown(new Vector3(xm, crest - 0.3f, 0f), new Vector3(w * 1.2f, 1.4f * s, t * 2.6f), (i & 1) == 0 ? rock : TownSwatch.StoneDark, i * 23f);
                    foreach (float face in new[] { -1f, 1f })
                    {
                        float z = face * (t + 0.01f);
                        if (tunnel)
                        {
                            // Bottle lamps hung from the pipe brackets, green and amber by turns.
                            b.Box(new Vector3(xm, 2.25f, z + face * 0.12f), new Vector3(0.2f, 0.3f, 0.2f), (i & 1) == 0 ? TownSwatch.GlassAmber : TownSwatch.GlassGreen, 0f);
                            b.Log(new Vector3(xm, 2.7f, z), new Vector3(xm, 2.42f, z + face * 0.12f), 0.015f, TownSwatch.Iron, 4);
                            b.Box(new Vector3(x0 + 0.1f, 2.7f, z + face * 0.08f), new Vector3(0.08f, 0.3f, 0.16f), TownSwatch.Iron, 0f);
                            // Arch ribs of the round tunnel.
                            b.BoxMinMax(new Vector3(x0 - 0.12f, 0f, Mathf.Min(z, z + face * 0.18f)), new Vector3(x0 + 0.12f, height - 0.2f, Mathf.Max(z, z + face * 0.18f)), TownSwatch.StoneDark);
                        }
                        else
                        {
                            // A niche with a bronze lintel; a bronze rib between the bays.
                            b.BoxMinMax(new Vector3(xm - 0.5f, 0.5f, Mathf.Min(z, z - face * 0.02f)), new Vector3(xm + 0.5f, 2.3f, Mathf.Max(z, z - face * 0.02f)), TownSwatch.Black);
                            b.BoxMinMax(new Vector3(xm - 0.65f, 2.3f, Mathf.Min(z, z + face * 0.1f)), new Vector3(xm + 0.65f, 2.45f, Mathf.Max(z, z + face * 0.1f)), TownSwatch.Bronze);
                            b.BoxMinMax(new Vector3(xm - 0.6f, 0.42f, Mathf.Min(z, z + face * 0.12f)), new Vector3(xm + 0.6f, 0.5f, Mathf.Max(z, z + face * 0.12f)), TownSwatch.Granite);
                            b.BoxMinMax(new Vector3(x0 - 0.08f, 0f, Mathf.Min(z, z + face * 0.1f)), new Vector3(x0 + 0.08f, height, Mathf.Max(z, z + face * 0.1f)), TownSwatch.Bronze);
                            if ((i % 2) == 1)
                            {
                                b.BoxMinMax(new Vector3(xm - 0.25f, 1.2f, Mathf.Min(z, z + face * 0.05f)), new Vector3(xm + 0.25f, 1.6f, Mathf.Max(z, z + face * 0.05f)), TownSwatch.RuneLight);
                            }
                        }
                    }
                }

                if (tunnel)
                {
                    foreach (float face in new[] { -1f, 1f })
                    {
                        float z = face * (t + 0.2f);
                        b.Log(new Vector3(0f, 2.75f, z), new Vector3(length, 2.75f, z), 0.12f, TownSwatch.Copper, 6);
                    }
                }
                else
                {
                    foreach (float face in new[] { -1f, 1f })
                    {
                        float z = face * (t + 0.03f);
                        b.BoxMinMax(new Vector3(0f, 3.4f, Mathf.Min(z, z + face * 0.04f)), new Vector3(length, 3.48f, Mathf.Max(z, z + face * 0.04f)), TownSwatch.RuneLight);
                    }
                }
            });
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Puzur's front: a bronze stand of rune plates, a granite slate of runes on the wall, a little rune lamp.</summary>
        private static void RuneHallDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Box(new Vector3(x, y + 0.45f, z - 0.2f), new Vector3(0.9f, 0.9f, 0.4f), TownSwatch.Granite, 0f, TownSwatch.Bronze);
            for (int i = 0; i < 3; i++)
            {
                float px = x - 0.28f + i * 0.28f;
                b.Box(new Vector3(px, y + 1.05f, z - 0.2f), new Vector3(0.22f, 0.3f, 0.04f), TownSwatch.Bronze, 8f * (i - 1));
                b.Box(new Vector3(px, y + 1.07f, z - 0.225f), new Vector3(0.06f, 0.16f, 0.01f), TownSwatch.RuneLight, 8f * (i - 1));
            }

            b.BoxMinMax(new Vector3(x - 0.55f, y + 1.3f, zFacade - 0.05f), new Vector3(x + 0.55f, y + 2.0f, zFacade - 0.01f), TownSwatch.Slate);
            for (int i = 0; i < 4; i++)
            {
                b.BoxMinMax(new Vector3(x - 0.42f + i * 0.26f, y + 1.45f, zFacade - 0.06f), new Vector3(x - 0.36f + i * 0.26f, y + 1.85f, zFacade - 0.05f), TownSwatch.RuneLight);
            }

            b.Cylinder(new Vector3(x + 0.7f, y, z - 0.35f), 0.08f, 0.06f, 0.9f, 5, TownSwatch.Bronze, TownSwatch.Bronze);
            b.Box(new Vector3(x + 0.7f, y + 1.0f, z - 0.35f), new Vector3(0.16f, 0.2f, 0.16f), TownSwatch.RuneLight, 0f);
        }
    }
}
