namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-fourth pass of beasts (bible §3.2.1, <c>WestOwnBeastRecipes</c>): the most-seen roles of the Costa das Monções
        /// (Docs/Mapas100.md 28–30), the Delta de Jhilpur (31–35), the Planície de Mingcheng (42–44) and the Estepe dos Nartes (51) that the
        /// continuous world still played with another beast, placed in the groups of their sheets in the Costa, Delta, Mingcheng and Nartes
        /// layouts (their region constants now point here). Each keeps its stand-in's biome and numbers.
        /// </summary>
        public static class WestOwnBeasts
        {
            public const string MudShark = Coast.MudShark;
            public const string SmokeRay = Coast.SmokeRay;
            public const string DimmedHeron = Coast.DimmedHeron;
            public const string MudGharial = Delta.MudGharial;
            public const string NinkiHatchling = Delta.NinkiHatchling;
            public const string RottenRoot = Delta.RottenRoot;
            public const string FishLantern = Ming.LanternFish;
            public const string SmokeBoatman = Ming.Boatman;
            public const string MandarinDuck = Ming.Duck;
            public const string DimmedScarecrow = Ming.Scarecrow;
            public const string CrookedRay = Ming.CrookedRay;
            public const string GreyWolf = Nart.Wolf;
        }
    }
}
