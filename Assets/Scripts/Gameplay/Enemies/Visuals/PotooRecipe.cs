using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Mãe-da-lua (urutau; Montanha, night), about 0.75 m tall: a stiff, upright bird in bark-grey and lichen
    /// camouflage, sitting on the dead branch it ambushes from — and taking the branch along when it flies, of
    /// course. Its giveaway in the dark is the pair of enormous glowing yellow eyes; when it strikes, the tiny beak
    /// opens into a huge pink gape. Pivot at the bottom of the branch (y = 0); the Flyer archetype lifts it.
    /// Hierarchy: <c>Branch</c> (twig, under the root); <c>Body</c> → <c>Head</c> → <c>Jaw</c> (opens on the windup,
    /// <see cref="ProceduralEnemyRig"/>); <c>Body</c> → <c>Wing_L</c>/<c>Wing_R</c> (folded, flap while travelling),
    /// <c>Tail</c> (long, pointing down: follow-through).
    /// </summary>
    public static class PotooRecipe
    {
        public static void Build(ProceduralEnemyBlueprint bp)
        {
            bp.Mesh(ProceduralEnemyParts.Branch, -1, new Vector3(0f, 0.04f, 0f), DrawBranch, Quaternion.Euler(0f, 35f, 0f));
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawBody, Quaternion.Euler(-12f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.25f, 0.02f), DrawHead, Quaternion.Euler(12f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.02f, 0.1f), DrawJaw);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.12f, 0.1f, -0.02f), b => DrawWing(b, -1f),
                Quaternion.Euler(0f, 0f, 70f), meshKey: "PotooWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.12f, 0.1f, -0.02f), b => DrawWing(b, 1f),
                Quaternion.Euler(0f, 0f, -70f), meshKey: "PotooWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.06f, -0.1f), DrawTail, Quaternion.Euler(-150f, 0f, 0f));
        }

        private static void DrawBranch(EnemyMeshBuilder b)
        {
            b.Cylinder(new Vector3(-0.42f, 0f, 0f), 0.04f, 0.03f, 0.84f, 5, EnemySwatch.BarkDark, EnemySwatch.Bark, Quaternion.Euler(0f, 0f, -90f), bottomCap: true);
            // A side twig and a leafless fork.
            b.Cylinder(new Vector3(0.2f, 0f, 0f), 0.018f, 0.006f, 0.26f, 4, EnemySwatch.BarkDark, rotation: Quaternion.Euler(60f, 30f, 0f));
            b.Cylinder(new Vector3(-0.25f, 0f, 0f), 0.016f, 0.005f, 0.22f, 4, EnemySwatch.BarkDark, rotation: Quaternion.Euler(-65f, -20f, 0f));
            b.Blob(new Vector3(0.05f, 0.03f, 0f), new Vector3(0.1f, 0.05f, 0.08f), EnemySwatch.Lichen, coarse: true);
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.26f, 0.42f, 0.24f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.2f, 0.34f, 0.16f), EnemySwatch.Lichen, coarse: true);
            // Bark streaks down the chest.
            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0.05f * i, 0.0f, 0.135f), new Vector3(0.015f, 0.2f, 0.015f), EnemySwatch.BarkDark, Quaternion.Euler(-8f, 0f, 6f * i));
            }

            // Tiny feet gripping the branch.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.05f * s, -0.18f, 0.03f), 0.015f, 0f, 0.07f, 3, EnemySwatch.BarkDark, rotation: Quaternion.Euler(170f, 0f, 0f));
            }
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            // Wide flat head, two huge glowing eyes with black pupils, a stubby upper beak.
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.24f, 0.18f, 0.22f), EnemySwatch.Bark);
            b.Box(new Vector3(0f, 0.15f, 0f), new Vector3(0.18f, 0.03f, 0.16f), EnemySwatch.BarkDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.08f, 0.08f), new Vector3(0.11f, 0.11f, 0.08f), EnemySwatch.MoonEye);
                b.Box(new Vector3(0.07f * s, 0.08f, 0.12f), new Vector3(0.035f, 0.035f, 0.01f), EnemySwatch.Black);
                // Feathery "eyebrows" (it looks permanently surprised).
                b.Box(new Vector3(0.07f * s, 0.15f, 0.08f), new Vector3(0.09f, 0.02f, 0.03f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, 18f * s));
            }

            b.Cylinder(new Vector3(0f, 0.03f, 0.1f), 0.022f, 0f, 0.05f, 4, EnemySwatch.BarkDark, rotation: Quaternion.Euler(110f, 0f, 0f));
        }

        private static void DrawJaw(EnemyMeshBuilder b)
        {
            // Hinge at the corners of the mouth: a wide lower bill with the big pink gape on its top.
            b.Blob(new Vector3(0f, -0.02f, 0.0f), new Vector3(0.2f, 0.04f, 0.1f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(0f, 0.0f, -0.01f), new Vector3(0.16f, 0.03f, 0.08f), EnemySwatch.Pink, coarse: true);
        }

        private static void DrawWing(EnemyMeshBuilder b, float side)
        {
            // Folded long wing (pointing back along the body when the part rotation holds it down at the side).
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            var r0 = new Vector3(0f, 0f, 0.06f);
            var r1 = new Vector3(0f, 0f, -0.1f);
            var tip = new Vector3(0.42f, 0f, -0.12f);
            var trail = new Vector3(0.26f, 0f, -0.2f);
            b.TwoSided(r0, tip, trail, EnemySwatch.Bark, EnemySwatch.Lichen);
            b.TwoSided(r0, trail, r1, EnemySwatch.Bark, EnemySwatch.Lichen);
            b.Box(new Vector3(0.22f, 0.01f, -0.06f), new Vector3(0.24f, 0.012f, 0.025f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 10f, 0f));
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawTail(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.12f, 0.26f, 0.05f), EnemySwatch.Bark, coarse: true);
            b.Box(new Vector3(0f, 0.12f, 0.025f), new Vector3(0.015f, 0.22f, 0.01f), EnemySwatch.BarkDark);
        }
    }
}
