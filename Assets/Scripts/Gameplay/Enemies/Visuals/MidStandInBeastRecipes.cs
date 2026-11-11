using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The thirteenth pass of beasts (bible §3.2.1): the roles of Regions 5–9, where the middle story walks (<c>MidStoryCatalog</c>), that the
    /// continuous world still played with stand-ins, low-poly from the shared palette like <see cref="EarlyStandInBeastRecipes"/>. Pivot at the feet
    /// (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Ave-Relâmpago</b> (26, ~1.2 m): black-and-white savanna bird with a crackle of caught light on the wings. Birds' parts.</item>
    /// <item><b>Garça-Lança Apagada</b> (31, ~1.7 m): grey heron with a beak like a spearhead. Birds' parts.</item>
    /// <item><b>Abutre-Barbudo Apagado</b> (37, ~1.3 m): bearded vulture with a stone in its claws. Birds' parts.</item>
    /// <item><b>Cisne de Feltro</b> (48, ~1.3 m): a swan of white felt with sewn-on colours, a stitched light for an eye. Birds' parts.</item>
    /// <item><b>Gralha-de-Bico-Vermelho</b> (36, ~1.1 m): glossy black chough with a red bill and red legs. Birds' parts.</item>
    /// <item><b>Moreia Apagada</b> (25, ~1.6 m long): a moray coiled on the reef, head raised. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Carpa-Dragão Apagada</b> (43, ~1 m): a grey carp with a few gold scales left, swimming in the air. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>, fins as <c>Wing_L</c>/<c>Wing_R</c>.</item>
    /// <item><b>Lobo de Gelo</b> (38, ~1.1 m): a wolf of packed snow with ice along the spine. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Cavalo de Terracota</b> (45, ~1.8 m): a clay horse from the guardian army, lit through its cracks. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Caranguejo-Violinista</b> (30, ~0.7 m): a dark mangrove crab with one huge pale claw. <c>Body</c> → <c>Head</c> (eye stalks), <c>Jaw</c> (the big claw), <c>Claw_L</c>.</item>
    /// <item><b>Ouriço de Fumaça</b> (25, ~0.9 m): a sea urchin of smoke, its spines tipped with embers. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Sanguessuga do Lodo</b> (32, ~0.8 m): a fat leech rearing up, a lit ring for a mouth. <c>Body</c> → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class MidStandInBeastRecipes
    {
        // ------------------------------------------------------------------
        // Shared: the walking birds' legs (feet on the ground, y = 0)
        // ------------------------------------------------------------------

        private static void DrawBirdLegs(EnemyMeshBuilder b, float bodyY, float spread, EnemySwatch leg, EnemySwatch foot)
        {
            float ground = -bodyY + 0.02f;
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(spread * side, -0.2f, 0f), new Vector3((spread + 0.02f) * side, ground, 0.04f), 0.035f, 0.02f, 5, leg);
                b.Box(new Vector3((spread + 0.02f) * side, ground, 0.1f), new Vector3(0.12f, 0.02f, 0.18f), foot);
            }
        }

        /// <summary>The five birds share one skeleton: Body → Head → Jaw (the lower beak), two broad wings and a fan tail.</summary>
        private static void BuildBird(ProceduralEnemyBlueprint bp, string key, float bodyY, System.Action<EnemyMeshBuilder> drawBody, Vector3 headAt,
            System.Action<EnemyMeshBuilder> drawHead, Vector3 jawAt, System.Action<EnemyMeshBuilder> drawJaw, EnemySwatch wingTop, EnemySwatch wingUnder,
            EnemySwatch wingTip, float span, EnemySwatch tailTop, EnemySwatch tailUnder)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, bodyY, 0f), drawBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, headAt, drawHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, jawAt, drawJaw);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, wingTop, wingUnder, wingTip, span), Quaternion.Euler(0f, 0f, -12f), meshKey: key + "Wing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, wingTop, wingUnder, wingTip, span), Quaternion.Euler(0f, 0f, 12f), meshKey: key + "Wing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.34f), b => RangedEnemyRecipes.DrawFanTail(b, tailTop, tailUnder), Quaternion.Euler(12f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Ave-Relâmpago
        // ------------------------------------------------------------------

        public static void BuildLightningBird(ProceduralEnemyBlueprint bp)
        {
            BuildBird(bp, "LightningBird", 0.66f, DrawLightningBirdBody, new Vector3(0f, 0.36f, 0.34f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.26f), EnemySwatch.Black);
                    b.Blob(new Vector3(0f, -0.05f, 0.04f), new Vector3(0.16f, 0.12f, 0.16f), EnemySwatch.Ivory, coarse: true);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.035f, 0.01f, 0.12f, 5, EnemySwatch.Gold, rotation: Quaternion.Euler(84f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.07f, 0.05f, EnemySwatch.Crystal);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.12f), EnemySwatch.Gold),
                EnemySwatch.Black, EnemySwatch.Ivory, EnemySwatch.Crystal, 0.9f, EnemySwatch.Black, EnemySwatch.Ivory);
        }

        private static void DrawLightningBirdBody(EnemyMeshBuilder b)
        {
            // The storm birds of the savanna catch a spark of the light that falls in the rain. The Darkness clouded this one, and it throws the spark back.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.5f, 0.72f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.08f, 0.14f), new Vector3(0.38f, 0.3f, 0.38f), EnemySwatch.Ivory, coarse: true);
            // The crackle along the flanks, zigzag strokes of caught light.
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    float z = 0.16f - i * 0.16f;
                    b.Box(new Vector3(0.23f * s, 0.06f - (i % 2) * 0.05f, z), new Vector3(0.02f, 0.03f, 0.14f), EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, (i % 2 == 0 ? 30f : -30f) * s));
                }
            }

            DrawBirdLegs(b, 0.66f, 0.09f, EnemySwatch.IronDark, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Garça-Lança Apagada
        // ------------------------------------------------------------------

        public static void BuildSpearHeron(ProceduralEnemyBlueprint bp)
        {
            BuildBird(bp, "SpearHeron", 0.86f, DrawSpearHeronBody, new Vector3(0f, 0.72f, 0.44f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.24f), EnemySwatch.GolemStone);
                    // The black crest drawn back, and the spearhead of a beak with a lit point.
                    b.Segment(new Vector3(0f, 0.06f, -0.06f), new Vector3(0f, 0.08f, -0.26f), 0.02f, 0.006f, 4, EnemySwatch.Black);
                    b.Cylinder(new Vector3(0f, 0.01f, 0.1f), 0.045f, 0.004f, 0.42f, 5, EnemySwatch.IronDark, rotation: Quaternion.Euler(86f, 0f, 0f));
                    b.Blob(new Vector3(0f, 0.035f, 0.5f), new Vector3(0.035f, 0.035f, 0.05f), EnemySwatch.Ember, coarse: true);
                    HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.035f, 0.07f, 0.04f, EnemySwatch.MoonEye);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.16f), new Vector3(0.05f, 0.02f, 0.32f), EnemySwatch.IronDark),
                EnemySwatch.GolemStone, EnemySwatch.GolemSnow, EnemySwatch.Black, 0.95f, EnemySwatch.GolemStoneDark, EnemySwatch.Shadow);
        }

        private static void DrawSpearHeronBody(EnemyMeshBuilder b)
        {
            // It stands in the shallows of the delta as still as a post and spears what passes. Now it spears the light off the water and throws the shaft.
            b.Blob(Vector3.zero, new Vector3(0.44f, 0.46f, 0.78f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.06f, 0.18f), new Vector3(0.32f, 0.3f, 0.34f), EnemySwatch.GolemSnow, coarse: true);
            b.Segment(new Vector3(0f, 0.14f, 0.28f), new Vector3(0f, 0.72f, 0.42f), 0.065f, 0.045f, 6, EnemySwatch.GolemSnow);
            // The dark streak down the long neck, and the plumes on the breast.
            b.Segment(new Vector3(0f, 0.2f, 0.34f), new Vector3(0f, 0.6f, 0.43f), 0.03f, 0.02f, 4, EnemySwatch.Shadow);
            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0.06f * i, -0.12f, 0.3f), new Vector3(0.04f, 0.2f, 0.02f), EnemySwatch.GolemSnow, Quaternion.Euler(-14f, 0f, 8f * i));
            }

            DrawBirdLegs(b, 0.86f, 0.08f, EnemySwatch.SandstoneDark, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Abutre-Barbudo Apagado
        // ------------------------------------------------------------------

        public static void BuildBeardedVulture(ProceduralEnemyBlueprint bp)
        {
            BuildBird(bp, "BeardedVulture", 0.62f, DrawBeardedVultureBody, new Vector3(0f, 0.38f, 0.36f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.28f), EnemySwatch.OwlBuff);
                    b.Blob(new Vector3(0f, 0.04f, 0.02f), new Vector3(0.2f, 0.12f, 0.2f), EnemySwatch.Ivory, coarse: true);
                    b.Cylinder(new Vector3(0f, 0f, 0.1f), 0.04f, 0.012f, 0.14f, 5, EnemySwatch.IronDark, rotation: Quaternion.Euler(86f, 0f, 0f));
                    // The black mask through the eye and the bristle beard under the beak.
                    b.Box(new Vector3(0f, 0.03f, 0.06f), new Vector3(0.22f, 0.03f, 0.08f), EnemySwatch.Black);
                    b.Segment(new Vector3(0f, -0.06f, 0.12f), new Vector3(0f, -0.14f, 0.14f), 0.03f, 0.008f, 4, EnemySwatch.Black);
                    HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.08f, 0.05f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.07f), new Vector3(0.06f, 0.02f, 0.14f), EnemySwatch.IronDark),
                EnemySwatch.Shadow, EnemySwatch.SandstoneDark, EnemySwatch.Black, 1f, EnemySwatch.Shadow, EnemySwatch.Black);
        }

        private static void DrawBeardedVultureBody(EnemyMeshBuilder b)
        {
            // It drops bones on the stones of the chutes to break them. The wind of ruin taught it to drop the stones on whoever climbs.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.52f, 0.8f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.08f, 0.14f), new Vector3(0.42f, 0.34f, 0.42f), EnemySwatch.OwlBuff, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.24f), new Vector3(0f, 0.36f, 0.34f), 0.09f, 0.07f, 6, EnemySwatch.OwlBuff);
            // The stone it carries in its claws, ready to let go; a crack lit where the light got in.
            b.Blob(new Vector3(0f, -0.32f, 0.12f), new Vector3(0.2f, 0.16f, 0.2f), EnemySwatch.GolemStone, coarse: true);
            b.Box(new Vector3(0.02f, -0.27f, 0.2f), new Vector3(0.1f, 0.02f, 0.03f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 20f));
            DrawBirdLegs(b, 0.62f, 0.12f, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Cisne de Feltro
        // ------------------------------------------------------------------

        public static void BuildFeltSwan(ProceduralEnemyBlueprint bp)
        {
            BuildBird(bp, "FeltSwan", 0.6f, DrawFeltSwanBody, new Vector3(0f, 0.64f, 0.4f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.24f), EnemySwatch.Ivory);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.04f, 0.015f, 0.12f, 5, EnemySwatch.Copper, rotation: Quaternion.Euler(84f, 0f, 0f));
                    // The eye is a stitched circle of thread around a spark.
                    HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.03f, 0.06f, 0.05f, EnemySwatch.StolenLight);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.12f), EnemySwatch.CopperDark),
                EnemySwatch.Ivory, EnemySwatch.IvoryDark, EnemySwatch.WarCape, 0.85f, EnemySwatch.Ivory, EnemySwatch.IvoryDark);
        }

        private static void DrawFeltSwanBody(EnemyMeshBuilder b)
        {
            // The steppe people sewed swans of felt for the tents of their dead. This one got up off the wall hanging and flies over the water as it was sewn.
            b.Blob(Vector3.zero, new Vector3(0.52f, 0.44f, 0.82f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, -0.06f, 0.1f), new Vector3(0.4f, 0.3f, 0.5f), EnemySwatch.IvoryDark, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.3f), new Vector3(0f, 0.64f, 0.4f), 0.07f, 0.05f, 6, EnemySwatch.Ivory);
            // The sewn-on colours: a red band, a green wave, and the big stitches holding it all together.
            b.Box(new Vector3(0f, 0.14f, -0.02f), new Vector3(0.5f, 0.04f, 0.1f), EnemySwatch.WarCape);
            b.Box(new Vector3(0f, 0.08f, -0.2f), new Vector3(0.46f, 0.03f, 0.08f), EnemySwatch.Verdigris, Quaternion.Euler(0f, 10f, 0f));
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(-0.15f + i * 0.1f, 0.2f, 0.12f), new Vector3(0.012f, 0.02f, 0.06f), EnemySwatch.Gold);
            }

            DrawBirdLegs(b, 0.6f, 0.1f, EnemySwatch.Black, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Gralha-de-Bico-Vermelho
        // ------------------------------------------------------------------

        public static void BuildRedBilledChough(ProceduralEnemyBlueprint bp)
        {
            BuildBird(bp, "RedBilledChough", 0.56f, DrawRedBilledChoughBody, new Vector3(0f, 0.32f, 0.32f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.24f), EnemySwatch.Black);
                    // The long red bill curved down, like the real chough's.
                    b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.03f, 0.008f, 0.16f, 5, EnemySwatch.WarCape, rotation: Quaternion.Euler(100f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.035f, 0.07f, 0.045f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.02f, 0.07f), new Vector3(0.04f, 0.02f, 0.13f), EnemySwatch.WarCape, Quaternion.Euler(14f, 0f, 0f)),
                EnemySwatch.Black, EnemySwatch.IronDark, EnemySwatch.Shadow, 0.85f, EnemySwatch.Black, EnemySwatch.IronDark);
        }

        private static void DrawRedBilledChoughBody(EnemyMeshBuilder b)
        {
            // The choughs of the terraces steal the apricots off the drying roofs. These steal the little lights out of the windows and peck whoever comes for them.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.48f, 0.7f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.08f, 0.12f), new Vector3(0.36f, 0.3f, 0.38f), EnemySwatch.IronDark, coarse: true);
            // An apricot stone it is carrying, still lit.
            b.Blob(new Vector3(0.12f, -0.2f, 0.16f), new Vector3(0.07f, 0.07f, 0.07f), EnemySwatch.Ember, coarse: true);
            DrawBirdLegs(b, 0.56f, 0.09f, EnemySwatch.WarCape, EnemySwatch.WarCape);
        }

        // ------------------------------------------------------------------
        // Moreia Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedMoray(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.18f, 0f), DrawDimmedMorayBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.46f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.36f), EnemySwatch.CaveMoss);
                b.Blob(new Vector3(0f, 0.06f, -0.04f), new Vector3(0.2f, 0.14f, 0.26f), EnemySwatch.Lichen, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.05f * s, -0.04f, 0.14f), 0.012f, 0f, 0.05f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.06f, 0.08f, 0.05f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.04f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.18f, 0.05f, 0.22f), EnemySwatch.Lichen);
                b.Cylinder(new Vector3(0.05f, 0.01f, 0.16f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.05f, 0.01f, 0.16f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(-0.1f, 0f, -0.82f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.08f, 0f, -0.34f), 0.11f, 0.03f, 6, EnemySwatch.CaveMoss);
                b.Box(new Vector3(0.04f, 0.1f, -0.16f), new Vector3(0.02f, 0.1f, 0.3f), EnemySwatch.Shadow);
            });
        }

        private static void DrawDimmedMorayBody(EnemyMeshBuilder b)
        {
            // The morays live in the holes of the reef and bite what puts a hand in. The light of the coral went out and this one came out of its hole to look for more.
            for (int i = 0; i < 6; i++)
            {
                float z = 0.3f - i * 0.2f;
                float x = Mathf.Sin(i * 1.05f) * 0.16f;
                float w = 0.32f - i * 0.02f;
                b.Blob(new Vector3(x, 0f, z), new Vector3(w, w, 0.26f), i % 2 == 0 ? EnemySwatch.CaveMoss : EnemySwatch.Verdigris);
                b.Blob(new Vector3(x, 0.1f, z), new Vector3(w * 0.4f, 0.04f, 0.12f), EnemySwatch.Lichen, coarse: true);
            }

            // The raised neck, and the low fin running down the back with a few cold sparks caught in it.
            b.Segment(new Vector3(0f, 0.04f, 0.3f), new Vector3(0f, 0.38f, 0.42f), 0.14f, 0.12f, 6, EnemySwatch.CaveMoss);
            b.Box(new Vector3(0f, 0.32f, 0.34f), new Vector3(0.02f, 0.12f, 0.2f), EnemySwatch.Shadow, Quaternion.Euler(-30f, 0f, 0f));
            b.Blob(new Vector3(0.1f, 0.12f, -0.1f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.FireflyGlow, coarse: true);
            b.Blob(new Vector3(-0.12f, 0.12f, -0.5f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.FireflyGlow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Carpa-Dragão Apagada
        // ------------------------------------------------------------------

        public static void BuildDragonCarp(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawDragonCarpBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.4f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.34f, 0.36f, 0.3f), EnemySwatch.GolemStone);
                // The two long whiskers of a carp that meant to become a dragon, an ember at each end.
                for (int s = -1; s <= 1; s += 2)
                {
                    float side = s;
                    b.Segment(new Vector3(0.08f * side, -0.08f, 0.12f), new Vector3(0.22f * side, -0.2f, 0.2f), 0.012f, 0.006f, 3, EnemySwatch.GolemStoneDark);
                    b.Blob(new Vector3(0.22f * side, -0.2f, 0.2f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.06f, 0.06f, 0.07f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.16f, 0.04f, 0.1f), EnemySwatch.GolemStoneDark));
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.18f * side, -0.12f, 0.18f), b =>
                {
                    b.Box(new Vector3(0.12f * side, 0f, -0.04f), new Vector3(0.24f, 0.02f, 0.16f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 20f * side, -14f * side));
                    b.Box(new Vector3(0.2f * side, 0.005f, -0.06f), new Vector3(0.06f, 0.022f, 0.08f), EnemySwatch.Gold, Quaternion.Euler(0f, 20f * side, -14f * side));
                }, meshKey: s < 0 ? "DragonCarpFin_L" : "DragonCarpFin_R");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.44f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.16f), 0.12f, 0.06f, 6, EnemySwatch.GolemStone);
                b.Box(new Vector3(0f, 0.1f, -0.26f), new Vector3(0.02f, 0.24f, 0.16f), EnemySwatch.GolemStoneDark, Quaternion.Euler(-30f, 0f, 0f));
                b.Box(new Vector3(0f, -0.1f, -0.26f), new Vector3(0.02f, 0.24f, 0.16f), EnemySwatch.GolemStoneDark, Quaternion.Euler(30f, 0f, 0f));
            });
        }

        private static void DrawDragonCarpBody(EnemyMeshBuilder b)
        {
            // The carp that leaps the falls of the canal becomes a dragon, the boatmen say. This one leapt the lock on the night the light went out and stayed in the air, grey, with only a few scales of gold left.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.5f, 0.86f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.32f, 0.26f, 0.66f), EnemySwatch.GolemSnow, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3((i % 2 == 0 ? 0.17f : -0.17f), 0.06f - (i / 2) * 0.1f, 0.16f - i * 0.12f), new Vector3(0.08f, 0.08f, 0.06f), EnemySwatch.Gold, coarse: true);
            }

            // The dorsal fin, and the belly fins hanging down to the water it is not in: the lowest point of the model.
            b.Box(new Vector3(0f, 0.3f, -0.06f), new Vector3(0.02f, 0.14f, 0.4f), EnemySwatch.GolemStoneDark, Quaternion.Euler(-8f, 0f, 0f));
            b.Box(new Vector3(0f, -0.36f, -0.14f), new Vector3(0.02f, 0.24f, 0.18f), EnemySwatch.GolemStoneDark, Quaternion.Euler(16f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.08f * s, -0.3f, 0.12f), new Vector3(0.02f, 0.14f, 0.1f), EnemySwatch.GolemStoneDark, Quaternion.Euler(20f, 0f, 14f * s));
            }
        }

        // ------------------------------------------------------------------
        // Lobo de Gelo
        // ------------------------------------------------------------------

        public static void BuildIceWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawIceWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.18f, 0.54f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.32f, 0.3f, 0.32f), EnemySwatch.GolemSnow);
                b.Blob(new Vector3(0f, -0.04f, 0.2f), new Vector3(0.16f, 0.14f, 0.24f), EnemySwatch.GolemSnow, coarse: true);
                b.Blob(new Vector3(0f, -0.01f, 0.32f), new Vector3(0.06f, 0.05f, 0.05f), EnemySwatch.Black, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    // Ears of ice.
                    b.Cylinder(new Vector3(0.09f * s, 0.12f, -0.04f), 0.05f, 0f, 0.16f, 4, EnemySwatch.Crystal);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.13f, 0.05f, EnemySwatch.Crystal);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.12f), new Vector3(0.13f, 0.04f, 0.22f), EnemySwatch.GolemStone);
                b.Cylinder(new Vector3(0.04f, 0.005f, 0.2f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.005f, 0.2f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.48f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.1f, -0.3f), 0.09f, 0.05f, 5, EnemySwatch.GolemSnow);
                b.Cylinder(new Vector3(0f, -0.12f, -0.34f), 0.04f, 0f, 0.1f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(-110f, 0f, 0f));
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.5f, 0.22f, 0.34f, -0.34f, 0.07f, EnemySwatch.GolemSnow, EnemySwatch.GolemStone, "IceWolf");
        }

        private static void DrawIceWolfBody(EnemyMeshBuilder b)
        {
            // The glacier keeps a pack of wolves made of the snow that never melts. They do not eat; they run down whatever is warm and leave it cold.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.52f, 1f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, -0.12f, 0.02f), new Vector3(0.48f, 0.26f, 0.74f), EnemySwatch.GolemStone, coarse: true);
            b.Blob(new Vector3(0f, 0.06f, 0.36f), new Vector3(0.56f, 0.44f, 0.36f), EnemySwatch.GolemSnow, coarse: true);
            // The ridge of ice along the spine, lit from inside by the cold.
            for (int i = 0; i < 5; i++)
            {
                b.Cylinder(new Vector3(0f, 0.22f, 0.34f - i * 0.16f), 0.05f, 0f, 0.16f - i * 0.015f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(-20f, 0f, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Cavalo de Terracota
        // ------------------------------------------------------------------

        public static void BuildTerracottaHorse(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.95f, 0f), DrawTerracottaHorseBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.62f, 0.72f), b =>
            {
                b.Blob(new Vector3(0f, 0f, 0.08f), new Vector3(0.24f, 0.28f, 0.44f), EnemySwatch.SandstoneDark, Quaternion.Euler(30f, 0f, 0f));
                // The clay mane, cut short in a brush like the old figures, and the ears.
                b.Box(new Vector3(0f, 0.16f, -0.08f), new Vector3(0.05f, 0.1f, 0.2f), EnemySwatch.Sandstone);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.07f * s, 0.14f, -0.02f), 0.035f, 0f, 0.1f, 4, EnemySwatch.SandstoneDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.04f, 0.04f, 0.05f, EnemySwatch.GolemCrack);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.12f, 0.2f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.14f, 0.04f, 0.16f), EnemySwatch.SandstoneDark, Quaternion.Euler(30f, 0f, 0f)));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.14f, -0.6f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.36f, -0.14f), 0.07f, 0.04f, 5, EnemySwatch.Sandstone);
                b.Blob(new Vector3(0f, -0.38f, -0.15f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.SandstoneDark, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.85f, 0.2f, 0.4f, -0.4f, 0.08f, EnemySwatch.SandstoneDark, EnemySwatch.Sandstone, "TerracottaHorse");
        }

        private static void DrawTerracottaHorseBody(EnemyMeshBuilder b)
        {
            // The clay army was made to guard a sleep. The Darkness woke the horses without their riders, and they run the moats looking for someone to carry.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.56f, 1.2f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.12f, 0f), new Vector3(0.46f, 0.3f, 0.96f), EnemySwatch.Sandstone, coarse: true);
            b.Segment(new Vector3(0f, 0.16f, 0.46f), new Vector3(0f, 0.56f, 0.7f), 0.15f, 0.11f, 6, EnemySwatch.SandstoneDark);
            // The saddle cloth of painted clay, and the cracks where the light shows through.
            b.Box(new Vector3(0f, 0.27f, -0.04f), new Vector3(0.5f, 0.04f, 0.4f), EnemySwatch.WarCape);
            b.Box(new Vector3(0.3f, 0.02f, 0.16f), new Vector3(0.02f, 0.2f, 0.03f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 20f));
            b.Box(new Vector3(-0.3f, -0.04f, -0.26f), new Vector3(0.02f, 0.18f, 0.03f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -16f));
        }

        // ------------------------------------------------------------------
        // Caranguejo-Violinista
        // ------------------------------------------------------------------

        public static void BuildFiddlerCrab(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawFiddlerCrabBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.2f), b =>
            {
                // Two eye stalks, the eyes lit like little lamps.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.06f * s, 0f, 0f), new Vector3(0.08f * s, 0.2f, 0.02f), 0.018f, 0.012f, 4, EnemySwatch.BarkDark);
                    b.Blob(new Vector3(0.08f * s, 0.22f, 0.02f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.MoonEye, coarse: true);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, body, new Vector3(0.24f, 0.06f, 0.2f), b =>
            {
                // The one great claw, pale as the moon, raised to wave: it swings down on the windup.
                b.Segment(Vector3.zero, new Vector3(0.12f, 0.12f, 0.1f), 0.05f, 0.04f, 5, EnemySwatch.Ivory);
                b.Box(new Vector3(0.16f, 0.24f, 0.14f), new Vector3(0.16f, 0.26f, 0.12f), EnemySwatch.Ivory, Quaternion.Euler(0f, 0f, -20f));
                b.Box(new Vector3(0.24f, 0.32f, 0.15f), new Vector3(0.06f, 0.18f, 0.08f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 0f, -30f));
                b.Blob(new Vector3(0.12f, 0.16f, 0.2f), new Vector3(0.05f, 0.05f, 0.03f), EnemySwatch.StolenLight, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.ClawLeft, body, new Vector3(-0.2f, -0.02f, 0.22f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(-0.06f, 0f, 0.08f), 0.025f, 0.02f, 4, EnemySwatch.BarkDark);
                b.Box(new Vector3(-0.07f, 0f, 0.12f), new Vector3(0.05f, 0.04f, 0.07f), EnemySwatch.Bark);
            });
        }

        private static void DrawFiddlerCrabBody(EnemyMeshBuilder b)
        {
            // In the mangroves the fiddler crabs wave their big claw at the tide to call it in. This one waves it at the cousins, and it means it.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.24f, 0.46f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.48f, 0.12f, 0.36f), EnemySwatch.Shadow, coarse: true);
            b.Blob(new Vector3(0.1f, 0.1f, 0.04f), new Vector3(0.08f, 0.03f, 0.08f), EnemySwatch.FireflyGlow, coarse: true);
            // Three pairs of walking legs, splayed: their tips are the lowest point of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                for (int k = 0; k < 3; k++)
                {
                    float z = 0.1f - k * 0.12f;
                    var hip = new Vector3(0.24f * side, -0.02f, z);
                    var knee = new Vector3(0.44f * side, 0.08f, z - 0.02f);
                    var tip = new Vector3(0.54f * side, -0.29f, z - 0.04f);
                    b.Segment(hip, knee, 0.03f, 0.025f, 4, EnemySwatch.BarkDark);
                    b.Segment(knee, tip, 0.025f, 0.008f, 4, EnemySwatch.Bark);
                }
            }
        }

        // ------------------------------------------------------------------
        // Ouriço de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeUrchin(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.36f, 0f), DrawSmokeUrchinBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.27f, 0f), b =>
            {
                // The ember on top, where an urchin keeps its mouth's opposite: it flares before the spines fly.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.1f, 0.16f), EnemySwatch.Ember);
                b.Blob(new Vector3(0f, 0.03f, 0f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawSmokeUrchinBody(EnemyMeshBuilder b)
        {
            // The urchins of the tide pools graze the stones. Smoke came in on the tide and filled one up, and now it rolls up the beach after the warm feet.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.56f, 0.62f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.5f, 0.5f, 0.5f), EnemySwatch.Shadow, coarse: true);
            // Spines in four rings, never pointing into the ground; an ember on every other tip.
            float[] elevations = { -12f, 22f, 52f, 80f };
            int[] counts = { 10, 9, 7, 4 };
            for (int ring = 0; ring < elevations.Length; ring++)
            {
                float el = elevations[ring] * Mathf.Deg2Rad;
                for (int i = 0; i < counts[ring]; i++)
                {
                    float az = (i + ring * 0.5f) / counts[ring] * Mathf.PI * 2f;
                    var dir = new Vector3(Mathf.Cos(el) * Mathf.Cos(az), Mathf.Sin(el), Mathf.Cos(el) * Mathf.Sin(az));
                    float length = 0.26f - ring * 0.03f;
                    b.Cylinder(dir * 0.24f, 0.03f, 0f, length, 3, (i + ring) % 2 == 0 ? EnemySwatch.Shadow : EnemySwatch.Black, rotation: Quaternion.FromToRotation(Vector3.up, dir));
                    if ((i + ring) % 2 == 0)
                    {
                        b.Blob(dir * (0.24f + length), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.Ember, coarse: true);
                    }
                }
            }

            // The tube feet it shuffles on: the lowest point of the model.
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                b.Segment(new Vector3(Mathf.Cos(a) * 0.14f, -0.22f, Mathf.Sin(a) * 0.14f), new Vector3(Mathf.Cos(a) * 0.18f, -0.34f, Mathf.Sin(a) * 0.18f), 0.03f, 0.02f, 4, EnemySwatch.Shroud);
            }
        }

        // ------------------------------------------------------------------
        // Sanguessuga do Lodo
        // ------------------------------------------------------------------

        public static void BuildMireLeech(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.2f, 0f), DrawMireLeechBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.46f, 0.36f), b =>
            {
                // The sucker at the front, a ring with a little of the light it drank showing at the bottom.
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.12f), EnemySwatch.BarkDark, Quaternion.Euler(-40f, 0f, 0f));
                b.Blob(new Vector3(0f, 0.02f, 0.04f), new Vector3(0.12f, 0.12f, 0.06f), EnemySwatch.StolenLight, Quaternion.Euler(-40f, 0f, 0f), coarse: true);
            });
        }

        private static void DrawMireLeechBody(EnemyMeshBuilder b)
        {
            // The leeches of the delta drink a little blood and let go. The mire made these thirsty for light instead, and they never let go.
            for (int i = 0; i < 6; i++)
            {
                float f = i / 5f;
                float z = -0.5f + f * 0.8f;
                float y = -0.04f + f * f * 0.44f;
                float w = 0.56f - f * 0.26f;
                b.Blob(new Vector3(0f, y, z), new Vector3(w, 0.3f - f * 0.06f, 0.26f), i % 2 == 0 ? EnemySwatch.CaveMoss : EnemySwatch.BarkDark);
            }

            // Pale stripes down the back, the way the real leeches are painted, and a couple of lit spots it swallowed.
            for (int i = 0; i < 4; i++)
            {
                float f = i / 4f;
                b.Box(new Vector3(0.08f, 0.1f + f * f * 0.44f, -0.46f + f * 0.8f), new Vector3(0.03f, 0.02f, 0.14f), EnemySwatch.OwlBuff, Quaternion.Euler(-f * 40f, 0f, 0f));
                b.Box(new Vector3(-0.08f, 0.1f + f * f * 0.44f, -0.46f + f * 0.8f), new Vector3(0.03f, 0.02f, 0.14f), EnemySwatch.OwlBuff, Quaternion.Euler(-f * 40f, 0f, 0f));
            }

            b.Blob(new Vector3(0.14f, 0.04f, -0.3f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.FireflyGlow, coarse: true);
            b.Blob(new Vector3(-0.1f, 0.12f, -0.06f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.FireflyGlow, coarse: true);
        }
    }
}
