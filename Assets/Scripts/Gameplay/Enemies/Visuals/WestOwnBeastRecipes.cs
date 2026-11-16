using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-fourth pass of beasts (bible §3.2.1): the most-seen roles of the Costa das Monções (Docs/Mapas100.md 28–30), the Delta de
    /// Jhilpur (31–35), the Planície de Mingcheng (42–44) and the Estepe dos Nartes (51) that the continuous world still played with another
    /// beast, low-poly from the shared palette like <see cref="NorthOwnBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Tubarão-de-Lodo</b> (28, ~2 m long): a squat shark of the kelp gardens caked in grey mud. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Raia de Fumaça</b> (29, ~1.4 m across): a ray of smoke over the drowned streets of Kilwa, its whip tail on the ground. Bird skeleton (fins as wings).</item>
    /// <item><b>Garça Apagada</b> (30, ~1.4 m): a tall grey heron of the Liyongo canals with the white gone out of its feathers. Bird skeleton.</item>
    /// <item><b>Gavial do Lodo</b> (31, ~3 m): a long-snouted gharial of Jhilpur's lake with the knob on its nose. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Ninki Nanka Filhote</b> (34, ~2.6 m): a young bog dragon, crocodile body, long neck, little horns and a crest of glinting scales. Four legs.</item>
    /// <item><b>Raiz Podre</b> (35, ~1.1 m): a knot of rotten roots held up by its own cloud of spores, a stolen light rotting inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Lanterna-Peixe</b> (43, ~1 m): a red paper carp lantern of the canal with a stolen light inside, its tassel trailing. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Barqueiro de Fumaça</b> (43, ~1.6 m): a boatman of smoke in a straw hat, his punting pole in hand, the smoke of his legs on the water. Arms.</item>
    /// <item><b>Pato-Mandarim Ladrão</b> (44, ~0.8 m): a gaudy mandarin drake with a stolen trinket in its bill. Bird skeleton.</item>
    /// <item><b>Espantalho Apagado</b> (44, ~1.7 m): a straw scarecrow of the mirror paddies with a lantern hung on its crossbar. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Raio Torto</b> (42, ~1.2 m): a bent beam of light caught in a shard of smoked glass, zigzagging down to the ground. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Lobo-Cinzento</b> (51, ~0.9 m): a grey steppe wolf with its fur blown the wrong way by the Destroying Wind. Jaw, tail, four legs.</item>
    /// </list>
    /// </summary>
    public static class WestOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Tubarão-de-Lodo
        // ------------------------------------------------------------------

        public static void BuildMudShark(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.36f, 0f), DrawMudSharkBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.6f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.4f, 0.32f, 0.4f), EnemySwatch.CaveSkinDark);
                // The blunt snout under the mud, and the small pale eyes of a fish that hunts in the murk of the kelp.
                b.Blob(new Vector3(0f, -0.02f, 0.18f), new Vector3(0.3f, 0.2f, 0.26f), EnemySwatch.CaveSkinDark, coarse: true);
                b.Blob(new Vector3(0f, 0.1f, 0.04f), new Vector3(0.24f, 0.08f, 0.24f), EnemySwatch.WolfFurDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.05f, 0.1f, 0.05f, EnemySwatch.MoonEye);
                for (int i = 0; i < 4; i++)
                {
                    b.Cylinder(new Vector3(-0.09f + i * 0.06f, -0.08f, 0.24f), 0.014f, 0f, 0.05f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.06f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.12f), new Vector3(0.26f, 0.05f, 0.24f), EnemySwatch.CaveBelly);
                b.Box(new Vector3(0f, 0.016f, 0.12f), new Vector3(0.18f, 0.01f, 0.18f), EnemySwatch.Pink);
                for (int i = 0; i < 4; i++)
                {
                    b.Cylinder(new Vector3(-0.08f + i * 0.055f, 0.01f, 0.2f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.6f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.02f, -0.3f), 0.13f, 0.06f, 6, EnemySwatch.CaveSkinDark);
                // The tall crescent of the tail, the lower lobe dragging in the silt.
                b.TwoSided(new Vector3(0f, 0.02f, -0.26f), new Vector3(0f, 0.36f, -0.46f), new Vector3(0f, 0.02f, -0.38f), EnemySwatch.CaveSkinDark, EnemySwatch.WolfFurDark);
                b.TwoSided(new Vector3(0f, 0.02f, -0.26f), new Vector3(0f, 0.02f, -0.38f), new Vector3(0f, -0.28f, -0.42f), EnemySwatch.CaveSkinDark, EnemySwatch.WolfFurDark);
            });
        }

        private static void DrawMudSharkBody(EnemyMeshBuilder b)
        {
            // In the kelp gardens of Kirta the sharks were the reef's sweepers, and the divers swam beside them. The Lodo of the Delta came down the coast in the monsoon and the sharks lay in it to cool off; it dried on them grey, and it brought the shadow in with it.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.44f, 1.3f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, -0.1f, 0.06f), new Vector3(0.4f, 0.22f, 1.04f), EnemySwatch.CaveBelly, coarse: true);
            // Cakes of grey mud on the back, gill slits, and a dim light of the reef caught under the dorsal fin.
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(i % 2 == 0 ? 0.1f : -0.1f, 0.2f, -0.36f + i * 0.22f), new Vector3(0.18f, 0.06f, 0.18f), EnemySwatch.WolfFurDark, coarse: true);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0.22f * s, 0.02f, 0.34f - i * 0.06f), new Vector3(0.01f, 0.12f, 0.02f), EnemySwatch.Black);
                }

                // The broad pectoral fins it props itself on in the shallows: the lowest points of the model.
                b.TwoSided(new Vector3(0.16f * s, -0.12f, 0.24f), new Vector3(0.44f * s, -0.32f, -0.02f), new Vector3(0.16f * s, -0.12f, -0.04f), EnemySwatch.CaveSkinDark, EnemySwatch.CaveBelly);
            }

            b.TwoSided(new Vector3(0f, 0.18f, 0.12f), new Vector3(0f, 0.52f, -0.12f), new Vector3(0f, 0.18f, -0.16f), EnemySwatch.CaveSkinDark, EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0f, 0.2f, -0.04f), new Vector3(0.08f, 0.06f, 0.1f), EnemySwatch.StolenLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Raia de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeRay(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawSmokeRayBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0f, 0.28f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.1f, 0.18f), EnemySwatch.Shroud);
                // The two eyes on top that look up from the drowned streets, and the horns of the head fins curled forward.
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.02f, 0.045f, EnemySwatch.MoonEye);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.1f * s, -0.01f, 0.04f), new Vector3(0.12f * s, -0.04f, 0.16f), 0.03f, 0.012f, 4, EnemySwatch.Shadow);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.04f), b =>
                b.Box(new Vector3(0f, -0.005f, 0.04f), new Vector3(0.14f, 0.02f, 0.08f), EnemySwatch.Black));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.18f, 0f, 0.02f), b => DrawSmokeRayFin(b, -1f), meshKey: "SmokeRayFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.18f, 0f, 0.02f), b => DrawSmokeRayFin(b, 1f), meshKey: "SmokeRayFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.28f), b =>
            {
                // The long whip of the tail curling down to the street stones, the sting at the end: the lowest point of the model.
                b.Segment(Vector3.zero, new Vector3(0f, -0.2f, -0.36f), 0.04f, 0.025f, 4, EnemySwatch.Shadow);
                b.Segment(new Vector3(0f, -0.2f, -0.36f), new Vector3(0.02f, -0.5f, -0.5f), 0.025f, 0.015f, 4, EnemySwatch.Shroud);
                b.Cylinder(new Vector3(0.02f, -0.5f, -0.5f), 0.02f, 0f, 0.1f, 3, EnemySwatch.Bone, rotation: Quaternion.Euler(180f, 0f, 0f));
                b.Blob(new Vector3(0.02f, -0.56f, -0.5f), new Vector3(0.22f, 0.08f, 0.22f), EnemySwatch.Shroud, coarse: true);
            });
        }

        private static void DrawSmokeRayBody(EnemyMeshBuilder b)
        {
            // When Kilwa went under, the rays came in over the streets and lay on the warm stones of the houses. The shadow blew its smoke into them through the windows, and now they glide through the rooms like smoke themselves, and spit the light they swallowed.
            b.Blob(Vector3.zero, new Vector3(0.44f, 0.16f, 0.62f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.06f, -0.02f), new Vector3(0.34f, 0.08f, 0.46f), EnemySwatch.Shroud, coarse: true);
            // The breathing holes behind the eyes puffing smoke, and the glob of swallowed light it spits, glowing through its back.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, 0.1f, 0.12f), new Vector3(0.08f, 0.12f, 0.08f), EnemySwatch.Shroud, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.08f, -0.06f), new Vector3(0.12f, 0.05f, 0.12f), EnemySwatch.StolenLight, coarse: true);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.3f, 0.04f, 0.4f), EnemySwatch.IvoryDark, coarse: true);
        }

        private static void DrawSmokeRayFin(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.TwoSided(new Vector3(0f, 0f, 0.26f), new Vector3(0.52f, -0.04f, -0.04f), new Vector3(0f, 0f, -0.28f), EnemySwatch.Shadow, EnemySwatch.IvoryDark);
            b.TwoSided(new Vector3(0f, 0.01f, 0.18f), new Vector3(0.4f, -0.02f, -0.02f), new Vector3(0f, 0.01f, -0.18f), EnemySwatch.Shroud, EnemySwatch.Shroud);
            b.Matrix = Matrix4x4.identity;
        }

        // ------------------------------------------------------------------
        // Garça Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedHeron(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "DimmedHeron", 0.8f, DrawDimmedHeronBody, new Vector3(0f, 0.5f, 0.3f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.16f, 0.16f, 0.22f), EnemySwatch.GolemStone);
                    // The black plume going out of the crown, and the long dagger bill, gone grey.
                    b.Segment(new Vector3(0f, 0.05f, -0.06f), new Vector3(0f, 0.02f, -0.24f), 0.02f, 0.005f, 3, EnemySwatch.Black);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.08f), 0.035f, 0.008f, 0.26f, 5, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(92f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.03f, 0.05f, 0.04f, EnemySwatch.MoonEye);
                },
                new Vector3(0f, -0.03f, 0.08f), b => b.Box(new Vector3(0f, -0.005f, 0.1f), new Vector3(0.04f, 0.015f, 0.2f), EnemySwatch.SandstoneDark),
                EnemySwatch.GolemStone, EnemySwatch.IvoryDark, EnemySwatch.Black, 0.95f, EnemySwatch.GolemStone, EnemySwatch.IvoryDark);
        }

        private static void DrawDimmedHeronBody(EnemyMeshBuilder b)
        {
            // The white herons of Liyongo stood in the canals so still that the fishermen tied their boats to them by mistake. The shadow put out the white of their feathers first, and then the patience: now they stab at anything that moves on the water.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.36f, 0.62f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.28f, 0.24f, 0.4f), EnemySwatch.IvoryDark, coarse: true);
            // The long S of the neck up to the head, and the soot of the shadow where the white plumes were.
            b.Segment(new Vector3(0f, 0.08f, 0.2f), new Vector3(0f, 0.3f, 0.18f), 0.07f, 0.05f, 5, EnemySwatch.IvoryDark);
            b.Segment(new Vector3(0f, 0.3f, 0.18f), new Vector3(0f, 0.48f, 0.28f), 0.05f, 0.045f, 5, EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.14f, -0.12f), new Vector3(0.22f, 0.06f, 0.3f), EnemySwatch.Shadow, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.8f, 0.07f, EnemySwatch.Black, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Gavial do Lodo
        // ------------------------------------------------------------------

        public static void BuildMudGharial(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawMudGharialBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0f, 0.74f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.32f, 0.22f, 0.28f), EnemySwatch.Verdigris);
                // The long thin snout of the gharial, made for fish, and the round knob at its tip like a clay pot.
                b.Box(new Vector3(0f, -0.01f, 0.36f), new Vector3(0.12f, 0.08f, 0.56f), EnemySwatch.Verdigris);
                b.Blob(new Vector3(0f, 0.03f, 0.64f), new Vector3(0.14f, 0.1f, 0.12f), EnemySwatch.BarkDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.1f, 0.02f, 0.06f, EnemySwatch.MoonEye);
                for (int i = 0; i < 5; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.055f * s, -0.04f, 0.16f + i * 0.1f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.26f), new Vector3(0.11f, 0.04f, 0.52f), EnemySwatch.Verdigris);
                b.Box(new Vector3(0f, 0.012f, 0.24f), new Vector3(0.07f, 0.01f, 0.44f), EnemySwatch.Pink);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.72f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.42f), 0.19f, 0.1f, 6, EnemySwatch.Verdigris);
                b.Segment(new Vector3(0f, -0.04f, -0.42f), new Vector3(0.04f, -0.1f, -0.76f), 0.1f, 0.02f, 5, EnemySwatch.BarkDark);
                for (int i = 0; i < 4; i++)
                {
                    b.Box(new Vector3(0f, 0.11f - i * 0.025f, -0.08f - i * 0.18f), new Vector3(0.03f, 0.08f - i * 0.012f, 0.1f), EnemySwatch.BarkDark);
                }
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.3f, 0.32f, 0.42f, -0.42f, 0.08f, EnemySwatch.Verdigris, EnemySwatch.BarkDark, "MudGharial");
        }

        private static void DrawMudGharialBody(EnemyMeshBuilder b)
        {
            // The gharials of Jhilpur's lake only ever ate fish, and the children swam beside them. When the Lodo came up the delta it covered them so long that it got in through the scales, and now they lie in the shallows with their thin jaws open and wait for anything at all.
            b.Blob(Vector3.zero, new Vector3(0.74f, 0.42f, 1.56f), EnemySwatch.Verdigris);
            b.Blob(new Vector3(0f, -0.08f, 0f), new Vector3(0.62f, 0.22f, 1.36f), EnemySwatch.CaveBelly, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                // The two rows of scutes, and the cakes of mud of the lake between them.
                float z = -0.56f + i * 0.22f;
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.11f * s, 0.2f, z), new Vector3(0.12f, 0.07f, 0.14f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, -12f * s));
                }

                if (i % 2 == 0)
                {
                    b.Blob(new Vector3(0f, 0.22f, z + 0.1f), new Vector3(0.16f, 0.06f, 0.16f), EnemySwatch.Bark, coarse: true);
                }
            }

            b.Blob(new Vector3(-0.06f, 0.2f, 0.18f), new Vector3(0.24f, 0.05f, 0.26f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Ninki Nanka Filhote
        // ------------------------------------------------------------------

        public static void BuildNinkiHatchling(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawNinkiHatchlingBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.36f, 0.66f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.24f, 0.32f), EnemySwatch.CaveMoss);
                b.Box(new Vector3(0f, -0.02f, 0.22f), new Vector3(0.2f, 0.1f, 0.28f), EnemySwatch.CaveMoss);
                // The little horns that will be the mother's, the frill behind the head, and the eyes that must not be looked at.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Horn(new Vector3(0.08f * s, 0.1f, -0.06f), 0.035f, 0.18f, 3, -18f, EnemySwatch.Horn);
                    b.TwoSided(new Vector3(0.1f * s, 0.02f, -0.1f), new Vector3(0.26f * s, 0.1f, -0.2f), new Vector3(0.12f * s, -0.08f, -0.14f), EnemySwatch.Crystal, EnemySwatch.CaveMoss);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.07f, 0.1f, 0.06f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.14f), new Vector3(0.18f, 0.04f, 0.26f), EnemySwatch.CaveMoss);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.06f * s, 0.01f, 0.22f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.6f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.4f), 0.17f, 0.09f, 6, EnemySwatch.CaveMoss);
                b.Segment(new Vector3(0f, -0.06f, -0.4f), new Vector3(-0.06f, -0.14f, -0.72f), 0.09f, 0.02f, 5, EnemySwatch.BarkDark);
                for (int i = 0; i < 4; i++)
                {
                    b.Cylinder(new Vector3(0f, 0.12f - i * 0.03f, -0.06f - i * 0.18f), 0.03f, 0f, 0.1f - i * 0.015f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(-24f, 0f, 0f));
                }
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.32f, 0.3f, 0.36f, -0.38f, 0.08f, EnemySwatch.CaveMoss, EnemySwatch.BarkDark, "NinkiHatchling");
        }

        private static void DrawNinkiHatchlingBody(EnemyMeshBuilder b)
        {
            // The Ninki Nanka of the bog was a dragon who guarded the water and never left it; who saw her face did not come back the same. Her young hatched after the shadow came, in the warm mud, and they guard nothing yet: they only bite.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.44f, 1.2f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, -0.08f, 0.02f), new Vector3(0.54f, 0.24f, 1.04f), EnemySwatch.CaveBelly, coarse: true);
            // The long neck rising to the head, and the crest of glinting scales down the back, the mother's mirrors in small.
            b.Segment(new Vector3(0f, 0.08f, 0.42f), new Vector3(0f, 0.34f, 0.62f), 0.14f, 0.1f, 6, EnemySwatch.CaveMoss);
            for (int i = 0; i < 5; i++)
            {
                b.Cylinder(new Vector3(0f, 0.2f, -0.4f + i * 0.2f), 0.04f, 0f, 0.14f, 4, EnemySwatch.Crystal);
            }

            b.Blob(new Vector3(0.14f, 0.12f, -0.1f), new Vector3(0.2f, 0.08f, 0.3f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Raiz Podre
        // ------------------------------------------------------------------

        public static void BuildRottenRoot(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawRottenRootBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.2f), b =>
            {
                // The light the root drank from the mire, rotting inside the knot; it swells before the root spits its splinters.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.14f, 0.08f), EnemySwatch.Lichen);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.09f, 0.08f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawRottenRootBody(EnemyMeshBuilder b)
        {
            // Under the Coração do Lodo the roots of the old trees drank the black water until they rotted. Some of them broke off and did not fall: a cloud of spores holds them up, and they drift through the dead wood spitting splinters at whatever is still alive.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.44f, 0.46f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0.1f, 0.08f, -0.06f), new Vector3(0.3f, 0.26f, 0.3f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(-0.12f, -0.06f, 0.06f), new Vector3(0.26f, 0.24f, 0.26f), EnemySwatch.Lichen, coarse: true);
            // The snapped ends sticking up, and the rotten roots hanging down to the mud: the lowest points of the model.
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 30f) * Mathf.Deg2Rad;
                b.Cylinder(new Vector3(Mathf.Cos(a) * 0.12f, 0.16f, Mathf.Sin(a) * 0.1f), 0.04f, 0.01f, 0.18f, 4, EnemySwatch.BarkDark,
                    rotation: Quaternion.Euler(Mathf.Sin(a) * 20f, 0f, -Mathf.Cos(a) * 20f));
            }

            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 10f) * Mathf.Deg2Rad;
                var top = new Vector3(Mathf.Cos(a) * 0.14f, -0.14f, Mathf.Sin(a) * 0.12f);
                var mid = new Vector3(Mathf.Cos(a) * 0.32f, -0.4f, Mathf.Sin(a) * 0.26f);
                var low = new Vector3(Mathf.Cos(a) * 0.22f, -0.68f, Mathf.Sin(a) * 0.18f);
                b.Segment(top, mid, 0.04f, 0.028f, 4, EnemySwatch.BarkDark);
                b.Segment(mid, low, 0.028f, 0.012f, 4, i % 2 == 0 ? EnemySwatch.Bark : EnemySwatch.Lichen);
            }

            // The cloud of spores round it, grey-green, thickest underneath where it holds the knot up.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 20f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.3f, -0.22f + (i % 2) * 0.1f, Mathf.Sin(a) * 0.26f), new Vector3(0.14f, 0.1f, 0.14f),
                    i % 2 == 0 ? EnemySwatch.CaveMoss : EnemySwatch.Shroud, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.06f, 0.2f), new Vector3(0.22f, 0.2f, 0.1f), EnemySwatch.Black, coarse: true);
        }

        // ------------------------------------------------------------------
        // Lanterna-Peixe
        // ------------------------------------------------------------------

        public static void BuildFishLantern(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawFishLanternBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0f, 0.02f), b =>
            {
                // The candle of the festival that the shadow swapped for a stolen light; it swells before the lantern spits it.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.22f, 0.3f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.1f, 0.12f, 0.16f), EnemySwatch.FireflyGlow, coarse: true);
            });
        }

        private static void DrawFishLanternBody(EnemyMeshBuilder b)
        {
            // On the canal of the lanterns the children carry paper carps on sticks, a candle inside each, so the carps will remember to jump the falls. The shadow blew out the candles and put a stolen light in their place, and the lanterns swam off their sticks by themselves.
            // The paper skin, red and gold, torn here and there so the light shows through.
            b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.4f, 0.2f, 0.8f), EnemySwatch.WarCape, coarse: true);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.36f, 0.18f, 0.72f), EnemySwatch.Gold, coarse: true);
            // The bamboo ribs that hold the paper in the shape of a carp.
            for (int i = 0; i < 5; i++)
            {
                float z = -0.28f + i * 0.14f;
                float r = 0.2f - Mathf.Abs(z) * 0.3f;
                b.Box(new Vector3(0f, 0f, z), new Vector3(r * 2.1f, 0.03f, 0.025f), EnemySwatch.Bark);
                b.Box(new Vector3(0f, 0f, z), new Vector3(0.03f, r * 2.1f, 0.025f), EnemySwatch.Bark);
            }

            // The painted eyes and scales, the paper fins and the forked tail.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.15f * s, 0.06f, 0.32f), new Vector3(0.08f, 0.08f, 0.05f), EnemySwatch.Ivory, coarse: true);
                b.Blob(new Vector3(0.17f * s, 0.06f, 0.33f), new Vector3(0.04f, 0.04f, 0.03f), EnemySwatch.Black, coarse: true);
                b.TwoSided(new Vector3(0.14f * s, -0.08f, 0.18f), new Vector3(0.34f * s, -0.2f, 0.02f), new Vector3(0.14f * s, -0.06f, 0.0f), EnemySwatch.WarCape, EnemySwatch.Gold);
            }

            b.TwoSided(new Vector3(0f, 0.18f, 0.14f), new Vector3(0f, 0.34f, -0.06f), new Vector3(0f, 0.18f, -0.2f), EnemySwatch.Gold, EnemySwatch.WarCape);
            b.TwoSided(new Vector3(0f, 0f, -0.36f), new Vector3(0.18f, 0.2f, -0.58f), new Vector3(0f, 0f, -0.48f), EnemySwatch.WarCape, EnemySwatch.Gold);
            b.TwoSided(new Vector3(0f, 0f, -0.36f), new Vector3(0f, 0f, -0.48f), new Vector3(-0.18f, -0.2f, -0.58f), EnemySwatch.WarCape, EnemySwatch.Gold);
            // The red string and the tassel hanging under it, brushing the water: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.18f, 0.02f), new Vector3(0f, -0.56f, 0.04f), 0.012f, 0.012f, 4, EnemySwatch.WarCape);
            b.Box(new Vector3(0f, -0.56f, 0.04f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Gold);
            b.Cylinder(new Vector3(0f, -0.68f, 0.04f), 0.06f, 0.02f, 0.1f, 6, EnemySwatch.WarCape);
        }

        // ------------------------------------------------------------------
        // Barqueiro de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeBoatman(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.82f, 0f), DrawSmokeBoatmanBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.02f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.28f, 0.26f), EnemySwatch.Shadow);
                // The wide straw hat of the boatmen of the canal, and under it two cold eyes where his face was.
                b.Cylinder(new Vector3(0f, 0.08f, 0f), 0.34f, 0f, 0.2f, 10, EnemySwatch.SandstoneDark, bottomCap: true);
                b.Cylinder(new Vector3(0f, 0.09f, 0f), 0.3f, 0.12f, 0.08f, 10, EnemySwatch.Sandstone);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0f, 0.11f, 0.05f, EnemySwatch.MoonEye);
            });
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bool right = side > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.24f * sd, 0.24f, 0.04f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.05f * sd, -0.26f, 0.1f), 0.07f, 0.06f, 5, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.05f * sd, -0.26f, 0.1f), new Vector3(0.03f * sd, -0.42f, 0.24f), 0.06f, 0.045f, 5, EnemySwatch.Shadow);
                    b.Blob(new Vector3(0.03f * sd, -0.45f, 0.26f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.Shadow, coarse: true);
                    if (right)
                    {
                        // The punting pole he still pushes, though there is no boat under him any more.
                        b.Segment(new Vector3(0.03f * sd, -0.9f, 0.22f), new Vector3(0.03f * sd, 0.46f, 0.3f), 0.025f, 0.025f, 5, EnemySwatch.Bark);
                        b.Blob(new Vector3(0.03f * sd, -0.9f, 0.22f), new Vector3(0.06f, 0.05f, 0.06f), EnemySwatch.BarkDark, coarse: true);
                    }
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: "SmokeBoatman" + (right ? "Arm_R" : "Arm_L"));
            }
        }

        private static void DrawSmokeBoatmanBody(EnemyMeshBuilder b)
        {
            // The boatmen of the canal took the crafts from ring to ring at night, singing so the barges would not bump into each other in the dark. The shadow took the boats and the songs, and left the boatmen: smoke in a straw hat, pushing a pole through the water, looking for a barge to steer.
            b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.52f, 0.56f, 0.4f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, 0.18f, 0.06f), new Vector3(0.36f, 0.36f, 0.3f), EnemySwatch.Shadow, coarse: true);
            // The smoke of his legs, a column that thins down to the water: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.8f, 0f), 0.3f, 0.2f, 0.4f, 8, EnemySwatch.Shroud, bottomCap: true, twist: 15f);
            b.Cylinder(new Vector3(0f, -0.42f, 0f), 0.2f, 0.24f, 0.4f, 8, EnemySwatch.Shadow, twist: 30f);
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 15f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.26f, -0.74f, Mathf.Sin(a) * 0.24f), new Vector3(0.16f, 0.1f, 0.16f), EnemySwatch.Shroud, coarse: true);
            }

            // The rope belt with its little lamp, out.
            b.Box(new Vector3(0f, -0.08f, 0.02f), new Vector3(0.44f, 0.04f, 0.34f), EnemySwatch.Leather);
            b.Blob(new Vector3(-0.14f, -0.16f, 0.16f), new Vector3(0.07f, 0.1f, 0.07f), EnemySwatch.Copper, coarse: true);
        }

        // ------------------------------------------------------------------
        // Pato-Mandarim Ladrão
        // ------------------------------------------------------------------

        public static void BuildMandarinDuck(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "MandarinDuck", 0.42f, DrawMandarinDuckBody, new Vector3(0f, 0.26f, 0.26f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.22f), EnemySwatch.Copper);
                    // The green crest, the white stripe over the eye, and the stolen ring held in the bill.
                    b.Blob(new Vector3(0f, 0.07f, -0.05f), new Vector3(0.12f, 0.12f, 0.2f), EnemySwatch.Verdigris, coarse: true);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Box(new Vector3(0.08f * s, 0.04f, -0.01f), new Vector3(0.02f, 0.04f, 0.16f), EnemySwatch.Ivory);
                    }

                    b.Cylinder(new Vector3(0f, -0.01f, 0.09f), 0.035f, 0.025f, 0.08f, 5, EnemySwatch.WarCape, rotation: Quaternion.Euler(90f, 0f, 0f));
                    b.Blob(new Vector3(0f, -0.03f, 0.18f), new Vector3(0.05f, 0.05f, 0.02f), EnemySwatch.Gold, coarse: true);
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.02f, 0.06f, 0.035f, EnemySwatch.MoonEye);
                },
                new Vector3(0f, -0.04f, 0.08f), b => b.Box(new Vector3(0f, -0.005f, 0.05f), new Vector3(0.05f, 0.015f, 0.08f), EnemySwatch.Pink),
                EnemySwatch.Copper, EnemySwatch.Ivory, EnemySwatch.Verdigris, 0.8f, EnemySwatch.Black, EnemySwatch.Ivory);
        }

        private static void DrawMandarinDuckBody(EnemyMeshBuilder b)
        {
            // The mandarin drakes of the willow river were the brides' good luck: a pair carved in wood went into every new house. The shadow made the live ones thieves, and they snatch the rings and the coins off the boats and the paddies, and bite the hand that takes them back.
            b.Blob(Vector3.zero, new Vector3(0.4f, 0.34f, 0.6f), EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.06f, 0.1f), new Vector3(0.32f, 0.2f, 0.34f), EnemySwatch.WarCape, coarse: true);
            b.Segment(new Vector3(0f, 0.08f, 0.18f), new Vector3(0f, 0.24f, 0.24f), 0.08f, 0.07f, 6, EnemySwatch.Copper);
            // The two orange sails on its back, the black-and-white bars on the breast, and the shadow's soot on the flanks.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.1f * s, 0.2f, -0.12f), new Vector3(0.02f, 0.16f, 0.14f), EnemySwatch.Gold, Quaternion.Euler(-20f, 0f, -14f * s));
                b.Box(new Vector3(0.14f * s, 0.04f, 0.16f), new Vector3(0.02f, 0.12f, 0.03f), EnemySwatch.Ivory, Quaternion.Euler(0f, 30f * s, 0f));
            }

            b.Blob(new Vector3(0.1f, 0.02f, -0.12f), new Vector3(0.18f, 0.12f, 0.24f), EnemySwatch.Shadow, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.42f, 0.08f, EnemySwatch.Gold, EnemySwatch.Gold);
        }

        // ------------------------------------------------------------------
        // Espantalho Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedScarecrow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.9f, 0f), DrawDimmedScarecrowBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.42f, 0.1f, 0.04f), b =>
            {
                // The paddy lantern hung on its arm; the shadow put a stolen light in it, and it flares before the scarecrow throws it.
                b.Blob(Vector3.zero, new Vector3(0.14f, 0.18f, 0.14f), EnemySwatch.StolenLight);
                b.Box(new Vector3(0f, 0.1f, 0f), new Vector3(0.14f, 0.03f, 0.14f), EnemySwatch.Iron);
                b.Box(new Vector3(0f, -0.1f, 0f), new Vector3(0.14f, 0.03f, 0.14f), EnemySwatch.Iron);
            });
        }

        private static void DrawDimmedScarecrowBody(EnemyMeshBuilder b)
        {
            // The scarecrows of the mirror paddies kept the sparrows off the rice and a lantern lit for the ducks to find their way home. The shadow got into the straw, and now they keep the people off, and the lantern shows where to throw.
            // The pole down into the mud of the paddy: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.88f, 0f), new Vector3(0f, 0.5f, 0f), 0.035f, 0.03f, 5, EnemySwatch.Bark);
            b.Blob(new Vector3(0f, -0.88f, 0f), new Vector3(0.3f, 0.08f, 0.3f), EnemySwatch.BarkDark, coarse: true);
            // The crossbar of its arms, and the straw spilling out of the sleeves.
            b.Box(new Vector3(0f, 0.28f, 0f), new Vector3(1.04f, 0.05f, 0.05f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.4f, 0.58f, 0.28f), EnemySwatch.OwlBuff);
            b.Box(new Vector3(0f, 0.1f, 0.01f), new Vector3(0.46f, 0.4f, 0.3f), EnemySwatch.Leather);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.3f * s, 0.26f, 0f), new Vector3(0.3f, 0.12f, 0.12f), EnemySwatch.Leather);
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0.46f * s, 0.24f + i * 0.03f, -0.02f + i * 0.02f), 0.025f, 0f, 0.14f, 3, EnemySwatch.OwlBuff,
                        rotation: Quaternion.Euler(0f, 0f, -80f * s + i * 12f));
                }
            }

            // The sack head and the straw hat, its stitched mouth, and soot where its painted eyes were.
            b.Blob(new Vector3(0f, 0.6f, 0f), new Vector3(0.28f, 0.3f, 0.26f), EnemySwatch.SandstoneLight);
            b.Cylinder(new Vector3(0f, 0.7f, 0f), 0.3f, 0.06f, 0.16f, 9, EnemySwatch.OwlBuff, bottomCap: true);
            b.Box(new Vector3(0f, 0.53f, 0.13f), new Vector3(0.12f, 0.015f, 0.01f), EnemySwatch.Black);
            HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.62f, 0.12f, 0.05f, EnemySwatch.Shadow);
            b.Blob(new Vector3(0.06f, 0.63f, 0.13f), new Vector3(0.02f, 0.02f, 0.01f), EnemySwatch.WarEye, coarse: true);
        }

        // ------------------------------------------------------------------
        // Raio Torto
        // ------------------------------------------------------------------

        public static void BuildCrookedRay(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.76f, 0f), DrawCrookedRayBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0.06f), b =>
            {
                // The glare of the bent light in the heart of the shard; it flares and blinds for a moment before it strikes.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.2f, 0.14f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0f, 0.03f), new Vector3(0.1f, 0.1f, 0.06f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawCrookedRayBody(EnemyMeshBuilder b)
        {
            // The White Towers send their light out straight, from spire to spire, so the light of the city finds its way home. The shadow held a shard of smoked glass in the path of one ray, and it came out bent: now it wanders the terraces crooked, and whoever it touches cannot see for a moment.
            b.Crystal(new Vector3(0f, -0.2f, 0f), 0.18f, 0.6f, EnemySwatch.Crystal);
            b.Crystal(new Vector3(0.1f, -0.1f, -0.06f), 0.08f, 0.34f, EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, -24f));
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(0.56f, 0.3f, 0.44f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(-0.08f, 0.08f, -0.04f), new Vector3(0.3f, 0.2f, 0.3f), EnemySwatch.Shadow, coarse: true);
            // The two short beams that broke off sideways.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.14f * s, 0.02f, 0.02f), new Vector3(0.3f * s, 0.12f, 0.06f), 0.03f, 0.02f, 4, EnemySwatch.StolenLight);
                b.Segment(new Vector3(0.3f * s, 0.12f, 0.06f), new Vector3(0.36f * s, -0.04f, 0.04f), 0.02f, 0.01f, 4, EnemySwatch.SoftEye);
            }

            // The crooked beam zigzagging down to the terrace, where it scorched the stone: the lowest point of the model.
            var points = new[]
            {
                new Vector3(0f, -0.18f, 0.04f), new Vector3(-0.12f, -0.36f, 0.06f), new Vector3(0.08f, -0.52f, 0.02f), new Vector3(-0.04f, -0.74f, 0.04f),
            };
            for (int i = 0; i < points.Length - 1; i++)
            {
                b.Segment(points[i], points[i + 1], 0.035f - i * 0.006f, 0.03f - i * 0.006f, 4, i % 2 == 0 ? EnemySwatch.StolenLight : EnemySwatch.SoftEye);
            }

            b.Blob(new Vector3(-0.04f, -0.74f, 0.04f), new Vector3(0.18f, 0.05f, 0.18f), EnemySwatch.Black, coarse: true);
        }

        // ------------------------------------------------------------------
        // Lobo-Cinzento
        // ------------------------------------------------------------------

        public static void BuildGreyWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawGreyWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.18f, 0.46f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.26f), EnemySwatch.WolfFur);
                // The long grey muzzle and the ears laid back against the wind.
                b.Blob(new Vector3(0f, -0.04f, 0.15f), new Vector3(0.13f, 0.11f, 0.2f), EnemySwatch.WolfFur, coarse: true);
                b.Blob(new Vector3(0f, -0.02f, 0.25f), new Vector3(0.05f, 0.04f, 0.04f), EnemySwatch.Black, coarse: true);
                b.Blob(new Vector3(0f, -0.07f, 0.1f), new Vector3(0.1f, 0.05f, 0.14f), EnemySwatch.WolfBelly, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.07f * s, 0.09f, -0.04f), 0.045f, 0.01f, 0.14f, 4, EnemySwatch.WolfFurDark, rotation: Quaternion.Euler(-40f, 0f, -14f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.035f, 0.1f, 0.045f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.09f, 0.025f, 0.16f), EnemySwatch.WolfBelly);
                b.Cylinder(new Vector3(0.028f, 0.005f, 0.13f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.028f, 0.005f, 0.13f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.44f), b =>
            {
                // The brush blown sideways by the Destroying Wind, grey to the black tip.
                b.Segment(Vector3.zero, new Vector3(0.06f, -0.12f, -0.2f), 0.06f, 0.07f, 5, EnemySwatch.WolfFur);
                b.Segment(new Vector3(0.06f, -0.12f, -0.2f), new Vector3(0.1f, -0.22f, -0.3f), 0.07f, 0.02f, 4, EnemySwatch.WolfFurDark);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.44f, 0.15f, 0.28f, -0.28f, 0.055f, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, "GreyWolf");
        }

        private static void DrawGreyWolfBody(EnemyMeshBuilder b)
        {
            // The grey wolves of the prairie ran with the wild horses and took only the old and the lame, and the Nartes left them alone for it. The Destroying Wind blows the wrong way through their fur now, and they run in packs at the herds, and at the riders.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.4f, 0.88f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.32f, 0.2f, 0.64f), EnemySwatch.WolfBelly, coarse: true);
            b.Segment(new Vector3(0f, 0.08f, 0.3f), new Vector3(0f, 0.18f, 0.42f), 0.11f, 0.09f, 6, EnemySwatch.WolfFur);
            // The thick ruff of the steppe wolves, blown up the wrong way, and the dark saddle the wind pressed into its back.
            b.Blob(new Vector3(0f, 0.1f, 0.3f), new Vector3(0.6f, 0.3f, 0.28f), EnemySwatch.WolfFurDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0.26f * s, 0.12f - i * 0.06f, 0.3f), 0.04f, 0f, 0.1f, 3, EnemySwatch.WolfFur, rotation: Quaternion.Euler(-30f, 0f, -70f * s));
                }
            }

            b.Blob(new Vector3(0f, 0.16f, -0.08f), new Vector3(0.3f, 0.1f, 0.44f), EnemySwatch.WolfFurDark, coarse: true);
            b.Blob(new Vector3(-0.1f, 0.06f, -0.2f), new Vector3(0.14f, 0.12f, 0.18f), EnemySwatch.Shadow, coarse: true);
        }
    }
}
