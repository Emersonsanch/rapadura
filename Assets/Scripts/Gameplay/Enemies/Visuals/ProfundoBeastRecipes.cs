using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Limiar Profundo (Docs/Mapas100.md 84–90) that the continuous world played with stand-ins, low-poly from the
    /// shared palette like <see cref="MisturaBeastRecipes"/>: one or two of each of the five pits and the gate. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Sombra-Cópia</b> (84, ~1.6 m): a thin copy of whoever walks in, blank face, amber seam. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Gigante de Fumaça</b> (84, ~2.7 m): rings of smoke stacked into a giant. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Arqueiro de Fumaça</b> (84, ~1.5 m): drifting smoke with a copper bow and a quiver. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (sleeves), <c>Tail</c>.</item>
    /// <item><b>Lobo de Brasa</b> (85, ~1 m): ash fur with embers down the spine. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Leão de Fumaça Quente</b> (85, ~1.4 m): basalt lion, a mane of smoke with fire inside. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four heavy legs.</item>
    /// <item><b>Águia Negra</b> (86, ~1.2 m): black eagle with white eyes on long legs. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Harpia de Vento</b> (86, ~1.2 m): grey-blue harpy, throws feathers. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Polvo de Lodo</b> (87, ~1.2 m): a dome of mire on eight arms, amber caught inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Peixe da Treva</b> (87, ~0.8 m): flat black fish with an amber seam, swims in the air. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (fins), <c>Tail</c>.</item>
    /// <item><b>Lagarto-Sombra</b> (88, ~1 m, 2.6 m long): low and very wide, nearly no eyes. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four heavy legs.</item>
    /// <item><b>Dragão-Sombra Menor</b> (88, ~1.4 m): small dark dragon with useless wing plates. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Arconte Menor</b> (90, ~2.5 m): fat with swallowed light, a crown of dead lamps. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// </list>
    /// </summary>
    public static class ProfundoBeastRecipes
    {
        // ------------------------------------------------------------------
        // Sombra-Cópia
        // ------------------------------------------------------------------

        public static void BuildCopyShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.92f, 0f), DrawCopyShadowBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.46f, 0.02f), b =>
            {
                // No face yet: it is waiting to see yours.
                b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.3f, 0.34f, 0.3f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, 0.1f, 0.13f), new Vector3(0.14f, 0.02f, 0.02f), EnemySwatch.StolenLight);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.33f * side, 0.26f, 0f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.34f, 0.06f), 0.07f, 0.055f, 6, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.06f * side, -0.34f, 0.06f), new Vector3(0.08f * side, -0.66f, 0.1f), 0.055f, 0.04f, 6, EnemySwatch.Black);
                    b.Blob(new Vector3(0.08f * side, -0.7f, 0.1f), new Vector3(0.13f, 0.16f, 0.13f), EnemySwatch.Shroud, coarse: true);
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: s > 0 ? "CopyShadowArm_R" : "CopyShadowArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.6f, 0.15f, 0.1f, EnemySwatch.Shadow, EnemySwatch.Black, EnemySwatch.Shroud, "CopyShadow");
        }

        private static void DrawCopyShadowBody(EnemyMeshBuilder b)
        {
            // The first world of the Dark walks on two legs and never learned to be anybody. It borrows the way you stand and returns it with bruises.
            b.Cylinder(new Vector3(0f, -0.36f, 0f), 0.2f, 0.3f, 0.66f, 9, EnemySwatch.Shadow, bottomCap: true);
            b.Blob(new Vector3(0f, 0.2f, 0f), new Vector3(0.56f, 0.3f, 0.5f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.44f, 0.56f, 0.4f), EnemySwatch.Black);
            b.Box(new Vector3(0.05f, 0f, 0.22f), new Vector3(0.03f, 0.52f, 0.02f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 0f, 6f));
        }

        // ------------------------------------------------------------------
        // Gigante de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeGiant(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.42f, 0f), DrawSmokeGiantBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.84f, 0.08f), b =>
            {
                b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.62f, 0.52f, 0.56f), EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, 0.26f, -0.06f), new Vector3(0.7f, 0.34f, 0.64f), EnemySwatch.GolemStone, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.14f, 0.1f, 0.25f, 0.08f, EnemySwatch.StolenLight);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.72f * side, 0.46f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.42f, 0.4f, 0.42f), EnemySwatch.GolemStone, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.14f * side, -0.72f, 0.08f), 0.2f, 0.17f, 6, EnemySwatch.GolemStoneDark);
                    b.Blob(new Vector3(0.14f * side, -0.86f, 0.08f), new Vector3(0.48f, 0.46f, 0.48f), EnemySwatch.GolemStone);
                }, Quaternion.Euler(-12f, 0f, 0f), meshKey: s > 0 ? "SmokeGiantArm_R" : "SmokeGiantArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.96f, 0.32f, 0.22f, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone, EnemySwatch.Black, "SmokeGiant");
        }

        private static void DrawSmokeGiantBody(EnemyMeshBuilder b)
        {
            // Rings of smoke that stacked themselves up and forgot how to come down. Wind takes it apart; a sword only annoys it.
            for (int i = 0; i < 4; i++)
            {
                float y = -0.5f + i * 0.34f;
                float width = 1.22f - Mathf.Abs(i - 1.5f) * 0.14f;
                b.Blob(new Vector3(0f, y, 0f), new Vector3(width, 0.46f, width * 0.84f), i % 2 == 0 ? EnemySwatch.GolemStoneDark : EnemySwatch.GolemStone);
            }

            b.Blob(new Vector3(0f, 0.1f, 0.34f), new Vector3(0.34f, 0.3f, 0.2f), EnemySwatch.StolenLight, coarse: true);
            b.Box(new Vector3(0f, 0.02f, 0.5f), new Vector3(0.6f, 0.04f, 0.03f), EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Arqueiro de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeArcher(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawSmokeArcherBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.54f, 0.04f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.3f, 0.26f), EnemySwatch.GolemStoneDark);
                b.Cylinder(new Vector3(0f, 0.08f, 0f), 0.2f, 0f, 0.26f, 7, EnemySwatch.Copper, bottomCap: true);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.02f, 0.11f, 0.04f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.03f), new Vector3(0.12f, 0.03f, 0.08f), EnemySwatch.Black));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.22f, 0.22f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.GolemStoneDark, EnemySwatch.Shroud, EnemySwatch.Ember, 0.72f), Quaternion.Euler(0f, 0f, -24f), meshKey: "SmokeArcherWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.22f, 0.22f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.GolemStoneDark, EnemySwatch.Shroud, EnemySwatch.Ember, 0.72f), Quaternion.Euler(0f, 0f, 24f), meshKey: "SmokeArcherWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.3f, -0.22f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.3f, -0.56f), 0.2f, 0.08f, 6, EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, -0.32f, -0.62f), new Vector3(0.2f, 0.1f, 0.3f), EnemySwatch.Shroud, coarse: true);
            });
        }

        private static void DrawSmokeArcherBody(EnemyMeshBuilder b)
        {
            // It keeps the shape of the last soldier the Artificers' vents blew out of the pit, down to the bow. The arrow is only hot smoke that knows where it is going.
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.54f, 0.86f, 0.42f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, -0.3f, 0f), new Vector3(0.62f, 0.5f, 0.54f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.63f, 0f), new Vector3(0.72f, 0.2f, 0.66f), EnemySwatch.Shroud, coarse: true);

            // The bow on the left, the arrow nocked, the quiver on the back.
            Vector3 top = new Vector3(-0.32f, 0.56f, 0.28f);
            Vector3 grip = new Vector3(-0.42f, 0.12f, 0.36f);
            Vector3 bottom = new Vector3(-0.32f, -0.32f, 0.28f);
            b.Segment(top, grip, 0.018f, 0.03f, 5, EnemySwatch.Copper);
            b.Segment(grip, bottom, 0.03f, 0.018f, 5, EnemySwatch.Copper);
            b.Segment(top, bottom, 0.006f, 0.006f, 3, EnemySwatch.Bone);
            b.Segment(new Vector3(-0.3f, 0.12f, 0.34f), new Vector3(-0.3f, 0.12f, 0.86f), 0.012f, 0.012f, 3, EnemySwatch.Bone);
            b.Cylinder(new Vector3(-0.3f, 0.12f, 0.86f), 0.03f, 0f, 0.1f, 4, EnemySwatch.Ember, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Cylinder(new Vector3(0.08f, -0.08f, -0.2f), 0.08f, 0.1f, 0.5f, 6, EnemySwatch.CopperDark, rotation: Quaternion.Euler(-18f, 0f, 10f));
        }

        // ------------------------------------------------------------------
        // Lobo de Brasa
        // ------------------------------------------------------------------

        public static void BuildEmberWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.68f, 0f), DrawEmberWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.62f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.3f, 0.26f, 0.3f), EnemySwatch.WolfFurDark);
                b.Box(new Vector3(0f, -0.03f, 0.24f), new Vector3(0.14f, 0.1f, 0.26f), EnemySwatch.WolfFurDark);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.06f, 0.14f, 0.05f, EnemySwatch.Ember);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.09f * s, 0.12f, -0.04f), 0.05f, 0f, 0.16f, 4, EnemySwatch.Black);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.12f), new Vector3(0.12f, 0.04f, 0.24f), EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.012f, 0.12f), new Vector3(0.09f, 0.01f, 0.2f), EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.6f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.08f, -0.42f), 0.07f, 0.04f, 5, EnemySwatch.WolfFurDark);
                b.Blob(new Vector3(0f, -0.1f, -0.5f), new Vector3(0.13f, 0.13f, 0.2f), EnemySwatch.Ember, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.56f, 0.22f, 0.38f, -0.42f, 0.075f, EnemySwatch.WolfFurDark, EnemySwatch.Black, "EmberWolf");
        }

        private static void DrawEmberWolfBody(EnemyMeshBuilder b)
        {
            // The second world goes on four legs and eats whatever is warm. Its pack does not hunt; it just goes where the embers go.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.5f, 1.22f), EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0f, -0.12f, 0f), new Vector3(0.46f, 0.24f, 1.04f), EnemySwatch.BatFurDark, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                float z = -0.46f + i * 0.19f;
                b.Box(new Vector3(0f, 0.25f, z), new Vector3(0.05f, 0.05f, 0.09f), i % 2 == 0 ? EnemySwatch.Ember : EnemySwatch.StolenLight, Quaternion.Euler(0f, 45f, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Leão de Fumaça Quente
        // ------------------------------------------------------------------

        public static void BuildHotSmokeLion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.86f, 0f), DrawHotSmokeLionBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.26f, 0.72f), DrawHotSmokeLionHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.18f), b =>
            {
                b.Box(new Vector3(0f, -0.03f, 0.12f), new Vector3(0.34f, 0.08f, 0.26f), EnemySwatch.GolemStoneDark);
                b.Box(new Vector3(0f, 0.015f, 0.12f), new Vector3(0.24f, 0.012f, 0.2f), EnemySwatch.Ember);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.12f * s, 0.01f, 0.2f), 0.02f, 0f, 0.07f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.16f, -0.76f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.12f, -0.46f), 0.07f, 0.045f, 5, EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, 0.14f, -0.52f), new Vector3(0.2f, 0.2f, 0.24f), EnemySwatch.GolemStone, coarse: true);
                b.Blob(new Vector3(0f, 0.14f, -0.52f), new Vector3(0.09f, 0.09f, 0.1f), EnemySwatch.Ember, coarse: true);
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.72f, 0.32f, 0.48f, -0.48f, 0.11f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, "HotSmokeLion");
        }

        private static void DrawHotSmokeLionBody(EnemyMeshBuilder b)
        {
            // Basalt on the outside and furnace on the inside. It breathes out, and the corridor is a little harder to see for a while.
            b.Blob(Vector3.zero, new Vector3(0.94f, 0.76f, 1.6f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, -0.18f, 0f), new Vector3(0.72f, 0.3f, 1.3f), EnemySwatch.BatFurDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0.45f * s, 0.06f - (i % 2) * 0.1f, -0.42f + i * 0.28f), new Vector3(0.03f, 0.16f, 0.04f), EnemySwatch.Ember, Quaternion.Euler(0f, 0f, 16f * s));
                }
            }

            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0.05f * (i % 2 == 0 ? 1f : -1f), 0.38f + i * 0.08f, -0.2f + i * 0.2f), new Vector3(0.26f, 0.22f, 0.28f), EnemySwatch.GolemStone, coarse: true);
            }
        }

        private static void DrawHotSmokeLionHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.04f, 0.04f), new Vector3(0.5f, 0.46f, 0.44f), EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0f, -0.06f, 0.26f), new Vector3(0.34f, 0.14f, 0.24f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.16f, -0.12f), new Vector3(0.76f, 0.68f, 0.36f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.16f, -0.08f), new Vector3(0.5f, 0.42f, 0.26f), EnemySwatch.Ember, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.12f, 0.2f, 0.07f, EnemySwatch.StolenLight);
        }

        // ------------------------------------------------------------------
        // Águia Negra
        // ------------------------------------------------------------------

        public static void BuildBlackEagle(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawBlackEagleBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.3f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.2f, 0.2f, 0.26f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.06f, 0.08f, 0.05f, EnemySwatch.OwlFace);
                b.Horn(new Vector3(0f, 0.02f, 0.12f), 0.05f, 0.16f, 3, 22f, EnemySwatch.Gold, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.12f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.07f, 0.025f, 0.12f), EnemySwatch.Gold));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.15f, 0.12f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.Sorcery, 1f), Quaternion.Euler(0f, 0f, -14f), meshKey: "BlackEagleWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.15f, 0.12f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.Sorcery, 1f), Quaternion.Euler(0f, 0f, 14f), meshKey: "BlackEagleWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.04f, -0.3f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.Shadow), Quaternion.Euler(16f, 0f, 0f));
        }

        private static void DrawBlackEagleBody(EnemyMeshBuilder b)
        {
            // An eagle borrowed the black wings for a single afternoon and the third world never asked for them back. It still hunts like a bird: from above, and only once.
            b.Blob(Vector3.zero, new Vector3(0.38f, 0.42f, 0.74f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, 0.08f, -0.06f), new Vector3(0.3f, 0.2f, 0.5f), EnemySwatch.Shadow, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.2f), new Vector3(0f, 0.3f, 0.3f), 0.11f, 0.08f, 6, EnemySwatch.Black);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.09f * s, -0.16f, 0f), new Vector3(0.09f * s, -0.54f, 0.02f), 0.04f, 0.028f, 4, EnemySwatch.Shadow);
            }

            RangedEnemyRecipes.DrawTalons(b, -0.72f, 0.09f, EnemySwatch.Gold);
        }

        // ------------------------------------------------------------------
        // Harpia de Vento
        // ------------------------------------------------------------------

        public static void BuildWindHarpy(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawWindHarpyBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.34f, 0.12f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.26f, 0.24f), EnemySwatch.CaveBelly);
                b.Blob(new Vector3(0f, 0.08f, -0.06f), new Vector3(0.34f, 0.26f, 0.34f), EnemySwatch.CaveSkin, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.02f, 0.1f, 0.04f, EnemySwatch.Crystal);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.03f), new Vector3(0.12f, 0.03f, 0.08f), EnemySwatch.CaveSkinDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.16f, 0.16f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark, EnemySwatch.Crystal, 0.92f), Quaternion.Euler(0f, 0f, -20f), meshKey: "WindHarpyWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.16f, 0.16f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark, EnemySwatch.Crystal, 0.92f), Quaternion.Euler(0f, 0f, 20f), meshKey: "WindHarpyWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.08f, -0.2f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark), Quaternion.Euler(22f, 0f, 0f));
        }

        private static void DrawWindHarpyBody(EnemyMeshBuilder b)
        {
            // What the wind leaves behind when it takes the feathers it wanted. It sends the rest after you, one at a time, sharpened.
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.5f, 0.44f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, 0.12f, 0.06f), new Vector3(0.26f, 0.26f, 0.24f), EnemySwatch.CaveBelly, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(0f, -0.16f - i * 0.06f, -0.08f - i * 0.04f), new Vector3(0.28f - i * 0.05f, 0.12f, 0.28f - i * 0.05f), EnemySwatch.CaveSkin, coarse: true);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.07f * s, -0.2f, 0f), new Vector3(0.08f * s, -0.54f, 0.04f), 0.035f, 0.024f, 4, EnemySwatch.CaveSkinDark);
            }

            RangedEnemyRecipes.DrawTalons(b, -0.72f, 0.08f, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Polvo de Lodo
        // ------------------------------------------------------------------

        public static void BuildMudOctopus(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawMudOctopus);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.86f, 0.3f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.2f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0.12f, -0.1f, -0.02f), new Vector3(0.12f, 0.12f, 0.1f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawMudOctopus(EnemyMeshBuilder b)
        {
            // The fourth world swims, and this one stopped bothering. It is more or less the floor of the pit with an opinion.
            b.Blob(new Vector3(0f, 0.72f, 0f), new Vector3(1.04f, 0.96f, 1.04f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, 1f, -0.06f), new Vector3(0.74f, 0.5f, 0.74f), EnemySwatch.CaveMoss, coarse: true);
            b.Blob(new Vector3(0f, 0.42f, 0f), new Vector3(1.1f, 0.3f, 1.1f), EnemySwatch.Black);
            HinokuraBeastRecipes.AddEyes(b, 0.22f, 0.78f, 0.45f, 0.13f, EnemySwatch.Ember);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f + 0.39f;
                Vector3 root = new Vector3(Mathf.Sin(a) * 0.42f, 0.34f, Mathf.Cos(a) * 0.42f);
                Vector3 knee = new Vector3(Mathf.Sin(a) * 0.72f, 0.18f, Mathf.Cos(a) * 0.72f);
                Vector3 tip = new Vector3(Mathf.Sin(a + 0.2f) * 0.98f, 0.03f, Mathf.Cos(a + 0.2f) * 0.98f);
                b.Segment(root, knee, 0.11f, 0.08f, 5, EnemySwatch.CaveSkinDark);
                b.Segment(knee, tip, 0.08f, 0.03f, 5, i % 2 == 0 ? EnemySwatch.Black : EnemySwatch.CaveSkinDark);
                b.Blob(knee + Vector3.up * 0.05f, new Vector3(0.06f, 0.05f, 0.06f), EnemySwatch.StolenLight, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Peixe da Treva
        // ------------------------------------------------------------------

        public static void BuildDarkFish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawDarkFishBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.01f, 0.48f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.3f, 0.3f), EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.05f, 0.14f), new Vector3(0.1f, 0.06f, 0.14f), EnemySwatch.Shadow);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.06f, 0.08f, 0.06f, EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.06f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.09f, 0.025f, 0.2f), EnemySwatch.Shadow);
                b.Cylinder(new Vector3(0f, 0.005f, 0.18f), 0.014f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.1f, -0.06f, 0.2f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.StolenLight, 0.52f), Quaternion.Euler(0f, 0f, -18f), meshKey: "DarkFishFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.1f, -0.06f, 0.2f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.StolenLight, 0.52f), Quaternion.Euler(0f, 0f, 18f), meshKey: "DarkFishFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.48f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.22f), 0.08f, 0.03f, 5, EnemySwatch.Black);
                b.TwoSided(new Vector3(0f, 0f, -0.2f), new Vector3(0f, 0.28f, -0.48f), new Vector3(0f, 0.04f, -0.34f), EnemySwatch.Shadow, EnemySwatch.Black);
                b.TwoSided(new Vector3(0f, 0f, -0.2f), new Vector3(0f, -0.04f, -0.34f), new Vector3(0f, -0.26f, -0.46f), EnemySwatch.Shadow, EnemySwatch.Black);
            });
        }

        private static void DrawDarkFishBody(EnemyMeshBuilder b)
        {
            // There is no water where it swims any more. Nobody has told it, and it would not change anything if they did.
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.44f, 1.1f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.1f, 0.02f), new Vector3(0.16f, 0.2f, 0.86f), EnemySwatch.Shadow, coarse: true);
            b.Box(new Vector3(0f, 0.02f, 0f), new Vector3(0.24f, 0.02f, 0.62f), EnemySwatch.StolenLight);
            for (int i = 0; i < 5; i++)
            {
                b.Crystal(new Vector3(0f, 0.18f, -0.3f + i * 0.14f), 0.03f, 0.13f + (i % 2) * 0.06f, EnemySwatch.Shadow, sides: 4);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.04f * s, -0.18f, 0.06f), new Vector3(0.05f * s, -0.39f, -0.04f), new Vector3(0.04f * s, -0.18f, -0.12f), EnemySwatch.Black, EnemySwatch.Shadow);
            }
        }

        // ------------------------------------------------------------------
        // Lagarto-Sombra
        // ------------------------------------------------------------------

        public static void BuildShadowLizard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawShadowLizardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.9f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.06f), new Vector3(0.56f, 0.32f, 0.56f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, -0.02f, 0.3f), new Vector3(0.4f, 0.12f, 0.34f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.17f, 0.12f, 0.1f, 0.05f, EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.16f), b =>
            {
                b.Box(new Vector3(0f, -0.02f, 0.16f), new Vector3(0.38f, 0.06f, 0.34f), EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.012f, 0.16f), new Vector3(0.3f, 0.01f, 0.28f), EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.86f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.12f, -0.56f), 0.2f, 0.12f, 6, EnemySwatch.Shadow);
                b.Segment(new Vector3(0f, -0.12f, -0.56f), new Vector3(0f, -0.28f, -1.08f), 0.12f, 0.03f, 5, EnemySwatch.Black);
                for (int i = 0; i < 4; i++)
                {
                    b.Crystal(new Vector3(0f, 0.04f - i * 0.08f, -0.2f - i * 0.2f), 0.04f, 0.14f, EnemySwatch.Sorcery, sides: 4);
                }
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.46f, 0.42f, 0.52f, -0.52f, 0.11f, EnemySwatch.Shadow, EnemySwatch.Black, "ShadowLizard");
        }

        private static void DrawShadowLizardBody(EnemyMeshBuilder b)
        {
            // Fifth world: the ones that crawl. It has been so long in the dark that its eyes went small and the rest of it went wide to make up for it.
            b.Blob(Vector3.zero, new Vector3(1.14f, 0.58f, 2.02f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.16f, 0f), new Vector3(1f, 0.26f, 1.76f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, 0.2f, 0.14f), new Vector3(0.8f, 0.3f, 1.24f), EnemySwatch.BatFurDark, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                float z = -0.68f + i * 0.34f;
                b.Box(new Vector3(0f, 0.31f, z), new Vector3(0.1f, 0.05f, 0.14f), EnemySwatch.Sorcery, Quaternion.Euler(0f, 45f, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Dragão-Sombra Menor
        // ------------------------------------------------------------------

        public static void BuildLesserShadowDragon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.78f, 0f), DrawLesserShadowDragonBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.5f, 0.78f), DrawLesserShadowDragonHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.14f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.14f), new Vector3(0.2f, 0.05f, 0.3f), EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.016f, 0.14f), new Vector3(0.14f, 0.01f, 0.24f), EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.68f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.12f, -0.54f), 0.15f, 0.09f, 6, EnemySwatch.Shadow);
                b.Segment(new Vector3(0f, -0.12f, -0.54f), new Vector3(0f, -0.06f, -1.02f), 0.09f, 0.02f, 5, EnemySwatch.Black);
                b.Cylinder(new Vector3(0f, -0.06f, -1.02f), 0.09f, 0f, 0.2f, 4, EnemySwatch.Gold, rotation: Quaternion.Euler(-90f, 0f, 0f), bottomCap: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.62f, 0.26f, 0.46f, -0.46f, 0.09f, EnemySwatch.Shadow, EnemySwatch.Gold, "LesserShadowDragon");
        }

        private static void DrawLesserShadowDragonBody(EnemyMeshBuilder b)
        {
            // A scrap off the great body that is shut behind the seal; it remembers being enormous, and it behaves accordingly.
            b.Blob(Vector3.zero, new Vector3(0.74f, 0.62f, 1.56f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.14f, 0f), new Vector3(0.56f, 0.3f, 1.32f), EnemySwatch.Black, coarse: true);
            b.Segment(new Vector3(0f, 0.1f, 0.6f), new Vector3(0f, 0.5f, 0.78f), 0.19f, 0.12f, 6, EnemySwatch.Shadow);
            for (int i = 0; i < 6; i++)
            {
                float z = -0.58f + i * 0.22f;
                b.Crystal(new Vector3(0f, 0.26f, z), 0.05f, 0.16f + (i % 2) * 0.08f, EnemySwatch.Gold, sides: 4);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                // The wings never finished: two leathery plates that fold against the sides.
                b.TwoSided(new Vector3(0.28f * s, 0.2f, 0.26f), new Vector3(0.62f * s, 0.34f, -0.2f), new Vector3(0.34f * s, 0.16f, -0.36f), EnemySwatch.Black, EnemySwatch.Sorcery);
                b.TwoSided(new Vector3(0.28f * s, 0.2f, 0.26f), new Vector3(0.34f * s, 0.16f, -0.36f), new Vector3(0.3f * s, 0.14f, -0.5f), EnemySwatch.Shadow, EnemySwatch.Black);
            }
        }

        private static void DrawLesserShadowDragonHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.3f, 0.26f, 0.32f), EnemySwatch.Shadow);
            b.Box(new Vector3(0f, -0.01f, 0.22f), new Vector3(0.22f, 0.1f, 0.3f), EnemySwatch.Shadow);
            HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.07f, 0.13f, 0.05f, EnemySwatch.StolenLight);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Horn(new Vector3(0.09f * s, 0.1f, -0.06f), 0.04f, 0.3f, 4, 16f, EnemySwatch.Gold, Quaternion.Euler(-40f, 0f, 0f), 5, Vector3.right);
            }
        }

        // ------------------------------------------------------------------
        // Arconte Menor
        // ------------------------------------------------------------------

        public static void BuildLesserArchon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.34f, 0f), DrawLesserArchonBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 0.1f), DrawLesserArchonHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.68f * side, 0.42f, 0.02f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.36f, 0.34f, 0.36f), EnemySwatch.IronDark, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.14f * side, -0.62f, 0.08f), 0.15f, 0.13f, 6, EnemySwatch.Leather);
                    b.Blob(new Vector3(0.14f * side, -0.76f, 0.08f), new Vector3(0.4f, 0.38f, 0.4f), EnemySwatch.IronDark);
                }, Quaternion.Euler(-12f, 0f, 0f), meshKey: s > 0 ? "LesserArchonArm_R" : "LesserArchonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.9f, 0.3f, 0.2f, EnemySwatch.Leather, EnemySwatch.IronDark, EnemySwatch.Black, "LesserArchon");
        }

        private static void DrawLesserArchonBody(EnemyMeshBuilder b)
        {
            // The small ones at the gate are the ones who got here late and swallowed what the big ones dropped. The light shows through at the belly, and it is not comfortable.
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(1.24f, 1.2f, 1f), EnemySwatch.Leather);
            b.Blob(new Vector3(0f, 0.34f, 0f), new Vector3(1.12f, 0.56f, 0.86f), EnemySwatch.IronDark, coarse: true);
            b.Blob(new Vector3(0f, -0.14f, 0.36f), new Vector3(0.6f, 0.58f, 0.34f), EnemySwatch.StolenLight);
            b.Blob(new Vector3(0f, -0.14f, 0.42f), new Vector3(0.34f, 0.32f, 0.24f), EnemySwatch.Gold, coarse: true);
            b.Box(new Vector3(0f, -0.46f, 0.02f), new Vector3(1.1f, 0.12f, 0.9f), EnemySwatch.CopperDark);
            for (int i = 0; i < 5; i++)
            {
                float x = -0.4f + i * 0.2f;
                b.Box(new Vector3(x, -0.46f, 0.47f), new Vector3(0.05f, 0.1f, 0.03f), EnemySwatch.StolenLight);
            }
        }

        private static void DrawLesserArchonHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.6f, 0.5f, 0.54f), EnemySwatch.Leather);
            b.Box(new Vector3(0f, -0.08f, 0.22f), new Vector3(0.42f, 0.12f, 0.2f), EnemySwatch.IronDark);
            HinokuraBeastRecipes.AddEyes(b, 0.14f, 0.12f, 0.23f, 0.08f, EnemySwatch.WarEye);
            b.Cylinder(new Vector3(0f, 0.3f, -0.02f), 0.36f, 0.4f, 0.1f, 10, EnemySwatch.CopperDark, cap: EnemySwatch.Black);
            for (int i = 0; i < 6; i++)
            {
                // Lamps it took and could not keep lit.
                float a = i * Mathf.PI * 2f / 6f + 0.26f;
                Vector3 p = new Vector3(Mathf.Sin(a) * 0.35f, 0.4f, Mathf.Cos(a) * 0.35f - 0.02f);
                b.Cylinder(p, 0.05f, 0.03f, 0.14f, 5, EnemySwatch.Copper, cap: i % 3 == 0 ? EnemySwatch.Ember : EnemySwatch.Black);
            }
        }
    }
}
