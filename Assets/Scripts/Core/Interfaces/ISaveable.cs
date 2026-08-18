namespace Rapadura.Core.Interfaces
{
    /// <summary>
    /// Implemented by any system that needs to persist state through the SaveManager
    /// (inventory, player transform/stats, skills, world/building state, quests, settings).
    /// Each saveable is responsible for its own slice of the save file, identified by <see cref="SaveKey"/>.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Unique identifier for this saveable's data slice inside the save file.</summary>
        string SaveKey { get; }

        /// <summary>Returns a plain, JSON-serializable snapshot of this system's current state.</summary>
        object CaptureState();

        /// <summary>Restores this system's state from a previously captured snapshot.</summary>
        void RestoreState(object state);
    }
}
