using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Escorpião do Deserto, about 0.7 m to the raised stinger and 1.2 m long: a low, plated brown
    /// scorpion with sandstone-coloured plate edges, two big pincers held forward, eight short jointed
    /// legs and a five-piece tail curling over its back to a black stinger, with tiny glowing eyes.
    /// Hierarchy: <c>Body</c> → <c>Claw_L</c>/<c>Claw_R</c> (shoulder pivot), <c>Body</c> → <c>Tail</c> →
    /// <c>Tail_1</c> → <c>Tail_2</c> → <c>Tail_3</c> → <c>Stinger</c> (each a joint, so the tail lags and
    /// sways through follow-through springs); legs under the root as in <see cref="SpiderRecipe"/>
    /// (<c>Leg_L0..R3</c> → <c>Shin_</c> → <c>Foot_</c>), lower and splayed further back.
    /// </summary>
    public static class ScorpionRecipe
    {
        public const int PairCount = 4;

        private const float HipHeight = 0.22f;
        private static readonly float[] HipZ = { 0.1f, 0.03f, -0.04f, -0.11f };
        private static readonly float[] Splay = { 35f, 10f, -15f, -40f };

        private static readonly Vector3 Knee = new Vector3(0.16f, 0.1f, 0f);
        private static readonly Vector3 Tip = new Vector3(0.16f, -(HipHeight + 0.1f - 0.01f), 0f);

        /// <summary>Tail joints, each relative to the previous (back, then up and over toward the head).</summary>
        private static readonly Vector3[] TailSteps =
        {
            new Vector3(0f, 0.063f, -0.158f),
            new Vector3(0f, 0.146f, -0.087f),
            new Vector3(0f, 0.144f, 0.042f),
            new Vector3(0f, 0.066f, 0.112f),
        };

        private static readonly float[] TailRadii = { 0.055f, 0.05f, 0.045f, 0.04f, 0.035f };

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, HipHeight, 0f), DrawBody);

            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body,
                    new Vector3(0.12f * side, 0f, 0.2f), b => DrawClaw(b, sd));
            }

            int parent = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.36f), b => DrawTailSegment(b, 0));
            for (int i = 1; i < TailSteps.Length; i++)
            {
                int index = i;
                parent = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + i, parent, TailSteps[i - 1], b => DrawTailSegment(b, index));
            }

            bp.Mesh(ProceduralEnemyParts.Stinger, parent, TailSteps[TailSteps.Length - 1], DrawStinger);

            for (int pair = 0; pair < PairCount; pair++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    string id = (side < 0 ? "L" : "R") + pair;
                    float yaw = side > 0 ? -Splay[pair] : 180f + Splay[pair];
                    int hip = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, new Vector3(0.13f * side, HipHeight, HipZ[pair]), DrawUpper,
                        Quaternion.Euler(0f, yaw, 0f), meshKey: "Upper");
                    int knee = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, hip, Knee, DrawLower, meshKey: "Lower");
                    bp.Node(ProceduralEnemyParts.FootPrefix + id, knee, Tip);
                }
            }
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            // Head shield, then plated segments narrowing toward the tail.
            b.Blob(new Vector3(0f, 0f, 0.13f), new Vector3(0.3f, 0.12f, 0.24f), EnemySwatch.Leather);
            b.Box(new Vector3(0f, 0.05f, 0.14f), new Vector3(0.24f, 0.03f, 0.2f), EnemySwatch.SandstoneDark, Quaternion.Euler(-6f, 0f, 0f));
            float[] z = { -0.02f, -0.13f, -0.23f, -0.32f };
            float[] w = { 0.34f, 0.32f, 0.27f, 0.2f };
            for (int k = 0; k < z.Length; k++)
            {
                b.Blob(new Vector3(0f, 0f, z[k]), new Vector3(w[k], 0.13f, 0.14f), EnemySwatch.Leather, coarse: k > 1);
                b.Box(new Vector3(0f, 0.055f, z[k]), new Vector3(w[k] * 0.85f, 0.025f, 0.09f), EnemySwatch.SandstoneDark, top: EnemySwatch.Sandstone);
            }

            b.Blob(new Vector3(0f, -0.045f, -0.08f), new Vector3(0.26f, 0.06f, 0.4f), EnemySwatch.SandstoneDark, coarse: true);

            // Tiny glowing eyes on the head shield.
            for (int side = -1; side <= 1; side += 2)
            {
                b.Blob(new Vector3(0.025f * side, 0.075f, 0.18f), new Vector3(0.025f, 0.025f, 0.025f), EnemySwatch.WarEye, coarse: true);
                b.Blob(new Vector3(0.1f * side, 0.05f, 0.21f), new Vector3(0.02f, 0.02f, 0.02f), EnemySwatch.WarEye, coarse: true);
            }
        }

        private static void DrawClaw(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            Vector3 elbow = new Vector3(0.1f, 0.02f, 0.1f);
            Vector3 wrist = new Vector3(0.08f, 0.03f, 0.24f);
            b.Segment(Vector3.zero, elbow, 0.035f, 0.03f, 5, EnemySwatch.Leather);
            b.Segment(elbow, wrist, 0.03f, 0.04f, 5, EnemySwatch.Leather);
            b.Blob(wrist + new Vector3(0f, 0f, 0.05f), new Vector3(0.11f, 0.08f, 0.14f), EnemySwatch.Leather);
            b.Box(wrist + new Vector3(0f, 0.035f, 0.05f), new Vector3(0.08f, 0.02f, 0.1f), EnemySwatch.SandstoneDark);
            // The two fingers of the pincer, curving toward each other.
            b.Horn(wrist + new Vector3(0.03f, 0f, 0.1f), 0.026f, 0.12f, 3, -16f, EnemySwatch.Leather, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.forward);
            b.Horn(wrist + new Vector3(-0.025f, 0f, 0.1f), 0.022f, 0.11f, 3, 16f, EnemySwatch.Bone, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.forward);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawTailSegment(EnemyMeshBuilder b, int index)
        {
            Vector3 next = TailSteps[Mathf.Min(index, TailSteps.Length - 1)];
            float r0 = TailRadii[index], r1 = TailRadii[index + 1];
            b.Blob(Vector3.zero, Vector3.one * (r0 * 2.2f), EnemySwatch.Leather, coarse: true);
            b.Segment(Vector3.zero, next, r0, r1, 6, EnemySwatch.Leather);
            b.Box(next * 0.5f + Vector3.up * r0 * 0.7f, new Vector3(r0 * 1.4f, 0.015f, next.magnitude * 0.6f), EnemySwatch.SandstoneDark,
                Quaternion.LookRotation(next.normalized, Vector3.up));
        }

        private static void DrawStinger(EnemyMeshBuilder b)
        {
            // Venom bulb, then the curved black barb pointing forward and down.
            b.Blob(new Vector3(0f, 0.02f, 0.04f), new Vector3(0.09f, 0.08f, 0.12f), EnemySwatch.SandstoneDark);
            b.Horn(new Vector3(0f, 0.01f, 0.09f), 0.025f, 0.12f, 3, 30f, EnemySwatch.Black, Quaternion.Euler(80f, 0f, 0f), 4);
        }

        private static void DrawUpper(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, Knee, 0.025f, 0.022f, 5, EnemySwatch.Leather);
            b.Blob(Knee, new Vector3(0.045f, 0.045f, 0.045f), EnemySwatch.SandstoneDark, coarse: true);
        }

        private static void DrawLower(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, Tip, 0.02f, 0.006f, 5, EnemySwatch.Leather);
        }
    }
}
