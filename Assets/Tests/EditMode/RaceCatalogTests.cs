using System.Linq;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Tests
{
    /// <summary>
    /// The 25 peoples (Docs/MetodologiaMapasERacas.md Part III, Docs/Mapas100.md §3): unique ids, homes that exist in the
    /// map catalog and sit in the people's own region, pt-BR + English texts, and the naming rules of the world bible
    /// (no folksy "Dona/Seu" names, no "naga").
    /// </summary>
    public class RaceCatalogTests
    {
        [Test]
        public void TwentyFivePeoples_UniqueIds()
        {
            Assert.AreEqual(25, RaceCatalog.Races.Count);
            Assert.AreEqual(25, RaceCatalog.Races.Select(r => r.Id).Distinct().Count());
            foreach (RaceDefinition race in RaceCatalog.Races)
            {
                StringAssert.IsMatch("^[a-z_]+$", race.Id);
                Assert.AreSame(race, RaceCatalog.Get(race.Id));
                Assert.IsNotEmpty(race.CosmeticTraits, race.Id);
                Assert.IsFalse(string.IsNullOrEmpty(race.NameSource), race.Id);
            }

            Assert.IsNull(RaceCatalog.Get("naga"));
            Assert.AreEqual(25, MapCatalog.Regions.Sum(region => RaceCatalog.InRegion(region).Count()), "every people has a home region");
        }

        [Test]
        public void HomeMaps_ExistAndSitInTheHomeRegion()
        {
            foreach (RaceDefinition race in RaceCatalog.Races.Where(r => r.HomeMapId != null))
            {
                MapDefinition home = MapCatalog.Get(race.HomeMapId);
                Assert.IsNotNull(home, $"{race.Id}: home map {race.HomeMapId}");
                Assert.AreEqual(race.HomeRegion, home.Region, race.Id);
            }

            Assert.AreEqual(MapCatalog.PomarId, RaceCatalog.Get("sogdian").HomeMapId);
        }

        [Test]
        public void Texts_PtBrAndEnglish_ForEveryPeople()
        {
            var entries = RaceTexts.Entries.ToDictionary(e => e.Key);
            Assert.AreEqual(50, entries.Count, "a name and a summary each");
            foreach (RaceDefinition race in RaceCatalog.Races)
            {
                foreach (string key in new[] { race.NameKey, race.SummaryKey })
                {
                    Assert.IsTrue(entries.TryGetValue(key, out var entry), $"missing {key}");
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), key);
                    Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), key);
                    Assert.AreEqual(entry.PtBR, RaceTexts.Get(null, key), "the fallback is pt-BR");
                    StringAssert.DoesNotContain("naga", entry.PtBR.ToLowerInvariant(), key);
                }

                Assert.AreNotEqual(entries[race.SummaryKey].En, entries[race.SummaryKey].PtBR, $"{race.Id}: the summary is translated");
            }

            Assert.AreEqual("Sogdianos da Alvorada", RaceTexts.Get(null, "race.sogdian.name"));
        }

        [Test]
        public void NotableNpcs_FollowTheWorldBible_NoFolksyTitles()
        {
            foreach (RaceDefinition race in RaceCatalog.Races)
            {
                Assert.AreEqual(race.NotableNpcs.Count, race.NotableNpcs.Distinct().Count(), race.Id);
                foreach (string npc in race.NotableNpcs)
                {
                    Assert.IsFalse(string.IsNullOrWhiteSpace(npc), race.Id);
                    Assert.IsFalse(npc.StartsWith("Dona ") || npc.StartsWith("Seu ") || npc.StartsWith("Vó Zefa"), $"{race.Id}: {npc}");
                }
            }
        }
    }
}
