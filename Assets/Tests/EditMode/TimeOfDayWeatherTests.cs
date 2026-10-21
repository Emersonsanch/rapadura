using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Menus;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the day/night + weather layer: <see cref="TimeOfDayManager"/> (progression,
    /// pause, phase/hour events, midnight wrap, SaveManager round-trip), <see cref="WeatherTable"/> /
    /// <see cref="WeatherSystem"/> (seeded determinism, per-biome tables, blending, persistence) and
    /// the night encounter multiplier read by <see cref="RandomEncounterSystem"/>.
    /// </summary>
    public class TimeOfDayWeatherTests
    {
        private const int TestSlot = 90051;

        private readonly List<Object> _created = new List<Object>();
        private readonly List<TimeOfDayManager> _clocks = new List<TimeOfDayManager>();
        private readonly List<WeatherSystem> _weathers = new List<WeatherSystem>();

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Clear();
            BiomeRegistry.SetForTests(null);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (TimeOfDayManager clock in _clocks)
            {
                clock.Shutdown();
            }

            foreach (WeatherSystem weather in _weathers)
            {
                weather.Shutdown();
            }

            _clocks.Clear();
            _weathers.Clear();
            ServiceLocator.Clear();
            BiomeRegistry.Invalidate();

            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();

            string slotPath = Path.Combine(Application.persistentDataPath, "Saves", $"slot_{TestSlot}.json");
            foreach (string path in new[] { slotPath, slotPath + ".bak", slotPath + ".tmp" })
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        // ------------------------------------------------------------------
        // Helpers.
        // ------------------------------------------------------------------

        private TimeOfDayManager MakeClock(float dayLength, float startHour, bool initialize = false)
        {
            var clock = new TimeOfDayManager(dayLength, startHour);
            if (initialize)
            {
                clock.Initialize();
                _clocks.Add(clock);
            }

            return clock;
        }

        private WeatherSystem MakeWeather(int seed, BiomeType biome = BiomeType.Forest)
        {
            return new WeatherSystem(seed, biome)
            {
                MinDurationSeconds = 10f,
                MaxDurationSeconds = 30f,
                TransitionSeconds = 4f,
            };
        }

        private BiomeDefinition MakeBiome(BiomeType type, List<BiomeEnemySpawnEntry> enemies = null, List<BiomeWeatherEntry> weather = null)
        {
            var biome = ScriptableObject.CreateInstance<BiomeDefinition>();
            biome.SetDataForTests(type, "biome.test.name", string.Empty, 0f, 20f, string.Empty, enemies, null);
            biome.SetWeatherForTests(weather);
            _created.Add(biome);
            return biome;
        }

        private EnemyDefinition MakeEnemy(string id)
        {
            var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
            enemy.name = id;
            _created.Add(enemy);
            return enemy;
        }

        // ------------------------------------------------------------------
        // TimeOfDayManager.
        // ------------------------------------------------------------------

        [Test]
        public void Tick_AdvancesHoursProportionallyToDayLength()
        {
            TimeOfDayManager clock = MakeClock(dayLength: 240f, startHour: 8f); // 10 real s = 1 in-game hour

            clock.Tick(10f);
            Assert.AreEqual(9f, clock.CurrentHour, 0.001f);

            clock.Tick(5f);
            Assert.AreEqual(9.5f, clock.CurrentHour, 0.001f);
            Assert.AreEqual(TimeOfDayPhase.Day, clock.Phase);
            Assert.AreEqual(9.5f / 24f, clock.NormalizedTime, 0.0001f);
        }

        [Test]
        public void Tick_DoesNothingWhilePausedByEvents()
        {
            TimeOfDayManager clock = MakeClock(240f, 8f, initialize: true);

            EventBus.Publish(new GamePausedEvent());
            clock.Tick(100f);
            Assert.AreEqual(8f, clock.CurrentHour, 0.001f);
            Assert.IsTrue(clock.IsPaused);

            EventBus.Publish(new GameResumedEvent());
            clock.Tick(10f);
            Assert.AreEqual(9f, clock.CurrentHour, 0.001f);
        }

        [Test]
        public void AdvanceHours_PublishesEveryPhaseCrossedInOrder()
        {
            TimeOfDayManager clock = MakeClock(240f, 4f);
            var phases = new List<TimeOfDayPhaseChangedEvent>();
            System.Action<TimeOfDayPhaseChangedEvent> handler = e => phases.Add(e);
            EventBus.Subscribe(handler);

            try
            {
                clock.AdvanceHours(4f); // 04:00 night -> 05 dawn -> 07 day -> 08:00
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.AreEqual(2, phases.Count);
            Assert.AreEqual(TimeOfDayPhase.Night, phases[0].PreviousPhase);
            Assert.AreEqual(TimeOfDayPhase.Dawn, phases[0].CurrentPhase);
            Assert.AreEqual(TimeOfDayPhase.Dawn, phases[1].PreviousPhase);
            Assert.AreEqual(TimeOfDayPhase.Day, phases[1].CurrentPhase);
            Assert.AreEqual(8f, clock.CurrentHour, 0.001f);
        }

        [Test]
        public void AdvanceHours_ThroughDuskIntoNight_AndHourEventCarriesHour()
        {
            TimeOfDayManager clock = MakeClock(240f, 17.5f);
            var phases = new List<TimeOfDayPhase>();
            int lastHour = -1;
            System.Action<TimeOfDayPhaseChangedEvent> phaseHandler = e => phases.Add(e.CurrentPhase);
            System.Action<TimeOfDayHourChangedEvent> hourHandler = e => lastHour = e.Hour;
            EventBus.Subscribe(phaseHandler);
            EventBus.Subscribe(hourHandler);

            try
            {
                clock.AdvanceHours(3f);
            }
            finally
            {
                EventBus.Unsubscribe(phaseHandler);
                EventBus.Unsubscribe(hourHandler);
            }

            CollectionAssert.AreEqual(new[] { TimeOfDayPhase.Dusk, TimeOfDayPhase.Night }, phases);
            Assert.AreEqual(20, lastHour);
        }

        [Test]
        public void AdvanceHours_WrapsAtMidnightAndIncrementsDay()
        {
            TimeOfDayManager clock = MakeClock(240f, 23f);
            int phaseEvents = 0;
            System.Action<TimeOfDayPhaseChangedEvent> handler = _ => phaseEvents++;
            EventBus.Subscribe(handler);

            try
            {
                clock.AdvanceHours(2f);
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.AreEqual(1f, clock.CurrentHour, 0.001f);
            Assert.AreEqual(2, clock.Day);
            Assert.AreEqual(0, phaseEvents, "Midnight is inside the night phase.");
        }

        [Test]
        public void Tick_IsCappedAtOneDayPerCall()
        {
            TimeOfDayManager clock = MakeClock(10f, 8f);

            clock.Tick(1000f);

            Assert.AreEqual(8f, clock.CurrentHour, 0.01f);
            Assert.AreEqual(2, clock.Day);
        }

        [Test]
        public void GetPhase_AndDaylightFactor_MatchBoundaries()
        {
            Assert.AreEqual(TimeOfDayPhase.Night, TimeOfDayManager.GetPhase(0f));
            Assert.AreEqual(TimeOfDayPhase.Dawn, TimeOfDayManager.GetPhase(5f));
            Assert.AreEqual(TimeOfDayPhase.Day, TimeOfDayManager.GetPhase(12f));
            Assert.AreEqual(TimeOfDayPhase.Dusk, TimeOfDayManager.GetPhase(19f));
            Assert.AreEqual(TimeOfDayPhase.Night, TimeOfDayManager.GetPhase(22f));

            Assert.AreEqual(0f, TimeOfDayManager.GetDaylightFactor(2f), 0.0001f);
            Assert.AreEqual(0.5f, TimeOfDayManager.GetDaylightFactor(6f), 0.0001f);
            Assert.AreEqual(1f, TimeOfDayManager.GetDaylightFactor(12f), 0.0001f);
            Assert.AreEqual(0.5f, TimeOfDayManager.GetDaylightFactor(19f), 0.0001f);
        }

        [Test]
        public void SaveManagerSlot_RoundTripsClockAndWeather()
        {
            var saveManager = new SaveManager();
            saveManager.Initialize();
            TimeOfDayManager clock = MakeClock(240f, 8f);
            WeatherSystem weather = MakeWeather(7, BiomeType.Mountain);
            saveManager.Register(clock);
            saveManager.Register(weather);

            clock.SetTime(21.25f);
            clock.AdvanceHours(24f); // day 2
            weather.ForceWeather(WeatherType.Snow, instant: true);
            float remaining = weather.SecondsUntilNextRoll;
            saveManager.Save(TestSlot);

            clock.SetTime(10f);
            weather.ForceWeather(WeatherType.Clear, instant: true);
            weather.SetBiome(BiomeType.Forest);

            Assert.IsTrue(saveManager.Load(TestSlot));
            Assert.AreEqual(21.25f, clock.CurrentHour, 0.001f);
            Assert.AreEqual(2, clock.Day);
            Assert.AreEqual(TimeOfDayPhase.Night, clock.Phase);
            Assert.AreEqual(WeatherType.Snow, weather.CurrentWeather);
            Assert.AreEqual(BiomeType.Mountain, weather.CurrentBiome);
            Assert.AreEqual(1f, weather.TransitionProgress, 0.0001f, "Loads snap, no blend.");
            Assert.AreEqual(remaining, weather.SecondsUntilNextRoll, 0.001f);
        }

        // ------------------------------------------------------------------
        // Weather.
        // ------------------------------------------------------------------

        [Test]
        public void WeatherTable_Roll_IsDeterministicForSeed()
        {
            var first = new List<WeatherType>();
            var second = new List<WeatherType>();
            var randomA = new System.Random(1234);
            var randomB = new System.Random(1234);

            for (int i = 0; i < 50; i++)
            {
                first.Add(WeatherTable.Roll(null, BiomeType.Mountain, randomA));
                second.Add(WeatherTable.Roll(null, BiomeType.Mountain, randomB));
            }

            CollectionAssert.AreEqual(first, second);
            CollectionAssert.Contains(first, WeatherType.Snow);
        }

        [Test]
        public void WeatherTable_Defaults_KeepBiomeSpecificStates()
        {
            var random = new System.Random(99);
            for (int i = 0; i < 200; i++)
            {
                WeatherType desert = WeatherTable.Roll(null, BiomeType.Desert, random);
                Assert.IsTrue(desert == WeatherType.Clear || desert == WeatherType.Sandstorm, desert.ToString());

                WeatherType forest = WeatherTable.Roll(null, BiomeType.Forest, random);
                Assert.AreNotEqual(WeatherType.Snow, forest);
                Assert.AreNotEqual(WeatherType.Sandstorm, forest);

                Assert.AreEqual(WeatherType.Clear, WeatherTable.Roll(null, BiomeType.Caves, random));
            }
        }

        [Test]
        public void WeatherTable_UsesAuthoredBiomeTable_AndFallsBackWhenEmptyOrZero()
        {
            BiomeDefinition rainy = MakeBiome(BiomeType.Forest, weather: new List<BiomeWeatherEntry>
            {
                new BiomeWeatherEntry(WeatherType.Rain, 5),
                new BiomeWeatherEntry(WeatherType.Clear, 0),
            });
            BiomeDefinition zero = MakeBiome(BiomeType.Caves, weather: new List<BiomeWeatherEntry>
            {
                new BiomeWeatherEntry(WeatherType.Rain, 0),
            });

            var random = new System.Random(5);
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(WeatherType.Rain, WeatherTable.Roll(rainy, BiomeType.Forest, random));
                Assert.AreEqual(WeatherType.Clear, WeatherTable.Roll(zero, BiomeType.Caves, random), "Zero-weight table falls back to Caves defaults.");
            }
        }

        [Test]
        public void WeatherTable_DoesNotAffectEnemyRolls()
        {
            EnemyDefinition wolf = MakeEnemy("enemy_test_wolf");
            BiomeDefinition biome = MakeBiome(
                BiomeType.Forest,
                new List<BiomeEnemySpawnEntry> { new BiomeEnemySpawnEntry(wolf, 1) },
                new List<BiomeWeatherEntry> { new BiomeWeatherEntry(WeatherType.Fog, 1) });

            Assert.AreSame(wolf, biome.RollEnemy(new System.Random(3)));
        }

        [Test]
        public void WeatherSystem_SameSeedAndTicks_ProduceSameSequence()
        {
            WeatherSystem a = MakeWeather(42, BiomeType.Mountain);
            WeatherSystem b = MakeWeather(42, BiomeType.Mountain);
            var sequenceA = new List<WeatherType>();
            var sequenceB = new List<WeatherType>();

            for (int i = 0; i < 400; i++)
            {
                a.Tick(1f);
                b.Tick(1f);
                sequenceA.Add(a.CurrentWeather);
                sequenceB.Add(b.CurrentWeather);
            }

            CollectionAssert.AreEqual(sequenceA, sequenceB);
            Assert.Greater(new HashSet<WeatherType>(sequenceA).Count, 1, "400 s with 10-30 s durations should see more than one state.");
        }

        [Test]
        public void WeatherSystem_ForceWeather_BlendsOverTransitionAndPublishes()
        {
            WeatherSystem weather = MakeWeather(1);
            weather.ForceWeather(WeatherType.Clear, instant: true);

            WeatherChangedEvent? published = null;
            System.Action<WeatherChangedEvent> handler = e => published = e;
            EventBus.Subscribe(handler);

            try
            {
                weather.ForceWeather(WeatherType.Rain);
            }
            finally
            {
                EventBus.Unsubscribe(handler);
            }

            Assert.IsTrue(published.HasValue);
            Assert.AreEqual(WeatherType.Clear, published.Value.PreviousWeather);
            Assert.AreEqual(WeatherType.Rain, published.Value.CurrentWeather);
            Assert.AreEqual(4f, published.Value.TransitionSeconds, 0.0001f);

            Assert.AreEqual(0f, weather.GetWeight(WeatherType.Rain), 0.0001f);
            Assert.AreEqual(1f, weather.GetWeight(WeatherType.Clear), 0.0001f);

            weather.Tick(2f);
            Assert.AreEqual(0.5f, weather.GetWeight(WeatherType.Rain), 0.0001f);
            Assert.AreEqual(0.5f, weather.GetWeight(WeatherType.Clear), 0.0001f);
            Assert.AreEqual(Mathf.Lerp(1f, WeatherTable.GetSunMultiplier(WeatherType.Rain), 0.5f), weather.SunIntensityMultiplier, 0.0001f);

            weather.Tick(10f);
            Assert.IsFalse(weather.IsTransitioning);
            Assert.AreEqual(1f, weather.GetWeight(WeatherType.Rain), 0.0001f);
        }

        [Test]
        public void WeatherSystem_PausedByEvents_FreezesBlend()
        {
            WeatherSystem weather = MakeWeather(1);
            weather.Initialize();
            _weathers.Add(weather);
            weather.ForceWeather(WeatherType.Fog);

            EventBus.Publish(new GamePausedEvent());
            weather.Tick(3f);
            Assert.AreEqual(0f, weather.TransitionProgress, 0.0001f);

            EventBus.Publish(new GameResumedEvent());
            weather.Tick(2f);
            Assert.AreEqual(0.5f, weather.TransitionProgress, 0.0001f);
        }

        [Test]
        public void WeatherSystem_RegionChangedEvent_SwitchesBiomeAndRollsFromItsTable()
        {
            BiomeRegistry.SetForTests(new[]
            {
                MakeBiome(BiomeType.Desert, weather: new List<BiomeWeatherEntry> { new BiomeWeatherEntry(WeatherType.Sandstorm, 1) }),
            });

            WeatherSystem weather = MakeWeather(3);
            weather.Initialize();
            _weathers.Add(weather);

            EventBus.Publish(new WorldMapRegionChangedEvent(BiomeType.Forest, BiomeType.Desert));

            Assert.AreEqual(BiomeType.Desert, weather.CurrentBiome);
            Assert.AreEqual(WeatherType.Sandstorm, weather.CurrentWeather);
        }

        // ------------------------------------------------------------------
        // Encounter multiplier.
        // ------------------------------------------------------------------

        [Test]
        public void EncounterChanceMultiplier_IsRaisedOnlyAtNight()
        {
            TimeOfDayManager clock = MakeClock(240f, 12f);
            Assert.AreEqual(1f, clock.EncounterChanceMultiplier, 0.0001f);

            clock.SetTime(22f);
            Assert.AreEqual(TimeOfDayManager.DefaultNightEncounterMultiplier, clock.EncounterChanceMultiplier, 0.0001f);
            Assert.Greater(clock.EncounterChanceMultiplier, 1f);

            clock.SetTime(6f);
            Assert.AreEqual(1f, clock.EncounterChanceMultiplier, 0.0001f);
        }

        [Test]
        public void RandomEncounterSystem_ReadsMultiplierFromRegisteredClock_DefaultOne()
        {
            var settings = new RandomEncounterSettings(EncounterStepMode.Distance, 1f, 1f, 0, 0.1f, 0f, 0.3f);
            var encounters = new RandomEncounterSystem(settings, 1);

            Assert.AreEqual(1f, encounters.ChanceMultiplier, 0.0001f, "No clock registered -> 1.");

            TimeOfDayManager clock = MakeClock(240f, 23f);
            ServiceLocator.Register(clock);
            Assert.AreEqual(clock.NightEncounterMultiplier, encounters.ChanceMultiplier, 0.0001f);

            clock.SetTime(12f);
            Assert.AreEqual(1f, encounters.ChanceMultiplier, 0.0001f);

            encounters.ChanceMultiplierProvider = null;
            clock.SetTime(23f);
            Assert.AreEqual(1f, encounters.ChanceMultiplier, 0.0001f, "Null provider means exactly 1.");
        }

        [Test]
        public void RandomEncounterSystem_MultiplierScalesStepChance()
        {
            BiomeDefinition biome = MakeBiome(
                BiomeType.Forest,
                new List<BiomeEnemySpawnEntry> { new BiomeEnemySpawnEntry(MakeEnemy("enemy_test_wolf"), 1) });
            var settings = new RandomEncounterSettings(EncounterStepMode.Distance, 1f, 1f, 0, 0.5f, 0f, 0.5f);

            var doubled = new RandomEncounterSystem(settings, 11) { ChanceMultiplierProvider = () => 2f };
            var zeroed = new RandomEncounterSystem(settings, 11) { ChanceMultiplierProvider = () => 0f };

            for (int i = 0; i < 20; i++)
            {
                Assert.IsTrue(doubled.RollStep(biome, out EncounterPayload payload), "0.5 x 2 = guaranteed.");
                Assert.IsNotNull(payload);
                Assert.IsFalse(zeroed.RollStep(biome, out _), "x0 never triggers.");
            }

            Assert.AreEqual(0.5f, doubled.GetChanceForStep(1), 0.0001f, "GetChanceForStep stays the unscaled base formula.");
        }
    }
}
