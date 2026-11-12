using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Environment;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Night;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Gameplay.World.Cities.Town;
using Rapadura.Gameplay.World.Map;
using Rapadura.Gameplay.World.Map.Areas;
using Rapadura.Gameplay.World.Map.Overworld;
using Rapadura.Save;
using Rapadura.UI.Menus;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// Plays "A Lua que Não Enche" (<see cref="MidStoryCatalog"/>, Docs/HistoriaMeio_20a100.md) start to end on the real scenes, accelerated but
    /// with no shortcut on the quest itself: a new game, "Mover" to the Pomar (Main.unity in map mode with the stitched world), then for each of
    /// the 15 steps:
    /// <list type="number">
    /// <item>the hero is raised to the step's level through <see cref="PlayerStats.AddExperience"/> (only when below it; below it the giver is
    /// asked first and must NOT start the step);</item>
    /// <item>stood (the game's teleport: WarmUp + ResetTracking + <see cref="CityModeController.PlaceAt"/>) at the giver's door in their town of
    /// the overworld; the <see cref="CityResidentPost"/> must see the hero in range, <see cref="LateStoryCatalog.Decide"/> must say Offer, and
    /// <see cref="CityResidentPost.Interact"/> (the walk-in's own call) must start the step;</item>
    /// <item>stood in the step's zone, by a spawn clearing of that zone whose table holds the step's beast: the zone's own
    /// <see cref="AreaChangedEvent"/> → <see cref="LateStoryLocationReporter"/> must tick the place objective; then the zone's own
    /// <see cref="MapMonsterSpawner"/> spawns (<see cref="MapMonsterSpawner.TrySpawnOne"/>, the real roll) until N of that beast came, each one
    /// killed through <see cref="Health.ApplyDamage(float, ElementType, GameObject)"/> (other beasts rolled on the way are killed too, a night
    /// variant of the target is put away uncounted). The first step's seal (Tages) is not a monster of the open world: it is instantiated
    /// in his hall from the arena's enemy prefab like the spawners do and killed the same way;</item>
    /// <item>the last kill completes the step: XP grown by exactly the step's reward, each reward item arrived (bag or the Baú's inbox), no
    /// reward left pending, and the next step's giver now offers it (or says "too low" under its level).</item>
    /// </list>
    /// Halfway (step 8, the Delta's rain, place done and one kill of six) the game is saved to slot 0, the app "closed" and the slot loaded
    /// from the title's "Carregar": the saved quest state is read back from the file (schema, no duplicate ids, nothing pending) and after the
    /// load the completed steps, the running step's progress, the hero's XP and the reward items must be exactly as before — nothing lost,
    /// nothing duplicated, and <see cref="MidStorySaveMigration"/> must not mark any later step done (no letter was touched). At the end Soslan
    /// must say "too low" under level 100 and offer the first of "As Cartas da Estrada" at 100.
    ///
    /// Kills pass no attacker (source null): the "Perigoso" miss roll (<c>LevelDangerCombat.HeroMisses</c>) never fires, and no kill XP mixes into the
    /// step's XP reward, which is then checked to the point. The hero is invulnerable all along. Errors and exceptions fail the test (the test
    /// runner's default); the game's informative logs are allowed.
    /// </summary>
    [Category("Campaign")]
    public class MidStoryPlaythroughTests : WorldMapFlowTestBase
    {
        private const string LogTag = "[MidStoryPlaythrough]";
        private const CharacterId Hero = CharacterId.Joaquim;
        private const int SaveSlot = 0;

        /// <summary>The step saved and loaded in the middle (DeltaRain: 6 kills, so one kill done leaves work on both sides of the load).</summary>
        private const int SaveStepIndex = 7;

        private const float SettleSeconds = 0.6f;
        private const float InRangeTimeoutSeconds = 3f;
        private const float ReachTimeoutSeconds = 4f;
        private const float SpawnerTimeoutSeconds = 8f;
        private const int MaxSpawnAttemptsPerTarget = 150;
        private const float KillDamage = 1_000_000_000f;

        private bool _monsterSpawningBefore;

        [UnityTest]
        [Timeout(300000)]
        public IEnumerator MidStory_FifteenSteps_OfferedDoneRewarded_SurvivesSaveLoad_ThenSoslanOffersTheFirstLetter()
        {
            MapExitTrigger.Enabled = false;
            CityExitTrigger.Enabled = false;
            CityResidentPost.AutoInteract = false;
            _monsterSpawningBefore = MapMonsterSpawner.SpawningEnabled;
            MapMonsterSpawner.SpawningEnabled = true;
            try
            {
                yield return Run();
            }
            finally
            {
                MapExitTrigger.Enabled = true;
                CityExitTrigger.Enabled = true;
                CityResidentPost.AutoInteract = true;
                MapMonsterSpawner.SpawningEnabled = _monsterSpawningBefore;
                RestoreTimeScale();
            }
        }

        private IEnumerator Run()
        {
            float started = Time.realtimeSinceStartup;
            yield return NewGameIntoTheWorld();

            QuestManager quests = GetService<QuestManager>();
            foreach (LateStoryStep step in LateStoryCatalog.WholeStory)
            {
                Assert.IsFalse(quests.IsActive(step.QuestId) || quests.IsCompleted(step.QuestId), $"A new game already has '{step.QuestId}' running or done.");
            }

            Assert.AreEqual(15, MidStoryCatalog.Steps.Count, "The mid story should have 15 steps.");
            for (int i = 0; i < MidStoryCatalog.Steps.Count; i++)
            {
                yield return PlayStep(i);
                Debug.Log($"{LogTag} step {i + 1}/{MidStoryCatalog.Steps.Count} done at {Time.realtimeSinceStartup - started:0.0}s");
            }

            yield return FirstLetter();
            Debug.Log($"{LogTag} whole run in {Time.realtimeSinceStartup - started:0.0}s");
        }

        // ------------------------------------------------------------------
        // One step
        // ------------------------------------------------------------------

        private IEnumerator PlayStep(int index)
        {
            LateStoryStep step = MidStoryCatalog.Steps[index];
            string what = $"Step {index + 1} '{step.QuestId}' ({step.GiverResidentId} in {step.CityId}, level {step.MinLevel})";
            Debug.Log($"{LogTag} {what}");

            QuestManager quests = GetService<QuestManager>();
            QuestDefinition definition = LateStoryCatalog.GetQuest(step.QuestId);
            Assert.IsNotNull(definition, $"{what}: GetQuest built nothing.");
            string prerequisite = step.Quest.PrerequisiteQuestId;
            if (!string.IsNullOrEmpty(prerequisite))
            {
                Assert.IsTrue(quests.IsCompleted(prerequisite), $"{what}: its prerequisite '{prerequisite}' is not done.");
            }

            // 1-2. To the giver; under the level they must not hand the step out; at the level they offer it and Interact starts it.
            CityResidentPost post = null;
            yield return GoToGiver(step, p => post = p, what);
            PlayerStats stats = FindPlayer().GetComponent<PlayerStats>();
            if (stats.Level < step.MinLevel)
            {
                LateStoryTalk under = LateStoryCatalog.Decide(step.GiverResidentId, quests.IsActive, quests.IsCompleted, stats.Level, out _);
                Assert.AreEqual(string.IsNullOrEmpty(prerequisite) ? LateStoryTalk.None : LateStoryTalk.TooLow, under,
                    $"{what}: the giver's answer at level {stats.Level}, under the step's level.");
                if (!CityQuestCatalog.QuestsOf(step.GiverResidentId).Any())
                {
                    // A giver with city quests would hand one of those out here; the others only say a line.
                    post.Interact();
                    Assert.IsFalse(quests.IsActive(step.QuestId), $"{what}: the giver handed the step out at level {stats.Level}.");
                }

                RaiseLevel(stats, step.MinLevel);
            }

            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(step.GiverResidentId, quests.IsActive, quests.IsCompleted, stats.Level, out LateStoryStep offered),
                $"{what}: the giver does not offer the step at level {stats.Level}.");
            Assert.AreEqual(step.QuestId, offered.QuestId, $"{what}: the giver offers another step.");
            post.Interact();
            Assert.IsTrue(quests.IsActive(step.QuestId), $"{what}: talking to {step.GiverResidentId} (CityResidentPost.Interact) did not start the step.");
            QuestProgress progress = quests.GetProgress(step.QuestId);
            Assert.IsNotNull(progress);
            Assert.IsFalse(progress.Objectives.Any(o => o.IsComplete), $"{what}: an objective is already done when the step starts.");

            // 3. The place: stood in the zone, by the clearing of the beast (any clearing of the zone for the seal step).
            bool seal = string.IsNullOrEmpty(prerequisite);
            QuestObjective kill = progress.Objectives.Single(o => o.Type == QuestObjectiveType.KillEnemyType);
            Assert.AreEqual(seal ? MidStoryCatalog.SealObjectiveId : LateStoryCatalog.KillObjectiveId, kill.ObjectiveId);
            OverworldLayout world = MapModeController.Current.Overworld;
            OverworldZone zone = world.Zone(step.ZoneId);
            Assert.IsNotNull(zone, $"{what}: the stitched world has no zone '{step.ZoneId}'.");
            MapSpawnZone clearing = ClearingFor(world, zone, seal ? null : kill.TargetId, what);
            yield return GoToZone(world, zone, clearing, what);
            yield return WaitUntilOrFail(() => quests.GetProgress(step.QuestId)?.Objectives.Single(o => o.ObjectiveId == LateStoryCatalog.ReachObjectiveId).IsComplete == true,
                $"{what}: the place objective '{step.ZoneId}' to tick with the hero stood in it (LateStoryLocationReporter)", ReachTimeoutSeconds);
            Assert.IsTrue(quests.IsActive(step.QuestId), $"{what}: the step ended on the place alone.");

            // The kills, all but the last.
            if (seal)
            {
                Assert.AreEqual(1, kill.RequiredAmount);
                Assert.AreEqual(MidStoryCatalog.SealEnemyId, kill.TargetId);
            }
            else
            {
                Assert.Greater(kill.RequiredAmount, 1, $"{what}: a kill objective of one beast (the save in the middle needs more).");
                MapMonsterSpawner spawner = null;
                yield return WaitForSpawner(clearing, s => spawner = s, what);
                for (int done = 0; done < kill.RequiredAmount - 1; done++)
                {
                    if (index == SaveStepIndex && done == 1)
                    {
                        yield return SaveQuitAndLoad(step, what);
                        quests = GetService<QuestManager>();
                        world = MapModeController.Current.Overworld;
                        zone = world.Zone(step.ZoneId);
                        clearing = ClearingFor(world, zone, kill.TargetId, what);
                        yield return GoToZone(world, zone, clearing, what + " after the load");
                        yield return WaitForSpawner(clearing, s => spawner = s, what + " after the load");
                    }

                    EnemyController target = null;
                    yield return SpawnTarget(spawner, kill.TargetId, allowVariant: true, e => target = e, what);
                    int before = Kills(quests, step.QuestId);
                    Kill(target, what);
                    Assert.AreEqual(before + 1, Kills(quests, step.QuestId), $"{what}: killing '{target.name}' did not count.");
                    Assert.IsTrue(quests.IsActive(step.QuestId), $"{what}: the step ended before its last kill.");
                }
            }

            // 4. The last kill completes the step: the rewards are measured around that one blow.
            EnemyController last = null;
            if (seal)
            {
                last = SpawnTages(FindPlayer(), what);
            }
            else
            {
                MapMonsterSpawner spawner = FindSpawner(clearing);
                Assert.IsNotNull(spawner, $"{what}: the clearing's spawner went to sleep.");
                yield return SpawnTarget(spawner, kill.TargetId, allowVariant: false, e => last = e, what);
            }

            stats = FindPlayer().GetComponent<PlayerStats>();
            InventoryManager bag = FindPlayer().GetComponent<InventoryManager>();
            long xpBefore = stats.TotalExperience;
            Dictionary<string, int> itemsBefore = step.Quest.Rewards.ToDictionary(r => r.ItemId, r => Owned(bag, r.ItemId));
            var doneBefore = new HashSet<string>(quests.CompletedQuestIds);

            Kill(last, what);

            // The same blow can finish another quest the run holds (a daily of the hero, by the date and the order the suite ran in):
            // its XP is not the step's, so it is taken off before the step's reward is checked.
            long otherXp = Resources.LoadAll<QuestDefinition>("Quests")
                .Where(q => q != null && q.QuestId != step.QuestId && !doneBefore.Contains(q.QuestId) && quests.IsCompleted(q.QuestId))
                .Sum(q => (long)q.RewardExperience);

            Assert.IsTrue(quests.IsCompleted(step.QuestId), $"{what}: the last kill did not complete the step ({Describe(quests.GetProgress(step.QuestId))}).");
            Assert.IsFalse(quests.IsActive(step.QuestId));
            CollectionAssert.DoesNotContain(quests.PendingRewardQuestIds, step.QuestId, $"{what}: the rewards were queued instead of given (no live hero?).");
            Assert.Greater(definition.RewardExperience, 0, $"{what}: the step pays no XP.");
            Assert.AreEqual(definition.RewardExperience, stats.TotalExperience - xpBefore - otherXp, $"{what}: the XP reward.");
            foreach (RegionRewardSpec reward in step.Quest.Rewards)
            {
                Assert.IsNotNull(ItemDatabase.GetById(reward.ItemId), $"{what}: the reward item '{reward.ItemId}' does not exist (it can never arrive).");
                // At least the reward: the last kill's own loot (granted even without an attacker) may drop the same item.
                Assert.GreaterOrEqual(Owned(bag, reward.ItemId) - itemsBefore[reward.ItemId], reward.Quantity, $"{what}: the reward '{reward.ItemId}' x{reward.Quantity} did not arrive.");
            }

            // The next step is open now: offered at its level, "too low" under it.
            LateStoryStep next = index + 1 < MidStoryCatalog.Steps.Count ? MidStoryCatalog.Steps[index + 1] : LateStoryCatalog.Steps[0];
            LateStoryTalk nextTalk = LateStoryCatalog.Decide(next.GiverResidentId, quests.IsActive, quests.IsCompleted, stats.Level, out LateStoryStep nextStep);
            Assert.AreEqual(stats.Level >= next.MinLevel ? LateStoryTalk.Offer : LateStoryTalk.TooLow, nextTalk,
                $"{what}: after it, {next.GiverResidentId} (level {next.MinLevel}) answers a level-{stats.Level} hero wrong.");
            Assert.AreEqual(next.QuestId, nextStep.QuestId);
            Assert.AreEqual(next, MidStorySaveMigration.NextMainStep(quests.IsCompleted), $"{what}: the story's next step is not '{next.QuestId}'.");
        }

        // ------------------------------------------------------------------
        // The first letter
        // ------------------------------------------------------------------

        private IEnumerator FirstLetter()
        {
            LateStoryStep letter = LateStoryCatalog.Steps[0];
            Assert.AreEqual(LateStoryCatalog.SteppeFoals, letter.QuestId);
            Assert.AreEqual(MidStoryCatalog.FinalQuestId, letter.Quest.PrerequisiteQuestId);
            string what = $"The first letter '{letter.QuestId}' ({letter.GiverResidentId} in {letter.CityId}, level {letter.MinLevel})";
            QuestManager quests = GetService<QuestManager>();
            foreach (LateStoryStep step in MidStoryCatalog.Steps)
            {
                Assert.IsTrue(quests.IsCompleted(step.QuestId), $"{what}: '{step.QuestId}' is not done at the end of the moon.");
            }

            CityResidentPost post = null;
            yield return GoToGiver(letter, p => post = p, what);
            PlayerStats stats = FindPlayer().GetComponent<PlayerStats>();
            if (stats.Level < letter.MinLevel)
            {
                Assert.AreEqual(LateStoryTalk.TooLow, LateStoryCatalog.Decide(letter.GiverResidentId, quests.IsActive, quests.IsCompleted, stats.Level, out _), what);
                if (!CityQuestCatalog.QuestsOf(letter.GiverResidentId).Any())
                {
                    post.Interact();
                    Assert.IsFalse(quests.IsActive(letter.QuestId), $"{what}: Soslan handed the letter out at level {stats.Level}.");
                }

                RaiseLevel(stats, letter.MinLevel);
            }

            Assert.AreEqual(LateStoryTalk.Offer, LateStoryCatalog.Decide(letter.GiverResidentId, quests.IsActive, quests.IsCompleted, stats.Level, out _), what);
            post.Interact();
            Assert.IsTrue(quests.IsActive(letter.QuestId), $"{what}: talking to Soslan did not start the first letter.");
        }

        // ------------------------------------------------------------------
        // Save in the middle
        // ------------------------------------------------------------------

        /// <summary>Manual save to <see cref="SaveSlot"/>, the saved quest state read back, a cold restart, "Carregar", back into the world; nothing lost or doubled.</summary>
        private IEnumerator SaveQuitAndLoad(LateStoryStep running, string what)
        {
            QuestManager quests = GetService<QuestManager>();
            GameObject player = FindPlayer();
            PlayerStats stats = player.GetComponent<PlayerStats>();
            InventoryManager bag = player.GetComponent<InventoryManager>();
            List<string> doneBefore = MidStoryCatalog.Steps.Select(s => s.QuestId).Where(quests.IsCompleted).OrderBy(id => id).ToList();
            List<int> amountsBefore = quests.GetProgress(running.QuestId).Objectives.Select(o => o.CurrentAmount).ToList();
            int completedCountBefore = quests.CompletedQuestIds.Count;
            int levelBefore = stats.Level;
            long xpBefore = stats.TotalExperience;
            List<string> rewardIds = MidStoryCatalog.Steps.SelectMany(s => s.Quest.Rewards).Select(r => r.ItemId).Distinct().ToList();
            Dictionary<string, int> itemsBefore = rewardIds.ToDictionary(id => id, id => Owned(bag, id));

            SaveManager saveManager = GetService<SaveManager>();
            saveManager.Save(SaveSlot);
            Assert.IsTrue(File.Exists(SaveManager.GetSlotFilePath(SaveSlot)), $"{what}: the manual save did not write the slot file.");
            StringAssert.StartsWith(Path.GetFullPath(SaveFolderPath), Path.GetFullPath(SaveManager.GetSlotFilePath(SaveSlot)), "The save went outside the test's temp folder.");

            SaveData saved = saveManager.PeekSlot(SaveSlot);
            Assert.IsNotNull(saved, $"{what}: the slot cannot be read back.");
            Assert.IsTrue(SaveManager.TryGetEntryState(saved, quests.SaveKey, out QuestSaveState state), $"{what}: the save has no '{quests.SaveKey}' entry.");
            Assert.AreEqual(MidStorySaveMigration.Schema, state.StorySchema, $"{what}: the save is not written at the mid story's schema.");
            Assert.AreEqual(state.CompletedQuestIds.Count, state.CompletedQuestIds.Distinct().Count(), $"{what}: the saved completed list holds duplicates.");
            CollectionAssert.AreEqual(doneBefore, state.CompletedQuestIds.Where(id => MidStoryCatalog.FindStep(id) != null).OrderBy(id => id).ToList(),
                $"{what}: the saved completed steps differ from the game's.");
            Assert.AreEqual(1, state.ActiveQuests.Count(p => p.QuestId == running.QuestId), $"{what}: the running step is saved {state.ActiveQuests.Count(p => p.QuestId == running.QuestId)} times.");
            CollectionAssert.AreEqual(amountsBefore, state.ActiveQuests.First(p => p.QuestId == running.QuestId).Objectives.Select(o => o.CurrentAmount).ToList(),
                $"{what}: the running step's progress is saved wrong.");
            Assert.IsFalse(state.PendingRewardQuestIds.Any(id => MidStoryCatalog.FindStep(id) != null), $"{what}: a step's rewards are saved as still owed.");

            yield return ColdRestart();
            MainMenuController menu = FindInScene<MainMenuController>();
            Assert.IsNotNull(menu, "No title screen after the restart.");
            Assert.IsNotNull(menu.LoadSlotHandler, "The title screen's \"Carregar\" is not wired.");
            Assert.IsTrue(menu.LoadSlotHandler(SaveSlot), $"\"Carregar\" slot {SaveSlot} failed.");
            yield return WaitFrames(3);
            yield return BackIntoTheWorld();

            quests = GetService<QuestManager>();
            player = FindPlayer();
            stats = player.GetComponent<PlayerStats>();
            bag = player.GetComponent<InventoryManager>();
            Assert.AreEqual(Hero, GetService<CharacterSelectionState>().Selected, "The load lost the hero.");
            CollectionAssert.AreEqual(doneBefore, MidStoryCatalog.Steps.Select(s => s.QuestId).Where(quests.IsCompleted).OrderBy(id => id).ToList(),
                $"{what}: the completed steps changed across the load.");
            Assert.AreEqual(completedCountBefore, quests.CompletedQuestIds.Count, $"{what}: the completed list grew or shrank across the load.");
            Assert.IsTrue(quests.IsActive(running.QuestId), $"{what}: the running step is gone after the load.");
            CollectionAssert.AreEqual(amountsBefore, quests.GetProgress(running.QuestId).Objectives.Select(o => o.CurrentAmount).ToList(),
                $"{what}: the running step's progress changed across the load.");
            Assert.AreEqual(running, MidStorySaveMigration.NextMainStep(quests.IsCompleted), $"{what}: after the load the story points elsewhere (migration marked steps?).");
            foreach (LateStoryStep later in LateStoryCatalog.WholeStory.SkipWhile(s => s != running).Skip(1))
            {
                Assert.IsFalse(quests.IsCompleted(later.QuestId) || quests.IsActive(later.QuestId), $"{what}: '{later.QuestId}' is running or done after the load.");
            }

            Assert.IsEmpty(quests.PendingRewardQuestIds.Where(id => LateStoryCatalog.FindStep(id) != null).ToList(), $"{what}: rewards owed after the load (they would be paid twice).");
            Assert.AreEqual(levelBefore, stats.Level, $"{what}: the hero's level changed across the load.");
            Assert.AreEqual(xpBefore, stats.TotalExperience, $"{what}: the hero's XP changed across the load.");
            foreach (string id in rewardIds)
            {
                Assert.AreEqual(itemsBefore[id], Owned(bag, id), $"{what}: '{id}' changed across the load.");
            }
        }

        // ------------------------------------------------------------------
        // Session
        // ------------------------------------------------------------------

        /// <summary>A new game with <see cref="Hero"/>, then "Mover" to the Pomar.</summary>
        private static IEnumerator NewGameIntoTheWorld()
        {
            yield return LoadScene(WorldMapSceneName);
            yield return SelectHero(Hero, requireCharacterSelect: false);
            yield return BackIntoTheWorld();
        }

        /// <summary>From the world map (or a load that resumed in Main), "Mover" to the Pomar: Main.unity in map mode with the stitched world.</summary>
        private static IEnumerator BackIntoTheWorld()
        {
            yield return WaitUntilOrFail(() =>
                {
                    HandleMapPopups();
                    return IsMapModeReady() || (SceneManager.GetActiveScene().name == WorldMapSceneName && UnityEngine.Object.FindFirstObjectByType<WorldMapController>() is WorldMapController m
                                                && m.isActiveAndEnabled && m.CurrentNode != null);
                }, "the world map (or the map mode) to be running", SceneLoadTimeoutSeconds);

            if (!IsMapModeReady())
            {
                WorldMapController map = null;
                yield return WaitForMap(m => map = m);
                Assert.IsTrue(GetService<SceneTransitionService>().TravelToMap(MapCatalog.PomarId), "TravelToMap refused the Pomar.");
                yield return WaitUntilOrFail(IsMapModeReady, "Main.unity to load in map mode with the overworld built", SceneLoadTimeoutSeconds);
            }

            MapModeController mode = MapModeController.Current;
            Assert.IsNotNull(mode.Overworld, "The Pomar must load the overworld.");
            Assert.IsTrue(mode.Overworld.IsStitched, "The Pomar must load the stitched world.");
            Assert.IsNotNull(mode.OverworldArea);
            FindPlayer().GetComponent<PlayerStats>().GrantInvulnerability(3600f);
            yield return WaitFrames(3);
        }

        private static bool IsMapModeReady() =>
            SceneManager.GetActiveScene().name == MainSceneName && MapModeController.Current != null && MapModeController.Current.IsReady
            && MapModeController.Current.OverworldArea != null;

        // ------------------------------------------------------------------
        // Moving the hero
        // ------------------------------------------------------------------

        /// <summary>Stands the hero at <paramref name="step"/>'s giver's door in their overworld town; the post must have the hero in range.</summary>
        private static IEnumerator GoToGiver(LateStoryStep step, Action<CityResidentPost> found, string what)
        {
            OverworldLayout world = MapModeController.Current.Overworld;
            OverworldSettlement settlement = world.Settlements.Find(s => s.Id == step.CityId);
            Assert.IsNotNull(settlement, $"{what}: the stitched world has no town '{step.CityId}'.");
            CityLot lot = settlement.Town.LotOfResident(step.GiverResidentId);
            Assert.IsNotNull(lot, $"{what}: {step.GiverResidentId} stands nowhere in {step.CityId}.");

            Vector2 stand = lot.StandPoint;
            Vector2 toVisitor = lot.DoorApproach - lot.StandPoint;
            Vector2 local = toVisitor.magnitude <= lot.InteractRadius * 0.75f && toVisitor.sqrMagnitude > 0.01f
                ? lot.DoorApproach
                : stand + (toVisitor.sqrMagnitude > 0.01f ? toVisitor.normalized : (lot.Front.sqrMagnitude > 0.01f ? lot.Front.normalized : Vector2.down)) * 1.2f;
            Vector2 at = local + settlement.Offset;

            GameObject player = FindPlayer();
            Place(player, at, (stand + settlement.Offset) - at);
            CityTownView town = MapModeController.Current.OverworldArea.TownOf(settlement);
            Assert.IsNotNull(town, $"{what}: the town '{step.CityId}' was not built with the hero standing in it.");
            CityResidentPost post = town.PostOf(lot.Id);
            Assert.IsNotNull(post, $"{what}: no CityResidentPost on {step.GiverResidentId}'s lot '{lot.Id}'.");
            Assert.AreEqual(step.GiverResidentId, post.ResidentId);
            yield return WaitUntilOrFail(() => post.PlayerInRange,
                $"{what}: {step.GiverResidentId}'s post to see the hero in range at {at} (keeper at {stand + settlement.Offset}, paused: {GameplayPause.IsPaused})", InRangeTimeoutSeconds);
            yield return WaitRealtime(0.1f);
            found(post);
        }

        /// <summary>Stands the hero in <paramref name="zone"/>, just outside <paramref name="clearing"/> (spawns keep 9 m off the hero).</summary>
        private static IEnumerator GoToZone(OverworldLayout world, OverworldZone zone, MapSpawnZone clearing, string what)
        {
            Vector2 center = clearing != null ? clearing.Center : zone.Bounds.center;
            float first = clearing != null ? clearing.Radius + 4f : 0f;
            Vector2? spot = null;
            foreach (float r in new[] { first, first + 8f, first + 16f, first + 28f })
            {
                for (int k = 0; k < 16 && !spot.HasValue; k++)
                {
                    float a = k * Mathf.PI * 2f / 16f;
                    Vector2 p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r;
                    if (OverworldRules.ZoneAt(world, p) == zone && OverworldRules.SettlementAt(world, p) == null && !MapLayoutRules.InRiver(world.World, p, 1.5f))
                    {
                        spot = p;
                    }
                }

                if (spot.HasValue)
                {
                    break;
                }
            }

            Assert.IsTrue(spot.HasValue, $"{what}: no dry spot of the zone '{zone.Id}' around {center}.");
            Place(FindPlayer(), spot.Value, center - spot.Value);
            yield return WaitRealtime(SettleSeconds);
            Assert.AreEqual(zone, MapModeController.Current.OverworldArea.CurrentZone, $"{what}: the overworld did not track the hero into '{zone.Id}' at {spot.Value}.");
        }

        /// <summary>The game's teleport (arrival, barca, respawn): chunks and towns built round the spot, the barrier's memory reset, the hero stood on the ground.</summary>
        private static void Place(GameObject player, Vector2 at, Vector2 facing)
        {
            OverworldView view = MapModeController.Current.OverworldArea;
            view.WarmUp(at);
            view.ResetTracking(at);
            CityModeController.PlaceAt(player, at, facing.sqrMagnitude > 0.01f ? facing : Vector2.down);
            if (player.TryGetComponent(out PlayerStats stats))
            {
                stats.GrantInvulnerability(3600f);
            }
        }

        // ------------------------------------------------------------------
        // Beasts
        // ------------------------------------------------------------------

        /// <summary>A spawn clearing of <paramref name="zone"/> whose table holds <paramref name="enemyId"/> (any clearing of the zone when null).</summary>
        private static MapSpawnZone ClearingFor(OverworldLayout world, OverworldZone zone, string enemyId, string what)
        {
            List<MapSpawnZone> clearings = world.World.Zones
                .Where(c => world.ZoneOfSpawn.TryGetValue(c.Id, out OverworldZone owner) && owner == zone)
                .ToList();
            if (enemyId == null)
            {
                return clearings.FirstOrDefault();
            }

            MapSpawnZone found = clearings
                .Where(c => c.AllEntries().Any(e => e != null && e.EnemyId == enemyId))
                .OrderByDescending(c => c.AllEntries().Any(e => e != null && e.EnemyId == enemyId && e.Form == NightForm.Normal))
                .FirstOrDefault();
            Assert.IsNotNull(found, $"{what}: no spawn clearing of the zone '{zone.Id}' ({clearings.Count} clearings) ever rolls '{enemyId}': the step cannot be done there.");
            return found;
        }

        private static MapMonsterSpawner FindSpawner(MapSpawnZone clearing) =>
            UnityEngine.Object.FindObjectsByType<MapMonsterSpawner>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).FirstOrDefault(s => s.Zone == clearing);

        private static IEnumerator WaitForSpawner(MapSpawnZone clearing, Action<MapMonsterSpawner> found, string what)
        {
            yield return WaitUntilOrFail(() => FindSpawner(clearing) != null, $"{what}: the overworld to wake the spawner of the clearing '{clearing.Id}'", SpawnerTimeoutSeconds);
            found(FindSpawner(clearing));
        }

        /// <summary>
        /// Rolls the clearing (<see cref="MapMonsterSpawner.TrySpawnOne"/>, in a period whose table holds the target) until a <paramref name="enemyId"/>
        /// comes; every other beast rolled on the way is killed, a night variant of the target is put away when <paramref name="allowVariant"/> is off.
        /// </summary>
        private static IEnumerator SpawnTarget(MapMonsterSpawner spawner, string enemyId, bool allowVariant, Action<EnemyController> found, string what)
        {
            TimeOfDayPhase phase = PhaseFor(spawner.Zone, enemyId);
            for (int attempt = 0; attempt < MaxSpawnAttemptsPerTarget; attempt++)
            {
                GameObject go = spawner.TrySpawnOne(phase);
                if (go == null)
                {
                    yield return null;
                    continue;
                }

                EnemyController enemy = go.GetComponent<EnemyController>();
                Assert.IsNotNull(enemy, $"{what}: the spawner made '{go.name}' without an EnemyController.");
                Assert.IsNotNull(enemy.Health, $"{what}: '{go.name}' has no Health.");
                bool target = enemy.Definition != null && enemy.Definition.name == enemyId;
                bool variant = go.GetComponent<NightVariantTag>() != null;
                if (target && (allowVariant || !variant))
                {
                    found(enemy);
                    yield break;
                }

                if (target)
                {
                    UnityEngine.Object.Destroy(go);
                }
                else
                {
                    Kill(enemy, what);
                }

                yield return null;
            }

            Assert.Fail($"{what}: {MaxSpawnAttemptsPerTarget} rolls of the clearing '{spawner.Zone.Id}' ({phase}) brought no '{enemyId}'{(allowVariant ? string.Empty : " in its normal form")}.");
        }

        /// <summary>The period whose table holds <paramref name="enemyId"/> in its normal form (the current one first), else any that holds it.</summary>
        private static TimeOfDayPhase PhaseFor(MapSpawnZone clearing, string enemyId)
        {
            var phases = new List<TimeOfDayPhase>();
            if (Rapadura.Core.DI.ServiceLocator.TryGet(out TimeOfDayManager time) && time != null)
            {
                phases.Add(time.Phase);
            }

            phases.AddRange(Enum.GetValues(typeof(TimeOfDayPhase)).Cast<TimeOfDayPhase>());
            foreach (TimeOfDayPhase phase in phases)
            {
                if (clearing.EntriesFor(phase)?.Any(e => e != null && e.EnemyId == enemyId && e.Form == NightForm.Normal) == true)
                {
                    return phase;
                }
            }

            foreach (TimeOfDayPhase phase in phases)
            {
                if (clearing.EntriesFor(phase)?.Any(e => e != null && e.EnemyId == enemyId) == true)
                {
                    return phase;
                }
            }

            Assert.Fail($"The clearing '{clearing.Id}' never rolls '{enemyId}'.");
            return default;
        }

        /// <summary>
        /// Tages, the seal of the first step, is the campaign's last boss and no monster of the open world: instantiated a few metres from the hero in
        /// his hall the way the spawners make a beast (the arena's enemy prefab, ResetForSpawn, the procedural look).
        /// </summary>
        private static EnemyController SpawnTages(GameObject player, string what)
        {
            EnemyDefinition tages = Resources.Load<EnemyDefinition>($"{EncounterPayload.EnemyResourcesFolder}/{MidStoryCatalog.SealEnemyId}");
            Assert.IsNotNull(tages, $"{what}: no EnemyDefinition '{MidStoryCatalog.SealEnemyId}' under Resources.");
            EncounterArenaController arena = UnityEngine.Object.FindAnyObjectByType<EncounterArenaController>(FindObjectsInactive.Include);
            Assert.IsNotNull(arena?.EnemyPrefab, $"{what}: Main.unity has no arena enemy prefab.");

            OverworldView view = MapModeController.Current.OverworldArea;
            Vector2 hero = new Vector2(player.transform.position.x, player.transform.position.z);
            Vector2 ground = hero + new Vector2(player.transform.forward.x, player.transform.forward.z).normalized * 6f;
            float foot = EnemyVisualSwapper.GetFootHeight(arena.EnemyPrefab.transform) * arena.EnemyPrefab.transform.localScale.y;
            Vector3 position = EncounterSpawnLayout.StandOnGround(new Vector3(ground.x, view.GroundHeight(ground), ground.y), foot);
            GameObject instance = UnityEngine.Object.Instantiate(arena.EnemyPrefab, position, Quaternion.identity);
            instance.name = $"{arena.EnemyPrefab.name}_{tages.name}_midstory_seal";
            EnemyController controller = instance.GetComponent<EnemyController>();
            Assert.IsNotNull(controller, $"{what}: the arena prefab has no EnemyController.");
            controller.ResetForSpawn(tages, position);
            EnemyVisualSwapper.Apply(instance, tages);
            Assert.IsNotNull(controller.Health, $"{what}: Tages has no Health.");
            return controller;
        }

        /// <summary>Real damage, no attacker (no "Perigoso" miss roll, no kill XP): the death goes through Health → CombatTargetDiedEvent.</summary>
        private static void Kill(EnemyController enemy, string what)
        {
            enemy.Health.ClearInvulnerability();
            enemy.Health.ApplyDamage(KillDamage, ElementType.Physical, null);
            Assert.IsTrue(enemy.Health.IsDead, $"{what}: '{enemy.name}' survived {KillDamage} damage at {enemy.transform.position}.");
        }

        private static int Kills(QuestManager quests, string questId) =>
            quests.GetProgress(questId)?.Objectives.Single(o => o.Type == QuestObjectiveType.KillEnemyType).CurrentAmount ?? -1;

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>Raises the hero to <paramref name="level"/> through <see cref="PlayerStats.AddExperience"/>, one level's bar at a time.</summary>
        private static void RaiseLevel(PlayerStats stats, int level)
        {
            for (int guard = 0; stats.Level < level && guard < 1000; guard++)
            {
                stats.AddExperience(Mathf.Max(1, stats.GetExperienceToNextLevel() - stats.CurrentExperience));
            }

            Assert.AreEqual(level, stats.Level, $"AddExperience did not take the hero to level {level}.");
            stats.GrantInvulnerability(3600f);
        }

        /// <summary>What the hero owns of <paramref name="itemId"/>: the bag plus the Baú da Vó Edna (where a reward that does not fit goes).</summary>
        private static int Owned(InventoryManager bag, string itemId)
        {
            int total = bag != null ? bag.GetTotalCount(itemId) : 0;
            VillageChestStorage chest = VillageChestStorage.ResolveShared();
            if (chest != null)
            {
                total += chest.Slots.Where(s => s != null && s.itemId == itemId).Sum(s => s.quantity);
                total += chest.Inbox.Where(s => s != null && s.itemId == itemId).Sum(s => s.quantity);
            }

            return total;
        }

        private static string Describe(QuestProgress progress) =>
            progress == null ? "not running" : string.Join(", ", progress.Objectives.Select(o => $"{o.ObjectiveId} {o.CurrentAmount}/{o.RequiredAmount}"));

        private static IEnumerator WaitRealtime(float seconds)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (Time.realtimeSinceStartup < end)
            {
                yield return null;
            }
        }
    }
}
