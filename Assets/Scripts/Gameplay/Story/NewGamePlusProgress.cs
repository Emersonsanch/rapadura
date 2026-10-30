using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Enemies;
using Rapadura.Gameplay.Quests;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Story
{
    /// <summary>JSON-serializable snapshot of <see cref="NewGamePlusProgress"/>.</summary>
    [Serializable]
    public class NewGamePlusSaveState
    {
        /// <summary>Current Nova Jornada+ cycle (0 = first campaign).</summary>
        public int cycle;

        /// <summary>Highest Nova Jornada+ cycle whose campaign was finished in this save (0 = none).</summary>
        public int highestCompletedCycle;
    }

    /// <summary>Raised when a Nova Jornada+ begins (after the campaign state was reset). <see cref="Cycle"/> is the new cycle (1+).</summary>
    public readonly struct NewGamePlusStartedEvent : IGameEvent
    {
        public readonly int Cycle;

        public NewGamePlusStartedEvent(int cycle)
        {
            Cycle = cycle;
        }
    }

    /// <summary>Raised when the campaign of a Nova Jornada+ cycle (1+) is completed — the "Jornada+" achievement listens to it.</summary>
    public readonly struct NewGamePlusCycleCompletedEvent : IGameEvent
    {
        public readonly int Cycle;

        public NewGamePlusCycleCompletedEvent(int cycle)
        {
            Cycle = cycle;
        }
    }

    /// <summary>
    /// "Nova Jornada+" (New Game+): once the campaign is complete (<see cref="NewGamePlusRules.CanOffer"/>),
    /// Vó Zefa offers to start it over (<c>NewGamePlusHud</c> on the world map). <see cref="BeginNextCycle"/>
    /// keeps everything the hero carries — level, attributes, skills, equipment, backpack and gold (the
    /// Player's carried save state, which nothing here touches) and the Baú da Vó Zefa — and resets the
    /// campaign: <see cref="StoryProgress"/> (beats, campaign flag), <see cref="WorldMapProgress"/> (back to the
    /// Floresta only), <see cref="DefeatedEnemyRegistry"/> (bosses alive again) and the region side quests of
    /// <see cref="RegionQuestCatalog"/>. Personal, convergence and daily quests, buildings, recipes, the clock
    /// and the weather are kept. Enemies then scale by <see cref="Cycle"/> (<see cref="NewGamePlusRules"/>).
    ///
    /// Plain C# built by GameManager, saved under <see cref="Key"/> and reset on "Novo jogo"/before a load like
    /// every per-save system; a save written before this system has no entry and loads as cycle 0.
    /// </summary>
    public class NewGamePlusProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "NewGamePlus";

        private const string LogCategory = "NewGamePlus";

        private readonly StoryProgress _story;
        private readonly WorldMapProgress _worldMap;
        private readonly DefeatedEnemyRegistry _defeatedEnemies;
        private readonly QuestManager _quests;
        private bool _subscribed;

        public string SaveKey => Key;

        /// <summary>Current cycle: 0 on the first campaign, 1 for "Nova Jornada+ 1", ... up to <see cref="NewGamePlusRules.MaxCycle"/>.</summary>
        public int Cycle { get; private set; }

        /// <summary>Highest cycle (1+) whose campaign was finished in this save; 0 when none.</summary>
        public int HighestCompletedCycle { get; private set; }

        /// <summary>Any of the dependencies may be null (tests): that part of the campaign is simply not reset.</summary>
        public NewGamePlusProgress(StoryProgress story, WorldMapProgress worldMap = null, DefeatedEnemyRegistry defeatedEnemies = null, QuestManager quests = null)
        {
            _story = story;
            _worldMap = worldMap;
            _defeatedEnemies = defeatedEnemies;
            _quests = quests;
        }

        /// <summary>The live save's cycle (GameManager's instance); 0 outside a game session (tests, a scene played on its own).</summary>
        public static int ResolveCurrentCycle()
        {
            return ServiceLocator.TryGet(out NewGamePlusProgress progress) && progress != null ? progress.Cycle : 0;
        }

        public void Initialize()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<CampaignCompletedEvent>(OnCampaignCompleted);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<CampaignCompletedEvent>(OnCampaignCompleted);
        }

        /// <summary>Back to the first campaign ("Novo jogo" / right before a save is applied).</summary>
        public void ResetForNewSession()
        {
            Cycle = 0;
            HighestCompletedCycle = 0;
        }

        /// <summary>Whether "Nova Jornada+" can start right now (campaign complete and its ending shown).</summary>
        public bool CanBeginNextCycle => _story != null && NewGamePlusRules.CanOffer(_story.CampaignComplete, _story.HasSeen(StoryBeat.Ending));

        /// <summary>The cycle <see cref="BeginNextCycle"/> would start.</summary>
        public int NextCycle => NewGamePlusRules.NextCycle(Cycle);

        /// <summary>
        /// Starts the next journey: resets the campaign (see the class summary), advances <see cref="Cycle"/>
        /// and raises <see cref="NewGamePlusStartedEvent"/>. False (nothing changes) unless
        /// <see cref="CanBeginNextCycle"/>. Saving and reloading the world map are the caller's job.
        /// </summary>
        public bool BeginNextCycle()
        {
            if (!CanBeginNextCycle)
            {
                return false;
            }

            _story.BeginNewJourney();
            _worldMap?.ResetToNewGame();
            _defeatedEnemies?.ResetForNewSession();
            _quests?.ForgetQuests(RegionQuestIds());

            Cycle = NextCycle;
            GameLogger.Info(LogCategory, $"Nova Jornada+ {Cycle} started.");
            EventBus.Publish(new NewGamePlusStartedEvent(Cycle));
            return true;
        }

        private static IEnumerable<string> RegionQuestIds()
        {
            foreach (RegionQuestSpec spec in RegionQuestCatalog.Quests)
            {
                yield return spec.QuestId;
            }
        }

        private void OnCampaignCompleted(CampaignCompletedEvent evt)
        {
            if (Cycle <= 0)
            {
                return;
            }

            HighestCompletedCycle = Math.Max(HighestCompletedCycle, Cycle);
            EventBus.Publish(new NewGamePlusCycleCompletedEvent(Cycle));
        }

        public object CaptureState()
        {
            return new NewGamePlusSaveState { cycle = Cycle, highestCompletedCycle = HighestCompletedCycle };
        }

        public void RestoreState(object state)
        {
            if (!(state is NewGamePlusSaveState saved))
            {
                return;
            }

            Cycle = NewGamePlusRules.ClampCycle(saved.cycle);
            HighestCompletedCycle = NewGamePlusRules.ClampCycle(saved.highestCompletedCycle);
        }
    }
}
