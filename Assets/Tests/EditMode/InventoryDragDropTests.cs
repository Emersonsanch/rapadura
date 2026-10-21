using NUnit.Framework;
using Rapadura.Gameplay.Items;
using Rapadura.UI.Common;
using Rapadura.UI.Inventory;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="InventoryDragDrop"/> — the pure rules behind the Mochila's
    /// drag-and-drop (no panel required): drag threshold vs. the long-press tooltip, drop target hit
    /// testing (with ScrollView clipping) and what each source/target pair does.
    /// </summary>
    public class InventoryDragDropTests
    {
        // ---------------------------------------------------------------
        // Threshold
        // ---------------------------------------------------------------

        [Test]
        public void DragThreshold_MatchesTooltipLongPressTolerance()
        {
            // Hold still = tooltip, move = drag: both decided by the same distance.
            Assert.AreEqual(TooltipTrigger.MoveTolerancePixels, InventoryDragDrop.DragThresholdPixels);
        }

        [Test]
        public void HasExceededDragThreshold_SmallJitter_IsNotADrag()
        {
            Vector2 press = new Vector2(100f, 100f);

            Assert.IsFalse(InventoryDragDrop.HasExceededDragThreshold(press, press));
            Assert.IsFalse(InventoryDragDrop.HasExceededDragThreshold(press, press + new Vector2(6f, -6f)));
            Assert.IsFalse(InventoryDragDrop.HasExceededDragThreshold(press, press + new Vector2(InventoryDragDrop.DragThresholdPixels, 0f)),
                "exactly at the threshold is still a press (strictly greater starts a drag, like TooltipTrigger)");
        }

        [Test]
        public void HasExceededDragThreshold_BeyondThreshold_InAnyDirection_IsADrag()
        {
            Vector2 press = new Vector2(100f, 100f);
            float d = InventoryDragDrop.DragThresholdPixels + 1f;

            Assert.IsTrue(InventoryDragDrop.HasExceededDragThreshold(press, press + new Vector2(d, 0f)));
            Assert.IsTrue(InventoryDragDrop.HasExceededDragThreshold(press, press + new Vector2(0f, -d)));
            Assert.IsTrue(InventoryDragDrop.HasExceededDragThreshold(press, press + new Vector2(-d, d) * 0.75f));
        }

        // ---------------------------------------------------------------
        // Target hit testing
        // ---------------------------------------------------------------

        [Test]
        public void FindTargetIndex_ReturnsRectContainingPoint()
        {
            Rect[] rects =
            {
                new Rect(0f, 0f, 72f, 72f),
                new Rect(78f, 0f, 72f, 72f),
                new Rect(0f, 78f, 72f, 72f),
            };

            Assert.AreEqual(0, InventoryDragDrop.FindTargetIndex(new Vector2(10f, 10f), rects));
            Assert.AreEqual(1, InventoryDragDrop.FindTargetIndex(new Vector2(100f, 30f), rects));
            Assert.AreEqual(2, InventoryDragDrop.FindTargetIndex(new Vector2(30f, 100f), rects));
        }

        [Test]
        public void FindTargetIndex_GapOrOutside_ReturnsMinusOne()
        {
            Rect[] rects = { new Rect(0f, 0f, 72f, 72f), new Rect(78f, 0f, 72f, 72f) };

            Assert.AreEqual(-1, InventoryDragDrop.FindTargetIndex(new Vector2(75f, 30f), rects), "between tiles");
            Assert.AreEqual(-1, InventoryDragDrop.FindTargetIndex(new Vector2(500f, 500f), rects));
            Assert.AreEqual(-1, InventoryDragDrop.FindTargetIndex(Vector2.zero, null));
        }

        [Test]
        public void FindTargetIndex_TargetScrolledOutOfViewport_IsIgnored()
        {
            // Row 1 lies under the header area: its rect contains the point but its ScrollView
            // viewport (clip) doesn't, so the drop must not land on it.
            Rect viewport = new Rect(0f, 100f, 300f, 200f);
            Rect header = new Rect(0f, 0f, 300f, 100f);
            Rect[] rects = { new Rect(0f, 40f, 300f, 44f), new Rect(0f, 120f, 300f, 44f), new Rect(250f, 20f, 44f, 44f) };
            Rect[] clips = { viewport, viewport, header };

            Assert.AreEqual(-1, InventoryDragDrop.FindTargetIndex(new Vector2(50f, 60f), rects, clips));
            Assert.AreEqual(1, InventoryDragDrop.FindTargetIndex(new Vector2(50f, 130f), rects, clips));
            Assert.AreEqual(2, InventoryDragDrop.FindTargetIndex(new Vector2(260f, 30f), rects, clips));
        }

        [Test]
        public void FindTargetIndex_OverlappingRects_LastOneWins()
        {
            Rect[] rects = { new Rect(0f, 0f, 100f, 100f), new Rect(50f, 50f, 100f, 100f) };

            Assert.AreEqual(1, InventoryDragDrop.FindTargetIndex(new Vector2(75f, 75f), rects));
        }

        // ---------------------------------------------------------------
        // Drop resolution
        // ---------------------------------------------------------------

        [Test]
        public void ResolveDrop_BackpackToOtherBackpackSlot_MovesOrMerges()
        {
            Assert.AreEqual(InventoryDropAction.MoveOrMerge,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Backpack(2), InventoryDragEndpoint.Backpack(7), EquipmentSlot.None));
            Assert.AreEqual(InventoryDropAction.MoveOrMerge,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Backpack(0), InventoryDragEndpoint.Backpack(1), EquipmentSlot.MainHand),
                "equippables move between slots too");
        }

        [Test]
        public void ResolveDrop_BackpackOntoItself_DoesNothing()
        {
            Assert.AreEqual(InventoryDropAction.None,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Backpack(3), InventoryDragEndpoint.Backpack(3), EquipmentSlot.None));
        }

        [Test]
        public void ResolveDrop_BackpackOntoMatchingEquipmentSlot_Equips()
        {
            Assert.AreEqual(InventoryDropAction.Equip,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Backpack(4), InventoryDragEndpoint.Equipment(EquipmentSlot.Head), EquipmentSlot.Head));
        }

        [Test]
        public void ResolveDrop_BackpackOntoWrongEquipmentSlot_OrNonEquippable_IsRejected()
        {
            Assert.AreEqual(InventoryDropAction.None,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Backpack(4), InventoryDragEndpoint.Equipment(EquipmentSlot.Boots), EquipmentSlot.Head));
            Assert.AreEqual(InventoryDropAction.None,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Backpack(4), InventoryDragEndpoint.Equipment(EquipmentSlot.Head), EquipmentSlot.None));
        }

        [Test]
        public void ResolveDrop_EquipmentOntoBackpackSlot_Unequips()
        {
            Assert.AreEqual(InventoryDropAction.Unequip,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Equipment(EquipmentSlot.MainHand), InventoryDragEndpoint.Backpack(9), EquipmentSlot.MainHand));
        }

        [Test]
        public void ResolveDrop_EquipmentOntoEquipment_OrNowhere_DoesNothing()
        {
            Assert.AreEqual(InventoryDropAction.None,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Equipment(EquipmentSlot.MainHand), InventoryDragEndpoint.Equipment(EquipmentSlot.OffHand), EquipmentSlot.MainHand));
            Assert.AreEqual(InventoryDropAction.None,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Equipment(EquipmentSlot.MainHand), InventoryDragEndpoint.None, EquipmentSlot.MainHand));
            Assert.AreEqual(InventoryDropAction.None,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.Backpack(1), InventoryDragEndpoint.None, EquipmentSlot.None));
            Assert.AreEqual(InventoryDropAction.None,
                InventoryDragDrop.ResolveDrop(InventoryDragEndpoint.None, InventoryDragEndpoint.Backpack(1), EquipmentSlot.None));
        }
    }
}
