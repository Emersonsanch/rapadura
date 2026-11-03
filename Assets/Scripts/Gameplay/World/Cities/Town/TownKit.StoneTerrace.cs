using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Sangkent, the stair city of the Teto do Mundo (Docs/Mapas100.md §2.1 and map 36, style <c>StoneTerrace</c>):
    /// the two-storey stone-and-loam houses of the Pamir (<see cref="StoneTerraceHouse"/>: rubble stone below, loam above,
    /// juniper lacing beams with their ends showing, a lattice balcony, a flat roof with a parapet, the square skylight and
    /// apricots drying, a timber porch on posts), the dry-stone terraces stepping up behind the town to the fort (the
    /// 7 terraces of the ficha seen from the 4th, where the plaza is), the polished stone chutes coming down the terraces and
    /// their plank carts with the five ribbons of the five lights, the stone irrigation channels with their slab bridges,
    /// apricot trees in blossom, the Marco das Caravanas on the plaza and the stone gate at the streets' ends; Sisinnios's
    /// mountain gear. Same rules as the rest of the kit: flat colours from <see cref="TownPalette"/> (row 13 holds terrace
    /// stone, loam, juniper, firn, apricot blossom, the silk red and blue and the hearth glow), low-poly, deterministic. The
    /// ribbons are the caravans' colours (the five lights met at this crossroads), not a rite.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Half the thickness of a <see cref="CityPropKind.TerraceWall"/>'s foot: its collider is the run inflated by this.</summary>
        public const float TerraceWallHalfThickness = 0.5f;

        /// <summary>Depth of one terrace step behind a <see cref="CityPropKind.TerraceWall"/> (three steps).</summary>
        public const float TerraceStepDepth = 2.1f;

        /// <summary>Height of one terrace step (× the prop's scale).</summary>
        public const float TerraceStepHeight = 1.45f;

        /// <summary>Footprint of a <see cref="CityPropKind.StoneChute"/>'s landing (local x across, z along the chute; × its scale): its collider.</summary>
        public static readonly Vector2 ChuteLandingSize = new Vector2(1.9f, 2.2f);

        /// <summary>Footprint of a <see cref="CityPropKind.ChuteCart"/> (local x across, z along; × its scale): its collider.</summary>
        public static readonly Vector2 ChuteCartSize = new Vector2(1.1f, 1.8f);

        /// <summary>Radius of an <see cref="CityPropKind.ApricotTree"/>'s ring of stones (× its scale): its collider.</summary>
        public const float ApricotTreeRadius = 0.55f;

        /// <summary>Radius of the <see cref="CityPropKind.CaravanStone"/>'s round step (× its scale): its collider.</summary>
        public const float CaravanStoneRadius = 1.15f;

        /// <summary>Half the width of the water of an <see cref="CityPropKind.IrrigationChannel"/>.</summary>
        public const float IrrigationHalfWidth = 0.5f;

        /// <summary>The five ribbons of the five lights, in the caravans' order (Ar Sereno, Vento Vivo, Luz, Água Viva, Fogo Vivo).</summary>
        private static readonly TownSwatch[] FiveLights =
        {
            TownSwatch.ClothWhite, TownSwatch.ClothGreen, TownSwatch.ClothYellow, TownSwatch.SilkBlue, TownSwatch.SilkRed,
        };

        // ------------------------------------------------------------------
        // The stone-and-loam house
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.StoneTerrace"/> (local frame, front −z): a rubble-stone plinth and ground storey, the
        /// upper storey in <see cref="TownBuildingSpec.WallColor"/> (loam or stone), juniper lacing beams round both storeys
        /// with the beam ends of the floor showing on the front, the door and small deep windows below, lit windows above and
        /// a lattice balcony on the side away from the door, a flat roof with a parapet, the square skylight lantern of the
        /// Pamiri house, apricots drying and a stack of firewood; a flagstone deck in front with juniper posts (at
        /// <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) carrying a flat porch roof at the
        /// first-floor line, the sign hanging from it.
        /// </summary>
        public static void StoneTerraceHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float floorY = TownBuildingBuilder.FloorY;
            float storey = Mathf.Max(2.1f, spec.WallHeight);
            float mid = floorY + storey;
            float roofY = Mathf.Max(top, mid + 1.9f);
            TownSwatch beam = spec.JoineryColor;

            // Plinth and the two storeys.
            b.BoxMinMax(new Vector3(-hw - 0.06f, 0f, zFacade - 0.06f), new Vector3(hw + 0.06f, floorY, zBack + 0.06f), spec.PlinthColor);
            b.BoxMinMax(new Vector3(-hw, floorY, zFacade), new Vector3(hw, mid, zBack), TownSwatch.TerraceStone);
            b.BoxMinMax(new Vector3(-hw + 0.04f, mid, zFacade + 0.04f), new Vector3(hw - 0.04f, roofY, zBack - 0.04f), spec.WallColor);

            // Juniper lacing: a beam round the walls at the floor line and under the roof; one more halfway up the stone storey.
            foreach (float y in new[] { floorY + storey * 0.5f, mid - 0.08f, roofY - 0.12f })
            {
                b.BoxMinMax(new Vector3(-hw - 0.03f, y, zFacade - 0.03f), new Vector3(hw + 0.03f, y + 0.12f, zBack + 0.03f), beam);
            }

            // The beam ends of the upper floor showing on the facade.
            int ends = Mathf.Max(3, Mathf.RoundToInt(hw * 2f / 0.9f));
            for (int i = 0; i <= ends; i++)
            {
                float x = Mathf.Lerp(-hw + 0.15f, hw - 0.15f, i / (float)ends);
                b.BoxMinMax(new Vector3(x - 0.07f, mid - 0.2f, zFacade - 0.18f), new Vector3(x + 0.07f, mid - 0.06f, zFacade), beam);
            }

            // Door with a heavy juniper frame and lintel.
            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.45f : 1.05f;
            float doorTop = floorY + Mathf.Min(2.05f, storey - 0.25f);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, floorY, zFacade - 0.03f), new Vector3(doorX + doorW * 0.5f, doorTop, zFacade - 0.01f), TownSwatch.DoorPlank);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.13f, floorY, zFacade - 0.08f), new Vector3(doorX - doorW * 0.5f, doorTop, zFacade), beam);
            b.BoxMinMax(new Vector3(doorX + doorW * 0.5f, floorY, zFacade - 0.08f), new Vector3(doorX + doorW * 0.5f + 0.13f, doorTop, zFacade), beam);
            b.BoxMinMax(new Vector3(doorX - doorW * 0.5f - 0.3f, doorTop, zFacade - 0.1f), new Vector3(doorX + doorW * 0.5f + 0.3f, doorTop + 0.18f, zFacade), beam);

            // Small deep windows below (away from the door), lit windows above.
            float balconySide = doorX > 0.01f ? -1f : 1f;
            int below = Mathf.Clamp(spec.FrontWindows, 0, 2);
            for (int i = 0; i < below; i++)
            {
                float x = balconySide * Mathf.Lerp(hw * 0.3f, hw * 0.72f, below == 1 ? 0.5f : i);
                if (Mathf.Abs(x - doorX) < doorW * 0.5f + 0.5f)
                {
                    continue;
                }

                b.BoxMinMax(new Vector3(x - 0.22f, floorY + 1.0f, zFacade - 0.02f), new Vector3(x + 0.22f, floorY + 1.5f, zFacade - 0.005f), TownSwatch.Black);
                b.BoxMinMax(new Vector3(x - 0.3f, floorY + 0.94f, zFacade - 0.07f), new Vector3(x + 0.3f, floorY + 1.0f, zFacade), beam);
            }

            float winY = mid + 0.6f;
            b.BoxMinMax(new Vector3(doorX - 0.28f, winY, zFacade + 0.02f), new Vector3(doorX + 0.28f, winY + 0.62f, zFacade + 0.035f), TownSwatch.WindowLit);
            b.BoxMinMax(new Vector3(doorX - 0.36f, winY - 0.06f, zFacade + 0.0f), new Vector3(doorX + 0.36f, winY, zFacade + 0.04f), beam);

            // The lattice balcony on the upper floor (the jharokha of Baltit), on the side away from the door.
            float bx = balconySide * Mathf.Min(hw * 0.5f, hw - 1.0f);
            float bw = Mathf.Min(1.7f, hw * 0.75f);
            float by = mid + 0.1f;
            float bz = zFacade + 0.04f;
            b.BoxMinMax(new Vector3(bx - bw * 0.5f - 0.1f, by - 0.1f, bz - 0.62f), new Vector3(bx + bw * 0.5f + 0.1f, by + 0.04f, bz), beam);
            b.BoxMinMax(new Vector3(bx - bw * 0.5f, by + 0.04f, bz - 0.55f), new Vector3(bx + bw * 0.5f, by + 1.3f, bz - 0.5f), TownSwatch.HearthGlow);
            foreach (float x in new[] { bx - bw * 0.5f, bx - bw * 0.17f, bx + bw * 0.17f, bx + bw * 0.5f })
            {
                b.BoxMinMax(new Vector3(x - 0.04f, by + 0.04f, bz - 0.6f), new Vector3(x + 0.04f, by + 1.3f, bz - 0.52f), beam);
            }

            foreach (float y in new[] { by + 0.45f, by + 0.88f })
            {
                b.BoxMinMax(new Vector3(bx - bw * 0.5f, y, bz - 0.6f), new Vector3(bx + bw * 0.5f, y + 0.06f, bz - 0.52f), beam);
            }

            b.Slab(new Vector3(bx - bw * 0.5f - 0.18f, by + 1.32f, bz - 0.72f), new Vector3(bx - bw * 0.5f - 0.18f, by + 1.5f, bz),
                new Vector3(bx + bw * 0.5f + 0.18f, by + 1.5f, bz), new Vector3(bx + bw * 0.5f + 0.18f, by + 1.32f, bz - 0.72f), 0.07f, spec.RoofColor, beam);

            // The flat roof, its parapet, the square skylight lantern, apricots drying and the firewood.
            b.BoxMinMax(new Vector3(-hw - 0.12f, roofY, zFacade - 0.12f), new Vector3(hw + 0.12f, roofY + 0.16f, zBack + 0.12f), beam, spec.RoofColor);
            float py = roofY + 0.16f;
            b.BoxMinMax(new Vector3(-hw - 0.12f, py, zFacade - 0.12f), new Vector3(hw + 0.12f, py + 0.32f, zFacade + 0.1f), spec.WallColor);
            b.BoxMinMax(new Vector3(-hw - 0.12f, py, zBack - 0.1f), new Vector3(hw + 0.12f, py + 0.32f, zBack + 0.12f), spec.WallColor);
            b.BoxMinMax(new Vector3(-hw - 0.12f, py, zFacade + 0.1f), new Vector3(-hw + 0.1f, py + 0.32f, zBack - 0.1f), spec.WallColor);
            b.BoxMinMax(new Vector3(hw - 0.1f, py, zFacade + 0.1f), new Vector3(hw + 0.12f, py + 0.32f, zBack - 0.1f), spec.WallColor);

            float zMid = (zFacade + zBack) * 0.5f;
            float sx = -balconySide * hw * 0.3f;
            b.BoxMinMax(new Vector3(sx - 0.55f, py, zMid - 0.55f), new Vector3(sx + 0.55f, py + 0.45f, zMid + 0.55f), beam, TownSwatch.HearthGlow);
            b.BoxMinMax(new Vector3(sx - 0.38f, py + 0.45f, zMid - 0.38f), new Vector3(sx + 0.38f, py + 0.7f, zMid + 0.38f), beam, TownSwatch.Black);

            float ax = balconySide * hw * 0.42f;
            b.BoxMinMax(new Vector3(ax - 0.7f, py + 0.01f, zMid - 0.5f), new Vector3(ax + 0.7f, py + 0.04f, zMid + 0.5f), TownSwatch.ClothWhite);
            for (int i = 0; i < 6; i++)
            {
                float fx = ax - 0.5f + (i % 3) * 0.5f;
                float fz = zMid - 0.25f + (i / 3) * 0.5f;
                b.Box(new Vector3(fx, py + 0.07f, fz), new Vector3(0.28f, 0.05f, 0.28f), TownSwatch.Fruit, 0f);
            }

            float wz = zBack - 0.5f;
            for (int i = 0; i < 3; i++)
            {
                float y = py + 0.1f + i * 0.17f;
                float x0 = -hw * 0.5f + (i % 2) * 0.1f;
                b.Log(new Vector3(x0, y, wz), new Vector3(x0 + 1.1f, y, wz), 0.085f, TownSwatch.Trunk, 5, TownSwatch.WoodLight);
            }

            // A fall of snow on the back of the roof.
            b.BoxMinMax(new Vector3(-hw * 0.9f, roofY + 0.16f, zBack - 1.0f), new Vector3(-hw * 0.1f, roofY + 0.22f, zBack - 0.15f), TownSwatch.Firn);

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), TownSwatch.TerraceStone, TownSwatch.Paving);

                float z = zFront + TownBuildingBuilder.PostInset;
                float porchTop = Mathf.Max(doorTop + 0.35f, mid - 0.3f);
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Box(new Vector3(x, deck + 0.08f, z), new Vector3(0.3f, 0.16f, 0.3f), TownSwatch.StoneDark, 0f);
                    b.Log(new Vector3(x, deck + 0.16f, z), new Vector3(x, porchTop - 0.18f, z), 0.09f, beam, 6);
                    b.Box(new Vector3(x, porchTop - 0.1f, z), new Vector3(0.42f, 0.16f, 0.3f), beam, 0f);
                }

                b.BoxMinMax(new Vector3(-hw - 0.05f, porchTop - 0.02f, z - 0.15f), new Vector3(hw + 0.05f, porchTop + 0.12f, z + 0.15f), beam);
                b.Slab(new Vector3(-hw - 0.15f, porchTop + 0.12f, z - 0.35f), new Vector3(-hw - 0.15f, porchTop + 0.32f, zFacade),
                    new Vector3(hw + 0.15f, porchTop + 0.32f, zFacade), new Vector3(hw + 0.15f, porchTop + 0.12f, z - 0.35f), 0.12f, spec.RoofColor, beam);

                if (spec.Sign)
                {
                    var at = new Vector3(doorX, porchTop - 0.45f, z - 0.05f);
                    b.Log(at + new Vector3(-0.55f, 0.4f, 0f), at + new Vector3(-0.55f, 0.2f, 0f), 0.012f, TownSwatch.Black, 3);
                    b.Log(at + new Vector3(0.55f, 0.4f, 0f), at + new Vector3(0.55f, 0.2f, 0f), 0.012f, TownSwatch.Black, 3);
                    SignBoard(b, at, spec.SignColor, spec.AccentColor);
                }
            }
        }

        /// <summary>The stone gate of Sangkent: two dry-stone piers laced with juniper, a juniper lintel under a little flat roof, the five ribbons and two lanterns.</summary>
        public static void TerraceGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            const float height = 3.4f;
            foreach (Vector3 p in new[] { a, c })
            {
                b.Box(p + Vector3.up * (height * 0.5f), new Vector3(0.85f, height, 0.85f), TownSwatch.TerraceStone, 0f, TownSwatch.Loam);
                foreach (float y in new[] { 1.1f, 2.3f })
                {
                    b.Box(p + Vector3.up * y, new Vector3(0.9f, 0.12f, 0.9f), TownSwatch.Juniper, 0f);
                }

                b.Crown(p + Vector3.up * (height + 0.12f), new Vector3(0.55f, 0.25f, 0.55f), TownSwatch.Firn);
            }

            Vector3 along = (c - a).normalized;
            var across = new Vector3(-along.z, 0f, along.x);
            Vector3 ta = a + Vector3.up * (height - 0.25f) - along * 0.5f;
            Vector3 tc = c + Vector3.up * (height - 0.25f) + along * 0.5f;
            b.Log(ta, tc, 0.13f, TownSwatch.Juniper, 5);
            b.Slab(ta - across * 0.6f + Vector3.up * 0.15f, ta + across * 0.6f + Vector3.up * 0.15f, tc + across * 0.6f + Vector3.up * 0.15f, tc - across * 0.6f + Vector3.up * 0.15f,
                0.12f, TownSwatch.Loam, TownSwatch.Juniper);

            for (int i = 0; i < FiveLights.Length; i++)
            {
                Vector3 h = Vector3.Lerp(ta, tc, 0.3f + 0.1f * i) + Vector3.down * 0.12f;
                b.Box(h + Vector3.down * 0.45f, new Vector3(0.12f, 0.8f, 0.03f), FiveLights[i], 0f);
            }

            foreach (float t in new[] { 0.12f, 0.88f })
            {
                Vector3 h = Vector3.Lerp(ta, tc, t) + Vector3.down * 0.12f;
                b.Log(h, h + Vector3.down * 0.35f, 0.012f, TownSwatch.Black, 3);
                b.Box(h + Vector3.down * 0.52f, new Vector3(0.22f, 0.3f, 0.22f), TownSwatch.HearthGlow, 0f);
            }
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>
        /// The terraces stepping up behind the line from the origin to <paramref name="d"/> (the mass on the left, local +z): a
        /// dry-stone retaining wall (its collider is the run inflated by <see cref="TerraceWallHalfThickness"/>), then three
        /// steps of <see cref="TerraceStepDepth"/>, each a field strip with apricot trees and, on the upper two, small stone
        /// houses with lit windows; variant 1 puts the fort of the 7th terrace (a stone tower laced with juniper, a lattice
        /// balcony, a flat roof with snow) in the middle of the top step.
        /// </summary>
        private static void TerraceWall(TownMeshBuilder b, Vector2 d, float s, int variant)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            string key = "terrace" + Mathf.RoundToInt(length * 10f) + "_" + variant;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                const int steps = 3;
                for (int i = 0; i < steps; i++)
                {
                    float z0 = -TerraceWallHalfThickness + i * TerraceStepDepth;
                    float z1 = TerraceWallHalfThickness + steps * TerraceStepDepth - 0.6f;
                    float h = (i + 1) * TerraceStepHeight * s;
                    float x0 = -0.2f - i * 0.3f;
                    float x1 = length + 0.2f + i * 0.3f;
                    b.BoxMinMax(new Vector3(x0, 0f, z0), new Vector3(x1, h, z1), TownSwatch.TerraceStone, i == steps - 1 ? TownSwatch.GrassDark : TownSwatch.Grass);
                    b.BoxMinMax(new Vector3(x0, h * 0.45f + i * 0.2f, z0 - 0.02f), new Vector3(x1, h * 0.45f + i * 0.2f + 0.06f, z0), TownSwatch.StoneDark);
                    b.BoxMinMax(new Vector3(x0, h - 0.1f, z0 - 0.06f), new Vector3(x1, h + 0.04f, z0 + 0.25f), TownSwatch.StoneDark);

                    // On the strip of this step: apricot trees and, higher up, a house or two.
                    float stripZ = z0 + TerraceStepDepth * 0.5f + 0.25f;
                    int count = Mathf.Max(1, Mathf.RoundToInt(length / 4.5f));
                    for (int k = 0; k < count; k++)
                    {
                        float t = (k + 0.3f + 0.4f * TownStyle.Hash01(key, i * 13 + k)) / count;
                        float x = Mathf.Lerp(0.6f, length - 0.6f, t);
                        bool house = i > 0 && TownStyle.Hash01(key, i * 31 + k + 5) < 0.45f;
                        if (variant == 1 && i == steps - 1 && Mathf.Abs(x - length * 0.5f) < 2.6f)
                        {
                            continue;
                        }

                        if (house)
                        {
                            float hw = 0.9f + 0.3f * TownStyle.Hash01(key, k + 40);
                            float hh = 1.6f + 0.6f * TownStyle.Hash01(key, k + 50);
                            b.BoxMinMax(new Vector3(x - hw, h, stripZ - 0.6f), new Vector3(x + hw, h + hh, stripZ + 0.7f), (k % 2) == 0 ? TownSwatch.Loam : TownSwatch.TerraceStone);
                            b.BoxMinMax(new Vector3(x - hw - 0.08f, h + hh, stripZ - 0.68f), new Vector3(x + hw + 0.08f, h + hh + 0.12f, stripZ + 0.78f), TownSwatch.Juniper, TownSwatch.Firn);
                            b.BoxMinMax(new Vector3(x - 0.2f, h + hh * 0.45f, stripZ - 0.62f), new Vector3(x + 0.2f, h + hh * 0.45f + 0.4f, stripZ - 0.6f), TownSwatch.WindowLit);
                        }
                        else
                        {
                            b.Log(new Vector3(x, h, stripZ), new Vector3(x + 0.1f, h + 0.9f * s, stripZ), 0.08f, TownSwatch.Trunk, 5);
                            b.Crown(new Vector3(x + 0.1f, h + 1.25f * s, stripZ), new Vector3(1.3f, 0.9f, 1.2f) * s,
                                (k + i) % 3 == 0 ? TownSwatch.LeafDark : TownSwatch.ApricotBlossom);
                        }
                    }
                }

                if (variant == 1)
                {
                    // The fort of the 7th terrace (Baltit): a stone tower laced with juniper, its lattice balcony and a lantern.
                    float baseY = steps * TerraceStepHeight * s;
                    float fz = TerraceWallHalfThickness + (steps - 1) * TerraceStepDepth + 0.8f;
                    float fx = length * 0.5f;
                    float fh = 4.6f * s;
                    b.BoxMinMax(new Vector3(fx - 1.9f, baseY, fz - 0.9f), new Vector3(fx + 1.9f, baseY + fh * 0.62f, fz + 1.4f), TownSwatch.TerraceStone);
                    b.BoxMinMax(new Vector3(fx - 1.2f, baseY + fh * 0.62f, fz - 0.6f), new Vector3(fx + 1.2f, baseY + fh, fz + 1.2f), TownSwatch.Loam);
                    foreach (float y in new[] { baseY + fh * 0.3f, baseY + fh * 0.6f, baseY + fh - 0.12f })
                    {
                        float w = y > baseY + fh * 0.62f ? 1.23f : 1.93f;
                        float zf = y > baseY + fh * 0.62f ? fz - 0.63f : fz - 0.93f;
                        b.BoxMinMax(new Vector3(fx - w, y, zf), new Vector3(fx + w, y + 0.12f, fz + 1.42f), TownSwatch.Juniper);
                    }

                    b.BoxMinMax(new Vector3(fx - 0.7f, baseY + fh * 0.7f, fz - 1.1f), new Vector3(fx + 0.7f, baseY + fh * 0.7f + 0.9f, fz - 0.6f), TownSwatch.HearthGlow, TownSwatch.Juniper);
                    b.BoxMinMax(new Vector3(fx - 1.35f, baseY + fh, fz - 0.75f), new Vector3(fx + 1.35f, baseY + fh + 0.15f, fz + 1.35f), TownSwatch.Juniper, TownSwatch.Firn);
                    b.BoxMinMax(new Vector3(fx - 0.35f, baseY + fh * 0.2f, fz - 0.92f), new Vector3(fx + 0.35f, baseY + fh * 0.2f + 1.0f, fz - 0.9f), TownSwatch.DoorPlank);
                    b.Log(new Vector3(fx + 1.0f, baseY + fh + 0.15f, fz), new Vector3(fx + 1.0f, baseY + fh + 1.6f, fz), 0.04f, TownSwatch.Juniper, 4);
                    for (int i = 0; i < FiveLights.Length; i++)
                    {
                        b.Box(new Vector3(fx + 1.0f + 0.25f, baseY + fh + 1.45f - i * 0.16f, fz), new Vector3(0.45f, 0.12f, 0.02f), FiveLights[i], 0f);
                    }
                }

                // Snow lying along the top of the last step.
                float topY = steps * TerraceStepHeight * s;
                float zTop = TerraceWallHalfThickness + steps * TerraceStepDepth - 1.2f;
                b.BoxMinMax(new Vector3(-0.6f, topY, zTop), new Vector3(length + 0.6f, topY + 0.07f, zTop + 0.6f), TownSwatch.Firn);
            });
        }

        /// <summary>
        /// The foot of a stone chute (local frame, the landing facing −z): a flagstone landing (its collider is
        /// <see cref="ChuteLandingSize"/>) with a juniper buffer at its front, the polished trough coming down the terraces
        /// behind (+z) on stone piers, its two rails, a waiting cart at the bottom and a lantern on a post.
        /// </summary>
        private static void StoneChute(TownMeshBuilder b, float s)
        {
            float hx = ChuteLandingSize.x * 0.5f * s;
            float hz = ChuteLandingSize.y * 0.5f * s;
            b.BoxMinMax(new Vector3(-hx, 0f, -hz), new Vector3(hx, 0.22f, hz), TownSwatch.TerraceStone, TownSwatch.Paving);
            b.BoxMinMax(new Vector3(-hx + 0.15f, 0.22f, -hz + 0.05f), new Vector3(hx - 0.15f, 0.55f, -hz + 0.3f), TownSwatch.Juniper);

            // The polished trough, from the landing up the terraces (out to ~7 m behind, ~4.6 m up).
            var low = new Vector3(0f, 0.3f, -hz + 0.6f);
            var high = new Vector3(0f, 4.6f * s, hz + 6.5f * s);
            const float w = 0.48f;
            b.Slab(new Vector3(-w, low.y, low.z), new Vector3(-w, high.y, high.z), new Vector3(w, high.y, high.z), new Vector3(w, low.y, low.z), 0.1f, TownSwatch.Paving, TownSwatch.TerraceStone);
            foreach (float x in new[] { -w - 0.05f, w + 0.05f })
            {
                b.Log(new Vector3(x, low.y + 0.18f, low.z), new Vector3(x, high.y + 0.18f, high.z), 0.07f, TownSwatch.TerraceStone, 4);
            }

            for (int i = 1; i <= 3; i++)
            {
                Vector3 p = Vector3.Lerp(low, high, i / 4f);
                b.Box(new Vector3(0f, (p.y - 0.05f) * 0.5f, p.z), new Vector3(0.7f, p.y - 0.05f, 0.45f), TownSwatch.StoneDark, 0f);
            }

            // The cart waiting at the bottom and the station lantern.
            b.With(new Vector3(0f, 0.22f, -0.05f), 0f, () => CartBody(b, 0.8f, 0));
            b.Log(new Vector3(hx - 0.15f, 0.22f, hz - 0.2f), new Vector3(hx - 0.15f, 2.4f, hz - 0.2f), 0.06f, TownSwatch.Juniper, 5);
            b.Log(new Vector3(hx - 0.15f, 2.35f, hz - 0.2f), new Vector3(hx - 0.55f, 2.35f, hz - 0.2f), 0.03f, TownSwatch.Iron, 4);
            b.Box(new Vector3(hx - 0.55f, 2.0f, hz - 0.2f), new Vector3(0.22f, 0.3f, 0.22f), TownSwatch.HearthGlow, 0f);
        }

        /// <summary>A plank chute cart on its own (variant 0: loaded with apricots, 1: with felt bales).</summary>
        private static void ChuteCart(TownMeshBuilder b, float s, int variant)
        {
            b.With(Vector3.zero, 0f, new Vector3(s, s, s), () => CartBody(b, 1f, variant % 2 == 0 ? 1 : 2));
        }

        /// <summary>The plank cart of the chutes: two stone-shod runners, a plank bed with low sides, the bowed front and the five ribbons on a short pole; a load by <paramref name="load"/> (0 none, 1 apricots, 2 felt bales).</summary>
        private static void CartBody(TownMeshBuilder b, float k, int load)
        {
            float hx = ChuteCartSize.x * 0.5f * k;
            float hz = ChuteCartSize.y * 0.5f * k;
            foreach (float x in new[] { -hx + 0.12f, hx - 0.12f })
            {
                b.BoxMinMax(new Vector3(x - 0.06f, 0f, -hz), new Vector3(x + 0.06f, 0.12f, hz), TownSwatch.StoneDark);
            }

            b.BoxMinMax(new Vector3(-hx, 0.12f, -hz + 0.1f), new Vector3(hx, 0.24f, hz), TownSwatch.Juniper, TownSwatch.WoodWeathered);
            b.BoxMinMax(new Vector3(-hx, 0.24f, hz - 0.08f), new Vector3(hx, 0.5f, hz), TownSwatch.Juniper);
            b.BoxMinMax(new Vector3(-hx, 0.24f, -hz + 0.1f), new Vector3(-hx + 0.07f, 0.45f, hz), TownSwatch.Juniper);
            b.BoxMinMax(new Vector3(hx - 0.07f, 0.24f, -hz + 0.1f), new Vector3(hx, 0.45f, hz), TownSwatch.Juniper);
            b.Slab(new Vector3(-hx, 0.24f, -hz + 0.1f), new Vector3(-hx, 0.62f, -hz - 0.12f), new Vector3(hx, 0.62f, -hz - 0.12f), new Vector3(hx, 0.24f, -hz + 0.1f), 0.06f,
                TownSwatch.WoodWeathered, TownSwatch.Juniper);

            var pole = new Vector3(-hx + 0.1f, 0.5f, -hz + 0.15f);
            b.Log(pole, pole + Vector3.up * 0.9f, 0.025f, TownSwatch.Juniper, 4);
            for (int i = 0; i < FiveLights.Length; i++)
            {
                b.Box(pole + new Vector3(0.2f, 0.85f - i * 0.11f, 0f), new Vector3(0.36f, 0.08f, 0.02f), FiveLights[i], 0f);
            }

            switch (load)
            {
                case 1:
                    b.Crown(new Vector3(0f, 0.3f, 0.05f), new Vector3(hx * 1.6f, 0.35f, hz * 1.4f), TownSwatch.Fruit);
                    break;
                case 2:
                    b.Blob(new Vector3(-hx * 0.4f, 0.45f, 0f), new Vector3(0.45f, 0.4f, 0.7f), TownSwatch.ClothWhite);
                    b.Blob(new Vector3(hx * 0.4f, 0.45f, 0.1f), new Vector3(0.45f, 0.4f, 0.7f), TownSwatch.SilkRed);
                    break;
            }
        }

        /// <summary>An apricot tree in a ring of field stones: a twisted trunk, three crowns of blossom (variant 1: green leaf and orange fruit).</summary>
        private static void ApricotTree(TownMeshBuilder b, float s, int variant)
        {
            float r = ApricotTreeRadius * s;
            b.Ring(new Vector3(0f, 0.02f, 0f), r * 0.75f, r, 8, TownSwatch.StoneDark, TownSwatch.TerraceStone);
            b.Disc(new Vector3(0f, 0.03f, 0f), r * 0.75f, 8, TownSwatch.Loam);
            b.Log(Vector3.zero, new Vector3(0.12f, 1.1f * s, 0.05f), 0.11f * s, TownSwatch.Trunk, 5);
            b.Log(new Vector3(0.12f, 1.1f * s, 0.05f), new Vector3(-0.35f, 1.7f * s, -0.1f), 0.07f * s, TownSwatch.Trunk, 4);
            b.Log(new Vector3(0.12f, 1.1f * s, 0.05f), new Vector3(0.55f, 1.8f * s, 0.15f), 0.07f * s, TownSwatch.Trunk, 4);
            TownSwatch crown = (variant % 2) == 0 ? TownSwatch.ApricotBlossom : TownSwatch.LeafDark;
            b.Crown(new Vector3(-0.4f, 2.05f * s, -0.1f), new Vector3(1.3f, 0.95f, 1.2f) * s, crown);
            b.Crown(new Vector3(0.6f, 2.15f * s, 0.15f), new Vector3(1.2f, 0.9f, 1.2f) * s, crown);
            b.Crown(new Vector3(0.05f, 2.55f * s, 0f), new Vector3(1.1f, 0.85f, 1.0f) * s, (variant % 2) == 0 ? TownSwatch.ClothWhite : crown);
            if ((variant % 2) == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    float a = (i * 90f + 30f) * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.6f * s, 1.85f * s, Mathf.Sin(a) * 0.6f * s), new Vector3(0.14f, 0.14f, 0.14f), TownSwatch.Fruit);
                }
            }
            else
            {
                b.Disc(new Vector3(0.3f, 0.04f, -0.35f), 0.35f * s, 6, TownSwatch.ApricotBlossom);
            }
        }

        /// <summary>
        /// A stone irrigation channel from the origin to <paramref name="d"/> (water <see cref="IrrigationHalfWidth"/> each side
        /// between low kerbs) crossed at its middle by a stone slab with a little parapet at both ends. Walkable: the slab and
        /// the kerbs stay under the walker's step.
        /// </summary>
        private static void IrrigationChannel(TownMeshBuilder b, Vector2 d)
        {
            float length = d.magnitude;
            if (length < 0.5f)
            {
                return;
            }

            float w = IrrigationHalfWidth;
            b.With(Vector3.zero, YawAlong(d / length), () =>
            {
                b.Flat(0f, -w, length, w, 0.04f, TownSwatch.Water);
                foreach (float z in new[] { -w - 0.12f, w })
                {
                    b.BoxMinMax(new Vector3(0f, 0f, z), new Vector3(length, 0.1f, z + 0.12f), TownSwatch.TerraceStone, TownSwatch.Paving);
                }

                float xm = length * 0.5f;
                float half = Mathf.Min(1.4f, length * 0.5f - 0.1f);
                b.BoxMinMax(new Vector3(xm - half, 0.06f, -w - 0.3f), new Vector3(xm + half, 0.16f, w + 0.3f), TownSwatch.TerraceStone, TownSwatch.Paving);
                foreach (float x in new[] { xm - half, xm + half - 0.25f })
                {
                    b.BoxMinMax(new Vector3(x, 0.16f, -w - 0.3f), new Vector3(x + 0.25f, 0.42f, -w + 0.05f), TownSwatch.StoneDark);
                    b.BoxMinMax(new Vector3(x, 0.16f, w - 0.05f), new Vector3(x + 0.25f, 0.42f, w + 0.3f), TownSwatch.StoneDark);
                }
            });
        }

        /// <summary>
        /// The Marco das Caravanas on the plaza (the 4th terrace): a round stone step, a tall milestone with a juniper band and
        /// a crossbar at the top hung with the five ribbons of the five lights and a lantern, a stone trough of meltwater at
        /// its foot, where every caravan of the Rota da Seda waters its animals.
        /// </summary>
        private static void CaravanStone(TownMeshBuilder b, float s)
        {
            float r = CaravanStoneRadius * s;
            b.Cylinder(Vector3.zero, r, r * 0.95f, 0.3f, 10, TownSwatch.TerraceStone, TownSwatch.Paving);
            b.Ring(new Vector3(0f, 0.31f, 0f), r - 0.18f, r * 0.95f, 10, TownSwatch.StoneDark);
            float h = 3.0f * s;
            b.Box(new Vector3(0f, 0.3f + h * 0.5f, 0.1f), new Vector3(0.55f, h, 0.42f) * 1f, TownSwatch.TerraceStone, 0f, TownSwatch.Firn);
            b.Box(new Vector3(0f, 0.3f + h * 0.62f, 0.1f), new Vector3(0.6f, 0.14f, 0.47f), TownSwatch.Juniper, 0f);
            b.Box(new Vector3(0f, 0.3f + h * 0.4f, -0.12f), new Vector3(0.36f, 0.5f, 0.02f), TownSwatch.Loam, 0f);

            float barY = 0.3f + h + 0.1f;
            b.Log(new Vector3(-0.85f * s, barY, 0.1f), new Vector3(0.85f * s, barY, 0.1f), 0.06f, TownSwatch.Juniper, 5);
            for (int i = 0; i < FiveLights.Length; i++)
            {
                float x = Mathf.Lerp(-0.7f, 0.7f, i / 4f) * s;
                b.Box(new Vector3(x, barY - 0.5f, 0.1f), new Vector3(0.13f, 0.85f, 0.025f), FiveLights[i], 0f);
            }

            Lantern(b, new Vector3(0f, barY - 0.75f, -0.35f), 0.9f);
            b.BoxMinMax(new Vector3(-0.75f * s, 0.3f, -r + 0.05f), new Vector3(0.75f * s, 0.62f, -r + 0.55f), TownSwatch.StoneDark, TownSwatch.Water);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Sisinnios's front: a rack of quilted felt coats, crampons hanging on a peg, a stack of folded storm cloaks, a coil of rope and a ribbon pole.</summary>
        private static void StoneTerraceDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            // The coat rack.
            foreach (float lx in new[] { -0.7f, 0.7f })
            {
                b.Log(new Vector3(x + lx, y, z - 0.45f), new Vector3(x + lx, y + 1.55f, z - 0.45f), 0.045f, TownSwatch.Juniper, 4);
            }

            b.Log(new Vector3(x - 0.75f, y + 1.5f, z - 0.45f), new Vector3(x + 0.75f, y + 1.5f, z - 0.45f), 0.035f, TownSwatch.Juniper, 4);
            TownSwatch[] coats = { TownSwatch.SilkRed, TownSwatch.ClothWhite, TownSwatch.SilkBlue };
            for (int i = 0; i < coats.Length; i++)
            {
                float cx = x - 0.45f + i * 0.45f;
                b.Box(new Vector3(cx, y + 1.0f, z - 0.45f), new Vector3(0.36f, 0.95f, 0.14f), coats[i], 0f);
                b.Box(new Vector3(cx, y + 1.42f, z - 0.45f), new Vector3(0.4f, 0.1f, 0.16f), TownSwatch.Firn, 0f);
            }

            // Crampons hanging on a peg by the door.
            for (int i = 0; i < 2; i++)
            {
                b.Box(new Vector3(x + 0.95f, y + 1.1f - i * 0.28f, zFacade - 0.05f), new Vector3(0.18f, 0.22f, 0.04f), TownSwatch.Iron, 0f);
            }

            // Folded storm cloaks, a coil of rope.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(x - 0.2f, y + 0.08f + i * 0.13f, z - 0.95f), new Vector3(0.6f - i * 0.05f, 0.12f, 0.42f), (i % 2) == 0 ? TownSwatch.SilkBlue : TownSwatch.Juniper, 0f);
            }

            b.Cylinder(new Vector3(x + 0.5f, y, z - 0.95f), 0.22f, 0.2f, 0.18f, 8, TownSwatch.WoodLight, TownSwatch.Beam);
        }
    }
}
