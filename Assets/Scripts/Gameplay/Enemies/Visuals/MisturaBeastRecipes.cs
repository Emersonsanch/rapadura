using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Margens da Mistura (Docs/Mapas100.md 77–82) that the continuous world played with stand-ins, low-poly from the
    /// shared palette like <see cref="RioBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Leão de Tijolo Apagado</b> (77, ~1.3 m, 2 m long): baked-brick lion from the walls, courses of blue glaze along the flank.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four heavy legs.</item>
    /// <item><b>Mušhuššu Jovem</b> (77, ~1.5 m): scaled dragon-snake of the gates, one curled horn, a sting at the end of the tail.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Gafanhoto de Betume</b> (77, ~0.8 m): locust dipped in pitch with embers in its seams.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Ampulheta Viva</b> (78, ~2 m): a walking hourglass of stone with lit sand in its bulbs.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Sombra Atrasada</b> (78, ~1.5 m): the shadow that arrives after the beast, ragged sleeves and a long trail.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Betume Vivo</b> (79, ~0.8 m): heap of bitumen with embers caught inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Garça de Piche</b> (79, ~1.2 m): black heron on long legs that spits glue. <c>Body</c> → <c>Head</c> → <c>Jaw</c>,
    /// <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Cedro Raivoso</b> (80, ~3 m): cedar walking on its roots, resin down the bark, a crown of three tiers.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Manto de Brilho</b> (80, ~1.3 m): one of the guardian's seven cloaks, empty and hovering. <c>Body</c>,
    /// <c>Wing_L</c>/<c>Wing_R</c> (sleeves), <c>Tail</c> (the hem).</item>
    /// <item><b>Filho de Ullikummi</b> (81, ~2.4 m): diorite that grew legs, crystals coming out of the shoulders.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Tábua Quebrada</b> (82, ~1.4 m): standing slab with a broken corner and lit cuneiform. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Jardim-Sombra</b> (82, ~1.9 m): a cut tree of the dream, sawn flat at the top, rings shining.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// </list>
    /// </summary>
    public static class MisturaBeastRecipes
    {
        // ------------------------------------------------------------------
        // Leão de Tijolo Apagado
        // ------------------------------------------------------------------

        public static void BuildBrickLion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.82f, 0f), DrawBrickLionBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.72f), DrawBrickLionHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.18f), b =>
            {
                b.Box(new Vector3(0f, -0.03f, 0.12f), new Vector3(0.34f, 0.08f, 0.26f), EnemySwatch.SandstoneDark);
                b.Box(new Vector3(0f, 0.015f, 0.12f), new Vector3(0.24f, 0.012f, 0.2f), EnemySwatch.Pink);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.12f * s, 0.01f, 0.2f), 0.02f, 0f, 0.07f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.16f, -0.76f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.1f, -0.44f), 0.07f, 0.045f, 5, EnemySwatch.SandstoneDark);
                b.Blob(new Vector3(0f, 0.12f, -0.5f), new Vector3(0.16f, 0.16f, 0.2f), EnemySwatch.CaveSkin, coarse: true);
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.7f, 0.32f, 0.48f, -0.48f, 0.11f, EnemySwatch.SandstoneDark, EnemySwatch.CaveSkinDark, "BrickLion");
        }

        private static void DrawBrickLionBody(EnemyMeshBuilder b)
        {
            // It was glazed to guard the street of the processions; the shadow put out the blue from the inside, course by course.
            b.Blob(Vector3.zero, new Vector3(0.92f, 0.74f, 1.6f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, 0.16f, 0.5f), new Vector3(0.9f, 0.7f, 0.74f), EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.18f, 0f), new Vector3(0.7f, 0.3f, 1.3f), EnemySwatch.Sandstone, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int row = 0; row < 3; row++)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float z = -0.5f + i * 0.24f + (row % 2) * 0.12f;
                        EnemySwatch glaze = (row + i) % 2 == 0 ? EnemySwatch.CaveSkin : EnemySwatch.CaveSkinDark;
                        b.Box(new Vector3(0.44f * s, 0.14f - row * 0.15f, z), new Vector3(0.05f, 0.12f, 0.2f), glaze);
                    }
                }

                b.Box(new Vector3(0.46f * s, 0.24f, -0.1f), new Vector3(0.03f, 0.03f, 1f), EnemySwatch.Gold);
            }
        }

        private static void DrawBrickLionHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.04f, 0.04f), new Vector3(0.5f, 0.46f, 0.44f), EnemySwatch.Sandstone);
            b.Box(new Vector3(0f, -0.06f, 0.26f), new Vector3(0.34f, 0.14f, 0.24f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, 0.16f, -0.12f), new Vector3(0.68f, 0.6f, 0.34f), EnemySwatch.CaveSkin, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.12f, 0.2f, 0.07f, EnemySwatch.MoonEye);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.12f * s, -0.12f, 0.32f), 0.022f, 0f, 0.08f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Mušhuššu Jovem
        // ------------------------------------------------------------------

        public static void BuildYoungMushussu(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawMushussuBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.74f), DrawMushussuHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.13f), new Vector3(0.17f, 0.04f, 0.28f), EnemySwatch.Verdigris);
                b.Box(new Vector3(0f, 0.015f, 0.13f), new Vector3(0.12f, 0.01f, 0.22f), EnemySwatch.Pink);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.66f), DrawMushussuTail);
            HinokuraBeastRecipes.AddFourLegs(bp, 0.6f, 0.2f, 0.4f, -0.44f, 0.075f, EnemySwatch.Verdigris, EnemySwatch.Gold, "YoungMushussu");
        }

        private static void DrawMushussuBody(EnemyMeshBuilder b)
        {
            // The dragon-snake of the blue gates, young enough to bite first and remember the procession afterwards.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.54f, 1.42f), EnemySwatch.Verdigris);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.48f, 0.3f, 1.24f), EnemySwatch.Copper, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.56f), new Vector3(0f, 0.42f, 0.74f), 0.17f, 0.11f, 6, EnemySwatch.Verdigris);
            for (int i = 0; i < 7; i++)
            {
                float z = -0.54f + i * 0.17f;
                b.Box(new Vector3(0f, 0.26f, z), new Vector3(0.08f, 0.06f, 0.1f), EnemySwatch.Gold, Quaternion.Euler(0f, 45f, 0f));
            }
        }

        private static void DrawMushussuHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.24f, 0.2f, 0.28f), EnemySwatch.Verdigris);
            b.Box(new Vector3(0f, 0f, 0.2f), new Vector3(0.17f, 0.08f, 0.26f), EnemySwatch.Verdigris);
            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.07f, 0.1f, 0.04f, EnemySwatch.MoonEye);
            b.Horn(new Vector3(0f, 0.1f, -0.04f), 0.04f, 0.3f, 4, 26f, EnemySwatch.Gold, Quaternion.Euler(-30f, 0f, 0f), 5, Vector3.right);
        }

        private static void DrawMushussuTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.46f), 0.14f, 0.09f, 6, EnemySwatch.Verdigris);
            b.Segment(new Vector3(0f, 0f, -0.46f), new Vector3(0f, 0.18f, -0.78f), 0.09f, 0.03f, 5, EnemySwatch.Copper);
            b.Cylinder(new Vector3(0f, 0.18f, -0.78f), 0.03f, 0f, 0.14f, 4, EnemySwatch.Ember, rotation: Quaternion.Euler(-60f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Gafanhoto de Betume
        // ------------------------------------------------------------------

        public static void BuildBitumenLocust(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawBitumenLocustBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.06f, 0.34f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, 0.05f, -0.02f), new Vector3(0.16f, 0.1f, 0.16f), EnemySwatch.Shadow, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.03f, 0.08f, 0.05f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.12f, 0.03f, 0.08f), EnemySwatch.IronDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.08f, 0.1f, 0.04f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Shadow, EnemySwatch.Black, EnemySwatch.Ember, 0.6f), Quaternion.Euler(0f, 0f, -10f), meshKey: "BitumenLocustWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.08f, 0.1f, 0.04f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Shadow, EnemySwatch.Black, EnemySwatch.Ember, 0.6f), Quaternion.Euler(0f, 0f, 10f), meshKey: "BitumenLocustWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.3f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.32f), 0.09f, 0.03f, 5, EnemySwatch.Black);
                b.Blob(new Vector3(0f, -0.08f, -0.36f), new Vector3(0.06f, 0.1f, 0.06f), EnemySwatch.Shadow, coarse: true);
            });
        }

        private static void DrawBitumenLocustBody(EnemyMeshBuilder b)
        {
            // It fell in the pitch once and liked what stuck to it; the embers in the seams are the last thing it ate.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.26f, 0.66f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.22f, 0.12f, 0.56f), EnemySwatch.Shadow, coarse: true);
            b.Box(new Vector3(0f, 0.13f, 0.04f), new Vector3(0.03f, 0.02f, 0.42f), EnemySwatch.Ember);
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 knee = new Vector3(0.26f * s, 0.2f, -0.2f);
                Vector3 foot = new Vector3(0.24f * s, -0.4f, -0.3f);
                b.Segment(new Vector3(0.1f * s, -0.02f, -0.1f), knee, 0.035f, 0.02f, 4, EnemySwatch.Black);
                b.Segment(knee, foot, 0.02f, 0.008f, 4, EnemySwatch.Shadow);
                b.Segment(new Vector3(0.09f * s, -0.08f, 0.12f), new Vector3(0.2f * s, -0.4f, 0.16f), 0.018f, 0.008f, 4, EnemySwatch.Shadow);
                b.Blob(new Vector3(0.12f * s, -0.14f, -0.06f), new Vector3(0.05f, 0.09f, 0.05f), EnemySwatch.Black, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Ampulheta Viva
        // ------------------------------------------------------------------

        public static void BuildLivingHourglass(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.05f, 0f), DrawHourglassBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.68f, 0f), b =>
            {
                b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.36f, 0.26f, 0.32f), EnemySwatch.Sandstone);
                b.Box(new Vector3(0f, 0.25f, 0f), new Vector3(0.3f, 0.05f, 0.3f), EnemySwatch.SandstoneDark);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.13f, 0.12f, 0.05f, EnemySwatch.MoonEye);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.48f * side, 0.46f, 0f), b => DrawHourglassArm(b, side),
                    Quaternion.Euler(-12f, 0f, 0f));
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.42f, 0.22f, 0.13f, EnemySwatch.SandstoneDark, EnemySwatch.Sandstone, EnemySwatch.GolemStoneDark, "LivingHourglass");
        }

        private static void DrawHourglassBody(EnemyMeshBuilder b)
        {
            // The two plates and the four bars of the old clock room; the sand keeps falling, and it has stopped caring in which direction.
            b.Box(new Vector3(0f, 0.62f, 0f), new Vector3(0.84f, 0.09f, 0.84f), EnemySwatch.SandstoneDark);
            b.Box(new Vector3(0f, -0.62f, 0f), new Vector3(0.84f, 0.09f, 0.84f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, 0.3f, 0f), new Vector3(0.62f, 0.58f, 0.62f), EnemySwatch.SandstoneLight);
            b.Blob(new Vector3(0f, -0.3f, 0f), new Vector3(0.62f, 0.58f, 0.62f), EnemySwatch.SandstoneLight);
            b.Segment(new Vector3(0f, 0.08f, 0f), new Vector3(0f, -0.08f, 0f), 0.08f, 0.08f, 6, EnemySwatch.StolenLight);
            b.Blob(new Vector3(0f, -0.36f, 0.14f), new Vector3(0.42f, 0.32f, 0.36f), EnemySwatch.StolenLight);
            b.Blob(new Vector3(0f, 0.22f, 0.14f), new Vector3(0.26f, 0.18f, 0.36f), EnemySwatch.StolenLight, coarse: true);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    b.Segment(new Vector3(0.36f * x, -0.62f, 0.36f * z), new Vector3(0.36f * x, 0.62f, 0.36f * z), 0.05f, 0.05f, 5, EnemySwatch.Sandstone);
                }
            }
        }

        private static void DrawHourglassArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.SandstoneDark, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.1f * side, -0.54f, 0.08f), 0.08f, 0.065f, 6, EnemySwatch.Sandstone);
            b.Blob(new Vector3(0.1f * side, -0.6f, 0.08f), new Vector3(0.22f, 0.22f, 0.22f), EnemySwatch.SandstoneDark);
        }

        // ------------------------------------------------------------------
        // Sombra Atrasada
        // ------------------------------------------------------------------

        public static void BuildLateShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawLateShadowBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.54f, 0.04f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.3f, 0.26f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, 0.08f, -0.05f), new Vector3(0.32f, 0.22f, 0.3f), EnemySwatch.Shadow, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.02f, 0.11f, 0.04f, EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.03f), new Vector3(0.12f, 0.03f, 0.08f), EnemySwatch.Shadow));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.22f, 0.22f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Shadow, EnemySwatch.Shroud, EnemySwatch.Black, 0.7f), Quaternion.Euler(0f, 0f, -24f), meshKey: "LateShadowWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.22f, 0.22f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Shadow, EnemySwatch.Shroud, EnemySwatch.Black, 0.7f), Quaternion.Euler(0f, 0f, 24f), meshKey: "LateShadowWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.3f, -0.22f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.3f, -0.6f), 0.2f, 0.08f, 6, EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, -0.32f, -0.66f), new Vector3(0.2f, 0.1f, 0.3f), EnemySwatch.Black, coarse: true);
            });
        }

        private static void DrawLateShadowBody(EnemyMeshBuilder b)
        {
            // It belonged to something that walked past an hour ago; it is still trying to catch up, and it takes that out on whoever is near.
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.54f, 0.86f, 0.42f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, -0.3f, 0f), new Vector3(0.62f, 0.5f, 0.54f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.63f, 0f), new Vector3(0.72f, 0.2f, 0.66f), EnemySwatch.Black, coarse: true);
            b.Box(new Vector3(0.06f, 0.14f, 0.22f), new Vector3(0.05f, 0.42f, 0.02f), EnemySwatch.Sorcery, Quaternion.Euler(0f, 0f, 10f));
        }

        // ------------------------------------------------------------------
        // Betume Vivo
        // ------------------------------------------------------------------

        public static void BuildLivingTar(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawLivingTar);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.08f, 0.48f, 0.2f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), EnemySwatch.Ember);
                b.Blob(new Vector3(-0.16f, -0.08f, 0.04f), new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawLivingTar(EnemyMeshBuilder b)
        {
            // The same pitch that glues the bricks and the boats, except that this one decided the glue should start with you.
            b.Blob(new Vector3(0f, 0.34f, 0f), new Vector3(1.1f, 0.68f, 1f), EnemySwatch.Black);
            b.Blob(new Vector3(0.04f, 0.6f, -0.04f), new Vector3(0.6f, 0.36f, 0.54f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(1.24f, 0.14f, 1.14f), EnemySwatch.Black, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f + 0.4f;
                b.Blob(new Vector3(Mathf.Sin(a) * 0.42f, 0.5f, Mathf.Cos(a) * 0.38f), new Vector3(0.14f, 0.18f, 0.14f), EnemySwatch.Shadow, coarse: true);
                b.Segment(new Vector3(Mathf.Sin(a) * 0.5f, 0.22f, Mathf.Cos(a) * 0.46f), new Vector3(Mathf.Sin(a) * 0.62f, 0.02f, Mathf.Cos(a) * 0.56f), 0.05f, 0.02f, 4, EnemySwatch.Black);
            }
        }

        // ------------------------------------------------------------------
        // Garça de Piche
        // ------------------------------------------------------------------

        public static void BuildTarHeron(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawTarHeronBody, Quaternion.Euler(-8f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.44f, 0.24f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.13f, 0.12f, 0.18f), EnemySwatch.Shadow);
                HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.05f, 0.05f, 0.03f, EnemySwatch.Ember);
                b.Horn(new Vector3(0f, 0.03f, 0.08f), 0.024f, 0.34f, 2, 4f, EnemySwatch.Black, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.01f, 0.1f), b =>
                b.Horn(Vector3.zero, 0.018f, 0.28f, 2, 4f, EnemySwatch.Shadow, Quaternion.Euler(92f, 0f, 0f), 4, Vector3.right));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.14f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.Ember, 0.85f), Quaternion.Euler(0f, 0f, -22f), meshKey: "TarHeronWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.14f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.Ember, 0.85f), Quaternion.Euler(0f, 0f, 22f), meshKey: "TarHeronWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.22f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.Shadow), Quaternion.Euler(20f, 0f, 0f));
        }

        private static void DrawTarHeronBody(EnemyMeshBuilder b)
        {
            // Herons fish by standing still. This one stood still in the wrong pool, and now it spits what it could not swallow.
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.36f, 0.54f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, 0.08f, -0.06f), new Vector3(0.26f, 0.18f, 0.38f), EnemySwatch.Shadow, coarse: true);
            b.Blob(new Vector3(0f, -0.16f, 0.1f), new Vector3(0.1f, 0.16f, 0.1f), EnemySwatch.Black, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.16f), new Vector3(0f, 0.44f, 0.24f), 0.065f, 0.04f, 5, EnemySwatch.Shadow);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.07f * s, -0.16f, 0f), new Vector3(0.07f * s, -0.54f, 0.04f), 0.03f, 0.022f, 4, EnemySwatch.Black);
            }

            RangedEnemyRecipes.DrawTalons(b, -0.72f, 0.07f, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Cedro Raivoso
        // ------------------------------------------------------------------

        public static void BuildAngryCedar(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.3f, 0f), DrawCedarBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.66f, 0.04f), DrawCedarHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.46f * side, 0.38f, 0f), b => DrawCedarArm(b, side),
                    Quaternion.Euler(-10f, 0f, 0f), meshKey: s > 0 ? "AngryCedarArm_R" : "AngryCedarArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.86f, 0.28f, 0.19f, EnemySwatch.BarkDark, EnemySwatch.Bark, EnemySwatch.BarkDark, "AngryCedar");
        }

        private static void DrawCedarBody(EnemyMeshBuilder b)
        {
            // Cedar that heard the axe once, three hundred years ago, and has been angry ever since. The resin is the only part that stayed kind.
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(0.9f, 1.3f, 0.76f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.34f, 0.04f), new Vector3(0.8f, 0.7f, 0.68f), EnemySwatch.Bark, coarse: true);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                b.Box(new Vector3(Mathf.Sin(a) * 0.4f, 0.04f, Mathf.Cos(a) * 0.34f), new Vector3(0.07f, 0.9f, 0.06f), EnemySwatch.BarkDark, Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
            }

            for (int i = 0; i < 5; i++)
            {
                float a = 0.5f + i * 1.25f;
                b.Blob(new Vector3(Mathf.Sin(a) * 0.4f, -0.34f + i * 0.24f, Mathf.Cos(a) * 0.32f), new Vector3(0.11f, 0.18f, 0.09f), EnemySwatch.StolenLight, coarse: true);
            }
        }

        private static void DrawCedarHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.56f, 0.4f, 0.52f), EnemySwatch.BarkDark);
            HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.14f, 0.22f, 0.08f, EnemySwatch.Ember);
            b.Box(new Vector3(0f, -0.02f, 0.25f), new Vector3(0.24f, 0.05f, 0.05f), EnemySwatch.Black);
            b.Cylinder(new Vector3(0f, 0.2f, -0.02f), 0.84f, 0.1f, 0.42f, 9, EnemySwatch.CaveMoss, bottomCap: true);
            b.Cylinder(new Vector3(0f, 0.46f, -0.02f), 0.64f, 0.08f, 0.4f, 8, EnemySwatch.Verdigris, bottomCap: true);
            b.Cylinder(new Vector3(0f, 0.72f, -0.02f), 0.42f, 0f, 0.46f, 7, EnemySwatch.CaveMoss, bottomCap: true);
        }

        private static void DrawCedarArm(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.2f * side, -0.36f, 0.08f), 0.12f, 0.09f, 6, EnemySwatch.BarkDark);
            b.Segment(new Vector3(0.2f * side, -0.36f, 0.08f), new Vector3(0.22f * side, -0.84f, 0.12f), 0.09f, 0.06f, 6, EnemySwatch.Bark);
            b.Blob(new Vector3(0.22f * side, -0.9f, 0.12f), new Vector3(0.36f, 0.3f, 0.36f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0.1f * side, -0.5f, 0.1f), new Vector3(0.1f, 0.14f, 0.08f), EnemySwatch.StolenLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Manto de Brilho
        // ------------------------------------------------------------------

        public static void BuildGlowMantle(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawGlowMantleBody);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.22f, 0.22f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Gold, EnemySwatch.StolenLight, EnemySwatch.Shroud, 0.6f), Quaternion.Euler(0f, 0f, -30f), meshKey: "GlowMantleWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.22f, 0.22f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Gold, EnemySwatch.StolenLight, EnemySwatch.Shroud, 0.6f), Quaternion.Euler(0f, 0f, 30f), meshKey: "GlowMantleWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.3f, -0.26f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.14f, -0.46f), 0.26f, 0.06f, 6, EnemySwatch.Gold);
                b.Blob(new Vector3(0f, -0.14f, -0.5f), new Vector3(0.18f, 0.08f, 0.2f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawGlowMantleBody(EnemyMeshBuilder b)
        {
            // One of the seven cloaks the cedar's guardian wore. There is nobody inside it, and it does not intend to be taken off again.
            b.Cylinder(new Vector3(0f, -0.52f, 0f), 0.56f, 0.26f, 0.94f, 12, EnemySwatch.Gold, bottomCap: true);
            b.Blob(new Vector3(0f, 0.42f, 0f), new Vector3(0.54f, 0.5f, 0.5f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, 0.1f, 0.04f), new Vector3(0.5f, 0.72f, 0.5f), EnemySwatch.StolenLight);
            b.Blob(new Vector3(0f, 0.46f, 0.1f), new Vector3(0.32f, 0.3f, 0.3f), EnemySwatch.Shroud, coarse: true);
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2f / 7f;
                b.Box(new Vector3(Mathf.Sin(a) * 0.52f, -0.44f, Mathf.Cos(a) * 0.52f), new Vector3(0.13f, 0.1f, 0.03f), EnemySwatch.Shroud, Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Filho de Ullikummi
        // ------------------------------------------------------------------

        public static void BuildUllikummiSon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.42f, 0f), DrawUllikummiSonBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 0.08f), b =>
            {
                b.Rock(new Vector3(0f, 0.1f, 0f), new Vector3(0.56f, 0.46f, 0.48f), EnemySwatch.GolemStoneDark, 1);
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.1f, 0.21f, 0.07f, EnemySwatch.Rune);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.66f * side, 0.44f, 0.02f), b => DrawUllikummiSonArm(b, side),
                    Quaternion.Euler(-12f, 0f, 0f), meshKey: s > 0 ? "UllikummiSonArm_R" : "UllikummiSonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.96f, 0.3f, 0.2f, EnemySwatch.GolemStoneDark, EnemySwatch.BatFurDark, EnemySwatch.Black, "UllikummiSon");
        }

        private static void DrawUllikummiSonBody(EnemyMeshBuilder b)
        {
            // A splinter off the stone that grew to block the sky; it has inherited the growing, and nothing else.
            b.Rock(new Vector3(0f, 0.1f, 0f), new Vector3(1.26f, 1.14f, 0.96f), EnemySwatch.GolemStoneDark, 0);
            b.Rock(new Vector3(-0.3f, 0.44f, -0.04f), new Vector3(0.5f, 0.44f, 0.5f), EnemySwatch.BatFurDark, 1);
            b.Rock(new Vector3(0.32f, 0.46f, 0.02f), new Vector3(0.48f, 0.42f, 0.46f), EnemySwatch.GolemStone, 2);
            b.Box(new Vector3(0.02f, 0.06f, 0.5f), new Vector3(0.05f, 0.7f, 0.04f), EnemySwatch.Rune, Quaternion.Euler(0f, 0f, -10f));
            b.Box(new Vector3(-0.22f, 0.16f, 0.46f), new Vector3(0.3f, 0.04f, 0.04f), EnemySwatch.Rune, Quaternion.Euler(0f, 0f, 22f));
            b.Crystal(new Vector3(-0.42f, 0.56f, -0.06f), 0.13f, 0.5f, EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, 16f));
            b.Crystal(new Vector3(0.42f, 0.58f, -0.06f), 0.13f, 0.56f, EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, -18f));
            b.Crystal(new Vector3(0f, 0.6f, -0.34f), 0.11f, 0.42f, EnemySwatch.BatFurDark, Quaternion.Euler(-20f, 0f, 0f));
        }

        private static void DrawUllikummiSonArm(EnemyMeshBuilder b, float side)
        {
            b.Rock(Vector3.zero, new Vector3(0.36f, 0.34f, 0.34f), EnemySwatch.GolemStoneDark, 2);
            b.Segment(Vector3.zero, new Vector3(0.14f * side, -0.68f, 0.06f), 0.16f, 0.14f, 6, EnemySwatch.GolemStoneDark);
            b.Rock(new Vector3(0.14f * side, -0.82f, 0.06f), new Vector3(0.44f, 0.42f, 0.42f), EnemySwatch.BatFurDark, 0);
            b.Box(new Vector3(0.14f * side, -0.82f, 0.28f), new Vector3(0.2f, 0.03f, 0.03f), EnemySwatch.Rune);
        }

        // ------------------------------------------------------------------
        // Tábua Quebrada
        // ------------------------------------------------------------------

        public static void BuildBrokenTablet(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawBrokenTablet);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(-0.08f, 0.02f, 0.13f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.08f), EnemySwatch.StolenLight);
                b.Box(new Vector3(0.14f, 0.1f, 0f), new Vector3(0.22f, 0.03f, 0.03f), EnemySwatch.SoulFire, Quaternion.Euler(0f, 0f, 32f));
                b.Box(new Vector3(-0.12f, -0.12f, 0f), new Vector3(0.2f, 0.03f, 0.03f), EnemySwatch.SoulFire, Quaternion.Euler(0f, 0f, 38f));
            });
        }

        private static void DrawBrokenTablet(EnemyMeshBuilder b)
        {
            // A line of the dream that lost the rest of its sentence; it throws what is left of the words at whoever comes to read it.
            b.Box(Vector3.zero, new Vector3(1f, 1.36f, 0.22f), EnemySwatch.SandstoneDark, top: EnemySwatch.Sandstone);
            b.Rock(new Vector3(0.36f, 0.54f, 0f), new Vector3(0.34f, 0.34f, 0.3f), EnemySwatch.Black, 0);
            b.Rock(new Vector3(0.48f, 0.26f, 0.02f), new Vector3(0.14f, 0.24f, 0.24f), EnemySwatch.Black, 1);
            b.Blob(new Vector3(0f, -0.66f, 0f), new Vector3(1.08f, 0.1f, 0.5f), EnemySwatch.GolemStoneDark, coarse: true);
            for (int row = 0; row < 6; row++)
            {
                for (int i = 0; i < 7; i++)
                {
                    // The upper-right corner of the text went with the stone.
                    if (row >= 4 && i >= 5)
                    {
                        continue;
                    }

                    float x = -0.34f + i * 0.11f;
                    float y = -0.42f + row * 0.17f;
                    b.Box(new Vector3(x, y, 0.115f), new Vector3(0.07f, 0.04f, 0.015f), row % 2 == 0 ? EnemySwatch.Rune : EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, (i % 3 - 1) * 28f));
                }
            }
        }

        // ------------------------------------------------------------------
        // Jardim-Sombra
        // ------------------------------------------------------------------

        public static void BuildGardenShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.02f, 0f), DrawGardenShadowBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.5f, 0f), DrawGardenShadowHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.42f * side, 0.2f, 0f), b => DrawGardenShadowArm(b, side),
                    Quaternion.Euler(-14f, 0f, 0f), meshKey: s > 0 ? "GardenShadowArm_R" : "GardenShadowArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.62f, 0.22f, 0.16f, EnemySwatch.Shadow, EnemySwatch.Black, EnemySwatch.Shroud, "GardenShadow");
        }

        private static void DrawGardenShadowBody(EnemyMeshBuilder b)
        {
            // The trees that were cut in the dream of Ohya and Hahya. Nobody has ever found the stumps, so they went looking for us.
            b.Segment(new Vector3(0f, -0.42f, 0f), new Vector3(0f, 0.5f, 0f), 0.42f, 0.36f, 10, EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.3f, 0f), new Vector3(0.9f, 0.5f, 0.84f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, 0.2f, 0f), new Vector3(0.74f, 0.7f, 0.7f), EnemySwatch.Shadow);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f + 0.2f;
                b.Box(new Vector3(Mathf.Sin(a) * 0.37f, 0.06f, Mathf.Cos(a) * 0.35f), new Vector3(0.06f, 0.78f, 0.05f), EnemySwatch.Black, Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
            }
        }

        private static void DrawGardenShadowHead(EnemyMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.42f, 0.42f, 0.12f, 12, EnemySwatch.IvoryDark, cap: EnemySwatch.IvoryDark, bottomCap: true);
            b.Cylinder(new Vector3(0f, 0.12f, 0f), 0.3f, 0.3f, 0.02f, 12, EnemySwatch.Sorcery, cap: EnemySwatch.IvoryDark);
            b.Cylinder(new Vector3(0f, 0.14f, 0f), 0.14f, 0.14f, 0.02f, 10, EnemySwatch.Sorcery, cap: EnemySwatch.Ivory);
            HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.04f, 0.36f, 0.08f, EnemySwatch.Sorcery);
        }

        private static void DrawGardenShadowArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), EnemySwatch.Black, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.14f * side, -0.34f, 0.1f), 0.1f, 0.08f, 6, EnemySwatch.Shadow);
            b.Segment(new Vector3(0.14f * side, -0.34f, 0.1f), new Vector3(0.18f * side, -0.7f, 0.12f), 0.08f, 0.1f, 6, EnemySwatch.Black);
            b.Cylinder(new Vector3(0.18f * side, -0.72f, 0.12f), 0.11f, 0.11f, 0.03f, 8, EnemySwatch.IvoryDark, cap: EnemySwatch.Sorcery, rotation: Quaternion.Euler(180f, 0f, 0f));
        }
    }
}
