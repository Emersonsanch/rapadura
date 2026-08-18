namespace Rapadura.Core.Interfaces
{
    /// <summary>
    /// Contract implemented by every top-level manager (Inventory, Save, Skills, Audio, etc.).
    /// GameManager calls Initialize/Shutdown on all registered managers in a predictable order.
    /// </summary>
    public interface IManager
    {
        /// <summary>Called once by GameManager during bootstrap, after all managers are registered.</summary>
        void Initialize();

        /// <summary>Called once by GameManager on application shutdown or scene teardown.</summary>
        void Shutdown();
    }
}
