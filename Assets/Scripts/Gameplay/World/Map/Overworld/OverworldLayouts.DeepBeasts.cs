namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-second pass of beasts (bible §3.2.1, <c>DeepOwnBeastRecipes</c>): the roles of the Rio das Doze Horas (Docs/Mapas100.md
        /// 74–76), the Margens da Mistura (78, 79) and the Limiar Profundo (84–90) that the continuous world still played with another region's
        /// beast, placed in the groups of their sheets in each region's layout (the constants of the sheets now point at them). Each keeps its
        /// stand-in's biome and numbers.
        /// </summary>
        public static class DeepBeasts
        {
            public const string CoilScorpion = Rio.RingScorpion;
            public const string DimmedUshabti = Rio.DimmedUshabti;
            public const string ShadowSalamander = Rio.Salamander;
            public const string BoiledShadow = Rio.Ooze;
            public const string CrackedUshabti = Rio.CrackedUshabti;
            public const string MudCrocodile = Rio.MudCrocodile;
            public const string ClockUshabti = Mistura.ClockUshabti;
            public const string PitchSmoke = Mistura.TarSmoke;
            public const string ChokingSmoke = Profundo.ChokeSmoke;
            public const string DarkShoal = Profundo.DarkShoal;
            public const string BlindSerpent = Profundo.BlindSerpent;
            public const string GiantDimmedUshabti = Profundo.GiantUshabti;
        }
    }
}
