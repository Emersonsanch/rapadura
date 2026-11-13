using UnityEngine;

namespace Rapadura.Gameplay.Enemies.Visuals
{
    /// <summary>
    /// The twentieth pass of beasts (bible §3.2.1): the roles of Região 7, the Teto do Mundo (Docs/Mapas100.md 36–40), that the continuous
    /// world still played with another region's beast, and two of Região 8, the Planície de Mingcheng (42 and 45), low-poly from the shared
    /// palette like <see cref="LentRoleBeastRecipes"/>. Pivot at the feet (y = 0), facing +Z.
    /// <list type="bullet">
    /// <item><b>Pedra-Rolante da Morena</b> (36, ~1.1 m): a moraine boulder with snow on top and blue ice in its cracks. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Ushabti de Calha</b> (37, ~1.5 m): a stone answering figure of the Artificers with a pick and an ore basket on its back. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// <item><b>Neblina das Cristas</b> (37, ~1.4 m): a cold cloud off the crests of the ill wind, with a stolen glint of frost inside. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Pipa de Fumaça</b> (39, ~1.7 m): a caravan kite gone grey, five ribbons on its tail and the lamp still tied at the cross. <c>Body</c> → <c>StolenLight</c>.</item>
    /// <item><b>Gavião-Real Apagado</b> (39, ~1.1 m): the big crested falcon of the fort's falconers, ash on its back. Bird skeleton (<c>Body</c> → <c>Head</c> → <c>Jaw</c>, wings, tail).</item>
    /// <item><b>Morcego do Vento</b> (39, ~1 m): a pale bat of the wind-bell towers with a little bell at its throat. Bird skeleton.</item>
    /// <item><b>Guarda de Pedra Ushabti</b> (39, ~1.7 m): a lime-white answering figure with a juniper shield and a spear. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// <item><b>Bandoleiro de Āz</b> (40, ~1.7 m): a pass robber in a sheepskin coat with caravan ribbons tied to his belt. <c>Body</c> → <c>Head</c>, two arms, two legs.</item>
    /// <item><b>Argali Apagado</b> (40, ~1.5 m): a wild sheep of the pass with great spiral horns. <c>Body</c> → <c>Head</c> → <c>Jaw</c>, <c>Tail</c>; four legs.</item>
    /// <item><b>Gralha das Neves</b> (40, ~1 m): a black chough with frost on its wingtips and a yellow bill. Bird skeleton.</item>
    /// <item><b>Arqueiro de Terracota</b> (45, ~1.6 m): a kneeling clay archer whose legs broke off, floating on its own smoke. <c>Body</c> → <c>Head</c>, two arms.</item>
    /// <item><b>Íbis de Luz Apagada</b> (42, ~1.2 m): a white ibis of the white spires with a little mirror on its brow. Bird skeleton.</item>
    /// </list>
    /// </summary>
    public static class HighlandOwnBeastRecipes
    {
        // ------------------------------------------------------------------
        // Pedra-Rolante da Morena
        // ------------------------------------------------------------------

        public static void BuildMoraineRoller(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.56f, 0f), DrawMoraineRollerBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0.1f, 0.08f, 0.46f), b =>
            {
                // The ice in its deepest crack, holding a little of the light the glacier ground out of the mountain; it flares before the roll.
                b.Blob(Vector3.zero, new Vector3(0.16f, 0.22f, 0.08f), EnemySwatch.Crystal);
                b.Blob(new Vector3(0.02f, 0.05f, 0.02f), new Vector3(0.06f, 0.08f, 0.04f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawMoraineRollerBody(EnemyMeshBuilder b)
        {
            // The glacier carries its boulders down a finger's width a year and leaves them on the moraines, where the yaks scratch on them. The shadow got into the ice of their cracks, and now they roll down on purpose, onto the terraces.
            b.Blob(Vector3.zero, new Vector3(1.08f, 1f, 1.04f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0.16f, 0.14f, -0.12f), new Vector3(0.62f, 0.52f, 0.6f), EnemySwatch.GolemStoneDark, coarse: true);
            b.Blob(new Vector3(-0.2f, -0.1f, 0.14f), new Vector3(0.5f, 0.46f, 0.5f), EnemySwatch.GolemStoneDark, coarse: true);
            // The snow cap it carried down off the glacier and the blue ice in the cracks.
            b.Blob(new Vector3(0f, 0.38f, 0f), new Vector3(0.72f, 0.24f, 0.68f), EnemySwatch.GolemSnow, coarse: true);
            b.Box(new Vector3(-0.12f, 0.06f, 0.48f), new Vector3(0.05f, 0.42f, 0.04f), EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, 18f));
            b.Box(new Vector3(0.46f, -0.04f, 0.1f), new Vector3(0.04f, 0.36f, 0.05f), EnemySwatch.Crystal, Quaternion.Euler(0f, 0f, -12f));
            // The shadow's soot in the scratch marks of the yaks, and two cold eyes in the stone.
            b.Blob(new Vector3(-0.3f, 0.2f, -0.3f), new Vector3(0.3f, 0.12f, 0.26f), EnemySwatch.Shadow, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.16f, 0.14f, 0.48f, 0.07f, EnemySwatch.Crystal);
            // The moraine gravel stuck round its foot: the lowest points of the model.
            for (int i = 0; i < 6; i++)
            {
                float a = (i * 60f + 15f) * Mathf.Deg2Rad;
                b.Blob(new Vector3(Mathf.Cos(a) * 0.4f, -0.46f, Mathf.Sin(a) * 0.38f), new Vector3(0.18f, 0.16f, 0.18f),
                    i % 2 == 0 ? EnemySwatch.WolfFur : EnemySwatch.GolemStoneDark, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Ushabti de Calha
        // ------------------------------------------------------------------

        public static void BuildChuteUshabti(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.62f, 0f), DrawChuteUshabtiBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.52f, 0.02f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.3f, 0.34f, 0.28f), EnemySwatch.GolemStone);
                // The striped wig of the figures, and the miner's lamp the Artificers fixed on its brow.
                b.Blob(new Vector3(0f, 0.06f, -0.04f), new Vector3(0.38f, 0.32f, 0.34f), EnemySwatch.IronDark, coarse: true);
                for (int i = 0; i < 3; i++)
                {
                    b.Box(new Vector3(0f, 0.12f - i * 0.08f, -0.2f), new Vector3(0.36f, 0.02f, 0.04f), EnemySwatch.Gold);
                }

                b.Cylinder(new Vector3(0f, 0.12f, 0.12f), 0.05f, 0.05f, 0.06f, 6, EnemySwatch.Copper, rotation: Quaternion.Euler(90f, 0f, 0f));
                b.Blob(new Vector3(0f, 0.12f, 0.19f), new Vector3(0.06f, 0.06f, 0.03f), EnemySwatch.StolenLight, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.02f, 0.13f, 0.045f, EnemySwatch.Ember);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.27f * side, 0.3f, 0.04f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.04f * side, -0.28f, 0.12f), 0.08f, 0.065f, 6, EnemySwatch.GolemStone);
                    b.Segment(new Vector3(0.04f * side, -0.28f, 0.12f), new Vector3(0.02f * side, -0.46f, 0.28f), 0.065f, 0.055f, 6, EnemySwatch.GolemStoneDark);
                    b.Blob(new Vector3(0.02f * side, -0.5f, 0.3f), new Vector3(0.13f, 0.12f, 0.13f), EnemySwatch.GolemStone, coarse: true);
                    if (right)
                    {
                        // The pick it breaks the ore loose with, the one the chutes carry down.
                        b.Segment(new Vector3(0.02f, -0.62f, 0.3f), new Vector3(0.02f, -0.1f, 0.34f), 0.025f, 0.025f, 5, EnemySwatch.Bark);
                        b.Box(new Vector3(0.02f, -0.1f, 0.38f), new Vector3(0.05f, 0.05f, 0.3f), EnemySwatch.Iron, Quaternion.Euler(-14f, 0f, 0f));
                    }
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: right ? "ChuteUshabtiArm_R" : "ChuteUshabtiArm_L");
            }
        }

        private static void DrawChuteUshabtiBody(EnemyMeshBuilder b)
        {
            // The Artificers cut the chutes to bring the ore of the Teto down to Sangkent, and set answering figures at the top to fill the carts, as the figures in the old tombs answer when called to work the fields. The shadow called them too, and they answer it, and push the carts onto anyone below.
            b.Box(new Vector3(0f, -0.58f, 0f), new Vector3(0.62f, 0.08f, 0.5f), EnemySwatch.GolemStoneDark);
            b.Cylinder(new Vector3(0f, -0.56f, 0f), 0.28f, 0.24f, 0.9f, 8, EnemySwatch.GolemStone, bottomCap: true);
            b.Blob(new Vector3(0f, 0.3f, 0f), new Vector3(0.6f, 0.36f, 0.42f), EnemySwatch.GolemStone);
            // The column of glyphs down the front, the call to work still lit, and the soot of the shadow over it.
            b.Box(new Vector3(0f, -0.14f, 0.25f), new Vector3(0.1f, 0.62f, 0.03f), EnemySwatch.SandstoneDark);
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.08f - i * 0.15f, 0.27f), new Vector3(0.06f, 0.04f, 0.02f), EnemySwatch.Rune);
            }

            b.Blob(new Vector3(0.12f, 0.1f, 0.18f), new Vector3(0.2f, 0.26f, 0.12f), EnemySwatch.Shadow, coarse: true);
            // The cart harness across the chest and the ore basket on its back, heaped with copper ore.
            b.Box(new Vector3(0f, 0.18f, 0.2f), new Vector3(0.5f, 0.05f, 0.04f), EnemySwatch.Leather, Quaternion.Euler(0f, 0f, 24f));
            b.Cylinder(new Vector3(0f, -0.06f, -0.34f), 0.2f, 0.24f, 0.4f, 7, EnemySwatch.Bark, bottomCap: true);
            for (int i = 0; i < 3; i++)
            {
                b.Blob(new Vector3(-0.08f + i * 0.08f, 0.36f, -0.34f), new Vector3(0.14f, 0.12f, 0.14f), i == 1 ? EnemySwatch.CopperDark : EnemySwatch.Copper, coarse: true);
            }
        }

        // ------------------------------------------------------------------
        // Neblina das Cristas
        // ------------------------------------------------------------------

        public static void BuildCrestMist(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.96f, 0f), DrawCrestMistBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.04f, 0.26f), b =>
            {
                // The frost glint it took off a climber's lamp and keeps in the middle of itself; it flares before the cold gust.
                b.Blob(Vector3.zero, new Vector3(0.18f, 0.18f, 0.14f), EnemySwatch.Crystal);
                b.Blob(new Vector3(0.03f, 0.03f, 0.03f), new Vector3(0.08f, 0.08f, 0.06f), EnemySwatch.StolenLight, coarse: true);
            });
        }

        private static void DrawCrestMistBody(EnemyMeshBuilder b)
        {
            // The ill wind of the crests comes over the ridge every evening and goes down the other side; the herders count the days by it. The shadow made part of it stay, a cold cloud that sits on the chutes and the lake and will not move on.
            b.Blob(Vector3.zero, new Vector3(0.66f, 0.5f, 0.6f), EnemySwatch.Shroud);
            b.Blob(new Vector3(0.28f, 0.08f, -0.06f), new Vector3(0.42f, 0.36f, 0.4f), EnemySwatch.IvoryDark, coarse: true);
            b.Blob(new Vector3(-0.28f, 0.04f, 0.04f), new Vector3(0.44f, 0.38f, 0.42f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(0f, 0.24f, -0.1f), new Vector3(0.4f, 0.3f, 0.36f), EnemySwatch.GolemSnow, coarse: true);
            // The dark it is curdled round, showing through the front.
            b.Blob(new Vector3(0f, -0.02f, 0.18f), new Vector3(0.34f, 0.28f, 0.16f), EnemySwatch.Shadow, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.12f, 0.12f, 0.27f, 0.05f, EnemySwatch.MoonEye);
            // The tail of mist it drags over the snow: the lowest point of the model.
            b.Segment(new Vector3(0f, -0.2f, -0.04f), new Vector3(0.04f, -0.7f, -0.14f), 0.2f, 0.1f, 6, EnemySwatch.Shroud);
            b.Blob(new Vector3(0.05f, -0.86f, -0.16f), new Vector3(0.4f, 0.12f, 0.34f), EnemySwatch.IvoryDark, coarse: true);
            // Hoarfrost needles growing out of its edge.
            for (int i = 0; i < 5; i++)
            {
                float a = (i * 72f + 20f) * Mathf.Deg2Rad;
                var at = new Vector3(Mathf.Cos(a) * 0.4f, 0.02f, Mathf.Sin(a) * 0.34f);
                b.Cylinder(at, 0.03f, 0f, 0.14f, 4, EnemySwatch.Crystal, rotation: Quaternion.Euler(Mathf.Sin(a) * 60f, 0f, -Mathf.Cos(a) * 60f));
            }
        }

        // ------------------------------------------------------------------
        // Pipa de Fumaça
        // ------------------------------------------------------------------

        public static void BuildSmokeKite(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 1.24f, 0f), DrawSmokeKiteBody);
            bp.Mesh(ProceduralEnemyParts.StolenLight, body, new Vector3(0f, 0.02f, 0.06f), b =>
            {
                // The little caravan lamp tied at the cross of the spars; it swells before the kite throws its bolt.
                b.Cylinder(new Vector3(0f, -0.06f, 0f), 0.05f, 0.04f, 0.1f, 6, EnemySwatch.Copper, bottomCap: true);
                b.Blob(new Vector3(0f, 0f, 0.02f), new Vector3(0.1f, 0.1f, 0.08f), EnemySwatch.StolenLight);
            });
        }

        private static void DrawSmokeKiteBody(EnemyMeshBuilder b)
        {
            // The caravan children fly paper kites off the fort with a ribbon of each of the five lights on the tail, and the falconers say the wind is good when the kites stand still. One night the kites came down without their children, grey with smoke, and the ribbons had gone dull.
            b.Box(Vector3.zero, new Vector3(0.62f, 0.62f, 0.04f), EnemySwatch.SandstoneLight, Quaternion.Euler(0f, 0f, 45f));
            b.Box(new Vector3(0f, 0f, 0.012f), new Vector3(0.4f, 0.4f, 0.03f), EnemySwatch.IvoryDark, Quaternion.Euler(0f, 0f, 45f));
            // The smoke stain spreading from one corner, and the face the children painted on it, its eyes still lit.
            b.Box(new Vector3(0.14f, 0.16f, 0.025f), new Vector3(0.24f, 0.2f, 0.02f), EnemySwatch.Shadow, Quaternion.Euler(0f, 0f, 30f));
            HinokuraBeastRecipes.AddEyes(b, 0.1f, 0.14f, 0.03f, 0.05f, EnemySwatch.Ember);
            // The juniper spars.
            b.Segment(new Vector3(0f, -0.44f, -0.03f), new Vector3(0f, 0.44f, -0.03f), 0.02f, 0.02f, 4, EnemySwatch.Bark);
            b.Segment(new Vector3(-0.44f, 0.04f, -0.03f), new Vector3(0.44f, 0.04f, -0.03f), 0.02f, 0.02f, 4, EnemySwatch.Bark);
            // The tail line with the five ribbons of the five lights, gone dull, sweeping back and down to the snow.
            var top = new Vector3(0f, -0.44f, -0.03f);
            var end = new Vector3(0.06f, -1.2f, -0.5f);
            b.Segment(top, end, 0.015f, 0.012f, 4, EnemySwatch.Black);
            EnemySwatch[] ribbons = { EnemySwatch.WarCape, EnemySwatch.Gold, EnemySwatch.Crystal, EnemySwatch.Lichen, EnemySwatch.Ivory };
            for (int i = 0; i < ribbons.Length; i++)
            {
                Vector3 at = Vector3.Lerp(top, end, 0.16f + i * 0.18f);
                b.Box(at, new Vector3(0.16f, 0.05f, 0.03f), ribbons[i], Quaternion.Euler(0f, 0f, i % 2 == 0 ? 20f : -20f));
            }

            // The broken string trailing off the bridle, its end lying on the ground where the child let go.
            b.Segment(new Vector3(0f, -0.1f, 0.03f), new Vector3(-0.08f, -1.22f, 0.3f), 0.012f, 0.012f, 4, EnemySwatch.IvoryDark);
        }

        // ------------------------------------------------------------------
        // Gavião-Real Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedRoyalHawk(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "DimmedRoyalHawk", 0.66f, DrawDimmedRoyalHawkBody, new Vector3(0f, 0.38f, 0.36f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.24f, 0.24f, 0.28f), EnemySwatch.WolfFurDark);
                    // The pale cheeks and the dark "moustache" of the big falcons, and the crest the fort calls royal.
                    b.Blob(new Vector3(0f, -0.04f, 0.05f), new Vector3(0.2f, 0.12f, 0.16f), EnemySwatch.Ivory, coarse: true);
                    b.Box(new Vector3(0f, -0.02f, 0.08f), new Vector3(0.22f, 0.08f, 0.03f), EnemySwatch.Black);
                    b.Cylinder(new Vector3(0f, 0.1f, -0.04f), 0.05f, 0f, 0.16f, 4, EnemySwatch.WolfFurDark, rotation: Quaternion.Euler(-40f, 0f, 0f));
                    b.Cylinder(new Vector3(0f, -0.01f, 0.11f), 0.045f, 0.012f, 0.13f, 5, EnemySwatch.Horn, rotation: Quaternion.Euler(104f, 0f, 0f));
                    HinokuraBeastRecipes.AddEyes(b, 0.075f, 0.04f, 0.08f, 0.05f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.06f, 0.02f, 0.12f), EnemySwatch.Horn),
                EnemySwatch.WolfFurDark, EnemySwatch.WolfBelly, EnemySwatch.Black, 1.2f, EnemySwatch.WolfFurDark, EnemySwatch.Ivory);
        }

        private static void DrawDimmedRoyalHawkBody(EnemyMeshBuilder b)
        {
            // The falconers of the Forte dos Ventos fly the big crested falcons off the towers to keep the vultures from the caravans' mules. The shadow came up the gorge on the ill wind, and the falcons stopped coming back to the glove; now they stoop on the caravans themselves.
            b.Blob(Vector3.zero, new Vector3(0.54f, 0.5f, 0.8f), EnemySwatch.WolfFurDark);
            b.Blob(new Vector3(0f, -0.08f, 0.12f), new Vector3(0.4f, 0.32f, 0.42f), EnemySwatch.WolfBelly, coarse: true);
            b.Segment(new Vector3(0f, 0.12f, 0.26f), new Vector3(0f, 0.36f, 0.34f), 0.09f, 0.08f, 6, EnemySwatch.WolfFurDark);
            // The barred breast, the jess of the falconers still on one leg, and the ash on its back.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0f, -0.04f - i * 0.08f, 0.3f), new Vector3(0.24f, 0.025f, 0.04f), EnemySwatch.WolfFur);
            }

            b.Box(new Vector3(0.11f, -0.36f, 0.06f), new Vector3(0.03f, 0.12f, 0.03f), EnemySwatch.WarCape);
            b.Blob(new Vector3(0.06f, 0.22f, -0.06f), new Vector3(0.26f, 0.06f, 0.3f), EnemySwatch.Shadow, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.66f, 0.11f, EnemySwatch.Gold, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Morcego do Vento
        // ------------------------------------------------------------------

        public static void BuildWindBat(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "WindBat", 0.56f, DrawWindBatBody, new Vector3(0f, 0.22f, 0.3f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.26f, 0.24f, 0.24f), EnemySwatch.GolemStone);
                    // Ears wide like the mouths of bells, to hear the bells ring before anyone else.
                    for (int s = -1; s <= 1; s += 2)
                    {
                        b.Cylinder(new Vector3(0.09f * s, 0.08f, -0.02f), 0.07f, 0.02f, 0.18f, 5, EnemySwatch.BatWing, rotation: Quaternion.Euler(-14f, 0f, -24f * s));
                    }

                    b.Blob(new Vector3(0f, -0.02f, 0.11f), new Vector3(0.09f, 0.06f, 0.06f), EnemySwatch.Pink, coarse: true);
                    HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.04f, 0.1f, 0.05f, EnemySwatch.BatEye);
                },
                new Vector3(0f, -0.06f, 0.08f), b =>
                {
                    b.Box(new Vector3(0f, -0.01f, 0.04f), new Vector3(0.12f, 0.03f, 0.08f), EnemySwatch.GolemStone);
                    b.Cylinder(new Vector3(0.03f, 0.005f, 0.07f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                    b.Cylinder(new Vector3(-0.03f, 0.005f, 0.07f), 0.01f, 0f, 0.035f, 3, EnemySwatch.Fang);
                },
                EnemySwatch.BatWing, EnemySwatch.GolemStoneDark, EnemySwatch.Copper, 1.15f, EnemySwatch.GolemStone, EnemySwatch.BatWing);
        }

        private static void DrawWindBatBody(EnemyMeshBuilder b)
        {
            // The pale bats roost inside the wind bells of the fort's towers and keep the moths off the bell ropes; the watch says a bell rings by itself when a bat comes home. Since the shadow the bells ring all night, and the bats come out of them in a cloud at whoever climbs.
            b.Blob(Vector3.zero, new Vector3(0.42f, 0.42f, 0.52f), EnemySwatch.GolemStone);
            b.Blob(new Vector3(0f, -0.06f, 0.08f), new Vector3(0.3f, 0.26f, 0.3f), EnemySwatch.GolemStoneDark, coarse: true);
            // The little copper bell it wears at the throat, the clapper glowing like an ember.
            b.Cylinder(new Vector3(0f, -0.2f, 0.18f), 0.07f, 0.035f, 0.1f, 6, EnemySwatch.Copper, bottomCap: true);
            b.Blob(new Vector3(0f, -0.22f, 0.18f), new Vector3(0.04f, 0.04f, 0.04f), EnemySwatch.Ember, coarse: true);
            b.Blob(new Vector3(-0.06f, 0.16f, -0.08f), new Vector3(0.2f, 0.06f, 0.24f), EnemySwatch.Shadow, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.56f, 0.08f, EnemySwatch.GolemStoneDark, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Guarda de Pedra Ushabti
        // ------------------------------------------------------------------

        public static void BuildStoneGuardUshabti(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.72f, 0f), DrawStoneGuardUshabtiBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.6f, 0.02f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.32f, 0.36f, 0.3f), EnemySwatch.Ivory);
                // The tall felt cap of the fort's watch, carved in the stone, and the painted eyes that keep the gorge.
                b.Cylinder(new Vector3(0f, 0.1f, -0.02f), 0.18f, 0.12f, 0.24f, 7, EnemySwatch.WarCape, bottomCap: true);
                b.Box(new Vector3(0f, 0.12f, 0.15f), new Vector3(0.3f, 0.04f, 0.03f), EnemySwatch.Gold);
                HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.0f, 0.14f, 0.05f, EnemySwatch.Rune);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.3f * side, 0.36f, 0.04f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.05f * side, -0.3f, 0.1f), 0.085f, 0.07f, 6, EnemySwatch.Ivory);
                    b.Segment(new Vector3(0.05f * side, -0.3f, 0.1f), new Vector3(0.03f * side, -0.5f, 0.26f), 0.07f, 0.06f, 6, EnemySwatch.IvoryDark);
                    b.Blob(new Vector3(0.03f * side, -0.54f, 0.28f), new Vector3(0.14f, 0.13f, 0.14f), EnemySwatch.Ivory, coarse: true);
                    if (right)
                    {
                        // The spear of the watch, its head black iron.
                        b.Segment(new Vector3(0.03f, -0.9f, 0.28f), new Vector3(0.03f, 0.3f, 0.3f), 0.025f, 0.025f, 5, EnemySwatch.Bark);
                        b.Cylinder(new Vector3(0.03f, 0.3f, 0.3f), 0.05f, 0f, 0.2f, 4, EnemySwatch.IronDark);
                    }
                    else
                    {
                        // The round juniper shield with the fort's wind-vane painted on it.
                        b.Cylinder(new Vector3(-0.08f, -0.44f, 0.26f), 0.24f, 0.24f, 0.05f, 9, EnemySwatch.Bark, EnemySwatch.BarkDark, Quaternion.Euler(0f, 0f, 90f), bottomCap: true);
                        b.Box(new Vector3(-0.14f, -0.44f, 0.26f), new Vector3(0.02f, 0.3f, 0.04f), EnemySwatch.Gold);
                        b.Box(new Vector3(-0.14f, -0.44f, 0.26f), new Vector3(0.02f, 0.04f, 0.3f), EnemySwatch.Gold);
                    }
                }, Quaternion.Euler(-8f, 0f, 0f), meshKey: right ? "StoneGuardUshabtiArm_R" : "StoneGuardUshabtiArm_L");
            }
        }

        private static void DrawStoneGuardUshabtiBody(EnemyMeshBuilder b)
        {
            // The Artificers left the fort a watch of answering figures, whitewashed with the same lime as the walls, to stand on the battlements when the falconers slept. The shadow woke the watch at the wrong hour, and it guards the gorge against everyone now, the falconers too.
            b.Box(new Vector3(0f, -0.68f, 0f), new Vector3(0.66f, 0.08f, 0.54f), EnemySwatch.GolemStoneDark);
            b.Cylinder(new Vector3(0f, -0.66f, 0f), 0.3f, 0.27f, 1.02f, 8, EnemySwatch.Ivory, bottomCap: true);
            b.Blob(new Vector3(0f, 0.36f, 0f), new Vector3(0.66f, 0.4f, 0.46f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, 0.22f, 0.04f), new Vector3(0.56f, 0.24f, 0.42f), EnemySwatch.IvoryDark, coarse: true);
            // The wind bell hung on its chest, as on every post of the fort, its clapper an ember.
            b.Cylinder(new Vector3(0f, 0.02f, 0.26f), 0.09f, 0.045f, 0.14f, 6, EnemySwatch.Copper, bottomCap: true);
            b.Blob(new Vector3(0f, -0.01f, 0.27f), new Vector3(0.05f, 0.05f, 0.05f), EnemySwatch.Ember, coarse: true);
            // The glyph bands of the figures and the grey of the shadow where the whitewash flaked.
            for (int i = 0; i < 3; i++)
            {
                b.Box(new Vector3(0f, -0.2f - i * 0.16f, 0.28f), new Vector3(0.4f, 0.03f, 0.02f), EnemySwatch.SandstoneDark);
            }

            b.Blob(new Vector3(-0.14f, -0.3f, 0.2f), new Vector3(0.2f, 0.3f, 0.12f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Bandoleiro de Āz
        // ------------------------------------------------------------------

        public static void BuildAzBandit(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.9f, 0f), DrawAzBanditBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.44f, 0.04f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.28f, 0.3f, 0.28f), EnemySwatch.SandstoneDark);
                // The fur hat of the pass and the scarf pulled up over the face; only the greedy eyes show.
                b.Cylinder(new Vector3(0f, 0.08f, -0.01f), 0.2f, 0.17f, 0.18f, 8, EnemySwatch.WolfFur, EnemySwatch.WolfFurDark, bottomCap: true);
                b.Blob(new Vector3(0f, -0.06f, 0.06f), new Vector3(0.3f, 0.16f, 0.24f), EnemySwatch.Black, coarse: true);
                HinokuraBeastRecipes.AddEyes(b, 0.06f, 0.03f, 0.13f, 0.045f, EnemySwatch.WarEye);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.3f * side, 0.26f, 0f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.06f * side, -0.3f, 0.1f), 0.075f, 0.06f, 6, EnemySwatch.WolfBelly);
                    b.Segment(new Vector3(0.06f * side, -0.3f, 0.1f), new Vector3(0.05f * side, -0.56f, 0.2f), 0.06f, 0.045f, 6, EnemySwatch.Leather);
                    b.Blob(new Vector3(0.05f * side, -0.6f, 0.22f), new Vector3(0.11f, 0.12f, 0.11f), EnemySwatch.SandstoneDark, coarse: true);
                    if (right)
                    {
                        // The curved sabre, and the gold of a caravan's bridle wound round its hilt.
                        b.Box(new Vector3(0.05f, -0.62f, 0.3f), new Vector3(0.04f, 0.06f, 0.12f), EnemySwatch.Gold);
                        b.Segment(new Vector3(0.05f, -0.62f, 0.36f), new Vector3(0.05f, -0.5f, 0.86f), 0.035f, 0.012f, 4, EnemySwatch.Steel);
                    }
                }, Quaternion.Euler(-12f, 0f, 0f), meshKey: right ? "AzBanditArm_R" : "AzBanditArm_L");
            }

            HinokuraBeastRecipes.AddBipedLegs(bp, 0.6f, 0.14f, 0.09f, EnemySwatch.Leather, EnemySwatch.WolfFurDark, EnemySwatch.Black, "AzBandit");
        }

        private static void DrawAzBanditBody(EnemyMeshBuilder b)
        {
            // Āz is the greed that wants every load on the road for itself. Her bandits wait in the snow of the pass where the caravans must go slow, take what the mules carry, and tie the caravan's ribbons to their belts to count; they have more ribbons than they can count.
            b.Cylinder(new Vector3(0f, -0.34f, 0f), 0.26f, 0.22f, 0.5f, 9, EnemySwatch.WolfBelly, bottomCap: true);
            b.Blob(new Vector3(0f, 0.08f, 0f), new Vector3(0.56f, 0.52f, 0.42f), EnemySwatch.WolfBelly);
            b.Blob(new Vector3(0f, 0.2f, -0.04f), new Vector3(0.6f, 0.26f, 0.44f), EnemySwatch.WolfFur, coarse: true);
            // The belt of stolen coins and the ribbons of the five lights hanging from it, taken off the carts.
            b.Box(new Vector3(0f, -0.14f, 0f), new Vector3(0.5f, 0.07f, 0.42f), EnemySwatch.Leather);
            for (int i = 0; i < 4; i++)
            {
                b.Blob(new Vector3(-0.15f + i * 0.1f, -0.14f, 0.21f), new Vector3(0.05f, 0.05f, 0.02f), EnemySwatch.Gold, coarse: true);
            }

            EnemySwatch[] ribbons = { EnemySwatch.WarCape, EnemySwatch.Gold, EnemySwatch.Crystal, EnemySwatch.Lichen, EnemySwatch.Ivory };
            for (int i = 0; i < ribbons.Length; i++)
            {
                float a = (i * 34f - 70f) * Mathf.Deg2Rad;
                b.Box(new Vector3(Mathf.Sin(a) * 0.26f, -0.3f, Mathf.Cos(a) * -0.2f), new Vector3(0.05f, 0.24f, 0.02f), ribbons[i], Quaternion.Euler(0f, 0f, i % 2 == 0 ? 6f : -6f));
            }

            // The sack over its shoulder, and the shadow's soot on the coat.
            b.Blob(new Vector3(-0.14f, 0.24f, -0.3f), new Vector3(0.4f, 0.42f, 0.32f), EnemySwatch.SandstoneLight, coarse: true);
            b.Blob(new Vector3(0.12f, 0.02f, 0.18f), new Vector3(0.2f, 0.24f, 0.1f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Argali Apagado
        // ------------------------------------------------------------------

        public static void BuildDimmedArgali(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.84f, 0f), DrawDimmedArgaliBody);
            int head = bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.3f, 0.6f), DrawDimmedArgaliHead);
            bp.Mesh(ProceduralEnemyParts.Jaw, head, new Vector3(0f, -0.09f, 0.12f), b =>
                b.Box(new Vector3(0f, -0.01f, 0.07f), new Vector3(0.1f, 0.03f, 0.14f), EnemySwatch.WolfBelly));
            bp.Mesh(ProceduralEnemyParts.Tail, body, new Vector3(0f, 0.12f, -0.54f), b =>
                b.Segment(Vector3.zero, new Vector3(0f, -0.06f, -0.08f), 0.05f, 0.025f, 4, EnemySwatch.WolfBelly));
            HinokuraBeastRecipes.AddFourLegs(bp, 0.62f, 0.2f, 0.38f, -0.36f, 0.06f, EnemySwatch.Sandstone, EnemySwatch.Black, "DimmedArgali");
        }

        private static void DrawDimmedArgaliBody(EnemyMeshBuilder b)
        {
            // The argali walk the pass in single file above the caravans and knock little stones down to warn them of the snow, so the caravan men call them the pass's own sentries. The shadow turned the sentries round: now they knock the stones onto the road, and come down after them horns first.
            b.Blob(Vector3.zero, new Vector3(0.62f, 0.58f, 1.1f), EnemySwatch.Sandstone);
            b.Blob(new Vector3(0f, -0.14f, 0.04f), new Vector3(0.48f, 0.26f, 0.84f), EnemySwatch.WolfBelly, coarse: true);
            b.Blob(new Vector3(0f, 0.14f, 0.3f), new Vector3(0.56f, 0.44f, 0.44f), EnemySwatch.SandstoneDark, coarse: true);
            // The white ruff of an old ram at the throat and the pale rump patch.
            b.Blob(new Vector3(0f, 0.06f, 0.5f), new Vector3(0.34f, 0.3f, 0.2f), EnemySwatch.Ivory, coarse: true);
            b.Blob(new Vector3(0f, 0.04f, -0.5f), new Vector3(0.4f, 0.34f, 0.16f), EnemySwatch.Ivory, coarse: true);
            b.Segment(new Vector3(0f, 0.16f, 0.4f), new Vector3(0f, 0.3f, 0.58f), 0.14f, 0.1f, 6, EnemySwatch.SandstoneDark);
            // The soot down its spine where the shadow rode it.
            b.Box(new Vector3(0f, 0.28f, -0.04f), new Vector3(0.12f, 0.04f, 0.7f), EnemySwatch.Shadow);
        }

        private static void DrawDimmedArgaliHead(EnemyMeshBuilder b)
        {
            b.Blob(Vector3.zero, new Vector3(0.2f, 0.22f, 0.32f), EnemySwatch.SandstoneDark);
            b.Blob(new Vector3(0f, -0.04f, 0.14f), new Vector3(0.13f, 0.11f, 0.16f), EnemySwatch.WolfBelly, coarse: true);
            HinokuraBeastRecipes.AddEyes(b, 0.09f, 0.04f, 0.06f, 0.04f, EnemySwatch.MoonEye);
            // The great horns, each a full turn of a spiral out to the side and round forward again, ridged with the years.
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                Vector3 prev = new Vector3(0.07f * side, 0.1f, -0.04f);
                for (int i = 1; i <= 6; i++)
                {
                    float a = i * 55f * Mathf.Deg2Rad;
                    float r = 0.06f + i * 0.025f;
                    var next = new Vector3((0.1f + i * 0.035f) * side, 0.08f + Mathf.Cos(a) * r, -0.04f - Mathf.Sin(a) * r);
                    b.Segment(prev, next, 0.065f - i * 0.007f, 0.06f - i * 0.007f, 6, i % 2 == 0 ? EnemySwatch.Horn : EnemySwatch.Bone);
                    prev = next;
                }
            }
        }

        // ------------------------------------------------------------------
        // Gralha das Neves
        // ------------------------------------------------------------------

        public static void BuildSnowChough(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "SnowChough", 0.58f, DrawSnowChoughBody, new Vector3(0f, 0.32f, 0.3f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.22f, 0.22f, 0.24f), EnemySwatch.Black);
                    // The short yellow bill of the high choughs, frost on the crown.
                    b.Cylinder(new Vector3(0f, -0.01f, 0.1f), 0.04f, 0.012f, 0.13f, 5, EnemySwatch.Gold, rotation: Quaternion.Euler(98f, 0f, 0f));
                    b.Blob(new Vector3(0f, 0.09f, -0.02f), new Vector3(0.14f, 0.05f, 0.14f), EnemySwatch.GolemSnow, coarse: true);
                    HinokuraBeastRecipes.AddEyes(b, 0.07f, 0.03f, 0.07f, 0.045f, EnemySwatch.Crystal);
                },
                new Vector3(0f, -0.04f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.05f, 0.02f, 0.12f), EnemySwatch.Gold),
                EnemySwatch.Black, EnemySwatch.Shadow, EnemySwatch.GolemSnow, 0.9f, EnemySwatch.Black, EnemySwatch.GolemSnow);
        }

        private static void DrawSnowChoughBody(EnemyMeshBuilder b)
        {
            // The choughs of the high pass follow the caravans for the crumbs and fly ahead of them into the snow, so the caravan men know where the road is when it is white. The shadow got into the flock in a storm, and now they fly ahead to lead the caravans off the road, and throw frozen feathers at anyone who will not follow.
            b.Blob(Vector3.zero, new Vector3(0.46f, 0.44f, 0.68f), EnemySwatch.Black);
            b.Blob(new Vector3(0f, -0.06f, 0.1f), new Vector3(0.32f, 0.26f, 0.36f), EnemySwatch.Shadow, coarse: true);
            b.Segment(new Vector3(0f, 0.1f, 0.22f), new Vector3(0f, 0.3f, 0.28f), 0.08f, 0.07f, 6, EnemySwatch.Black);
            // The frost caked on its back from the storm, and a crumb of the caravans' bread still in its claws.
            b.Blob(new Vector3(0.04f, 0.2f, -0.1f), new Vector3(0.28f, 0.06f, 0.32f), EnemySwatch.GolemSnow, coarse: true);
            b.Blob(new Vector3(-0.1f, -0.3f, 0.1f), new Vector3(0.06f, 0.04f, 0.06f), EnemySwatch.SandstoneLight, coarse: true);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.58f, 0.1f, EnemySwatch.WarCape, EnemySwatch.Black);
        }

        // ------------------------------------------------------------------
        // Arqueiro de Terracota
        // ------------------------------------------------------------------

        public static void BuildTerracottaArcher(ProceduralEnemyBlueprint bp)
        {
            int body = bp.Mesh(ProceduralEnemyParts.Body, -1, new Vector3(0f, 0.9f, 0f), DrawTerracottaArcherBody);
            bp.Mesh(ProceduralEnemyParts.Head, body, new Vector3(0f, 0.42f, 0.02f), b =>
            {
                b.Blob(Vector3.zero, new Vector3(0.26f, 0.3f, 0.26f), EnemySwatch.SandstoneDark);
                // The topknot the clay archers were made with, set to one side, and the moustache cut into the clay.
                b.Blob(new Vector3(0.06f, 0.16f, -0.04f), new Vector3(0.12f, 0.1f, 0.12f), EnemySwatch.Black, coarse: true);
                b.Box(new Vector3(0f, -0.05f, 0.13f), new Vector3(0.12f, 0.02f, 0.02f), EnemySwatch.CopperDark);
                HinokuraBeastRecipes.AddEyes(b, 0.055f, 0.03f, 0.12f, 0.04f, EnemySwatch.Ember);
            });
            for (int s = -1; s <= 1; s += 2)
            {
                float side = s;
                bool right = s > 0;
                bp.Mesh(right ? ProceduralEnemyParts.ArmRight : ProceduralEnemyParts.ArmLeft, body, new Vector3(0.24f * side, 0.24f, 0.02f), b =>
                {
                    b.Segment(Vector3.zero, new Vector3(0.05f * side, -0.26f, 0.12f), 0.065f, 0.055f, 6, EnemySwatch.SandstoneDark);
                    b.Segment(new Vector3(0.05f * side, -0.26f, 0.12f), new Vector3(0.03f * side, -0.42f, 0.3f), 0.055f, 0.045f, 6, EnemySwatch.Sandstone);
                    b.Blob(new Vector3(0.03f * side, -0.45f, 0.32f), new Vector3(0.1f, 0.1f, 0.1f), EnemySwatch.SandstoneDark, coarse: true);
                    if (!right)
                    {
                        // The bow, its wood long rotted and the shadow holding its shape.
                        var grip = new Vector3(-0.03f, -0.45f, 0.36f);
                        b.Segment(grip + new Vector3(0f, 0.04f, 0f), grip + new Vector3(0f, 0.36f, -0.08f), 0.025f, 0.015f, 4, EnemySwatch.Shadow);
                        b.Segment(grip - new Vector3(0f, 0.04f, 0f), grip + new Vector3(0f, -0.36f, -0.08f), 0.025f, 0.015f, 4, EnemySwatch.Shadow);
                        b.Segment(grip + new Vector3(0f, 0.36f, -0.08f), grip + new Vector3(0f, -0.36f, -0.08f), 0.006f, 0.006f, 3, EnemySwatch.IvoryDark);
                    }
                    else
                    {
                        // The arrow it nocks, tipped with a spark of a watch-tower lantern.
                        b.Segment(new Vector3(0.03f, -0.45f, 0.3f), new Vector3(0.03f, -0.45f, 0.74f), 0.012f, 0.012f, 4, EnemySwatch.Bark);
                        b.Blob(new Vector3(0.03f, -0.45f, 0.78f), new Vector3(0.06f, 0.06f, 0.08f), EnemySwatch.StolenLight, coarse: true);
                    }
                }, Quaternion.Euler(-10f, 0f, 0f), meshKey: right ? "TerracottaArcherArm_R" : "TerracottaArcherArm_L");
            }
        }

        private static void DrawTerracottaArcherBody(EnemyMeshBuilder b)
        {
            // The clay army stands in the pits under the Wall to keep the Wall when the soldiers sleep, and the archers kneel at the front of every rank. Their knees cracked off in the cold; the shadow filled the break with its own smoke, and now they float up out of the pits and shoot from the watch-towers.
            b.Blob(Vector3.zero, new Vector3(0.48f, 0.5f, 0.36f), EnemySwatch.SandstoneDark);
            // The lacquered lamellae of the armour, row on row, and the short skirt.
            for (int i = 0; i < 4; i++)
            {
                b.Box(new Vector3(0f, 0.14f - i * 0.1f, 0.17f), new Vector3(0.42f - i * 0.02f, 0.06f, 0.04f), i % 2 == 0 ? EnemySwatch.Copper : EnemySwatch.CopperDark);
            }

            b.Cylinder(new Vector3(0f, -0.42f, 0f), 0.24f, 0.22f, 0.22f, 8, EnemySwatch.Sandstone, bottomCap: true);
            // The broken knees, and the smoke it floats on, poured out of the break down to the ground.
            for (int s = -1; s <= 1; s += 2)
            {
                b.Blob(new Vector3(0.1f * s, -0.44f, 0.06f), new Vector3(0.14f, 0.1f, 0.14f), EnemySwatch.SandstoneDark, coarse: true);
            }

            b.Segment(new Vector3(0f, -0.44f, 0f), new Vector3(0.02f, -0.8f, -0.06f), 0.2f, 0.09f, 6, EnemySwatch.Shroud);
            b.Blob(new Vector3(0.02f, -0.86f, -0.06f), new Vector3(0.42f, 0.08f, 0.4f), EnemySwatch.Shroud, coarse: true);
            // The quiver on its back and the shadow's soot along the cracks.
            b.Cylinder(new Vector3(0.12f, -0.1f, -0.2f), 0.07f, 0.07f, 0.4f, 6, EnemySwatch.Leather, rotation: Quaternion.Euler(0f, 0f, -16f), bottomCap: true);
            b.Blob(new Vector3(-0.12f, 0.06f, 0.14f), new Vector3(0.16f, 0.2f, 0.08f), EnemySwatch.Shadow, coarse: true);
        }

        // ------------------------------------------------------------------
        // Íbis de Luz Apagada
        // ------------------------------------------------------------------

        public static void BuildLightIbis(ProceduralEnemyBlueprint bp)
        {
            MidStandInBeastRecipes.BuildBird(bp, "LightIbis", 0.74f, DrawLightIbisBody, new Vector3(0f, 0.42f, 0.34f), b =>
                {
                    b.Blob(Vector3.zero, new Vector3(0.2f, 0.2f, 0.24f), EnemySwatch.Ivory);
                    // The little mirror of the white spires on its brow, and the long curved bill.
                    b.Cylinder(new Vector3(0f, 0.08f, 0.04f), 0.05f, 0.05f, 0.015f, 8, EnemySwatch.StolenLight, rotation: Quaternion.Euler(70f, 0f, 0f));
                    b.Segment(new Vector3(0f, -0.02f, 0.1f), new Vector3(0f, -0.08f, 0.26f), 0.03f, 0.022f, 5, EnemySwatch.Black);
                    b.Segment(new Vector3(0f, -0.08f, 0.26f), new Vector3(0f, -0.18f, 0.36f), 0.022f, 0.01f, 5, EnemySwatch.Black);
                    HinokuraBeastRecipes.AddEyes(b, 0.065f, 0.03f, 0.07f, 0.04f, EnemySwatch.Ember);
                },
                new Vector3(0f, -0.05f, 0.1f), b => b.Box(new Vector3(0f, -0.005f, 0.06f), new Vector3(0.04f, 0.02f, 0.12f), EnemySwatch.Black),
                EnemySwatch.Ivory, EnemySwatch.IvoryDark, EnemySwatch.Black, 1f, EnemySwatch.Ivory, EnemySwatch.IvoryDark);
        }

        private static void DrawLightIbisBody(EnemyMeshBuilder b)
        {
            // The ibises nest on the white spires and carry the light from mirror to mirror in the morning, a glint at a time, so the gardens wake together. The shadow dimmed the glint on their brows, and they carry it back the wrong way now and throw it at whoever walks the terraces.
            b.Blob(Vector3.zero, new Vector3(0.44f, 0.42f, 0.7f), EnemySwatch.Ivory);
            b.Blob(new Vector3(0f, -0.06f, 0.1f), new Vector3(0.32f, 0.26f, 0.38f), EnemySwatch.IvoryDark, coarse: true);
            // The long neck rising in an S.
            b.Segment(new Vector3(0f, 0.08f, 0.24f), new Vector3(0f, 0.26f, 0.28f), 0.07f, 0.055f, 6, EnemySwatch.Ivory);
            b.Segment(new Vector3(0f, 0.26f, 0.28f), new Vector3(0f, 0.4f, 0.34f), 0.055f, 0.05f, 6, EnemySwatch.Ivory);
            // The grey of the shadow over the white back, and the black wingtip plumes folded over the tail.
            b.Blob(new Vector3(-0.04f, 0.18f, -0.08f), new Vector3(0.3f, 0.06f, 0.36f), EnemySwatch.Shadow, coarse: true);
            b.Box(new Vector3(0f, 0.06f, -0.34f), new Vector3(0.26f, 0.06f, 0.14f), EnemySwatch.Black);
            MidStandInBeastRecipes.DrawBirdLegs(b, 0.74f, 0.09f, EnemySwatch.WarCape, EnemySwatch.Black);
        }
    }
}
