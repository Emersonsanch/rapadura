using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Combat;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Shop;
using Rapadura.Gameplay.World.Map;
using Rapadura.UI.WorldMap;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>
    /// A keeper standing at their shop front in a walkable city: a trigger sphere (the interact range, from the layout)
    /// around the figure. Stepping in opens the keeper's counter through the same <see cref="CityShopService"/> and
    /// <see cref="ShopManager"/> as the hub (today's rotating stock, freguesia, the house depot), or, for a resident without
    /// a counter, shows their line for the cousin in play. Same trigger shape as <c>NpcInteractable</c>; <see cref="Interact"/>
    /// is public for a future tap-to-talk. Uses resident ids, never display names.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CityResidentPost : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        /// <summary>Off in PlayMode walks that only check reachability (no window should pop and pause the walk).</summary>
        public static bool AutoInteract { get; set; } = true;

        [SerializeField] private string _cityId;
        [SerializeField] private string _residentId;
        [SerializeField] private string _shopId;
        [SerializeField] private string _lotId;

        public string CityId => _cityId;
        public string ResidentId => _residentId;
        public string ShopId => _shopId;
        public string LotId => _lotId;
        public bool PlayerInRange { get; private set; }

        public void Configure(string cityId, string lotId, string residentId, string shopId)
        {
            _cityId = cityId;
            _lotId = lotId;
            _residentId = residentId;
            _shopId = shopId;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PlayerTag))
            {
                return;
            }

            PlayerInRange = true;
            if (AutoInteract)
            {
                Interact();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(PlayerTag))
            {
                PlayerInRange = false;
            }
        }

        /// <summary>Opens the counter (or says the line). True when something opened.</summary>
        public bool Interact()
        {
            if (!string.IsNullOrEmpty(_shopId) && OpenShop())
            {
                return true;
            }

            Speak();
            return false;
        }

        private bool OpenShop()
        {
            if (!ServiceLocator.TryGet(out ShopManager shops))
            {
                return false;
            }

            GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
            InventoryManager inventory = player != null ? player.GetComponent<InventoryManager>() : null;
            if (inventory == null)
            {
                return false;
            }

            ServiceLocator.TryGet(out WorldMapProgress map);
            ShopDefinition shop = CityShopService.Resolve(_shopId, CityProgress.ResolveShared(), shops, map);
            if (shop == null)
            {
                return false;
            }

            if (shops.IsOpen)
            {
                shops.CloseShop();
            }

            shops.OpenShop(shop, inventory);
            return true;
        }

        private void Speak()
        {
            CityResidentSpec resident = FindResident();
            if (resident == null)
            {
                return;
            }

            ServiceLocator.TryGet(out LocalizationManager localization);
            CharacterId? hero = ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.HasSelection ? selection.Selected : null;
            CombatFxManager.SpawnLootText(transform.position + Vector3.up * 2.3f, CityTexts.ResidentSpeech(localization, resident, hero));
        }

        private CityResidentSpec FindResident()
        {
            if (string.IsNullOrEmpty(_residentId))
            {
                return null;
            }

            if (CityCatalog.Mascate.ResidentId == _residentId)
            {
                return CityCatalog.Mascate;
            }

            CitySpec city = CityCatalog.Get(_cityId);
            return city?.Residents != null ? System.Linq.Enumerable.FirstOrDefault(city.Residents, r => r.ResidentId == _residentId) : null;
        }
    }
}
