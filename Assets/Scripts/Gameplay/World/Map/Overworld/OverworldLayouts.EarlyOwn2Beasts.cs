namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-sixth pass of beasts (bible §3.2.1, <c>EarlyOwn2BeastRecipes</c>): the roles of Regions 2 and 3 (Docs/Mapas100.md 09, 12, 15,
        /// 17 and 18) that the continuous world still played with another beast after <see cref="EarlyOwnBeasts"/>, placed in the groups of their
        /// sheets in the Areias and Kaggen layouts, so the trails copy them too. Each keeps its stand-in's biome and numbers. The Águia da
        /// Tempestade of the Cume dos Mazans (18) is the Farnkesh's own (<see cref="StormEagle"/>), not a new beast.
        /// </summary>
        public static class EarlyOwn2Beasts
        {
            public const string ThirstyRoot = "enemy_desert_thirsty_root";
            public const string CrystalScorpion = "enemy_desert_crystal_scorpion";
            public const string SaltSerpent = "enemy_desert_salt_serpent";
            public const string BlackEagle = "enemy_mountain_dimmed_black_eagle";
            public const string ColdSalamander = "enemy_cave_cold_salamander";

            /// <summary>The Águia da Tempestade (the Farnkesh's own beast) now also stands for the storm eagles of the Cume dos Mazans (18).</summary>
            public const string StormEagle = "enemy_mountain_storm_eagle";
        }
    }
}
