using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The world bosses of the continuous Regions 5–16 (Docs/Mapas100.md 30, 35, 40, 46, 52, 58, 64, 70, 76, 82, 90, 96), low-poly from the shared palette. Pivot at the feet (y = 0), facing +Z.
    /// Each carries a glowing <c>Core</c> the rig feeds with its fight phase (<see cref="RegionBossRules.PhaseFor"/>).
    /// <list type="bullet">
    /// <item><b>O Rei-Vendaval</b> (58, ~3 m): a tall bird of grey wind with no face at all, a seam of light where one should be, 3.4 m of wings. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>; two legs.</item>
    /// <item><b>O Velho Kul</b> (64, ~2.8 m): an old bent water spirit, hair of weed, silver fish held in the ice of its chest. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>O Glutão de Brasa</b> (70, ~3 m): a fat cone of slag, no face, one huge mouth with the forge fire it stole inside. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Apep</b> (76, ~2.7 m): a coiled serpent, her neck straight up, gold bands down the throat. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; no legs.</item>
    /// <item><b>O Sonho da Destruição</b> (82, ~2.7 m): a hooded shadow with a broken tablet in its chest and shards in its hands. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>A Grande Treva</b> (90, ~3 m): the five forms in silhouette, lion head, eagle wings, dragon body, fish tail, demon feet. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>; two legs.</item>
    /// <item><b>Āz, a Capitã Cobiçosa</b> (96, ~2.8 m): tricorn and long coat, the Última Arca held out in front (the <c>Core</c>). <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Hedammu</b> (30, ~2.8 m): the gluttonous sea serpent coiled with her mouth open, the sparks she swallowed lit in the coils. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; no legs.</item>
    /// <item><b>O Lodo-que-Engole</b> (35, ~2.7 m): faceless black mud standing up, smoke bubbles, the Água Viva it swallowed in its chest. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Aydahar</b> (40, ~3.1 m): the snow-pale dragon of the pass, scales gone gold, the fire banked in its chest. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>; two legs.</item>
    /// <item><b>Os Quatro Gulosos</b> (46, ~2.9 m): a walking bronze vessel with a face on each side (Taotie, Hundun, Qiongqi, Taowu). <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Ohya</b> (52, ~2.9 m): Hahya's giant brother in hides with a chain across his chest, one link already lit. <c>Body</c> → <c>Core</c>, <c>Head</c> → <c>Jaw</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// </list>
    /// </summary>
    public static class RegionBossRecipes
    {
        // ------------------------------------------------------------------
        // O Rei-Vendaval
        // ------------------------------------------------------------------

        public static void BuildWindKing(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.62f, 0f), b =>
            {
                // It has been pushing at the nest for a very long time and never once looked at what it was pushing.
                b.Blob(Vector3.zero, new Vector3(0.95f, 1.1f, 1.05f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(0f, -0.28f, 0.14f), new Vector3(0.74f, 0.66f, 0.72f), EnemySwatch.Steel);
                b.Blob(new Vector3(0f, 0.42f, -0.05f), new Vector3(0.72f, 0.5f, 0.74f), EnemySwatch.GolemStoneDark, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.05f, 0.42f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.42f, 0.2f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 0.2f), b =>
            {
                b.Blob(new Vector3(0f, 0.16f, 0f), new Vector3(0.56f, 0.6f, 0.6f), EnemySwatch.Steel);
                // No eyes. The seam is the only thing that ever looks back at you.
                b.Box(new Vector3(0f, 0.2f, 0.29f), new Vector3(0.05f, 0.34f, 0.03f), EnemySwatch.Rune);
                b.Horn(new Vector3(0f, 0.42f, -0.05f), 0.11f, 0.5f, 4, -10f, EnemySwatch.Gold, Quaternion.Euler(-20f, 0f, 0f));
                b.Crystal(new Vector3(0f, 0.08f, 0.3f), 0.09f, 0.34f, EnemySwatch.Gold, Quaternion.Euler(80f, 0f, 0f));
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.02f, 0.27f), b => b.Crystal(Vector3.zero, 0.07f, 0.26f, EnemySwatch.Horn, Quaternion.Euler(70f, 0f, 0f)));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.WingRight : ProceduralEnemyParts.WingLeft, body, new Vector3(0.42f * side, 0.3f, -0.08f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.62f * side, 0.2f, -0.08f), 0.16f, 0.1f, 6, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.62f * side, 0.2f, -0.08f), new Vector3(1.24f * side, -0.05f, -0.16f), 0.1f, 0.05f, 5, EnemySwatch.Steel);
                    for (int i = 0; i < 6; i++)
                    {
                        float t = i / 5f;
                        Vector3 root = new Vector3(Mathf.Lerp(0.12f, 1.15f, t) * side, Mathf.Lerp(0.08f, 0f, t), -0.1f);
                        b.Box(root + new Vector3(0f, -0.34f, -0.03f), new Vector3(0.22f, 0.72f - 0.06f * i, 0.04f), i % 2 == 0 ? EnemySwatch.GolemStoneDark : EnemySwatch.Shroud);
                    }

                    b.Box(new Vector3(0.7f * side, -0.05f, -0.06f), new Vector3(0.05f, 0.5f, 0.03f), EnemySwatch.Rune);
                }, meshKey: s > 0 ? "WindKingWing_R" : "WindKingWing_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.35f, -0.45f), b =>
            {
                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(i * 0.14f, -0.08f, -0.42f), new Vector3(0.14f, 0.08f, 0.9f), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Steel, Quaternion.Euler(0f, i * 8f, 0f));
                }
            });

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.2f, 0.28f, 0.13f, EnemySwatch.GolemStoneDark, EnemySwatch.Horn, EnemySwatch.Gold, "WindKing");
        }

        // ------------------------------------------------------------------
        // O Velho Kul
        // ------------------------------------------------------------------

        public static void BuildOldKul(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.32f, 0f), b =>
            {
                // A water spirit that went sour: it shut every fish in its chest so that nobody could say it had none.
                b.Blob(new Vector3(0f, 0.02f, -0.05f), new Vector3(1.34f, 1.4f, 1.16f), EnemySwatch.CaveSkin, coarse: true);
                b.Blob(new Vector3(0f, -0.3f, 0.14f), new Vector3(1.06f, 0.82f, 0.9f), EnemySwatch.CaveBelly);
                b.Blob(new Vector3(0f, 0.56f, -0.22f), new Vector3(1.02f, 0.72f, 0.82f), EnemySwatch.CaveSkinDark, coarse: true);
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    b.Cylinder(new Vector3(Mathf.Cos(a) * 0.5f, -0.6f, Mathf.Sin(a) * 0.42f - 0.05f), 0.09f, 0.05f, 0.52f, 5, EnemySwatch.CaveMoss, bottomCap: true);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.1f, 0.5f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.46f, 0.52f, 0.2f), EnemySwatch.Crystal, coarse: true);
                b.Box(new Vector3(0.06f, 0.05f, 0.1f), new Vector3(0.22f, 0.06f, 0.03f), EnemySwatch.Steel);
                b.Box(new Vector3(-0.08f, -0.1f, 0.1f), new Vector3(0.18f, 0.05f, 0.03f), EnemySwatch.Steel);
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.76f, 0.3f), b =>
            {
                b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.74f, 0.62f, 0.68f), EnemySwatch.CaveSkin);
                b.Blob(new Vector3(0f, 0.34f, -0.1f), new Vector3(0.84f, 0.42f, 0.74f), EnemySwatch.CaveMoss, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.18f, 0.16f, 0.3f, 0.1f, EnemySwatch.SoftEye);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.18f * s, -0.02f, 0.27f), new Vector3(0.36f * s, -0.42f, 0.34f), 0.05f, 0.02f, 4, EnemySwatch.Bone);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.14f), b => b.Blob(new Vector3(0f, -0.08f, 0.1f), new Vector3(0.56f, 0.2f, 0.42f), EnemySwatch.CaveSkinDark, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.68f * side, 0.42f, 0.05f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.4f, 0.38f, 0.4f), EnemySwatch.CaveSkinDark, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.2f * side, -0.72f, 0.16f), 0.17f, 0.13f, 6, EnemySwatch.CaveSkin);
                    b.Segment(new Vector3(0.2f * side, -0.72f, 0.16f), new Vector3(0.24f * side, -1.16f, 0.3f), 0.13f, 0.1f, 6, EnemySwatch.CaveSkinDark);
                    b.Blob(new Vector3(0.24f * side, -1.24f, 0.32f), new Vector3(0.4f, 0.32f, 0.42f), EnemySwatch.CaveBelly, coarse: true);
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: s > 0 ? "OldKulArm_R" : "OldKulArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.72f, 0.36f, 0.22f, EnemySwatch.CaveSkinDark, EnemySwatch.CaveSkin, EnemySwatch.CaveMoss, "OldKul");
        }

        // ------------------------------------------------------------------
        // O Glutão de Brasa
        // ------------------------------------------------------------------

        public static void BuildEmberGlutton(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.42f, 0f), b =>
            {
                // It ate the forges' fire and is still hungry. It eats its own, too, when the forges are out; nobody ever said it was clever.
                b.Cylinder(new Vector3(0f, -0.72f, 0f), 0.92f, 0.56f, 1.34f, 9, EnemySwatch.GolemStoneDark, bottomCap: true);
                b.Blob(new Vector3(0f, 0.18f, 0f), new Vector3(1.42f, 1.14f, 1.3f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 7; i++)
                {
                    float a = i * 51f * Mathf.Deg2Rad;
                    b.Box(new Vector3(Mathf.Cos(a) * 0.58f, -0.32f + (i % 3) * 0.3f, Mathf.Sin(a) * 0.58f), new Vector3(0.05f, 0.42f, 0.05f), EnemySwatch.GolemCrack,
                        Quaternion.Euler(0f, -i * 51f, 10f * (i % 2 == 0 ? 1f : -1f)));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.1f, 0.46f), b => b.Blob(Vector3.zero, new Vector3(0.74f, 0.56f, 0.34f), EnemySwatch.WarEye));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 0.12f), b =>
            {
                // No eyes and no nose: all of the face went to the mouth.
                b.Blob(new Vector3(0f, 0.14f, 0f), new Vector3(0.96f, 0.5f, 0.92f), EnemySwatch.GolemStoneDark, coarse: true);
                b.Blob(new Vector3(0f, 0.34f, -0.08f), new Vector3(0.7f, 0.3f, 0.66f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    float x = (i - 2) * 0.17f;
                    b.Crystal(new Vector3(x, -0.08f, 0.38f), 0.06f, 0.22f, EnemySwatch.Bone, Quaternion.Euler(180f, 0f, 0f));
                }

                for (int s = -1; s <= 1; s += 2)
                {
                    b.Crystal(new Vector3(0.28f * s, 0.46f, -0.04f), 0.11f, 0.42f, EnemySwatch.GolemCrack, Quaternion.Euler(-8f, 0f, -12f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.16f), b =>
            {
                b.Blob(new Vector3(0f, -0.1f, 0.14f), new Vector3(0.92f, 0.26f, 0.72f), EnemySwatch.GolemStoneDark, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    float x = (i - 1.5f) * 0.2f;
                    b.Crystal(new Vector3(x, 0.02f, 0.3f), 0.06f, 0.2f, EnemySwatch.Bone);
                }
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.78f * side, 0.44f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.52f, 0.5f, 0.52f), EnemySwatch.GolemStoneDark, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.16f * side, -0.74f, 0.14f), 0.2f, 0.17f, 6, EnemySwatch.Black);
                    b.Blob(new Vector3(0.18f * side, -0.94f, 0.2f), new Vector3(0.56f, 0.56f, 0.56f), EnemySwatch.GolemStoneDark, coarse: true);
                    b.Box(new Vector3(0.18f * side, -0.94f, 0.47f), new Vector3(0.32f, 0.06f, 0.04f), EnemySwatch.GolemCrack);
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: s > 0 ? "EmberGluttonArm_R" : "EmberGluttonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.8f, 0.4f, 0.26f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, EnemySwatch.GolemStoneDark, "EmberGlutton");
        }

        // ------------------------------------------------------------------
        // Apep, a Serpente que Bebe o Rio
        // ------------------------------------------------------------------

        public static void BuildApep(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.33f, 0f), b =>
            {
                // Coiled up on the sandbank with the whole river inside her and her neck straight up, watching the barque come.
                for (int i = 0; i < 10; i++)
                {
                    float a = i * 36f * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.82f, 0f, Mathf.Sin(a) * 0.82f), new Vector3(0.82f, 0.66f, 0.82f), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.CaveSkinDark, coarse: true);
                }

                for (int i = 0; i < 7; i++)
                {
                    float a = (i * 51f + 20f) * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.48f, 0.4f, Mathf.Sin(a) * 0.48f), new Vector3(0.64f, 0.5f, 0.64f), i % 2 == 0 ? EnemySwatch.CaveSkinDark : EnemySwatch.Shroud, coarse: true);
                }

                b.Segment(new Vector3(0f, 0.1f, -0.05f), new Vector3(0f, 1.85f, 0.3f), 0.44f, 0.24f, 7, EnemySwatch.Shroud);
                for (int i = 0; i < 4; i++)
                {
                    float y = 0.7f + i * 0.3f;
                    b.Box(new Vector3(0f, y, 0.31f + i * 0.035f), new Vector3(0.34f - i * 0.04f, 0.1f, 0.05f), EnemySwatch.Gold);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.76f, 0.36f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.46f, 0.2f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.9f, 0.32f), b =>
            {
                b.Blob(new Vector3(0f, 0.08f, 0.12f), new Vector3(0.62f, 0.44f, 0.9f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(0f, 0.22f, -0.1f), new Vector3(0.4f, 0.26f, 0.5f), EnemySwatch.CaveSkinDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.17f, 0.14f, 0.34f, 0.08f, EnemySwatch.WarEye);
                b.Crystal(new Vector3(0f, 0.3f, -0.1f), 0.1f, 0.34f, EnemySwatch.Gold, Quaternion.Euler(-14f, 0f, 0f));
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.12f, 0.2f), b =>
            {
                b.Blob(new Vector3(0f, -0.02f, 0.14f), new Vector3(0.5f, 0.16f, 0.72f), EnemySwatch.CaveSkinDark, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Crystal(new Vector3(0.14f * s, 0.02f, 0.42f), 0.05f, 0.2f, EnemySwatch.Bone);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.98f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.1f, 0.06f, -0.42f), 0.26f, 0.08f, 6, EnemySwatch.Shroud);
                b.Crystal(new Vector3(0.1f, 0.06f, -0.42f), 0.08f, 0.24f, EnemySwatch.Rune, Quaternion.Euler(-90f, 0f, 0f));
            });
        }

        // ------------------------------------------------------------------
        // O Sonho da Destruição
        // ------------------------------------------------------------------

        public static void BuildDreamOfDestruction(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.36f, 0f), b =>
            {
                // The fear of two giants in a bad night, wearing the broken tablet they dreamed of.
                b.Blob(new Vector3(0f, 0f, -0.04f), new Vector3(1.14f, 1.42f, 1f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(0f, -0.5f, 0.08f), new Vector3(0.94f, 0.72f, 0.84f), EnemySwatch.Black, coarse: true);
                b.Blob(new Vector3(0f, 0.5f, -0.14f), new Vector3(0.9f, 0.6f, 0.8f), EnemySwatch.GolemStoneDark, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    float a = i * 72f * Mathf.Deg2Rad;
                    b.Box(new Vector3(Mathf.Cos(a) * 0.62f, 0.12f + (i % 3) * 0.24f, Mathf.Sin(a) * 0.5f), new Vector3(0.22f, 0.3f, 0.05f), EnemySwatch.Bone, Quaternion.Euler(8f, i * 72f, 10f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.04f, 0.5f), b =>
            {
                b.Box(Vector3.zero, new Vector3(0.48f, 0.62f, 0.09f), EnemySwatch.Crystal);
                b.Box(new Vector3(0.08f, 0.1f, 0.05f), new Vector3(0.05f, 0.34f, 0.03f), EnemySwatch.Rune, Quaternion.Euler(0f, 0f, -14f));
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.74f, 0.14f), b =>
            {
                b.Blob(new Vector3(0f, 0.16f, 0f), new Vector3(0.74f, 0.74f, 0.76f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(0f, 0.08f, 0.3f), new Vector3(0.44f, 0.5f, 0.2f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.12f, 0.4f, 0.06f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.2f), b => b.Blob(new Vector3(0f, -0.04f, 0.06f), new Vector3(0.36f, 0.16f, 0.26f), EnemySwatch.GolemStoneDark, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.66f * side, 0.46f, 0.02f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.4f, 0.4f, 0.4f), EnemySwatch.Shroud, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.16f * side, -0.82f, 0.14f), 0.16f, 0.12f, 6, EnemySwatch.Black);
                    b.Box(new Vector3(0.18f * side, -1.04f, 0.2f), new Vector3(0.44f, 0.52f, 0.1f), EnemySwatch.Bone, Quaternion.Euler(0f, 0f, 8f * side));
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: s > 0 ? "DreamArm_R" : "DreamArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.74f, 0.32f, 0.19f, EnemySwatch.Black, EnemySwatch.Shroud, EnemySwatch.GolemStoneDark, "DreamOfDestruction");
        }

        // ------------------------------------------------------------------
        // A Grande Treva
        // ------------------------------------------------------------------

        public static void BuildGreatDark(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.6f, 0f), b =>
            {
                // Only ever seen in outline: the fire behind it is what you actually look at.
                b.Blob(Vector3.zero, new Vector3(1.2f, 1.34f, 1.14f), EnemySwatch.Black, coarse: true);
                b.Blob(new Vector3(0f, 0.5f, -0.1f), new Vector3(1.04f, 0.7f, 0.94f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(0f, -0.48f, 0.1f), new Vector3(0.92f, 0.62f, 0.84f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    b.Crystal(new Vector3(0f, 0.5f - i * 0.28f, -0.56f), 0.1f, 0.34f, EnemySwatch.Black, Quaternion.Euler(-100f, 0f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.08f, 0.5f), b => b.Blob(Vector3.zero, new Vector3(0.46f, 0.6f, 0.22f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.84f, 0.26f), b =>
            {
                // The lion's head.
                b.Blob(new Vector3(0f, 0.14f, -0.1f), new Vector3(1.04f, 0.84f, 0.74f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(0f, 0.08f, 0.18f), new Vector3(0.62f, 0.56f, 0.56f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.17f, 0.16f, 0.42f, 0.09f, EnemySwatch.WarEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.26f), b =>
            {
                b.Blob(new Vector3(0f, -0.1f, 0.1f), new Vector3(0.46f, 0.2f, 0.42f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Crystal(new Vector3(0.13f * s, -0.02f, 0.26f), 0.05f, 0.18f, EnemySwatch.Rune);
                }
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.WingRight : ProceduralEnemyParts.WingLeft, body, new Vector3(0.42f * side, 0.3f, -0.1f), b =>
                {
                    // The eagle's wings.
                    b.Segment(Vector3.zero, new Vector3(0.62f * side, 0.2f, -0.08f), 0.16f, 0.1f, 6, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.62f * side, 0.2f, -0.08f), new Vector3(1.2f * side, -0.05f, -0.16f), 0.1f, 0.05f, 5, EnemySwatch.Black);
                    for (int i = 0; i < 6; i++)
                    {
                        float t = i / 5f;
                        Vector3 root = new Vector3(Mathf.Lerp(0.12f, 1.1f, t) * side, Mathf.Lerp(0.08f, 0f, t), -0.1f);
                        b.Box(root + new Vector3(0f, -0.34f, -0.03f), new Vector3(0.22f, 0.72f - 0.06f * i, 0.04f), i % 2 == 0 ? EnemySwatch.Black : EnemySwatch.Shroud);
                    }
                }, meshKey: s > 0 ? "GreatDarkWing_R" : "GreatDarkWing_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.5f, -0.5f), b =>
            {
                // The fish's tail, the wrong way up and proud of it.
                b.Segment(Vector3.zero, new Vector3(0f, -0.3f, -0.56f), 0.22f, 0.08f, 6, EnemySwatch.Black);
                b.Box(new Vector3(0f, -0.3f, -0.8f), new Vector3(0.72f, 0.5f, 0.06f), EnemySwatch.Shroud, Quaternion.Euler(0f, 0f, 45f));
            });

            // The demon's feet (the legs); a dragon's body is what's between all the rest.
            HinokuraBeastRecipes.AddBipedLegs(bp, 1.02f, 0.34f, 0.2f, EnemySwatch.Black, EnemySwatch.Shroud, EnemySwatch.GolemCrack, "GreatDark");
        }

        // ------------------------------------------------------------------
        // Āz, a Capitã Cobiçosa, e a Última Arca
        // ------------------------------------------------------------------

        public static void BuildAzCaptain(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.32f, 0f), b =>
            {
                // A sea captain's long coat with all the buttons on the wrong side: every one of them is someone else's.
                b.Blob(Vector3.zero, new Vector3(1.12f, 1.38f, 0.96f), EnemySwatch.WarCape, coarse: true);
                b.Blob(new Vector3(0f, -0.56f, -0.04f), new Vector3(1.2f, 0.62f, 1.02f), EnemySwatch.WarCape, coarse: true);
                b.Blob(new Vector3(0f, 0.5f, -0.04f), new Vector3(0.92f, 0.52f, 0.76f), EnemySwatch.GolemStoneDark, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    b.Box(new Vector3(-0.2f, 0.32f - i * 0.2f, 0.48f - i * 0.02f), new Vector3(0.08f, 0.08f, 0.04f), EnemySwatch.Gold);
                }

                for (int i = 0; i < 4; i++)
                {
                    b.Blob(new Vector3(0.22f + i * 0.04f, 0.2f - i * 0.26f, 0.46f), new Vector3(0.12f, 0.14f, 0.08f), EnemySwatch.Crystal);
                }
            });
            // The Última Arca, held out in front: a little gold chest of stolen light.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.1f, 0.74f), b =>
            {
                b.Box(new Vector3(0f, 0.2f, 0f), new Vector3(0.6f, 0.08f, 0.4f), EnemySwatch.Gold);
                b.Box(Vector3.zero, new Vector3(0.56f, 0.36f, 0.36f), EnemySwatch.Copper);
                b.Blob(new Vector3(0f, 0.3f, 0f), new Vector3(0.4f, 0.26f, 0.3f), EnemySwatch.StolenLight, coarse: true);
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.78f, 0.1f), b =>
            {
                b.Blob(new Vector3(0f, 0.14f, 0f), new Vector3(0.56f, 0.6f, 0.58f), EnemySwatch.CaveBelly);
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.2f, 0.26f, 0.06f, EnemySwatch.BatEye);
                // The tricorn, gold lace first.
                b.Blob(new Vector3(0f, 0.44f, 0f), new Vector3(0.9f, 0.16f, 0.74f), EnemySwatch.WarCape, coarse: true);
                b.Blob(new Vector3(0f, 0.56f, 0f), new Vector3(0.5f, 0.28f, 0.46f), EnemySwatch.WarCape, coarse: true);
                b.Box(new Vector3(0f, 0.46f, 0.32f), new Vector3(0.76f, 0.05f, 0.04f), EnemySwatch.Gold);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.2f), b => b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.34f, 0.14f, 0.26f), EnemySwatch.CaveBelly));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.64f * side, 0.42f, 0.04f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.38f, 0.38f, 0.38f), EnemySwatch.Gold, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.14f * side, -0.7f, 0.22f), 0.15f, 0.12f, 6, EnemySwatch.WarCape);
                    b.Blob(new Vector3(0.14f * side, -0.84f, 0.26f), new Vector3(0.28f, 0.28f, 0.28f), EnemySwatch.CaveBelly);
                }, Quaternion.Euler(-30f, 0f, 0f), meshKey: s > 0 ? "AzCaptainArm_R" : "AzCaptainArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.72f, 0.3f, 0.2f, EnemySwatch.WarCape, EnemySwatch.GolemStoneDark, EnemySwatch.Gold, "AzCaptain");
        }

        // ------------------------------------------------------------------
        // Hedammu, a Serpente Gulosa do Mar (30)
        // ------------------------------------------------------------------

        public static void BuildHedammu(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.33f, 0f), b =>
            {
                // Coiled in the Largo with her mouth open: she eats everything and is never full, and the sparks she swallowed show through.
                for (int i = 0; i < 10; i++)
                {
                    float a = i * 36f * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.86f, 0f, Mathf.Sin(a) * 0.86f), new Vector3(0.84f, 0.66f, 0.84f), i % 2 == 0 ? EnemySwatch.CaveMoss : EnemySwatch.Verdigris, coarse: true);
                }

                for (int i = 0; i < 7; i++)
                {
                    float a = (i * 51f + 20f) * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.5f, 0.4f, Mathf.Sin(a) * 0.5f), new Vector3(0.66f, 0.5f, 0.66f), i % 2 == 0 ? EnemySwatch.Verdigris : EnemySwatch.CaveSkinDark, coarse: true);
                }

                // The swallowed sparks, still lit inside the coils.
                for (int i = 0; i < 5; i++)
                {
                    float a = (i * 72f + 10f) * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.98f, 0.12f, Mathf.Sin(a) * 0.98f), new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.StolenLight, coarse: true);
                }

                b.Segment(new Vector3(0f, 0.1f, -0.05f), new Vector3(0f, 1.7f, 0.46f), 0.46f, 0.26f, 7, EnemySwatch.CaveMoss);
                for (int i = 0; i < 4; i++)
                {
                    float t = i / 3f;
                    b.Crystal(new Vector3(0f, Mathf.Lerp(0.6f, 1.6f, t), Mathf.Lerp(-0.12f, 0.2f, t)), 0.08f, 0.3f, EnemySwatch.Bone, Quaternion.Euler(-60f, 0f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.72f, 0.38f), b => b.Blob(Vector3.zero, new Vector3(0.36f, 0.46f, 0.2f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.75f, 0.5f), b =>
            {
                b.Blob(new Vector3(0f, 0.1f, 0.12f), new Vector3(0.7f, 0.46f, 1f), EnemySwatch.CaveMoss, coarse: true);
                b.Blob(new Vector3(0f, 0.24f, -0.1f), new Vector3(0.44f, 0.26f, 0.5f), EnemySwatch.Verdigris, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.19f, 0.18f, 0.34f, 0.09f, EnemySwatch.Crystal);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Crystal(new Vector3(0.16f * s, 0.28f, -0.12f), 0.09f, 0.45f, EnemySwatch.Bone, Quaternion.Euler(-24f, 0f, -14f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.2f), b =>
            {
                // The mouth that is never full.
                b.Blob(new Vector3(0f, -0.04f, 0.16f), new Vector3(0.6f, 0.18f, 0.8f), EnemySwatch.CaveSkinDark, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Crystal(new Vector3((i - 1.5f) * 0.12f, 0.02f, 0.44f), 0.04f, 0.16f, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -1.02f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.08f, 0.06f, -0.4f), 0.26f, 0.08f, 6, EnemySwatch.Verdigris);
                b.Box(new Vector3(0.08f, 0.06f, -0.48f), new Vector3(0.5f, 0.05f, 0.26f), EnemySwatch.CaveMoss);
            });
        }

        // ------------------------------------------------------------------
        // O Lodo-que-Engole (35)
        // ------------------------------------------------------------------

        public static void BuildSwallowingMire(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.3f, 0f), b =>
            {
                // The Dark as mud with no face. It wears the shape of something standing only because the heroes do.
                b.Blob(new Vector3(0f, 0.02f, -0.05f), new Vector3(1.36f, 1.4f, 1.16f), EnemySwatch.Shadow, coarse: true);
                b.Blob(new Vector3(0f, -0.32f, 0.12f), new Vector3(1.1f, 0.8f, 0.94f), EnemySwatch.CaveSkinDark, coarse: true);
                b.Blob(new Vector3(0f, 0.54f, -0.2f), new Vector3(1f, 0.7f, 0.82f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    b.Cylinder(new Vector3(Mathf.Cos(a) * 0.5f, -0.62f, Mathf.Sin(a) * 0.42f - 0.05f), 0.1f, 0.04f, 0.5f, 5, EnemySwatch.Black, bottomCap: true);
                }

                // Smoke bubbles coming up through the mud.
                for (int i = 0; i < 7; i++)
                {
                    float a = (i * 51f + 15f) * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.56f, -0.1f + (i % 3) * 0.26f, Mathf.Sin(a) * 0.46f), new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.Sorcery, coarse: true);
                }
            });
            // The Living Water it swallowed, the only clean thing in it.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.08f, 0.5f), b => b.Blob(Vector3.zero, new Vector3(0.44f, 0.5f, 0.2f), EnemySwatch.Crystal, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.74f, 0.26f), b =>
            {
                b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.76f, 0.62f, 0.68f), EnemySwatch.Shadow, coarse: true);
                b.Blob(new Vector3(0f, 0.36f, -0.08f), new Vector3(0.86f, 0.46f, 0.76f), EnemySwatch.Black, coarse: true);
                b.Blob(new Vector3(0.12f, 0.6f, -0.1f), new Vector3(0.16f, 0.16f, 0.16f), EnemySwatch.Sorcery, coarse: true);
                b.Blob(new Vector3(-0.18f, 0.52f, 0.04f), new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.Sorcery, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.2f), b =>
            {
                // No face, only the place where things go in.
                b.Blob(new Vector3(0f, -0.06f, 0.1f), new Vector3(0.56f, 0.22f, 0.4f), EnemySwatch.CaveSkinDark, coarse: true);
                b.Box(new Vector3(0f, 0.04f, 0.24f), new Vector3(0.34f, 0.05f, 0.04f), EnemySwatch.Sorcery);
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.68f * side, 0.4f, 0.05f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.42f, 0.38f, 0.4f), EnemySwatch.Shadow, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.2f * side, -0.72f, 0.16f), 0.18f, 0.14f, 6, EnemySwatch.CaveSkinDark);
                    b.Segment(new Vector3(0.2f * side, -0.72f, 0.16f), new Vector3(0.24f * side, -1.14f, 0.3f), 0.14f, 0.1f, 6, EnemySwatch.Black);
                    b.Blob(new Vector3(0.24f * side, -1.22f, 0.32f), new Vector3(0.4f, 0.3f, 0.42f), EnemySwatch.Shadow, coarse: true);
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: s > 0 ? "SwallowingMireArm_R" : "SwallowingMireArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.72f, 0.36f, 0.22f, EnemySwatch.Black, EnemySwatch.CaveSkinDark, EnemySwatch.Shadow, "SwallowingMire");
        }

        // ------------------------------------------------------------------
        // Aydahar, o Dragão do Passo (40)
        // ------------------------------------------------------------------

        public static void BuildAydahar(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.6f, 0f), b =>
            {
                // A snow-pale dragon coiled on the pass, guarding Āz's gold without ever asking why. The scales went gold from lying on it.
                b.Blob(Vector3.zero, new Vector3(1.1f, 1.2f, 1.3f), EnemySwatch.Steel, coarse: true);
                b.Blob(new Vector3(0f, -0.42f, 0.12f), new Vector3(0.86f, 0.6f, 0.9f), EnemySwatch.GolemSnow, coarse: true);
                b.Blob(new Vector3(0f, 0.46f, -0.12f), new Vector3(0.92f, 0.6f, 0.9f), EnemySwatch.Iron, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    b.Crystal(new Vector3(0f, 0.6f - i * 0.26f, -0.5f), 0.09f, 0.3f, EnemySwatch.Horn, Quaternion.Euler(-100f, 0f, 0f));
                    b.Box(new Vector3(0f, 0.3f - i * 0.18f, 0.52f), new Vector3(0.4f - i * 0.04f, 0.08f, 0.05f), EnemySwatch.Gold);
                }
            });
            // The fire it breathes, banked in its chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.02f, 0.54f), b => b.Blob(Vector3.zero, new Vector3(0.36f, 0.5f, 0.2f), EnemySwatch.Ember, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.84f, 0.34f), b =>
            {
                b.Blob(new Vector3(0f, 0.12f, 0.16f), new Vector3(0.6f, 0.5f, 0.86f), EnemySwatch.Steel, coarse: true);
                b.Blob(new Vector3(0f, 0.26f, -0.08f), new Vector3(0.5f, 0.3f, 0.5f), EnemySwatch.Iron, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.18f, 0.2f, 0.36f, 0.08f, EnemySwatch.Crystal);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.18f * s, 0.34f, -0.12f), 0.07f, 0.44f, 4, -14f, EnemySwatch.Horn, Quaternion.Euler(-40f, 0f, -16f * s));
                    // The long whiskers of the steppe dragons.
                    b.Segment(new Vector3(0.12f * s, 0.06f, 0.52f), new Vector3(0.34f * s, -0.12f, 0.56f), 0.025f, 0.012f, 4, EnemySwatch.Gold);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.36f), b =>
            {
                b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.44f, 0.16f, 0.58f), EnemySwatch.GolemSnow, coarse: true);
                b.Box(new Vector3(0f, 0.02f, 0.12f), new Vector3(0.18f, 0.04f, 0.34f), EnemySwatch.Ember);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Crystal(new Vector3(0.12f * s, 0.02f, 0.28f), 0.04f, 0.14f, EnemySwatch.Fang);
                }
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.WingRight : ProceduralEnemyParts.WingLeft, body, new Vector3(0.42f * side, 0.3f, -0.12f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.56f * side, 0.24f, -0.08f), 0.14f, 0.09f, 6, EnemySwatch.Steel);
                    b.Segment(new Vector3(0.56f * side, 0.24f, -0.08f), new Vector3(1.06f * side, 0f, -0.18f), 0.09f, 0.05f, 5, EnemySwatch.Iron);
                    for (int i = 0; i < 5; i++)
                    {
                        float t = i / 4f;
                        Vector3 root = new Vector3(Mathf.Lerp(0.14f, 0.98f, t) * side, Mathf.Lerp(0.1f, 0f, t), -0.12f);
                        b.Box(root + new Vector3(0f, -0.3f, -0.03f), new Vector3(0.22f, 0.64f - 0.06f * i, 0.04f), i % 2 == 0 ? EnemySwatch.GolemSnow : EnemySwatch.Steel);
                    }
                }, meshKey: s > 0 ? "AydaharWing_R" : "AydaharWing_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.46f, -0.5f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.36f, -0.56f), 0.26f, 0.16f, 6, EnemySwatch.Steel);
                b.Segment(new Vector3(0f, -0.36f, -0.56f), new Vector3(0f, -0.62f, -1.06f), 0.16f, 0.06f, 6, EnemySwatch.GolemSnow);
                b.Crystal(new Vector3(0f, -0.62f, -1.06f), 0.08f, 0.24f, EnemySwatch.Gold, Quaternion.Euler(-110f, 0f, 0f));
            });

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.02f, 0.34f, 0.2f, EnemySwatch.Steel, EnemySwatch.GolemSnow, EnemySwatch.Horn, "Aydahar");
        }

        // ------------------------------------------------------------------
        // Os Quatro Gulosos (46)
        // ------------------------------------------------------------------

        public static void BuildFourGluttons(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.42f, 0f), b =>
            {
                // An old bronze vessel that got up and walked: four appetites of Āz left on their own, one face on each side.
                b.Cylinder(new Vector3(0f, -0.72f, 0f), 0.9f, 0.6f, 1.3f, 9, EnemySwatch.Verdigris, bottomCap: true);
                b.Blob(new Vector3(0f, 0.18f, 0f), new Vector3(1.4f, 1.1f, 1.3f), EnemySwatch.CopperDark, coarse: true);
                b.Box(new Vector3(0f, -0.1f, 0f), new Vector3(1.5f, 0.08f, 0.08f), EnemySwatch.Gold);
                // Taotie, in front: the brows and the curled snout of the vessels' mask.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.24f * s, 0.42f, 0.56f), new Vector3(0.3f, 0.08f, 0.06f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, 14f * s));
                    b.Blob(new Vector3(0.24f * s, 0.3f, 0.58f), new Vector3(0.12f, 0.1f, 0.06f), EnemySwatch.SoulFire, coarse: true);
                }

                // Hundun, on the left: no face at all, only a smooth dark swelling.
                b.Blob(new Vector3(-0.56f, 0.22f, 0f), new Vector3(0.36f, 0.5f, 0.6f), EnemySwatch.Shadow, coarse: true);
                // Qiongqi, on the right: the tiger's stripes and a folded wing.
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0.6f, 0.04f + i * 0.2f, 0f), new Vector3(0.05f, 0.06f, 0.42f), EnemySwatch.Black);
                }

                b.Box(new Vector3(0.62f, 0.3f, -0.26f), new Vector3(0.05f, 0.42f, 0.3f), EnemySwatch.Copper, Quaternion.Euler(20f, 0f, 0f));
                // Taowu, behind: a stubborn brow that never turns round.
                b.Box(new Vector3(0f, 0.42f, -0.6f), new Vector3(0.6f, 0.12f, 0.06f), EnemySwatch.Gold);
                b.Blob(new Vector3(0f, 0.24f, -0.6f), new Vector3(0.22f, 0.16f, 0.08f), EnemySwatch.Bone, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.08f, 0.46f), b => b.Blob(Vector3.zero, new Vector3(0.6f, 0.4f, 0.3f), EnemySwatch.SoulFire, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.7f, 0.1f), b =>
            {
                // The vessel's lid, with its two handles and Taotie's horns.
                b.Blob(new Vector3(0f, 0.14f, 0f), new Vector3(0.94f, 0.46f, 0.9f), EnemySwatch.Verdigris, coarse: true);
                b.Cylinder(new Vector3(0f, 0.3f, 0f), 0.3f, 0.2f, 0.16f, 8, EnemySwatch.CopperDark, bottomCap: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.36f * s, 0.46f, 0f), new Vector3(0.06f, 0.3f, 0.26f), EnemySwatch.Gold);
                    b.Horn(new Vector3(0.22f * s, 0.32f, 0.22f), 0.07f, 0.36f, 4, -18f, EnemySwatch.Horn, Quaternion.Euler(-30f, 0f, -30f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.16f), b =>
            {
                b.Blob(new Vector3(0f, -0.1f, 0.14f), new Vector3(0.86f, 0.24f, 0.68f), EnemySwatch.CopperDark, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Crystal(new Vector3((i - 1.5f) * 0.18f, 0.02f, 0.3f), 0.05f, 0.18f, EnemySwatch.Fang);
                }
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.76f * side, 0.44f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.48f, 0.46f, 0.48f), EnemySwatch.Verdigris, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.16f * side, -0.72f, 0.14f), 0.18f, 0.15f, 6, EnemySwatch.CopperDark);
                    b.Blob(new Vector3(0.18f * side, -0.9f, 0.2f), new Vector3(0.5f, 0.5f, 0.5f), EnemySwatch.Verdigris, coarse: true);
                    b.Box(new Vector3(0.18f * side, -0.9f, 0.44f), new Vector3(0.28f, 0.05f, 0.04f), EnemySwatch.Gold);
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: s > 0 ? "FourGluttonsArm_R" : "FourGluttonsArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.8f, 0.4f, 0.24f, EnemySwatch.CopperDark, EnemySwatch.Verdigris, EnemySwatch.Gold, "FourGluttons");
        }

        // ------------------------------------------------------------------
        // Ohya, o Gigante Irmão (52)
        // ------------------------------------------------------------------

        public static void BuildOhya(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.5f, 0f), b =>
            {
                // Hahya's brother, chained to the mountain "so as not to fight anyone", and fighting anyway.
                b.Blob(new Vector3(0f, 0.02f, -0.04f), new Vector3(1.3f, 1.44f, 1f), EnemySwatch.Leather, coarse: true);
                b.Blob(new Vector3(0f, 0.5f, -0.06f), new Vector3(1.2f, 0.64f, 0.9f), EnemySwatch.SandstoneDark, coarse: true);
                b.Blob(new Vector3(0f, -0.5f, 0.04f), new Vector3(1.02f, 0.56f, 0.86f), EnemySwatch.GolemStoneDark, coarse: true);
                // The chain across his chest, from the left shoulder to the right hip.
                for (int i = 0; i < 6; i++)
                {
                    float t = i / 5f;
                    b.Box(new Vector3(Mathf.Lerp(-0.42f, 0.42f, t), Mathf.Lerp(0.44f, -0.4f, t), 0.46f), new Vector3(0.14f, 0.1f, 0.06f), i % 2 == 0 ? EnemySwatch.IronDark : EnemySwatch.Iron,
                        Quaternion.Euler(0f, 0f, 45f + (i % 2) * 45f));
                }
            });
            // The one link where the light has already come up: it glows.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.02f, 0.5f), b =>
            {
                b.Box(Vector3.zero, new Vector3(0.3f, 0.2f, 0.08f), EnemySwatch.StolenLight);
                b.Box(new Vector3(0f, 0f, 0.03f), new Vector3(0.14f, 0.08f, 0.04f), EnemySwatch.IronDark);
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.82f, 0.1f), b =>
            {
                b.Blob(new Vector3(0f, 0.16f, 0f), new Vector3(0.62f, 0.66f, 0.62f), EnemySwatch.SandstoneDark, coarse: true);
                b.Blob(new Vector3(0f, 0.4f, -0.06f), new Vector3(0.66f, 0.3f, 0.6f), EnemySwatch.Steel, coarse: true);
                b.Box(new Vector3(0f, 0.26f, 0.28f), new Vector3(0.42f, 0.07f, 0.06f), EnemySwatch.GolemStoneDark);
                // Not wicked eyes: the eyes of someone who hasn't come back to himself yet.
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.18f, 0.28f, 0.07f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.16f), b => b.Blob(new Vector3(0f, -0.12f, 0.08f), new Vector3(0.5f, 0.36f, 0.3f), EnemySwatch.Steel, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.7f * side, 0.42f, 0.04f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.44f, 0.42f, 0.42f), EnemySwatch.Leather, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.2f * side, -0.78f, 0.12f), 0.18f, 0.15f, 6, EnemySwatch.SandstoneDark);
                    b.Segment(new Vector3(0.2f * side, -0.78f, 0.12f), new Vector3(0.24f * side, -1.2f, 0.24f), 0.15f, 0.12f, 6, EnemySwatch.SandstoneDark);
                    b.Cylinder(new Vector3(0.23f * side, -1.12f, 0.21f), 0.17f, 0.17f, 0.12f, 7, EnemySwatch.IronDark, bottomCap: true);
                    b.Blob(new Vector3(0.25f * side, -1.3f, 0.26f), new Vector3(0.4f, 0.36f, 0.4f), EnemySwatch.SandstoneDark, coarse: true);
                    if (right)
                    {
                        // The loose end of the chain he never quite lets go of.
                        for (int i = 0; i < 3; i++)
                        {
                            b.Box(new Vector3(0.3f * side, -1.5f - i * 0.12f, 0.24f), new Vector3(0.08f, 0.12f, 0.04f), i % 2 == 0 ? EnemySwatch.IronDark : EnemySwatch.Iron,
                                Quaternion.Euler(0f, i % 2 == 0 ? 0f : 90f, 0f));
                        }
                    }
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: right ? "OhyaArm_R" : "OhyaArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.86f, 0.36f, 0.22f, EnemySwatch.Leather, EnemySwatch.SandstoneDark, EnemySwatch.GolemStoneDark, "Ohya");
        }
    }
}
