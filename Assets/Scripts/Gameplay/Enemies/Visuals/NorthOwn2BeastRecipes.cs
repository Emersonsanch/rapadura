using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twenty-ninth pass of beasts (bible §3.2.1): the roles of the sheets 57–100 that the continuous world still played with another
    /// region's beast after the twenty-eighth (<see cref="PeaksOwnBeastRecipes"/>), the Ventania of the Ninho da Huma (58), the Rachadura of the
    /// Canais de Gelo (60), the Cristal Vivo and the Raiz-Gelo of the Galerias dos Mamutes (62), the Carpa de Brasa and the Bambu de Fumaça of
    /// the Jardins de Lava Laqueada (66), the Fumarola of Echeyde (67) and the Fole Vivo of the Nine Furnaces (69), and the Guarda de Taowu of the
    /// Cidadela dos Quatro Gulosos (46). Low-poly from the shared palette. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Ventania</b> (58, ~1.4 m): a funnel of the Destroying Wind with golden twigs torn from the huma's nest. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Rachadura do Gelo</b> (60, ~1 m): a slab of canal ice heaved up round a crack of black water. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Cristal Vivo</b> (62, ~1.1 m): a cluster of cold crystals on a clod of frozen earth, a stolen light in the middle. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Raiz-Gelo</b> (62, ~0.9 m): a knot of roots the mammoth dug out, frozen together and walking. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Carpa de Brasa</b> (66, ~1 m): a lacquer-red koi of the lava ponds swimming in the hot air, embers for scales. Jaw, tail.</item>
    /// <item><b>Bambu de Fumaça</b> (66, ~1.6 m): a clump of smoke-grey bamboo of the cloud pines, a light caught in a node. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Fumarola</b> (67, ~1.4 m): a vent of black lava rock breathing Guayota's sulphur smoke. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Fole Vivo</b> (69, ~0.8 m): a forge bellows of the castle that blows on its own. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Guarda de Taowu</b> (46, ~1.2 m): the tiger with a man's face and a boar's tusks of the old books, in a bronze guard's collar. Jaw, tail, four legs.</item>
    /// </list>
    /// </summary>
    public static class NorthOwn2BeastRecipes
    {
        // ------------------------------------------------------------------
        // Ventania
        // ------------------------------------------------------------------

        public static void BuildGale(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.9f, 0f), DrawGaleBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.12f, 0f), b =>
            {
                // A scrap of the huma's lucky shadow it tore off the nest, spinning in the eye of the wind: it flares before the gust.
                b.Blob(Vector3.zero, new Vector3(0.14f, 0.14f, 0.14f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.07f, 0.07f, 0.06f), EnemySwatch.Gold, coarse: true);
            });
        }

        private static void DrawGaleBody(EnemyMeshBuilder b)
        {
            // The Destroying Wind is one of the five dark lands of the old teachers. Its king wants the huma's nest, and the little gales
            // run ahead of him round the cloud isles, tearing the golden twigs out of the nest and throwing them at whoever climbs.
            for (int i = 0; i < 6; i++)
            {
                float y = -0.68f + i * 0.24f;
                float r = 0.06f + i * 0.07f;
                for (int k = 0; k < 6; k++)
                {
                    float a = (k * 60f + i * 25f) * Mathf.Deg2Rad;
                    float size = 0.12f + i * 0.025f;
                    b.Blob(new Vector3(Mathf.Cos(a) * r, y, Mathf.Sin(a) * r), new Vector3(size, 0.08f, size), (i + k) % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.GolemSnow,
                        coarse: true);
                }
            }

            // The two pale eyes of a wind with no face, and the twigs and feathers caught in the spin.
            HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.36f, 0.3f, 0.05f, EnemySwatch.Sorcery);
            for (int i = 0; i < 4; i++)
            {
                float a = (i * 90f + 20f) * Mathf.Deg2Rad;
                float r = 0.22f + (i % 2) * 0.08f;
                var at = new Vector3(Mathf.Cos(a) * r, -0.1f + i * 0.12f, Mathf.Sin(a) * r);
                b.Segment(at, at + new Vector3(-Mathf.Sin(a) * 0.2f, 0.04f, Mathf.Cos(a) * 0.2f), 0.018f, 0.01f, 4, i % 2 == 0 ? EnemySwatch.Gold : EnemySwatch.Bark);
                b.TwoSided(at + new Vector3(0f, 0.1f, 0f), at + new Vector3(Mathf.Cos(a) * 0.1f, 0.2f, Mathf.Sin(a) * 0.1f), at + new Vector3(0f, 0.16f, 0.06f),
                    EnemySwatch.Ivory, EnemySwatch.Gold);
            }

            b.Blob(new Vector3(0.1f, 0.2f, 0.12f), new Vector3(0.12f, 0.16f, 0.08f), EnemySwatch.Shadow, coarse: true);
            // The little whirl of grit it drags over the stone: the lowest point of the model.
            b.Blob(new Vector3(0f, -0.84f, 0f), new Vector3(0.32f, 0.06f, 0.32f), EnemySwatch.GolemStone, coarse: true);
        }

        // ------------------------------------------------------------------
        // Rachadura do Gelo
        // ------------------------------------------------------------------

        public static void BuildIceCrack(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.35f, 0f), DrawIceCrackBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.1f, 0.2f), b =>
            {
                // The cold light down in the black water of the crack, like an eye: it swells before the ice snaps shut.
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.14f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawIceCrackBody(EnemyMeshBuilder b)
        {
            // Where the boatmen's canals freeze too thin, the ice splits, and the shadow down in the black water pushes the two edges up like
            // a mouth. It walks the canals on the broken slab, and where it goes the ice cracks behind it.
            b.Box(new Vector3(0f, -0.2f, 0f), new Vector3(0.92f, 0.3f, 0.8f), EnemySwatch.GolemSnow, top: EnemySwatch.Crystal);
            // The two heaved edges, leaning apart, and the black water between them.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Box(new Vector3(0.2f * s, 0.12f, 0f), new Vector3(0.32f, 0.42f, 0.7f), EnemySwatch.GolemSnow, Quaternion.Euler(0f, 0f, -18f * s), EnemySwatch.Crystal);
                b.Cylinder(new Vector3(0.3f * s, 0.26f, 0.18f), 0.06f, 0f, 0.3f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(0f, 0f, -20f * s));
                b.Cylinder(new Vector3(0.26f * s, 0.24f, -0.2f), 0.05f, 0f, 0.24f, 4, EnemySwatch.GolemSnow, rotation: Quaternion.Euler(0f, 0f, -14f * s));
            }

            b.Box(new Vector3(0f, 0f, 0f), new Vector3(0.1f, 0.46f, 0.74f), EnemySwatch.Black);
            b.Box(new Vector3(0f, 0.04f, 0.26f), new Vector3(0.12f, 0.2f, 0.06f), EnemySwatch.Shadow);
            HinokuraBeastRecipes.AddEyes(b, 0.05f, 0.16f, 0.36f, 0.04f, EnemySwatch.Sorcery);
            // The hairline cracks running out over the slab, glowing faintly.
            b.Box(new Vector3(0.26f, -0.05f, 0.36f), new Vector3(0.24f, 0.015f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, 30f, 0f));
            b.Box(new Vector3(-0.24f, -0.05f, -0.34f), new Vector3(0.22f, 0.015f, 0.02f), EnemySwatch.GolemCrack, Quaternion.Euler(0f, -24f, 0f));
        }

        // ------------------------------------------------------------------
        // Cristal Vivo
        // ------------------------------------------------------------------

        public static void BuildLivingCrystal(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.1f, 0f), DrawLivingCrystalBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.5f, 0.06f), b =>
            {
                // The light it keeps in its heart and throws: it brightens before the shot.
                b.Blob(Vector3.zero, new Vector3(0.15f, 0.15f, 0.15f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.07f, 0.07f, 0.06f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawLivingCrystalBody(EnemyMeshBuilder b)
        {
            // The mammoth digs its valleys under the earth, and where its tusks scrape the cold stone, crystals grow. Down in the dark the
            // shadow found them hungry for light: they catch the hero's lamp, keep it in the middle and throw it back hard.
            b.Blob(Vector3.zero, new Vector3(0.6f, 0.2f, 0.52f), EnemySwatch.SandstoneDark, coarse: true);
            b.Blob(new Vector3(0.06f, 0.06f, -0.04f), new Vector3(0.4f, 0.14f, 0.36f), EnemySwatch.GolemSnow, coarse: true);
            // The great crystal in the middle and the smaller ones round it, leaning out.
            b.Cylinder(new Vector3(0f, 0.04f, 0f), 0.13f, 0f, 0.98f, 6, EnemySwatch.Crystal);
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 15f) * Mathf.Deg2Rad;
                float lean = 18f + (i % 3) * 8f;
                var rot = Quaternion.Euler(Mathf.Sin(a) * lean, 0f, -Mathf.Cos(a) * lean);
                b.Cylinder(new Vector3(Mathf.Cos(a) * 0.15f, 0.04f, Mathf.Sin(a) * 0.14f), 0.07f, 0f, 0.4f + (i % 2) * 0.2f, 5,
                    i % 2 == 0 ? EnemySwatch.Crystal : EnemySwatch.GolemSnow, rotation: rot);
            }

            // The shadow it took in with the light, and a chip of the mammoth's ivory caught in the clod.
            b.Blob(new Vector3(-0.08f, 0.4f, 0.08f), new Vector3(0.1f, 0.16f, 0.06f), EnemySwatch.Shadow, coarse: true);
            b.Segment(new Vector3(0.2f, 0.02f, 0.16f), new Vector3(0.32f, 0.08f, 0.24f), 0.03f, 0.015f, 4, EnemySwatch.Ivory);
        }

        // ------------------------------------------------------------------
        // Raiz-Gelo
        // ------------------------------------------------------------------

        public static void BuildIceRoot(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawIceRootBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.06f, 0.16f), b =>
            {
                // The warmth it sucked out of the frozen earth, gone cold and pale: it flares before the roots squeeze.
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.12f, 0.1f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.Crystal, coarse: true);
            });
        }

        private static void DrawIceRootBody(EnemyMeshBuilder b)
        {
            // The old trees of the north have their roots deep in the frozen earth. When the mammoth cuts a new gallery it tears them out, and
            // the cold knots them together: they go on reaching for anything warm, and grip it until it is as cold as they are.
            b.Blob(Vector3.zero, new Vector3(0.5f, 0.42f, 0.44f), EnemySwatch.BarkDark, coarse: true);
            b.Blob(new Vector3(0f, 0.16f, -0.02f), new Vector3(0.38f, 0.2f, 0.34f), EnemySwatch.GolemSnow, coarse: true);
            // The roots coiling round the knot and reaching down to the ground like legs: the lowest points of the model.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 30f) * Mathf.Deg2Rad;
                var knee = new Vector3(Mathf.Cos(a) * 0.3f, -0.1f, Mathf.Sin(a) * 0.28f);
                var foot = new Vector3(Mathf.Cos(a) * 0.42f, -0.43f, Mathf.Sin(a) * 0.38f);
                b.Segment(new Vector3(Mathf.Cos(a) * 0.12f, 0.02f, Mathf.Sin(a) * 0.12f), knee, 0.06f, 0.05f, 5, i % 2 == 0 ? EnemySwatch.Bark : EnemySwatch.BarkDark);
                b.Segment(knee, foot, 0.05f, 0.02f, 5, EnemySwatch.Bark);
                b.Blob(knee, new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.GolemSnow, coarse: true);
            }

            // The icicles hanging from the knot and the two pale eyes in the frost.
            for (int i = 0; i < 3; i++)
            {
                b.Segment(new Vector3(-0.12f + i * 0.12f, -0.14f, 0.16f), new Vector3(-0.12f + i * 0.12f, -0.3f, 0.18f), 0.025f, 0.004f, 4, EnemySwatch.Crystal);
            }

            HinokuraBeastRecipes.AddEyes(b, 0.08f, 0.1f, 0.2f, 0.045f, EnemySwatch.MoonEye);
            b.Blob(new Vector3(0.12f, -0.02f, 0.16f), new Vector3(0.12f, 0.14f, 0.08f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Carpa de Brasa
        // ------------------------------------------------------------------

        public static void BuildEmberCarp(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.75f, 0f), DrawEmberCarpBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.02f, 0.34f), b =>
            {
                // The round head of a koi, lacquer red with a gold blaze, the whiskers, and two ember eyes.
                b.Blob(new Vector3(0f, 0f, 0.04f), new Vector3(0.28f, 0.28f, 0.26f), EnemySwatch.WarCape);
                b.Blob(new Vector3(0f, 0.08f, 0.06f), new Vector3(0.16f, 0.1f, 0.16f), EnemySwatch.Gold, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Segment(new Vector3(0.06f * s, -0.06f, 0.16f), new Vector3(0.14f * s, -0.14f, 0.2f), 0.012f, 0.005f, 3, EnemySwatch.Gold);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.03f, 0.12f, 0.05f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.07f, 0.12f), b =>
            {
                // The round mouth that opens to spit the ember it carries.
                b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.14f, 0.04f, 0.1f), EnemySwatch.Gold);
                b.Blob(new Vector3(0f, 0.01f, 0.06f), new Vector3(0.07f, 0.04f, 0.05f), EnemySwatch.Ember, coarse: true);
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0f, -0.36f), b =>
            {
                // The long trailing tail fins of a garden koi, gone to flame at the edges.
                b.Segment(Vector3.zero, new Vector3(0f, 0f, -0.12f), 0.08f, 0.04f, 6, EnemySwatch.WarCape);
                b.TwoSided(new Vector3(0f, 0f, -0.1f), new Vector3(0f, 0.22f, -0.36f), new Vector3(0f, 0.02f, -0.28f), EnemySwatch.Ember, EnemySwatch.Gold);
                b.TwoSided(new Vector3(0f, 0f, -0.1f), new Vector3(0f, -0.02f, -0.28f), new Vector3(0f, -0.2f, -0.34f), EnemySwatch.Ember, EnemySwatch.Gold);
            });
        }

        private static void DrawEmberCarpBody(EnemyMeshBuilder b)
        {
            // The garden ponds of the lacquered isle are lava now, and the koi that lived in them learned to swim in the hot air above.
            // Their scales went to embers: they carry one in the mouth like a pearl, and spit it at whoever leans over the pond.
            b.Blob(Vector3.zero, new Vector3(0.32f, 0.34f, 0.74f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0f, -0.07f, 0.02f), new Vector3(0.24f, 0.2f, 0.6f), EnemySwatch.Gold, coarse: true);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3((i - 1) * 0.06f, 0.13f, 0.18f - i * 0.18f), new Vector3(0.14f, 0.08f, 0.14f), i == 1 ? EnemySwatch.Ivory : EnemySwatch.Ember, coarse: true);
            }

            // The dorsal fin and the two side fins.
            b.TwoSided(new Vector3(0f, 0.15f, 0.16f), new Vector3(0f, 0.3f, -0.08f), new Vector3(0f, 0.15f, -0.22f), EnemySwatch.WarCape, EnemySwatch.Ember);
            for (int s = -1; s <= 1; s += 2)
            {
                b.TwoSided(new Vector3(0.14f * s, -0.06f, 0.18f), new Vector3(0.3f * s, -0.14f, 0.04f), new Vector3(0.14f * s, -0.08f, 0.02f), EnemySwatch.Gold, EnemySwatch.Ember);
            }

            b.Blob(new Vector3(0.08f, 0.06f, -0.12f), new Vector3(0.12f, 0.1f, 0.16f), EnemySwatch.Shadow, coarse: true);
            // The sparks it drips as it swims, falling to the stone, and the dark ring they leave there: the lowest point of the model.
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(0.02f * (i % 2 == 0 ? 1f : -1f), -0.26f - i * 0.12f, -0.04f - i * 0.03f), new Vector3(0.06f - i * 0.008f, 0.06f, 0.06f - i * 0.008f),
                    EnemySwatch.Ember, coarse: true);
            }

            b.Blob(new Vector3(0f, -0.73f, -0.04f), new Vector3(0.3f, 0.04f, 0.4f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Bambu de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeBamboo(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.05f, 0f), DrawSmokeBambooBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.82f, 0.1f), b =>
            {
                // A light caught inside a node of the middle cane, the way a lantern hangs in the pines: it flares before the cane whips.
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.14f, 0.12f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.02f, 0.03f), new Vector3(0.06f, 0.06f, 0.05f), EnemySwatch.SoftEye, coarse: true);
            });
        }

        private static void DrawSmokeBambooBody(EnemyMeshBuilder b)
        {
            // Under the cloud pines of the gardens the bamboo grew in the smoke of the lava, and went grey with it. The shadow got into the
            // hollow canes: now the clump walks on its roots, and whips at whoever comes down the raked paths.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.12f, 0.5f), EnemySwatch.BarkDark, coarse: true);
            float[] xs = { 0f, -0.16f, 0.16f, -0.08f, 0.1f };
            float[] zs = { 0.06f, 0f, -0.02f, -0.14f, -0.12f };
            float[] hs = { 1.5f, 1.2f, 1.3f, 1.0f, 1.1f };
            for (int i = 0; i < xs.Length; i++)
            {
                var foot = new Vector3(xs[i], 0.02f, zs[i]);
                var rot = Quaternion.Euler(-zs[i] * 30f, 0f, -xs[i] * 40f);
                b.Cylinder(foot, 0.05f, 0.04f, hs[i], 6, i % 2 == 0 ? EnemySwatch.Lichen : EnemySwatch.Shroud, rotation: rot, bottomCap: false);
                for (int n = 1; n <= 3; n++)
                {
                    b.Cylinder(foot + rot * new Vector3(0f, hs[i] * n * 0.25f, 0f), 0.056f, 0.056f, 0.03f, 6, EnemySwatch.Bark, rotation: rot);
                }

                // The smoke leaves at the top of each cane.
                Vector3 tip = foot + rot * new Vector3(0f, hs[i], 0f);
                float side = i % 2 == 0 ? 1f : -1f;
                b.TwoSided(tip, tip + new Vector3(0.22f * side, -0.06f, 0.04f), tip + new Vector3(0.06f * side, -0.14f, 0f), EnemySwatch.Shroud, EnemySwatch.Lichen);
                b.Blob(tip + new Vector3(0f, 0.04f, 0f), new Vector3(0.12f, 0.08f, 0.12f), EnemySwatch.Shroud, coarse: true);
            }

            // The two eyes in the middle cane, and the soot where the shadow went in.
            HinokuraBeastRecipes.AddEyes(b, 0.03f, 1.02f, 0.12f, 0.035f, EnemySwatch.Ember);
            b.Blob(new Vector3(0.04f, 0.6f, 0.1f), new Vector3(0.08f, 0.18f, 0.06f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Fumarola
        // ------------------------------------------------------------------

        public static void BuildFumarole(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.3f, 0f), DrawFumaroleBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.22f, 0f), b =>
            {
                // A spark of the sun that Guayota shut in the mountain, glowing in the throat of the vent: it swells before the breath.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.12f, 0.16f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.03f, 0f), new Vector3(0.08f, 0.06f, 0.08f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawFumaroleBody(EnemyMeshBuilder b)
        {
            // The old people of the island said Guayota shut the sun inside Echeyde, and the mountain still breathes out of a thousand little
            // vents. Some of the vents got up and walked off over the malpaís; where they breathe, the air goes yellow and hard to see through.
            b.Cylinder(new Vector3(0f, -0.3f, 0f), 0.42f, 0.18f, 0.5f, 9, EnemySwatch.IronDark, bottomCap: false);
            b.Cylinder(new Vector3(0f, 0.2f, 0f), 0.2f, 0.16f, 0.05f, 9, EnemySwatch.Black, bottomCap: true);
            // The yellow sulphur crust round the rim and the lava glowing in the cracks of the cone.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 10f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.19f, 0.22f, Mathf.Sin(a) * 0.19f), new Vector3(0.08f, 0.05f, 0.08f), EnemySwatch.Gold, coarse: true);
            }

            b.Box(new Vector3(0.14f, -0.12f, 0.27f), new Vector3(0.025f, 0.3f, 0.025f), EnemySwatch.GolemCrack, Quaternion.Euler(-30f, 0f, 16f));
            b.Box(new Vector3(-0.2f, -0.16f, 0.22f), new Vector3(0.025f, 0.24f, 0.025f), EnemySwatch.GolemCrack, Quaternion.Euler(-30f, 0f, -20f));
            b.Box(new Vector3(0.06f, -0.2f, -0.3f), new Vector3(0.025f, 0.2f, 0.025f), EnemySwatch.Ember, Quaternion.Euler(30f, 0f, 10f));
            // The column of sulphur smoke, widening as it rises, and the two eyes looking out of it.
            for (int i = 0; i < 5; i++)
            {
                float y = 0.34f + i * 0.17f;
                float w = 0.18f + i * 0.07f;
                b.Blob(new Vector3(Mathf.Sin(i * 1.3f) * 0.05f, y, Mathf.Cos(i * 1.1f) * 0.03f), new Vector3(w, 0.18f, w), i % 2 == 0 ? EnemySwatch.Shroud : EnemySwatch.GolemStone,
                    coarse: true);
            }

            HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.58f, 0.16f, 0.05f, EnemySwatch.Ember);
            b.Blob(new Vector3(-0.06f, 0.7f, 0.12f), new Vector3(0.1f, 0.12f, 0.06f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Fole Vivo
        // ------------------------------------------------------------------

        public static void BuildLivingBellows(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.46f, 0f), DrawLivingBellowsBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, -0.13f, 0.66f), b =>
            {
                // The coal at the end of its nozzle, the one it blows at you: it flares as the leather fills.
                b.Blob(Vector3.zero, new Vector3(0.12f, 0.12f, 0.12f), EnemySwatch.StolenLight);
                b.Blob(new Vector3(0f, 0.01f, 0.03f), new Vector3(0.07f, 0.07f, 0.06f), EnemySwatch.Ember, coarse: true);
            });
        }

        private static void DrawLivingBellowsBody(EnemyMeshBuilder b)
        {
            // Every furnace of the castle had its bellows, and the smiths worked them all day so the fire would never sleep. When the Fole
            // Apagado got up in the great hall, the little bellows got up after it, and now they blow on their own: on the furnaces, and at you.
            b.Box(new Vector3(0f, 0.22f, 0f), new Vector3(0.6f, 0.05f, 0.7f), EnemySwatch.Bark, Quaternion.Euler(-4f, 0f, 0f));
            b.Box(new Vector3(0f, -0.22f, 0f), new Vector3(0.6f, 0.05f, 0.7f), EnemySwatch.Bark);
            b.Blob(new Vector3(0f, 0f, -0.02f), new Vector3(0.56f, 0.42f, 0.62f), EnemySwatch.Leather);
            for (int i = -1; i <= 1; i++)
            {
                b.Box(new Vector3(0f, i * 0.12f, -0.02f), new Vector3(0.6f, 0.03f, 0.64f), EnemySwatch.BarkDark);
            }

            // The iron nozzle, the two handles at the back and the gold crest of the castle on the top board.
            b.Segment(new Vector3(0f, -0.06f, 0.32f), new Vector3(0f, -0.12f, 0.62f), 0.07f, 0.04f, 6, EnemySwatch.IronDark);
            for (int s = -1; s <= 1; s += 2)
            {
                b.Segment(new Vector3(0.1f * s, 0.22f * s, -0.34f), new Vector3(0.1f * s, 0.26f * s, -0.6f), 0.03f, 0.025f, 4, EnemySwatch.BarkDark);
            }

            b.Blob(new Vector3(0f, 0.26f, 0.1f), new Vector3(0.14f, 0.03f, 0.14f), EnemySwatch.Gold, coarse: true);
            // The two coals of eyes in the leather, the soot round them, and the four stubby feet of the frame: the lowest points of the model.
            HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.04f, 0.3f, 0.06f, EnemySwatch.Ember);
            b.Blob(new Vector3(0f, 0.04f, 0.28f), new Vector3(0.36f, 0.12f, 0.06f), EnemySwatch.Shadow, coarse: true);
            for (int x = -1; x <= 1; x += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    b.Box(new Vector3(0.22f * x, -0.32f, 0.25f * z), new Vector3(0.08f, 0.16f, 0.08f), EnemySwatch.BarkDark);
                }
            }
        }

        // ------------------------------------------------------------------
        // Guarda de Taowu
        // ------------------------------------------------------------------

        public static void BuildTaowuGuard(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawTaowuGuardBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.18f, 0.56f), b =>
            {
                // A man's stern face on a tiger's head, the long hair round it, the heavy brows of one who will not listen.
                b.Blob(new Vector3(0f, 0.02f, -0.04f), new Vector3(0.44f, 0.42f, 0.36f), EnemySwatch.PeccaryFur, coarse: true);
                b.Blob(new Vector3(0f, 0f, 0.06f), new Vector3(0.3f, 0.34f, 0.26f), EnemySwatch.SandstoneLight);
                b.Box(new Vector3(0f, 0.08f, 0.18f), new Vector3(0.22f, 0.04f, 0.04f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, 0f));
                b.Blob(new Vector3(0f, -0.02f, 0.2f), new Vector3(0.06f, 0.08f, 0.05f), EnemySwatch.SandstoneDark, coarse: true);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Blob(new Vector3(0.16f * s, 0.18f, -0.04f), new Vector3(0.08f, 0.08f, 0.05f), EnemySwatch.PeccaryFur, coarse: true);
                }

                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.04f, 0.17f, 0.045f, EnemySwatch.Ember);
            });
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.12f, 0.12f), b =>
            {
                // The lower jaw with the boar's tusks standing up out of it.
                b.Box(new Vector3(0f, -0.02f, 0.06f), new Vector3(0.2f, 0.05f, 0.14f), EnemySwatch.SandstoneLight);
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Cylinder(new Vector3(0.07f * s, 0.004f, 0.11f), 0.018f, 0f, 0.1f, 4, EnemySwatch.Fang, rotation: Quaternion.Euler(-16f, 0f, 10f * s));
                }
            });
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.1f, -0.5f), b =>
            {
                // The tail of the old book, far longer than the beast, dragging behind it.
                b.Segment(Vector3.zero, new Vector3(0f, -0.2f, -0.4f), 0.05f, 0.045f, 5, EnemySwatch.PeccaryFur);
                b.Segment(new Vector3(0f, -0.2f, -0.4f), new Vector3(0.04f, -0.5f, -0.8f), 0.045f, 0.04f, 5, EnemySwatch.Black);
                b.Segment(new Vector3(0.04f, -0.5f, -0.8f), new Vector3(0.1f, -0.62f, -1.16f), 0.04f, 0.03f, 5, EnemySwatch.PeccaryFur);
            });
            HinokuraBeastRecipes.AddFourLegs(bp, 0.62f, 0.18f, 0.32f, -0.32f, 0.075f, EnemySwatch.PeccaryFur, EnemySwatch.Black, "TaowuGuard");
        }

        private static void DrawTaowuGuardBody(EnemyMeshBuilder b)
        {
            // The old books say Taowu had a tiger's body and a man's face, a boar's teeth and hair two feet long, and that it fought on and
            // never once stepped back, because it would not listen. Its guards are made like it, in the bronze collars of the citadel.
            b.Blob(Vector3.zero, new Vector3(0.52f, 0.48f, 1.02f), EnemySwatch.PeccaryFur);
            b.Blob(new Vector3(0f, -0.1f, 0.04f), new Vector3(0.4f, 0.26f, 0.8f), EnemySwatch.BarkDark, coarse: true);
            // The black stripes and the long shaggy hair hanging from the flanks.
            for (int i = 0; i < 4; i++)
            {
                for (int s = -1; s <= 1; s += 2)
                {
                    b.Box(new Vector3(0.24f * s, 0.04f, -0.3f + i * 0.18f), new Vector3(0.02f, 0.24f, 0.04f), EnemySwatch.Black, Quaternion.Euler(0f, 0f, -12f * s));
                    b.Box(new Vector3(0.25f * s, -0.24f, -0.26f + i * 0.18f), new Vector3(0.04f, 0.2f, 0.08f), EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, 8f * s));
                }
            }

            // The bronze guard's collar with the gold boss, the verdigris saddle-plate on the back, and the shadow in the fur.
            b.Cylinder(new Vector3(0f, 0.02f, 0.4f), 0.24f, 0.22f, 0.1f, 8, EnemySwatch.Copper, rotation: Quaternion.Euler(70f, 0f, 0f));
            b.Blob(new Vector3(0f, -0.1f, 0.5f), new Vector3(0.1f, 0.1f, 0.06f), EnemySwatch.Gold, coarse: true);
            b.Box(new Vector3(0f, 0.25f, -0.06f), new Vector3(0.36f, 0.04f, 0.4f), EnemySwatch.Verdigris);
            b.Blob(new Vector3(0.1f, 0.16f, -0.24f), new Vector3(0.14f, 0.08f, 0.18f), EnemySwatch.Shadow, coarse: true);
        }
    }
}
