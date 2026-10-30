using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rapadura.Audio;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Enemies.Visuals;
using Rapadura.Gameplay.Pets;
using Rapadura.Gameplay.Pets.Visuals;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.UI.WorldMap;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rapadura.Tests
{
    /// <summary>
    /// Caramelo, the companion dog: the pure follow / alert / fetch / mode rules (<see cref="CompanionMath"/>),
    /// the settings toggle and join rules (<see cref="CompanionPreferences"/>), the cuddle lines and buff
    /// (<see cref="CarameloCuddle"/>, <see cref="CarameloTexts"/>), the world map button rule, the procedural
    /// model (<see cref="CarameloModel"/>) and his sound cues in the SFX map.
    /// </summary>
    public class CarameloCompanionTests
    {
        private readonly List<Object> _owned = new List<Object>();
        private bool _hadEnabledKey;
        private int _savedEnabled;
        private bool _hadIntroducedKey;
        private int _savedIntroduced;

        [SetUp]
        public void SetUp()
        {
            _hadEnabledKey = PlayerPrefs.HasKey(CompanionPreferences.CarameloEnabledKey);
            _savedEnabled = PlayerPrefs.GetInt(CompanionPreferences.CarameloEnabledKey, 1);
            _hadIntroducedKey = PlayerPrefs.HasKey(CompanionPreferences.IntroducedKey);
            _savedIntroduced = PlayerPrefs.GetInt(CompanionPreferences.IntroducedKey, 0);
            CarameloCuddle.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            Restore(CompanionPreferences.CarameloEnabledKey, _hadEnabledKey, _savedEnabled);
            Restore(CompanionPreferences.IntroducedKey, _hadIntroducedKey, _savedIntroduced);
            CarameloCuddle.ResetForTests();
            foreach (Object o in _owned)
            {
                if (o != null) Object.DestroyImmediate(o);
            }

            _owned.Clear();
        }

        private static void Restore(string key, bool had, int value)
        {
            if (had) PlayerPrefs.SetInt(key, value);
            else PlayerPrefs.DeleteKey(key);
        }

        // ------------------------------------------------------------------
        // Follow
        // ------------------------------------------------------------------

        [Test]
        public void FollowSlot_IsBehindAndBesideTheHero()
        {
            Vector3 hero = new Vector3(3f, 1f, 5f);
            Vector3 right = CompanionMath.FollowSlot(hero, Vector3.forward, 1f);
            Vector3 left = CompanionMath.FollowSlot(hero, Vector3.forward, -1f);

            Assert.AreEqual(hero.z - CompanionMath.FollowBack, right.z, 1e-4f, "behind");
            Assert.AreEqual(hero.x + CompanionMath.FollowSide, right.x, 1e-4f, "right side");
            Assert.AreEqual(hero.x - CompanionMath.FollowSide, left.x, 1e-4f, "left side");
            Assert.AreEqual(hero.y, right.y, 1e-4f, "hero height");

            // Facing +X: behind is -X, the right is -Z.
            Vector3 east = CompanionMath.FollowSlot(Vector3.zero, Vector3.right * 4f, 1f);
            Assert.AreEqual(-CompanionMath.FollowBack, east.x, 1e-4f);
            Assert.AreEqual(-CompanionMath.FollowSide, east.z, 1e-4f);

            // Degenerate forward (looking straight down) still gives a slot.
            Vector3 fallback = CompanionMath.FollowSlot(Vector3.zero, Vector3.down, 1f);
            Assert.IsFalse(float.IsNaN(fallback.x) || float.IsNaN(fallback.z));
            Assert.AreEqual(-CompanionMath.FollowBack, fallback.z, 1e-4f);
        }

        [Test]
        public void ArrivalSpeed_EasesInsideTheSlowRadius_AndStopsAtTheSlot()
        {
            Assert.AreEqual(0f, CompanionMath.ArrivalSpeed(CompanionMath.StopRadius * 0.5f, 5f));
            Assert.AreEqual(5f, CompanionMath.ArrivalSpeed(CompanionMath.SlowRadius, 5f), 1e-4f);
            Assert.AreEqual(5f, CompanionMath.ArrivalSpeed(20f, 5f), 1e-4f);

            float previous = 0f;
            for (float d = CompanionMath.StopRadius; d <= CompanionMath.SlowRadius; d += 0.1f)
            {
                float speed = CompanionMath.ArrivalSpeed(d, 5f);
                Assert.GreaterOrEqual(speed, previous - 1e-5f, "monotonic ease");
                previous = speed;
            }

            Assert.AreEqual(5f, CompanionMath.ArrivalSpeed(1f, 5f, 0.5f, 0.5f), "degenerate radii → full speed");
        }

        [Test]
        public void CatchUpSpeed_OutrunsAFarHero_AndNeverLagsBehindIt()
        {
            float near = CompanionMath.CatchUpSpeed(1f, 0f, 4f);
            float far = CompanionMath.CatchUpSpeed(12f, 0f, 4f);
            Assert.AreEqual(4f, near, 1e-4f);
            Assert.Greater(far, near, "boost when far");
            Assert.GreaterOrEqual(CompanionMath.CatchUpSpeed(1f, 8f, 4f), 8f, "keeps up with a sprinting hero");
        }

        [Test]
        public void ShouldTeleport_OnlyWhenFarFromTheHero_OnThePlane()
        {
            Assert.IsFalse(CompanionMath.ShouldTeleport(Vector3.zero, new Vector3(3f, 20f, 0f)), "height ignored");
            Assert.IsTrue(CompanionMath.ShouldTeleport(Vector3.zero, new Vector3(CompanionMath.TeleportDistance + 1f, 0f, 0f)));
        }

        // ------------------------------------------------------------------
        // Alert (telegraph helper) and fetch
        // ------------------------------------------------------------------

        [Test]
        public void SelectAlert_PicksTheTelegraphClosestToTheHero_WithinRange()
        {
            var enemies = new List<Vector3> { new Vector3(8f, 0f, 0f), new Vector3(0f, 0f, 3f), new Vector3(30f, 0f, 0f) };
            Assert.AreEqual(1, CompanionMath.SelectAlert(enemies, Vector3.zero));
            Assert.AreEqual(-1, CompanionMath.SelectAlert(new List<Vector3> { new Vector3(30f, 0f, 0f) }, Vector3.zero), "too far");
            Assert.AreEqual(-1, CompanionMath.SelectAlert(new List<Vector3>(), Vector3.zero));
            Assert.AreEqual(-1, CompanionMath.SelectAlert(null, Vector3.zero));
        }

        [Test]
        public void AlertStandPoint_StopsShortOfTheEnemy_OnTheHeroSide()
        {
            Vector3 enemy = new Vector3(10f, 0f, 0f);
            Vector3 point = CompanionMath.AlertStandPoint(enemy, Vector3.zero);
            Assert.AreEqual(10f - CompanionMath.AlertStandOff, point.x, 1e-4f);
            Assert.AreEqual(0f, point.z, 1e-4f);

            // Enemy closer than the stand-off: he stops at the hero, never past them.
            Vector3 close = CompanionMath.AlertStandPoint(new Vector3(1f, 0f, 0f), Vector3.zero);
            Assert.AreEqual(0f, close.x, 1e-4f);
            Assert.AreEqual(Vector3.zero, CompanionMath.AlertStandPoint(Vector3.zero, Vector3.zero));
        }

        [Test]
        public void SelectFetch_PicksTheSpotClosestToTheDog_AmongThoseNearTheHero()
        {
            var spots = new List<Vector3> { new Vector3(5f, 0f, 0f), new Vector3(-3f, 0f, 0f), new Vector3(40f, 0f, 0f) };
            Vector3 hero = Vector3.zero;
            Assert.AreEqual(1, CompanionMath.SelectFetch(spots, hero, new Vector3(-2f, 0f, 0f)));
            Assert.AreEqual(0, CompanionMath.SelectFetch(spots, hero, new Vector3(4f, 0f, 0f)));
            Assert.AreEqual(0, CompanionMath.SelectFetch(spots, hero, new Vector3(39f, 0f, 0f)), "the far spot is out of the hero's radius");
            Assert.AreEqual(-1, CompanionMath.SelectFetch(new List<Vector3> { new Vector3(40f, 0f, 0f) }, hero, hero));
            Assert.AreEqual(-1, CompanionMath.SelectFetch(null, hero, hero));
        }

        [Test]
        public void ChooseMode_HideBeatsAlertBeatsFetchBeatsFollow()
        {
            Assert.AreEqual(CompanionMode.Hide, CompanionMath.ChooseMode(true, true, true));
            Assert.AreEqual(CompanionMode.Alert, CompanionMath.ChooseMode(false, true, true));
            Assert.AreEqual(CompanionMode.Fetch, CompanionMath.ChooseMode(false, false, true));
            Assert.AreEqual(CompanionMode.Follow, CompanionMath.ChooseMode(false, false, false));
        }

        [Test]
        public void IdleRules_SitOnlyWhileFollowing_SniffIsDeterministicAndBounded()
        {
            Assert.IsFalse(CompanionMath.ShouldSit(CompanionMode.Follow, CompanionMath.SitDelay - 0.1f));
            Assert.IsTrue(CompanionMath.ShouldSit(CompanionMode.Follow, CompanionMath.SitDelay));
            Assert.IsFalse(CompanionMath.ShouldSit(CompanionMode.Alert, 99f));
            Assert.IsFalse(CompanionMath.ShouldSit(CompanionMode.Hide, 99f));

            Assert.AreEqual(0f, CompanionMath.SniffWeight(1f, 4f, 3), "nothing during the first interval");
            float max = 0f;
            for (float t = 0f; t < 60f; t += 0.05f)
            {
                float w = CompanionMath.SniffWeight(t, 4f, 3);
                Assert.AreEqual(w, CompanionMath.SniffWeight(t, 4f, 3), "deterministic");
                Assert.That(w, Is.InRange(0f, 1f));
                max = Mathf.Max(max, w);
            }

            Assert.Greater(max, 0.9f, "he does sniff eventually");
            Assert.AreEqual(0f, CompanionMath.TailWagFrequency(3f, true, true), "tail tucked while hiding");
            Assert.Greater(CompanionMath.TailWagFrequency(0f, true, false), CompanionMath.TailWagFrequency(0f, false, false), "happy wags faster");
        }

        // ------------------------------------------------------------------
        // Settings toggle and join rules
        // ------------------------------------------------------------------

        [Test]
        public void SettingsToggle_PersistsAndNotifies_DefaultOn()
        {
            PlayerPrefs.DeleteKey(CompanionPreferences.CarameloEnabledKey);
            Assert.IsTrue(CompanionPreferences.CarameloEnabled, "on by default");

            int changes = 0;
            Action onChanged = () => changes++;
            CompanionPreferences.Changed += onChanged;
            try
            {
                CompanionPreferences.CarameloEnabled = false;
                Assert.IsFalse(CompanionPreferences.CarameloEnabled);
                Assert.AreEqual(0, PlayerPrefs.GetInt(CompanionPreferences.CarameloEnabledKey));
                CompanionPreferences.CarameloEnabled = false;
                Assert.AreEqual(1, changes, "no event when nothing changed");
                CompanionPreferences.CarameloEnabled = true;
                Assert.AreEqual(2, changes);
            }
            finally
            {
                CompanionPreferences.Changed -= onChanged;
            }
        }

        [Test]
        public void ShouldAccompany_NeedsTheOption_AndThePrologueInRealEncounters()
        {
            Assert.IsTrue(CompanionPreferences.ShouldAccompany(true, true, true));
            Assert.IsFalse(CompanionPreferences.ShouldAccompany(true, true, false), "joins only after the prologue");
            Assert.IsTrue(CompanionPreferences.ShouldAccompany(true, false, false), "sandbox: always there");
            Assert.IsFalse(CompanionPreferences.ShouldAccompany(false, false, true), "switched off");
            Assert.IsFalse(CompanionPreferences.ShouldAccompany(false, true, true));

            Assert.IsTrue(CompanionPreferences.ShouldOfferCuddle(true, true));
            Assert.IsFalse(CompanionPreferences.ShouldOfferCuddle(false, true));
            Assert.IsFalse(CompanionPreferences.ShouldOfferCuddle(true, false));
        }

        [Test]
        public void CuddleButton_ShowsOnlyInTheVillage_WhenIdleAndOffered()
        {
            BiomeType village = Rapadura.Gameplay.Dialogue.VillageNpcCatalog.VillageRegion;
            BiomeType elsewhere = village == BiomeType.Caves ? BiomeType.Desert : BiomeType.Caves;
            Assert.IsTrue(CarameloCuddleButton.ShouldShow(true, true, false, false, village, true));
            Assert.IsFalse(CarameloCuddleButton.ShouldShow(true, true, false, false, elsewhere, true));
            Assert.IsFalse(CarameloCuddleButton.ShouldShow(true, true, false, false, village, false), "option off / before the prologue");
            Assert.IsFalse(CarameloCuddleButton.ShouldShow(true, true, true, false, village, true), "paused");
            Assert.IsFalse(CarameloCuddleButton.ShouldShow(true, true, false, true, village, true), "in a conversation");
            Assert.IsFalse(CarameloCuddleButton.ShouldShow(true, false, false, false, village, true), "token travelling");
            Assert.IsFalse(CarameloCuddleButton.ShouldShow(false, true, false, false, village, true), "HUD hidden");
            Assert.IsFalse(CarameloCuddleButton.ShouldShow(true, true, false, false, null, true));
        }

        // ------------------------------------------------------------------
        // Cuddle
        // ------------------------------------------------------------------

        [Test]
        public void Cuddle_IntroFirst_ThenTheCousinsLinesInTurn_AndArmsTheBuffOnce()
        {
            Assert.AreEqual(CarameloTexts.Intro, CarameloCuddle.Cuddle(CharacterId.Lavine, false), "Vó Edna introduces him first");
            Assert.IsTrue(CarameloCuddle.BuffPending);

            string first = CarameloCuddle.Cuddle(CharacterId.Lavine, true);
            string second = CarameloCuddle.Cuddle(CharacterId.Lavine, true);
            string third = CarameloCuddle.Cuddle(CharacterId.Lavine, true);
            Assert.AreEqual(CarameloTexts.CuddleLineKey(CharacterId.Lavine, 0), first);
            Assert.AreNotEqual(first, second);
            Assert.AreEqual(first, third, "wraps around");
            Assert.AreEqual(CarameloTexts.Intro, CarameloCuddle.Cuddle(null, true), "no hero → Vó Edna speaks");

            Assert.IsTrue(CarameloCuddle.TryConsumeBuff());
            Assert.IsFalse(CarameloCuddle.TryConsumeBuff(), "cuddling twice never stacks");
            Assert.IsFalse(CarameloCuddle.BuffPending);

            StatModifierDefinition buff = CarameloCuddle.Buff();
            Assert.AreEqual(StatType.MoveSpeed, buff.affectedStat);
            Assert.AreEqual(ModifierApplication.PercentAdditive, buff.application);
            Assert.AreEqual(0.05f, buff.value, 1e-5f, "tiny");
            Assert.Greater(buff.durationSeconds, 0f, "temporary");
            Assert.IsFalse(buff.isDebuff);
        }

        [Test]
        public void EveryCousin_HasTheirOwnCuddleLines_InBothLanguages()
        {
            var keys = new HashSet<string>(CarameloTexts.Entries.Select(e => e.Key));
            foreach (CharacterId hero in Enum.GetValues(typeof(CharacterId)))
            {
                for (int i = 0; i < CarameloTexts.LinesPerHero; i++)
                {
                    string key = CarameloTexts.CuddleLineKey(hero, i);
                    Assert.IsTrue(keys.Contains(key), key);
                    string pt = CarameloTexts.GetFallback(key);
                    StringAssert.StartsWith(CharacterRegistry.Get(hero).DisplayName, pt, $"{key} is spoken by {hero}");
                }
            }

            foreach (var entry in CarameloTexts.Entries)
            {
                Assert.IsFalse(string.IsNullOrEmpty(entry.En), entry.Key + " en");
                Assert.IsFalse(string.IsNullOrEmpty(entry.PtBR), entry.Key + " pt-BR");
            }

            Assert.AreEqual(CarameloTexts.CuddleLineKey(CharacterId.Maria, 0), CarameloTexts.CuddleLineKey(CharacterId.Maria, -2), "negative index wraps");
        }

        // ------------------------------------------------------------------
        // Model
        // ------------------------------------------------------------------

        [Test]
        public void Model_HasEveryAnimatedPart_WithinBudget_AndIsDeterministic()
        {
            ProceduralEnemyBlueprint a = CarameloModel.CreateBlueprint();
            ProceduralEnemyBlueprint b = CarameloModel.CreateBlueprint();
            try
            {
                foreach (string part in new[]
                         {
                             CarameloParts.Body, CarameloParts.Head, CarameloParts.EarLeft, CarameloParts.EarRight, CarameloParts.Tail,
                             CarameloParts.TailTip, CarameloParts.Tongue, CarameloParts.PawFrontLeft, CarameloParts.PawFrontRight,
                             CarameloParts.PawBackLeft, CarameloParts.PawBackRight, CarameloParts.CarryGold, CarameloParts.CarryBundle,
                         })
                {
                    Assert.GreaterOrEqual(a.IndexOf(part), 0, part);
                }

                Assert.Greater(a.TriangleCount, 200);
                Assert.LessOrEqual(a.TriangleCount, CarameloModel.TriangleBudget);
                Assert.AreEqual(a.TriangleCount, b.TriangleCount);
                Assert.AreEqual(a.Meshes.Count, b.Meshes.Count);
                Assert.IsTrue(a.Meshes.All(m => m.name.StartsWith(CarameloModel.MeshNamePrefix)), "own mesh names");
                Assert.AreSame(a.Parts[a.IndexOf(CarameloParts.PawFrontLeft)].Mesh, a.Parts[a.IndexOf(CarameloParts.PawFrontRight)].Mesh, "legs share meshes");

                // Caramel coloured (the palette's caramel cell dominates the body).
                Mesh body = a.Parts[a.IndexOf(CarameloParts.Body)].Mesh;
                var uvs = new List<Vector2>();
                body.GetUVs(0, uvs);
                int caramel = uvs.Count(uv => EnemyModelPalette.SwatchAt(uv) == EnemySwatch.SandstoneDark);
                Assert.Greater(caramel, uvs.Count / 3, "mostly caramel");

                // Pivot at the feet: the paws reach the ground, nothing pokes below it.
                GameObject instance = CarameloModel.Instantiate(a, null, null);
                _owned.Add(instance);
                float minY = float.MaxValue;
                foreach (MeshFilter filter in instance.GetComponentsInChildren<MeshFilter>())
                {
                    foreach (Vector3 v in filter.sharedMesh.vertices)
                    {
                        minY = Mathf.Min(minY, filter.transform.TransformPoint(v).y);
                    }
                }

                Assert.That(minY, Is.InRange(-0.02f, 0.03f));
                Assert.AreEqual(CarameloModel.VisualName, instance.name);
            }
            finally
            {
                a.DestroyMeshes();
                b.DestroyMeshes();
            }
        }

        // ------------------------------------------------------------------
        // Audio
        // ------------------------------------------------------------------

        [Test]
        public void PetCues_ExistInDefaultAndShippedMap_AndTheBarkIsShortAndDeterministic()
        {
            string[] cues = { CarameloCompanion.BarkCue, CarameloCompanion.YipCue, CarameloCompanion.SniffCue, CarameloCompanion.FetchCue };
            SfxEventMap defaults = SfxEventMap.CreateDefault();
            _owned.Add(defaults);
            var shipped = Resources.Load<SfxEventMap>(SfxEventMap.ResourcePath);
            Assert.IsNotNull(shipped, "Resources/Audio/SfxEventMap.asset");

            foreach (string id in cues)
            {
                Assert.IsTrue(defaults.TryGetCue(id, out SfxEventMap.Cue a), "default " + id);
                Assert.IsTrue(shipped.TryGetCue(id, out SfxEventMap.Cue b), "shipped " + id);
                Assert.AreEqual(a.preset, b.preset, id);
                Assert.AreEqual(a.volume, b.volume, 1e-4f, id);
                Assert.AreEqual(a.cooldown, b.cooldown, 1e-4f, id);
                Assert.AreEqual(a.frequency, b.frequency, 1e-2f, id);
                Assert.AreNotEqual(ProceduralSfxPreset.None, a.preset, id + " has a placeholder");
                Assert.LessOrEqual(a.volume, 0.75f, id + " mixed under the existing cues");
                Assert.Greater(a.cooldown, 0f, id + " has a cooldown");
            }

            float[] woof = ProceduralSfxSynth.Generate(ProceduralSfxPreset.Bark, 420f);
            CollectionAssert.AreEqual(woof, ProceduralSfxSynth.Generate(ProceduralSfxPreset.Bark, 420f));
            Assert.That(woof.Length / (float)ProceduralSfxSynth.SampleRate, Is.InRange(0.1f, 0.3f));
            Assert.IsTrue(woof.All(v => !float.IsNaN(v) && Mathf.Abs(v) <= 1f));
            Assert.Greater(woof.Max(v => Mathf.Abs(v)), 0.1f, "audible");
            Assert.IsFalse(ProceduralSfxSynth.IsLoopPreset(ProceduralSfxPreset.Bark));
        }
    }
}
