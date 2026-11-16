using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Mechanics;
using Rapadura.Gameplay.World.Map.Overworld;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>"O painel do aviso" of the Ruínas dos Artífices (Docs/Mapas100.md 08, <see cref="ArtificesInscriptionRules"/>).</summary>
    public class ArtificesInscriptionTests
    {
        [Test]
        public void Reach_FindsTheClosestStela()
        {
            Vector2[] stelae = OverworldLayouts.ArtificesInscriptionStelae;
            Assert.AreEqual(0, ArtificesInscriptionRules.StelaAt(stelae[0] + new Vector2(2f, 0f), stelae));
            Assert.AreEqual(2, ArtificesInscriptionRules.StelaAt(stelae[2], stelae));
            Assert.AreEqual(-1, ArtificesInscriptionRules.StelaAt(stelae[0] + new Vector2(0f, 10f), stelae));
            Assert.AreEqual(-1, ArtificesInscriptionRules.StelaAt(Vector2.zero, null));
        }

        [Test]
        public void Experience_EachOnce_ThePanelGivesTheBonus_IcaroReadsBest()
        {
            Assert.AreEqual(ArtificesInscriptionRules.ReadExperience, ArtificesInscriptionRules.ExperienceFor(0, 3, false));
            Assert.AreEqual(ArtificesInscriptionRules.ReadExperience, ArtificesInscriptionRules.ExperienceFor(1, 3, false));
            Assert.AreEqual(ArtificesInscriptionRules.ReadExperience + ArtificesInscriptionRules.PanelExperience, ArtificesInscriptionRules.ExperienceFor(2, 3, false));
            Assert.AreEqual(0, ArtificesInscriptionRules.ExperienceFor(3, 3, false), "nothing left to learn");
            Assert.AreEqual(Mathf.RoundToInt(75 * ArtificesInscriptionRules.IcaroFactor), ArtificesInscriptionRules.ExperienceFor(2, 3, true));
            Assert.IsTrue(ArtificesInscriptionRules.Completes(2, 3));
            Assert.IsFalse(ArtificesInscriptionRules.Completes(1, 3));
            Assert.AreEqual(2, ArtificesInscriptionRules.ReadCount(new[] { true, false, true }));
        }

        [Test]
        public void Marks_SurviveTheSave()
        {
            var progress = new WorldMapProgress();
            progress.SetTimeMark(ArtificesInscriptionRules.MarkKey(1), 1L);
            var reloaded = new WorldMapProgress();
            reloaded.RestoreState(progress.CaptureState());
            Assert.Greater(reloaded.GetTimeMark(ArtificesInscriptionRules.MarkKey(1)), 0L);
            Assert.AreEqual(0L, reloaded.GetTimeMark(ArtificesInscriptionRules.MarkKey(0)));
        }

        [Test]
        public void Layout_StelaeStandOnTheAvenue_OfTheRuinsMap()
        {
            OverworldLayout w = OverworldLayouts.Get(OverworldLayouts.AreiasId);
            Assert.AreEqual(3, OverworldLayouts.ArtificesInscriptionStelae.Length);
            Vector2[] own = { new Vector2(-10.5f, -50f), new Vector2(10.5f, -80f), new Vector2(-10.5f, -110f) };
            for (int i = 0; i < own.Length; i++)
            {
                Vector2 stela = OverworldLayouts.ArtificesInscriptionStelae[i];
                Assert.AreEqual(OverworldLayouts.ArtificesOffset + own[i], stela, "stela " + i + " moved off the map's own stela");
                Assert.IsTrue(OverworldLayouts.ArtificesRuinsBounds.Contains(stela), "stela " + i);
                Assert.AreEqual(MapLayouts.ArtificesId, OverworldRules.ZoneAt(w, stela)?.MapId, "stela " + i);
            }
        }

        [Test]
        public void Texts_EveryStelaAndCousin_Translated()
        {
            Assert.AreNotEqual(ArtificesInscriptionTexts.StelaKey(0), ArtificesInscriptionTexts.StelaKey(2));
            Assert.AreEqual(ArtificesInscriptionTexts.LineIcaro, ArtificesInscriptionTexts.LineFor(CharacterId.Icaro));
            Assert.AreEqual(ArtificesInscriptionTexts.LineJoaquim, ArtificesInscriptionTexts.LineFor(null));
            foreach (var entry in ArtificesInscriptionTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.En), entry.Key);
                Assert.IsFalse(string.IsNullOrWhiteSpace(entry.PtBR), entry.Key);
            }
        }
    }
}
