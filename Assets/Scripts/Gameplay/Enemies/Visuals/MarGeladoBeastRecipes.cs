using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The beasts of the Mar Gelado (Docs/Mapas100.md 59–64) that the continuous world played with stand-ins, low-poly from
    /// the shared palette like <see cref="FarnkeshBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Foca-Leopardo Apagada</b> (Porto, ~0.6 m tall, 1.5 m long): long grey seal with dark spots and the big reptile
    /// head; front and hind flippers. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c> (the hind flippers); four flipper legs.</item>
    /// <item><b>Morsa de Fumaça</b> (Porto, ~1.1 m): heavy brown walrus whose hide smokes, embers in the folds, long ivory tusks;
    /// lowers its head to gore. <c>Body</c> → <c>Head</c>, <c>Tail</c>; four flipper legs.</item>
    /// <item><b>Gaivota-do-Gelo</b> (Porto, ~0.75 m): white gull with frosted grey wings and black tips; [distância].
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Raposa-do-Ártico Ladra</b> (Porto, ~0.7 m): small white fox, short round ears, a stolen silver fish tied in the
    /// brush. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Urso-Polar Apagado</b> (Canais, ~1.3 m): big white bear streaked with shadow-grey, long neck, small ears.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Rena de Barro</b> (Tundra, ~1.4 m): one of Omöl's clumsy creatures, a reindeer of cracked clay with crooked
    /// antlers that don't match and lichen on its back; lowers its head to charge. <c>Body</c> → <c>Head</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Lebre-Pedra</b> (Tundra, ~0.8 m with the ears): Omöl's hare of grey stone, long ears; throws itself head first.
    /// <c>Body</c> → <c>Head</c>, <c>Tail</c>; four legs (the hind ones big).</item>
    /// <item><b>Ave-Graveto de Omöl</b> (Tundra, ~0.75 m): clumsy bird made of bark and twigs, big yellow eyes, a crooked beak;
    /// [distância]. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Mamute de Baixo da Terra</b> (Galerias, ~2 m): shaggy mammoth that digs the valleys under the earth, frost and
    /// ice crystals on its back, great curved tusks and a trunk; lowers its tusks to charge. <c>Body</c> → <c>Head</c>,
    /// <c>Tail</c>; four thick legs.</item>
    /// <item><b>Lobo-do-Mar</b> (Mar das Auroras, ~0.85 m): slate-blue wolf that swims, webbed paws, a fin down its back and a
    /// finned tail, kelp in its ruff. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Lúcio Gigante</b> (Fundo do Kul, ~0.8 m, 1.4 m long): long olive pike with pale spots and the duck-bill jaw,
    /// swimming in the cold air over the fishing holes. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>
    /// (pectoral fins), <c>Tail</c>.</item>
    /// <item><b>Foca-Sombra</b> (Fundo do Kul, ~0.8 m): round dark seal with pale glowing rings on its coat, long whiskers.
    /// <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four flipper legs.</item>
    /// </list>
    /// </summary>
    public static class MarGeladoBeastRecipes
    {
        // ------------------------------------------------------------------
        // Shared: seal flippers
        // ------------------------------------------------------------------

        /// <summary>Four short flipper legs (a <c>Leg_</c> → <c>Shin_</c> → <c>Foot_</c> chain each, the foot a flat flipper).</summary>
        private static void AddFlippers(ProceduralEnemyBlueprint bp, float hipY, float halfWidth, float front, float hind, float radius,
            EnemySwatch skin, EnemySwatch flipper, string meshKey)
        {
            float upper = hipY * 0.5f;
            float lower = hipY - upper - 0.02f;
            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(halfWidth * side, hipY, front), new Vector3(0f, -upper, 0.02f),
                    new Vector3(0f, -lower, 0.04f), radius, skin, flipper, new Vector3(radius * 2.6f, 0.05f, radius * 3.4f), meshKey + "Front");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(halfWidth * 0.8f * side, hipY, hind), new Vector3(0f, -upper, -0.02f),
                    new Vector3(0f, -lower, -0.04f), radius * 0.9f, skin, flipper, new Vector3(radius * 2.4f, 0.05f, radius * 3f), meshKey + "Hind");
            }
        }

        // ------------------------------------------------------------------
        // Foca-Leopardo Apagada
        // ------------------------------------------------------------------

        public static void BuildLeopardSeal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.36f, 0f), DrawLeopardSealBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.62f), DrawLeopardSealHead, Quaternion.Euler(-6f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.1f), new Vector3(0.14f, 0.04f, 0.2f), EnemySwatch.GolemSnow);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.05f * s, 0.005f, 0.16f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.08f, -0.66f), b => DrawSealTail(b, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark));
            AddFlippers(bp, 0.24f, 0.24f, 0.3f, -0.36f, 0.05f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, "LeopardSeal");
        }

        private static void DrawLeopardSealBody(EnemyMeshBuilder b)
        {
            // A long, lithe body, grey on top and pale beneath, with the dark leopard spots the shadow made spread like ink.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.5f, 1.3f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.46f, 0.32f, 1.1f), EnemySwatch.GolemSnow, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                float x = (i % 2 == 0 ? 0.14f : -0.12f);
                b.Blob(new Vector3(x, 0.16f, -0.4f + i * 0.15f), new Vector3(0.09f, 0.04f, 0.1f), EnemySwatch.Shadow, coarse: true);
            }

            b.Segment(new Vector3(0f, 0.04f, 0.5f), new Vector3(0f, 0.12f, 0.62f), 0.16f, 0.13f, 6, EnemySwatch.GolemStone);
        }

        private static void DrawLeopardSealHead(EnemyMeshBuilder b)
        {
            // The big flat reptile head with the wide gape, the eyes gone pale with the dimming.
            b.Blob(Vector3.zero, new Vector3(0.24f, 0.2f, 0.3f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.02f, 0.14f), new Vector3(0.18f, 0.12f, 0.2f), EnemySwatch.GolemStoneDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, 0.05f, 0.08f), new Vector3(0.04f, 0.04f, 0.03f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.03f * s, 0.02f, 0.24f), new Vector3(0.02f, 0.015f, 0.015f), EnemySwatch.Black, coarse: true);
            }
        }

        private static void DrawSealTail(EnemyMeshBuilder b, EnemySwatch skin, EnemySwatch flipper)
        {
            // The hind flippers, held together like a fish tail.
            b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.16f), 0.1f, 0.05f, 5, skin);
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.02f * s, -0.04f, -0.14f), new Vector3(0.16f * s, -0.06f, -0.32f), new Vector3(0.03f * s, -0.06f, -0.3f), flipper, skin);
            }
        }

        // ------------------------------------------------------------------
        // Morsa de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeWalrus(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawWalrusBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.66f), DrawWalrusHead, Quaternion.Euler(8f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.2f, -0.72f), b => DrawSealTail(b, EnemySwatch.PeccaryFur, EnemySwatch.BarkDark));
            AddFlippers(bp, 0.36f, 0.34f, 0.36f, -0.46f, 0.08f, EnemySwatch.PeccaryFur, EnemySwatch.BarkDark, "SmokeWalrus");
        }

        private static void DrawWalrusBody(EnemyMeshBuilder b)
        {
            // A mountain of wrinkled brown hide; the shadow keeps it smouldering, smoke rising off the back, embers in the folds.
            b.Blob(Vector3.zero, new Vector3(0.9f, 0.82f, 1.46f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, 0.08f, 0.32f), new Vector3(0.84f, 0.7f, 0.66f), EnemySwatch.Leather, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                float x = (i % 2 == 0 ? 0.12f : -0.12f);
                b.Blob(new Vector3(x, 0.36f + (i % 3) * 0.03f, -0.36f + i * 0.16f), new Vector3(0.26f, 0.2f, 0.22f), i % 2 == 0 ? EnemySwatch.GolemStone : EnemySwatch.Shadow, coarse: true);
            }

            b.Blob(new Vector3(0.3f, 0.12f, 0.1f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.28f, 0.18f, -0.14f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            b.Segment(new Vector3(0f, 0.1f, 0.5f), new Vector3(0f, 0.22f, 0.66f), 0.32f, 0.24f, 6, EnemySwatch.PeccaryFur);
        }

        private static void DrawWalrusHead(EnemyMeshBuilder b)
        {
            // Small head, the bristly muzzle, tiny ember eyes and the two long ivory tusks.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.3f, 0.3f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, -0.06f, 0.14f), new Vector3(0.34f, 0.2f, 0.16f), EnemySwatch.Leather, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.1f * s, 0.07f, 0.1f), new Vector3(0.035f, 0.03f, 0.025f), EnemySwatch.Ember, coarse: true);
                b.Horn(new Vector3(0.07f * s, -0.1f, 0.18f), 0.03f, 0.36f, 4, 6f, EnemySwatch.Ivory, Quaternion.Euler(170f, 0f, 0f), 5, Vector3.right);
                for (int k = 0; k < 3; k++)
                {
                    b.Segment(new Vector3(0.06f * s, -0.06f, 0.2f), new Vector3(0.16f * s, -0.08f - k * 0.02f, 0.22f), 0.006f, 0.004f, 3, EnemySwatch.Ivory);
                }
            }
        }

        // ------------------------------------------------------------------
        // Gaivota-do-Gelo
        // ------------------------------------------------------------------

        public static void BuildIceGull(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawIceGullBody, Quaternion.Euler(-8f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.12f), DrawIceGullHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.04f, 0.12f), b =>
            {
                b.Cylinder(Vector3.zero, 0.02f, 0.006f, 0.1f, 4, EnemySwatch.Gold, rotation: Quaternion.Euler(95f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.005f, 0.07f), new Vector3(0.015f, 0.015f, 0.015f), EnemySwatch.WarCape, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Steel, EnemySwatch.GolemSnow, EnemySwatch.Black, 0.9f),
                Quaternion.Euler(0f, 0f, -22f), meshKey: "IceGullWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.13f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Steel, EnemySwatch.GolemSnow, EnemySwatch.Black, 0.9f),
                Quaternion.Euler(0f, 0f, 22f), meshKey: "IceGullWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.18f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.GolemSnow, EnemySwatch.Steel),
                Quaternion.Euler(20f, 0f, 0f));
        }

        private static void DrawIceGullBody(EnemyMeshBuilder b)
        {
            // White body, grey mantle with frost crystals along it: the gull that steals the fish off the harbour's racks.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.32f, 0.44f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, 0.07f, -0.05f), new Vector3(0.24f, 0.14f, 0.34f), EnemySwatch.Steel, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                b.Crystal(new Vector3(0.05f * (i - 1), 0.15f, -0.12f + i * 0.06f), 0.015f, 0.05f, EnemySwatch.Crystal, sides: 4);
            }

            RangedEnemyRecipes.DrawTalons(b, -0.4f, 0.06f, EnemySwatch.WarCape);
        }

        private static void DrawIceGullHead(EnemyMeshBuilder b)
        {
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.17f, 0.16f, 0.2f), EnemySwatch.GolemSnow);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.055f * s, 0.08f, 0.06f), new Vector3(0.03f, 0.03f, 0.022f), EnemySwatch.MoonEye, coarse: true);
            }

            // The upper beak, yellow with the hook at the tip.
            b.Horn(new Vector3(0f, 0.07f, 0.09f), 0.022f, 0.12f, 3, 8f, EnemySwatch.Gold, Quaternion.Euler(90f, 0f, 0f), 4, Vector3.right);
        }

        // ------------------------------------------------------------------
        // Raposa-do-Ártico Ladra
        // ------------------------------------------------------------------

        public static void BuildArcticFox(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawArcticFoxBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.32f), DrawArcticFoxHead, Quaternion.Euler(4f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.07f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.07f), new Vector3(0.06f, 0.025f, 0.12f), EnemySwatch.WolfBelly));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.3f), DrawArcticFoxTail, Quaternion.Euler(-18f, 0f, 0f));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.08f * side, 0.34f, 0.18f), new Vector3(0f, -0.17f, -0.02f), new Vector3(0f, -0.16f, 0.03f),
                    0.035f, EnemySwatch.GolemSnow, EnemySwatch.WolfBelly, "ArcticFoxFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.08f * side, 0.36f, -0.18f), new Vector3(0f, -0.18f, -0.05f), new Vector3(0f, -0.17f, 0.05f),
                    0.04f, EnemySwatch.GolemSnow, EnemySwatch.WolfBelly, "ArcticFoxHind");
            }
        }

        private static void DrawArcticFoxBody(EnemyMeshBuilder b)
        {
            // Thick winter-white fur, a smudge of shadow-grey down the back where the dimming touched it.
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.3f, 0.6f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, 0.12f, -0.04f), new Vector3(0.16f, 0.06f, 0.4f), EnemySwatch.GolemStone, coarse: true);
            b.Blob(new Vector3(0f, -0.02f, 0.2f), new Vector3(0.26f, 0.24f, 0.2f), EnemySwatch.WolfBelly, coarse: true);
            b.Segment(new Vector3(0f, 0.04f, 0.22f), new Vector3(0f, 0.12f, 0.32f), 0.08f, 0.06f, 5, EnemySwatch.GolemSnow);
        }

        private static void DrawArcticFoxHead(EnemyMeshBuilder b)
        {
            // Short muzzle, short round ears against the cold, quick bright eyes.
            b.Blob(Vector3.zero, new Vector3(0.17f, 0.14f, 0.16f), EnemySwatch.GolemSnow);
            b.Cylinder(new Vector3(0f, -0.02f, 0.05f), 0.045f, 0.022f, 0.1f, 5, EnemySwatch.WolfBelly, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.01f, 0.16f), new Vector3(0.025f, 0.02f, 0.02f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.045f * s, 0.03f, 0.06f), new Vector3(0.025f, 0.022f, 0.018f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.05f * s, 0.08f, -0.02f), new Vector3(0.05f, 0.06f, 0.025f), EnemySwatch.GolemSnow, coarse: true);
            }
        }

        private static void DrawArcticFoxTail(EnemyMeshBuilder b)
        {
            // The great white brush, and the silver fish it stole off a drying rack tied in with a string of kelp.
            b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.3f), 0.08f, 0.11f, 6, EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, -0.07f, -0.36f), new Vector3(0.15f, 0.15f, 0.16f), EnemySwatch.GolemSnow, coarse: true);
            b.Cylinder(new Vector3(0f, -0.03f, -0.18f), 0.09f, 0.09f, 0.02f, 6, EnemySwatch.CaveMoss, rotation: Quaternion.Euler(-80f, 0f, 0f));
            b.Blob(new Vector3(0.1f, -0.04f, -0.2f), new Vector3(0.035f, 0.06f, 0.16f), EnemySwatch.Steel, coarse: true);
            b.TwoSided(new Vector3(0.1f, -0.04f, -0.28f), new Vector3(0.1f, 0.02f, -0.34f), new Vector3(0.1f, -0.1f, -0.34f), EnemySwatch.Steel, EnemySwatch.Iron);
        }

        // ------------------------------------------------------------------
        // Urso-Polar Apagado
        // ------------------------------------------------------------------

        public static void BuildPolarBear(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawPolarBearBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.1f, 0.76f), DrawPolarBearHead, Quaternion.Euler(6f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.02f, 0.08f), new Vector3(0.13f, 0.05f, 0.16f), EnemySwatch.GolemSnow);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.045f * s, 0.005f, 0.14f), 0.014f, 0f, 0.045f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.62f), b =>
                b.Blob(Vector3.zero, new Vector3(0.1f, 0.1f, 0.08f), EnemySwatch.GolemSnow, coarse: true));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.2f * side, 0.6f, 0.36f), new Vector3(0f, -0.3f, -0.02f), new Vector3(0f, -0.28f, 0.04f),
                    0.1f, EnemySwatch.GolemSnow, EnemySwatch.BarkDark, new Vector3(0.16f, 0.07f, 0.2f), "PolarBearFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.2f * side, 0.62f, -0.4f), new Vector3(0f, -0.31f, -0.06f), new Vector3(0f, -0.29f, 0.06f),
                    0.11f, EnemySwatch.GolemSnow, EnemySwatch.BarkDark, new Vector3(0.16f, 0.07f, 0.2f), "PolarBearHind");
            }
        }

        private static void DrawPolarBearBody(EnemyMeshBuilder b)
        {
            // The long white body, high hindquarters, and the grey streaks of the dimming running down from the shoulders.
            b.Blob(Vector3.zero, new Vector3(0.7f, 0.68f, 1.36f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, 0.04f, -0.3f), new Vector3(0.64f, 0.66f, 0.6f), EnemySwatch.WolfBelly, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0.32f * s, 0.06f, 0.16f - i * 0.18f), new Vector3(0.025f, 0.28f, 0.04f), EnemySwatch.GolemStone, Quaternion.Euler(14f, 0f, 8f * s));
                }
            }

            b.Segment(new Vector3(0f, 0.04f, 0.52f), new Vector3(0f, 0.1f, 0.76f), 0.2f, 0.15f, 6, EnemySwatch.GolemSnow);
        }

        private static void DrawPolarBearHead(EnemyMeshBuilder b)
        {
            // The long narrow head, black nose, small ears; eyes that the shadow left pale.
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.3f), EnemySwatch.GolemSnow);
            b.Cylinder(new Vector3(0f, -0.03f, 0.08f), 0.08f, 0.06f, 0.14f, 6, EnemySwatch.WolfBelly, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.01f, 0.23f), new Vector3(0.06f, 0.04f, 0.035f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.05f, 0.08f), new Vector3(0.03f, 0.03f, 0.02f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.09f * s, 0.11f, -0.04f), new Vector3(0.06f, 0.06f, 0.03f), EnemySwatch.GolemSnow, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Rena de Barro
        // ------------------------------------------------------------------

        public static void BuildClayReindeer(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.78f, 0f), DrawClayReindeerBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.5f), DrawClayReindeerHead, Quaternion.Euler(16f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.46f), b =>
                b.Blob(Vector3.zero, new Vector3(0.09f, 0.12f, 0.06f), EnemySwatch.Sandstone, coarse: true));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                // Omöl made it in a hurry: the front left leg comes out a little longer than the rest.
                float clumsy = side < 0 ? 0.02f : 0f;
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.13f * side, 0.68f, 0.3f), new Vector3(0f, -0.33f - clumsy, -0.02f), new Vector3(0.01f * side, -0.32f + clumsy, 0.04f),
                    0.055f, EnemySwatch.SandstoneDark, EnemySwatch.BarkDark, new Vector3(0.09f, 0.07f, 0.12f), "ClayReindeerFront" + x);
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.13f * side, 0.7f, -0.3f), new Vector3(0f, -0.34f, -0.06f), new Vector3(0f, -0.33f, 0.06f),
                    0.06f, EnemySwatch.SandstoneDark, EnemySwatch.BarkDark, new Vector3(0.09f, 0.07f, 0.12f), "ClayReindeerHind");
            }
        }

        private static void DrawClayReindeerBody(EnemyMeshBuilder b)
        {
            // A lumpy clay body with the cracks of the firing, a pale clay ruff, and a blanket of lichen on its back.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.5f, 0.96f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0.04f, 0.04f, -0.2f), new Vector3(0.4f, 0.4f, 0.42f), EnemySwatch.Copper, coarse: true);
            b.Blob(new Vector3(0f, -0.04f, 0.34f), new Vector3(0.34f, 0.34f, 0.24f), EnemySwatch.Sandstone, coarse: true);
            b.Blob(new Vector3(0f, 0.22f, -0.06f), new Vector3(0.32f, 0.08f, 0.46f), EnemySwatch.Lichen, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.23f * s, 0.04f, 0.04f), new Vector3(0.015f, 0.2f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(20f * s, 0f, 6f * s));
            }

            b.Segment(new Vector3(0f, 0.12f, 0.36f), new Vector3(0f, 0.3f, 0.5f), 0.12f, 0.08f, 5, EnemySwatch.SandstoneDark);
        }

        private static void DrawClayReindeerHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.16f, 0.16f, 0.26f), EnemySwatch.SandstoneDark);
            b.Cylinder(new Vector3(0f, -0.03f, 0.08f), 0.055f, 0.045f, 0.1f, 5, EnemySwatch.Sandstone, rotation: Quaternion.Euler(95f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                // Kiln-glow eyes, ears of pinched clay.
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.06f), new Vector3(0.03f, 0.03f, 0.022f), EnemySwatch.Ember, coarse: true);
                b.Cylinder(new Vector3(0.07f * s, 0.05f, -0.06f), 0.025f, 0f, 0.08f, 3, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(0f, 0f, -70f * s));
            }

            // Antlers that don't match: one tall and branched, one short and bent the wrong way.
            b.Horn(new Vector3(-0.04f, 0.07f, -0.04f), 0.025f, 0.42f, 5, 10f, EnemySwatch.Bark, Quaternion.Euler(-25f, 0f, 20f), 4, Vector3.forward);
            b.Segment(new Vector3(-0.1f, 0.24f, -0.08f), new Vector3(-0.2f, 0.32f, 0.04f), 0.014f, 0.008f, 3, EnemySwatch.Bark);
            b.Segment(new Vector3(-0.13f, 0.36f, -0.12f), new Vector3(-0.24f, 0.44f, -0.06f), 0.012f, 0.007f, 3, EnemySwatch.Bark);
            b.Horn(new Vector3(0.04f, 0.07f, -0.04f), 0.025f, 0.24f, 4, -14f, EnemySwatch.BarkDark, Quaternion.Euler(-10f, 0f, -40f), 4, Vector3.forward);
        }

        // ------------------------------------------------------------------
        // Lebre-Pedra
        // ------------------------------------------------------------------

        public static void BuildStoneHare(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.32f, 0f), DrawStoneHareBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.14f, 0.22f), DrawStoneHareHead);
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.24f), b =>
                b.Blob(Vector3.zero, new Vector3(0.09f, 0.09f, 0.08f), EnemySwatch.GolemSnow, coarse: true));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.07f * side, 0.24f, 0.13f), new Vector3(0f, -0.12f, 0f), new Vector3(0f, -0.11f, 0.02f),
                    0.03f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, "StoneHareFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.1f * side, 0.28f, -0.12f), new Vector3(0f, -0.13f, 0.06f), new Vector3(0f, -0.14f, 0.02f),
                    0.05f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, "StoneHareHind");
            }
        }

        private static void DrawStoneHareBody(EnemyMeshBuilder b)
        {
            // A rounded grey stone with cracks for fur; Omöl forgot to make it light, so it hits like a thrown rock.
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.46f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.04f, -0.08f), new Vector3(0.28f, 0.26f, 0.3f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(0f, 0.13f, 0f), new Vector3(0.12f, 0.04f, 0.14f), EnemySwatch.Lichen, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.14f * s, 0.02f, 0f), new Vector3(0.012f, 0.12f, 0.016f), EnemySwatch.GolemCrack, Quaternion.Euler(25f, 0f, 8f * s));
            }
        }

        private static void DrawStoneHareHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.16f, 0.14f, 0.18f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.03f, 0.08f), new Vector3(0.08f, 0.06f, 0.05f), EnemySwatch.GolemSnow, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.02f, 0.04f), new Vector3(0.03f, 0.03f, 0.022f), EnemySwatch.MoonEye, coarse: true);
                // The long stone ears, a little lopsided.
                b.Box(new Vector3(0.035f * s, 0.16f, -0.04f), new Vector3(0.04f, 0.24f, 0.015f), EnemySwatch.GolemStone, Quaternion.Euler(-18f, 0f, -10f * s - (s < 0 ? 12f : 0f)));
                b.Box(new Vector3(0.035f * s, 0.16f, -0.03f), new Vector3(0.022f, 0.18f, 0.01f), EnemySwatch.Pink, Quaternion.Euler(-18f, 0f, -10f * s - (s < 0 ? 12f : 0f)));
            }
        }

        // ------------------------------------------------------------------
        // Ave-Graveto de Omöl
        // ------------------------------------------------------------------

        public static void BuildTwigBird(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawTwigBirdBody, Quaternion.Euler(-12f, 0f, 0f));
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.24f, 0.08f), DrawTwigBirdHead, Quaternion.Euler(0f, 0f, 8f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, 0.03f, 0.1f), b =>
                b.Cylinder(Vector3.zero, 0.022f, 0.006f, 0.1f, 4, EnemySwatch.BarkDark, rotation: Quaternion.Euler(100f, 0f, 0f)));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.14f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Bark, EnemySwatch.Lichen, EnemySwatch.BarkDark, 0.8f),
                Quaternion.Euler(0f, 0f, -26f), meshKey: "TwigBirdWing_L");
            // Omöl gave it one wing shorter than the other.
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.14f, 0.1f, -0.02f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Bark, EnemySwatch.Lichen, EnemySwatch.BarkDark, 0.65f),
                Quaternion.Euler(0f, 0f, 18f), meshKey: "TwigBirdWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.18f), b =>
            {
                // A tail of loose sticks, the "feathers" it throws.
                for (int i = -2; i <= 2; i++)
                {
                    b.Segment(Vector3.zero, new Vector3(0.05f * i, -0.04f, -0.3f - Mathf.Abs(i) * 0.03f), 0.016f, 0.008f, 3, i % 2 == 0 ? EnemySwatch.Bark : EnemySwatch.BarkDark);
                }
            }, Quaternion.Euler(18f, 0f, 0f));
        }

        private static void DrawTwigBirdBody(EnemyMeshBuilder b)
        {
            // A bundle of bark and twigs held together with lichen, like a nest that learned to fly.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.34f, 0.4f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, -0.04f, 0.06f), new Vector3(0.22f, 0.24f, 0.24f), EnemySwatch.Lichen, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                float a = i * 47f;
                b.Segment(new Vector3(-0.14f, 0.05f * (i - 1), 0.08f - i * 0.05f), new Vector3(0.14f, 0.05f * (i - 1) + 0.04f, -0.02f - i * 0.04f), 0.012f, 0.01f, 3,
                    EnemySwatch.BarkDark);
                b.Segment(new Vector3(0.08f * Mathf.Cos(a), 0.12f, 0.04f), new Vector3(0.1f * Mathf.Cos(a), 0.24f, -0.06f), 0.01f, 0.006f, 3, EnemySwatch.Bark);
            }

            RangedEnemyRecipes.DrawTalons(b, -0.38f, 0.06f, EnemySwatch.BarkDark);
        }

        private static void DrawTwigBirdHead(EnemyMeshBuilder b)
        {
            // A knot of wood for a head and two huge yellow eyes, too big for it, like the potoo's.
            b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.18f, 0.16f, 0.17f), EnemySwatch.Bark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.055f * s, 0.08f, 0.06f), new Vector3(0.06f, 0.06f, 0.035f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.06f * s, 0.08f, 0.075f), new Vector3(0.022f, 0.022f, 0.012f), EnemySwatch.Black, coarse: true);
            }

            b.Horn(new Vector3(0.01f, 0.06f, 0.08f), 0.022f, 0.11f, 3, 18f, EnemySwatch.BarkDark, Quaternion.Euler(90f, 0f, 10f), 4, Vector3.up);
            b.Segment(new Vector3(0f, 0.13f, -0.02f), new Vector3(0.04f, 0.24f, -0.08f), 0.012f, 0.006f, 3, EnemySwatch.BarkDark);
        }

        // ------------------------------------------------------------------
        // Mamute de Baixo da Terra
        // ------------------------------------------------------------------

        public static void BuildBurrowMammoth(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.05f, 0f), DrawMammothBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.22f, 0.8f), DrawMammothHead, Quaternion.Euler(10f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.8f), b =>
            {
                b.Cylinder(Vector3.zero, 0.05f, 0.03f, 0.3f, 4, EnemySwatch.BarkDark, rotation: Quaternion.Euler(-160f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.3f, -0.1f), new Vector3(0.08f, 0.1f, 0.08f), EnemySwatch.PeccaryFur, coarse: true);
            });

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.3f * side, 0.82f, 0.44f), new Vector3(0f, -0.41f, -0.02f), new Vector3(0f, -0.38f, 0.04f),
                    0.15f, EnemySwatch.BarkDark, EnemySwatch.GolemStoneDark, new Vector3(0.24f, 0.08f, 0.26f), "MammothFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.3f * side, 0.84f, -0.46f), new Vector3(0f, -0.42f, -0.06f), new Vector3(0f, -0.39f, 0.06f),
                    0.16f, EnemySwatch.BarkDark, EnemySwatch.GolemStoneDark, new Vector3(0.24f, 0.08f, 0.26f), "MammothHind");
            }
        }

        private static void DrawMammothBody(EnemyMeshBuilder b)
        {
            // The shaggy hump, the long skirt of brown hair, frozen earth and frost on the back, and the ice crystals that grew
            // on it in the dark under the ground.
            b.Blob(Vector3.zero, new Vector3(1.04f, 1.06f, 1.7f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, 0.3f, 0.3f), new Vector3(0.9f, 0.6f, 0.8f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0f, -0.3f, 0f), new Vector3(0.98f, 0.5f, 1.5f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0f, 0.46f, 0.1f), new Vector3(0.56f, 0.16f, 0.8f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(0.2f, 0.4f, -0.36f), new Vector3(0.3f, 0.12f, 0.3f), EnemySwatch.SandstoneDark, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                b.Crystal(new Vector3(-0.18f + i * 0.12f, 0.5f, 0.3f - i * 0.18f), 0.04f, 0.14f + (i % 2) * 0.06f, EnemySwatch.Crystal, sides: 4);
            }

            b.Segment(new Vector3(0f, 0.1f, 0.6f), new Vector3(0f, 0.22f, 0.8f), 0.4f, 0.32f, 6, EnemySwatch.PeccaryFur);
        }

        private static void DrawMammothHead(EnemyMeshBuilder b)
        {
            // The high domed skull, small furry ears, the trunk curling down, and the great tusks that dig the valleys.
            b.Blob(new Vector3(0f, 0.1f, 0f), new Vector3(0.5f, 0.6f, 0.46f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, 0.3f, -0.04f), new Vector3(0.34f, 0.24f, 0.3f), EnemySwatch.BarkDark, coarse: true);
            b.Segment(new Vector3(0f, -0.06f, 0.18f), new Vector3(0f, -0.4f, 0.32f), 0.1f, 0.07f, 6, EnemySwatch.PeccaryFur);
            b.Segment(new Vector3(0f, -0.4f, 0.32f), new Vector3(0f, -0.66f, 0.26f), 0.07f, 0.05f, 5, EnemySwatch.PeccaryFur);
            b.Segment(new Vector3(0f, -0.66f, 0.26f), new Vector3(0f, -0.74f, 0.36f), 0.05f, 0.04f, 4, EnemySwatch.Leather);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.16f * s, 0.12f, 0.16f), new Vector3(0.045f, 0.04f, 0.03f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.24f * s, 0.06f, -0.06f), new Vector3(0.08f, 0.16f, 0.12f), EnemySwatch.BarkDark, coarse: true);
                b.Horn(new Vector3(0.13f * s, -0.12f, 0.16f), 0.06f, 0.9f, 6, -20f, EnemySwatch.Ivory, Quaternion.Euler(150f, 18f * s, 0f), 6, Vector3.right);
            }
        }

        // ------------------------------------------------------------------
        // Lobo-do-Mar
        // ------------------------------------------------------------------

        public static void BuildSeaWolf(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawSeaWolfBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.4f), DrawSeaWolfHead, Quaternion.Euler(6f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.08f, 0.03f, 0.15f), EnemySwatch.CaveBelly);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.03f * s, 0.005f, 0.13f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.36f), DrawSeaWolfTail, Quaternion.Euler(-14f, 0f, 0f));

            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                OverworldBeastRecipes.AddLeg(bp, "F" + x, new Vector3(0.09f * side, 0.43f, 0.22f), new Vector3(0f, -0.21f, -0.02f), new Vector3(0f, -0.2f, 0.03f),
                    0.04f, EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark, "SeaWolfFront");
                OverworldBeastRecipes.AddLeg(bp, "H" + x, new Vector3(0.09f * side, 0.45f, -0.22f), new Vector3(0f, -0.22f, -0.05f), new Vector3(0f, -0.21f, 0.05f),
                    0.045f, EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark, "SeaWolfHind");
            }
        }

        private static void DrawSeaWolfBody(EnemyMeshBuilder b)
        {
            // Sleek slate-blue fur slicked flat by the sea, kelp tangled in the ruff, and a fin standing up along the spine.
            b.Blob(Vector3.zero, new Vector3(0.36f, 0.34f, 0.76f), EnemySwatch.CaveSkin);
            b.Blob(new Vector3(0f, -0.07f, 0.06f), new Vector3(0.26f, 0.2f, 0.56f), EnemySwatch.CaveBelly, coarse: true);
            b.Blob(new Vector3(0f, 0.04f, 0.26f), new Vector3(0.34f, 0.3f, 0.22f), EnemySwatch.CaveSkinDark, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.14f * s, 0.06f, 0.3f), new Vector3(0.04f, 0.14f, 0.02f), EnemySwatch.CaveMoss, Quaternion.Euler(-20f, 0f, 14f * s));
            }

            b.TwoSided(new Vector3(0f, 0.16f, 0.16f), new Vector3(0f, 0.3f, -0.04f), new Vector3(0f, 0.15f, -0.22f), EnemySwatch.CaveSkinDark, EnemySwatch.CaveSkin);
            b.Segment(new Vector3(0f, 0.04f, 0.28f), new Vector3(0f, 0.12f, 0.4f), 0.09f, 0.07f, 5, EnemySwatch.CaveSkin);
        }

        private static void DrawSeaWolfHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.19f, 0.16f, 0.2f), EnemySwatch.CaveSkin);
            b.Cylinder(new Vector3(0f, -0.02f, 0.06f), 0.055f, 0.03f, 0.14f, 5, EnemySwatch.CaveSkinDark, rotation: Quaternion.Euler(90f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.01f, 0.2f), new Vector3(0.03f, 0.025f, 0.025f), EnemySwatch.Black, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                // Sea-green eyes, ears laid back like a swimmer's.
                b.Blob(new Vector3(0.05f * s, 0.04f, 0.06f), new Vector3(0.03f, 0.025f, 0.02f), EnemySwatch.SoftEye, coarse: true);
                b.Cylinder(new Vector3(0.05f * s, 0.06f, -0.03f), 0.035f, 0f, 0.1f, 3, EnemySwatch.CaveSkinDark, rotation: Quaternion.Euler(-60f, 0f, -14f * s));
            }
        }

        private static void DrawSeaWolfTail(EnemyMeshBuilder b)
        {
            // A wolf's tail that ends in a fish's fin, to steer in the water between the floes.
            b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.26f), 0.06f, 0.05f, 5, EnemySwatch.CaveSkin);
            b.TwoSided(new Vector3(0f, -0.06f, -0.24f), new Vector3(0.14f, -0.04f, -0.42f), new Vector3(0f, -0.06f, -0.36f), EnemySwatch.CaveSkinDark, EnemySwatch.CaveSkin);
            b.TwoSided(new Vector3(0f, -0.06f, -0.24f), new Vector3(0f, -0.06f, -0.36f), new Vector3(-0.14f, -0.04f, -0.42f), EnemySwatch.CaveSkinDark, EnemySwatch.CaveSkin);
        }

        // ------------------------------------------------------------------
        // Lúcio Gigante
        // ------------------------------------------------------------------

        public static void BuildGiantPike(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.4f, 0f), DrawPikeBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.5f), DrawPikeHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.04f), b =>
            {
                // The long lower jaw of the duck-bill, full of needle teeth.
                b.Box(new Vector3(0f, -0.01f, 0.13f), new Vector3(0.1f, 0.03f, 0.26f), EnemySwatch.WolfBelly);
                for (int i = 0; i < 3; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.035f * s, 0.005f, 0.06f + i * 0.07f), 0.008f, 0f, 0.03f, 3, EnemySwatch.Fang);
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.14f, -0.08f, 0.26f), b => DrawPikeFin(b, -1f), Quaternion.Euler(0f, 0f, -20f), meshKey: "PikeFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.14f, -0.08f, 0.26f), b => DrawPikeFin(b, 1f), Quaternion.Euler(0f, 0f, 20f), meshKey: "PikeFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.52f), DrawPikeTail);
        }

        private static void DrawPikeBody(EnemyMeshBuilder b)
        {
            // Long and torpedo-shaped, olive with rows of pale spots, the pale belly, and the dorsal and anal fins set far back.
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.38f, 1.1f), EnemySwatch.Verdigris);
            b.Blob(new Vector3(0f, -0.08f, 0.04f), new Vector3(0.24f, 0.2f, 0.9f), EnemySwatch.OwlFace, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.14f * s, 0.05f - (i % 2) * 0.04f, -0.3f + i * 0.14f), new Vector3(0.03f, 0.03f, 0.05f), EnemySwatch.Lichen, coarse: true);
                }
            }

            b.TwoSided(new Vector3(0f, 0.16f, -0.24f), new Vector3(0f, 0.38f, -0.38f), new Vector3(0f, 0.16f, -0.44f), EnemySwatch.Verdigris, EnemySwatch.CaveMoss);
            b.TwoSided(new Vector3(0f, -0.16f, -0.24f), new Vector3(0f, -0.16f, -0.44f), new Vector3(0f, -0.34f, -0.4f), EnemySwatch.Verdigris, EnemySwatch.CaveMoss);
            // The pelvic fins hanging under the belly, the lowest point as it swims low over the ice.
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.05f * s, -0.16f, 0.04f), new Vector3(0.07f * s, -0.36f, -0.06f), new Vector3(0.05f * s, -0.16f, -0.12f), EnemySwatch.Verdigris, EnemySwatch.CaveMoss);
            }
        }

        private static void DrawPikeHead(EnemyMeshBuilder b)
        {
            // The flat duck-bill snout and the high-set glaring eyes of the hunter of the lake.
            b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.26f), EnemySwatch.Verdigris);
            b.Box(new Vector3(0f, 0.01f, 0.16f), new Vector3(0.12f, 0.06f, 0.24f), EnemySwatch.Verdigris);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.07f, 0.06f), new Vector3(0.04f, 0.04f, 0.03f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.075f * s, 0.07f, 0.07f), new Vector3(0.018f, 0.018f, 0.012f), EnemySwatch.Black, coarse: true);
            }
        }

        private static void DrawPikeFin(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.TwoSided(new Vector3(0f, 0f, 0.05f), new Vector3(0.18f, -0.02f, -0.04f), new Vector3(0.12f, -0.02f, -0.14f), EnemySwatch.Verdigris, EnemySwatch.WolfBelly);
            b.TwoSided(new Vector3(0f, 0f, 0.05f), new Vector3(0.12f, -0.02f, -0.14f), new Vector3(0f, 0f, -0.08f), EnemySwatch.Verdigris, EnemySwatch.WolfBelly);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawPikeTail(EnemyMeshBuilder b)
        {
            b.Segment(new Vector3(0f, 0f, 0.06f), new Vector3(0f, 0f, -0.14f), 0.1f, 0.05f, 5, EnemySwatch.Verdigris);
            b.TwoSided(new Vector3(0f, 0f, -0.12f), new Vector3(0f, 0.22f, -0.34f), new Vector3(0f, 0f, -0.26f), EnemySwatch.Verdigris, EnemySwatch.CaveMoss);
            b.TwoSided(new Vector3(0f, 0f, -0.12f), new Vector3(0f, 0f, -0.26f), new Vector3(0f, -0.22f, -0.34f), EnemySwatch.Verdigris, EnemySwatch.CaveMoss);
        }

        // ------------------------------------------------------------------
        // Foca-Sombra
        // ------------------------------------------------------------------

        public static void BuildShadowSeal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawShadowSealBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.52f), DrawShadowSealHead, Quaternion.Euler(-4f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.05f), new Vector3(0.12f, 0.04f, 0.1f), EnemySwatch.BatFur);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.035f * s, 0.005f, 0.08f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.14f, -0.58f), b => DrawSealTail(b, EnemySwatch.Shadow, EnemySwatch.BatFurDark));
            AddFlippers(bp, 0.28f, 0.3f, 0.3f, -0.4f, 0.065f, EnemySwatch.Shadow, EnemySwatch.BatFurDark, "ShadowSeal");
        }

        private static void DrawShadowSealBody(EnemyMeshBuilder b)
        {
            // Round and heavy, black-grey like water under ice, with the pale rings of the ringed seal that glow like the holes at night.
            b.Blob(Vector3.zero, new Vector3(0.74f, 0.66f, 1.2f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.12f, 0.04f), new Vector3(0.6f, 0.4f, 1f), EnemySwatch.BatFurDark, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                float x = (i % 3 - 1) * 0.16f;
                b.Cylinder(new Vector3(x, 0.3f - Mathf.Abs(x) * 0.4f, -0.36f + i * 0.13f), 0.05f, 0.05f, 0.012f, 6, EnemySwatch.SoftEye,
                    rotation: Quaternion.Euler(0f, 0f, x * 120f));
            }

            b.Segment(new Vector3(0f, 0.08f, 0.4f), new Vector3(0f, 0.2f, 0.52f), 0.24f, 0.18f, 6, EnemySwatch.Shadow);
        }

        private static void DrawShadowSealHead(EnemyMeshBuilder b)
        {
            // A round dog-like head, big dark eyes with a pale shine, and the long whiskers that feel the fish under the ice.
            b.Blob(Vector3.zero, new Vector3(0.28f, 0.26f, 0.28f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, -0.04f, 0.12f), new Vector3(0.18f, 0.12f, 0.12f), EnemySwatch.BatFur, coarse: true);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.08f * s, 0.05f, 0.1f), new Vector3(0.05f, 0.05f, 0.03f), EnemySwatch.SoftEye, coarse: true);
                for (int k = 0; k < 3; k++)
                {
                    b.Segment(new Vector3(0.05f * s, -0.03f, 0.17f), new Vector3(0.2f * s, -0.02f - k * 0.03f, 0.16f), 0.005f, 0.003f, 3, EnemySwatch.Ivory);
                }
            }
        }
    }
}
