using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Housing;
using Rapadura.Gameplay.Wardrobe;
using Rapadura.UI.Secrets;
using UnityEngine;

namespace Rapadura.UI.Housing
{
    /// <summary>
    /// The "Encomenda para a casa!" toast of H3 (Housing.md §7.2): forwards each <see cref="HouseItemGrantedEvent"/> to the
    /// existing queued toast (and, for H5, each <see cref="WardrobeEntryGrantedEvent"/>) (<see cref="SecretToastController"/>, which bootstraps itself), so no new overlay or sheet is
    /// needed. Self-installing static listener, alive for the whole play session.
    /// </summary>
    public static class HouseGrantToastRelay
    {
        private static bool _subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (_subscribed) return;
            _subscribed = true;
            EventBus.Subscribe<HouseItemGrantedEvent>(OnGranted);
            EventBus.Subscribe<WardrobeEntryGrantedEvent>(OnWardrobeGranted);
        }

        /// <summary>The toast message for one delivery (pure, tested).</summary>
        public static SecretToastController.Message ForGrant(HouseItemGrantedEvent evt, LocalizationManager l)
        {
            string body = HouseSourceTexts.Format(l, HouseSourceTexts.ToastBody, HouseItemTexts.Name(l, evt.ItemId))
                          + "\n" + HouseSourceTexts.Get(l, HouseSourceTexts.ReasonKey(evt.Reason));
            return new SecretToastController.Message { Title = HouseSourceTexts.Get(l, HouseSourceTexts.ToastTitle), Body = body };
        }

        /// <summary>The toast of a new wardrobe look or dye (H5; pure, tested).</summary>
        public static SecretToastController.Message ForWardrobe(WardrobeEntryGrantedEvent evt, LocalizationManager l)
        {
            string title = WardrobeUiTexts.Get(l, evt.IsNewLook ? WardrobeUiTexts.ToastTitleLook : WardrobeUiTexts.ToastTitleDye);
            string body = WardrobeUiTexts.Format(l, WardrobeUiTexts.ToastBody, WardrobeTexts.Name(l, evt.EntryId)) + "\n" + WardrobeTexts.Description(l, evt.EntryId);
            return new SecretToastController.Message { Title = title, Body = body };
        }

        private static void OnGranted(HouseItemGrantedEvent evt) => Enqueue(l => ForGrant(evt, l));

        private static void OnWardrobeGranted(WardrobeEntryGrantedEvent evt) => Enqueue(l => ForWardrobe(evt, l));

        private static void Enqueue(System.Func<LocalizationManager, SecretToastController.Message> build)
        {
            SecretToastController toast = Object.FindFirstObjectByType<SecretToastController>();
            if (toast == null) return;
            LocalizationManager l = ServiceLocator.TryGet(out LocalizationManager manager) ? manager : null;
            toast.Enqueue(build(l));
        }
    }
}
