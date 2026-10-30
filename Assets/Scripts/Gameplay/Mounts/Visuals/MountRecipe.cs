using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>Part names of a mount model (what <see cref="MountActor"/> and the leg locomotion look for).</summary>
    public static class MountParts
    {
        public const string Body = "Body";
        public const string Head = "Head";
        public const string EarLeft = "Ear_L";
        public const string EarRight = "Ear_R";
        public const string Tail = "Tail";
        public const string TailTip = "Tail_1";

        /// <summary>Pivot on the saddle where the rider's hips sit (child of <see cref="Body"/>, so it bobs with it).</summary>
        public const string Seat = "Seat";

        /// <summary>Cowbell hanging under the neck (the donkey's chocalho): swings on its own.</summary>
        public const string Bell = "Bell";
    }

    /// <summary>
    /// The body plan of a procedural quadruped mount: one parameter set covers the pony, the big dog and the donkey
    /// (Docs/Mounts.md §13 M1 step 3). Metres; the model faces +Z with its pivot on the ground under the body.
    /// </summary>
    public struct QuadrupedShape
    {
        public float LegLength;
        public float BodyLength;
        public float BodyThickness;
        public float BodyWidth;
        public float NeckLength;
        public float NeckPitch;
        public float HeadLength;
        public float HeadSize;
        public float EarLength;
        public bool FloppyEars;
        public float TailLength;
        public bool BushyTail;
        public bool UprightMane;
        public bool Hooves;
        public bool Cowbell;
        public bool Collar;
        public bool Packs;

        // ---- M3 wave 1 extras (MountWaveDecor draws them; all default to "off") ----

        /// <summary>Two legs (the rheas): only <c>Leg_HL/HR</c>, set under the body's centre.</summary>
        public bool Biped;
        public MountHornStyle Horns;
        public MountCoatPattern Pattern;
        public EnemySwatch PatternSwatch;
        public EnemySwatch Accent;
        public bool Tusks;
        public bool Trunk;
        public bool Shell;
        public bool Shaggy;
        public bool Jewel;
        public bool FlameCrest;
        public bool Beard;
        public bool Wings;

        /// <summary>Leg thickness factor (0 = 1: the starters; the mammoth and the tortoise have thick legs).</summary>
        public float LegGirth;

        public float Girth => LegGirth > 0f ? LegGirth : 1f;

        /// <summary>Extra height of the seat (the tortoise rides on top of its shell).</summary>
        public float SeatLift;

        public EnemySwatch Coat;
        public EnemySwatch Dark;
        public EnemySwatch Light;
        public EnemySwatch Mane;
        public EnemySwatch Socks;
        public EnemySwatch Foot;
        public EnemySwatch Blanket;
        public EnemySwatch Saddle;
        public EnemySwatch Trim;

        /// <summary>Height of the body centre above the ground.</summary>
        public float BodyHeight => LegLength + BodyThickness * 0.12f;

        /// <summary>Height of the saddle top (the rider's hips).</summary>
        public float SeatHeight => BodyHeight + BodyThickness * 0.5f + 0.05f + SeatLift;
    }

    /// <summary>
    /// The three starter mounts as low-poly jointed models, built with the enemies' <see cref="EnemyMeshBuilder"/> on
    /// their shared palette (one material for everything), in the same part layout as <see cref="WolfRecipe"/> so
    /// <see cref="Rapadura.Gameplay.Locomotion.ProceduralLegLocomotion"/> steps them: <c>Body</c> → <c>Head</c> →
    /// <c>Ear_L</c>/<c>Ear_R</c>, <c>Body</c> → <c>Tail</c> → <c>Tail_1</c>, <c>Body</c> → <c>Seat</c> (and
    /// <c>Bell</c>); four legs under the root, <c>Leg_xx</c> → <c>Shin_xx</c> → <c>Foot_xx</c>, xx = FL/FR/HL/HR.
    /// <list type="bullet">
    /// <item><b>Pônei Pé-de-Pano</b>: small bay pony, dark mane, white "pé-de-pano" socks and blaze, red chita blanket.</item>
    /// <item><b>Cachorrão Paçoca</b>: Caramelo's big nephew, paçoca-coloured (caramel) with cream socks and muzzle,
    /// floppy ears, bushy tail, red collar with a gold tag, green blanket.</item>
    /// <item><b>Jegue Carregador</b>: grey donkey, dark stripe and short upright mane, pale muzzle and belly, very long
    /// ears, saddle packs and a gold cowbell (chocalho) under the neck.</item>
    /// </list>
    /// </summary>
    public static class MountRecipe
    {
        public static QuadrupedShape ShapeFor(MountVisualRecipe recipe)
        {
            switch (recipe)
            {
                case MountVisualRecipe.BigDog:
                    return new QuadrupedShape
                    {
                        LegLength = 0.52f, BodyLength = 1.05f, BodyThickness = 0.42f, BodyWidth = 0.4f,
                        NeckLength = 0.24f, NeckPitch = 35f, HeadLength = 0.3f, HeadSize = 0.3f, EarLength = 0.2f, FloppyEars = true,
                        TailLength = 0.42f, BushyTail = true, Collar = true,
                        // The fluffy alabai: cream coat, pale socks.
                        Coat = EnemySwatch.Bone, Dark = EnemySwatch.SandstoneDark, Light = EnemySwatch.Fang,
                        Mane = EnemySwatch.Bone, Socks = EnemySwatch.Fang, Foot = EnemySwatch.Leather,
                        Blanket = EnemySwatch.CaveMoss, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                case MountVisualRecipe.Donkey:
                    return new QuadrupedShape
                    {
                        LegLength = 0.6f, BodyLength = 1.12f, BodyThickness = 0.48f, BodyWidth = 0.44f,
                        NeckLength = 0.34f, NeckPitch = 38f, HeadLength = 0.42f, HeadSize = 0.26f, EarLength = 0.34f,
                        TailLength = 0.5f, UprightMane = true, Hooves = true, Cowbell = true, Packs = true,
                        Coat = EnemySwatch.GolemStone, Dark = EnemySwatch.GolemStoneDark, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.GolemStoneDark, Socks = EnemySwatch.GolemStone, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.Sandstone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                default:
                    if (MountWaveRecipes.TryShape(recipe, out QuadrupedShape wave))
                    {
                        return wave;
                    }

                    return new QuadrupedShape
                    {
                        LegLength = 0.6f, BodyLength = 1.12f, BodyThickness = 0.44f, BodyWidth = 0.4f,
                        NeckLength = 0.44f, NeckPitch = 52f, HeadLength = 0.44f, HeadSize = 0.24f, EarLength = 0.13f,
                        TailLength = 0.55f, UprightMane = false, Hooves = true,
                        Coat = EnemySwatch.Leather, Dark = EnemySwatch.Shadow, Light = EnemySwatch.Horn,
                        Mane = EnemySwatch.Shadow, Socks = EnemySwatch.Bone, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.SandstoneDark, Trim = EnemySwatch.Gold,
                    };
            }
        }

        public static void Build(ProceduralEnemyBlueprint bp, MountVisualRecipe recipe) => Build(bp, ShapeFor(recipe));

        public static void Build(ProceduralEnemyBlueprint bp, QuadrupedShape s)
        {
            float h = s.BodyHeight;
            float halfLength = s.BodyLength * 0.5f;
            int body = bp.Mesh(MountParts.Body, -1, new Vector3(0f, h, 0f), b => DrawBody(b, s));

            // Neck rises from the chest; the head sits at its tip.
            float pitch = s.NeckPitch * Mathf.Deg2Rad;
            Vector3 neckBase = new Vector3(0f, s.BodyThickness * 0.22f, halfLength * 0.78f);
            Vector3 neckTip = neckBase + new Vector3(0f, Mathf.Sin(pitch), Mathf.Cos(pitch)) * s.NeckLength;
            int head = bp.Mesh(MountParts.Head, body, neckTip, b => DrawHead(b, s), Quaternion.Euler(12f, 0f, 0f));
            for (int side = -1; side <= 1 && s.EarLength > 0f; side += 2)
            {
                int captured = side;
                Quaternion earRotation = s.FloppyEars
                    ? Quaternion.Euler(0f, 0f, 120f * side)
                    : Quaternion.Euler(-10f, 0f, -14f * side);
                bp.Mesh(side < 0 ? MountParts.EarLeft : MountParts.EarRight, head,
                    new Vector3(s.HeadSize * 0.32f * side, s.HeadSize * 0.38f, -s.HeadSize * 0.12f), b => DrawEar(b, s, captured), earRotation,
                    meshKey: "Ear" + (side < 0 ? "L" : "R"));
            }

            int tail = bp.Mesh(MountParts.Tail, body, new Vector3(0f, s.BodyThickness * 0.25f, -halfLength * 0.95f), b => DrawTailBase(b, s),
                Quaternion.Euler(s.BushyTail ? 35f : -30f, 0f, 0f));
            bp.Mesh(MountParts.TailTip, tail, new Vector3(0f, 0f, -s.TailLength * 0.5f), b => DrawTailTip(b, s));

            bp.Node(MountParts.Seat, body, new Vector3(0f, s.SeatHeight - h, -0.02f));
            if (s.Cowbell)
            {
                bp.Mesh(MountParts.Bell, body, neckBase + new Vector3(0f, -0.06f, 0.1f), b => DrawBell(b, s));
            }

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                float L = s.LegLength;
                float halfWidth = s.BodyWidth * 0.32f;

                if (!s.Biped)
                {
                    // Front: shoulder → elbow (just behind) → hoof/paw.
                    Vector3 frontKnee = new Vector3(0f, -L * 0.48f, -L * 0.04f);
                    Vector3 frontFoot = new Vector3(0f, -L * 0.42f, L * 0.04f);
                    int front = bp.Mesh("Leg_F" + x, -1, new Vector3(halfWidth * side, L, halfLength * 0.62f),
                        b => DrawUpper(b, s, frontKnee, 0.085f, 0.05f, false), meshKey: "FrontUpper");
                    int frontShin = bp.Mesh("Shin_F" + x, front, frontKnee, b => DrawLower(b, s, frontFoot, 0.048f, 0.04f), meshKey: "FrontLower");
                    bp.Mesh("Foot_F" + x, frontShin, frontFoot, b => DrawFoot(b, s), meshKey: "Foot");
                }

                // Hind: hip → hock (well behind) → hoof/paw. A biped stands on them under the body's centre.
                Vector3 hindKnee = new Vector3(0f, -L * 0.55f, -L * 0.1f);
                Vector3 hindFoot = new Vector3(0f, -L * 0.35f, L * 0.1f);
                float hindZ = s.Biped ? -halfLength * 0.08f : -halfLength * 0.62f;
                int hind = bp.Mesh("Leg_H" + x, -1, new Vector3(halfWidth * side, L, hindZ),
                    b => DrawUpper(b, s, hindKnee, 0.095f, 0.048f, true), meshKey: "HindUpper");
                int hindShin = bp.Mesh("Shin_H" + x, hind, hindKnee, b => DrawLower(b, s, hindFoot, 0.045f, 0.04f), meshKey: "HindLower");
                bp.Mesh("Foot_H" + x, hindShin, hindFoot, b => DrawFoot(b, s), meshKey: "Foot");
            }

            // Horns, shell, trunk, spots, flames... of the M3 species (nothing for the starters).
            MountWaveDecor.Decorate(bp, s, body, head);
        }

        // ------------------------------------------------------------------

        private static void DrawBody(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float l = s.BodyLength;
            float t = s.BodyThickness;
            float w = s.BodyWidth;

            // Barrel, chest and rump, pale belly.
            b.Blob(Vector3.zero, new Vector3(w, t, l * 0.86f), s.Coat);
            b.Blob(new Vector3(0f, t * 0.02f, l * 0.3f), new Vector3(w * 1.05f, t * 1.05f, l * 0.42f), s.Coat);
            b.Blob(new Vector3(0f, t * 0.06f, -l * 0.32f), new Vector3(w * 1.08f, t * 1.02f, l * 0.4f), s.Coat);
            b.Blob(new Vector3(0f, -t * 0.28f, 0f), new Vector3(w * 0.7f, t * 0.4f, l * 0.6f), s.Light, coarse: true);
            if (s.Dark != s.Coat)
            {
                // Dorsal stripe (donkey cross, dog saddle patch).
                b.Box(new Vector3(0f, t * 0.47f, -l * 0.1f), new Vector3(w * 0.16f, 0.03f, l * 0.6f), s.Dark);
            }

            // Shoulder blades and haunches the legs plug into.
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(w * 0.26f * side, -t * 0.12f, -l * 0.31f), new Vector3(w * 0.42f, t * 0.72f, l * 0.3f), s.Coat, coarse: true);
                b.Blob(new Vector3(w * 0.24f * side, -t * 0.14f, l * 0.31f), new Vector3(w * 0.38f, t * 0.66f, l * 0.24f), s.Coat, coarse: true);
            }

            // Neck from the chest up to the head.
            float pitch = s.NeckPitch * Mathf.Deg2Rad;
            Vector3 neckBase = new Vector3(0f, t * 0.22f, l * 0.5f * 0.78f);
            Vector3 neckTip = neckBase + new Vector3(0f, Mathf.Sin(pitch), Mathf.Cos(pitch)) * s.NeckLength;
            b.Segment(neckBase + new Vector3(0f, -t * 0.1f, -0.04f), neckTip, w * 0.42f, s.HeadSize * 0.42f, 7, s.Coat);
            if (s.Collar)
            {
                Vector3 mid = Vector3.Lerp(neckBase, neckTip, 0.55f);
                b.Segment(mid - (neckTip - neckBase).normalized * 0.03f, mid + (neckTip - neckBase).normalized * 0.03f, w * 0.38f, w * 0.36f, 8, EnemySwatch.WarCape);
                b.Blob(mid + new Vector3(0f, -w * 0.3f, 0.06f), new Vector3(0.06f, 0.07f, 0.02f), EnemySwatch.Gold, coarse: true);
            }

            if (s.UprightMane)
            {
                // Short brush mane along the neck crest.
                for (int k = 0; k < 4; k++)
                {
                    Vector3 p = Vector3.Lerp(neckBase, neckTip, 0.15f + 0.25f * k) + new Vector3(0f, w * 0.18f, -0.03f);
                    b.Box(p, new Vector3(0.035f, 0.1f, 0.08f), s.Mane, Quaternion.Euler(-s.NeckPitch * 0.5f, 0f, 0f));
                }
            }
            else if (s.Mane != s.Coat)
            {
                // Flowing mane falling on one side of the neck.
                for (int k = 0; k < 4; k++)
                {
                    Vector3 p = Vector3.Lerp(neckBase, neckTip, 0.12f + 0.24f * k) + new Vector3(w * 0.12f, w * 0.12f, -0.04f);
                    b.Box(p, new Vector3(0.05f, 0.16f, 0.1f), s.Mane, Quaternion.Euler(-s.NeckPitch * 0.4f, 0f, -18f));
                }
            }

            // Saddle blanket, saddle and pommel (the seat).
            float top = t * 0.5f;
            b.Box(new Vector3(0f, top - 0.02f, -0.02f), new Vector3(w * 1.12f, 0.035f, l * 0.36f), s.Blanket, top: s.Blanket);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(new Vector3(w * 0.5f * side, top - t * 0.2f, -0.02f), new Vector3(0.025f, t * 0.42f, l * 0.34f), s.Blanket,
                    Quaternion.Euler(0f, 0f, 12f * side));
                b.Box(new Vector3(w * 0.52f * side, top - t * 0.42f, -0.02f), new Vector3(0.03f, 0.03f, l * 0.34f), s.Trim);
            }

            b.Box(new Vector3(0f, top + 0.02f, -0.02f), new Vector3(w * 0.7f, 0.05f, l * 0.26f), s.Saddle);
            b.Box(new Vector3(0f, top + 0.06f, l * 0.11f), new Vector3(w * 0.32f, 0.06f, 0.05f), s.Saddle);
            if (s.Packs)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    b.Box(new Vector3(w * 0.62f * side, -t * 0.02f, -l * 0.24f), new Vector3(0.12f, 0.2f, 0.24f), s.Blanket, top: s.Saddle);
                    b.Box(new Vector3(w * 0.69f * side, 0f, -l * 0.24f), new Vector3(0.02f, 0.05f, 0.18f), s.Saddle);
                }
            }
        }

        private static void DrawHead(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float size = s.HeadSize;
            float len = s.HeadLength;
            b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(size, size * 0.95f, size * 1.1f), s.Coat);

            // Muzzle toward +z, pale nose, nostrils / dog nose.
            b.Cylinder(new Vector3(0f, -size * 0.12f, size * 0.25f), size * 0.36f, size * 0.3f, len * 0.75f, 6, s.Coat, s.Light,
                Quaternion.Euler(100f, 0f, 0f));
            b.Blob(new Vector3(0f, -size * 0.28f, size * 0.25f + len * 0.62f), new Vector3(size * 0.62f, size * 0.5f, size * 0.55f), s.Light, coarse: true);
            if (s.Hooves)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    b.Blob(new Vector3(size * 0.14f * side, -size * 0.2f, size * 0.25f + len * 0.85f), new Vector3(0.03f, 0.03f, 0.02f), EnemySwatch.Black, coarse: true);
                }
            }
            else
            {
                b.Blob(new Vector3(0f, -size * 0.08f, size * 0.25f + len * 0.9f), new Vector3(0.08f, 0.06f, 0.06f), EnemySwatch.Black, coarse: true);
            }

            if (s.Socks != s.Coat && s.Hooves)
            {
                // The white blaze down the face ("pé-de-pano" pony).
                b.Box(new Vector3(0f, size * 0.12f, size * 0.42f), new Vector3(size * 0.16f, 0.02f, len * 0.6f), s.Socks, Quaternion.Euler(-18f, 0f, 0f));
            }

            // Big friendly eyes with a little shine.
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 eye = new Vector3(size * 0.42f * side, size * 0.1f, size * 0.18f);
                b.Blob(eye, new Vector3(0.06f, 0.065f, 0.05f), EnemySwatch.Black, coarse: true);
                b.Blob(eye + new Vector3(0.012f * side, 0.018f, 0.018f), new Vector3(0.02f, 0.02f, 0.012f), EnemySwatch.Fang, coarse: true);
            }

            if (!s.UprightMane && s.Mane != s.Coat)
            {
                // Forelock.
                b.Box(new Vector3(0f, size * 0.46f, size * 0.14f), new Vector3(size * 0.32f, 0.07f, 0.12f), s.Mane, Quaternion.Euler(30f, 0f, 0f));
            }
        }

        private static void DrawEar(EnemyMeshBuilder b, QuadrupedShape s, int side)
        {
            if (s.FloppyEars)
            {
                // Hangs down along the cheek.
                b.Blob(new Vector3(0f, s.EarLength * 0.5f, 0f), new Vector3(0.05f, s.EarLength, 0.12f), s.Dark);
                return;
            }

            b.Cylinder(Vector3.zero, 0.05f, 0.01f, s.EarLength, 4, s.Coat, s.Dark);
            if (s.EarLength > 0.25f)
            {
                // Donkey ear: pale inside, dark tip.
                b.Box(new Vector3(0f, s.EarLength * 0.45f, 0.022f), new Vector3(0.04f, s.EarLength * 0.6f, 0.01f), s.Light);
                b.Cylinder(new Vector3(0f, s.EarLength * 0.78f, 0f), 0.022f, 0f, s.EarLength * 0.24f, 4, EnemySwatch.Black);
            }
        }

        private static void DrawTailBase(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float half = s.TailLength * 0.5f;
            if (s.BushyTail)
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -half), 0.05f, 0.08f, 6, s.Coat);
            }
            else
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -half), 0.045f, 0.035f, 5, s.Hooves && s.Mane != s.Coat && !s.UprightMane ? s.Mane : s.Coat);
            }
        }

        private static void DrawTailTip(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float half = s.TailLength * 0.5f;
            if (s.BushyTail)
            {
                b.Blob(new Vector3(0f, 0f, -half * 0.5f), new Vector3(0.14f, 0.14f, half * 1.1f), s.Coat);
                b.Blob(new Vector3(0f, 0f, -half * 0.95f), new Vector3(0.1f, 0.1f, 0.12f), s.Light, coarse: true);
            }
            else if (s.UprightMane)
            {
                // Donkey: thin tail with a dark tuft at the end.
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -half * 0.8f), 0.03f, 0.022f, 5, s.Coat);
                b.Blob(new Vector3(0f, 0f, -half * 0.95f), new Vector3(0.08f, 0.08f, 0.16f), s.Dark, coarse: true);
            }
            else
            {
                // Pony: full flowing tail.
                b.Blob(new Vector3(0f, -0.02f, -half * 0.55f), new Vector3(0.13f, 0.13f, half * 1.2f), s.Mane);
            }
        }

        private static void DrawBell(EnemyMeshBuilder b, QuadrupedShape s)
        {
            b.Box(new Vector3(0f, 0.02f, 0f), new Vector3(0.02f, 0.06f, 0.02f), s.Saddle);
            b.Cylinder(new Vector3(0f, -0.1f, 0f), 0.06f, 0.035f, 0.1f, 6, s.Trim, s.Trim, bottomCap: true);
            b.Blob(new Vector3(0f, -0.11f, 0f), new Vector3(0.025f, 0.025f, 0.025f), EnemySwatch.IronDark, coarse: true);
        }

        private static void DrawUpper(EnemyMeshBuilder b, QuadrupedShape s, Vector3 knee, float topRadius, float bottomRadius, bool thigh)
        {
            float g = s.Girth;
            topRadius *= g;
            bottomRadius *= g;
            b.Segment(knee, new Vector3(0f, 0.05f, 0f), bottomRadius, topRadius, 6, s.Coat);
            if (thigh)
            {
                b.Blob(new Vector3(0f, -s.LegLength * 0.12f, -0.01f), new Vector3(0.16f * g, s.LegLength * 0.42f, 0.22f * g), s.Coat, coarse: true);
            }

            b.Blob(knee, Vector3.one * (bottomRadius * 2.1f), s.Coat, coarse: true);
        }

        private static void DrawLower(EnemyMeshBuilder b, QuadrupedShape s, Vector3 foot, float topRadius, float bottomRadius)
        {
            // The lower half takes the "socks" colour (white pé-de-pano, cream paws).
            topRadius *= s.Girth;
            bottomRadius *= s.Girth;
            Vector3 mid = foot * 0.45f;
            b.Segment(mid, Vector3.zero, bottomRadius * 1.05f, topRadius, 5, s.Coat);
            b.Segment(foot, mid, bottomRadius, bottomRadius * 1.05f, 5, s.Socks);
        }

        private static void DrawFoot(EnemyMeshBuilder b, QuadrupedShape s)
        {
            if (s.Hooves)
            {
                b.Cylinder(new Vector3(0f, -s.LegLength * 0.1f, 0.005f), 0.055f * s.Girth, 0.045f * s.Girth, s.LegLength * 0.1f, 6, s.Foot, s.Foot, bottomCap: true);
            }
            else
            {
                b.Blob(new Vector3(0f, -s.LegLength * 0.06f, 0.03f), new Vector3(0.09f * s.Girth, s.LegLength * 0.12f, 0.13f * s.Girth), s.Socks, coarse: true);
            }
        }
    }
}
