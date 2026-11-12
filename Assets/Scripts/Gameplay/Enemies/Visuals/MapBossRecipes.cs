using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The map bosses of the fichas (Docs/Mapas100.md 04, 05, 09, 10, 15, 22, 25, 26, 32, 33, 34, 37, 38, 39, 42, 43, 45, 48, 55, 56, 60, 62, 66, 69; <see cref="RegionBossRules.MapSpawns"/>), low-poly from the shared
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
    /// <item><b>A Fiandeira Esquecida</b> (04, ~2.8 m): a great pale spider, a silk cocoon with a spark held to her chest by two feelers (the arms); four walking legs, four more arched to the ground.</item>
    /// <item><b>A Raiz que Bebeu o Poço</b> (09, ~3 m): a vine stump walking on four roots, dry leaves on top, an empty water jar caught in it; two tendrils for arms.</item>
    /// <item><b>O Grande Borrão</b> (15, ~2.9 m): a grey heap of the colours it ate, ochre, red and white running down it, handprints on its palms; no legs.</item>
    /// <item><b>Ninki Nanka</b> (34, ~3.1 m): the shy bog dragoness sitting up, a crest of reeds, forearms folded before her face, her tail on the boardwalk; two legs.</item>
    /// <item><b>Nian</b> (45, ~2.9 m): the beast of the year, one horn, a dark red mane, dull gold scales down its back; four legs and a <c>Tail</c>.</item>
    /// <item><b>O Grifo Velho do Kurgan</b> (48, ~2.8 m): lion behind, old eagle in front, ragged wings folded (the arms), a plaque of Scythian gold on its chest; four legs.</item>
    /// <item><b>O Velho da Névoa</b> (05, ~2.7 m): a laurel lizard as big as a house, dry scales and moss, a mist drop in its throat; four legs and a <c>Tail</c>.</item>
    /// <item><b>O Coral da Duna</b> (10, ~3 m): a pale worm reared up out of its dune, a ring of teeth round the singing mouth; no legs.</item>
    /// <item><b>O Engarrafador Guloso</b> (22, ~2.8 m): a fat kobold hugging a great jug, a rack of bottles on his back; two legs.</item>
    /// <item><b>O Pescador de Luzes</b> (33, ~2.7 m): a catfish standing on its fins, drooping whiskers, a lure of light on a stalk, wisps glowing in its flanks.</item>
    /// <item><b>O Senhor do Cata-Vento</b> (39, ~3.3 m): a walnut and bronze vane on a tripod, four blades for a head, brake levers for arms, a tail vane.</item>
    /// <item><b>O Pequeno Eclipse</b> (42, ~3.5 m): a disc of shadow with a golden corona over a column of dark, a bead of stolen light on its rim; wisps for arms.</item>
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

        // ------------------------------------------------------------------
        // A Fiandeira Esquecida (04)
        // ------------------------------------------------------------------

        public static void BuildForgottenSpinner(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.55f, -0.1f), b =>
            {
                // The spinner of the mulberry grove, gone pale in the dark, who wrapped the spark that fell in her web and forgot her own name.
                b.Blob(new Vector3(0f, 0f, 0.35f), new Vector3(1f, 0.7f, 1f), EnemySwatch.IvoryDark, coarse: true);
                b.Blob(new Vector3(0f, 0.45f, -0.75f), new Vector3(1.45f, 1.35f, 1.6f), EnemySwatch.IvoryDark);
                // The silk bands round the abdomen, and the spinnerets.
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, 1.05f - i * 0.08f, -0.4f - i * 0.28f), new Vector3(0.9f - 0.08f * i, 0.06f, 0.12f), EnemySwatch.Ivory, Quaternion.Euler(-20f - i * 14f, 0f, 0f));
                }

                b.Blob(new Vector3(0f, 0.1f, -1.48f), new Vector3(0.3f, 0.3f, 0.24f), EnemySwatch.Bone, coarse: true);
                // The four middle legs, arched out and down to the ground (the front and hind pairs walk).
                for (int s = -1; s <= 1; s += 2)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        float z = 0.3f - j * 0.45f;
                        float spread = j == 0 ? 0.15f : -0.2f;
                        Vector3 knee = new Vector3(1f * s, 0.55f, z + spread);
                        Vector3 foot = new Vector3(1.32f * s, -1.5f, z + spread * 2f);
                        b.Segment(new Vector3(0.4f * s, 0f, z), knee, 0.09f, 0.07f, 5, EnemySwatch.IvoryDark);
                        b.Segment(knee, foot, 0.07f, 0.03f, 5, EnemySwatch.Ivory);
                    }
                }

                // Threads of silk still hanging off her, and a tuft on top.
                for (int i = -1; i <= 1; i++)
                {
                    b.Box(new Vector3(0.35f * i, -0.25f, -1.1f + 0.1f * i), new Vector3(0.02f, 0.7f, 0.02f), EnemySwatch.Bone);
                }

                b.Blob(new Vector3(0f, 1.18f, -0.7f), new Vector3(0.5f, 0.24f, 0.5f), EnemySwatch.Ivory, coarse: true);
            });
            // The cocoon she spun round the spark that fell in her web, held to her chest: it shines through the silk.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.18f, 0.95f), b => b.Blob(Vector3.zero, new Vector3(0.32f, 0.46f, 0.3f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.18f, 0.82f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.6f, 0.5f, 0.5f), EnemySwatch.IvoryDark, coarse: true);
                // Eight eyes, in rows, soft and frightened.
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.1f, 0.22f, 0.09f, EnemySwatch.SoftEye);
                HinokuraBeastRecipes.AddEyes(b, 0.22f, 0.06f, 0.18f, 0.06f, EnemySwatch.SoftEye);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.2f, 0.18f, 0.05f, EnemySwatch.SoftEye);
                HinokuraBeastRecipes.AddEyes(b, 0.18f, 0.18f, 0.14f, 0.04f, EnemySwatch.SoftEye);
            });
            // The fangs, that open on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.2f, 0.2f), b =>
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.08f * s, 0f, 0f), 0.04f, 0.2f, 3, 20f, EnemySwatch.Fang, Quaternion.Euler(150f, 0f, 0f));
                }
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The two feelers she holds the cocoon with: they lift on the windup, when she ties the thread.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.32f * side, -0.05f, 0.72f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.22f * side, -0.18f, 0.3f), 0.07f, 0.06f, 5, EnemySwatch.IvoryDark);
                    b.Segment(new Vector3(0.22f * side, -0.18f, 0.3f), new Vector3(0.08f * side, -0.32f, 0.42f), 0.06f, 0.03f, 5, EnemySwatch.Ivory);
                    b.Box(new Vector3(0.12f * side, -0.25f, 0.4f), new Vector3(0.02f, 0.02f, 0.3f), EnemySwatch.Bone);
                }, meshKey: right ? "ForgottenSpinnerFeeler_R" : "ForgottenSpinnerFeeler_L");
            }

            HinokuraBeastRecipes.AddFourLegs(bp, 1.15f, 0.55f, 0.6f, -0.5f, 0.1f, EnemySwatch.IvoryDark, EnemySwatch.Ivory, "ForgottenSpinner");
        }

        // ------------------------------------------------------------------
        // A Raiz que Bebeu o Poço (09)
        // ------------------------------------------------------------------

        public static void BuildWellDrinkingRoot(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.55f, 0f), b =>
            {
                // The vine at the end of the karez that drank only what it needed, until the shadow got into its roots and it drank the well dry.
                b.Cylinder(new Vector3(0f, -0.6f, 0f), 0.62f, 0.38f, 1.55f, 9, EnemySwatch.Bark, EnemySwatch.BarkDark, twist: 25f);
                for (int i = 0; i < 5; i++)
                {
                    float a = i * Mathf.PI * 2f / 5f;
                    b.Segment(new Vector3(Mathf.Cos(a) * 0.5f, -0.5f, Mathf.Sin(a) * 0.5f), new Vector3(Mathf.Cos(a + 0.6f) * 0.34f, 0.85f, Mathf.Sin(a + 0.6f) * 0.34f),
                        0.08f, 0.05f, 4, EnemySwatch.BarkDark);
                }

                // The canopy of leaves gone dry, and the last grapes, shrivelled.
                b.Blob(new Vector3(0f, 1.12f, -0.05f), new Vector3(1.5f, 0.62f, 1.3f), EnemySwatch.SandstoneDark, coarse: true);
                b.Blob(new Vector3(0.3f, 1.3f, 0.2f), new Vector3(0.7f, 0.4f, 0.6f), EnemySwatch.Lichen, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.55f * s, 0.78f, 0.35f), new Vector3(0.16f, 0.26f, 0.16f), EnemySwatch.VampWing, coarse: true);
                }

                // The empty water jar of the oasis, caught in its roots.
                b.Cylinder(new Vector3(-0.5f, -0.62f, 0.42f), 0.16f, 0.1f, 0.36f, 7, EnemySwatch.CopperDark, EnemySwatch.Black, Quaternion.Euler(0f, 0f, 25f));
            });
            // The last drop of the well it holds inside: Living Water still, glowing in the heartwood.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.15f, 0.5f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.36f, 0.16f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.62f, 0.36f), b =>
            {
                // A knot in the trunk that became a thirsty face.
                b.Blob(Vector3.zero, new Vector3(0.56f, 0.5f, 0.3f), EnemySwatch.BarkDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.06f, 0.13f, 0.08f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.18f, 0.14f), b => b.Box(new Vector3(0f, -0.02f, 0.02f), new Vector3(0.3f, 0.08f, 0.08f), EnemySwatch.Black));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Two vine tendrils, curling down: they rise on the windup, before the roots come up.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.42f * side, 0.55f, 0.05f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.42f * side, -0.25f, 0.18f), 0.1f, 0.08f, 5, EnemySwatch.Bark);
                    b.Segment(new Vector3(0.42f * side, -0.25f, 0.18f), new Vector3(0.55f * side, -0.75f, 0.3f), 0.08f, 0.05f, 5, EnemySwatch.Bark);
                    b.Segment(new Vector3(0.55f * side, -0.75f, 0.3f), new Vector3(0.4f * side, -0.95f, 0.42f), 0.05f, 0.02f, 4, EnemySwatch.Lichen);
                    b.Blob(new Vector3(0.48f * side, -0.5f, 0.28f), new Vector3(0.22f, 0.12f, 0.2f), EnemySwatch.SandstoneDark, coarse: true);
                }, meshKey: right ? "WellDrinkingRootTendril_R" : "WellDrinkingRootTendril_L");
            }

            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.95f, 0.5f, 0.42f, -0.42f, 0.14f, EnemySwatch.Bark, EnemySwatch.BarkDark, "WellDrinkingRoot");
        }

        // ------------------------------------------------------------------
        // O Grande Borrão (15)
        // ------------------------------------------------------------------

        public static void BuildGreatSmudge(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.2f, 0f), b =>
            {
                // A shadow that came out of the paintings and ate their colours until it was the size of a shelter, all of them gone grey.
                b.Box(new Vector3(0f, -1.15f, 0f), new Vector3(1.9f, 0.1f, 1.7f), EnemySwatch.Shadow);
                b.Blob(new Vector3(0f, -0.4f, 0f), new Vector3(2.1f, 1.6f, 1.8f), EnemySwatch.Shroud, coarse: true);
                b.Blob(new Vector3(0f, 0.55f, -0.05f), new Vector3(1.55f, 1.35f, 1.35f), EnemySwatch.GolemStoneDark);
                // Streaks of the colours it ate running down it: ochre, red and white, the three of the painted walls.
                EnemySwatch[] paint = { EnemySwatch.SandstoneDark, EnemySwatch.WarCape, EnemySwatch.Bone };
                for (int i = 0; i < 9; i++)
                {
                    float a = (i * 40f + 15f) * Mathf.Deg2Rad;
                    Vector3 at = new Vector3(Mathf.Sin(a) * 0.86f, 0.1f - 0.12f * (i % 3), Mathf.Cos(a) * 0.74f);
                    b.Box(at, new Vector3(0.12f, 0.7f, 0.06f), paint[i % 3], Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
                }

                // Handprints of the old painters, stolen with the colour.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.38f * s, 0.85f, 0.62f), new Vector3(0.18f, 0.2f, 0.04f), EnemySwatch.Bone, Quaternion.Euler(-15f, 0f, 12f * s));
                }
            });
            // The colours it still has inside, swirling: they shine brighter the more it is hurt.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.15f, 0.72f), b => b.Blob(Vector3.zero, new Vector3(0.44f, 0.44f, 0.2f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.3f, 0.15f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.8f, 0.62f, 0.72f), EnemySwatch.Shroud, coarse: true);
                b.Box(new Vector3(-0.15f, 0.22f, 0.2f), new Vector3(0.3f, 0.08f, 0.3f), EnemySwatch.SandstoneDark, Quaternion.Euler(0f, 0f, 14f));
                HinokuraBeastRecipes.AddEyes(b, 0.16f, 0.04f, 0.3f, 0.09f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.2f, 0.3f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.12f, 0.1f), EnemySwatch.Black, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Two arms of running paint, a white handprint on each palm: they rise on the windup, before the smear.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.85f * side, 0.55f, 0.1f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.3f * side, -0.55f, 0.15f), 0.24f, 0.18f, 6, EnemySwatch.GolemStoneDark);
                    b.Segment(new Vector3(0.3f * side, -0.55f, 0.15f), new Vector3(0.36f * side, -1f, 0.3f), 0.18f, 0.14f, 6, EnemySwatch.Shroud);
                    b.Blob(new Vector3(0.36f * side, -1.1f, 0.32f), new Vector3(0.32f, 0.26f, 0.3f), EnemySwatch.Shroud, coarse: true);
                    b.Box(new Vector3(0.36f * side, -1.12f, 0.48f), new Vector3(0.16f, 0.16f, 0.03f), EnemySwatch.Bone);
                }, meshKey: right ? "GreatSmudgeArm_R" : "GreatSmudgeArm_L");
            }
        }

        // ------------------------------------------------------------------
        // Ninki Nanka (34)
        // ------------------------------------------------------------------

        public static void BuildNinkiNanka(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.5f, 0f), b =>
            {
                // The dragoness of the bog sitting up in the reeds, so shy that being looked at hurts her, and whoever stares feels it.
                b.Blob(Vector3.zero, new Vector3(1.1f, 1.5f, 1f), EnemySwatch.CaveMoss);
                b.Blob(new Vector3(0f, -0.15f, 0.3f), new Vector3(0.78f, 1.1f, 0.5f), EnemySwatch.Lichen, coarse: true);
                // Bog mud on her flanks and a ridge of soft spines down the back.
                b.Blob(new Vector3(0f, -0.55f, -0.1f), new Vector3(1.05f, 0.5f, 0.9f), EnemySwatch.BarkDark, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    b.Crystal(new Vector3(0f, 0.6f - i * 0.28f, -0.44f - 0.03f * i), 0.06f, 0.22f, EnemySwatch.Verdigris, Quaternion.Euler(-70f, 0f, 0f));
                }
            });
            // The light in her chest that she hides from everyone.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.1f, 0.5f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.36f, 0.16f), EnemySwatch.MoonEye, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.95f, 0.22f), b =>
            {
                b.Blob(new Vector3(0f, 0.05f, 0f), new Vector3(0.55f, 0.5f, 0.55f), EnemySwatch.CaveMoss, coarse: true);
                b.Blob(new Vector3(0f, -0.04f, 0.36f), new Vector3(0.36f, 0.26f, 0.42f), EnemySwatch.CaveMoss, coarse: true);
                // Eyes cast down.
                HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.08f, 0.24f, 0.07f, EnemySwatch.SoftEye);
                // A crest of reeds, the colour of the bog's junco.
                for (int i = -2; i <= 2; i++)
                {
                    b.Horn(new Vector3(i * 0.07f, 0.26f, -0.1f), 0.03f, 0.42f - 0.05f * Mathf.Abs(i), 3, -10f, EnemySwatch.Sandstone, Quaternion.Euler(-20f, i * 10f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.15f, 0.36f), b => b.Blob(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.3f, 0.1f, 0.34f), EnemySwatch.Lichen, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The forearms, folded up in front of her face: she hides behind them, and lifts them to shine.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.5f * side, 0.42f, 0.25f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.12f * side, -0.45f, 0.28f), 0.13f, 0.1f, 6, EnemySwatch.CaveMoss);
                    b.Segment(new Vector3(0.12f * side, -0.45f, 0.28f), new Vector3(-0.12f * side, -0.05f, 0.45f), 0.1f, 0.08f, 6, EnemySwatch.CaveMoss);
                    b.Blob(new Vector3(-0.15f * side, 0f, 0.48f), new Vector3(0.18f, 0.14f, 0.16f), EnemySwatch.Verdigris, coarse: true);
                }, meshKey: right ? "NinkiNankaArm_R" : "NinkiNankaArm_L");
            }

            // The long tail lying on the boardwalk behind her.
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.85f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.5f, -0.55f), 0.28f, 0.18f, 6, EnemySwatch.CaveMoss);
                b.Segment(new Vector3(0f, -0.5f, -0.55f), new Vector3(0.18f, -0.55f, -1.25f), 0.18f, 0.08f, 6, EnemySwatch.BarkDark);
                b.Crystal(new Vector3(0.2f, -0.5f, -1.28f), 0.05f, 0.16f, EnemySwatch.Verdigris);
            });

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.85f, 0.36f, 0.16f, EnemySwatch.CaveMoss, EnemySwatch.Verdigris, EnemySwatch.BarkDark, "NinkiNanka");
        }

        // ------------------------------------------------------------------
        // Nian (45)
        // ------------------------------------------------------------------

        public static void BuildNian(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // The beast that comes down from the peaks at the turn of the year, gone dark and forgetting it is afraid of red and of noise.
                b.Blob(Vector3.zero, new Vector3(1.1f, 1f, 1.9f), EnemySwatch.VampFur);
                b.Blob(new Vector3(0f, 0.12f, 0.55f), new Vector3(1.15f, 1f, 0.9f), EnemySwatch.VampFur, coarse: true);
                b.Blob(new Vector3(0f, -0.32f, 0.1f), new Vector3(0.82f, 0.4f, 1.4f), EnemySwatch.WolfBelly, coarse: true);
                // Old scales of gold down its back, gone dull.
                for (int i = 0; i < 5; i++)
                {
                    b.Box(new Vector3(0f, 0.5f - i * 0.03f, 0.45f - i * 0.3f), new Vector3(0.26f, 0.06f, 0.22f), i % 2 == 0 ? EnemySwatch.Gold : EnemySwatch.CopperDark);
                }
            });
            // The ember of the old year it swallowed, glowing in its chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.05f, 0.95f), b => b.Blob(Vector3.zero, new Vector3(0.36f, 0.38f, 0.16f), EnemySwatch.Ember, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 1.05f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.78f, 0.68f, 0.72f), EnemySwatch.VampFur, coarse: true);
                b.Blob(new Vector3(0f, -0.1f, 0.34f), new Vector3(0.5f, 0.34f, 0.3f), EnemySwatch.WolfBelly, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.2f, 0.12f, 0.3f, 0.11f, EnemySwatch.WarEye);
                // The single horn of the nian, and its red mane gone dark.
                b.Horn(new Vector3(0f, 0.3f, 0.05f), 0.09f, 0.62f, 4, -12f, EnemySwatch.Horn, Quaternion.Euler(-15f, 0f, 0f));
                for (int i = 0; i < 7; i++)
                {
                    float a = Mathf.Lerp(-110f, 110f, i / 6f) * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Sin(a) * 0.42f, Mathf.Cos(a) * 0.34f, -0.18f), new Vector3(0.24f, 0.3f, 0.2f), i % 2 == 0 ? EnemySwatch.WarCape : EnemySwatch.VampWing, coarse: true);
                }
            });
            // The wide jaw with its fangs, that drops on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.26f, 0.3f), b =>
            {
                b.Blob(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.48f, 0.16f, 0.34f), EnemySwatch.VampWing, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Crystal(new Vector3(0.14f * s, 0.04f, 0.18f), 0.03f, 0.12f, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.2f, -0.95f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.35f, -0.35f), 0.1f, 0.07f, 5, EnemySwatch.VampFur);
                b.Blob(new Vector3(0f, 0.45f, -0.45f), new Vector3(0.3f, 0.34f, 0.3f), EnemySwatch.WarCape, coarse: true);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 1f, 0.42f, 0.6f, -0.6f, 0.17f, EnemySwatch.VampFur, EnemySwatch.Black, "Nian");
        }

        // ------------------------------------------------------------------
        // O Grifo Velho do Kurgan (48)
        // ------------------------------------------------------------------

        public static void BuildOldKurganGriffin(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // The griffin that has guarded the gold of the kurgans so long it forgot whose it was: lion behind, old eagle in front.
                b.Blob(new Vector3(0f, -0.05f, -0.25f), new Vector3(1f, 0.95f, 1.5f), EnemySwatch.SandstoneDark);
                b.Blob(new Vector3(0f, 0.12f, 0.5f), new Vector3(1.05f, 1.05f, 0.95f), EnemySwatch.IvoryDark, coarse: true);
                // Ruffled chest feathers, grey with age.
                for (int i = 0; i < 6; i++)
                {
                    b.Box(new Vector3((i % 3 - 1) * 0.22f, 0.2f - (i / 3) * 0.22f, 0.92f), new Vector3(0.2f, 0.18f, 0.05f), i % 2 == 0 ? EnemySwatch.Ivory : EnemySwatch.IvoryDark, Quaternion.Euler(-10f, 0f, 0f));
                }
            });
            // The plaque of Scythian gold on its chest, a stag curled on it: the part of the hoard it never lets go.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.25f, 0.95f), b => b.Box(Vector3.zero, new Vector3(0.34f, 0.26f, 0.05f), EnemySwatch.StolenLight));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.75f, 0.95f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.56f, 0.6f, 0.66f), EnemySwatch.Ivory, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.18f, 0.1f, 0.18f, 0.08f, EnemySwatch.MoonEye);
                // The hooked beak, and the ear tufts of an old griffin.
                b.Crystal(new Vector3(0f, 0.02f, 0.28f), 0.1f, 0.34f, EnemySwatch.Gold, Quaternion.Euler(95f, 0f, 0f));
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.16f * s, 0.26f, -0.1f), 0.05f, 0.32f, 3, -16f, EnemySwatch.IvoryDark, Quaternion.Euler(-25f, 0f, -18f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.12f, 0.24f), b => b.Crystal(Vector3.zero, 0.06f, 0.2f, EnemySwatch.CopperDark, Quaternion.Euler(100f, 0f, 0f)));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The old wings, ragged, folded along its sides: they open on the windup, over the hoard.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.5f * side, 0.4f, 0.35f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.12f * side, 0.45f, -0.6f), 0.12f, 0.09f, 5, EnemySwatch.Copper);
                    b.Segment(new Vector3(0.12f * side, 0.45f, -0.6f), new Vector3(0.08f * side, 0.2f, -1.25f), 0.09f, 0.05f, 5, EnemySwatch.CopperDark);
                    for (int i = 0; i < 5; i++)
                    {
                        float t = i / 4f;
                        Vector3 root = Vector3.Lerp(new Vector3(0.12f * side, 0.3f, -0.1f), new Vector3(0.1f * side, 0.3f, -1.15f), t);
                        b.Box(root + new Vector3(0.03f * side, -0.32f, -0.04f), new Vector3(0.05f, 0.62f - 0.06f * i, 0.24f), i % 2 == 0 ? EnemySwatch.Copper : EnemySwatch.Gold);
                    }
                }, meshKey: right ? "OldKurganGriffinWing_R" : "OldKurganGriffinWing_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.05f, -1f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.6f, -0.3f), 0.07f, 0.05f, 5, EnemySwatch.SandstoneDark);
                b.Blob(new Vector3(0f, -0.66f, -0.32f), new Vector3(0.18f, 0.24f, 0.18f), EnemySwatch.BarkDark, coarse: true);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 1f, 0.4f, 0.62f, -0.6f, 0.15f, EnemySwatch.SandstoneDark, EnemySwatch.Horn, "OldKurganGriffin");
        }

        // ------------------------------------------------------------------
        // O Velho da Névoa (05)
        // ------------------------------------------------------------------

        public static void BuildMistElder(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.3f, 0f), b =>
            {
                // The laurel lizard as big as a house who licked the mist off the leaves for ages, until the shadow dried the drops on his tongue.
                b.Blob(Vector3.zero, new Vector3(1.5f, 1.1f, 1.8f), EnemySwatch.Lichen);
                b.Blob(new Vector3(0f, -0.3f, 0.1f), new Vector3(1.1f, 0.5f, 1.5f), EnemySwatch.SandstoneDark, coarse: true);
                // Dry, cracked scales on his flanks, and the moss of the forest floor growing on his back.
                for (int i = 0; i < 4; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Box(new Vector3(0.7f * s, 0.05f - (i % 2) * 0.18f, 0.55f - i * 0.36f), new Vector3(0.05f, 0.16f, 0.24f),
                            i % 2 == 0 ? EnemySwatch.SandstoneDark : EnemySwatch.CaveMoss, Quaternion.Euler(0f, 0f, 10f * s));
                    }
                }

                b.Blob(new Vector3(0f, 0.48f, -0.1f), new Vector3(1f, 0.3f, 1.2f), EnemySwatch.CaveMoss, coarse: true);
                // The row of soft spines down his back.
                for (int i = 0; i < 5; i++)
                {
                    b.Crystal(new Vector3(0f, 0.5f, 0.6f - i * 0.3f), 0.07f, 0.4f - 0.04f * i, EnemySwatch.Verdigris, Quaternion.Euler(-20f, 0f, 0f));
                }

                // The long neck that lifts his head over the ferns.
                b.Segment(new Vector3(0f, 0.2f, 0.65f), new Vector3(0f, 0.65f, 0.9f), 0.4f, 0.32f, 7, EnemySwatch.Lichen);
            });
            // A drop of the mist he still keeps in his throat: it shines the more he is hurt.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.05f, 0.9f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.36f, 0.2f), EnemySwatch.MoonEye, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.8f, 1f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.05f), new Vector3(0.75f, 0.55f, 0.8f), EnemySwatch.Lichen, coarse: true);
                // Heavy brows, old soft eyes, and two frills of moss behind them.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.2f * s, 0.22f, 0.05f), new Vector3(0.25f, 0.15f, 0.35f), EnemySwatch.SandstoneDark, coarse: true);
                    b.Horn(new Vector3(0.15f * s, 0.25f, -0.1f), 0.05f, 0.4f, 3, -12f, EnemySwatch.CaveMoss, Quaternion.Euler(-35f, 0f, -20f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.22f, 0.08f, 0.3f, 0.1f, EnemySwatch.SoftEye);
            });
            // The wide jaw, that drops on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.18f, 0.25f), b => b.Blob(new Vector3(0f, -0.03f, 0.12f), new Vector3(0.6f, 0.16f, 0.5f), EnemySwatch.Lichen, coarse: true));
            // The tail, curling off to one side behind him.
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.15f, -0.85f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.25f, -0.25f, -0.45f), 0.3f, 0.18f, 7, EnemySwatch.Lichen);
                b.Segment(new Vector3(0.25f, -0.25f, -0.45f), new Vector3(-0.05f, -0.4f, -0.8f), 0.18f, 0.06f, 6, EnemySwatch.CaveMoss);
            });

            HinokuraBeastRecipes.AddFourLegs(bp, 0.95f, 0.62f, 0.55f, -0.55f, 0.17f, EnemySwatch.Lichen, EnemySwatch.SandstoneDark, "MistElder");
        }

        // ------------------------------------------------------------------
        // O Coral da Duna (10)
        // ------------------------------------------------------------------

        public static void BuildDuneChoir(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // The worm under the singing sand, reared up out of its dune: it used to call the light home at sunset, and now it calls the shadow.
                b.Blob(new Vector3(0f, 0.28f, 0f), new Vector3(2.3f, 0.56f, 2.3f), EnemySwatch.SandstoneDark, coarse: true);
                // The ripples its song leaves in the sand.
                for (int i = 0; i < 6; i++)
                {
                    float a = i * 60f;
                    float r = Mathf.Deg2Rad * a;
                    b.Box(new Vector3(Mathf.Sin(r) * 0.95f, 0.06f, Mathf.Cos(r) * 0.95f), new Vector3(0.5f, 0.06f, 0.08f), EnemySwatch.Sandstone, Quaternion.Euler(0f, a, 0f));
                }

                // The pale trunk rising out of the dune, ringed like a worm.
                b.Segment(new Vector3(0f, 0.3f, -0.1f), new Vector3(0f, 1.4f, 0.1f), 0.55f, 0.5f, 9, EnemySwatch.IvoryDark);
                b.Segment(new Vector3(0f, 1.4f, 0.1f), new Vector3(0f, 2.25f, 0.45f), 0.5f, 0.44f, 9, EnemySwatch.IvoryDark);
                for (int i = 0; i < 4; i++)
                {
                    float t = i / 3f;
                    Vector3 at = Vector3.Lerp(new Vector3(0f, 0.75f, -0.05f), new Vector3(0f, 1.95f, 0.32f), t);
                    b.Cylinder(at, 0.56f - 0.04f * i, 0.56f - 0.04f * i, 0.08f, 9, EnemySwatch.Ivory);
                }
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.35f, 0.55f), b =>
            {
                b.Blob(new Vector3(0f, 0.05f, 0.05f), new Vector3(1f, 0.85f, 0.8f), EnemySwatch.IvoryDark, coarse: true);
                // The ring of teeth round the singing mouth.
                for (int i = 0; i < 8; i++)
                {
                    float a = i * Mathf.PI * 2f / 8f;
                    b.Crystal(new Vector3(Mathf.Cos(a) * 0.3f, 0.02f + Mathf.Sin(a) * 0.3f, 0.36f), 0.04f, 0.14f, EnemySwatch.Fang, Quaternion.Euler(90f, 0f, 0f));
                }

                // Small soft eyes above the mouth, and a crown of frills that shiver when it sings.
                HinokuraBeastRecipes.AddEyes(b, 0.22f, 0.3f, 0.28f, 0.09f, EnemySwatch.SoftEye);
                for (int i = -2; i <= 2; i++)
                {
                    b.Horn(new Vector3(0.12f * i, 0.36f, -0.1f), 0.04f, 0.3f - 0.03f * Mathf.Abs(i), 3, -10f, EnemySwatch.Sandstone, Quaternion.Euler(-25f, 0f, -12f * i));
                }
            });
            // The light it swallowed in the dark, glowing at the back of its throat.
            bp.Mesh(ProceduralEnemyParts.Core, head, new Vector3(0f, 0f, 0.3f), b => b.Blob(Vector3.zero, new Vector3(0.32f, 0.32f, 0.12f), EnemySwatch.Ember, coarse: true));
            // The lower lip, that drops on the windup, when it sings out.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.28f, 0.3f), b => b.Blob(new Vector3(0f, 0f, 0.05f), new Vector3(0.6f, 0.18f, 0.35f), EnemySwatch.Ivory, coarse: true));
        }

        // ------------------------------------------------------------------
        // O Engarrafador Guloso (22)
        // ------------------------------------------------------------------

        public static void BuildGreedyBottler(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.45f, 0f), b =>
            {
                // The kobold who bottled every breath of Serene Air in the tunnels and hid it for Āz: fat on air he never breathes.
                b.Blob(new Vector3(0f, 0f, 0.05f), new Vector3(1.15f, 1.15f, 1f), EnemySwatch.CopperDark);
                b.Blob(new Vector3(0f, 0.35f, 0f), new Vector3(0.95f, 0.7f, 0.8f), EnemySwatch.Copper, coarse: true);
                b.Box(new Vector3(0f, -0.15f, 0.5f), new Vector3(0.8f, 0.7f, 0.06f), EnemySwatch.Bark);
                // The rack of bottles on his back, three shelves of them.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.45f * s, 0.35f, -0.6f), new Vector3(0.08f, 1.5f, 0.08f), EnemySwatch.Bark);
                }

                for (int shelf = 0; shelf < 3; shelf++)
                {
                    float y = -0.2f + shelf * 0.5f;
                    b.Box(new Vector3(0f, y, -0.6f), new Vector3(1f, 0.06f, 0.3f), EnemySwatch.BarkDark);
                    for (int i = -1; i <= 1; i++)
                    {
                        b.Cylinder(new Vector3(0.28f * i, y + 0.03f, -0.6f), 0.09f, 0.05f, 0.28f, 6, (shelf + i) % 2 == 0 ? EnemySwatch.Verdigris : EnemySwatch.Crystal);
                    }
                }

                // The great jug he hugs to his belly, and its neck.
                b.Cylinder(new Vector3(0f, -0.55f, 0.55f), 0.3f, 0.22f, 0.6f, 9, EnemySwatch.Verdigris, bottomCap: true);
                b.Cylinder(new Vector3(0f, 0.05f, 0.55f), 0.12f, 0.1f, 0.18f, 7, EnemySwatch.Verdigris, EnemySwatch.Bark);
            });
            // The Serene Air shining in the jug's mouth: the more he is hurt, the more of it escapes.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.28f, 0.55f), b => b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.2f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.85f, 0.15f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.05f), new Vector3(0.62f, 0.55f, 0.62f), EnemySwatch.Copper, coarse: true);
                // The snout of a kobold, the swept-back ears and eyes that are more scared than greedy.
                b.Blob(new Vector3(0f, -0.06f, 0.38f), new Vector3(0.36f, 0.28f, 0.4f), EnemySwatch.Copper, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.2f * s, 0.2f, -0.1f), 0.06f, 0.38f, 3, -15f, EnemySwatch.CopperDark, Quaternion.Euler(-20f, 0f, -25f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.14f, 0.1f, 0.25f, 0.08f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.17f, 0.38f), b => b.Box(new Vector3(0f, -0.03f, 0.08f), new Vector3(0.28f, 0.07f, 0.26f), EnemySwatch.CopperDark));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The arms hugging the jug: they lift on the windup, when he uncorks.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.6f * side, 0.45f, 0.05f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.1f * side, -0.45f, 0.3f), 0.13f, 0.11f, 6, EnemySwatch.CopperDark);
                    b.Segment(new Vector3(0.1f * side, -0.45f, 0.3f), new Vector3(-0.12f * side, -0.75f, 0.45f), 0.11f, 0.09f, 6, EnemySwatch.Copper);
                    b.Blob(new Vector3(-0.14f * side, -0.8f, 0.47f), new Vector3(0.2f, 0.18f, 0.2f), EnemySwatch.Copper, coarse: true);
                }, meshKey: right ? "GreedyBottlerArm_R" : "GreedyBottlerArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.8f, 0.28f, 0.14f, EnemySwatch.CopperDark, EnemySwatch.Copper, EnemySwatch.Black, "GreedyBottler");
        }

        // ------------------------------------------------------------------
        // O Pescador de Luzes (33)
        // ------------------------------------------------------------------

        public static void BuildLightFisher(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.5f, 0f), b =>
            {
                // The catfish as big as a boat who swallowed the marsh's will-o'-wisps to have the light all to himself, and went dark inside.
                b.Blob(Vector3.zero, new Vector3(1.3f, 1.6f, 1.9f), EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, -0.3f, 0.15f), new Vector3(1f, 0.9f, 1.4f), EnemySwatch.Bone, coarse: true);
                // The wisps he swallowed, glowing through his flanks.
                for (int i = 0; i < 3; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Blob(new Vector3(0.5f * s, -0.15f + 0.22f * (i % 2), 0.4f - i * 0.4f), new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.FireflyGlow, coarse: true);
                    }
                }

                // The dorsal fin, and the fins he stands on at the edge of the mud bank.
                b.Box(new Vector3(0f, 0.85f, -0.2f), new Vector3(0.06f, 0.45f, 0.8f), EnemySwatch.Shadow, Quaternion.Euler(-8f, 0f, 0f));
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.28f * s, -1.17f, -0.3f), new Vector3(0.08f, 0.66f, 0.4f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, 10f * s));
                }

                b.Box(new Vector3(0f, -1.1f, -0.65f), new Vector3(0.08f, 0.6f, 0.3f), EnemySwatch.Shadow);
            });
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.15f, 0.9f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.05f), new Vector3(1.25f, 0.75f, 0.85f), EnemySwatch.GolemStoneDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.42f, 0.12f, 0.2f, 0.1f, EnemySwatch.SoftEye);
                // The long whiskers, drooping out to the sides.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.4f * s, -0.15f, 0.3f), 0.04f, 0.55f, 4, 10f, EnemySwatch.Bark, Quaternion.Euler(0f, 0f, -120f * s));
                }

                // The stalk of his lure, bending forward over his head.
                b.Segment(new Vector3(0f, 0.35f, 0f), new Vector3(0f, 0.85f, 0.35f), 0.04f, 0.03f, 5, EnemySwatch.Bark);
            });
            // The lure of light at the end of the stalk: the last wisp, the one he fishes with.
            bp.Mesh(ProceduralEnemyParts.Core, head, new Vector3(0f, 0.9f, 0.4f), b => b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), EnemySwatch.FireflyGlow, coarse: true));
            // The wide mouth, that gapes on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.25f, 0.3f), b => b.Box(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.9f, 0.08f, 0.3f), EnemySwatch.Black));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The pectoral fins, like arms: they flap up on the windup, as he casts.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.6f * side, -0.35f, 0.45f), b =>
                    b.Box(new Vector3(0.22f * side, -0.38f, -0.05f), new Vector3(0.06f, 0.8f, 0.42f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, 24f * side)),
                    meshKey: right ? "LightFisherFin_R" : "LightFisherFin_L");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.05f, -0.85f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.3f), 0.28f, 0.16f, 7, EnemySwatch.GolemStoneDark);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0f, 0.25f * s, -0.4f), new Vector3(0.05f, 0.55f, 0.32f), EnemySwatch.Shadow, Quaternion.Euler(30f * s, 0f, 0f));
                }
            });
        }

        // ------------------------------------------------------------------
        // O Senhor do Cata-Vento (39)
        // ------------------------------------------------------------------

        public static void BuildVaneLord(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.2f, 0f), b =>
            {
                // The Artificers' great wind vane of walnut and bronze, that kept the valley's breeze kind until the shadow got into its gears.
                for (int i = 0; i < 3; i++)
                {
                    float a = (60f + i * 120f) * Mathf.Deg2Rad;
                    Vector3 foot = new Vector3(Mathf.Sin(a) * 0.85f, -1.2f, Mathf.Cos(a) * 0.85f);
                    b.Segment(new Vector3(0f, -0.05f, 0f), foot, 0.09f, 0.07f, 6, EnemySwatch.Bark);
                    b.Box(foot + new Vector3(0f, 0.03f, 0f), new Vector3(0.22f, 0.06f, 0.22f), EnemySwatch.Copper);
                }

                // The column, bound with bronze, and the wind bells hanging off it.
                b.Cylinder(new Vector3(0f, -0.1f, 0f), 0.32f, 0.26f, 1.35f, 8, EnemySwatch.Bark, EnemySwatch.BarkDark);
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0f, 0.1f + i * 0.47f, 0f), 0.34f - 0.02f * i, 0.34f - 0.02f * i, 0.06f, 8, EnemySwatch.Copper);
                }

                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.3f * s, 0.9f, -0.15f), 0.07f, 0.03f, 0.16f, 6, EnemySwatch.Gold, rotation: Quaternion.Euler(180f, 0f, 0f));
                }
            });
            // The gear at its heart, lit by the shadow: it spins faster the more it is hurt.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 0.55f, 0.3f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.12f), EnemySwatch.Rune, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.4f, 0.05f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.4f, 0.4f, 0.4f), EnemySwatch.Copper, coarse: true);
                // The four blades of the vane, in a cross.
                for (int k = 0; k < 4; k++)
                {
                    float angle = 45f + 90f * k;
                    float r = angle * Mathf.Deg2Rad;
                    b.Box(new Vector3(-Mathf.Sin(r) * 0.5f, Mathf.Cos(r) * 0.5f, 0.15f), new Vector3(0.22f, 0.85f, 0.04f), k % 2 == 0 ? EnemySwatch.Bark : EnemySwatch.Ivory,
                        Quaternion.Euler(0f, 0f, angle));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.19f, 0.07f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.3f, 0.18f), b => b.Box(Vector3.zero, new Vector3(0.24f, 0.1f, 0.06f), EnemySwatch.Iron));
            // The tail vane behind the head, that tells which way the wind comes from.
            bp.Mesh(ProceduralEnemyParts.Tail, head, new Vector3(0f, 0f, -0.2f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.7f), 0.05f, 0.04f, 5, EnemySwatch.Iron);
                b.Box(new Vector3(0f, 0.05f, -0.85f), new Vector3(0.04f, 0.5f, 0.45f), EnemySwatch.Copper);
            });

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The brake levers on the column: they swing up on the windup, when it turns the wind.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.36f * side, 0.95f, 0.05f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.25f * side, -0.5f, 0.15f), 0.06f, 0.05f, 5, EnemySwatch.Iron);
                    b.Blob(new Vector3(0.27f * side, -0.55f, 0.17f), new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.Copper, coarse: true);
                }, meshKey: right ? "VaneLordLever_R" : "VaneLordLever_L");
            }
        }

        // ------------------------------------------------------------------
        // O Pequeno Eclipse (42)
        // ------------------------------------------------------------------

        public static void BuildLittleEclipse(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 2.2f, 0f), b =>
            {
                // A disc of shadow that stuck itself to the White Spires and eats their light a slice at a time, over a pool and a column of its own dark.
                b.Cylinder(new Vector3(0f, -2.2f, 0f), 0.9f, 0.85f, 0.05f, 12, EnemySwatch.Shadow);
                b.Cylinder(new Vector3(0f, -2.15f, 0f), 0.35f, 0.12f, 1.25f, 8, EnemySwatch.Shroud);
                b.Cylinder(new Vector3(0f, 0f, -0.09f), 1.05f, 1.05f, 0.18f, 16, EnemySwatch.Shadow, EnemySwatch.Black, Quaternion.Euler(90f, 0f, 0f), bottomCap: true);
                // The rim of the light it stole, showing round the edge, and the corona.
                b.Cylinder(new Vector3(0f, 0f, -0.14f), 1.1f, 1.1f, 0.05f, 16, EnemySwatch.Gold, rotation: Quaternion.Euler(90f, 0f, 0f));
                for (int i = 0; i < 12; i++)
                {
                    float angle = i * 30f;
                    float r = angle * Mathf.Deg2Rad;
                    b.Crystal(new Vector3(Mathf.Sin(r), Mathf.Cos(r), 0f) * 1f + new Vector3(0f, 0f, -0.12f), 0.07f, 0.3f - 0.08f * (i % 2), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, -angle));
                }
            });
            // The bead of stolen light on its rim: it grows the more it is hurt.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0.62f, 0.62f, 0.1f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.14f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.05f, 0.12f), b =>
            {
                // A small face in the middle of the dark, soft eyes and no malice.
                HinokuraBeastRecipes.AddEyes(b, 0.28f, 0.12f, 0f, 0.16f, EnemySwatch.SoftEye);
                b.Blob(new Vector3(0f, 0.32f, -0.01f), new Vector3(0.5f, 0.06f, 0.04f), EnemySwatch.Shroud, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.3f, 0.02f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.08f, 0.06f), EnemySwatch.Shroud, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Two wisps of shadow trailing from its sides: they lift on the windup, as it covers the light.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.95f * side, -0.45f, 0f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.25f * side, -0.7f, 0.1f), 0.14f, 0.08f, 6, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.25f * side, -0.7f, 0.1f), new Vector3(0.1f * side, -1.1f, 0.2f), 0.08f, 0.03f, 5, EnemySwatch.Shadow);
                }, meshKey: right ? "LittleEclipseWisp_R" : "LittleEclipseWisp_L");
            }
        }
    }
}
