using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Kirta, the reef city of the povo-do-mar on its atoll (Docs/Mapas100.md §3.3 and map 28): the coral
    /// towers (<see cref="ReefTowerHouse"/>: a round tower of live coral rising from a drowned base (the old waterline, dim
    /// and barnacled), round windows, a mother-of-pearl dome, a low coral annex with the round-headed door and a
    /// scallop-shell canopy over the deck), the mother coral in its pool (one side dimmed grey by the Lodo, the other side
    /// relit), free-standing coral spires, kelp gardens, the reef water crossed by shell bridges, the reef edge over the
    /// abyss in the south and the coral gate at the streets' ends; Danel's diving gear. Same rules as the rest of the kit:
    /// flat colours from <see cref="TownPalette"/> (row 10 holds live coral, reef turquoise, kelp and the dimmed coral),
    /// low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of the <see cref="CityPropKind.CoralMother"/>'s pool (× its scale): its collider.</summary>
        public const float CoralMotherRadius = 2.3f;

        /// <summary>Radius of a <see cref="CityPropKind.CoralSpire"/>'s drowned base (× its scale): its collider.</summary>
        public const float CoralSpireRadius = 1.4f;

        /// <summary>Radius of a <see cref="CityPropKind.KelpGarden"/>'s ring of shells (× its scale): its collider.</summary>
        public const float KelpGardenRadius = 1.4f;

        /// <summary>Half the thickness of a <see cref="CityPropKind.ReefEdge"/>'s coral rim: its collider is the run inflated by this.</summary>
        public const float ReefEdgeHalfThickness = 0.5f;

        /// <summary>Half the width of the reef water of a <see cref="CityPropKind.ShellBridge"/>.</summary>
        public const float ReefChannelHalfWidth = 0.8f;

        // ------------------------------------------------------------------
        // The coral tower
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.ReefTower"/> house (local frame, front −z): a sand platform, the low coral annex across
        /// the whole front (dim and barnacled below the old waterline, live coral above) with the round-headed door and round
        /// windows, the round tower rising behind it to a mother-of-pearl dome, coral knobs and kelp at the foot, and the
        /// scallop canopy over the deck on pearl posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list).
        /// </summary>
        public static void ReefTowerHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float y0 = TownBuildingBuilder.FloorY;
            TownSwatch live = spec.WallColor;
            const TownSwatch dim = TownSwatch.CoralDim;
            b.BoxMinMax(new Vector3(-hw, 0f, zFacade), new Vector3(hw, y0, zBack), spec.PlinthColor, TownSwatch.Sand);

            // The round tower behind the annex: drowned base, live shaft, a teal band, the dome.
            float cz = (zFacade + zBack) * 0.5f + 0.2f;
            float r = Mathf.Min(hw, (zBack - zFacade) * 0.5f) - 0.1f;
            float waterline = y0 + 1.15f;
            float towerTop = top + 1.6f;
            b.Cylinder(new Vector3(0f, y0, cz), r, r * 0.97f, waterline - y0, 10, dim);
            b.Cylinder(new Vector3(0f, waterline, cz), r * 0.97f, r * 0.82f, towerTop - waterline, 10, live);
            b.Cylinder(new Vector3(0f, towerTop - 0.25f, cz), r * 0.86f, r * 0.86f, 0.25f, 10, spec.TrimColor, spec.TrimColor);
            Dome(b, new Vector3(0f, towerTop, cz), r * 0.62f, spec.TrimColor, spec.RoofColor, TownSwatch.CoralLive);
            for (int i = 0; i < 7; i++)
            {
                float a = (i * 51f + 20f) * Mathf.Deg2Rad;
                float y = waterline + 0.4f + (i % 3) * (towerTop - waterline - 0.8f) / 3f;
                float rr = Mathf.Lerp(r * 0.97f, r * 0.82f, (y - waterline) / (towerTop - waterline));
                b.Blob(new Vector3(Mathf.Cos(a) * rr, y, cz + Mathf.Sin(a) * rr), new Vector3(0.3f, 0.26f, 0.3f), (i % 2) == 0 ? live : TownSwatch.Pearl);
            }

            // Round windows up the front of the tower (lit at night).
            foreach (Vector2 w in new[] { new Vector2(-r * 0.35f, top + 0.3f), new Vector2(r * 0.3f, top + 1.0f) })
            {
                float y = w.y;
                float rr = Mathf.Lerp(r * 0.97f, r * 0.82f, (y - waterline) / (towerTop - waterline));
                float zFace = cz - Mathf.Sqrt(Mathf.Max(0.01f, rr * rr - w.x * w.x)) - 0.02f;
                b.With(new Vector3(0f, 0f, zFace), 0f, () => RoundWindow(b, w.x, y, 0.28f, spec.TrimColor));
            }

            // The low annex across the front: drowned band, live coral above, a lip of pearl.
            float annexTop = Mathf.Min(top, y0 + 2.75f);
            float annexBack = Mathf.Min(zBack, zFacade + 1.4f);
            b.BoxMinMax(new Vector3(-hw, y0, zFacade), new Vector3(hw, waterline, annexBack), dim);
            b.BoxMinMax(new Vector3(-hw, waterline, zFacade), new Vector3(hw, annexTop, annexBack), live);
            b.BoxMinMax(new Vector3(-hw - 0.08f, annexTop, zFacade - 0.08f), new Vector3(hw + 0.08f, annexTop + 0.14f, annexBack + 0.05f), TownSwatch.Pearl);
            b.BoxMinMax(new Vector3(-hw - 0.03f, waterline - 0.06f, zFacade - 0.03f), new Vector3(hw + 0.03f, waterline + 0.06f, annexBack), spec.TrimColor);
            for (int i = 0; i < 9; i++)
            {
                float x = -hw + 0.3f + (hw * 2f - 0.6f) * TownStyle.Hash01(spec.Seed, 40 + i);
                float y = y0 + 0.2f + 0.8f * TownStyle.Hash01(spec.Seed, 60 + i);
                b.Blob(new Vector3(x, y, zFacade - 0.03f), new Vector3(0.12f, 0.1f, 0.06f), TownSwatch.Pearl);
            }

            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.4f : 1.0f;
            float doorSpring = y0 + 1.55f;
            b.With(new Vector3(0f, 0f, zFacade - 0.02f), 0f, () =>
            {
                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, y0, -0.01f), new Vector3(doorX + doorW * 0.5f, doorSpring, 0f), TownSwatch.Black);
                RoundArchFill(b, doorX, doorSpring, doorW * 0.5f, -0.01f, TownSwatch.Black);
                const int shells = 9;
                for (int i = 0; i <= shells; i++)
                {
                    float a = Mathf.PI * i / shells;
                    var p = new Vector3(doorX + Mathf.Cos(a) * (doorW * 0.5f + 0.12f), doorSpring + Mathf.Sin(a) * (doorW * 0.5f + 0.12f), -0.06f);
                    b.Box(p, new Vector3(0.2f, 0.16f, 0.1f), (i % 2) == 0 ? TownSwatch.Pearl : spec.TrimColor, 0f);
                }

                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.2f, y0, -0.08f), new Vector3(doorX - doorW * 0.5f - 0.04f, doorSpring, 0f), TownSwatch.Pearl);
                b.BoxMinMax(new Vector3(doorX + doorW * 0.5f + 0.04f, y0, -0.08f), new Vector3(doorX + doorW * 0.5f + 0.2f, doorSpring, 0f), TownSwatch.Pearl);

                // A round window each side of the door, where there is room.
                foreach (float side in new[] { -1f, 1f })
                {
                    float wx = doorX + side * (doorW * 0.5f + 0.95f);
                    if (Mathf.Abs(wx) < hw - 0.45f)
                    {
                        RoundWindow(b, wx, y0 + 1.6f, 0.26f, TownSwatch.Pearl);
                    }
                }
            });

            // Kelp at the foot of the walls.
            foreach (float x in new[] { -hw + 0.25f, hw - 0.25f })
            {
                Kelp(b, new Vector3(x, 0f, zFacade - 0.1f), 1.1f, 0.6f, x < 0f ? 1 : 2);
            }

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), dim, TownSwatch.Pearl);
                float z = zFront + TownBuildingBuilder.PostInset;
                float low = Mathf.Min(annexTop - 0.1f, y0 + 2.4f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x, low + 0.05f, z), 0.06f, TownSwatch.Pearl, 6);
                    b.Blob(new Vector3(x, low + 0.08f, z), new Vector3(0.16f, 0.12f, 0.16f), spec.TrimColor);
                }

                // The scallop canopy: ribs fanning from the annex lip down to the posts, pearl and live coral by turns, a frilled edge.
                int ribs = Mathf.Max(5, Mathf.RoundToInt(spec.Width / 0.6f));
                float w = (spec.Width + 0.2f) / ribs;
                for (int i = 0; i < ribs; i++)
                {
                    float x0 = -hw - 0.1f + i * w;
                    float x1 = x0 + w;
                    TownSwatch rib = (i % 2) == 0 ? TownSwatch.Pearl : live;
                    b.TwoSidedQuad(new Vector3(x0, annexTop, zFacade + 0.02f), new Vector3(x1, annexTop, zFacade + 0.02f), new Vector3(x1, low, z - 0.15f), new Vector3(x0, low, z - 0.15f), rib);
                    b.Blob(new Vector3((x0 + x1) * 0.5f, low - 0.04f, z - 0.18f), new Vector3(w * 0.5f, 0.1f, 0.08f), rib);
                }
            }
        }

        /// <summary>A clump of kelp ribbons at <paramref name="foot"/>: zig-zag two-sided strips, kelp green and reef turquoise.</summary>
        private static void Kelp(TownMeshBuilder b, Vector3 foot, float height, float spread, int seed)
        {
            for (int k = 0; k < 4; k++)
            {
                float a = (k * 97f + seed * 31f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * spread * 0.25f;
                Vector3 p = foot + dir;
                float h = height * (0.75f + 0.25f * (k % 2));
                const int segs = 3;
                for (int i = 0; i < segs; i++)
                {
                    float y0 = h * i / segs;
                    float y1 = h * (i + 1) / segs;
                    float sway0 = ((i % 2) == 0 ? 0.08f : -0.08f);
                    float sway1 = -sway0;
                    TownSwatch c = (k % 2) == 0 ? TownSwatch.Kelp : TownSwatch.ReefTeal;
                    b.TwoSidedQuad(p + new Vector3(sway0 - 0.06f, y0, 0f), p + new Vector3(sway0 + 0.06f, y0, 0f), p + new Vector3(sway1 + 0.05f, y1, 0f), p + new Vector3(sway1 - 0.05f, y1, 0f), c);
                }
            }
        }

        /// <summary>The coral gate of Kirta: two coral pillars rising from a drowned foot and an arch of shells over the road.</summary>
        public static void CoralGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 along = (c - a).normalized;
            Vector3 mid = (a + c) * 0.5f;
            float r = Vector3.Distance(a, c) * 0.5f;
            const float pier = 2.4f;
            foreach (Vector3 p in new[] { a, c })
            {
                b.Cylinder(p, 0.42f, 0.4f, 1.0f, 8, TownSwatch.CoralDim);
                b.Cylinder(p + Vector3.up * 1.0f, 0.4f, 0.32f, pier - 1.0f, 8, TownSwatch.CoralLive);
                b.Blob(p + Vector3.up * 1.6f + (p == a ? along : -along) * 0.3f, new Vector3(0.22f, 0.2f, 0.22f), TownSwatch.Pearl);
                Kelp(b, p - along * 0.1f, 1.0f, 0.5f, p == a ? 3 : 4);
            }

            const int shells = 13;
            float yaw = Mathf.Atan2(along.x, along.z) * Mathf.Rad2Deg;
            for (int i = 0; i <= shells; i++)
            {
                float t = Mathf.PI * i / shells;
                Vector3 p = mid - along * Mathf.Cos(t) * r + Vector3.up * (pier + Mathf.Sin(t) * r * 0.8f);
                b.Box(p, new Vector3(0.48f, 0.4f, 0.5f), (i % 3) == 1 ? TownSwatch.ReefTeal : (i % 3) == 2 ? TownSwatch.CoralLive : TownSwatch.Pearl, yaw);
            }

            b.Blob(mid + Vector3.up * (pier + r * 0.8f + 0.35f), new Vector3(0.5f, 0.4f, 0.3f), TownSwatch.Pearl);
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The mother coral in its pool: a rim of dim coral, the reef water, and the great branching coral: the west half still
        /// grey from the Lodo, the east half relit (live coral that glows a little at night). Relighting it all is map 28's story.
        /// </summary>
        private static void CoralMother(TownMeshBuilder b, float s)
        {
            float r = CoralMotherRadius * s;
            b.Ring(new Vector3(0f, 0.4f, 0f), r - 0.4f, r, 14, TownSwatch.CoralDim, TownSwatch.Pearl);
            b.Cylinder(Vector3.zero, r, r, 0.4f, 14, TownSwatch.CoralDim);
            b.Disc(new Vector3(0f, 0.3f, 0f), r - 0.38f, 14, TownSwatch.Sea);
            var trunk = new Vector3(0f, 1.3f * s, 0f);
            b.Log(new Vector3(0f, 0.3f, 0f), trunk, 0.28f * s, TownSwatch.CoralDim, 7);
            for (int i = 0; i < 7; i++)
            {
                float a = (i * 51.4f + 10f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                bool lit = dir.x > 0.05f;
                TownSwatch coral = lit ? TownSwatch.CoralLive : TownSwatch.CoralDim;
                Vector3 mid = trunk + dir * 0.7f * s + Vector3.up * (0.8f + (i % 2) * 0.3f) * s;
                Vector3 tip = mid + dir * 0.5f * s + Vector3.up * (0.7f + (i % 3) * 0.25f) * s;
                b.Log(trunk, mid, 0.14f * s, coral, 6);
                b.Log(mid, tip, 0.09f * s, coral, 5);
                Vector3 twig = mid + new Vector3(-dir.z, 0f, dir.x) * 0.35f * s + Vector3.up * 0.45f * s;
                b.Log(mid, twig, 0.06f * s, coral, 4);
                b.Blob(tip, new Vector3(0.22f, 0.22f, 0.22f) * s, lit ? TownSwatch.CoralLive : TownSwatch.StoneDark);
                b.Blob(twig, new Vector3(0.14f, 0.14f, 0.14f) * s, lit ? TownSwatch.ReefTeal : TownSwatch.CoralDim);
            }

            Kelp(b, new Vector3(-r * 0.55f, 0.3f, r * 0.3f), 0.9f * s, 0.6f, 5);
            Kelp(b, new Vector3(r * 0.5f, 0.3f, -r * 0.35f), 0.8f * s, 0.5f, 6);
        }

        /// <summary>A free-standing coral spire: a drowned base, a tapering shaft of live coral with knobs and lit round windows, a pearl dome.</summary>
        private static void CoralSpire(TownMeshBuilder b, float s)
        {
            float r = CoralSpireRadius * s;
            float waterline = 1.4f * s;
            float shaftTop = 6.4f * s;
            b.Cylinder(Vector3.zero, r, r * 0.95f, waterline, 9, TownSwatch.CoralDim);
            b.Cylinder(new Vector3(0f, waterline, 0f), r * 0.85f, r * 0.55f, shaftTop - waterline, 9, TownSwatch.CoralLive);
            b.Cylinder(new Vector3(0f, waterline - 0.06f, 0f), r * 0.97f, r * 0.97f, 0.12f, 9, TownSwatch.ReefTeal, TownSwatch.ReefTeal);
            for (int i = 0; i < 9; i++)
            {
                float a = (i * 67f) * Mathf.Deg2Rad;
                float y = waterline + 0.5f + (i % 4) * (shaftTop - waterline - 1f) / 4f;
                float rr = Mathf.Lerp(r * 0.85f, r * 0.55f, (y - waterline) / (shaftTop - waterline));
                b.Blob(new Vector3(Mathf.Cos(a) * rr, y, Mathf.Sin(a) * rr), new Vector3(0.28f, 0.24f, 0.28f) * s, (i % 3) == 0 ? TownSwatch.Pearl : TownSwatch.CoralLive);
            }

            foreach (float y in new[] { 2.6f, 4.4f })
            {
                float rr = Mathf.Lerp(r * 0.85f, r * 0.55f, (y * s - waterline) / (shaftTop - waterline));
                b.With(new Vector3(0f, 0f, -rr - 0.01f), 0f, () => RoundWindow(b, 0f, y * s, 0.22f * s, TownSwatch.Pearl));
            }

            Dome(b, new Vector3(0f, shaftTop, 0f), r * 0.5f, TownSwatch.ReefTeal, TownSwatch.Pearl, TownSwatch.CoralLive);
            Kelp(b, new Vector3(-r * 0.8f, 0f, -r * 0.4f), 1.2f * s, 0.6f, 7);
        }

        /// <summary>A kelp garden: a sand bed in a ring of shells, tall ribbons of kelp and a few pearl knobs.</summary>
        private static void KelpGarden(TownMeshBuilder b, float s)
        {
            float r = KelpGardenRadius * s;
            b.Disc(new Vector3(0f, 0.04f, 0f), r - 0.1f, 12, TownSwatch.SandDark);
            const int shells = 14;
            for (int i = 0; i < shells; i++)
            {
                float a = i * Mathf.PI * 2f / shells;
                b.Blob(new Vector3(Mathf.Cos(a) * r, 0.1f, Mathf.Sin(a) * r), new Vector3(0.3f, 0.18f, 0.3f), (i % 3) == 0 ? TownSwatch.CoralLive : TownSwatch.Pearl);
            }

            for (int k = 0; k < 5; k++)
            {
                float a = (k * 72f + 15f) * Mathf.Deg2Rad;
                Kelp(b, new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * r * 0.5f, (1.6f + (k % 2) * 0.5f) * s, 0.8f, 10 + k);
            }

            Kelp(b, Vector3.zero, 2.2f * s, 0.6f, 20);
        }

        /// <summary>
        /// Reef water from the origin to <paramref name="d"/> (a strip <see cref="ReefChannelHalfWidth"/> each side, coral lips)
        /// and, at its middle, a shell bridge across it: a pearl deck rising in a hump with a rail of shells. Walkable.
        /// </summary>
        private static void ShellBridge(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = ReefChannelHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.06f, TownSwatch.Sea);
                foreach (float z in new[] { -w, w })
                {
                    b.BoxMinMax(new Vector3(0f, 0f, z - 0.12f), new Vector3(length, 0.12f, z + 0.12f), TownSwatch.CoralDim, TownSwatch.Pearl);
                }

                // The bridge across the water at the middle (along local z): a humped deck, posts and a shell rail.
                float xm = length * 0.5f;
                const int planks = 6;
                float bw = w + 0.5f;
                for (int i = 0; i < planks; i++)
                {
                    float z0 = -bw + 2f * bw * i / planks;
                    float z1 = z0 + 2f * bw / planks;
                    float h0 = 0.08f + 0.22f * Mathf.Sin(Mathf.PI * i / planks);
                    float h1 = 0.08f + 0.22f * Mathf.Sin(Mathf.PI * (i + 1) / planks);
                    b.Slab(new Vector3(xm - 0.75f, h0, z0), new Vector3(xm - 0.75f, h1, z1), new Vector3(xm + 0.75f, h1, z1), new Vector3(xm + 0.75f, h0, z0), 0.08f,
                        (i % 2) == 0 ? TownSwatch.Pearl : TownSwatch.Sail, TownSwatch.CoralDim);
                }

                foreach (float x in new[] { xm - 0.8f, xm + 0.8f })
                {
                    for (int i = 0; i <= 4; i++)
                    {
                        float z = -bw + 2f * bw * i / 4f;
                        float h = 0.08f + 0.22f * Mathf.Sin(Mathf.PI * i / 4f);
                        b.Log(new Vector3(x, h, z), new Vector3(x, h + 0.55f, z), 0.04f, TownSwatch.Pearl, 4);
                        b.Blob(new Vector3(x, h + 0.6f, z), new Vector3(0.14f, 0.1f, 0.14f), (i % 2) == 0 ? TownSwatch.CoralLive : TownSwatch.Pearl);
                    }
                }
            });
        }

        /// <summary>
        /// The edge of the reef from the origin to <paramref name="d"/>: a low rim of dim coral with live knobs (its collider
        /// is the run inflated by <see cref="ReefEdgeHalfThickness"/>) and beyond it, on the back side, the deep water of the abyss.
        /// </summary>
        private static void ReefEdge(TownMeshBuilder b, Vector2 d, float s)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float t = ReefEdgeHalfThickness;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.BoxMinMax(new Vector3(-0.05f, 0f, -t), new Vector3(length + 0.05f, 0.85f * s, t), TownSwatch.CoralDim, TownSwatch.Pearl);
                b.Flat(-0.5f, t, length + 0.5f, t + 7f, 0.03f, TownSwatch.Indigo);
                b.Flat(-0.5f, t, length + 0.5f, t + 1.6f, 0.045f, TownSwatch.Sea);
                int knobs = Mathf.Max(2, Mathf.RoundToInt(length / 1.3f));
                for (int i = 0; i < knobs; i++)
                {
                    float x = (i + 0.5f) * length / knobs;
                    float h = 0.85f * s + 0.15f + 0.3f * TownStyle.Hash01("reefedge" + Mathf.RoundToInt(length), i);
                    b.Blob(new Vector3(x, h, 0f), new Vector3(0.55f, 0.4f, 0.6f), (i % 3) == 0 ? TownSwatch.CoralLive : (i % 3) == 1 ? TownSwatch.CoralDim : TownSwatch.Pearl);
                    if ((i % 4) == 2)
                    {
                        Kelp(b, new Vector3(x + 0.3f, 0f, -t - 0.1f), 0.9f, 0.4f, 30 + i);
                    }
                }
            });
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Danel's front: a rack of breathing shells, jars of bottled air bubbles, diving stones on a cord and a conch whistle.</summary>
        private static void CoralReefDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            foreach (float lx in new[] { -0.6f, 0.6f })
            {
                b.Log(new Vector3(x + lx, y, z - 0.3f), new Vector3(x + lx, y + 1.35f, z - 0.3f), 0.04f, TownSwatch.Teak, 4);
            }

            b.Log(new Vector3(x - 0.68f, y + 1.3f, z - 0.3f), new Vector3(x + 0.68f, y + 1.3f, z - 0.3f), 0.035f, TownSwatch.Teak, 4);
            for (int i = 0; i < 4; i++)
            {
                float sx = x - 0.45f + i * 0.3f;
                b.Log(new Vector3(sx, y + 1.3f, z - 0.3f), new Vector3(sx, y + 1.05f, z - 0.3f), 0.008f, TownSwatch.Thatch, 3);
                b.Blob(new Vector3(sx, y + 0.95f, z - 0.3f), new Vector3(0.2f, 0.18f, 0.12f), (i % 2) == 0 ? TownSwatch.Pearl : TownSwatch.CoralLive);
            }

            for (int i = 0; i < 3; i++)
            {
                var jar = new Vector3(x - 0.4f + i * 0.4f, y, z - 0.75f);
                b.Cylinder(jar, 0.13f, 0.11f, 0.32f, 7, TownSwatch.Glass, TownSwatch.Kelp);
                b.Blob(jar + Vector3.up * 0.16f, new Vector3(0.12f, 0.12f, 0.12f), TownSwatch.ReefTeal);
            }

            b.Blob(new Vector3(x + 0.9f, y + 0.12f, z - 0.5f), new Vector3(0.24f, 0.2f, 0.24f), TownSwatch.StoneDark);
            b.Blob(new Vector3(x + 1.05f, y + 0.1f, z - 0.3f), new Vector3(0.2f, 0.18f, 0.2f), TownSwatch.StoneDark);
            b.Cylinder(new Vector3(x - 0.95f, y, z - 0.55f), 0.16f, 0.02f, 0.42f, 6, TownSwatch.Pearl, null);
        }
    }
}
