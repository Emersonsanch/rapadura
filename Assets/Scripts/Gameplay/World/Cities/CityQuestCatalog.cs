using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.World.Cities
{
    /// <summary>
    /// A city side quest, handed out by a resident standing at their door in a walkable town (<c>CityResidentPost</c>):
    /// the same plain data as a <see cref="RegionQuestSpec"/> (built by the same <see cref="RegionQuestCatalog.Build"/>), plus
    /// the city and the giver. Texts: <c>quest.&lt;id without "quest_"&gt;.*</c> (title, description, objectives, journal) and
    /// the giver's <see cref="OfferKey"/> / <see cref="ActiveKey"/> lines, in <see cref="CityQuestTexts"/>.
    /// </summary>
    public sealed class CityQuestSpec
    {
        public readonly string CityId;
        public readonly string GiverResidentId;
        public readonly RegionQuestSpec Quest;

        /// <summary>A quest on the road (<see cref="IsOnTheRoad"/>): the map and zone of the city's region where its beasts live; null for a quest of the city's own map.</summary>
        public readonly string MapId;
        public readonly string ZoneId;

        /// <summary>A quest on the road: the hero level the giver waits for, and the level its XP is paid at (0 = a quest of the city's own map, any level, paid at <see cref="CityQuestCatalog.LevelOf"/>).</summary>
        public readonly int MinLevel;

        public CityQuestSpec(string cityId, string giverResidentId, RegionQuestSpec quest)
            : this(cityId, giverResidentId, null, null, 0, quest)
        {
        }

        public CityQuestSpec(string cityId, string giverResidentId, string mapId, string zoneId, int minLevel, RegionQuestSpec quest)
        {
            CityId = cityId;
            GiverResidentId = giverResidentId;
            MapId = mapId;
            ZoneId = zoneId;
            MinLevel = Math.Max(0, minLevel);
            Quest = quest;
        }

        /// <summary>Sent out to a zone of the region at <see cref="MinLevel"/> (Docs/Balanco100a500.md §5.3, sugestão 5), not on the city's own map.</summary>
        public bool IsOnTheRoad => MinLevel > 0;

        /// <summary>The level the quest's XP follows (<see cref="QuestXpRules"/>): <see cref="MinLevel"/> on the road, else the city's walk level.</summary>
        public int PaidLevel => IsOnTheRoad ? MinLevel : CityQuestCatalog.LevelOf(CityId);

        public string QuestId => Quest.QuestId;
        public string KeyBase => Quest.KeyBase;

        /// <summary>What the giver says when the quest starts (the cousins step up to them and take it).</summary>
        public string OfferKey => KeyBase + ".offer";

        /// <summary>What the giver says while the quest runs.</summary>
        public string ActiveKey => KeyBase + ".active";
    }

    /// <summary>What a quest-giving resident does when the cousins step up to them.</summary>
    public enum CityQuestTalk
    {
        /// <summary>Nothing to hand out (all done, or the previous quest of the chain is not done yet): their usual line.</summary>
        None = 0,
        /// <summary>Starts the quest and says <see cref="CityQuestSpec.OfferKey"/>.</summary>
        Offer = 1,
        /// <summary>The quest runs: says <see cref="CityQuestSpec.ActiveKey"/>.</summary>
        Active = 2,
    }

    /// <summary>
    /// The city side quests (content as code, like <see cref="RegionQuestCatalog"/>; kept apart from it because that one
    /// is "two chained quests and one giver per world-map region"). Same content rules as the region quests: kill targets
    /// are non-boss <c>enemy_*</c> assets of the city's own map, rewards are <c>Resources/Items</c> ids, every objective
    /// progresses in combat (the quest completes on its own and the reward lands, or waits in the pending queue).
    /// Copas de Garajonay (map 05, Docs/Mapas100.md): Gara asks the cousins to calm the lizards of the dry mist (the wolves
    /// of the forest floor stand in for the Lagarto-Gigante do Louro), then Tinguaro sends them after the moss boars.
    /// Taa Pwani (map 25): Captain Liyongo sends the cousins after the thief gulls that tear the dhows' sails, then Zawadi
    /// asks them to free the mangrove roots from the giant crabs and pays with his own Botas de Raiz Viva (the gate of map 30).
    /// Tambor Rubro (map 11): Gurparanzah sends the cousins after the red scorpions of the badlands, then Kessi takes them
    /// hunting the ember hawks and pays with the Teriaga do Oásis (the gate of map 12).
    /// Salões de Bronze (map 17): Untash sends the cousins after the slag golems that block the halls' stairs, then Shilhak
    /// takes them into his seam after the ember bats and pays with the Chá de Gengibre (the gate of map 18).
    /// Mercado dos Túneis (map 22): Takhsich asks them to clear the webs choking his air pipes, then Farnkhund wants the stone
    /// golems off his rails and pays with the Óleo do Fogo Eterno (the gate of map 23).
    /// Aratta (map 26): Enmerkar sends the cousins after the jackals that hunt the herds in the dry grass, then Shulgi wants the
    /// baboons off his race course and pays with waterskins for the long run.
    /// Kirta (map 28): King Kirta asks them to scrape the Lodo's ooze off the reef, then Pughat takes them to rescue the divers
    /// cornered by the dimmed lantern fish and pays with a Bolha de Ar Engarrafada (the gate of map 29).
    /// Jhilpur (map 31): old Salmaios sends the cousins to scrape the living ooze off the ghats before it climbs to the Raiz-Mãe,
    /// then Abiesous wants the crows off his boat school's oars and pays with a Fruta Purificadora (the gate of map 35).
    /// Palafitas de Tarchon (map 33): old Tarchon asks the cousins to free the will-o'-wisps caught in the reeds round the stilts,
    /// then Vel the fisher wants the ghost herons off his nets and pays with an Amuleto Antiveneno (the gate of map 34).
    /// Sangkent (map 36): Abā, the chief of the chutes, sends the cousins after the rolling stones that jam the chutes, then
    /// Kustai wants the ill-wind mist off the caravan notice board and pays with a Capa de Tempestade (the gate of map 40).
    /// Mingcheng (map 41): old Bögü sends the cousins after the dimmed stone lions that block the gates of the outer rings,
    /// then Lin Deng wants the lantern fish out of his canal and pays with the Selo do Sol (the gate of map 42).
    /// Torres Brancas (map 42): Queen Amanirenas asks the cousins to break the broken mirrors that steal the light of the towers,
    /// then Shanakdakhete wants the dimmed ibises off the terraces and pays with a rune lamp and soul stones.
    /// Acampamento de Soslan (map 47): Soslan sends the cousins after the dimmed wolves that circle the foals, then Satana wants
    /// the dimmed eagles off the camp and pays with a rune lamp (her guardian lamp).
    /// Fortim dos Gigantes (map 49): Baraqel asks the cousins to chase the hill boars out of the giants' pantry, then Virogdad
    /// wants the smoke crows of a bad dream off the tablets and pays with the Grampos de Escalada (the gate of map 52).
    /// Arvoredo de Haripushpa (map 50): old Haripushpa asks the cousins to clear the bark-eaters off the sleeping trunks, then
    /// Aryaman Puhr wants the smoke birds that peck the bark out of the grove and pays with healing runes.
    /// Vale dos Sátiros (map 53): King Midas asks the cousins to chase the golden bees that sting gold into whoever they touch
    /// away from the river, then Gordias wants the vine foxes out of his knotted rows and pays with wind runes.
    /// Farnkesh (map 54): Shad-Ohrmizd sends the cousins after the peak vultures that perch on the wind bells, then Tadi wants
    /// the mist of the ill wind off the rope bridges and pays with the Capa de Tempestade (the gate of map 57).
    /// Porto da Lua Branca (map 59): Yirkap the ski hunter asks the cousins to chase the thieving gulls off the fish racks of
    /// the quays, then Pera the good giant wants the dimmed bears off the ice of the canals and pays with the Roupa Térmica.
    /// Hinokura (map 65): Mestra Akane asks the cousins to chase the volcano crabs off the lacquer drying racks of the forge
    /// quarter, then Sango the boatwoman wants the ember gulls away from the Barco-Farol and pays with a reed boat (the reef, 68).
    /// Cais de Mesektet (map 71): Sinuhe the chief boatman asks the cousins to chase the crocodiles of the dark river off the
    /// landing of the barque, then Djedi the magician wants the black scorpions out of the sands by the quay and pays with a runic
    /// lamp (the night that never ends, 73–74).
    /// Dur-Jarras (map 77): Utnapishtim asks the cousins to chase the dimmed brick lions off the canals, then Sin-leqi-unninni wants
    /// the beetles out of the reed shelves of the tablets and pays with a reed boat (the flood, and the pitch of map 79).
    /// Fortaleza da Última Lâmpada (map 83): Pamour asks the cousins to chase the bats off the lamps of the rampart, then Makarios wants the dimmed
    /// ushabti off the road of his letters and pays with a runic lamp (the dark of map 88).
    /// Cais da Coluna (map 91): Ammo wants the stowaway shadows out of the bales of his last caravan, then Adda wants the dimmed birds off the jars of the Roda Celeste and pays with a storm cloak.
    /// On the road (<see cref="CityQuestSpec.IsOnTheRoad"/>, regions 9–16): after their own quests, the same givers send the cousins to a zone of
    /// their region at a level (two per band of 10 levels of 101–500 that had no quest, Docs/Balanco100a500.md §5.3 sugestão 5); offered from that
    /// level on (<see cref="Decide(string, Func{string, bool}, Func{string, bool}, int, out CityQuestSpec)"/>) and paid at it.
    /// </summary>
    public static class CityQuestCatalog
    {
        public const string GarajonayMist = "quest_city_garajonay_dry_mist";
        public const string GarajonayMoss = "quest_city_garajonay_moss_tracks";
        public const string TaaPwaniGulls = "quest_city_taa_pwani_thief_gulls";
        public const string TaaPwaniClaws = "quest_city_taa_pwani_mangrove_claws";
        public const string TamborRubroScorpions = "quest_city_tambor_rubro_red_scorpions";
        public const string TamborRubroHawks = "quest_city_tambor_rubro_ember_hawks";
        public const string SaloesGolems = "quest_city_saloes_de_bronze_slag_golems";
        public const string SaloesBats = "quest_city_saloes_de_bronze_ember_bats";
        public const string MercadoWebs = "quest_city_mercado_dos_tuneis_webbed_pipes";
        public const string MercadoRails = "quest_city_mercado_dos_tuneis_stones_on_the_rails";
        public const string ArattaJackals = "quest_city_aratta_grass_jackals";
        public const string ArattaRace = "quest_city_aratta_shulgi_course";
        public const string KirtaOoze = "quest_city_kirta_reef_ooze";
        public const string KirtaRescue = "quest_city_kirta_divers_rescue";
        public const string JhilpurGhats = "quest_city_jhilpur_ooze_on_the_ghats";
        public const string JhilpurOars = "quest_city_jhilpur_boat_school_crows";
        public const string TarchonWisps = "quest_city_tarchon_caught_wisps";
        public const string TarchonNets = "quest_city_tarchon_herons_on_the_nets";
        public const string SangkentChutes = "quest_city_sangkent_stones_in_the_chutes";
        public const string SangkentBoard = "quest_city_sangkent_mist_on_the_board";
        public const string MingchengLions = "quest_city_mingcheng_lions_at_the_gates";
        public const string MingchengCanal = "quest_city_mingcheng_fish_in_the_canal";
        public const string TorresMirrors = "quest_city_torres_brancas_broken_mirrors";
        public const string TorresIbises = "quest_city_torres_brancas_dimmed_ibises";
        public const string SoslanWolves = "quest_city_soslan_wolves_round_the_foals";
        public const string SoslanEagles = "quest_city_soslan_eagles_over_the_camp";
        public const string FortimBoars = "quest_city_fortim_boars_in_the_pantry";
        public const string FortimCrows = "quest_city_fortim_crows_of_a_bad_dream";
        public const string ArvoredoBarkEaters = "quest_city_arvoredo_bark_eaters";
        public const string ArvoredoSmokeBirds = "quest_city_arvoredo_smoke_birds";
        public const string ValeGoldenBees = "quest_city_vale_golden_bees";
        public const string ValeVineFoxes = "quest_city_vale_vine_foxes";
        public const string FarnkeshVultures = "quest_city_farnkesh_vultures_on_the_bells";
        public const string FarnkeshIllWind = "quest_city_farnkesh_ill_wind_on_the_bridges";
        public const string PortoGulls = "quest_city_porto_gulls_on_the_racks";
        public const string PortoBears = "quest_city_porto_bears_on_the_canals";
        public const string HinokuraCrabs = "quest_city_hinokura_crabs_on_the_racks";
        public const string HinokuraEmberGulls = "quest_city_hinokura_ember_gulls_on_the_lighthouse";
        public const string MesektetCrocodiles = "quest_city_mesektet_crocodiles_on_the_quay";
        public const string MesektetScorpions = "quest_city_mesektet_scorpions_in_the_sands";
        public const string DurJarrasLions = "quest_city_dur_jarras_lions_on_the_canals";
        public const string DurJarrasBeetles = "quest_city_dur_jarras_beetles_on_the_tablets";
        public const string UltimaLampadaBats = "quest_city_ultima_lampada_bats_on_the_rampart";
        public const string UltimaLampadaUshabti = "quest_city_ultima_lampada_ushabti_on_the_letter_road";
        public const string CaisColunaStowaways = "quest_city_cais_coluna_stowaways_in_the_bales";
        public const string CaisColunaBirds = "quest_city_cais_coluna_birds_on_the_jars";

        // Quests on the road (Docs/Balanco100a500.md §5.3, sugestão 5): a giver of the city sends the cousins to a zone of the region at a
        // level, one or two per band of 10 levels between 101 and 500 that had no quest (paid at that level, offered from it on).
        public const string ArvoredoHeartwood = "quest_city_arvoredo_beetles_in_the_heartwood";
        public const string FortimWatchtower = "quest_city_fortim_ochokochi_at_the_watchtower";
        public const string FarnkeshShelter = "quest_city_farnkesh_eagles_over_the_shelter";
        public const string FarnkeshGorge = "quest_city_farnkesh_stones_in_the_gorge";
        public const string PortoIrbis = "quest_city_porto_irbis_in_the_fjord_of_tusks";
        public const string PortoAuroraSeals = "quest_city_porto_seals_under_the_auroras";
        public const string PortoKulSeals = "quest_city_porto_seals_under_the_kul";
        public const string HinokuraEcheyde = "quest_city_hinokura_bombs_on_echeyde";
        public const string HinokuraCoral = "quest_city_hinokura_coral_on_the_reef";
        public const string HinokuraBladeFish = "quest_city_hinokura_blade_fish_in_the_warm_channels";
        public const string MesektetSerpent = "quest_city_mesektet_serpent_in_the_hidden_sands";
        public const string MesektetOuterRing = "quest_city_mesektet_biters_on_the_outer_ring";
        public const string MesektetCoils = "quest_city_mesektet_dark_in_the_coils";
        public const string MesektetBoilingShore = "quest_city_mesektet_snakes_on_the_boiling_shore";
        public const string DurJarrasTar = "quest_city_dur_jarras_living_tar_in_the_wells";
        public const string DurJarrasHerons = "quest_city_dur_jarras_herons_in_the_pitch_reeds";
        public const string DurJarrasCedars = "quest_city_dur_jarras_angry_cedars";
        public const string DurJarrasMantles = "quest_city_dur_jarras_glow_mantles_of_the_elders";
        public const string UltimaLampadaSmoke = "quest_city_ultima_lampada_smoke_giants";
        public const string UltimaLampadaEmberWolves = "quest_city_ultima_lampada_ember_wolves";
        public const string UltimaLampadaLions = "quest_city_ultima_lampada_hot_smoke_lions";
        public const string UltimaLampadaBladeBats = "quest_city_ultima_lampada_blade_bats";
        public const string UltimaLampadaEagles = "quest_city_ultima_lampada_black_eagles";
        public const string UltimaLampadaHarpies = "quest_city_ultima_lampada_wind_harpies";
        public const string UltimaLampadaDarkFish = "quest_city_ultima_lampada_dark_fish";
        public const string UltimaLampadaEels = "quest_city_ultima_lampada_great_eels";
        public const string UltimaLampadaOctopus = "quest_city_ultima_lampada_mud_octopus";
        public const string UltimaLampadaSerpents = "quest_city_ultima_lampada_blind_serpents";
        public const string UltimaLampadaLizards = "quest_city_ultima_lampada_shadow_lizards";
        public const string UltimaLampadaDragons = "quest_city_ultima_lampada_lesser_dragons";
        public const string CaisColunaRiverShadows = "quest_city_cais_coluna_shadows_in_the_light_river";
        public const string CaisColunaHold = "quest_city_cais_coluna_hold_of_the_moon_barque";
        public const string CaisColunaRopes = "quest_city_cais_coluna_ropes_on_the_prow";
        public const string CaisColunaCrackedJars = "quest_city_cais_coluna_cracked_jars";
        public const string CaisColunaToyArchons = "quest_city_cais_coluna_toy_archons";
        public const string CaisColunaChainedArchons = "quest_city_cais_coluna_chained_archons";
        public const string CaisColunaBalloonGiants = "quest_city_cais_coluna_balloon_giants";
        public const string CaisColunaGardenThieves = "quest_city_cais_coluna_thieves_in_the_garden";
        public const string CaisColunaSunGulls = "quest_city_cais_coluna_gulls_on_the_sun_barque";
        public const string CaisColunaSmokeRopes = "quest_city_cais_coluna_smoke_ropes_on_the_sail";

        /// <summary>
        /// Every city quest. The XP written below is the authored number; above level 50 the quest pays
        /// <see cref="QuestXpRules.CityShare"/> of the XP of its city's level (<see cref="LevelOf"/>) when that is more
        /// (<see cref="QuestXpRules"/>, Docs/Balanco100a500.md §5.3).
        /// </summary>
        public static readonly IReadOnlyList<CityQuestSpec> Quests = AtCityLevel(new List<CityQuestSpec>
        {
            // -- Copas de Garajonay: Gara, guardiã das copas, then Tinguaro, the tracker --
            new CityQuestSpec(CityCatalog.CopasGarajonay, "gara", new RegionQuestSpec(GarajonayMist, BiomeType.Forest, null, 320,
                new[] { new RegionObjectiveSpec("calm_mist_lizards", QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 3), new RegionRewardSpec("item_mana_potion", 2) })),
            new CityQuestSpec(CityCatalog.CopasGarajonay, "tinguaro", new RegionQuestSpec(GarajonayMoss, BiomeType.Forest, GarajonayMist, 380,
                new[] { new RegionObjectiveSpec("follow_moss_boars", QuestObjectiveType.KillEnemyType, "enemy_forest_peccary", 5) },
                new[] { new RegionRewardSpec("item_hunting_bow", 1), new RegionRewardSpec("item_gold", 25) })),

            // -- Taa Pwani: Captain Liyongo, the nakhoda (the Gaivota Ladra), then Zawadi, the root weaver (the giant coconut
            // crab) — map 25's own beasts (Docs/Mapas100.md 25). --
            new CityQuestSpec(CityCatalog.TaaPwani, "liyongo", new RegionQuestSpec(TaaPwaniGulls, BiomeType.Forest, null, 700,
                new[] { new RegionObjectiveSpec("drive_off_thief_gulls", QuestObjectiveType.KillEnemyType, "enemy_forest_thief_gull", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 4), new RegionRewardSpec("item_mana_potion", 3), new RegionRewardSpec("item_gold", 60) })),
            new CityQuestSpec(CityCatalog.TaaPwani, "zawadi", new RegionQuestSpec(TaaPwaniClaws, BiomeType.Forest, TaaPwaniGulls, 820,
                new[] { new RegionObjectiveSpec("cut_free_the_roots", QuestObjectiveType.KillEnemyType, "enemy_forest_coconut_crab", 5) },
                new[] { new RegionRewardSpec("item_botas_raiz", 1), new RegionRewardSpec("item_gold", 80) })),

            // -- Tambor Rubro: Gurparanzah, the archer chief, then Kessi, the hunter. The desert scorpions stand in for the
            // Escorpião-Rubro and the crested hawks for the Gavião de Brasa (Docs/Mapas100.md 11). --
            new CityQuestSpec(CityCatalog.TamborRubro, "gurparanzah", new RegionQuestSpec(TamborRubroScorpions, BiomeType.Desert, null, 460,
                new[] { new RegionObjectiveSpec("clear_red_scorpions", QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 4), new RegionRewardSpec("item_pedra_bencao", 2), new RegionRewardSpec("item_gold", 45) })),
            new CityQuestSpec(CityCatalog.TamborRubro, "kessi", new RegionQuestSpec(TamborRubroHawks, BiomeType.Desert, TamborRubroScorpions, 540,
                new[] { new RegionObjectiveSpec("hunt_ember_hawks", QuestObjectiveType.KillEnemyType, "enemy_mountain_crested_hawk", 5) },
                new[] { new RegionRewardSpec("item_teriaga", 1), new RegionRewardSpec("item_gold", 55) })),

            // -- Salões de Bronze: Untash, the master builder, then Shilhak, the miner. The stone golems stand in for the Golem de
            // Escória (badly cast ushabtis) and the cave bat swarms for the Morcego de Brasa (Docs/Mapas100.md 17). --
            new CityQuestSpec(CityCatalog.SaloesDeBronze, "untash", new RegionQuestSpec(SaloesGolems, BiomeType.Mountain, null, 620,
                new[] { new RegionObjectiveSpec("break_slag_golems", QuestObjectiveType.KillEnemyType, "enemy_mountain_stone_golem", 5) },
                new[] { new RegionRewardSpec("item_health_potion", 4), new RegionRewardSpec("item_pedra_bencao", 2), new RegionRewardSpec("item_gold", 55) })),
            new CityQuestSpec(CityCatalog.SaloesDeBronze, "shilhak", new RegionQuestSpec(SaloesBats, BiomeType.Mountain, SaloesGolems, 700,
                new[] { new RegionObjectiveSpec("drive_out_ember_bats", QuestObjectiveType.KillEnemyType, "enemy_cave_bat_swarm", 6) },
                new[] { new RegionRewardSpec("item_cha_gengibre", 1), new RegionRewardSpec("item_gold", 60) })),

            // -- Mercado dos Túneis: Takhsich, the inventor, then Farnkhund, the cart driver. The cave spiders stand in for the
            // Lagarta de Pedra and the stone golems for the Toupeira-Gigante (Docs/Mapas100.md 22). --
            new CityQuestSpec(CityCatalog.MercadoDosTuneis, "takhsich", new RegionQuestSpec(MercadoWebs, BiomeType.Caves, null, 760,
                new[] { new RegionObjectiveSpec("clear_webbed_pipes", QuestObjectiveType.KillEnemyType, "enemy_forest_spider", 6) },
                new[] { new RegionRewardSpec("item_mana_potion", 4), new RegionRewardSpec("item_lamparina_oleo", 1), new RegionRewardSpec("item_garrafa_ar_sereno", 2),
                    new RegionRewardSpec("item_gold", 65) })),
            new CityQuestSpec(CityCatalog.MercadoDosTuneis, "farnkhund", new RegionQuestSpec(MercadoRails, BiomeType.Caves, MercadoWebs, 840,
                new[] { new RegionObjectiveSpec("clear_the_rails", QuestObjectiveType.KillEnemyType, "enemy_mountain_stone_golem", 4) },
                new[] { new RegionRewardSpec("item_oleo_fogo_eterno", 1), new RegionRewardSpec("item_gold", 70) })),

            // -- Aratta: Enmerkar, the chief builder, then Shulgi, the runner. The desert jackals stand in for the Leão do Capim
            // Apagado and the baboons for the beasts that block the course of the Corrida de Shulgi (Docs/Mapas100.md 26). --
            new CityQuestSpec(CityCatalog.Aratta, "enmerkar", new RegionQuestSpec(ArattaJackals, BiomeType.Forest, null, 900,
                new[] { new RegionObjectiveSpec("drive_off_grass_jackals", QuestObjectiveType.KillEnemyType, "enemy_desert_jackal", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 4), new RegionRewardSpec("item_mana_potion", 3), new RegionRewardSpec("item_gold", 75) })),
            new CityQuestSpec(CityCatalog.Aratta, "shulgi", new RegionQuestSpec(ArattaRace, BiomeType.Forest, ArattaJackals, 980,
                new[] { new RegionObjectiveSpec("clear_the_course", QuestObjectiveType.KillEnemyType, "enemy_mountain_baboon", 5) },
                new[] { new RegionRewardSpec("item_odre_agua", 2), new RegionRewardSpec("item_gold", 90) })),

            // -- Kirta: King Kirta, then Pughat. The Água-Viva de Lodo and the Peixe-Leão Apagado of the ficha (Docs/Mapas100.md 28)
            // are their own beasts now (CoastOwnBeastRecipes). --
            new CityQuestSpec(CityCatalog.Kirta, "kirta", new RegionQuestSpec(KirtaOoze, BiomeType.Forest, null, 1040,
                new[] { new RegionObjectiveSpec("scrape_the_ooze", QuestObjectiveType.KillEnemyType, "enemy_cave_mud_jellyfish", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 4), new RegionRewardSpec("item_mana_potion", 4), new RegionRewardSpec("item_gold", 90) })),
            new CityQuestSpec(CityCatalog.Kirta, "pughat", new RegionQuestSpec(KirtaRescue, BiomeType.Forest, KirtaOoze, 1120,
                new[] { new RegionObjectiveSpec("free_the_divers", QuestObjectiveType.KillEnemyType, "enemy_cave_dimmed_lionfish", 5) },
                new[] { new RegionRewardSpec("item_bolha_ar", 1), new RegionRewardSpec("item_gold", 100) })),

            // -- Jhilpur: old Salmaios, then Abiesous. The cave ooze stands in for the Filhos do Lodo that climb the ghats (the
            // Maré de Lodo of Docs/Mapas100.md 31) and the dark crows for the dimmed herons that steal the boat school's oars. --
            new CityQuestSpec(CityCatalog.Jhilpur, "salmaios", new RegionQuestSpec(JhilpurGhats, BiomeType.Forest, null, 1200,
                new[] { new RegionObjectiveSpec("scrape_the_ghats", QuestObjectiveType.KillEnemyType, "enemy_cave_ooze", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 4), new RegionRewardSpec("item_gold", 110) })),
            new CityQuestSpec(CityCatalog.Jhilpur, "abiesous", new RegionQuestSpec(JhilpurOars, BiomeType.Forest, JhilpurGhats, 1300,
                new[] { new RegionObjectiveSpec("chase_the_crows", QuestObjectiveType.KillEnemyType, "enemy_cave_dark_crow", 6) },
                new[] { new RegionRewardSpec("item_fruta_purificadora", 1), new RegionRewardSpec("item_gold", 120) })),

            // -- Palafitas de Tarchon: old Tarchon, then Vel. The Fogos-Fátuos Presos (the swarm of Docs/Mapas100.md 33) are their own beast
            // now (WideStandInBeastRecipes), and so is the Garça-Fantasma that steals from the nets (CoastOwnBeastRecipes; the shroud owl goes with it at night). --
            new CityQuestSpec(CityCatalog.PalafitasTarchon, "tarchon", new RegionQuestSpec(TarchonWisps, BiomeType.Forest, null, 1350,
                new[] { new RegionObjectiveSpec("free_the_wisps", QuestObjectiveType.KillEnemyType, "enemy_forest_marsh_wisp", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 120) })),
            new CityQuestSpec(CityCatalog.PalafitasTarchon, "vel", new RegionQuestSpec(TarchonNets, BiomeType.Forest, TarchonWisps, 1450,
                new[] { new RegionObjectiveSpec("chase_the_herons", QuestObjectiveType.KillEnemyType, "enemy_forest_ghost_heron", 6) },
                new[] { new RegionRewardSpec("item_amuleto_antiveneno", 1), new RegionRewardSpec("item_gold", 130) })),

            // -- Sangkent: Abā, then Kustai. The Pedra-Rolante of the ficha (Docs/Mapas100.md 36) and the Neblina do Vento Ruim of the
            // Calhas de Pedra (37), which blurs the notices of the caravans, are their own beasts now (HighlandOwnBeastRecipes: the
            // Pedra-Rolante da Morena and the Neblina das Cristas). --
            new CityQuestSpec(CityCatalog.Sangkent, "aba", new RegionQuestSpec(SangkentChutes, BiomeType.Forest, null, 1500,
                new[] { new RegionObjectiveSpec("clear_the_chutes", QuestObjectiveType.KillEnemyType, "enemy_mountain_moraine_roller", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 130) })),
            new CityQuestSpec(CityCatalog.Sangkent, "kustai", new RegionQuestSpec(SangkentBoard, BiomeType.Forest, SangkentChutes, 1600,
                new[] { new RegionObjectiveSpec("clear_the_board", QuestObjectiveType.KillEnemyType, "enemy_mountain_crest_mist", 6) },
                new[] { new RegionRewardSpec("item_capa_tempestade", 1), new RegionRewardSpec("item_gold", 140) })),

            // -- Mingcheng: Bögü, then Lin Deng. The stone golem stands in for the Leão de Pedra Apagado of the ficha (Docs/Mapas100.md
            // 41: the guardian statues of the gates) and the lantern fish for the Lanterna-Peixe of the Canal das Lanternas (43). --
            new CityQuestSpec(CityCatalog.Mingcheng, "bogu", new RegionQuestSpec(MingchengLions, BiomeType.Forest, null, 1700,
                new[] { new RegionObjectiveSpec("clear_the_gates", QuestObjectiveType.KillEnemyType, "enemy_mountain_stone_golem", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 150) })),
            new CityQuestSpec(CityCatalog.Mingcheng, "lin_deng", new RegionQuestSpec(MingchengCanal, BiomeType.Forest, MingchengLions, 1800,
                new[] { new RegionObjectiveSpec("clear_the_canal", QuestObjectiveType.KillEnemyType, "enemy_cave_lantern_fish", 6) },
                new[] { new RegionRewardSpec("item_selo_sol", 1), new RegionRewardSpec("item_gold", 150) })),

            // -- Torres Brancas: Amanirenas, then Shanakdakhete. The Espelho Quebrado of the ficha (Docs/Mapas100.md 42) is its own beast now
            // (WideStandInBeastRecipes), and the Íbis de Luz Apagada too (HighlandOwnBeastRecipes). --
            new CityQuestSpec(CityCatalog.TorresBrancas, "amanirenas", new RegionQuestSpec(TorresMirrors, BiomeType.Forest, null, 1850,
                new[] { new RegionObjectiveSpec("break_the_mirrors", QuestObjectiveType.KillEnemyType, "enemy_desert_broken_mirror", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 160) })),
            new CityQuestSpec(CityCatalog.TorresBrancas, "shanakdakhete", new RegionQuestSpec(TorresIbises, BiomeType.Forest, TorresMirrors, 1950,
                new[] { new RegionObjectiveSpec("chase_the_ibises", QuestObjectiveType.KillEnemyType, "enemy_mountain_dimmed_light_ibis", 6) },
                new[] { new RegionRewardSpec("item_lamparina_runica", 1), new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 170) })),

            // -- Acampamento de Soslan: Soslan, then Satana. The wolf stands in for the Lobo da Estepe Apagado of the ficha
            // (Docs/Mapas100.md 47); the Águia-Dourada Apagada is its own beast now (LentRoleBeastRecipes). --
            new CityQuestSpec(CityCatalog.AcampamentoSoslan, "soslan", new RegionQuestSpec(SoslanWolves, BiomeType.Forest, null, 2000,
                new[] { new RegionObjectiveSpec("chase_the_wolves", QuestObjectiveType.KillEnemyType, "enemy_forest_wolf", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 170) })),
            new CityQuestSpec(CityCatalog.AcampamentoSoslan, "satana", new RegionQuestSpec(SoslanEagles, BiomeType.Forest, SoslanWolves, 2100,
                new[] { new RegionObjectiveSpec("chase_the_eagles", QuestObjectiveType.KillEnemyType, "enemy_mountain_dimmed_golden_eagle", 6) },
                new[] { new RegionRewardSpec("item_lamparina_runica", 1), new RegionRewardSpec("item_gold", 170) })),

            // -- Fortim dos Gigantes: Baraqel, then Virogdad. The Javali da Colina and the Corvo-Mensageiro de Fumaça of the ficha
            // (Docs/Mapas100.md 49) are their own beasts now (LentRoleBeastRecipes). --
            new CityQuestSpec(CityCatalog.Fortim, "baraqel", new RegionQuestSpec(FortimBoars, BiomeType.Forest, null, 2200,
                new[] { new RegionObjectiveSpec("chase_the_boars", QuestObjectiveType.KillEnemyType, "enemy_forest_hill_boar", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 180) })),
            new CityQuestSpec(CityCatalog.Fortim, "virogdad", new RegionQuestSpec(FortimCrows, BiomeType.Forest, FortimBoars, 2300,
                new[] { new RegionObjectiveSpec("chase_the_crows", QuestObjectiveType.KillEnemyType, "enemy_cave_smoke_messenger", 6) },
                new[] { new RegionRewardSpec("item_grampos_escalada", 1), new RegionRewardSpec("item_gold", 180) })),

            // -- Arvoredo de Haripushpa: Haripushpa, then Aryaman Puhr. The spider stands in for the Besouro-Casca of the ficha
            // (Docs/Mapas100.md 50); the Pica-Pau de Fumaça is its own beast now (LentRoleBeastRecipes). --
            new CityQuestSpec(CityCatalog.Arvoredo, "haripushpa", new RegionQuestSpec(ArvoredoBarkEaters, BiomeType.Forest, null, 2400,
                new[] { new RegionObjectiveSpec("clear_the_bark", QuestObjectiveType.KillEnemyType, "enemy_forest_spider", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 190) })),
            new CityQuestSpec(CityCatalog.Arvoredo, "aryaman", new RegionQuestSpec(ArvoredoSmokeBirds, BiomeType.Forest, ArvoredoBarkEaters, 2500,
                new[] { new RegionObjectiveSpec("chase_the_smoke_birds", QuestObjectiveType.KillEnemyType, "enemy_mountain_smoke_woodpecker", 6) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 190) })),

            // -- Vale dos Sátiros: Midas, then Gordias. The firefly swarm stands in for the Abelha Dourada of the ficha
            // (Docs/Mapas100.md 53) and the jackal for the Raposa da Vinha. --
            new CityQuestSpec(CityCatalog.ValeDosSatiros, "midas", new RegionQuestSpec(ValeGoldenBees, BiomeType.Forest, null, 2600,
                new[] { new RegionObjectiveSpec("chase_the_bees", QuestObjectiveType.KillEnemyType, "enemy_desert_firefly_swarm", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 200) })),
            new CityQuestSpec(CityCatalog.ValeDosSatiros, "gordias", new RegionQuestSpec(ValeVineFoxes, BiomeType.Forest, ValeGoldenBees, 2700,
                new[] { new RegionObjectiveSpec("chase_the_foxes", QuestObjectiveType.KillEnemyType, "enemy_desert_jackal", 7) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 200) })),

            // -- Farnkesh: Shad-Ohrmizd, then Tadi. The Abutre do Pico of the ficha (Docs/Mapas100.md 54, its own beast now, FarStandInBeastRecipes)
            // and the ill-wind mist is the Neblina do Vento Ruim of the Pontes de Vento (55). --
            new CityQuestSpec(CityCatalog.Farnkesh, "shad_ohrmizd", new RegionQuestSpec(FarnkeshVultures, BiomeType.Forest, null, 2800,
                new[] { new RegionObjectiveSpec("chase_the_vultures", QuestObjectiveType.KillEnemyType, "enemy_mountain_peak_vulture", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 210) })),
            new CityQuestSpec(CityCatalog.Farnkesh, "tadi", new RegionQuestSpec(FarnkeshIllWind, BiomeType.Forest, FarnkeshVultures, 2900,
                new[] { new RegionObjectiveSpec("clear_the_mist", QuestObjectiveType.KillEnemyType, "enemy_mountain_ill_wind_mist", 6) },
                new[] { new RegionRewardSpec("item_capa_tempestade", 1), new RegionRewardSpec("item_gold", 210) })),

            // -- Porto da Lua Branca: Yirkap, then Pera. The thieving gull stands in for the Gaivota-do-Gelo of the ficha
            // (Docs/Mapas100.md 59) and the brown bear, dimmed, for the Urso-Polar Apagado of the Canais de Gelo (60). --
            new CityQuestSpec(CityCatalog.PortoDaLuaBranca, "yirkap", new RegionQuestSpec(PortoGulls, BiomeType.Forest, null, 3000,
                new[] { new RegionObjectiveSpec("chase_the_gulls", QuestObjectiveType.KillEnemyType, "enemy_forest_thief_gull", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 220) })),
            new CityQuestSpec(CityCatalog.PortoDaLuaBranca, "pera", new RegionQuestSpec(PortoBears, BiomeType.Forest, PortoGulls, 3100,
                new[] { new RegionObjectiveSpec("chase_the_bears", QuestObjectiveType.KillEnemyType, "enemy_mountain_brown_bear", 6) },
                new[] { new RegionRewardSpec("item_roupa_termica", 1), new RegionRewardSpec("item_gold", 220) })),

            // -- Hinokura: Akane, then Sango. The coconut crab stands in for the Caranguejo-Vulcão of the ficha (Docs/Mapas100.md
            // 65) and the ember vulture for its Gaivota-Brasa, until the region's own creatures exist. --
            new CityQuestSpec(CityCatalog.Hinokura, "akane", new RegionQuestSpec(HinokuraCrabs, BiomeType.Forest, null, 3200,
                new[] { new RegionObjectiveSpec("chase_the_crabs", QuestObjectiveType.KillEnemyType, "enemy_forest_coconut_crab", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 230) })),
            new CityQuestSpec(CityCatalog.Hinokura, "sango", new RegionQuestSpec(HinokuraEmberGulls, BiomeType.Forest, HinokuraCrabs, 3300,
                new[] { new RegionObjectiveSpec("chase_the_gulls", QuestObjectiveType.KillEnemyType, "enemy_desert_ember_vulture", 6) },
                new[] { new RegionRewardSpec("item_barco_junco", 1), new RegionRewardSpec("item_gold", 230) })),

            // -- Cais de Mesektet: Sinuhe, then Djedi. The mangrove crocodile stands in for the Crocodilo do Rio Escuro of the
            // ficha (Docs/Mapas100.md 71) and the desert scorpion for the Escorpião Negro of 73, until the region's own creatures exist. --
            new CityQuestSpec(CityCatalog.CaisDeMesektet, "sinuhe", new RegionQuestSpec(MesektetCrocodiles, BiomeType.Forest, null, 3400,
                new[] { new RegionObjectiveSpec("chase_the_crocodiles", QuestObjectiveType.KillEnemyType, "enemy_forest_mangrove_crocodile", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 240) })),
            new CityQuestSpec(CityCatalog.CaisDeMesektet, "djedi", new RegionQuestSpec(MesektetScorpions, BiomeType.Forest, MesektetCrocodiles, 3500,
                new[] { new RegionObjectiveSpec("chase_the_scorpions", QuestObjectiveType.KillEnemyType, "enemy_desert_scorpion", 6) },
                new[] { new RegionRewardSpec("item_lamparina_runica", 1), new RegionRewardSpec("item_gold", 240) })),

            // -- Dur-Jarras: Utnapishtim, then Sin-leqi-unninni. The stone lion stands in for the Leão de Tijolo Apagado of the ficha
            // (Docs/Mapas100.md 77) and the bark beetle for its Gafanhoto de Betume, until the region's own creatures exist. --
            new CityQuestSpec(CityCatalog.DurJarras, "utnapishtim", new RegionQuestSpec(DurJarrasLions, BiomeType.Forest, null, 3600,
                new[] { new RegionObjectiveSpec("chase_the_lions", QuestObjectiveType.KillEnemyType, "enemy_forest_stone_lion", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 250) })),
            new CityQuestSpec(CityCatalog.DurJarras, "sin_leqi_unninni", new RegionQuestSpec(DurJarrasBeetles, BiomeType.Forest, DurJarrasLions, 3700,
                new[] { new RegionObjectiveSpec("chase_the_beetles", QuestObjectiveType.KillEnemyType, "enemy_mountain_bark_beetle", 6) },
                new[] { new RegionRewardSpec("item_barco_junco", 1), new RegionRewardSpec("item_gold", 250) })),

            // -- Fortaleza da Última Lâmpada: Pamour, then Makarios. The Morcego da Treva of the ficha (Docs/Mapas100.md 83) is its own beast now
            // (FarStandInBeastRecipes); the rope ushabti still stands in for its Ushabti Apagado. --
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", new RegionQuestSpec(UltimaLampadaBats, BiomeType.Forest, null, 3800,
                new[] { new RegionObjectiveSpec("chase_the_bats", QuestObjectiveType.KillEnemyType, "enemy_forest_dark_bat", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 260) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "makarios_cartas", new RegionQuestSpec(UltimaLampadaUshabti, BiomeType.Forest, UltimaLampadaBats, 3900,
                new[] { new RegionObjectiveSpec("chase_the_ushabti", QuestObjectiveType.KillEnemyType, "enemy_mountain_rope_ushabti", 6) },
                new[] { new RegionRewardSpec("item_lamparina_runica", 1), new RegionRewardSpec("item_gold", 260) })),

            // -- Cais da Coluna: Ammo, then Adda. The Sombra Clandestina of the ficha (Docs/Mapas100.md 91) is its own beast now (BorrowedRoleBeastRecipes);
            // the dimmed ibis still stands in for its Ave de Nuvem Apagada. --
            new CityQuestSpec(CityCatalog.CaisDaColuna, "ammo_caravaneiro", new RegionQuestSpec(CaisColunaStowaways, BiomeType.Forest, null, 4000,
                new[] { new RegionObjectiveSpec("chase_the_stowaways", QuestObjectiveType.KillEnemyType, "enemy_forest_stowaway_shadow", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 270) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "adda_roda", new RegionQuestSpec(CaisColunaBirds, BiomeType.Forest, CaisColunaStowaways, 4200,
                new[] { new RegionObjectiveSpec("chase_the_birds", QuestObjectiveType.KillEnemyType, "enemy_forest_dimmed_ibis", 6) },
                new[] { new RegionRewardSpec("item_capa_tempestade", 1), new RegionRewardSpec("item_gold", 280) })),

            // ================= On the road (regions 9–16): the same givers, each quest at a zone of the region and a level =================
            // (map, zone, level). The beast lives in that zone's fauna; the giver hands it out from that level on and it pays 25% of
            // the XP of that level (QuestXpRules), one or two per band of 10 levels of 121–481 that had no quest. Chained after the
            // giver's own quests.
            new CityQuestSpec(CityCatalog.Arvoredo, "aryaman", "map_050_haripushpa", "arvoredo_de_haripushpa", 124, new RegionQuestSpec(ArvoredoHeartwood, BiomeType.Forest, ArvoredoSmokeBirds, 4300,
                new[] { new RegionObjectiveSpec("clear_the_heartwood", QuestObjectiveType.KillEnemyType, "enemy_mountain_bark_beetle", 7) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 200) })),
            new CityQuestSpec(CityCatalog.Fortim, "virogdad", "map_052_amirani", "torre_de_vigia", 128, new RegionQuestSpec(FortimWatchtower, BiomeType.Forest, FortimCrows, 4400,
                new[] { new RegionObjectiveSpec("chase_the_ochokochi", QuestObjectiveType.KillEnemyType, "enemy_mountain_ochokochi", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 200) })),
            new CityQuestSpec(CityCatalog.Farnkesh, "tadi", "map_057_garganta_ays", "gruta_abrigo", 164, new RegionQuestSpec(FarnkeshShelter, BiomeType.Forest, FarnkeshIllWind, 4500,
                new[] { new RegionObjectiveSpec("chase_the_storm_eagles", QuestObjectiveType.KillEnemyType, "enemy_mountain_storm_eagle", 7) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 210) })),
            new CityQuestSpec(CityCatalog.Farnkesh, "shad_ohrmizd", "map_057_garganta_ays", "garganta_dos_ays", 168, new RegionQuestSpec(FarnkeshGorge, BiomeType.Forest, FarnkeshVultures, 4600,
                new[] { new RegionObjectiveSpec("clear_the_gorge", QuestObjectiveType.KillEnemyType, "enemy_mountain_rolling_stone", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 210) })),
            new CityQuestSpec(CityCatalog.PortoDaLuaBranca, "yirkap", "map_062_galerias_mamutes", "fiorde_das_presas", 202, new RegionQuestSpec(PortoIrbis, BiomeType.Forest, PortoGulls, 4650,
                new[] { new RegionObjectiveSpec("chase_the_irbis", QuestObjectiveType.KillEnemyType, "enemy_mountain_irbis", 6) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 215) })),
            new CityQuestSpec(CityCatalog.PortoDaLuaBranca, "yirkap", "map_063_mar_auroras", "mar_das_auroras", 208, new RegionQuestSpec(PortoAuroraSeals, BiomeType.Forest, PortoIrbis, 4680,
                new[] { new RegionObjectiveSpec("chase_the_leopard_seals", QuestObjectiveType.KillEnemyType, "enemy_mountain_leopard_seal", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 215) })),
            new CityQuestSpec(CityCatalog.PortoDaLuaBranca, "pera", "map_064_fundo_velho_kul", "fiorde_do_kul", 232, new RegionQuestSpec(PortoKulSeals, BiomeType.Forest, PortoBears, 4700,
                new[] { new RegionObjectiveSpec("chase_the_shadow_seals", QuestObjectiveType.KillEnemyType, "enemy_mountain_shadow_seal", 7) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 220) })),
            new CityQuestSpec(CityCatalog.Hinokura, "akane", "map_067_monte_echeyde", "monte_echeyde", 236, new RegionQuestSpec(HinokuraEcheyde, BiomeType.Forest, HinokuraCrabs, 4800,
                new[] { new RegionObjectiveSpec("calm_the_bombs", QuestObjectiveType.KillEnemyType, "enemy_mountain_volcanic_bomb", 6) },
                new[] { new RegionRewardSpec("item_pedra_bencao", 2), new RegionRewardSpec("item_gold", 220) })),
            new CityQuestSpec(CityCatalog.Hinokura, "sango", "map_068_recife_obsidiana", "recife_de_obsidiana", 244, new RegionQuestSpec(HinokuraCoral, BiomeType.Forest, HinokuraEmberGulls, 4900,
                new[] { new RegionObjectiveSpec("break_the_ember_coral", QuestObjectiveType.KillEnemyType, "enemy_mountain_ember_coral", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 230) })),
            new CityQuestSpec(CityCatalog.Hinokura, "akane", "map_068_recife_obsidiana", "canais_quentes", 248, new RegionQuestSpec(HinokuraBladeFish, BiomeType.Forest, HinokuraEcheyde, 5000,
                new[] { new RegionObjectiveSpec("chase_the_blade_fish", QuestObjectiveType.KillEnemyType, "enemy_mountain_blade_fish", 6) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 230) })),
            new CityQuestSpec(CityCatalog.CaisDeMesektet, "djedi", "map_073_areal_escondido", "areal_escondido", 274, new RegionQuestSpec(MesektetSerpent, BiomeType.Forest, MesektetScorpions, 5100,
                new[] { new RegionObjectiveSpec("chase_the_serpents", QuestObjectiveType.KillEnemyType, "enemy_forest_three_headed_serpent", 6) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 240) })),
            new CityQuestSpec(CityCatalog.CaisDeMesektet, "sinuhe", "map_074_aneis_mehen", "anel_externo", 280, new RegionQuestSpec(MesektetOuterRing, BiomeType.Forest, MesektetCrocodiles, 5200,
                new[] { new RegionObjectiveSpec("chase_the_biters", QuestObjectiveType.KillEnemyType, "enemy_forest_shadow_biter", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 240) })),
            new CityQuestSpec(CityCatalog.CaisDeMesektet, "djedi", "map_074_aneis_mehen", "aneis_de_mehen", 284, new RegionQuestSpec(MesektetCoils, BiomeType.Forest, MesektetSerpent, 5300,
                new[] { new RegionObjectiveSpec("chase_the_creeping_dark", QuestObjectiveType.KillEnemyType, "enemy_forest_creeping_dark", 8) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 250) })),
            new CityQuestSpec(CityCatalog.CaisDeMesektet, "sinuhe", "map_075_lago_de_fogo", "beira_fervente", 288, new RegionQuestSpec(MesektetBoilingShore, BiomeType.Forest, MesektetOuterRing, 5400,
                new[] { new RegionObjectiveSpec("chase_the_snakes", QuestObjectiveType.KillEnemyType, "enemy_desert_sand_snake", 7) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 250) })),
            new CityQuestSpec(CityCatalog.DurJarras, "utnapishtim", "map_079_pocos_betume", "pocos_de_betume", 314, new RegionQuestSpec(DurJarrasTar, BiomeType.Forest, DurJarrasLions, 5500,
                new[] { new RegionObjectiveSpec("scrape_the_living_tar", QuestObjectiveType.KillEnemyType, "enemy_forest_living_tar", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 260) })),
            new CityQuestSpec(CityCatalog.DurJarras, "sin_leqi_unninni", "map_079_pocos_betume", "juncal_de_piche", 318, new RegionQuestSpec(DurJarrasHerons, BiomeType.Forest, DurJarrasBeetles, 5600,
                new[] { new RegionObjectiveSpec("chase_the_tar_herons", QuestObjectiveType.KillEnemyType, "enemy_forest_tar_heron", 6) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 260) })),
            new CityQuestSpec(CityCatalog.DurJarras, "utnapishtim", "map_080_cedral_hobabish", "cedral_de_hobabish", 322, new RegionQuestSpec(DurJarrasCedars, BiomeType.Forest, DurJarrasTar, 5700,
                new[] { new RegionObjectiveSpec("calm_the_cedars", QuestObjectiveType.KillEnemyType, "enemy_forest_angry_cedar", 6) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 270) })),
            new CityQuestSpec(CityCatalog.DurJarras, "sin_leqi_unninni", "map_080_cedral_hobabish", "cedros_anciaos", 326, new RegionQuestSpec(DurJarrasMantles, BiomeType.Forest, DurJarrasHerons, 5800,
                new[] { new RegionObjectiveSpec("chase_the_glow_mantles", QuestObjectiveType.KillEnemyType, "enemy_forest_glow_mantle", 7) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 270) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", "map_084_cova_fumaca", "corredores_de_cinza", 354, new RegionQuestSpec(UltimaLampadaSmoke, BiomeType.Forest, UltimaLampadaBats, 5900,
                new[] { new RegionObjectiveSpec("chase_the_smoke_giants", QuestObjectiveType.KillEnemyType, "enemy_forest_smoke_giant", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 280) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "makarios_cartas", "map_085_cova_fogo_devorador", "cova_do_fogo_devorador", 358, new RegionQuestSpec(UltimaLampadaEmberWolves, BiomeType.Forest, UltimaLampadaUshabti, 6000,
                new[] { new RegionObjectiveSpec("chase_the_ember_wolves", QuestObjectiveType.KillEnemyType, "enemy_forest_ember_wolf", 8) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 280) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", "map_085_cova_fogo_devorador", "saloes_de_cinza_quente", 364, new RegionQuestSpec(UltimaLampadaLions, BiomeType.Forest, UltimaLampadaSmoke, 6100,
                new[] { new RegionObjectiveSpec("chase_the_hot_lions", QuestObjectiveType.KillEnemyType, "enemy_forest_hot_smoke_lion", 5) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 285) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "makarios_cartas", "map_086_cova_vento_destruidor", "cova_do_vento_destruidor", 368, new RegionQuestSpec(UltimaLampadaBladeBats, BiomeType.Forest, UltimaLampadaEmberWolves, 6200,
                new[] { new RegionObjectiveSpec("chase_the_blade_bats", QuestObjectiveType.KillEnemyType, "enemy_forest_blade_bat", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 285) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", "map_086_cova_vento_destruidor", "cova_do_vento_destruidor", 372, new RegionQuestSpec(UltimaLampadaEagles, BiomeType.Forest, UltimaLampadaLions, 6300,
                new[] { new RegionObjectiveSpec("chase_the_black_eagles", QuestObjectiveType.KillEnemyType, "enemy_forest_black_eagle", 7) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 290) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "makarios_cartas", "map_086_cova_vento_destruidor", "plataformas_soltas", 376, new RegionQuestSpec(UltimaLampadaHarpies, BiomeType.Forest, UltimaLampadaBladeBats, 6400,
                new[] { new RegionObjectiveSpec("chase_the_wind_harpies", QuestObjectiveType.KillEnemyType, "enemy_forest_wind_harpy", 7) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 290) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", "map_087_cova_lodo", "cova_do_lodo", 384, new RegionQuestSpec(UltimaLampadaDarkFish, BiomeType.Forest, UltimaLampadaEagles, 6500,
                new[] { new RegionObjectiveSpec("chase_the_dark_fish", QuestObjectiveType.KillEnemyType, "enemy_forest_dark_fish", 8) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 295) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "makarios_cartas", "map_087_cova_lodo", "saloes_alagados", 388, new RegionQuestSpec(UltimaLampadaEels, BiomeType.Forest, UltimaLampadaHarpies, 6600,
                new[] { new RegionObjectiveSpec("chase_the_great_eels", QuestObjectiveType.KillEnemyType, "enemy_forest_great_eel", 6) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 295) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", "map_087_cova_lodo", "comportas_artifices", 394, new RegionQuestSpec(UltimaLampadaOctopus, BiomeType.Forest, UltimaLampadaDarkFish, 6700,
                new[] { new RegionObjectiveSpec("chase_the_mud_octopus", QuestObjectiveType.KillEnemyType, "enemy_forest_mud_octopus", 6) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 300) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "makarios_cartas", "map_088_cova_treva", "cova_da_treva", 398, new RegionQuestSpec(UltimaLampadaSerpents, BiomeType.Forest, UltimaLampadaEels, 6800,
                new[] { new RegionObjectiveSpec("chase_the_blind_serpents", QuestObjectiveType.KillEnemyType, "enemy_forest_dimmed_python", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 300) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", "map_088_cova_treva", "cova_da_treva", 402, new RegionQuestSpec(UltimaLampadaLizards, BiomeType.Forest, UltimaLampadaOctopus, 6900,
                new[] { new RegionObjectiveSpec("chase_the_shadow_lizards", QuestObjectiveType.KillEnemyType, "enemy_forest_shadow_lizard", 6) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 305) })),
            new CityQuestSpec(CityCatalog.FortalezaUltimaLampada, "makarios_cartas", "map_088_cova_treva", "tuneis_cegos", 406, new RegionQuestSpec(UltimaLampadaDragons, BiomeType.Forest, UltimaLampadaSerpents, 7000,
                new[] { new RegionObjectiveSpec("chase_the_shadow_dragons", QuestObjectiveType.KillEnemyType, "enemy_forest_lesser_shadow_dragon", 5) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 305) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "ammo_caravaneiro", "map_092_rio_de_luz", "foz_oeste", 432, new RegionQuestSpec(CaisColunaRiverShadows, BiomeType.Forest, CaisColunaStowaways, 7100,
                new[] { new RegionObjectiveSpec("chase_the_swimming_shadows", QuestObjectiveType.KillEnemyType, "enemy_forest_swimming_shadow", 7) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 300) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "adda_roda", "map_093_barca_da_lua", "conves_da_lua", 436, new RegionQuestSpec(CaisColunaHold, BiomeType.Forest, CaisColunaBirds, 7200,
                new[] { new RegionObjectiveSpec("chase_the_hold_shadows", QuestObjectiveType.KillEnemyType, "enemy_forest_hold_shadow", 6) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 300) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "ammo_caravaneiro", "map_093_barca_da_lua", "proa_da_lua", 442, new RegionQuestSpec(CaisColunaRopes, BiomeType.Forest, CaisColunaRiverShadows, 7300,
                new[] { new RegionObjectiveSpec("untangle_the_living_ropes", QuestObjectiveType.KillEnemyType, "enemy_forest_living_rope", 7) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 310) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "adda_roda", "map_094_roda_celeste", "roda_celeste", 446, new RegionQuestSpec(CaisColunaCrackedJars, BiomeType.Forest, CaisColunaHold, 7400,
                new[] { new RegionObjectiveSpec("break_the_cracked_jars", QuestObjectiveType.KillEnemyType, "enemy_forest_cracked_jar", 7) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 310) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "ammo_caravaneiro", "map_094_roda_celeste", "engrenagens_leste", 452, new RegionQuestSpec(CaisColunaToyArchons, BiomeType.Forest, CaisColunaRopes, 7500,
                new[] { new RegionObjectiveSpec("chase_the_toy_archons", QuestObjectiveType.KillEnemyType, "enemy_forest_toy_archon", 5) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 315) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "adda_roda", "map_095_esferas_arcontes", "esferas_dos_arcontes", 456, new RegionQuestSpec(CaisColunaChainedArchons, BiomeType.Forest, CaisColunaCrackedJars, 7600,
                new[] { new RegionObjectiveSpec("chase_the_chained_archons", QuestObjectiveType.KillEnemyType, "enemy_forest_chained_archon", 6) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 315) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "ammo_caravaneiro", "map_095_esferas_arcontes", "orbita_vazia_leste", 462, new RegionQuestSpec(CaisColunaBalloonGiants, BiomeType.Forest, CaisColunaToyArchons, 7700,
                new[] { new RegionObjectiveSpec("chase_the_balloon_giants", QuestObjectiveType.KillEnemyType, "enemy_forest_balloon_giant", 5) },
                new[] { new RegionRewardSpec("item_runa_vento", 3), new RegionRewardSpec("item_gold", 320) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "adda_roda", "map_096_jardim_de_luz", "pergola_do_jardineiro", 466, new RegionQuestSpec(CaisColunaGardenThieves, BiomeType.Forest, CaisColunaChainedArchons, 7800,
                new[] { new RegionObjectiveSpec("chase_the_garden_thieves", QuestObjectiveType.KillEnemyType, "enemy_forest_az_thief", 7) },
                new[] { new RegionRewardSpec("item_pedra_alma", 2), new RegionRewardSpec("item_gold", 320) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "ammo_caravaneiro", "map_097_barca_do_sol", "conves_do_sol", 474, new RegionQuestSpec(CaisColunaSunGulls, BiomeType.Forest, CaisColunaBalloonGiants, 7900,
                new[] { new RegionObjectiveSpec("chase_the_dimmed_gulls", QuestObjectiveType.KillEnemyType, "enemy_forest_dimmed_light_gull", 7) },
                new[] { new RegionRewardSpec("item_runa_cura", 3), new RegionRewardSpec("item_gold", 330) })),
            new CityQuestSpec(CityCatalog.CaisDaColuna, "adda_roda", "map_097_barca_do_sol", "vela_do_sol", 478, new RegionQuestSpec(CaisColunaSmokeRopes, BiomeType.Forest, CaisColunaGardenThieves, 8000,
                new[] { new RegionObjectiveSpec("cut_the_smoke_ropes", QuestObjectiveType.KillEnemyType, "enemy_forest_smoke_rope", 6) },
                new[] { new RegionRewardSpec("item_health_potion", 5), new RegionRewardSpec("item_mana_potion", 5), new RegionRewardSpec("item_gold", 330) })),
        });

        /// <summary>
        /// The level a city's quests are paid at: the walk level of the city's own map (the lowest non-outpost map that returns
        /// to it), else of the lowest map that returns to it, else 1.
        /// </summary>
        public static int LevelOf(string cityId)
        {
            int own = int.MaxValue;
            int any = int.MaxValue;
            foreach (MapDefinition map in MapCatalog.Maps)
            {
                if (map.ReturnCityId != cityId)
                {
                    continue;
                }

                any = Math.Min(any, map.WalkLevelRequirement);
                if (map.Settlement != MapSettlementKind.Outpost)
                {
                    own = Math.Min(own, map.WalkLevelRequirement);
                }
            }

            return own != int.MaxValue ? own : any != int.MaxValue ? any : 1;
        }

        private static IReadOnlyList<CityQuestSpec> AtCityLevel(List<CityQuestSpec> quests) =>
            quests.Select(q => new CityQuestSpec(q.CityId, q.GiverResidentId, q.MapId, q.ZoneId, q.MinLevel, QuestXpRules.Apply(q.Quest, QuestXpKind.City, q.PaidLevel))).ToList();

        private static readonly Dictionary<string, QuestDefinition> Built = new Dictionary<string, QuestDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Built.Clear();

        public static CityQuestSpec FindSpec(string questId) =>
            string.IsNullOrEmpty(questId) ? null : Quests.FirstOrDefault(q => q.QuestId == questId);

        public static bool IsCityQuest(string questId) => FindSpec(questId) != null;

        /// <summary>The quests <paramref name="residentId"/> hands out, in chain order.</summary>
        public static IEnumerable<CityQuestSpec> QuestsOf(string residentId) => Quests.Where(q => q.GiverResidentId == residentId);

        public static IEnumerable<CityQuestSpec> QuestsIn(string cityId) => Quests.Where(q => q.CityId == cityId);

        /// <summary>The quests of the city's own map (any level), without the ones on the road.</summary>
        public static IEnumerable<CityQuestSpec> HomeQuestsIn(string cityId) => QuestsIn(cityId).Where(q => !q.IsOnTheRoad);

        /// <summary>The quests that send the cousins out to a zone of the region at a level (<see cref="CityQuestSpec.IsOnTheRoad"/>).</summary>
        public static IEnumerable<CityQuestSpec> RoadQuestsIn(string cityId) => QuestsIn(cityId).Where(q => q.IsOnTheRoad);

        /// <summary><see cref="Decide(string, Func{string, bool}, Func{string, bool}, int, out CityQuestSpec)"/> for a hero of any level.</summary>
        public static CityQuestTalk Decide(string residentId, Func<string, bool> isActive, Func<string, bool> isCompleted, out CityQuestSpec quest) =>
            Decide(residentId, isActive, isCompleted, int.MaxValue, out quest);

        /// <summary>
        /// What <paramref name="residentId"/> does now (pure: the caller passes the quest state): the first of their quests
        /// that is running → <see cref="CityQuestTalk.Active"/>; else the first not done whose prerequisite is done →
        /// <see cref="CityQuestTalk.Offer"/>, when <paramref name="heroLevel"/> reaches its <see cref="CityQuestSpec.MinLevel"/>
        /// (a quest on the road waits for its level); else <see cref="CityQuestTalk.None"/>.
        /// </summary>
        public static CityQuestTalk Decide(string residentId, Func<string, bool> isActive, Func<string, bool> isCompleted, int heroLevel, out CityQuestSpec quest)
        {
            quest = null;
            if (string.IsNullOrEmpty(residentId) || isActive == null || isCompleted == null)
            {
                return CityQuestTalk.None;
            }

            foreach (CityQuestSpec spec in QuestsOf(residentId))
            {
                if (isActive(spec.QuestId))
                {
                    quest = spec;
                    return CityQuestTalk.Active;
                }

                if (isCompleted(spec.QuestId))
                {
                    continue;
                }

                string prerequisite = spec.Quest.PrerequisiteQuestId;
                if ((string.IsNullOrEmpty(prerequisite) || isCompleted(prerequisite)) && heroLevel >= spec.MinLevel)
                {
                    quest = spec;
                    return CityQuestTalk.Offer;
                }

                return CityQuestTalk.None;
            }

            return CityQuestTalk.None;
        }

        /// <summary>
        /// The cousins step up to <paramref name="residentId"/>: starts the offered quest on <paramref name="quests"/> and
        /// returns the line key to say (offer or progress), or null for the resident's usual line. A quest on the road is
        /// only offered from its level on (<paramref name="heroLevel"/>; left out = any level).
        /// </summary>
        public static string Talk(QuestManager quests, string residentId, int heroLevel = int.MaxValue)
        {
            if (quests == null)
            {
                return null;
            }

            switch (Decide(residentId, quests.IsActive, quests.IsCompleted, heroLevel, out CityQuestSpec spec))
            {
                case CityQuestTalk.Active:
                    return spec.ActiveKey;
                case CityQuestTalk.Offer:
                    return quests.StartQuest(GetQuest(spec.QuestId)) ? spec.OfferKey : null;
                default:
                    return null;
            }
        }

        /// <summary>The built definition of a city quest (cached), or null if <paramref name="questId"/> isn't one.</summary>
        public static QuestDefinition GetQuest(string questId)
        {
            CityQuestSpec spec = FindSpec(questId);
            if (spec == null)
            {
                // The late main story has a giver and a place like a city quest, and is found the same way (dialogue, quest log, saves).
                return LateStoryCatalog.GetQuest(questId);
            }

            if (Built.TryGetValue(questId, out QuestDefinition cached) && cached != null)
            {
                return cached;
            }

            QuestDefinition definition = RegionQuestCatalog.Build(spec.Quest, ItemDatabase.GetById);
            Built[questId] = definition;
            return definition;
        }

        public static IEnumerable<QuestDefinition> GetAllQuests() => Quests.Select(q => GetQuest(q.QuestId)).Concat(LateStoryCatalog.GetAllQuests());

        /// <summary>Registers every city quest with <paramref name="quests"/> (a quest restored from a save needs its rewards).</summary>
        public static void RegisterAll(QuestManager quests)
        {
            if (quests == null)
            {
                return;
            }

            foreach (QuestDefinition definition in GetAllQuests())
            {
                quests.RegisterDefinition(definition);
            }
        }
    }
}
