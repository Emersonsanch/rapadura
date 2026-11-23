namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-ninth pass of beasts (bible §3.2.1, <c>NorthOwn2BeastRecipes</c>): the roles of the sheets 57–100 that the continuous world
        /// still played with another region's beast after the twenty-eighth (<see cref="PeaksOwnBeasts"/>), the Ventania (58), the Rachadura (60),
        /// the Cristal Vivo and the Raiz-Gelo (62), the Carpa de Brasa and the Bambu de Fumaça (66), the Fumarola (67) and the Fole Vivo (69), and
        /// the Guarda de Taowu of the Cidadela dos Quatro Gulosos (46), placed in the groups of their sheets in the Farnkesh, Mar Gelado, Hinokura
        /// and Mingcheng layouts (their region constants now point here). Each keeps its stand-in's biome and numbers.
        /// </summary>
        public static class NorthOwn2Beasts
        {
            public const string Gale = Fark.Gale;
            public const string IceCrack = Gelo.IceCrack;
            public const string LivingCrystal = Gelo.Crystal;
            public const string IceRoot = Gelo.IceRoot;
            public const string EmberCarp = Fogo.Carp;
            public const string SmokeBamboo = Fogo.Bamboo;
            public const string Fumarole = Fogo.Fumarole;
            public const string LivingBellows = Fogo.Bellows;
            public const string TaowuGuard = Ming.TaowuGuard;
        }
    }
}
