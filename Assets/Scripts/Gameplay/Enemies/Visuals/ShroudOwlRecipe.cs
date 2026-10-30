using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Coruja-rasga-mortalha (barn owl; Floresta, visits the Montanha at night), about 0.95 m tall with its wings
    /// half open (1.6 m across): golden-buff back, speckled white belly and the pale heart-shaped face disc that
    /// floats in the dark like a little moon, with two glowing yellow eyes. Pivot at the talons (y = 0); the Flyer
    /// archetype lifts it into its hover and swoops. Hierarchy: <c>Body</c> → <c>Head</c> (swivels now and then:
    /// owls turn their heads all the way round, kids love it), <c>Wing_L</c>/<c>Wing_R</c> (slow flaps with long
    /// glides), <c>Tail</c> (follow-through).
    /// </summary>
    public static class ShroudOwlRecipe
    {
        public static void Build(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.32f, 0.04f), DrawHead);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.17f, 0.12f, -0.02f), b => DrawWing(b, -1f),
                Quaternion.Euler(0f, 0f, -25f), meshKey: "OwlWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.17f, 0.12f, -0.02f), b => DrawWing(b, 1f),
                Quaternion.Euler(0f, 0f, 25f), meshKey: "OwlWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.16f, -0.17f), DrawTail, Quaternion.Euler(30f, 0f, 0f));
        }

        private static void DrawBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.56f, 0.4f), EnemySwatch.OwlBuff);
            b.Blob(new Vector3(0f, -0.04f, 0.07f), new Vector3(0.34f, 0.46f, 0.3f), EnemySwatch.OwlFace);
            // A few grey speckles on the white belly.
            for (int i = 0; i < 6; i++)
            {
                float x = (i % 3 - 1) * 0.08f;
                float y = 0.06f - (i / 3) * 0.14f;
                b.Box(new Vector3(x, y, 0.215f), new Vector3(0.025f, 0.03f, 0.02f), EnemySwatch.GolemStone);
            }

            // Feathered legs and hooked talons down to the ground.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, -0.3f, 0.04f), new Vector3(0.1f, 0.18f, 0.1f), EnemySwatch.OwlFace, coarse: true);
                for (int k = -1; k <= 1; k++)
                {
                    b.Cylinder(new Vector3(0.08f * s + 0.025f * k, -0.4f, 0.06f), 0.014f, 0f, 0.06f, 3, EnemySwatch.BarkDark,
                        rotation: Quaternion.Euler(110f, 0f, 0f));
                }
            }
        }

        private static void DrawHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.36f, 0.32f, 0.32f), EnemySwatch.OwlBuff);
            // Heart-shaped face disc: two pale lobes meeting at a point over the beak.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.09f, 0.13f), new Vector3(0.17f, 0.24f, 0.08f), EnemySwatch.OwlFace, Quaternion.Euler(0f, 0f, 14f * s));
                b.Blob(new Vector3(0.068f * s, 0.1f, 0.17f), new Vector3(0.075f, 0.075f, 0.04f), EnemySwatch.MoonEye, coarse: true);
                b.Box(new Vector3(0.068f * s, 0.1f, 0.19f), new Vector3(0.025f, 0.035f, 0.01f), EnemySwatch.Black);
            }

            // Little hooked beak.
            b.Cylinder(new Vector3(0f, 0.05f, 0.17f), 0.02f, 0f, 0.06f, 4, EnemySwatch.Bone, rotation: Quaternion.Euler(150f, 0f, 0f));
        }

        /// <summary>Feathered wing in the bat's horizontal plane, ragged trailing edge; <paramref name="side"/> +1 = right.</summary>
        private static void DrawWing(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            var r0 = new Vector3(0f, 0f, 0.1f);
            var r1 = new Vector3(0f, 0f, -0.16f);
            var mid = new Vector3(0.32f, 0.04f, 0.08f);
            var tip = new Vector3(0.66f, 0f, -0.06f);
            var p1 = new Vector3(0.58f, 0f, -0.22f);
            var p2 = new Vector3(0.42f, 0f, -0.3f);
            var p3 = new Vector3(0.24f, 0f, -0.28f);
            var p4 = new Vector3(0.1f, 0f, -0.24f);
            b.TwoSided(r0, mid, p4, EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
            b.TwoSided(r0, p4, r1, EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
            b.TwoSided(mid, p3, p4, EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
            b.TwoSided(mid, p2, p3, EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
            b.TwoSided(mid, p1, p2, EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
            b.TwoSided(mid, tip, p1, EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
            // Darker bars along the flight feathers.
            b.Box(new Vector3(0.42f, 0.012f, -0.12f), new Vector3(0.3f, 0.012f, 0.03f), EnemySwatch.SandstoneDark, Quaternion.Euler(0f, -20f, 0f));
            b.Box(new Vector3(0.3f, 0.012f, -0.2f), new Vector3(0.22f, 0.012f, 0.03f), EnemySwatch.SandstoneDark, Quaternion.Euler(0f, -10f, 0f));
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawTail(EnemyMeshBuilder b)
        {
            b.TwoSided(new Vector3(-0.07f, 0f, 0f), new Vector3(0.07f, 0f, 0f), new Vector3(0.1f, 0f, -0.16f), EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
            b.TwoSided(new Vector3(-0.07f, 0f, 0f), new Vector3(0.1f, 0f, -0.16f), new Vector3(-0.1f, 0f, -0.16f), EnemySwatch.OwlBuff, EnemySwatch.OwlFace);
        }
    }
}
