using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Golem de Pedra (Montanha), about 2.1 m: a hunched pile of grey rocks with snow on every
    /// up-facing face, glowing molten cracks, a small head sunk between the shoulders and long arms
    /// ending in boulder fists that almost drag on the ground. Hierarchy: <c>Body</c> → <c>Head</c>,
    /// <c>Arm_L</c>/<c>Arm_R</c> (shoulder pivot) → <c>Fist_L</c>/<c>Fist_R</c>; <c>Leg_L</c>/<c>Leg_R</c>
    /// (hip pivot) under the root → <c>Shin_L</c>/<c>Shin_R</c> (knee, bent slightly forward) →
    /// <c>Foot_L</c>/<c>Foot_R</c> (ankle), so the legs step with knee IK.
    /// </summary>
    public static class StoneGolemRecipe
    {
        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.05f, 0f), DrawBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.55f, 0.24f), DrawHead, Quaternion.Euler(6f, 0f, 0f));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body,
                    new Vector3(0.62f * side, 0.25f, 0.02f), b => DrawArm(b, side), Quaternion.Euler(-8f, 0f, 0f));
                bp.Mesh(right ? ProceduralEnemyParts.FistRight : ProceduralEnemyParts.FistLeft, arm,
                    new Vector3(0.13f * side, -1.02f, 0.1f), b => DrawFist(b, side));
                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1,
                    new Vector3(0.3f * side, 0.62f, -0.02f), b => DrawLeg(b, side));
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg,
                    new Vector3(0f, -0.3f, 0.05f), b => DrawShin(b, side));
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin,
                    new Vector3(0f, -0.22f, -0.03f), b => DrawFoot(b, side));
            }
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            Quaternion lean = Quaternion.Euler(10f, 0f, 0f);
            b.Rock(new Vector3(0f, 0.12f, 0f), new Vector3(1.2f, 1.0f, 0.9f), EnemySwatch.GolemStone, 0, lean, EnemySwatch.GolemSnow);
            b.Rock(new Vector3(0f, 0.02f, 0.3f), new Vector3(0.82f, 0.62f, 0.42f), EnemySwatch.GolemStoneDark, 1, lean);
            b.Rock(new Vector3(0f, -0.42f, -0.02f), new Vector3(0.85f, 0.45f, 0.62f), EnemySwatch.GolemStoneDark, 2);
            b.Rock(new Vector3(0.05f, 0.5f, -0.3f), new Vector3(0.7f, 0.42f, 0.5f), EnemySwatch.GolemStone, 3, null, EnemySwatch.GolemSnow);

            // Molten cracks branching over the chest (and one across the back).
            b.Box(new Vector3(0.04f, 0.06f, 0.52f), new Vector3(0.06f, 0.46f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(-10f, 0f, 14f));
            b.Box(new Vector3(-0.1f, 0.18f, 0.5f), new Vector3(0.05f, 0.24f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(-10f, 0f, -38f));
            b.Box(new Vector3(0.14f, -0.12f, 0.49f), new Vector3(0.05f, 0.2f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(-10f, 0f, 52f));
            b.Box(new Vector3(0f, 0.15f, -0.47f), new Vector3(0.5f, 0.05f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 8f));
            b.Blob(new Vector3(0f, 0.04f, 0.5f), new Vector3(0.12f, 0.12f, 0.06f), EnemySwatch.GolemCrack, coarse: true);
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            b.Rock(Vector3.zero, new Vector3(0.44f, 0.36f, 0.4f), EnemySwatch.GolemStone, 3, null, EnemySwatch.GolemSnow);
            b.Box(new Vector3(0f, 0.06f, 0.17f), new Vector3(0.42f, 0.08f, 0.12f), EnemySwatch.GolemStoneDark, Quaternion.Euler(-12f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.09f * s, -0.01f, 0.2f), new Vector3(0.1f, 0.05f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 8f * s));
            }
        }

        private static void DrawArm(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Rock(new Vector3(0.08f, 0.02f, 0f), new Vector3(0.52f, 0.46f, 0.52f), EnemySwatch.GolemStone, 1, null, EnemySwatch.GolemSnow);
            b.Rock(new Vector3(0.1f, -0.38f, 0f), new Vector3(0.32f, 0.5f, 0.32f), EnemySwatch.GolemStoneDark, 2);
            b.Rock(new Vector3(0.12f, -0.74f, 0.05f), new Vector3(0.38f, 0.46f, 0.38f), EnemySwatch.GolemStone, 0);
            b.Box(new Vector3(0.12f, -0.74f, 0.23f), new Vector3(0.05f, 0.3f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 18f));
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawFist(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Rock(Vector3.zero, new Vector3(0.58f, 0.52f, 0.58f), EnemySwatch.GolemStone, 3, null, EnemySwatch.GolemSnow);
            for (int k = -1; k <= 1; k++)
            {
                b.Box(new Vector3(k * 0.13f, 0.02f, 0.27f), new Vector3(0.04f, 0.16f, 0.04f), EnemySwatch.GolemCrack);
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawLeg(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Rock(new Vector3(0f, -0.13f, 0f), new Vector3(0.44f, 0.38f, 0.46f), EnemySwatch.GolemStoneDark, 1);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawShin(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Rock(new Vector3(0f, -0.1f, -0.01f), new Vector3(0.4f, 0.32f, 0.42f), EnemySwatch.GolemStoneDark, 3);
            b.Box(new Vector3(0.02f, -0.06f, 0.2f), new Vector3(0.04f, 0.16f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 12f));
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawFoot(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.Rock(new Vector3(0.02f, 0f, 0.05f), new Vector3(0.52f, 0.22f, 0.62f), EnemySwatch.GolemStone, 2, null, EnemySwatch.GolemSnow);
            b.Matrix = Matrix4x4.identity;
        }
    }
}
