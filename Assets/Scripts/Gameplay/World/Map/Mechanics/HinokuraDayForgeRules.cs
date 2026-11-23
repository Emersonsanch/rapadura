namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// The forge of the day of Hinokura (Docs/Mapas100.md 65: "abrir e fechar comportas muda qual forja funciona (refino, fusão, conserto ficam
    /// melhores na forja acesa do dia)"; Cities.md: "o bônus de refino da forja acesa do dia"), pure and server-runnable. Each UTC day one of the
    /// three forges fed by the sluices (<see cref="HinokuraForgeRules"/>) is the forge of the day, the same for everyone; while that one burns
    /// (its sluice opened, 20 min, until a Tremor puts it out) every refine that can fail gets +5 % (on top of the Grande Forja, never past
    /// the 95 % cap of <c>RefineRules</c>). Kurogane at the plaza's Forja de Hinokura says which one it is and whether it burns. Nothing new is
    /// saved: it reads the moments the sluices were opened, which the sluices already keep.
    /// <code>
    /// forge of the day: one of 3 by the UTC day, never the same two days running · +5 % refine while it is lit
    /// </code>
    /// Decision: refine only (the repair and the fusion keep their own city prices); the bonus counts at any smith while it burns, because the
    /// 20 minutes are the walk back from the sluice and the Tremor can still put it out on the way: the risk is the game.
    /// </summary>
    public static class HinokuraDayForgeRules
    {
        public const float RefineBonusLit = 0.05f;

        /// <summary>Metres from the plaza's Forja de Hinokura where Kurogane's line shows.</summary>
        public const float PlazaReach = 4.5f;

        /// <summary>The forge of the day (0 … <see cref="HinokuraForgeRules.Forges"/> − 1) of a UTC day.</summary>
        public static int ForgeOfDay(long day)
        {
            int today = TunnelMarketStallRules.Pick(day * 5L + 2L, HinokuraForgeRules.Forges);
            int yesterday = TunnelMarketStallRules.Pick((day - 1L) * 5L + 2L, HinokuraForgeRules.Forges);
            return today != yesterday ? today : (today + 1) % HinokuraForgeRules.Forges;
        }

        /// <summary>Whether the forge of the day burns at <paramref name="unix"/> (<paramref name="litAt"/>: the sluices' saved moments).</summary>
        public static bool DayForgeLit(long[] litAt, long unix)
        {
            int forge = ForgeOfDay(CityServiceRules.DayOf(unix));
            return litAt != null && forge < litAt.Length && HinokuraForgeRules.IsLit(forge, litAt[forge], unix);
        }

        /// <summary>The refine chance the forge of the day adds now (0 when it does not burn).</summary>
        public static float RefineBonus(long[] litAt, long unix) => DayForgeLit(litAt, unix) ? RefineBonusLit : 0f;

        /// <summary><see cref="RefineBonus(long[], long)"/> for a save (0 without one).</summary>
        public static float RefineBonus(WorldMapProgress progress, long unix)
        {
            if (progress == null)
            {
                return 0f;
            }

            var litAt = new long[HinokuraForgeRules.Forges];
            HinokuraMechanicsDriver.ReadForges(progress, litAt);
            return RefineBonus(litAt, unix);
        }
    }
}
