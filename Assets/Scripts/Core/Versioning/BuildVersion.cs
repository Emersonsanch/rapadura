using UnityEngine;

namespace Rapadura.Core.Versioning
{
    /// <summary>
    /// Runtime view of the game's version (SemVer, see CHANGELOG.md). Source of truth is
    /// PlayerSettings.bundleVersion, exposed at runtime as <see cref="Application.version"/>.
    ///
    /// The project still ships Unity's placeholder bundleVersion "1.0" until PlayerSettings is set
    /// to the intended first public version (<see cref="IntendedVersion"/>); while that placeholder
    /// is present, <see cref="Current"/> reports <see cref="IntendedVersion"/> instead so credits,
    /// save metadata and bug reports never claim a "1.0" that was never released.
    /// </summary>
    public static class BuildVersion
    {
        /// <summary>First tracked version in CHANGELOG.md.</summary>
        public const string IntendedVersion = "0.1.0";

        /// <summary>Unity's default bundleVersion for new projects.</summary>
        public const string UnityPlaceholderVersion = "1.0";

        /// <summary>Normalized SemVer of the running build (e.g. "0.1.0").</summary>
        public static string Current => Resolve(Application.version);

        /// <summary>Label for UI (credits/menu footer): "v0.1.0", plus " (dev)" on development builds and in the Editor.</summary>
        public static string DisplayLabel => FormatLabel(Current, UnityEngine.Debug.isDebugBuild);

        /// <summary>
        /// Maps a raw bundleVersion to the version to report: empty/placeholder → <see cref="IntendedVersion"/>;
        /// "MAJOR.MINOR" → "MAJOR.MINOR.0"; valid "MAJOR.MINOR.PATCH" kept as-is; anything else returned trimmed.
        /// </summary>
        public static string Resolve(string rawVersion)
        {
            if (string.IsNullOrWhiteSpace(rawVersion))
            {
                return IntendedVersion;
            }

            string trimmed = rawVersion.Trim();
            if (trimmed == UnityPlaceholderVersion)
            {
                return IntendedVersion;
            }

            return TryParse(trimmed, out int major, out int minor, out int patch)
                ? $"{major}.{minor}.{patch}"
                : trimmed;
        }

        public static string FormatLabel(string version, bool isDevelopmentBuild)
        {
            return isDevelopmentBuild ? $"v{version} (dev)" : $"v{version}";
        }

        /// <summary>
        /// Parses "MAJOR.MINOR" or "MAJOR.MINOR.PATCH" (optional pre-release/build suffix after '-' or '+'
        /// is ignored). Returns false for anything else or negative parts.
        /// </summary>
        public static bool TryParse(string version, out int major, out int minor, out int patch)
        {
            major = minor = patch = 0;

            if (string.IsNullOrWhiteSpace(version))
            {
                return false;
            }

            string core = version.Trim();
            int suffix = core.IndexOfAny(new[] { '-', '+' });
            if (suffix >= 0)
            {
                core = core.Substring(0, suffix);
            }

            string[] parts = core.Split('.');
            if (parts.Length < 2 || parts.Length > 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out major) || !int.TryParse(parts[1], out minor))
            {
                return false;
            }

            if (parts.Length == 3 && !int.TryParse(parts[2], out patch))
            {
                return false;
            }

            return major >= 0 && minor >= 0 && patch >= 0;
        }

        /// <summary>Standard SemVer precedence on MAJOR.MINOR.PATCH (suffixes ignored): &lt;0 if a &lt; b, 0 if equal, &gt;0 if a &gt; b. Unparseable versions sort first.</summary>
        public static int Compare(string a, string b)
        {
            bool okA = TryParse(a, out int aMajor, out int aMinor, out int aPatch);
            bool okB = TryParse(b, out int bMajor, out int bMinor, out int bPatch);

            if (!okA || !okB)
            {
                return okA.CompareTo(okB);
            }

            if (aMajor != bMajor) return aMajor.CompareTo(bMajor);
            if (aMinor != bMinor) return aMinor.CompareTo(bMinor);
            return aPatch.CompareTo(bPatch);
        }
    }
}
