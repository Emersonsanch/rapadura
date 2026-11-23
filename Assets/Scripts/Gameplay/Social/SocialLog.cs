using System.Text;

namespace Rapadura.Gameplay.Social
{
    /// <summary>Formatting shared by the log lines of the chat, the reports and the staff commands (stable and invariant).</summary>
    internal static class SocialLog
    {
        /// <summary>An id for a "key=value" pair: "-" when empty, spaces and '=' replaced so the line stays splittable.</summary>
        internal static string Token(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "-";
            }

            var sb = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                sb.Append(char.IsWhiteSpace(c) || c == '=' ? '_' : c);
            }

            return sb.ToString();
        }

        /// <summary>A free text in double quotes, with '\' and '"' escaped and line breaks as "\n".</summary>
        internal static string Quote(string value)
        {
            var sb = new StringBuilder((value?.Length ?? 0) + 2);
            sb.Append('"');
            if (value != null)
            {
                foreach (char c in value)
                {
                    switch (c)
                    {
                        case '\\': sb.Append("\\\\"); break;
                        case '"': sb.Append("\\\""); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        default: sb.Append(c); break;
                    }
                }
            }

            sb.Append('"');
            return sb.ToString();
        }

        /// <summary>"RecipientUnavailable" → "recipient_unavailable".</summary>
        internal static string Snake(string pascal)
        {
            var builder = new StringBuilder(pascal.Length + 8);
            for (int i = 0; i < pascal.Length; i++)
            {
                char c = pascal[i];
                if (char.IsUpper(c))
                {
                    if (i > 0)
                    {
                        builder.Append('_');
                    }

                    builder.Append(char.ToLowerInvariant(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }
    }
}
