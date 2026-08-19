using Rapadura.Core.Events;

namespace Rapadura.Gameplay.Dialogue
{
    /// <summary>Raised when a dialogue starts, right after the entry node is resolved and shown.</summary>
    public readonly struct DialogueStartedEvent : IGameEvent
    {
        public readonly string DialogueId;
        public readonly string NodeId;

        public DialogueStartedEvent(string dialogueId, string nodeId)
        {
            DialogueId = dialogueId;
            NodeId = nodeId;
        }
    }

    /// <summary>Raised whenever the active dialogue moves to a new line/node (including the first one).</summary>
    public readonly struct DialogueLineChangedEvent : IGameEvent
    {
        public readonly string DialogueId;
        public readonly string NodeId;
        public readonly string SpeakerId;

        public DialogueLineChangedEvent(string dialogueId, string nodeId, string speakerId)
        {
            DialogueId = dialogueId;
            NodeId = nodeId;
            SpeakerId = speakerId;
        }
    }

    /// <summary>Raised when a dialogue ends, whether by reaching a terminal node or being force-closed.</summary>
    public readonly struct DialogueEndedEvent : IGameEvent
    {
        public readonly string DialogueId;

        public DialogueEndedEvent(string dialogueId)
        {
            DialogueId = dialogueId;
        }
    }
}
