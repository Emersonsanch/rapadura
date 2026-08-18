using System;

namespace Rapadura.Core.Localization
{
    /// <summary>
    /// One translation row: a stable key (e.g. "ui.pause.title") plus the text for every
    /// supported language. Serializable so it can be authored either in a
    /// <see cref="LocalizationTable"/> ScriptableObject (Inspector-editable list) or parsed
    /// straight out of an external CSV/JSON export from a translation pipeline.
    /// </summary>
    [Serializable]
    public class LocalizationEntry
    {
        public string Key;
        public string En;
        public string PtBR;

        public LocalizationEntry()
        {
        }

        public LocalizationEntry(string key, string en, string ptBR)
        {
            Key = key;
            En = en;
            PtBR = ptBR;
        }

        /// <summary>Returns the text for the given language, or null if it wasn't authored (untranslated).</summary>
        public string GetText(LanguageCode language)
        {
            switch (language)
            {
                case LanguageCode.en:
                    return En;
                case LanguageCode.ptBR:
                    return PtBR;
                default:
                    return null;
            }
        }
    }
}
