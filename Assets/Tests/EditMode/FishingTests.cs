using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Cooking;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Fishing;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Achievements;
using Rapadura.UI.Fishing;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests.EditMode
{
    /// <summary>
    /// "Pesca no açude": the catalog and its item assets, the catch table by spot/time/weather, bite timing and the
    /// hook window (with assist), the tension physics (winnable, losable both ways), the whole session state machine
    /// with a seed, the "Caderno de pesca" (collection, records, save/load, achievements, dishes it teaches), texts and
    /// the procedural cues.
    /// </summary>
    public class FishingTests
    {
        private readonly List<Object> _created = new List<Object>();
        private readonly List<FishingProgress> _progresses = new List<FishingProgress>();
        private readonly List<MealService> _meals = new List<MealService>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (FishingProgress progress in _progresses)
            {
                progress.Shutdown();
            }

            foreach (MealService meals in _meals)
            {
                meals.Shutdown();
            }

            _progresses.Clear();
            _meals.Clear();
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
            ServiceLocator.Clear();
            ItemDatabase.Invalidate();
        }

        private static FishingSpotSpec Acude => FishCatalog.GetSpot(FishingSpotId.Acude);
        private static FishingSpotSpec Riacho => FishCatalog.GetSpot(FishingSpotId.Riacho);
        private static FishingSpotSpec Lago => FishCatalog.GetSpot(FishingSpotId.LagoSubterraneo);

        private static readonly TimeOfDayPhase[] Phases = { TimeOfDayPhase.Dawn, TimeOfDayPhase.Day, TimeOfDayPhase.Dusk, TimeOfDayPhase.Night };
        private static readonly WeatherType[] Weathers = { WeatherType.Clear, WeatherType.Rain, WeatherType.Fog, WeatherType.Sandstorm, WeatherType.Snow };

        private FishingProgress NewProgress(AchievementManager achievements = null, MealService meals = null)
        {
            var progress = new FishingProgress { Achievements = achievements ?? new AchievementManager(new AchievementDefinition[0]), Meals = meals };
            progress.Initialize();
            _progresses.Add(progress);
            return progress;
        }

        private MealService NewMeals()
        {
            var meals = new MealService(_ => false, _ => true);
            meals.Initialize();
            _meals.Add(meals);
            return meals;
        }

        private AchievementDefinition Definition(string id, int target, string[] ids)
        {
            var definition = ScriptableObject.CreateInstance<AchievementDefinition>();
            _created.Add(definition);
            definition.ConfigureForTests(id, AchievementConditionType.FishSpeciesCaught, target, ids);
            return definition;
        }

        // ------------------------------------------------------------------
        // Catalog and items
        // ------------------------------------------------------------------

        [Test]
        public void Catalog_HasTwelvePlusUniqueSpecies_EachAnItemWithWeightAndRarity()
        {
            Assert.GreaterOrEqual(FishCatalog.Fish.Count, 12);
            Assert.AreEqual(FishCatalog.Fish.Count, FishCatalog.Fish.Select(f => f.Id).Distinct().Count());

            foreach (FishSpec fish in FishCatalog.Fish)
            {
                ItemDefinition item = ItemDatabase.GetById(fish.Id);
                Assert.IsNotNull(item, $"{fish.Id}: no Resources/Items asset");
                Assert.Greater(item.Weight, 0f, fish.Id);
                Assert.IsTrue(item.IsStackable, fish.Id);
                Assert.AreEqual(ItemCategory.Food, item.Category, fish.Id);
                Assert.AreEqual(fish.ItemRarity, item.Rarity, fish.Id);
                Assert.IsNotEmpty(fish.Spots, fish.Id);
                Assert.Greater(fish.MaxKg, fish.MinKg, fish.Id);
                Assert.That(fish.Strength, Is.InRange(0f, 1f), fish.Id);
                Assert.Greater(fish.Stamina, 0f, fish.Id);
                Assert.That(item.Weight, Is.InRange(fish.MinKg, fish.MaxKg), $"{fish.Id}: the item weighs a typical fish");
            }
        }

        [Test]
        public void Catalog_HasTheNamedFish_AndTheRareOnesWhereTheyBelong()
        {
            CollectionAssert.IsSubsetOf(
                new[] { FishCatalog.Tilapia, FishCatalog.Traira, FishCatalog.Piaba, FishCatalog.Tucunare, FishCatalog.Pacu, FishCatalog.Surubim, FishCatalog.PeixeDourado, FishCatalog.BagreCego },
                FishCatalog.AllIds());

            FishSpec golden = FishCatalog.Get(FishCatalog.PeixeDourado);
            Assert.AreEqual(FishRarity.Legendary, golden.Rarity);
            CollectionAssert.AreEquivalent(new[] { FishingSpotId.Acude }, golden.Spots);
            CollectionAssert.AreEquivalent(new[] { FishingSpotId.LagoSubterraneo }, FishCatalog.Get(FishCatalog.BagreCego).Spots);
            Assert.AreEqual(1, FishCatalog.Fish.Count(f => f.Rarity == FishRarity.Legendary), "one legend");
        }

        [Test]
        public void Spots_OnePerWaterRegion_TheSertaoHasNone()
        {
            Assert.AreEqual(FishingSpotId.Acude, FishCatalog.SpotFor(BiomeType.Forest).Id);
            Assert.AreEqual(FishingSpotId.Riacho, FishCatalog.SpotFor(BiomeType.Mountain).Id);
            Assert.AreEqual(FishingSpotId.LagoSubterraneo, FishCatalog.SpotFor(BiomeType.Caves).Id);
            Assert.IsNull(FishCatalog.SpotFor(BiomeType.Desert));
            Assert.IsNull(FishCatalog.SpotFor(null));
            Assert.IsTrue(Lago.Sheltered);
        }

        [Test]
        public void EverySpecies_BitesSomewhereSometime_AndEverySpotAlwaysHasFish()
        {
            foreach (FishSpec fish in FishCatalog.Fish)
            {
                bool catchable = FishCatalog.Spots.Any(spot => Phases.Any(phase => Weathers.Any(weather => FishingRules.CatchWeight(fish, spot, phase, weather) > 0f)));
                Assert.IsTrue(catchable, $"{fish.Id} can never be caught");
            }

            foreach (FishingSpotSpec spot in FishCatalog.Spots)
            {
                Assert.GreaterOrEqual(FishCatalog.Fish.Count(f => f.LivesIn(spot.Id)), 5, $"{spot.Id} needs variety");
                foreach (TimeOfDayPhase phase in Phases)
                {
                    foreach (WeatherType weather in Weathers)
                    {
                        Assert.GreaterOrEqual(FishingRules.BuildCatchTable(spot, phase, weather).Count, 3, $"{spot.Id} {phase} {weather}");
                    }
                }
            }
        }

        // ------------------------------------------------------------------
        // Catch table
        // ------------------------------------------------------------------

        [Test]
        public void CatchWeight_SpotPhaseAndWeatherRules()
        {
            FishSpec tilapia = FishCatalog.Get(FishCatalog.Tilapia);
            FishSpec traira = FishCatalog.Get(FishCatalog.Traira);
            FishSpec curimata = FishCatalog.Get(FishCatalog.Curimata);
            FishSpec golden = FishCatalog.Get(FishCatalog.PeixeDourado);

            Assert.AreEqual(0f, FishingRules.CatchWeight(tilapia, Riacho, TimeOfDayPhase.Day, WeatherType.Clear), "not in the stream");
            Assert.AreEqual(FishingRules.CommonWeight * FishingRules.PreferredPhaseMultiplier, FishingRules.CatchWeight(tilapia, Acude, TimeOfDayPhase.Day, WeatherType.Clear));
            Assert.AreEqual(FishingRules.CommonWeight, FishingRules.CatchWeight(tilapia, Acude, TimeOfDayPhase.Night, WeatherType.Clear));
            Assert.Greater(FishingRules.CatchWeight(traira, Acude, TimeOfDayPhase.Night, WeatherType.Clear), FishingRules.CatchWeight(traira, Acude, TimeOfDayPhase.Day, WeatherType.Clear));
            Assert.AreEqual(FishingRules.UncommonWeight * FishingRules.PreferredWeatherMultiplier, FishingRules.CatchWeight(curimata, Acude, TimeOfDayPhase.Day, WeatherType.Rain));
            Assert.AreEqual(0f, FishingRules.CatchWeight(golden, Acude, TimeOfDayPhase.Day, WeatherType.Rain), "the legend never bites at noon");
            Assert.AreEqual(0f, FishingRules.CatchWeight(golden, Acude, TimeOfDayPhase.Night, WeatherType.Rain));
            Assert.Greater(FishingRules.CatchWeight(golden, Acude, TimeOfDayPhase.Dawn, WeatherType.Rain), FishingRules.CatchWeight(golden, Acude, TimeOfDayPhase.Dusk, WeatherType.Clear));
        }

        [Test]
        public void ShelteredLake_IgnoresTheWeather()
        {
            foreach (WeatherType weather in Weathers)
            {
                CollectionAssert.AreEqual(
                    FishingRules.BuildCatchTable(Lago, TimeOfDayPhase.Night, WeatherType.Clear).Select(e => e.Weight),
                    FishingRules.BuildCatchTable(Lago, TimeOfDayPhase.Night, weather).Select(e => e.Weight), weather.ToString());
            }

            Assert.AreEqual(WeatherType.Clear, FishingRules.EffectiveWeather(Lago, WeatherType.Rain));
            Assert.AreEqual(WeatherType.Rain, FishingRules.EffectiveWeather(Acude, WeatherType.Rain));
        }

        [Test]
        public void RollCatch_IsWeighted_Bounded_AndSeeded()
        {
            List<CatchTableEntry> table = FishingRules.BuildCatchTable(Acude, TimeOfDayPhase.Dawn, WeatherType.Rain);
            Assert.AreSame(table[0].Fish, FishingRules.RollCatch(table, 0d));
            Assert.AreSame(table[table.Count - 1].Fish, FishingRules.RollCatch(table, 0.9999999d));
            Assert.AreSame(table[table.Count - 1].Fish, FishingRules.RollCatch(table, 1.5d), "clamped");
            Assert.IsNull(FishingRules.RollCatch(new List<CatchTableEntry>(), 0.5d));

            var a = new FishingRandom(1234);
            var b = new FishingRandom(1234);
            for (int i = 0; i < 50; i++)
            {
                Assert.AreSame(FishingRules.RollCatch(table, a.Next01()), FishingRules.RollCatch(table, b.Next01()));
            }

            // Frequencies follow the weights.
            var random = new FishingRandom(7);
            var counts = new Dictionary<string, int>();
            const int rolls = 20000;
            for (int i = 0; i < rolls; i++)
            {
                string id = FishingRules.RollCatch(table, random.Next01()).Id;
                counts[id] = counts.TryGetValue(id, out int n) ? n + 1 : 1;
            }

            foreach (CatchTableEntry entry in table)
            {
                float expected = FishingRules.Chance(table, entry.Fish.Id);
                counts.TryGetValue(entry.Fish.Id, out int seen);
                Assert.AreEqual(expected, seen / (float)rolls, 0.015f, entry.Fish.Id);
            }
        }

        [Test]
        public void TheLegend_IsRare_EvenAtItsBest()
        {
            float best = Phases.Max(phase => Weathers.Max(weather => FishingRules.Chance(FishingRules.BuildCatchTable(Acude, phase, weather), FishCatalog.PeixeDourado)));
            Assert.Greater(best, 0.005f);
            Assert.Less(best, 0.05f);

            FishSpec common = FishCatalog.Get(FishCatalog.Tilapia);
            Assert.Greater(FishingRules.Chance(FishingRules.BuildCatchTable(Acude, TimeOfDayPhase.Day, WeatherType.Clear), common.Id), 0.3f);
        }

        // ------------------------------------------------------------------
        // Bite timing and the hook window
        // ------------------------------------------------------------------

        [Test]
        public void PlanBite_NibblesComeFirst_ThenTheTremble_ThenTheBite()
        {
            for (int seed = 0; seed < 200; seed++)
            {
                foreach (bool assist in new[] { false, true })
                {
                    BitePlan plan = FishingRules.PlanBite(new FishingRandom(seed), Acude, WeatherType.Clear, assist);
                    Assert.AreEqual(FishingRules.Lead(assist), plan.BiteAt - plan.TrembleAt, 1e-4f);
                    Assert.LessOrEqual(plan.BiteAt, FishingRules.MaxBiteDelay * Acude.BiteDelayScale + 1e-3f);
                    Assert.GreaterOrEqual(plan.TrembleAt, 0.5f);
                    Assert.LessOrEqual(plan.NibbleTimes.Count, 2);
                    float previous = 0f;
                    foreach (float nibble in plan.NibbleTimes)
                    {
                        Assert.Greater(nibble, previous);
                        Assert.Less(nibble, plan.TrembleAt);
                        previous = nibble;
                    }
                }
            }
        }

        [Test]
        public void PlanBite_RainAndFastWater_BiteSooner()
        {
            float Average(FishingSpotSpec spot, WeatherType weather) =>
                Enumerable.Range(0, 400).Average(seed => FishingRules.PlanBite(new FishingRandom(seed), spot, weather, false).BiteAt);

            Assert.Less(Average(Acude, WeatherType.Rain), Average(Acude, WeatherType.Clear));
            Assert.Less(Average(Riacho, WeatherType.Clear), Average(Acude, WeatherType.Clear));
            Assert.Greater(Average(Lago, WeatherType.Clear), Average(Acude, WeatherType.Clear));
            Assert.AreEqual(Average(Lago, WeatherType.Clear), Average(Lago, WeatherType.Rain), 1e-4f, "no rain under the rock");
        }

        [Test]
        public void HookWindow_ShrinksWithRarity_AndAssistWidensIt()
        {
            float previous = float.MaxValue;
            foreach (FishRarity rarity in new[] { FishRarity.Common, FishRarity.Uncommon, FishRarity.Rare, FishRarity.Legendary })
            {
                float window = FishingRules.HookWindowSeconds(rarity, false);
                float assisted = FishingRules.HookWindowSeconds(rarity, true);
                Assert.Less(window, previous);
                Assert.GreaterOrEqual(window, 0.5f, "humanly possible");
                Assert.GreaterOrEqual(assisted, FishingRules.AssistMinWindow);
                Assert.GreaterOrEqual(assisted, window * 2f - 1e-4f);
                previous = window;
            }
        }

        [Test]
        public void EvaluateHook_EarlyHitLate_WithAWiderAssistGrace()
        {
            var plan = new BitePlan(new[] { 1f }, 4.5f, 5f);
            const float window = 0.8f;
            Assert.AreEqual(HookResult.TooEarly, FishingRules.EvaluateHook(1f, plan, window, false), "on a nibble");
            Assert.AreEqual(HookResult.TooEarly, FishingRules.EvaluateHook(4.7f, plan, window, false), "trembling is not biting");
            Assert.AreEqual(HookResult.Hooked, FishingRules.EvaluateHook(4.95f, plan, window, false), "a hair early still counts");
            Assert.AreEqual(HookResult.Hooked, FishingRules.EvaluateHook(5.79f, plan, window, false));
            Assert.AreEqual(HookResult.TooLate, FishingRules.EvaluateHook(5.81f, plan, window, false));

            Assert.AreEqual(HookResult.Hooked, FishingRules.EvaluateHook(4.7f, plan, window, true), "assist forgives an eager press on the tremble");
            Assert.AreEqual(HookResult.TooEarly, FishingRules.EvaluateHook(1f, plan, window, true), "but not a nibble");
        }

        // ------------------------------------------------------------------
        // Size
        // ------------------------------------------------------------------

        [Test]
        public void RollWeight_StaysInRange_AndBigOnesAreRare()
        {
            foreach (FishSpec fish in FishCatalog.Fish)
            {
                Assert.AreEqual(Mathf.Max(1, Mathf.RoundToInt(fish.MinKg * 1000f)), FishingRules.RollWeightGrams(fish, 0d, 0d), fish.Id);
                Assert.AreEqual(Mathf.RoundToInt(fish.MaxKg * 1000f), FishingRules.RollWeightGrams(fish, 1d, 1d), fish.Id);
            }

            FishSpec surubim = FishCatalog.Get(FishCatalog.Surubim);
            var random = new FishingRandom(3);
            var grams = Enumerable.Range(0, 2000).Select(_ => FishingRules.RollWeightGrams(surubim, random.Next01(), random.Next01())).OrderBy(g => g).ToList();
            float midpoint = (surubim.MinKg + surubim.MaxKg) * 500f;
            Assert.Less(grams[grams.Count / 2], midpoint, "median below the middle: a record is an event");
            Assert.That(grams.All(g => g >= surubim.MinKg * 1000f - 1 && g <= surubim.MaxKg * 1000f + 1));
        }

        // ------------------------------------------------------------------
        // Tension physics
        // ------------------------------------------------------------------

        private delegate bool Controller(ReelState state);

        private static ReelOutcome Fight(FishSpec fish, bool assist, int seed, Controller controller, out float seconds, float limit = 60f)
        {
            var random = new FishingRandom(seed);
            ReelParams tuning = FishingRules.ReelParamsFor(fish, assist);
            ReelState state = FishingRules.StartReel(random, fish);
            const float dt = 1f / 60f;
            seconds = 0f;
            while (state.Outcome == ReelOutcome.Fighting && seconds < limit)
            {
                state = FishingRules.StepReel(state, tuning, fish, controller(state), dt, random);
                seconds += dt;
            }

            return state.Outcome;
        }

        [Test]
        public void Reel_ACarefulRhythm_LandsEverySpecies_InBothModes()
        {
            foreach (FishSpec fish in FishCatalog.Fish)
            {
                foreach (bool assist in new[] { false, true })
                {
                    for (int seed = 0; seed < 5; seed++)
                    {
                        ReelOutcome outcome = Fight(fish, assist, seed, s => !s.Surging && s.Tension < 0.7f, out float seconds);
                        Assert.AreEqual(ReelOutcome.Landed, outcome, $"{fish.Id} assist={assist} seed={seed}");
                        Assert.Less(seconds, 30f, $"{fish.Id}: a fight should not drag on");
                    }
                }
            }
        }

        [Test]
        public void Reel_HoldingForever_SnapsTheLine_ReleasingForever_LetsItGo()
        {
            foreach (FishSpec fish in FishCatalog.Fish.Where(f => f.Stamina >= 3f))
            {
                Assert.AreEqual(ReelOutcome.Snapped, Fight(fish, false, 1, _ => true, out float snapNormal), fish.Id);
                Assert.AreEqual(ReelOutcome.Snapped, Fight(fish, true, 1, _ => true, out float snapAssist), fish.Id);
                Assert.Greater(snapAssist, snapNormal, $"{fish.Id}: assist gives more time before the snap");

                Assert.AreEqual(ReelOutcome.Escaped, Fight(fish, false, 1, _ => false, out float slackNormal), fish.Id);
                Assert.AreEqual(ReelOutcome.Escaped, Fight(fish, true, 1, _ => false, out float slackAssist), fish.Id);
                Assert.Greater(slackAssist, slackNormal, $"{fish.Id}: assist tolerates more slack");
            }

            // A tiny piaba just comes out if you keep reeling.
            Assert.AreEqual(ReelOutcome.Landed, Fight(FishCatalog.Get(FishCatalog.Piaba), false, 1, _ => true, out _));
        }

        [Test]
        public void Reel_ReelingThroughSurges_IsWhatSnapsBigFish_AndAssistForgivesIt()
        {
            FishSpec golden = FishCatalog.Get(FishCatalog.PeixeDourado);
            int normalLanded = 0;
            int assistLanded = 0;
            for (int seed = 0; seed < 20; seed++)
            {
                // Greedy and slow: ignores surges, keeps the line near the red zone and reacts 0.35 s late.
                if (Fight(golden, false, seed, Laggy(0.9f, 0.35f), out _) == ReelOutcome.Landed) normalLanded++;
                if (Fight(golden, true, seed, Laggy(0.9f, 0.35f), out _) == ReelOutcome.Landed) assistLanded++;
            }

            Assert.Greater(assistLanded, normalLanded);
            Assert.GreaterOrEqual(assistLanded, 18, "assist forgives a slow, greedy hand");
        }

        /// <summary>Holds while the tension it saw <paramref name="latency"/> seconds ago was under <paramref name="threshold"/> (a human's reaction time).</summary>
        private static Controller Laggy(float threshold, float latency)
        {
            var seen = new Queue<float>();
            int frames = Mathf.RoundToInt(latency * 60f);
            return state =>
            {
                seen.Enqueue(state.Tension);
                float tension = seen.Count > frames ? seen.Dequeue() : FishingRules.StartTension;
                return tension < threshold;
            };
        }

        [Test]
        public void Reel_SameSeed_SameFight_AndBigStepsAreClamped()
        {
            FishSpec traira = FishCatalog.Get(FishCatalog.Traira);
            ReelOutcome a = Fight(traira, false, 42, s => !s.Surging && s.Tension < 0.7f, out float ta);
            ReelOutcome b = Fight(traira, false, 42, s => !s.Surging && s.Tension < 0.7f, out float tb);
            Assert.AreEqual(a, b);
            Assert.AreEqual(ta, tb, 1e-5f);

            var random = new FishingRandom(1);
            ReelState state = FishingRules.StartReel(random, traira);
            ReelState after = FishingRules.StepReel(state, FishingRules.ReelParamsFor(traira, false), traira, true, 10f, random);
            Assert.Less(after.Tension - state.Tension, 0.2f, "a 10 s hitch is one MaxStep, not ten seconds");
        }

        // ------------------------------------------------------------------
        // Session
        // ------------------------------------------------------------------

        private static void TickUntil(FishingSession session, System.Func<bool> done, bool held = false, float limit = 30f)
        {
            float elapsed = 0f;
            while (!done() && elapsed < limit)
            {
                session.Tick(1f / 60f, held);
                elapsed += 1f / 60f;
            }
        }

        [Test]
        public void Session_CastWaitHookReel_LandsAFish_WithCuesInOrder()
        {
            var session = new FishingSession(Acude, TimeOfDayPhase.Day, WeatherType.Clear, assist: false, seed: 99);
            Assert.AreEqual(FishingPhase.Ready, session.Phase);
            session.Press();
            Assert.AreEqual(FishingPhase.Casting, session.Phase);
            CollectionAssert.AreEqual(new[] { FishingCue.Cast }, session.Cues);
            Assert.IsNotNull(session.Fish);

            var seen = new List<FishingCue>();
            float elapsed = 0f;
            while (!session.FloatSunk && elapsed < 20f)
            {
                session.Tick(1f / 60f, false);
                seen.AddRange(session.Cues);
                elapsed += 1f / 60f;
            }

            Assert.AreEqual(FishingCue.Splash, seen.First());
            Assert.AreEqual(FishingCue.Bite, seen.Last());
            Assert.AreEqual(1, seen.Count(c => c == FishingCue.Tremble), "one readable anticipation cue");
            Assert.Less(seen.IndexOf(FishingCue.Tremble), seen.IndexOf(FishingCue.Bite));

            session.Press();
            Assert.AreEqual(FishingPhase.Reeling, session.Phase);
            CollectionAssert.Contains(session.Cues, FishingCue.Hooked);

            elapsed = 0f;
            while (session.Phase == FishingPhase.Reeling && elapsed < 60f)
            {
                session.Tick(1f / 60f, !session.Reel.Surging && session.Reel.Tension < 0.7f);
                elapsed += 1f / 60f;
            }

            Assert.AreEqual(FishingPhase.Result, session.Phase);
            Assert.AreEqual(FishingResult.Landed, session.Result);
            CollectionAssert.Contains(session.Cues, FishingCue.Landed);
            Assert.That(session.LandedGrams, Is.InRange(Mathf.RoundToInt(session.Fish.MinKg * 1000f), Mathf.RoundToInt(session.Fish.MaxKg * 1000f)));
        }

        [Test]
        public void Session_HookingEarly_OrNeverHooking_LosesTheFish()
        {
            var early = new FishingSession(Acude, TimeOfDayPhase.Day, WeatherType.Clear, false, 5);
            early.Press();
            TickUntil(early, () => early.Phase == FishingPhase.Waiting);
            early.Press();
            Assert.AreEqual(FishingResult.TooEarly, early.Result);
            CollectionAssert.Contains(early.Cues, FishingCue.TooEarly);

            var late = new FishingSession(Acude, TimeOfDayPhase.Day, WeatherType.Clear, false, 5);
            late.Press();
            TickUntil(late, () => late.Phase == FishingPhase.Result);
            Assert.AreEqual(FishingResult.TooLate, late.Result);

            // Pressing again starts the next cast.
            late.Press();
            Assert.AreEqual(FishingPhase.Casting, late.Phase);
            Assert.AreEqual(FishingResult.None, late.Result);
        }

        [Test]
        public void Session_AssistWindow_StaysOpenLonger()
        {
            float Window(bool assist)
            {
                var session = new FishingSession(Acude, TimeOfDayPhase.Day, WeatherType.Clear, assist, 11);
                session.Press();
                TickUntil(session, () => session.FloatSunk);
                float open = 0f;
                while (session.FloatSunk)
                {
                    session.Tick(1f / 120f, false);
                    open += 1f / 120f;
                }

                return open;
            }

            Assert.Greater(Window(true), Window(false) * 1.8f);
        }

        [Test]
        public void Session_SameSeed_SameTrip()
        {
            var a = new FishingSession(Riacho, TimeOfDayPhase.Night, WeatherType.Rain, false, 2024);
            var b = new FishingSession(Riacho, TimeOfDayPhase.Night, WeatherType.Rain, false, 2024);
            for (int i = 0; i < 10; i++)
            {
                a.Press();
                b.Press();
                Assert.AreSame(a.Fish, b.Fish);
                Assert.AreEqual(a.Plan.BiteAt, b.Plan.BiteAt);
                CollectionAssert.AreEqual(a.Plan.NibbleTimes, b.Plan.NibbleTimes);
                TickUntil(a, () => a.Phase == FishingPhase.Result);
                TickUntil(b, () => b.Phase == FishingPhase.Result);
            }
        }

        // ------------------------------------------------------------------
        // Progress: collection, records, save
        // ------------------------------------------------------------------

        [Test]
        public void RecordCatch_NewSpecies_ThenRecords()
        {
            FishingProgress progress = NewProgress();
            var events = new List<FishCaughtEvent>();
            EventBus.Subscribe<FishCaughtEvent>(events.Add);

            FishCatchOutcome first = progress.RecordCatch(FishCatalog.Tilapia, 800);
            Assert.IsTrue(first.NewSpecies);
            Assert.IsTrue(first.NewRecord);
            Assert.AreEqual(0, first.PreviousBestGrams);

            FishCatchOutcome smaller = progress.RecordCatch(FishCatalog.Tilapia, 500);
            Assert.IsFalse(smaller.NewSpecies);
            Assert.IsFalse(smaller.NewRecord);
            Assert.AreEqual(800, smaller.PreviousBestGrams);

            FishCatchOutcome bigger = progress.RecordCatch(FishCatalog.Tilapia, 1900);
            Assert.IsTrue(bigger.NewRecord);
            Assert.AreEqual(800, bigger.PreviousBestGrams);

            Assert.AreEqual(1900, progress.GetBestGrams(FishCatalog.Tilapia));
            Assert.AreEqual(3, progress.GetCaughtCount(FishCatalog.Tilapia));
            Assert.AreEqual(1, progress.CaughtSpeciesCount);
            Assert.AreEqual(3, events.Count);

            Assert.IsFalse(progress.RecordCatch("item_fish_unicorn", 100).NewSpecies, "unknown species ignored");
            Assert.AreEqual(1, progress.CaughtSpeciesCount);
        }

        [Test]
        public void Collection_CompletesOnce_AndUnlocksTheAchievements()
        {
            string[] ids = FishCatalog.AllIds();
            AchievementDefinition all = Definition("ach_fish_test_all", ids.Length, ids);
            AchievementDefinition golden = Definition("ach_fish_test_golden", 1, new[] { FishCatalog.PeixeDourado });
            var achievements = new AchievementManager(new[] { all, golden });
            FishingProgress progress = NewProgress(achievements);
            int completed = 0;
            EventBus.Subscribe<FishCollectionCompletedEvent>(_ => completed++);

            foreach (string id in ids.Where(id => id != FishCatalog.PeixeDourado))
            {
                progress.RecordCatch(id, 100);
                progress.RecordCatch(id, 200);
            }

            Assert.IsFalse(achievements.IsUnlocked("ach_fish_test_all"));
            Assert.IsFalse(achievements.IsUnlocked("ach_fish_test_golden"));
            Assert.AreEqual(ids.Length - 1, achievements.GetProgress(all), "distinct species, not catches");
            Assert.AreEqual(0, completed);

            progress.RecordCatch(FishCatalog.PeixeDourado, 4000);
            Assert.IsTrue(achievements.IsUnlocked("ach_fish_test_all"));
            Assert.IsTrue(achievements.IsUnlocked("ach_fish_test_golden"));
            Assert.IsTrue(progress.Completed);
            Assert.AreEqual(1, completed);

            progress.RecordCatch(FishCatalog.PeixeDourado, 5000);
            Assert.AreEqual(1, completed, "once per save");
        }

        [Test]
        public void SaveLoad_RoundTrips_DropsUnknown_AndResets()
        {
            FishingProgress progress = NewProgress();
            progress.RecordCatch(FishCatalog.Surubim, 12000);
            progress.RecordCatch(FishCatalog.Surubim, 9000);
            progress.RecordCatch(FishCatalog.BagreCego, 700);

            var json = JsonUtility.ToJson(progress.CaptureState());
            var saved = JsonUtility.FromJson<FishingSaveState>(json);
            saved.entries.Add(new FishRecordEntry { fishId = "item_fish_removed", caught = 3, bestGrams = 10 });
            saved.entries.Add(new FishRecordEntry { fishId = FishCatalog.Piaba, caught = 0, bestGrams = 0 });

            FishingProgress restored = NewProgress();
            restored.RestoreState(saved);
            Assert.AreEqual(12000, restored.GetBestGrams(FishCatalog.Surubim));
            Assert.AreEqual(2, restored.GetCaughtCount(FishCatalog.Surubim));
            Assert.AreEqual(700, restored.GetBestGrams(FishCatalog.BagreCego));
            Assert.IsFalse(restored.HasCaught("item_fish_removed"));
            Assert.IsTrue(restored.HasCaught(FishCatalog.Piaba), "a corrupted 0 still means caught once");
            Assert.AreEqual(1, restored.GetCaughtCount(FishCatalog.Piaba));
            Assert.AreEqual(FishingProgress.Key, restored.SaveKey);

            restored.RestoreState(null);
            Assert.AreEqual(3, restored.CaughtSpeciesCount, "a missing entry (old save) changes nothing by itself");

            restored.ResetForNewSession();
            Assert.AreEqual(0, restored.CaughtSpeciesCount);
            Assert.IsFalse(restored.Completed);
        }

        [Test]
        public void AfterALoad_AchievementsAndDishesAreRefed()
        {
            string[] ids = { FishCatalog.Tilapia, FishCatalog.Piaba };
            AchievementDefinition pair = Definition("ach_fish_test_pair", 2, ids);
            var achievements = new AchievementManager(new[] { pair });
            MealService meals = NewMeals();
            FishingProgress progress = NewProgress(achievements, meals);

            progress.RestoreState(new FishingSaveState
            {
                entries = new List<FishRecordEntry>
                {
                    new FishRecordEntry { fishId = FishCatalog.Tilapia, caught = 2, bestGrams = 900 },
                    new FishRecordEntry { fishId = FishCatalog.Piaba, caught = 5, bestGrams = 80 },
                },
            });
            Assert.IsFalse(achievements.IsUnlocked("ach_fish_test_pair"));
            Assert.IsFalse(meals.IsKnown(MealCatalog.TilapiaNaBrasa));

            EventBus.Publish(new GameLoadedEvent());
            Assert.IsTrue(achievements.IsUnlocked("ach_fish_test_pair"));
            Assert.IsTrue(meals.IsKnown(MealCatalog.TilapiaNaBrasa));
            Assert.IsTrue(meals.IsKnown(MealCatalog.CaldoDePiaba));
        }

        // ------------------------------------------------------------------
        // Cooking
        // ------------------------------------------------------------------

        [Test]
        public void FishDishes_AreTaughtByTheirFirstCatch_AndCookWithFishFromTheBag()
        {
            MealService meals = NewMeals();
            FishingProgress progress = NewProgress(meals: meals);
            var learned = new List<MealRecipeLearnedEvent>();
            EventBus.Subscribe<MealRecipeLearnedEvent>(learned.Add);

            Assert.IsFalse(meals.IsKnown(MealCatalog.TilapiaNaBrasa));
            progress.RecordCatch(FishCatalog.Tilapia, 600);
            Assert.IsTrue(meals.IsKnown(MealCatalog.TilapiaNaBrasa));
            Assert.AreEqual(1, learned.Count);
            Assert.AreEqual(MealLearnSource.Fishing, learned[0].Source);

            progress.RecordCatch(FishCatalog.Tilapia, 700);
            Assert.AreEqual(1, learned.Count, "taught once");

            var bag = new Dictionary<string, int> { [MealCatalog.Wood] = 1, [MealCatalog.Herb] = 1, [MealCatalog.ProvisionsItemId] = 5 };
            System.Func<string, int> count = id => bag.TryGetValue(id, out int n) ? n : 0;
            System.Func<string, int, bool> remove = (id, n) => { if (count(id) < n) return false; bag[id] -= n; return true; };
            Assert.AreEqual(CookCheck.MissingIngredients, meals.Check(MealCatalog.TilapiaNaBrasa, count));
            bag[FishCatalog.Tilapia] = 1;
            Assert.AreEqual(CookCheck.Ok, meals.TryCook(MealCatalog.TilapiaNaBrasa, count, remove, out _));
            Assert.AreEqual(0, bag[FishCatalog.Tilapia], "the fish is eaten");
            Assert.AreEqual(MealCatalog.TilapiaNaBrasa, meals.Active.recipeId);
        }

        [Test]
        public void FishDishes_FollowTheCatalogRules()
        {
            List<MealRecipeSpec> fishDishes = MealCatalog.Recipes.Where(r => r.TaughtByCatchItemId != null).ToList();
            Assert.That(fishDishes.Count, Is.InRange(1, 2));
            foreach (MealRecipeSpec recipe in fishDishes)
            {
                Assert.IsNotNull(FishCatalog.Get(recipe.TaughtByCatchItemId), recipe.RecipeId);
                Assert.IsTrue(recipe.Ingredients.Any(i => i.ItemId == recipe.TaughtByCatchItemId), $"{recipe.RecipeId} uses its fish");
                Assert.IsFalse(recipe.Starter);
                Assert.IsNull(recipe.UnlockQuestId);
                Assert.IsFalse(recipe.FavouriteOf.HasValue, "every cousin keeps exactly one favourite");
                CollectionAssert.Contains(MealCatalog.TaughtByCatch(recipe.TaughtByCatchItemId), recipe);
                Assert.IsNotNull(KitchenTexts.GetFallback(recipe.NameKey));
                StringAssert.Contains(FishingTexts.GetFallback(ContentText.ItemNameKey(recipe.TaughtByCatchItemId)), KitchenPresenter.HowToLearn(recipe, null));
            }

            Assert.IsEmpty(MealCatalog.TaughtByCatch(FishCatalog.Surubim));
            Assert.IsEmpty(MealCatalog.TaughtByCatch(null));
        }

        // ------------------------------------------------------------------
        // Texts, presenter, achievements, audio
        // ------------------------------------------------------------------

        [Test]
        public void Texts_EveryFishSpotCousinAndRarityHasPtBrText_MatchingTheAssets()
        {
            foreach (FishSpec fish in FishCatalog.Fish)
            {
                ItemDefinition item = ItemDatabase.GetById(fish.Id);
                Assert.AreEqual(item.DisplayName, FishingTexts.GetFallback(ContentText.ItemNameKey(fish.Id)), $"{fish.Id} name drifted from the asset");
                Assert.AreEqual(item.Description, FishingTexts.GetFallback(ContentText.ItemDescriptionKey(fish.Id)), $"{fish.Id} description drifted from the asset");
            }

            foreach (FishingSpotSpec spot in FishCatalog.Spots)
            {
                Assert.IsNotNull(FishingTexts.GetFallback(spot.NameKey), spot.NameKey);
                Assert.IsNotNull(FishingTexts.GetFallback(spot.IntroKey), spot.IntroKey);
            }

            var heroes = System.Enum.GetValues(typeof(CharacterId)).Cast<CharacterId?>().Append(null).ToList();
            var reactions = new HashSet<string>();
            foreach (CharacterId? hero in heroes)
            {
                Assert.IsNotNull(FishingTexts.GetFallback(FishingTexts.EscapeKey(hero)), $"escape line for {hero}");
                foreach (FishRarity rarity in System.Enum.GetValues(typeof(FishRarity)))
                {
                    string line = FishingTexts.GetFallback(FishingTexts.ReactionKey(hero, rarity));
                    Assert.IsNotNull(line, $"{hero} {rarity}");
                    Assert.IsTrue(reactions.Add(line), $"{hero} {rarity}: every reaction is its own line");
                    Assert.IsNotNull(FishingTexts.GetFallback(FishingTexts.RarityKey(rarity)));
                }
            }
        }

        [Test]
        public void Texts_RegisterInTheManager_SoTheInventoryShowsEnglishNames()
        {
            var manager = new LocalizationManager();
            FishingTexts.EnsureRegistered(manager);
            Assert.IsTrue(manager.HasKey(ContentText.ItemNameKey(FishCatalog.PeixeDourado)));
            Assert.IsTrue(manager.HasKey(FishingTexts.Button));
        }

        [Test]
        public void Presenter_ButtonOnlyOnWaterRegions_WhenIdle()
        {
            Assert.IsTrue(FishingPresenter.ShouldShowButton(true, true, false, false, BiomeType.Forest));
            Assert.IsTrue(FishingPresenter.ShouldShowButton(true, true, false, false, BiomeType.Caves));
            Assert.IsFalse(FishingPresenter.ShouldShowButton(true, true, false, false, BiomeType.Desert));
            Assert.IsFalse(FishingPresenter.ShouldShowButton(true, true, true, false, BiomeType.Forest), "paused");
            Assert.IsFalse(FishingPresenter.ShouldShowButton(true, false, false, false, BiomeType.Forest), "token moving");
            Assert.IsFalse(FishingPresenter.ShouldShowButton(true, true, false, true, BiomeType.Forest), "talking");
            Assert.IsFalse(FishingPresenter.ShouldShowButton(false, true, false, false, BiomeType.Forest), "HUD hidden");
            Assert.IsFalse(FishingPresenter.ShouldShowButton(true, true, false, false, null));

            Assert.IsTrue(FishingPresenter.IsAssist(false, true), "Reduzir Movimento widens the window");
            Assert.IsTrue(FishingPresenter.IsAssist(true, false));
            Assert.IsFalse(FishingPresenter.IsAssist(false, false));
        }

        [Test]
        public void Presenter_WeightsResultsAndNotebook_InPtBr()
        {
            Assert.AreEqual("850 g", FishingPresenter.FormatWeight(null, 850));
            Assert.AreEqual("1,25 kg", FishingPresenter.FormatWeight(null, 1250));
            Assert.AreEqual("12,00 kg", FishingPresenter.FormatWeight(null, 12000));

            FishSpec golden = FishCatalog.Get(FishCatalog.PeixeDourado);
            var outcome = new FishCatchOutcome(golden.Id, 5400, true, true, 0);
            string landed = FishingPresenter.ResultText(null, FishingResult.Landed, golden, outcome, FishBagStatus.Added, CharacterId.Lavine);
            StringAssert.Contains("Peixe-Dourado do Açude", landed);
            StringAssert.Contains("lendário", landed);
            StringAssert.Contains("5,40 kg", landed);
            StringAssert.Contains(FishingTexts.GetFallback(FishingTexts.NewSpecies), landed);
            StringAssert.Contains(FishingTexts.GetFallback(FishingTexts.ReactionKey(CharacterId.Lavine, FishRarity.Legendary)), landed);

            string full = FishingPresenter.ResultText(null, FishingResult.Landed, golden, new FishCatchOutcome(golden.Id, 6000, false, true, 5400), FishBagStatus.BagFull, null);
            StringAssert.Contains("5,40 kg", full, "the record it beat");
            StringAssert.Contains(FishingTexts.GetFallback(FishingTexts.BagFull), full);

            string snapped = FishingPresenter.ResultText(null, FishingResult.Snapped, golden, default, FishBagStatus.Added, CharacterId.Joaquim);
            StringAssert.Contains(FishingTexts.GetFallback(FishingTexts.EscapeKey(CharacterId.Joaquim)), snapped);

            FishingProgress progress = NewProgress();
            progress.RecordCatch(FishCatalog.Tilapia, 1500);
            List<FishCollectionRow> rows = FishingPresenter.CollectionRows(null, progress);
            Assert.AreEqual(FishCatalog.Fish.Count, rows.Count);
            Assert.AreEqual(1, rows.Count(r => r.Caught));
            StringAssert.Contains("1,50 kg", rows.First(r => r.FishId == FishCatalog.Tilapia).Title);
            StringAssert.StartsWith("???", rows.First(r => r.FishId == FishCatalog.Surubim).Title);
            StringAssert.Contains("Riacho da Serra", rows.First(r => r.FishId == FishCatalog.Surubim).Where);
            StringAssert.Contains("(1/14)", FishingPresenter.CollectionButton(null, progress));
        }

        [Test]
        public void Presenter_HintsFollowTheCast()
        {
            var session = new FishingSession(Acude, TimeOfDayPhase.Dusk, WeatherType.Rain, false, 8);
            Assert.AreEqual(FishingTexts.GetFallback(FishingTexts.HintReady), FishingPresenter.Hint(null, session));
            Assert.AreEqual(FishingTexts.GetFallback(FishingTexts.ActionCast), FishingPresenter.ActionLabel(null, session));
            session.Press();
            TickUntil(session, () => session.Phase == FishingPhase.Waiting && session.PhaseTime >= session.Plan.TrembleAt);
            Assert.AreEqual(FishingTexts.GetFallback(FishingTexts.HintTremble), FishingPresenter.Hint(null, session));
            TickUntil(session, () => session.FloatSunk);
            Assert.AreEqual(FishingTexts.GetFallback(FishingTexts.HintBite), FishingPresenter.Hint(null, session));
            Assert.AreEqual(FishingTexts.GetFallback(FishingTexts.ActionHook), FishingPresenter.ActionLabel(null, session));
            StringAssert.Contains("Chuva", FishingPresenter.ConditionsLine(null, Acude, TimeOfDayPhase.Dusk, WeatherType.Rain));
            StringAssert.DoesNotContain("Chuva", FishingPresenter.ConditionsLine(null, Lago, TimeOfDayPhase.Dusk, WeatherType.Rain));
        }

        [Test]
        public void AchievementAssets_TargetTheCatalog_AndAreLocalized()
        {
            AchievementDefinition[] seeded = Resources.LoadAll<AchievementDefinition>(AchievementManager.ResourcesFolder);
            AchievementDefinition all = seeded.FirstOrDefault(d => d.AchievementId == "ach_fish_all");
            AchievementDefinition golden = seeded.FirstOrDefault(d => d.AchievementId == "ach_fish_golden");
            Assert.IsNotNull(all, "Resources/Achievements/ach_fish_all.asset");
            Assert.IsNotNull(golden, "Resources/Achievements/ach_fish_golden.asset");
            Assert.AreEqual(AchievementConditionType.FishSpeciesCaught, all.ConditionType);
            CollectionAssert.AreEquivalent(FishCatalog.AllIds(), all.TargetIds);
            Assert.AreEqual(FishCatalog.Fish.Count, all.TargetValue);
            CollectionAssert.AreEqual(new[] { FishCatalog.PeixeDourado }, golden.TargetIds);

            foreach (AchievementDefinition definition in new[] { all, golden })
            {
                foreach (string key in new[] { definition.TitleKey, definition.DescriptionKey })
                {
                    Assert.IsTrue(AchievementTexts.All.Any(e => e.Key == key && !string.IsNullOrEmpty(e.En) && !string.IsNullOrEmpty(e.PtBR)), key);
                }
            }
        }

        [Test]
        public void Audio_CuesAreAddedOnce_NeverOverrideTheAsset_AndNonePauses()
        {
            SfxEventMap map = SfxEventMap.CreateDefault();
            _created.Add(map);
            map.AddCue(new SfxEventMap.Cue { id = FishingAudio.Bite, volume = 0.9f, preset = ProceduralSfxPreset.Gong });

            int added = FishingAudio.EnsureCues(map);
            Assert.AreEqual(FishingAudio.BuildCues().Count - 1, added);
            Assert.AreEqual(0, FishingAudio.EnsureCues(map), "idempotent");
            Assert.IsTrue(map.TryGetCue(FishingAudio.Bite, out SfxEventMap.Cue authored));
            Assert.AreEqual(ProceduralSfxPreset.Gong, authored.preset, "an authored cue wins");

            foreach (SfxEventMap.Cue cue in FishingAudio.BuildCues())
            {
                Assert.IsFalse(cue.pausable, $"{cue.id}: the panel holds the pause");
                Assert.IsFalse(cue.loop, cue.id);
                Assert.AreNotEqual(ProceduralSfxPreset.None, cue.preset, cue.id);
            }

            foreach (FishingCue cue in System.Enum.GetValues(typeof(FishingCue)))
            {
                string id = FishingAudio.CueFor(cue, FishCatalog.Get(FishCatalog.Tilapia));
                if (id != null)
                {
                    Assert.IsTrue(map.TryGetCue(id, out _), $"{cue} → {id}");
                }
            }

            Assert.AreEqual(FishingAudio.CatchRare, FishingAudio.CueFor(FishingCue.Landed, FishCatalog.Get(FishCatalog.PeixeDourado)));
            Assert.AreEqual(FishingAudio.Catch, FishingAudio.CueFor(FishingCue.Landed, FishCatalog.Get(FishCatalog.Piaba)));
        }
    }
}
