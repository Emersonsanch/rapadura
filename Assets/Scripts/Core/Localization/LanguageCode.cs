namespace Rapadura.Core.Localization
{
    /// <summary>
    /// Languages supported by the localization pipeline. Values are used as lookup keys into
    /// <see cref="LocalizationTable"/> entries — keep them in sync with any external
    /// CSV/Crowdin export column headers (see Docs/Localization pipeline notes).
    /// </summary>
    public enum LanguageCode
    {
        /// <summary>English. Also the mandatory fallback language — every key must have an
        /// English translation, since <see cref="LocalizationManager"/> falls back to it.</summary>
        en,

        /// <summary>Brazilian Portuguese.</summary>
        ptBR,
    }
}
