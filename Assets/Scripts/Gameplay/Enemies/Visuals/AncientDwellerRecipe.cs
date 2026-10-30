using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Habitante Ancestral (Cavernas, chefe final), about 3.3 m: a huge, soft, hunched cave creature
    /// that is lonely and afraid of the dark, not evil (StoryTexts: "abraçado a um monte de luz
    /// roubada"). Rounded shapes only, no teeth or claws: a big head bowed down, large softly glowing
    /// eyes with a sad brow and a small frown, long drooping ears, cyan crystals growing along its
    /// back, and both arms hugging a warm pile of stolen light against its belly. Boss accent: the
    /// warm golden light against the cool cave purples. Hierarchy: <c>Body</c> → <c>Head</c> →
    /// <c>Ear_L</c>/<c>Ear_R</c>; <c>Body</c> → <c>Arm_L</c>/<c>Arm_R</c>, <c>StolenLight</c>;
    /// <c>Leg_L</c>/<c>Leg_R</c> under the root → <c>Shin_L</c>/<c>Shin_R</c> (knee) → <c>Foot_L</c>/<c>Foot_R</c>
    /// (ankle), so the big soft feet step with knee IK.
    /// </summary>
    public static class AncientDwellerRecipe
    {
        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.2f, -0.1f), DrawBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.3f, 0.68f), DrawHead, Quaternion.Euler(22f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.22f, 1.0f), DrawStolenLight);

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.EarRight : ProceduralEnemyParts.EarLeft, head,
                    new Vector3(0.42f * side, 0.14f, -0.05f), DrawEar, Quaternion.Euler(0f, 0f, 30f * side), meshKey: "Ear");
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body,
                    new Vector3(0.92f * side, 1.0f, 0.2f), b => DrawArm(b, side));
                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1,
                    new Vector3(0.52f * side, 0.8f, -0.15f), DrawLeg, meshKey: "Leg");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg,
                    new Vector3(0f, -0.42f, 0.05f), DrawShin, meshKey: "Shin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin,
                    new Vector3(0f, -0.28f, -0.03f), DrawFoot, meshKey: "Foot");
            }
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            Quaternion hunch = Quaternion.Euler(15f, 0f, 0f);
            b.Blob(new Vector3(0f, 0.6f, 0f), new Vector3(1.9f, 1.75f, 1.45f), EnemySwatch.CaveSkin, hunch);
            b.Blob(new Vector3(0f, 0.4f, 0.45f), new Vector3(1.3f, 1.2f, 0.7f), EnemySwatch.CaveBelly, hunch);
            b.Blob(new Vector3(0f, 1.05f, -0.35f), new Vector3(1.5f, 1.0f, 1.05f), EnemySwatch.CaveSkinDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.62f * s, 1.2f, 0f), new Vector3(0.62f, 0.26f, 0.55f), EnemySwatch.CaveMoss, coarse: true);
            }

            // Crystals growing along the back: the light it has been hoarding seeps out of it.
            Crystal(b, new Vector3(0f, 1.5f, -0.45f), 0.13f, 0.75f, -25f, 0f);
            Crystal(b, new Vector3(0.32f, 1.42f, -0.5f), 0.1f, 0.55f, -20f, -25f);
            Crystal(b, new Vector3(-0.32f, 1.42f, -0.5f), 0.11f, 0.6f, -20f, 25f);
            Crystal(b, new Vector3(0.12f, 1.2f, -0.82f), 0.09f, 0.45f, -55f, -10f);
            Crystal(b, new Vector3(-0.15f, 1.15f, -0.85f), 0.08f, 0.4f, -60f, 15f);
            Crystal(b, new Vector3(0.6f, 1.15f, -0.35f), 0.08f, 0.38f, -10f, -50f);
            Crystal(b, new Vector3(-0.6f, 1.15f, -0.35f), 0.09f, 0.42f, -10f, 50f);
        }

        private static void Crystal(EnemyMeshBuilder b, Vector3 at, float radius, float height, float pitch, float roll)
        {
            b.Crystal(at, radius, height, EnemySwatch.Crystal, Quaternion.Euler(pitch, 0f, roll));
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.95f, 0.82f, 0.85f), EnemySwatch.CaveSkin);
            b.Blob(new Vector3(0f, -0.17f, 0.36f), new Vector3(0.56f, 0.36f, 0.36f), EnemySwatch.CaveBelly);
            b.Blob(new Vector3(0f, -0.08f, 0.53f), new Vector3(0.17f, 0.1f, 0.1f), EnemySwatch.CaveSkinDark, coarse: true);

            for (int s = -1; s <= 1; s += 2)
            {
                // Big, soft, glowing eyes with the pupils low (looking down), sad brows, small frown.
                b.Blob(new Vector3(0.21f * s, 0.07f, 0.34f), new Vector3(0.22f, 0.24f, 0.12f), EnemySwatch.SoftEye);
                b.Blob(new Vector3(0.2f * s, 0.03f, 0.4f), new Vector3(0.08f, 0.1f, 0.03f), EnemySwatch.Shadow, coarse: true);
                b.Box(new Vector3(0.21f * s, 0.23f, 0.36f), new Vector3(0.22f, 0.05f, 0.08f), EnemySwatch.CaveSkinDark,
                    Quaternion.Euler(-15f, 0f, -18f * s));
                b.Box(new Vector3(0.055f * s, -0.28f, 0.5f), new Vector3(0.11f, 0.025f, 0.03f), EnemySwatch.Shadow,
                    Quaternion.Euler(0f, 0f, -22f * s));

                // A small crown of crystals on the brow.
                b.Crystal(new Vector3(0.13f * s, 0.36f, 0.04f), 0.05f, 0.2f, EnemySwatch.Crystal, Quaternion.Euler(-10f, 0f, -22f * s));
            }

            b.Crystal(new Vector3(0f, 0.39f, 0f), 0.06f, 0.28f, EnemySwatch.Crystal, Quaternion.Euler(-12f, 0f, 0f));
        }

        private static void DrawEar(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.3f, 0f), new Vector3(0.24f, 0.7f, 0.12f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, -0.28f, 0.04f), new Vector3(0.15f, 0.55f, 0.06f), EnemySwatch.CaveBelly, coarse: true);
        }

        private static void DrawArm(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Blob(new Vector3(0.08f, -0.38f, 0.12f), new Vector3(0.42f, 0.85f, 0.42f), EnemySwatch.CaveSkin, Quaternion.Euler(-25f, 0f, 12f));
            b.Blob(new Vector3(-0.12f, -0.78f, 0.5f), new Vector3(0.36f, 0.36f, 0.78f), EnemySwatch.CaveSkin, Quaternion.Euler(10f, -30f, 0f));
            b.Blob(new Vector3(-0.38f, -0.84f, 0.82f), new Vector3(0.34f, 0.26f, 0.32f), EnemySwatch.CaveBelly, coarse: true);
            for (int k = 0; k < 3; k++)
            {
                b.Blob(new Vector3(-0.48f - k * 0.02f, -0.8f - k * 0.05f, 0.9f - k * 0.08f), new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.CaveBelly, coarse: true);
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawStolenLight(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.42f, 0.42f), EnemySwatch.StolenLight);
            float[] tilts = { 0f, 30f, -30f, 55f, -55f, 15f };
            float[] yaws = { 0f, 40f, -40f, 120f, -120f, 180f };
            float[] heights = { 0.36f, 0.3f, 0.28f, 0.24f, 0.22f, 0.26f };
            for (int i = 0; i < tilts.Length; i++)
            {
                Quaternion r = Quaternion.Euler(0f, yaws[i], 0f) * Quaternion.Euler(tilts[i] * 0.6f, 0f, tilts[i]);
                b.Crystal(new Vector3(0f, 0.05f, 0f), 0.07f, heights[i] + 0.12f, EnemySwatch.StolenLight, r);
            }
        }

        private static void DrawLeg(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.2f, 0f), new Vector3(0.62f, 0.62f, 0.66f), EnemySwatch.CaveSkinDark);
        }

        private static void DrawShin(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.1f, -0.02f), new Vector3(0.52f, 0.44f, 0.56f), EnemySwatch.CaveSkinDark);
        }

        private static void DrawFoot(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.02f, 0.14f), new Vector3(0.62f, 0.26f, 0.78f), EnemySwatch.CaveSkinDark);
            for (int k = -1; k <= 1; k++)
            {
                b.Blob(new Vector3(k * 0.16f, -0.06f, 0.48f), new Vector3(0.14f, 0.12f, 0.14f), EnemySwatch.CaveBelly, coarse: true);
            }
        }
    }
}
