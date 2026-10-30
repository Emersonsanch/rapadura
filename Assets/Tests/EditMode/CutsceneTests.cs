using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.Common;
using Rapadura.UI.Encounter;
using Rapadura.UI.Story;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>Intro / outro camera cutscenes and boss stingers (CutsceneScript, CutsceneSequence, CutscenePlayerController).</summary>
    public class CutsceneTests
    {
        [Test]
        public void Script_IntroBeforePrologue_OutroAfterEnding_NoIntroOnNewGamePlus()
        {
            Assert.AreSame(CutsceneScript.Intro, CutsceneScript.For(StoryBeat.Prologue, CutscenePlacement.BeforePanels, 0));
            Assert.IsNull(CutsceneScript.For(StoryBeat.Prologue, CutscenePlacement.BeforePanels, 1), "Nova Jornada+ has its own prologue");
            Assert.IsNull(CutsceneScript.For(StoryBeat.Prologue, CutscenePlacement.AfterPanels, 0));
            Assert.AreSame(CutsceneScript.Outro, CutsceneScript.For(StoryBeat.Ending, CutscenePlacement.AfterPanels, 0));
            Assert.AreSame(CutsceneScript.Outro, CutsceneScript.For(StoryBeat.Ending, CutscenePlacement.AfterPanels, 2));
            Assert.IsNull(CutsceneScript.For(StoryBeat.InterludeForest, CutscenePlacement.BeforePanels, 0));
        }

        [Test]
        public void Outro_EndsOnVoEdnasPorch()
        {
            CutsceneShot last = CutsceneScript.Outro.Shots[CutsceneScript.Outro.Shots.Count - 1];
            Assert.AreEqual(CutsceneFocus.Village, last.To.Focus);
            StringAssert.Contains("varanda da Vó Edna", CutsceneTexts.GetFallback(last.LineKey));
            StringAssert.Contains("Grandma Edna's porch", CutsceneTexts.Entries.First(e => e.Key == last.LineKey).En);
        }

        [Test]
        public void EveryCutsceneKey_IsLocalizedInBothLanguages()
        {
            var table = CutsceneTexts.Entries.ToDictionary(e => e.Key);
            foreach (string key in CutsceneScript.AllKeys())
            {
                Assert.IsTrue(table.TryGetValue(key, out var entry), key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), key);
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), key);
            }
        }

        [Test]
        public void Texts_UseTheNewGrandparentNames()
        {
            foreach (var entry in CutsceneTexts.Entries)
            {
                foreach (string text in new[] { entry.En, entry.PtBR })
                {
                    StringAssert.DoesNotContain("Zefa", text, entry.Key);
                    StringAssert.DoesNotContain("Chico", text, entry.Key);
                }
            }
        }

        [Test]
        public void Sequence_AutoAdvances_TapsNext_AndFinishes()
        {
            var changes = new List<CutsceneChangedEvent>();
            System.Action<CutsceneChangedEvent> onChange = changes.Add;
            EventBus.Subscribe(onChange);
            try
            {
                var sequence = new CutsceneSequence(CutsceneScript.Intro, _ => 3f);
                Assert.AreEqual(0, sequence.Index);
                Assert.IsTrue(sequence.Tick(1.5f));
                Assert.AreEqual(0.5f, sequence.ShotProgress, 1e-4f);
                Assert.IsTrue(sequence.Tick(1.6f));
                Assert.AreEqual(1, sequence.Index, "shot time ran out");
                Assert.IsTrue(sequence.Next());
                Assert.AreEqual(2, sequence.Index);

                while (sequence.Next())
                {
                }

                Assert.IsTrue(sequence.IsFinished);
                Assert.IsFalse(sequence.WasSkipped);
                Assert.AreEqual(CutsceneChange.Started, changes.First().Change);
                Assert.AreEqual(CutsceneChange.Finished, changes.Last().Change);
                Assert.IsFalse(sequence.Tick(1f));
            }
            finally
            {
                EventBus.Unsubscribe(onChange);
            }
        }

        [Test]
        public void Sequence_Skip_EndsAtOnce()
        {
            var sequence = new CutsceneSequence(CutsceneScript.Outro);
            sequence.Skip();
            Assert.IsTrue(sequence.IsFinished);
            Assert.IsTrue(sequence.WasSkipped);
            Assert.IsFalse(sequence.Next());
        }

        [Test]
        public void Sequence_ShadowBlendsIn_ForTheSealHook()
        {
            var sequence = new CutsceneSequence(CutsceneScript.Intro, _ => 4f);
            Assert.AreEqual(0f, sequence.Shadow, 1e-4f, "the farm is warm at first");
            int hook = CutsceneScript.Intro.Shots.ToList().FindIndex(s => s.To.Focus == CutsceneFocus.Caves);
            Assert.Greater(hook, 0, "the intro crosses the map to the Caves");
            for (int i = 0; i < hook; i++)
            {
                sequence.Next();
            }

            float start = sequence.Shadow;
            sequence.Tick(3f);
            Assert.Greater(sequence.Shadow, start);
        }

        [Test]
        public void Rules_ReadingTimeClamped_AndPlayOncePerSession()
        {
            Assert.AreEqual(CutsceneRules.MinShotSeconds, CutsceneRules.ReadingSeconds(0), 1e-4f);
            Assert.AreEqual(CutsceneRules.MaxShotSeconds, CutsceneRules.ReadingSeconds(10000), 1e-4f);
            Assert.Greater(CutsceneRules.ReadingSeconds(120), CutsceneRules.ReadingSeconds(40));

            Assert.IsTrue(CutsceneRules.ShouldPlay(CutsceneScript.Intro, autoPlay: true, alreadyPlayed: false));
            Assert.IsFalse(CutsceneRules.ShouldPlay(CutsceneScript.Intro, autoPlay: false, alreadyPlayed: false), "PlayMode tests switch them off");
            Assert.IsFalse(CutsceneRules.ShouldPlay(CutsceneScript.Intro, autoPlay: true, alreadyPlayed: true));
            Assert.IsFalse(CutsceneRules.ShouldPlay(null, true, false));
        }

        [Test]
        public void CameraMath_OrbitLooksAtTheFocus_AndReducedMotionHoldsTheEndFraming()
        {
            Vector3 village = new Vector3(-6f, 0f, 2f);
            Vector3 caves = new Vector3(6f, 0f, 8f);
            System.Func<CutsceneFocus, Vector3> focus = f => f == CutsceneFocus.Caves ? caves : village;

            var shot = new CutsceneShot(new CutsceneFraming(CutsceneFocus.Village, 0f, 45f, 10f), new CutsceneFraming(CutsceneFocus.Caves, 30f, 30f, 5f), CutsceneSpeaker.Narrator, "x");

            CutsceneCameraMath.Pose(shot, 0f, focus, 1f, false, out Vector3 p0, out Quaternion r0, out _);
            Vector3 toFocus = (village - p0).normalized;
            Assert.Greater(Vector3.Dot(r0 * Vector3.forward, toFocus), 0.999f);
            Assert.AreEqual(10f, Vector3.Distance(p0, village), 1e-3f);
            Assert.Greater(p0.y, 0f, "above the map");

            CutsceneCameraMath.Pose(shot, 1f, focus, 2f, false, out Vector3 p1, out _, out _);
            Assert.AreEqual(10f, Vector3.Distance(p1, caves), 1e-3f, "distance × map scale");

            CutsceneCameraMath.Pose(shot, 0f, focus, 2f, true, out Vector3 still, out _, out _);
            Assert.AreEqual(p1, still, "Reduzir Movimento: a cut to the end framing, no glide");
        }

        [Test]
        public void CameraMath_IslandCentre_AndVillageIsTheForestNode()
        {
            CutsceneCameraMath.Island(new[] { new Vector3(-10f, 0f, 0f), new Vector3(10f, 0f, 0f) }, out Vector3 centre, out float spread);
            Assert.AreEqual(Vector3.zero, centre);
            Assert.AreEqual(10f, spread, 1e-4f);
            Assert.AreEqual(1f, CutsceneCameraMath.MapScale(spread), 1e-4f);

            Assert.AreEqual(BiomeType.Forest, CutsceneCameraMath.RegionOf(CutsceneFocus.Village));
            Assert.IsNull(CutsceneCameraMath.RegionOf(CutsceneFocus.Island));
            Vector3 forest = new Vector3(1f, 0f, 1f);
            Assert.AreEqual(forest, CutsceneCameraMath.FocusPoint(CutsceneFocus.Village, r => r == BiomeType.Forest ? forest : (Vector3?)null, Vector3.zero));
            Assert.AreEqual(Vector3.one, CutsceneCameraMath.FocusPoint(CutsceneFocus.Caves, r => null, Vector3.one), "missing node: island centre");
        }

        [Test]
        public void BossStinger_IsTheActiveCousinsQuip_OrTheNarrator()
        {
            string withHero = BossIntroBannerController.StingerText(null, BiomeType.Forest, CharacterId.Joaquim);
            StringAssert.StartsWith("Joaquim: ", withHero);
            StringAssert.Contains(CutsceneTexts.GetFallback("cutscene.stinger.forest.joaquim"), withHero);
            Assert.AreEqual(CutsceneTexts.GetFallback("cutscene.stinger.caves"), BossIntroBannerController.StingerText(null, BiomeType.Caves, null));
            Assert.AreEqual(string.Empty, BossIntroBannerController.StingerText(null, null, CharacterId.Maithe));

            VisualElement banner = BossIntroBannerController.CreateView(reducedMotion: false);
            Label stinger = banner.Q<Label>(BossIntroBannerController.StingerName);
            Assert.IsNotNull(stinger);
            Assert.AreEqual(PickingMode.Ignore, stinger.pickingMode);
            StringAssert.IsMatch(@"(?m)^\.boss-intro-stinger\s*\{", File.ReadAllText("Assets/Resources/UI/EncounterOverlays.uss"));
        }

        [Test]
        public void PlayerView_HasLetterboxSubtitlesPortraitAndSkip_AllStyled()
        {
            StyleSheet sheet = Resources.Load<StyleSheet>(CutscenePlayerController.StyleResourcePath);
            Assert.IsNotNull(sheet, "Resources/UI/Cutscene.uss missing");
            VisualElement root = CutscenePlayerController.CreateView(sheet, reducedMotion: true);
            Assert.IsTrue(root.ClassListContains(CutscenePlayerController.ThemeClass));
            Assert.IsTrue(root.ClassListContains(UiMotionPreferences.ReducedMotionClass));
            Assert.AreEqual(DisplayStyle.None, root.style.display.value, "hidden until a cutscene plays");

            string uss = File.ReadAllText("Assets/Resources/UI/Cutscene.uss");
            foreach (string name in new[] { "cutscene-bar-top", "cutscene-bar-bottom", "cutscene-title", "cutscene-skip", "cutscene-portrait", "cutscene-speaker", "cutscene-line", "cutscene-tint-shadow", "cutscene-tint-light" })
            {
                VisualElement element = root.Q(name);
                Assert.IsNotNull(element, name);
                foreach (string cls in element.GetClasses().Where(c => c.StartsWith("cutscene-")))
                {
                    StringAssert.IsMatch(@"(?m)^\." + System.Text.RegularExpressions.Regex.Escape(cls) + @"\s*[,{]", uss, $".{cls} is not styled");
                }
            }

            Assert.IsNotNull(root.Q<Button>("cutscene-skip"), "every cutscene is skippable");
            StringAssert.Contains(".rap-reduced-motion .cutscene-bar", uss);
        }
    }
}
