namespace Rapadura.Core.Interfaces
{
    /// <summary>
    /// Optional companion to <see cref="ISaveable"/> for saveables whose <c>RestoreState</c> must
    /// run before (or after) the others during <c>SaveManager.Load</c>. Lower values restore first;
    /// any saveable that doesn't implement this is treated as <see cref="Default"/>. Ties keep the
    /// order the entries were written in.
    ///
    /// Example: the selected character restores at <see cref="First"/> so its passive and signature
    /// skill set are already applied when PlayerStats/AttributeSet/SkillManager restore their own
    /// (authoritative) values on top.
    /// </summary>
    public interface IRestoreOrder
    {
        int RestoreOrder { get; }
    }

    /// <summary>Well-known <see cref="IRestoreOrder.RestoreOrder"/> values.</summary>
    public static class RestoreOrders
    {
        public const int First = -100;

        /// <summary>
        /// InventoryManager: its RestoreState clears every BuffController effect before re-applying
        /// equipment, so it must run before AttributeSet/SkillManager (Default) re-apply theirs.
        /// </summary>
        public const int Equipment = -10;

        public const int Default = 0;

        /// <summary>
        /// PlayerStats: restores current health/mana against MaxHealth/MaxMana, so it runs once every
        /// stat bonus (attributes, equipment, passives) is back — otherwise the max it caches is the
        /// bare one and the bonuses arriving later read as a max increase (free heal on every scene hop).
        /// </summary>
        public const int AfterStatBonuses = 100;
    }
}
