using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.World.Cities;
using Rapadura.UI.Secrets;
using UnityEngine;

namespace Rapadura.UI.WorldMap
{
    /// <summary>
    /// Toasts of the themed-shop purchases (<see cref="CityShopPurchaseAppliedEvent"/>, Docs/Cities.md §4): the Bornal de
    /// couro growing the Mochila, a house item sent to the depot and a new freguesia tier. Forwards to the existing queued
    /// toast (<see cref="SecretToastController"/>), like <see cref="Rapadura.UI.Housing.HouseGrantToastRelay"/>.
    /// Self-installing static listener, alive for the whole play session.
    /// </summary>
    public static class CityShopToastRelay
    {
        private static bool _subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (_subscribed) return;
            _subscribed = true;
            EventBus.Subscribe<CityShopPurchaseAppliedEvent>(OnApplied);
        }

        /// <summary>The toast for one effect, or null when it says nothing (pure, tested).</summary>
        public static SecretToastController.Message For(CityShopPurchaseAppliedEvent evt, LocalizationManager l)
        {
            string body;
            switch (evt.Effect)
            {
                case CityShopPurchaseEffect.BagExpanded:
                    body = CityTexts.Format(l, CityShopTexts.BornalDone, evt.Value);
                    break;
                case CityShopPurchaseEffect.BagAlreadyFull:
                    body = CityTexts.Get(l, CityShopTexts.BornalAlready);
                    break;
                case CityShopPurchaseEffect.HouseItemToDepot:
                    body = CityTexts.Format(l, CityShopTexts.HouseItemSent, HouseItemTexts.Name(l, evt.ItemId));
                    break;
                case CityShopPurchaseEffect.ReputationUp:
                    body = CityTexts.Format(l, CityShopTexts.ReputationUp, CityTexts.ShopName(l, evt.ShopId),
                        CityTexts.Get(l, ShopReputation.TierNameKey(evt.Value)));
                    break;
                default:
                    return null;
            }

            return new SecretToastController.Message { Title = CityTexts.ShopName(l, evt.ShopId), Body = body };
        }

        private static void OnApplied(CityShopPurchaseAppliedEvent evt)
        {
            SecretToastController toast = Object.FindFirstObjectByType<SecretToastController>();
            if (toast == null) return;
            LocalizationManager l = ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;
            SecretToastController.Message message = For(evt, l);
            if (message != null) toast.Enqueue(message);
        }
    }
}
