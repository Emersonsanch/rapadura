using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The eighteenth pass of beasts (bible §3.2.1): the roles of Região 16, the Reinos do Céu (Docs/Mapas100.md 91–99), that the continuous
    /// world still played with another region's beast, low-poly from the shared palette like <see cref="CeuBeastRecipes"/>: pale, gold and a
    /// little silly, the end of the story. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Arconte Fujão Menor</b> (91, ~1.4 m): a small archon in a travelling cloak, crown slipped sideways, a bundle of stolen sparks under his arm. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Faísca Teimosa</b> (93, ~0.9 m): a round spark with its feet planted on the deck and its arms crossed. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Fumaça de Esfera</b> (95, ~1.3 m): grey-lilac smoke billowing out of a broken shard of crystal sphere. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Sombra que Pisa as Flores</b> (96, ~1.1 m): a shadow on four long thin legs, petals stuck to it. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Espinho de Fumaça</b> (96, ~1.2 m): a bramble of smoke-black thorns with a stolen flower of light caught in it. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Clandestino de Brasa</b> (97, ~0.8 m): a low stowaway of embers with a coal in its mouth and a sooty boarding tag. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Sombra do Meio-Dia</b> (97, ~1.1 m): a squat shadow with a scrap of the sail on its head, its hem spread on the deck. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Última Sombra</b> (98, ~1.9 m): a tall thin shadow reaching for the step above. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// <item><b>Vento do Alto</b> (98, 99, ~1.2 m): a gust in the shape of a bird over a little whirl that touches the ground. Bird skeleton (<c>Body</c> → <c>Head</c> → <c>Jaw</c>, wings, tail).</item>
    /// <item><b>Arconte Fugido</b> (99, ~2 m): an archon who broke out of his sphere, crystal shards on his shoulders and a broken cuff on one wrist. <c>Body</c> → <c>Head</c>, <c>Arm_L</c>/<c>Arm_R</c>; two legs.</item>
    /// </list>
    /// </summary>
    public static class SkyOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Arconte Fujão Menor
        // ------------------------------------------------------------------

        public static void BuildRunawayArchon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.8f, 0f), DrawRunawayArchonBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.4f, 0.04f), b =>
            {
                b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.34f, 0.34f, 0.32f), EnemySwatch.GolemStone);
                b.Box(new Vector3(0f, 0.02f, 0.15f), new Vector3(0.14f, 0.03f, 0.03f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.1f, 0.14f, 0.05f, EnemySwatch.StolenLight);
                // The little crown, slipped to one side on the way out of the Limiar.
                b.Cylinder(new Vector3(0.05f, 0.22f, 0f), 0.1f, 0.11f, 0.07f, 7, EnemySwatch.Gold, rotation: Quaternion.Euler(0f, 0f, -18f), bottomCap: true);
                for (int i = 0; i < 4; i++)
                {
                    float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                    b.Cylinder(new Vector3(0.07f + Mathf.Sin(angle) * 0.09f, 0.28f - Mathf.Sin(angle) * 0.03f, Mathf.Cos(angle) * 0.09f), 0.02f, 0f, 0.06f, 3, EnemySwatch.Gold);
                }
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.28f * side, 0.24f, 0f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.28f, 0.08f), 0.07f, 0.055f, 6, EnemySwatch.Shroud);
                    b.Segment(new Vector3(0.06f * side, -0.28f, 0.08f), new Vector3(0.06f * side, -0.54f, 0.16f), 0.055f, 0.04f, 6, EnemySwatch.GolemStone);
                    b.Blob(new Vector3(0.06f * side, -0.58f, 0.18f), new Vector3(0.12f, 0.13f, 0.12f), EnemySwatch.GolemStoneDark, coarse: true);
                    if (right)
                    {
                        // The bundle of sparks he ran off with, tied in a scrap of cloak; they shine through the knot.
                        b.Blob(new Vector3(0.1f, -0.42f, 0.2f), new Vector3(0.24f, 0.22f, 0.22f), EnemySwatch.Leather);
                        b.Blob(new Vector3(0.1f, -0.36f, 0.28f), new Vector3(0.1f, 0.08f, 0.06f), EnemySwatch.StolenLight, coarse: true);
                    }
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: right ? "RunawayArchonArm_R" : "RunawayArchonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.52f, 0.13f, 0.08f, EnemySwatch.Shroud, EnemySwatch.GolemStone, EnemySwatch.Leather, "RunawayArchon");
        }

        private static void DrawRunawayArchonBody(EnemyMeshBuilder b)
        {
            // When the Gate opened, the small archons of the Limiar ran up the passes after the light like boys after a cart. They never meant to stay; now they cannot remember the way down.
            b.Cylinder(new Vector3(0f, -0.3f, 0f), 0.26f, 0.2f, 0.6f, 9, EnemySwatch.Shroud, bottomCap: true);
            b.Blob(new Vector3(0f, 0.04f, 0f), new Vector3(0.48f, 0.5f, 0.4f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.2f, -0.02f), new Vector3(0.54f, 0.24f, 0.42f), EnemySwatch.Shroud, coarse: true);
            b.Box(new Vector3(0f, -0.12f, 0.19f), new Vector3(0.34f, 0.05f, 0.03f), EnemySwatch.Gold);
            // The travelling cloak flapping behind, and the boarding tag of a barca he never paid for.
            b.Box(new Vector3(0f, -0.06f, -0.22f), new Vector3(0.46f, 0.66f, 0.04f), EnemySwatch.WarCape, Quaternion.Euler(-12f, 0f, 0f));
            b.Box(new Vector3(-0.14f, 0.1f, 0.2f), new Vector3(0.08f, 0.1f, 0.01f), EnemySwatch.Ivory);
        }

        // ------------------------------------------------------------------
        // Faísca Teimosa
        // ------------------------------------------------------------------

        public static void BuildStubbornSpark(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawStubbornSparkBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0f), b =>
            {
                // The light itself, the part that is supposed to go home.
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.36f, 0.36f), EnemySwatch.StolenLight);
                b.Cylinder(new Vector3(0f, 0.14f, 0f), 0.1f, 0f, 0.3f, 5, EnemySwatch.FireflyGlow);
            });
        }

        private static void DrawStubbornSparkBody(EnemyMeshBuilder b)
        {
            // A spark of the light that came all the way up and will not get on the barca. It is not angry. It just will not.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.6f, 0.56f), EnemySwatch.Gold);
            b.Blob(new Vector3(0f, -0.14f, 0.02f), new Vector3(0.52f, 0.3f, 0.48f), EnemySwatch.Ivory, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.08f, 0.25f, 0.07f, EnemySwatch.MoonEye);
            // The pout, and the two little arms crossed over the front.
            b.Box(new Vector3(0f, -0.04f, 0.27f), new Vector3(0.1f, 0.025f, 0.02f), EnemySwatch.CopperDark, Quaternion.Euler(0f, 0f, 8f));
            b.Segment(new Vector3(-0.26f, -0.08f, 0.1f), new Vector3(0.12f, -0.14f, 0.27f), 0.045f, 0.04f, 5, EnemySwatch.Gold);
            b.Segment(new Vector3(0.26f, -0.08f, 0.1f), new Vector3(-0.12f, -0.18f, 0.27f), 0.045f, 0.04f, 5, EnemySwatch.Gold);
            // The feet, planted on the deck: it is not going anywhere.
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.12f * side, -0.24f, 0f), new Vector3(0.14f * side, -0.42f, 0.02f), 0.05f, 0.04f, 5, EnemySwatch.Gold);
                b.Blob(new Vector3(0.14f * side, -0.43f, 0.06f), new Vector3(0.14f, 0.06f, 0.2f), EnemySwatch.Copper, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Fumaça de Esfera
        // ------------------------------------------------------------------

        public static void BuildSphereSmoke(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawSphereSmokeBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.12f, 0.3f), b =>
            {
                // The glare of the archon inside, two sulky eyes in the smoke.
                b.Blob(new Vector3(-0.1f, 0f, 0f), new Vector3(0.12f, 0.08f, 0.06f), EnemySwatch.Sorcery, coarse: true);
                b.Blob(new Vector3(0.1f, 0f, 0f), new Vector3(0.12f, 0.08f, 0.06f), EnemySwatch.Sorcery, coarse: true);
                b.Blob(new Vector3(0f, -0.04f, -0.06f), new Vector3(0.3f, 0.16f, 0.08f), EnemySwatch.Shroud, coarse: true);
            });
        }

        private static void DrawSphereSmokeBody(EnemyMeshBuilder b)
        {
            // The spheres hold the archons in, but not their sulking: it leaks out of every crack as smoke and rolls along the orbits looking for someone to sit on.
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(1.0f, 0.86f, 0.9f), EnemySwatch.Shroud);
            b.Blob(new Vector3(-0.24f, 0.26f, -0.06f), new Vector3(0.56f, 0.5f, 0.52f), EnemySwatch.GolemStone, coarse: true);
            b.Blob(new Vector3(0.26f, 0.22f, 0.04f), new Vector3(0.5f, 0.46f, 0.5f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(0f, 0.44f, 0f), new Vector3(0.4f, 0.32f, 0.4f), EnemySwatch.GolemSnow, coarse: true);
            // The shard of crystal sphere it came out of, curved like a bowl, gold at the rim.
            b.Cylinder(new Vector3(0f, -0.5f, -0.04f), 0.3f, 0.5f, 0.3f, 9, EnemySwatch.Crystal, bottomCap: true);
            b.Cylinder(new Vector3(0f, -0.21f, -0.04f), 0.5f, 0.52f, 0.03f, 9, EnemySwatch.Gold);
            // The smoke pooling on the orbit ridge under it: the lowest point of the model.
            b.Blob(new Vector3(0f, -0.62f, 0f), new Vector3(1.06f, 0.16f, 0.98f), EnemySwatch.Shroud, coarse: true);
        }

        // ------------------------------------------------------------------
        // Sombra que Pisa as Flores
        // ------------------------------------------------------------------

        public static void BuildFlowerTreader(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawFlowerTreaderBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.12f, 0.42f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.22f, 0.3f), EnemySwatch.Shadow);
                b.Box(new Vector3(0f, -0.02f, 0.16f), new Vector3(0.1f, 0.07f, 0.16f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.05f, 0.1f, 0.045f, EnemySwatch.MoonEye);
                // A crushed flower of light caught behind one ear.
                b.Blob(new Vector3(0.1f, 0.1f, -0.06f), new Vector3(0.1f, 0.04f, 0.1f), EnemySwatch.Pink, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.09f, 0.03f, 0.16f), EnemySwatch.Black));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.04f, -0.36f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.1f, -0.3f), 0.05f, 0.015f, 4, EnemySwatch.Shadow));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.64f, 0.17f, 0.26f, -0.26f, 0.045f, EnemySwatch.Shadow, EnemySwatch.Pink, "FlowerTreader");
        }

        private static void DrawFlowerTreaderBody(EnemyMeshBuilder b)
        {
            // The flowers of the Garden of Light sing when they are walked round. This shadow walks straight through them, and every flower it treads on goes quiet.
            b.Blob(Vector3.zero, new Vector3(0.56f, 0.38f, 0.86f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.1f, -0.04f), new Vector3(0.36f, 0.18f, 0.66f), EnemySwatch.Black, coarse: true);
            b.Segment(new Vector3(0f, 0.04f, 0.26f), new Vector3(0f, 0.12f, 0.42f), 0.1f, 0.08f, 6, EnemySwatch.Shadow);
            // The petals stuck to its flanks, still faintly glowing.
            for (int i = 0; i < 3; i++)
            {
                float side = i % 2 == 0 ? 1f : -1f;
                b.Blob(new Vector3(0.24f * side, -0.02f + i * 0.04f, -0.2f + i * 0.18f), new Vector3(0.04f, 0.07f, 0.09f), i == 1 ? EnemySwatch.FireflyGlow : EnemySwatch.Pink, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Espinho de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeThorn(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawSmokeThornBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.06f, 0.18f, 0.34f), b =>
            {
                // The flower of light it caught and will not let go of.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.16f, 0.12f), EnemySwatch.StolenLight);
                for (int i = 0; i < 5; i++)
                {
                    float angle = i * 72f * Mathf.Deg2Rad;
                    b.Blob(new Vector3(Mathf.Sin(angle) * 0.11f, Mathf.Cos(angle) * 0.11f, 0f), new Vector3(0.1f, 0.1f, 0.04f), EnemySwatch.Ivory, coarse: true);
                }
            });
        }

        private static void DrawSmokeThornBody(EnemyMeshBuilder b)
        {
            // Smoke that settled in a hedge of the Garden and hardened there into thorns. It grabs at whatever goes past, flowers and people alike, and keeps what it catches.
            b.Blob(new Vector3(0f, 0f, 0f), new Vector3(0.96f, 0.82f, 0.86f), EnemySwatch.Shadow);
            b.Blob(new Vector3(-0.2f, 0.26f, -0.06f), new Vector3(0.5f, 0.42f, 0.48f), EnemySwatch.Black, coarse: true);
            b.Blob(new Vector3(0.22f, 0.2f, 0.06f), new Vector3(0.46f, 0.4f, 0.44f), EnemySwatch.Shroud, coarse: true);
            // The thorns, long and black, pointing every way.
            for (int i = 0; i < 9; i++)
            {
                float yaw = i * 40f;
                float pitch = -30f + (i % 3) * 30f;
                Quaternion turn = Quaternion.Euler(pitch, yaw, 0f);
                Vector3 root = turn * new Vector3(0f, 0f, 0.36f);
                b.Cylinder(root, 0.05f, 0f, 0.26f, 4, EnemySwatch.Black, rotation: turn * Quaternion.Euler(90f, 0f, 0f));
            }

            // The roots it walks on, gripping the garden path.
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Segment(new Vector3(0.18f * side, -0.3f, 0.06f), new Vector3(0.34f * side, -0.58f, 0.14f), 0.07f, 0.04f, 5, EnemySwatch.BarkDark);
                b.Segment(new Vector3(0.1f * side, -0.3f, -0.12f), new Vector3(0.24f * side, -0.58f, -0.26f), 0.07f, 0.04f, 5, EnemySwatch.BarkDark);
            }

            b.Blob(new Vector3(0f, -0.56f, 0f), new Vector3(0.7f, 0.1f, 0.62f), EnemySwatch.Shroud, coarse: true);
        }

        // ------------------------------------------------------------------
        // Clandestino de Brasa
        // ------------------------------------------------------------------

        public static void BuildEmberStowaway(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.48f, 0f), DrawEmberStowawayBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.1f, 0.44f), b =>
            {
                b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.3f, 0.24f, 0.3f), EnemySwatch.IronDark);
                b.Box(new Vector3(0f, -0.03f, 0.18f), new Vector3(0.13f, 0.08f, 0.18f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.06f, 0.13f, 0.05f, EnemySwatch.Ember);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.09f * s, 0.09f, -0.06f), 0.045f, 0f, 0.14f, 4, EnemySwatch.Black);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.11f, 0.03f, 0.16f), EnemySwatch.Black);
                // The coal it carries in its mouth, the one it stole off the Sun's deck.
                b.Blob(new Vector3(0f, 0.02f, 0.12f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.Ember, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.06f, -0.4f), b =>
            {
                // A tail of smoke with sparks in it.
                b.Segment(Vector3.zero, new Vector3(0f, 0.14f, -0.32f), 0.07f, 0.03f, 5, EnemySwatch.Shroud);
                b.Blob(new Vector3(0f, 0.18f, -0.38f), new Vector3(0.1f, 0.1f, 0.12f), EnemySwatch.Ember, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.4f, 0.2f, 0.26f, -0.28f, 0.065f, EnemySwatch.IronDark, EnemySwatch.Black, "EmberStowaway");
        }

        private static void DrawEmberStowawayBody(EnemyMeshBuilder b)
        {
            // The dark ones that got on the Moon's barca were cold. This one hid on the Sun's, by the brazier, and came out hot, cracked, and very pleased with itself.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.42f, 0.88f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, 0.12f, -0.04f), new Vector3(0.44f, 0.22f, 0.68f), EnemySwatch.Black, coarse: true);
            // The cracks where the embers show through.
            b.Box(new Vector3(0.18f, 0.12f, 0.06f), new Vector3(0.03f, 0.03f, 0.36f), EnemySwatch.Ember, Quaternion.Euler(0f, 18f, 0f));
            b.Box(new Vector3(-0.16f, 0.14f, -0.14f), new Vector3(0.03f, 0.03f, 0.3f), EnemySwatch.Ember, Quaternion.Euler(0f, -24f, 0f));
            b.Box(new Vector3(0f, 0.2f, 0.18f), new Vector3(0.24f, 0.03f, 0.03f), EnemySwatch.SoulFire);
            // The boarding tag round its neck, singed at the edges.
            b.Segment(new Vector3(-0.15f, 0.1f, 0.3f), new Vector3(0.15f, 0.1f, 0.3f), 0.012f, 0.012f, 3, EnemySwatch.Copper);
            b.Box(new Vector3(0f, -0.02f, 0.38f), new Vector3(0.12f, 0.15f, 0.02f), EnemySwatch.SandstoneDark);
        }

        // ------------------------------------------------------------------
        // Sombra do Meio-Dia
        // ------------------------------------------------------------------

        public static void BuildNoonShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawNoonShadowBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.4f, 0.16f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.3f, 0.34f), EnemySwatch.Shadow);
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.15f, 0.05f, EnemySwatch.MoonEye);
                // The scrap of the sail it pulled over its head to hide from the sun.
                b.Box(new Vector3(0f, 0.15f, -0.02f), new Vector3(0.5f, 0.03f, 0.44f), EnemySwatch.Ivory, Quaternion.Euler(-10f, 0f, 8f));
                b.Box(new Vector3(0.23f, 0.06f, 0f), new Vector3(0.03f, 0.18f, 0.4f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 0f, 12f));
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.06f), new Vector3(0.16f, 0.03f, 0.12f), EnemySwatch.Black));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.44f, -0.34f), b =>
                b.Blob(new Vector3(0f, 0f, -0.2f), new Vector3(0.4f, 0.06f, 0.5f), EnemySwatch.Shadow, coarse: true));
        }

        private static void DrawNoonShadowBody(EnemyMeshBuilder b)
        {
            // At noon a shadow has nowhere to go: the sun stands straight overhead. This one squeezed under the sail of the Sun's barca, and comes out to trip whoever walks through its patch of shade.
            b.Blob(Vector3.zero, new Vector3(0.82f, 0.74f, 0.74f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.18f, -0.02f), new Vector3(0.6f, 0.36f, 0.56f), EnemySwatch.Black, coarse: true);
            // The hem spread flat on the deck, as wide as the shade of the sail allows.
            b.Cylinder(new Vector3(0f, -0.48f, 0f), 0.62f, 0.38f, 0.2f, 10, EnemySwatch.Shadow, bottomCap: true);
            b.Blob(new Vector3(0f, -0.44f, 0f), new Vector3(1.2f, 0.06f, 1.1f), EnemySwatch.Shroud, coarse: true);
            // A thread of the sun caught in it.
            b.Box(new Vector3(-0.2f, 0.02f, 0.34f), new Vector3(0.03f, 0.3f, 0.02f), EnemySwatch.StolenLight, Quaternion.Euler(0f, 0f, 20f));
        }

        // ------------------------------------------------------------------
        // Última Sombra
        // ------------------------------------------------------------------

        public static void BuildLastShadow(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.06f, 0f), DrawLastShadowBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.5f, 0.06f), b =>
            {
                b.Blob(new Vector3(0f, 0.06f, 0f), new Vector3(0.26f, 0.34f, 0.26f), EnemySwatch.Shadow);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.1f, 0.11f, 0.045f, EnemySwatch.MoonEye);
                // Looking up: the chin tipped back towards the Column.
                b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.14f, 0.1f, 0.1f), EnemySwatch.Black, coarse: true);
            }, Quaternion.Euler(-16f, 0f, 0f));
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.24f * side, 0.34f, 0f), b =>
                {
                    // Long thin arms, always reaching for the step above.
                    b.Segment(Vector3.zero, new Vector3(0.08f * side, -0.36f, 0.12f), 0.05f, 0.04f, 5, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.08f * side, -0.36f, 0.12f), new Vector3(0.1f * side, -0.7f, 0.22f), 0.04f, 0.025f, 5, EnemySwatch.Black);
                    for (int f = -1; f <= 1; f++)
                    {
                        b.Segment(new Vector3(0.1f * side, -0.7f, 0.22f), new Vector3((0.1f + 0.04f * f) * side, -0.84f, 0.28f), 0.018f, 0.006f, 3, EnemySwatch.Black);
                    }
                }, Quaternion.Euler(-24f, 0f, 0f), meshKey: right ? "LastShadowArm_R" : "LastShadowArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.7f, 0.12f, 0.07f, EnemySwatch.Shadow, EnemySwatch.Black, EnemySwatch.Shroud, "LastShadow");
        }

        private static void DrawLastShadowBody(EnemyMeshBuilder b)
        {
            // The shadows know there is nowhere higher to hide than the Column. These are the last of them, thin from the climb, still trying to go up the steps after the light.
            b.Cylinder(new Vector3(0f, -0.38f, 0f), 0.18f, 0.22f, 0.66f, 8, EnemySwatch.Shadow, bottomCap: true);
            b.Blob(new Vector3(0f, 0.3f, 0f), new Vector3(0.48f, 0.42f, 0.32f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.08f, -0.04f), new Vector3(0.36f, 0.46f, 0.26f), EnemySwatch.Black, coarse: true);
            // Wisps of it left behind on the lower steps, trailing off its back.
            b.Segment(new Vector3(0f, 0.2f, -0.14f), new Vector3(0.06f, -0.3f, -0.42f), 0.06f, 0.015f, 4, EnemySwatch.Shroud);
            b.Segment(new Vector3(-0.08f, 0f, -0.12f), new Vector3(-0.12f, -0.5f, -0.34f), 0.05f, 0.012f, 4, EnemySwatch.Shroud);
            // A grain of the light it caught on the way, held against the chest.
            b.Blob(new Vector3(0f, 0.2f, 0.17f), new Vector3(0.08f, 0.08f, 0.05f), EnemySwatch.StolenLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Vento do Alto
        // ------------------------------------------------------------------

        public static void BuildHighWind(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "HighWind", 0.72f, DrawHighWindBody, new Vector3(0f, 0.26f, 0.34f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.22f, 0.3f), EnemySwatch.GolemSnow);
                    b.Blob(new Vector3(0f, 0.06f, -0.1f), new Vector3(0.2f, 0.12f, 0.24f), EnemySwatch.Ivory, coarse: true);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.12f), 0.04f, 0.01f, 0.12f, 5, EnemySwatch.Gold, rotation: Quaternion.Euler(96f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.03f, 0.08f, 0.045f, EnemySwatch.Crystal);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.12f), EnemySwatch.Gold),
                EnemySwatch.GolemSnow, EnemySwatch.Ivory, EnemySwatch.Gold, 1.05f, EnemySwatch.GolemSnow, EnemySwatch.Ivory);
        }

        private static void DrawHighWindBody(EnemyMeshBuilder b)
        {
            // Up by the Column the wind blows from below, the way the light goes. Where it hits a crack in the sky it twists the wrong way and takes the shape of a bird to come back down.
            b.Blob(Vector3.zero, new Vector3(0.46f, 0.4f, 0.74f), EnemySwatch.GolemSnow);
            b.Blob(new Vector3(0f, 0.08f, -0.12f), new Vector3(0.34f, 0.2f, 0.5f), EnemySwatch.Ivory, coarse: true);
            // The gold streaks of the light the gust is carrying the wrong way.
            b.Box(new Vector3(0.12f, 0.12f, -0.1f), new Vector3(0.025f, 0.025f, 0.5f), EnemySwatch.Gold, Quaternion.Euler(6f, 10f, 0f));
            b.Box(new Vector3(-0.12f, 0.06f, -0.14f), new Vector3(0.025f, 0.025f, 0.44f), EnemySwatch.Gold, Quaternion.Euler(-4f, -12f, 0f));
            b.Blob(new Vector3(0f, -0.02f, 0.22f), new Vector3(0.12f, 0.1f, 0.08f), EnemySwatch.FireflyGlow, coarse: true);
            // The little whirl under it that touches the ground: a wind always keeps one foot down.
            b.Cylinder(new Vector3(0f, -0.7f, 0f), 0.05f, 0.24f, 0.52f, 7, EnemySwatch.GolemSnow, twist: 30f);
            b.Cylinder(new Vector3(0f, -0.46f, 0f), 0.14f, 0.3f, 0.2f, 7, EnemySwatch.Ivory, twist: 60f);
        }

        // ------------------------------------------------------------------
        // Arconte Fugido
        // ------------------------------------------------------------------

        public static void BuildFledArchon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.08f, 0f), DrawFledArchonBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.58f, 0.04f), b =>
            {
                b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.4f, 0.42f, 0.38f), EnemySwatch.GolemStoneDark);
                b.Box(new Vector3(0f, 0.12f, 0.18f), new Vector3(0.28f, 0.04f, 0.03f), EnemySwatch.Black);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.08f, 0.17f, 0.06f, EnemySwatch.Sorcery);
                // The halo of the sphere he was set in, cracked and askew.
                b.Cylinder(new Vector3(0f, 0.34f, -0.04f), 0.24f, 0.24f, 0.03f, 10, EnemySwatch.Gold, rotation: Quaternion.Euler(-14f, 0f, 10f));
                b.Cylinder(new Vector3(0f, 0.35f, -0.04f), 0.19f, 0.19f, 0.03f, 10, EnemySwatch.Black, rotation: Quaternion.Euler(-14f, 0f, 10f));
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.4f * side, 0.36f, 0f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.24f), EnemySwatch.GolemStoneDark, coarse: true);
                    b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.38f, 0.06f), 0.09f, 0.075f, 6, EnemySwatch.WarCape);
                    b.Segment(new Vector3(0.06f * side, -0.38f, 0.06f), new Vector3(0.08f * side, -0.72f, 0.14f), 0.075f, 0.06f, 6, EnemySwatch.GolemStone);
                    b.Blob(new Vector3(0.08f * side, -0.78f, 0.16f), new Vector3(0.18f, 0.18f, 0.18f), EnemySwatch.GolemStoneDark, coarse: true);
                    if (!right)
                    {
                        // The cuff he broke, still on his wrist, two links of the chain hanging off it.
                        b.Cylinder(new Vector3(0.08f * side, -0.68f, 0.14f), 0.1f, 0.1f, 0.08f, 8, EnemySwatch.Iron, bottomCap: true);
                        b.Box(new Vector3(0.1f * side, -0.82f, 0.08f), new Vector3(0.06f, 0.07f, 0.02f), EnemySwatch.Steel, Quaternion.Euler(0f, 0f, 25f));
                        b.Box(new Vector3(0.11f * side, -0.9f, 0.06f), new Vector3(0.06f, 0.07f, 0.02f), EnemySwatch.Steel, Quaternion.Euler(0f, 0f, -25f));
                    }
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: right ? "FledArchonArm_R" : "FledArchonArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.7f, 0.17f, 0.1f, EnemySwatch.WarCape, EnemySwatch.GolemStone, EnemySwatch.Iron, "FledArchon");
        }

        private static void DrawFledArchonBody(EnemyMeshBuilder b)
        {
            // He was one of those the Living Spirit set in the spheres. He broke his sphere from the inside and walked out with the pieces still on him, and now he hides at the Lookout, where the sky is cracked like he is.
            b.Cylinder(new Vector3(0f, -0.4f, 0f), 0.3f, 0.26f, 0.6f, 9, EnemySwatch.WarCape, bottomCap: true);
            b.Blob(new Vector3(0f, 0.12f, 0f), new Vector3(0.7f, 0.66f, 0.5f), EnemySwatch.GolemStoneDark);
            b.Blob(new Vector3(0f, 0.34f, -0.02f), new Vector3(0.8f, 0.3f, 0.52f), EnemySwatch.WarCape, coarse: true);
            b.Box(new Vector3(0f, -0.14f, 0.24f), new Vector3(0.46f, 0.06f, 0.03f), EnemySwatch.Gold);
            // The shards of his sphere, crystal still stuck to his shoulders and back.
            b.Cylinder(new Vector3(-0.3f, 0.42f, -0.04f), 0.07f, 0f, 0.3f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(0f, 0f, 24f));
            b.Cylinder(new Vector3(0.32f, 0.44f, 0.02f), 0.06f, 0f, 0.24f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(10f, 0f, -30f));
            b.Cylinder(new Vector3(0.06f, 0.3f, -0.22f), 0.08f, 0f, 0.34f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(-40f, 0f, 6f));
            // The light he took with him, showing at the chest.
            b.Blob(new Vector3(0f, 0.12f, 0.24f), new Vector3(0.18f, 0.18f, 0.08f), EnemySwatch.StolenLight, coarse: true);
        }
    }
}
