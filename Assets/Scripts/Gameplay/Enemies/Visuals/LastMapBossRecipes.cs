using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The last map bosses of Regions 1–9 (Docs/Mapas100.md 16, 28, 29, 44, 49, 50, 51; <see cref="RegionBossRules.MapSpawns"/>), low-poly from the shared
    /// palette like <see cref="MapBossRecipes"/>. Pivot at the feet (y = 0), facing +Z, each with the glowing <c>Core</c> the rig feeds with the fight's phase.
    /// <list type="bullet">
    /// <item><b>O Pedreiro Mazan</b> (16, ~3 m): a stone giant built of drystone courses, a block of granite in his hand; two legs.</item>
    /// <item><b>A Moreia do Coral Cinza</b> (28, ~2.8 m): a grey moray reared up out of the mother coral she coiled round, the coral's last colour in her throat; no legs.</item>
    /// <item><b>O Caranguejo-Palácio</b> (29, ~2.6 m): a hermit crab under the whitewashed dome of Husuni's palace, a lamp of the Farol at its mouth; four legs.</item>
    /// <item><b>O Espantalho de Palha Dourada</b> (44, ~3.1 m): a golden-straw scarecrow on its pole, a sack face with stitched eyes, a straw hat; no legs.</item>
    /// <item><b>O Pesadelo da Tábua</b> (49, ~3 m): a shadow rising out of a giant's clay dream tablet, long clawed arms; no legs.</item>
    /// <item><b>A Serra de Āz</b> (50, ~2.8 m): an iron sawing engine on four legs, a great round blade for a head, Āz's coins on its back.</item>
    /// <item><b>O Garanhão Cinzento</b> (51, ~2.9 m): the dappled grey chief of the thousand horses, a long white mane; four legs and a <c>Tail</c>.</item>
    /// </list>
    /// </summary>
    public static class LastMapBossRecipes
    {
        // ------------------------------------------------------------------
        // O Pedreiro Mazan (16)
        // ------------------------------------------------------------------

        public static void BuildMazanMason(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.75f, 0f), b =>
            {
                // A mazan of the summit who came down to the drystone walls and fell in love with building: his body is laid in courses, like a wall.
                b.Blob(Vector3.zero, new Vector3(1.35f, 1.25f, 0.95f), EnemySwatch.GolemStone, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    float y = -0.42f + i * 0.26f;
                    b.Box(new Vector3(0f, y, 0.4f), new Vector3(1.05f - 0.08f * Mathf.Abs(i - 1.5f), 0.2f, 0.12f), i % 2 == 0 ? EnemySwatch.GolemStoneDark : EnemySwatch.GolemStone,
                        Quaternion.Euler(0f, 0f, i % 2 == 0 ? 2f : -2f));
                }

                b.Blob(new Vector3(0f, 0.5f, -0.15f), new Vector3(1.2f, 0.5f, 0.8f), EnemySwatch.Lichen, coarse: true);
            });
            // The crack where the shadow got in, glowing between two courses.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.05f, 0.48f), b => b.Blob(Vector3.zero, new Vector3(0.28f, 0.16f, 0.08f), EnemySwatch.GolemCrack, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.88f, 0.08f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.62f, 0.6f, 0.6f), EnemySwatch.GolemStoneDark, coarse: true);
                b.Box(new Vector3(0f, 0.12f, 0.24f), new Vector3(0.5f, 0.08f, 0.1f), EnemySwatch.GolemStone);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.02f, 0.27f, 0.08f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.22f, 0.2f), b => b.Box(new Vector3(0f, 0f, 0.04f), new Vector3(0.42f, 0.14f, 0.2f), EnemySwatch.GolemStone));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The arms he builds with: they lift the stones high on the windup, before the walls go up.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.75f * side, 0.32f, 0f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.22f * side, -0.62f, 0.12f), 0.2f, 0.16f, 6, EnemySwatch.GolemStone);
                    b.Segment(new Vector3(0.22f * side, -0.62f, 0.12f), new Vector3(0.18f * side, -1f, 0.3f), 0.16f, 0.13f, 6, EnemySwatch.GolemStoneDark);
                    b.Blob(new Vector3(0.18f * side, -1.06f, 0.32f), new Vector3(0.24f, 0.22f, 0.24f), EnemySwatch.GolemStone, coarse: true);
                    if (right)
                    {
                        // The block of granite he is about to set.
                        b.Box(new Vector3(0.18f * side, -1.18f, 0.48f), new Vector3(0.36f, 0.26f, 0.3f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 15f, 0f));
                    }
                }, meshKey: right ? "MazanMasonArm_R" : "MazanMasonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.08f, 0.36f, 0.2f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStoneDark, "MazanMason");
        }

        // ------------------------------------------------------------------
        // A Moreia do Coral Cinza (28)
        // ------------------------------------------------------------------

        public static void BuildGreyMoray(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.5f, 0f), b =>
            {
                // The mother coral she coiled round to keep the shadow off, gone grey under her, and her long body rising out of it.
                b.Blob(new Vector3(0f, -1.25f, 0f), new Vector3(1.9f, 0.5f, 1.8f), EnemySwatch.GolemStoneDark, coarse: true);
                for (int i = 0; i < 6; i++)
                {
                    float a = i * Mathf.PI * 2f / 6f;
                    b.Crystal(new Vector3(Mathf.Sin(a) * 0.7f, -1.15f, Mathf.Cos(a) * 0.65f), 0.09f, 0.45f + 0.1f * (i % 3), EnemySwatch.GolemStone,
                        Quaternion.Euler(Mathf.Cos(a) * 20f, 0f, -Mathf.Sin(a) * 20f));
                }

                b.Segment(new Vector3(0f, -1.1f, -0.1f), new Vector3(0f, -0.2f, 0.05f), 0.38f, 0.34f, 8, EnemySwatch.Shroud);
                b.Segment(new Vector3(0f, -0.2f, 0.05f), new Vector3(0f, 0.6f, 0.3f), 0.34f, 0.28f, 8, EnemySwatch.Shroud);
                // Her pale belly and the fin along her back.
                b.Segment(new Vector3(0f, -0.6f, 0.25f), new Vector3(0f, 0.5f, 0.5f), 0.12f, 0.1f, 5, EnemySwatch.CaveBelly);
                for (int i = 0; i < 5; i++)
                {
                    b.Box(new Vector3(0f, -0.6f + i * 0.28f, -0.3f + i * 0.06f), new Vector3(0.04f, 0.26f, 0.2f), EnemySwatch.Shadow);
                }
            });
            // The last colour of the mother coral, still alight in her throat.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.3f, 0.5f), b => b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.12f), EnemySwatch.Crystal, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.9f, 0.42f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.1f), new Vector3(0.5f, 0.5f, 0.8f), EnemySwatch.Shroud, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.16f, 0.12f, 0.3f, 0.08f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.3f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.1f), new Vector3(0.36f, 0.12f, 0.46f), EnemySwatch.Shadow, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.1f * s, 0.05f, 0.25f), 0.03f, 0.1f, 2, 10f, EnemySwatch.Fang);
                }
            });
            // The rest of her, coiled round the grey coral.
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -1.05f, -0.2f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.7f, 0.05f, -0.2f), 0.26f, 0.2f, 6, EnemySwatch.Shroud);
                b.Segment(new Vector3(0.7f, 0.05f, -0.2f), new Vector3(0.6f, 0.1f, 0.5f), 0.2f, 0.12f, 6, EnemySwatch.Shroud);
            });
        }

        // ------------------------------------------------------------------
        // O Caranguejo-Palácio (29)
        // ------------------------------------------------------------------

        public static void BuildPalaceCrab(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.05f, 0f), b =>
            {
                // A hermit crab that took the drowned palace's dome for its shell: the dome is whitewashed coral, with a brass finial on top.
                b.Blob(Vector3.zero, new Vector3(1.6f, 0.55f, 1.2f), EnemySwatch.Copper, coarse: true);
                b.Blob(new Vector3(0f, 0.55f, -0.1f), new Vector3(1.55f, 1.3f, 1.5f), EnemySwatch.SandstoneLight);
                b.Cylinder(new Vector3(0f, 0.2f, -0.1f), 0.8f, 0.8f, 0.14f, 10, EnemySwatch.SandstoneDark);
                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                    b.Box(new Vector3(Mathf.Sin(a) * 0.62f, 0.45f, -0.1f + Mathf.Cos(a) * 0.62f), new Vector3(0.16f, 0.3f, 0.06f), EnemySwatch.Black,
                        Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
                }

                b.Crystal(new Vector3(0f, 1.15f, -0.1f), 0.08f, 0.38f, EnemySwatch.Verdigris);
            });
            // The lamp of the Farol de Coral it carries under the dome's rim.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.05f, 0.62f), b => b.Blob(Vector3.zero, new Vector3(0.24f, 0.2f, 0.12f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.66f), b =>
            {
                // Eyes on stalks.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.15f * s, 0f, 0f), new Vector3(0.2f * s, 0.32f, 0.05f), 0.04f, 0.035f, 5, EnemySwatch.Copper);
                    b.Blob(new Vector3(0.2f * s, 0.36f, 0.06f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.SoftEye, coarse: true);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.12f, 0.05f), b => b.Box(new Vector3(0f, 0f, 0.04f), new Vector3(0.3f, 0.1f, 0.12f), EnemySwatch.CopperDark));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The claws, the right one bigger: they rise on the windup, before it scuttles.
                float size = right ? 1.2f : 0.9f;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.7f * side, -0.05f, 0.45f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.22f * side, 0.05f, 0.4f), 0.1f, 0.09f, 6, EnemySwatch.Copper);
                    b.Blob(new Vector3(0.22f * side, 0.06f, 0.62f), new Vector3(0.34f, 0.26f, 0.42f) * size, EnemySwatch.Copper, coarse: true);
                    b.Horn(new Vector3(0.22f * side, 0.12f, 0.8f), 0.07f * size, 0.22f * size, 2, 15f, EnemySwatch.CopperDark);
                }, meshKey: right ? "PalaceCrabClaw_R" : "PalaceCrabClaw_L");
            }

            HinokuraBeastRecipes.AddFourLegs(bp, 1f, 0.72f, 0.3f, -0.4f, 0.1f, EnemySwatch.Copper, EnemySwatch.CopperDark, "PalaceCrab");
        }

        // ------------------------------------------------------------------
        // O Espantalho de Palha Dourada (44)
        // ------------------------------------------------------------------

        public static void BuildGoldenScarecrow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // The scarecrow of the terraces, so proud of scaring that it scares even the light: golden straw on a bamboo pole.
                b.Segment(new Vector3(0f, -1.45f, 0f), new Vector3(0f, 0.95f, 0f), 0.07f, 0.06f, 6, EnemySwatch.Bark);
                b.Blob(new Vector3(0f, -1.42f, 0f), new Vector3(0.5f, 0.08f, 0.5f), EnemySwatch.BarkDark, coarse: true);
                b.Blob(new Vector3(0f, 0.15f, 0f), new Vector3(0.85f, 1.1f, 0.55f), EnemySwatch.Gold, coarse: true);
                b.Box(new Vector3(0f, 0.1f, 0f), new Vector3(0.9f, 0.08f, 0.6f), EnemySwatch.Leather);
                // Straw sticking out below the coat, rustling.
                for (int i = 0; i < 7; i++)
                {
                    float a = i * Mathf.PI * 2f / 7f;
                    b.Crystal(new Vector3(Mathf.Sin(a) * 0.3f, -0.35f, Mathf.Cos(a) * 0.2f), 0.06f, 0.45f, EnemySwatch.Gold, Quaternion.Euler(180f + Mathf.Cos(a) * 15f, 0f, Mathf.Sin(a) * 15f));
                }
            });
            // The moonlight it scared off the paddies and keeps in its chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.3f, 0.3f), b => b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.1f), EnemySwatch.MoonEye, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1f, 0f), b =>
            {
                // A sack face with stitched eyes, under a wide straw hat.
                b.Blob(Vector3.zero, new Vector3(0.5f, 0.55f, 0.48f), EnemySwatch.SandstoneLight, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.05f, 0.22f, 0.07f, EnemySwatch.StolenLight);
                b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.6f, 0.05f, 0.4f, 10, EnemySwatch.Gold, EnemySwatch.BarkDark);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.13f, 0.2f), b => b.Box(new Vector3(0f, 0f, 0.03f), new Vector3(0.26f, 0.04f, 0.06f), EnemySwatch.BarkDark));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The crossbar sleeves: it flings them up wide on the windup, when it scares.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.35f * side, 0.55f, 0f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.75f * side, -0.05f, 0f), 0.13f, 0.11f, 6, EnemySwatch.Leather);
                    for (int i = 0; i < 3; i++)
                    {
                        b.Crystal(new Vector3(0.8f * side, -0.05f, -0.08f + i * 0.08f), 0.04f, 0.28f, EnemySwatch.Gold, Quaternion.Euler(0f, 0f, -90f * side + (i - 1) * 15f));
                    }
                }, meshKey: right ? "GoldenScarecrowSleeve_R" : "GoldenScarecrowSleeve_L");
            }
        }

        // ------------------------------------------------------------------
        // O Pesadelo da Tábua (49)
        // ------------------------------------------------------------------

        public static void BuildTabletNightmare(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.5f, 0f), b =>
            {
                // A giant's bad dream that climbed out of the clay tablet it was written on: the tablet is still under it, the writing still lit.
                b.Box(new Vector3(0f, -1.32f, 0f), new Vector3(1.5f, 0.36f, 1f), EnemySwatch.SandstoneDark, Quaternion.Euler(0f, 8f, 0f));
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(-0.45f + i * 0.3f, -1.13f, 0.1f - 0.12f * (i % 2)), new Vector3(0.18f, 0.03f, 0.05f), EnemySwatch.Rune);
                }

                b.Blob(new Vector3(0f, -0.75f, 0f), new Vector3(0.8f, 0.7f, 0.6f), EnemySwatch.Shroud, coarse: true);
                b.Blob(Vector3.zero, new Vector3(1.05f, 1.4f, 0.75f), EnemySwatch.Shadow, coarse: true);
                b.Blob(new Vector3(0f, 0.55f, -0.1f), new Vector3(1.2f, 0.5f, 0.7f), EnemySwatch.Shroud, coarse: true);
            });
            // The bad dream itself, a knot of violet in its chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.1f, 0.38f), b => b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.12f), EnemySwatch.Sorcery, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.05f, 0.05f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.6f, 0.7f, 0.55f), EnemySwatch.Shadow, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.18f * s, 0.3f, -0.05f), 0.07f, 0.3f, 3, -12f, EnemySwatch.Shroud, Quaternion.Euler(-15f, 0f, -25f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.06f, 0.24f, 0.09f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.22f, 0.2f), b => b.Blob(new Vector3(0f, 0f, 0.04f), new Vector3(0.34f, 0.1f, 0.2f), EnemySwatch.Black, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Long arms of shadow with grasping fingers: they reach up on the windup, when it dreams at you.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.55f * side, 0.45f, 0.05f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.3f * side, -0.7f, 0.25f), 0.13f, 0.09f, 6, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.3f * side, -0.7f, 0.25f), new Vector3(0.28f * side, -1.15f, 0.45f), 0.09f, 0.06f, 6, EnemySwatch.Shroud);
                    for (int f = -1; f <= 1; f++)
                    {
                        b.Horn(new Vector3((0.28f + 0.05f * f) * side, -1.18f, 0.48f), 0.03f, 0.18f, 2, 18f, EnemySwatch.Shadow);
                    }
                }, meshKey: right ? "TabletNightmareArm_R" : "TabletNightmareArm_L");
            }
        }

        // ------------------------------------------------------------------
        // A Serra de Āz (50)
        // ------------------------------------------------------------------

        public static void BuildAzSaw(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // The sawing engine Āz had built to cut down the taiga for gold: an iron frame, a firebox, her coins heaped on the back.
                b.Box(Vector3.zero, new Vector3(1.1f, 0.7f, 1.5f), EnemySwatch.Iron);
                b.Box(new Vector3(0f, 0.38f, 0f), new Vector3(1.2f, 0.08f, 1.6f), EnemySwatch.IronDark);
                b.Box(new Vector3(0f, 0.6f, -0.45f), new Vector3(0.7f, 0.36f, 0.5f), EnemySwatch.BarkDark);
                for (int i = 0; i < 5; i++)
                {
                    b.Blob(new Vector3(-0.24f + 0.12f * i, 0.82f, -0.45f + 0.05f * (i % 2)), new Vector3(0.14f, 0.05f, 0.14f), EnemySwatch.Gold, coarse: true);
                }

                b.Cylinder(new Vector3(0.35f, 0.4f, 0.3f), 0.1f, 0.08f, 0.5f, 6, EnemySwatch.IronDark, EnemySwatch.Black);
            });
            // The fire in its box, showing through the grate.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.05f, 0.76f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.22f, 0.1f), EnemySwatch.Ember, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.6f, 0.85f), b =>
            {
                // The great round blade, standing up like a wheel, teeth all round.
                b.Cylinder(new Vector3(-0.04f, 0f, 0f), 0.7f, 0.7f, 0.08f, 14, EnemySwatch.Steel, rotation: Quaternion.Euler(0f, 0f, 90f));
                for (int i = 0; i < 12; i++)
                {
                    float a = i * Mathf.PI * 2f / 12f;
                    b.Crystal(new Vector3(0f, Mathf.Cos(a) * 0.7f, Mathf.Sin(a) * 0.7f), 0.05f, 0.12f, EnemySwatch.Steel, Quaternion.Euler(a * Mathf.Rad2Deg, 0f, 0f));
                }

                b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), EnemySwatch.IronDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.08f, 0.1f, 0.07f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.35f, -0.2f), b => b.Box(Vector3.zero, new Vector3(0.3f, 0.12f, 0.3f), EnemySwatch.IronDark));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The two levers that turn the blade: they heave up on the windup, as it turns to follow.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.58f * side, 0.3f, -0.3f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.08f * side, 0.6f, 0.15f), 0.05f, 0.04f, 5, EnemySwatch.Bark);
                    b.Blob(new Vector3(0.08f * side, 0.62f, 0.16f), new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.Copper, coarse: true);
                }, meshKey: right ? "AzSawLever_R" : "AzSawLever_L");
            }

            HinokuraBeastRecipes.AddFourLegs(bp, 1.1f, 0.5f, 0.55f, -0.55f, 0.12f, EnemySwatch.Iron, EnemySwatch.Black, "AzSaw");
        }

        // ------------------------------------------------------------------
        // O Garanhão Cinzento (51)
        // ------------------------------------------------------------------

        public static void BuildGreyStallion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.55f, 0f), b =>
            {
                // The chief of the thousand horses, a dappled grey the shadow frightened so badly he forgot the way to water.
                b.Blob(Vector3.zero, new Vector3(0.95f, 0.95f, 2.05f), EnemySwatch.GolemStone);
                for (int i = 0; i < 6; i++)
                {
                    b.Blob(new Vector3(0.38f * (i % 2 == 0 ? 1f : -1f), 0.1f - 0.08f * (i % 3), 0.6f - i * 0.25f), new Vector3(0.16f, 0.16f, 0.16f), EnemySwatch.GolemStoneDark, coarse: true);
                }

                b.Segment(new Vector3(0f, 0.25f, 0.78f), new Vector3(0f, 0.92f, 1.05f), 0.32f, 0.23f, 7, EnemySwatch.GolemStone);
                // The long white mane, blowing.
                for (int i = 0; i < 6; i++)
                {
                    b.Blob(new Vector3(0f, 0.4f + i * 0.11f, 0.35f + i * 0.12f), new Vector3(0.14f, 0.3f, 0.26f), EnemySwatch.GolemSnow, coarse: true);
                }
            });
            // The steady heart of the herd's chief, under the fright.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.05f, 0.92f), b => b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.14f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.02f, 1.12f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.12f), new Vector3(0.42f, 0.46f, 0.75f), EnemySwatch.GolemStone, coarse: true);
                b.Blob(new Vector3(0f, -0.08f, 0.44f), new Vector3(0.3f, 0.24f, 0.2f), EnemySwatch.GolemStoneDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.18f, 0.08f, 0.1f, 0.08f, EnemySwatch.SoftEye);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.11f * s, 0.21f, -0.1f), 0.05f, 0.2f, 2, -10f, EnemySwatch.GolemStoneDark, Quaternion.Euler(-10f, 0f, -10f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.16f, 0.32f), b => b.Blob(Vector3.zero, new Vector3(0.28f, 0.12f, 0.26f), EnemySwatch.GolemStoneDark, coarse: true));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.15f, -1f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.5f, -0.35f), 0.14f, 0.1f, 6, EnemySwatch.GolemSnow);
                b.Blob(new Vector3(0f, -0.62f, -0.38f), new Vector3(0.24f, 0.45f, 0.22f), EnemySwatch.GolemSnow, coarse: true);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 1.2f, 0.36f, 0.72f, -0.72f, 0.14f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, "GreyStallion");
        }
    }
}
