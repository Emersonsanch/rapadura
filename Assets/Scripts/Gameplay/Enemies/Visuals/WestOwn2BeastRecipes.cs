using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-seventh pass of beasts (bible §3.2.1): the roles of the Costa das Monções (Docs/Mapas100.md 25–29), the Delta de Jhilpur
    /// (33, 35) and the Planície de Mingcheng (41, 46) that the continuous world still played with another beast after the twenty-fourth pass
    /// (<see cref="WestOwnBeastRecipes"/>), low-poly from the shared palette. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Dingonek</b> (29, ~1.7 m long): the scaly river beast of the Kenyan tales, two tusks down from the upper jaw, a spotted hide of scales. Jaw, tail, four legs.</item>
    /// <item><b>Chacal-Listrado Apagado</b> (25–26, ~0.9 m): the side-striped jackal of the coast, its white tail-tip gone grey. Jaw, tail, four legs.</item>
    /// <item><b>Hiena de Fumaça</b> (26, ~1.1 m): a spotted hyena, the shoulders high and gone to smoke. Jaw, tail, four legs.</item>
    /// <item><b>Carneiro de Pedra Apagado</b> (27, ~1.3 m): a sandstone ram of Meroë's avenues, off its plinth at night. Head (drops on the windup), jaw, tail, four legs.</item>
    /// <item><b>Crocodilo de Musgo</b> (33, ~2.8 m): a broad crocodile of Tarchon's swamp grown over with moss and reed. Jaw, tail, four legs.</item>
    /// <item><b>Gavial-Sombra</b> (35, ~3 m): a gharial of the black mire half gone to shadow, its snout thin as a reed. Jaw, tail, four legs.</item>
    /// <item><b>Névoa do Shen</b> (Vale da Névoa, ~1.4 m): the breath of the shen, the clam of the old books whose sigh draws towers in the air. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Bolha de Hundun</b> (46, ~1.1 m): a faceless yellow-red sack with six little legs and four little wings, as the Shanhai jing draws Hundun. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Lanterna de Charada Apagada</b> (41, ~1.5 m): a six-sided palace lantern of the Festival of Lanterns, its riddle slip hanging. <c>Body</c> → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class WestOwn2BeastRecipes
    {
        // ------------------------------------------------------------------
        // Dingonek
        // ------------------------------------------------------------------

        public static void BuildDingonek(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawDingonekBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.62f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.3f, 0.36f), EnemySwatch.Lichen);
                b.Blob(new Vector3(0f, -0.04f, 0.18f), new Vector3(0.24f, 0.18f, 0.24f), EnemySwatch.Lichen, coarse: true);
                b.Blob(new Vector3(0f, 0.1f, -0.02f), new Vector3(0.28f, 0.08f, 0.26f), EnemySwatch.BarkDark, coarse: true);
                // The two long tusks hanging down from the upper jaw, as the hunters of the river told it, and the small ears of a river beast.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.08f * s, -0.06f, 0.22f), 0.025f, 0f, 0.22f, 4, EnemySwatch.Bone, rotation: Quaternion.Euler(170f, 0f, 0f));
                    b.Cylinder(new Vector3(0.13f * s, 0.12f, -0.08f), 0.035f, 0.01f, 0.08f, 4, EnemySwatch.BarkDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.07f, 0.12f, 0.05f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.12f), new Vector3(0.2f, 0.04f, 0.22f), EnemySwatch.CaveBelly);
                b.Box(new Vector3(0f, 0.012f, 0.12f), new Vector3(0.14f, 0.01f, 0.16f), EnemySwatch.Pink);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.6f), b =>
            {
                // The long scaled tail of a beast that lives half in the river, with the flat tip it steers by.
                b.Segment(Vector3.zero, new Vector3(0f, -0.14f, -0.36f), 0.1f, 0.06f, 5, EnemySwatch.Lichen);
                b.Segment(new Vector3(0f, -0.14f, -0.36f), new Vector3(0.06f, -0.3f, -0.62f), 0.06f, 0.02f, 5, EnemySwatch.BarkDark);
                b.Blob(new Vector3(0.06f, -0.3f, -0.62f), new Vector3(0.12f, 0.03f, 0.14f), EnemySwatch.BarkDark, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.52f, 0.2f, 0.36f, -0.36f, 0.075f, EnemySwatch.Lichen, EnemySwatch.BarkDark, "Dingonek");
        }

        private static void DrawDingonekBody(EnemyMeshBuilder b)
        {
            // The dingonek kept the fords of the rivers that run down to the coast, and let only the honest cross; the boatmen of Kilwa threw it a fish on the way over. When Kilwa went under the sea the fords went under with it, and the shadow found the dingonek alone in the drowned streets, guarding a ford that is not there any more. Now it lets no one cross.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.5f, 1.12f), EnemySwatch.Lichen);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.5f, 0.26f, 0.86f), EnemySwatch.CaveBelly, coarse: true);
            b.Segment(new Vector3(0f, 0.06f, 0.4f), new Vector3(0f, 0.12f, 0.58f), 0.16f, 0.14f, 6, EnemySwatch.Lichen);
            // The scales like a pangolin's down its back, the dark spots of a leopard between them, and a sea-shell stuck where a scale broke off.
            for (int i = 0; i < 5; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.12f * s, 0.22f, -0.4f + i * 0.2f), new Vector3(0.16f, 0.05f, 0.14f), EnemySwatch.BarkDark, Quaternion.Euler(-14f, 0f, -18f * s));
                }

                b.Blob(new Vector3(i % 2 == 0 ? 0.26f : -0.26f, 0.04f, -0.36f + i * 0.18f), new Vector3(0.08f, 0.1f, 0.1f), EnemySwatch.Black, coarse: true);
            }

            b.Blob(new Vector3(-0.1f, 0.24f, 0.1f), new Vector3(0.1f, 0.06f, 0.1f), EnemySwatch.Ivory, coarse: true);
            b.Blob(new Vector3(0.12f, 0.1f, -0.22f), new Vector3(0.22f, 0.14f, 0.3f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Chacal-Listrado Apagado
        // ------------------------------------------------------------------

        public static void BuildStripedJackal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawStripedJackalBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.4f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), EnemySwatch.Sandstone);
                // The narrow muzzle and the big ears of a jackal, held out sideways to catch the wind off the dunes.
                b.Blob(new Vector3(0f, -0.04f, 0.14f), new Vector3(0.1f, 0.09f, 0.18f), EnemySwatch.Sandstone, coarse: true);
                b.Blob(new Vector3(0f, -0.02f, 0.23f), new Vector3(0.04f, 0.035f, 0.035f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.TwoSided(new Vector3(0.05f * s, 0.08f, -0.02f), new Vector3(0.2f * s, 0.2f, -0.06f), new Vector3(0.1f * s, 0.06f, -0.1f), EnemySwatch.SandstoneDark, EnemySwatch.WolfBelly);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.055f, 0.03f, 0.09f, 0.04f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.09f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.07f), new Vector3(0.08f, 0.02f, 0.14f), EnemySwatch.WolfBelly);
                b.Cylinder(new Vector3(0.024f, 0.004f, 0.12f), 0.009f, 0f, 0.03f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.024f, 0.004f, 0.12f), 0.009f, 0f, 0.03f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.4f), b =>
            {
                // The bushy tail with the white tip the side-striped jackal is known by; the shadow greyed the tip first.
                b.Segment(Vector3.zero, new Vector3(0f, -0.14f, -0.18f), 0.05f, 0.06f, 5, EnemySwatch.SandstoneDark);
                b.Segment(new Vector3(0f, -0.14f, -0.18f), new Vector3(0f, -0.24f, -0.26f), 0.06f, 0.02f, 4, EnemySwatch.GolemStone);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.4f, 0.14f, 0.24f, -0.26f, 0.045f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, "StripedJackal");
        }

        private static void DrawStripedJackalBody(EnemyMeshBuilder b)
        {
            // The side-striped jackals hunted the dunes of the casuarinas at dusk and ate what the tide left, and the fishermen of Taa Pwani counted on them to keep the beach clean. The shadow came down the coast with the monsoon and blew into their dens: they hunt in packs by day now, and the fishermen stay off the dunes.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.34f, 0.76f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, -0.07f, 0.04f), new Vector3(0.28f, 0.18f, 0.56f), EnemySwatch.WolfBelly, coarse: true);
            b.Segment(new Vector3(0f, 0.06f, 0.26f), new Vector3(0f, 0.15f, 0.38f), 0.09f, 0.08f, 6, EnemySwatch.Sandstone);
            // The pale stripe down each flank with the dark edge under it, and the loose fur of the shoulders blown out sideways by the monsoon.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.175f * s, 0.04f, 0f), new Vector3(0.02f, 0.03f, 0.52f), EnemySwatch.SandstoneLight);
                b.Box(new Vector3(0.172f * s, 0.005f, 0f), new Vector3(0.02f, 0.025f, 0.5f), EnemySwatch.Black);
                b.Cylinder(new Vector3(0.18f * s, 0.06f, 0.2f), 0.04f, 0f, 0.1f, 3, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(-20f, 0f, -80f * s));
            }

            b.Blob(new Vector3(0f, 0.12f, -0.06f), new Vector3(0.22f, 0.08f, 0.4f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(-0.06f, 0.06f, -0.16f), new Vector3(0.12f, 0.1f, 0.14f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Hiena de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeHyena(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.54f, 0f), DrawSmokeHyenaBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.5f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.26f, 0.26f), EnemySwatch.SandstoneDark);
                // The heavy blunt muzzle that cracks bones, the round ears, and the dark face.
                b.Blob(new Vector3(0f, -0.05f, 0.15f), new Vector3(0.16f, 0.14f, 0.18f), EnemySwatch.Leather, coarse: true);
                b.Blob(new Vector3(0f, -0.02f, 0.24f), new Vector3(0.06f, 0.05f, 0.04f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.1f * s, 0.12f, -0.04f), new Vector3(0.08f, 0.09f, 0.04f), EnemySwatch.Leather, coarse: true);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.04f, 0.1f, 0.045f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.13f, 0.035f, 0.16f), EnemySwatch.Leather);
                b.Cylinder(new Vector3(0.04f, 0.008f, 0.14f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.008f, 0.14f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.16f, -0.12f), 0.04f, 0.035f, 4, EnemySwatch.SandstoneDark);
                b.Segment(new Vector3(0f, -0.16f, -0.12f), new Vector3(0f, -0.26f, -0.16f), 0.05f, 0.02f, 4, EnemySwatch.Black);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.48f, 0.15f, 0.28f, -0.26f, 0.055f, EnemySwatch.SandstoneDark, EnemySwatch.Black, "SmokeHyena");
        }

        private static void DrawSmokeHyenaBody(EnemyMeshBuilder b)
        {
            // In the acacias the hyenas laughed at night and the people of Aratta laughed back, and that was how they knew where the other was. The shadow put out the laugh first: the hyenas go silent now, and smoke rises off their shoulders where the mane was, and they come out of the dark in a pack without a sound.
            b.Blob(Vector3.zero, new Vector3(0.44f, 0.42f, 0.84f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.08f, 0.02f), new Vector3(0.34f, 0.22f, 0.6f), EnemySwatch.Sandstone, coarse: true);
            // The high shoulders sloping down to the haunches, the mane of smoke standing off the neck and the spots of the hide.
            b.Blob(new Vector3(0f, 0.12f, 0.22f), new Vector3(0.42f, 0.34f, 0.34f), EnemySwatch.SandstoneDark, coarse: true);
            b.Segment(new Vector3(0f, 0.1f, 0.3f), new Vector3(0f, 0.12f, 0.46f), 0.12f, 0.1f, 6, EnemySwatch.SandstoneDark);
            for (int i = 0; i < 5; i++)
            {
                float z = 0.36f - i * 0.12f;
                b.Blob(new Vector3(0f, 0.26f - i * 0.02f, z), new Vector3(0.16f + (i % 2) * 0.06f, 0.14f, 0.12f), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Shadow, coarse: true);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.24f * s, 0.18f, 0.24f), new Vector3(0.1f, 0.12f, 0.1f), EnemySwatch.Shroud, coarse: true);
                for (int i = 0; i < 3; i++)
                {
                    b.Blob(new Vector3(0.2f * s, 0.02f - i * 0.03f, 0.14f - i * 0.2f), new Vector3(0.06f, 0.07f, 0.07f), EnemySwatch.Black, coarse: true);
                }
            }
        }

        // ------------------------------------------------------------------
        // Carneiro de Pedra Apagado
        // ------------------------------------------------------------------

        public static void BuildStoneRam(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.82f, 0f), DrawStoneRamBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.28f, 0.58f), DrawStoneRamHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.12f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.12f, 0.04f, 0.14f), EnemySwatch.SandstoneDark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.52f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.12f, -0.06f), 0.06f, 0.04f, 4, EnemySwatch.SandstoneDark));
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.6f, 0.22f, 0.36f, -0.34f, 0.08f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, "StoneRam");
        }

        private static void DrawStoneRamBody(EnemyMeshBuilder b)
        {
            // The rams of the avenues of Meroë lay on their plinths with a little king standing between their front legs, and watched the road to the temples for a thousand years. The shadow got into the cracks of the sandstone: by day they lie still as ever, but at night they get up off the plinth and walk the avenue, and butt whoever comes up it.
            b.Box(Vector3.zero, new Vector3(0.6f, 0.5f, 1.04f), EnemySwatch.Sandstone, top: EnemySwatch.SandstoneLight);
            b.Blob(new Vector3(0f, 0.08f, 0.32f), new Vector3(0.56f, 0.48f, 0.44f), EnemySwatch.Sandstone, coarse: true);
            // The carved fleece in rows of curls, the little king's feet still on the chest, and the glowing cracks where the shadow went in.
            for (int i = 0; i < 4; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.16f * s, 0.26f, -0.36f + i * 0.22f), new Vector3(0.14f, 0.06f, 0.14f), EnemySwatch.SandstoneDark, coarse: true);
                }
            }

            b.Box(new Vector3(0f, -0.06f, 0.54f), new Vector3(0.18f, 0.14f, 0.08f), EnemySwatch.SandstoneDark);
            b.Box(new Vector3(0.18f, 0.1f, -0.1f), new Vector3(0.02f, 0.3f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 20f));
            b.Box(new Vector3(-0.2f, 0.02f, 0.2f), new Vector3(0.02f, 0.22f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -24f));
            b.Box(new Vector3(0f, 0.27f, 0f), new Vector3(0.1f, 0.02f, 0.6f), EnemySwatch.Shadow);
        }

        private static void DrawStoneRamHead(EnemyMeshBuilder b)
        {
            b.Box(Vector3.zero, new Vector3(0.22f, 0.24f, 0.32f), EnemySwatch.Sandstone, top: EnemySwatch.SandstoneLight);
            b.Box(new Vector3(0f, -0.04f, 0.16f), new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.Sandstone);
            HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.03f, 0.08f, 0.04f, EnemySwatch.GolemCrack);
            // The carved horns curled round the ears, a full turn each, worn smooth where the pilgrims touched them.
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                Vector3 prev = new Vector3(0.08f * side, 0.1f, -0.04f);
                for (int i = 1; i <= 6; i++)
                {
                    float a = i * 60f * Mathf.Deg2Rad;
                    float r = 0.05f + i * 0.02f;
                    var next = new Vector3((0.1f + i * 0.03f) * side, 0.06f + Mathf.Cos(a) * r, -0.02f - Mathf.Sin(a) * r);
                    b.Segment(prev, next, 0.06f - i * 0.006f, 0.055f - i * 0.006f, 5, i % 2 == 0 ? EnemySwatch.SandstoneDark : EnemySwatch.Sandstone);
                    prev = next;
                }
            }
        }

        // ------------------------------------------------------------------
        // Crocodilo de Musgo
        // ------------------------------------------------------------------

        public static void BuildMossCrocodile(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawMossCrocodileBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.74f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.22f, 0.3f), EnemySwatch.CaveMoss);
                // The broad blunt snout of a swamp crocodile, the eyes and nostrils just above the water line, moss on the brow.
                b.Box(new Vector3(0f, -0.01f, 0.3f), new Vector3(0.26f, 0.1f, 0.44f), EnemySwatch.CaveMoss);
                b.Blob(new Vector3(0f, 0.05f, 0.5f), new Vector3(0.1f, 0.05f, 0.06f), EnemySwatch.BarkDark, coarse: true);
                b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.3f, 0.06f, 0.2f), EnemySwatch.Lichen, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.11f, 0.04f, 0.06f, EnemySwatch.SoftEye);
                for (int i = 0; i < 4; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.11f * s, -0.05f, 0.16f + i * 0.1f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.22f), new Vector3(0.24f, 0.05f, 0.42f), EnemySwatch.CaveBelly);
                b.Box(new Vector3(0f, 0.016f, 0.2f), new Vector3(0.16f, 0.01f, 0.34f), EnemySwatch.Pink);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.7f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.42f), 0.2f, 0.11f, 6, EnemySwatch.CaveMoss);
                b.Segment(new Vector3(0f, -0.04f, -0.42f), new Vector3(-0.05f, -0.1f, -0.76f), 0.11f, 0.02f, 5, EnemySwatch.Lichen);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, 0.12f - i * 0.025f, -0.08f - i * 0.18f), new Vector3(0.03f, 0.08f - i * 0.012f, 0.1f), EnemySwatch.BarkDark);
                }
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.3f, 0.34f, 0.42f, -0.42f, 0.085f, EnemySwatch.CaveMoss, EnemySwatch.BarkDark, "MossCrocodile");
        }

        private static void DrawMossCrocodileBody(EnemyMeshBuilder b)
        {
            // The crocodiles of the open swamp slept so long under the stilts of Tarchon that moss grew on their backs, and the trolls planted reeds in it for luck. The shadow came up the swamp in the lilac mist and woke them: now the moss moves, and the reeds with it, and the nets of Tarchon come up torn.
            b.Blob(Vector3.zero, new Vector3(0.8f, 0.42f, 1.5f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, -0.08f, 0f), new Vector3(0.66f, 0.22f, 1.3f), EnemySwatch.CaveBelly, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                // The scutes under the moss, the mats of moss between them, and the reeds the trolls planted, still standing.
                float z = -0.56f + i * 0.22f;
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.12f * s, 0.2f, z), new Vector3(0.12f, 0.07f, 0.14f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, -12f * s));
                }

                b.Blob(new Vector3(i % 2 == 0 ? 0.16f : -0.16f, 0.23f, z + 0.1f), new Vector3(0.2f, 0.06f, 0.18f), EnemySwatch.Lichen, coarse: true);
            }

            for (int i = 0; i < 3; i++)
            {
                b.Cylinder(new Vector3(-0.1f + i * 0.1f, 0.22f, -0.2f + i * 0.24f), 0.012f, 0.006f, 0.3f - i * 0.04f, 3, EnemySwatch.Bark, rotation: Quaternion.Euler(-8f + i * 6f, 0f, 10f - i * 10f));
            }

            b.Blob(new Vector3(0.06f, 0.2f, 0.3f), new Vector3(0.2f, 0.05f, 0.22f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Gavial-Sombra
        // ------------------------------------------------------------------

        public static void BuildShadowGharial(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawShadowGharialBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0f, 0.72f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.2f, 0.26f), EnemySwatch.Shadow);
                // The snout thin as a reed, longer than the gharials of the lake, and the eyes of cold light that see in the black water.
                b.Box(new Vector3(0f, -0.01f, 0.38f), new Vector3(0.1f, 0.07f, 0.62f), EnemySwatch.Shadow);
                b.Blob(new Vector3(0f, 0.02f, 0.68f), new Vector3(0.12f, 0.08f, 0.1f), EnemySwatch.Black, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.09f, 0.02f, 0.055f, EnemySwatch.Sorcery);
                for (int i = 0; i < 6; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.048f * s, -0.035f, 0.14f + i * 0.09f), 0.009f, 0f, 0.03f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.055f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.28f), new Vector3(0.09f, 0.035f, 0.56f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, 0.01f, 0.26f), new Vector3(0.06f, 0.01f, 0.48f), EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.7f), b =>
            {
                // The tail thins into black smoke at the end: there is less of it every year.
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.4f), 0.18f, 0.09f, 6, EnemySwatch.Shadow);
                b.Segment(new Vector3(0f, -0.04f, -0.4f), new Vector3(0.05f, -0.08f, -0.7f), 0.09f, 0.03f, 5, EnemySwatch.Black);
                b.Blob(new Vector3(0.06f, -0.06f, -0.78f), new Vector3(0.16f, 0.1f, 0.14f), EnemySwatch.Shroud, coarse: true);
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.28f, 0.3f, 0.4f, -0.4f, 0.075f, EnemySwatch.Shadow, EnemySwatch.Black, "ShadowGharial");
        }

        private static void DrawShadowGharialBody(EnemyMeshBuilder b)
        {
            // The Lodo is the fourth of the worlds of the Darkness, and its people are the ones who swim. The gharials that went down the delta after the fish swam into the black water at the heart of it and did not come back up the same: half of them is shadow now, and the shadow half is the one that is hungry.
            b.Blob(Vector3.zero, new Vector3(0.7f, 0.4f, 1.5f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.08f, 0f), new Vector3(0.58f, 0.2f, 1.3f), EnemySwatch.IvoryDark, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                float z = -0.56f + i * 0.22f;
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.1f * s, 0.19f, z), new Vector3(0.11f, 0.07f, 0.13f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, -12f * s));
                }
            }

            // The half that went to shadow: smoke lifting off the left flank, and the cracks of violet light of the mire's bubbles.
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(-0.3f, 0.06f + i * 0.03f, -0.4f + i * 0.26f), new Vector3(0.16f, 0.18f, 0.18f), EnemySwatch.Shroud, coarse: true);
            }

            b.Box(new Vector3(0.2f, 0.12f, 0.1f), new Vector3(0.02f, 0.02f, 0.4f), EnemySwatch.Sorcery, Quaternion.Euler(0f, 14f, 0f));
        }

        // ------------------------------------------------------------------
        // Névoa do Shen
        // ------------------------------------------------------------------

        public static void BuildShenMist(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.8f, 0f), DrawShenMistBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.12f, 0.06f), b =>
            {
                // The pearl in the open shell: the light the shen swallowed, which it breathes out as towers of mist.
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawShenMistBody(EnemyMeshBuilder b)
        {
            // The old books of the canal say the shen is a clam of the deep river, and when it sighs its breath draws towers and walls in the air over the water. In the Vale da Névoa the shen sighed the shadow's smoke: the towers it draws now are towers of nothing, and whoever walks into them gets lost.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.36f, 0.56f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0.12f, 0.12f, -0.08f), new Vector3(0.4f, 0.3f, 0.4f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(-0.16f, 0.04f, 0.08f), new Vector3(0.36f, 0.26f, 0.36f), EnemySwatch.Shroud, coarse: true);
            // The mirage it breathes: three little towers of mist with curled roofs standing on the cloud.
            for (int i = 0; i < 3; i++)
            {
                float x = -0.18f + i * 0.18f;
                float h = 0.18f + (i == 1 ? 0.1f : 0f);
                b.Box(new Vector3(x, 0.16f + h * 0.5f, -0.04f), new Vector3(0.06f, h, 0.06f), EnemySwatch.GolemSnow);
                b.Cylinder(new Vector3(x, 0.16f + h, -0.04f), 0.07f, 0f, 0.06f, 4, EnemySwatch.Shroud);
            }

            // The two halves of the shell open under the cloud, and the mist pouring down from it to the water: the lowest point of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0f, -0.16f, -0.1f), new Vector3(0.2f * s, -0.1f, 0.06f), new Vector3(0f, -0.14f, 0.18f), EnemySwatch.Ivory, EnemySwatch.IvoryDark);
            }

            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0.04f * (i % 2 == 0 ? 1f : -1f), -0.26f - i * 0.14f, 0.02f), new Vector3(0.34f - i * 0.04f, 0.14f, 0.3f - i * 0.03f), EnemySwatch.Shroud, coarse: true);
            }

            b.Blob(new Vector3(0f, -0.74f, 0.02f), new Vector3(0.4f, 0.06f, 0.36f), EnemySwatch.GolemSnow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Bolha de Hundun
        // ------------------------------------------------------------------

        public static void BuildHundunBubble(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawHundunBubbleBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0f), b =>
            {
                // The lights it swallowed, glowing through the skin of the sack: Hundun eats, and has no mouth to let anything out.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.18f, 0.2f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0.06f, 0.04f, 0.04f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawHundunBubbleBody(EnemyMeshBuilder b)
        {
            // The old book of the mountains and seas draws Hundun as a yellow sack, red as cinnabar fire, with six legs and four wings and no face at all. In the Citadel of the Four Gluttons it is the one that eats the light, and the bubbles that come off it eat it too, a little at a time, and float on swollen.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.54f, 0.6f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, 0.08f, 0.04f), new Vector3(0.46f, 0.38f, 0.46f), EnemySwatch.WarCape, coarse: true);
            b.Blob(new Vector3(-0.1f, 0.16f, 0.12f), new Vector3(0.12f, 0.1f, 0.08f), EnemySwatch.SandstoneLight, coarse: true);
            // The four small wings on the back, folded like a moth's.
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.12f * s, 0.22f, 0.06f), new Vector3(0.34f * s, 0.36f, 0.02f), new Vector3(0.16f * s, 0.24f, -0.08f), EnemySwatch.Gold, EnemySwatch.WarCape);
                b.TwoSided(new Vector3(0.12f * s, 0.2f, -0.1f), new Vector3(0.3f * s, 0.3f, -0.2f), new Vector3(0.14f * s, 0.2f, -0.2f), EnemySwatch.WarCape, EnemySwatch.Gold);
            }

            // The six stubby legs it paddles the air with, three to a side, just touching the floor: the lowest points of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    var hip = new Vector3(0.16f * s, -0.2f, -0.16f + i * 0.16f);
                    var toe = new Vector3(0.22f * s, -0.52f, -0.18f + i * 0.18f);
                    b.Segment(hip, toe, 0.04f, 0.025f, 4, EnemySwatch.WarCape);
                    b.Blob(toe, new Vector3(0.06f, 0.04f, 0.08f), EnemySwatch.Black, coarse: true);
                }
            }

            // Where a face should be, nothing: only the skin of the sack pulled smooth, and a little smoke where it breathes anyway.
            b.Blob(new Vector3(0f, 0.02f, 0.28f), new Vector3(0.3f, 0.26f, 0.06f), EnemySwatch.SandstoneLight, coarse: true);
            b.Blob(new Vector3(0f, 0.3f, -0.08f), new Vector3(0.14f, 0.12f, 0.14f), EnemySwatch.Shroud, coarse: true);
        }

        // ------------------------------------------------------------------
        // Lanterna de Charada Apagada
        // ------------------------------------------------------------------

        public static void BuildRiddleLantern(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.9f, 0f), DrawRiddleLanternBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0f), b =>
            {
                // The candle inside, caught by the shadow: it flares before the lantern throws its bolt.
                b.Blob(Vector3.zero, new Vector3(0.14f, 0.2f, 0.14f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.07f, 0.1f, 0.07f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawRiddleLanternBody(EnemyMeshBuilder b)
        {
            // At the Festival of Lanterns the people of Mingcheng hang a riddle on every lantern, and whoever guesses it takes the lantern home. The shadow took the lanterns nobody guessed, and put out the candle in them: now they drift over the villages of the ring with their riddles still hanging, and throw their cold light at whoever cannot answer.
            b.Cylinder(new Vector3(0f, -0.22f, 0f), 0.24f, 0.24f, 0.44f, 6, EnemySwatch.IvoryDark);
            for (int i = 0; i < 6; i++)
            {
                // The six carved ribs of the palace lantern, and the paper between them gone grey.
                float a = (i * 60f + 30f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 0.25f, 0f, Mathf.Sin(a) * 0.25f), new Vector3(0.04f, 0.48f, 0.04f), EnemySwatch.WarCape);
            }

            b.Cylinder(new Vector3(0f, 0.22f, 0f), 0.3f, 0.12f, 0.1f, 6, EnemySwatch.WarCape, bottomCap: true);
            b.Cylinder(new Vector3(0f, -0.3f, 0f), 0.14f, 0.3f, 0.08f, 6, EnemySwatch.WarCape, bottomCap: true);
            b.Cylinder(new Vector3(0f, 0.32f, 0f), 0.04f, 0.04f, 0.14f, 6, EnemySwatch.Gold);
            // The little tassels at the six corners of the roof.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f) * Mathf.Deg2Rad;
                b.Segment(new Vector3(Mathf.Cos(a) * 0.3f, 0.22f, Mathf.Sin(a) * 0.3f), new Vector3(Mathf.Cos(a) * 0.32f, 0.08f, Mathf.Sin(a) * 0.32f), 0.012f, 0.03f, 4, EnemySwatch.WarCape);
            }

            // The great tassel under it and the riddle slip hanging from it, the characters gone faint: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.3f, 0f), new Vector3(0f, -0.5f, 0f), 0.02f, 0.02f, 4, EnemySwatch.Gold);
            b.Cylinder(new Vector3(0f, -0.7f, 0f), 0.08f, 0.03f, 0.2f, 6, EnemySwatch.WarCape);
            b.Box(new Vector3(0.06f, -0.74f, 0.04f), new Vector3(0.08f, 0.28f, 0.01f), EnemySwatch.Ivory);
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0.06f, -0.66f - i * 0.08f, 0.046f), new Vector3(0.04f, 0.03f, 0.004f), EnemySwatch.Black);
            }

            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.36f, 0.1f, 0.36f), EnemySwatch.Shadow, coarse: true);
        }
    }
}
