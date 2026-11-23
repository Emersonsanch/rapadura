using System;

namespace Rapadura.Gameplay.Items.Durability
{
    /// <summary>
    /// The cheaper repair of some city forges (Docs/Mapas100.md §3.3 and map 11: the Forja de Tambor of the Tambor Rubro repairs
    /// 20 % cheaper). Pure and server-runnable: <see cref="RepairService"/> takes the discount as a number, the shop window and the
    /// ferreiro window ask here which one applies. The discount is taken off each piece's own price (rounded up, at least 1 gold),
    /// so "repair all" is always the sum of the pieces.
    /// </summary>
    public static class RepairDiscountRules
    {
        /// <summary>The Forja de Tambor's shop id (= <c>CityShopCatalog.ForjaTambor</c>).</summary>
        public const string ForjaTamborShopId = "shop_forja_tambor";

        /// <summary>The Tambor Rubro's city id (= <c>CityCatalog.TamborRubro</c>).</summary>
        public const string TamborRubroCityId = "tambor_rubro";

        /// <summary>Repair 20 % cheaper at the Forja de Tambor (ficha 11, §3.3).</summary>
        public const float TamborRubroDiscount = 0.2f;

        /// <summary>No forge takes more than this off (a repair is never free).</summary>
        public const float MaxDiscount = 0.9f;

        /// <summary>The repair discount of a shop's counter (0 when the shop does not repair at all).</summary>
        public static float ForShop(string shopId) => shopId == ForjaTamborShopId ? TamborRubroDiscount : 0f;

        /// <summary>Whether the shop's own window offers "repair all" (only the forges with a repair of their own).</summary>
        public static bool OffersRepair(string shopId) => ForShop(shopId) > 0f;

        /// <summary>The discount of the ferreiro window opened in a city (the hub's Serviços): the city's forge, else 0.</summary>
        public static float ForCity(string cityId) => cityId == TamborRubroCityId ? TamborRubroDiscount : 0f;

        /// <summary>A repair price after the discount: rounded up, at least 1 when there was anything to pay, 0 stays 0.</summary>
        public static int Apply(int cost, float discount)
        {
            if (cost <= 0)
            {
                return 0;
            }

            float d = Math.Max(0f, Math.Min(MaxDiscount, discount));
            if (d <= 0f)
            {
                return cost;
            }

            return Math.Max(1, (int)Math.Ceiling(cost * (1.0 - d) - 1e-4));
        }

        /// <summary>"20" for 0.2 (whole percent).</summary>
        public static int Percent(float discount) => (int)Math.Round(Math.Max(0f, discount) * 100f);
    }
}
