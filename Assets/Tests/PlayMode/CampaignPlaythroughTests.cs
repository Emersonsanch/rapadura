using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Managers;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Flow;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.Story;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using Rapadura.Save;
using Rapadura.UI.Menus;
using Rapadura.UI.Story;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// Plays the whole campaign on the real scenes, start to credits, to prove the game can be finished:
    /// title → "Novo jogo" → hero → prologue → for each region in map order (Floresta → Deserto →
    /// Montanha → Cavernas) the required N normal encounters ("Explorar", <see cref="WorldMapController.TryExplore"/>)
    /// and the boss challenge (<see cref="WorldMapController.TryChallengeBoss"/>), each won by killing every
    /// spawned enemy through <see cref="Health"/> in Main.unity → region cleared, next region unlocked,
    /// interlude shown → after Cavernas the ending, the credits, back on the post-game map with
    /// "A Luz Volta para Casa" unlocked. Halfway (Deserto cleared, its interlude still pending) the
    /// session is saved to slot 0, the app is "closed" (every scene and the GameManager destroyed) and
    /// the save is loaded from the title screen's "Carregar" — the interlude must still play after the load.
    ///
    /// Determinism: <see cref="WorldMapController.SeedOverride"/> fixes the encounter and boss-minion
    /// rolls, random step encounters are switched off while the token walks, the hero is made
    /// invulnerable in each arena (enemies are held by the "Prepare-se!" grace anyway), and story beats
    /// only start when the test turns <see cref="StoryPanelController.AutoPresent"/> on for them.
    /// Saves go to the base class's temp folder. Runs in ~1-2 minutes (16 encounters, 32 scene loads,
    /// scaled time sped up while waiting on arena delays and token travel).
    ///
    /// One hero (<see cref="Hero"/>) instead of all five: the hero only changes the prologue's focus panel
    /// and the combat kit, and victories are resolved through Health, so five runs would cost ~5x the
    /// time for no extra coverage of the campaign flow (hero kits have their own tests). Maithe is the
    /// one hero the other smoke tests never pick (and the only one on procedural animation).
    /// </summary>
    [Category("Campaign")]
    public class CampaignPlaythroughTests : WorldMapFlowTestBase
    {
        private const CharacterId Hero = CharacterId.Maithe;
        private const int Seed = 20261029;
        private const int SaveSlot = 0;
        private const string CampaignAchievementId = "ach_campaign_complete";

        private const float EncounterTimeoutSeconds = 45f;
        private const float BeatStartTimeoutSeconds = 10f;

        /// <summary>Real seconds between two "Continuar" presses (StoryPanelController's double-tap guard is 0.25 s).</summary>
        private const float AdvanceIntervalSeconds = 0.3f;

        private static readonly BiomeType[] RegionsInOrder = { BiomeType.Forest, BiomeType.Desert, BiomeType.Mountain, BiomeType.Caves };

        private readonly List<GameObject> _spawned = new List<GameObject>();
        private readonly List<StoryPanelChangedEvent> _panelChanges = new List<StoryPanelChangedEvent>();
        private Action<EnemySpawnedEvent> _onSpawned;
        private Action<StoryPanelChangedEvent> _onPanelChanged;

        [SetUp]
        public void CampaignSetUp()
        {
            WorldMapController.SeedOverride = Seed;
            EncounterArenaController.ReinforcementWavesEnabled = false; // one wave per fight: kill all → Victory at once
            _spawned.Clear();
            _panelChanges.Clear();
            _onSpawned = e => _spawned.Add(e.Enemy);
            _onPanelChanged = e => _panelChanges.Add(e);
            EventBus.Subscribe(_onSpawned);
            EventBus.Subscribe(_onPanelChanged);
        }

        [TearDown]
        public void CampaignTearDown()
        {
            WorldMapController.SeedOverride = null;
            EncounterArenaController.ReinforcementWavesEnabled = true;
            EventBus.Unsubscribe(_onSpawned);
            EventBus.Unsubscribe(_onPanelChanged);
            _spawned.Clear();
            _panelChanges.Clear();
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Campaign_NewGameToCredits_AllRegionsBossesBeatsSaveLoadAndAchievement()
        {
            // --- 1. Title → Novo jogo → hero ---------------------------------------------------------
            yield return LoadScene(WorldMapSceneName);

            GameFlowService flow = GetService<GameFlowService>();
            Assert.AreEqual(GameFlowState.MainMenu, flow.State, "WorldMap.unity did not boot to the title screen.");
            Assert.IsFalse(GetService<AchievementManager>().IsUnlocked(CampaignAchievementId),
                "The campaign achievement is already unlocked on a fresh profile (test save folder not isolated?).");

            MainMenuController menu = FindInScene<MainMenuController>();
            Assert.IsNotNull(menu, "WorldMap.unity has no MainMenuController (title screen).");
            Assert.IsTrue(menu.gameObject.activeInHierarchy, "The title screen is not shown on boot.");
            Assert.IsNotNull(menu.OnNewGameRequested, "The title screen's \"Novo jogo\" event is not wired.");
            menu.OnNewGameRequested.Invoke(); // the "Novo jogo" button
            yield return WaitFrames(2);
            Assert.AreEqual(GameFlowState.CharacterSelect, flow.State, "\"Novo jogo\" did not open the hero picker.");

            yield return SelectHero(Hero, requireCharacterSelect: false);
            Assert.AreEqual(GameFlowState.World, flow.State, "Choosing a hero did not hand control to the world map.");
            Assert.AreEqual(Hero, GetService<CharacterSelectionState>().Selected);

            WorldMapController map = null;
            yield return WaitForMap(m => map = m);
            Assert.AreEqual(BiomeType.Forest, map.Progress.CurrentRegion, "A new game must start in the Floresta.");
            CollectionAssert.AreEquivalent(new[] { BiomeType.Forest }, map.Progress.UnlockedRegions.ToArray(),
                "A new game must start with only the Floresta unlocked.");

            // --- 2. Prologue ---------------------------------------------------------------------------
            StoryProgress story = GetService<StoryProgress>();
            Assert.AreEqual(StoryBeat.Prologue, story.GetPendingBeat(), "A new game has no pending prologue.");
            yield return PlayBeat(StoryBeat.Prologue, readToEnd: true);

            // --- 3. Regions ----------------------------------------------------------------------------
            for (int i = 0; i < RegionsInOrder.Length; i++)
            {
                BiomeType region = RegionsInOrder[i];
                BiomeType? next = i + 1 < RegionsInOrder.Length ? RegionsInOrder[i + 1] : (BiomeType?)null;

                if (i > 0)
                {
                    yield return TravelTo(region);
                }

                yield return WaitForMap(m => map = m);
                WorldMapProgress progress = map.Progress;
                RegionBossProgress bosses = map.RegionBosses;
                Assert.AreEqual(region, progress.CurrentRegion);
                Assert.IsFalse(bosses.IsRegionCleared(region), $"{region} is already cleared before its boss was fought.");
                if (next.HasValue)
                {
                    Assert.IsFalse(progress.IsUnlocked(next.Value), $"{next} is unlocked before the {region} boss fell.");
                }

                EnemyDefinition boss = bosses.BossFor(region);
                Assert.IsNotNull(boss, $"{region} has no region boss in its enemy table — the campaign cannot be finished.");
                int required = bosses.VictoriesToChallenge(region);
                Assert.Greater(required, 0);

                // N normal encounters through "Explorar".
                for (int v = progress.GetVictories(region); v < required; v++)
                {
                    Assert.IsFalse(map.CanChallengeCurrentBoss, $"\"Enfrentar o chefe\" offered in {region} after only {v}/{required} victories.");
                    int before = progress.GetVictories(region);
                    WorldMapController current = map;
                    yield return RunEncounter(() => current.TryExplore(), region, boss, bossChallenge: false);
                    yield return WaitForMap(m => map = m);
                    Assert.AreEqual(before + 1, map.Progress.GetVictories(region), $"Victory {v + 1} in {region} was not recorded.");
                }

                // Boss challenge.
                Assert.IsTrue(map.CanChallengeCurrentBoss, $"\"Enfrentar o chefe\" not offered in {region} after {required} victories.");
                {
                    WorldMapController current = map;
                    yield return RunEncounter(() => current.TryChallengeBoss(), region, boss, bossChallenge: true);
                }

                yield return WaitForMap(m => map = m);
                progress = map.Progress;
                Assert.IsTrue(progress.IsRegionCleared(region), $"Winning the {region} boss challenge did not clear the region.");
                Assert.IsTrue(map.RegionBosses.IsRegionCleared(region));
                Assert.IsFalse(map.CanChallengeCurrentBoss, $"\"Enfrentar o chefe\" still offered in the cleared {region}.");
                if (next.HasValue)
                {
                    Assert.IsTrue(progress.IsUnlocked(next.Value), $"Clearing {region} did not unlock {next}.");
                }

                StoryBeat beat = StoryBeats.AfterRegionCleared(region);
                Assert.AreEqual(beat, story.GetPendingBeat(), $"Clearing {region} did not leave its beat ({beat}) pending.");

                // --- 5. Save / quit / load in the middle ------------------------------------------------
                if (region == BiomeType.Desert)
                {
                    yield return SaveQuitAndLoad(region);
                    yield return WaitForMap(m => map = m);
                    flow = GetService<GameFlowService>();
                    story = GetService<StoryProgress>();
                    Assert.AreEqual(beat, story.GetPendingBeat(), $"After loading, the {region} interlude is no longer pending — it would never play.");
                }

                if (region == StoryBeats.FinalRegion)
                {
                    Assert.IsTrue(story.CampaignComplete, "Clearing the Cavernas did not complete the campaign.");
                    break;
                }

                // Interludes: skipped ("Pular" still counts as seen).
                yield return PlayBeat(beat, readToEnd: false);
            }

            // --- 4. Ending → credits → post-game map ---------------------------------------------------
            Assert.AreEqual(StoryBeat.Ending, story.GetPendingBeat());
            CreditsController credits = FindInScene<CreditsController>();
            Assert.IsNotNull(credits, "WorldMap.unity has no CreditsController — the ending cannot roll the credits.");

            yield return PlayBeat(StoryBeat.Ending, readToEnd: true, expectCredits: true);

            yield return WaitUntilOrFail(() => credits.IsOpen, "the credits to open after the ending", 5f);
            Assert.IsTrue(StoryPanelController.IsShowing, "StoryPanelController lost track of the credits it opened.");
            Assert.IsTrue(GameplayPause.IsPaused, "The credits do not pause the map behind them.");

            credits.Hide(); // "Voltar"
            yield return WaitUntilOrFail(() => !HandleMapPopups() && !credits.IsOpen && !StoryPanelController.IsShowing && !GameplayPause.IsPaused,
                "the credits to close and release the map", 5f);
            yield return WaitFrames(2);

            Assert.AreEqual(WorldMapSceneName, SceneManager.GetActiveScene().name, "Closing the credits did not leave the player on the world map.");
            Assert.AreEqual(GameFlowState.World, flow.State);
            yield return WaitForMap(m => map = m);
            Assert.IsTrue(map.CanExplore, "The post-game world map cannot be explored.");
            Assert.AreEqual(StoryBeat.None, story.GetPendingBeat(), "A story beat is still pending after the ending.");
            Assert.IsTrue(story.CampaignComplete);
            foreach (BiomeType region in RegionsInOrder)
            {
                Assert.IsTrue(map.Progress.IsRegionCleared(region), $"{region} is not cleared at the end of the campaign.");
            }

            Assert.IsTrue(GetService<AchievementManager>().IsUnlocked(CampaignAchievementId),
                $"Finishing the campaign did not unlock '{CampaignAchievementId}'.");

            // The ending's auto-save carries the post-game state.
            SaveData autoSave = GetService<SaveManager>().PeekSlot(SaveManager.AutoSaveSlot);
            Assert.IsNotNull(autoSave, "No auto-save after the ending.");
            Assert.IsTrue(SaveManager.TryGetEntryState(autoSave, StoryProgress.Key, out StorySaveState savedStory), "The auto-save has no Story entry.");
            Assert.IsTrue(savedStory.campaignComplete, "The auto-save after the ending is not marked campaignComplete.");
            CollectionAssert.AreEquivalent(StoryBeats.InOrder.Select(StoryBeats.Id).ToArray(), savedStory.seenBeats,
                "The auto-save after the ending does not list every beat as seen.");
        }

        // ------------------------------------------------------------------
        // Steps
        // ------------------------------------------------------------------

        /// <summary>Starts an encounter from the map, wins it in Main.unity by killing every spawned enemy, and waits for the return.</summary>
        private IEnumerator RunEncounter(Func<bool> start, BiomeType region, EnemyDefinition boss, bool bossChallenge)
        {
            SceneTransitionService transition = GetService<SceneTransitionService>();
            string what = bossChallenge ? $"{region} boss challenge" : $"{region} encounter";

            _spawned.Clear();
            Assert.IsTrue(start(), $"The map refused to start the {what}.");
            Assert.IsTrue(transition.HasPendingEncounter, $"The {what} did not reach SceneTransitionService.");
            Assert.AreEqual(region, transition.PendingEncounter.biome);
            Assert.AreEqual(bossChallenge, transition.PendingEncounter.isBossChallenge);

            yield return WaitFast(() => SceneManager.GetActiveScene().name == MainSceneName && SceneManager.GetActiveScene().isLoaded,
                $"Main.unity to load for the {what}", SceneLoadTimeoutSeconds);

            EncounterArenaController arena = null;
            yield return WaitFast(() =>
            {
                arena = UnityEngine.Object.FindFirstObjectByType<EncounterArenaController>();
                return arena != null && arena.IsEncounterActive;
            }, $"EncounterArenaController to start the {what}", EncounterTimeoutSeconds);

            List<EnemyController> enemies = _spawned
                .Where(go => go != null)
                .Select(go => go.GetComponent<EnemyController>())
                .Where(e => e != null && e.Health != null)
                .ToList();
            Assert.Greater(enemies.Count, 0, $"The {what} spawned no enemies.");
            Assert.AreEqual(arena.AliveEnemies, enemies.Count, $"The arena tracks a different number of enemies than were spawned for the {what}.");
            if (bossChallenge)
            {
                Assert.IsTrue(enemies.Any(e => e.Definition == boss), $"The {what} did not spawn {boss.name}.");
            }
            else
            {
                Assert.IsFalse(enemies.Any(e => e.Definition == boss),
                    $"A normal {region} encounter rolled the region boss.");
            }

            // No combat randomness: the hero cannot be hurt while the enemies are cleared.
            GameObject player = FindPlayer();
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.GrantInvulnerability(600f);
            }

            foreach (EnemyController enemy in enemies)
            {
                enemy.Health.ClearInvulnerability();
                enemy.Health.ApplyDamage(1_000_000f, ElementType.Physical, player);
                Assert.IsTrue(enemy.Health.IsDead, $"'{enemy.name}' survived 1,000,000 damage in the {what}.");
            }

            Assert.IsFalse(arena.IsEncounterActive, $"Killing every spawned enemy did not resolve the {what}.");

            yield return WaitFast(() => SceneManager.GetActiveScene().name == WorldMapSceneName && SceneManager.GetActiveScene().isLoaded,
                $"the return to the world map after the {what}", EncounterTimeoutSeconds);
            RestoreTimeScale();

            Assert.IsFalse(transition.HasPendingEncounter, $"The {what} is still pending after returning to the map.");
            Assert.AreEqual(EncounterOutcome.Victory, transition.LastOutcome, $"The {what} was not resolved as a Victory.");
        }

        /// <summary>
        /// Lets <see cref="StoryPanelController"/> present <paramref name="beat"/> on its own (AutoPresent on),
        /// then reads it panel by panel ("Continuar") or skips it ("Pular"), and checks it was marked seen.
        /// </summary>
        private IEnumerator PlayBeat(StoryBeat beat, bool readToEnd, bool expectCredits = false)
        {
            StoryProgress story = GetService<StoryProgress>();
            Assert.AreEqual(beat, story.GetPendingBeat(), $"{beat} is not the pending beat.");
            Assert.IsFalse(story.HasSeen(beat));

            _panelChanges.Clear();
            StoryPanelController.AutoPresent = true;
            try
            {
                // A beat never starts over another modal: the daily reward popup opening first would hold it off.
                yield return WaitUntilOrFail(() =>
                    {
                        if (StoryPanelController.ShowingBeat == beat)
                        {
                            return true;
                        }

                        HandleMapPopups();
                        return false;
                    },
                    $"StoryPanelController to present {beat} on the world map", BeatStartTimeoutSeconds);
            }
            finally
            {
                // Only this beat: the next one must wait for the test (e.g. the save before the Deserto interlude).
                StoryPanelController.AutoPresent = false;
            }

            int count = StoryPanelController.ShowingPanelCount;
            Assert.Greater(count, 0, $"{beat} has no panels.");
            Assert.IsTrue(GameplayPause.IsPaused, $"{beat} does not pause the map behind it.");
            Assert.IsFalse(UnityEngine.Object.FindFirstObjectByType<WorldMapController>().TryExplore(), $"\"Explorar\" started an encounter behind {beat}.");

            if (readToEnd)
            {
                for (int presses = 0; StoryPanelController.ShowingBeat == beat; presses++)
                {
                    Assert.LessOrEqual(presses, count, $"{beat} did not end after {count} \"Continuar\" presses.");
                    yield return new WaitForSecondsRealtime(AdvanceIntervalSeconds);
                    Assert.IsTrue(StoryPanelController.AdvanceShowing());
                    yield return null;
                }

                Assert.AreEqual(count - 1, _panelChanges.Count(c => c.Beat == beat && c.Change == StoryPanelChange.Advanced),
                    $"Reading {beat} to the end did not page through all {count} panels.");
                Assert.IsTrue(_panelChanges.Any(c => c.Beat == beat && c.Change == StoryPanelChange.Finished), $"{beat} did not finish.");
            }
            else
            {
                Assert.IsTrue(StoryPanelController.SkipShowing());
                yield return null;
                Assert.IsTrue(_panelChanges.Any(c => c.Beat == beat && c.Change == StoryPanelChange.Skipped), $"{beat} was not skipped.");
            }

            Assert.AreEqual(StoryBeat.None, StoryPanelController.ShowingBeat);
            Assert.IsTrue(story.HasSeen(beat), $"{beat} was not marked seen after it ended.");
            Assert.AreNotEqual(beat, story.GetPendingBeat(), $"{beat} is still pending after it was seen.");

            if (!expectCredits)
            {
                yield return WaitUntilOrFail(() => !HandleMapPopups() && !GameplayPause.IsPaused && !StoryPanelController.IsShowing,
                    $"the map to be released after {beat}", 5f);
            }
        }

        /// <summary>
        /// Manual save to <see cref="SaveSlot"/>, then a cold restart (every scene and the GameManager
        /// destroyed, like quitting the app), then the title screen's "Carregar" on that slot.
        /// </summary>
        private IEnumerator SaveQuitAndLoad(BiomeType clearedRegion)
        {
            SaveManager saveManager = GetService<SaveManager>();
            saveManager.Save(SaveSlot);
            Assert.IsTrue(File.Exists(SaveManager.GetSlotFilePath(SaveSlot)), "Manual save did not write the slot file.");
            StringAssert.StartsWith(Path.GetFullPath(SaveFolderPath), Path.GetFullPath(SaveManager.GetSlotFilePath(SaveSlot)),
                "The save went outside the test's temp save folder.");

            List<BiomeType> unlockedBefore = GetService<WorldMapProgress>().UnlockedRegions.OrderBy(r => r).ToList();
            List<StoryBeat> seenBefore = GetService<StoryProgress>().SeenBeats.OrderBy(b => b).ToList();

            yield return ColdRestart();

            GameFlowService flow = GetService<GameFlowService>();
            Assert.AreEqual(GameFlowState.MainMenu, flow.State, "The restarted session did not boot to the title screen.");
            Assert.IsFalse(GetService<CharacterSelectionState>().HasSelection, "A hero survived the restart before loading.");

            MainMenuController menu = FindInScene<MainMenuController>();
            Assert.IsNotNull(menu);
            Assert.IsNotNull(menu.LoadSlotHandler, "The title screen's \"Carregar\" is not wired to the game flow.");
            Assert.IsTrue(menu.LoadSlotHandler(SaveSlot), $"\"Carregar\" slot {SaveSlot} failed.");
            yield return WaitFrames(3);
            Assert.AreEqual(GameFlowState.World, flow.State, "Loading a save with a hero did not go to the world map.");

            WorldMapController map = null;
            yield return WaitForMap(m => map = m);

            Assert.AreEqual(Hero, GetService<CharacterSelectionState>().Selected, "The loaded save lost the hero.");
            Assert.AreEqual(clearedRegion, map.Progress.CurrentRegion, "The loaded save put the token in another region.");
            Assert.IsTrue(map.Progress.IsRegionCleared(clearedRegion), $"The loaded save lost the {clearedRegion} clear.");
            CollectionAssert.AreEqual(unlockedBefore, map.Progress.UnlockedRegions.OrderBy(r => r).ToList(), "The loaded save lost unlocked regions.");
            CollectionAssert.AreEqual(seenBefore, GetService<StoryProgress>().SeenBeats.OrderBy(b => b).ToList(), "The loaded save lost seen story beats.");
            Assert.IsFalse(GetService<StoryProgress>().CampaignComplete);
        }
    }
}
