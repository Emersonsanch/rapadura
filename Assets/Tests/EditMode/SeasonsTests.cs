using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Seasons;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.Seasons;
using Rapadura.UI.WorldMap;

namespace Rapadura.Tests
{
    /// <summary>
    /// "Sistema de temporadas" step 1 (Docs/Seasons.md) and the viagem rápida gates of the carro de boi: the date-driven
    /// calendar, the quadrilha rhythm game, prendas, the Barraca de Prendas (CatalogoCasa.md seasonal items), the
    /// arraiá decoration layout, the cousins' festa lines and the boss/event gates of <see cref="FastTravelRules"/>.
    /// </summary>
    public class SeasonsTests
    {
        private sealed class FixedClock : IDailyClock
        {
            public DateTime Now { get; set; }
        }

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            SeasonCalendar.Clock = null;
            SeasonCalendar.DebugForceFestaJunina = false;
        }

        [TearDown]
        public void TearDown()
        {
            SeasonCalendar.Clock = null;
            SeasonCalendar.DebugForceFestaJunina = false;
            EventBus.Clear();
        }

        // ------------------------------------------------------------------
        // Calendar
        // ------------------------------------------------------------------

        [Test]
        public void Calendar_JuneIsFestaJunina_AnyYear_AndTheDebugToggleForcesIt()
        {
            Assert.AreEqual(SeasonalEventId.FestaJunina, SeasonCalendar.ActiveOn(new DateTime(2027, 6, 1)));
            Assert.AreEqual(SeasonalEventId.FestaJunina, SeasonCalendar.ActiveOn(new DateTime(2031, 6, 30)));
            Assert.AreEqual(SeasonalEventId.None, SeasonCalendar.ActiveOn(new DateTime(2027, 5, 31)));
            Assert.AreEqual(SeasonalEventId.None, SeasonCalendar.ActiveOn(new DateTime(2027, 7, 1)));

            SeasonCalendar.Clock = new FixedClock { Now = new DateTime(2026, 10, 30, 20, 0, 0) };
            Assert.IsFalse(SeasonCalendar.IsFestaJuninaActive);
            SeasonCalendar.DebugForceFestaJunina = true;
            Assert.IsTrue(SeasonCalendar.IsFestaJuninaActive);
            SeasonCalendar.DebugForceFestaJunina = false;
            SeasonCalendar.Clock = new FixedClock { Now = new DateTime(2027, 6, 24, 21, 0, 0) };
            Assert.IsTrue(SeasonCalendar.IsFestaJuninaActive, "São João");
            Assert.AreEqual(20270624, SeasonCalendar.DayKey(SeasonCalendar.Today));
        }

        // ------------------------------------------------------------------
        // Quadrilha
        // ------------------------------------------------------------------

        [Test]
        public void Quadrilha_SequenceIsDeterministic_AndNeverThreeInARow()
        {
            List<QuadrilhaMove> a = QuadrilhaSession.Sequence(20270624, 200);
            List<QuadrilhaMove> b = QuadrilhaSession.Sequence(20270624, 200);
            CollectionAssert.AreEqual(a, b);
            CollectionAssert.AreNotEqual(a, QuadrilhaSession.Sequence(20270625, 200));
            for (int i = 2; i < a.Count; i++)
            {
                Assert.IsFalse(a[i] == a[i - 1] && a[i] == a[i - 2], $"three {a[i]} in a row at {i}");
            }
        }

        [Test]
        public void Quadrilha_JudgesOnTheBeat_WrongMovesAndLateStepsMiss()
        {
            var session = new QuadrilhaSession(7, 4);
            QuadrilhaStep first = session.Steps[0];

            // Too early: ignored, no penalty.
            Assert.AreEqual(QuadrilhaJudgement.None, session.Press(first.Move));
            Assert.AreEqual(0, session.Misses);

            session.Tick(first.Time);
            Assert.AreEqual(QuadrilhaJudgement.Perfect, session.Press(first.Move));

            QuadrilhaStep second = session.Steps[1];
            session.Tick(second.Time - session.Time + 0.2f);
            Assert.AreEqual(QuadrilhaJudgement.Good, session.Press(second.Move));

            QuadrilhaStep third = session.Steps[2];
            session.Tick(third.Time - session.Time);
            QuadrilhaMove wrong = (QuadrilhaMove)(((int)third.Move + 1) % 4);
            Assert.AreEqual(QuadrilhaJudgement.Miss, session.Press(wrong));

            // The last one is let past.
            session.Tick(10f);
            Assert.IsTrue(session.IsFinished);
            Assert.AreEqual(1, session.Perfects);
            Assert.AreEqual(1, session.Goods);
            Assert.AreEqual(2, session.Misses);
            Assert.AreEqual(QuadrilhaSession.PerfectPoints + QuadrilhaSession.GoodPoints, session.Score);
            Assert.AreEqual(2, session.BestCombo);
            Assert.IsFalse(session.IsFullCombo);
        }

        [Test]
        public void Quadrilha_PerfectRun_IsAFullCombo_AndRelaxedWidensTheWindows()
        {
            var session = new QuadrilhaSession(42);
            foreach (QuadrilhaStep step in session.Steps)
            {
                session.Tick(step.Time - session.Time);
                Assert.AreEqual(QuadrilhaJudgement.Perfect, session.Press(step.Move));
            }

            Assert.IsTrue(session.IsFullCombo);
            Assert.AreEqual(session.MaxScore, session.Score);

            var relaxed = new QuadrilhaSession(42, relaxed: true);
            Assert.Greater(relaxed.GoodWindow, session.GoodWindow);
            Assert.Greater(relaxed.Beat, session.Beat);
            QuadrilhaStep late = relaxed.Steps[0];
            relaxed.Tick(late.Time + session.GoodWindow + 0.05f);
            Assert.AreEqual(QuadrilhaJudgement.Good, relaxed.Press(late.Move), "inside the relaxed window");
        }

        // ------------------------------------------------------------------
        // Prendas and the Barraca
        // ------------------------------------------------------------------

        [Test]
        public void Prendas_ScaleWithTheScore_PlusComboAndFirstDanceBonus()
        {
            Assert.AreEqual(0, FestaJuninaRules.PrendasForDance(0, 32, false, false));
            Assert.AreEqual(5, FestaJuninaRules.PrendasForDance(16, 32, false, false));
            Assert.AreEqual(FestaJuninaRules.DancePrendasMax + FestaJuninaRules.FullComboBonus + FestaJuninaRules.FirstDanceBonus,
                FestaJuninaRules.PrendasForDance(32, 32, true, true));
            Assert.AreEqual(1, FestaJuninaRules.PrendasForFish(true, 0));
            Assert.AreEqual(0, FestaJuninaRules.PrendasForFish(true, FestaJuninaRules.FishPrendasPerDay));
            Assert.AreEqual(0, FestaJuninaRules.PrendasForFish(false, 0));
        }

        [Test]
        public void Stall_SellsTheEighteenFestaJuninaItems_ForPrendas_NotTheLegendary()
        {
            IReadOnlyList<HouseItemDefinition> stall = FestaJuninaRules.StallItems;
            Assert.AreEqual(18, stall.Count);
            Assert.IsTrue(stall.All(item => item.Seasonal == HouseSeason.Junina && item.EventPrice > 0 && item.ItemId.StartsWith("casa_sazonal_")));
            Assert.AreEqual("casa_sazonal_010", stall[0].ItemId, "cheapest first (5 prendas)");
            Assert.IsNull(FestaJuninaRules.StallItem(FestaJuninaRules.LegendaryItemId));

            HouseItemDefinition legendary = HouseCatalog.Get(FestaJuninaRules.LegendaryItemId);
            Assert.IsNotNull(legendary);
            Assert.AreEqual(HouseSeason.Junina, legendary.Seasonal);
            Assert.AreEqual(0, legendary.EventPrice);

            Assert.AreEqual(PrendaBuyCheck.FestaClosed, FestaJuninaRules.CheckBuy(stall[0], 999, false));
            Assert.AreEqual(PrendaBuyCheck.NotEnoughPrendas, FestaJuninaRules.CheckBuy(stall[0], stall[0].EventPrice - 1, true));
            Assert.AreEqual(PrendaBuyCheck.Ok, FestaJuninaRules.CheckBuy(stall[0], stall[0].EventPrice, true));
            Assert.AreEqual(PrendaBuyCheck.NotInStall, FestaJuninaRules.CheckBuy(legendary, 999, true));
        }

        [Test]
        public void Progress_PaysDances_SellsPrizesToTheDepot_GrantsTheLegendary_AndRoundTrips()
        {
            var house = new HouseService();
            var day = new DateTime(2027, 6, 10);
            var progress = new FestaJuninaProgress { House = house, FestaActiveProbe = () => true, TodayProbe = () => day };

            QuadrilhaReward first = progress.RecordDance(32, 32, true);
            Assert.IsTrue(first.FirstToday);
            Assert.AreEqual(17, first.Prendas);
            QuadrilhaReward second = progress.RecordDance(16, 32, false);
            Assert.IsFalse(second.FirstToday);
            Assert.AreEqual(5, second.Prendas);
            Assert.AreEqual(22, progress.Prendas);

            Assert.AreEqual(PrendaBuyCheck.Ok, progress.TryBuy("casa_sazonal_010"));
            Assert.AreEqual(17, progress.Prendas);
            Assert.AreEqual(1, house.Layout.DepotCount("casa_sazonal_010"));
            Assert.AreEqual(PrendaBuyCheck.NotEnoughPrendas, progress.TryBuy("casa_sazonal_015"));

            // Finish the tarefas do arraiá: 5 dances, a full combo (done), 6 different prizes.
            for (int i = 0; i < 3; i++)
            {
                progress.RecordDance(32, 32, true);
            }

            progress.Earn(200);
            foreach (HouseItemDefinition item in FestaJuninaRules.StallItems.Take(FestaJuninaRules.TaskDistinctPrizes))
            {
                progress.TryBuy(item.ItemId);
            }

            Assert.IsTrue(progress.TasksDone);
            Assert.IsTrue(progress.LegendaryGranted);
            Assert.AreEqual(1, house.Layout.DepotCount(FestaJuninaRules.LegendaryItemId));

            var copy = new FestaJuninaProgress { House = house, FestaActiveProbe = () => true, TodayProbe = () => day };
            copy.RestoreState(UnityEngine.JsonUtility.FromJson<FestaJuninaSaveState>(UnityEngine.JsonUtility.ToJson(progress.CaptureState())));
            Assert.AreEqual(progress.Prendas, copy.Prendas);
            Assert.AreEqual(progress.Dances, copy.Dances);
            Assert.AreEqual(progress.DistinctPrizes, copy.DistinctPrizes);
            Assert.IsTrue(copy.LegendaryGranted);
            Assert.IsFalse(copy.FirstDanceBonusAvailable, "today's first dance already paid");

            copy.ResetForNewSession();
            Assert.AreEqual(0, copy.Prendas);
            Assert.IsFalse(copy.LegendaryGranted);
        }

        [Test]
        public void Progress_PaysNothingOutsideTheFesta_AndTheFishPrendaIsCapped()
        {
            var day = new DateTime(2027, 6, 3);
            bool active = false;
            var progress = new FestaJuninaProgress { House = new HouseService(), FestaActiveProbe = () => active, TodayProbe = () => day };
            Assert.AreEqual(0, progress.RecordDance(32, 32, true).Prendas);
            Assert.AreEqual(0, progress.RecordFish());
            Assert.AreEqual(PrendaBuyCheck.FestaClosed, progress.TryBuy("casa_sazonal_010"));

            active = true;
            int paid = 0;
            for (int i = 0; i < FestaJuninaRules.FishPrendasPerDay + 5; i++)
            {
                paid += progress.RecordFish();
            }

            Assert.AreEqual(FestaJuninaRules.FishPrendasPerDay, paid);
            day = day.AddDays(1);
            Assert.AreEqual(1, progress.RecordFish(), "a new day, a new cap");
        }

        // ------------------------------------------------------------------
        // Texts, button, decoration
        // ------------------------------------------------------------------

        [Test]
        public void EveryCousin_HasEveryFestaLine()
        {
            foreach (CharacterId? hero in Enum.GetValues(typeof(CharacterId)).Cast<CharacterId>().Select(id => (CharacterId?)id).Append(null))
            {
                foreach (FestaLineKind kind in Enum.GetValues(typeof(FestaLineKind)))
                {
                    Assert.IsNotNull(FestaJuninaTexts.GetFallback(FestaJuninaTexts.LineKey(hero, kind)), $"{hero} {kind}");
                }
            }

            Assert.AreEqual(FestaLineKind.DanceGreat, FestaJuninaTexts.DanceKind(10, 32, true));
            Assert.AreEqual(FestaLineKind.DanceGreat, FestaJuninaTexts.DanceKind(26, 32, false));
            Assert.AreEqual(FestaLineKind.DanceOk, FestaJuninaTexts.DanceKind(16, 32, false));
            Assert.AreEqual(FestaLineKind.DanceBad, FestaJuninaTexts.DanceKind(15, 32, false));
            StringAssert.Contains("Edna", FestaJuninaTexts.GetFallback(FestaJuninaTexts.Title));
        }

        [Test]
        public void ArraiaButton_OnlyInTheVila_DuringTheFesta()
        {
            Assert.IsTrue(FestaJuninaPresenter.ShouldShowButton(true, true, true, false, false, BiomeType.Forest, true));
            Assert.IsFalse(FestaJuninaPresenter.ShouldShowButton(false, true, true, false, false, BiomeType.Forest, true), "no festa");
            Assert.IsFalse(FestaJuninaPresenter.ShouldShowButton(true, true, true, false, false, BiomeType.Desert, true), "another city");
            Assert.IsFalse(FestaJuninaPresenter.ShouldShowButton(true, true, true, false, false, BiomeType.Forest, false), "out on the road");
            Assert.IsFalse(FestaJuninaPresenter.ShouldShowButton(true, true, true, true, false, BiomeType.Forest, true), "paused");

            Assert.AreEqual(FestaJuninaPresenter.HitZonePercent, FestaJuninaPresenter.NotePercent(5f, 5f));
            Assert.IsNull(FestaJuninaPresenter.NotePercent(9f, 5f), "beyond the lookahead");
            Assert.Greater(FestaJuninaPresenter.NotePercent(6f, 5f).Value, FestaJuninaPresenter.NotePercent(5.5f, 5f).Value);

            PrizeRow row = FestaJuninaPresenter.BuildPrizeRow(FestaJuninaRules.StallItems[0], 0, true, false, null);
            Assert.IsFalse(row.CanBuy);
            StringAssert.Contains("oferendas", row.ButtonText);
        }

        [Test]
        public void Decor_PolesStayOffTheRoads_AndTheFireTakesTheWidestGap()
        {
            var roads = new List<float> { 0f, 90f, 180f };
            List<float> poles = FestaJuninaDecor.PoleAngles(roads, FestaJuninaDecor.PoleCount, FestaJuninaDecor.PoleRadius);
            Assert.AreEqual(FestaJuninaDecor.PoleCount, poles.Count);
            float halfGap = WorldMapCityDressing.GateHalfAngle(FestaJuninaDecor.PoleRadius);
            foreach (float pole in poles)
            {
                Assert.IsFalse(WorldMapCityDressing.IsInGate(pole, roads, halfGap), $"pole at {pole} stands on a road");
            }

            Assert.AreEqual(270f, FestaJuninaDecor.FireAngle(roads), 0.01f);
            Assert.AreEqual(270f, FestaJuninaDecor.FireAngle(null), 0.01f);
            Assert.AreEqual(225f, FestaJuninaDecor.FireAngle(new List<float> { 90f, 0f }), 0.01f);

            var builder = new Rapadura.Gameplay.World.Map.Dressing.WorldMapMeshBuilder();
            FestaJuninaDecor.AppendBandeirinhas(builder, poles, FestaJuninaDecor.PoleRadius);
            FestaJuninaDecor.AppendFireLogs(builder);
            FestaJuninaDecor.AppendFlame(builder);
            Assert.IsFalse(builder.IsEmpty);
        }

        // ------------------------------------------------------------------
        // Viagem rápida (carro de boi)
        // ------------------------------------------------------------------

        [Test]
        public void FastTravel_StopsOnlyWhereTheBossFell_HomeAlwaysOpen_NeverDuringAnEvent()
        {
            WorldMapGraph graph = WorldMapGraph.CreateDefault();
            Func<BiomeType, bool> all = _ => true;
            Func<BiomeType, bool> onlyDesertBoss = region => region == BiomeType.Desert;

            Assert.AreEqual(FastTravelCheck.Ok, FastTravelRules.Check(BiomeType.Forest, BiomeType.Desert, true, all, all, 1, 99, onlyDesertBoss, false));
            Assert.AreEqual(FastTravelCheck.BossNotDefeated, FastTravelRules.Check(BiomeType.Forest, BiomeType.Caves, true, all, all, 3, 99, onlyDesertBoss, false));
            Assert.AreEqual(FastTravelCheck.Ok, FastTravelRules.Check(BiomeType.Desert, BiomeType.Forest, true, all, all, 1, 99, _ => false, false),
                "home (the Vila) is always a stop");
            Assert.AreEqual(FastTravelCheck.EventActive, FastTravelRules.Check(BiomeType.Forest, BiomeType.Desert, true, all, all, 1, 99, all, true));
            Assert.AreEqual(FastTravelCheck.SameCity, FastTravelRules.Check(BiomeType.Forest, BiomeType.Forest, true, all, all, 0, 99, all, true));
            Assert.AreEqual(FastTravelCheck.Ok, FastTravelRules.Check(BiomeType.Forest, BiomeType.Caves, true, all, all, 3, 6), "old overload: no boss gate");

            Assert.IsTrue(FastTravelRules.IsEventActive(true, false, false));
            Assert.IsTrue(FastTravelRules.IsEventActive(false, true, false));
            Assert.IsTrue(FastTravelRules.IsEventActive(false, false, true));
            Assert.IsFalse(FastTravelRules.IsEventActive(false, false, false));

            FastTravelRow locked = CityFastTravelPanel.BuildRow(CityCatalog.ForRegion(BiomeType.Mountain), BiomeType.Forest, true, all, all,
                graph.GetNeighbours, 99, null, onlyDesertBoss, false);
            Assert.AreEqual(FastTravelCheck.BossNotDefeated, locked.Check);
            Assert.AreEqual(CityTexts.Get(null, CityTexts.FastTravelBossLocked), locked.Status);
            FastTravelRow busy = CityFastTravelPanel.BuildRow(CityCatalog.ForRegion(BiomeType.Desert), BiomeType.Forest, true, all, all,
                graph.GetNeighbours, 99, null, all, true);
            Assert.AreEqual(CityTexts.Get(null, CityTexts.FastTravelEventActive), busy.Status);
        }

        [Test]
        public void FastTravelTransition_FadesIn_ArrivesCovered_FadesOut()
        {
            Assert.AreEqual(0f, FastTravelTransition.Opacity(0f));
            Assert.AreEqual(0.5f, FastTravelTransition.Opacity(FastTravelTransition.FadeSeconds * 0.5f), 0.001f);
            Assert.AreEqual(1f, FastTravelTransition.Opacity(FastTravelTransition.FadeSeconds + 0.1f));
            Assert.AreEqual(0f, FastTravelTransition.Opacity(FastTravelTransition.TotalSeconds));
            Assert.IsFalse(FastTravelTransition.ShouldArrive(FastTravelTransition.FadeSeconds * 0.9f));
            Assert.IsTrue(FastTravelTransition.ShouldArrive(FastTravelTransition.FadeSeconds));
            Assert.AreEqual(1f, FastTravelTransition.Opacity(FastTravelTransition.FadeSeconds), 0.001f, "fully covered when the token moves");
            Assert.Less(FastTravelTransition.TotalSeconds, 2f, "a short trip");
        }
    }
}
