using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-eighth pass of beasts (bible §3.2.1): the roles of the Planície de Mingcheng (Docs/Mapas100.md 42, 46) and of the Picos de
    /// Farnkesh (56) that the continuous world still played with another beast after the twenty-seventh pass (<see cref="WestOwn2BeastRecipes"/>),
    /// low-poly from the shared palette. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Eco Apagado</b> (56, ~1.2 m): an Answer that lost its Call, a hood of mist with an open mouth and rings of sound in front of it. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Sino Rachado</b> (56, ~1.3 m): a cracked bronze bell of the hermitages, adrift on its frayed rope, the clapper glowing. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Servo de Taotie</b> (46, ~1.3 m): a bronze ding vessel of the old rites walking on its three legs, the taotie mask on its belly. <c>Body</c> → <c>Head</c> (the lid), two arms (the handles).</item>
    /// <item><b>Filhote de Qiongqi</b> (46, ~1 m): the cub of the winged tiger of the Shanhai jing, hedgehog quills down its back. Jaw, tail, four legs.</item>
    /// <item><b>Guardião de Cal</b> (42, ~1.6 m): a lime-plastered guard of Amanirenas's white spires, with the bow of Ta-Seti. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// </list>
    /// </summary>
    public static class PeaksOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Eco Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedEcho(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.78f, 0f), DrawDimmedEchoBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0.18f), b =>
            {
                // The last sound it heard, kept in its mouth: it flares before the Echo gives it back.
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.12f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawDimmedEchoBody(EnemyMeshBuilder b)
        {
            // The Call goes out and the Answer comes back: that is how the Light finds its sparks. On the cliff of the hermitages the shadow
            // caught the answers before the calls, and now they answer anything, the wrong way round: run, and they run at you.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.56f, 0.46f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0f, 0.16f, -0.04f), new Vector3(0.42f, 0.36f, 0.4f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(0f, 0.3f, -0.1f), new Vector3(0.26f, 0.2f, 0.26f), EnemySwatch.Shroud, coarse: true);
            // The open mouth that only repeats, and the two pale eyes over it.
            b.Blob(new Vector3(0f, 0.03f, 0.2f), new Vector3(0.24f, 0.26f, 0.08f), EnemySwatch.Black, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.2f, 0.19f, 0.04f, EnemySwatch.Sorcery);
            // The rings of sound going out from the mouth, wider and fainter, like the ripples of a bell.
            for (int i = 0; i < 3; i++)
            {
                float r = 0.18f + i * 0.08f;
                for (int k = -2; k <= 2; k++)
                {
                    float a = k * 0.5f;
                    b.Blob(new Vector3(Mathf.Sin(a) * r, 0.04f + Mathf.Cos(a) * r * 0.4f - r * 0.4f, 0.28f + i * 0.08f), new Vector3(0.06f, 0.06f, 0.03f),
                        i == 0 ? EnemySwatch.Sorcery : EnemySwatch.Shroud, coarse: true);
                }
            }

            // The trail of mist it drags down the cliff, thinning to the floor: the lowest point of the model.
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0.03f * (i % 2 == 0 ? 1f : -1f), -0.3f - i * 0.12f, -0.02f), new Vector3(0.3f - i * 0.05f, 0.14f, 0.28f - i * 0.04f), EnemySwatch.Shroud, coarse: true);
            }

            b.Blob(new Vector3(0f, -0.74f, 0f), new Vector3(0.3f, 0.05f, 0.26f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Sino Rachado
        // ------------------------------------------------------------------

        public static void BuildCrackedBell(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.9f, 0f), DrawCrackedBellBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.5f, 0f), b =>
            {
                // The clapper, gone to a ball of cold light: it swells before the bell throws its note.
                b.Blob(Vector3.zero, new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.07f, 0.07f, 0.06f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawCrackedBellBody(EnemyMeshBuilder b)
        {
            // The hermits of the cliff hung a bell at every hermitage, and rang it once at dusk so the others knew they were well. When the
            // great bell would not stop, the little ones cracked trying to answer it. The shadow got in at the crack, and now they ring at you.
            b.Cylinder(new Vector3(0f, -0.26f, 0f), 0.32f, 0.2f, 0.42f, 10, EnemySwatch.Copper, bottomCap: false);
            b.Cylinder(new Vector3(0f, -0.3f, 0f), 0.34f, 0.33f, 0.05f, 10, EnemySwatch.CopperDark);
            b.Cylinder(new Vector3(0f, 0.16f, 0f), 0.2f, 0.1f, 0.1f, 10, EnemySwatch.Copper, bottomCap: true);
            // The verdigris of the high damp, the band of little suns cast round the waist, and the crack the shadow came in by.
            b.Cylinder(new Vector3(0f, -0.12f, 0f), 0.285f, 0.27f, 0.05f, 10, EnemySwatch.Verdigris);
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 18f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.285f, -0.04f, Mathf.Sin(a) * 0.285f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Gold, coarse: true);
            }

            b.Box(new Vector3(0.12f, -0.12f, 0.26f), new Vector3(0.025f, 0.34f, 0.025f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, -24f, 14f));
            b.Box(new Vector3(0.16f, -0.24f, 0.23f), new Vector3(0.02f, 0.14f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, -30f, -30f));
            b.Blob(new Vector3(0.12f, 0f, 0.22f), new Vector3(0.14f, 0.2f, 0.08f), EnemySwatch.Shadow, coarse: true);
            // The crown loop and the frayed rope it broke from, standing up off the top.
            b.Segment(new Vector3(-0.06f, 0.26f, 0f), new Vector3(0f, 0.36f, 0f), 0.03f, 0.03f, 5, EnemySwatch.CopperDark);
            b.Segment(new Vector3(0f, 0.36f, 0f), new Vector3(0.06f, 0.26f, 0f), 0.03f, 0.03f, 5, EnemySwatch.CopperDark);
            b.Segment(new Vector3(0f, 0.36f, 0f), new Vector3(0.04f, 0.5f, -0.02f), 0.025f, 0.012f, 4, EnemySwatch.Bark);
            // The iron rod down to the clapper, and the end of the bell-rope hanging under it: the lowest point of the model.
            b.Segment(new Vector3(0f, 0.1f, 0f), new Vector3(0f, -0.44f, 0f), 0.018f, 0.018f, 4, EnemySwatch.IronDark);
            b.Segment(new Vector3(0f, -0.56f, 0f), new Vector3(0.03f, -0.86f, 0.02f), 0.02f, 0.012f, 4, EnemySwatch.Bark);
        }

        // ------------------------------------------------------------------
        // Servo de Taotie
        // ------------------------------------------------------------------

        public static void BuildTaotieServant(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawTaotieServantBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.36f, 0f), b =>
            {
                // The lid of the vessel, with the knob the priests lifted it by, gone green.
                b.Cylinder(new Vector3(0f, -0.02f, 0f), 0.34f, 0.26f, 0.08f, 10, EnemySwatch.CopperDark, bottomCap: true);
                b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.36f, 0.12f, 0.36f), EnemySwatch.Verdigris, coarse: true);
                b.Cylinder(new Vector3(0f, 0.12f, 0f), 0.05f, 0.07f, 0.08f, 6, EnemySwatch.Copper, bottomCap: true);
                b.Blob(new Vector3(0f, 0.22f, 0f), new Vector3(0.1f, 0.06f, 0.1f), EnemySwatch.Ember, coarse: true);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.34f * side, 0.26f, 0f), b =>
                {
                    // The two loop handles on the rim, grown into arms that reach for whatever comes near: the vessel still wants to be filled.
                    b.Segment(Vector3.zero, new Vector3(0.1f * side, 0.12f, 0f), 0.04f, 0.04f, 5, EnemySwatch.Copper);
                    b.Segment(new Vector3(0.1f * side, 0.12f, 0f), new Vector3(0.12f * side, -0.18f, 0.08f), 0.045f, 0.05f, 5, EnemySwatch.CopperDark);
                    b.Segment(new Vector3(0.12f * side, -0.18f, 0.08f), new Vector3(0.08f * side, -0.42f, 0.18f), 0.05f, 0.045f, 5, EnemySwatch.Copper);
                    b.Blob(new Vector3(0.08f * side, -0.46f, 0.2f), new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.Verdigris, coarse: true);
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: right ? "TaotieServantArm_R" : "TaotieServantArm_L");
            }
        }

        private static void DrawTaotieServantBody(EnemyMeshBuilder b)
        {
            // In the old rites the bronze vessels held the offerings, and the taotie cast on their bellies was a face that ate and was never
            // full. When Taotie woke in the citadel, the vessels got up on their three legs and went to serve it: they gather up what you drop.
            b.Cylinder(new Vector3(0f, -0.2f, 0f), 0.3f, 0.34f, 0.46f, 10, EnemySwatch.Copper, bottomCap: true);
            b.Cylinder(new Vector3(0f, 0.24f, 0f), 0.36f, 0.36f, 0.06f, 10, EnemySwatch.CopperDark);
            b.Cylinder(new Vector3(0f, -0.04f, 0f), 0.325f, 0.33f, 0.06f, 10, EnemySwatch.Verdigris);
            // The taotie mask on the belly: the two big eyes, the curled horns, the flat nose and the jawless mouth of a face that only eats.
            HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.08f, 0.32f, 0.06f, EnemySwatch.Ember);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.12f * s, 0.08f, 0.33f), new Vector3(0.14f, 0.1f, 0.02f), EnemySwatch.Verdigris);
                b.Segment(new Vector3(0.06f * s, 0.16f, 0.33f), new Vector3(0.2f * s, 0.2f, 0.3f), 0.025f, 0.02f, 4, EnemySwatch.CopperDark);
                b.Segment(new Vector3(0.2f * s, 0.2f, 0.3f), new Vector3(0.24f * s, 0.1f, 0.28f), 0.02f, 0.015f, 4, EnemySwatch.CopperDark);
            }

            b.Box(new Vector3(0f, -0.02f, 0.335f), new Vector3(0.06f, 0.14f, 0.03f), EnemySwatch.CopperDark);
            b.Box(new Vector3(0f, -0.14f, 0.335f), new Vector3(0.26f, 0.04f, 0.02f), EnemySwatch.Black);
            b.Blob(new Vector3(-0.16f, 0.02f, 0.26f), new Vector3(0.12f, 0.16f, 0.08f), EnemySwatch.Shadow, coarse: true);
            // The three legs it walks on, cast with little feet like an ox's: the lowest points of the model.
            for (int i = 0; i < 3; i++)
            {
                float a = (i * 120f + 90f) * Mathf.Deg2Rad;
                var top = new Vector3(Mathf.Cos(a) * 0.22f, -0.2f, Mathf.Sin(a) * 0.22f);
                var foot = new Vector3(Mathf.Cos(a) * 0.28f, -0.58f, Mathf.Sin(a) * 0.28f);
                b.Segment(top, foot, 0.07f, 0.05f, 6, EnemySwatch.CopperDark);
                b.Blob(foot, new Vector3(0.1f, 0.06f, 0.12f), EnemySwatch.Copper, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Filhote de Qiongqi
        // ------------------------------------------------------------------

        public static void BuildQiongqiCub(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawQiongqiCubBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.4f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.24f), EnemySwatch.Gold);
                // The round face of a tiger cub, the stripes on the brow, the round ears and the eyes of the one who calls the wrong right.
                b.Blob(new Vector3(0f, -0.05f, 0.12f), new Vector3(0.16f, 0.12f, 0.12f), EnemySwatch.Ivory, coarse: true);
                b.Blob(new Vector3(0f, -0.02f, 0.19f), new Vector3(0.05f, 0.04f, 0.03f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.1f * s, 0.12f, -0.02f), new Vector3(0.07f, 0.07f, 0.04f), EnemySwatch.Gold, coarse: true);
                    b.Box(new Vector3(0.05f * s, 0.1f, 0.06f), new Vector3(0.02f, 0.05f, 0.08f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, 20f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.04f, 0.11f, 0.04f, EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.11f, 0.03f, 0.12f), EnemySwatch.Ivory);
                b.Cylinder(new Vector3(0.03f, 0.006f, 0.1f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.03f, 0.006f, 0.1f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.38f), b =>
            {
                // A tiger's tail, ringed black, the tip curled up.
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.22f), 0.04f, 0.035f, 5, EnemySwatch.Gold);
                b.Segment(new Vector3(0f, -0.06f, -0.22f), new Vector3(0f, 0.08f, -0.36f), 0.035f, 0.025f, 5, EnemySwatch.Black);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.4f, 0.14f, 0.24f, -0.26f, 0.05f, EnemySwatch.Gold, EnemySwatch.Black, "QiongqiCub");
        }

        private static void DrawQiongqiCubBody(EnemyMeshBuilder b)
        {
            // The old book of the mountains and seas draws Qiongqi as a tiger with wings, and says it eats the honest and feeds the quarrelsome.
            // In the citadel its cubs run ahead of it, and where they run the hero's left goes right: they are small, but they pounce.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.34f, 0.72f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.26f, 0.16f, 0.54f), EnemySwatch.Ivory, coarse: true);
            b.Segment(new Vector3(0f, 0.06f, 0.24f), new Vector3(0f, 0.12f, 0.36f), 0.1f, 0.09f, 6, EnemySwatch.Gold);
            // The black stripes round the flanks, and the hedgehog quills the old book gives it standing down the back.
            for (int i = 0; i < 4; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.17f * s, 0.04f, -0.24f + i * 0.15f), new Vector3(0.02f, 0.2f, 0.035f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, -14f * s));
                }

                b.Cylinder(new Vector3(0f, 0.15f, -0.2f + i * 0.12f), 0.025f, 0f, 0.1f, 3, EnemySwatch.BarkDark, rotation: Quaternion.Euler(-30f, 0f, 0f));
            }

            // The little wings at the shoulders, too small yet to lift it, but they spread when it leaps.
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.12f * s, 0.12f, 0.16f), new Vector3(0.36f * s, 0.26f, 0.02f), new Vector3(0.16f * s, 0.12f, -0.06f), EnemySwatch.Ivory, EnemySwatch.Gold);
                b.TwoSided(new Vector3(0.14f * s, 0.13f, 0.06f), new Vector3(0.3f * s, 0.18f, -0.08f), new Vector3(0.16f * s, 0.12f, -0.1f), EnemySwatch.WarCape, EnemySwatch.Ivory);
            }

            b.Blob(new Vector3(0.06f, 0.12f, -0.18f), new Vector3(0.14f, 0.08f, 0.16f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Guardião de Cal
        // ------------------------------------------------------------------

        public static void BuildLimeGuardian(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawLimeGuardianBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.5f, 0.02f), b =>
            {
                // The dark painted face under the plaster, and the tall white crown of the guards of the spires, with the gold band.
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.28f, 0.24f), EnemySwatch.SandstoneDark);
                b.Cylinder(new Vector3(0f, 0.08f, -0.02f), 0.13f, 0.09f, 0.3f, 8, EnemySwatch.GolemSnow, bottomCap: true);
                b.Blob(new Vector3(0f, 0.38f, -0.02f), new Vector3(0.12f, 0.08f, 0.12f), EnemySwatch.GolemSnow, coarse: true);
                b.Cylinder(new Vector3(0f, 0.08f, -0.02f), 0.135f, 0.13f, 0.04f, 8, EnemySwatch.Gold);
                HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.02f, 0.11f, 0.035f, EnemySwatch.Ember);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.25f * side, 0.3f, 0.02f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.04f * side, -0.26f, 0.08f), 0.07f, 0.06f, 6, EnemySwatch.GolemSnow);
                    b.Segment(new Vector3(0.04f * side, -0.26f, 0.08f), new Vector3(0.02f * side, -0.44f, 0.22f), 0.06f, 0.05f, 6, EnemySwatch.Ivory);
                    b.Blob(new Vector3(0.02f * side, -0.48f, 0.24f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.SandstoneDark, coarse: true);
                    if (!right)
                    {
                        // The great bow of Ta-Seti, the Land of the Bow, held upright: the shadow took the arrows and left the string.
                        b.Segment(new Vector3(0.02f, -0.92f, 0.26f), new Vector3(0.08f, -0.48f, 0.3f), 0.022f, 0.02f, 4, EnemySwatch.Bark);
                        b.Segment(new Vector3(0.08f, -0.48f, 0.3f), new Vector3(0.02f, -0.04f, 0.26f), 0.02f, 0.022f, 4, EnemySwatch.Bark);
                        b.Segment(new Vector3(0.02f, -0.92f, 0.26f), new Vector3(0.02f, -0.04f, 0.26f), 0.006f, 0.006f, 3, EnemySwatch.Ivory);
                    }
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: right ? "LimeGuardianArm_R" : "LimeGuardianArm_L");
            }
        }

        private static void DrawLimeGuardianBody(EnemyMeshBuilder b)
        {
            // Amanirenas had the guards of her spires plastered white with lime, so they would shine at the gate like the towers. The plaster
            // cracked in the long sun of the plain, the shadow got in under it, and now they keep the spires from the elves who built them.
            b.Box(new Vector3(0f, -0.64f, 0f), new Vector3(0.6f, 0.12f, 0.5f), EnemySwatch.SandstoneLight, top: EnemySwatch.GolemSnow);
            b.Cylinder(new Vector3(0f, -0.58f, 0f), 0.22f, 0.26f, 0.4f, 8, EnemySwatch.GolemSnow, bottomCap: true);
            b.Blob(new Vector3(0f, 0.16f, 0f), new Vector3(0.46f, 0.6f, 0.34f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, -0.18f, 0f), new Vector3(0.4f, 0.24f, 0.32f), EnemySwatch.Ivory, coarse: true);
            // The red ochre sash, the gold collar, and the cracks in the lime where the shadow went in, glowing.
            b.Box(new Vector3(0f, -0.06f, 0.15f), new Vector3(0.36f, 0.06f, 0.04f), EnemySwatch.WarCape, Quaternion.Euler(0f, 0f, -12f));
            b.Cylinder(new Vector3(0f, 0.38f, 0f), 0.2f, 0.16f, 0.06f, 8, EnemySwatch.Gold);
            b.Box(new Vector3(0.1f, 0.14f, 0.17f), new Vector3(0.02f, 0.26f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 18f));
            b.Box(new Vector3(-0.12f, -0.3f, 0.2f), new Vector3(0.02f, 0.18f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -22f));
            b.Blob(new Vector3(-0.08f, 0.22f, 0.14f), new Vector3(0.14f, 0.18f, 0.08f), EnemySwatch.Shadow, coarse: true);
            // The flakes of plaster fallen at its feet, on the plinth.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(-0.2f + i * 0.2f, -0.57f, 0.18f), new Vector3(0.08f, 0.02f, 0.06f), EnemySwatch.Ivory, Quaternion.Euler(0f, i * 40f, 0f));
            }
        }
    }
}
