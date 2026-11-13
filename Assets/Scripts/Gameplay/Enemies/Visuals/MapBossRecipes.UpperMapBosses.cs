using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Six more map bosses of Regions 10–16 (Docs/Mapas100.md 57, 63, 67, 74, 86, 97), built like the rest of <see cref="MapBossRecipes"/>: pivot at the
    /// feet, facing +Z, the glowing <c>Core</c> the rig feeds with the fight's phase, and soft eyes, because each is a creature gone dark, not a villain.
    /// <list type="bullet">
    /// <item><b>O Rei dos Ays</b> (57, ~2.9 m): a funnel of dust and wind, stones caught circling in it, a crown of gusts; no legs.</item>
    /// <item><b>A Aurora Presa</b> (63, ~3 m): a jagged iceberg, the green and violet light of the aurora caught inside it; no legs.</item>
    /// <item><b>Guayota</b> (67, ~2.75 m): a hunched giant of black lava rock, glowing cracks, the little stolen sun held in his chest; two legs.</item>
    /// <item><b>O Nó Errado</b> (74, ~2.8 m): a dark coil of Mehen with gold bands, reared up out of a knot, two loops of itself for arms; no legs.</item>
    /// <item><b>A Águia de Asas de Treva</b> (86, ~2.9 m): a small grey eagle under wings of darkness far too big for it. <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>; two legs.</item>
    /// <item><b>A Sombra da Vela</b> (97, ~3.5 m): a shadow hanging from the mast, the lateen sail and its boom of the Sun Ship, a scrap of noon in it; no legs.</item>
    /// </list>
    /// </summary>
    public static partial class MapBossRecipes
    {
        // ------------------------------------------------------------------
        // O Rei dos Ays (57)
        // ------------------------------------------------------------------

        public static void BuildAysKing(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // The biggest whirlwind of the gorge, that forgot it was ever a breeze: a funnel of dust, narrow at the ground, wide at the top.
                b.Cylinder(Vector3.zero, 0.35f, 0.6f, 0.8f, 10, EnemySwatch.SandstoneDark, twist: 0f);
                b.Cylinder(new Vector3(0f, 0.8f, 0f), 0.6f, 0.92f, 0.8f, 10, EnemySwatch.SandstoneLight, twist: 18f);
                b.Cylinder(new Vector3(0f, 1.6f, 0f), 0.92f, 1.15f, 0.62f, 10, EnemySwatch.Shroud, twist: 36f);
                // The bands of dust going round, and the stones it carries.
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0f, 0.55f + i * 0.6f, 0f), 0.5f + i * 0.25f, 0.55f + i * 0.25f, 0.08f, 10, EnemySwatch.Sandstone);
                }

                for (int i = 0; i < 7; i++)
                {
                    float a = i * Mathf.PI * 2f / 7f;
                    float r = 0.7f + 0.06f * (i % 3);
                    b.Box(new Vector3(Mathf.Sin(a) * r, 0.9f + 0.18f * (i % 4), Mathf.Cos(a) * r), new Vector3(0.18f, 0.14f, 0.16f), i % 2 == 0 ? EnemySwatch.GolemStone : EnemySwatch.GolemStoneDark, Quaternion.Euler(i * 20f, i * 51f, 0f));
                }
            });
            // The breeze it used to be, a soft light deep in the dust.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.25f, 0.62f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.14f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.36f, 0.1f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.8f, 0.46f, 0.7f), EnemySwatch.Shroud, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.16f, 0.04f, 0.3f, 0.1f, EnemySwatch.SoftEye);
                // A crown of gusts: wisps of dust blown up and back.
                for (int i = -2; i <= 2; i++)
                {
                    b.Horn(new Vector3(i * 0.14f, 0.16f, -0.05f), 0.05f, 0.34f, 3, -14f, EnemySwatch.SandstoneLight, Quaternion.Euler(-20f, 0f, -i * 12f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.28f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.08f, 0.1f), EnemySwatch.SandstoneDark, coarse: true));
        }

        // ------------------------------------------------------------------
        // A Aurora Presa (63)
        // ------------------------------------------------------------------

        public static void BuildTrappedAurora(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // A piece of the aurora that fell into the sea and froze in: an iceberg, jagged, with the light flailing inside it.
                b.Blob(new Vector3(0f, 0.42f, 0f), new Vector3(2f, 0.84f, 1.8f), EnemySwatch.GolemSnow, coarse: true);
                b.Crystal(new Vector3(0f, 0.5f, -0.1f), 0.62f, 2.45f, EnemySwatch.GolemSnow, sides: 6);
                b.Crystal(new Vector3(0.45f, 0.45f, 0.15f), 0.38f, 1.6f, EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, -16f));
                b.Crystal(new Vector3(-0.5f, 0.45f, -0.05f), 0.42f, 1.75f, EnemySwatch.GolemSnow, Quaternion.Euler(0f, 0f, 17f));
                b.Crystal(new Vector3(0.1f, 0.4f, -0.6f), 0.34f, 1.3f, EnemySwatch.Crystal, Quaternion.Euler(-14f, 0f, 6f));
                // The curtains of the aurora caught in the ice, green and violet.
                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(i * 0.2f, 1.25f + 0.08f * (i & 1), 0.46f), new Vector3(0.07f, 0.9f - 0.08f * Mathf.Abs(i), 0.04f), i % 2 == 0 ? EnemySwatch.Sorcery : EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, i * 5f));
                }
            });
            // The heart of the light, flailing to get out: it shines brighter as the ice cracks.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.1f, 0.52f), b => b.Blob(Vector3.zero, new Vector3(0.32f, 0.32f, 0.14f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.9f, 0.42f), b =>
            {
                // A face of light under the ice, eyes that only want to go home.
                b.Blob(Vector3.zero, new Vector3(0.52f, 0.4f, 0.24f), EnemySwatch.Crystal, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.03f, 0.1f, 0.09f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.1f), b => b.Blob(Vector3.zero, new Vector3(0.22f, 0.06f, 0.06f), EnemySwatch.Sorcery, coarse: true));
        }

        // ------------------------------------------------------------------
        // Guayota (67)
        // ------------------------------------------------------------------

        public static void BuildGuayota(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.6f, -0.05f), b =>
            {
                // The one of the deep who shut the sun inside the mountain: a hunched giant of black lava rock, cracked, smoking at the shoulders.
                b.Blob(Vector3.zero, new Vector3(1.2f, 1.3f, 1f), EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, 0.35f, -0.2f), new Vector3(1.25f, 0.6f, 0.8f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 6; i++)
                {
                    float a = i * Mathf.PI * 2f / 6f + 0.3f;
                    b.Box(new Vector3(Mathf.Sin(a) * 0.5f, -0.2f + 0.12f * (i % 3), Mathf.Cos(a) * 0.4f), new Vector3(0.06f, 0.42f, 0.04f), EnemySwatch.Ember, Quaternion.Euler(0f, a * Mathf.Rad2Deg, 18f * ((i & 1) * 2 - 1)));
                }

                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.42f * s, 0.62f, -0.15f), new Vector3(0.34f, 0.3f, 0.34f), EnemySwatch.Shroud, coarse: true);
                }
            });
            // The little sun he stole, held in his chest: the more he is hurt, the more it shows.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.05f, 0.46f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.16f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.74f, 0.3f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.6f, 0.52f, 0.58f), EnemySwatch.Black, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.02f, 0.26f, 0.09f, EnemySwatch.SoftEye);
                // Two blunt crags of rock on his brow.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.18f * s, 0.18f, -0.04f), 0.07f, 0.26f, 2, -12f, EnemySwatch.GolemStoneDark, Quaternion.Euler(-12f, 0f, -18f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.18f, 0.2f), b => b.Blob(Vector3.zero, new Vector3(0.28f, 0.08f, 0.12f), EnemySwatch.Ember, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Heavy arms of rock with lava in the cracks: they heave up on the windup, as the crater spits.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.62f * side, 0.32f, 0.08f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.14f * side, -0.55f, 0.28f), 0.17f, 0.14f, 6, EnemySwatch.GolemStoneDark);
                    b.Segment(new Vector3(0.14f * side, -0.55f, 0.28f), new Vector3(0.08f * side, -0.9f, 0.42f), 0.14f, 0.11f, 6, EnemySwatch.Black);
                    b.Blob(new Vector3(0.08f * side, -0.95f, 0.44f), new Vector3(0.24f, 0.2f, 0.24f), EnemySwatch.GolemStoneDark, coarse: true);
                    b.Box(new Vector3(0.12f * side, -0.4f, 0.3f), new Vector3(0.04f, 0.26f, 0.04f), EnemySwatch.Ember, Quaternion.Euler(20f, 0f, 0f));
                }, meshKey: right ? "GuayotaArm_R" : "GuayotaArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.85f, 0.32f, 0.17f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, EnemySwatch.GolemStoneDark, "Guayota");
        }

        // ------------------------------------------------------------------
        // O Nó Errado (74)
        // ------------------------------------------------------------------

        public static void BuildWrongKnot(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // A stretch of Mehen's coil that tied itself wrong in the hours of the night: the knot on the ground, the coil reared up out of it.
                for (int i = 0; i < 6; i++)
                {
                    float a = i * Mathf.PI * 2f / 6f;
                    b.Blob(new Vector3(Mathf.Sin(a) * 0.72f, 0.25f, Mathf.Cos(a) * 0.72f), new Vector3(0.58f, 0.5f, 0.58f), i % 2 == 0 ? EnemySwatch.BatFurDark : EnemySwatch.Black, coarse: true);
                }

                // The knot itself, a tight crossing of coil.
                b.Segment(new Vector3(-0.5f, 0.35f, 0.3f), new Vector3(0.5f, 0.6f, -0.3f), 0.26f, 0.26f, 8, EnemySwatch.BatFurDark);
                b.Segment(new Vector3(0.5f, 0.35f, 0.3f), new Vector3(-0.5f, 0.6f, -0.3f), 0.26f, 0.26f, 8, EnemySwatch.Black);
                b.Segment(new Vector3(0f, 0.4f, -0.2f), new Vector3(0f, 1.6f, 0f), 0.38f, 0.33f, 8, EnemySwatch.BatFurDark);
                b.Segment(new Vector3(0f, 1.6f, 0f), new Vector3(0f, 2.35f, 0.35f), 0.33f, 0.28f, 8, EnemySwatch.BatFurDark);
                // The gold bands of the great serpent, still on it.
                for (int i = 0; i < 4; i++)
                {
                    b.Cylinder(new Vector3(0f, 0.75f + i * 0.38f, -0.12f + 0.04f * i), 0.38f, 0.37f, 0.08f, 10, EnemySwatch.Gold);
                }
            });
            // The light of the barca it was coiled round to guard, a disc on its chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.45f, 0.36f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.12f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.45f, 0.5f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.56f, 0.42f, 0.74f), EnemySwatch.BatFurDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.18f, 0.1f, 0.2f, 0.09f, EnemySwatch.SoftEye);
                b.Box(new Vector3(0f, 0.2f, -0.05f), new Vector3(0.3f, 0.04f, 0.4f), EnemySwatch.Gold);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.15f, 0.2f), b => b.Blob(new Vector3(0f, 0f, 0.05f), new Vector3(0.42f, 0.1f, 0.38f), EnemySwatch.Black, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The two loops it throws, a ring of coil on either side: they lift on the windup.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.4f * side, 1.25f, 0.05f), b =>
                {
                    const int pieces = 6;
                    for (int i = 0; i < pieces; i++)
                    {
                        float a0 = i * Mathf.PI * 2f / pieces;
                        float a1 = (i + 1) * Mathf.PI * 2f / pieces;
                        Vector3 p0 = new Vector3((0.34f - Mathf.Cos(a0) * 0.34f) * side, Mathf.Sin(a0) * 0.34f, 0.1f);
                        Vector3 p1 = new Vector3((0.34f - Mathf.Cos(a1) * 0.34f) * side, Mathf.Sin(a1) * 0.34f, 0.1f);
                        b.Segment(p0, p1, 0.11f, 0.11f, 5, i % 2 == 0 ? EnemySwatch.BatFurDark : EnemySwatch.Gold);
                    }
                }, meshKey: right ? "WrongKnotLoop_R" : "WrongKnotLoop_L");
            }
        }

        // ------------------------------------------------------------------
        // A Águia de Asas de Treva (86)
        // ------------------------------------------------------------------

        public static void BuildDarkWingedEagle(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.62f, 0f), b =>
            {
                // A grey eagle of the pit that grew the wings of one of the Prince's shapes in the dark: the bird is small, the wings are not.
                b.Blob(Vector3.zero, new Vector3(0.86f, 1f, 1.06f), EnemySwatch.GolemStone, coarse: true);
                b.Blob(new Vector3(0f, -0.22f, 0.16f), new Vector3(0.66f, 0.6f, 0.66f), EnemySwatch.Bone);
                b.Blob(new Vector3(0f, 0.36f, -0.06f), new Vector3(0.64f, 0.44f, 0.66f), EnemySwatch.GolemStoneDark, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.06f, 0.42f), b => b.Blob(Vector3.zero, new Vector3(0.26f, 0.34f, 0.18f), EnemySwatch.MoonEye, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.66f, 0.26f), b =>
            {
                b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.48f, 0.48f, 0.54f), EnemySwatch.Bone, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.18f, 0.2f, 0.08f, EnemySwatch.SoftEye);
                // A few grey crest feathers, swept back.
                for (int i = -1; i <= 1; i++)
                {
                    b.Horn(new Vector3(i * 0.08f, 0.32f, -0.08f), 0.05f, 0.32f, 3, -14f, EnemySwatch.GolemStone, Quaternion.Euler(-34f, i * 12f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.08f, 0.26f), b => b.Crystal(Vector3.zero, 0.08f, 0.32f, EnemySwatch.Horn, Quaternion.Euler(80f, 0f, 0f)));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.WingRight : ProceduralEnemyParts.WingLeft, body, new Vector3(0.4f * side, 0.28f, -0.06f), b =>
                {
                    // The wings of darkness, too big and too heavy: they beat slowly, and their shadow is what falls on you.
                    b.Segment(Vector3.zero, new Vector3(0.58f * side, 0.18f, -0.08f), 0.15f, 0.1f, 6, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.58f * side, 0.18f, -0.08f), new Vector3(1.1f * side, -0.08f, -0.14f), 0.1f, 0.05f, 5, EnemySwatch.Shroud);
                    for (int i = 0; i < 6; i++)
                    {
                        float t = i / 5f;
                        Vector3 root = new Vector3(Mathf.Lerp(0.12f, 1.04f, t) * side, Mathf.Lerp(0.08f, -0.02f, t), -0.1f);
                        b.Box(root + new Vector3(0f, -0.32f, -0.02f), new Vector3(0.2f, 0.64f - 0.05f * i, 0.04f), i % 2 == 0 ? EnemySwatch.Shadow : EnemySwatch.Black);
                    }

                    b.Box(new Vector3(0.6f * side, -0.04f, -0.06f), new Vector3(0.05f, 0.46f, 0.03f), EnemySwatch.Sorcery);
                }, meshKey: s > 0 ? "DarkWingedEagleWing_R" : "DarkWingedEagleWing_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.34f, -0.46f), b =>
            {
                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(i * 0.12f, -0.06f, -0.36f), new Vector3(0.12f, 0.06f, 0.76f), i % 2 == 0 ? EnemySwatch.GolemStone : EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, i * 9f, 0f));
                }
            });

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.12f, 0.24f, 0.12f, EnemySwatch.GolemStone, EnemySwatch.Horn, EnemySwatch.Horn, "DarkWingedEagle");
        }

        // ------------------------------------------------------------------
        // A Sombra da Vela (97)
        // ------------------------------------------------------------------

        public static void BuildSailShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // A shadow that hid under the sail of the Sun Ship, where the noon can't reach, and makes it catch the wrong wind.
                b.Cylinder(Vector3.zero, 0.85f, 0.8f, 0.05f, 12, EnemySwatch.Shadow);
                b.Cylinder(new Vector3(0f, 0f, -0.25f), 0.09f, 0.07f, 3.4f, 6, EnemySwatch.Bark);
                // The boom across the deck, and the slanted yard of the lateen sail.
                b.Box(new Vector3(0f, 1f, -0.2f), new Vector3(2.4f, 0.08f, 0.08f), EnemySwatch.BarkDark);
                b.Segment(new Vector3(-1.05f, 1.05f, -0.2f), new Vector3(0.35f, 3.45f, -0.2f), 0.05f, 0.04f, 5, EnemySwatch.BarkDark);
                // The sail itself, a great triangle of cloth (both faces), with a dark stain where the shadow sits.
                Vector3 a = new Vector3(-1.05f, 1.05f, -0.28f);
                Vector3 c = new Vector3(0.35f, 3.45f, -0.28f);
                Vector3 d = new Vector3(1.1f, 1.08f, -0.28f);
                b.Triangle(a, c, d, EnemySwatch.Ivory);
                b.Triangle(a, d, c, EnemySwatch.IvoryDark);
                // The shadow hanging from the boom.
                b.Blob(new Vector3(0f, 1.25f, 0.2f), new Vector3(0.95f, 1.35f, 0.7f), EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, 0.55f, 0.2f), new Vector3(0.6f, 0.6f, 0.5f), EnemySwatch.Shadow, coarse: true);
            });
            // The scrap of the noon sun it hides from, caught in it: it shines through as it weakens.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.25f, 0.54f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.12f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.05f, 0.22f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.6f, 0.52f, 0.5f), EnemySwatch.Shadow, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.02f, 0.22f, 0.09f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.17f, 0.2f), b => b.Blob(Vector3.zero, new Vector3(0.22f, 0.06f, 0.08f), EnemySwatch.Black, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Two hands of shadow holding the sheet: they haul up on the windup, as the boom swings.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.45f * side, 1.55f, 0.2f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.22f * side, -0.45f, 0.15f), 0.11f, 0.08f, 6, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.22f * side, -0.45f, 0.15f), new Vector3(0.12f * side, -0.8f, 0.25f), 0.08f, 0.04f, 5, EnemySwatch.Shadow);
                }, meshKey: right ? "SailShadowHand_R" : "SailShadowHand_L");
            }
        }
    }
}
