using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Rio das Doze Horas (Docs/Mapas100.md 71–74) that the continuous world played with stand-ins, low-poly from the
    /// shared palette like <see cref="HinokuraBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Crocodilo do Rio Escuro</b> (71, ~0.9 m, 3 m long): night-blue crocodile with gold eyes and a line of gold under the scutes.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four heavy legs.</item>
    /// <item><b>Íbis Apagada</b> (71, ~0.9 m): grey ibis with a black head and a long down-curved bill.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Escaravelho de Sombra</b> (71, 73, 76, ~0.7 m): black scarab with gold seams and a dim sun held on its brow.
    /// <c>Body</c>, <c>Claw_L</c>/<c>Claw_R</c> (mandibles); six legs.</item>
    /// <item><b>Junco-Laço</b> (71, ~1.3 m): a sheaf of reeds that drifts on its root, nooses of reed at both sides.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (nooses), <c>Tail</c> (the root).</item>
    /// <item><b>Gafanhoto de Sombra</b> (72, ~0.75 m): dark locust with glassy wings and long hind legs drawn on the body.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Hipopótamo de Lodo</b> (72, ~1.3 m): barrel of grey mud with a huge square mouth and reeds growing on its back.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four heavy legs.</item>
    /// <item><b>Cobra do Junco</b> (72, ~0.7 m): reed-green snake, raised head and a jaw that strikes.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c> → <c>Tail_1..3</c>.</item>
    /// <item><b>Serpente de Três Cabeças</b> (73, ~0.9 m): heavy sand-gold serpent with three raised heads.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Head_L</c>/<c>Head_R</c>, <c>Tail</c> → <c>Tail_1..3</c>.</item>
    /// <item><b>Escorpião Negro</b> (73, ~0.9 m with the sting): black scorpion, gold bands on the tail and an ember sting.
    /// <c>Body</c>, <c>Claw_L</c>/<c>Claw_R</c>, <c>Tail</c>; eight legs.</item>
    /// <item><b>Ushabti de Areia</b> (73, ~1.5 m): servant figure of packed sand with a hoe, a column of signs on its chest.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Mordedor de Sombra</b> (74, ~0.85 m): lean dog cut out of the dark, all mouth, a bright seam down its back.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Treva Rastejante</b> (74, ~0.75 m): low heap of dark with crawling tendrils and swallowed gold inside.
    /// <c>Body</c> → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class RioBeastRecipes
    {
        /// <summary>The Three-Headed Serpent's two side heads (the rig sways them; the middle one is <see cref="ProceduralEnemyParts.Head"/>).</summary>
        public const string SerpentHeadLeft = "Head_L";
        public const string SerpentHeadRight = "Head_R";

        // ------------------------------------------------------------------
        // Shared
        // ------------------------------------------------------------------

        /// <summary>Jointed insect legs splayed out from the body: <c>Leg_L0</c> → <c>Shin_L0</c> → <c>Foot_L0</c>, the foot on the ground.</summary>
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
                        b.Blob(Vector3.zero, new Vector3(0.08f, 0.08f, 0.08f), lower, coarse: true);
                        b.Segment(Vector3.zero, knee, 0.038f, 0.03f, 5, upper);
                        b.Blob(knee, new Vector3(0.06f, 0.06f, 0.06f), lower, coarse: true);
                    }, Quaternion.Euler(0f, yaw, 0f), meshKey: meshKey + "Upper");
                    int shin = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, hip, knee, b =>
                    {
                        Vector3 mid = tip * 0.5f;
                        b.Segment(Vector3.zero, mid, 0.028f, 0.022f, 5, upper);
                        b.Segment(mid, tip, 0.022f, 0.006f, 4, lower);
                    }, meshKey: meshKey + "Lower");
                    bp.Node(ProceduralEnemyParts.FootPrefix + id, shin, tip);
                }
            }
        }

        /// <summary>The tail of a snake as a chain the rig can wave: <c>Tail</c> → <c>Tail_1</c> → … (<see cref="OverworldBeastRecipes.SnakeTailSegments"/> in all).</summary>
        private static void AddSnakeTail(ProceduralEnemyBlueprint bp, int body, Vector3 origin, Vector3[] steps, float[] radii, EnemySwatch scale, EnemySwatch mark, EnemySwatch tip)
        {
            int parent = bp.Mesh(ProceduralEnemyParts.Tail, body, origin, b => DrawSnakeSegment(b, 0, steps, radii, scale, mark, tip));
            for (int i = 1; i < OverworldBeastRecipes.SnakeTailSegments; i++)
            {
                int index = i;
                parent = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + i, parent, steps[i - 1], b => DrawSnakeSegment(b, index, steps, radii, scale, mark, tip));
            }
        }

        private static void DrawSnakeSegment(EnemyMeshBuilder b, int index, Vector3[] steps, float[] radii, EnemySwatch scale, EnemySwatch mark, EnemySwatch tip)
        {
            Vector3 end = steps[index];
            float r0 = radii[index];
            float r1 = radii[index + 1];
            b.Segment(Vector3.zero, end, r0, r1, 6, scale);
            b.Blob(Vector3.zero, Vector3.one * (r0 * 2f), scale, coarse: true);
            b.Box(end * 0.5f + new Vector3(0f, (r0 + r1) * 0.45f, 0f), new Vector3(0.06f, 0.02f, 0.06f), mark, Quaternion.Euler(0f, 45f, 0f));
            if (index == OverworldBeastRecipes.SnakeTailSegments - 1)
            {
                b.Segment(end, end + end.normalized * 0.1f, r1, 0.004f, 4, mark);
                b.Blob(end + end.normalized * 0.06f, new Vector3(0.03f, 0.03f, 0.03f), tip, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Crocodilo do Rio Escuro
        // ------------------------------------------------------------------

        public static void BuildDarkRiverCrocodile(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.48f, 0f), DrawCrocodileBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.8f), DrawCrocodileHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.24f), new Vector3(0.26f, 0.05f, 0.5f), EnemySwatch.CaveSkinDark);
                b.Box(new Vector3(0f, 0.02f, 0.22f), new Vector3(0.18f, 0.012f, 0.42f), EnemySwatch.Pink);
                for (int i = 0; i < 4; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.11f * s, 0.01f, 0.08f + i * 0.12f), 0.014f, 0f, 0.05f, 3, EnemySwatch.Fang);
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.76f), DrawCrocodileTail);
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.36f, 0.36f, 0.44f, -0.44f, 0.09f, EnemySwatch.CaveSkinDark, EnemySwatch.Shadow, "DarkRiverCrocodile");
        }

        private static void DrawCrocodileBody(EnemyMeshBuilder b)
        {
            // Black-blue as the river at the sixth hour; the scutes go down the back in a double row, and the gold shows between them.
            b.Blob(Vector3.zero, new Vector3(0.84f, 0.48f, 1.7f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.7f, 0.26f, 1.5f), EnemySwatch.CaveBelly, coarse: true);
            for (int i = 0; i < 7; i++)
            {
                float z = -0.66f + i * 0.22f;
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.13f * s, 0.23f, z), new Vector3(0.14f, 0.09f, 0.16f), EnemySwatch.Shadow, Quaternion.Euler(0f, 0f, -14f * s));
                }

                b.Box(new Vector3(0f, 0.22f, z), new Vector3(0.03f, 0.02f, 0.12f), EnemySwatch.StolenLight);
            }

            b.Segment(new Vector3(0f, 0f, 0.7f), new Vector3(0f, 0.02f, 0.8f), 0.24f, 0.2f, 6, EnemySwatch.CaveSkinDark);
        }

        private static void DrawCrocodileHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.4f, 0.26f, 0.4f), EnemySwatch.CaveSkinDark);
            b.Box(new Vector3(0f, 0.01f, 0.32f), new Vector3(0.28f, 0.1f, 0.56f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, 0.05f, 0.58f), new Vector3(0.14f, 0.07f, 0.1f), EnemySwatch.Shadow, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.14f, 0.13f, 0.06f, 0.07f, EnemySwatch.MoonEye);
            for (int i = 0; i < 4; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.12f * s, -0.04f, 0.14f + i * 0.13f), 0.014f, 0f, 0.05f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                }
            }
        }

        private static void DrawCrocodileTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.5f), 0.22f, 0.11f, 6, EnemySwatch.CaveSkinDark);
            b.Segment(new Vector3(0f, -0.04f, -0.5f), new Vector3(0f, -0.1f, -0.9f), 0.11f, 0.02f, 5, EnemySwatch.Shadow);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.12f - i * 0.03f, -0.1f - i * 0.2f), new Vector3(0.03f, 0.1f - i * 0.015f, 0.12f), EnemySwatch.Shadow);
            }
        }

        // ------------------------------------------------------------------
        // Íbis Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedIbis(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawIbisBody, Quaternion.Euler(-8f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.32f, 0.2f), DrawIbisHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.01f, 0.1f), b =>
                b.Horn(Vector3.zero, 0.016f, 0.3f, 3, 14f, EnemySwatch.Black, Quaternion.Euler(95f, 0f, 0f), 4, Vector3.right));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.GolemStone, EnemySwatch.Shroud, EnemySwatch.Black, 0.9f),
                Quaternion.Euler(0f, 0f, -22f), meshKey: "DimmedIbisWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.GolemStone, EnemySwatch.Shroud, EnemySwatch.Black, 0.9f),
                Quaternion.Euler(0f, 0f, 22f), meshKey: "DimmedIbisWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.2f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.Shroud),
                Quaternion.Euler(20f, 0f, 0f));
        }

        private static void DrawIbisBody(EnemyMeshBuilder b)
        {
            // It was white. The shadow took the white and left the ash; only one gold feather stayed lit on its back.
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.34f, 0.5f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.07f, -0.08f), new Vector3(0.24f, 0.16f, 0.36f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Box(new Vector3(0.02f, 0.17f, -0.06f), new Vector3(0.03f, 0.015f, 0.16f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 12f, 0f));
            b.Segment(new Vector3(0f, 0.1f, 0.14f), new Vector3(0f, 0.32f, 0.2f), 0.07f, 0.045f, 5, EnemySwatch.Black);
            RangedEnemyRecipes.DrawTalons(b, -0.44f, 0.06f, EnemySwatch.Black);
        }

        private static void DrawIbisHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.13f, 0.12f, 0.17f), EnemySwatch.Black);
            HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.05f, 0.05f, 0.03f, EnemySwatch.MoonEye);
            b.Horn(new Vector3(0f, 0.04f, 0.08f), 0.02f, 0.32f, 3, 12f, EnemySwatch.Black, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
        }

        // ------------------------------------------------------------------
        // Escaravelho de Sombra
        // ------------------------------------------------------------------

        private const float ScarabHip = 0.34f;
        private static readonly float[] ScarabHipZ = { 0.16f, 0.02f, -0.16f };
        private static readonly float[] ScarabSplay = { 34f, 4f, -32f };

        public static void BuildShadowScarab(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, ScarabHip, 0f), DrawScarabBody);
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body, new Vector3(0.08f * side, 0.02f, 0.42f),
                    b => DrawScarabMandible(b, sd), meshKey: side < 0 ? "ScarabMandible_L" : "ScarabMandible_R");
            }

            AddSplayedLegs(bp, ScarabHip, 0.2f, ScarabHipZ, ScarabSplay, new Vector3(0.24f, 0.14f, 0f), 0.2f, EnemySwatch.Black, EnemySwatch.IronDark, "ShadowScarab");
        }

        private static void DrawScarabBody(EnemyMeshBuilder b)
        {
            // The dome black as lacquer with gold seams, and on the brow the little sun it used to roll across the sand.
            b.Blob(new Vector3(0f, 0.08f, -0.08f), new Vector3(0.66f, 0.46f, 0.74f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.08f, -0.06f), new Vector3(0.5f, 0.14f, 0.62f), EnemySwatch.IronDark, coarse: true);
            b.Box(new Vector3(0f, 0.3f, -0.08f), new Vector3(0.025f, 0.02f, 0.62f), EnemySwatch.Gold);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.14f * s, 0.25f, -0.08f), new Vector3(0.02f, 0.02f, 0.5f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, -24f * s));
                b.Blob(new Vector3(0.1f * s, 0.06f, 0.4f), new Vector3(0.05f, 0.05f, 0.04f), EnemySwatch.MoonEye, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.04f, 0.28f), new Vector3(0.42f, 0.22f, 0.22f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, 0.02f, 0.4f), new Vector3(0.26f, 0.15f, 0.14f), EnemySwatch.Black, coarse: true);
            // The dim sun on the horns of the brow; it glows as if it might still be rolled somewhere.
            b.Segment(new Vector3(0f, 0.14f, 0.34f), new Vector3(0f, 0.3f, 0.36f), 0.024f, 0.014f, 4, EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, 0.34f, 0.36f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.StolenLight);
        }

        private static void DrawScarabMandible(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.07f * side, 0f, 0.1f), 0.032f, 0.022f, 4, EnemySwatch.IronDark);
            b.Segment(new Vector3(0.07f * side, 0f, 0.1f), new Vector3(0f, -0.01f, 0.2f), 0.022f, 0.005f, 4, EnemySwatch.Gold);
        }

        // ------------------------------------------------------------------
        // Junco-Laço
        // ------------------------------------------------------------------

        public static void BuildReedSnare(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawReedSnareBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.22f), DrawReedSnareHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.07f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.14f, 0.03f, 0.1f), EnemySwatch.Bark);
                b.Blob(new Vector3(0f, 0.005f, 0.05f), new Vector3(0.06f, 0.02f, 0.05f), EnemySwatch.MoonEye, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.1f, 0f), b => DrawReedNoose(b, -1f), Quaternion.Euler(0f, 0f, -14f), meshKey: "ReedSnareNoose_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.1f, 0f), b => DrawReedNoose(b, 1f), Quaternion.Euler(0f, 0f, 14f), meshKey: "ReedSnareNoose_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.2f, 0f), DrawReedSnareRoot);
        }

        private static void DrawReedSnareBody(EnemyMeshBuilder b)
        {
            // A sheaf tied at the waist, that the shadow taught to tie other things.
            b.Blob(Vector3.zero, new Vector3(0.52f, 0.46f, 0.5f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, -0.12f, 0f), new Vector3(0.44f, 0.22f, 0.42f), EnemySwatch.Bark, coarse: true);
            b.Cylinder(new Vector3(0f, -0.02f, 0f), 0.26f, 0.26f, 0.05f, 8, EnemySwatch.Leather);
            for (int i = 0; i < 9; i++)
            {
                float a = i * Mathf.PI * 2f / 9f;
                float r = 0.12f + (i % 3) * 0.05f;
                Vector3 from = new Vector3(Mathf.Cos(a) * r, 0.12f, Mathf.Sin(a) * r);
                Vector3 to = new Vector3(Mathf.Cos(a) * (r + 0.1f), 0.46f + (i % 3) * 0.1f, Mathf.Sin(a) * (r + 0.1f));
                b.Segment(from, to, 0.022f, 0.008f, 4, i % 2 == 0 ? EnemySwatch.CaveMoss : EnemySwatch.Lichen);
                b.Blob(to, new Vector3(0.05f, 0.09f, 0.05f), EnemySwatch.Bark, coarse: true);
            }
        }

        private static void DrawReedSnareHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.18f, 0.2f), EnemySwatch.Lichen);
            b.Blob(new Vector3(0f, 0.04f, -0.02f), new Vector3(0.18f, 0.08f, 0.16f), EnemySwatch.CaveMoss, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.05f, 0.07f, 0.035f, EnemySwatch.MoonEye);
        }

        private static void DrawReedNoose(EnemyMeshBuilder b, float side)
        {
            // An arm of plaited reed that ends in the loop, held open.
            b.Segment(Vector3.zero, new Vector3(0.24f * side, 0.06f, 0f), 0.035f, 0.02f, 4, EnemySwatch.CaveMoss);
            Vector3 c = new Vector3(0.36f * side, 0.08f, 0f);
            const int Sides = 7;
            for (int i = 0; i < Sides; i++)
            {
                float a0 = i * Mathf.PI * 2f / Sides;
                float a1 = (i + 1) * Mathf.PI * 2f / Sides;
                Vector3 p0 = c + new Vector3(Mathf.Cos(a0) * 0.11f * side, Mathf.Sin(a0) * 0.11f, 0f);
                Vector3 p1 = c + new Vector3(Mathf.Cos(a1) * 0.11f * side, Mathf.Sin(a1) * 0.11f, 0f);
                b.Segment(p0, p1, 0.014f, 0.014f, 3, i % 2 == 0 ? EnemySwatch.Lichen : EnemySwatch.Bark);
            }

            b.Blob(c, new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.MoonEye, coarse: true);
        }

        private static void DrawReedSnareRoot(EnemyMeshBuilder b)
        {
            // The root still dragging its mud, the lowest point as it drifts over the bank.
            b.Segment(Vector3.zero, new Vector3(0.03f, -0.22f, 0f), 0.09f, 0.04f, 5, EnemySwatch.Bark);
            b.Segment(new Vector3(0.03f, -0.22f, 0f), new Vector3(-0.02f, -0.42f, 0.02f), 0.04f, 0.012f, 4, EnemySwatch.BarkDark);
        }

        // ------------------------------------------------------------------
        // Gafanhoto de Sombra
        // ------------------------------------------------------------------

        public static void BuildShadowLocust(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawLocustBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.05f, 0.34f), DrawLocustHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.12f, 0.03f, 0.08f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, 0.01f, 0.05f), new Vector3(0.06f, 0.02f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.08f, 0.1f, 0.06f), b => DrawLocustWing(b, -1f), Quaternion.Euler(0f, 0f, -10f), meshKey: "ShadowLocustWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.08f, 0.1f, 0.06f), b => DrawLocustWing(b, 1f), Quaternion.Euler(0f, 0f, 10f), meshKey: "ShadowLocustWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.3f), DrawLocustTail);
        }

        private static void DrawLocustBody(EnemyMeshBuilder b)
        {
            // One grain of the cloud that eats the fields of light; each one is hungry for one ear of wheat, and there are too many.
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.64f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.05f, 0f), new Vector3(0.2f, 0.12f, 0.54f), EnemySwatch.IronDark, coarse: true);
            b.Box(new Vector3(0f, 0.12f, 0.05f), new Vector3(0.03f, 0.02f, 0.4f), EnemySwatch.StolenLight);
            for (int s = -1; s <= 1; s += 2)
            {
                // The long folded hind leg, up over the back and down to the ground, and the two short front pairs.
                Vector3 knee = new Vector3(0.24f * s, 0.2f, -0.2f);
                Vector3 foot = new Vector3(0.2f * s, -0.4f, -0.3f);
                b.Segment(new Vector3(0.1f * s, -0.02f, -0.1f), knee, 0.035f, 0.02f, 4, EnemySwatch.BarkDark);
                b.Segment(knee, foot, 0.02f, 0.008f, 4, EnemySwatch.Black);
                b.Segment(new Vector3(0.09f * s, -0.08f, 0.1f), new Vector3(0.2f * s, -0.4f, 0.12f), 0.018f, 0.008f, 4, EnemySwatch.Black);
                b.Segment(new Vector3(0.08f * s, -0.08f, 0.24f), new Vector3(0.16f * s, -0.4f, 0.3f), 0.018f, 0.008f, 4, EnemySwatch.Black);
            }
        }

        private static void DrawLocustHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.2f, 0.18f), EnemySwatch.Shadow);
            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.05f, 0.06f, EnemySwatch.MoonEye);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.03f * s, 0.07f, 0.06f), new Vector3(0.1f * s, 0.26f, 0.16f), 0.01f, 0.004f, 3, EnemySwatch.Black);
            }
        }

        private static void DrawLocustWing(EnemyMeshBuilder b, float side)
        {
            b.TwoSided(Vector3.zero, new Vector3(0.52f * side, 0.02f, -0.1f), new Vector3(0.04f * side, 0f, -0.34f), EnemySwatch.Shroud, EnemySwatch.Black);
            b.TwoSided(Vector3.zero, new Vector3(0.44f * side, 0.04f, 0.04f), new Vector3(0.52f * side, 0.02f, -0.1f), EnemySwatch.CaveSkin, EnemySwatch.Shroud);
            b.Segment(Vector3.zero, new Vector3(0.5f * side, 0.02f, -0.08f), 0.008f, 0.003f, 3, EnemySwatch.Black);
            b.Blob(new Vector3(0.44f * side, 0.02f, -0.08f), new Vector3(0.04f, 0.015f, 0.04f), EnemySwatch.StolenLight, coarse: true);
        }

        private static void DrawLocustTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.02f, -0.22f), 0.08f, 0.03f, 5, EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.02f, -0.23f), new Vector3(0.05f, 0.04f, 0.06f), EnemySwatch.IronDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Hipopótamo de Lodo
        // ------------------------------------------------------------------

        public static void BuildMudHippo(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawHippoBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.84f), DrawHippoHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.16f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.02f, 0.18f), new Vector3(0.46f, 0.1f, 0.36f), EnemySwatch.GolemStoneDark);
                b.Box(new Vector3(0f, 0.03f, 0.18f), new Vector3(0.36f, 0.02f, 0.3f), EnemySwatch.Pink);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.18f * s, 0.02f, 0.3f), 0.035f, 0.008f, 0.14f, 4, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.86f), DrawHippoTail, Quaternion.Euler(30f, 0f, 0f));
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.48f, 0.38f, 0.46f, -0.5f, 0.15f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, "MudHippo");
        }

        private static void DrawHippoBody(EnemyMeshBuilder b)
        {
            // A barrel of river mud with a hippo inside; the reeds grew on its back while it slept, and a thin crack of gold still shows.
            b.Blob(Vector3.zero, new Vector3(1.1f, 0.92f, 1.8f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, -0.14f, 0f), new Vector3(0.94f, 0.56f, 1.6f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(0f, 0.3f, -0.1f), new Vector3(0.7f, 0.3f, 1.1f), EnemySwatch.BarkDark, coarse: true);
            b.Box(new Vector3(0.4f, 0.1f, 0.1f), new Vector3(0.02f, 0.3f, 0.04f), EnemySwatch.StolenLight, Quaternion.Euler(12f, 0f, 0f));
            for (int i = 0; i < 7; i++)
            {
                float a = i * 2.4f;
                float x = Mathf.Cos(a) * 0.26f;
                float z = -0.1f + Mathf.Sin(a) * 0.5f;
                b.Segment(new Vector3(x, 0.42f, z), new Vector3(x * 1.2f, 0.76f + (i % 3) * 0.08f, z + 0.04f), 0.02f, 0.006f, 3, i % 2 == 0 ? EnemySwatch.CaveMoss : EnemySwatch.Lichen);
            }

            b.Segment(new Vector3(0f, 0.06f, 0.76f), new Vector3(0f, 0.08f, 0.84f), 0.36f, 0.3f, 7, EnemySwatch.GolemStoneDark);
        }

        private static void DrawHippoHead(EnemyMeshBuilder b)
        {
            // The great square muzzle, little ears high up, and the eyes that are almost all lid.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.46f, 0.56f), EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0f, -0.04f, 0.3f), new Vector3(0.52f, 0.24f, 0.4f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.04f, 0.42f), new Vector3(0.34f, 0.12f, 0.16f), EnemySwatch.Bark, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.2f, 0.22f, 0.08f, 0.07f, EnemySwatch.MoonEye);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.22f * s, 0.3f, -0.12f), new Vector3(0.1f, 0.12f, 0.06f), EnemySwatch.GolemStoneDark, coarse: true);
                b.Blob(new Vector3(0.1f * s, 0.08f, 0.5f), new Vector3(0.06f, 0.05f, 0.03f), EnemySwatch.Black, coarse: true);
                b.Cylinder(new Vector3(0.18f * s, -0.14f, 0.32f), 0.035f, 0.008f, 0.12f, 4, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            }
        }

        private static void DrawHippoTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.18f, -0.04f), 0.08f, 0.03f, 5, EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, -0.2f, -0.04f), new Vector3(0.09f, 0.1f, 0.03f), EnemySwatch.Black, coarse: true);
        }

        // ------------------------------------------------------------------
        // Cobra do Junco
        // ------------------------------------------------------------------

        private static readonly Vector3[] ReedSnakeSteps =
        {
            new Vector3(0.3f, 0f, -0.16f), new Vector3(0.06f, 0f, -0.26f), new Vector3(-0.4f, 0f, -0.14f), new Vector3(-0.14f, 0f, -0.22f),
        };

        private static readonly float[] ReedSnakeRadii = { 0.085f, 0.075f, 0.062f, 0.048f, 0.012f };

        public static void BuildReedSnake(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.09f, 0.2f), DrawReedSnakeNeck);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.56f, 0.12f), DrawReedSnakeHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.02f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.09f, 0.018f, 0.12f), EnemySwatch.Lichen);
                b.Cylinder(new Vector3(0f, 0.005f, 0.1f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Fang);
            });
            AddSnakeTail(bp, body, new Vector3(0f, 0f, -0.06f), ReedSnakeSteps, ReedSnakeRadii, EnemySwatch.CaveMoss, EnemySwatch.BarkDark, EnemySwatch.MoonEye);
        }

        private static void DrawReedSnakeNeck(EnemyMeshBuilder b)
        {
            // Green as the stalks it hides among; the dark diamonds are the shadows between two reeds.
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.22f, 0.18f, 0.24f), EnemySwatch.CaveMoss);
            b.Segment(new Vector3(0f, 0.02f, 0f), new Vector3(0f, 0.3f, 0.06f), 0.085f, 0.07f, 6, EnemySwatch.CaveMoss);
            b.Segment(new Vector3(0f, 0.3f, 0.06f), new Vector3(0f, 0.56f, 0.12f), 0.07f, 0.05f, 6, EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, 0.24f, 0.11f), new Vector3(0.1f, 0.26f, 0.05f), EnemySwatch.Lichen, coarse: true);
            for (int k = 0; k < 3; k++)
            {
                b.Box(new Vector3(0f, 0.12f + k * 0.14f, k * 0.022f - 0.055f), new Vector3(0.05f, 0.05f, 0.03f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, 45f));
            }
        }

        private static void DrawReedSnakeHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.06f), new Vector3(0.15f, 0.08f, 0.2f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.11f, 0.04f, 0.14f), EnemySwatch.Lichen, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.05f, 0.1f, 0.03f, EnemySwatch.MoonEye);
            b.Box(new Vector3(0f, -0.01f, 0.2f), new Vector3(0.01f, 0.005f, 0.06f), EnemySwatch.Pink);
        }

        // ------------------------------------------------------------------
        // Serpente de Três Cabeças
        // ------------------------------------------------------------------

        private static readonly Vector3[] SerpentSteps =
        {
            new Vector3(0.36f, 0f, -0.2f), new Vector3(0.08f, 0f, -0.32f), new Vector3(-0.5f, 0f, -0.16f), new Vector3(-0.18f, 0f, -0.28f),
        };

        private static readonly float[] SerpentRadii = { 0.135f, 0.115f, 0.095f, 0.07f, 0.016f };

        public static void BuildThreeHeadedSerpent(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.14f, 0.2f), DrawSerpentBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.68f, 0.14f), DrawSerpentHead, meshKey: "SerpentHead");
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.14f, 0.025f, 0.16f), EnemySwatch.SandstoneLight);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.05f * s, 0f, 0.13f), 0.012f, 0f, 0.05f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(SerpentHeadLeft, body, new Vector3(-0.28f, 0.56f, 0.06f), DrawSerpentHead, Quaternion.Euler(0f, -14f, 0f), meshKey: "SerpentHead");
            bp.Mesh(SerpentHeadRight, body, new Vector3(0.28f, 0.56f, 0.06f), DrawSerpentHead, Quaternion.Euler(0f, 14f, 0f), meshKey: "SerpentHead");
            AddSnakeTail(bp, body, new Vector3(0f, 0f, -0.08f), SerpentSteps, SerpentRadii, EnemySwatch.SandstoneDark, EnemySwatch.Gold, EnemySwatch.Ember);
        }

        private static void DrawSerpentBody(EnemyMeshBuilder b)
        {
            // One coil on the sand and three necks up out of it, each sure it is the one that pulls the barque.
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.42f, 0.28f, 0.42f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.36f, 0.14f, 0.36f), EnemySwatch.Sandstone, coarse: true);
            for (int s = -1; s <= 1; s++)
            {
                float x = s * 0.28f;
                float top = s == 0 ? 0.68f : 0.56f;
                float z = s == 0 ? 0.14f : 0.06f;
                b.Segment(new Vector3(s * 0.08f, 0.02f, 0f), new Vector3(x * 0.6f, top * 0.55f, z * 0.5f), 0.12f, 0.09f, 6, EnemySwatch.SandstoneDark);
                b.Segment(new Vector3(x * 0.6f, top * 0.55f, z * 0.5f), new Vector3(x, top, z), 0.09f, 0.065f, 6, EnemySwatch.SandstoneDark);
                b.Blob(new Vector3(x * 0.7f, top * 0.65f, z * 0.6f + 0.06f), new Vector3(0.1f, 0.2f, 0.05f), EnemySwatch.SandstoneLight, coarse: true);
                b.Box(new Vector3(x * 0.8f, top * 0.8f, z * 0.8f - 0.06f), new Vector3(0.06f, 0.06f, 0.03f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, 45f));
            }
        }

        private static void DrawSerpentHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.02f, 0.06f), new Vector3(0.2f, 0.11f, 0.24f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.01f, 0.12f), new Vector3(0.15f, 0.06f, 0.16f), EnemySwatch.SandstoneLight, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.06f, 0.12f, 0.04f, EnemySwatch.Ember);
            b.Box(new Vector3(0f, 0.08f, 0f), new Vector3(0.07f, 0.03f, 0.1f), EnemySwatch.Gold);
        }

        // ------------------------------------------------------------------
        // Escorpião Negro
        // ------------------------------------------------------------------

        private const float ScorpionHip = 0.28f;
        private static readonly float[] ScorpionHipZ = { 0.18f, 0.06f, -0.08f, -0.2f };
        private static readonly float[] ScorpionSplay = { 36f, 12f, -14f, -38f };

        public static void BuildBlackScorpion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, ScorpionHip, 0f), DrawScorpionBody);
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body, new Vector3(0.18f * side, 0.02f, 0.3f),
                    b => DrawScorpionClaw(b, sd), meshKey: side < 0 ? "ScorpionClaw_L" : "ScorpionClaw_R");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.34f), DrawScorpionTail);
            AddSplayedLegs(bp, ScorpionHip, 0.16f, ScorpionHipZ, ScorpionSplay, new Vector3(0.22f, 0.13f, 0f), 0.18f, EnemySwatch.Black, EnemySwatch.IronDark, "BlackScorpion");
        }

        private static void DrawScorpionBody(EnemyMeshBuilder b)
        {
            // A black plate of armour on eight legs; the shadow did not change what it does, only what it wants.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.26f, 0.78f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.42f, 0.14f, 0.68f), EnemySwatch.IronDark, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.12f, -0.24f + i * 0.16f), new Vector3(0.36f - i * 0.03f, 0.02f, 0.04f), EnemySwatch.Gold);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.1f, 0.34f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            }
        }

        private static void DrawScorpionClaw(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.12f * side, 0.03f, 0.14f), 0.05f, 0.042f, 5, EnemySwatch.Black);
            b.Blob(new Vector3(0.14f * side, 0.04f, 0.27f), new Vector3(0.18f, 0.12f, 0.22f), EnemySwatch.IronDark);
            b.Cylinder(new Vector3(0.1f * side, 0.06f, 0.36f), 0.03f, 0.006f, 0.13f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(80f, 0f, 0f));
            b.Cylinder(new Vector3(0.17f * side, 0.02f, 0.36f), 0.026f, 0.005f, 0.11f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(95f, 0f, 0f));
        }

        private static void DrawScorpionTail(EnemyMeshBuilder b)
        {
            // Five knuckles arching forward with a band of gold on each, and the ember in the sting.
            Vector3[] points =
            {
                Vector3.zero, new Vector3(0f, 0.14f, -0.16f), new Vector3(0f, 0.36f, -0.24f), new Vector3(0f, 0.56f, -0.16f), new Vector3(0f, 0.66f, 0.02f),
            };
            for (int i = 0; i < points.Length - 1; i++)
            {
                float r0 = 0.075f - i * 0.012f;
                float r1 = 0.075f - (i + 1) * 0.012f;
                b.Segment(points[i], points[i + 1], r0, r1, 5, EnemySwatch.Black);
                b.Blob(points[i + 1], Vector3.one * (r1 * 2.2f), EnemySwatch.Gold, coarse: true);
            }

            Vector3 end = points[points.Length - 1];
            b.Blob(end + new Vector3(0f, -0.02f, 0.05f), new Vector3(0.1f, 0.1f, 0.12f), EnemySwatch.Ember);
            b.Cylinder(end + new Vector3(0f, -0.02f, 0.1f), 0.03f, 0f, 0.12f, 4, EnemySwatch.Fang, rotation: Quaternion.Euler(115f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Ushabti de Areia
        // ------------------------------------------------------------------

        public static void BuildSandUshabti(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.84f, 0f), DrawSandUshabtiBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.4f, 0.01f), DrawSandUshabtiHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.27f * side, 0.28f, 0.02f),
                    b => DrawSandUshabtiArm(b, side, right), Quaternion.Euler(-10f, 0f, 0f));
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.54f, 0.11f, 0.08f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, EnemySwatch.SandstoneDark, "SandUshabti");
        }

        private static void DrawSandUshabtiBody(EnemyMeshBuilder b)
        {
            // Packed sand with the line of signs still on its chest; it sheds a little sand with every step, and the signs shed nothing.
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.48f, 0.68f, 0.34f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, -0.14f, 0f), new Vector3(0.42f, 0.28f, 0.3f), EnemySwatch.SandstoneDark, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                b.Box(new Vector3(0f, 0.28f - i * 0.09f, 0.175f), new Vector3(0.06f, 0.05f, 0.02f), EnemySwatch.Rune);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.16f * s, -0.34f, 0.1f), new Vector3(0.08f, 0.1f, 0.08f), EnemySwatch.SandstoneLight, coarse: true);
            }
        }

        private static void DrawSandUshabtiHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.26f, 0.28f, 0.26f), EnemySwatch.SandstoneLight);
            // The striped cloth of the old figures, faded.
            b.Box(new Vector3(0f, 0.08f, -0.08f), new Vector3(0.3f, 0.26f, 0.14f), EnemySwatch.Copper);
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0f, -0.02f + i * 0.09f, -0.155f), new Vector3(0.28f, 0.02f, 0.01f), EnemySwatch.CopperDark);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.055f * s, 0.08f, 0.13f), new Vector3(0.05f, 0.02f, 0.01f), EnemySwatch.Rune);
            }
        }

        private static void DrawSandUshabtiArm(EnemyMeshBuilder b, float side, bool right)
        {
            b.Blob(Vector3.zero, new Vector3(0.14f, 0.13f, 0.12f), EnemySwatch.SandstoneDark, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.02f * side, -0.4f, 0.03f), 0.06f, 0.05f, 6, EnemySwatch.Sandstone);
            b.Blob(new Vector3(0.02f * side, -0.44f, 0.04f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.SandstoneLight, coarse: true);
            if (right)
            {
                // The hoe it was buried with: it is still waiting to be told which field.
                b.Cylinder(new Vector3(0.02f * side, -0.62f, 0.04f), 0.018f, 0.018f, 0.58f, 5, EnemySwatch.Bark);
                b.Box(new Vector3(0.02f * side, -0.64f, 0.1f), new Vector3(0.1f, 0.05f, 0.14f), EnemySwatch.CopperDark);
            }
        }

        // ------------------------------------------------------------------
        // Mordedor de Sombra
        // ------------------------------------------------------------------

        public static void BuildShadowBiter(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawBiterBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.42f), DrawBiterHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.1f), new Vector3(0.14f, 0.03f, 0.2f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, 0.005f, 0.09f), new Vector3(0.1f, 0.012f, 0.16f), EnemySwatch.Sorcery);
                for (int i = 0; i < 3; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.055f * s, 0f, 0.05f + i * 0.06f), 0.012f, 0f, 0.05f, 3, EnemySwatch.Fang);
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.05f, -0.38f), DrawBiterTail, Quaternion.Euler(-18f, 0f, 0f));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.46f, 0.14f, 0.24f, -0.26f, 0.045f, EnemySwatch.Shadow, EnemySwatch.Black, "ShadowBiter");
        }

        private static void DrawBiterBody(EnemyMeshBuilder b)
        {
            // A piece of the dark bit off and left to run; the seam of light down its back is the place it was torn from.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.34f, 0.84f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.05f, 0f), new Vector3(0.28f, 0.2f, 0.72f), EnemySwatch.Black, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                b.Box(new Vector3(0f, 0.17f - Mathf.Abs(i - 2.5f) * 0.01f, -0.3f + i * 0.12f), new Vector3(0.04f, 0.02f, 0.08f), EnemySwatch.Sorcery);
            }

            b.Segment(new Vector3(0f, 0.06f, 0.32f), new Vector3(0f, 0.12f, 0.42f), 0.11f, 0.09f, 5, EnemySwatch.Shadow);
        }

        private static void DrawBiterHead(EnemyMeshBuilder b)
        {
            // Almost nothing but mouth: a blunt wedge, two torn ears and teeth too white for the rest of it.
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.18f, 0.24f), EnemySwatch.Shadow);
            b.Box(new Vector3(0f, -0.01f, 0.14f), new Vector3(0.14f, 0.07f, 0.2f), EnemySwatch.Black);
            HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.05f, 0.08f, 0.04f, EnemySwatch.Sorcery);
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.04f * s, 0.07f, -0.02f), new Vector3(0.12f * s, 0.08f, -0.03f), new Vector3(0.1f * s, 0.2f, -0.08f), EnemySwatch.Shadow, EnemySwatch.Black);
                b.Cylinder(new Vector3(0.05f * s, -0.04f, 0.2f), 0.012f, 0f, 0.05f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            }
        }

        private static void DrawBiterTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.36f), 0.05f, 0.01f, 4, EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.04f, -0.36f), new Vector3(0.035f, 0.035f, 0.06f), EnemySwatch.Sorcery, coarse: true);
        }

        // ------------------------------------------------------------------
        // Treva Rastejante
        // ------------------------------------------------------------------

        public static void BuildCreepingDark(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawCreepingDark);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.1f, 0.5f, 0.02f), DrawCreepingLight);
        }

        private static void DrawCreepingDark(EnemyMeshBuilder b)
        {
            // Low and spreading, darker than the night it crawls in, with arms feeling ahead along the coils.
            b.Blob(new Vector3(0f, 0.32f, 0f), new Vector3(1f, 0.64f, 0.9f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0.04f, 0.5f, -0.04f), new Vector3(0.62f, 0.3f, 0.54f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(1.1f, 0.18f, 1f), EnemySwatch.Black, coarse: true);
            for (int i = 0; i < 7; i++)
            {
                float a = i * Mathf.PI * 2f / 7f + 0.3f;
                Vector3 root = new Vector3(Mathf.Sin(a) * 0.4f, 0.12f, Mathf.Cos(a) * 0.36f);
                Vector3 elbow = new Vector3(Mathf.Sin(a) * 0.62f, 0.2f, Mathf.Cos(a) * 0.56f);
                Vector3 tip = new Vector3(Mathf.Sin(a) * 0.78f, 0.04f, Mathf.Cos(a) * 0.72f);
                b.Segment(root, elbow, 0.08f, 0.05f, 5, EnemySwatch.Shadow);
                b.Segment(elbow, tip, 0.05f, 0.012f, 4, EnemySwatch.Black);
            }

            // Spines of dark standing up where it has fed, reaching over the top.
            for (int i = 0; i < 5; i++)
            {
                float a = i * 2.2f;
                b.Crystal(new Vector3(Mathf.Sin(a) * 0.16f, 0.58f, Mathf.Cos(a) * 0.14f), 0.05f, 0.18f + (i % 2) * 0.08f, EnemySwatch.Shroud, sides: 4);
            }

            b.Blob(new Vector3(-0.14f, 0.42f, 0.42f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.Sorcery, coarse: true);
            b.Blob(new Vector3(0.14f, 0.42f, 0.42f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.Sorcery, coarse: true);
        }

        private static void DrawCreepingLight(EnemyMeshBuilder b)
        {
            // The small gold scales it has bitten off Mehen, still lit, sinking slowly inside.
            b.Blob(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.StolenLight, coarse: true);
            b.Crystal(new Vector3(-0.18f, -0.06f, 0.08f), 0.05f, 0.1f, EnemySwatch.Gold, Quaternion.Euler(10f, 0f, 24f), 4);
            b.Blob(new Vector3(-0.2f, 0.05f, -0.1f), new Vector3(0.07f, 0.07f, 0.07f), EnemySwatch.StolenLight, coarse: true);
        }
    }
}
