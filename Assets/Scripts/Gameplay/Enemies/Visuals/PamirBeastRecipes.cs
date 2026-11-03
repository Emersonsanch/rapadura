using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Teto do Mundo (Docs/Mapas100.md 36–40) that the continuous world played with stand-ins, low-poly
    /// from the shared palette like <see cref="OverworldBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Urso-Pardo do Pamir Apagado</b> (~1.15 m at the hump): brown bear, round ears, a jaw that gapes.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Ear_L</c>/<c>Ear_R</c>; four thick legs with pads.</item>
    /// <item><b>Iaque Selvagem Apagado</b> (~1.2 m): black shaggy yak, a skirt of long hair, snow on the hump, wide horns
    /// curving up; lowers its head to charge. <c>Body</c> → <c>Head</c>, <c>Tail</c>; four legs with hooves.</item>
    /// <item><b>Irbis Apagado</b> (~0.75 m): pale snow leopard with dark rosettes and a long thick tail.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c> → <c>Tail_1</c> → <c>Tail_2</c>; four jointed legs.</item>
    /// <item><b>Almas</b> (~1.8 m): stooped hairy wild man of the glacier with big feet and a snowball in the right hand.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>, <c>Arm_R</c> → <c>Weapon</c> (the snowball); <c>Leg_L</c>/<c>Leg_R</c> →
    /// <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Marmota Gigante</b> (~0.65 m): chubby golden marmot with a dark mask, big incisors and a bushy tail.
    /// <c>Body</c> → <c>Head</c>, <c>Tail</c> → <c>Tail_1</c>; four short legs.</item>
    /// </list>
    /// </summary>
    public static class PamirBeastRecipes
    {
        // ------------------------------------------------------------------
        // Urso-Pardo do Pamir Apagado
        // ------------------------------------------------------------------

        public static void BuildBrownBear(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.68f, 0f), DrawBearBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.64f), DrawBearHead, Quaternion.Euler(12f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.11f, 0.08f), b => CoastDeltaBeastRecipes.DrawFelineJaw(b, EnemySwatch.WolfBelly, 1.25f));
            for (int side = -1; side <= 1; side += 2)
            {
                bp.Mesh(side < 0 ? ProceduralEnemyParts.EarLeft : ProceduralEnemyParts.EarRight, head, new Vector3(0.12f * side, 0.15f, -0.02f),
                    b => b.Blob(Vector3.zero, new Vector3(0.09f, 0.09f, 0.05f), EnemySwatch.WolfFurDark, coarse: true), meshKey: "BearEar");
            }

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.2f * side, 0.56f, 0.4f), new Vector3(0f, -0.27f, 0.02f), new Vector3(0f, -0.26f, 0.02f),
                    0.11f, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, new Vector3(0.16f, 0.08f, 0.2f), "BearFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.21f * side, 0.58f, -0.4f), new Vector3(0f, -0.28f, -0.05f), new Vector3(0f, -0.27f, 0.05f),
                    0.12f, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, new Vector3(0.17f, 0.08f, 0.22f), "BearHind");
            }
        }

        private static void DrawBearBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.72f, 0.7f, 1.2f), EnemySwatch.WolfFur);
            // The shoulder hump of the brown bear, the darker belly, the frost caught in the fur of the back.
            b.Blob(new Vector3(0f, 0.26f, 0.3f), new Vector3(0.56f, 0.4f, 0.46f), EnemySwatch.WolfFurDark, coarse: true);
            b.Blob(new Vector3(0f, -0.14f, 0f), new Vector3(0.56f, 0.36f, 0.9f), EnemySwatch.WolfFurDark, coarse: true);
            b.Blob(new Vector3(0.04f, 0.3f, -0.12f), new Vector3(0.34f, 0.1f, 0.4f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(0f, 0.04f, -0.6f), new Vector3(0.1f, 0.1f, 0.08f), EnemySwatch.WolfFurDark, coarse: true);
        }

        private static void DrawBearHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.32f, 0.34f), EnemySwatch.WolfFur);
            b.Cylinder(new Vector3(0f, -0.05f, 0.1f), 0.09f, 0.07f, 0.16f, 6, EnemySwatch.WolfBelly, EnemySwatch.WolfBelly, Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.04f, 0.27f), new Vector3(0.07f, 0.05f, 0.04f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, 0.05f, 0.14f), new Vector3(0.04f, 0.03f, 0.03f), EnemySwatch.MoonEye, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Iaque Selvagem Apagado
        // ------------------------------------------------------------------

        public static void BuildWildYak(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.82f, 0f), DrawYakBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, -0.06f, 0.72f), DrawYakHead, Quaternion.Euler(28f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.64f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.3f, -0.08f), 0.04f, 0.03f, 5, EnemySwatch.Black);
                b.Blob(new Vector3(0f, -0.32f, -0.08f), new Vector3(0.1f, 0.16f, 0.1f), EnemySwatch.Black, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.22f * side, 0.6f, 0.42f), new Vector3(0f, -0.29f, 0.02f), new Vector3(0f, -0.28f, 0f),
                    0.09f, EnemySwatch.PeccaryFur, EnemySwatch.Black, new Vector3(0.13f, 0.08f, 0.15f), "YakFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.22f * side, 0.62f, -0.42f), new Vector3(0f, -0.3f, -0.04f), new Vector3(0f, -0.29f, 0.04f),
                    0.095f, EnemySwatch.PeccaryFur, EnemySwatch.Black, new Vector3(0.13f, 0.08f, 0.15f), "YakHind");
            }
        }

        private static void DrawYakBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.76f, 0.72f, 1.3f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, 0.3f, 0.32f), new Vector3(0.6f, 0.44f, 0.5f), EnemySwatch.PeccaryFur, coarse: true);
            b.Blob(new Vector3(0f, 0.34f, 0.12f), new Vector3(0.48f, 0.12f, 0.6f), EnemySwatch.GolemSnow, coarse: true);
            // The skirt of long hair hanging from the flanks and the belly almost to the snow.
            b.Blob(new Vector3(0f, -0.36f, 0f), new Vector3(0.5f, 0.3f, 0.9f), EnemySwatch.Black, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.32f * s, -0.3f, 0.44f - 0.22f * i), new Vector3(0.18f, 0.42f, 0.22f), EnemySwatch.Black, coarse: true);
                }
            }
        }

        private static void DrawYakHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, 0.1f), new Vector3(0.32f, 0.32f, 0.42f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, -0.06f, 0.3f), new Vector3(0.24f, 0.2f, 0.18f), EnemySwatch.GolemStone, coarse: true);
            b.Blob(new Vector3(0f, 0.12f, 0.06f), new Vector3(0.3f, 0.16f, 0.2f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // Wide horns out to the sides, curving up (bent round the forward axis), pale at the tips.
                b.Horn(new Vector3(0.13f * s, 0.12f, 0.02f), 0.05f, 0.4f, 4, 25f * s, EnemySwatch.Horn, Quaternion.Euler(0f, 0f, -70f * s), 5, Vector3.forward);
                b.Blob(new Vector3(0.13f * s, 0.06f, 0.2f), new Vector3(0.04f, 0.03f, 0.03f), EnemySwatch.SoftEye, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Irbis Apagado
        // ------------------------------------------------------------------

        public static void BuildIrbis(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawIrbisBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.5f), DrawIrbisHead, Quaternion.Euler(6f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.06f), b => CoastDeltaBeastRecipes.DrawFelineJaw(b, EnemySwatch.GolemSnow, 0.85f));

            // The long thick tail it balances with on the cliffs: down, along, and curling up at the tip.
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.46f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.18f, -0.26f), 0.055f, 0.055f, 6, EnemySwatch.Steel));
            int tail1 = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, -0.18f, -0.26f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.3f), 0.055f, 0.055f, 6, EnemySwatch.Steel);
                b.Blob(new Vector3(0f, 0f, -0.15f), new Vector3(0.12f, 0.08f, 0.08f), EnemySwatch.Shadow, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "2", tail1, new Vector3(0f, -0.04f, -0.3f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.12f, -0.14f), 0.055f, 0.05f, 6, EnemySwatch.Steel);
                b.Blob(new Vector3(0f, 0.12f, -0.14f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.Shadow, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.11f * side, 0.46f, 0.3f), new Vector3(0f, -0.22f, -0.02f), new Vector3(0f, -0.21f, 0.04f),
                    0.055f, EnemySwatch.Steel, EnemySwatch.GolemSnow, "IrbisFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.11f * side, 0.48f, -0.3f), new Vector3(0f, -0.23f, -0.07f), new Vector3(0f, -0.22f, 0.07f),
                    0.06f, EnemySwatch.Steel, EnemySwatch.GolemSnow, "IrbisHind");
            }
        }

        /// <summary>Rosettes on the snow leopard's back and flanks (fixed, so the model stays deterministic).</summary>
        private static readonly Vector3[] Rosettes =
        {
            new Vector3(0.08f, 0.16f, 0.28f), new Vector3(-0.1f, 0.15f, 0.18f), new Vector3(0.12f, 0.12f, 0.02f), new Vector3(-0.06f, 0.17f, -0.06f),
            new Vector3(0.1f, 0.14f, -0.22f), new Vector3(-0.12f, 0.12f, -0.3f), new Vector3(0.17f, 0.02f, 0.14f), new Vector3(-0.17f, 0.03f, -0.12f),
        };

        private static void DrawIrbisBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.36f, 0.92f), EnemySwatch.Steel);
            b.Blob(new Vector3(0f, -0.07f, 0f), new Vector3(0.28f, 0.2f, 0.74f), EnemySwatch.GolemSnow, coarse: true);
            foreach (Vector3 spot in Rosettes)
            {
                b.Blob(spot, new Vector3(0.07f, 0.05f, 0.07f), EnemySwatch.Shadow, coarse: true);
            }
        }

        private static void DrawIrbisHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), EnemySwatch.Steel);
            b.Blob(new Vector3(0f, -0.04f, 0.1f), new Vector3(0.12f, 0.09f, 0.1f), EnemySwatch.GolemSnow, coarse: true);
            b.Box(new Vector3(0f, -0.01f, 0.15f), new Vector3(0.04f, 0.025f, 0.02f), EnemySwatch.Pink);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.055f * s, 0.035f, 0.09f), new Vector3(0.035f, 0.025f, 0.02f), EnemySwatch.SoftEye, coarse: true);
                b.Cylinder(new Vector3(0.07f * s, 0.08f, -0.01f), 0.035f, 0f, 0.06f, 4, EnemySwatch.Shadow, rotation: Quaternion.Euler(-10f, 0f, -15f * s));
                b.Blob(new Vector3(0.08f * s, 0.05f, 0.02f), new Vector3(0.035f, 0.03f, 0.03f), EnemySwatch.Shadow, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Almas
        // ------------------------------------------------------------------

        public static void BuildAlmas(ProceduralEnemyBlueprint bp)
        {
            // Stooped forward: the torso leans out over the big feet.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.95f, 0f), DrawAlmasBody, Quaternion.Euler(14f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.66f, 0.1f), DrawAlmasHead, Quaternion.Euler(-10f, 0f, 0f));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.36f * side, 0.42f, 0.02f),
                    b => DrawAlmasArm(b, side), Quaternion.Euler(-10f, 0f, 0f));
                if (right)
                {
                    // The snowball it throws when it is too shy to come close.
                    bp.Mesh(ProceduralEnemyParts.Weapon, arm, new Vector3(0.05f, -0.8f, 0.16f), b =>
                    {
                        b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.GolemSnow);
                        b.Blob(new Vector3(0.03f, 0.04f, 0.06f), new Vector3(0.05f, 0.05f, 0.04f), EnemySwatch.Crystal, coarse: true);
                    });
                }

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1, new Vector3(0.17f * side, 0.58f, 0f),
                    b => b.Segment(new Vector3(0f, -0.28f, 0.05f), new Vector3(0f, 0.04f, 0f), 0.09f, 0.11f, 6, EnemySwatch.WolfFur), meshKey: "AlmasThigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg, new Vector3(0f, -0.28f, 0.05f),
                    b => b.Segment(new Vector3(0f, -0.27f, -0.02f), Vector3.zero, 0.07f, 0.08f, 6, EnemySwatch.WolfFur), meshKey: "AlmasShin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -0.27f, -0.02f),
                    b => b.Blob(new Vector3(0f, -0.005f, 0.07f), new Vector3(0.14f, 0.07f, 0.26f), EnemySwatch.SandstoneDark, coarse: true), meshKey: "AlmasFoot");
            }
        }

        private static void DrawAlmasBody(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.25f, 0f), new Vector3(0.62f, 0.72f, 0.46f), EnemySwatch.WolfFur);
            // Shaggy red-brown hair in tufts, a paler chest, snow stuck on the shoulders.
            b.Blob(new Vector3(0f, 0.36f, -0.08f), new Vector3(0.56f, 0.44f, 0.36f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(0f, 0.2f, 0.14f), new Vector3(0.34f, 0.4f, 0.22f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, -0.08f, 0f), new Vector3(0.5f, 0.3f, 0.38f), EnemySwatch.WolfFurDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.24f * s, 0.54f, 0f), new Vector3(0.2f, 0.08f, 0.2f), EnemySwatch.GolemSnow, coarse: true);
            }
        }

        private static void DrawAlmasHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.3f, 0.32f, 0.3f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, 0.05f, 0.11f), new Vector3(0.18f, 0.2f, 0.1f), EnemySwatch.SandstoneDark, coarse: true);
            b.Box(new Vector3(0f, 0.12f, 0.15f), new Vector3(0.2f, 0.04f, 0.06f), EnemySwatch.WolfFurDark, Quaternion.Euler(-10f, 0f, 0f));
            b.Blob(new Vector3(0f, 0.2f, -0.02f), new Vector3(0.28f, 0.14f, 0.26f), EnemySwatch.Bark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // Shy, pale eyes under the brow.
                b.Blob(new Vector3(0.05f * s, 0.08f, 0.15f), new Vector3(0.035f, 0.03f, 0.02f), EnemySwatch.SoftEye, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.02f, 0.165f), new Vector3(0.05f, 0.04f, 0.03f), EnemySwatch.SandstoneDark, coarse: true);
        }

        private static void DrawAlmasArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.WolfFur, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.04f * side, -0.36f, 0.06f), 0.08f, 0.07f, 6, EnemySwatch.WolfFur);
            b.Segment(new Vector3(0.04f * side, -0.36f, 0.06f), new Vector3(0.05f * side, -0.66f, 0.1f), 0.07f, 0.06f, 6, EnemySwatch.Bark);
            b.Blob(new Vector3(0.05f * side, -0.74f, 0.12f), new Vector3(0.13f, 0.13f, 0.13f), EnemySwatch.SandstoneDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Marmota Gigante
        // ------------------------------------------------------------------

        public static void BuildGiantMarmot(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.32f, 0f), DrawMarmotBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.34f), DrawMarmotHead);
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.34f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.18f), 0.05f, 0.06f, 5, EnemySwatch.Bark));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, -0.06f, -0.18f), b =>
                b.Blob(new Vector3(0f, -0.02f, -0.06f), new Vector3(0.11f, 0.1f, 0.16f), EnemySwatch.BarkDark, coarse: true));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.14f * side, 0.24f, 0.22f), new Vector3(0f, -0.11f, 0.02f), new Vector3(0f, -0.1f, 0.02f),
                    0.05f, EnemySwatch.OwlBuff, EnemySwatch.BarkDark, "MarmotFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.15f * side, 0.26f, -0.22f), new Vector3(0f, -0.12f, -0.04f), new Vector3(0f, -0.11f, 0.04f),
                    0.055f, EnemySwatch.OwlBuff, EnemySwatch.BarkDark, "MarmotHind");
            }
        }

        private static void DrawMarmotBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.46f, 0.42f, 0.72f), EnemySwatch.OwlBuff);
            b.Blob(new Vector3(0f, 0.08f, -0.04f), new Vector3(0.4f, 0.26f, 0.6f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(0f, -0.08f, 0.06f), new Vector3(0.34f, 0.2f, 0.5f), EnemySwatch.WolfBelly, coarse: true);
        }

        private static void DrawMarmotHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.24f, 0.22f, 0.22f), EnemySwatch.OwlBuff);
            b.Blob(new Vector3(0f, 0.04f, 0.08f), new Vector3(0.17f, 0.08f, 0.08f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0f, -0.04f, 0.09f), new Vector3(0.14f, 0.09f, 0.08f), EnemySwatch.WolfBelly, coarse: true);
            b.Box(new Vector3(0f, -0.01f, 0.13f), new Vector3(0.04f, 0.025f, 0.02f), EnemySwatch.Pink);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.09f), new Vector3(0.03f, 0.025f, 0.02f), EnemySwatch.Ember, coarse: true);
                b.Blob(new Vector3(0.07f * s, 0.1f, -0.02f), new Vector3(0.05f, 0.05f, 0.03f), EnemySwatch.BarkDark, coarse: true);
                // The two big orange-white incisors it gnaws the caravan bags with.
                b.Box(new Vector3(0.012f * s, -0.075f, 0.11f), new Vector3(0.02f, 0.05f, 0.012f), EnemySwatch.Fang);
            }
        }
    }
}
