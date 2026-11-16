using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-sixth pass of beasts (bible §3.2.1): the roles of Regions 2 and 3 (Docs/Mapas100.md 09, 12, 15 and 17) that the continuous world
    /// still played with another beast after <see cref="EarlyOwnBeastRecipes"/>, low-poly from the shared palette. Pivot at the feet (y = 0),
    /// facing +Z. Each keeps its stand-in's numbers (<c>EarlyOwn2BeastsTests</c>). The Águia da Tempestade of the Cume dos Mazans (18) is the
    /// Farnkesh's own (<see cref="FarnkeshBeastRecipes.BuildStormEagle"/>), not a new beast.
    /// <list type="bullet">
    /// <item><b>Raiz Sedenta</b> (09, ~0.9 m): a knot of dimmed vine root that crawls along the karez on its tendrils, a swallowed drop lit
    /// inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Escorpião-Cristal</b> (12, ~0.9 m long): a scorpion of red salt crystal, pincers forward, the sting a crystal point.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Serpente de Sal</b> (12, ~1.6 m long): a salt-white serpent in an S on the crust, a ridge of crystals down its back.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Águia-Preta Apagada</b> (15, ~1.2 m): the black eagle of the painted cliffs, a white V on its back, yellow cere and feet.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Salamandra Fria</b> (17, ~1.1 m long): a fire salamander gone out, black with ash-grey spots and one ember left, cold smoke
    /// rising off its back. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// </list>
    /// </summary>
    public static class EarlyOwn2BeastRecipes
    {
        // ------------------------------------------------------------------
        // Shared
        // ------------------------------------------------------------------

        /// <summary>Static jointed legs drawn into the body mesh (scorpions): hip → knee up and out → tip on the ground.</summary>
        private static void DrawSpreadLegs(EnemyMeshBuilder b, float bodyY, float[] hipZ, float[] splay, float reach, float kneeUp, float radius, EnemySwatch leg, EnemySwatch tip)
        {
            float ground = -bodyY + 0.02f;
            for (int i = 0; i < hipZ.Length; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    float side = s;
                    Quaternion yaw = Quaternion.Euler(0f, splay[i] * side, 0f);
                    Vector3 hip = new Vector3(0.14f * side, -0.02f, hipZ[i]);
                    Vector3 knee = hip + yaw * new Vector3(reach * 0.55f * side, kneeUp, 0f);
                    Vector3 foot = hip + yaw * new Vector3(reach * side, 0f, 0f);
                    foot.y = ground;
                    b.Segment(hip, knee, radius, radius * 0.8f, 4, leg);
                    b.Segment(knee, foot, radius * 0.8f, radius * 0.4f, 4, tip);
                }
            }
        }

        // ------------------------------------------------------------------
        // Raiz Sedenta
        // ------------------------------------------------------------------

        public static void BuildThirstyRoot(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawThirstyRootBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.06f, 0.08f), b =>
            {
                // The drop of the karez it drank, lit inside the knot, seen between the roots.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.2f, 0.16f), EnemySwatch.Crystal, coarse: true);
                b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.08f, 0.1f, 0.08f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawThirstyRootBody(EnemyMeshBuilder b)
        {
            // The vines of the oasis sent their roots down the shafts to the water. When the Root that Drank the Well went dark, one knot of
            // them broke off and goes along the channel on its own, drinking every puddle it finds; where it has passed the mud is dry.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.46f, 0.56f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.16f, -0.04f), new Vector3(0.46f, 0.3f, 0.42f), EnemySwatch.Bark, coarse: true);
            b.Blob(new Vector3(0.12f, 0.06f, 0.2f), new Vector3(0.2f, 0.18f, 0.16f), EnemySwatch.SandstoneDark, coarse: true);

            // The tendrils it walks on, splayed on the karez floor (the lowest point of the model), and the thin ones that sip.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 15f) * Mathf.Deg2Rad;
                Vector3 knee = new Vector3(Mathf.Cos(a) * 0.3f, -0.2f, Mathf.Sin(a) * 0.26f);
                Vector3 tip = new Vector3(Mathf.Cos(a) * 0.46f, -0.4f, Mathf.Sin(a) * 0.4f);
                b.Segment(new Vector3(0f, -0.08f, 0f), knee, 0.07f, 0.05f, 5, EnemySwatch.BarkDark);
                b.Segment(knee, tip, 0.05f, 0.015f, 4, i % 2 == 0 ? EnemySwatch.Bark : EnemySwatch.BarkDark);
            }

            // Dry vine leaves still on the top, curled, and a sucker reaching up for the light of the shaft.
            for (int i = 0; i < 3; i++)
            {
                float a = i * 120f;
                b.Blob(Quaternion.Euler(0f, a, 0f) * new Vector3(0.16f, 0.3f, 0f), new Vector3(0.18f, 0.04f, 0.14f), EnemySwatch.Lichen, Quaternion.Euler(0f, a, 25f), coarse: true);
            }

            b.Segment(new Vector3(-0.06f, 0.2f, 0f), new Vector3(-0.14f, 0.46f, 0.08f), 0.03f, 0.01f, 4, EnemySwatch.Bark);
        }

        // ------------------------------------------------------------------
        // Escorpião-Cristal
        // ------------------------------------------------------------------

        private static readonly float[] CrystalScorpionHipZ = { 0.08f, -0.02f, -0.12f, -0.22f };
        private static readonly float[] CrystalScorpionSplay = { -30f, -8f, 12f, 34f };

        public static void BuildCrystalScorpion(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.28f, 0f), DrawCrystalScorpionBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.3f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.14f, 0.2f), EnemySwatch.WarCape);
                HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.06f, 0.06f, 0.035f, EnemySwatch.WarEye);
                // The pincers, held forward, red crystal grown over the salt.
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.1f * s, -0.02f, 0.04f), new Vector3(0.24f * s, 0f, 0.2f), 0.035f, 0.03f, 4, EnemySwatch.WarCape);
                    b.Blob(new Vector3(0.24f * s, 0f, 0.26f), new Vector3(0.12f, 0.08f, 0.14f), EnemySwatch.GolemSnow, coarse: true);
                    b.Crystal(new Vector3(0.22f * s, 0f, 0.3f), 0.025f, 0.12f, EnemySwatch.WarCape, Quaternion.Euler(80f, 0f, 0f), sides: 4);
                    b.Crystal(new Vector3(0.27f * s, 0f, 0.3f), 0.02f, 0.1f, EnemySwatch.WarCape, Quaternion.Euler(80f, 0f, 0f), sides: 4);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.08f), b =>
            {
                // The little mouthparts under the head, salt-white.
                b.Box(new Vector3(0f, -0.01f, 0.03f), new Vector3(0.08f, 0.02f, 0.06f), EnemySwatch.GolemSnow);
                b.Cylinder(new Vector3(0.025f, -0.01f, 0.06f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Black, rotation: Quaternion.Euler(90f, 0f, 0f));
                b.Cylinder(new Vector3(-0.025f, -0.01f, 0.06f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Black, rotation: Quaternion.Euler(90f, 0f, 0f));
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.3f), b =>
            {
                // The tail arched over the back in five crystal segments; the sting is a clear point with the stolen light in it.
                Vector3[] joints =
                {
                    Vector3.zero, new Vector3(0f, 0.14f, -0.14f), new Vector3(0f, 0.32f, -0.16f), new Vector3(0f, 0.46f, -0.06f), new Vector3(0f, 0.52f, 0.08f),
                };
                for (int i = 0; i < joints.Length - 1; i++)
                {
                    b.Segment(joints[i], joints[i + 1], 0.06f - i * 0.008f, 0.05f - i * 0.008f, 5, i % 2 == 0 ? EnemySwatch.WarCape : EnemySwatch.GolemSnow);
                }

                b.Blob(new Vector3(0f, 0.52f, 0.1f), new Vector3(0.08f, 0.08f, 0.1f), EnemySwatch.Crystal, coarse: true);
                b.Crystal(new Vector3(0f, 0.5f, 0.14f), 0.025f, 0.1f, EnemySwatch.StolenLight, Quaternion.Euler(120f, 0f, 0f), sides: 4);
            });
        }

        private static void DrawCrystalScorpionBody(EnemyMeshBuilder b)
        {
            // On Namrael's salt the red crystal grows on everything that keeps still. These scorpions keep still all day under the crust and
            // the crystal grew on them too; they are slow and very hard, and the plates ring when you hit them.
            b.Blob(Vector3.zero, new Vector3(0.46f, 0.2f, 0.62f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, -0.04f, 0f), new Vector3(0.38f, 0.12f, 0.54f), EnemySwatch.GolemSnow, coarse: true);
            // Crystal growths on the back plates.
            for (int i = 0; i < 4; i++)
            {
                float z = 0.18f - i * 0.14f;
                b.Crystal(new Vector3(i % 2 == 0 ? 0.08f : -0.08f, 0.06f, z), 0.04f, 0.12f - (i % 2) * 0.03f, EnemySwatch.WarCape,
                    Quaternion.Euler(-10f, 0f, i % 2 == 0 ? -18f : 18f), sides: 4);
            }

            DrawSpreadLegs(b, 0.28f, CrystalScorpionHipZ, CrystalScorpionSplay, 0.42f, 0.14f, 0.024f, EnemySwatch.WarCape, EnemySwatch.GolemSnow);
        }

        // ------------------------------------------------------------------
        // Serpente de Sal
        // ------------------------------------------------------------------

        public static void BuildSaltSerpent(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.2f, 0f), DrawSaltSerpentBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.36f, 0.46f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.14f, 0.28f), EnemySwatch.GolemSnow);
                b.Blob(new Vector3(0f, 0.03f, -0.02f), new Vector3(0.16f, 0.08f, 0.22f), EnemySwatch.SandstoneLight, coarse: true);
                // Two crystal brows over pale eyes that kept a little of the sun of the plateau.
                b.Crystal(new Vector3(0.06f, 0.05f, 0.02f), 0.02f, 0.07f, EnemySwatch.WarCape, Quaternion.Euler(-60f, 0f, -20f), sides: 3);
                b.Crystal(new Vector3(-0.06f, 0.05f, 0.02f), 0.02f, 0.07f, EnemySwatch.WarCape, Quaternion.Euler(-60f, 0f, 20f), sides: 3);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.03f, 0.06f, 0.035f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.04f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.14f, 0.03f, 0.18f), EnemySwatch.SandstoneLight);
                b.Cylinder(new Vector3(0.035f, 0.005f, 0.12f), 0.01f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.035f, 0.005f, 0.12f), 0.01f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(-0.16f, -0.1f, -0.42f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.12f, 0f, -0.24f), 0.08f, 0.06f, 6, EnemySwatch.GolemSnow);
                b.Segment(new Vector3(0.12f, 0f, -0.24f), new Vector3(0.04f, 0.02f, -0.44f), 0.06f, 0.015f, 5, EnemySwatch.SandstoneLight);
            });
        }

        private static void DrawSaltSerpentBody(EnemyMeshBuilder b)
        {
            // Under the crust of the salt plain the serpents swim as if it were sand, and they come up where the crust is thin to strike at
            // feet. They are the colour of the salt, so at noon you only see the red crystals down their backs.
            Vector3[] spine =
            {
                new Vector3(-0.16f, -0.1f, -0.42f), new Vector3(0.18f, -0.11f, -0.2f), new Vector3(0.22f, -0.11f, 0.06f), new Vector3(-0.08f, -0.08f, 0.24f),
                new Vector3(-0.02f, 0.1f, 0.38f), new Vector3(0f, 0.32f, 0.44f),
            };
            for (int i = 0; i < spine.Length - 1; i++)
            {
                b.Segment(spine[i], spine[i + 1], 0.09f, 0.09f, 7, i % 2 == 0 ? EnemySwatch.GolemSnow : EnemySwatch.SandstoneLight);
                b.Blob(spine[i], new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.GolemSnow, coarse: true);
                // The ridge of red salt crystals.
                b.Crystal(spine[i] + new Vector3(0f, 0.07f, 0f), 0.03f, 0.1f, EnemySwatch.WarCape, Quaternion.Euler(i % 2 == 0 ? 12f : -12f, 0f, 0f), sides: 4);
            }

            // A fleck of the light it struck, caught in one scale; the crust it pushed up round the coils.
            b.Blob(new Vector3(0.2f, -0.02f, -0.08f), new Vector3(0.05f, 0.04f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            b.Cylinder(new Vector3(0f, -0.2f, -0.04f), 0.4f, 0.34f, 0.03f, 8, EnemySwatch.SandstoneLight);
        }

        // ------------------------------------------------------------------
        // Águia-Preta Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedBlackEagle(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawDimmedBlackEagleBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.32f, 0.32f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.26f, 0.3f), EnemySwatch.Black);
                // The yellow cere and the hooked beak; the eye ringed in yellow, gone dull, with an ember left in it.
                b.Blob(new Vector3(0f, 0.01f, 0.13f), new Vector3(0.1f, 0.08f, 0.08f), EnemySwatch.Gold, coarse: true);
                b.Cylinder(new Vector3(0f, -0.01f, 0.15f), 0.04f, 0.012f, 0.12f, 5, EnemySwatch.IronDark, rotation: Quaternion.Euler(100f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.05f, 0.07f, 0.05f, EnemySwatch.Gold);
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.05f, 0.09f, 0.025f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.12f), b =>
                b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.1f), EnemySwatch.IronDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.22f, 0.12f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Black, EnemySwatch.IronDark, EnemySwatch.Bone, 1.05f), Quaternion.Euler(0f, 0f, -12f), meshKey: "DimmedBlackEagleWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.22f, 0.12f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Black, EnemySwatch.IronDark, EnemySwatch.Bone, 1.05f), Quaternion.Euler(0f, 0f, 12f), meshKey: "DimmedBlackEagleWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.38f), b =>
            {
                RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.IronDark);
                // The white rump just above the tail.
                b.Blob(new Vector3(0f, 0.04f, 0.04f), new Vector3(0.18f, 0.06f, 0.12f), EnemySwatch.Bone, coarse: true);
            }, Quaternion.Euler(10f, 0f, 0f));
        }

        private static void DrawDimmedBlackEagleBody(EnemyMeshBuilder b)
        {
            // The black eagles nest on the ledges above the painted shelters and hunt the hyraxes on the scree. The Borrão ate the colour off the
            // walls and the eagles went dull with it; they still throw their flight feathers like darts at whatever climbs to the paintings.
            b.Blob(Vector3.zero, new Vector3(0.52f, 0.54f, 0.82f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.12f, 0.12f), new Vector3(0.4f, 0.32f, 0.44f), EnemySwatch.IronDark, coarse: true);
            // The white V on its back, where the wings meet.
            b.Box(new Vector3(0.08f, 0.25f, -0.04f), new Vector3(0.06f, 0.03f, 0.3f), EnemySwatch.Bone, Quaternion.Euler(0f, -24f, 0f));
            b.Box(new Vector3(-0.08f, 0.25f, -0.04f), new Vector3(0.06f, 0.03f, 0.3f), EnemySwatch.Bone, Quaternion.Euler(0f, 24f, 0f));
            // A speck of the colour it carried off the wall, under the breast.
            b.Blob(new Vector3(0f, -0.2f, 0.2f), new Vector3(0.05f, 0.04f, 0.05f), EnemySwatch.StolenLight, coarse: true);

            // Feathered legs and the yellow feet with black talons (the lowest point of the model).
            float ground = -0.6f + 0.02f;
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.1f * side, -0.2f, 0f), new Vector3(0.12f * side, -0.42f, 0.03f), 0.06f, 0.045f, 5, EnemySwatch.Black);
                b.Segment(new Vector3(0.12f * side, -0.42f, 0.03f), new Vector3(0.12f * side, ground, 0.05f), 0.03f, 0.025f, 4, EnemySwatch.Gold);
                b.Box(new Vector3(0.12f * side, ground, 0.12f), new Vector3(0.12f, 0.02f, 0.18f), EnemySwatch.Gold);
                b.Cylinder(new Vector3(0.12f * side, ground, 0.2f), 0.012f, 0f, 0.05f, 3, EnemySwatch.Black, rotation: Quaternion.Euler(100f, 0f, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Salamandra Fria
        // ------------------------------------------------------------------

        public static void BuildColdSalamander(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawColdSalamanderBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.06f, 0.46f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.16f, 0.3f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, 0.04f, -0.04f), new Vector3(0.22f, 0.08f, 0.18f), EnemySwatch.GolemStoneDark, coarse: true);
                // Big pale eyes: the fire went out but it still looks for it.
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.06f, 0.06f, 0.05f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.06f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.22f, 0.03f, 0.18f), EnemySwatch.Shadow));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.4f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.06f, -0.04f, -0.3f), 0.09f, 0.06f, 6, EnemySwatch.Black);
                b.Segment(new Vector3(0.06f, -0.04f, -0.3f), new Vector3(-0.04f, -0.08f, -0.56f), 0.06f, 0.015f, 5, EnemySwatch.Black);
                b.Blob(new Vector3(0.05f, 0.02f, -0.28f), new Vector3(0.07f, 0.03f, 0.08f), EnemySwatch.Shroud, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.24f, 0.26f, 0.26f, -0.26f, 0.05f, EnemySwatch.Black, EnemySwatch.Shadow, "ColdSalamander");
        }

        private static void DrawColdSalamanderBody(EnemyMeshBuilder b)
        {
            // The dwarves kept salamanders in the Grande Forja: they lived in the coals and the fire never went out while they did. When the
            // Bellows went dark the salamanders went cold, their yellow spots turned to ash, and now they run through the halls looking for a
            // fire to lie in. One spot each still glows.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.24f, 0.86f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.34f, 0.14f, 0.7f), EnemySwatch.Shadow, coarse: true);
            // The ash-grey spots where the yellow was, and the one ember left.
            float[] spotX = { 0.1f, -0.12f, 0.08f, -0.06f, 0.12f };
            for (int i = 0; i < spotX.Length; i++)
            {
                b.Blob(new Vector3(spotX[i], 0.1f, 0.3f - i * 0.15f), new Vector3(0.09f, 0.04f, 0.1f), EnemySwatch.Shroud, coarse: true);
            }

            b.Blob(new Vector3(-0.1f, 0.1f, 0.02f), new Vector3(0.07f, 0.04f, 0.08f), EnemySwatch.Ember, coarse: true);
            // Cold smoke rising off its back, the last of the forge fire leaving it.
            for (int i = 0; i < 3; i++)
            {
                Vector3 from = new Vector3(i == 1 ? 0.04f : -0.04f, 0.1f, 0.18f - i * 0.2f);
                b.Segment(from, from + new Vector3(i == 1 ? 0.06f : -0.04f, 0.3f + i * 0.04f, -0.06f), 0.04f, 0.015f, 4, EnemySwatch.Shroud);
            }
        }
    }
}
