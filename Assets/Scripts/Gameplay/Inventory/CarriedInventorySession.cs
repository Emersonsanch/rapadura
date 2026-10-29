using Rapadura.Save;
using UnityEngine;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// A live <see cref="InventoryManager"/> for a scene without a Player (the world map), backed by the
    /// inventory <see cref="SaveManager"/> carries between scenes. The hero's InventoryManager lives on the
    /// Player in Main.unity; on the map only its carried JSON exists, which shops and crafting cannot use.
    /// <list type="bullet">
    /// <item><see cref="Open"/> creates a stand-alone InventoryManager (no PlayerStats/BuffController: equip
    /// bonuses are not applied, which is fine off-combat), registers it under the same save key and applies
    /// the carried state to it (<see cref="SaveManager.RestoreCarriedState"/>), so the carried entry is consumed.</item>
    /// <item>While open it is a regular registered saveable: a save written now captures it, a load restores it.</item>
    /// <item><see cref="Close"/> unregisters it, which captures its state back into the carry (and the HUD's
    /// Mochila summary), then destroys it. The next scene's Player picks that up as usual, so purchases and
    /// crafted items reach the next encounter and every save.</item>
    /// </list>
    /// Keep sessions short (while a shop/crafting window is open): the carry is the source of truth otherwise.
    /// </summary>
    public sealed class CarriedInventorySession
    {
        public const string HostName = "CarriedInventory";

        private readonly SaveManager _saveManager;
        private GameObject _host;

        public CarriedInventorySession(SaveManager saveManager)
        {
            _saveManager = saveManager;
        }

        /// <summary>The live inventory, or null when closed (or destroyed with its scene).</summary>
        public InventoryManager Inventory { get; private set; }

        public bool IsOpen => Inventory != null;

        /// <summary>Opens (or returns the already open) live inventory. <paramref name="parent"/> ties it to a scene object.</summary>
        public InventoryManager Open(Transform parent = null)
        {
            if (Inventory != null)
            {
                return Inventory;
            }

            _host = new GameObject(HostName);
            if (parent != null)
            {
                _host.transform.SetParent(parent, false);
            }

            Inventory = _host.AddComponent<InventoryManager>();

            if (_saveManager != null)
            {
                _saveManager.Register(Inventory);
                _saveManager.RestoreCarriedState();
            }

            return Inventory;
        }

        /// <summary>Hands the inventory back to the carry and destroys it. Safe to call when closed.</summary>
        public void Close()
        {
            InventoryManager inventory = Inventory;
            Inventory = null;

            if (inventory != null)
            {
                _saveManager?.Unregister(inventory);
            }

            if (_host != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(_host);
                }
                else
                {
                    Object.DestroyImmediate(_host);
                }
            }

            _host = null;
        }
    }
}
