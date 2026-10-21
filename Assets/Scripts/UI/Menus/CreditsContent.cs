using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.UI.Common;

namespace Rapadura.UI.Menus
{
    /// <summary>
    /// One heading + its lines on the credits screen. <see cref="Title"/> is the pt-BR heading and
    /// the fallback of the optional <see cref="TitleKey"/> (see <see cref="GetTitle"/>); the lines are
    /// authored text (names, tools, licenses) and stay as written.
    /// </summary>
    public readonly struct CreditsSection
    {
        public readonly string TitleKey;
        public readonly string Title;
        public readonly IReadOnlyList<string> Lines;

        /// <param name="titleKey">UiTexts key of the heading, or null to always show <paramref name="title"/>.</param>
        public CreditsSection(string titleKey, string title, params string[] lines)
        {
            TitleKey = titleKey;
            Title = title;
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
    }

    /// <summary>
    /// Static text shown by <see cref="CreditsController"/>, kept as plain data (not in the UXML) so
    /// it can be unit-tested and kept in sync with Docs/ThirdPartyLicenses.md in one place. When an
    /// entry is added/removed in ThirdPartyLicenses.md, mirror it in <see cref="ThirdPartySection"/>.
    /// </summary>
    public static class CreditsContent
    {
        public const string StudioName = "Engenho Estúdio";
        public const string CreatorName = "Emerson Leandro Sanches";

        public static readonly string[] HeroNames = { "Maithe", "Maria", "Joaquim", "Ícaro", "Lavine" };

        public static readonly CreditsSection TeamSection = new CreditsSection(
            UiTexts.CreditsSectionTeam,
            "Equipe",
            StudioName,
            $"{CreatorName} — criador do jogo");

        public static readonly CreditsSection ToolsSection = new CreditsSection(
            UiTexts.CreditsSectionTools,
            "Ferramentas",
            "Unity 6 — Universal Render Pipeline, UI Toolkit, Input System, Addressables",
            "Meshy — modelos 3D e rigs dos personagens, inimigos e árvores (gerados por IA)",
            "FBX2glTF — conversão de modelos",
            "Claude Code (Anthropic) e MCP for Unity (Coplay) — assistência no desenvolvimento");

        public static readonly CreditsSection ThirdPartySection = new CreditsSection(
            UiTexts.CreditsSectionThirdParty,
            "Bibliotecas e licenças de terceiros",
            "Pacotes Unity (URP, Input System, Addressables, uGUI, TextMesh Pro) — Unity Companion License",
            "glTFast (com.unity.cloud.gltfast) — Apache License 2.0",
            "Modelos 3D gerados com Meshy — uso conforme os Termos de Serviço da Meshy",
            "Texturas de ambiente — geradas proceduralmente para o projeto",
            "Lista completa: Docs/ThirdPartyLicenses.md");

        public static readonly CreditsSection ThanksSection = new CreditsSection(
            UiTexts.CreditsSectionThanks,
            "Agradecimentos",
            $"A {JoinHeroNames()}, que carregam esta história nas costas.",
            "E a você, por caminhar com eles pelo sertão. Obrigado por jogar!");

        public static IReadOnlyList<CreditsSection> Sections { get; } = new[]
        {
            TeamSection,
            ToolsSection,
            ThirdPartySection,
            ThanksSection
        };

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
