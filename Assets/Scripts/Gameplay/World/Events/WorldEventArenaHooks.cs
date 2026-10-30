using System.Collections;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Content;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Events
{
    /// <summary>
    /// The combat-scene half of the world map road events, self-bootstrapping (no scene edit): after every scene
    /// load with a pending encounter (<see cref="SceneTransitionService.HasPendingEncounter"/>, i.e. Main.unity
    /// entered from the map) a "WorldEventArenaHooks" object is created in that scene, and dies with it.
    /// <list type="bullet">
    /// <item>Boons: two frames in (GameManager restores the carried PlayerStats one frame after the load, which
    /// would overwrite health/XP) the queued XP, heal and buffs of <see cref="WorldEventState"/> go to the Player
    /// (<see cref="WorldEventOutcomes.ApplyPendingBoons"/>).</item>
    /// <item>Elite ambush: when the pending payload is the one <c>WorldEventCardController</c> launched
    /// (<see cref="WorldEventState.PendingEliteSeed"/>), the first spawned enemy becomes the elite (health ×2,
    /// +35% damage, bigger) and its death pays the bonus gold, extra loot rolls and bonus XP.</item>
    /// </list>
    /// Created on sceneLoaded, i.e. before the scene's Start, so it is subscribed before
    /// EncounterArenaController.Start spawns the enemies.
    /// </summary>
    public class WorldEventArenaHooks : MonoBehaviour
    {
        public const string HostName = "WorldEventArenaHooks";
        private const string PlayerTag = "Player";
        private const string LogCategory = "WorldEvents";

        private readonly System.Random _random = new System.Random();
        private WorldEventState _state;
        private bool _eliteEncounter;
        private GameObject _elite;
        private BiomeType _region;

        public bool IsEliteEncounter => _eliteEncounter;
        public GameObject Elite => _elite;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!ServiceLocator.TryGet(out SceneTransitionService transition) || !transition.HasPendingEncounter)
            {
                return;
            }

            if (FindFirstObjectByType<WorldEventArenaHooks>() != null)
            {
                return;
            }

            var host = new GameObject(HostName);
            SceneManager.MoveGameObjectToScene(host, scene);
            host.AddComponent<WorldEventArenaHooks>().Begin(transition.PendingEncounter, WorldEventState.ResolveShared());
        }

        /// <summary>Binds this fight: elite or not, and schedules the boons.</summary>
        public void Begin(EncounterPayload payload, WorldEventState state)
        {
            _state = state;
            _region = payload != null ? payload.biome : BiomeType.Forest;
            _eliteEncounter = payload != null && state != null && state.PendingEliteSeed.HasValue && state.PendingEliteSeed.Value == payload.seed;

            if (state != null && state.HasPendingBoons && isActiveAndEnabled)
            {
                StartCoroutine(ApplyBoonsAfterCarriedRestore());
            }
        }

        private void OnEnable()
        {
            EventBus.Subscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Subscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<EnemySpawnedEvent>(OnEnemySpawned);
            EventBus.Unsubscribe<CombatTargetDiedEvent>(OnCombatTargetDied);
            EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
        }

        private IEnumerator ApplyBoonsAfterCarriedRestore()
        {
            yield return null;
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null)
            {
                yield break;
            }

            if (WorldEventOutcomes.ApplyPendingBoons(_state, player.GetComponent<PlayerStats>(), player.GetComponent<BuffController>()))
            {
                GameLogger.Info(LogCategory, "Road event boons applied to the hero for this fight.");
            }
        }

        private void OnEnemySpawned(EnemySpawnedEvent evt)
        {
            if (!_eliteEncounter || _elite != null || evt.Enemy == null)
            {
                return;
            }

            _elite = evt.Enemy;
            StartCoroutine(MakeEliteAfterStart(_elite));
        }

        /// <summary>
        /// EnemyController.Start re-applies the definition's health (ApplyDefinitionToHealth), which would undo a
        /// doubling done at spawn time: wait a frame. The arena's "Prepare-se!" hold (1.75 s) covers it.
        /// </summary>
        private IEnumerator MakeEliteAfterStart(GameObject enemy)
        {
            yield return null;
            if (enemy != null)
            {
                MakeElite(enemy);
            }
        }

        /// <summary>
        /// Health ×<see cref="WorldEventElite.HealthMultiplier"/>, +damage buff, a bigger model with the boss tint (its
        /// <see cref="EnemyVisualSwapper.VisualName"/> child, whose pivot is at the feet, so it stays on the ground; the
        /// root itself without one). Public for tests.
        /// </summary>
        public static void MakeElite(GameObject enemy)
        {
            if (enemy == null)
            {
                return;
            }

            EnemyController controller = enemy.GetComponent<EnemyController>();
            Health health = controller != null ? controller.Health : enemy.GetComponent<Health>();
            if (health != null)
            {
                health.ConfigureMaxHealth(health.MaxHealth * WorldEventElite.HealthMultiplier);
            }

            BuffController buffs = controller != null ? controller.Buffs : enemy.GetComponent<BuffController>();
            if (buffs != null)
            {
                buffs.ApplyEffect(new StatModifierDefinition
                {
                    displayName = WorldEventElite.BuffSource,
                    affectedStat = StatType.AttackDamage,
                    application = ModifierApplication.PercentAdditive,
                    value = WorldEventElite.AttackDamageBonus,
                    durationSeconds = 0f,
                }, WorldEventElite.BuffSource);
            }

            Transform visual = enemy.transform.Find(EnemyVisualSwapper.VisualName);
            Transform scaled = visual != null ? visual : enemy.transform;
            scaled.localScale *= WorldEventElite.ScaleMultiplier;
            if (visual != null && visual.GetComponent<Rapadura.Gameplay.Enemies.Visuals.ProceduralEnemyRig>() == null)
            {
                EnemyVisualSwapper.TintBossModel(visual.gameObject);
            }
        }

        private void OnCombatTargetDied(CombatTargetDiedEvent evt)
        {
            if (_elite == null || evt.Target != _elite)
            {
                return;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            InventoryManager inventory = player != null ? player.GetComponent<InventoryManager>() : null;
            PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;
            EnemyController controller = _elite.GetComponent<EnemyController>();

            Vector3 labelPosition = _elite.transform.position + Vector3.up * 2.6f;
            int line = 0;

            if (inventory != null)
            {
                System.Func<string, int, int> outstanding = ServiceLocator.TryGet(out QuestManager quests) && quests != null
                    ? quests.GetOutstandingCollectAmount
                    : (System.Func<string, int, int>)null;

                var drops = new List<LootDrop>();
                Rapadura.Gameplay.Items.ItemDefinition gold = Rapadura.Gameplay.Items.ItemDatabase.GetById(WorldEventCatalog.Gold);
                if (gold != null)
                {
                    drops.Add(new LootDrop(gold, WorldEventElite.BonusGold));
                }

                if (controller != null && controller.Definition != null)
                {
                    BiomeDefinition biome = BiomeRegistry.FindBiomeOf(controller.Definition);
                    for (int i = 0; i < WorldEventElite.ExtraLootRolls; i++)
                    {
                        drops.AddRange(EnemyLootRoller.Roll(controller.Definition, biome, _random));
                    }
                }

                foreach (LootDrop drop in drops)
                {
                    LootGrantResult result = LootGrantRules.Grant(inventory, drop, outstanding);
                    if (result.Granted > 0)
                    {
                        string name = drop.IsGenerated ? drop.Generated.GeneratedName : drop.Item.LocalizedName();
                        CombatFxManager.SpawnLootText(labelPosition + Vector3.up * (0.35f * line++),
                            result.Granted > 1 ? $"+{result.Granted} {name}" : "+ " + name);
                    }
                }
            }

            if (stats != null)
            {
                stats.AddExperience(WorldEventElite.ScaledExperience(_region));
            }

            GameLogger.Info(LogCategory, $"Elite down in {_region}: bonus gold, {WorldEventElite.ExtraLootRolls} extra loot rolls and XP granted.");
            _elite = null;
            _eliteEncounter = false;
        }

        private void OnEncounterEnded(EncounterEndedEvent evt)
        {
            if (_state != null && evt.Payload != null && _state.PendingEliteSeed == evt.Payload.seed)
            {
                _state.PendingEliteSeed = null;
            }
        }
    }
}
