using NUnit.Framework;
using Rapadura.UI.Menus;

namespace Rapadura.Tests
{
    /// <summary>
    /// EditMode tests for <see cref="SaveSlotPresenter"/> — the pure label/enabled-state rules
    /// behind <see cref="SaveLoadMenuController"/>'s slot rows, exercised without a real
    /// <c>UIDocument</c>/<c>SaveManager</c>.
    /// </summary>
    public class SaveSlotPresenterTests
    {
        [Test]
        public void GetState_SlotExists_ReturnsOccupied()
        {
            Assert.AreEqual(SaveSlotPresenter.SlotState.Occupied, SaveSlotPresenter.GetState(true));
        }

        [Test]
        public void GetState_SlotMissing_ReturnsEmpty()
        {
            Assert.AreEqual(SaveSlotPresenter.SlotState.Empty, SaveSlotPresenter.GetState(false));
        }

        [Test]
        public void GetSlotTitle_IsOneBasedForDisplay()
        {
            Assert.AreEqual("Slot 1", SaveSlotPresenter.GetSlotTitle(0));
            Assert.AreEqual("Slot 3", SaveSlotPresenter.GetSlotTitle(2));
        }

        [Test]
        public void GetStatusLabel_ReflectsExistence()
        {
            Assert.AreEqual("Salvo", SaveSlotPresenter.GetStatusLabel(true));
            Assert.AreEqual("Vazio", SaveSlotPresenter.GetStatusLabel(false));
        }

        [Test]
        public void CanSave_AlwaysTrue_RegardlessOfExistence()
        {
            Assert.IsTrue(SaveSlotPresenter.CanSave(true));
            Assert.IsTrue(SaveSlotPresenter.CanSave(false));
        }

        [Test]
        public void CanLoad_OnlyWhenSlotExists()
        {
            Assert.IsTrue(SaveSlotPresenter.CanLoad(true));
            Assert.IsFalse(SaveSlotPresenter.CanLoad(false));
        }

        [Test]
        public void CanDelete_OnlyWhenSlotExists()
        {
            Assert.IsTrue(SaveSlotPresenter.CanDelete(true));
            Assert.IsFalse(SaveSlotPresenter.CanDelete(false));
        }

        [Test]
        public void GetPrimaryActionLabel_SaveMode_EmptySlot_IsSalvar()
        {
            Assert.AreEqual("Salvar", SaveSlotPresenter.GetPrimaryActionLabel(isSaveMode: true, slotExists: false));
        }

        [Test]
        public void GetPrimaryActionLabel_SaveMode_OccupiedSlot_IsSobrescrever()
        {
            Assert.AreEqual("Sobrescrever", SaveSlotPresenter.GetPrimaryActionLabel(isSaveMode: true, slotExists: true));
        }

        [Test]
        public void GetPrimaryActionLabel_LoadMode_EmptySlot_IsVazio()
        {
            Assert.AreEqual("Vazio", SaveSlotPresenter.GetPrimaryActionLabel(isSaveMode: false, slotExists: false));
        }

        [Test]
        public void GetPrimaryActionLabel_LoadMode_OccupiedSlot_IsCarregar()
        {
            Assert.AreEqual("Carregar", SaveSlotPresenter.GetPrimaryActionLabel(isSaveMode: false, slotExists: true));
        }

        [Test]
        public void IsPrimaryActionEnabled_SaveMode_AlwaysEnabled()
        {
            Assert.IsTrue(SaveSlotPresenter.IsPrimaryActionEnabled(isSaveMode: true, slotExists: false));
            Assert.IsTrue(SaveSlotPresenter.IsPrimaryActionEnabled(isSaveMode: true, slotExists: true));
        }

        [Test]
        public void IsPrimaryActionEnabled_LoadMode_OnlyWhenSlotExists()
        {
            Assert.IsFalse(SaveSlotPresenter.IsPrimaryActionEnabled(isSaveMode: false, slotExists: false));
            Assert.IsTrue(SaveSlotPresenter.IsPrimaryActionEnabled(isSaveMode: false, slotExists: true));
        }
    }
}
