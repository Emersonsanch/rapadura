using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Costa das Monções and the Delta de Jhilpur (Docs/Mapas100.md 26, 30–33) that the continuous worlds
    /// played with stand-ins, low-poly from the shared palette like <see cref="OverworldBeastRecipes"/>. Pivot at the feet
    /// (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Crocodilo do Mangue Apagado</b> (~0.65 m, 2.8 m long): armoured back with rows of scutes, eyes on top of the
    /// head, a lower jaw that gapes. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Body</c> → <c>Tail</c> → <c>Tail_1</c> →
    /// <c>Tail_2</c>, four sprawled legs <c>Leg_FL/FR/HL/HR</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Leão do Capim Apagado</b> (~1 m): tawny lion, dark mane, tufted tail, ember eyes. <c>Body</c> → <c>Head</c> →
    /// <c>Jaw</c>, <c>Tail</c> → <c>Tail_1</c>, four jointed legs.</item>
    /// <item><b>Rinoceronte-de-Pó</b> (~1.15 m): dust-grey rhino, two horns, cracks of trapped light on the back; lowers its
    /// head to charge. <c>Body</c> → <c>Head</c>, <c>Tail</c>, four thick legs with hooves.</item>
    /// <item><b>Macaco Ladrão</b> (~0.9 m to the tail): grey langur with a black face, long tail curled up with a stolen gold
    /// bangle on it. <c>Body</c> → <c>Head</c>, <c>Tail</c> → <c>Tail_1</c> → <c>Tail_2</c>, four jointed legs.</item>
    /// <item><b>Lontra Ladra</b> (~0.65 m, 1.4 m long): long low otter with its head up and a stolen glowing fish in the mouth.
    /// <c>Body</c> → <c>Head</c>, <c>Tail</c> → <c>Tail_1</c>, four short legs.</item>
    /// <item><b>Píton Apagada</b> (~0.75 m raised, 1.5 m long): blotched python, forepart up, the rest in a wide S. Same joints
    /// as the sand snake (<c>Body</c> → <c>Head</c>; <c>Tail</c> → <c>Tail_1</c> → <c>Tail_2</c> → <c>Tail_3</c>), slithered
    /// by the rig.</item>
    /// <item><b>Trol Apagado</b> (~2.1 m): mossy swamp troll, long arms, droopy ears, a caged will-o'-wisp on its back.
    /// <c>Body</c> → <c>Head</c>, <c>Body</c> → <c>StolenLight</c>, <c>Arm_L</c>/<c>Arm_R</c>; <c>Leg_L</c>/<c>Leg_R</c> →
    /// <c>Shin_</c> → <c>Foot_</c>.</item>
    /// </list>
    /// </summary>
    public static class CoastDeltaBeastRecipes
    {
        // ------------------------------------------------------------------
        // Crocodilo do Mangue Apagado
        // ------------------------------------------------------------------

        public static void BuildMangroveCrocodile(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.36f, 0f), DrawCrocBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0f, 0.56f), DrawCrocHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.02f), DrawCrocJaw);

            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.52f), b => DrawCrocTail(b, new Vector3(0f, 0f, -0.42f), 0.17f, 0.13f));
            int tail1 = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, 0f, -0.42f), b => DrawCrocTail(b, new Vector3(0f, 0f, -0.4f), 0.13f, 0.08f));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "2", tail1, new Vector3(0f, 0f, -0.4f), b => DrawCrocTail(b, new Vector3(0f, -0.02f, -0.38f), 0.08f, 0.015f));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                // Sprawled legs: the knee sticks out sideways, the foot comes back under it.
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.24f * side, 0.3f, 0.3f), new Vector3(0.13f * side, -0.13f, 0.02f),
                    new Vector3(0.03f * side, -0.14f, 0.05f), 0.06f, EnemySwatch.CaveMoss, EnemySwatch.BarkDark, "CrocFront" + x);
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.25f * side, 0.32f, -0.32f), new Vector3(0.14f * side, -0.14f, -0.02f),
                    new Vector3(0.03f * side, -0.15f, 0.04f), 0.07f, EnemySwatch.CaveMoss, EnemySwatch.BarkDark, "CrocHind" + x);
            }
        }

        private static void DrawCrocBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.44f, 1.1f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.56f, 0.22f, 0.96f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, 0.12f, -0.04f), new Vector3(0.44f, 0.18f, 0.84f), EnemySwatch.Lichen, coarse: true);
            // Two rows of dark scutes down the back, and the mangrove mud dried on the flanks.
            for (int k = 0; k < 5; k++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.11f * s, 0.22f, 0.4f - 0.2f * k), new Vector3(0.08f, 0.12f, 0.1f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, 10f * s));
                }
            }

            b.Blob(new Vector3(0.26f, -0.04f, 0.1f), new Vector3(0.14f, 0.16f, 0.4f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(-0.26f, -0.06f, -0.2f), new Vector3(0.14f, 0.14f, 0.34f), EnemySwatch.BarkDark, coarse: true);
        }

        private static void DrawCrocHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.04f, 0.04f), new Vector3(0.34f, 0.2f, 0.3f), EnemySwatch.CaveMoss);
            b.Box(new Vector3(0f, 0.02f, 0.3f), new Vector3(0.22f, 0.1f, 0.42f), EnemySwatch.CaveMoss, top: EnemySwatch.Lichen);
            b.Blob(new Vector3(0f, 0.07f, 0.5f), new Vector3(0.09f, 0.05f, 0.06f), EnemySwatch.BarkDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // Eyes on bumps on top of the head (it watches with the rest under the water), the light it swallowed in them.
                b.Blob(new Vector3(0.08f * s, 0.13f, 0.06f), new Vector3(0.08f, 0.07f, 0.08f), EnemySwatch.CaveMoss, coarse: true);
                b.Blob(new Vector3(0.08f * s, 0.15f, 0.09f), new Vector3(0.045f, 0.035f, 0.035f), EnemySwatch.MoonEye, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Cylinder(new Vector3(0.1f * s, -0.03f, 0.16f + 0.1f * i), 0.012f, 0f, 0.045f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                }
            }
        }

        private static void DrawCrocJaw(EnemyMeshBuilder b)
        {
            b.Box(new Vector3(0f, -0.02f, 0.26f), new Vector3(0.2f, 0.06f, 0.46f), EnemySwatch.WolfBelly);
            b.Blob(new Vector3(0f, -0.02f, 0.02f), new Vector3(0.26f, 0.08f, 0.14f), EnemySwatch.CaveMoss, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0.085f * s, 0.01f, 0.14f + 0.11f * i), 0.011f, 0f, 0.04f, 3, EnemySwatch.Fang);
                }
            }
        }

        private static void DrawCrocTail(EnemyMeshBuilder b, Vector3 end, float r0, float r1)
        {
            b.Segment(Vector3.zero, end, r0, r1, 6, EnemySwatch.CaveMoss);
            b.Blob(Vector3.zero, Vector3.one * (r0 * 2f), EnemySwatch.CaveMoss, coarse: true);
            // The single crest of scutes that runs down the tail.
            for (int k = 1; k <= 2; k++)
            {
                Vector3 at = end * (k / 3f);
                float r = Mathf.Lerp(r0, r1, k / 3f);
                b.Box(at + new Vector3(0f, r * 0.95f, 0f), new Vector3(0.04f, r * 0.7f + 0.03f, 0.07f), EnemySwatch.BarkDark);
            }
        }

        // ------------------------------------------------------------------
        // Leão do Capim Apagado
        // ------------------------------------------------------------------

        public static void BuildGrassLion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawLionBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.52f), DrawLionHead, Quaternion.Euler(8f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.09f, 0.08f), b => DrawFelineJaw(b, EnemySwatch.SandstoneLight, 1f));

            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.46f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.14f, -0.22f), 0.032f, 0.03f, 5, EnemySwatch.OwlBuff));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, -0.14f, -0.22f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.2f, -0.12f), 0.028f, 0.022f, 5, EnemySwatch.OwlBuff);
                b.Blob(new Vector3(0f, -0.22f, -0.14f), new Vector3(0.09f, 0.11f, 0.09f), EnemySwatch.BarkDark, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.12f * side, 0.55f, 0.32f), new Vector3(0f, -0.26f, -0.02f), new Vector3(0f, -0.26f, 0.04f),
                    0.065f, EnemySwatch.OwlBuff, EnemySwatch.SandstoneDark, "LionFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.12f * side, 0.57f, -0.32f), new Vector3(0f, -0.27f, -0.07f), new Vector3(0f, -0.27f, 0.07f),
                    0.07f, EnemySwatch.OwlBuff, EnemySwatch.SandstoneDark, "LionHind");
            }
        }

        private static void DrawLionBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.4f, 0.42f, 0.95f), EnemySwatch.OwlBuff);
            b.Blob(new Vector3(0f, -0.09f, 0.04f), new Vector3(0.3f, 0.2f, 0.74f), EnemySwatch.SandstoneLight, coarse: true);
            // The mane runs on over the shoulders; a few cinders of the dry-grass fire caught in it.
            b.Blob(new Vector3(0f, 0.08f, 0.32f), new Vector3(0.44f, 0.4f, 0.36f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0.08f, 0.24f, 0.3f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.1f, 0.2f, 0.22f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.Ember, coarse: true);
        }

        private static void DrawLionHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, -0.06f), new Vector3(0.52f, 0.5f, 0.34f), EnemySwatch.BarkDark);
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.26f), EnemySwatch.OwlBuff);
            b.Cylinder(new Vector3(0f, -0.04f, 0.08f), 0.08f, 0.06f, 0.13f, 6, EnemySwatch.SandstoneLight, EnemySwatch.SandstoneLight, Quaternion.Euler(90f, 0f, 0f));
            b.Box(new Vector3(0f, -0.01f, 0.215f), new Vector3(0.06f, 0.04f, 0.02f), EnemySwatch.Black);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.05f, 0.11f), new Vector3(0.04f, 0.03f, 0.03f), EnemySwatch.Ember, coarse: true);
                b.Blob(new Vector3(0.11f * s, 0.16f, 0.02f), new Vector3(0.07f, 0.07f, 0.04f), EnemySwatch.OwlBuff, coarse: true);
            }
        }

        /// <summary>Lower jaw of the big cats and the bear: pale chin, four fangs pointing up.</summary>
        internal static void DrawFelineJaw(EnemyMeshBuilder b, EnemySwatch chin, float size)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * size);
            b.Box(new Vector3(0f, -0.015f, 0.08f), new Vector3(0.1f, 0.04f, 0.15f), chin);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.035f * s, 0f, 0.13f), 0.012f, 0f, 0.045f, 3, EnemySwatch.Fang);
            }

            b.Matrix = Matrix4x4.identity;
        }

        // ------------------------------------------------------------------
        // Rinoceronte-de-Pó
        // ------------------------------------------------------------------

        public static void BuildDustRhino(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawRhinoBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, -0.08f, 0.72f), DrawRhinoHead, Quaternion.Euler(22f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.68f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.26f, -0.06f), 0.035f, 0.025f, 5, EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, -0.28f, -0.06f), new Vector3(0.05f, 0.08f, 0.05f), EnemySwatch.Black, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                AddLimb(bp, "F" + x, new Vector3(0.22f * side, 0.6f, 0.42f), new Vector3(0f, -0.3f, 0.02f), new Vector3(0f, -0.27f, 0f),
                    0.1f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.17f, 0.08f, 0.19f), "RhinoFront");
                AddLimb(bp, "H" + x, new Vector3(0.22f * side, 0.62f, -0.42f), new Vector3(0f, -0.3f, -0.04f), new Vector3(0f, -0.29f, 0.04f),
                    0.11f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.18f, 0.08f, 0.2f), "RhinoHind");
            }
        }

        private static void DrawRhinoBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.78f, 0.78f, 1.4f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.1f, 0.32f), new Vector3(0.7f, 0.6f, 0.6f), EnemySwatch.GolemStone, coarse: true);
            b.Blob(new Vector3(0f, -0.16f, 0f), new Vector3(0.6f, 0.4f, 1.1f), EnemySwatch.GolemStoneDark, coarse: true);
            // The dust it rolls in, caked on the flanks, and the cracks where the trapped light shows through.
            b.Blob(new Vector3(0.3f, 0.04f, -0.2f), new Vector3(0.2f, 0.34f, 0.5f), EnemySwatch.SandstoneDark, coarse: true);
            b.Blob(new Vector3(-0.3f, 0.08f, 0.12f), new Vector3(0.2f, 0.3f, 0.44f), EnemySwatch.SandstoneDark, coarse: true);
            b.Box(new Vector3(0.06f, 0.38f, 0.02f), new Vector3(0.04f, 0.03f, 0.4f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 18f, 0f));
            b.Box(new Vector3(-0.1f, 0.36f, -0.3f), new Vector3(0.04f, 0.03f, 0.26f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, -30f, 0f));
            // The folds of the skin round the shoulders and the hips.
            b.Box(new Vector3(0f, 0.04f, 0.5f), new Vector3(0.58f, 0.52f, 0.04f), EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0f, 0.02f, -0.42f), new Vector3(0.64f, 0.58f, 0.04f), EnemySwatch.GolemStoneDark);
        }

        private static void DrawRhinoHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, 0.12f), new Vector3(0.36f, 0.34f, 0.5f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.06f, 0.36f), new Vector3(0.3f, 0.26f, 0.24f), EnemySwatch.GolemStoneDark, coarse: true);
            // Two horns of dusty keratin, the front one long.
            b.Cylinder(new Vector3(0f, 0.06f, 0.42f), 0.07f, 0f, 0.34f, 6, EnemySwatch.Horn, rotation: Quaternion.Euler(-5f, 0f, 0f));
            b.Cylinder(new Vector3(0f, 0.12f, 0.24f), 0.05f, 0f, 0.16f, 5, EnemySwatch.Horn);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.15f * s, 0.06f, 0.2f), new Vector3(0.035f, 0.03f, 0.03f), EnemySwatch.WarEye, coarse: true);
                b.Cylinder(new Vector3(0.11f * s, 0.14f, -0.06f), 0.05f, 0f, 0.13f, 4, EnemySwatch.GolemStone, rotation: Quaternion.Euler(-15f, 0f, -20f * s));
            }
        }

        // ------------------------------------------------------------------
        // Macaco Ladrão
        // ------------------------------------------------------------------

        public static void BuildThiefMonkey(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawMonkeyBody, Quaternion.Euler(-12f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.32f), DrawMonkeyHead, Quaternion.Euler(12f, 0f, 0f));

            // The long tail goes up and curls forward, the stolen bangle hanging on it.
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.28f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, 0.2f, -0.12f), 0.032f, 0.028f, 5, EnemySwatch.GolemStone));
            int tail1 = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, 0.2f, -0.12f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.22f, 0.06f), 0.028f, 0.022f, 5, EnemySwatch.GolemStone);
                b.Cylinder(new Vector3(0f, 0.1f, 0.02f), 0.05f, 0.05f, 0.025f, 8, EnemySwatch.Gold, rotation: Quaternion.Euler(-15f, 0f, 0f));
            });
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "2", tail1, new Vector3(0f, 0.22f, 0.06f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, 0.03f, 0.12f), 0.022f, 0.01f, 4, EnemySwatch.GolemStoneDark));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.11f * side, 0.44f, 0.2f), new Vector3(0f, -0.22f, 0.02f), new Vector3(0f, -0.19f, 0f),
                    0.045f, EnemySwatch.GolemStone, EnemySwatch.Black, "MonkeyFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.1f * side, 0.4f, -0.22f), new Vector3(0f, -0.19f, 0.06f), new Vector3(0f, -0.18f, -0.04f),
                    0.05f, EnemySwatch.GolemStone, EnemySwatch.Black, "MonkeyHind");
            }
        }

        private static void DrawMonkeyBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.32f, 0.56f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.07f, 0.06f), new Vector3(0.24f, 0.18f, 0.4f), EnemySwatch.SandstoneLight, coarse: true);
            b.Blob(new Vector3(0f, 0.08f, -0.02f), new Vector3(0.26f, 0.14f, 0.42f), EnemySwatch.GolemStoneDark, coarse: true);
        }

        private static void DrawMonkeyHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.19f), EnemySwatch.Steel);
            // The black face of the langur under a pale brow fringe, eyes glinting with the shine it steals.
            b.Blob(new Vector3(0f, -0.01f, 0.07f), new Vector3(0.13f, 0.14f, 0.08f), EnemySwatch.Black, coarse: true);
            b.Box(new Vector3(0f, 0.06f, 0.06f), new Vector3(0.18f, 0.04f, 0.06f), EnemySwatch.Steel, Quaternion.Euler(-15f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.035f * s, 0.02f, 0.11f), new Vector3(0.03f, 0.025f, 0.02f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.1f * s, 0.01f, -0.01f), new Vector3(0.04f, 0.06f, 0.03f), EnemySwatch.Black, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Lontra Ladra
        // ------------------------------------------------------------------

        public static void BuildThiefOtter(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawOtterBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.25f, 0.46f), DrawOtterHead);
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.4f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.26f), 0.08f, 0.06f, 6, EnemySwatch.BarkDark));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, -0.06f, -0.26f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.24f), 0.06f, 0.015f, 5, EnemySwatch.BarkDark));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.1f * side, 0.26f, 0.26f), new Vector3(0f, -0.12f, 0.02f), new Vector3(0f, -0.11f, 0.02f),
                    0.045f, EnemySwatch.BarkDark, EnemySwatch.Black, "OtterFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.11f * side, 0.27f, -0.28f), new Vector3(0f, -0.12f, -0.04f), new Vector3(0f, -0.12f, 0.04f),
                    0.05f, EnemySwatch.BarkDark, EnemySwatch.Black, "OtterHind");
            }
        }

        private static void DrawOtterBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.28f, 0.82f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.05f, 0.08f), new Vector3(0.22f, 0.16f, 0.6f), EnemySwatch.Bark, coarse: true);
            // The long neck going up to the head it keeps raised over the reeds, pale throat.
            b.Segment(new Vector3(0f, 0.04f, 0.3f), new Vector3(0f, 0.23f, 0.46f), 0.11f, 0.085f, 6, EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.12f, 0.43f), new Vector3(0.12f, 0.18f, 0.08f), EnemySwatch.WolfBelly, coarse: true);
        }

        private static void DrawOtterHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.03f, 0.04f), new Vector3(0.17f, 0.15f, 0.2f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0f, 0.13f), new Vector3(0.12f, 0.09f, 0.1f), EnemySwatch.WolfBelly, coarse: true);
            b.Box(new Vector3(0f, 0.025f, 0.18f), new Vector3(0.045f, 0.025f, 0.02f), EnemySwatch.Black);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.05f * s, 0.06f, 0.1f), new Vector3(0.03f, 0.025f, 0.02f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.07f * s, 0.09f, 0f), new Vector3(0.04f, 0.035f, 0.025f), EnemySwatch.Bark, coarse: true);
            }

            // The stolen fish across its mouth, still glowing with the lantern light it was caught under.
            b.Blob(new Vector3(0.02f, -0.02f, 0.17f), new Vector3(0.05f, 0.06f, 0.2f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 85f, 0f), coarse: true);
            b.Cylinder(new Vector3(-0.09f, -0.02f, 0.17f), 0.035f, 0f, 0.05f, 3, EnemySwatch.Gold, rotation: Quaternion.Euler(0f, 0f, 90f));
        }

        // ------------------------------------------------------------------
        // Píton Apagada
        // ------------------------------------------------------------------

        /// <summary>Python tail joints, each relative to the previous: a wide S behind the raised neck (same count as the sand snake's).</summary>
        private static readonly Vector3[] PythonSteps =
        {
            new Vector3(0.36f, 0f, -0.2f),
            new Vector3(0.06f, 0f, -0.36f),
            new Vector3(-0.46f, 0f, -0.16f),
            new Vector3(-0.16f, 0f, -0.3f),
        };

        private static readonly float[] PythonRadii = { 0.12f, 0.11f, 0.095f, 0.075f, 0.02f };

        public static void BuildPython(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.12f, 0.3f), DrawPythonNeck);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.56f, 0.16f), DrawPythonHead);

            int parent = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.06f), b => DrawPythonSegment(b, 0));
            for (int i = 1; i < OverworldBeastRecipes.SnakeTailSegments; i++)
            {
                int index = i;
                parent = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + i, parent, PythonSteps[i - 1], b => DrawPythonSegment(b, index));
            }
        }

        private static void DrawPythonNeck(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.28f, 0.24f, 0.3f), EnemySwatch.Bark);
            b.Segment(new Vector3(0f, 0.02f, 0f), new Vector3(0f, 0.3f, 0.07f), 0.12f, 0.095f, 6, EnemySwatch.Bark);
            b.Segment(new Vector3(0f, 0.3f, 0.07f), new Vector3(0f, 0.54f, 0.14f), 0.095f, 0.075f, 6, EnemySwatch.Bark);
            b.Blob(new Vector3(0f, 0.26f, 0.13f), new Vector3(0.12f, 0.32f, 0.06f), EnemySwatch.WolfBelly, coarse: true);
            for (int k = 0; k < 3; k++)
            {
                b.Blob(new Vector3(0f, 0.1f + k * 0.15f, -0.07f + k * 0.03f), new Vector3(0.12f, 0.08f, 0.05f), EnemySwatch.BarkDark, coarse: true);
            }
        }

        private static void DrawPythonHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.07f), new Vector3(0.17f, 0.1f, 0.24f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, 0.045f, 0.05f), new Vector3(0.12f, 0.05f, 0.18f), EnemySwatch.BarkDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.1f), new Vector3(0.03f, 0.03f, 0.025f), EnemySwatch.MoonEye, coarse: true);
            }

            b.Box(new Vector3(0f, -0.01f, 0.22f), new Vector3(0.01f, 0.005f, 0.07f), EnemySwatch.Pink);
            b.Box(new Vector3(0.012f, -0.01f, 0.26f), new Vector3(0.008f, 0.005f, 0.03f), EnemySwatch.Pink, Quaternion.Euler(0f, 25f, 0f));
            b.Box(new Vector3(-0.012f, -0.01f, 0.26f), new Vector3(0.008f, 0.005f, 0.03f), EnemySwatch.Pink, Quaternion.Euler(0f, -25f, 0f));
        }

        private static void DrawPythonSegment(EnemyMeshBuilder b, int index)
        {
            Vector3 end = index < PythonSteps.Length ? PythonSteps[index] : Vector3.zero;
            float r0 = PythonRadii[index];
            float r1 = PythonRadii[index + 1];
            b.Segment(Vector3.zero, end, r0, r1, 6, EnemySwatch.Bark);
            b.Blob(Vector3.zero, Vector3.one * (r0 * 2f), EnemySwatch.Bark, coarse: true);
            // The dark saddles of the python's pattern, with olive between them.
            b.Blob(end * 0.3f + new Vector3(0f, (r0 + r1) * 0.42f, 0f), new Vector3(0.1f, 0.04f, 0.12f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(end * 0.7f + new Vector3(0f, (r0 + r1) * 0.42f, 0f), new Vector3(0.09f, 0.04f, 0.1f), EnemySwatch.Lichen, coarse: true);
            if (index == OverworldBeastRecipes.SnakeTailSegments - 1)
            {
                b.Segment(end, end + end.normalized * 0.12f, r1, 0.004f, 4, EnemySwatch.BarkDark);
                b.Blob(end + end.normalized * 0.07f, new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.MoonEye, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Trol Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedTroll(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.05f, 0f), DrawTrollBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 0.16f), DrawTrollHead);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.62f, -0.36f), b =>
                b.Blob(Vector3.zero, new Vector3(0.14f, 0.17f, 0.14f), EnemySwatch.StolenLight, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.48f * side, 0.4f, 0.02f),
                    b => DrawTrollArm(b, side), Quaternion.Euler(-6f, 0f, 0f));

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1, new Vector3(0.22f * side, 0.66f, 0f),
                    b => b.Segment(new Vector3(0f, -0.32f, 0.04f), new Vector3(0f, 0.04f, 0f), 0.11f, 0.14f, 6, EnemySwatch.CaveMoss), meshKey: "TrollThigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg, new Vector3(0f, -0.32f, 0.04f),
                    b => b.Segment(new Vector3(0f, -0.3f, -0.02f), Vector3.zero, 0.085f, 0.1f, 6, EnemySwatch.CaveMoss), meshKey: "TrollShin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -0.3f, -0.02f),
                    b => b.Blob(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.2f, 0.1f, 0.28f), EnemySwatch.CaveSkinDark, coarse: true), meshKey: "TrollFoot");
            }
        }

        private static void DrawTrollBody(EnemyMeshBuilder b)
        {
            // A hunched mossy body, reed skirt and belt; a hump of swamp moss and reeds on the back.
            b.Blob(new Vector3(0f, 0.25f, 0f), new Vector3(0.8f, 0.85f, 0.56f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, 0.08f, 0.1f), new Vector3(0.6f, 0.5f, 0.4f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(0f, 0.44f, -0.14f), new Vector3(0.62f, 0.4f, 0.32f), EnemySwatch.CaveSkinDark, coarse: true);
            b.Box(new Vector3(0f, -0.2f, 0f), new Vector3(0.62f, 0.26f, 0.46f), EnemySwatch.Bark);
            b.Box(new Vector3(0f, -0.06f, 0f), new Vector3(0.66f, 0.07f, 0.5f), EnemySwatch.BarkDark);

            // The wicker cage on its back with the will-o'-wisp it caught (StolenLight sits inside): four bars and a lid.
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                b.Box(new Vector3(Mathf.Sin(a) * 0.1f, 0.62f, -0.36f + Mathf.Cos(a) * 0.1f), new Vector3(0.025f, 0.24f, 0.025f), EnemySwatch.BarkDark);
            }

            b.Cylinder(new Vector3(0f, 0.74f, -0.36f), 0.14f, 0f, 0.1f, 6, EnemySwatch.Bark);
            b.Cylinder(new Vector3(0f, 0.48f, -0.36f), 0.13f, 0.13f, 0.03f, 6, EnemySwatch.BarkDark);
        }

        private static void DrawTrollHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.08f, 0.02f), new Vector3(0.36f, 0.32f, 0.34f), EnemySwatch.CaveMoss);
            b.Box(new Vector3(0f, 0.15f, 0.15f), new Vector3(0.3f, 0.05f, 0.08f), EnemySwatch.CaveSkinDark, Quaternion.Euler(-12f, 0f, 0f));
            b.Cylinder(new Vector3(0f, 0.08f, 0.16f), 0.06f, 0.03f, 0.15f, 5, EnemySwatch.CaveSkin, rotation: Quaternion.Euler(100f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                // Pale, sleepy eyes: it forgot its way home.
                b.Blob(new Vector3(0.08f * s, 0.12f, 0.15f), new Vector3(0.05f, 0.03f, 0.03f), EnemySwatch.SoftEye, coarse: true);
                b.Cylinder(new Vector3(0.16f * s, 0.08f, 0f), 0.05f, 0f, 0.18f, 4, EnemySwatch.CaveMoss, rotation: Quaternion.Euler(0f, 0f, -105f * s));
                b.Cylinder(new Vector3(0.07f * s, -0.02f, 0.14f), 0.02f, 0f, 0.07f, 4, EnemySwatch.Fang);
            }

            // Reeds growing out of the top of its head like hair.
            for (int i = 0; i < 4; i++)
            {
                b.Cylinder(new Vector3(-0.08f + i * 0.055f, 0.2f, -0.02f), 0.015f, 0f, 0.16f + 0.03f * (i % 2), 3, EnemySwatch.Lichen,
                    rotation: Quaternion.Euler(-12f, 0f, (i - 1.5f) * 12f));
            }
        }

        private static void DrawTrollArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), EnemySwatch.CaveMoss, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.4f, 0.04f), 0.11f, 0.09f, 6, EnemySwatch.CaveMoss);
            b.Segment(new Vector3(0.06f * side, -0.4f, 0.04f), new Vector3(0.07f * side, -0.74f, 0.08f), 0.09f, 0.085f, 6, EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0.07f * side, -0.84f, 0.1f), new Vector3(0.2f, 0.18f, 0.2f), EnemySwatch.CaveSkin, coarse: true);
        }

        // ------------------------------------------------------------------
        // Shared: heavy legs with hooves/pads
        // ------------------------------------------------------------------

        /// <summary>
        /// A thick <c>Leg_id</c> → <c>Shin_id</c> → <c>Foot_id</c> chain (ProceduralLegLocomotion) with a hoof or pad of
        /// <paramref name="hoofSize"/> whose sole sits 3 cm under the foot pivot.
        /// </summary>
        internal static void AddLimb(ProceduralEnemyBlueprint bp, string id, Vector3 hip, Vector3 knee, Vector3 foot, float radius,
            EnemySwatch fur, EnemySwatch hoof, Vector3 hoofSize, string meshKey)
        {
            int leg = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, hip, b =>
            {
                b.Segment(knee, new Vector3(0f, 0.05f, 0f), radius * 0.8f, radius, 6, fur);
                b.Blob(knee, Vector3.one * (radius * 1.6f), fur, coarse: true);
            }, meshKey: meshKey + "Upper");
            int shin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, leg, knee, b => b.Segment(foot, Vector3.zero, radius * 0.6f, radius * 0.75f, 5, fur),
                meshKey: meshKey + "Lower");
            bp.Mesh(ProceduralEnemyParts.FootPrefix + id, shin, foot, b => b.Blob(new Vector3(0f, hoofSize.y * 0.5f - 0.03f, 0.02f), hoofSize, hoof, coarse: true),
                meshKey: meshKey + "Hoof");
        }
    }
}
