using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Senhor da Guerra (Montanha, chefe), about 2.6 m to the horn tips: an armoured humanoid in dark
    /// iron with a horned helmet, a red-crested visor with burning eyes, a fur collar, a long red cape
    /// and a double-bitted greataxe in the right hand. Boss accent: red cape/crest + gold trim.
    /// Hierarchy: <c>Body</c> (waist pivot) → <c>Head</c>, <c>Cape</c> (neck pivot), <c>Arm_L</c>/<c>Arm_R</c>
    /// (shoulder pivot) → <c>Weapon</c> (in the right hand); <c>Leg_L</c>/<c>Leg_R</c> (hip pivot) under the root.
    /// </summary>
    public static class WarlordRecipe
    {
        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.12f, 0f), DrawBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.8f, 0.03f), DrawHead);
            bp.Mesh(ProceduralEnemyParts.Cape, body, new Vector3(0f, 0.7f, -0.27f), DrawCape, Quaternion.Euler(6f, 0f, 0f));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body,
                    new Vector3(0.5f * side, 0.62f, 0f), b => DrawArm(b, side), Quaternion.Euler(-10f, 0f, 6f * side));
                if (right)
                {
                    bp.Mesh(ProceduralEnemyParts.Weapon, arm, new Vector3(0.06f, -0.86f, 0.14f), DrawAxe, Quaternion.Euler(18f, 0f, 0f));
                }

                bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1,
                    new Vector3(0.2f * side, 1.06f, 0f), b => DrawLeg(b, side));
            }
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            // Belt, buckle and the plates hanging from it.
            b.Cylinder(new Vector3(0f, -0.06f, 0f), 0.31f, 0.31f, 0.15f, 8, EnemySwatch.Leather, twist: 22.5f);
            b.Box(new Vector3(0f, 0.01f, 0.31f), new Vector3(0.15f, 0.13f, 0.05f), EnemySwatch.Gold);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.14f * s, -0.24f, 0.22f), new Vector3(0.22f, 0.32f, 0.05f), EnemySwatch.Iron, Quaternion.Euler(-12f, 0f, 0f));
                b.Box(new Vector3(0.29f * s, -0.22f, 0f), new Vector3(0.05f, 0.3f, 0.3f), EnemySwatch.IronDark, Quaternion.Euler(0f, 0f, -10f * s));
            }

            b.Box(new Vector3(0f, -0.24f, -0.22f), new Vector3(0.42f, 0.32f, 0.05f), EnemySwatch.IronDark, Quaternion.Euler(12f, 0f, 0f));

            // Breastplate widening to the shoulders, ridge, gold emblem, gorget.
            b.Cylinder(new Vector3(0f, 0.08f, 0f), 0.3f, 0.45f, 0.62f, 8, EnemySwatch.Iron, EnemySwatch.IronDark, twist: 22.5f);
            b.Box(new Vector3(0f, 0.38f, 0.36f), new Vector3(0.1f, 0.52f, 0.1f), EnemySwatch.Steel, Quaternion.Euler(-14f, 0f, 0f));
            b.Box(new Vector3(0f, 0.46f, 0.4f), new Vector3(0.18f, 0.18f, 0.05f), EnemySwatch.Gold, Quaternion.Euler(-14f, 0f, 45f));
            b.Cylinder(new Vector3(0f, 0.68f, 0f), 0.42f, 0.42f, 0.04f, 8, EnemySwatch.Gold, twist: 22.5f);
            b.Cylinder(new Vector3(0f, 0.7f, 0f), 0.24f, 0.18f, 0.13f, 8, EnemySwatch.IronDark);

            // Fur collar over the shoulders.
            b.Blob(new Vector3(0f, 0.72f, -0.06f), new Vector3(0.8f, 0.22f, 0.5f), EnemySwatch.Leather, coarse: true);
            b.Blob(new Vector3(0f, 0.75f, -0.14f), new Vector3(0.62f, 0.2f, 0.36f), EnemySwatch.Shadow, coarse: true);
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.2f, 0.19f, 0.25f, 8, EnemySwatch.Iron, twist: 22.5f);
            b.Blob(new Vector3(0f, 0.25f, 0f), new Vector3(0.39f, 0.26f, 0.39f), EnemySwatch.Iron);
            b.Box(new Vector3(0f, 0.05f, 0.18f), new Vector3(0.27f, 0.12f, 0.07f), EnemySwatch.IronDark);
            b.Box(new Vector3(0f, 0.14f, 0.19f), new Vector3(0.3f, 0.05f, 0.05f), EnemySwatch.Black);
            b.Box(new Vector3(0f, 0.21f, 0.19f), new Vector3(0.3f, 0.04f, 0.04f), EnemySwatch.Gold);
            b.Box(new Vector3(0f, 0.08f, 0.22f), new Vector3(0.04f, 0.16f, 0.04f), EnemySwatch.Steel);

            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.075f * s, 0.14f, 0.215f), new Vector3(0.075f, 0.035f, 0.02f), EnemySwatch.WarEye, Quaternion.Euler(0f, 0f, -10f * s));

                // Bull horns: out to the side, curling up and in.
                b.Horn(new Vector3(0.17f * s, 0.26f, 0f), 0.07f, 0.56f, 4, 26f * s, EnemySwatch.Horn,
                    Quaternion.Euler(0f, 0f, -72f * s), 6, Vector3.forward);
            }

            // Red crest along the top.
            b.Box(new Vector3(0f, 0.4f, -0.03f), new Vector3(0.06f, 0.13f, 0.38f), EnemySwatch.WarCape);
            b.Box(new Vector3(0f, 0.32f, -0.24f), new Vector3(0.06f, 0.22f, 0.12f), EnemySwatch.WarCape, Quaternion.Euler(-30f, 0f, 0f));
        }

        private static void DrawCape(EnemyMeshBuilder b)
        {
            // A folded slab: columns alternate in/out, rows fan out and sweep back toward the hem.
            float[] xs = { -1f, -0.5f, 0f, 0.5f, 1f };
            float[] folds = { 0f, -0.07f, 0f, -0.07f, 0f };
            float[] ys = { 0f, -0.55f, -1.1f, -1.55f };
            float[] widths = { 0.3f, 0.42f, 0.5f, 0.55f };
            float[] backs = { 0f, -0.08f, -0.18f, -0.26f };

            Vector3 P(int column, int row) => new Vector3(xs[column] * widths[row], ys[row], backs[row] + folds[column]);
            for (int row = 0; row < ys.Length - 1; row++)
            {
                for (int column = 0; column < xs.Length - 1; column++)
                {
                    Vector3 a = P(column, row), c = P(column + 1, row), d = P(column + 1, row + 1), e = P(column, row + 1);
                    b.TwoSided(a, c, d, EnemySwatch.WarCape);
                    b.TwoSided(a, d, e, EnemySwatch.WarCape);
                }
            }

            b.Box(new Vector3(0f, -1.56f, -0.26f), new Vector3(1.1f, 0.05f, 0.06f), EnemySwatch.Gold);
        }

        private static void DrawArm(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));

            // Spiked pauldron with a gold rim.
            b.Blob(new Vector3(0.05f, 0.04f, 0f), new Vector3(0.4f, 0.3f, 0.4f), EnemySwatch.Iron);
            b.Cylinder(new Vector3(0.05f, -0.06f, 0f), 0.21f, 0.21f, 0.04f, 8, EnemySwatch.Gold);
            for (int k = 0; k < 3; k++)
            {
                b.Cylinder(new Vector3(0.1f, 0.14f, -0.1f + k * 0.1f), 0.04f, 0f, 0.16f, 4, EnemySwatch.Steel,
                    rotation: Quaternion.Euler(0f, 0f, -25f));
            }

            // Upper arm, elbow, gauntlet, fist.
            b.Cylinder(new Vector3(0.04f, -0.45f, 0f), 0.09f, 0.11f, 0.42f, 6, EnemySwatch.IronDark);
            b.Blob(new Vector3(0.04f, -0.47f, 0f), new Vector3(0.18f, 0.16f, 0.18f), EnemySwatch.Steel, coarse: true);
            b.Cylinder(new Vector3(0.05f, -0.82f, 0.05f), 0.1f, 0.13f, 0.36f, 6, EnemySwatch.Iron, rotation: Quaternion.Euler(10f, 0f, 0f));
            b.Cylinder(new Vector3(0.05f, -0.52f, 0.1f), 0.135f, 0.135f, 0.04f, 6, EnemySwatch.Gold, rotation: Quaternion.Euler(10f, 0f, 0f));
            b.Blob(new Vector3(0.06f, -0.88f, 0.12f), new Vector3(0.17f, 0.17f, 0.17f), EnemySwatch.Leather, coarse: true);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawAxe(EnemyMeshBuilder b)
        {
            // Haft through the fist (pivot), pommel below, spike on top.
            b.Cylinder(new Vector3(0f, -0.55f, 0f), 0.04f, 0.035f, 1.95f, 6, EnemySwatch.Leather);
            b.Blob(new Vector3(0f, -0.57f, 0f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.Gold, coarse: true);
            b.Cylinder(new Vector3(0f, 1.38f, 0f), 0.05f, 0f, 0.26f, 4, EnemySwatch.Steel);
            b.Cylinder(new Vector3(0f, 1.0f, 0f), 0.065f, 0.065f, 0.4f, 6, EnemySwatch.IronDark);
            b.Cylinder(new Vector3(0f, 1.0f, 0f), 0.07f, 0.07f, 0.04f, 6, EnemySwatch.Gold);
            b.Cylinder(new Vector3(0f, 1.36f, 0f), 0.07f, 0.07f, 0.04f, 6, EnemySwatch.Gold);

            // Double-bitted head: a big crescent to the front, a smaller one behind.
            b.Plate(new[]
            {
                new Vector2(0.05f, 1.02f), new Vector2(0.3f, 0.86f), new Vector2(0.46f, 1.06f),
                new Vector2(0.5f, 1.2f), new Vector2(0.46f, 1.36f), new Vector2(0.32f, 1.56f), new Vector2(0.05f, 1.38f),
            }, 0.025f, EnemySwatch.Steel, EnemySwatch.IronDark);
            b.Plate(new[]
            {
                new Vector2(-0.05f, 1.38f), new Vector2(-0.24f, 1.48f), new Vector2(-0.34f, 1.3f),
                new Vector2(-0.34f, 1.1f), new Vector2(-0.24f, 0.94f), new Vector2(-0.05f, 1.02f),
            }, 0.022f, EnemySwatch.Steel, EnemySwatch.IronDark);
        }

        private static void DrawLeg(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Cylinder(new Vector3(0f, -0.5f, 0f), 0.13f, 0.16f, 0.52f, 6, EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, -0.52f, 0.09f), new Vector3(0.2f, 0.17f, 0.14f), EnemySwatch.Steel, coarse: true);
            b.Cylinder(new Vector3(0f, -0.94f, 0f), 0.12f, 0.15f, 0.44f, 6, EnemySwatch.Iron);
            b.Box(new Vector3(0f, -0.99f, 0.06f), new Vector3(0.25f, 0.15f, 0.38f), EnemySwatch.IronDark, top: EnemySwatch.Iron);
            b.Matrix = Matrix4x4.identity;
        }
    }
}
