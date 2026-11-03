using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>
    /// Região 4 — Grutas do Limiar: the cave pieces of the overworld (Docs/Mapas100.md 19–24). The light of the caves is the
    /// palette's emissive cells (glowing moss and mushrooms, the blue crystals, the lamps): no real lights. Each piece is
    /// built around the origin (placed by the chunk builder) except the runs (tuff walls, the abyss), which take world points.
    /// Kept cheap: a stalagmite ~30 triangles, a crystal cluster ~40, a wall piece ~40, the biggest monument ~400.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        /// <summary>Thickness (m) of a tuff wall piece of seed <paramref name="seed"/> (the collider uses the same).</summary>
        public static float CaveWallThickness(int seed) => 2.6f + 1.4f * TownStyle.Hash01("tuffwall", seed);

        /// <summary>
        /// A piece of the tall tuff walls between the halls, from ground points <paramref name="a"/> to <paramref name="c"/> (same
        /// height: the lower ground): a main block, a narrower stepped upper tier and a fallen boulder at its foot.
        /// </summary>
        public static void CaveTuffWall(TownMeshBuilder b, Vector3 a, Vector3 c, float height, int seed, TownSwatch rock)
        {
            Vector3 d = c - a;
            d.y = 0f;
            if (d.sqrMagnitude < 1e-3f)
            {
                return;
            }

            float h01 = TownStyle.Hash01("tuffwall", seed + 7);
            float thick = CaveWallThickness(seed);
            Vector3 n = new Vector3(-d.z, 0f, d.x).normalized;
            TownSwatch top = rock == TownSwatch.PinkTuff ? TownSwatch.TaipaRose : TownSwatch.StoneDark;
            Run(b, a, c, thick, -0.8f, height, rock, top);
            Vector3 shift = n * (thick * 0.18f * (h01 < 0.5f ? 1f : -1f));
            Run(b, Vector3.Lerp(a, c, 0.12f) + shift, Vector3.Lerp(a, c, 0.82f) + shift, thick * 0.62f, height - 0.1f, height + 0.9f + 1.8f * h01,
                rock, top);
            if ((seed & 1) == 0)
            {
                Vector3 foot = Vector3.Lerp(a, c, 0.3f + 0.4f * h01) + n * (thick * 0.55f * (h01 < 0.5f ? -1f : 1f));
                b.Crown(foot + Vector3.up * 0.35f, new Vector3(2.2f, 1.4f, 1.8f), TownSwatch.Basalt, seed * 37f);
            }

            if (seed % 5 == 0)
            {
                // Fluorescent moss hanging on the face.
                Vector3 mid = (a + c) * 0.5f + n * (thick * 0.5f + 0.02f) + Vector3.up * (height * 0.55f);
                b.QuadFacing(mid + d.normalized * 0.9f + Vector3.up * 0.8f, mid - d.normalized * 0.9f + Vector3.up * 0.8f,
                    mid - d.normalized * 0.6f - Vector3.up * 1.2f, mid + d.normalized * 0.6f - Vector3.up * 1.2f, n, TownSwatch.Moss);
            }
        }

        /// <summary>The black void of a chasm over its carved bed, from <paramref name="a"/> to <paramref name="c"/> (world XZ), half width <paramref name="half"/>.</summary>
        public static void CaveAbyss(TownMeshBuilder b, Vector2 a, Vector2 c, float half, float y)
        {
            Vector2 d = c - a;
            if (d.sqrMagnitude < 1e-4f)
            {
                return;
            }

            d.Normalize();
            Vector2 n = new Vector2(-d.y, d.x);
            Vector2 o = n * half;
            Vector2 e = d * Mathf.Min(1.5f, half * 0.3f);
            b.QuadFacing(new Vector3(a.x + o.x - e.x, y, a.y + o.y - e.y), new Vector3(c.x + o.x + e.x, y, c.y + o.y + e.y),
                new Vector3(c.x - o.x + e.x, y, c.y - o.y + e.y), new Vector3(a.x - o.x - e.x, y, a.y - o.y - e.y), Vector3.up, TownSwatch.Black);
            // Two dark inner lips: the walls of the chasm going down out of sight.
            Vector2 lip = n * (half * 0.78f);
            foreach (float side in new[] { -1f, 1f })
            {
                Vector2 p = lip * side;
                b.QuadFacing(new Vector3(a.x + o.x * side - e.x, y + 0.01f, a.y + o.y * side - e.y), new Vector3(c.x + o.x * side + e.x, y + 0.01f, c.y + o.y * side + e.y),
                    new Vector3(c.x + p.x + e.x, y + 0.01f, c.y + p.y + e.y), new Vector3(a.x + p.x - e.x, y + 0.01f, a.y + p.y - e.y), Vector3.up, TownSwatch.Indigo);
            }
        }

        /// <summary>A stalagmite with a smaller one beside it and a fluorescent moss skirt.</summary>
        public static void CaveStalagmite(TownMeshBuilder b, float s, int variant, TownSwatch rock)
        {
            float h = (2.4f + 0.9f * (variant & 3)) * s;
            b.Cylinder(Vector3.zero, 0.75f * s, 0f, h, 6, rock);
            b.Cylinder(new Vector3(0.75f, 0f, 0.35f) * s, 0.42f * s, 0f, h * 0.45f, 5, rock);
            b.Cylinder(Vector3.zero, 0.95f * s, 0.72f * s, 0.18f * s, 6, (variant & 1) == 0 ? TownSwatch.Moss : TownSwatch.StoneDark);
        }

        /// <summary>A cluster of glowing crystals on a dark boulder (blue; violet in variant 1).</summary>
        public static void CaveCrystal(TownMeshBuilder b, float s, int variant)
        {
            TownSwatch glow = TownSwatch.MushroomBlue;
            TownSwatch other = (variant & 1) == 1 ? TownSwatch.Violet : TownSwatch.Glass;
            b.Crown(new Vector3(0f, 0.15f, 0f) * s, new Vector3(1.6f, 0.6f, 1.4f) * s, TownSwatch.Indigo, variant * 31f);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 1.047f + variant * 0.4f;
                float lean = 0.35f + 0.25f * (i % 3);
                var foot = new Vector3(Mathf.Cos(a) * 0.35f, 0.2f, Mathf.Sin(a) * 0.35f) * s;
                float len = (1.1f + 0.5f * ((i * 7 + variant) % 4)) * s;
                Vector3 tip = foot + new Vector3(Mathf.Cos(a) * lean, 1f, Mathf.Sin(a) * lean).normalized * len;
                Spike(b, foot, tip, (0.16f + 0.05f * (i & 1)) * s, (i & 1) == 0 ? glow : other);
            }

            Spike(b, new Vector3(0f, 0.2f, 0f), new Vector3(0.05f, 2.4f, 0f) * s, 0.24f * s, glow);
        }

        /// <summary>A natural stone column: a stalagmite and a stalactite grown together, flaring out where the camera cuts it.</summary>
        public static void CaveColumn(TownMeshBuilder b, float s, TownSwatch rock)
        {
            b.Cylinder(Vector3.zero, 1.35f * s, 0.75f * s, 3.2f * s, 7, rock);
            b.Cylinder(new Vector3(0f, 3.2f, 0f) * s, 0.75f * s, 0.62f * s, 2.2f * s, 7, rock);
            b.Cylinder(new Vector3(0f, 5.4f, 0f) * s, 0.62f * s, 1.45f * s, 2.6f * s, 7, rock, TownSwatch.StoneDark);
            b.Cylinder(Vector3.zero, 1.55f * s, 1.3f * s, 0.2f * s, 7, TownSwatch.Moss);
        }

        /// <summary>A cluster of three giant glowing mushrooms.</summary>
        public static void CaveMushrooms(TownMeshBuilder b, float s, int variant)
        {
            for (int i = 0; i < 3; i++)
            {
                float a = i * 2.2f + variant;
                var foot = i == 0 ? Vector3.zero : new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a)) * (1.5f * s);
                float k = i == 0 ? 1f : 0.55f + 0.15f * i;
                float h = 3.6f * s * k;
                float r = 1.7f * s * k;
                b.Cylinder(foot, 0.3f * s * k, 0.2f * s * k, h, 5, TownSwatch.LimeWhite);
                Vector3 cap = foot + Vector3.up * h;
                b.Cylinder(cap, r, 0.25f * r, 0.75f * s * k, 7, i == 1 && (variant & 1) == 1 ? TownSwatch.Violet : TownSwatch.MushroomBlue, TownSwatch.MushroomBlue);
                b.Disc(cap + Vector3.up * 0.01f, r, 7, TownSwatch.Indigo);
            }
        }

        /// <summary>A kobold mine cart on a short piece of rail (22).</summary>
        public static void CaveMineCart(TownMeshBuilder b, float s)
        {
            foreach (float x in new[] { -0.5f, 0.5f })
            {
                b.Box(new Vector3(x, 0.05f, 0f), new Vector3(0.08f, 0.1f, 4f), TownSwatch.Iron);
            }

            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0f, 0.03f, i * 1.3f), new Vector3(1.4f, 0.06f, 0.25f), TownSwatch.WoodDark);
            }

            b.Box(new Vector3(0f, 0.75f, 0f) * s, new Vector3(1.2f, 0.8f, 1.8f) * s, TownSwatch.Copper, 0f, TownSwatch.Black);
            b.Crown(new Vector3(0f, 1.2f, 0.1f) * s, new Vector3(0.9f, 0.5f, 1.2f) * s, TownSwatch.Indigo, 20f);
            foreach (float z in new[] { -0.6f, 0.6f })
            {
                b.Log(new Vector3(-0.62f, 0.25f, z) * s, new Vector3(0.62f, 0.25f, z) * s, 0.22f * s, TownSwatch.Iron, 6);
            }
        }

        /// <summary>A copper air pipe on two posts (the market's tunnels breathe through them).</summary>
        public static void CaveAirPipe(TownMeshBuilder b, float s)
        {
            foreach (float z in new[] { -2f, 2f })
            {
                b.Cylinder(new Vector3(0f, 0f, z), 0.12f, 0.1f, 1.9f * s, 4, TownSwatch.WoodDark);
            }

            b.Log(new Vector3(0f, 1.8f * s, -2.6f), new Vector3(0f, 1.8f * s, 2.6f), 0.28f, TownSwatch.Copper, 6, TownSwatch.Black);
            b.Log(new Vector3(0f, 1.8f * s, 2.6f), new Vector3(0f, 0.2f, 3.1f), 0.24f, TownSwatch.Copper, 6, TownSwatch.Black);
        }

        /// <summary>A bronze jar of the wheel's aqueducts (23), some cracked and lying.</summary>
        public static void CaveJar(TownMeshBuilder b, float s, int variant)
        {
            if ((variant & 3) == 3)
            {
                b.Log(new Vector3(-0.7f, 0.5f, 0f) * s, new Vector3(0.7f, 0.5f, 0f) * s, 0.5f * s, TownSwatch.Copper, 7, TownSwatch.Black);
                return;
            }

            b.Cylinder(Vector3.zero, 0.4f * s, 0.62f * s, 0.7f * s, 7, TownSwatch.Copper);
            b.Cylinder(new Vector3(0f, 0.7f, 0f) * s, 0.62f * s, 0.25f * s, 0.6f * s, 7, TownSwatch.Copper);
            b.Cylinder(new Vector3(0f, 1.3f, 0f) * s, 0.25f * s, 0.3f * s, 0.2f * s, 7, TownSwatch.Gold, TownSwatch.Water);
        }

        /// <summary>A nest of Mahaway's flock: a ring of sticks and bones, two pale eggs (24).</summary>
        public static void CaveNest(TownMeshBuilder b, float s)
        {
            b.Ring(new Vector3(0f, 0.35f * s, 0f), 0.9f * s, 1.6f * s, 8, TownSwatch.WoodWeathered, TownSwatch.Trunk);
            b.Cylinder(Vector3.zero, 1.7f * s, 1.6f * s, 0.35f * s, 8, TownSwatch.WoodWeathered);
            for (int i = 0; i < 5; i++)
            {
                float a = i * 1.26f;
                var p = new Vector3(Mathf.Cos(a) * 1.25f, 0.4f, Mathf.Sin(a) * 1.25f) * s;
                b.Log(p, p + new Vector3(Mathf.Sin(a) * 1.1f, 0.25f, -Mathf.Cos(a) * 1.1f) * s, 0.06f * s, (i & 1) == 0 ? TownSwatch.LimeWhite : TownSwatch.Trunk, 4);
            }

            b.Blob(new Vector3(-0.3f, 0.6f, 0.1f) * s, new Vector3(0.45f, 0.6f, 0.45f) * s, TownSwatch.LimeWhite);
            b.Blob(new Vector3(0.3f, 0.55f, -0.15f) * s, new Vector3(0.4f, 0.55f, 0.4f) * s, TownSwatch.Plaster);
        }

        /// <summary>The posts and hand ropes at one end of a rope bridge (<paramref name="half"/> = half the deck's width), the bridge running along local +z.</summary>
        public static void CaveRopePosts(TownMeshBuilder b, float half)
        {
            foreach (float x in new[] { -half - 0.25f, half + 0.25f })
            {
                b.Cylinder(new Vector3(x, 0f, 0f), 0.14f, 0.11f, 1.6f, 5, TownSwatch.WoodDark);
                b.Log(new Vector3(x, 1.45f, 0f), new Vector3(x, 0.9f, 2.6f), 0.035f, TownSwatch.Thatch, 4);
                b.Log(new Vector3(x, 0.9f, 0f), new Vector3(x - Mathf.Sign(x) * 0.4f, 0f, -0.7f), 0.035f, TownSwatch.Thatch, 4);
            }

            b.Blob(new Vector3(half + 0.25f, 1.75f, 0f), new Vector3(0.22f, 0.3f, 0.22f), TownSwatch.LampGlow);
        }

        /// <summary>A lamp niche of the Lamparinas: a tuff post with a lit oil lamp in its hollow (the lamps along the trails).</summary>
        public static void CaveLampNiche(TownMeshBuilder b, float s)
        {
            b.Box(new Vector3(0f, 0.9f, 0f) * s, new Vector3(0.7f, 1.8f, 0.6f) * s, TownSwatch.PinkTuff, 0f, TownSwatch.TaipaRose);
            b.Box(new Vector3(0f, 1.15f, -0.31f) * s, new Vector3(0.42f, 0.5f, 0.04f) * s, TownSwatch.Black);
            b.Blob(new Vector3(0f, 1.1f, -0.36f) * s, new Vector3(0.22f, 0.28f, 0.16f) * s, TownSwatch.LampGlow);
            b.Box(new Vector3(0f, 1.85f, 0f) * s, new Vector3(0.9f, 0.12f, 0.75f) * s, TownSwatch.Basalt);
        }

        /// <summary>The wheel of twelve jars (23): a great wheel on two tuff piers, its rim hung with bronze jars, over its pool.</summary>
        public static void CaveJarWheel(TownMeshBuilder b)
        {
            const float r = 5.2f;
            const float axle = 6.2f;
            foreach (float x in new[] { -1.6f, 1.6f })
            {
                b.Box(new Vector3(x, axle * 0.5f, 0f), new Vector3(0.9f, axle, 1.6f), TownSwatch.PinkTuff, 0f, TownSwatch.TaipaRose);
            }

            b.Log(new Vector3(-2.1f, axle, 0f), new Vector3(2.1f, axle, 0f), 0.28f, TownSwatch.Copper, 6);
            for (int i = 0; i < 12; i++)
            {
                float a0 = i * Mathf.PI / 6f;
                float a1 = (i + 1) * Mathf.PI / 6f;
                var p0 = new Vector3(0f, axle + Mathf.Sin(a0) * r, Mathf.Cos(a0) * r);
                var p1 = new Vector3(0f, axle + Mathf.Sin(a1) * r, Mathf.Cos(a1) * r);
                b.Log(p0, p1, 0.16f, TownSwatch.WoodDark, 4);
                if ((i & 1) == 0)
                {
                    b.Log(new Vector3(0f, axle, 0f), p0, 0.1f, TownSwatch.WoodDark, 4);
                }

                Vector3 jar = p0 + new Vector3(0.55f, -0.5f, 0f);
                b.Cylinder(jar, 0.25f, 0.38f, 0.5f, 6, TownSwatch.Copper);
                b.Cylinder(jar + Vector3.up * 0.5f, 0.38f, 0.18f, 0.35f, 6, TownSwatch.Copper, (i % 3) == 0 ? TownSwatch.LampGlow : TownSwatch.Water);
            }
        }

        /// <summary>
        /// The barred portal of the Ossuário dos Artífices cut in a block of tuff: bone-white jambs and lintel carved with
        /// skulls, iron bars, two lamps (faces local −z).
        /// </summary>
        public static void CaveOssuaryPortal(TownMeshBuilder b)
        {
            b.Box(new Vector3(0f, 3.3f, 0.9f), new Vector3(8f, 6.6f, 1.6f), TownSwatch.Basalt, 0f, TownSwatch.StoneDark);
            foreach (float x in new[] { -2.1f, 2.1f })
            {
                b.Box(new Vector3(x, 2f, -0.1f), new Vector3(0.9f, 4f, 0.9f), TownSwatch.LimeWhite, 0f, TownSwatch.Plaster);
                b.Blob(new Vector3(x, 4.25f, -0.45f), new Vector3(0.5f, 0.55f, 0.45f), TownSwatch.LimeWhite);
                b.Blob(new Vector3(x, 3.1f, -0.65f), new Vector3(0.24f, 0.3f, 0.2f), TownSwatch.LampGlow);
            }

            b.Box(new Vector3(0f, 4.4f, -0.1f), new Vector3(5.1f, 0.8f, 1f), TownSwatch.LimeWhite, 0f, TownSwatch.Plaster);
            b.Box(new Vector3(0f, 2f, 0.05f), new Vector3(3.3f, 4f, 0.1f), TownSwatch.Black);
            for (int i = 0; i < 6; i++)
            {
                float x = -1.4f + i * 0.56f;
                b.Box(new Vector3(x, 2f, -0.05f), new Vector3(0.08f, 4f, 0.08f), TownSwatch.Iron);
            }

            b.Box(new Vector3(0f, 2.2f, -0.05f), new Vector3(3.2f, 0.1f, 0.08f), TownSwatch.Iron);
        }

        /// <summary>A vishapakar (21): the town kit's dragon stone with the fish head; variant 1 carries a lamp (the Vishapakar Grande).</summary>
        public static void CaveVishapakar(TownMeshBuilder b, float s, int variant)
        {
            TownKit.Prop(b, new CityProp { Kind = CityPropKind.Vishapakar, Scale = s }, TownStyles.For(BiomeType.Caves));
            if (variant == 1)
            {
                b.Blob(new Vector3(0f, 3.35f * s, 0f), new Vector3(0.35f, 0.45f, 0.35f) * s, TownSwatch.LampGlow);
            }
        }
    }
}
