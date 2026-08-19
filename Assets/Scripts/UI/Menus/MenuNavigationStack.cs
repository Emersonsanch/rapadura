using System.Collections.Generic;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// Pure, UIDocument-free screen-stack used by menu controllers that have more than one panel
    /// (e.g. Pause -> Settings -> back to Pause, or Settings' own tabs). Screens are identified by
    /// a plain string id chosen by the controller, so this class knows nothing about UI Toolkit and
    /// can be exercised by EditMode tests directly. Controllers are expected to call
    /// <see cref="Open"/>/<see cref="Back"/> and then toggle the matching VisualElement's
    /// <c>display</c> style based on <see cref="Current"/>.
    /// </summary>
    public class MenuNavigationStack
    {
        private readonly Stack<string> _stack = new Stack<string>();

        /// <summary>The screen currently on top of the stack, or null if nothing has been opened yet.</summary>
        public string Current => _stack.Count > 0 ? _stack.Peek() : null;

        public int Depth => _stack.Count;

        /// <summary>Clears the stack and pushes <paramref name="rootScreen"/> as the new base — used when a menu is (re)opened from scratch.</summary>
        public void Reset(string rootScreen)
        {
            _stack.Clear();

            if (!string.IsNullOrEmpty(rootScreen))
            {
                _stack.Push(rootScreen);
            }
        }

        /// <summary>Pushes a new screen on top of the current one (e.g. opening Settings from Pause).</summary>
        public void Open(string screen)
        {
            if (string.IsNullOrEmpty(screen))
            {
                return;
            }

            _stack.Push(screen);
        }

        /// <summary>
        /// Pops the current screen, returning to whatever was below it. Refuses to pop the last
        /// remaining screen (a menu with nothing left on the stack has nothing to close back to —
        /// the controller should treat that as "close the whole menu" instead).
        /// </summary>
        /// <returns>True if a screen was popped, false if already at the root.</returns>
        public bool Back()
        {
            if (_stack.Count <= 1)
            {
                return false;
            }

            _stack.Pop();
            return true;
        }

        public bool IsAtRoot => _stack.Count <= 1;
    }
}
