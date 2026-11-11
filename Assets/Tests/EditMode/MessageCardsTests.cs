using NUnit.Framework;
using Rapadura.Core.Systems;
using Rapadura.UI.Common;
using Rapadura.UI.Tutorial;
using UnityEngine.UIElements;

namespace Rapadura.Tests
{
    /// <summary>
    /// The one rule for message cards (MessageCards / MessageCardStack): a modal holds non-critical cards, and two cards
    /// in the top-centre lane never share a spot (the later / lower one moves down below the other).
    /// </summary>
    public class MessageCardsTests
    {
        private const float Now = 10f;

        [SetUp]
        public void SetUp() => GameplayPause.Reset();

        [TearDown]
        public void TearDown() => GameplayPause.Reset();

        [Test]
        public void ModalOpen_HoldsNonCriticalCards_ButNotCriticalOnes()
        {
            Assert.IsTrue(MessageCards.ShouldHold(modalOpen: true));
            Assert.IsFalse(MessageCards.ShouldHold(modalOpen: false));
            Assert.IsFalse(MessageCards.ShouldHold(modalOpen: true, critical: true), "A critical card still shows over a modal.");
        }

        [Test]
        public void ModalOpen_FollowsGameplayPause_AndIgnoresTheCardsOwnPause()
        {
            var travelWindow = new object();
            var hint = new object();
            Assert.IsFalse(MessageCards.IsModalOpen);

            GameplayPause.Acquire(travelWindow);
            Assert.IsTrue(MessageCards.IsModalOpen, "Barcas de Luz (or any window holding a GameplayPause) counts as a modal.");
            Assert.IsTrue(MessageCards.IsModalOpenFor(hint));
            GameplayPause.Release(travelWindow);

            GameplayPause.Acquire(hint);
            Assert.IsFalse(MessageCards.IsModalOpenFor(hint), "A pausing tutorial step does not hold itself.");
            GameplayPause.Release(hint);
            Assert.IsFalse(MessageCards.IsModalOpen);
        }

        [Test]
        public void ModalOpen_HoldsTheTutorialHint()
        {
            var travelWindow = new object();
            var hint = new object();
            GameplayPause.Acquire(travelWindow);
            Assert.IsTrue(TutorialHintController.ShouldDefer(MessageCards.IsModalOpenFor(hint)), "\"Nova missão!\" waits behind the map window.");
            GameplayPause.Release(travelWindow);
            Assert.IsFalse(TutorialHintController.ShouldDefer(MessageCards.IsModalOpenFor(hint)), "...and comes back when it closes.");
        }

        [Test]
        public void LoneCard_StaysWhereItsUssPutsIt()
        {
            var lane = new MessageCardStack();
            lane.Report(MessageCards.TutorialHintId, MessageCards.TutorialHintPriority, 168f, 90f, Now);
            Assert.AreEqual(168f, lane.TopFor(MessageCards.TutorialHintId, Now), 1e-3f);
            Assert.AreEqual(0f, lane.OffsetFor(MessageCards.TutorialHintId, Now), 1e-3f);
        }

        [Test]
        public void OverlappingCards_ArePushedDownBelowTheOneAbove()
        {
            var lane = new MessageCardStack();
            lane.Report(MessageCards.AreaBannerId, MessageCards.AreaBannerPriority, 108f, 80f, Now);
            lane.Report(MessageCards.AchievementId, MessageCards.AchievementPriority, 112f, 84f, Now);
            lane.Report(MessageCards.TutorialHintId, MessageCards.TutorialHintPriority, 168f, 90f, Now);

            Assert.AreEqual(108f, lane.TopFor(MessageCards.AreaBannerId, Now), 1e-3f, "The first card keeps its place.");
            float achievementTop = lane.TopFor(MessageCards.AchievementId, Now);
            Assert.AreEqual(108f + 80f + MessageCardStack.Gap, achievementTop, 1e-3f);
            Assert.AreEqual(achievementTop + 84f + MessageCardStack.Gap, lane.TopFor(MessageCards.TutorialHintId, Now), 1e-3f,
                "The tip goes below both instead of covering them.");
        }

        [Test]
        public void SameSpot_LowerPriorityKeepsIt_ThenFirstReported()
        {
            var lane = new MessageCardStack();
            lane.Report("b", 2, 100f, 40f, Now);
            lane.Report("a", 1, 100f, 40f, Now);
            lane.Report("c", 2, 100f, 40f, Now);

            Assert.AreEqual(100f, lane.TopFor("a", Now), 1e-3f);
            Assert.AreEqual(148f, lane.TopFor("b", Now), 1e-3f);
            Assert.AreEqual(196f, lane.TopFor("c", Now), 1e-3f);
        }

        [Test]
        public void CardsFarApart_AreNotMoved()
        {
            var lane = new MessageCardStack();
            lane.Report("top", 0, 100f, 40f, Now);
            lane.Report("low", 1, 400f, 40f, Now);
            Assert.AreEqual(0f, lane.OffsetFor("low", Now), 1e-3f);
        }

        [Test]
        public void RemovingTheCardAbove_LetsTheOtherGoBack()
        {
            var lane = new MessageCardStack();
            lane.Report("banner", 0, 108f, 80f, Now);
            lane.Report("toast", 1, 144f, 60f, Now);
            Assert.Greater(lane.OffsetFor("toast", Now), 0f);

            Assert.IsTrue(lane.Remove("banner"));
            Assert.IsFalse(lane.Remove("banner"));
            Assert.AreEqual(0f, lane.OffsetFor("toast", Now), 1e-3f);
        }

        [Test]
        public void StaleCard_NoLongerPushesOthers()
        {
            var lane = new MessageCardStack();
            lane.Report("gone", 0, 108f, 80f, Now);
            lane.Report("toast", 1, 144f, 60f, Now + 1f);
            Assert.AreEqual(0f, lane.OffsetFor("toast", Now + 1f), 1e-3f, "A card not reported for StaleSeconds (controller destroyed) is ignored.");
        }

        [Test]
        public void Place_WithoutAPanel_KeepsTheCardOutOfTheLane()
        {
            var card = new VisualElement();
            MessageCards.Place(card, "test-card-no-panel", 0, visible: true);
            Assert.IsFalse(MessageCards.Lane.Contains("test-card-no-panel"));
            MessageCards.Release(card, "test-card-no-panel");
        }
    }
}
