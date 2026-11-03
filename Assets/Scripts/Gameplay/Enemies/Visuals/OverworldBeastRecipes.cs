using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The real animals the map sheets asked for and the continuous worlds played with stand-ins (Docs/Mapas100.md 07, 08,
    /// 13, 15, 24, 25), low-poly from the shared palette like the other recipes. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Chacal Apagado</b> (Areias, ~0.85 m to the ear tips): lean sandy jackal, dark saddle, tall ears, ember eyes.
    /// Jointed like the wolf: <c>Body</c> → <c>Head</c> → <c>Ear_L</c>/<c>Ear_R</c>, <c>Body</c> → <c>Tail</c> → <c>Tail_1</c>,
    /// <c>Leg_FL/FR/HL/HR</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Babuíno Apagado</b> (Kaggen, ~0.8 m): grey-maned baboon on all fours, long pink muzzle, glowing yellow eyes,
    /// arched tail. <c>Body</c> → <c>Head</c>, <c>Body</c> → <c>Tail</c> → <c>Tail_1</c>, four jointed legs.</item>
    /// <item><b>Cobra-de-Areia Apagada</b> (Areias, ~0.7 m raised, 1.2 m long): horned sand viper, forepart up, the rest in an
    /// S on the sand. <c>Body</c> (raised neck) → <c>Head</c>; <c>Body</c> → <c>Tail</c> → <c>Tail_1</c> → <c>Tail_2</c> →
    /// <c>Tail_3</c> (the rig slithers them).</item>
    /// <item><b>Caranguejo-dos-Coqueiros Gigante</b> (Costa das Monções, ~0.7 m, 1.1 m across): red giant crab, one pincer
    /// bigger than the other, eyes on stalks, ember cracks on the shell. <c>Body</c> → <c>Claw_L</c>/<c>Claw_R</c>, eight
    /// jointed legs <c>Leg_L0..R3</c> as in <see cref="SpiderRecipe"/>.</item>
    /// <item><b>Gaivota Ladra</b> (Costa das Monções, ~0.75 m, 1.6 m across): white-and-grey gull, black wing tips, yellow eyes,
    /// a stolen coin in its beak. <c>Body</c> → <c>Head</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Corvo-da-Treva</b> (Limiar, ~0.85 m, 1.8 m across): black raven whose wing tips glow violet, pale cold eyes.
    /// <c>Body</c> → <c>Head</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// </list>
    /// </summary>
    public static class OverworldBeastRecipes
    {
        // ------------------------------------------------------------------
        // Chacal Apagado
        // ------------------------------------------------------------------

        public static void BuildJackal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawJackalBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.38f), DrawJackalHead, Quaternion.Euler(10f, 0f, 0f));
            for (int side = -1; side <= 1; side += 2)
            {
                bp.Mesh(side < 0 ? ProceduralEnemyParts.EarLeft : ProceduralEnemyParts.EarRight, head,
                    new Vector3(0.06f * side, 0.08f, -0.02f), DrawJackalEar, Quaternion.Euler(-8f, 0f, -12f * side), meshKey: "JackalEar");
            }

            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.05f, -0.36f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.1f, -0.16f), 0.035f, 0.05f, 5, EnemySwatch.Sandstone));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, -0.1f, -0.16f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.08f, -0.14f), 0.05f, 0.015f, 5, EnemySwatch.Shadow));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                AddLeg(bp, "F" + x, new Vector3(0.09f * side, 0.42f, 0.24f), new Vector3(0f, -0.2f, -0.02f), new Vector3(0f, -0.19f, 0.03f),
                    0.045f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, "JackalFront");
                AddLeg(bp, "H" + x, new Vector3(0.09f * side, 0.44f, -0.26f), new Vector3(0f, -0.21f, -0.06f), new Vector3(0f, -0.2f, 0.06f),
                    0.05f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, "JackalHind");
            }
        }

        private static void DrawJackalBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.3f, 0.72f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, -0.07f, 0.02f), new Vector3(0.22f, 0.16f, 0.56f), EnemySwatch.SandstoneLight, coarse: true);
            // The dark saddle of the black-backed jackal, with a few cinders of trapped light caught in it.
            b.Blob(new Vector3(0f, 0.08f, -0.04f), new Vector3(0.24f, 0.16f, 0.5f), EnemySwatch.Shadow, coarse: true);
            b.Blob(new Vector3(0.05f, 0.15f, 0.06f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.04f, 0.14f, -0.14f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.Ember, coarse: true);
        }

        private static void DrawJackalHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.2f, 0.18f, 0.2f), EnemySwatch.Sandstone);
            b.Cylinder(new Vector3(0f, -0.01f, 0.08f), 0.06f, 0.028f, 0.18f, 6, EnemySwatch.SandstoneLight, EnemySwatch.Black, Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.01f, 0.265f), new Vector3(0.04f, 0.035f, 0.03f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.055f * s, 0.05f, 0.09f), new Vector3(0.04f, 0.03f, 0.03f), EnemySwatch.Ember, coarse: true);
            }
        }

        private static void DrawJackalEar(EnemyMeshBuilder b)
        {
            b.Cylinder(Vector3.zero, 0.05f, 0f, 0.17f, 4, EnemySwatch.SandstoneDark);
            b.Cylinder(new Vector3(0f, 0.01f, 0.012f), 0.03f, 0f, 0.12f, 4, EnemySwatch.Pink);
        }

        // ------------------------------------------------------------------
        // Babuíno Apagado
        // ------------------------------------------------------------------

        public static void BuildBaboon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawBaboonBody, Quaternion.Euler(-10f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.36f), DrawBaboonHead, Quaternion.Euler(14f, 0f, 0f));
            int tail = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.3f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, 0.12f, -0.1f), 0.035f, 0.03f, 5, EnemySwatch.WolfFur));
            bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + "1", tail, new Vector3(0f, 0.12f, -0.1f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.2f, -0.12f), 0.03f, 0.02f, 5, EnemySwatch.WolfFur);
                b.Blob(new Vector3(0f, -0.2f, -0.12f), new Vector3(0.05f, 0.06f, 0.05f), EnemySwatch.WolfFurDark, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                // Long arms in front (the shoulders ride high), short bent hind legs.
                AddLeg(bp, "F" + x, new Vector3(0.13f * side, 0.48f, 0.22f), new Vector3(0f, -0.24f, 0.02f), new Vector3(0f, -0.21f, 0f),
                    0.05f, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, "BaboonFront");
                AddLeg(bp, "H" + x, new Vector3(0.12f * side, 0.42f, -0.24f), new Vector3(0f, -0.2f, 0.06f), new Vector3(0f, -0.19f, -0.04f),
                    0.055f, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, "BaboonHind");
            }
        }

        private static void DrawBaboonBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.36f, 0.62f), EnemySwatch.WolfFur);
            // The grey mane over the shoulders, the pale chest and the bare pink rump.
            b.Blob(new Vector3(0f, 0.08f, 0.18f), new Vector3(0.44f, 0.4f, 0.36f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(0f, -0.1f, 0.12f), new Vector3(0.24f, 0.16f, 0.26f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, -0.02f, -0.3f), new Vector3(0.16f, 0.14f, 0.06f), EnemySwatch.Pink, coarse: true);
        }

        private static void DrawBaboonHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), EnemySwatch.WolfFurDark);
            b.Box(new Vector3(0f, 0.05f, 0.08f), new Vector3(0.18f, 0.04f, 0.06f), EnemySwatch.Shadow, Quaternion.Euler(-10f, 0f, 0f));
            // The long dog-like muzzle, bare and pink, with black nostrils at the tip.
            b.Cylinder(new Vector3(0f, -0.03f, 0.06f), 0.07f, 0.05f, 0.17f, 6, EnemySwatch.Pink, EnemySwatch.Pink, Quaternion.Euler(98f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.05f * s, 0.03f, 0.1f), new Vector3(0.035f, 0.03f, 0.025f), EnemySwatch.MoonEye, coarse: true);
                b.Box(new Vector3(0.018f * s, -0.055f, 0.235f), new Vector3(0.016f, 0.02f, 0.008f), EnemySwatch.Black);
                b.Cylinder(new Vector3(0.03f * s, -0.07f, 0.16f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Cobra-de-Areia Apagada
        // ------------------------------------------------------------------

        /// <summary>Snake tail joints, each relative to the previous: the body lies in an S on the sand behind the raised neck.</summary>
        private static readonly Vector3[] SnakeSteps =
        {
            new Vector3(0.26f, 0f, -0.14f),
            new Vector3(0.04f, 0f, -0.26f),
            new Vector3(-0.34f, 0f, -0.12f),
            new Vector3(-0.12f, 0f, -0.22f),
        };

        private static readonly float[] SnakeRadii = { 0.085f, 0.075f, 0.062f, 0.048f, 0.012f };

        public const int SnakeTailSegments = 4;

        public static void BuildSandSnake(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.09f, 0.2f), DrawSnakeNeck);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.5f, 0.12f), DrawSnakeHead);

            int parent = bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.06f), b => DrawSnakeSegment(b, 0));
            for (int i = 1; i < SnakeTailSegments; i++)
            {
                int index = i;
                parent = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + i, parent, SnakeSteps[i - 1], b => DrawSnakeSegment(b, index));
            }
        }

        private static void DrawSnakeNeck(EnemyMeshBuilder b)
        {
            // From the coil on the sand up to the raised head, scaled sand with dark diamonds.
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.2f, 0.18f, 0.22f), EnemySwatch.Sandstone);
            b.Segment(new Vector3(0f, 0.02f, 0f), new Vector3(0f, 0.26f, 0.06f), 0.085f, 0.07f, 6, EnemySwatch.Sandstone);
            b.Segment(new Vector3(0f, 0.26f, 0.06f), new Vector3(0f, 0.48f, 0.11f), 0.07f, 0.055f, 6, EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, 0.2f, 0.11f), new Vector3(0.1f, 0.22f, 0.05f), EnemySwatch.SandstoneLight, coarse: true);
            for (int k = 0; k < 3; k++)
            {
                b.Box(new Vector3(0f, 0.1f + k * 0.13f, 0.0f + k * 0.02f - 0.055f), new Vector3(0.05f, 0.05f, 0.03f), EnemySwatch.SandstoneDark,
                    Quaternion.Euler(0f, 0f, 45f));
            }
        }

        private static void DrawSnakeHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.06f), new Vector3(0.15f, 0.09f, 0.2f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.11f, 0.05f, 0.14f), EnemySwatch.SandstoneLight, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // The little horns over the eyes (a horned viper of the dunes) and the ember eyes under them.
                b.Horn(new Vector3(0.04f * s, 0.06f, 0.08f), 0.014f, 0.07f, 2, -12f, EnemySwatch.Bone, Quaternion.Euler(-10f, 0f, 10f * s), 4);
                b.Blob(new Vector3(0.05f * s, 0.045f, 0.1f), new Vector3(0.03f, 0.03f, 0.025f), EnemySwatch.Ember, coarse: true);
            }

            // Forked tongue.
            b.Box(new Vector3(0f, -0.01f, 0.2f), new Vector3(0.01f, 0.005f, 0.06f), EnemySwatch.Pink);
            b.Box(new Vector3(0.012f, -0.01f, 0.235f), new Vector3(0.008f, 0.005f, 0.025f), EnemySwatch.Pink, Quaternion.Euler(0f, 25f, 0f));
            b.Box(new Vector3(-0.012f, -0.01f, 0.235f), new Vector3(0.008f, 0.005f, 0.025f), EnemySwatch.Pink, Quaternion.Euler(0f, -25f, 0f));
        }

        private static void DrawSnakeSegment(EnemyMeshBuilder b, int index)
        {
            Vector3 end = index < SnakeSteps.Length ? SnakeSteps[index] : Vector3.zero;
            float r0 = SnakeRadii[index];
            float r1 = SnakeRadii[index + 1];
            b.Segment(Vector3.zero, end, r0, r1, 6, EnemySwatch.Sandstone);
            b.Blob(Vector3.zero, Vector3.one * (r0 * 2f), EnemySwatch.Sandstone, coarse: true);
            // A dark diamond on the back of each piece.
            b.Box(end * 0.5f + new Vector3(0f, (r0 + r1) * 0.45f, 0f), new Vector3(0.05f, 0.02f, 0.05f), EnemySwatch.SandstoneDark, Quaternion.Euler(0f, 45f, 0f));
            if (index == SnakeTailSegments - 1)
            {
                // The tail tip: a short dark cone and a faint ember at the very end (the light it swallowed).
                b.Segment(end, end + end.normalized * 0.1f, r1, 0.004f, 4, EnemySwatch.SandstoneDark);
                b.Blob(end + end.normalized * 0.06f, new Vector3(0.025f, 0.025f, 0.025f), EnemySwatch.Ember, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Caranguejo-dos-Coqueiros Gigante
        // ------------------------------------------------------------------

        public const int CrabLegPairs = 4;

        private const float CrabHipHeight = 0.3f;
        private static readonly float[] CrabHipZ = { 0.12f, 0.04f, -0.05f, -0.14f };
        private static readonly float[] CrabSplay = { 40f, 12f, -15f, -42f };
        private static readonly Vector3 CrabKnee = new Vector3(0.22f, 0.14f, 0f);
        private static readonly Vector3 CrabTip = new Vector3(0.18f, -(CrabHipHeight + 0.14f - 0.01f), 0f);

        public static void BuildCoconutCrab(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, CrabHipHeight, 0f), DrawCrabBody);
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                // The left pincer is the big one, like a real coconut crab.
                float size = side < 0 ? 1.25f : 0.9f;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body,
                    new Vector3(0.2f * side, 0.02f, 0.2f), b => DrawCrabClaw(b, sd, size));
            }

            for (int pair = 0; pair < CrabLegPairs; pair++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    string id = (side < 0 ? "L" : "R") + pair;
                    float yaw = side > 0 ? -CrabSplay[pair] : 180f + CrabSplay[pair];
                    int hip = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, new Vector3(0.16f * side, CrabHipHeight, CrabHipZ[pair]), DrawCrabUpper,
                        Quaternion.Euler(0f, yaw, 0f), meshKey: "CrabUpper");
                    int knee = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, hip, CrabKnee, DrawCrabLower, meshKey: "CrabLower");
                    bp.Node(ProceduralEnemyParts.FootPrefix + id, knee, CrabTip);
                }
            }
        }

        private static void DrawCrabBody(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.56f, 0.28f, 0.46f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, 0.1f, -0.02f), new Vector3(0.44f, 0.16f, 0.36f), EnemySwatch.Copper, coarse: true);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.44f, 0.12f, 0.38f), EnemySwatch.CopperDark, coarse: true);
            // The soft curled abdomen tucked under the back.
            b.Blob(new Vector3(0f, -0.02f, -0.22f), new Vector3(0.3f, 0.18f, 0.2f), EnemySwatch.CopperDark, coarse: true);
            // Ember cracks across the shell: the light the shadow trapped in it.
            b.Box(new Vector3(0.05f, 0.18f, 0f), new Vector3(0.03f, 0.02f, 0.22f), EnemySwatch.Ember, Quaternion.Euler(0f, 20f, 0f));
            b.Box(new Vector3(-0.08f, 0.17f, -0.04f), new Vector3(0.03f, 0.02f, 0.14f), EnemySwatch.Ember, Quaternion.Euler(0f, -35f, 0f));
            // Eyes on stalks and the mouthparts.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.05f * s, 0.12f, 0.2f), new Vector3(0.07f * s, 0.32f, 0.24f), 0.016f, 0.012f, 4, EnemySwatch.WarCape);
                b.Blob(new Vector3(0.07f * s, 0.34f, 0.24f), new Vector3(0.045f, 0.045f, 0.045f), EnemySwatch.MoonEye, coarse: true);
                b.Segment(new Vector3(0.03f * s, 0f, 0.22f), new Vector3(0.06f * s, 0.1f, 0.3f), 0.01f, 0.004f, 3, EnemySwatch.Black);
            }

            b.Box(new Vector3(0f, -0.02f, 0.23f), new Vector3(0.12f, 0.06f, 0.03f), EnemySwatch.Black);
        }

        private static void DrawCrabClaw(EnemyMeshBuilder b, float side, float size)
        {
            b.Matrix = Matrix4x4.Scale(Vector3.one * size);
            b.Segment(Vector3.zero, new Vector3(0.1f * side, 0.02f, 0.14f), 0.045f, 0.04f, 5, EnemySwatch.WarCape);
            b.Blob(new Vector3(0.12f * side, 0.03f, 0.24f), new Vector3(0.14f, 0.12f, 0.2f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0.12f * side, 0.07f, 0.24f), new Vector3(0.1f, 0.05f, 0.14f), EnemySwatch.Copper, coarse: true);
            // The two fingers of the pincer, tipped in black.
            b.Cylinder(new Vector3(0.1f * side, 0.05f, 0.32f), 0.03f, 0.008f, 0.12f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(80f, 0f, 0f));
            b.Cylinder(new Vector3(0.14f * side, 0.0f, 0.32f), 0.026f, 0.006f, 0.1f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(95f, 0f, 0f));
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawCrabUpper(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.07f, 0.07f, 0.07f), EnemySwatch.CopperDark, coarse: true);
            b.Segment(Vector3.zero, CrabKnee, 0.035f, 0.03f, 5, EnemySwatch.WarCape);
            b.Blob(CrabKnee, new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.Copper, coarse: true);
        }

        private static void DrawCrabLower(EnemyMeshBuilder b)
        {
            Vector3 mid = CrabTip * 0.5f;
            b.Segment(Vector3.zero, mid, 0.028f, 0.022f, 5, EnemySwatch.WarCape);
            b.Segment(mid, CrabTip, 0.022f, 0.006f, 4, EnemySwatch.CopperDark);
        }

        // ------------------------------------------------------------------
        // Gaivota Ladra
        // ------------------------------------------------------------------

        public static void BuildThiefGull(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.36f, 0f), DrawGullBody, Quaternion.Euler(-8f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.12f), DrawGullHead);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.14f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Steel, EnemySwatch.OwlFace, EnemySwatch.Black, 0.8f),
                Quaternion.Euler(0f, 0f, -22f), meshKey: "GullWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.14f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Steel, EnemySwatch.OwlFace, EnemySwatch.Black, 0.8f),
                Quaternion.Euler(0f, 0f, 22f), meshKey: "GullWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.2f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.OwlFace, EnemySwatch.Steel),
                Quaternion.Euler(20f, 0f, 0f));
        }

        private static void DrawGullBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.34f, 0.46f), EnemySwatch.OwlFace);
            b.Blob(new Vector3(0f, 0.08f, -0.05f), new Vector3(0.27f, 0.14f, 0.38f), EnemySwatch.Steel, coarse: true);
            RangedEnemyRecipes.DrawTalons(b, -0.36f, 0.06f, EnemySwatch.Gold);
        }

        private static void DrawGullHead(EnemyMeshBuilder b)
        {
            b.Segment(new Vector3(0f, -0.06f, -0.03f), new Vector3(0f, 0.04f, 0.01f), 0.06f, 0.055f, 5, EnemySwatch.OwlFace);
            b.Blob(new Vector3(0f, 0.07f, 0.03f), new Vector3(0.16f, 0.15f, 0.18f), EnemySwatch.OwlFace);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.05f * s, 0.1f, 0.08f), new Vector3(0.03f, 0.03f, 0.025f), EnemySwatch.MoonEye, coarse: true);
            }

            // Yellow hooked beak with the red spot, and the stolen coin held in it.
            b.Cylinder(new Vector3(0f, 0.06f, 0.1f), 0.028f, 0.008f, 0.12f, 4, EnemySwatch.Gold, rotation: Quaternion.Euler(95f, 0f, 0f));
            b.Blob(new Vector3(0f, 0.045f, 0.17f), new Vector3(0.02f, 0.02f, 0.02f), EnemySwatch.WarEye, coarse: true);
            b.Cylinder(new Vector3(0f, 0.03f, 0.19f), 0.04f, 0.04f, 0.012f, 8, EnemySwatch.Gold, rotation: Quaternion.Euler(0f, 0f, 90f));
        }

        // ------------------------------------------------------------------
        // Corvo-da-Treva
        // ------------------------------------------------------------------

        public static void BuildDarkCrow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawCrowBody, Quaternion.Euler(-10f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.27f, 0.12f), DrawCrowHead);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.16f, 0.12f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.Sorcery, 0.95f),
                Quaternion.Euler(0f, 0f, -20f), meshKey: "CrowWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.16f, 0.12f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.Sorcery, 0.95f),
                Quaternion.Euler(0f, 0f, 20f), meshKey: "CrowWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.14f, -0.2f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.Shadow),
                Quaternion.Euler(25f, 0f, 0f));
        }

        private static void DrawCrowBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.46f, 0.48f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, 0.04f, -0.04f), new Vector3(0.3f, 0.3f, 0.4f), EnemySwatch.BatFurDark, coarse: true);
            // The shaggy throat feathers of a raven.
            b.Blob(new Vector3(0f, 0.16f, 0.14f), new Vector3(0.22f, 0.14f, 0.16f), EnemySwatch.Shadow, coarse: true);
            RangedEnemyRecipes.DrawTalons(b, -0.4f, 0.07f, EnemySwatch.IronDark);
        }

        private static void DrawCrowHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.07f, 0f), new Vector3(0.2f, 0.19f, 0.22f), EnemySwatch.Black);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.062f * s, 0.1f, 0.07f), new Vector3(0.035f, 0.035f, 0.03f), EnemySwatch.SoftEye, coarse: true);
            }

            // The heavy raven bill, and a violet glint at the corner of the mouth.
            b.Cylinder(new Vector3(0f, 0.07f, 0.09f), 0.04f, 0.006f, 0.15f, 5, EnemySwatch.IronDark, rotation: Quaternion.Euler(97f, 0f, 0f));
            b.Blob(new Vector3(0f, 0.05f, 0.12f), new Vector3(0.025f, 0.02f, 0.02f), EnemySwatch.Sorcery, coarse: true);
        }

        // ------------------------------------------------------------------
        // Shared: jointed legs
        // ------------------------------------------------------------------

        /// <summary>One <c>Leg_id</c> (hip) → <c>Shin_id</c> (knee) → <c>Foot_id</c> (paw) chain under the root, for ProceduralLegLocomotion.</summary>
        internal static void AddLeg(ProceduralEnemyBlueprint bp, string id, Vector3 hip, Vector3 knee, Vector3 foot, float radius,
            EnemySwatch fur, EnemySwatch paw, string meshKey)
        {
            int leg = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, hip, b =>
            {
                b.Segment(knee, new Vector3(0f, 0.04f, 0f), radius * 0.7f, radius, 5, fur);
                b.Blob(knee, Vector3.one * (radius * 1.5f), fur, coarse: true);
            }, meshKey: meshKey + "Upper");
            int shin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, leg, knee, b => b.Segment(foot, Vector3.zero, radius * 0.5f, radius * 0.65f, 4, fur),
                meshKey: meshKey + "Lower");
            bp.Mesh(ProceduralEnemyParts.FootPrefix + id, shin, foot, b => b.Blob(new Vector3(0f, -0.01f, 0.015f), new Vector3(0.06f, 0.045f, 0.075f), paw, coarse: true),
                meshKey: meshKey + "Paw");
        }
    }
}
