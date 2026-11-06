using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Planície de Mingcheng (Docs/Mapas100.md 41–45) that the continuous world played with stand-ins,
    /// low-poly from the shared palette like <see cref="OverworldBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Leão de Pedra Apagado</b> (~1.15 m): grey guardian lion statue with a curly mane, cracks of trapped light.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four thick legs with pads.</item>
    /// <item><b>Raposa de Cauda Lanterna</b> (~0.6 m): red fox whose bushy tail ends in a glowing paper lantern.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c> → <c>Tail_1</c> → <c>Lantern</c>; four jointed legs.</item>
    /// <item><b>Soldado de Terracota Apagado</b> (~1.9 m): clay soldier in lamellar armour with a halberd in the right hand.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>, <c>Arm_R</c> → <c>Weapon</c>; <c>Leg_L</c>/<c>Leg_R</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Tartaruga de Pedra</b> (~1.3 m with the stele): stone turtle carrying a stele with glowing script.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four stubby legs.</item>
    /// <item><b>Búfalo de Lama</b> (~1.2 m): grey water buffalo caked in mud, horns swept back; lowers its head to charge.
    /// <c>Body</c> → <c>Head</c>, <c>Tail</c>; four legs with hooves.</item>
    /// <item><b>Filhote de Nian</b> (~0.9 m): red-and-gold lion-dog cub, one horn, a big gaping jaw.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c> → <c>Tail_1</c>; four jointed legs.</item>
    /// </list>
    /// </summary>
    public static class MingchengBeastRecipes
    {
        // ------------------------------------------------------------------
        // Leão de Pedra Apagado
        // ------------------------------------------------------------------

        public static void BuildStoneLion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawStoneLionBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.5f), DrawStoneLionHead, Quaternion.Euler(4f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.12f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.02f, 0.09f), new Vector3(0.2f, 0.06f, 0.18f), EnemySwatch.GolemStoneDark);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.06f * s, 0.01f, 0.15f), 0.016f, 0f, 0.05f, 3, EnemySwatch.Bone);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.46f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.18f, -0.14f), 0.05f, 0.04f, 5, EnemySwatch.GolemStone);
                b.Blob(new Vector3(0f, 0.24f, -0.14f), new Vector3(0.16f, 0.16f, 0.14f), EnemySwatch.GolemStoneDark, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.18f * side, 0.52f, 0.34f), new Vector3(0f, -0.25f, 0.02f), new Vector3(0f, -0.24f, 0.02f),
                    0.09f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.15f, 0.08f, 0.18f), "StoneLionFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.18f * side, 0.54f, -0.34f), new Vector3(0f, -0.26f, -0.04f), new Vector3(0f, -0.25f, 0.04f),
                    0.1f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.15f, 0.08f, 0.18f), "StoneLionHind");
            }
        }

        private static void DrawStoneLionBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.52f, 0.96f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.44f, 0.3f, 0.8f), EnemySwatch.GolemStoneDark, coarse: true);
            // The carved curls of the mane over the shoulders, the ribbon and bell round the neck.
            for (int i = 0; i < 6; i++)
            {
                float a = i / 5f * Mathf.PI;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.24f, 0.1f + Mathf.Sin(a) * 0.16f, 0.36f), new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.GolemStoneDark, coarse: true);
            }

            b.Cylinder(new Vector3(0f, 0.02f, 0.44f), 0.22f, 0.22f, 0.05f, 8, EnemySwatch.WarCape, rotation: Quaternion.Euler(70f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.12f, 0.5f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.Gold, coarse: true);
            // The cracks where the shadow's cold got into the stone and the light it holds shows through.
            b.Box(new Vector3(0.08f, 0.26f, -0.06f), new Vector3(0.04f, 0.03f, 0.34f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 22f, 0f));
            b.Box(new Vector3(-0.2f, 0.12f, -0.2f), new Vector3(0.03f, 0.2f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 12f));
        }

        private static void DrawStoneLionHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, -0.04f), new Vector3(0.46f, 0.44f, 0.3f), EnemySwatch.GolemStoneDark);
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.28f, 0.28f), EnemySwatch.GolemStone);
            b.Box(new Vector3(0f, -0.04f, 0.15f), new Vector3(0.18f, 0.1f, 0.1f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.01f, 0.21f), new Vector3(0.07f, 0.05f, 0.04f), EnemySwatch.GolemStoneDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // Bulging carved eyes, lit by the trapped light; curls over the brow.
                b.Blob(new Vector3(0.07f * s, 0.06f, 0.13f), new Vector3(0.06f, 0.05f, 0.04f), EnemySwatch.WarEye, coarse: true);
                b.Blob(new Vector3(0.08f * s, 0.15f, 0.06f), new Vector3(0.09f, 0.07f, 0.07f), EnemySwatch.GolemStoneDark, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Raposa de Cauda Lanterna
        // ------------------------------------------------------------------

        public static void BuildLanternFox(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawFoxBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.1f, 0.36f), DrawFoxHead, Quaternion.Euler(4f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.1f), b => CoastDeltaBeastRecipes.DrawFelineJaw(b, EnemySwatch.WolfBelly, 0.7f));

            // The bushy tail curves up behind it; at the tip hangs the paper lantern the fox steals the light with.
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.32f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.08f, -0.22f), 0.06f, 0.09f, 6, EnemySwatch.Copper);
            });
            int tail1 = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, 0.08f, -0.22f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.2f, -0.1f), 0.09f, 0.05f, 6, EnemySwatch.Copper);
                b.Blob(new Vector3(0f, 0.18f, -0.09f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.WolfBelly, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Lantern, tail1, new Vector3(0f, 0.2f, -0.1f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.03f), 0.008f, 0.008f, 3, EnemySwatch.BarkDark);
                b.Cylinder(new Vector3(0f, -0.06f, -0.03f), 0.012f, 0.012f, 0.02f, 4, EnemySwatch.BarkDark);
                b.Blob(new Vector3(0f, -0.12f, -0.03f), new Vector3(0.1f, 0.12f, 0.1f), EnemySwatch.Ember, coarse: true);
                b.Cylinder(new Vector3(0f, -0.19f, -0.03f), 0.03f, 0.03f, 0.015f, 5, EnemySwatch.WarCape);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.08f * side, 0.34f, 0.22f), new Vector3(0f, -0.17f, -0.02f), new Vector3(0f, -0.16f, 0.03f),
                    0.035f, EnemySwatch.Copper, EnemySwatch.Black, "FoxFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.08f * side, 0.35f, -0.22f), new Vector3(0f, -0.17f, -0.05f), new Vector3(0f, -0.17f, 0.05f),
                    0.04f, EnemySwatch.Copper, EnemySwatch.Black, "FoxHind");
            }
        }

        private static void DrawFoxBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.28f, 0.64f), EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.18f, 0.14f, 0.44f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, 0.08f, -0.02f), new Vector3(0.18f, 0.1f, 0.46f), EnemySwatch.CopperDark, coarse: true);
        }

        private static void DrawFoxHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.16f, 0.18f), EnemySwatch.Copper);
            b.Cylinder(new Vector3(0f, -0.02f, 0.06f), 0.05f, 0.015f, 0.14f, 5, EnemySwatch.WolfBelly, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.02f, 0.2f), new Vector3(0.03f, 0.025f, 0.02f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // Sly narrow eyes with the lantern's glint, tall black-tipped ears.
                b.Blob(new Vector3(0.045f * s, 0.035f, 0.07f), new Vector3(0.03f, 0.015f, 0.02f), EnemySwatch.Ember, coarse: true);
                b.Cylinder(new Vector3(0.05f * s, 0.06f, -0.01f), 0.035f, 0f, 0.1f, 4, EnemySwatch.Copper, rotation: Quaternion.Euler(-8f, 0f, -14f * s));
                b.Cylinder(new Vector3(0.062f * s, 0.13f, -0.02f), 0.014f, 0f, 0.035f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(-8f, 0f, -14f * s));
            }
        }

        // ------------------------------------------------------------------
        // Soldado de Terracota Apagado
        // ------------------------------------------------------------------

        public static void BuildTerracottaSoldier(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.96f, 0f), DrawTerracottaBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.62f, 0.02f), DrawTerracottaHead);

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.3f * side, 0.46f, 0f),
                    b => DrawTerracottaArm(b, side), Quaternion.Euler(-8f, 0f, 0f));
                if (right)
                {
                    // The bronze halberd (ji), held upright, its blade green with age.
                    bp.Mesh(ProceduralEnemyParts.Weapon, arm, new Vector3(0.04f, -0.6f, 0.12f), b =>
                    {
                        b.Segment(new Vector3(0f, -0.3f, 0f), new Vector3(0f, 1.2f, 0f), 0.022f, 0.02f, 5, EnemySwatch.Bark);
                        b.Cylinder(new Vector3(0f, 1.2f, 0f), 0.03f, 0f, 0.2f, 4, EnemySwatch.Verdigris);
                        b.Box(new Vector3(0f, 1.12f, 0.1f), new Vector3(0.02f, 0.06f, 0.2f), EnemySwatch.Verdigris);
                        b.Box(new Vector3(0f, 1.04f, 0f), new Vector3(0.05f, 0.04f, 0.05f), EnemySwatch.WarCape);
                    });
                }

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1, new Vector3(0.12f * side, 0.6f, 0f),
                    b => b.Segment(new Vector3(0f, -0.29f, 0.02f), new Vector3(0f, 0.04f, 0f), 0.07f, 0.085f, 6, EnemySwatch.Sandstone), meshKey: "TerracottaThigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg, new Vector3(0f, -0.29f, 0.02f), b =>
                {
                    b.Segment(new Vector3(0f, -0.27f, 0f), Vector3.zero, 0.06f, 0.07f, 6, EnemySwatch.SandstoneDark);
                    b.Cylinder(new Vector3(0f, -0.2f, 0f), 0.075f, 0.075f, 0.12f, 6, EnemySwatch.Sandstone);
                }, meshKey: "TerracottaShin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -0.27f, 0f),
                    b => b.Box(new Vector3(0f, -0.005f, 0.05f), new Vector3(0.11f, 0.06f, 0.22f), EnemySwatch.SandstoneDark), meshKey: "TerracottaFoot");
            }
        }

        private static void DrawTerracottaBody(EnemyMeshBuilder b)
        {
            // Clay torso under armour of small plates (the lamellae), a belted skirt, the clay cracked where the light leaks.
            b.Blob(new Vector3(0f, 0.24f, 0f), new Vector3(0.5f, 0.66f, 0.34f), EnemySwatch.Sandstone);
            for (int row = 0; row < 3; row++)
            {
                for (int col = -2; col <= 2; col++)
                {
                    b.Box(new Vector3(col * 0.08f, 0.38f - row * 0.11f, 0.16f), new Vector3(0.07f, 0.09f, 0.03f), EnemySwatch.SandstoneDark);
                }
            }

            b.Box(new Vector3(0f, 0.02f, 0f), new Vector3(0.48f, 0.06f, 0.34f), EnemySwatch.Leather);
            b.Cylinder(new Vector3(0f, -0.26f, 0f), 0.26f, 0.22f, 0.28f, 8, EnemySwatch.SandstoneDark);
            b.Box(new Vector3(0.06f, 0.3f, 0.18f), new Vector3(0.02f, 0.2f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 20f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.24f * s, 0.5f, 0f), new Vector3(0.18f, 0.12f, 0.2f), EnemySwatch.SandstoneDark, coarse: true);
            }
        }

        private static void DrawTerracottaHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.22f, 0.26f, 0.22f), EnemySwatch.SandstoneLight);
            // The topknot tied to the right, the moustache, the stern sleeping eyes lit faintly from inside.
            b.Blob(new Vector3(0.06f, 0.26f, -0.02f), new Vector3(0.09f, 0.08f, 0.09f), EnemySwatch.SandstoneDark, coarse: true);
            b.Box(new Vector3(0f, 0.05f, 0.11f), new Vector3(0.1f, 0.02f, 0.02f), EnemySwatch.SandstoneDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.045f * s, 0.12f, 0.105f), new Vector3(0.04f, 0.012f, 0.01f), EnemySwatch.SoftEye);
            }

            b.Cylinder(new Vector3(0f, -0.04f, 0f), 0.07f, 0.08f, 0.06f, 6, EnemySwatch.Sandstone);
        }

        private static void DrawTerracottaArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.SandstoneDark, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.03f * side, -0.32f, 0.04f), 0.06f, 0.055f, 6, EnemySwatch.Sandstone);
            b.Segment(new Vector3(0.03f * side, -0.32f, 0.04f), new Vector3(0.04f * side, -0.58f, 0.1f), 0.055f, 0.05f, 6, EnemySwatch.Sandstone);
            b.Blob(new Vector3(0.04f * side, -0.62f, 0.12f), new Vector3(0.09f, 0.1f, 0.09f), EnemySwatch.SandstoneLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Tartaruga de Pedra
        // ------------------------------------------------------------------

        public static void BuildStoneTurtle(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.34f, 0f), DrawTurtleBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.58f), DrawTurtleHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.06f), b =>
                b.Box(new Vector3(0f, -0.02f, 0.1f), new Vector3(0.16f, 0.05f, 0.2f), EnemySwatch.GolemStoneDark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.04f, -0.56f), b =>
                b.Cylinder(Vector3.zero, 0.06f, 0f, 0.18f, 5, EnemySwatch.GolemStone, rotation: Quaternion.Euler(-100f, 0f, 0f)));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.36f * side, 0.28f, 0.34f), new Vector3(0.06f * side, -0.13f, 0.02f), new Vector3(0f, -0.12f, 0.02f),
                    0.08f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.16f, 0.07f, 0.18f), "TurtleFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.36f * side, 0.28f, -0.34f), new Vector3(0.06f * side, -0.13f, -0.02f), new Vector3(0f, -0.12f, 0.02f),
                    0.08f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.16f, 0.07f, 0.18f), "TurtleHind");
            }
        }

        private static void DrawTurtleBody(EnemyMeshBuilder b)
        {
            // The domed shell with its hexagon plates, moss on the rim, and the stele it carries for the canal's dead kings.
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.92f, 0.46f, 1.1f), EnemySwatch.GolemStone);
            b.Cylinder(new Vector3(0f, -0.1f, 0f), 0.5f, 0.5f, 0.08f, 10, EnemySwatch.GolemStoneDark);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f;
                b.Cylinder(new Vector3(Mathf.Sin(a) * 0.26f, 0.2f, Mathf.Cos(a) * 0.3f), 0.1f, 0.09f, 0.04f, 6, EnemySwatch.GolemStoneDark,
                    rotation: Quaternion.Euler(Mathf.Cos(a) * 20f, 0f, -Mathf.Sin(a) * 20f));
            }

            b.Blob(new Vector3(0.36f, -0.04f, -0.1f), new Vector3(0.14f, 0.1f, 0.4f), EnemySwatch.CaveMoss, coarse: true);
            b.Box(new Vector3(0f, 0.32f, -0.04f), new Vector3(0.4f, 0.1f, 0.24f), EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0f, 0.64f, -0.04f), new Vector3(0.32f, 0.56f, 0.1f), EnemySwatch.GolemStone);
            b.Cylinder(new Vector3(0f, 0.92f, -0.04f), 0.18f, 0.06f, 0.1f, 6, EnemySwatch.GolemStoneDark);
            // The script on the stele, still lit by the light the turtle swore to keep.
            for (int line = 0; line < 4; line++)
            {
                b.Box(new Vector3(-0.09f + line * 0.06f, 0.64f, 0.015f), new Vector3(0.02f, 0.38f, 0.01f), EnemySwatch.Rune);
            }
        }

        private static void DrawTurtleHead(EnemyMeshBuilder b)
        {
            b.Segment(new Vector3(0f, 0f, -0.14f), new Vector3(0f, 0.02f, 0.04f), 0.11f, 0.1f, 6, EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.04f, 0.08f), new Vector3(0.2f, 0.16f, 0.24f), EnemySwatch.GolemStone);
            b.Cylinder(new Vector3(0f, 0.03f, 0.18f), 0.06f, 0.02f, 0.06f, 5, EnemySwatch.GolemStoneDark, rotation: Quaternion.Euler(90f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.08f, 0.12f), new Vector3(0.035f, 0.03f, 0.03f), EnemySwatch.MoonEye, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Búfalo de Lama
        // ------------------------------------------------------------------

        public static void BuildMudBuffalo(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.78f, 0f), DrawBuffaloBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, -0.02f, 0.7f), DrawBuffaloHead, Quaternion.Euler(24f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.66f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.34f, -0.04f), 0.03f, 0.02f, 5, EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, -0.36f, -0.04f), new Vector3(0.06f, 0.1f, 0.06f), EnemySwatch.Black, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.22f * side, 0.6f, 0.42f), new Vector3(0f, -0.29f, 0.02f), new Vector3(0f, -0.28f, 0f),
                    0.085f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, new Vector3(0.13f, 0.08f, 0.15f), "BuffaloFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.22f * side, 0.62f, -0.42f), new Vector3(0f, -0.3f, -0.04f), new Vector3(0f, -0.29f, 0.04f),
                    0.09f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, new Vector3(0.13f, 0.08f, 0.15f), "BuffaloHind");
            }
        }

        private static void DrawBuffaloBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.74f, 0.66f, 1.36f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.18f, 0.3f), new Vector3(0.6f, 0.4f, 0.5f), EnemySwatch.GolemStoneDark, coarse: true);
            // The paddy mud dried in cakes on the legs and the belly, cracking where the cold light leaks.
            b.Blob(new Vector3(0f, -0.22f, 0f), new Vector3(0.6f, 0.26f, 1.1f), EnemySwatch.SandstoneDark, coarse: true);
            b.Blob(new Vector3(0.3f, -0.06f, -0.3f), new Vector3(0.16f, 0.26f, 0.4f), EnemySwatch.SandstoneDark, coarse: true);
            b.Box(new Vector3(-0.06f, 0.33f, -0.1f), new Vector3(0.04f, 0.03f, 0.36f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, -14f, 0f));
        }

        private static void DrawBuffaloHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, 0.1f), new Vector3(0.3f, 0.3f, 0.44f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, -0.06f, 0.3f), new Vector3(0.22f, 0.18f, 0.16f), EnemySwatch.GolemStone, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // The wide flat horns of the water buffalo, swept out and back in a crescent.
                b.Horn(new Vector3(0.12f * s, 0.13f, 0.04f), 0.055f, 0.5f, 5, -18f, EnemySwatch.Horn, Quaternion.Euler(0f, 0f, -80f * s), 5, Vector3.right * s);
                b.Blob(new Vector3(0.13f * s, 0.04f, 0.2f), new Vector3(0.035f, 0.03f, 0.03f), EnemySwatch.WarEye, coarse: true);
                b.Blob(new Vector3(0.18f * s, 0.08f, 0.02f), new Vector3(0.09f, 0.04f, 0.05f), EnemySwatch.GolemStoneDark, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Filhote de Nian
        // ------------------------------------------------------------------

        public static void BuildNianCub(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawNianBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.18f, 0.38f), DrawNianHead, Quaternion.Euler(4f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.02f, 0.1f), new Vector3(0.2f, 0.05f, 0.18f), EnemySwatch.WarCape);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.07f * s, 0f, 0.16f), 0.018f, 0f, 0.06f, 3, EnemySwatch.Fang);
                }
            });
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.34f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, 0.12f, -0.16f), 0.04f, 0.04f, 5, EnemySwatch.WarCape));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, 0.12f, -0.16f), b =>
                b.Blob(new Vector3(0f, 0.06f, -0.06f), new Vector3(0.16f, 0.16f, 0.14f), EnemySwatch.Gold, coarse: true));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.13f * side, 0.4f, 0.24f), new Vector3(0f, -0.2f, -0.02f), new Vector3(0f, -0.19f, 0.03f),
                    0.06f, EnemySwatch.WarCape, EnemySwatch.Gold, "NianFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.13f * side, 0.42f, -0.24f), new Vector3(0f, -0.21f, -0.05f), new Vector3(0f, -0.2f, 0.05f),
                    0.065f, EnemySwatch.WarCape, EnemySwatch.Gold, "NianHind");
            }
        }

        private static void DrawNianBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.4f, 0.74f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.32f, 0.2f, 0.56f), EnemySwatch.Gold, coarse: true);
            // A ridge of gold tufts down the back, the shadow's smoke in the fur.
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0f, 0.2f, 0.24f - i * 0.16f), new Vector3(0.14f, 0.1f, 0.12f), EnemySwatch.Gold, coarse: true);
            }

            b.Blob(new Vector3(0.14f, 0.06f, -0.16f), new Vector3(0.12f, 0.12f, 0.2f), EnemySwatch.Shadow, coarse: true);
        }

        private static void DrawNianHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, -0.04f), new Vector3(0.44f, 0.4f, 0.3f), EnemySwatch.Gold);
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.3f, 0.3f), EnemySwatch.WarCape);
            b.Box(new Vector3(0f, -0.03f, 0.15f), new Vector3(0.22f, 0.1f, 0.1f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, 0.01f, 0.21f), new Vector3(0.08f, 0.05f, 0.04f), EnemySwatch.Black, coarse: true);
            // The single horn on the brow, big round eyes that hate red lanterns and loud drums.
            b.Horn(new Vector3(0f, 0.14f, 0.04f), 0.04f, 0.2f, 3, -12f, EnemySwatch.Ivory, Quaternion.Euler(-20f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, 0.06f, 0.13f), new Vector3(0.06f, 0.06f, 0.04f), EnemySwatch.WarEye, coarse: true);
                b.Blob(new Vector3(0.15f * s, 0.12f, -0.02f), new Vector3(0.08f, 0.06f, 0.04f), EnemySwatch.Gold, coarse: true);
            }
        }
    }
}
