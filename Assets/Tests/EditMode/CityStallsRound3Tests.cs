using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The third round of city stalls: Shayn's Banca de Troca, the fair that walks the galleries and O Engarrafador's gift (Mercado dos Túneis,
    /// <see cref="TunnelMarketStallRules"/>), Pughat's diver of the day (Kirta, <see cref="KirtaDiverRescueRules"/>), the forge of the day
    /// (Hinokura, <see cref="HinokuraDayForgeRules"/>) and Makarios' moon counter (Porto da Lua Branca, <see cref="PortoMoonStallRules"/>), with
    /// their texts.
    /// </summary>
    public class CityStallsRound3Tests
    {
        // ------------------------------------------------------------------
        // Banca de Troca
        // ------------------------------------------------------------------

        [Test]
        public void Barter_ThreeForTwo_SameTierOnly_TenADay()
        {
            Assert.AreEqual(TunnelStallCheck.Ready, TunnelMarketStallRules.CheckBarter("item_stone", "item_fiber", 3, 10));
            Assert.AreEqual(TunnelStallCheck.NotEnough, TunnelMarketStallRules.CheckBarter("item_stone", "item_fiber", 2, 10));
            Assert.AreEqual(TunnelStallCheck.WrongPair, TunnelMarketStallRules.CheckBarter("item_stone", "item_parte_asa_morcego", 30, 10), "stones never become wings");
            Assert.AreEqual(TunnelStallCheck.WrongPair, TunnelMarketStallRules.CheckBarter("item_stone", "item_stone", 30, 10));
            Assert.AreEqual(TunnelStallCheck.WrongPair, TunnelMarketStallRules.CheckBarter("item_gold", "item_stone", 30, 10), "no gold at Shayn's bench");
            Assert.AreEqual(TunnelStallCheck.UsedUp, TunnelMarketStallRules.CheckBarter("item_stone", "item_fiber", 30, 0));

            Assert.AreEqual(10, TunnelMarketStallRules.TradesLeft(0, 0, 500));
            Assert.AreEqual(4, TunnelMarketStallRules.TradesLeft(500, 6, 500));
            Assert.AreEqual(0, TunnelMarketStallRules.TradesLeft(500, 99, 500));
            Assert.AreEqual(10, TunnelMarketStallRules.TradesLeft(499, 10, 500), "a new UTC day, ten new trades");
        }

        [Test]
        public void Barter_EveryMaterialIsAnItem_OnceOnly_AndTwoNeverSellForMoreThanThree()
        {
            var seen = new HashSet<string>();
            foreach (string[] tier in TunnelMarketStallRules.BarterTiers)
            {
                Assert.GreaterOrEqual(tier.Length, 2, "a tier needs something to trade into");
                foreach (string id in tier)
                {
                    Assert.IsTrue(seen.Add(id), id + " in two tiers");
                    Assert.IsNotNull(ItemDatabase.GetById(id), id);
                }

                foreach (string give in tier)
                {
                    foreach (string take in tier.Where(t => t != give))
                    {
                        int giveValue = ItemDatabase.GetById(give).Value;
                        int takeValue = ItemDatabase.GetById(take).Value;
                        Assert.LessOrEqual(TunnelMarketStallRules.BarterTake * takeValue, TunnelMarketStallRules.BarterGive * giveValue, give + " → " + take);
                    }
                }
            }

            Assert.AreEqual(CityStallsTexts.TierNames.Length, TunnelMarketStallRules.BarterTiers.Length);
        }

        // ------------------------------------------------------------------
        // Feira da Galeria + O Engarrafador
        // ------------------------------------------------------------------

        [Test]
        public void Fair_MovesEveryGameDay_NeverTheSameTwoDaysRunning_VisitsAllFour()
        {
            var visited = new HashSet<TunnelGallery>();
            for (int day = -5; day < 400; day++)
            {
                Assert.AreNotEqual(TunnelMarketStallRules.GalleryOf(day), TunnelMarketStallRules.GalleryOf(day + 1), "day " + day);
                visited.Add(TunnelMarketStallRules.GalleryOf(day));
            }

            Assert.AreEqual(4, visited.Count);
        }

        [Test]
        public void Fair_StandsSeventyMetresDownEachGallery_OnTheMarketMapAndOffTheCrossing()
        {
            Vector2 market = OverworldLayouts.MercadoTuneisPost;
            foreach (TunnelGallery gallery in new[] { TunnelGallery.North, TunnelGallery.East, TunnelGallery.South, TunnelGallery.West })
            {
                Vector2 spot = TunnelMarketStallRules.FairSpot(market, gallery);
                float along = Vector2.Dot(spot - market, TunnelMarketStallRules.DirectionOf(gallery));
                Assert.AreEqual(TunnelMarketStallRules.FairDistance, along, 0.01f, gallery.ToString());
                Assert.IsTrue(OverworldLayouts.TunnelMarketBounds.Contains(spot), gallery + " stays on map 22");
                Assert.Greater((spot - market).magnitude, CityTownRecipes.TunnelMarketHalfExtent + 20f, gallery + " is out in the gallery, not in the town");

                // Near the trail: within a few metres of the gallery's axis, where its air pipes stand.
                float side = Mathf.Abs(Vector2.Dot(spot - market, new Vector2(-TunnelMarketStallRules.DirectionOf(gallery).y, TunnelMarketStallRules.DirectionOf(gallery).x)));
                Assert.LessOrEqual(side, 4f);
            }
        }

        [Test]
        public void Fair_LinesAreCheaperThanTheCrossing_RealItems_OncePerGameDay()
        {
            Assert.AreEqual(4, TunnelMarketStallRules.FairLines.Length);
            foreach (TunnelFairLine[] lines in TunnelMarketStallRules.FairLines.Concat(PortoMoonStallRules.Lines))
            {
                Assert.AreEqual(2, lines.Length);
                foreach (TunnelFairLine line in lines)
                {
                    Assert.IsNotNull(ItemDatabase.GetById(line.ItemId), line.ItemId);
                    Assert.Greater(line.Count, 0);
                    Assert.Less(line.Price, line.CrossingPrice, line.ItemId);
                    Assert.GreaterOrEqual(line.Price, line.CrossingPrice * 0.6f, line.ItemId + ": a fair price, not a giveaway");
                }
            }

            TunnelFairLine bottles = TunnelMarketStallRules.FairLines[(int)TunnelGallery.East][0];
            Assert.AreEqual(TunnelStallCheck.Ready, TunnelMarketStallRules.CheckFairLine(bottles, 0, 12, 100));
            Assert.AreEqual(TunnelStallCheck.NotEnoughGold, TunnelMarketStallRules.CheckFairLine(bottles, 0, 12, bottles.Price - 1));
            Assert.AreEqual(TunnelStallCheck.UsedUp, TunnelMarketStallRules.CheckFairLine(bottles, 12, 12, 1000));
            Assert.AreEqual(TunnelStallCheck.Ready, TunnelMarketStallRules.CheckFairLine(bottles, 11, 12, 1000));
            Assert.AreNotEqual(TunnelMarketStallRules.FairLineKey(TunnelGallery.East, 0), TunnelMarketStallRules.FairLineKey(TunnelGallery.West, 0));
        }

        [Test]
        public void Bottler_HelpsOnlyOnceBeaten_TwoBottlesPerUtcDay()
        {
            Assert.AreEqual(TunnelStallCheck.NoHelper, TunnelMarketStallRules.CheckBottlerGift(false, 0, 900));
            Assert.AreEqual(TunnelStallCheck.Ready, TunnelMarketStallRules.CheckBottlerGift(true, 0, 900));
            Assert.AreEqual(TunnelStallCheck.UsedUp, TunnelMarketStallRules.CheckBottlerGift(true, 900, 900));
            Assert.AreEqual(TunnelStallCheck.Ready, TunnelMarketStallRules.CheckBottlerGift(true, 899, 900));
            Assert.AreEqual(Rapadura.Gameplay.Enemies.RegionBossRules.GreedyBottlerId, TunnelMarketStallRules.BottlerBossId);
            Assert.AreEqual(TunnelStillAirRules.BottleItemId, TunnelMarketStallRules.BottleItemId);
            Assert.IsNotNull(ItemDatabase.GetById(TunnelMarketStallRules.BottleItemId));
        }

        // ------------------------------------------------------------------
        // Kirta: Pughat's diver of the day
        // ------------------------------------------------------------------

        private static List<Vector2[]> Courses() => OverworldLayouts.KirtaReefDives.Select(d => d.Course).ToList();

        [Test]
        public void Diver_WaitsAtABubble_NeverAStation_NewOneEachDay_BothWalksUsed()
        {
            List<Vector2[]> courses = Courses();
            var dives = new HashSet<int>();
            for (long day = 20000; day < 20400; day++)
            {
                KirtaDiverSpot spot = KirtaDiverRescueRules.SpotOf(day, courses);
                KirtaDiverSpot next = KirtaDiverRescueRules.SpotOf(day + 1, courses);
                Assert.That(spot.Bubble, Is.InRange(1, courses[spot.Dive].Length - 2), "a bubble, not a station");
                Assert.IsFalse(spot.Dive == next.Dive && spot.Bubble == next.Bubble, "day " + day);
                dives.Add(spot.Dive);
                StringAssert.DoesNotContain(" ", KirtaDiverRescueRules.DiverOf(day));
            }

            Assert.AreEqual(courses.Count, dives.Count);
            Assert.AreEqual(-1, KirtaDiverRescueRules.SpotOf(1, new List<Vector2[]>()).Dive);
        }

        [Test]
        public void Diver_FreedOnlyAtTheBubble_OncePerUtcDay_AbyssPaysMore()
        {
            Vector2 bubble = new Vector2(1000f, 126f);
            Assert.AreEqual(KirtaDiverCheck.Freed, KirtaDiverRescueRules.Check(true, bubble + new Vector2(3f, 0f), bubble, 0, 700));
            Assert.AreEqual(KirtaDiverCheck.NotThere, KirtaDiverRescueRules.Check(true, bubble + new Vector2(6f, 0f), bubble, 0, 700));
            Assert.AreEqual(KirtaDiverCheck.NotThere, KirtaDiverRescueRules.Check(false, bubble, bubble, 0, 700));
            Assert.AreEqual(KirtaDiverCheck.DoneToday, KirtaDiverRescueRules.Check(true, bubble, bubble, 700, 700));
            Assert.Greater(KirtaDiverRescueRules.GoldFor(true), KirtaDiverRescueRules.GoldFor(false));
            Assert.AreEqual(1, KirtaDiverRescueRules.BubblesFor(true));
            Assert.AreEqual(0, KirtaDiverRescueRules.BubblesFor(false));
            Assert.IsNotNull(ItemDatabase.GetById(KirtaDiverRescueRules.BubbleItemId));
        }

        [Test]
        public void Diver_PughatHasADoorInKirta()
        {
            CityLayout kirta = CityTownRecipes.LayoutFor(CityTownRecipes.KirtaCityId);
            Assert.IsNotNull(kirta.LotOfResident("pughat"));
            CityLayout tunnels = CityTownRecipes.LayoutFor(CityTownRecipes.TunnelMarketCityId);
            Assert.IsNotNull(tunnels.LotOfResident("shayn"));
            CityLayout porto = CityTownRecipes.LayoutFor(CityTownRecipes.PortoDaLuaBrancaCityId);
            Assert.IsNotNull(porto.LotOfResident("makarios"));
        }

        // ------------------------------------------------------------------
        // Hinokura: the forge of the day
        // ------------------------------------------------------------------

        [Test]
        public void DayForge_RotatesByUtcDay_BonusOnlyWhileItBurns()
        {
            var seen = new HashSet<int>();
            for (long day = 20000; day < 20200; day++)
            {
                int forge = HinokuraDayForgeRules.ForgeOfDay(day);
                Assert.That(forge, Is.InRange(0, HinokuraForgeRules.Forges - 1));
                Assert.AreNotEqual(forge, HinokuraDayForgeRules.ForgeOfDay(day + 1));
                seen.Add(forge);
            }

            Assert.AreEqual(HinokuraForgeRules.Forges, seen.Count);

            // A moment just after a Tremor, so nothing strikes for a while.
            long unix = 20100L * 86400L + 3600L;
            unix = (HinokuraForgeRules.TremorCycle(unix) + 1L) * HinokuraForgeRules.TremorPeriodSeconds + HinokuraForgeRules.TremorJamSeconds + 1L;
            int today = HinokuraDayForgeRules.ForgeOfDay(CityServiceRules.DayOf(unix));
            var litAt = new[] { HinokuraForgeRules.NeverLit, HinokuraForgeRules.NeverLit, HinokuraForgeRules.NeverLit };
            Assert.AreEqual(0f, HinokuraDayForgeRules.RefineBonus(litAt, unix));

            litAt[(today + 1) % 3] = unix - 10L;
            Assert.AreEqual(0f, HinokuraDayForgeRules.RefineBonus(litAt, unix), "another forge burning does not count");

            litAt[today] = unix - 10L;
            Assert.AreEqual(HinokuraDayForgeRules.RefineBonusLit, HinokuraDayForgeRules.RefineBonus(litAt, unix));
            Assert.AreEqual(0f, HinokuraDayForgeRules.RefineBonus(litAt, unix + HinokuraForgeRules.BurnSeconds + 5L), "out after 20 min at the latest");
            Assert.AreEqual(0f, HinokuraDayForgeRules.RefineBonus((WorldMapProgress)null, unix));
        }

        [Test]
        public void DayForge_StacksWithTheGreatForge_UnderTheCap()
        {
            float bonus = HinokuraDayForgeRules.RefineBonusLit + 0.11f;
            for (int level = 0; level < 15; level++)
            {
                float chance = Rapadura.Gameplay.Refining.RefineRules.ChanceFor(level, true, true, bonus);
                Assert.LessOrEqual(chance, Rapadura.Gameplay.Refining.RefineRules.MaxChance + 1e-5f);
            }
        }

        // ------------------------------------------------------------------
        // Porto da Lua Branca: the moon counter
        // ------------------------------------------------------------------

        [Test]
        public void Moon_EightCounters_FullMoonBringsTheSoulStones_KeysPerMoon()
        {
            Assert.AreEqual(MoonCanalRules.CycleDays, PortoMoonStallRules.Lines.Length);
            Assert.AreEqual(CityStallsTexts.MoonNames.Length, PortoMoonStallRules.Lines.Length);
            Assert.AreEqual("item_pedra_alma", PortoMoonStallRules.Lines[MoonCanalRules.FullMoonIndex][0].ItemId);
            Assert.AreSame(PortoMoonStallRules.Lines[MoonCanalRules.FullMoonIndex], PortoMoonStallRules.LinesOf(5), "day 5 is the full moon");
            Assert.AreSame(PortoMoonStallRules.LinesOf(1), PortoMoonStallRules.LinesOf(9), "the cycle comes back");
            var keys = new HashSet<string>();
            for (int moon = 0; moon < PortoMoonStallRules.Lines.Length; moon++)
            {
                for (int line = 0; line < PortoMoonStallRules.LinesPerMoon; line++)
                {
                    Assert.IsTrue(keys.Add(PortoMoonStallRules.LineKey(moon, line)));
                }
            }

            int items = PortoMoonStallRules.Lines.SelectMany(l => l).Select(l => l.ItemId).Distinct().Count();
            Assert.AreEqual(16, items, "every moon brings its own goods");
        }

        // ------------------------------------------------------------------
        // Texts
        // ------------------------------------------------------------------

        [Test]
        public void Texts_BothLanguages_FiveCousinsEach_NoFolksyTitles()
        {
            var heroes = new[] { CharacterId.Maithe, CharacterId.Icaro, CharacterId.Joaquim, CharacterId.Maria, CharacterId.Lavine };
            foreach (System.Func<CharacterId, string> lineOf in new System.Func<CharacterId, string>[]
                     { CityStallsTexts.BarterLine, CityStallsTexts.BottlerLine, CityStallsTexts.DiverLine, CityStallsTexts.ForgeLine, CityStallsTexts.MoonLine })
            {
                Assert.AreEqual(5, heroes.Select(lineOf).Distinct().Count());
                foreach (CharacterId hero in heroes)
                {
                    Assert.AreNotEqual(lineOf(hero), CityStallsTexts.Get(null, lineOf(hero)), lineOf(hero));
                }
            }

            string all = string.Join(" ", CityStallsTexts.Entries.Select(e => e.PtBR));
            StringAssert.Contains("Vó Edna", all);
            StringAssert.Contains("Vô Jorge", all);
            var keys = new HashSet<string>();
            foreach (var entry in CityStallsTexts.Entries)
            {
                Assert.IsTrue(keys.Add(entry.Key), "duplicate " + entry.Key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR) || string.IsNullOrEmpty(entry.En), entry.Key);
                Assert.IsFalse(entry.PtBR.Contains("Dona ") || entry.PtBR.Contains("Seu "), entry.Key);
            }

            Assert.AreEqual(KirtaDiverRescueRules.DiverIds.Length, KirtaDiverRescueRules.DiverIds.Select(CityStallsTexts.DiverName).Distinct().Count());
            Assert.AreEqual(4, CityStallsTexts.GalleryNames.Length);
        }
    }
}
