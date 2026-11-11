using System;
using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Items;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Cities;
using UnityEngine;

namespace Rapadura.Gameplay.Quests
{
    /// <summary>One step of the late main story: which quest, who hands it out, where it happens and from which level it opens.</summary>
    public sealed class LateStoryStep
    {
        /// <summary>World region of Docs/Mapas100.md (1..16: 9..16 for the letters, 1..8 for <see cref="MidStoryCatalog"/>).</summary>
        public readonly int Region;
        public readonly string CityId;
        public readonly string GiverResidentId;

        /// <summary>Overworld zone id the step sends the cousins to (<c>OverworldZone.Id</c>, the id of a ReachLocation objective).</summary>
        public readonly string ZoneId;

        /// <summary>Catalog map of the zone (<c>map_NNN_*</c>).</summary>
        public readonly string MapId;

        /// <summary>The hero's level from which the giver hands the step out.</summary>
        public readonly int MinLevel;

        public readonly RegionQuestSpec Quest;

        public LateStoryStep(int region, string cityId, string giverResidentId, string zoneId, string mapId, int minLevel, RegionQuestSpec quest)
        {
            Region = region;
            CityId = cityId;
            GiverResidentId = giverResidentId;
            ZoneId = zoneId;
            MapId = mapId;
            MinLevel = minLevel;
            Quest = quest;
        }

        public string QuestId => Quest.QuestId;
        public string KeyBase => Quest.KeyBase;

        /// <summary>What the giver says to <paramref name="hero"/> when the step starts: a different line per cousin.</summary>
        public string OfferKey(CharacterId hero) => KeyBase + ".offer." + LateStoryCatalog.HeroKey(hero);

        /// <summary>What the giver says while the step runs.</summary>
        public string ActiveKey => KeyBase + ".active";
    }

    /// <summary>What a giver of the late story does when the cousins step up to them.</summary>
    public enum LateStoryTalk
    {
        /// <summary>Nothing to say about the story: the resident's usual line (or their city quests).</summary>
        None = 0,
        /// <summary>Starts the step and says its offer.</summary>
        Offer = 1,
        /// <summary>The step runs: says how it goes.</summary>
        Active = 2,
        /// <summary>The previous step is done but the hero is below the step's level.</summary>
        TooLow = 3,
        /// <summary>The whole chain is done: the last giver says the closing line for that cousin.</summary>
        Finale = 4,
    }

    /// <summary>
    /// "As Cartas da Estrada" (Docs/HistoriaFinal_Cartas.md): the main story of regions 9–16, one chain from the Estepe dos Nartes to the Varanda of map 100,
    /// content as code like <see cref="CityQuestCatalog"/> and kept apart from it because a step also has a level gate, a place and a line per cousin. Same content
    /// rules: kill targets are non-boss <c>enemy_*</c> assets of the zone's own fauna, rewards are <c>Resources/Items</c> ids; a location objective is the id of an
    /// overworld zone, reported by <see cref="LateStoryLocationReporter"/>. Every step requires the one before it and a higher level, so the chain opens in order.
    /// The first letter requires the last step of "A Lua que Não Enche" (<see cref="MidStoryCatalog"/>, levels 20–97), and the lookups below (<see cref="FindStep"/>,
    /// <see cref="StepsOf"/>, <see cref="Decide"/>, <see cref="Talk"/>, <see cref="GetQuest"/>) see both chains as one story (<see cref="WholeStory"/>).
    /// </summary>
    public static class LateStoryCatalog
    {
        public const string SteppeFoals = "quest_late_story_r09_foals";
        public const string SteppeKurgans = "quest_late_story_r09_kurgans";
        public const string PeaksBridges = "quest_late_story_r10_bridges";
        public const string PeaksBells = "quest_late_story_r10_bells";
        public const string IceCanals = "quest_late_story_r11_canals";
        public const string IceGalleries = "quest_late_story_r11_galleries";
        public const string ForgeGardens = "quest_late_story_r12_gardens";
        public const string ForgeFurnaces = "quest_late_story_r12_furnaces";
        public const string RiverPapyrus = "quest_late_story_r13_papyrus";
        public const string RiverRowers = "quest_late_story_r13_rowers";
        public const string MixBarley = "quest_late_story_r14_barley";
        public const string MixGate = "quest_late_story_r14_gate";
        public const string DeepRampart = "quest_late_story_r15_rampart";
        public const string DeepBrink = "quest_late_story_r15_brink";
        public const string SkyRiver = "quest_late_story_r16_river";
        public const string SkyColumn = "quest_late_story_r16_column";
        public const string SkyPorch = "quest_late_story_r16_porch";

        /// <summary>Objective id of "reach the place" in every step.</summary>
        public const string ReachObjectiveId = "reach_the_place";

        /// <summary>Objective id of "free the beasts" in every step that has a fight.</summary>
        public const string KillObjectiveId = "free_the_beasts";

        public const string FinaleKeyBase = "quest.late_story.finale.";
        public const string TooLowKeyBase = "quest.late_story.too_low.";

        public static readonly IReadOnlyList<LateStoryStep> Steps = new List<LateStoryStep>
        {
            // -- Região 9, Estepe dos Nartes --
            Step(9, CityCatalog.AcampamentoSoslan, "soslan", "pistas_de_corrida", "map_047_soslan", 100, SteppeFoals, MidStoryCatalog.FinalQuestId, 2300, "enemy_mountain_young_griffin", 5,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 180)),
            Step(9, CityCatalog.AcampamentoSoslan, "satana", "kurgans_de_ouro", "map_048_kurgans", 106, SteppeKurgans, SteppeFoals, 2400, "enemy_mountain_bronze_wolf", 6,
                Reward("item_lamparina_runica", 1), Reward("item_gold", 190)),

            // -- Região 10, Picos de Farnkesh --
            Step(10, CityCatalog.Farnkesh, "tadi", "pontes_de_vento", "map_055_pontes_vento", 148, PeaksBridges, SteppeKurgans, 3000, "enemy_mountain_rope_ushabti", 6,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 210)),
            Step(10, CityCatalog.Farnkesh, "shad_ohrmizd", "pinaculo_do_sino", "map_056_ermidas", 156, PeaksBells, PeaksBridges, 3100, "enemy_desert_broken_lantern", 6,
                Reward("item_runa_vento", 3), Reward("item_gold", 220)),

            // -- Região 11, Mar Gelado --
            Step(11, CityCatalog.PortoDaLuaBranca, "yirkap", "canais_de_gelo", "map_060_canais_gelo", 184, IceCanals, PeaksBells, 3200, "enemy_mountain_giant_pike", 6,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 230)),
            Step(11, CityCatalog.PortoDaLuaBranca, "pera", "galerias_dos_mamutes", "map_062_galerias_mamutes", 200, IceGalleries, IceCanals, 3300, "enemy_mountain_burrow_mammoth", 4,
                Reward("item_roupa_termica", 1), Reward("item_gold", 230)),

            // -- Região 12, Arquipélago de Hinokura --
            Step(12, CityCatalog.Hinokura, "akane", "jardins_de_lava_laqueada", "map_066_jardins_lava", 224, ForgeGardens, IceGalleries, 3400, "enemy_mountain_dimmed_shisa", 6,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 240)),
            Step(12, CityCatalog.Hinokura, "sango", "patio_das_fornalhas", "map_069_nove_fornalhas", 254, ForgeFurnaces, ForgeGardens, 3500, "enemy_mountain_furnace_ogre", 4,
                Reward("item_barco_junco", 1), Reward("item_gold", 250)),

            // -- Região 13, Rio das Doze Horas --
            Step(13, CityCatalog.CaisDeMesektet, "sinuhe", "margem_de_papiro", "map_071_cais_mesektet", 258, RiverPapyrus, ForgeFurnaces, 3600, "enemy_forest_shadow_scarab", 8,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 250)),
            Step(13, CityCatalog.CaisDeMesektet, "djedi", "praia_dos_remadores", "map_071_cais_mesektet", 262, RiverRowers, RiverPapyrus, 3700, "enemy_forest_dimmed_ibis", 6,
                Reward("item_lamparina_runica", 1), Reward("item_gold", 260)),

            // -- Região 14, Margens da Mistura --
            Step(14, CityCatalog.DurJarras, "utnapishtim", "terracos_de_cevada", "map_077_dur_jarras", 300, MixBarley, RiverRowers, 3800, "enemy_forest_bitumen_locust", 8,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 260)),
            Step(14, CityCatalog.DurJarras, "sin_leqi_unninni", "portao_de_tijolo", "map_077_dur_jarras", 302, MixGate, MixBarley, 3900, "enemy_forest_brick_lion", 6,
                Reward("item_runa_cura", 3), Reward("item_gold", 270)),

            // -- Região 15, Limiar Profundo --
            Step(15, CityCatalog.FortalezaUltimaLampada, "pamour_lampadas", "muralha_em_arco", "map_083_fortaleza_ultima_lampada", 340, DeepRampart, MixGate, 4000, "enemy_mountain_lacquer_ushabti", 6,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 270)),
            Step(15, CityCatalog.FortalezaUltimaLampada, "makarios_cartas", "beira_do_abismo", "map_083_fortaleza_ultima_lampada", 344, DeepBrink, DeepRampart, 4100, "enemy_forest_shadow_biter", 8,
                Reward("item_lamparina_runica", 1), Reward("item_gold", 280)),

            // -- Região 16, Reinos do Céu: Adda sends them up, step by step, to the Coluna and the Varanda --
            Step(16, CityCatalog.CaisDaColuna, "adda_roda", "rio_de_luz", "map_092_rio_de_luz", 426, SkyRiver, DeepBrink, 4200, "enemy_forest_dimmed_light_fish", 7,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 280)),
            Step(16, CityCatalog.CaisDaColuna, "adda_roda", "pes_da_coluna", "map_098_pes_da_coluna", 482, SkyColumn, SkyRiver, 4400, "enemy_forest_creeping_dark", 8,
                Reward("item_capa_tempestade", 1), Reward("item_gold", 290)),
            Step(16, CityCatalog.CaisDaColuna, "adda_roda", "varanda_do_tempo_de_depois", "map_100_varanda", 495, SkyPorch, SkyColumn, 4600, null, 0,
                Reward("item_health_potion", 5), Reward("item_mana_potion", 5), Reward("item_gold", 300)),
        };

        private static readonly Dictionary<string, QuestDefinition> Built = new Dictionary<string, QuestDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Built.Clear();

        public static LateStoryStep Last => Steps[Steps.Count - 1];

        /// <summary>"Icaro" → "icaro": the suffix of a per-cousin key.</summary>
        public static string HeroKey(CharacterId hero) => hero.ToString().ToLowerInvariant();

        /// <summary>The whole main story after the campaign, in order: "A Lua que Não Enche" (<see cref="MidStoryCatalog"/>), then the letters (<see cref="Steps"/>).</summary>
        public static IEnumerable<LateStoryStep> WholeStory => MidStoryCatalog.Steps.Concat(Steps);

        public static LateStoryStep FindStep(string questId) =>
            string.IsNullOrEmpty(questId) ? null : WholeStory.FirstOrDefault(s => s.QuestId == questId);

        public static IEnumerable<LateStoryStep> StepsOf(string residentId) => WholeStory.Where(s => s.GiverResidentId == residentId);

        public static IEnumerable<LateStoryStep> StepsIn(int region) => Steps.Where(s => s.Region == region);

        /// <summary>
        /// What <paramref name="residentId"/> does now (pure: the caller passes the quest state and the hero's level): a running step of theirs → Active; the next one whose
        /// prerequisite is done → Offer, or TooLow under its level; once the last step of the chain is done, the last giver → Finale; else None. The root of the chain (no
        /// prerequisite, Vó Edna) stays silent under its level: the cousins meet her from level 1, long before the story needs her.
        /// </summary>
        public static LateStoryTalk Decide(string residentId, Func<string, bool> isActive, Func<string, bool> isCompleted, int heroLevel, out LateStoryStep step)
        {
            step = null;
            if (string.IsNullOrEmpty(residentId) || isActive == null || isCompleted == null)
            {
                return LateStoryTalk.None;
            }

            foreach (LateStoryStep candidate in StepsOf(residentId))
            {
                if (isActive(candidate.QuestId))
                {
                    step = candidate;
                    return LateStoryTalk.Active;
                }

                if (isCompleted(candidate.QuestId))
                {
                    continue;
                }

                string prerequisite = candidate.Quest.PrerequisiteQuestId;
                if (!string.IsNullOrEmpty(prerequisite) && !isCompleted(prerequisite))
                {
                    return LateStoryTalk.None;
                }

                if (heroLevel < candidate.MinLevel && string.IsNullOrEmpty(prerequisite))
                {
                    return LateStoryTalk.None;
                }

                step = candidate;
                return heroLevel >= candidate.MinLevel ? LateStoryTalk.Offer : LateStoryTalk.TooLow;
            }

            if (residentId == Last.GiverResidentId && isCompleted(Last.QuestId))
            {
                step = Last;
                return LateStoryTalk.Finale;
            }

            return LateStoryTalk.None;
        }

        /// <summary>
        /// The cousins step up to <paramref name="residentId"/>: starts the offered step on <paramref name="quests"/> and returns the line key to say, or null when the
        /// story has nothing for this resident (the caller goes on to their city quests and usual line).
        /// </summary>
        public static string Talk(QuestManager quests, string residentId, CharacterId hero, int heroLevel)
        {
            if (quests == null)
            {
                return null;
            }

            switch (Decide(residentId, quests.IsActive, quests.IsCompleted, heroLevel, out LateStoryStep step))
            {
                case LateStoryTalk.Active:
                    return step.ActiveKey;
                case LateStoryTalk.Offer:
                    return quests.StartQuest(GetQuest(step.QuestId)) ? step.OfferKey(hero) : null;
                case LateStoryTalk.TooLow:
                    return (MidStoryCatalog.FindStep(step.QuestId) != null ? MidStoryCatalog.TooLowKeyBase : TooLowKeyBase) + HeroKey(hero);
                case LateStoryTalk.Finale:
                    return FinaleKeyBase + HeroKey(hero);
                default:
                    return null;
            }
        }

        /// <summary>The built definition of a step (cached, <see cref="QuestType.MainStory"/>), or null if <paramref name="questId"/> isn't one.</summary>
        public static QuestDefinition GetQuest(string questId)
        {
            LateStoryStep step = FindStep(questId);
            if (step == null)
            {
                return null;
            }

            if (Built.TryGetValue(questId, out QuestDefinition cached) && cached != null)
            {
                return cached;
            }

            QuestDefinition definition = RegionQuestCatalog.Build(step.Quest, ItemDatabase.GetById);
            definition.SetDataForTests(definition.QuestId, definition.TitleKey, definition.DescriptionKey, QuestType.MainStory, definition.Objectives.ToList(),
                definition.PrerequisiteQuestIds.ToList(), definition.RewardExperience, definition.RewardItems.ToList());
            Built[questId] = definition;
            return definition;
        }

        /// <summary>Every step of <see cref="WholeStory"/> (the quest log, the saves and <c>CityQuestCatalog.GetAllQuests</c>).</summary>
        public static IEnumerable<QuestDefinition> GetAllQuests() => WholeStory.Select(s => GetQuest(s.QuestId));

        private static RegionRewardSpec Reward(string itemId, int quantity) => new RegionRewardSpec(itemId, quantity);

        private static LateStoryStep Step(int region, string cityId, string giver, string zoneId, string mapId, int minLevel, string questId, string prerequisite, int experience,
            string enemyId, int killCount, params RegionRewardSpec[] rewards)
        {
            var objectives = new List<RegionObjectiveSpec> { new RegionObjectiveSpec(ReachObjectiveId, QuestObjectiveType.ReachLocation, zoneId, 1) };
            if (!string.IsNullOrEmpty(enemyId))
            {
                objectives.Add(new RegionObjectiveSpec(KillObjectiveId, QuestObjectiveType.KillEnemyType, enemyId, killCount));
            }

            return new LateStoryStep(region, cityId, giver, zoneId, mapId, minLevel,
                new RegionQuestSpec(questId, BiomeType.Forest, prerequisite, experience, objectives.ToArray(), rewards));
        }
    }
}
