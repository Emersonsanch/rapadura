using System.Collections.Generic;
using Rapadura.Gameplay.Inventory;
using Rapadura.Gameplay.Refining;

namespace Rapadura.Gameplay.Items.Options
{
    /// <summary>Why an engraving can or cannot be done right now.</summary>
    public enum EngraveCheck
    {
        Ok = 0,
        NoSocket = 1,
        UnknownEngraving = 2,
        MissingMaterial = 3,
        NotEnoughGold = 4,
        /// <summary>Weapon engravings only go in weapons, armour ones only in armour.</summary>
        WrongKind = 5,
    }

    /// <summary>
    /// The ferreiro's socket engraving ("Engaste", MU's socket items): spends the engraving's material and gold from the
    /// bag and writes its id in the piece's first empty socket (<see cref="InventorySlotData.sockets"/>). A full piece
    /// can have a socket re-engraved by index (the old engraving is lost, the item never is). Pure over a live
    /// <see cref="InventoryManager"/>; online (Fase 12) the server runs it.
    /// </summary>
    public static class EngraveService
    {
        /// <summary>Every piece with at least one socket: equipped first, then the bag (reuses <see cref="RefineService.ListTargets"/>).</summary>
        public static List<RefineTarget> ListSocketed(InventoryManager bag)
        {
            var list = new List<RefineTarget>();
            foreach (RefineTarget target in RefineService.ListTargets(bag))
            {
                InventorySlotData data = RefineService.Resolve(bag, target);
                if (data?.sockets != null && data.sockets.Length > 0)
                {
                    list.Add(target);
                }
            }

            return list;
        }

        /// <summary>Whether <paramref name="engravingId"/> can go in socket <paramref name="socketIndex"/> (-1 = first empty).</summary>
        public static EngraveCheck Check(InventoryManager bag, RefineTarget target, string engravingId, int socketIndex = -1)
        {
            InventorySlotData data = RefineService.Resolve(bag, target);
            int index = socketIndex >= 0 ? socketIndex : ItemOptionRules.FirstEmptySocket(data);
            if (data?.sockets == null || index < 0 || index >= data.sockets.Length)
            {
                return EngraveCheck.NoSocket;
            }

            Engraving engraving = ItemOptionCatalog.FindEngraving(engravingId);
            if (engraving == null)
            {
                return EngraveCheck.UnknownEngraving;
            }

            if (!ItemOptionRules.CanEngrave(ItemDatabase.GetById(data.itemId), engraving))
            {
                return EngraveCheck.WrongKind;
            }

            if (bag.GetTotalCount(engraving.MaterialId) < engraving.MaterialQuantity)
            {
                return EngraveCheck.MissingMaterial;
            }

            return bag.GetTotalCount(RefineItems.Gold) < engraving.Gold ? EngraveCheck.NotEnoughGold : EngraveCheck.Ok;
        }

        /// <summary>Spends and engraves. False (nothing spent) when <see cref="Check"/> fails.</summary>
        public static bool TryEngrave(InventoryManager bag, RefineTarget target, string engravingId, out EngraveCheck check, int socketIndex = -1)
        {
            check = bag == null ? EngraveCheck.NoSocket : Check(bag, target, engravingId, socketIndex);
            if (check != EngraveCheck.Ok)
            {
                return false;
            }

            InventorySlotData data = RefineService.Resolve(bag, target);
            int index = socketIndex >= 0 ? socketIndex : ItemOptionRules.FirstEmptySocket(data);
            Engraving engraving = ItemOptionCatalog.FindEngraving(engravingId);
            if (!bag.RemoveById(engraving.MaterialId, engraving.MaterialQuantity))
            {
                check = EngraveCheck.MissingMaterial;
                return false;
            }

            if (engraving.Gold > 0)
            {
                bag.RemoveById(RefineItems.Gold, engraving.Gold);
            }

            data.sockets[index] = engraving.Id;
            bag.NotifyItemDataChanged(target.IsEquipped ? target.EquipSlot : EquipmentSlot.None);
            return true;
        }
    }
}
