using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.World.Cities;
using Rapadura.Save;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// The city hub's "Oratório" service (<see cref="VillageHubButton"/> → Serviços): the button label with today's
    /// price of the Bênção da Romaria and the purchase itself, paid from the carried bag
    /// (<see cref="RomariaOratory.TryBuy"/>). The keeper answers on the panel's speech line.
    /// </summary>
    public static class RomariaOratoryHub
    {
        private const string PlayerStatsKey = "PlayerStats";

        /// <summary>The hero's level from the carried PlayerStats (the map has no live player); 1 when unknown.</summary>
        public static int CarriedLevel()
        {
            return ServiceLocator.TryGet(out SaveManager saveManager) && saveManager.TryGetCarriedJson(PlayerStatsKey, out string json)
                ? WorldMapHudPresenter.ParseLevel(json)
                : 1;
        }

        public static string KeeperName(LocalizationManager localization, OratorySpec oratory)
        {
            if (oratory == null)
            {
                return string.Empty;
            }

            CityResidentSpec keeper = oratory.KeeperResidentId == null
                ? null
                : CityCatalog.AllResidents().FirstOrDefaultById(oratory.KeeperResidentId);
            return keeper != null
                ? CityTexts.ResidentName(localization, keeper)
                : DeathProtectionTexts.Get(localization, DeathProtectionTexts.DonaFeName);
        }

        public static string ButtonText(LocalizationManager localization, CitySpec city)
        {
            OratorySpec oratory = city != null ? RomariaOratory.ForCity(city.CityId) : null;
            if (oratory == null)
            {
                return string.Empty;
            }

            string name = DeathProtectionTexts.Get(localization, oratory.NameKey);
            return DeathProtectionState.ResolveShared().IsBlessed
                ? DeathProtectionTexts.Format(localization, DeathProtectionTexts.HubButtonBlessed, name)
                : DeathProtectionTexts.Format(localization, DeathProtectionTexts.HubButton, name, RomariaBlessingRules.Price(CarriedLevel()));
        }

        /// <summary>Tries to buy the blessing from <paramref name="bag"/>; returns the keeper's answer. Auto-saves on success.</summary>
        public static string Buy(CitySpec city, InventoryManager bag, LocalizationManager localization)
        {
            OratorySpec oratory = city != null ? RomariaOratory.ForCity(city.CityId) : null;
            if (oratory == null)
            {
                return null;
            }

            int level = CarriedLevel();
            string keeper = KeeperName(localization, oratory);
            switch (RomariaOratory.TryBuy(bag, DeathProtectionState.ResolveShared(), level))
            {
                case BlessingPurchase.Ok:
                    if (ServiceLocator.TryGet(out SaveManager saveManager))
                    {
                        saveManager.AutoSave();
                    }

                    return DeathProtectionTexts.Format(localization, DeathProtectionTexts.Blessed, keeper);
                case BlessingPurchase.AlreadyBlessed:
                    return DeathProtectionTexts.Format(localization, DeathProtectionTexts.AlreadyBlessed, keeper);
                case BlessingPurchase.NotEnoughGold:
                    return DeathProtectionTexts.Format(localization, DeathProtectionTexts.NoGold, keeper, RomariaBlessingRules.Price(level));
                default:
                    return null;
            }
        }

        private static CityResidentSpec FirstOrDefaultById(this System.Collections.Generic.IEnumerable<CityResidentSpec> residents, string id)
        {
            foreach (CityResidentSpec resident in residents)
            {
                if (resident.ResidentId == id)
                {
                    return resident;
                }
            }

            return null;
        }
    }
}
