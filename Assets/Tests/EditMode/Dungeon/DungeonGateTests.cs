using System.Collections.Generic;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Dungeon;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Items;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// The barred gates of the Ossuário dos Artífices (TODO "Testes Automatizados da Masmorra"): no key = nothing changes,
    /// a key opens the gate and is spent, an open gate never asks again (also after a save round-trip). Pure rule
    /// (<see cref="DungeonRules.TryUnlockGate"/>) and the component (<see cref="LockedDungeonGate"/>) with a real bag and
    /// the shipped Chave de Osso Rúnica.
    /// </summary>
    public class DungeonGateTests
    {
        private readonly List<Object> _created = new List<Object>();

        [SetUp]
        public void SetUp()
        {
            EventBus.Clear();
            ItemDatabase.Invalidate();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object obj in _created)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }

            _created.Clear();
            EventBus.Clear();
            ItemDatabase.Invalidate();
        }

        // ------------------------------------------------------------------
        // Pure rule
        // ------------------------------------------------------------------

        [Test]
        public void Rule_WithoutKey_ReturnsNoKey_AndChangesNothing()
        {
            var progress = new DungeonProgress();
            bool removed = false;
            GateUnlockResult result = DungeonRules.TryUnlockGate(progress, DungeonRules.GateGalleryId, DungeonRules.BoneKeyItemId, true,
                _ => 0, (_, __) => removed = true);

            Assert.AreEqual(GateUnlockResult.NoKey, result);
            Assert.IsFalse(removed, "nothing is taken from the bag");
            Assert.IsFalse(progress.IsGateOpen(DungeonRules.GateGalleryId));
        }

        [Test]
        public void Rule_WithKey_SpendsOne_AndOpens()
        {
            var progress = new DungeonProgress();
            int keys = 2;
            GateUnlockResult result = DungeonRules.TryUnlockGate(progress, DungeonRules.GateGalleryId, DungeonRules.BoneKeyItemId, true,
                _ => keys, (_, amount) => { keys -= amount; return true; });

            Assert.AreEqual(GateUnlockResult.Unlocked, result);
            Assert.AreEqual(1, keys);
            Assert.IsTrue(progress.IsGateOpen(DungeonRules.GateGalleryId));
            Assert.IsFalse(progress.IsGateOpen(DungeonRules.GateColossusId), "only that gate");
        }

        [Test]
        public void Rule_KeyThatCannotBeRemoved_KeepsTheGateShut()
        {
            var progress = new DungeonProgress();
            GateUnlockResult result = DungeonRules.TryUnlockGate(progress, DungeonRules.GateColossusId, DungeonRules.BoneKeyItemId, true,
                _ => 1, (_, __) => false);

            Assert.AreEqual(GateUnlockResult.NoKey, result);
            Assert.IsFalse(progress.IsGateOpen(DungeonRules.GateColossusId));
        }

        [Test]
        public void Rule_NotConsumingGate_KeepsTheKey()
        {
            var progress = new DungeonProgress();
            bool removed = false;
            GateUnlockResult result = DungeonRules.TryUnlockGate(progress, DungeonRules.GateGalleryId, DungeonRules.BoneKeyItemId, false,
                _ => 1, (_, __) => removed = true);

            Assert.AreEqual(GateUnlockResult.Unlocked, result);
            Assert.IsFalse(removed);
        }

        [Test]
        public void Rule_OpenGate_IsAlreadyOpen_WithoutTouchingTheBag()
        {
            var progress = new DungeonProgress();
            progress.MarkGateOpen(DungeonRules.GateGalleryId);
            bool asked = false;
            GateUnlockResult result = DungeonRules.TryUnlockGate(progress, DungeonRules.GateGalleryId, DungeonRules.BoneKeyItemId, true,
                _ => { asked = true; return 0; }, (_, __) => { asked = true; return true; });

            Assert.AreEqual(GateUnlockResult.AlreadyOpen, result);
            Assert.IsFalse(asked);
        }

        // ------------------------------------------------------------------
        // Component + real bag + shipped key
        // ------------------------------------------------------------------

        [Test]
        public void Gate_WithoutKey_RemainsLocked()
        {
            var progress = new DungeonProgress();
            LockedDungeonGate gate = NewGate(progress, out Transform bars);
            InventoryManager bag = NewBag();

            Assert.AreEqual(GateUnlockResult.NoKey, gate.TryUnlock(bag));
            Assert.IsFalse(gate.IsOpen);
            Assert.IsTrue(gate.CanInteract);
            Assert.AreEqual(0f, gate.OpenProgress, 1e-4f, "the bars did not start rising");
            Assert.AreEqual(0f, bars.localPosition.y, 1e-4f);
        }

        [Test]
        public void Gate_WithKey_ConsumesKeyAndUnlocks()
        {
            var progress = new DungeonProgress();
            LockedDungeonGate gate = NewGate(progress, out Transform bars);
            InventoryManager bag = NewBag();
            ItemDefinition key = ItemDatabase.GetById(DungeonRules.BoneKeyItemId);
            Assert.IsNotNull(key, "Resources/Items/Keys/BoneKey.asset");
            bag.AddItemIgnoringWeight(key, 1);
            Assert.AreEqual(1, bag.GetTotalCount(DungeonRules.BoneKeyItemId));

            int opened = 0;
            gate.Opened += _ => opened++;
            Assert.AreEqual(GateUnlockResult.Unlocked, gate.TryUnlock(bag));

            Assert.AreEqual(0, bag.GetTotalCount(DungeonRules.BoneKeyItemId), "the key stays in the lock");
            Assert.IsTrue(gate.IsOpen);
            Assert.IsTrue(progress.IsGateOpen(DungeonRules.GateGalleryId));
            Assert.AreEqual(1, opened);
            Assert.IsFalse(gate.CanInteract);

            gate.StepOpening(10f);
            Assert.AreEqual(1f, gate.OpenProgress, 1e-4f);
            Assert.Greater(bars.localPosition.y, 3f, "the bars rose");
        }

        [Test]
        public void Gate_Unlocked_DoesNotRequireKeyAgain()
        {
            var progress = new DungeonProgress();
            LockedDungeonGate gate = NewGate(progress, out _);
            InventoryManager bag = NewBag();
            bag.AddItemIgnoringWeight(ItemDatabase.GetById(DungeonRules.BoneKeyItemId), 2);
            Assert.AreEqual(GateUnlockResult.Unlocked, gate.TryUnlock(bag));

            Assert.AreEqual(GateUnlockResult.AlreadyOpen, gate.TryUnlock(bag));
            Assert.AreEqual(1, bag.GetTotalCount(DungeonRules.BoneKeyItemId), "the second key is kept");

            // Saved and loaded (next visit): the gate builds already open, no key asked.
            var loaded = new DungeonProgress();
            loaded.RestoreState(progress.CaptureState());
            LockedDungeonGate rebuilt = NewGate(loaded, out Transform bars);
            Assert.IsTrue(rebuilt.IsOpen);
            Assert.AreEqual(1f, rebuilt.OpenProgress, 1e-4f, "snapped open on load");
            Assert.Greater(bars.localPosition.y, 3f);
            Assert.AreEqual(GateUnlockResult.AlreadyOpen, rebuilt.TryUnlock(NewBag()));
        }

        [Test]
        public void ClearingTheOssuary_ReSealsTheGates()
        {
            var progress = new DungeonProgress();
            progress.MarkGateOpen(DungeonRules.GateGalleryId);
            progress.MarkGateOpen(DungeonRules.GateColossusId);

            progress.RecordClear();

            Assert.IsFalse(progress.IsGateOpen(DungeonRules.GateGalleryId));
            Assert.IsFalse(progress.IsGateOpen(DungeonRules.GateColossusId));
            Assert.AreEqual(1, progress.Clears);
        }

        private LockedDungeonGate NewGate(DungeonProgress progress, out Transform bars)
        {
            var go = new GameObject("DungeonGateTests_Gate");
            _created.Add(go);
            bars = new GameObject("Bars").transform;
            bars.SetParent(go.transform, false);
            LockedDungeonGate gate = go.AddComponent<LockedDungeonGate>();
            gate.Progress = progress;
            gate.Configure(DungeonRules.GateGalleryId, bars);
            return gate;
        }

        private InventoryManager NewBag()
        {
            var go = new GameObject("DungeonGateTests_Bag");
            _created.Add(go);
            return go.AddComponent<InventoryManager>();
        }
    }
}
