using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The map bosses of the fichas (Docs/Mapas100.md 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>), low-poly from the shared
    /// palette like <see cref="RegionBossRecipes"/>. Pivot at the feet (y = 0), facing +Z, each with the glowing <c>Core</c> the rig feeds with the
    /// fight's phase. Every one of them is a creature gone dark that the cousins free, not a villain: the faces say so.
    /// <list type="bullet">
    /// <item><b>O Cortador de Cordas</b> (55, ~2.9 m): a shadow bird, blade-edged wings, a frayed rope end in its beak. <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>; two legs.</item>
    /// <item><b>O Sino que Não Para</b> (56, ~3 m): a bronze bell walking on two legs, its clapper the <c>Jaw</c>, its two ropes the arms.</item>
    /// <item><b>A Morsa-Rainha</b> (60, ~2.9 m): a crowned walrus sitting up, the fish she keeps shining through her belly; flippers for arms, no legs.</item>
    /// <item><b>O Mamute Avô</b> (62, ~3 m): the old mammoth of the galleries, hair to the knees, earth on his back; tusks for arms, trunk for the <c>Jaw</c>; four legs.</item>
    /// <item><b>O Rei Shisa do Telhado</b> (66, ~3 m): the glazed lion-dog of the palace roof grown huge, curled mane, mouth wide; four legs and a <c>Tail</c>.</item>
    /// <item><b>O Mestre das Nove Fornalhas</b> (69, ~3 m): an iron forging engine with a furnace for a chest, a hammer arm and a tongs arm; two legs.</item>
    /// </list>
    /// </summary>
    public static class MapBossRecipes
    {
        // ------------------------------------------------------------------
        // O Cortador de Cordas (55)
        // ------------------------------------------------------------------

        public static void BuildRopeCutter(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.62f, 0f), b =>
            {
                // A bird of shadow that learned it likes to watch people fall. It never had to fly anywhere: the bridges did the falling.
                b.Blob(Vector3.zero, new Vector3(0.9f, 1f, 1.1f), EnemySwatch.Shadow, coarse: true);
                b.Blob(new Vector3(0f, -0.24f, 0.16f), new Vector3(0.7f, 0.62f, 0.7f), EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, 0.38f, -0.06f), new Vector3(0.66f, 0.46f, 0.7f), EnemySwatch.BatFurDark, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.06f, 0.44f), b => b.Blob(Vector3.zero, new Vector3(0.26f, 0.36f, 0.18f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.66f, 0.26f), b =>
            {
                b.Blob(new Vector3(0f, 0.14f, 0f), new Vector3(0.5f, 0.5f, 0.56f), EnemySwatch.Shadow, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.2f, 0.2f, 0.08f, EnemySwatch.WarEye);
                // A crest of cut rope ends, swept back.
                for (int i = -1; i <= 1; i++)
                {
                    b.Horn(new Vector3(i * 0.08f, 0.34f, -0.08f), 0.05f, 0.4f, 3, -14f, EnemySwatch.Leather, Quaternion.Euler(-30f, i * 12f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.08f, 0.26f), b =>
            {
                // The hooked beak, sharp as a knife, and the frayed rope it still holds.
                b.Crystal(Vector3.zero, 0.08f, 0.36f, EnemySwatch.Steel, Quaternion.Euler(80f, 0f, 0f));
                b.Box(new Vector3(0.16f, -0.08f, 0.16f), new Vector3(0.32f, 0.04f, 0.04f), EnemySwatch.Leather, Quaternion.Euler(0f, 0f, -25f));
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.WingRight : ProceduralEnemyParts.WingLeft, body, new Vector3(0.4f * side, 0.28f, -0.06f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.58f * side, 0.18f, -0.08f), 0.15f, 0.1f, 6, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.58f * side, 0.18f, -0.08f), new Vector3(1.12f * side, -0.08f, -0.14f), 0.1f, 0.05f, 5, EnemySwatch.BatFurDark);
                    for (int i = 0; i < 6; i++)
                    {
                        float t = i / 5f;
                        Vector3 root = new Vector3(Mathf.Lerp(0.12f, 1.05f, t) * side, Mathf.Lerp(0.08f, -0.02f, t), -0.1f);
                        // Every flight feather ends in a blade: that is how the ropes go.
                        b.Box(root + new Vector3(0f, -0.3f, -0.02f), new Vector3(0.2f, 0.6f - 0.05f * i, 0.04f), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Shadow);
                        b.Box(root + new Vector3(0f, -0.63f + 0.025f * i, -0.02f), new Vector3(0.16f, 0.08f, 0.03f), EnemySwatch.Steel);
                    }

                    b.Box(new Vector3(0.64f * side, -0.04f, -0.06f), new Vector3(0.05f, 0.46f, 0.03f), EnemySwatch.Rune);
                }, meshKey: s > 0 ? "RopeCutterWing_R" : "RopeCutterWing_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.34f, -0.46f), b =>
            {
                for (int i = -2; i <= 2; i++)
                {
                    b.Box(new Vector3(i * 0.12f, -0.06f, -0.38f), new Vector3(0.12f, 0.06f, 0.8f), i % 2 == 0 ? EnemySwatch.Shadow : EnemySwatch.Steel, Quaternion.Euler(0f, i * 9f, 0f));
                }
            });

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.12f, 0.24f, 0.12f, EnemySwatch.BatFurDark, EnemySwatch.Horn, EnemySwatch.Steel, "RopeCutter");
        }

        // ------------------------------------------------------------------
        // O Sino que Não Para (56)
        // ------------------------------------------------------------------

        public static void BuildEndlessBell(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.55f, 0f), b =>
            {
                // The great bell of the cliff, rung by the shadow so long that it got up to ring itself. It is tired. Bells are not meant to walk.
                b.Cylinder(new Vector3(0f, -0.82f, 0f), 1f, 0.95f, 0.12f, 12, EnemySwatch.Gold);
                b.Cylinder(new Vector3(0f, -0.72f, 0f), 0.95f, 0.55f, 1.3f, 12, EnemySwatch.Copper);
                b.Blob(new Vector3(0f, 0.6f, 0f), new Vector3(1.12f, 0.5f, 1.12f), EnemySwatch.CopperDark, coarse: true);
                b.Cylinder(new Vector3(0f, -0.38f, 0f), 0.86f, 0.84f, 0.08f, 12, EnemySwatch.Verdigris);
                b.Cylinder(new Vector3(0f, 0.18f, 0f), 0.65f, 0.63f, 0.08f, 12, EnemySwatch.Verdigris);
                // A face cast in the bronze, worn down by the ringing: closed eyes that would like to sleep.
                HinokuraBeastRecipes.AddEyes(b, 0.22f, 0.02f, 0.74f, 0.1f, EnemySwatch.SoftEye);
                b.Box(new Vector3(0f, -0.18f, 0.77f), new Vector3(0.3f, 0.04f, 0.04f), EnemySwatch.CopperDark);
            });
            // The sound that never stops, glowing through a crack in the bronze.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.5f, 0.82f), b =>
            {
                b.Box(Vector3.zero, new Vector3(0.12f, 0.46f, 0.06f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 0f, 12f));
                b.Box(new Vector3(0.08f, -0.22f, 0f), new Vector3(0.18f, 0.06f, 0.05f), EnemySwatch.StolenLight);
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.88f, 0f), b =>
            {
                // The crown loop and the yoke it hung from.
                b.Box(new Vector3(0f, 0.06f, 0f), new Vector3(1f, 0.14f, 0.22f), EnemySwatch.BarkDark);
                b.Cylinder(new Vector3(0f, 0.12f, 0f), 0.18f, 0.12f, 0.36f, 8, EnemySwatch.CopperDark);
                b.Horn(new Vector3(0.44f, 0.12f, 0f), 0.06f, 0.3f, 3, 18f, EnemySwatch.Gold, Quaternion.Euler(0f, 0f, -20f), bendAxis: Vector3.forward);
                b.Horn(new Vector3(-0.44f, 0.12f, 0f), 0.06f, 0.3f, 3, -18f, EnemySwatch.Gold, Quaternion.Euler(0f, 0f, 20f), bendAxis: Vector3.forward);
            });
            // The clapper, hanging inside: it swings on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.2f, 0f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -1.05f, 0f), 0.06f, 0.06f, 6, EnemySwatch.IronDark);
                b.Blob(new Vector3(0f, -1.15f, 0f), new Vector3(0.3f, 0.3f, 0.3f), EnemySwatch.Iron, coarse: true);
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The two bell ropes, which it pulls itself.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.66f * side, 0.36f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), EnemySwatch.Gold, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.3f * side, -0.9f, 0.12f), 0.07f, 0.06f, 6, EnemySwatch.Leather);
                    b.Box(new Vector3(0.31f * side, -0.98f, 0.12f), new Vector3(0.14f, 0.22f, 0.14f), EnemySwatch.BarkDark);
                }, Quaternion.Euler(-6f, 0f, 0f), meshKey: right ? "EndlessBellArm_R" : "EndlessBellArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.78f, 0.32f, 0.16f, EnemySwatch.IronDark, EnemySwatch.Iron, EnemySwatch.CopperDark, "EndlessBell");
        }

        // ------------------------------------------------------------------
        // A Morsa-Rainha (60)
        // ------------------------------------------------------------------

        public static void BuildWalrusQueen(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1f, 0f), b =>
            {
                // She sits up on the ice like a heap of old leather and wants every fish of the canal for herself. Freed, she shares.
                b.Cylinder(new Vector3(0f, -1f, 0f), 0.98f, 0.9f, 0.5f, 10, EnemySwatch.CaveSkinDark, bottomCap: true);
                b.Blob(new Vector3(0f, 0.05f, 0f), new Vector3(1.86f, 1.9f, 1.66f), EnemySwatch.Leather);
                b.Blob(new Vector3(0f, -0.1f, 0.42f), new Vector3(1.3f, 1.3f, 0.9f), EnemySwatch.CaveBelly, coarse: true);
                // The fish she keeps, shining through her belly.
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3((i - 1) * 0.3f, -0.42f + 0.12f * (i % 2), 0.84f), new Vector3(0.22f, 0.08f, 0.04f), EnemySwatch.FireflyGlow, Quaternion.Euler(0f, 0f, (i - 1) * 20f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.08f, 0.84f), b => b.Blob(Vector3.zero, new Vector3(0.5f, 0.4f, 0.16f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.1f, 0.26f), b =>
            {
                b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.82f, 0.66f, 0.76f), EnemySwatch.Leather, coarse: true);
                b.Blob(new Vector3(0f, -0.06f, 0.32f), new Vector3(0.6f, 0.32f, 0.3f), EnemySwatch.CaveBelly, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.2f, 0.2f, 0.3f, 0.08f, EnemySwatch.SoftEye);
                // The crown she gave herself, of ice and the canal's gold.
                b.Cylinder(new Vector3(0f, 0.38f, -0.02f), 0.24f, 0.28f, 0.16f, 8, EnemySwatch.Gold);
                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI * 0.5f + Mathf.PI * 0.25f;
                    b.Crystal(new Vector3(Mathf.Cos(a) * 0.2f, 0.52f, Mathf.Sin(a) * 0.2f - 0.02f), 0.05f, 0.18f, EnemySwatch.Crystal);
                }
            });
            // The two great tusks: they rise on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.14f, 0.38f), b =>
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.12f * s, 0f, 0f), 0.06f, 0.62f, 4, 6f, EnemySwatch.Ivory, Quaternion.Euler(170f, 0f, 0f));
                }
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.82f * side, -0.38f, 0.2f), b =>
                {
                    b.Blob(new Vector3(0.18f * side, -0.18f, 0.06f), new Vector3(0.34f, 0.78f, 0.52f), EnemySwatch.CaveSkinDark, Quaternion.Euler(0f, 0f, 28f * side), coarse: true);
                    b.Box(new Vector3(0.32f * side, -0.5f, 0.12f), new Vector3(0.3f, 0.06f, 0.36f), EnemySwatch.CaveSkin);
                }, meshKey: right ? "WalrusQueenFlipper_R" : "WalrusQueenFlipper_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.86f, -0.78f), b =>
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.22f * s, 0f, -0.18f), new Vector3(0.36f, 0.08f, 0.46f), EnemySwatch.CaveSkinDark, Quaternion.Euler(0f, 18f * s, 0f));
                }
            });
        }

        // ------------------------------------------------------------------
        // O Mamute Avô (62)
        // ------------------------------------------------------------------

        public static void BuildGrandfatherMammoth(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.78f, -0.1f), b =>
            {
                // The one who dug the valleys from below, so long ago that he forgot which way is up.
                b.Blob(Vector3.zero, new Vector3(1.5f, 1.4f, 2.2f), EnemySwatch.WolfFurDark);
                b.Blob(new Vector3(0f, 0.5f, 0.42f), new Vector3(1.2f, 0.8f, 1.1f), EnemySwatch.WolfFur, coarse: true);
                // The long hair, down to the knees.
                for (int i = 0; i < 8; i++)
                {
                    float z = Mathf.Lerp(-0.9f, 0.8f, i / 7f);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Box(new Vector3(0.66f * s, -0.62f, z), new Vector3(0.12f, 0.62f, 0.2f), i % 2 == 0 ? EnemySwatch.WolfFurDark : EnemySwatch.BarkDark);
                    }
                }

                // The earth of the galleries still on his back, moss and all.
                b.Rock(new Vector3(0.1f, 0.72f, -0.38f), new Vector3(0.7f, 0.34f, 0.6f), EnemySwatch.GolemStoneDark, 2, upFacing: EnemySwatch.CaveMoss);
            });
            // The spark he carries up without knowing: crystals grown in the earth on his back.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(-0.2f, 0.82f, -0.2f), b =>
            {
                b.Crystal(Vector3.zero, 0.1f, 0.36f, EnemySwatch.Crystal, Quaternion.Euler(10f, 0f, 14f));
                b.Crystal(new Vector3(0.14f, -0.02f, 0.06f), 0.07f, 0.24f, EnemySwatch.StolenLight, Quaternion.Euler(-12f, 0f, -18f));
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 1.14f), b =>
            {
                b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.92f, 1f, 0.8f), EnemySwatch.WolfFurDark, coarse: true);
                b.Blob(new Vector3(0f, 0.5f, -0.1f), new Vector3(0.7f, 0.4f, 0.6f), EnemySwatch.WolfFur, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.28f, 0.18f, 0.32f, 0.08f, EnemySwatch.SoftEye);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.46f * s, 0.06f, -0.12f), new Vector3(0.16f, 0.5f, 0.36f), EnemySwatch.WolfFurDark, coarse: true);
                }
            });
            // The trunk, curling down: it lifts on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.12f, 0.34f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.5f, 0.16f), 0.16f, 0.13f, 6, EnemySwatch.WolfFurDark);
                b.Segment(new Vector3(0f, -0.5f, 0.16f), new Vector3(0f, -0.92f, 0.12f), 0.13f, 0.1f, 6, EnemySwatch.WolfFurDark);
                b.Segment(new Vector3(0f, -0.92f, 0.12f), new Vector3(0f, -1.06f, 0.26f), 0.1f, 0.08f, 5, EnemySwatch.WolfFur);
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The tusks, curling forward and up: they rise on the windup like arms.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.26f * side, 0.06f, 1.32f), b =>
                {
                    b.Horn(Vector3.zero, 0.11f, 0.96f, 4, -28f, EnemySwatch.Ivory, Quaternion.Euler(150f, 0f, 14f * side));
                }, meshKey: right ? "GrandfatherMammothTusk_R" : "GrandfatherMammothTusk_L");
            }

            HinokuraBeastRecipes.AddHeavyLegs(bp, 1.12f, 0.46f, 0.52f, -0.66f, 0.2f, EnemySwatch.WolfFurDark, EnemySwatch.GolemStoneDark, "GrandfatherMammoth");
        }

        // ------------------------------------------------------------------
        // O Rei Shisa do Telhado (66)
        // ------------------------------------------------------------------

        public static void BuildShisaKing(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.42f, -0.1f), b =>
            {
                // The guardian of the palace roof, who came down to chase the tremor and forgot to go back up. Glazed clay, hollow, proud.
                b.Blob(Vector3.zero, new Vector3(1.16f, 1.06f, 1.8f), EnemySwatch.Copper);
                b.Blob(new Vector3(0f, 0.32f, 0.56f), new Vector3(1.08f, 1.06f, 0.9f), EnemySwatch.Copper, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, 0.5f - i * 0.02f, -0.6f + i * 0.34f), new Vector3(0.18f, 0.08f, 0.2f), EnemySwatch.Verdigris);
                }
            });
            // The pearl at its collar, the fire of the gardens kept in glaze.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.02f, 1.02f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.22f), EnemySwatch.Ember, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.92f, 0.82f), b =>
            {
                b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.94f, 0.84f, 0.84f), EnemySwatch.Copper, coarse: true);
                // The curled mane, glazed green.
                for (int i = 0; i < 9; i++)
                {
                    float a = Mathf.Lerp(-150f, -30f, i / 8f) * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.52f, -Mathf.Sin(a) * 0.46f - 0.06f, -0.18f), new Vector3(0.3f, 0.3f, 0.3f), EnemySwatch.Verdigris, coarse: true);
                }

                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.52f * s, -0.06f, -0.14f), new Vector3(0.3f, 0.34f, 0.3f), EnemySwatch.Verdigris, coarse: true);
                    b.Box(new Vector3(0.18f * s, 0.24f, 0.4f), new Vector3(0.24f, 0.08f, 0.06f), EnemySwatch.CopperDark, Quaternion.Euler(0f, 0f, -14f * s));
                }

                // Bulging eyes that bark at the tremor.
                HinokuraBeastRecipes.AddEyes(b, 0.17f, 0.12f, 0.38f, 0.14f, EnemySwatch.WarEye);
                b.Blob(new Vector3(0f, -0.02f, 0.42f), new Vector3(0.24f, 0.16f, 0.12f), EnemySwatch.CopperDark, coarse: true);
            });
            // The wide mouth, teeth out, that opens on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.24f, 0.26f), b =>
            {
                b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.66f, 0.2f, 0.42f), EnemySwatch.CopperDark, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Crystal(new Vector3((i - 1.5f) * 0.13f, 0.02f, 0.24f), 0.04f, 0.14f, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.3f, -0.86f), b =>
            {
                // The flame-curl tail of the roof shisa.
                b.Blob(new Vector3(0f, 0.16f, -0.12f), new Vector3(0.36f, 0.5f, 0.36f), EnemySwatch.Verdigris, coarse: true);
                b.Blob(new Vector3(0f, 0.46f, -0.2f), new Vector3(0.3f, 0.36f, 0.3f), EnemySwatch.Verdigris, coarse: true);
                b.Blob(new Vector3(0f, 0.64f, -0.06f), new Vector3(0.22f, 0.24f, 0.22f), EnemySwatch.Copper, coarse: true);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 0.92f, 0.4f, 0.5f, -0.62f, 0.17f, EnemySwatch.Copper, EnemySwatch.IvoryDark, "ShisaKing");
        }

        // ------------------------------------------------------------------
        // O Mestre das Nove Fornalhas (69)
        // ------------------------------------------------------------------

        public static void BuildFurnaceMaster(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.72f, 0f), b =>
            {
                // The engine that forges for Āz without stopping, because nobody ever taught it how to stop.
                b.Box(Vector3.zero, new Vector3(1.24f, 1.16f, 0.92f), EnemySwatch.IronDark);
                b.Box(new Vector3(0f, -0.66f, 0f), new Vector3(0.9f, 0.24f, 0.7f), EnemySwatch.Iron);
                b.Box(new Vector3(0f, 0.08f, 0.47f), new Vector3(0.78f, 0.66f, 0.04f), EnemySwatch.Black);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3((i - 1.5f) * 0.18f, 0.08f, 0.5f), new Vector3(0.05f, 0.62f, 0.05f), EnemySwatch.Iron);
                }

                // Three of the nine chimneys, on the shoulders and the back.
                b.Cylinder(new Vector3(0.42f, 0.58f, -0.2f), 0.14f, 0.12f, 0.56f, 7, EnemySwatch.Iron, EnemySwatch.Ember);
                b.Cylinder(new Vector3(-0.42f, 0.58f, -0.2f), 0.14f, 0.12f, 0.56f, 7, EnemySwatch.Iron, EnemySwatch.Ember);
                b.Cylinder(new Vector3(0f, 0.58f, -0.34f), 0.12f, 0.1f, 0.42f, 7, EnemySwatch.Iron, EnemySwatch.Ember);
                // The bellows on its back, old leather.
                b.Blob(new Vector3(0f, -0.1f, -0.58f), new Vector3(0.8f, 0.7f, 0.36f), EnemySwatch.Leather, coarse: true);
                for (int i = 0; i < 6; i++)
                {
                    b.Box(new Vector3(0.6f * (i % 2 == 0 ? 1f : -1f), 0.46f - (i / 2) * 0.4f, 0.47f), new Vector3(0.06f, 0.06f, 0.04f), EnemySwatch.Gold);
                }
            });
            // The furnace fire behind the grate.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.08f, 0.44f), b => b.Box(Vector3.zero, new Vector3(0.62f, 0.52f, 0.06f), EnemySwatch.Ember));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.74f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, 0.12f, 0f), new Vector3(0.5f, 0.38f, 0.48f), EnemySwatch.Iron);
                b.Box(new Vector3(0f, 0.16f, 0.25f), new Vector3(0.36f, 0.06f, 0.03f), EnemySwatch.WarEye);
                b.Box(new Vector3(0f, 0.34f, 0f), new Vector3(0.56f, 0.06f, 0.54f), EnemySwatch.Steel);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.18f), b => b.Box(new Vector3(0f, -0.04f, 0.04f), new Vector3(0.4f, 0.12f, 0.12f), EnemySwatch.IronDark));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.78f * side, 0.36f, 0.02f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.4f, 0.4f, 0.4f), EnemySwatch.Iron, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.12f * side, -0.64f, 0.1f), 0.15f, 0.13f, 6, EnemySwatch.IronDark);
                    b.Segment(new Vector3(0.12f * side, -0.64f, 0.1f), new Vector3(0.14f * side, -1.04f, 0.2f), 0.13f, 0.11f, 6, EnemySwatch.Iron);
                    if (right)
                    {
                        // The forging hammer.
                        b.Segment(new Vector3(0.14f * side, -1.04f, 0.2f), new Vector3(0.14f * side, -1.04f, 0.62f), 0.05f, 0.05f, 5, EnemySwatch.BarkDark);
                        b.Box(new Vector3(0.14f * side, -1.04f, 0.68f), new Vector3(0.3f, 0.3f, 0.22f), EnemySwatch.Steel);
                    }
                    else
                    {
                        // The tongs, still holding a glowing bar.
                        b.Box(new Vector3(0.1f * side, -1.2f, 0.24f), new Vector3(0.04f, 0.3f, 0.04f), EnemySwatch.IronDark, Quaternion.Euler(0f, 0f, 10f));
                        b.Box(new Vector3(0.2f * side, -1.2f, 0.24f), new Vector3(0.04f, 0.3f, 0.04f), EnemySwatch.IronDark, Quaternion.Euler(0f, 0f, -10f));
                        b.Box(new Vector3(0.15f * side, -1.38f, 0.24f), new Vector3(0.08f, 0.08f, 0.34f), EnemySwatch.Ember);
                    }
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: right ? "FurnaceMasterArm_R" : "FurnaceMasterArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.98f, 0.36f, 0.2f, EnemySwatch.IronDark, EnemySwatch.Iron, EnemySwatch.IronDark, "FurnaceMaster");
        }
    }
}
