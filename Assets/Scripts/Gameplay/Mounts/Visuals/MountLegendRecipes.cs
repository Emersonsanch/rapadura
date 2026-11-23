using Rapadura.Gameplay.Enemies.Visuals;

namespace Rapadura.Gameplay.Mounts.Visuals
{
    /// <summary>
    /// The body plan of <see cref="MountLegendCatalog"/>: the Barquinha de Luz as a <see cref="QuadrupedShape"/> read as a little boat (a long,
    /// wide, shallow hull of gold-white planks, the raised prow as the neck and head, a short stern as the tail, the jewel of light on the
    /// prow and four short legs of light that read as its oars). Same <see cref="MountRecipe.Build(ProceduralEnemyBlueprint, QuadrupedShape)"/>
    /// path as every mount, no new mesh code; it floats over the ground instead of walking (<see cref="MountLegendRules.Floats"/>).
    /// </summary>
    public static class MountLegendRecipes
    {
        public static bool TryShape(MountVisualRecipe recipe, out QuadrupedShape shape)
        {
            switch (recipe)
            {
                case MountVisualRecipe.LightSkiff:
                    shape = new QuadrupedShape
                    {
                        // The skiff: shallow wide hull, the prow raised like a hand, a gold band along the gunwale, oars of light.
                        LegLength = 0.42f, BodyLength = 1.5f, BodyThickness = 0.36f, BodyWidth = 0.62f,
                        NeckLength = 0.32f, NeckPitch = 62f, HeadLength = 0.22f, HeadSize = 0.2f, EarLength = 0f,
                        TailLength = 0.3f, LegGirth = 0.8f, Jewel = true,
                        Pattern = MountCoatPattern.Band, PatternSwatch = EnemySwatch.Gold, Accent = EnemySwatch.Crystal,
                        Coat = EnemySwatch.Ivory, Dark = EnemySwatch.IvoryDark, Light = EnemySwatch.GolemSnow,
                        Mane = EnemySwatch.Gold, Socks = EnemySwatch.Crystal, Foot = EnemySwatch.Crystal,
                        Blanket = EnemySwatch.Crystal, Saddle = EnemySwatch.Ivory, Trim = EnemySwatch.Gold,
                    };
                    return true;
                default:
                    shape = default;
                    return false;
            }
        }
    }
}
