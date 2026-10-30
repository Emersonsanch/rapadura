using System;
using Rapadura.Core.DI;
using Rapadura.Core.Localization;
using Rapadura.Core.Logging;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Small, non-blocking banner at the bottom of the title screen when the previous session crashed or
    /// logged an uncaught exception (<see cref="CrashReporter.PendingNotice"/>): "O jogo fechou de forma
    /// inesperada da última vez" + "Copiar relatório" (the local report, see <see cref="CrashReportBuilder"/>)
    /// + "Fechar". Built in code and added to the menu root by <see cref="MainMenuController"/>, so the
    /// UXML stays the same for the normal case; it never takes focus, never pauses anything and the menu
    /// buttons keep working around it. Closing it calls <see cref="CrashReporter.DismissNotice"/> (gone
    /// for the rest of the session; the report file itself stays, rotated with the others).
    /// Look: MainMenuView.uss (<see cref="RootClass"/>) on top of the theme's rap-panel/rap-button.
    /// </summary>
    public sealed class CrashNoticeBanner
    {
        public const string RootName = "crash-notice";
        public const string RootClass = "main-menu-crash-notice";
        public const string MessageClass = "main-menu-crash-notice__message";
        public const string HintClass = "main-menu-crash-notice__hint";
        public const string ActionsClass = "main-menu-crash-notice__actions";
        public const string CopyButtonName = "crash-notice-copy-button";
        public const string DismissButtonName = "crash-notice-dismiss-button";

        private VisualElement _root;
        private Button _copyButton;
        private Button _dismissButton;
        private LocalizationManager _localization;

        /// <summary>Which notice to show. Defaults to the live <see cref="CrashReporter"/>; tests inject their own.</summary>
        public Func<CrashNoticeKind> NoticeProvider { get; set; } = () => CrashReporter.PendingNotice;

        /// <summary>The report text "Copiar relatório" copies (null = nothing to copy, banner not shown).</summary>
        public Func<string> ReportProvider { get; set; } = CrashReporter.ReadPendingReport;

        public Action<string> CopyToClipboard { get; set; } = text => GUIUtility.systemCopyBuffer = text;

        public Action OnDismissed { get; set; } = CrashReporter.DismissNotice;

        public bool IsShown => _root != null && _root.parent != null;

        public VisualElement Root => _root;

        public static bool ShouldShow(CrashNoticeKind kind, bool hasReport) => kind != CrashNoticeKind.None && hasReport;

        public static string MessageKey(CrashNoticeKind kind)
        {
            return kind == CrashNoticeKind.ErrorLogged ? CrashNoticeTexts.ErrorLogged : CrashNoticeTexts.UnexpectedClose;
        }

        /// <summary>Adds the banner under <paramref name="parent"/> when there is a notice with a readable report. Returns whether it is shown.</summary>
        public bool Attach(VisualElement parent)
        {
            Detach();

            if (parent == null)
            {
                return false;
            }

            CrashNoticeKind kind = NoticeProvider != null ? NoticeProvider() : CrashNoticeKind.None;
            if (!ShouldShow(kind, !string.IsNullOrEmpty(ReportProvider?.Invoke())))
            {
                return false;
            }

            ServiceLocator.TryGet(out _localization);

            _root = new VisualElement { name = RootName, focusable = false };
            _root.AddToClassList(RootClass);
            _root.AddToClassList("rap-panel");

            var message = new Label(Text(MessageKey(kind)));
            message.AddToClassList(MessageClass);
            _root.Add(message);

            var hint = new Label(Text(CrashNoticeTexts.Hint));
            hint.AddToClassList(HintClass);
            hint.AddToClassList("rap-subtle");
            _root.Add(hint);

            var actions = new VisualElement();
            actions.AddToClassList(ActionsClass);
            _copyButton = new Button(() => CopyReport()) { name = CopyButtonName, text = Text(CrashNoticeTexts.Copy) };
            _copyButton.AddToClassList("rap-button");
            _dismissButton = new Button(Dismiss) { name = DismissButtonName, text = Text(CrashNoticeTexts.Dismiss) };
            _dismissButton.AddToClassList("rap-button");
            actions.Add(_copyButton);
            actions.Add(_dismissButton);
            _root.Add(actions);

            parent.Add(_root);
            return true;
        }

        /// <summary>Removes the banner from the screen without dismissing the notice (the title was disabled).</summary>
        public void Detach()
        {
            _root?.RemoveFromHierarchy();
            _root = null;
            _copyButton = null;
            _dismissButton = null;
        }

        /// <summary>"Copiar relatório". Returns whether something was copied.</summary>
        public bool CopyReport()
        {
            string report = ReportProvider?.Invoke();
            if (string.IsNullOrEmpty(report))
            {
                return false;
            }

            CopyToClipboard?.Invoke(report);
            if (_copyButton != null)
            {
                _copyButton.text = Text(CrashNoticeTexts.Copied);
            }

            return true;
        }

        /// <summary>"Fechar": hides the banner for the rest of the session.</summary>
        public void Dismiss()
        {
            Detach();
            OnDismissed?.Invoke();
        }

        private string Text(string key) => CrashNoticeTexts.Get(_localization, key);
    }
}
