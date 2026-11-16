namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-first pass of beasts (bible §3.2.1, <c>NorthOwnBeastRecipes</c>): the most-seen roles of the Picos de Farnkesh
        /// (Docs/Mapas100.md 57–58), the Mar Gelado (60–64) and the Arquipélago de Hinokura (68–70) that the continuous world still played with
        /// another beast, placed in the groups of their sheets in the Farnkesh, Mar Gelado and Hinokura layouts (their region constants now
        /// point here). Each keeps its stand-in's biome and numbers.
        /// </summary>
        public static class NorthBeasts
        {
            public const string FlyingStone = Fark.Stone;
            public const string GorgeAys = Fark.Ays;
            public const string TrappedLightning = Fark.Lightning;
            public const string IceFish = Gelo.IceFish;
            public const string LanternEel = Gelo.Eel;
            public const string TundraMosquito = Gelo.Mosquito;
            public const string DimmedNarwhal = Gelo.Narwhal;
            public const string StormGull = Gelo.StormGull;
            public const string WitchFish = Gelo.WitchFish;
            public const string ObsidianOctopus = Fogo.Octopus;
            public const string RagingSpark = Fogo.Spark;
            public const string LivingAsh = Fogo.Ash;
        }
    }
}
