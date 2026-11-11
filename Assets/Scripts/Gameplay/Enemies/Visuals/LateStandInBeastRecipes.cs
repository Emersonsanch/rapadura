using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twelve most visible roles of Regions 12–16 that the continuous world still played with stand-ins (bible §3.2.1, eleventh pass), low-poly
    /// from the shared palette like <see cref="CeuBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Salamandra Devoradora</b> (70, ~1.5 m long): low and dark, an ember stripe down the back. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Bomba Vulcânica</b> (67, ~1.3 m): a cracked ball of slag trailing smoke, the core showing. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Ganso Ladrão</b> (72, ~1.5 m): a big white goose with a stolen ring on its neck. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Filho de Apep</b> (76, ~0.85 m tall): a small violet serpent, head up. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>…<c>Tail_3</c>.</item>
    /// <item><b>Areia Movediça</b> (76, ~1 m): a slow heap of sand with something gold sunk in it. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Homem-Escorpião</b> (77, ~1.9 m): the girtablullu as a story gives him, bow in hand, a hand above the ground. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Lince do Cedro</b> (80, ~1 m): tufted ears and resin in the fur. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Abelha-Resina</b> (80, ~1 m): a fat bee with a lit drop of resin for an abdomen. <c>Body</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Stinger</c>.</item>
    /// <item><b>Morcego-Lâmina</b> (86, ~1.1 m): grey iron bat, the wing fingers are blades. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>.</item>
    /// <item><b>Enguia Grande</b> (87, ~1 m tall): a thick dark eel with barbels. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>…<c>Tail_3</c>.</item>
    /// <item><b>Touro de Cinza</b> (85, ~1.9 m): a bull of packed ash with embers in the seams. <c>Body</c> → <c>Head</c>, <c>Tail</c>; four heavy legs.</item>
    /// <item><b>Corda de Fumaça</b> (97, ~1.5 m): a coil of grey smoke holding the shape of a rope. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c> (loose ends), <c>Tail</c>.</item>
    /// </list>
    /// </summary>
    public static class LateStandInBeastRecipes
    {
        // ------------------------------------------------------------------
        // Shared: the serpents' tail
        // ------------------------------------------------------------------

        private static void AddSerpentTail(ProceduralEnemyBlueprint bp, int body, Vector3 origin, Vector3[] steps, float[] radii, EnemySwatch scale, EnemySwatch mark, EnemySwatch tip)
        {
            int parent = bp.Mesh(ProceduralEnemyParts.Tail, body, origin, b => DrawSerpentSegment(b, 0, steps, radii, scale, mark, tip));
            for (int i = 1; i < OverworldBeastRecipes.SnakeTailSegments; i++)
            {
                int index = i;
                parent = bp.Mesh(ProceduralEnemyParts.TailSegmentPrefix + i, parent, steps[i - 1], b => DrawSerpentSegment(b, index, steps, radii, scale, mark, tip));
            }
        }

        private static void DrawSerpentSegment(EnemyMeshBuilder b, int index, Vector3[] steps, float[] radii, EnemySwatch scale, EnemySwatch mark, EnemySwatch tip)
        {
            Vector3 end = steps[index];
            float r0 = radii[index];
            float r1 = radii[index + 1];
            b.Segment(Vector3.zero, end, r0, r1, 6, scale);
            b.Blob(Vector3.zero, Vector3.one * (r0 * 2f), scale, coarse: true);
            b.Box(end * 0.5f + new Vector3(0f, (r0 + r1) * 0.45f, 0f), new Vector3(0.06f, 0.02f, 0.06f), mark, Quaternion.Euler(0f, 45f, 0f));
            if (index == OverworldBeastRecipes.SnakeTailSegments - 1)
            {
                b.Segment(end, end + end.normalized * 0.1f, r1, 0.004f, 4, mark);
                b.Blob(end + end.normalized * 0.07f, new Vector3(0.05f, 0.05f, 0.05f), tip, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Salamandra Devoradora
        // ------------------------------------------------------------------

        public static void BuildDevouringSalamander(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawDevouringSalamanderBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.06f, 0.56f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.44f, 0.28f, 0.48f), EnemySwatch.CopperDark);
                b.Box(new Vector3(0f, 0.1f, 0.02f), new Vector3(0.05f, 0.04f, 0.28f), EnemySwatch.Ember);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.08f, 0.14f, 0.07f, EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.09f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.12f), new Vector3(0.26f, 0.05f, 0.28f), EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.02f, 0.12f), new Vector3(0.18f, 0.015f, 0.22f), EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.48f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.34f), 0.13f, 0.08f, 6, EnemySwatch.CopperDark);
                b.Segment(new Vector3(0f, -0.04f, -0.34f), new Vector3(0f, -0.1f, -0.66f), 0.08f, 0.02f, 5, EnemySwatch.Black);
                b.Blob(new Vector3(0f, -0.1f, -0.68f), new Vector3(0.08f, 0.08f, 0.1f), EnemySwatch.Ember, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.34f, 0.26f, 0.32f, -0.32f, 0.07f, EnemySwatch.CopperDark, EnemySwatch.Black, "DevouringSalamander");
        }

        private static void DrawDevouringSalamanderBody(EnemyMeshBuilder b)
        {
            // It eats the fire and is never full. The stripe down its back is the last thing it swallowed, still going down.
            b.Blob(Vector3.zero, new Vector3(0.7f, 0.42f, 1.04f), EnemySwatch.CopperDark);
            b.Blob(new Vector3(0f, -0.08f, 0.02f), new Vector3(0.5f, 0.24f, 0.82f), EnemySwatch.Black, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.2f, 0.34f - i * 0.22f), new Vector3(0.1f, 0.04f, 0.16f), EnemySwatch.Ember);
            }
        }

        // ------------------------------------------------------------------
        // Bomba Vulcânica
        // ------------------------------------------------------------------

        public static void BuildVolcanicBomb(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.64f, 0f), DrawVolcanicBombBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.36f), b =>
            {
                // The lit heart through the biggest crack: the part of the mountain that has not cooled down yet.
                b.Blob(Vector3.zero, new Vector3(0.34f, 0.34f, 0.2f), EnemySwatch.Ember);
                b.Blob(new Vector3(0f, 0f, 0.04f), new Vector3(0.18f, 0.18f, 0.12f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawVolcanicBombBody(EnemyMeshBuilder b)
        {
            // A rock Echeyde threw up and never caught again; it has kept on falling sideways ever since, and it spits what it still has inside.
            b.Blob(Vector3.zero, new Vector3(0.92f, 0.88f, 0.9f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.2f, -0.06f), new Vector3(0.66f, 0.44f, 0.66f), EnemySwatch.Black, coarse: true);
            b.Box(new Vector3(-0.26f, 0.2f, 0.32f), new Vector3(0.05f, 0.34f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 24f));
            b.Box(new Vector3(0.28f, -0.1f, 0.3f), new Vector3(0.05f, 0.3f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -18f));
            b.Box(new Vector3(0.34f, 0.2f, -0.2f), new Vector3(0.04f, 0.26f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(14f, 0f, 30f));
            // The trail it drags: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.34f, -0.02f), new Vector3(0f, -0.62f, -0.08f), 0.1f, 0.03f, 5, EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, -0.62f, -0.08f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Ganso Ladrão
        // ------------------------------------------------------------------

        public static void BuildThiefGoose(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.68f, 0f), DrawThiefGooseBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.66f, 0.5f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.22f, 0.28f), EnemySwatch.OwlFace);
                b.Cylinder(new Vector3(0f, 0f, 0.12f), 0.06f, 0.02f, 0.18f, 5, EnemySwatch.Copper, rotation: Quaternion.Euler(84f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.05f, 0.08f, 0.05f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.005f, 0.08f), new Vector3(0.07f, 0.02f, 0.16f), EnemySwatch.CopperDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.22f, 0.14f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.OwlFace, EnemySwatch.OwlBuff, EnemySwatch.Gold, 0.85f), Quaternion.Euler(0f, 0f, -14f), meshKey: "ThiefGooseWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.22f, 0.14f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.OwlFace, EnemySwatch.OwlBuff, EnemySwatch.Gold, 0.85f), Quaternion.Euler(0f, 0f, 14f), meshKey: "ThiefGooseWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.36f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.OwlFace, EnemySwatch.OwlBuff), Quaternion.Euler(18f, 0f, 0f));
        }

        private static void DrawThiefGooseBody(EnemyMeshBuilder b)
        {
            // Fat on other people's bread. The ring round its neck was somebody's, and it knows exactly whose.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.56f, 0.88f), EnemySwatch.OwlFace);
            b.Blob(new Vector3(0f, -0.14f, 0.02f), new Vector3(0.44f, 0.3f, 0.66f), EnemySwatch.Ivory, coarse: true);
            b.Segment(new Vector3(0f, 0.16f, 0.3f), new Vector3(0f, 0.66f, 0.46f), 0.1f, 0.06f, 6, EnemySwatch.OwlFace);
            b.Cylinder(new Vector3(0f, 0.36f, 0.37f), 0.09f, 0.09f, 0.03f, 8, EnemySwatch.StolenLight);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.11f * side, -0.22f, 0f), new Vector3(0.13f * side, -0.66f, 0.04f), 0.035f, 0.022f, 5, EnemySwatch.Copper);
                b.Box(new Vector3(0.13f * side, -0.66f, 0.1f), new Vector3(0.12f, 0.02f, 0.18f), EnemySwatch.CopperDark);
            }
        }

        // ------------------------------------------------------------------
        // Filho de Apep
        // ------------------------------------------------------------------

        private static readonly Vector3[] ChildOfApepSteps =
        {
            new Vector3(0.3f, 0f, -0.2f), new Vector3(0.06f, 0f, -0.3f), new Vector3(-0.46f, 0f, -0.14f), new Vector3(-0.16f, 0f, -0.26f),
        };

        private static readonly float[] ChildOfApepRadii = { 0.13f, 0.11f, 0.09f, 0.06f, 0.016f };

        public static void BuildChildOfApep(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.12f, 0.2f), DrawChildOfApepNeck);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.62f, 0.14f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.06f), new Vector3(0.2f, 0.11f, 0.26f), EnemySwatch.BatFurDark);
                b.Box(new Vector3(0f, 0.08f, 0.04f), new Vector3(0.04f, 0.02f, 0.16f), EnemySwatch.Sorcery);
                HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.05f, 0.12f, 0.04f, EnemySwatch.Sorcery);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.02f, 0.12f), b =>
            {
                b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.12f, 0.02f, 0.14f), EnemySwatch.VampWing);
                b.Cylinder(new Vector3(0f, 0.005f, 0.12f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            AddSerpentTail(bp, body, new Vector3(0f, 0f, -0.06f), ChildOfApepSteps, ChildOfApepRadii, EnemySwatch.BatFur, EnemySwatch.Sorcery, EnemySwatch.Sorcery);
        }

        private static void DrawChildOfApepNeck(EnemyMeshBuilder b)
        {
            // She drinks the river, and the small ones she drops on the way go about thinking that they are her.
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.26f, 0.22f, 0.28f), EnemySwatch.BatFur);
            b.Segment(new Vector3(0f, 0.02f, 0f), new Vector3(0f, 0.34f, 0.06f), 0.1f, 0.08f, 6, EnemySwatch.BatFur);
            b.Segment(new Vector3(0f, 0.34f, 0.06f), new Vector3(0f, 0.62f, 0.14f), 0.08f, 0.055f, 6, EnemySwatch.BatFur);
            b.Blob(new Vector3(0f, 0.26f, 0.13f), new Vector3(0.11f, 0.28f, 0.05f), EnemySwatch.VampWing, coarse: true);
        }

        // ------------------------------------------------------------------
        // Areia Movediça
        // ------------------------------------------------------------------

        public static void BuildQuicksand(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.58f, 0f), DrawQuicksandBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.16f, 0.46f), b =>
            {
                // Somebody's lamp, nearly gone under, still lit, still very cross about it.
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.28f, 0.24f), EnemySwatch.StolenLight);
                b.Cylinder(new Vector3(0f, 0.11f, 0f), 0.08f, 0.11f, 0.07f, 6, EnemySwatch.CopperDark, bottomCap: true);
            });
        }

        private static void DrawQuicksandBody(EnemyMeshBuilder b)
        {
            // Sand that stopped being ground and started wanting things. It does not walk, it arrives.
            b.Blob(Vector3.zero, new Vector3(1.5f, 1f, 1.4f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, 0.24f, -0.04f), new Vector3(1.04f, 0.54f, 1f), EnemySwatch.SandstoneDark, coarse: true);
            b.Blob(new Vector3(0f, -0.44f, 0f), new Vector3(1.86f, 0.3f, 1.76f), EnemySwatch.SandstoneLight, coarse: true);
            b.Box(new Vector3(-0.46f, 0.3f, 0.26f), new Vector3(0.3f, 0.2f, 0.24f), EnemySwatch.BarkDark, Quaternion.Euler(10f, 24f, -18f));
            b.Box(new Vector3(0.5f, 0.16f, 0.3f), new Vector3(0.2f, 0.14f, 0.22f), EnemySwatch.Bark, Quaternion.Euler(-14f, -26f, 20f));
            HinokuraBeastRecipes.AddEyes(b, 0.28f, 0.3f, 0.6f, 0.09f, EnemySwatch.MoonEye);
        }

        // ------------------------------------------------------------------
        // Homem-Escorpião
        // ------------------------------------------------------------------

        public static void BuildScorpionMan(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.98f, 0f), DrawScorpionManBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.44f, 0.02f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.32f, 0.32f), EnemySwatch.Sandstone);
                b.Cylinder(new Vector3(0f, 0.06f, 0f), 0.17f, 0.04f, 0.26f, 6, EnemySwatch.Copper, bottomCap: true);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.02f, 0.13f, 0.05f, EnemySwatch.SoulFire);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.11f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.02f, 0.05f), new Vector3(0.14f, 0.05f, 0.12f), EnemySwatch.SandstoneDark));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.4f, -0.84f), DrawScorpionManTail);
        }

        private static void DrawScorpionManBody(EnemyMeshBuilder b)
        {
            // The man of the mountain gate in the old tale, only as the tale gives him. The ground stopped knowing him, so he goes a hand above it.
            b.Blob(Vector3.zero, new Vector3(0.52f, 0.64f, 0.36f), EnemySwatch.Sandstone);
            b.Box(new Vector3(0f, -0.12f, 0.16f), new Vector3(0.4f, 0.1f, 0.06f), EnemySwatch.Copper);
            b.Blob(new Vector3(0f, -0.46f, -0.36f), new Vector3(0.64f, 0.4f, 1.02f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.3f, -0.36f), new Vector3(0.42f, 0.2f, 0.8f), EnemySwatch.CopperDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                for (int k = 0; k < 3; k++)
                {
                    float z = -0.1f - k * 0.24f;
                    b.Segment(new Vector3(0.2f * side, -0.54f, z), new Vector3(0.48f * side, -0.62f, z + 0.04f), 0.05f, 0.035f, 4, EnemySwatch.CopperDark);
                    b.Segment(new Vector3(0.48f * side, -0.62f, z + 0.04f), new Vector3(0.56f * side, -0.96f, z + 0.1f), 0.035f, 0.012f, 4, EnemySwatch.Black);
                }

                b.Segment(new Vector3(0.28f * side, 0.2f, 0f), new Vector3(0.42f * side, -0.04f, 0.22f), 0.07f, 0.05f, 5, EnemySwatch.Sandstone);
            }

            // The bow in the left hand, the arrow already on the string, lit like the tablets he guards.
            b.Segment(new Vector3(-0.42f, -0.04f, 0.26f), new Vector3(-0.3f, 0.44f, 0.3f), 0.03f, 0.018f, 4, EnemySwatch.Bark);
            b.Segment(new Vector3(-0.42f, -0.04f, 0.26f), new Vector3(-0.3f, -0.5f, 0.3f), 0.03f, 0.018f, 4, EnemySwatch.Bark);
            b.Segment(new Vector3(-0.3f, 0.44f, 0.3f), new Vector3(-0.3f, -0.5f, 0.3f), 0.008f, 0.008f, 3, EnemySwatch.Ivory);
            b.Segment(new Vector3(-0.3f, -0.03f, 0.3f), new Vector3(0.14f, -0.02f, 0.62f), 0.012f, 0.012f, 3, EnemySwatch.Rune);
        }

        private static void DrawScorpionManTail(EnemyMeshBuilder b)
        {
            b.Segment(Vector3.zero, new Vector3(0f, 0.36f, -0.28f), 0.11f, 0.08f, 6, EnemySwatch.SandstoneDark);
            b.Segment(new Vector3(0f, 0.36f, -0.28f), new Vector3(0f, 0.74f, -0.2f), 0.08f, 0.06f, 5, EnemySwatch.CopperDark);
            b.Segment(new Vector3(0f, 0.74f, -0.2f), new Vector3(0f, 0.86f, 0.06f), 0.06f, 0.02f, 5, EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, 0.86f, 0.08f), new Vector3(0.1f, 0.1f, 0.14f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Lince do Cedro
        // ------------------------------------------------------------------

        public static void BuildCedarLynx(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawCedarLynxBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.52f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.32f, 0.36f), EnemySwatch.Bark);
                b.Blob(new Vector3(0f, -0.08f, 0.1f), new Vector3(0.22f, 0.14f, 0.2f), EnemySwatch.Lichen, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.11f * s, 0.12f, -0.03f), 0.05f, 0.01f, 0.2f, 4, EnemySwatch.BarkDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.04f, 0.13f, 0.055f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.08f), new Vector3(0.16f, 0.04f, 0.16f), EnemySwatch.BarkDark);
                b.Cylinder(new Vector3(0.05f, 0.005f, 0.14f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.05f, 0.005f, 0.14f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.48f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.04f, -0.2f), 0.07f, 0.05f, 5, EnemySwatch.Bark);
                b.Blob(new Vector3(0f, 0.04f, -0.22f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.BarkDark, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.5f, 0.24f, 0.34f, -0.34f, 0.07f, EnemySwatch.Bark, EnemySwatch.BarkDark, "CedarLynx");
        }

        private static void DrawCedarLynxBody(EnemyMeshBuilder b)
        {
            // It rubbed against the angry cedars until the resin set in its coat. Now the cedars think it is one of them.
            b.Blob(Vector3.zero, new Vector3(0.72f, 0.56f, 1.02f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, -0.14f, 0.02f), new Vector3(0.5f, 0.26f, 0.76f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(0.2f, 0.14f, 0.2f), new Vector3(0.14f, 0.12f, 0.16f), EnemySwatch.StolenLight, coarse: true);
            b.Blob(new Vector3(-0.22f, 0.1f, -0.16f), new Vector3(0.12f, 0.1f, 0.14f), EnemySwatch.Gold, coarse: true);
            b.Blob(new Vector3(0.14f, 0.12f, -0.3f), new Vector3(0.1f, 0.09f, 0.12f), EnemySwatch.Gold, coarse: true);
        }

        // ------------------------------------------------------------------
        // Abelha-Resina
        // ------------------------------------------------------------------

        public static void BuildResinBee(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawResinBeeBody);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.1f * side, 0.14f, 0.02f),
                    b => DrawResinBeeWing(b, side), Quaternion.Euler(0f, 0f, 16f * side), meshKey: s < 0 ? "ResinBeeWing_L" : "ResinBeeWing_R");
            }

            bp.Mesh(ProceduralEnemyParts.Stinger, body, new Vector3(0f, -0.06f, -0.5f), b =>
            {
                b.Cylinder(Vector3.zero, 0.05f, 0f, 0.18f, 5, EnemySwatch.Black, rotation: Quaternion.Euler(-110f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.06f, -0.15f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawResinBeeBody(EnemyMeshBuilder b)
        {
            // The golden bee that found the cedars bleeding and carried it home by the ton. Only one of the two things it is carrying is honey.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.26f, 0.28f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.02f, -0.3f), new Vector3(0.36f, 0.34f, 0.52f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, -0.02f, -0.34f), new Vector3(0.24f, 0.22f, 0.3f), EnemySwatch.StolenLight, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(0f, -0.02f, -0.18f - i * 0.12f), new Vector3(0.37f - i * 0.03f, 0.35f - i * 0.03f, 0.05f), EnemySwatch.BarkDark, coarse: true);
            }

            b.Blob(new Vector3(0f, 0.02f, 0.2f), new Vector3(0.2f, 0.2f, 0.16f), EnemySwatch.BarkDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, 0.04f, 0.22f), new Vector3(0.08f, 0.11f, 0.08f), EnemySwatch.MoonEye, coarse: true);
                b.Segment(new Vector3(0.03f * s, 0.1f, 0.24f), new Vector3(0.1f * s, 0.24f, 0.36f), 0.012f, 0.008f, 3, EnemySwatch.BarkDark);
                // Three pairs of legs hanging under the hover, sticky to the last joint: the lowest point of the model.
                for (int k = 0; k < 3; k++)
                {
                    float z = 0.06f - k * 0.08f;
                    b.Segment(new Vector3(0.07f * s, -0.1f, z), new Vector3(0.12f * s, -0.34f, z - 0.02f), 0.02f, 0.016f, 4, EnemySwatch.BarkDark);
                    b.Segment(new Vector3(0.12f * s, -0.34f, z - 0.02f), new Vector3(0.1f * s, -0.54f, z - 0.05f), 0.016f, 0.008f, 3, EnemySwatch.Gold);
                }
            }
        }

        private static void DrawResinBeeWing(EnemyMeshBuilder b, float side)
        {
            b.Box(new Vector3(0.26f * side, 0.02f, -0.02f), new Vector3(0.52f, 0.015f, 0.24f), EnemySwatch.Ivory, Quaternion.Euler(0f, 18f * side, 0f));
            b.Box(new Vector3(0.18f * side, 0.01f, -0.16f), new Vector3(0.34f, 0.012f, 0.14f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 34f * side, 0f));
        }

        // ------------------------------------------------------------------
        // Morcego-Lâmina
        // ------------------------------------------------------------------

        public static void BuildBladeBat(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawBladeBatBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.22f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.26f, 0.26f), EnemySwatch.Iron);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.09f * s, 0.1f, -0.02f), 0.05f, 0f, 0.2f, 4, EnemySwatch.Steel);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.03f, 0.1f, 0.05f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.09f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.13f, 0.03f, 0.1f), EnemySwatch.IronDark);
                b.Cylinder(new Vector3(0f, 0.005f, 0.09f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.16f * side, 0.08f, 0f),
                    b => DrawBladeBatWing(b, side), Quaternion.Euler(0f, 0f, 14f * side), meshKey: s < 0 ? "BladeBatWing_L" : "BladeBatWing_R");
            }
        }

        private static void DrawBladeBatBody(EnemyMeshBuilder b)
        {
            // It nested in the Wind's own shaft until the wind sharpened it. It cannot land any more without cutting what it lands on.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.44f, 0.5f), EnemySwatch.Iron);
            b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.3f, 0.28f, 0.36f), EnemySwatch.IronDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.07f * side, -0.18f, -0.02f), new Vector3(0.09f * side, -0.58f, -0.06f), 0.025f, 0.012f, 4, EnemySwatch.IronDark);
                b.Cylinder(new Vector3(0.09f * side, -0.6f, -0.06f), 0.03f, 0f, 0.06f, 4, EnemySwatch.Steel, rotation: Quaternion.Euler(180f, 0f, 0f));
            }
        }

        private static void DrawBladeBatWing(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.3f * side, 0.04f, 0f), 0.04f, 0.025f, 4, EnemySwatch.IronDark);
            for (int k = 0; k < 3; k++)
            {
                float spread = k * 18f - 18f;
                Quaternion angle = Quaternion.Euler(0f, spread * side, -10f * side);
                b.Box(new Vector3(0.52f * side, 0.04f, spread * -0.012f), new Vector3(0.5f, 0.012f, 0.1f - k * 0.015f), EnemySwatch.Steel, angle);
            }

            b.Blob(new Vector3(0.74f * side, 0.04f, -0.02f), new Vector3(0.05f, 0.05f, 0.06f), EnemySwatch.Ember, coarse: true);
        }

        // ------------------------------------------------------------------
        // Enguia Grande
        // ------------------------------------------------------------------

        private static readonly Vector3[] GreatEelSteps =
        {
            new Vector3(0.34f, 0f, -0.22f), new Vector3(0.08f, 0f, -0.34f), new Vector3(-0.52f, 0f, -0.16f), new Vector3(-0.18f, 0f, -0.3f),
        };

        private static readonly float[] GreatEelRadii = { 0.16f, 0.13f, 0.11f, 0.08f, 0.018f };

        public static void BuildGreatEel(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.15f, 0.2f), DrawGreatEelNeck);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.66f, 0.16f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.08f), new Vector3(0.26f, 0.15f, 0.34f), EnemySwatch.BatFurDark);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.08f * s, -0.02f, 0.18f), new Vector3(0.18f * s, -0.14f, 0.26f), 0.012f, 0.005f, 3, EnemySwatch.IvoryDark);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.07f, 0.12f, 0.045f, EnemySwatch.SoulFire);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.03f, 0.16f), b =>
            {
                b.Box(new Vector3(0f, -0.005f, 0.08f), new Vector3(0.14f, 0.025f, 0.2f), EnemySwatch.Ivory);
                b.Cylinder(new Vector3(0f, 0.005f, 0.15f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            AddSerpentTail(bp, body, new Vector3(0f, 0f, -0.08f), GreatEelSteps, GreatEelRadii, EnemySwatch.BatFurDark, EnemySwatch.CaveSkinDark, EnemySwatch.Rune);
        }

        private static void DrawGreatEelNeck(EnemyMeshBuilder b)
        {
            // The water goes up and down by the sluices and this one stopped checking. It lies in the air of the flooded halls as if nothing had changed.
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.32f, 0.28f, 0.34f), EnemySwatch.BatFurDark);
            b.Segment(new Vector3(0f, 0.02f, 0f), new Vector3(0f, 0.36f, 0.08f), 0.12f, 0.1f, 6, EnemySwatch.BatFurDark);
            b.Segment(new Vector3(0f, 0.36f, 0.08f), new Vector3(0f, 0.66f, 0.16f), 0.1f, 0.075f, 6, EnemySwatch.BatFurDark);
            b.Blob(new Vector3(0f, 0.3f, 0.16f), new Vector3(0.14f, 0.34f, 0.05f), EnemySwatch.Ivory, coarse: true);
        }

        // ------------------------------------------------------------------
        // Touro de Cinza
        // ------------------------------------------------------------------

        public static void BuildAshBull(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.02f, 0f), DrawAshBullBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.92f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.58f, 0.52f, 0.6f), EnemySwatch.GolemStone);
                b.Blob(new Vector3(0f, -0.14f, 0.2f), new Vector3(0.36f, 0.24f, 0.24f), EnemySwatch.GolemStoneDark, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    float side = s;
                    b.Segment(new Vector3(0.24f * side, 0.18f, -0.02f), new Vector3(0.46f * side, 0.3f, 0.08f), 0.06f, 0.04f, 5, EnemySwatch.Bone);
                    b.Segment(new Vector3(0.46f * side, 0.3f, 0.08f), new Vector3(0.5f * side, 0.5f, 0.2f), 0.04f, 0.005f, 4, EnemySwatch.Ember);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.17f, 0.06f, 0.24f, 0.07f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.84f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.5f, -0.1f), 0.05f, 0.03f, 5, EnemySwatch.GolemStoneDark);
                b.Blob(new Vector3(0f, -0.56f, -0.1f), new Vector3(0.14f, 0.2f, 0.14f), EnemySwatch.Ember, coarse: true);
            });
            HinokuraBeastRecipes.AddHeavyLegs(bp, 0.78f, 0.42f, 0.52f, -0.52f, 0.16f, EnemySwatch.GolemStoneDark, EnemySwatch.Black, "AshBull");
        }

        private static void DrawAshBullBody(EnemyMeshBuilder b)
        {
            // A bull that stood too near the Fire and burned to the bone without noticing. Everything in it is ash except for the temper.
            b.Blob(Vector3.zero, new Vector3(1.16f, 0.98f, 1.68f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.3f, 0.3f), new Vector3(0.88f, 0.6f, 0.86f), EnemySwatch.GolemStone, coarse: true);
            b.Box(new Vector3(-0.42f, 0.14f, 0.1f), new Vector3(0.04f, 0.42f, 0.5f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 8f));
            b.Box(new Vector3(0.42f, -0.02f, -0.2f), new Vector3(0.04f, 0.36f, 0.42f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -8f));
            b.Box(new Vector3(0f, 0.46f, -0.3f), new Vector3(0.5f, 0.04f, 0.34f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 18f, 0f));
        }

        // ------------------------------------------------------------------
        // Corda de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeRope(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawSmokeRopeBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.6f, 0.04f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0f), new Vector3(0.3f, 0.26f, 0.3f), EnemySwatch.Shroud);
                b.Cylinder(new Vector3(0f, -0.1f, 0f), 0.13f, 0.1f, 0.14f, 6, EnemySwatch.Shadow, bottomCap: true);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.06f, 0.13f, 0.05f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.13f), b =>
                b.Box(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.15f, 0.04f, 0.12f), EnemySwatch.Shadow));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.3f, 0.18f, 0f), b => DrawSmokeRopeEnd(b, -1f), Quaternion.Euler(0f, 0f, -26f), meshKey: "SmokeRopeEnd_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.3f, 0.18f, 0f), b => DrawSmokeRopeEnd(b, 1f), Quaternion.Euler(0f, 0f, 26f), meshKey: "SmokeRopeEnd_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.26f, -0.4f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.2f, -0.44f), 0.07f, 0.03f, 5, EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, -0.22f, -0.5f), new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawSmokeRopeEnd(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.28f * side, 0.1f, 0.02f), 0.055f, 0.04f, 5, EnemySwatch.Shroud);
            b.Segment(new Vector3(0.28f * side, 0.1f, 0.02f), new Vector3(0.52f * side, -0.08f, 0.06f), 0.04f, 0.015f, 5, EnemySwatch.Shadow);
            b.Blob(new Vector3(0.54f * side, -0.1f, 0.06f), new Vector3(0.09f, 0.09f, 0.09f), EnemySwatch.Ember, coarse: true);
        }

        private static void DrawSmokeRopeBody(EnemyMeshBuilder b)
        {
            // The rope on the Sun's boat was burned years ago. The smoke of it did not hear, and goes on tying knots.
            for (int i = 0; i < 5; i++)
            {
                float y = -0.66f + i * 0.28f;
                float width = 1f - i * 0.12f;
                b.Blob(new Vector3(0f, y, 0f), new Vector3(width, 0.2f, width), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Shadow);
            }

            b.Segment(new Vector3(0f, 0.46f, 0f), new Vector3(0f, 0.6f, 0.02f), 0.09f, 0.07f, 6, EnemySwatch.Shroud);
            b.Box(new Vector3(0f, -0.72f, 0f), new Vector3(0.5f, 0.03f, 0.5f), EnemySwatch.Shadow, Quaternion.Euler(0f, 30f, 0f));
        }
    }
}
