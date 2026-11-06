using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Picos de Farnkesh (Docs/Mapas100.md 53–58) that the continuous world played with stand-ins,
    /// low-poly from the shared palette like <see cref="NartesBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Bode de Fumaça</b> (Vale, ~1 m): goat whose fleece is grey smoke with embers in it, black curled horns; lowers its
    /// head to butt. <c>Body</c> → <c>Head</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Abelha Dourada</b> (Vale, ~0.75 m): a bee the size of a dog that Midas's touch turned to gold; buzzing wings,
    /// the sting swings under to strike. <c>Body</c> → <c>Wing_L</c>/<c>Wing_R</c>, <c>Stinger</c>.</item>
    /// <item><b>Pássaro-Flauta Apagado</b> (Vale, ~0.75 m): long-tailed songbird whose beak is a reed pipe; [distância].
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Raposa da Vinha</b> (Vale, ~0.75 m): red fox with a purple-stained muzzle, vine leaves and grapes in the brush.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Gralha-Alpina</b> (Picos, ~0.7 m): black chough, yellow beak, red legs. <c>Body</c> → <c>Head</c> → <c>Jaw</c>,
    /// <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Takin Apagado</b> (Picos, ~1.3 m): golden takin with the heavy Roman nose and the swept-back horns, greyed by the
    /// shadow; lowers its head to charge. <c>Body</c> → <c>Head</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Cabra-Azul</b> (Picos, ~0.9 m): the bharal, slate-blue with the black flank stripe and horns curving out.
    /// <c>Body</c> → <c>Head</c>, <c>Tail</c>; four thin legs.</item>
    /// <item><b>Morcego-Vela</b> (Pontes, ~1 m): bat with a lit candle stuck in the wax on its head. <c>Body</c> →
    /// <c>Lantern</c> (the flame), <c>Wing_L</c>/<c>Wing_R</c>.</item>
    /// <item><b>Ushabti de Corda</b> (Pontes, ~1.3 m): a servant figure of knotted rope and clay, hoe and basket; both arms go up
    /// to strike. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>, <c>Arm_R</c>; <c>Leg_L</c>/<c>Leg_R</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Gato-de-Pallas Apagado</b> (Ermidas, ~0.7 m): stocky long-furred wild cat, flat ears, ringed tail.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four short legs.</item>
    /// <item><b>Águia da Tempestade</b> (Garganta, ~1 m, 1.6 m across): dark grey eagle with lightning in its wing tips; [distância].
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Harpia de Fumaça</b> (Ninho, ~1.1 m): big smoky bird of prey with a pale masked face and long talons.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// </list>
    /// </summary>
    public static class FarnkeshBeastRecipes
    {
        // ------------------------------------------------------------------
        // Bode de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeGoat(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawSmokeGoatBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.46f), DrawSmokeGoatHead, Quaternion.Euler(10f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.42f), b =>
                b.Cylinder(Vector3.zero, 0.04f, 0f, 0.12f, 4, EnemySwatch.GolemStoneDark, rotation: Quaternion.Euler(-140f, 0f, 0f)));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.13f * side, 0.54f, 0.28f), new Vector3(0f, -0.27f, -0.02f), new Vector3(0f, -0.25f, 0.04f),
                    0.055f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, new Vector3(0.08f, 0.07f, 0.1f), "SmokeGoatFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.13f * side, 0.56f, -0.28f), new Vector3(0f, -0.28f, -0.06f), new Vector3(0f, -0.26f, 0.06f),
                    0.06f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, new Vector3(0.08f, 0.07f, 0.1f), "SmokeGoatHind");
            }
        }

        private static void DrawSmokeGoatBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.46f, 0.84f), EnemySwatch.GolemStoneDark);
            // The fleece is smoke: grey puffs rolling off the back, and embers still glowing in it.
            for (int i = 0; i < 6; i++)
            {
                float z = -0.3f + i * 0.12f;
                float x = (i % 2 == 0 ? 0.1f : -0.1f);
                b.Blob(new Vector3(x, 0.2f + (i % 3) * 0.03f, z), new Vector3(0.26f, 0.2f, 0.22f), i % 2 == 0 ? EnemySwatch.GolemStone : EnemySwatch.Shadow, coarse: true);
            }

            b.Blob(new Vector3(0.12f, 0.24f, -0.12f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.1f, 0.22f, 0.14f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            b.Segment(new Vector3(0f, 0.06f, 0.3f), new Vector3(0f, 0.16f, 0.46f), 0.11f, 0.08f, 5, EnemySwatch.GolemStoneDark);
        }

        private static void DrawSmokeGoatHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.2f, 0.26f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.04f, 0.12f), new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.PeccaryFur, coarse: true);
            // The goat's beard, a wisp of smoke.
            b.Cylinder(new Vector3(0f, -0.08f, 0.1f), 0.04f, 0f, 0.14f, 4, EnemySwatch.GolemStone, rotation: Quaternion.Euler(170f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.06f), new Vector3(0.035f, 0.03f, 0.025f), EnemySwatch.Ember, coarse: true);
                b.Cylinder(new Vector3(0.08f * s, 0.03f, -0.04f), 0.03f, 0f, 0.09f, 3, EnemySwatch.BarkDark, rotation: Quaternion.Euler(0f, 0f, -80f * s));
                // Black horns curling back over the neck.
                b.Horn(new Vector3(0.04f * s, 0.09f, -0.02f), 0.03f, 0.3f, 5, 28f, EnemySwatch.Black, Quaternion.Euler(-30f, 0f, -18f * s), 5, Vector3.right);
            }
        }

        // ------------------------------------------------------------------
        // Abelha Dourada
        // ------------------------------------------------------------------

        public static void BuildGoldenBee(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawBeeBody);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.08f * side, 0.12f, 0.02f),
                    b => DrawBeeWing(b, side), Quaternion.Euler(0f, 0f, 14f * side));
            }

            bp.Mesh(ProceduralEnemyParts.Stinger, body, new Vector3(0f, -0.06f, -0.44f), b =>
            {
                b.Cylinder(Vector3.zero, 0.05f, 0f, 0.16f, 5, EnemySwatch.Black, rotation: Quaternion.Euler(-110f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.05f, -0.13f), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawBeeBody(EnemyMeshBuilder b)
        {
            // Thorax, the golden abdomen with its dark bands, and the head with its big eyes.
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.26f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.22f, 0.22f, 0.2f), EnemySwatch.SandstoneDark, coarse: true);
            b.Blob(new Vector3(0f, -0.02f, -0.26f), new Vector3(0.3f, 0.28f, 0.42f), EnemySwatch.Gold);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(0f, -0.02f, -0.16f - i * 0.1f), new Vector3(0.31f - i * 0.03f, 0.29f - i * 0.03f, 0.05f), EnemySwatch.BarkDark, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.02f, 0.18f), new Vector3(0.18f, 0.18f, 0.14f), EnemySwatch.BarkDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.04f, 0.2f), new Vector3(0.07f, 0.1f, 0.07f), EnemySwatch.MoonEye, coarse: true);
                b.Segment(new Vector3(0.03f * s, 0.09f, 0.22f), new Vector3(0.09f * s, 0.22f, 0.32f), 0.012f, 0.008f, 3, EnemySwatch.BarkDark);
                // Three pairs of legs hanging under the hover: the lowest point of the model.
                for (int k = 0; k < 3; k++)
                {
                    float z = 0.06f - k * 0.08f;
                    b.Segment(new Vector3(0.06f * s, -0.08f, z), new Vector3(0.1f * s, -0.3f, z - 0.02f), 0.02f, 0.015f, 4, EnemySwatch.BarkDark);
                    b.Segment(new Vector3(0.1f * s, -0.3f, z - 0.02f), new Vector3(0.09f * s, -0.5f, z - 0.05f), 0.015f, 0.008f, 3, EnemySwatch.Gold);
                }
            }
        }

        private static void DrawBeeWing(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            var root0 = new Vector3(0f, 0f, 0.06f);
            var root1 = new Vector3(0f, 0f, -0.06f);
            var lead = new Vector3(0.24f, 0.04f, 0.02f);
            var tip = new Vector3(0.42f, 0.05f, -0.12f);
            var trail = new Vector3(0.24f, 0.03f, -0.24f);
            b.TwoSided(root0, lead, root1, EnemySwatch.OwlFace, EnemySwatch.Ivory);
            b.TwoSided(lead, tip, trail, EnemySwatch.OwlFace, EnemySwatch.Ivory);
            b.TwoSided(lead, trail, root1, EnemySwatch.OwlFace, EnemySwatch.Ivory);
            b.Segment(root0, tip, 0.008f, 0.005f, 3, EnemySwatch.Gold);
            b.Matrix = Matrix4x4.identity;
        }

        // ------------------------------------------------------------------
        // Pássaro-Flauta Apagado
        // ------------------------------------------------------------------

        public static void BuildFluteBird(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawFluteBirdBody, Quaternion.Euler(-8f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.1f), DrawFluteBirdHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.04f, 0.1f), b =>
                b.Cylinder(Vector3.zero, 0.018f, 0.006f, 0.13f, 4, EnemySwatch.Bark, rotation: Quaternion.Euler(95f, 0f, 0f)));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Verdigris, EnemySwatch.Ivory, EnemySwatch.BarkDark, 0.8f),
                Quaternion.Euler(0f, 0f, -22f), meshKey: "FluteBirdWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Verdigris, EnemySwatch.Ivory, EnemySwatch.BarkDark, 0.8f),
                Quaternion.Euler(0f, 0f, 22f), meshKey: "FluteBirdWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.18f), b =>
            {
                RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Verdigris, EnemySwatch.Ivory);
                // The two long tail plumes that whistle when it dives.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.03f * s, -0.02f, -0.36f), new Vector3(0.03f, 0.01f, 0.5f), EnemySwatch.Verdigris, Quaternion.Euler(-10f, 6f * s, 0f));
                    b.Blob(new Vector3(0.05f * s, -0.06f, -0.6f), new Vector3(0.05f, 0.02f, 0.07f), EnemySwatch.SoftEye, coarse: true);
                }
            }, Quaternion.Euler(20f, 0f, 0f));
        }

        private static void DrawFluteBirdBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.32f, 0.4f), EnemySwatch.Verdigris);
            b.Blob(new Vector3(0f, -0.03f, 0.07f), new Vector3(0.2f, 0.24f, 0.24f), EnemySwatch.Ivory, coarse: true);
            RangedEnemyRecipes.DrawTalons(b, -0.38f, 0.06f, EnemySwatch.Horn);
        }

        private static void DrawFluteBirdHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.17f, 0.16f, 0.18f), EnemySwatch.Verdigris);
            // A swept crest, eyes that once shone with the dance tunes, and the beak: a reed pipe with its finger holes.
            b.Box(new Vector3(0f, 0.17f, -0.05f), new Vector3(0.02f, 0.12f, 0.03f), EnemySwatch.Ivory, Quaternion.Euler(-45f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.055f * s, 0.08f, 0.06f), new Vector3(0.035f, 0.035f, 0.025f), EnemySwatch.SoftEye, coarse: true);
            }

            b.Cylinder(new Vector3(0f, 0.07f, 0.08f), 0.025f, 0.02f, 0.2f, 6, EnemySwatch.Bark, rotation: Quaternion.Euler(90f, 0f, 0f));
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(0f, 0.093f, 0.13f + i * 0.045f), new Vector3(0.014f, 0.008f, 0.014f), EnemySwatch.Black, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Raposa da Vinha
        // ------------------------------------------------------------------

        public static void BuildVineFox(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawVineFoxBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.36f), DrawVineFoxHead, Quaternion.Euler(4f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.07f, 0.03f, 0.15f), EnemySwatch.VampFur));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.32f), DrawVineFoxTail, Quaternion.Euler(-20f, 0f, 0f));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.08f * side, 0.38f, 0.2f), new Vector3(0f, -0.19f, -0.02f), new Vector3(0f, -0.18f, 0.03f),
                    0.035f, EnemySwatch.Copper, EnemySwatch.Black, "VineFoxFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.08f * side, 0.4f, -0.2f), new Vector3(0f, -0.2f, -0.05f), new Vector3(0f, -0.19f, 0.05f),
                    0.04f, EnemySwatch.Copper, EnemySwatch.Black, "VineFoxHind");
            }
        }

        private static void DrawVineFoxBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.3f, 0.66f), EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.06f, 0.14f), new Vector3(0.2f, 0.18f, 0.3f), EnemySwatch.OwlFace, coarse: true);
            b.Segment(new Vector3(0f, 0.04f, 0.24f), new Vector3(0f, 0.12f, 0.36f), 0.08f, 0.06f, 5, EnemySwatch.Copper);
        }

        private static void DrawVineFoxHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.15f, 0.18f), EnemySwatch.Copper);
            // The long muzzle stained purple with stolen grapes.
            b.Cylinder(new Vector3(0f, -0.02f, 0.06f), 0.05f, 0.025f, 0.14f, 5, EnemySwatch.VampFur, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.01f, 0.2f), new Vector3(0.03f, 0.025f, 0.025f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.05f * s, 0.03f, 0.06f), new Vector3(0.03f, 0.025f, 0.02f), EnemySwatch.MoonEye, coarse: true);
                b.Cylinder(new Vector3(0.05f * s, 0.06f, -0.02f), 0.04f, 0f, 0.13f, 4, EnemySwatch.Copper, rotation: Quaternion.Euler(-10f, 0f, -14f * s));
                b.Cylinder(new Vector3(0.05f * s, 0.07f, -0.01f), 0.02f, 0f, 0.09f, 3, EnemySwatch.Black, rotation: Quaternion.Euler(-10f, 0f, -14f * s));
            }
        }

        private static void DrawVineFoxTail(EnemyMeshBuilder b)
        {
            // A big brush with a white tip, a vine tangled in it and a stolen bunch of grapes.
            b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.3f), 0.07f, 0.1f, 6, EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.07f, -0.36f), new Vector3(0.14f, 0.14f, 0.16f), EnemySwatch.OwlFace, coarse: true);
            b.Segment(new Vector3(0.06f, 0.04f, -0.04f), new Vector3(-0.07f, 0.02f, -0.26f), 0.012f, 0.01f, 3, EnemySwatch.BarkDark);
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0.05f - i * 0.05f, 0.06f, -0.06f - i * 0.08f), new Vector3(0.07f, 0.01f, 0.06f), EnemySwatch.CaveMoss, Quaternion.Euler(0f, 30f * i, 20f));
            }

            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(-0.07f + (i % 2) * 0.03f, -0.02f - i * 0.025f, -0.24f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.VampWing, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Gralha-Alpina
        // ------------------------------------------------------------------

        public static void BuildAlpineChough(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.34f, 0f), DrawChoughBody, Quaternion.Euler(-8f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.1f), DrawChoughHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.04f, 0.1f), b =>
                b.Cylinder(Vector3.zero, 0.02f, 0.004f, 0.09f, 4, EnemySwatch.Gold, rotation: Quaternion.Euler(100f, 0f, 0f)));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Shadow, EnemySwatch.BatFurDark, EnemySwatch.Black, 0.75f),
                Quaternion.Euler(0f, 0f, -24f), meshKey: "ChoughWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Shadow, EnemySwatch.BatFurDark, EnemySwatch.Black, 0.75f),
                Quaternion.Euler(0f, 0f, 24f), meshKey: "ChoughWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.18f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Shadow, EnemySwatch.BatFurDark),
                Quaternion.Euler(22f, 0f, 0f));
        }

        private static void DrawChoughBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.32f, 0.42f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.06f, -0.04f), new Vector3(0.24f, 0.14f, 0.34f), EnemySwatch.BatFurDark, coarse: true);
            // Coral-red legs.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.06f * s, -0.14f, 0.02f), new Vector3(0.06f * s, -0.32f, 0.04f), 0.03f, 0.018f, 4, EnemySwatch.WarCape);
                for (int k = -1; k <= 1; k++)
                {
                    b.Cylinder(new Vector3(0.06f * s + 0.022f * k, -0.33f, 0.05f), 0.012f, 0f, 0.05f, 3, EnemySwatch.Black, rotation: Quaternion.Euler(110f, 0f, 0f));
                }
            }
        }

        private static void DrawChoughHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.17f, 0.16f, 0.18f), EnemySwatch.Shadow);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.05f * s, 0.08f, 0.06f), new Vector3(0.03f, 0.03f, 0.022f), EnemySwatch.WarEye, coarse: true);
            }

            // The curved yellow beak.
            b.Horn(new Vector3(0f, 0.07f, 0.08f), 0.024f, 0.12f, 3, 10f, EnemySwatch.Gold, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
        }

        // ------------------------------------------------------------------
        // Takin Apagado
        // ------------------------------------------------------------------

        public static void BuildTakin(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawTakinBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.6f), DrawTakinHead, Quaternion.Euler(14f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.56f), b =>
                b.Cylinder(Vector3.zero, 0.05f, 0.02f, 0.14f, 4, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(-150f, 0f, 0f)));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.18f * side, 0.62f, 0.34f), new Vector3(0f, -0.31f, -0.02f), new Vector3(0f, -0.29f, 0.04f),
                    0.085f, EnemySwatch.SandstoneDark, EnemySwatch.Black, new Vector3(0.13f, 0.08f, 0.15f), "TakinFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.18f * side, 0.64f, -0.36f), new Vector3(0f, -0.32f, -0.06f), new Vector3(0f, -0.3f, 0.06f),
                    0.09f, EnemySwatch.SandstoneDark, EnemySwatch.Black, new Vector3(0.13f, 0.08f, 0.15f), "TakinHind");
            }
        }

        private static void DrawTakinBody(EnemyMeshBuilder b)
        {
            // Thick golden coat greyed by the shadow, high shoulders, a hump of fur; dark streaks of the dimming.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.62f, 1.2f), EnemySwatch.OwlBuff);
            b.Blob(new Vector3(0f, 0.16f, 0.24f), new Vector3(0.54f, 0.4f, 0.5f), EnemySwatch.Sandstone, coarse: true);
            b.Blob(new Vector3(0f, -0.12f, 0f), new Vector3(0.48f, 0.3f, 0.96f), EnemySwatch.SandstoneDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.28f * s, 0.06f, -0.1f), new Vector3(0.03f, 0.26f, 0.5f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, 8f * s));
            }

            b.Segment(new Vector3(0f, 0.08f, 0.4f), new Vector3(0f, 0.12f, 0.6f), 0.18f, 0.14f, 6, EnemySwatch.OwlBuff);
        }

        private static void DrawTakinHead(EnemyMeshBuilder b)
        {
            // The heavy arched Roman nose, dark face, and the short horns that sweep out then back.
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.3f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.02f, 0.16f), new Vector3(0.22f, 0.22f, 0.24f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.08f, 0.26f), new Vector3(0.14f, 0.1f, 0.08f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.1f * s, 0.05f, 0.08f), new Vector3(0.04f, 0.035f, 0.03f), EnemySwatch.WarEye, coarse: true);
                b.Horn(new Vector3(0.08f * s, 0.12f, -0.02f), 0.045f, 0.24f, 4, 26f, EnemySwatch.Black, Quaternion.Euler(0f, 0f, -80f * s), 5, Vector3.forward * -s);
                b.Cylinder(new Vector3(0.14f * s, 0.06f, -0.06f), 0.04f, 0f, 0.1f, 3, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(0f, 0f, -100f * s));
            }
        }

        // ------------------------------------------------------------------
        // Cabra-Azul (bharal)
        // ------------------------------------------------------------------

        public static void BuildBlueSheep(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawBlueSheepBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.4f), DrawBlueSheepHead, Quaternion.Euler(10f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.36f), b =>
                b.Cylinder(Vector3.zero, 0.035f, 0f, 0.1f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(-130f, 0f, 0f)));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.1f * side, 0.5f, 0.24f), new Vector3(0f, -0.25f, -0.02f), new Vector3(0f, -0.24f, 0.03f),
                    0.04f, EnemySwatch.CaveSkin, EnemySwatch.Black, new Vector3(0.06f, 0.06f, 0.08f), "BlueSheepFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.1f * side, 0.52f, -0.24f), new Vector3(0f, -0.26f, -0.06f), new Vector3(0f, -0.25f, 0.06f),
                    0.045f, EnemySwatch.CaveSkin, EnemySwatch.Black, new Vector3(0.06f, 0.06f, 0.08f), "BlueSheepHind");
            }
        }

        private static void DrawBlueSheepBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.36f, 0.72f), EnemySwatch.CaveSkin);
            b.Blob(new Vector3(0f, -0.1f, 0.02f), new Vector3(0.26f, 0.14f, 0.56f), EnemySwatch.CaveBelly, coarse: true);
            // The black stripe down the flank and the black chest, the way it hides against the slate.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.16f * s, -0.04f, 0f), new Vector3(0.02f, 0.05f, 0.56f), EnemySwatch.Black);
            }

            b.Blob(new Vector3(0f, -0.04f, 0.28f), new Vector3(0.2f, 0.22f, 0.14f), EnemySwatch.Black, coarse: true);
            b.Segment(new Vector3(0f, 0.06f, 0.26f), new Vector3(0f, 0.16f, 0.4f), 0.09f, 0.07f, 5, EnemySwatch.CaveSkin);
        }

        private static void DrawBlueSheepHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.15f, 0.16f, 0.22f), EnemySwatch.CaveSkin);
            b.Cylinder(new Vector3(0f, -0.02f, 0.06f), 0.05f, 0.035f, 0.1f, 5, EnemySwatch.CaveBelly, rotation: Quaternion.Euler(95f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.055f * s, 0.04f, 0.05f), new Vector3(0.03f, 0.03f, 0.022f), EnemySwatch.MoonEye, coarse: true);
                b.Cylinder(new Vector3(0.07f * s, 0.05f, -0.05f), 0.025f, 0f, 0.07f, 3, EnemySwatch.CaveSkin, rotation: Quaternion.Euler(0f, 0f, -70f * s));
                // Heavy horns that sweep out sideways and back, the bharal's own.
                b.Horn(new Vector3(0.04f * s, 0.08f, -0.02f), 0.035f, 0.28f, 5, 18f, EnemySwatch.Horn, Quaternion.Euler(-20f, 0f, -60f * s), 5, Vector3.forward * -s);
            }
        }

        // ------------------------------------------------------------------
        // Morcego-Vela
        // ------------------------------------------------------------------

        /// <summary>How far (m) a Morcego-Vela wing reaches out from the shoulder.</summary>
        public const float CandleBatWingReach = 0.78f;

        public static void BuildCandleBat(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.55f, 0f), DrawCandleBatBody);
            bp.Mesh(ProceduralEnemyParts.Lantern, body, new Vector3(0f, 0.42f, 0.2f), b =>
            {
                // The flame of the candle on its head: the light it stole from the bridge lamps.
                b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.07f, 0.12f, 0.07f), EnemySwatch.Ember);
                b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.04f, 0.07f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.15f * side, 0.08f, 0f),
                    b => DrawCandleBatWing(b, side), Quaternion.Euler(0f, 0f, 14f * side));
            }
        }

        private static void DrawCandleBatBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.4f, 0.48f), EnemySwatch.BatFur);
            b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.26f, 0.26f, 0.34f), EnemySwatch.BatFurDark, coarse: true);
            b.Blob(new Vector3(0f, 0.16f, 0.24f), new Vector3(0.26f, 0.24f, 0.24f), EnemySwatch.BatFur);
            // The candle stub stuck to the head with old wax dripping down.
            b.Cylinder(new Vector3(0f, 0.26f, 0.2f), 0.045f, 0.04f, 0.16f, 6, EnemySwatch.Ivory);
            b.Blob(new Vector3(0.03f, 0.26f, 0.22f), new Vector3(0.1f, 0.05f, 0.09f), EnemySwatch.IvoryDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.08f * s, 0.24f, 0.22f), 0.06f, 0f, 0.18f, 4, EnemySwatch.BatWing, rotation: Quaternion.Euler(-10f, 0f, -26f * s));
                b.Blob(new Vector3(0.06f * s, 0.18f, 0.35f), new Vector3(0.06f, 0.055f, 0.03f), EnemySwatch.BatEye, coarse: true);
                b.Cylinder(new Vector3(0.03f * s, 0.08f, 0.35f), 0.015f, 0f, 0.05f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                // Little hooked feet hanging under the body: the lowest point of the model.
                b.Segment(new Vector3(0.07f * s, -0.16f, -0.08f), new Vector3(0.08f * s, -0.5f, -0.12f), 0.03f, 0.018f, 4, EnemySwatch.BatFurDark);
                b.Cylinder(new Vector3(0.08f * s, -0.52f, -0.12f), 0.025f, 0f, 0.05f, 3, EnemySwatch.Bone, rotation: Quaternion.Euler(120f, 0f, 0f));
            }
        }

        private static void DrawCandleBatWing(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            var root0 = new Vector3(0f, 0f, 0.12f);
            var root1 = new Vector3(0f, 0f, -0.18f);
            var elbow = new Vector3(0.32f, 0.07f, 0.1f);
            var tip = new Vector3(CandleBatWingReach, 0.02f, -0.04f);
            var f1 = new Vector3(0.66f, -0.02f, -0.26f);
            var f2 = new Vector3(0.46f, 0f, -0.32f);
            var f3 = new Vector3(0.22f, 0f, -0.28f);
            b.TwoSided(root0, elbow, f3, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(root0, f3, root1, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(elbow, f2, f3, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(elbow, f1, f2, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.TwoSided(elbow, tip, f1, EnemySwatch.BatWing, EnemySwatch.BatFurDark);
            b.Segment(root0, elbow, 0.025f, 0.02f, 4, EnemySwatch.BatFurDark);
            b.Segment(elbow, tip, 0.02f, 0.012f, 4, EnemySwatch.BatFurDark);
            b.Segment(elbow, f1, 0.012f, 0.008f, 3, EnemySwatch.BatFurDark);
            b.Segment(elbow, f2, 0.012f, 0.008f, 3, EnemySwatch.BatFurDark);
            // Drops of wax on the wing, from the candle.
            b.Blob(new Vector3(0.28f, 0.03f, -0.12f), new Vector3(0.04f, 0.02f, 0.04f), EnemySwatch.Ivory, coarse: true);
            b.Matrix = Matrix4x4.identity;
        }

        // ------------------------------------------------------------------
        // Ushabti de Corda
        // ------------------------------------------------------------------

        public static void BuildRopeUshabti(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.78f, 0f), DrawUshabtiBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.38f, 0.01f), DrawUshabtiHead);

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.23f * side, 0.26f, 0.02f),
                    b => DrawUshabtiArm(b, side, right), Quaternion.Euler(-10f, 0f, 0f));

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1, new Vector3(0.09f * side, 0.5f, 0f),
                    b => b.Segment(new Vector3(0f, -0.25f, 0.02f), new Vector3(0f, 0.04f, 0f), 0.06f, 0.075f, 6, EnemySwatch.SandstoneDark), meshKey: "UshabtiThigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg, new Vector3(0f, -0.25f, 0.02f), b =>
                {
                    b.Segment(new Vector3(0f, -0.23f, -0.01f), Vector3.zero, 0.05f, 0.06f, 6, EnemySwatch.SandstoneDark);
                    b.Cylinder(new Vector3(0f, -0.14f, -0.01f), 0.065f, 0.065f, 0.03f, 6, EnemySwatch.Bark);
                }, meshKey: "UshabtiShin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -0.23f, -0.01f),
                    b => b.Blob(new Vector3(0f, -0.005f, 0.04f), new Vector3(0.1f, 0.06f, 0.18f), EnemySwatch.Sandstone, coarse: true), meshKey: "UshabtiFoot");
            }
        }

        private static void DrawUshabtiBody(EnemyMeshBuilder b)
        {
            // A little clay servant figure, its body bound in knotted rope; the column of glowing signs that tells it to work.
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.4f, 0.62f, 0.3f), EnemySwatch.SandstoneDark);
            for (int i = 0; i < 4; i++)
            {
                b.Cylinder(new Vector3(0f, -0.12f + i * 0.12f, 0f), 0.2f - Mathf.Abs(i - 1.5f) * 0.02f, 0.2f - Mathf.Abs(i - 1.5f) * 0.02f, 0.035f, 8, EnemySwatch.Bark,
                    rotation: Quaternion.Euler(0f, 0f, i % 2 == 0 ? 6f : -6f));
            }

            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.2f - i * 0.08f, 0.15f), new Vector3(0.05f, 0.04f, 0.02f), EnemySwatch.Rune);
            }

            // The rope basket on its back.
            b.Cylinder(new Vector3(0f, 0.04f, -0.2f), 0.12f, 0.14f, 0.24f, 7, EnemySwatch.BarkDark, cap: EnemySwatch.Bark);
            b.Cylinder(new Vector3(0f, -0.28f, 0f), 0.16f, 0.14f, 0.1f, 8, EnemySwatch.SandstoneDark);
        }

        private static void DrawUshabtiHead(EnemyMeshBuilder b)
        {
            // The striped headcloth and a calm painted face, eyes dimmed to embers.
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.22f, 0.24f, 0.22f), EnemySwatch.Sandstone);
            b.Box(new Vector3(0f, 0.08f, -0.04f), new Vector3(0.28f, 0.22f, 0.2f), EnemySwatch.Verdigris);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.12f * s, -0.04f, 0.02f), new Vector3(0.06f, 0.18f, 0.08f), EnemySwatch.Verdigris);
                b.Box(new Vector3(0.045f * s, 0.07f, 0.11f), new Vector3(0.05f, 0.02f, 0.01f), EnemySwatch.Ember);
            }

            b.Box(new Vector3(0f, -0.07f, 0.1f), new Vector3(0.03f, 0.08f, 0.03f), EnemySwatch.SandstoneDark);
        }

        private static void DrawUshabtiArm(EnemyMeshBuilder b, float side, bool right)
        {
            b.Blob(Vector3.zero, new Vector3(0.11f, 0.11f, 0.11f), EnemySwatch.SandstoneDark, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.02f * side, -0.38f, 0.03f), 0.05f, 0.045f, 6, EnemySwatch.SandstoneDark);
            b.Cylinder(new Vector3(0.01f * side, -0.2f, 0.015f), 0.055f, 0.055f, 0.03f, 6, EnemySwatch.Bark);
            b.Blob(new Vector3(0.02f * side, -0.42f, 0.04f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.Sandstone, coarse: true);
            if (right)
            {
                // The hoe it was made to work the fields of the other world with.
                b.Cylinder(new Vector3(0.02f * side, -0.6f, 0.04f), 0.018f, 0.018f, 0.5f, 5, EnemySwatch.Bark);
                b.Box(new Vector3(0.02f * side, -0.6f, 0.08f), new Vector3(0.06f, 0.1f, 0.14f), EnemySwatch.CopperDark);
            }
        }

        // ------------------------------------------------------------------
        // Gato-de-Pallas Apagado
        // ------------------------------------------------------------------

        public static void BuildPallasCat(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawPallasBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.34f), DrawPallasHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.06f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.1f, 0.03f, 0.08f), EnemySwatch.WolfBelly);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.03f * s, 0f, 0.08f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.3f), DrawPallasTail, Quaternion.Euler(-14f, 0f, 0f));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.1f * side, 0.36f, 0.18f), new Vector3(0f, -0.18f, -0.02f), new Vector3(0f, -0.17f, 0.03f),
                    0.045f, EnemySwatch.GolemStone, EnemySwatch.WolfFurDark, "PallasFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.1f * side, 0.37f, -0.18f), new Vector3(0f, -0.19f, -0.05f), new Vector3(0f, -0.17f, 0.05f),
                    0.05f, EnemySwatch.GolemStone, EnemySwatch.WolfFurDark, "PallasHind");
            }
        }

        private static void DrawPallasBody(EnemyMeshBuilder b)
        {
            // So much long grey fur it looks like a cushion with legs; faint dark bars on the flanks.
            b.Blob(Vector3.zero, new Vector3(0.38f, 0.36f, 0.62f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.34f, 0.26f, 0.56f), EnemySwatch.WolfBelly, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0.18f * s, 0.06f, -0.14f + i * 0.1f), new Vector3(0.02f, 0.12f, 0.025f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, 10f * s));
                }
            }
        }

        private static void DrawPallasHead(EnemyMeshBuilder b)
        {
            // The flat wide face, ears set low on the sides, the round pupils and the cross look of the manul.
            b.Blob(Vector3.zero, new Vector3(0.24f, 0.18f, 0.18f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.03f, 0.07f), new Vector3(0.14f, 0.09f, 0.08f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, -0.01f, 0.11f), new Vector3(0.03f, 0.02f, 0.02f), EnemySwatch.Pink, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.05f * s, 0.03f, 0.08f), new Vector3(0.04f, 0.04f, 0.02f), EnemySwatch.MoonEye, coarse: true);
                b.Box(new Vector3(0.05f * s, 0.065f, 0.085f), new Vector3(0.05f, 0.012f, 0.01f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, -14f * s));
                b.Cylinder(new Vector3(0.11f * s, 0.03f, -0.02f), 0.035f, 0f, 0.06f, 3, EnemySwatch.GolemStone, rotation: Quaternion.Euler(0f, 0f, -75f * s));
                b.Box(new Vector3(0.1f * s, -0.04f, 0.04f), new Vector3(0.06f, 0.012f, 0.012f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, 20f * s));
            }
        }

        private static void DrawPallasTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.1f, -0.3f), 0.07f, 0.07f, 6, EnemySwatch.GolemStone);
            for (int i = 0; i < 3; i++)
            {
                float f = 0.4f + i * 0.25f;
                b.Cylinder(new Vector3(0f, -0.1f * f, -0.3f * f), 0.074f, 0.074f, 0.025f, 6, EnemySwatch.WolfFurDark, rotation: Quaternion.Euler(-72f, 0f, 0f));
            }

            b.Blob(new Vector3(0f, -0.1f, -0.31f), new Vector3(0.12f, 0.12f, 0.08f), EnemySwatch.Black, coarse: true);
        }

        // ------------------------------------------------------------------
        // Águia da Tempestade
        // ------------------------------------------------------------------

        public static void BuildStormEagle(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawStormEagleBody, Quaternion.Euler(-10f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.1f), DrawStormEagleHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.04f, 0.13f), b =>
                b.Cylinder(Vector3.zero, 0.025f, 0.006f, 0.08f, 4, EnemySwatch.Gold, rotation: Quaternion.Euler(105f, 0f, 0f)));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.18f, 0.12f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.IronDark, EnemySwatch.Iron, EnemySwatch.Crystal, 1.05f),
                Quaternion.Euler(0f, 0f, -18f), meshKey: "StormEagleWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.18f, 0.12f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.IronDark, EnemySwatch.Iron, EnemySwatch.Crystal, 1.05f),
                Quaternion.Euler(0f, 0f, 18f), meshKey: "StormEagleWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.16f, -0.22f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.IronDark, EnemySwatch.Iron),
                Quaternion.Euler(28f, 0f, 0f));
        }

        private static void DrawStormEagleBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.54f, 0.52f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, -0.03f, 0.08f), new Vector3(0.32f, 0.4f, 0.3f), EnemySwatch.Iron, coarse: true);
            // A zigzag of trapped lightning across the chest.
            b.Box(new Vector3(-0.03f, 0.08f, 0.24f), new Vector3(0.02f, 0.12f, 0.02f), EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, 25f));
            b.Box(new Vector3(0.02f, -0.04f, 0.24f), new Vector3(0.02f, 0.12f, 0.02f), EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, -25f));
            RangedEnemyRecipes.DrawTalons(b, -0.46f, 0.08f, EnemySwatch.Gold);
        }

        private static void DrawStormEagleHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.07f, 0f), new Vector3(0.22f, 0.21f, 0.24f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.12f, -0.06f), new Vector3(0.2f, 0.12f, 0.18f), EnemySwatch.IronDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.065f * s, 0.1f, 0.08f), new Vector3(0.04f, 0.035f, 0.03f), EnemySwatch.MoonEye, coarse: true);
                b.Box(new Vector3(0.06f * s, 0.13f, 0.08f), new Vector3(0.06f, 0.015f, 0.04f), EnemySwatch.IronDark, Quaternion.Euler(0f, 0f, -12f * s));
            }

            // The big hooked upper beak.
            b.Horn(new Vector3(0f, 0.08f, 0.1f), 0.04f, 0.13f, 3, 24f, EnemySwatch.Gold, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
        }

        // ------------------------------------------------------------------
        // Harpia de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeHarpy(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawHarpyBody, Quaternion.Euler(-8f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.34f, 0.1f), DrawHarpyHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.02f, 0.13f), b =>
                b.Cylinder(Vector3.zero, 0.03f, 0.008f, 0.08f, 4, EnemySwatch.Horn, rotation: Quaternion.Euler(110f, 0f, 0f)));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.14f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Shadow, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone, 1f),
                Quaternion.Euler(0f, 0f, -20f), meshKey: "HarpyWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.14f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Shadow, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone, 1f),
                Quaternion.Euler(0f, 0f, 20f), meshKey: "HarpyWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.18f, -0.22f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Shadow, EnemySwatch.GolemStoneDark),
                Quaternion.Euler(26f, 0f, 0f));
        }

        private static void DrawHarpyBody(EnemyMeshBuilder b)
        {
            // Feathers that are half smoke: dark puffs trailing off the back and shoulders.
            b.Blob(Vector3.zero, new Vector3(0.46f, 0.58f, 0.52f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.04f, 0.08f), new Vector3(0.34f, 0.42f, 0.3f), EnemySwatch.GolemStoneDark, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                float x = (i % 2 == 0 ? 0.14f : -0.14f);
                b.Blob(new Vector3(x, 0.2f - i * 0.08f, -0.22f - i * 0.04f), new Vector3(0.18f, 0.16f, 0.16f), EnemySwatch.GolemStone, coarse: true);
            }

            // Long hooked talons, the harpy's "snatchers".
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.09f * s, -0.24f, 0.04f), new Vector3(0.09f * s, -0.46f, 0.08f), 0.045f, 0.025f, 4, EnemySwatch.BarkDark);
                for (int k = -1; k <= 1; k++)
                {
                    b.Cylinder(new Vector3(0.09f * s + 0.03f * k, -0.48f, 0.1f), 0.016f, 0f, 0.1f, 3, EnemySwatch.Horn, rotation: Quaternion.Euler(105f, 0f, 0f));
                }
            }
        }

        private static void DrawHarpyHead(EnemyMeshBuilder b)
        {
            // A pale heart-shaped face mask (the harpy of the Greek tales, here only a beast), a wild crest of smoke.
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.24f, 0.24f, 0.22f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.05f, 0.07f), new Vector3(0.2f, 0.2f, 0.08f), EnemySwatch.OwlFace, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.05f * s, 0.08f, 0.11f), new Vector3(0.04f, 0.035f, 0.02f), EnemySwatch.WarEye, coarse: true);
            }

            for (int i = -2; i <= 2; i++)
            {
                b.Box(new Vector3(0.04f * i, 0.2f, -0.06f), new Vector3(0.03f, 0.16f, 0.03f), i % 2 == 0 ? EnemySwatch.GolemStone : EnemySwatch.Shadow, Quaternion.Euler(-35f, 0f, 14f * i));
            }

            b.Horn(new Vector3(0f, 0.04f, 0.1f), 0.032f, 0.09f, 3, 22f, EnemySwatch.Horn, Quaternion.Euler(95f, 0f, 0f), 4, Vector3.right);
        }
    }
}
