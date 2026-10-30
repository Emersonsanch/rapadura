using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Lobo da Mata (Floresta), about 1 m to the ear tips and 1.3 m long: a lean grey wolf with a darker
    /// saddle and mane, pale belly and muzzle, amber glowing eyes and a white-tipped bushy tail. Built
    /// from jointed parts so <see cref="Rapadura.Gameplay.Locomotion.ProceduralLegLocomotion"/> can step
    /// it: <c>Body</c> → <c>Head</c> → <c>Ear_L</c>/<c>Ear_R</c>; <c>Body</c> → <c>Tail</c> → <c>Tail_1</c>;
    /// four legs under the root, each <c>Leg_xx</c> (shoulder/hip pivot) → <c>Shin_xx</c> (elbow/hock pivot)
    /// → <c>Foot_xx</c> (paw), xx = FL/FR/HL/HR. The rest pose bends every leg slightly backward (elbows,
    /// hocks), which is the bend the IK keeps. Lobo Alfa (<paramref name="alpha"/>) is the same wolf ×1.35,
    /// near-black with a blood-red mane, a bone scar and red eyes (boss accent).
    /// </summary>
    public static class WolfRecipe
    {
        public const float AlphaScale = 1.35f;

        private struct Coat
        {
            public EnemySwatch Fur, Saddle, Mane, Belly, Eye;
        }

        public static void Build(ProceduralEnemyBlueprint bp, bool alpha)
        {
            float s = alpha ? AlphaScale : 1f;
            Coat coat = alpha
                ? new Coat { Fur = EnemySwatch.IronDark, Saddle = EnemySwatch.Shadow, Mane = EnemySwatch.WarCape, Belly = EnemySwatch.Iron, Eye = EnemySwatch.WarEye }
                : new Coat { Fur = EnemySwatch.Iron, Saddle = EnemySwatch.IronDark, Mane = EnemySwatch.IronDark, Belly = EnemySwatch.Steel, Eye = EnemySwatch.BatEye };

            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f) * s, b => DrawBody(b, s, coat, alpha));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.5f) * s, b => DrawHead(b, s, coat, alpha), Quaternion.Euler(8f, 0f, 0f));
            for (int side = -1; side <= 1; side += 2)
            {
                bp.Mesh(side < 0 ? ProceduralEnemyParts.EarLeft : ProceduralEnemyParts.EarRight, head,
                    new Vector3(0.075f * side, 0.12f, 0.0f) * s, b => DrawEar(b, s, coat), Quaternion.Euler(-12f, 0f, -16f * side), meshKey: "Ear");
            }

            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.07f, -0.47f) * s, b => DrawTailBase(b, s, coat));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, -0.08f, -0.2f) * s, b => DrawTailTip(b, s, coat));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                // Front: shoulder → elbow (behind) → paw.
                int front = bp.Mesh(ProceduralEnemyParts.LegPrefix + "F" + x, -1, new Vector3(0.12f * side, 0.5f, 0.3f) * s,
                    b => DrawUpper(b, s, coat, new Vector3(0f, -0.24f, -0.04f), 0.07f, 0.045f, false), meshKey: "FrontUpper");
                int frontShin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + "F" + x, front, new Vector3(0f, -0.24f, -0.04f) * s,
                    b => DrawLower(b, s, coat, new Vector3(0f, -0.21f, 0.04f), 0.042f, 0.03f), meshKey: "FrontLower");
                bp.Mesh(ProceduralEnemyParts.FootPrefix + "F" + x, frontShin, new Vector3(0f, -0.21f, 0.04f) * s, b => DrawPaw(b, s, coat), meshKey: "Paw");

                // Hind: hip → hock (well behind) → paw.
                int hind = bp.Mesh(ProceduralEnemyParts.LegPrefix + "H" + x, -1, new Vector3(0.12f * side, 0.52f, -0.36f) * s,
                    b => DrawUpper(b, s, coat, new Vector3(0f, -0.3f, -0.08f), 0.068f, 0.038f, true), meshKey: "HindUpper");
                int hindShin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + "H" + x, hind, new Vector3(0f, -0.3f, -0.08f) * s,
                    b => DrawLower(b, s, coat, new Vector3(0f, -0.17f, 0.05f), 0.036f, 0.03f), meshKey: "HindLower");
                bp.Mesh(ProceduralEnemyParts.FootPrefix + "H" + x, hindShin, new Vector3(0f, -0.17f, 0.05f) * s, b => DrawPaw(b, s, coat), meshKey: "Paw");
            }
        }

        private static void DrawBody(EnemyMeshBuilder b, float s, Coat c, bool alpha)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Blob(new Vector3(0f, 0f, -0.06f), new Vector3(0.32f, 0.32f, 0.86f), c.Fur);
            b.Blob(new Vector3(0f, -0.02f, 0.26f), new Vector3(0.36f, 0.4f, 0.42f), c.Fur);
            b.Blob(new Vector3(0f, 0.1f, -0.1f), new Vector3(0.26f, 0.15f, 0.66f), c.Saddle);
            b.Blob(new Vector3(0f, -0.11f, 0.06f), new Vector3(0.22f, 0.16f, 0.56f), c.Belly);
            // Haunches and shoulder blades the legs plug into.
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(0.1f * side, -0.06f, -0.36f), new Vector3(0.16f, 0.26f, 0.26f), c.Fur, coarse: true);
                b.Blob(new Vector3(0.1f * side, -0.08f, 0.3f), new Vector3(0.14f, 0.24f, 0.2f), c.Fur, coarse: true);
            }

            // Mane over the neck and chest.
            b.Blob(new Vector3(0f, 0.08f, 0.4f), new Vector3(0.32f, 0.34f, 0.3f), c.Mane);
            b.Blob(new Vector3(0f, -0.1f, 0.42f), new Vector3(0.22f, 0.22f, 0.18f), c.Belly, coarse: true);
            if (alpha)
            {
                // Spiky blood-red ruff along the spine.
                for (int k = 0; k < 4; k++)
                {
                    b.Cylinder(new Vector3(0f, 0.14f, 0.36f - k * 0.14f), 0.045f, 0f, 0.12f - k * 0.015f, 4, EnemySwatch.WarCape,
                        rotation: Quaternion.Euler(-35f, 0f, 0f));
                }
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawHead(EnemyMeshBuilder b, float s, Coat c, bool alpha)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Blob(new Vector3(0f, 0.02f, 0.04f), new Vector3(0.24f, 0.22f, 0.26f), c.Fur);
            b.Blob(new Vector3(0f, 0.08f, 0.0f), new Vector3(0.2f, 0.1f, 0.2f), c.Saddle, coarse: true);
            // Muzzle (toward +z), pale underside, black nose, fangs.
            b.Cylinder(new Vector3(0f, -0.03f, 0.12f), 0.075f, 0.045f, 0.2f, 6, c.Fur, c.Belly, Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.065f, 0.2f), new Vector3(0.1f, 0.05f, 0.17f), c.Belly, coarse: true);
            b.Blob(new Vector3(0f, -0.01f, 0.325f), new Vector3(0.06f, 0.05f, 0.05f), EnemySwatch.Black, coarse: true);
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(0.065f * side, 0.05f, 0.14f), new Vector3(0.045f, 0.035f, 0.03f), c.Eye, coarse: true);
                b.Box(new Vector3(0.06f * side, 0.08f, 0.13f), new Vector3(0.07f, 0.02f, 0.04f), c.Saddle, Quaternion.Euler(0f, 0f, -18f * side));
                b.Cylinder(new Vector3(0.03f * side, -0.07f, 0.27f), 0.012f, 0f, 0.045f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            }

            if (alpha)
            {
                // A pale scar across the left eye.
                b.Box(new Vector3(-0.07f, 0.06f, 0.15f), new Vector3(0.012f, 0.1f, 0.012f), EnemySwatch.Bone, Quaternion.Euler(0f, 0f, 25f));
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawEar(EnemyMeshBuilder b, float s, Coat c)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Cylinder(Vector3.zero, 0.05f, 0f, 0.13f, 4, c.Saddle);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawTailBase(EnemyMeshBuilder b, float s, Coat c)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Segment(Vector3.zero, new Vector3(0f, -0.08f, -0.2f), 0.045f, 0.07f, 6, c.Fur);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawTailTip(EnemyMeshBuilder b, float s, Coat c)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Blob(new Vector3(0f, -0.04f, -0.08f), new Vector3(0.13f, 0.13f, 0.22f), c.Fur);
            b.Blob(new Vector3(0f, -0.08f, -0.18f), new Vector3(0.09f, 0.09f, 0.12f), EnemySwatch.Bone, coarse: true);
            b.Matrix = Matrix4x4.identity;
        }

        /// <summary>Upper leg from the pivot down to <paramref name="knee"/> (local), thick at the top (thigh for hind legs).</summary>
        private static void DrawUpper(EnemyMeshBuilder b, float s, Coat c, Vector3 knee, float topRadius, float bottomRadius, bool thigh)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Segment(knee, new Vector3(0f, 0.04f, 0f), bottomRadius, topRadius, 6, c.Fur);
            if (thigh)
            {
                b.Blob(new Vector3(0f, -0.07f, -0.01f), new Vector3(0.13f, 0.24f, 0.19f), c.Fur, coarse: true);
            }

            b.Blob(knee, Vector3.one * (bottomRadius * 2.1f), c.Fur, coarse: true);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawLower(EnemyMeshBuilder b, float s, Coat c, Vector3 paw, float topRadius, float bottomRadius)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Segment(paw, Vector3.zero, bottomRadius, topRadius, 5, c.Saddle);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawPaw(EnemyMeshBuilder b, float s, Coat c)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * s);
            b.Blob(new Vector3(0f, -0.015f, 0.03f), new Vector3(0.075f, 0.05f, 0.11f), c.Saddle, coarse: true);
            b.Matrix = Matrix4x4.identity;
        }
    }
}
