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
        public const int Default = 0;
    }
}
