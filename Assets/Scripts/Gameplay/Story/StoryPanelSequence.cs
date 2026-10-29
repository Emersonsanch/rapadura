using System.Collections.Generic;
using Rapadura.Gameplay.Characters;

namespace Rapadura.Gameplay.Story
{
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
        }

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
                return false;
            }

            Index++;
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
