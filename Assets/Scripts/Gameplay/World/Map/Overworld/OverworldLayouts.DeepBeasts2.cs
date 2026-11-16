namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The second half of the twenty-second pass of beasts (bible §3.2.1, <c>DeepOwn2BeastRecipes</c>): the last roles of the Limiar
        /// Profundo (Docs/Mapas100.md 83, 86, 88, 89) that still borrowed another region's beast after <see cref="DeepBeasts"/>: the Ushabti
        /// Apagado and the Fumaça of the fortress, the Redemoinho of the Pit of Ruinous Wind and the Rastejante of the Pit of Darkness (also
        /// on the Bridge of the Bōlos). The constants of the sheets in <c>OverworldLayouts.Profundo</c> point at them; each keeps its
        /// stand-in's biome and numbers.
        /// </summary>
        public static class DeepBeasts2
        {
            public const string RampartUshabti = Profundo.DimUshabti;
            public const string GateSmoke = Profundo.LampSmoke;
            public const string RuinWhirlwind = Profundo.Whirlwind;
            public const string DarkCrawler = Profundo.Creeper;
        }
    }
}
