using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Arquipélago de Hinokura (Docs/Mapas100.md 65–70) that the continuous world played with stand-ins, low-poly
    /// from the shared palette like <see cref="MarGeladoBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Caranguejo-Vulcão</b> (65, ~0.8 m): basalt crab with a little smoking cone on its back and ember cracks; pincers that
    /// open. <c>Body</c>, <c>Claw_L</c>/<c>Claw_R</c>; eight legs.</item>
    /// <item><b>Lagarto de Lava</b> (65, ~0.6 m, 1.4 m long): low black lizard with a glowing stripe down the back.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Gaivota-Brasa</b> (65, ~0.75 m): soot-grey gull with ember wing tips; [distância], spits embers.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Shisa Apagada</b> (66, ~1.2 m): roof lion-dog of red-glazed ceramic with a curled mane, the glaze dimmed and cracked.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four thick legs.</item>
    /// <item><b>Tanuki Ladrão</b> (66, ~0.8 m): round brown raccoon-dog, black mask, a ringed brush and a leaf on its head.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Tibicena</b> (67, ~0.95 m): lean black dog of the volcano's tale, ember eyes and ember down the throat.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Lagarto-Gigante de Lava</b> (67, ~1.1 m, 2.4 m long): broad lizard plated in basalt, lava in the cracks.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four heavy legs.</item>
    /// <item><b>Peixe-Lâmina</b> (68, ~0.8 m): flat fish of black glass with a cutting dorsal crest, swimming in the steam.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (blade fins), <c>Tail</c>.</item>
    /// <item><b>Coral de Brasa</b> (68, ~1.1 m): lump of coral that drifts on its root, ember at every branch tip; [distância], spits
    /// embers. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (fronds), <c>Tail</c> (the root).</item>
    /// <item><b>Ushabti de Laca</b> (69, ~1.5 m): servant figure lacquered red with gold bands, a tall black cap and a forge tongs.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Ogro-Fornalha</b> (69, ~2.1 m): iron-skinned ogre with a furnace grate in its belly and a hammer.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Gigante de Escória</b> (70, ~2.5 m): heap of slag walking, lava in the seams, huge fists.
    /// <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// </list>
    /// </summary>
    public static class HinokuraBeastRecipes
    {
        // ------------------------------------------------------------------
        // Shared
        // ------------------------------------------------------------------

        /// <summary>Four legs: a hip → knee → foot chain each (<c>Leg_FL</c>, <c>Leg_HR</c>…).</summary>
        internal static void AddFourLegs(ProceduralEnemyBlueprint bp, float hipY, float halfWidth, float front, float hind, float radius,
            EnemySwatch fur, EnemySwatch paw, string meshKey)
        {
            float upper = hipY * 0.5f;
            float lower = hipY - upper - 0.015f;
            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(halfWidth * side, hipY, front), new Vector3(0f, -upper, -0.02f),
                    new Vector3(0f, -lower, 0.03f), radius, fur, paw, meshKey + "Front");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(halfWidth * side, hipY, hind), new Vector3(0f, -upper, -0.05f),
                    new Vector3(0f, -lower, 0.05f), radius * 1.1f, fur, paw, meshKey + "Hind");
            }
        }

        /// <summary>Four thick legs with broad feet, for the heavy ones.</summary>
        internal static void AddHeavyLegs(ProceduralEnemyBlueprint bp, float hipY, float halfWidth, float front, float hind, float radius,
            EnemySwatch fur, EnemySwatch foot, string meshKey)
        {
            float upper = hipY * 0.5f;
            float lower = hipY - upper - 0.02f;
            Vector3 footSize = new Vector3(radius * 2.6f, 0.07f, radius * 3f);
            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(halfWidth * side, hipY, front), new Vector3(0f, -upper, 0.02f),
                    new Vector3(0f, -lower, 0.02f), radius, fur, foot, footSize, meshKey + "Front");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(halfWidth * side, hipY, hind), new Vector3(0f, -upper, -0.03f),
                    new Vector3(0f, -lower, 0.03f), radius, fur, foot, footSize, meshKey + "Hind");
            }
        }

        /// <summary>Two legs as thigh → shin → foot (the named biped parts ProceduralLegLocomotion drives).</summary>
        internal static void AddBipedLegs(ProceduralEnemyBlueprint bp, float hipY, float halfWidth, float radius, EnemySwatch thigh, EnemySwatch shinSwatch,
            EnemySwatch foot, string meshKey)
        {
            float upper = (hipY - 0.03f) * 0.5f;
            float lower = hipY - 0.03f - upper;
            for (int s = -1; s <= 1; s += 2)
            {
                bool right = s > 0;
                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1, new Vector3(halfWidth * s, hipY, 0f),
                    b => b.Segment(new Vector3(0f, -upper, 0.02f), new Vector3(0f, 0.04f, 0f), radius * 0.85f, radius, 6, thigh), meshKey: meshKey + "Thigh");
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg, new Vector3(0f, -upper, 0.02f),
                    b => b.Segment(new Vector3(0f, -lower, -0.01f), Vector3.zero, radius * 0.7f, radius * 0.85f, 6, shinSwatch), meshKey: meshKey + "Shin");
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin, new Vector3(0f, -lower, -0.01f),
                    b => b.Blob(new Vector3(0f, -0.005f, 0.05f), new Vector3(radius * 1.8f, 0.06f, radius * 3f), foot, coarse: true), meshKey: meshKey + "Foot");
            }
        }

        internal static void AddEyes(EnemyMeshBuilder b, float x, float y, float z, float size, EnemySwatch swatch)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(x * s, y, z), new Vector3(size, size, size * 0.75f), swatch, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Caranguejo-Vulcão
        // ------------------------------------------------------------------

        private const int CrabLegPairs = OverworldBeastRecipes.CrabLegPairs;
        private const float CrabHipHeight = 0.32f;
        private static readonly float[] CrabHipZ = { 0.13f, 0.04f, -0.06f, -0.15f };
        private static readonly float[] CrabSplay = { 40f, 12f, -15f, -42f };
        private static readonly Vector3 CrabKnee = new Vector3(0.24f, 0.15f, 0f);
        private static readonly Vector3 CrabTip = new Vector3(0.2f, -(CrabHipHeight + 0.15f - 0.01f), 0f);

        public static void BuildVolcanoCrab(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, CrabHipHeight, 0f), DrawVolcanoCrabBody);
            for (int side = -1; side <= 1; side += 2)
            {
                float sd = side;
                bp.Mesh(side < 0 ? ProceduralEnemyParts.ClawLeft : ProceduralEnemyParts.ClawRight, body, new Vector3(0.22f * side, 0.02f, 0.22f),
                    b => DrawVolcanoCrabClaw(b, sd));
            }

            for (int pair = 0; pair < CrabLegPairs; pair++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    string id = (side < 0 ? "L" : "R") + pair;
                    float yaw = side > 0 ? -CrabSplay[pair] : 180f + CrabSplay[pair];
                    int hip = bp.Mesh(ProceduralEnemyParts.LegPrefix + id, -1, new Vector3(0.18f * side, CrabHipHeight, CrabHipZ[pair]), DrawVolcanoCrabUpper,
                        Quaternion.Euler(0f, yaw, 0f), meshKey: "VolcanoCrabUpper");
                    int knee = bp.Mesh(ProceduralEnemyParts.ShinPrefix + id, hip, CrabKnee, DrawVolcanoCrabLower, meshKey: "VolcanoCrabLower");
                    bp.Node(ProceduralEnemyParts.FootPrefix + id, knee, CrabTip);
                }
            }
        }

        private static void DrawVolcanoCrabBody(EnemyMeshBuilder b)
        {
            // A wide shell of basalt, cracked with embers, and the little cone it grew on its back, still smoking.
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.66f, 0.3f, 0.54f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.5f, 0.14f, 0.42f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Cylinder(new Vector3(0f, 0.14f, -0.03f), 0.17f, 0.07f, 0.17f, 7, EnemySwatch.GolemStoneDark, cap: EnemySwatch.Ember);
            b.Rock(new Vector3(0.19f, 0.15f, 0.04f), new Vector3(0.12f, 0.08f, 0.12f), EnemySwatch.IronDark, 1);
            b.Rock(new Vector3(-0.19f, 0.15f, -0.08f), new Vector3(0.1f, 0.07f, 0.1f), EnemySwatch.GolemStoneDark, 2);
            b.Box(new Vector3(0.14f, 0.15f, -0.08f), new Vector3(0.03f, 0.02f, 0.2f), EnemySwatch.Ember, Quaternion.Euler(0f, 25f, 0f));
            b.Box(new Vector3(-0.13f, 0.15f, 0.08f), new Vector3(0.03f, 0.02f, 0.18f), EnemySwatch.Ember, Quaternion.Euler(0f, -30f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.06f * s, 0.12f, 0.24f), new Vector3(0.08f * s, 0.3f, 0.28f), 0.016f, 0.012f, 4, EnemySwatch.IronDark);
                b.Blob(new Vector3(0.08f * s, 0.32f, 0.28f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            }

            b.Box(new Vector3(0f, -0.02f, 0.27f), new Vector3(0.14f, 0.06f, 0.03f), EnemySwatch.Black);
        }

        private static void DrawVolcanoCrabClaw(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.1f * side, 0.02f, 0.14f), 0.05f, 0.045f, 5, EnemySwatch.IronDark);
            b.Blob(new Vector3(0.12f * side, 0.03f, 0.25f), new Vector3(0.17f, 0.14f, 0.22f), EnemySwatch.IronDark);
            b.Box(new Vector3(0.12f * side, 0.09f, 0.24f), new Vector3(0.02f, 0.015f, 0.12f), EnemySwatch.Ember);
            b.Cylinder(new Vector3(0.09f * side, 0.06f, 0.33f), 0.034f, 0.008f, 0.13f, 4, EnemySwatch.WarCape, rotation: Quaternion.Euler(80f, 0f, 0f));
            b.Cylinder(new Vector3(0.15f * side, 0f, 0.33f), 0.03f, 0.006f, 0.11f, 4, EnemySwatch.WarCape, rotation: Quaternion.Euler(95f, 0f, 0f));
        }

        private static void DrawVolcanoCrabUpper(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Segment(Vector3.zero, CrabKnee, 0.04f, 0.033f, 5, EnemySwatch.IronDark);
            b.Blob(CrabKnee, new Vector3(0.065f, 0.065f, 0.065f), EnemySwatch.WarCape, coarse: true);
        }

        private static void DrawVolcanoCrabLower(EnemyMeshBuilder b)
        {
            Vector3 mid = CrabTip * 0.5f;
            b.Segment(Vector3.zero, mid, 0.03f, 0.024f, 5, EnemySwatch.IronDark);
            b.Segment(mid, CrabTip, 0.024f, 0.006f, 4, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Lagarto de Lava
        // ------------------------------------------------------------------

        public static void BuildLavaLizard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.36f, 0f), DrawLavaLizardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.06f, 0.48f), DrawLavaLizardHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.06f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.1f, 0.025f, 0.15f), EnemySwatch.WarCape);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.03f * s, 0f, 0.12f), 0.008f, 0f, 0.03f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.42f), DrawLavaLizardTail);
            AddFourLegs(bp, 0.3f, 0.17f, 0.26f, -0.26f, 0.04f, EnemySwatch.IronDark, EnemySwatch.Black, "LavaLizard");
        }

        private static void DrawLavaLizardBody(EnemyMeshBuilder b)
        {
            // Long and low, black as cooled lava, with the stripe of live lava down the back that gives it away in the dark.
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.24f, 0.96f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, -0.05f, 0f), new Vector3(0.24f, 0.14f, 0.8f), EnemySwatch.GolemStoneDark, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                b.Box(new Vector3(0f, 0.12f, -0.36f + i * 0.14f), new Vector3(0.05f, 0.03f, 0.09f), EnemySwatch.Ember);
            }

            // The crest of cooled spines, tall enough to read over the rocks.
            for (int i = 0; i < 4; i++)
            {
                b.Crystal(new Vector3(0f, 0.1f, -0.24f + i * 0.16f), 0.035f, 0.17f, EnemySwatch.IronDark, sides: 4);
            }

            b.Segment(new Vector3(0f, 0.02f, 0.38f), new Vector3(0f, 0.06f, 0.48f), 0.11f, 0.09f, 5, EnemySwatch.IronDark);
        }

        private static void DrawLavaLizardHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.16f, 0.12f, 0.2f), EnemySwatch.IronDark);
            b.Box(new Vector3(0f, 0f, 0.13f), new Vector3(0.11f, 0.05f, 0.16f), EnemySwatch.GolemStoneDark);
            AddEyes(b, 0.06f, 0.05f, 0.04f, 0.035f, EnemySwatch.Ember);
        }

        private static void DrawLavaLizardTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.36f), 0.1f, 0.05f, 5, EnemySwatch.IronDark);
            b.Segment(new Vector3(0f, -0.04f, -0.36f), new Vector3(0f, -0.07f, -0.66f), 0.05f, 0.01f, 4, EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0f, 0.04f, -0.2f), new Vector3(0.04f, 0.03f, 0.12f), EnemySwatch.Ember);
        }

        // ------------------------------------------------------------------
        // Gaivota-Brasa
        // ------------------------------------------------------------------

        public static void BuildEmberGull(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawEmberGullBody, Quaternion.Euler(-8f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.12f), DrawEmberGullHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.04f, 0.12f), b =>
            {
                b.Cylinder(Vector3.zero, 0.02f, 0.006f, 0.1f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(95f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.005f, 0.07f), new Vector3(0.015f, 0.015f, 0.015f), EnemySwatch.Ember, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone, EnemySwatch.Ember, 0.9f),
                Quaternion.Euler(0f, 0f, -22f), meshKey: "EmberGullWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone, EnemySwatch.Ember, 0.9f),
                Quaternion.Euler(0f, 0f, 22f), meshKey: "EmberGullWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.18f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone),
                Quaternion.Euler(20f, 0f, 0f));
        }

        private static void DrawEmberGullBody(EnemyMeshBuilder b)
        {
            // Soot on the white: a gull that has flown too long over the forges, the embers caught under its feathers.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.32f, 0.44f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.07f, -0.05f), new Vector3(0.24f, 0.14f, 0.34f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(0.09f, 0.1f, -0.1f), new Vector3(0.05f, 0.03f, 0.06f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.08f, 0.11f, 0f), new Vector3(0.04f, 0.03f, 0.05f), EnemySwatch.Ember, coarse: true);
            RangedEnemyRecipes.DrawTalons(b, -0.4f, 0.06f, EnemySwatch.Black);
        }

        private static void DrawEmberGullHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.17f, 0.16f, 0.2f), EnemySwatch.GolemStone);
            AddEyes(b, 0.055f, 0.08f, 0.06f, 0.03f, EnemySwatch.Ember);
            b.Horn(new Vector3(0f, 0.07f, 0.09f), 0.022f, 0.12f, 3, 8f, EnemySwatch.Black, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
        }

        // ------------------------------------------------------------------
        // Shisa Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedShisa(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.68f, 0f), DrawShisaBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.34f, 0.46f), DrawShisaHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.13f, 0.14f), b =>
            {
                // The guardian's wide, square bottom jaw: the mouth that was open to keep bad things off the roof.
                b.Box(new Vector3(0f, -0.02f, 0.09f), new Vector3(0.3f, 0.06f, 0.2f), EnemySwatch.SandstoneDark);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.1f * s, 0.01f, 0.16f), 0.02f, 0f, 0.06f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.5f), DrawShisaTail, Quaternion.Euler(-30f, 0f, 0f));
            AddHeavyLegs(bp, 0.5f, 0.24f, 0.3f, -0.34f, 0.1f, EnemySwatch.SandstoneDark, EnemySwatch.WarCape, "Shisa");
        }

        private static void DrawShisaBody(EnemyMeshBuilder b)
        {
            // Fired clay with a red glaze, gone dull where the shadow touched it, the crack down the flank showing the ember inside.
            b.Blob(Vector3.zero, new Vector3(0.74f, 0.7f, 1.06f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, -0.1f, 0f), new Vector3(0.62f, 0.46f, 0.94f), EnemySwatch.SandstoneDark, coarse: true);
            b.Box(new Vector3(0.33f, 0.08f, -0.06f), new Vector3(0.02f, 0.26f, 0.04f), EnemySwatch.Ember, Quaternion.Euler(14f, 0f, 0f));
            b.Box(new Vector3(-0.32f, 0.02f, 0.14f), new Vector3(0.02f, 0.2f, 0.04f), EnemySwatch.Ember, Quaternion.Euler(-10f, 0f, 0f));
            // The roof tile it was made to sit on, still stuck to its back.
            b.Box(new Vector3(0f, 0.36f, -0.14f), new Vector3(0.34f, 0.04f, 0.3f), EnemySwatch.WarCape, Quaternion.Euler(6f, 0f, 0f), EnemySwatch.Horn);
            b.Segment(new Vector3(0f, 0.16f, 0.4f), new Vector3(0f, 0.34f, 0.46f), 0.26f, 0.22f, 6, EnemySwatch.WarCape);
        }

        private static void DrawShisaHead(EnemyMeshBuilder b)
        {
            // The big square head of the lion-dog, round eyes dimmed, and the mane in tight curls.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.42f, 0.42f), EnemySwatch.WarCape);
            b.Box(new Vector3(0f, -0.04f, 0.2f), new Vector3(0.3f, 0.12f, 0.16f), EnemySwatch.SandstoneDark);
            AddEyes(b, 0.14f, 0.08f, 0.18f, 0.08f, EnemySwatch.Ember);
            for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 7f;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.3f, 0.02f + Mathf.Sin(a) * 0.26f, -0.1f), new Vector3(0.13f, 0.13f, 0.13f), i % 2 == 0 ? EnemySwatch.WarCape : EnemySwatch.Sandstone, coarse: true);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.22f * s, 0.2f, -0.02f), new Vector3(0.1f, 0.12f, 0.06f), EnemySwatch.SandstoneDark, coarse: true);
            }
        }

        private static void DrawShisaTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, 0.16f, -0.16f), 0.06f, 0.05f, 5, EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, 0.22f, -0.2f), new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.Sandstone, coarse: true);
        }

        // ------------------------------------------------------------------
        // Tanuki Ladrão
        // ------------------------------------------------------------------

        public static void BuildTanuki(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawTanukiBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.32f), DrawTanukiHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.09f, 0.025f, 0.1f), EnemySwatch.WolfBelly));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.28f), DrawTanukiTail, Quaternion.Euler(-12f, 0f, 0f));
            AddFourLegs(bp, 0.3f, 0.12f, 0.17f, -0.17f, 0.04f, EnemySwatch.Black, EnemySwatch.Black, "Tanuki");
        }

        private static void DrawTanukiBody(EnemyMeshBuilder b)
        {
            // Round and well fed on stolen rice cakes; the coin pouch and the temple bell of a thief who is proud of it.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.42f, 0.58f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.32f, 0.3f, 0.34f), EnemySwatch.OwlBuff, coarse: true);
            b.Blob(new Vector3(0f, 0.17f, -0.05f), new Vector3(0.22f, 0.08f, 0.36f), EnemySwatch.Leather, coarse: true);
            b.Blob(new Vector3(0.2f, -0.05f, 0.02f), new Vector3(0.1f, 0.12f, 0.1f), EnemySwatch.Bark, coarse: true);
            b.Box(new Vector3(0.2f, -0.05f, 0.07f), new Vector3(0.03f, 0.03f, 0.02f), EnemySwatch.Gold);
        }

        private static void DrawTanukiHead(EnemyMeshBuilder b)
        {
            // The black mask, the small round ears and the leaf it wears on its head for the tricks.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.24f, 0.26f), EnemySwatch.PeccaryFur);
            b.Box(new Vector3(0f, 0.02f, 0.1f), new Vector3(0.24f, 0.08f, 0.08f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.02f, 0.14f), new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.OwlBuff, coarse: true);
            b.Blob(new Vector3(0f, -0.01f, 0.2f), new Vector3(0.035f, 0.03f, 0.03f), EnemySwatch.Black, coarse: true);
            AddEyes(b, 0.075f, 0.03f, 0.14f, 0.035f, EnemySwatch.MoonEye);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.1f * s, 0.12f, -0.02f), new Vector3(0.08f, 0.08f, 0.05f), EnemySwatch.Leather, coarse: true);
            }

            b.TwoSided(new Vector3(-0.05f, 0.13f, 0f), new Vector3(0.05f, 0.13f, 0.03f), new Vector3(0.01f, 0.24f, -0.03f), EnemySwatch.CaveMoss, EnemySwatch.Lichen);
        }

        private static void DrawTanukiTail(EnemyMeshBuilder b)
        {
            // A fat brush in dark rings.
            for (int i = 0; i < 4; i++)
            {
                float size = 0.2f - i * 0.03f;
                b.Blob(new Vector3(0f, 0.02f * i, -0.08f - i * 0.11f), new Vector3(size, size, 0.14f), i % 2 == 0 ? EnemySwatch.PeccaryFur : EnemySwatch.Black, coarse: i > 0);
            }
        }

        // ------------------------------------------------------------------
        // Tibicena
        // ------------------------------------------------------------------

        public static void BuildTibicena(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawTibicenaBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.16f, 0.42f), DrawTibicenaHead, Quaternion.Euler(6f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.09f), new Vector3(0.08f, 0.025f, 0.16f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, 0.008f, 0.08f), new Vector3(0.05f, 0.012f, 0.12f), EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.38f), DrawTibicenaTail, Quaternion.Euler(-20f, 0f, 0f));
            AddFourLegs(bp, 0.46f, 0.11f, 0.22f, -0.24f, 0.04f, EnemySwatch.Shadow, EnemySwatch.Black, "Tibicena");
        }

        private static void DrawTibicenaBody(EnemyMeshBuilder b)
        {
            // A long black dog with its ribs showing: a dog that is mostly hunger, and ember where the ribs meet.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.32f, 0.82f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.04f, 0.18f), new Vector3(0.26f, 0.3f, 0.3f), EnemySwatch.WolfFurDark, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.14f * s, 0f, -0.06f + i * 0.12f), new Vector3(0.015f, 0.16f, 0.03f), EnemySwatch.Ember);
                }
            }

            b.Segment(new Vector3(0f, 0.08f, 0.32f), new Vector3(0f, 0.16f, 0.42f), 0.09f, 0.07f, 5, EnemySwatch.Shadow);
        }

        private static void DrawTibicenaHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.16f, 0.22f), EnemySwatch.Shadow);
            b.Box(new Vector3(0f, -0.01f, 0.14f), new Vector3(0.09f, 0.06f, 0.18f), EnemySwatch.WolfFurDark);
            AddEyes(b, 0.06f, 0.05f, 0.06f, 0.04f, EnemySwatch.Ember);
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.04f * s, 0.07f, -0.02f), new Vector3(0.1f * s, 0.07f, -0.02f), new Vector3(0.09f * s, 0.2f, -0.06f), EnemySwatch.Shadow, EnemySwatch.Black);
            }
        }

        private static void DrawTibicenaTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.34f), 0.04f, 0.01f, 4, EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.06f, -0.34f), new Vector3(0.03f, 0.03f, 0.05f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Lagarto-Gigante de Lava
        // ------------------------------------------------------------------

        public static void BuildGiantLavaLizard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawGiantLizardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.76f), DrawGiantLizardHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.02f, 0.16f), new Vector3(0.28f, 0.05f, 0.32f), EnemySwatch.IronDark);
                b.Box(new Vector3(0f, 0.01f, 0.14f), new Vector3(0.18f, 0.015f, 0.24f), EnemySwatch.Ember);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.1f * s, 0f, 0.26f), 0.016f, 0f, 0.06f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.04f, -0.74f), DrawGiantLizardTail);
            AddHeavyLegs(bp, 0.4f, 0.36f, 0.42f, -0.42f, 0.1f, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark, "GiantLavaLizard");
        }

        private static void DrawGiantLizardBody(EnemyMeshBuilder b)
        {
            // Broad as a cart, plated in basalt slabs; the lava shows where the plates don't meet.
            b.Blob(Vector3.zero, new Vector3(0.84f, 0.56f, 1.6f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.7f, 0.34f, 1.4f), EnemySwatch.IronDark, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                float z = -0.56f + i * 0.28f;
                b.Rock(new Vector3(0f, 0.26f, z), new Vector3(0.36f, 0.14f, 0.22f), i % 2 == 0 ? EnemySwatch.GolemStoneDark : EnemySwatch.IronDark, i);
                b.Box(new Vector3(0f, 0.3f, z + 0.14f), new Vector3(0.26f, 0.02f, 0.03f), EnemySwatch.Ember);
            }

            b.Segment(new Vector3(0f, 0.04f, 0.68f), new Vector3(0f, 0.08f, 0.76f), 0.22f, 0.19f, 6, EnemySwatch.GolemStoneDark);
        }

        private static void DrawGiantLizardHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.38f, 0.26f, 0.42f), EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0f, 0f, 0.24f), new Vector3(0.26f, 0.1f, 0.3f), EnemySwatch.IronDark);
            AddEyes(b, 0.14f, 0.1f, 0.06f, 0.06f, EnemySwatch.Ember);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Rock(new Vector3(0.14f * s, 0.13f, -0.06f), new Vector3(0.09f, 0.08f, 0.1f), EnemySwatch.IronDark, s + 1);
            }
        }

        private static void DrawGiantLizardTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, -0.08f, -0.5f), 0.2f, 0.1f, 6, EnemySwatch.GolemStoneDark);
            b.Segment(new Vector3(0f, -0.08f, -0.5f), new Vector3(0f, -0.14f, -0.92f), 0.1f, 0.03f, 5, EnemySwatch.IronDark);
            b.Box(new Vector3(0f, 0.08f, -0.3f), new Vector3(0.05f, 0.03f, 0.16f), EnemySwatch.Ember);
        }

        // ------------------------------------------------------------------
        // Peixe-Lâmina
        // ------------------------------------------------------------------

        public static void BuildBladeFish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawBladeFishBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.01f, 0.44f), DrawBladeFishHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.05f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.08f, 0.025f, 0.2f), EnemySwatch.Steel);
                b.Cylinder(new Vector3(0f, 0.005f, 0.18f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.1f, -0.06f, 0.22f), b => DrawBladeFishFin(b, -1f), Quaternion.Euler(0f, 0f, -18f), meshKey: "BladeFishFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.1f, -0.06f, 0.22f), b => DrawBladeFishFin(b, 1f), Quaternion.Euler(0f, 0f, 18f), meshKey: "BladeFishFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.46f), DrawBladeFishTail);
        }

        private static void DrawBladeFishBody(EnemyMeshBuilder b)
        {
            // A knife that learned to swim: flat, black, shining, a crest of glass along its back and the low fins cutting the steam.
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.4f, 1f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, -0.1f, 0.02f), new Vector3(0.14f, 0.18f, 0.8f), EnemySwatch.Steel, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                b.Crystal(new Vector3(0f, 0.17f, -0.3f + i * 0.14f), 0.03f, 0.13f + (i % 2) * 0.05f, EnemySwatch.Black, sides: 4);
            }

            b.Box(new Vector3(0.09f, 0.04f, 0f), new Vector3(0.01f, 0.02f, 0.5f), EnemySwatch.Ember);
            b.Box(new Vector3(-0.09f, 0.04f, 0f), new Vector3(0.01f, 0.02f, 0.5f), EnemySwatch.Ember);
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.04f * s, -0.17f, 0.06f), new Vector3(0.05f * s, -0.38f, -0.04f), new Vector3(0.04f * s, -0.17f, -0.12f), EnemySwatch.IronDark, EnemySwatch.Steel);
            }
        }

        private static void DrawBladeFishHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.16f, 0.26f, 0.22f), EnemySwatch.IronDark);
            b.Cylinder(new Vector3(0f, 0.02f, 0.08f), 0.05f, 0.01f, 0.28f, 4, EnemySwatch.Steel, rotation: Quaternion.Euler(90f, 0f, 0f));
            AddEyes(b, 0.075f, 0.05f, 0.03f, 0.04f, EnemySwatch.Ember);
        }

        private static void DrawBladeFishFin(EnemyMeshBuilder b, float side)
        {
            b.TwoSided(Vector3.zero, new Vector3(0.28f * side, -0.08f, -0.04f), new Vector3(0.02f * side, 0f, -0.2f), EnemySwatch.Steel, EnemySwatch.IronDark);
        }

        private static void DrawBladeFishTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.12f), 0.05f, 0.03f, 4, EnemySwatch.IronDark);
            b.TwoSided(new Vector3(0f, 0f, -0.1f), new Vector3(0f, 0.22f, -0.32f), new Vector3(0f, 0f, -0.24f), EnemySwatch.Steel, EnemySwatch.IronDark);
            b.TwoSided(new Vector3(0f, 0f, -0.1f), new Vector3(0f, 0f, -0.24f), new Vector3(0f, -0.22f, -0.32f), EnemySwatch.Steel, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Coral de Brasa
        // ------------------------------------------------------------------

        public static void BuildEmberCoral(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawCoralBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.24f), DrawCoralHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.14f, 0.03f, 0.1f), EnemySwatch.WarCape);
                b.Blob(new Vector3(0f, 0.005f, 0.05f), new Vector3(0.06f, 0.02f, 0.05f), EnemySwatch.Ember, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.1f, 0f), b => DrawCoralFrond(b, -1f), Quaternion.Euler(0f, 0f, -14f), meshKey: "CoralFrond_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.1f, 0f), b => DrawCoralFrond(b, 1f), Quaternion.Euler(0f, 0f, 14f), meshKey: "CoralFrond_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.2f, 0f), DrawCoralRoot);
        }

        private static void DrawCoralBody(EnemyMeshBuilder b)
        {
            // Burnt-orange coral grown on black glass, that let go of the reef; an ember at each short branch.
            b.Blob(Vector3.zero, new Vector3(0.54f, 0.46f, 0.5f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, -0.12f, 0f), new Vector3(0.44f, 0.24f, 0.42f), EnemySwatch.IronDark, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI * 2f / 6f;
                Vector3 from = new Vector3(Mathf.Cos(a) * 0.16f, 0.14f, Mathf.Sin(a) * 0.16f);
                Vector3 to = new Vector3(Mathf.Cos(a) * 0.28f, 0.38f + (i % 2) * 0.1f, Mathf.Sin(a) * 0.28f);
                b.Segment(from, to, 0.04f, 0.018f, 4, EnemySwatch.WarCape);
                b.Blob(to, new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            }
        }

        private static void DrawCoralHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.18f, 0.2f), EnemySwatch.WarCape);
            b.Cylinder(new Vector3(0f, -0.02f, 0.06f), 0.09f, 0.11f, 0.05f, 6, EnemySwatch.Copper, rotation: Quaternion.Euler(90f, 0f, 0f));
            AddEyes(b, 0.07f, 0.05f, 0.06f, 0.035f, EnemySwatch.Ember);
        }

        private static void DrawCoralFrond(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.22f * side, 0.08f, 0f), 0.04f, 0.022f, 4, EnemySwatch.WarCape);
            b.Segment(new Vector3(0.14f * side, 0.05f, 0f), new Vector3(0.2f * side, 0.2f, 0.05f), 0.022f, 0.012f, 4, EnemySwatch.Copper);
            b.Segment(new Vector3(0.2f * side, 0.07f, 0f), new Vector3(0.36f * side, 0.14f, -0.05f), 0.02f, 0.01f, 4, EnemySwatch.Copper);
            b.Blob(new Vector3(0.36f * side, 0.14f, -0.05f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(0.2f * side, 0.2f, 0.05f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.Ember, coarse: true);
        }

        private static void DrawCoralRoot(EnemyMeshBuilder b)
        {
            // The broken root it still drags, the lowest point as it drifts over the pools.
            b.Segment(Vector3.zero, new Vector3(0.03f, -0.22f, 0f), 0.08f, 0.04f, 5, EnemySwatch.IronDark);
            b.Segment(new Vector3(0.03f, -0.22f, 0f), new Vector3(-0.02f, -0.42f, 0.02f), 0.04f, 0.012f, 4, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Ushabti de Laca
        // ------------------------------------------------------------------

        public static void BuildLacquerUshabti(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.84f, 0f), DrawLacquerBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.4f, 0.01f), DrawLacquerHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.25f * side, 0.28f, 0.02f),
                    b => DrawLacquerArm(b, side, right), Quaternion.Euler(-10f, 0f, 0f));
            }

            AddBipedLegs(bp, 0.54f, 0.1f, 0.075f, EnemySwatch.WarCape, EnemySwatch.Black, EnemySwatch.Gold, "LacquerUshabti");
        }

        private static void DrawLacquerBody(EnemyMeshBuilder b)
        {
            // Red lacquer polished like a mirror, gold bands, and the column of glowing signs that orders it to work.
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.44f, 0.66f, 0.32f), EnemySwatch.WarCape);
            for (int i = 0; i < 3; i++)
            {
                b.Cylinder(new Vector3(0f, -0.1f + i * 0.17f, 0f), 0.215f, 0.215f, 0.03f, 8, EnemySwatch.Gold);
            }

            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.22f - i * 0.08f, 0.17f), new Vector3(0.05f, 0.04f, 0.02f), EnemySwatch.Rune);
            }

            b.Cylinder(new Vector3(0f, -0.3f, 0f), 0.17f, 0.15f, 0.1f, 8, EnemySwatch.Black);
        }

        private static void DrawLacquerHead(EnemyMeshBuilder b)
        {
            // A calm painted face under the tall black cap of the forge's court, eyes dimmed to embers.
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.24f, 0.26f, 0.24f), EnemySwatch.Sandstone);
            b.Cylinder(new Vector3(0f, 0.13f, -0.01f), 0.13f, 0.09f, 0.22f, 7, EnemySwatch.Black, cap: EnemySwatch.Gold);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.05f * s, 0.07f, 0.12f), new Vector3(0.05f, 0.02f, 0.01f), EnemySwatch.Ember);
            }

            b.Box(new Vector3(0f, -0.06f, 0.11f), new Vector3(0.04f, 0.09f, 0.03f), EnemySwatch.Black);
        }

        private static void DrawLacquerArm(EnemyMeshBuilder b, float side, bool right)
        {
            b.Blob(Vector3.zero, new Vector3(0.13f, 0.12f, 0.12f), EnemySwatch.Gold, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.02f * side, -0.4f, 0.03f), 0.055f, 0.05f, 6, EnemySwatch.WarCape);
            b.Blob(new Vector3(0.02f * side, -0.44f, 0.04f), new Vector3(0.09f, 0.09f, 0.09f), EnemySwatch.Sandstone, coarse: true);
            if (right)
            {
                // The long forge tongs it was made to hand things to the smiths with.
                b.Cylinder(new Vector3(0.02f * side, -0.58f, 0.04f), 0.015f, 0.015f, 0.5f, 5, EnemySwatch.IronDark);
                b.Box(new Vector3(0.05f * side, -0.6f, 0.04f), new Vector3(0.02f, 0.14f, 0.04f), EnemySwatch.Iron);
                b.Box(new Vector3(0f, -0.6f, 0.04f), new Vector3(0.02f, 0.14f, 0.04f), EnemySwatch.Iron);
            }
        }

        // ------------------------------------------------------------------
        // Ogro-Fornalha
        // ------------------------------------------------------------------

        public static void BuildFurnaceOgre(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.18f, 0f), DrawOgreBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.56f, 0.04f), DrawOgreHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.5f * side, 0.36f, 0.02f),
                    b => DrawOgreArm(b, side, right), Quaternion.Euler(-14f, 0f, 0f));
            }

            AddBipedLegs(bp, 0.78f, 0.22f, 0.15f, EnemySwatch.Iron, EnemySwatch.IronDark, EnemySwatch.Black, "FurnaceOgre");
        }

        private static void DrawOgreBody(EnemyMeshBuilder b)
        {
            // An iron belly with a furnace door in it, bars and live coals behind; a chimney of a spine smoking off the shoulders.
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(1f, 0.96f, 0.76f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, 0.26f, -0.02f), new Vector3(0.94f, 0.5f, 0.66f), EnemySwatch.Iron, coarse: true);
            b.Box(new Vector3(0f, -0.02f, 0.36f), new Vector3(0.38f, 0.34f, 0.06f), EnemySwatch.Black);
            b.Box(new Vector3(0f, -0.02f, 0.37f), new Vector3(0.3f, 0.26f, 0.06f), EnemySwatch.Ember);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(-0.12f + i * 0.08f, -0.02f, 0.41f), new Vector3(0.03f, 0.3f, 0.03f), EnemySwatch.IronDark);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.2f * s, 0.42f, -0.24f), 0.07f, 0.06f, 0.34f, 6, EnemySwatch.Iron, cap: EnemySwatch.Ember);
            }
        }

        private static void DrawOgreHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.44f, 0.38f, 0.38f), EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0f, 0.08f, 0.16f), new Vector3(0.36f, 0.06f, 0.1f), EnemySwatch.IronDark);
            AddEyes(b, 0.11f, 0.03f, 0.18f, 0.06f, EnemySwatch.Ember);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.09f * s, -0.14f, 0.16f), 0.03f, 0f, 0.1f, 4, EnemySwatch.Fang);
                b.Horn(new Vector3(0.18f * s, 0.14f, 0f), 0.05f, 0.26f, 3, 12f * s, EnemySwatch.IronDark, Quaternion.Euler(0f, 0f, -18f * s), 5, Vector3.forward);
            }
        }

        private static void DrawOgreArm(EnemyMeshBuilder b, float side, bool right)
        {
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.26f, 0.26f), EnemySwatch.Iron, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.52f, 0.04f), 0.12f, 0.1f, 6, EnemySwatch.IronDark);
            b.Blob(new Vector3(0.06f * side, -0.62f, 0.05f), new Vector3(0.22f, 0.2f, 0.22f), EnemySwatch.GolemStoneDark, coarse: true);
            if (right)
            {
                // The forge hammer: iron on a charred haft, its head still hot.
                b.Cylinder(new Vector3(0.06f * side, -0.82f, 0.05f), 0.035f, 0.035f, 0.68f, 5, EnemySwatch.BarkDark);
                b.Box(new Vector3(0.06f * side, -0.82f, 0.05f), new Vector3(0.36f, 0.24f, 0.24f), EnemySwatch.IronDark, top: EnemySwatch.Ember);
            }
        }

        // ------------------------------------------------------------------
        // Gigante de Escória
        // ------------------------------------------------------------------

        public static void BuildSlagGiant(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.38f, 0f), DrawSlagBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.7f, 0.08f), DrawSlagHead);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.6f * side, 0.44f, 0.02f),
                    b => DrawSlagArm(b, side), Quaternion.Euler(-12f, 0f, 0f));
            }

            AddBipedLegs(bp, 0.9f, 0.28f, 0.2f, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark, EnemySwatch.BatFurDark, "SlagGiant");
        }

        private static void DrawSlagBody(EnemyMeshBuilder b)
        {
            // A heap of what the forges threw away, walking: dark clinker, rust, and lava still running in the seams.
            b.Rock(new Vector3(0f, 0.1f, 0f), new Vector3(1.2f, 1.1f, 0.9f), EnemySwatch.GolemStoneDark, 0);
            b.Rock(new Vector3(-0.28f, 0.42f, -0.06f), new Vector3(0.5f, 0.44f, 0.5f), EnemySwatch.IronDark, 1);
            b.Rock(new Vector3(0.3f, 0.46f, 0.04f), new Vector3(0.48f, 0.4f, 0.46f), EnemySwatch.BatFurDark, 2, upFacing: EnemySwatch.GolemStoneDark);
            b.Box(new Vector3(0.06f, 0.08f, 0.46f), new Vector3(0.05f, 0.62f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 12f));
            b.Box(new Vector3(-0.18f, 0.16f, 0.43f), new Vector3(0.28f, 0.04f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 20f));
            b.Box(new Vector3(0.3f, -0.1f, 0.38f), new Vector3(0.22f, 0.04f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -24f));
        }

        private static void DrawSlagHead(EnemyMeshBuilder b)
        {
            b.Rock(Vector3.zero, new Vector3(0.5f, 0.42f, 0.46f), EnemySwatch.IronDark, 3, upFacing: EnemySwatch.GolemStoneDark);
            AddEyes(b, 0.12f, 0.02f, 0.21f, 0.08f, EnemySwatch.GolemCrack);
            b.Box(new Vector3(0f, -0.12f, 0.2f), new Vector3(0.22f, 0.04f, 0.05f), EnemySwatch.GolemCrack);
        }

        private static void DrawSlagArm(EnemyMeshBuilder b, float side)
        {
            b.Rock(Vector3.zero, new Vector3(0.36f, 0.34f, 0.34f), EnemySwatch.IronDark, 1);
            b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.58f, 0.03f), 0.15f, 0.13f, 6, EnemySwatch.GolemStoneDark);
            b.Rock(new Vector3(0.06f * side, -0.72f, 0.04f), new Vector3(0.42f, 0.38f, 0.42f), EnemySwatch.GolemStoneDark, 2);
            b.Box(new Vector3(0.06f * side, -0.72f, 0.24f), new Vector3(0.2f, 0.03f, 0.03f), EnemySwatch.GolemCrack);
        }
    }
}
