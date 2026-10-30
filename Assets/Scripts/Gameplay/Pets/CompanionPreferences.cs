using System;
using UnityEngine;

namespace Rapadura.Gameplay.Pets
{
    /// <summary>
    /// Player-facing options of the companion (Caramelo, Vó Zefa's mutt), persisted in PlayerPrefs like
    /// <c>TouchControlsPreferences</c> (device-wide: a "no dog on screen" preference is about the player,
    /// not the save slot). Written by the Settings "Acessibilidade" tab (<c>CompanionSettingsSection</c>)
    /// and read by <see cref="CarameloCompanion"/>, which listens to <see cref="Changed"/> to leave or come
    /// back live. On by default.
    /// </summary>
    public static class CompanionPreferences
    {
        public const string CarameloEnabledKey = "Pets.Caramelo.Enabled";
        public const bool CarameloEnabledDefault = true;
        /// <summary>Set once Vó Zefa introduced Caramelo on the world map (first "Fazer carinho").</summary>
        public const string IntroducedKey = "Pets.Caramelo.Introduced";

        /// <summary>Raised after any option changes.</summary>
        public static event Action Changed;

        public static bool CarameloEnabled
        {
            get => PlayerPrefs.GetInt(CarameloEnabledKey, CarameloEnabledDefault ? 1 : 0) != 0;
            set
            {
                if (value == CarameloEnabled)
                {
                    return;
                }

                PlayerPrefs.SetInt(CarameloEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        public static bool Introduced
        {
            get => PlayerPrefs.GetInt(IntroducedKey, 0) != 0;
            set
            {
                PlayerPrefs.SetInt(IntroducedKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Pure rule (EditMode-testable): Caramelo comes along when the option is on and, in a real
        /// encounter, once the prologue was seen (he joins the cousins when they leave the farm). In the
        /// standalone Main.unity sandbox (no encounter) he is always there, for testing.
        /// </summary>
        public static bool ShouldAccompany(bool enabled, bool inEncounter, bool prologueSeen)
        {
            return enabled && (!inEncounter || prologueSeen);
        }

        /// <summary>Pure rule: the "Fazer carinho" button needs the option on and the prologue seen (no dog yet before it).</summary>
        public static bool ShouldOfferCuddle(bool enabled, bool prologueSeen)
        {
            return enabled && prologueSeen;
        }
    }
}
