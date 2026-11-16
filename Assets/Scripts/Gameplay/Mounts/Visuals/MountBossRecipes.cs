using Rapadura.Gameplay.Enemies.Visuals;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The body plans of <see cref="MountBossCatalog"/>: the same <see cref="QuadrupedShape"/> /
    /// <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/> path as every mount, only with the extras that already
    /// exist (<see cref="MountWaveDecor"/>: horns, tusks, scales, flames, shaggy wool, the eland's dewlap of <see cref="MountSheetRecipes"/>),
    /// so no new mesh code and the triangle count stays close to the species they borrow from. Deterministic.
    /// </summary>
    public static class MountBossRecipes
    {
        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.LaurelLizard:
                    shape = new QuadrupedShape
                    {
                        // A giant laurel-forest lizard: low and long, brown scales with green flecks, a long tail, a blunt head.
                        LegLength = 0.32f, BodyLength = 1.3f, BodyThickness = 0.34f, BodyWidth = 0.46f,
                        NeckLength = 0.14f, NeckPitch = 10f, HeadLength = 0.34f, HeadSize = 0.26f, EarLength = 0f,
                        TailLength = 1.0f, LegGirth = 1.25f,
                        Pattern = MountCoatPattern.Scales, PatternSwatch = EnemySwatch.CaveMoss, Accent = EnemySwatch.Lichen,
                        Coat = EnemySwatch.Bark, Dark = EnemySwatch.BarkDark, Light = EnemySwatch.Lichen,
                        Mane = EnemySwatch.BarkDark, Socks = EnemySwatch.Bark, Foot = EnemySwatch.BarkDark,
                        Blanket = EnemySwatch.CaveMoss, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.JhilpurBuffalo:
                    shape = new QuadrupedShape
                    {
                        // A grey water buffalo: wide low body, thick legs, horns swept out and back, mud on the legs, a reed blanket.
                        LegLength = 0.6f, BodyLength = 1.45f, BodyThickness = 0.7f, BodyWidth = 0.64f,
                        NeckLength = 0.18f, NeckPitch = 14f, HeadLength = 0.4f, HeadSize = 0.32f, EarLength = 0.12f, FloppyEars = true,
                        TailLength = 0.5f, Hooves = true, LegGirth = 1.5f, Horns = MountHornStyle.Bull,
                        Coat = EnemySwatch.GolemStoneDark, Dark = EnemySwatch.Shadow, Light = EnemySwatch.GolemStone,
                        Mane = EnemySwatch.Shadow, Socks = EnemySwatch.BarkDark, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.Lichen, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Copper, Accent = EnemySwatch.Horn,
                    };
                    return true;
                case MountVisualRecipe.PamirArgali:
                    shape = new QuadrupedShape
                    {
                        // A great wild sheep: tawny back, pale belly and ruff, long legs, huge horns curled in a full turn.
                        LegLength = 0.72f, BodyLength = 1.2f, BodyThickness = 0.52f, BodyWidth = 0.46f,
                        NeckLength = 0.3f, NeckPitch = 36f, HeadLength = 0.34f, HeadSize = 0.3f, EarLength = 0.1f,
                        TailLength = 0.14f, BushyTail = true, Hooves = true, LegGirth = 1.15f, Horns = MountHornStyle.Goat, Dewlap = true,
                        Pattern = MountCoatPattern.Band, PatternSwatch = EnemySwatch.Ivory, Accent = EnemySwatch.Horn,
                        Coat = EnemySwatch.Sandstone, Dark = EnemySwatch.SandstoneDark, Light = EnemySwatch.Ivory,
                        Mane = EnemySwatch.Ivory, Socks = EnemySwatch.Ivory, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.FreedSmokeHorse:
                    shape = new QuadrupedShape
                    {
                        // The freed horse: lean dapple grey, a dark upright mane like the last of the smoke, a felt blanket of the steppe.
                        LegLength = 0.78f, BodyLength = 1.2f, BodyThickness = 0.44f, BodyWidth = 0.38f,
                        NeckLength = 0.54f, NeckPitch = 54f, HeadLength = 0.46f, HeadSize = 0.23f, EarLength = 0.14f,
                        TailLength = 0.7f, UprightMane = true, Hooves = true,
                        Pattern = MountCoatPattern.Patches, PatternSwatch = EnemySwatch.GolemStoneDark, Accent = EnemySwatch.Shroud,
                        Coat = EnemySwatch.GolemStone, Dark = EnemySwatch.GolemStoneDark, Light = EnemySwatch.GolemSnow,
                        Mane = EnemySwatch.Shroud, Socks = EnemySwatch.GolemStoneDark, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.TameNinkiNanka:
                    shape = new QuadrupedShape
                    {
                        // The shy dragoness of the bog: long scaled body low on the legs, a long neck and tail, short horns, reed-green and gold.
                        LegLength = 0.5f, BodyLength = 1.5f, BodyThickness = 0.48f, BodyWidth = 0.5f,
                        NeckLength = 0.5f, NeckPitch = 40f, HeadLength = 0.44f, HeadSize = 0.26f, EarLength = 0f,
                        TailLength = 1.2f, LegGirth = 1.3f, Horns = MountHornStyle.Kirin,
                        Pattern = MountCoatPattern.Scales, PatternSwatch = EnemySwatch.Gold, Accent = EnemySwatch.Lichen,
                        Coat = EnemySwatch.CaveMoss, Dark = EnemySwatch.BarkDark, Light = EnemySwatch.Lichen,
                        Mane = EnemySwatch.Lichen, Socks = EnemySwatch.CaveMoss, Foot = EnemySwatch.Gold,
                        Blanket = EnemySwatch.Sandstone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.LivingEmberBoar:
                    shape = new QuadrupedShape
                    {
                        // A big black forge boar: deep chest, short legs, curved tusks, living embers along the bristles of the spine.
                        LegLength = 0.5f, BodyLength = 1.25f, BodyThickness = 0.62f, BodyWidth = 0.52f,
                        NeckLength = 0.14f, NeckPitch = 12f, HeadLength = 0.44f, HeadSize = 0.34f, EarLength = 0.12f,
                        TailLength = 0.12f, Hooves = true, LegGirth = 1.4f, Tusks = true, FlameCrest = true,
                        PatternSwatch = EnemySwatch.Ember, Accent = EnemySwatch.Gold,
                        Coat = EnemySwatch.Shadow, Dark = EnemySwatch.Black, Light = EnemySwatch.PeccaryFur,
                        Mane = EnemySwatch.Black, Socks = EnemySwatch.Shadow, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Copper,
                    };
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }
    }
}
