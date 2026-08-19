using System;
using System.Collections.Generic;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>
    /// A single line of dialogue plus the choices that lead out of it. Follows the same
    /// "ScriptableObject-lite node graph" pattern used by most lightweight Unity dialogue
    /// systems (e.g. Yarn Spinner-style trees): a flat list of nodes identified by string id,
    /// each one pointing at the next node(s) by id rather than a direct object reference, so the
    /// whole graph is serializable as plain data inside one <see cref="DialogueDefinition"/> asset.
    ///
    /// Text is NEVER hardcoded here: <see cref="TextKey"/> is a localization key resolved through
    /// <see cref="Rapadura.Core.Localization.LocalizationManager.Get(string)"/> at display time,
    /// exactly like every other piece of UI text in this project.
    /// </summary>
    [Serializable]
    public class DialogueNode
    {
        /// <summary>Unique id of this node within its owning <see cref="DialogueDefinition"/>.</summary>
        public string Id;

        /// <summary>Localization key for the spoken line (resolved via LocalizationManager.Get).</summary>
        public string TextKey;

        /// <summary>Id of the speaker (e.g. an NPC id / localization key for the display name).</summary>
        public string SpeakerId;

        /// <summary>
        /// Localization key for the speaker's display name. Optional — if empty, the UI falls
        /// back to showing <see cref="SpeakerId"/> verbatim (useful while content is stubbed).
        /// </summary>
        public string SpeakerNameKey;

        /// <summary>
        /// Choices presented to the player after this line. Empty means the node has no branching
        /// and dialogue simply ends when it's reached (a terminal line), unless
        /// <see cref="AutoAdvanceToNodeId"/> is set.
        /// </summary>
        public List<DialogueChoice> Choices = new List<DialogueChoice>();

        /// <summary>
        /// Optional: when a node has no choices, this lets a linear line auto-advance to the next
        /// node id without requiring the player to click a choice button. Empty/null means the
        /// node is terminal and ends the dialogue.
        /// </summary>
        public string AutoAdvanceToNodeId;

        public bool IsTerminal => Choices.Count == 0 && string.IsNullOrEmpty(AutoAdvanceToNodeId);
    }

    /// <summary>
    /// One branch out of a <see cref="DialogueNode"/>: the text shown on the choice button and the
    /// id of the node it leads to. <see cref="ConditionFlag"/> is an optional, deliberately simple
    /// gate (a boolean flag by string key, checked against <see cref="DialogueManager.SetFlag"/>)
    /// so quest/relationship gating can be layered on later (Fase 8 — Quests) without this class
    /// needing to know about the quest system.
    /// </summary>
    [Serializable]
    public class DialogueChoice
    {
        /// <summary>Localization key for the choice button label.</summary>
        public string TextKey;

        /// <summary>Id of the <see cref="DialogueNode"/> this choice leads to. Empty ends the dialogue.</summary>
        public string NextNodeId;

        /// <summary>
        /// Optional condition: when non-empty, this choice is only offered if the named flag is
        /// currently set to true in the running <see cref="DialogueManager"/> (see
        /// <see cref="DialogueManager.SetFlag"/>/<see cref="DialogueManager.HasFlag"/>). Leave
        /// empty for an unconditional choice.
        /// </summary>
        public string ConditionFlag;

        /// <summary>When true, the condition is satisfied by the flag being ABSENT/false instead of present/true.</summary>
        public bool NegateCondition;
    }
}
