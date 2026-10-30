using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Porco-do-mato da Caipora (queixada; Floresta, night, comes in herds of 2–3), 0.7 m to the top of its crest and
    /// 1.1 m long: a stocky, dark bristly pig with a big wedge head carried low for the charge, a pink snout disc,
    /// little tusks, glowing ember-orange eyes and the white jaw ("queixo branco") that gives the queixada its name —
    /// a pale stripe low on the silhouette, plus a spiky crest of bristles along the back. Jointed like the wolf for
    /// the stepping legs (short trotting legs, black hooves): <c>Body</c> → <c>Head</c> → <c>Ear_L</c>/<c>Ear_R</c>;
    /// <c>Body</c> → <c>Tail</c>; <c>Leg_FL/FR/HL/HR</c> → <c>Shin_</c> → <c>Foot_</c> under the root.
    /// </summary>
    public static class PeccaryRecipe
    {
        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.0f, 0.34f), DrawHead, Quaternion.Euler(18f, 0f, 0f));
            for (int side = -1; side <= 1; side += 2)
            {
                bp.Mesh(side < 0 ? ProceduralEnemyParts.EarLeft : ProceduralEnemyParts.EarRight, head,
                    new Vector3(0.07f * side, 0.1f, -0.02f), DrawEar, Quaternion.Euler(-20f, 0f, -25f * side), meshKey: "Ear");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.36f), DrawTail);

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                int front = bp.Mesh(ProceduralEnemyParts.LegPrefix + "F" + x, -1, new Vector3(0.1f * side, 0.36f, 0.24f),
                    b => DrawUpper(b, new Vector3(0f, -0.16f, -0.02f), 0.055f, 0.035f), meshKey: "FrontUpper");
                int frontShin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + "F" + x, front, new Vector3(0f, -0.16f, -0.02f),
                    b => DrawLower(b, new Vector3(0f, -0.15f, 0.02f)), meshKey: "Lower");
                bp.Mesh(ProceduralEnemyParts.FootPrefix + "F" + x, frontShin, new Vector3(0f, -0.15f, 0.02f), DrawHoof, meshKey: "Hoof");

                int hind = bp.Mesh(ProceduralEnemyParts.LegPrefix + "H" + x, -1, new Vector3(0.1f * side, 0.38f, -0.26f),
                    b => DrawUpper(b, new Vector3(0f, -0.17f, -0.05f), 0.065f, 0.035f), meshKey: "HindUpper");
                int hindShin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + "H" + x, hind, new Vector3(0f, -0.17f, -0.05f),
                    b => DrawLower(b, new Vector3(0f, -0.16f, 0.05f)), meshKey: "HindLower");
                bp.Mesh(ProceduralEnemyParts.FootPrefix + "H" + x, hindShin, new Vector3(0f, -0.16f, 0.05f), DrawHoof, meshKey: "Hoof");
            }
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.4f, 0.4f, 0.74f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, 0.04f, 0.2f), new Vector3(0.42f, 0.44f, 0.36f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, -0.12f, 0f), new Vector3(0.28f, 0.16f, 0.5f), EnemySwatch.IronDark, coarse: true);
            // Bristle crest along the spine, tallest over the shoulders.
            for (int k = 0; k < 7; k++)
            {
                float z = 0.3f - k * 0.1f;
                float h = 0.15f - Mathf.Abs(k - 1.5f) * 0.015f;
                b.Cylinder(new Vector3(0f, 0.18f - Mathf.Abs(z) * 0.08f, z), 0.04f, 0f, h, 4, EnemySwatch.Shadow, rotation: Quaternion.Euler(-28f, 0f, 0f));
            }
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            // Wedge head, long snout with a pink disc, white jaw, tusks, glowing eyes.
            b.Blob(new Vector3(0f, 0.02f, 0.04f), new Vector3(0.26f, 0.26f, 0.26f), EnemySwatch.PeccaryFur);
            b.Cylinder(new Vector3(0f, 0f, 0.1f), 0.085f, 0.06f, 0.22f, 6, EnemySwatch.PeccaryFur, EnemySwatch.Pink, Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.07f, 0.12f), new Vector3(0.2f, 0.09f, 0.22f), EnemySwatch.OwlFace, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.075f * s, 0.06f, 0.11f), new Vector3(0.045f, 0.04f, 0.03f), EnemySwatch.Ember, coarse: true);
                b.Cylinder(new Vector3(0.05f * s, -0.04f, 0.24f), 0.014f, 0f, 0.05f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(-20f, 0f, 10f * s));
                b.Box(new Vector3(0.022f * s, 0f, 0.322f), new Vector3(0.015f, 0.025f, 0.008f), EnemySwatch.Black);
            }
        }

        private static void DrawEar(EnemyMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.04f, 0f, 0.09f, 4, EnemySwatch.Shadow);
        }

        private static void DrawTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.1f, -0.05f), 0.025f, 0.012f, 4, EnemySwatch.Shadow);
        }

        private static void DrawUpper(EnemyMeshBuilder b, Vector3 knee, float topRadius, float bottomRadius)
        {
            b.Segment(knee, new Vector3(0f, 0.04f, 0f), bottomRadius, topRadius, 5, EnemySwatch.PeccaryFur);
            b.Blob(knee, Vector3.one * (bottomRadius * 2.1f), EnemySwatch.PeccaryFur, coarse: true);
        }

        private static void DrawLower(EnemyMeshBuilder b, Vector3 hoof)
        {
            b.Segment(hoof, Vector3.zero, 0.025f, 0.033f, 4, EnemySwatch.Shadow);
        }

        private static void DrawHoof(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.02f, 0.015f), new Vector3(0.06f, 0.05f, 0.07f), EnemySwatch.Black, coarse: true);
        }
    }
}
