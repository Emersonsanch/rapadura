using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Guardião da Ruína (Deserto, chefe), about 3.1 m: a construct of stacked sandstone blocks held
    /// together by turquoise rune light. Its head floats above the shoulders with a gold halo and a
    /// slow ring of rune tablets, and its arms are broken temple columns hovering beside the torso,
    /// each ending in a loose floating chunk. Boss accent: turquoise runes + gold. Hierarchy:
    /// <c>Body</c> → <c>Head</c> → <c>Runes</c>; <c>Arm_L</c>/<c>Arm_R</c> (shoulder pivot) →
    /// <c>Fist_L</c>/<c>Fist_R</c>; <c>Leg_L</c>/<c>Leg_R</c> (hip pivot) under the root → <c>Shin_L</c>/<c>Shin_R</c>
    /// (knee, at the rune band) → <c>Foot_L</c>/<c>Foot_R</c> (pivot at the sole), so the legs step with knee IK.
    /// </summary>
    public static class RuinGuardianRecipe
    {
        public const int RuneCount = 5;

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.05f, 0f), DrawBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.78f, 0.05f), DrawHead);
            bp.Mesh(ProceduralEnemyParts.Runes, head, Vector3.zero, DrawRunes);

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body,
                    new Vector3(1.0f * side, 1.18f, 0.02f), b => DrawColumn(b, side), Quaternion.Euler(-6f, 0f, 5f * side));
                bp.Mesh(right ? ProceduralEnemyParts.FistRight : ProceduralEnemyParts.FistLeft, arm,
                    new Vector3(0f, -1.6f, 0.04f), b => DrawFragment(b, side), Quaternion.Euler(12f, 25f * side, 8f));
                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1,
                    new Vector3(0.36f * side, 1.0f, 0f), b => DrawLeg(b, side));
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg,
                    new Vector3(0f, -0.54f, 0.05f), b => DrawShin(b, side));
                bp.Node(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -0.44f, -0.03f));
            }
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            b.Box(Vector3.zero, new Vector3(0.9f, 0.36f, 0.56f), EnemySwatch.SandstoneDark);
            b.Box(new Vector3(0f, 0.6f, 0f), new Vector3(1.2f, 0.82f, 0.7f), EnemySwatch.Sandstone, Quaternion.Euler(0f, -3f, 0f), EnemySwatch.SandstoneLight);
            b.Box(new Vector3(0f, 0.95f, 0f), new Vector3(1.5f, 0.06f, 0.84f), EnemySwatch.Gold);
            b.Box(new Vector3(0f, 1.17f, 0f), new Vector3(1.45f, 0.4f, 0.8f), EnemySwatch.SandstoneLight, Quaternion.Euler(0f, 2f, 0f));

            // Chipped corners and loose rubble.
            b.Box(new Vector3(0.58f, 1.34f, 0.3f), new Vector3(0.22f, 0.14f, 0.2f), EnemySwatch.SandstoneDark, Quaternion.Euler(10f, 20f, 15f));
            b.Box(new Vector3(-0.5f, 0.24f, 0.32f), new Vector3(0.18f, 0.16f, 0.12f), EnemySwatch.SandstoneDark, Quaternion.Euler(-8f, -15f, 10f));

            // Rune sigil on the chest: a glowing hexagon with a core and three spokes.
            const float z = 0.36f;
            var center = new Vector3(0f, 0.6f, z);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                Vector3 p = center + new Vector3(Mathf.Sin(a), Mathf.Cos(a), 0f) * 0.24f;
                b.Box(p, new Vector3(0.25f, 0.045f, 0.03f), EnemySwatch.Rune, Quaternion.Euler(0f, 0f, -i * 60f));
            }

            b.Blob(center + new Vector3(0f, 0f, 0.01f), new Vector3(0.14f, 0.14f, 0.06f), EnemySwatch.Rune, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                b.Box(center, new Vector3(0.03f, 0.36f, 0.025f), EnemySwatch.Rune, Quaternion.Euler(0f, 0f, i * 60f));
            }

            // Rune seams between the blocks, floating glow where the neck would be.
            b.Box(new Vector3(0f, 0.18f, 0f), new Vector3(1.0f, 0.04f, 0.6f), EnemySwatch.Rune);
            b.Blob(new Vector3(0f, 1.48f, 0.02f), new Vector3(0.16f, 0.12f, 0.16f), EnemySwatch.Rune, coarse: true);
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            b.Box(Vector3.zero, new Vector3(0.56f, 0.5f, 0.5f), EnemySwatch.Sandstone, top: EnemySwatch.SandstoneLight);
            b.Box(new Vector3(0f, 0.13f, 0.02f), new Vector3(0.62f, 0.1f, 0.56f), EnemySwatch.SandstoneDark);
            b.Box(new Vector3(0f, -0.13f, 0.26f), new Vector3(0.22f, 0.045f, 0.03f), EnemySwatch.Shadow);
            b.Box(new Vector3(0f, -0.01f, 0.27f), new Vector3(0.06f, 0.14f, 0.04f), EnemySwatch.SandstoneDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.13f * s, 0.03f, 0.26f), new Vector3(0.13f, 0.06f, 0.03f), EnemySwatch.Rune);
                // Headdress flaps.
                b.Box(new Vector3(0.3f * s, -0.12f, -0.02f), new Vector3(0.06f, 0.42f, 0.4f), EnemySwatch.SandstoneDark, Quaternion.Euler(0f, 0f, 8f * s));
            }

            // Gold crown spikes and a halo disc behind.
            for (int k = -1; k <= 1; k++)
            {
                b.Cylinder(new Vector3(k * 0.17f, 0.25f, 0.05f), 0.06f, 0f, k == 0 ? 0.26f : 0.17f, 4, EnemySwatch.Gold);
            }

            b.Cylinder(new Vector3(0f, 0.08f, -0.3f), 0.42f, 0.42f, 0.04f, 12, EnemySwatch.Gold, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Cylinder(new Vector3(0f, 0.08f, -0.27f), 0.3f, 0.3f, 0.02f, 12, EnemySwatch.Rune, rotation: Quaternion.Euler(90f, 0f, 0f));
        }

        private static void DrawRunes(EnemyMeshBuilder b)
        {
            for (int i = 0; i < RuneCount; i++)
            {
                float angle = i * 360f / RuneCount;
                float a = angle * Mathf.Deg2Rad;
                var p = new Vector3(Mathf.Sin(a) * 0.62f, -0.05f + (i % 2) * 0.12f, Mathf.Cos(a) * 0.62f);
                b.Box(p, new Vector3(0.11f, 0.15f, 0.03f), EnemySwatch.Rune, Quaternion.Euler(0f, angle, 0f));
            }
        }

        private static void DrawColumn(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));

            // Floating shoulder block, then the capital, the fluted shaft and its broken end.
            b.Box(new Vector3(0f, 0.02f, 0f), new Vector3(0.36f, 0.36f, 0.36f), EnemySwatch.SandstoneDark, Quaternion.Euler(0f, 45f, 20f));
            b.Box(new Vector3(0f, -0.3f, 0f), new Vector3(0.46f, 0.15f, 0.46f), EnemySwatch.SandstoneLight);
            b.Box(new Vector3(0f, -0.4f, 0f), new Vector3(0.38f, 0.06f, 0.38f), EnemySwatch.Gold);
            b.Cylinder(new Vector3(0f, -1.22f, 0f), 0.16f, 0.17f, 0.8f, 10, EnemySwatch.Sandstone);
            b.Cylinder(new Vector3(0f, -0.82f, 0f), 0.175f, 0.175f, 0.04f, 10, EnemySwatch.Rune);
            float[] lengths = { 0.22f, 0.12f, 0.17f, 0.09f };
            for (int k = 0; k < lengths.Length; k++)
            {
                float a = k * 90f * Mathf.Deg2Rad + 0.4f;
                b.Cylinder(new Vector3(Mathf.Sin(a) * 0.08f, -1.2f, Mathf.Cos(a) * 0.08f), 0.09f, 0f, lengths[k], 4,
                    EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(180f, 0f, 0f));
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawFragment(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Box(Vector3.zero, new Vector3(0.3f, 0.26f, 0.3f), EnemySwatch.Sandstone, top: EnemySwatch.SandstoneLight);
            b.Box(new Vector3(0f, 0f, 0.155f), new Vector3(0.2f, 0.04f, 0.02f), EnemySwatch.Rune);
            b.Box(new Vector3(0.08f, -0.17f, 0.02f), new Vector3(0.14f, 0.1f, 0.14f), EnemySwatch.SandstoneDark, Quaternion.Euler(20f, 30f, 10f));
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawLeg(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Box(new Vector3(0f, -0.26f, 0f), new Vector3(0.42f, 0.5f, 0.46f), EnemySwatch.Sandstone, Quaternion.Euler(0f, 4f, 0f));
            b.Box(new Vector3(0f, -0.52f, 0f), new Vector3(0.45f, 0.05f, 0.49f), EnemySwatch.Rune);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawShin(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Box(new Vector3(0f, -0.24f, 0.01f), new Vector3(0.52f, 0.44f, 0.62f), EnemySwatch.SandstoneDark, top: EnemySwatch.Sandstone);
            b.Matrix = Matrix4x4.identity;
        }
    }
}
