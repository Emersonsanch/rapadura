using System.Collections;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Skills;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>
    /// Puts the relic collection to work on the scene's Player (Main.unity — the world map has none):
    /// <list type="bullet">
    /// <item>Each owned relic's passive becomes a permanent <see cref="BuffController"/> modifier under its own
    /// source key (<see cref="RelicDefinition.SourceKey"/>, "relic/&lt;id&gt;"), sized for the hero playing
    /// (<see cref="RelicDefinition.ValueFor"/>). Re-applied two frames after every scene load (after
    /// GameManager's carried-state restore), on <see cref="GameLoadedEvent"/> (InventoryManager.RestoreState
    /// clears every effect), on a new relic and on a hero change.</item>
    /// <item>Gold won on the map (<see cref="SecretsProgress.PendingGold"/>) goes into the bag as
    /// <see cref="GoldItemId"/>; whatever does not fit stays pending.</item>
    /// </list>
    /// Self-bootstrapping (one DontDestroyOnLoad host), no scene wiring.
    /// </summary>
    public class RelicPassiveApplier : MonoBehaviour
    {
        public const string GoldItemId = "item_gold_coin";
        private const string PlayerTag = "Player";

        private static RelicPassiveApplier _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null)
            {
                return;
            }

            var host = new GameObject(nameof(RelicPassiveApplier));
            DontDestroyOnLoad(host);
            _instance = host.AddComponent<RelicPassiveApplier>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Subscribe<RelicCollectedEvent>(OnRelicCollected);
            EventBus.Subscribe<CharacterSelectedEvent>(OnCharacterSelected);
            StartCoroutine(ApplyAfterSceneStart());
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
            EventBus.Unsubscribe<RelicCollectedEvent>(OnRelicCollected);
            EventBus.Unsubscribe<CharacterSelectedEvent>(OnCharacterSelected);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => StartCoroutine(ApplyAfterSceneStart());

        private void OnGameLoaded(GameLoadedEvent _) => ApplyNow();

        private void OnRelicCollected(RelicCollectedEvent _) => ApplyNow();

        private void OnCharacterSelected(CharacterSelectedEvent _) => ApplyNow();

        /// <summary>GameManager restores the carried player state one frame after the load; land after it.</summary>
        private IEnumerator ApplyAfterSceneStart()
        {
            yield return null;
            yield return null;
            ApplyNow();
        }

        private static void ApplyNow()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            if (player == null)
            {
                return;
            }

            SecretsProgress secrets = SecretsProgress.ResolveShared();
            CharacterId? playing = ServiceLocator.TryGet(out CharacterSelectionState selection) ? selection.Selected : null;

            if (player.TryGetComponent(out BuffController buffs))
            {
                ApplyPassives(buffs, secrets.OwnedRelics, playing);
            }

            if (player.TryGetComponent(out InventoryManager inventory))
            {
                DeliverPendingGold(secrets, inventory, ItemDatabase.GetById(GoldItemId));
            }
        }

        // ------------------------------------------------------------------
        // Pure-ish helpers (EditMode-tested)
        // ------------------------------------------------------------------

        /// <summary>
        /// Replaces every relic modifier on <paramref name="buffs"/> with one per owned relic (unknown ids skipped).
        /// Idempotent: applying twice leaves exactly one modifier per relic. Returns how many were applied.
        /// </summary>
        public static int ApplyPassives(BuffController buffs, IEnumerable<string> ownedRelicIds, CharacterId? playing)
        {
            if (buffs == null)
            {
                return 0;
            }

            buffs.RemoveEffectsBySourcePrefix(RelicCatalog.PassiveSourcePrefix);
            int applied = 0;
            if (ownedRelicIds == null)
            {
                return 0;
            }

            foreach (string id in ownedRelicIds)
            {
                RelicDefinition relic = RelicCatalog.Get(id);
                if (relic == null)
                {
                    continue;
                }

                buffs.ApplyEffect(relic.CreateModifier(playing), relic.SourceKey);
                applied++;
            }

            return applied;
        }

        /// <summary>Moves the queued map gold into <paramref name="inventory"/>; what does not fit stays queued. Returns the amount delivered.</summary>
        public static int DeliverPendingGold(SecretsProgress secrets, InventoryManager inventory, ItemDefinition gold)
        {
            if (secrets == null || inventory == null || gold == null || secrets.PendingGold <= 0)
            {
                return 0;
            }

            int amount = secrets.TakePendingGold();
            int leftover = inventory.AddItem(gold, amount);
            secrets.ReturnPendingGold(leftover);
            return amount - leftover;
        }
    }
}
