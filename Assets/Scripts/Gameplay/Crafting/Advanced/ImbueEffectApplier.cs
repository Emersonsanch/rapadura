using System.Collections;
using Rapadura.Core.DI;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.Skills;
using Rapadura.Gameplay.World.Map;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Crafting.Advanced
{
    /// <summary>
    /// Puts Vô Jorge's weapon enchantment (<see cref="CraftingManager.Imbue"/>) on the hero in the arena. Self-bootstrapping,
    /// no scene edit (same pattern as <c>MealEffectApplier</c>): created in every scene that has a pending encounter.
    /// Waits <see cref="SettleFrames"/> frames (the carried-state restore and the road-event boons land first), then applies
    /// the modifier for the whole encounter unless a road blessing is already active or no weapon is in hand
    /// (<see cref="ImbueRules.AppliesThisFight"/>); only then is the fight counted against the enchantment.
    /// </summary>
    public class ImbueEffectApplier : MonoBehaviour
    {
        public const int SettleFrames = 5;
        private const float RecheckInterval = 0.5f;
        private const string PlayerTag = "Player";

        private BuffController _buffs;
        private StatModifierDefinition _effect;
        private float _nextCheck;

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

            if (FindFirstObjectByType<ImbueEffectApplier>() != null)
            {
                return;
            }

            var host = new GameObject(nameof(ImbueEffectApplier));
            if (scene.IsValid() && scene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(host, scene);
            }

            host.AddComponent<ImbueEffectApplier>();
        }

        private IEnumerator Start()
        {
            for (int i = 0; i < SettleFrames; i++)
            {
                yield return null;
            }

            if (!ServiceLocator.TryGet(out CraftingManager crafting) || crafting == null || !crafting.Imbue.IsActive)
            {
                yield break;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null)
            {
                PlayerStats stats = FindFirstObjectByType<PlayerStats>();
                player = stats != null ? stats.gameObject : null;
            }

            _buffs = player != null ? player.GetComponent<BuffController>() : null;
            if (_buffs == null)
            {
                yield break;
            }

            InventoryManager bag = player.GetComponentInParent<InventoryManager>();
            bool weapon = bag != null && bag.Equipped.TryGetValue(EquipmentSlot.MainHand, out InventorySlotData hand) && hand != null && !hand.IsEmpty;
            bool blessed = _buffs.HasEffectsFromSource(ImbueRules.RoadBlessingSource);
            if (!ImbueRules.AppliesThisFight(crafting.Imbue, blessed, weapon))
            {
                yield break;
            }

            _effect = ImbueRules.BuildEffect(ImbueCatalog.Get(crafting.Imbue.imbueId));
            _buffs.ApplyEffect(_effect, ImbueRules.EffectSourceKey, _buffs.gameObject);
            crafting.MarkImbueApplied();
        }

        private void Update()
        {
            if (_effect == null || _buffs == null || Time.unscaledTime < _nextCheck)
            {
                return;
            }

            // Put back if something cleared it later (a revive restoring the carried state).
            _nextCheck = Time.unscaledTime + RecheckInterval;
            if (!_buffs.HasEffectsFromSource(ImbueRules.EffectSourceKey))
            {
                _buffs.ApplyEffect(_effect, ImbueRules.EffectSourceKey, _buffs.gameObject);
            }
        }
    }
}
