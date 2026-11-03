using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of the Torres Brancas de Amanirenas, the high elves' city (Docs/Mapas100.md §3.3 and map 42, style
    /// <c>WhiteSpire</c>): the white spire houses (<see cref="SpireHouseHouse"/>: a low stepped base of Meroë sandstone, a
    /// lime-white hall with pale-gold bands and tall windows of light, a slender tower with a glass dome and a bronze mirror
    /// passing the sunlight down to the door, a portico of white pillars), the garden terraces with the seven spires behind
    /// the town (the tallest one with its glass dome and the leaf bridges), the Relógio de Espelhos on the plaza, the garden
    /// rills with their leaf bridges (curved stone blades), mirror posts, the small steep pyramids of the gardens (an echo of
    /// Meroë) and the pylon gate at the streets' ends; Natakamani's mirrors. Same rules as the rest of the kit: flat colours
    /// from <see cref="TownPalette"/> (row 15 holds lime white, pale gold, sky blue, silver leaf, Meroë sandstone, the
    /// sunlight, mirror bronze and garden green), low-poly, deterministic. The mirrors are astronomy and light, no rite.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Half the thickness of a <see cref="CityPropKind.SpireTerrace"/>'s front wall: its collider is the run inflated by this.</summary>
        public const float SpireTerraceHalfThickness = 0.5f;

        /// <summary>Depth of one garden step behind a <see cref="CityPropKind.SpireTerrace"/> (three steps).</summary>
        public const float SpireStepDepth = 2.1f;

        /// <summary>Radius of the <see cref="CityPropKind.MirrorClock"/>'s round step (× its scale): its collider.</summary>
        public const float MirrorClockRadius = 1.3f;

        /// <summary>Radius of a <see cref="CityPropKind.MirrorPost"/>'s foot (× its scale): its collider.</summary>
        public const float MirrorPostRadius = 0.3f;

        /// <summary>Half the width of the water of a <see cref="CityPropKind.LeafBridge"/>.</summary>
        public const float RillHalfWidth = 0.5f;

        /// <summary>Side of a <see cref="CityPropKind.SpirePyramid"/>'s plinth (× its scale): its collider.</summary>
        public const float SpirePyramidSize = 2.0f;

        // ------------------------------------------------------------------
        // The white spire house
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.SpireHouse"/> (local frame, front −z): a two-step base of Meroë sandstone, the
        /// lime-white hall (<see cref="TownBuildingSpec.WallColor"/>) with a pale-gold band and cornice and small gold pyramids
        /// on its corners, an arched door of bronze and tall arched windows of light, a slender tower set back on the side away
        /// from the door with gold bands, a strip of light and a glass dome (<see cref="Dome"/>), a bronze mirror on the tower
        /// and the beam it passes down to the deck; a sandstone deck in front with white pillars (at
        /// <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) under a white architrave with the
        /// sign as a plaque.
        /// </summary>
        public static void SpireHouseHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float floorY = TownBuildingBuilder.FloorY;
            float storey = Mathf.Max(2.6f, spec.WallHeight);
            float wallTop = floorY + storey;
            TownSwatch gold = spec.TrimColor;

            // The stepped base and the hall.
            b.BoxMinMax(new Vector3(-hw - 0.2f, 0f, zFacade - 0.2f), new Vector3(hw + 0.2f, floorY * 0.5f, zBack + 0.2f), spec.PlinthColor);
            b.BoxMinMax(new Vector3(-hw - 0.08f, floorY * 0.5f, zFacade - 0.08f), new Vector3(hw + 0.08f, floorY, zBack + 0.08f), spec.PlinthColor, TownSwatch.SpireLime);
            b.BoxMinMax(new Vector3(-hw, floorY, zFacade), new Vector3(hw, wallTop, zBack), spec.WallColor);
            b.BoxMinMax(new Vector3(-hw - 0.03f, wallTop - 0.45f, zFacade - 0.03f), new Vector3(hw + 0.03f, wallTop - 0.32f, zBack + 0.03f), gold);
            b.BoxMinMax(new Vector3(-hw - 0.12f, wallTop, zFacade - 0.12f), new Vector3(hw + 0.12f, wallTop + 0.2f, zBack + 0.12f), spec.WallColor, gold);
            foreach (Vector2 c in new[] { new Vector2(-1, -1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(1, 1) })
            {
                float cz = c.y < 0f ? zFacade + 0.1f : zBack - 0.1f;
                b.Cylinder(new Vector3(c.x * (hw - 0.1f), wallTop + 0.2f, cz), 0.24f, 0f, 0.6f, 4, gold, null, 0.5f);
            }

            // The arched bronze door with its gold frame.
            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.4f : 1.05f;
            float doorTop = floorY + Mathf.Min(2.0f, storey - 0.75f);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.1f, floorY, zFacade - 0.05f), new Vector3(doorX + doorW * 0.5f + 0.1f, doorTop, zFacade), gold);
            ArchFill(b, doorX, doorTop, doorW + 0.2f, 0.5f, zFacade - 0.05f, gold);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, floorY, zFacade - 0.08f), new Vector3(doorX + doorW * 0.5f, doorTop, zFacade - 0.05f), spec.JoineryColor);
            ArchFill(b, doorX, doorTop, doorW, 0.42f, zFacade - 0.08f, spec.JoineryColor);

            // Tall arched windows of light (the sunlight the mirrors pass in glows through them at night).
            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = i == 0 ? (doorX > 0.01f ? -1f : 1f) : (doorX > 0.01f ? 1f : -1f);
                float x = side * hw * 0.62f;
                if (Mathf.Abs(x - doorX) < doorW * 0.5f + 0.6f)
                {
                    continue;
                }

                float y0 = floorY + 0.8f;
                float y1 = floorY + 2.0f;
                b.BoxMinMax(new Vector3(x - 0.2f, y0, zFacade - 0.03f), new Vector3(x + 0.2f, y1, zFacade - 0.01f), TownSwatch.SpireLight);
                ArchFill(b, x, y1, 0.4f, 0.25f, zFacade - 0.02f, TownSwatch.SpireLight);
                b.BoxMinMax(new Vector3(x - 0.3f, y0 - 0.08f, zFacade - 0.08f), new Vector3(x + 0.3f, y0, zFacade), gold);
            }

            // The tower, set back on the side away from the door, its glass dome and the mirror.
            float towerSide = doorX > 0.01f ? -1f : 1f;
            float tw = Mathf.Clamp(hw * 0.32f, 0.6f, 0.85f);
            float tx = towerSide * Mathf.Max(0f, hw - tw - 0.45f);
            float tz = Mathf.Lerp(zFacade, zBack, 0.6f);
            float towerTop = wallTop + 2.6f + 0.2f * spec.Floors;
            b.BoxMinMax(new Vector3(tx - tw, wallTop + 0.2f, tz - tw), new Vector3(tx + tw, towerTop, tz + tw), spec.WallColor);
            foreach (float y in new[] { wallTop + 1.1f, towerTop - 0.25f })
            {
                b.BoxMinMax(new Vector3(tx - tw - 0.03f, y, tz - tw - 0.03f), new Vector3(tx + tw + 0.03f, y + 0.12f, tz + tw + 0.03f), gold);
            }

            b.BoxMinMax(new Vector3(tx - 0.13f, wallTop + 1.35f, tz - tw - 0.02f), new Vector3(tx + 0.13f, towerTop - 0.45f, tz - tw), TownSwatch.SpireLight);
            Dome(b, new Vector3(tx, towerTop, tz), tw * 0.8f, gold, TownSwatch.Glass, TownSwatch.Gold);

            var mirror = new Vector3(tx - towerSide * (tw + 0.05f), towerTop - 0.6f, tz - tw - 0.05f);
            SunMirror(b, mirror, 0.55f);
            b.Log(mirror + new Vector3(0f, -0.1f, -0.1f), new Vector3(doorX, CitySolids.DeckHeight + 0.02f, Mathf.Lerp(zFront, zFacade, 0.45f)), 0.025f, TownSwatch.SpireLight, 4);

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), spec.PlinthColor, TownSwatch.SpireLime);

                float z = zFront + TownBuildingBuilder.PostInset;
                float porchTop = floorY + Mathf.Min(2.6f, storey - 0.1f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, deck + 0.08f, z), new Vector3(0.38f, 0.16f, 0.38f), TownSwatch.MeroeSandstone, 0f);
                    b.Cylinder(new Vector3(x, deck + 0.16f, z), 0.15f, 0.12f, porchTop - deck - 0.3f, 8, TownSwatch.SpireLime);
                    b.Box(new Vector3(x, porchTop - 0.07f, z), new Vector3(0.36f, 0.14f, 0.36f), gold, 0f);
                }

                b.BoxMinMax(new Vector3(-hw - 0.06f, porchTop, z - 0.2f), new Vector3(hw + 0.06f, porchTop + 0.26f, zFacade), spec.WallColor, gold);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, porchTop + 0.13f, z - 0.23f);
                    b.Box(at, new Vector3(1.3f, 0.24f, 0.05f), gold, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(1.1f, 0.16f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        /// <summary>A round bronze mirror on a short arm, tilted down and forward to pass the light on (seen from the front).</summary>
        private static void SunMirror(TownMeshBuilder b, Vector3 at, float size)
        {
            float h = size * 0.5f;
            b.Log(at + new Vector3(0f, 0f, 0.15f), at, 0.03f, TownSwatch.MirrorBronze, 4);
            b.Slab(new Vector3(-h, -h * 0.8f, -h * 0.35f) + at, new Vector3(-h, h * 0.8f, h * 0.35f) + at, new Vector3(h, h * 0.8f, h * 0.35f) + at, new Vector3(h, -h * 0.8f, -h * 0.35f) + at,
                0.05f, TownSwatch.Glass, TownSwatch.MirrorBronze);
            b.BoxMinMax(at + new Vector3(-h - 0.04f, -h * 0.85f, -h * 0.38f), at + new Vector3(h + 0.04f, -h * 0.75f, -h * 0.28f), TownSwatch.MirrorBronze);
        }

        /// <summary>
        /// The pylon gate of the Torres Brancas at a street's end: two battered lime-white pylons with gold cornices (the
        /// pylons of Meroë), a white lintel with the gold sun disc and a lamp of the passed-down light hanging under it.
        /// </summary>
        public static void PylonGate(TownMeshBuilder b, Vector3 a, Vector3 c)
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
                    b.Cylinder(new Vector3(x, 0f, 0f), 0.85f, 0.55f, 3.6f, 4, TownSwatch.SpireLime, TownSwatch.PaleGold, 0.5f);
                    b.Box(new Vector3(x, 3.68f, 0f), new Vector3(0.95f, 0.16f, 0.95f), TownSwatch.PaleGold, 0f);
                    b.BoxMinMax(new Vector3(x - 0.15f, 1.2f, -0.52f), new Vector3(x + 0.15f, 2.6f, -0.5f), TownSwatch.SpireLight);
                }

                b.BoxMinMax(new Vector3(0f, 3.2f, -0.3f), new Vector3(w, 3.65f, 0.3f), TownSwatch.SpireLime, TownSwatch.PaleGold);
                b.BoxMinMax(new Vector3(0.3f, 3.2f, -0.33f), new Vector3(w - 0.3f, 3.3f, -0.3f), TownSwatch.SkyBlue);
                float mid = w * 0.5f;
                b.Log(new Vector3(mid, 4.15f, -0.08f), new Vector3(mid, 4.15f, 0.08f), 0.45f, TownSwatch.PaleGold, 10);
                b.Log(new Vector3(mid, 4.15f, -0.1f), new Vector3(mid, 4.15f, -0.08f), 0.28f, TownSwatch.SpireLight, 10);
                b.Log(new Vector3(mid, 3.2f, 0f), new Vector3(mid, 2.85f, 0f), 0.015f, TownSwatch.MirrorBronze, 3);
                b.Box(new Vector3(mid, 2.7f, 0f), new Vector3(0.24f, 0.3f, 0.24f), TownSwatch.SpireLight, 45f);
            });
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The garden terraces behind the line from the origin to <paramref name="d"/> (the mass on the left, local +z): a
        /// lime-white wall with a gold coping (its collider is the run inflated by <see cref="SpireTerraceHalfThickness"/>),
        /// then three garden steps of <see cref="SpireStepDepth"/> with hedges of garden green and silver leaf and, on the
        /// upper two, slender white spires with gold bands, a strip of light and a glass dome or a gold point. Variant 1 raises
        /// the tallest spire in the middle of the top step (a sandstone pyramid base, three gold bands, the glass dome, the
        /// great mirror and its beam down to the wall) with two lower spires beside it, joined by leaf bridges.
        /// </summary>
        private static void SpireTerrace(TownMeshBuilder b, Vector2 d, float s, int variant)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            string key = "spires" + Mathf.RoundToInt(length * 10f) + "_" + variant;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                float wallH = 1.5f * s;
                b.BoxMinMax(new Vector3(-0.3f, 0f, -SpireTerraceHalfThickness), new Vector3(length + 0.3f, wallH, SpireTerraceHalfThickness), TownSwatch.SpireLime, TownSwatch.PaleGold);
                b.BoxMinMax(new Vector3(-0.32f, 0f, -SpireTerraceHalfThickness - 0.04f), new Vector3(length + 0.32f, 0.3f, SpireTerraceHalfThickness), TownSwatch.MeroeSandstone);

                const int steps = 3;
                float mid = length * 0.5f;
                for (int i = 0; i < steps; i++)
                {
                    float z0 = SpireTerraceHalfThickness + i * SpireStepDepth;
                    float z1 = SpireTerraceHalfThickness + steps * SpireStepDepth;
                    float h = wallH + i * 1.2f * s;
                    float x0 = -0.2f - i * 0.3f;
                    float x1 = length + 0.2f + i * 0.3f;
                    if (i > 0)
                    {
                        b.BoxMinMax(new Vector3(x0, 0f, z0), new Vector3(x1, h, z1), TownSwatch.SpireLime, TownSwatch.GardenGreen);
                        b.BoxMinMax(new Vector3(x0, h - 0.1f, z0 - 0.04f), new Vector3(x1, h + 0.02f, z0 + 0.2f), TownSwatch.PaleGold);
                    }
                    else
                    {
                        b.Flat(x0, z0, x1, z1, h, TownSwatch.GardenGreen);
                    }

                    float stripZ = z0 + SpireStepDepth * 0.5f;
                    int count = Mathf.Max(1, Mathf.RoundToInt(length / 4.5f));
                    for (int k = 0; k < count; k++)
                    {
                        float t = (k + 0.3f + 0.4f * TownStyle.Hash01(key, i * 13 + k)) / count;
                        float x = Mathf.Lerp(0.6f, length - 0.6f, t);
                        bool spire = i > 0 && TownStyle.Hash01(key, i * 31 + k + 5) < 0.55f;
                        if (variant == 1 && i == steps - 1 && Mathf.Abs(x - mid) < 5.6f)
                        {
                            continue;
                        }

                        if (spire)
                        {
                            float sh = (4.2f + 2.0f * TownStyle.Hash01(key, k + 50)) * s;
                            Spire(b, new Vector3(x, h, stripZ), 0.5f, sh, (k + i) % 2 == 0);
                        }
                        else
                        {
                            b.Crown(new Vector3(x, h + 0.35f * s, stripZ), new Vector3(1.4f, 0.7f, 1.0f) * s, (k + i) % 3 == 0 ? TownSwatch.SilverLeaf : TownSwatch.GardenGreen);
                        }
                    }
                }

                if (variant == 1)
                {
                    float topY = wallH + (steps - 1) * 1.2f * s;
                    float fz = SpireTerraceHalfThickness + (steps - 1) * SpireStepDepth + SpireStepDepth * 0.5f;
                    b.Cylinder(new Vector3(mid, topY, fz), 2.0f, 0.9f, 1.4f * s, 4, TownSwatch.MeroeSandstone, TownSwatch.PaleGold, 0.5f);
                    float baseY = topY + 1.4f * s;
                    float greatTop = baseY + 7.5f * s;
                    Spire(b, new Vector3(mid, baseY, fz), 0.7f, greatTop - baseY, true);
                    b.BoxMinMax(new Vector3(mid - 0.73f, baseY + 2.4f * s, fz - 0.73f), new Vector3(mid + 0.73f, baseY + 2.55f * s, fz + 0.73f), TownSwatch.PaleGold);
                    var great = new Vector3(mid, greatTop - 1.6f * s, fz - 0.85f);
                    SunMirror(b, great, 0.8f);
                    b.Log(great + new Vector3(0f, -0.2f, -0.2f), new Vector3(mid, wallH + 0.05f, 0f), 0.04f, TownSwatch.SpireLight, 4);

                    // Two lower spires beside it, and the leaf bridges.
                    foreach (float side in new[] { -1f, 1f })
                    {
                        float sx = mid + side * 4.4f;
                        float sh = 5.5f * s;
                        Spire(b, new Vector3(sx, topY, fz), 0.55f, sh, false);
                        float by = topY + sh * 0.7f;
                        var from = new Vector3(mid + side * 0.7f, baseY + 3.2f * s, fz);
                        var to = new Vector3(sx - side * 0.55f, by, fz);
                        var peak = (from + to) * 0.5f + Vector3.up * 0.5f;
                        LeafSpan(b, from, peak, 0.32f);
                        LeafSpan(b, peak, to, 0.32f);
                    }
                }
            });
        }

        /// <summary>A slender white spire: square shaft from <paramref name="foot"/> with a gold band, a strip of light and a glass dome (or a gold point).</summary>
        private static void Spire(TownMeshBuilder b, Vector3 foot, float half, float height, bool dome)
        {
            float topY = foot.y + height;
            b.BoxMinMax(new Vector3(foot.x - half, foot.y, foot.z - half), new Vector3(foot.x + half, topY, foot.z + half), TownSwatch.SpireLime);
            b.BoxMinMax(new Vector3(foot.x - half - 0.03f, topY - 0.3f, foot.z - half - 0.03f), new Vector3(foot.x + half + 0.03f, topY - 0.18f, foot.z + half + 0.03f), TownSwatch.PaleGold);
            b.BoxMinMax(new Vector3(foot.x - 0.1f, foot.y + height * 0.3f, foot.z - half - 0.02f), new Vector3(foot.x + 0.1f, topY - 0.5f, foot.z - half), TownSwatch.SpireLight);
            if (dome)
            {
                Dome(b, new Vector3(foot.x, topY, foot.z), half * 0.75f, TownSwatch.PaleGold, TownSwatch.Glass, TownSwatch.Gold);
            }
            else
            {
                b.Cylinder(new Vector3(foot.x, topY, foot.z), half * 1.3f, 0f, half * 2.4f, 4, TownSwatch.PaleGold, null, 0.5f);
            }
        }

        /// <summary>One straight run of a leaf bridge (a thin curved stone blade, silver leaf on top) from <paramref name="a"/> to <paramref name="c"/>.</summary>
        private static void LeafSpan(TownMeshBuilder b, Vector3 a, Vector3 c, float halfWidth)
        {
            Vector3 dir = c - a;
            var flat = new Vector3(dir.x, 0f, dir.z);
            if (flat.sqrMagnitude < 1e-4f)
            {
                return;
            }

            Vector3 across = Vector3.Cross(Vector3.up, flat.normalized) * halfWidth;
            Vector3 p0 = a - across, p1 = c - across, p2 = c + across, p3 = a + across;
            if (Vector3.Cross(p1 - p0, p3 - p0).y > 0f)
            {
                b.Slab(p0, p1, p2, p3, 0.08f, TownSwatch.SilverLeaf, TownSwatch.SpireLime);
            }
            else
            {
                b.Slab(p3, p2, p1, p0, 0.08f, TownSwatch.SilverLeaf, TownSwatch.SpireLime);
            }
        }

        /// <summary>
        /// The Relógio de Espelhos on the plaza: a round lime step with the hour lines of sandstone, a tall white obelisk on a
        /// gold-capped base with the gold sun disc, and six bronze mirrors on posts round it that turn the light with the hour
        /// (the puzzle of the ficha; here they stand still).
        /// </summary>
        private static void MirrorClock(TownMeshBuilder b, float s)
        {
            float r = MirrorClockRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.96f, 0.28f, 12, TownSwatch.SpireLime, TownSwatch.SpireLime);
            b.Ring(new Vector3(0f, 0.29f, 0f), r * 0.82f, r * 0.96f, 12, TownSwatch.MeroeSandstone, TownSwatch.SpireLime);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 30f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Log(dir * 0.45f + Vector3.up * 0.29f, dir * (r * 0.8f) + Vector3.up * 0.29f, 0.03f, TownSwatch.MeroeSandstone, 4);
            }

            b.Box(new Vector3(0f, 0.48f, 0f), new Vector3(0.75f, 0.4f, 0.75f), TownSwatch.MeroeSandstone, 0f, TownSwatch.PaleGold);
            b.Cylinder(new Vector3(0f, 0.68f, 0f), 0.36f, 0.2f, 3.0f * s, 4, TownSwatch.SpireLime, null, 0.5f);
            b.Cylinder(new Vector3(0f, 0.68f + 3.0f * s, 0f), 0.2f, 0f, 0.4f, 4, TownSwatch.PaleGold, null, 0.5f);
            float discY = 0.68f + 2.5f * s;
            b.Log(new Vector3(0f, discY, -0.24f), new Vector3(0f, discY, -0.16f), 0.34f, TownSwatch.PaleGold, 10);
            b.Log(new Vector3(0f, discY, -0.26f), new Vector3(0f, discY, -0.24f), 0.2f, TownSwatch.SpireLight, 10);

            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f) * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Cos(a) * r * 0.9f, 0.28f, Mathf.Sin(a) * r * 0.9f);
                b.Log(p, p + Vector3.up * 0.75f, 0.03f, TownSwatch.MirrorBronze, 4);
                b.Box(p + Vector3.up * 0.85f, new Vector3(0.24f, 0.22f, 0.03f), TownSwatch.Glass, -i * 60f + 90f);
            }
        }

        /// <summary>
        /// A garden rill from the origin to <paramref name="d"/> (water <see cref="RillHalfWidth"/> each side between lime
        /// kerbs) crossed at its middle by a leaf bridge: a pointed blade of stone, silver leaf on top with a gold midrib.
        /// Walkable: the blade stays under the walker's step.
        /// </summary>
        private static void LeafBridge(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = RillHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.04f, TownSwatch.Water);
                foreach (float z in new[] { -w - 0.12f, w })
                {
                    b.BoxMinMax(new Vector3(0f, 0f, z), new Vector3(length, 0.1f, z + 0.12f), TownSwatch.SpireLime, TownSwatch.PaleGold);
                }

                float xm = length * 0.5f;
                float half = Mathf.Min(1.35f, length * 0.5f - 0.1f);
                float zEnd = w + 0.55f;
                const float y = 0.15f;
                var leaf = new List<Vector3>
                {
                    new Vector3(xm, y, -zEnd), new Vector3(xm - half, y, -0.35f), new Vector3(xm - half, y, 0.35f),
                    new Vector3(xm, y, zEnd), new Vector3(xm + half, y, 0.35f), new Vector3(xm + half, y, -0.35f),
                };
                b.Plate(leaf, 0.1f, TownSwatch.SilverLeaf, TownSwatch.MeroeSandstone);
                b.BoxMinMax(new Vector3(xm - 0.04f, y, -zEnd + 0.15f), new Vector3(xm + 0.04f, y + 0.02f, zEnd - 0.15f), TownSwatch.PaleGold);
            });
        }

        /// <summary>A bronze mirror on a slim white post on a sandstone foot, tilted to pass the light (variant 1: its beam down to a glowing stone at its foot).</summary>
        private static void MirrorPost(TownMeshBuilder b, float s, int variant)
        {
            float r = MirrorPostRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.85f, 0.22f, 6, TownSwatch.MeroeSandstone, TownSwatch.SpireLime);
            b.Cylinder(new Vector3(0f, 0.22f, 0f), 0.08f, 0.07f, 2.0f * s, 6, TownSwatch.SpireLime);
            b.Box(new Vector3(0f, 0.22f + 2.0f * s, 0f), new Vector3(0.2f, 0.1f, 0.2f), TownSwatch.PaleGold, 0f);
            var mirror = new Vector3(0f, 2.55f * s, -0.12f);
            SunMirror(b, mirror, 0.5f);
            if ((variant % 2) == 1)
            {
                var stone = new Vector3(0.05f, 0.12f, -r - 0.1f);
                b.Blob(stone, new Vector3(0.26f, 0.22f, 0.26f), TownSwatch.SpireLight);
                b.Log(mirror + new Vector3(0f, -0.15f, -0.1f), stone, 0.02f, TownSwatch.SpireLight, 4);
            }
        }

        /// <summary>A small steep pyramid of Meroë sandstone on a plinth with its little pylon front and door, and a gold cap.</summary>
        private static void SpirePyramid(TownMeshBuilder b, float s)
        {
            float h = SpirePyramidSize * 0.5f * s;
            b.BoxMinMax(new Vector3(-h, 0f, -h + 0.25f), new Vector3(h, 0.25f, h), TownSwatch.MeroeSandstone, TownSwatch.SpireLime);
            float half = (h - 0.12f) * 1.414f;
            b.Cylinder(new Vector3(0f, 0.25f, 0.12f), half, 0.08f, 2.5f * s, 4, TownSwatch.MeroeSandstone, TownSwatch.PaleGold, 0.5f);
            b.Cylinder(new Vector3(0f, 0.25f + 2.5f * s, 0.12f), 0.16f, 0f, 0.32f, 4, TownSwatch.PaleGold, null, 0.5f);
            b.BoxMinMax(new Vector3(-0.42f, 0f, -h), new Vector3(0.42f, 1.15f * s, -h + 0.42f), TownSwatch.SpireLime, TownSwatch.PaleGold);
            b.BoxMinMax(new Vector3(-0.16f, 0.1f, -h - 0.01f), new Vector3(0.16f, 0.75f * s, -h), TownSwatch.Black);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Natakamani's front: two bronze mirrors on stands catching the light, a crate of polished stones and a little brazier of light.</summary>
        private static void WhiteSpireDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            foreach (float lx in new[] { -0.55f, 0.45f })
            {
                b.Log(new Vector3(x + lx, y, z - 0.5f), new Vector3(x + lx, y + 1.1f, z - 0.5f), 0.035f, TownSwatch.MirrorBronze, 4);
                SunMirror(b, new Vector3(x + lx, y + 1.3f, z - 0.55f), 0.45f);
            }

            b.BoxMinMax(new Vector3(x - 0.35f, y, z - 1.15f), new Vector3(x + 0.25f, y + 0.3f, z - 0.75f), TownSwatch.MirrorBronze, TownSwatch.WoodDark);
            TownSwatch[] stones = { TownSwatch.SkyBlue, TownSwatch.PaleGold, TownSwatch.SpireLight };
            for (int i = 0; i < stones.Length; i++)
            {
                b.Blob(new Vector3(x - 0.22f + i * 0.17f, y + 0.36f, z - 0.95f), new Vector3(0.14f, 0.12f, 0.14f), stones[i]);
            }

            b.Cylinder(new Vector3(x + 0.75f, y, z - 0.95f), 0.14f, 0.22f, 0.55f, 6, TownSwatch.MirrorBronze, TownSwatch.MirrorBronze);
            b.Blob(new Vector3(x + 0.75f, y + 0.68f, z - 0.95f), new Vector3(0.26f, 0.3f, 0.26f), TownSwatch.SpireLight);
        }
    }
}
