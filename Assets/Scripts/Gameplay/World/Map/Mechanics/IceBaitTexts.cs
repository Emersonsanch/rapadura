using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of the baits of Old Kul's lake and the silver lure's call (64, <see cref="IceBaitRules"/>, <see cref="KulLureCall"/>).
    /// Pamour, of the Porto da Lua Branca, says the rule; the cousins say one line each when the Kul comes up. Same self-contained pattern as
    /// <see cref="HinokuraPuzzleTexts"/>.
    /// </summary>
    public static class IceBaitTexts
    {
        public const string Rule = "ui.ice_bait.rule";
        public const string GrubOn = "ui.ice_bait.grub_on";
        public const string SilverOn = "ui.ice_bait.silver_on";
        public const string Tug = "ui.ice_bait.tug";
        public const string Called = "ui.ice_bait.called";
        public const string KulHere = "ui.ice_bait.kul_here";
        public const string Recharging = "ui.ice_bait.recharging";

        private const string CalledMaithe = "ui.ice_bait.called_maithe";
        private const string CalledIcaro = "ui.ice_bait.called_icaro";
        private const string CalledJoaquim = "ui.ice_bait.called_joaquim";
        private const string CalledMaria = "ui.ice_bait.called_maria";
        private const string CalledLavine = "ui.ice_bait.called_lavine";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Rule, "Pamour: \"Three fish on the silver lure in the Great Hole, and the old one comes up to see who is fishing his lake. Only once he has gone down, mind.\"",
                "Pamour: \"Três peixes na isca de prata no Buraco Grande, e o velho sobe para ver quem pesca no lago dele. Só depois que ele afundou, viu?\""),
            new LocalizationEntry(GrubOn, "Ice grub on the hook: the fish bite sooner", "Larva do gelo no anzol: o peixe morde mais cedo"),
            new LocalizationEntry(SilverOn, "Kul's silver lure on the hook: the dark water stirs", "Isca de prata do Kul no anzol: a água negra se mexe"),
            new LocalizationEntry(Tug, "Something big tugs at the silver below the fish: {0} of {1}", "Algo grande puxa a prata debaixo do peixe: {0} de {1}"),
            new LocalizationEntry(Called, "The ice groans: the Old Kul comes up to his stone ring!", "O gelo geme: o Velho Kul sobe para o anel de pedras!"),
            new LocalizationEntry(KulHere, "The Old Kul is on the ice already: the silver stays in the bag", "O Velho Kul já está no gelo: a prata fica na mochila"),
            new LocalizationEntry(Recharging, "The Old Kul is still sulking down there: he listens to the silver again in {0} min", "O Velho Kul ainda está emburrado lá no fundo: volta a ouvir a prata em {0} min"),
            new LocalizationEntry(CalledMaithe, "Maithe: He's grumpy because he's lonely. Nobody fishes WITH him.", "Maithe: Ele é rabugento porque é sozinho. Ninguém pesca COM ele."),
            new LocalizationEntry(CalledIcaro, "Ícaro: Three tugs, three fish, one old spirit. The lake keeps count better than I do.", "Ícaro: Três puxões, três peixes, um espírito velho. O lago conta melhor que eu."),
            new LocalizationEntry(CalledJoaquim, "Joaquim: Vô Jorge always said: the fish you don't let go is the one that drags you in.", "Joaquim: O Vô Jorge sempre dizia: peixe que você não solta é o que te puxa para a água."),
            new LocalizationEntry(CalledMaria, "Maria: You wanted the silver back? Come and get it, old man!", "Maria: Queria a prata de volta? Vem buscar, velho!"),
            new LocalizationEntry(CalledLavine, "Lavine: His light went out under the ice. Let's give the fish back theirs.", "Lavine: A luz dele apagou debaixo do gelo. Vamos devolver a dos peixes."),
        };

        /// <summary>The cousin's line when the Kul comes up (null when no cousin is known).</summary>
        public static string CalledLineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return CalledMaithe;
                case CharacterId.Icaro: return CalledIcaro;
                case CharacterId.Joaquim: return CalledJoaquim;
                case CharacterId.Maria: return CalledMaria;
                case CharacterId.Lavine: return CalledLavine;
                default: return null;
            }
        }

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Rule))
                {
                    manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
                }

                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            _fallback ??= Entries.ToDictionary(entry => entry.Key, entry => entry.PtBR);
            return key != null && _fallback.TryGetValue(key, out string text) ? text : key;
        }
    }
}
