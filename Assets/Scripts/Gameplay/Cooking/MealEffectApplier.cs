using System.Collections;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Cooking
{
    /// <summary>
    /// Feeds the active meal (<see cref="MealService"/>) to the hero in the arena. Self-bootstrapping, no scene edit:
    /// after every scene load that has a pending encounter (<see cref="SceneTransitionService.HasPendingEncounter"/>)
    /// an instance is created in that scene, and dies with it.
    /// <list type="bullet">
    /// <item>Waits <see cref="SettleFrames"/> frames first: GameManager hands the carried player state to the fresh
    /// Player one frame after the load, and <c>InventoryManager.RestoreState</c> clears every BuffController effect.</item>
    /// <item>Whole-encounter effects go under <see cref="CookingRules.EffectSourceKey"/> (duration 0, so they last until
    /// the scene unloads) and are put back if anything clears them later (a revive restoring the carried state);
    /// the opening burst under <see cref="CookingRules.BurstSourceKey"/>, once.</item>
    /// <item>The favourite bonus is decided by the hero actually in the arena (<see cref="CharacterSelectionState"/>).</item>
    /// </list>
    /// </summary>
    public class MealEffectApplier : MonoBehaviour
    {
        public const int SettleFrames = 3;
        private const float RecheckInterval = 0.5f;
        private const string PlayerTag = "Player";

        private BuffController _buffs;
        private MealRecipeSpec _recipe;
        private List<StatModifierDefinition> _encounterEffects;
        private float _nextCheck;
        private bool _ready;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            AttachIfEncounter(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => AttachIfEncounter(scene);

        private static void AttachIfEncounter(Scene scene)
        {
            if (!ServiceLocator.TryGet(out SceneTransitionService transition) || !transition.HasPendingEncounter)
            {
                return;
            }

            if (FindFirstObjectByType<MealEffectApplier>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(MealEffectApplier));
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(host, scene);
            }

            host.AddComponent<MealEffectApplier>();
        }

        private IEnumerator Start()
        {
            for (int i = 0; i < SettleFrames; i++)
            {
                yield return null;
            }

            MealService meals = MealService.ResolveShared();
            _recipe = meals.ActiveRecipe;
            if (_recipe == null)
            {
                yield break;
            }

            _buffs = ResolvePlayerBuffs();
            if (_buffs == null)
            {
                yield break;
            }

            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;
            _encounterEffects = CookingRules.BuildEncounterEffects(_recipe, hero);
            List<StatModifierDefinition> burst = CookingRules.BuildOpeningBurst(_recipe, hero);

            ApplyEncounterEffects();
            if (burst.Count > 0)
            {
                _buffs.ApplyEffects(burst, CookingRules.BurstSourceKey, _buffs.gameObject);
            }

            _ready = true;
        }

        private void Update()
        {
            if (!_ready || _buffs == null || Time.unscaledTime < _nextCheck)
            {
                return;
            }

            _nextCheck = Time.unscaledTime + RecheckInterval;
            if (_encounterEffects.Count > 0 && !_buffs.HasEffectsFromSource(CookingRules.EffectSourceKey))
            {
                ApplyEncounterEffects();
            }
        }

        private void ApplyEncounterEffects()
        {
            if (_encounterEffects.Count > 0)
            {
                _buffs.ApplyEffects(_encounterEffects, CookingRules.EffectSourceKey, _buffs.gameObject);
            }
        }

        private static BuffController ResolvePlayerBuffs()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player != null && player.TryGetComponent(out BuffController buffs))
            {
                return buffs;
            }

            PlayerStats stats = FindFirstObjectByType<PlayerStats>();
            return stats != null ? stats.GetComponent<BuffController>() : null;
        }
    }
}
