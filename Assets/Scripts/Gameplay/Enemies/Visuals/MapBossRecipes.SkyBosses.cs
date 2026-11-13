using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The last four map bosses of the Reinos do Céu (Docs/Mapas100.md 92, 94, 98, 99), built like the rest of <see cref="MapBossRecipes"/>: pivot at the
    /// feet, facing +Z, the glowing <c>Core</c> the rig feeds with the fight's phase, and soft eyes, because each is a creature gone dark, not a villain.
    /// <list type="bullet">
    /// <item><b>O Remador Perdido</b> (92, ~3 m): a tall rower of light gone grey, standing in a little hull of light, a long oar held across him; no legs.</item>
    /// <item><b>O Arconte da Roda</b> (94, ~2.8 m): a squat bronze archon with the spoked wheel of twelve jars on his back, a jar for each hand; two legs.</item>
    /// <item><b>O Último Arconte do Degrau</b> (98, ~2.9 m): a heavy archon of grey stone sitting on a block of firm cloud, arms braced on his knees; no legs that walk.</item>
    /// <item><b>A Rachadura Grande</b> (99, ~3.1 m): a tall slit of night between two jagged edges of sky, the shadow running down it like tar; no legs.</item>
    /// </list>
    /// </summary>
    public static partial class MapBossRecipes
    {
        // ------------------------------------------------------------------
        // O Remador Perdido (92)
        // ------------------------------------------------------------------

        public static void BuildLostRower(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // A rower of the barcas of light that stopped to look at the shadow in the water: the little hull of light round his waist, gone pale.
                b.Blob(new Vector3(0f, 0.35f, 0f), new Vector3(1.4f, 0.7f, 2.3f), EnemySwatch.Ivory, coarse: true);
                b.Box(new Vector3(0f, 0.68f, 0f), new Vector3(1.2f, 0.06f, 1.9f), EnemySwatch.Gold);
                // The rower himself, tall and grey, the light gone out of him but for the heart.
                b.Segment(new Vector3(0f, 0.6f, 0f), new Vector3(0f, 1.9f, 0f), 0.32f, 0.4f, 8, EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, 1.55f, 0.04f), new Vector3(0.85f, 0.9f, 0.6f), EnemySwatch.GolemStone, coarse: true);
                b.Blob(new Vector3(0f, 2.02f, -0.04f), new Vector3(0.95f, 0.36f, 0.6f), EnemySwatch.Shroud, coarse: true);
                // The long oar held across him, its blade low on the left, dripping light.
                b.Segment(new Vector3(-0.55f, 0.85f, 0.5f), new Vector3(0.55f, 2.35f, 0.42f), 0.05f, 0.05f, 6, EnemySwatch.BarkDark);
                b.Box(new Vector3(-0.6f, 0.8f, 0.51f), new Vector3(0.1f, 0.5f, 0.26f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, 36f));
            });
            // The light he was carrying home, still in his chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.45f, 0.36f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.12f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.4f, 0.06f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.5f, 0.55f, 0.5f), EnemySwatch.Ivory, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.03f, 0.21f, 0.09f, EnemySwatch.SoftEye);
                // The tall cap of the rowers, a cone of gold.
                b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.2f, 0.04f, 0.38f, 8, EnemySwatch.Gold);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.16f, 0.2f), b => b.Blob(Vector3.zero, new Vector3(0.2f, 0.06f, 0.08f), EnemySwatch.Shadow, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Long arms on the oar: they pull back on the windup, as the wake starts.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.42f * side, 1.88f, 0.08f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.12f * side, -0.45f, 0.26f), 0.11f, 0.09f, 6, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.12f * side, -0.45f, 0.26f), new Vector3(0.05f * side, -0.7f, 0.42f), 0.09f, 0.07f, 6, EnemySwatch.GolemStone);
                    b.Blob(new Vector3(0.05f * side, -0.74f, 0.44f), new Vector3(0.16f, 0.16f, 0.16f), EnemySwatch.Ivory, coarse: true);
                }, meshKey: right ? "LostRowerArm_R" : "LostRowerArm_L");
            }
        }

        // ------------------------------------------------------------------
        // O Arconte da Roda (94)
        // ------------------------------------------------------------------

        public static void BuildWheelArchon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // The archon who sat on the hub of the Celestial Wheel to lock it: squat, bronze, heavy as the hub.
                b.Blob(Vector3.zero, new Vector3(1.2f, 1.1f, 0.9f), EnemySwatch.Copper);
                b.Blob(new Vector3(0f, -0.2f, 0.12f), new Vector3(0.9f, 0.7f, 0.7f), EnemySwatch.CopperDark, coarse: true);
                // The wheel on his back: a rim, six spokes and the twelve jars, the one that keeps turning lit.
                Vector3 hub = new Vector3(0f, 0.35f, -0.6f);
                const int jars = 12;
                const float rim = 0.85f;
                for (int i = 0; i < jars; i++)
                {
                    float a0 = i * Mathf.PI * 2f / jars;
                    float a1 = (i + 1) * Mathf.PI * 2f / jars;
                    Vector3 p0 = hub + new Vector3(Mathf.Sin(a0), Mathf.Cos(a0), 0f) * rim;
                    Vector3 p1 = hub + new Vector3(Mathf.Sin(a1), Mathf.Cos(a1), 0f) * rim;
                    b.Segment(p0, p1, 0.05f, 0.05f, 5, EnemySwatch.CopperDark);
                    b.Blob(p0, new Vector3(0.18f, 0.22f, 0.18f), i % 4 == 0 ? EnemySwatch.Crystal : EnemySwatch.Verdigris, coarse: true);
                    if (i % 2 == 0)
                    {
                        b.Segment(hub, p0, 0.04f, 0.03f, 4, EnemySwatch.Copper);
                    }
                }

                b.Cylinder(hub + new Vector3(0f, 0f, -0.08f), 0.16f, 0.16f, 0.16f, 8, EnemySwatch.Gold, rotation: Quaternion.Euler(90f, 0f, 0f));
            });
            // The light the jars should be lifting, held fast in his chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.1f, 0.44f), b => b.Blob(Vector3.zero, new Vector3(0.32f, 0.32f, 0.14f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.75f, 0.14f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.55f, 0.5f, 0.55f), EnemySwatch.CopperDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.03f, 0.23f, 0.09f, EnemySwatch.SoftEye);
                // A crown of three short spokes.
                for (int i = -1; i <= 1; i++)
                {
                    b.Horn(new Vector3(i * 0.14f, 0.2f, -0.02f), 0.05f, 0.24f, 2, 0f, EnemySwatch.Gold, Quaternion.Euler(0f, 0f, -i * 16f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.16f, 0.2f), b => b.Blob(Vector3.zero, new Vector3(0.26f, 0.07f, 0.1f), EnemySwatch.Copper, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // A jar for each hand: they heave up on the windup, as he locks the wheel.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.62f * side, 0.25f, 0.05f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.12f * side, -0.5f, 0.24f), 0.15f, 0.12f, 6, EnemySwatch.Copper);
                    b.Blob(new Vector3(0.12f * side, -0.68f, 0.28f), new Vector3(0.3f, 0.38f, 0.3f), EnemySwatch.Verdigris, coarse: true);
                    b.Cylinder(new Vector3(0.12f * side, -0.52f, 0.28f), 0.08f, 0.1f, 0.08f, 6, EnemySwatch.Gold);
                }, meshKey: right ? "WheelArchonArm_R" : "WheelArchonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.92f, 0.3f, 0.17f, EnemySwatch.CopperDark, EnemySwatch.Copper, EnemySwatch.CopperDark, "WheelArchon");
        }

        // ------------------------------------------------------------------
        // O Último Arconte do Degrau (98)
        // ------------------------------------------------------------------

        public static void BuildLastStepArchon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // The last archon of all, sitting on the step of firm cloud so that nobody goes up: the step, and the one above it at his back.
                b.Box(new Vector3(0f, 0.3f, -0.3f), new Vector3(1.9f, 0.6f, 1.1f), EnemySwatch.GolemSnow);
                b.Box(new Vector3(0f, 0.75f, -0.95f), new Vector3(1.9f, 0.3f, 0.3f), EnemySwatch.GolemSnow);
                // His legs folded on the step, feet planted on the one below.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.3f * s, 0.78f, -0.25f), new Vector3(0.32f * s, 0.78f, 0.42f), 0.2f, 0.19f, 6, EnemySwatch.GolemStoneDark);
                    b.Segment(new Vector3(0.32f * s, 0.78f, 0.42f), new Vector3(0.3f * s, 0.12f, 0.5f), 0.18f, 0.15f, 6, EnemySwatch.GolemStone);
                    b.Blob(new Vector3(0.3f * s, 0.07f, 0.6f), new Vector3(0.3f, 0.14f, 0.45f), EnemySwatch.GolemStoneDark, coarse: true);
                }

                // A heavy body of grey stone, a mantle of shadow on the shoulders.
                b.Blob(new Vector3(0f, 1.45f, -0.18f), new Vector3(1.3f, 1.4f, 0.95f), EnemySwatch.GolemStone);
                b.Blob(new Vector3(0f, 1.95f, -0.22f), new Vector3(1.4f, 0.5f, 0.9f), EnemySwatch.Shroud, coarse: true);
            });
            // The spark he took from the last one who tried to climb, held in his chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.4f, 0.3f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.12f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.35f, -0.02f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.6f, 0.6f, 0.6f), EnemySwatch.GolemStoneDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.04f, 0.25f, 0.09f, EnemySwatch.SoftEye);
                // The tall flat cap of an archon, a gold band round it.
                b.Cylinder(new Vector3(0f, 0.24f, 0f), 0.28f, 0.22f, 0.3f, 8, EnemySwatch.Black);
                b.Cylinder(new Vector3(0f, 0.26f, 0f), 0.29f, 0.29f, 0.05f, 8, EnemySwatch.Gold);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.18f, 0.22f), b => b.Blob(Vector3.zero, new Vector3(0.26f, 0.07f, 0.1f), EnemySwatch.Black, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Arms braced on his knees: they heave up on the windup, as he plants his feet.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.62f * side, 1.85f, -0.12f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.05f * side, -0.6f, 0.35f), 0.17f, 0.14f, 6, EnemySwatch.GolemStone);
                    b.Segment(new Vector3(0.05f * side, -0.6f, 0.35f), new Vector3(-0.1f * side, -0.92f, 0.6f), 0.14f, 0.12f, 6, EnemySwatch.GolemStoneDark);
                    b.Blob(new Vector3(-0.1f * side, -0.97f, 0.63f), new Vector3(0.28f, 0.26f, 0.28f), EnemySwatch.GolemStone, coarse: true);
                }, meshKey: right ? "LastStepArchonArm_R" : "LastStepArchonArm_L");
            }
        }

        // ------------------------------------------------------------------
        // A Rachadura Grande (99)
        // ------------------------------------------------------------------

        public static void BuildGreatCrack(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // The pool of shadow it has dripped onto the Lookout.
                b.Cylinder(Vector3.zero, 0.95f, 0.85f, 0.06f, 12, EnemySwatch.Black);
                // Two jagged edges of sky, the gold of the sky round them, and the slit of night between them.
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float jag = 0.06f * ((i * 7 + (s > 0 ? 3 : 0)) % 3);
                        b.Box(new Vector3((0.36f + jag) * s, 0.32f + i * 0.5f, 0f), new Vector3(0.36f, 0.56f, 0.3f), i % 2 == 0 ? EnemySwatch.GolemSnow : EnemySwatch.Ivory,
                            Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 6f : -5f) * s));
                        b.Box(new Vector3((0.17f + jag * 0.5f) * s, 0.32f + i * 0.5f, 0.08f), new Vector3(0.04f, 0.5f, 0.16f), EnemySwatch.Gold);
                    }
                }

                b.Box(new Vector3(0f, 1.6f, 0f), new Vector3(0.3f, 3f, 0.2f), EnemySwatch.Shadow);
                // The shadow running down it like tar.
                for (int i = 0; i < 5; i++)
                {
                    b.Blob(new Vector3(0.04f * ((i & 1) * 2 - 1), 0.35f + i * 0.55f, 0.12f), new Vector3(0.18f, 0.32f, 0.14f), EnemySwatch.Black, coarse: true);
                }
            });
            // The sky behind it, shining through: the more it is sealed, the more it shows.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.5f, 0.18f), b => b.Blob(Vector3.zero, new Vector3(0.24f, 0.36f, 0.12f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.6f, 0.18f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.48f, 0.5f, 0.32f), EnemySwatch.Shadow, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.03f, 0.14f, 0.08f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.16f, 0.12f), b => b.Blob(Vector3.zero, new Vector3(0.2f, 0.06f, 0.08f), EnemySwatch.Black, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Two long runs of tar that lift on the windup, as the crack opens.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.24f * side, 2.05f, 0.2f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.32f * side, -0.55f, 0.18f), 0.1f, 0.07f, 6, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.32f * side, -0.55f, 0.18f), new Vector3(0.28f * side, -1f, 0.28f), 0.07f, 0.04f, 5, EnemySwatch.Black);
                }, meshKey: right ? "GreatCrackArm_R" : "GreatCrackArm_L");
            }
        }
    }
}
