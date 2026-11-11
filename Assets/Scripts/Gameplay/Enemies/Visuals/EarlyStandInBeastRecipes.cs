using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twelfth pass of beasts (bible §3.2.1): the most visible roles of Regions 1–4 that the continuous world still played with stand-ins,
    /// low-poly from the shared palette like <see cref="LateStandInBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Cegonha Apagada</b> (02, ~1.7 m): white stork gone grey at the edges, a bundle of twigs held to throw. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Sapo-Bolha do Lodo</b> (03, ~0.9 m): a fat mud toad with a lit bubble of mud in its throat. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Mosquito de Fumaça</b> (03, ~1 m): a mosquito of smoke, an ember in the belly. <c>Body</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Stinger</c> (the proboscis).</item>
    /// <item><b>Mariposa de Seda Gigante</b> (04, ~1.4 m): pale silk moth with lit eyespots and feathery antennae. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Pombo-do-Louro Apagado</b> (05, ~1.3 m): plump grey laurel pigeon, a green band on the neck. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Lagarto-Gigante do Louro</b> (05, ~1.6 m long): green lizard with a crest of bark. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Pavão de Fumaça</b> (06, ~1.5 m): a peacock whose fan is smoke with embers for eyes. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Lagarto-Espinho</b> (10, ~1.4 m long): sand-coloured lizard covered in thorns. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Lince Apagado</b> (14, ~1 m): grey mountain lynx with a dark patch where its light went out. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Borrão</b> (15, ~1.5 m): the shadow that comes out of the paintings, smeared with the ochre it ate. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Gralha de Pedra</b> (16, ~1.2 m): slate-grey jackdaw with pale eyes, a pebble in the beak. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// <item><b>Ubır Jovem</b> (20, ~1.3 m): a young ubır, a floating ball of dark hair with an ember mouth. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Wing_L</c>/<c>Wing_R</c>, <c>Tail</c>.</item>
    /// </list>
    /// </summary>
    public static class EarlyStandInBeastRecipes
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

        // ------------------------------------------------------------------
        // Cegonha Apagada
        // ------------------------------------------------------------------

        public static void BuildDimmedStork(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.86f, 0f), DrawDimmedStorkBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 0.44f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.24f), EnemySwatch.OwlFace);
                b.Cylinder(new Vector3(0f, 0.01f, 0.1f), 0.04f, 0.012f, 0.34f, 5, EnemySwatch.GolemStoneDark, rotation: Quaternion.Euler(84f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.04f, 0.07f, 0.045f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.005f, 0.14f), new Vector3(0.05f, 0.02f, 0.28f), EnemySwatch.IronDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.12f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.OwlFace, EnemySwatch.GolemSnow, EnemySwatch.Black, 0.95f), Quaternion.Euler(0f, 0f, -12f), meshKey: "DimmedStorkWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.12f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.OwlFace, EnemySwatch.GolemSnow, EnemySwatch.Black, 0.95f), Quaternion.Euler(0f, 0f, 12f), meshKey: "DimmedStorkWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.02f, -0.36f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.Shadow), Quaternion.Euler(14f, 0f, 0f));
        }

        private static void DrawDimmedStorkBody(EnemyMeshBuilder b)
        {
            // The storks nest on the meadow's poles every spring. This one came back with its white going grey and the nest it builds is for throwing.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.46f, 0.8f), EnemySwatch.OwlFace);
            b.Blob(new Vector3(0f, 0.06f, -0.2f), new Vector3(0.4f, 0.3f, 0.46f), EnemySwatch.GolemStone, coarse: true);
            b.Segment(new Vector3(0f, 0.14f, 0.28f), new Vector3(0f, 0.72f, 0.42f), 0.07f, 0.05f, 6, EnemySwatch.OwlFace);
            // The twigs it carries to throw, tied under the breast, one of them still smouldering.
            for (int i = 0; i < 3; i++)
            {
                b.Segment(new Vector3(-0.16f + i * 0.16f, -0.2f, 0.26f), new Vector3(-0.1f + i * 0.1f, -0.16f, -0.16f), 0.018f, 0.014f, 4, EnemySwatch.Bark);
            }

            b.Blob(new Vector3(0.12f, -0.16f, -0.16f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            DrawBirdLegs(b, 0.86f, 0.09f, EnemySwatch.CopperDark, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Sapo-Bolha do Lodo
        // ------------------------------------------------------------------

        public static void BuildBubbleToad(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.44f, 0f), DrawBubbleToadBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.12f, 0.36f), b =>
            {
                // The bubble of mud in its throat, with the bit of light it swallowed with the water still showing through.
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.3f, 0.3f), EnemySwatch.BarkDark);
                b.Blob(new Vector3(0f, 0.02f, 0.06f), new Vector3(0.18f, 0.16f, 0.16f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawBubbleToadBody(EnemyMeshBuilder b)
        {
            // It drank from the dark mire until the mire was in it. Now it blows the mud back out at whoever comes along the boardwalk.
            b.Blob(Vector3.zero, new Vector3(0.92f, 0.62f, 0.86f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, -0.14f, 0.06f), new Vector3(0.74f, 0.34f, 0.7f), EnemySwatch.Lichen, coarse: true);
            b.Blob(new Vector3(0f, 0.18f, -0.06f), new Vector3(0.66f, 0.3f, 0.6f), EnemySwatch.BarkDark, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.26f, 0.26f, Mathf.Sin(a) * 0.22f - 0.04f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.Bark, coarse: true);
            }

            // The big eyes on top, and the four splayed feet: the lowest point of the model.
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                b.Blob(new Vector3(0.2f * side, 0.3f, 0.26f), new Vector3(0.16f, 0.16f, 0.16f), EnemySwatch.CaveMoss, coarse: true);
                b.Blob(new Vector3(0.21f * side, 0.33f, 0.32f), new Vector3(0.08f, 0.08f, 0.06f), EnemySwatch.MoonEye, coarse: true);
                b.Box(new Vector3(0.36f * side, -0.4f, 0.26f), new Vector3(0.2f, 0.04f, 0.22f), EnemySwatch.CaveMoss, Quaternion.Euler(0f, 20f * side, 0f));
                b.Box(new Vector3(0.4f * side, -0.4f, -0.22f), new Vector3(0.26f, 0.04f, 0.3f), EnemySwatch.CaveMoss, Quaternion.Euler(0f, -24f * side, 0f));
            }
        }

        // ------------------------------------------------------------------
        // Mosquito de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeMosquito(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawSmokeMosquitoBody);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.08f * side, 0.12f, -0.02f),
                    b => DrawSmokeMosquitoWing(b, side), Quaternion.Euler(0f, 0f, 14f * side), meshKey: s < 0 ? "SmokeMosquitoWing_L" : "SmokeMosquitoWing_R");
            }

            bp.Mesh(ProceduralEnemyParts.Stinger, body, new Vector3(0f, 0f, 0.3f), b =>
            {
                // The proboscis, pointing forward and down; it swings up on the windup like a sting.
                b.Cylinder(Vector3.zero, 0.025f, 0.004f, 0.3f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(110f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.1f, 0.27f), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawSmokeMosquitoBody(EnemyMeshBuilder b)
        {
            // Smoke, the first element of the Darkness, gathered over the mire in a cloud and decided to bite. The ember is what it drank.
            b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.28f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, 0.02f, 0.2f), new Vector3(0.18f, 0.18f, 0.16f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.05f, -0.34f), new Vector3(0.2f, 0.18f, 0.56f), EnemySwatch.Shadow);
            b.Blob(new Vector3(0f, 0.05f, -0.38f), new Vector3(0.12f, 0.1f, 0.3f), EnemySwatch.Ember, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0f, 0.05f, -0.16f - i * 0.11f), new Vector3(0.21f - i * 0.02f, 0.19f - i * 0.02f, 0.04f), EnemySwatch.Shroud, coarse: true);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.07f * s, 0.05f, 0.24f), new Vector3(0.07f, 0.09f, 0.07f), EnemySwatch.Ember, coarse: true);
                b.Segment(new Vector3(0.03f * s, 0.08f, 0.26f), new Vector3(0.12f * s, 0.2f, 0.34f), 0.01f, 0.006f, 3, EnemySwatch.Shroud);
                // Long thin legs hanging under the hover: the lowest point of the model.
                for (int k = 0; k < 3; k++)
                {
                    float z = 0.06f - k * 0.09f;
                    b.Segment(new Vector3(0.06f * s, -0.08f, z), new Vector3(0.2f * s, -0.3f, z - 0.02f), 0.016f, 0.012f, 4, EnemySwatch.Shadow);
                    b.Segment(new Vector3(0.2f * s, -0.3f, z - 0.02f), new Vector3(0.18f * s, -0.54f, z - 0.05f), 0.012f, 0.006f, 3, EnemySwatch.Shroud);
                }
            }
        }

        private static void DrawSmokeMosquitoWing(EnemyMeshBuilder b, float side)
        {
            b.Box(new Vector3(0.28f * side, 0.02f, -0.06f), new Vector3(0.56f, 0.012f, 0.16f), EnemySwatch.Shroud, Quaternion.Euler(0f, 24f * side, 0f));
            b.Box(new Vector3(0.2f * side, 0.01f, -0.18f), new Vector3(0.36f, 0.01f, 0.1f), EnemySwatch.Shadow, Quaternion.Euler(0f, 40f * side, 0f));
        }

        // ------------------------------------------------------------------
        // Mariposa de Seda Gigante
        // ------------------------------------------------------------------

        public static void BuildSilkMoth(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawSilkMothBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.04f, 0.26f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.18f), EnemySwatch.Ivory);
                for (int s = -1; s <= 1; s += 2)
                {
                    float side = s;
                    // Feathery antennae, combed like the threads on a loom.
                    b.Segment(new Vector3(0.04f * side, 0.08f, 0.04f), new Vector3(0.18f * side, 0.34f, 0.16f), 0.012f, 0.006f, 3, EnemySwatch.IvoryDark);
                    for (int k = 0; k < 3; k++)
                    {
                        float f = 0.3f + k * 0.22f;
                        b.Box(new Vector3((0.04f + 0.14f * f) * side, 0.08f + 0.26f * f, 0.04f + 0.12f * f), new Vector3(0.1f, 0.01f, 0.03f), EnemySwatch.Ivory, Quaternion.Euler(0f, 0f, 30f * side));
                    }
                }

                HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.01f, 0.06f, 0.06f, EnemySwatch.Black);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.06f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.08f, 0.02f, 0.08f), EnemySwatch.IvoryDark));
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.1f * side, 0.1f, 0.04f),
                    b => DrawSilkMothWing(b, side), Quaternion.Euler(0f, 0f, 10f * side), meshKey: s < 0 ? "SilkMothWing_L" : "SilkMothWing_R");
            }

            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.02f, -0.42f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.14f, 0.2f), EnemySwatch.IvoryDark);
                b.Blob(new Vector3(0f, -0.01f, -0.1f), new Vector3(0.08f, 0.07f, 0.08f), EnemySwatch.FireflyGlow, coarse: true);
            });
        }

        private static void DrawSilkMothBody(EnemyMeshBuilder b)
        {
            // It grew up on the mulberries of the looms and never spun its cocoon; it flies about with the silk still in it, and the dust off its wings makes you sleep.
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.3f, 0.34f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, -0.02f, -0.24f), new Vector3(0.26f, 0.24f, 0.42f), EnemySwatch.IvoryDark);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(0f, -0.02f, -0.12f - i * 0.1f), new Vector3(0.27f - i * 0.03f, 0.25f - i * 0.03f, 0.04f), EnemySwatch.Ivory, coarse: true);
            }

            for (int s = -1; s <= 1; s += 2)
            {
                // Six legs hanging under the hover, the last joint dusted: the lowest point of the model.
                for (int k = 0; k < 3; k++)
                {
                    float z = 0.08f - k * 0.08f;
                    b.Segment(new Vector3(0.06f * s, -0.1f, z), new Vector3(0.16f * s, -0.38f, z - 0.02f), 0.018f, 0.014f, 4, EnemySwatch.IvoryDark);
                    b.Segment(new Vector3(0.16f * s, -0.38f, z - 0.02f), new Vector3(0.14f * s, -0.68f, z - 0.05f), 0.014f, 0.006f, 3, EnemySwatch.Ivory);
                }
            }
        }

        private static void DrawSilkMothWing(EnemyMeshBuilder b, float side)
        {
            // Broad and rounded, the colour of raw silk, an eyespot of stolen light on each.
            b.Box(new Vector3(0.34f * side, 0.02f, 0.06f), new Vector3(0.6f, 0.014f, 0.42f), EnemySwatch.Ivory, Quaternion.Euler(0f, -10f * side, 0f));
            b.Box(new Vector3(0.28f * side, 0.012f, -0.26f), new Vector3(0.44f, 0.012f, 0.3f), EnemySwatch.OwlBuff, Quaternion.Euler(0f, 18f * side, 0f));
            b.Blob(new Vector3(0.42f * side, 0.035f, 0.08f), new Vector3(0.14f, 0.02f, 0.14f), EnemySwatch.StolenLight, coarse: true);
            b.Blob(new Vector3(0.3f * side, 0.025f, -0.26f), new Vector3(0.09f, 0.02f, 0.09f), EnemySwatch.IvoryDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Pombo-do-Louro Apagado
        // ------------------------------------------------------------------

        public static void BuildLaurelPigeon(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawLaurelPigeonBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.36f, 0.34f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.26f), EnemySwatch.GolemStone);
                b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.035f, 0.012f, 0.1f, 5, EnemySwatch.Ivory, rotation: Quaternion.Euler(84f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.07f, 0.05f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.1f), b =>
                b.Box(new Vector3(0f, -0.005f, 0.05f), new Vector3(0.06f, 0.02f, 0.1f), EnemySwatch.IvoryDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.22f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark, 0.8f), Quaternion.Euler(0f, 0f, -14f), meshKey: "LaurelPigeonWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.22f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark, 0.8f), Quaternion.Euler(0f, 0f, 14f), meshKey: "LaurelPigeonWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.36f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark), Quaternion.Euler(10f, 0f, 0f));
        }

        private static void DrawLaurelPigeonBody(EnemyMeshBuilder b)
        {
            // The pigeon of the laurel woods eats only laurel berries. The mist took its colour and left the green band, so it spits the stones of the berries instead.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.56f, 0.78f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.1f, 0.14f), new Vector3(0.46f, 0.36f, 0.42f), EnemySwatch.Lichen, coarse: true);
            b.Cylinder(new Vector3(0f, 0.18f, 0.28f), 0.15f, 0.12f, 0.08f, 8, EnemySwatch.Verdigris, rotation: Quaternion.Euler(30f, 0f, 0f));
            b.Blob(new Vector3(0f, 0.2f, 0.3f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.FireflyGlow, coarse: true);
            DrawBirdLegs(b, 0.6f, 0.1f, EnemySwatch.Pink, EnemySwatch.CopperDark);
        }

        // ------------------------------------------------------------------
        // Lagarto-Gigante do Louro
        // ------------------------------------------------------------------

        public static void BuildLaurelLizard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawLaurelLizardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.58f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.42f, 0.3f, 0.5f), EnemySwatch.CaveMoss);
                b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.3f, 0.14f, 0.36f), EnemySwatch.Lichen, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.13f, 0.08f, 0.12f, 0.07f, EnemySwatch.MoonEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.12f), new Vector3(0.26f, 0.05f, 0.28f), EnemySwatch.Lichen);
                b.Cylinder(new Vector3(0.07f, 0.01f, 0.22f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.07f, 0.01f, 0.22f), 0.012f, 0f, 0.035f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.5f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.4f), 0.14f, 0.08f, 6, EnemySwatch.CaveMoss);
                b.Segment(new Vector3(0f, -0.06f, -0.4f), new Vector3(0f, -0.14f, -0.78f), 0.08f, 0.02f, 5, EnemySwatch.Bark);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.28f, 0.28f, 0.34f, -0.32f, 0.075f, EnemySwatch.CaveMoss, EnemySwatch.BarkDark, "LaurelLizard");
        }

        private static void DrawLaurelLizardBody(EnemyMeshBuilder b)
        {
            // The giant lizards of the laurel woods are real and very old. The mist dried this one out and it thinks the whole canopy is its stone to sun on.
            b.Blob(Vector3.zero, new Vector3(0.74f, 0.44f, 1.1f), EnemySwatch.CaveMoss);
            b.Blob(new Vector3(0f, -0.1f, 0.02f), new Vector3(0.54f, 0.24f, 0.86f), EnemySwatch.Lichen, coarse: true);
            // A crest of bark down the spine, a laurel leaf caught in it, and a little light it kept from the sun.
            for (int i = 0; i < 5; i++)
            {
                b.Box(new Vector3(0f, 0.22f, 0.4f - i * 0.2f), new Vector3(0.05f, 0.1f - i * 0.01f, 0.12f), EnemySwatch.Bark, Quaternion.Euler(-12f, 0f, 0f));
            }

            b.Box(new Vector3(0.14f, 0.2f, -0.1f), new Vector3(0.12f, 0.015f, 0.22f), EnemySwatch.Verdigris, Quaternion.Euler(0f, 30f, 10f));
            b.Blob(new Vector3(-0.12f, 0.18f, 0.1f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.StolenLight, coarse: true);
        }

        // ------------------------------------------------------------------
        // Pavão de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokePeacock(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.66f, 0f), DrawSmokePeacockBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.58f, 0.4f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.22f), EnemySwatch.Shroud);
                b.Cylinder(new Vector3(0f, -0.01f, 0.08f), 0.03f, 0.01f, 0.1f, 5, EnemySwatch.IronDark, rotation: Quaternion.Euler(84f, 0f, 0f));
                // The crest: three little stalks with an ember at each tip.
                for (int i = -1; i <= 1; i++)
                {
                    b.Segment(new Vector3(0.03f * i, 0.08f, -0.02f), new Vector3(0.06f * i, 0.24f, -0.06f), 0.008f, 0.006f, 3, EnemySwatch.Shadow);
                    b.Blob(new Vector3(0.06f * i, 0.25f, -0.06f), new Vector3(0.035f, 0.035f, 0.035f), EnemySwatch.Ember, coarse: true);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.03f, 0.07f, 0.04f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.005f, 0.05f), new Vector3(0.05f, 0.02f, 0.1f), EnemySwatch.IronDark));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.Shroud, EnemySwatch.Shadow, EnemySwatch.Verdigris, 0.7f), Quaternion.Euler(0f, 0f, -12f), meshKey: "SmokePeacockWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.Shroud, EnemySwatch.Shadow, EnemySwatch.Verdigris, 0.7f), Quaternion.Euler(0f, 0f, 12f), meshKey: "SmokePeacockWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.34f), DrawSmokePeacockFan);
        }

        private static void DrawSmokePeacockBody(EnemyMeshBuilder b)
        {
            // Mihrshah's garden kept peacocks once. The one that stayed is all smoke now except the eyes in its tail, and it throws those.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.5f, 0.74f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0f, 0.04f, 0.18f), new Vector3(0.34f, 0.36f, 0.36f), EnemySwatch.Verdigris, coarse: true);
            b.Segment(new Vector3(0f, 0.16f, 0.26f), new Vector3(0f, 0.58f, 0.38f), 0.08f, 0.055f, 6, EnemySwatch.Verdigris);
            DrawBirdLegs(b, 0.66f, 0.09f, EnemySwatch.GolemStoneDark, EnemySwatch.IronDark);
        }

        private static void DrawSmokePeacockFan(EnemyMeshBuilder b)
        {
            // The raised fan of smoke, seven feathers, an ember eye on each.
            for (int i = 0; i < 7; i++)
            {
                float angle = -60f + i * 20f;
                Quaternion tilt = Quaternion.Euler(-20f, 0f, angle);
                Vector3 dir = tilt * Vector3.up;
                b.Box(dir * 0.36f + new Vector3(0f, 0f, -0.08f), new Vector3(0.12f, 0.66f, 0.02f), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.Shadow, tilt);
                b.Blob(dir * 0.62f + new Vector3(0f, 0f, -0.1f), new Vector3(0.1f, 0.1f, 0.03f), EnemySwatch.Ember, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Lagarto-Espinho
        // ------------------------------------------------------------------

        public static void BuildThornLizard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawThornLizardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.06f, 0.5f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.26f, 0.4f), EnemySwatch.Sandstone);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.1f * s, 0.1f, -0.06f), 0.04f, 0f, 0.16f, 4, EnemySwatch.SandstoneDark, rotation: Quaternion.Euler(-30f, 0f, -20f * s));
                }

                HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.06f, 0.1f, 0.06f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.1f), new Vector3(0.22f, 0.04f, 0.22f), EnemySwatch.SandstoneLight));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.44f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.32f), 0.11f, 0.06f, 6, EnemySwatch.Sandstone);
                b.Segment(new Vector3(0f, -0.04f, -0.32f), new Vector3(0f, -0.1f, -0.6f), 0.06f, 0.015f, 5, EnemySwatch.SandstoneDark);
                for (int i = 0; i < 3; i++)
                {
                    b.Cylinder(new Vector3(0f, 0.06f - i * 0.02f, -0.1f - i * 0.16f), 0.03f, 0f, 0.1f, 4, EnemySwatch.SandstoneDark);
                }
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.24f, 0.26f, 0.3f, -0.3f, 0.065f, EnemySwatch.Sandstone, EnemySwatch.SandstoneDark, "ThornLizard");
        }

        private static void DrawThornLizardBody(EnemyMeshBuilder b)
        {
            // The thorny lizard drinks the dew off its own back. The Living Fire dried the dunes and it learned to drink the sparks instead; the thorns glow at the tips.
            b.Blob(Vector3.zero, new Vector3(0.7f, 0.4f, 0.92f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, -0.08f, 0f), new Vector3(0.5f, 0.22f, 0.72f), EnemySwatch.SandstoneLight, coarse: true);
            for (int i = 0; i < 9; i++)
            {
                float x = (i % 3 - 1) * 0.18f;
                float z = 0.28f - (i / 3) * 0.26f;
                b.Cylinder(new Vector3(x, 0.14f, z), 0.045f, 0f, 0.16f, 4, i % 3 == 1 ? EnemySwatch.SandstoneDark : EnemySwatch.WarCape);
                b.Blob(new Vector3(x, 0.3f, z), new Vector3(0.025f, 0.025f, 0.025f), EnemySwatch.Ember, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Lince Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedLynx(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.6f, 0f), DrawDimmedLynxBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.2f, 0.52f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.32f, 0.34f), EnemySwatch.GolemStone);
                b.Blob(new Vector3(0f, -0.08f, 0.1f), new Vector3(0.22f, 0.14f, 0.2f), EnemySwatch.GolemSnow, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.11f * s, 0.12f, -0.03f), 0.05f, 0.01f, 0.18f, 4, EnemySwatch.GolemStoneDark);
                    b.Segment(new Vector3(0.11f * s, 0.29f, -0.03f), new Vector3(0.12f * s, 0.38f, -0.04f), 0.01f, 0.004f, 3, EnemySwatch.Black);
                    // The ruff of the lynx's cheeks.
                    b.Blob(new Vector3(0.15f * s, -0.08f, 0.02f), new Vector3(0.1f, 0.14f, 0.1f), EnemySwatch.GolemSnow, coarse: true);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.04f, 0.13f, 0.055f, EnemySwatch.SoftEye);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.1f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.015f, 0.08f), new Vector3(0.16f, 0.04f, 0.16f), EnemySwatch.GolemStoneDark);
                b.Cylinder(new Vector3(0.05f, 0.005f, 0.14f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.05f, 0.005f, 0.14f), 0.012f, 0f, 0.04f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.08f, -0.48f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0f, 0.02f, -0.16f), 0.07f, 0.06f, 5, EnemySwatch.GolemStone);
                b.Blob(new Vector3(0f, 0.02f, -0.19f), new Vector3(0.1f, 0.1f, 0.08f), EnemySwatch.Black, coarse: true);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.5f, 0.24f, 0.34f, -0.34f, 0.075f, EnemySwatch.GolemStone, EnemySwatch.GolemSnow, "DimmedLynx");
        }

        private static void DrawDimmedLynxBody(EnemyMeshBuilder b)
        {
            // It hunts the narrow passes on snow feet. Where the light went out of it there is a dark patch on its flank that takes in the snow and gives nothing back.
            b.Blob(Vector3.zero, new Vector3(0.7f, 0.54f, 1f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.14f, 0.02f), new Vector3(0.5f, 0.26f, 0.74f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(0.22f, 0.06f, -0.08f), new Vector3(0.3f, 0.3f, 0.36f), EnemySwatch.Shadow, coarse: true);
            b.Blob(new Vector3(-0.2f, 0.14f, 0.18f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(-0.16f, 0.12f, -0.22f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.GolemStoneDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Borrão
        // ------------------------------------------------------------------

        public static void BuildSmudge(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.74f, 0f), DrawSmudgeBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.58f, 0.06f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.34f, 0.3f, 0.28f), EnemySwatch.Shadow);
                b.Box(new Vector3(0.06f, 0.06f, 0.12f), new Vector3(0.2f, 0.04f, 0.03f), EnemySwatch.WarCape, Quaternion.Euler(0f, 0f, 20f));
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.02f, 0.13f, 0.05f, EnemySwatch.StolenLight);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.12f), b =>
                b.Box(new Vector3(0f, -0.02f, 0.05f), new Vector3(0.16f, 0.04f, 0.1f), EnemySwatch.Black));
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.28f, 0.2f, 0.02f), b => DrawSmudgeArm(b, -1f), Quaternion.Euler(0f, 0f, -20f), meshKey: "SmudgeArm_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.28f, 0.2f, 0.02f), b => DrawSmudgeArm(b, 1f), Quaternion.Euler(0f, 0f, 20f), meshKey: "SmudgeArm_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.3f, -0.36f), b =>
            {
                b.Segment(Vector3.zero, new Vector3(0.04f, -0.2f, -0.36f), 0.08f, 0.03f, 5, EnemySwatch.Shadow);
                b.Blob(new Vector3(0.04f, -0.22f, -0.4f), new Vector3(0.1f, 0.06f, 0.12f), EnemySwatch.SandstoneDark, coarse: true);
            });
        }

        private static void DrawSmudgeArm(EnemyMeshBuilder b, float side)
        {
            // A long smear of a hand, the colours it rubbed off the walls still on the fingers.
            b.Segment(Vector3.zero, new Vector3(0.26f * side, 0.06f, 0.06f), 0.07f, 0.05f, 5, EnemySwatch.Shadow);
            b.Segment(new Vector3(0.26f * side, 0.06f, 0.06f), new Vector3(0.48f * side, -0.12f, 0.12f), 0.05f, 0.02f, 5, EnemySwatch.Shroud);
            b.Blob(new Vector3(0.5f * side, -0.14f, 0.13f), new Vector3(0.08f, 0.08f, 0.08f), side < 0 ? EnemySwatch.WarCape : EnemySwatch.SandstoneLight, coarse: true);
        }

        private static void DrawSmudgeBody(EnemyMeshBuilder b)
        {
            // It came out of the painted shelters, where it ate the eland and the hunters one colour at a time. What it ate shows through it in streaks.
            for (int i = 0; i < 5; i++)
            {
                float y = -0.66f + i * 0.28f;
                float width = 0.96f - i * 0.12f;
                b.Blob(new Vector3(0.03f * (i % 2 == 0 ? 1f : -1f), y, 0f), new Vector3(width, 0.24f, width * 0.86f), i % 2 == 0 ? EnemySwatch.Shadow : EnemySwatch.Shroud);
            }

            b.Box(new Vector3(-0.12f, -0.1f, 0.34f), new Vector3(0.36f, 0.06f, 0.03f), EnemySwatch.WarCape, Quaternion.Euler(0f, 0f, -14f));
            b.Box(new Vector3(0.1f, 0.12f, 0.3f), new Vector3(0.28f, 0.05f, 0.03f), EnemySwatch.SandstoneLight, Quaternion.Euler(0f, 0f, 10f));
            b.Box(new Vector3(0f, -0.36f, 0.4f), new Vector3(0.3f, 0.05f, 0.03f), EnemySwatch.Ivory, Quaternion.Euler(0f, 0f, 4f));
            b.Box(new Vector3(0f, -0.72f, 0f), new Vector3(0.56f, 0.03f, 0.5f), EnemySwatch.Shadow, Quaternion.Euler(0f, 20f, 0f));
        }

        // ------------------------------------------------------------------
        // Gralha de Pedra
        // ------------------------------------------------------------------

        public static void BuildStoneJackdaw(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.58f, 0f), DrawStoneJackdawBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.32f, 0.32f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.26f), EnemySwatch.IronDark);
                b.Blob(new Vector3(0f, 0.02f, -0.06f), new Vector3(0.22f, 0.2f, 0.18f), EnemySwatch.GolemStone, coarse: true);
                b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.04f, 0.01f, 0.12f, 5, EnemySwatch.Black, rotation: Quaternion.Euler(84f, 0f, 0f));
                HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.04f, 0.07f, 0.05f, EnemySwatch.Crystal);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.1f), b =>
            {
                b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.12f), EnemySwatch.Black);
                b.Blob(new Vector3(0f, -0.01f, 0.12f), new Vector3(0.06f, 0.05f, 0.06f), EnemySwatch.GolemStoneDark, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.2f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, -1f, EnemySwatch.IronDark, EnemySwatch.Black, EnemySwatch.GolemStoneDark, 0.8f), Quaternion.Euler(0f, 0f, -14f), meshKey: "StoneJackdawWing_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.2f, 0.1f, 0f),
                b => RangedEnemyRecipes.DrawBroadWing(b, 1f, EnemySwatch.IronDark, EnemySwatch.Black, EnemySwatch.GolemStoneDark, 0.8f), Quaternion.Euler(0f, 0f, 14f), meshKey: "StoneJackdawWing_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.34f), b => RangedEnemyRecipes.DrawFanTail(b, EnemySwatch.Black, EnemySwatch.IronDark), Quaternion.Euler(12f, 0f, 0f));
        }

        private static void DrawStoneJackdawBody(EnemyMeshBuilder b)
        {
            // Jackdaws nest in the drystone walls and take the loose chips to line the nest. These take the walls, one stone at a time, and throw them.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.5f, 0.72f), EnemySwatch.IronDark);
            b.Blob(new Vector3(0f, -0.1f, 0.12f), new Vector3(0.38f, 0.3f, 0.4f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Box(new Vector3(0.14f, 0.16f, -0.1f), new Vector3(0.12f, 0.06f, 0.1f), EnemySwatch.GolemStone, Quaternion.Euler(10f, 30f, 0f));
            b.Box(new Vector3(0.13f, 0.19f, -0.1f), new Vector3(0.03f, 0.03f, 0.08f), EnemySwatch.GolemCrack, Quaternion.Euler(10f, 30f, 0f));
            DrawBirdLegs(b, 0.58f, 0.09f, EnemySwatch.Black, EnemySwatch.IronDark);
        }

        // ------------------------------------------------------------------
        // Ubır Jovem
        // ------------------------------------------------------------------

        public static void BuildYoungUbir(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.66f, 0f), DrawYoungUbirBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.34f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.36f, 0.3f, 0.2f), EnemySwatch.BatFurDark);
                HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.06f, 0.08f, 0.06f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.06f, 0.08f), b =>
            {
                b.Box(new Vector3(0f, -0.02f, 0.04f), new Vector3(0.18f, 0.04f, 0.08f), EnemySwatch.Black);
                b.Box(new Vector3(0f, 0.005f, 0.05f), new Vector3(0.12f, 0.012f, 0.05f), EnemySwatch.Ember);
                b.Cylinder(new Vector3(0.04f, 0.01f, 0.07f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Fang);
                b.Cylinder(new Vector3(-0.04f, 0.01f, 0.07f), 0.01f, 0f, 0.03f, 3, EnemySwatch.Fang);
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.32f, 0.04f, 0.08f), b => DrawYoungUbirClaw(b, -1f), Quaternion.Euler(0f, 0f, -16f), meshKey: "YoungUbirClaw_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.32f, 0.04f, 0.08f), b => DrawYoungUbirClaw(b, 1f), Quaternion.Euler(0f, 0f, 16f), meshKey: "YoungUbirClaw_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.2f, -0.3f), b =>
            {
                // The trail of sparks it drags, the way the old ones fly about at night like a ball of fire.
                b.Segment(Vector3.zero, new Vector3(0f, -0.22f, -0.3f), 0.08f, 0.03f, 5, EnemySwatch.BatFurDark);
                for (int i = 0; i < 3; i++)
                {
                    b.Blob(new Vector3(0.04f * (i - 1), -0.12f - i * 0.1f, -0.16f - i * 0.12f), new Vector3(0.06f, 0.06f, 0.06f), EnemySwatch.Ember, coarse: true);
                }
            });
        }

        private static void DrawYoungUbirClaw(EnemyMeshBuilder b, float side)
        {
            b.Segment(Vector3.zero, new Vector3(0.2f * side, -0.06f, 0.06f), 0.05f, 0.035f, 5, EnemySwatch.BatFurDark);
            for (int k = -1; k <= 1; k++)
            {
                b.Cylinder(new Vector3(0.22f * side, -0.07f, 0.06f + k * 0.03f), 0.012f, 0f, 0.08f, 3, EnemySwatch.Bone, rotation: Quaternion.Euler(80f, 0f, 0f));
            }
        }

        private static void DrawYoungUbirBody(EnemyMeshBuilder b)
        {
            // A young ubır has not learned to hide in a person yet. It goes about the halls as a ball of black hair, hungry, quick, and very sure of itself.
            b.Blob(Vector3.zero, new Vector3(0.78f, 0.72f, 0.74f), EnemySwatch.BatFurDark);
            b.Blob(new Vector3(0f, 0.08f, -0.06f), new Vector3(0.62f, 0.5f, 0.6f), EnemySwatch.Shadow, coarse: true);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(a), 0.35f, Mathf.Sin(a) * 0.9f).normalized;
                b.Segment(dir * 0.3f, dir * 0.5f, 0.06f, 0.008f, 4, EnemySwatch.BatFur);
            }

            // The wisp of hair it hangs from: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.3f, 0f), new Vector3(0.02f, -0.62f, -0.04f), 0.08f, 0.02f, 5, EnemySwatch.Shadow);
            b.Blob(new Vector3(0.02f, -0.62f, -0.04f), new Vector3(0.06f, 0.05f, 0.06f), EnemySwatch.Ember, coarse: true);
        }
    }
}
