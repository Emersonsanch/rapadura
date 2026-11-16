using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-third pass of beasts (bible §3.2.1): the roles of Regions 1–4 (Docs/Mapas100.md 03–06, 09, 16, 19, 20) that the continuous
    /// world still played with another beast after <see cref="EarlyStandInBeastRecipes"/>, low-poly from the shared palette. Pivot at the feet
    /// (y = 0), facing +Z. Each keeps its stand-in's numbers (<c>EarlyOwnBeastsTests</c>).
    /// <list type="bullet">
    /// <item><b>Enguia de Lama</b> (03, ~0.9 m reared): a mud eel half out of its puddle, fins of reed. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Lagarta-Casulo</b> (04, ~0.9 m): a fat silk caterpillar with a lit cocoon spun on its back. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Gralha Ladra de Fio</b> (04, ~1.1 m): a jackdaw with a stolen spool of silk, the thread trailing. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Névoa Pegajosa</b> (05, ~1 m): a low cloud of Smoke that drips sticky strands to the ground. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Roseira-Espinho Apagada</b> (06, ~1.2 m): a dimmed rose bush walking on its roots, one rose still lit. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Lobo de Cinzas</b> (06, ~1 m): one of Saklas's wolves, ash-grey with embers in the ruff. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Aranha de Gruta</b> (19, ~0.7 m): a pale cave spider, crystal eyes, eight legs on the tufa. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Pedra-Lâmina</b> (20, ~1.1 m): a cluster of basalt blades round a lit seam. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Lagarto-de-Pedra</b> (16, ~1.5 m long): a granite-grey lizard with lichen on its back. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Rato-do-Karez Gigante</b> (09, ~0.8 m): a giant karez rat, sand fur, ember eyes, long bare tail. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Escorpião-d'Água Apagado</b> (09, ~0.7 m): a flat water scorpion, raptorial forelegs raised, a breathing tube for a tail. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// </list>
    /// </summary>
    public static class EarlyOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Shared
        // ------------------------------------------------------------------

        private static void DrawWalkingBirdLegs(EnemyMeshBuilder b, float bodyY, float spread, EnemySwatch leg, EnemySwatch foot)
        {
            float ground = -bodyY + 0.02f;
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(spread * side, -0.2f, 0f), new Vector3((spread + 0.02f) * side, ground, 0.04f), 0.035f, 0.02f, 5, leg);
                b.Box(new Vector3((spread + 0.02f) * side, ground, 0.1f), new Vector3(0.12f, 0.02f, 0.18f), foot);
            }
        }

        /// <summary>Static jointed legs drawn into the body mesh (spiders, water scorpions): hip → knee up and out → tip on the ground.</summary>
        private static void DrawSpreadLegs(EnemyMeshBuilder b, float bodyY, float[] hipZ, float[] splay, float reach, float kneeUp, float radius, EnemySwatch leg, EnemySwatch tip)
        {
            float ground = -bodyY + 0.02f;
            for (int i = 0; i < hipZ.Length; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float side = s;
                    Quaternion yaw = Quaternion.Euler(0f, splay[i] * side, 0f);
                    Vector3 hip = new Vector3(0.12f * side, -0.02f, hipZ[i]);
                    Vector3 knee = hip + yaw * new Vector3(reach * 0.55f * side, kneeUp, 0f);
                    Vector3 foot = hip + yaw * new Vector3(reach * side, 0f, 0f);
                    foot.y = ground;
                    b.Segment(hip, knee, radius, radius * 0.8f, 4, leg);
                    b.Segment(knee, foot, radius * 0.8f, radius * 0.4f, 4, tip);
                }
            }
        }

        // ------------------------------------------------------------------
        // Enguia de Lama
        // ------------------------------------------------------------------

        public static void BuildMudEel(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.34f, 0f), DrawMudEelBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.36f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.22f, 0.34f), EnemySwatch.BarkDark);
                b.Blob(new Vector3(0f, -0.05f, 0.04f), new Vector3(0.2f, 0.12f, 0.26f), EnemySwatch.Lichen, coarse: true);
                // The barbels of the mud, and the eyes that kept a little of the spring it lives under.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.08f * s, -0.04f, 0.14f), new Vector3(0.16f * s, -0.12f, 0.2f), 0.012f, 0.004f, 3, EnemySwatch.Bark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.09f, 0.05f, EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.16f, 0.04f, 0.18f), EnemySwatch.CaveMoss);
                b.Cylinder(new Vector3(0.04f, 0.01f, 0.14f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.01f, 0.14f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.2f, -0.26f), b =>
            {
                // The rest of it, still in the mud: two loops that break the surface and a fin of reed.
                b.Segment(Vector3.zero, new Vector3(0.12f, -0.04f, -0.3f), 0.1f, 0.08f, 6, EnemySwatch.BarkDark);
                b.Segment(new Vector3(0.12f, -0.04f, -0.3f), new Vector3(-0.08f, -0.06f, -0.56f), 0.08f, 0.05f, 6, EnemySwatch.BarkDark);
                b.Box(new Vector3(-0.08f, 0.04f, -0.56f), new Vector3(0.02f, 0.14f, 0.18f), EnemySwatch.Lichen);
            });
        }

        private static void DrawMudEelBody(EnemyMeshBuilder b)
        {
            // The Mud of the Darkness lies under the mire like a second floor. The eel came up out of it and waits in the puddles off the boardwalk,
            // only its head and one loop above the water, for whoever steps off the logs.
            b.Cylinder(new Vector3(0f, -0.33f, 0f), 0.46f, 0.4f, 0.04f, 8, EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.26f, 0f), new Vector3(0.7f, 0.12f, 0.7f), EnemySwatch.CaveMoss, coarse: true);
            b.Segment(new Vector3(0f, -0.26f, -0.1f), new Vector3(0f, 0.06f, 0.14f), 0.13f, 0.11f, 7, EnemySwatch.BarkDark);
            b.Segment(new Vector3(0f, 0.06f, 0.14f), new Vector3(0f, 0.36f, 0.3f), 0.11f, 0.1f, 7, EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.08f, 0.2f), new Vector3(0.14f, 0.3f, 0.12f), EnemySwatch.Lichen, coarse: true);
            // Bubbles where the rest of it breathes under the mud, one with a speck of the swallowed light.
            b.Blob(new Vector3(0.26f, -0.24f, 0.18f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(-0.24f, -0.24f, -0.2f), new Vector3(0.07f, 0.05f, 0.07f), EnemySwatch.StolenLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Lagarta-Casulo
        // ------------------------------------------------------------------

        public static void BuildCocoonCaterpillar(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawCocoonCaterpillarBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.34f, -0.08f), b =>
            {
                // The cocoon it spun on its own back round a bit of stolen light, so nobody can take it back without unwinding it.
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.26f, 0.42f), EnemySwatch.Ivory);
                b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.16f, 0.14f, 0.24f), EnemySwatch.StolenLight, coarse: true);
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0f, 0f, -0.12f + i * 0.12f), new Vector3(0.32f, 0.02f, 0.02f), EnemySwatch.IvoryDark);
                }
            });
        }

        private static void DrawCocoonCaterpillarBody(EnemyMeshBuilder b)
        {
            // The silkworms of the grove were left alone when the looms stopped. One ate on and on in the dark and grew this big; it will not turn
            // into a moth, it only spins.
            for (int i = 0; i < 5; i++)
            {
                float z = 0.42f - i * 0.22f;
                float r = i == 0 ? 0.42f : 0.5f - i * 0.03f;
                b.Blob(new Vector3(0f, 0f, z), new Vector3(r + 0.08f, r, 0.3f), i % 2 == 0 ? EnemySwatch.OwlBuff : EnemySwatch.Ivory);
                // Little prolegs under each ring, the lowest point of the model.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.14f * s, -0.27f, z), new Vector3(0.08f, 0.05f, 0.08f), EnemySwatch.IvoryDark);
                }
            }

            // The head: dark mandibles and two lit eyes.
            b.Blob(new Vector3(0f, 0.02f, 0.58f), new Vector3(0.3f, 0.28f, 0.2f), EnemySwatch.BarkDark, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.06f, 0.66f, 0.05f, EnemySwatch.SoftEye);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.05f * s, -0.08f, 0.66f), 0.025f, 0.005f, 0.08f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(70f, 0f, 0f));
            }

            // Loose silk off its sides, where it brushed the mulberry rows.
            b.Segment(new Vector3(0.22f, 0.1f, 0.1f), new Vector3(0.34f, -0.26f, 0.02f), 0.01f, 0.006f, 3, EnemySwatch.Ivory);
            b.Segment(new Vector3(-0.22f, 0.08f, -0.2f), new Vector3(-0.32f, -0.26f, -0.28f), 0.01f, 0.006f, 3, EnemySwatch.Ivory);
        }

        // ------------------------------------------------------------------
        // Gralha Ladra de Fio
        // ------------------------------------------------------------------

        public static void BuildThreadJackdaw(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawThreadJackdawBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.3f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.26f), EnemySwatch.Black);
                // The grey nape of a jackdaw, and its pale thief's eye.
                b.Blob(new Vector3(0f, 0.03f, -0.07f), new Vector3(0.22f, 0.2f, 0.16f), EnemySwatch.GolemStoneDark, coarse: true);
                b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.035f, 0.01f, 0.11f, 5, EnemySwatch.IronDark, rotation: Quaternion.Euler(84f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.07f, 0.05f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.05f, 0.02f, 0.11f), EnemySwatch.IronDark);
                // The end of the stolen thread, still in its beak.
                b.Segment(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.06f, -0.2f, 0.04f), 0.008f, 0.006f, 3, EnemySwatch.Ivory);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.19f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Black, EnemySwatch.IronDark, EnemySwatch.GolemStoneDark, 0.8f), Quaternion.Euler(0f, 0f, -14f), meshKey: "ThreadJackdawWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.19f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Black, EnemySwatch.IronDark, EnemySwatch.GolemStoneDark, 0.8f), Quaternion.Euler(0f, 0f, 14f), meshKey: "ThreadJackdawWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.32f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.IronDark), Quaternion.Euler(12f, 0f, 0f));
        }

        private static void DrawThreadJackdawBody(EnemyMeshBuilder b)
        {
            // Jackdaws take what shines. In the grove what shines is the silk on the looms, and this one has been unwinding the Casa do Tear
            // one spool at a time; the spool is tied under its breast and the thread runs out behind it.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.48f, 0.7f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.1f, 0.12f), new Vector3(0.36f, 0.3f, 0.38f), EnemySwatch.IronDark, coarse: true);
            b.Cylinder(new Vector3(0f, -0.3f, 0.02f), 0.08f, 0.08f, 0.14f, 6, EnemySwatch.Bark, rotation: Quaternion.Euler(0f, 0f, 90f));
            b.Cylinder(new Vector3(0f, -0.3f, 0.02f), 0.065f, 0.065f, 0.1f, 6, EnemySwatch.Ivory, rotation: Quaternion.Euler(0f, 0f, 90f));
            b.Segment(new Vector3(0f, -0.3f, -0.04f), new Vector3(0.1f, -0.42f, -0.5f), 0.008f, 0.006f, 3, EnemySwatch.StolenLight);
            DrawWalkingBirdLegs(b, 0.56f, 0.09f, EnemySwatch.Black, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Névoa Pegajosa
        // ------------------------------------------------------------------

        public static void BuildStickyMist(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawStickyMistBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.06f), b =>
            {
                // The drop of the laurels' water it drank, lit inside the cloud.
                b.Blob(Vector3.zero, new Vector3(0.22f, 0.2f, 0.22f), EnemySwatch.StolenLight, coarse: true);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.3f, 0.28f, 0.3f), EnemySwatch.Shroud, coarse: true);
            });
        }

        private static void DrawStickyMistBody(EnemyMeshBuilder b)
        {
            // Smoke, the first element of the Darkness, came up through the laurels mixed with the real mist. Where it touches it sticks, and it
            // hangs low to touch as many legs as it can.
            b.Blob(Vector3.zero, new Vector3(0.9f, 0.42f, 0.8f), EnemySwatch.Shroud);
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.32f, 0.08f + (i % 2) * 0.08f, Mathf.Sin(a) * 0.28f), new Vector3(0.34f, 0.28f, 0.32f),
                    i % 2 == 0 ? EnemySwatch.GolemSnow : EnemySwatch.Shadow, coarse: true);
            }

            // The sticky strands it lets down to the forest floor; the lowest point of the model.
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 20f) * Mathf.Deg2Rad;
                Vector3 top = new Vector3(Mathf.Cos(a) * 0.26f, -0.14f, Mathf.Sin(a) * 0.22f);
                b.Segment(top, new Vector3(top.x * 1.2f, -0.6f, top.z * 1.2f), 0.03f, 0.012f, 4, EnemySwatch.GolemSnow);
                b.Blob(new Vector3(top.x * 1.2f, -0.6f, top.z * 1.2f), new Vector3(0.05f, 0.03f, 0.05f), EnemySwatch.Shroud, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Roseira-Espinho Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedThornRose(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawDimmedThornRoseBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.6f, 0.08f), b =>
            {
                // The one rose that kept its light, at the top of the bush, petals round a lit heart.
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.StolenLight, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    float a = i * 72f;
                    b.Blob(Quaternion.Euler(0f, a, 0f) * new Vector3(0.08f, 0f, 0f), new Vector3(0.1f, 0.08f, 0.06f), EnemySwatch.Pink, Quaternion.Euler(0f, a, 20f), coarse: true);
                }
            });
        }

        private static void DrawDimmedThornRoseBody(EnemyMeshBuilder b)
        {
            // Mihrshah's roses grew in the four beds of the garden. When the canals went dry one bush pulled its roots out of the bed to go looking
            // for water, and the thorns came with it.
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(0.72f, 0.66f, 0.62f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.22f, 0f), new Vector3(0.62f, 0.46f, 0.56f), EnemySwatch.CaveMoss, coarse: true);
            // Stems with thorns, and grey roses gone out.
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f * Mathf.Deg2Rad;
                Vector3 from = new Vector3(Mathf.Cos(a) * 0.12f, 0.1f, Mathf.Sin(a) * 0.12f);
                Vector3 to = new Vector3(Mathf.Cos(a) * 0.38f, 0.42f + (i % 2) * 0.1f, Mathf.Sin(a) * 0.32f);
                b.Segment(from, to, 0.03f, 0.018f, 4, EnemySwatch.Bark);
                b.Cylinder((from + to) * 0.5f, 0.02f, 0f, 0.07f, 3, EnemySwatch.IvoryDark, rotation: Quaternion.Euler(0f, 0f, i % 2 == 0 ? 70f : -70f));
                b.Blob(to, new Vector3(0.12f, 0.1f, 0.12f), i % 3 == 0 ? EnemySwatch.Pink : EnemySwatch.Shroud, coarse: true);
            }

            // The roots it walks on, splayed on the ground (the lowest point of the model).
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                b.Segment(new Vector3(0f, -0.2f, 0f), new Vector3(Mathf.Cos(a) * 0.34f, -0.48f, Mathf.Sin(a) * 0.3f), 0.06f, 0.025f, 5, EnemySwatch.BarkDark);
            }
        }

        // ------------------------------------------------------------------
        // Lobo de Cinzas
        // ------------------------------------------------------------------

        public static void BuildAshWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawAshWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.54f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.32f, 0.3f, 0.36f), EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, -0.04f, 0.18f), new Vector3(0.16f, 0.14f, 0.22f), EnemySwatch.GolemStoneDark, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.09f * s, 0.12f, -0.06f), 0.05f, 0.005f, 0.16f, 4, EnemySwatch.Shadow);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.14f, 0.05f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.1f), new Vector3(0.14f, 0.04f, 0.2f), EnemySwatch.GolemStoneDark);
                b.Cylinder(new Vector3(0.04f, 0.005f, 0.18f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.005f, 0.18f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.48f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.12f, -0.34f), 0.08f, 0.05f, 5, EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, -0.14f, -0.38f), new Vector3(0.08f, 0.08f, 0.1f), EnemySwatch.Ember, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.5f, 0.22f, 0.34f, -0.34f, 0.07f, EnemySwatch.Shroud, EnemySwatch.Shadow, "AshWolf");
        }

        private static void DrawAshWolfBody(EnemyMeshBuilder b)
        {
            // Saklas runs his garden with a pack of wolves made of what the fire left. They are grey all through, and the embers in the ruff still
            // burn; where they lie down the grass goes grey too.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.52f, 1.02f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, -0.12f, 0.04f), new Vector3(0.46f, 0.26f, 0.76f), EnemySwatch.GolemStoneDark, coarse: true);
            // The ruff, ragged like burnt paper, embers caught in it.
            b.Blob(new Vector3(0f, 0.08f, 0.34f), new Vector3(0.56f, 0.5f, 0.34f), EnemySwatch.Shadow, coarse: true);
            b.Blob(new Vector3(0.16f, 0.2f, 0.36f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.14f, 0.24f, 0.3f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(0.02f, 0.28f, 0.22f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Aranha de Gruta
        // ------------------------------------------------------------------

        private static readonly float[] GrottoSpiderHipZ = { 0.1f, 0.03f, -0.04f, -0.11f };
        private static readonly float[] GrottoSpiderSplay = { -40f, -12f, 14f, 40f };

        public static void BuildGrottoSpider(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawGrottoSpiderBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.2f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.18f, 0.2f), EnemySwatch.CaveSkin);
                // Eight eyes of cave crystal, the two big ones in front.
                HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.05f, 0.09f, 0.045f, EnemySwatch.Crystal);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.07f, 0.06f, 0.025f, EnemySwatch.Crystal);
                HinokuraBeastRecipes.AddEyes(b, 0.03f, 0.09f, 0.07f, 0.02f, EnemySwatch.Crystal);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.09f, 0.03f, 0.02f, EnemySwatch.Crystal);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.08f), b =>
            {
                // The fangs (chelicerae), pale with dark tips.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.04f * s, 0f, 0f), new Vector3(0.035f * s, -0.07f, 0.06f), 0.022f, 0.012f, 4, EnemySwatch.CaveBelly);
                    b.Cylinder(new Vector3(0.035f * s, -0.07f, 0.06f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Black, rotation: Quaternion.Euler(160f, 0f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.18f), b =>
            {
                // The abdomen, swollen, with the pale chevrons of the cave and a glow where it keeps the light it bit.
                b.Blob(new Vector3(0f, 0.06f, -0.2f), new Vector3(0.46f, 0.44f, 0.52f), EnemySwatch.CaveSkin);
                b.Blob(new Vector3(0f, 0.2f, -0.2f), new Vector3(0.28f, 0.16f, 0.38f), EnemySwatch.CaveBelly, coarse: true);
                b.Blob(new Vector3(0f, 0.25f, -0.28f), new Vector3(0.08f, 0.06f, 0.1f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawGrottoSpiderBody(EnemyMeshBuilder b)
        {
            // The spiders of the Limiar never saw the sun. They hang their webs by the lake where the lamplight reaches, because that is where the
            // moths come, and lately the cousins too.
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.22f, 0.34f), EnemySwatch.CaveSkinDark);
            DrawSpreadLegs(b, 0.3f, GrottoSpiderHipZ, GrottoSpiderSplay, 0.46f, 0.22f, 0.025f, EnemySwatch.CaveSkinDark, EnemySwatch.CaveSkin);
        }

        // ------------------------------------------------------------------
        // Pedra-Lâmina
        // ------------------------------------------------------------------

        public static void BuildBladeStone(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.36f, 0f), DrawBladeStoneBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.16f, 0.1f), b =>
            {
                // The seam of light it closed itself round, showing between the blades.
                b.Box(Vector3.zero, new Vector3(0.08f, 0.36f, 0.06f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 0f, 8f));
                b.Blob(new Vector3(0f, 0.04f, 0.02f), new Vector3(0.12f, 0.12f, 0.08f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawBladeStoneBody(EnemyMeshBuilder b)
        {
            // Tages's hall grew its columns drop by drop for ten thousand years. When the hall started to sigh, a few of the thin ones split off
            // sharp, and now they shuffle about on their base and lean into whoever passes. Lit lamps make them slow.
            b.Rock(new Vector3(0f, -0.2f, 0f), new Vector3(0.84f, 0.34f, 0.7f), EnemySwatch.GolemStoneDark, 1);
            float[] tilt = { -26f, -10f, 6f, 20f, 32f };
            for (int i = 0; i < tilt.Length; i++)
            {
                float x = -0.26f + i * 0.13f;
                float z = (i % 2 == 0 ? 0.06f : -0.08f);
                b.Crystal(new Vector3(x, -0.08f, z), 0.09f - (i % 2) * 0.02f, 0.7f - Mathf.Abs(i - 2) * 0.12f, i % 2 == 0 ? EnemySwatch.Black : EnemySwatch.IronDark,
                    Quaternion.Euler(i % 2 == 0 ? 8f : -8f, 0f, -tilt[i]), sides: 4);
            }

            // Two flakes of indigo crystal grown on the base, and a dim one.
            b.Crystal(new Vector3(0.3f, -0.12f, 0.2f), 0.05f, 0.18f, EnemySwatch.Crystal, Quaternion.Euler(20f, 0f, -30f));
            b.Crystal(new Vector3(-0.28f, -0.12f, -0.18f), 0.04f, 0.14f, EnemySwatch.Crystal, Quaternion.Euler(-20f, 0f, 30f));
        }

        // ------------------------------------------------------------------
        // Lagarto-de-Pedra
        // ------------------------------------------------------------------

        public static void BuildStoneLizard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawStoneLizardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.06f, 0.52f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.34f, 0.24f, 0.4f), EnemySwatch.GolemStone);
                b.Blob(new Vector3(0f, 0.08f, -0.06f), new Vector3(0.26f, 0.12f, 0.24f), EnemySwatch.GolemStoneDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.06f, 0.1f, 0.055f, EnemySwatch.WarEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.22f, 0.04f, 0.22f), EnemySwatch.GolemSnow));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.46f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.34f), 0.12f, 0.07f, 6, EnemySwatch.GolemStone);
                b.Segment(new Vector3(0f, -0.06f, -0.34f), new Vector3(0f, -0.14f, -0.66f), 0.07f, 0.02f, 5, EnemySwatch.GolemStoneDark);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.26f, 0.27f, 0.3f, -0.3f, 0.07f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, "StoneLizard");
        }

        private static void DrawStoneLizardBody(EnemyMeshBuilder b)
        {
            // The real lizards of the walls are the colour of the granite so the hawks miss them. This one kept still so long the lichen grew on
            // its back; it sleeps in the warm courses of the drystone and runs out fast at footsteps.
            b.Blob(Vector3.zero, new Vector3(0.74f, 0.42f, 0.96f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.54f, 0.22f, 0.74f), EnemySwatch.GolemSnow, coarse: true);
            // Plates down the spine in the herringbone of the walls, and lichen on them.
            for (int i = 0; i < 5; i++)
            {
                b.Box(new Vector3(0f, 0.22f, 0.36f - i * 0.18f), new Vector3(0.16f, 0.06f, 0.1f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, i % 2 == 0 ? 30f : -30f, 0f));
            }

            b.Blob(new Vector3(0.16f, 0.18f, -0.06f), new Vector3(0.14f, 0.05f, 0.16f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(-0.14f, 0.18f, 0.16f), new Vector3(0.12f, 0.05f, 0.12f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(0f, 0.3f, 0.06f), new Vector3(0.06f, 0.05f, 0.06f), EnemySwatch.GolemCrack, coarse: true);
        }

        // ------------------------------------------------------------------
        // Rato-do-Karez Gigante
        // ------------------------------------------------------------------

        public static void BuildKarezRat(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawKarezRatBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.04f, 0.46f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.26f, 0.34f), EnemySwatch.Sandstone);
                b.Blob(new Vector3(0f, -0.02f, 0.18f), new Vector3(0.14f, 0.12f, 0.16f), EnemySwatch.SandstoneDark, coarse: true);
                b.Blob(new Vector3(0f, 0f, 0.26f), new Vector3(0.05f, 0.04f, 0.04f), EnemySwatch.Pink, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    // Round ears, and whiskers.
                    b.Blob(new Vector3(0.12f * s, 0.14f, -0.04f), new Vector3(0.12f, 0.12f, 0.04f), EnemySwatch.Pink, coarse: true);
                    b.Segment(new Vector3(0.05f * s, -0.01f, 0.22f), new Vector3(0.18f * s, 0.01f, 0.2f), 0.004f, 0.002f, 3, EnemySwatch.SandstoneLight);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.12f, 0.045f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.14f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.1f, 0.03f, 0.12f), EnemySwatch.SandstoneDark);
                // The two long front teeth that gnawed the karez walls.
                b.Box(new Vector3(0f, 0.03f, 0.11f), new Vector3(0.04f, 0.06f, 0.015f), EnemySwatch.Ivory);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.06f, -0.44f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.06f, -0.14f, -0.36f), 0.04f, 0.03f, 5, EnemySwatch.Pink);
                b.Segment(new Vector3(0.06f, -0.14f, -0.36f), new Vector3(-0.04f, -0.36f, -0.7f), 0.03f, 0.01f, 4, EnemySwatch.Pink);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.34f, 0.2f, 0.26f, -0.26f, 0.06f, EnemySwatch.Sandstone, EnemySwatch.Pink, "KarezRat");
        }

        private static void DrawKarezRatBody(EnemyMeshBuilder b)
        {
            // The karez carry the water under the sand for miles. The rats followed the water down, and with no sun they grew; now they swarm out of
            // the shafts in the dark sections, where the sunbeams don't reach.
            b.Blob(Vector3.zero, new Vector3(0.58f, 0.5f, 0.92f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, -0.12f, 0.04f), new Vector3(0.44f, 0.24f, 0.7f), EnemySwatch.SandstoneLight, coarse: true);
            b.Blob(new Vector3(0f, 0.14f, -0.1f), new Vector3(0.44f, 0.24f, 0.6f), EnemySwatch.SandstoneDark, coarse: true);
            // Wet mud of the channel on its flank, a fleck of light it licked off a sunbeam.
            b.Blob(new Vector3(0.2f, -0.04f, -0.16f), new Vector3(0.16f, 0.14f, 0.24f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(-0.18f, 0.16f, 0.1f), new Vector3(0.05f, 0.04f, 0.05f), EnemySwatch.StolenLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Escorpião-d'Água Apagado
        // ------------------------------------------------------------------

        private static readonly float[] WaterScorpionHipZ = { 0.02f, -0.12f };
        private static readonly float[] WaterScorpionSplay = { 10f, 40f };

        public static void BuildWaterScorpion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.26f, 0f), DrawWaterScorpionBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.3f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.12f, 0.16f), EnemySwatch.BarkDark);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.03f, 0.04f, 0.045f, EnemySwatch.SoftEye);
                // The raptorial forelegs, folded like a jackknife and held up to strike.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.08f * s, -0.02f, 0.02f), new Vector3(0.2f * s, 0.24f, 0.12f), 0.03f, 0.025f, 4, EnemySwatch.Bark);
                    b.Segment(new Vector3(0.2f * s, 0.24f, 0.12f), new Vector3(0.16f * s, 0.06f, 0.24f), 0.025f, 0.012f, 4, EnemySwatch.BarkDark);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.08f), b =>
                b.Cylinder(Vector3.zero, 0.02f, 0.005f, 0.1f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(110f, 0f, 0f)));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.34f), b =>
            {
                // The breathing tube it pokes up through the water, two thin halves; the tip lit where it sips the air.
                b.Segment(Vector3.zero, new Vector3(0f, 0.18f, -0.26f), 0.018f, 0.012f, 4, EnemySwatch.Bark);
                b.Segment(new Vector3(0f, 0.18f, -0.26f), new Vector3(0f, 0.46f, -0.36f), 0.012f, 0.008f, 4, EnemySwatch.Bark);
                b.Blob(new Vector3(0f, 0.47f, -0.36f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawWaterScorpionBody(EnemyMeshBuilder b)
        {
            // In the karez the water runs clear and cold, and the water scorpion waits under the leaves at the edge with its forelegs folded. The
            // Darkness dimmed the channel and it grew as big as a dog; it still waits, it still strikes.
            b.Blob(Vector3.zero, new Vector3(0.44f, 0.16f, 0.66f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, 0.05f, -0.04f), new Vector3(0.34f, 0.08f, 0.52f), EnemySwatch.BarkDark, coarse: true);
            b.Box(new Vector3(0f, 0.08f, 0.08f), new Vector3(0.02f, 0.02f, 0.4f), EnemySwatch.Lichen);
            DrawSpreadLegs(b, 0.26f, WaterScorpionHipZ, WaterScorpionSplay, 0.4f, 0.16f, 0.022f, EnemySwatch.BarkDark, EnemySwatch.Bark);
        }
    }
}
