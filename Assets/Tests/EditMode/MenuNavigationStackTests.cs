using NUnit.Framework;
using Rapadura.UI.Menus;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="MenuNavigationStack"/> — the pure screen-stack logic backing
    /// <see cref="SettingsMenuController"/>'s tabs and any future multi-panel menu flow (e.g.
    /// Pause -> Settings -> back to Pause). Deliberately does not touch any <c>UIDocument</c>.
    /// </summary>
    public class MenuNavigationStackTests
    {
        [Test]
        public void Current_BeforeAnyReset_IsNull()
        {
            var stack = new MenuNavigationStack();

            Assert.IsNull(stack.Current);
            Assert.AreEqual(0, stack.Depth);
        }

        [Test]
        public void Reset_SetsCurrentToRootScreen()
        {
            var stack = new MenuNavigationStack();

            stack.Reset("graphics");

            Assert.AreEqual("graphics", stack.Current);
            Assert.AreEqual(1, stack.Depth);
            Assert.IsTrue(stack.IsAtRoot);
        }

        [Test]
        public void Reset_ClearsPreviouslyOpenedScreens()
        {
            var stack = new MenuNavigationStack();
            stack.Reset("graphics");
            stack.Open("audio");

            stack.Reset("controls");

            Assert.AreEqual("controls", stack.Current);
            Assert.AreEqual(1, stack.Depth);
        }

        [Test]
        public void Open_PushesNewScreenOnTop()
        {
            var stack = new MenuNavigationStack();
            stack.Reset("pause");

            stack.Open("settings");

            Assert.AreEqual("settings", stack.Current);
            Assert.AreEqual(2, stack.Depth);
            Assert.IsFalse(stack.IsAtRoot);
        }

        [Test]
        public void Open_NullOrEmpty_DoesNothing()
        {
            var stack = new MenuNavigationStack();
            stack.Reset("pause");

            stack.Open(null);
            stack.Open(string.Empty);

            Assert.AreEqual("pause", stack.Current);
            Assert.AreEqual(1, stack.Depth);
        }

        [Test]
        public void Back_PopsToRoutePreviousScreen_ReturnsTrue()
        {
            var stack = new MenuNavigationStack();
            stack.Reset("pause");
            stack.Open("settings");

            bool popped = stack.Back();

            Assert.IsTrue(popped);
            Assert.AreEqual("pause", stack.Current);
            Assert.AreEqual(1, stack.Depth);
        }

        [Test]
        public void Back_AtRoot_RefusesToPop_ReturnsFalse()
        {
            var stack = new MenuNavigationStack();
            stack.Reset("pause");

            bool popped = stack.Back();

            Assert.IsFalse(popped);
            Assert.AreEqual("pause", stack.Current);
            Assert.AreEqual(1, stack.Depth);
        }

        [Test]
        public void Back_BeforeAnyReset_RefusesToPop()
        {
            var stack = new MenuNavigationStack();

            bool popped = stack.Back();

            Assert.IsFalse(popped);
            Assert.AreEqual(0, stack.Depth);
        }
    }
}
