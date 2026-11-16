using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Hunting;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>The Ritmo do Tambor of the Tambor Rubro (Docs/Mapas100.md 11, <see cref="DrumRhythmRules"/>): the beat, the city, the stamina and the hunt point.</summary>
    public class DrumRhythmTests
    {
        [SetUp]
        public void SetUp() => EventBus.Clear();

        [TearDown]
        public void TearDown() => EventBus.Clear();

        [Test]
        public void Beat_PlaysTheFirstMinuteOfEveryTen_WarnsBefore()
        {
            Assert.IsTrue(DrumRhythmRules.Playing(0d));
            Assert.IsTrue(DrumRhythmRules.Playing(59.9d));
            Assert.IsFalse(DrumRhythmRules.Playing(60d));
            Assert.IsFalse(DrumRhythmRules.Playing(589d));
            Assert.IsTrue(DrumRhythmRules.Warning(591d));
            Assert.IsFalse(DrumRhythmRules.Warning(580d));
            Assert.IsTrue(DrumRhythmRules.Playing(600d), "the next cycle");
            Assert.AreEqual(DrumRhythmRules.BeatIndex(600d), DrumRhythmRules.BeatIndex(1199d));
            Assert.AreNotEqual(DrumRhythmRules.BeatIndex(599d), DrumRhythmRules.BeatIndex(600d));
            Assert.AreEqual(0d, DrumRhythmRules.SecondsToBeat(30d));
            Assert.AreEqual(100d, DrumRhythmRules.SecondsToBeat(500d), 1e-6);
            Assert.IsTrue(DrumRhythmRules.Playing(-590d), "negative clocks stay in the cycle");
        }

        [Test]
        public void Stamina_OnlyInTheCity_WhileTheDrumsPlay_AndAlive()
        {
            Assert.AreEqual(DrumRhythmRules.StaminaFactor, DrumRhythmRules.HeroStaminaFactor(true, true, true));
            Assert.AreEqual(1f, DrumRhythmRules.HeroStaminaFactor(false, true, true));
            Assert.AreEqual(1f, DrumRhythmRules.HeroStaminaFactor(true, false, true));
            Assert.AreEqual(1f, DrumRhythmRules.HeroStaminaFactor(true, true, false));
            Assert.AreEqual(1.1f, DrumRhythmRules.StaminaFactor, 1e-5f, "the sheet's +10%");
        }

        [Test]
        public void City_IsTheRecipeSquare_InsideTheBadlands()
        {
            Rect city = DrumRhythmRules.CityRect(OverworldLayouts.TamborRubroOffset);
            Assert.IsTrue(city.Contains(OverworldLayouts.TamborRubroOffset));
            Assert.IsTrue(city.Contains(OverworldLayouts.TamborRubroOffset + new Vector2(30f, -30f)));
            Assert.IsFalse(city.Contains(OverworldLayouts.TamborRubroOffset + new Vector2(40f, 0f)));
            Assert.IsTrue(OverworldLayouts.TamborRubroBadlandsBounds.Contains(city.min) && OverworldLayouts.TamborRubroBadlandsBounds.Contains(city.max),
                "the city sits inside the Ermos");
        }

        [Test]
        public void TaskPoint_WhileTheDrumsPlay_InTheBadlands_AddsToTheHunt()
        {
            Assert.AreEqual(1, DrumRhythmRules.TaskBonusPoints(true, true));
            Assert.AreEqual(0, DrumRhythmRules.TaskBonusPoints(false, true));
            Assert.AreEqual(0, DrumRhythmRules.TaskBonusPoints(true, false));

            var hunting = new HuntingProgress(random: () => 0.5);
            hunting.AddBonusHuntPoints(DrumRhythmRules.TaskBonusPoints(true, true));
            hunting.AddBonusHuntPoints(0);
            hunting.AddBonusHuntPoints(-3);
            Assert.AreEqual(1, hunting.HuntPointsEarned);
            Assert.AreEqual(1, hunting.HuntPointsAvailable);
        }

        [Test]
        public void Lines_EveryCousinAndAshmunikal_Translated()
        {
            Assert.AreEqual(DrumRhythmTexts.LineAshmunikal, DrumRhythmTexts.LineFor(CharacterId.Maria, 2));
            Assert.AreEqual(DrumRhythmTexts.LineAshmunikal, DrumRhythmTexts.LineFor(null, 3));
            foreach (CharacterId hero in System.Enum.GetValues(typeof(CharacterId)))
            {
                string key = DrumRhythmTexts.LineFor(hero, 1);
                Assert.AreNotEqual(DrumRhythmTexts.LineAshmunikal, key, hero.ToString());
            }

            foreach (var entry in DrumRhythmTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
                Assert.AreEqual(entry.PtBR, DrumRhythmTexts.Get(null, entry.Key));
            }
        }
    }
}
