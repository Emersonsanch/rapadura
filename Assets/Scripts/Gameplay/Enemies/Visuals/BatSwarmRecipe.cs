using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Enxame de Morcegos (Cavernas): six small purple bats circling one centre. Hierarchy:
    /// <c>Swarm</c> (orbit pivot) → <c>Bat_0..Bat_5</c> (body mesh, shared) → <c>Wing_L</c>/<c>Wing_R</c>
    /// (pivot at the shoulder, flapping around the bat's forward axis). Ember-orange eyes glow.
    /// </summary>
    public static class BatSwarmRecipe
    {
        public const int BatCount = 6;

        // Fixed (not random) so the swarm is identical on every build.
        private static readonly float[] Angles = { 0f, 58f, 125f, 180f, 238f, 300f };
        private static readonly float[] Radii = { 0.62f, 0.42f, 0.66f, 0.5f, 0.7f, 0.38f };
        private static readonly float[] Heights = { 0.12f, -0.22f, 0.3f, -0.05f, -0.28f, 0.22f };
        private static readonly float[] Scales = { 1.15f, 0.9f, 1f, 1.25f, 0.85f, 1.05f };
        private static readonly float[] Banks = { -14f, -6f, -10f, -16f, -8f, -4f };

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int swarm = bp.Node(ProceduralEnemyParts.Swarm, -1, new Vector3(0f, 1.05f, 0f));
            for (int i = 0; i < BatCount; i++)
            {
                float angle = Angles[i] * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Sin(angle) * Radii[i], Heights[i], Mathf.Cos(angle) * Radii[i]);

                // Faces along the orbit (clockwise seen from above, the way ProceduralEnemyRig spins the swarm), banked into the turn.
                float yaw = Angles[i] + 90f;
                Quaternion rotation = Quaternion.Euler(0f, yaw, Banks[i]);

                int bat = bp.Mesh(ProceduralEnemyParts.BatPrefix + i, swarm, position, DrawBody, rotation,
                    Vector3.one * Scales[i], "BatBody");
                bp.Mesh(ProceduralEnemyParts.WingLeft, bat, new Vector3(-0.08f, 0.03f, 0f), b => DrawWing(b, -1f),
                    Quaternion.Euler(0f, 0f, -12f), meshKey: "BatWing_L");
                bp.Mesh(ProceduralEnemyParts.WingRight, bat, new Vector3(0.08f, 0.03f, 0f), b => DrawWing(b, 1f),
                    Quaternion.Euler(0f, 0f, 12f), meshKey: "BatWing_R");
            }
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.3f), EnemySwatch.BatFur);
            b.Blob(new Vector3(0f, -0.04f, 0.03f), new Vector3(0.16f, 0.14f, 0.22f), EnemySwatch.BatFurDark, coarse: true);
            b.Blob(new Vector3(0f, 0.06f, 0.17f), new Vector3(0.17f, 0.15f, 0.15f), EnemySwatch.BatFur, coarse: true);

            for (int s = -1; s <= 1; s += 2)
            {
                // Big pointed ears, tips leaning out.
                b.Cylinder(new Vector3(0.045f * s, 0.11f, 0.15f), 0.04f, 0f, 0.12f, 4, EnemySwatch.BatWing,
                    rotation: Quaternion.Euler(-10f, 0f, -22f * s));
                // Glowing eyes and two tiny fangs.
                b.Blob(new Vector3(0.042f * s, 0.075f, 0.235f), new Vector3(0.045f, 0.045f, 0.03f), EnemySwatch.BatEye, coarse: true);
                b.Cylinder(new Vector3(0.022f * s, 0.025f, 0.235f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang,
                    rotation: Quaternion.Euler(180f, 0f, 0f));
                // Little feet tucked under.
                b.Cylinder(new Vector3(0.04f * s, -0.09f, -0.06f), 0.015f, 0.008f, 0.06f, 3, EnemySwatch.BatFurDark,
                    rotation: Quaternion.Euler(150f, 0f, 0f));
            }
        }

        /// <summary>Membrane with three finger points, in the bat's horizontal plane; <paramref name="side"/> +1 = right (+x).</summary>
        private static void DrawWing(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            var root0 = new Vector3(0f, 0f, 0.06f);
            var root1 = new Vector3(0f, 0f, -0.08f);
            var elbow = new Vector3(0.2f, 0.035f, 0.05f);
            var tip = new Vector3(0.44f, 0.01f, -0.02f);
            var f1 = new Vector3(0.37f, -0.01f, -0.13f);
            var f2 = new Vector3(0.24f, 0f, -0.15f);
            var f3 = new Vector3(0.11f, 0f, -0.13f);

            b.TwoSided(root0, elbow, f3, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(root0, f3, root1, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(elbow, tip, f1, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(elbow, f1, f2, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(elbow, f2, f3, EnemySwatch.BatWing, EnemySwatch.BatFurDark);

            // Bony leading edge.
            Bone(b, root0, elbow);
            Bone(b, elbow, tip);
            b.Matrix = Matrix4x4.identity;
        }

        private static void Bone(EnemyMeshBuilder b, Vector3 from, Vector3 to)
        {
            Vector3 d = to - from;
            b.Cylinder(from, 0.014f, 0.009f, d.magnitude, 3, EnemySwatch.BatFurDark, rotation: Quaternion.FromToRotation(Vector3.up, d));
        }
    }
}
