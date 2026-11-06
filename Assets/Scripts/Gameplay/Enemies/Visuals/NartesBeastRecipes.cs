using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Estepe dos Nartes (Docs/Mapas100.md 47–52) that the continuous world played with stand-ins,
    /// low-poly from the shared palette like <see cref="OverworldBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Grifo Jovem</b> (~1.2 m): lion body, eagle head and folded wings, Scythian gold; the wings spread on the windup.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c> (lower beak), <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>; four jointed legs.</item>
    /// <item><b>Saiga Assustada</b> (~0.85 m): pale steppe antelope, drooping nose, amber ridged horns; lowers its head to butt.
    /// <c>Body</c> → <c>Head</c>, <c>Tail</c>; four thin legs.</item>
    /// <item><b>Lobo de Bronze</b> (~0.95 m): bronze wolf of the kurgans in the animal style, verdigris and gold spirals.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Gigante Sonâmbulo</b> (~2.5 m): hill giant walking asleep, eyes shut, arms reaching out; slams with both.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>, <c>Arm_R</c>; <c>Leg_L</c>/<c>Leg_R</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Besouro-Casca</b> (~0.6 m): six-legged beetle with bark-ridged wing cases and pincer mandibles.
    /// <c>Body</c> → <c>Claw_L</c>/<c>Claw_R</c>; six legs <c>Leg_L0..2</c>/<c>Leg_R0..2</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Ochokochi</b> (~1.9 m): shaggy forest man of the Georgian tales with the axe-blade growing from his chest.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>, <c>Arm_R</c>; <c>Leg_L</c>/<c>Leg_R</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// </list>
    /// </summary>
    public static class NartesBeastRecipes
    {
        /// <summary>Leg pairs of the Besouro-Casca.</summary>
        public const int BeetleLegPairs = 3;

        // ------------------------------------------------------------------
        // Grifo Jovem
        // ------------------------------------------------------------------

        public static void BuildYoungGriffin(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.58f, 0f), DrawGriffinBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.46f), DrawGriffinHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.03f, 0.12f), b =>
                b.Cylinder(Vector3.zero, 0.04f, 0.005f, 0.1f, 4, EnemySwatch.Horn, rotation: Quaternion.Euler(100f, 0f, 0f)));
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.16f * side, 0.18f, 0.22f),
                    b => DrawGriffinWing(b, side));
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.2f, -0.24f), 0.03f, 0.025f, 5, EnemySwatch.OwlBuff);
                b.Blob(new Vector3(0f, -0.22f, -0.26f), new Vector3(0.08f, 0.1f, 0.08f), EnemySwatch.Gold, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                // Eagle talons in front, lion paws behind.
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.12f * side, 0.52f, 0.3f), new Vector3(0f, -0.25f, -0.02f), new Vector3(0f, -0.24f, 0.04f),
                    0.055f, EnemySwatch.Gold, EnemySwatch.Horn, "GriffinFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.12f * side, 0.54f, -0.3f), new Vector3(0f, -0.26f, -0.07f), new Vector3(0f, -0.25f, 0.07f),
                    0.065f, EnemySwatch.OwlBuff, EnemySwatch.SandstoneDark, "GriffinHind");
            }
        }

        private static void DrawGriffinBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.4f, 0.42f, 0.9f), EnemySwatch.OwlBuff);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.3f, 0.2f, 0.7f), EnemySwatch.SandstoneLight, coarse: true);
            // The feathered breast and neck of the eagle half, gold like the plaques of the kurgans.
            b.Blob(new Vector3(0f, 0.12f, 0.3f), new Vector3(0.36f, 0.44f, 0.34f), EnemySwatch.Gold, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.32f), new Vector3(0f, 0.3f, 0.44f), 0.13f, 0.1f, 6, EnemySwatch.Gold);
        }

        private static void DrawGriffinHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.22f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, 0.09f, -0.06f), new Vector3(0.14f, 0.12f, 0.12f), EnemySwatch.IvoryDark, coarse: true);
            // The hooked beak, the young griffin's small crest and the ear tufts of the Scythian griffins.
            b.Horn(new Vector3(0f, 0.01f, 0.08f), 0.05f, 0.16f, 3, 22f, EnemySwatch.Horn, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
            b.Cylinder(new Vector3(0f, 0.09f, -0.02f), 0.04f, 0f, 0.14f, 4, EnemySwatch.Gold, rotation: Quaternion.Euler(-40f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.06f), new Vector3(0.035f, 0.035f, 0.025f), EnemySwatch.Ember, coarse: true);
                b.Cylinder(new Vector3(0.06f * s, 0.08f, -0.04f), 0.025f, 0f, 0.09f, 3, EnemySwatch.Gold, rotation: Quaternion.Euler(-25f, 0f, -20f * s));
            }
        }

        private static void DrawGriffinWing(EnemyMeshBuilder b, float side)
        {
            // Folded along the flank, tips back past the hips: a few long feather plates.
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0.05f * side, 0.04f - i * 0.03f, -0.2f - i * 0.05f), new Vector3(0.04f, 0.14f, 0.5f - i * 0.06f), i % 2 == 0 ? EnemySwatch.Gold : EnemySwatch.OwlBuff,
                    Quaternion.Euler(-8f, 6f * side, 12f * side));
            }

            b.Blob(Vector3.zero, new Vector3(0.12f, 0.14f, 0.16f), EnemySwatch.Gold, coarse: true);
        }

        // ------------------------------------------------------------------
        // Saiga Assustada
        // ------------------------------------------------------------------

        public static void BuildSaiga(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawSaigaBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.42f), DrawSaigaHead, Quaternion.Euler(14f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.36f), b =>
                b.Cylinder(Vector3.zero, 0.03f, 0f, 0.1f, 4, EnemySwatch.SandstoneLight, rotation: Quaternion.Euler(-130f, 0f, 0f)));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.1f * side, 0.5f, 0.24f), new Vector3(0f, -0.25f, -0.02f), new Vector3(0f, -0.24f, 0.03f),
                    0.035f, EnemySwatch.Sandstone, EnemySwatch.Black, "SaigaFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.1f * side, 0.52f, -0.24f), new Vector3(0f, -0.26f, -0.06f), new Vector3(0f, -0.25f, 0.06f),
                    0.04f, EnemySwatch.Sandstone, EnemySwatch.Black, "SaigaHind");
            }
        }

        private static void DrawSaigaBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.72f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, -0.08f, 0.02f), new Vector3(0.24f, 0.16f, 0.56f), EnemySwatch.SandstoneLight, coarse: true);
            b.Segment(new Vector3(0f, 0.06f, 0.28f), new Vector3(0f, 0.16f, 0.42f), 0.09f, 0.07f, 5, EnemySwatch.Sandstone);
        }

        private static void DrawSaigaHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.16f, 0.16f, 0.2f), EnemySwatch.Sandstone);
            // The big soft drooping nose that warms the steppe air and filters the dust.
            b.Blob(new Vector3(0f, -0.04f, 0.12f), new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.SandstoneDark, coarse: true);
            b.Cylinder(new Vector3(0f, -0.06f, 0.17f), 0.04f, 0.03f, 0.06f, 5, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(150f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                // Wide frightened eyes, small ears, the amber lyre horns with their rings.
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.04f), new Vector3(0.035f, 0.035f, 0.025f), EnemySwatch.SoftEye, coarse: true);
                b.Cylinder(new Vector3(0.07f * s, 0.07f, -0.05f), 0.025f, 0f, 0.06f, 4, EnemySwatch.Sandstone, rotation: Quaternion.Euler(-20f, 0f, -50f * s));
                b.Horn(new Vector3(0.035f * s, 0.07f, 0f), 0.022f, 0.2f, 4, -8f, EnemySwatch.Horn, Quaternion.Euler(-20f, 0f, -8f * s), 5, Vector3.right);
            }
        }

        // ------------------------------------------------------------------
        // Lobo de Bronze
        // ------------------------------------------------------------------

        public static void BuildBronzeWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawBronzeWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.52f), DrawBronzeWolfHead, Quaternion.Euler(6f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.1f), new Vector3(0.1f, 0.04f, 0.2f), EnemySwatch.CopperDark);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.035f * s, 0f, 0.17f), 0.012f, 0f, 0.045f, 3, EnemySwatch.Gold);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.48f), b =>
            {
                // The animal-style tail curled into a ring, like the plaques in the kurgans.
                b.Segment(Vector3.zero, new Vector3(0f, -0.2f, -0.14f), 0.05f, 0.04f, 5, EnemySwatch.Copper);
                b.Cylinder(new Vector3(0f, -0.24f, -0.14f), 0.1f, 0.1f, 0.04f, 8, EnemySwatch.Verdigris, rotation: Quaternion.Euler(0f, 0f, 90f));
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.14f * side, 0.52f, 0.34f), new Vector3(0f, -0.25f, -0.02f), new Vector3(0f, -0.24f, 0.04f),
                    0.065f, EnemySwatch.Copper, EnemySwatch.CopperDark, new Vector3(0.1f, 0.07f, 0.14f), "BronzeWolfFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.14f * side, 0.54f, -0.34f), new Vector3(0f, -0.26f, -0.06f), new Vector3(0f, -0.25f, 0.06f),
                    0.07f, EnemySwatch.Copper, EnemySwatch.CopperDark, new Vector3(0.1f, 0.07f, 0.14f), "BronzeWolfHind");
            }
        }

        private static void DrawBronzeWolfBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.44f, 1f), EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.3f, 0.2f, 0.76f), EnemySwatch.CopperDark, coarse: true);
            // The spirals cast on the shoulder and the hip, green with age, and the seam where the trapped light shows.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.2f * s, 0.04f, 0.28f), 0.11f, 0.11f, 0.03f, 8, EnemySwatch.Verdigris, rotation: Quaternion.Euler(0f, 0f, 90f * s));
                b.Cylinder(new Vector3(0.21f * s, 0.04f, 0.28f), 0.05f, 0.05f, 0.03f, 6, EnemySwatch.Gold, rotation: Quaternion.Euler(0f, 0f, 90f * s));
                b.Cylinder(new Vector3(0.2f * s, 0.02f, -0.3f), 0.1f, 0.1f, 0.03f, 8, EnemySwatch.Verdigris, rotation: Quaternion.Euler(0f, 0f, 90f * s));
            }

            b.Box(new Vector3(0f, 0.22f, 0f), new Vector3(0.04f, 0.03f, 0.7f), EnemySwatch.GolemCrack);
        }

        private static void DrawBronzeWolfHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.2f, 0.24f), EnemySwatch.Copper);
            b.Cylinder(new Vector3(0f, -0.02f, 0.08f), 0.07f, 0.04f, 0.16f, 5, EnemySwatch.Copper, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, 0f, 0.24f), new Vector3(0.04f, 0.03f, 0.03f), EnemySwatch.CopperDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.05f, 0.08f), new Vector3(0.04f, 0.03f, 0.03f), EnemySwatch.WarEye, coarse: true);
                b.Cylinder(new Vector3(0.07f * s, 0.08f, -0.03f), 0.04f, 0f, 0.1f, 4, EnemySwatch.Verdigris, rotation: Quaternion.Euler(-12f, 0f, -14f * s));
            }
        }

        // ------------------------------------------------------------------
        // Gigante Sonâmbulo
        // ------------------------------------------------------------------

        public static void BuildSleepwalkerGiant(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.28f, 0f), DrawGiantBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.86f, 0.06f), DrawGiantHead, Quaternion.Euler(10f, 0f, 0f));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                // Arms reaching out in front, the way sleepwalkers walk.
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.5f * side, 0.6f, 0.04f),
                    b => DrawGiantArm(b, side), Quaternion.Euler(-62f, 0f, 0f));

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1, new Vector3(0.24f * side, 0.8f, 0f),
                    b => b.Segment(new Vector3(0f, -0.4f, 0.04f), new Vector3(0f, 0.05f, 0f), 0.13f, 0.16f, 6, EnemySwatch.Leather), meshKey: "GiantThigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg, new Vector3(0f, -0.4f, 0.04f), b =>
                {
                    b.Segment(new Vector3(0f, -0.36f, -0.02f), Vector3.zero, 0.1f, 0.12f, 6, EnemySwatch.CaveSkin);
                    b.Cylinder(new Vector3(0f, -0.3f, -0.02f), 0.13f, 0.12f, 0.16f, 6, EnemySwatch.WarCape);
                }, meshKey: "GiantShin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -0.36f, -0.02f),
                    b => b.Blob(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.24f, 0.12f, 0.36f), EnemySwatch.Leather, coarse: true), meshKey: "GiantFoot");
            }
        }

        private static void DrawGiantBody(EnemyMeshBuilder b)
        {
            // A huge round body in a sheepskin tunic belted with rope; a patched felt blanket over one shoulder.
            b.Blob(new Vector3(0f, 0.3f, 0f), new Vector3(0.98f, 1.04f, 0.7f), EnemySwatch.CaveSkin);
            b.Blob(new Vector3(0f, 0.18f, 0.02f), new Vector3(0.9f, 0.8f, 0.66f), EnemySwatch.IvoryDark, coarse: true);
            b.Box(new Vector3(0f, -0.08f, 0f), new Vector3(0.86f, 0.08f, 0.64f), EnemySwatch.Bark);
            b.Box(new Vector3(-0.18f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0.7f), EnemySwatch.WarCape, Quaternion.Euler(0f, 0f, 30f));
            b.Cylinder(new Vector3(0f, -0.38f, 0f), 0.42f, 0.36f, 0.32f, 8, EnemySwatch.IvoryDark);
        }

        private static void DrawGiantHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.42f, 0.44f, 0.4f), EnemySwatch.CaveSkin);
            // Eyes shut (a bad dream glows faintly behind the lids), a big beard, a felt cap slumped to one side.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.09f * s, 0.16f, 0.19f), new Vector3(0.09f, 0.018f, 0.02f), EnemySwatch.SoftEye, Quaternion.Euler(0f, 0f, -8f * s));
                b.Blob(new Vector3(0.21f * s, 0.12f, 0f), new Vector3(0.07f, 0.1f, 0.06f), EnemySwatch.CaveSkin, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.08f, 0.21f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.CaveSkinDark, coarse: true);
            b.Blob(new Vector3(0f, -0.04f, 0.12f), new Vector3(0.36f, 0.3f, 0.24f), EnemySwatch.Bark, coarse: true);
            b.Cylinder(new Vector3(0f, 0.28f, -0.02f), 0.22f, 0.04f, 0.3f, 6, EnemySwatch.WarCape, rotation: Quaternion.Euler(-30f, 0f, 25f));
        }

        private static void DrawGiantArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.28f, 0.28f), EnemySwatch.IvoryDark, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.03f * side, -0.46f, 0f), 0.13f, 0.11f, 6, EnemySwatch.CaveSkin);
            b.Segment(new Vector3(0.03f * side, -0.46f, 0f), new Vector3(0.03f * side, -0.86f, 0.04f), 0.11f, 0.1f, 6, EnemySwatch.CaveSkin);
            b.Blob(new Vector3(0.03f * side, -0.96f, 0.05f), new Vector3(0.22f, 0.2f, 0.18f), EnemySwatch.CaveSkinDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Besouro-Casca
        // ------------------------------------------------------------------

        private const float BeetleHipHeight = 0.3f;
        private static readonly float[] BeetleHipZ = { 0.14f, 0f, -0.14f };
        private static readonly float[] BeetleSplay = { 35f, 0f, -35f };
        private static readonly Vector3 BeetleKnee = new Vector3(0.2f, 0.12f, 0f);
        private static readonly Vector3 BeetleTip = new Vector3(0.16f, -(BeetleHipHeight + 0.12f - 0.01f), 0f);

        public static void BuildBarkBeetle(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, BeetleHipHeight, 0f), DrawBeetleBody);
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body, new Vector3(0.07f * side, 0.02f, 0.46f),
                    b => DrawBeetleMandible(b, sd));
            }

            for (int pair = 0; pair < BeetleLegPairs; pair++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    string id = (side < 0 ? "L" : "R") + pair;
                    float yaw = side > 0 ? -BeetleSplay[pair] : 180f + BeetleSplay[pair];
                    int hip = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, new Vector3(0.18f * side, BeetleHipHeight, BeetleHipZ[pair]), DrawBeetleUpper,
                        Quaternion.Euler(0f, yaw, 0f), meshKey: "BeetleUpper");
                    int knee = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, hip, BeetleKnee, DrawBeetleLower, meshKey: "BeetleLower");
                    bp.Node(ProceduralEnemyParts.FootPrefix + id, knee, BeetleTip);
                }
            }
        }

        private static void DrawBeetleBody(EnemyMeshBuilder b)
        {
            // Wing cases ridged like larch bark, a seam of trapped light down the middle, lichen on the shoulders.
            b.Blob(new Vector3(0f, 0.1f, -0.08f), new Vector3(0.56f, 0.42f, 0.74f), EnemySwatch.Bark);
            for (int i = -2; i <= 2; i++)
            {
                b.Box(new Vector3(i * 0.09f, 0.26f - Mathf.Abs(i) * 0.04f, -0.08f), new Vector3(0.04f, 0.06f, 0.6f), EnemySwatch.BarkDark);
            }

            b.Box(new Vector3(0f, 0.3f, -0.08f), new Vector3(0.02f, 0.02f, 0.62f), EnemySwatch.GolemCrack);
            b.Blob(new Vector3(0.16f, 0.16f, 0.2f), new Vector3(0.14f, 0.06f, 0.12f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(0f, -0.02f, -0.06f), new Vector3(0.44f, 0.12f, 0.6f), EnemySwatch.BarkDark, coarse: true);
            // Thorax and head.
            b.Blob(new Vector3(0f, 0.04f, 0.3f), new Vector3(0.34f, 0.2f, 0.2f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.02f, 0.42f), new Vector3(0.2f, 0.14f, 0.14f), EnemySwatch.Bark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, 0.05f, 0.46f), new Vector3(0.04f, 0.04f, 0.03f), EnemySwatch.MoonEye, coarse: true);
                b.Segment(new Vector3(0.05f * s, 0.07f, 0.47f), new Vector3(0.16f * s, 0.34f, 0.62f), 0.01f, 0.006f, 3, EnemySwatch.BarkDark);
            }
        }

        private static void DrawBeetleMandible(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.06f * side, 0f, 0.12f), 0.035f, 0.025f, 4, EnemySwatch.BarkDark);
            b.Segment(new Vector3(0.06f * side, 0f, 0.12f), new Vector3(-0.02f * side, -0.01f, 0.2f), 0.025f, 0.006f, 4, EnemySwatch.Horn);
        }

        private static void DrawBeetleUpper(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.07f, 0.07f, 0.07f), EnemySwatch.BarkDark, coarse: true);
            b.Segment(Vector3.zero, BeetleKnee, 0.035f, 0.03f, 5, EnemySwatch.Bark);
            b.Blob(BeetleKnee, new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.BarkDark, coarse: true);
        }

        private static void DrawBeetleLower(EnemyMeshBuilder b)
        {
            Vector3 mid = BeetleTip * 0.5f;
            b.Segment(Vector3.zero, mid, 0.028f, 0.022f, 5, EnemySwatch.Bark);
            b.Segment(mid, BeetleTip, 0.022f, 0.006f, 4, EnemySwatch.BarkDark);
        }

        // ------------------------------------------------------------------
        // Ochokochi
        // ------------------------------------------------------------------

        public static void BuildOchokochi(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.98f, 0f), DrawOchokochiBody, Quaternion.Euler(8f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.64f, 0.06f), DrawOchokochiHead, Quaternion.Euler(-6f, 0f, 0f));

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.36f * side, 0.44f, 0.02f),
                    b => DrawOchokochiArm(b, side), Quaternion.Euler(-12f, 0f, 0f));

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1, new Vector3(0.16f * side, 0.6f, 0f),
                    b => b.Segment(new Vector3(0f, -0.29f, 0.05f), new Vector3(0f, 0.04f, 0f), 0.085f, 0.11f, 6, EnemySwatch.BarkDark), meshKey: "OchokochiThigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg, new Vector3(0f, -0.29f, 0.05f),
                    b => b.Segment(new Vector3(0f, -0.28f, -0.02f), Vector3.zero, 0.065f, 0.08f, 6, EnemySwatch.BarkDark), meshKey: "OchokochiShin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -0.28f, -0.02f),
                    b => b.Blob(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.13f, 0.07f, 0.24f), EnemySwatch.Leather, coarse: true), meshKey: "OchokochiFoot");
            }
        }

        private static void DrawOchokochiBody(EnemyMeshBuilder b)
        {
            // Long dark shaggy hair all over, moss and beech leaves caught in it.
            b.Blob(new Vector3(0f, 0.24f, 0f), new Vector3(0.6f, 0.74f, 0.44f), EnemySwatch.BarkDark);
            for (int i = 0; i < 5; i++)
            {
                float a = i / 4f * Mathf.PI;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.26f, -0.06f + (i % 2) * 0.06f, -0.1f + Mathf.Sin(a) * 0.08f), new Vector3(0.16f, 0.3f, 0.16f), EnemySwatch.WolfFurDark, coarse: true);
            }

            b.Blob(new Vector3(0.16f, 0.48f, -0.1f), new Vector3(0.14f, 0.06f, 0.14f), EnemySwatch.CaveMoss, coarse: true);
            // The axe-blade growing out of his chest, the edge lit by the light the shadow trapped in it.
            b.Box(new Vector3(0f, 0.3f, 0.24f), new Vector3(0.05f, 0.34f, 0.14f), EnemySwatch.Steel);
            b.Box(new Vector3(0f, 0.3f, 0.32f), new Vector3(0.02f, 0.36f, 0.03f), EnemySwatch.Crystal);
        }

        private static void DrawOchokochiHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.28f, 0.3f, 0.28f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.06f, 0.1f), new Vector3(0.16f, 0.16f, 0.1f), EnemySwatch.CaveSkinDark, coarse: true);
            b.Blob(new Vector3(0f, 0.2f, -0.02f), new Vector3(0.3f, 0.16f, 0.28f), EnemySwatch.WolfFurDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.045f * s, 0.09f, 0.14f), new Vector3(0.035f, 0.025f, 0.02f), EnemySwatch.WarEye, coarse: true);
            }

            b.Blob(new Vector3(0f, -0.04f, 0.1f), new Vector3(0.22f, 0.16f, 0.14f), EnemySwatch.WolfFurDark, coarse: true);
        }

        private static void DrawOchokochiArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.BarkDark, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.04f * side, -0.36f, 0.05f), 0.08f, 0.07f, 6, EnemySwatch.BarkDark);
            b.Segment(new Vector3(0.04f * side, -0.36f, 0.05f), new Vector3(0.05f * side, -0.68f, 0.1f), 0.07f, 0.06f, 6, EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0.05f * side, -0.76f, 0.12f), new Vector3(0.13f, 0.13f, 0.13f), EnemySwatch.CaveSkinDark, coarse: true);
            // Long hooked nails.
            b.Cylinder(new Vector3(0.05f * side, -0.8f, 0.17f), 0.02f, 0f, 0.08f, 3, EnemySwatch.Horn, rotation: Quaternion.Euler(150f, 0f, 0f));
        }
    }
}
