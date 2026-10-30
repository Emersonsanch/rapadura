using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Story
{
    /// <summary>What just happened to a <see cref="StoryPanelSequence"/> (page-turn / skip sounds, music).</summary>
    public enum StoryPanelChange
    {
        /// <summary>A beat with at least one panel started (first panel on screen).</summary>
        Opened = 0,
        /// <summary>Moved to the next panel.</summary>
        Advanced = 1,
        /// <summary>The last panel was read to the end.</summary>
        Finished = 2,
        /// <summary>"Pular" ended the beat early.</summary>
        Skipped = 3,
    }

    /// <summary>
    /// Raised by <see cref="StoryPanelSequence"/> on open / page advance / finish / skip. For audio
    /// (<c>SfxEventRouter</c>: page rustle, skip, the ending into the credits); nothing in gameplay depends on it.
    /// </summary>
    public readonly struct StoryPanelChangedEvent : IGameEvent
    {
        public readonly StoryBeat Beat;
        public readonly StoryPanelChange Change;
        public readonly int Index;
        public readonly int Count;

        public StoryPanelChangedEvent(StoryBeat beat, StoryPanelChange change, int index, int count)
        {
            Beat = beat;
            Change = change;
            Index = index;
            Count = count;
        }
    }

    /// <summary>
    /// Page-by-page state of one story beat on screen (plain C#, so the advance/skip rules are
    /// EditMode-tested without UI): the panel keys from <see cref="StoryScript"/>, the current page,
    /// and whether it ended. The presenter (<c>StoryPanelController</c>) only draws it.
    /// </summary>
    public class StoryPanelSequence
    {
        private readonly List<string> _panelKeys;

        public StoryBeat Beat { get; }
        public string TitleKey { get; }
        public IReadOnlyList<string> PanelKeys => _panelKeys;
        public int Index { get; private set; }
        public int Count => _panelKeys.Count;
        public bool IsFinished { get; private set; }

        /// <summary>True when the player skipped instead of reading to the end.</summary>
        public bool WasSkipped { get; private set; }

        public bool IsLastPanel => Index >= Count - 1;
        public string CurrentKey => Count > 0 && !IsFinished ? _panelKeys[Index] : null;

        public StoryPanelSequence(StoryBeat beat, CharacterId? hero)
        {
            Beat = beat;
            TitleKey = StoryScript.TitleKey(beat);
            _panelKeys = StoryScript.GetPanelKeys(beat, hero);
            IsFinished = _panelKeys.Count == 0;
            if (!IsFinished)
            {
                Publish(StoryPanelChange.Opened);
            }
        }

        private void Publish(StoryPanelChange change) => EventBus.Publish(new StoryPanelChangedEvent(Beat, change, Index, Count));

        /// <summary>Next page; on the last one, finishes the sequence. Returns true while there is still a page to show.</summary>
        public bool Advance()
        {
            if (IsFinished)
            {
                return false;
            }

            if (IsLastPanel)
            {
                IsFinished = true;
                Publish(StoryPanelChange.Finished);
                return false;
            }

            Index++;
            Publish(StoryPanelChange.Advanced);
            return true;
        }

        /// <summary>Ends the sequence right away. A skipped beat still counts as seen.</summary>
        public void Skip()
        {
            if (IsFinished)
            {
                return;
            }

            WasSkipped = true;
            IsFinished = true;
            Publish(StoryPanelChange.Skipped);
        }
    }

    /// <summary>When the world map may start a story beat (pure, see <c>StoryPanelController</c>).</summary>
    public static class StoryPresentationRules
    {
        /// <summary>
        /// A pending beat only starts on the world map with a hero in play (flow in World — never over
        /// the title/hero screens), with the map controller running and the token standing still, and
        /// while no other screen is freezing gameplay (pause menu, quest log, dialogue...).
        /// </summary>
        public static bool CanPresent(StoryBeat pending, bool worldInteractive, bool hasHero, bool mapReady, bool mapTraveling, bool pausedByOthers)
        {
            return pending != StoryBeat.None && worldInteractive && hasHero && mapReady && !mapTraveling && !pausedByOthers;
        }

        /// <summary>The ending is followed by the credits; every other beat goes straight back to the map.</summary>
        public static bool RollsCredits(StoryBeat beat) => beat == StoryBeat.Ending;
    }
}
