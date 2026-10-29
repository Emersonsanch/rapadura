using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Accessibility;
using Rapadura.Core.Localization;

namespace Rapadura.UI.Menus
{
    /// <summary>One row of the "Controles" tab: an action (+ optional composite part) and its label key.</summary>
    public readonly struct RebindRowDefinition
    {
        public readonly string ActionName;
        /// <summary>Composite part name (e.g. "up" of Move's WASD composite), or null for a plain action binding.</summary>
        public readonly string PartName;
        public readonly string LabelKey;
        /// <summary>
        /// The keyboard column is shown but can't be rebound (see Pause: Esc is also the rebind
        /// cancel key and the universal "back" key, so moving it could lock the player out of menus).
        /// </summary>
        public readonly bool KeyboardLocked;

        public RebindRowDefinition(string actionName, string partName, string labelKey, bool keyboardLocked = false)
        {
            ActionName = actionName;
            PartName = partName;
            LabelKey = labelKey;
            KeyboardLocked = keyboardLocked;
        }
    }

    /// <summary>
    /// Localization keys (en + pt-BR) and row definitions for <see cref="SettingsMenuController"/>'s
    /// "Controles" and "Acessibilidade" tabs, following the same self-contained pattern as
    /// <c>TooltipLocalization</c>: entries are merged into the live <see cref="LocalizationManager"/>
    /// on first use, and <see cref="Get"/> falls back to the pt-BR text when no manager is
    /// registered — pt-BR because the rest of SettingsMenuView.uxml is authored in Portuguese, so
    /// the screen never mixes languages in that fallback case.
    /// </summary>
    public static class SettingsMenuTexts
    {
        public const string StatusPressKey = "ui.settings.controls.status.press_key";
        public const string StatusPressButton = "ui.settings.controls.status.press_button";
        public const string StatusCancelled = "ui.settings.controls.status.cancelled";
        public const string StatusSaved = "ui.settings.controls.status.saved";
        public const string StatusConflict = "ui.settings.controls.status.conflict";
        public const string StatusResetAll = "ui.settings.controls.status.reset_all";
        public const string StatusUnavailable = "ui.settings.controls.status.unavailable";
        public const string HeaderAction = "ui.settings.controls.header.action";
        public const string HeaderKeyboard = "ui.settings.controls.header.keyboard";
        public const string HeaderGamepad = "ui.settings.controls.header.gamepad";
        public const string ButtonReset = "ui.settings.controls.reset";
        public const string ButtonResetAll = "ui.settings.controls.reset_all";
        public const string ButtonCancel = "ui.settings.controls.cancel";
        public const string Waiting = "ui.settings.controls.waiting";
        public const string ColorblindPrefix = "ui.settings.accessibility.colorblind.";
        public const string HighFpsCombat = "ui.settings.graphics.high_fps_combat";
        /// <summary>Prefix + <see cref="Slug"/> of a Unity QualitySettings level name ("Medium" → "...quality_level.medium").</summary>
        public const string QualityLevelPrefix = "ui.settings.graphics.quality_level.";
        /// <summary>Prefix + <see cref="Slug"/> of an Input System control display name ("Left Stick Press").</summary>
        public const string BindingNamePrefix = "ui.settings.controls.binding.";
        /// <summary>Prefix + lower-case <see cref="LanguageCode"/> name; each language is shown by its own name (autonym).</summary>
        public const string LanguageNamePrefix = "ui.settings.language.name.";

        // Static SettingsMenuView.uxml text, applied by UiDocumentLocalizer (class "loc" + view-data-key).
        public const string Title = "ui.settings.title";
        public const string Close = "ui.settings.close";
        public const string TabGraphics = "ui.settings.tab.graphics";
        public const string TabAudio = "ui.settings.tab.audio";
        public const string TabControls = "ui.settings.tab.controls";
        public const string TabLanguage = "ui.settings.tab.language";
        public const string TabAccessibility = "ui.settings.tab.accessibility";
        public const string GraphicsQuality = "ui.settings.graphics.quality";
        public const string GraphicsFullscreen = "ui.settings.graphics.fullscreen";
        public const string AudioMaster = "ui.settings.audio.master";
        public const string AudioMusic = "ui.settings.audio.music";
        public const string AudioSfx = "ui.settings.audio.sfx";
        public const string AudioUi = "ui.settings.audio.ui";
        public const string LanguageLabel = "ui.settings.language.label";
        public const string AccessibilityFontScale = "ui.settings.accessibility.font_scale";
        public const string AccessibilityHighContrast = "ui.settings.accessibility.high_contrast";
        public const string AccessibilityColorblind = "ui.settings.accessibility.colorblind";
        public const string AccessibilityScreenShake = "ui.settings.accessibility.screen_shake";
        public const string AccessibilityReduceFlash = "ui.settings.accessibility.reduce_flash";
        public const string AccessibilityReduceMotion = "ui.settings.accessibility.reduce_motion";

        /// <summary>Rows shown in the "Controles" tab, in display order.</summary>
        public static readonly IReadOnlyList<RebindRowDefinition> Rows = new[]
        {
            new RebindRowDefinition("Move", "up", "ui.settings.controls.action.move_up"),
            new RebindRowDefinition("Move", "down", "ui.settings.controls.action.move_down"),
            new RebindRowDefinition("Move", "left", "ui.settings.controls.action.move_left"),
            new RebindRowDefinition("Move", "right", "ui.settings.controls.action.move_right"),
            new RebindRowDefinition("Move", null, "ui.settings.controls.action.move_stick"),
            new RebindRowDefinition("Jump", null, "ui.settings.controls.action.jump"),
            new RebindRowDefinition("Run", null, "ui.settings.controls.action.run"),
            new RebindRowDefinition("Crouch", null, "ui.settings.controls.action.crouch"),
            new RebindRowDefinition("Interact", null, "ui.settings.controls.action.interact"),
            new RebindRowDefinition("Attack", null, "ui.settings.controls.action.attack"),
            new RebindRowDefinition("Skill1", null, "ui.settings.controls.action.skill1"),
            new RebindRowDefinition("Skill2", null, "ui.settings.controls.action.skill2"),
            new RebindRowDefinition("Skill3", null, "ui.settings.controls.action.skill3"),
            new RebindRowDefinition("Skill4", null, "ui.settings.controls.action.skill4"),
            new RebindRowDefinition("Skill5", null, "ui.settings.controls.action.skill5"),
            new RebindRowDefinition("QuestLog", null, "ui.settings.controls.action.quest_log"),
            new RebindRowDefinition("Inventory", null, "ui.settings.controls.action.inventory"),
            new RebindRowDefinition("SkillTree", null, "ui.settings.controls.action.skill_tree"),
            new RebindRowDefinition("CharacterSheet", null, "ui.settings.controls.action.character_sheet"),
            // Keyboard Pause stays on Esc: Esc is InputRebindManager.DefaultCancelPath (and the
            // SettingsMenuController.Update backup cancel), so a rebind of Pause's keyboard binding
            // could never be confirmed with Esc anyway, and moving Pause off Esc would leave Esc as
            // "cancel" in Settings but not "pause/back" in gameplay. Gamepad Start stays rebindable.
            new RebindRowDefinition("Pause", null, "ui.settings.controls.action.pause", keyboardLocked: true),
        };

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry("ui.settings.controls.action.move_up", "Move up", "Mover para cima"),
            new LocalizationEntry("ui.settings.controls.action.move_down", "Move down", "Mover para baixo"),
            new LocalizationEntry("ui.settings.controls.action.move_left", "Move left", "Mover para a esquerda"),
            new LocalizationEntry("ui.settings.controls.action.move_right", "Move right", "Mover para a direita"),
            new LocalizationEntry("ui.settings.controls.action.move_stick", "Move (stick)", "Mover (analógico)"),
            new LocalizationEntry("ui.settings.controls.action.jump", "Jump", "Pular"),
            new LocalizationEntry("ui.settings.controls.action.run", "Run", "Correr"),
            new LocalizationEntry("ui.settings.controls.action.crouch", "Crouch", "Agachar"),
            new LocalizationEntry("ui.settings.controls.action.interact", "Interact", "Interagir"),
            new LocalizationEntry("ui.settings.controls.action.attack", "Attack", "Atacar"),
            new LocalizationEntry("ui.settings.controls.action.skill1", "Skill 1", "Habilidade 1"),
            new LocalizationEntry("ui.settings.controls.action.skill2", "Skill 2", "Habilidade 2"),
            new LocalizationEntry("ui.settings.controls.action.skill3", "Skill 3", "Habilidade 3"),
            new LocalizationEntry("ui.settings.controls.action.skill4", "Skill 4", "Habilidade 4"),
            new LocalizationEntry("ui.settings.controls.action.skill5", "Skill 5", "Habilidade 5"),
            new LocalizationEntry("ui.settings.controls.action.quest_log", "Quest log", "Missões"),
            new LocalizationEntry("ui.settings.controls.action.inventory", "Inventory", "Mochila"),
            new LocalizationEntry("ui.settings.controls.action.skill_tree", "Skill tree", "Árvore de habilidades"),
            new LocalizationEntry("ui.settings.controls.action.character_sheet", "Character sheet", "Ficha do personagem"),
            new LocalizationEntry("ui.settings.controls.action.pause", "Pause / back", "Pausar / voltar"),

            new LocalizationEntry(HeaderAction, "Action", "Ação"),
            new LocalizationEntry(HeaderKeyboard, "Keyboard", "Teclado"),
            new LocalizationEntry(HeaderGamepad, "Gamepad", "Controle"),
            new LocalizationEntry(ButtonReset, "Reset", "Restaurar"),
            new LocalizationEntry(ButtonResetAll, "Reset all", "Restaurar tudo"),
            new LocalizationEntry(ButtonCancel, "Cancel", "Cancelar"),
            new LocalizationEntry(Waiting, "...", "..."),

            new LocalizationEntry(StatusPressKey, "Press a key for \"{0}\" (Esc cancels).", "Pressione uma tecla para \"{0}\" (Esc cancela)."),
            new LocalizationEntry(StatusPressButton, "Press a gamepad button for \"{0}\" (Esc cancels).", "Pressione um botão do controle para \"{0}\" (Esc cancela)."),
            new LocalizationEntry(StatusCancelled, "Rebind cancelled.", "Remapeamento cancelado."),
            new LocalizationEntry(StatusSaved, "\"{0}\" is now {1}.", "\"{0}\" agora é {1}."),
            new LocalizationEntry(StatusConflict, "Warning: {0} is also used by {1}.", "Atenção: {0} também é usado por {1}."),
            new LocalizationEntry(StatusResetAll, "All controls restored to default.", "Todos os controles foram restaurados."),
            new LocalizationEntry(StatusUnavailable, "Controls unavailable (no input actions found).", "Controles indisponíveis (nenhum mapa de entrada encontrado)."),

            new LocalizationEntry(HighFpsCombat, "60 FPS in combat", "60 FPS em combate"),

            new LocalizationEntry(ColorblindPrefix + "none", "Off", "Desligado"),
            new LocalizationEntry(ColorblindPrefix + "protanopia", "Protanopia (red)", "Protanopia (vermelho)"),
            new LocalizationEntry(ColorblindPrefix + "deuteranopia", "Deuteranopia (green)", "Deuteranopia (verde)"),
            new LocalizationEntry(ColorblindPrefix + "tritanopia", "Tritanopia (blue)", "Tritanopia (azul)"),

            new LocalizationEntry(Title, "Settings", "Configurações"),
            new LocalizationEntry(Close, "Close", "Fechar"),
            new LocalizationEntry(TabGraphics, "Graphics", "Gráficos"),
            new LocalizationEntry(TabAudio, "Audio", "Áudio"),
            new LocalizationEntry(TabControls, "Controls", "Controles"),
            new LocalizationEntry(TabLanguage, "Language", "Idioma"),
            new LocalizationEntry(TabAccessibility, "Accessibility", "Acessibilidade"),
            new LocalizationEntry(GraphicsQuality, "Quality", "Qualidade"),
            new LocalizationEntry(GraphicsFullscreen, "Fullscreen", "Tela Cheia"),
            new LocalizationEntry(AudioMaster, "Master volume", "Volume Geral"),
            new LocalizationEntry(AudioMusic, "Music", "Música"),
            new LocalizationEntry(AudioSfx, "Effects", "Efeitos"),
            new LocalizationEntry(AudioUi, "Interface", "Interface"),
            new LocalizationEntry(LanguageLabel, "Language", "Idioma"),
            new LocalizationEntry(AccessibilityFontScale, "Font size", "Tamanho da Fonte"),
            new LocalizationEntry(AccessibilityHighContrast, "High contrast", "Alto Contraste"),
            new LocalizationEntry(AccessibilityColorblind, "Colorblind mode", "Modo Daltonismo"),
            new LocalizationEntry(AccessibilityScreenShake, "Screen shake", "Tremor de Tela"),
            new LocalizationEntry(AccessibilityReduceFlash, "Reduce flashes", "Reduzir Flashes"),
            new LocalizationEntry(AccessibilityReduceMotion, "Reduce motion", "Reduzir Movimento"),
            new LocalizationEntry(LanguageNamePrefix + "en", "English", "English"),
            new LocalizationEntry(LanguageNamePrefix + "ptbr", "Português (Brasil)", "Português (Brasil)"),

            // Unity QualitySettings level names (ProjectSettings/QualitySettings.asset).
            new LocalizationEntry(QualityLevelPrefix + "very_low", "Very Low", "Muito Baixa"),
            new LocalizationEntry(QualityLevelPrefix + "low", "Low", "Baixa"),
            new LocalizationEntry(QualityLevelPrefix + "medium", "Medium", "Média"),
            new LocalizationEntry(QualityLevelPrefix + "high", "High", "Alta"),
            new LocalizationEntry(QualityLevelPrefix + "very_high", "Very High", "Muito Alta"),
            new LocalizationEntry(QualityLevelPrefix + "ultra", "Ultra", "Ultra"),

            // Input System control display names (InputBindingUtility.LocalizeDisplay; whole names
            // first, then each "/" part). Single letters / F-keys are left as the Input System shows them.
            new LocalizationEntry(BindingNamePrefix + "left_stick_press", "Left Stick Press", "Analógico Esq. (L3)"),
            new LocalizationEntry(BindingNamePrefix + "right_stick_press", "Right Stick Press", "Analógico Dir. (R3)"),
            new LocalizationEntry(BindingNamePrefix + "left_stick", "Left Stick", "Analógico Esq."),
            new LocalizationEntry(BindingNamePrefix + "right_stick", "Right Stick", "Analógico Dir."),
            new LocalizationEntry(BindingNamePrefix + "d_pad", "D-Pad", "Direcional"),
            new LocalizationEntry(BindingNamePrefix + "up", "Up", "Cima"),
            new LocalizationEntry(BindingNamePrefix + "down", "Down", "Baixo"),
            new LocalizationEntry(BindingNamePrefix + "left", "Left", "Esquerda"),
            new LocalizationEntry(BindingNamePrefix + "right", "Right", "Direita"),
            new LocalizationEntry(BindingNamePrefix + "left_shoulder", "Left Shoulder", "Bumper Esq. (LB)"),
            new LocalizationEntry(BindingNamePrefix + "right_shoulder", "Right Shoulder", "Bumper Dir. (RB)"),
            new LocalizationEntry(BindingNamePrefix + "left_trigger", "Left Trigger", "Gatilho Esq. (LT)"),
            new LocalizationEntry(BindingNamePrefix + "right_trigger", "Right Trigger", "Gatilho Dir. (RT)"),
            new LocalizationEntry(BindingNamePrefix + "button_south", "Button South", "Botão Sul"),
            new LocalizationEntry(BindingNamePrefix + "button_north", "Button North", "Botão Norte"),
            new LocalizationEntry(BindingNamePrefix + "button_east", "Button East", "Botão Leste"),
            new LocalizationEntry(BindingNamePrefix + "button_west", "Button West", "Botão Oeste"),
            new LocalizationEntry(BindingNamePrefix + "start", "Start", "Start"),
            new LocalizationEntry(BindingNamePrefix + "select", "Select", "Select"),
            new LocalizationEntry(BindingNamePrefix + "space", "Space", "Espaço"),
            new LocalizationEntry(BindingNamePrefix + "enter", "Enter", "Enter"),
            new LocalizationEntry(BindingNamePrefix + "escape", "Escape", "Esc"),
            new LocalizationEntry(BindingNamePrefix + "backspace", "Backspace", "Backspace"),
            new LocalizationEntry(BindingNamePrefix + "left_shift", "Left Shift", "Shift Esq."),
            new LocalizationEntry(BindingNamePrefix + "right_shift", "Right Shift", "Shift Dir."),
            new LocalizationEntry(BindingNamePrefix + "left_ctrl", "Left Ctrl", "Ctrl Esq."),
            new LocalizationEntry(BindingNamePrefix + "right_ctrl", "Right Ctrl", "Ctrl Dir."),
            new LocalizationEntry(BindingNamePrefix + "left_alt", "Left Alt", "Alt Esq."),
            new LocalizationEntry(BindingNamePrefix + "up_arrow", "Up Arrow", "Seta para Cima"),
            new LocalizationEntry(BindingNamePrefix + "down_arrow", "Down Arrow", "Seta para Baixo"),
            new LocalizationEntry(BindingNamePrefix + "left_arrow", "Left Arrow", "Seta Esquerda"),
            new LocalizationEntry(BindingNamePrefix + "right_arrow", "Right Arrow", "Seta Direita"),
            new LocalizationEntry(BindingNamePrefix + "left_button", "Left Button", "Botão Esq. do Mouse"),
            new LocalizationEntry(BindingNamePrefix + "right_button", "Right Button", "Botão Dir. do Mouse"),
            new LocalizationEntry(BindingNamePrefix + "middle_button", "Middle Button", "Botão do Meio"),
            new LocalizationEntry(BindingNamePrefix + "lmb", "LMB", "Botão Esq. do Mouse"),
            new LocalizationEntry(BindingNamePrefix + "rmb", "RMB", "Botão Dir. do Mouse"),
            new LocalizationEntry(BindingNamePrefix + "mmb", "MMB", "Botão do Meio"),
        };

        private static Dictionary<string, string> _fallbackByKey;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has (external tables take precedence). Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(HeaderAction)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        private static LocalizationManager _mergedInto;

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
        public static string Get(LocalizationManager manager, string key)
        {
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

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            string pattern = Get(manager, key);
            try
            {
                return string.Format(pattern, args);
            }
            catch (FormatException)
            {
                return string.Format(GetFallback(key) ?? key, args);
            }
        }

        public static string GetFallback(string key)
        {
            if (_fallbackByKey == null)
            {
                _fallbackByKey = new Dictionary<string, string>();
                foreach (LocalizationEntry entry in Entries)
                {
                    _fallbackByKey[entry.Key] = entry.PtBR;
                }
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : null;
        }

        public static string LanguageNameKey(LanguageCode language)
        {
            return LanguageNamePrefix + language.ToString().ToLowerInvariant();
        }

        /// <summary>Display name of <paramref name="language"/> in that language ("English", "Português (Brasil)").</summary>
        public static string LanguageName(LocalizationManager manager, LanguageCode language)
        {
            return Get(manager, LanguageNameKey(language));
        }

        /// <summary>Lower-case key fragment of a display name: letters/digits kept, anything else → "_" (collapsed). "D-Pad" → "d_pad".</summary>
        public static string Slug(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            var builder = new System.Text.StringBuilder(text.Length);
            bool pendingSeparator = false;

            foreach (char c in text.Trim())
            {
                if (char.IsLetterOrDigit(c))
                {
                    if (pendingSeparator && builder.Length > 0)
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(c));
                    pendingSeparator = false;
                }
                else
                {
                    pendingSeparator = true;
                }
            }

            return builder.ToString();
        }

        /// <summary>Localized Unity quality level name ("Medium" → "Média"); unknown names are shown as Unity names them.</summary>
        public static string QualityName(LocalizationManager manager, string unityName)
        {
            return TryGetKnown(manager, QualityLevelPrefix + Slug(unityName)) ?? unityName;
        }

        /// <summary>Localized Input System control name, or null when it isn't one we translate (for <see cref="InputBindingUtility.LocalizeDisplay"/>).</summary>
        public static string BindingName(LocalizationManager manager, string englishName)
        {
            string slug = Slug(englishName);
            return slug.Length == 0 ? null : TryGetKnown(manager, BindingNamePrefix + slug);
        }

        /// <summary><see cref="Get"/> restricted to keys this table (or the live manager) actually has; null otherwise.</summary>
        private static string TryGetKnown(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            return GetFallback(key);
        }

        public static string ColorblindKey(ColorblindMode mode)
        {
            return ColorblindPrefix + mode.ToString().ToLowerInvariant();
        }

        /// <summary>Human label for a conflicting binding: "Interagir", or "Mover para cima" for a Move composite part.</summary>
        public static string DescribeConflict(LocalizationManager manager, BindingConflict conflict)
        {
            foreach (RebindRowDefinition row in Rows)
            {
                if (string.Equals(row.ActionName, conflict.ActionName, StringComparison.Ordinal)
                    && string.Equals(row.PartName ?? string.Empty, conflict.PartName ?? string.Empty, StringComparison.OrdinalIgnoreCase))
                {
                    return Get(manager, row.LabelKey);
                }
            }

            return string.IsNullOrEmpty(conflict.PartName) ? conflict.ActionName : $"{conflict.ActionName} ({conflict.PartName})";
        }
    }
}
