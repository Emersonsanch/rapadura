using System;
using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-second pass of beasts (bible §3.2.1): the roles of Regions 13–15, the Rio das Doze Horas, the Margens da Mistura and the
    /// Limiar Profundo (Docs/Mapas100.md 74–90), that the continuous world still played with another region's beast, low-poly from the shared
    /// palette like <see cref="HighlandOwnBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Salamandra de Sombra</b> (75, ~0.7 m): a black salamander of the Lake of Fire with embers down its back and a lit mouth. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Escorpião dos Anéis</b> (74, ~1.2 m): a heavy scorpion of the Coils of Mehen, sandstone bands across its back like the serpent's rings. <c>Body</c>, claws, <c>Tail</c>; eight legs.</item>
    /// <item><b>Ushabti Apagado</b> (74, ~1.4 m): an answering figure of the night barge whose glyphs went out, a hoe in its hand. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// <item><b>Sombra Fervida</b> (75, ~1.1 m): a puddle of shadow boiling on the shore of the Lake of Fire. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Crocodilo de Lodo</b> (76, ~0.8 m): a river crocodile caked in the grey mud of Apep's sandbank, reeds stuck to its back. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Ushabti Rachado</b> (75, ~1.4 m): a red clay figure cracked by the heat, the fire showing through. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// <item><b>Ushabti de Relógio</b> (78, ~1.4 m): a sandstone figure with a sundial on its chest and a gnomon for a staff. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// <item><b>Fumaça de Piche</b> (79, ~1.4 m): a black smoke off the bitumen wells, dripping pitch, a spark caught in it. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Fumaça Sufocante</b> (84, ~1.4 m): a column of the Smoke's own grey smoke, one of the five elements of the Darkness. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Cardume Escuro</b> (87, ~1.2 m): a swirl of small dark fish of the Pit of Mire round the light they swallowed. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Serpente Cega</b> (88, ~0.8 m): a pale eyeless serpent of the Pit of Darkness that finds you by the warmth. Snake skeleton (<c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c> chain).</item>
    /// <item><b>Ushabti Apagado Gigante</b> (90, ~2.3 m): a huge answering figure set to keep the Gate of the Land of Darkness shut. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// </list>
    /// </summary>
    public static class DeepOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Shared: the answering figures (ushabtis), scaled by s
        // ------------------------------------------------------------------

        private static void BuildFigure(ProceduralEnemyBlueprint bp, string key, float s, EnemySwatch stone, EnemySwatch dark,
            Action<EnemyMeshBuilder> bodyExtra, Action<EnemyMeshBuilder> headExtra, Action<EnemyMeshBuilder, float> rightHand)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f * s, 0f), b =>
            {
                // The mummy-shaped body of the figures: a plinth at the feet, the wrapped legs, the shoulders.
                b.Box(new Vector3(0f, -0.58f * s, 0f), new Vector3(0.62f * s, 0.08f * s, 0.5f * s), dark);
                b.Cylinder(new Vector3(0f, -0.56f * s, 0f), 0.28f * s, 0.24f * s, 0.9f * s, 8, stone, bottomCap: true);
                b.Blob(new Vector3(0f, 0.3f * s, 0f), new Vector3(0.6f * s, 0.36f * s, 0.42f * s), stone);
                bodyExtra(b);
            });
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.52f * s, 0.02f * s), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f * s, 0.34f * s, 0.28f * s), stone);
                // The striped wig of the figures.
                b.Blob(new Vector3(0f, 0.06f * s, -0.04f * s), new Vector3(0.38f * s, 0.32f * s, 0.34f * s), dark, coarse: true);
                headExtra(b);
            });
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bool right = side > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.27f * s * sd, 0.3f * s, 0.04f * s), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.04f * s * sd, -0.28f * s, 0.12f * s), 0.08f * s, 0.065f * s, 6, stone);
                    b.Segment(new Vector3(0.04f * s * sd, -0.28f * s, 0.12f * s), new Vector3(0.02f * s * sd, -0.46f * s, 0.28f * s), 0.065f * s, 0.055f * s, 6, dark);
                    b.Blob(new Vector3(0.02f * s * sd, -0.5f * s, 0.3f * s), new Vector3(0.13f * s, 0.12f * s, 0.13f * s), stone, coarse: true);
                    if (right)
                    {
                        rightHand(b, s);
                    }
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: key + (right ? "Arm_R" : "Arm_L"));
            }
        }

        /// <summary>The hoe the figures carry to work the fields of the other world.</summary>
        private static void DrawHoe(EnemyMeshBuilder b, float s, EnemySwatch blade)
        {
            b.Segment(new Vector3(0.02f * s, -0.62f * s, 0.3f * s), new Vector3(0.02f * s, -0.08f * s, 0.34f * s), 0.025f * s, 0.025f * s, 5, EnemySwatch.Bark);
            b.Box(new Vector3(0.02f * s, -0.66f * s, 0.4f * s), new Vector3(0.14f * s, 0.04f * s, 0.18f * s), blade, Quaternion.Euler(20f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Salamandra de Sombra
        // ------------------------------------------------------------------

        public static void BuildShadowSalamander(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawShadowSalamanderBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.04f, 0.5f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.32f, 0.17f, 0.3f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, 0.01f, 0.18f), new Vector3(0.24f, 0.08f, 0.22f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.07f, 0.08f, 0.05f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.06f), b =>
            {
                // The mouth that drank from the Lake of Fire and still glows inside.
                b.Box(new Vector3(0f, -0.01f, 0.13f), new Vector3(0.22f, 0.04f, 0.24f), EnemySwatch.BatFurDark);
                b.Box(new Vector3(0f, 0.012f, 0.13f), new Vector3(0.15f, 0.01f, 0.2f), EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.44f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.05f, -0.02f, -0.4f), 0.12f, 0.06f, 6, EnemySwatch.Shadow);
                b.Segment(new Vector3(0.05f, -0.02f, -0.4f), new Vector3(-0.06f, -0.06f, -0.74f), 0.06f, 0.01f, 5, EnemySwatch.Black);
                for (int i = 0; i < 3; i++)
                {
                    b.Blob(new Vector3(0.02f, 0.08f - i * 0.02f, -0.12f - i * 0.2f), new Vector3(0.06f, 0.04f, 0.06f), EnemySwatch.Ember, coarse: true);
                }
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.3f, 0.27f, 0.26f, -0.26f, 0.055f, EnemySwatch.Shadow, EnemySwatch.Black, "ShadowSalamander");
        }

        private static void DrawShadowSalamanderBody(EnemyMeshBuilder b)
        {
            // The salamanders of the stories live in the fire and do not burn. These came out of the Lake of Fire where the Duat burns its
            // enemies, and the shadow they carry out with them does not burn either: it only got hotter.
            b.Blob(Vector3.zero, new Vector3(0.58f, 0.3f, 0.96f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.48f, 0.16f, 0.84f), EnemySwatch.BatFurDark, coarse: true);
            // The flame-crest down its back, and the embers it carries between the scales.
            for (int i = 0; i < 5; i++)
            {
                float z = -0.3f + i * 0.15f;
                b.Cylinder(new Vector3(0f, 0.12f, z), 0.05f, 0f, 0.18f - Mathf.Abs(i - 2) * 0.03f, 4, EnemySwatch.Black);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.17f * s, 0.08f, z + 0.06f), new Vector3(0.07f, 0.05f, 0.07f), EnemySwatch.Ember, coarse: true);
                }
            }
        }

        // ------------------------------------------------------------------
        // Escorpião dos Anéis
        // ------------------------------------------------------------------

        private const float CoilScorpionHip = 0.34f;
        private static readonly float[] CoilScorpionHipZ = { 0.22f, 0.07f, -0.09f, -0.24f };
        private static readonly float[] CoilScorpionSplay = { 36f, 12f, -14f, -38f };

        public static void BuildCoilScorpion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, CoilScorpionHip, 0f), DrawCoilScorpionBody);
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body, new Vector3(0.22f * side, 0.02f, 0.38f),
                    b => DrawCoilScorpionClaw(b, sd), meshKey: side < 0 ? "CoilScorpionClaw_L" : "CoilScorpionClaw_R");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.42f), DrawCoilScorpionTail);
            AddSplayedLegs(bp, CoilScorpionHip, 0.2f, CoilScorpionHipZ, CoilScorpionSplay, new Vector3(0.26f, 0.15f, 0f), 0.22f,
                EnemySwatch.GolemStoneDark, EnemySwatch.Black, "CoilScorpion");
        }

        private static void DrawCoilScorpionBody(EnemyMeshBuilder b)
        {
            // Mehen coils round the barge every night so Apep cannot reach it; these scorpions nest between the coils and took the rings
            // for their own armour. The shadow taught them to guard the coils against the barge instead.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.32f, 0.98f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, -0.07f, 0f), new Vector3(0.56f, 0.18f, 0.86f), EnemySwatch.Black, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                // The sandstone rings across its back, a glyph of the coiled serpent lit on each.
                float z = -0.3f + i * 0.2f;
                b.Box(new Vector3(0f, 0.14f, z), new Vector3(0.5f - i * 0.04f, 0.04f, 0.07f), EnemySwatch.SandstoneDark);
                b.Box(new Vector3(0f, 0.165f, z), new Vector3(0.06f, 0.015f, 0.04f), EnemySwatch.Rune);
            }

            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.12f, 0.43f, 0.05f, EnemySwatch.Ember);
        }

        private static void DrawCoilScorpionClaw(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.14f * side, 0.03f, 0.16f), 0.06f, 0.05f, 5, EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0.16f * side, 0.04f, 0.32f), new Vector3(0.22f, 0.15f, 0.26f), EnemySwatch.Black);
            b.Box(new Vector3(0.16f * side, 0.1f, 0.3f), new Vector3(0.16f, 0.03f, 0.05f), EnemySwatch.SandstoneDark);
            b.Cylinder(new Vector3(0.12f * side, 0.06f, 0.42f), 0.035f, 0.006f, 0.15f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(80f, 0f, 0f));
            b.Cylinder(new Vector3(0.2f * side, 0.02f, 0.42f), 0.03f, 0.005f, 0.13f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(95f, 0f, 0f));
        }

        private static void DrawCoilScorpionTail(EnemyMeshBuilder b)
        {
            // Six knuckles arching forward, a sandstone ring on each, and the coal in the sting.
            Vector3[] points =
            {
                Vector3.zero, new Vector3(0f, 0.16f, -0.18f), new Vector3(0f, 0.4f, -0.28f), new Vector3(0f, 0.62f, -0.2f),
                new Vector3(0f, 0.76f, -0.02f), new Vector3(0f, 0.78f, 0.14f),
            };
            for (int i = 0; i < points.Length - 1; i++)
            {
                float r0 = 0.09f - i * 0.012f;
                float r1 = 0.09f - (i + 1) * 0.012f;
                b.Segment(points[i], points[i + 1], r0, r1, 5, EnemySwatch.GolemStoneDark);
                b.Blob(points[i + 1], Vector3.one * (r1 * 2.3f), EnemySwatch.SandstoneDark, coarse: true);
            }

            Vector3 end = points[points.Length - 1];
            b.Blob(end + new Vector3(0f, -0.03f, 0.05f), new Vector3(0.12f, 0.12f, 0.14f), EnemySwatch.Ember);
            b.Cylinder(end + new Vector3(0f, -0.03f, 0.1f), 0.035f, 0f, 0.14f, 4, EnemySwatch.Fang, rotation: Quaternion.Euler(115f, 0f, 0f));
        }

        /// <summary>Jointed legs splayed out from the body: <c>Leg_L0</c> → <c>Shin_L0</c> → <c>Foot_L0</c>, the foot on the ground.</summary>
        private static void AddSplayedLegs(ProceduralEnemyBlueprint bp, float hipY, float hipX, float[] hipZ, float[] splay, Vector3 knee, float tipX,
            EnemySwatch upper, EnemySwatch lower, string meshKey)
        {
            Vector3 tip = new Vector3(tipX, -(hipY + knee.y - 0.01f), 0f);
            for (int pair = 0; pair < hipZ.Length; pair++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    string id = (side < 0 ? "L" : "R") + pair;
                    float yaw = side > 0 ? -splay[pair] : 180f + splay[pair];
                    int hip = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, new Vector3(hipX * side, hipY, hipZ[pair]), b =>
                    {
                        b.Blob(Vector3.zero, new Vector3(0.09f, 0.09f, 0.09f), lower, coarse: true);
                        b.Segment(Vector3.zero, knee, 0.042f, 0.034f, 5, upper);
                        b.Blob(knee, new Vector3(0.07f, 0.07f, 0.07f), lower, coarse: true);
                    }, Quaternion.Euler(0f, yaw, 0f), meshKey: meshKey + "Upper");
                    int shin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, hip, knee, b =>
                    {
                        Vector3 mid = tip * 0.5f;
                        b.Segment(Vector3.zero, mid, 0.032f, 0.025f, 5, upper);
                        b.Segment(mid, tip, 0.025f, 0.006f, 4, lower);
                    }, meshKey: meshKey + "Lower");
                    bp.Node(ProceduralEnemyParts.FootPrefix + id, shin, tip);
                }
            }
        }

        // ------------------------------------------------------------------
        // Ushabti Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedUshabti(ProceduralEnemyBlueprint bp)
        {
            // The answering figures ride the night barge to work the fields of the other world when they are called. These were called by
            // the shadow among the Coils of Mehen, and the glyphs that said whose they were went out one by one.
            BuildFigure(bp, "DimmedUshabti", 1f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, b =>
            {
                b.Box(new Vector3(0f, -0.14f, 0.25f), new Vector3(0.1f, 0.62f, 0.03f), EnemySwatch.GolemStoneDark);
                for (int i = 0; i < 4; i++)
                {
                    // Only the lowest glyph is still lit; soot over the rest.
                    b.Box(new Vector3(0f, 0.08f - i * 0.15f, 0.27f), new Vector3(0.06f, 0.04f, 0.02f), i == 3 ? EnemySwatch.Rune : EnemySwatch.Shadow);
                }

                b.Blob(new Vector3(-0.1f, 0.16f, 0.16f), new Vector3(0.24f, 0.24f, 0.14f), EnemySwatch.Shadow, coarse: true);
                // The seed basket on its back, empty.
                b.Cylinder(new Vector3(0f, 0f, -0.32f), 0.18f, 0.22f, 0.34f, 7, EnemySwatch.Bark, bottomCap: true);
            }, b =>
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0f, 0.12f - i * 0.08f, -0.2f), new Vector3(0.36f, 0.02f, 0.04f), EnemySwatch.Shadow);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.02f, 0.13f, 0.045f, EnemySwatch.Rune);
            }, (b, s) => DrawHoe(b, s, EnemySwatch.Iron));
        }

        // ------------------------------------------------------------------
        // Sombra Fervida
        // ------------------------------------------------------------------

        public static void BuildBoiledShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawBoiledShadowBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.08f, 0.36f), b =>
            {
                // The coal it carried out of the lake, still boiling it from inside; it flares before it spits.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.18f, 0.1f), EnemySwatch.Ember);
                b.Blob(new Vector3(0.02f, 0.03f, 0.03f), new Vector3(0.08f, 0.07f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawBoiledShadowBody(EnemyMeshBuilder b)
        {
            // Where the Lake of Fire laps the shore, the shadows that fall in come out boiled: soft, hot and in a very bad mood.
            b.Blob(new Vector3(0f, -0.38f, 0f), new Vector3(1.12f, 0.12f, 1.04f), EnemySwatch.Black, coarse: true);
            b.Blob(Vector3.zero, new Vector3(0.9f, 0.76f, 0.86f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0.16f, 0.24f, -0.1f), new Vector3(0.5f, 0.42f, 0.5f), EnemySwatch.BatFurDark, coarse: true);
            // The bubbles that rise and burst on it, and the steam going up.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 20f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.28f, 0.28f + (i % 3) * 0.04f, Mathf.Sin(a) * 0.26f), new Vector3(0.14f, 0.12f, 0.14f),
                    i % 2 == 0 ? EnemySwatch.Black : EnemySwatch.VampFur, coarse: true);
            }

            for (int i = 0; i < 3; i++)
            {
                b.Cylinder(new Vector3(-0.2f + i * 0.2f, 0.34f, -0.06f + i * 0.05f), 0.06f, 0.02f, 0.3f, 5, EnemySwatch.Shroud);
            }

            HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.16f, 0.38f, 0.07f, EnemySwatch.Ember);
        }

        // ------------------------------------------------------------------
        // Crocodilo de Lodo
        // ------------------------------------------------------------------

        public static void BuildMudCrocodile(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawMudCrocodileBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.82f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.42f, 0.27f, 0.4f), EnemySwatch.BarkDark);
                b.Box(new Vector3(0f, 0.01f, 0.32f), new Vector3(0.3f, 0.11f, 0.56f), EnemySwatch.BarkDark);
                // Grey mud dried on the snout, and the eyes over it like two lamps on a bank.
                b.Blob(new Vector3(0f, 0.08f, 0.24f), new Vector3(0.22f, 0.06f, 0.3f), EnemySwatch.WolfFurDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.14f, 0.14f, 0.06f, 0.075f, EnemySwatch.MoonEye);
                for (int i = 0; i < 4; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.13f * s, -0.04f, 0.14f + i * 0.13f), 0.015f, 0f, 0.05f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.24f), new Vector3(0.28f, 0.05f, 0.5f), EnemySwatch.BarkDark);
                b.Box(new Vector3(0f, 0.02f, 0.22f), new Vector3(0.2f, 0.012f, 0.42f), EnemySwatch.Pink);
                for (int i = 0; i < 4; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.12f * s, 0.01f, 0.08f + i * 0.12f), 0.015f, 0f, 0.05f, 3, EnemySwatch.Fang);
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.78f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.5f), 0.23f, 0.12f, 6, EnemySwatch.BarkDark);
                b.Segment(new Vector3(0f, -0.04f, -0.5f), new Vector3(0.04f, -0.1f, -0.9f), 0.12f, 0.02f, 5, EnemySwatch.WolfFurDark);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, 0.13f - i * 0.03f, -0.1f - i * 0.2f), new Vector3(0.035f, 0.1f - i * 0.015f, 0.12f), EnemySwatch.Bark);
                }
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.34f, 0.37f, 0.46f, -0.46f, 0.095f, EnemySwatch.BarkDark, EnemySwatch.WolfFurDark, "MudCrocodile");
        }

        private static void DrawMudCrocodileBody(EnemyMeshBuilder b)
        {
            // Apep drinks the river at the twelfth hour and leaves the barge on a bank of mud; the crocodiles of the bank lie in it so long
            // that they turn the colour of it, and the shadow found them there, waiting for whatever runs aground.
            b.Blob(Vector3.zero, new Vector3(0.88f, 0.5f, 1.74f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.74f, 0.26f, 1.52f), EnemySwatch.Bark, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                // The scutes under the dried mud, and the lumps of grey mud on top.
                float z = -0.6f + i * 0.24f;
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.14f * s, 0.24f, z), new Vector3(0.15f, 0.09f, 0.16f), EnemySwatch.Bark, Quaternion.Euler(0f, 0f, -14f * s));
                }

                b.Blob(new Vector3(i % 2 == 0 ? 0.2f : -0.18f, 0.27f, z + 0.1f), new Vector3(0.2f, 0.08f, 0.2f), EnemySwatch.WolfFurDark, coarse: true);
            }

            // Two reeds stuck in the mud on its back, and a dim light of the river caught between the scutes.
            b.Segment(new Vector3(0.1f, 0.26f, -0.2f), new Vector3(0.16f, 0.62f, -0.3f), 0.015f, 0.008f, 4, EnemySwatch.Lichen);
            b.Segment(new Vector3(-0.12f, 0.26f, 0.1f), new Vector3(-0.2f, 0.56f, 0.04f), 0.015f, 0.008f, 4, EnemySwatch.Lichen);
            b.Box(new Vector3(0f, 0.25f, 0.3f), new Vector3(0.03f, 0.02f, 0.12f), EnemySwatch.StolenLight);
            b.Segment(new Vector3(0f, 0f, 0.7f), new Vector3(0f, 0.02f, 0.82f), 0.25f, 0.21f, 6, EnemySwatch.BarkDark);
        }

        // ------------------------------------------------------------------
        // Ushabti Rachado
        // ------------------------------------------------------------------

        public static void BuildCrackedUshabti(ProceduralEnemyBlueprint bp)
        {
            // Red clay figures that were sent to carry water to the shore of the Lake of Fire. The heat cracked them, and the fire got in
            // through the cracks; they carry it now instead of the water.
            BuildFigure(bp, "CrackedUshabti", 1f, EnemySwatch.SandstoneDark, EnemySwatch.CopperDark, b =>
            {
                // The cracks down the front and the shoulders, the fire showing through.
                b.Box(new Vector3(0.06f, -0.12f, 0.26f), new Vector3(0.03f, 0.6f, 0.02f), EnemySwatch.Ember, Quaternion.Euler(0f, 0f, 12f));
                b.Box(new Vector3(-0.08f, 0.02f, 0.25f), new Vector3(0.025f, 0.34f, 0.02f), EnemySwatch.Ember, Quaternion.Euler(0f, 0f, -20f));
                b.Box(new Vector3(0.18f, 0.36f, 0.12f), new Vector3(0.02f, 0.16f, 0.12f), EnemySwatch.Ember, Quaternion.Euler(0f, 0f, 30f));
                b.Blob(new Vector3(0f, -0.3f, 0.2f), new Vector3(0.24f, 0.2f, 0.1f), EnemySwatch.Black, coarse: true);
                // The water jar on its back, dry and cracked too.
                b.Blob(new Vector3(0f, 0.06f, -0.3f), new Vector3(0.3f, 0.4f, 0.28f), EnemySwatch.Sandstone);
                b.Cylinder(new Vector3(0f, 0.24f, -0.3f), 0.07f, 0.09f, 0.1f, 6, EnemySwatch.SandstoneDark);
            }, b =>
            {
                b.Box(new Vector3(0.05f, 0.1f, 0.14f), new Vector3(0.02f, 0.14f, 0.02f), EnemySwatch.Ember, Quaternion.Euler(0f, 0f, 18f));
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.02f, 0.13f, 0.045f, EnemySwatch.Ember);
            }, (b, s) => DrawHoe(b, s, EnemySwatch.Copper));
        }

        // ------------------------------------------------------------------
        // Ushabti de Relógio
        // ------------------------------------------------------------------

        public static void BuildClockUshabti(ProceduralEnemyBlueprint bp)
        {
            // In the Desert of the Clock the hours run slow in one place and fast in the next, and the Mistura set figures to count them.
            // The shadow made them count wrong, and they keep everyone else's time wrong with theirs.
            BuildFigure(bp, "ClockUshabti", 1f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, b =>
            {
                // The sundial on its chest: a gold disc, twelve lit hour marks, and the shadow of the gnomon pointing at the wrong one.
                b.Cylinder(new Vector3(0f, 0.08f, 0.22f), 0.18f, 0.18f, 0.03f, 12, EnemySwatch.Gold, rotation: Quaternion.Euler(90f, 0f, 0f));
                for (int i = 0; i < 12; i++)
                {
                    float a = i * 30f * Mathf.Deg2Rad;
                    b.Box(new Vector3(Mathf.Cos(a) * 0.14f, 0.08f + Mathf.Sin(a) * 0.14f, 0.26f), new Vector3(0.025f, 0.025f, 0.015f), EnemySwatch.Rune);
                }

                b.Box(new Vector3(0.04f, 0.04f, 0.265f), new Vector3(0.02f, 0.12f, 0.015f), EnemySwatch.Shadow, Quaternion.Euler(0f, 0f, 35f));
                b.Box(new Vector3(0f, -0.3f, 0.25f), new Vector3(0.1f, 0.3f, 0.03f), EnemySwatch.SandstoneLight);
            }, b =>
            {
                b.Cylinder(new Vector3(0f, 0.17f, 0f), 0.08f, 0.04f, 0.08f, 6, EnemySwatch.Gold);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.02f, 0.13f, 0.045f, EnemySwatch.Rune);
            }, (b, s) =>
            {
                // The gnomon it carries like a staff, a long bronze needle with a gold tip.
                b.Segment(new Vector3(0.02f, -0.72f, 0.3f), new Vector3(0.02f, 0.18f, 0.34f), 0.025f, 0.018f, 5, EnemySwatch.Copper);
                b.Blob(new Vector3(0.02f, 0.2f, 0.34f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.Gold, coarse: true);
            });
        }

        // ------------------------------------------------------------------
        // Fumaça de Piche
        // ------------------------------------------------------------------

        public static void BuildPitchSmoke(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.86f, 0f), DrawPitchSmokeBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.06f, 0.36f), b =>
            {
                // A spark from the lamps of the wells caught in the smoke: the pitch around it could catch any moment, and it knows.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.16f, 0.1f), EnemySwatch.Ember);
                b.Blob(new Vector3(0.02f, 0.02f, 0.03f), new Vector3(0.07f, 0.07f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawPitchSmokeBody(EnemyMeshBuilder b)
        {
            // The bitumen wells breathe a black smoke all day. Most of it goes up; the part the shadow got into stays low and heavy,
            // dripping pitch, and sticks to whoever walks through.
            b.Blob(Vector3.zero, new Vector3(0.8f, 0.6f, 0.72f), EnemySwatch.Black);
            b.Blob(new Vector3(0.24f, 0.22f, -0.06f), new Vector3(0.5f, 0.42f, 0.46f), EnemySwatch.Shadow, coarse: true);
            b.Blob(new Vector3(-0.26f, 0.14f, 0.04f), new Vector3(0.46f, 0.4f, 0.44f), EnemySwatch.BatFurDark, coarse: true);
            b.Blob(new Vector3(0.02f, 0.4f, 0.02f), new Vector3(0.4f, 0.3f, 0.36f), EnemySwatch.Shadow, coarse: true);
            // The pitch dripping off its underside down to the ground, in long strings with a drop at the end.
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 10f) * Mathf.Deg2Rad;
                Vector3 top = new Vector3(Mathf.Cos(a) * 0.24f, -0.22f, Mathf.Sin(a) * 0.22f);
                Vector3 end = top + new Vector3(0f, -0.52f - (i % 2) * 0.06f, 0f);
                b.Segment(top, end, 0.05f, 0.02f, 4, EnemySwatch.Black);
                b.Blob(end, new Vector3(0.06f, 0.08f, 0.06f), EnemySwatch.IronDark, coarse: true);
            }

            HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.12f, 0.33f, 0.06f, EnemySwatch.Ember);
        }

        // ------------------------------------------------------------------
        // Fumaça Sufocante
        // ------------------------------------------------------------------

        public static void BuildChokingSmoke(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawChokingSmokeBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.18f, 0.32f), b =>
            {
                // The breath it took from a lamp at the fortress: a little light it squeezes until it is almost out.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.16f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0f, 0.03f), new Vector3(0.06f, 0.06f, 0.04f), EnemySwatch.MoonEye, coarse: true);
            });
        }

        private static void DrawChokingSmokeBody(EnemyMeshBuilder b)
        {
            // Smoke is the first of the five elements of the Darkness, and the Pit of Smoke is full of it. This one is thick enough to hold
            // a shape, and it wraps round whoever comes near and squeezes, as smoke does a lamp.
            for (int i = 0; i < 5; i++)
            {
                float y = -0.58f + i * 0.27f;
                float w = 0.78f - Mathf.Abs(i - 2) * 0.12f;
                b.Blob(new Vector3((i % 2 == 0 ? 0.05f : -0.06f), y, 0f), new Vector3(w, 0.36f, w * 0.92f), i % 2 == 0 ? EnemySwatch.WolfFurDark : EnemySwatch.Shroud,
                    coarse: i != 2);
            }

            // The coils that reach out to squeeze, and the soot-dark ring of its throat.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.3f * s, 0.12f, 0.06f), new Vector3(0.5f * s, -0.06f, 0.26f), 0.1f, 0.04f, 5, EnemySwatch.Shroud);
            }

            b.Cylinder(new Vector3(0f, 0.3f, 0f), 0.24f, 0.26f, 0.06f, 8, EnemySwatch.Shadow);
            HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.36f, 0.28f, 0.06f, EnemySwatch.SoftEye);
        }

        // ------------------------------------------------------------------
        // Cardume Escuro
        // ------------------------------------------------------------------

        public static void BuildDarkShoal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.66f, 0f), DrawDarkShoalBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0f), b =>
            {
                // The light they swallowed between them, passed from mouth to mouth so none of them has to hold it long; it flares before
                // they spit a bead of it.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.StolenLight);
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.28f, 0.28f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawDarkShoalBody(EnemyMeshBuilder b)
        {
            // In the Pit of Mire the fish swim in black water and never saw a light until the shadow brought one down; now they turn round
            // it all together, and round whoever brought another.
            for (int i = 0; i < 7; i++)
            {
                float a = i * (360f / 7f) * Mathf.Deg2Rad;
                Vector3 at = new Vector3(Mathf.Cos(a) * 0.36f, -0.18f + (i % 3) * 0.17f, Mathf.Sin(a) * 0.34f);
                // Each fish swims along the ring: nose on the tangent, the tail fin behind, one pale eye in front.
                Quaternion heading = Quaternion.LookRotation(new Vector3(-Mathf.Sin(a), 0f, Mathf.Cos(a)));
                b.Blob(at, new Vector3(0.1f, 0.09f, 0.24f), i % 2 == 0 ? EnemySwatch.CaveSkinDark : EnemySwatch.Black, heading);
                b.Box(at + heading * new Vector3(0f, 0f, -0.15f), new Vector3(0.02f, 0.08f, 0.07f), EnemySwatch.Shadow, heading);
                b.Blob(at + heading * new Vector3(0f, 0.02f, 0.1f), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.SoftEye, coarse: true);
            }

            // The mud they stir up off the bottom, hanging under them, down to the ground.
            b.Blob(new Vector3(0f, -0.46f, 0f), new Vector3(0.7f, 0.2f, 0.66f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0.06f, -0.6f, -0.04f), new Vector3(0.84f, 0.1f, 0.8f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(0f, 0.36f, 0f), new Vector3(0.3f, 0.12f, 0.3f), EnemySwatch.CaveSkinDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Serpente Cega
        // ------------------------------------------------------------------

        private static readonly Vector3[] BlindSerpentSteps =
        {
            new Vector3(-0.36f, 0f, -0.2f), new Vector3(-0.04f, 0f, -0.36f), new Vector3(0.46f, 0f, -0.16f), new Vector3(0.18f, 0f, -0.3f),
        };

        private static readonly float[] BlindSerpentRadii = { 0.13f, 0.115f, 0.1f, 0.075f, 0.02f };

        public static void BuildBlindSerpent(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.13f, 0.3f), b =>
            {
                // In the Pit of Darkness there is nothing to see, so the serpents stopped making eyes. They follow the warmth of a body,
                // and the shadow taught them that a hero is the warmest thing in the pit.
                b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.3f, 0.26f, 0.32f), EnemySwatch.CaveBelly);
                b.Segment(new Vector3(0f, 0.02f, 0f), new Vector3(0f, 0.3f, 0.07f), 0.13f, 0.1f, 6, EnemySwatch.CaveBelly);
                b.Segment(new Vector3(0f, 0.3f, 0.07f), new Vector3(0f, 0.54f, 0.14f), 0.1f, 0.08f, 6, EnemySwatch.CaveBelly);
                b.Blob(new Vector3(0f, 0.26f, 0.13f), new Vector3(0.12f, 0.32f, 0.06f), EnemySwatch.Bone, coarse: true);
                for (int k = 0; k < 3; k++)
                {
                    b.Blob(new Vector3(0f, 0.1f + k * 0.15f, -0.08f + k * 0.03f), new Vector3(0.12f, 0.08f, 0.05f), EnemySwatch.Shadow, coarse: true);
                }
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.56f, 0.16f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.07f), new Vector3(0.19f, 0.11f, 0.26f), EnemySwatch.CaveBelly);
                // Smooth scales where the eyes should be, and the two warm pits under them that glow when it feels you.
                b.Blob(new Vector3(0f, 0.05f, 0.05f), new Vector3(0.14f, 0.05f, 0.2f), EnemySwatch.Bone, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.055f, 0.0f, 0.17f, 0.025f, EnemySwatch.Sorcery);
                b.Box(new Vector3(0f, -0.01f, 0.24f), new Vector3(0.01f, 0.005f, 0.07f), EnemySwatch.Pink);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.03f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.13f, 0.02f, 0.15f), EnemySwatch.CaveSkin);
                b.Cylinder(new Vector3(0.04f, 0.005f, 0.12f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.005f, 0.12f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });

            int parent = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.06f), b => DrawBlindSerpentSegment(b, 0));
            for (int i = 1; i < OverworldBeastRecipes.SnakeTailSegments; i++)
            {
                int index = i;
                parent = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + i, parent, BlindSerpentSteps[i - 1], b => DrawBlindSerpentSegment(b, index));
            }
        }

        private static void DrawBlindSerpentSegment(EnemyMeshBuilder b, int index)
        {
            Vector3 end = BlindSerpentSteps[index];
            float r0 = BlindSerpentRadii[index];
            float r1 = BlindSerpentRadii[index + 1];
            b.Segment(Vector3.zero, end, r0, r1, 6, EnemySwatch.CaveBelly);
            b.Blob(Vector3.zero, Vector3.one * (r0 * 2f), EnemySwatch.CaveBelly, coarse: true);
            // The dark bands of the pit's soot across the pale back.
            b.Blob(end * 0.5f + new Vector3(0f, (r0 + r1) * 0.42f, 0f), new Vector3(0.1f, 0.04f, 0.12f), EnemySwatch.Shadow, coarse: true);
            if (index == OverworldBeastRecipes.SnakeTailSegments - 1)
            {
                b.Segment(end, end + end.normalized * 0.12f, r1, 0.004f, 4, EnemySwatch.Bone);
            }
        }

        // ------------------------------------------------------------------
        // Ushabti Apagado Gigante
        // ------------------------------------------------------------------

        public static void BuildGiantDimmedUshabti(ProceduralEnemyBlueprint bp)
        {
            // The Artificers set the biggest of the answering figures before the Gate of the Land of Darkness, to keep it shut when called.
            // The shadow on the other side called louder.
            const float s = 1.7f;
            BuildFigure(bp, "GiantDimmedUshabti", s, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark, b =>
            {
                b.Box(new Vector3(0f, -0.14f * s, 0.25f * s), new Vector3(0.12f * s, 0.62f * s, 0.03f * s), EnemySwatch.Black);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, (0.08f - i * 0.15f) * s, 0.27f * s), new Vector3(0.07f * s, 0.04f * s, 0.02f * s), EnemySwatch.Shadow);
                }

                // The seams of shadow down its sides, and the chain of the Gate across its chest.
                for (int side = -1; side <= 1; side += 2)
                {
                    b.Box(new Vector3(0.2f * s * side, -0.1f * s, 0.12f * s), new Vector3(0.02f * s, 0.5f * s, 0.02f * s), EnemySwatch.Shadow, Quaternion.Euler(0f, 0f, 8f * side));
                }

                for (int i = 0; i < 5; i++)
                {
                    b.Blob(new Vector3((-0.24f + i * 0.12f) * s, (0.16f + (i % 2) * 0.03f) * s, 0.2f * s), new Vector3(0.08f * s, 0.06f * s, 0.05f * s), EnemySwatch.Iron, coarse: true);
                }
            }, b =>
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0f, (0.12f - i * 0.08f) * s, -0.2f * s), new Vector3(0.36f * s, 0.02f * s, 0.04f * s), EnemySwatch.Shadow);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.06f * s, 0.02f * s, 0.13f * s, 0.045f * s, EnemySwatch.StolenLight);
            }, (b, sc) => DrawHoe(b, sc, EnemySwatch.IronDark));
        }
    }
}
