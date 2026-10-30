using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The modular low-poly architecture kit of the walkable towns (code-built, shared palette): plinths, plaster bands
    /// and timber frames; doors with pointed-arch frames and carved rosettes, latticed windows, round windows, lamp
    /// niches; flat roofs with parapets and beam ends, domes on drums, tiled gables, upper rooms; the aivan (deep porch)
    /// with carved Sogdian columns (fan capitals), beam and joisted ceiling; the pishtaq portal, the lamp tower, chimneys,
    /// stork nests, sign boards and shop dressings. Façade modules work in a "face frame" (x across, y up, −z outwards)
    /// set by <see cref="TownMeshBuilder.With(Vector3, float, System.Action)"/>; the rest in the building's frame.
    /// Region-specific modules (rondavel, tuff facade, dry-stone wall, stela) live next to them so a new city only needs data.
    /// </summary>
    public static partial class TownKit
    {
        // ------------------------------------------------------------------
        // Mass
        // ------------------------------------------------------------------

        public static void Plinth(TownMeshBuilder b, float x0, float x1, float z0, float z1, TownSwatch swatch)
        {
            b.BoxMinMax(new Vector3(x0 - 0.06f, 0f, z0 - 0.06f), new Vector3(x1 + 0.06f, TownBuildingBuilder.FloorY + 0.05f, z1 + 0.06f), swatch);
        }

        /// <summary>The lower plaster band (and corner reinforcement) over the adobe, a classic of mud-brick houses.</summary>
        public static void PlasterBand(TownMeshBuilder b, float x0, float x1, float z0, float z1, float y0, TownSwatch swatch)
        {
            b.BoxMinMax(new Vector3(x0 - 0.025f, y0, z0 - 0.025f), new Vector3(x1 + 0.025f, y0 + 0.5f, z1 + 0.025f), swatch);
        }

        /// <summary>Dark timber posts and rails over a light wall (stable, carpenter).</summary>
        public static void TimberFrame(TownMeshBuilder b, float x0, float x1, float z0, float z1, float y0, float y1, TownSwatch swatch)
        {
            const float t = 0.12f;
            foreach (Vector2 corner in new[] { new Vector2(x0, z0), new Vector2(x1, z0), new Vector2(x0, z1), new Vector2(x1, z1) })
            {
                b.Box(new Vector3(corner.x, (y0 + y1) * 0.5f, corner.y), new Vector3(t + 0.04f, y1 - y0, t + 0.04f), swatch);
            }

            foreach (float y in new[] { y0 + 0.06f, (y0 + y1) * 0.5f, y1 - 0.06f })
            {
                b.BoxMinMax(new Vector3(x0 - 0.03f, y - 0.06f, z0 - 0.03f), new Vector3(x1 + 0.03f, y + 0.06f, z1 + 0.03f), swatch);
            }

            for (float x = x0 + 1.2f; x < x1 - 0.6f; x += 1.2f)
            {
                b.BoxMinMax(new Vector3(x - 0.06f, y0, z0 - 0.03f), new Vector3(x + 0.06f, y1, z0), swatch);
                b.BoxMinMax(new Vector3(x - 0.06f, y0, z1), new Vector3(x + 0.06f, y1, z1 + 0.03f), swatch);
            }
        }

        // ------------------------------------------------------------------
        // Openings (face frame: x across, y up, −z outwards)
        // ------------------------------------------------------------------

        /// <summary>A pointed (ogee-ish) arch outline as a flat fan in the face plane at depth <paramref name="z"/>.</summary>
        public static void ArchFill(TownMeshBuilder b, float x, float y, float width, float rise, float z, TownSwatch swatch)
        {
            float hw = width * 0.5f;
            var apex = new Vector3(x, y + rise, z);
            const int steps = 4;
            for (int i = 0; i < steps; i++)
            {
                float t0 = (float)i / steps;
                float t1 = (float)(i + 1) / steps;
                Vector3 l0 = ArchPoint(x, y, hw, rise, -1f, t0, z);
                Vector3 l1 = ArchPoint(x, y, hw, rise, -1f, t1, z);
                Vector3 r0 = ArchPoint(x, y, hw, rise, 1f, t0, z);
                Vector3 r1 = ArchPoint(x, y, hw, rise, 1f, t1, z);
                b.QuadFacing(l0, l1, r1, r0, Vector3.back, swatch);
            }

            b.TriangleFacing(ArchPoint(x, y, hw, rise, -1f, 1f, z), apex, ArchPoint(x, y, hw, rise, 1f, 1f, z), Vector3.back, swatch);
        }

        private static Vector3 ArchPoint(float x, float y, float hw, float rise, float side, float t, float z)
        {
            // Two circle-ish arcs meeting at the apex: x shrinks with a sine so the spring is vertical.
            float angle = t * Mathf.PI * 0.5f;
            return new Vector3(x + side * hw * Mathf.Cos(angle) * (1f - 0.08f * t), y + rise * Mathf.Sin(angle) * 0.92f, z);
        }

        /// <summary>
        /// A door: frame (jambs, pointed-arch head or lintel), carved walnut leaves with rosettes, a turquoise tile line,
        /// a stone threshold. <paramref name="x"/> is the centre, <paramref name="y"/> the sill.
        /// </summary>
        public static void Door(TownMeshBuilder b, float x, float y, float width, float height, TownSwatch leaf, TownSwatch frame, TownSwatch accent, bool arched)
        {
            float hw = width * 0.5f;
            const float f = 0.11f;
            // Dark reveal behind the leaves, then the leaves.
            b.BoxMinMax(new Vector3(x - hw, y, -0.02f), new Vector3(x + hw, y + height, 0f), TownSwatch.Black);
            int leaves = width > 1.3f ? 2 : 1;
            float leafW = width / leaves;
            for (int i = 0; i < leaves; i++)
            {
                float lx0 = x - hw + i * leafW + 0.02f;
                float lx1 = lx0 + leafW - 0.04f;
                b.BoxMinMax(new Vector3(lx0, y, -0.05f), new Vector3(lx1, y + height - 0.02f, -0.02f), leaf);
                // Carved panels (two) with a rosette each.
                float cx = (lx0 + lx1) * 0.5f;
                foreach (float py in new[] { y + height * 0.28f, y + height * 0.7f })
                {
                    b.BoxMinMax(new Vector3(lx0 + 0.08f, py - 0.25f, -0.07f), new Vector3(lx1 - 0.08f, py + 0.25f, -0.05f), TownSwatch.WoodDark);
                    b.Cylinder(new Vector3(cx, py, -0.07f), 0.11f, 0.08f, 0.03f, 8, TownSwatch.WoodLight, TownSwatch.Gold);
                }
            }

            b.BoxMinMax(new Vector3(x - hw - f, y, -0.07f), new Vector3(x - hw, y + height, 0.0f), frame);
            b.BoxMinMax(new Vector3(x + hw, y, -0.07f), new Vector3(x + hw + f, y + height, 0.0f), frame);
            if (arched)
            {
                float rise = Mathf.Min(0.7f, width * 0.55f);
                ArchFill(b, x, y + height, width + f * 2f, rise + 0.12f, -0.06f, frame);
                ArchFill(b, x, y + height, width, rise, -0.065f, TownSwatch.WindowLit);
                b.BoxMinMax(new Vector3(x - hw - f, y + height - 0.05f, -0.075f), new Vector3(x + hw + f, y + height + 0.03f, -0.06f), accent);
            }
            else
            {
                b.BoxMinMax(new Vector3(x - hw - f, y + height, -0.08f), new Vector3(x + hw + f, y + height + 0.16f, 0.0f), frame);
            }

            b.BoxMinMax(new Vector3(x - hw - 0.12f, Mathf.Max(0f, y - 0.06f), -0.22f), new Vector3(x + hw + 0.12f, y + 0.02f, 0.0f), TownSwatch.Stone);
        }

        /// <summary>A window with a carved lattice (panjara) over a pane that glows at night, sill and frame.</summary>
        public static void Window(TownMeshBuilder b, float x, float y, float width, float height, TownSwatch joinery, TownSwatch frame, bool lit)
        {
            float hw = width * 0.5f;
            b.BoxMinMax(new Vector3(x - hw, y, -0.02f), new Vector3(x + hw, y + height, 0f), lit ? TownSwatch.WindowLit : TownSwatch.Black);
            // Lattice: two mullions and two transoms.
            for (int i = 1; i <= 2; i++)
            {
                float mx = x - hw + width * i / 3f;
                b.BoxMinMax(new Vector3(mx - 0.025f, y, -0.05f), new Vector3(mx + 0.025f, y + height, -0.02f), joinery);
                float my = y + height * i / 3f;
                b.BoxMinMax(new Vector3(x - hw, my - 0.025f, -0.05f), new Vector3(x + hw, my + 0.025f, -0.02f), joinery);
            }

            b.BoxMinMax(new Vector3(x - hw - 0.08f, y, -0.06f), new Vector3(x - hw, y + height, 0f), frame);
            b.BoxMinMax(new Vector3(x + hw, y, -0.06f), new Vector3(x + hw + 0.08f, y + height, 0f), frame);
            ArchFill(b, x, y + height, width + 0.16f, 0.32f, -0.04f, frame);
            b.BoxMinMax(new Vector3(x - hw - 0.12f, y - 0.07f, -0.12f), new Vector3(x + hw + 0.12f, y, 0.0f), frame);
        }

        public static void RoundWindow(TownMeshBuilder b, float x, float y, float radius, TownSwatch frame)
        {
            b.With(new Vector3(x, y, -0.01f), 0f, () =>
            {
                b.Matrix *= Matrix4x4.Rotate(Quaternion.Euler(-90f, 0f, 0f));
                b.Cylinder(Vector3.zero, radius + 0.09f, radius + 0.09f, 0.05f, 10, frame, frame);
                b.Cylinder(Vector3.up * 0.02f, radius, radius, 0.05f, 10, TownSwatch.WindowLit, TownSwatch.WindowLit);
            });
        }

        /// <summary>An arched niche with a little clay lamp burning in it (the motif of the light kept against the dark).</summary>
        public static void LampNiche(TownMeshBuilder b, float x, float y)
        {
            b.BoxMinMax(new Vector3(x - 0.2f, y, -0.015f), new Vector3(x + 0.2f, y + 0.4f, 0f), TownSwatch.Basalt);
            ArchFill(b, x, y + 0.4f, 0.4f, 0.18f, -0.016f, TownSwatch.Basalt);
            b.BoxMinMax(new Vector3(x - 0.27f, y - 0.06f, -0.1f), new Vector3(x + 0.27f, y, 0f), TownSwatch.LimeWhite);
            b.Cylinder(new Vector3(x, y, -0.07f), 0.07f, 0.05f, 0.06f, 6, TownSwatch.Copper, TownSwatch.Copper);
            b.Blob(new Vector3(x, y + 0.13f, -0.07f), new Vector3(0.07f, 0.13f, 0.07f), TownSwatch.LampGlow);
        }

        /// <summary>A suzani (embroidered hanging): a coloured panel with a border and three medallions.</summary>
        public static void Suzani(TownMeshBuilder b, Vector3 center, float width, float height, TownSwatch accent)
        {
            float hw = width * 0.5f;
            float hh = height * 0.5f;
            b.BoxMinMax(new Vector3(center.x - hw, center.y - hh, center.z - 0.02f), new Vector3(center.x + hw, center.y + hh, center.z), TownSwatch.ClothWhite);
            b.BoxMinMax(new Vector3(center.x - hw + 0.06f, center.y - hh + 0.06f, center.z - 0.03f), new Vector3(center.x + hw - 0.06f, center.y + hh - 0.06f, center.z - 0.02f), TownSwatch.ClothRed);
            for (int i = -1; i <= 1; i++)
            {
                var c = new Vector3(center.x + i * width * 0.28f, center.y, center.z - 0.04f);
                b.Box(c, new Vector3(0.2f, 0.2f, 0.01f), i == 0 ? accent : TownSwatch.ClothYellow, 0f);
            }
        }

        /// <summary>The open front of a workshop (smithy): a wide dark bay with the glowing forge set in it.</summary>
        public static void WorkshopOpening(TownMeshBuilder b, float x, float width, TownBuildingSpec spec)
        {
            float hw = width * 0.5f;
            float y = TownBuildingBuilder.FloorY;
            float h = 2.3f;
            b.BoxMinMax(new Vector3(x - hw, y, -0.02f), new Vector3(x + hw, y + h, 0f), TownSwatch.Black);
            b.BoxMinMax(new Vector3(x - hw - 0.14f, y, -0.1f), new Vector3(x - hw, y + h + 0.2f, 0f), spec.JoineryColor);
            b.BoxMinMax(new Vector3(x + hw, y, -0.1f), new Vector3(x + hw + 0.14f, y + h + 0.2f, 0f), spec.JoineryColor);
            b.BoxMinMax(new Vector3(x - hw - 0.2f, y + h, -0.12f), new Vector3(x + hw + 0.2f, y + h + 0.24f, 0f), spec.JoineryColor);

            // Forge just inside the bay: brick hearth, embers, hood.
            float fx = x - TownBuildingBuilder.KeeperSide(spec) * hw * 0.45f;
            b.BoxMinMax(new Vector3(fx - 0.6f, y, -0.05f), new Vector3(fx + 0.6f, y + 0.75f, 0.6f), TownSwatch.Brick);
            b.BoxMinMax(new Vector3(fx - 0.45f, y + 0.75f, 0f), new Vector3(fx + 0.45f, y + 0.82f, 0.45f), TownSwatch.Ember);
            b.Cylinder(new Vector3(fx, y + 1.5f, 0.25f), 0.55f, 0.2f, 0.7f, 4, TownSwatch.Brick, null, 0.5f);
            // Anvil on its stump at the bay mouth.
            float ax = x + TownBuildingBuilder.KeeperSide(spec) * hw * 0.15f;
            b.Cylinder(new Vector3(ax, 0f, 0.15f), 0.22f, 0.2f, 0.55f, 7, TownSwatch.Trunk, TownSwatch.WoodLight);
            b.Box(new Vector3(ax, 0.68f, 0.15f), new Vector3(0.45f, 0.2f, 0.2f), TownSwatch.Iron);
            b.Box(new Vector3(ax - 0.3f, 0.72f, 0.15f), new Vector3(0.2f, 0.1f, 0.12f), TownSwatch.Iron);
        }

        /// <summary>Stable front: stall half-doors with the dark top open, a big double door in the middle.</summary>
        public static void StableFront(TownMeshBuilder b, TownBuildingSpec spec)
        {
            float hw = spec.Width * 0.5f;
            float y = TownBuildingBuilder.FloorY;
            Door(b, 0f, y, 1.8f, 2.3f, spec.JoineryColor, spec.TrimColor, spec.AccentColor, arched: false);
            foreach (float x in new[] { -hw * 0.66f, hw * 0.66f })
            {
                b.BoxMinMax(new Vector3(x - 0.55f, y + 1.1f, -0.02f), new Vector3(x + 0.55f, y + 2.1f, 0f), TownSwatch.Black);
                b.BoxMinMax(new Vector3(x - 0.55f, y, -0.06f), new Vector3(x + 0.55f, y + 1.1f, -0.02f), spec.JoineryColor);
                b.BoxMinMax(new Vector3(x - 0.55f, y + 0.5f, -0.08f), new Vector3(x + 0.55f, y + 0.6f, -0.06f), TownSwatch.WoodDark);
                b.BoxMinMax(new Vector3(x - 0.65f, y + 2.1f, -0.08f), new Vector3(x + 0.65f, y + 2.25f, 0f), TownSwatch.WoodDark);
            }

            // A horse looking out of the left stall.
            float hx = -hw * 0.66f;
            b.Box(new Vector3(hx, y + 1.55f, -0.25f), new Vector3(0.26f, 0.32f, 0.55f), TownSwatch.Walnut, 0f);
            b.Box(new Vector3(hx, y + 1.42f, -0.55f), new Vector3(0.2f, 0.22f, 0.25f), TownSwatch.Walnut, 0f);
            b.Box(new Vector3(hx, y + 1.75f, -0.15f), new Vector3(0.06f, 0.3f, 0.3f), TownSwatch.Hair, 0f);
        }

        // ------------------------------------------------------------------
        // Roofs
        // ------------------------------------------------------------------

        /// <summary>
        /// Flat packed-earth roof: slab over the walls (and the aivan), parapet with lime coping all round, wooden beam ends
        /// poking out under it and two wooden spouts.
        /// </summary>
        public static void FlatRoof(TownMeshBuilder b, float x0, float x1, float z0, float z1, float top, TownSwatch roof, TownSwatch wall,
            TownSwatch coping, TownSwatch beams, bool coversAivan)
        {
            const float p = TownBuildingBuilder.ParapetHeight;
            const float t = 0.2f;
            b.BoxMinMax(new Vector3(x0 - 0.08f, top - 0.18f, z0 - 0.08f), new Vector3(x1 + 0.08f, top, z1 + 0.08f), wall, roof);
            b.BoxMinMax(new Vector3(x0 - 0.08f, top, z0 - 0.08f), new Vector3(x1 + 0.08f, top + p, z0 - 0.08f + t), wall);
            b.BoxMinMax(new Vector3(x0 - 0.08f, top, z1 + 0.08f - t), new Vector3(x1 + 0.08f, top + p, z1 + 0.08f), wall);
            b.BoxMinMax(new Vector3(x0 - 0.08f, top, z0 - 0.08f + t), new Vector3(x0 - 0.08f + t, top + p, z1 + 0.08f - t), wall);
            b.BoxMinMax(new Vector3(x1 + 0.08f - t, top, z0 - 0.08f + t), new Vector3(x1 + 0.08f, top + p, z1 + 0.08f - t), wall);
            // Lime coping.
            b.BoxMinMax(new Vector3(x0 - 0.12f, top + p, z0 - 0.12f), new Vector3(x1 + 0.12f, top + p + 0.07f, z0 - 0.08f + t + 0.04f), coping);
            b.BoxMinMax(new Vector3(x0 - 0.12f, top + p, z1 + 0.08f - t - 0.04f), new Vector3(x1 + 0.12f, top + p + 0.07f, z1 + 0.12f), coping);
            b.BoxMinMax(new Vector3(x0 - 0.12f, top + p, z0), new Vector3(x0 - 0.08f + t + 0.04f, top + p + 0.07f, z1), coping);
            b.BoxMinMax(new Vector3(x1 + 0.08f - t - 0.04f, top + p, z0), new Vector3(x1 + 0.12f, top + p + 0.07f, z1), coping);

            // Beam ends (vassa) along the front and the back.
            for (float x = x0 + 0.35f; x < x1 - 0.2f; x += 0.62f)
            {
                b.Box(new Vector3(x, top - 0.28f, z0 - 0.16f), new Vector3(0.12f, 0.12f, 0.3f), beams);
                b.Box(new Vector3(x, top - 0.28f, z1 + 0.16f), new Vector3(0.12f, 0.12f, 0.3f), beams);
            }

            // Spouts.
            foreach (float x in new[] { x0 + 0.5f, x1 - 0.5f })
            {
                b.Box(new Vector3(x, top + 0.05f, z0 - 0.35f), new Vector3(0.14f, 0.1f, 0.6f), TownSwatch.WoodDark);
            }
        }

        /// <summary>A dome on an octagonal drum, with a lime ring and a finial.</summary>
        public static void Dome(TownMeshBuilder b, Vector3 baseCenter, float radius, TownSwatch drum, TownSwatch dome, TownSwatch finial)
        {
            const int sides = 12;
            b.Cylinder(baseCenter, radius + 0.12f, radius + 0.12f, 0.55f, 8, drum, drum);
            b.Cylinder(baseCenter + Vector3.up * 0.55f, radius + 0.16f, radius + 0.16f, 0.08f, 8, TownSwatch.LimeWhite, TownSwatch.LimeWhite);
            float y = baseCenter.y + 0.63f;
            float previousR = radius;
            float previousY = 0f;
            const int rings = 5;
            for (int i = 1; i <= rings; i++)
            {
                float a = i / (float)rings * Mathf.PI * 0.5f;
                float r = Mathf.Cos(a) * radius;
                float h = Mathf.Sin(a) * radius * 1.12f;
                b.Cylinder(new Vector3(baseCenter.x, y + previousY, baseCenter.z), previousR, Mathf.Max(0.001f, r), h - previousY, sides, dome, i == rings ? (TownSwatch?)dome : null);
                previousR = r;
                previousY = h;
            }

            Vector3 tip = new Vector3(baseCenter.x, y + radius * 1.12f, baseCenter.z);
            b.Log(tip, tip + Vector3.up * 0.55f, 0.035f, finial);
            b.Blob(tip + Vector3.up * 0.3f, new Vector3(0.16f, 0.16f, 0.16f), finial);
            b.Blob(tip + Vector3.up * 0.58f, new Vector3(0.1f, 0.14f, 0.1f), finial);
        }

        /// <summary>Two tiled slopes, ridge along x (stables and sheds), with gable-end triangles in the wall colour.</summary>
        public static void GableRoof(TownMeshBuilder b, float x0, float x1, float z0, float z1, float eave, float rise, TownSwatch tiles, TownSwatch under, TownSwatch wall)
        {
            const float o = 0.35f;
            float zm = (z0 + z1) * 0.5f;
            float ridge = eave + rise;
            var front = new List<Vector3> { new Vector3(x0 - o, eave - 0.15f, z0 - o), new Vector3(x0 - o, ridge, zm), new Vector3(x1 + o, ridge, zm), new Vector3(x1 + o, eave - 0.15f, z0 - o) };
            var back = new List<Vector3> { new Vector3(x0 - o, ridge, zm), new Vector3(x0 - o, eave - 0.15f, z1 + o), new Vector3(x1 + o, eave - 0.15f, z1 + o), new Vector3(x1 + o, ridge, zm) };
            b.Plate(front, 0.1f, tiles, under);
            b.Plate(back, 0.1f, tiles, under);
            // Tile courses: thin steps across each slope.
            for (int i = 1; i < 6; i++)
            {
                float t = i / 6f;
                foreach (List<Vector3> slope in new[] { front, back })
                {
                    Vector3 a = Vector3.Lerp(slope[0], slope[1], t) + Vector3.up * 0.03f;
                    Vector3 c = Vector3.Lerp(slope[3], slope[2], t) + Vector3.up * 0.03f;
                    b.Log(a, c, 0.035f, TownSwatch.ClayTile, 4);
                }
            }

            b.Log(new Vector3(x0 - o - 0.05f, ridge + 0.04f, zm), new Vector3(x1 + o + 0.05f, ridge + 0.04f, zm), 0.1f, TownSwatch.Brick);
            foreach (float x in new[] { x0, x1 })
            {
                b.TriangleFacing(new Vector3(x, eave, z0), new Vector3(x, ridge - 0.08f, zm), new Vector3(x, eave, z1), new Vector3(x < 0f ? -1f : 1f, 0f, 0f), wall);
            }
        }

        /// <summary>A smaller room set back on the terrace, with its own parapet and a window to the street.</summary>
        public static void UpperRoom(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFacade, float zBack, float top, TownSwatch wall)
        {
            float w = spec.Width * 0.45f;
            float x0 = spec.DoorOffset >= 0f ? -hw + 0.3f : hw - 0.3f - w;
            float z0 = zFacade + (zBack - zFacade) * 0.45f;
            float z1 = zBack - 0.3f;
            float h = top + 2.3f;
            b.BoxMinMax(new Vector3(x0, top, z0), new Vector3(x0 + w, h, z1), wall);
            FlatRoof(b, x0, x0 + w, z0, z1, h, spec.RoofColor, wall, spec.TrimColor, spec.JoineryColor, false);
            b.With(new Vector3(x0 + w * 0.5f, top, z0), 0f, () => Window(b, 0f, 0.7f, 0.8f, 1f, spec.JoineryColor, spec.TrimColor, true));
        }

        public static void Chimney(TownMeshBuilder b, Vector3 roofPoint, bool smithy)
        {
            float h = smithy ? 1.9f : 1.2f;
            b.BoxMinMax(new Vector3(roofPoint.x - 0.3f, roofPoint.y - 0.2f, roofPoint.z - 0.3f), new Vector3(roofPoint.x + 0.3f, roofPoint.y + h, roofPoint.z + 0.3f), TownSwatch.Brick);
            b.BoxMinMax(new Vector3(roofPoint.x - 0.38f, roofPoint.y + h, roofPoint.z - 0.38f), new Vector3(roofPoint.x + 0.38f, roofPoint.y + h + 0.1f, roofPoint.z + 0.38f), TownSwatch.Stone);
            b.BoxMinMax(new Vector3(roofPoint.x - 0.18f, roofPoint.y + h + 0.1f, roofPoint.z - 0.18f), new Vector3(roofPoint.x + 0.18f, roofPoint.y + h + 0.11f, roofPoint.z + 0.18f), smithy ? TownSwatch.Ember : TownSwatch.Black);
        }

        /// <summary>A stork's nest of twigs with a white stork standing in it (Zarafshan roofs).</summary>
        public static void StorkNest(TownMeshBuilder b, Vector3 at)
        {
            b.Cylinder(at, 0.55f, 0.65f, 0.28f, 9, TownSwatch.WoodWeathered, TownSwatch.DirtDark);
            Vector3 body = at + new Vector3(0f, 0.65f, 0f);
            b.Blob(body, new Vector3(0.32f, 0.3f, 0.55f), TownSwatch.ClothWhite);
            b.Blob(body + new Vector3(0f, 0.02f, 0.18f), new Vector3(0.34f, 0.18f, 0.32f), TownSwatch.Black);
            b.Log(body + new Vector3(0f, 0.08f, -0.2f), body + new Vector3(0f, 0.45f, -0.3f), 0.05f, TownSwatch.ClothWhite);
            b.Blob(body + new Vector3(0f, 0.5f, -0.32f), new Vector3(0.12f, 0.12f, 0.14f), TownSwatch.ClothWhite);
            b.Log(body + new Vector3(0f, 0.5f, -0.38f), body + new Vector3(0f, 0.42f, -0.62f), 0.025f, TownSwatch.ClothRed, 4);
            b.Log(at + new Vector3(0.06f, 0.28f, 0f), body + new Vector3(0.06f, -0.12f, 0f), 0.02f, TownSwatch.ClothRed, 4);
        }

        // ------------------------------------------------------------------
        // Aivan (deep porch on carved columns)
        // ------------------------------------------------------------------

        /// <summary>Brick-paved deck of the aivan, with a stone edge and a step at the door.</summary>
        public static void AivanDeck(TownMeshBuilder b, float x0, float x1, float z0, float z1, TownBuildingSpec spec)
        {
            float h = CitySolids.DeckHeight;
            b.BoxMinMax(new Vector3(x0, 0f, z0), new Vector3(x1, h, z1), TownSwatch.Stone, TownSwatch.Brick);
            for (float x = x0 + 0.6f; x < x1 - 0.1f; x += 0.6f)
            {
                b.BoxMinMax(new Vector3(x - 0.02f, h, z0 + 0.05f), new Vector3(x + 0.02f, h + 0.005f, z1), TownSwatch.PavingDark);
            }

            float doorX = spec.DoorOffset * spec.Width;
            b.BoxMinMax(new Vector3(doorX - 0.7f, 0f, z0 - 0.32f), new Vector3(doorX + 0.7f, h * 0.5f, z0), TownSwatch.Stone);
        }

        /// <summary>The aivan columns at <see cref="TownBuildingBuilder.PorchPostsX"/> (the colliders use the same list).</summary>
        public static void AivanColumns(TownMeshBuilder b, TownBuildingSpec spec, float z, float y0, float y1)
        {
            foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
            {
                CarvedColumn(b, new Vector3(x, y0 - TownBuildingBuilder.FloorY + CitySolids.DeckHeight, z), y1 - (CitySolids.DeckHeight), spec.JoineryColor == TownSwatch.Turquoise ? TownSwatch.Walnut : spec.JoineryColor, TownSwatch.Stone);
            }
        }

        /// <summary>
        /// A Sogdian carved wooden column: stone base, slender fluted shaft with carved rings, and the fan capital (a
        /// stepped, flaring block) that carries the beam. <paramref name="height"/> from the base to the top of the capital.
        /// </summary>
        public static void CarvedColumn(TownMeshBuilder b, Vector3 foot, float height, TownSwatch wood, TownSwatch stone)
        {
            b.Cylinder(foot, 0.17f, 0.15f, 0.18f, 8, stone, stone);
            float shaftTop = foot.y + height - 0.42f;
            b.Cylinder(foot + Vector3.up * 0.18f, 0.11f, 0.085f, shaftTop - foot.y - 0.18f, 8, wood);
            foreach (float t in new[] { 0.3f, 0.62f })
            {
                b.Cylinder(new Vector3(foot.x, Mathf.Lerp(foot.y + 0.18f, shaftTop, t), foot.z), 0.12f, 0.12f, 0.08f, 8, TownSwatch.WoodDark, TownSwatch.WoodDark);
            }

            // Fan capital: a flaring frustum and two stepped brackets.
            var capitalBase = new Vector3(foot.x, shaftTop, foot.z);
            b.Cylinder(capitalBase, 0.09f, 0.2f, 0.22f, 4, wood, wood, 0.5f);
            b.Box(capitalBase + Vector3.up * 0.28f, new Vector3(0.62f, 0.1f, 0.26f), TownSwatch.WoodDark);
            b.Box(capitalBase + Vector3.up * 0.37f, new Vector3(0.9f, 0.1f, 0.3f), wood);
        }

        public static void AivanBeam(TownMeshBuilder b, float x0, float x1, float z, float y, TownSwatch wood)
        {
            b.BoxMinMax(new Vector3(x0 - 0.05f, y - 0.02f, z - 0.18f), new Vector3(x1 + 0.05f, y + 0.24f, z + 0.18f), wood == TownSwatch.Turquoise ? TownSwatch.Walnut : wood);
        }

        /// <summary>Joisted wooden ceiling of the aivan (seen from under the roof edge at the isometric angle).</summary>
        public static void AivanCeiling(TownMeshBuilder b, float x0, float x1, float z0, float z1, float y)
        {
            b.BoxMinMax(new Vector3(x0, y - 0.2f, z0), new Vector3(x1, y - 0.12f, z1), TownSwatch.WoodLight);
            for (float x = x0 + 0.4f; x < x1; x += 0.45f)
            {
                b.BoxMinMax(new Vector3(x - 0.05f, y - 0.32f, z0 + 0.1f), new Vector3(x + 0.05f, y - 0.2f, z1), TownSwatch.Beam);
            }
        }

        // ------------------------------------------------------------------
        // Portals and towers
        // ------------------------------------------------------------------

        /// <summary>The pishtaq: a tall rectangular portal standing proud of the facade, framed in tile, with a deep arch.</summary>
        public static void Pishtaq(TownMeshBuilder b, float x, float width, float height, TownSwatch wall, TownSwatch tile, TownSwatch accent)
        {
            float hw = width * 0.5f;
            float y0 = TownBuildingBuilder.FloorY;
            b.BoxMinMax(new Vector3(x - hw, y0, -0.35f), new Vector3(x + hw, height, 0f), wall);
            // Tile frame.
            b.BoxMinMax(new Vector3(x - hw - 0.04f, height - 0.35f, -0.39f), new Vector3(x + hw + 0.04f, height, -0.33f), tile);
            b.BoxMinMax(new Vector3(x - hw - 0.04f, y0 + 0.4f, -0.39f), new Vector3(x - hw + 0.22f, height - 0.35f, -0.33f), tile);
            b.BoxMinMax(new Vector3(x + hw - 0.22f, y0 + 0.4f, -0.39f), new Vector3(x + hw + 0.04f, height - 0.35f, -0.33f), tile);
            // The recessed arch (dark blue vault) above the door.
            float archW = width - 0.75f;
            b.BoxMinMax(new Vector3(x - archW * 0.5f, y0, -0.36f), new Vector3(x + archW * 0.5f, y0 + 2.6f, -0.35f), TownSwatch.Indigo);
            ArchFill(b, x, y0 + 2.6f, archW, Mathf.Min(1.1f, height - y0 - 3f), -0.37f, TownSwatch.Indigo);
            b.BoxMinMax(new Vector3(x - archW * 0.5f - 0.08f, y0 + 2.58f, -0.4f), new Vector3(x + archW * 0.5f + 0.08f, y0 + 2.66f, -0.36f), accent);
            b.BoxMinMax(new Vector3(x - hw - 0.08f, height, -0.42f), new Vector3(x + hw + 0.08f, height + 0.1f, 0.05f), TownSwatch.LimeWhite);
        }

        /// <summary>A bronze lantern hanging from a short iron bracket.</summary>
        public static void Lantern(TownMeshBuilder b, Vector3 at, float scale)
        {
            float s = scale;
            b.Log(at + new Vector3(0f, 0.45f * s, 0.25f), at + new Vector3(0f, 0.45f * s, 0f), 0.02f, TownSwatch.Iron, 4);
            b.Log(at + new Vector3(0f, 0.45f * s, 0f), at + new Vector3(0f, 0.3f * s, 0f), 0.015f, TownSwatch.Iron, 4);
            b.Box(at + Vector3.up * 0.15f * s, new Vector3(0.22f, 0.3f, 0.22f) * s, TownSwatch.LampGlow, 0f);
            b.Cylinder(at + Vector3.up * 0.29f * s, 0.17f * s, 0.02f, 0.14f * s, 6, TownSwatch.Copper);
            b.Box(at, new Vector3(0.26f, 0.04f, 0.26f) * s, TownSwatch.Copper, 0f);
        }

        /// <summary>The lamp tower: a slender square shaft with tile bands and an open lantern room at the top.</summary>
        public static void LampTower(TownMeshBuilder b, Vector3 foot, float size, float height, TownSwatch wall, TownSwatch tile, TownSwatch dome)
        {
            float h = size * 0.5f;
            Plinth(b, foot.x - h, foot.x + h, foot.z - h, foot.z + h, TownSwatch.Stone);
            float shaftTop = height - 1.6f;
            b.BoxMinMax(new Vector3(foot.x - h * 0.85f, TownBuildingBuilder.FloorY, foot.z - h * 0.85f), new Vector3(foot.x + h * 0.85f, shaftTop, foot.z + h * 0.85f), wall);
            foreach (float y in new[] { 2.8f, 5.0f })
            {
                b.BoxMinMax(new Vector3(foot.x - h * 0.88f, y, foot.z - h * 0.88f), new Vector3(foot.x + h * 0.88f, y + 0.22f, foot.z + h * 0.88f), tile);
            }

            b.BoxMinMax(new Vector3(foot.x - h, shaftTop, foot.z - h), new Vector3(foot.x + h, shaftTop + 0.15f, foot.z + h), TownSwatch.LimeWhite);
            // Lantern room: four corner piers around the light.
            foreach (Vector2 c in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
            {
                var p = new Vector3(foot.x + c.x * h * 0.75f, shaftTop + 0.15f, foot.z + c.y * h * 0.75f);
                b.Box(p + Vector3.up * 0.55f, new Vector3(0.22f, 1.1f, 0.22f), wall, 0f);
            }

            b.Box(new Vector3(foot.x, shaftTop + 0.7f, foot.z), new Vector3(h * 1.1f, 0.9f, h * 1.1f), TownSwatch.LampGlow, 0f);
            b.BoxMinMax(new Vector3(foot.x - h, shaftTop + 1.25f, foot.z - h), new Vector3(foot.x + h, shaftTop + 1.4f, foot.z + h), TownSwatch.LimeWhite);
            Dome(b, new Vector3(foot.x, shaftTop + 1.4f - 0.55f, foot.z), h * 0.7f, wall, dome, TownSwatch.Gold);
        }

        public static void SignBoard(TownMeshBuilder b, Vector3 at, TownSwatch color, TownSwatch accent)
        {
            b.Box(at, new Vector3(1.5f, 0.42f, 0.06f), TownSwatch.WoodDark, 0f);
            b.Box(at + new Vector3(0f, 0f, -0.035f), new Vector3(1.34f, 0.3f, 0.02f), color, 0f);
            b.Box(at + new Vector3(0f, 0f, -0.05f), new Vector3(0.3f, 0.16f, 0.02f), accent, 0f);
        }
    }
}
