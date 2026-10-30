namespace Rapadura.Gameplay.Items
{
    /// <summary>Body/loadout slot an equippable item occupies. None means the item cannot be equipped.</summary>
    public enum EquipmentSlot
    {
        None,
        Head,
        Chest,
        Legs,
        Boots,
        MainHand,
        OffHand,
        Tool,
        Accessory,
        // Append only: item assets and saves store the slot as its int (Wings = 9).
        /// <summary>"Costas/Asas": wings and capes of the Tacho Encantado (<c>Rapadura.Gameplay.Tacho.WingCatalog</c>).</summary>
        Wings
    }
}
