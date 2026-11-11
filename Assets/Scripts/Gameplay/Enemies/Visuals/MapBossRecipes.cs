using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The map bosses of the fichas (Docs/Mapas100.md 25, 26, 32, 37, 38, 43, 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>), low-poly from the shared
    /// palette like <see cref="RegionBossRecipes"/>. Pivot at the feet (y = 0), facing +Z, each with the glowing <c>Core</c> the rig feeds with the
    /// fight's phase. Every one of them is a creature gone dark that the cousins free, not a villain: the faces say so.
    /// <list type="bullet">
    /// <item><b>O Cortador de Cordas</b> (55, ~2.9 m): a shadow bird, blade-edged wings, a frayed rope end in its beak. <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>; two legs.</item>
    /// <item><b>O Sino que Não Para</b> (56, ~3 m): a bronze bell walking on two legs, its clapper the <c>Jaw</c>, its two ropes the arms.</item>
    /// <item><b>A Morsa-Rainha</b> (60, ~2.9 m): a crowned walrus sitting up, the fish she keeps shining through her belly; flippers for arms, no legs.</item>
    /// <item><b>O Mamute Avô</b> (62, ~3 m): the old mammoth of the galleries, hair to the knees, earth on his back; tusks for arms, trunk for the <c>Jaw</c>; four legs.</item>
    /// <item><b>O Rei Shisa do Telhado</b> (66, ~3 m): the glazed lion-dog of the palace roof grown huge, curled mane, mouth wide; four legs and a <c>Tail</c>.</item>
    /// <item><b>O Mestre das Nove Fornalhas</b> (69, ~3 m): an iron forging engine with a furnace for a chest, a hammer arm and a tongs arm; two legs.</item>
    /// <item><b>O Polvo das Monções</b> (25, ~2.75 m): a dimmed octopus heaped on the sand under a torn dhow sail; the two front arms are the arms, the other six the <c>Tail</c>; no legs.</item>
    /// <item><b>O Touro de Fumaça</b> (26, ~2.85 m): a giant aurochs, lyre horns, dark smoke curling off its back, an ember in its chest; four legs and a <c>Tail</c>.</item>
    /// <item><b>A Ponte que Esqueceu</b> (32, ~2.9 m): a root bridge walking on four root legs, planks on its back (two forgotten), a young fig growing from it; its rope rails are the arms.</item>
    /// <item><b>O Carrinho Desgovernado</b> (37, ~2.65 m): an Artificer ore cart on four iron legs, ore heaped high, the engine lit behind the grate; brake levers for arms, the plough for the <c>Jaw</c>.</item>
    /// <item><b>O Grande Almas</b> (38, ~3 m): a huge shy hairy wild man, snow and frozen sparks on his shoulders, a snowball in hand; two legs.</item>
    /// <item><b>A Carpa que Não Pulou</b> (43, ~2.65 m): a great dull-gold carp standing on its fins, barbels, the dragon's pearl under its chin; pectoral fins for arms; no legs.</item>
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

        // ------------------------------------------------------------------
        // O Polvo das Monções (25)
        // ------------------------------------------------------------------

        public static void BuildMonsoonOctopus(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.25f, -0.1f), b =>
            {
                // It holds on to the dhows because everything the monsoon takes away might not come back. The torn sail is still on its head.
                b.Cylinder(new Vector3(0f, -1.25f, 0f), 1.25f, 0.78f, 0.62f, 12, EnemySwatch.CaveSkinDark, bottomCap: true);
                b.Blob(new Vector3(0f, -0.42f, 0.05f), new Vector3(1.7f, 0.9f, 1.6f), EnemySwatch.VampFur, coarse: true);
                b.Blob(new Vector3(0f, 0.35f, -0.2f), new Vector3(1.4f, 1.85f, 1.5f), EnemySwatch.VampWing);
                b.Blob(new Vector3(0f, 0.75f, -0.35f), new Vector3(0.9f, 0.8f, 0.9f), EnemySwatch.VampFur, coarse: true);
                // Pale spots along the mantle.
                for (int i = 0; i < 6; i++)
                {
                    float a = i * Mathf.PI / 3f;
                    b.Blob(new Vector3(Mathf.Cos(a) * 0.62f, 0.25f + 0.12f * (i % 2), Mathf.Sin(a) * 0.62f - 0.2f), new Vector3(0.16f, 0.16f, 0.1f), EnemySwatch.Pink, coarse: true);
                }

                // The sail it tore off a dhow, draped over its head, and the broken spar.
                b.Box(new Vector3(0f, 1.18f, -0.45f), new Vector3(1.05f, 0.62f, 0.05f), EnemySwatch.OwlFace, Quaternion.Euler(-24f, 8f, 6f));
                b.Segment(new Vector3(-0.6f, 1.42f, -0.36f), new Vector3(0.62f, 1.3f, -0.5f), 0.04f, 0.04f, 5, EnemySwatch.BarkDark);
            });
            // The monsoon it swallowed, glowing through the mantle.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.38f, 0.5f), b => b.Blob(Vector3.zero, new Vector3(0.42f, 0.52f, 0.2f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, -0.15f, 0.62f), b =>
            {
                // The great sad eyes of an octopus, set low over the arms.
                b.Blob(Vector3.zero, new Vector3(0.95f, 0.42f, 0.36f), EnemySwatch.VampWing, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.3f, 0.06f, 0.14f, 0.2f, EnemySwatch.SoftEye);
                HinokuraBeastRecipes.AddEyes(b, 0.3f, 0.06f, 0.22f, 0.08f, EnemySwatch.Black);
            });
            // The siphon, that puffs up on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.26f, 0.12f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.12f, 0.26f), 0.12f, 0.08f, 6, EnemySwatch.VampFur, EnemySwatch.Black));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The two front arms, curled forward on the sand: they rise and slam on the windup.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.5f * side, -0.82f, 0.55f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.28f * side, -0.3f, 0.42f), 0.2f, 0.15f, 6, EnemySwatch.VampWing);
                    b.Segment(new Vector3(0.28f * side, -0.3f, 0.42f), new Vector3(0.46f * side, -0.32f, 0.82f), 0.15f, 0.09f, 6, EnemySwatch.VampWing);
                    b.Segment(new Vector3(0.46f * side, -0.32f, 0.82f), new Vector3(0.34f * side, -0.18f, 1.02f), 0.09f, 0.04f, 5, EnemySwatch.VampFur);
                    for (int i = 0; i < 4; i++)
                    {
                        float t = (i + 0.5f) / 4f;
                        b.Blob(new Vector3(Mathf.Lerp(0.05f, 0.44f, t) * side, -0.4f + 0.04f * i, Mathf.Lerp(0.12f, 0.86f, t)), new Vector3(0.1f, 0.05f, 0.1f), EnemySwatch.Pink, coarse: true);
                    }
                }, meshKey: right ? "MonsoonOctopusArm_R" : "MonsoonOctopusArm_L");
            }

            // The six other arms, spread on the sand round and behind it.
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -1.08f, -0.2f), b =>
            {
                for (int i = 0; i < 6; i++)
                {
                    float a = Mathf.Lerp(-20f, 200f, i / 5f) * Mathf.Deg2Rad;
                    Vector3 dir = new Vector3(Mathf.Cos(a), 0f, -Mathf.Abs(Mathf.Sin(a)) - 0.1f).normalized;
                    Vector3 start = dir * 0.6f;
                    Vector3 mid = dir * 0.95f + Vector3.down * 0.04f;
                    Vector3 tip = dir * 1.18f + Vector3.up * 0.08f;
                    b.Segment(start, mid, 0.13f, 0.08f, 5, EnemySwatch.VampWing);
                    b.Segment(mid, tip, 0.08f, 0.03f, 5, EnemySwatch.VampFur);
                }
            });
        }

        // ------------------------------------------------------------------
        // O Touro de Fumaça (26)
        // ------------------------------------------------------------------

        public static void BuildSmokeAurochs(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.5f, -0.05f), b =>
            {
                // The leader of Aratta's herd, lost in the dark smoke of the old grass. The smoke still curls off its back.
                b.Blob(Vector3.zero, new Vector3(1.3f, 1.2f, 2f), EnemySwatch.WolfFurDark);
                b.Blob(new Vector3(0f, 0.4f, 0.45f), new Vector3(1.12f, 0.72f, 0.95f), EnemySwatch.WolfFurDark, coarse: true);
                b.Blob(new Vector3(0f, -0.28f, 0.1f), new Vector3(1.05f, 0.6f, 1.6f), EnemySwatch.WolfFur, coarse: true);
                // The pale stripe down the spine of the old aurochs.
                for (int i = 0; i < 5; i++)
                {
                    b.Box(new Vector3(0f, 0.64f - i * 0.04f, 0.5f - i * 0.32f), new Vector3(0.14f, 0.06f, 0.28f), EnemySwatch.WolfBelly);
                }

                // The dark smoke rising off its back.
                b.Blob(new Vector3(0.05f, 0.82f, -0.15f), new Vector3(0.66f, 0.42f, 0.62f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(-0.15f, 1.04f, -0.42f), new Vector3(0.48f, 0.38f, 0.46f), EnemySwatch.Shadow, coarse: true);
                b.Blob(new Vector3(0.12f, 1.2f, -0.62f), new Vector3(0.32f, 0.3f, 0.32f), EnemySwatch.Shroud, coarse: true);
            });
            // The ember in its chest: the clean low fire it forgot, still burning.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.05f, 0.98f), b => b.Blob(Vector3.zero, new Vector3(0.4f, 0.42f, 0.18f), EnemySwatch.Ember, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 1.1f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.1f), new Vector3(0.78f, 0.74f, 0.82f), EnemySwatch.WolfFurDark, coarse: true);
                b.Blob(new Vector3(0f, -0.16f, 0.42f), new Vector3(0.52f, 0.4f, 0.32f), EnemySwatch.WolfFur, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.26f, 0.12f, 0.32f, 0.08f, EnemySwatch.WarEye);
                // Nostrils glowing with the smoke it breathes.
                HinokuraBeastRecipes.AddEyes(b, 0.1f, -0.2f, 0.58f, 0.08f, EnemySwatch.Ember);
                // The great lyre horns of the aurochs.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.32f * s, 0.26f, 0.05f), 0.09f, 0.72f, 4, -16f, EnemySwatch.Horn, Quaternion.Euler(-10f, 0f, -68f * s), bendAxis: Vector3.forward * s);
                }
            });
            // The lower jaw, that drops on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.3f, 0.3f), b => b.Blob(new Vector3(0f, 0f, 0.08f), new Vector3(0.44f, 0.18f, 0.38f), EnemySwatch.WolfFur, coarse: true));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.25f, -0.98f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.72f, -0.22f), 0.06f, 0.04f, 5, EnemySwatch.WolfFurDark);
                b.Blob(new Vector3(0f, -0.8f, -0.24f), new Vector3(0.16f, 0.26f, 0.16f), EnemySwatch.Shadow, coarse: true);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 1.02f, 0.42f, 0.58f, -0.68f, 0.18f, EnemySwatch.WolfFurDark, EnemySwatch.Black, "SmokeAurochs");
        }

        // ------------------------------------------------------------------
        // A Ponte que Esqueceu (32)
        // ------------------------------------------------------------------

        public static void BuildForgetfulBridge(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.7f, 0f), b =>
            {
                // A living root bridge left alone so long it forgot it was a bridge, pulled its roots out of the rock and walked off.
                b.Blob(new Vector3(0f, -0.3f, 0f), new Vector3(1.2f, 0.55f, 2.2f), EnemySwatch.Bark, coarse: true);
                for (int i = -4; i <= 4; i++)
                {
                    // The planks of root across its back; a few already forgotten (missing).
                    if (i == -2 || i == 3)
                    {
                        continue;
                    }

                    b.Box(new Vector3(0f, 0f, i * 0.25f), new Vector3(1.4f, 0.08f, 0.2f), i % 2 == 0 ? EnemySwatch.Bark : EnemySwatch.BarkDark, Quaternion.Euler(0f, i * 2f, 0f));
                }

                // Moss in the joints, and a young fig growing out of the deck.
                b.Blob(new Vector3(-0.3f, 0.05f, 0.6f), new Vector3(0.4f, 0.12f, 0.4f), EnemySwatch.CaveMoss, coarse: true);
                b.Segment(new Vector3(0.32f, 0.04f, -0.6f), new Vector3(0.38f, 0.86f, -0.7f), 0.06f, 0.04f, 5, EnemySwatch.Bark);
                b.Blob(new Vector3(0.38f, 1f, -0.7f), new Vector3(0.62f, 0.42f, 0.58f), EnemySwatch.Lichen, coarse: true);
                // Hanging aerial roots under the deck.
                for (int i = 0; i < 6; i++)
                {
                    float z = Mathf.Lerp(-0.9f, 0.9f, i / 5f);
                    float x = (i % 2 == 0 ? 0.45f : -0.45f);
                    b.Segment(new Vector3(x, -0.4f, z), new Vector3(x * 1.1f, -0.95f, z + 0.05f), 0.05f, 0.02f, 4, EnemySwatch.BarkDark);
                }
            });
            // The Living Water still in its heartwood: it glows where it was watered last.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.4f, 0.86f), b => b.Blob(Vector3.zero, new Vector3(0.32f, 0.3f, 0.26f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 1.3f), b =>
            {
                // The knot at the bridge's end, which became a face.
                b.Blob(Vector3.zero, new Vector3(0.7f, 0.66f, 0.56f), EnemySwatch.BarkDark, coarse: true);
                b.Blob(new Vector3(0f, 0.38f, -0.06f), new Vector3(0.5f, 0.3f, 0.48f), EnemySwatch.CaveMoss, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.06f, 0.24f, 0.11f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.2f, 0.2f), b => b.Box(new Vector3(0f, -0.02f, 0.04f), new Vector3(0.38f, 0.1f, 0.14f), EnemySwatch.Bark));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The rope rails of root along each side, on three posts: they lift on the windup.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.68f * side, 0.04f, 0f), b =>
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        b.Segment(new Vector3(0f, 0f, i * 0.9f), new Vector3(0.03f * side, 0.58f, i * 0.9f), 0.05f, 0.04f, 5, EnemySwatch.BarkDark);
                    }

                    b.Segment(new Vector3(0.03f * side, 0.56f, -0.95f), new Vector3(0.03f * side, 0.5f, 0f), 0.035f, 0.035f, 4, EnemySwatch.Leather);
                    b.Segment(new Vector3(0.03f * side, 0.5f, 0f), new Vector3(0.03f * side, 0.56f, 0.95f), 0.035f, 0.035f, 4, EnemySwatch.Leather);
                }, meshKey: right ? "ForgetfulBridgeRail_R" : "ForgetfulBridgeRail_L");
            }

            HinokuraBeastRecipes.AddHeavyLegs(bp, 1.3f, 0.48f, 0.78f, -0.78f, 0.15f, EnemySwatch.Bark, EnemySwatch.BarkDark, "ForgetfulBridge");
        }

        // ------------------------------------------------------------------
        // O Carrinho Desgovernado (37)
        // ------------------------------------------------------------------

        public static void BuildRunawayCart(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // An ore cart of the Artificers whose little engine went dark and forgot how to stop.
                b.Box(Vector3.zero, new Vector3(1.3f, 0.8f, 1.9f), EnemySwatch.IronDark);
                b.Box(new Vector3(0f, 0.42f, 0f), new Vector3(1.4f, 0.08f, 2f), EnemySwatch.Copper);
                b.Box(new Vector3(0f, -0.1f, 0f), new Vector3(1.34f, 0.08f, 1.94f), EnemySwatch.CopperDark);
                // The ore heaped high, some of it still lit.
                b.Rock(new Vector3(0f, 0.6f, 0.05f), new Vector3(1.1f, 0.48f, 1.5f), EnemySwatch.GolemStoneDark, 1, upFacing: EnemySwatch.GolemStone);
                b.Crystal(new Vector3(0.25f, 0.7f, -0.2f), 0.08f, 0.26f, EnemySwatch.GolemCrack);
                b.Crystal(new Vector3(-0.3f, 0.66f, 0.3f), 0.07f, 0.22f, EnemySwatch.GolemCrack);
                // The engine's chimney at the back.
                b.Cylinder(new Vector3(0.38f, 0.4f, -0.72f), 0.12f, 0.1f, 0.8f, 7, EnemySwatch.Iron, EnemySwatch.Ember);
                // The wheels it no longer needs: it ran so long it grew legs.
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        b.Cylinder(new Vector3(0.66f * s, -0.32f, 0.6f * z), 0.28f, 0.28f, 0.08f, 10, EnemySwatch.Iron, EnemySwatch.Steel, Quaternion.Euler(0f, 0f, 90f * s));
                    }
                }
            });
            // The engine fire behind the front grate.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.02f, 0.96f), b => b.Box(Vector3.zero, new Vector3(0.62f, 0.4f, 0.05f), EnemySwatch.Ember));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 1f), b =>
            {
                // The miner's lamp on the front, which became its eyes.
                b.Cylinder(new Vector3(0f, 0f, -0.05f), 0.2f, 0.2f, 0.22f, 8, EnemySwatch.Copper, EnemySwatch.StolenLight, Quaternion.Euler(90f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.3f, 0.02f, 0.06f, 0.1f, EnemySwatch.WarEye);
            });
            // The ore plough at the front, that drops on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.62f, 0.06f), b => b.Box(new Vector3(0f, -0.04f, 0.06f), new Vector3(1.2f, 0.16f, 0.14f), EnemySwatch.Steel, Quaternion.Euler(20f, 0f, 0f)));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The brake levers, that nobody pulls any more.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.7f * side, 0.2f, 0.3f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.Copper, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.08f * side, -0.55f, 0.32f), 0.05f, 0.04f, 5, EnemySwatch.Iron);
                    b.Box(new Vector3(0.08f * side, -0.6f, 0.34f), new Vector3(0.1f, 0.1f, 0.22f), EnemySwatch.BarkDark);
                }, meshKey: right ? "RunawayCartLever_R" : "RunawayCartLever_L");
            }

            HinokuraBeastRecipes.AddFourLegs(bp, 1.05f, 0.48f, 0.62f, -0.62f, 0.12f, EnemySwatch.Iron, EnemySwatch.Steel, "RunawayCart");
        }

        // ------------------------------------------------------------------
        // O Grande Almas (38)
        // ------------------------------------------------------------------

        public static void BuildGreatAlmas(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.75f, 0f), b =>
            {
                // The biggest of the wild men of the snow, very shy, who hid the sparks of the glacier and sat on them.
                b.Blob(Vector3.zero, new Vector3(1.3f, 1.5f, 1f), EnemySwatch.WolfFur);
                b.Blob(new Vector3(0f, 0.42f, -0.05f), new Vector3(1.38f, 0.66f, 0.92f), EnemySwatch.WolfFur, coarse: true);
                b.Blob(new Vector3(0f, -0.12f, 0.32f), new Vector3(0.8f, 0.9f, 0.4f), EnemySwatch.WolfBelly, coarse: true);
                // Long shaggy hair down the sides and the back.
                for (int i = 0; i < 7; i++)
                {
                    float a = Mathf.Lerp(30f, 330f, i / 6f) * Mathf.Deg2Rad;
                    b.Box(new Vector3(Mathf.Sin(a) * 0.6f, -0.62f, Mathf.Cos(a) * -0.42f), new Vector3(0.16f, 0.46f, 0.12f), i % 2 == 0 ? EnemySwatch.WolfFurDark : EnemySwatch.WolfFur,
                        Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
                }

                // Snow on the shoulders, and the sparks frozen in it.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.44f * s, 0.72f, -0.05f), new Vector3(0.46f, 0.18f, 0.5f), EnemySwatch.GolemSnow, coarse: true);
                    b.Crystal(new Vector3(0.44f * s, 0.78f, 0.05f), 0.06f, 0.2f, EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, -14f * s));
                }
            });
            // The spark he holds tight in the fur of his chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.08f, 0.52f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.14f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.86f, 0.1f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.72f, 0.72f, 0.66f), EnemySwatch.WolfFur, coarse: true);
                b.Blob(new Vector3(0f, -0.04f, 0.26f), new Vector3(0.44f, 0.42f, 0.2f), EnemySwatch.WolfBelly, coarse: true);
                // Small shy eyes under a heavy brow.
                b.Box(new Vector3(0f, 0.1f, 0.33f), new Vector3(0.44f, 0.07f, 0.08f), EnemySwatch.WolfFurDark);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.03f, 0.35f, 0.07f, EnemySwatch.SoftEye);
                b.Blob(new Vector3(0f, 0.32f, -0.04f), new Vector3(0.48f, 0.18f, 0.46f), EnemySwatch.GolemSnow, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.22f, 0.24f), b => b.Blob(new Vector3(0f, -0.02f, 0.02f), new Vector3(0.34f, 0.12f, 0.16f), EnemySwatch.WolfFurDark, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.7f * side, 0.42f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.44f, 0.44f, 0.44f), EnemySwatch.WolfFur, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.14f * side, -0.78f, 0.08f), 0.2f, 0.17f, 6, EnemySwatch.WolfFur);
                    b.Segment(new Vector3(0.14f * side, -0.78f, 0.08f), new Vector3(0.18f * side, -1.28f, 0.2f), 0.17f, 0.14f, 6, EnemySwatch.WolfFurDark);
                    b.Blob(new Vector3(0.18f * side, -1.36f, 0.22f), new Vector3(0.3f, 0.28f, 0.3f), EnemySwatch.WolfFurDark, coarse: true);
                    if (right)
                    {
                        // The snowball, ready to throw.
                        b.Blob(new Vector3(0.18f * side, -1.42f, 0.42f), new Vector3(0.38f, 0.38f, 0.38f), EnemySwatch.GolemSnow, coarse: true);
                    }
                }, Quaternion.Euler(-6f, 0f, 0f), meshKey: right ? "GreatAlmasArm_R" : "GreatAlmasArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 1f, 0.34f, 0.2f, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, EnemySwatch.Leather, "GreatAlmas");
        }

        // ------------------------------------------------------------------
        // A Carpa que Não Pulou (43)
        // ------------------------------------------------------------------

        public static void BuildUnleapedCarp(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // The carp that swam the whole canal and was afraid at the foot of the falls. Its gold has gone dull in the dark water.
                b.Blob(Vector3.zero, new Vector3(1f, 1.6f, 2f), EnemySwatch.Copper);
                b.Blob(new Vector3(0f, -0.36f, 0.1f), new Vector3(0.86f, 0.9f, 1.6f), EnemySwatch.SandstoneDark, coarse: true);
                // Rows of dull scales, a few still golden.
                for (int row = 0; row < 3; row++)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        for (int s = -1; s <= 1; s += 2)
                        {
                            b.Box(new Vector3(0.47f * s, 0.32f - row * 0.3f, 0.55f - i * 0.38f), new Vector3(0.04f, 0.16f, 0.18f),
                                (row + i) % 3 == 0 ? EnemySwatch.Gold : EnemySwatch.CopperDark, Quaternion.Euler(0f, 0f, 8f * s));
                        }
                    }
                }

                // The dorsal fin along its back.
                b.Box(new Vector3(0f, 0.86f, -0.2f), new Vector3(0.06f, 0.5f, 1.15f), EnemySwatch.Shadow, Quaternion.Euler(-8f, 0f, 0f));
                // The fins it stands on at the canal's edge.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.28f * s, -1.12f, -0.35f), new Vector3(0.08f, 0.66f, 0.4f), EnemySwatch.CopperDark, Quaternion.Euler(0f, 0f, 10f * s));
                }

                b.Box(new Vector3(0f, -1.06f, -0.75f), new Vector3(0.08f, 0.6f, 0.3f), EnemySwatch.Shadow);
            });
            // The pearl of the dragon it never became, under its chin.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.42f, 0.92f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.3f), EnemySwatch.MoonEye, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.05f, 0.95f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.84f, 1f, 0.8f), EnemySwatch.Copper, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.34f, 0.16f, 0.12f, 0.18f, EnemySwatch.SoftEye);
                // The long barbels of a carp.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.14f * s, -0.18f, 0.32f), 0.03f, 0.32f, 3, 18f, EnemySwatch.Gold, Quaternion.Euler(130f, 0f, -20f * s));
                }
            });
            // The round mouth, that opens on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.2f, 0.3f), b =>
                b.Cylinder(Vector3.zero, 0.2f, 0.16f, 0.14f, 8, EnemySwatch.CopperDark, EnemySwatch.Black, Quaternion.Euler(90f, 0f, 0f)));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The pectoral fins, like arms: they beat on the windup before it leaps.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.45f * side, -0.42f, 0.55f), b =>
                    b.Box(new Vector3(0.22f * side, -0.42f, -0.05f), new Vector3(0.06f, 0.88f, 0.42f), EnemySwatch.CopperDark, Quaternion.Euler(0f, 0f, 24f * side)),
                    meshKey: right ? "UnleapedCarpFin_R" : "UnleapedCarpFin_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.05f, -0.9f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.32f), 0.28f, 0.16f, 7, EnemySwatch.Copper);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0f, 0.28f * s, -0.45f), new Vector3(0.05f, 0.62f, 0.36f), EnemySwatch.Shadow, Quaternion.Euler(30f * s, 0f, 0f));
                }
            });
        }
    }
}
