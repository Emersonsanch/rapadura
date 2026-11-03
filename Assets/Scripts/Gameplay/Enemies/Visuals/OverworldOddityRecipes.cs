using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The non-animal stand-in replacements of the continuous worlds (Docs/Mapas100.md 11, 13, 14, 18, 19), low-poly from the
    /// shared palette. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Orc Coroado de Āz</b> (Areias, Ermos do Tambor Rubro, ~2.1 m): a big grey-green orc with tusks, leather vest
    /// and a log club, wearing the tin crown Āz pushed on his head; his eyes glow red with her greed (never the orc people:
    /// beaten, he drops the crown and walks home). <c>Body</c> → <c>Head</c>, <c>Arm_L</c>, <c>Arm_R</c> → <c>Weapon</c>;
    /// <c>Leg_L</c>/<c>Leg_R</c> → <c>Shin_</c> → <c>Foot_</c>.</item>
    /// <item><b>Pedra que Rola</b> (Kaggen, ~1 m): a ushabti boulder that rolls at the cousins, lichen on top, molten cracks
    /// and a band of runes. <c>Body</c> (the rig rolls it while it travels).</item>
    /// <item><b>Neblina do Vento Ruim</b> (Kaggen, ~0.8 m): a knot of cold fog with two pale eyes, puffs circling it and a
    /// wisp trailing to the ground. <c>Body</c> → <c>Swarm</c> → <c>Puff_0..5</c>, <c>Body</c> → <c>Wisp</c>.</item>
    /// <item><b>Lodo de Gruta</b> (Limiar, ~0.75 m, 1.1 m across): a dome of dark cave mud with moss on top, three pale eyes and
    /// the crystals it swallowed poking out. <c>Body</c> (squashes and stretches) → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class OverworldOddityRecipes
    {
        public const int MistPuffs = 6;

        // ------------------------------------------------------------------
        // Orc Coroado de Āz
        // ------------------------------------------------------------------

        public static void BuildCrownedOrc(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.0f, 0f), DrawOrcBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.62f, 0.08f), DrawOrcHead);

            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                int arm = bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body,
                    new Vector3(0.42f * side, 0.36f, 0f), b => DrawOrcArm(b, side), Quaternion.Euler(-6f, 0f, 0f));
                if (right)
                {
                    bp.Mesh(ProceduralEnemyParts.Weapon, arm, new Vector3(0.06f, -0.62f, 0.06f), DrawOrcClub);
                }

                int leg = bp.Mesh(right ? ProceduralEnemyParts.LegRight : ProceduralEnemyParts.LegLeft, -1,
                    new Vector3(0.2f * side, 0.62f, 0f), b => b.Segment(new Vector3(0f, -0.3f, 0.05f), new Vector3(0f, 0.04f, 0f), 0.1f, 0.13f, 6, EnemySwatch.Leather));
                int shin = bp.Mesh(right ? ProceduralEnemyParts.ShinRight : ProceduralEnemyParts.ShinLeft, leg,
                    new Vector3(0f, -0.3f, 0.05f), b =>
                    {
                        b.Segment(new Vector3(0f, -0.28f, -0.03f), Vector3.zero, 0.075f, 0.095f, 6, EnemySwatch.Lichen);
                        b.Box(new Vector3(0f, -0.08f, 0.02f), new Vector3(0.17f, 0.1f, 0.17f), EnemySwatch.Leather);
                    });
                bp.Mesh(right ? ProceduralEnemyParts.FootRight : ProceduralEnemyParts.FootLeft, shin,
                    new Vector3(0f, -0.28f, -0.03f), b => b.Blob(new Vector3(0f, -0.02f, 0.05f), new Vector3(0.16f, 0.1f, 0.24f), EnemySwatch.BarkDark, coarse: true));
            }
        }

        private static void DrawOrcBody(EnemyMeshBuilder b)
        {
            // Broad hunched torso, leather vest, belt with a gold buckle (Āz likes gold), red loincloth.
            b.Blob(new Vector3(0f, 0.22f, 0f), new Vector3(0.72f, 0.72f, 0.48f), EnemySwatch.Lichen);
            b.Blob(new Vector3(0f, 0.2f, -0.02f), new Vector3(0.7f, 0.56f, 0.46f), EnemySwatch.Leather, coarse: true);
            b.Blob(new Vector3(0f, 0.3f, 0.12f), new Vector3(0.36f, 0.36f, 0.26f), EnemySwatch.Lichen, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.3f * s, 0.44f, 0f), new Vector3(0.28f, 0.2f, 0.3f), EnemySwatch.Leather, coarse: true);
            }

            b.Box(new Vector3(0f, -0.1f, 0f), new Vector3(0.62f, 0.1f, 0.42f), EnemySwatch.BarkDark);
            b.Box(new Vector3(0f, -0.1f, 0.215f), new Vector3(0.1f, 0.08f, 0.02f), EnemySwatch.Gold);
            b.Box(new Vector3(0f, -0.28f, 0.16f), new Vector3(0.3f, 0.3f, 0.04f), EnemySwatch.WarCape, Quaternion.Euler(-6f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.22f, 0f), new Vector3(0.52f, 0.28f, 0.38f), EnemySwatch.Leather, coarse: true);
        }

        private static void DrawOrcHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.34f, 0.32f, 0.32f), EnemySwatch.Lichen);
            // Heavy brow, wide underbite jaw and two tusks pointing up.
            b.Box(new Vector3(0f, 0.17f, 0.13f), new Vector3(0.3f, 0.06f, 0.08f), EnemySwatch.Lichen, Quaternion.Euler(-12f, 0f, 0f));
            b.Blob(new Vector3(0f, 0.0f, 0.08f), new Vector3(0.3f, 0.14f, 0.22f), EnemySwatch.Lichen, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Cylinder(new Vector3(0.08f * s, 0.02f, 0.18f), 0.022f, 0.004f, 0.09f, 4, EnemySwatch.Fang, rotation: Quaternion.Euler(-8f, 0f, 6f * s));
                // Eyes glowing with Āz's greed.
                b.Blob(new Vector3(0.075f * s, 0.13f, 0.15f), new Vector3(0.05f, 0.03f, 0.03f), EnemySwatch.WarEye, coarse: true);
                b.Cylinder(new Vector3(0.16f * s, 0.12f, 0f), 0.04f, 0f, 0.1f, 4, EnemySwatch.Lichen, rotation: Quaternion.Euler(0f, 0f, -80f * s));
            }

            // Top-knot and the tin crown Āz pushed on him (five little points, a fake gem).
            b.Cylinder(new Vector3(0f, 0.25f, -0.06f), 0.05f, 0.02f, 0.14f, 5, EnemySwatch.Shadow, rotation: Quaternion.Euler(-30f, 0f, 0f));
            b.Cylinder(new Vector3(0f, 0.22f, 0f), 0.14f, 0.15f, 0.08f, 8, EnemySwatch.Gold);
            for (int i = 0; i < 5; i++)
            {
                float a = (i / 5f) * Mathf.PI * 2f;
                b.Cylinder(new Vector3(Mathf.Sin(a) * 0.14f, 0.3f, Mathf.Cos(a) * 0.14f), 0.03f, 0f, 0.07f, 4, EnemySwatch.Gold);
            }

            b.Blob(new Vector3(0f, 0.26f, 0.15f), new Vector3(0.04f, 0.04f, 0.02f), EnemySwatch.WarEye, coarse: true);
        }

        private static void DrawOrcArm(EnemyMeshBuilder b, float side)
        {
            b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.22f), EnemySwatch.Lichen, coarse: true);
            b.Segment(Vector3.zero, new Vector3(0.05f * side, -0.32f, 0.02f), 0.1f, 0.085f, 6, EnemySwatch.Lichen);
            b.Segment(new Vector3(0.05f * side, -0.32f, 0.02f), new Vector3(0.06f * side, -0.56f, 0.06f), 0.085f, 0.08f, 6, EnemySwatch.Lichen);
            b.Cylinder(new Vector3(0.055f * side, -0.52f, 0.05f), 0.09f, 0.09f, 0.14f, 6, EnemySwatch.Leather);
            b.Blob(new Vector3(0.06f * side, -0.62f, 0.06f), new Vector3(0.15f, 0.14f, 0.15f), EnemySwatch.Lichen, coarse: true);
        }

        private static void DrawOrcClub(EnemyMeshBuilder b)
        {
            // A knotted log club held low, studded with old iron.
            b.Segment(new Vector3(0f, 0.04f, -0.06f), new Vector3(0f, -0.1f, 0.5f), 0.035f, 0.08f, 6, EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.1f, 0.5f), new Vector3(0.17f, 0.17f, 0.17f), EnemySwatch.Bark, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0.06f * (i - 1), -0.07f + 0.02f * i, 0.3f + 0.08f * i), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.Iron);
            }
        }

        // ------------------------------------------------------------------
        // Pedra que Rola
        // ------------------------------------------------------------------

        /// <summary>Radius of the boulder: the rig turns its roll speed into degrees from it.</summary>
        public const float RollingStoneRadius = 0.5f;

        public static void BuildRollingStone(ProceduralEnemyBlueprint bp)
        {
            bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, RollingStoneRadius + 0.02f, 0f), DrawBoulder);
        }

        private static void DrawBoulder(EnemyMeshBuilder b)
        {
            float d = RollingStoneRadius * 2f;
            b.Rock(Vector3.zero, new Vector3(d, d, d), EnemySwatch.GolemStone, 1, null, EnemySwatch.Lichen);
            b.Rock(new Vector3(0.2f, 0.18f, 0.22f), new Vector3(0.34f, 0.3f, 0.3f), EnemySwatch.GolemStoneDark, 2);
            b.Rock(new Vector3(-0.24f, -0.12f, -0.18f), new Vector3(0.3f, 0.28f, 0.32f), EnemySwatch.GolemStoneDark, 3);
            // The band of ushabti runes round its belly, and the molten cracks of the light trapped inside.
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f;
                Quaternion around = Quaternion.Euler(a, 0f, 0f);
                b.Box(around * new Vector3(0.43f, 0f, 0.2f), new Vector3(0.05f, 0.08f, 0.04f), EnemySwatch.Rune, around);
            }

            b.Box(new Vector3(0f, 0.08f, 0.44f), new Vector3(0.06f, 0.36f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 18f));
            b.Box(new Vector3(-0.1f, -0.12f, 0.42f), new Vector3(0.05f, 0.2f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, -40f));
            b.Box(new Vector3(0f, 0.3f, -0.36f), new Vector3(0.36f, 0.05f, 0.05f), EnemySwatch.GolemCrack, Quaternion.Euler(30f, 0f, 8f));
            // Two narrow eye-slits: it is still a ushabti, curled up.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.1f * s, 0.16f, 0.45f), new Vector3(0.1f, 0.03f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 8f * s));
            }
        }

        // ------------------------------------------------------------------
        // Neblina do Vento Ruim
        // ------------------------------------------------------------------

        public static void BuildIllWindMist(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.55f, 0f), DrawMistCore);
            int swirl = bp.Node(ProceduralEnemyParts.Swarm, body, Vector3.zero);
            for (int i = 0; i < MistPuffs; i++)
            {
                float a = i * Mathf.PI * 2f / MistPuffs;
                float y = i % 2 == 0 ? -0.16f : 0.08f;
                bp.Mesh(ProceduralEnemyParts.PuffPrefix + i, swirl, new Vector3(Mathf.Sin(a) * 0.34f, y, Mathf.Cos(a) * 0.34f), DrawPuff, meshKey: "Puff");
            }

            bp.Mesh(ProceduralEnemyParts.Wisp, body, new Vector3(0f, -0.36f, -0.08f), DrawMistWisp);
        }

        private static void DrawMistCore(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.42f, 0.5f), EnemySwatch.Steel);
            b.Blob(new Vector3(0.08f, 0.1f, -0.04f), new Vector3(0.34f, 0.28f, 0.34f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(-0.1f, -0.06f, 0.06f), new Vector3(0.3f, 0.24f, 0.3f), EnemySwatch.Iron, coarse: true);
            // Two pale eyes and the dark swirl of a mouth that blows the bad wind.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.09f * s, 0.06f, 0.22f), new Vector3(0.06f, 0.04f, 0.03f), EnemySwatch.SoftEye, coarse: true);
            }

            b.Blob(new Vector3(0f, -0.06f, 0.23f), new Vector3(0.08f, 0.06f, 0.03f), EnemySwatch.Shadow, coarse: true);
        }

        private static void DrawPuff(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.2f, 0.26f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(0.04f, -0.03f, 0.02f), new Vector3(0.18f, 0.14f, 0.18f), EnemySwatch.Steel, coarse: true);
        }

        private static void DrawMistWisp(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.04f, 0f), new Vector3(0.24f, 0.26f, 0.24f), EnemySwatch.Steel, coarse: true);
            b.Segment(new Vector3(0f, 0.06f, 0.04f), new Vector3(0f, 0.2f, 0.06f), 0.1f, 0.14f, 6, EnemySwatch.Iron);
        }

        // ------------------------------------------------------------------
        // Lodo de Gruta
        // ------------------------------------------------------------------

        public static void BuildCaveOoze(ProceduralEnemyBlueprint bp)
        {
            // Pivot on the ground so the rig squashes it toward the floor.
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, Vector3.zero, DrawOoze);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.08f, 0.6f, -0.04f), DrawSwallowedLights);
        }

        private static void DrawOoze(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.34f, 0f), new Vector3(1.1f, 0.72f, 1.0f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, 0.52f, -0.06f), new Vector3(0.72f, 0.3f, 0.6f), EnemySwatch.CaveMoss, coarse: true);
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(1.16f, 0.2f, 1.06f), EnemySwatch.Shadow, coarse: true);
            // Drips and lumps round the rim.
            for (int i = 0; i < 6; i++)
            {
                float a = i * Mathf.PI / 3f + 0.4f;
                b.Blob(new Vector3(Mathf.Sin(a) * 0.5f, 0.06f, Mathf.Cos(a) * 0.46f), new Vector3(0.16f, 0.12f, 0.16f), EnemySwatch.CaveSkin, coarse: true);
            }

            // Three pale eyes, one higher than the others.
            b.Blob(new Vector3(-0.13f, 0.46f, 0.42f), new Vector3(0.07f, 0.07f, 0.05f), EnemySwatch.SoftEye, coarse: true);
            b.Blob(new Vector3(0.13f, 0.46f, 0.42f), new Vector3(0.07f, 0.07f, 0.05f), EnemySwatch.SoftEye, coarse: true);
            b.Blob(new Vector3(0.02f, 0.58f, 0.36f), new Vector3(0.05f, 0.05f, 0.04f), EnemySwatch.SoftEye, coarse: true);
        }

        private static void DrawSwallowedLights(EnemyMeshBuilder b)
        {
            // The crystals and the lamp flame it swallowed, still shining through the mud.
            b.Crystal(Vector3.zero, 0.05f, 0.18f, EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, -18f), 5);
            b.Crystal(new Vector3(-0.16f, -0.04f, 0.06f), 0.04f, 0.13f, EnemySwatch.Crystal, Quaternion.Euler(10f, 0f, 24f), 5);
            b.Blob(new Vector3(0.12f, -0.02f, -0.08f), new Vector3(0.08f, 0.08f, 0.08f), EnemySwatch.StolenLight, coarse: true);
        }
    }
}
