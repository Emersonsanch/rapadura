using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// Five map bosses of the Rio das Doze Horas and the Margens da Mistura (Docs/Mapas100.md 72, 75, 78, 80, 81), built like the rest of
    /// <see cref="MapBossRecipes"/>: pivot at the feet, facing +Z, the glowing <c>Core</c> the rig feeds with the fight's phase, and soft eyes, because each
    /// is a creature gone dark, not a villain.
    /// <list type="bullet">
    /// <item><b>O Hipopótamo-de-Junco Gigante</b> (72, ~2.8 m): a huge river hippo caked in canal mud, reeds growing out of his back; four heavy legs, <c>Tail</c>.</item>
    /// <item><b>O Guardião do Lago</b> (75, ~3 m): a giant ushabti of blue faience, mummy-shaped, arms crossed on the chest with two hoes, glowing glyphs; no legs.</item>
    /// <item><b>O Instante Perdido</b> (78, ~3.3 m): an hourglass of sand in a bronze frame under a broken sundial, a shadow face on top, two clock hands for arms; no legs.</item>
    /// <item><b>Hobabish, o Guardião do Cedro</b> (80, ~3.5 m): a giant of cedar bark with a mane of cedar boughs and seven dimmed mantles round his shoulders; two legs.</item>
    /// <item><b>Ullikummi, o Que Cresce</b> (81, ~3.8 m): a column of speckled diorite rooted in the shoulder, crystals growing out of it, slab arms; no legs.</item>
    /// </list>
    /// </summary>
    public static partial class MapBossRecipes
    {
        // ------------------------------------------------------------------
        // O Hipopótamo-de-Junco Gigante (72)
        // ------------------------------------------------------------------

        public static void BuildReedHippo(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.5f, 0f), b =>
            {
                // The gardener of the Reed Fields who went dark and tramples the canals he kept: a barrel of a hippo, caked in canal mud.
                b.Blob(Vector3.zero, new Vector3(1.9f, 1.5f, 2.5f), EnemySwatch.CaveSkin);
                b.Blob(new Vector3(0f, -0.4f, 0.05f), new Vector3(1.6f, 0.75f, 2.1f), EnemySwatch.CaveBelly, coarse: true);
                // Mud slapped on his flanks and back.
                for (int i = 0; i < 6; i++)
                {
                    float side = i % 2 == 0 ? 1f : -1f;
                    b.Blob(new Vector3(0.62f * side, 0.2f - 0.1f * (i % 3), 0.7f - i * 0.28f), new Vector3(0.5f, 0.42f, 0.55f), EnemySwatch.BarkDark, coarse: true);
                }

                // The reeds that took root in the mud on his back, gold and green.
                for (int i = 0; i < 9; i++)
                {
                    float x = (i % 3 - 1) * 0.26f;
                    float z = 0.5f - (i / 3) * 0.45f;
                    b.Horn(new Vector3(x, 0.6f, z), 0.04f, 0.62f + 0.06f * (i % 2), 3, -6f, i % 2 == 0 ? EnemySwatch.Lichen : EnemySwatch.Gold,
                        Quaternion.Euler(-12f + 6f * (i % 3), 0f, -x * 30f));
                }
            });
            // The calm of the fields he used to keep, a soft light under his chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.2f, 1.12f), b => b.Blob(Vector3.zero, new Vector3(0.42f, 0.42f, 0.2f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 1.2f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(1.05f, 0.8f, 0.8f), EnemySwatch.CaveSkin, coarse: true);
                // The broad muzzle, the little ears and the eyes set high, as a hippo's are, to look over the water.
                b.Blob(new Vector3(0f, -0.08f, 0.32f), new Vector3(0.95f, 0.55f, 0.4f), EnemySwatch.CaveBelly, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.26f, 0.32f, 0.12f, 0.12f, EnemySwatch.SoftEye);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.36f * s, 0.4f, -0.12f), new Vector3(0.14f, 0.18f, 0.08f), EnemySwatch.CaveSkinDark, coarse: true);
                    b.Blob(new Vector3(0.2f * s, 0.1f, 0.5f), new Vector3(0.1f, 0.08f, 0.06f), EnemySwatch.Black, coarse: true);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.3f, 0.2f), b =>
            {
                // The great jaw that opens on the windup, two tusks at the front.
                b.Blob(new Vector3(0f, 0f, 0.05f), new Vector3(0.9f, 0.28f, 0.6f), EnemySwatch.Pink, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.3f * s, 0.08f, 0.28f), 0.05f, 0.2f, 2, 0f, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -1.22f), b => b.Segment(Vector3.zero, new Vector3(0f, -0.25f, -0.2f), 0.08f, 0.04f, 5, EnemySwatch.CaveSkinDark));

            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.85f, 0.6f, 0.75f, -0.75f, 0.24f, EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark, "ReedHippo");
        }

        // ------------------------------------------------------------------
        // O Guardião do Lago (75)
        // ------------------------------------------------------------------

        public static void BuildLakeGuardian(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // A giant ushabti of blue faience, the stone servant that answered when called and let the lake's light pass: mummy-shaped, feet together on a plinth.
                b.Cylinder(Vector3.zero, 0.85f, 0.85f, 0.18f, 10, EnemySwatch.GolemStoneDark);
                b.Cylinder(new Vector3(0f, 0.18f, 0f), 0.62f, 0.58f, 1.3f, 10, EnemySwatch.Verdigris);
                b.Cylinder(new Vector3(0f, 1.48f, 0f), 0.58f, 0.72f, 0.82f, 10, EnemySwatch.Verdigris);
                // The column of glyphs down his front, still glowing where the light got in.
                for (int i = 0; i < 5; i++)
                {
                    b.Box(new Vector3(0f, 0.4f + i * 0.24f, 0.6f - 0.01f * i), new Vector3(0.22f, 0.14f, 0.05f), i % 2 == 0 ? EnemySwatch.Rune : EnemySwatch.Gold);
                }

                // Cracks where the dark fire got in.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.42f * s, 0.9f, 0.4f), new Vector3(0.04f, 0.6f, 0.04f), EnemySwatch.Ember, Quaternion.Euler(0f, 30f * s, 12f * s));
                }
            });
            // The light he stopped letting through, held in his chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.95f, 0.66f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.14f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.6f, 0.04f), b =>
            {
                // The striped headcloth falling to the shoulders, the face calm and dark, eyes that only want to open the way.
                b.Blob(new Vector3(0f, 0.05f, -0.05f), new Vector3(0.95f, 0.75f, 0.7f), EnemySwatch.Black, coarse: true);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, 0.28f - i * 0.15f, -0.05f), new Vector3(0.98f, 0.05f, 0.66f), EnemySwatch.Gold);
                }

                b.Blob(new Vector3(0f, 0f, 0.22f), new Vector3(0.5f, 0.56f, 0.32f), EnemySwatch.Verdigris, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.06f, 0.36f, 0.08f, EnemySwatch.SoftEye);
            });
            // The plaited beard, tipped forward on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.26f, 0.3f), b => b.Box(new Vector3(0f, -0.1f, 0f), new Vector3(0.12f, 0.24f, 0.1f), EnemySwatch.Black));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The arms crossed on his chest, a hoe in each hand: they lift on the windup, and his shadow with them.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.62f * side, 2.18f, 0.1f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(-0.1f * side, -0.42f, 0.42f), 0.15f, 0.13f, 6, EnemySwatch.Verdigris);
                    b.Segment(new Vector3(-0.1f * side, -0.42f, 0.42f), new Vector3(-0.55f * side, -0.25f, 0.56f), 0.13f, 0.11f, 6, EnemySwatch.Verdigris);
                    b.Blob(new Vector3(-0.58f * side, -0.25f, 0.58f), new Vector3(0.2f, 0.2f, 0.2f), EnemySwatch.Verdigris, coarse: true);
                    b.Segment(new Vector3(-0.58f * side, -0.65f, 0.6f), new Vector3(-0.58f * side, 0.25f, 0.6f), 0.035f, 0.035f, 4, EnemySwatch.BarkDark);
                    b.Box(new Vector3(-0.58f * side, 0.3f, 0.66f), new Vector3(0.1f, 0.16f, 0.18f), EnemySwatch.Copper);
                }, meshKey: right ? "LakeGuardianArm_R" : "LakeGuardianArm_L");
            }
        }

        // ------------------------------------------------------------------
        // O Instante Perdido (78)
        // ------------------------------------------------------------------

        public static void BuildLostInstant(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // A moment the shadow stole from the clock: an hourglass of sand in a bronze frame, standing in the sand it spilled.
                b.Cylinder(Vector3.zero, 0.95f, 0.8f, 0.08f, 12, EnemySwatch.SandstoneLight);
                b.Cylinder(new Vector3(0f, 0.08f, 0f), 0.85f, 0.85f, 0.1f, 10, EnemySwatch.CopperDark);
                b.Cylinder(new Vector3(0f, 0.18f, 0f), 0.72f, 0.1f, 1.05f, 10, EnemySwatch.Sandstone);
                b.Cylinder(new Vector3(0f, 1.23f, 0f), 0.1f, 0.72f, 1.05f, 10, EnemySwatch.Crystal);
                b.Cylinder(new Vector3(0f, 2.28f, 0f), 0.85f, 0.85f, 0.1f, 10, EnemySwatch.CopperDark);
                for (int i = 0; i < 3; i++)
                {
                    float a = i * Mathf.PI * 2f / 3f + Mathf.PI;
                    b.Segment(new Vector3(Mathf.Sin(a) * 0.78f, 0.18f, Mathf.Cos(a) * 0.78f), new Vector3(Mathf.Sin(a) * 0.78f, 2.28f, Mathf.Cos(a) * 0.78f), 0.05f, 0.05f, 5, EnemySwatch.Copper);
                }

                // The sundial face on top, its gnomon snapped and leaning.
                b.Cylinder(new Vector3(0f, 2.38f, 0f), 0.92f, 0.88f, 0.06f, 12, EnemySwatch.SandstoneLight);
                for (int i = 0; i < 12; i++)
                {
                    float a = i * Mathf.PI / 6f;
                    b.Box(new Vector3(Mathf.Sin(a) * 0.74f, 2.45f, Mathf.Cos(a) * 0.74f), new Vector3(0.06f, 0.03f, 0.14f), EnemySwatch.Black, Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
                }

                b.Box(new Vector3(0f, 2.82f, -0.38f), new Vector3(0.06f, 0.8f, 0.4f), EnemySwatch.Copper, Quaternion.Euler(-18f, 0f, 8f));
                // Splinters of time hanging round it.
                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI * 0.5f + 0.4f;
                    b.Crystal(new Vector3(Mathf.Sin(a) * 1.05f, 1.1f + 0.3f * (i % 2), Mathf.Cos(a) * 0.9f), 0.06f, 0.3f, EnemySwatch.Crystal, Quaternion.Euler(20f * (i - 1.5f), 0f, 15f));
                }
            });
            // The stolen moment itself, at the neck of the glass where the sand runs: it shines more as it is given back.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 1.23f, 0f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.34f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 2.72f, 0.12f), b =>
            {
                // A shadow face looking out over the dial, the shadow that should have told the hour.
                b.Blob(Vector3.zero, new Vector3(0.62f, 0.56f, 0.5f), EnemySwatch.Shadow, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.04f, 0.2f, 0.09f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.18f, 0.16f), b => b.Blob(Vector3.zero, new Vector3(0.24f, 0.06f, 0.08f), EnemySwatch.Black, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // The two hands of the clock it fell out of, for arms: they swing up on the windup, to strike the hour.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.82f * side, 1.9f, 0.1f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.Gold, coarse: true);
                    b.Box(new Vector3(0.1f * side, -0.45f, 0.08f), new Vector3(0.08f, 0.9f, 0.04f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, 10f * side));
                    b.Crystal(new Vector3(0.18f * side, -0.88f, 0.08f), 0.08f, 0.22f, EnemySwatch.Gold, Quaternion.Euler(180f, 0f, 0f), 4);
                }, meshKey: right ? "LostInstantHand_R" : "LostInstantHand_L");
            }
        }

        // ------------------------------------------------------------------
        // Hobabish, o Guardião do Cedro (80)
        // ------------------------------------------------------------------

        public static void BuildHobabish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.95f, 0f), b =>
            {
                // The guardian of the cedars of the Book of the Giants: a giant grown like a cedar, bark for skin, moss in the cracks.
                b.Blob(Vector3.zero, new Vector3(1.3f, 1.4f, 1f), EnemySwatch.Bark);
                b.Blob(new Vector3(0f, -0.35f, 0.08f), new Vector3(1.05f, 0.75f, 0.85f), EnemySwatch.BarkDark, coarse: true);
                for (int i = 0; i < 5; i++)
                {
                    b.Box(new Vector3(-0.4f + i * 0.2f, -0.05f, 0.43f), new Vector3(0.05f, 0.9f, 0.05f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, (i - 2) * 6f));
                }

                // The seven mantles of brilliance, dimmed, one over another round his shoulders.
                for (int i = 0; i < 7; i++)
                {
                    b.Cylinder(new Vector3(0f, 0.2f + i * 0.07f, -0.04f), 0.74f - i * 0.04f, 0.7f - i * 0.04f, 0.07f, 12, i % 2 == 0 ? EnemySwatch.Gold : EnemySwatch.IvoryDark);
                }
            });
            // The melammu, the brilliance under the dark: it shines through as he calms.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, -0.1f, 0.5f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.16f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.92f, 0.1f), b =>
            {
                // A lion-browed face with coiled lines in the bark, and a mane of cedar boughs.
                b.Blob(Vector3.zero, new Vector3(0.72f, 0.68f, 0.62f), EnemySwatch.Bark, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.14f * s, 0.16f, 0.26f), 0.05f, 0.24f, 3, 40f, EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, -70f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.04f, 0.27f, 0.09f, EnemySwatch.SoftEye);
                for (int i = 0; i < 7; i++)
                {
                    float a = (i - 3) * 26f;
                    b.Horn(new Vector3(0f, 0.12f, -0.1f), 0.07f, 0.48f, 3, -10f, i % 2 == 0 ? EnemySwatch.CaveMoss : EnemySwatch.Lichen, Quaternion.Euler(-35f, 0f, a));
                }
            });
            // A beard of moss that drops open on the windup.
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.26f, 0.2f), b => b.Blob(new Vector3(0f, -0.08f, 0f), new Vector3(0.4f, 0.26f, 0.2f), EnemySwatch.CaveMoss, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Long arms like boughs, the hands heavy as stumps: they lift on the windup, a mantle in them.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.72f * side, 0.4f, 0.05f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.16f * side, -0.6f, 0.12f), 0.19f, 0.15f, 6, EnemySwatch.Bark);
                    b.Segment(new Vector3(0.16f * side, -0.6f, 0.12f), new Vector3(0.12f * side, -1.1f, 0.3f), 0.15f, 0.12f, 6, EnemySwatch.BarkDark);
                    b.Blob(new Vector3(0.12f * side, -1.18f, 0.32f), new Vector3(0.28f, 0.26f, 0.28f), EnemySwatch.Bark, coarse: true);
                    b.Box(new Vector3(0.16f * side, -0.35f, 0.1f), new Vector3(0.05f, 0.3f, 0.05f), EnemySwatch.Gold);
                }, meshKey: right ? "HobabishArm_R" : "HobabishArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 1.25f, 0.36f, 0.2f, EnemySwatch.BarkDark, EnemySwatch.Bark, EnemySwatch.BarkDark, "Hobabish");
        }

        // ------------------------------------------------------------------
        // Ullikummi, o Que Cresce (81)
        // ------------------------------------------------------------------

        public static void BuildUllikummi(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, b =>
            {
                // The stone of diorite set on Upelluri's shoulder, that grows to cover the sky: a column rooted in the shoulder, speckled, cracking as it grows.
                b.Blob(new Vector3(0f, 0.32f, 0f), new Vector3(1.8f, 0.76f, 1.6f), EnemySwatch.GolemStoneDark, coarse: true);
                b.Cylinder(new Vector3(0f, 0.4f, 0f), 0.72f, 0.58f, 2.2f, 8, EnemySwatch.GolemStone);
                b.Blob(new Vector3(0f, 2.62f, 0f), new Vector3(1.3f, 0.9f, 1f), EnemySwatch.GolemStone);
                // The black and white speckles of diorite.
                for (int i = 0; i < 12; i++)
                {
                    float a = i * 2.4f;
                    float y = 0.6f + (i % 6) * 0.33f;
                    float r = 0.66f - (y - 0.4f) * 0.06f;
                    b.Box(new Vector3(Mathf.Sin(a) * r, y, Mathf.Cos(a) * r), new Vector3(0.1f, 0.08f, 0.06f), i % 2 == 0 ? EnemySwatch.Black : EnemySwatch.Bone, Quaternion.Euler(0f, a * Mathf.Rad2Deg, 0f));
                }

                // The new stone pushing out of it, crystals on the shoulders, and the seams where it grows.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Crystal(new Vector3(0.48f * s, 2.85f, -0.1f), 0.14f, 0.55f, EnemySwatch.Crystal, Quaternion.Euler(-10f, 0f, -24f * s));
                    b.Crystal(new Vector3(0.3f * s, 2.95f, -0.3f), 0.1f, 0.4f, EnemySwatch.GolemSnow, Quaternion.Euler(-24f, 0f, -12f * s));
                    b.Box(new Vector3(0.45f * s, 1.4f, 0.4f), new Vector3(0.04f, 0.9f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 30f * s, 8f * s));
                }

                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI * 0.5f + 0.5f;
                    b.Blob(new Vector3(Mathf.Sin(a) * 0.85f, 0.15f, Mathf.Cos(a) * 0.75f), new Vector3(0.45f, 0.3f, 0.45f), EnemySwatch.Lichen, coarse: true);
                }
            });
            // The little light the shadow keeps it growing for, in its chest.
            bp.Mesh(ProceduralEnemyParts.Core, body, new Vector3(0f, 2.15f, 0.58f), b => b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.16f), EnemySwatch.StolenLight, coarse: true));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 3.12f, 0.18f), b =>
            {
                // A blunt face high up, that can't see the ground any more: only the sky it wants to cover.
                b.Blob(Vector3.zero, new Vector3(0.66f, 0.6f, 0.56f), EnemySwatch.GolemStoneDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.04f, 0.24f, 0.09f, EnemySwatch.SoftEye);
                b.Crystal(new Vector3(0f, 0.24f, -0.06f), 0.1f, 0.3f, EnemySwatch.Crystal);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.2f, 0.18f), b => b.Blob(Vector3.zero, new Vector3(0.3f, 0.08f, 0.12f), EnemySwatch.GolemStone, coarse: true));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Arms like slabs that broke off the column: they heave up on the windup, and the shoulder shakes when they come down.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.74f * side, 2.6f, 0.05f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.18f * side, -0.7f, 0.15f), 0.24f, 0.2f, 6, EnemySwatch.GolemStone);
                    b.Segment(new Vector3(0.18f * side, -0.7f, 0.15f), new Vector3(0.12f * side, -1.25f, 0.32f), 0.2f, 0.17f, 6, EnemySwatch.GolemStoneDark);
                    b.Blob(new Vector3(0.12f * side, -1.35f, 0.34f), new Vector3(0.36f, 0.32f, 0.36f), EnemySwatch.GolemStone, coarse: true);
                    b.Box(new Vector3(0.18f * side, -0.5f, 0.2f), new Vector3(0.04f, 0.36f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(14f, 0f, 0f));
                }, meshKey: right ? "UllikummiArm_R" : "UllikummiArm_L");
            }
        }
    }
}
