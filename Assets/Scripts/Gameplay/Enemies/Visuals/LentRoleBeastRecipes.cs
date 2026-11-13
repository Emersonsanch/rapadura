using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The seventeenth pass of beasts (bible §3.2.1): the roles of Região 9, the Estepe dos Nartes (Docs/Mapas100.md 47–52), that the continuous
    /// world still played with another region's beast, low-poly from the shared palette like <see cref="BorrowedRoleBeastRecipes"/>. Pivot at the
    /// feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Águia-Dourada Apagada</b> (47, ~1.1 m): a golden eagle with ash in its nape. Bird skeleton (<c>Body</c> → <c>Head</c> → <c>Jaw</c>, wings, tail).</item>
    /// <item><b>Cervo de Ouro Apagado</b> (48, ~1.8 m): a stag of Scythian gold with antlers curled into spirals. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Cabra-Montês Gigante</b> (49, ~1.6 m): a hill giants' ibex with ridged horns swept back. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Javali da Colina</b> (49, ~0.95 m): a big dark boar with tusks and a bristle ridge. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Corvo-Mensageiro de Fumaça</b> (49, ~1 m): a crow of smoke with a dream tablet tied to its leg. Bird skeleton.</item>
    /// <item><b>Pica-Pau de Fumaça</b> (50, ~1 m): a black-and-white woodpecker with a red crest, smoke trailing off its back. Bird skeleton.</item>
    /// <item><b>Fungo Apagado</b> (50, ~1.1 m): shelf fungus on a larch stump, its spores lit. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Lince da Taiga</b> (50, ~0.95 m): a grey lynx with ear tufts, ruff and bobtail. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Garanhão Apagado</b> (51, ~1.8 m): a grey stallion with a mane of smoke. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Águia de Ferro</b> (52, ~1.1 m): an eagle with iron plates for feathers and a link of Amirani's chain on its leg. Bird skeleton.</item>
    /// <item><b>Urso do Cáucaso Apagado</b> (52, ~1.3 m): a dark mountain bear with a broken link round its neck. <c>Body</c> → <c>Head</c> → <c>Jaw</c>; four thick legs.</item>
    /// <item><b>Lobo de Neve</b> (52, ~1 m): a white wolf of Ohya's crest with frost in its ruff. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// </list>
    /// </summary>
    public static class LentRoleBeastRecipes
    {
        // ------------------------------------------------------------------
        // Águia-Dourada Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedGoldenEagle(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "DimmedGoldenEagle", 0.64f, DrawDimmedGoldenEagleBody, new Vector3(0f, 0.36f, 0.36f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.28f), EnemySwatch.Leather);
                    // The golden nape the eagle is named for, with the ash of the shadow settled in it.
                    b.Blob(new Vector3(0f, 0.05f, -0.06f), new Vector3(0.22f, 0.16f, 0.2f), EnemySwatch.Gold, coarse: true);
                    b.Blob(new Vector3(0.05f, 0.09f, -0.08f), new Vector3(0.08f, 0.05f, 0.08f), EnemySwatch.Shadow, coarse: true);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.11f), 0.045f, 0.012f, 0.13f, 5, EnemySwatch.Horn, rotation: Quaternion.Euler(104f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.04f, 0.08f, 0.05f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.12f), EnemySwatch.Horn),
                EnemySwatch.Leather, EnemySwatch.SandstoneDark, EnemySwatch.Black, 1.15f, EnemySwatch.Leather, EnemySwatch.SandstoneDark);
        }

        private static void DrawDimmedGoldenEagleBody(EnemyMeshBuilder b)
        {
            // The golden eagles hunted for Soslan's camp from the riders' fists and came back to the fist. The shadow came in on the wind, and now they stoop on the riders.
            b.Blob(Vector3.zero, new Vector3(0.54f, 0.5f, 0.8f), EnemySwatch.Leather);
            b.Blob(new Vector3(0f, -0.08f, 0.12f), new Vector3(0.4f, 0.3f, 0.42f), EnemySwatch.SandstoneDark, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.26f), new Vector3(0f, 0.34f, 0.34f), 0.09f, 0.08f, 6, EnemySwatch.Leather);
            // The pale bars at the root of the tail and the ash lying on the back.
            b.Box(new Vector3(0f, 0.04f, -0.36f), new Vector3(0.26f, 0.05f, 0.08f), EnemySwatch.Ivory);
            b.Blob(new Vector3(-0.06f, 0.22f, -0.04f), new Vector3(0.24f, 0.06f, 0.3f), EnemySwatch.Shadow, coarse: true);
            // The feathered "trousers" down to the yellow feet: the lowest points of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.12f * s, -0.22f, 0.02f), new Vector3(0.14f, 0.18f, 0.16f), EnemySwatch.SandstoneDark, coarse: true);
            }

            MidStandInBeastRecipes.DrawBirdLegs(b, 0.64f, 0.11f, EnemySwatch.Gold, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Cervo de Ouro Apagado
        // ------------------------------------------------------------------

        public static void BuildGoldenStag(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.86f, 0f), DrawGoldenStagBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.58f), DrawGoldenStagHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.14f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.1f, 0.03f, 0.16f), EnemySwatch.CopperDark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.5f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.08f, -0.1f), 0.05f, 0.03f, 4, EnemySwatch.Gold));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.64f, 0.18f, 0.36f, -0.36f, 0.055f, EnemySwatch.Gold, EnemySwatch.CopperDark, "GoldenStag");
        }

        private static void DrawGoldenStagBody(EnemyMeshBuilder b)
        {
            // The kurgans keep stags of gold in the animal style, legs folded under, antlers rolled into spirals. Since the shadow some of them stood up out of the graves and run the kurgans like real deer, cold and dull.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.5f, 1.04f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, -0.12f, 0.02f), new Vector3(0.42f, 0.22f, 0.8f), EnemySwatch.Copper, coarse: true);
            // The neck rising to the head, and the hammered spirals on the haunch and the shoulder of the animal style.
            b.Segment(new Vector3(0f, 0.14f, 0.36f), new Vector3(0f, 0.42f, 0.56f), 0.13f, 0.09f, 6, EnemySwatch.Gold);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.27f * s, 0.02f, -0.22f), 0.12f, 0.12f, 0.02f, 8, EnemySwatch.CopperDark, rotation: Quaternion.Euler(0f, 0f, 90f));
                b.Cylinder(new Vector3(0.27f * s, 0.04f, 0.24f), 0.09f, 0.09f, 0.02f, 8, EnemySwatch.CopperDark, rotation: Quaternion.Euler(0f, 0f, 90f));
            }

            // The soot of the shadow over the gold, where it went dull.
            b.Blob(new Vector3(0.08f, 0.2f, -0.1f), new Vector3(0.3f, 0.08f, 0.4f), EnemySwatch.Shadow, coarse: true);
        }

        private static void DrawGoldenStagHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.2f, 0.3f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, -0.04f, 0.14f), new Vector3(0.12f, 0.1f, 0.16f), EnemySwatch.Copper, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.06f, 0.04f, EnemySwatch.MoonEye);
            // The antlers swept back along the spine in the animal style, each tine rolled into a spiral.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 root = new Vector3(0.06f * s, 0.08f, -0.04f);
                Vector3 bend = new Vector3(0.16f * s, 0.34f, -0.14f);
                Vector3 tip = new Vector3(0.12f * s, 0.46f, -0.44f);
                b.Segment(root, bend, 0.03f, 0.025f, 5, EnemySwatch.Gold);
                b.Segment(bend, tip, 0.025f, 0.015f, 5, EnemySwatch.Gold);
                for (int i = 0; i < 3; i++)
                {
                    Vector3 at = Vector3.Lerp(bend, tip, 0.2f + i * 0.3f);
                    b.Cylinder(at + new Vector3(0.02f * s, 0.04f, 0f), 0.04f, 0.04f, 0.015f, 6, EnemySwatch.Copper, rotation: Quaternion.Euler(0f, 0f, 90f));
                }
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.09f * s, 0.06f, -0.06f), 0.035f, 0f, 0.1f, 4, EnemySwatch.Copper, rotation: Quaternion.Euler(0f, 0f, -60f * s));
            }
        }

        // ------------------------------------------------------------------
        // Cabra-Montês Gigante
        // ------------------------------------------------------------------

        public static void BuildGiantIbex(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.82f, 0f), DrawGiantIbexBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.6f), DrawGiantIbexHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.09f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.07f), new Vector3(0.1f, 0.03f, 0.14f), EnemySwatch.WolfFurDark);
                // The beard of an old billy goat.
                b.Cylinder(new Vector3(0f, -0.02f, 0.04f), 0.04f, 0.01f, 0.16f, 4, EnemySwatch.WolfFurDark, rotation: Quaternion.Euler(180f, 0f, 0f));
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.14f, -0.54f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, 0.04f, -0.08f), 0.045f, 0.02f, 4, EnemySwatch.WolfFurDark));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.6f, 0.2f, 0.38f, -0.36f, 0.065f, EnemySwatch.WolfFur, EnemySwatch.Black, "GiantIbex");
        }

        private static void DrawGiantIbexBody(EnemyMeshBuilder b)
        {
            // The hill giants keep ibexes the size of their own houses and milk them into stone pots. The shadow got into the herd in the night, and the goats came out of the pens butting at anything smaller than a giant.
            b.Blob(Vector3.zero, new Vector3(0.64f, 0.6f, 1.1f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, -0.14f, 0.04f), new Vector3(0.48f, 0.28f, 0.84f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, 0.12f, 0.3f), new Vector3(0.56f, 0.46f, 0.44f), EnemySwatch.WolfFurDark, coarse: true);
            b.Segment(new Vector3(0f, 0.14f, 0.4f), new Vector3(0f, 0.3f, 0.58f), 0.14f, 0.1f, 6, EnemySwatch.WolfFurDark);
            // The dark stripe down the spine and a tuft of giant's wool caught on its flank, with a burr that still glows.
            b.Box(new Vector3(0f, 0.28f, -0.04f), new Vector3(0.12f, 0.04f, 0.7f), EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0.28f, 0.04f, -0.2f), new Vector3(0.1f, 0.14f, 0.16f), EnemySwatch.Ivory, coarse: true);
            b.Blob(new Vector3(0.32f, 0.08f, -0.22f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
        }

        private static void DrawGiantIbexHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.2f, 0.22f, 0.3f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, -0.04f, 0.14f), new Vector3(0.12f, 0.1f, 0.14f), EnemySwatch.WolfBelly, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.085f, 0.04f, 0.06f, 0.04f, EnemySwatch.Ember);
            // The great horns, swept back in a half moon with the knots of every year standing out on them.
            for (int s = -1; s <= 1; s += 2)
            {
                Vector3 previous = new Vector3(0.05f * s, 0.1f, -0.02f);
                for (int i = 1; i <= 6; i++)
                {
                    float f = i / 6f;
                    float a = f * Mathf.PI * 0.9f;
                    var p = new Vector3((0.05f + f * 0.1f) * s, 0.1f + Mathf.Sin(a) * 0.36f, -0.02f - (1f - Mathf.Cos(a)) * 0.3f);
                    b.Segment(previous, p, 0.055f - f * 0.035f, 0.05f - f * 0.035f, 5, EnemySwatch.Horn);
                    b.Blob(p, new Vector3(0.09f - f * 0.05f, 0.03f, 0.09f - f * 0.05f), EnemySwatch.IvoryDark, coarse: true);
                    previous = p;
                }
            }
        }

        // ------------------------------------------------------------------
        // Javali da Colina
        // ------------------------------------------------------------------

        public static void BuildHillBoar(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawHillBoarBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, -0.02f, 0.56f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.34f, 0.36f), EnemySwatch.PeccaryFur);
                // The long snout with its disc, and the tusks curling up beside it.
                b.Cylinder(new Vector3(0f, -0.04f, 0.12f), 0.1f, 0.08f, 0.18f, 6, EnemySwatch.PeccaryFur, rotation: Quaternion.Euler(96f, 0f, 0f));
                b.Cylinder(new Vector3(0f, -0.06f, 0.3f), 0.08f, 0.08f, 0.02f, 6, EnemySwatch.Pink, rotation: Quaternion.Euler(90f, 0f, 0f));
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.08f * s, -0.08f, 0.2f), new Vector3(0.14f * s, 0.04f, 0.24f), 0.022f, 0.008f, 4, EnemySwatch.Fang);
                    b.Cylinder(new Vector3(0.12f * s, 0.14f, -0.06f), 0.05f, 0f, 0.1f, 4, EnemySwatch.BarkDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.06f, 0.1f, 0.04f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.13f, 0.14f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.16f, 0.04f, 0.14f), EnemySwatch.BarkDark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.5f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.16f, -0.06f), 0.025f, 0.015f, 4, EnemySwatch.BarkDark));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.36f, 0.2f, 0.3f, -0.32f, 0.07f, EnemySwatch.PeccaryFur, EnemySwatch.Black, "HillBoar");
        }

        private static void DrawHillBoarBody(EnemyMeshBuilder b)
        {
            // The boars of the hills root under the giants' oaks, and the giants left the acorns to them. Since the shadow they come down to the Fortim's pantry at night and eat the winter, pots and all.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.6f, 1.04f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, 0.08f, 0.24f), new Vector3(0.62f, 0.56f, 0.5f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0f, -0.16f, -0.02f), new Vector3(0.5f, 0.24f, 0.8f), EnemySwatch.Bark, coarse: true);
            // The ridge of bristles down the back, and the dried mud of the hillside on the flanks.
            for (int i = 0; i < 7; i++)
            {
                b.Cylinder(new Vector3(0f, 0.24f, 0.36f - i * 0.11f), 0.05f, 0f, 0.14f - i * 0.012f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(-24f, 0f, 0f));
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.28f * s, -0.12f, -0.16f), new Vector3(0.1f, 0.16f, 0.26f), EnemySwatch.SandstoneDark, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Corvo-Mensageiro de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeMessenger(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "SmokeMessenger", 0.6f, DrawSmokeMessengerBody, new Vector3(0f, 0.32f, 0.32f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.26f), EnemySwatch.Black);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.045f, 0.012f, 0.16f, 5, EnemySwatch.BatFurDark, rotation: Quaternion.Euler(96f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.04f, 0.07f, 0.05f, EnemySwatch.Sorcery);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.07f), new Vector3(0.06f, 0.02f, 0.14f), EnemySwatch.BatFurDark),
                EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.Shroud, 0.95f, EnemySwatch.Black, EnemySwatch.Shroud);
        }

        private static void DrawSmokeMessengerBody(EnemyMeshBuilder b)
        {
            // In the Book of the Giants two of the giants are sent off with their dreams to be read, and come back with the reading. The crows of the Fortim carried the small dreams the same way. Now they are smoke, and carry bad dreams back.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.48f, 0.74f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.06f, 0.1f), new Vector3(0.36f, 0.28f, 0.4f), EnemySwatch.Shadow, coarse: true);
            b.Segment(new Vector3(0f, 0.1f, 0.24f), new Vector3(0f, 0.3f, 0.3f), 0.09f, 0.08f, 6, EnemySwatch.Black);
            // The smoke it is half made of, rising off its back in two curls.
            b.Segment(new Vector3(0.04f, 0.2f, -0.1f), new Vector3(0.1f, 0.42f, -0.24f), 0.07f, 0.03f, 5, EnemySwatch.Shroud);
            b.Segment(new Vector3(-0.06f, 0.18f, -0.2f), new Vector3(-0.08f, 0.36f, -0.36f), 0.06f, 0.02f, 5, EnemySwatch.Shroud);
            // The little dream tablet tied to its leg, the sign on it still lit.
            b.Box(new Vector3(0.14f, -0.3f, 0.06f), new Vector3(0.04f, 0.12f, 0.1f), EnemySwatch.SandstoneLight, Quaternion.Euler(0f, 0f, 8f));
            b.Box(new Vector3(0.165f, -0.3f, 0.06f), new Vector3(0.01f, 0.06f, 0.05f), EnemySwatch.Rune);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.6f, 0.1f, EnemySwatch.BatFurDark, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Pica-Pau de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeWoodpecker(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "SmokeWoodpecker", 0.58f, DrawSmokeWoodpeckerBody, new Vector3(0f, 0.34f, 0.3f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.2f, 0.22f, 0.24f), EnemySwatch.Black);
                    b.Blob(new Vector3(0f, -0.04f, 0.04f), new Vector3(0.16f, 0.08f, 0.14f), EnemySwatch.Ivory, coarse: true);
                    // The red crest, and the long chisel of a beak that drums on the larches.
                    b.Blob(new Vector3(0f, 0.1f, -0.04f), new Vector3(0.1f, 0.1f, 0.16f), EnemySwatch.WarCape, coarse: true);
                    b.Cylinder(new Vector3(0f, 0f, 0.1f), 0.035f, 0.01f, 0.2f, 5, EnemySwatch.Horn, rotation: Quaternion.Euler(90f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.03f, 0.07f, 0.045f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.09f), new Vector3(0.05f, 0.02f, 0.18f), EnemySwatch.Horn),
                EnemySwatch.Black, EnemySwatch.Ivory, EnemySwatch.Shroud, 0.8f, EnemySwatch.Black, EnemySwatch.Shroud);
        }

        private static void DrawSmokeWoodpeckerBody(EnemyMeshBuilder b)
        {
            // The woodpeckers of the taiga take the beetles out of the trunk folk's bark, and the trunk folk sleep the better for it. Since the shadow their drumming sets the bark smoking, and the smoke stays in their feathers.
            b.Blob(Vector3.zero, new Vector3(0.44f, 0.46f, 0.66f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.06f, 0.1f), new Vector3(0.32f, 0.3f, 0.36f), EnemySwatch.Ivory, coarse: true);
            b.Segment(new Vector3(0f, 0.1f, 0.22f), new Vector3(0f, 0.3f, 0.28f), 0.08f, 0.07f, 6, EnemySwatch.Black);
            // The white bars on the back and the smoke off the shoulders, an ember of the bark it drummed caught in it.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0f, 0.2f, 0.06f - i * 0.12f), new Vector3(0.26f, 0.03f, 0.04f), EnemySwatch.Ivory);
            }

            b.Segment(new Vector3(0f, 0.2f, -0.16f), new Vector3(0.04f, 0.42f, -0.3f), 0.07f, 0.02f, 5, EnemySwatch.Shroud);
            b.Blob(new Vector3(0.03f, 0.36f, -0.26f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.58f, 0.1f, EnemySwatch.IronDark, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Fungo Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedFungus(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawDimmedFungusBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.3f, 0.18f), b =>
            {
                // The cloud of spores it lets go of, lit with the light it ate out of the wood; it flares before the spores burst.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.16f, 0.16f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0.04f, 0.04f, 0.02f), new Vector3(0.1f, 0.08f, 0.08f), EnemySwatch.FireflyGlow, coarse: true);
            });
        }

        private static void DrawDimmedFungusBody(EnemyMeshBuilder b)
        {
            // Under the larches of Haripushpa the fungus eats the dead wood and gives it back as earth, slowly. The shadow made it hungry for the living trees, and it walks out of the moss on its stump to sleep on the trunk folk.
            // The rotten larch stump it rides: the lowest point of the model.
            b.Cylinder(new Vector3(0f, -0.5f, 0f), 0.34f, 0.26f, 0.62f, 8, EnemySwatch.BarkDark, EnemySwatch.Bark, bottomCap: true);
            b.Blob(new Vector3(0f, -0.44f, 0f), new Vector3(0.84f, 0.12f, 0.8f), EnemySwatch.CaveMoss, coarse: true);
            // The shelf caps in tiers round the stump, and the big cap on top.
            float[] heights = { -0.24f, -0.06f, 0.12f };
            for (int i = 0; i < heights.Length; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    float a = (j * 120f + i * 50f) * Mathf.Deg2Rad;
                    var p = new Vector3(Mathf.Cos(a) * 0.3f, heights[i], Mathf.Sin(a) * 0.28f);
                    b.Blob(p, new Vector3(0.3f - i * 0.04f, 0.07f, 0.26f - i * 0.03f), j % 2 == 0 ? EnemySwatch.OwlBuff : EnemySwatch.SandstoneDark,
                        Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f), coarse: true);
                }
            }

            b.Blob(new Vector3(0f, 0.36f, 0f), new Vector3(0.7f, 0.26f, 0.66f), EnemySwatch.OwlBuff);
            b.Blob(new Vector3(0f, 0.28f, 0f), new Vector3(0.6f, 0.1f, 0.56f), EnemySwatch.IvoryDark, coarse: true);
            // The dull grey of the shadow spreading on the cap, and the lit gills underneath.
            b.Blob(new Vector3(-0.1f, 0.44f, 0.06f), new Vector3(0.36f, 0.12f, 0.3f), EnemySwatch.Shadow, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 10f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.22f, 0.24f, Mathf.Sin(a) * 0.2f), new Vector3(0.05f, 0.03f, 0.05f), EnemySwatch.FireflyGlow, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Lince da Taiga
        // ------------------------------------------------------------------

        public static void BuildTaigaLynx(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawTaigaLynxBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.46f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.26f, 0.26f), EnemySwatch.WolfFur);
                b.Blob(new Vector3(0f, -0.04f, 0.12f), new Vector3(0.16f, 0.12f, 0.12f), EnemySwatch.WolfBelly, coarse: true);
                b.Blob(new Vector3(0f, 0f, 0.18f), new Vector3(0.05f, 0.04f, 0.04f), EnemySwatch.Pink, coarse: true);
                // The ruff of the lynx and the black tufts on its ears.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.14f * s, -0.06f, -0.02f), new Vector3(0.1f, 0.16f, 0.12f), EnemySwatch.Ivory, coarse: true);
                    b.Cylinder(new Vector3(0.08f * s, 0.11f, -0.04f), 0.05f, 0.01f, 0.12f, 4, EnemySwatch.WolfFur);
                    b.Cylinder(new Vector3(0.08f * s, 0.22f, -0.04f), 0.012f, 0f, 0.08f, 3, EnemySwatch.Black);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.04f, 0.11f, 0.045f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.12f, 0.03f, 0.1f), EnemySwatch.WolfBelly);
                b.Cylinder(new Vector3(0.03f, 0.005f, 0.09f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.03f, 0.005f, 0.09f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.42f), b =>
            {
                // The bobtail, tipped black.
                b.Segment(Vector3.zero, new Vector3(0f, 0.02f, -0.12f), 0.05f, 0.04f, 5, EnemySwatch.WolfFur);
                b.Blob(new Vector3(0f, 0.02f, -0.14f), new Vector3(0.07f, 0.07f, 0.06f), EnemySwatch.Black, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.42f, 0.18f, 0.28f, -0.28f, 0.065f, EnemySwatch.WolfFur, EnemySwatch.Ivory, "TaigaLynx");
        }

        private static void DrawTaigaLynxBody(EnemyMeshBuilder b)
        {
            // The lynxes of the taiga are as old as the larches and as quiet. The shadow did not make them louder; it made them hungry, and they lie on the bark trails now and wait for the trunk folk to pass.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.44f, 0.9f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, -0.1f, 0.02f), new Vector3(0.42f, 0.22f, 0.66f), EnemySwatch.Ivory, coarse: true);
            // The dark spots on the flanks and the frost of the taiga on the back.
            for (int i = 0; i < 6; i++)
            {
                float s = i % 2 == 0 ? 1f : -1f;
                b.Blob(new Vector3(0.24f * s, 0.04f + (i % 3) * 0.04f, 0.24f - i * 0.1f), new Vector3(0.04f, 0.05f, 0.06f), EnemySwatch.WolfFurDark, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.2f, -0.06f), new Vector3(0.3f, 0.06f, 0.42f), EnemySwatch.GolemSnow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Garanhão Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedStallion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1f, 0f), DrawDimmedStallionBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.5f, 0.66f), b =>
            {
                // The long head bowed on the neck, the dark muzzle and the ember eyes.
                b.Blob(new Vector3(0f, 0f, 0.06f), new Vector3(0.2f, 0.24f, 0.4f), EnemySwatch.GolemStoneDark, Quaternion.Euler(30f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.12f, 0.24f), new Vector3(0.16f, 0.14f, 0.16f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.06f * s, 0.12f, -0.06f), 0.03f, 0f, 0.1f, 4, EnemySwatch.GolemStoneDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.04f, 0.06f, 0.04f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.16f, 0.2f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.12f, 0.03f, 0.12f), EnemySwatch.Black));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.16f, -0.62f), b =>
            {
                // The tail of smoke, hanging and frayed.
                b.Segment(Vector3.zero, new Vector3(0f, -0.3f, -0.18f), 0.08f, 0.06f, 5, EnemySwatch.Shroud);
                b.Segment(new Vector3(0f, -0.3f, -0.18f), new Vector3(0.02f, -0.56f, -0.2f), 0.06f, 0.02f, 5, EnemySwatch.Shadow);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.82f, 0.17f, 0.44f, -0.44f, 0.07f, EnemySwatch.GolemStone, EnemySwatch.Black, "DimmedStallion");
        }

        private static void DrawDimmedStallionBody(EnemyMeshBuilder b)
        {
            // On the Thousand-Horse Prairie the herds run free and come to the Narts' cup to drink. The shadow went into the stallions first, the ones that lead, and they lead the herd away from the well now, or at whoever is near it.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.6f, 1.3f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.16f, 0.02f), new Vector3(0.44f, 0.26f, 1f), EnemySwatch.GolemStoneDark, coarse: true);
            // The dapples of the grey, gone dark at the edges.
            for (int i = 0; i < 6; i++)
            {
                float s = i % 2 == 0 ? 1f : -1f;
                b.Blob(new Vector3(0.24f * s, 0.06f, 0.4f - i * 0.16f), new Vector3(0.06f, 0.08f, 0.1f), EnemySwatch.GolemStoneDark, coarse: true);
            }

            // The neck up to the head, and the mane of smoke along it.
            b.Segment(new Vector3(0f, 0.16f, 0.46f), new Vector3(0f, 0.5f, 0.62f), 0.17f, 0.11f, 6, EnemySwatch.GolemStone);
            for (int i = 0; i < 5; i++)
            {
                float f = i / 4f;
                Vector3 root = Vector3.Lerp(new Vector3(0f, 0.28f, 0.4f), new Vector3(0f, 0.6f, 0.58f), f);
                b.Segment(root, root + new Vector3(0.06f, 0.04f, -0.16f), 0.05f, 0.015f, 4, i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Shadow);
            }

            b.Blob(new Vector3(0.05f, 0.52f, 0.54f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Águia de Ferro
        // ------------------------------------------------------------------

        public static void BuildIronEagle(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "IronEagle", 0.66f, DrawIronEagleBody, new Vector3(0f, 0.36f, 0.36f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.28f), EnemySwatch.Iron);
                    b.Blob(new Vector3(0f, 0.06f, -0.04f), new Vector3(0.2f, 0.12f, 0.2f), EnemySwatch.IronDark, coarse: true);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.11f), 0.05f, 0.012f, 0.14f, 5, EnemySwatch.Steel, rotation: Quaternion.Euler(104f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.04f, 0.08f, 0.05f, EnemySwatch.WarEye);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.12f), EnemySwatch.Steel),
                EnemySwatch.Iron, EnemySwatch.IronDark, EnemySwatch.Steel, 1.25f, EnemySwatch.IronDark, EnemySwatch.Iron);
        }

        private static void DrawIronEagleBody(EnemyMeshBuilder b)
        {
            // The tale of the chained hero says a bird came every day to the mountain. Whatever it was, the eagles of these crags nest on the old chain and lined their nests with its rust, and the shadow turned the rust into feathers.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.52f, 0.82f), EnemySwatch.Iron);
            b.Blob(new Vector3(0f, -0.08f, 0.12f), new Vector3(0.42f, 0.3f, 0.42f), EnemySwatch.IronDark, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.26f), new Vector3(0f, 0.34f, 0.34f), 0.09f, 0.08f, 6, EnemySwatch.Iron);
            // The iron plates for feathers, riveted down the back, with rust at the seams.
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.22f - i * 0.02f, 0.14f - i * 0.12f), new Vector3(0.3f - i * 0.03f, 0.03f, 0.12f), i % 2 == 0 ? EnemySwatch.Steel : EnemySwatch.Iron, Quaternion.Euler(-8f, 0f, 0f));
                b.Blob(new Vector3(0.12f, 0.24f - i * 0.02f, 0.14f - i * 0.12f), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.CopperDark, coarse: true);
            }

            // A link of the old chain round its leg, and the glow of the light that already climbed it.
            b.Cylinder(new Vector3(-0.11f, -0.4f, 0.04f), 0.07f, 0.07f, 0.04f, 6, EnemySwatch.IronDark);
            b.Blob(new Vector3(-0.11f, -0.36f, 0.04f), new Vector3(0.04f, 0.03f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.66f, 0.11f, EnemySwatch.IronDark, EnemySwatch.Steel);
        }

        // ------------------------------------------------------------------
        // Urso do Cáucaso Apagado
        // ------------------------------------------------------------------

        public static void BuildCaucasusBear(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawCaucasusBearBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.1f, 0.68f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.38f, 0.34f, 0.36f), EnemySwatch.WolfFurDark);
                b.Cylinder(new Vector3(0f, -0.05f, 0.12f), 0.1f, 0.08f, 0.16f, 6, EnemySwatch.WolfFur, EnemySwatch.WolfFur, Quaternion.Euler(90f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.04f, 0.29f), new Vector3(0.08f, 0.06f, 0.04f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.14f * s, 0.16f, -0.04f), new Vector3(0.1f, 0.1f, 0.05f), EnemySwatch.WolfFurDark, coarse: true);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.05f, 0.15f, 0.04f, EnemySwatch.Ember);
            }, Quaternion.Euler(10f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.13f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.1f), new Vector3(0.18f, 0.05f, 0.2f), EnemySwatch.WolfFur);
                b.Cylinder(new Vector3(0.05f, 0.01f, 0.18f), 0.015f, 0f, 0.05f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.05f, 0.01f, 0.18f), 0.015f, 0f, 0.05f, 3, EnemySwatch.Fang);
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.58f, 0.22f, 0.42f, -0.42f, 0.11f, EnemySwatch.WolfFurDark, EnemySwatch.Black, "CaucasusBear");
        }

        private static void DrawCaucasusBearBody(EnemyMeshBuilder b)
        {
            // The bears of the Caucasus sleep the winter in the caves under the chain, and the svan towers left honey out for them in spring. The shadow woke one too early, and it walks the crags in the snow wearing a broken link of the chain like a collar.
            b.Blob(Vector3.zero, new Vector3(0.8f, 0.76f, 1.3f), EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0f, 0.28f, 0.32f), new Vector3(0.6f, 0.42f, 0.5f), EnemySwatch.Black, coarse: true);
            b.Blob(new Vector3(0f, -0.16f, 0f), new Vector3(0.6f, 0.36f, 0.96f), EnemySwatch.WolfFur, coarse: true);
            // The snow on its back, and the broken link round the neck with the light of the chain still in it.
            b.Blob(new Vector3(-0.06f, 0.34f, -0.16f), new Vector3(0.4f, 0.1f, 0.46f), EnemySwatch.GolemSnow, coarse: true);
            b.Cylinder(new Vector3(0f, 0.04f, 0.5f), 0.32f, 0.32f, 0.08f, 8, EnemySwatch.IronDark, rotation: Quaternion.Euler(80f, 0f, 0f));
            b.Blob(new Vector3(0.18f, -0.14f, 0.56f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            b.Blob(new Vector3(0f, 0.04f, -0.64f), new Vector3(0.1f, 0.1f, 0.08f), EnemySwatch.Black, coarse: true);
        }

        // ------------------------------------------------------------------
        // Lobo de Neve
        // ------------------------------------------------------------------

        public static void BuildSnowWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.58f, 0f), DrawSnowWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.52f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.28f, 0.3f), EnemySwatch.GolemSnow);
                b.Blob(new Vector3(0f, -0.04f, 0.2f), new Vector3(0.15f, 0.13f, 0.24f), EnemySwatch.Ivory, coarse: true);
                b.Blob(new Vector3(0f, -0.01f, 0.32f), new Vector3(0.06f, 0.05f, 0.05f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.09f * s, 0.12f, -0.04f), 0.05f, 0f, 0.15f, 4, EnemySwatch.IvoryDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.13f, 0.05f, EnemySwatch.Crystal);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.12f), new Vector3(0.13f, 0.04f, 0.22f), EnemySwatch.IvoryDark);
                b.Cylinder(new Vector3(0.04f, 0.005f, 0.2f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.005f, 0.2f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.46f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.14f, -0.32f), 0.09f, 0.06f, 5, EnemySwatch.GolemSnow);
                b.Blob(new Vector3(0f, -0.15f, -0.34f), new Vector3(0.09f, 0.09f, 0.11f), EnemySwatch.Ivory, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.48f, 0.2f, 0.32f, -0.32f, 0.07f, EnemySwatch.GolemSnow, EnemySwatch.IvoryDark, "SnowWolf");
        }

        private static void DrawSnowWolfBody(EnemyMeshBuilder b)
        {
            // The snow wolves live above the svan towers and come down only in the worst winters. Since the shadow and Ohya on the crest, every winter is the worst, and they hunt the chain's climbers in the white.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.5f, 1f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, -0.12f, 0.02f), new Vector3(0.46f, 0.24f, 0.72f), EnemySwatch.Ivory, coarse: true);
            // The thick ruff of the winter coat with frost grown into it, and a grey saddle where the shadow lies.
            b.Blob(new Vector3(0f, 0.06f, 0.34f), new Vector3(0.58f, 0.46f, 0.36f), EnemySwatch.Ivory, coarse: true);
            b.Box(new Vector3(0f, 0.22f, -0.06f), new Vector3(0.22f, 0.04f, 0.52f), EnemySwatch.GolemStoneDark);
            for (int i = 0; i < 3; i++)
            {
                b.Crystal(new Vector3(-0.12f + i * 0.12f, 0.22f, 0.34f), 0.025f, 0.1f, EnemySwatch.Crystal, Quaternion.Euler(-20f, 0f, -20f + i * 20f));
            }
        }
    }
}
