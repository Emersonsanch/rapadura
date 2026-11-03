using Rapadura.Gameplay.World.Cities.Town;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Overworld
{
    /// <summary>The overworld's own dressing pieces (beside the town kit's trees, shrubs and rocks).</summary>
    public enum OverworldDecor
    {
        None = 0,

        // Ground detail, no colliders.
        /// <summary>A piece of crop row (<see cref="OverworldProp.End"/> = its other end; <see cref="OverworldProp.Variant"/> = <see cref="FieldCrop"/>).</summary>
        CropRow = 1,
        Tufts = 2,
        Flowers = 3,
        Reeds = 4,
        Pebbles = 5,
        Ferns = 6,
        Puddle = 7,

        // Solid pieces.
        /// <summary>A piece of hedgerow along a field lane (<see cref="OverworldProp.End"/> = its other end).</summary>
        Hedge = 20,
        Log = 21,
        Stump = 22,
        MossRock = 23,
        /// <summary>The Altos do Mensageiro: stacked-stone cairns.</summary>
        Cairn = 24,
        /// <summary>The Ravina: clusters of basalt columns.</summary>
        Basalt = 25,
        /// <summary>The Ruínas: broken columns and fallen drums.</summary>
        Column = 26,
        /// <summary>The Jardim: tall cypresses.</summary>
        Cypress = 27,
        /// <summary>The Brejo: dead trees.</summary>
        DeadTree = 28,

        // Região 2 — Areias Flamejantes (OverworldDecorKit.Desert.cs): 80–99.
        /// <summary>Bleached bones: a camel's rib cage and skull half in the sand.</summary>
        Bones = 80,
        /// <summary>A broken caravan cart with spilled crates and bales of silk.</summary>
        CaravanWreck = 81,
        /// <summary>A steep Meroë pyramid with its chapel (the town kit's ruin).</summary>
        Pyramid = 82,
        /// <summary>A broken lotus column and its fallen drum.</summary>
        LotusColumn = 83,
        /// <summary>A row of ushabtis on a plinth.</summary>
        Ushabtis = 84,
        /// <summary>A karez well shaft: spoil ring, timber frame, windlass.</summary>
        KarezShaft = 85,
        /// <summary>A stone chimney (hoodoo) of layered red sandstone with its cap rock.</summary>
        Hoodoo = 86,
        /// <summary>A natural sandstone arch.</summary>
        RockArch = 87,
        /// <summary>A cluster of red salt crystals (Namrael's).</summary>
        SaltCrystal = 88,
        /// <summary>A rammed-earth watchtower of Qocho's sentinels (half ruined).</summary>
        TaipaTower = 89,
        /// <summary>A flat polygon of salt crust (ground detail, no collider).</summary>
        SaltCrust = 90,
        /// <summary>A stone marker (stela) along the caravan ways.</summary>
        Marker = 91,
        /// <summary>A resting caravan: camels, bales, jars round a cold hearth.</summary>
        CaravanCamp = 92,
        /// <summary>A clan camp of the crowned orcs: ochre leather tents and a drum (11).</summary>
        ClanTent = 93,

        // Região 4 — Grutas do Limiar (OverworldDecorKit.Caves.cs): 60–79, all solid.
        /// <summary>A run of tall tuff wall (<see cref="OverworldProp.End"/> = its other end): the cave's walls between halls.</summary>
        TuffWall = 60,
        Stalagmite = 61,
        /// <summary>A cluster of glowing crystals.</summary>
        Crystal = 62,
        /// <summary>A natural stone column, floor to "ceiling" (cut off high above the camera).</summary>
        CaveColumn = 63,
        /// <summary>A giant glowing mushroom cluster.</summary>
        GiantMushroom = 64,
        /// <summary>A vishapakar, the dragon stone with a fish head (21).</summary>
        Vishapakar = 65,
        /// <summary>A mine cart on a piece of rail (22).</summary>
        MineCart = 66,
        /// <summary>A copper air pipe on posts (22).</summary>
        AirPipe = 67,
        /// <summary>A bronze jar of the wheel's aqueducts (23).</summary>
        Jar = 68,
        /// <summary>Mahaway's flock: a nest of sticks and bones (24).</summary>
        Nest = 69,
        /// <summary>The posts and ropes at the end of a rope bridge.</summary>
        RopePosts = 70,
        /// <summary>A lamp niche: a tuff post with a lit oil lamp (the Lamparinas' lamps along the trails).</summary>
        LampNiche = 71,
        /// <summary>The wheel of twelve jars (23's centrepiece).</summary>
        JarWheel = 72,
        /// <summary>The barred portal of the Ossuário dos Artífices.</summary>
        OssuaryPortal = 73,
        /// <summary>The black void over a chasm's bed (<see cref="OverworldProp.End"/> = its other end; ground detail, no collider: the bed blocks).</summary>
        Abyss = 74,

        // Região 3 — Serra de Kaggen (OverworldDecorKit.Mountain.cs): 100–119.
        /// <summary>A herder's rondavel: round stone-and-clay wall with a litema band, conical thatch.</summary>
        Rondavel = 100,
        /// <summary>A round dry-stone kraal (curral) with its gap.</summary>
        Kraal = 101,
        /// <summary>A run of dry-stone wall (<see cref="OverworldProp.End"/> = its other end; <see cref="OverworldProp.Scale"/> = height).</summary>
        DryStoneWall = 102,
        /// <summary>A terrace step: low retaining wall with a sorghum row on top (<see cref="OverworldProp.End"/> = its other end).</summary>
        TerraceRow = 103,
        /// <summary>A sandstone overhang with a painted panel (eland, dancers' silhouettes only as shapes).</summary>
        PaintedShelter = 104,
        /// <summary>A tall slab of the rock walls of a narrow pass.</summary>
        RockSpire = 105,
        /// <summary>A Great-Zimbabwe-like conical stone tower.</summary>
        ConicalTower = 106,
        /// <summary>A granite pillar with a bronze cap and runes (Napirasu).</summary>
        BronzePillar = 107,
        /// <summary>A snow drift on the ground (no collider).</summary>
        SnowDrift = 108,
        /// <summary>A low bank of mist (no collider).</summary>
        MistBank = 109,
        /// <summary>A giant granite block of the Mazans.</summary>
        GraniteBlock = 110,
        /// <summary>A tall stone stela (Estela Alta, the summit).</summary>
        Stela = 111,
    }

    /// <summary>
    /// Low-poly meshes of the <see cref="OverworldDecor"/> pieces, on the shared town palette (one material, merged per
    /// chunk). Each piece is built around the origin (placed with <see cref="TownMeshBuilder.With(Vector3,float,System.Action)"/>)
    /// except the row pieces (crop rows, hedges), which take their two ground points so they follow the slope.
    /// Kept cheap: a grass tuft is 15 triangles, a flower clump 20, a crop-row piece 6–26.
    /// </summary>
    public static partial class OverworldDecorKit
    {
        public static bool IsSolid(OverworldDecor decor) => (int)decor >= (int)OverworldDecor.Hedge;

        // Colour slots of the recorded ground-detail pieces (painted per stamp).
        private const TownSwatch Slot0 = TownSwatch.Black;
        private const TownSwatch Slot1 = TownSwatch.Iron;

        /// <summary>The ground detail recorded once (unit scale) and stamped: hundreds per chunk at a few vertex copies each.</summary>
        public static readonly TownStamp TuftStamp = TownMeshBuilder.Record(b => Tufts(b, 1f, Slot0, Slot1), Slot0, Slot1);
        public static readonly TownStamp FlowerStamp = TownMeshBuilder.Record(b => Flowers(b, 1f, TownSwatch.GrassDark, Slot0, Slot1), Slot0, Slot1);
        public static readonly TownStamp ReedStamp = TownMeshBuilder.Record(b => Reeds(b, 1f));
        public static readonly TownStamp PebbleStamp = TownMeshBuilder.Record(b => Pebbles(b, 1f, Slot0), Slot0);
        public static readonly TownStamp FernStamp = TownMeshBuilder.Record(b => Ferns(b, 1f, Slot0), Slot0);
        public static readonly TownStamp[] PuddleStamps =
        {
            TownMeshBuilder.Record(b => Puddle(b, 1f, 0)), TownMeshBuilder.Record(b => Puddle(b, 1f, 1)),
            TownMeshBuilder.Record(b => Puddle(b, 1f, 2)), TownMeshBuilder.Record(b => Puddle(b, 1f, 3)),
        };

        /// <summary>A thin three-sided spike (a blade of grass, a reed).</summary>
        public static void Spike(TownMeshBuilder b, Vector3 basePoint, Vector3 tip, float radius, TownSwatch swatch)
        {
            Vector3 axis = tip - basePoint;
            Vector3 side = Vector3.Cross(axis, Vector3.up);
            if (side.sqrMagnitude < 1e-6f)
            {
                side = Vector3.right;
            }

            side = side.normalized * radius;
            Vector3 fwd = Vector3.Cross(side, axis).normalized * radius;
            Vector3 a = basePoint + side;
            Vector3 c = basePoint - side * 0.5f + fwd * 0.87f;
            Vector3 d = basePoint - side * 0.5f - fwd * 0.87f;
            b.TriangleFacing(a, c, tip, (a + c) * 0.5f - basePoint, swatch);
            b.TriangleFacing(c, d, tip, (c + d) * 0.5f - basePoint, swatch);
            b.TriangleFacing(d, a, tip, (d + a) * 0.5f - basePoint, swatch);
        }

        public static void Tufts(TownMeshBuilder b, float s, TownSwatch color, TownSwatch tip)
        {
            for (int i = 0; i < 5; i++)
            {
                float a = i * 2.39996f;
                float r = (0.08f + 0.05f * (i & 1)) * s;
                var at = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                float h = (0.32f + 0.08f * (i % 3)) * s;
                Spike(b, at, at * 2.6f + Vector3.up * h, 0.05f * s, (i & 1) == 0 ? color : tip);
            }
        }

        public static void Flowers(TownMeshBuilder b, float s, TownSwatch leaf, TownSwatch a, TownSwatch c)
        {
            for (int i = 0; i < 3; i++)
            {
                float ang = i * 2.1f;
                var at = new Vector3(Mathf.Cos(ang) * 0.12f, 0f, Mathf.Sin(ang) * 0.12f) * s;
                Spike(b, at, at * 2f + Vector3.up * 0.26f * s, 0.045f * s, leaf);
            }

            for (int i = 0; i < 5; i++)
            {
                float ang = i * 1.256f + 0.3f;
                float r = (0.12f + 0.12f * (i & 1)) * s;
                var head = new Vector3(Mathf.Cos(ang) * r, (0.24f + 0.06f * (i % 3)) * s, Mathf.Sin(ang) * r);
                float k = 0.11f * s;
                TownSwatch color = (i % 3) == 0 ? c : a;
                b.Triangle(head + new Vector3(-k, 0f, 0f), head + new Vector3(0f, 0.015f, k), head + new Vector3(k, 0f, 0f), color);
                b.Triangle(head + new Vector3(k, 0f, 0f), head + new Vector3(0f, 0.015f, -k), head + new Vector3(-k, 0f, 0f), color);
            }
        }

        public static void Reeds(TownMeshBuilder b, float s)
        {
            for (int i = 0; i < 7; i++)
            {
                float a = i * 2.39996f;
                float r = (0.1f + 0.06f * (i % 3)) * s;
                var at = new Vector3(Mathf.Cos(a) * r, 0f, Mathf.Sin(a) * r);
                float h = (0.9f + 0.18f * (i % 3)) * s;
                Vector3 tip = at * 1.6f + Vector3.up * h;
                Spike(b, at, tip, 0.04f * s, (i & 1) == 0 ? TownSwatch.Vine : TownSwatch.GrassDark);
                if (i % 3 == 0)
                {
                    Vector3 head = Vector3.Lerp(at, tip, 0.78f);
                    b.Cylinder(head, 0.045f * s, 0.045f * s, 0.16f * s, 4, TownSwatch.Trunk);
                }
            }
        }

        public static void Pebbles(TownMeshBuilder b, float s, TownSwatch color)
        {
            b.Blob(new Vector3(0f, 0.05f * s, 0f), new Vector3(0.34f, 0.16f, 0.26f) * s, color);
            b.Blob(new Vector3(0.28f, 0.035f * s, 0.12f) * s, new Vector3(0.2f, 0.1f, 0.18f) * s, TownSwatch.StoneDark);
        }

        public static void Ferns(TownMeshBuilder b, float s, TownSwatch color)
        {
            for (int i = 0; i < 6; i++)
            {
                float a = i * 1.047f + 0.2f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                var side = new Vector3(-dir.z, 0f, dir.x) * 0.13f * s;
                Vector3 mid = dir * 0.38f * s + Vector3.up * 0.32f * s;
                Vector3 tip = dir * 0.7f * s + Vector3.up * 0.12f * s;
                TownSwatch c = (i & 1) == 0 ? color : TownSwatch.GrassDark;
                b.TriangleFacing(Vector3.up * 0.04f, mid + side, tip, Vector3.up, c);
                b.TriangleFacing(Vector3.up * 0.04f, tip, mid - side, Vector3.up, c);
            }
        }

        /// <summary>A flat irregular puddle (7 corners).</summary>
        public static void Puddle(TownMeshBuilder b, float s, int seed)
        {
            const int n = 7;
            Vector3 center = Vector3.up * 0.04f;
            for (int i = 0; i < n; i++)
            {
                float a0 = i * Mathf.PI * 2f / n;
                float a1 = (i + 1) * Mathf.PI * 2f / n;
                float r0 = (0.75f + 0.45f * TownStyle.Hash01("pud", seed * 13 + i)) * s;
                float r1 = (0.75f + 0.45f * TownStyle.Hash01("pud", seed * 13 + (i + 1) % n)) * s;
                b.Triangle(center, center + new Vector3(Mathf.Cos(a1) * r1, 0f, Mathf.Sin(a1) * r1), center + new Vector3(Mathf.Cos(a0) * r0, 0f, Mathf.Sin(a0) * r0), TownSwatch.Water);
            }
        }

        public static void Log(TownMeshBuilder b, float s)
        {
            b.Log(new Vector3(-1.3f, 0.2f, 0f) * s, new Vector3(1.3f, 0.24f, 0.05f) * s, 0.22f * s, TownSwatch.Trunk, 6, TownSwatch.WoodLight);
            b.Log(new Vector3(0.4f, 0.3f, 0.1f) * s, new Vector3(0.75f, 0.62f, 0.45f) * s, 0.07f * s, TownSwatch.Trunk, 4);
            b.Blob(new Vector3(-0.5f, 0.42f, 0f) * s, new Vector3(0.7f, 0.16f, 0.36f) * s, TownSwatch.Vine);
        }

        public static void Stump(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.36f * s, 0.3f * s, 0.48f * s, 6, TownSwatch.Trunk, TownSwatch.WoodLight);
            Spike(b, new Vector3(0.28f, 0f, 0.05f) * s, new Vector3(0.62f, 0.02f, 0.12f) * s, 0.08f * s, TownSwatch.Trunk);
            Spike(b, new Vector3(-0.2f, 0f, -0.2f) * s, new Vector3(-0.48f, 0.02f, -0.46f) * s, 0.08f * s, TownSwatch.Trunk);
        }

        public static void MossRock(TownMeshBuilder b, float s, TownSwatch rock)
        {
            b.Crown(new Vector3(0f, 0.32f * s, 0f), new Vector3(1.2f, 0.8f, 1.0f) * s, rock, 20f);
            b.Crown(new Vector3(0.08f * s, 0.62f * s, 0.05f * s), new Vector3(0.9f, 0.26f, 0.7f) * s, TownSwatch.Vine, 70f);
        }

        public static void Cairn(TownMeshBuilder b, float s)
        {
            b.Crown(new Vector3(0f, 0.3f, 0f) * s, new Vector3(1.2f, 0.62f, 1.1f) * s, TownSwatch.Stone, 10f);
            b.Crown(new Vector3(0.05f, 0.8f, 0.02f) * s, new Vector3(0.85f, 0.48f, 0.8f) * s, TownSwatch.StoneDark, 50f);
            b.Crown(new Vector3(0f, 1.18f, 0.04f) * s, new Vector3(0.5f, 0.36f, 0.48f) * s, TownSwatch.Stone, 80f);
        }

        public static void Basalt(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.42f * s, 0.42f * s, 2.6f * s, 6, TownSwatch.Basalt, TownSwatch.StoneDark);
            b.Cylinder(new Vector3(0.75f, 0f, 0.2f) * s, 0.38f * s, 0.38f * s, 1.7f * s, 6, TownSwatch.Basalt, TownSwatch.StoneDark, 0.5f);
            b.Cylinder(new Vector3(0.2f, 0f, 0.78f) * s, 0.36f * s, 0.36f * s, 1.1f * s, 6, TownSwatch.StoneDark, TownSwatch.Basalt);
            b.Cylinder(new Vector3(-0.6f, 0f, 0.45f) * s, 0.34f * s, 0.34f * s, 0.6f * s, 6, TownSwatch.Basalt, TownSwatch.StoneDark, 0.5f);
        }

        public static void Column(TownMeshBuilder b, float s, int variant)
        {
            b.Box(new Vector3(0f, 0.15f, 0f) * s, new Vector3(1f, 0.3f, 1f) * s, TownSwatch.Sandstone, 0f, TownSwatch.Paving);
            float h = (variant & 1) == 0 ? 2.4f : 1.2f;
            b.Cylinder(new Vector3(0f, 0.3f, 0f) * s, 0.36f * s, 0.33f * s, h * s, 8, TownSwatch.Paving, TownSwatch.PavingDark);
            if ((variant & 2) == 0)
            {
                b.Log(new Vector3(0.9f, 0.32f, -0.4f) * s, new Vector3(1.9f, 0.32f, -0.1f) * s, 0.32f * s, TownSwatch.Paving, 8, TownSwatch.PavingDark);
            }
        }

        public static void Cypress(TownMeshBuilder b, float s)
        {
            b.Cylinder(Vector3.zero, 0.14f * s, 0.1f * s, 0.8f * s, 5, TownSwatch.Trunk);
            b.Crown(new Vector3(0f, 2.6f, 0f) * s, new Vector3(1.1f, 4.4f, 1.1f) * s, TownSwatch.LeafDark, 15f);
            b.Crown(new Vector3(0f, 4.6f, 0f) * s, new Vector3(0.6f, 1.6f, 0.6f) * s, TownSwatch.Pine, 50f);
        }

        public static void DeadTree(TownMeshBuilder b, float s)
        {
            Vector3 top = new Vector3(0.2f, 2.8f, 0.1f) * s;
            b.Log(Vector3.zero, top, 0.16f * s, TownSwatch.WoodWeathered, 5);
            b.Log(new Vector3(0.1f, 1.6f, 0.05f) * s, new Vector3(0.9f, 2.4f, 0.3f) * s, 0.07f * s, TownSwatch.WoodWeathered, 4);
            b.Log(new Vector3(0.15f, 2.1f, 0.08f) * s, new Vector3(-0.6f, 2.9f, -0.3f) * s, 0.06f * s, TownSwatch.WoodWeathered, 4);
            b.Log(top, top + new Vector3(0.3f, 0.6f, 0.2f) * s, 0.05f * s, TownSwatch.WoodWeathered, 4);
        }

        // ------------------------------------------------------------------
        // Row pieces (two ground points: they follow the slope)
        // ------------------------------------------------------------------

        /// <summary>A ridge (triangular cross-section) from <paramref name="a"/> to <paramref name="c"/> (ground points): 6 triangles.</summary>
        public static void Ridge(TownMeshBuilder b, Vector3 a, Vector3 c, float width, float height, TownSwatch left, TownSwatch right)
        {
            Vector3 d = c - a;
            d.y = 0f;
            if (d.sqrMagnitude < 1e-4f)
            {
                return;
            }

            Vector3 n = new Vector3(-d.z, 0f, d.x).normalized * (width * 0.5f);
            Vector3 up = Vector3.up * height;
            Vector3 ta = a + up;
            Vector3 tc = c + up;
            b.QuadFacing(a + n, c + n, tc, ta, n + up * 0.5f, left);
            b.QuadFacing(a - n, ta, tc, c - n, -n + up * 0.5f, right);
            b.TriangleFacing(a + n, ta, a - n, -d, left);
            b.TriangleFacing(c - n, tc, c + n, d, right);
        }

        /// <summary>A box-like run from <paramref name="a"/> to <paramref name="c"/> between heights <paramref name="bottom"/> and <paramref name="top"/> over the ground (no underside unless lifted).</summary>
        public static void Run(TownMeshBuilder b, Vector3 a, Vector3 c, float width, float bottom, float top, TownSwatch side, TownSwatch upper)
        {
            Vector3 d = c - a;
            d.y = 0f;
            if (d.sqrMagnitude < 1e-4f)
            {
                return;
            }

            Vector3 n = new Vector3(-d.z, 0f, d.x).normalized * (width * 0.5f);
            Vector3 lo = Vector3.up * bottom;
            Vector3 hi = Vector3.up * top;
            Vector3 a0 = a + n + lo, a1 = a - n + lo, c0 = c + n + lo, c1 = c - n + lo;
            Vector3 a2 = a + n + hi, a3 = a - n + hi, c2 = c + n + hi, c3 = c - n + hi;
            b.QuadFacing(a0, c0, c2, a2, n, side);
            b.QuadFacing(a1, a3, c3, c1, -n, side);
            b.QuadFacing(a2, c2, c3, a3, Vector3.up, upper);
            b.QuadFacing(a0, a2, a3, a1, -d, side);
            b.QuadFacing(c0, c1, c3, c2, d, side);
            if (bottom > 0.05f)
            {
                b.QuadFacing(a0, a1, c1, c0, Vector3.down, side);
            }
        }

        /// <summary>One piece of crop row of <paramref name="crop"/> between ground points <paramref name="a"/> and <paramref name="c"/>.</summary>
        public static void CropRow(TownMeshBuilder b, Vector3 a, Vector3 c, FieldCrop crop, int seed)
        {
            float h01 = TownStyle.Hash01("crop", seed);
            Vector3 mid = (a + c) * 0.5f;
            Vector3 d = c - a;
            switch (crop)
            {
                case FieldCrop.Wheat:
                    Ridge(b, a, c, 0.62f, 0.62f + 0.12f * h01, TownSwatch.LeafGold, TownSwatch.Thatch);
                    break;
                case FieldCrop.Cotton:
                    Ridge(b, a, c, 0.6f, 0.42f, TownSwatch.GrassDark, TownSwatch.LeafDark);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector3 p = Vector3.Lerp(a, c, (i + 0.3f + 0.4f * h01) / 3f) + Vector3.up * 0.36f;
                        b.Cylinder(p, 0.11f, 0f, 0.14f, 3, TownSwatch.ClothWhite, null, i * 0.3f);
                    }

                    break;
                case FieldCrop.Vines:
                    b.Cylinder(a, 0.05f, 0.05f, 1.45f, 4, TownSwatch.WoodWeathered);
                    Run(b, a, c, 0.5f, 0.72f, 1.32f + 0.08f * h01, TownSwatch.Vine, TownSwatch.Grass);
                    b.Cylinder(mid + Vector3.up * 0.55f + Vector3.Cross(d, Vector3.up).normalized * 0.26f, 0.09f, 0f, 0.2f, 3, TownSwatch.Violet);
                    break;
                case FieldCrop.Melons:
                    Ridge(b, a, c, 0.95f, 0.16f, TownSwatch.Vine, TownSwatch.GrassDark);
                    b.Blob(Vector3.Lerp(a, c, 0.25f + 0.2f * h01) + new Vector3(0f, 0.18f, 0f), new Vector3(0.42f, 0.32f, 0.34f), h01 > 0.5f ? TownSwatch.Grass : TownSwatch.ClothYellow);
                    b.Blob(Vector3.Lerp(a, c, 0.75f) + new Vector3(0.12f, 0.16f, 0.1f), new Vector3(0.36f, 0.28f, 0.3f), TownSwatch.Grass);
                    break;
                case FieldCrop.Hay:
                    Ridge(b, a, c, 0.4f, 0.12f, TownSwatch.Thatch, TownSwatch.SandDark);
                    break;
                default:
                    Ridge(b, a, c, 0.55f, 0.2f, TownSwatch.Dirt, TownSwatch.DirtDark);
                    break;
            }
        }

        /// <summary>One piece of hedgerow between ground points <paramref name="a"/> and <paramref name="c"/>.</summary>
        public static void Hedge(TownMeshBuilder b, Vector3 a, Vector3 c, int seed)
        {
            float h01 = TownStyle.Hash01("hedge", seed);
            Run(b, a, c, 1.0f, 0f, 1.05f + 0.2f * h01, TownSwatch.LeafDark, TownSwatch.Poplar);
            Vector3 top = Vector3.Lerp(a, c, 0.3f + 0.4f * h01) + Vector3.up * (1.15f + 0.2f * h01);
            b.Blob(top, new Vector3(1.1f, 0.5f, 1.2f), (seed & 1) == 0 ? TownSwatch.Poplar : TownSwatch.LeafDark);
        }
    }
}
