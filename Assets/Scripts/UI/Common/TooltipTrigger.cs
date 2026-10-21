using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Rapadura.UI.Common
{
    /// <summary>
    /// Wires a UI Toolkit element to a <see cref="TooltipController"/> for every input style the
    /// game supports (touch-first, see Docs/PlatformRequirements.md):
    /// <list type="bullet">
    /// <item><b>Touch / pen</b>: press and hold for <see cref="LongPressMilliseconds"/> shows the
    /// tooltip anchored to the element; releasing hides it and swallows that release so the
    /// element's own click (buy/sell/equip/learn) does <i>not</i> fire. A quick tap still clicks
    /// normally; dragging past <see cref="MoveTolerancePixels"/> (scrolling a list) cancels.</item>
    /// <item><b>Mouse</b>: hovering shows the tooltip following the cursor; leaving hides it.</item>
    /// <item><b>Keyboard/gamepad</b>: focusing the element shows it anchored; losing focus hides it.</item>
    /// </list>
    /// Content is produced lazily by <c>contentProvider</c> at show time, so it always reflects the
    /// current state (e.g. a skill's level after leveling it up).
    /// </summary>
    public static class TooltipTrigger
    {
        public const long LongPressMilliseconds = 450;
        public const float MoveTolerancePixels = 14f;

        public static void Bind(VisualElement element, TooltipController tooltip, Func<TooltipContent> contentProvider)
        {
            if (element == null || tooltip == null || contentProvider == null)
            {
                return;
            }

            var binding = new Binding(element, tooltip, contentProvider);
            binding.Register();
        }

        private sealed class Binding
        {
            private readonly VisualElement _element;
            private readonly TooltipController _tooltip;
            private readonly Func<TooltipContent> _contentProvider;

            private IVisualElementScheduledItem _pendingLongPress;
            private int _pressPointerId = -1;
            private Vector2 _pressPosition;
            private bool _shownByLongPress;
            private bool _pointerPressed;

            public Binding(VisualElement element, TooltipController tooltip, Func<TooltipContent> contentProvider)
            {
                _element = element;
                _tooltip = tooltip;
                _contentProvider = contentProvider;
            }

            public void Register()
            {
                _element.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
                _element.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
                // TrickleDown: run before the Button's Clickable manipulator sees the event.
                _element.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
                _element.RegisterCallback<PointerMoveEvent>(OnPointerMove, TrickleDown.TrickleDown);
                _element.RegisterCallback<PointerUpEvent>(OnPointerUp, TrickleDown.TrickleDown);
                _element.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
                _element.RegisterCallback<FocusInEvent>(OnFocusIn);
                _element.RegisterCallback<FocusOutEvent>(OnFocusOut);
                _element.RegisterCallback<DetachFromPanelEvent>(OnDetached);
            }

            private static bool IsMouse(IPointerEvent evt) => evt.pointerType == UnityEngine.UIElements.PointerType.mouse;

            private void OnPointerEnter(PointerEnterEvent evt)
            {
                if (IsMouse(evt))
                {
                    _tooltip.Show(_contentProvider(), null);
                }
            }

            private void OnPointerLeave(PointerLeaveEvent evt)
            {
                if (IsMouse(evt))
                {
                    _tooltip.Hide();
                }
                else
                {
                    CancelLongPress();
                }
            }

            private void OnPointerDown(PointerDownEvent evt)
            {
                // Clicking/tapping a focusable Button also focuses it; remember that so OnFocusIn
                // doesn't pop an anchored tooltip on every ordinary tap.
                _pointerPressed = true;

                if (IsMouse(evt))
                {
                    return;
                }

                CancelLongPress();
                _pressPointerId = evt.pointerId;
                _pressPosition = evt.position;
                _shownByLongPress = false;
                _pendingLongPress = _element.schedule.Execute(OnLongPressElapsed).StartingIn(LongPressMilliseconds);
            }

            private void OnLongPressElapsed()
            {
                _pendingLongPress = null;
                _shownByLongPress = true;
                _tooltip.Show(_contentProvider(), _element);

                // Releasing capture makes the Button's Clickable drop its pressed state, so lifting the
                // finger after inspecting doesn't count as a click (also stopped in OnPointerUp).
                if (_pressPointerId >= 0 && _element.HasPointerCapture(_pressPointerId))
                {
                    _element.ReleasePointer(_pressPointerId);
                }
            }

            private void OnPointerMove(PointerMoveEvent evt)
            {
                if (_pendingLongPress == null || evt.pointerId != _pressPointerId)
                {
                    return;
                }

                Vector2 delta = (Vector2)evt.position - _pressPosition;
                if (delta.sqrMagnitude > MoveTolerancePixels * MoveTolerancePixels)
                {
                    CancelLongPress();
                }
            }

            private void OnPointerUp(PointerUpEvent evt)
            {
                _pointerPressed = false;

                if (IsMouse(evt))
                {
                    return;
                }

                CancelLongPress();

                if (_shownByLongPress)
                {
                    _shownByLongPress = false;
                    _tooltip.Hide();
                    evt.StopImmediatePropagation();
                }

                _pressPointerId = -1;
            }

            private void OnPointerCancel(PointerCancelEvent evt)
            {
                _pointerPressed = false;
                CancelLongPress();

                if (_shownByLongPress)
                {
                    _shownByLongPress = false;
                    _tooltip.Hide();
                }

                _pressPointerId = -1;
            }

            private void OnFocusIn(FocusInEvent evt)
            {
                if (_pointerPressed)
                {
                    return;
                }

                _tooltip.Show(_contentProvider(), _element);
            }

            private void OnFocusOut(FocusOutEvent evt)
            {
                if (_shownByLongPress)
                {
                    return;
                }

                _tooltip.Hide();
            }

            private void OnDetached(DetachFromPanelEvent evt)
            {
                CancelLongPress();
            }

            private void CancelLongPress()
            {
                _pendingLongPress?.Pause();
                _pendingLongPress = null;
            }
        }
    }
}
