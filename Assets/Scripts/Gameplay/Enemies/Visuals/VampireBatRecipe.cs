using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Morcego-vampiro gigante (Cavernas, night): one big maroon bat instead of a swarm, wings spread about 2.2 m —
    /// the wide "M" of its wings is the silhouette that reads in the dark, with two glowing red eyes, huge ears, a
    /// pink nose-leaf and two cartoon fangs. Hangs its little feet under it (pivot at the feet, y = 0; the
    /// Flyer archetype lifts it into its hover). Hierarchy: <c>Body</c> (body, head, ears, feet) →
    /// <c>Wing_L</c>/<c>Wing_R</c> (pivot at the shoulder, flapped by <see cref="ProceduralEnemyRig"/> around the
    /// bat's forward axis, slower and heavier than the swarm's).
    /// </summary>
    public static class VampireBatRecipe
    {
        public const float WingReach = 1.05f;

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.55f, 0f), DrawBody);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.17f, 0.08f, 0f), b => DrawWing(b, -1f),
                Quaternion.Euler(0f, 0f, -14f), meshKey: "VampWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.17f, 0.08f, 0f), b => DrawWing(b, 1f),
                Quaternion.Euler(0f, 0f, 14f), meshKey: "VampWing_R");
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.46f, 0.56f), EnemySwatch.VampFur);
            b.Blob(new Vector3(0f, -0.08f, 0.06f), new Vector3(0.3f, 0.3f, 0.4f), EnemySwatch.VampWing, coarse: true);
            // Shaggy ruff around the neck.
            b.Blob(new Vector3(0f, 0.12f, 0.16f), new Vector3(0.36f, 0.28f, 0.24f), EnemySwatch.BatFurDark, coarse: true);

            // Head: round, with a pug nose-leaf, big eyes, huge ears.
            b.Blob(new Vector3(0f, 0.17f, 0.3f), new Vector3(0.3f, 0.28f, 0.28f), EnemySwatch.VampFur);
            b.Blob(new Vector3(0f, 0.13f, 0.45f), new Vector3(0.1f, 0.08f, 0.05f), EnemySwatch.Pink, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.09f * s, 0.28f, 0.27f), 0.08f, 0f, 0.26f, 4, EnemySwatch.VampWing,
                    rotation: Quaternion.Euler(-12f, 0f, -24f * s));
                b.Cylinder(new Vector3(0.09f * s, 0.29f, 0.29f), 0.04f, 0f, 0.17f, 3, EnemySwatch.Pink,
                    rotation: Quaternion.Euler(-12f, 0f, -24f * s));
                b.Blob(new Vector3(0.075f * s, 0.21f, 0.42f), new Vector3(0.08f, 0.075f, 0.04f), EnemySwatch.WarEye, coarse: true);
                // Two cartoon fangs, more silly than scary.
                b.Cylinder(new Vector3(0.035f * s, 0.07f, 0.42f), 0.02f, 0f, 0.07f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                // Little feet with hooked toes hanging under the body: the lowest point of the model.
                b.Segment(new Vector3(0.08f * s, -0.18f, -0.08f), new Vector3(0.09f * s, -0.5f, -0.12f), 0.035f, 0.02f, 4, EnemySwatch.BatFurDark);
                b.Cylinder(new Vector3(0.09f * s, -0.52f, -0.12f), 0.03f, 0f, 0.06f, 3, EnemySwatch.Bone, rotation: Quaternion.Euler(120f, 0f, 0f));
            }

            // Stubby tail membrane between the feet.
            b.TwoSided(new Vector3(-0.09f, -0.2f, -0.12f), new Vector3(0.09f, -0.2f, -0.12f), new Vector3(0f, -0.36f, -0.3f),
                EnemySwatch.VampWing, EnemySwatch.BatFurDark);
        }

        /// <summary>Membrane with four finger points in the bat's horizontal plane; <paramref name="side"/> +1 = right (+x).</summary>
        private static void DrawWing(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            var root0 = new Vector3(0f, 0f, 0.14f);
            var root1 = new Vector3(0f, 0f, -0.2f);
            var elbow = new Vector3(0.42f, 0.09f, 0.12f);
            var tip = new Vector3(WingReach, 0.02f, -0.04f);
            var f1 = new Vector3(0.9f, -0.02f, -0.3f);
            var f2 = new Vector3(0.66f, 0f, -0.38f);
            var f3 = new Vector3(0.42f, 0f, -0.36f);
            var f4 = new Vector3(0.2f, 0f, -0.3f);

            const EnemySwatch top = EnemySwatch.VampWing;
            const EnemySwatch under = EnemySwatch.VampFur;
            b.TwoSided(root0, elbow, f4, top, under);
            b.TwoSided(root0, f4, root1, top, under);
            b.TwoSided(elbow, f3, f4, top, under);
            b.TwoSided(elbow, f2, f3, top, under);
            b.TwoSided(elbow, f1, f2, top, under);
            b.TwoSided(elbow, tip, f1, top, under);

            Bone(b, root0, elbow, 0.03f);
            Bone(b, elbow, tip, 0.022f);
            Bone(b, elbow, f1, 0.014f);
            Bone(b, elbow, f2, 0.014f);
            Bone(b, elbow, f3, 0.014f);
            // Thumb claw on the leading edge.
            b.Cylinder(elbow, 0.022f, 0f, 0.09f, 3, EnemySwatch.Bone, rotation: Quaternion.Euler(0f, 0f, -20f));
            b.Matrix = Matrix4x4.identity;
        }

        private static void Bone(EnemyMeshBuilder b, Vector3 from, Vector3 to, float radius)
        {
            Vector3 d = to - from;
            b.Cylinder(from, radius, radius * 0.6f, d.magnitude, 4, EnemySwatch.BatFurDark, rotation: Quaternion.FromToRotation(Vector3.up, d));
        }
    }
}
