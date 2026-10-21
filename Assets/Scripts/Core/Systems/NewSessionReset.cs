using System;
using System.Collections.Generic;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;

namespace Rapadura.Core.Systems
{
    /// <summary>Raised by <see cref="NewSessionReset.ResetAll"/> after every registered system went back to new-game state.</summary>
    public readonly struct NewSessionResetEvent : IGameEvent
    {
    }

    /// <summary>
    /// Puts every registered <see cref="IResettable"/> back to new-game state, in registration order
    /// (so a dependent — e.g. DailyQuestService restarting today's dailies — runs after what it depends
    /// on — QuestManager). Built by GameManager, which registers every per-save manager; called by
    /// GameFlowService on "Novo jogo" and by SaveManager right before a save file is applied.
    ///
    /// Deliberately not reset (see GameManager.BuildManagers):
    /// <list type="bullet">
    /// <item>Achievements — per-profile lifetime progress, not per-save (loading or starting a game never takes unlocks away).</item>
    /// <item>Settings — language, audio, accessibility, frame rate are per-device.</item>
    /// <item>Analytics — telemetry buffer, not game state.</item>
    /// <item>CheckpointManager / SceneTransitionService — scene-scoped; rebuilt by the next scene load
    /// (GameFlowService.ReturnToTitle abandons a pending encounter explicitly).</item>
    /// </list>
    /// </summary>
    public class NewSessionReset
    {
        private const string LogCategory = "Session";

        private readonly List<IResettable> _resettables = new List<IResettable>();
        private bool _resetting;

        public IReadOnlyList<IResettable> Resettables => _resettables;

        public void Register(IResettable resettable)
        {
            if (resettable != null && !_resettables.Contains(resettable))
            {
                _resettables.Add(resettable);
            }
        }

        public void Unregister(IResettable resettable)
        {
            _resettables.Remove(resettable);
        }

        /// <summary>Resets every registered system (re-entrant calls are ignored). One failing system never blocks the others.</summary>
        public void ResetAll()
        {
            if (_resetting)
            {
                return;
            }

            _resetting = true;

            try
            {
                foreach (IResettable resettable in _resettables.ToArray())
                {
                    try
                    {
                        resettable.ResetForNewSession();
                    }
                    catch (Exception e)
                    {
                        GameLogger.Error(LogCategory, $"ResetForNewSession failed for {resettable.GetType().Name}: {e.Message}");
                    }
                }
            }
            finally
            {
                _resetting = false;
            }

            GameLogger.Info(LogCategory, $"New session: reset {_resettables.Count} systems.");
            EventBus.Publish(new NewSessionResetEvent());
        }
    }
}
