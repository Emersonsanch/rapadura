using Rapadura.Gameplay.Mounts;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>What Shulgi's ribbon post answers to [E] (<see cref="ShulgiRaceRules.CheckTrade"/>).</summary>
    public enum ShulgiTradeCheck
    {
        /// <summary>Enough Fitas for the next offer: trade.</summary>
        Ready,

        /// <summary>Not enough Fitas yet for the next offer.</summary>
        NeedRibbons,

        /// <summary>Everything on Shulgi's list is already owned.</summary>
        AllOwned,
    }

    /// <summary>What Shulgi trades for Fitas de Corrida.</summary>
    public enum ShulgiOfferKind
    {
        Mount,
        Cosmetic,
    }

    /// <summary>One thing on Shulgi's list: a mount or a cosmetic, and its price in Fitas de Corrida.</summary>
    public readonly struct ShulgiOffer
    {
        public readonly ShulgiOfferKind Kind;
        public readonly string Id;
        public readonly int Cost;

        public ShulgiOffer(ShulgiOfferKind kind, string id, int cost)
        {
            Kind = kind;
            Id = id;
            Cost = cost;
        }
    }

    /// <summary>
    /// The Corrida de Shulgi of Aratta (Docs/Mapas100.md 26 and §3.3: "percurso marcado pela savana; tempo bom dá Fitas de Corrida que
    /// trocam por cosméticos de montaria e pela montaria nova Auroque de Aratta"), pure and server-runnable. The race itself is a
    /// <see cref="NartesRaceSession"/> (same gates, countdown, time limit and leash as the steppe race of map 47) on its own course
    /// (<c>OverworldLayouts.ShulgiRaceCourse</c>); this class holds Shulgi's medals, the Fitas he pays and what they buy.
    /// <code>
    /// course: ~860 m from the horn posts of the South street, 8 gates through the Savana dos Auroques, finish at the south gate
    /// run 5.5 m/s ≈ 155 s (bronze) · mount ×1.35–1.5 ≈ 105–120 s (silver) · a good gallop ≤ 110 s (gold)
    /// gold ≤ 110 s · silver ≤ 135 s · bronze ≤ 170 s · slower: finishes, no Fita
    /// Fitas de Corrida: gold 3 · silver 2 · bronze 1; the first medal of the UTC day pays +2
    /// Shulgi's list: Auroque de Aratta 25 (the Rare price of the Fitas do Laço) · Manta de Fitas de Aratta 6 · Sela de Junco de Aratta 10
    /// </code>
    /// The Fitas are kept in the mount collection's ribbon store under <see cref="RibbonKey"/> (saved with the Fitas do Laço, never mixed
    /// with any boss's), and the Auroque is bought through the same <see cref="MountDropRules.Ribbons"/> row as the boss mounts.
    /// </summary>
    public static class ShulgiRaceRules
    {
        /// <summary>The ribbon store key of the Fitas de Corrida (a race, not a boss: no enemy has this id).</summary>
        public const string RibbonKey = MountDropRules.ShulgiRace;

        /// <summary>Distance from the start (between the horn posts) at which the race can be called.</summary>
        public const float StartReach = 2.6f;

        /// <summary>Distance from Shulgi's ribbon post at which the trade answers.</summary>
        public const float TradeReach = 2.4f;

        public const float GoldSeconds = 110f;
        public const float SilverSeconds = 135f;
        public const float BronzeSeconds = 170f;

        public const int GoldRibbons = 3;
        public const int SilverRibbons = 2;
        public const int BronzeRibbons = 1;

        /// <summary>Extra Fitas for the first medal of each UTC day.</summary>
        public const int FirstOfDayBonus = 2;

        /// <summary>The daily mark of the first medal (the UTC day it was last paid).</summary>
        public const string DailyKey = "city.aratta.shulgi_race";

        public const string BlanketId = "blanket_fitas_de_aratta";
        public const string SaddleId = "saddle_junco_de_aratta";

        /// <summary>Shulgi's list, in the order he offers it: the Auroque first (the prize of the ficha), then the two cosmetics.</summary>
        public static readonly ShulgiOffer[] Offers =
        {
            new ShulgiOffer(ShulgiOfferKind.Mount, MountSheetCatalog.ArattaAurochs, MountDropRules.RareRibbonCost),
            new ShulgiOffer(ShulgiOfferKind.Cosmetic, BlanketId, 6),
            new ShulgiOffer(ShulgiOfferKind.Cosmetic, SaddleId, 10),
        };

        public static NartesRaceMedal MedalFor(float seconds)
        {
            if (seconds <= GoldSeconds)
            {
                return NartesRaceMedal.Gold;
            }

            if (seconds <= SilverSeconds)
            {
                return NartesRaceMedal.Silver;
            }

            return seconds <= BronzeSeconds ? NartesRaceMedal.Bronze : NartesRaceMedal.None;
        }

        /// <summary>Fitas de Corrida for a medal; <paramref name="firstOfDay"/> is the first medal of this UTC day (+<see cref="FirstOfDayBonus"/>).</summary>
        public static int RibbonsFor(NartesRaceMedal medal, bool firstOfDay)
        {
            int ribbons;
            switch (medal)
            {
                case NartesRaceMedal.Gold: ribbons = GoldRibbons; break;
                case NartesRaceMedal.Silver: ribbons = SilverRibbons; break;
                case NartesRaceMedal.Bronze: ribbons = BronzeRibbons; break;
                default: return 0;
            }

            return firstOfDay ? ribbons + FirstOfDayBonus : ribbons;
        }

        /// <summary>
        /// The first offer of <see cref="Offers"/> not owned yet (<paramref name="owns"/> answers for a mount id or a cosmetic id), or
        /// -1 when everything is owned.
        /// </summary>
        public static int NextOffer(System.Func<ShulgiOffer, bool> owns)
        {
            for (int i = 0; i < Offers.Length; i++)
            {
                if (owns == null || !owns(Offers[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>Whether the next offer can be traded with <paramref name="ribbons"/> Fitas.</summary>
        public static ShulgiTradeCheck CheckTrade(int nextOffer, int ribbons)
        {
            if (nextOffer < 0 || nextOffer >= Offers.Length)
            {
                return ShulgiTradeCheck.AllOwned;
            }

            return ribbons >= Offers[nextOffer].Cost ? ShulgiTradeCheck.Ready : ShulgiTradeCheck.NeedRibbons;
        }
    }
}
