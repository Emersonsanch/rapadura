using Rapadura.Gameplay.Crafting;
using Rapadura.Gameplay.Inventory;
using Rapadura.UI.HUD;
using UnityEngine;

namespace Rapadura.UI.Crafting
{
    /// <summary>
    /// Physical "bancada": a trigger collider that opens the Crafting window for the player while
    /// they stand inside it, passing this station's <see cref="CraftingStationType"/> so recipes
    /// requiring it become craftable. Same plain-trigger shape as <c>NpcInteractable</c>/
    /// <c>SpawnPoint</c> (Collider marked "Is Trigger", player tagged "Player").
    ///
    /// Opening goes through the HUD's screen-exclusivity rule (<see cref="HudController.OpenScreenExclusive"/>)
    /// so walking up to a bancada closes Mochila/Missões/Loja/etc.; without a HUD in the scene it
    /// opens the window directly, as before.
    /// </summary>
    [DisallowMultipleComponent]
    public class CraftingStationTrigger : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [SerializeField] private CraftingUIController _craftingUI;
        [SerializeField] private CraftingStationType _station = CraftingStationType.Workbench;
        [Tooltip("If true, entering the trigger opens the window immediately; otherwise call Interact() from an interact prompt.")]
        [SerializeField] private bool _openOnTriggerEnter = true;
        [SerializeField] private bool _closeOnTriggerExit = true;
        [Tooltip("HUD whose screen-exclusivity rule the window opens through. If empty, resolved from the scene on first use.")]
        [SerializeField] private HudController _hud;

        private InventoryManager _playerInventory;

        public CraftingStationType Station => _station;
        public bool PlayerInRange => _playerInventory != null;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            _playerInventory = other.GetComponentInParent<InventoryManager>();

            if (_openOnTriggerEnter)
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

            _playerInventory = null;

            if (_closeOnTriggerExit && _craftingUI != null && _craftingUI.IsOpen)
            {
                _craftingUI.Close();
            }
        }

        public void Interact()
        {
            if (_craftingUI == null)
            {
                return;
            }

            if (_hud == null)
            {
                _hud = FindFirstObjectByType<HudController>();
            }

            if (_hud == null || !_hud.OpenScreenExclusive(HudController.CraftingScreenId, OpenCraftingWindow))
            {
                OpenCraftingWindow();
            }
        }

        private void OpenCraftingWindow()
        {
            _craftingUI.Open(_playerInventory, _station);
        }
    }
}
