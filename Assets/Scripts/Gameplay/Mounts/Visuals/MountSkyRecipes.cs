using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The body plans of the flying mounts (<see cref="MountSkyCatalog"/>): the same <see cref="QuadrupedShape"/> /
    /// <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/> path as every mount (four jointed legs,
    /// so the gait engine can still bind them when the mount walks on the ground), plus two jointed wings
    /// (<see cref="MountParts.WingLeft"/> → <see cref="MountParts.WingTipLeft"/>, same on the right) that
    /// <see cref="MountActor"/> beats, and the eagle beak. Deterministic, no randomness.
    /// <list type="bullet">
    /// <item><b>Grifo da Coluna da Glória</b>: ivory lion body with a gold dorsal line, pale feather ruff on the neck,
    /// white eagle head with a gold hooked beak and ear tufts, gold talons, white wings whose primaries end in glowing
    /// light (the sparks climbing the Coluna), silver blanket (Lavine's Ar Sereno).</item>
    /// </list>
    /// </summary>
    public static class MountSkyRecipes
    {
        public const string Prefix = "Decor_Sky_";

        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.ColumnGriffin:
                    shape = new QuadrupedShape
                    {
                        LegLength = 0.62f, BodyLength = 1.2f, BodyThickness = 0.46f, BodyWidth = 0.42f,
                        NeckLength = 0.36f, NeckPitch = 55f, HeadLength = 0.1f, HeadSize = 0.28f, EarLength = 0.12f,
                        TailLength = 0.7f, LegGirth = 1.1f,
                        FlightWings = true, WingSpan = 1.3f, Beak = true,
                        Coat = EnemySwatch.Ivory, Dark = EnemySwatch.Gold, Light = EnemySwatch.SandstoneLight,
                        Mane = EnemySwatch.Fang, Socks = EnemySwatch.Ivory, Foot = EnemySwatch.Gold,
                        Blanket = EnemySwatch.Steel, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                        Accent = EnemySwatch.Gold, PatternSwatch = EnemySwatch.StolenLight,
                    };
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }

        /// <summary>Where a wing joins the body (body-local; <paramref name="side"/> -1 left, +1 right): high on the front shoulders, ahead of the rider's knees.</summary>
        public static Vector3 WingRoot(QuadrupedShape s, int side) => new Vector3(s.BodyWidth * 0.42f * side, s.BodyThickness * 0.38f, s.BodyLength * 0.27f);

        /// <summary>Where the outer half of a wing joins the inner half (wing-local).</summary>
        public static Vector3 WingElbow(QuadrupedShape s, int side) => new Vector3(s.WingSpan * 0.5f * side, 0.04f, 0.02f);

        /// <summary>Adds the wings and the beak of a flyer (nothing for the walkers).</summary>
        public static void Decorate(ProceduralEnemyBlueprint bp, QuadrupedShape s, int body, int head)
        {
            if (s.Beak)
            {
                bp.Mesh(Prefix + "Beak", head, Vector3.zero, b => DrawBeak(b, s));
            }

            if (!s.FlightWings || s.WingSpan <= 0f)
            {
                return;
            }

            for (int side = -1; side <= 1; side += 2)
            {
                int captured = side;
                string x = side < 0 ? "L" : "R";
                int wing = bp.Mesh(side < 0 ? MountParts.WingLeft : MountParts.WingRight, body, WingRoot(s, side), b => DrawInnerWing(b, s, captured),
                    meshKey: "WingInner" + x);
                bp.Mesh(side < 0 ? MountParts.WingTipLeft : MountParts.WingTipRight, wing, WingElbow(s, side), b => DrawOuterWing(b, s, captured),
                    meshKey: "WingOuter" + x);
            }
        }

        private static void DrawBeak(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float size = s.HeadSize;

            // Hooked beak pointing forward and curling down, eyes, a crest of three feathers swept back.
            b.Horn(new Vector3(0f, size * 0.02f, size * 0.42f), 0.07f, 0.2f, 3, 32f, s.Accent, Quaternion.Euler(90f, 0f, 0f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(new Vector3(size * 0.33f * side, size * 0.14f, size * 0.22f), new Vector3(0.035f, 0.045f, 0.045f), EnemySwatch.Black);
            }

            for (int k = -1; k <= 1; k++)
            {
                b.Box(new Vector3(size * 0.12f * k, size * 0.42f, -size * 0.32f), new Vector3(0.04f, 0.03f, 0.2f), s.Mane,
                    Quaternion.Euler(-28f, 14f * k, 0f));
            }
        }

        /// <summary>Inner half: the arm along the leading edge, the covert panel and a row of secondaries.</summary>
        private static void DrawInnerWing(EnemyMeshBuilder b, QuadrupedShape s, int side)
        {
            float half = s.WingSpan * 0.5f;
            Vector3 elbow = WingElbow(s, side);
            b.Segment(Vector3.zero, elbow, 0.06f, 0.04f, 5, s.Coat);
            Panel(b, new Vector3(0f, 0f, 0.08f), new Vector3(half * side, 0.03f, 0.05f), new Vector3(half * side, 0f, -0.3f),
                new Vector3(0f, 0f, -0.36f), s.Mane);
            Panel(b, new Vector3(0f, 0.012f, 0.06f), new Vector3(half * 0.9f * side, 0.04f, 0.04f), new Vector3(half * 0.9f * side, 0.012f, -0.12f),
                new Vector3(0f, 0.012f, -0.16f), s.Coat);
            for (int k = 0; k < 4; k++)
            {
                float x = half * (0.14f + 0.24f * k) * side;
                b.Box(new Vector3(x, -0.008f, -0.42f), new Vector3(half * 0.22f, 0.02f, 0.24f), k % 2 == 0 ? s.Mane : s.Coat,
                    Quaternion.Euler(0f, 6f * side, 0f));
            }
        }

        /// <summary>Outer half: the hand, its panel and five primaries fanning out, tipped with light.</summary>
        private static void DrawOuterWing(EnemyMeshBuilder b, QuadrupedShape s, int side)
        {
            float half = s.WingSpan * 0.5f;
            b.Segment(Vector3.zero, new Vector3(half * 0.8f * side, 0.02f, -0.04f), 0.04f, 0.02f, 5, s.Coat);
            Panel(b, new Vector3(0f, 0f, 0.05f), new Vector3(half * 0.8f * side, 0.01f, -0.02f), new Vector3(half * 0.9f * side, 0f, -0.25f),
                new Vector3(0f, 0f, -0.3f), s.Mane);
            for (int k = 0; k < 5; k++)
            {
                Quaternion yaw = Quaternion.Euler(0f, (12f + 9f * k) * side, 0f);
                Vector3 root = new Vector3(half * (0.3f + 0.14f * k) * side, -0.006f, -0.2f);
                Vector3 length = yaw * new Vector3(0f, 0f, -0.36f);
                b.Box(root + length * 0.5f, new Vector3(0.075f, 0.02f, 0.36f), s.Mane, yaw);
                b.Box(root + length * 1.02f, new Vector3(0.06f, 0.024f, 0.1f), s.PatternSwatch, yaw);
            }
        }

        /// <summary>A flat feather plate seen from above and below.</summary>
        private static void Panel(EnemyMeshBuilder b, Vector3 a, Vector3 c1, Vector3 c2, Vector3 d, EnemySwatch swatch)
        {
            b.TwoSided(a, c1, c2, swatch);
            b.TwoSided(a, c2, d, swatch);
        }
    }
}
