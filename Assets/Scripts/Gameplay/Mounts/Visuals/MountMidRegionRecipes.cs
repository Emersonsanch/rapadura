using Rapadura.Gameplay.Enemies.Visuals;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The body plans of the Regions 5–9 mounts (<see cref="MountMidRegionCatalog"/>): the same <see cref="QuadrupedShape"/> /
    /// <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/> path as every mount, with the extras of
    /// <see cref="MountWaveDecor"/> (the turtle's shell and the dhow's <see cref="QuadrupedShape.Sail"/>). Deterministic.
    /// </summary>
    public static class MountMidRegionRecipes
    {
        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.DhowTurtle:
                    shape = new QuadrupedShape
                    {
                        // A green sea turtle: low wide body, flipper-thick legs, a dark shell with sand-gold scutes and a dhow's sail.
                        LegLength = 0.3f, BodyLength = 1.4f, BodyThickness = 0.46f, BodyWidth = 0.9f,
                        NeckLength = 0.28f, NeckPitch = 12f, HeadLength = 0.26f, HeadSize = 0.25f, EarLength = 0f,
                        TailLength = 0.1f, LegGirth = 1.9f, Shell = true, Sail = true,
                        Accent = EnemySwatch.CaveMoss, PatternSwatch = EnemySwatch.SandstoneLight,
                        Coat = EnemySwatch.Lichen, Dark = EnemySwatch.CaveMoss, Light = EnemySwatch.Ivory,
                        Mane = EnemySwatch.Lichen, Socks = EnemySwatch.Lichen, Foot = EnemySwatch.CaveMoss,
                        Blanket = EnemySwatch.Crystal, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    shape.SeatLift = MountWaveDecor.ShellSeatLift(shape);
                    return true;
                case MountVisualRecipe.TarchonFrog:
                    shape = new QuadrupedShape
                    {
                        // A giant swamp frog: squat wide body, big flat head, no ears or tail, mossy green with dark spots.
                        LegLength = 0.36f, BodyLength = 0.95f, BodyThickness = 0.5f, BodyWidth = 0.66f,
                        NeckLength = 0.08f, NeckPitch = 10f, HeadLength = 0.3f, HeadSize = 0.42f, EarLength = 0f,
                        TailLength = 0f, LegGirth = 1.5f,
                        Pattern = MountCoatPattern.Rosettes, PatternSwatch = EnemySwatch.BarkDark, Accent = EnemySwatch.Gold,
                        Coat = EnemySwatch.CaveMoss, Dark = EnemySwatch.BarkDark, Light = EnemySwatch.Lichen,
                        Mane = EnemySwatch.CaveMoss, Socks = EnemySwatch.CaveMoss, Foot = EnemySwatch.Lichen,
                        Blanket = EnemySwatch.Sandstone, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Horn,
                    };
                    return true;
                case MountVisualRecipe.RoofYak:
                    shape = new QuadrupedShape
                    {
                        // The caravan yak: black wool to the ground, white face, wide horns, packs and a bell of the passes.
                        LegLength = 0.6f, BodyLength = 1.35f, BodyThickness = 0.74f, BodyWidth = 0.68f,
                        NeckLength = 0.18f, NeckPitch = 22f, HeadLength = 0.36f, HeadSize = 0.32f, EarLength = 0.1f,
                        TailLength = 0.4f, BushyTail = true, Hooves = true, LegGirth = 1.45f, Horns = MountHornStyle.Bull, Shaggy = true,
                        Packs = true, Cowbell = true,
                        Coat = EnemySwatch.Shadow, Dark = EnemySwatch.Black, Light = EnemySwatch.Bone,
                        Mane = EnemySwatch.Black, Socks = EnemySwatch.Black, Foot = EnemySwatch.IronDark,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.Ivory,
                    };
                    return true;
                case MountVisualRecipe.WhiteLightStag:
                    shape = new QuadrupedShape
                    {
                        // A slender white stag, antlers and hooves of captured light, little stars of light along the flanks.
                        LegLength = 0.82f, BodyLength = 1.15f, BodyThickness = 0.44f, BodyWidth = 0.36f,
                        NeckLength = 0.5f, NeckPitch = 58f, HeadLength = 0.38f, HeadSize = 0.23f, EarLength = 0.16f,
                        TailLength = 0.14f, Hooves = true, Horns = MountHornStyle.Kirin, Jewel = true,
                        Pattern = MountCoatPattern.Stars, PatternSwatch = EnemySwatch.StolenLight, Accent = EnemySwatch.StolenLight,
                        Coat = EnemySwatch.Fang, Dark = EnemySwatch.Ivory, Light = EnemySwatch.StolenLight,
                        Mane = EnemySwatch.Fang, Socks = EnemySwatch.Ivory, Foot = EnemySwatch.StolenLight,
                        Blanket = EnemySwatch.Crystal, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold,
                    };
                    return true;
                case MountVisualRecipe.GoldenSteppeHorse:
                    shape = new QuadrupedShape
                    {
                        // The golden horse of the steppe: lean and long-legged, metallic gold coat, short pale mane, red felt blanket.
                        LegLength = 0.8f, BodyLength = 1.18f, BodyThickness = 0.42f, BodyWidth = 0.36f,
                        NeckLength = 0.56f, NeckPitch = 56f, HeadLength = 0.46f, HeadSize = 0.22f, EarLength = 0.14f,
                        TailLength = 0.6f, UprightMane = true, Hooves = true,
                        Coat = EnemySwatch.Gold, Dark = EnemySwatch.SandstoneDark, Light = EnemySwatch.SandstoneLight,
                        Mane = EnemySwatch.SandstoneLight, Socks = EnemySwatch.Gold, Foot = EnemySwatch.Black,
                        Blanket = EnemySwatch.WarCape, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Gold, Accent = EnemySwatch.Copper,
                    };
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }
    }
}
