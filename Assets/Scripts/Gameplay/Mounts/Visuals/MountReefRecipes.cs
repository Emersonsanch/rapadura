using Rapadura.Gameplay.Enemies.Visuals;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The body plan of <see cref="MountReefCatalog"/>: the Hipocampo de Kirta as a <see cref="QuadrupedShape"/> (a horse's chest and an upright
    /// neck, a long heavy tail that reads as the fish's tail, short legs, small ears that read as fins, verdigris scales with an ivory
    /// belly of nacre and a coral-pink fin mane). Same <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/> path as every
    /// mount, no new mesh code.
    /// </summary>
    public static class MountReefRecipes
    {
        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.KirtaHippocamp:
                    shape = new QuadrupedShape
                    {
                        // The hippocamp: horse front, upright arched neck, long fish tail, short legs, fin ears, scales, coral mane.
                        LegLength = 0.48f, BodyLength = 1.1f, BodyThickness = 0.46f, BodyWidth = 0.42f,
                        NeckLength = 0.5f, NeckPitch = 64f, HeadLength = 0.4f, HeadSize = 0.22f, EarLength = 0.1f,
                        TailLength = 1.1f, BushyTail = true, UprightMane = true, LegGirth = 1.1f,
                        Pattern = MountCoatPattern.Scales, PatternSwatch = EnemySwatch.Crystal, Accent = EnemySwatch.Pink,
                        Coat = EnemySwatch.Verdigris, Dark = EnemySwatch.CopperDark, Light = EnemySwatch.Ivory,
                        Mane = EnemySwatch.Pink, Socks = EnemySwatch.Verdigris, Foot = EnemySwatch.Ivory,
                        Blanket = EnemySwatch.Ivory, Saddle = EnemySwatch.Leather, Trim = EnemySwatch.Pink,
                    };
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }
    }
}
