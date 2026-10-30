using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Save;
using UnityEngine.SceneManagement;

namespace Rapadura.Gameplay.World.Map
{
    /// <summary>Abstraction over scene loading so <see cref="SceneTransitionService"/> is testable without real scenes.</summary>
    public interface ISceneLoader
    {
        void LoadScene(string sceneName);
    }

    /// <summary>Default <see cref="ISceneLoader"/>: single-mode async load through Unity's SceneManager.</summary>
    public class UnitySceneLoader : ISceneLoader
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// Owns the WorldMap ⇄ combat scene hop for random encounters. The world map calls
    /// <see cref="BeginEncounter"/> with the rolled <see cref="EncounterPayload"/>; the payload is
    /// kept here (GameManager-built, so it survives the single-mode scene load) while Main.unity
    /// loads, where <see cref="EncounterArenaController"/> reads <see cref="PendingEncounter"/>,
    /// spawns the enemies and eventually reports back via <see cref="CompleteEncounter"/>, which
    /// records the result in <see cref="WorldMapProgress"/>, auto-saves and returns to the map.
    ///
    /// Decision (see Docs/GDD.md "Mapa mundi e encontros"): the overworld is its own scene instead
    /// of a mode inside Main.unity, so the map can have its own camera/lighting/input without
    /// toggling half of the combat scene on and off.
    /// </summary>
    public class SceneTransitionService : IManager
    {
        private const string LogCategory = "SceneTransition";

        public const string DefaultWorldMapSceneName = "WorldMap";
        public const string DefaultCombatSceneName = "Main";

        private readonly WorldMapProgress _progress;
        private readonly ISceneLoader _sceneLoader;

        public string WorldMapSceneName { get; set; } = DefaultWorldMapSceneName;
        public string CombatSceneName { get; set; } = DefaultCombatSceneName;

        /// <summary>The encounter currently being fought (set between <see cref="BeginEncounter"/> and <see cref="CompleteEncounter"/>), else null.</summary>
        public EncounterPayload PendingEncounter { get; private set; }
        public bool HasPendingEncounter => PendingEncounter != null;

        /// <summary>Outcome of the most recently completed encounter (null until one completes) — lets the map show "Fugiu!"/"Vitória" on return.</summary>
        public EncounterOutcome? LastOutcome { get; private set; }

        /// <summary>
        /// Set by the death screen while it is active (same claim pattern as
        /// <c>CheckpointManager.DeferRespawn</c>): the player's death in an encounter is then resolved
        /// by that screen's "Continuar" (<see cref="CompleteEncounter"/> with Defeat) instead of
        /// <see cref="EncounterArenaController"/>'s own timed return — which used to count down
        /// underneath the paused death screen and race it.
        /// </summary>
        public bool DeferDefeatResolution { get; set; }

        /// <summary>
        /// The outcome the arena already decided for the pending encounter (see <see cref="DecideOutcome"/>),
        /// while it waits out its return delay; null when undecided.
        /// </summary>
        public EncounterOutcome? DecidedOutcome { get; private set; }

        /// <summary>
        /// The city being walked in (<c>CityCatalog</c> id) between <see cref="BeginCityVisit"/> and
        /// <see cref="EndCityVisit"/>: Main.unity then loads in "city mode" (the walkable town, Docs/Cities.md §5) instead
        /// of an arena. Null otherwise. Never saved: a save loaded later starts on the map.
        /// </summary>
        public string PendingCityVisit { get; private set; }
        public bool HasPendingCityVisit => !string.IsNullOrEmpty(PendingCityVisit);

        /// <summary>
        /// The pending city visit is a respawn (Docs/Progressao500.md §7): the hero fell outside an encounter and wakes up
        /// at the plaza of the city instead of walking in by the road. Cleared with the visit.
        /// </summary>
        public bool PendingCityRespawn { get; private set; }

        /// <summary>Set by <see cref="EndCityVisit"/> from the Cais da Barca: the map opens the fast-travel window once (consumed by the hub).</summary>
        public bool OpenFastTravelOnMapReturn { get; set; }

                public SceneTransitionService(WorldMapProgress progress, ISceneLoader sceneLoader = null)
        {
            _progress = progress;
            _sceneLoader = sceneLoader ?? new UnitySceneLoader();
        }

        public void Initialize()
        {
        }

        public void Shutdown()
        {
            PendingEncounter = null;
            DecidedOutcome = null;
        }

        /// <summary>
        /// Stores <paramref name="payload"/>, publishes <see cref="EncounterStartedEvent"/> and loads
        /// the combat scene. Returns false (no-op) for a null/empty payload or while another
        /// encounter is still pending.
        /// </summary>
        public bool BeginEncounter(EncounterPayload payload)
        {
            if (payload == null || payload.EnemyCount == 0)
            {
                GameLogger.Warning(LogCategory, "BeginEncounter called with an empty payload; ignoring.");
                return false;
            }

            if (HasPendingEncounter)
            {
                GameLogger.Warning(LogCategory, "BeginEncounter called while another encounter is still pending; ignoring.");
                return false;
            }

            PendingEncounter = payload;
            DecidedOutcome = null;
            EventBus.Publish(new EncounterStartedEvent(payload));
            _sceneLoader.LoadScene(CombatSceneName);
            return true;
        }

        /// <summary>
        /// Resolves the pending encounter: a victory is counted towards the biome's unlock rules (and a
        /// won boss challenge marks the region cleared),
        /// <see cref="EncounterEndedEvent"/> is published, the game auto-saves (if a SaveManager is
        /// registered) and the world map scene is loaded. Returns false if nothing was pending.
        /// </summary>
        public bool CompleteEncounter(EncounterOutcome outcome)
        {
            if (!HasPendingEncounter)
            {
                return false;
            }

            // A victory already decided by the arena sticks: the player can die during the arena's
            // return delay (last hit traded in the same frame as the kill), and the death screen —
            // which pauses gameplay and so freezes that delay — then completes the encounter as a
            // Defeat, which used to throw away the win (and, for a boss challenge, the region clear).
            if (DecidedOutcome == EncounterOutcome.Victory)
            {
                outcome = EncounterOutcome.Victory;
            }

            EncounterPayload payload = PendingEncounter;
            PendingEncounter = null;
            DecidedOutcome = null;
            LastOutcome = outcome;

            // Event rooms (Terreiro / Torre) and world events are not regional fights: no victory towards the region's unlock rules.
            if (outcome == EncounterOutcome.Victory && !payload.IsDirected)
            {
                _progress?.RecordVictory(payload.biome);

                // Winning the region boss challenge clears the region; the map's ApplyUnlocks then
                // opens the next one (see RegionBossProgress).
                if (payload.isBossChallenge)
                {
                    _progress?.MarkRegionCleared(payload.biome);
                }
            }

            EventBus.Publish(new EncounterEndedEvent(payload, outcome));

            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }

            _sceneLoader.LoadScene(WorldMapSceneName);
            return true;
        }

        /// <summary>
        /// Forgets the pending encounter and the last outcome without resolving them — no victory
        /// recorded, no <see cref="EncounterEndedEvent"/>, no auto-save. For leaving the session
        /// altogether (pause menu "Menu principal" → GameFlowService.ReturnToTitle), so the next
        /// session neither resumes the old arena nor shows its "Vitória"/"Fugiu!" on the map.
        /// </summary>
        /// <summary>
        /// Records the outcome the arena reached for the pending encounter before it actually returns
        /// to the map (<see cref="EncounterArenaController"/> waits a short delay first). The first
        /// decision wins; a decided <see cref="EncounterOutcome.Victory"/> overrides a later
        /// <see cref="CompleteEncounter"/> request. No-op without a pending encounter.
        /// </summary>
        public void DecideOutcome(EncounterOutcome outcome)
        {
            if (HasPendingEncounter && !DecidedOutcome.HasValue)
            {
                DecidedOutcome = outcome;
            }
        }

        public void AbandonPendingEncounter()
        {
            PendingCityVisit = null;
            PendingCityRespawn = false;
            OpenFastTravelOnMapReturn = false;
            PendingEncounter = null;
            DecidedOutcome = null;
            LastOutcome = null;
            DeferDefeatResolution = false;
        }

        /// <summary>
        /// "Entrar na cidade": loads Main.unity in city mode for <paramref name="cityId"/> (no enemies, no arena; the
        /// walkable town is built there by <c>CityModeController</c>). False while an encounter or another visit is pending.
        /// <paramref name="respawn"/>: the hero wakes up at the plaza (a defeat outside an encounter) instead of arriving by the road.
        /// </summary>
        public bool BeginCityVisit(string cityId, bool respawn = false)
        {
            if (string.IsNullOrEmpty(cityId) || HasPendingEncounter || HasPendingCityVisit)
            {
                GameLogger.Warning(LogCategory, $"BeginCityVisit('{cityId}') ignored (encounter or visit pending, or no city).");
                return false;
            }

            PendingCityVisit = cityId;
            PendingCityRespawn = respawn;
            OpenFastTravelOnMapReturn = false;
            EventBus.Publish(new Rapadura.Gameplay.World.Cities.CityVisitStartedEvent(cityId));
            _sceneLoader.LoadScene(CombatSceneName);
            return true;
        }

        /// <summary>
        /// Leaves the walkable city (a gate, or the Cais da Barca with <paramref name="openFastTravel"/>): auto-saves and
        /// loads the world map, where the token is still on the city's node. False when no visit is pending.
        /// </summary>
        public bool EndCityVisit(bool openFastTravel = false)
        {
            if (!HasPendingCityVisit)
            {
                return false;
            }

            string cityId = PendingCityVisit;
            PendingCityVisit = null;
            PendingCityRespawn = false;
            OpenFastTravelOnMapReturn = openFastTravel;
            EventBus.Publish(new Rapadura.Gameplay.World.Cities.CityVisitEndedEvent(cityId, openFastTravel));
            if (ServiceLocator.TryGet(out SaveManager saveManager))
            {
                saveManager.AutoSave();
            }

            _sceneLoader.LoadScene(WorldMapSceneName);
            return true;
        }

                /// <summary>Loads the world map without any encounter bookkeeping (e.g. from a "Mapa" menu button in Main.unity).</summary>
        public void ReturnToWorldMap()
        {
            _sceneLoader.LoadScene(WorldMapSceneName);
        }
    }
}
