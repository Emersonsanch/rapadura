using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The four [distância] commons of the playable maps (Docs/Mapas100.md 07, 08, 13, 19), low-poly from the shared
    /// palette like the other recipes; all hover (Flyer archetype) and shoot through <see cref="EnemyRangedAttack"/>.
    /// Pivot at the bottom (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Abutre de Brasa</b> (Areias, ~0.95 m, 1.8 m across): sooty vulture, bald pink head, bone ruff, wings whose
    /// tips still smoulder like embers. <c>Body</c> → <c>Head</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Lanterna Quebrada dos Artífices</b> (Areias, ~1 m): an old artificers' lantern engine, iron frame, a cracked
    /// pane and a pulsing amber core of trapped light with runes. <c>Body</c> (frame, sways) → <c>Lantern</c> (core).</item>
    /// <item><b>Gavião-de-Penacho</b> (Kaggen, ~0.8 m, 1.5 m across): brown hawk, barred buff belly, a dark crest and
    /// glowing yellow eyes. <c>Body</c> → <c>Head</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Peixe-Lanterna Apagado</b> (Limiar, ~0.8 m, 1 m long): a dimmed anglerfish swimming in the cave air, a big
    /// toothy jaw and the crystal lure on its stalk. <c>Body</c> → <c>Jaw</c>, <c>Lantern</c> (lure), fins
    /// <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// </list>
    /// </summary>
    public static class RangedEnemyRecipes
    {
        // ------------------------------------------------------------------
        // Abutre de Brasa
        // ------------------------------------------------------------------

        public static void BuildEmberVulture(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawVultureBody, Quaternion.Euler(-10f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.12f), DrawVultureHead);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.17f, 0.12f, -0.02f), b => DrawBroadWing(b, -1f, EnemySwatch.BarkDark, EnemySwatch.PeccaryFur, EnemySwatch.Ember),
                Quaternion.Euler(0f, 0f, -20f), meshKey: "VultureWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.17f, 0.12f, -0.02f), b => DrawBroadWing(b, 1f, EnemySwatch.BarkDark, EnemySwatch.PeccaryFur, EnemySwatch.Ember),
                Quaternion.Euler(0f, 0f, 20f), meshKey: "VultureWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.14f, -0.2f), b => DrawFanTail(b, EnemySwatch.BarkDark, EnemySwatch.PeccaryFur), Quaternion.Euler(25f, 0f, 0f));
        }

        private static void DrawVultureBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.5f, 0.5f), EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, -0.04f, 0.08f), new Vector3(0.3f, 0.36f, 0.3f), EnemySwatch.PeccaryFur, coarse: true);
            // The bone-white ruff around the base of the bare neck.
            b.Blob(new Vector3(0f, 0.2f, 0.08f), new Vector3(0.32f, 0.12f, 0.26f), EnemySwatch.Bone, coarse: true);
            // Glowing cinders caught in the back feathers.
            b.Blob(new Vector3(0.1f, 0.12f, -0.2f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.08f, 0.02f, -0.22f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            DrawTalons(b, -0.42f, 0.08f, EnemySwatch.WolfFurDark);
        }

        private static void DrawVultureHead(EnemyMeshBuilder b)
        {
            // Long bare neck, small bald pink head, a heavy hooked beak and ember eyes.
            b.Segment(new Vector3(0f, -0.06f, -0.04f), new Vector3(0f, 0.1f, 0.04f), 0.05f, 0.04f, 5, EnemySwatch.Pink);
            b.Blob(new Vector3(0f, 0.14f, 0.06f), new Vector3(0.15f, 0.13f, 0.17f), EnemySwatch.Pink);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.055f * s, 0.17f, 0.11f), new Vector3(0.035f, 0.035f, 0.03f), EnemySwatch.Ember, coarse: true);
            }

            b.Cylinder(new Vector3(0f, 0.13f, 0.14f), 0.035f, 0.005f, 0.12f, 4, EnemySwatch.Bone, rotation: Quaternion.Euler(105f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Gavião-de-Penacho
        // ------------------------------------------------------------------

        public static void BuildCrestedHawk(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawHawkBody, Quaternion.Euler(-8f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.27f, 0.06f), DrawHawkHead);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.15f, 0.1f, -0.02f), b => DrawBroadWing(b, -1f, EnemySwatch.WolfFur, EnemySwatch.OwlBuff, EnemySwatch.WolfFurDark, 0.85f),
                Quaternion.Euler(0f, 0f, -25f), meshKey: "HawkWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.15f, 0.1f, -0.02f), b => DrawBroadWing(b, 1f, EnemySwatch.WolfFur, EnemySwatch.OwlBuff, EnemySwatch.WolfFurDark, 0.85f),
                Quaternion.Euler(0f, 0f, 25f), meshKey: "HawkWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.14f, -0.17f), b => DrawFanTail(b, EnemySwatch.WolfFur, EnemySwatch.OwlBuff), Quaternion.Euler(30f, 0f, 0f));
        }

        private static void DrawHawkBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.34f, 0.46f, 0.38f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, -0.03f, 0.07f), new Vector3(0.26f, 0.36f, 0.24f), EnemySwatch.OwlBuff);
            // Brown bars across the buff chest.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0f, 0.06f - i * 0.08f, 0.185f), new Vector3(0.16f - i * 0.02f, 0.018f, 0.02f), EnemySwatch.WolfFurDark);
            }

            DrawTalons(b, -0.38f, 0.07f, EnemySwatch.Gold);
        }

        private static void DrawHawkHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.07f, 0f), new Vector3(0.22f, 0.2f, 0.22f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, 0.04f, 0.06f), new Vector3(0.16f, 0.1f, 0.12f), EnemySwatch.OwlBuff, coarse: true);
            // The crest: three dark feathers raked back.
            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0.025f * i, 0.2f, -0.06f), new Vector3(0.025f, 0.12f, 0.02f), EnemySwatch.WolfFurDark, Quaternion.Euler(-40f, 0f, 12f * i));
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.1f, 0.08f), new Vector3(0.045f, 0.045f, 0.035f), EnemySwatch.MoonEye, coarse: true);
                b.Box(new Vector3(0.062f * s, 0.1f, 0.1f), new Vector3(0.015f, 0.018f, 0.008f), EnemySwatch.Black);
            }

            b.Cylinder(new Vector3(0f, 0.07f, 0.1f), 0.028f, 0.004f, 0.08f, 4, EnemySwatch.Gold, rotation: Quaternion.Euler(110f, 0f, 0f));
        }

        // ------------------------------------------------------------------
        // Shared bird parts
        // ------------------------------------------------------------------

        /// <summary>Feathered legs down to hooked talons on the ground (y = <paramref name="ground"/> in body space).</summary>
        private static void DrawTalons(EnemyMeshBuilder b, float ground, float spread, EnemySwatch claw)
        {
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(spread * s, ground + 0.22f, 0.02f), new Vector3(spread * s, ground + 0.04f, 0.04f), 0.035f, 0.02f, 4, EnemySwatch.BarkDark);
                for (int k = -1; k <= 1; k++)
                {
                    b.Cylinder(new Vector3(spread * s + 0.025f * k, ground + 0.03f, 0.05f), 0.013f, 0f, 0.06f, 3, claw, rotation: Quaternion.Euler(110f, 0f, 0f));
                }
            }
        }

        /// <summary>Broad fingered wing in the horizontal plane; <paramref name="side"/> +1 = right; tips in <paramref name="tip"/>.</summary>
        private static void DrawBroadWing(EnemyMeshBuilder b, float side, EnemySwatch top, EnemySwatch under, EnemySwatch tip, float span = 1f)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side * span, 1f, 1f));
            var r0 = new Vector3(0f, 0f, 0.12f);
            var r1 = new Vector3(0f, 0f, -0.16f);
            var mid = new Vector3(0.36f, 0.04f, 0.1f);
            var p4 = new Vector3(0.12f, 0f, -0.26f);
            var p3 = new Vector3(0.3f, 0f, -0.3f);
            var hand = new Vector3(0.56f, 0.02f, 0.02f);
            b.TwoSided(r0, mid, p4, top, under);
            b.TwoSided(r0, p4, r1, top, under);
            b.TwoSided(mid, p3, p4, top, under);
            b.TwoSided(mid, hand, p3, top, under);
            // Splayed primaries ("fingers"), their tips in the accent swatch.
            for (int i = 0; i < 4; i++)
            {
                float z = 0.04f - i * 0.09f;
                b.Box(new Vector3(0.66f, 0.01f, z), new Vector3(0.22f, 0.012f, 0.05f), top, Quaternion.Euler(0f, -12f * i, 0f));
                b.Box(new Vector3(0.78f, 0.012f, z - 0.03f * i), new Vector3(0.07f, 0.014f, 0.05f), tip, Quaternion.Euler(0f, -12f * i, 0f));
            }

            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawFanTail(EnemyMeshBuilder b, EnemySwatch top, EnemySwatch under)
        {
            b.TwoSided(new Vector3(-0.07f, 0f, 0f), new Vector3(0.07f, 0f, 0f), new Vector3(0.12f, 0f, -0.2f), top, under);
            b.TwoSided(new Vector3(-0.07f, 0f, 0f), new Vector3(0.12f, 0f, -0.2f), new Vector3(-0.12f, 0f, -0.2f), top, under);
        }

        // ------------------------------------------------------------------
        // Lanterna Quebrada dos Artífices
        // ------------------------------------------------------------------

        public static void BuildBrokenLantern(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.02f, 0f), DrawLanternFrame);
            bp.Mesh(ProceduralEnemyParts.Lantern, body, new Vector3(0f, 0.47f, 0f), DrawLanternCore);
        }

        private static void DrawLanternFrame(EnemyMeshBuilder b)
        {
            // Base and lid plates, four corner posts, a pointed roof and the carrying ring.
            b.Box(new Vector3(0f, 0.04f, 0f), new Vector3(0.56f, 0.08f, 0.56f), EnemySwatch.IronDark, top: EnemySwatch.Iron);
            b.Cylinder(new Vector3(0f, -0.02f, 0f), 0.1f, 0.18f, 0.06f, 6, EnemySwatch.IronDark, bottomCap: true);
            b.Box(new Vector3(0f, 0.86f, 0f), new Vector3(0.5f, 0.06f, 0.5f), EnemySwatch.IronDark, top: EnemySwatch.Gold);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    b.Cylinder(new Vector3(0.22f * x, 0.08f, 0.22f * z), 0.03f, 0.03f, 0.76f, 4, EnemySwatch.Iron);
                }
            }

            b.Cylinder(new Vector3(0f, 0.89f, 0f), 0.3f, 0f, 0.22f, 4, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(0f, 45f, 0f));
            b.Segment(new Vector3(-0.06f, 1.1f, 0f), new Vector3(0f, 1.17f, 0f), 0.02f, 0.02f, 4, EnemySwatch.Gold);
            b.Segment(new Vector3(0f, 1.17f, 0f), new Vector3(0.06f, 1.1f, 0f), 0.02f, 0.02f, 4, EnemySwatch.Gold);

            // Two whole panes and a broken one (a jagged shard still hanging in the front).
            b.Box(new Vector3(-0.22f, 0.47f, 0f), new Vector3(0.01f, 0.6f, 0.38f), EnemySwatch.SandstoneLight);
            b.Box(new Vector3(0f, 0.47f, -0.22f), new Vector3(0.38f, 0.6f, 0.01f), EnemySwatch.SandstoneLight);
            b.TwoSided(new Vector3(-0.19f, 0.78f, 0.22f), new Vector3(0.1f, 0.78f, 0.22f), new Vector3(-0.12f, 0.46f, 0.22f), EnemySwatch.SandstoneLight);

            // Artificers' runes on the base, still faintly lit.
            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0.12f * i, 0.05f, 0.285f), new Vector3(0.05f, 0.03f, 0.01f), EnemySwatch.Rune);
            }
        }

        private static void DrawLanternCore(EnemyMeshBuilder b)
        {
            // The trapped light: a cracked amber core with a few crystal splinters sticking out.
            b.Blob(Vector3.zero, new Vector3(0.24f, 0.3f, 0.24f), EnemySwatch.StolenLight);
            b.Crystal(new Vector3(0.05f, 0.08f, 0.04f), 0.03f, 0.14f, EnemySwatch.StolenLight, Quaternion.Euler(0f, 0f, -35f), 4);
            b.Crystal(new Vector3(-0.05f, -0.06f, -0.03f), 0.03f, 0.12f, EnemySwatch.StolenLight, Quaternion.Euler(150f, 0f, 20f), 4);
            b.Box(new Vector3(0f, 0f, 0.12f), new Vector3(0.015f, 0.2f, 0.01f), EnemySwatch.Shadow, Quaternion.Euler(0f, 0f, 20f));
        }

        // ------------------------------------------------------------------
        // Peixe-Lanterna Apagado
        // ------------------------------------------------------------------

        public static void BuildLanternFish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawFishBody);
            bp.Mesh(ProceduralEnemyParts.Jaw, body, new Vector3(0f, -0.08f, 0.22f), DrawFishJaw);
            bp.Mesh(ProceduralEnemyParts.Lantern, body, new Vector3(0f, 0.42f, 0.42f), DrawLure);
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, -0.02f, 0.02f), b => DrawFin(b, -1f), Quaternion.Euler(0f, 0f, -20f), meshKey: "FishFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, -0.02f, 0.02f), b => DrawFin(b, 1f), Quaternion.Euler(0f, 0f, 20f), meshKey: "FishFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.3f), DrawFishTail);
        }

        private static void DrawFishBody(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.46f, 0.6f, 0.66f), EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.36f, 0.36f, 0.5f), EnemySwatch.CaveSkin, coarse: true);
            // Upper fangs hanging over the jaw, tiny pale eyes high on the head.
            for (int i = -2; i <= 2; i++)
            {
                b.Cylinder(new Vector3(0.05f * i, -0.04f, 0.3f), 0.016f, 0f, 0.07f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(180f, 0f, 0f));
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.12f * s, 0.12f, 0.24f), new Vector3(0.05f, 0.05f, 0.04f), EnemySwatch.SoftEye, coarse: true);
            }

            // The lure's stalk, bent forward from the forehead.
            b.Segment(new Vector3(0f, 0.26f, 0.12f), new Vector3(0f, 0.42f, 0.28f), 0.022f, 0.016f, 4, EnemySwatch.CaveSkinDark);
            b.Segment(new Vector3(0f, 0.42f, 0.28f), new Vector3(0f, 0.42f, 0.38f), 0.016f, 0.012f, 4, EnemySwatch.CaveSkinDark);
            // Dorsal spine fin.
            b.TwoSided(new Vector3(0f, 0.28f, 0.02f), new Vector3(0f, 0.36f, -0.12f), new Vector3(0f, 0.22f, -0.22f), EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark);
        }

        private static void DrawFishJaw(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, -0.06f, 0.02f), new Vector3(0.36f, 0.12f, 0.2f), EnemySwatch.CaveSkinDark, coarse: true);
            b.Blob(new Vector3(0f, -0.03f, 0.0f), new Vector3(0.28f, 0.05f, 0.14f), EnemySwatch.Shadow, coarse: true);
            for (int i = -2; i <= 2; i++)
            {
                b.Cylinder(new Vector3(0.055f * i, -0.02f, 0.08f), 0.016f, 0f, 0.08f, 3, EnemySwatch.Fang);
            }
        }

        private static void DrawLure(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.Crystal);
            b.Crystal(new Vector3(0f, 0.03f, 0f), 0.025f, 0.08f, EnemySwatch.SoftEye, sides: 4);
        }

        private static void DrawFin(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.TwoSided(new Vector3(0f, 0f, 0.06f), new Vector3(0.2f, 0.02f, -0.02f), new Vector3(0.14f, 0f, -0.14f), EnemySwatch.CaveSkin, EnemySwatch.CaveBelly);
            b.TwoSided(new Vector3(0f, 0f, 0.06f), new Vector3(0.14f, 0f, -0.14f), new Vector3(0f, 0f, -0.08f), EnemySwatch.CaveSkin, EnemySwatch.CaveBelly);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawFishTail(EnemyMeshBuilder b)
        {
            b.Segment(new Vector3(0f, 0f, 0.04f), new Vector3(0f, 0f, -0.14f), 0.12f, 0.05f, 5, EnemySwatch.CaveSkinDark);
            b.TwoSided(new Vector3(0f, 0f, -0.12f), new Vector3(0f, 0.2f, -0.32f), new Vector3(0f, 0f, -0.24f), EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark);
            b.TwoSided(new Vector3(0f, 0f, -0.12f), new Vector3(0f, 0f, -0.24f), new Vector3(0f, -0.2f, -0.32f), EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark);
        }
    }
}
