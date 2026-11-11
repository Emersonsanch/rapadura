using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>
    /// "A Lua que Não Enche" (Docs/HistoriaMeio_20a100.md): the main story between the end of the campaign (Tages and the seal, level ~20) and the first of "As Cartas da
    /// Estrada" (<see cref="LateStoryCatalog"/>, level 100). One chain from the Vila da Vó Edna through the second ring of regions 1–4 and regions 5–8 to Mingcheng, in
    /// the same shape as the letters (<see cref="LateStoryStep"/>: giver, zone, level gate, a line per cousin) and run by the same code: <see cref="LateStoryCatalog.Decide"/>,
    /// <see cref="LateStoryCatalog.Talk"/> and <see cref="LateStoryCatalog.GetQuest"/> see these steps too, so the city residents, the quest log and the saves need nothing new.
    /// Same content rules as the letters (non-boss <c>enemy_*</c> of the zone's own fauna, <c>Resources/Items</c> rewards, the place is an overworld zone), with one exception:
    /// the first step also asks for the seal of the Grutas, Tages (<see cref="SealEnemyId"/>), credited at once when he was already faced (the campaign is the way in).
    /// The last step opens the first letter (<see cref="LateStoryCatalog.SteppeFoals"/> requires it).
    /// </summary>
    public static class MidStoryCatalog
    {
        public const string LimiarMoon = "quest_mid_story_r04_moon";
        public const string ZarafshanGarden = "quest_mid_story_r01_garden";
        public const string AreiasThrone = "quest_mid_story_r02_throne";
        public const string KaggenSummit = "quest_mid_story_r03_summit";
        public const string LimiarAqueducts = "quest_mid_story_r04_aqueducts";
        public const string CostaGulls = "quest_mid_story_r05_gulls";
        public const string CostaBaobabs = "quest_mid_story_r05_baobabs";
        public const string CostaMangrove = "quest_mid_story_r05_mangrove";
        public const string DeltaRain = "quest_mid_story_r06_rain";
        public const string DeltaDeadwood = "quest_mid_story_r06_deadwood";
        public const string TetoJunipers = "quest_mid_story_r07_junipers";
        public const string TetoPeaks = "quest_mid_story_r07_peaks";
        public const string MingchengLions = "quest_mid_story_r08_lions";
        public const string MingchengLock = "quest_mid_story_r08_lock";
        public const string MingchengFullMoon = "quest_mid_story_r08_full_moon";

        /// <summary>The step that opens "As Cartas da Estrada" (the prerequisite of <see cref="LateStoryCatalog.SteppeFoals"/>).</summary>
        public const string FinalQuestId = MingchengFullMoon;

        /// <summary>Tages, the Habitante Ancestral: the boss of the campaign's end (<c>Docs/Narrative.md</c> §4), the first objective of the chain.</summary>
        public const string SealEnemyId = "enemy_cave_dweller";

        /// <summary>Objective id of "the seal is remade" in the first step.</summary>
        public const string SealObjectiveId = "remake_the_seal";

        /// <summary>"Too low" lines of this chain, per cousin (the letters have their own, <see cref="LateStoryCatalog.TooLowKeyBase"/>).</summary>
        public const string TooLowKeyBase = "quest.mid_story.too_low.";

        /// <summary>The Vila's resident who opens the chain: Vó Edna on her iwan.</summary>
        public const string FirstGiver = "vo_edna";

        public static readonly IReadOnlyList<LateStoryStep> Steps = new List<LateStoryStep>
        {
            // -- Região 4, Grutas do Limiar: the end of the campaign. Vó Edna sends the cousins to ask Tages why the moon won't fill --
            Seal(4, CityCatalog.VilaZefa, FirstGiver, "salao_de_tages", "map_020_tages", 20, LimiarMoon, 300,
                Reward("item_health_potion", 3), Reward("item_mana_potion", 3), Reward("item_gold", 40)),

            // -- The second ring of regions 1–4 (levels 24–40): the catchers Tages named --
            Step(1, CityCatalog.CopasGarajonay, "gara", "jardim_mihrshah", "map_006_mihrshah", 24, ZarafshanGarden, LimiarMoon, 400, "enemy_mountain_stone_golem", 6,
                Reward("item_health_potion", 3), Reward("item_teriaga", 1), Reward("item_gold", 50)),
            Step(2, CityCatalog.TamborRubro, "ashmunikal", "trono_de_namrael", "map_012_namrael", 30, AreiasThrone, ZarafshanGarden, 560, "enemy_desert_broken_lantern", 6,
                Reward("item_mana_potion", 3), Reward("item_cha_gengibre", 1), Reward("item_gold", 60)),
            Step(3, CityCatalog.SaloesDeBronze, "napirasu", "cume_dos_mazans", "map_018_mazans", 36, KaggenSummit, AreiasThrone, 720, "enemy_mountain_ill_wind_mist", 6,
                Reward("item_health_potion", 4), Reward("item_oleo_fogo_eterno", 1), Reward("item_gold", 70)),
            Step(4, CityCatalog.MercadoDosTuneis, "takhsich", "aquedutos_profundos", "map_023_canion_roda", 40, LimiarAqueducts, KaggenSummit, 880, "enemy_cave_lantern_fish", 6,
                Reward("item_mana_potion", 4), Reward("item_passagem_dhow", 1), Reward("item_gold", 80)),

            // -- Região 5, Costa das Monções --
            Step(5, CityCatalog.TaaPwani, "nuru", "praia_dos_dhows", "map_025_taa_pwani", 46, CostaGulls, LimiarAqueducts, 960, "enemy_forest_thief_gull", 7,
                Reward("item_health_potion", 4), Reward("item_concha_respiravel", 1), Reward("item_gold", 90)),
            Step(5, CityCatalog.Aratta, "ensuhkeshdanna", "bosque_dos_baobas", "map_026_auroques", 52, CostaBaobabs, CostaGulls, 1060, "enemy_mountain_baboon", 6,
                Reward("item_mana_potion", 4), Reward("item_botas_raiz", 1), Reward("item_gold", 100)),
            Step(5, CityCatalog.TaaPwani, "liyongo", "mangue_raso", "map_030_liyongo", 60, CostaMangrove, CostaBaobabs, 1180, "enemy_forest_mangrove_crocodile", 6,
                Reward("item_health_potion", 4), Reward("item_vara_equilibrio", 1), Reward("item_gold", 110)),

            // -- Região 6, Delta de Jhilpur --
            Step(6, CityCatalog.Jhilpur, "salmaios", "cascatas_da_chuva", "map_032_raiz_viva", 64, DeltaRain, CostaMangrove, 1340, "enemy_forest_dimmed_python", 6,
                Reward("item_mana_potion", 5), Reward("item_amuleto_antiveneno", 1), Reward("item_gold", 120)),
            Step(6, CityCatalog.PalafitasTarchon, "ramtha", "mata_morta", "map_035_coracao_lodo", 74, DeltaDeadwood, DeltaRain, 1500, "enemy_forest_dimmed_troll", 5,
                Reward("item_health_potion", 5), Reward("item_roupa_termica", 1), Reward("item_gold", 130)),

            // -- Região 7, Teto do Mundo --
            Step(7, CityCatalog.Sangkent, "aba", "encosta_dos_zimbros", "map_037_calhas", 78, TetoJunipers, DeltaDeadwood, 1640, "enemy_mountain_brown_bear", 6,
                Reward("item_mana_potion", 5), Reward("item_grampos_escalada", 1), Reward("item_gold", 140)),
            Step(7, CityCatalog.Sangkent, "baraies", "picos_gelados", "map_038_geleira", 86, TetoPeaks, TetoJunipers, 1760, "enemy_mountain_almas", 5,
                Reward("item_health_potion", 5), Reward("item_capa_tempestade", 1), Reward("item_gold", 150)),

            // -- Região 8, Planície de Mingcheng: the moon fills, and Vó Edna finds what is sewn into the Caderno --
            Step(8, CityCatalog.Mingcheng, "bogu", "caminho_dos_leoes", "map_041_mingcheng", 90, MingchengLions, TetoPeaks, 1880, "enemy_forest_stone_lion", 6,
                Reward("item_mana_potion", 5), Reward("item_selo_sol", 1), Reward("item_gold", 160)),
            Step(8, CityCatalog.Mingcheng, "lin_deng", "eclusa_da_lua", "map_043_canal_das_lanternas", 94, MingchengLock, MingchengLions, 2000, "enemy_forest_stone_turtle", 5,
                Reward("item_health_potion", 5), Reward("item_lamparina_runica", 1), Reward("item_gold", 170)),
            Step(8, CityCatalog.Mingcheng, "fuduodan", "fossos_de_terracota", "map_041_mingcheng", 97, MingchengFullMoon, MingchengLock, 2150, "enemy_forest_terracotta_soldier", 6,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 180)),
        };

        public static LateStoryStep First => Steps[0];

        public static LateStoryStep Last => Steps[Steps.Count - 1];

        public static LateStoryStep FindStep(string questId) =>
            string.IsNullOrEmpty(questId) ? null : Steps.FirstOrDefault(s => s.QuestId == questId);

        public static IEnumerable<LateStoryStep> StepsIn(int region) => Steps.Where(s => s.Region == region);

        private static RegionRewardSpec Reward(string itemId, int quantity) => new RegionRewardSpec(itemId, quantity);

        /// <summary><paramref name="experience"/> is the authored XP; above level 50 the step pays <see cref="QuestXpRules.MidStoryShare"/> of the XP of
        /// <paramref name="minLevel"/> when that is more (<see cref="QuestXpRules"/>).</summary>
        private static LateStoryStep Step(int region, string cityId, string giver, string zoneId, string mapId, int minLevel, string questId, string prerequisite, int experience,
            string enemyId, int killCount, params RegionRewardSpec[] rewards)
        {
            var objectives = new[]
            {
                new RegionObjectiveSpec(LateStoryCatalog.ReachObjectiveId, QuestObjectiveType.ReachLocation, zoneId, 1),
                new RegionObjectiveSpec(LateStoryCatalog.KillObjectiveId, QuestObjectiveType.KillEnemyType, enemyId, killCount),
            };
            return new LateStoryStep(region, cityId, giver, zoneId, mapId, minLevel, new RegionQuestSpec(questId, BiomeType.Forest, prerequisite, QuestXpRules.For(QuestXpKind.MidStory, minLevel, experience), objectives, rewards));
        }

        /// <summary>The root: no prerequisite quest; the seal (Tages, one kill, credited when already faced) and the visit to his hall.</summary>
        private static LateStoryStep Seal(int region, string cityId, string giver, string zoneId, string mapId, int minLevel, string questId, int experience,
            params RegionRewardSpec[] rewards)
        {
            var objectives = new[]
            {
                new RegionObjectiveSpec(SealObjectiveId, QuestObjectiveType.KillEnemyType, SealEnemyId, 1),
                new RegionObjectiveSpec(LateStoryCatalog.ReachObjectiveId, QuestObjectiveType.ReachLocation, zoneId, 1),
            };
            return new LateStoryStep(region, cityId, giver, zoneId, mapId, minLevel, new RegionQuestSpec(questId, BiomeType.Caves, null, QuestXpRules.For(QuestXpKind.MidStory, minLevel, experience), objectives, rewards));
        }
    }
}
