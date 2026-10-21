using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Rapadura.Gameplay.Player
{
    /// <summary>
    /// Plain-C# (de)serializer between a <see cref="ProgressionCurve"/> table and the designer-facing
    /// spreadsheet <c>Docs/Balance/progression.csv</c>. Kept in the runtime assembly (not the Editor
    /// one) so EditMode tests can round-trip it; the menu items live in
    /// <c>Rapadura.Editor.BalanceCsvMenu</c>.
    ///
    /// Format: comma separated, invariant culture (dot decimal separator), first line is the header.
    /// Columns are matched by header name, so a designer may reorder them; <c>xp_total</c> is
    /// informational (cumulative XP to reach the level) and ignored on import. Lines starting with
    /// <c>#</c> and blank lines are skipped.
    /// </summary>
    public static class ProgressionCsv
    {
        public const string ColLevel = "level";
        public const string ColXpToNext = "xp_to_next";
        public const string ColXpTotal = "xp_total";
        public const string ColHp = "hp";
        public const string ColMana = "mana";
        public const string ColStamina = "stamina";
        public const string ColDamageMultiplier = "damage_mult";
        public const string ColAttributePoints = "attribute_points";
        public const string ColSkillPoints = "skill_points";

        public static readonly string[] Header =
        {
            ColLevel, ColXpToNext, ColXpTotal, ColHp, ColMana, ColStamina, ColDamageMultiplier, ColAttributePoints, ColSkillPoints
        };

        private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        /// <summary>Exports levels 1..maxLevel of the curve (table rows, falling back to the formula past the table).</summary>
        public static string Export(ProgressionCurve curve, int maxLevel = ProgressionCurve.DefaultMaxLevel)
        {
            if (curve == null)
            {
                throw new ArgumentNullException(nameof(curve));
            }

            var rows = new List<ProgressionLevelRow>();
            for (int level = 1; level <= maxLevel; level++)
            {
                rows.Add(curve.GetRow(level));
            }

            return Export(rows);
        }

        public static string Export(IReadOnlyList<ProgressionLevelRow> rows)
        {
            var sb = new StringBuilder();
            sb.Append(string.Join(",", Header)).Append('\n');

            long total = 0;
            foreach (ProgressionLevelRow row in rows)
            {
                sb.Append(row.level.ToString(Invariant)).Append(',')
                    .Append(row.experienceToNext.ToString(Invariant)).Append(',')
                    .Append(total.ToString(Invariant)).Append(',')
                    .Append(FormatFloat(row.maxHealth)).Append(',')
                    .Append(FormatFloat(row.maxMana)).Append(',')
                    .Append(FormatFloat(row.maxStamina)).Append(',')
                    .Append(FormatFloat(row.damageMultiplier)).Append(',')
                    .Append(row.attributePoints.ToString(Invariant)).Append(',')
                    .Append(row.skillPoints.ToString(Invariant)).Append('\n');
                total += row.experienceToNext;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Parses CSV text into rows. Throws <see cref="FormatException"/> with the offending line
        /// number on any malformed value or missing required column.
        /// </summary>
        public static List<ProgressionLevelRow> Import(string csv)
        {
            if (csv == null)
            {
                throw new ArgumentNullException(nameof(csv));
            }

            var result = new List<ProgressionLevelRow>();
            Dictionary<string, int> columns = null;
            string[] lines = csv.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0 || line[0] == '#')
                {
                    continue;
                }

                List<string> cells = CsvUtility.SplitLine(line);

                if (columns == null)
                {
                    columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                    for (int c = 0; c < cells.Count; c++)
                    {
                        columns[cells[c].Trim()] = c;
                    }

                    foreach (string required in new[] { ColLevel, ColXpToNext })
                    {
                        if (!columns.ContainsKey(required))
                        {
                            throw new FormatException($"progression.csv: missing required column '{required}'.");
                        }
                    }

                    continue;
                }

                int lineNumber = i + 1;
                var row = new ProgressionLevelRow
                {
                    level = ReadInt(cells, columns, ColLevel, 1, lineNumber),
                    experienceToNext = ReadInt(cells, columns, ColXpToNext, 100, lineNumber),
                    maxHealth = ReadFloat(cells, columns, ColHp, 100f, lineNumber),
                    maxMana = ReadFloat(cells, columns, ColMana, 100f, lineNumber),
                    maxStamina = ReadFloat(cells, columns, ColStamina, 100f, lineNumber),
                    damageMultiplier = ReadFloat(cells, columns, ColDamageMultiplier, 1f, lineNumber),
                    attributePoints = ReadInt(cells, columns, ColAttributePoints, 0, lineNumber),
                    skillPoints = ReadInt(cells, columns, ColSkillPoints, 0, lineNumber)
                };
                result.Add(row);
            }

            return result;
        }

        /// <summary>"R" round-trips every float exactly while still printing short values like "100" or "1.05".</summary>
        public static string FormatFloat(float value)
        {
            return value.ToString("R", Invariant);
        }

        private static string Cell(List<string> cells, Dictionary<string, int> columns, string name)
        {
            if (!columns.TryGetValue(name, out int index) || index >= cells.Count)
            {
                return null;
            }

            string value = cells[index].Trim();
            return value.Length == 0 ? null : value;
        }

        private static int ReadInt(List<string> cells, Dictionary<string, int> columns, string name, int fallback, int lineNumber)
        {
            string value = Cell(cells, columns, name);
            if (value == null)
            {
                return fallback;
            }

            // Spreadsheets love to turn "2" into "2.0"; accept integral decimals too.
            if (int.TryParse(value, NumberStyles.Integer, Invariant, out int parsed))
            {
                return parsed;
            }

            if (double.TryParse(value, NumberStyles.Float, Invariant, out double asDouble) && Math.Abs(asDouble - Math.Round(asDouble)) < 1e-9)
            {
                return (int)Math.Round(asDouble);
            }

            throw new FormatException($"progression.csv line {lineNumber}: column '{name}' expects an integer but got '{value}'.");
        }

        private static float ReadFloat(List<string> cells, Dictionary<string, int> columns, string name, float fallback, int lineNumber)
        {
            string value = Cell(cells, columns, name);
            if (value == null)
            {
                return fallback;
            }

            if (float.TryParse(value, NumberStyles.Float, Invariant, out float parsed))
            {
                return parsed;
            }

            throw new FormatException($"progression.csv line {lineNumber}: column '{name}' expects a number but got '{value}'.");
        }
    }

    /// <summary>Minimal RFC-4180-style CSV helpers shared by the balance exporters.</summary>
    public static class CsvUtility
    {
        /// <summary>Quotes a cell if it contains a comma, quote or newline.</summary>
        public static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) < 0)
            {
                return value;
            }

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        /// <summary>Splits a single CSV line, honouring double-quoted cells with escaped quotes.</summary>
        public static List<string> SplitLine(string line)
        {
            var cells = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current.Append(ch);
                    }
                }
                else if (ch == '"')
                {
                    inQuotes = true;
                }
                else if (ch == ',')
                {
                    cells.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            cells.Add(current.ToString());
            return cells;
        }
    }
}
