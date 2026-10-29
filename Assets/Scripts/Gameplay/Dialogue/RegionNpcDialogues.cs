using System;
using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Biomes;
using UnityEngine;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>
    /// Builds the conversation of each region quest-giver (<see cref="RegionQuestCatalog.Npcs"/>) as a
    /// <see cref="DialogueDefinition"/>, in code like the quests themselves (no asset, no seeder). One graph
    /// per NPC, all with the same shape:
    /// <list type="bullet">
    /// <item><c>hub</c> → a greeting per hero (<see cref="DialogueConditionType.ActiveCharacter"/>; every NPC
    /// knows the family: Maithe the eldest, Maria, Joaquim the stubborn protector, Ícaro the inventor,
    /// Lavine the youngest) or <c>greet_any</c> when no hero is selected.</item>
    /// <item>The greeting and <c>menu</c> share one list of choices gated by quest state: offer quest 1 →
    /// "how is it going" while active → offer quest 2 once quest 1 is done → "any news?" after both.</item>
    /// <item>Pre-boss hint (<c>boss</c>, with a special line for <see cref="RegionNpcSpec.BossHintHero"/>) only
    /// while <see cref="BossReadyFlag"/> is set, and a thank-you (<c>cleared</c>) once
    /// <see cref="RegionClearedFlag"/> is: whoever opens the conversation sets/clears both flags on the
    /// <see cref="DialogueManager"/> from the world map state first (<see cref="ApplyRegionFlags"/>).</item>
    /// <item>Quest 1's offer has a hero-specific accept line for <see cref="RegionNpcSpec.AcceptHero"/>.</item>
    /// </list>
    /// Every shown node has a choice or auto-advances, so the box can always be closed (DialogueUIController
    /// shows no button on a node with neither). Texts: <see cref="RegionQuestTexts"/>.
    /// </summary>
    public static class RegionNpcDialogues
    {
        public const string BossReadyFlag = "region_npc.boss_ready";
        public const string RegionClearedFlag = "region_npc.region_cleared";

        /// <summary>Choice labels shared by every region NPC.</summary>
        public const string SharedKeyBase = "dialogue.region_npc";
        public const string ChoiceAcceptKey = SharedKeyBase + ".choice_accept";
        public const string ChoiceDeclineKey = SharedKeyBase + ".choice_decline";
        public const string ChoiceProgressKey = SharedKeyBase + ".choice_progress";
        public const string ChoiceNewsKey = SharedKeyBase + ".choice_news";
        public const string ChoiceBossKey = SharedKeyBase + ".choice_boss";
        public const string ChoiceClearedKey = SharedKeyBase + ".choice_cleared";
        public const string ChoiceGoodbyeKey = SharedKeyBase + ".choice_goodbye";

        public const string HubNodeId = "hub";
        public const string GreetAnyNodeId = "greet_any";
        public const string MenuNodeId = "menu";
        public const string BossNodeId = "boss";
        public const string BossSpecialNodeId = "boss_special";

        private static readonly CharacterId[] Heroes = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        private static readonly Dictionary<BiomeType, DialogueDefinition> Built = new Dictionary<BiomeType, DialogueDefinition>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Built.Clear();

        public static string GreetNodeId(CharacterId hero) => "greet_" + hero.ToString().ToLowerInvariant();

        /// <summary>The cached conversation of <paramref name="region"/>'s quest-giver, or null if the region has none.</summary>
        public static DialogueDefinition Get(BiomeType region)
        {
            if (Built.TryGetValue(region, out DialogueDefinition cached) && cached != null)
            {
                return cached;
            }

            RegionNpcSpec npc = RegionQuestCatalog.NpcFor(region);
            if (npc == null)
            {
                return null;
            }

            DialogueDefinition dialogue = Build(npc);
            dialogue.hideFlags = HideFlags.DontUnloadUnusedAsset;
            Built[region] = dialogue;
            return dialogue;
        }

        /// <summary>Sets/clears the boss flags the graph reads. Call right before <see cref="DialogueManager.StartDialogue"/>.</summary>
        public static void ApplyRegionFlags(DialogueManager dialogue, bool bossReady, bool regionCleared)
        {
            if (dialogue == null)
            {
                return;
            }

            dialogue.SetFlag(BossReadyFlag, bossReady);
            dialogue.SetFlag(RegionClearedFlag, regionCleared);
        }

        /// <summary>Builds a fresh (uncached) graph for <paramref name="npc"/>.</summary>
        public static DialogueDefinition Build(RegionNpcSpec npc)
        {
            string key = npc.KeyBase;
            string q1 = npc.FirstQuestId;
            string q2 = npc.SecondQuestId;

            var nodes = new List<DialogueNode>();

            var hub = Node(npc, HubNodeId, null);
            foreach (CharacterId hero in Heroes)
            {
                hub.Redirects.Add(new DialogueRedirect { TargetNodeId = GreetNodeId(hero), Conditions = { HeroIs(hero) } });
            }

            hub.Redirects.Add(new DialogueRedirect { TargetNodeId = GreetAnyNodeId });
            nodes.Add(hub);

            foreach (CharacterId hero in Heroes)
            {
                nodes.Add(WithMenu(Node(npc, GreetNodeId(hero), key + "." + GreetNodeId(hero)), npc));
            }

            nodes.Add(WithMenu(Node(npc, GreetAnyNodeId, key + ".greet_any"), npc));
            nodes.Add(WithMenu(Node(npc, MenuNodeId, key + ".menu"), npc));

            // Quest 1: offer (hero-specific accept line for npc.AcceptHero) → accepted / declined; progress line.
            DialogueNode offer1 = Node(npc, "q1_offer", key + ".q1_offer");
            offer1.Choices.Add(new DialogueChoice
            {
                TextKey = key + ".choice_accept_special",
                NextNodeId = "q1_accepted",
                Conditions = { HeroIs(npc.AcceptHero) },
                Actions = { StartQuest(q1) }
            });
            offer1.Choices.Add(new DialogueChoice
            {
                TextKey = ChoiceAcceptKey,
                NextNodeId = "q1_accepted",
                Conditions = { HeroIs(npc.AcceptHero, negate: true) },
                Actions = { StartQuest(q1) }
            });
            offer1.Choices.Add(new DialogueChoice { TextKey = ChoiceDeclineKey, NextNodeId = "declined" });
            nodes.Add(offer1);
            nodes.Add(BackToMenu(Node(npc, "q1_accepted", key + ".q1_accepted")));
            nodes.Add(BackToMenu(Node(npc, "q1_active", key + ".q1_active")));

            // Quest 2: offered once quest 1 is done (its prerequisite).
            DialogueNode offer2 = Node(npc, "q2_offer", key + ".q2_offer");
            offer2.Choices.Add(new DialogueChoice { TextKey = ChoiceAcceptKey, NextNodeId = "q2_accepted", Actions = { StartQuest(q2) } });
            offer2.Choices.Add(new DialogueChoice { TextKey = ChoiceDeclineKey, NextNodeId = "declined" });
            nodes.Add(offer2);
            nodes.Add(BackToMenu(Node(npc, "q2_accepted", key + ".q2_accepted")));
            nodes.Add(BackToMenu(Node(npc, "q2_active", key + ".q2_active")));

            nodes.Add(BackToMenu(Node(npc, "all_done", key + ".all_done")));
            nodes.Add(BackToMenu(Node(npc, "declined", key + ".declined")));

            // Pre-boss hint, special line for one hero.
            DialogueNode boss = BackToMenu(Node(npc, BossNodeId, key + ".boss"));
            boss.Redirects.Add(new DialogueRedirect { TargetNodeId = BossSpecialNodeId, Conditions = { HeroIs(npc.BossHintHero) } });
            nodes.Add(boss);
            nodes.Add(BackToMenu(Node(npc, BossSpecialNodeId, key + ".boss_special")));
            nodes.Add(BackToMenu(Node(npc, "cleared", key + ".cleared")));

            var dialogue = ScriptableObject.CreateInstance<DialogueDefinition>();
            dialogue.name = npc.DialogueId;
            dialogue.SetDataForTests(npc.DialogueId, HubNodeId, nodes);
            return dialogue;
        }

        /// <summary>The menu shared by every greeting and the <c>menu</c> node.</summary>
        private static DialogueNode WithMenu(DialogueNode node, RegionNpcSpec npc)
        {
            string key = npc.KeyBase;
            string q1 = npc.FirstQuestId;
            string q2 = npc.SecondQuestId;

            node.Choices.Add(new DialogueChoice { TextKey = key + ".choice_q1", NextNodeId = "q1_offer", Conditions = { QuestIs(q1, DialogueQuestState.NotStarted) } });
            node.Choices.Add(new DialogueChoice { TextKey = ChoiceProgressKey, NextNodeId = "q1_active", Conditions = { QuestIs(q1, DialogueQuestState.Active) } });
            node.Choices.Add(new DialogueChoice
            {
                TextKey = key + ".choice_q2",
                NextNodeId = "q2_offer",
                Conditions = { QuestIs(q1, DialogueQuestState.Completed), QuestIs(q2, DialogueQuestState.NotStarted) }
            });
            node.Choices.Add(new DialogueChoice { TextKey = ChoiceProgressKey, NextNodeId = "q2_active", Conditions = { QuestIs(q2, DialogueQuestState.Active) } });
            node.Choices.Add(new DialogueChoice { TextKey = ChoiceNewsKey, NextNodeId = "all_done", Conditions = { QuestIs(q2, DialogueQuestState.Completed) } });
            node.Choices.Add(new DialogueChoice { TextKey = ChoiceBossKey, NextNodeId = BossNodeId, Conditions = { FlagIs(BossReadyFlag) } });
            node.Choices.Add(new DialogueChoice { TextKey = ChoiceClearedKey, NextNodeId = "cleared", Conditions = { FlagIs(RegionClearedFlag) } });
            node.Choices.Add(new DialogueChoice { TextKey = ChoiceGoodbyeKey, NextNodeId = "" });
            return node;
        }

        private static DialogueNode Node(RegionNpcSpec npc, string id, string textKey)
        {
            return new DialogueNode { Id = id, TextKey = textKey ?? string.Empty, SpeakerId = npc.NpcId, SpeakerNameKey = npc.NameKey };
        }

        private static DialogueNode BackToMenu(DialogueNode node)
        {
            node.AutoAdvanceToNodeId = MenuNodeId;
            return node;
        }

        private static DialogueCondition HeroIs(CharacterId hero, bool negate = false)
        {
            return new DialogueCondition { Type = DialogueConditionType.ActiveCharacter, Key = hero.ToString(), Negate = negate };
        }

        private static DialogueCondition QuestIs(string questId, DialogueQuestState state)
        {
            return new DialogueCondition { Type = DialogueConditionType.QuestState, Key = questId, QuestState = state };
        }

        private static DialogueCondition FlagIs(string flag)
        {
            return new DialogueCondition { Type = DialogueConditionType.Flag, Key = flag };
        }

        private static DialogueAction StartQuest(string questId)
        {
            return new DialogueAction { Type = DialogueActionType.StartQuest, Key = questId };
        }
    }
}
