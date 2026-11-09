using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Reinos do Céu (Docs/Mapas100.md 91–99) that the continuous world played with stand-ins, low-poly from the shared
    /// palette like <see cref="ProfundoBeastRecipes"/>: quiet, pale and a little silly, the end of the story. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Ave de Nuvem Apagada</b> (91, ~1.3 m): a grey bird of cloud gone out. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Peixe de Luz Apagado</b> (92, ~0.9 m): a dull gold fish swimming in the air, a spent drop on a thread. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (fins), <c>Tail</c>.</item>
    /// <item><b>Clandestino de Treva</b> (93, ~0.8 m): a low shadow with a stolen boarding tag. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Sombra do Porão</b> (93, ~1.1 m): a heap of shadow with cargo stuck in it and a lantern inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Corda Viva</b> (93, 97, ~1.5 m): a coil of ship's rope with a noose for a head. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (loose ends), <c>Tail</c>.</item>
    /// <item><b>Um dos Doze</b> (94, ~0.9 m): a toy ram with a tin crown and a wind-up key. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Jarra Rachada</b> (94, ~1.6 m): a jar of the Wheel, floating, light leaking from the crack. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Arconte Acorrentado</b> (95, ~2.3 m): a sulking giant in cuffs, the chains reaching the ground. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Gigante-Balão</b> (95, ~3.2 m): an archon blown up round like a balloon on two thin legs. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Ladrão de Āz</b> (96, ~1.5 m): hooded, a sack of light on its back. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Gaivota de Luz Apagada</b> (97, ~1.1 m): a pale gull, spits the last ember of its light. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Pedra do Céu</b> (99, ~1.4 m): a slab of the sky that fell, gold in the cracks, on four stumps. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four heavy legs.</item>
    /// </list>
    /// </summary>
    public static class CeuBeastRecipes
    {
        // ------------------------------------------------------------------
        // Ave de Nuvem Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedCloudBird(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.82f, 0f), DrawDimmedCloudBirdBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.32f, 0.2f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.28f, 0.3f), EnemySwatch.GolemSnow);
                b.Blob(new Vector3(0f, 0.08f, -0.08f), new Vector3(0.3f, 0.2f, 0.3f), EnemySwatch.GolemStone, coarse: true);
                b.Cylinder(new Vector3(0f, 0.01f, 0.12f), 0.05f, 0.015f, 0.26f, 5, EnemySwatch.Gold, rotation: Quaternion.Euler(84f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.04f, 0.1f, 0.05f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.12f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.07f, 0.025f, 0.18f), EnemySwatch.CopperDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.12f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.GolemSnow, EnemySwatch.GolemStone, EnemySwatch.Gold, 0.9f), Quaternion.Euler(0f, 0f, -18f), meshKey: "DimmedCloudBirdWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.12f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.GolemSnow, EnemySwatch.GolemStone, EnemySwatch.Gold, 0.9f), Quaternion.Euler(0f, 0f, 18f), meshKey: "DimmedCloudBirdWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.04f, -0.26f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.GolemSnow, EnemySwatch.GolemStone), Quaternion.Euler(18f, 0f, 0f));
        }

        private static void DrawDimmedCloudBirdBody(EnemyMeshBuilder b)
        {
            // Birds made of cloud carry a little of the light up; this one set it down somewhere and cannot remember where.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.52f, 0.68f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, -0.14f, 0.04f), new Vector3(0.42f, 0.28f, 0.48f), EnemySwatch.Ivory, coarse: true);
            b.Blob(new Vector3(0f, 0.16f, -0.14f), new Vector3(0.36f, 0.2f, 0.42f), EnemySwatch.GolemStone, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.1f * side, -0.2f, 0f), new Vector3(0.12f * side, -0.8f, 0.04f), 0.03f, 0.018f, 5, EnemySwatch.CopperDark);
                b.Box(new Vector3(0.12f * side, -0.8f, 0.08f), new Vector3(0.08f, 0.02f, 0.14f), EnemySwatch.CopperDark);
            }
        }

        // ------------------------------------------------------------------
        // Peixe de Luz Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedLightFish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawDimmedLightFishBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.01f, 0.44f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.3f, 0.3f), EnemySwatch.SandstoneDark);
                b.Box(new Vector3(0f, 0.05f, 0.14f), new Vector3(0.12f, 0.06f, 0.14f), EnemySwatch.Sandstone);
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.06f, 0.08f, 0.06f, EnemySwatch.Crystal);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.06f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.1f, 0.025f, 0.2f), EnemySwatch.Sandstone);
                b.Cylinder(new Vector3(0f, 0.005f, 0.18f), 0.014f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.12f, -0.04f, 0.16f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, EnemySwatch.Gold, 0.56f), Quaternion.Euler(0f, 0f, -16f), meshKey: "DimmedLightFishFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.12f, -0.04f, 0.16f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, EnemySwatch.Gold, 0.56f), Quaternion.Euler(0f, 0f, 16f), meshKey: "DimmedLightFishFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.2f), 0.08f, 0.03f, 5, EnemySwatch.SandstoneDark);
                b.Box(new Vector3(0f, 0f, -0.28f), new Vector3(0.03f, 0.34f, 0.16f), EnemySwatch.Sandstone);
            });
        }

        private static void DrawDimmedLightFishBody(EnemyMeshBuilder b)
        {
            // The River of Light is a road, and these fish are the ones who stopped carrying anything along it. The drop hanging from the belly is what they have left.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.46f, 0.96f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, 0.14f, -0.02f), new Vector3(0.26f, 0.2f, 0.8f), EnemySwatch.SandstoneDark, coarse: true);
            b.Blob(new Vector3(0f, -0.12f, 0.02f), new Vector3(0.3f, 0.2f, 0.7f), EnemySwatch.SandstoneLight, coarse: true);
            b.Box(new Vector3(0f, 0.3f, -0.04f), new Vector3(0.03f, 0.2f, 0.42f), EnemySwatch.SandstoneDark);
            b.Segment(new Vector3(0f, -0.2f, 0.1f), new Vector3(0f, -0.42f, 0.1f), 0.012f, 0.01f, 3, EnemySwatch.Bone);
            b.Blob(new Vector3(0f, -0.42f, 0.1f), new Vector3(0.12f, 0.14f, 0.12f), EnemySwatch.Gold, coarse: true);
        }

        // ------------------------------------------------------------------
        // Clandestino de Treva
        // ------------------------------------------------------------------

        public static void BuildDarkStowaway(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawDarkStowawayBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.46f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.32f, 0.26f, 0.32f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, -0.03f, 0.2f), new Vector3(0.14f, 0.09f, 0.2f), EnemySwatch.Shadow);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.06f, 0.14f, 0.055f, EnemySwatch.MoonEye);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.1f * s, 0.1f, -0.06f), 0.05f, 0f, 0.16f, 4, EnemySwatch.Black);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.09f), new Vector3(0.12f, 0.035f, 0.18f), EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.01f, 0.09f), new Vector3(0.09f, 0.01f, 0.14f), EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.06f, -0.4f), 0.07f, 0.03f, 5, EnemySwatch.Shadow);
                b.Blob(new Vector3(0f, 0.08f, -0.44f), new Vector3(0.12f, 0.12f, 0.14f), EnemySwatch.Black, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.42f, 0.2f, 0.28f, -0.3f, 0.07f, EnemySwatch.Shadow, EnemySwatch.Black, "DarkStowaway");
        }

        private static void DrawDarkStowawayBody(EnemyMeshBuilder b)
        {
            // A piece of the dark that heard the boats go up and thought: why not me. The tag round its neck is someone else's.
            b.Blob(Vector3.zero, new Vector3(0.68f, 0.44f, 0.92f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.14f, -0.04f), new Vector3(0.44f, 0.22f, 0.7f), EnemySwatch.Black, coarse: true);
            b.Blob(new Vector3(0f, -0.1f, 0.02f), new Vector3(0.42f, 0.2f, 0.66f), EnemySwatch.Shroud, coarse: true);
            b.Segment(new Vector3(-0.16f, 0.12f, 0.32f), new Vector3(0.16f, 0.12f, 0.32f), 0.012f, 0.012f, 3, EnemySwatch.Gold);
            b.Box(new Vector3(0f, -0.01f, 0.4f), new Vector3(0.12f, 0.16f, 0.02f), EnemySwatch.Ivory);
            b.Box(new Vector3(0f, -0.01f, 0.412f), new Vector3(0.08f, 0.02f, 0.01f), EnemySwatch.Copper);
        }

        // ------------------------------------------------------------------
        // Sombra do Porão
        // ------------------------------------------------------------------

        public static void BuildHoldShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawHoldShadowBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.14f, 0.5f), b =>
            {
                // The ship's lantern it sat on: still lit, and rather put out about it.
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.3f, 0.26f), EnemySwatch.StolenLight);
                b.Cylinder(new Vector3(0f, 0.12f, 0f), 0.08f, 0.12f, 0.08f, 6, EnemySwatch.CopperDark, bottomCap: true);
                b.Segment(new Vector3(0f, 0.2f, 0f), new Vector3(0f, 0.3f, 0f), 0.012f, 0.012f, 3, EnemySwatch.CopperDark);
            });
        }

        private static void DrawHoldShadowBody(EnemyMeshBuilder b)
        {
            // It got on at the bottom of the world and has been sitting on the luggage ever since. It will get off when someone makes it.
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(1.58f, 1.18f, 1.5f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.3f, -0.04f), new Vector3(1.1f, 0.62f, 1.06f), EnemySwatch.Black, coarse: true);
            b.Blob(new Vector3(0f, -0.48f, 0f), new Vector3(1.9f, 0.3f, 1.8f), EnemySwatch.Shroud, coarse: true);
            b.Box(new Vector3(-0.48f, 0.26f, 0.3f), new Vector3(0.34f, 0.3f, 0.3f), EnemySwatch.Bark, Quaternion.Euler(8f, 22f, -14f), top: EnemySwatch.BarkDark);
            b.Box(new Vector3(0.5f, 0.42f, 0.18f), new Vector3(0.28f, 0.24f, 0.28f), EnemySwatch.BarkDark, Quaternion.Euler(-10f, -30f, 12f));
            b.Box(new Vector3(0.28f, -0.1f, 0.62f), new Vector3(0.36f, 0.18f, 0.2f), EnemySwatch.Bark, Quaternion.Euler(0f, 18f, 8f));
            HinokuraBeastRecipes.AddEyes(b, 0.3f, 0.34f, 0.66f, 0.1f, EnemySwatch.MoonEye);
        }

        // ------------------------------------------------------------------
        // Corda Viva
        // ------------------------------------------------------------------

        public static void BuildLivingRope(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawLivingRopeBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.6f, 0.04f), b =>
            {
                // The noose: a knot with two knots for eyes.
                b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.28f, 0.26f, 0.28f), EnemySwatch.Leather);
                b.Cylinder(new Vector3(0f, -0.1f, 0f), 0.12f, 0.1f, 0.14f, 6, EnemySwatch.Bark, bottomCap: true);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.06f, 0.12f, 0.05f, EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.12f), b =>
                b.Box(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.14f, 0.04f, 0.12f), EnemySwatch.Bark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.3f, 0.18f, 0f), b => DrawRopeEnd(b, -1f), Quaternion.Euler(0f, 0f, -26f), meshKey: "LivingRopeEnd_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.3f, 0.18f, 0f), b => DrawRopeEnd(b, 1f), Quaternion.Euler(0f, 0f, 26f), meshKey: "LivingRopeEnd_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.26f, -0.4f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.22f, -0.42f), 0.06f, 0.05f, 5, EnemySwatch.Leather);
                b.Blob(new Vector3(0f, -0.26f, -0.46f), new Vector3(0.16f, 0.14f, 0.16f), EnemySwatch.Bark, coarse: true);
            });
        }

        private static void DrawRopeEnd(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.28f * side, 0.1f, 0.02f), 0.05f, 0.045f, 5, EnemySwatch.Leather);
            b.Segment(new Vector3(0.28f * side, 0.1f, 0.02f), new Vector3(0.5f * side, -0.06f, 0.06f), 0.045f, 0.03f, 5, EnemySwatch.Bark);
            b.Blob(new Vector3(0.52f * side, -0.08f, 0.06f), new Vector3(0.11f, 0.13f, 0.11f), EnemySwatch.Leather, coarse: true);
        }

        private static void DrawLivingRopeBody(EnemyMeshBuilder b)
        {
            // A ship's rope that was tied to a shadow for one crossing too many, and picked up the habit of grabbing things.
            for (int i = 0; i < 5; i++)
            {
                float y = -0.66f + i * 0.28f;
                float width = 0.98f - i * 0.12f;
                b.Blob(new Vector3(0f, y, 0f), new Vector3(width, 0.2f, width), i % 2 == 0 ? EnemySwatch.Leather : EnemySwatch.Bark);
            }

            b.Segment(new Vector3(0f, 0.46f, 0f), new Vector3(0f, 0.6f, 0.02f), 0.09f, 0.08f, 6, EnemySwatch.Leather);
            b.Box(new Vector3(0f, -0.72f, 0f), new Vector3(0.5f, 0.03f, 0.5f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 30f, 0f));
        }

        // ------------------------------------------------------------------
        // Um dos Doze (Os Doze da Roda)
        // ------------------------------------------------------------------

        public static void BuildToyArchon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.52f, 0f), DrawToyArchonBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.18f, 0.42f), DrawToyArchonHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.14f), b =>
                b.Box(new Vector3(0f, -0.015f, 0.07f), new Vector3(0.14f, 0.04f, 0.14f), EnemySwatch.Bark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.42f), b =>
                b.Blob(new Vector3(0f, 0.02f, -0.08f), new Vector3(0.16f, 0.16f, 0.2f), EnemySwatch.Ivory, coarse: true));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.44f, 0.22f, 0.26f, -0.26f, 0.075f, EnemySwatch.Bark, EnemySwatch.BarkDark, "ToyArchon");
        }

        private static void DrawToyArchonBody(EnemyMeshBuilder b)
        {
            // The twelve signs were archons once, tied to the Wheel; up here they have worn down into toys, and they are furious about it.
            b.Blob(Vector3.zero, new Vector3(0.7f, 0.56f, 0.86f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, 0.12f, -0.04f), new Vector3(0.56f, 0.28f, 0.68f), EnemySwatch.IvoryDark, coarse: true);
            b.Box(new Vector3(0f, -0.16f, 0f), new Vector3(0.6f, 0.08f, 0.8f), EnemySwatch.Copper);
            b.Cylinder(new Vector3(0f, 0.24f, -0.1f), 0.03f, 0.03f, 0.2f, 5, EnemySwatch.Steel);
            b.Box(new Vector3(0f, 0.46f, -0.1f), new Vector3(0.3f, 0.09f, 0.04f), EnemySwatch.Steel);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.17f * s, 0.46f, -0.1f), new Vector3(0.11f, 0.14f, 0.05f), EnemySwatch.Steel, coarse: true);
            }
        }

        private static void DrawToyArchonHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.34f, 0.3f, 0.34f), EnemySwatch.IvoryDark);
            b.Box(new Vector3(0f, -0.05f, 0.18f), new Vector3(0.17f, 0.1f, 0.16f), EnemySwatch.Bark);
            HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.07f, 0.14f, 0.06f, EnemySwatch.MoonEye);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.13f * side, 0.1f, -0.04f), new Vector3(0.28f * side, 0.14f, -0.08f), 0.05f, 0.035f, 5, EnemySwatch.Copper);
                b.Segment(new Vector3(0.28f * side, 0.14f, -0.08f), new Vector3(0.3f * side, 0f, -0.02f), 0.035f, 0.02f, 5, EnemySwatch.CopperDark);
            }

            // The tin crown: five points and a dent.
            b.Cylinder(new Vector3(0f, 0.13f, 0f), 0.14f, 0.15f, 0.08f, 8, EnemySwatch.Gold, bottomCap: true);
            for (int i = 0; i < 5; i++)
            {
                float angle = i * 72f * Mathf.Deg2Rad;
                b.Cylinder(new Vector3(Mathf.Sin(angle) * 0.13f, 0.21f, Mathf.Cos(angle) * 0.13f), 0.025f, 0f, 0.08f, 3, EnemySwatch.Gold);
            }
        }

        // ------------------------------------------------------------------
        // Jarra Rachada
        // ------------------------------------------------------------------

        public static void BuildCrackedJar(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.9f, 0f), DrawCrackedJarBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.1f, 0.04f, 0.32f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.3f, 0.12f), EnemySwatch.StolenLight);
                b.Box(new Vector3(0.14f, 0.12f, 0f), new Vector3(0.2f, 0.03f, 0.03f), EnemySwatch.SoulFire, Quaternion.Euler(0f, 0f, 40f));
                b.Box(new Vector3(-0.12f, -0.14f, 0f), new Vector3(0.24f, 0.03f, 0.03f), EnemySwatch.SoulFire, Quaternion.Euler(0f, 0f, 48f));
            });
        }

        private static void DrawCrackedJarBody(EnemyMeshBuilder b)
        {
            // Twelve jars carry the light up the Wheel. This one cracked on the way, and what leaks out comes out sideways, fast, at you.
            b.Cylinder(new Vector3(0f, -0.72f, 0f), 0.26f, 0.42f, 0.46f, 12, EnemySwatch.Copper, bottomCap: true);
            b.Blob(new Vector3(0f, -0.02f, 0f), new Vector3(1f, 1.04f, 1f), EnemySwatch.Copper);
            b.Cylinder(new Vector3(0f, 0.4f, 0f), 0.36f, 0.18f, 0.3f, 12, EnemySwatch.CopperDark);
            b.Cylinder(new Vector3(0f, 0.66f, 0f), 0.18f, 0.26f, 0.12f, 12, EnemySwatch.Copper, EnemySwatch.Black, bottomCap: true);
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.96f, 0.1f, 0.96f), EnemySwatch.Verdigris, coarse: true);
            b.Blob(new Vector3(0f, -0.28f, 0f), new Vector3(0.9f, 0.08f, 0.9f), EnemySwatch.Verdigris, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.18f * side, 0.56f, 0f), new Vector3(0.62f * side, 0.42f, 0f), 0.04f, 0.04f, 5, EnemySwatch.CopperDark);
                b.Segment(new Vector3(0.62f * side, 0.42f, 0f), new Vector3(0.5f * side, 0.04f, 0f), 0.04f, 0.04f, 5, EnemySwatch.CopperDark);
            }

            // A drip of the light that got out, reaching the deck.
            b.Segment(new Vector3(0.1f, -0.2f, 0.42f), new Vector3(0.08f, -0.86f, 0.46f), 0.03f, 0.008f, 4, EnemySwatch.StolenLight);
        }

        // ------------------------------------------------------------------
        // Arconte Acorrentado
        // ------------------------------------------------------------------

        public static void BuildChainedArchon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.1f, 0f), DrawChainedArchonBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.66f, 0.06f), b =>
            {
                b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.5f, 0.46f, 0.46f), EnemySwatch.GolemStone);
                b.Box(new Vector3(0f, 0.14f, 0.22f), new Vector3(0.36f, 0.05f, 0.04f), EnemySwatch.GolemStoneDark, Quaternion.Euler(0f, 0f, 0f));
                b.Box(new Vector3(0f, -0.06f, 0.22f), new Vector3(0.18f, 0.03f, 0.03f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.09f, 0.2f, 0.07f, EnemySwatch.StolenLight);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.62f * side, 0.42f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.36f, 0.34f, 0.36f), EnemySwatch.GolemStone, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.1f * side, -0.68f, 0.06f), 0.16f, 0.14f, 6, EnemySwatch.GolemStoneDark);
                    b.Cylinder(new Vector3(0.1f * side, -0.68f, 0.06f), 0.16f, 0.16f, 0.14f, 8, EnemySwatch.Iron, bottomCap: true);
                    b.Blob(new Vector3(0.1f * side, -0.82f, 0.06f), new Vector3(0.36f, 0.32f, 0.36f), EnemySwatch.GolemStone);
                    for (int i = 0; i < 4; i++)
                    {
                        b.Box(new Vector3(0.14f * side, -0.98f - i * 0.1f, 0.06f + (i % 2) * 0.02f), new Vector3(0.08f, 0.08f, 0.03f), EnemySwatch.Steel, Quaternion.Euler(0f, 0f, i % 2 == 0 ? 25f : -25f));
                    }
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: s > 0 ? "ChainedArchonArm_R" : "ChainedArchonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.76f, 0.26f, 0.18f, EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone, EnemySwatch.Iron, "ChainedArchon");
        }

        private static void DrawChainedArchonBody(EnemyMeshBuilder b)
        {
            // The Living Spirit chained the archons in the spheres to keep them from eating the light. They have had a very long time to complain about it.
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(1.12f, 1.2f, 0.92f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.36f, 0.02f), new Vector3(1.18f, 0.48f, 0.84f), EnemySwatch.GolemStone, coarse: true);
            b.Box(new Vector3(0f, -0.4f, 0f), new Vector3(1.02f, 0.22f, 0.86f), EnemySwatch.Iron);
            b.Blob(new Vector3(0f, 0f, 0.42f), new Vector3(0.32f, 0.3f, 0.12f), EnemySwatch.StolenLight, coarse: true);
            b.Box(new Vector3(0f, 0.56f, 0f), new Vector3(0.66f, 0.1f, 0.66f), EnemySwatch.Iron);
        }

        // ------------------------------------------------------------------
        // Gigante-Balão
        // ------------------------------------------------------------------

        public static void BuildBalloonGiant(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.74f, 0f), DrawBalloonGiantBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 1.12f, 0.12f), b =>
            {
                b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.62f, 0.58f, 0.58f), EnemySwatch.Ivory);
                b.Blob(new Vector3(0f, 0.3f, -0.04f), new Vector3(0.42f, 0.2f, 0.42f), EnemySwatch.Sorcery, coarse: true);
                b.Box(new Vector3(0f, -0.08f, 0.27f), new Vector3(0.26f, 0.03f, 0.03f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.1f, 0.24f, 0.08f, EnemySwatch.Black);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.94f * side, 0.2f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.34f, 0.32f, 0.34f), EnemySwatch.Ivory, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.12f * side, -0.72f, 0.04f), 0.08f, 0.07f, 6, EnemySwatch.IvoryDark);
                    b.Blob(new Vector3(0.12f * side, -0.78f, 0.04f), new Vector3(0.4f, 0.4f, 0.4f), EnemySwatch.Sorcery);
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: s > 0 ? "BalloonGiantArm_R" : "BalloonGiantArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.96f, 0.36f, 0.12f, EnemySwatch.IvoryDark, EnemySwatch.Ivory, EnemySwatch.Copper, "BalloonGiant");
        }

        private static void DrawBalloonGiantBody(EnemyMeshBuilder b)
        {
            // The ones held in the spheres swell up with everything they refuse to let go. This one has gone completely round and can only reach things by being angry at them.
            b.Blob(Vector3.zero, new Vector3(2.08f, 2.08f, 1.96f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(2.14f, 0.22f, 2.02f), EnemySwatch.Sorcery, coarse: true);
            b.Blob(new Vector3(0f, 0.5f, 0f), new Vector3(1.78f, 0.18f, 1.72f), EnemySwatch.Gold, coarse: true);
            b.Blob(new Vector3(0f, -0.5f, 0f), new Vector3(1.78f, 0.18f, 1.72f), EnemySwatch.Gold, coarse: true);
            b.Blob(new Vector3(0f, 0.16f, 0.92f), new Vector3(0.3f, 0.26f, 0.08f), EnemySwatch.StolenLight, coarse: true);
            b.Segment(new Vector3(0f, -0.92f, 0f), new Vector3(0f, -1.1f, 0.02f), 0.1f, 0.06f, 5, EnemySwatch.Sorcery);
        }

        // ------------------------------------------------------------------
        // Ladrão de Āz
        // ------------------------------------------------------------------

        public static void BuildAzThief(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.86f, 0f), DrawAzThiefBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.04f), b =>
            {
                b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.36f, 0.36f, 0.34f), EnemySwatch.WarCape);
                b.Cylinder(new Vector3(0f, 0.2f, -0.06f), 0.13f, 0f, 0.28f, 6, EnemySwatch.WarCape, rotation: Quaternion.Euler(-24f, 0f, 0f), bottomCap: true);
                b.Blob(new Vector3(0f, 0.04f, 0.13f), new Vector3(0.22f, 0.18f, 0.1f), EnemySwatch.Black, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.06f, 0.16f, 0.04f, EnemySwatch.StolenLight);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s > 0 ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.3f * side, 0.24f, 0f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.3f, 0.1f), 0.07f, 0.055f, 6, EnemySwatch.WarCape);
                    b.Segment(new Vector3(0.06f * side, -0.3f, 0.1f), new Vector3(0.06f * side, -0.58f, 0.2f), 0.055f, 0.04f, 6, EnemySwatch.Leather);
                    b.Blob(new Vector3(0.06f * side, -0.62f, 0.22f), new Vector3(0.13f, 0.14f, 0.13f), EnemySwatch.Gold, coarse: true);
                }, Quaternion.Euler(-12f, 0f, 0f), meshKey: s > 0 ? "AzThiefArm_R" : "AzThiefArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.56f, 0.14f, 0.09f, EnemySwatch.WarCape, EnemySwatch.Leather, EnemySwatch.Black, "AzThief");
        }

        private static void DrawAzThiefBody(EnemyMeshBuilder b)
        {
            // Āz wants the whole garden in one chest, and she has people for that. The sack is full and they are still reaching.
            b.Cylinder(new Vector3(0f, -0.3f, 0f), 0.26f, 0.2f, 0.62f, 9, EnemySwatch.WarCape, bottomCap: true);
            b.Blob(new Vector3(0f, 0.16f, 0f), new Vector3(0.52f, 0.34f, 0.42f), EnemySwatch.Leather, coarse: true);
            b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.44f, 0.48f, 0.38f), EnemySwatch.WarCape);
            b.Box(new Vector3(0f, -0.14f, 0.2f), new Vector3(0.36f, 0.05f, 0.03f), EnemySwatch.Gold);

            // The sack on its back, with the light showing through the weave.
            b.Blob(new Vector3(0f, 0.2f, -0.36f), new Vector3(0.56f, 0.6f, 0.46f), EnemySwatch.SandstoneLight);
            b.Blob(new Vector3(0f, 0.2f, -0.54f), new Vector3(0.24f, 0.24f, 0.08f), EnemySwatch.StolenLight, coarse: true);
            b.Segment(new Vector3(0f, 0.5f, -0.3f), new Vector3(0f, 0.58f, -0.24f), 0.05f, 0.03f, 5, EnemySwatch.Leather);
        }

        // ------------------------------------------------------------------
        // Gaivota de Luz Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedLightGull(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawDimmedLightGullBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.28f, 0.16f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.28f), EnemySwatch.Ivory);
                b.Box(new Vector3(0f, 0.02f, 0.16f), new Vector3(0.09f, 0.05f, 0.16f), EnemySwatch.Gold);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.05f, 0.09f, 0.045f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.08f, 0.025f, 0.16f), EnemySwatch.Copper));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.16f, 0.14f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Ivory, EnemySwatch.IvoryDark, EnemySwatch.Ember, 0.95f), Quaternion.Euler(0f, 0f, -14f), meshKey: "DimmedLightGullWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.16f, 0.14f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Ivory, EnemySwatch.IvoryDark, EnemySwatch.Ember, 0.95f), Quaternion.Euler(0f, 0f, 14f), meshKey: "DimmedLightGullWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.04f, -0.24f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Ivory, EnemySwatch.Gold), Quaternion.Euler(20f, 0f, 0f));
        }

        private static void DrawDimmedLightGullBody(EnemyMeshBuilder b)
        {
            // Gulls follow boats, even these. This one followed the Sun's boat so long it burned out, and it spits the last of its light at anyone who looks as if they have some.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.42f, 0.6f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, -0.12f, 0.02f), new Vector3(0.34f, 0.24f, 0.42f), EnemySwatch.IvoryDark, coarse: true);
            b.Blob(new Vector3(0f, 0.04f, 0.12f), new Vector3(0.2f, 0.16f, 0.14f), EnemySwatch.Ember, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.08f * side, -0.16f, 0f), new Vector3(0.1f * side, -0.7f, 0.02f), 0.025f, 0.015f, 5, EnemySwatch.Copper);
                b.Box(new Vector3(0.1f * side, -0.7f, 0.06f), new Vector3(0.1f, 0.02f, 0.12f), EnemySwatch.Copper);
            }
        }

        // ------------------------------------------------------------------
        // Pedra do Céu
        // ------------------------------------------------------------------

        public static void BuildSkyStone(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.88f, 0f), DrawSkyStoneBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.8f), b =>
            {
                b.Rock(Vector3.zero, new Vector3(0.62f, 0.5f, 0.5f), EnemySwatch.GolemSnow, 1);
                b.Box(new Vector3(0f, 0.02f, 0.24f), new Vector3(0.34f, 0.03f, 0.02f), EnemySwatch.Gold);
                HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.12f, 0.22f, 0.08f, EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.18f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.04f, 0.1f), new Vector3(0.42f, 0.1f, 0.22f), EnemySwatch.GolemStone);
                b.Box(new Vector3(0f, 0.015f, 0.1f), new Vector3(0.3f, 0.012f, 0.16f), EnemySwatch.Gold);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.86f), b =>
                b.Rock(new Vector3(0f, -0.02f, -0.14f), new Vector3(0.28f, 0.24f, 0.34f), EnemySwatch.GolemStone, 2));
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.66f, 0.46f, 0.56f, -0.56f, 0.15f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, "SkyStone");
        }

        private static void DrawSkyStoneBody(EnemyMeshBuilder b)
        {
            // A piece of the ceiling. It came down through one of the cracks at the Lookout and has been trying to climb back up ever since, through whoever is in the way.
            b.Rock(new Vector3(0f, 0.02f, 0f), new Vector3(1.5f, 1.1f, 1.7f), EnemySwatch.GolemSnow, 0);
            b.Rock(new Vector3(-0.28f, 0.42f, 0.14f), new Vector3(0.72f, 0.46f, 0.82f), EnemySwatch.GolemStone, 1);
            b.Rock(new Vector3(0.34f, 0.36f, -0.28f), new Vector3(0.68f, 0.4f, 0.7f), EnemySwatch.GolemStone, 2);
            b.Box(new Vector3(0.05f, 0.1f, 0.74f), new Vector3(0.05f, 0.62f, 0.03f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, 18f));
            b.Box(new Vector3(-0.32f, -0.1f, 0.66f), new Vector3(0.04f, 0.4f, 0.03f), EnemySwatch.Gold, Quaternion.Euler(0f, 0f, -30f));
            b.Box(new Vector3(0.68f, 0.02f, 0.1f), new Vector3(0.03f, 0.54f, 0.5f), EnemySwatch.Gold);
            b.Box(new Vector3(-0.7f, 0.08f, -0.08f), new Vector3(0.03f, 0.42f, 0.46f), EnemySwatch.Gold);
        }
    }
}
