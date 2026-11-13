using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The map bosses of the other pits of the Limiar Profundo and of the bridge (Docs/Mapas100.md 84, 85, 87, 88, 89), built like the rest of
    /// <see cref="MapBossRecipes"/>: pivot at the feet, facing +Z, the glowing <c>Core</c> the rig feeds with the fight's phase, and soft eyes, because
    /// each is a creature gone dark (or, the Guardian, bound), not a villain.
    /// <list type="bullet">
    /// <item><b>O Rei da Fumaça</b> (84, ~3.2 m): a two-legged figure of grey smoke with no face of its own, a crown of wisps, an amber spark in its chest; two legs.</item>
    /// <item><b>A Leoa do Fogo Devorador</b> (85, ~2.9 m): a great lioness of black rock with ember cracks and a mane of flames; four legs and a <c>Tail</c>.</item>
    /// <item><b>O Peixe de Cauda Negra</b> (87, ~3.2 m): a fish reared up out of a pool of black water, its pectoral fins for arms, a black tail curled behind
    /// and the lure of light it will become on its brow (the <c>Core</c>); no legs.</item>
    /// <item><b>O Dragão do Fundo</b> (88, ~2.8 m): a long dark dragon on four short legs, neck raised, a little lizard of light in its chest; four legs and a <c>Tail</c>.</item>
    /// <item><b>O Guardião do Novelo</b> (89, ~3 m): a tall old ushabti of ivory and copper, the dark globe of the Novelo strapped on his back, threads of it round
    /// his arms, a hoe in his hand; two legs.</item>
    /// </list>
    /// </summary>
    public static partial class MapBossRecipes
    {
        // ------------------------------------------------------------------
        // O Rei da Fumaça (84)
        // ------------------------------------------------------------------

        public static void BuildSmokeKing(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.75f, 0f), b =>
            {
                // The smoke of the first world of the Darkness, standing up on two legs in whatever shape came in last: grey, drifting, never quite still.
                b.Blob(Vector3.zero, new Vector3(1f, 1.2f, 0.75f), EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, 0.3f, 0.06f), new Vector3(0.92f, 0.6f, 0.62f), EnemySwatch.GolemStone, coarse: true);
                b.Blob(new Vector3(0f, -0.5f, 0f), new Vector3(0.82f, 0.5f, 0.62f), EnemySwatch.Shadow, coarse: true);
                // Wisps coming off it all round.
                for (int i = 0; i < 7; i++)
                {
                    float a = i * Mathf.PI * 2f / 7f + 0.4f;
                    b.Blob(new Vector3(Mathf.Sin(a) * 0.46f, -0.3f + 0.14f * (i % 4), Mathf.Cos(a) * 0.32f), new Vector3(0.3f, 0.36f, 0.3f), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.GolemStone, coarse: true);
                }
            });
            // The amber of a lamp it swallowed: it shows through as the smoke thins.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.15f, 0.36f), b => b.Blob(Vector3.zero, new Vector3(0.28f, 0.28f, 0.14f), EnemySwatch.Ember, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.86f, 0.04f), b =>
            {
                // No face of its own: a smooth head of smoke, two faint eyes, and a crown of wisps rising.
                b.Blob(Vector3.zero, new Vector3(0.56f, 0.6f, 0.5f), EnemySwatch.GolemStone, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.04f, 0.22f, 0.08f, EnemySwatch.SoftEye);
                for (int i = -2; i <= 2; i++)
                {
                    b.Horn(new Vector3(i * 0.1f, 0.2f, -0.04f), 0.06f, 0.36f, 3, -12f, i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Shadow, Quaternion.Euler(-12f, 0f, -i * 10f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.18f, 0.2f), b => b.Blob(Vector3.zero, new Vector3(0.22f, 0.06f, 0.08f), EnemySwatch.Shadow, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Long arms of smoke that thin out to hands: they rise on the windup, as the copy comes up.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.52f * side, 0.42f, 0.04f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.16f * side, -0.5f, 0.12f), 0.15f, 0.11f, 6, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.16f * side, -0.5f, 0.12f), new Vector3(0.1f * side, -0.92f, 0.24f), 0.11f, 0.06f, 6, EnemySwatch.GolemStone);
                    b.Blob(new Vector3(0.1f * side, -0.96f, 0.26f), new Vector3(0.16f, 0.16f, 0.16f), EnemySwatch.Shadow, coarse: true);
                }, meshKey: right ? "SmokeKingArm_R" : "SmokeKingArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.1f, 0.22f, 0.13f, EnemySwatch.Shroud, EnemySwatch.Shadow, EnemySwatch.Black, "SmokeKing");
        }

        // ------------------------------------------------------------------
        // A Leoa do Fogo Devorador (85)
        // ------------------------------------------------------------------

        public static void BuildDevouringLioness(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.35f, -0.1f), b =>
            {
                // The fire that eats, on four legs: black rock with embers in the cracks, lean and long.
                b.Blob(Vector3.zero, new Vector3(1f, 0.95f, 1.9f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, 0.2f, 0.5f), new Vector3(1.02f, 0.92f, 0.9f), EnemySwatch.GolemStoneDark, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        b.Box(new Vector3(0.48f * s, -0.05f + 0.1f * i, -0.5f + 0.42f * i), new Vector3(0.04f, 0.36f, 0.05f), EnemySwatch.Ember, Quaternion.Euler(20f * (i - 1), 0f, 0f));
                    }
                }
            });
            // The good fire she swallowed, still burning bright inside her.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.02f, 0.96f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.16f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 1.02f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.68f, 0.64f, 0.76f), EnemySwatch.Black, coarse: true);
                b.Blob(new Vector3(0f, -0.08f, 0.34f), new Vector3(0.34f, 0.26f, 0.3f), EnemySwatch.GolemStoneDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.1f, 0.3f, 0.1f, EnemySwatch.SoftEye);
                // The mane: flames standing up and streaming back round her head.
                for (int i = 0; i < 9; i++)
                {
                    float a = Mathf.Lerp(-150f, -30f, i / 8f) * Mathf.Deg2Rad;
                    Vector3 root = new Vector3(Mathf.Cos(a) * 0.36f, -Mathf.Sin(a) * 0.3f - 0.04f, -0.16f);
                    b.Horn(root, 0.1f, 0.42f, 3, -16f, i % 2 == 0 ? EnemySwatch.Ember : EnemySwatch.Copper, Quaternion.Euler(-30f, 0f, -Mathf.Cos(a) * 40f));
                }
            });
            // The jaw that eats the lamps, opening on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.24f, 0.26f), b =>
            {
                b.Blob(new Vector3(0f, -0.04f, 0.08f), new Vector3(0.4f, 0.14f, 0.34f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 3; i++)
                {
                    b.Crystal(new Vector3((i - 1f) * 0.12f, 0.02f, 0.2f), 0.035f, 0.12f, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.22f, -0.9f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.25f, -0.45f), 0.09f, 0.07f, 5, EnemySwatch.Black);
                b.Segment(new Vector3(0f, -0.25f, -0.45f), new Vector3(0f, 0.05f, -0.8f), 0.07f, 0.05f, 5, EnemySwatch.Black);
                b.Blob(new Vector3(0f, 0.1f, -0.84f), new Vector3(0.18f, 0.26f, 0.18f), EnemySwatch.Ember, coarse: true);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 1f, 0.36f, 0.55f, -0.72f, 0.16f, EnemySwatch.Black, EnemySwatch.GolemStoneDark, "DevouringLioness");
        }

        // ------------------------------------------------------------------
        // O Peixe de Cauda Negra (87)
        // ------------------------------------------------------------------

        public static void BuildBlackTailFish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // A fish of the black water, reared up out of its pool on its fins: dark green-black, a paler belly, rows of dark scales.
                b.Cylinder(new Vector3(0f, -1.45f, 0f), 0.9f, 0.82f, 0.06f, 12, EnemySwatch.Black);
                b.Blob(Vector3.zero, new Vector3(1f, 2f, 1.1f), EnemySwatch.CaveMoss);
                b.Blob(new Vector3(0f, -0.1f, 0.26f), new Vector3(0.78f, 1.6f, 0.62f), EnemySwatch.Lichen, coarse: true);
                b.Blob(new Vector3(0f, -0.95f, 0f), new Vector3(0.82f, 0.4f, 0.82f), EnemySwatch.BatFurDark, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, -0.55f + i * 0.34f, -0.5f), new Vector3(0.62f, 0.06f, 0.12f), EnemySwatch.Black);
                }

                // The dorsal fin down its back.
                b.Triangle(new Vector3(0f, 0.7f, -0.42f), new Vector3(0f, -0.3f, -0.5f), new Vector3(0f, 0.3f, -0.85f), EnemySwatch.Black);
                b.Triangle(new Vector3(0f, 0.7f, -0.42f), new Vector3(0f, 0.3f, -0.85f), new Vector3(0f, -0.3f, -0.5f), EnemySwatch.Black);
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.05f, 0.14f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.8f, 0.7f, 0.84f), EnemySwatch.BatFurDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.26f, 0.1f, 0.24f, 0.14f, EnemySwatch.SoftEye);
                // The stalk of the lure on its brow: what it will be when the black is gone.
                b.Horn(new Vector3(0f, 0.3f, 0.06f), 0.04f, 0.42f, 3, 22f, EnemySwatch.CaveMoss, Quaternion.Euler(10f, 0f, 0f));
            });
            // The little light at the end of the lure: a lantern fish's, already.
            bp.Mesh(ProceduralEnemyParts.Core, head, new Vector3(0f, 0.62f, 0.4f), b => b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), EnemySwatch.StolenLight, coarse: true));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.24f, 0.24f), b =>
            {
                b.Blob(new Vector3(0f, -0.02f, 0.1f), new Vector3(0.6f, 0.16f, 0.4f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Crystal(new Vector3((i - 1.5f) * 0.12f, 0.04f, 0.26f), 0.03f, 0.1f, EnemySwatch.Fang);
                }
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The pectoral fins it stands up on: they lift on the windup, as it turns its tail.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.48f * side, 0.2f, 0.12f), b =>
                {
                    Vector3 root = Vector3.zero;
                    Vector3 tip = new Vector3(0.42f * side, -0.62f, 0.18f);
                    Vector3 back = new Vector3(0.3f * side, -0.3f, -0.22f);
                    b.Segment(root, tip, 0.08f, 0.04f, 5, EnemySwatch.BatFurDark);
                    b.Triangle(root, tip, back, EnemySwatch.CaveMoss);
                    b.Triangle(root, back, tip, EnemySwatch.CaveMoss);
                }, meshKey: right ? "BlackTailFishFin_R" : "BlackTailFishFin_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.72f, -0.48f), b =>
            {
                // The black tail, one of the five shapes of the Prince: curled behind it out of the water.
                b.Segment(Vector3.zero, new Vector3(0f, -0.25f, -0.5f), 0.26f, 0.18f, 6, EnemySwatch.Black);
                b.Segment(new Vector3(0f, -0.25f, -0.5f), new Vector3(0f, 0.15f, -0.95f), 0.18f, 0.1f, 6, EnemySwatch.Black);
                Vector3 root = new Vector3(0f, 0.15f, -0.95f);
                b.Triangle(root, new Vector3(0f, 0.72f, -1.25f), new Vector3(0f, 0.05f, -1.4f), EnemySwatch.Shadow);
                b.Triangle(root, new Vector3(0f, 0.05f, -1.4f), new Vector3(0f, 0.72f, -1.25f), EnemySwatch.Shadow);
            });
        }

        // ------------------------------------------------------------------
        // O Dragão do Fundo (88)
        // ------------------------------------------------------------------

        public static void BuildDeepDragon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1f, -0.1f), b =>
            {
                // The body of a dragon that the Prince wore in the fifth world and left behind: long, low, black, nobody inside.
                b.Blob(Vector3.zero, new Vector3(1f, 0.85f, 1.8f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, -0.18f, 0.1f), new Vector3(0.8f, 0.5f, 1.4f), EnemySwatch.BatFurDark, coarse: true);
                // The neck rising to the head, and a ridge of dark spines with a few old gold scales.
                b.Segment(new Vector3(0f, 0.2f, 0.7f), new Vector3(0f, 1.25f, 1.15f), 0.3f, 0.2f, 8, EnemySwatch.Black);
                for (int i = 0; i < 5; i++)
                {
                    b.Horn(new Vector3(0f, 0.38f, 0.6f - i * 0.34f), 0.07f, 0.26f - 0.02f * i, 2, -18f, i % 2 == 0 ? EnemySwatch.Shadow : EnemySwatch.Gold, Quaternion.Euler(-20f, 0f, 0f));
                }
            });
            // The little lizard of light that is left when its shadow draws back, curled where the heart would be.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.02f, 0.86f), b => b.Blob(Vector3.zero, new Vector3(0.24f, 0.22f, 0.3f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.4f, 1.2f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.56f, 0.46f, 0.76f), EnemySwatch.Black, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.1f, 0.18f, 0.09f, EnemySwatch.SoftEye);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.14f * s, 0.16f, -0.2f), 0.06f, 0.32f, 3, -18f, EnemySwatch.Bone, Quaternion.Euler(-50f, 0f, -14f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.16f, 0.22f), b =>
            {
                b.Blob(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.4f, 0.12f, 0.46f), EnemySwatch.BatFurDark, coarse: true);
                for (int i = 0; i < 3; i++)
                {
                    b.Crystal(new Vector3((i - 1f) * 0.1f, 0.03f, 0.22f), 0.03f, 0.1f, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.05f, -0.85f), b =>
            {
                // The tail drags and curls aside in the dark.
                b.Segment(Vector3.zero, new Vector3(0.15f, -0.32f, -0.45f), 0.26f, 0.18f, 6, EnemySwatch.Black);
                b.Segment(new Vector3(0.15f, -0.32f, -0.45f), new Vector3(0.45f, -0.58f, -0.62f), 0.18f, 0.08f, 6, EnemySwatch.BatFurDark);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 0.62f, 0.42f, 0.45f, -0.55f, 0.15f, EnemySwatch.Black, EnemySwatch.BatFurDark, "DeepDragon");
        }

        // ------------------------------------------------------------------
        // O Guardião do Novelo (89)
        // ------------------------------------------------------------------

        public static void BuildNoveloGuardian(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.75f, 0f), b =>
            {
                // An old ushabti of the Artificers, tall and wrapped like the little figures in the tombs, set on the bridge to hold the Novelo shut.
                b.Cylinder(new Vector3(0f, -0.72f, 0f), 0.4f, 0.47f, 1.22f, 10, EnemySwatch.Ivory);
                b.Cylinder(new Vector3(0f, 0.36f, 0f), 0.5f, 0.42f, 0.14f, 10, EnemySwatch.Gold);
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0f, -0.62f + i * 0.3f, 0f), 0.43f + i * 0.02f, 0.43f + i * 0.02f, 0.05f, 10, EnemySwatch.Copper);
                }

                // The column of glyphs down his front.
                b.Box(new Vector3(0f, -0.2f, 0.45f), new Vector3(0.14f, 0.8f, 0.03f), EnemySwatch.Rune);
                // The Novelo strapped on his back like the basket of a field ushabti: a dark globe, lilac threads loose round it.
                b.Blob(new Vector3(0f, 0.3f, -0.66f), new Vector3(0.9f, 0.9f, 0.9f), EnemySwatch.Black);
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0f, 0.3f, -0.66f), 0.46f, 0.46f, 0.03f, 10, EnemySwatch.Sorcery, rotation: Quaternion.Euler(30f + i * 50f, i * 40f, 0f));
                }

                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.28f * s, 0.42f, 0.1f), new Vector3(0.3f * s, -0.05f, -0.4f), 0.04f, 0.04f, 4, EnemySwatch.Leather);
                }
            });
            // The light of the Artificers he was made with, in his chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.2f, 0.44f), b => b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.12f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.74f, 0.04f), b =>
            {
                // A calm old face under a striped headcloth, tired eyes, the little plaited beard.
                b.Blob(Vector3.zero, new Vector3(0.46f, 0.5f, 0.46f), EnemySwatch.Ivory, coarse: true);
                b.Blob(new Vector3(0f, 0.06f, -0.06f), new Vector3(0.6f, 0.52f, 0.5f), EnemySwatch.Copper, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.26f * s, -0.2f, 0.02f), new Vector3(0.12f, 0.42f, 0.2f), EnemySwatch.Gold);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.02f, 0.2f, 0.07f, EnemySwatch.SoftEye);
                b.Box(new Vector3(0f, -0.3f, 0.16f), new Vector3(0.08f, 0.18f, 0.06f), EnemySwatch.CopperDark);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.2f), b => b.Blob(Vector3.zero, new Vector3(0.18f, 0.05f, 0.06f), EnemySwatch.IvoryDark, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Wrapped arms with the threads of the Novelo wound round them: the threads lift them on the windup. The right one holds the hoe.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.5f * side, 0.3f, 0.08f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.08f * side, -0.5f, 0.18f), 0.13f, 0.11f, 6, EnemySwatch.Ivory);
                    b.Segment(new Vector3(0.08f * side, -0.5f, 0.18f), new Vector3(0.02f * side, -0.8f, 0.34f), 0.11f, 0.09f, 6, EnemySwatch.IvoryDark);
                    b.Cylinder(new Vector3(0.05f * side, -0.32f, 0.1f), 0.14f, 0.14f, 0.03f, 8, EnemySwatch.Sorcery, rotation: Quaternion.Euler(20f, 0f, 0f));
                    if (side > 0f)
                    {
                        b.Segment(new Vector3(0.02f, -0.3f, 0.36f), new Vector3(0.02f, -1.2f, 0.4f), 0.035f, 0.035f, 5, EnemySwatch.Bark);
                        b.Box(new Vector3(0.02f, -1.22f, 0.5f), new Vector3(0.24f, 0.05f, 0.22f), EnemySwatch.Copper);
                    }
                }, meshKey: right ? "NoveloGuardianArm_R" : "NoveloGuardianArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.03f, 0.2f, 0.16f, EnemySwatch.Ivory, EnemySwatch.IvoryDark, EnemySwatch.CopperDark, "NoveloGuardian");
        }
    }
}
