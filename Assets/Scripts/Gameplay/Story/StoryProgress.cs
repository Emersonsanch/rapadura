using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Gameplay.World.Biomes;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Story
{
    /// <summary>JSON-serializable snapshot of <see cref="StoryProgress"/>. Beats are stored by <see cref="StoryBeats.Id"/>.</summary>
    [Serializable]
    public class StorySaveState
    {
        /// <summary>The Cavernas boss was beaten in this save (post-game from then on).</summary>
        public bool campaignComplete;

        /// <summary>Beats already shown (or skipped): <c>prologue</c>, <c>interlude_forest</c>, ..., <c>ending</c>.</summary>
        public List<string> seenBeats = new List<string>();
    }

    /// <summary>Raised once per save, the first time the campaign is completed (final region boss beaten).</summary>
    public readonly struct CampaignCompletedEvent : IGameEvent
    {
    }

    /// <summary>Raised when a story beat is marked as seen (shown to the end or skipped).</summary>
    public readonly struct StoryBeatSeenEvent : IGameEvent
    {
        public readonly StoryBeat Beat;

        public StoryBeatSeenEvent(StoryBeat beat)
        {
            Beat = beat;
        }
    }

    /// <summary>
    /// Campaign framing state: which story beats were seen and whether the campaign is complete.
    /// Plain C# (EditMode-testable), built by GameManager next to <see cref="WorldMapProgress"/>,
    /// saved under <see cref="SaveKey"/> and reset on "Novo jogo"/before a load like every per-save system.
    ///
    /// When a beat is due is derived, not scheduled: <see cref="GetPendingBeat"/> looks at the beats
    /// seen so far and the regions <see cref="WorldMapProgress"/> marks cleared, so a beat survives a
    /// crash/quit between the boss kill (in Main.unity) and the world map (where
    /// <c>StoryPanelController</c> shows it). Order: prologue (new game) → one interlude per cleared
    /// region on the way (Floresta, Deserto, Montanha) → ending (Cavernas cleared).
    ///
    /// The campaign completes when the final region's boss is beaten
    /// (<see cref="WorldMapRegionClearedEvent"/> for <see cref="StoryBeats.FinalRegion"/>), raising
    /// <see cref="CampaignCompletedEvent"/> once (the "Fim da jornada" achievement listens to it).
    ///
    /// Old saves (written before this system) have no "Story" entry, so <see cref="RestoreState"/> is
    /// never called for them: on the next <see cref="GameLoadedEvent"/> the beats that are already
    /// behind the player (prologue + interludes of regions cleared back then) are marked seen without
    /// being shown, and a save whose Cavernas were already cleared becomes complete — its ending is
    /// still shown once, since it never existed before.
    /// </summary>
    public class StoryProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Story";

        private readonly HashSet<StoryBeat> _seen = new HashSet<StoryBeat>();
        private WorldMapProgress _worldMap;
        private bool _subscribed;

        /// <summary>
        /// True from a reset (new game / right before a save is applied) until either the save's own
        /// "Story" entry is restored or a beat is seen in play. A <see cref="GameLoadedEvent"/> while
        /// still true means the loaded save predates this system (see <see cref="ReconcileLegacySave"/>).
        /// </summary>
        private bool _awaitingLoadCheck;

        public string SaveKey => Key;

        public bool CampaignComplete { get; private set; }

        public IReadOnlyCollection<StoryBeat> SeenBeats => _seen;

        /// <param name="worldMap">Source of the cleared regions (may be set later through <see cref="SetWorldMap"/>).</param>
        public StoryProgress(WorldMapProgress worldMap = null)
        {
            _worldMap = worldMap;
        }

        public void SetWorldMap(WorldMapProgress worldMap) => _worldMap = worldMap;

        public void Initialize()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<WorldMapRegionClearedEvent>(OnRegionCleared);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        /// <summary>Back to a brand-new game: nothing seen, campaign not complete.</summary>
        public void ResetForNewSession()
        {
            _seen.Clear();
            CampaignComplete = false;
            _awaitingLoadCheck = true;
        }

        public bool HasSeen(StoryBeat beat) => _seen.Contains(beat);

        /// <summary>
        /// The next beat to show, or <see cref="StoryBeat.None"/>: the prologue first, then the interlude
        /// of each cleared region not seen yet (in map order), then the ending once the campaign is complete.
        /// </summary>
        public StoryBeat GetPendingBeat()
        {
            foreach (StoryBeat beat in StoryBeats.InOrder)
            {
                if (!_seen.Contains(beat) && IsUnlocked(beat))
                {
                    return beat;
                }
            }

            return StoryBeat.None;
        }

        /// <summary>Whether <paramref name="beat"/>'s trigger already happened (seen or not).</summary>
        public bool IsUnlocked(StoryBeat beat)
        {
            if (beat == StoryBeat.Prologue)
            {
                return true;
            }

            if (beat == StoryBeat.Ending)
            {
                return CampaignComplete || IsCleared(StoryBeats.FinalRegion);
            }

            BiomeType? region = StoryBeats.RegionFor(beat);
            return region.HasValue && IsCleared(region.Value);
        }

        /// <summary>Marks a beat as seen (shown to the end or skipped). Returns true only the first time.</summary>
        public bool MarkSeen(StoryBeat beat)
        {
            if (beat == StoryBeat.None)
            {
                return false;
            }

            // A beat seen in play means this session is live, not a just-loaded legacy save.
            _awaitingLoadCheck = false;

            if (!_seen.Add(beat))
            {
                return false;
            }

            if (beat == StoryBeat.Ending)
            {
                MarkCampaignComplete();
            }

            EventBus.Publish(new StoryBeatSeenEvent(beat));
            return true;
        }

        /// <summary>Flags the campaign as complete. Returns true (and raises <see cref="CampaignCompletedEvent"/>) only the first time per save.</summary>
        public bool MarkCampaignComplete()
        {
            if (CampaignComplete)
            {
                return false;
            }

            CampaignComplete = true;
            EventBus.Publish(new CampaignCompletedEvent());
            return true;
        }

        /// <summary>
        /// Legacy save (no "Story" entry): marks as seen every beat already behind the player — the
        /// prologue, and the interludes of regions cleared back then — and completes the campaign if the
        /// final region was cleared. The ending itself stays pending so it plays once.
        /// </summary>
        public void ReconcileLegacySave()
        {
            _awaitingLoadCheck = false;
            _seen.Add(StoryBeat.Prologue);

            foreach (StoryBeat beat in StoryBeats.InOrder)
            {
                if (beat != StoryBeat.Prologue && beat != StoryBeat.Ending && IsUnlocked(beat))
                {
                    _seen.Add(beat);
                }
            }

            if (IsCleared(StoryBeats.FinalRegion))
            {
                MarkCampaignComplete();
            }
        }

        private bool IsCleared(BiomeType region) => _worldMap != null && _worldMap.IsRegionCleared(region);

        private void OnRegionCleared(WorldMapRegionClearedEvent evt)
        {
            if (evt.Region == StoryBeats.FinalRegion)
            {
                MarkCampaignComplete();
            }
        }

        private void OnGameLoaded(GameLoadedEvent evt)
        {
            if (_awaitingLoadCheck)
            {
                ReconcileLegacySave();
            }
        }

        public object CaptureState()
        {
            var state = new StorySaveState { campaignComplete = CampaignComplete };

            foreach (StoryBeat beat in StoryBeats.InOrder)
            {
                if (_seen.Contains(beat))
                {
                    state.seenBeats.Add(StoryBeats.Id(beat));
                }
            }

            return state;
        }

        public void RestoreState(object state)
        {
            if (!(state is StorySaveState saved))
            {
                return;
            }

            _awaitingLoadCheck = false;
            _seen.Clear();
            CampaignComplete = saved.campaignComplete;

            if (saved.seenBeats != null)
            {
                foreach (string id in saved.seenBeats)
                {
                    StoryBeat beat = StoryBeats.FromId(id);
                    if (beat != StoryBeat.None)
                    {
                        _seen.Add(beat);
                    }
                }
            }
        }
    }
}
