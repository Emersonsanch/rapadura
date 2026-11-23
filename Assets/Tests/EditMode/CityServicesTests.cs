using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The small services of three race cities (<see cref="CityServiceRules"/>, Docs/Cities.md): Puzur's free rune of the day (Salões de
    /// Bronze), the Lira do Touro's song of the day (Aratta) and the Têmpera do Tambor (Tambor Rubro): the rules, the props they stand at
    /// in the recipes, where they land in the seamless world, and the texts in both languages for every cousin.
    /// </summary>
    public class CityServicesTests
    {
        private static readonly CharacterId[] Heroes = (CharacterId[])System.Enum.GetValues(typeof(CharacterId));

        [Test]
        public void Rune_OneADay_TheDayPicksIt_AllFourComeRound()
        {
            const long today = 20408L;
            Assert.AreEqual(CityServiceCheck.Ready, CityServiceRules.CheckRune(0L, today), "never taken");
            Assert.AreEqual(CityServiceCheck.Ready, CityServiceRules.CheckRune(today - 1L, today), "taken yesterday");
            Assert.AreEqual(CityServiceCheck.TakenToday, CityServiceRules.CheckRune(today, today));
            Assert.AreEqual(CityServiceRules.RuneOf(today), CityServiceRules.RuneOf(today), "the same for everyone that day");

            var seen = new HashSet<string>();
            for (long day = today; day < today + 60L; day++)
            {
                string rune = CityServiceRules.RuneOf(day);
                CollectionAssert.Contains(CityServiceRules.FreeRunes, rune);
                seen.Add(rune);
            }

            Assert.AreEqual(CityServiceRules.FreeRunes.Length, seen.Count, "every free rune comes round in two months");
            CollectionAssert.DoesNotContain(CityServiceRules.FreeRunes, "item_runa_cura_grande", "the Cura Grande stays for sale");
            foreach (string id in CityServiceRules.FreeRunes)
            {
                Assert.IsNotNull(ItemDatabase.GetById(id), id);
            }
        }

        [Test]
        public void Lyre_NewSongEveryDay_OutOfCombat_OnFoot()
        {
            for (long day = 20000L; day < 20100L; day++)
            {
                int song = CityServiceRules.SongOf(day);
                Assert.That(song, Is.InRange(0, CityServiceRules.SongCount - 1));
                Assert.AreNotEqual(CityServiceRules.SongOf(day - 1L), song, $"day {day}: a new song every day");
            }

            Assert.AreEqual(CityServiceCheck.Ready, CityServiceRules.CheckLyre(false));
            Assert.AreEqual(CityServiceCheck.InCombat, CityServiceRules.CheckLyre(true));
            Assert.IsTrue(CityServiceRules.SongApplies(10f, false, false, true));
            Assert.IsFalse(CityServiceRules.SongApplies(0f, false, false, true), "over");
            Assert.IsFalse(CityServiceRules.SongApplies(10f, true, false, true), "never in combat");
            Assert.IsFalse(CityServiceRules.SongApplies(10f, false, true, true), "on foot only");
            Assert.IsFalse(CityServiceRules.SongApplies(10f, false, false, false), "dead");
            Assert.AreEqual(300f, CityServiceRules.SongSeconds, "5 min (ficha 26)");
            Assert.AreEqual(0.05f, CityServiceRules.SongSpeedBonus, 1e-6f, "+5 % walking speed (ficha 26)");
        }

        [Test]
        public void Temper_OnlyWhileTheDrumsPlay_OnceADay_NotInAFight()
        {
            const long today = 20408L;
            Assert.AreEqual(CityServiceCheck.Ready, CityServiceRules.CheckTemper(true, 0L, today, false));
            Assert.AreEqual(CityServiceCheck.WaitDrums, CityServiceRules.CheckTemper(false, 0L, today, false));
            Assert.AreEqual(CityServiceCheck.InCombat, CityServiceRules.CheckTemper(true, today - 1L, today, true));
            Assert.AreEqual(CityServiceCheck.TakenToday, CityServiceRules.CheckTemper(true, today, today, false));
            Assert.AreEqual(CityServiceCheck.TakenToday, CityServiceRules.CheckTemper(false, today, today, true), "taken wins: no reason to wait");
            Assert.IsTrue(DrumRhythmRules.Playing(0d) && !DrumRhythmRules.Playing(120d), "the same beat as the Ritmo do Tambor");
        }

        [Test]
        public void Recipes_PutEachServiceOnItsProp()
        {
            AssertProp(CityTownRecipes.BronzeHalls(), CityPropKind.BronzePillar, CityTownRecipes.BronzeHallsRuneStone);
            AssertProp(CityTownRecipes.Aratta(), CityPropKind.BullLyre, CityTownRecipes.ArattaBullLyre);
            AssertProp(CityTownRecipes.TamborRubro(), CityPropKind.WarDrum, CityTownRecipes.TamborRubroTemperDrum);
        }

        [Test]
        public void World_EachServiceStandsInsideItsCity()
        {
            AssertInCity(OverworldLayouts.NapirasuId, OverworldLayouts.KaggenAnchorZoneId, OverworldLayouts.SaloesOffset, CityTownRecipes.BronzeHallsCityId,
                CityTownRecipes.BronzeHallsRuneStone);
            AssertInCity(OverworldLayouts.AuroquesId, OverworldLayouts.CostaAnchorZoneId, OverworldLayouts.ArattaOffset, CityTownRecipes.ArattaCityId,
                CityTownRecipes.ArattaBullLyre);
            AssertInCity(OverworldLayouts.TamborRubroMapId, OverworldLayouts.AreiasAnchorZoneId, OverworldLayouts.TamborRubroOffset,
                CityTownRecipes.TamborRubroCityId, CityTownRecipes.TamborRubroTemperDrum);
        }

        [Test]
        public void Texts_BothLanguages_EveryCousin_EverySong()
        {
            var keys = new HashSet<string>(CityServiceTexts.Entries.Select(e => e.Key));
            Assert.AreEqual(CityServiceTexts.Entries.Count, keys.Count, "no key twice");
            foreach (var entry in CityServiceTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key + " en");
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key + " pt");
                StringAssert.DoesNotContain("Dona ", entry.PtBR, entry.Key);
                StringAssert.DoesNotContain("Seu ", entry.PtBR, entry.Key);
            }

            foreach (CharacterId hero in Heroes)
            {
                Assert.IsTrue(keys.Contains(CityServiceTexts.RuneLine(hero)), "rune line " + hero);
                Assert.IsTrue(keys.Contains(CityServiceTexts.SongLine(hero)), "song line " + hero);
                Assert.IsTrue(keys.Contains(CityServiceTexts.TemperLine(hero)), "temper line " + hero);
            }

            for (int song = 0; song < CityServiceRules.SongCount; song++)
            {
                Assert.IsTrue(keys.Contains(CityServiceTexts.SongName(song)), "song " + song);
            }

            Assert.AreEqual("Recebido: Runa ×1", string.Format(CityServiceTexts.Get(null, CityServiceTexts.RuneReceived), "Runa", 1));
        }

        private static void AssertProp(CityTownRecipe recipe, CityPropKind kind, Vector2 at)
        {
            Assert.IsTrue(recipe.PlazaProps.Any(p => p.Kind == kind && (p.Position - at).sqrMagnitude < 0.01f), $"{recipe.CityId}: a {kind} at {at}");
        }

        private static void AssertInCity(string mapId, string anchorZoneId, Vector2 cityOffset, string cityId, Vector2 point)
        {
            OverworldLayout layout = OverworldLayouts.ForMap(mapId);
            Assert.IsNotNull(layout, mapId);
            OverworldZone anchor = layout.Zone(anchorZoneId);
            Assert.IsNotNull(anchor, anchorZoneId);
            OverworldSettlement city = layout.Settlements.FirstOrDefault(s => s.Recipe != null && s.Recipe.CityId == cityId);
            Assert.IsNotNull(city, cityId);
            Vector2 world = anchor.Origin + cityOffset + point;
            Assert.Less(Vector2.Distance(city.Offset + point, world), 0.01f, $"{cityId}: the driver's point is the recipe's point");
            Assert.AreSame(city, OverworldRules.SettlementAt(layout, world), $"{cityId}: inside the city's safe zone");
        }
    }
}
