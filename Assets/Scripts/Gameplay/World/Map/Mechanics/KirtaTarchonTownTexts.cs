using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.World.Map.Mechanics
{
    /// <summary>
    /// Localization (en + pt-BR) of what the towns of Kirta (28) and Tarchon (33) do with the reef's corals and the woken neighbours
    /// (<see cref="KirtaMotherCoralRules"/>, <see cref="TarchonCrowdRules"/>, <see cref="LarthiaWispRules"/>, <see cref="TarchonDailyRules"/>): the
    /// mother coral's news (Kirta and Pughat, Ugaritic names of the epics), the six trols who come home (Aule, Seianti, Thana, Sethre, Hasti
    /// and Tite, Etruscan praenomina), Larthia buying the freed wisps with one line per cousin, and old Tarchon's daily. Same self-contained
    /// pattern as <see cref="TarchonLanternTexts"/>.
    /// </summary>
    public static class KirtaTarchonTownTexts
    {
        public const string Mother1 = "ui.kirta_town.mother_1";
        public const string Mother2 = "ui.kirta_town.mother_2";
        public const string Mother3 = "ui.kirta_town.mother_3";
        public const string Mother4 = "ui.kirta_town.mother_4";
        public const string Mother5 = "ui.kirta_town.mother_5";

        public const string CrowdBack = "ui.tarchon_town.crowd_back";
        public const string Aule = "ui.tarchon_town.aule";
        public const string Seianti = "ui.tarchon_town.seianti";
        public const string Thana = "ui.tarchon_town.thana";
        public const string Sethre = "ui.tarchon_town.sethre";
        public const string Hasti = "ui.tarchon_town.hasti";
        public const string Tite = "ui.tarchon_town.tite";

        public const string WispFreed = "ui.tarchon_town.wisp_freed";
        public const string WispsFull = "ui.tarchon_town.wisps_full";
        public const string LarthiaPrompt = "ui.tarchon_town.larthia_prompt";
        public const string LarthiaSold = "ui.tarchon_town.larthia_sold";
        public const string LarthiaGift = "ui.tarchon_town.larthia_gift";

        public const string SellMaithe = "ui.tarchon_town.sell_maithe";
        public const string SellIcaro = "ui.tarchon_town.sell_icaro";
        public const string SellJoaquim = "ui.tarchon_town.sell_joaquim";
        public const string SellMaria = "ui.tarchon_town.sell_maria";
        public const string SellLavine = "ui.tarchon_town.sell_lavine";

        public const string DailyOffer = "ui.tarchon_town.daily_offer";
        public const string DailyDone = "ui.tarchon_town.daily_done";

        private static List<LocalizationEntry> _entries;
        private static Dictionary<string, string> _fallback;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= new List<LocalizationEntry>
        {
            new LocalizationEntry(Mother1, "Pughat: Look, the mother coral! One more branch turned pink, the one the reef coral you lit was feeding.", "Pughat: Olha o coral-mãe! Mais um galho ficou rosa, o que o coral do recife que vocês acenderam alimentava."),
            new LocalizationEntry(Mother2, "Pughat: Two corals of the reef, two new branches. My father would have counted them with me.", "Pughat: Dois corais do recife, dois galhos novos. Meu pai teria contado com a gente."),
            new LocalizationEntry(Mother3, "Kirta: Three. The grey is retreating west, like the tide. The divers swim slower past it now, smiling.", "Kirta: Três. O cinza recua para o oeste, como a maré. Os mergulhadores passam devagar por ele agora, sorrindo."),
            new LocalizationEntry(Mother4, "Kirta: Every branch is alive again. Only the old trunk still remembers the Lodo.", "Kirta: Todos os galhos vivos de novo. Só o tronco velho ainda lembra do Lodo."),
            new LocalizationEntry(Mother5, "Kirta: Whole. Trunk, branches and rim. I lost my family to the sea and I never thought I'd see a thing come back from it. Thank you.", "Kirta: Inteiro. Tronco, galhos e borda. Perdi minha família para o mar e nunca pensei ver algo voltar dele. Obrigado."),

            new LocalizationEntry(CrowdBack, "A trol came home to the stilts ({0}/{1})", "Um trol voltou para casa nas palafitas ({0}/{1})"),
            new LocalizationEntry(Aule, "Aule: I followed the wisps all the way to the reeds... and the neighbours' lanterns showed me the way back.", "Aule: Eu segui os fogos-fátuos até o junco... e as lanternas dos vizinhos me mostraram o caminho de volta."),
            new LocalizationEntry(Seianti, "Seianti: My loom is still here! Velia kept the thread for me.", "Seianti: Meu tear ainda está aqui! A Velia guardou o fio para mim."),
            new LocalizationEntry(Thana, "Thana: Someone woke my brother, so I came to see who. Was it you? Then I'm staying.", "Thana: Alguém acordou meu irmão, então vim ver quem foi. Foi você? Então eu fico."),
            new LocalizationEntry(Sethre, "Sethre: The planks creak with feet again. That is the sound of a town.", "Sethre: As tábuas rangem com pés de novo. Esse é o som de uma cidade."),
            new LocalizationEntry(Hasti, "Hasti: I'll keep the east boardwalk. Whoever comes in after dark gets a light from me.", "Hasti: Eu cuido da passarela do leste. Quem chegar depois do escuro ganha uma luz minha."),
            new LocalizationEntry(Tite, "Tite: Grandpa said Tarchon would fill up again one day. He said it slowly, but he was right.", "Tite: O vô dizia que Tarchon ia encher de novo um dia. Ele dizia devagar, mas estava certo."),

            new LocalizationEntry(WispFreed, "The wisp that held the neighbour goes free and follows you ({0}/{1}). Larthia buys them", "O fogo-fátuo que prendia o vizinho se solta e segue você ({0}/{1}). A Larthia compra"),
            new LocalizationEntry(WispsFull, "You already carry ten wisps: this one rises to the sky", "Você já leva dez fogos-fátuos: este sobe para o céu"),
            new LocalizationEntry(LarthiaPrompt, "[E] Sell {0} freed wisps to Larthia (+{1} gold)", "[E] Vender {0} fogos-fátuos libertados à Larthia (+{1} de ouro)"),
            new LocalizationEntry(LarthiaSold, "Larthia: Into the lantern basket they go. Light that sat in a reed cage deserves a glass house. +{0} gold", "Larthia: Para o cesto de lanternas. Luz que ficou presa no junco merece uma casa de vidro. +{0} de ouro"),
            new LocalizationEntry(LarthiaGift, "Larthia: Twenty wisps! With the twentieth I wove one lantern just for you. Keep it in the bag: the lilac mist won't hide a thing.", "Larthia: Vinte fogos-fátuos! Com o vigésimo eu trancei uma lanterna só para vocês. Guardem na bolsa: a névoa lilás não esconde mais nada."),

            new LocalizationEntry(SellMaithe, "Maithe: Vó Edna keeps fireflies in a jar for one night only, then lets them go. Larthia's are going somewhere better.", "Maithe: A Vó Edna guarda vaga-lumes num pote só por uma noite, depois solta. Os da Larthia vão para um lugar melhor."),
            new LocalizationEntry(SellIcaro, "Ícaro: A wisp in a lantern lights a house all night. That's a good trade for everyone, the wisp included.", "Ícaro: Um fogo-fátuo na lanterna ilumina uma casa a noite toda. Bom negócio para todo mundo, o fogo-fátuo também."),
            new LocalizationEntry(SellJoaquim, "Joaquim: Vô Jorge never sold a thing without a story on top. Larthia gave us one for free.", "Joaquim: O Vô Jorge nunca vendeu nada sem uma história de troco. A Larthia deu a dela de graça."),
            new LocalizationEntry(SellMaria, "Maria: Bye, little lights! Shine on somebody's window!", "Maria: Tchau, luzinhas! Brilhem na janela de alguém!"),
            new LocalizationEntry(SellLavine, "Lavine: Vô Jorge says a light you hand on is never lost. I think he'd like this basket.", "Lavine: O Vô Jorge diz que luz que a gente passa adiante nunca se perde. Acho que ele ia gostar desse cesto."),

            new LocalizationEntry(DailyOffer, "Tarchon: The night dimmed some of them again... Wake three neighbours today, little ones. Slowly, with the mast's light.", "Tarchon: A noite apagou alguns de novo... Acordem três vizinhos hoje, pequenos. Devagar, com a luz do mastro."),
            new LocalizationEntry(DailyDone, "Tarchon: Three awake. The stilts breathe easier. Come back tomorrow: the wisps never rest, and neither does a neighbour.", "Tarchon: Três acordados. As palafitas respiram melhor. Voltem amanhã: os fogos-fátuos não descansam, e vizinho também não."),
        };

        /// <summary>The mother coral's line for the <paramref name="coralsLit"/>-th coral of the reef (1–5).</summary>
        public static string MotherLine(int coralsLit)
        {
            switch (KirtaMotherCoralRules.Clamp(coralsLit))
            {
                case 1: return Mother1;
                case 2: return Mother2;
                case 3: return Mother3;
                case 4: return Mother4;
                default: return Mother5;
            }
        }

        /// <summary>The line of a trol who came home (ids of <c>CityTownRecipes.TarchonCrowdSpots</c>).</summary>
        public static string CrowdLine(string id)
        {
            switch (id)
            {
                case "aule": return Aule;
                case "seianti": return Seianti;
                case "thana": return Thana;
                case "sethre": return Sethre;
                case "hasti": return Hasti;
                default: return Tite;
            }
        }

        /// <summary>The cousin's line when Larthia buys the wisps (Joaquim's when no cousin is known).</summary>
        public static string SellLineFor(CharacterId? hero)
        {
            switch (hero)
            {
                case CharacterId.Maithe: return SellMaithe;
                case CharacterId.Icaro: return SellIcaro;
                case CharacterId.Maria: return SellMaria;
                case CharacterId.Lavine: return SellLavine;
                default: return SellJoaquim;
            }
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                if (!manager.HasKey(Mother1))
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

        public static string Format(LocalizationManager manager, string key, params object[] args) => string.Format(Get(manager, key), args);
    }
}
