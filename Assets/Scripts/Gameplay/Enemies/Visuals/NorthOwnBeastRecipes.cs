using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-first pass of beasts (bible §3.2.1): the most-seen roles of Região 10, the Picos de Farnkesh (Docs/Mapas100.md 57–58),
    /// Região 11, the Mar Gelado (60–64), and Região 12, the Arquipélago de Hinokura (68–70), that the continuous world still played with
    /// another beast, low-poly from the shared palette like <see cref="HighlandOwnBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Pedra Voadora</b> (57, ~1.2 m): a scree stone held up by the ill wind in a ring of grey dust. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Ays da Garganta</b> (57, ~1.4 m): a spirit of the ill wind seen only by the ash-grey dust it spins. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Raio Preso</b> (58, ~1.6 m): a bolt of lightning caged in a knot of cloud. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Peixe-Gelo</b> (60, ~0.8 m): a knife-thin white fish of the canals with frost on its back. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, fins, <c>Tail</c>.</item>
    /// <item><b>Enguia-Lanterna</b> (60, ~1 m): a long eel rearing out of a crack, a row of dimmed lights down its side. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Mosquito da Tundra</b> (61, ~0.8 m): a big grey mosquito of Omöl's lakes with a bead of stolen light in its belly. <c>Body</c>, wings, <c>Stinger</c>.</item>
    /// <item><b>Narval Apagado</b> (63, ~0.8 m, 2.6 m with the tusk): a narwhal hauled out on the floe, its twisted tusk gone dull. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four flippers.</item>
    /// <item><b>Gaivota-Tempestade</b> (63, ~1 m): a stiff-winged gull of the gales with grey in its feathers. Bird skeleton.</item>
    /// <item><b>Peixe-Bruxa</b> (64, ~0.8 m): an eyeless hagfish tied in a loose knot, in a cloud of slime. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>.</item>
    /// <item><b>Polvo de Obsidiana</b> (68, ~0.9 m): an octopus the colour of cold glass, its arms spread on the reef. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Faísca Raivosa</b> (69, ~1 m): a furnace spark held down by the shadow until it is angry, trailing cinders to the ground. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Cinza Viva</b> (70, ~0.9 m): a heap of warm ash with two coals for eyes. <c>Body</c> → <c>StolenLight</c>.</item>
    /// </list>
    /// </summary>
    public static class NorthOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Pedra Voadora
        // ------------------------------------------------------------------

        public static void BuildFlyingStone(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawFlyingStoneBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.02f, 0.4f), b =>
            {
                // The spark of the ill wind caught in its heart, showing through the crack; it flares before the stone dives.
                b.Blob(Vector3.zero, new Vector3(0.14f, 0.18f, 0.08f), EnemySwatch.GolemCrack);
                b.Blob(new Vector3(0f, 0.02f, 0.02f), new Vector3(0.06f, 0.08f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawFlyingStoneBody(EnemyMeshBuilder b)
        {
            // The destroying wind of the gorge lifts the loose stones off the scree and throws them like hail. Most fall back and stay; the ones the shadow got into stay up, spinning in a ring of grey dust.
            b.Blob(Vector3.zero, new Vector3(0.86f, 0.66f, 0.8f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0.14f, 0.12f, -0.1f), new Vector3(0.5f, 0.4f, 0.48f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(-0.18f, -0.08f, 0.1f), new Vector3(0.42f, 0.36f, 0.42f), EnemySwatch.GolemStoneDark, coarse: true);
            // The sharp scree edges it broke off with, and the crack down its face where the wind got in.
            b.Box(new Vector3(0f, 0.04f, 0.38f), new Vector3(0.05f, 0.46f, 0.04f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 0f, 14f));
            b.Cylinder(new Vector3(0.36f, 0.12f, 0f), 0.08f, 0f, 0.18f, 4, EnemySwatch.GolemStoneDark, rotation: Quaternion.Euler(0f, 0f, -70f));
            b.Cylinder(new Vector3(-0.36f, 0.06f, -0.04f), 0.08f, 0f, 0.16f, 4, EnemySwatch.GolemStoneDark, rotation: Quaternion.Euler(0f, 0f, 70f));
            HinokuraBeastRecipes.AddEyes(b, 0.15f, 0.12f, 0.36f, 0.06f, EnemySwatch.MoonEye);
            // The ring of grey dust it spins in, and the dust devil under it that holds it up: the lowest points of the model.
            for (int i = 0; i < 8; i++)
            {
                float a = (i * 45f + 10f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.5f, -0.3f + (i % 2) * 0.06f, Mathf.Sin(a) * 0.46f), new Vector3(0.2f, 0.1f, 0.2f),
                    i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.IvoryDark, coarse: true);
            }

            b.Segment(new Vector3(0f, -0.3f, 0f), new Vector3(0.02f, -0.64f, -0.02f), 0.22f, 0.08f, 6, EnemySwatch.Shroud);
            b.Blob(new Vector3(0.02f, -0.66f, -0.02f), new Vector3(0.36f, 0.1f, 0.34f), EnemySwatch.IvoryDark, coarse: true);
        }

        // ------------------------------------------------------------------
        // Ays da Garganta
        // ------------------------------------------------------------------

        public static void BuildGorgeAys(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.96f, 0f), DrawGorgeAysBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.16f, 0.26f), b =>
            {
                // The one eye the dust shows when it turns to look, a cold glint of the light the wind blew out; it flares before the gust.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.12f, 0.08f), EnemySwatch.Shadow);
                b.Blob(new Vector3(0f, 0f, 0.03f), new Vector3(0.1f, 0.07f, 0.04f), EnemySwatch.MoonEye, coarse: true);
            });
        }

        private static void DrawGorgeAysBody(EnemyMeshBuilder b)
        {
            // The Ays are the spirits of the ill wind; you only see them by the dust they carry. Since the shadow came the dust is grey and full of ash, and it spins in a funnel that runs with the gusts.
            b.Blob(new Vector3(0f, 0.18f, 0f), new Vector3(0.7f, 0.36f, 0.64f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0.12f, 0.26f, -0.06f), new Vector3(0.42f, 0.24f, 0.4f), EnemySwatch.IvoryDark, coarse: true);
            // The funnel, ring under ring, narrower to the ground, each ring twisted a little further round.
            for (int i = 0; i < 5; i++)
            {
                float y = 0.02f - i * 0.18f;
                float r = 0.3f - i * 0.05f;
                b.Cylinder(new Vector3(0.03f * (i % 2 == 0 ? 1f : -1f), y - 0.12f, 0f), r * 0.8f, r, 0.14f, 7,
                    i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.IvoryDark, twist: 20f + i * 12f);
            }

            // The tip of the funnel, scraping the scree: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.8f, 0f), new Vector3(0.02f, -0.94f, 0.02f), 0.08f, 0.04f, 5, EnemySwatch.Shroud);
            // The ash and the bits of grass it carries round, and the dark it spins round, showing through the front.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 25f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 0.36f, 0.12f - i * 0.1f, Mathf.Sin(a) * 0.32f), new Vector3(0.06f, 0.02f, 0.1f),
                    i % 3 == 0 ? EnemySwatch.Lichen : EnemySwatch.Black, Quaternion.Euler(0f, i * 50f, 30f));
            }

            b.Blob(new Vector3(0f, 0.1f, 0.18f), new Vector3(0.3f, 0.2f, 0.14f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Raio Preso
        // ------------------------------------------------------------------

        public static void BuildTrappedLightning(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.14f, 0f), DrawTrappedLightningBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.02f, 0.2f), b =>
            {
                // The heart of the bolt, held in the knot of cloud; it swells before the lightning throws itself.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.12f), EnemySwatch.Crystal);
                b.Blob(new Vector3(0f, 0f, 0.03f), new Vector3(0.1f, 0.1f, 0.06f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawTrappedLightningBody(EnemyMeshBuilder b)
        {
            // Over the cloud isles the storms leave little bolts behind, and the huma gathers them into her nest to keep the eggs warm. The shadow caught some in cages of cloud before they reached her.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.42f, 0.56f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0.24f, 0.06f, -0.04f), new Vector3(0.38f, 0.32f, 0.36f), EnemySwatch.Shroud, coarse: true);
            b.Blob(new Vector3(-0.24f, 0.04f, 0.02f), new Vector3(0.4f, 0.34f, 0.38f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(0f, 0.18f, -0.06f), new Vector3(0.36f, 0.24f, 0.32f), EnemySwatch.Shroud, coarse: true);
            // The cage: dark bars of the shadow's cloud bent round it.
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 45f) * Mathf.Deg2Rad;
                var top = new Vector3(Mathf.Cos(a) * 0.12f, 0.22f, Mathf.Sin(a) * 0.1f);
                var mid = new Vector3(Mathf.Cos(a) * 0.36f, 0f, Mathf.Sin(a) * 0.3f);
                var low = new Vector3(Mathf.Cos(a) * 0.14f, -0.24f, Mathf.Sin(a) * 0.12f);
                b.Segment(top, mid, 0.025f, 0.025f, 4, EnemySwatch.Shadow);
                b.Segment(mid, low, 0.025f, 0.02f, 4, EnemySwatch.Shadow);
            }

            // The bolt hanging out of the cage in zigzags down to the ground, where it scorched the stone: the lowest point.
            var points = new[]
            {
                new Vector3(0f, -0.2f, 0.06f), new Vector3(0.14f, -0.42f, 0.08f), new Vector3(-0.06f, -0.6f, 0.04f),
                new Vector3(0.12f, -0.84f, 0.06f), new Vector3(-0.02f, -1.1f, 0.04f),
            };
            for (int i = 0; i < points.Length - 1; i++)
            {
                b.Segment(points[i], points[i + 1], 0.04f - i * 0.006f, 0.034f - i * 0.006f, 4, i % 2 == 0 ? EnemySwatch.StolenLight : EnemySwatch.Crystal);
            }

            b.Blob(new Vector3(-0.02f, -1.1f, 0.04f), new Vector3(0.2f, 0.06f, 0.2f), EnemySwatch.Black, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.06f, 0.26f, 0.05f, EnemySwatch.Crystal);
        }

        // ------------------------------------------------------------------
        // Peixe-Gelo
        // ------------------------------------------------------------------

        public static void BuildIceFish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawIceFishBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.4f), DrawIceFishHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.04f), b =>
            {
                // The thin lower jaw, a row of small clear teeth.
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.07f, 0.025f, 0.16f), EnemySwatch.Ivory);
                for (int i = 0; i < 3; i++)
                {
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.025f * s, 0.004f, 0.04f + i * 0.04f), 0.006f, 0f, 0.022f, 3, EnemySwatch.Crystal);
                    }
                }
            });
            bp.Mesh(ProceduralEnemyParts.WingLeft, body, new Vector3(-0.07f, -0.06f, 0.2f), b => DrawIceFishFin(b, -1f), Quaternion.Euler(0f, 0f, -24f), meshKey: "IceFishFin_L");
            bp.Mesh(ProceduralEnemyParts.WingRight, body, new Vector3(0.07f, -0.06f, 0.2f), b => DrawIceFishFin(b, 1f), Quaternion.Euler(0f, 0f, 24f), meshKey: "IceFishFin_R");
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.42f), DrawIceFishTail);
        }

        private static void DrawIceFishBody(EnemyMeshBuilder b)
        {
            // The ice fish of the canals are thin as a knife and white as the ice; the boatmen say they swim through the floes without breaking them. The shadow grew a frost on their backs.
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.32f, 0.9f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, -0.06f, 0.04f), new Vector3(0.14f, 0.16f, 0.74f), EnemySwatch.GolemSnow, coarse: true);
            // The see-through flank with its little bones showing, and the frost crystals of the shadow along the back.
            for (int i = 0; i < 6; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.085f * s, 0.02f, -0.26f + i * 0.1f), new Vector3(0.01f, 0.16f, 0.015f), EnemySwatch.IvoryDark, Quaternion.Euler(-18f, 0f, 0f));
                }
            }

            for (int i = 0; i < 5; i++)
            {
                b.Cylinder(new Vector3(0f, 0.14f, -0.22f + i * 0.1f), 0.035f, 0f, 0.1f + (i % 2) * 0.05f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(-14f, 0f, 0f));
            }

            b.Blob(new Vector3(0f, 0.12f, 0.08f), new Vector3(0.1f, 0.05f, 0.3f), EnemySwatch.Shadow, coarse: true);
            // The long belly fins it rests on, clear as ice: the lowest points as it swims low over the canals.
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.04f * s, -0.14f, 0.1f), new Vector3(0.08f * s, -0.4f, -0.04f), new Vector3(0.04f * s, -0.14f, -0.16f), EnemySwatch.Crystal, EnemySwatch.Ivory);
            }

            b.TwoSided(new Vector3(0f, -0.14f, -0.18f), new Vector3(0f, -0.14f, -0.36f), new Vector3(0f, -0.38f, -0.32f), EnemySwatch.Ivory, EnemySwatch.IvoryDark);
        }

        private static void DrawIceFishHead(EnemyMeshBuilder b)
        {
            // The narrow pointed head and the big pale eyes of a fish of the dark water under the ice.
            b.Blob(Vector3.zero, new Vector3(0.16f, 0.22f, 0.22f), EnemySwatch.Ivory);
            b.Box(new Vector3(0f, 0f, 0.12f), new Vector3(0.08f, 0.06f, 0.16f), EnemySwatch.Ivory);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.04f, 0.04f), new Vector3(0.05f, 0.05f, 0.04f), EnemySwatch.MoonEye, coarse: true);
                b.Blob(new Vector3(0.068f * s, 0.04f, 0.05f), new Vector3(0.02f, 0.02f, 0.015f), EnemySwatch.Black, coarse: true);
            }
        }

        private static void DrawIceFishFin(EnemyMeshBuilder b, float side)
        {
            b.Matrix = Matrix4x4.Scale(new Vector3(side, 1f, 1f));
            b.TwoSided(new Vector3(0f, 0f, 0.04f), new Vector3(0.24f, -0.02f, -0.06f), new Vector3(0.16f, -0.02f, -0.16f), EnemySwatch.Crystal, EnemySwatch.Ivory);
            b.TwoSided(new Vector3(0f, 0f, 0.04f), new Vector3(0.16f, -0.02f, -0.16f), new Vector3(0f, 0f, -0.1f), EnemySwatch.Crystal, EnemySwatch.Ivory);
            b.Matrix = Matrix4x4.identity;
        }

        private static void DrawIceFishTail(EnemyMeshBuilder b)
        {
            b.Segment(new Vector3(0f, 0f, 0.06f), new Vector3(0f, 0f, -0.12f), 0.06f, 0.03f, 5, EnemySwatch.Ivory);
            b.TwoSided(new Vector3(0f, 0f, -0.1f), new Vector3(0f, 0.2f, -0.3f), new Vector3(0f, 0f, -0.22f), EnemySwatch.Crystal, EnemySwatch.Ivory);
            b.TwoSided(new Vector3(0f, 0f, -0.1f), new Vector3(0f, 0f, -0.22f), new Vector3(0f, -0.2f, -0.3f), EnemySwatch.Crystal, EnemySwatch.Ivory);
        }

        // ------------------------------------------------------------------
        // Enguia-Lanterna
        // ------------------------------------------------------------------

        public static void BuildLanternEel(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.2f, 0f), DrawLanternEelBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.72f, 0.3f), DrawLanternEelHead, Quaternion.Euler(14f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.05f, 0.06f), b =>
            {
                b.Box(new Vector3(0f, -0.01f, 0.08f), new Vector3(0.1f, 0.03f, 0.16f), EnemySwatch.CaveBelly);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.035f * s, 0.005f, 0.13f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.1f, -0.36f), DrawLanternEelTail);
        }

        private static void DrawLanternEelBody(EnemyMeshBuilder b)
        {
            // In the black water under the canals live the lantern eels, long and slick, with a row of little lights down their sides that the boatmen steered by on the polar nights. The shadow dimmed the lights to a sickly glow.
            // The crack in the ice it comes up through, black water and broken edges: the lowest points of the model.
            b.Blob(new Vector3(0f, -0.17f, 0f), new Vector3(0.74f, 0.06f, 0.64f), EnemySwatch.Black, coarse: true);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 15f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 0.38f, -0.15f, Mathf.Sin(a) * 0.34f), new Vector3(0.2f, 0.06f, 0.14f), EnemySwatch.GolemSnow, Quaternion.Euler(0f, i * 60f, 8f));
            }

            // The body rising out of the crack in a slow S, slick and dark, the dimmed lights down each side.
            var spine = new[]
            {
                new Vector3(0f, -0.16f, -0.06f), new Vector3(0.06f, 0.1f, -0.02f), new Vector3(0.02f, 0.32f, 0.08f),
                new Vector3(-0.04f, 0.5f, 0.2f), new Vector3(0f, 0.66f, 0.28f),
            };
            for (int i = 0; i < spine.Length - 1; i++)
            {
                b.Segment(spine[i], spine[i + 1], 0.12f - i * 0.012f, 0.11f - i * 0.012f, 7, EnemySwatch.CaveSkinDark);
                Vector3 at = Vector3.Lerp(spine[i], spine[i + 1], 0.5f);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(at + new Vector3(0.1f * s, 0f, 0f), new Vector3(0.035f, 0.035f, 0.035f), i % 2 == 0 ? EnemySwatch.SoftEye : EnemySwatch.Crystal, coarse: true);
                }
            }

            // The pale belly and the low fin along its back, and the grey of the shadow over the lights near the top.
            b.Segment(new Vector3(0f, 0.06f, 0.06f), new Vector3(0f, 0.5f, 0.28f), 0.06f, 0.05f, 5, EnemySwatch.CaveBelly);
            b.TwoSided(new Vector3(-0.02f, 0.1f, -0.1f), new Vector3(-0.04f, 0.5f, 0.06f), new Vector3(-0.1f, 0.32f, -0.08f), EnemySwatch.CaveSkin, EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0f, 0.44f, 0.12f), new Vector3(0.18f, 0.12f, 0.16f), EnemySwatch.Shadow, coarse: true);
        }

        private static void DrawLanternEelHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.18f, 0.16f, 0.26f), EnemySwatch.CaveSkinDark);
            b.Box(new Vector3(0f, 0.01f, 0.12f), new Vector3(0.12f, 0.06f, 0.14f), EnemySwatch.CaveSkin);
            // The first light of the row, on its brow, still the brightest; and the small mean eyes.
            b.Blob(new Vector3(0f, 0.08f, 0.02f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.StolenLight, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.04f, 0.08f, 0.035f, EnemySwatch.SoftEye);
        }

        private static void DrawLanternEelTail(EnemyMeshBuilder b)
        {
            // The end of the tail lying out over the ice behind the crack, flicking.
            b.Segment(Vector3.zero, new Vector3(0.1f, -0.04f, -0.22f), 0.08f, 0.05f, 6, EnemySwatch.CaveSkinDark);
            b.Segment(new Vector3(0.1f, -0.04f, -0.22f), new Vector3(0.02f, -0.06f, -0.42f), 0.05f, 0.015f, 5, EnemySwatch.CaveSkinDark);
            b.Blob(new Vector3(0.08f, -0.02f, -0.24f), new Vector3(0.03f, 0.03f, 0.03f), EnemySwatch.Crystal, coarse: true);
        }

        // ------------------------------------------------------------------
        // Mosquito da Tundra
        // ------------------------------------------------------------------

        public static void BuildTundraMosquito(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.58f, 0f), DrawTundraMosquitoBody);
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bp.Mesh(s < 0 ? ProceduralEnemyParts.WingLeft : ProceduralEnemyParts.WingRight, body, new Vector3(0.08f * side, 0.12f, -0.02f),
                    b => DrawTundraMosquitoWing(b, side), Quaternion.Euler(0f, 0f, 16f * side), meshKey: s < 0 ? "TundraMosquitoWing_L" : "TundraMosquitoWing_R");
            }

            bp.Mesh(ProceduralEnemyParts.Stinger, body, new Vector3(0f, 0f, 0.3f), b =>
            {
                // The long proboscis, pointing forward and down; it swings up on the windup.
                b.Cylinder(Vector3.zero, 0.022f, 0.004f, 0.32f, 4, EnemySwatch.Black, rotation: Quaternion.Euler(108f, 0f, 0f));
            });
        }

        private static void DrawTundraMosquitoBody(EnemyMeshBuilder b)
        {
            // In the short summer of the tundra Omöl's lakes breed mosquitoes by the cloud, his way of reminding everyone he is still there. The shadow got into a few swarms and they grew big and grey, with a bead of stolen light in each belly.
            b.Blob(Vector3.zero, new Vector3(0.24f, 0.22f, 0.26f), EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0f, 0.02f, 0.2f), new Vector3(0.16f, 0.16f, 0.14f), EnemySwatch.WolfFur);
            // The banded grey belly, swollen round the bead of stolen light.
            b.Blob(new Vector3(0f, 0.04f, -0.34f), new Vector3(0.22f, 0.2f, 0.56f), EnemySwatch.WolfFur);
            b.Blob(new Vector3(0f, 0.02f, -0.36f), new Vector3(0.12f, 0.12f, 0.2f), EnemySwatch.StolenLight, coarse: true);
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0f, 0.04f, -0.16f - i * 0.11f), new Vector3(0.23f - i * 0.02f, 0.21f - i * 0.02f, 0.04f), EnemySwatch.WolfFurDark, coarse: true);
            }

            // The feathery antennae of the lake mosquitoes, and the eyes.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.06f * s, 0.05f, 0.24f), new Vector3(0.07f, 0.08f, 0.07f), EnemySwatch.Ember, coarse: true);
                b.Segment(new Vector3(0.03f * s, 0.08f, 0.26f), new Vector3(0.14f * s, 0.24f, 0.32f), 0.014f, 0.008f, 3, EnemySwatch.IvoryDark);
                b.Box(new Vector3(0.1f * s, 0.18f, 0.3f), new Vector3(0.08f, 0.01f, 0.04f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 0f, 40f * s));
                // Long thin legs hanging under the hover: the lowest point of the model.
                for (int k = 0; k < 3; k++)
                {
                    float z = 0.06f - k * 0.09f;
                    b.Segment(new Vector3(0.06f * s, -0.08f, z), new Vector3(0.22f * s, -0.3f, z - 0.02f), 0.016f, 0.012f, 4, EnemySwatch.WolfFurDark);
                    b.Segment(new Vector3(0.22f * s, -0.3f, z - 0.02f), new Vector3(0.2f * s, -0.56f, z - 0.05f), 0.012f, 0.006f, 3, EnemySwatch.WolfFur);
                }
            }
        }

        private static void DrawTundraMosquitoWing(EnemyMeshBuilder b, float side)
        {
            b.Box(new Vector3(0.28f * side, 0.02f, -0.06f), new Vector3(0.56f, 0.012f, 0.16f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 24f * side, 0f));
            b.Box(new Vector3(0.2f * side, 0.01f, -0.18f), new Vector3(0.36f, 0.01f, 0.1f), EnemySwatch.Shroud, Quaternion.Euler(0f, 40f * side, 0f));
        }

        // ------------------------------------------------------------------
        // Narval Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedNarwhal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.38f, 0f), DrawDimmedNarwhalBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.08f, 0.64f), DrawDimmedNarwhalHead, Quaternion.Euler(-4f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.08f, 0.08f), b =>
                b.Box(new Vector3(0f, -0.015f, 0.08f), new Vector3(0.16f, 0.04f, 0.16f), EnemySwatch.GolemSnow));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, -0.04f, -0.7f), DrawDimmedNarwhalFlukes);
            for (int side = -1; side <= 1; side += 2)
            {
                string x = side < 0 ? "L" : "R";
                CoastDeltaBeastRecipes.AddLimb(bp, "F" + x, new Vector3(0.26f * side, 0.26f, 0.34f), new Vector3(0f, -0.12f, 0.02f),
                    new Vector3(0f, -0.11f, 0.04f), 0.05f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.14f, 0.05f, 0.18f), "DimmedNarwhalFront");
                CoastDeltaBeastRecipes.AddLimb(bp, "H" + x, new Vector3(0.2f * side, 0.26f, -0.4f), new Vector3(0f, -0.12f, -0.02f),
                    new Vector3(0f, -0.11f, -0.04f), 0.045f, EnemySwatch.GolemStone, EnemySwatch.GolemStoneDark, new Vector3(0.12f, 0.05f, 0.16f), "DimmedNarwhalHind");
            }
        }

        private static void DrawDimmedNarwhalBody(EnemyMeshBuilder b)
        {
            // The narwhals of the Aurora Sea carry the light of the auroras down under the ice for the fish on their tusks. The shadow dimmed the tusks, and they come up between the floes to charge whoever stands on the edge.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.6f, 1.4f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.5f, 0.32f, 1.18f), EnemySwatch.GolemSnow, coarse: true);
            // The grey mottling of an old narwhal, gone to soot where the shadow touched it, and the low ridge in place of a back fin.
            for (int i = 0; i < 7; i++)
            {
                float x = i % 2 == 0 ? 0.14f : -0.13f;
                b.Blob(new Vector3(x, 0.18f, -0.46f + i * 0.15f), new Vector3(0.1f, 0.04f, 0.1f), i % 3 == 0 ? EnemySwatch.Shadow : EnemySwatch.GolemStoneDark, coarse: true);
            }

            b.Box(new Vector3(0f, 0.25f, -0.2f), new Vector3(0.05f, 0.04f, 0.5f), EnemySwatch.GolemStoneDark);
            b.Segment(new Vector3(0f, 0.02f, 0.5f), new Vector3(0f, 0.08f, 0.64f), 0.18f, 0.15f, 6, EnemySwatch.GolemStone);
            b.Segment(new Vector3(0f, -0.02f, -0.52f), new Vector3(0f, -0.04f, -0.7f), 0.16f, 0.09f, 6, EnemySwatch.GolemStone);
        }

        private static void DrawDimmedNarwhalHead(EnemyMeshBuilder b)
        {
            // The round melon of the head, the small eyes, and the long twisted tusk that grows out of the lip, gone dull with one last glint at the tip.
            b.Blob(Vector3.zero, new Vector3(0.3f, 0.28f, 0.32f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.06f, 0.06f), new Vector3(0.24f, 0.18f, 0.22f), EnemySwatch.GolemStoneDark, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.11f, 0.02f, 0.08f, 0.04f, EnemySwatch.MoonEye);
            Vector3 prev = new Vector3(0.04f, -0.02f, 0.14f);
            for (int i = 1; i <= 6; i++)
            {
                var next = new Vector3(0.04f + Mathf.Sin(i * 1.4f) * 0.012f, -0.02f + i * 0.012f, 0.14f + i * 0.13f);
                b.Segment(prev, next, 0.04f - i * 0.005f, 0.035f - i * 0.005f, 5, i % 2 == 0 ? EnemySwatch.Bone : EnemySwatch.IvoryDark);
                prev = next;
            }

            b.Blob(prev, new Vector3(0.03f, 0.03f, 0.05f), EnemySwatch.Crystal, coarse: true);
        }

        private static void DrawDimmedNarwhalFlukes(EnemyMeshBuilder b)
        {
            // The broad tail flukes lying flat on the ice.
            b.Segment(Vector3.zero, new Vector3(0f, -0.04f, -0.14f), 0.09f, 0.05f, 5, EnemySwatch.GolemStone);
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.02f * s, -0.05f, -0.12f), new Vector3(0.24f * s, -0.08f, -0.3f), new Vector3(0.04f * s, -0.07f, -0.28f), EnemySwatch.GolemStoneDark, EnemySwatch.GolemStone);
            }
        }

        // ------------------------------------------------------------------
        // Gaivota-Tempestade
        // ------------------------------------------------------------------

        public static void BuildStormGull(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "StormGull", 0.62f, DrawStormGullBody, new Vector3(0f, 0.32f, 0.32f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.26f), EnemySwatch.Ivory);
                    // The dark storm hood of the gale gulls, and the heavy hooked bill with its red spot.
                    b.Blob(new Vector3(0f, 0.06f, -0.03f), new Vector3(0.2f, 0.12f, 0.2f), EnemySwatch.GolemStoneDark, coarse: true);
                    b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.04f, 0.014f, 0.14f, 5, EnemySwatch.Gold, rotation: Quaternion.Euler(96f, 0f, 0f));
                    b.Blob(new Vector3(0f, -0.04f, 0.19f), new Vector3(0.025f, 0.025f, 0.025f), EnemySwatch.WarCape, coarse: true);
                    HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.03f, 0.07f, 0.045f, EnemySwatch.MoonEye);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.05f, 0.02f, 0.12f), EnemySwatch.Gold),
                EnemySwatch.GolemStone, EnemySwatch.Ivory, EnemySwatch.Black, 1.25f, EnemySwatch.Ivory, EnemySwatch.GolemStone);
        }

        private static void DrawStormGullBody(EnemyMeshBuilder b)
        {
            // The storm gulls ride the gales of the Aurora Sea on stiff wings, and the fishermen of the Lua Branca read the weather by how high they fly. The shadow put a grey in their feathers, and they bring the storm now instead of warning of it.
            b.Blob(Vector3.zero, new Vector3(0.46f, 0.44f, 0.72f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, 0.12f, -0.06f), new Vector3(0.4f, 0.2f, 0.56f), EnemySwatch.GolemStone, coarse: true);
            b.Segment(new Vector3(0f, 0.1f, 0.22f), new Vector3(0f, 0.3f, 0.3f), 0.08f, 0.07f, 6, EnemySwatch.Ivory);
            // The soot of the shadow over the grey mantle, and the frost of the gale on the wingtips folded over the tail.
            b.Blob(new Vector3(-0.06f, 0.22f, -0.1f), new Vector3(0.24f, 0.05f, 0.3f), EnemySwatch.Shadow, coarse: true);
            b.Box(new Vector3(0f, 0.08f, -0.36f), new Vector3(0.28f, 0.05f, 0.12f), EnemySwatch.Black);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.62f, 0.1f, EnemySwatch.Gold, EnemySwatch.Gold);
        }

        // ------------------------------------------------------------------
        // Peixe-Bruxa
        // ------------------------------------------------------------------

        public static void BuildWitchFish(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawWitchFishBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.36f, 0.24f), DrawWitchFishHead, Quaternion.Euler(18f, 0f, 0f));
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.04f, 0.1f), b =>
            {
                // The round sucking mouth, its rasping tooth-plates.
                b.Cylinder(new Vector3(0f, -0.02f, 0.02f), 0.05f, 0.045f, 0.04f, 6, EnemySwatch.CaveBelly, rotation: Quaternion.Euler(90f, 0f, 0f), bottomCap: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.02f * s, -0.02f, 0.05f), 0.008f, 0f, 0.025f, 3, EnemySwatch.Fang, rotation: Quaternion.Euler(90f, 0f, 0f));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(-0.2f, -0.18f, -0.26f), DrawWitchFishTail);
        }

        private static void DrawWitchFishBody(EnemyMeshBuilder b)
        {
            // At the bottom of Old Kul's lake live the witch fish, eyeless and slow, that tidy the lake by eating what falls to it. They have no bones to speak of and tie themselves in knots to get loose. Since the Kul went grumpy they come up in a cloud of slime.
            // The cloud of slime on the ice round it, glinting where it caught the light of the holes: the lowest points of the model.
            b.Blob(new Vector3(0f, -0.26f, 0f), new Vector3(0.9f, 0.08f, 0.8f), EnemySwatch.CaveMoss, coarse: true);
            b.Blob(new Vector3(0.16f, -0.22f, -0.12f), new Vector3(0.4f, 0.1f, 0.36f), EnemySwatch.Shroud, coarse: true);
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 30f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.32f, -0.2f, Mathf.Sin(a) * 0.28f), new Vector3(0.05f, 0.04f, 0.05f), EnemySwatch.Crystal, coarse: true);
            }

            // The loose knot: the long pink-grey body looped once round itself and rising to the head.
            const int steps = 9;
            Vector3 prev = new Vector3(0.24f, -0.12f, -0.04f);
            for (int i = 1; i <= steps; i++)
            {
                float a = i * (360f / steps) * Mathf.Deg2Rad;
                var next = new Vector3(Mathf.Cos(a) * 0.24f, -0.12f + Mathf.Sin(a * 0.5f) * 0.14f, Mathf.Sin(a) * 0.2f);
                b.Segment(prev, next, 0.075f, 0.075f, 6, i % 2 == 0 ? EnemySwatch.CaveSkin : EnemySwatch.Pink);
                prev = next;
            }

            b.Segment(prev, new Vector3(0f, 0.12f, 0.12f), 0.075f, 0.07f, 6, EnemySwatch.CaveSkin);
            b.Segment(new Vector3(0f, 0.12f, 0.12f), new Vector3(0f, 0.32f, 0.22f), 0.07f, 0.065f, 6, EnemySwatch.Pink);
            // The slime pores down its flank and the soot of the shadow where it rubbed against the black ice.
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0.06f, 0.0f + i * 0.08f, 0.14f + i * 0.02f), new Vector3(0.025f, 0.025f, 0.025f), EnemySwatch.SoftEye, coarse: true);
            }

            b.Blob(new Vector3(-0.1f, -0.06f, 0.1f), new Vector3(0.16f, 0.08f, 0.14f), EnemySwatch.Shadow, coarse: true);
        }

        private static void DrawWitchFishHead(EnemyMeshBuilder b)
        {
            // No eyes, only the pale spots where eyes would be, and the barbels it feels the way with.
            b.Blob(Vector3.zero, new Vector3(0.16f, 0.14f, 0.2f), EnemySwatch.Pink);
            HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.04f, 0.04f, 0.03f, EnemySwatch.SoftEye);
            for (int s = -1; s <= 1; s += 2)
            {
                for (int k = 0; k < 2; k++)
                {
                    b.Segment(new Vector3(0.03f * s, -0.02f - k * 0.02f, 0.09f), new Vector3((0.07f + k * 0.03f) * s, -0.06f - k * 0.03f, 0.16f), 0.01f, 0.004f, 3, EnemySwatch.CaveBelly);
                }
            }
        }

        private static void DrawWitchFishTail(EnemyMeshBuilder b)
        {
            // The end of the knot, the flat tail fin lying out of the slime.
            b.Segment(Vector3.zero, new Vector3(-0.06f, 0f, -0.18f), 0.06f, 0.04f, 5, EnemySwatch.CaveSkin);
            b.TwoSided(new Vector3(-0.06f, 0f, -0.16f), new Vector3(-0.04f, 0.1f, -0.3f), new Vector3(-0.1f, -0.06f, -0.32f), EnemySwatch.Pink, EnemySwatch.CaveSkin);
        }

        // ------------------------------------------------------------------
        // Polvo de Obsidiana
        // ------------------------------------------------------------------

        public static void BuildObsidianOctopus(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.5f, 0f), DrawObsidianOctopusBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.3f), b =>
            {
                // The lure it hangs between its eyes, a glint of the reef's light it keeps to draw the fish in; it flares before the arms close.
                b.Blob(Vector3.zero, new Vector3(0.1f, 0.1f, 0.08f), EnemySwatch.Ember);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.05f, 0.05f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawObsidianOctopusBody(EnemyMeshBuilder b)
        {
            // In the warm channels of the obsidian reef the octopuses hide in the glass and change colour with it, black and red and gold; the smiths say they taught them the patterns of the lacquer. The shadow turned them the colour of cold glass.
            b.Blob(new Vector3(0f, 0.1f, -0.08f), new Vector3(0.5f, 0.56f, 0.56f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, 0.18f, -0.12f), new Vector3(0.36f, 0.34f, 0.4f), EnemySwatch.IronDark, coarse: true);
            b.Blob(new Vector3(0f, -0.14f, 0.04f), new Vector3(0.44f, 0.24f, 0.4f), EnemySwatch.Black, coarse: true);
            // The glassy sheen: thin facets of cold crystal over the mantle, where the colours used to move.
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 10f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Cos(a) * 0.2f, 0.22f, -0.08f + Mathf.Sin(a) * 0.2f), new Vector3(0.1f, 0.02f, 0.08f), EnemySwatch.Crystal, Quaternion.Euler(0f, i * 72f, 30f));
            }

            HinokuraBeastRecipes.AddEyes(b, 0.13f, -0.02f, 0.18f, 0.06f, EnemySwatch.Ember);
            // The eight arms spread over the reef, curling at the tips: the lowest points of the model.
            for (int i = 0; i < 8; i++)
            {
                float a = (i * 45f + 22.5f) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Sin(a), 0f, Mathf.Cos(a));
                var root = new Vector3(dir.x * 0.16f, -0.2f, dir.z * 0.16f + 0.02f);
                var mid = new Vector3(dir.x * 0.4f, -0.44f, dir.z * 0.38f);
                var tip = new Vector3(dir.x * 0.62f, -0.44f, dir.z * 0.58f);
                b.Segment(root, mid, 0.07f, 0.05f, 5, EnemySwatch.Black);
                b.Segment(mid, tip, 0.05f, 0.015f, 4, i % 2 == 0 ? EnemySwatch.IronDark : EnemySwatch.Black);
                b.Blob(tip + new Vector3(0f, 0.03f, 0f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Black, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Faísca Raivosa
        // ------------------------------------------------------------------

        public static void BuildRagingSpark(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.7f, 0f), DrawRagingSparkBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.06f), b =>
            {
                // The white-hot heart of the spark, the bit of the Fogo Vivo that wanted the sky; it flares before the swarm burns.
                b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.18f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.04f), new Vector3(0.1f, 0.1f, 0.08f), EnemySwatch.FireflyGlow, coarse: true);
            });
        }

        private static void DrawRagingSparkBody(EnemyMeshBuilder b)
        {
            // The nine furnaces of the castle throw sparks all night, and the smiths say every spark is a little of the Fogo Vivo that wants to go home to the sky. The shadow catches some before they rise, and they come back down angry.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.46f, 0.46f), EnemySwatch.Ember);
            b.Blob(new Vector3(0f, -0.02f, -0.04f), new Vector3(0.4f, 0.36f, 0.36f), EnemySwatch.WarCape, coarse: true);
            // The tongues of flame flicking out of it, and the frown of the shadow pressed into its face.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 30f) * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Cos(a) * 0.18f, 0.12f, Mathf.Sin(a) * 0.16f);
                b.Cylinder(at, 0.07f, 0f, 0.2f + (i % 2) * 0.08f, 4, i % 2 == 0 ? EnemySwatch.Ember : EnemySwatch.Gold, rotation: Quaternion.Euler(Mathf.Sin(a) * 35f, 0f, -Mathf.Cos(a) * 35f));
            }

            b.Box(new Vector3(0f, 0.1f, 0.22f), new Vector3(0.2f, 0.03f, 0.03f), EnemySwatch.Shadow, Quaternion.Euler(0f, 0f, 0f));
            HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.04f, 0.22f, 0.05f, EnemySwatch.Black);
            // The ring of the shadow's soot that holds it down, and the cinders it sheds, falling to the ground in a smoking trail: the lowest point.
            b.Cylinder(new Vector3(0f, -0.16f, 0f), 0.28f, 0.28f, 0.05f, 9, EnemySwatch.Shadow, rotation: Quaternion.Euler(8f, 0f, 0f));
            var trail = new[] { new Vector3(0f, -0.2f, -0.04f), new Vector3(0.08f, -0.38f, -0.1f), new Vector3(-0.04f, -0.54f, -0.08f), new Vector3(0.02f, -0.68f, -0.12f) };
            for (int i = 0; i < trail.Length - 1; i++)
            {
                b.Segment(trail[i], trail[i + 1], 0.09f - i * 0.02f, 0.07f - i * 0.02f, 5, EnemySwatch.Shroud);
                b.Blob(trail[i + 1] + new Vector3(0.06f, 0f, 0.02f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            }

            b.Blob(new Vector3(0.02f, -0.68f, -0.12f), new Vector3(0.3f, 0.04f, 0.26f), EnemySwatch.Black, coarse: true);
        }

        // ------------------------------------------------------------------
        // Cinza Viva
        // ------------------------------------------------------------------

        public static void BuildLivingAsh(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.42f, 0f), DrawLivingAshBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.18f, 0.34f), b =>
            {
                // The two coals for eyes, still hot from what the fire ate; they flare before the heap smothers.
                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0f, 0f, 0.08f, EnemySwatch.Ember);
                b.Blob(new Vector3(0f, -0.06f, 0f), new Vector3(0.08f, 0.04f, 0.04f), EnemySwatch.SoulFire, coarse: true);
            });
        }

        private static void DrawLivingAshBody(EnemyMeshBuilder b)
        {
            // In the caldera of the Devouring Fire the ash lies so deep that it moves when you step on it. Some of it moves when no one steps on it: the shadow breathed into the warm ash of what the fire ate, and it rises in heaps.
            b.Blob(new Vector3(0f, -0.12f, 0f), new Vector3(0.96f, 0.6f, 0.9f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, 0.16f, -0.04f), new Vector3(0.64f, 0.5f, 0.6f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(0.12f, 0.36f, -0.06f), new Vector3(0.36f, 0.28f, 0.34f), EnemySwatch.IvoryDark, coarse: true);
            // The ash spilling off its edge into the caldera floor, with the glow of embers buried in it: the lowest points of the model.
            for (int i = 0; i < 7; i++)
            {
                float a = (i * 51.4f + 12f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.46f, -0.36f, Mathf.Sin(a) * 0.42f), new Vector3(0.26f, 0.12f, 0.24f),
                    i % 2 == 0 ? EnemySwatch.GolemStoneDark : EnemySwatch.Shroud, coarse: true);
            }

            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 40f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.3f, -0.1f + i * 0.06f, Mathf.Sin(a) * 0.3f), new Vector3(0.06f, 0.04f, 0.06f), EnemySwatch.Ember, coarse: true);
            }

            // The charred stick of something the fire ate, still poking out of the heap, and the shadow's soot under the coals.
            b.Segment(new Vector3(-0.18f, 0.16f, -0.12f), new Vector3(-0.32f, 0.5f, -0.2f), 0.035f, 0.02f, 4, EnemySwatch.BarkDark);
            b.Blob(new Vector3(0f, 0.08f, 0.3f), new Vector3(0.3f, 0.12f, 0.1f), EnemySwatch.Shadow, coarse: true);
        }
    }
}
