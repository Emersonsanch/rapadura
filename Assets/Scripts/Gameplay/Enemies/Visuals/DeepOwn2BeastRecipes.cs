using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The second half of the twenty-second pass of beasts (bible §3.2.1): the last roles of the Limiar Profundo (Docs/Mapas100.md 83, 86,
    /// 88, 89) that the continuous world still played with another region's beast after <see cref="DeepOwnBeastRecipes"/>, low-poly from the
    /// shared palette. Kept in a file of their own so each pass stays at most twelve beasts. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Ushabti Apagado da Muralha</b> (83, ~1.4 m): a basalt answering figure of the Arched Rampart with a cold lamp on its head and
    /// a snuffer for a staff. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// <item><b>Fumaça do Portão</b> (83, ~1.3 m): the smoke of the five pits seeping under the sealed gates, a lamp flame it swallowed
    /// smouldering inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Redemoinho</b> (86, ~1.6 m): a funnel of the Ruinous Wind, grit spinning round it and a stolen spark in its eye.
    /// <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Rastejante</b> (88, 89, ~0.7 m): a low six-legged crawler of the Pit of Darkness, spines down its back like the dragon
    /// body it crawled out of. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; six legs.</item>
    /// </list>
    /// </summary>
    public static class DeepOwn2BeastRecipes
    {
        // ------------------------------------------------------------------
        // Ushabti Apagado da Muralha
        // ------------------------------------------------------------------

        public static void BuildRampartUshabti(ProceduralEnemyBlueprint bp)
        {
            // The guardians of the lamps set answering figures along the Arched Rampart to keep the lamps lit at night. The shadow that
            // climbs from the abyss called them, and now they march the wall putting the lamps out one by one.
            DeepOwnBeastRecipes.BuildFigure(bp, "RampartUshabti", 1f, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark, b =>
            {
                b.Box(new Vector3(0f, -0.14f, 0.25f), new Vector3(0.1f, 0.62f, 0.03f), EnemySwatch.Black);
                for (int i = 0; i < 4; i++)
                {
                    // The glyphs of the lamp-keepers; only the top one still glows.
                    b.Box(new Vector3(0f, 0.08f - i * 0.15f, 0.27f), new Vector3(0.06f, 0.04f, 0.02f), i == 0 ? EnemySwatch.Rune : EnemySwatch.Shadow);
                }

                // The copper belt of the fortress and the soot it rubbed off the lamps.
                b.Cylinder(new Vector3(0f, -0.02f, 0f), 0.31f, 0.31f, 0.06f, 8, EnemySwatch.Copper);
                b.Blob(new Vector3(0.12f, 0.2f, 0.17f), new Vector3(0.22f, 0.2f, 0.12f), EnemySwatch.Shadow, coarse: true);
            }, b =>
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0f, 0.12f - i * 0.08f, -0.2f), new Vector3(0.36f, 0.02f, 0.04f), EnemySwatch.Shadow);
                }

                // The lamp it carries on its head, gone cold, with a curl of the smoke of its own snuffing.
                b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.1f, 0.15f, 0.08f, 8, EnemySwatch.CopperDark, bottomCap: true);
                b.Blob(new Vector3(0f, 0.29f, 0.02f), new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.Shadow, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.02f, 0.13f, 0.045f, EnemySwatch.SoftEye);
            }, DrawSnuffer);
        }

        /// <summary>The snuffer the lamp-keepers gave the figures to trim the wicks; now it only puts them out.</summary>
        private static void DrawSnuffer(EnemyMeshBuilder b, float s)
        {
            b.Segment(new Vector3(0.02f * s, -0.62f * s, 0.3f * s), new Vector3(0.02f * s, -0.08f * s, 0.34f * s), 0.022f * s, 0.022f * s, 5, EnemySwatch.Copper);
            b.Cylinder(new Vector3(0.02f * s, -0.74f * s, 0.29f * s), 0.09f * s, 0.03f * s, 0.12f * s, 7, EnemySwatch.CopperDark, bottomCap: true);
        }

        // ------------------------------------------------------------------
        // Fumaça do Portão
        // ------------------------------------------------------------------

        public static void BuildGateSmoke(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawGateSmokeBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.08f, 0.36f), b =>
            {
                // A wick it pulled out of a niche of the fortress, still smouldering where the smoke holds it.
                b.Segment(new Vector3(0f, -0.06f, 0f), new Vector3(0f, 0.04f, 0f), 0.02f, 0.015f, 4, EnemySwatch.Bark);
                b.Blob(new Vector3(0f, 0.07f, 0f), new Vector3(0.14f, 0.16f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.08f, 0.03f), new Vector3(0.06f, 0.07f, 0.04f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawGateSmokeBody(EnemyMeshBuilder b)
        {
            // The five pits are sealed from the other side, but smoke goes where doors do not: it seeps under the five gates of the fortress
            // and creeps along the niches, wide and low, looking for a lamp to sit on.
            for (int i = 0; i < 4; i++)
            {
                float y = -0.5f + i * 0.3f;
                float w = 0.98f - i * 0.16f;
                b.Blob(new Vector3(i % 2 == 0 ? -0.04f : 0.05f, y, 0f), new Vector3(w, 0.34f, w * 0.88f), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.GolemStoneDark,
                    coarse: i == 3);
            }

            // The curl that rises off its top, and the tongues it pushes ahead along the floor.
            b.Segment(new Vector3(0f, 0.44f, 0f), new Vector3(0.14f, 0.7f, -0.1f), 0.12f, 0.03f, 5, EnemySwatch.Shroud);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.36f * s, -0.56f, 0.3f), new Vector3(0.3f, 0.12f, 0.34f), EnemySwatch.Shadow, coarse: true);
            }

            HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.14f, 0.27f, 0.06f, EnemySwatch.SoftEye);
        }

        // ------------------------------------------------------------------
        // Redemoinho
        // ------------------------------------------------------------------

        public static void BuildRuinWhirlwind(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.75f, 0f), DrawRuinWhirlwindBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.62f, 0f), b =>
            {
                // A spark it tore off the Lamp 3 of the shaft, spinning in the eye of the wind where it cannot fall.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.MoonEye, coarse: true);
            });
        }

        private static void DrawRuinWhirlwindBody(EnemyMeshBuilder b)
        {
            // The Ruinous Wind is the third of the five elements of the Darkness, the one of the things that fly. In its pit it turns
            // round and round the shaft, and where it turns tight enough it holds a shape: a funnel, narrow at the floor and wide on top.
            for (int i = 0; i < 6; i++)
            {
                float y = -0.72f + i * 0.25f;
                b.Cylinder(new Vector3(0f, y, 0f), 0.12f + i * 0.08f, 0.17f + i * 0.08f, 0.24f, 8, i % 2 == 0 ? EnemySwatch.Iron : EnemySwatch.Steel,
                    twist: 20f + i * 6f);
            }

            // The grit it carries round: chips of the shaft's stone, at different heights.
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f * Mathf.Deg2Rad;
                float radius = 0.22f + i * 0.07f;
                b.Box(new Vector3(Mathf.Cos(angle) * radius, -0.5f + i * 0.26f, Mathf.Sin(angle) * radius), new Vector3(0.08f, 0.06f, 0.07f),
                    EnemySwatch.GolemStoneDark, Quaternion.Euler(i * 30f, i * 50f, 0f));
            }

            // The streaks of the wind across its front.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0f, -0.3f + i * 0.34f, 0.22f + i * 0.08f), new Vector3(0.3f + i * 0.1f, 0.03f, 0.03f), EnemySwatch.GolemSnow,
                    Quaternion.Euler(0f, 0f, 12f));
            }
        }

        // ------------------------------------------------------------------
        // Rastejante
        // ------------------------------------------------------------------

        private const float CrawlerHip = 0.28f;
        private static readonly float[] CrawlerHipZ = { 0.2f, 0f, -0.2f };
        private static readonly float[] CrawlerSplay = { 30f, 0f, -30f };

        public static void BuildDarkCrawler(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, CrawlerHip, 0f), DrawDarkCrawlerBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.06f, 0.44f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.16f, 0.28f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, 0.03f, 0.14f), new Vector3(0.18f, 0.06f, 0.14f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.05f, 0.1f, 0.04f, EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.06f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.2f, 0.04f, 0.2f), EnemySwatch.BatFurDark);
                b.Cylinder(new Vector3(0.06f, 0.01f, 0.18f), 0.014f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.06f, 0.01f, 0.18f), 0.014f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.34f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.04f, -0.06f, -0.3f), 0.1f, 0.05f, 6, EnemySwatch.Shadow);
                b.Segment(new Vector3(0.04f, -0.06f, -0.3f), new Vector3(-0.04f, -0.12f, -0.52f), 0.05f, 0.008f, 5, EnemySwatch.Black);
            });
            DeepOwnBeastRecipes.AddSplayedLegs(bp, CrawlerHip, 0.15f, CrawlerHipZ, CrawlerSplay, new Vector3(0.2f, 0.12f, 0f), 0.18f,
                EnemySwatch.Shadow, EnemySwatch.Black, "DarkCrawler");
        }

        private static void DrawDarkCrawlerBody(EnemyMeshBuilder b)
        {
            // The Darkness is the fifth of the elements, the one of the things that crawl, and the Prince took his dragon's body from it.
            // These are what fell off that body: small, low and many, and they come at the warmth of a lamp all together.
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(0f, 0.02f - i * 0.01f, 0.24f - i * 0.24f), new Vector3(0.36f - i * 0.03f, 0.26f, 0.32f), i == 1 ? EnemySwatch.Black : EnemySwatch.Shadow);
            }

            // The spines down its back, and a seam of the dark light it crawls with between them.
            for (int i = 0; i < 4; i++)
            {
                float z = 0.3f - i * 0.18f;
                b.Cylinder(new Vector3(0f, 0.1f, z), 0.05f, 0f, 0.3f - i * 0.04f, 4, EnemySwatch.Black);
            }

            b.Box(new Vector3(0f, 0.13f, 0.03f), new Vector3(0.03f, 0.02f, 0.5f), EnemySwatch.Sorcery);
        }
    }
}
