using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Dur-Jarras, the ziggurat city of the Margens da Mistura (Docs/Mapas100.md map 77, style <c>Ziggurat</c>): the
    /// brick houses (<see cref="ZigguratHallHouse"/>), the ziggurat of five steps, the Relógio de Zurvan (only a clock, never a
    /// figure, bible §1.1), the canals of black water with brick bridges, the date palms, the glazed lions of the walls, the stacks
    /// of jars, the gate of two stepped brick towers and Atrahasis' bitumen yard. Chogha Zanbil and the glazed walls of Mesopotamia
    /// are form only: no temple rite, no figure of a god. Same rules as the rest of the kit: flat colours from
    /// <see cref="TownPalette"/> (row 24), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Full footprint of a <see cref="CityPropKind.Ziggurat"/> (× its scale): its collider; the stair stays inside it.</summary>
        public static readonly Vector2 ZigguratFootprint = new Vector2(9.2f, 9.2f);

        /// <summary>Radius of a <see cref="CityPropKind.ZurvanClock"/>'s plinth (× its scale): its collider.</summary>
        public const float ZurvanClockRadius = 0.95f;

        /// <summary>Radius of a <see cref="CityPropKind.DatePalm"/>'s trunk base (× its scale): its collider.</summary>
        public const float DatePalmRadius = 0.5f;

        /// <summary>Radius of a <see cref="CityPropKind.GlazedLion"/>'s pier (× its scale): its collider.</summary>
        public const float GlazedLionRadius = 0.8f;

        /// <summary>Radius of a <see cref="CityPropKind.JarStack"/> (× its scale): its collider.</summary>
        public const float JarStackRadius = 0.8f;

        /// <summary>Half the width of the black water of a <see cref="CityPropKind.BitumenCanal"/>.</summary>
        public const float BitumenCanalHalfWidth = 0.85f;

        // ------------------------------------------------------------------
        // The house
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.ZigguratHall"/> (local frame, front −z): walls of baked brick
        /// (<see cref="TownBuildingSpec.WallColor"/>) with buttress niches and a glazed band (<see cref="TownBuildingSpec.TrimColor"/>),
        /// a door with a gold lintel, windows of lit linen, a flat roof of tarred reed (<see cref="TownBuildingSpec.RoofColor"/>)
        /// behind a stepped parapet; in front the porch: reed matting on brick posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>,
        /// the colliders use the same list), a flame and the plaque.
        /// </summary>
        public static void ZigguratHallHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float eave = Mathf.Max(top, 2.6f);
            float wx = hw - 0.15f;
            float zw = zFacade + 0.1f, zb = zBack - 0.1f;

            // The bitumen footing and the brick walls.
            b.BoxMinMax(new Vector3(-hw, 0f, zw - 0.08f), new Vector3(hw, 0.3f, zBack), spec.PlinthColor, spec.PlinthColor);
            b.BoxMinMax(new Vector3(-wx, 0.3f, zw), new Vector3(wx, eave, zb), spec.WallColor, spec.WallColor);

            // The buttress niches of the front, the glazed band and its row of gold.
            int bays = Mathf.Clamp(Mathf.RoundToInt(wx * 2f / 1.3f), 3, 7);
            for (int i = 0; i <= bays; i++)
            {
                float x = Mathf.Lerp(-wx, wx, (float)i / bays);
                b.Box(new Vector3(x, (0.3f + eave) * 0.5f, zw - 0.06f), new Vector3(0.28f, eave - 0.3f, 0.12f), spec.WallColor, 0f);
            }

            b.Box(new Vector3(0f, eave - 0.32f, zw - 0.14f), new Vector3(wx * 2f + 0.1f, 0.26f, 0.08f), spec.TrimColor, 0f);
            for (int i = 0; i < bays; i++)
            {
                float x = Mathf.Lerp(-wx, wx, (i + 0.5f) / bays);
                b.Box(new Vector3(x, eave - 0.32f, zw - 0.19f), new Vector3(0.14f, 0.14f, 0.02f), spec.AccentColor, 45f);
            }

            // The door with its gold lintel and the windows of lit linen.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -wx + 0.8f, wx - 0.8f);
            float door = spec.WideDoor ? 0.75f : 0.55f;
            const float doorTop = 2.2f;
            b.BoxMinMax(new Vector3(doorX - door - 0.1f, 0.3f, zw - 0.16f), new Vector3(doorX + door + 0.1f, doorTop + 0.1f, zw - 0.02f), spec.JoineryColor);
            b.Box(new Vector3(doorX, (0.3f + doorTop) * 0.5f, zw - 0.18f), new Vector3(door * 2f - 0.1f, doorTop - 0.35f, 0.03f), TownSwatch.Bitumen, 0f);
            b.Box(new Vector3(doorX, doorTop + 0.18f, zw - 0.19f), new Vector3(door * 2f + 0.5f, 0.2f, 0.08f), spec.AccentColor, 0f);

            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? -1f : 1f;
                float x = Mathf.Clamp(doorX + side * (door + 1.05f), -wx + 0.45f, wx - 0.45f);
                if (Mathf.Abs(x - doorX) < door + 0.45f)
                {
                    continue;
                }

                float y = Mathf.Min(eave - 1.0f, 1.8f);
                b.Box(new Vector3(x, y, zw - 0.15f), new Vector3(0.56f, 0.5f, 0.05f), spec.JoineryColor, 0f);
                b.Box(new Vector3(x, y, zw - 0.18f), new Vector3(0.42f, 0.36f, 0.03f), TownSwatch.WindowLit, 0f);
            }

            // The flat roof of tarred reed behind a stepped parapet.
            float roofY = eave;
            b.BoxMinMax(new Vector3(-hw, roofY, zFacade), new Vector3(hw, roofY + 0.14f, zBack), spec.RoofColor, spec.RoofColor);
            int merlons = Mathf.Clamp(Mathf.RoundToInt(hw * 2f / 0.9f), 5, 11);
            for (int i = 0; i < merlons; i++)
            {
                float x = Mathf.Lerp(-hw + 0.25f, hw - 0.25f, i / (merlons - 1f));
                bool tall = (i % 2) == 0;
                b.Box(new Vector3(x, roofY + (tall ? 0.42f : 0.28f), zFacade + 0.1f), new Vector3(0.5f, tall ? 0.56f : 0.28f, 0.22f), spec.WallColor, 0f, spec.TrimColor);
            }

            foreach (float side in new[] { -1f, 1f })
            {
                b.Box(new Vector3(side * (hw - 0.12f), roofY + 0.24f, (zFacade + zBack) * 0.5f), new Vector3(0.24f, 0.34f, zBack - zFacade), spec.WallColor, 0f, spec.TrimColor);
            }

            b.Box(new Vector3(0f, roofY + 0.24f, zBack - 0.12f), new Vector3(hw * 2f, 0.34f, 0.24f), spec.WallColor, 0f, spec.TrimColor);

            if (spec.PorchDepth > 0.01f)
            {
                // The porch: a brick deck, posts of brick, a mat of tarred reed, a flame and the plaque on the beam.
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.Bitumen, TownSwatch.MarshReed);
                float z = zFront + TownBuildingBuilder.PostInset;
                float beamY = Mathf.Min(eave, 2.75f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, deck + (beamY - deck) * 0.5f, z), new Vector3(0.26f, beamY - deck, 0.26f), spec.WallColor, 0f);
                    b.Box(new Vector3(x, beamY - 0.3f, z), new Vector3(0.3f, 0.12f, 0.3f), spec.TrimColor, 0f);
                }

                b.Box(new Vector3(0f, beamY + 0.05f, z), new Vector3(hw * 2f, 0.14f, 0.2f), spec.JoineryColor, 0f);
                b.Slab(new Vector3(-hw, beamY + 0.08f, zFront - 0.1f), new Vector3(-hw, beamY + 0.25f, zFacade),
                    new Vector3(hw, beamY + 0.25f, zFacade), new Vector3(hw, beamY + 0.08f, zFront - 0.1f), 0.06f, TownSwatch.MarshReed, TownSwatch.Bitumen);

                float flameX = Mathf.Clamp(doorX + (doorX >= 0f ? -1f : 1f) * (door + 0.9f), -hw + 0.4f, hw - 0.4f);
                FlameBowl(b, new Vector3(flameX, deck, (zFront + zFacade) * 0.5f), 0.8f);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, beamY - 0.2f, z - 0.14f);
                    b.Box(at, new Vector3(1.1f, 0.3f, 0.05f), spec.AccentColor, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.94f, 0.2f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>A standing fire bowl of <paramref name="s"/> on <paramref name="foot"/>: a brick drum, a bronze bowl and its flame.</summary>
        private static void FlameBowl(TownMeshBuilder b, Vector3 foot, float s)
        {
            b.Cylinder(foot, 0.18f * s, 0.14f * s, 0.7f * s, 6, TownSwatch.BakedBrick, TownSwatch.Bitumen);
            b.Cylinder(foot + Vector3.up * 0.7f * s, 0.1f * s, 0.24f * s, 0.14f * s, 6, TownSwatch.ElamGold, TownSwatch.ElamGold);
            b.Blob(foot + Vector3.up * 0.95f * s, new Vector3(0.18f, 0.26f, 0.18f) * s, TownSwatch.ZigguratFlame);
        }

        // ------------------------------------------------------------------
        // The gate
        // ------------------------------------------------------------------

        /// <summary>
        /// The gate of Dur-Jarras at a street's end: two stepped towers of baked brick with glazed bands, a lintel of glazed blue
        /// brick with a row of gold rosettes and a stepped parapet, a flame bowl hanging in the middle (the gateways of the glazed
        /// walls as form, without a figure).
        /// </summary>
        public static void ZigguratGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                const float towerTop = 3.7f;
                foreach (float x in new[] { 0f, w })
                {
                    b.Box(new Vector3(x, 1.3f, 0f), new Vector3(1.1f, 2.6f, 1.1f), TownSwatch.BakedBrick, 0f);
                    b.Box(new Vector3(x, 2.2f, 0f), new Vector3(1.16f, 0.3f, 1.16f), TownSwatch.GlazedBlue, 0f);
                    b.Box(new Vector3(x, 3.15f, 0f), new Vector3(0.85f, 1.1f, 0.85f), TownSwatch.BakedBrick, 0f, TownSwatch.Bitumen);
                    b.Box(new Vector3(x, towerTop + 0.08f, 0f), new Vector3(0.6f, 0.16f, 0.6f), TownSwatch.ElamGold, 0f);
                }

                b.Box(new Vector3(w * 0.5f, towerTop - 0.3f, 0f), new Vector3(w, 0.7f, 0.6f), TownSwatch.GlazedBlue, 0f, TownSwatch.Bitumen);
                for (int i = 0; i < 7; i++)
                {
                    float x = Mathf.Lerp(0.35f, w - 0.35f, i / 6f);
                    b.Box(new Vector3(x, towerTop - 0.3f, -0.32f), new Vector3(0.16f, 0.16f, 0.02f), TownSwatch.ElamGold, 45f);
                    b.Box(new Vector3(x, towerTop + 0.18f - 0.1f, 0f), new Vector3(0.28f, (i % 2 == 0) ? 0.3f : 0.18f, 0.5f), TownSwatch.BakedBrick, 0f);
                }

                float mid = w * 0.5f;
                b.Log(new Vector3(mid, towerTop - 0.65f, 0f), new Vector3(mid, towerTop - 1.05f, 0f), 0.02f, TownSwatch.Bitumen, 3);
                b.Cylinder(new Vector3(mid, towerTop - 1.25f, 0f), 0.12f, 0.24f, 0.2f, 6, TownSwatch.ElamGold, TownSwatch.ElamGold);
                b.Blob(new Vector3(mid, towerTop - 1.0f, 0f), new Vector3(0.16f, 0.22f, 0.16f), TownSwatch.ZigguratFlame);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The ziggurat of five steps (front −z): tiers of baked brick with a glazed blue band at each rim and buttress strips, a
        /// straight stair up the front (inside the footprint), fire bowls on the tier corners (the ziggurat lights from top to
        /// bottom at night) and a small glazed room on top, roofed in gold. A stepped mountain of brick: no idol, no altar.
        /// </summary>
        private static void Ziggurat(TownMeshBuilder b, float s)
        {
            float[] halves = { 4.6f, 3.75f, 2.9f, 2.1f, 1.3f };
            const float tier = 1.1f;
            for (int i = 0; i < halves.Length; i++)
            {
                float h = halves[i] * s;
                float y0 = i * tier * s;
                b.Box(new Vector3(0f, y0 + tier * 0.5f * s, 0f), new Vector3(h * 2f, tier * s, h * 2f), TownSwatch.BakedBrick, 0f, TownSwatch.Bitumen);
                b.Box(new Vector3(0f, y0 + (tier - 0.1f) * s, 0f), new Vector3(h * 2f + 0.08f * s, 0.14f * s, h * 2f + 0.08f * s), TownSwatch.GlazedBlue, 0f);

                // The buttress strips of the front and sides of the tier.
                int strips = Mathf.Max(3, Mathf.RoundToInt(halves[i] * 1.2f));
                for (int k = 0; k <= strips; k++)
                {
                    float t = Mathf.Lerp(-h + 0.2f * s, h - 0.2f * s, (float)k / strips);
                    if (Mathf.Abs(t) < 1.1f * s)
                    {
                        continue;
                    }

                    b.Box(new Vector3(t, y0 + 0.45f * s, -h - 0.04f * s), new Vector3(0.24f, 0.9f, 0.08f) * s, TownSwatch.BakedBrick, 0f);
                    b.Box(new Vector3(-h - 0.04f * s, y0 + 0.45f * s, t), new Vector3(0.08f, 0.9f, 0.24f) * s, TownSwatch.BakedBrick, 0f);
                    b.Box(new Vector3(h + 0.04f * s, y0 + 0.45f * s, t), new Vector3(0.08f, 0.9f, 0.24f) * s, TownSwatch.BakedBrick, 0f);
                }

                if (i == 0)
                {
                    continue;
                }

                foreach (float cx in new[] { -1f, 1f })
                {
                    foreach (float cz in new[] { -1f, 1f })
                    {
                        FlameBowl(b, new Vector3(cx * (h - 0.25f * s), y0, cz * (h - 0.25f * s)), 0.5f * s);
                    }
                }
            }

            // The straight stair up the front, from the base to the top tier, inside the footprint.
            const int steps = 14;
            float zStart = -4.5f * s;
            float zEnd = -1.3f * s;
            float rise = halves.Length * tier * s / steps;
            for (int i = 0; i < steps; i++)
            {
                float z = Mathf.Lerp(zStart, zEnd, (float)i / (steps - 1));
                float y = (i + 1) * rise;
                b.Box(new Vector3(0f, y * 0.5f, z), new Vector3(1.8f * s, y, 0.26f * s), TownSwatch.BakedBrick, 0f, TownSwatch.MarshReed);
            }

            foreach (float side in new[] { -1f, 1f })
            {
                b.Log(new Vector3(side * 1.0f * s, 0.25f * s, zStart), new Vector3(side * 1.0f * s, halves.Length * tier * s + 0.1f * s, zEnd), 0.09f * s, TownSwatch.Bitumen, 4);
            }

            // The glazed room on top, roofed in gold, the flame that lights first at dusk.
            float top = halves.Length * tier * s;
            b.Box(new Vector3(0f, top + 0.55f * s, 0.2f * s), new Vector3(1.5f, 1.1f, 1.3f) * s, TownSwatch.GlazedBlue, 0f, TownSwatch.ElamGold);
            b.Box(new Vector3(0f, top + 0.5f * s, -0.47f * s), new Vector3(0.6f, 0.9f, 0.04f) * s, TownSwatch.Bitumen, 0f);
            b.Box(new Vector3(0f, top + 1.2f * s, 0.2f * s), new Vector3(1.7f, 0.16f, 1.5f) * s, TownSwatch.ElamGold, 0f);
            b.Blob(new Vector3(0f, top + 1.55f * s, 0.2f * s), new Vector3(0.28f, 0.42f, 0.28f) * s, TownSwatch.ZigguratFlame);
        }

        /// <summary>
        /// The Relógio de Zurvan on the plaza: a round plinth of baked brick with a glazed rim, twelve gold marks of the hours, a
        /// disc of pale dust and a slanted bitumen gnomon with a gold tip. Only a clock: Zurvan is never a person (bible §1.1).
        /// </summary>
        private static void ZurvanClock(TownMeshBuilder b, float s)
        {
            float r = ZurvanClockRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.92f, 0.3f * s, 12, TownSwatch.BakedBrick, TownSwatch.BakedBrick);
            b.Cylinder(new Vector3(0f, 0.3f * s, 0f), r * 0.86f, r * 0.82f, 0.12f * s, 12, TownSwatch.GlazedBlue, TownSwatch.DustSky);
            for (int i = 0; i < 12; i++)
            {
                float a = i * 30f * Mathf.Deg2Rad;
                bool quarter = (i % 3) == 0;
                b.Box(new Vector3(Mathf.Sin(a) * r * 0.7f, 0.44f * s, Mathf.Cos(a) * r * 0.7f), new Vector3(quarter ? 0.14f : 0.08f, 0.03f, 0.2f) * s, TownSwatch.ElamGold, i * 30f);
            }

            var foot = new Vector3(0f, 0.42f * s, -0.15f * s);
            var tip = new Vector3(0f, 2.2f * s, 0.45f * s);
            b.Log(foot, tip, 0.1f * s, TownSwatch.Bitumen, 4);
            b.Cylinder(tip - Vector3.up * 0.05f * s, 0.12f * s, 0.03f * s, 0.25f * s, 5, TownSwatch.ElamGold, TownSwatch.ElamGold);
        }

        /// <summary>
        /// A canal from the origin to <paramref name="d"/>: black water <see cref="BitumenCanalHalfWidth"/> each side between baked
        /// brick lips sealed with bitumen and, in the middle, a low bridge of brick with a glazed curb and a fire bowl on each side.
        /// Walkable: the bridge stays under the walker's step.
        /// </summary>
        private static void BitumenCanal(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = BitumenCanalHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.02f, TownSwatch.Bitumen);
                foreach (float z in new[] { -w - 0.12f, w + 0.12f })
                {
                    b.Box(new Vector3(length * 0.5f, 0.06f, z), new Vector3(length, 0.12f, 0.24f), TownSwatch.BakedBrick, 0f, TownSwatch.Bitumen);
                }

                int glints = Mathf.Max(2, Mathf.RoundToInt(length / 1.5f));
                for (int i = 0; i < glints; i++)
                {
                    float x = length * (i + 0.5f) / glints;
                    if (Mathf.Abs(x - length * 0.5f) >= 1.3f)
                    {
                        b.Box(new Vector3(x, 0.03f, (i % 2 == 0 ? -0.4f : 0.35f) * w), new Vector3(0.14f, 0.01f, 0.08f), TownSwatch.MarshReed, 30f);
                    }
                }

                float xm = length * 0.5f;
                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(xm + i * 0.4f, 0.14f, 0f), new Vector3(0.38f, 0.1f, w * 2f + 0.55f), TownSwatch.BakedBrick, 0f, i == 0 ? TownSwatch.GlazedBlue : TownSwatch.BakedBrick);
                }

                foreach (float z in new[] { -w - 0.3f, w + 0.3f })
                {
                    FlameBowl(b, new Vector3(xm, 0.1f, z), 0.7f);
                }
            });
        }

        /// <summary>A date palm on a ring of brick: a ringed trunk, a crown of drooping fronds and clusters of dates (Variant 1: taller, with a lean).</summary>
        private static void DatePalm(TownMeshBuilder b, float s, int variant)
        {
            bool tall = (variant % 2) == 1;
            float h = (tall ? 4.2f : 3.2f) * s;
            float lean = (tall ? 0.45f : 0.15f) * s;
            b.Cylinder(Vector3.zero, DatePalmRadius * s, DatePalmRadius * 0.9f * s, 0.2f * s, 7, TownSwatch.BakedBrick, TownSwatch.Bitumen);
            var top = new Vector3(lean, h, 0f);
            b.Log(new Vector3(0f, 0.15f * s, 0f), top, 0.15f * s, TownSwatch.Bitumen, 5);
            for (int i = 1; i < 5; i++)
            {
                Vector3 p = Vector3.Lerp(Vector3.zero, top, i / 5f);
                b.Cylinder(p, 0.17f * s, 0.17f * s, 0.06f * s, 5, TownSwatch.MarshReed);
            }

            for (int i = 0; i < 7; i++)
            {
                float a = i * 360f / 7f * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                Vector3 mid = top + dir * 0.75f * s + Vector3.up * 0.22f * s;
                Vector3 tip = top + dir * 1.5f * s + Vector3.down * 0.35f * s;
                b.Log(top, mid, 0.08f * s, TownSwatch.PalmFrond, 3);
                b.Log(mid, tip, 0.05f * s, TownSwatch.PalmFrond, 3);
            }

            foreach (float sx in new[] { -0.25f, 0.25f })
            {
                b.Blob(top + new Vector3(sx, -0.25f, sx * 0.6f) * s, new Vector3(0.22f, 0.3f, 0.22f) * s, TownSwatch.ElamGold);
            }
        }

        /// <summary>A pier of glazed blue brick with a striding lion in gold relief on its front (the lions of the glazed walls; Variant 1: taller, with a stepped cap).</summary>
        private static void GlazedLion(TownMeshBuilder b, float s, int variant)
        {
            bool tall = (variant % 2) == 1;
            float w = GlazedLionRadius * 1.5f * s;
            float h = (tall ? 2.4f : 1.9f) * s;
            b.Box(new Vector3(0f, 0.12f * s, 0f), new Vector3(w + 0.24f * s, 0.24f * s, 0.9f * s), TownSwatch.Bitumen, 0f);
            b.Box(new Vector3(0f, 0.24f * s + h * 0.5f, 0f), new Vector3(w, h, 0.5f * s), TownSwatch.GlazedBlue, 0f, TownSwatch.BakedBrick);
            float y = 0.24f * s + h * 0.42f;
            float z = -0.28f * s;
            b.Box(new Vector3(0f, y, z), new Vector3(0.7f, 0.26f, 0.05f) * s, TownSwatch.ElamGold, 0f);
            b.Box(new Vector3(0.42f * s, y + 0.2f * s, z), new Vector3(0.26f, 0.26f, 0.05f) * s, TownSwatch.ElamGold, 0f);
            b.Box(new Vector3(0.58f * s, y + 0.14f * s, z - 0.01f * s), new Vector3(0.14f, 0.1f, 0.03f) * s, TownSwatch.Bitumen, 0f);
            foreach (float x in new[] { -0.3f, -0.1f, 0.15f, 0.3f })
            {
                b.Box(new Vector3(x * s, y - 0.25f * s, z), new Vector3(0.09f, 0.3f, 0.05f) * s, TownSwatch.ElamGold, 0f);
            }

            b.Log(new Vector3(-0.35f * s, y + 0.05f * s, z), new Vector3(-0.55f * s, y + 0.35f * s, z), 0.03f * s, TownSwatch.ElamGold, 3);
            if (tall)
            {
                for (int i = -1; i <= 1; i++)
                {
                    b.Box(new Vector3(i * 0.35f * s, 0.24f * s + h + (i == 0 ? 0.18f : 0.1f) * s, 0f), new Vector3(0.3f, i == 0 ? 0.36f : 0.2f, 0.4f) * s, TownSwatch.BakedBrick, 0f, TownSwatch.ElamGold);
                }
            }
        }

        /// <summary>A stack of the jars that name the city, on a mat of reed (Variant 1: a taller stack with two rows and a sealed jar on top).</summary>
        private static void JarStack(TownMeshBuilder b, float s, int variant)
        {
            bool tall = (variant % 2) == 1;
            float r = JarStackRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.95f, 0.05f * s, 8, TownSwatch.MarshReed, TownSwatch.MarshReed);
            int jars = tall ? 5 : 3;
            for (int i = 0; i < jars; i++)
            {
                float a = i * 360f / jars * Mathf.Deg2Rad;
                Jar(b, new Vector3(Mathf.Cos(a) * r * 0.55f, 0.05f * s, Mathf.Sin(a) * r * 0.55f), s, i % 2 == 0 ? TownSwatch.BakedBrick : TownSwatch.DustSky);
            }

            if (tall)
            {
                Jar(b, new Vector3(0f, 0.7f * s, 0f), 0.9f * s, TownSwatch.GlazedBlue);
                b.Cylinder(new Vector3(0f, 1.32f * s, 0f), 0.12f * s, 0.1f * s, 0.08f * s, 5, TownSwatch.Bitumen, TownSwatch.Bitumen);
            }
        }

        private static void Jar(TownMeshBuilder b, Vector3 foot, float s, TownSwatch swatch)
        {
            b.Cylinder(foot, 0.14f * s, 0.26f * s, 0.32f * s, 6, swatch, swatch);
            b.Cylinder(foot + Vector3.up * 0.32f * s, 0.26f * s, 0.12f * s, 0.26f * s, 6, swatch, swatch);
            b.Cylinder(foot + Vector3.up * 0.58f * s, 0.12f * s, 0.16f * s, 0.08f * s, 6, TownSwatch.Bitumen, TownSwatch.Bitumen);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Atrahasis' front: a tarred reed raft on trestles, jars of bitumen, a coil of reed rope, a ladle in a pot and a fire bowl.</summary>
        private static void ZigguratDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Box(new Vector3(x, y + 0.02f, z - 0.45f), new Vector3(2.4f, 0.04f, 0.9f), TownSwatch.MarshReed, 0f);

            foreach (float dx in new[] { -1.0f, -0.3f })
            {
                b.Box(new Vector3(x + dx, y + 0.22f, z - 0.5f), new Vector3(0.08f, 0.4f, 0.7f), TownSwatch.WoodDark, 0f);
            }

            for (int i = 0; i < 4; i++)
            {
                float dz = -0.8f + i * 0.2f;
                b.Log(new Vector3(x - 1.2f, y + 0.46f, z + dz), new Vector3(x - 0.1f, y + 0.46f, z + dz), 0.09f, TownSwatch.Bitumen, 5, TownSwatch.MarshReed);
            }

            foreach (float dx in new[] { 0.35f, 0.7f })
            {
                Jar(b, new Vector3(x + dx, y + 0.04f, z - 0.35f), 0.9f, TownSwatch.Bitumen);
            }

            var pot = new Vector3(x + 1.15f, y + 0.04f, z - 0.55f);
            b.Cylinder(pot, 0.22f, 0.26f, 0.26f, 6, TownSwatch.Iron, TownSwatch.Bitumen);
            b.Log(pot + new Vector3(0f, 0.2f, 0f), pot + new Vector3(0.25f, 0.65f, 0.08f), 0.02f, TownSwatch.WoodDark, 3);

            b.Cylinder(new Vector3(x - 0.15f, y + 0.05f, z - 0.2f), 0.2f, 0.2f, 0.1f, 8, TownSwatch.MarshReed, TownSwatch.Bitumen);
            FlameBowl(b, new Vector3(x + 1.5f, y, zFacade - 0.25f), 0.8f);
        }
    }
}
