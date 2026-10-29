using System;
using System.IO;
using NUnit.Framework;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Flow;
using Rapadura.Save;
using Rapadura.UI.Menus;
using UnityEngine;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for the title screen's save buttons: "Continuar" picks the newest save across
    /// the auto-save and every manual slot (<see cref="GameFlowService.FindNewestSaveSlot"/>), and the
    /// "Continuar"/"Carregar" enabled states (<see cref="MainMenuPresenter"/>). Saves go to a temp
    /// folder (<see cref="TestSaveRoot"/>); timestamps are written straight into the files.
    /// </summary>
    public class MainMenuSaveSelectionTests
    {
        private SaveManager _saveManager;
        private DateTime _now;

        [SetUp]
        public void SetUp()
        {
            TestSaveRoot.Enter();
            EventBus.Clear();
            _saveManager = new SaveManager();
            _saveManager.Initialize();
            _now = DateTime.UtcNow;
        }

        [TearDown]
        public void TearDown()
        {
            _saveManager.Shutdown();
            EventBus.Clear();
            TestSaveRoot.Exit();
        }

        private static void WriteSave(int slot, DateTime savedAtUtc)
        {
            var data = new SaveData
            {
                createdAtIso8601 = savedAtUtc.ToString("o"),
                lastSavedAtIso8601 = savedAtUtc.ToString("o")
            };

            File.WriteAllText(SaveManager.GetSlotFilePath(slot), JsonUtility.ToJson(data));
        }

        private static void WriteCorrupt(int slot)
        {
            File.WriteAllText(SaveManager.GetSlotFilePath(slot), "{{{ not a save");
        }

        /// <summary>Rewrites a real save's timestamp so tests control which one is "newest".</summary>
        private static void SetSavedAt(int slot, DateTime savedAtUtc)
        {
            string path = SaveManager.GetSlotFilePath(slot);
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            data.lastSavedAtIso8601 = savedAtUtc.ToString("o");
            File.WriteAllText(path, JsonUtility.ToJson(data));
        }

        [Test]
        public void ContinueSlots_CoverAutoSaveAndEveryManualSlot()
        {
            CollectionAssert.AreEqual(new[] { SaveManager.AutoSaveSlot, 0, 1, 2 }, GameFlowService.DefaultContinueSlots);
            CollectionAssert.AreEqual(new[] { 0, 1, 2 }, GameFlowService.ManualSlots);
            Assert.AreEqual(SaveManager.ManualSlotCount, GameFlowService.ManualSlots.Length);
        }

        [Test]
        public void NewestSave_AutoSaveNewer_WinsOverManualSlots()
        {
            WriteSave(SaveManager.AutoSaveSlot, _now);
            WriteSave(0, _now.AddHours(-2));
            WriteSave(2, _now.AddMinutes(-1));

            Assert.AreEqual(SaveManager.AutoSaveSlot, GameFlowService.FindNewestSaveSlot(_saveManager));
            Assert.AreEqual(SaveManager.AutoSaveSlot, MainMenuPresenter.GetContinueSlot(_saveManager));
        }

        [Test]
        public void NewestSave_ManualSlot2Newer_WinsOverOlderAutoSave()
        {
            WriteSave(SaveManager.AutoSaveSlot, _now.AddDays(-1));
            WriteSave(0, _now.AddHours(-3));
            WriteSave(2, _now);

            Assert.AreEqual(2, GameFlowService.FindNewestSaveSlot(_saveManager));
            Assert.AreEqual(2, MainMenuPresenter.GetContinueSlot(_saveManager));
        }

        [Test]
        public void NewestSave_SkipsMissingAndCorruptSlots()
        {
            WriteSave(SaveManager.AutoSaveSlot, _now.AddHours(-1));
            WriteCorrupt(1); // no .bak either: unreadable, never picked
            // slot 0 and 2 missing

            Assert.AreEqual(SaveManager.AutoSaveSlot, GameFlowService.FindNewestSaveSlot(_saveManager));
        }

        [Test]
        public void NewestSave_OnlyCorruptOrNoSaves_ReturnsNoSlot()
        {
            Assert.AreEqual(GameFlowMachine.NoSlot, GameFlowService.FindNewestSaveSlot(_saveManager));
            Assert.AreEqual(GameFlowMachine.NoSlot, GameFlowService.FindNewestSaveSlot(null));

            WriteCorrupt(0);
            Assert.AreEqual(GameFlowMachine.NoSlot, GameFlowService.FindNewestSaveSlot(_saveManager));
        }

        [Test]
        public void Buttons_NoSaves_ContinueAndLoadDisabled()
        {
            Assert.IsFalse(MainMenuPresenter.IsContinueEnabled(_saveManager));
            Assert.IsFalse(MainMenuPresenter.IsLoadEnabled(_saveManager, hasLoadMenu: true));
            Assert.IsFalse(MainMenuPresenter.IsContinueEnabled(null));
            Assert.IsFalse(MainMenuPresenter.IsLoadEnabled(null, hasLoadMenu: true));
        }

        [Test]
        public void Buttons_OnlyAutoSave_ContinueEnabled_LoadDisabled()
        {
            WriteSave(SaveManager.AutoSaveSlot, _now);

            Assert.IsTrue(MainMenuPresenter.IsContinueEnabled(_saveManager));
            Assert.IsFalse(MainMenuPresenter.IsLoadEnabled(_saveManager, hasLoadMenu: true), "The Save/Load screen only lists manual slots.");
        }

        [Test]
        public void Buttons_ManualSlot_BothEnabled_UnlessNoLoadMenu()
        {
            WriteSave(1, _now);

            Assert.IsTrue(MainMenuPresenter.IsContinueEnabled(_saveManager));
            Assert.IsTrue(MainMenuPresenter.IsLoadEnabled(_saveManager, hasLoadMenu: true));
            Assert.IsFalse(MainMenuPresenter.IsLoadEnabled(_saveManager, hasLoadMenu: false));
        }

        [Test]
        public void Buttons_OnlyCorruptManualSlot_BothDisabled()
        {
            WriteCorrupt(2);

            Assert.IsFalse(MainMenuPresenter.IsContinueEnabled(_saveManager));
            Assert.IsFalse(MainMenuPresenter.IsLoadEnabled(_saveManager, hasLoadMenu: true));
        }

        [Test]
        public void Continue_LoadsNewestManualSlot_InsteadOfOlderAutoSave()
        {
            var selection = new CharacterSelectionState();
            _saveManager.Register(selection);

            selection.Select(CharacterId.Joaquim);
            _saveManager.AutoSave();
            selection.Select(CharacterId.Maithe);
            _saveManager.Save(2);
            SetSavedAt(SaveManager.AutoSaveSlot, _now.AddHours(-1));
            SetSavedAt(2, _now);
            selection.Clear();

            var service = new GameFlowService(selection, _saveManager);
            service.Initialize();

            try
            {
                service.EnterFrontEnd();
                Assert.IsTrue(service.CanContinue());
                Assert.IsTrue(service.Continue());
                Assert.AreEqual(CharacterId.Maithe, selection.Selected, "Continuar must load slot 2 (newest), not the older auto-save.");
                Assert.AreEqual(GameFlowState.World, service.State);
            }
            finally
            {
                service.Shutdown();
            }
        }

        [Test]
        public void LoadFromTitle_LoadsChosenSlot_AndLeavesTitle()
        {
            var selection = new CharacterSelectionState();
            _saveManager.Register(selection);

            selection.Select(CharacterId.Icaro);
            _saveManager.Save(1);
            selection.Select(CharacterId.Lavine);
            _saveManager.Save(0); // newer, but the player picked slot 1
            selection.Clear();

            var service = new GameFlowService(selection, _saveManager);
            service.Initialize();

            try
            {
                service.EnterFrontEnd();
                Assert.IsFalse(service.LoadFromTitle(2), "Empty slot: nothing to load, stay on the title.");
                Assert.AreEqual(GameFlowState.MainMenu, service.State);

                Assert.IsTrue(service.LoadFromTitle(1));
                Assert.AreEqual(CharacterId.Icaro, selection.Selected);
                Assert.AreEqual(GameFlowState.World, service.State);
            }
            finally
            {
                service.Shutdown();
            }
        }
    }
}
