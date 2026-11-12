using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The fifteenth pass of beasts (bible §3.2.1): the most-seen roles of Regions 5–16 that the continuous world still played with stand-ins
    /// after the passes by region, low-poly from the shared palette like <see cref="FarStandInBeastRecipes"/> (the birds share the skeleton of
    /// <see cref="MidStandInBeastRecipes"/>). Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Abutre da Estepe</b> (51, ~1.3 m): a tawny steppe vulture with a pale ruff. Birds' parts.</item>
    /// <item><b>Lobo da Estepe Apagado</b> (47, ~1.1 m): a lean grey-tan wolf of the grass. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Tempestade de Capim</b> (51, ~1.1 m): a funnel of dry grass and dust round a caught spark. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Fogo-Fátuo Preso</b> (33, ~1 m): a marsh light shut in a cage of reeds. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Sapo-Boi</b> (34, ~0.75 m): a fat bullfrog of the palm swamp; its throat sac is the <c>StolenLight</c>.</item>
    /// <item><b>Peixe-Saltador de Lodo</b> (31, ~0.8 m long): a mudskipper propped on its fins, eyes on top. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Espelho Quebrado</b> (42, ~1.1 m): a cracked bronze mirror on its broken stand. <c>Body</c> → <c>StolenLight</c> (the glass).</item>
    /// <item><b>Grou de Papel</b> (41, ~1.3 m): a folded paper crane with ink on its wings. Birds' parts.</item>
    /// <item><b>Gafanhoto de Fumaça</b> (44, ~0.8 m): a big locust of smoke on long jumping legs. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, wings, the abdomen as <c>Tail</c>.</item>
    /// <item><b>Ushabti de Coral</b> (29, ~1 m): a squat figure of coral with its arms crossed. <c>Body</c> → <c>StolenLight</c> (the seal on its chest).</item>
    /// <item><b>Faísca Confusa</b> (91, ~1 m): a spark with a halo of shards and a trail of dust. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Gota de Treva</b> (94, ~0.95 m): a teardrop of the Dark on its own puddle, a light swallowed inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class WideStandInBeastRecipes
    {
        // ------------------------------------------------------------------
        // Abutre da Estepe
        // ------------------------------------------------------------------

        public static void BuildSteppeVulture(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "SteppeVulture", 0.62f, DrawSteppeVultureBody, new Vector3(0f, 0.38f, 0.36f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.26f), EnemySwatch.SandstoneLight);
                    b.Cylinder(new Vector3(0f, 0f, 0.1f), 0.045f, 0.012f, 0.15f, 5, EnemySwatch.Horn, rotation: Quaternion.Euler(96f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.04f, 0.07f, 0.05f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.07f), new Vector3(0.06f, 0.02f, 0.14f), EnemySwatch.Horn, Quaternion.Euler(10f, 0f, 0f)),
                EnemySwatch.Leather, EnemySwatch.SandstoneDark, EnemySwatch.Black, 1.05f, EnemySwatch.Leather, EnemySwatch.SandstoneDark);
        }

        private static void DrawSteppeVultureBody(EnemyMeshBuilder b)
        {
            // The vultures of the steppe followed the herds and the horsemen and cleaned up after both. The shadow came in on the wind over the poppies, and now they follow whoever is still alive.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.5f, 0.82f), EnemySwatch.Leather);
            b.Blob(new Vector3(0f, -0.08f, 0.14f), new Vector3(0.4f, 0.32f, 0.42f), EnemySwatch.OwlBuff, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.26f), new Vector3(0f, 0.36f, 0.34f), 0.08f, 0.06f, 6, EnemySwatch.SandstoneLight);
            // The pale ruff of down, with a seed of the poppies' ember caught in it.
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.13f, 0.15f, 0.24f + Mathf.Sin(a) * 0.09f), new Vector3(0.13f, 0.09f, 0.11f), EnemySwatch.Ivory, coarse: true);
            }

            b.Blob(new Vector3(-0.08f, 0.19f, 0.3f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.62f, 0.11f, EnemySwatch.SandstoneDark, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Lobo da Estepe Apagado
        // ------------------------------------------------------------------

        public static void BuildSteppeWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.58f, 0f), DrawSteppeWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.52f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.28f, 0.3f), EnemySwatch.WolfFur);
                b.Blob(new Vector3(0f, -0.04f, 0.2f), new Vector3(0.15f, 0.13f, 0.24f), EnemySwatch.WolfBelly, coarse: true);
                b.Blob(new Vector3(0f, -0.01f, 0.32f), new Vector3(0.06f, 0.05f, 0.05f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.09f * s, 0.12f, -0.04f), 0.05f, 0f, 0.15f, 4, EnemySwatch.WolfFurDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.13f, 0.05f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.12f), new Vector3(0.13f, 0.04f, 0.22f), EnemySwatch.WolfFurDark);
                b.Cylinder(new Vector3(0.04f, 0.005f, 0.2f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.005f, 0.2f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.46f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.12f, -0.3f), 0.08f, 0.05f, 5, EnemySwatch.WolfFur);
                b.Blob(new Vector3(0f, -0.13f, -0.32f), new Vector3(0.08f, 0.08f, 0.1f), EnemySwatch.WolfFurDark, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.48f, 0.2f, 0.32f, -0.32f, 0.065f, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, "SteppeWolf");
        }

        private static void DrawSteppeWolfBody(EnemyMeshBuilder b)
        {
            // The steppe wolves ran beside Soslan's horses for the joy of it and never took a foal. The shadow thinned them to the bone and taught them to run at the horses instead.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.48f, 1f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, -0.12f, 0.02f), new Vector3(0.44f, 0.24f, 0.72f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, 0.06f, 0.34f), new Vector3(0.52f, 0.42f, 0.34f), EnemySwatch.WolfFurDark, coarse: true);
            // A saddle of darker fur down the back and a few dry grass seeds stuck in it, one of them still glowing.
            b.Box(new Vector3(0f, 0.22f, -0.04f), new Vector3(0.22f, 0.04f, 0.56f), EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0.1f, 0.2f, -0.2f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Tempestade de Capim
        // ------------------------------------------------------------------

        public static void BuildGrassStorm(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawGrassStormBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.12f, 0f), b =>
            {
                // The spark of a lightning that fell on the prairie, spun round and round; it flares before the gust.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.22f, 0.18f), EnemySwatch.StolenLight);
                b.Blob(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawGrassStormBody(EnemyMeshBuilder b)
        {
            // Over the prairie of the thousand horses the wind lifts the dry grass every evening and puts it down again. Since the shadow it does not put it down.
            b.Cylinder(new Vector3(0f, -0.6f, 0f), 0.08f, 0.34f, 0.92f, 8, EnemySwatch.OwlBuff, twist: 50f);
            b.Cylinder(new Vector3(0f, -0.58f, 0f), 0.05f, 0.24f, 0.8f, 7, EnemySwatch.SandstoneDark, twist: -40f);
            // Blades of grass caught in the spin, on a slant.
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2f + i * 0.4f;
                float y = -0.4f + (i % 7) * 0.12f;
                float r = 0.14f + (y + 0.6f) * 0.24f;
                b.Box(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r), new Vector3(0.02f, 0.16f, 0.03f), i % 3 == 0 ? EnemySwatch.Gold : EnemySwatch.SandstoneLight,
                    Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 35f));
            }

            b.Blob(new Vector3(0f, 0.34f, 0f), new Vector3(0.5f, 0.12f, 0.46f), EnemySwatch.Shroud, coarse: true);
        }

        // ------------------------------------------------------------------
        // Fogo-Fátuo Preso
        // ------------------------------------------------------------------

        public static void BuildMarshWisp(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawMarshWispBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0f), b =>
            {
                // The marsh light itself, pale green, flickering; it flares before it bursts against you.
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.32f, 0.24f), EnemySwatch.Rune);
                b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.12f, 0.16f, 0.12f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawMarshWispBody(EnemyMeshBuilder b)
        {
            // The fishermen of Tarchon caught the marsh lights in reed cages to see by at night. The Dark got into the cages, and now the lights run about the swamp with their cages on.
            const int Bars = 6;
            for (int i = 0; i < Bars; i++)
            {
                float a = i / (float)Bars * Mathf.PI * 2f;
                var low = new Vector3(Mathf.Cos(a) * 0.2f, -0.48f, Mathf.Sin(a) * 0.2f);
                var mid = new Vector3(Mathf.Cos(a) * 0.28f, -0.05f, Mathf.Sin(a) * 0.28f);
                var top = new Vector3(Mathf.Cos(a) * 0.08f, 0.32f, Mathf.Sin(a) * 0.08f);
                b.Segment(low, mid, 0.018f, 0.018f, 4, EnemySwatch.Bark);
                b.Segment(mid, top, 0.018f, 0.014f, 4, EnemySwatch.BarkDark);
            }

            // The rings that bind the reeds, the knot on top, and the base it was set down on: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.07f, 0f), 0.29f, 0.29f, 0.04f, 10, EnemySwatch.Lichen);
            b.Cylinder(new Vector3(0f, -0.5f, 0f), 0.22f, 0.21f, 0.05f, 8, EnemySwatch.BarkDark, bottomCap: true);
            b.Blob(new Vector3(0f, 0.34f, 0f), new Vector3(0.1f, 0.08f, 0.1f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(0f, 0.26f, 0f), new Vector3(0.18f, 0.14f, 0.18f), EnemySwatch.CaveMoss);
        }

        // ------------------------------------------------------------------
        // Sapo-Boi
        // ------------------------------------------------------------------

        public static void BuildBullFrog(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.32f, 0f), DrawBullFrogBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.06f, 0.34f), b =>
            {
                // The throat sac, with the light it croaked up from the bottom of the swamp; it swells before the frog throws itself.
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.2f, 0.18f), EnemySwatch.CaveBelly);
                b.Blob(new Vector3(0f, -0.02f, 0.04f), new Vector3(0.14f, 0.1f, 0.1f), EnemySwatch.FireflyGlow, coarse: true);
            });
        }

        private static void DrawBullFrogBody(EnemyMeshBuilder b)
        {
            // The bullfrogs of the palm swamp sang the rain in every evening. The mire went into their throats, and now the song pulls the travellers off the paths instead.
            b.Blob(Vector3.zero, new Vector3(0.8f, 0.56f, 0.7f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, -0.1f, 0.06f), new Vector3(0.6f, 0.3f, 0.56f), EnemySwatch.Lichen, coarse: true);
            // The warts on its back, and the eyes standing up out of the head.
            for (int i = 0; i < 6; i++)
            {
                b.Blob(new Vector3(-0.2f + i * 0.08f, 0.24f, -0.12f + (i % 2) * 0.1f), new Vector3(0.06f, 0.04f, 0.06f), EnemySwatch.BarkDark, coarse: true);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.16f * s, 0.26f, 0.22f), new Vector3(0.14f, 0.12f, 0.12f), EnemySwatch.CaveMoss);
                b.Blob(new Vector3(0.16f * s, 0.29f, 0.26f), new Vector3(0.08f, 0.07f, 0.06f), EnemySwatch.MoonEye, coarse: true);
                // The folded legs: the hind ones and the hands it squats on, the lowest points of the model.
                b.Blob(new Vector3(0.34f * s, -0.18f, -0.12f), new Vector3(0.24f, 0.18f, 0.34f), EnemySwatch.CaveMoss, coarse: true);
                b.Box(new Vector3(0.36f * s, -0.3f, 0.08f), new Vector3(0.16f, 0.04f, 0.2f), EnemySwatch.Lichen);
                b.Box(new Vector3(0.2f * s, -0.3f, 0.3f), new Vector3(0.1f, 0.04f, 0.12f), EnemySwatch.Lichen);
            }
        }

        // ------------------------------------------------------------------
        // Peixe-Saltador de Lodo
        // ------------------------------------------------------------------

        public static void BuildMudskipper(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.26f, 0f), DrawMudskipperBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.04f, 0.32f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.24f, 0.28f), EnemySwatch.SandstoneDark);
                // The eyes on top of the head like two lamps, the way a mudskipper's are.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.06f * s, 0.14f, 0.02f), new Vector3(0.09f, 0.1f, 0.09f), EnemySwatch.SandstoneDark, coarse: true);
                    b.Blob(new Vector3(0.06f * s, 0.17f, 0.04f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.MoonEye, coarse: true);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.2f, 0.04f, 0.1f), EnemySwatch.SandstoneLight));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.34f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.2f), 0.09f, 0.04f, 5, EnemySwatch.SandstoneDark);
                b.Box(new Vector3(0f, 0.02f, -0.28f), new Vector3(0.02f, 0.16f, 0.14f), EnemySwatch.Leather);
            });
        }

        private static void DrawMudskipperBody(EnemyMeshBuilder b)
        {
            // The mudskippers walk on the mud of the lake at low tide and skip from bank to bank. The mire taught them to skip at people, and they come by the dozen.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.3f, 0.7f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.28f, 0.14f, 0.56f), EnemySwatch.SandstoneLight, coarse: true);
            // The tall dorsal fin, blue-spotted, and the spots of the light it swallowed.
            b.Box(new Vector3(0f, 0.22f, -0.04f), new Vector3(0.02f, 0.3f, 0.34f), EnemySwatch.CaveSkin, Quaternion.Euler(-6f, 0f, 0f));
            b.Blob(new Vector3(0.12f, 0.06f, 0.08f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.FireflyGlow, coarse: true);
            b.Blob(new Vector3(-0.12f, 0.04f, -0.12f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.FireflyGlow, coarse: true);
            // The pectoral fins it props itself up on, splayed wide: the lowest points of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.14f * s, -0.06f, 0.14f), new Vector3(0.28f * s, -0.24f, 0.2f), 0.04f, 0.025f, 4, EnemySwatch.SandstoneDark);
                b.Box(new Vector3(0.29f * s, -0.25f, 0.21f), new Vector3(0.1f, 0.02f, 0.08f), EnemySwatch.Leather);
            }
        }

        // ------------------------------------------------------------------
        // Espelho Quebrado
        // ------------------------------------------------------------------

        public static void BuildBrokenMirror(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawBrokenMirrorBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0f, 0.035f), b =>
            {
                // The glass that still keeps a piece of the moon; it flares before it throws the light back.
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.36f, 0.03f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0.06f, 0.06f, 0.01f), new Vector3(0.12f, 0.12f, 0.02f), EnemySwatch.GolemSnow, coarse: true);
            });
        }

        private static void DrawBrokenMirrorBody(EnemyMeshBuilder b)
        {
            // In the terraced gardens they hung bronze mirrors to send the moon into the houses at night. The shadow cracked them, and the pieces send it at the walkers instead.
            b.Cylinder(new Vector3(0f, 0f, -0.02f), 0.32f, 0.32f, 0.04f, 12, EnemySwatch.Copper, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Cylinder(new Vector3(0f, 0f, -0.03f), 0.26f, 0.26f, 0.02f, 12, EnemySwatch.CopperDark, rotation: Quaternion.Euler(90f, 0f, 0f));
            // The cracks across the glass and the knob of the back.
            b.Box(new Vector3(-0.04f, 0.04f, 0.055f), new Vector3(0.02f, 0.44f, 0.01f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 28f));
            b.Box(new Vector3(0.08f, -0.06f, 0.055f), new Vector3(0.02f, 0.28f, 0.01f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -40f));
            b.Blob(new Vector3(0f, 0f, -0.06f), new Vector3(0.12f, 0.12f, 0.06f), EnemySwatch.Verdigris);
            // The broken stand and its foot: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.32f, -0.02f), new Vector3(0.03f, -0.66f, 0f), 0.04f, 0.05f, 6, EnemySwatch.CopperDark);
            b.Box(new Vector3(0.03f, -0.7f, 0f), new Vector3(0.34f, 0.07f, 0.42f), EnemySwatch.CopperDark, top: EnemySwatch.Verdigris);
        }

        // ------------------------------------------------------------------
        // Grou de Papel
        // ------------------------------------------------------------------

        public static void BuildPaperCrane(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "PaperCrane", 0.72f, DrawPaperCraneBody, new Vector3(0f, 0.6f, 0.42f), b =>
                {
                    b.Box(Vector3.zero, new Vector3(0.12f, 0.14f, 0.2f), EnemySwatch.Ivory, Quaternion.Euler(0f, 45f, 0f));
                    b.Cylinder(new Vector3(0f, 0f, 0.08f), 0.03f, 0f, 0.18f, 4, EnemySwatch.IvoryDark, rotation: Quaternion.Euler(90f, 0f, 0f));
                    // The red crown, and two ink dots for eyes that the shadow lit.
                    b.Box(new Vector3(0f, 0.08f, -0.02f), new Vector3(0.05f, 0.03f, 0.06f), EnemySwatch.WarCape);
                    HinokuraBeastRecipes.AddEyes(b, 0.055f, 0.02f, 0.03f, 0.04f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.04f, 0.08f), b => b.Box(new Vector3(0f, -0.005f, 0.08f), new Vector3(0.04f, 0.02f, 0.16f), EnemySwatch.IvoryDark),
                EnemySwatch.Ivory, EnemySwatch.IvoryDark, EnemySwatch.Black, 1f, EnemySwatch.Ivory, EnemySwatch.IvoryDark);
        }

        private static void DrawPaperCraneBody(EnemyMeshBuilder b)
        {
            // The children of the rings fold a thousand paper cranes for a wish. The wish was eaten, and the cranes flew off without it; they throw sharp folds of paper.
            b.Box(Vector3.zero, new Vector3(0.36f, 0.3f, 0.6f), EnemySwatch.Ivory, Quaternion.Euler(0f, 0f, 45f));
            b.Box(new Vector3(0f, -0.06f, 0f), new Vector3(0.24f, 0.2f, 0.66f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 0f, 45f));
            b.Segment(new Vector3(0f, 0.1f, 0.26f), new Vector3(0f, 0.6f, 0.4f), 0.04f, 0.03f, 4, EnemySwatch.Ivory);
            // The brush strokes of ink on the paper, one of them still wet with light.
            b.Box(new Vector3(0.13f, 0.12f, -0.04f), new Vector3(0.02f, 0.03f, 0.3f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, 45f));
            b.Box(new Vector3(-0.13f, 0.12f, 0.02f), new Vector3(0.02f, 0.03f, 0.22f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, -45f));
            b.Blob(new Vector3(0f, 0.2f, -0.1f), new Vector3(0.05f, 0.03f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            b.Blob(new Vector3(0f, -0.02f, 0f), new Vector3(0.2f, 0.2f, 0.3f), EnemySwatch.IvoryDark);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.72f, 0.07f, EnemySwatch.Black, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Gafanhoto de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeLocust(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawSmokeLocustBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.04f, 0.3f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.24f, 0.2f), EnemySwatch.Shadow);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.04f * s, 0.1f, 0.06f), new Vector3(0.12f * s, 0.3f, 0.22f), 0.008f, 0.004f, 3, EnemySwatch.Black);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.05f, 0.07f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.06f), b =>
            {
                // The mandibles that chew the millet to the ground; they part before the bite.
                b.Box(new Vector3(0.03f, -0.01f, 0.04f), new Vector3(0.04f, 0.04f, 0.08f), EnemySwatch.IronDark);
                b.Box(new Vector3(-0.03f, -0.01f, 0.04f), new Vector3(0.04f, 0.04f, 0.08f), EnemySwatch.IronDark);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.08f * side, 0.12f, 0.06f), b =>
                {
                    // The long smoke-grey wings, folded back along the body and half spread.
                    b.TwoSided(Vector3.zero, new Vector3(0.36f * side, 0.02f, -0.3f), new Vector3(0.1f * side, 0.01f, -0.52f), EnemySwatch.Shroud, EnemySwatch.Shadow);
                    b.TwoSided(Vector3.zero, new Vector3(0.42f * side, 0.03f, -0.08f), new Vector3(0.36f * side, 0.02f, -0.3f), EnemySwatch.Shroud, EnemySwatch.Shadow);
                    b.Box(new Vector3(0.3f * side, 0.025f, -0.2f), new Vector3(0.04f, 0.01f, 0.04f), EnemySwatch.Ember);
                }, meshKey: s < 0 ? "SmokeLocustWing_L" : "SmokeLocustWing_R");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.26f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.3f), 0.1f, 0.05f, 6, EnemySwatch.Shadow);
                b.Blob(new Vector3(0f, -0.06f, -0.3f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.BatFurDark, coarse: true);
            });
        }

        private static void DrawSmokeLocustBody(EnemyMeshBuilder b)
        {
            // The Smoke, the first of the Dark's five, settles where people are hungry. Over the mirrored paddies it took the shape of locusts, which are hunger with legs.
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.6f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.1f, 0.1f), new Vector3(0.26f, 0.16f, 0.3f), EnemySwatch.BatFurDark, coarse: true);
            // The big jumping legs, knees high, and the four front feet: the lowest points of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                var hip = new Vector3(0.12f * side, -0.04f, -0.06f);
                var knee = new Vector3(0.26f * side, 0.2f, -0.2f);
                var foot = new Vector3(0.24f * side, -0.4f, -0.3f);
                b.Segment(hip, knee, 0.05f, 0.035f, 4, EnemySwatch.Shadow);
                b.Segment(knee, foot, 0.025f, 0.015f, 4, EnemySwatch.BatFurDark);
                for (int k = 0; k < 2; k++)
                {
                    float z = 0.2f - k * 0.12f;
                    b.Segment(new Vector3(0.1f * side, -0.08f, z), new Vector3(0.22f * side, -0.4f, z + 0.04f), 0.02f, 0.012f, 4, EnemySwatch.BatFurDark);
                }
            }
        }

        // ------------------------------------------------------------------
        // Ushabti de Coral
        // ------------------------------------------------------------------

        public static void BuildCoralUshabti(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawCoralUshabtiBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0.2f), b =>
            {
                // The seal of the Artificers on its chest, still lit; it flares before the figure throws its weight.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.16f, 0.06f), EnemySwatch.StolenLight);
                b.Box(new Vector3(0f, 0f, 0.02f), new Vector3(0.1f, 0.02f, 0.02f), EnemySwatch.Rune);
            });
        }

        private static void DrawCoralUshabtiBody(EnemyMeshBuilder b)
        {
            // The Artificers left answering figures in the drowned town to keep it until the sea gave it back. The coral grew over them and they forgot whom they were waiting for.
            b.Box(new Vector3(0f, 0f, 0f), new Vector3(0.46f, 0.66f, 0.34f), EnemySwatch.Pink, top: EnemySwatch.Bone);
            b.Blob(new Vector3(0f, 0.44f, 0f), new Vector3(0.3f, 0.3f, 0.28f), EnemySwatch.Bone);
            HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.46f, 0.12f, 0.05f, EnemySwatch.Crystal);
            // The arms crossed over the chest, as the figures are made.
            b.Box(new Vector3(0f, 0.14f, 0.19f), new Vector3(0.48f, 0.08f, 0.08f), EnemySwatch.Bone, Quaternion.Euler(0f, 0f, 12f));
            b.Box(new Vector3(0f, 0.08f, 0.2f), new Vector3(0.48f, 0.08f, 0.08f), EnemySwatch.Bone, Quaternion.Euler(0f, 0f, -12f));
            // Branches of coral grown over the shoulders and the head.
            float[] xs = { -0.2f, -0.08f, 0.12f, 0.22f };
            for (int i = 0; i < xs.Length; i++)
            {
                b.Segment(new Vector3(xs[i], 0.3f + (i % 2) * 0.24f, -0.06f), new Vector3(xs[i] * 1.5f, 0.5f + (i % 2) * 0.24f, -0.12f), 0.03f, 0.015f, 4, EnemySwatch.WarCape);
                b.Blob(new Vector3(xs[i] * 1.5f, 0.5f + (i % 2) * 0.24f, -0.12f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Pink, coarse: true);
            }

            // The base block its feet were carved into: the lowest point of the model.
            b.Box(new Vector3(0f, -0.44f, 0.02f), new Vector3(0.52f, 0.12f, 0.42f), EnemySwatch.IvoryDark, top: EnemySwatch.Pink);
        }

        // ------------------------------------------------------------------
        // Faísca Confusa
        // ------------------------------------------------------------------

        public static void BuildConfusedSpark(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawConfusedSparkBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, Vector3.zero, b =>
            {
                // The spark itself, which does not know which way is up; it flares before it darts at you.
                b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), EnemySwatch.StolenLight);
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.GolemSnow, coarse: true);
            });
        }

        private static void DrawConfusedSparkBody(EnemyMeshBuilder b)
        {
            // The sparks gathered on the quay wait their turn for the Barcas. Some of them waited so long they forgot what they were waiting for, and fly about the edges bumping into everyone.
            for (int i = 0; i < 10; i++)
            {
                float a = i / 10f * Mathf.PI * 2f;
                float y = Mathf.Sin(a * 2f) * 0.08f;
                var dir = new Vector3(Mathf.Cos(a), 0f, Mathf.Sin(a));
                b.Cylinder(dir * 0.16f + new Vector3(0f, y, 0f), 0.03f, 0f, 0.14f, 3, i % 2 == 0 ? EnemySwatch.Crystal : EnemySwatch.Gold, rotation: Quaternion.FromToRotation(Vector3.up, dir));
            }

            b.Blob(Vector3.zero, new Vector3(0.36f, 0.36f, 0.36f), EnemySwatch.Shroud, coarse: true);
            // The trail of dust it leaves, curling down to the deck: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.14f, -0.04f), new Vector3(0.06f, -0.38f, -0.16f), 0.03f, 0.02f, 4, EnemySwatch.Gold);
            b.Segment(new Vector3(0.06f, -0.38f, -0.16f), new Vector3(-0.02f, -0.58f, -0.24f), 0.02f, 0.01f, 4, EnemySwatch.SandstoneLight);
            b.Blob(new Vector3(-0.02f, -0.58f, -0.24f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Gota de Treva
        // ------------------------------------------------------------------

        public static void BuildDarkDrop(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawDarkDropBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0f, 0.2f), b =>
            {
                // The light it fell on and swallowed, showing through; it flares before the drop splashes at you.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.08f, 0.08f, 0.06f), EnemySwatch.Sorcery, coarse: true);
            });
        }

        private static void DrawDarkDropBody(EnemyMeshBuilder b)
        {
            // The Wheel lifts the light up in its jars, and now and then a drop of the Dark comes up with it. It drips off the gears and runs back down to look for more light to fall on.
            b.Blob(Vector3.zero, new Vector3(0.58f, 0.58f, 0.58f), EnemySwatch.Black);
            b.Blob(new Vector3(-0.08f, 0.1f, 0.1f), new Vector3(0.24f, 0.2f, 0.2f), EnemySwatch.BatFurDark, coarse: true);
            b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.2f, 0f, 0.38f, 8, EnemySwatch.Black);
            HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.1f, 0.25f, 0.06f, EnemySwatch.Sorcery);
            // The puddle it stands in, with a few splashes: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.38f, 0f), 0.36f, 0.34f, 0.03f, 10, EnemySwatch.BatFurDark, bottomCap: true);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 30f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.34f, -0.33f, Mathf.Sin(a) * 0.34f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.Black, coarse: true);
            }
        }
    }
}
