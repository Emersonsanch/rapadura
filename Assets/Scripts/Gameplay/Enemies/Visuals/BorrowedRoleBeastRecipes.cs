using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The sixteenth pass of beasts (bible §3.2.1): roles of Regions 7–16 that the continuous world still played with another region's beast,
    /// low-poly from the shared palette like <see cref="WideStandInBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Pedra-Pomes Voadora</b> (65, ~1 m): a grey pumice stone afloat on a thread of its own steam, embers in the pores. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Ouriço de Vidro</b> (68, ~0.8 m): an urchin of black volcanic glass, the heat still inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Toupeira-Gelo</b> (62, ~0.6 m): a pale mole of the frozen tunnels with claws of ice. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Lagarto do Instante</b> (78, ~0.6 m): a sand lizard with a sundial fin, its after-image a step behind. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Bagre Negro</b> (79, ~0.7 m): a tar-black catfish propped on its fins, long barbels with lit tips. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Tremor</b> (81, ~0.85 m): a heap of the giant's shoulder stone that shakes along its seam. <c>Body</c> → <c>StolenLight</c> (the seam).</item>
    /// <item><b>Água que Apaga</b> (82, ~1.1 m): a column of still water with blank tablet chips in it. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Corrente Errada</b> (92, ~1.1 m): a coil of the River of Light running the wrong way. <c>Body</c> → <c>StolenLight</c> (the knot).</item>
    /// <item><b>Sombra Nadadora</b> (92, ~0.7 m): a shadow that swims in the light like an eel. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Sombra Clandestina</b> (91, ~0.9 m): a shadow hunched under a rag of Barca sail. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Rachadura do Céu</b> (99, ~1.5 m): a crack of the sky walking off with its gold edges. <c>Body</c> → <c>StolenLight</c> (the dark inside).</item>
    /// <item><b>Fenda Viva</b> (38, ~0.85 m): a slab of glacier with a crevasse for a mouth. <c>Body</c> → <c>StolenLight</c> (the blue deep).</item>
    /// </list>
    /// </summary>
    public static class BorrowedRoleBeastRecipes
    {
        // ------------------------------------------------------------------
        // Pedra-Pomes Voadora
        // ------------------------------------------------------------------

        public static void BuildFlyingPumice(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawFlyingPumiceBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.22f), b =>
            {
                // The ember the volcano left in its heart; it flares before the stone drops on you.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.18f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.1f, 0.1f, 0.06f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawFlyingPumiceBody(EnemyMeshBuilder b)
        {
            // The forges of Hinokura blow pumice up out of the vents, and it used to fall back as grit for the polishers. Since the shadow it does not come down: it drifts about the fumaroles in flocks.
            b.Blob(Vector3.zero, new Vector3(0.7f, 0.56f, 0.64f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0.1f, 0.12f, -0.06f), new Vector3(0.44f, 0.34f, 0.44f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(-0.12f, -0.1f, 0.04f), new Vector3(0.4f, 0.3f, 0.4f), EnemySwatch.GolemStoneDark);
            // The pores, a few of them still glowing.
            for (int i = 0; i < 8; i++)
            {
                float a = i / 8f * Mathf.PI * 2f;
                var p = new Vector3(Mathf.Cos(a) * 0.3f, Mathf.Sin(a * 2f) * 0.12f, Mathf.Sin(a) * 0.28f);
                b.Blob(p, new Vector3(0.06f, 0.05f, 0.06f), i % 3 == 0 ? EnemySwatch.Ember : EnemySwatch.Black, coarse: true);
            }

            // The thread of steam it rides on, and the puff where it touches the ground: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.7f, 0f), 0.08f, 0.18f, 0.44f, 6, EnemySwatch.Shroud, twist: 30f);
            b.Blob(new Vector3(0f, -0.66f, 0f), new Vector3(0.3f, 0.08f, 0.3f), EnemySwatch.Shroud, coarse: true);
        }

        // ------------------------------------------------------------------
        // Ouriço de Vidro
        // ------------------------------------------------------------------

        public static void BuildGlassUrchin(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawGlassUrchinBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0.24f), b =>
            {
                // The heat of the lava that went into the sea, kept inside the glass; it flares before the urchin rolls at you.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.18f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.1f, 0.1f, 0.06f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawGlassUrchinBody(EnemyMeshBuilder b)
        {
            // The lava that ran into the sea turned to black glass, and some of it kept walking. It sits on the reef like an urchin, and the spines cut.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.54f, 0.6f), EnemySwatch.Black);
            b.Blob(new Vector3(0.06f, 0.1f, 0.04f), new Vector3(0.4f, 0.34f, 0.4f), EnemySwatch.BatFurDark);
            // The glass spines, upward and sideways (none into the reef).
            for (int i = 0; i < 14; i++)
            {
                float a = i / 14f * Mathf.PI * 2f + (i % 2) * 0.2f;
                float up = i % 3 == 0 ? 0.8f : i % 3 == 1 ? 0.3f : -0.15f;
                Vector3 dir = new Vector3(Mathf.Cos(a), up, Mathf.Sin(a)).normalized;
                b.Cylinder(dir * 0.24f, 0.035f, 0f, 0.22f, 4, i % 4 == 0 ? EnemySwatch.Crystal : EnemySwatch.Black, rotation: Quaternion.FromToRotation(Vector3.up, dir));
            }

            b.Cylinder(new Vector3(0f, 0.24f, 0f), 0.04f, 0f, 0.26f, 4, EnemySwatch.Crystal);
            // The flat foot it grips the reef with: the lowest point of the model.
            b.Blob(new Vector3(0f, -0.32f, 0f), new Vector3(0.5f, 0.12f, 0.5f), EnemySwatch.BatFurDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Toupeira-Gelo
        // ------------------------------------------------------------------

        public static void BuildIceMole(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawIceMoleBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.42f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.24f, 0.3f), EnemySwatch.Ivory);
                // The pink star of a nose it feels the tunnels with, and the tiny eyes that the frost lit.
                b.Cylinder(new Vector3(0f, -0.01f, 0.12f), 0.07f, 0.05f, 0.1f, 6, EnemySwatch.Pink, rotation: Quaternion.Euler(90f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.01f, 0.23f), new Vector3(0.1f, 0.08f, 0.04f), EnemySwatch.Pink, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.06f, 0.1f, 0.035f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.14f, 0.04f, 0.12f), EnemySwatch.IvoryDark);
                b.Cylinder(new Vector3(0.03f, 0.01f, 0.11f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.03f, 0.01f, 0.11f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.38f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.16f), 0.04f, 0.015f, 4, EnemySwatch.Pink));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.24f, 0.22f, 0.24f, -0.22f, 0.06f, EnemySwatch.Ivory, EnemySwatch.Crystal, "IceMole");
        }

        private static void DrawIceMoleBody(EnemyMeshBuilder b)
        {
            // The moles dig the little tunnels beside the mammoth's big ones, and they kept the galleries from falling in. The cold went into their claws, and now they dig at whoever walks the tunnels.
            b.Blob(Vector3.zero, new Vector3(0.58f, 0.46f, 0.8f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.44f, 0.24f, 0.6f), EnemySwatch.OwlBuff, coarse: true);
            b.Blob(new Vector3(0f, 0.08f, -0.08f), new Vector3(0.48f, 0.3f, 0.56f), EnemySwatch.GolemSnow);
            // Frozen earth on its back and a few ice shards grown in the fur.
            b.Blob(new Vector3(-0.1f, 0.18f, 0.06f), new Vector3(0.14f, 0.06f, 0.16f), EnemySwatch.SandstoneDark, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                b.Crystal(new Vector3(-0.08f + i * 0.08f, 0.16f, -0.18f + i * 0.08f), 0.03f, 0.12f, EnemySwatch.Crystal, Quaternion.Euler(-20f + i * 15f, 0f, -15f + i * 15f));
            }
        }

        // ------------------------------------------------------------------
        // Lagarto do Instante
        // ------------------------------------------------------------------

        public static void BuildInstantLizard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawInstantLizardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.04f, 0.5f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.18f, 0.3f), EnemySwatch.SandstoneLight);
                b.Blob(new Vector3(0f, 0.04f, -0.04f), new Vector3(0.18f, 0.08f, 0.2f), EnemySwatch.Sandstone, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.06f, 0.04f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.07f), new Vector3(0.16f, 0.03f, 0.16f), EnemySwatch.SandstoneDark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.04f, -0.06f, -0.3f), 0.08f, 0.04f, 5, EnemySwatch.SandstoneLight);
                b.Segment(new Vector3(0.04f, -0.06f, -0.3f), new Vector3(-0.02f, -0.1f, -0.52f), 0.04f, 0.01f, 4, EnemySwatch.Sandstone);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.2f, 0.26f, 0.28f, -0.24f, 0.05f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, "InstantLizard");
        }

        private static void DrawInstantLizardBody(EnemyMeshBuilder b)
        {
            // In the desert of the gnomons the shadows are a moment late, and these lizards live in that moment: they are already where you will look, and their after-image is still where they were.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.28f, 0.9f), EnemySwatch.SandstoneLight);
            b.Blob(new Vector3(0f, -0.06f, 0.04f), new Vector3(0.36f, 0.14f, 0.7f), EnemySwatch.IvoryDark, coarse: true);
            // The sundial fin on its back: a half disc of bronze with its hour lines, and the gnomon that throws no shadow.
            b.Cylinder(new Vector3(0f, 0.1f, -0.02f), 0.24f, 0.24f, 0.02f, 8, EnemySwatch.Copper, rotation: Quaternion.Euler(0f, 0f, 90f));
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.2f, -0.18f + i * 0.11f), new Vector3(0.03f, 0.1f, 0.01f), EnemySwatch.CopperDark);
            }

            b.Cylinder(new Vector3(0f, 0.1f, 0f), 0.015f, 0.005f, 0.3f, 4, EnemySwatch.Gold);
            b.Blob(new Vector3(0f, 0.4f, 0f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            // The after-image a step behind it, a smudge of violet shadow.
            b.Blob(new Vector3(0.06f, 0.02f, -0.3f), new Vector3(0.36f, 0.2f, 0.4f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Bagre Negro
        // ------------------------------------------------------------------

        public static void BuildBlackCatfish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.32f, 0f), DrawBlackCatfishBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.44f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.5f, 0.3f, 0.34f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.4f, 0.14f, 0.24f), EnemySwatch.CaveSkinDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.16f, 0.06f, 0.08f, 0.04f, EnemySwatch.MoonEye);
                // The long barbels, drooping, each with a drop of the light it fishes with at the tip.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.16f * s, -0.02f, 0.14f), new Vector3(0.36f * s, -0.14f, 0.26f), 0.016f, 0.008f, 3, EnemySwatch.BatFurDark);
                    b.Blob(new Vector3(0.36f * s, -0.14f, 0.26f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.FireflyGlow, coarse: true);
                    b.Segment(new Vector3(0.08f * s, -0.08f, 0.14f), new Vector3(0.14f * s, -0.2f, 0.2f), 0.01f, 0.005f, 3, EnemySwatch.BatFurDark);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.34f, 0.04f, 0.12f), EnemySwatch.CaveSkinDark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.44f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.24f), 0.12f, 0.05f, 6, EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.02f, -0.32f), new Vector3(0.02f, 0.22f, 0.16f), EnemySwatch.BatFurDark);
            });
        }

        private static void DrawBlackCatfishBody(EnemyMeshBuilder b)
        {
            // The catfish of the bitumen wells lived on the bottom and kept the tar soft for the boat-caulkers. The pitch went dark, and the catfish with it; now it lies under the rafts and tips them.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.4f, 0.9f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.44f, 0.18f, 0.7f), EnemySwatch.CaveSkinDark, coarse: true);
            b.Blob(new Vector3(0f, 0.12f, -0.06f), new Vector3(0.36f, 0.16f, 0.5f), EnemySwatch.BatFurDark);
            // The dorsal spine and the drips of tar running off it.
            b.Box(new Vector3(0f, 0.24f, 0.04f), new Vector3(0.02f, 0.24f, 0.18f), EnemySwatch.BatFurDark, Quaternion.Euler(-12f, 0f, 0f));
            b.Blob(new Vector3(0.16f, -0.04f, -0.2f), new Vector3(0.05f, 0.08f, 0.05f), EnemySwatch.Black, coarse: true);
            // The pectoral fins it props itself up on in the mud: the lowest points of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.2f * s, -0.08f, 0.16f), new Vector3(0.34f * s, -0.28f, 0.22f), 0.04f, 0.025f, 4, EnemySwatch.Black);
                b.Box(new Vector3(0.35f * s, -0.3f, 0.23f), new Vector3(0.12f, 0.02f, 0.1f), EnemySwatch.BatFurDark);
            }
        }

        // ------------------------------------------------------------------
        // Tremor
        // ------------------------------------------------------------------

        public static void BuildTremor(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawTremorBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.34f), b =>
            {
                // The seam it shakes along, lit from the giant's stone underneath; it flares before the ground jumps.
                b.Box(Vector3.zero, new Vector3(0.08f, 0.4f, 0.04f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 0f, 8f));
                b.Blob(new Vector3(0f, 0.04f, 0.01f), new Vector3(0.1f, 0.14f, 0.04f), EnemySwatch.GolemCrack, coarse: true);
            });
        }

        private static void DrawTremorBody(EnemyMeshBuilder b)
        {
            // On Upelluri's shoulder the ground is skin of stone, and it shudders where the diorite grows into it. The shudders took a shape of their own and walk about, knocking people off their feet.
            b.Box(Vector3.zero, new Vector3(0.78f, 0.5f, 0.66f), EnemySwatch.GolemStone, top: EnemySwatch.Lichen);
            b.Blob(new Vector3(-0.18f, 0.24f, -0.06f), new Vector3(0.4f, 0.3f, 0.4f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0.2f, 0.22f, 0.04f), new Vector3(0.36f, 0.28f, 0.36f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.32f, -0.12f), new Vector3(0.3f, 0.2f, 0.3f), EnemySwatch.Lichen);
            // Pebbles shaken loose round it, and the cracked ground it stands on: the lowest point of the model.
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 20f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.46f, -0.4f, Mathf.Sin(a) * 0.4f), new Vector3(0.1f, 0.08f, 0.1f), EnemySwatch.GolemStoneDark, coarse: true);
            }

            b.Box(new Vector3(0f, -0.44f, 0f), new Vector3(0.9f, 0.12f, 0.8f), EnemySwatch.GolemStoneDark, top: EnemySwatch.GolemStone);
            b.Box(new Vector3(0.1f, -0.37f, 0.12f), new Vector3(0.5f, 0.02f, 0.03f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 30f, 0f));
        }

        // ------------------------------------------------------------------
        // Água que Apaga
        // ------------------------------------------------------------------

        public static void BuildErasingWater(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawErasingWaterBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0.1f), b =>
            {
                // A sign it washed off a tablet, still glowing as it sinks; it flares before the water lunges.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.12f), EnemySwatch.StolenLight);
                b.Box(new Vector3(0f, 0f, 0.05f), new Vector3(0.1f, 0.02f, 0.02f), EnemySwatch.Rune);
            });
        }

        private static void DrawErasingWaterBody(EnemyMeshBuilder b)
        {
            // In the field of the dream tablets the dreams were written down so they would not be lost. The still water came up between the tablets and began to wash them blank, and it stands up when someone comes to read.
            b.Cylinder(new Vector3(0f, -0.6f, 0f), 0.36f, 0.2f, 0.96f, 9, EnemySwatch.CaveSkin, twist: 20f);
            b.Blob(new Vector3(0f, 0.36f, 0f), new Vector3(0.44f, 0.24f, 0.44f), EnemySwatch.CaveSkin);
            b.Blob(new Vector3(0f, 0.42f, 0f), new Vector3(0.3f, 0.12f, 0.3f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.5f, 0.5f, 0.5f), EnemySwatch.CaveSkinDark);
            // Chips of the tablets it has washed blank, turning in it.
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 0.26f, -0.2f + i * 0.14f, Mathf.Sin(a) * 0.26f), new Vector3(0.12f, 0.08f, 0.02f), EnemySwatch.SandstoneLight,
                    Quaternion.Euler(10f * i, -a * Mathf.Rad2Deg, 20f));
            }

            // The pool it rises out of: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.6f, 0f), 0.48f, 0.46f, 0.03f, 10, EnemySwatch.CaveSkinDark, bottomCap: true);
        }

        // ------------------------------------------------------------------
        // Corrente Errada
        // ------------------------------------------------------------------

        public static void BuildWrongCurrent(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawWrongCurrentBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.42f, 0f), b =>
            {
                // The knot where the current turned back on itself; it flares before the coil whips round.
                b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), EnemySwatch.StolenLight);
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.Gold, coarse: true);
            });
        }

        private static void DrawWrongCurrentBody(EnemyMeshBuilder b)
        {
            // The River of Light runs one way, up toward the Moon and the Sun, carrying the freed light. Here and there a strand of it runs the other way, back down, and it pulls at the boats and at the swimmers.
            const int Steps = 16;
            Vector3 previous = new Vector3(0.3f, -0.55f, 0f);
            for (int i = 1; i <= Steps; i++)
            {
                float f = i / (float)Steps;
                float a = f * Mathf.PI * 4f;
                float r = 0.3f - f * 0.1f;
                var p = new Vector3(Mathf.Cos(a) * r, -0.55f + f * 0.95f, Mathf.Sin(a) * r);
                b.Segment(previous, p, 0.07f - f * 0.03f, 0.065f - f * 0.03f, 5, i % 2 == 0 ? EnemySwatch.Gold : EnemySwatch.SandstoneLight);
                previous = p;
            }

            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.3f, 0.6f, 0.3f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0f, 0.2f, 0f), new Vector3(0.24f, 0.3f, 0.24f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.4f, 0f), new Vector3(0.36f, 0.2f, 0.36f), EnemySwatch.SandstoneLight);
            // The eddy it spins in, on the surface: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.6f, 0f), 0.38f, 0.36f, 0.03f, 10, EnemySwatch.Gold, bottomCap: true, twist: 15f);
        }

        // ------------------------------------------------------------------
        // Sombra Nadadora
        // ------------------------------------------------------------------

        public static void BuildSwimmingShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.45f, 0f), DrawSwimmingShadowBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.04f, 0.46f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.24f, 0.32f), EnemySwatch.Shadow);
                b.Blob(new Vector3(0f, 0.04f, -0.04f), new Vector3(0.2f, 0.12f, 0.2f), EnemySwatch.BatFurDark, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.1f, 0.045f, EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.07f), new Vector3(0.18f, 0.04f, 0.16f), EnemySwatch.Black);
                b.Cylinder(new Vector3(0.05f, 0.01f, 0.13f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.05f, 0.01f, 0.13f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.06f, -0.04f, -0.3f), 0.1f, 0.05f, 6, EnemySwatch.Shadow);
                b.Segment(new Vector3(0.06f, -0.04f, -0.3f), new Vector3(-0.04f, -0.08f, -0.5f), 0.05f, 0.015f, 5, EnemySwatch.BatFurDark);
            });
        }

        private static void DrawSwimmingShadowBody(EnemyMeshBuilder b)
        {
            // A shadow cannot swim in water, but it can in light: it slides along under the surface of the River of Light like an eel and drinks the glow of the boats from below.
            b.Blob(Vector3.zero, new Vector3(0.4f, 0.32f, 0.92f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.3f, 0.16f, 0.7f), EnemySwatch.BatFurDark, coarse: true);
            b.Blob(new Vector3(0f, 0.08f, -0.1f), new Vector3(0.3f, 0.2f, 0.5f), EnemySwatch.Black);
            // The rippling dorsal fin, and a few drops of the light it drank showing through.
            for (int i = 0; i < 5; i++)
            {
                b.Box(new Vector3(0f, 0.18f + (i % 2) * 0.02f, 0.24f - i * 0.12f), new Vector3(0.02f, 0.12f, 0.1f), EnemySwatch.BatFurDark, Quaternion.Euler(-10f + i * 6f, 0f, 0f));
            }

            b.Blob(new Vector3(0.1f, 0.02f, 0.1f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            b.Blob(new Vector3(-0.08f, 0f, -0.14f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            // The side fins, and the wisps that trail down to the light it swims in: the lowest points of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.16f * s, -0.04f, 0.12f), new Vector3(0.32f * s, -0.14f, 0.02f), 0.035f, 0.01f, 4, EnemySwatch.Shadow);
            }

            b.Segment(new Vector3(0f, -0.12f, 0f), new Vector3(0.04f, -0.42f, -0.12f), 0.05f, 0.015f, 4, EnemySwatch.Shadow);
            b.Blob(new Vector3(0.04f, -0.42f, -0.12f), new Vector3(0.06f, 0.04f, 0.06f), EnemySwatch.BatFurDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Sombra Clandestina
        // ------------------------------------------------------------------

        public static void BuildStowawayShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.45f, 0f), DrawStowawayShadowBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.28f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.26f, 0.26f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.03f, 0.11f, 0.045f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.18f, 0.04f, 0.12f), EnemySwatch.Shadow);
                b.Cylinder(new Vector3(0.04f, 0.01f, 0.11f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.01f, 0.11f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.28f), b =>
                b.Segment(Vector3.zero, new Vector3(0.04f, -0.2f, -0.24f), 0.08f, 0.02f, 5, EnemySwatch.Shadow));
        }

        private static void DrawStowawayShadowBody(EnemyMeshBuilder b)
        {
            // The Barcas carry the freed light up to the Moon, and a shadow that wants to go along hides in the hold under a rag of old sail. On the quay they come out at the edges, stealing a little of the light that waits for the boats.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.58f, 0.56f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.44f, 0.4f, 0.44f), EnemySwatch.BatFurDark);
            // The rag of sail over its back, with the Barca's painted moon half washed off, and the rope that tied it.
            b.Box(new Vector3(0f, 0.26f, -0.04f), new Vector3(0.7f, 0.04f, 0.6f), EnemySwatch.Ivory, Quaternion.Euler(-12f, 0f, 6f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.33f * s, 0.06f, -0.04f), new Vector3(0.04f, 0.34f, 0.5f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 0f, -10f * s));
            }

            b.Blob(new Vector3(0.12f, 0.3f, -0.1f), new Vector3(0.14f, 0.03f, 0.14f), EnemySwatch.StolenLight, coarse: true);
            b.Segment(new Vector3(-0.3f, 0.14f, 0.2f), new Vector3(0.3f, 0.14f, 0.22f), 0.015f, 0.015f, 4, EnemySwatch.Bark);
            // The skirt of shadow it creeps on: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.45f, 0f), 0.34f, 0.26f, 0.3f, 8, EnemySwatch.Shadow, bottomCap: true);
        }

        // ------------------------------------------------------------------
        // Rachadura do Céu
        // ------------------------------------------------------------------

        public static void BuildSkyCrack(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.8f, 0f), DrawSkyCrackBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.08f, 0.05f), b =>
            {
                // The dark that runs out of the crack; it swells before the crack splits wider at you.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.42f, 0.06f), EnemySwatch.Sorcery);
                b.Blob(new Vector3(0f, 0.04f, 0.01f), new Vector3(0.08f, 0.2f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawSkyCrackBody(EnemyMeshBuilder b)
        {
            // From the lookout you can see the cracks in the sky where the shadow drips through. Some of the small ones came loose with their edges and walk about the platform, trying to let more of it in.
            float[] ys = { -0.6f, -0.24f, 0.12f, 0.46f };
            float[] xs = { 0.08f, -0.06f, 0.07f, -0.04f };
            float[] tilts = { 15f, -18f, 16f, -12f };
            for (int i = 0; i < ys.Length; i++)
            {
                // The zigzag of the crack, black, with its edges of the sky's gold and white on either side.
                b.Box(new Vector3(xs[i], ys[i], 0f), new Vector3(0.1f, 0.4f, 0.06f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, tilts[i]));
                b.Box(new Vector3(xs[i] + 0.08f, ys[i], 0f), new Vector3(0.04f, 0.36f, 0.08f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, tilts[i]));
                b.Box(new Vector3(xs[i] - 0.08f, ys[i], 0f), new Vector3(0.04f, 0.36f, 0.08f), EnemySwatch.Ivory, Quaternion.Euler(0f, 0f, tilts[i]));
            }

            // Shards of sky it tore off, drifting round it.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 15f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 0.3f, -0.3f + i * 0.14f, Mathf.Sin(a) * 0.22f), new Vector3(0.1f, 0.12f, 0.03f), i % 2 == 0 ? EnemySwatch.Ivory : EnemySwatch.Gold,
                    Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 25f));
            }

            b.Blob(new Vector3(0f, 0.68f, 0f), new Vector3(0.18f, 0.2f, 0.12f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.1f, -0.06f), new Vector3(0.22f, 0.5f, 0.1f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.6f, 0f), new Vector3(0.3f, 0.16f, 0.2f), EnemySwatch.Black);
            // The puddle of dark it drips on the platform: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.8f, 0f), 0.3f, 0.28f, 0.03f, 10, EnemySwatch.Black, bottomCap: true);
        }

        // ------------------------------------------------------------------
        // Fenda Viva
        // ------------------------------------------------------------------

        public static void BuildLivingCrevasse(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.32f, 0f), DrawLivingCrevasseBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.12f, 0f), b =>
            {
                // The blue deep of the crevasse, where the cold keeps the light it swallowed; it flares before the crevasse opens under you.
                b.Box(Vector3.zero, new Vector3(0.5f, 0.06f, 0.12f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 20f, 0f));
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.06f, 0.08f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawLivingCrevasseBody(EnemyMeshBuilder b)
        {
            // The glacier on the Roof of the World splits and closes every year, and the yak herders know where. Since the shadow, the cracks move: one opens under the path, swallows, and closes again.
            b.Box(Vector3.zero, new Vector3(0.92f, 0.36f, 0.8f), EnemySwatch.GolemSnow, top: EnemySwatch.GolemSnow);
            // The two lips of the crevasse, ice-blue at the break, and the dark between them.
            b.Box(new Vector3(0f, 0.12f, 0f), new Vector3(0.56f, 0.14f, 0.16f), EnemySwatch.Black, Quaternion.Euler(0f, 20f, 0f));
            b.Blob(new Vector3(-0.22f, 0.16f, 0.12f), new Vector3(0.4f, 0.16f, 0.36f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0.22f, 0.16f, -0.12f), new Vector3(0.4f, 0.16f, 0.36f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.6f, 0.12f, 0.2f), EnemySwatch.Crystal, Quaternion.Euler(0f, 20f, 0f), coarse: true);
            // The seracs standing on it like teeth.
            b.Crystal(new Vector3(-0.3f, 0.16f, -0.24f), 0.08f, 0.36f, EnemySwatch.GolemSnow, Quaternion.Euler(-8f, 0f, 10f));
            b.Crystal(new Vector3(0.32f, 0.16f, 0.22f), 0.07f, 0.3f, EnemySwatch.Crystal, Quaternion.Euler(8f, 0f, -12f));
            b.Crystal(new Vector3(0.1f, 0.16f, -0.3f), 0.06f, 0.22f, EnemySwatch.GolemSnow, Quaternion.Euler(-12f, 0f, -4f));
            // The packed snow it lies in: the lowest point of the model.
            b.Box(new Vector3(0f, -0.27f, 0f), new Vector3(1f, 0.1f, 0.9f), EnemySwatch.GolemStoneDark, top: EnemySwatch.GolemSnow);
        }
    }
}
