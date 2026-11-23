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

        // The beaten Kul lets the fish go (KulFreedLake).
        public const string LakeFreed = "ui.ice_bait.lake_freed";
        public const string FreeCast = "ui.ice_bait.free_cast";
        public const string ExtraFish = "ui.ice_bait.extra_fish";
        public const string ExtraFishBagFull = "ui.ice_bait.extra_fish_bag_full";

        private const string CalledMaithe = "ui.ice_bait.called_maithe";
        private const string CalledIcaro = "ui.ice_bait.called_icaro";
        private const string CalledJoaquim = "ui.ice_bait.called_joaquim";
        private const string CalledMaria = "ui.ice_bait.called_maria";
        private const string CalledLavine = "ui.ice_bait.called_lavine";
        private const string FreedMaithe = "ui.ice_bait.freed_maithe";
        private const string FreedIcaro = "ui.ice_bait.freed_icaro";
        private const string FreedJoaquim = "ui.ice_bait.freed_joaquim";
        private const string FreedMaria = "ui.ice_bait.freed_maria";
        private const string FreedLavine = "ui.ice_bait.freed_lavine";

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
            new LocalizationEntry(LakeFreed, "The Old Kul lets go of the fish: the whole lake is free for {0} min, and you are the masters of the fishing",
                "O Velho Kul solta os peixes: o lago todo fica livre por {0} min, e vocês são os donos da pesca"),
            new LocalizationEntry(FreeCast, "Free lake: the better fish come up and the holes rest less ({0} min left)",
                "Lago livre: os peixes bons sobem e os buracos descansam menos (faltam {0} min)"),
            new LocalizationEntry(ExtraFish, "Master of the fishing: another {0} came up with the first!", "Dono da pesca: veio outro {0} junto com o primeiro!"),
            new LocalizationEntry(ExtraFishBagFull, "Master of the fishing: another {0} came up, but the bag is full", "Dono da pesca: veio outro {0}, mas a mochila está cheia"),
            new LocalizationEntry(FreedMaithe, "Maithe: Look how many! They were all stuck down there with him.", "Maithe: Olha quantos! Estavam todos presos lá embaixo com ele."),
            new LocalizationEntry(FreedIcaro, "Ícaro: He held the fish the way the dark holds the light. Let go, both shine.", "Ícaro: Ele segurava os peixes como a treva segura a luz. Soltou, os dois brilham."),
            new LocalizationEntry(FreedJoaquim, "Joaquim: Vô Jorge would have fished here all afternoon. I'm staying.", "Joaquim: O Vô Jorge pescaria aqui a tarde inteira. Eu vou ficar."),
            new LocalizationEntry(FreedMaria, "Maria: The lake is ours now. Lines in, everyone!", "Maria: Agora o lago é nosso. Linha na água, todo mundo!"),
            new LocalizationEntry(FreedLavine, "Lavine: The holes are glowing silver. That's the fish, breathing again.", "Lavine: Os buracos brilham de prata. São os peixes respirando de novo."),
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

        /// <summary>The cousin's line when the beaten Kul lets the fish go (null when no cousin is known).</summary>
        public static string FreedLineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return FreedMaithe;
                case CharacterId.Icaro: return FreedIcaro;
                case CharacterId.Joaquim: return FreedJoaquim;
                case CharacterId.Maria: return FreedMaria;
                case CharacterId.Lavine: return FreedLavine;
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
