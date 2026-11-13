using Rapadura.Gameplay.Enemies.Visuals;
using UnityEngine;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The body plans of the map-sheet mounts (<see cref="MountSheetCatalog"/>): the same <see cref="QuadrupedShape"/> /
    /// <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/> path as every mount, with the extras of
    /// <see cref="MountWaveDecor"/> and three of their own drawn here: the heron's straight <see cref="QuadrupedShape.Bill"/>,
    /// the camel's <see cref="QuadrupedShape.Humps"/> and the eland's <see cref="QuadrupedShape.Dewlap"/>. Deterministic.
    /// </summary>
    public static class MountSheetRecipes
    {
        public const string Prefix = "Sheet_";

        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.MarshHeron:
                    shape = new QuadrupedShape
                    {
                        // A grey heron: two very long yellow legs, a long S neck, folded wings, a dagger bill and a black crest.
                        Biped = true, LegLength = 0.95f, BodyLength = 0.8f, BodyThickness = 0.42f, BodyWidth = 0.4f,
                        NeckLength = 0.7f, NeckPitch = 70f, HeadLength = 0.14f, HeadSize = 0.15f, EarLength = 0f,
                        TailLength = 0.22f, BushyTail = true, Wings = true, Bill = true,
                        Coat = EnemySwatch.GolemStone, Dark = EnemySwatch.GolemStoneDark, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.Black, Socks = EnemySwatch.Sandstone, Foot = EnemySwatch.SandstoneDark,
                        Blanket = EnemySwatch.CaveMoss, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.OchreWarWolf:
                    shape = new QuadrupedShape
                    {
                        // A big grey-brown war wolf with stripes of red ochre on the flanks and a bronze-studded collar.
                        LegLength = 0.6f, BodyLength = 1.15f, BodyThickness = 0.46f, BodyWidth = 0.42f,
                        NeckLength = 0.26f, NeckPitch = 32f, HeadLength = 0.36f, HeadSize = 0.3f, EarLength = 0.18f,
                        TailLength = 0.5f, BushyTail = true, UprightMane = true, Collar = true, LegGirth = 1.1f,
                        Pattern = MountCoatPattern.Stripes, PatternSwatch = EnemySwatch.WarCape, Accent = EnemySwatch.Copper,
                        Coat = EnemySwatch.WolfFur, Dark = EnemySwatch.WolfFurDark, Light = EnemySwatch.WolfBelly,
                        Mane = EnemySwatch.WolfFurDark, Socks = EnemySwatch.WolfFurDark, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Copper,
                    };
                    return true;
                case MountVisualRecipe.TallGrassEland:
                    shape = new QuadrupedShape
                    {
                        // A tawny eland: heavy body on slim legs, thin white stripes, twisted horns swept back, the dewlap.
                        LegLength = 0.8f, BodyLength = 1.35f, BodyThickness = 0.6f, BodyWidth = 0.5f,
                        NeckLength = 0.38f, NeckPitch = 40f, HeadLength = 0.44f, HeadSize = 0.25f, EarLength = 0.14f,
                        TailLength = 0.45f, Hooves = true, LegGirth = 1.1f, Horns = MountHornStyle.Goat, Dewlap = true,
                        Pattern = MountCoatPattern.Stripes, PatternSwatch = EnemySwatch.Bone, Accent = EnemySwatch.Shadow,
                        Coat = EnemySwatch.Sandstone, Dark = EnemySwatch.SandstoneDark, Light = EnemySwatch.SandstoneLight,
                        Mane = EnemySwatch.SandstoneDark, Socks = EnemySwatch.SandstoneDark, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.LanternOlm:
                    shape = new QuadrupedShape
                    {
                        // A giant blind cave salamander: low, long and pale, a flat wide head, a long tail, spots of cold light.
                        LegLength = 0.3f, BodyLength = 1.5f, BodyThickness = 0.36f, BodyWidth = 0.5f,
                        NeckLength = 0.12f, NeckPitch = 6f, HeadLength = 0.36f, HeadSize = 0.3f, EarLength = 0f,
                        TailLength = 1.1f, LegGirth = 1.3f,
                        Pattern = MountCoatPattern.Rosettes, PatternSwatch = EnemySwatch.StolenLight, Accent = EnemySwatch.Pink,
                        Coat = EnemySwatch.Fang, Dark = EnemySwatch.CaveBelly, Light = EnemySwatch.Pink,
                        Mane = EnemySwatch.Fang, Socks = EnemySwatch.Fang, Foot = EnemySwatch.Pink,
                        Blanket = EnemySwatch.Crystal, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.StolenLight,
                    };
                    return true;
                case MountVisualRecipe.ArattaAurochs:
                    shape = new QuadrupedShape
                    {
                        // A black aurochs: deep chest, wide forward horns, pale stripe down the back, race ribbons on the blanket.
                        LegLength = 0.66f, BodyLength = 1.4f, BodyThickness = 0.72f, BodyWidth = 0.62f,
                        NeckLength = 0.2f, NeckPitch = 24f, HeadLength = 0.4f, HeadSize = 0.34f, EarLength = 0.12f,
                        TailLength = 0.55f, Hooves = true, LegGirth = 1.45f, Horns = MountHornStyle.Bull,
                        Pattern = MountCoatPattern.Band, PatternSwatch = EnemySwatch.Horn, Accent = EnemySwatch.Horn,
                        Coat = EnemySwatch.Black, Dark = EnemySwatch.Shadow, Light = EnemySwatch.Horn,
                        Mane = EnemySwatch.Shadow, Socks = EnemySwatch.Shadow, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.Gold, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.WarCape,
                    };
                    return true;
                case MountVisualRecipe.WhiteBactrian:
                    shape = new QuadrupedShape
                    {
                        // A white two-humped camel: long legs, a long curved neck, shaggy humps, the caravan's packs.
                        LegLength = 0.86f, BodyLength = 1.3f, BodyThickness = 0.52f, BodyWidth = 0.48f,
                        NeckLength = 0.56f, NeckPitch = 38f, HeadLength = 0.44f, HeadSize = 0.24f, EarLength = 0.08f,
                        TailLength = 0.3f, Hooves = true, LegGirth = 1.15f, Humps = true, Packs = true,
                        Coat = EnemySwatch.Fang, Dark = EnemySwatch.Bone, Light = EnemySwatch.Ivory,
                        Mane = EnemySwatch.Bone, Socks = EnemySwatch.Bone, Foot = EnemySwatch.SandstoneDark,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.Gold,
                    };
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }

        /// <summary>Adds the bill, the humps and the dewlap (nothing for the other mounts).</summary>
        public static void Decorate(ProceduralEnemyBlueprint bp, QuadrupedShape s, int body, int head)
        {
            if (s.Bill)
            {
                bp.Mesh(Prefix + "Bill", head, Vector3.zero, b => DrawBill(b, s));
            }

            if (s.Humps || s.Dewlap)
            {
                bp.Mesh(Prefix + "Body", body, Vector3.zero, b => DrawBodyExtras(b, s));
            }
        }

        private static void DrawBill(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float size = s.HeadSize;

            // A long straight dagger bill, black eyes and a thin crest swept back.
            b.Horn(new Vector3(0f, -size * 0.02f, size * 0.4f), 0.04f, 0.34f, 2, 0f, s.Accent, Quaternion.Euler(92f, 0f, 0f));
            for (int side = -1; side <= 1; side += 2)
            {
                b.Box(new Vector3(size * 0.33f * side, size * 0.14f, size * 0.2f), new Vector3(0.03f, 0.04f, 0.04f), EnemySwatch.Black);
            }

            b.Box(new Vector3(0f, size * 0.3f, -size * 0.45f), new Vector3(0.025f, 0.025f, 0.24f), s.Mane, Quaternion.Euler(-12f, 0f, 0f));
        }

        private static void DrawBodyExtras(EnemyMeshBuilder b, QuadrupedShape s)
        {
            float l = s.BodyLength;
            float t = s.BodyThickness;
            float w = s.BodyWidth;

            if (s.Humps)
            {
                // One hump before and one behind the saddle, with a tuft of long hair on each.
                foreach (float z in new[] { l * 0.3f, -l * 0.3f })
                {
                    b.Blob(new Vector3(0f, t * 0.55f, z), new Vector3(w * 0.62f, t * 0.6f, l * 0.22f), s.Coat, coarse: true);
                    b.Blob(new Vector3(0f, t * 0.82f, z), new Vector3(w * 0.4f, t * 0.22f, l * 0.16f), s.Mane, coarse: true);
                }
            }

            if (s.Dewlap)
            {
                // The loose fold of skin hanging under the throat and the chest.
                b.Blob(new Vector3(0f, -t * 0.3f, l * 0.44f), new Vector3(0.09f, t * 0.5f, 0.24f), s.Coat, coarse: true);
            }
        }
    }
}
