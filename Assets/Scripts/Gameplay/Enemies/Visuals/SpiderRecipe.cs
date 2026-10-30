using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Aranha da Mata (Floresta), about 0.65 m tall and 1 m across the legs: a dark hairy spider with a
    /// big mossy-striped abdomen, a cluster of glowing red eyes and bone fangs. Eight jointed legs radiate
    /// from the cephalothorax, knees high above the body as real spiders hold them, so the IK bends them
    /// upward. Hierarchy: <c>Body</c> → <c>Abdomen</c> (waist pivot, sways); legs under the root,
    /// <c>Leg_L0..Leg_L3</c>/<c>Leg_R0..Leg_R3</c> front to back (hip pivot, yawed outward) →
    /// <c>Shin_xx</c> (knee) → <c>Foot_xx</c> (tip). All legs share one upper and one lower mesh drawn
    /// along the hip's local +x.
    /// </summary>
    public static class SpiderRecipe
    {
        public const int PairCount = 4;

        private const float HipHeight = 0.34f;
        private static readonly float[] HipZ = { 0.17f, 0.11f, 0.04f, -0.03f };
        /// <summary>Leg direction (deg from straight out, + toward the front).</summary>
        private static readonly float[] Splay = { 50f, 18f, -18f, -50f };

        private static readonly Vector3 Knee = new Vector3(0.24f, 0.16f, 0f);
        private static readonly Vector3 Tip = new Vector3(0.2f, -(HipHeight + 0.16f - 0.01f), 0f);

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, HipHeight, 0.08f), DrawCephalothorax);
            bp.Mesh(ProceduralEnemyParts.Abdomen, body, new Vector3(0f, 0.03f, -0.12f), DrawAbdomen);

            for (int pair = 0; pair < PairCount; pair++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    string id = (side < 0 ? "L" : "R") + pair;
                    // Local +x points along the leg: right legs yaw back from +x, left legs mirror through 180°.
                    float yaw = side > 0 ? -Splay[pair] : 180f + Splay[pair];
                    int hip = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, new Vector3(0.1f * side, HipHeight, HipZ[pair]), DrawUpper,
                        Quaternion.Euler(0f, yaw, 0f), meshKey: "Upper");
                    int knee = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, hip, Knee, DrawLower, meshKey: "Lower");
                    bp.Node(ProceduralEnemyParts.FootPrefix + id, knee, Tip);
                }
            }
        }

        private static void DrawCephalothorax(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.17f, 0.3f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.05f, -0.02f), new Vector3(0.2f, 0.08f, 0.2f), EnemySwatch.BatFurDark, coarse: true);
            // Eye cluster: two big, four small.
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(0.035f * side, 0.06f, 0.135f), new Vector3(0.045f, 0.045f, 0.035f), EnemySwatch.WarEye, coarse: true);
                b.Blob(new Vector3(0.075f * side, 0.045f, 0.115f), new Vector3(0.03f, 0.03f, 0.025f), EnemySwatch.WarEye, coarse: true);
                b.Blob(new Vector3(0.05f * side, 0.085f, 0.105f), new Vector3(0.022f, 0.022f, 0.02f), EnemySwatch.WarEye, coarse: true);
                // Chelicerae and their fangs.
                b.Segment(new Vector3(0.035f * side, -0.02f, 0.13f), new Vector3(0.03f * side, -0.1f, 0.18f), 0.03f, 0.018f, 5, EnemySwatch.Black);
                b.Segment(new Vector3(0.03f * side, -0.1f, 0.18f), new Vector3(0.012f * side, -0.13f, 0.16f), 0.012f, 0.002f, 3, EnemySwatch.Bone);
                // Pedipalps.
                b.Segment(new Vector3(0.07f * side, -0.03f, 0.12f), new Vector3(0.09f * side, -0.08f, 0.22f), 0.016f, 0.012f, 4, EnemySwatch.Shadow);
            }
        }

        private static void DrawAbdomen(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.08f, -0.24f), new Vector3(0.44f, 0.38f, 0.5f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.0f, -0.02f), new Vector3(0.12f, 0.1f, 0.1f), EnemySwatch.Black, coarse: true);
            // Mossy chevrons down the back and a pale mark.
            for (int k = 0; k < 3; k++)
            {
                float z = -0.12f - k * 0.1f;
                float y = 0.26f - k * 0.015f;
                for (int side = -1; side <= 1; side += 2)
                {
                    b.Box(new Vector3(0.05f * side, y, z), new Vector3(0.1f - k * 0.015f, 0.02f, 0.035f), EnemySwatch.CaveMoss,
                        Quaternion.Euler(-12f + k * 8f, 25f * side, 0f));
                }
            }

            b.Blob(new Vector3(0f, 0.25f, -0.06f), new Vector3(0.07f, 0.03f, 0.07f), EnemySwatch.Bone, coarse: true);
            b.Cylinder(new Vector3(0f, 0.02f, -0.48f), 0.03f, 0f, 0.04f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(-90f, 0f, 0f));
        }

        private static void DrawUpper(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.Black, coarse: true);
            b.Segment(Vector3.zero, Knee, 0.028f, 0.024f, 5, EnemySwatch.Shadow);
            b.Blob(Knee, new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Black, coarse: true);
            // A few coarse hairs.
            b.Cylinder(Knee * 0.5f + new Vector3(0f, 0.02f, 0f), 0.01f, 0f, 0.05f, 3, EnemySwatch.BatFurDark, rotation: Quaternion.Euler(0f, 0f, 20f));
        }

        private static void DrawLower(EnemyMeshBuilder b)
        {
            Vector3 mid = Tip * 0.45f;
            b.Segment(Vector3.zero, mid, 0.022f, 0.018f, 5, EnemySwatch.Shadow);
            b.Segment(mid, Tip, 0.018f, 0.004f, 4, EnemySwatch.Black);
        }
    }
}
