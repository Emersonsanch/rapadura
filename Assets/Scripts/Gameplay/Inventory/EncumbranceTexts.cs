using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Inventory
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the carry load: the backpack's load bar, the item tooltip's weight line, the
    /// HUD load chip and the "heavy load" toasts (<see cref="EncumbranceRules"/>). Same self-contained pattern as the
    /// other *Texts helpers: merged into the live <see cref="LocalizationManager"/> on first use (keys it already has
    /// win), pt-BR fallback when no manager is registered.
    /// </summary>
    public static class EncumbranceTexts
    {
        /// <summary>{0} = carried weight, {1} = capacity.</summary>
        public const string BagLoad = "ui.load.bag";
        /// <summary>{0} = unit weight.</summary>
        public const string TooltipWeight = "ui.load.tooltip_weight";
        /// <summary>{0} = unit weight, {1} = quantity, {2} = stack weight.</summary>
        public const string TooltipStackWeight = "ui.load.tooltip_stack_weight";
        public const string TooltipWeightless = "ui.load.tooltip_weightless";
        /// <summary>{0} = carried weight, {1} = ride capacity.</summary>
        public const string HudMountLoad = "ui.load.hud_mount";
        /// <summary>{0} = carried weight, {1} = hero capacity.</summary>
        public const string HudHeroLoad = "ui.load.hud_hero";

        /// <summary>{0} = mount name.</summary>
        public const string ToastMountHeavy = "ui.load.toast_mount_heavy";
        /// <summary>{0} = mount name.</summary>
        public const string ToastMountOverloaded = "ui.load.toast_mount_overloaded";
        /// <summary>{0} = mount name.</summary>
        public const string ToastMountLight = "ui.load.toast_mount_light";
        public const string ToastHeroOverloaded = "ui.load.toast_hero_overloaded";
        public const string ToastHeroLight = "ui.load.toast_hero_light";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(BagLoad, "Load: {0}/{1}", "Carga: {0}/{1}"),
            new LocalizationEntry(TooltipWeight, "Weight: {0}", "Peso: {0}"),
            new LocalizationEntry(TooltipStackWeight, "Weight: {0} each ({1} = {2})", "Peso: {0} cada ({1} = {2})"),
            new LocalizationEntry(TooltipWeightless, "Weightless", "Não pesa nada"),
            new LocalizationEntry(HudMountLoad, "Saddlebags {0}/{1}", "Alforjes {0}/{1}"),
            new LocalizationEntry(HudHeroLoad, "Load {0}/{1}", "Carga {0}/{1}"),

            new LocalizationEntry(ToastMountHeavy, "{0} feels the weight of the saddlebags: the ride slows down.",
                "{0} sente o peso dos alforjes: a montaria anda mais devagar."),
            new LocalizationEntry(ToastMountOverloaded, "Too much load! {0} can only plod along, like a caravan in the dunes. No gallop.",
                "Carga demais! {0} só anda a passo, como caravana na duna. Nada de galope."),
            new LocalizationEntry(ToastMountLight, "Lighter saddlebags: {0} picks up the pace again.",
                "Alforjes mais leves: {0} volta a andar ligeiro."),
            new LocalizationEntry(ToastHeroOverloaded, "The backpack is too heavy: you walk slower. Leave something in the chest or ride a pack animal.",
                "A mochila pesou demais: vocês andam mais devagar. Deixem algo no baú ou montem um animal de carga."),
            new LocalizationEntry(ToastHeroLight, "The backpack is light again.", "A mochila ficou leve de novo."),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Entries[0].Key)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key) ?? key;
        }

        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (System.FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        /// <summary>
        /// The toast for entering <paramref name="band"/> ({0} = mount name when mounted); null when there is nothing to
        /// say. <paramref name="lighter"/> = the band went down without a mount/dismount.
        /// </summary>
        public static string ToastKey(LoadBand band, bool mounted, bool lighter)
        {
            switch (band)
            {
                case LoadBand.Overloaded:
                    return mounted ? ToastMountOverloaded : ToastHeroOverloaded;
                case LoadBand.Heavy:
                    return mounted ? ToastMountHeavy : null;
                default:
                    return lighter ? (mounted ? ToastMountLight : ToastHeroLight) : null;
            }
        }

        /// <summary>Weight as shown in the UI: up to one decimal, invariant ("12", "1.5", "0.1").</summary>
        public static string Number(float value)
        {
            return value.ToString(value > 0f && value < 0.1f ? "0.##" : "0.#", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string GetFallback(string key)
        {
            if (key == null)
            {
                return null;
            }

            if (_fallbackByKey == null)
            {
                var table = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    table[entry.Key] = entry.PtBR;
                }

                _fallbackByKey = table;
            }

            return _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }
    }
}
