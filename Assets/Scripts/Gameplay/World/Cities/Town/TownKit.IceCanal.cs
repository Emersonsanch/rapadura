using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Porto da Lua Branca, the capital of the Mar Gelado (Docs/Mapas100.md map 59, style <c>IceCanal</c>): the
    /// long plank houses with a moon painted on the gable (<see cref="MoonLonghouseHouse"/>: a base of packed snow, walls of
    /// dark vertical planks, a low gabled roof thick with snow, a lilac ring round a silver full moon on the gable, crescents
    /// beside the door, a plank porch with a moon lantern), the snow domes (<see cref="SnowDomeHouse"/>: courses of snow blocks
    /// and a low entrance tunnel), the Cais da Lua of the plaza, the canals of black water crossed by stone bridges of whale-rib
    /// arches, small snow domes, crescent boats on runners, fish racks, ridges of sea ice and the gate of two whale ribs;
    /// Pamour's rack of furs and boats. The plank houses of the North Pacific coast and the snow houses of the Arctic are form
    /// only: no crest poles (they belong to living clans), no figure of the faiths of the Arctic. Same rules as the rest of the
    /// kit: flat colours from <see cref="TownPalette"/> (row 21 holds sea ice, snowfield, plank, the lilac of the moons,
    /// silver, the polar night, the black water and the moon lanterns), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of the <see cref="CityPropKind.MoonQuay"/>'s ice platform (× its scale): its collider.</summary>
        public const float MoonQuayRadius = 1.3f;

        /// <summary>Radius of a <see cref="CityPropKind.SnowDomeHut"/> (× its scale), tunnel included: its collider.</summary>
        public const float SnowDomeHutRadius = 1.0f;

        /// <summary>Radius of a <see cref="CityPropKind.CrescentBoat"/> on its runners (× its scale): its collider.</summary>
        public const float CrescentBoatRadius = 0.9f;

        /// <summary>Radius of a <see cref="CityPropKind.FishRack"/> (× its scale): its collider.</summary>
        public const float FishRackRadius = 0.8f;

        /// <summary>Radius of a <see cref="CityPropKind.IceBlock"/> ridge (× its scale): its collider.</summary>
        public const float IceBlockRadius = 0.9f;

        /// <summary>Half the width of the black water of a <see cref="CityPropKind.IceCanal"/>.</summary>
        public const float IceCanalHalfWidth = 0.8f;

        /// <summary>Height of the packed-snow base a <see cref="TownBuildingSpec.MoonLonghouse"/> stands on.</summary>
        private const float SnowBaseTop = 0.3f;

        // ------------------------------------------------------------------
        // The long plank house with the moon on its gable
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.MoonLonghouse"/> (local frame, front −z): the packed-snow base, walls of dark vertical
        /// planks (<see cref="TownBuildingSpec.WallColor"/>) with battens, a low gabled roof (ridge front to back) thick with snow
        /// (<see cref="TownBuildingSpec.RoofColor"/>), the front gable painted with a lilac ring (<see cref="TownBuildingSpec.TrimColor"/>)
        /// round a silver full moon (<see cref="TownBuildingSpec.AccentColor"/>), two crescents beside the plank door, small
        /// windows of lit ice; in front a plank porch on two posts (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the
        /// colliders use the same list) with a snowy lean-to, a moon lantern hanging and the sign from the beam.
        /// </summary>
        public static void MoonLonghouseHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float eave = Mathf.Max(top, SnowBaseTop + 2.3f);
            float ridge = eave + Mathf.Min(1.6f, hw * 0.45f);
            float wx = hw - 0.15f;
            float zw = zFacade + 0.1f, zb = zBack - 0.1f;

            // The packed-snow base and drifts at the corners.
            b.BoxMinMax(new Vector3(-hw, 0f, zFacade), new Vector3(hw, SnowBaseTop, zBack), TownSwatch.Snowfield, TownSwatch.Snowfield);
            foreach (float sx in new[] { -1f, 1f })
            {
                b.Blob(new Vector3(sx * (hw - 0.2f), SnowBaseTop, zFacade + 0.15f), new Vector3(0.7f, 0.4f, 0.5f), TownSwatch.Snowfield);
                b.Blob(new Vector3(sx * (hw - 0.3f), SnowBaseTop, zBack - 0.2f), new Vector3(0.8f, 0.45f, 0.6f), TownSwatch.SeaIce);
            }

            // The plank walls, battens on the front, and the two gables.
            b.BoxMinMax(new Vector3(-wx, SnowBaseTop, zw), new Vector3(wx, eave, zb), spec.WallColor, spec.WallColor);
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -wx + 0.8f, wx - 0.8f);
            float doorW = spec.WideDoor ? 1.3f : 1.0f;
            int battens = Mathf.Clamp(Mathf.RoundToInt(wx * 2f / 0.75f), 3, 9);
            for (int i = 0; i < battens; i++)
            {
                float x = Mathf.Lerp(-wx + 0.15f, wx - 0.15f, i / (battens - 1f));
                if (Mathf.Abs(x - doorX) < doorW * 0.5f + 0.15f)
                {
                    continue;
                }

                b.Box(new Vector3(x, (SnowBaseTop + eave) * 0.5f, zw - 0.03f), new Vector3(0.08f, eave - SnowBaseTop, 0.05f), TownSwatch.WoodDark, 0f);
            }

            b.TriangleFacing(new Vector3(-wx, eave, zw), new Vector3(wx, eave, zw), new Vector3(0f, ridge - 0.08f, zw), Vector3.back, spec.WallColor);
            b.TriangleFacing(new Vector3(-wx, eave, zb), new Vector3(wx, eave, zb), new Vector3(0f, ridge - 0.08f, zb), Vector3.forward, spec.WallColor);

            // The moon on the gable: a lilac ring round the silver full moon.
            float moonY = eave + (ridge - eave) * 0.38f;
            float moonR = Mathf.Min(0.42f, (ridge - eave) * 0.3f + 0.15f);
            FacingDisc(b, new Vector3(0f, moonY, zw - 0.02f), moonR + 0.13f, 10, spec.TrimColor);
            FacingDisc(b, new Vector3(0f, moonY, zw - 0.04f), moonR, 10, spec.AccentColor);

            // The plank door with a silver frame, two crescents beside it and small windows of lit ice.
            float doorTop = SnowBaseTop + 1.95f;
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.1f, SnowBaseTop, zw - 0.06f), new Vector3(doorX + doorW * 0.5f + 0.1f, doorTop + 0.1f, zw - 0.01f), spec.AccentColor);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, SnowBaseTop, zw - 0.09f), new Vector3(doorX + doorW * 0.5f, doorTop, zw - 0.06f), TownSwatch.DoorPlank);
            foreach (float side in new[] { -1f, 1f })
            {
                float cx = doorX + side * (doorW * 0.5f + 0.5f);
                if (Mathf.Abs(cx) > wx - 0.3f)
                {
                    continue;
                }

                var c = new Vector3(cx, doorTop - 0.3f, zw - 0.03f);
                FacingDisc(b, c, 0.26f, 8, spec.TrimColor);
                FacingDisc(b, c + new Vector3(side * 0.1f, 0.06f, -0.02f), 0.22f, 8, spec.WallColor);
            }

            int windows = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < windows; i++)
            {
                float side = doorX > 0f ? -1f : 1f;
                float x = Mathf.Clamp(i == 0 ? doorX + side * (doorW + 0.9f) : doorX - side * (doorW + 0.9f), -wx + 0.4f, wx - 0.4f);
                if (Mathf.Abs(x - doorX) < doorW * 0.5f + 0.4f)
                {
                    continue;
                }

                b.Box(new Vector3(x, eave - 0.75f, zw - 0.04f), new Vector3(0.56f, 0.46f, 0.06f), spec.TrimColor, 0f);
                b.Box(new Vector3(x, eave - 0.75f, zw - 0.07f), new Vector3(0.4f, 0.3f, 0.03f), TownSwatch.WindowLit, 0f);
            }

            // The low gabled roof, snow thick on top, plank underneath; a smoke hole on the ridge.
            float over = 0.3f;
            b.Slab(new Vector3(-hw - over, eave - 0.1f, zFacade - over), new Vector3(-hw - over, eave - 0.1f, zBack + 0.15f),
                new Vector3(0f, ridge + 0.12f, zBack + 0.15f), new Vector3(0f, ridge + 0.12f, zFacade - over), 0.22f, spec.RoofColor, spec.JoineryColor);
            b.Slab(new Vector3(0f, ridge + 0.12f, zFacade - over), new Vector3(0f, ridge + 0.12f, zBack + 0.15f),
                new Vector3(hw + over, eave - 0.1f, zBack + 0.15f), new Vector3(hw + over, eave - 0.1f, zFacade - over), 0.22f, spec.RoofColor, spec.JoineryColor);
            b.Box(new Vector3(0f, ridge + 0.2f, (zFacade + zBack) * 0.5f + 0.4f), new Vector3(0.5f, 0.3f, 0.5f), TownSwatch.MoonPlank, 0f, TownSwatch.PolarNight);

            if (spec.PorchDepth > 0.01f)
            {
                // The plank porch: deck, two posts, a beam, a snowy lean-to, the moon lantern and the sign.
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.MoonPlank, TownSwatch.WoodWeathered);
                float z = zFront + TownBuildingBuilder.PostInset;
                float beamY = eave - 0.35f;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, (deck + beamY) * 0.5f, z), new Vector3(0.18f, beamY - deck, 0.18f), spec.JoineryColor, 0f);
                }

                b.Box(new Vector3(0f, beamY, z), new Vector3(hw * 2f - 0.1f, 0.16f, 0.2f), spec.JoineryColor, 0f);
                b.Slab(new Vector3(-hw + 0.05f, beamY + 0.12f, z - 0.15f), new Vector3(-hw + 0.05f, eave - 0.05f, zw),
                    new Vector3(hw - 0.05f, eave - 0.05f, zw), new Vector3(hw - 0.05f, beamY + 0.12f, z - 0.15f), 0.12f, spec.RoofColor, TownSwatch.MoonPlank);

                float lanternX = Mathf.Clamp(-doorX * 0.6f + (doorX >= 0f ? -0.9f : 0.9f), -hw + 0.4f, hw - 0.4f);
                b.Log(new Vector3(lanternX, beamY - 0.08f, z), new Vector3(lanternX, beamY - 0.45f, z), 0.012f, TownSwatch.Iron, 3);
                b.Blob(new Vector3(lanternX, beamY - 0.62f, z), new Vector3(0.28f, 0.34f, 0.28f), TownSwatch.MoonGlow);
                b.Box(new Vector3(lanternX, beamY - 0.44f, z), new Vector3(0.24f, 0.05f, 0.24f), spec.AccentColor, 0f);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, beamY - 0.32f, z - 0.12f);
                    b.Box(at, new Vector3(1.1f, 0.32f, 0.05f), spec.JoineryColor, 0f);
                    b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.94f, 0.22f, 0.02f), spec.SignColor, 0f);
                }
            }
        }

        // ------------------------------------------------------------------
        // The snow dome
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.SnowDome"/> (local frame, front −z): a dome of snow (<see cref="TownBuildingSpec.RoofColor"/>)
        /// over the footprint in four courses with darker joints, a clear ice window, the low entrance tunnel running out to the
        /// front of the lot with its dark mouth, a block step, a lantern post by it and the sign on a short post.
        /// </summary>
        public static void SnowDomeHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float cz = (zFacade + zBack) * 0.5f;
            float hd = (zBack - zFacade) * 0.5f;
            float radius = Mathf.Max(1f, hw - 0.1f);
            float height = Mathf.Clamp(top, 2.2f, radius * 0.95f + 0.6f);
            float depthScale = Mathf.Clamp(hd / radius, 0.5f, 1.5f);

            b.With(new Vector3(0f, 0f, cz), 0f, new Vector3(1f, 1f, depthScale), () =>
            {
                SnowDomeShell(b, radius, height, spec.RoofColor);
                b.Ring(new Vector3(0f, 0.02f, 0f), radius * 0.95f, radius * 1.08f, 10, TownSwatch.SeaIce);
            });

            // The clear ice window high on the front.
            float frontZ = cz - hd * 0.62f;
            FacingDisc(b, new Vector3(radius * 0.42f, height * 0.62f, frontZ - 0.04f), 0.22f, 6, TownSwatch.WindowLit);

            // The entrance tunnel from the dome to the front of the lot (half under the ice), its dark mouth and a block step.
            float doorX = Mathf.Clamp(spec.DoorOffset * spec.Width, -radius + 0.8f, radius - 0.8f);
            float tunnelR = spec.WideDoor ? 1.0f : 0.85f;
            float mouthZ = zFront + 0.2f;
            ArchTube(b, doorX, cz - hd * 0.5f, mouthZ, tunnelR, 6, spec.RoofColor);
            FacingDisc(b, new Vector3(doorX, 0.02f, mouthZ - 0.02f), tunnelR * 0.7f, 6, TownSwatch.HollowDark, upperHalf: true);
            b.Box(new Vector3(doorX, 0.06f, mouthZ - 0.3f), new Vector3(tunnelR * 1.4f, 0.12f, 0.4f), TownSwatch.SeaIce, 0f, TownSwatch.Snowfield);

            // The lantern post and the sign beside the tunnel.
            float side = doorX > 0f ? -1f : 1f;
            float px = Mathf.Clamp(doorX + side * (tunnelR + 0.55f), -hw + 0.3f, hw - 0.3f);
            b.Log(new Vector3(px, 0f, zFront + 0.35f), new Vector3(px, 1.9f, zFront + 0.35f), 0.05f, TownSwatch.MoonPlank, 4);
            b.Blob(new Vector3(px, 2.05f, zFront + 0.35f), new Vector3(0.26f, 0.3f, 0.26f), TownSwatch.MoonGlow);
            b.Box(new Vector3(px, 2.24f, zFront + 0.35f), new Vector3(0.24f, 0.05f, 0.24f), spec.AccentColor, 0f);
            if (spec.Sign)
            {
                var at = new Vector3(px, 1.35f, zFront + 0.25f);
                b.Box(at, new Vector3(0.9f, 0.32f, 0.05f), spec.JoineryColor, 0f);
                b.Box(at + new Vector3(0f, 0f, -0.03f), new Vector3(0.76f, 0.22f, 0.02f), spec.SignColor, 0f);
            }
        }

        /// <summary>A dome of snow of <paramref name="radius"/> and <paramref name="height"/> on the origin: four courses with darker joints.</summary>
        private static void SnowDomeShell(TownMeshBuilder b, float radius, float height, TownSwatch snow)
        {
            float[] angles = { 0f, 24f, 48f, 70f, 90f };
            for (int i = 0; i < angles.Length - 1; i++)
            {
                float a0 = angles[i] * Mathf.Deg2Rad, a1 = angles[i + 1] * Mathf.Deg2Rad;
                float r0 = radius * Mathf.Cos(a0), r1 = radius * Mathf.Cos(a1);
                float y0 = height * Mathf.Sin(a0), y1 = height * Mathf.Sin(a1);
                bool last = i == angles.Length - 2;
                b.Cylinder(new Vector3(0f, y0, 0f), r0, last ? 0f : r1, y1 - y0, 10, snow, null, i * 0.5f);
                if (!last)
                {
                    b.Cylinder(new Vector3(0f, y1 - 0.03f, 0f), r1 + 0.03f, r1 + 0.02f, 0.06f, 10, TownSwatch.SeaIce, null, i * 0.5f);
                }
            }
        }

        /// <summary>
        /// A half tube on the ground along z from <paramref name="z0"/> to <paramref name="z1"/> at <paramref name="x"/> (an
        /// entrance tunnel of snow): <paramref name="sides"/> faces round the arch from one foot to the other, nothing under the ice.
        /// </summary>
        private static void ArchTube(TownMeshBuilder b, float x, float z0, float z1, float radius, int sides, TownSwatch swatch)
        {
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * Mathf.PI / sides, a1 = (i + 1) * Mathf.PI / sides;
                var p0 = new Vector3(x + Mathf.Cos(a0) * radius, Mathf.Sin(a0) * radius, 0f);
                var p1 = new Vector3(x + Mathf.Cos(a1) * radius, Mathf.Sin(a1) * radius, 0f);
                Vector3 outward = (p0 + p1) * 0.5f - new Vector3(x, 0f, 0f);
                b.QuadFacing(new Vector3(p0.x, p0.y, z0), new Vector3(p1.x, p1.y, z0), new Vector3(p1.x, p1.y, z1), new Vector3(p0.x, p0.y, z1), outward, swatch);
            }
        }

        /// <summary>
        /// A flat disc facing −z (a painted moon, a window) of <paramref name="radius"/> round <paramref name="center"/>; with
        /// <paramref name="upperHalf"/> only the half above the centre (a tunnel mouth on the ground).
        /// </summary>
        private static void FacingDisc(TownMeshBuilder b, Vector3 center, float radius, int sides, TownSwatch swatch, bool upperHalf = false)
        {
            sides = Mathf.Max(3, sides);
            float span = upperHalf ? Mathf.PI : Mathf.PI * 2f;
            for (int i = 0; i < sides; i++)
            {
                float a0 = i * span / sides, a1 = (i + 1) * span / sides;
                b.TriangleFacing(center, center + new Vector3(Mathf.Cos(a0), Mathf.Sin(a0), 0f) * radius,
                    center + new Vector3(Mathf.Cos(a1), Mathf.Sin(a1), 0f) * radius, Vector3.back, swatch);
            }
        }

        // ------------------------------------------------------------------
        // The gate
        // ------------------------------------------------------------------

        /// <summary>
        /// The gate of the Porto at a street's end: two stone ribs shaped like whale ribs rising from silver feet in the snow and
        /// curving in over the road, a crescent lantern hanging between their tips and drifts of snow at each foot.
        /// </summary>
        public static void WhaleRibGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 span = c - a;
            float w = new Vector2(span.x, span.z).magnitude;
            if (w < 0.5f)
            {
                return;
            }

            b.With(a, YawAlong(new Vector2(span.x, span.z) / w), () =>
            {
                foreach (float x0 in new[] { 0f, w })
                {
                    float dir = x0 < 0.01f ? 1f : -1f;
                    b.Cylinder(new Vector3(x0, 0f, 0f), 0.42f, 0.34f, 0.45f, 6, TownSwatch.MoonSilver, TownSwatch.Snowfield);
                    b.Blob(new Vector3(x0, 0.2f, -0.4f), new Vector3(0.9f, 0.35f, 0.6f), TownSwatch.Snowfield);

                    // The rib: five segments up and in, thinning, the tip leaning over the middle of the road.
                    const int pieces = 5;
                    Vector3 prev = new Vector3(x0, 0.4f, 0f);
                    for (int i = 1; i <= pieces; i++)
                    {
                        float t = i / (float)pieces;
                        float ang = t * 70f * Mathf.Deg2Rad;
                        var p = new Vector3(x0 + dir * (w * 0.42f) * (1f - Mathf.Cos(ang)) / (1f - Mathf.Cos(70f * Mathf.Deg2Rad)), 0.4f + 3.8f * Mathf.Sin(ang) / Mathf.Sin(70f * Mathf.Deg2Rad), 0f);
                        b.Log(prev, p, Mathf.Lerp(0.2f, 0.09f, t), i % 2 == 0 ? TownSwatch.Pearl : TownSwatch.MoonSilver, 5);
                        prev = p;
                    }
                }

                // The crescent lantern between the tips.
                float mid = w * 0.5f;
                b.Log(new Vector3(mid - w * 0.08f, 4.1f, 0f), new Vector3(mid + w * 0.08f, 4.1f, 0f), 0.025f, TownSwatch.Iron, 4);
                b.Log(new Vector3(mid, 4.1f, 0f), new Vector3(mid, 3.6f, 0f), 0.012f, TownSwatch.Iron, 3);
                b.Blob(new Vector3(mid, 3.4f, 0f), new Vector3(0.36f, 0.4f, 0.36f), TownSwatch.MoonGlow);
                CrescentArc(b, new Vector3(mid, 3.4f, -0.05f), 0.32f, TownSwatch.MoonLilac);
            });
        }

        /// <summary>A crescent of four short logs round <paramref name="center"/> in the x–y plane (open to the right).</summary>
        private static void CrescentArc(TownMeshBuilder b, Vector3 center, float radius, TownSwatch swatch)
        {
            Vector3 prev = center + new Vector3(Mathf.Cos(60f * Mathf.Deg2Rad), Mathf.Sin(60f * Mathf.Deg2Rad), 0f) * radius;
            for (int i = 1; i <= 4; i++)
            {
                float a = (60f + i * 60f) * Mathf.Deg2Rad;
                Vector3 p = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * radius;
                b.Log(prev, p, 0.045f, swatch, 4);
                prev = p;
            }
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The Cais da Lua: a round platform of sea ice with a silver rim, a crescent of silver stone blocks on it (open to the
        /// south, where the cousins come), three bollards and the tall moon lantern in the middle: a dark post, a cross-arm and
        /// the great moon globe hanging in a lilac crescent.
        /// </summary>
        private static void MoonQuay(TownMeshBuilder b, float s)
        {
            float r = MoonQuayRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.95f, 0.25f * s, 12, TownSwatch.SeaIce, TownSwatch.SeaIce);
            b.Ring(new Vector3(0f, 0.26f * s, 0f), r * 0.82f, r * 0.95f, 12, TownSwatch.MoonSilver, TownSwatch.Pearl);

            // The crescent of stone blocks, from the west round the north to the east.
            for (int i = 0; i < 7; i++)
            {
                float a = (200f - i * 36.6f) * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * 0.7f);
                float h = (0.35f + 0.25f * Mathf.Sin(i / 6f * Mathf.PI)) * s;
                b.Box(at + Vector3.up * (0.25f * s + h * 0.5f), new Vector3(0.38f, h, 0.3f) * s, TownSwatch.MoonSilver, -a * Mathf.Rad2Deg + 90f, TownSwatch.Snowfield);
            }

            foreach (float deg in new[] { 250f, 290f, 330f })
            {
                float a = deg * Mathf.Deg2Rad;
                b.Cylinder(new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (r * 0.78f) + Vector3.up * 0.25f * s, 0.09f * s, 0.07f * s, 0.35f * s, 6, TownSwatch.PolarNight, TownSwatch.Iron);
            }

            // The moon lantern.
            float postTop = 3.9f * s;
            b.Cylinder(new Vector3(0f, 0.25f * s, 0f), 0.16f * s, 0.11f * s, postTop - 0.25f * s, 6, TownSwatch.PolarNight, TownSwatch.MoonSilver);
            b.Box(new Vector3(0f, postTop - 0.15f * s, 0f), new Vector3(1.1f * s, 0.1f * s, 0.12f * s), TownSwatch.MoonPlank, 0f, TownSwatch.MoonSilver);
            b.Log(new Vector3(0.45f * s, postTop - 0.2f * s, 0f), new Vector3(0.45f * s, postTop - 0.55f * s, 0f), 0.015f * s, TownSwatch.Iron, 3);
            b.Blob(new Vector3(0.45f * s, postTop - 0.85f * s, 0f), new Vector3(0.5f, 0.55f, 0.5f) * s, TownSwatch.MoonGlow);
            CrescentArc(b, new Vector3(0.45f * s, postTop - 0.85f * s, -0.1f * s), 0.36f * s, TownSwatch.MoonLilac);
            b.Blob(new Vector3(-0.45f * s, postTop - 0.35f * s, 0f), new Vector3(0.2f, 0.24f, 0.2f) * s, TownSwatch.MoonSilver);
        }

        /// <summary>
        /// A canal of black water from the origin to <paramref name="d"/> (the water <see cref="IceCanalHalfWidth"/> each side,
        /// floes of sea ice along both lips) crossed at its middle by a stone bridge: a deck of silver slabs with snow on it and,
        /// on each side, a whale-rib arch of three curved pieces. Walkable: the slabs stay under the walker's step.
        /// </summary>
        private static void IceCanal(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = IceCanalHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.02f, TownSwatch.ColdWater);
                int floes = Mathf.Max(2, Mathf.RoundToInt(length / 1.1f));
                for (int i = 0; i < floes; i++)
                {
                    float x = length * (i + 0.5f) / floes;
                    if (Mathf.Abs(x - length * 0.5f) < 1.3f)
                    {
                        continue;
                    }

                    b.Blob(new Vector3(x, 0.06f, -w * 0.55f), new Vector3(0.8f, 0.16f, 0.5f), TownSwatch.SeaIce);
                    b.Blob(new Vector3(x + 0.35f, 0.05f, w * 0.6f), new Vector3(0.7f, 0.14f, 0.45f), TownSwatch.Snowfield);
                }

                float xm = length * 0.5f;
                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(xm + i * 0.42f, 0.1f, 0f), new Vector3(0.38f, 0.1f, w * 2f + 0.5f), TownSwatch.MoonSilver, 0f, i == 0 ? TownSwatch.Snowfield : TownSwatch.MoonSilver);
                }

                foreach (float z in new[] { -w - 0.2f, w + 0.2f })
                {
                    Vector3 prev = new Vector3(xm - 1.05f, 0.1f, z);
                    for (int i = 1; i <= 3; i++)
                    {
                        float t = i / 3f;
                        var p = new Vector3(xm - 1.05f + 2.1f * t, 0.1f + 0.85f * Mathf.Sin(t * Mathf.PI), z);
                        b.Log(prev, p, 0.07f, i == 2 ? TownSwatch.Pearl : TownSwatch.MoonSilver, 5);
                        prev = p;
                    }
                }
            });
        }

        /// <summary>A small snow dome with its entrance tunnel facing −z (Variant 1: a lantern lit at the tunnel and a fur drying on the dome).</summary>
        private static void SnowDomeHut(TownMeshBuilder b, float s, int variant)
        {
            float r = SnowDomeHutRadius * s * 0.8f;
            SnowDomeShell(b, r, 1.5f * s, TownSwatch.Snowfield);
            ArchTube(b, 0f, -r * 0.5f, -SnowDomeHutRadius * s + 0.05f, 0.48f * s, 5, TownSwatch.Snowfield);
            FacingDisc(b, new Vector3(0f, 0.02f, -SnowDomeHutRadius * s + 0.03f), 0.32f * s, 5, TownSwatch.HollowDark, upperHalf: true);
            if ((variant % 2) == 1)
            {
                b.Blob(new Vector3(0.45f * s, 0.62f * s, -SnowDomeHutRadius * s + 0.15f), new Vector3(0.2f, 0.24f, 0.2f) * s, TownSwatch.MoonGlow);
                b.Box(new Vector3(-0.2f * s, 1.3f * s, 0.1f * s), new Vector3(0.7f, 0.06f, 0.5f) * s, TownSwatch.Felt, 25f);
            }
        }

        /// <summary>
        /// A crescent boat drawn up on the ice: a plank hull with a lilac stripe whose bow and stern curve up like the horns of the
        /// moon, a thwart and a paddle, on two runners (Variant 1: a moon lantern on a pole at the bow).
        /// </summary>
        private static void CrescentBoat(TownMeshBuilder b, float s, int variant)
        {
            float half = CrescentBoatRadius * s * 0.85f;
            foreach (float z in new[] { -0.22f * s, 0.22f * s })
            {
                b.Box(new Vector3(0f, 0.06f, z), new Vector3(half * 1.8f, 0.12f, 0.08f), TownSwatch.WoodDark, 0f);
            }

            b.Box(new Vector3(0f, 0.32f * s, 0f), new Vector3(half * 1.3f, 0.38f * s, 0.62f * s), TownSwatch.MoonPlank, 0f, TownSwatch.PolarNight);
            b.Box(new Vector3(0f, 0.42f * s, -0.32f * s), new Vector3(half * 1.3f, 0.07f * s, 0.02f), TownSwatch.MoonLilac, 0f);
            b.Box(new Vector3(0f, 0.42f * s, 0.32f * s), new Vector3(half * 1.3f, 0.07f * s, 0.02f), TownSwatch.MoonLilac, 0f);
            foreach (float sx in new[] { -1f, 1f })
            {
                Vector3 prev = new Vector3(sx * half * 0.62f, 0.3f * s, 0f);
                for (int i = 1; i <= 3; i++)
                {
                    float t = i / 3f;
                    var p = new Vector3(sx * (half * 0.62f + half * 0.38f * Mathf.Sin(t * Mathf.PI * 0.5f)), (0.3f + 0.75f * t * t) * s, 0f);
                    b.Log(prev, p, Mathf.Lerp(0.2f, 0.06f, t) * s, TownSwatch.MoonPlank, 5);
                    prev = p;
                }
            }

            b.Box(new Vector3(0f, 0.52f * s, 0f), new Vector3(0.12f, 0.05f, 0.6f) * s, TownSwatch.WoodWeathered, 0f);
            b.Log(new Vector3(-half * 0.4f, 0.55f * s, -0.2f * s), new Vector3(half * 0.5f, 0.6f * s, 0.25f * s), 0.03f * s, TownSwatch.WoodLight, 4);
            if ((variant % 2) == 1)
            {
                b.Log(new Vector3(half * 0.5f, 0.5f * s, 0f), new Vector3(half * 0.5f, 1.7f * s, 0f), 0.03f * s, TownSwatch.MoonPlank, 4);
                b.Blob(new Vector3(half * 0.5f, 1.55f * s, -0.1f * s), new Vector3(0.22f, 0.26f, 0.22f) * s, TownSwatch.MoonGlow);
            }
        }

        /// <summary>A fish-drying rack: two A-frames of driftwood, a cross pole and two rows of silver fish hanging, a basket at the foot.</summary>
        private static void FishRack(TownMeshBuilder b, float s)
        {
            float half = FishRackRadius * s * 0.75f;
            float topY = 1.7f * s;
            foreach (float x in new[] { -half, half })
            {
                b.Log(new Vector3(x, 0f, -0.3f * s), new Vector3(x, topY, 0f), 0.05f * s, TownSwatch.WoodWeathered, 4);
                b.Log(new Vector3(x, 0f, 0.3f * s), new Vector3(x, topY, 0f), 0.05f * s, TownSwatch.WoodWeathered, 4);
            }

            b.Log(new Vector3(-half - 0.1f, topY, 0f), new Vector3(half + 0.1f, topY, 0f), 0.04f * s, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(-half, topY * 0.62f, 0f), new Vector3(half, topY * 0.62f, 0f), 0.03f * s, TownSwatch.WoodWeathered, 4);
            for (int i = 0; i < 5; i++)
            {
                float x = Mathf.Lerp(-half + 0.15f, half - 0.15f, i / 4f);
                b.Blob(new Vector3(x, topY - 0.32f * s, 0f), new Vector3(0.1f, 0.5f, 0.07f) * s, i % 2 == 0 ? TownSwatch.MoonSilver : TownSwatch.Pearl);
                b.Blob(new Vector3(x + 0.08f, topY * 0.62f - 0.28f * s, 0f), new Vector3(0.09f, 0.42f, 0.06f) * s, TownSwatch.MoonSilver);
            }

            b.Cylinder(new Vector3(half * 0.4f, 0f, -0.45f * s), 0.2f * s, 0.24f * s, 0.3f * s, 8, TownSwatch.Wicker, TownSwatch.MoonSilver);
        }

        /// <summary>
        /// A pressure ridge of sea ice: four blocks heaved up and tilted against each other, snow on their tops (Variant 1: taller,
        /// with a crack of moon light inside).
        /// </summary>
        private static void IceBlock(TownMeshBuilder b, float s, int variant)
        {
            float r = IceBlockRadius * s;
            bool tall = (variant % 2) == 1;
            float h0 = tall ? 1.9f : 1.2f, h1 = tall ? 1.5f : 0.9f;
            b.Box(new Vector3(-0.25f * r, h0 * 0.5f * s, 0.1f * r), new Vector3(0.9f, h0, 0.7f) * s, TownSwatch.SeaIce, 18f, TownSwatch.Snowfield);
            b.Box(new Vector3(0.35f * r, h1 * 0.5f * s, -0.15f * r), new Vector3(0.8f, h1, 0.6f) * s, TownSwatch.SeaIce, -24f, TownSwatch.Snowfield);
            b.Box(new Vector3(0.1f * r, 0.3f * s, 0.45f * r), new Vector3(0.7f, 0.6f, 0.5f) * s, TownSwatch.MoonSilver, 40f, TownSwatch.Snowfield);
            b.Box(new Vector3(-0.45f * r, 0.2f * s, -0.4f * r), new Vector3(0.5f, 0.4f, 0.4f) * s, TownSwatch.Snowfield, -10f);
            if (tall)
            {
                b.Box(new Vector3(-0.25f * r, 0.9f * s, 0.1f * r - 0.36f * s), new Vector3(0.06f, 1.1f, 0.04f) * s, TownSwatch.MoonGlow, 18f);
            }
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Pamour's front: a rack of white furs, a small crescent boat on trestles with its paddle, coils of line and a moon lantern on the deck.</summary>
        private static void IceCanalDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.Log(new Vector3(x - 1.4f, y, z - 0.3f), new Vector3(x - 1.4f, y + 1.5f, z - 0.3f), 0.04f, TownSwatch.MoonPlank, 4);
            b.Log(new Vector3(x - 0.4f, y, z - 0.3f), new Vector3(x - 0.4f, y + 1.5f, z - 0.3f), 0.04f, TownSwatch.MoonPlank, 4);
            b.Log(new Vector3(x - 1.45f, y + 1.45f, z - 0.3f), new Vector3(x - 0.35f, y + 1.45f, z - 0.3f), 0.035f, TownSwatch.MoonPlank, 4);
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(x - 1.2f + i * 0.33f, y + 1.0f, z - 0.32f), new Vector3(0.28f, 0.85f, 0.05f), i == 1 ? TownSwatch.Felt : TownSwatch.Snowfield, 0f);
            }

            foreach (float tx in new[] { x + 0.35f, x + 1.25f })
            {
                b.Box(new Vector3(tx, y + 0.22f, z - 0.45f), new Vector3(0.08f, 0.44f, 0.4f), TownSwatch.WoodDark, 0f);
            }

            b.Box(new Vector3(x + 0.8f, y + 0.55f, z - 0.45f), new Vector3(1.3f, 0.22f, 0.36f), TownSwatch.MoonPlank, 0f, TownSwatch.PolarNight);
            b.Box(new Vector3(x + 0.8f, y + 0.6f, z - 0.64f), new Vector3(1.3f, 0.05f, 0.02f), TownSwatch.MoonLilac, 0f);
            b.Log(new Vector3(x + 1.5f, y, z - 0.15f), new Vector3(x + 1.35f, y + 1.6f, z - 0.1f), 0.03f, TownSwatch.WoodLight, 4);
            b.Cylinder(new Vector3(x - 0.05f, y, z - 0.6f), 0.18f, 0.18f, 0.16f, 8, TownSwatch.NetTwine, TownSwatch.WoodDark);
            b.Blob(new Vector3(x + 0.1f, y + 0.95f, zFacade - 0.08f), new Vector3(0.22f, 0.26f, 0.22f), TownSwatch.MoonGlow);
        }
    }
}
