namespace Rapadura.Gameplay.Refining
{
    /// <summary>
    /// Item ids (<c>Resources/Items</c>) of the "Pedras da Luz Presa" and the Amuleto da Sorte. Stackable materials (1×1 on
    /// the Mochila grid); their names/descriptions live in <see cref="RefineTexts"/> (<c>item.&lt;id&gt;.*</c>).
    /// </summary>
    public static class RefineItems
    {
        public const string BlessingStone = "item_pedra_bencao";
        public const string SoulStone = "item_pedra_alma";
        public const string LifeStone = "item_pedra_vida";
        public const string ChaosStone = "item_pedra_caos";
        public const string LuckCharm = "item_amuleto_sorte";

        /// <summary>The currency the high refines (+9 → +15) are paid in (same as the shops).</summary>
        public const string Gold = "item_gold";

        /// <summary>Every refine item, in the order drops are rolled (<see cref="RefineStoneDrops"/>).</summary>
        public static readonly string[] All = { BlessingStone, SoulStone, LifeStone, ChaosStone, LuckCharm };

        public static string IdOf(RefineStone stone)
        {
            switch (stone)
            {
                case RefineStone.Blessing: return BlessingStone;
                case RefineStone.Soul: return SoulStone;
                case RefineStone.Life: return LifeStone;
                case RefineStone.Chaos: return ChaosStone;
                default: return null;
            }
        }
    }
}
