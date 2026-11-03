using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// The pieces of Aratta, the city of the horns of the povo-touro on the Savana dos Auroques (Docs/Mapas100.md §3.3 and
    /// map 26): the great halls of bound reed (<see cref="ReedHallHouse"/>: the mudhif of the Mesopotamian marshes, a barrel
    /// vault of reed bundles bent into arches and covered in mats, a reed lattice front with a tall arched door between two
    /// thick bundled columns, a pair of horns over the door, a mat awning on reed poles), the horn posts with blue ribbons
    /// (way-markers, not cult objects), the round dry-stone enclosures, the bull-headed lyre of the plaza (the Lira do Touro),
    /// the flat-topped acacias and the reed gate at the streets' ends; Lugalbanda's wanderer dressing. Same rules as the rest
    /// of the kit: flat colours from <see cref="TownPalette"/> (row 10 holds the ficha's reed, mat, golden grass and ribbon
    /// blue), low-poly, deterministic.
    /// </summary>
    public static partial class TownKit
    {
        /// <summary>Radius of a <see cref="CityPropKind.HornPost"/>'s foot (× its scale): its collider.</summary>
        public const float HornPostRadius = 0.35f;

        /// <summary>Radius of a <see cref="CityPropKind.StoneKraal"/>'s dry-stone ring (× its scale): its collider covers the whole pen.</summary>
        public const float KraalRadius = 2.6f;

        /// <summary>Footprint of the <see cref="CityPropKind.BullLyre"/>'s clay plinth (local x × z, × its scale): its collider box.</summary>
        public static readonly Vector2 BullLyreSize = new Vector2(1.8f, 1.0f);

        /// <summary>Radius of an <see cref="CityPropKind.Acacia"/>'s trunk collider (× its scale).</summary>
        public const float AcaciaTrunkRadius = 0.35f;

        // ------------------------------------------------------------------
        // The reed hall
        // ------------------------------------------------------------------

        /// <summary>
        /// A <see cref="TownBuildingSpec.ReedHall"/> house (local frame, front −z): a red-clay platform, the barrel vault of mats
        /// over reed-bundle arches running from the facade to the back, the lattice front with the tall arched door, the two
        /// bundled columns with curled tops at the facade corners, the horns over the door and, over the deck, two mat awnings on
        /// reed poles (at <see cref="TownBuildingBuilder.PorchPostsX"/>, the colliders use the same list) that leave the door bay open.
        /// </summary>
        public static void ReedHallHouse(TownMeshBuilder b, TownBuildingSpec spec, float hw, float zFront, float zFacade, float zBack, float top)
        {
            float y0 = TownBuildingBuilder.FloorY;
            TownSwatch reed = spec.WallColor;
            TownSwatch mat = spec.RoofColor;
            b.BoxMinMax(new Vector3(-hw, 0f, zFacade), new Vector3(hw, y0, zBack), spec.PlinthColor);

            // The vault profile: a pointed barrel from one side of the platform to the other.
            float crown = top + 1.2f;
            const int steps = 8;
            var profile = new Vector3[steps + 1];
            for (int i = 0; i <= steps; i++)
            {
                float a = Mathf.PI * i / steps;
                profile[i] = new Vector3(-Mathf.Cos(a) * (hw - 0.05f), y0 + (crown - y0) * Mathf.Pow(Mathf.Max(0f, Mathf.Sin(a)), 0.7f), 0f);
            }

            // The mats over the vault, in bands of mat and bare reed.
            for (int i = 0; i < steps; i++)
            {
                Vector3 p0 = profile[i];
                Vector3 p1 = profile[i + 1];
                b.TwoSidedQuad(new Vector3(p0.x, p0.y, zFacade), new Vector3(p1.x, p1.y, zFacade), new Vector3(p1.x, p1.y, zBack), new Vector3(p0.x, p0.y, zBack),
                    (i % 3) == 1 ? reed : mat);
            }

            // The arches of bound reed, every ~0.9 m along the hall, standing proud of the mats.
            int arches = Mathf.Max(2, Mathf.RoundToInt((zBack - zFacade) / 0.9f));
            for (int k = 0; k <= arches; k++)
            {
                float z = Mathf.Lerp(zFacade + 0.08f, zBack - 0.08f, k / (float)arches);
                for (int i = 0; i < steps; i++)
                {
                    b.Log(Lift(profile[i], y0, z), Lift(profile[i + 1], y0, z), 0.1f, reed, 5);
                }
            }

            // The back end (mat) and the front end (reed) closing the vault.
            var backCenter = new Vector3(0f, y0, zBack);
            var frontCenter = new Vector3(0f, y0, zFacade);
            for (int i = 0; i < steps; i++)
            {
                b.TriangleFacing(backCenter, new Vector3(profile[i].x, profile[i].y, zBack), new Vector3(profile[i + 1].x, profile[i + 1].y, zBack), Vector3.forward, mat);
                b.TriangleFacing(frontCenter, new Vector3(profile[i].x, profile[i].y, zFacade), new Vector3(profile[i + 1].x, profile[i + 1].y, zFacade), Vector3.back, reed);
            }

            // The lattice front: upright reeds up to the vault and three binding bands.
            float doorX = spec.DoorOffset * spec.Width;
            float doorW = spec.WideDoor ? 1.6f : 1.15f;
            float doorH = 1.95f;
            float zf = zFacade - 0.03f;
            for (float x = -hw + 0.3f; x < hw - 0.2f; x += 0.34f)
            {
                if (Mathf.Abs(x - doorX) < doorW * 0.5f + 0.12f)
                {
                    continue;
                }

                float yTop = VaultHeightAt(x, hw, y0, crown) - 0.12f;
                b.Log(new Vector3(x, y0, zf), new Vector3(x, yTop, zf), 0.028f, mat, 4);
            }

            foreach (float h in new[] { 0.8f, 1.6f, 2.4f })
            {
                float y = y0 + h;
                float xm = VaultHalfWidthAt(y, hw, y0, crown) - 0.1f;
                if (xm > 0.3f)
                {
                    b.Log(new Vector3(-xm, y, zf - 0.03f), new Vector3(xm, y, zf - 0.03f), 0.035f, spec.TrimColor, 4);
                }
            }

            // The tall arched door: dark opening, reed jambs and a pointed reed head.
            b.With(new Vector3(0f, 0f, zFacade - 0.06f), 0f, () =>
            {
                b.BoxMinMax(new Vector3(doorX - doorW * 0.5f, y0, -0.01f), new Vector3(doorX + doorW * 0.5f, y0 + doorH, 0f), TownSwatch.Black);
                ArchFill(b, doorX, y0 + doorH, doorW, 0.75f, -0.01f, TownSwatch.Black);
            });

            float jz = zFacade - 0.1f;
            foreach (float side in new[] { -1f, 1f })
            {
                var foot = new Vector3(doorX + side * (doorW * 0.5f + 0.08f), y0, jz);
                var spring = foot + Vector3.up * doorH;
                var apex = new Vector3(doorX, y0 + doorH + 0.82f, jz);
                b.Log(foot, spring, 0.09f, reed, 5);
                b.Log(spring, Vector3.Lerp(spring, apex, 0.55f) + Vector3.up * 0.15f, 0.08f, reed, 5);
                b.Log(Vector3.Lerp(spring, apex, 0.55f) + Vector3.up * 0.15f, apex, 0.08f, reed, 5);
            }

            // The horns of the house over the door.
            Horns(b, new Vector3(doorX, y0 + doorH + 1.05f, jz - 0.05f), 0.9f, TownSwatch.Sail);

            // The two great bundled columns at the facade corners, curled at the top, bound with ribbon.
            foreach (float side in new[] { -1f, 1f })
            {
                var foot = new Vector3(side * (hw - 0.22f), 0f, zFacade - 0.22f);
                float height = crown + 0.6f;
                b.Cylinder(foot, 0.3f, 0.2f, height, 7, reed, reed);
                foreach (float h in new[] { 0.9f, 1.9f, height - 0.5f })
                {
                    b.Cylinder(foot + Vector3.up * h, 0.31f - h * 0.02f, 0.31f - h * 0.02f, 0.12f, 7, (h < 1.5f) ? spec.AccentColor : spec.TrimColor, spec.AccentColor);
                }

                b.Blob(foot + new Vector3(-side * 0.18f, height + 0.12f, 0f), new Vector3(0.42f, 0.32f, 0.3f), reed);
            }

            if (spec.PorchDepth > 0.01f)
            {
                float deck = CitySolids.DeckHeight;
                b.BoxMinMax(new Vector3(-hw, 0f, zFront), new Vector3(hw, deck, zFacade), spec.PlinthColor, TownSwatch.ReedMat);
                float z = zFront + TownBuildingBuilder.PostInset;
                float low = Mathf.Min(y0 + 2.35f, top);
                float high = low + 0.45f;
                foreach (float x in TownBuildingBuilder.PorchPostsX(spec))
                {
                    b.Log(new Vector3(x, deck, z), new Vector3(x, low + 0.1f, z), 0.07f, reed, 5);
                    b.Box(new Vector3(x + 0.1f, low - 0.35f, z - 0.05f), new Vector3(0.14f, 0.4f, 0.02f), spec.AccentColor, 0f);
                }

                // Two mat awnings, one each side of the door bay.
                float gap0 = doorX - doorW * 0.5f - 0.25f;
                float gap1 = doorX + doorW * 0.5f + 0.25f;
                foreach ((float x0, float x1) in new[] { (-hw - 0.1f, gap0), (gap1, hw + 0.1f) })
                {
                    if (x1 - x0 < 0.4f)
                    {
                        continue;
                    }

                    b.TwoSidedQuad(new Vector3(x0, high, zFacade), new Vector3(x1, high, zFacade), new Vector3(x1, low, z - 0.15f), new Vector3(x0, low, z - 0.15f), TownSwatch.ReedMat);
                    b.Log(new Vector3(x0, low, z - 0.15f), new Vector3(x1, low, z - 0.15f), 0.035f, reed, 4);
                }
            }
        }

        /// <summary>A point of the vault profile pushed a little out of the mats (the arch ribs stand proud).</summary>
        private static Vector3 Lift(Vector3 p, float y0, float z)
        {
            Vector3 n = new Vector3(p.x, p.y - y0, 0f).normalized;
            return new Vector3(p.x, p.y, z) + n * 0.06f;
        }

        /// <summary>Height of the reed vault over local x (0 past the springing).</summary>
        private static float VaultHeightAt(float x, float hw, float y0, float crown)
        {
            float u = Mathf.Clamp01(Mathf.Abs(x) / Mathf.Max(0.01f, hw - 0.05f));
            return y0 + (crown - y0) * Mathf.Pow(Mathf.Sqrt(1f - u * u), 0.7f);
        }

        /// <summary>Half width of the reed vault at height y.</summary>
        private static float VaultHalfWidthAt(float y, float hw, float y0, float crown)
        {
            float v = Mathf.Clamp01((y - y0) / Mathf.Max(0.01f, crown - y0));
            float sin = Mathf.Pow(v, 1f / 0.7f);
            return (hw - 0.05f) * Mathf.Sqrt(Mathf.Max(0f, 1f - sin * sin));
        }

        /// <summary>A pair of aurochs horns on a clay boss (face frame: −z outwards), curving out and up; dark tips.</summary>
        public static void Horns(TownMeshBuilder b, Vector3 at, float scale, TownSwatch horn)
        {
            float s = scale;
            b.Blob(at, new Vector3(0.36f, 0.26f, 0.2f) * s, TownSwatch.RedClay);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector3 p0 = at + new Vector3(side * 0.14f, 0.04f, 0f) * s;
                Vector3 p1 = at + new Vector3(side * 0.5f, 0.12f, -0.04f) * s;
                Vector3 p2 = at + new Vector3(side * 0.74f, 0.4f, -0.06f) * s;
                Vector3 p3 = at + new Vector3(side * 0.7f, 0.66f, -0.04f) * s;
                b.Log(p0, p1, 0.08f * s, horn, 5);
                b.Log(p1, p2, 0.06f * s, horn, 5);
                b.Log(p2, p3, 0.04f * s, TownSwatch.WoodDark, 4);
            }
        }

        /// <summary>The reed gate of Aratta: two bundled columns with curled tops and horns, a cord of blue ribbons across the road.</summary>
        public static void ReedGate(TownMeshBuilder b, Vector3 a, Vector3 c)
        {
            Vector3 along = (c - a).normalized;
            Vector3 n = Vector3.Cross(along, Vector3.up).normalized;
            foreach (Vector3 p in new[] { a, c })
            {
                b.Cylinder(p, 0.3f, 0.2f, 4.2f, 7, TownSwatch.Reed, TownSwatch.Reed);
                foreach (float h in new[] { 1.0f, 2.2f, 3.6f })
                {
                    b.Cylinder(p + Vector3.up * h, 0.3f, 0.3f, 0.12f, 7, (h < 3f) ? TownSwatch.Ribbon : TownSwatch.RedClay, TownSwatch.Ribbon);
                }

                Vector3 inward = p == a ? along : -along;
                b.Blob(p + Vector3.up * 4.3f + inward * 0.18f, new Vector3(0.42f, 0.32f, 0.42f), TownSwatch.Reed);
                b.With(p + Vector3.up * 3.2f - n * 0.3f, 0f, () => Horns(b, Vector3.zero, 0.8f, TownSwatch.Sail));
            }

            b.Log(a + Vector3.up * 3.4f, c + Vector3.up * 3.4f, 0.03f, TownSwatch.Thatch, 4);
            float length = Vector3.Distance(a, c);
            int flags = Mathf.Max(3, Mathf.RoundToInt(length / 0.55f));
            for (int i = 1; i < flags; i++)
            {
                Vector3 p = a + along * (length * i / flags) + Vector3.up * 3.4f;
                TownSwatch cloth = (i % 2) == 0 ? TownSwatch.Ribbon : TownSwatch.GoldGrass;
                b.TwoSidedQuad(p - along * 0.12f, p + along * 0.12f, p + along * 0.05f - Vector3.up * 0.55f, p - along * 0.05f - Vector3.up * 0.55f, cloth);
            }
        }

        // ------------------------------------------------------------------
        // Props
        // ------------------------------------------------------------------

        /// <summary>A horn post: a reed-bound pole on a clay foot, a crossbar with a pair of horns and blue ribbons hanging (variant: ribbon colours).</summary>
        private static void HornPost(TownMeshBuilder b, float s, int variant)
        {
            float h = 2.8f * s;
            b.Cylinder(Vector3.zero, HornPostRadius * s, HornPostRadius * 0.8f * s, 0.3f, 7, TownSwatch.RedClay, TownSwatch.RedClay);
            b.Log(new Vector3(0f, 0.3f, 0f), new Vector3(0f, h, 0f), 0.09f * s, TownSwatch.Reed, 6);
            for (int i = 0; i < 4; i++)
            {
                b.Cylinder(new Vector3(0f, 0.6f * s + i * 0.55f * s, 0f), 0.11f * s, 0.11f * s, 0.06f, 6, TownSwatch.ReedMat, TownSwatch.ReedMat);
            }

            b.Log(new Vector3(-0.35f * s, h - 0.35f * s, 0f), new Vector3(0.35f * s, h - 0.35f * s, 0f), 0.05f * s, TownSwatch.WoodDark, 4);
            Horns(b, new Vector3(0f, h - 0.1f * s, -0.04f), 0.75f * s, TownSwatch.Sail);
            TownSwatch second = (variant % 3) == 0 ? TownSwatch.GoldGrass : (variant % 3) == 1 ? TownSwatch.ClothRed : TownSwatch.ClothWhite;
            for (int i = 0; i < 4; i++)
            {
                float x = -0.3f * s + i * 0.2f * s;
                float len = (0.7f + (i % 2) * 0.25f) * s;
                TownSwatch cloth = (i % 2) == 0 ? TownSwatch.Ribbon : second;
                Vector3 top = new Vector3(x, h - 0.38f * s, -0.06f);
                b.TwoSidedQuad(top + new Vector3(-0.05f, 0f, 0f), top + new Vector3(0.05f, 0f, 0f), top + new Vector3(0.08f, -len, -0.05f), top + new Vector3(-0.02f, -len, -0.05f), cloth);
            }
        }

        /// <summary>A round enclosure of dry stone (open to the front, −z): stacked blocks with a capstone course, grass and a hay rack inside.</summary>
        private static void StoneKraal(TownMeshBuilder b, float s)
        {
            float r = KraalRadius * s;
            b.Disc(new Vector3(0f, 0.03f, 0f), r - 0.3f, 14, TownSwatch.Thatch);
            const int blocks = 18;
            float chord = 2f * Mathf.PI * r / blocks;
            for (int i = 0; i < blocks; i++)
            {
                float deg = i * 360f / blocks;
                if (Mathf.Abs(Mathf.DeltaAngle(deg, 270f)) < 22f)
                {
                    continue;
                }

                float a = deg * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                float yaw = -deg + 90f;
                b.Box(dir * r + Vector3.up * 0.25f, new Vector3(chord + 0.06f, 0.5f, 0.5f), (i % 2) == 0 ? TownSwatch.Stone : TownSwatch.StoneDark, yaw);
                b.Box(dir * r + Vector3.up * 0.68f, new Vector3(chord * 0.92f, 0.36f, 0.42f), (i % 2) == 0 ? TownSwatch.StoneDark : TownSwatch.Stone, yaw + 4f);
                b.Box(dir * r + Vector3.up * 0.93f, new Vector3(chord * 0.85f, 0.14f, 0.5f), TownSwatch.Stone, yaw);
            }

            // A hay rack of reed and a water trough.
            var rack = new Vector3(0.4f * s, 0f, 0.8f * s);
            foreach (float x in new[] { -0.6f, 0.6f })
            {
                b.Log(rack + new Vector3(x, 0f, 0f), rack + new Vector3(x, 1.1f, 0f), 0.05f, TownSwatch.Reed, 4);
            }

            b.Box(rack + Vector3.up * 0.75f, new Vector3(1.3f, 0.5f, 0.5f), TownSwatch.GoldGrass, 0f, TownSwatch.Thatch);
            b.Box(new Vector3(-0.8f * s, 0.2f, 0.2f * s), new Vector3(1.0f, 0.4f, 0.45f), TownSwatch.RedClay, 30f, TownSwatch.Water);
        }

        /// <summary>
        /// The Lira do Touro on its clay plinth (front −z): a great lyre with a sound box of dark wood and old gold, two arms
        /// and a yoke, the strings, and on the front of the box the bull's head of gold with its beard of lapis blue.
        /// </summary>
        private static void BullLyre(TownMeshBuilder b, float s)
        {
            float hx = BullLyreSize.x * 0.5f * s;
            float hz = BullLyreSize.y * 0.5f * s;
            b.BoxMinMax(new Vector3(-hx, 0f, -hz), new Vector3(hx, 0.45f, hz), TownSwatch.RedClay, TownSwatch.ReedMat);
            var box0 = new Vector3(-0.75f * s, 0.45f, -0.18f * s);
            var box1 = new Vector3(0.75f * s, 1.25f * s, 0.18f * s);
            b.BoxMinMax(box0, box1, TownSwatch.WoodDark, TownSwatch.Gold);
            b.BoxMinMax(new Vector3(-0.75f * s, 0.45f, -0.2f * s), new Vector3(0.75f * s, 0.55f, -0.18f * s), TownSwatch.Gold);
            foreach (float side in new[] { -1f, 1f })
            {
                b.Log(new Vector3(side * 0.6f * s, 1.2f * s, 0f), new Vector3(side * 0.72f * s, 2.9f * s, 0f), 0.07f * s, TownSwatch.WoodDark, 5);
            }

            b.Log(new Vector3(-0.95f * s, 2.85f * s, 0f), new Vector3(0.95f * s, 2.95f * s, 0f), 0.08f * s, TownSwatch.Gold, 6);
            for (int i = 0; i < 9; i++)
            {
                float x = -0.45f * s + i * 0.112f * s;
                b.Log(new Vector3(x, 1.25f * s, -0.02f), new Vector3(x * 1.1f, 2.85f * s, -0.02f), 0.008f, TownSwatch.Sail, 3);
            }

            // The bull's head on the front of the sound box: gold, lapis beard, horns.
            var head = new Vector3(-0.42f * s, 1.15f * s, -0.32f * s);
            b.Blob(head, new Vector3(0.38f, 0.44f, 0.3f) * s, TownSwatch.Gold);
            b.Blob(head + new Vector3(0f, -0.14f, -0.1f) * s, new Vector3(0.24f, 0.2f, 0.18f) * s, TownSwatch.Gold);
            b.Blob(head + new Vector3(0f, -0.4f, -0.05f) * s, new Vector3(0.2f, 0.34f, 0.12f) * s, TownSwatch.Indigo);
            b.Box(head + new Vector3(-0.08f, 0.08f, -0.16f) * s, new Vector3(0.05f, 0.05f, 0.02f) * s, TownSwatch.Indigo, 0f);
            b.Box(head + new Vector3(0.08f, 0.08f, -0.16f) * s, new Vector3(0.05f, 0.05f, 0.02f) * s, TownSwatch.Indigo, 0f);
            Horns(b, head + new Vector3(0f, 0.2f, 0f) * s, 0.55f * s, TownSwatch.Gold);
        }

        /// <summary>A flat-topped savanna acacia: a leaning forked trunk and a wide, thin umbrella crown.</summary>
        private static void Acacia(TownMeshBuilder b, float s)
        {
            var fork = new Vector3(0.3f * s, 2.4f * s, 0.1f * s);
            b.Log(Vector3.zero, fork, AcaciaTrunkRadius * 0.6f * s, TownSwatch.Trunk, 6);
            foreach (Vector3 tip in new[] { new Vector3(-1.4f, 3.5f, 0.3f), new Vector3(1.6f, 3.6f, -0.2f), new Vector3(0.4f, 3.8f, 1.2f) })
            {
                b.Log(fork, tip * s, 0.1f * s, TownSwatch.Trunk, 5);
            }

            b.Crown(new Vector3(0.2f, 4.0f, 0.3f) * s, new Vector3(4.4f, 0.7f, 3.6f) * s, TownSwatch.LeafDark, 15f);
            b.Crown(new Vector3(-0.6f, 4.25f, -0.2f) * s, new Vector3(2.4f, 0.45f, 2.0f) * s, TownSwatch.GrassDark, 50f);
        }

        // ------------------------------------------------------------------
        // Dressing
        // ------------------------------------------------------------------

        /// <summary>Lugalbanda's front: a reed mat with rolled maps, a coil of rope, his walking staff and a small relic of the Artífices.</summary>
        private static void ReedHornDressing(TownMeshBuilder b, TownBuildingSpec spec, float x, float y, float z, float zFacade)
        {
            b.BoxMinMax(new Vector3(x - 0.75f, y, z - 0.75f), new Vector3(x + 0.75f, y + 0.04f, z + 0.1f), TownSwatch.ReedMat);
            for (int i = 0; i < 3; i++)
            {
                b.Log(new Vector3(x - 0.55f, y + 0.1f + i * 0.02f, z - 0.55f + i * 0.17f), new Vector3(x - 0.05f, y + 0.1f + i * 0.02f, z - 0.5f + i * 0.17f), 0.07f,
                    i == 1 ? TownSwatch.Thatch : TownSwatch.Sail, 6, TownSwatch.Ribbon);
            }

            b.Cylinder(new Vector3(x + 0.35f, y + 0.04f, z - 0.4f), 0.22f, 0.2f, 0.16f, 8, TownSwatch.Thatch, TownSwatch.ReedMat);
            b.Box(new Vector3(x + 0.35f, y + 0.18f, z - 0.05f), new Vector3(0.24f, 0.2f, 0.2f), TownSwatch.Gold, 20f, TownSwatch.Turquoise);
            b.Log(new Vector3(x + 0.85f, y, z - 0.2f), new Vector3(x + 0.95f, y + 1.7f, z + 0.05f), 0.035f, TownSwatch.WoodDark, 5);
            b.Box(new Vector3(x + 0.95f, y + 1.55f, z + 0.02f), new Vector3(0.05f, 0.3f, 0.02f), TownSwatch.Ribbon, 0f);
        }
    }
}
