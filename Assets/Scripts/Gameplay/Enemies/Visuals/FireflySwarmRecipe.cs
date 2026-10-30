using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Enxame de vaga-lumes-de-fogo (Deserto, also the Floresta at night): nine chubby fireflies, each the size of a
    /// fist, drifting around one centre and moving as a single enemy. Each one is a dark little bug with clear wings,
    /// two tiny yellow eyes and a big glowing yellow-green "lantern" with an ember core — the whole swarm is a cloud of
    /// lights you see from far away in the dark. Hierarchy: <c>Swarm</c> (orbit pivot, ~0.9 m up) →
    /// <c>Firefly_0..Firefly_8</c> (shared body mesh) → <c>Lantern</c> (shared glow mesh, blinks by scale in
    /// <see cref="ProceduralEnemyRig"/>).
    /// </summary>
    public static class FireflySwarmRecipe
    {
        public const int FireflyCount = 9;

        // Fixed (not random) so the swarm is identical on every build.
        private static readonly float[] Angles = { 0f, 40f, 95f, 140f, 185f, 220f, 265f, 305f, 340f };
        private static readonly float[] Radii = { 0.55f, 0.3f, 0.62f, 0.42f, 0.58f, 0.25f, 0.5f, 0.66f, 0.36f };
        private static readonly float[] Heights = { 0.1f, -0.2f, 0.25f, -0.05f, -0.25f, 0.3f, 0.05f, -0.15f, 0.18f };
        private static readonly float[] Scales = { 1.1f, 0.9f, 1f, 1.2f, 0.85f, 1f, 1.15f, 0.9f, 1.05f };

        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int swarm = bp.Node(ProceduralEnemyParts.Swarm, -1, new Vector3(0f, 0.9f, 0f));
            for (int i = 0; i < FireflyCount; i++)
            {
                float angle = Angles[i] * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Sin(angle) * Radii[i], Heights[i], Mathf.Cos(angle) * Radii[i]);
                Quaternion rotation = Quaternion.Euler(-10f, Angles[i] + 90f, 0f);
                int fly = bp.Mesh(ProceduralEnemyParts.FireflyPrefix + i, swarm, position, DrawBug, rotation, Vector3.one * Scales[i], "FireflyBody");
                bp.Mesh(ProceduralEnemyParts.Lantern, fly, new Vector3(0f, -0.01f, -0.09f), DrawLantern, meshKey: "FireflyLantern");
            }
        }

        private static void DrawBug(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.1f, 0.09f, 0.13f), EnemySwatch.Shadow, coarse: true);
            b.Blob(new Vector3(0f, 0.01f, 0.1f), new Vector3(0.08f, 0.07f, 0.07f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.025f * s, 0.025f, 0.13f), new Vector3(0.02f, 0.02f, 0.012f), EnemySwatch.MoonEye);
                // Clear wings held up in a V.
                b.TwoSided(new Vector3(0.02f * s, 0.04f, 0.05f), new Vector3(0.13f * s, 0.09f, -0.02f), new Vector3(0.05f * s, 0.05f, -0.07f),
                    EnemySwatch.Steel, EnemySwatch.GolemSnow);
                // Antenna.
                b.Cylinder(new Vector3(0.015f * s, 0.035f, 0.12f), 0.006f, 0f, 0.07f, 3, EnemySwatch.Black, rotation: Quaternion.Euler(50f, 0f, -20f * s));
            }
        }

        private static void DrawLantern(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, -0.04f), new Vector3(0.13f, 0.12f, 0.17f), EnemySwatch.FireflyGlow);
            b.Blob(new Vector3(0f, -0.02f, -0.05f), new Vector3(0.08f, 0.07f, 0.1f), EnemySwatch.Ember, coarse: true);
        }
    }
}
