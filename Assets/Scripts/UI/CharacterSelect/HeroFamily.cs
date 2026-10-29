using System.Collections.Generic;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Content;
using Rapadura.UI.Common;

namespace Rapadura.UI.CharacterSelect
{
    /// <summary>
    /// The heroes' canonical family tree (Docs/GDD.md, "Os 5 personagens jogáveis são todos primos"):
    /// Maithe &amp; Ícaro are siblings, Maria &amp; Joaquim are siblings, Lavine is cousin to all four;
    /// birth order Maithe &gt; Maria &gt; Joaquim &gt; Ícaro &gt; Lavine (matches
    /// <see cref="PlayableCharacter.Age"/>). Used by the hero select's family line
    /// ("Irmã do Ícaro · a mais velha"); the text rules are pure so EditMode tests cover them.
    /// Keys live in <see cref="UiTexts"/> under <see cref="UiTexts.CharacterSelectFamilyPrefix"/>.
    /// </summary>
    public static class HeroFamily
    {
        /// <summary>Separator between the relation and the birth rank in <see cref="BuildFamilyLine"/>.</summary>
        public const string Separator = " · ";

        /// <summary>Eldest first.</summary>
        public static readonly IReadOnlyList<CharacterId> BirthOrder = new[]
        {
            CharacterId.Maithe,
            CharacterId.Maria,
            CharacterId.Joaquim,
            CharacterId.Icaro,
            CharacterId.Lavine,
        };

        /// <summary>1 = eldest … 5 = youngest (0 for an id outside the roster).</summary>
        public static int BirthRank(CharacterId id)
        {
            for (int i = 0; i < BirthOrder.Count; i++)
            {
                if (BirthOrder[i] == id)
                {
                    return i + 1;
                }
            }

            return 0;
        }

        /// <summary>Maithe, Maria and Lavine are girls; Joaquim and Ícaro are boys (drives pt-BR "Irmã/Irmão", "do/da").</summary>
        public static bool IsFemale(CharacterId id) =>
            id == CharacterId.Maithe || id == CharacterId.Maria || id == CharacterId.Lavine;

        /// <summary>The hero's sibling; false for Lavine (the cousin, no sibling in the party).</summary>
        public static bool TryGetSibling(CharacterId id, out CharacterId sibling)
        {
            switch (id)
            {
                case CharacterId.Maithe: sibling = CharacterId.Icaro; return true;
                case CharacterId.Icaro: sibling = CharacterId.Maithe; return true;
                case CharacterId.Maria: sibling = CharacterId.Joaquim; return true;
                case CharacterId.Joaquim: sibling = CharacterId.Maria; return true;
                default: sibling = default; return false;
            }
        }

        /// <summary>Gender suffix of the family keys: "f" or "m".</summary>
        private static string GenderSuffix(CharacterId id) => IsFemale(id) ? "f" : "m";

        /// <summary>"Irmã do Ícaro" / "Ícaro's sister", or "Prima de todos" for Lavine.</summary>
        public static string BuildRelationText(CharacterId id, LocalizationManager manager)
        {
            if (!TryGetSibling(id, out CharacterId sibling))
            {
                return UiTexts.Get(manager, UiTexts.CharacterSelectFamilyPrefix + "cousin." + GenderSuffix(id));
            }

            // "sister_of" / "brother_of" = this hero's gender; ".m"/".f" = the sibling's (do Ícaro / da Maithe).
            string relation = IsFemale(id) ? "sister_of." : "brother_of.";
            string siblingName = CharacterRegistry.Get(sibling).LocalizedName(manager);
            return UiTexts.Format(manager, UiTexts.CharacterSelectFamilyPrefix + relation + GenderSuffix(sibling), siblingName);
        }

        /// <summary>"a mais velha", "o do meio", "a caçula"… (empty for an unknown id).</summary>
        public static string BuildRankText(CharacterId id, LocalizationManager manager)
        {
            int rank = BirthRank(id);
            return rank <= 0
                ? string.Empty
                : UiTexts.Get(manager, UiTexts.CharacterSelectFamilyPrefix + "rank." + rank + "." + GenderSuffix(id));
        }

        /// <summary>Full family line: "Irmã do Ícaro · a mais velha" / "Ícaro's sister · the eldest".</summary>
        public static string BuildFamilyLine(CharacterId id, LocalizationManager manager)
        {
            string relation = BuildRelationText(id, manager);
            string rank = BuildRankText(id, manager);
            if (string.IsNullOrEmpty(rank))
            {
                return relation;
            }

            return string.IsNullOrEmpty(relation) ? rank : relation + Separator + rank;
        }

        /// <summary>Every key <see cref="BuildFamilyLine"/> can ask for (coverage tests).</summary>
        public static IEnumerable<string> AllKeys()
        {
            string prefix = UiTexts.CharacterSelectFamilyPrefix;
            foreach (string gender in new[] { "f", "m" })
            {
                yield return prefix + "sister_of." + gender;
                yield return prefix + "brother_of." + gender;
                yield return prefix + "cousin." + gender;
                for (int rank = 1; rank <= BirthOrder.Count; rank++)
                {
                    yield return prefix + "rank." + rank + "." + gender;
                }
            }
        }
    }
}
