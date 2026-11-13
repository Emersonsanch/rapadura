using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The nineteenth pass of beasts (bible §3.2.1): roles of Região 5, the Costa das Monções (Docs/Mapas100.md 27–30), and Região 6, the Delta de
    /// Jhilpur (32–35), that the continuous world still played with another region's beast, low-poly from the shared palette like
    /// <see cref="LentRoleBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Ushabti de Bronze Kushita</b> (27, ~1.5 m): a bronze tomb figure of Meroë, a ram's-head amulet on the chest and a hoe. Biped with arms.</item>
    /// <item><b>Escorpião-Rei Jovem</b> (27, ~1 m): a sand-coloured young scorpion with a little gold crown on its carapace. Claws, tail, eight legs.</item>
    /// <item><b>Chacal de Areia Negra</b> (27, ~0.9 m): a lean black jackal of the pyramid fields with a beaded collar. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Água-Viva de Lodo</b> (28, ~1.2 m): a jellyfish of mud on its tentacles. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Peixe-Leão Apagado</b> (28, ~0.9 m): a dimmed lionfish with a mane of venom spines. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, fins, <c>Tail</c>.</item>
    /// <item><b>Kongamato</b> (30, ~1.1 m): a leathery-winged flyer of the mangrove channels with a toothed beak. Bird skeleton.</item>
    /// <item><b>Polvo Ladrão</b> (29, ~1.1 m): an octopus of Kilwa's drowned streets clutching what it stole. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Raiz-Laço</b> (32, ~1 m): an aerial root of the banyans knotted into a noose, walking on its rootlets. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Garça-Fantasma</b> (33, ~1.5 m): a pale heron gone half shadow, smoke off its back. Bird skeleton.</item>
    /// <item><b>Libélula de Fumaça</b> (34, ~0.8 m): a big dragonfly of smoke. <c>Body</c>, two wings, the long abdomen as <c>Stinger</c>.</item>
    /// <item><b>Bolha de Fumaça</b> (35, ~1.2 m): a bubble of the Lodo's smoke on a stem of smoke. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Filho do Lodo</b> (35, ~0.9 m): a little swimmer of the Lodo, a lump of mud with stubby arms. <c>Body</c> → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class CoastOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Ushabti de Bronze Kushita
        // ------------------------------------------------------------------

        public static void BuildKushiteUshabti(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.84f, 0f), DrawKushiteUshabtiBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.01f), DrawKushiteUshabtiHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.28f * side, 0.28f, 0.02f),
                    b => DrawKushiteUshabtiArm(b, side, right), Quaternion.Euler(-10f, 0f, 0f));
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.54f, 0.12f, 0.085f, EnemySwatch.Copper, EnemySwatch.CopperDark, EnemySwatch.Verdigris, "KushiteUshabti");
        }

        private static void DrawKushiteUshabtiBody(EnemyMeshBuilder b)
        {
            // The kings of Meroë were buried under their steep pyramids with hundreds of bronze figures to work the fields of the other world for them. The shadow came into the chapels and woke them without telling them which field, so they work on whoever passes.
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.5f, 0.7f, 0.36f), EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.14f, 0f), new Vector3(0.44f, 0.28f, 0.32f), EnemySwatch.CopperDark, coarse: true);
            // The green of old bronze in the folds and the ram's-head amulet of the avenues on the chest, its eyes still lit.
            b.Blob(new Vector3(0.12f, -0.02f, 0.12f), new Vector3(0.16f, 0.22f, 0.1f), EnemySwatch.Verdigris, coarse: true);
            b.Blob(new Vector3(0f, 0.26f, 0.17f), new Vector3(0.14f, 0.12f, 0.06f), EnemySwatch.Gold);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Horn(new Vector3(0.05f * s, 0.29f, 0.19f), 0.025f, 0.08f, 3, 70f, EnemySwatch.Gold, Quaternion.Euler(0f, 0f, -60f * s));
                b.Blob(new Vector3(0.025f * s, 0.27f, 0.2f), new Vector3(0.02f, 0.02f, 0.02f), EnemySwatch.Ember, coarse: true);
            }

            // The column of signs down the front, the spell that tells it to answer when called.
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.1f - i * 0.09f, 0.185f), new Vector3(0.05f, 0.05f, 0.02f), EnemySwatch.Rune);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.17f * s, -0.34f, 0.1f), new Vector3(0.08f, 0.1f, 0.08f), EnemySwatch.Verdigris, coarse: true);
            }
        }

        private static void DrawKushiteUshabtiHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.26f, 0.28f, 0.26f), EnemySwatch.Copper);
            // The cap crown of the Kushite kings with the two uraei in front, gone green, and the long eyes of light.
            b.Blob(new Vector3(0f, 0.15f, -0.02f), new Vector3(0.28f, 0.16f, 0.28f), EnemySwatch.CopperDark, coarse: true);
            b.Box(new Vector3(0f, 0.08f, 0.12f), new Vector3(0.26f, 0.03f, 0.03f), EnemySwatch.Gold);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.04f * s, 0.09f, 0.13f), 0.018f, 0.008f, 0.08f, 4, EnemySwatch.Verdigris);
                b.Box(new Vector3(0.055f * s, 0.02f, 0.13f), new Vector3(0.06f, 0.02f, 0.01f), EnemySwatch.Rune);
            }

            b.Box(new Vector3(0f, -0.06f, 0.13f), new Vector3(0.04f, 0.08f, 0.03f), EnemySwatch.CopperDark);
        }

        private static void DrawKushiteUshabtiArm(EnemyMeshBuilder b, float side, bool right)
        {
            b.Blob(Vector3.zero, new Vector3(0.15f, 0.14f, 0.13f), EnemySwatch.CopperDark, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.02f * side, -0.4f, 0.04f), 0.065f, 0.05f, 6, EnemySwatch.Copper);
            b.Blob(new Vector3(0.02f * side, -0.44f, 0.05f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.Verdigris, coarse: true);
            if (right)
            {
                // The bronze hoe of the fields of the other world.
                b.Cylinder(new Vector3(0.02f * side, -0.62f, 0.05f), 0.018f, 0.018f, 0.58f, 5, EnemySwatch.Bark);
                b.Box(new Vector3(0.02f * side, -0.64f, 0.11f), new Vector3(0.11f, 0.05f, 0.15f), EnemySwatch.Gold);
            }
            else
            {
                // The basket for the seed on its shoulder strap.
                b.Cylinder(new Vector3(0.03f * side, -0.6f, 0.02f), 0.07f, 0.09f, 0.12f, 6, EnemySwatch.Bark, EnemySwatch.BarkDark);
            }
        }

        // ------------------------------------------------------------------
        // Escorpião-Rei Jovem
        // ------------------------------------------------------------------

        private const float ScorpionHip = 0.28f;
        private static readonly float[] ScorpionHipZ = { 0.18f, 0.06f, -0.08f, -0.2f };
        private static readonly float[] ScorpionSplay = { 36f, 12f, -14f, -38f };

        public static void BuildYoungScorpionKing(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, ScorpionHip, 0f), DrawScorpionKingBody);
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body, new Vector3(0.18f * side, 0.02f, 0.3f),
                    b => DrawScorpionKingClaw(b, sd), meshKey: side < 0 ? "ScorpionKingClaw_L" : "ScorpionKingClaw_R");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.34f), DrawScorpionKingTail);
            AddSplayedLegs(bp, ScorpionHip, 0.16f, ScorpionHipZ, ScorpionSplay, new Vector3(0.22f, 0.13f, 0f), 0.18f, EnemySwatch.SandstoneDark,
                EnemySwatch.CopperDark, "YoungScorpionKing");
        }

        private static void DrawScorpionKingBody(EnemyMeshBuilder b)
        {
            // In the stories of the pyramid fields the first king of the river had a scorpion for a name, and the young scorpions of Meroë still go about as if they had inherited it. Since the shadow they act on it: they rule the sand between the chapels and chase out anyone who did not bow.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.26f, 0.78f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.42f, 0.14f, 0.68f), EnemySwatch.Sandstone, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.12f, -0.24f + i * 0.16f), new Vector3(0.36f - i * 0.03f, 0.02f, 0.04f), EnemySwatch.CopperDark);
            }

            // The little crown it wears on the carapace, a ring of gold with five points, and its ember eyes.
            b.Cylinder(new Vector3(0f, 0.12f, 0.16f), 0.09f, 0.09f, 0.05f, 8, EnemySwatch.Gold);
            for (int i = 0; i < 5; i++)
            {
                float a = i * Mathf.PI * 2f / 5f;
                b.Cylinder(new Vector3(Mathf.Cos(a) * 0.08f, 0.17f, 0.16f + Mathf.Sin(a) * 0.08f), 0.02f, 0f, 0.07f, 3, EnemySwatch.Gold);
            }

            b.Blob(new Vector3(0f, 0.18f, 0.16f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.1f, 0.34f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            }
        }

        private static void DrawScorpionKingClaw(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.12f * side, 0.03f, 0.14f), 0.05f, 0.042f, 5, EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0.14f * side, 0.04f, 0.27f), new Vector3(0.18f, 0.12f, 0.22f), EnemySwatch.Sandstone);
            b.Cylinder(new Vector3(0.1f * side, 0.06f, 0.36f), 0.03f, 0.006f, 0.13f, 4, EnemySwatch.CopperDark, rotation: Quaternion.Euler(80f, 0f, 0f));
            b.Cylinder(new Vector3(0.17f * side, 0.02f, 0.36f), 0.026f, 0.005f, 0.11f, 4, EnemySwatch.CopperDark, rotation: Quaternion.Euler(95f, 0f, 0f));
        }

        private static void DrawScorpionKingTail(EnemyMeshBuilder b)
        {
            // Five knuckles arching forward, red-gold bands like a royal staff, and the sting lit.
            Vector3[] points =
            {
                Vector3.zero, new Vector3(0f, 0.14f, -0.16f), new Vector3(0f, 0.36f, -0.24f), new Vector3(0f, 0.56f, -0.16f), new Vector3(0f, 0.66f, 0.02f),
            };
            for (int i = 0; i < points.Length - 1; i++)
            {
                float r0 = 0.075f - i * 0.012f;
                float r1 = 0.075f - (i + 1) * 0.012f;
                b.Segment(points[i], points[i + 1], r0, r1, 5, EnemySwatch.SandstoneDark);
                b.Blob(points[i + 1], Vector3.one * (r1 * 2.2f), i % 2 == 0 ? EnemySwatch.Gold : EnemySwatch.WarCape, coarse: true);
            }

            Vector3 end = points[points.Length - 1];
            b.Blob(end + new Vector3(0f, -0.02f, 0.05f), new Vector3(0.1f, 0.1f, 0.12f), EnemySwatch.Ember);
            b.Cylinder(end + new Vector3(0f, -0.02f, 0.1f), 0.03f, 0f, 0.12f, 4, EnemySwatch.Fang, rotation: Quaternion.Euler(115f, 0f, 0f));
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

        // ------------------------------------------------------------------
        // Chacal de Areia Negra
        // ------------------------------------------------------------------

        public static void BuildBlackSandJackal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawBlackSandJackalBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.44f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.26f), EnemySwatch.Black);
                // The long thin muzzle and the tall ears of the jackal on the chapel doors.
                b.Blob(new Vector3(0f, -0.03f, 0.16f), new Vector3(0.11f, 0.1f, 0.2f), EnemySwatch.Shadow, coarse: true);
                b.Blob(new Vector3(0f, -0.02f, 0.26f), new Vector3(0.05f, 0.04f, 0.04f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.07f * s, 0.08f, -0.03f), 0.05f, 0.01f, 0.22f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(-8f, 0f, -16f * s));
                    b.Cylinder(new Vector3(0.075f * s, 0.1f, -0.01f), 0.025f, 0.005f, 0.16f, 3, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(-8f, 0f, -16f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.04f, 0.1f, 0.045f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.08f, 0.025f, 0.16f), EnemySwatch.Shadow);
                b.Cylinder(new Vector3(0.025f, 0.005f, 0.13f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.025f, 0.005f, 0.13f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.42f), b =>
            {
                // The long brush hanging low, black to the tip.
                b.Segment(Vector3.zero, new Vector3(0f, -0.16f, -0.16f), 0.05f, 0.04f, 5, EnemySwatch.Black);
                b.Segment(new Vector3(0f, -0.16f, -0.16f), new Vector3(0f, -0.26f, -0.2f), 0.04f, 0.015f, 4, EnemySwatch.Shadow);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.42f, 0.15f, 0.28f, -0.28f, 0.05f, EnemySwatch.Black, EnemySwatch.Shadow, "BlackSandJackal");
        }

        private static void DrawBlackSandJackalBody(EnemyMeshBuilder b)
        {
            // The black jackals of Meroë were the ones that showed the dead the way between the pyramids, so nobody got lost in the sand. Since the shadow they still lead, in packs, but the way they show goes nowhere and they bite whoever follows.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.38f, 0.84f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.3f, 0.2f, 0.6f), EnemySwatch.Shadow, coarse: true);
            b.Segment(new Vector3(0f, 0.08f, 0.3f), new Vector3(0f, 0.2f, 0.42f), 0.1f, 0.08f, 6, EnemySwatch.Black);
            // The beaded collar of the chapel jackals, faience and gold, and the black sand that sticks to its flanks.
            b.Blob(new Vector3(0f, 0.1f, 0.32f), new Vector3(0.62f, 0.1f, 0.24f), EnemySwatch.Gold, coarse: true);
            b.Blob(new Vector3(0f, 0.08f, 0.34f), new Vector3(0.56f, 0.08f, 0.22f), EnemySwatch.Verdigris, coarse: true);
            b.Blob(new Vector3(0f, 0.04f, 0.45f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Rune, coarse: true);
            b.Blob(new Vector3(0.12f, -0.02f, -0.18f), new Vector3(0.14f, 0.12f, 0.2f), EnemySwatch.IronDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Água-Viva de Lodo
        // ------------------------------------------------------------------

        public static void BuildMudJellyfish(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawMudJellyfish);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.92f, 0f), b =>
            {
                // The light of the reef it swallowed, glowing through the mud of the bell.
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.2f, 0.3f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0.06f, -0.04f, 0.04f), new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawMudJellyfish(EnemyMeshBuilder b)
        {
            // The jellyfish of Kirta's reef are clear as the water and carry the coral's light on their backs from one head to another. The Lodo got into them and they went brown and heavy; they sink on the reef now and sting with the mud.
            b.Blob(new Vector3(0f, 0.96f, 0f), new Vector3(0.9f, 0.5f, 0.9f), EnemySwatch.CaveSkin);
            b.Blob(new Vector3(0f, 1.06f, -0.04f), new Vector3(0.62f, 0.3f, 0.62f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0f, 0.76f, 0f), new Vector3(0.84f, 0.12f, 0.84f), EnemySwatch.CaveSkinDark);
            HinokuraBeastRecipes.AddEyes(b, 0.14f, 0.86f, 0.36f, 0.08f, EnemySwatch.SoftEye);
            // The frilled arms in the middle and the long tentacles it walks on, dragging along the reef: the lowest points of the model.
            for (int i = 0; i < 4; i++)
            {
                float a = i * Mathf.PI * 0.5f + 0.4f;
                b.Segment(new Vector3(Mathf.Sin(a) * 0.08f, 0.74f, Mathf.Cos(a) * 0.08f), new Vector3(Mathf.Sin(a) * 0.14f, 0.36f, Mathf.Cos(a) * 0.14f), 0.07f, 0.04f, 5, EnemySwatch.CaveBelly);
            }

            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f;
                Vector3 root = new Vector3(Mathf.Sin(a) * 0.38f, 0.76f, Mathf.Cos(a) * 0.38f);
                Vector3 knee = new Vector3(Mathf.Sin(a + 0.15f) * 0.46f, 0.4f, Mathf.Cos(a + 0.15f) * 0.46f);
                Vector3 tip = new Vector3(Mathf.Sin(a + 0.3f) * 0.52f, 0.03f, Mathf.Cos(a + 0.3f) * 0.52f);
                b.Segment(root, knee, 0.035f, 0.025f, 4, i % 2 == 0 ? EnemySwatch.CaveSkinDark : EnemySwatch.BarkDark);
                b.Segment(knee, tip, 0.025f, 0.01f, 4, EnemySwatch.CaveSkinDark);
                if (i % 2 == 0)
                {
                    b.Blob(knee, new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Crystal, coarse: true);
                }
            }
        }

        // ------------------------------------------------------------------
        // Peixe-Leão Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedLionfish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawDimmedLionfishBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.34f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.3f, 0.26f), EnemySwatch.WarCape);
                b.Blob(new Vector3(0f, -0.04f, 0.08f), new Vector3(0.18f, 0.16f, 0.16f), EnemySwatch.Ivory, coarse: true);
                // The feathery tassels over the eyes.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.07f * s, 0.12f, 0.04f), 0.02f, 0.004f, 0.12f, 3, EnemySwatch.IvoryDark, rotation: Quaternion.Euler(-20f, 0f, -25f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.05f, 0.07f, 0.06f, EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.12f, 0.025f, 0.12f), EnemySwatch.Ivory);
                b.Cylinder(new Vector3(0f, 0.005f, 0.11f), 0.012f, 0f, 0.03f, 3, EnemySwatch.Fang);
            });
            // The great fan fins like a lion's mane, striped and edged with spines.
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.14f, -0.02f, 0.18f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.WarCape, EnemySwatch.IvoryDark, EnemySwatch.Shadow, 0.5f), Quaternion.Euler(0f, 0f, -24f), meshKey: "DimmedLionfishFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.14f, -0.02f, 0.18f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.WarCape, EnemySwatch.IvoryDark, EnemySwatch.Shadow, 0.5f), Quaternion.Euler(0f, 0f, 24f), meshKey: "DimmedLionfishFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.36f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.14f), 0.07f, 0.03f, 5, EnemySwatch.WarCape);
                b.Box(new Vector3(0f, 0f, -0.22f), new Vector3(0.03f, 0.28f, 0.14f), EnemySwatch.IvoryDark);
                b.Box(new Vector3(0f, 0f, -0.2f), new Vector3(0.035f, 0.04f, 0.1f), EnemySwatch.Shadow);
            });
        }

        private static void DrawDimmedLionfishBody(EnemyMeshBuilder b)
        {
            // The lionfish of the atoll were the coral's guards, red and white so that everyone could see them from far off and keep away. The shadow greyed the white of their stripes; they come looking now, and throw the light gathered at the tips of their spines.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.46f, 0.74f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.26f, 0.22f, 0.56f), EnemySwatch.Ivory, coarse: true);
            // The stripes, gone grey where the shadow is.
            for (int i = 0; i < 5; i++)
            {
                b.Blob(new Vector3(0f, 0.02f, 0.24f - i * 0.12f), new Vector3(0.37f, 0.4f - i * 0.03f, 0.04f), i % 2 == 0 ? EnemySwatch.Ivory : EnemySwatch.Shadow, coarse: true);
            }

            // The venom spines of the back in a crest, each with a bead of light at its tip.
            for (int i = 0; i < 7; i++)
            {
                float z = 0.22f - i * 0.07f;
                float h = 0.3f - Mathf.Abs(i - 2) * 0.04f;
                b.Cylinder(new Vector3(0f, 0.18f, z), 0.016f, 0.004f, h, 3, EnemySwatch.IvoryDark, rotation: Quaternion.Euler(-14f, 0f, 0f));
                if (i % 2 == 0)
                {
                    b.Blob(new Vector3(0f, 0.18f + h * 0.96f, z - h * 0.24f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.StolenLight, coarse: true);
                }
            }

            // A thread of glow dripping from the belly spines: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.2f, 0.06f), new Vector3(0f, -0.44f, 0.08f), 0.012f, 0.008f, 3, EnemySwatch.IvoryDark);
            b.Blob(new Vector3(0f, -0.44f, 0.08f), new Vector3(0.12f, 0.14f, 0.12f), EnemySwatch.StolenLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Kongamato
        // ------------------------------------------------------------------

        public static void BuildKongamato(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "Kongamato", 0.62f, DrawKongamatoBody, new Vector3(0f, 0.32f, 0.36f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.28f), EnemySwatch.VampFur);
                    // The bony crest swept back and the long upper beak with its row of teeth.
                    b.Cylinder(new Vector3(0f, 0.06f, -0.06f), 0.05f, 0.005f, 0.26f, 4, EnemySwatch.Leather, rotation: Quaternion.Euler(-120f, 0f, 0f));
                    b.Box(new Vector3(0f, 0.01f, 0.2f), new Vector3(0.07f, 0.04f, 0.26f), EnemySwatch.Leather);
                    for (int i = 0; i < 4; i++)
                    {
                        for (int s = -1; s <= 1; s += 2)
                        {
                            b.Cylinder(new Vector3(0.028f * s, -0.01f, 0.1f + i * 0.06f), 0.008f, 0f, 0.03f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                        }
                    }

                    HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.04f, 0.06f, 0.045f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.04f, 0.12f), b =>
                {
                    b.Box(new Vector3(0f, -0.005f, 0.11f), new Vector3(0.06f, 0.025f, 0.22f), EnemySwatch.Leather);
                    b.Cylinder(new Vector3(0f, 0.008f, 0.2f), 0.008f, 0f, 0.03f, 3, EnemySwatch.Fang);
                },
                EnemySwatch.VampWing, EnemySwatch.Leather, EnemySwatch.BatFurDark, 1.2f, EnemySwatch.VampFur, EnemySwatch.Leather);
        }

        private static void DrawKongamatoBody(EnemyMeshBuilder b)
        {
            // The boatmen of Liyongo's channels say the kongamato only ever upset the canoes, and only of those who poled too fast past its roost. The shadow made it upset every boat, and it throws its hard skin-feathers at whoever is in the channel.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.4f, 0.76f), EnemySwatch.VampFur);
            b.Blob(new Vector3(0f, -0.08f, 0.1f), new Vector3(0.3f, 0.24f, 0.4f), EnemySwatch.Leather, coarse: true);
            b.Segment(new Vector3(0f, 0.08f, 0.26f), new Vector3(0f, 0.3f, 0.34f), 0.07f, 0.06f, 6, EnemySwatch.VampFur);
            // The long thin tail with its diamond tip, the mangrove mud dried on the belly.
            b.Segment(new Vector3(0f, 0f, -0.34f), new Vector3(0f, -0.04f, -0.7f), 0.03f, 0.012f, 4, EnemySwatch.Leather);
            b.Blob(new Vector3(0f, -0.04f, -0.72f), new Vector3(0.1f, 0.03f, 0.1f), EnemySwatch.WarCape, Quaternion.Euler(0f, 45f, 0f), coarse: true);
            b.Blob(new Vector3(0.04f, -0.16f, 0.02f), new Vector3(0.22f, 0.08f, 0.26f), EnemySwatch.BarkDark, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.62f, 0.1f, EnemySwatch.Leather, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Polvo Ladrão
        // ------------------------------------------------------------------

        public static void BuildThiefOctopus(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawThiefOctopus);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.42f, 0.42f, 0.36f), b =>
            {
                // The brass lamp it took out of a drowned house, still lit.
                b.Cylinder(new Vector3(0f, -0.08f, 0f), 0.08f, 0.05f, 0.1f, 6, EnemySwatch.Gold, bottomCap: true);
                b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.12f, 0.14f, 0.12f), EnemySwatch.StolenLight);
                b.Cylinder(new Vector3(0f, 0.12f, 0f), 0.03f, 0f, 0.06f, 4, EnemySwatch.Gold);
            });
        }

        private static void DrawThiefOctopus(EnemyMeshBuilder b)
        {
            // The octopuses of Kilwa lived in the jars of the drowned houses and kept them tidy. Since the shadow they keep things that are not theirs: lamps, beads, the brass pins of the doors. Every arm holds something.
            b.Blob(new Vector3(0f, 0.74f, -0.08f), new Vector3(0.7f, 0.74f, 0.8f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, 0.92f, -0.2f), new Vector3(0.46f, 0.38f, 0.5f), EnemySwatch.CopperDark, coarse: true);
            b.Blob(new Vector3(0f, 0.42f, 0f), new Vector3(0.84f, 0.26f, 0.84f), EnemySwatch.CopperDark);
            // The shadow spreading over the mantle like ink.
            b.Blob(new Vector3(-0.14f, 0.86f, -0.1f), new Vector3(0.3f, 0.3f, 0.36f), EnemySwatch.Shadow, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.18f, 0.6f, 0.26f, 0.11f, EnemySwatch.MoonEye);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI * 2f / 8f + 0.39f;
                Vector3 root = new Vector3(Mathf.Sin(a) * 0.32f, 0.34f, Mathf.Cos(a) * 0.32f);
                Vector3 knee = new Vector3(Mathf.Sin(a) * 0.6f, 0.16f, Mathf.Cos(a) * 0.6f);
                Vector3 tip = new Vector3(Mathf.Sin(a - 0.25f) * 0.82f, 0.03f, Mathf.Cos(a - 0.25f) * 0.82f);
                b.Segment(root, knee, 0.09f, 0.07f, 5, EnemySwatch.WarCape);
                b.Segment(knee, tip, 0.07f, 0.025f, 5, i % 2 == 0 ? EnemySwatch.CopperDark : EnemySwatch.WarCape);
            }

            // What the other arms hold: a string of coral beads, a little teak box with a brass pin, a pearl.
            for (int i = 0; i < 5; i++)
            {
                b.Blob(new Vector3(-0.5f + i * 0.04f, 0.14f + Mathf.Sin(i * 1.3f) * 0.03f, 0.42f - i * 0.04f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Pink, coarse: true);
            }

            b.Box(new Vector3(-0.52f, 0.1f, -0.36f), new Vector3(0.16f, 0.12f, 0.12f), EnemySwatch.Bark, top: EnemySwatch.BarkDark);
            b.Box(new Vector3(-0.52f, 0.13f, -0.29f), new Vector3(0.03f, 0.03f, 0.02f), EnemySwatch.Gold);
            b.Blob(new Vector3(0.5f, 0.12f, -0.4f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.OwlFace, coarse: true);
        }

        // ------------------------------------------------------------------
        // Raiz-Laço
        // ------------------------------------------------------------------

        public static void BuildSnareRoot(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawSnareRootBody);
            // The noose: the upper half of the loop is the head, the lower half snaps up from under it on the windup.
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.24f), b =>
            {
                Vector3 last = new Vector3(0.18f, 0f, 0.14f);
                for (int i = 1; i <= 5; i++)
                {
                    float a = i * Mathf.PI / 5f;
                    var p = new Vector3(Mathf.Cos(a) * 0.18f, Mathf.Sin(a) * 0.18f, 0.14f);
                    b.Segment(last, p, 0.035f, 0.035f, 5, EnemySwatch.Bark);
                    last = p;
                }

                b.Segment(Vector3.zero, new Vector3(0f, 0.06f, 0.14f), 0.06f, 0.04f, 5, EnemySwatch.BarkDark);
                // The knot it tied itself into, with the shadow's eyes in it.
                b.Blob(new Vector3(0f, 0.02f, 0.06f), new Vector3(0.16f, 0.14f, 0.14f), EnemySwatch.BarkDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.045f, 0.04f, 0.12f, 0.04f, EnemySwatch.FireflyGlow);
                for (int i = 0; i < 3; i++)
                {
                    b.Blob(new Vector3(-0.12f + i * 0.12f, 0.15f + (i == 1 ? 0.04f : 0f), 0.14f), new Vector3(0.05f, 0.04f, 0.05f), EnemySwatch.Lichen, coarse: true);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.02f, 0.14f), b =>
            {
                Vector3 last = new Vector3(0.18f, 0f, 0f);
                for (int i = 1; i <= 5; i++)
                {
                    float a = -i * Mathf.PI / 5f;
                    var p = new Vector3(Mathf.Cos(a) * 0.18f, Mathf.Sin(a) * 0.14f, 0f);
                    b.Segment(last, p, 0.03f, 0.03f, 5, EnemySwatch.BarkDark);
                    last = p;
                }

                b.Cylinder(new Vector3(0f, -0.14f, 0f), 0.02f, 0f, 0.06f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.24f), b =>
            {
                // The aerial root it came down on, still trailing behind.
                b.Segment(Vector3.zero, new Vector3(0.04f, 0.18f, -0.18f), 0.05f, 0.04f, 5, EnemySwatch.Bark);
                b.Segment(new Vector3(0.04f, 0.18f, -0.18f), new Vector3(0f, 0.46f, -0.24f), 0.04f, 0.015f, 4, EnemySwatch.BarkDark);
            });
        }

        private static void DrawSnareRootBody(EnemyMeshBuilder b)
        {
            // The banyans of the gorges let their aerial roots down to the water and tie the bridges with them. The shadow taught a few of the roots to tie knots for themselves, and they come down at night and walk the bridges on their rootlets looking for an ankle.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.36f, 0.48f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, 0.08f, -0.02f), new Vector3(0.3f, 0.22f, 0.36f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0.1f, 0.14f, 0.06f), new Vector3(0.14f, 0.08f, 0.16f), EnemySwatch.CaveMoss, coarse: true);
            // The rootlets it walks on, splayed out like a spider's legs: the lowest points of the model.
            for (int i = 0; i < 8; i++)
            {
                float a = (i + 0.5f) * Mathf.PI * 2f / 8f;
                var root = new Vector3(Mathf.Sin(a) * 0.16f, -0.08f, Mathf.Cos(a) * 0.16f);
                var knee = new Vector3(Mathf.Sin(a) * 0.36f, -0.12f, Mathf.Cos(a) * 0.32f);
                var tip = new Vector3(Mathf.Sin(a + 0.1f) * 0.44f, -0.4f, Mathf.Cos(a + 0.1f) * 0.4f);
                b.Segment(root, knee, 0.035f, 0.028f, 4, EnemySwatch.Bark);
                b.Segment(knee, tip, 0.028f, 0.008f, 4, EnemySwatch.BarkDark);
            }
        }

        // ------------------------------------------------------------------
        // Garça-Fantasma
        // ------------------------------------------------------------------

        public static void BuildGhostHeron(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "GhostHeron", 0.84f, DrawGhostHeronBody, new Vector3(0f, 0.6f, 0.28f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.24f), EnemySwatch.OwlFace);
                    // The trailing plume at the back of the head, smoke at its end.
                    b.Segment(new Vector3(0f, 0.04f, -0.08f), new Vector3(0f, -0.02f, -0.3f), 0.02f, 0.005f, 3, EnemySwatch.Shroud);
                    b.Cylinder(new Vector3(0f, 0f, 0.1f), 0.035f, 0.008f, 0.26f, 5, EnemySwatch.IvoryDark, rotation: Quaternion.Euler(90f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.03f, 0.06f, 0.045f, EnemySwatch.SoftEye);
                },
                new Vector3(0f, -0.03f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.12f), new Vector3(0.04f, 0.02f, 0.24f), EnemySwatch.IvoryDark),
                EnemySwatch.OwlFace, EnemySwatch.Shroud, EnemySwatch.Shadow, 1f, EnemySwatch.OwlFace, EnemySwatch.Shroud);
        }

        private static void DrawGhostHeronBody(EnemyMeshBuilder b)
        {
            // The herons of Tarchon fish beside the net-menders and take what falls from the nets, and nobody minds. The shadow came over the marsh in the lilac mist; the herons that stood in it went pale and thin as the mist, and they take from the nets now while the menders watch.
            b.Blob(Vector3.zero, new Vector3(0.38f, 0.4f, 0.66f), EnemySwatch.OwlFace);
            b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.28f, 0.26f, 0.36f), EnemySwatch.Ivory, coarse: true);
            // The long S of the neck and the shadow showing through the back like water under thin ice.
            b.Segment(new Vector3(0f, 0.12f, 0.22f), new Vector3(0f, 0.36f, 0.16f), 0.06f, 0.05f, 6, EnemySwatch.OwlFace);
            b.Segment(new Vector3(0f, 0.36f, 0.16f), new Vector3(0f, 0.6f, 0.28f), 0.05f, 0.045f, 6, EnemySwatch.OwlFace);
            b.Blob(new Vector3(0f, 0.16f, -0.08f), new Vector3(0.26f, 0.1f, 0.4f), EnemySwatch.Shroud, coarse: true);
            b.Segment(new Vector3(0f, 0.14f, -0.24f), new Vector3(0.06f, 0.42f, -0.44f), 0.06f, 0.015f, 4, EnemySwatch.Shadow);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.84f, 0.08f, EnemySwatch.IvoryDark, EnemySwatch.Shroud);
        }

        // ------------------------------------------------------------------
        // Libélula de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeDragonfly(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawSmokeDragonflyBody);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.06f * side, 0.08f, 0f),
                    b => DrawDragonflyWings(b, side), Quaternion.Euler(0f, 0f, 6f * side));
            }

            // The long abdomen: it curls in under the body on the windup, like the bees' sting.
            bp.Mesh(ProceduralEnemyParts.Stinger, body, new Vector3(0f, 0f, -0.14f), b =>
            {
                for (int i = 0; i < 6; i++)
                {
                    var from = new Vector3(0f, -i * 0.012f, -i * 0.09f);
                    var to = new Vector3(0f, -(i + 1) * 0.012f, -(i + 1) * 0.09f);
                    b.Segment(from, to, 0.045f - i * 0.004f, 0.041f - i * 0.004f, 5, i % 2 == 0 ? EnemySwatch.Shadow : EnemySwatch.Verdigris);
                }

                b.Blob(new Vector3(0f, -0.08f, -0.56f), new Vector3(0.04f, 0.04f, 0.06f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawSmokeDragonflyBody(EnemyMeshBuilder b)
        {
            // Over the Ninki Nanka's marsh the dragonflies hunt the midges at dusk, green and quick. The dragon's sleep went sour with the shadow and its breath came out as smoke; the dragonflies that flew through it became smoke too, and they hunt in swarms over the reeds.
            b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.3f), EnemySwatch.Verdigris);
            b.Blob(new Vector3(0f, 0.02f, -0.02f), new Vector3(0.16f, 0.16f, 0.24f), EnemySwatch.Shadow, coarse: true);
            // The round head that is nearly all eyes.
            b.Blob(new Vector3(0f, 0.02f, 0.18f), new Vector3(0.14f, 0.12f, 0.1f), EnemySwatch.Shadow);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.2f), new Vector3(0.09f, 0.1f, 0.09f), EnemySwatch.FireflyGlow, coarse: true);
                // Three pairs of legs hanging under the hover: the lowest point of the model.
                for (int k = 0; k < 3; k++)
                {
                    float z = 0.08f - k * 0.06f;
                    b.Segment(new Vector3(0.04f * s, -0.07f, z), new Vector3(0.12f * s, -0.32f, z + 0.02f), 0.014f, 0.01f, 3, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.12f * s, -0.32f, z + 0.02f), new Vector3(0.1f * s, -0.58f, z), 0.01f, 0.005f, 3, EnemySwatch.Verdigris);
                }
            }

            // The smoke it trails, two puffs over the back.
            b.Blob(new Vector3(0f, 0.12f, -0.12f), new Vector3(0.12f, 0.08f, 0.14f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0.03f, 0.18f, -0.22f), new Vector3(0.08f, 0.06f, 0.1f), EnemySwatch.Shroud, coarse: true);
        }

        private static void DrawDragonflyWings(EnemyMeshBuilder b, float side)
        {
            // Two long wings a side, smoke-grey with a dark spot near each tip.
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            for (int w = 0; w < 2; w++)
            {
                float z = w == 0 ? 0.04f : -0.1f;
                var root0 = new Vector3(0f, 0f, z + 0.03f);
                var root1 = new Vector3(0f, 0f, z - 0.03f);
                var lead = new Vector3(0.28f, 0.02f, z + 0.04f);
                var tip = new Vector3(0.52f, 0.03f, z - 0.02f);
                var trail = new Vector3(0.28f, 0.01f, z - 0.07f);
                b.TwoSided(root0, lead, root1, EnemySwatch.Shroud, EnemySwatch.IvoryDark);
                b.TwoSided(lead, tip, trail, EnemySwatch.Shroud, EnemySwatch.IvoryDark);
                b.TwoSided(lead, trail, root1, EnemySwatch.Shroud, EnemySwatch.IvoryDark);
                b.Box(new Vector3(0.44f, 0.03f, z - 0.01f), new Vector3(0.05f, 0.012f, 0.03f), EnemySwatch.Shadow);
            }

            b.Matrix = Matrix4x4.identity;
        }

        // ------------------------------------------------------------------
        // Bolha de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeBubble(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawSmokeBubble);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.8f, 0f), b =>
            {
                // The ember the Lodo breathed into it, smouldering inside the skin; it flares before the bubble bursts.
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), EnemySwatch.Ember);
                b.Blob(new Vector3(0.04f, 0.04f, 0.02f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawSmokeBubble(EnemyMeshBuilder b)
        {
            // In the heart of the Lodo the black mud breathes, and every breath comes up as a bubble of smoke. Most of them burst on the surface; the ones the shadow holds together float off over the drowned roots and burst on whoever they find.
            b.Blob(new Vector3(0f, 0.8f, 0f), new Vector3(0.8f, 0.76f, 0.8f), EnemySwatch.Shroud);
            b.Blob(new Vector3(-0.1f, 0.92f, 0.08f), new Vector3(0.5f, 0.42f, 0.5f), EnemySwatch.Shadow, coarse: true);
            // The shine on the skin, two dark eyes of soot and a mouth like a seam about to split.
            b.Blob(new Vector3(0.16f, 1.02f, 0.24f), new Vector3(0.12f, 0.08f, 0.06f), EnemySwatch.IvoryDark, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.86f, 0.36f, 0.08f, EnemySwatch.Ember);
            b.Box(new Vector3(0f, 0.72f, 0.38f), new Vector3(0.18f, 0.025f, 0.02f), EnemySwatch.Black);
            // The stem of smoke it rises on, out of the mud: the lowest point of the model.
            float[] ys = { 0.08f, 0.22f, 0.36f };
            for (int i = 0; i < ys.Length; i++)
            {
                float w = 0.28f - i * 0.06f;
                b.Blob(new Vector3(0.03f * (i - 1), ys[i], 0f), new Vector3(w, 0.16f, w), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Shadow, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Filho do Lodo
        // ------------------------------------------------------------------

        public static void BuildMudChild(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawMudChild);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.42f, 0.2f), b =>
            {
                // The drop of the Água Viva it swallowed, shining through its belly.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.12f), EnemySwatch.SoulFire);
                b.Blob(new Vector3(0.03f, 0.03f, 0.02f), new Vector3(0.08f, 0.08f, 0.06f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawMudChild(EnemyMeshBuilder b)
        {
            // The Lodo-que-Engole has no children of its own; the Filhos are bits of it that learnt to swim. They climb the ghats of Jhilpur in the Maré de Lodo and swim the dark water of the heart, and wherever they go they try to swallow a little of the clean water to make it black.
            b.Blob(new Vector3(0f, 0.4f, 0f), new Vector3(0.62f, 0.72f, 0.56f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.68f, -0.02f), new Vector3(0.42f, 0.3f, 0.4f), EnemySwatch.Black, coarse: true);
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.76f, 0.16f, 0.7f), EnemySwatch.Black);
            HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.6f, 0.24f, 0.08f, EnemySwatch.MoonEye);
            b.Box(new Vector3(0f, 0.5f, 0.27f), new Vector3(0.14f, 0.03f, 0.02f), EnemySwatch.Shadow);
            // The stubby arms it swims with, dripping, and the drips on the ground round it.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.26f * s, 0.46f, 0.04f), new Vector3(0.42f * s, 0.26f, 0.14f), 0.08f, 0.06f, 5, EnemySwatch.BarkDark);
                b.Blob(new Vector3(0.44f * s, 0.24f, 0.15f), new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.Black, coarse: true);
                b.Blob(new Vector3(0.36f * s, 0.02f, -0.22f), new Vector3(0.14f, 0.04f, 0.12f), EnemySwatch.Black, coarse: true);
            }

            b.Blob(new Vector3(-0.08f, 0.82f, 0.06f), new Vector3(0.12f, 0.08f, 0.12f), EnemySwatch.CaveMoss, coarse: true);
        }
    }
}
