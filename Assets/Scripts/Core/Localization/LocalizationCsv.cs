using System;
using System.Collections.Generic;

namespace Rapadura.Core.Localization
{
    /// <summary>
    /// Parses the CSV interchange format used by the translation pipeline: a spreadsheet
    /// (Google Sheets/Excel export) or a Crowdin-style export with columns
    /// "key,en,pt-BR". This is intentionally a small hand-rolled parser (no external
    /// dependency) supporting quoted fields with embedded commas, so translators can use
    /// commas/newlines inside a translated string.
    ///
    /// Usage: export the spreadsheet to StreamingAssets/Localization/strings.csv (or feed a
    /// Resources TextAsset's .text), then call <see cref="Parse"/> and hand the result to
    /// <see cref="LocalizationTable.ReplaceEntries"/> or directly to
    /// <see cref="LocalizationManager.LoadEntries"/>.
    /// </summary>
    public static class LocalizationCsv
    {
        /// <summary>Parses CSV text with a header row "key,en,pt-BR" (column order matters, names are case-insensitive).</summary>
        public static List<LocalizationEntry> Parse(string csvText)
        {
            var entries = new List<LocalizationEntry>();
            if (string.IsNullOrEmpty(csvText))
            {
                return entries;
            }

            List<List<string>> rows = ParseRows(csvText);
            if (rows.Count == 0)
            {
                return entries;
            }

            List<string> header = rows[0];
            int keyIndex = IndexOfHeader(header, "key");
            int enIndex = IndexOfHeader(header, "en");
            int ptBRIndex = IndexOfHeader(header, "pt-br", "ptbr", "pt_br");

            if (keyIndex < 0 || enIndex < 0)
            {
                throw new FormatException("Localization CSV must have at least 'key' and 'en' columns.");
            }

            for (int i = 1; i < rows.Count; i++)
            {
                List<string> row = rows[i];
                if (row.Count == 0 || (row.Count == 1 && row[0].Length == 0))
                {
                    continue; // skip blank lines
                }

                string key = GetCell(row, keyIndex);
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                string en = GetCell(row, enIndex);
                string ptBR = ptBRIndex >= 0 ? GetCell(row, ptBRIndex) : null;

                entries.Add(new LocalizationEntry(key, en, ptBR));
            }

            return entries;
        }

        private static string GetCell(List<string> row, int index)
        {
            return index >= 0 && index < row.Count ? row[index] : null;
        }

        private static int IndexOfHeader(List<string> header, params string[] candidates)
        {
            for (int i = 0; i < header.Count; i++)
            {
                string normalized = header[i].Trim().ToLowerInvariant();
                foreach (string candidate in candidates)
                {
                    if (normalized == candidate)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private static List<List<string>> ParseRows(string csvText)
        {
            var rows = new List<List<string>>();
            var currentRow = new List<string>();
            var field = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char c = csvText[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < csvText.Length && csvText[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(c);
                    }

                    continue;
                }

                switch (c)
                {
                    case '"':
                        inQuotes = true;
                        break;
                    case ',':
                        currentRow.Add(field.ToString());
                        field.Clear();
                        break;
                    case '\r':
                        break;
                    case '\n':
                        currentRow.Add(field.ToString());
                        field.Clear();
                        rows.Add(currentRow);
                        currentRow = new List<string>();
                        break;
                    default:
                        field.Append(c);
                        break;
                }
            }

            // Flush trailing field/row if the text doesn't end with a newline.
            if (field.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(field.ToString());
                rows.Add(currentRow);
            }

            return rows;
        }
    }
}
