using System.Collections.Generic;
using UnityEngine;

namespace Rapadura.Core.Localization
{
    /// <summary>
    /// Externalized string table for the whole game. This is the "source of truth" asset a
    /// designer/translator edits in the Inspector (or that gets regenerated from a CSV export
    /// of a translation spreadsheet / Crowdin-style service — see
    /// <see cref="LocalizationCsv"/> for the interchange format used by that pipeline).
    ///
    /// Deliberately holds no runtime logic beyond exposing its rows — all lookup/fallback
    /// behaviour lives in <see cref="LocalizationManager"/> so it can be unit tested without
    /// touching ScriptableObjects.
    /// </summary>
    [CreateAssetMenu(fileName = "LocalizationTable", menuName = "Rapadura/Localization/Localization Table")]
    public class LocalizationTable : ScriptableObject
    {
        [SerializeField]
        private List<LocalizationEntry> _entries = new List<LocalizationEntry>();

        public IReadOnlyList<LocalizationEntry> Entries => _entries;

        /// <summary>Merges parsed CSV/JSON rows into this table's entries (used by import tooling).
        /// Existing keys are overwritten so re-imports are idempotent.</summary>
        public void ReplaceEntries(IEnumerable<LocalizationEntry> entries)
        {
            _entries.Clear();
            _entries.AddRange(entries);
        }
    }
}
