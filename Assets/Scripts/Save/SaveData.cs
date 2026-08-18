using System;
using System.Collections.Generic;

namespace Rapadura.Save
{
    /// <summary>
    /// One serialized slice of save data, produced by a single <see cref="Rapadura.Core.Interfaces.ISaveable"/>.
    /// The state is stored pre-serialized as JSON text plus its runtime type name so it can be
    /// deserialized back into the correct concrete type via reflection.
    /// </summary>
    [Serializable]
    public class SaveEntry
    {
        public string key;
        public string typeName;
        public string json;
    }

    /// <summary>
    /// Root container written to disk as a single JSON file. Holds metadata about the save
    /// (version, timestamps) plus every registered system's serialized state.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Schema version this save was written with. <see cref="SaveManager"/> reads this on load
        /// and runs any incremental migrations needed to bring an older save up to
        /// <see cref="SaveManager.CurrentSaveVersion"/> before applying it to registered saveables.
        /// A save file that predates versioning entirely (field missing/empty) is treated as version 0.
        /// </summary>
        public string saveVersion = SaveManager.CurrentSaveVersion.ToString();

        public string createdAtIso8601;
        public string lastSavedAtIso8601;
        public List<SaveEntry> entries = new List<SaveEntry>();
    }
}
