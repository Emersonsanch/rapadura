using Rapadura.Gameplay.Enemies.Visuals;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The body plans of the Regions 10–16 mounts (<see cref="MountFarRegionCatalog"/>): the same <see cref="QuadrupedShape"/> /
    /// <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/> path as every mount, with the extras of
    /// <see cref="MountWaveDecor"/> and, for the Íbis, the wings and beak of <see cref="MountSkyRecipes"/>. Deterministic.
    /// </summary>
    public static class MountFarRegionRecipes
    {
        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.GordiasGoat:
                    shape = new QuadrupedShape
                    {
                        // Gold fleece and a knotted red cord at the neck.
                        LegLength = 0.6f, BodyLength = 1.05f, BodyThickness = 0.46f, BodyWidth = 0.4f,
                        NeckLength = 0.32f, NeckPitch = 46f, HeadLength = 0.34f, HeadSize = 0.24f, EarLength = 0.14f,
                        TailLength = 0.16f, BushyTail = true, Hooves = true, Horns = MountHornStyle.Goat, Beard = true, Collar = true,
                        Coat = EnemySwatch.Gold, Dark = EnemySwatch.SandstoneDark, Light = EnemySwatch.SandstoneLight,
                        Mane = EnemySwatch.Gold, Socks = EnemySwatch.SandstoneDark, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.Horn,
                    };
                    return true;
                case MountVisualRecipe.AuroraReindeer:
                    shape = new QuadrupedShape
                    {
                        // Pale coat, antlers and a blanket in the colours of the aurora.
                        LegLength = 0.76f, BodyLength = 1.2f, BodyThickness = 0.46f, BodyWidth = 0.4f,
                        NeckLength = 0.46f, NeckPitch = 52f, HeadLength = 0.4f, HeadSize = 0.25f, EarLength = 0.15f,
                        TailLength = 0.16f, BushyTail = true, Hooves = true, Shaggy = true, Horns = MountHornStyle.Kirin,
                        Coat = EnemySwatch.GolemSnow, Dark = EnemySwatch.GolemStone, Light = EnemySwatch.Fang,
                        Mane = EnemySwatch.Fang, Socks = EnemySwatch.GolemStone, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.Crystal, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.StolenLight, Accent = EnemySwatch.StolenLight,
                    };
                    return true;
                case MountVisualRecipe.LacquerLion:
                    shape = new QuadrupedShape
                    {
                        // A guardian statue that walks: red lacquer, gold marks, black curled mane.
                        LegLength = 0.56f, BodyLength = 1.2f, BodyThickness = 0.52f, BodyWidth = 0.5f,
                        NeckLength = 0.2f, NeckPitch = 40f, HeadLength = 0.22f, HeadSize = 0.4f, EarLength = 0.1f,
                        TailLength = 0.6f, BushyTail = true, LegGirth = 1.4f, UprightMane = true, Jewel = true,
                        Pattern = MountCoatPattern.Scales, PatternSwatch = EnemySwatch.Gold, Accent = EnemySwatch.Gold,
                        Coat = EnemySwatch.WarCape, Dark = EnemySwatch.Black, Light = EnemySwatch.Gold,
                        Mane = EnemySwatch.Black, Socks = EnemySwatch.WarCape, Foot = EnemySwatch.Gold,
                        Blanket = EnemySwatch.Black, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.VolcanoGoat:
                    shape = new QuadrupedShape
                    {
                        // Soot-black herder's goat, ash belly, ember-red blanket.
                        LegLength = 0.54f, BodyLength = 0.96f, BodyThickness = 0.42f, BodyWidth = 0.38f,
                        NeckLength = 0.28f, NeckPitch = 44f, HeadLength = 0.32f, HeadSize = 0.22f, EarLength = 0.14f,
                        TailLength = 0.14f, BushyTail = true, Hooves = true, Horns = MountHornStyle.Goat, Beard = true,
                        Coat = EnemySwatch.Shadow, Dark = EnemySwatch.Black, Light = EnemySwatch.GolemStone,
                        Mane = EnemySwatch.Shadow, Socks = EnemySwatch.GolemStoneDark, Foot = EnemySwatch.Ember,
                        Blanket = EnemySwatch.Ember, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Horn, Accent = EnemySwatch.Horn,
                    };
                    return true;
                case MountVisualRecipe.MesektetIbis:
                    shape = new QuadrupedShape
                    {
                        // White body, black head, long neck, gold-tipped wings of the night boat. Two legs that fold in flight.
                        Biped = true, LegLength = 0.78f, BodyLength = 0.82f, BodyThickness = 0.46f, BodyWidth = 0.42f,
                        NeckLength = 0.6f, NeckPitch = 68f, HeadLength = 0.16f, HeadSize = 0.17f, EarLength = 0f,
                        TailLength = 0.28f, BushyTail = true,
                        FlightWings = true, WingSpan = 1.45f, Beak = true,
                        Coat = EnemySwatch.Fang, Dark = EnemySwatch.Black, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.Ivory, Socks = EnemySwatch.Black, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.Sandstone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                        Accent = EnemySwatch.Black, PatternSwatch = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.Mushhushshu:
                    shape = new QuadrupedShape
                    {
                        // The dragon-serpent of the glazed bricks: long scaly body and neck, short horns, long tail, blue and gold.
                        LegLength = 0.62f, BodyLength = 1.45f, BodyThickness = 0.5f, BodyWidth = 0.44f,
                        NeckLength = 0.56f, NeckPitch = 48f, HeadLength = 0.42f, HeadSize = 0.26f, EarLength = 0f,
                        TailLength = 1.2f, LegGirth = 1.2f, Horns = MountHornStyle.Kirin,
                        Pattern = MountCoatPattern.Scales, PatternSwatch = EnemySwatch.Gold, Accent = EnemySwatch.Horn,
                        Coat = EnemySwatch.Crystal, Dark = EnemySwatch.Shadow, Light = EnemySwatch.Gold,
                        Mane = EnemySwatch.Crystal, Socks = EnemySwatch.Shadow, Foot = EnemySwatch.Gold,
                        Blanket = EnemySwatch.Sandstone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.TaigaElk:
                    shape = new QuadrupedShape
                    {
                        // Tall and dark, long face, antlers the colour of cedar bark.
                        LegLength = 0.86f, BodyLength = 1.3f, BodyThickness = 0.56f, BodyWidth = 0.46f,
                        NeckLength = 0.36f, NeckPitch = 36f, HeadLength = 0.52f, HeadSize = 0.28f, EarLength = 0.17f,
                        TailLength = 0.1f, Hooves = true, LegGirth = 1.15f, Horns = MountHornStyle.Kirin, Shaggy = true,
                        Coat = EnemySwatch.BarkDark, Dark = EnemySwatch.Black, Light = EnemySwatch.Bark,
                        Mane = EnemySwatch.WolfFurDark, Socks = EnemySwatch.Bark, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.CaveMoss, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Horn, Accent = EnemySwatch.Bark,
                    };
                    return true;
                case MountVisualRecipe.HillRam:
                    shape = new QuadrupedShape
                    {
                        // A giant's ram: broad, woolly, huge curled horns, stone-hard head.
                        LegLength = 0.66f, BodyLength = 1.3f, BodyThickness = 0.72f, BodyWidth = 0.66f,
                        NeckLength = 0.22f, NeckPitch = 30f, HeadLength = 0.34f, HeadSize = 0.36f, EarLength = 0.13f,
                        TailLength = 0.14f, BushyTail = true, Hooves = true, LegGirth = 1.5f, Horns = MountHornStyle.Goat, Shaggy = true,
                        Coat = EnemySwatch.Bone, Dark = EnemySwatch.WolfFur, Light = EnemySwatch.WolfBelly,
                        Mane = EnemySwatch.Bone, Socks = EnemySwatch.WolfFur, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.Sandstone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.GolemStoneDark,
                    };
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }
    }
}
