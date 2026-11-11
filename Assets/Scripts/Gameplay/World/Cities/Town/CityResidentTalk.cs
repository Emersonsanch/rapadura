using System.Collections.Generic;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Dialogue;
using Rapadura.Gameplay.Quests;
using Rapadura.UI.WorldMap;

namespace Rapadura.Gameplay.World.Cities.Town
{
    /// <summary>What a resident does when the cousins step up to them.</summary>
    public enum CityResidentAction
    {
        /// <summary>Nobody to answer (a counter with no keeper, and it cannot open).</summary>
        Nothing = 0,
        /// <summary>A resident without a counter: their line floats over them.</summary>
        Say = 1,
        /// <summary>A keeper with something to say: the dialogue box, with the shop as a choice after the line.</summary>
        Converse = 2,
        /// <summary>A keeper with nothing pending: straight to the counter.</summary>
        OpenShop = 3,
    }

    /// <summary>The outcome of <see cref="CityResidentTalk.Plan"/>.</summary>
    public readonly struct CityResidentPlan
    {
        public readonly CityResidentAction Action;

        /// <summary>The localization key of the line to say, or null when nothing is said.</summary>
        public readonly string LineKey;

        /// <summary>True when the line is the keeper's greeting for the cousin (the first visit), not a quest line.</summary>
        public readonly bool IsGreeting;

        public CityResidentPlan(CityResidentAction action, string lineKey = null, bool isGreeting = false)
        {
            Action = action;
            LineKey = lineKey;
            IsGreeting = isGreeting;
        }
    }

    /// <summary>
    /// The rules of a resident's post (pure, EditMode-tested, server-runnable): whether the cousins hear a line or go straight to the counter, and the short conversation that
    /// keeps the counter one choice away. Scene side: <see cref="CityResidentPost"/>.
    /// </summary>
    public static class CityResidentTalk
    {
        /// <summary>Raised by the "see the shop" choice; read and cleared when the conversation ends.</summary>
        public const string OpenShopFlag = "city_resident.open_shop";

        public const string LineNodeId = "line";

        public static string DialogueId(string residentId) => "city_resident." + residentId;

        /// <summary>The <see cref="Rapadura.Gameplay.World.Map.WorldMapProgress"/> mark of "this cousin already heard this keeper's greeting" (saved per slot).</summary>
        public static string GreetedMark(string residentId, CharacterId? hero) =>
            "city.greeted." + residentId + "." + (hero.HasValue ? hero.Value.ToString().ToLowerInvariant() : "any");

        /// <summary>
        /// The line this resident has about the story or their quests, or null when nothing is pending. Starts what is offered. The story's offer and progress come first, then
        /// the city quests; the story's lines that hand out nothing ("too low", the closing line) come last, so they can never hide a city quest for good.
        /// </summary>
        public static string QuestLine(QuestManager quests, string residentId, CharacterId? hero, int heroLevel)
        {
            if (quests == null || string.IsNullOrEmpty(residentId))
            {
                return null;
            }

            LateStoryTalk story = hero.HasValue ? LateStoryCatalog.Decide(residentId, quests.IsActive, quests.IsCompleted, heroLevel, out _) : LateStoryTalk.None;
            if (story == LateStoryTalk.Offer || story == LateStoryTalk.Active)
            {
                string storyLine = LateStoryCatalog.Talk(quests, residentId, hero.Value, heroLevel);
                if (storyLine != null)
                {
                    return storyLine;
                }
            }

            string cityLine = CityQuestCatalog.Talk(quests, residentId, heroLevel);
            if (cityLine != null)
            {
                return cityLine;
            }

            return story == LateStoryTalk.TooLow || story == LateStoryTalk.Finale ? LateStoryCatalog.Talk(quests, residentId, hero.Value, heroLevel) : null;
        }

        /// <summary>What the post does: a keeper speaks first when there is a quest line or this cousin's first visit, else opens the counter; a resident without one just says their line.</summary>
        public static CityResidentPlan Plan(bool hasCounter, string questLineKey, string greetingKey, bool alreadyGreeted)
        {
            if (!hasCounter)
            {
                string line = questLineKey ?? greetingKey;
                return line != null ? new CityResidentPlan(CityResidentAction.Say, line) : new CityResidentPlan(CityResidentAction.Nothing);
            }

            if (questLineKey != null)
            {
                return new CityResidentPlan(CityResidentAction.Converse, questLineKey);
            }

            return greetingKey != null && !alreadyGreeted
                ? new CityResidentPlan(CityResidentAction.Converse, greetingKey, true)
                : new CityResidentPlan(CityResidentAction.OpenShop);
        }

        /// <summary>One line and two choices: the counter (raises <see cref="OpenShopFlag"/>) and goodbye. Always has a choice, so the box can always be closed.</summary>
        public static List<DialogueNode> BuildNodes(string residentId, string speakerNameKey, string lineKey)
        {
            var node = new DialogueNode { Id = LineNodeId, TextKey = lineKey, SpeakerId = residentId, SpeakerNameKey = speakerNameKey };
            node.Choices.Add(new DialogueChoice
            {
                TextKey = CityTexts.TalkOpenShop,
                NextNodeId = string.Empty,
                Actions = { new DialogueAction { Type = DialogueActionType.SetFlag, Key = OpenShopFlag } },
            });
            node.Choices.Add(new DialogueChoice { TextKey = CityTexts.TalkGoodbye, NextNodeId = string.Empty });
            return new List<DialogueNode> { node };
        }
    }
}
