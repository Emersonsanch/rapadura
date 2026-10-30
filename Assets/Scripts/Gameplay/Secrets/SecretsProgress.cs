using System;
using System.Collections.Generic;
using Rapadura.Core.DI;
using Rapadura.Core.Events;
using Rapadura.Core.Interfaces;
using Rapadura.Core.Logging;
using Rapadura.Gameplay.Achievements;
using Rapadura.Gameplay.World.Map;

namespace Rapadura.Gameplay.Secrets
{
    /// <summary>JSON-serializable snapshot of <see cref="SecretsProgress"/> (absent in saves written before secrets = all empty).</summary>
    [Serializable]
    public class SecretsSaveState
    {
        public List<string> revealedSecrets = new List<string>();
        public List<string> claimedSecrets = new List<string>();

        /// <summary>Relics owned, in the order they were found.</summary>
        public List<string> relics = new List<string>();

        /// <summary>Bonus gold won on the map, waiting for the next arena (the bag lives on the Player, see <see cref="RelicPassiveApplier"/>).</summary>
        public int pendingGold;

        /// <summary>Arena secrets broken (any reward) — lifetime of this save, for the panel.</summary>
        public int arenaSecretsBroken;
    }

    /// <summary>Raised the first time a world-map secret reveals itself.</summary>
    public readonly struct SecretRevealedEvent : IGameEvent
    {
        public readonly string SecretId;

        public SecretRevealedEvent(string secretId)
        {
            SecretId = secretId;
        }
    }

    /// <summary>Raised when a world-map secret was investigated successfully (relic granted, gold queued).</summary>
    public readonly struct SecretClaimedEvent : IGameEvent
    {
        public readonly string SecretId;
        public readonly int Gold;

        public SecretClaimedEvent(string secretId, int gold)
        {
            SecretId = secretId;
            Gold = gold;
        }
    }

    /// <summary>Raised when a relic joins the collection (map secret or arena secret).</summary>
    public readonly struct RelicCollectedEvent : IGameEvent
    {
        public readonly string RelicId;
        public readonly int OwnedCount;

        public RelicCollectedEvent(string relicId, int ownedCount)
        {
            RelicId = relicId;
            OwnedCount = ownedCount;
        }
    }

    /// <summary>Raised when an arena secret breaks; <see cref="RelicId"/> is null when it only held gold.</summary>
    public readonly struct ArenaSecretBrokenEvent : IGameEvent
    {
        public readonly string RelicId;
        public readonly int Gold;

        public ArenaSecretBrokenEvent(string relicId, int gold)
        {
            RelicId = relicId;
            Gold = gold;
        }
    }

    /// <summary>
    /// "Segredos e áreas ocultas" — per save slot: which world-map secrets were revealed / investigated, which
    /// "Relíquias do Sertão" are owned, and the bonus gold still waiting for the Player's bag.
    /// <list type="bullet">
    /// <item>A GameManager-built manager registered with <c>SaveManager</c> under <see cref="Key"/> and with
    /// <c>NewSessionReset</c>: "Novo jogo" and every load start empty, and a save written before secrets existed
    /// (no entry) loads empty.</item>
    /// <item>Secret encounters: <see cref="BeginSecretEncounter"/> remembers which secret a payload belongs to;
    /// the matching <see cref="EncounterEndedEvent"/> with a Victory claims it (published by
    /// SceneTransitionService right before its auto-save, so the claim is saved with the win).</item>
    /// <item>Every new relic is fed to <see cref="AchievementManager"/> (<see cref="AchievementConditionType.RelicsCollected"/>,
    /// distinct ids) — achievements are per profile, so relics already owned are re-fed after each load.</item>
    /// </list>
    /// </summary>
    public class SecretsProgress : IManager, ISaveable, IResettable
    {
        public const string Key = "Secrets";
        private const string LogCategory = "Secrets";

        private readonly HashSet<string> _revealed = new HashSet<string>();
        private readonly HashSet<string> _claimed = new HashSet<string>();
        private readonly List<string> _relics = new List<string>();
        private EncounterPayload _pendingEncounter;
        private string _pendingEncounterSecret;
        private bool _subscribed;

        public string SaveKey => Key;

        public IReadOnlyList<string> OwnedRelics => _relics;
        public int OwnedRelicCount => _relics.Count;
        public bool HasAllRelics => _relics.Count >= RelicCatalog.Count;
        public int PendingGold { get; private set; }
        public int ArenaSecretsBroken { get; private set; }

        /// <summary>Secret whose guardian fight is under way (null when none).</summary>
        public string PendingEncounterSecret => _pendingEncounterSecret;

        /// <summary>Achievement sink; null = the ServiceLocator's <see cref="AchievementManager"/> (tests inject their own).</summary>
        public AchievementManager Achievements { get; set; }

        private static SecretsProgress _standalone;

        /// <summary>The GameManager's instance (ServiceLocator); without one (a scene played on its own) a process-wide, unsaved one.</summary>
        public static SecretsProgress ResolveShared()
        {
            if (ServiceLocator.TryGet(out SecretsProgress secrets))
            {
                return secrets;
            }

            return _standalone ??= new SecretsProgress();
        }

        public void Initialize()
        {
            if (_subscribed)
            {
                return;
            }

            _subscribed = true;
            EventBus.Subscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Subscribe<GameLoadedEvent>(OnGameLoaded);
        }

        public void Shutdown()
        {
            if (!_subscribed)
            {
                return;
            }

            _subscribed = false;
            EventBus.Unsubscribe<EncounterEndedEvent>(OnEncounterEnded);
            EventBus.Unsubscribe<GameLoadedEvent>(OnGameLoaded);
        }

        // ------------------------------------------------------------------
        // Queries
        // ------------------------------------------------------------------

        public bool IsRevealed(string secretId) => !string.IsNullOrEmpty(secretId) && _revealed.Contains(secretId);

        public bool IsClaimed(string secretId) => !string.IsNullOrEmpty(secretId) && _claimed.Contains(secretId);

        public bool HasRelic(string relicId) => !string.IsNullOrEmpty(relicId) && _relics.Contains(relicId);

        /// <summary>Revealed and not yet investigated: "Investigar" is offered.</summary>
        public bool IsInvestigable(string secretId) => IsRevealed(secretId) && !IsClaimed(secretId);

        // ------------------------------------------------------------------
        // Map secrets
        // ------------------------------------------------------------------

        /// <summary>Marks a catalog secret as revealed. True (and <see cref="SecretRevealedEvent"/>) only the first time.</summary>
        public bool Reveal(string secretId)
        {
            if (WorldSecretCatalog.Get(secretId) == null || !_revealed.Add(secretId))
            {
                return false;
            }

            EventBus.Publish(new SecretRevealedEvent(secretId));
            return true;
        }

        /// <summary>
        /// Investigates a revealed secret: grants its relic, queues its bonus gold and publishes
        /// <see cref="SecretClaimedEvent"/>. False when unknown, not revealed or already claimed.
        /// </summary>
        public bool Claim(string secretId)
        {
            WorldSecretDefinition secret = WorldSecretCatalog.Get(secretId);
            if (secret == null || !IsInvestigable(secretId))
            {
                return false;
            }

            _claimed.Add(secretId);
            PendingGold += Math.Max(0, secret.BonusGold);
            AddRelic(secret.RelicId);
            EventBus.Publish(new SecretClaimedEvent(secretId, secret.BonusGold));
            GameLogger.Info(LogCategory, $"Secret {secretId} investigated: {secret.RelicId}, +{secret.BonusGold} gold pending.");
            return true;
        }

        /// <summary>
        /// Remembers that <paramref name="payload"/> is <paramref name="secretId"/>'s guardian fight, so its victory
        /// claims the secret. False (nothing remembered) for an unknown / non-investigable secret or a null payload.
        /// </summary>
        public bool BeginSecretEncounter(string secretId, EncounterPayload payload)
        {
            if (payload == null || !IsInvestigable(secretId))
            {
                return false;
            }

            _pendingEncounter = payload;
            _pendingEncounterSecret = secretId;
            return true;
        }

        /// <summary>Forgets a guardian fight that never started (the scene transition refused it).</summary>
        public void CancelSecretEncounter()
        {
            _pendingEncounter = null;
            _pendingEncounterSecret = null;
        }

        private void OnEncounterEnded(EncounterEndedEvent evt)
        {
            if (_pendingEncounter == null || !ReferenceEquals(evt.Payload, _pendingEncounter))
            {
                return;
            }

            string secretId = _pendingEncounterSecret;
            CancelSecretEncounter();
            if (evt.Outcome == EncounterOutcome.Victory)
            {
                Claim(secretId);
            }
        }

        // ------------------------------------------------------------------
        // Relics / gold
        // ------------------------------------------------------------------

        /// <summary>Adds a catalog relic to the collection. True (and <see cref="RelicCollectedEvent"/>) only the first time.</summary>
        public bool AddRelic(string relicId)
        {
            if (RelicCatalog.Get(relicId) == null || _relics.Contains(relicId))
            {
                return false;
            }

            _relics.Add(relicId);
            RecordAchievement(relicId);
            EventBus.Publish(new RelicCollectedEvent(relicId, _relics.Count));
            return true;
        }

        /// <summary>
        /// An arena secret broke: its biome's relic when still missing, else <see cref="SecretRules.ArenaGoldFor"/>
        /// gold (returned through <paramref name="gold"/>, for the caller to put in the bag). Returns the relic id or null.
        /// </summary>
        public string BreakArenaSecret(World.Biomes.BiomeType biome, out int gold)
        {
            ArenaSecretsBroken++;
            RelicDefinition relic = RelicCatalog.ArenaRelicFor(biome);
            string relicId = relic != null && AddRelic(relic.Id) ? relic.Id : null;
            gold = relicId == null ? SecretRules.ArenaGoldFor(biome) : 0;
            EventBus.Publish(new ArenaSecretBrokenEvent(relicId, gold));
            return relicId;
        }

        /// <summary>Hands over (and clears) the queued map gold.</summary>
        public int TakePendingGold()
        {
            int gold = PendingGold;
            PendingGold = 0;
            return gold;
        }

        /// <summary>Puts back gold that did not fit anywhere (no bag yet).</summary>
        public void ReturnPendingGold(int gold)
        {
            PendingGold += Math.Max(0, gold);
        }

        private void RecordAchievement(string relicId)
        {
            AchievementManager achievements = Achievements;
            if (achievements == null)
            {
                ServiceLocator.TryGet(out achievements);
            }

            achievements?.RecordProgress(AchievementConditionType.RelicsCollected, 1, relicId, distinct: true);
        }

        /// <summary>Achievements are per profile: a save's relics are re-fed after each load (distinct ids, so no double counting).</summary>
        private void OnGameLoaded(GameLoadedEvent _)
        {
            foreach (string relicId in _relics)
            {
                RecordAchievement(relicId);
            }
        }

        // ------------------------------------------------------------------
        // Save / reset
        // ------------------------------------------------------------------

        public void ResetToNewGame()
        {
            _revealed.Clear();
            _claimed.Clear();
            _relics.Clear();
            PendingGold = 0;
            ArenaSecretsBroken = 0;
            CancelSecretEncounter();
        }

        void IResettable.ResetForNewSession() => ResetToNewGame();

        public object CaptureState()
        {
            return new SecretsSaveState
            {
                revealedSecrets = new List<string>(_revealed),
                claimedSecrets = new List<string>(_claimed),
                relics = new List<string>(_relics),
                pendingGold = PendingGold,
                arenaSecretsBroken = ArenaSecretsBroken,
            };
        }

        public void RestoreState(object state)
        {
            if (state is not SecretsSaveState saved)
            {
                return;
            }

            ResetToNewGame();

            // Unknown ids (a secret or relic removed in a later version) are dropped; a claimed secret is revealed too.
            if (saved.revealedSecrets != null)
            {
                foreach (string id in saved.revealedSecrets)
                {
                    if (WorldSecretCatalog.Get(id) != null)
                    {
                        _revealed.Add(id);
                    }
                }
            }

            if (saved.claimedSecrets != null)
            {
                foreach (string id in saved.claimedSecrets)
                {
                    if (WorldSecretCatalog.Get(id) != null)
                    {
                        _revealed.Add(id);
                        _claimed.Add(id);
                    }
                }
            }

            if (saved.relics != null)
            {
                foreach (string id in saved.relics)
                {
                    if (RelicCatalog.Get(id) != null && !_relics.Contains(id))
                    {
                        _relics.Add(id);
                    }
                }
            }

            PendingGold = Math.Max(0, saved.pendingGold);
            ArenaSecretsBroken = Math.Max(0, saved.arenaSecretsBroken);
        }
    }
}
