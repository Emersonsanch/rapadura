using Rapadura.Core.DI;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Shop;
using UnityEngine;

namespace Rapadura.Gameplay.World
{
    /// <summary>
    /// Generic "talk to / trade with this NPC" trigger, mirroring <see cref="SpawnPoint"/>'s shape
    /// (plain MonoBehaviour reacting to a trigger collider, resolving managers via
    /// <see cref="ServiceLocator.TryGet{TService}"/> rather than a hard reference).
    ///
    /// Configured entirely via the Inspector: assign either <see cref="_dialogue"/> or
    /// <see cref="_shop"/> (or both — dialogue takes priority when both are set, since a common
    /// pattern is "greet, then optionally redirect into the shop via a dialogue choice", as seen
    /// in <c>DialogueSeeder.BuildBlacksmithOffer</c>). <see cref="Interact"/> is public so it can
    /// also be called directly by a player input/interaction-prompt system instead of relying on
    /// physics triggers.
    ///
    /// Trigger-based flow (<see cref="OnTriggerEnter"/>/<see cref="OnTriggerExit"/>) tracks whether
    /// the player is currently inside range via <see cref="_playerInRange"/> so a future input
    /// script can guard "only allow Interact() while in range" without duplicating the tag check;
    /// today it also auto-fires on enter for parity with <see cref="SpawnPoint"/>'s auto-trigger
    /// behaviour, since this project has no dedicated player-input/interact-prompt component yet.
    ///
    /// NOTE: this script is a reusable component only. Actually placing it on an NPC
    /// GameObject/prefab in a Scene (adding a Collider marked "Is Trigger", assigning the
    /// DialogueDefinition/ShopDefinition asset references in the Inspector, tagging the player
    /// object "Player") is Unity Editor/Scene work and is intentionally NOT done by this change.
    /// </summary>
    [DisallowMultipleComponent]
    public class NpcInteractable : MonoBehaviour
    {
        private const string LogCategory = "NpcInteractable";
        private const string PlayerTag = "Player";

        [Header("Interaction source (assign one)")]
        [SerializeField] private DialogueDefinition _dialogue;
        [SerializeField] private ShopDefinition _shop;

        [Header("Trigger behaviour")]
        [Tooltip("If true, entering the trigger immediately calls Interact(). If false, only the public Interact() call (e.g. from a player-input/interact-prompt script) does.")]
        [SerializeField] private bool _interactOnTriggerEnter = true;

        private bool _playerInRange;

        public DialogueDefinition Dialogue => _dialogue;
        public ShopDefinition Shop => _shop;
        public bool PlayerInRange => _playerInRange;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            _playerInRange = true;

            if (_interactOnTriggerEnter)
            {
                Interact();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            _playerInRange = false;
        }

        /// <summary>
        /// Starts whichever interaction is configured: a dialogue via
        /// <see cref="DialogueManager.StartDialogue"/> (preferred when both are assigned) or a shop
        /// session via <see cref="ShopManager.OpenShop"/>. Resolves both the target manager and,
        /// for shops, the buyer's <see cref="InventoryManager"/> through the
        /// <see cref="ServiceLocator"/>; logs a warning and no-ops instead of throwing if a
        /// required manager isn't registered (e.g. called before <c>GameManager.BuildManagers()</c>
        /// has run, or from a scene/test that never registered one).
        /// </summary>
        public void Interact()
        {
            if (_dialogue != null)
            {
                InteractDialogue();
                return;
            }

            if (_shop != null)
            {
                InteractShop();
                return;
            }

            GameLogger.Warning(LogCategory, $"NpcInteractable on '{name}' has neither a DialogueDefinition nor a ShopDefinition assigned.");
        }

        private void InteractDialogue()
        {
            if (!ServiceLocator.TryGet(out DialogueManager dialogueManager))
            {
                GameLogger.Warning(LogCategory, $"NpcInteractable on '{name}' wants to start dialogue '{_dialogue.DialogueId}' but no DialogueManager is registered.");
                return;
            }

            dialogueManager.StartDialogue(_dialogue);
        }

        private void InteractShop()
        {
            if (!ServiceLocator.TryGet(out ShopManager shopManager))
            {
                GameLogger.Warning(LogCategory, $"NpcInteractable on '{name}' wants to open shop '{_shop.ShopId}' but no ShopManager is registered.");
                return;
            }

            InventoryManager inventoryManager = ResolveBuyerInventory();

            if (inventoryManager == null)
            {
                GameLogger.Warning(LogCategory, $"NpcInteractable on '{name}' wants to open shop '{_shop.ShopId}' but no buyer InventoryManager could be resolved.");
                return;
            }

            shopManager.OpenShop(_shop, inventoryManager);
        }

        /// <summary>
        /// Resolves the buyer's inventory the same way other systems do (e.g. WeaponController/
        /// RangedWeapon hold a direct <see cref="InventoryManager"/> reference,
        /// <see cref="CheckpointManager"/> looks up the player by tag) —
        /// <see cref="InventoryManager"/> is a MonoBehaviour on the player object,
        /// not a ServiceLocator-registered manager, so it's found via
        /// <see cref="GameObject.FindGameObjectWithTag"/> + <see cref="GameObject.GetComponent{T}"/>
        /// rather than <see cref="ServiceLocator.TryGet{TService}"/>.
        /// </summary>
        private static InventoryManager ResolveBuyerInventory()
        {
            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            return player != null ? player.GetComponent<InventoryManager>() : null;
        }

#if UNITY_EDITOR
        /// <summary>Editor/test-only helper to assign the interaction source without a SerializedObject round-trip, mirroring DialogueDefinition.SetDataForTests.</summary>
        public void SetInteractionSourceForTests(DialogueDefinition dialogue, ShopDefinition shop)
        {
            _dialogue = dialogue;
            _shop = shop;
        }
#endif
    }
}
