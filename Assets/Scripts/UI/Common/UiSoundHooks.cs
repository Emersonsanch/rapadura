using Rapadura.Core.Audio;
using Rapadura.Core.DI;
using Rapadura.Core.Logging;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Thin, static convenience layer over <see cref="AudioManager.PlayOneShot(string)"/> for
    /// common interface sounds, so Menu/HUD controllers can call
    /// <c>UiSoundHooks.PlayClick()</c> instead of resolving <see cref="AudioManager"/> from the
    /// <see cref="ServiceLocator"/> and remembering a raw cue id string themselves — mirrors how
    /// <c>SettingsMenuController</c> resolves <see cref="AudioManager"/> via
    /// <c>ServiceLocator.TryGet</c> rather than a hard reference.
    ///
    /// Deliberately never throws and never requires <see cref="AudioManager"/> to be registered:
    /// EditMode tests and any screen instantiated before <c>GameManager.BuildManagers()</c> runs
    /// can call these safely — a missing manager (or a cue id missing from the
    /// <see cref="AudioCueDatabase"/> asset) just logs a warning via <see cref="AudioManager"/>
    /// itself and no-ops, the same graceful-degradation behavior <see cref="AudioManager.PlayOneShot"/>
    /// already has for unknown cue ids.
    ///
    /// CUE ID CONVENTION — this class does not create the <see cref="AudioCueDatabase"/> asset
    /// (an Editor-authored ScriptableObject, out of scope here — see TODO.md Fase 6 note style used
    /// elsewhere for Editor-only assets). Whoever authors that asset should add entries for:
    /// <list type="bullet">
    /// <item><description><c>ui.click</c> — generic button/menu-item activation (category Ui).</description></item>
    /// <item><description><c>ui.hover</c> — pointer/focus enters an interactive element (category Ui, low volume).</description></item>
    /// <item><description><c>ui.error</c> — invalid action / disabled control pressed / form validation failure (category Ui).</description></item>
    /// <item><description><c>ui.confirm</c> — a menu confirms/submits (e.g. "Novo Jogo", "Salvar") (category Ui).</description></item>
    /// <item><description><c>ui.back</c> — closing a menu / cancel navigation (category Ui).</description></item>
    /// </list>
    /// Only <c>ui.click</c>/<c>ui.hover</c>/<c>ui.error</c> have dedicated methods below (per Fase 6
    /// scope); the others are documented here so the same convention extends cleanly later.
    /// </summary>
    public static class UiSoundHooks
    {
        public const string ClickCueId = "ui.click";
        public const string HoverCueId = "ui.hover";
        public const string ErrorCueId = "ui.error";

        /// <summary>Plays the generic UI click/confirm cue, e.g. on a Button's clicked callback.</summary>
        public static void PlayClick() => PlayCue(ClickCueId);

        /// <summary>Plays the generic UI hover/focus cue, e.g. on PointerEnterEvent/FocusInEvent.</summary>
        public static void PlayHover() => PlayCue(HoverCueId);

        /// <summary>Plays the generic UI error cue, e.g. when an action is attempted but currently invalid.</summary>
        public static void PlayError() => PlayCue(ErrorCueId);

        private static void PlayCue(string cueId)
        {
            if (!ServiceLocator.TryGet(out AudioManager audioManager) || audioManager == null)
            {
                GameLogger.Warning("UI", $"UiSoundHooks: AudioManager not registered, skipping cue '{cueId}'.");
                return;
            }

            audioManager.PlayOneShot(cueId);
        }
    }
}
