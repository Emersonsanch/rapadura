namespace Rapadura.Gameplay.World.Map.Overworld
{
    public static partial class OverworldLayouts
    {
        /// <summary>
        /// The twenty-third pass of beasts (bible §3.2.1, <c>EarlyOwnBeastRecipes</c>): the roles of Regions 1–4 (Docs/Mapas100.md 03–06, 09, 16, 19,
        /// 20) that the continuous world still played with another beast after <see cref="EarlyBeasts"/>, placed in the groups of their sheets in the
        /// Zarafshan, Areias, Kaggen and Limiar layouts and in the clearings of the hand-made maps embedded there (MapLayouts.Zarafshan / Limiar),
        /// so the trails copy them too. Each keeps its stand-in's biome and numbers. The Sombra Pequena of the Salão de Tages (20) is the Limiar
        /// Profundo's own (<see cref="LittleShadow"/>), not a new beast.
        /// </summary>
        public static class EarlyOwnBeasts
        {
            public const string MudEel = "enemy_forest_mud_eel";
            public const string CocoonCaterpillar = "enemy_forest_cocoon_caterpillar";
            public const string ThreadJackdaw = "enemy_forest_thread_jackdaw";
            public const string StickyMist = "enemy_forest_sticky_mist";
            public const string ThornRose = "enemy_forest_dimmed_thornrose";
            public const string AshWolf = "enemy_forest_ash_wolf";
            public const string GrottoSpider = "enemy_forest_grotto_spider";
            public const string BladeStone = "enemy_mountain_blade_stone";
            public const string StoneLizard = "enemy_forest_stone_lizard";
            public const string KarezRat = "enemy_cave_karez_rat";
            public const string WaterScorpion = "enemy_desert_water_scorpion";

            /// <summary>The Sombra Pequena (83's own beast) now also stands for the Sombra Pequena of the Salão de Tages (20).</summary>
            public const string LittleShadow = "enemy_forest_little_shadow";
        }
    }
}
