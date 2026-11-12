using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The fourteenth pass of beasts (bible §3.2.1): the most-seen roles of Regions 10–16 that the continuous world still played with stand-ins,
    /// low-poly from the shared palette like <see cref="MidStandInBeastRecipes"/> (the birds share its skeleton). Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Abutre do Pico</b> (54, ~1.3 m): the dark vulture of Farnkesh's pinnacles, a ruff of cloud round its neck. Birds' parts.</item>
    /// <item><b>Pipa-Sombra</b> (55, ~1.3 m): a paper kite of shadow cut loose from the wind bridges, a painted face, its ribbon trailing on the ground.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c> (the painted mouth), the side streamers as <c>Wing_L</c>/<c>Wing_R</c>, the ribbon as <c>Tail</c>.</item>
    /// <item><b>Gelo Flutuante</b> (63, ~0.9 m): a slab of pack ice with the aurora caught inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Kul Pequeno</b> (64, ~0.9 m): a little water spirit of the fishing holes, wet hair to the ice, a lamp's light in its chest. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Chama Faminta</b> (70, 75, 85, ~1 m): a flame with a mouth on a heap of ash, the light it ate burning in the middle. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Pedra de Diorito</b> (81, ~0.8 m): a speckled block of Ullikummi's stone that crawls on stubs. <c>Body</c> → <c>StolenLight</c> (the lit eye).</item>
    /// <item><b>Ave do Ombro</b> (81, ~1.1 m): a stone-grey bird with lichen on its back, from the giant's shoulder. Birds' parts.</item>
    /// <item><b>Pássaro-Sonho</b> (82, ~1.2 m): a night-blue bird with marks like the dream tablets on its wings. Birds' parts.</item>
    /// <item><b>Sombra Pequena</b> (83, ~0.8 m): a child-sized shadow on all fours with ember eyes and a stolen wick. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Morcego da Treva</b> (83, ~1 m): a bat of the Dark with ears like horns. Birds' parts (the wings are membranes).</item>
    /// <item><b>Vento de Órbita</b> (95, ~1.2 m): a ring of wind round a lit core over a little funnel of dust. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Lanterna Fugitiva</b> (92, ~1.2 m): a paper lantern that ran off a Barca de Luz, its tassel on the ground. <c>Body</c> → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class FarStandInBeastRecipes
    {
        // ------------------------------------------------------------------
        // Abutre do Pico
        // ------------------------------------------------------------------

        public static void BuildPeakVulture(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "PeakVulture", 0.64f, DrawPeakVultureBody, new Vector3(0f, 0.4f, 0.38f), b =>
                {
                    // The bare head of a vulture, a hooked beak and the ember of the wind of ruin in the eyes.
                    b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.26f), EnemySwatch.SandstoneDark);
                    b.Cylinder(new Vector3(0f, 0f, 0.1f), 0.045f, 0.012f, 0.15f, 5, EnemySwatch.Horn, rotation: Quaternion.Euler(96f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.04f, 0.07f, 0.05f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.07f), new Vector3(0.06f, 0.02f, 0.14f), EnemySwatch.Horn, Quaternion.Euler(10f, 0f, 0f)),
                EnemySwatch.Shadow, EnemySwatch.IronDark, EnemySwatch.GolemSnow, 1.05f, EnemySwatch.Shadow, EnemySwatch.IronDark);
        }

        private static void DrawPeakVultureBody(EnemyMeshBuilder b)
        {
            // The vultures of the pinnacles kept the peaks clean and the stairs free. The wind of ruin settled in their ruffs, and now they keep the stairs from the climbers.
            b.Blob(Vector3.zero, new Vector3(0.58f, 0.52f, 0.84f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.08f, 0.14f), new Vector3(0.42f, 0.32f, 0.42f), EnemySwatch.IronDark, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.26f), new Vector3(0f, 0.38f, 0.36f), 0.08f, 0.06f, 6, EnemySwatch.SandstoneDark);
            // The ruff of cloud round the base of the neck, with frost caught in it.
            for (int i = 0; i < 6; i++)
            {
                float a = i / 6f * Mathf.PI * 2f;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.14f, 0.16f, 0.24f + Mathf.Sin(a) * 0.1f), new Vector3(0.14f, 0.1f, 0.12f), EnemySwatch.GolemSnow, coarse: true);
            }

            b.Blob(new Vector3(0.1f, 0.2f, 0.32f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Crystal, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.64f, 0.11f, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Pipa-Sombra
        // ------------------------------------------------------------------

        private const float KiteY = 0.9f;
        private static readonly Quaternion KiteTilt = Quaternion.Euler(-35f, 0f, 0f);

        public static void BuildShadowKite(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, KiteY, 0f), DrawShadowKiteBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.1f, 0.08f), b =>
            {
                // The face the bridge children painted on it, gone dark; the eyes still lit.
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.22f, 0.06f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.04f, 0.03f, 0.05f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.03f), b =>
            {
                // The painted mouth, a flap of paper that drops open before it spits.
                b.Box(new Vector3(0f, -0.015f, 0f), new Vector3(0.14f, 0.03f, 0.02f), EnemySwatch.Black);
                b.Box(new Vector3(0f, -0.02f, 0.012f), new Vector3(0.1f, 0.015f, 0.01f), EnemySwatch.Ember);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, KiteTilt * new Vector3(0.3f * side, 0.05f, 0f), b =>
                {
                    // The streamers on the side corners, cut from the same shadow-paper.
                    b.TwoSided(Vector3.zero, new Vector3(0.3f * side, 0.06f, -0.04f), new Vector3(0.2f * side, -0.12f, 0f), EnemySwatch.Shadow, EnemySwatch.IronDark);
                    b.Box(new Vector3(0.28f * side, 0.05f, -0.035f), new Vector3(0.05f, 0.02f, 0.02f), EnemySwatch.WarCape);
                }, meshKey: s < 0 ? "ShadowKiteStreamer_L" : "ShadowKiteStreamer_R");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, KiteTilt * new Vector3(0f, -0.42f, 0f), DrawShadowKiteRibbon);
        }

        private static void DrawShadowKiteBody(EnemyMeshBuilder b)
        {
            // The children of Farnkesh flew kites from the wind bridges. When the ropes were cut the kites went on flying on their own, and the shadow got into the paper.
            b.Matrix = Matrix4x4.Rotate(KiteTilt);
            var top = new Vector3(0f, 0.45f, 0f);
            var right = new Vector3(0.32f, 0.05f, 0f);
            var bottom = new Vector3(0f, -0.42f, 0f);
            var left = new Vector3(-0.32f, 0.05f, 0f);
            var centre = new Vector3(0f, 0.05f, 0.02f);
            b.TwoSided(top, right, centre, EnemySwatch.Shadow, EnemySwatch.IronDark);
            b.TwoSided(top, centre, left, EnemySwatch.BatFurDark, EnemySwatch.IronDark);
            b.TwoSided(centre, right, bottom, EnemySwatch.BatFurDark, EnemySwatch.IronDark);
            b.TwoSided(centre, bottom, left, EnemySwatch.Shadow, EnemySwatch.IronDark);
            // The cross of sticks behind the paper, and the stolen lamp-light glued in the middle where the sticks cross.
            b.Box(new Vector3(0f, 0.015f, -0.02f), new Vector3(0.025f, 0.88f, 0.025f), EnemySwatch.Bark);
            b.Box(new Vector3(0f, 0.05f, -0.02f), new Vector3(0.64f, 0.025f, 0.025f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, 0.05f, -0.04f), new Vector3(0.12f, 0.12f, 0.06f), EnemySwatch.StolenLight);
            b.Blob(new Vector3(0f, 0.3f, 0.01f), new Vector3(0.08f, 0.08f, 0.03f), EnemySwatch.WarCape, coarse: true);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawShadowKiteRibbon(EnemyMeshBuilder b)
        {
            // The ribbon trails back over the ground with its bows; the last bow is the lowest point of the model.
            var points = new[] { Vector3.zero, new Vector3(0.03f, -0.18f, -0.18f), new Vector3(-0.03f, -0.34f, -0.36f), new Vector3(0.02f, -0.46f, -0.56f), new Vector3(0f, -0.5f, -0.72f) };
            for (int i = 0; i < points.Length - 1; i++)
            {
                b.Segment(points[i], points[i + 1], 0.014f, 0.012f, 4, EnemySwatch.Black);
            }

            for (int i = 1; i < points.Length; i++)
            {
                b.Box(points[i], new Vector3(0.1f, 0.03f, 0.03f), i % 2 == 0 ? EnemySwatch.WarCape : EnemySwatch.Shadow, Quaternion.Euler(0f, 0f, 30f));
            }

            b.Blob(new Vector3(0f, -0.5f, -0.72f), new Vector3(0.06f, 0.04f, 0.06f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Gelo Flutuante
        // ------------------------------------------------------------------

        public static void BuildDriftingFloe(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawDriftingFloeBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.08f, 0.1f), b =>
            {
                // The ribbon of aurora frozen inside; it flares before the slab comes down on you.
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.14f, 0.2f), EnemySwatch.Rune);
                b.Blob(new Vector3(0.08f, 0.02f, 0.02f), new Vector3(0.14f, 0.08f, 0.1f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawDriftingFloeBody(EnemyMeshBuilder b)
        {
            // On the night of the aurora a slab of the pack broke off with a piece of the light inside. It drifts after the boats now, and leans on whatever is warm.
            b.Box(Vector3.zero, new Vector3(1f, 0.3f, 0.82f), EnemySwatch.GolemStone, Quaternion.Euler(0f, 8f, 3f), top: EnemySwatch.GolemSnow);
            b.Blob(new Vector3(-0.2f, 0.12f, -0.12f), new Vector3(0.44f, 0.22f, 0.4f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0.22f, 0.1f, 0.14f), new Vector3(0.36f, 0.18f, 0.32f), EnemySwatch.GolemSnow, coarse: true);
            // The broken edges stand up in shards of clear ice.
            float[] xs = { -0.36f, -0.12f, 0.16f, 0.34f, -0.26f, 0.06f };
            float[] zs = { 0.24f, 0.3f, 0.26f, -0.18f, -0.26f, -0.3f };
            for (int i = 0; i < xs.Length; i++)
            {
                b.Cylinder(new Vector3(xs[i], 0.12f, zs[i]), 0.07f, 0f, 0.26f + (i % 3) * 0.08f, 4, i % 2 == 0 ? EnemySwatch.Crystal : EnemySwatch.GolemSnow,
                    rotation: Quaternion.Euler(zs[i] * 40f, 0f, -xs[i] * 40f));
            }

            // The keel of old blue ice under the water line: the lowest point of the model.
            b.Blob(new Vector3(0f, -0.16f, 0f), new Vector3(0.74f, 0.28f, 0.6f), EnemySwatch.CaveSkin);
        }

        // ------------------------------------------------------------------
        // Kul Pequeno
        // ------------------------------------------------------------------

        public static void BuildLittleKul(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawLittleKulBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.22f), b =>
            {
                // The fisherman's lamp-light it pulled under the ice, still burning in its chest.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0f, 0.03f), new Vector3(0.08f, 0.08f, 0.05f), EnemySwatch.FireflyGlow, coarse: true);
            });
        }

        private static void DrawLittleKulBody(EnemyMeshBuilder b)
        {
            // The kul are the water's own people in the Komi tales, and they keep the fish. When the Old Kul soured, the little ones came up through the fishing holes after the lamps.
            b.Blob(Vector3.zero, new Vector3(0.52f, 0.66f, 0.46f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, 0.22f, 0.04f), new Vector3(0.38f, 0.3f, 0.36f), EnemySwatch.CaveSkin, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.24f, 0.22f, 0.07f, EnemySwatch.MoonEye);
            // The long wet hair hanging down the back to the ice.
            for (int i = 0; i < 5; i++)
            {
                float x = -0.16f + i * 0.08f;
                b.Segment(new Vector3(x * 0.7f, 0.3f, -0.1f), new Vector3(x, -0.36f, -0.24f), 0.035f, 0.015f, 4, i % 2 == 0 ? EnemySwatch.Black : EnemySwatch.CaveMoss);
            }

            // Webbed hands held out in front, and the puddle it stands in: the lowest point of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.22f * s, 0.02f, 0.06f), new Vector3(0.3f * s, -0.14f, 0.2f), 0.04f, 0.03f, 4, EnemySwatch.CaveSkinDark);
                b.Box(new Vector3(0.31f * s, -0.17f, 0.22f), new Vector3(0.08f, 0.03f, 0.08f), EnemySwatch.Crystal);
            }

            b.Cylinder(new Vector3(0f, -0.4f, 0f), 0.32f, 0.3f, 0.03f, 8, EnemySwatch.Crystal, bottomCap: true);
        }

        // ------------------------------------------------------------------
        // Chama Faminta
        // ------------------------------------------------------------------

        public static void BuildHungryFlame(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.45f, 0f), DrawHungryFlameBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0f), b =>
            {
                // The light it ate, at the heart of the fire; it flares before the bite.
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.34f, 0.28f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.14f, 0.16f, 0.14f), EnemySwatch.GolemSnow, coarse: true);
            });
        }

        private static void DrawHungryFlameBody(EnemyMeshBuilder b)
        {
            // The Devouring Fire is the Dark's fire, the one that eats and is never warm. Its flames come loose from the caldera and go looking for more to eat.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.56f, 0.46f), EnemySwatch.Ember);
            // Tongues of flame licking up, the tallest at the back.
            float[] angles = { 0f, 60f, 120f, 180f, 240f, 300f };
            for (int i = 0; i < angles.Length; i++)
            {
                float a = angles[i] * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Sin(a) * 0.14f, 0.12f, Mathf.Cos(a) * 0.12f);
                b.Cylinder(at, 0.11f, 0f, 0.3f + (i % 2) * 0.12f, 5, i % 2 == 0 ? EnemySwatch.GolemCrack : EnemySwatch.Ember,
                    rotation: Quaternion.Euler(-Mathf.Cos(a) * 18f, 0f, Mathf.Sin(a) * 18f));
            }

            b.Cylinder(new Vector3(0f, 0.2f, -0.06f), 0.12f, 0f, 0.4f, 5, EnemySwatch.WarEye);
            // The mouth: a black gap that never closes, with a row of embers for teeth.
            b.Box(new Vector3(0f, -0.02f, 0.22f), new Vector3(0.24f, 0.1f, 0.04f), EnemySwatch.Black);
            for (int i = 0; i < 4; i++)
            {
                b.Cylinder(new Vector3(-0.08f + i * 0.055f, 0.03f, 0.24f), 0.015f, 0f, 0.04f, 3, EnemySwatch.Ember, rotation: Quaternion.Euler(180f, 0f, 0f));
            }

            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.1f, 0.2f, 0.06f, EnemySwatch.Black);
            // The heap of ash and coals it squats on: the lowest point of the model.
            b.Blob(new Vector3(0f, -0.35f, 0f), new Vector3(0.62f, 0.2f, 0.6f), EnemySwatch.Black);
            for (int i = 0; i < 5; i++)
            {
                float a = i / 5f * Mathf.PI * 2f;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.22f, -0.3f, Mathf.Sin(a) * 0.22f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.GolemCrack, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Pedra de Diorito
        // ------------------------------------------------------------------

        public static void BuildDioriteStone(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawDioriteStoneBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.08f, 0.4f), b =>
            {
                // One eye, cut like the eye of the stone child in the old song, lit from inside; it widens before the stone throws itself forward.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.12f, 0.06f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.08f, 0.08f, 0.04f), EnemySwatch.Black, coarse: true);
            });
        }

        private static void DrawDioriteStoneBody(EnemyMeshBuilder b)
        {
            // Ullikummi was a child of diorite that grew on the giant's shoulder. Chips of him broke off as he grew, and they crawl about the shoulder looking for the rest of themselves.
            b.Box(new Vector3(0f, 0.02f, 0f), new Vector3(0.82f, 0.62f, 0.76f), EnemySwatch.IronDark, Quaternion.Euler(4f, 6f, -3f), top: EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.3f, -0.04f), new Vector3(0.6f, 0.24f, 0.56f), EnemySwatch.GolemStoneDark, coarse: true);
            // The salt-and-pepper speckle of diorite.
            for (int i = 0; i < 14; i++)
            {
                float x = -0.32f + (i * 0.37f % 0.64f);
                float y = -0.18f + (i * 0.23f % 0.4f);
                b.Box(new Vector3(x, y, 0.38f), new Vector3(0.05f, 0.04f, 0.02f), i % 3 == 0 ? EnemySwatch.Ivory : EnemySwatch.GolemSnow);
                b.Box(new Vector3(0.4f, y, x), new Vector3(0.02f, 0.04f, 0.05f), EnemySwatch.Ivory);
            }

            // A crack of light where it broke off the giant.
            b.Box(new Vector3(-0.2f, 0.12f, 0.385f), new Vector3(0.03f, 0.22f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 24f));
            // Four stubs of stone it crawls on: the lowest point of the model.
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    b.Blob(new Vector3(0.28f * x, -0.32f, 0.24f * z), new Vector3(0.22f, 0.16f, 0.24f), EnemySwatch.Black, coarse: true);
                }
            }
        }

        // ------------------------------------------------------------------
        // Ave do Ombro
        // ------------------------------------------------------------------

        public static void BuildShoulderBird(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "ShoulderBird", 0.58f, DrawShoulderBirdBody, new Vector3(0f, 0.34f, 0.34f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.24f), EnemySwatch.GolemStone);
                    b.Cylinder(new Vector3(0f, 0f, 0.1f), 0.04f, 0.01f, 0.13f, 5, EnemySwatch.IronDark, rotation: Quaternion.Euler(88f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.04f, 0.07f, 0.045f, EnemySwatch.Crystal);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.05f, 0.02f, 0.12f), EnemySwatch.IronDark),
                EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, EnemySwatch.Lichen, 0.9f, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone);
        }

        private static void DrawShoulderBirdBody(EnemyMeshBuilder b)
        {
            // Upelluri carries the sky and the earth on his shoulder and never felt the birds that nest there. The stone child grew in their nests, and the birds took the colour of the stone and its temper.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.46f, 0.74f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.08f, 0.12f), new Vector3(0.36f, 0.28f, 0.38f), EnemySwatch.GolemSnow, coarse: true);
            // Lichen and a tuft of moss growing on its back, the way they grow on the giant.
            b.Blob(new Vector3(0.06f, 0.2f, -0.06f), new Vector3(0.2f, 0.08f, 0.24f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(-0.08f, 0.19f, 0.08f), new Vector3(0.12f, 0.06f, 0.12f), EnemySwatch.CaveMoss, coarse: true);
            b.Blob(new Vector3(0f, 0.2f, 0.2f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.GolemCrack, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.58f, 0.09f, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Pássaro-Sonho
        // ------------------------------------------------------------------

        public static void BuildDreamBird(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "DreamBird", 0.62f, DrawDreamBirdBody, new Vector3(0f, 0.36f, 0.34f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), EnemySwatch.CaveSkin);
                    // A crest of three long plumes, a soft lit eye that never quite closes.
                    for (int i = -1; i <= 1; i++)
                    {
                        b.Segment(new Vector3(0.03f * i, 0.08f, -0.04f), new Vector3(0.06f * i, 0.24f, -0.14f), 0.018f, 0.006f, 4, EnemySwatch.CaveSkinDark);
                    }

                    b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.035f, 0.01f, 0.1f, 5, EnemySwatch.Gold, rotation: Quaternion.Euler(88f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.03f, 0.07f, 0.06f, EnemySwatch.SoftEye);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.05f), new Vector3(0.05f, 0.02f, 0.1f), EnemySwatch.Gold),
                EnemySwatch.CaveSkinDark, EnemySwatch.CaveBelly, EnemySwatch.SoftEye, 1f, EnemySwatch.CaveSkinDark, EnemySwatch.CaveSkin);
        }

        private static void DrawDreamBirdBody(EnemyMeshBuilder b)
        {
            // The old kings wrote their dreams on clay so they would not forget them. A bird flew out of one of those dreams and kept the writing on its feathers; the shadow broke the tablets, and it lost its way back.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.46f, 0.74f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, -0.08f, 0.12f), new Vector3(0.34f, 0.28f, 0.38f), EnemySwatch.CaveBelly, coarse: true);
            // The wedge-shaped marks of the tablets along its flanks.
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    b.Cylinder(new Vector3(0.235f * s, 0.06f - (i % 2) * 0.06f, 0.18f - i * 0.1f), 0.02f, 0f, 0.05f, 3, EnemySwatch.SandstoneLight,
                        rotation: Quaternion.Euler(0f, 0f, 90f * s));
                }
            }

            b.Blob(new Vector3(0f, 0.18f, -0.1f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.SoftEye, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.62f, 0.09f, EnemySwatch.CaveSkinDark, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Sombra Pequena
        // ------------------------------------------------------------------

        public static void BuildLittleShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawLittleShadowBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.38f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.34f, 0.3f, 0.3f), EnemySwatch.Black);
                // Two little horns of smoke, and the ember eyes too big for the face.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.09f * s, 0.12f, -0.02f), 0.04f, 0f, 0.14f, 4, EnemySwatch.Shroud, rotation: Quaternion.Euler(-16f, 0f, -18f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.03f, 0.13f, 0.08f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.16f, 0.035f, 0.12f), EnemySwatch.Shadow);
                // The candle wick it stole, still smouldering in its teeth.
                b.Segment(new Vector3(-0.1f, 0.01f, 0.08f), new Vector3(0.1f, 0.01f, 0.1f), 0.012f, 0.012f, 4, EnemySwatch.Ivory);
                b.Blob(new Vector3(0.11f, 0.01f, 0.1f), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.Ember, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.36f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.04f, 0.1f, -0.22f), 0.07f, 0.03f, 5, EnemySwatch.Shadow);
                b.Blob(new Vector3(0.05f, 0.12f, -0.26f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.Shroud, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.36f, 0.2f, 0.26f, -0.24f, 0.06f, EnemySwatch.Shadow, EnemySwatch.Black, "LittleShadow");
        }

        private static void DrawLittleShadowBody(EnemyMeshBuilder b)
        {
            // The little shadows are what the fortress lamps throw on the wall, the ones that got up and walked off when the lamps were looking the other way. They steal wicks, because a wick is the only thing they are afraid of.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.42f, 0.78f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.08f, 0.16f), new Vector3(0.44f, 0.32f, 0.36f), EnemySwatch.Black, coarse: true);
            // Wisps lifting off its back like smoke off a snuffed candle.
            for (int i = 0; i < 4; i++)
            {
                b.Cylinder(new Vector3(-0.09f + i * 0.06f, 0.16f, 0.12f - i * 0.12f), 0.05f, 0f, 0.14f + (i % 2) * 0.06f, 4, EnemySwatch.Shroud, rotation: Quaternion.Euler(-24f, 0f, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Morcego da Treva
        // ------------------------------------------------------------------

        public static void BuildDarkBat(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "DarkBat", 0.56f, DrawDarkBatBody, new Vector3(0f, 0.22f, 0.3f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.24f), EnemySwatch.VampFur);
                    // Ears tall as horns, a pug snout, the lamp-thief's lit eyes.
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.08f * s, 0.08f, -0.02f), 0.05f, 0f, 0.2f, 4, EnemySwatch.VampWing, rotation: Quaternion.Euler(-10f, 0f, -14f * s));
                    }

                    b.Blob(new Vector3(0f, -0.02f, 0.11f), new Vector3(0.1f, 0.07f, 0.06f), EnemySwatch.Pink, coarse: true);
                    HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.04f, 0.1f, 0.05f, EnemySwatch.BatEye);
                },
                new Vector3(0f, -0.06f, 0.08f), b =>
                {
                    b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.12f, 0.03f, 0.08f), EnemySwatch.VampFur);
                    b.Cylinder(new Vector3(0.03f, 0.005f, 0.07f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                    b.Cylinder(new Vector3(-0.03f, 0.005f, 0.07f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                },
                EnemySwatch.VampWing, EnemySwatch.BatWing, EnemySwatch.Black, 1.1f, EnemySwatch.VampFur, EnemySwatch.VampWing);
        }

        private static void DrawDarkBatBody(EnemyMeshBuilder b)
        {
            // The bats of the Dark hang in the arrow slits of the last fortress and drink the lamps one by one. They come in a cloud, and each one alone is small.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.42f, 0.54f), EnemySwatch.VampFur);
            b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.3f, 0.26f, 0.3f), EnemySwatch.BatFurDark, coarse: true);
            b.Blob(new Vector3(0f, -0.02f, 0.2f), new Vector3(0.06f, 0.06f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.56f, 0.08f, EnemySwatch.BatFurDark, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Vento de Órbita
        // ------------------------------------------------------------------

        public static void BuildOrbitWind(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawOrbitWindBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.06f, 0f), b =>
            {
                // The little light the wind still circles, as it was taught to; it flares before the gust.
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), EnemySwatch.StolenLight);
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.GolemSnow, coarse: true);
            });
        }

        private static void DrawOrbitWindBody(EnemyMeshBuilder b)
        {
            // The winds of the spheres turn the lights round the Column, each on its own path. The archons knocked this one off its orbit, and it circles whatever comes near instead.
            for (int ring = 0; ring < 2; ring++)
            {
                Quaternion tilt = Quaternion.Euler(ring == 0 ? 18f : -24f, ring * 40f, ring == 0 ? 0f : 16f);
                float radius = ring == 0 ? 0.42f : 0.32f;
                for (int i = 0; i < 12; i++)
                {
                    float a = i / 12f * Mathf.PI * 2f;
                    Vector3 at = tilt * new Vector3(Mathf.Cos(a) * radius, 0.06f, Mathf.Sin(a) * radius);
                    Quaternion along = tilt * Quaternion.Euler(0f, -a * Mathf.Rad2Deg, 0f);
                    b.Box(at, new Vector3(0.05f, 0.04f, 0.22f), i % 3 == 0 ? EnemySwatch.Crystal : ring == 0 ? EnemySwatch.Shroud : EnemySwatch.GolemSnow, along);
                }
            }

            // A puff of cloud above, and the little funnel of dust it spins on the ground: the lowest point of the model.
            b.Blob(new Vector3(0f, 0.3f, 0f), new Vector3(0.3f, 0.14f, 0.26f), EnemySwatch.GolemSnow, coarse: true);
            b.Cylinder(new Vector3(0f, -0.6f, 0f), 0.05f, 0.26f, 0.56f, 8, EnemySwatch.Shroud, twist: 40f);
            b.Cylinder(new Vector3(0f, -0.6f, 0f), 0.08f, 0.04f, 0.04f, 6, EnemySwatch.Sandstone, bottomCap: true);
        }

        // ------------------------------------------------------------------
        // Lanterna Fugitiva
        // ------------------------------------------------------------------

        public static void BuildRunawayLantern(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.64f, 0f), DrawRunawayLanternBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0f, 0.22f), b =>
            {
                // The flame inside shows through a tear in the front; it flares before the lantern rams you.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.2f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, -0.02f, 0.02f), new Vector3(0.08f, 0.1f, 0.06f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawRunawayLanternBody(EnemyMeshBuilder b)
        {
            // The Barcas de Luz carry the gathered light up the Column in paper lanterns. One of them did not want to go up, ran off the deck and has been hiding in the clouds ever since.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.58f, 0.56f), EnemySwatch.Ivory);
            // The red ribs of the paper, a black lid and base.
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, -0.21f + i * 0.14f, 0f), new Vector3(0.5f - Mathf.Abs(i - 1.5f) * 0.1f, 0.025f, 0.5f - Mathf.Abs(i - 1.5f) * 0.1f), EnemySwatch.WarCape);
            }

            b.Cylinder(new Vector3(0f, 0.26f, 0f), 0.16f, 0.12f, 0.08f, 8, EnemySwatch.Black);
            b.Cylinder(new Vector3(0f, -0.34f, 0f), 0.12f, 0.16f, 0.08f, 8, EnemySwatch.Black, bottomCap: true);
            // The two holes of a face it burnt into itself, so it could see where it was running.
            HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.1f, 0.27f, 0.07f, EnemySwatch.FireflyGlow);
            // The tassel hanging to the ground: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.34f, 0f), new Vector3(0f, -0.54f, 0f), 0.015f, 0.015f, 4, EnemySwatch.Gold);
            b.Blob(new Vector3(0f, -0.58f, 0f), new Vector3(0.08f, 0.1f, 0.08f), EnemySwatch.WarCape, coarse: true);
        }
    }
}
