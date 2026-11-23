namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The moon counter of the Porto da Lua Branca (Docs/Mapas100.md 59 and Cities.md: "rotas e lojas que mudam com a lua"), pure and
    /// server-runnable; the scene side is <c>CityStallsMechanicsDriver.Porto.cs</c>. Makarios, the Keeper of the Lanterns, opens a counter at his
    /// door whose goods follow the moon of <see cref="MoonCanalRules"/> (8 in-game days, day 1 = new, day 5 = full): two bundles per moon, each
    /// once per in-game day, what the ice lets the boats bring in that night.
    /// <code>
    /// 0 new moon      Lamparina Rúnica 110 (160) · 3 Runas de Clareza 32 (45)        — the dark: lamps for the galleries
    /// 1 crescent      3 Chás de Gengibre 150 (210) · 2 Iscas de Prata do Kul 56 (80)
    /// 2 first quarter 2 Cordas de Escalada 56 (80) · 3 Pedras da Bênção 25 (36)
    /// 3 waxing        2 Mantas de Lã 64 (90) · 2 Runas de Vento 28 (40)              — the canals start to freeze
    /// 4 full moon     2 Pedras da Alma 48 (70) · 1 Amuleto da Sorte 17 (25)          — the white moon: only now the Soul stones
    /// 5 waning        3 Garrafas de Ar Sereno 63 (90) · Roupa Térmica 700 (900)
    /// 6 last quarter  Passagem de Dhow 220 (300) · Barco de Junco 450 (600)          — the canals open again: boats
    /// 7 old moon      Grampos de Escalada 760 (950) · Capa de Tempestade 880 (1100)
    /// </code>
    /// About 30 % off Pamour's prices (in brackets) and the full moon is the one night the Pedras da Alma reach the north. Saved: the in-game day
    /// each line was last bought (a daily mark, written only on a purchase).
    /// </summary>
    public static class PortoMoonStallRules
    {
        public const float Reach = 2.6f;
        public const int LinesPerMoon = 2;

        public const string LineKeyPrefix = "porto.makarios.moon_line.";

        /// <summary>The bundles of each moon (index = <see cref="MoonCanalRules.MoonIndex"/>), two each.</summary>
        public static readonly TunnelFairLine[][] Lines =
        {
            new[] { new TunnelFairLine("item_lamparina_runica", 1, 110, 160), new TunnelFairLine("item_runa_clareza", 3, 32, 45) },
            new[] { new TunnelFairLine("item_cha_gengibre", 3, 150, 210), new TunnelFairLine("item_isca_prata_kul", 2, 56, 80) },
            new[] { new TunnelFairLine("item_corda_escalada", 2, 56, 80), new TunnelFairLine("item_pedra_bencao", 3, 25, 36) },
            new[] { new TunnelFairLine("item_manta_la", 2, 64, 90), new TunnelFairLine("item_runa_vento", 2, 28, 40) },
            new[] { new TunnelFairLine("item_pedra_alma", 2, 48, 70), new TunnelFairLine("item_amuleto_sorte", 1, 17, 25) },
            new[] { new TunnelFairLine("item_garrafa_ar_sereno", 3, 63, 90), new TunnelFairLine("item_roupa_termica", 1, 700, 900) },
            new[] { new TunnelFairLine("item_passagem_dhow", 1, 220, 300), new TunnelFairLine("item_barco_junco", 1, 450, 600) },
            new[] { new TunnelFairLine("item_grampos_escalada", 1, 760, 950), new TunnelFairLine("item_capa_tempestade", 1, 880, 1100) },
        };

        public static TunnelFairLine[] LinesOf(int day) => Lines[MoonCanalRules.MoonIndex(day)];

        public static string LineKey(int moon, int line) => LineKeyPrefix + moon + "." + line;

        /// <summary>Once per in-game day per line (the same check as the gallery fair).</summary>
        public static TunnelStallCheck Check(TunnelFairLine line, int boughtDay, int day, int gold) => TunnelMarketStallRules.CheckFairLine(line, boughtDay, day, gold);
    }
}
