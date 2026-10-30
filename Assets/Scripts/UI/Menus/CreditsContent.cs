using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.UI.Common;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// One heading + its lines on the credits screen. <see cref="Title"/> is the pt-BR heading and
    /// the fallback of the optional <see cref="TitleKey"/> (see <see cref="GetTitle"/>). Every line is a
    /// <c>ui.credits.line.*</c> key of <see cref="UiTexts"/> (<see cref="LineKeys"/>); <see cref="Lines"/>
    /// holds their pt-BR text, and <see cref="GetLine"/> resolves one in the current language.
    /// </summary>
    public readonly struct CreditsSection
    {
        public readonly string TitleKey;
        public readonly string Title;
        public readonly IReadOnlyList<string> LineKeys;
        public readonly IReadOnlyList<string> Lines;

        /// <param name="titleKey">UiTexts key of the heading, or null to always show <paramref name="title"/>.</param>
        /// <param name="lineKeys">UiTexts keys of the lines, in order.</param>
        public CreditsSection(string titleKey, string title, params string[] lineKeys)
        {
            TitleKey = titleKey;
            Title = title;
            LineKeys = lineKeys;

            var lines = new string[lineKeys.Length];
            for (int i = 0; i < lineKeys.Length; i++)
            {
                lines[i] = UiTexts.GetFallback(lineKeys[i]) ?? lineKeys[i];
            }

            Lines = lines;
        }

        /// <summary>The heading in the current language (pt-BR <see cref="Title"/> without a key/manager).</summary>
        public string GetTitle(LocalizationManager localization)
        {
            if (string.IsNullOrEmpty(TitleKey))
            {
                return Title;
            }

            string text = UiTexts.Get(localization, TitleKey);
            return string.IsNullOrEmpty(text) || text == TitleKey ? Title : text;
        }

        /// <summary>Line <paramref name="index"/> in the current language (its pt-BR text without a manager).</summary>
        public string GetLine(LocalizationManager localization, int index)
        {
            string key = LineKeys[index];
            string text = UiTexts.Get(localization, key);
            return string.IsNullOrEmpty(text) || text == key ? Lines[index] : text;
        }
    }

    /// <summary>
    /// Text shown by <see cref="CreditsController"/>, kept as plain data (not in the UXML) so it can be
    /// unit-tested and kept in sync with Docs/ThirdPartyLicenses.md in one place. When an entry is
    /// added/removed in ThirdPartyLicenses.md, mirror it in <see cref="ThirdPartySection"/> (and add the
    /// en + pt-BR line to <see cref="UiTexts"/>). The roll ends on the family back on Vó Edna's porch,
    /// right after the campaign epilogue (<c>story.ending.4</c>).
    /// </summary>
    public static class CreditsContent
    {
        public const string StudioName = "Engenho Estúdio";
        public const string CreatorName = "Emerson Leandro Sanches";

        /// <summary>Attribution the CMU Graphics Lab asks for (Docs/Mocap.md, Docs/ThirdPartyLicenses.md §3).</summary>
        public const string CmuAttribution = "Motion capture data from mocap.cs.cmu.edu (NSF EIA-0196217)";

        public static readonly string[] HeroNames = { "Maithe", "Maria", "Joaquim", "Ícaro", "Lavine" };

        public static readonly CreditsSection TeamSection = new CreditsSection(
            UiTexts.CreditsSectionTeam,
            "Equipe",
            UiTexts.CreditsLineStudio,
            UiTexts.CreditsLineCreator);

        /// <summary>The five cousins, credited as the characters, in birth order.</summary>
        public static readonly CreditsSection CastSection = new CreditsSection(
            UiTexts.CreditsSectionCast,
            "Os primos",
            UiTexts.CreditsLineMaithe,
            UiTexts.CreditsLineMaria,
            UiTexts.CreditsLineJoaquim,
            UiTexts.CreditsLineIcaro,
            UiTexts.CreditsLineLavine);

        public static readonly CreditsSection ToolsSection = new CreditsSection(
            UiTexts.CreditsSectionTools,
            "Ferramentas",
            UiTexts.CreditsLineUnity,
            UiTexts.CreditsLineUnityPackages,
            UiTexts.CreditsLineMeshyTool,
            UiTexts.CreditsLineMocapTool,
            UiTexts.CreditsLineFbx2Gltf,
            UiTexts.CreditsLineMcpForUnity,
            UiTexts.CreditsLineClaudeCode);

        public static readonly CreditsSection ThirdPartySection = new CreditsSection(
            UiTexts.CreditsSectionThirdParty,
            "Bibliotecas e licenças de terceiros",
            UiTexts.CreditsLineCmuDatabase,
            UiTexts.CreditsLineCmuAttribution,
            UiTexts.CreditsLineCmuConversion,
            UiTexts.CreditsLineMeshyAssets,
            UiTexts.CreditsLineUnityLicense,
            UiTexts.CreditsLineGltfast,
            UiTexts.CreditsLineNewtonsoft,
            UiTexts.CreditsLineTextures,
            UiTexts.CreditsLineFonts,
            UiTexts.CreditsLineFullList);

        public static readonly CreditsSection ThanksSection = new CreditsSection(
            UiTexts.CreditsSectionThanks,
            "Agradecimentos",
            UiTexts.CreditsLineThanksCousins,
            UiTexts.CreditsLineThanksPlayer);

        /// <summary>Last words of the roll: the family on Vó Edna's porch, as in the epilogue.</summary>
        public static readonly CreditsSection ClosingSection = new CreditsSection(
            UiTexts.CreditsSectionClosing,
            "Até a próxima",
            UiTexts.CreditsLineClosing);

        public static IReadOnlyList<CreditsSection> Sections { get; } = new[]
        {
            TeamSection,
            CastSection,
            ToolsSection,
            ThirdPartySection,
            ThanksSection,
            ClosingSection
        };

        /// <summary>Every key the credits screen shows: headings, lines, title, back button and skip hint.</summary>
        public static IEnumerable<string> AllKeys()
        {
            yield return UiTexts.CreditsTitle;
            yield return UiTexts.CreditsBack;
            yield return UiTexts.CreditsSkipHint;
            foreach (CreditsSection section in Sections)
            {
                yield return section.TitleKey;
                foreach (string key in section.LineKeys)
                {
                    yield return key;
                }
            }
        }

        /// <summary>"Maithe, Maria, Joaquim, Ícaro e Lavine" (birth order, see GDD).</summary>
        public static string JoinHeroNames()
        {
            if (HeroNames.Length == 1)
            {
                return HeroNames[0];
            }

            return string.Join(", ", HeroNames, 0, HeroNames.Length - 1) + " e " + HeroNames[HeroNames.Length - 1];
        }
    }
}
