namespace Rapadura.Gameplay.Items
{
    /// <summary>Sub-classification used to group and filter items in the inventory UI/shop.</summary>
    public enum ItemCategory
    {
        // Materials
        Wood,
        Stone,
        Metal,
        Herb,
        Fiber,

        // Consumables
        Food,
        Potion,

        // Weapons
        MeleeWeapon,
        RangedWeapon,
        MagicWeapon,

        // Armor
        Head,
        Chest,
        Legs,
        Boots,
        Accessory,

        // Tools
        GatheringTool,
        BuildingTool,

        // Misc
        QuestItem,
        Currency,
        Blueprint,
        Miscellaneous
    }
}
