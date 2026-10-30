using System.Collections.Generic;
using System.Linq;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>One villager of the world-map village hub: the dialogue asset Main.unity's NPC uses, its quest and cousin.</summary>
    public sealed class VillageNpcSpec
    {
        /// <summary>Short id, e.g. <c>village_survivor</c> (dialogue <c>dialogue_village_survivor</c>, objective target <c>npc_village_survivor</c>).</summary>
        public readonly string NpcKey;

        /// <summary>The quest this villager gives (used for the "(!)" marker only, the graph does the rest).</summary>
        public readonly string QuestId;

        /// <summary>The cousin whose personal story this is (shown next to the name).</summary>
        public readonly CharacterId Hero;

        /// <summary>Name key; null = <c>dialogue.&lt;npc&gt;.name</c>.</summary>
        private readonly string _nameKey;

        public VillageNpcSpec(string npcKey, string questId, CharacterId hero, string nameKey = null)
        {
            NpcKey = npcKey;
            QuestId = questId;
            Hero = hero;
            _nameKey = nameKey;
        }

        public string DialogueId => "dialogue_" + NpcKey;
        public string DialogueResourcePath => VillageNpcCatalog.DialogueFolder + DialogueId;
        public string NameKey => _nameKey ?? "dialogue." + NpcKey + ".name";
    }

    /// <summary>
    /// The personal-story villagers (one per cousin) and Maria's gathering point (the convergence quest), the
    /// same <c>Resources/Dialogue</c> graphs the <c>NpcInteractable</c>s of Main.unity use. Main.unity only loads
    /// for encounters, with its village switched off, so these are reached from the world map instead: the
    /// "Vila" hub (<c>VillageHubButton</c>) is offered while the token stands in <see cref="VillageRegion"/>,
    /// where Vó Zefa's farm and the village are in the story (<c>StoryTexts</c>).
    /// </summary>
    public static class VillageNpcCatalog
    {
        public const string DialogueFolder = "Dialogue/";
        public const BiomeType VillageRegion = BiomeType.Forest;

        /// <summary>Name key of the gathering point (not an NPC of its own: Maria/Joaquim speak there).</summary>
        public const string GatheringPointNameKey = "ui.village.gathering_point";

        public static readonly IReadOnlyList<VillageNpcSpec> Npcs = new List<VillageNpcSpec>
        {
            new VillageNpcSpec("village_survivor", "quest_joaquim_echoes_of_home", CharacterId.Joaquim),
            new VillageNpcSpec("temple_acolyte", "quest_maria_fading_light", CharacterId.Maria),
            new VillageNpcSpec("windtrail_scholar", "quest_maithe_windtrail", CharacterId.Maithe),
            new VillageNpcSpec("ruins_archivist", "quest_icaro_ancient_gears", CharacterId.Icaro),
            new VillageNpcSpec("hermit_mystic", "quest_lavine_redtide", CharacterId.Lavine),
            new VillageNpcSpec("maria_gathering_point", "quest_convergence_the_gathering", CharacterId.Maria, GatheringPointNameKey),
        };

        /// <summary>Whether the village hub belongs to <paramref name="region"/>.</summary>
        public static bool IsVillageRegion(BiomeType? region) => region.HasValue && region.Value == VillageRegion;

        public static VillageNpcSpec Find(string npcKey) => Npcs.FirstOrDefault(n => n.NpcKey == npcKey);

        /// <summary>The villager's conversation (<c>Resources/Dialogue/dialogue_&lt;npc&gt;</c>), or null if the asset is missing.</summary>
        public static DialogueDefinition LoadDialogue(VillageNpcSpec npc)
        {
            DialogueDefinition asset = npc != null ? Resources.Load<DialogueDefinition>(npc.DialogueResourcePath) : null;
            // + "Soube das novidades da estrada?" (StoryBeatSeen): the villager comments on the latest interlude.
            return VillageStoryNews.WithNews(asset, npc != null ? npc.NpcKey : null);
        }

        /// <summary>"(!)": the villager's quest can be taken now, or waits to be handed in.</summary>
        public static bool HasNews(DialogueQuestState questState)
        {
            return questState == DialogueQuestState.Available || questState == DialogueQuestState.ObjectivesComplete;
        }
    }
}
