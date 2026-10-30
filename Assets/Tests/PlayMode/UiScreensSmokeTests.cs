using System.Collections;
using NUnit.Framework;
using Rapadura.Gameplay.Characters;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Rapadura.Tests.PlayMode
{
    /// <summary>
    /// Opens and closes every screen controller the HUD/pause menu can reach in Main.unity through
    /// its public Open/Show/Close/Hide API and checks it is actually on screen in between (its
    /// panel root and every ancestor resolve to a visible display, on an active UIDocument), not
    /// just that an IsOpen flag flipped. One test case per screen so a missing/broken screen
    /// doesn't hide the others. Any exception or error log fails the case. The screen list lives in
    /// <see cref="UiScreenHandles"/> (the soak run reuses it).
    /// </summary>
    [Category("Smoke")]
    public class UiScreensSmokeTests : SmokeTestBase
    {
        [UnityTest]
        [Timeout(120000)]
        public IEnumerator Screen_OpensVisibly_AndCloses(
            [ValueSource(typeof(UiScreenHandles), nameof(UiScreenHandles.All))] string screenId)
        {
            yield return LoadScene(MainSceneName);
            yield return SelectHero(CharacterId.Joaquim);

            UiScreenHandles.Handle screen = UiScreenHandles.Resolve(screenId);
            Assert.IsNotNull(screen, $"No controller for screen '{screenId}' exists in Main.unity — the HUD/pause button for it can only fail silently.");

            UIDocument document = screen.Controller.GetComponent<UIDocument>();
            Assert.IsNotNull(document, $"'{screenId}' controller has no UIDocument.");

            screen.Open();
            yield return WaitFrames(3);

            Assert.IsTrue(screen.IsOpen(), $"'{screenId}' does not report open after Open().");
            Assert.IsTrue(screen.Controller.gameObject.activeInHierarchy, $"'{screenId}' GameObject is inactive — opening it shows nothing.");
            Assert.IsTrue(document.enabled, $"'{screenId}' UIDocument is disabled.");
            Assert.IsNotNull(document.rootVisualElement, $"'{screenId}' UIDocument has no visual tree.");

            VisualElement root = document.rootVisualElement.Q<VisualElement>(screen.RootName);
            Assert.IsNotNull(root, $"'{screenId}' UXML has no '{screen.RootName}' element.");
            AssertVisible(root, screenId);

            screen.Close();
            yield return WaitFrames(3);

            Assert.IsFalse(screen.IsOpen(), $"'{screenId}' still reports open after closing.");
            Assert.AreEqual(DisplayStyle.None, root.resolvedStyle.display, $"'{screenId}' panel is still displayed after closing.");

            // Unexpected Error/Exception logs already fail the test (LogAssert default).
        }

        private static void AssertVisible(VisualElement element, string screenId)
        {
            for (VisualElement e = element; e != null; e = e.parent)
            {
                Assert.AreNotEqual(DisplayStyle.None, e.resolvedStyle.display,
                    $"'{screenId}' is open but '{e.name}' (or an ancestor) resolves to display:none.");
                Assert.AreNotEqual(Visibility.Hidden, e.resolvedStyle.visibility,
                    $"'{screenId}' is open but '{e.name}' (or an ancestor) is visibility:hidden.");
            }
        }
    }
}
