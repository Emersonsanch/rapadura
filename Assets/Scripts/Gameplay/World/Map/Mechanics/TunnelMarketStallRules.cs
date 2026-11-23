using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>The four galleries of the Mercado dos Túneis (22), the trails that leave its four streets.</summary>
    public enum TunnelGallery
    {
        North = 0,
        East = 1,
        South = 2,
        West = 3,
    }

    /// <summary>What a stall of the Mercado dos Túneis answers (<see cref="TunnelMarketStallRules"/>).</summary>
    public enum TunnelStallCheck
    {
        Ready,

        /// <summary>The barter wants three of what is given.</summary>
        NotEnough,

        /// <summary>Only materials of the same tier trade (or the same material twice, or one Shayn does not take).</summary>
        WrongPair,

        /// <summary>The trades of the day (or the line of the fair, or the bottler's gift) are used up.</summary>
        UsedUp,

        NotEnoughGold,

        /// <summary>O Engarrafador Guloso has not been beaten yet: he is not at the fair.</summary>
        NoHelper,
    }

    /// <summary>One line of the gallery fair: a bundle at a fair price, once per in-game day.</summary>
    public readonly struct TunnelFairLine
    {
        public readonly string ItemId;
        public readonly int Count;
        public readonly int Price;

        /// <summary>What the same bundle costs at the Feira da Miwnay in the crossing (for the "you save" line and the tests).</summary>
        public readonly int CrossingPrice;

        public TunnelFairLine(string itemId, int count, int price, int crossingPrice)
        {
            ItemId = itemId;
            Count = count;
            Price = price;
            CrossingPrice = crossingPrice;
        }
    }

    /// <summary>
    /// Three things of the Mercado dos Túneis the sheets asked for (Docs/Mapas100.md §3.3 and 22: "a Banca de Troca (troca material por material
    /// sem ouro, 3 por 2)", "mercado que muda de túnel: a feira se move pelas 4 galerias a cada dia do jogo (o mapa mostra onde)", the map boss
    /// "vencido, solta as garrafas e vira ajudante da feira"), pure and server-runnable; the scene side is <c>CityStallsMechanicsDriver.Tunnel.cs</c>.
    /// <code>
    /// Banca de Troca da Shayn (East street): 3 of a material → 2 of another of the same tier, no gold · 10 trades per UTC day
    ///   tiers: stone/fibre · wood/herb · iron ore/spider silk/wolf fang · iron/scorpion sting · golem chip/bat wing
    /// Feira da Galeria: each in-game day the fair stands 70 m down one gallery (never the same two days running); two bundles of that
    ///   gallery's trade at ~30 % off the crossing, each once per in-game day
    /// O Engarrafador Guloso, once beaten (bestiary): helps at the gallery fair · 2 Garrafas de Ar Sereno free per UTC day
    /// </code>
    /// The tiers keep the 3-for-2 honest: a trade always gives back less than it takes (never stones into bat wings to sell).
    /// </summary>
    public static class TunnelMarketStallRules
    {
        // ---- Banca de Troca ----

        public const float BarterReach = 2.6f;
        public const int BarterGive = 3;
        public const int BarterTake = 2;
        public const int BarterTradesPerDay = 10;

        /// <summary>Daily mark: the UTC day of the last trade. With <see cref="BarterCountKey"/> (a time mark) the trades of that day.</summary>
        public const string BarterDayKey = "tunnel.shayn.barter_day";

        public const string BarterCountKey = "tunnel.shayn.barter_count";

        /// <summary>
        /// What Shayn takes and gives, by tier (sell values 1 · 2–3 · 5–6 · 8 · 10–12): inside a tier the dearest is at most 1.5× the cheapest,
        /// so 2 of anything never sell for more than the 3 given.
        /// </summary>
        public static readonly string[][] BarterTiers =
        {
            new[] { "item_stone", "item_fiber" },
            new[] { "item_wood", "item_herb" },
            new[] { "item_iron_ore", "item_parte_seda_aranha", "item_parte_presa_lobo" },
            new[] { "item_iron", "item_parte_ferrao_escorpiao" },
            new[] { "item_parte_lasca_golem", "item_parte_asa_morcego" },
        };

        /// <summary>The tier of a material (−1 when Shayn does not take it).</summary>
        public static int TierOf(string itemId)
        {
            for (int tier = 0; tier < BarterTiers.Length; tier++)
            {
                if (System.Array.IndexOf(BarterTiers[tier], itemId) >= 0)
                {
                    return tier;
                }
            }

            return -1;
        }

        /// <summary>All the materials of the stall, tier by tier.</summary>
        public static IEnumerable<string> BarterMaterials()
        {
            foreach (string[] tier in BarterTiers)
            {
                foreach (string id in tier)
                {
                    yield return id;
                }
            }
        }

        /// <summary>The trades already made today (the saved count only counts on its own day).</summary>
        public static int TradesUsed(long savedDay, long savedCount, long today) => savedDay == today ? (int)Mathf.Clamp(savedCount, 0, BarterTradesPerDay) : 0;

        public static int TradesLeft(long savedDay, long savedCount, long today) => BarterTradesPerDay - TradesUsed(savedDay, savedCount, today);

        /// <summary>Whether 3 × <paramref name="give"/> → 2 × <paramref name="take"/> can be done now.</summary>
        public static TunnelStallCheck CheckBarter(string give, string take, int giveOwned, int tradesLeft)
        {
            int tier = TierOf(give);
            if (tier < 0 || give == take || TierOf(take) != tier)
            {
                return TunnelStallCheck.WrongPair;
            }

            if (tradesLeft <= 0)
            {
                return TunnelStallCheck.UsedUp;
            }

            return giveOwned >= BarterGive ? TunnelStallCheck.Ready : TunnelStallCheck.NotEnough;
        }

        // ---- Feira da Galeria ----

        public const float FairReach = 3.2f;

        /// <summary>Metres from the crossing down the gallery where the fair stands.</summary>
        public const float FairDistance = 70f;

        /// <summary>Metres to the side of the trail (east of a north–south gallery, north of an east–west one).</summary>
        public const float FairSide = 3f;

        public const int FairLinesPerGallery = 2;

        /// <summary>Prefix of the daily marks of the fair's lines (+ gallery and line index): the in-game day it was bought.</summary>
        public const string FairLineKeyPrefix = "tunnel.fair.line.";

        /// <summary>The bundles of each gallery's trade (index = <see cref="TunnelGallery"/>).</summary>
        public static readonly TunnelFairLine[][] FairLines =
        {
            // North: the gallery of the lamplighters (to the Vila das Lamparinas) and Takhsich's workshop.
            new[] { new TunnelFairLine("item_lamparina_oleo", 2, 40, 56), new TunnelFairLine("item_oleo_fogo_eterno", 1, 140, 190) },
            // East: the gallery of the bottles, where O Engarrafador Guloso kept the Ar Sereno.
            new[] { new TunnelFairLine("item_garrafa_ar_sereno", 3, 63, 90), new TunnelFairLine("item_amuleto_sorte", 1, 17, 24) },
            // South: Farnkhund's rails, the ore of the deep veins.
            new[] { new TunnelFairLine("item_iron_ore", 10, 35, 50), new TunnelFairLine("item_pickaxe", 1, 12, 18) },
            // West: the gallery of the boat quay, what the barges bring down.
            new[] { new TunnelFairLine("item_health_potion", 4, 42, 60), new TunnelFairLine("item_mana_potion", 4, 39, 56) },
        };

        /// <summary>The gallery of the fair on an in-game day (any integer): a new one each day, never the same two days running.</summary>
        public static TunnelGallery GalleryOf(int day)
        {
            int today = Pick(day, 4);
            int yesterday = Pick(day - 1, 4);
            return (TunnelGallery)(today != yesterday ? today : (today + 1) % 4);
        }

        /// <summary>The way down a gallery (x = east, z = north).</summary>
        public static Vector2 DirectionOf(TunnelGallery gallery)
        {
            switch (gallery)
            {
                case TunnelGallery.North: return Vector2.up;
                case TunnelGallery.East: return Vector2.right;
                case TunnelGallery.South: return Vector2.down;
                default: return Vector2.left;
            }
        }

        /// <summary>Where the fair of <paramref name="gallery"/> stands, from the market's centre <paramref name="market"/>.</summary>
        public static Vector2 FairSpot(Vector2 market, TunnelGallery gallery)
        {
            Vector2 down = DirectionOf(gallery);
            Vector2 side = new Vector2(Mathf.Abs(down.y), Mathf.Abs(down.x));
            return market + down * FairDistance + side * FairSide;
        }

        public static string FairLineKey(TunnelGallery gallery, int line) => FairLineKeyPrefix + (int)gallery + "." + line;

        /// <summary>A line of the fair: once per in-game day (<paramref name="boughtDay"/> is the saved day, 0 when never).</summary>
        public static TunnelStallCheck CheckFairLine(TunnelFairLine line, int boughtDay, int day, int gold)
        {
            if (boughtDay == day)
            {
                return TunnelStallCheck.UsedUp;
            }

            return gold >= line.Price ? TunnelStallCheck.Ready : TunnelStallCheck.NotEnoughGold;
        }

        // ---- O Engarrafador Guloso at the fair ----

        public const string BottlerBossId = "enemy_mountain_greedy_bottler";
        public const string BottleItemId = "item_garrafa_ar_sereno";
        public const int BottlerGiftBottles = 2;

        /// <summary>Daily mark: the UTC day of the bottler's last gift.</summary>
        public const string BottlerGiftDayKey = "tunnel.fair.bottler_gift";

        /// <summary>The bottler's gift: only once he was beaten, once per UTC day.</summary>
        public static TunnelStallCheck CheckBottlerGift(bool bottlerBeaten, long givenDay, long today)
        {
            if (!bottlerBeaten)
            {
                return TunnelStallCheck.NoHelper;
            }

            return givenDay == today ? TunnelStallCheck.UsedUp : TunnelStallCheck.Ready;
        }

        /// <summary>A well-mixed pick of 0 … <paramref name="count"/> − 1 for a day number (the same mix as <see cref="CityServiceRules"/>).</summary>
        internal static int Pick(long day, int count)
        {
            unchecked
            {
                ulong h = (ulong)(day + 0x5F3759DFL) * 0xD6E8FEB86659FD93UL;
                h ^= h >> 32;
                h *= 0x9E3779B97F4A7C15UL;
                h ^= h >> 29;
                return (int)(h % (ulong)count);
            }
        }
    }
}
