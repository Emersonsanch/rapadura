using System.Collections.Generic;
using System.Linq;
using Rapadura.Core.Localization;

namespace Rapadura.Gameplay.Characters
{
    /// <summary>
    /// Names and one-line summaries (en + pt-BR) of the 25 peoples of <see cref="RaceCatalog"/>
    /// (Docs/MetodologiaMapasERacas.md Part III). Same self-contained pattern as <c>CityWalkTexts</c>.
    /// </summary>
    public static class RaceTexts
    {
        private static readonly (string Id, string NameEn, string NamePt, string SummaryEn, string SummaryPt)[] Rows =
        {
            ("sogdian", "Sogdians of the Dawn", "Sogdianos da Alvorada",
                "Apricot farmers and honest traders of the Zarafshan; family meals around the tandir are sacred.",
                "Agricultores de damasco e comerciantes honrados do Zarafshan; a comida em família no tandir é sagrada."),
            ("qocho_nomad", "Nomads of the Qocho Sands", "Nômades das Areias de Qocho",
                "Veiled caravan folk who read the invisible paths of the wind and tell stories by the fire.",
                "Caravaneiros de véu que leem os caminhos invisíveis do vento e contam histórias na fogueira."),
            ("kaggen_highlander", "Highlanders of the Kaggen Walls", "Homens das Muralhas de Kaggen",
                "Climbers and dry-stone masters of the round rondavels; they speak slowly and sing in chorus.",
                "Escaladores e mestres da pedra seca dos rondavéis; falam devagar e cantam em coro."),
            ("swahili_sailor", "Sailors of Taa Pwani", "Marinheiros de Taa Pwani",
                "Merchants of the triangular-sail dhows, steered by the stars of the Moon Ship.",
                "Mercadores das dhows de vela triangular, guiados pelas estrelas da Barca da Lua."),
            ("nart_rider", "Nart Riders of the Steppe", "Cavaleiros Nartes das Estepes",
                "Born riders who keep their given word above all and duel with bows at a gallop.",
                "Cavaleiros natos que guardam a palavra dada acima de tudo e duelam com arco a galope."),
            ("cave_folk", "Lampkeepers of the Threshold", "Povo das Lamparinas do Limiar",
                "Folk of the rose-tuff houses who never let a window lamp go out.",
                "Gente das casas de tufo rosa que nunca deixa apagar a lamparina da janela."),
            ("sangkent_monk", "Monks of the Heights", "Monges das Alturas",
                "Keepers of the five lights of the stair city, patient as the glacier.",
                "Guardiões das cinco luzes da cidade-escadaria, pacientes como a geleira."),
            ("mingcheng_imperial", "People of the Nine Rings", "Povo dos Nove Anéis",
                "Scholars and clerks of the great round capital, fond of ceremony and lanterns.",
                "Letrados e escribas da grande capital redonda, amigos da cerimônia e das lanternas."),
            ("forest_elf", "Wood Elves of Garajonay", "Elfos-da-Mata de Garajonay",
                "They never fell a tree; houses hang from the trunks and their whistled speech crosses valleys.",
                "Não derrubam árvores; as casas pendem dos troncos e a fala assobiada atravessa vales."),
            ("high_elf", "Sun Elves of Amanirenas", "Altos-Elfos Solares de Amanirenas",
                "Keepers of the sun prisms and the old astronomy, tall and gracious, golden-eyed.",
                "Guardiões dos prismas solares e da astronomia antiga, altos e gentis, de olhos dourados."),
            ("red_drum_orc", "Orcs of the Red Drum", "Orcs do Tambor Rubro",
                "Copper and bronze smiths whose great drums wake the Living Fire; honour is work and hospitality.",
                "Ferreiros de cobre e bronze cujos tambores acordam o Fogo Vivo; honra é trabalho e hospitalidade."),
            ("ivory_ossuary", "Warriors of the Ivory Ossuary", "Guerreiros do Ossuário de Marfim",
                "Volunteers who sealed themselves in to guard the Artificers' Seal; set free, they remember and thank.",
                "Voluntários que se fecharam para guardar o Selo dos Artífices; libertos, lembram e agradecem."),
            ("jhilpuri", "Jhilpuris of the Sacred Roots", "Jhilpuris das Raízes Sagradas",
                "They guide banyan roots for generations into bridges and homes without cutting a single log.",
                "Guiam raízes de figueira por gerações até virarem pontes e casas, sem cortar uma tora."),
            ("serpent_folk", "Serpent Folk of the Golden River", "Povo-Serpente do Rio Dourado",
                "Scaled guardians of the deep channels who charm currents and pearls.",
                "Guardiões de escamas dos canais fundos, que encantam correntes e pérolas."),
            ("vanara", "Vanaras, the Acrobats", "Vanaras, os Acrobatas",
                "Playful builders of hanging walkways, lovers of sweet fruit and bamboo staves.",
                "Construtores brincalhões de passarelas suspensas, amantes de fruta doce e bastões de bambu."),
            ("gandharva", "Gandharvas, the Musicians", "Gandharvas, os Músicos",
                "They carry no blades: sparks of light flow from their sitars and soothe the weary.",
                "Não usam lâminas: faíscas de luz saem das cítaras e curam o cansaço."),
            ("napirasu_dwarf", "Dwarves of Napirasu", "Anões de Napirasu",
                "Miners who smell the ore in the rock and forge star-bronze with hot-spring steam.",
                "Mineradores que sentem o minério pelo cheiro da rocha e forjam bronze estelar no vapor das fontes."),
            ("tunnel_kobold", "Kobolds of the Tunnels", "Kobolds dos Túneis",
                "Small big-eyed traders of glowing mushrooms and odd gears, fond of anything that jingles.",
                "Pequenos negociantes de olhos grandes, de cogumelos que brilham e engrenagens; amam o que tilinta."),
            ("aratta_bull_folk", "Bull Folk of Aratta", "Povo-Touro de Aratta",
                "Herders of wild aurochs; whoever shares bread and salt in their tents is under the clan's care.",
                "Pastores de auroques; quem divide pão e sal nas tendas fica sob a proteção do clã."),
            ("kirta_sea_folk", "Sea Folk of Kirta", "Povo-do-Mar de Kirta",
                "Pearl-skinned folk of the reef palaces under domes of air the coral breathes.",
                "Gente de pele de madrepérola dos palácios do recife, sob cúpulas de ar que o coral respira."),
            ("tarchon_troll", "Trolls of the Tarchon Stilts", "Trols das Palafitas de Tarchon",
                "Gentle giant fishers with mossy bark-skin, living on stilts over the deep bog.",
                "Pescadores gigantes e gentis de pele de casca com musgo, vivendo em palafitas no brejo fundo."),
            ("soslan_centaur", "Centaurs of Soslan", "Centauros de Soslan",
                "Swift nomads guided by the Sun Ship's stars, masters of the lasso.",
                "Nômades velozes guiados pelas estrelas da Barca do Sol, mestres do laço."),
            ("hill_giant", "Hill Giants", "Gigantes-da-Colina",
                "Once dimmed by the Dark, now repentant, they rebuild what they broke.",
                "Já foram apagados pela Treva; arrependidos, reconstroem o que quebraram."),
            ("trunk_folk", "Trunk Folk of Haripushpa", "Povo-Tronco de Haripushpa",
                "Slow tree-bodied keepers of the grove who answer a question next season.",
                "Guardiões lentos de corpo de árvore que respondem uma pergunta na estação seguinte."),
            ("satyr", "Satyrs of the Vale", "Sátiros do Vale",
                "Reed-pipe players of the windy peaks who answer with riddles.",
                "Tocadores de flauta de junco dos picos de vento que respondem com charadas."),
        };

        private static List<LocalizationEntry> _entries;

        public static IReadOnlyList<LocalizationEntry> Entries => _entries ??= Build();

        private static List<LocalizationEntry> Build()
        {
            var entries = new List<LocalizationEntry>();
            foreach ((string id, string nameEn, string namePt, string summaryEn, string summaryPt) in Rows)
            {
                entries.Add(new LocalizationEntry("race." + id + ".name", nameEn, namePt));
                entries.Add(new LocalizationEntry("race." + id + ".summary", summaryEn, summaryPt));
            }

            return entries;
        }

        private static Dictionary<string, string> _fallbackByKey;
        private static LocalizationManager _mergedInto;

        public static void EnsureRegistered(LocalizationManager manager)
        {
            if (manager == null || (ReferenceEquals(manager, _mergedInto) && manager.HasKey("race.sogdian.name")))
            {
                return;
            }

            manager.LoadEntries(Entries.Where(entry => !manager.HasKey(entry.Key)).ToList());
            _mergedInto = manager;
        }

        public static string Get(LocalizationManager manager, string key)
        {
            if (manager != null)
            {
                EnsureRegistered(manager);
                if (manager.HasKey(key))
                {
                    return manager.Get(key);
                }
            }

            if (_fallbackByKey == null)
            {
                _fallbackByKey = Entries.ToDictionary(e => e.Key, e => e.PtBR);
            }

            return key != null && _fallbackByKey.TryGetValue(key, out string text) ? text : key;
        }
    }
}
