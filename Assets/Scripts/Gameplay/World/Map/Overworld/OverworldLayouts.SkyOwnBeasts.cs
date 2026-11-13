namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The eighteenth pass of beasts (bible §3.2.1, <c>SkyOwnBeastRecipes</c>): the roles of the Reinos do Céu (Docs/Mapas100.md 91–99)
        /// that the continuous world still played with another region's beast, placed in the groups of their sheets in the Céu layout. Each
        /// keeps its stand-in's biome and numbers. The ids live as text in the <c>Ceu</c> constants (tools/balance reads them from there).
        /// </summary>
        public static class SkyOwnBeasts
        {
            public const string RunawayArchon = Ceu.RunawayArchon;
            public const string StubbornSpark = Ceu.StubbornSpark;
            public const string SphereSmoke = Ceu.SphereSmoke;
            public const string FlowerTreader = Ceu.FlowerShadow;
            public const string SmokeThorn = Ceu.SmokeThorn;
            public const string EmberStowaway = Ceu.EmberStowaway;
            public const string NoonShadow = Ceu.NoonShadow;
            public const string LastShadow = Ceu.LastShadow;
            public const string HighWind = Ceu.HighWind;
            public const string FledArchon = Ceu.FledArchon;
        }
    }
}
