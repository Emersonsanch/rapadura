using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Gamepad/keyboard navigation for a full-screen UI Toolkit window (Mochila, Loja, Criação,
    /// Missões, Conquistas, Skills). Built on UI Toolkit's own navigation events, which the scene's
    /// <c>InputSystemUIInputModule</c> already produces from the D-pad/left stick/arrow keys
    /// (<see cref="NavigationMoveEvent"/>), A/Enter (<see cref="NavigationSubmitEvent"/>) and B/Esc
    /// (<see cref="NavigationCancelEvent"/>):
    /// <list type="bullet">
    /// <item><b>Move</b>: spatial — picks the nearest focusable, visible, enabled element of the window
    /// in that direction (<see cref="FocusGridMath.FindNext"/>), so grids, lists and side-by-side panels
    /// all work without per-screen layout code. Focus is trapped inside the window while it's open
    /// (never jumps to the HUD behind it) and the target is scrolled into view.</item>
    /// <item><b>Submit</b>: left to UI Toolkit — a focused <see cref="Button"/> clicks itself. Only when
    /// focus is outside the window (e.g. still on the HUD button that opened it) the press is eaten
    /// and focus moves into the window instead.</item>
    /// <item><b>Cancel</b>: closes the window — deferred by a frame and only if it's still open, because
    /// keyboard Esc is ALSO the HUD's Pause action (<c>HudController.OnPauseShortcut</c> already closes
    /// the open screen through <c>ScreenExclusivityCoordinator</c>); closing it here first would make the
    /// HUD see "nothing open" and pop the pause menu. Gamepad B is only UI Cancel, so it lands here.</item>
    /// </list>
    /// Handlers sit on the panel's root (TrickleDown) rather than on the window, because navigation
    /// events target the focused element — which, right after opening, is often outside the window.
    /// Every handler is a no-op while <c>isOpen</c> is false, so several windows can share one panel.
    ///
    /// Touch-first: tooltips appear on focus (<see cref="TooltipTrigger"/>), so the navigator only
    /// restores/moves focus onto content after a navigation input; a tap resets that mode.
    /// </summary>
    public sealed class FocusNavigator
    {
        // Input modality is global (one player, one device at a time): any pointer press anywhere
        // switches every navigator back to "touch/mouse" mode.
        private static bool _navigationInputActive;

        private readonly VisualElement _root;
        private readonly Func<bool> _isOpen;
        private readonly Action _close;
        private readonly Func<VisualElement> _initialFocus;
        private readonly List<VisualElement> _candidates = new List<VisualElement>();
        private readonly List<Rect> _candidateRects = new List<Rect>();

        private VisualElement _panelRoot;
        private int _lastFocusedIndex = -1;
        private int _cancelFrame = -1;
        private bool _restorePending;

        /// <summary>True after a D-pad/arrow/Tab/Submit press, false again after any pointer press.</summary>
        public static bool NavigationInputActive => _navigationInputActive;

        private FocusNavigator(VisualElement root, Func<bool> isOpen, Action close, Func<VisualElement> initialFocus)
        {
            _root = root;
            _isOpen = isOpen;
            _close = close;
            _initialFocus = initialFocus;
        }

        /// <summary>
        /// Hooks navigation for the window <paramref name="root"/>. <paramref name="close"/> runs on
        /// B/Esc; <paramref name="initialFocus"/> (optional, e.g. the window's close button — something
        /// without a tooltip) is what <see cref="FocusInitial"/> focuses. Returns null for a null root.
        /// </summary>
        public static FocusNavigator Attach(VisualElement root, Func<bool> isOpen, Action close, Func<VisualElement> initialFocus = null)
        {
            if (root == null || isOpen == null)
            {
                return null;
            }

            var navigator = new FocusNavigator(root, isOpen, close, initialFocus);
            navigator.Register();
            return navigator;
        }

        public void Detach()
        {
            _root.UnregisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _root.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            _root.UnregisterCallback<FocusInEvent>(OnFocusIn);
            UnhookPanel();
        }

        /// <summary>
        /// Call when the window opens: focuses the initial element so the D-pad works straight away
        /// (with nothing focused the input module has no target for navigation events). Retried next
        /// frame because a just-shown element may not accept focus before its first layout.
        /// </summary>
        public void FocusInitial()
        {
            _lastFocusedIndex = -1;
            TryFocusInitial();
            _root.schedule.Execute(() =>
            {
                if (_isOpen() && !IsFocusInsideRoot())
                {
                    TryFocusInitial();
                }
            });
        }

        /// <summary>Call when the window closes: drops focus held inside it, so hidden elements don't keep receiving navigation.</summary>
        public void ReleaseFocus()
        {
            _lastFocusedIndex = -1;
            _restorePending = false;

            if (IsFocusInsideRoot())
            {
                (_root.focusController?.focusedElement as VisualElement)?.Blur();
            }
        }

        /// <summary>
        /// Call after rebuilding the window's rows: if the player was navigating with a gamepad and the
        /// focused row was destroyed by the rebuild, focus moves to the element now at the same position
        /// in focus order (equip with A, keep pressing A/moving without losing your place).
        /// </summary>
        public void RestoreFocusAfterRebuild()
        {
            if (!_navigationInputActive || _lastFocusedIndex < 0 || _restorePending)
            {
                return;
            }

            _restorePending = true;
            int index = _lastFocusedIndex;
            _root.schedule.Execute(() =>
            {
                _restorePending = false;

                if (!_isOpen() || IsFocusInsideRoot())
                {
                    return;
                }

                CollectCandidates();

                if (_candidates.Count > 0)
                {
                    FocusElement(_candidates[Mathf.Clamp(index, 0, _candidates.Count - 1)]);
                }
            });
        }

        private void Register()
        {
            _root.RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            _root.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            _root.RegisterCallback<FocusInEvent>(OnFocusIn);
            HookPanel();
        }

        private void OnAttachToPanel(AttachToPanelEvent evt) => HookPanel();
        private void OnDetachFromPanel(DetachFromPanelEvent evt) => UnhookPanel();

        private void HookPanel()
        {
            VisualElement panelRoot = _root.panel?.visualTree;

            if (panelRoot == null || panelRoot == _panelRoot)
            {
                return;
            }

            UnhookPanel();
            _panelRoot = panelRoot;
            _panelRoot.RegisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            _panelRoot.RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit, TrickleDown.TrickleDown);
            _panelRoot.RegisterCallback<NavigationCancelEvent>(OnNavigationCancel, TrickleDown.TrickleDown);
            _panelRoot.RegisterCallback<PointerDownEvent>(OnAnyPointerDown, TrickleDown.TrickleDown);
        }

        private void UnhookPanel()
        {
            if (_panelRoot == null)
            {
                return;
            }

            _panelRoot.UnregisterCallback<NavigationMoveEvent>(OnNavigationMove, TrickleDown.TrickleDown);
            _panelRoot.UnregisterCallback<NavigationSubmitEvent>(OnNavigationSubmit, TrickleDown.TrickleDown);
            _panelRoot.UnregisterCallback<NavigationCancelEvent>(OnNavigationCancel, TrickleDown.TrickleDown);
            _panelRoot.UnregisterCallback<PointerDownEvent>(OnAnyPointerDown, TrickleDown.TrickleDown);
            _panelRoot = null;
        }

        private static void OnAnyPointerDown(PointerDownEvent evt) => _navigationInputActive = false;

        private void OnFocusIn(FocusInEvent evt)
        {
            if (!(evt.target is VisualElement focused))
            {
                return;
            }

            CollectCandidates();
            int index = _candidates.IndexOf(focused);

            if (index >= 0)
            {
                _lastFocusedIndex = index;
            }
        }

        private void OnNavigationMove(NavigationMoveEvent evt)
        {
            if (!_isOpen())
            {
                return;
            }

            if (!TryMapDirection(evt.direction, out FocusDirection direction))
            {
                return;
            }

            _navigationInputActive = true;
            Consume(evt);
            CollectCandidates();

            if (_candidates.Count == 0)
            {
                return;
            }

            VisualElement focused = FocusedElementInsideRoot();

            if (focused == null)
            {
                // Coming from outside (HUD button, nothing focused, a destroyed row): enter the window
                // where the player last was, else at its initial element.
                VisualElement entry = _lastFocusedIndex >= 0 && _lastFocusedIndex < _candidates.Count
                    ? _candidates[_lastFocusedIndex]
                    : ResolveInitialCandidate();
                FocusElement(entry);
                return;
            }

            int currentIndex = _candidates.IndexOf(focused);
            int next = FocusGridMath.FindNext(focused.worldBound, _candidateRects, direction, currentIndex);

            if (next >= 0)
            {
                FocusElement(_candidates[next]);
            }
        }

        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            if (!_isOpen())
            {
                return;
            }

            _navigationInputActive = true;

            if (FocusedElementInsideRoot() != null)
            {
                return; // the focused Button clicks itself
            }

            Consume(evt);
            CollectCandidates();
            FocusElement(ResolveInitialCandidate());
        }

        private void OnNavigationCancel(NavigationCancelEvent evt)
        {
            if (!_isOpen() || _close == null)
            {
                return;
            }

            Consume(evt);

            if (_cancelFrame >= 0)
            {
                return; // already scheduled
            }

            _cancelFrame = Time.frameCount;
            _root.schedule.Execute(DeferredCancel);
        }

        private void DeferredCancel()
        {
            if (Time.frameCount == _cancelFrame)
            {
                // Still the frame the cancel arrived in: the HUD's Esc may not have run yet.
                _root.schedule.Execute(DeferredCancel);
                return;
            }

            _cancelFrame = -1;

            if (_isOpen())
            {
                _close();
            }
        }

        private void Consume(EventBase evt)
        {
            evt.StopPropagation();
            _root.focusController?.IgnoreEvent(evt);
        }

        private VisualElement ResolveInitialCandidate()
        {
            VisualElement initial = _initialFocus?.Invoke();

            if (initial != null && _candidates.Contains(initial))
            {
                return initial;
            }

            return _candidates.Count > 0 ? _candidates[0] : null;
        }

        private void TryFocusInitial()
        {
            VisualElement initial = _initialFocus?.Invoke();

            if (initial != null && initial.canGrabFocus)
            {
                initial.Focus();
                return;
            }

            CollectCandidates();

            if (_candidates.Count > 0)
            {
                _candidates[0].Focus();
            }
        }

        private void FocusElement(VisualElement element)
        {
            if (element == null)
            {
                return;
            }

            element.Focus();

            for (VisualElement parent = element.hierarchy.parent; parent != null && parent != _root.hierarchy.parent; parent = parent.hierarchy.parent)
            {
                if (parent is ScrollView scrollView)
                {
                    scrollView.ScrollTo(element);
                    break;
                }
            }
        }

        private bool IsFocusInsideRoot() => FocusedElementInsideRoot() != null;

        private VisualElement FocusedElementInsideRoot()
        {
            if (_root.focusController?.focusedElement is VisualElement focused && focused.panel != null && IsInsideRoot(focused))
            {
                return focused;
            }

            return null;
        }

        private bool IsInsideRoot(VisualElement element)
        {
            for (VisualElement current = element; current != null; current = current.hierarchy.parent)
            {
                if (current == _root)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Focusable, enabled, shown descendants of the window in tree (= focus) order, with their world rects.</summary>
        private void CollectCandidates()
        {
            _candidates.Clear();
            _candidateRects.Clear();
            Collect(_root);
        }

        private void Collect(VisualElement element)
        {
            if (!IsShown(element))
            {
                return;
            }

            if (element != _root && element.focusable && element.tabIndex >= 0 && element.enabledInHierarchy && element.canGrabFocus)
            {
                Rect bounds = element.worldBound;

                if (bounds.width > 0f && bounds.height > 0f)
                {
                    _candidates.Add(element);
                    _candidateRects.Add(bounds);
                }
            }

            int childCount = element.hierarchy.childCount;

            for (int i = 0; i < childCount; i++)
            {
                Collect(element.hierarchy[i]);
            }
        }

        private static bool IsShown(VisualElement element)
        {
            IResolvedStyle style = element.resolvedStyle;
            return style.display != DisplayStyle.None && style.visibility == Visibility.Visible;
        }

        private static bool TryMapDirection(NavigationMoveEvent.Direction source, out FocusDirection direction)
        {
            switch (source)
            {
                case NavigationMoveEvent.Direction.Up: direction = FocusDirection.Up; return true;
                case NavigationMoveEvent.Direction.Down: direction = FocusDirection.Down; return true;
                case NavigationMoveEvent.Direction.Left: direction = FocusDirection.Left; return true;
                case NavigationMoveEvent.Direction.Right: direction = FocusDirection.Right; return true;
                case NavigationMoveEvent.Direction.Next: direction = FocusDirection.Next; return true;
                case NavigationMoveEvent.Direction.Previous: direction = FocusDirection.Previous; return true;
                default: direction = FocusDirection.Up; return false;
            }
        }
    }
}
