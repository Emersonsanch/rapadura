using Rapadura.Core.DI;
using Rapadura.Core.Logging;
using Rapadura.Core.Systems;
using Rapadura.Gameplay.Characters;
using Rapadura.Gameplay.Player;
using Rapadura.Gameplay.World.Map;
using UnityEngine;

namespace Rapadura.Gameplay.Flow
{
    /// <summary>
    /// The "Estado do jogo" lines of a crash report (<see cref="CrashReporter.RegisterContext"/>): flow state,
    /// hero, world map region, hero level and pause state. Lives in Gameplay so Core's reporter does not
    /// reference game types; registered before the first scene, read only when a report is built. Each
    /// line resolves its service at that moment (they come and go with the GameManager), "-" when absent.
    /// </summary>
    public static class CrashReportGameContext
    {
        public const string FlowStateLabel = "Estado do jogo";
        public const string HeroLabel = "Herói";
        public const string RegionLabel = "Região";
        public const string LevelLabel = "Nível do herói";
        public const string PauseLabel = "Pausado";

        private const string PlayerTag = "Player";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Register()
        {
            CrashReporter.RegisterContext(FlowStateLabel, () => ServiceLocator.TryGet(out GameFlowService flow) ? flow.State.ToString() : null);
            CrashReporter.RegisterContext(HeroLabel, () =>
                ServiceLocator.TryGet(out CharacterSelectionState selection) && selection.Selected.HasValue ? selection.Selected.Value.ToString() : null);
            CrashReporter.RegisterContext(RegionLabel, () => ServiceLocator.TryGet(out WorldMapProgress progress) ? progress.CurrentRegion.ToString() : null);
            CrashReporter.RegisterContext(LevelLabel, () =>
            {
                GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
                PlayerStats stats = player != null ? player.GetComponent<PlayerStats>() : null;
                return stats != null ? stats.Level.ToString() : null;
            });
            CrashReporter.RegisterContext(PauseLabel, () => GameplayPause.IsPaused ? "sim" : "não");
        }
    }
}
