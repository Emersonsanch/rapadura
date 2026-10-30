using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.UI.PhotoMode
{
    /// <summary>
    /// Localization keys (en + pt-BR) of the photo mode (<see cref="PhotoModeController"/>): panel,
    /// hints, toasts, filter and pose names (<c>PhotoFilterPresets.LabelKey</c>,
    /// <c>PhotoPoseDefinition.LabelKey</c>). Same self-contained pattern as <c>ReturnToTitleTexts</c>:
    /// merged into the live <see cref="LocalizationManager"/> on first use (keys it already has win),
    /// pt-BR fallback when no manager is registered.
    /// </summary>
    public static class PhotoModeTexts
    {
        public const string Title = "ui.photo.title";
        public const string Filter = "ui.photo.filter";
        public const string Pose = "ui.photo.pose";
        public const string Roll = "ui.photo.roll";
        public const string Fov = "ui.photo.fov";
        public const string Save = "ui.photo.save";
        public const string HidePanel = "ui.photo.hide_panel";
        public const string ShowPanel = "ui.photo.show_panel";
        public const string Recenter = "ui.photo.recenter";
        public const string Exit = "ui.photo.exit";
        public const string Previous = "ui.photo.previous";
        public const string Next = "ui.photo.next";
        public const string Saved = "ui.photo.saved";
        public const string SaveFailed = "ui.photo.save_failed";
        public const string FiltersUnavailable = "ui.photo.filters_unavailable";
        public const string HintPointer = "ui.photo.hint.pointer";
        public const string HintTouch = "ui.photo.hint.touch";
        public const string HintGamepad = "ui.photo.hint.gamepad";

        public static readonly List<LocalizationEntry> Entries = new List<LocalizationEntry>
        {
            new LocalizationEntry(Title, "Photo mode", "Modo foto"),
            new LocalizationEntry(Filter, "Filter", "Filtro"),
            new LocalizationEntry(Pose, "Pose", "Pose"),
            new LocalizationEntry(Roll, "Tilt", "Inclinação"),
            new LocalizationEntry(Fov, "Field of view", "Campo de visão"),
            new LocalizationEntry(Save, "Save photo", "Salvar foto"),
            new LocalizationEntry(HidePanel, "Hide panel", "Ocultar painel"),
            new LocalizationEntry(ShowPanel, "Show panel", "Mostrar painel"),
            new LocalizationEntry(Recenter, "Recenter", "Centralizar"),
            new LocalizationEntry(Exit, "Exit", "Sair"),
            new LocalizationEntry(Previous, "‹", "‹"),
            new LocalizationEntry(Next, "›", "›"),
            new LocalizationEntry(Saved, "Photo saved to {0}", "Foto salva em {0}"),
            new LocalizationEntry(SaveFailed, "Couldn't save the photo.", "Não foi possível salvar a foto."),
            new LocalizationEntry(FiltersUnavailable, "Filters unavailable on this device.", "Filtros indisponíveis neste aparelho."),
            new LocalizationEntry(HintPointer,
                "Drag: orbit · Right button: move · Wheel: zoom · WASD/QE: move · Arrows: orbit · Z/C: tilt · F: filter · G: pose · P: photo · H: panel · Esc: exit",
                "Arraste: girar · Botão direito: mover · Roda: zoom · WASD/QE: mover · Setas: girar · Z/C: inclinar · F: filtro · G: pose · P: foto · H: painel · Esc: sair"),
            new LocalizationEntry(HintTouch,
                "Drag: orbit · Pinch: zoom · Two fingers: move",
                "Arraste: girar · Pinça: zoom · Dois dedos: mover"),
            new LocalizationEntry(HintGamepad,
                "Left stick: move · Right stick: orbit · Triggers: zoom · LB/RB: tilt · D-pad: filter/pose · A: photo · Y: panel · B: exit",
                "Analógico esq.: mover · Analógico dir.: girar · Gatilhos: zoom · LB/RB: inclinar · Direcional: filtro/pose · A: foto · Y: painel · B: sair"),

            new LocalizationEntry("ui.photo.filter.none", "None", "Nenhum"),
            new LocalizationEntry("ui.photo.filter.cordel", "Illumination", "Iluminura"),
            new LocalizationEntry("ui.photo.filter.xilogravura", "Paper cut", "Papel recortado"),
            new LocalizationEntry("ui.photo.filter.entardecer", "Sunset", "Entardecer"),
            new LocalizationEntry("ui.photo.filter.sao_joao", "Bema Night", "Noite do Bema"),

            new LocalizationEntry("ui.photo.pose.neutral", "Natural", "Natural"),
            new LocalizationEntry("ui.photo.pose.wave", "Wave", "Acenar"),
            new LocalizationEntry("ui.photo.pose.victory", "Victory", "Vitória"),
            new LocalizationEntry("ui.photo.pose.sit", "Sit", "Sentar"),
            new LocalizationEntry("ui.photo.pose.signature.maithe", "Drawn bow", "Arco tenso"),
            new LocalizationEntry("ui.photo.pose.signature.maria", "Warmth of Light", "Calor da Luz"),
            new LocalizationEntry("ui.photo.pose.signature.joaquim", "Guard of the Roots", "Guarda das Raízes"),
            new LocalizationEntry("ui.photo.pose.signature.icaro", "Light Engine", "Engenho da Luz"),
            new LocalizationEntry("ui.photo.pose.signature.lavine", "Veil of Shadows", "Véu de Sombras"),
        };

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        /// <summary>Merges <see cref="Entries"/> into <paramref name="manager"/>, skipping keys it already has. Safe to call repeatedly.</summary>
        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey(Title)))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        /// <summary>Localized text via <paramref name="manager"/> when available, else the pt-BR fallback, else the key.</summary>
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

        /// <summary><see cref="Get"/> + <see cref="string.Format(string, object[])"/>, tolerant of malformed translations.</summary>
        public static string Format(LocalizationManager manager, string key, params object[] args)
        {
            try
            {
                return string.Format(Get(manager, key), args);
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
    }
}
